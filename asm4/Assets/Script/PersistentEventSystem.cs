using UnityEngine;
using UnityEngine.EventSystems;

public class PersistentEventSystem : MonoBehaviour
{
    private static PersistentEventSystem _instance;

    public static PersistentEventSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("PersistentEventSystem");
                _instance = obj.AddComponent<PersistentEventSystem>();
                DontDestroyOnLoad(obj);
                _instance.InitializeEventSystem();
            }
            return _instance;
        }
    }

    private EventSystem eventSystem;

    private void InitializeEventSystem()
    {
        eventSystem = gameObject.AddComponent<EventSystem>();
        gameObject.AddComponent<StandaloneInputModule>();
        // You can add other input modules as needed
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
    }
}
