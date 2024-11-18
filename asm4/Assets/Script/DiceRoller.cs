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

    public IEnumerator RollTheDice(int predeterminedSide = -1)
    {
        rolling = true;
        int randomDiceSide = 0;
        for (int i = 0; i <= 20; i++)
        {
            randomDiceSide = Random.Range(0, 6);
            rend.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }
        rend.sprite = diceSides[predeterminedSide-1];
        steps = randomDiceSide + 1;
        rolling = false;
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
