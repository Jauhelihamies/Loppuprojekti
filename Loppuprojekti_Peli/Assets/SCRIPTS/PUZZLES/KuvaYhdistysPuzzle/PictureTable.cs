using UnityEngine;
using UnityEngine.UIElements;

public class PictureTable : MonoBehaviour
{
    [Header("Item Name of accepted item")]
    //mikä esine hyväksytään
    [SerializeField] private string acceptedItemName = "Picture";

    [Header("Pictures placements")]
    //tarkista onko paikassa jo kuva
    private bool spaceL = false;
    private bool spaceR = false;

    //kuvien paikat
    [SerializeField] Vector3 transL;
    Quaternion rotL;
    Vector3 transR;

    [Header("Pictures on the table")]
    //taulukko pitää kirjaa, mitkä kuvat on
    [SerializeField] private InventoryItemData[] picturesOnTable = { null, null };

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.TryGetComponent(out InventoryItemData item))
        
            if (!spaceL && !spaceR)
            {
                if (!spaceL)
                {
                    other.gameObject.transform.position = transL;
                    other.gameObject.transform.rotation = new Quaternion(90, 0, 0, 0);
                    spaceL = true;
                    Debug.Log("Paikka L");
                }
                else
                {
                    other.gameObject.transform.position = transR;
                    other.gameObject.transform.rotation = new Quaternion(90, 0, 0, 0);
                    spaceR = true;
                    Debug.Log("Paikka R");
                }
            }
        
    }

    /// <summary>
    /// SCRÄPÄTTY NYT TÄLLÄ HETKELLÄ!
    /// </summary>
    /// <param name="player"></param>
    public void PictureTableInteract(Player player)
    {
        //katso onko item oikea, eli kuva
        InventoryItemData newPicture = player.GetItemWithItemName(acceptedItemName);
        if (newPicture && !spaceL && !spaceR)
        {
            //tarkista ensin ensimmäinen paikka, ja jos on täytetty, tarkista toinen paikka
            if (!spaceL) 
            {
                picturesOnTable[0] = newPicture;
                
                spaceL = true;
                Debug.Log("Paikka L");
            }
            else if (!spaceR) 
            {
                picturesOnTable[1] = newPicture;
                spaceR = true;
                Debug.Log("Paikka R");
            }
        }
    }

    public bool IsPictureSolved()
    {
        return false;
    }
}
