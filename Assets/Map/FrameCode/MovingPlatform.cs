using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Move Between Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float moveSpeed = 20f;

    [Header("Rotate Around Point A")]
    [SerializeField] private bool rotateAroundPointA = false;
    [SerializeField] private float rotationRadius = 5f;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 nextPosition;
    private float rotationAngle = 0f;

    private void Start()
    {
        nextPosition = pointB.position;

        if (rotateAroundPointA)
        {
            transform.position = pointA.position + Vector3.right * rotationRadius;
        }
    }

    private void Update()
    {
        if (rotateAroundPointA)
        {
            RotateAroundPointA();
        }
        else
        {
            MovePlatform();
            Debug.DrawLine(pointA.position, pointB.position, Color.red);
        }
    }

    private void MovePlatform()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        if (transform.position == nextPosition)
        {
            nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
        }
    }

    private void RotateAroundPointA()
    {
        rotationAngle += rotationSpeed * Time.deltaTime;

        float rad = rotationAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * rotationRadius;
        float y = Mathf.Sin(rad) * rotationRadius;

        transform.position = pointA.position + new Vector3(x, y, 0f);
    }

    private void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        if (rotateAroundPointA && pointA != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(pointA.position, rotationRadius);
        }
    }
}
