using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
public class MenuManager : MonoBehaviour
{

    public GameObject mainMenuCanvas;  // Reference to the Main Menu Canvas
    public GameObject title;
    public GameObject background;
    void Start()
    {
        PersistentEventSystem eventSystem = PersistentEventSystem.Instance;
        if (SceneManager.GetSceneByName("GameScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("GameScene");
        }
        if (SceneManager.GetSceneByName("RoomScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("RoomScene");
        }
        if (SceneManager.GetSceneByName("OptionSence").isLoaded)
        {
            SceneManager.UnloadSceneAsync("OptionSence");
        }
        // Ensure the main menu is shown at the start
        mainMenuCanvas = GameObject.Find("MainMenu");

        title = GameObject.Find("Title");
        background = GameObject.Find("Backgound");

        AssignButtonEvents();
    }

    void AssignButtonEvents()
    {
        // Main Menu Buttons
        Button playButton = mainMenuCanvas.transform.Find("Play").GetComponent<Button>();
        playButton.onClick.AddListener(OnPlayButtonClicked);

        Button optionsButton = mainMenuCanvas.transform.Find("Options").GetComponent<Button>();
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);

        Button quitButton = mainMenuCanvas.transform.Find("Quit").GetComponent<Button>();
        quitButton.onClick.AddListener(OnQuitButtonClicked);

        // Options Menu Back Button
        Debug.Log("Button events assigned");
    }



    public void OnPlayButtonClicked()
    {
        // Load the game scene, replace "GameScene" with your actual game scene name
        mainMenuCanvas.SetActive(false);

        title.SetActive(false);
        background.SetActive(true);


        SceneManager.LoadScene("RoomScene", LoadSceneMode.Additive);
    }

    public void OnOptionsButtonClicked()
    {
        SceneManager.LoadSceneAsync("OptionScene", LoadSceneMode.Additive).completed += OnSceneLoaded;
    }

    public void OnQuitButtonClicked()
    {
        // Quit the application
        Debug.Log("Quit button clicked");
        Application.Quit();

#if UNITY_EDITOR
        // If in the Unity editor, stop playing the game
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    private void OnSceneLoaded(AsyncOperation operation)
    {
        if (operation.isDone)
        {
            // Start the coroutine to unload old scenes
            StartCoroutine(UnloadOldScenes());
        }
        else
        {
            Debug.LogError("Failed to load GameScene");
        }
    }

    private IEnumerator UnloadOldScenes()
    {
        // Wait until the new scene is fully loaded
        while (!SceneManager.GetSceneByName("OptionScene").isLoaded)
        {
            // Debug.Log("Waiting for GameScene to load...");
            yield return new WaitForSeconds(0.5f);
        }

        // Unload the old scenes
        // Debug.Log("Unloading old scenes...");
        AsyncOperation unloadMainMenu = SceneManager.UnloadSceneAsync("MainMenu");

        while (!unloadMainMenu.isDone)
        {
            yield return null;
        }

        // Debug.Log("Old scenes unloaded successfully");
    }
}
