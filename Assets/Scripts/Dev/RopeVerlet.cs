
using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;

public class RopeVerlet : MonoBehaviour
{
    [System.Serializable]
    public class RopePoint
    {
        public Vector2 CurrrentPosition;
        public Vector2 OldPosition;
        public float Mass;

        public RopePoint(Vector2 pos, float mass)
        {
            CurrrentPosition = pos;
            OldPosition = pos;
            Mass = 1;
        }

        public float InvertMass() => 1 / Mass;
    }

    [System.Serializable]
    public class RopeSegment
    {
        public RopePoint A;
        public RopePoint B;
        public float RestLength;
        public float Tension;

        public RopeSegment(RopePoint a, RopePoint b, float restLength)
        {
            A = a;
            B = b;
            RestLength = restLength;
            Tension = 0;
        }
    }

    [Header("Rope")]
    [SerializeField] int numberOfRopeSegment = 50;
    [SerializeField] float segmentLength = 0.225f;
    [SerializeField] int pointMass = 1;
    [SerializeField] float maxLength = 5;

    [Header("Physics")]
    [SerializeField] Vector2 gravityForce = new Vector2(0f, -2f);
    [SerializeField] float dampingFactor = 0.98f;
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float collisionRadius = 0.1f;
    [SerializeField] float bounceFactor = 0.1f;
    [SerializeField] float correctionClampAmount = 0.1f;


    [Header("Constraints")]
    [SerializeField] int constrainPerUpdate = 50;
    [SerializeField] float stiffness = 0.225f;

    [SerializeField] bool isHeadFixed = false;
    [SerializeField] Transform headFixedTarget = null;
    [SerializeField] Rigidbody2D headRigidBody;

    [SerializeField] bool isTailFixed = false;
    [SerializeField] Transform tailFixedTransform = null;
    [SerializeField] Rigidbody2D tailRigidBody;

    [Header("Optimizaions")]
    [SerializeField] int _collisionSegmentInterval = 2;

    [SerializeField] Vector3 RopeStartPoint;

    private LineRenderer lineRenderer;
    public List<RopePoint> ropePoints = new List<RopePoint>();
    public List<RopeSegment> ropeSegments = new List<RopeSegment>();

    public Vector3 HeadPosition { get => ropePoints[0].CurrrentPosition; } 
    public Vector3 TailPosition { get => ropePoints[ropePoints.Count - 1].CurrrentPosition; }
    
    [ContextMenu("Update Mass")]
    void UpdateMass()
    {
        foreach (var point in ropePoints)
        {
            point.Mass = pointMass;
        }
    }

    public void FixedHeadTo(Transform t)
    {
        isHeadFixed = true;
        headFixedTarget = t;
    }
    public void FixedTailTo(Transform t)
    {
        isTailFixed = true;
        tailFixedTransform = t;
    }

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfRopeSegment + 1;

