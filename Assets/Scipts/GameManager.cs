using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // TODO: Split into 2 scripts: Manager (references only) and Controller (configurations and managing)
    // GLOBAL TODO: Remove "forgor" checks everywhere and leave essential null checks only
    public static GameManager Instance { get; private set; }
    public static InputSystem_Actions Input { get; private set; }

    [Header("References")]
    [field: SerializeField] public CameraStatesManager Camera { get; private set; }
    [field: SerializeField] public Camera MainCamera { get; private set; }
    [field: SerializeField] public MenuManager Menus { get; private set; }
    [field: SerializeField] public LayersHandler Layers { get; private set; }
    [field: SerializeField] public MaterialsHandler Materials { get; private set; }
    [field: SerializeField] public NameRandomizer NameRandomizer { get; private set; }

    [Header("Global References")]
    [field: SerializeField] public PlayerController Player { get; private set; }
    [field: SerializeField] public Transform PlayerSpawnPoint { get; private set; }
    [SerializeField] private GameObject _playerHUD;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Input = new InputSystem_Actions();
        Input.Enable();
    }
    private void Start()
    {
        InitializeDefaultPlayerState();
        Player.InputHandler.OnCursorUnlockChanged += HandleCursorUnlocked;
    }

    public void InitializeDefaultPlayerState()
    {
        Camera.SetState(CamState.FirstPersonView);
        Menus.CloseAllCurrentMenus();
        Player.State.ResetStates();
        SetStateForMenu(false);
    }

    private void OnDestroy()
    {
        Player.InputHandler.OnCursorUnlockChanged -= HandleCursorUnlocked;
        if (Instance == this && Input != null)
        {
            Input.Disable();
        }
    }
    // TODO: Remove this method from here (?) and make it more configurating (for non full screen menus)
    public void SetStateForMenu(bool menuIsOpened)
    {
        if (menuIsOpened)
        {
            Player.State.SetLockSource(MultLocked.Locked, true, "Menu");
            Player.State.SetLockSource(MultLocked.Look, true, "Menu");
            Camera.UpdateCursorState(true, "Menu");
            Time.timeScale = 0.05f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Player.State.SetLockSource(MultLocked.Locked, false, "Menu");
            Player.State.SetLockSource(MultLocked.Look, false, "Menu");
            Camera.UpdateCursorState(false, "Menu");
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
    public void HandleCursorUnlocked(bool isShown)
    {
        Player.State.SetLockSource(MultLocked.Look, isShown, "CursorShow");
        Camera.UpdateCursorState(isShown, "CursorShow");
    }

    public string GetInteractKeyName()
    {
        return Input != null ? Input.Player.Interact.GetBindingDisplayString() : "E";
    }
}