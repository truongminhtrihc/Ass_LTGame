using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
public class OptionMenu : MonoBehaviour
{  // Reference to the Options Menu Canvas
     public Button backButton;
    void Start()
    {
        if (SceneManager.GetSceneByName("GameScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("GameScene");
        }
        if (SceneManager.GetSceneByName("RoomScene").isLoaded)
        {
            SceneManager.UnloadSceneAsync("RoomScene");
        }
        if (SceneManager.GetSceneByName("MainMenu").isLoaded)
        {
            SceneManager.UnloadSceneAsync("MainMenu");
        }

        backButton = GameObject.Find("Back").GetComponent<Button>();
        backButton.onClick.AddListener(OnBackButtonClicked);
        Debug.Log("OptionMenu loaded");
    }
     
    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked");
        SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive).completed += OnSceneLoaded;
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
        AsyncOperation unloadOptionscene = SceneManager.UnloadSceneAsync("OptionScene");
        
        while (!unloadOptionscene.isDone)
        {
            yield return null;
        }

        
    }
}