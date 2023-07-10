using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	float moveSpeed = 6;

	private float maxWallSlideSpeedMax = 3;

	public Vector2 wallJumpClimb;//上爬
	public Vector2 wallJumpOff; //离开
	public Vector2 wallJumpLeap;//斜跳

	public float wallStickTime = .25f;
	private float timeToWallUnStick = 0;
	
	
	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

	void Start() {
		controller = GetComponent<Controller2D> ();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpHeight = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight); 
		print ("Gravity: " + gravity + "  Jump Velocity: " + maxJumpVelocity);
	}
	
	void Update()
	{
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		
		int wallDirX = controller.collisions.left ? -1 : 1;
		bool wallSliding = false;
		if ( (controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
		{
			wallSliding = true;
			if (velocity.y < -maxWallSlideSpeedMax)
			{
				velocity.y = -maxWallSlideSpeedMax;
			}

			//设置玩家粘连时间 从按右键开始最少粘连wallStickTime
			if (timeToWallUnStick > 0)
			{
				velocityXSmoothing = 0;
				velocity.x = 0;
				if (input.x != wallDirX && input.x != 0)
				{
					timeToWallUnStick -= Time.deltaTime;
				}
				else
				{
					timeToWallUnStick = wallStickTime;
				}
			}
			else
			{
				timeToWallUnStick = wallStickTime;
			}
		}
		

		if (Input.GetKeyDown (KeyCode.Space)) {
			if (wallSliding)
			{
				//注意是input.x
				if (input.x == wallDirX)
				{
					velocity.x = wallJumpClimb.x * -wallDirX;
					velocity.y = wallJumpClimb.y;
				}else if (input.x == 0)
				{
					velocity.x = wallJumpOff.x * -wallDirX;
					velocity.y = wallJumpOff.y;
				}
				else
				{
					velocity.x = wallJumpLeap.x * -wallDirX;
					velocity.y = wallJumpLeap.y;
				}
			}
			if (controller.collisions.below)
			{
				velocity.y = maxJumpVelocity;

			}
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			if (velocity.y >= minJumpVelocity)
			{
				velocity.y = minJumpVelocity;
			}
		}

		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime,input);
		//先移动再判断是否在平台上  防止穿过平台时速度没有平台下降速度快
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
	}
}
