using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private List<PuzzleScript> puzzleScripts;

    //Katsoo onko puzzle tehty ja laukaisee toisen koodin, joka antaa pelaajalle kuulan.
    private void Start()
    {
        puzzleScripts = new List<PuzzleScript>();
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject ro in rootObjects)
        {
            PuzzleScript[] puzzles = ro.GetComponentsInChildren<PuzzleScript>();
            puzzleScripts.AddRange(puzzles);
        }
        foreach (PuzzleScript p in puzzleScripts)
        {
            //Kytke tapahtumaan funktio
            p.SolvedAllAction += PuzzleTehty;
        }
    }

    /// <summary>
    /// Laukaisee, jos puzzle skripti on ratkaistu.
    /// </summary>
    private void PuzzleTehty()
    {
        Debug.Log("Manager sai viestin!");
    }
}
