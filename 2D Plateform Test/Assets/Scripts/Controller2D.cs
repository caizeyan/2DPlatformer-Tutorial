using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    
    public LayerMask collisionMask;
    
    private const float skinWith = .015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    public float horizontalRaySpace = 0;
    public float verticalRaySpace = 0;
    
    private RaycastOrigins raycastOrigins;
    private BoxCollider2D collider;
    
    
    
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
    
    // Update is called once per frame
    void Update()
    {
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();
        if (velocity.y != 0)
        {
            VerticalCollision(ref velocity);
        }

        if (velocity.x != 0)
        {
            HorizontalCollision(ref velocity);
        }
        transform.Translate(velocity);
    }

    //竖直方向 即上下
    private void VerticalCollision(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWith;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += (horizontalRaySpace * i + velocity.x) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin,(Vector2.up*(rayLength*directionY)),Color.red);
            if (hit)
            {
                velocity.y = (hit.distance-skinWith)*directionY;
                rayLength = hit.distance;
            }
        }
    }
    
    //水平方向碰撞  即左右
    private void HorizontalCollision(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWith;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += (verticalRaySpace * i + velocity.y) * Vector2.up;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit)
            {
                velocity.x = (hit.distance-skinWith)*directionX;
                rayLength = hit.distance;
            }
        }
    }
    

   
}
