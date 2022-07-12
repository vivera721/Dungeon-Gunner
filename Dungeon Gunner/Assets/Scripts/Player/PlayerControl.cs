using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip

    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]

    #endregion

    [SerializeField] private MovementDetailsSO movementDetails;

    private Player player;
    private bool leftMouseDownPreviousFrame = false;
    private int currentWeaponIndex = 1;
    private float moveSpeed;
    private Coroutine playerRollCoroutine;
    private WaitForFixedUpdate waitForFixedUpdate;
    private float playerRollCooldownTimer = 0f;
    private bool isPlayerMovementDisabled = false;

    [HideInInspector] public bool isPlayerRolling = false;

    private void Awake()
    {
        player = GetComponent<Player>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        // Create waitforfixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Set Starting Weapon
        SetStartingWeapon();

        // Set Player animation speed
        // 플레이어 애니메이션 속도 설정
        SetPlayerAnimationSpeed();

    }

    /// <summary>
    /// Set the player starting weapon
    /// </summary>
    private void SetStartingWeapon()
    {
        int index = 1;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }
            index++;
        }
    }

    /// <summary>
    /// set player animator speed to match movement speed
    /// </summary>
    private void SetPlayerAnimationSpeed()
    {
        // Set animator speed to match movement speed
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        // if player movement disabled then return
        if (isPlayerMovementDisabled)
            return;

        // if player is rolling then return
        // 플레이어가 Rolling 중이라면 다른 움직임 처리 x -> return
        if (isPlayerRolling) return;

        // Player movement input
        // 플레이어 움직임 입력
        MovementInput();

        // Weapon Input
        // 플레이어 무기 입력
        WeaponInput();

        // Process player use item input
        UseItemInput();

        // Player roll cooldown timer
        PlayerRollCooldownTimer();

    }

    /// <summary>
    /// Player movement input
    /// </summary>
    private void MovementInput()
    {
        // Get movement input
        // 움직임 입력
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
        bool rightMouseButtonDown = Input.GetMouseButtonDown(1); // 오른쪽 마우스 버튼 누름

        // Create a direction vector based on the input
        // 움직임 입력값에 따라서 방향벡터 생성
        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);
        
        // Adjust distance for diagonal movement (pythagoras approximation)
        // 대각선으로 움직일때 거리 고정 - 대각선 거리 1 을 움직일때는 수평 수직 모두 0.7 정도가 되어야함 (피타고라서 정리)
        if (horizontalMovement != 0f && verticalMovement != 0f)
        {
            direction *= 0.7f;
        }

        // if there is movement either move or roll
        // 움직임이나 구르는 입력값이 있다면
        if (direction != Vector2.zero)
        {
            if (!rightMouseButtonDown)
            {
                // trigger movement event
                // 이동 이벤트 트리거 작동시킴
                player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed); 
            }
            // else player roll if not cooling down
            // roll 이 쿨다운중이 아니라면 플레이어는 roll 한다
            else if (playerRollCooldownTimer <= 0f)
            {
                PlayerRoll((Vector3)direction);
            }
        }
        // 움직임 입력값이 없다면 idle 이벤트 트리거 작동
        else
        {
            player.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Player roll
    /// </summary>
    private void PlayerRoll(Vector3 direction)
    {
        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    /// <summary>
    /// Player roll Coroutine
    /// 플레이어 roll 코루틴
    /// </summary>
    private IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        // minDistance used to decide when to exit coroutine loop
        float minDistance = 0.2f;

        isPlayerRolling = true;

        // 플레이어의 현재 위치에 방향벡터와 roll 거리를 곱한 값을 추가
        // 방향벡터는 단위벡터가 되고 roll 거리는 movementDetails 에 지정되어있다
        Vector3 targetPosition = player.transform.position + (Vector3)direction * movementDetails.rollDistance;

        // 플레이어 위치와 목표 위치 사이의 거리가 큰 동안 반복
        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.transform.position, movementDetails.rollSpeed, direction, isPlayerRolling);

            // yield and wait for fixed update
            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        // set cooldown timer
        playerRollCooldownTimer = movementDetails.rollCooldownTime;

        player.transform.position = targetPosition;

    }


    private void PlayerRollCooldownTimer()
    {
        if (playerRollCooldownTimer >= 0f)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Weapon Input
    /// </summary>
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        // Fire weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Switch weapon input
        SwitchWeaponInput();

        // Reload weapon input
        ReloadWeaponInput();
    }


    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        // get mouse world position
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // calculate direction vector of mouse cursor from weapon shoot position
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetShootPosition());

        // calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Set player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        // Trigger weapon aim event
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);

    }


    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        // Fire when left mouse button is clicked
        if (Input.GetMouseButton(0))
        {
            // Trigger fire weapon event
            player.fireWeaponEvent.CallFireWeaponEvent(true,leftMouseDownPreviousFrame ,playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);

            leftMouseDownPreviousFrame = true;
        }
        else
        {
            leftMouseDownPreviousFrame = false;
        }

    }


    private void SwitchWeaponInput()
    {
        // Switch weapon if mouse scroll wheel selected
        if (Input.mouseScrollDelta.y < 0f)
        {
            PreviousWeapon();
        }

        if (Input.mouseScrollDelta.y > 0f)
        {
            NextWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeaponByIndex(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeaponByIndex(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeaponByIndex(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetWeaponByIndex(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetWeaponByIndex(5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetWeaponByIndex(6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetWeaponByIndex(7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SetWeaponByIndex(8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetWeaponByIndex(9);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetWeaponByIndex(10);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetCurrentWeaponToFirstInTheList();
        }

    }

    private void SetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex - 1 < player.weaponList.Count)
        {
            currentWeaponIndex = weaponIndex;
            player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[weaponIndex - 1]);
        }
    }

    private void NextWeapon()
    {
        currentWeaponIndex++;

        if (currentWeaponIndex > player.weaponList.Count)
        {
            currentWeaponIndex = 1;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    private void PreviousWeapon()
    {
        currentWeaponIndex--;

        if (currentWeaponIndex < 1)
        {
            currentWeaponIndex = player.weaponList.Count;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    private void ReloadWeaponInput()
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

        // if current weapon is reloading return
        // 현재 무기가 재장전 중이라면 반환
        if (currentWeapon.isWeaponReloading) 
            return;

        // Remaining ammo is less than clip capacity then return and not infinite ammo then return
        // 남은 탄약이 탄창 용량보다 작고 무한 탄약이 아니라면 반환됩니다.
        if (currentWeapon.weaponRemainingAmmo < currentWeapon.weaponDetails.weaponClipAmmoCapacity && !currentWeapon.weaponDetails.hasInfiniteAmmo)
            return;

        // if ammo in clip equals clip capacity then return
        // 탄창안의 탄약이 탄창 용량과 같다면 (탄창이 가득 차 있다면) 반환
        if (currentWeapon.weaponClipRemainingAmmo == currentWeapon.weaponDetails.weaponClipAmmoCapacity)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Call the reload weapon event
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 0);
        }

    }

    /// <summary>
    /// Use the nearest item within 2 unity units from the player
    /// </summary>
    private void UseItemInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float useItemRadius = 2f;

            // get any useable item near the palyer
            Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(player.GetPlayerPosition(), useItemRadius);

            foreach (Collider2D collider2D in collider2DArray)
            {
                IUseable iUseable = collider2D.GetComponent<IUseable>();

                if (iUseable != null)
                {
                    iUseable.UseItem();
                }

            }

        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if collided with something stop player roll coroutine
        // 플레이어를 멈추게 하는 무언가와 충돌했을때
        StopPlayerRollRoutine();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // if in collision with something stop player roll coroutine
        // 플레이어를 멈추게 하는 무언가와 충돌하고 있을때
        StopPlayerRollRoutine();
    }

    private void StopPlayerRollRoutine()
    {
        if (playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);

            isPlayerRolling = false;
        }
    }

    /// <summary>
    /// Enable the player movement
    /// </summary>
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.idleEvent.CallIdleEvent();
    }

    /// <summary>
    /// Set the current weapon to be first in the player weapon list
    /// </summary>
    private void SetCurrentWeaponToFirstInTheList()
    {
        // Create new temporary list
        List<Weapon> tempWeaponList = new List<Weapon>();

        // Add the current weapon to first in the temp list
        Weapon currentWeapon = player.weaponList[currentWeaponIndex - 1];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        // Loop through existing weapon list and add = skipping current weapon
        int index = 2;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon == currentWeapon)
                continue;

            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        // Assign new list
        player.weaponList = tempWeaponList;

        currentWeaponIndex = 1;

        // Set current weapon
        SetWeaponByIndex(currentWeaponIndex);

    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion


}
