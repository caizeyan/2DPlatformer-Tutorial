using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : RaycastController
{

    //斜坡最大可攀爬角度
    public float maxClambAngle = 80;
    //最大下滑高度
    public float maxDescendAngle = 60;
    
    // Start is called before the first frame update
    public override void  Start()
    {
        base.Start();
    }
    
   
    public void Move(Vector3 offset)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.startOffset = offset;
        if (offset.y < 0)
        {
            DescendSlope(ref offset);

        }
        if (offset.x != 0)
        {
            HorizontalCollision(ref offset);

        }
        if (offset.y != 0)
        {
            VerticalCollision(ref offset);
        }
        transform.Translate(offset);
    }
    
    
    //下坡
    private void DescendSlope(ref Vector3 offset)
    {
        float directionX = Mathf.Sign(offset.x);
        float moveDistance = Mathf.Abs(offset.x);
        Vector2 rayOrigin = (directionX == 1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
        //假设是在坡上可以到的最大高度
        float maxHitDistance =Mathf.Abs(offset.y - moveDistance * Mathf.Sin(maxDescendAngle * Mathf.Deg2Rad))+skinWidth;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, maxHitDistance, collisionMask);
        //通过最大角度算出射线最长距离
        Debug.DrawRay(rayOrigin, Vector2.down * (maxHitDistance) ,Color.red);

        if (hit)
        {
            float angle = Vector2.Angle(hit.normal, Vector2.up);
            //判断是否为同一侧
            if (angle!= 0 && angle<=maxDescendAngle && Mathf.Sign(hit.normal.x) == directionX)
            {
                float distanceY = moveDistance * Mathf.Sin(angle * Mathf.Deg2Rad);
                //判断当前高度是否可以到达地面
                if ( Mathf.Abs(offset.y - distanceY) +skinWidth <= hit.distance)
                {
                    return;
                }
                offset.y -= distanceY;
                offset.x = moveDistance * Mathf.Cos(angle * Mathf.Deg2Rad)*directionX;

                collisions.isDecenting = true;
                collisions.blow = true;
                collisions.slopeAngle = angle;
            }
        }
       
    }
    
    
    //水平方向碰撞  即左右
    private void HorizontalCollision(ref Vector3 offset)
    {
        float directionX = Mathf.Sign(offset.x);
        float rayLength = Mathf.Abs(offset.x) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += (verticalRaySpace * i ) * Vector2.up;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            //检测是否有障碍物
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength,Color.red);

            if (hit)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                //如果平行的话 不会被hit打中 也就不存在angle<0的情况
                if (i == 0 && angle <= maxClambAngle)
                {
                    //同时在下坡又在上坡时 当为上坡  如左右两边都有坡，玩家从右到左中间部分走时会存在上坡和下坡同时存在的情况
                    if (collisions.isDecenting)
                    {
                        collisions.isDecenting = false;
                        offset = collisions.startOffset;
                    }
                    //去掉没到斜坡上的距离
                    float distanceX = 0;
                    if (Mathf.Abs(angle - collisions.oldSlampeAngle) > .001f)
                    {
                        distanceX = (hit.distance - skinWidth) * directionX;
                    }

                    offset.x -= distanceX;
                    ClimbSlope(ref offset, angle);
                    offset.x += distanceX;

                }

                //判断在空中 或在攀爬中遇到障碍物
                if (!collisions.isClambing || angle > maxClambAngle)
                {
                    offset.x = (hit.distance-skinWidth)*directionX;
                    rayLength = hit.distance;
                    //在攀爬中遇到水平方向有障碍物 y值斜坡为最大值
                    if (collisions.isClambing)
                    {
                        offset.y = Mathf.Tan(collisions.slopeAngle * Mathf.Rad2Deg) * Mathf.Abs( offset.x);
                    }
                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;

                }
              
            }
        }
    }

    private void ClimbSlope(ref Vector3 offset, float angle)
    {
        float distance = Mathf.Abs( offset.x);
        float climbOffsetY = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
        //判断是否在空中
        if (climbOffsetY >= offset.y)
        {
            offset.y = climbOffsetY;
            offset.x = distance*Mathf.Cos(angle * Mathf.Deg2Rad)*Mathf.Sign(offset.x);
            collisions.isClambing = true;
            collisions.slopeAngle = angle;
            collisions.blow = true;
        }
    }

    //竖直方向 即上下
    private void VerticalCollision(ref Vector3 offset)
    {
        float directionY = Mathf.Sign(offset.y);
        float rayLength = Mathf.Abs(offset.y) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += (horizontalRaySpace * i + offset.x) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin,(Vector2.up*(rayLength*directionY)),Color.red);
            if (hit)
            {
                offset.y = (hit.distance-skinWidth)*directionY;
                if (collisions.isClambing)
                {
                    offset.x = offset.y / (Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad)) * Mathf.Sign(offset.x);
                }
               
                collisions.blow = directionY == -1;
                collisions.up = directionY == 1;
                rayLength = hit.distance;
            }
        }
        
        
        //判断位移后 x是否陷入
        if (collisions.isClambing)
        {
            float directionX = Mathf.Sign(offset.x);
            rayLength = Mathf.Abs(offset.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * offset.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);

            if (hit) {
                float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
                if (slopeAngle != collisions.slopeAngle) {
                    offset.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }
    

   
}
