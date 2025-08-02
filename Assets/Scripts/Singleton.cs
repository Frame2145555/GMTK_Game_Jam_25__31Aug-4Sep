using UnityEngine;
using System;

// https://gist.github.com/mstevenson/4325117
public class Singleton<T> : MonoBehaviour
    where T : Component
{
    private static T _instance;

    [Obsolete("This is Obsolate Class")]
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType<T>();
                if (objs.Length > 0)
                {
                    _instance = objs[0];
                }
                if (objs.Length > 1)
                {
                    Debug.LogError($"There are more than on {typeof(T).Name} in scene.");
                }
                if (_instance == null)
                {
                    GameObject gob = new GameObject();
                    gob.hideFlags = HideFlags.HideAndDontSave;
                    _instance = gob.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}


public class SingletonPersistent<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
