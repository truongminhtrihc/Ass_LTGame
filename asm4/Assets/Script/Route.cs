using UnityEngine;
using System.Collections.Generic;

public class Route : MonoBehaviour
{
    [SerializeField]
    private List<Transform> childNodeList = new List<Transform>();

    private void OnDrawGizmos()
    {
        // Set the color for the gizmos
        Gizmos.color = Color.red;

        // Fill the node list with child transforms
        FillNodes();

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
}
