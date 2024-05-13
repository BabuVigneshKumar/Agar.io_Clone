using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerForce : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2 dir = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //float Angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 90f;
        //transform.rotation = Quaternion.Euler(0, 0, Angle);

        if (rb != null)
        {

            //Vector2 forceDirection = dir.normalized;
            float forceMagnitude = 10f;
            rb.AddForce(Vector2.up * forceMagnitude * Time.deltaTime, ForceMode2D.Impulse);
        }

    }
}
