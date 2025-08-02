using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] GameObject anchor;
    [SerializeField] GameObject segPrefab;
    List<GameObject> segs = new List<GameObject>();

    [SerializeField] float segLength = 0.1f;
    [SerializeField] int batch = 10;

    [ContextMenu("Make Batch")] 
    void MakeRope()
    {
        for(int i = 0; i < batch; i++)
        {
            AddSegment();
        }
    }

    [ContextMenu("Add Segment")]
    void AddSegment()
    {
        GameObject newSeg = null;
        HingeJoint2D hingeJ = null;
        DistanceJoint2D lenJ = null;
        if (segs.Count <= 0)
        {
            newSeg = Instantiate(segPrefab, transform,true);
            newSeg.transform.position = anchor.transform.position;
            newSeg.transform.rotation = anchor.transform.rotation;
            hingeJ = newSeg.GetComponent<HingeJoint2D>();
            hingeJ.connectedBody = anchor.GetComponent<Rigidbody2D>();
            hingeJ.autoConfigureConnectedAnchor = false;
            lenJ = newSeg.GetComponent<DistanceJoint2D>();
            lenJ.connectedBody = anchor.GetComponent<Rigidbody2D>();
        }
        else
        {
            GameObject tail = segs[segs.Count - 1];
            newSeg = Instantiate(segPrefab,tail.transform,true);
            newSeg.transform.parent = transform;
            newSeg.transform.localScale = segPrefab.transform.localScale;
            newSeg.transform.position = tail.transform.position - (tail.transform.up.normalized * segLength);
            hingeJ = newSeg.GetComponent<HingeJoint2D>();
            hingeJ.connectedBody = tail.GetComponent<Rigidbody2D>();
            lenJ = newSeg.GetComponent<DistanceJoint2D>();
            lenJ.connectedBody = tail.GetComponent<Rigidbody2D>();

        }
        lenJ.distance = segLength;
        newSeg.transform.SetParent(transform,false);
        segs.Add(newSeg);
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        }
    }
}
