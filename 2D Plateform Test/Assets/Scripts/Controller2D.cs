using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public  struct CollisionInfo
    {
        public bool left, right;
        public bool blow, up;
        //斜坡相关数据
        public bool isClambing;
        public float curClambAngle, oldClambAngle;

        public void Reset()
        {
            left = right = false;
            blow = up = false;
            isClambing = false;
            oldClambAngle = curClambAngle;
            curClambAngle = 0;
        }
    }

    
    public LayerMask collisionMask;
    
    private const float skinWith = .015f;
    
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpace = 0; 
    float verticalRaySpace = 0;

    //斜坡最大可攀爬角度
    public float maxClambAngle = 80;
    
    private RaycastOrigins raycastOrigins;
    private new BoxCollider2D collider;
    public CollisionInfo collision;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }
    
    
    //更新顶点
    void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWith*-2);
        
        raycastOrigins.bottomLeft = bounds.min;
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = bounds.max;
    }

    //计算间隔
    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWith*-2);
        
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, Int32.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, Int32.MaxValue);

        horizontalRaySpace = bounds.size.x / (horizontalRayCount - 1);
        verticalRaySpace = bounds.size.y / (verticalRayCount - 1);
    }
    

    public void Move(Vector3 offset)
    {
        UpdateRaycastOrigins();
        collision.Reset();
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
    
    
    //水平方向碰撞  即左右
    private void HorizontalCollision(ref Vector3 offset)
    {
        float directionX = Mathf.Sign(offset.x);
        float rayLength = Mathf.Abs(offset.x) + skinWith;

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
                    //去掉没到斜坡上的距离
                    float distanceX = 0;
                    if (Mathf.Abs(angle - collision.oldClambAngle) > .001f)
                    {
                        distanceX = (hit.distance - skinWith) * directionX;
                    }

                    offset.x -= distanceX;
                    ClambSlope(ref offset, angle);
                    offset.x += distanceX;

                }

                //判断在空中 或在攀爬中遇到障碍物
                if (!collision.isClambing || angle > maxClambAngle)
                {
                    offset.x = (hit.distance-skinWith)*directionX;
                    rayLength = hit.distance;
                    //在攀爬中遇到水平方向有障碍物 y值斜坡为最大值
                    if (collision.isClambing)
                    {
                        offset.y = Mathf.Tan(collision.curClambAngle * Mathf.Rad2Deg) * Mathf.Abs( offset.x);
                    }
                    collision.left = directionX == -1;
                    collision.right = directionX == 1;

                }
              
            }
        }
    }

    private void ClambSlope(ref Vector3 offset, float angle)
    {
        float distance = Mathf.Abs( offset.x);
        float clambOffsetY = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
        //判断是否在空中
        if (clambOffsetY >= offset.y)
        {
            offset.y = clambOffsetY;
            offset.x = distance*Mathf.Cos(angle * Mathf.Deg2Rad)*Mathf.Sign(offset.x);
            collision.isClambing = true;
            collision.curClambAngle = angle;
            collision.blow = true;
        }
    }

    //竖直方向 即上下
    private void VerticalCollision(ref Vector3 offset)
    {
        float directionY = Mathf.Sign(offset.y);
        float rayLength = Mathf.Abs(offset.y) + skinWith;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += (horizontalRaySpace * i + offset.x) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin,(Vector2.up*(rayLength*directionY)),Color.red);
            if (hit)
            {
                offset.y = (hit.distance-skinWith)*directionY;
                if (collision.isClambing)
                {
                    offset.x = offset.y / (Mathf.Tan(collision.curClambAngle * Mathf.Deg2Rad)) * Mathf.Sign(offset.x);
                }
               
                collision.blow = directionY == -1;
                collision.up = directionY == 1;
                rayLength = hit.distance;
            }
        }
    }
    

   
}
