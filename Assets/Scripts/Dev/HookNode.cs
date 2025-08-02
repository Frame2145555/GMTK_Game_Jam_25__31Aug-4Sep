using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

public class HookNode : MonoBehaviour
{
    public class Checker
    {
        public Vector2 Position;
        public float Radius;
        public bool containRopeSegment;
            
        public Checker(Vector2 position,float radius)
        {
            Position = position;
            Radius = radius;
            containRopeSegment = false;
        }

        public bool Contain(Vector2 point)
        {
            return (Position - point).magnitude < Radius;
        }
    }

    [Header("Reference")]
    [SerializeField] RopeVerlet rope;
    Mom mom;
    RopeHead ropeHead;

    [Header("Node")]
    [SerializeField] float nodeRadius = 1;
    bool haveRope = false;

    [Header("Checker")]
    [SerializeField] float checkerRadius = 0.1f;
    [SerializeField] float passFactor = 0.8f;
    [SerializeField] int checkerCount = 8;
    [SerializeField] int ropeSegmentSubtract = 10;
    [SerializeField] float ropeLengthSubtract = 2;

    [SerializeField] List<Checker> checkers = new List<Checker>();

    private void Awake()
    {
        for (int i = 0; i < checkerCount; i++)
        {
            float x = Mathf.Cos(2 * Mathf.PI / checkerCount * i) * nodeRadius;
            float y = Mathf.Sin(2 * Mathf.PI / checkerCount * i) * nodeRadius;
            Vector3 offset = new(x, y, 0);
            
            checkers.Add(new Checker(transform.position + offset, checkerRadius));
        }

    }

    private void Start()
    {

        mom = FindFirstObjectByType<Mom>();
        ropeHead = FindFirstObjectByType<RopeHead>();

        mom.onRelease += Release;
    }

    private void Update()
    {
        //look for ropePoint in radius
        LookForRopePoints();

        if (rope.intersection.Count == 0) return;

        bool intersectionInRange = false;
        foreach (Checker checker in checkers)
        {
            foreach (var intersection in rope.intersection)
            {
                if (checker.Contain(intersection.Point))
                {
                    intersectionInRange = true;
                    break;
                }
            }
            if (intersectionInRange) break;
        }

        if (!intersectionInRange) return;

        int checkerContainPointCount = 0;
        foreach (var checker in checkers)
        {
            if (checker.containRopeSegment)
                checkerContainPointCount++;
        }

        if (checkerContainPointCount > checkerCount * passFactor && !haveRope)
        {
            Grab();
        }
    }

    private void Grab()
    {
        rope.DetachHead();
        rope.FixedHeadTo(gameObject);
        rope.RemoveSegments(ropeSegmentSubtract);
        rope.MaxLength -= ropeLengthSubtract;
        haveRope = true;
        mom.CanBeGrab = true;
        mom.grabbingNodeTransform = transform;
        ropeHead.CanBeGrab = false;
    }

    public void Release()
    {
        rope.DetachHead();
        rope.FixedHeadTo(ropeHead.gameObject);
        rope.AddSegments(ropeSegmentSubtract);
        rope.MaxLength += ropeLengthSubtract;
        haveRope = false;
        mom.CanBeGrab = false;
        ropeHead.CanBeGrab = true;

    }
    private void LookForRopePoints()
    {
        foreach (var checker in checkers)
        {
            checker.containRopeSegment = false;

            foreach (var point in rope.ropePoints)
            {
                if (checker.Contain(point.CurrentPosition))
                {
                    checker.containRopeSegment = true;
                    break;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < checkerCount; i++)
        {
            float x = Mathf.Cos(2 * Mathf.PI / checkerCount * i) * nodeRadius;
            float y = Mathf.Sin(2 * Mathf.PI / checkerCount * i) * nodeRadius;
            Vector3 offset = new(x,y,0);
            if (checkers.Count > 0)
            {
                if (checkers[i].containRopeSegment)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.green;
                }
            }
            Gizmos.DrawWireSphere(transform.position + offset, checkerRadius);
        }
    }
}
