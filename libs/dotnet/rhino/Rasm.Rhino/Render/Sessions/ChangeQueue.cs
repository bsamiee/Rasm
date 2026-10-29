using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Render.ChangeQueue;

namespace Rasm.Rhino.Render.Sessions;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record QueuePolicy(bool RespectDisplayAttributes, bool NotifyChanges, bool OriginalObjects, Option<ChangeQueue.BakingFunctions> Bake) {
    public static QueuePolicy Viewport { get; } = new(RespectDisplayAttributes: true, NotifyChanges: true, OriginalObjects: false, Bake: None);
    public static QueuePolicy Capture { get; } = Viewport with { NotifyChanges = false };
    public static QueuePolicy Render { get; } = Capture with { RespectDisplayAttributes = false };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class DefinedChangeFeed : ChangeQueue {
    // --- [ACQUISITION]
    private readonly QueuePolicy policy;
    private readonly Option<IO<Unit>> wake;
    private readonly Ref<Option<SceneFeed>> feed = Ref(Some(new SceneFeed(SceneBatch.Empty, [], SceneBatch.Empty, [])));

    private DefinedChangeFeed(RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake)
        : base(Session.PlugIn.Id, document.RuntimeSerialNumber, view, attributes.ValueUnsafe(), policy.RespectDisplayAttributes, policy.NotifyChanges) =>
        (this.policy, this.wake) = (policy, wake);

    private DefinedChangeFeed(CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake) : base(Session.PlugIn.Id, preview) =>
        (this.policy, this.wake) = (policy, wake);

    public static IO<DefinedChangeFeed> Of(RhinoDoc document, ViewInfo view, Option<DisplayPipelineAttributes> attributes, QueuePolicy policy, Option<IO<Unit>> wake) =>
        use(() => new DefinedChangeFeed(document, view, attributes, policy, wake));

    public static IO<DefinedChangeFeed> Of(CreatePreviewEventArgs preview, QueuePolicy policy, Option<IO<Unit>> wake) =>
        use(() => new DefinedChangeFeed(preview, policy, wake));

    // --- [FEED]
    public IO<Unit> World => IO.lift(() => CreateWorld(bFlushWhenReady: false)) >> Drain;

    public IO<SceneBatch> Take =>
        from prior in atomicIO(() => {
            Option<SceneFeed> held = feed.Value;
            feed.Value = held.Map(static current => current with { Pending = SceneBatch.Empty, Published = [] });
            return held;
        })
        from taken in IO.lift(prior.ToFin(Errors.Cancelled))
        from owned in taken.Published.TraverseM(static scope => use(() => scope)).As()
        select taken.Pending;

    private IO<Unit> Drain =>
        from flushed in IO.lift(Flush)
        from published in atomicIO(() => feed.Swap(static held => held.Map(static current => current.Publish())))
        from woken in Signal
        select unit;

    private IO<Unit> Signal => wake.TraverseM(static signal => signal).As().Map(static _ => unit);

    protected override void Dispose(bool isDisposing) =>
        _ = Session.Run(
            from prior in atomicIO(() => {
                Option<SceneFeed> held = feed.Value;
                feed.Value = None;
                return held;
            })
            from released in Products.Each(
                IO.lift(() => base.Dispose(isDisposing)).Cons(prior.ToSeq().Bind(static current => current.Held + current.Published).Map(static scope => IO.lift(scope.Dispose))))
            select unit);

    private sealed record SceneFeed(SceneBatch Collecting, Seq<EnvIO> Held, SceneBatch Pending, Seq<EnvIO> Published) {
        public SceneFeed Collected(SceneBatch change, EnvIO scope) => this with { Collecting = Collecting.Combine(change), Held = Held.Add(scope) };

        public SceneFeed Publish() => new(SceneBatch.Empty, [], Pending.Combine(Collecting), Published + Held);
    }

    // --- [CHANGES]
    protected override void ApplyViewChange(ViewInfo viewInfo) =>
        Collect(use(() => new ViewportInfo(viewInfo.Viewport)).Map(static view => new SceneBatch { View = Some(view) }));

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) =>
        Collect(use(() => new LinearWorkflow(lw)).Map(static workflow => new SceneBatch { Workflow = Some(workflow) }));

