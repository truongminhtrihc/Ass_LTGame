using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
public class Player : MonoBehaviour
{
    public string playerName;
    public int playerID;
    public int currentPosition;
    public int money;
    public int livePreserver = 0; 
    public List<Property> propertyList;
    public GameObject gameObject;
    public bool isInJail = false;
    public bool hasFreeJailCard = false;
    public int jailTurns = 0;
    public Sprite[] playerIcons;
    public int monopolyGroupCount = 0;
    public Player(string name)
    {
        playerName = name;
        currentPosition = 0; // Starting position on the board
        money = 1500; // Starting money
        propertyList = new List<Property>();
    }
    
    public virtual void Initialize()
    {
        money = 1500;
        livePreserver = 0;
        isInJail = false;
        hasFreeJailCard = false;
        jailTurns = 0;
        currentPosition = 0;
        monopolyGroupCount = 0;
        propertyList = new List<Property>();
    }

    public int GetValueAllProperties()
    {
        int value = 0;
        foreach (Property property in propertyList)
        {
            value += property.price;
        }
        return value;
    }
    // Method to convert Player to PlayerState
    public PlayerState GetPlayerState()
    {
        return new PlayerState
        {
            PlayerName = this.playerName,
            PlayerID = this.playerID,
            Funds = this.money,
            OwnedProperties = this.propertyList.Select(p => p.Clone()).ToList(),
            IsInJail = this.isInJail
            // Add other necessary player state details
        };
    }
}
