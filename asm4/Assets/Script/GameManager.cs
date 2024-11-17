using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
// using SharpNeat.Phenomes;
// using SharpNeat.Genomes.Neat;
// using SharpNeat.Decoders.Neat;
// using SharpNeat.Core;
// using SharpNeat.Decoders;
using System.Linq;

[System.Serializable]
public class PlayerUIPosition
{
    public Vector2 AnchorMin;
    public Vector2 AnchorMax;
    public Vector2 Pivot;
    public Vector2 AnchoredPosition;
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Player> players;
    public DiceRoller diceRoller1;
    public DiceRoller diceRoller2;
    public Route route;  // Reference to the Route
    public int currentTurn = 0;
    private int currentPlayerIndex;
    private GameObject playerPrefab;
    private bool isPlayerMoving = false;
    private bool isWaitingForUserDecision = false;
    private int doubleCount = 0;
    private bool isGameStopped = false;
    private string[] treasureCards = new string[] { "jail", "live", "free_jail", "money"};
    
    // UI 
    public Canvas playerCanvasPrefab; // Assign a Canvas prefab in the Inspector
    public Transform uiParent; // Assign a parent Transform for UI elements

    // Define hard set positions for up to 4 players
    // Define hard set positions and anchor settings for up to 4 players
    private readonly PlayerUIPosition[] playerUIPositions = new PlayerUIPosition[]
    {
        new PlayerUIPosition
        {
            AnchorMin = new Vector2(0, 1),
            AnchorMax = new Vector2(0, 1),
            Pivot = new Vector2(0, 1),
            AnchoredPosition = new Vector2(100f, -20f)
        },
        new PlayerUIPosition
        {
            AnchorMin = new Vector2(1, 1),
            AnchorMax = new Vector2(1, 1),
            Pivot = new Vector2(1, 1),
            AnchoredPosition = new Vector2(-100f, -20f) // 10 units from top-right
        },
        new PlayerUIPosition
        {
            AnchorMin = new Vector2(0, 0),
            AnchorMax = new Vector2(0, 0),
            Pivot = new Vector2(0, 0),
            AnchoredPosition = new Vector2(100f, 100f) // 10 units from bottom-left
        },
        new PlayerUIPosition
        {
            AnchorMin = new Vector2(1, 0),
            AnchorMax = new Vector2(1, 0),
            Pivot = new Vector2(1, 0),
            AnchoredPosition = new Vector2(-100f, 100f) // 10 units from bottom-right
        }
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure singleton instance
        }
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }
    void CreatePlayerUI(Player player, int playerIndex)
    {
        if (playerCanvasPrefab == null)
        {
            Debug.LogError("Player Canvas Prefab is not assigned in the Inspector.");
            return;
        }

        if (uiParent == null)
        {
            Debug.LogError("UI Parent is not assigned in the Inspector.");
            return;
        }

        // Instantiate the Canvas prefab as a child of uiParent
        Canvas playerCanvas = Instantiate(playerCanvasPrefab, uiParent);
        playerCanvas.name = $"{player.playerName} UI";

        // Assign the Player reference in PlayerUI script
        PlayerUI playerUI = playerCanvas.GetComponent<PlayerUI>();
        if (playerUI != null)
        {
            playerUI.player = player;
            playerUI.UpdateUI();
        }
        else
        {
            Debug.LogError("PlayerUI script not found on Canvas prefab.");
        }

        // Set the UI Canvas position and anchoring
        RectTransform canvasRect = playerCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            SetPlayerUICanvas(canvasRect, playerIndex);
        }
        else
        {
            Debug.LogError("RectTransform component missing on PlayerCanvas prefab.");
        }

        // Set Panel position and size
        Transform panelTransform = playerCanvas.transform.Find("PlayerInfoPanel");
        if (panelTransform != null)
        {
            RectTransform panelRect = panelTransform.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                SetPlayerUIPanel(panelRect, playerIndex);
            }
            else
            {
                Debug.LogError("RectTransform component missing on PlayerInfoPanel.");
            }
        }
        else
        {
            Debug.LogError("PlayerInfoPanel child not found in PlayerCanvas prefab.");
        }
    }

    void SetPlayerUICanvas(RectTransform rectTransform, int playerIndex)
    {
        if (playerIndex >= playerUIPositions.Length)
        {
            Debug.LogError("Player index exceeds predefined positions.");
            return;
        }

        PlayerUIPosition positionSetting = playerUIPositions[playerIndex];

        // Set anchors
        rectTransform.anchorMin = positionSetting.AnchorMin;
        rectTransform.anchorMax = positionSetting.AnchorMax;

        // Set pivot
        rectTransform.pivot = positionSetting.Pivot;

        // Set anchored position
        rectTransform.anchoredPosition = positionSetting.AnchoredPosition;
        // Adjust anchoredPosition based on screen size if needed
        float offsetX = positionSetting.AnchoredPosition.x * (Screen.width / 1920f); // Assuming Reference Resolution is 1920
        float offsetY = positionSetting.AnchoredPosition.y * (Screen.height / 1080f); // Assuming Reference Resolution is 1080

        rectTransform.anchoredPosition = new Vector2(offsetX, offsetY);
    }
    void SetPlayerUIPanel(RectTransform rectTransform, int playerIndex)
    {
        if (playerIndex >= playerUIPositions.Length)
        {
            Debug.LogError("Player index exceeds predefined positions.");
            return;
        }

        PlayerUIPosition positionSetting = playerUIPositions[playerIndex];

        // Set anchors
        rectTransform.anchorMin = positionSetting.AnchorMin;
        rectTransform.anchorMax = positionSetting.AnchorMax;

        // Set pivot
        rectTransform.pivot = positionSetting.Pivot;

        // Set anchored position
        rectTransform.anchoredPosition = positionSetting.AnchoredPosition;
    }

    // Call this method whenever player data changes
    public void RefreshAllPlayerUIs()
    {
        PlayerUI[] allPlayerUIs = FindObjectsOfType<PlayerUI>();
        foreach (PlayerUI playerUI in allPlayerUIs)
        {
            playerUI.UpdateUI();
        }
    }
    void Start()
    {
        currentPlayerIndex = 0;

        // Load the player prefab from the Resources folder
        playerPrefab = Resources.Load<GameObject>("PlayerPrefab");

        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab could not be found in the Resources folder!");
            return;
        }
        List<Transform> pathNodes = route.GetPathNodes();
        Vector3 vt = pathNodes[0].position;
        vt.z = -1;
        // Initialize the players by instantiating the PlayerPrefab
        players = new List<Player> {
            InstantiateBotPlayer("toan", vt, "Sprites/Player_1", 0),
            InstantiateBotPlayer("hau", vt, "Sprites/Player_2", 1),
            InstantiateBotPlayer("tri", vt, "Sprites/Player_3", 2),
            InstantiateBotPlayer("khanh_anh", vt, "Sprites/Player_4", 3)
        };

        // Create UI for each player
        for (int i = 0; i < players.Count; i++)
        {
            CreatePlayerUI(players[i], i);
        }
        // Start the game
        StartTurn();
    }

    void Update()
    {
        
        if (players[currentPlayerIndex] is BotPlayer botPlayer)
        {
            if (!isPlayerMoving && !isWaitingForUserDecision)
            {
                // Generate the current game state
                GameState currentState = GenerateCurrentGameState(botPlayer.playerID);
                
                // Make decision using MCTS
                StartCoroutine(BotTakeTurn(botPlayer, currentState));
            }
        }
        else
        {
            // Handle player input for rolling the dice
            if (!isPlayerMoving && !isWaitingForUserDecision && Input.GetMouseButtonDown(0))
            {
                // Player's turn
                StartCoroutine(HandleDiceRoll());
            }
        }
        // if (!isPlayerMoving && !isWaitingForUserDecision && Input.GetMouseButtonDown(0))
        // {
        //     if (players[currentPlayerIndex] is BotPlayer botPlayer)
        //     {
        //         // Bot do not need to mouse click
        //         return;
        //     }
        //     else
        //     {
        //         // Player's turn
        //         StartCoroutine(HandleDiceRoll());
        //     }
        // }

        // Debug: Press 'B' to simulate a player going bankrupt
        if (Input.GetKeyDown(KeyCode.B))
        {
            SimulateBankrupt(players[currentPlayerIndex]);
        }
    }

    private void SimulateBankrupt(Player player)
    {
        Debug.Log("Simulating bankruptcy for player: " + player.playerName);
        Bankrupt(player);
    }

    void StartTurn()
    {
        if (isGameStopped) return;

        Player currentPlayer = players[currentPlayerIndex];
        Debug.Log(currentPlayer.playerName + "'s turn");

        // if (currentPlayer is BotPlayer botPlayer)
        // {
        //     // Generate the current game state
        //     GameState currentState = GenerateCurrentGameState(currentPlayer.playerID);
            
        //     // Make decision using MCTS
        //     StartCoroutine(BotTakeTurn(botPlayer, currentState));
        // }
        // else
        // {
        //     // For human players, prompt to roll the dice
        //     Debug.Log("Press left mouse button to roll the dice.");
        // }
    }

    private IEnumerator BotTakeTurn(BotPlayer botPlayer, GameState currentState)
    {
        isPlayerMoving = true;
        isWaitingForUserDecision = true;

        Debug.Log($"{botPlayer.playerName} is rolling the dice...");

        // Roll both dice
        yield return StartCoroutine(RollDice());

        int steps = diceRoller1.GetSteps() + diceRoller2.GetSteps();
        bool rolledDouble = diceRoller1.GetSteps() == diceRoller2.GetSteps();

        Debug.Log($"{botPlayer.playerName} rolled {diceRoller1.GetSteps()} and {diceRoller2.GetSteps()} {(rolledDouble ? "(Double!)" : "")}, total steps: {steps}");

        // Move the bot player
        yield return StartCoroutine(MovePlayer(botPlayer, steps));

        // Generate updated game state after moving
        GameState updatedState = GenerateCurrentGameState(botPlayer.playerID);

        // Make a decision based on the updated game state
        ActionType decision = botPlayer.MakeDecision(updatedState);

        // Execute the decided action
        ExecuteBotAction(botPlayer, decision);

        // Handle doubles
        if (rolledDouble)
        {
            doubleCount++;
            if (doubleCount >= 3)
            {
                Debug.Log($"{botPlayer.playerName} rolled doubles three times and is sent to jail.");
                SendPlayerToJail(botPlayer);
                doubleCount = 0;
            }
            else
            {
                Debug.Log($"{botPlayer.playerName} rolled doubles and gets another turn.");
                yield return new WaitForSeconds(0.5f);
                // Bot get another turn
                yield return StartCoroutine(BotTakeTurn(botPlayer, updatedState));

                yield break;
            }
        }
        else
        {
            doubleCount = 0;
        }

        isPlayerMoving = false;
        isWaitingForUserDecision = false;

        EndTurn(); // Advance to the next player's turn
    }

    private IEnumerator RollDice()
    {
        // Start rolling both dice
        yield return StartCoroutine(diceRoller1.RollTheDice());
        yield return StartCoroutine(diceRoller2.RollTheDice());
    }
    /// <summary>
    /// Executes the action decided by the bot.
    /// </summary>
    private void ExecuteBotAction(BotPlayer botPlayer, ActionType action)
    {
        NodeInfo nodeInfo = route.properties.FirstOrDefault(p => p.ID == botPlayer.currentPosition);
        if (nodeInfo == null)
        {
            Debug.LogError("Bot landed on an invalid property.");
            return;
        }

        // Manually map NodeInfo to PropertyState
        PropertyState currentProperty = new PropertyState
        {
            PropertyName = nodeInfo.name,
            PropertyID = nodeInfo.ID,
            IsOwned = nodeInfo.owner != null,
            OwnerID = nodeInfo.owner != null ? nodeInfo.owner.playerID : -1,
            Price = nodeInfo.price,
            Group = nodeInfo.group
        };

        switch (action)
        {
            case ActionType.BuyProperty:
                if (!currentProperty.IsOwned && botPlayer.money >= currentProperty.Price)
                {
                    BuyProperty(botPlayer);
                }
                else
                {
                    Debug.Log($"{botPlayer.playerName} decided not to buy {currentProperty.PropertyName}.");
                }
                break;

            case ActionType.Pass:
                Debug.Log($"{botPlayer.playerName} decided to pass on {currentProperty.PropertyName}.");
                // Optionally, perform any 'Pass'-specific logic here
                break;

            default:
                Debug.LogWarning($"Unhandled action type: {action}");
                break;
        }
        // After executing the action, refresh the UI
        RefreshAllPlayerUIs();
    }
    IEnumerator HandleDiceRoll()
    {
        isPlayerMoving = true;
        isWaitingForUserDecision = true;

        // Start rolling both dice
        yield return StartCoroutine(RollDice());

        int steps = diceRoller1.GetSteps() + diceRoller2.GetSteps();
        bool rolledDouble = diceRoller1.GetSteps() == diceRoller2.GetSteps();

        Debug.Log($"Player rolled {diceRoller1.GetSteps()} and {diceRoller2.GetSteps()} {(rolledDouble ? "(Double!)" : "")}, total steps: {steps}");

        // Move the player
        yield return StartCoroutine(MovePlayer(players[currentPlayerIndex], steps));

        if (rolledDouble)
        {
            doubleCount++;
            if (doubleCount >= 3)
            {
                Debug.Log($"{players[currentPlayerIndex].playerName} rolled doubles three times and is sent to jail.");
                SendPlayerToJail(players[currentPlayerIndex]);
                doubleCount = 0;
            }
            else
            {
                Debug.Log($"{players[currentPlayerIndex].playerName} rolled doubles and gets another turn.");
                isPlayerMoving = false;
                isWaitingForUserDecision = false;
                StartTurn(); // Player gets another turn
                yield break;
            }
        }
        else
        {
            doubleCount = 0;
        }

        isPlayerMoving = false;
        isWaitingForUserDecision = false;
        RefreshAllPlayerUIs();

        EndTurn(); // Advance to the next player's turn
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
        isWaitingForUserDecision = true;
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
        if (FindObjectOfType<EventSystem>() == null)
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
        callback(userDecision);
    } 
    
    IEnumerator MovePlayer(Player player, int steps)
    {
        if (doubleCount == 3){
            SendPlayerToJail(player);
            EndTurn();
            yield break;
        }

        isPlayerMoving = true;  // Prevent other actions while the player is moving

        List<Transform> pathNodes = route.GetPathNodes();  // Get the path nodes from the Route

        for (int i = 0; i < steps; i++)
        {
            if (isGameStopped) yield break; // Exit coroutine if the game is stopped

            player.currentPosition = (player.currentPosition + 1) % pathNodes.Count;  // Move along the path
            Vector3 newPosition = pathNodes[player.currentPosition].position;
            newPosition.z = player.transform.position.z;  // Maintain the player's original Z position
            player.transform.position = newPosition;  // Update the player's position
            yield return new WaitForSeconds(0.5f); // Wait a bit between moves for better visualization
        }
        // Handle the player landing on the node: buy property, pay rent, etc.
        yield return StartCoroutine(HandlePlayerLanding(player));

        // isPlayerMoving = false;  // Allow other actions after the player has finished moving

        // if (doubleCount > 0){
        //     // Wait for the player to click the left mouse button before rolling again
        //     Debug.Log("Click the left mouse button to roll again.");
        //     yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        //     yield break;
        // }
        // EndTurn();
    }

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        doubleCount = 0;
        currentTurn++;
        StartTurn();
    }
    private Player InstantiatePlayer(string playerName, Vector3 vector, string spritePath, int ID)
    {
        Debug.Log("Instantiating player: " + playerName);
        Debug.Log("Sprite path: " + spritePath);
        GameObject playerObject = Instantiate(playerPrefab, vector, Quaternion.identity);
        Player player = playerObject.GetComponent<Player>();
        player.playerID = ID;
        player.playerName = playerName;
        player.money = 1500;
        player.gameObject = playerObject;

        // Load and assign the sprite
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
        player.playerIcons = sprites;

        if (sprites != null)
        {
            playerObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
        }
        else
        {
            Debug.LogError("Sprite could not be found at path: " + spritePath);
        }

        // Ensure the GameObject is active
        playerObject.SetActive(true);

        // Ensure the SpriteRenderer is on the correct sorting layer and order
        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Default"; // Change to your sorting layer name
        spriteRenderer.sortingOrder = 0; // Change to your desired sorting order

        // Scale the player object up to 50 times its original size
        playerObject.transform.localScale = new Vector3(60, 60, 0);

        return player;
    }
    private BotPlayer InstantiateBotPlayer(string playerName, Vector3 position, string spritePath, int ID)
    {
        GameObject playerObject = Instantiate(playerPrefab, position, Quaternion.identity);
        BotPlayer botPlayer = playerObject.AddComponent<BotPlayer>();
        botPlayer.playerID = ID;
        botPlayer.playerName = playerName;
        botPlayer.money = 1500;
        botPlayer.livePreserver = 0;
        botPlayer.isInJail = false;
        botPlayer.hasFreeJailCard = false;
        botPlayer.jailTurns = 0;
        botPlayer.currentPosition = 0;
        botPlayer.monopolyGroupCount = 0;
        botPlayer.propertyList = new List<Property>();

        // Load and assign the sprite
        Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
        botPlayer.playerIcons = sprites;

        if (sprites != null && sprites.Length > 0)
        {
            playerObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
        }
        else
        {
            Debug.LogError("Sprite could not be found at path: " + spritePath);
        }

        // Ensure the GameObject is active
        playerObject.SetActive(true);

        // Ensure the SpriteRenderer is on the correct sorting layer and order
        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Default"; // Change to your sorting layer name
        spriteRenderer.sortingOrder = 0; // Change to your desired sorting order

        // Scale the player object up
        playerObject.transform.localScale = new Vector3(60, 60, 1);

        // Add to players list
        // players.Add(botPlayer);

        return botPlayer;
    }
    // private BotPlayer InstantiateBotPlayer(string playerName, Vector3 vector, string spritePath, int ID)
    // {
    //     Debug.Log("Instantiating bot player: " + playerName);
    //     Debug.Log("Sprite path: " + spritePath);
    //     GameObject playerObject = Instantiate(playerPrefab, vector, Quaternion.identity);
    //     BotPlayer botPlayer = playerObject.AddComponent<BotPlayer>();
    //     botPlayer.playerID = ID;
    //     botPlayer.playerName = playerName;
    //     botPlayer.money = 1500;
    //     botPlayer.livePreserver = 0;
    //     botPlayer.isInJail = false;
    //     botPlayer.hasFreeJailCard = false;
    //     botPlayer.jailTurns = 0;
    //     botPlayer.currentPosition = 0;
    //     botPlayer.monopolyGroupCount = 0;
    //     botPlayer.propertyList = new List<Property>(); // Ensure propertyList is initialized

    //     // Initialize the neural network for the bot player
    //     IBlackBox neuralNetwork = CreateNeuralNetwork(); // Implement this method to create and initialize the neural network
    //     botPlayer.Initialize(neuralNetwork);

    //     // Load and assign the sprite
    //     Sprite[] sprites = Resources.LoadAll<Sprite>(spritePath);
    //     botPlayer.playerIcons = sprites;

    //     if (sprites != null)
    //     {
    //         playerObject.GetComponent<SpriteRenderer>().sprite = sprites[0];
    //     }
    //     else
    //     {
    //         Debug.LogError("Sprite could not be found at path: " + spritePath);
    //     }

    //     // Ensure the GameObject is active
    //     playerObject.SetActive(true);

    //     // Ensure the SpriteRenderer is on the correct sorting layer and order
    //     SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
    //     spriteRenderer.sortingLayerName = "Default"; // Change to your sorting layer name
    //     spriteRenderer.sortingOrder = 0; // Change to your desired sorting order

    //     // Scale the player object up to 50 times its original size
    //     playerObject.transform.localScale = new Vector3(60, 60, 0);

    //     return botPlayer;
    // }

    // private IBlackBox CreateNeuralNetwork()
    // {
    //     // Load the genome from a file
    //     string genomeFilePath = "Assets/UnitySharpNEAT/SharpNeAT/Genomes/genome.xml";
    //     NeatGenome genome = LoadGenome(genomeFilePath);

    //     // Create a network activation scheme using the static factory method
    //     var activationScheme = NetworkActivationScheme.CreateCyclicFixedTimestepsScheme(1);

    //     // Create a genome decoder with the activation scheme
    //     var genomeDecoder = new NeatGenomeDecoder(activationScheme);

    //     // Decode the genome to create a neural network (IBlackBox)
    //     IBlackBox neuralNetwork = genomeDecoder.Decode(genome);

    //     return neuralNetwork;
    // }

    // private NeatGenome LoadGenome(string filePath)
    // {
    //     // Create and initialize the NeatGenomeFactory with appropriate input and output counts
    //     var neatGenomeFactory = new NeatGenomeFactory(10, 2); // Adjust as needed

    //     using (var reader = XmlReader.Create(filePath))
    //     {
    //         return NeatGenomeXmlIO.ReadCompleteGenomeList(reader, false, neatGenomeFactory)[0];
    //     }
    // }
    private IEnumerator HandlePlayerLanding(Player player)
    {
        string nodeType = route.GetNodeType(player.currentPosition);
        // Check what type of node the player landed on
        switch (nodeType)
        {
            case "property":
            case "transportation":
                if (!route.IsNodeOwned(player.currentPosition))
                {
                    // Skip BotPlayer's decision to buy the property
                    if (player is BotPlayer){
                        break;
                    }
                    // Ask the player if they want to buy the property
                    if (player.money >= route.GetNodePrice(player.currentPosition))
                    {
                        yield return StartCoroutine(GetUserDecision("Do you want to buy " + route.GetNodeName(player.currentPosition) + " for " + route.GetNodePrice(player.currentPosition) + "?", decision => {
                            Debug.Log("Decision: " + decision);
                            if (decision)
                            {
                                BuyProperty(player);
                            }
                        }));
                    }
                    else
                    {
                        Debug.Log("You don't have enough money to buy " + route.GetNodeName(player.currentPosition));
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
                player.livePreserver += 1;
                break;
            case "treasure":
                // Implement random choice on treasureCards effect
                int randomIndex = UnityEngine.Random.Range(0, treasureCards.Length);
                string selectedCard = treasureCards[randomIndex];
                Debug.Log(player.playerName + " drew a treasure card: " + selectedCard);

                switch (selectedCard)
                {
                    case "jail":
                        SendPlayerToJail(player);
                        break;
                    case "live":
                        // Implement live effect (e.g., gain money, move forward, etc.)
                        player.livePreserver += 1;
                        Debug.Log(player.playerName + " gained a live preserver.");
                        break;
                    case "free_jail":
                        // Implement free jail effect (e.g., get out of jail free card)
                        player.hasFreeJailCard = true;
                        Debug.Log(player.playerName + " received a Get Out of Jail Free card.");
                        break;
                    case "money":
                        // Implement money effect (e.g., gain a random amount of money)
                        int moneyAmount = UnityEngine.Random.Range(100, 200); // Example effect: gain 100 to 200 money
                        player.money += moneyAmount;
                        Debug.Log(player.playerName + " gained " + moneyAmount + "$.");
                        break;
                }
                break;
            case "go":
                player.money += 200;
                break;
            default: // "free"
                break;
        }
        yield return null;
    }
    public void BuyProperty(Player player)
    {
        route.BuyNode(player.currentPosition, player);
        Debug.Log(player.playerName + " has bought " + route.GetNodeName(player.currentPosition) + " for " + route.GetNodePrice(player.currentPosition));

        if (player.monopolyGroupCount == 3)
        {
            DeclareWinner(player);
        }
    }
    private void PayRent(Player player, Player owner, int rentAmount)
    {
        if (player.livePreserver > 0){
            player.livePreserver -= 1;
            Debug.Log(player.playerName + " used a live preserver.");
            return;
        }
        while (player.money < rentAmount)
        {
            if (player.propertyList.Count > 0)
            {
                // Sell the first property in the list
                Property propertyToSell = player.propertyList[0];
                player.propertyList.RemoveAt(0);
                player.money += propertyToSell.price;  // Sell the property for half the price
                route.SellNode(player.currentPosition, player);
                Debug.Log(player.playerName + " sold " + propertyToSell.name + " for " + propertyToSell.price);
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
        Debug.Log(player.playerName + " paid " + rentAmount + "$ for " + owner.playerName + " in rent.");
    }
    private void PayTax(Player player)
    {
        if (player.livePreserver > 0){
            player.livePreserver -= 1;
            Debug.Log(player.playerName + " used a live preserver.");
            return;
        }
        int taxAmount = 200 + (int)Math.Ceiling(0.1 * (player.money + player.GetValueAllProperties()));
        while (player.money < taxAmount)
        {
            if (player.propertyList.Count > 0)
            {
                // Sell the first property in the list
                Property propertyToSell = player.propertyList[0];
                player.propertyList.RemoveAt(0);
                player.money += propertyToSell.price;  // Sell the property
                route.SellNode(player.currentPosition, player);
                Debug.Log(player.playerName + " sold " + propertyToSell.name + " for " + propertyToSell.price);
            }
            else
            {
                // Player is bankrupt
                Bankrupt(player);
                return;
            }
        }
        player.money -= taxAmount;
        Debug.Log(player.playerName + " paid " + taxAmount + "$ " + " in tax.");
    }

    private void SendPlayerToJail(Player player)
    {
        if (player.hasFreeJailCard)
        {
            Debug.Log(player.playerName + " used a Get Out of Jail Free card.");
            player.hasFreeJailCard = false;
            return;
        }
        player.currentPosition = 10;  // Send the player to jail
        player.isInJail = true;
        player.jailTurns = 3;

        isPlayerMoving = true;  // Prevent other actions while the player is moving

        List<Transform> pathNodes = route.GetPathNodes();  // Get the path nodes from the Route

        Vector3 newPosition = pathNodes[player.currentPosition].position;
        newPosition.z = player.transform.position.z;  // Maintain the player's original Z position
        player.transform.position = newPosition;  // Update the player's position

        isPlayerMoving = false;  // Allow other actions after the player has finished moving
        EndTurn();
    }
    private void Bankrupt(Player player)
    {
        Debug.Log(player.playerName + " is bankrupt!");
        // Remove the player's GameObject from the board
        Destroy(player.gameObject);

        // Remove the player from the list of active players
        players.Remove(player);

        // Update currentPlayerIndex
        currentPlayerIndex = currentPlayerIndex % players.Count;

        // Check if only one player remains
        if (players.Count == 1)
        {
            DeclareWinner(players[0]);
        }
    }
    private void DeclareWinner(Player winner)
    {
        Debug.Log(winner.playerName + " is the winner!");
        ShowWinnerUI(winner);
        StopGame();
    }

    private void ShowWinnerUI(Player winner)
    {
        // Create a UI panel to display the winner
        GameObject winnerPanel = new GameObject("WinnerPanel");
        Canvas canvas = winnerPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler canvasScaler = winnerPanel.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        winnerPanel.AddComponent<GraphicRaycaster>();

        // Set the position of the Canvas
        RectTransform rectTransform = winnerPanel.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, 0);

        // Create winner text
        GameObject winnerTextObject = new GameObject("WinnerText");
        winnerTextObject.transform.SetParent(winnerPanel.transform);
        Text winnerText = winnerTextObject.AddComponent<Text>();
        winnerText.text = winner.playerName + " is the winner!";
        winnerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winnerText.alignment = TextAnchor.MiddleCenter;
        winnerText.color = Color.black; // Set text color

        // Set the RectTransform of the winner text to be centered
        RectTransform winnerTextRect = winnerText.GetComponent<RectTransform>();
        winnerTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        winnerTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        winnerTextRect.anchoredPosition = new Vector2(0, 50); // Set position of the text box
        winnerTextRect.sizeDelta = new Vector2(300, 50); // Set size of the text box

        // Create Back to Main Menu button
        GameObject backButton = CreateButton("Back to Main Menu", new Vector2(0, -50), Color.red, () => { Debug.Log("clicked"); SceneManager.LoadScene("MainMenu"); });
        backButton.transform.SetParent(winnerPanel.transform);
    }

    private void StopGame()
    {
        // Implement logic to stop the game, such as disabling player input and stopping coroutines
        isPlayerMoving = false;
        isWaitingForUserDecision = false;
        isGameStopped = true;
        StopAllCoroutines();
    }
    private Player FindPlayerByID(int id)
    {
        return players.FirstOrDefault(p => p.playerID == id);
    }

    public GameState GenerateCurrentGameState(int playerID)
    {
        // Generate and return the current game state for the specified player
        GameState currentState = new GameState
        {
            CurrentPlayerID = playerID,
            Players = players.Select(p => p.GetPlayerState()).ToList(),
            Properties = route.properties.Select(p => new PropertyState
            {
                PropertyName = p.name,
                PropertyID = p.ID,
                IsOwned = p.owner != null,
                OwnerID = p.owner != null ? p.owner.playerID : -1,
                Price = p.price,
                Group = p.group
            }).ToList(),
            CurrentPosition = players.FirstOrDefault(p => p.playerID == playerID)?.currentPosition ?? 0,
            PlayerFunds = players.FirstOrDefault(p => p.playerID == playerID)?.money ?? 0f
            // Add other necessary game state details
        };
        return currentState;
    }
}
