using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public InputField roomIDInput;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Text currentRoomText;
    public Button startButton;
    public Text user0, user1, user2, user3;
    private string currentRoomID;
    private PhotonView photonView;
    private Coroutine reloadUserStatusCoroutine;

    private void Start()
    {
        ConnectToServer();
        EnsureSingleAudioListener();
        InitializeUI();
    }

    private void InitializeUI()
    {
        photonView = GetComponent<PhotonView>();
        currentRoomText = GameObject.Find("CurrentRoomText").GetComponent<Text>();
        roomIDInput = GameObject.Find("InputField").GetComponent<InputField>();
        startButton = GameObject.Find("Start").GetComponent<Button>();
        user0 = GameObject.Find("User0").GetComponent<Text>();
        user1 = GameObject.Find("User1").GetComponent<Text>();
        user2 = GameObject.Find("User2").GetComponent<Text>();
        user3 = GameObject.Find("User3").GetComponent<Text>();
        createRoomButton = GameObject.Find("CreateRoomId").GetComponent<Button>();
        joinRoomButton = GameObject.Find("Join").GetComponent<Button>();

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        startButton.onClick.AddListener(OnStartGameButtonClicked);

        startButton.gameObject.SetActive(false);
    }

    private void ConnectToServer()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }

    private void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomIDInput.text))
        {
            Debug.LogWarning("Room ID cannot be empty!");
            return;
        }

        currentRoomID = System.Text.RegularExpressions.Regex.Replace(roomIDInput.text, @"[^0-9]", "");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 }; // Adjust max players as needed
        PhotonNetwork.CreateRoom(currentRoomID, roomOptions, null);
        ToggleRoomButtons(false);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        currentRoomText.text = $"Current Room: {currentRoomID}";
        UpdatePlayerList();
    }

    private void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomIDInput.text))
        {
            Debug.LogWarning("Room ID cannot be empty!");
            return;
        }

        PhotonNetwork.JoinRoom(roomIDInput.text);
        ToggleRoomButtons(false);
    }

    private void ToggleRoomButtons(bool active)
    {
        createRoomButton.gameObject.SetActive(active);
        joinRoomButton.gameObject.SetActive(active);
    }

    private IEnumerator ReloadUserStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // Wait for 5 seconds
            UpdatePlayerList();
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        currentRoomID = PhotonNetwork.CurrentRoom.Name;
        AssignPlayerName();
        currentRoomText.text = $"Current Room: {currentRoomID}";

        if (reloadUserStatusCoroutine != null)
        {
            StopCoroutine(reloadUserStatusCoroutine);
        }
        reloadUserStatusCoroutine = StartCoroutine(ReloadUserStatus());

        UpdatePlayerList();
    }

    private void AssignPlayerName()
    {
        string playerName = $"Player{PhotonNetwork.LocalPlayer.ActorNumber:00}";
        PhotonNetwork.NickName = playerName;
    }

    private void UpdatePlayerList()
    {
        user0.text = "";
        user1.text = "";
        user2.text = "";
        user3.text = "";

        List<Photon.Realtime.Player> players = new List<Photon.Realtime.Player>(PhotonNetwork.CurrentRoom.Players.Values);
        startButton.gameObject.SetActive(players.Count > 1);

        for (int index = 0; index < players.Count; index++)
        {
            Text userText = index switch
            {
                0 => user0,
                1 => user1,
                2 => user2,
                3 => user3,
                _ => null
            };

            if (userText != null)
            {
                userText.text = players[index].NickName;
            }
        }

        int localPlayerIndex = players.FindIndex(p => p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        if (localPlayerIndex >= 0)
        {
            Text localUserText = localPlayerIndex switch
            {
                0 => user0,
                1 => user1,
                2 => user2,
                3 => user3,
                _ => null
            };

            if (localUserText != null)
            {
                localUserText.text += " <- You";
            }
        }
    }

    public void OnStartGameButtonClicked()
    {
        photonView.RPC("StartGame", RpcTarget.All);
    }

    [PunRPC]
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting the game...");
            PhotonNetwork.LoadLevel("GameScene");
        }
        else{
            Debug.Log("You are not the master client.");
        }
    }

    private void EnsureSingleAudioListener()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
        {
            for (int i = 1; i < listeners.Length; i++)
            {
                listeners[i].enabled = false;
            }
        }
        else if (listeners.Length == 0)
        {
            Debug.LogWarning("No AudioListener found. Adding one.");
            gameObject.AddComponent<AudioListener>();
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"{otherPlayer.NickName} left the room.");
        UpdatePlayerList();
    }

    private void OnDestroy()
    {
        if (reloadUserStatusCoroutine != null)
        {
            StopCoroutine(reloadUserStatusCoroutine);
        }
    }
}
