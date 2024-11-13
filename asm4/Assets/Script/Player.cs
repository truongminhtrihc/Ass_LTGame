using UnityEngine;
using Unity.Netcode;
public class Player : MonoBehaviour
{
    public string playerName;
    public int currentPosition;
    public int money;
    public Player(string name)
    {
        playerName = name;
        currentPosition = 0; // Starting position on the board
        money = 1500; // Starting money
    }

}
