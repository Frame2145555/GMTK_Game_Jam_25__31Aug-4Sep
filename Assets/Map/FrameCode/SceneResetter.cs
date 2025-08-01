using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneResetter : MonoBehaviour
{
    [SerializeField] private Animator transitionAnim;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
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

