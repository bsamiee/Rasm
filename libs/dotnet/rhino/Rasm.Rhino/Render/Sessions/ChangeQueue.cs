using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Shapes;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using ChangeQueue = Rhino.Render.ChangeQueue;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SceneBatch {
    public static SceneBatch Empty { get; } = new();

    public Option<ViewportInfo> View { get; init; }
    public Option<LinearWorkflow> Workflow { get; init; }
    public HashMap<uint, Transform> Transforms { get; init; } = HashMap<uint, Transform>();
    public HashMap<Guid, Light> DynamicLights { get; init; } = HashMap<Guid, Light>();
    public Option<RenderSettings> Settings { get; init; }
    public HashMap<RenderSettings.EnvironmentUsage, uint> Environments { get; init; } = HashMap<RenderSettings.EnvironmentUsage, uint>();
    public Option<SceneSkylight> Skylight { get; init; }
    public Option<Light> Sun { get; init; }
    public HashMap<Guid, Option<SceneLight>> Lights { get; init; } = HashMap<Guid, Option<SceneLight>>();
    public HashMap<uint, uint> Materials { get; init; } = HashMap<uint, uint>();
    public HashMap<Guid, Option<SceneMesh>> Meshes { get; init; } = HashMap<Guid, Option<SceneMesh>>();
    public HashMap<uint, Option<SceneInstance>> Instances { get; init; } = HashMap<uint, Option<SceneInstance>>();
    public Option<SceneGround> Ground { get; init; }
    public HashMap<Guid, Option<SceneClip>> ClippingPlanes { get; init; } = HashMap<Guid, Option<SceneClip>>();
    public Option<SceneDisplay> Display { get; init; }

    public SceneBatch Combine(SceneBatch later) =>
        new() {
            View = later.View | View,
            Workflow = later.Workflow | Workflow,
            Transforms = Transforms.AddOrUpdateRange(later.Transforms.AsIterable()),
            DynamicLights = DynamicLights.AddOrUpdateRange(later.DynamicLights.AsIterable()),
            Settings = later.Settings | Settings,
            Environments = Environments.AddOrUpdateRange(later.Environments.AsIterable()),
            Skylight = later.Skylight | Skylight,
            Sun = later.Sun | Sun,
            Lights = Lights.AddOrUpdateRange(later.Lights.AsIterable()),
            Materials = Materials.AddOrUpdateRange(later.Materials.AsIterable()),
            Meshes = Meshes.AddOrUpdateRange(later.Meshes.AsIterable()),
            Instances = Instances.AddOrUpdateRange(later.Instances.AsIterable()),
            Ground = later.Ground | Ground,
            ClippingPlanes = ClippingPlanes.AddOrUpdateRange(later.ClippingPlanes.AsIterable()),
            Display = later.Display | Display,
        };
}

public sealed record SceneLight(Guid Id, Light Data, Option<uint> MaterialId);

public sealed record SceneMesh(Guid Id, Mesh Mesh, Seq<SceneMapping> Mappings, Transform OcsTransform, Option<ObjectAttributes> Attributes, Option<GeometryBase> Original);

public sealed record SceneMapping(int Channel, Transform Local, Option<TextureMapping> Mapping);

public sealed record SceneInstance(uint InstanceId, Guid MeshId, uint MaterialId, Transform Transform, Option<Guid> RootId, Option<Guid> ParentId, Option<ObjectAttributes> Attributes, Seq<SceneAncestor> Ancestry);

public sealed record SceneAncestor(Guid ReferenceId, Guid DefinitionId, Option<ObjectAttributes> ReferenceAttributes, Transform Transform);

public sealed record SceneGround(bool Enabled, double Altitude, bool IsShadowOnly, bool ShowUnderside, uint MaterialId, Vector2d TextureScale, Vector2d TextureOffset, double TextureRotation);

public sealed record SceneSkylight(bool Enabled, bool UsesCustomEnvironment);

public sealed record SceneClip(Guid Id, bool IsEnabled, Plane Plane, Seq<Guid> ViewIds, Option<ObjectAttributes> Attributes);

