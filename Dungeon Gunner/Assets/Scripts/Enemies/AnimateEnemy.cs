using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class AnimateEnemy : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        enemy.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;

        enemy.idleEvent.OnIdle += IdleEvent_OnIdle;
    }

    private void OnDisable()
    {
        enemy.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;

        enemy.idleEvent.OnIdle -= IdleEvent_OnIdle;
    }

    /// <summary>
    /// On movement event handler
    /// </summary>
    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        if (enemy.transform.position.x < GameManager.Instance.GetPlayer().transform.position.x)
        {
            SetAimWeaponAnimationParameters(AimDirection.Right);
        }
        else
        {
            SetAimWeaponAnimationParameters(AimDirection.Left);
        }

        SetMovementAnimationParameters();

    }

    /// <summary>
    /// On Idle event handler
    /// </summary>
    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        SetIdleAniamtionParameters();
    }

    /// <summary>
    /// Initialise anim animation parameters
    /// </summary>
    private void InitialiseAimAnimationParameters()
    {
        enemy.animator.SetBool(Settings.aimUp, false);
        enemy.animator.SetBool(Settings.aimUpRight, false);
        enemy.animator.SetBool(Settings.aimUpLeft, false);
        enemy.animator.SetBool(Settings.aimRight, false);
        enemy.animator.SetBool(Settings.aimLeft, false);
        enemy.animator.SetBool(Settings.aimDown, false);
    }

    /// <summary>
    /// Set movement animation parameters
    /// </summary>
    private void SetMovementAnimationParameters()
    {
        // Set Moving
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isMoving, true);
    }


    /// <summary>
    /// Set idle animation parameters
    /// </summary>
    private void SetIdleAniamtionParameters()
    {
        // Set idle
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, true);
    }

    /// <summary>
    /// Set aim animation parameters
    /// </summary>
    private void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        InitialiseAimAnimationParameters();

        // Set aim direction
        switch (aimDirection)
        {
            case AimDirection.Up:
                enemy.animator.SetBool(Settings.aimUp, true);
                break;
            case AimDirection.UpRight:
                enemy.animator.SetBool(Settings.aimUpRight, true);
                break;
            case AimDirection.UpLeft:
                enemy.animator.SetBool(Settings.aimUpLeft, true);
                break;
            case AimDirection.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                break;
            case AimDirection.Left:
                enemy.animator.SetBool(Settings.aimLeft, true);
                break;
            case AimDirection.Down:
                enemy.animator.SetBool(Settings.aimDown, true);
                break;
        }

    }

}
