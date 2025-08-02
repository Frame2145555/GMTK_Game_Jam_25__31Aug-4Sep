using System;
using UnityEngine;

public class Mom : Grabable
{
    [SerializeField] RopeVerlet rope;
    [SerializeField] public DistanceJoint2D distanceJoint;

    [Header("throwing momentum")]
    [SerializeField] float initialMomemntum = 10f;
    float momentum = 10;
    Vector2 currentVel;
    bool doMomentum = false;
    [SerializeField] float reduceSpeed = 10f;
    [SerializeField] float throwingMomentumAmpilfier = 2;

    [Header("grabbing momentum")]
    [SerializeField] float forceThresholdSquare = 10f;

    [SerializeField] float pullStrengthInitial;
    [SerializeField] float pullStrengthBuildFactor = 1.2f;
    [SerializeField] float pullStrengthMaximumFactor =2;
    [SerializeField] float pullStrengthMinimumFactor = 0.6f;
    [SerializeField] float maxForceDecayFactor;

    [SerializeField] float maxForceInitial;
    [SerializeField] float maxForceBuildFactor = 1.2f;
    [SerializeField] float maxForceMaximumFactor = 2;
    [SerializeField] float maxForceMinimumFactor = 0.6f;
    [SerializeField] float pullStrengthDecayFactor;

    protected override void Start()
    {
        base.Start();
        rope.FixedTailTo(gameObject);
        distanceJoint = GetComponent<DistanceJoint2D>();
        pullStrengthInitial = pullStrengthByThousand;
        maxForceInitial = maxForceByThousand;
        distanceJoint.distance = rope.MaxLength;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        Momemntum();

        if(isBeingGrab)
        {
            Debug.Log(rb.linearVelocity.sqrMagnitude);
            if (rb.linearVelocity.sqrMagnitude > forceThresholdSquare)
            {
                pullStrengthByThousand += pullStrengthBuildFactor * Time.deltaTime;
                if (pullStrengthByThousand > pullStrengthInitial * pullStrengthMaximumFactor) pullStrengthByThousand = pullStrengthInitial * pullStrengthMaximumFactor;

                    maxForceByThousand += maxForceBuildFactor * Time.deltaTime;
                if (maxForceByThousand > maxForceInitial * maxForceMaximumFactor) maxForceByThousand = maxForceInitial * maxForceMaximumFactor;
            }
            else
            {
                pullStrengthByThousand -= pullStrengthDecayFactor * Time.deltaTime;
                if (pullStrengthByThousand < pullStrengthInitial * pullStrengthMinimumFactor) pullStrengthByThousand = pullStrengthInitial * pullStrengthMinimumFactor;


                maxForceByThousand -= maxForceDecayFactor * Time.deltaTime;
                if (maxForceByThousand < maxForceInitial * maxForceMinimumFactor) maxForceByThousand = maxForceInitial * maxForceMinimumFactor;

            }
        }
    }

    private void Momemntum()
    {
        if (!doMomentum) return;
        momentum -= Time.deltaTime * reduceSpeed;
        if (momentum < 0)
        {
            doMomentum = false ;
            return;
        }
        Vector2 newVel = currentVel * momentum;
        if (rb.linearVelocity.sqrMagnitude < newVel.sqrMagnitude)
        {
            rb.linearVelocity = newVel;
        }
        else
        {
            doMomentum = false;
            return;
        }
    }

    public override void Release()
    {
        base.Release();
        StartMomentum();
        pullStrengthByThousand = pullStrengthInitial;
        maxForceByThousand = maxForceInitial;
    }

    private void StartMomentum()
    {
        doMomentum = true;
        momentum = initialMomemntum;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - rb.position).normalized;
        currentVel = rb.linearVelocity.magnitude * throwingMomentumAmpilfier * dir;
    }
}
