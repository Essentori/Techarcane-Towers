using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera PlayerCamera;
    public Transform CameraPosition;

    [Header("Settings")]
    [SerializeField] private float cameraHeightPercentage = 0.9f;

    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerStatesManager State { get; private set; }
    public PlayerMovement Movement { get; private set; }
    public PlayerSizeController SizeController { get; private set; }
    public FPVCameraEffects CameraEffects { get; private set; }
    public PlayerInteraction Interaction { get; private set; }
    public PlayerFlyController FlyController { get; private set; }
    public BuildingModeController BuildingMode { get; private set; }
    [field: SerializeField] public PlayerInventory Inventory { get; private set; }

    public float LocalEyeHeight => Movement.Controller.height * cameraHeightPercentage;

    void Awake()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        State = GetComponent<PlayerStatesManager>();
        Movement = GetComponent<PlayerMovement>();
        SizeController = GetComponent<PlayerSizeController>();
        CameraEffects = GetComponent<FPVCameraEffects>();
        Interaction = GetComponent<PlayerInteraction>();
        FlyController = GetComponent<PlayerFlyController>();
        BuildingMode = GetComponent<BuildingModeController>();
        Inventory = GetComponent<PlayerInventory>();
    }
    void Start()
    {
        PlayerCamera = Camera.main;
        CameraPosition = Camera.main.gameObject.transform;
    }
    public Vector3 GetEyeWorldPosition() => transform.position + Vector3.up * LocalEyeHeight;

    public void SnapCameraBackToPlayer()
    {
        CameraPosition.SetParent(transform);
        CameraPosition.localPosition = new Vector3(0f, LocalEyeHeight, 0f);
        CameraPosition.localRotation = Quaternion.Euler(Movement.CameraXRotation, 0f, 0f);
    }
}