using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    
    public static ColorManager instance;
    public Color[] colors;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    public void GetRandomColor(SpriteRenderer sprite)
    {
        int cr = Random.Range(0, colors.Length);
        sprite.color = colors[cr];
        //colors.a = 1f;
    }

    public void TargetColor(SpriteRenderer Sourcecolor, SpriteRenderer Targetcolor)
    {
        Targetcolor.color = Sourcecolor.color;

    }



























}
