using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] RopeVerlet rope;

    [SerializeField] Transform headAttachment;
    [SerializeField] Transform tailAttachment;

    void Update()
    {
        rope.FixedHeadTo(headAttachment);

        rope.FixedTailTo(tailAttachment);


    }
}
