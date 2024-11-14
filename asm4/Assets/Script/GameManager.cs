using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public List<Player> players;
    public DiceRoller diceRoller1;
    public DiceRoller diceRoller2;
    public Route route;  // Reference to the Route
    private int currentPlayerIndex;
    private GameObject playerPrefab;
    private bool isPlayerMoving = false;
    private bool isWaitingForUserDecision = false;
    private int doubleCount = 0;

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
            InstantiatePlayer("toan", vt, "Sprites/Player_1"),
            InstantiatePlayer("hau", vt, "Sprites/Player_2"),
            InstantiatePlayer("tri", vt, "Sprites/Player_3"),
            InstantiatePlayer("khanh_anh", vt, "Sprites/Player_4")
        };

        
        // Start the game
        StartTurn();
    }

    void Update()
    {
        // Handle player input for rolling the dice
        if (!isPlayerMoving && !isWaitingForUserDecision && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(HandleDiceRoll());
        }
    }

    void StartTurn()
    {
        Player currentPlayer = players[currentPlayerIndex];
        Debug.Log(currentPlayer.playerName + "'s turn");
    }

    IEnumerator HandleDiceRoll()
    {
        bool rolledDouble;
        do
        {
            rolledDouble = false;

            if (!diceRoller1.IsRolling() && !diceRoller2.IsRolling())
            {
                // Roll both dice
                yield return StartCoroutine(diceRoller1.RollTheDice());
                yield return StartCoroutine(diceRoller2.RollTheDice());

                int steps1 = diceRoller1.GetSteps();
                int steps2 = diceRoller2.GetSteps();

                if (steps1 == steps2)
                {
                    doubleCount++;
                    Debug.Log("Player rolled a double! They get to roll again.");
                    rolledDouble = true;
                }
                else
                {
                    doubleCount = 0;
                }

                // Get the sum of both dice rolls
                int steps = steps1 + steps2;

                // Move the player
                yield return StartCoroutine(MovePlayer(players[currentPlayerIndex], steps));
            }
        } while (rolledDouble);
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
            player.currentPosition = (player.currentPosition + 1) % pathNodes.Count;  // Move along the path
            Vector3 newPosition = pathNodes[player.currentPosition].position;
            newPosition.z = player.transform.position.z;  // Maintain the player's original Z position
            player.transform.position = newPosition;  // Update the player's position
            yield return new WaitForSeconds(0.5f); // Wait a bit between moves for better visualization
        }
        // Handle the player landing on the node: buy property, pay rent, etc.
        StartCoroutine(HandlePlayerLanding(player));

        isPlayerMoving = false;  // Allow other actions after the player has finished moving

        if (doubleCount > 0){
            // Wait for the player to click the left mouse button before rolling again
            Debug.Log("Click the left mouse button to roll again.");
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield break;
        }
        EndTurn();
    }

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        doubleCount = 0;
        StartTurn();
    }
    private Player InstantiatePlayer(string playerName,Vector3 vector, string spritePath)
    {
        Debug.Log("Instantiating player: " + playerName);
        Debug.Log("Sprite path: " + spritePath);
        GameObject playerObject = Instantiate(playerPrefab, vector, Quaternion.identity);
        Player player = playerObject.GetComponent<Player>();
        player.playerName = playerName;
        player.money = 1500;
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
                    // Ask the player if they want to buy the property
                    if (player.money >= route.GetNodePrice(player.currentPosition))
                    {
                        yield return StartCoroutine(GetUserDecision("Do you want to buy " + route.GetNodeName(player.currentPosition) + " for " + route.GetNodePrice(player.currentPosition) + "?", decision => {
                            Debug.Log("Decision: " + decision);
                            if (decision)
                            {
                                route.BuyNode(player.currentPosition, player);
                                Debug.Log(player.playerName + " has bought " + route.GetNodeName(player.currentPosition) + " for " + route.GetNodePrice(player.currentPosition));
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
        int taxAmount = 200 + (int)Math.Ceiling(0.1 * (player.money + player.GetValueAllProperties()));
        while (player.money < taxAmount)
        {
            if (player.propertyList.Count > 0)
            {
                // Sell the first property in the list
                Property propertyToSell = player.propertyList[0];
                player.propertyList.RemoveAt(0);
                player.money += propertyToSell.price;  // Sell the property for half the price
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
    private void Bankrupt(Player player)
    {
        Debug.Log(player.playerName + " is bankrupt!");
        players.Remove(player);
    }

    private void SendPlayerToJail(Player player)
    {
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
}
