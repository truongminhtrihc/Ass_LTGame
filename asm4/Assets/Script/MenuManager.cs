using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{   

    public GameObject mainMenuCanvas;  // Reference to the Main Menu Canvas
    public GameObject optionsMenuCanvas;  // Reference to the Options Menu Canvas
    public GameObject title;
    public GameObject roomScreenCanvas;
    public GameObject background;
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
        // Ensure the main menu is shown at the start
        mainMenuCanvas = GameObject.Find("MainMenu");
        optionsMenuCanvas = GameObject.Find("OptionMenu");
        title = GameObject.Find("Title");
        background = GameObject.Find("Backgound");
        
        ShowMainMenu();
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
        Button backButton = optionsMenuCanvas.transform.Find("Back").GetComponent<Button>();
        backButton.onClick.AddListener(OnBackButtonClicked);
        Debug.Log("Button events assigned");
    }

    public void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        optionsMenuCanvas.SetActive(false);
    }

    public void ShowOptionsMenu()
    {
        mainMenuCanvas.SetActive(false);
        optionsMenuCanvas.SetActive(true);
    }

    public void OnPlayButtonClicked()
    {
        // Load the game scene, replace "GameScene" with your actual game scene name
        mainMenuCanvas.SetActive(false);
        optionsMenuCanvas.SetActive(false);
        
        title.SetActive(false);
        background.SetActive(true);

        
        SceneManager.LoadScene("RoomScene", LoadSceneMode.Additive);
    }

    public void OnOptionsButtonClicked()
    {
        ShowOptionsMenu();
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

    public void OnBackButtonClicked()
    {
        ShowMainMenu();
    }
}
