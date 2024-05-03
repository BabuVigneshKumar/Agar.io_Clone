using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public GameObject LobbyPanel, GamePanel;
    public Button FindMatchBtn;

    public string RandomID;
    public string RandomName;

    public TMP_InputField UserName;

    public PlayerData myPlayerData;
    
    private void Awake()
    {
        instance = this;
    }
   
    public GameObject VirtualCamera;
    public GameObject playerCloneParent;

    public GameObject PoolMangerParent;
    public bool IsFocus;

    private void OnApplicationFocus(bool focus)
    {
        //Debug.Log("CHECK ********* " + focus + $" ****** {PlayerMovement.Instance != null}");
        IsFocus = focus;
        if (focus && PlayerMovement.Instance != null)
        {
            //Debug.Log($"CHECK 1");
            StopCoroutine(PlayerMovement.Instance.MoveTowardsMouseCoroutine());
            StartCoroutine(PlayerMovement.Instance.MoveTowardsMouseCoroutine());
            //Debug.Log($"CHECK 2");
        }
    }
    void Start()
    {
        FindMatchBtn.onClick.AddListener(() => InitializeData());
        Invoke(nameof(CallWithDelay), 3f);
    }

    private void CallWithDelay()
    {
        if(PoolManager.Instance != null)
        {
            //PoolManager.Instance.SpawnAndActivateObjects();
        }
    }

    public void GamePlayPanel()
    {
        GamePanel.gameObject.SetActive(true);
        LobbyPanel.gameObject.SetActive(false);

    }
   

    public void InitializeData()
    {
        RandomID = Random.Range(0, 10000).ToString() + SystemInfo.deviceUniqueIdentifier;

        RandomName = "Player " + Random.Range(0, 100);

        myPlayerData.PlayerId = RandomID;



        RandomName = UserName.text;

        myPlayerData.PlayerName = RandomName;
        Debug.Log("User NAme ==> " + RandomName);



        StartCoroutine(StartGame());
    }

    public IEnumerator StartGame()
    {
        if (!NetworkClient.isConnected)
            NetworkManager.singleton.StartClient();
        while (!NetworkClient.isConnected)
        {
            yield return null;
        }
        if (!PlayerManager.instance)
            NetworkClient.AddPlayer();
        while (!PlayerManager.instance)
        {
            yield return null;
        }
        PlayerManager.instance.UpdatePlayerDetails(JsonUtility.ToJson(myPlayerData));


        PlayerManager.instance.SearchGame();
    }

}
