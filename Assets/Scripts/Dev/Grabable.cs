using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Grabable : MonoBehaviour
{
    [Header("Force")]
    [SerializeField] protected float pullStrengthByThousand = 2.5f;
    [SerializeField] protected float maxForceByThousand = 3f;
    [SerializeField] protected bool useGravityWhenBeingGrab = false;
    [SerializeField] float grabbableRadius = 2f;

    [SerializeField] protected bool isBeingGrab = false;
    [SerializeField] protected bool canBeGrab = true;

    protected Rigidbody2D rb;
    float initialRbGravity = 0;

    public Action onGrab;
    public Action onRelease;

    public bool CanBeGrab { set => canBeGrab = value; }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialRbGravity = rb.gravityScale;
    }

    protected virtual void Update()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if ((mouseWorldPosition - (Vector2)transform.position).magnitude < grabbableRadius
            && Input.GetMouseButtonDown(0) && !isBeingGrab)
        {
            Grab();
        }

        if (Input.GetMouseButtonUp(0) && isBeingGrab)
        {
            Release();
        }
    }

    protected virtual void FixedUpdate()
    {
        //boolean algebra = a`b` + a
        ToggleGavity((!useGravityWhenBeingGrab && !isBeingGrab) || useGravityWhenBeingGrab);
        
        if (isBeingGrab)
        {
            PullObject();
        }
    }

    protected virtual void PullObject()
    {
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 diff = mouseWorldPosition - rb.position;

        float pullStrength = pullStrengthByThousand * 1000f;
        float maxForce = maxForceByThousand * 1000f;

        Vector2 force = diff * pullStrength;
        if (force.magnitude > maxForce) force = force.normalized * maxForce;

        rb.AddForce(force);
    }

    void ToggleGavity(bool value)
    {
        rb.gravityScale = value ? initialRbGravity : 0;
    }

    public virtual void Grab()
    {
        if (canBeGrab)
        {
            isBeingGrab = true;
            onGrab?.Invoke();
        }
    }

    public virtual void Release()
    {
        isBeingGrab = false;
        onRelease?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isBeingGrab? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, grabbableRadius);
    }

}
