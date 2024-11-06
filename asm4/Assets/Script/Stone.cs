using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stone : MonoBehaviour
{
    public Route currentRoute;
    public bool IsMoving { get; private set; }

    private int routePosition;
    private SpriteRenderer spriteRenderer;
    private static int baseSortingOrder = 0;
    private static int sortingOrderOffset = 10;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool CanMove(int steps)
    {
        return routePosition + steps < currentRoute.childNodeList.Count;
    }

    public IEnumerator Move(int steps)
    {
        if (IsMoving)
        {
            yield break;
        }
        IsMoving = true;

        // Bring this stone to the front
        spriteRenderer.sortingOrder = baseSortingOrder + sortingOrderOffset;

        while (steps > 0)
        {
            Vector3 nextPos = currentRoute.childNodeList[routePosition + 1].position;
            while (MoveToNextNode(nextPos)) { yield return null; }

            yield return new WaitForSeconds(0.1f);
            steps--;
            routePosition++;
        }

        // Reset sorting order after movement
        spriteRenderer.sortingOrder = baseSortingOrder;

        IsMoving = false;
    }

    private bool MoveToNextNode(Vector3 goal)
    {
        return goal != (transform.position = Vector3.MoveTowards(transform.position, goal, 2f * Time.deltaTime));
    }
}
