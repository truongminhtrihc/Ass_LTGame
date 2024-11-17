using UnityEngine;
public class Property : MonoBehaviour
{
    public string name;
    public int price;
    public int group;
    public Property(string name, int price, int group)
    {
        this.name = name;
        this.price = price;
        this.group = group;
    }
    public Property Clone()
    {
        return new Property(this.name, this.price, this.group);
    }
}