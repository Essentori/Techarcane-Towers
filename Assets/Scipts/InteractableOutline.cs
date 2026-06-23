using System.Collections.Generic;
using UnityEngine;

public class InteractableOutline : MonoBehaviour
{
    private uint _outlineLayer;
    private uint _defaultLayer;
    private readonly List<RendererCache> _cachedParts = new List<RendererCache>();

    private struct RendererCache
    {
        public MeshRenderer Renderer;
        public MeshFilter Filter;
    }

    private void Awake()
    {
        _outlineLayer = GameManager.Instance.Layers.OutlinedLayer;
        _defaultLayer = GameManager.Instance.Layers.DefaultRenderingLayer;
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false);
        foreach (var r in renderers)
        {
            MeshFilter f = r.GetComponent<MeshFilter>();
            if (f != null)
            {
                _cachedParts.Add(new RendererCache { Renderer = r, Filter = f });
            }
        }
    }
    public void ToggleOutline(bool show)
    {
        uint targetLayer = show ? _outlineLayer : _defaultLayer;

        foreach (var part in _cachedParts)
        {
            if (part.Renderer == null) continue;
            part.Renderer.renderingLayerMask = targetLayer;
            if (show && part.Filter != null)
            {
                OutlineFeature.SmoothMeshWithBuffer(part.Filter);
            }
        }
    }
}