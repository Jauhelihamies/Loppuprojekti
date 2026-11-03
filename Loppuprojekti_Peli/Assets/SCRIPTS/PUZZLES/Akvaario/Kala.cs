using UnityEngine;

/// <summary>
/// Kala, jota k‰ytet‰‰n akvaario puzzlessa. Scripti kertoo vain painon.
/// </summary>
public class Kala : MonoBehaviour
{
    [SerializeField] int paino;
    public int Paino { get => paino; private set => paino = value; }
}
