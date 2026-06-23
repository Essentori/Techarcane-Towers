using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_ModulesInventoryMenu : UI_InventoryMenuBase
{
    public enum ModuleTabType
    {
        Conditions,
        Actions
    }

    [Serializable]
    public struct ModuleTabMapping
    {
        public ModuleTabType Type;
        public Button TabButton;
        public GameObject CategoryPanelContainer;
    }

    [Header("Modules Tabs Configuration")]
    [SerializeField] private ModuleTabMapping[] _tabs;
    [SerializeField] private RectTransform _tabsTransform;

    private Vector2 _tabsPositionExpanded = Vector2.zero;
    private Vector2 _tabsPositionCollapsed;

    private Tween _tabsMoveTween;

    protected void Awake()
    {
        _screenEdge = ScreenEdge.Bottom;

        foreach (var tab in _tabs)
        {
            ModuleTabType capturedType = tab.Type;
            if (tab.TabButton != null)
            {
                tab.TabButton.onClick.AddListener(() => SwitchToTab(capturedType));
            }
        }
    }

    protected override void Start()
    {
        _tabsPositionCollapsed = new Vector2(0f, -_tabsTransform.rect.height);

        base.Start();

        if (_tabs != null && _tabs.Length > 0)
        {
            SwitchToTab(_tabs[0].Type);
        }

        if (_tabsTransform != null)
        {
            _tabsTransform.anchoredPosition = 
                (CurrentState == InventoryState.Collapsed)? _tabsPositionCollapsed: _tabsPositionExpanded;
        }
    }

    protected override void PlayOpeningAnimation()
    {
        base.PlayOpeningAnimation();

        if (_tabsTransform != null)
        {
            _tabsMoveTween?.Kill();
            _tabsMoveTween = _tabsTransform
                .DOAnchorPos(_tabsPositionExpanded, _animationDuration / 1.5f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
    }

    protected override void PlayClosingAnimation()
    {
        base.PlayClosingAnimation();

        if (_tabsTransform != null)
        {
            _tabsMoveTween?.Kill();
            _tabsMoveTween = _tabsTransform
                .DOAnchorPos(_tabsPositionCollapsed, _animationDuration / 1.5f)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }
    }

    public void SwitchToTab(ModuleTabType type)
    {
        foreach (var tab in _tabs)
        {
            bool isTargetTab = tab.Type == type;

            if (tab.CategoryPanelContainer != null)
            {
                tab.CategoryPanelContainer.SetActive(isTargetTab);
            }

            if (tab.TabButton != null)
            {
                ColorBlock colors = tab.TabButton.colors;
                colors.normalColor = isTargetTab ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                tab.TabButton.colors = colors;
            }
        }
    }
}