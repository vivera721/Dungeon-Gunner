using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class StaticEventHandler
{
    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        // ? 연산자는 null 이 아닐경우 에만 실행
        // invoke 는 호출하는 역할
        OnRoomChanged?.Invoke(new RoomChangedEventArgs() { room = room });
    }

}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}
