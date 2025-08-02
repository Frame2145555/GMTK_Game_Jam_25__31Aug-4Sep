using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FrameTestSceneControllor : MonoBehaviour
{
    [SerializeField] private Animator transitionAnim;

    private void Start()
    {
        transitionAnim = GameObject.Find("SceneTransition/Canvas/Black").GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NextScene();
        }
    }

    private void NextScene()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        transitionAnim.SetTrigger("End");
        yield return new WaitForSeconds(1);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
