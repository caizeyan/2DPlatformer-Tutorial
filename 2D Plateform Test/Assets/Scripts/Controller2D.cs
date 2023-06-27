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

    public  struct CollisionInfo
    {
        public bool left, right;
        public bool blow, up;

        public void Reset()
        {
            left = right = false;
            blow = up = false;
        }
    }

    
    public LayerMask collisionMask;
    
    private const float skinWith = .015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    public float horizontalRaySpace = 0;
    public float verticalRaySpace = 0;
    
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
    
    // Update is called once per frame
    void Update()
    {
    }

    public void Move(Vector3 offset)
    {
        collision.Reset();
        UpdateRaycastOrigins();
        if (offset.y != 0)
        {
            VerticalCollision(ref offset);
        }

        if (offset.x != 0)
        {
            HorizontalCollision(ref offset);
        }
        transform.Translate(offset);
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
                collision.blow = directionY == -1;
                collision.up = directionY == 1;
                rayLength = hit.distance;
                Debug.LogError(collision.blow);
            }
        }
    }
    
    //水平方向碰撞  即左右
    private void HorizontalCollision(ref Vector3 offset)
    {
        float directionX = Mathf.Sign(offset.x);
        float rayLength = Mathf.Abs(offset.x) + skinWith;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += (verticalRaySpace * i + offset.y) * Vector2.up;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit)
            {
                offset.x = (hit.distance-skinWith)*directionX;
                rayLength = hit.distance;
                collision.left = directionX == -1;
                collision.right = directionX == 1;
            }
        }
    }
    

   
}
