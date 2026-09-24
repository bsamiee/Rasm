using System.Threading.Channels;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rasm.Rhino.Render;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.ChangeQueue;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SceneChangeQueueOptions(
    bool ProvideOriginalObject,
    ChangeQueue.BakingFunctions BakeFor,
    Option<Func<Option<RhinoObject>, Option<RenderMaterial>, TextureType, IO<int>>> BakingSize,
    Option<Func<RenderContent, CrcRenderHashFlags, string, Option<LinearWorkflow>, IO<uint>>> ContentRenderHash,
    Action<Error> Reject);

public sealed record MeshBatch(Guid Id, Seq<global::Rhino.Geometry.Mesh> Meshes, Seq<(int Channel, Transform Local, TextureMapping Mapping)> Mappings);

public sealed record InstanceRow(
    uint InstanceId,
    Guid MeshId,
    Guid RootId,
    Guid ParentId,
    uint MaterialId,
    Transform Xform,
    bool CastShadows,
    bool ReceiveShadows);

public sealed record LightRow(Guid Id, uint IdCrc, uint MaterialId, global::Rhino.Render.ChangeQueue.Light.Event Change, global::Rhino.Geometry.Light Data);

public sealed record ClippingPlaneRow(Guid Id, bool Enabled, Plane Plane, Seq<Guid> ViewIds);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SceneChange {
    public sealed record ViewChanged(string Name, ViewportInfo Viewport) : SceneChange;

    public sealed record DynamicTransforms(Seq<(uint MeshInstanceId, Transform Xform)> Rows) : SceneChange;

    public sealed record DynamicLights(Seq<global::Rhino.Geometry.Light> Changed) : SceneChange;

    public sealed record Meshes(Seq<Guid> Deleted, Seq<MeshBatch> Added) : SceneChange;

    public sealed record Instances(Seq<uint> Deleted, Seq<InstanceRow> AddedOrChanged) : SceneChange;

    public sealed record SunChanged(global::Rhino.Geometry.Light Sun) : SceneChange;

    public sealed record SkylightChanged(SkylightState Skylight, bool UsesCustomEnvironment) : SceneChange;

    public sealed record Lights(Seq<LightRow> Rows) : SceneChange;

    public sealed record Materials(Seq<(uint Id, uint MeshInstanceId)> Rows) : SceneChange;

    public sealed record RenderSettingsChanged() : SceneChange;

    public sealed record DisplayRenderSettingsChanged() : SceneChange;

    public sealed record GroundPlaneChanged(
        bool Enabled,
        bool ShadowOnly,
        bool ShowUnderside,
        double Altitude,
        double TextureRotation,
        uint MaterialId,
        Vector2d TextureScale,
        Vector2d TextureOffset,
        uint Crc) : SceneChange;

    public sealed record ClippingPlanes(Seq<Guid> Deleted, Seq<ClippingPlaneRow> AddedOrModified) : SceneChange;

    public sealed record DynamicClippingPlanes(Seq<ClippingPlaneRow> Changed) : SceneChange;

    public sealed record LinearWorkflowChanged(LinearWorkflowState Workflow) : SceneChange;

    public sealed record AttributesChanged() : SceneChange;

    public sealed record DynamicUpdatesAvailable() : SceneChange;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneChangeQueue : ChangeQueue {
    private readonly Channel<Seq<SceneChange>> changes;

    private readonly SceneChangeQueueOptions options;

    private Seq<SceneChange> staged = Seq<SceneChange>();

    public SceneChangeQueue(Guid pluginId, RhinoDoc doc, ViewInfo view, Option<DisplayPipelineAttributes> attributes, bool respectAttributes, bool notifyChanges, SceneChangeQueueOptions options, ChannelMode mode)
        : base(pluginId, doc.RuntimeSerialNumber, view, attributes.ValueUnsafe(), respectAttributes, notifyChanges) =>
        (this.options, changes) = (options, Created(mode));

    public SceneChangeQueue(Guid pluginId, CreatePreviewEventArgs args, SceneChangeQueueOptions options, ChannelMode mode)
        : base(pluginId, args) =>
        (this.options, changes) = (options, Created(mode));

    public ChannelReader<Seq<SceneChange>> Changes => changes.Reader;

    protected override void ApplyViewChange(ViewInfo viewInfo) {
        base.ApplyViewChange(viewInfo);
        Stage(new SceneChange.ViewChanged(viewInfo.Name, new ViewportInfo(viewInfo.Viewport)));
    }

    protected override void ApplyDynamicObjectTransforms(List<DynamicObjectTransform> dynamicObjectTransforms) {
        base.ApplyDynamicObjectTransforms(dynamicObjectTransforms);
        Stage(new SceneChange.DynamicTransforms(toSeq(dynamicObjectTransforms).Map(static row => (row.MeshInstanceId, row.Transform)).Strict()));
    }

