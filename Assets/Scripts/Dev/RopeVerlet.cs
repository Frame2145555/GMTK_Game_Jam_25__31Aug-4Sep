
using System;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class RopeVerlet : MonoBehaviour
{
    [System.Serializable]
    public struct RopeSegment
    {
        public Vector2 CurrrentPosition;
        public Vector2 OldPosition;
        public float mass;
        public float Tension;

        public RopeSegment(Vector2 pos)
        {
            CurrrentPosition = pos;
            OldPosition = pos;
            mass = 1;
            Tension = 0;
        }
    }

    [Header("Rope")]
    [SerializeField] int numberOfRopeSegment = 50;
    [SerializeField] float segmentLength = 0.225f;

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
    [SerializeField] bool isTailFixed = false;
    [SerializeField] Transform tailFixedTransform = null;

    [Header("Optimizaions")]
    [SerializeField] int _collisionSegmentInterval = 2;

    [SerializeField] Vector3 _ropeStartPoint;

    private LineRenderer _lineRenderer;
    public List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    public Vector3 HeadPosition { get => _ropeSegments[0].CurrrentPosition; } 
    public Vector3 TailPosition { get => _ropeSegments[numberOfRopeSegment - 1].CurrrentPosition; } 

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
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = numberOfRopeSegment;

        Vector3 ropeStartPoint = _ropeStartPoint;
        for (int i = 0; i < numberOfRopeSegment; i++)
        {
            _ropeSegments.Add(new RopeSegment(_ropeStartPoint));
            ropeStartPoint.y -= segmentLength;
        }

    }
    void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[numberOfRopeSegment];
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            ropePositions[i] = _ropeSegments[i].CurrrentPosition;
        }
        _lineRenderer.SetPositions(ropePositions);
    }
    void Simulate()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = (segment.CurrrentPosition - segment.OldPosition) * dampingFactor;

            segment.OldPosition = segment.CurrrentPosition;
            segment.CurrrentPosition += velocity;
            segment.CurrrentPosition += gravityForce * Time.fixedDeltaTime;
            _ropeSegments[i] = segment;
        }
    }
    void Constraints()
    {
        //fixed the positions
        if (isHeadFixed)
        {
            RopeSegment refHead = _ropeSegments[0];
            refHead.CurrrentPosition = headFixedTarget.position;
            _ropeSegments[0] = refHead;

        }

        if (isTailFixed)
        {
            int tailIndex = _ropeSegments.Count - 1;

            RopeSegment refTail = _ropeSegments[tailIndex];
            refTail.CurrrentPosition = tailFixedTransform.position;
            _ropeSegments[tailIndex] = refTail;

        }

        //apply correction to each segment
        for (int i = 0; i < numberOfRopeSegment - 1; i++)
        {
            //get the structs pair
            RopeSegment segment_A = _ropeSegments[i];
            RopeSegment segment_B = _ropeSegments[i + 1];

            //calculate stretch
            float dist = (segment_A.CurrrentPosition - segment_B.CurrrentPosition).magnitude;
            float stretch = dist - segmentLength;

            //calculate invmass
            float invMass_A = 1 / segment_A.mass;
            float invMass_B = 1 / segment_B.mass;
            float totalInvMass = invMass_A + invMass_B;

            Vector2 changeDir = (segment_A.CurrrentPosition - segment_B.CurrrentPosition).normalized;
            
            //calculate correction value
            Vector2 totalCorrection = changeDir * stretch;
            Vector2 correction_A = totalCorrection * (invMass_A / totalInvMass);
            Vector2 correction_B = totalCorrection * (invMass_B / totalInvMass);

            //apply correction value
            segment_A.CurrrentPosition -= correction_A;
            segment_B.CurrrentPosition += correction_B;

            int firstSegmentPairIndex = 0;
            if (isHeadFixed && i == firstSegmentPairIndex)
            {
                segment_A.CurrrentPosition += correction_A;
                segment_B.CurrrentPosition += correction_A;
            }

            int lastSegmentPairIndex = numberOfRopeSegment - 2;
            if (isHeadFixed && i == lastSegmentPairIndex)
            {
                segment_A.CurrrentPosition -= correction_B;
                segment_B.CurrrentPosition -= correction_B;
            }

            //calculate tension
            float tension = 0;
            if (stretch > 0f)
            {
                tension = stiffness * stretch; // Or whatever factor you choose
            }

            //apply the tension
            //first
            if (i == firstSegmentPairIndex)
            {
                segment_B.Tension = tension;
            }
            else if (i == lastSegmentPairIndex)
            {
                segment_A.Tension = (segment_A.Tension + tension) /2;
            }
            else
            {
                segment_A.Tension = (segment_A.Tension + tension) / 2;
                segment_B.Tension = tension;
            }

                //Apply back the struct pair
                _ropeSegments[i] = segment_A;
            _ropeSegments[i + 1] = segment_B;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }
    void FixedUpdate()
    {
        Simulate();

        for (int i = 0; i < constrainPerUpdate; i++)
        {
            Constraints();

            if (i % _collisionSegmentInterval == 0)
            {
                HandleCollisions();
            }
        }
    }
    void HandleCollisions()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector2 velocity = segment.CurrrentPosition - segment.OldPosition;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(segment.CurrrentPosition, collisionRadius, collisionMask);

            foreach (Collider2D collider in colliders)
            {
                Vector2 closestPoint = collider.ClosestPoint(segment.CurrrentPosition);
                float distance = Vector2.Distance(segment.CurrrentPosition, closestPoint);

                if (distance < collisionRadius)
                {
                    Vector2 normal = (segment.CurrrentPosition - closestPoint).normalized;
                    if (normal == Vector2.zero)
                    {
                        normal = (segment.CurrrentPosition - (Vector2)collider.transform.position).normalized;

                    }
                    float depth = collisionRadius - distance;
                    segment.CurrrentPosition += normal * depth;

                    velocity = Vector2.Reflect(velocity, normal) * bounceFactor;
                }
            }
            segment.OldPosition = segment.CurrrentPosition - velocity;
            _ropeSegments[i] = segment;
        }
    }

    public float Length()
    {
        float sum = 0;
        for (int i = 0; i < numberOfRopeSegment-1; i++)
        {
            sum += (_ropeSegments[i].CurrrentPosition - _ropeSegments[i + 1].CurrrentPosition).magnitude;
        }
        return sum;
    }

    public float LengthNormal()
    {
        return segmentLength * numberOfRopeSegment;
    }
}