using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class NodeInfo
{
    public int ID; // Added ID field
    public string name;
    public string type;
    public int price;
    public int group;
    public Player owner = null;

    public static NodeInfo[] CreateFromJSON(string jsonString)
    {
        NodeInfoList nodeInfoList = JsonUtility.FromJson<NodeInfoList>(jsonString);
        return nodeInfoList?.nodes;
    }

    // Clone method for deep copying
    public NodeInfo Clone()
    {
        return new NodeInfo
        {
            ID = this.ID,
            name = this.name,
            type = this.type,
            price = this.price,
            group = this.group,
            owner = this.owner
        };
    }
}
[System.Serializable]
public class NodeInfoList
{
    public NodeInfo[] nodes;
}

public class Route : MonoBehaviour
{
    [SerializeField]
    private List<Transform> childNodeList = new List<Transform>();

    [SerializeField]
    private NodeInfo[] nodeInfoList;

    // Public property to expose nodeInfoList
    public NodeInfo[] properties => nodeInfoList;

    [Header("JSON Data Path")]
    [Tooltip("Relative path to the JSON file from the project's Assets folder.")]
    public string jsonFilePath = "data.json"; // Ensure this path is correct

    private void Awake()
    {
        // Initialize nodes and load JSON data
        FillNodes();
        LoadJSONData();
    }

    private void Start()
    {
        // Optional: Initialize or verify data after loading
        if (nodeInfoList == null || nodeInfoList.Length == 0)
        {
            Debug.LogError("No properties loaded. Please check the JSON file.");
        }
        else
        {
            Debug.Log("Properties loaded successfully.");
            PrintAllPropertyIDs(); // Example method to print IDs
        }
    }

    private void LoadJSONData()
    {
        string path = Path.Combine(Application.dataPath, jsonFilePath);
        if (!File.Exists(path))
        {
            Debug.LogError("JSON data file not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        nodeInfoList = NodeInfo.CreateFromJSON(json);

        if (nodeInfoList == null || nodeInfoList.Length == 0)
        {
            Debug.LogError("Failed to parse JSON data or data is empty.");
        }
    }
    private void OnDrawGizmos()
    {
        // Set the color for the gizmos
        Gizmos.color = Color.red;

        // Draw lines between the nodes
        for (int i = 0; i < childNodeList.Count; i++)
        {
            Vector3 currentPos = childNodeList[i].position;
            if (i > 0)
            {
                Vector3 prevPos = childNodeList[i - 1].position;
                Gizmos.DrawLine(prevPos, currentPos);
            }
        }
    }

    private void FillNodes()
    {
        // Clear the existing list to avoid duplicates
        childNodeList.Clear();

        // Get all child transforms
        foreach (Transform child in transform)
        {
            // Add only the direct children of this transform
            childNodeList.Add(child);
        }
    }
    public List<Transform> GetPathNodes()
    {
        return childNodeList;
    }

    public string GetNodeType(int nodeIndex)
    {
        return nodeInfoList[nodeIndex].type;
    }
    public bool IsNodeOwned(int nodeIndex)
    {
        return nodeInfoList[nodeIndex].owner != null;
    }
    public int GetNodePrice(int nodeIndex)
    {
        return nodeInfoList[nodeIndex].price;
    }
    public string GetNodeName(int nodeIndex)
    {
        return nodeInfoList[nodeIndex].name;
    }
    public string GetOwnerName(int nodeIndex)
    {
        return nodeInfoList[nodeIndex].owner.playerName;
    }
    public Player GetOwner(int nodeIndex)
    {
        return nodeInfoList[nodeIndex].owner;
    }
    public void BuyNode(int nodeIndex, Player player)
    {
        NodeInfo nodeInfo = nodeInfoList[nodeIndex];
        if (nodeInfo.owner == null)
        {
            nodeInfo.owner = player;
            Property newProperty = new Property(nodeInfo.name, nodeInfo.price, nodeInfo.group);
            player.propertyList.Add(newProperty);
            player.money -= nodeInfo.price;
            Debug.Log(player.playerName + " bought " + newProperty.name + " for " + newProperty.price);
            Debug.Log(player.playerName + " now has " + player.money + " money");

            if (newProperty.group != 0)
            {
                // Check if the player has 3 properties in the same group
                int propertiesInGroup = CountPropertiesInGroup(player, newProperty.group);
                if (propertiesInGroup == 3 || (propertiesInGroup == 2 && (newProperty.group == 1 || newProperty.group == 8)))
                {
                    player.monopolyGroupCount += 1;
                    // Increase the price of properties in the same group by 2x
                    IncreasePropertyPricesInGroup(newProperty.group);
                    Debug.Log(player.playerName + " now has 3 properties in the " + newProperty.group + " group. Prices increased by 2x.");
                }
            }
            
        }
    }
    public void SellNode(int nodeIndex, Player player)
    {
        NodeInfo nodeInfo = nodeInfoList[nodeIndex];
        nodeInfo.owner = null;
        int group = nodeInfo.group;
        if (group != 0)
        {
            // Check if the player has 3 properties in the same group
            int propertiesInGroup = CountPropertiesInGroup(player, group);
            if (propertiesInGroup == 3 || (propertiesInGroup == 2 && (group == 1 || group == 8)))
            {
                player.monopolyGroupCount -= 1;
                // Increase the price of properties in the same group by 2x
                DecreasePropertyPricesInGroup(group);
                Debug.Log(player.playerName + " sold a property in the " + group + " group. Prices decreased by half.");
            }
        }
    }
    // Method to count properties in the same group
    private int CountPropertiesInGroup(Player player, int group)
    {
        int count = 0;
        foreach (Property property in player.propertyList)
        {
            if (property.group == group)
            {
                count++;
            }
        }
        return count;
    }
    // Method to increase the price of properties in the same group by 2x
    private void IncreasePropertyPricesInGroup(int group)
    {
        foreach (NodeInfo nodeInfo in nodeInfoList)
        {
            if (nodeInfo.group == group)
            {
                nodeInfo.price *= 2;
            }
        }
    }
    // Method to decrease the price of properties in the same group by half
    private void DecreasePropertyPricesInGroup(int group)
    {
        foreach (NodeInfo nodeInfo in nodeInfoList)
        {
            if (nodeInfo.group == group)
            {
                nodeInfo.price /= 2;
            }
        }
    }
    // Example method to print all property IDs
    public void PrintAllPropertyIDs()
    {
        if (nodeInfoList == null || nodeInfoList.Length == 0)
        {
            Debug.LogError("PrintAllPropertyIDs: No properties loaded to print.");
            return;
        }

        Debug.Log("Listing all Property IDs:");
        foreach (NodeInfo node in nodeInfoList)
        {
            Debug.Log($"Property ID: {node.ID}, Name: {node.name}");
        }
    }
}
