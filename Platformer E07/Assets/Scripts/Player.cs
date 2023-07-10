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

	void Update() {
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		
		if (Input.GetKeyDown (KeyCode.Space) && controller.collisions.below) {
			velocity.y = maxJumpVelocity;
		}

		if (Input.GetKeyUp(KeyCode.Space))
		{
			if (velocity.y >= minJumpVelocity)
			{
				velocity.y = minJumpVelocity;
			}
		}

		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
		controller.Move (velocity * Time.deltaTime,input);
		//先移动再判断是否在平台上  防止穿过平台时速度没有平台下降速度快
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y = 0;
		}
	}
}
