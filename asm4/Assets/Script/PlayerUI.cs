using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Player player; // Reference to the Player script
    public Text nameText;
    public Text moneyText;
    public Text livePreserverText;
    public Text freeJailCardText;

    void Start()
    {
        if (player != null)
        {
            UpdateUI();
        }
        else
        {
            Debug.LogError("Player reference not assigned in PlayerUI.");
        }
    }

    public void UpdateUI()
    {
        nameText.text = $"Name: {player.playerName}";
        moneyText.text = $"Money: ${player.money}";
        livePreserverText.text = $"Lives: {player.livePreserver}";
        freeJailCardText.text = $"Free Jail Cards: {(player.hasFreeJailCard ? "1" : "0")}";
    }

    // Optional: Update UI every frame or when player data changes
    void Update()
    {
        UpdateUI();
    }
}