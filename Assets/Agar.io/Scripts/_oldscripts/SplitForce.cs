using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitForce : MonoBehaviour
{
    public float Speed;
    public float LooseSpeed;
    public float DefaultSpeed;
    public bool ApplyForce = false;

    public void SplitForcee()
    {
        

        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<PlayerMovementTest>().LockAction = true;
        Vector2 dir = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float Angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0,0,Angle);
        Speed = DefaultSpeed;
        ApplyForce = true;
    }

    private void Update()
    {
        if (ApplyForce == false)
        {
            enabled = false;
            return;
        }

        transform.Translate(Vector2.up *Speed * Time.deltaTime); 
        Speed -= LooseSpeed * Time.deltaTime;
        
        if(Speed <= 0)
        {
            GetComponent<CircleCollider2D>().enabled=true;
            GetComponent<PlayerMovementTest>().LockAction = false;
            enabled = false ;

        }
    }


}