public sealed record SceneDisplay(Guid Id, int RealtimeRenderPasses);

public sealed record QueuePolicy(bool RespectDisplayAttributes, bool NotifyChanges, bool OriginalObjects, Option<ChangeQueue.ChangeQueue.BakingFunctions> Bake) {
    public static QueuePolicy Viewport { get; } = new(RespectDisplayAttributes: true, NotifyChanges: true, OriginalObjects: false, Bake: None);
    public static QueuePolicy Capture { get; } = Viewport with { NotifyChanges = false };
    public static QueuePolicy Render { get; } = Capture with { RespectDisplayAttributes = false };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedChangeQueue : ChangeQueue.ChangeQueue {
    // --- [ACQUISITION]
    private readonly IPlugInSink sink;
    private readonly QueuePolicy policy;
    private readonly Option<IO<Unit>> wake;

    private DefinedChangeQueue(PlugIn plugIn, RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(plugIn.Id, document.RuntimeSerialNumber, view, attributes.ValueUnsafe(), policy.RespectDisplayAttributes, policy.NotifyChanges) =>
        (sink, this.policy, this.wake) = (IPlugInSink.Of(plugIn), policy, wake);

    private DefinedChangeQueue(PlugIn plugIn, CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(plugIn.Id, preview) =>
        (sink, this.policy, this.wake) = (IPlugInSink.Of(plugIn), policy, wake);

    public static IO<DefinedChangeQueue> Of(PlugIn plugIn, RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake) =>
        IO.lift(() => new DefinedChangeQueue(plugIn, document, view, attributes, policy, wake));

    public static IO<DefinedChangeQueue> Of(PlugIn plugIn, CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake) =>
        IO.lift(() => new DefinedChangeQueue(plugIn, preview, policy, wake));

    // --- [FEED]
    private readonly Atom<SceneFeed> feed = Atom(SceneFeed.Empty);

    public IO<Unit> World => IO.lift(() => CreateWorld(bFlushWhenReady: false)).Bind(_ => Drain);

    public IO<SceneBatch> Take => feed.SwapIO(static held => held.Take()).Map(static held => held.Taken);

    private IO<Unit> Drain =>
        from flushed in IO.lift(Flush)
        from published in feed.SwapIO(static held => held.Publish())
        from woken in Signal
        select unit;

    private IO<Unit> Signal => wake.TraverseM(static signal => signal).As().Map(static _ => unit);

    private sealed record SceneFeed(SceneBatch Collecting, SceneBatch Pending, SceneBatch Taken) {
        public static SceneFeed Empty { get; } = new(SceneBatch.Empty, SceneBatch.Empty, SceneBatch.Empty);

        public SceneFeed Collect(SceneBatch change) => this with { Collecting = Collecting.Combine(change) };

        public SceneFeed Publish() => new(SceneBatch.Empty, Pending.Combine(Collecting), SceneBatch.Empty);

        public SceneFeed Take() => new(Collecting, SceneBatch.Empty, Pending);
    }

    // --- [CHANGES]
    protected override void ApplyViewChange(ViewInfo viewInfo) =>
        Collect(IO.lift(() => new SceneBatch { View = Some(new ViewportInfo(viewInfo.Viewport)) }));

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) =>
        Collect(IO.lift(() => new SceneBatch { Workflow = Some(new LinearWorkflow(lw)) }));

    protected override void ApplyDynamicObjectTransforms(List<ChangeQueue.DynamicObjectTransform> dynamicObjectTransforms) =>
        Collect(IO.lift(() => new SceneBatch { Transforms = new(toSeq(dynamicObjectTransforms).Map(static moved => (moved.MeshInstanceId, moved.Transform)), tryAdd: false) }));

    protected override void ApplyDynamicLightChanges(List<Light> dynamicLightChanges) =>
        Collect(DisposalOps.AcquireAll(toSeq(dynamicLightChanges).Map(Copies.Duplicate), DisposalOps.Release)
            .Map(static copies => new SceneBatch { DynamicLights = new(copies.Map(static copy => (copy.Id, copy)), tryAdd: false) }));

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) =>
        Collect(IO.lift(() => new SceneBatch { Settings = Some(new RenderSettings(rs)) }));

#pragma warning disable CS0618
    protected override void ApplyEnvironmentChanges(RenderEnvironment.Usage usage) =>
        Collect(IO.lift(() => new SceneBatch { Environments = [((RenderSettings.EnvironmentUsage)uint.TrailingZeroCount((uint)usage), EnvironmentIdForUsage(usage))] }));
#pragma warning restore CS0618

    protected override void ApplySkylightChanges(ChangeQueue.Skylight skylight) =>
        Collect(IO.lift(() => new SceneBatch { Skylight = Some(SceneMapper.ToSkylight(skylight)) }));

    protected override void ApplySunChanges(Light sun) =>
        Collect(Copies.Duplicate(sun).Map(static copy => new SceneBatch { Sun = Some(copy) }));

    protected override void ApplyLightChanges(List<ChangeQueue.Light> lightChanges) =>
        Collect(
            from converted in IO.lift(() => Optional(GetQueueView())).Bracket(
                Use: view => IO.lift(() => view.Iter(held => toSeq(lightChanges)
                    .Filter(static light => light.Data.CoordinateSystem == CoordinateSystem.Camera)
                    .Iter(light => ConvertCameraBasedLightToWorld(this, light, held)))),
                Fin: static view => DisposalOps.Release(view.ToSeq()))
            from rows in DisposalOps.AcquireAll(toSeq(lightChanges).Map(LightState), static rows => DisposalOps.Release(rows.Choose(static row => row.State.Map(static light => light.Data))))
            select new SceneBatch { Lights = new(rows, tryAdd: false) });

    private static IO<(Guid Id, Option<SceneLight> State)> LightState(ChangeQueue.Light light) =>
        light.ChangeType == ChangeQueue.Light.Event.Deleted
            ? IO.pure((light.Id, Option<SceneLight>.None))
            : Copies.Duplicate(light.Data).Map(copy => (light.Id, Some(SceneMapper.ToLight(light, copy))));

    protected override void ApplyMaterialChanges(List<ChangeQueue.Material> mats) =>
        Collect(IO.lift(() => new SceneBatch { Materials = new(toSeq(mats).Map(static material => (material.MeshInstanceId, material.Id)), tryAdd: false) }));

    protected override void ApplyMeshChanges(Guid[] deleted, List<ChangeQueue.Mesh> added) =>
        Collect(DisposalOps.AcquireAll(
                toSeq(added).Map(static mesh => Copies.Duplicate(mesh.SingleMesh).Map(copy => SceneMapper.ToMesh(mesh, mesh.Id(), copy))),
                static meshes => DisposalOps.Release(meshes.Map(static held => held.Mesh)))
            .Map(meshes => new SceneBatch {
                Meshes = new(toSeq(deleted).Map(static id => (id, Option<SceneMesh>.None)).Concat(meshes.Map(static mesh => (mesh.Id, Some(mesh)))), tryAdd: false),
            }));

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<ChangeQueue.MeshInstance> addedOrChanged) =>
        Collect(toSeq(addedOrChanged).TraverseM(Instance).As().Map(instances => new SceneBatch {
            Instances = new(toSeq(deleted).Map(static id => (id, Option<SceneInstance>.None)).Concat(instances.Map(static instance => (instance.InstanceId, Some(instance)))), tryAdd: false),
        }));

