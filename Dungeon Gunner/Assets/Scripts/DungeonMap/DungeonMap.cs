using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMap : SingletoneMonobehaviour<DungeonMap>
{
    #region Header GAMEOBJECT REFERENCE
    [Space(10)]
    [Header("GAMEOBJECT REFERENCE")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the MinimapUI gameobject")]
    #endregion
    [SerializeField] private GameObject minimapUI;
    private Camera dungeonMapCamera;
    private Camera cameraMain;

    private void Start()
    {
        // Cache main camera
        cameraMain = Camera.main;

        // Get player transform
        Transform playerTransform = GameManager.Instance.GetPlayer().transform;

        // Populate player as cinemachine camera target
        CinemachineVirtualCamera cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = playerTransform;

        // get dungeonmap camera
        dungeonMapCamera = GetComponentInChildren<Camera>();
        dungeonMapCamera.gameObject.SetActive(false);

    }

    private void Update()
    {
        // If mouse button pressed and gamestate is overview map then get the room clicked
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.gameState == GameState.dungeonOverviewMap)
        {
            GetRoomClicked();
        }
    }

    /// <summary>
    /// Get the room clicked on the map
    /// </summary>
    private void GetRoomClicked()
    {
        // convert screen position to world position
        Vector3 worldPosition = dungeonMapCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPosition = new Vector3(worldPosition.x, worldPosition.y, 0f);

        // Check for collisions at cursor position
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 1f);

        // Check if any of the colliders are a room
        foreach (Collider2D collider2D in collider2DArray)
        {
            if (collider2D.GetComponent<InstantiatedRoom>() != null)
            {
                InstantiatedRoom instantiatedRoom = collider2D.GetComponent<InstantiatedRoom>();

                // if clicked room is clear of enemies and previously visited then move player to the room
                // 클릭한 방이 적들이 다 제거됬고 방문했던 방이라면 플레이어를 그 방으로 이동시킨다
                if (instantiatedRoom.room.isClearedOfEnemies && instantiatedRoom.room.isPreviouslyVisited)
                {
                    // Move player to room
                    StartCoroutine(MovePlayerToRoom(worldPosition, instantiatedRoom.room));
                }

            }
        }
    }

    /// <summary>
    /// Move the player to the selected room
    /// </summary>
    private IEnumerator MovePlayerToRoom(Vector3 worldPosition, Room room)
    {
        // Call room changed event
        StaticEventHandler.CallRoomChangedEvent(room);

        // Fade out screen to black immediately
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 0f, Color.black));

        // Clear dungeon overview
        ClearDungeonOverViewMap();

        // Disable palyer during the fade
        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        // Get nearest spawn point in room nearest to player
        Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(worldPosition);

        // Move player to new location - spawning them at the closest spawn point
        GameManager.Instance.GetPlayer().transform.position = spawnPosition;

        // Fade the screen back in
        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));

        // Enable player
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();

    }

    /// <summary>
    /// Display dungeon overview map UI
    /// </summary>
    public void DisplayDungeonOverViewMap()
    {
        // Set game state
        GameManager.Instance.previousGameState = GameManager.Instance.gameState;
        GameManager.Instance.gameState = GameState.dungeonOverviewMap;

        // Disable player
        // dungeon overview 할때 플레이어 움직이지 않게 만듦
        GameManager.Instance.GetPlayer().playerControl.DisablePlayer();

        // Disable main camera and enable dungeon overview camera
        // 메인 카메라 비활성화하고 dungeon overview camera 활성화
        cameraMain.gameObject.SetActive(false);
        dungeonMapCamera.gameObject.SetActive(true);

        // Ensure all rooms are active so they can be displayed
        ActivateRoomsForDisplay();

        // Disable small minimap UI
        // 작은 미니맵 UI 비활성화
        minimapUI.SetActive(false);

    }

    /// <summary>
    /// Clear the dungeon overview map UI
    /// </summary>
    public void ClearDungeonOverViewMap()
    {
        // Set game state
        GameManager.Instance.gameState = GameManager.Instance.previousGameState;
        GameManager.Instance.previousGameState = GameState.dungeonOverviewMap;

        // Enable player
        GameManager.Instance.GetPlayer().playerControl.EnablePlayer();

        // Enable main camera and disable dungeon overview camera
        // dungeon overview camera 비활성화하고 메인 카메라 활성화
        cameraMain.gameObject.SetActive(true);
        dungeonMapCamera.gameObject.SetActive(false);

        // Enable small minimap UI
        // 작은 미니맵 UI 활성화
        minimapUI.SetActive(true);
    }

    /// <summary>
    /// Ensure all rooms are active so they can be displayed
    /// 내가 지나간 모든 방을 활성화 시킴으로서 overview map 에 보이게 함
    /// </summary>
    public void ActivateRoomsForDisplay()
    {
        // Iterate through dungeon rooms
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;

            room.instantiatedRoom.gameObject.SetActive(true);
        }
    }

}