    protected override void ApplyDynamicLightChanges(List<global::Rhino.Geometry.Light> dynamicLightChanges) {
        base.ApplyDynamicLightChanges(dynamicLightChanges);
        _ = Answers.Answer(
            Disposal.AcquireAll(toSeq(dynamicLightChanges).Map(static light => DuplicateMode.Duplicate.Acquire(light)))
                .Bind(copies => IO.lift(() => Stage(new SceneChange.DynamicLights(copies.Map(static copy => copy.Value).Strict())))),
            options.Reject,
            unit);
    }

    protected override void ApplyMeshChanges(Guid[] deleted, List<global::Rhino.Render.ChangeQueue.Mesh> added) {
        base.ApplyMeshChanges(deleted, added);
        Stage(new SceneChange.Meshes(
            toSeq(deleted),
            toSeq(added)
                .Map(static mesh => new MeshBatch(
                    mesh.Id(),
                    toSeq(mesh.GetMeshes()).Choose(static geometry => Optional(geometry.DuplicateMesh())).Strict(),
                    toSeq(mesh.Mappings).Map(static channel => (channel.Channel, channel.Local, channel.Mapping)).Strict()))
                .Strict()));
    }

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<MeshInstance> addedOrChanged) {
        base.ApplyMeshInstanceChanges(deleted, addedOrChanged);
        Stage(new SceneChange.Instances(
            toSeq(deleted),
            toSeq(addedOrChanged)
                .Map(static instance => {
                    using ObjectAttributes attributes = instance.ObjectAttributes;
                    return SceneChangeMapper.ToRow(instance, attributes.CastsShadows, attributes.ReceivesShadows);
                })
                .Strict()));
    }

    protected override void ApplySunChanges(global::Rhino.Geometry.Light sun) {
        base.ApplySunChanges(sun);
        _ = Answers.Answer(DuplicateMode.Duplicate.Acquire(sun).Bind(copy => IO.lift(() => Stage(new SceneChange.SunChanged(copy.Value)))), options.Reject, unit);
    }

    protected override void ApplySkylightChanges(global::Rhino.Render.ChangeQueue.Skylight skylight) {
        base.ApplySkylightChanges(skylight);
        Stage(new SceneChange.SkylightChanged(SceneChangeMapper.ToState(skylight), skylight.UsesCustomEnvironment));
    }

    protected override void ApplyLightChanges(List<global::Rhino.Render.ChangeQueue.Light> lightChanges) {
        base.ApplyLightChanges(lightChanges);
        Seq<global::Rhino.Render.ChangeQueue.Light> changed = toSeq(lightChanges);
        _ = Answers.Answer(
            Disposal.AcquireAll(changed.Map(static light => DuplicateMode.Duplicate.Acquire(light.Data)))
                .Bind(copies => IO.lift(() => Stage(new SceneChange.Lights(changed.Zip(copies, static (light, copy) => new LightRow(light.Id, light.IdCrc, light.MaterialId, light.ChangeType, copy.Value)).Strict())))),
            options.Reject,
            unit);
    }

