using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tarkistaa onko puzzle tehty
/// </summary>
public class PuzzleScript : MonoBehaviour
{
    public AkvaarioScript[] puzzle;
    /// <summary>
    /// Tarkistaa onko puzzle ratkaistu eli katsooko onko kaikki valuet true.
    /// </summary>
    /// <returns></returns>
    private bool Ratkaistu()
    {
        for (int i = 0; i < puzzle.Length; i++)
        {
            if (puzzle[i].Ratkaistu() == false)
            {
                return false;
            }
        }
        return true;
    }
}
