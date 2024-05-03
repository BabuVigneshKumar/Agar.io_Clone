using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager instance;

    [SyncVar] public PlayerData myPlayerDetails;
    [SyncVar] public string PlayerID;
    [SyncVar] public string playerName;
    [SyncVar] public string RoomName;
    public PlayerState myPlayerState = new PlayerState();

    [SyncVar(hook = nameof(OnPlayerStateChanged))] public string myPlayerStateJson;

    public List<PlayerUI> playerUIs = new List<PlayerUI>();


    public NetworkIdentity gameMangerNetId;
    public NetworkMatch networkMatch;
    public GameManager gameManager;
    public GameObject testChild;

    private void Awake()
    {
        if (isLocalPlayer)
            instance = this;
        networkMatch = GetComponent<NetworkMatch>();
    }
    public override void OnStartLocalPlayer()
    {
        instance = this;

        UpdatePlayerDetails(JsonUtility.ToJson(UIController.instance.myPlayerData));
    }
    public override void OnStartClient()
    {

        //RpcSpawnBall();
        if (isLocalPlayer)
        {
            instance = this;

        }
        else
        {
            StartCoroutine(WaitingStatusJoinedPlayer(false));
        }

    }



    public void RpcSpawnBall()
    {
        if (!isLocalPlayer) return;



        GameObject BallObj = Instantiate(GameHUD.instance.Player, transform.position, Quaternion.identity);




        if (BallObj != null && this.transform != null)
        {
            BallObj.transform.SetParent(this.transform);
        }



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
        Debug.LogError($"Total Player Count -->  {GetComponentsInChildren<PlayerMovement>().Length} ");
        if (GetComponentsInChildren<PlayerMovement>().Length > 0)
        {
            foreach (PlayerMovement ball in GetComponentsInChildren<PlayerMovement>())
            {
                Debug.LogError($"Total Player Count FOreachh -->  {GetComponentsInChildren<PlayerMovement>().Length} ");

                ball.MyPlayerID = myPlayerData.PlayerId;
            }
        }
    }
    //public void OnPlayerNameChanged(string oldName, string newName)
    //{
    //    Debug.Log($"Player Name Updated OLd Name --> {oldName}  + New Name {newName}");
    //    PlayerUI.Instance.SetPlayerName(newName);
    //}

    public void OnPlayerStateChanged(string oldStr, string newStr)
    {
        myPlayerState = JsonUtility.FromJson<PlayerState>(newStr);

    }
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
            Debug.Log("<color=green> Game Found !!</color>");

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
        foreach (PlayerState playerState in gameManager.gameState.WaitingPlayers)
        {
            if (playerState.playerData.PlayerId == myPlayerState.playerData.PlayerId)
            {
                myPlayerState = playerState;

                return;
            }
        }
        NewJoinedPlayersToWaitingList(JsonUtility.ToJson(myPlayerState));

    }
    [Command]
    public void NewJoinedPlayersToWaitingList(string _playerDetails)
    {
        Debug.Log("Entered To Waiting List ");
        PlayerState NewplayerState = JsonUtility.FromJson<PlayerState>(_playerDetails);
        Debug.Log($"Json Player Details {_playerDetails}");



        gameManager.JoinedWaitingPlayersList(NewplayerState);
        gameManager.CheckActionRequired();


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
            else if (gameManager.gameState.WaitingPlayers.Exists(x => x.playerData.PlayerId == PlayerID))
            {

                IsPlayerFound = true;

            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    [Header("___Test___")]
    public bool startTest;
    private void Update()
    {

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log($"TEST_SPLIT ******** 0");
            if (true || transform.localScale.x > 1 && transform.localScale.y > 1)
            {
                TestInfoToServerForSplit(PlayerID);
                Debug.Log($"TEST_SPLIT ******** 1");
            }
        }

    }


    [Command]
    public void TestInfoToServerForSplit(string _playerID)
    {
        Debug.Log($"TEST_SPLIT ******** 2");
        gameManager.TestSplit(_playerID);
    }


}
