using System.Runtime.CompilerServices;
using Rasm.Rhino.Modeling;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using NativeQueue = Rhino.Render.ChangeQueue;

namespace Rasm.Rhino.Render.Sessions;

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

public sealed record QueuePolicy(bool RespectDisplayAttributes, bool NotifyChanges, bool OriginalObjects, Option<NativeQueue.ChangeQueue.BakingFunctions> Bake) {
    public static QueuePolicy Viewport { get; } = new(RespectDisplayAttributes: true, NotifyChanges: true, OriginalObjects: false, Bake: None);
    public static QueuePolicy Capture { get; } = Viewport with { NotifyChanges = false };
    public static QueuePolicy Render { get; } = Capture with { RespectDisplayAttributes = false };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedChangeFeed : NativeQueue.ChangeQueue {
    // --- [ACQUISITION]
    private readonly IPlugInSink sink;
    private readonly QueuePolicy policy;
    private readonly Option<IO<Unit>> wake;

    private DefinedChangeFeed(PlugIn plugIn, RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(plugIn.Id, document.RuntimeSerialNumber, view, attributes.ValueUnsafe(), policy.RespectDisplayAttributes, policy.NotifyChanges) =>
        (sink, this.policy, this.wake) = (IPlugInSink.Of(plugIn), policy, wake);

    private DefinedChangeFeed(PlugIn plugIn, CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(plugIn.Id, preview) =>
        (sink, this.policy, this.wake) = (IPlugInSink.Of(plugIn), policy, wake);

    public static IO<DefinedChangeFeed> Of(PlugIn plugIn, RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => new DefinedChangeFeed(plugIn, document, view, attributes, policy, wake)), retain);

    public static IO<DefinedChangeFeed> Of(PlugIn plugIn, CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => new DefinedChangeFeed(plugIn, preview, policy, wake)), retain);

    // --- [FEED]
    private readonly Ref<Option<SceneFeed>> feed = Ref(Some(SceneFeed.Empty));

    public IO<Unit> World => IO.lift(() => CreateWorld(bFlushWhenReady: false)).Bind(_ => Drain);

    public IO<SceneBatch> Take(Action<Disposal<SceneBatch>> retain) =>
        DisposalOps.Acquire(IO.lift(() => atomic(() => feed.Value.Match(
                Some: current => {
                    _ = feed.Swap(static held => held.Map(static value => value with { Pending = (SceneBatch.Empty, Seq<IDisposable>()) }));
                    return Fin.Succ(current.Pending);
                },
                None: static () => Errors.Cancelled))),
            static pending => DisposalOps.Release(pending.Owned),
            pending => retain(new Disposal<SceneBatch>(pending.Batch, _ => DisposalOps.Release(pending.Owned).Run())))
            .Map(static pending => pending.Batch);

    private IO<Unit> Drain =>
        from flushed in IO.lift(Flush)
        from environments in IO.lift(() => Collect(owned =>
            DisposalOps.Within(GetQueueRenderSettings, settings => toSeq(Enum.GetValues<RenderSettings.EnvironmentUsage>()).TraverseM(usage => Copied(settings, usage, owned)).As()
                .Map(static rows => new SceneBatch { Environments = new(rows, tryAdd: false) }))))
        from published in atomicIO(() => feed.Swap(static held => held.Map(static current => current.Publish())))
        from woken in Signal
        select unit;

    private IO<(RenderSettings.EnvironmentUsage Usage, Option<RenderEnvironment> Copy)> Copied(RenderSettings settings, RenderSettings.EnvironmentUsage usage, Action<IDisposable> owned) =>
        from environment in IO.lift(() => Optional(EnvironmentFromOriginalInstanceId(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.ForRendering))))
        from copy in environment.TraverseM(source =>
            DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound((RenderEnvironment?)source.MakeCopy(), nameof(RenderContent.MakeCopy))), owned)).As()
        select (usage, copy);

    private IO<Unit> Signal => wake.TraverseM(static signal => signal).As().Map(unit);

    protected override void Dispose(bool isDisposing) =>
        atomicIO(() => {
            Option<SceneFeed> current = feed.Value;
            _ = feed.Swap(static _ => None);
            return current;
        }).Bind(current => Callbacks.Each(Seq(
            IO.lift(() => base.Dispose(isDisposing)),
            DisposalOps.Release(current.ToSeq().Bind(static held => held.Pending.Owned + held.Collecting.Owned)))))
            .Map(unit)
            .Uninterruptible().Run();

    private sealed record SceneFeed((SceneBatch Batch, Seq<IDisposable> Owned) Collecting, (SceneBatch Batch, Seq<IDisposable> Owned) Pending) {
        public static SceneFeed Empty { get; } = new((SceneBatch.Empty, []), (SceneBatch.Empty, []));

        public SceneFeed Collect(SceneBatch change, IDisposable lease) =>
            this with { Collecting = (Collecting.Batch.Combine(change), Collecting.Owned.Add(lease)) };

        public SceneFeed Publish() => new((SceneBatch.Empty, []), (Pending.Batch.Combine(Collecting.Batch), Pending.Owned + Collecting.Owned));
    }

    // --- [CHANGES]
    protected override void ApplyViewChange(ViewInfo viewInfo) =>
        Collect(owned => DisposalOps.Acquire(IO.lift(() => new ViewportInfo(viewInfo.Viewport)), owned)
            .Map(static view => new SceneBatch { View = Some(view) }));

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) =>
        Collect(owned => DisposalOps.Acquire(IO.lift(() => new LinearWorkflow(lw)), owned)
            .Map(static workflow => new SceneBatch { Workflow = Some(workflow) }));

    protected override void ApplyDynamicObjectTransforms(List<NativeQueue.DynamicObjectTransform> dynamicObjectTransforms) =>
        Collect(_ => IO.lift(() => new SceneBatch { Transforms = new(toSeq(dynamicObjectTransforms).Map(static moved => (moved.MeshInstanceId, moved.Transform)), tryAdd: false) }));

    protected override void ApplyDynamicLightChanges(List<Light> dynamicLightChanges) =>
        Collect(owned => toSeq(dynamicLightChanges).TraverseM(light => Copies.Duplicate(light, owned)).As()
            .Map(static copies => new SceneBatch { DynamicLights = new(copies.Map(static copy => (copy.Id, copy)), tryAdd: false) }));

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) =>
        Collect(owned => DisposalOps.Acquire(IO.lift(() => new RenderSettings(rs)), owned)
            .Map(static settings => new SceneBatch { Settings = Some(settings) }));

    protected override void ApplySkylightChanges(NativeQueue.Skylight skylight) =>
        Collect(_ => IO.lift(() => new SceneBatch { Skylight = Some(SceneMapper.ToSkylight(skylight)) }));

    protected override void ApplySunChanges(Light sun) =>
        Collect(owned => Copies.Duplicate(sun, owned).Map(static copy => new SceneBatch { Sun = Some(copy) }));

    protected override void ApplyLightChanges(List<NativeQueue.Light> lightChanges) =>
        Collect(owned => DisposalOps.Within(() => Optional(GetQueueView()),
            view => toSeq(lightChanges).TraverseM(light => light.ChangeType is NativeQueue.Light.Event.Deleted
                ? IO.pure((light.Id, Option<SceneLight>.None))
                : DisposalOps.Within(() => light.Data, data =>
                    from converted in IO.lift(() => view.Filter(_ => data.CoordinateSystem is CoordinateSystem.Camera)
                        .Iter(held => ConvertCameraBasedLightToWorld(this, light, held)))
                    from copy in Copies.Duplicate(data, owned)
                    select (light.Id, Some(SceneMapper.ToLight(light, copy))))).As()
                .Map(static rows => new SceneBatch { Lights = new(rows, tryAdd: false) }),
            static view => DisposalOps.Release(view.ToSeq())));

    protected override void ApplyMaterialChanges(List<NativeQueue.Material> mats) =>
        Collect(_ => IO.lift(() => new SceneBatch { Materials = new(toSeq(mats).Map(static material => (material.MeshInstanceId, material.Id)), tryAdd: false) }));

    protected override void ApplyMeshChanges(Guid[] deleted, List<NativeQueue.Mesh> added) =>
        Collect(owned => toSeq(added).TraverseM(mesh => Mesh(mesh, owned)).As()
            .Map(meshes => new SceneBatch {
                Meshes = new(toSeq(deleted).Map(static id => (id, Option<SceneMesh>.None)).Concat(meshes.Map(static mesh => (mesh.Id, Some(mesh)))), tryAdd: false),
            }));

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<NativeQueue.MeshInstance> addedOrChanged) =>
        Collect(owned => toSeq(addedOrChanged).TraverseM(instance => Instance(instance, owned)).As().Map(instances => new SceneBatch {
            Instances = new(toSeq(deleted).Map(static id => (id, Option<SceneInstance>.None)).Concat(instances.Map(static instance => (instance.InstanceId, Some(instance)))), tryAdd: false),
        }));

    protected override void ApplyGroundPlaneChanges(NativeQueue.GroundPlane gp) =>
        Collect(_ => IO.lift(() => new SceneBatch { Ground = Some(SceneMapper.ToGround(gp)) }));

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<NativeQueue.ClippingPlane> addedOrModified) =>
        Collect(owned => toSeq(addedOrModified).TraverseM(plane => Clip(plane, owned)).As().Map(clips => new SceneBatch {
            ClippingPlanes = new(toSeq(deleted).Map(static id => (id, Option<SceneClip>.None)).Concat(clips.Map(static clip => (clip.Id, Some(clip)))), tryAdd: false),
        }));

    protected override void ApplyDynamicClippingPlaneChanges(List<NativeQueue.ClippingPlane> changed) =>
        Collect(owned => toSeq(changed).TraverseM(plane => Clip(plane, owned)).As()
            .Map(static clips => new SceneBatch { ClippingPlanes = new(clips.Map(static clip => (clip.Id, Some(clip))), tryAdd: false) }));

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) =>
        Collect(_ => IO.lift(() => new SceneBatch { Display = Some(SceneMapper.ToDisplay(displayPipelineAttributes)) }));

    // --- [COPIES]
    private static IO<SceneMesh> Mesh(NativeQueue.Mesh source, Action<IDisposable> retain) =>
        from mesh in DisposalOps.Within(() => source.SingleMesh, borrowed => Copies.Duplicate(borrowed, retain))
        from mappings in IO.lift(() => Conversions.Rows(source.Mappings)).Bind(channels => channels.TraverseM(channel =>
            DisposalOps.Acquire(IO.lift(() => Optional(channel.Mapping)), static mapping => DisposalOps.Release(mapping.ToSeq()), mapping => mapping.Iter(retain))
                .Map(mapping => SceneMapper.ToMapping(channel, mapping))).As())
        from attributes in Attributes(() => source.Attributes, retain)
        from original in DisposalOps.Within(() => Optional(source.Object),
            objects => objects.TraverseM(obj => Copies.Duplicate(obj.Geometry, retain)).As(),
            static objects => DisposalOps.Release(objects.ToSeq()))
        select SceneMapper.ToMesh(source, source.Id(), mesh, mappings, attributes, original);

    private static IO<SceneInstance> Instance(NativeQueue.MeshInstance instance, Action<IDisposable> retain) =>
        from attributes in DisposalOps.Acquire(IO.lift(() => Optional(instance.ObjectAttributes)), static value => DisposalOps.Release(value.ToSeq()), value => value.Iter(retain))
        from ancestry in DisposalOps.Within(() => Conversions.Rows(instance.Ancestry),
            records => records.TraverseM(record => Attributes(() => record.ReferenceAttributes, retain)
                .Map(copied => SceneMapper.ToAncestor(record, copied))).As(), DisposalOps.Release)
        select SceneMapper.ToInstance(instance, attributes, ancestry);

    private static IO<SceneClip> Clip(NativeQueue.ClippingPlane plane, Action<IDisposable> retain) =>
        Attributes(() => plane.Attributes, retain).Map(attributes => SceneMapper.ToClip(plane, attributes));

    private static IO<Option<ObjectAttributes>> Attributes(Func<ObjectAttributes?> read, Action<IDisposable> retain) =>
        DisposalOps.Within(() => Optional(read()),
            borrowed => borrowed.TraverseM(attributes => DisposalOps.Acquire(IO.lift(attributes.Duplicate), retain)).As(),
            static borrowed => DisposalOps.Release(borrowed.ToSeq()));

    // --- [NOTICES]
    protected override void NotifyBeginUpdates() => Answer(Signal);

    protected override void NotifyEndUpdates() => Answer(Drain);

    protected override void NotifyDynamicUpdatesAreAvailable() => Answer(Drain);

    // --- [POLICY]
    protected override BakingFunctions BakeFor() => policy.Bake.IfNone(base.BakeFor);

    protected override bool ProvideOriginalObject() => policy.OriginalObjects;

    // --- [ANSWERS]
    private void Collect(Func<Action<IDisposable>, IO<SceneBatch>> change, [CallerMemberName] string member = "") =>
        Answer(DisposalOps.Replace(change, (batch, lease) => atomicIO(() => feed.Value.Match(
                Some: current => {
                    _ = feed.Swap(_ => Some(current.Collect(batch, lease)));
                    return Fin.Succ(unit);
                },
                None: static () => Errors.Cancelled)).Bind(IO.lift))
            .Map(unit), member);

    private void Answer(IO<Unit> effect, [CallerMemberName] string member = "") =>
        Callbacks.Run(effect, new CallbackSite(sink, typeof(DefinedChangeFeed), member));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class SceneMapper {
    public static partial SceneLight ToLight(NativeQueue.Light light, Light data);

    public static partial SceneMesh ToMesh(NativeQueue.Mesh source, Guid id, Mesh mesh, Seq<SceneMapping> mappings, Option<ObjectAttributes> attributes, Option<GeometryBase> original);

    public static partial SceneInstance ToInstance(NativeQueue.MeshInstance instance, Option<ObjectAttributes> attributes, Seq<SceneAncestor> ancestry);

    public static partial SceneGround ToGround(NativeQueue.GroundPlane ground);

    public static partial SceneSkylight ToSkylight(NativeQueue.Skylight skylight);

    [MapProperty(nameof(NativeQueue.ClippingPlane.ViewIds), nameof(SceneClip.ViewIds), Use = nameof(@Conversions.Rows))]
    public static partial SceneClip ToClip(NativeQueue.ClippingPlane plane, Option<ObjectAttributes> attributes);

    public static partial SceneDisplay ToDisplay(DisplayPipelineAttributes attributes);

    public static partial SceneMapping ToMapping(NativeQueue.MappingChannel channel, Option<TextureMapping> mapping);

    public static partial SceneAncestor ToAncestor(NativeQueue.MeshInstance.AncestryRecord record, Option<ObjectAttributes> referenceAttributes);
}
