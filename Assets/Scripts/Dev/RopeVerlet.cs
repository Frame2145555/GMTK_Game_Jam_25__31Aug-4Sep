
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

        public float Length()
        {
            return Vector2.Distance(A.CurrentPosition, B.CurrentPosition);
        }
    }

    public struct IntersectPair
    {
        public RopeSegment A;
        public RopeSegment B;
        public Vector2 Point;

        public IntersectPair(RopeSegment a, RopeSegment b, Vector2 intersectionPoint)
        {
            A= a; 
            B=b;
            Point = intersectionPoint;
        }
    }

    [Header("Rope")]
    [SerializeField] int numberOfRopeSegmentOnStart = 50;
    [SerializeField] float segmentLength = 0.225f;
    [SerializeField] int pointMass = 1;
    [SerializeField] float maxLength = 5;
    [SerializeField] float currentLength = 0;

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
    [SerializeField] Transform headFixedTransform = null;
    [SerializeField] Rigidbody2D headRigidBody = null;
    [SerializeField] float headPullForceByThousand = 100;

    [SerializeField] bool isTailFixed = false;
    [SerializeField] Transform tailFixedTransform = null;
    [SerializeField] Rigidbody2D tailRigidBody = null;
    [SerializeField] float tailPullForceByThousand = 100;

    [Header("Intersection")]
    public List<IntersectPair> intersection = new List<IntersectPair>();

    [Header("Optimizaions")]
    [SerializeField] int _collisionSegmentInterval = 2;
    [SerializeField] float intersectionInterval = 2;
    [SerializeField] int fixUpdateCountMod10 = 0;

    [SerializeField] Vector3 RopeStartPoint;

    private LineRenderer lineRenderer;
    public List<RopePoint> ropePoints = new List<RopePoint>();
    public List<RopeSegment> ropeSegments = new List<RopeSegment>();

    public Vector3 HeadPosition { get => ropePoints[0].CurrentPosition; } 
    public Vector3 TailPosition { get => ropePoints[^1].CurrentPosition; }
    public float MaxLength { get => maxLength; set => maxLength = value; }
    
    [ContextMenu("Update Mass")]
    void UpdateMass()
    {
        foreach (var point in ropePoints)
        {
            point.Mass = pointMass;
        }
    }

    public void AddSegments(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            RopeSegment lastSegment = ropeSegments[^1];
            RopePoint lastPoint = ropePoints[^1];
        
            Vector2 dir = (lastSegment.B.CurrentPosition - lastSegment.A.CurrentPosition).normalized;

            RopePoint newTailPoint = new RopePoint(lastPoint.CurrentPosition + dir * segmentLength,pointMass);

            RopeSegment newTailSegment = new RopeSegment(lastPoint,newTailPoint,segmentLength);

            ropePoints.Add(newTailPoint);
            ropeSegments.Add(newTailSegment);
        }
    }
    public void RemoveSegments(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (ropePoints.Count <= 1) return;

            ropeSegments.RemoveAt(ropeSegments.Count - 1);
            ropePoints.RemoveAt(ropePoints.Count - 1);
        }
    }

    public void FixedHeadTo(GameObject go)
    {
        isHeadFixed = true;
        headFixedTransform = go.transform;
        headRigidBody = go.GetComponent<Rigidbody2D>();
    }
    public void FixedTailTo(GameObject go)
    {
        isTailFixed = true;
        tailFixedTransform = go.transform;
        tailRigidBody = go.GetComponent<Rigidbody2D>();
    }

    public void DetachHead()
    {
        isHeadFixed = false;
        headFixedTransform = null;
        headRigidBody = null;
    }
    public void DetachTail()
    {
        isTailFixed = false; 
        tailFixedTransform = null;
        tailRigidBody = null;
    }
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        BuildRope();

    }

    private void BuildRope()
    {
        Vector3 refRopeStartPoint = RopeStartPoint;
        int pointCount = numberOfRopeSegmentOnStart + 1;
        int segmentCount = numberOfRopeSegmentOnStart;

        for (int i = 0; i < pointCount; i++)
        {
            ropePoints.Add(new RopePoint(RopeStartPoint, pointMass));
            Debug.Log(refRopeStartPoint);
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
        fixUpdateCountMod10++;
        fixUpdateCountMod10 = fixUpdateCountMod10 % 10;
        RunSolver();

        currentLength = 0;
        foreach (var segment in ropeSegments)
        {
            currentLength += segment.Length();
        }

        if (fixUpdateCountMod10 % intersectionInterval == 0)
            intersection = FindAllIntersection();
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
            refHead.CurrentPosition = headFixedTransform.position;

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
                    if (headRigidBody != null)
                        headRigidBody.AddForce(0.5f * 10e3f * headPullForceByThousand * shrinkPerSegment * Time.fixedDeltaTime * dir);
                }
                if (i == ropeSegments.Count - 1)
                {
                    if (tailRigidBody != null)
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

        if (headRigidBody != null)
        {
            //head Rigidbody
            dir = (firstSegment.B.CurrentPosition - firstSegment.A.CurrentPosition).normalized;
            force = firstSegment.Tension * dir;

            if (firstSegment.Tension > 0f)
                headRigidBody.AddForce(force);
            firstSegment.A.CurrentPosition = headRigidBody.position;
        }

        if (tailRigidBody != null)
        {
            //tail Rigidbody
            RopeSegment lastSegment = ropeSegments[^1];

            dir = (lastSegment.B.CurrentPosition - lastSegment.A.CurrentPosition).normalized;
            force = lastSegment.Tension * -dir;

            if (firstSegment.Tension > 0f)
                tailRigidBody.AddForce(force);
            lastSegment.B.CurrentPosition = tailRigidBody.position;
        }
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
        lineRenderer.positionCount = ropePoints.Count;
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
        for (int i = 0; i < ropePoints.Count-1; i++)
        {
            sum += (ropePoints[i].CurrentPosition - ropePoints[i + 1].CurrentPosition).magnitude;
        }
        return sum;
    }

    public float LengthNormal()
    {
        return segmentLength * ropePoints.Count;
    }

    public List<IntersectPair> FindAllIntersection()
    {
        List<IntersectPair> intersectPairs = new();
        for (int i = 0;i < ropeSegments.Count - 2;i++)
        {
            for (int j = i + 1; j < ropeSegments.Count - 1; j++)
            {
                RopeSegment P = ropeSegments[i];
                RopeSegment Q = ropeSegments[j];

                if (ReferenceEquals(P.A, Q.A) ||
                    ReferenceEquals(P.A, Q.B) ||
                    ReferenceEquals(P.B, Q.A) ||
                    ReferenceEquals(P.B, Q.B)) continue;

                Vector2 intersectPoint = Vector2.zero;
                
                if (intersect(P.A.CurrentPosition,P.B.CurrentPosition,Q.A.CurrentPosition,Q.B.CurrentPosition, out intersectPoint))
                {
                    intersectPairs.Add(new IntersectPair(P, Q, intersectPoint));
                }
            }
        }
        return intersectPairs;
    }
    bool intersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2,out Vector2 intersect)
    {
        intersect = Vector2.zero;
        Vector2 r = p2 - p1;
        Vector2 s = q2 - q1;
        Vector2 qp = q1 - p1;

        float rxs = Cross2D(r, s);
        float qpxr = Cross2D(qp, r);

        if (Mathf.Approximately(rxs, 0f))
        {
            // Lines are parallel
            return false;
        }

        float t = Cross2D(qp, s) / rxs;
        float u = qpxr / rxs;

        if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
        {
            intersect = p1 + t * r;
            return true;
        }

        return false;
    }
    float Cross2D(Vector2 a, Vector2 b)
    {
        return a.x *b.y -a.y *b.x;
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