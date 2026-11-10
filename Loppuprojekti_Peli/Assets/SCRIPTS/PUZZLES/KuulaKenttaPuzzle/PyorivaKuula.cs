using UnityEngine;

public class PyorivaKuula : MonoBehaviour
{
    private Vector3 oldPostition;
    
    void Start()
    {
        oldPostition = transform.position;
    }

    /// <summary>
    /// resetoi kuulan takaisin alkuun.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = oldPostition;
    }
}
