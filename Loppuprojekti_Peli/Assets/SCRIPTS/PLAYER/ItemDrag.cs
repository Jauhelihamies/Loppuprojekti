using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicsDragAndScroll : MonoBehaviour
{
    [Header("Asetukset")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float dragForce = 20f; // Kuinka nopeasti objekti seuraa hiirt‰
    [SerializeField] private LayerMask draggableLayer;

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
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 1. Aloitetaan raahaus
        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryStartDrag(mouse.position.ReadValue());
        }

        // 2. P‰ivitet‰‰n et‰isyytt‰ rullalla raahauksen aikana
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
        // Fysiikkaliike tehd‰‰n aina FixedUpdate-metodissa
        if (targetRigidbody == null) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Lasketaan kohdepiste maailmassa (miss‰ hiiren "pit‰isi" olla)
        Vector2 mousePosition = mouse.position.ReadValue();
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, currentDistance);
        Vector3 targetWorldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

        // Lasketaan suunta ja et‰isyys objektista kohdepisteeseen
        Vector3 direction = targetWorldPosition - targetRigidbody.position;

        // Liikutetaan objektia fysiikkavoimalla kohti kohdetta (linearVelocity on uusi ja oikein Unity 6:ssa!)
        targetRigidbody.linearVelocity = direction * dragForce;
    }

    private void TryStartDrag(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, draggableLayer))
        {
            if (hit.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                targetRigidbody = rb;

                // KORJATTU: K‰ytet‰‰n .drag-muuttujaa linearDragin sijaan
                originalDrag = targetRigidbody.drag;
                originalAngularDrag = targetRigidbody.angularDrag;

                targetRigidbody.useGravity = false;
                targetRigidbody.drag = 10f; // Est‰‰ liiallisen heilahtelun
                targetRigidbody.angularDrag = 5f;

                // Lasketaan aloituset‰isyys
                currentDistance = Vector3.Distance(mainCamera.transform.position, targetRigidbody.position);
            }
        }
    }

    private void EndDrag()
    {
        if (targetRigidbody != null)
        {
            // Palautetaan objektin alkuper‰iset fysiikka-asetukset
            // KORJATTU: K‰ytet‰‰n .drag-muuttujaa linearDragin sijaan
            targetRigidbody.useGravity = true;
            targetRigidbody.drag = originalDrag;
            targetRigidbody.angularDrag = originalAngularDrag;

            targetRigidbody = null;
        }
    }
}
