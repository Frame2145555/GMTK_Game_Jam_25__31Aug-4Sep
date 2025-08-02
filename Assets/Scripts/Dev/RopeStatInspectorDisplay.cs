using UnityEngine;

public class RopeStatInspectorDisplay : MonoBehaviour
{
    [SerializeReference] RopeVerlet rope;

    [Header("Rope Status")]
    public float ropeLength;
    public float ropeLengthNormal;


    void Update()
    {
        if (rope == null) return;

        ropeLength = rope.Length();
        ropeLengthNormal = rope.LengthNormal();
    }
}
