using Unity.VisualScripting;
using UnityEngine;

public class RopeHead : Grabable
{
    [SerializeField] RopeVerlet rope;
    protected override void Start()
    {
        base.Start();
        rope.FixedHeadTo(gameObject);
        transform.position = rope.HeadPosition;

    }
    protected override void Update()
    {
        base.Update();
        if (!isBeingGrab)
            transform.position = rope.HeadPosition;
    }
}
