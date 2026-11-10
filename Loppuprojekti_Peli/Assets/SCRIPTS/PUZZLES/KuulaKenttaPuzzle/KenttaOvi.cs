using UnityEngine;
using UnityEngine.InputSystem;

public class KenttaOvi : MonoBehaviour
{
    private bool open = false;
    private Vector3 oldPosition;

    private void Start()
    {
        //kopio objektin alkuperäinen paikka.
        oldPosition = gameObject.transform.position;
        Debug.Log(oldPosition);
    }

    /// <summary>
    /// avaa oven.
    /// </summary>
    public void Open()
    {
        //katsoo onko kiinni
        if (!open)
        {
            //avaa oven
            open = true;

            gameObject.transform.position = new Vector3(
                gameObject.transform.position.x,
                gameObject.transform.position.y + 1f,
                gameObject.transform.position.z);
        }
    }

    /// <summary>
    /// Resetoi oven takaisin kiinni.
    /// </summary>
    public void ResetDoor()
    {
        open = false;
        gameObject.transform.position = oldPosition;
    }
}
