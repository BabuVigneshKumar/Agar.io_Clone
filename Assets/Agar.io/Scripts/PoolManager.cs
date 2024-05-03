using Mirror;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PoolManager : NetworkBehaviour
{
    public static PoolManager Instance;
    public GameObject MassObjs;
    public List<GameObject> pooledObjects = new();
    public int count;
    MapLimits map;
    int ColorCode;
    public ColorPick currentColorPicker;
    public List<GameObject> activeObjects = new List<GameObject>();
    public List<GameObject> Players = new List<GameObject>();
    public GameManager gameManager;

    private void Awake()
    {
        Instance = this;
        map = MapLimits.Instance;
      
    }
    private void Start()
    {
        
        //SpawnAndActivateObjects();
        SpawnObjs();

    }


    #region Server Side

    [Server]
    public void SpawnObjs()
    {
        StartCoroutine(CheckCountMassObj());
    }

    public void SpawnAndActivateObjects()
    {
        for (int i = 0; i < count; i++)
        {
            GameObject CoinObj = Instantiate(MassObjs);

            CoinObj.transform.SetParent(UIController.instance.PoolMangerParent.transform);

            pooledObjects.Add(CoinObj);

            CoinObj.SetActive(false);
            CoinObj.GetComponent<MassForce>().enabled = true;
        }
    }

    public GameObject GetPooledObject()
    {
        GameObject G = UIController.instance.PoolMangerParent.transform.GetChild(0).gameObject;
        G.transform.SetAsLastSibling();
        return G;
      
    }

    public IEnumerator CheckCountMassObj()
    {
        Debug.Log(">>>>>> Server Side CAlling  1");

        int currentCount = gameManager.gameState.collectableDatas.Count;
        Debug.Log("Check mass Count **** " + currentCount);
        if (currentCount < count)
        {
            StartCoroutine(CreateNewMass(currentCount));
        }
        yield return null;
       
    }


    [Server]
    public IEnumerator CreateNewMass(int _count)
    {
        Debug.Log(">>>>>> ServerSide CAlling 2 ");

        List<CollectableData> _newData = new List<CollectableData>();

        yield return new WaitForSeconds(1f);


        for (int i = _count; i < (count - _count); i++)
        {


            Vector2 _pos = new Vector2(Random.Range(-map.Maplimits.x, map.Maplimits.x), Random.Range(-map.Maplimits.y, map.Maplimits.y)) / 2;
            (Color col, string val) = RandomColorGeneration();
            CollectableData _data = new();
            _data.MyPosition = _pos;
            _data.MyColor = col;
            _data.MyCode = $"Code_{ColorCode}";

            gameManager.gameState.collectableDatas.Add(_data);
            _newData.Add(_data);
            ColorCode += 1;
        }

        
        Debug.Log(">>>>>> RPC Positions  Server Side ----> " + _newData.Count);

        gameManager.UpdateGameStateServer();

    }




    #endregion



    #region Color Generator

    public (Color, string) RandomColorGeneration()
    {
        int randomPick = Random.Range(0, 5);

        currentColorPicker = (ColorPick)randomPick;

        switch (randomPick)
        {
            case 0:
                if (currentColorPicker == ColorPick.Magenta)
                {

                    return (Color.magenta, "Magenta");
                }
                break;
            case 1:
                if (currentColorPicker == ColorPick.Blue)
                {


                    return (Color.blue, "Blue");
                }
                break;
            case 2:
                if (currentColorPicker == ColorPick.Red)
                {


                    return (Color.red, "Red");

                }
                break;
            case 3:
                if (currentColorPicker == ColorPick.Brown)
                {

                    return (new Color(150f / 255f, 75f / 255f, 0f), "Brown");
                }
                break;
            case 4:
                if (currentColorPicker == ColorPick.Green)
                {


                    return (Color.green, "Green");

                }
                break;

        }

        return (Color.white, "White");

    }
    #endregion



}

public enum ColorPick
{
    Magenta,
    Blue,
    Red,
    Brown,
    Green

}
