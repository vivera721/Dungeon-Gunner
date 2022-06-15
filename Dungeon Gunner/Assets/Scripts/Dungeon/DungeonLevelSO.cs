using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("BASIC LEVEL DETAILS")]
    #endregion Header BASIC LEVEL DETAILS

    #region Tooltip
    [Tooltip("The name for the level")]
    #endregion Tooltip

    public string levelName;

    #region Header ROOM TEMPLATES FOR LEVEL
    [Space(10)]
    [Header("ROOM TEMPLATES FOR LEVEL")]
    #endregion Header ROOM TEMPLATES FOR LEVEL

    #region Tooltip
    [Tooltip("Populate the list with the room templates that you want to be part of the level." +
        "You need to ensure that room templates are included for all room node types that are specified in the Room Node Graphs for the level")]
    #endregion Tooltip

    // 룸 템플릿 목록에 대한 멤버 변수
    public List<RoomTemplateSO> roomTemplateList;

    #region Header ROOM NODE GRAPHS FOR LEVEL
    [Space(10)]
    [Header("ROOM NODE GRAPHS FOR LEVEL")]
    #endregion Header ROOM NODE GRAPHS FOR LEVEL

    #region Tooltip
    [Tooltip("Populate this list with the room node graphs which should be randomly selected from for the level.")]
    #endregion Tooltip

    // 룸 노드 그래프 스크립트 목록이 될 멤버 변수
    public List<RoomNodeGraphSO> roomNodeGraphList;

    // 유효성 검사
    // 메모 그래프가 지정되고 다른 메모가 포함되어 있지만 방에 포함되어 있지 않은 경우
    // 내용끼리 일치하는지 확인
    #region Validation
#if UNITY_EDITOR

    // Validate scriptable object details entered
    private void OnValidate()
    {
        // 문자열 비었는지 체크
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomTemplateList), roomTemplateList))
            return;
        if (HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomNodeGraphList), roomNodeGraphList))
            return;

        // Check to make sure that room templates are specified for all the node types in the specified node graphs
        // 지정된 노드 그래프의 모든 노드 유형에 대해 룸 템플릿이 지정되었는지 확인
        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        // Loop through all room templates to check that this node type has been specified
        // 노드 유형이 지정되었는지 확인하기 위해 모든 룸 템플릿을 반복
        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if (roomTemplateSO.roomNodeType.isCorridorEW)
                isEWCorridor = true;

            if (roomTemplateSO.roomNodeType.isCorridorNS)
                isNSCorridor = true;

            if (roomTemplateSO.roomNodeType.isEntrance)
                isEntrance = true;

        }

        if (isEWCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No E/W Corridor Room Type Specified");
        }
        if (isNSCorridor == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No N/S Corridor Room Type Specified");
        }
        if (isEntrance == false)
        {
            Debug.Log("In " + this.name.ToString() + " : No Entrance Room Type Specified");
        }

        // Loop through all node graphs
        // 모든 노드 반복
        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {

            if (roomNodeGraph == null)
                return;

            // Loop through all nodes in node graph
            foreach (RoomNodeSO roomNodeSO in roomNodeGraph.roomNodeList)
            {
                if (roomNodeSO == null)
                    continue;

                // Check that a room template has been specified for each roomNode type

                // Corridors and Entrance already checked
                if (roomNodeSO.roomNodeType.isEntrance || roomNodeSO.roomNodeType.isCorridorEW || roomNodeSO.roomNodeType.isCorridorNS
                    || roomNodeSO.roomNodeType.isCorridor || roomNodeSO.roomNodeType.isNone)
                    continue;

                bool isRoomNodeTypeFound = false;

                // Loop through all room templates to check that this node type has been specified
                // 노드 유형이 지정되었는지 확인하기 위해 모든 룸 템플릿을 반복
                foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
                {
                    if (roomTemplateSO == null)
                        continue;

                    if (roomTemplateSO.roomNodeType == roomNodeSO.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }

                }

                // 방 유형에 대한 방 템플릿 없을시 Log 출력
                if (!isRoomNodeTypeFound)
                    Debug.Log("In " + this.name.ToString() + " : No room template " + roomNodeSO.roomNodeType.name.ToString() +
                        " found for node graph "+ roomNodeGraph.name.ToString());


            }
        }



    }



#endif
    #endregion Validation

}
