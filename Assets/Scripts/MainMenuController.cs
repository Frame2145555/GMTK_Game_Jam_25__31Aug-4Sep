using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] Button b_Start, b_Option, b_Exit;
    [SerializeField] GameObject optionMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        b_Start.onClick.AddListener(() => NextScene());
        b_Option.onClick.AddListener(() => SetActiveOption(true));
        b_Exit.onClick.AddListener(() => Exit());
    }
    void NextScene()
    {
        Debug.Log("ChangeScene");
    }
    void SetActiveOption(bool value)
    {
        Debug.Log("SetActive Option Menu");
        optionMenu.SetActive(value);
    }
    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}
