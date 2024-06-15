using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    public GameHUD gameHUD;

    public bool IsFocus;

    public string RandomID;
    public string RandomName;

    public PlayerData myPlayerData;
    //public ColorPick currentColorPicker;
    private void Awake()
    {
        instance = this;
    }

    //private void OnApplicationFocus(bool focus)
    //{
       
    //    IsFocus = focus;
    //    if (focus && PlayerController.Instance != null)
    //    {
    //        Debug.Log("<color=yellow>IsFocus  ^^^  </color>" + IsFocus);
    //        //StopCoroutine(PlayerController.Instance.MoveTowardsMouseCoroutine());
    //        //StartCoroutine(PlayerController.Instance.MoveTowardsMouseCoroutine());
    //        Debug.Log("<color=fusia>IsFocus  ^^^ </color>" + IsFocus);

    //    }
    //}
    void Start()
    {

        gameHUD.FindMatchBtn.onClick.AddListener(() => InitializeData());

    }

   

    public void GamePlayPanel()
    {
        gameHUD.GamePanel.gameObject.SetActive(true);
        gameHUD.LobbyPanel.gameObject.SetActive(false);

    }

    public void InitializeData()
    {
        RandomID = Random.Range(0, 10000).ToString() + SystemInfo.deviceUniqueIdentifier;

        RandomName = "Player " + Random.Range(0, 100);

        myPlayerData.PlayerId = RandomID;
        myPlayerData.CurrentColor = RandomColorGeneration();


        RandomName = gameHUD.UserName.text;

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
    public ColorPick currentColorPicker;
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



}
