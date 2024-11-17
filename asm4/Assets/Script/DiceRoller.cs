using UnityEngine;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    public Sprite[] diceSides;
    private SpriteRenderer rend;
    private bool rolling;
    public int steps { get; private set; }

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        rolling = false;
        steps = 0;
    }

    public IEnumerator RollTheDice()
    {
        rolling = true;  // Set rolling to true at the start of the roll
        int randomDiceSide = 0;
        for (int i = 0; i <= 6; i++)
        {
            randomDiceSide = Random.Range(0, 6);
            rend.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }
        steps = randomDiceSide + 1;
        rolling = false;  // Set rolling to false after the roll is complete
    }

    public bool IsRolling()
    {
        return rolling;
    }

    public int GetSteps()
    {
        return steps;
    }
}
