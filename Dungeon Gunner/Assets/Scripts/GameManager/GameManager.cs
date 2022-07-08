using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletoneMonobehaviour<GameManager>
{
    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS

    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip

    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]
    #endregion Tooltip

    [SerializeField] private int currentDungeonLevelListIndex = 0;

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;

    protected override void Awake()
    {
        base.Awake();

        // Set player details - saved in current player scriptable object from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        // instantiate player
        InstantiatePlayer();

    }

    /// <summary>
    /// Scene 상에 player 생성
    /// </summary>
    private void InstantiatePlayer()
    {
        // instantiate player
        GameObject playerGameobject = Instantiate(playerDetails.playerPrefab);

        // initialize Player
        player = playerGameobject.GetComponent<Player>();

        player.Initialize(playerDetails);

    }


    private void OnEnable()
    {
        // subscribe to room changed event
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        // unsubscribe to room changed event
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    // Start is called before the first frame update
    private void Start()
    {
        previousGameState = GameState.gameStarted;

        gameState = GameState.gameStarted;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleGameState();

        //// 테스트 용 - 나중에 삭제할 것
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    gameState = GameState.gameStarted;
        //}

    }

    /// <summary>
    /// Handle game state
    /// </summary>
    private void HandleGameState()
    {
        // Handle game state
        switch (gameState)
        {
            case GameState.gameStarted:

                // Play first level
                PlayDungeonLevel(currentDungeonLevelListIndex);

                gameState = GameState.playingLevel;

                break;

        }
    }

    /// <summary>
    /// Set the current room the player in
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;

        //// Debug
        // Debug.Log(room.prefab.name.ToString());
    }

    
    private void PlayDungeonLevel(int DungeonLevelListIndex)
    {
        // 게임 시작시 던전을 레벨에 맞게 빌드
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[DungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
            Debug.LogError("지정된 룸 및 노드 그래프에서 던전을 작성할 수 없습니다.");
        }

        // Call static event that room has changed
        // 게임 시작시 방 변경 이벤트 트리거 작동
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // set player roughly mid-room
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
                                                             (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        // get nearest spawn point in room nearest to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

    }

    /// <summary>
    /// Get the player
    /// </summary>
    public Player GetPlayer()
    {
        return player;
    }

    /// <summary>
    /// Get the player minimap icon
    /// </summary>
    public Sprite GetPlayerMiniMapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }

    /// <summary>
    /// Get the current room the player is in
    /// </summary>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Get the current dungeon level
    /// </summary>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }


    #region Validation

#if UNITY_EDITOR

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }


#endif

    #endregion


}
