using UnityEngine;
using System.Collections.Generic;

using System.IO;
[System.Serializable]
public class NodeInfo
{
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
    private void OnDrawGizmos()
    {
        // Set the color for the gizmos
        Gizmos.color = Color.red;

        // Fill the node list with child transforms
        FillNodes();

        string path = Application.dataPath + "/data.json";
        if (!File.Exists(path))
        {
            Debug.LogError("JSON data file not found at: " + path);
            return;
        }
        string json = File.ReadAllText(path);
        nodeInfoList = NodeInfo.CreateFromJSON(json);

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
    public void BuyNode(int nodeIndex, Player player)
    {
        NodeInfo nodeInfo = nodeInfoList[nodeIndex];
        if (nodeInfo.owner == null)
        {
            nodeInfo.owner = player;
            player.propertyList.Add(new Property(nodeInfo.name, nodeInfo.price, nodeInfo.group));
            player.money -= nodeInfo.price;
            Debug.Log(player.playerName + " bought " + nodeInfo.name + " for " + nodeInfo.price);
            Debug.Log(player.playerName + " now has " + player.money + " money");
        }
        else
        {
            Debug.Log(nodeInfo.name + " is already owned by " + nodeInfo.owner.playerName);
        }
    }
}
