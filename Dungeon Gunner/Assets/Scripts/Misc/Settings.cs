using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Settings
{
    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion


    #region DUNGEON BUILD SETTINGS
    // 알고리즘이 던전 빌드를 시도하는 횟수를 제어함 == 무한루프 방지
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion


    #region ROOM SETTINGS

    public const float fadeInTime = 0.5f; // 방에서 페이드 인 하는데 걸리는 시간
    // 하나의 방에서 나갈 수 있는 최대 자식(Child) 방의 개수를 지정
    // 방이 서로 맞지 않을 가능성이 높기 때문에 던전 빌딩을 붕괴시킬 수 있으므로 권장되지 않지만 최대값은 3 이어야 한다
    public const int maxChildCorridors = 3;
    public const float doorUnlockDelay = 1f;

    #endregion

    #region ANIMATOR PARAMETERS
    
    // animator parameters - player
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollRight = Animator.StringToHash("rollRight");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int rollDown = Animator.StringToHash("rollDown");
    public static int flipUp = Animator.StringToHash("flipUp");
    public static int flipRight = Animator.StringToHash("flipRight");
    public static int flipLeft = Animator.StringToHash("flipLeft");
    public static int flipDown = Animator.StringToHash("flipDown");

    public static float baseSpeedForPlayerAnimations = 8f;

    // Animator parameters - Enemy
    public static float baseSpeedForEnemyAnimations = 3f;


    // Animator parameters - Door
    public static int open = Animator.StringToHash("open");

    // Animator parameters - Damageable Decoration
    public static int destroy = Animator.StringToHash("destroy");
    public static String stateDestroyed = "Destroyed";

    #endregion


    #region GAMEOBJECT TAGS
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    #endregion


    #region FIRING CONTROL
    // 타겟과의 거리가 useAimAngleDistance 보다 짧다면 Aim Angle 이 사용되고 - 플레이어에서 부터 계산됨
    // 타겟과의 거리가 useAimAngleDistance 보다 멀다면 weapon aim angle 이 사용된다 - 무기 발사 위치에서 부터 계산됨
    public const float useAimAngleDistance = 3.5f;
    #endregion

    #region ASTAR PATHFINDING PARAMETERS
    public const int defaultAStarMovementPenalty = 40;
    public const int preferredPathAStarMovementPenalty = 1;
    public const int targetFrameRateToSpreadPathfindingOver = 60;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPathRebuildCooldown = 2f;
    #endregion

    #region ENEMY PARAMETERS
    public const int defaultEnemyHealth = 20;
    #endregion


    #region UI PARAMETERS
    public const float uiHeartSpacing = 16f;
    public const float uiAmmoIconSpacing = 4f;
    #endregion

    #region CONTACT DAMAGE PARAMETERS
    public const float contactDamageCollisionResetDelay = 0.5f;
    #endregion


}