    protected override void ApplyMaterialChanges(List<global::Rhino.Render.ChangeQueue.Material> mats) {
        base.ApplyMaterialChanges(mats);
        Stage(new SceneChange.Materials(toSeq(mats).Map(static material => (material.Id, material.MeshInstanceId)).Strict()));
    }

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) {
        base.ApplyRenderSettingsChanges(rs);
        Stage(new SceneChange.RenderSettingsChanged());
    }

    protected override void ApplyRenderSettingsChanges(DisplayRenderSettings settings) {
        base.ApplyRenderSettingsChanges(settings);
        Stage(new SceneChange.DisplayRenderSettingsChanged());
    }

    protected override void ApplyGroundPlaneChanges(global::Rhino.Render.ChangeQueue.GroundPlane gp) {
        base.ApplyGroundPlaneChanges(gp);
        Stage(SceneChangeMapper.ToChange(gp));
    }

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<ClippingPlane> addedOrModified) {
        base.ApplyClippingPlaneChanges(deleted, addedOrModified);
        Stage(new SceneChange.ClippingPlanes(toSeq(deleted), toSeq(addedOrModified).Map(static plane => SceneChangeMapper.ToRow(plane)).Strict()));
    }

    protected override void ApplyDynamicClippingPlaneChanges(List<ClippingPlane> changed) {
        base.ApplyDynamicClippingPlaneChanges(changed);
        Stage(new SceneChange.DynamicClippingPlanes(toSeq(changed).Map(static plane => SceneChangeMapper.ToRow(plane)).Strict()));
    }

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) {
        base.ApplyLinearWorkflowChanges(lw);
        Stage(new SceneChange.LinearWorkflowChanged(SceneMapper.ToState(lw)));
    }

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) {
        base.ApplyDisplayPipelineAttributesChanges(displayPipelineAttributes);
        Stage(new SceneChange.AttributesChanged());
    }

    protected override void NotifyBeginUpdates() {
        base.NotifyBeginUpdates();
        _ = SceneChangeQueues.Release(Taken()).Run();
    }

    protected override void NotifyEndUpdates() {
        base.NotifyEndUpdates();
        Publish(Taken());
    }

    protected override void NotifyDynamicUpdatesAreAvailable() {
        base.NotifyDynamicUpdatesAreAvailable();
        Publish(Seq<SceneChange>(new SceneChange.DynamicUpdatesAvailable()));
    }

    protected override bool ProvideOriginalObject() =>
        options.ProvideOriginalObject;

    protected override BakingFunctions BakeFor() =>
        options.BakeFor;

    protected override int BakingSize(RhinoObject? ro, RenderMaterial? material, TextureType type) =>
        Answers.Answer(options.BakingSize.Map(size => size(Optional(ro), Optional(material), type)), options.Reject, () => base.BakingSize(ro, material, type));

    protected override uint ContentRenderHash(RenderContent content, CrcRenderHashFlags flags, string excludeParameterNames, LinearWorkflow? lw) =>
        Answers.Answer(options.ContentRenderHash.Map(hash => hash(content, flags, excludeParameterNames, Optional(lw))), options.Reject, () => base.ContentRenderHash(content, flags, excludeParameterNames, lw));

    protected override void Dispose(bool isDisposing) {
        if (isDisposing) {
            _ = changes.Writer.TryComplete();
            _ = SceneChangeQueues.Release(Taken()).Run();
        }
        base.Dispose(isDisposing);
    }

    private static Channel<Seq<SceneChange>> Created(ChannelMode mode) =>
        Deliveries.CreateChannel<Seq<SceneChange>>(mode, static dropped => SceneChangeQueues.Release(dropped).Run());

    private void Stage(SceneChange change) =>
        staged = staged.Add(change);

    private void Publish(Seq<SceneChange> batch) {
        if (!changes.Writer.TryWrite(batch))
            _ = SceneChangeQueues.Release(batch).Run();
    }

    private Seq<SceneChange> Taken() {
        Seq<SceneChange> batch = staged;
        staged = Seq<SceneChange>();
        return batch;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SceneChangeQueues {
    public static IO<TValue> WithView<TValue>(SceneChangeQueue queue, Func<ViewInfo, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Missing.Unless(queue.GetQueueView(), nameof(ChangeQueue.GetQueueView))), body);

    public static IO<Unit> Release(Seq<SceneChange> batch) =>
        Disposal.Release(batch
            .Bind(static change => change.Switch(
                viewChanged: static changed => Seq<IDisposable>(changed.Viewport),
                dynamicTransforms: static _ => Seq<IDisposable>(),
                dynamicLights: static lights => lights.Changed.Map<IDisposable>(static light => light),
                meshes: static meshes =>
                    from added in meshes.Added
                    from owned in added.Meshes.Map<IDisposable>(static mesh => mesh) + added.Mappings.Map<IDisposable>(static mapping => mapping.Mapping)
                    select owned,
                instances: static _ => Seq<IDisposable>(),
                sunChanged: static sun => Seq<IDisposable>(sun.Sun),
                skylightChanged: static _ => Seq<IDisposable>(),
                lights: static rows => rows.Rows.Map<IDisposable>(static row => row.Data),
                materials: static _ => Seq<IDisposable>(),
                renderSettingsChanged: static _ => Seq<IDisposable>(),
                displayRenderSettingsChanged: static _ => Seq<IDisposable>(),
                groundPlaneChanged: static _ => Seq<IDisposable>(),
                clippingPlanes: static _ => Seq<IDisposable>(),
                dynamicClippingPlanes: static _ => Seq<IDisposable>(),
                linearWorkflowChanged: static _ => Seq<IDisposable>(),
                attributesChanged: static _ => Seq<IDisposable>(),
                dynamicUpdatesAvailable: static _ => Seq<IDisposable>())));
}

[Mapper]
internal static partial class SceneChangeMapper {
    [MapProperty(nameof(MeshInstance.Transform), nameof(InstanceRow.Xform))]
    internal static partial InstanceRow ToRow(MeshInstance instance, bool castShadows, bool receiveShadows);

    [MapProperty(nameof(ClippingPlane.IsEnabled), nameof(ClippingPlaneRow.Enabled))]
    internal static partial ClippingPlaneRow ToRow(ClippingPlane plane);

    [MapProperty(nameof(global::Rhino.Render.ChangeQueue.GroundPlane.IsShadowOnly), nameof(SceneChange.GroundPlaneChanged.ShadowOnly))]
    internal static partial SceneChange.GroundPlaneChanged ToChange(global::Rhino.Render.ChangeQueue.GroundPlane gp);

    internal static partial SkylightState ToState(global::Rhino.Render.ChangeQueue.Skylight skylight);

    [UserMapping]
    private static Seq<Guid> ToSeq(List<Guid>? ids) => toSeq(ids);
}
