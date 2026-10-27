using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon.Modeling;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SceneBatch {
    public static SceneBatch Empty { get; } = new();

    public Option<ViewportInfo> View { get; init; }
    public Option<LinearWorkflow> Workflow { get; init; }
    public HashMap<uint, Transform> Transforms { get; init; } = HashMap<uint, Transform>();
    public HashMap<Guid, Light> DynamicLights { get; init; } = HashMap<Guid, Light>();
    public Option<RenderSettings> Settings { get; init; }
    public HashMap<RenderSettings.EnvironmentUsage, Option<RenderEnvironment>> Environments { get; init; } = HashMap<RenderSettings.EnvironmentUsage, Option<RenderEnvironment>>();
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

public sealed record QueuePolicy(bool RespectDisplayAttributes, bool NotifyChanges, bool OriginalObjects, Option<Rhino.Render.ChangeQueue.ChangeQueue.BakingFunctions> Bake) {
    public static QueuePolicy Viewport { get; } = new(RespectDisplayAttributes: true, NotifyChanges: true, OriginalObjects: false, Bake: None);
    public static QueuePolicy Capture { get; } = Viewport with { NotifyChanges = false };
    public static QueuePolicy Render { get; } = Capture with { RespectDisplayAttributes = false };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedChangeFeed : Rhino.Render.ChangeQueue.ChangeQueue {
    // --- [ACQUISITION]
    private readonly IPlugInViews owner;
    private readonly QueuePolicy policy;
    private readonly Option<IO<Unit>> wake;

    private DefinedChangeFeed(IPlugInViews owner, RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(owner.PlugIn.Id, document.RuntimeSerialNumber, view, attributes.ValueUnsafe(), policy.RespectDisplayAttributes, policy.NotifyChanges) =>
        (this.owner, this.policy, this.wake) = (owner, policy, wake);

    private DefinedChangeFeed(IPlugInViews owner, CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(owner.PlugIn.Id, preview) =>
        (this.owner, this.policy, this.wake) = (owner, policy, wake);

    public static IO<DefinedChangeFeed> Of(IPlugInViews owner, RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake) =>
        use(() => new DefinedChangeFeed(owner, document, view, attributes, policy, wake));

    public static IO<DefinedChangeFeed> Of(IPlugInViews owner, CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake) =>
        use(() => new DefinedChangeFeed(owner, preview, policy, wake));

    // --- [FEED]
    private readonly Ref<Option<SceneFeed>> feed = Ref(Some(new SceneFeed(new Lease<SceneBatch>(SceneBatch.Empty, EnvIO.New(token: CancellationToken.None)), SceneBatch.Empty, [])));

    public IO<Unit> World => IO.lift(() => CreateWorld(bFlushWhenReady: false)).Bind(_ => Drain);

    public IO<SceneBatch> Take =>
        from prior in atomicIO(() => {
            Option<SceneFeed> held = feed.Value;
            feed.Value = held.Map(static current => current.Taken);
            return held;
        })
        from taken in IO.lift(prior.ToFin(Errors.Cancelled))
        from owned in taken.Published.TraverseM(static scope => use(() => scope)).As()
        select taken.Pending;

    private IO<Unit> Drain =>
        from flushed in IO.lift(Flush)
        from environments in Collected(Lent(GetQueueRenderSettings(), settings => toSeq(Enum.GetValues<RenderSettings.EnvironmentUsage>())
            .TraverseM(usage =>
                from environment in IO.lift(() => Optional(EnvironmentFromOriginalInstanceId(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.ForRendering))))
                from copy in environment.TraverseM(static source => Products.Made((RenderEnvironment?)source.MakeCopy())).As()
                select (usage, copy)).As()
            .Map(static rows => new SceneBatch { Environments = new(rows, tryAdd: false) })))
        from fresh in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
        from published in atomicIO(() => feed.Swap(held => held.Map(current => current.Publish(fresh))))
        from closed in when(published.IsNone, IO.lift(fresh.Dispose)).As()
        from woken in Signal
        select unit;

    private IO<Unit> Signal => wake.TraverseM(static signal => signal).As().Map(unit);

    protected override void Dispose(bool isDisposing) =>
        _ = owner.Run(
            from prior in atomicIO(() => {
                Option<SceneFeed> held = feed.Value;
                feed.Value = None;
                return held;
            })
            from released in Callbacks.Each(
                IO.lift(() => base.Dispose(isDisposing)).Cons(prior.ToSeq().Bind(static current => current.Scopes).Map(static scope => IO.lift(scope.Dispose))))
            select unit);

    private sealed record SceneFeed(Lease<SceneBatch> Collecting, SceneBatch Pending, Seq<EnvIO> Published) {
        public Seq<EnvIO> Scopes => Published.Add(Collecting.Scope);

        public SceneFeed Taken => this with { Pending = SceneBatch.Empty, Published = [] };

        public SceneFeed Collected(SceneBatch change) => this with { Collecting = Collecting with { Value = Collecting.Value.Combine(change) } };

        public SceneFeed Publish(EnvIO fresh) =>
            new(new Lease<SceneBatch>(SceneBatch.Empty, fresh), Pending.Combine(Collecting.Value), Published.Add(Collecting.Scope));
    }

    // --- [CHANGES]
    protected override void ApplyViewChange(ViewInfo viewInfo) =>
        Collect(use(() => new ViewportInfo(viewInfo.Viewport)).Map(static view => new SceneBatch { View = Some(view) }));

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) =>
        Collect(use(() => new LinearWorkflow(lw)).Map(static workflow => new SceneBatch { Workflow = Some(workflow) }));

    protected override void ApplyDynamicObjectTransforms(List<Rhino.Render.ChangeQueue.DynamicObjectTransform> dynamicObjectTransforms) =>
        Collect(IO.lift(() => new SceneBatch { Transforms = new(toSeq(dynamicObjectTransforms).Map(static moved => (moved.MeshInstanceId, moved.Transform)), tryAdd: false) }));

    protected override void ApplyDynamicLightChanges(List<Light> dynamicLightChanges) =>
        Collect(toSeq(dynamicLightChanges).TraverseM(Copies.Duplicate).As()
            .Map(static copies => new SceneBatch { DynamicLights = new(copies.Map(static copy => (copy.Id, copy)), tryAdd: false) }));

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) =>
        Collect(use(() => new RenderSettings(rs)).Map(static settings => new SceneBatch { Settings = Some(settings) }));

    protected override void ApplySkylightChanges(Rhino.Render.ChangeQueue.Skylight skylight) =>
        Collect(IO.lift(() => new SceneBatch { Skylight = Some(SceneMapper.ToSkylight(skylight)) }));

    protected override void ApplySunChanges(Light sun) =>
        Collect(Copies.Duplicate(sun).Map(static copy => new SceneBatch { Sun = Some(copy) }));

    protected override void ApplyLightChanges(List<Rhino.Render.ChangeQueue.Light> lightChanges) =>
        Collect(IO.lift(() => Optional(GetQueueView())).Bind(view =>
            toSeq(lightChanges).TraverseM(light => light.ChangeType is Rhino.Render.ChangeQueue.Light.Event.Deleted
                    ? IO.pure((light.Id, Option<SceneLight>.None))
                    : Lent(light.Data, data =>
                        from converted in IO.lift(() => view.Filter(_ => data.CoordinateSystem is CoordinateSystem.Camera).Iter(held => ConvertCameraBasedLightToWorld(this, light, held)))
                        from copy in Copies.Duplicate(data)
                        select (light.Id, Some(SceneMapper.ToLight(light, copy))))).As()
                .Map(static rows => new SceneBatch { Lights = new(rows, tryAdd: false) })
                .Finally(IO.lift(() => view.Iter(static held => held.Dispose())))));

    protected override void ApplyMaterialChanges(List<Rhino.Render.ChangeQueue.Material> mats) =>
        Collect(IO.lift(() => new SceneBatch { Materials = new(toSeq(mats).Map(static material => (material.MeshInstanceId, material.Id)), tryAdd: false) }));

    protected override void ApplyMeshChanges(Guid[] deleted, List<Rhino.Render.ChangeQueue.Mesh> added) =>
        Collect(toSeq(added).TraverseM(Meshed).As().Map(meshes => new SceneBatch {
            Meshes = new(toSeq(deleted).Map(static id => (id, Option<SceneMesh>.None)).Concat(meshes.Map(static mesh => (mesh.Id, Some(mesh)))), tryAdd: false),
        }));

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<Rhino.Render.ChangeQueue.MeshInstance> addedOrChanged) =>
        Collect(toSeq(addedOrChanged).TraverseM(Instance).As().Map(instances => new SceneBatch {
            Instances = new(toSeq(deleted).Map(static id => (id, Option<SceneInstance>.None)).Concat(instances.Map(static instance => (instance.InstanceId, Some(instance)))), tryAdd: false),
        }));

    protected override void ApplyGroundPlaneChanges(Rhino.Render.ChangeQueue.GroundPlane gp) =>
        Collect(IO.lift(() => new SceneBatch { Ground = Some(SceneMapper.ToGround(gp)) }));

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<Rhino.Render.ChangeQueue.ClippingPlane> addedOrModified) =>
        Collect(toSeq(addedOrModified).TraverseM(Clip).As().Map(clips => new SceneBatch {
            ClippingPlanes = new(toSeq(deleted).Map(static id => (id, Option<SceneClip>.None)).Concat(clips.Map(static clip => (clip.Id, Some(clip)))), tryAdd: false),
        }));

    protected override void ApplyDynamicClippingPlaneChanges(List<Rhino.Render.ChangeQueue.ClippingPlane> changed) =>
        Collect(toSeq(changed).TraverseM(Clip).As().Map(static clips => new SceneBatch { ClippingPlanes = new(clips.Map(static clip => (clip.Id, Some(clip))), tryAdd: false) }));

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) =>
        Collect(IO.lift(() => new SceneBatch { Display = Some(SceneMapper.ToDisplay(displayPipelineAttributes)) }));

    // --- [COPIES]
    private static IO<SceneMesh> Meshed(Rhino.Render.ChangeQueue.Mesh source) =>
        from mesh in Lent(source.SingleMesh, Copies.Duplicate)
        from mappings in IO.lift(() => Conversions.Rows(source.Mappings)).Bind(static channels => channels.TraverseM(static channel =>
            Optional(channel.Mapping).TraverseM(static mapping => use(() => mapping)).As().Map(mapping => SceneMapper.ToMapping(channel, mapping))).As())
        from attributes in Attributes(source.Attributes)
        from original in Optional(source.Object).TraverseM(static lent => Lent(lent, static held => Copies.Duplicate(held.Geometry))).As()
        select SceneMapper.ToMesh(source, source.Id(), mesh, mappings, attributes, original);

    private static IO<SceneInstance> Instance(Rhino.Render.ChangeQueue.MeshInstance instance) =>
        from attributes in Optional(instance.ObjectAttributes).TraverseM(static held => use(() => held)).As()
        from records in IO.lift(() => Conversions.Rows(instance.Ancestry))
        from ancestry in records.TraverseM(static record => Attributes(record.ReferenceAttributes).Map(copied => SceneMapper.ToAncestor(record, copied))).As()
            .Finally(IO.lift(() => records.Iter(static record => record.Dispose())))
        select SceneMapper.ToInstance(instance, attributes, ancestry);

    private static IO<SceneClip> Clip(Rhino.Render.ChangeQueue.ClippingPlane plane) =>
        Attributes(plane.Attributes).Map(attributes => SceneMapper.ToClip(plane, attributes));

    private static IO<Option<ObjectAttributes>> Attributes(ObjectAttributes? lent) =>
        Optional(lent).TraverseM(static held => Lent(held, static attributes => use(attributes.Duplicate))).As();

    private static IO<T> Lent<TLent, T>(TLent? lent, Func<TLent, IO<T>> copy, [CallerArgumentExpression(nameof(lent))] string call = "") where TLent : class, IDisposable =>
        IO.lift(Conversions.Found(lent, call: call)).Bind(held => copy(held).Finally(IO.lift(held.Dispose)));

    // --- [NOTICES]
    protected override void NotifyBeginUpdates() => _ = owner.Run(Signal);

    protected override void NotifyEndUpdates() => _ = owner.Run(Drain);

    protected override void NotifyDynamicUpdatesAreAvailable() => _ = owner.Run(Drain);

    // --- [POLICY]
    protected override BakingFunctions BakeFor() => policy.Bake.IfNone(base.BakeFor);

    protected override bool ProvideOriginalObject() => policy.OriginalObjects;

    // --- [ANSWERS]
    private void Collect(IO<SceneBatch> change, [CallerMemberName] string member = "") => _ = owner.Run(Collected(change), member);

    private IO<Unit> Collected(IO<SceneBatch> change) =>
        from current in IO.lift(() => feed.Value.ToFin(Errors.Cancelled))
        from batch in change.Under(current.Collecting.Scope)
        from collected in atomicIO(() => feed.Swap(held => held.Map(state => state.Collected(batch))))
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SceneMapper {
    public static partial SceneLight ToLight(Rhino.Render.ChangeQueue.Light light, Light data);

    public static partial SceneMesh ToMesh(Rhino.Render.ChangeQueue.Mesh source, Guid id, Mesh mesh, Seq<SceneMapping> mappings, Option<ObjectAttributes> attributes, Option<GeometryBase> original);

    public static partial SceneInstance ToInstance(Rhino.Render.ChangeQueue.MeshInstance instance, Option<ObjectAttributes> attributes, Seq<SceneAncestor> ancestry);

    public static partial SceneGround ToGround(Rhino.Render.ChangeQueue.GroundPlane ground);

    public static partial SceneSkylight ToSkylight(Rhino.Render.ChangeQueue.Skylight skylight);

    [MapProperty(nameof(Rhino.Render.ChangeQueue.ClippingPlane.ViewIds), nameof(SceneClip.ViewIds), Use = nameof(@Conversions.Rows))]
    public static partial SceneClip ToClip(Rhino.Render.ChangeQueue.ClippingPlane plane, Option<ObjectAttributes> attributes);

    public static partial SceneDisplay ToDisplay(DisplayPipelineAttributes attributes);

    public static partial SceneMapping ToMapping(Rhino.Render.ChangeQueue.MappingChannel channel, Option<TextureMapping> mapping);

    public static partial SceneAncestor ToAncestor(Rhino.Render.ChangeQueue.MeshInstance.AncestryRecord record, Option<ObjectAttributes> referenceAttributes);
}
