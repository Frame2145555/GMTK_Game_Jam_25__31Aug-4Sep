using UnityEngine;

public class Mom : Grabable
{
    [SerializeField] RopeVerlet rope;
    [SerializeField] float impulseSurfaceForceByThousand = 1;

    [SerializeField] Vector2 _mouseDiff;
    [SerializeField] Vector2 _centDiff;
    [SerializeField] Vector2 _centDir;
    [SerializeField] Vector2 _centCW;
    [SerializeField] Vector2 _proj;
    [SerializeField] Vector2 _force;

    public Transform grabbingNodeTransform = null;
    protected override void Start()
    {
        base.Start();
        rope.FixedTailTo(gameObject);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    protected override void PullObject()
    {
        if (grabbingNodeTransform == null) return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float pullStrength = pullStrengthByThousand * 1000f;
        float maxForce = maxForceByThousand * 1000f;

        Vector2 mouseDiff = mouseWorld - rb.position;

        Vector2 centDiff = (Vector2)grabbingNodeTransform.position - rb.position;
        Vector2 centDir = centDiff.normalized;
        Vector2 centCW = new Vector2(centDir.y, -centDir.x);

        Vector2 proj = Vector2.Dot(mouseDiff, centCW) / centCW.sqrMagnitude * centCW;

        Vector2 force = proj * pullStrength;
        if (force.magnitude > maxForce) force = force.normalized * maxForce;

        rb.AddForce(force);

        _mouseDiff = mouseDiff ;
        _centDiff  = centDiff  ;
        _centDir   = centDir   ;
        _centCW    = centCW    ;
        _proj      = proj      ;
        _force     = force     ;
    }

    public override void Release()
    {
        base.Release();

        rb.AddForce(_force.normalized * impulseSurfaceForceByThousand * 1000, ForceMode2D.Impulse);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3)_mouseDiff);
        Gizmos.color = Color.white;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3)_centDiff );
        Gizmos.color = Color.blue;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3)_centDir  );
        Gizmos.color = Color.red;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3)_centCW   );
        Gizmos.color = Color.black;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3)_proj     );
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine( transform.position, transform.position + (Vector3)_force    );
    }

}
