using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFllow : MonoBehaviour
{
    
    public Controller2D target;
    public Vector2 focusAreaSize;
    public float verticalOffset = -1; //摄像机偏移 

    public float lookAheadDstX;
    public float lookSmoothTimeX;
    

    private FocusArea focusArea;

    private float currentLookAheadX;
    private float targetLookAheadX;
    private float lookAheadDirX;
    private float smoothLookVelocityX;
    private bool lookAheadStopped;
    
    // Start is called before the first frame update
    void Start()
    {
        focusArea = new FocusArea(target.collider.bounds, focusAreaSize);
    }

    private void LateUpdate()
    {
        focusArea.Update(target.collider.bounds);
        Vector2 focusPos = focusArea.center + Vector2.up * verticalOffset;
        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Mathf.Sign(target.collisions.playerInput.x) == lookAheadDirX && target.collisions.playerInput.x!=0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDstX;
            }
            else    //玩家停止输入时 但player仍在移动时 防止镜头移动太远
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    //使用原始移动距离的 1/4
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                }
            }
        }
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX,
            lookSmoothTimeX);
        focusPos += Vector2.right * currentLookAheadX;
        transform.position = (Vector3)focusPos + Vector3.back*10;
    }

  

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, .5f);
        Gizmos.DrawCube (focusArea.center, focusAreaSize);

    }
    
    struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;
        private float left, right;
        private float top, bottom;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x/2;
            right = targetBounds.center.x + size.x/2;
            bottom = targetBounds.min.y;
            top = bottom + size.y;
            
            velocity = Vector2.zero;
            center = new Vector2((left + right) / 2, (bottom + top) / 2);
        }

        public void Update(Bounds targetBounds)
        {
            float shiftX = 0;
            if (targetBounds.min.x <left)
            {
                shiftX = targetBounds.min.x - left;
            }else if (targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;
            
            float shiftY = 0;
            if (targetBounds.min.y <bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }
            bottom += shiftY;
            top += shiftY;

            velocity = new Vector2(shiftX, shiftY);
            center += velocity;
        }
        
    }

}
