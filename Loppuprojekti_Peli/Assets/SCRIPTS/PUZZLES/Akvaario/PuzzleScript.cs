using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Tarkistaa onko 2 tai enemm‰n puzzlen osaa tehty! Esim: Akvaario puzzle, jossa molempien tankkien pit‰‰ olla samaan aikaan tehty, ennenkuin koko puzzle on ratkaistu ja saa kuulan.
/// </summary>
public class PuzzleScript : MonoBehaviour
{
    //puzzle taulukko
    public AkvaarioScript[] puzzle;
    public UnityAction SolvedAllAction;

    //[SerializeField] private UnityEvent tavoiteSaavutettu = new UnityEvent();
    private void Awake()
    {
        //Unity Action lis‰‰minen alussa
        foreach (var script in puzzle)
        {
            script.SolvedAction += TarkistaRatkaistu;
            script.UnsolvedAction += TarkistaRatkaistu;
        }
    }

    /// <summary>
    /// Tarkistaa onko puzzlet ratkaistu (k‰ytet‰‰n koodia vaan, jos on useampi eri puzzle osaa, jotka pit‰‰ olla samaan aikaan ratkaistu.)
    /// </summary>
    private void TarkistaRatkaistu()
    {
        Debug.Log("SAIN VIESTIN");
        //katsoo onko ratkaistu ja l‰hett‰‰ viestin/invokkaa
        if (OnkoRatkaistu())
        {
            SolvedAllAction.Invoke();
        }
    }

    /// <summary>
    /// Tarkistaa, onko puzzle ratkaistu, eli katsooko onko kaikki valuet true.
    /// </summary>
    /// <returns></returns>
    private bool OnkoRatkaistu()
    {
        //tarkistaa koko listan
        for (int i = 0; i < puzzle.Length; i++)
        {
            //jos joku on false, palauttaa false
            if (puzzle[i].Ratkaistu() == false)
            {
                Debug.Log("EI RATKAISTU");
                return false;
            }
        }
        //jos kaikki meni l‰pi, palauttaa true
        Debug.Log("RATKAISTU!");
        return true;
    }
}
