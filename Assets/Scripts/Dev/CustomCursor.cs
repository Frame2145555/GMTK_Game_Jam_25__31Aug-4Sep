using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [SerializeField] float interpolationFactor = 0.3f;
    [SerializeField] float mouseSpeed = 0.6f;
    [SerializeField] float maxForce = 20f;
    [SerializeField] float forceMultitiler = 1.5f;

    [SerializeField] float gravity = 10;

    Vector2 desireMouseDistance;

    bool physicActive = false;

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        desireMouseDistance = rb.position;
    }

    private void Update()
    {
        physicActive = Input.GetMouseButton(0);
    }
    private void FixedUpdate()
    {
        ToggleMouseCursor(physicActive);

        if (physicActive)
        {
            rb.gravityScale = 0;
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 diff = mouseWorldPosition - rb.position;
            Vector2 dir = diff.normalized;

            Vector2 force = diff * forceMultitiler;

            if (force.magnitude > maxForce) force = force.normalized * maxForce;
            rb.AddForce(force);

            //rb.MovePosition(Vector2.Lerp(rb.position, worldMousePosition, interpolationFactor));


            
            //Vector2 mouseDetla = Input.mousePositionDelta;
            //desireMouseDistance = rb.position + mouseDetla * mouseSpeed * Time.fixedDeltaTime;
            //rb.MovePosition(Vector2.Lerp(rb.position, desireMouseDistance, interpolationFactor));
        }
        else
        {
            rb.gravityScale = 10;
            desireMouseDistance = rb.position;
        }

    }
    private void ToggleMouseCursor(bool isPhysicActive)
    {
        if (isPhysicActive)
        {
            //Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            //Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

}
