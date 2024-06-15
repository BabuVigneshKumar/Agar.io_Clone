using Mirror;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    public GameObject MassObjs;
    public List<GameObject> pooledObjects = new();
    public int count;

    public List<GameObject> activeObjects = new List<GameObject>();
    public List<GameObject> Players = new List<GameObject>();
    public GameManager gameManager;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        count = 5;
        SpawnAndActivateObjects();
    }

    public void SpawnAndActivateObjects()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject CoinObj = Instantiate(MassObjs);

            CoinObj.transform.SetParent(UIController.instance.gameHUD.PoolMangerParent.transform);

            pooledObjects.Add(CoinObj);

            CoinObj.SetActive(false);
            CoinObj.GetComponent<MassForce>().enabled = true;
        }
    }

    public GameObject GetPooledObject()
    {
        GameObject G = UIController.instance.gameHUD.PoolMangerParent.transform.GetChild(0).gameObject;
        G.transform.SetAsLastSibling();
        return G;

    }
}
public enum ColorPick
{
    Magenta,
    Blue,
    Red,
    Brown,
    Green

}


