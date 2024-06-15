using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class MirrorManager : NetworkBehaviour
{
    public static MirrorManager instance;

    public SyncList<string> RoomIds = new SyncList<string>();
    public SyncList<RoomInfo> roomList = new SyncList<RoomInfo>();
    public GameObject GameManagerPrefab;
    //public GameObject PoolManagerPrefab;
    
    private void Awake()
    {
        instance = this;
    }


    public bool SearchingGame(GameObject _playerGameobject, string _playerID, out string _MatchID, out NetworkIdentity _gameManagerID)
    {
        _MatchID = string.Empty;
        _gameManagerID = GetComponent<NetworkIdentity>();

        for (int i = 0; i < roomList.Count; i++)
        {
            if (!roomList[i].IsRoomClosed)
            {
                if (JoinGame(roomList[i].roomName, _playerID, _playerGameobject, out _gameManagerID))
                {
                    _MatchID = roomList[i].roomName;
                    _gameManagerID = roomList[i].gameManagerID;
                    return true;
                }
                else
                {

                }
            }
        }
        return false;
    }
    public bool HostGame(string _matchId, string _PlayerId, GameObject _playerGameObject, out NetworkIdentity _gameManagerId)
    {

        _gameManagerId = GetComponent<NetworkIdentity>();

        if (!RoomIds.Contains(_matchId))
        {
            GameObject gm = Instantiate(GameManagerPrefab);
            NetworkServer.Spawn(gm);

    
            gm.GetComponent<GameManager>().networkMatch.matchId = _matchId.ToGuID();
            RoomInfo Rooms = new RoomInfo();

            Rooms.roomName = _matchId;
            Rooms.gameManagerID = gm.GetComponent<NetworkIdentity>();
            Debug.Log($"======> GameManagerCheck <====== HostGame **** {Rooms.gameManagerID == null}");
            NewPlayer newPlayer = new NewPlayer();

            newPlayer.PlayerId = _PlayerId;
            newPlayer.playerManagerobj = _playerGameObject.GetComponent<PlayerManager>();
            Rooms.players.Add(newPlayer);

            roomList.Add(Rooms);
            RoomIds.Add(_matchId);
            _gameManagerId = Rooms.gameManagerID;
            gm.GetComponent<GameManager>().roomInfo = Rooms;

            Debug.Log("<color=Green> Game Hosted SuccessFully </color>");

            return true;
        }
        else
        {
            Debug.Log("<color=red> Game Hosted Failure </color>");


            return false;
        }
    }
    public bool JoinGame(string _matchID, string _playerId, GameObject _playerGameObject, out NetworkIdentity _gameManagerId)
    {
        _gameManagerId = GetComponent<NetworkIdentity>();

        if (RoomIds.Contains(_matchID))
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                if (roomList[i].roomName.Contains(_matchID))
                {
                    if (!roomList[i].IsRoomClosed || roomList[i].players.Exists(x => x.PlayerId == _playerId))
                    {
                        NewPlayer newPlayer = new NewPlayer();

                        newPlayer.PlayerId = _playerId;
                        newPlayer.playerManagerobj = _playerGameObject.GetComponent<PlayerManager>();
                        _gameManagerId = roomList[i].gameManagerID;

                        roomList[i].players.Add(newPlayer);
                        if (roomList[i].players.Count == roomList[i].MaxPlayers)
                        {
                            roomList[i].IsRoomClosed = true;
                        }
                        Debug.Log("<color=green> Match  Joined Successfully</color>");

                        break;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        else
        {
            Debug.Log("<color=red>NO Matching MatchID </color>");
            return false;
        }
    }


}
public static class MatchExtension
{
    public static Guid ToGuID(this string Id)
    {
        SHA256 securityhash = SHA256.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(Id);
        byte[] hasbytes = securityhash.ComputeHash(inputBytes);

        byte[] guidBytes = new byte[16];
        Array.Copy(hasbytes, guidBytes, 16);
        return new Guid(guidBytes);
    }
}


