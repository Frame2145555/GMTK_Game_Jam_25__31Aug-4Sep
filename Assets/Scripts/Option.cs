using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Option : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] Button b_continue, b_respawn, b_backToMainMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        b_continue.onClick.AddListener(() =>
        {
            menu.SetActive(false);
        });
        b_respawn.onClick.AddListener(() => Respawn());
        b_backToMainMenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
    void Respawn()
    {
        SceneResetter resetter = FindFirstObjectByType<SceneResetter>();
        StartCoroutine(resetter.ReloadScene());
    }
}
