using UnityEngine;
using UnityEngine.Events;

public class KenttaLoppu : MonoBehaviour
{
    public UnityEvent onPuzzleSolved;

    private void OnTriggerEnter(Collider other)
    {
        //katsoo onko pyörivä kuula tullut
        if (other.gameObject.TryGetComponent(out PyorivaKuula kuula))
        {
            //jos on, "poista" ja anna reward kuula
            other.gameObject.SetActive(false);
            onPuzzleSolved.Invoke();
        }
    }
}
