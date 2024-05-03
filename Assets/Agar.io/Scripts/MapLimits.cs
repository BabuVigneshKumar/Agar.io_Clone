using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLimits : MonoBehaviour
{

    #region Instance
    public static MapLimits Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion


    public Vector2 Maplimits;
    public Color MapColor;


    private void OnDrawGizmos()
    {
        Gizmos.color = MapColor;
        Gizmos.DrawWireCube(transform.position, Maplimits);     
    }



}
