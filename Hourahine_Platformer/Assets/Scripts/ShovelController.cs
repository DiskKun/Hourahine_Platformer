using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelController : MonoBehaviour
{
    Rigidbody2D rb;
    public GameObject visuals;
    public float baseRotation;
    public float timeToDestroy;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, timeToDestroy);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mathf.Abs(rb.velocity.magnitude) < 0.1)
        {
            rb.velocity = Vector2.zero;
        }
        visuals.transform.localEulerAngles = new Vector3(0, 0, baseRotation + Mathf.Rad2Deg * Mathf.Atan2(rb.velocity.y, rb.velocity.x));
        
    }
}
