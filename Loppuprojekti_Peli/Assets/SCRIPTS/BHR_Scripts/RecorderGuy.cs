using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RecorderGuy : MonoBehaviour
{
    [System.Serializable]
    public struct CustomSound
    {
        public string soundName; // Optional: Just to help you organize in the inspector
        public AudioClip clip;
        [Range(0f, 1f)] public float volume;
    }

    [Header("Sound List")]
    [SerializeField] private CustomSound[] soundList;

    [Header("3D Sound Settings")]
    [Tooltip("0 is fully 2D, 1 is fully 3D")]
    [Range(0f, 1f)][SerializeField] private float spatialBlend = 1f;
    [SerializeField] private float min3DDistance = 1f;
    [SerializeField] private float max3DDistance = 50f;
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (soundList != null && soundList.Length > 0)
        {
            // Pick one random sound from the array
            int randomIndex = Random.Range(0, soundList.Length);
            CustomSound chosenSound = soundList[randomIndex];

            if (chosenSound.clip != null)
            {
                // Configure the AudioSource for 3D and looping
                audioSource.spatialBlend = spatialBlend;
                audioSource.minDistance = min3DDistance;
                audioSource.maxDistance = max3DDistance;
                audioSource.rolloffMode = rolloffMode;

                // Set the chosen audio settings
                audioSource.clip = chosenSound.clip;
                audioSource.volume = chosenSound.volume;

                // Force it to loop seamlessly
                audioSource.loop = true;
                audioSource.playOnAwake = false;

                // Start playing immediately
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"[RecorderGuy] The chosen random sound slot ({randomIndex}) has no AudioClip assigned!");
            }
        }
        else
        {
            Debug.LogWarning($"[RecorderGuy] No sounds assigned in the inspector on {gameObject.name}!");
        }
    }
}