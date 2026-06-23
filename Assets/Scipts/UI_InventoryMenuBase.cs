using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class UI_InventoryMenuBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum InventoryState { Disabled, Collapsed, Pinned }
    public enum ScreenEdge { Left, Right, Top, Bottom }

    [Header("Inventory State")]
    [SerializeField] private InventoryState _currentState;
    public InventoryState CurrentState => _currentState;

    [Header("References")]
    [SerializeField] protected RectTransform _movingContainer;

    [Header("Dynamic Layout Settings")]
    [SerializeField] protected ScreenEdge _screenEdge;
    [SerializeField][Range(0.01f, 1f)] protected float _visiblePercentageWhenCollapsed = 0.05f;
    [SerializeField] protected float _animationDuration = 0.3f;

    protected Vector2 _posExpanded;
    protected Vector2 _posCollapsed;
    protected Vector2 _posDisabled;

    [Header("Hover Intent Settings")]
    [SerializeField] private float _hoverDelay = 0.2f;

    private Tween _moveTween;
    private Coroutine _hoverCoroutine;

    protected virtual void Start()
    {
        CalculateDynamicPositions();
        SnapToState(InventoryState.Collapsed);
    }

    protected void CalculateDynamicPositions()
    {

        _posExpanded = _movingContainer.anchoredPosition;

        float width = _movingContainer.rect.width;
        float height = _movingContainer.rect.height;

        switch (_screenEdge)
        {
            case ScreenEdge.Left:
                _posCollapsed = _posExpanded + new Vector2(-width * (1f - _visiblePercentageWhenCollapsed), 0f);
                _posDisabled = _posExpanded + new Vector2(-width, 0f);
                break;

            case ScreenEdge.Right:
                _posCollapsed = _posExpanded + new Vector2(width * (1f - _visiblePercentageWhenCollapsed), 0f);
                _posDisabled = _posExpanded + new Vector2(width, 0f);
                break;

            case ScreenEdge.Bottom:
                _posCollapsed = _posExpanded + new Vector2(0f, -height * (1f - _visiblePercentageWhenCollapsed));
                _posDisabled = _posExpanded + new Vector2(0f, -height);
                break;

            case ScreenEdge.Top:
                _posCollapsed = _posExpanded + new Vector2(0f, height * (1f - _visiblePercentageWhenCollapsed));
                _posDisabled = _posExpanded + new Vector2(0f, height);
                break;
        }
    }

    public void SnapToState(InventoryState state)
    {
        _currentState = state;
        _moveTween?.Kill();
        StopHoverCoroutine();

        switch (_currentState)
        {
            case InventoryState.Disabled:
                _movingContainer.anchoredPosition = _posDisabled;
                _movingContainer.gameObject.SetActive(false);
                break;
            case InventoryState.Collapsed:
                _movingContainer.gameObject.SetActive(true);
                _movingContainer.anchoredPosition = _posCollapsed;
                break;
            case InventoryState.Pinned:
                _movingContainer.gameObject.SetActive(true);
                _movingContainer.anchoredPosition = _posExpanded;
                break;
        }
    }

    public void ChangeState(InventoryState newState)
    {
        if (_currentState == newState) return;
        _currentState = newState;

        _moveTween?.Kill();
        StopHoverCoroutine();

        if (_currentState != InventoryState.Disabled && !_movingContainer.gameObject.activeSelf)
        {
            _movingContainer.gameObject.SetActive(true);
        }

        switch (_currentState)
        {
            case InventoryState.Disabled:
                PlayDisabledAnimation();
                break;

            case InventoryState.Collapsed:
                PlayClosingAnimation();
                break;

            case InventoryState.Pinned:
                PlayOpeningAnimation();
                break;
        }
    }

    protected virtual void PlayOpeningAnimation()
    {
        _moveTween = _movingContainer
            .DOAnchorPos(_posExpanded, _animationDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    protected virtual void PlayClosingAnimation()
    {
        _moveTween = _movingContainer
            .DOAnchorPos(_posCollapsed, _animationDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void PlayDisabledAnimation()
    {
        Sequence disableSequence = DOTween.Sequence().SetUpdate(true);

        if (_screenEdge == ScreenEdge.Bottom)
            disableSequence.Append(_movingContainer.DOAnchorPosY(_movingContainer.anchoredPosition.y + 15f, 0.1f).SetEase(Ease.OutQuad));
        else if (_screenEdge == ScreenEdge.Left)
            disableSequence.Append(_movingContainer.DOAnchorPosX(_movingContainer.anchoredPosition.x + 15f, 0.1f).SetEase(Ease.OutQuad)); // Исправлено с .y на .x

        disableSequence.Append(_movingContainer.DOAnchorPos(_posDisabled, _animationDuration).SetEase(Ease.InQuad));

        disableSequence.OnComplete(() => {
            if (_currentState == InventoryState.Disabled)
            {
                _movingContainer.gameObject.SetActive(false);
            }
        });

        _moveTween = disableSequence;
    }

    #region Pointer Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentState == InventoryState.Collapsed && Cursor.visible)
        {
            StopHoverCoroutine();
            _hoverCoroutine = StartCoroutine(WaitBeforeExpandRoutine());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHoverCoroutine();
        if (_currentState == InventoryState.Collapsed)
        {
            _moveTween?.Kill();
            PlayClosingAnimation();
        }
    }

    private IEnumerator WaitBeforeExpandRoutine()
    {
        yield return new WaitForSecondsRealtime(_hoverDelay);
        PlayOpeningAnimation();
    }

    private void StopHoverCoroutine()
    {
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
    }

    #endregion

    protected virtual void OnDisable() => StopHoverCoroutine();
}