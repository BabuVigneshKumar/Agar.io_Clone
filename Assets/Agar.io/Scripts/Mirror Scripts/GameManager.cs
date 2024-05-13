using DG.Tweening.Core.Easing;
using Mirror;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public NetworkMatch networkMatch;
    public GameState gameState;
    public RoomInfo roomInfo;
    public NetworkIdentity gameManagerNetID;

    public List<PlayerManager> playerManagerList = new List<PlayerManager>();

    [SyncVar(hook = nameof(OnGameStateChanged))] public string gameStateJson;

    [Client]
    private void Awake()
    {
        instance = this;

    }
    public void OnGameStateChanged(string _oldgameData, string _newGameData)
    {
        Debug.Log("Game state ==> " + _newGameData);
        gameState = JsonUtility.FromJson<GameState>(_newGameData);

        if (isClient)
        {
            UpdateNetworkGame();
        }

    }
    public void JoinedWaitingPlayersList(PlayerState _playerState)
    {
        gameState.players.Add(_playerState);
    }

    public void AddPlayerManagerList(PlayerManager playerManager)
    {

        if (playerManager.networkMatch.matchId != networkMatch.matchId)
        {
            return;
        }

        if (!playerManagerList.Contains(playerManager))
        {
            Debug.Log("PlayerManager Contains !! " + playerManager);

            playerManagerList.Add(playerManager);

        }
    }

    public void CoinCollected(string _myCode)
    {
        gameState.collectableDatas.RemoveAll(x => x.MyCode == _myCode);
    }


    public void CoinCollected(string _myCode, string _playerID)
    {
        Debug.Log($"Point_System 4 ******  {_myCode} removed from the list.");
        gameState.collectableDatas.RemoveAll(x => x.MyCode == _myCode);

    }


    public void UpdateUI()
    {
        Debug.Log($"UPDATEA_UI _____ SYAY {gameState}");

        foreach (var item in playerManagerList)
        {
            Debug.Log($"Update_UI ______ Id {item.playerName}");

            foreach (var UI in item.playerUI)
            {

                UI.UpdateUI(item.myPlayerState);
            }

            //item.GetComponent<PlayerUI>().UpdateUI(item.myPlayerState);



        }
        if (PoolManager.Instance != null && PoolManager.Instance.pooledObjects.Count > 0)
        {
            foreach (var item in gameState.collectableDatas)
            {
                Debug.Log($"$$$$$$$$$$$$$$$ {item.MyCode}");

                if (!PoolManager.Instance.pooledObjects.Exists(x => x.name == item.MyCode))
                {
                    GameObject m = PoolManager.Instance.GetPooledObject();
                    m.SetActive(true);
                    m.transform.position = item.MyPosition;
                    m.GetComponent<SpriteRenderer>().color = item.MyColor;
                    m.name = item.MyCode;
                }
            }
        }
    }
    [Client]
    public void UpdateNetworkGame()
    {
        Debug.Log("<color=green>Calling UpdateNetworkGAME!!!</color>");
        Debug.Log("Game state ==> 3 Player_Count " + gameState.players.Count);

        UpdateUI();

        switch (gameState.CurrentState)
        {
            case 0:
                UIController.instance.GamePlayPanel();
                break;
                //case 1:
                //    UpdateUI();
                //    break;

        }
    }
    [Server]
    public void CheckActionRequired()
    {
        Debug.Log("<color=blue> Check ACtion Required !!</color>");
        foreach (ServerEvent request in requestList)
        {
            request.eventAction.Invoke();
            gameState.completedRequests.Add(request.GetEventID());
        }
        switch (gameState.CurrentState)
        {
            case 0:
                AddPlayerGame();
                break;


        }
        UpdateGameStateServer();

    }

    [SerializeField]
    List<ServerEvent> requestList = new List<ServerEvent>();
    public void AddServerEvent(ServerEvent newEvent)
    {
        requestList.Add(newEvent);
        CheckActionRequired();
    }

    public void RemoveServerEvent(string eventID)
    {
        requestList.RemoveAll(x => x.GetEventID() == eventID);
        //requestList.Remove(requestList.Find(x => x.GetEventID() == eventID));
    }

    void RemoveCompletedEvents()
    {
        foreach (string reqID in gameState.completedRequests)
            RemoveServerEvent(reqID);
        //gameState.completedRequests.Clear();
    }


    public void AddPlayerGame()
    {
        Debug.Log("<color=blue>  ADD PLAYERS GAME !!! !</color>" + (gameState.players.Count + gameState.players.Count > 1));

        if (gameState.players.Count + gameState.players.Count > 1)
        {

            //if (gameState.players.Count > 1)
            ////{
            //if (!gameStart)
            //{
            //gameState.CurrentState = 1;
            //gameState.gameStartTime = NetworkTime.time;
            //}
            //}
        }

    }
   
    [Server]
    public void UpdateGameStateServer()
    {
        Debug.Log("<color=red>Update Game State Server </color>");
        foreach (NewPlayer _mirrorPlayer in roomInfo.players)
        {
            Debug.Log("<color=red>Foreqch condition </color>");

            PlayerManager _playerManager = _mirrorPlayer.playerManagerobj;

            if (_playerManager != null)
            {
                if (gameState.players.Exists(x => x.playerData.PlayerId == _playerManager.PlayerID))
                {
                    _playerManager.myPlayerStateJson = JsonUtility.ToJson(GetPlayerState(_playerManager.PlayerID));

                }
            }
            string newGameStateJson = JsonUtility.ToJson(gameState);

            Debug.Log($"<color=green>Update Game State Server To the Json ==> </color>  {gameStateJson != newGameStateJson}  <color=red>^^^ Equal  ==></color>   {gameStateJson == newGameStateJson}");


            if (gameStateJson != newGameStateJson)
            {
                gameStateJson = newGameStateJson;
                RemoveCompletedEvents();
                CheckActionRequired();

            }
        }
    }

    PlayerState GetPlayerState(string playerId)
    {
        PlayerState playerState = new PlayerState();


        foreach (PlayerState ps in gameState.players)
        {
            if (playerId == ps.playerData.PlayerId)
            {
                playerState = ps;


                return ps;
            }
        }
        return playerState;
    }
    /////////////////// Split
    public void Split(string _playerID)
    {
        foreach (var player in roomInfo.players)
        {
            if (player.PlayerId == _playerID)
            {
                //if (player.playerManagerobj.playerUIs.Count == 1)
                //{
                //    NetworkIdentity _id = player.playerManagerobj.GetComponent<NetworkIdentity>();
                //    PlayerUI ui = player.playerManagerobj.playerUIs[0];
                //    Vector2 _scale = ui.transform.localScale / 2;
                //    //ui.gameObject.transform.localScale = _scale;

                //    GameObject _new = Instantiate(player.playerManagerobj.testChild, ui.transform.position, Quaternion.identity, player.playerManagerobj.transform);
                //    NetworkIdentity _childId = _new.GetComponent<NetworkIdentity>();
                //    _childId = _id;
                //    _new.GetComponent<PlayerController>().MyPlayerID = _playerID;

                //    //player.playerManagerobj.playerUIs.Add(_new.GetComponent<PlayerUI>());
                //    //_new.transform.localScale = _scale;
                //    NetworkServer.Spawn(_new.gameObject, connectionToClient);
                //    gameState.CurrentState = 0;

                //    foreach (PlayerState ps in gameState.players)
                //    {
                //        ps.CurrentState = 1;
                //    }

                //    //foreach (var playobj in player.playerManagerobj.playerUIs)
                //    //{
                //    //    playobj.transform.localScale = _scale;
                //    //}
                //}
            }
        }
        UpdateGameStateServer();
        //UpdateNetworkGame();

        Debug.Log($"TEST_SPLIT ******** 3");
    }



    public void ThorwMass(string _playerID)
    {
        foreach (var player in roomInfo.players)
        {
            if (player.PlayerId == _playerID)
            {

                //        if (player.playerManagerobj.playerUIs.Count == 1)
                //        {

                //            if (transform.localScale.x <= 1)
                //            {
                //                return;
                //            }

                //            Vector2 direction = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

                //            float an = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + 90f;
                //            transform.rotation = Quaternion.Euler(0, 0, an);


                //            NetworkIdentity _id = player.playerManagerobj.GetComponent<NetworkIdentity>();
                //            PlayerUI ui = player.playerManagerobj.playerUIs[0];

                //            GameObject massObj = Instantiate(GameHUD.instance.Mass, GameHUD.instance.MassPosition.position, Quaternion.identity);

                //            //massObj.GetComponent<MassForce>().ApplyForce = true;

                //            //Adding Mass to the player
                //            //playerMass.AddMass(m);

                //            //loosing Mass when throwing
                //            transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);

                //        }
            }
        }
    }

}
