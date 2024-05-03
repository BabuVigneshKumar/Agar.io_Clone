using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public static GameHUD instance;

    public GameObject Player;
    public TMP_Text ScoreTxt;

   
    private void Awake()
    {
        instance = this;
    }
    

  
  
}
