using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsDragAndScroll : MonoBehaviour
{
    [Header("Raahausasetukset")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float dragForce = 20f;
    [SerializeField] private LayerMask draggableLayer;

    [Header("Palautusasetukset")]
    [SerializeField] private float minHeight = -15f; // Korkeus, jonka alapuolella objekti palautetaan
    [SerializeField] private float respawnDistance = 5f; // Etäisyys kamerasta palautettaessa

    private Camera mainCamera;
    private Rigidbody targetRigidbody;
    private float currentDistance;
    private float originalDrag;
    private float originalAngularDrag;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Tarkistetaan onko yksikään raahattava objekti pudonnut liian alas
        CheckOutOfBounds();

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 1. Aloitetaan raahaus
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryStartDrag(mouse.position.ReadValue());
        }

        // 2. Päivitetään etäisyyttä rullalla raahauksen aikana
        if (mouse.leftButton.isPressed && targetRigidbody != null)
        {
            float scrollInput = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                currentDistance += (scrollInput / 120f) * scrollSpeed;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            }
        }

        // 3. Lopetetaan raahaus
        if (mouse.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    private void FixedUpdate()
    {
        if (targetRigidbody == null) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePosition = mouse.position.ReadValue();
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, currentDistance);
        Vector3 targetWorldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        Vector3 direction = targetWorldPosition - targetRigidbody.position;
        targetRigidbody.linearVelocity = direction * dragForce;
    }

    // Tarkistaa kaikki scenen Rigidbodyt ja palauttaa pudonneet
    private void CheckOutOfBounds()
    {
        Rigidbody[] allRigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);

        foreach (Rigidbody rb in allRigidbodies)
        {
            // Tarkistetaan, kuuluuko objekti draggableLayer-tasoon
            if (((1 << rb.gameObject.layer) & draggableLayer) != 0)
            {
                // Jos objekti on pudonnut alle minHeight-rajan
                if (rb.position.y < minHeight)
                {
                    RespawnInFrontOfCamera(rb);
                }
            }
        }
    }

    // Siirtää objektin kameran eteen ja nollaa vauhdin
    private void RespawnInFrontOfCamera(Rigidbody rb)
    {
        // Jos pelaaja raahasi tätä objektia juuri, lopetetaan raahaus ensin
        if (targetRigidbody == rb)
        {
            EndDrag();
        }

        // Lasketaan sijainti suoraan kameran eteen
        Vector3 spawnPosition = mainCamera.transform.position + (mainCamera.transform.forward * respawnDistance);

        rb.position = spawnPosition;
        rb.linearVelocity = Vector3.zero; // Nollataan putoamisnopeus
        rb.angularVelocity = Vector3.zero; // Nollataan pyöriminen
    }

    private void TryStartDrag(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, draggableLayer))
        {
            if (hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                targetRigidbody = rb;
                originalDrag = targetRigidbody.linearDamping;
                originalAngularDrag = targetRigidbody.angularDamping;

                targetRigidbody.useGravity = false;
                targetRigidbody.linearDamping = 10f;
                targetRigidbody.angularDamping = 5f;

                currentDistance = Vector3.Distance(mainCamera.transform.position, targetRigidbody.position);
            }
        }
    }

    private void EndDrag()
    {
        if (targetRigidbody != null)
        {
            targetRigidbody.useGravity = true;
            targetRigidbody.linearDamping = originalDrag;
            targetRigidbody.angularDamping = originalAngularDrag;
            targetRigidbody = null;
        }
    }
}
