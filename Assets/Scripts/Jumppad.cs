using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class Jumppad : MonoBehaviour
{
    public float bounceForce = 10f;
    void Start()
    {
        Collider2D boxCollider = GetComponent<Collider2D>();
        boxCollider.isTrigger = true;   
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            float angle = transform.eulerAngles.z + 90;

            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

            rb.linearVelocity = Vector2.zero; 
            rb.AddForce(direction * bounceForce * rb.mass, ForceMode2D.Impulse);
        }
    }
}
