using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.PlugIns;

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
    public static ConduitFilter Unfiltered { get; } = new(Option<bool>.None, Seq<Guid>(), ObjectType.AnyObject, ActiveSpace.None);

    public Option<bool> SelectionFilter { get; }

    public Seq<Guid> ObjectIds { get; }

    public ObjectType GeometryFilter { get; }

    public ActiveSpace SpaceFilter { get; }

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
internal abstract class ChannelConduit : DisplayConduit {
    private readonly CallbackSite site;

    protected ChannelConduit(ConduitFilter filter, IPlugInSink sink, string member) {
        site = new CallbackSite(sink, GetType(), member);
        GeometryFilter = filter.GeometryFilter;
        SpaceFilter = filter.SpaceFilter;
        SetSelectionFilter(filter.SelectionFilter.IsSome, filter.SelectionFilter.Exists(identity));
        SetObjectIdFilter(filter.ObjectIds);
    }

    protected void Answer<T>(T args, Func<T, IO<Unit>> effect) where T : DrawEventArgs =>
        _ = Callbacks.Answer(args, effect, static () => unit, site);
}

internal sealed class CalculateBoundingBoxConduit(Func<DrawEventArgs, IO<Option<BoundingBox>>> box, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(CalculateBoundingBox)) {
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) =>
        Answer(e, args => box(args).Bind(found => IO.lift(() => found.Iter(args.IncludeBoundingBox))));
}

internal sealed class CalculateBoundingBoxZoomExtentsConduit(Func<DrawEventArgs, IO<Option<BoundingBox>>> box, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(CalculateBoundingBoxZoomExtents)) {
    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        Answer(e, args => box(args).Bind(found => IO.lift(() => found.Iter(args.IncludeBoundingBox))));
}

internal sealed class ObjectCullingConduit(Func<CullObjectEventArgs, bool> cullObject, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(ObjectCulling)) {
    protected override void ObjectCulling(CullObjectEventArgs e) =>
        Answer(e, args => IO.lift(() => cullObject(args)).Bind(culled => when(culled, IO.lift(() => { args.CullObject = true; })).As()));
}

internal sealed class PreDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(PreDrawObjects)) {
    protected override void PreDrawObjects(DrawEventArgs e) => Answer(e, draw);
}

internal sealed class PreDrawObjectConduit(Func<DrawObjectEventArgs, IO<bool>> drawObject, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(PreDrawObject)) {
    protected override void PreDrawObject(DrawObjectEventArgs e) =>
        Answer(e, args => drawObject(args).Bind(drawn => unless(drawn, IO.lift(() => { args.DrawObject = false; })).As()));
}

internal sealed class PostDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(PostDrawObjects)) {
    protected override void PostDrawObjects(DrawEventArgs e) => Answer(e, draw);
}

internal sealed class DrawForegroundConduit(Func<DrawEventArgs, IO<Unit>> draw, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(DrawForeground)) {
    protected override void DrawForeground(DrawEventArgs e) => Answer(e, draw);
}

internal sealed class DrawOverlayConduit(Func<DrawEventArgs, IO<Unit>> draw, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(DrawOverlay)) {
    protected override void DrawOverlay(DrawEventArgs e) => Answer(e, draw);
}

