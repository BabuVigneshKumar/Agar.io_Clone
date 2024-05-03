using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

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
    private void Start()
    {

       
    }


    public void OnGameStateChanged(string _oldgameData, string _newGameData)
    {
        gameState = JsonUtility.FromJson<GameState>(_newGameData);

        if (isClient)
        {
            UpdateNetworkGame();
        }

    }
    
    public void JoinedWaitingPlayersList(PlayerState _playerState)
    {
        gameState.WaitingPlayers.Add(_playerState);
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

    public void UpdateUI()
    {
        //PlayerUI playerUI = new PlayerUI();


        //Debug.Log($"PlayerUI ==>  Id {playerUI.playerID}");


        //PlayerState tempState = null;
        ////Debug.Log($"TempsTate ==>  Id {tempState?.playerData.PlayerId} && NAme {tempState?.playerData.PlayerName}");

        //for (int i = 0; i < gameState.players.Count; i++)
        //{
        //    Debug.Log($"Condition Check  Waiting -->>  {gameState.players.Count}");

        //    Debug.Log($"Condition Check  Waiting -->>  {gameState.players[i].playerData.PlayerId == playerUI.playerID}");

        //    if (gameState.players[i].playerData.PlayerId == playerUI.playerID)
        //    {
        //        tempState = gameState.players[i];
        //        break;
        //    }
        //}

        //if (tempState == null)
        //{

        //    for (int i = 0; i < gameState.WaitingPlayers.Count; i++)
        //    {
        //        Debug.Log($"Condition Check Real Player -->>  {gameState.WaitingPlayers.Count}");

        //        Debug.Log($"Condition Check  Real Player -->>  {gameState.WaitingPlayers[i].playerData.PlayerId == playerUI.playerID}");

        //        if (gameState.WaitingPlayers[i].playerData.PlayerId == playerUI.playerID)
        //        {
        //            tempState = gameState.WaitingPlayers[i];
        //            break;
        //        }
        //    }
        //}

        //if (tempState != null)
        //{
        //    //    Debug.Log($"TempsTate 1 ==> {tempState}");
        //    //playerUI.UpdateUI(tempState);

        foreach (var item in playerManagerList)
        {
            foreach (var UI in item.playerUIs)
            {
                UI.InitUI(item.myPlayerState);
            }
        }

        if (PoolManager.Instance != null && PoolManager.Instance.pooledObjects.Count > 0)
        {
            foreach (var item in gameState.collectableDatas)
            {
                Debug.Log($"$$$$$$$$$$$$$$$ {item.MyCode}");

                if (!PoolManager.Instance.pooledObjects.Find(x => x.name == item.MyCode))
                {
                    GameObject m = PoolManager.Instance.GetPooledObject();
                    m.SetActive(true);
                    m.transform.position = item.MyPosition;
                    m.GetComponent<SpriteRenderer>().color = item.MyColor;
                    m.name = item.MyCode;
                }
            }
        }

       // PlayerUI.Instance.InitUI();
    }

    public void SplitPlayer()
    {

    }



    [Client]
    public void UpdateNetworkGame()
    {
        UpdateUI();

        switch (gameState.CurrentState)
        {

            case 0:
                UIController.instance.GamePlayPanel();
              

                break;
            case 1:

                //StopCoroutine(nameof(StartGameTimerClient));
                //StartCoroutine(nameof(StartGameTimerClient));


                break;

            case 2:

                
                break;

        }
    }






    [Server]
    public void CheckActionRequired()
    {

        switch (gameState.CurrentState)
        {
            case 0:
                AddPlayerGame(false);
                break;
            case 1:
                AddPlayerGame(true);
                //StopCoroutine(nameof(StartGameTimerServer));
                //StartCoroutine(nameof(StartGameTimerServer));
                break;

        }
        UpdateGameStateServer();

    }

    public void AddPlayerGame(bool gameStart)
    {

        if (gameState.WaitingPlayers.Count + gameState.players.Count > 1)
        {

            foreach (PlayerState Waitingplayer in gameState.WaitingPlayers)
            {
                gameState.players.Add(Waitingplayer);
            }
            gameState.WaitingPlayers.Clear();
            if (gameState.players.Count > 1)
            {
                if (!gameStart)
                {
                    gameState.CurrentState = 1;
                    gameState.gameStartTime = NetworkTime.time;
                }
            }

        }

    }


    [Server]
    public void UpdateGameStateServer()
    {
        foreach (NewPlayer _mirrorPlayer in roomInfo.players)
        {
            PlayerManager _playerManager = _mirrorPlayer.playerManagerobj;



            if (_playerManager != null)
            {
                if (gameState.WaitingPlayers.Exists(x => x.playerData.PlayerId == _playerManager.PlayerID))
                {
                    _playerManager.myPlayerStateJson = JsonUtility.ToJson(GetPlayerState(_playerManager.PlayerID));

                }
                else if (gameState.players.Exists(x => x.playerData.PlayerId == _playerManager.PlayerID))
                {
                    _playerManager.myPlayerStateJson = JsonUtility.ToJson(GetPlayerState(_playerManager.PlayerID));

                }
            }
            string newGameStateJson = JsonUtility.ToJson(gameState);

            if (gameStateJson != newGameStateJson)
            {
                gameStateJson = newGameStateJson;

                CheckActionRequired();
                UpdateNetworkGame();
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
        foreach (PlayerState Newplayers in gameState.WaitingPlayers)
        {
            if (playerId == Newplayers.playerData.PlayerId)
            {
                playerState = Newplayers;


                return Newplayers;
            }
        }


        return playerState;
    }

    /////////////////// TEST
    public void TestSplit(string _playerID)
    {
        foreach (var player in roomInfo.players)
        {
            if(player.PlayerId == _playerID)
            {
                
                if(player.playerManagerobj.playerUIs.Count == 1)
                {
                
                    NetworkIdentity _id = player.playerManagerobj.GetComponent<NetworkIdentity>();
                    PlayerUI ui = player.playerManagerobj.playerUIs[0];
                    Vector2 _scale = ui.transform.localScale/2;
                  
                    ui.gameObject.transform.localScale = _scale;
                   
                    GameObject _new = Instantiate(player.playerManagerobj.testChild, ui.transform.position, Quaternion.identity, player.playerManagerobj.transform);
                    NetworkIdentity _childId = _new.GetComponent<NetworkIdentity>();

                    Debug.Log($"SPACE __ 0 __ Parent_ID {_id.netId} ___ Child_ID {_childId.netId}");

                    _childId = _id;

                    Debug.Log($"SPACE __ 1 __ Parent_ID {_id.netId} ___ Child_ID {_childId.netId}");


                    _new.GetComponent<PlayerMovement>().MyPlayerID = _playerID;
              
                    player.playerManagerobj.playerUIs.Add(_new.GetComponent<PlayerUI>());
                    _new.transform.localScale = _scale;
                    NetworkServer.Spawn(_new.gameObject,connectionToClient);
                }
            }
        }

        Debug.Log($"TEST_SPLIT ******** 3");
    }
}