        BuildRope();

    }

    private void BuildRope()
    {
        Vector3 refRopeStartPoint = RopeStartPoint;
        int pointCount = numberOfRopeSegment + 1;
        int segmentCount = numberOfRopeSegment;

        for (int i = 0; i < pointCount; i++)
        {
            ropePoints.Add(new RopePoint(RopeStartPoint, pointMass));
            refRopeStartPoint.y -= segmentLength;
        }

        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropePoints[i], ropePoints[i + 1], segmentLength));
        }
    }



    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }
    void FixedUpdate()
    {
        RunSolver();
    }

    private void RunSolver()
    {
        VerletIntegration();

        for (int i = 0; i < constrainPerUpdate; i++)
        {
            FixPosition();
            SolveDistanceConstrain();
            SolveMaxLengthConstrain();
            ApplyTension();
            CollisionHandle();
            RigidbodyHandle();
        }
    }

    void VerletIntegration()
    {
        for (int i = 0; i < ropePoints.Count; i++)
        {
            RopePoint p = ropePoints[i];
            Vector2 velocity = (p.CurrrentPosition - p.OldPosition) * dampingFactor;

            p.OldPosition = p.CurrrentPosition;
            p.CurrrentPosition += velocity;
            p.CurrrentPosition += p.Mass * gravityForce * Time.fixedDeltaTime * Time.fixedDeltaTime;
        }
    }
    private void FixPosition()
    {
        if (isHeadFixed)
        {
            RopePoint refHead = ropePoints[0];
            refHead.CurrrentPosition = headFixedTarget.position;

        }

        if (isTailFixed)
        {
            int tailIndex = ropePoints.Count - 1;

            RopePoint refTail = ropePoints[tailIndex];
            refTail.CurrrentPosition = tailFixedTransform.position;
        }
    }


    private void SolveDistanceConstrain()
    {
        foreach (var segment in ropeSegments)
        {
            RopePoint point_A = segment.A;
            RopePoint point_B = segment.B;

            //calculate stretch
            float dist = (point_A.CurrrentPosition - point_B.CurrrentPosition).magnitude;
            float stretch = dist - segment.RestLength;

            //calculate invmass
            float invMass_A = point_A.InvertMass();
            float invMass_B = point_B.InvertMass();
            float totalInvMass = invMass_A + invMass_B;

            Vector2 changeDir = (point_A.CurrrentPosition - point_B.CurrrentPosition).normalized;

            //calculate correction value
            Vector2 totalCorrection = changeDir * stretch * stiffness;
            Vector2 correction_A = totalCorrection * (invMass_A / totalInvMass);
            Vector2 correction_B = totalCorrection * (invMass_B / totalInvMass);

            //apply correction value
            point_A.CurrrentPosition -= correction_A;
            point_B.CurrrentPosition += correction_B;
        }
    }

    private void SolveMaxLengthConstrain()
    {
        RopePoint head = ropePoints[0];
        RopePoint tail = ropePoints[^1];

        Vector2 diff = tail.CurrrentPosition - head.CurrrentPosition;
        float currentLength = (head.CurrrentPosition - tail.CurrrentPosition).magnitude;

        if (currentLength > maxLength)
        {
            Vector2 dir = diff.normalized;
            float excess = currentLength - maxLength;

            Vector2 correction = dir * excess;

            head.CurrrentPosition += correction * 0.5f;
            tail.CurrrentPosition -= correction * 0.5f;

            if (headRigidBody != null)
            {
                headRigidBody.AddForce(correction*0.5f / Time.fixedDeltaTime);
            }
            if (tailRigidBody != null)
            {
                tailRigidBody.AddForce(-correction*0.5f / Time.fixedDeltaTime);
            }
        }
    }

    void ApplyTension()
    {
        foreach (var segment in ropeSegments)
        {
            RopePoint point_A = segment.A;
            RopePoint point_B = segment.B;

            //calculate stretch
            float dist = (point_A.CurrrentPosition - point_B.CurrrentPosition).magnitude;
            float stretch = dist - segment.RestLength;

            //calculate tension
            float tension = 0;
            if (stretch > 0f)
            {
                tension = stiffness * stretch;
            }

            segment.Tension = tension;

        }
    }

    private void RigidbodyHandle()
    {
        RopeSegment firstSegment = ropeSegments[0];
        Vector2 force;
        Vector2 dir;

        dir = (firstSegment.B.CurrrentPosition - firstSegment.A.CurrrentPosition).normalized;
        force = firstSegment.Tension * dir; 
        
        if (firstSegment.Tension > 0f)
            headRigidBody.AddForce(force);
        firstSegment.A.CurrrentPosition = headRigidBody.position;

        //tail Rigidbody
        RopeSegment lastSegment = ropeSegments[ropeSegments.Count - 1];
        
        dir = (lastSegment.B.CurrrentPosition - lastSegment.A.CurrrentPosition).normalized;
        force = lastSegment.Tension * -dir; 
        
        tailRigidBody.AddForce(force);
        lastSegment.B.CurrrentPosition = tailRigidBody.position;
    }

    void CollisionHandle()
    {
        foreach (var point in ropePoints)
        {
            Vector2 oldPos = point.OldPosition;
            Vector2 newPos = point.CurrrentPosition;
            Vector2 vel = newPos - oldPos;
            Vector2 dir = vel.normalized;
            float dist = vel.magnitude;

            if (dist <= 0f) continue;

            RaycastHit2D hit = Physics2D.CircleCast(oldPos, collisionRadius, dir, dist, collisionMask);

            if (hit.collider != null)
            {
                point.OldPosition = point.CurrrentPosition;
                point.CurrrentPosition = hit.point + hit.normal * collisionRadius;
            }
        }
    }

    void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[ropePoints.Count];
        for (int i = 0; i < ropePoints.Count; i++)
        {
            ropePositions[i] = ropePoints[i].CurrrentPosition;
        }
        lineRenderer.SetPositions(ropePositions);
    }
    public float Length()
    {
        float sum = 0;
        for (int i = 0; i < numberOfRopeSegment-1; i++)
        {
            sum += (ropePoints[i].CurrrentPosition - ropePoints[i + 1].CurrrentPosition).magnitude;
        }
        return sum;
    }

    public float LengthNormal()
    {
        return segmentLength * numberOfRopeSegment;
    }
}