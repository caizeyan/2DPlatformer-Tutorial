using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    private Controller2D controller2D;

    public float moveSpeed = 6;
    private float gravity = -10;
    public Vector3 velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        controller2D = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"),0);
        velocity.x = input.x * (Time.deltaTime * moveSpeed);
        velocity.y += gravity * Time.deltaTime;
        controller2D.Move(velocity);
    }
}
