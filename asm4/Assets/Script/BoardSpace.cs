using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public int spaceIndex;
    public string spaceName;
    public Vector3 position;

    void Start()
    {
        position = transform.position;
    }
}
