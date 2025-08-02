using UnityEngine;

public class Moveable : MonoBehaviour
{

    [Header("Positions")]
    Transform startPoint;
    public Transform point;

    [Header("Movement")]
    public float speed = 1f;
    void Start()
    {
        startPoint = transform;
    } 
    void Update()
    {
        // Calculate a time-based t value that loops between 0 and 1
        float t = Mathf.PingPong(Time.time * speed, 1f);
        
        // Interpolate between the two points
        transform.position = Vector3.Lerp(startPoint.position, point.position, t);
    }
}
