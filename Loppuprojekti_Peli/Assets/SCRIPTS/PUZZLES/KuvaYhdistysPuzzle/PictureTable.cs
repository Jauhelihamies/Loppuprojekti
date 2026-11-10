using UnityEngine;

public class PictureTable : MonoBehaviour
{
    [Header("Item Name of accepted item")]
    //mik‰ esine hyv‰ksyt‰‰n
    [SerializeField] private string acceptedItemName = "Picture";

    [Header("Pictures placements")]
    //tarkista onko paikassa jo kuva
    private bool placeL = false;
    private bool placeR = false;

    //kuvien paikat
    Transform transL;
    Transform transR;

    [Header("Pictures on the table")]
    //taulukko pit‰‰ kirjaa, mitk‰ kuvat on
    [SerializeField] private InventoryItemData[] picturesOnTable = { null, null };

    public void PictureTableInteract(Player player)
    {
        //katso onko item oikea, eli kuva
        InventoryItemData newPicture = player.GetItemWithItemName(acceptedItemName);
        if (newPicture && !placeL && !placeR)
        {
            //tarkista ensin ensimm‰inen paikka, ja jos on t‰ytetty, tarkista toinen paikka
            if (!placeL) 
            {
                picturesOnTable[0] = newPicture;
                placeL = true;
                Debug.Log("Paikka L");
            }
            else if (!placeR) 
            {
                picturesOnTable[1] = newPicture;
                placeR = true;
                Debug.Log("Paikka R");
            }
        }
    }

    public bool IsPictureSolved()
    {
        return false;
    }
}
