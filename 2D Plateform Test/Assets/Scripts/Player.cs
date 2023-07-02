using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    private Controller2D controller2D;

    public float jumpHight = 4;
    public float timeToJumpApex = .4f;

    private float acclerationTimeAirborn = .2f;
    private float acclerationTimeGrounded = .1f;
    
    public float moveSpeed = 6;
    private float gravity;
    private float jumpVelocity;
    public Vector3 velocity = Vector3.zero;

    private float velocityXSmoothing;
    
    // Start is called before the first frame update
    void Start()
    {
        controller2D = GetComponent<Controller2D>();
        //通过跳跃高度 和顶点时间计算速度
        gravity = -(2 * jumpHight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
      
    }

    // Update is called once per frame
    void Update()
    {
        if (controller2D.collisions.blow || controller2D.collisions.up)
        {
            velocity.y = 0;
        }
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && controller2D.collisions.blow)
        {
            velocity.y = jumpVelocity;
        }

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller2D.collisions.blow)?acclerationTimeGrounded:acclerationTimeAirborn);

        velocity.y += gravity * Time.deltaTime;
        controller2D.Move(velocity*Time.deltaTime);
    }
}
