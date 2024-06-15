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

    private int SpawnCoun;

    [Client]
    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        PoolManager.Instance.gameManager = this;
        map = MapLimits.Instance;
#if UNITY_SERVER
        StartCoroutine(CreateNewMass());
        SpawnCoun = 5;
#endif
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
    public int _pointss = 0;
    public void CoinCollected(string _myCode, string _playerID)
    {
        Debug.Log($"Point_System 4 ******  {_myCode} removed from the list.");

        Debug.Log($"Data Check ==> First Try  {gameState.players.Exists(x => x.playerData.PlayerId == _playerID)}  ==> {_playerID}  ");
        if (gameState.players.Exists(x => x.playerData.PlayerId == _playerID))
        {
            Debug.Log($"Data Check ==> Second Try.");
            _pointss += 1;
            gameState.players.Find(x => x.playerData.PlayerId == _playerID).Points = _pointss;
            GameHUD.instance.ScoreTxt.text = _pointss.ToString();

            Debug.Log($"Data Check ==> Third Try. {_pointss}  Score Text ==> {GameHUD.instance.ScoreTxt.text}");
        }
        gameState.collectableDatas.RemoveAll(x => x.MyCode == _myCode);
        StartCoroutine(CreateNewMass());
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
    

        UpdateUI();

        switch (gameState.CurrentState)
        {
            case 0:
                UIController.instance.GamePlayPanel();
                break;

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

    }

    void RemoveCompletedEvents()
    {
        foreach (string reqID in gameState.completedRequests)
            RemoveServerEvent(reqID);
        
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

            Debug.Log($"<color=green>Update Game State Server To the Json ==> </color>  {gameStateJson != newGameStateJson}  <color=red>^^^ Equal  ==></color>   {gameStateJson == newGameStateJson} ");
            Debug.Log($"New --> {newGameStateJson}");
            Debug.Log($"Old --> {gameStateJson}");


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
    public void Split(string _playerID, GameObject PlayerObj, float halfMass)
    {
        //float halfMass = player.playerManagerobj.rb.mass / 2;

        foreach (var player in roomInfo.players)
        {
            if (player.PlayerId == _playerID)
            {
                if (player.playerManagerobj.playerUI.Count == 1)
                {
                    PlayerUI ui = player.playerManagerobj.playerUI[0];

                    Vector2 _scale = ui.transform.localScale / 2;

                    GameObject _new = Instantiate(player.playerManagerobj.agarPlayerPrefab, ui.transform.position, Quaternion.identity, player.playerManagerobj.transform);

                    _new.GetComponent<PlayerController>().MyPlayerID = _playerID;
                    _new.GetComponent<Rigidbody2D>().mass = halfMass;   

                    NetworkServer.Spawn(_new.gameObject);

                    //foreach (PlayerState ps in gameState.players)
                    //{
                    //    ps.CurrentState = 1;
                    //}


                }
            }
        }
        UpdateGameStateServer();

        Debug.Log($"TEST_SPLIT ******** 3");
    }
    MapLimits map;
    public ColorPick currentColorPicker;
    int ColorCode;
    [Server]
    public IEnumerator CreateNewMass()
    {
        Debug.Log(">>>>>> ServerSide CAlling 2 ");

        List<CollectableData> _newData = new List<CollectableData>();

        yield return new WaitForSeconds(1f);

        for (int i = gameState.collectableDatas.Count; i < SpawnCoun; i++)
        {
            Vector2 _pos = new Vector2(Random.Range(-map.Maplimits.x, map.Maplimits.x), Random.Range(-map.Maplimits.y, map.Maplimits.y)) / 2;
            Color col = RandomColorGeneration();

            CollectableData _data = new();
            _data.MyPosition = _pos;
            _data.MyColor = col;
            _data.MyCode = $"Code_{ColorCode}";

            gameState.collectableDatas.Add(_data);
            _newData.Add(_data);
            ColorCode += 1;
        }


        Debug.Log(">>>>>> RPC Positions  Server Side ----> " + _newData.Count);

        UpdateGameStateServer();
        Debug.Log(">>>>>> Update GameState server " + _newData.Count);


    }


     

    #region Color Generator

    public Color RandomColorGeneration()
    {
        int randomPick = Random.Range(0, 5);

        currentColorPicker = (ColorPick)randomPick;

        switch (randomPick)
        {
            case 0:
                if (currentColorPicker == ColorPick.Magenta)
                {
                    return Color.magenta;
                }
                break;
            case 1:
                if (currentColorPicker == ColorPick.Blue)
                {
                    return Color.blue;
                }
                break;
            case 2:
                if (currentColorPicker == ColorPick.Red)
                {
                    return Color.red;
                }
                break;
            case 3:
                if (currentColorPicker == ColorPick.Brown)
                {
                    return new Color(150f / 255f, 75f / 255f, 0f);
                }
                break;
            case 4:
                if (currentColorPicker == ColorPick.Green)
                {
                    return Color.green;
                }
                break;

        }
        return Color.white;

    }
    #endregion



}
