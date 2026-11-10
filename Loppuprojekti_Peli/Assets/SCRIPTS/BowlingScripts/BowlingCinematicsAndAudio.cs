using UnityEngine;
using System.Collections;

public class BowlingCinematicsAndAudio : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera bowlingCamera;
    [SerializeField] private BowlingSystemNew bowlingBallSystem;

    [Header("Audio Channels")]
    [SerializeField] private AudioSource loopAudioSource; // Handles the release and rolling loops
    private AudioSource dedicatedEffectsSource;           // NEW: Handles the crash sound so it never cuts off

    [Header("Camera Tracking")]
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float returnSpeed = 3f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.8f, -3.5f);

    [Tooltip("The exact Z position where the camera should stop moving forward before the pins.")]
    [SerializeField] private float maxCamZPosition = -1.5f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip throwReleaseClip;
    [SerializeField] private AudioClip ballRollingLoopClip;
    [SerializeField] private AudioClip pinCollisionClip;

    private Vector3 initialCameraPos;
    private Quaternion initialCameraRot;
    private bool playedThrowSound = false;
    private bool playedCollisionSound = false;
    private bool isReturningHome = false;

    private void Start()
    {
        if (bowlingCamera != null)
        {
            initialCameraPos = bowlingCamera.transform.position;
            initialCameraRot = bowlingCamera.transform.rotation;
        }

        // Setup our rolling channel
        if (loopAudioSource == null)
        {
            loopAudioSource = gameObject.AddComponent<AudioSource>();
        }

       
        dedicatedEffectsSource = gameObject.AddComponent<AudioSource>();
        dedicatedEffectsSource.spatialBlend = 0f; // Forces it to be crisp 2D audio (full volume everywhere)
    }

    private void Update()
    {
        if (bowlingBallSystem == null) return;

        if (bowlingBallSystem.CurrentState == BowlingSystemNew.BowlingState.Rolling)
        {
            isReturningHome = false;

            if (!playedThrowSound)
            {
                PlayOneShotAudio(throwReleaseClip);
                playedThrowSound = true;

                if (ballRollingLoopClip != null)
                {
                    loopAudioSource.clip = ballRollingLoopClip;
                    loopAudioSource.loop = true;
                    loopAudioSource.Play();
                }
            }
        }
        else
        {
            if (playedThrowSound && !isReturningHome)
            {
                StartCoroutine(SmoothReturnHomeRoutine());
            }
        }
    }

    private void LateUpdate()
    {
        if (bowlingBallSystem == null || bowlingCamera == null) return;

        if (bowlingBallSystem.CurrentState == BowlingSystemNew.BowlingState.Rolling)
        {
            Vector3 targetPosition = bowlingBallSystem.transform.position + cameraOffset;
            targetPosition.y = initialCameraPos.y;
            targetPosition.x = initialCameraPos.x;

            if (targetPosition.z > maxCamZPosition)
            {
                targetPosition.z = maxCamZPosition;
            }

            bowlingCamera.transform.position = Vector3.Lerp(bowlingCamera.transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pin") && !playedCollisionSound)
        {
            
            loopAudioSource.loop = false;
            loopAudioSource.Stop();

            
            if (pinCollisionClip != null && dedicatedEffectsSource != null)
            {
                dedicatedEffectsSource.PlayOneShot(pinCollisionClip);
            }

            playedCollisionSound = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PitTrigger" || other.CompareTag("PitTrigger"))
        {
            loopAudioSource.loop = false;
            loopAudioSource.Stop();
        }
    }

    private void ResetCinematicState()
    {
        
    }

    private IEnumerator SmoothReturnHomeRoutine()
    {
        isReturningHome = true;

        loopAudioSource.loop = false;
        loopAudioSource.Stop();

        while (isReturningHome && Vector3.Distance(bowlingCamera.transform.position, initialCameraPos) > 0.01f)
        {
            bowlingCamera.transform.position = Vector3.Lerp(bowlingCamera.transform.position, initialCameraPos, Time.deltaTime * returnSpeed);
            bowlingCamera.transform.rotation = Quaternion.Slerp(bowlingCamera.transform.rotation, initialCameraRot, Time.deltaTime * returnSpeed);
            yield return null;
        }

        if (isReturningHome)
        {
            bowlingCamera.transform.position = initialCameraPos;
            bowlingCamera.transform.rotation = initialCameraRot;
        }

        playedThrowSound = false;
        playedCollisionSound = false;
    }

    private void PlayOneShotAudio(AudioClip clip)
    {
        if (clip != null && loopAudioSource != null)
        {
            loopAudioSource.PlayOneShot(clip);
        }
    }
}