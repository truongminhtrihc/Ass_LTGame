using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public List<Player> players = new List<Player>();
    public DiceRoller diceRoller1;
    public DiceRoller diceRoller2;
    public Route route;  // Reference to the Route
    private int currentPlayerIndex;
    private bool isPlayerMoving = false;
    private bool isWaitingForUserDecision = false;
    private int doubleCount = 0;
    private PhotonView photonView;
    private bool isCurrentPlayerTurn = false;
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            // Debug.LogError("PhotonView component missing from GameObject");
        }
    }

    void Start()
    {

        currentPlayerIndex = 0;

        if (!PhotonNetwork.IsConnected)
        {
            // Debug.LogError("PhotonNetwork is not connected!");
            // Debug.Log("Quit button clicked");
            Application.Quit();

#if UNITY_EDITOR
        // If in the Unity editor, stop playing the game
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            // Debug.Log("Connected to Photon Network");
        }
        // init 
        InitializePlayers();
        // Start the game
        StartTurn();
    }

    private void InitializePlayers()
    {
        List<Transform> pathNodes = route.GetPathNodes();
        Vector3 initialPosition = pathNodes[0].position;
        // Debug.Log("Initial position: " + initialPosition);
        initialPosition.z = -1;
        int index = 0;
        // Sort PhotonNetwork.PlayerList by Name
        Array.Sort(PhotonNetwork.PlayerList, (x, y) => string.Compare(x.NickName, y.NickName, StringComparison.Ordinal));
        // Instantiate players for each Photon player
        foreach (var photonPlayer in PhotonNetwork.PlayerList)
        {
            // Debug.Log("Player: " + photonPlayer.NickName);
            GameObject playerObject = PhotonNetwork.Instantiate("PlayerPrefab", initialPosition, Quaternion.identity);
            Player player = playerObject.GetComponent<Player>();
            player.playerName = photonPlayer.NickName;
            player.money = 1500;

            // Load and assign the player sprite
            string spriteName = index switch
            {
                0 => "Player_1",
                1 => "Player_2",
                2 => "Player_3",
                3 => "Player_4",
                _ => "Player_1"
            };
            AssignPlayerSprite(playerObject, spriteName);
            players.Add(player);
            index++;
        }
    }
    private void AssignPlayerSprite(GameObject playerObject, string spriteName)
    {
        // Define the sprite path based on player name
        string spritePath = "Sprites/" + spriteName; // Assuming sprites are named after player names
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);

        if (sprites != null && sprites.Length > 0)
        {
            // Assign the first sprite to the SpriteRenderer
            SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
            }
            spriteRenderer.sprite = sprites[0];

            // Store the sprites in the player object if needed
            Player player = playerObject.GetComponent<Player>();
            player.playerIcons = sprites; // Assuming playerIcons is an array or list in your Player class
            float scaleFactor = 9.0f;
            playerObject.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1); // Scale the player object
            // Debug.Log("Player sprite assigned: " + spriteName);
        }
        else
        {
            // Debug.LogError("No sprites found at path: " + spritePath);
        }
    }

    void Update()
    {
        // Handle player input for rolling the dice
        if (CheckCurrentTurn() && !isPlayerMoving && !isWaitingForUserDecision && Input.GetMouseButtonDown(0))
        {

            StartTurn();
            StartCoroutine(HandleDiceRoll());
        }
    }

    void StartTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];
    }



    private IEnumerator HandleDiceRoll()
    {
        // Determine dice rolls on the current player's client
        int steps1 = UnityEngine.Random.Range(1, 7);
        int steps2 = UnityEngine.Random.Range(1, 7);

        // Share the dice results with all players
        photonView.RPC("ReceiveDiceResults", RpcTarget.All, steps1, steps2);
        // yield return new WaitUntil(() => !PhotonNetwork.IsMessageQueueRunning);
        yield return null; // Coroutine must yield
    }




    [PunRPC]
    private void ReceiveDiceResults(int steps1, int steps2)
    {
        StartCoroutine(RollAndWait(diceRoller1, steps1));
        StartCoroutine(RollAndWait(diceRoller2, steps2));
        StartCoroutine(ProcessDiceResults(steps1, steps2));
    }

    private IEnumerator RollAndWait(DiceRoller diceRoller, int predeterminedSide)
    {
        // Roll the dice with the predetermined result
        yield return StartCoroutine(diceRoller.RollTheDice(predeterminedSide));

        // Ensure the dice roller has finished rolling
        yield return new WaitUntil(() => !diceRoller.IsRolling());
    }

    private IEnumerator ProcessDiceResults(int steps1, int steps2)
    {
        // Debug.Log("Dice results received: " + steps1 + ", " + steps2);
        int totalSteps = steps1 + steps2;

        if (steps1 == steps2)
        {
            doubleCount++;
            
            // Debug.Log("Player rolled a double! They get to roll again.");
        }
        else
        {
            doubleCount = 0;
        }
        photonView.RPC("SyncDoubleCount", RpcTarget.All, doubleCount);
        yield return StartCoroutine(MovePlayer(players[currentPlayerIndex], totalSteps));
    }
    [PunRPC]
    private void SyncDoubleCount(int count)
    {
        doubleCount = count;
    }
    private GameObject CreateButton(string buttonText, Vector2 position, Color buttonColor, System.Action onClickAction)
    {
        // Create button GameObject
        GameObject button = new GameObject(buttonText + "Button");

        // Add RectTransform component and set its position
        RectTransform rectTransform = button.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(50, 20); // Set size of the button
        rectTransform.anchoredPosition = position; // Set the desired position

        // Add Image component to the button and set its color
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = buttonColor; // Set the desired color

        // Add Button component and set its target graphic
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonImage; // Set the target graphic to the Image component

        // Create and configure the Text component
        GameObject textObject = new GameObject(buttonText + "Text");
        textObject.transform.SetParent(button.transform);
        Text buttonTextComponent = textObject.AddComponent<Text>();
        buttonTextComponent.text = buttonText;
        buttonTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonTextComponent.alignment = TextAnchor.MiddleCenter;
        buttonTextComponent.color = Color.black; // Set text color

        // Set the RectTransform of the Text component to fill the button
        RectTransform textRectTransform = buttonTextComponent.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;

        // Add the onClick listener
        buttonComponent.onClick.AddListener(() => onClickAction());

        return button;
    }
    private IEnumerator GetUserDecision(string message, System.Action<bool> callback)
    {
        Debug.Log("GetUserDecision: checkhere");
        if (!CheckCurrentTurn())
        {
            yield break;
        }

        isWaitingForUserDecision = true;
        photonView.RPC("SyncWatingForUserDecision", RpcTarget.All, isWaitingForUserDecision);
        // Show a pop-up to let the user decide
        bool decisionMade = false;
        bool userDecision = false;
        // Create a UI panel for the decision
        GameObject decisionPanel = new GameObject("DecisionPanel");
        Canvas canvas = decisionPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler canvasScaler = decisionPanel.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        decisionPanel.AddComponent<GraphicRaycaster>();

        // Set the position of the Canvas
        RectTransform rectTransform = decisionPanel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 6);

        // Create message text
        GameObject messageTextObject = new GameObject("MessageText");
        messageTextObject.transform.SetParent(decisionPanel.transform);
        Text messageText = messageTextObject.AddComponent<Text>();
        messageText.text = message;
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.color = Color.black; // Set text color

        // Set the RectTransform of the message text to be centered
        RectTransform messageTextRect = messageText.GetComponent<RectTransform>();
        messageTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageTextRect.anchoredPosition = new Vector2(10, 10); // Set position of the text box
        messageTextRect.sizeDelta = new Vector2(300, 50); // Set size of the text box

        // Create Yes button
        GameObject yesButton = CreateButton("Yes", new Vector2(-30, -15), Color.green, () => { userDecision = true; decisionMade = true; });
        yesButton.transform.SetParent(decisionPanel.transform);

        // Create No button
        GameObject noButton = CreateButton("No", new Vector2(60, -15), Color.red, () => { userDecision = false; decisionMade = true; });
        noButton.transform.SetParent(decisionPanel.transform);

        // Ensure there is an EventSystem in the scene
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
        // Wait until the user makes a decision
        while (!decisionMade)
        {
            yield return null;
        }
        // Destroy the decision panel
        Destroy(decisionPanel);

        isWaitingForUserDecision = false; // Allow other actions after the user has made a decision
        photonView.RPC("SyncWatingForUserDecision", RpcTarget.All, isWaitingForUserDecision);
        callback(userDecision);
    }

    private IEnumerator MovePlayer(Player player, int steps)
    {
        // Debug.Log(player.playerName + " is moving " + steps + " steps.");
        if (doubleCount == 3)
        {
            SendPlayerToJail(player);
            yield break;
        }

        isPlayerMoving = true;  // Prevent other actions while the player is moving

        List<Transform> pathNodes = route.GetPathNodes();  // Get the path nodes from the Route

        for (int i = 0; i < steps; i++)
        {
            player.currentPosition = (player.currentPosition + 1) % pathNodes.Count;  // Move along the path
            Vector3 newPosition = pathNodes[player.currentPosition].position;
            newPosition.z = player.transform.position.z;  // Maintain the player's original Z position
            player.transform.position = newPosition;  // Update the player's position
            yield return new WaitForSeconds(0.5f); // Wait a bit between moves for better visualization
        }

        StartCoroutine(HandlePlayerLanding(player)); // Notify all players
        while(isWaitingForUserDecision == true){
            yield return null;
        }
        isPlayerMoving = false;  // Allow other actions after the player has finished moving

        if (doubleCount > 0)
        {
            // Debug.Log("Double count: " + PhotonNetwork.LocalPlayer.NickName);
            // Wait for the player to click the left mouse button before rolling again
            // Debug.Log("Click the left mouse button to roll again.");
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield break;
        }

        EndTurn();



    }


    void EndTurn()
    {
        if (CheckCurrentTurn())
        {
            Debug.Log("EndTurn: " + PhotonNetwork.LocalPlayer.NickName + " " + players[currentPlayerIndex].playerName);
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            doubleCount = 0;
            photonView.RPC("SyncCurrenplayerIndex", RpcTarget.All, currentPlayerIndex); // Notify all players
            photonView.RPC("SyncDoubleCount", RpcTarget.All, doubleCount); // Notify all players
            StartTurn();
        }

    }

    private void UpdatePlayerState(Player player)
    {
        photonView.RPC("SyncPlayerState", RpcTarget.All, player.playerName, player.money, player.currentPosition);
    }

    [PunRPC]
    private void SyncPlayerState(string playerName, int money, int position)
    {
        // Find the player and update their state
        Player player = players.Find(p => p.playerName == playerName);
        if (player != null)
        {
            player.money = money;
            player.currentPosition = position;
            // Update the player's visual representation if necessary
        }
    }
    private IEnumerator HandlePlayerLanding(Player player)
    {
        // Debug.Log("[+]HandlePlayerLanding: current Player local: " + players[currentPlayerIndex].playerName);
        string nodeType = route.GetNodeType(player.currentPosition);
        // Check what type of node the player landed on
        switch (nodeType)
        {
            case "property":
            case "transportation":
                if (!route.IsNodeOwned(player.currentPosition))
                {
                    // Debug.Log("[+] HandlePlayerLanding:Player local: " + players[currentPlayerIndex].playerName);
                    // Debug.Log("[+] HandlePlayerLanding:CheckCurrent: " + CheckCurrentTurn());

                    // Ask the player if they want to buy the property
                    if ((isWaitingForUserDecision == false) && CheckCurrentTurn() && player.money >= route.GetNodePrice(player.currentPosition))
                    {

                        yield return StartCoroutine(GetUserDecision("Do you want to buy " + route.GetNodeName(player.currentPosition) + " for " + route.GetNodePrice(player.currentPosition) + "?", decision =>
                        {
                            // Debug.Log("Decision: " + decision);
                            if (decision)
                            {
                                route.BuyNode(player.currentPosition, player);
                                // Debug.Log(player.playerName + " has bought " + route.GetNodeName(player.currentPosition) + " for " + route.GetNodePrice(player.currentPosition));
                            }
                        }));
                    }
                    else
                    {
                        // Debug.Log("You don't have enough money to buy " + route.GetNodeName(player.currentPosition));
                    }
                }
                else
                {
                    Player owner = route.GetOwner(player.currentPosition);
                    // Pay rent to the owner
                    PayRent(player, owner, route.GetNodePrice(player.currentPosition));
                }
                break;
            case "tax":
                PayTax(player);
                break;
            case "jail":
                SendPlayerToJail(player);
                break;
            case "util":
                break;
            case "live":
                break;
            case "treasure":
                break;
            case "go":
                player.money += 200;
                break;
            default: // "free"
                break;
        }
        yield return null;
    }
    private void PayRent(Player player, Player owner, int rentAmount)
    {
        while (player.money < rentAmount)
        {
            if (player.propertyList.Count > 0)
            {
                // Sell the first property in the list
                Property propertyToSell = player.propertyList[0];
                player.propertyList.RemoveAt(0);
                player.money += propertyToSell.price;  // Sell the property for half the price
                // Debug.Log(player.playerName + " sold " + propertyToSell.name + " for " + propertyToSell.price);
            }
            else
            {
                // Player is bankrupt
                Bankrupt(player);
                return;
            }
        }
        player.money -= rentAmount;
        owner.money += rentAmount;
        UpdatePlayerState(player);
        UpdatePlayerState(owner);
        // Debug.Log(player.playerName + " paid " + rentAmount + "$ for " + owner.playerName + " in rent.");

    }
    private void PayTax(Player player)
    {
        int taxAmount = 200 + (int)Math.Ceiling(0.1 * (player.money + player.GetValueAllProperties()));
        while (player.money < taxAmount)
        {
            if (player.propertyList.Count > 0)
            {
                // Sell the first property in the list
                Property propertyToSell = player.propertyList[0];
                player.propertyList.RemoveAt(0);
                player.money += propertyToSell.price;  // Sell the property for half the price
                // Debug.Log(player.playerName + " sold " + propertyToSell.name + " for " + propertyToSell.price);
            }
            else
            {
                // Player is bankrupt
                Bankrupt(player);
                return;
            }
        }
        player.money -= taxAmount;
        // Debug.Log(player.playerName + " paid " + taxAmount + "$ " + " in tax.");
    }
    private void Bankrupt(Player player)
    {
        // Debug.Log(player.playerName + " is bankrupt!");
        players.Remove(player);
    }

    private void SendPlayerToJail(Player player)
    {
        // Implement jail logic here
        player.currentPosition = 10;  // Send the player to jail
        player.isInJail = true;
        player.jailTurns = 3;

        // Update the player's position
        List<Transform> pathNodes = route.GetPathNodes();
        Vector3 newPosition = pathNodes[player.currentPosition].position;
        newPosition.z = player.transform.position.z;  // Maintain the player's original Z position
        player.transform.position = newPosition;  // Update the player's position
    }
    [PunRPC]
    void SyncWatingForUserDecision(bool isWaiting)
    {
        isWaitingForUserDecision = isWaiting;
    }
    bool CheckCurrentTurn()
    {
        return PhotonNetwork.LocalPlayer.NickName == players[currentPlayerIndex].playerName;
    }
    [PunRPC]
    private void SyncCurrenplayerIndex(int playerIndex)
    {
        currentPlayerIndex = playerIndex;
    }
}
