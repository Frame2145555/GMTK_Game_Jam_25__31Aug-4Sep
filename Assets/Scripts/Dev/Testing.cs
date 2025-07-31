using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] RopeVerlet rope;

    void Update()
    {
        rope.TailFollowTarget = transform.position;

        rope.HeadFollowTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (rope.Length() > rope.LengthNormal() * 1.5)
        {
            Vector2 dir = (transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized;
            transform.transform.position -= (Vector3)(dir * 10 * Time.deltaTime);
        }
    }
}
