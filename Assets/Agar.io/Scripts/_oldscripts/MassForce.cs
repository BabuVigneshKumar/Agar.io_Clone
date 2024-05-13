using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MassForce : MonoBehaviour
{
    public bool ApplyForce = false;
    public float speed = 30f;
    public float LoseSpeed = 140f;
    public float RandomRotation = 10;
    public float RandomForce = 10;
    public float MaxSize = 1f;
    public float MinSize = 0.4f;


    [System.Obsolete]
    private void Start()
    {
        RandomSize();
        //if (ApplyForce == false)
        //{
        //    enabled = false;
        //    return;
        //}

        //giving random rotation
        Vector2 direction = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float zr = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        zr += Random.RandomRange(-RandomRotation, RandomRotation);
        transform.rotation = Quaternion.Euler(0, 0, zr);

        speed += Random.Range(-RandomForce, RandomForce);
    }

    private void Update()
    {
        if (ApplyForce)
        {
            transform.Translate(Vector3.up * speed * Time.deltaTime);   
            speed -= LoseSpeed * Time.deltaTime;

            //stopping the script if it is not in use
            if(speed <= 0)
            {
                //enabled = false;
            }
        }
    }


    public void RandomSize()
    {
        float r = Random.Range(MinSize, MaxSize);
        r *= transform.localScale.x;
        transform.localScale = new Vector3(r, r, r);

    }
}
