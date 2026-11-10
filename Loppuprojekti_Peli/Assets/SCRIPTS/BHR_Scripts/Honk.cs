using UnityEngine;
using UnityEngine.InputSystem;

public class TukTukHonk : MonoBehaviour
{
    [Header("Ääniasetukset")]
    [SerializeField] private AudioSource honkAudioSource; // Raahaa tähän tööttäystä varten tehty Audio Source
    [SerializeField] private AudioClip honkClip;         // Tööttäysääni

    void Update()
    {

        if (Keyboard.current.eKey.isPressed)
        {
            if (honkAudioSource != null && honkClip != null && !honkAudioSource.isPlaying)
            {
                honkAudioSource.PlayOneShot(honkClip);
            }
        }
    }
}
