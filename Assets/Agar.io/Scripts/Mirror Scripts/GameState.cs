using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameState
{
    public double gameStartTime;
    public int CurrentState;

    public List<PlayerState> players = new List<PlayerState>();

    public List<string> completedRequests = new List<string>();

    public List<RoomInfo> rooms = new List<RoomInfo>();

    public List<PlayerController> myPlayers = new List<PlayerController>(); 

    public List<CollectableData> collectableDatas = new List<CollectableData>();
}
[System.Serializable]
public class PlayerState
{
    public PlayerData playerData = new PlayerData();
    public int CurrentState = 0;
    public int Points;
    public float speed;
    public Vector2 NextScale;

}

[System.Serializable]
public class PlayerData
{
    public string PlayerId, PlayerName;
    public Color CurrentColor;
}
