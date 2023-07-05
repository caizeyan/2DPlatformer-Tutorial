using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    public struct RaycastOrigins
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
        public float slopeAngle, oldSlampeAngle;

        //是否在下坡
        public bool isDecenting;
        public Vector3 startOffset;
        
        public void Reset()
        {
            left = right = false;
            blow = up = false;
            isClambing = false;
            oldSlampeAngle = slopeAngle;
            slopeAngle = 0;
        }
    }

    
        
    public LayerMask collisionMask;
    
    protected const float skinWidth = .015f;
    
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    protected float horizontalRaySpace = 0; 
    protected float verticalRaySpace = 0;
    
        
    protected RaycastOrigins raycastOrigins;
    private new BoxCollider2D collider;
    public CollisionInfo collisions;
    
    // Start is called before the first frame update
    public virtual void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

     
    //更新顶点
    protected  void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth*-2);
        
        raycastOrigins.bottomLeft = bounds.min;
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = bounds.max;
    }

    //计算间隔
    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth*-2);
        
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, Int32.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, Int32.MaxValue);

        horizontalRaySpace = bounds.size.x / (horizontalRayCount - 1);
        verticalRaySpace = bounds.size.y / (verticalRayCount - 1);
    }


    
}
