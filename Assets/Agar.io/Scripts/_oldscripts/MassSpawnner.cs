using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class MassSpawnner : NetworkBehaviour
{
    public static MassSpawnner Instance;

    public List<GameObject> Players = new List<GameObject>();

    public List<GameObject> CreatedMass = new List<GameObject>();
    public int MaxMass = 500;
    public int MaxPlayers = 5;
    public float TimetoCreateMass = 0.05f;
    MapLimits map;


    private void Awake()
    {
        Instance = this;
        map = MapLimits.Instance;
    }
    public bool IsCreateMass;
    public void Start()
    {
       
    }

    [Command]
    public void CmdSpawnMass()
    {
        IsCreateMass = true;
        Debug.Log("MAss Obj  &&&&&&&&& ");
        StartCoroutine(CreateNewMass());

    }

    //public void CreateMass()
    //{


    //    StartCoroutine(CreateNewMass());
    //}
    public IEnumerator CreateNewMass()
    {

        Debug.Log("MAss Obj  &&&&&&&&&  Created ");

        yield return new WaitForSeconds(TimetoCreateMass);


        if (CreatedMass.Count < MaxMass)
        {

            GameObject m = PoolManager.Instance.GetPooledObject();

            //ColorManager.instance.GetRandomColor(m.GetComponent<SpriteRenderer>());




            if (m == null)
            {
                Debug.LogWarning("No available objects in the pool.");
                yield break;
            }


            m.SetActive(true);

            m.transform.position = new Vector2(Random.Range(-map.Maplimits.x, map.Maplimits.x), Random.Range(-map.Maplimits.y, map.Maplimits.y)) / 2;

            for (int i = 0; i < Players.Count; ++i)
            {
                PlayerMassTest pp = Players[i].GetComponent<PlayerMassTest>();
                pp.AddMass(m);
            }


            AddMass(m);

        }

        StartCoroutine(CreateNewMass());
    }

    public void AddMass(GameObject m)
    {
        if (CreatedMass.Contains(m) == false)
        {
            CreatedMass.Add(m);

        }
    }

    public void RemoveMass(GameObject m)
    {
        if (CreatedMass.Contains(m) == true)
        {
            CreatedMass.Remove(m);

        }

    }
    public void AddPlayer(GameObject b)
    {
        if (Players.Contains(b) == false)
        {
            Players.Add(b);
        }

    }

    public void RemovePlayer(GameObject b)
    {
        if (Players.Contains(b) == true)
        {
            Players.Remove(b);
        }
    }



}
