using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class MovementByVelocityEvent : MonoBehaviour
{
    // 액션 델리게이트 키
    public event Action<MovementByVelocityEvent, MovementByVelocityArgs> OnMovementByVelocity;

    // 움직임 호출
    public void CallMovementByVelocityEvent(Vector2 moveDirection, float moveSpeed)
    {
        OnMovementByVelocity?.Invoke(this, new MovementByVelocityArgs() { moveDirection = moveDirection, moveSpeed = moveSpeed });
    }

}


public class MovementByVelocityArgs: EventArgs
{
    public Vector2 moveDirection;
    public float moveSpeed;
}
