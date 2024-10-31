using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<Stone> stones;
    private int currentStoneIndex = 0;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!stones[currentStoneIndex].IsMoving)
            {
                int steps = Random.Range(1, 7);
                Debug.Log("Dice Rolled: " + steps);

                if (stones[currentStoneIndex].CanMove(steps))
                {
                    StartCoroutine(stones[currentStoneIndex].Move(steps));
                }
                else
                {
                    Debug.Log("Rolled number is too high");
                }

                // Move to the next stone
                currentStoneIndex = (currentStoneIndex + 1) % stones.Count;
            }
        }
    }
}
