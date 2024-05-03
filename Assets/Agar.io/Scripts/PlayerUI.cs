using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;


public class PlayerUI : NetworkBehaviour
{
    public static PlayerUI Instance;

    public TMP_Text PlayerName;

    public SpriteRenderer ProfilePic;
    public string playerID;


    [SyncVar(hook = nameof(OnColorStateChanged))] public Color CurrentColor;
    public bool isMine;

    public bool isInit = false;



    private void Awake()
    {
        Instance = this;
    }

    //public void SetPlayerName(string newName)
    //{

    //    PlayerName.text = newName;

    //}

   
    public void OnColorStateChanged(Color oldColor, Color newColor)
    {
        ProfilePic.color = newColor;

    }


    public void InitUI(PlayerState ps)
    {
        if (isInit)
            return;

        playerID = ps.playerData.PlayerId;
        PlayerName.text = ps.playerData.PlayerName;

        isInit = true;

        if (isLocalPlayer)
        {
            (Color col, string val) = PoolManager.Instance.RandomColorGeneration();
            CmdSetProfileColor(col);
        }
        if (playerID == UIController.instance.myPlayerData.PlayerId)
        {
            Debug.Log("Player Id --> " + playerID);
            Debug.Log("Player Id Ui Controller  --> " + UIController.instance.myPlayerData.PlayerId);

            isMine = true;

        }
    }


  

    [Command]
    private void CmdSetProfileColor(Color color)
    {
        CurrentColor = color;
    }

   

}

