using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Objects/Dungeon/Room Node Type")]
public class RoomNodeTypeSO : ScriptableObject
{
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")] // RoomNodeTypes 만 에디터에서 보여져야 한다
    #endregion Header
    public bool displayInNodeGraphEditor = true;
    #region Header
    [Header("One Type Should Be A Corridor")] // 하나는 복도(기본)
    #endregion Header
    public bool isCorridor;
    #region Header
    [Header("One Type Should Be A CorridorNS")] // 하나는 복도 남북방향
    #endregion Header
    public bool isCorridorNS;
    #region Header
    [Header("One Type Should Be A CorridorEW")] // 하나는 복도 동서 방향
    #endregion Header
    public bool isCorridorEW;
    #region Header
    [Header("One Type Should Be A Entrance")] // 하나는 입구
    #endregion Header
    public bool isEntrance;
    #region Header
    [Header("One Type Should Be A Boss Room")] // 하나는 보스 방
    #endregion Header
    public bool isBossRoom;
    #region Header
    [Header("One Type Should Be (Unassigned)")] // 하나는 아직 미정된 구역
    #endregion Header
    public bool isNone;


    #region Validation 
#if UNITY_EDITOR
    private void OnValidate() // 유효성 검사
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion

}
