using UnityEditor.Callbacks;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
[RequireComponent(typeof(BoxCollider2D))]
public class DestoryableBox : MonoBehaviour
{
    [SerializeField] float requiredVelocity = 100f;
    BoxCollider2D boxCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Mom")
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rb.velocity.magnitude >= requiredVelocity)
            {
                Debug.Log("Destroy");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Speed too low");
                StartCoroutine(Await());


            }
        }
    }
    IEnumerator Await()
    {
        boxCollider.isTrigger = false;
        yield return new WaitForSeconds(2f);
        boxCollider.isTrigger = true;
    }
}
