using UnityEngine;

public class KenttaVaihdaSuunta : MonoBehaviour
{
    private bool open = false;
    private Vector3 oldPosition;

    private void Start()
    {
        //kopio objektin alkuper‰inen paikka.
        oldPosition = gameObject.transform.position;
        Debug.Log(oldPosition);
    }

    /// <summary>
    /// k‰‰nt‰‰ objektia
    /// </summary>
    public void TurnPiece()
    {
        gameObject.transform.Rotate(0f, 45f, 0f, Space.Self);
    }

    /// <summary>
    /// avaa ja sulkee objektin eli nostaa ja laittaa takaisin alas.
    /// </summary>
    public void CloseAndOpenMove()
    {
        if (!open)
        {
            gameObject.transform.position = new Vector3(
                gameObject.transform.position.x, 
                gameObject.transform.position.y + 1f,
                gameObject.transform.position.z);

            open = true;
        } else
        {
            gameObject.transform.position = oldPosition;
        }
    }
}
