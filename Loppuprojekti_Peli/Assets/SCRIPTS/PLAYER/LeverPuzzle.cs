using UnityEngine;
using UnityEngine.Events;

public class LeverPuzzleManager : MonoBehaviour
{
    [Header("Vedä tähän kaikki 5 vipuobjektia vasemmalta oikealle")]
    public AnimateLever[] leverObjects;

    [Header("Vipujen nykyiset tilat (Peli täyttää nämä itse alussa)")]
    public bool[] leverStates;

    [Header("Oikea ratkaisu (kuvan mukaan: ylös, alas, alas, ylös, alas)")]
    private readonly bool[] correctSolution = { true, false, false, true, false };

    [Header("Tapahtuma, kun puzzle ratkeaa")]
    public UnityEvent onPuzzleSolved;

    private void Start()
    {
        // Alustetaan taulukko vipujen määrän mukaan
        leverStates = new bool[leverObjects.Length];

        // Luodaan silmukka, joka pyörii kunnes arvottu tilanne EI ole voittotilanne
        bool isSolutionAtStart = true;
        while (isSolutionAtStart)
        {
            for (int i = 0; i < leverObjects.Length; i++)
            {
                leverStates[i] = Random.value > 0.5f;
            }

            // Tarkistetaan, tuliko vahingossa heti oikea vastaus
            isSolutionAtStart = true;
            for (int i = 0; i < correctSolution.Length; i++)
            {
                if (leverStates[i] != correctSolution[i])
                {
                    isSolutionAtStart = false; // Löytyi eroavaisuus, eli alkutilanne on uniikki!
                    break;
                }
            }
        }

        // Nyt kun meillä on varmasti uniikki alkutilanne, asetetaan vivut paikoilleen
        for (int i = 0; i < leverObjects.Length; i++)
        {
            if (leverObjects[i] == null) continue;
            leverObjects[i].SetInitialState(leverStates[i]);
        }

        // Poistettu CheckSolution täältä, koska tiedämme sen olevan nyt false!
    }


    public void ToggleLever(int leverIndex)
    {
        if (leverIndex < 0 || leverIndex >= leverStates.Length) return;

        // Vaihdetaan tilaa managerissa (true -> false tai false -> true)
        leverStates[leverIndex] = !leverStates[leverIndex];

        // Pakotetaan vipu liikkumaan täsmälleen managerin määräämään tilaan
        if (leverObjects[leverIndex] != null)
        {
            leverObjects[leverIndex].ForceLeverVisualState(leverStates[leverIndex]);
        }

        CheckSolution();
    }

    private void CheckSolution()
    {
        for (int i = 0; i < correctSolution.Length; i++)
        {
            if (leverStates[i] != correctSolution[i])
            {
                return;
            }
        }

        Debug.Log("Puzzle ratkaistu!");
        if (onPuzzleSolved != null)
        {
            onPuzzleSolved.Invoke();
        }
    }
}
