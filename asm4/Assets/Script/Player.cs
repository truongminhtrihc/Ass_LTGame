using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
public class Player : MonoBehaviour
{
    public string playerName;
    public int currentPosition;
    public int money;
    public List<Property> propertyList;
    public bool isInJail = false;
    public int jailTurns = 0;
    public Player(string name)
    {
        playerName = name;
        currentPosition = 0; // Starting position on the board
        money = 1500; // Starting money
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
}
