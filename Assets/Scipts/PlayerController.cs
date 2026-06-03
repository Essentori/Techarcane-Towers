using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    enum CameraState
    {
        FirstPersonView, //FPV for short
        TopDownView, //TDV for short
        Locked //Cinematic
    }
    CameraState cameraState;
    public Camera playerCamera;
    public Transform cameraObject;
    [Range(0, 1000)] public float mouseSensitivity = 200f;

    [Header("FPV settings")]
    public float baseFOV_FPV = 75f;
    [SerializeField][Range(-90f, 0f)] private float upCameraRotationClamp = -85f;
    [SerializeField][Range(0f, 90f)] private float downCameraRotationClamp = 80f;
    [Header("FPV head bob animation settings")]
    private bool jumpInputInPressed = false;
    private float jumpCooldown = 0.1f; //To avoid jolting and bugs
    private bool wasGroundedLastFrame = true;
    private float timeFalling; //To avoid jolting and bugs (can be used for fall damage calculation)
    private Tweener FOVTweener;
    private float targetFOV;
    [SerializeField][Range(1.1f, 2f)] private float FOVIncreaseMultiplier = 1.4f;
    private float FOVIncrease;
    [SerializeField][Range(0.5f, 5f)] private float fovIncreaseTime = 2f; //In seconds
    private float fovIncreaseSpeed;
    [SerializeField][Range(0.1f, 1f)] private float fovResetDuration = 0.15f;

    [Header("TDV settings")]
    public float baseFOV_TDV = 60f;

    [Header("Movement settings")]
    [Range(0f, 50f)] public float moveSpeed = 6f;
    [Range(0.5f, 30f)] public float jumpHeight = 2f;
    private float gravity = -10f;
    [SerializeField][Range(0, 10)] private float fallingGravityMultiplier = 2.5f;
    public bool inFlyMode = false;

    private CharacterController cController;
    private Vector3 velocity;
    private float cameraXRotation = 0f;

    private InputSystem_Actions inputHandler;
    private Vector2 moveInput;
    private Vector2 lookInput;
    void Awake()
    {
        cController = GetComponent<CharacterController>();
        inputHandler = new InputSystem_Actions();

        //Input System Actions subs for MOVE, applaying to moveInput
        inputHandler.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputHandler.Player.Move.canceled += context => moveInput = Vector2.zero;

        //Input System Actions subs for LOOK, applaying to lookInput
        inputHandler.Player.Look.performed += context => lookInput = context.ReadValue<Vector2>();
        inputHandler.Player.Look.canceled += context => lookInput = Vector2.zero;
    }
    void Start()
    {
        //Lock the cursor for first person
        Cursor.lockState = CursorLockMode.Locked;

        //Game starts with FPV
        cameraState = CameraState.FirstPersonView;
        playerCamera = cameraObject.GetComponent<Camera>();
        targetFOV = baseFOV_FPV;
        playerCamera.fieldOfView = targetFOV;
        FOVIncrease = Mathf.Clamp(baseFOV_FPV * FOVIncreaseMultiplier, 60f, 140f);
        fovIncreaseSpeed = (FOVIncrease - baseFOV_FPV) / fovIncreaseTime;
    }
    void OnEnable() => inputHandler.Enable();
    void OnDisable() => inputHandler.Disable();

    void Update()
    {
        if (cameraState == CameraState.FirstPersonView)
        {
            LookHandle();
            MoveHandle();
        }
    }
    void LookHandle()
    {
        //Player mouse movement handlers
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        //Vertical movement (camera clamped rotation)
        cameraXRotation -= mouseY;
        cameraXRotation = Mathf.Clamp(cameraXRotation, upCameraRotationClamp, downCameraRotationClamp);
        cameraObject.localRotation = Quaternion.Euler(cameraXRotation, 0f, 0f);

        //Horizontal movement (player object rotation)
        transform.Rotate(Vector3.up * mouseX);
    }
    void MoveHandle()
    {
        //Different
        if (inFlyMode == false)
        {
            if (cController.isGrounded == false) timeFalling = Time.deltaTime;

            //Grounded check (upon landing) to avoid flying away or bounce
            if (cController.isGrounded && velocity.y < 0)
            {
                //Reset the velocity(applied by gravity) and stick to the ground
                velocity.y = -3f;
                timeFalling = 0;

                //Small head bob (down) and FOV reset on land (after jump or fall)
                if (!wasGroundedLastFrame)
                {
                    cameraObject.DOLocalMoveY(0.6f, 0.08f).OnComplete(() => cameraObject.DOLocalMoveY(0.8f, 0.15f));
                    targetFOV = baseFOV_FPV;
                    FOVTweener?.Kill();
                    FOVTweener = playerCamera.DOFieldOfView(baseFOV_FPV, fovResetDuration).SetEase(Ease.OutCubic);
                }
            }

            //Jump mechanic
            wasGroundedLastFrame = cController.isGrounded;
            jumpInputInPressed = inputHandler.Player.Jump.IsPressed(); //Check if Jump button is holded
            if (jumpInputInPressed && wasGroundedLastFrame && (timeFalling <= jumpCooldown))
            //Read as: if player is pressing Jump button, while they are not on jump cooldown and stand on the ground, then they can jump
            {
                //Small head bob (down) on jump
                cameraObject.DOLocalMoveY(0.7f, 0.1f).OnComplete(() => cameraObject.DOLocalMoveY(0.8f, 0.2f));
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            //Apllaying gravitation (gravity is stronger on falling for smoother jump expirience)
            float currentGravity = velocity.y < 1f ? gravity * fallingGravityMultiplier : gravity;
            velocity.y += currentGravity * Time.deltaTime;

            //Increase camera FOV when falling (with threshhold to avoid too much shacking on small falls)
            if (!cController.isGrounded && velocity.y < -9f)
            {
                FOVTweener?.Kill();
                targetFOV = FOVIncrease;
                playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, targetFOV, fovIncreaseSpeed * Time.deltaTime);
            }
        }

        //Relative movement mechanic
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        cController.Move(move * moveSpeed * Time.deltaTime);

        //Applaying velocity to player and move them
        cController.Move(velocity * Time.deltaTime);
    }

}
