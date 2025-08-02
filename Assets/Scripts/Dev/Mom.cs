using UnityEngine;

public class Mom : Grabable
{
    [SerializeField] RopeVerlet rope;
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
}
