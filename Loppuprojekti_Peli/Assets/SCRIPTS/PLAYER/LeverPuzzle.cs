using UnityEngine;
using UnityEngine.Events;

public class LeverPuzzleManager : MonoBehaviour
{
    [Header("Vipujen nykyiset tilat (ylhäällä = true)")]
    public bool[] leverStates = new bool[5];

    [Header("Oikea ratkaisu (kuvan mukaan: ylös, alas, alas, ylös, alas)")]
    private readonly bool[] correctSolution = { true, false, false, true, false };

    [Header("Tapahtuma, kun puzzle ratkeaa (esim. ovi aukeaa)")]
    public UnityEvent onPuzzleSolved;

    // Tätä funktiota kutsutaan, kun pelaaja kääntää vipua
    public void ToggleLever(int leverIndex)
    {
        if (leverIndex < 0 || leverIndex >= leverStates.Length) return;

        // Vaihdetaan vivun tilaa (ylhäältä alas tai päinvastoin)
        leverStates[leverIndex] = !leverStates[leverIndex];

        // TODO: Tässä kohdassa voit myös käynnistää vivun animaation (ylös/alas)
        Debug.Log($"Vipu {leverIndex} on nyt " + (leverStates[leverIndex] ? "YLHÄÄLLÄ" : "ALHAALLA"));

        // Tarkistetaan ratkaisiko tämä siirto puzzlen
        CheckSolution();
    }

    private void CheckSolution()
    {
        for (int i = 0; i < correctSolution.Length; i++)
        {
            // Jos yksikin vipu on väärässä asennossa, keskeytetään tarkistus
            if (leverStates[i] != correctSolution[i])
            {
                return;
            }
        }

        // Jos päästiin tänne asti, kaikki vivut ovat oikein!
        Debug.Log("Puzzle ratkaistu!");
        if (onPuzzleSolved != null)
        {
            onPuzzleSolved.Invoke();
        }
    }
}
