using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] RopeVerlet rope;

    [SerializeField] Transform headAttachment;
    [SerializeField] Transform tailAttachment;

    private void Start()
    {
        rope.FixedHeadTo(headAttachment.gameObject);
        rope.FixedTailTo(tailAttachment.gameObject);
        
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
        {
        }

    }
}
