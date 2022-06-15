using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region DUNGEON BUILD SETTINGS
    // 알고리즘이 던전 빌드를 시도하는 횟수를 제어함 == 무한루프 방지
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion


    #region ROOM SETTINGS

    // 하나의 방에서 나갈 수 있는 최대 자식(Child) 방의 개수를 지정
    // 방이 서로 맞지 않을 가능성이 높기 때문에 던전 빌딩을 붕괴시킬 수 있으므로 권장되지 않지만 최대값은 3 이어야 한다
    public const int maxChildCorridors = 3;

    #endregion
}
