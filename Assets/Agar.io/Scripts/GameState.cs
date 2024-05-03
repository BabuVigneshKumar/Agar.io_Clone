using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameState
{
    public List<PlayerState> players = new List<PlayerState>();
    public List<PlayerState> WaitingPlayers = new List<PlayerState>();
    public double gameStartTime;
    public List<RoomInfo> rooms = new List<RoomInfo>();
    public int CurrentState;
    public List<CollectableData> collectableDatas = new List<CollectableData>();
}
[System.Serializable]
public class PlayerState
{
    public PlayerData playerData = new PlayerData();
    public Color CurrentColor;
    public int CurrentState = 0;

   

}

[System.Serializable]
public class PlayerData
{
    public string PlayerId, PlayerName;
}
