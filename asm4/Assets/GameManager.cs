using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<Stone> stones;  // List of stone objects
    public DiceRoller diceRoller;  // Reference to the dice roller
    private int currentStoneIndex = 0;  // Index of the current stone | playerIdx

    void Update()
    {
        if (diceRoller.IsRolling())
        {
            Debug.Log("Dice Rolling");
        }
        else
        {
            int steps = diceRoller.GetSteps();
            diceRoller.Reset();
            if (steps > 0 && !stones[currentStoneIndex].IsMoving)
            {
                Debug.Log("Dice Rolled: " + steps);

                if (stones[currentStoneIndex].CanMove(steps))
                {
                    StartCoroutine(stones[currentStoneIndex].Move(steps));
                }
                else
                {
                    Debug.Log("Rolled number is too high");
                }

                // Move to the next stone | next player
                currentStoneIndex = (currentStoneIndex + 1) % stones.Count;
            }
        }

        // Optionally, allow rolling the dice with a space key
        if (Input.GetKeyDown(KeyCode.Space) && !diceRoller.IsRolling())
        {
            // Trigger dice roll
            StartCoroutine(diceRoller.RollTheDice());
        }
    }
}
