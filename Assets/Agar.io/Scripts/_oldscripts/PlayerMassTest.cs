using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class PlayerMassTest : MonoBehaviour
{

    public List<GameObject> MassList = new List<GameObject>();
    //public GameObject parent;

    // Start is called before the first frame update
    void Start()
    {

        UpdateMass();
        InvokeRepeating("CheckMass", 0, 0.1f);
    }

    // Update is called once per frame
    public void UpdateMass()
    {
        MassList.Clear();
        MassList.AddRange(GameObject.FindGameObjectsWithTag("Mass"));
    }


    public void AddMass(GameObject MassObject)
    {
        MassList.Add(MassObject);
        MassObject.transform.SetParent(UIController.instance.gameHUD.playerCloneParent.transform);
    }


    public void RemoveMass(GameObject MassObject)
    {
        MassList.Remove(MassObject);
        MassSpawnner.Instance.RemoveMass(MassObject);
        MassObject.SetActive(false);
        MassObject.transform.SetParent(UIController.instance.gameHUD.PoolMangerParent.transform);
    }


    public void CheckMass()
    {
        for (int i = 0; i < MassList.Count; i++)
        {
            if (MassList[i] == null)
            {
                UpdateMass();
                return;
            }
            GameObject m = MassList[i];
            if (m.activeSelf && Vector2.Distance(transform.position, m.transform.position) <= transform.localScale.x / 2)
            {
                RemoveMass(m);
                playerSize();

            }
        }
    }


    public void playerSize()
    {
        transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
    }



}