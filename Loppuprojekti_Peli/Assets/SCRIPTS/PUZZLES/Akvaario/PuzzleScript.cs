using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Tarkistaa onko puzzle tehty
/// </summary>
public class PuzzleScript : MonoBehaviour
{
    //puzzle taulukko
    public AkvaarioScript[] puzzle;
    public UnityAction SolvedAllAction;

    //[SerializeField] private UnityEvent tavoiteSaavutettu = new UnityEvent();
    private void Awake()
    {
        foreach (var script in puzzle)
        {
            script.SolvedAction += TarkistaRatkaistu;
            script.UnsolvedAction += TarkistaRatkaistu;
        }
    }

    private void TarkistaRatkaistu()
    {
        Debug.Log("SAIN VIESTIN");
        if (OnkoRatkaistu())
        {
            SolvedAllAction.Invoke();
        }
    }

    /// <summary>
    /// Tarkistaa onko puzzle ratkaistu eli katsooko onko kaikki valuet true.
    /// </summary>
    /// <returns></returns>
    private bool OnkoRatkaistu()
    {
        for (int i = 0; i < puzzle.Length; i++)
        {
            if (puzzle[i].Ratkaistu() == false)
            {
                Debug.Log("EI RATKAISTU");
                return false;
            }
        }
        Debug.Log("RATKAISTU!");
        return true;
    }
}
