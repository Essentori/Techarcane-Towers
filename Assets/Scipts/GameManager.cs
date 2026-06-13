using UnityEngine;
using UnityEngine.InputSystem;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static InputSystem_Actions Input { get; private set; }

    [Header("References")]
    [field: SerializeField] public CameraManager Camera { get; private set; }
    [field: SerializeField] public Camera MainCamera { get; private set; }
    [field: SerializeField] public MenuManager Menus { get; private set; }
    [field: SerializeField] public LayersHandler Layers { get; private set; }
    [field: SerializeField] public MaterialsHandler Materials { get; private set; }
    [field: SerializeField] public NameRandomizer NameRandomizer { get; private set; }
    [Header("Global References")]
    [field: SerializeField] public PlayerController Player { get; private set; }
    [field: SerializeField] public Material OutlineMaterial { get; private set; }
    [SerializeField] private GameObject _playerHUD;

    void Awake()
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

    public void SetStateForMenu(bool isGameSlowed)
    {
        if (isGameSlowed)
        {
            Player.State.SetMoveLock(true, "Menu");
            Player.State.SetLookLock(true, "Menu");
            Time.timeScale = 0.05f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Player.State.SetMoveLock(false, "Menu");
            Player.State.SetLookLock(false, "Menu");
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    public string GetInteractKeyName()
    {
        return Input != null ? Input.Player.Interact.GetBindingDisplayString() : "E";
    }

    void OnDestroy()
    {
        if (Instance == this && Input != null)
        {
            Input.Disable();
        }
    }
}