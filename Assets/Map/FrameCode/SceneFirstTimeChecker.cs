using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneFirstTimeChecker : SingletonPersistent<SceneFirstTimeChecker>
{
    private  List<string> visitedScenes = new List<string>();
    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (!visitedScenes.Contains(sceneName))
        {
            Debug.Log($"First time visiting scene: {sceneName} in this session!");
            visitedScenes.Add(sceneName);
        }
        else if (sceneName != "MainMenu" && visitedScenes.Contains(sceneName))
        {
            Debug.Log($"Already visited scene: {sceneName} before in this session.");
            Mom mom = FindFirstObjectByType<Mom>();
            if (!mom)
                return;
            mom.Respawn();
        }
    }

}
