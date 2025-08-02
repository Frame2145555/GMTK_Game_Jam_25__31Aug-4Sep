using UnityEngine;

public class LeverDirection : MonoBehaviour
{
    [Header("Rotation Limits")]
    public float minAngle = -45f;
    public float maxAngle = 45f;

    [Header("Config")]
    public float rotateSpeed = 100f;

    private float currentAngle = 0f;


    void Update()
    {
        float input = Input.GetAxis("Horizontal"); // Left/Right Arrow or A/D
        currentAngle += input * rotateSpeed * Time.deltaTime;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        CheckLeverLimit();
    }
     void CheckLeverLimit()
    {
        if (Mathf.Approximately(currentAngle, maxAngle))
        {
            Debug.Log("Lever reached max angle!");
            // Place logic here for when lever reaches max angle
        }
        else if (Mathf.Approximately(currentAngle, minAngle))
        {
            Debug.Log("Lever reached min angle!");
            // Place logic here for when lever reaches min angle
        }
    }
}

