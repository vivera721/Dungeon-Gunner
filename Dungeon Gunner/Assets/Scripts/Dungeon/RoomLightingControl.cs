using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(InstantiatedRoom))]
[DisallowMultipleComponent]
public class RoomLightingControl : MonoBehaviour
{
    private InstantiatedRoom instantiatedRoom;

    private void Awake()
    {
        instantiatedRoom = GetComponent<InstantiatedRoom>();
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
        // If this is the room entered and the room isn't already lit, then fade in the room lighting
        // 방에 들어왔고 방이 밝혀지지 않은 상태 라면 -> 방을 밝힌다 
        if (roomChangedEventArgs.room == instantiatedRoom.room && !instantiatedRoom.room.isLit)
        {
            // Fade in room
            FadeInRoomLighting();

            // Fade in the room doors lighting
            FadeInDoors();

            instantiatedRoom.room.isLit = true; // 만들어진 방이 밝혀졌으므로 isLit 바꿈
        }
    }

    /// <summary>
    /// Fade in the room lighting
    /// </summary>
    private void FadeInRoomLighting()
    {
        // Fade in the lighting for the room tilemaps
        StartCoroutine(FadeInRoomLightingRoutine(instantiatedRoom));
    }

    /// <summary>
    /// Fade in the room lighting coroutine
    /// </summary>
    private IEnumerator FadeInRoomLightingRoutine(InstantiatedRoom instantiatedRoom)
    {
        // Create new material to fade in
        Material material = new Material(GameResources.Instance.variableLitShader);

        // 만들어진 방에서 TilemapRenderer 만 필요하고 그중에서 material 를 fade in 하기 위해 만든 material 로 설정
        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = material;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = material;

        for (float i = 0.05f; i <= 1f; i+= Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;
        }

        // Set material back to lit material
        instantiatedRoom.groundTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration1Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.decoration2Tilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.frontTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;
        instantiatedRoom.minimapTilemap.GetComponent<TilemapRenderer>().material = GameResources.Instance.litMaterial;

    }

    /// <summary>
    /// Fade in the doors
    /// </summary>
    private void FadeInDoors()
    {
        // 모든 문을 찾아야 함
        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach (Door door in doorArray)
        {
            DoorLightingControl doorLightingControl = door.GetComponentInChildren<DoorLightingControl>();

            doorLightingControl.FadeInDoor(door);
        }

    }

}
