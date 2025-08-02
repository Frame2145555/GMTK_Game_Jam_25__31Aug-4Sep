
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        public Vector2 CurrentPosition;
        public Vector2 OldPosition;
        public float Mass;

        public RopePoint(Vector2 pos, float mass)
        {
            CurrentPosition = pos;
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
    [SerializeField] float headPullForceByThousand = 100;

    [SerializeField] bool isTailFixed = false;
    [SerializeField] Transform tailFixedTransform = null;
    [SerializeField] Rigidbody2D tailRigidBody;
    [SerializeField] float tailPullForceByThousand = 100;

    [Header("Optimizaions")]
    [SerializeField] int _collisionSegmentInterval = 2;

    [SerializeField] Vector3 RopeStartPoint;

    private LineRenderer lineRenderer;
    public List<RopePoint> ropePoints = new List<RopePoint>();
    public List<RopeSegment> ropeSegments = new List<RopeSegment>();

    public Vector3 HeadPosition { get => ropePoints[0].CurrentPosition; } 
    public Vector3 TailPosition { get => ropePoints[^1].CurrentPosition; }
    
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
            Vector2 velocity = (p.CurrentPosition - p.OldPosition) * dampingFactor;

            p.OldPosition = p.CurrentPosition;
            p.CurrentPosition += velocity;
            p.CurrentPosition += p.Mass * Time.fixedDeltaTime * Time.fixedDeltaTime * gravityForce;
        }
    }
    private void FixPosition()
    {
        if (isHeadFixed)
        {
            RopePoint refHead = ropePoints[0];
            refHead.CurrentPosition = headFixedTarget.position;

        }

        if (isTailFixed)
        {
            int tailIndex = ropePoints.Count - 1;

            RopePoint refTail = ropePoints[tailIndex];
            refTail.CurrentPosition = tailFixedTransform.position;
        }
    }


    private void SolveDistanceConstrain()
    {
        foreach (var segment in ropeSegments)
        {
            RopePoint point_A = segment.A;
            RopePoint point_B = segment.B;

            //calculate stretch
            float dist = (point_A.CurrentPosition - point_B.CurrentPosition).magnitude;
            float stretch = dist - segment.RestLength;

            //calculate invmass
            float invMass_A = point_A.InvertMass();
            float invMass_B = point_B.InvertMass();
            float totalInvMass = invMass_A + invMass_B;

            Vector2 changeDir = (point_A.CurrentPosition - point_B.CurrentPosition).normalized;

            //calculate correction value
            Vector2 totalCorrection = stiffness * stretch * changeDir;
            Vector2 correction_A = totalCorrection * (invMass_A / totalInvMass);
            Vector2 correction_B = totalCorrection * (invMass_B / totalInvMass);

            //apply correction value
            point_A.CurrentPosition -= correction_A;
            point_B.CurrentPosition += correction_B;
        }
    }

    private void SolveMaxLengthConstrain()
    {
        //RopePoint head = ropePoints[0];
        //RopePoint tail = ropePoints[^1];

        //Vector2 diff = tail.CurrrentPosition - head.CurrrentPosition;
        //float currentLength = (head.CurrrentPosition - tail.CurrrentPosition).magnitude;

        //if (currentLength > maxLength)
        //{
        //    Vector2 dir = diff.normalized;
        //    float excess = currentLength - maxLength;

        //    Vector2 correction = dir * excess;

        //    head.CurrrentPosition += correction * 0.5f;
        //    tail.CurrrentPosition -= correction * 0.5f;

        //    if (headRigidBody != null)
        //    {
        //        headRigidBody.AddForce(correction*0.5f / Time.fixedDeltaTime);
        //    }
        //    if (tailRigidBody != null)
        //    {
        //        tailRigidBody.AddForce(-correction*0.5f / Time.fixedDeltaTime);
        //    }
        //}

        float totalLength = 0;
        foreach (var segment in ropeSegments)
        {
            totalLength += Vector2.Distance(segment.A.CurrentPosition, segment.B.CurrentPosition);
        }

        float excess = totalLength - maxLength;

        if (excess > 0)
        {
            float shrinkPerSegment = excess / ropeSegments.Count;   

            for (int i = 0; i < ropeSegments.Count; i++)
            {
                RopeSegment segment = ropeSegments[i];
                Vector2 dir = (segment.B.CurrentPosition - segment.A.CurrentPosition).normalized;

                segment.A.CurrentPosition += 0.5f * shrinkPerSegment * dir;
                segment.B.CurrentPosition -= 0.5f * shrinkPerSegment * dir;
                if (i == 0)
                {
                    headRigidBody.AddForce(0.5f * 10e3f * headPullForceByThousand * shrinkPerSegment * Time.fixedDeltaTime * dir);
                }
                if (i == ropeSegments.Count - 1)
                {
                    tailRigidBody.AddForce(0.5f * 10e3f * shrinkPerSegment * tailPullForceByThousand * Time.fixedDeltaTime * -dir);
                }
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
            float dist = (point_A.CurrentPosition - point_B.CurrentPosition).magnitude;
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

        dir = (firstSegment.B.CurrentPosition - firstSegment.A.CurrentPosition).normalized;
        force = firstSegment.Tension * dir; 
        
        if (firstSegment.Tension > 0f)
            headRigidBody.AddForce(force);
        firstSegment.A.CurrentPosition = headRigidBody.position;

        //tail Rigidbody
        RopeSegment lastSegment = ropeSegments[^1];
        
        dir = (lastSegment.B.CurrentPosition - lastSegment.A.CurrentPosition).normalized;
        force = lastSegment.Tension * -dir; 
        
        tailRigidBody.AddForce(force);
        lastSegment.B.CurrentPosition = tailRigidBody.position;
    }

    void CollisionHandle()
    {
        foreach (var point in ropePoints)
        {
            Vector2 oldPos = point.OldPosition;
            Vector2 newPos = point.CurrentPosition;
            Vector2 vel = newPos - oldPos;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(point.CurrentPosition,collisionRadius, collisionMask);
            foreach (var collider in colliders)
            {
                Vector2 closestPoint = collider.ClosestPoint(point.CurrentPosition);
                float distance = Vector2.Distance(point.CurrentPosition, closestPoint);

                if (distance < collisionRadius)
                {
                    Vector2 normal = (point.CurrentPosition - closestPoint).normalized;
                    if (normal == Vector2.zero)
                    {
                        normal = (point.CurrentPosition - (Vector2)collider.transform.position).normalized;

                    }
                    float depth = collisionRadius - distance;
                    point.CurrentPosition += normal * depth;

                    vel = Vector2.Reflect(vel, normal) * bounceFactor;
                }
            }
            point.OldPosition = point.CurrentPosition - vel;
            
            Vector2 dir = vel.normalized;
            float dist = vel.magnitude;

            if (dist <= 0f) continue;

            RaycastHit2D hit = Physics2D.CircleCast(oldPos, collisionRadius, dir, dist, collisionMask);

            if (hit.collider != null)
            {
                point.OldPosition = point.CurrentPosition;
                point.CurrentPosition = hit.point + hit.normal * collisionRadius;
            }
        }
    }

    void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[ropePoints.Count];
        for (int i = 0; i < ropePoints.Count; i++)
        {
            ropePositions[i] = ropePoints[i].CurrentPosition;
        }
        lineRenderer.SetPositions(ropePositions);
    }
    public float Length()
    {
        float sum = 0;
        for (int i = 0; i < numberOfRopeSegment-1; i++)
        {
            sum += (ropePoints[i].CurrentPosition - ropePoints[i + 1].CurrentPosition).magnitude;
        }
        return sum;
    }

    public float LengthNormal()
    {
        return segmentLength * numberOfRopeSegment;
    }

    private void OnDrawGizmos()
    {
        if (ropePoints.Count <= 0) 
        { 
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, collisionRadius);
            return;
        }

        for (int i = 0; i < ropePoints.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ropePoints[i].OldPosition, collisionRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ropePoints[i].CurrentPosition, collisionRadius);

        }

        foreach (var segment in ropeSegments)
        {
            Vector3 a;
            Vector3 b;

            a = segment.A.OldPosition;
            b = segment.B.OldPosition;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(a, b);

            a = segment.A.CurrentPosition;
            b = segment.A.CurrentPosition;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(a, b);

        }
    }
}