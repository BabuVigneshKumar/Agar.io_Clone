using DG.Tweening.Core.Easing;
using Mirror;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;

    [SyncVar] public PlayerData myPlayerDetails;
    [SyncVar] public string PlayerID;
    [SyncVar] public string playerName;
    [SyncVar] public string RoomName;
    [SyncVar] public PlayerState myPlayerState = new PlayerState();

    [SyncVar(hook = nameof(OnPlayerStateChanged))] public string myPlayerStateJson;


    public List<PlayerUI> playerUI = new List<PlayerUI>();

    public NetworkIdentity gameMangerNetId;
    public NetworkMatch networkMatch;
    public GameManager gameManager;

    public GameObject agarPlayerPrefab;

    public Rigidbody2D rb;

    [Header("___Test___")]
    public bool startTest;

    [Header("PlayerMovements Data")]

    [SyncVar(hook = nameof(OnPositionChanged))] public Vector3 myPosition;
    [SyncVar(hook = nameof(OnVelocityChange))] public Vector3 myVelocity;
    [SyncVar] public Vector3 PreviousPos;

    private void Awake()
    {
        agarPlayerPrefab = GameHUD.instance.Player;

        if (isLocalPlayer)
            instance = this;
        networkMatch = GetComponent<NetworkMatch>();
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (true || transform.localScale.x > 1 && transform.localScale.y > 1)
            {
                CmdServerForSplit(PlayerID);
            }
        }
    }
    public override void OnStartLocalPlayer()
    {
        instance = this;
        UpdatePlayerDetails(JsonUtility.ToJson(UIController.instance.myPlayerData));
    }
    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            instance = this;
        }
        else
        {
            StartCoroutine(WaitingStatusJoinedPlayer(false));
        }
    }


    #region Movement Data Send
    [Command]
    public void SendCoinCollectedData(string _data, double serverTime)
    {
        gameManager.CoinCollected(_data, PlayerID);
        gameManager.UpdateGameStateServer();
    }

    [Command]
    public void CmdSendMovement(Vector3 position, Vector3 velocity, string _playerID, double serverTime)
    {
        myPosition = position;
        PreviousPos = position;
        myVelocity = velocity;
       
    }
    private void OnPositionChanged(Vector3 oldPositions, Vector3 newPosition)
    {
        if (oldPositions != newPosition)
        {
            if (isServer)
            {
                Debug.Log("Client Position " + newPosition);

                transform.position = newPosition;
            }
            else if (isClient && !isLocalPlayer)
            {
                Debug.Log("Client Position " + newPosition);

                transform.position = newPosition;
            }
        }
    }

    private void OnVelocityChange(Vector3 oldVelocity, Vector3 newVelocity)
    {
        if (oldVelocity != newVelocity)
        {
            if (isServer)
            {
                Debug.Log("Client Position " + newVelocity);

                rb.velocity = newVelocity;
            }
            else if (isClient && !isLocalPlayer)
            {
                Debug.Log("Client Position " + newVelocity);

                rb.velocity = newVelocity;
            }
        }
    }

    #endregion


    public void OnDisable()
    {
        Debug.Log("Destroyed Player Manager !!!");
    }

    [Command]
    public void UpdatePlayerDetails(string JsonData)
    {
        PlayerData myPlayerData = JsonUtility.FromJson<PlayerData>(JsonData);

        myPlayerDetails = myPlayerData;

        PlayerID = myPlayerData.PlayerId;
        playerName = myPlayerData.PlayerName;

        PlayerState playerState = new PlayerState();
        playerState.playerData = myPlayerDetails;


        myPlayerStateJson = JsonUtility.ToJson(playerState);

        //if (GetComponentsInChildren<PlayerController>().Length > 0)
        //{
        foreach (PlayerController ball in GetComponentsInChildren<PlayerController>())
        {
            ball.MyPlayerID = myPlayerData.PlayerId;
        }
        //}
    }
    public void OnPlayerStateChanged(string oldStr, string newStr)
    {
        myPlayerState = JsonUtility.FromJson<PlayerState>(newStr);
    }


    #region Split 
    [Command]
    public void CmdServerForSplit(string _playerID)
    {
        gameManager.Split(_playerID);
    }
    #endregion

    #region Room Managers
    public void SearchGame()
    {
        SearchGameServer();
    }
    [Command]
    public void SearchGameServer()
    {
        if (MirrorManager.instance.SearchingGame(this.gameObject, PlayerID, out RoomName, out gameMangerNetId))
        {
            networkMatch.matchId = RoomName.ToGuID();
            gameManager = gameMangerNetId.GetComponent<GameManager>();
            TargetSearchGame(true, RoomName);
        }
        else
        {
            TargetSearchGame(false, RoomName);

        }
    }
    [TargetRpc]
    public void TargetSearchGame(bool IsSuccess, string _matchID)
    {
        RoomName = _matchID;
        if (!IsSuccess)
        {
            HostGame();
        }
        else
        {
            Debug.Log("<color=green> Game Found !!</color>");
            //PlayerPrefs.SetString("LastEnteredRoom", _matchID);

            UIController.instance.GamePlayPanel();
            StartCoroutine(WaitingStatusJoinedPlayer(true));
        }
    }

    public void HostGame()
    {
        int matchID = Random.Range(180, 10000);
        RoomName = "Room " + matchID.ToString();
        HostGameServer(RoomName);
    }
    [Command]
    public void HostGameServer(string _matchId)
    {
        RoomName = _matchId.ToString();
        if (MirrorManager.instance.HostGame(_matchId, PlayerID, this.gameObject, out gameMangerNetId))
        {
            networkMatch.matchId = _matchId.ToGuID();
            gameManager = gameMangerNetId.GetComponent<GameManager>();
            Debug.Log("<color=fusia> Game Host Check to the  Server !!</color>");
            TargetHostGame(true, _matchId);
        }
        else
        {
            Debug.Log("<color=green> Game Found !! </color>");

            TargetHostGame(false, _matchId);
        }

    }
    [TargetRpc]
    public void TargetHostGame(bool IsSuccess, string matchID)
    {
        if (!IsSuccess)
        {
            HostGame();
        }
        else
        {
            RoomName = matchID;

            PlayerPrefs.SetString("LastEnteredRoom", matchID);
            StartCoroutine(WaitingStatusJoinedPlayer(true));

            Debug.Log("<color=green> Game Found !!</color>");
        }
    }
    public void JoinGame(string _matchID)
    {
        CmdJoinGame(_matchID);
    }

    [Command]
    void CmdJoinGame(string _matchID)
    {
        RoomName = _matchID;
        if (MirrorManager.instance.JoinGame(_matchID, PlayerID, gameObject, out gameMangerNetId))
        {
            networkMatch.matchId = _matchID.ToGuID();


            gameManager = gameMangerNetId.GetComponent<GameManager>();
            TargetJoinGame(true, _matchID);
        }
        else
        {
            TargetJoinGame(false, _matchID);
        }
    }

    [TargetRpc]
    void TargetJoinGame(bool success, string _matchID)
    {
        if (success)
        {
            RoomName = _matchID;
            //UIController.instance.ShowHUD();
            PlayerPrefs.SetString("LastEnteredRoom", _matchID);
            StartCoroutine(WaitingStatusJoinedPlayer(true));
        }

    }
    public IEnumerator WaitingStatusJoinedPlayer(bool IsMine)
    {
        while (!gameManager)
        {
            if (GameManager.instance)
            {
                gameManager = GameManager.instance;
            }
            yield return null;
        }

        gameManager.AddPlayerManagerList(this);

        if (IsMine)
            CheckJoinStatus();
        StartCoroutine(WaitingNewPlayerInGameState());

    }




    public void CheckJoinStatus()
    {
        if (!isLocalPlayer) return;


        foreach (PlayerState playerState in gameManager.gameState.players)
        {
            if (playerState.playerData.PlayerId == myPlayerState.playerData.PlayerId)
            {
                myPlayerState = playerState;
                return;
            }
        }


        //Debug.Log($"Color Values ARe ===> {myPlayerState.CurrentColor.GetHashCode()}   Geenerated Color {col.GetHashCode()}  PlayerName ==> {myPlayerState.playerData.PlayerName} Player Id ==> {myPlayerState.playerData.PlayerId} ");
        Debug.Log($"Entered To JOined  List  --->>> Name {myPlayerState.playerData.PlayerName} **** Color {myPlayerState.playerData.CurrentColor.GetHashCode()}");
        CmdAddPlayersToWaitingList(myPlayerDetails.PlayerId, NetworkTime.time, JsonUtility.ToJson(myPlayerState));

    }




    [Command]
    public void CmdAddPlayersToWaitingList(string userID, double serverTime, string newPlayerDetails)
    {
        PlayerState NewplayerState = JsonUtility.FromJson<PlayerState>(newPlayerDetails);
        ServerEvent request = new ServerEvent();
        request.playerID = userID;
        request.reqTime = serverTime;
        request.eventAction = new System.Action(() => gameManager.JoinedWaitingPlayersList(NewplayerState));
        gameManager.AddServerEvent(request);


    }
    public IEnumerator WaitingNewPlayerInGameState()
    {
        bool IsPlayerFound = false;

        while (!IsPlayerFound)
        {
            if (gameManager.gameState.players.Exists(x => x.playerData.PlayerId == PlayerID))
            {
                IsPlayerFound = true;


            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        InitPlayerManager();

    }

    public void InitPlayerManager()
    {
        //foreach (var myUI in playerUIs)
        //{
        //    Debug.Log($" Name  --> {myUI.myPlayerState.playerData.PlayerName} ||  Color --> {myUI.myPlayerState.playerData.CurrentColor.GetHashCode()} || ID  --> {myUI.myPlayerState.playerData.PlayerName} ");

        //    myUI.UpdateUI(myPlayerState);
        //}

    }
    #endregion






}
