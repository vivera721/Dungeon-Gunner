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

    [HideInInspector] public GameState gameState;


    // Start is called before the first frame update
    private void Start()
    {
        gameState = GameState.gameStarted;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleGameState();

        // 테스트 용 - 나중에 삭제할 것
        if (Input.GetKeyDown(KeyCode.R))
        {
            gameState = GameState.gameStarted;
        }

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

    
    private void PlayDungeonLevel(int DungeonLevelListIndex)
    {
        // 게임 시작시 던전을 레벨에 맞게 빌드
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[DungeonLevelListIndex]);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
            Debug.LogError("지정된 룸 및 노드 그래프에서 던전을 작성할 수 없습니다.");
        }

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
