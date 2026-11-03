using UnityEngine;

public class Koira : MonoBehaviour
{
    //koiran parametrit
    private int winCondition = 2;
    private int nytMaara = 0;

    private bool saavutettu = false; //kun saavuttaa m‰‰r‰n, muuttuu trueksi.

    public void TriggerKoira(Collider other)
    {
        //Koira tarkistaa, ett‰ playerill‰ on oikea item (patteri)
        if (other.gameObject.TryGetComponent(out InteractableItem item))
        { 
            //jos on tarvittava m‰‰r‰ jo, kerro pelaajalle
            if (saavutettu) 
            {
                Debug.Log("saavutit m‰‰r‰n jo.");
                return; 
            }
            //true, ottaa itemin ja poistaa pelaajan inventaariosta
            nytMaara++;

            //tarkistaa ett‰ on Win condition eli tarvittava m‰‰r‰ itemeit‰
            if (nytMaara >= winCondition)
            {
                saavutettu = true;
                Debug.Log("saavutettu m‰‰r‰");
            }
        }

        //false ilmoittaa pelaajalle ja palauttaa
        Debug.Log("Ei ole oikea item."); 
        return; 
    }
}
