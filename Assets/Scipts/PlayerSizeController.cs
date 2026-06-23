using UnityEngine;

public class PlayerSizeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _capsuleVisual;

    [Header("Settings")]
    [SerializeField][Range(0.1f, 1f)] private float _crouchHeightPercentage = 0.6f;
    [SerializeField] private float _normalHeight = 2f;
    [SerializeField] private float _changeSpeed = 5f;

    private CharacterController _characterController;
    private PlayerController _playerHub;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerHub = GetComponent<PlayerController>();

        if (_normalHeight <= 0.1f) _normalHeight = 2f;
        if (_crouchHeightPercentage <= 0.05f) _crouchHeightPercentage = 0.6f;

        _characterController.height = _normalHeight;
        _characterController.center = new Vector3(0f, _normalHeight / 2f, 0f);

        SyncCapsuleSize();
    }

    void Update()
    {
        float targetHeight = _playerHub.State.IsCrouching ? (_normalHeight * _crouchHeightPercentage) : _normalHeight;

        if (!Mathf.Approximately(_characterController.height, targetHeight))
        {
            _characterController.height = Mathf.MoveTowards(_characterController.height, targetHeight, Time.deltaTime * _changeSpeed);
            _characterController.center = new Vector3(0f, _characterController.height / 2f, 0f);

            SyncCapsuleSize();
        }
    }

    private void SyncCapsuleSize()
    {
        float targetScaleY = _characterController.height / 2f;
        _capsuleVisual.localScale = new Vector3(_capsuleVisual.localScale.x, targetScaleY, _capsuleVisual.localScale.z);
        _capsuleVisual.localPosition = _characterController.center;
    }
}