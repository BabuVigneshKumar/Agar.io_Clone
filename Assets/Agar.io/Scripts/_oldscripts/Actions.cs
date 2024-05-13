using JetBrains.Annotations;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions : NetworkBehaviour
{
    public GameObject Mass;
    public Transform MassPosition;
    public float percentage = 1f;
    PlayerMassTest playerMass;
    public float SplitMass = 1.5f;
    public GameObject PlayerCloneParent;

    void Start()
    {
        playerMass = GetComponent<PlayerMassTest>();    

        //ColorManager.instance.GetRandomColor(GetComponent<SpriteRenderer>());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void ThorwMass()
    {
        if (transform.localScale.x <=1)
        {
            return;
        }

        Vector2 direction = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //changing the x and y position to angles
        float an = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0,0,an);

        //ins new mass to throw
        GameObject m = Instantiate(Mass, MassPosition.position, Quaternion.identity);
        ColorManager.instance.TargetColor(GetComponent<SpriteRenderer>(), m.GetComponent<SpriteRenderer>());
        
        //applying force
        m.GetComponent<MassForce>().ApplyForce = true;



        //Adding Mass to the player
        playerMass.AddMass(m);

        //loosing Mass when throwing
        transform.localScale -= new Vector3(0.05f, 0.05f, 0.05f);


    }

   
    public void Split()
    {
        if(transform.localScale.x <= 2)
        {
            return;
        }
        transform.localScale /= SplitMass;
        GameObject g = Instantiate(gameObject,transform.position, Quaternion.identity);
       
        g.transform.parent = PlayerCloneParent.transform;
        g.GetComponent<SplitForce>().enabled = true;
        g.GetComponent<SplitForce>().SplitForcee();

    }



}

