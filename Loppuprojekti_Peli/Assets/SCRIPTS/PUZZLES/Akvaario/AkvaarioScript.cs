using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class AkvaarioScript : IPuzzle
{
    private int winPaino = 20;
    [SerializeField] private int nytPaino = 0;
    private List<Kala> kalat = new List<Kala>();

    public UnityAction SolvedAction;
    public UnityAction UnsolvedAction;


    /// <summary>
    /// Tarkistaa ja lis‰‰ kalan akvaarioon.
    /// </summary>
    /// <param name="other">Toimii vaan, jos komponenttiin on yhdistetty Kala skripti</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Kala kala))
        {
            Lisaa(kala);
            Laske();
            if (Ratkaistu())
            { SolvedAction.Invoke();}
            else 
            { UnsolvedAction.Invoke();}
        }
    }

    /// <summary>
    /// Tarkistaa ja poistaa kalan akvaariosta.
    /// </summary>
    /// <param name="other">Toimii vaan, jos komponentiin on yhdistetty Kala skripti</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Kala kala))
        {
            Poista(kala);
            Laske();
            if (Ratkaistu())
            { SolvedAction.Invoke(); }
            else 
            { UnsolvedAction.Invoke(); }
        }
    }

    /// <summary>
    /// lis‰‰ kalan listaan.
    /// </summary>
    /// <param name="kala">mik‰ kala pit‰‰ lis‰t‰</param>
    public void Lisaa(Kala kala)
    {
        kalat.Add(kala);
        Debug.Log("lis‰ttiin!");
    }

    /// <summary>
    /// Poistaa kalan listasta.
    /// </summary>
    /// <param name="kala">mik‰ kala pit‰‰ lis‰t‰</param>
    public void Poista(Kala kala)
    {
        kalat.Remove(kala);
        Debug.Log("poistettiin");
    }

    /// <summary>
    /// Laskee kalojen summan/painon yhteen.
    /// </summary>
    private void Laske()
    {
        nytPaino = 0; //jos en tee, se kokoajan lis‰isi lukuun, joka ei ole oikein
        for (int i = 0; i < kalat.Count; i++)
        {
            nytPaino += kalat[i].Paino;
        }
    }

    /// <summary>
    /// Kertoo, onko t‰m‰ puzzle ratkaistu.
    /// </summary>
    /// <returns></returns>
    public override bool Ratkaistu()
    {
        if (nytPaino == winPaino)
        {
            return true;
        }
        return false;
    }
}
