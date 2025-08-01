
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class RopeVerlet : MonoBehaviour
{
    public struct RopeSegment
    {
        public Vector2 CurrrentPosition;
        public Vector2 OldPosition;

        public RopeSegment(Vector2 pos)
        {
            CurrrentPosition = pos;
            OldPosition = pos;
        }
    }

    [Header("Rope")]
    [SerializeField] int _numOfRopeSegments = 50;
    [SerializeField] float _ropeSegmentLength = 0.225f;

    [Header("Physics")]
    [SerializeField] Vector2 _gravityForce = new Vector2(0f, -2f);
    [SerializeField] float _dampingFactor = 0.98f;
    [SerializeField] LayerMask _collisionMask;
    [SerializeField] float _collisionRadius = 0.1f;
    [SerializeField] float _bounceFactor = 0.1f;
    [SerializeField] float _correctionClampAmount = 0.1f;

    [Header("Constraints")]
    [SerializeField] int _numOfConstraintRuns = 50;

    [Header("Optimizaions")]
    [SerializeField] int _collisionSegmentInterval = 2;

    [Header("Config")]
    [SerializeField] Vector3 headFollowTarget;
    [SerializeField] Vector3 tailFollowTarget;
    [SerializeField] Vector3 _ropeStartPoint;

    private LineRenderer _lineRenderer;
    public List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    public Vector3 HeadFollowTarget { set => headFollowTarget = value; }
    public Vector3 TailFollowTarget { set => tailFollowTarget = value; }
    public Vector3 HeadPosition { get => _ropeSegments[0].CurrrentPosition; } 
    public Vector3 TailPosition { get => _ropeSegments[_numOfRopeSegments - 1].CurrrentPosition; } 


    void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _numOfRopeSegments;

        Vector3 ropeStartPoint = _ropeStartPoint;
        for (int i = 0; i < _numOfRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(_ropeStartPoint));
            ropeStartPoint.y -= _ropeSegmentLength;
        }

    }
    void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[_numOfRopeSegments];
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
            Vector2 velocity = (segment.CurrrentPosition - segment.OldPosition) * _dampingFactor;

            segment.OldPosition = segment.CurrrentPosition;
            segment.CurrrentPosition += velocity;
            segment.CurrrentPosition += _gravityForce * Time.fixedDeltaTime;
            _ropeSegments[i] = segment;
        }
    }
    void ApplyConstraints()
    {
        RopeSegment firstSegment = _ropeSegments[0];
        firstSegment.CurrrentPosition = headFollowTarget;
        _ropeSegments[0] = firstSegment;

        RopeSegment lastSegment = _ropeSegments[_numOfRopeSegments - 1];
        lastSegment.CurrrentPosition = tailFollowTarget;
        _ropeSegments[_numOfRopeSegments - 1] = lastSegment;

        for (int i = 0; i < _numOfRopeSegments - 1; i++)
        {
            RopeSegment currentSeg = _ropeSegments[i];
            RopeSegment nextSeg = _ropeSegments[i + 1];

            float dist = (currentSeg.CurrrentPosition - nextSeg.CurrrentPosition).magnitude;
            float difference = (dist - _ropeSegmentLength);

            Vector2 changeDIr = (currentSeg.CurrrentPosition - nextSeg.CurrrentPosition).normalized;
            Vector2 changeVector = changeDIr * difference;

            if (i != 0)
            {
                currentSeg.CurrrentPosition -= (changeVector * 0.5f);
                nextSeg.CurrrentPosition += (changeVector * 0.5f);

            }
            else
            {
                nextSeg.CurrrentPosition += changeVector;
            }
            _ropeSegments[i] = currentSeg;
            _ropeSegments[i + 1] = nextSeg;
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

        for (int i = 0; i < _numOfConstraintRuns; i++)
        {
            ApplyConstraints();

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
            float distance = velocity.magnitude;

            if (distance > 0.0001f)
            {
                RaycastHit2D hit = Physics2D.CircleCast(
                    segment.OldPosition,
                    _collisionRadius,
                    velocity.normalized,
                    distance,
                    _collisionMask
                );

                if (hit.collider != null)
                {
                    Vector2 normal = hit.normal;
                    Vector2 point = hit.point;

                    // Offset the segment slightly out of collision
                    segment.CurrrentPosition = point + normal * _collisionRadius;

                    // Reflect the velocity
                    velocity = Vector2.Reflect(velocity, normal) * _bounceFactor;
                }
            }

            segment.OldPosition = segment.CurrrentPosition - velocity;
            _ropeSegments[i] = segment;
        }
    }


    public float Length()
    {
        float sum = 0;
        for (int i = 0; i < _numOfRopeSegments-1; i++)
        {
            sum += (_ropeSegments[i].CurrrentPosition - _ropeSegments[i + 1].CurrrentPosition).magnitude;
        }
        return sum;
    }

    public float LengthNormal()
    {
        return _ropeSegmentLength * _numOfRopeSegments;
    }
}