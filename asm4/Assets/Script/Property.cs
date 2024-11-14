using UnityEngine;
public class Property : MonoBehaviour
{
    public string propertyName;
    public int price;
    public int group;
    public Player owner = null;
    public Property(string name, int price, int group)
    {
        propertyName = name;
        this.price = price;
        this.group = group;
    }
    public void SetOwner(Player player)
    {
        owner = player;
    }
}