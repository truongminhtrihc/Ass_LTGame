using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Player> players;
    public DiceRoller diceRoller;
    public Route route;  // Reference to the Route
    private int currentPlayerIndex;
    private GameObject playerPrefab;

    private bool isPlayerMoving = false;

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

        // Initialize the players by instantiating the PlayerPrefab
        players = new List<Player> {
            InstantiatePlayer("toan", new Vector3(0, 0, -1), "Sprites/player1"),
            InstantiatePlayer("hau", new Vector3(0, 0, -1), "Sprites/player2"),
            InstantiatePlayer("tri", new Vector3(0, 0, -1), "Sprites/player3"),
            InstantiatePlayer("khanh_anh", new Vector3(0, 0, -1), "Sprites/player4")
        };

        
        // Start the game
        StartTurn();
    }

    void Update()
    {
        // Handle player input for rolling the dice
        if (!isPlayerMoving && Input.GetMouseButtonDown(0))
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
        if (!diceRoller.IsRolling())
        {
            yield return StartCoroutine(diceRoller.RollTheDice());
            int steps = diceRoller.GetSteps();
            yield return StartCoroutine(MovePlayer(players[currentPlayerIndex], steps));
        }
    }

    private IEnumerator GetUserDecision(string message, System.Action<bool> callback)
    {
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
        rectTransform.anchoredPosition = new Vector2(-3, 6);

        // Create Yes button
        GameObject yesButton = new GameObject("YesButton");
        yesButton.transform.SetParent(decisionPanel.transform);

        // Add Image component to the button and set its color
        Image yesImage = yesButton.AddComponent<Image>();
        yesImage.color = Color.green; // Set the desired color

        Button yesBtn = yesButton.AddComponent<Button>();
        yesBtn.targetGraphic = yesImage; // Set the target graphic to the Image component

        // Create and configure the Text component
        GameObject yesTextObject = new GameObject("YesText");
        yesTextObject.transform.SetParent(yesButton.transform);
        Text yesText = yesTextObject.AddComponent<Text>();
        yesText.text = "Yes";
        yesText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        yesText.alignment = TextAnchor.MiddleCenter;
        yesText.color = Color.black; // Set text color

        // Set the RectTransform of the Text component to fill the button
        RectTransform yesTextRect = yesText.GetComponent<RectTransform>();
        yesTextRect.anchorMin = Vector2.zero;
        yesTextRect.anchorMax = Vector2.one;
        yesTextRect.offsetMin = Vector2.zero;
        yesTextRect.offsetMax = Vector2.zero;

        yesBtn.onClick.AddListener(() => { userDecision = true; decisionMade = true; });

        // Create No button
        // GameObject noButton = new GameObject("NoButton");
        // noButton.transform.SetParent(decisionPanel.transform);
        // RectTransform noRect = noButton.AddComponent<RectTransform>();
        // noRect.sizeDelta = new Vector2(2, 2); // Set size of the button
        // noRect.anchoredPosition = new Vector2(-3, 9); // Position the button
        // Button noBtn = noButton.AddComponent<Button>();
        // Text noText = noButton.AddComponent<Text>();
        // noText.text = "No";
        // noText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        // noText.alignment = TextAnchor.MiddleCenter;
        // noText.color = Color.black; // Set text color
        // noBtn.onClick.AddListener(() => { userDecision = false; decisionMade = true; });

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

        callback(userDecision);
    }
    IEnumerator MovePlayer(Player player, int steps)
    {
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

        // Check what type of node the player landed on
        switch (route.GetNodeType(player.currentPosition))
        {
            case "property":
            case "transportation":
                if (!route.IsNodeOwned(player.currentPosition))
                {
                    // Ask the player if they want to buy the property
                    if (player.money >= route.GetNodePrice(player.currentPosition))
                    {
                        yield return StartCoroutine(GetUserDecision("Do you want to buy", decision => {
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
                    // Pay rent to the owner
                    Debug.Log("You landed on " + route.GetNodeName(player.currentPosition) + " owned by " + route.GetOwnerName(player.currentPosition) + ". Pay rent!");
                }
                break;
            case "tax":

                break;
            case "jail":

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
        
        isPlayerMoving = false;  // Allow other actions after the player has finished moving
        EndTurn();
    }

    void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        StartTurn();
    }
    private Player InstantiatePlayer(string playerName,Vector3 vector, string spritePath)
    {
        GameObject playerObject = Instantiate(playerPrefab, vector, Quaternion.identity);
        Player player = playerObject.GetComponent<Player>();
        player.playerName = playerName;
        player.money = 1500;
        // Load and assign the sprite
        Sprite playerSprite = Resources.Load<Sprite>(spritePath);
        if (playerSprite != null)
        {
            playerObject.GetComponent<SpriteRenderer>().sprite = playerSprite;
        }
        else
        {
            Debug.LogError("Sprite could not be found at path: " + spritePath);
        }

        return player;
    }


}
