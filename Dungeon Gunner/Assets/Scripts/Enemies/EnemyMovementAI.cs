using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion
    [SerializeField] private MovementDetailsSO movementDetails;
    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    [HideInInspector] public float moveSpeed;
    private bool chasePlayer = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        // Create waitforfixed update for use in coroutine
        // waitForFixedUpdate 를 coroutine 으로 사용하기 위해 생성
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Reset player reference position
        // 플레이어 참조 위치 리셋
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
    }

    private void Update()
    {
        MoveEnemy();
    }

    /// <summary>
    /// Use AStar pathfinding to build a path to the player - and then move the enemy to each grid location on the path
    /// </summary>
    private void MoveEnemy()
    {
        // Movement cooldown timer
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        // Check distance to player to see if enemy should start chasing
        // 적이 플레이어를 인식하는 범위 내에 있고 현재 쫓는 중이 아니라면
        if (!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        // if not close enough to chase player then return
        // 인식범위 내에 없으면
        if (!chasePlayer)
            return;

        // if the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
        // 움직임 쿨타임이 다 되었거나 플레이어가 적이 원래 도달해야하는 거리 보다 더 멀어졌다면 -- 적을 움직인다
        if (currentEnemyPathRebuildCooldown <= 0f || 
            (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) > Settings.playerMoveDistanceToRebuildPath))
        {
            // Reset path rebuild cooldown timer
            // 경로 재구축 쿨타임 리셋
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            // Reset player reference position
            // 플레이어 참조 위치 리셋
            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            // Move the enemy using AStar pathfinding - Trigger rebuild of path to player
            // 적을 A* 경로 찾기에 따라 움직인다 - 플레이어를 향한 경로 재구축 실행
            CreatePath();

            // If a path has been found move the enemy
            // 적에게 경로가 존재한다면 -- 움직이고 있다면
            if (movementSteps != null)
            {
                if (moveEnemyRoutine != null)
                {
                    // Trigger idle event
                    // ( 새로운 )다른 이동 routine 실행전 idle 상태로 들어가면서 coroutine 멈춤
                    enemy.idleEvent.CallIdleEvent();
                    StopCoroutine(moveEnemyRoutine);
                }

                // Move enemy along the path using a coroutine
                // coroutine 을 사용하여 경로를 따라 적 이동
                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));

            }

        }
    }

    /// <summary>
    /// Coroutine to move the enemy to the next location on the path
    /// 다음 위치에 대한 경로로 적을 움직이게 하기 위한 Coroutine
    /// </summary>
    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        // 이동단계가 없어질때 까지 -- 다 이동할때 까지 or 중간에 멈추고 다른 이동 다시 할때까지
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            // while not very close continue to move - when close move onto the next step
            // 그리 가깝지 않은 상태에서 계속 이동 - 가까우면 다음 단계로 이동
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                // Trigger movement event
                // 움직임 이벤트 실행                                      이동위치         현재위치        이동속도    이동방향 - 이동위치에서 현재위치를 빼고 정규화하면 정규화된 방향벡터 생성 
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed, (nextPosition - transform.position).normalized);

                // moving the enemy using the 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

            }

            yield return waitForFixedUpdate;

        }

        // End of path steps - trigger the enemy idle event
        // 경로 설정 마지막 - 적 idle event 실행
        enemy.idleEvent.CallIdleEvent();

    }

    /// <summary>
    /// Use the AStar static class to create a path for the enemy
    /// </summary>
    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiatedRoom.grid;

        // Get players position on the grid
        // 그리드 상의 플레이어 위치
        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom);

        // Get enemy position on the grid
        // 그리드 상의 적 위치
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        // Build a path for the enemy to move on
        // 적이 이동할 경로 구축 
        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        // Take off first step on path - this is the grid square the enemy is already on
        // 경로의 첫 번째 단계 전진 - 이동하므로 스택에서 빼버린다
        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
        else
        {
            // Trigger idle event - no path
            enemy.idleEvent.CallIdleEvent();
        }

    }

    /// <summary>
    /// Get the nearest position to the player that isn't on an obstacle
    /// 플레이어에게 가장 가까운 위치 중 장애물이 없는 위치 찾기
    /// </summary>
    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(playerPosition);

        // 모든 그리드를 원점이 0,0 안 그리드를 기반으로 하므로 템플릿 lowerbounds 를 조정해야함
        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x, 
            playerCellPosition.y - currentRoom.templateLowerBounds.y);

        int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        // if the player isn't on a cell square marked as an obstacle then return that position
        // 플레이어가 장애물로 표시된 셀 사각형에 있지 않으면 해당 위치를 반환
        if (obstacle != 0)
        {
            return playerCellPosition;
        }
        // find a surrounding cell that isn't an obstacle - required because with the 'half collision' tiles the player can be on a grid square that is marked as an obstacle
        // 그렇지 않다면 -- 장애물이 아닌 주변 셀 찾기 - 'half collision' 타일을 사용하면 플레이어가 장애물로 표시된 grid 사각형에 있을 수 있기 때문
        else
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) continue;

                    // 범위를 벗어날 경우
                    try
                    {
                        obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + i, adjustedPlayerCellPosition.y + j];
                        if (obstacle != 0)
                            return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j);
                    }
                    catch 
                    {
                        continue;
                    }
                }
            }

            // No non-obstacle cells surrounding the player so just return the player position
            // 플레이어 주변에 장애물이 아닌 셀만 있다면 플레이어 위치 반환
            return playerCellPosition;
        }

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
