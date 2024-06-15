using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public static GameHUD instance;

    public GameObject Player;
    public GameObject Mass;
    public GameObject LobbyPanel, GamePanel;
    public GameObject VirtualCamera;
    public GameObject playerCloneParent;
    public GameObject PoolMangerParent;
    public GameObject gameManager;

    public TMP_Text ScoreTxt;
    public TMP_InputField UserName;

    public Transform MassPosition;

    public Button FindMatchBtn;

    private void Awake()
    {
        instance = this;
    }
    

  
  
}
