using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    // TODO: Make outline visible through all obstacles
    [Header("Settings")]
    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
    public Material outlineMaterial;
    public RenderingLayerMask renderingLayer;
    public int stencilRefValue = 2;

    [Header("Optimization Buffer")]
    [Tooltip("The anount of unique meshes stored in memory as smooth meshes")]
    public int maxCacheSize = 30;

    private OutlinePass _outlinePass;
    private static OutlineFeature _instance;
    private readonly Dictionary<Mesh, Mesh> _smoothedMeshCache = new Dictionary<Mesh, Mesh>();
    private readonly Queue<Mesh> _cacheOrder = new Queue<Mesh>();

    private class PassData
    {
        public RendererListHandle maskRendererList;
        public RendererListHandle outlineRendererList;
    }

    public override void Create()
    {
        _instance = this;
        _outlinePass = new OutlinePass(this);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (outlineMaterial == null || _outlinePass == null) return;
        renderer.EnqueuePass(_outlinePass);
    }
    public static void SmoothMeshWithBuffer(MeshFilter filter)
    {
        if (_instance == null || filter == null || filter.sharedMesh == null) return;

        Mesh originalMesh = filter.sharedMesh;
        if (_instance._smoothedMeshCache.TryGetValue(originalMesh, out Mesh smoothedMesh))
        {
            if (filter.sharedMesh != smoothedMesh)
            {
                filter.sharedMesh = smoothedMesh;
            }
            return;
        }

        if (_instance._smoothedMeshCache.ContainsValue(originalMesh)) return;

        if (_instance._smoothedMeshCache.Count >= _instance.maxCacheSize)
        {
            Mesh oldestOriginal = _instance._cacheOrder.Dequeue();
            if (_instance._smoothedMeshCache.TryGetValue(oldestOriginal, out Mesh oldestSmoothed))
            {
                if (oldestSmoothed != null) DestroyImmediate(oldestSmoothed);
                _instance._smoothedMeshCache.Remove(oldestOriginal);
            }
        }
        Mesh clonedMesh = Instantiate(originalMesh);
        BakeSmoothedNormals(clonedMesh);
        _instance._smoothedMeshCache[originalMesh] = clonedMesh;
        _instance._cacheOrder.Enqueue(originalMesh);
        filter.sharedMesh = clonedMesh;
    }

    private static void BakeSmoothedNormals(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector3[] smoothedNormals = new Vector3[vertices.Length];

        Dictionary<Vector3, Vector3> positionToNormal = new Dictionary<Vector3, Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            if (!positionToNormal.ContainsKey(vertex))
            {
                positionToNormal[vertex] = Vector3.zero;
            }
            positionToNormal[vertex] += normals[i];
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            smoothedNormals[i] = positionToNormal[vertices[i]].normalized;
        }

        mesh.SetUVs(1, smoothedNormals);
    }

    protected override void Dispose(bool disposing)
    {
        foreach (var mesh in _smoothedMeshCache.Values)
        {
            if (mesh != null) DestroyImmediate(mesh);
        }
        _smoothedMeshCache.Clear();
        _cacheOrder.Clear();
        if (_instance == this) _instance = null;
    }
    private class OutlinePass : ScriptableRenderPass
    {
        private readonly OutlineFeature _feature;
        private readonly ShaderTagId[] _shaderTags = new[]
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit")
        };

        public OutlinePass(OutlineFeature feature)
        {
            _feature = feature;
            renderPassEvent = _feature.injectionPoint;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("OutlineMaskPass", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);

                var stencilState = StencilState.defaultValue;
                stencilState.enabled = true;
                stencilState.SetCompareFunction(CompareFunction.Always);
                stencilState.SetPassOperation(StencilOp.Replace);
                stencilState.SetFailOperation(StencilOp.Replace);
                stencilState.SetZFailOperation(StencilOp.Replace);

                var blendState = BlendState.defaultValue;
                blendState.blendState0 = new RenderTargetBlendState(writeMask: (ColorWriteMask)0);

                var maskStateBlock = new RenderStateBlock(RenderStateMask.Stencil | RenderStateMask.Blend)
                {
                    stencilState = stencilState,
                    stencilReference = _feature.stencilRefValue,
                    blendState = blendState
                };

                var maskDesc = new RendererListDesc(_shaderTags, renderingData.cullResults, cameraData.camera)
                {
                    sortingCriteria = cameraData.defaultOpaqueSortFlags,
                    renderQueueRange = RenderQueueRange.all,
                    layerMask = -1,
                    renderingLayerMask = (uint)_feature.renderingLayer.value,
                    stateBlock = maskStateBlock
                };

                passData.maskRendererList = renderGraph.CreateRendererList(maskDesc);
                builder.UseRendererList(passData.maskRendererList);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.maskRendererList);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("OutlineDrawPass", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);

                var stencilState = StencilState.defaultValue;
                stencilState.enabled = true;
                stencilState.SetCompareFunction(CompareFunction.NotEqual);
                stencilState.SetPassOperation(StencilOp.Keep);
                stencilState.SetFailOperation(StencilOp.Keep);
                stencilState.SetZFailOperation(StencilOp.Keep);

                var outlineStateBlock = new RenderStateBlock(RenderStateMask.Stencil)
                {
                    stencilState = stencilState,
                    stencilReference = _feature.stencilRefValue
                };

                var outlineDesc = new RendererListDesc(_shaderTags, renderingData.cullResults, cameraData.camera)
                {
                    sortingCriteria = cameraData.defaultOpaqueSortFlags,
                    renderQueueRange = RenderQueueRange.all,
                    layerMask = -1,
                    renderingLayerMask = (uint)_feature.renderingLayer.value,
                    stateBlock = outlineStateBlock,
                    overrideMaterial = _feature.outlineMaterial,
                    overrideMaterialPassIndex = 0
                };

                passData.outlineRendererList = renderGraph.CreateRendererList(outlineDesc);
                builder.UseRendererList(passData.outlineRendererList);
                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.outlineRendererList);
                });
            }
        }
    }
}