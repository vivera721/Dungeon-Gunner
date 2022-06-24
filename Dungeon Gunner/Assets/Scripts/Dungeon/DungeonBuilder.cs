using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletoneMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        // Load the room node type list
        LoadRoomNodeTypeList();

        // Set dimmed material to fully visible
        // 셰이더 그래프에서 사용했던 Alpha_Slider 값을 설정
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);

    }

    /// <summary>
    /// Load the room node type list
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Generate random dungeon, returns true if dungeon built, false if failed
    /// </summary>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        // Load scriptable object room templates into the dictionary
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;

            // 임의의 room node graph 선택
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            int dungeonRebuildAttemptsForNodeGraph = 0;
            dungeonBuildSuccessful = false;

            // dungeon이 제대로 빌드될때까지 or 노드 그래프의 최대 시도 횟수를 넘길때 까지 반복
            while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
            {
                // Clear dungeon room gameobjects and dungeon room dictionary
                ClearDungeon();

                dungeonRebuildAttemptsForNodeGraph++;

                // Attempt to Build A Random Dungeon For The Selected room node graph
                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);

            }

            if (dungeonBuildSuccessful)
            {
                // Instantiate Room Gameobjects
                InstantiateRoomGameobjects();
            }

        }

        return dungeonBuildSuccessful;

    }

    /// <summary>
    /// Load the room templates into the dictionary
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        // room template dictionary 지움
        roomTemplateDictionary.Clear();

        // Load room template list into dictionary
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // dictionary 에 template 에 대한 키가 포함되어있지 않은 경우 -> 추가
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                // 키가 있다면 복제본이 있다는 것이니 디버그로 출력
                Debug.Log("Duplicate Room Template Key In " + roomTemplateList);
            }
        }

    }

    /// <summary>
    /// 지정된 룸 노드 그래프에 대한 던전을 임의로 빌드하려고 시도함
    /// 제대로 생성되었다면 true 반환
    /// 다른 시도가 필요하거나 문제를 맞닥뜨렸다면 false 반환
    /// </summary>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {

        // Create Open Room Node Queue
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        // Add Entrance Node To Room Node Queue From Room Node Graph
        //                                                                             x 가 entrance 인 x를 찾는다
        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance Node");
            return false;
        }

        // Start with no room overlaps
        bool noRoomOverlaps = true;

        // Process open room nodes queue
        noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        // 모든 룸 노드가 처리되었고 룸이 겹치지 않은 경우 return true
        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Process rooms in the open room node queue, 중복이 없다면 return true 
    /// </summary>
    private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        // While room nodes in open room node queue & no room overlaps detected
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            // Get next room node from open room node queue
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            // 룸 노드 그래프에서 (this 부모노드와 연결된) 자식 노드를 큐에 넣는다
            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            // if the room is the entrance mark as positioned and add to room dictionary
            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

                room.isPositioned = true;

                // add room to room dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            // 룸 타입이 entrance 가 아닌경우
            else
            {
                // Else get parent room for node
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                // See if room can be placed without overlaps
                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }

        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// Attempt to place the room node in the dungeon 
    /// </summary>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        // initialise and assume overlap until proven otherwise
        bool roomOverlaps = true;

        // 방이 배치될때 겹치지 않을때까지 반복
        while (roomOverlaps)
        {
            // Parent 를 위해 연결되지 않았고 가능한 출입구를 고른다
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                // if no more doorways to try then overlap failure
                return false; // room overlap 겹침
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            // Get a random room template for room node that is consistent with the parent door orientation
            // 상위 도어 방향과 일치하는 룸 노드에 대한 임의 룸 템플릿 가져오기
            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(roomNode, doorwayParent);

            // create a room
            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            // 방 배치
            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                // if room doesn't overlap then set to false to exit while loop
                roomOverlaps = false;

                // mark room as positioned
                room.isPositioned = true;

                // add room to dictionary
                dungeonBuilderRoomDictionary.Add(room.id, room);

            }
            else
            {
                roomOverlaps = true;
            }

        }
        return true; // no room overlaps

    }

    /// <summary>
    /// Get a random room template for room node taking into account the parent door orientation
    /// 상위 도어 방향을 고려하여 룸 노드에 대한 임의의 룸 템플릿을 가져온다
    /// </summary>
    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;

        // 룸 노드가 복도이면 부모 출입구의 방향과 맞는 복도 룸 템플릿을 선택한다
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;

                default:
                    break;
            }
        }
        // 그렇지 않다면 임의의 룸 템플릿 선택
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    /// <summary>
    /// Place the room
    /// </summary>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {

        // get current room doorway position
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        // return if no doorway in room opposite to parent doorway
        if (doorway == null)
        {
            // Mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;

            return false;
        }

        // Calculate 'world' grid parent doorway position
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        // Calculate adjustment position offset based on room doorway position that we are trying to connect
        // e.g. if this doorway is west then we need to add (1,0) to the east parent doorway
        // 연결하려는 객실 출입구 위치를 기준으로 조정 위치 오프셋 계산
        // 예를들어 : 이 출입구가 서쪽이면 (1,0)을 동쪽 부모 출입구에 추가해야 합니다

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;

            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;

            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;

            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;

            case Orientation.none:
                break;

            default:
                break;
        }

        // Calculate room lower bounds and upper bounds absed on positioning to align with parent doorway
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room OverlappingRoom = CheckForRoomOverlap(room);

        if (OverlappingRoom == null)
        {
            // mark doorways as connected & unavailable
            // 출입구가 연결되었고 이제 사용불가능 하다고 마크
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;

            // return true to show rooms have been connected with no overlap
            return true;
        }
        else
        {
            // Mark the parent doorway as unavailable so we don't try and connect it again
            doorwayParent.isUnavailable = true;

            return false;
        }

    }

    /// <summary>
    /// Get the doorway from the doorway list that has the opposite orientation to doorway
    /// </summary>
    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {

        foreach (Doorway doorwayToCheck in doorwayList)
        {
            if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
            {
                return doorwayToCheck;
            }
            else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
            {
                return doorwayToCheck;
            }
        }
        return null;
    }

    /// <summary>
    /// Check for rooms that overlap the upper and lower bounds parameters, and if there are overlapping rooms then return room else return null
    /// </summary>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        // 모든 방 반복
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // skip if same room as room to test or room hasn't been positioned
            // 배치하려고 하는 방이나 배치되지 않은 방에 대해서 스킵
            if (room.id == roomToTest.id || !room.isPositioned)
                continue;

            // 방이 겹친다면
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }

        }

        return null;
    }

    /// <summary>
    /// 2개의 방이 서로 겹친다면 true 반환
    /// 그렇지 않다면 false 반환
    /// </summary>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);

        bool isOverlappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);

        if (isOverlappingX && isOverlappingY)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// 두 방의 간격이 겹치는지 확인
    /// </summary>
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        // 두 최솟값의 최댓값이 두 최댓값의 최솟값보다 작다면 겹친다
        if (Mathf.Max(imax1, imin2) <= Mathf.Min(imax1, imax2))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 룸타입과 맞는 무작위 룸 템플릿을 룸템플릿 리스트에서 가져온다
    /// 맞는 룸 템플릿이 없다면 null 반환
    /// </summary>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        // 룸 템플릿 리스트 루프
        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            // add matching room templates
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        // returning null if list is zero
        if (matchingRoomTemplateList.Count == 0)
            return null;

        // 리스트에서 무작위 룸 템플릿을 선택하고 반환
        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];

    }

    /// <summary>
    /// get unconnected doorway
    /// </summary>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        // doorway 리스트 반복
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
                yield return doorway;
        }

    }

    /// <summary>
    /// Create room based on roomTemplate and layoutNode, and return the created room
    /// </summary>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        // Initialise room from template
        Room room = new Room();
        
        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;
        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        // Set parent ID for room
        if (roomNode.parentRoomNodeIDList.Count == 0) // 입구일때 Entrance
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;

            // set entrance in game manager
            GameManager.Instance.SetCurrentRoom(room);

        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        return room;
    }

    /// <summary>
    /// 룸 노드 그래프 리스트에서 랜덤한 룸 노드 그래프를 선택
    /// </summary>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        // 룸 노드 그래프 리스트의 갯수가 0보다 많다면 (크다면)
        if (roomNodeGraphList.Count > 0)
        {
            // 그 중에 랜덤한 룸 노드 그래프를 반환
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    /// <summary>
    /// doorway 리스트를 새로 만들고 원래 있던것을 복사후 새로만든것을 반환
    /// </summary>
    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in oldDoorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

            newDoorwayList.Add(newDoorway);
        }
        return newDoorwayList;

    }

    /// <summary>
    /// Create deep copy of string list 
    /// 새로 만든 문자열 리스트에 복사형식으로 넣고 새로만든 문자열 반환 (참조x)
    /// </summary>
    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }

        return newStringList;
    }

    /// <summary>
    /// Instantiate the dungeon room gameobjects from the prefabs
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        // 모든 던전룸에 대해 반복
        foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
        {
            Room room = keyvaluepair.Value;

            // Calculate room position (remember the room instantiation position needs to be adjusted by the room template lower bounds)
            // 룸 위치 계산(룸 템플릿 하한을 통해 룸 인스턴스화 위치를 조정해야 함)
            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            // 룸 인스턴스화
            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            // Get instantiated room component from the instantiated prefab
            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            // initialise the instantiated room
            instantiatedRoom.Initialise(roomGameobject);

            // save gameobject reference
            room.instantiatedRoom = instantiatedRoom;

        }
    }

    /// <summary>
    /// Get a room template by room template ID
    /// </summary>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get room by room ID
    /// </summary>
    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Clear dungeon room gameobjects and dungeon manager room dictionary
    /// </summary>
    private void ClearDungeon()
    {
        // 인스턴스화 된 던전 게임오브젝트들과 던전 룸 dictionary 를 제거
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in dungeonBuilderRoomDictionary)
            {
                Room room = keyvaluepair.Value;

                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }

            dungeonBuilderRoomDictionary.Clear();
        }

    }

}