    private static IO<SceneInstance> Instance(ChangeQueue.MeshInstance instance) =>
        IO.lift(() => toSeq(instance.Ancestry)).Bracket(Use: _ => IO.lift(() => SceneMapper.ToInstance(instance)), Fin: DisposalOps.Release);

    protected override void ApplyGroundPlaneChanges(ChangeQueue.GroundPlane gp) =>
        Collect(IO.lift(() => new SceneBatch { Ground = Some(SceneMapper.ToGround(gp)) }));

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<ChangeQueue.ClippingPlane> addedOrModified) =>
        Collect(IO.lift(() => new SceneBatch {
            ClippingPlanes = new(toSeq(deleted).Map(static id => (id, Option<SceneClip>.None)).Concat(toSeq(addedOrModified).Map(SceneMapper.ToClip).Map(static clip => (clip.Id, Some(clip)))), tryAdd: false),
        }));

    protected override void ApplyDynamicClippingPlaneChanges(List<ChangeQueue.ClippingPlane> changed) =>
        Collect(IO.lift(() => new SceneBatch { ClippingPlanes = new(toSeq(changed).Map(SceneMapper.ToClip).Map(static clip => (clip.Id, Some(clip))), tryAdd: false) }));

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) =>
        Collect(IO.lift(() => new SceneBatch { Display = Some(SceneMapper.ToDisplay(displayPipelineAttributes)) }));

    // --- [NOTICES]
    protected override void NotifyBeginUpdates() => Answer(Signal);

    protected override void NotifyEndUpdates() => Answer(Drain);

    protected override void NotifyDynamicUpdatesAreAvailable() => Answer(Drain);

    // --- [POLICY]
    protected override BakingFunctions BakeFor() => policy.Bake.IfNone(base.BakeFor);

    protected override bool ProvideOriginalObject() => policy.OriginalObjects;

    // --- [ANSWERS]
    private void Collect(IO<SceneBatch> change, [CallerMemberName] string member = "") =>
        Answer(change.Bind(batch => feed.SwapIO(held => held.Collect(batch))).Map(static _ => unit), member);

    private void Answer(IO<Unit> effect, [CallerMemberName] string member = "") =>
        _ = Callbacks.Answer(effect, static () => unit, new CallbackSite(sink, GetType(), member));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SceneMapper {
    public static partial SceneLight ToLight(ChangeQueue.Light light, Light data);

    [MapProperty(nameof(ChangeQueue.Mesh.Object), nameof(SceneMesh.Original), Use = nameof(Original))]
    public static partial SceneMesh ToMesh(ChangeQueue.Mesh source, Guid id, Mesh mesh);

    [MapProperty(nameof(ChangeQueue.MeshInstance.ObjectAttributes), nameof(SceneInstance.Attributes), Use = nameof(Owned))]
    public static partial SceneInstance ToInstance(ChangeQueue.MeshInstance instance);

    public static partial SceneGround ToGround(ChangeQueue.GroundPlane ground);

    public static partial SceneSkylight ToSkylight(ChangeQueue.Skylight skylight);

    [MapProperty(nameof(ChangeQueue.ClippingPlane.ViewIds), nameof(SceneClip.ViewIds), Use = nameof(@Conversions.Rows))]
    public static partial SceneClip ToClip(ChangeQueue.ClippingPlane plane);

    public static partial SceneDisplay ToDisplay(DisplayPipelineAttributes attributes);

    private static partial SceneMapping ToMapping(ChangeQueue.MappingChannel channel);

    private static partial SceneAncestor ToAncestor(ChangeQueue.MeshInstance.AncestryRecord record);

    [UserMapping]
    private static Seq<SceneMapping> Mappings(ChangeQueue.MappingChannel?[]? channels) => Conversions.Rows(channels).Map(ToMapping).Strict();

    [UserMapping]
    private static Seq<SceneAncestor> Ancestors(ChangeQueue.MeshInstance.AncestryRecord?[]? records) => Conversions.Rows(records).Map(ToAncestor).Strict();

    [UserMapping]
    private static Option<ObjectAttributes> Copied(ObjectAttributes? attributes) => Optional(attributes).Map(static held => held.Duplicate());

    [UserMapping]
    private static Option<T> Owned<T>(T? value) where T : class => Optional(value);

    [UserMapping(Default = false)]
    private static Option<GeometryBase> Original(RhinoObject? source) => Optional(source).Bind(static held => Optional(held.Geometry.Duplicate()));
}
