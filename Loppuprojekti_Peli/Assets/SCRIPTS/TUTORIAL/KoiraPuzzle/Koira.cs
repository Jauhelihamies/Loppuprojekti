using UnityEngine;
using UnityEngine.Events;

public class Koira : MonoBehaviour
{
    //koiran parametrit
    private int winCondition = 2;
    private int nytMaara = 0;
    [SerializeField] private string checkName = "Battery";

    private bool saavutettu = false; //kun saavuttaa m‰‰r‰n, muuttuu trueksi.

    public UnityAction KoiraTehty;

    public void TriggerKoira(Player player)
    {
        //Koira tarkistaa, ett‰ playerill‰ on oikea item (patteri)
        if (player.GetItemWithItemName(checkName))
        {
            //jos on tarvittava m‰‰r‰ jo, kerro pelaajalle
            if (saavutettu)
            {
                Debug.Log("saavutit m‰‰r‰n jo.");
                return;
            }
            //true, ottaa itemin ja poistaa pelaajan inventaariosta
            nytMaara++;
            Debug.Log("itemi annettu");

            //tarkistaa ett‰ on Win condition eli tarvittava m‰‰r‰ itemeit‰
            if (nytMaara >= winCondition)
            {
                saavutettu = true;
                KoiraTehty.Invoke();
                Debug.Log("saavutettu m‰‰r‰");
            }
        }
        else
        {
            //false ilmoittaa pelaajalle ja palauttaa
            Debug.Log("Ei ole oikea item.");
        }
    }
}
