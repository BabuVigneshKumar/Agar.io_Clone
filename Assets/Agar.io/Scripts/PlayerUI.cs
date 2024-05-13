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
    public PlayerState myPlayerState;


    public bool isMine;

    public bool isInit = false;

    private void Awake()
    {
        Instance = this;

      
    }

    public void InitUI()
    {
        //Debug.Log($"Init_State ___ 1 ___ Name {myPlayerState.playerData.PlayerName} ___ Col {myPlayerState.CurrentColor.GetHashCode()} ___ init {isInit}");


        if (isInit)
            return;

        playerID = myPlayerState.playerData.PlayerId;
        Debug.Log($"Init_State ___ 0 ___ Name {myPlayerState.playerData.PlayerName} ___ Col {myPlayerState.playerData.CurrentColor.GetHashCode()} ___ init {isInit}");

        ProfilePic.color = myPlayerState.playerData.CurrentColor;
        PlayerName.text = myPlayerState.playerData.PlayerName;

        isInit = true;

        //Debug.Log($"Init_State ___ 2 ___ Name {myPlayerState.playerData.PlayerName} ___ Col {myPlayerState.CurrentColor.GetHashCode()} ___ init {isInit}");
    }

    public void UpdateUI(PlayerState ps)
    {
        //Debug.Log($"PLayerSTate UPdated ---> Name {ps.playerData.PlayerName} _____" + ps.CurrentColor);
        myPlayerState = ps;
        InitUI();
    }



}

