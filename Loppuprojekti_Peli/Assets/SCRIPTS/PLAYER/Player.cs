using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5.5f;
    public float walkSpeed = 2.86f;
    public float acceleration = 10f;
    public float airAcceleration = 80f;
    public float airMaxSpeed = 3.5f;
    public float friction = 5f;
    public float stopSpeed = 1.65f;
    public float jumpForce = 6f;
    public float gravity = 20f;

    [Header("Mouse Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.05f;
    public float upDownRange = 85f;

    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public LayerMask interactableLayer;
    public float holdDuration = 1.0f;
    [Tooltip("Kuinka nopeasti latauspalkki valuu takaisin nollaan (esim. 2 = kaksinkertaisella nopeudella)")]
    public float drainSpeed = 2.0f;

    [Header("UI References")]
    public Image crosshairImage;
    public Color normalCrosshairColor = Color.white;
    public Color interactCrosshairColor = Color.green;
    public Slider interactSlider;

    [Header("Inventory Settings")]
    private InventoryItemData[] inventory = new InventoryItemData[3];
    private int currentSlot = 0;

    [Header("Inventory UI References")]
    [Tooltip("Raahaa t‰h‰n Slot1, Slot2 ja Slot3 objektien p‰‰-Imaget")]
    public Image[] slotBackgrounds = new Image[3];
    [Tooltip("Raahaa t‰h‰n Slot-objektien alta lˆytyv‰t lapsi-Imaget (Icon)")]
    public Image[] slotIcons = new Image[3];

    public Color activeSlotColor = new Color(1f, 0.6f, 0f, 0.8f);
    public Color inactiveSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    // Komponentit ja sis‰iset muuttujat
    private CharacterController controller;
    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 wishDir;
    private float currentMaxSpeed;
    private float verticalRotation = 0f;
    private bool isJumpPressed;
    private bool isWalking;

   

    // Interaktio-optimointi ja hold-valutus
    private bool isHoldingInteract;
    private float interactTimer;
    private InteractableItem currentTargetItem;
    private Transform lastHitTransform;
    private bool hasSlider;
    private bool hasCrosshair;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Etsit‰‰n skenest‰ "InventoryBar" niminen paneeli dynaamisesti
        GameObject invBar = GameObject.Find("InventoryBar");
        if (invBar != null)
        {
            slotBackgrounds = new Image[3];
            slotIcons = new Image[3];

            for (int i = 0; i < 3; i++)
            {
                Transform slotTransform = invBar.transform.Find($"Slot{i + 1}");
                if (slotTransform != null)
                {
                    slotBackgrounds[i] = slotTransform.GetComponent<Image>();

                    Transform iconTransform = slotTransform.Find("Icon");
                    if (iconTransform != null)
                    {
                        slotIcons[i] = iconTransform.GetComponent<Image>();
                    }
                }
            }
        }

        hasSlider = interactSlider != null;
        hasCrosshair = crosshairImage != null;

        // P‰ivitet‰‰n UI heti alussa
        UpdateInventoryUI();
    }

    void Update()
    {
        HandleMouseLook();
        UpdateCrosshairAndHold();
        CalculateWishDirection();


    }

    void FixedUpdate()
    {
        HandleMovement();
    }


    public void LaunchPlayer(Vector3 launchVelocity)
    {
        velocity.x = (velocity.x * 0.4f) + launchVelocity.x;
        velocity.y = launchVelocity.y;
        velocity.z = (velocity.z * 0.4f) + launchVelocity.z;
    }

    // --- INPUT SYSTEM VIESTIT ---
    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }
    public void OnJump(InputValue value) { isJumpPressed = value.isPressed; }
    public void OnWalk(InputValue value) { isWalking = value.isPressed; }

    private void UpdateCrosshairAndHold()
    {
        if (cameraTransform == null) return;

        if (Keyboard.current != null)
        {
            isHoldingInteract = Keyboard.current.eKey.isPressed;
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        bool hitSomething = interactableLayer != 0
            ? Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer)
            : Physics.Raycast(ray, out hit, interactRange);

        InteractableItem itemUnderCursor = null;

        if (hitSomething)
        {
            if (hit.transform != lastHitTransform)
            {
                lastHitTransform = hit.transform;
                currentTargetItem = hit.collider.GetComponent<InteractableItem>();
            }
            itemUnderCursor = currentTargetItem;
        }
        else
        {
            lastHitTransform = null;
            currentTargetItem = null;
        }

        if (hasCrosshair)
        {
            crosshairImage.color = (itemUnderCursor != null) ? interactCrosshairColor : normalCrosshairColor;
        }

        if (itemUnderCursor != null && isHoldingInteract)
        {
            if (hasSlider && !interactSlider.gameObject.activeSelf)
            {
                interactSlider.gameObject.SetActive(true);
            }

            interactTimer += Time.deltaTime;

            if (hasSlider)
            {
                interactSlider.value = interactTimer / holdDuration;
            }

            if (interactTimer >= holdDuration)
            {
                itemUnderCursor.Interact(this);
                ResetInteractHold();
            }
        }
        else
        {
            if (interactTimer > 0f)
            {
                interactTimer -= Time.deltaTime * drainSpeed;

                if (hasSlider)
                {
                    interactSlider.value = interactTimer / holdDuration;
                }

                if (interactTimer <= 0f)
                {
                    ResetInteractHold();
                }
            }
        }
    }

    private void ResetInteractHold()
    {
        interactTimer = 0f;
        lastHitTransform = null;
        currentTargetItem = null;
        isHoldingInteract = false;

        if (hasSlider)
        {
            interactSlider.value = 0f;
            interactSlider.gameObject.SetActive(false);
        }
    }

    // --- LIIKKUMISLASKENTA ---
    private void CalculateWishDirection()
    {
        wishDir = transform.forward * moveInput.y + transform.right * moveInput.x;
        wishDir.y = 0;
        wishDir.Normalize();
        currentMaxSpeed = isWalking ? walkSpeed : maxSpeed;
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            float wishSpeed = wishDir.magnitude * currentMaxSpeed;

            ApplyFriction();
            Accelerate(wishDir, wishSpeed, acceleration);

            if (isJumpPressed)
            {
                velocity.y = jumpForce;
                isJumpPressed = false;
            }
        }
        else
        {
            float wishSpeed = wishDir.magnitude * maxSpeed;
            velocity.y -= gravity * Time.fixedDeltaTime;
            Accelerate(wishDir, wishSpeed, airAcceleration);
        }

        controller.Move(velocity * Time.fixedDeltaTime);
    }

    private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
    {
        float currentSpeed = Vector3.Dot(velocity, targetDir);
        float speedCap = controller.isGrounded ? targetSpeed : airMaxSpeed;
        float addSpeed = speedCap - currentSpeed;

        if (addSpeed <= 0) return;

        float accelSpeed = accel * Time.fixedDeltaTime * targetSpeed;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        velocity.x += accelSpeed * targetDir.x;
        velocity.z += accelSpeed * targetDir.z;
    }

    private void ApplyFriction()
    {
        Vector3 speedVector = velocity;
        speedVector.y = 0;
        float speed = speedVector.magnitude;

        if (speed < 0.001f) return;

        float control = (speed < stopSpeed) ? stopSpeed : speed;
        float drop = control * friction * Time.fixedDeltaTime;

        float newSpeed = speed - drop;
        if (newSpeed < 0) newSpeed = 0;

        if (speed > 0)
            newSpeed /= speed;

        velocity.x *= newSpeed;
        velocity.z *= newSpeed;
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (slotBackgrounds[i] != null)
            {
                slotBackgrounds[i].color = (i == currentSlot) ? activeSlotColor : inactiveSlotColor;
                slotBackgrounds[i].transform.localScale = (i == currentSlot) ? new Vector3(1.1f, 1.1f, 1f) : Vector3.one;
            }

            if (slotIcons[i] != null)
            {
                if (inventory[i] != null && inventory[i].itemIcon != null)
                {

                    slotIcons[i].sprite = inventory[i].itemIcon;
                    slotIcons[i].color = Color.white;
                }
                else
                {
                    slotIcons[i].sprite = null;
                    slotIcons[i].color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }
    public void AddItemToInventory(InventoryItemData item)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = item;
                Debug.Log($"Lis‰tty reppuun paikkaan {i + 1}: {item.itemName}");
                UpdateInventoryUI();
                return;
            }
        }
        inventory[currentSlot] = item;
        Debug.Log($"Reppu t‰ynn‰! Korvattu k‰dess‰ ollut paikka esineell‰: {item.itemName}");
        UpdateInventoryUI();
    }
    public void OnSwitchSlot1(InputValue value) { if (value.isPressed) SwitchSlot(0); }
    public void OnSwitchSlot2(InputValue value) { if (value.isPressed) SwitchSlot(1); }
    public void OnSwitchSlot3(InputValue value) { if (value.isPressed) SwitchSlot(2); }
    public void OnUseItem(InputValue value)
    {
        if (!value.isPressed) return;
        if (inventory[currentSlot] != null)
        {
            inventory[currentSlot].UseItem(this);
            inventory[currentSlot] = null;
            Debug.Log($"Esine k‰ytetty ja kulutettu paikasta {currentSlot + 1}.");
            UpdateInventoryUI();
        }
        else
        {
            Debug.Log("K‰si on tyhj‰, ei mit‰‰n k‰ytett‰v‰‰.");
        }
    }
    private void SwitchSlot(int newSlot)
    {
        currentSlot = newSlot;
        string currentItemName = inventory[currentSlot] != null ? inventory[currentSlot].itemName : "Tyhj‰";
        Debug.Log($"Vaihdettu paikkaan {currentSlot + 1}. K‰dess‰: {currentItemName}");
        UpdateInventoryUI();
    }
    private void HandleMouseLook()
    {
        if (cameraTransform == null) return;
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 100f;
        transform.Rotate(Vector3.up * mouseX);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        lookInput = Vector2.zero;
    }
}