internal sealed class PostProcessFrameBufferConduit(Func<PostProcessFrameBufferEventArgs, IO<Unit>> read, ConduitFilter filter, IPlugInSink sink)
    : ChannelConduit(filter, sink, nameof(PostProcessFrameBuffer)) {
    protected override void PostProcessFrameBuffer(PostProcessFrameBufferEventArgs e) => Answer(e, read);
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
            DisposalOps.AcquireAll(toSeq(definition.Channels).Map(channel => IO.lift(() => Enabled(Built(channel, definition.Filter, sink), definition.Binding, rows))), DisposalOps.Release),
            DisposalOps.Release(rows))
        select DisposalOps.Composite(rows.Map(static row => (IDisposable)row) + conduits, new CallbackSite(sink, typeof(Conduits), nameof(Enable)));

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(ConduitDefinition definition) =>
        (_, sink) => Enable(definition, sink);

    private static ChannelConduit Built(ConduitChannel channel, ConduitFilter filter, IPlugInSink sink) =>
        channel.Switch<(ConduitFilter Filter, IPlugInSink Sink), ChannelConduit>(
            (filter, sink),
            calculateBoundingBox: static (held, bounds) => new CalculateBoundingBoxConduit(bounds.Box, held.Filter, held.Sink),
            calculateBoundingBoxZoomExtents: static (held, bounds) => new CalculateBoundingBoxZoomExtentsConduit(bounds.Box, held.Filter, held.Sink),
            objectCulling: static (held, culling) => new ObjectCullingConduit(culling.CullObject, held.Filter, held.Sink),
            preDrawObjects: static (held, phase) => new PreDrawObjectsConduit(phase.Draw, held.Filter, held.Sink),
            preDrawObject: static (held, phase) => new PreDrawObjectConduit(phase.DrawObject, held.Filter, held.Sink),
            postDrawObjects: static (held, phase) => new PostDrawObjectsConduit(phase.Draw, held.Filter, held.Sink),
            drawForeground: static (held, phase) => new DrawForegroundConduit(phase.Draw, held.Filter, held.Sink),
            drawOverlay: static (held, phase) => new DrawOverlayConduit(phase.Draw, held.Filter, held.Sink),
            postProcessFrameBuffer: static (held, frame) => new PostProcessFrameBufferConduit(frame.Read, held.Filter, held.Sink));

    private static IDisposable Enabled(ChannelConduit conduit, ConduitBinding binding, Seq<ViewportRef> rows) {
        Action<RhinoViewport> bind = binding.Switch<ChannelConduit, Action<RhinoViewport>>(
            conduit,
            everywhere: static (held, _) => held.Bind,
            bound: static (held, bound) => bound.Exclusive ? held.ExclusiveBind : held.Bind);
        _ = rows.Iter(row => bind(row.Viewport));
        conduit.Enabled = true;
        return new Disposal<ChannelConduit>(conduit, static held => held.Enabled = false);
    }

    // --- [PIPELINE]
    public static IO<TValue> WithState<TValue>(DisplayPipeline pipeline, Seq<RenderState> state, IO<TValue> draw) =>
        state.FoldBack(draw, (inner, render) => render.Switch(
            (Pipeline: pipeline, Draw: inner),
            depthTest: static (scope, depth) => IO.lift(() => scope.Pipeline.PushDepthTesting(depth.Enabled)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopDepthTesting)),
            depthWrite: static (scope, depth) => IO.lift(() => scope.Pipeline.PushDepthWriting(depth.Enabled)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopDepthWriting)),
            cullFace: static (scope, cull) => IO.lift(() => scope.Pipeline.PushCullFaceMode(cull.Mode)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopCullFaceMode)),
            model: static (scope, model) => IO.lift(() => scope.Pipeline.PushModelTransform(model.Xform)).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopModelTransform)),
            projection2d: static (scope, _) => IO.lift(scope.Pipeline.Push2dProjection).Bracket(Use: _ => scope.Draw, Fin: _ => IO.lift(scope.Pipeline.PopProjection)),
            depth: static (scope, depth) => IO.lift(() => scope.Pipeline.DepthMode).Bracket(
                Use: _ => IO.lift(() => { scope.Pipeline.DepthMode = depth.Mode; }).Bind(_ => scope.Draw),
                Fin: prior => IO.lift(() => { scope.Pipeline.DepthMode = prior; })),
            zBias: static (scope, bias) => IO.lift(() => scope.Pipeline.ZBiasMode).Bracket(
                Use: _ => IO.lift(() => { scope.Pipeline.ZBiasMode = bias.Mode; }).Bind(_ => scope.Draw),
                Fin: prior => IO.lift(() => { scope.Pipeline.ZBiasMode = prior; })),
            clippingPlane: static (scope, plane) => IO.lift(() => scope.Pipeline.AddClippingPlane(plane.Point, plane.Normal)).Bracket(
                Use: _ => scope.Draw,
                Fin: index => IO.lift(() => scope.Pipeline.RemoveClippingPlane(index)))));
}
