﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 offset;
        //是否正在平台上
        public bool isStandingOnPlatform;
        //是否在平台前移动
        public bool moveBeforePlatform;

        public PassengerMovement(Transform passenger, Vector3 offset, bool isStandingOnPlatform,
            bool moveBeforePlatform)
        {
            this.transform = passenger;
            this.offset = offset;
            this.isStandingOnPlatform = isStandingOnPlatform;
            this.moveBeforePlatform = moveBeforePlatform;
        }
    }
    
    public LayerMask passengerMask;

    public Vector3 moveSpeed;
    
    private HashSet<Transform> passengers = new HashSet<Transform>();
    List<PassengerMovement> passengerMovements = new List<PassengerMovement>();
    private Dictionary<Transform, Controller2D> controllerDictionary = new Dictionary<Transform, Controller2D>();

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRaycastOrigins();
        Vector3 offset = Time.deltaTime * moveSpeed;
        CalculateMovePassenger(offset);
        MovePassenger(false);
        transform.Translate(offset);
        MovePassenger(true);
    }

    private void MovePassenger(bool beforePlatform)
    {
        foreach (var passenger in passengerMovements)
        {
            Transform trans = passenger.transform;
            if (!controllerDictionary.ContainsKey(trans))
            {
                controllerDictionary.Add(trans,trans.GetComponent<Controller2D>());
            }

            if (passenger.moveBeforePlatform == beforePlatform)
            {
                controllerDictionary[trans].Move(passenger.offset);
            }
        }
    }
    
    //移动乘客
    private void CalculateMovePassenger(Vector3 offset)
    {
        //防止多次对乘客进行操作
        passengers.Clear();
        passengerMovements.Clear();
        float directionX = Mathf.Sign(offset.x);
        float directionY = Mathf.Sign(offset.y);
        
        //玩家站在平台移动方向上
        if (offset.y != 0)
        {
            float rayLength = Mathf.Abs(offset.y) + skinWidth;
            Vector2 rayOrigin = (directionY == 1) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
            
            //判断平台移动方向上是否有玩家
            for (int i = 0; i < horizontalRayCount; i++)
            {
                if (i != 0)
                {
                    rayOrigin += Vector2.right * horizontalRaySpace;
                }

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                if (hit)
                {
                    if (!passengers.Contains(hit.transform))
                    {
                        passengers.Add(hit.transform);
                        float pushY = offset.y - (hit.distance - skinWidth)*directionY;
                        float pushX = (directionY == 1) ? offset.x : 0;
                        passengerMovements.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY,0),directionY == 1,directionY == 1));
                    }
                }
            }
        }

        if (offset.x != 0)
        {
            float rayLength = Mathf.Abs(offset.x)+skinWidth;
            Vector2 rayOrigin = (directionX == 1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            
            //判断平台移动方向上是否有玩家
            for (int i = 0; i < verticalRayCount; i++)
            {
                if (i != 0)
                {
                    rayOrigin += Vector2.up * verticalRaySpace;
                }

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                if (hit)
                {
                    if (!passengers.Contains(hit.transform))
                    {
                        passengers.Add(hit.transform);
                        float pushX = offset.x - (hit.distance - skinWidth)*directionX;
                        float pushY = 0;
                        passengerMovements.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY,0),false,true));
                    }
                }
            }
        }
        //玩家在平台上，但没在平台移动方向上 
        if (offset.y == -1 || (offset.y == 0&& offset.x != 0))
        {
            float rayLength = 2*skinWidth;
            Vector2 rayOrigin = raycastOrigins.topLeft;
      
            for (int i = 0; i < horizontalRayCount; i++)
            {
                if (i != 0)
                {
                    rayOrigin += Vector2.right * horizontalRaySpace;
                }

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                if (hit)
                {
                    if (!passengers.Contains(hit.transform))
                    {
                        passengers.Add(hit.transform);
                        float pushY = offset.y;
                        float pushX = offset.x;
                        passengerMovements.Add(new PassengerMovement(hit.transform,new Vector3(pushX,pushY,0),true,true));

                    }
                }
            }
        }

    }
}
