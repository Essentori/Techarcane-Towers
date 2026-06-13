using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UI_TowerSelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Reference")]
    public GameObject SlotTower;

    [Header("Scale Settings")]
    [SerializeField] private float _normalSize = 100f;
    [SerializeField] private float _hoverSize = 130f;
    [SerializeField] private float _duration = 0.15f;

    private LayoutElement _layoutElement;
    private Tween _sizeTween;
    private bool _isFocused;

    private void Awake()
    {
        _layoutElement = GetComponent<LayoutElement>();
        SetSize(_normalSize);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isFocused) AnimateSize(_hoverSize);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isFocused) AnimateSize(_normalSize);
    }
    public void SetFocus(bool focus)
    {
        _isFocused = focus;
        AnimateSize(_isFocused ? _hoverSize : _normalSize);
    }

    private void AnimateSize(float targetSize)
    {
        _sizeTween?.Kill();
        _sizeTween = DOTween.To(() => _layoutElement.preferredWidth, SetSize, targetSize, _duration)
            .SetEase(Ease.OutCubic);
    }

    private void SetSize(float size)
    {
        _layoutElement.preferredWidth = size;
        _layoutElement.preferredHeight = size;
    }
}