    protected override void ApplyDynamicObjectTransforms(List<DynamicObjectTransform> dynamicObjectTransforms) =>
        Collect(IO.lift(() => new SceneBatch { Transforms = new(toSeq(dynamicObjectTransforms).Map(static moved => (moved.MeshInstanceId, moved.Transform)), tryAdd: false) }));

    protected override void ApplyDynamicLightChanges(List<global::Rhino.Geometry.Light> dynamicLightChanges) =>
        Collect(toSeq(dynamicLightChanges).TraverseM(static lent => Copied(lent)).As()
            .Map(static copies => new SceneBatch { DynamicLights = new(copies.Map(static copy => (copy.Id, copy)), tryAdd: false) }));

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) =>
        Collect(use(() => new RenderSettings(rs)).Map(static settings => new SceneBatch {
            Settings = Some(settings),
            Environments = new(toSeq(Enum.GetValues<RenderSettings.EnvironmentUsage>())
                .Map(usage => (usage, Conversions.Present(settings.RenderEnvironmentId(usage, RenderSettings.EnvironmentPurpose.ForRendering)))), tryAdd: false),
        }));

    protected override void ApplySkylightChanges(global::Rhino.Render.ChangeQueue.Skylight skylight) =>
        Collect(IO.lift(() => new SceneBatch { Skylight = Some(new SceneSkylight(skylight.Enabled, skylight.UsesCustomEnvironment, skylight.ShadowIntensity)) }));

    protected override void ApplySunChanges(global::Rhino.Geometry.Light sun) =>
        Collect(Copied(sun).Map(static copy => new SceneBatch { Sun = Some(copy) }));

    protected override void ApplyLightChanges(List<global::Rhino.Render.ChangeQueue.Light> lightChanges) =>
        Collect(
            from view in Products.Maybe(GetQueueView)
            from rows in toSeq(lightChanges).TraverseM(light => light.ChangeType is global::Rhino.Render.ChangeQueue.Light.Event.Deleted
                ? IO.pure((light.Id, Option<SceneLight>.None))
                : from data in Products.Of(() => light.Data)
                  from converted in IO.lift(() => view.Filter(_ => data.CoordinateSystem is CoordinateSystem.Camera).Iter(held => ConvertCameraBasedLightToWorld(this, light, held)))
                  from copy in Copied(data)
                  select (light.Id, Some(new SceneLight(copy, Conversions.Present(light.MaterialId))))).As()
            select new SceneBatch { Lights = new(rows, tryAdd: false) });

    protected override void ApplyMaterialChanges(List<global::Rhino.Render.ChangeQueue.Material> mats) =>
        Collect(IO.lift(() => new SceneBatch { Materials = new(toSeq(mats).Map(static material => (material.MeshInstanceId, material.Id)), tryAdd: false) }));

    protected override void ApplyMeshChanges(Guid[] deleted, List<global::Rhino.Render.ChangeQueue.Mesh> added) =>
        Collect(toSeq(added).TraverseM(static source =>
                from meshes in toSeq(source.GetMeshes()).TraverseM(static lent => Copied(lent)).As()
                from mappings in toSeq(source.Mappings).TraverseM(static channel => Products.Of(() => channel.Mapping).Map(mapping => new SceneMapping(channel.Channel, channel.Local, mapping))).As()
                from attributes in Products.Maybe(() => source.Attributes?.Duplicate())
                from original in Optional(source.Object).TraverseM(static held => Copied(held.Geometry)).As()
                select (Id: source.Id(), Mesh: new SceneMesh(meshes, mappings, source.OcsTransform, attributes, original))).As()
            .Map(meshes => new SceneBatch {
                Meshes = new(toSeq(deleted).Map(static id => (id, Option<SceneMesh>.None)).Concat(meshes.Map(static mesh => (mesh.Id, Some(mesh.Mesh)))), tryAdd: false),
            }));

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<MeshInstance> addedOrChanged) =>
        Collect(toSeq(addedOrChanged).TraverseM(static instance =>
                from attributes in Products.Maybe(() => instance.ObjectAttributes)
                from ancestry in toSeq(instance.Ancestry).TraverseM(static record =>
                    Products.Maybe(() => record.ReferenceAttributes?.Duplicate()).Map(copied => new SceneAncestor(record.ReferenceId, record.DefinitionId, copied, record.Transform))
                        .Finally(IO.lift(record.Dispose))).As()
                select (instance.InstanceId, Some(new SceneInstance(
                    instance.MeshId, instance.MaterialId, instance.Transform, Conversions.Present(instance.RootId), Conversions.Present(instance.ParentId)) {
                    Attributes = attributes,
                    Ancestry = ancestry,
                }))).As()
            .Map(instances => new SceneBatch { Instances = new(toSeq(deleted).Map(static id => (id, Option<SceneInstance>.None)).Concat(instances), tryAdd: false) }));

    protected override void ApplyGroundPlaneChanges(global::Rhino.Render.ChangeQueue.GroundPlane gp) =>
        Collect(IO.lift(() => new SceneBatch {
            Ground = Some(new SceneGround(gp.Enabled, gp.Altitude, gp.IsShadowOnly, gp.ShowUnderside, Conversions.Present(gp.MaterialId), gp.TextureScale, gp.TextureOffset, gp.TextureRotation)),
        }));

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<ClippingPlane> addedOrModified) =>
        Collect(Clipped(addedOrModified).Map(clips => new SceneBatch {
            ClippingPlanes = new(toSeq(deleted).Map(static id => (id, Option<SceneClip>.None)).Concat(clips), tryAdd: false),
        }));

    protected override void ApplyDynamicClippingPlaneChanges(List<ClippingPlane> changed) =>
        Collect(Clipped(changed).Map(static clips => new SceneBatch { ClippingPlanes = new(clips, tryAdd: false) }));

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) =>
        Collect(IO.lift(() => new SceneBatch { Display = Some(new SceneDisplay(displayPipelineAttributes.Id, displayPipelineAttributes.RealtimeRenderPasses)) }));

    // --- [COPIES]
    private static IO<T> Copied<T>(T lent) where T : GeometryBase => Products.Of(() => lent.Duplicate() as T);

    private static IO<Seq<(Guid, Option<SceneClip>)>> Clipped(List<ClippingPlane> planes) =>
        toSeq(planes).TraverseM(static plane => Products.Maybe(() => plane.Attributes?.Duplicate())
            .Map(attributes => (plane.Id, Some(new SceneClip(plane.IsEnabled, plane.Plane, toSeq(plane.ViewIds), attributes))))).As();

    // --- [NOTICES]
    protected override void NotifyBeginUpdates() => _ = Session.Run(Signal);

    protected override void NotifyEndUpdates() => _ = Session.Run(Drain);

    protected override void NotifyDynamicUpdatesAreAvailable() => _ = Session.Run(Drain);

    // --- [POLICY]
    protected override BakingFunctions BakeFor() => policy.Bake.IfNone(base.BakeFor);

    protected override bool ProvideOriginalObject() => policy.OriginalObjects;

    // --- [COLLECTION]
    private void Collect(IO<SceneBatch> change, [CallerMemberName] string member = "") =>
        _ = Session.Run(
            Products.Transfer(static () => EnvIO.New(token: CancellationToken.None), scope =>
                from batch in change.Scoped(scope)
                from collected in atomicIO(() => feed.Swap(held => held.Map(state => state.Collected(batch, scope))))
                from closed in when(collected.IsNone, IO.lift(scope.Dispose)).As()
                select unit),
            member: member);
}
