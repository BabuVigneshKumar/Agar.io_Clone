using Mirror;
using System.Collections.Generic;

[System.Serializable]
public class RoomInfo
{
    public string roomName;
    public bool IsRoomClosed;

    public List<NewPlayer> players = new List<NewPlayer>();


    public int MaxPlayers = 2;

    public NetworkIdentity gameManagerID;

}

[System.Serializable]
public class NewPlayer
{
    public string PlayerId;
    public PlayerManager playerManagerobj;
}

