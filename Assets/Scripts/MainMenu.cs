using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Button b_Start, b_Option, b_Exit;
    [SerializeField] GameObject optionMenu, creditMenu;
    [SerializeField] Slider s_Sound, s_Music;
    [SerializeField] TMP_Text t_Sound, t_Music;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        t_Sound.text = $"{SoundController.Instance.SoundValue} %";
        t_Music.text = $"{SoundController.Instance.MusicValue} %";

        s_Sound.value = SoundController.Instance.SoundValue;
        s_Music.value = SoundController.Instance.MusicValue;
        SetActiveObject(optionMenu,false);
        SetActiveObject(creditMenu,false);

        b_Start.onClick.AddListener(() => NextScene());
        b_Option.onClick.AddListener(() => SetActiveObject(optionMenu,true));
        b_Exit.onClick.AddListener(() => Exit());
        s_Sound.onValueChanged.AddListener((v) =>
        {
            SoundController.Instance.SoundValue = v;
            t_Sound.text = $"{SoundController.Instance.SoundValue} %";
        });
        s_Music.onValueChanged.AddListener((v) =>
        {
            SoundController.Instance.MusicValue = v;
            t_Music.text = $"{SoundController.Instance.MusicValue} %";
        });
    }
    void NextScene()
    {
        Debug.Log("ChangeScene");
    }
    void SetActiveObject(GameObject o_value, bool value)
    {
        Debug.Log("SetActive Option Menu");
        o_value.SetActive(value);
    }
    public void Exit()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
    void Update()
    {
        
    }
}
