using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneResetOnCollision : MonoBehaviour
{
    [SerializeField] private Animator transitionAnim;

    private void Start()
    {
        transitionAnim = GameObject.Find("Scene Transition/Canvas/Black").GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(ReloadScene());
        }
    }

    IEnumerator ReloadScene()
    {
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
