using UnityEngine;
using UnityEngine.InputSystem;
public class Mum : MonoBehaviour
{
    [SerializeField] Transform pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.position = new Vector2(mousePos.x, mousePos.y);
    }
}
