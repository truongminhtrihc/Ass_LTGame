using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public InputField roomIDInput;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Text playerListText;
    public Button startButton;
    private Dictionary<string, List<ulong>> rooms = new Dictionary<string, List<ulong>>();
    private string currentRoomID;
    void Start(){
        roomIDInput = GameObject.Find("InputField").GetComponent<InputField>();
        createRoomButton = GameObject.Find("CreateRoomId").GetComponent<Button>();
        joinRoomButton = GameObject.Find("Join").GetComponent<Button>();
        playerListText = GameObject.Find("Users").GetComponent<Text>();
        startButton = GameObject.Find("Start").GetComponent<Button>();
        startButton.gameObject.SetActive(true);
        startButton.onClick.AddListener(startGameTmp);
    }
    void startGameTmp(){
        SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("RoomScene");
        SceneManager.UnloadSceneAsync("MainMenu");
    }
    void StartTmp()
    {
        roomIDInput = GameObject.Find("InputField").GetComponent<InputField>();
        createRoomButton = GameObject.Find("CreateRoomId").GetComponent<Button>();
        joinRoomButton = GameObject.Find("Join").GetComponent<Button>();
        playerListText = GameObject.Find("Users").GetComponent<Text>();
        startButton = GameObject.Find("Start").GetComponent<Button>();

        // Hide the start button for clients
        if (!NetworkManager.Singleton.IsServer)
        {
            startButton.gameObject.SetActive(false);
        }

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);

        if (NetworkManager.Singleton.IsServer)
        {
            startButton.gameObject.SetActive(true);
            startButton.onClick.AddListener(StartGame);
        }
    }

    void CreateRoom()
    {
        currentRoomID = GenerateRoomID();
        rooms[currentRoomID] = new List<ulong>();
        roomIDInput.text = currentRoomID;
        UpdatePlayerList();
    }

    void JoinRoom()
    {
        string roomID = roomIDInput.text;
        if (rooms.ContainsKey(roomID))
        {
            currentRoomID = roomID;
            rooms[roomID].Add(NetworkManager.Singleton.LocalClientId);
            UpdatePlayerList();
        }
        else
        {
            Debug.LogError("Room ID not found!");
        }
    }

    string GenerateRoomID()
    {
        return Random.Range(1000, 9999).ToString(); // Simple room ID generation, you can use a more complex system if needed
    }

    void UpdatePlayerList()
    {
        playerListText.text = "Players in Room " + currentRoomID + ":\n";
        foreach (var clientId in rooms[currentRoomID])
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();
            if (playerObject != null)
            {
                playerListText.text += playerObject.playerName + "\n";
            }
        }

        if (rooms[currentRoomID].Count == 4)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }
}
