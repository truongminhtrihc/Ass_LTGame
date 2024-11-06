using UnityEngine;
using System.Collections;

public class DiceRoller : MonoBehaviour
{
    public Sprite[] diceSides;
    private SpriteRenderer rend;
    private bool rolling;
    public int steps { get; private set; }  // Properly declare the steps property

    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        rolling = false;
        steps = 0;  // Initialize steps
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !rolling)
        {
            rolling = true;
            StartCoroutine(RollTheDice());
        }
    }

    public IEnumerator RollTheDice()
    {
        int randomDiceSide = 0;
        for (int i = 0; i <= 20; i++)
        {
            randomDiceSide = Random.Range(0, 6);
            rend.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }

        steps = randomDiceSide + 1;
        rolling = false;
    }
    public bool IsRolling()
    {
        return rolling;
    }
    public void Reset()
    {
        steps = 0;  // Reset the steps value
    }
    public int GetSteps()
    {
        return steps;  // Return the stored value of steps
    }
}
