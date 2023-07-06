using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {

	public LayerMask passengerMask;
	
	//移动目标  相对位置
	public Vector3[] moveTarget;
	public float moveSpeed = 1;
	//判断是为循环
	public bool isCycle = false;
	//平滑系数 越大抖动越快
	[Range(0,2)]
	public float easeAmount = 0;
	//暂停时间
	public float pauseTime = 0;
	
	//绝对位置
	private Vector3[] globalTarget;
	private float moveProgress = 0;
	private int curIndex = 0;
	private float moveTime = 0;
	
	List<PassengerMovement> passengerMovement;
	Dictionary<Transform,Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();
	
	public override void Start () {
		base.Start ();
		globalTarget = new Vector3[moveTarget.Length];
		for (int i = 0; i < globalTarget.Length; i++)
		{
			globalTarget[i] = transform.position + moveTarget[i];
		}
	}

	void Update () {

		UpdateRaycastOrigins ();

		Vector3 velocity = CalculateMove();

		CalculatePassengerMovement(velocity);

		MovePassengers (true);
		transform.Translate (velocity);
		MovePassengers (false);
	}

	private Vector3 CalculateMove()
	{
		if (moveTime >Time.time)
		{
			return Vector3.zero;
		}
		
		int nextIndex = (curIndex + 1) % globalTarget.Length;
		moveProgress += (Time.deltaTime * moveSpeed) /
		                (Vector3.Distance(globalTarget[curIndex], globalTarget[nextIndex]));
		float easeProgress = Ease(moveProgress);
		Vector3 moveOff = Vector3.Lerp(globalTarget[curIndex], globalTarget[nextIndex], easeProgress) -
		                     transform.position;
		
		if (moveProgress >= 1)
		{
			curIndex = (curIndex+1)%globalTarget.Length;
			//非循环的且到最后一个翻转数组
			if (!isCycle && curIndex == globalTarget.Length-1)
			{
				curIndex = 0;
				Array.Reverse(globalTarget);
			}
			moveProgress = 0;
			moveTime = Time.time + pauseTime;
		}
		return moveOff;
	}

	//平滑  从慢到快再到慢  (x^a) / ((x^a)+(1-x)^a)
	private float Ease(float progress)
	{
		progress = Mathf.Clamp01(progress);
		float a = 1 + easeAmount;
		return Mathf.Pow(progress, a) /( Mathf.Pow(progress, a) + Mathf.Pow((1 - progress), a));
	}

	void MovePassengers(bool beforeMovePlatform) {
		foreach (PassengerMovement passenger in passengerMovement) {
			if (!passengerDictionary.ContainsKey(passenger.transform)) {
				passengerDictionary.Add(passenger.transform,passenger.transform.GetComponent<Controller2D>());
			}

			if (passenger.moveBeforePlatform == beforeMovePlatform) {
				passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
			}
		}
	}

	void CalculatePassengerMovement(Vector3 velocity) {
		HashSet<Transform> movedPassengers = new HashSet<Transform> ();
		passengerMovement = new List<PassengerMovement> ();

		float directionX = Mathf.Sign (velocity.x);
		float directionY = Mathf.Sign (velocity.y);

		// Vertically moving platform
		if (velocity.y != 0) {
			float rayLength = Mathf.Abs (velocity.y) + skinWidth;
			
			for (int i = 0; i < verticalRayCount; i ++) {
				Vector2 rayOrigin = (directionY == -1)?raycastOrigins.bottomLeft:raycastOrigins.topLeft;
				rayOrigin += Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

				if (hit) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = (directionY == 1)?velocity.x:0;
						float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), directionY == 1, true));
					}
				}
			}
		}

		// Horizontally moving platform
		if (velocity.x != 0) {
			float rayLength = Mathf.Abs (velocity.x) + skinWidth;
			
			for (int i = 0; i < horizontalRayCount; i ++) {
				Vector2 rayOrigin = (directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight;
				rayOrigin += Vector2.up * (horizontalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

				if (hit) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
						float pushY = -skinWidth;
						
						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), false, true));
					}
				}
			}
		}

		// Passenger on top of a horizontally or downward moving platform
		if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
			float rayLength = skinWidth * 2;
			
			for (int i = 0; i < verticalRayCount; i ++) {
				Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
				RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
				
				if (hit) {
					if (!movedPassengers.Contains(hit.transform)) {
						movedPassengers.Add(hit.transform);
						float pushX = velocity.x;
						float pushY = velocity.y;
						
						passengerMovement.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY), true, false));
					}
				}
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (moveTarget != null)
		{
			Gizmos.color = Color.red;
			for (int i = 0; i < moveTarget.Length; i++)
			{
				Vector3 globalPos = Application.isPlaying ? globalTarget[i] : (moveTarget[i] + transform.position);
				Gizmos.DrawCube(globalPos,Vector3.one/5);
			}
		}
	}	

	struct PassengerMovement {
		public Transform transform;
		public Vector3 velocity;
		public bool standingOnPlatform;
		public bool moveBeforePlatform;

		public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
			transform = _transform;
			velocity = _velocity;
			standingOnPlatform = _standingOnPlatform;
			moveBeforePlatform = _moveBeforePlatform;
		}
	}

}
