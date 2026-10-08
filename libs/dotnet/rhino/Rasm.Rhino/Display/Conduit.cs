using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ConduitChannel {
    public sealed record CalculateBoundingBox(Func<DrawEventArgs, IO<Option<BoundingBox>>> Box) : ConduitChannel;

    public sealed record CalculateBoundingBoxZoomExtents(Func<DrawEventArgs, IO<Option<BoundingBox>>> Box) : ConduitChannel;

    public sealed record ObjectCulling(Func<CullObjectEventArgs, bool> CullObject) : ConduitChannel;

    public sealed record PreDrawObjects(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record PreDrawObject(Func<DrawObjectEventArgs, IO<bool>> DrawObject) : ConduitChannel;

    public sealed record PostDrawObjects(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record DrawForeground(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record DrawOverlay(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record PostProcessFrameBuffer(Func<PostProcessFrameBufferEventArgs, IO<Unit>> Read) : ConduitChannel;
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class ConduitFilter {
    // --- [VALUES]
    public static ConduitFilter Unfiltered { get; } = new(Option<bool>.None, Seq<Guid>(), ObjectType.AnyObject, ActiveSpace.None);

    public Option<bool> SelectionFilter { get; }

    public Seq<Guid> ObjectIds { get; }

    public ObjectType GeometryFilter { get; }

    public ActiveSpace SpaceFilter { get; }

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Option<bool> selectionFilter, ref Seq<Guid> objectIds, ref ObjectType geometryFilter, ref ActiveSpace spaceFilter) =>
        validationError = spaceFilter is ActiveSpace.None or ActiveSpace.ModelSpace or ActiveSpace.PageSpace ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record ConduitBinding {
    public sealed record Everywhere() : ConduitBinding;

    public sealed record Bound(RhinoDoc Document, IterableNE<ViewportSet> Sets, bool Exclusive) : ConduitBinding;
}

public sealed record ConduitDefinition(IterableNE<ConduitChannel> Channels, ConduitFilter Filter, ConduitBinding Binding);

[Union]
public abstract partial record RenderState {
    public sealed record DepthTest(bool Enabled) : RenderState;

    public sealed record DepthWrite(bool Enabled) : RenderState;

    public sealed record CullFace(CullFaceMode Mode) : RenderState;

    public sealed record Model(Transform Xform) : RenderState;

    public sealed record Projection2d() : RenderState;

    public sealed record Depth(DepthMode Mode) : RenderState;

    public sealed record ZBias(ZBiasMode Mode) : RenderState;

    public sealed record ClippingPlane(Point3d Point, Vector3d Normal) : RenderState;
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class CalculateBoundingBoxConduit(Func<DrawEventArgs, IO<Option<BoundingBox>>> box, IPlugInSink sink) : DisplayConduit {
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) =>
        _ = Callbacks.Answer(
            from found in IO.pure(e).Bind(box)
            from included in IO.lift(() => found.Iter(e.IncludeBoundingBox))
            select included,
            static () => unit, new(sink, GetType(), nameof(CalculateBoundingBox)));
}

internal sealed class CalculateBoundingBoxZoomExtentsConduit(Func<DrawEventArgs, IO<Option<BoundingBox>>> box, IPlugInSink sink) : DisplayConduit {
    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        _ = Callbacks.Answer(
            from found in IO.pure(e).Bind(box)
            from included in IO.lift(() => found.Iter(e.IncludeBoundingBox))
            select included,
            static () => unit, new(sink, GetType(), nameof(CalculateBoundingBoxZoomExtents)));
}

internal sealed class ObjectCullingConduit(Func<CullObjectEventArgs, bool> cullObject, IPlugInSink sink) : DisplayConduit {
    protected override void ObjectCulling(CullObjectEventArgs e) =>
        _ = Callbacks.Answer(IO.lift(() => { if (cullObject(e)) e.CullObject = true; }), static () => unit, new(sink, GetType(), nameof(ObjectCulling)));
}

internal sealed class PreDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void PreDrawObjects(DrawEventArgs e) =>
        _ = Callbacks.Answer(e, draw, static () => unit, new(sink, GetType(), nameof(PreDrawObjects)));
}

internal sealed class PreDrawObjectConduit(Func<DrawObjectEventArgs, IO<bool>> drawObject, IPlugInSink sink) : DisplayConduit {
    protected override void PreDrawObject(DrawObjectEventArgs e) =>
        _ = Callbacks.Answer(
            from drawn in IO.pure(e).Bind(drawObject)
            from suppressed in IO.lift(() => { if (!drawn) e.DrawObject = false; })
            select suppressed,
            static () => unit, new(sink, GetType(), nameof(PreDrawObject)));
}

internal sealed class PostDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void PostDrawObjects(DrawEventArgs e) =>
        _ = Callbacks.Answer(e, draw, static () => unit, new(sink, GetType(), nameof(PostDrawObjects)));
}

internal sealed class DrawForegroundConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void DrawForeground(DrawEventArgs e) =>
        _ = Callbacks.Answer(e, draw, static () => unit, new(sink, GetType(), nameof(DrawForeground)));
}

internal sealed class DrawOverlayConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void DrawOverlay(DrawEventArgs e) =>
        _ = Callbacks.Answer(e, draw, static () => unit, new(sink, GetType(), nameof(DrawOverlay)));
}

internal sealed class PostProcessFrameBufferConduit(Func<PostProcessFrameBufferEventArgs, IO<Unit>> read, IPlugInSink sink) : DisplayConduit {
    protected override void PostProcessFrameBuffer(PostProcessFrameBufferEventArgs e) =>
        _ = Callbacks.Answer(e, read, static () => unit, new(sink, GetType(), nameof(PostProcessFrameBuffer)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conduits {
    // --- [LIFETIME]
    public static IO<IDisposable> Enable(ConduitDefinition definition, IPlugInSink sink) =>
        from rows in definition.Binding.Switch(
            everywhere: static _ => IO.pure(Seq<ViewportRef>()),
            bound: static bound => DisposalOps
                .AcquireAll(toSeq(bound.Sets).Map(set => Viewports.ResolveViewports(bound.Document, set)), static held => DisposalOps.Release(held.Flatten()))
                .Map(static held => held.Flatten()))
        from conduits in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(toSeq(definition.Channels).Map(channel => Enabled(channel, definition.Filter, definition.Binding, rows, sink)), DisposalOps.Release),
            DisposalOps.Release(rows))
        select DisposalOps.Composite(rows.Map(static row => (IDisposable)row) + conduits, new CallbackSite(sink, typeof(Conduits), nameof(Enable)));

    private static IO<IDisposable> Enabled(ConduitChannel channel, ConduitFilter filter, ConduitBinding binding, Seq<ViewportRef> rows, IPlugInSink sink) =>
        from conduit in IO.lift(() => channel.Switch<IPlugInSink, DisplayConduit>(
            sink,
            calculateBoundingBox: static (held, bounds) => new CalculateBoundingBoxConduit(bounds.Box, held),
            calculateBoundingBoxZoomExtents: static (held, bounds) => new CalculateBoundingBoxZoomExtentsConduit(bounds.Box, held),
            objectCulling: static (held, culling) => new ObjectCullingConduit(culling.CullObject, held),
            preDrawObjects: static (held, phase) => new PreDrawObjectsConduit(phase.Draw, held),
            preDrawObject: static (held, phase) => new PreDrawObjectConduit(phase.DrawObject, held),
            postDrawObjects: static (held, phase) => new PostDrawObjectsConduit(phase.Draw, held),
            drawForeground: static (held, phase) => new DrawForegroundConduit(phase.Draw, held),
            drawOverlay: static (held, phase) => new DrawOverlayConduit(phase.Draw, held),
            postProcessFrameBuffer: static (held, frame) => new PostProcessFrameBufferConduit(frame.Read, held)))
        let release = new Disposal<DisplayConduit>(conduit, static held => held.Enabled = false)
        from enabled in DisposalOps.OnFailure(IO.lift(IDisposable () => {
            ConduitMapper.Update((filter.GeometryFilter, filter.SpaceFilter), conduit);
            conduit.SetSelectionFilter(filter.SelectionFilter.IsSome, filter.SelectionFilter.Exists(identity));
            conduit.SetObjectIdFilter(filter.ObjectIds);
            Action<RhinoViewport> bind = binding.Switch<DisplayConduit, Action<RhinoViewport>>(
                conduit, everywhere: static (held, _) => held.Bind,
                bound: static (held, bound) => bound.Exclusive ? held.ExclusiveBind : held.Bind);
            _ = rows.Iter(row => bind(row.Viewport));
            conduit.Enabled = true;
            return release;
        }), IO.lift(release.Dispose))
        select enabled;

    // --- [PIPELINE]
    public static IO<TValue> WithState<TValue>(DisplayPipeline pipeline, Seq<RenderState> state, IO<TValue> draw) =>
        state.Map(render => render.Switch(
            pipeline,
            depthTest: static (host, depth) => IO.lift(() => host.PushDepthTesting(depth.Enabled)).Map(_ => IO.lift(host.PopDepthTesting)),
            depthWrite: static (host, depth) => IO.lift(() => host.PushDepthWriting(depth.Enabled)).Map(_ => IO.lift(host.PopDepthWriting)),
            cullFace: static (host, cull) => IO.lift(() => host.PushCullFaceMode(cull.Mode)).Map(_ => IO.lift(host.PopCullFaceMode)),
            model: static (host, model) => IO.lift(() => host.PushModelTransform(model.Xform)).Map(_ => IO.lift(host.PopModelTransform)),
            projection2d: static (host, _) => IO.lift(host.Push2dProjection).Map(_ => IO.lift(host.PopProjection)),
            depth: static (host, depth) => from prior in IO.lift(() => host.DepthMode)
                                           let restore = IO.lift(() => { host.DepthMode = prior; })
                                           from written in DisposalOps.OnFailure(IO.lift(() => { host.DepthMode = depth.Mode; }), restore)
                                           select restore,
            zBias: static (host, bias) => from prior in IO.lift(() => host.ZBiasMode)
                                          let restore = IO.lift(() => { host.ZBiasMode = prior; })
                                          from written in DisposalOps.OnFailure(IO.lift(() => { host.ZBiasMode = bias.Mode; }), restore)
                                          select restore,
            clippingPlane: static (host, plane) => from index in IO.lift(() => host.AddClippingPlane(plane.Point, plane.Normal))
                                                   select IO.lift(() => host.RemoveClippingPlane(index))))
        .FoldBack(draw, static (inner, acquire) => use(acquire, identity).Action(inner).As().Bracket());
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class ConduitMapper {
    internal static partial void Update((ObjectType GeometryFilter, ActiveSpace SpaceFilter) filter, DisplayConduit conduit);
}
