using System.Collections;
using System.Collections.Generic;
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
