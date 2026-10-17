using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

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
public sealed partial class ConduitFilter {
    // --- [VALUES]
    public static ConduitFilter Unfiltered { get; } = new(Option<bool>.None, Seq<Guid>(), ObjectType.AnyObject, ActiveSpace.None);

    public Option<bool> SelectionFilter { get; }

    public Seq<Guid> ObjectIds { get; }

    public ObjectType GeometryFilter { get; }

    public ActiveSpace SpaceFilter { get; }

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Option<bool> selectionFilter, ref Seq<Guid> objectIds, ref ObjectType geometryFilter, ref ActiveSpace spaceFilter) =>
        validationError = spaceFilter is ActiveSpace.None or ActiveSpace.ModelSpace or ActiveSpace.PageSpace ? null : new ValidationError($"space filter {spaceFilter} is no model, page, or open space");
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
        Callbacks.Run(
            from found in IO.pure(e).Bind(box)
            from included in IO.lift(() => found.Iter(e.IncludeBoundingBox))
            select included,
            new(sink, GetType(), nameof(CalculateBoundingBox)));
}

internal sealed class CalculateBoundingBoxZoomExtentsConduit(Func<DrawEventArgs, IO<Option<BoundingBox>>> box, IPlugInSink sink) : DisplayConduit {
    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        Callbacks.Run(
            from found in IO.pure(e).Bind(box)
            from included in IO.lift(() => found.Iter(e.IncludeBoundingBox))
            select included,
            new(sink, GetType(), nameof(CalculateBoundingBoxZoomExtents)));
}

internal sealed class ObjectCullingConduit(Func<CullObjectEventArgs, bool> cullObject, IPlugInSink sink) : DisplayConduit {
    protected override void ObjectCulling(CullObjectEventArgs e) =>
        Callbacks.Run(IO.lift(() => { if (cullObject(e)) e.CullObject = true; }), new(sink, GetType(), nameof(ObjectCulling)));
}

internal sealed class PreDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void PreDrawObjects(DrawEventArgs e) =>
        Callbacks.Run(IO.pure(e).Bind(draw), new(sink, GetType(), nameof(PreDrawObjects)));
}

internal sealed class PreDrawObjectConduit(Func<DrawObjectEventArgs, IO<bool>> drawObject, IPlugInSink sink) : DisplayConduit {
    protected override void PreDrawObject(DrawObjectEventArgs e) =>
        Callbacks.Run(
            from drawn in IO.pure(e).Bind(drawObject)
            from suppressed in IO.lift(() => { if (!drawn) e.DrawObject = false; })
            select suppressed,
            new(sink, GetType(), nameof(PreDrawObject)));
}

internal sealed class PostDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void PostDrawObjects(DrawEventArgs e) =>
        Callbacks.Run(IO.pure(e).Bind(draw), new(sink, GetType(), nameof(PostDrawObjects)));
}

internal sealed class DrawForegroundConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void DrawForeground(DrawEventArgs e) =>
        Callbacks.Run(IO.pure(e).Bind(draw), new(sink, GetType(), nameof(DrawForeground)));
}

internal sealed class DrawOverlayConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : DisplayConduit {
    protected override void DrawOverlay(DrawEventArgs e) =>
        Callbacks.Run(IO.pure(e).Bind(draw), new(sink, GetType(), nameof(DrawOverlay)));
}

internal sealed class PostProcessFrameBufferConduit(Func<PostProcessFrameBufferEventArgs, IO<Unit>> read, IPlugInSink sink) : DisplayConduit {
    protected override void PostProcessFrameBuffer(PostProcessFrameBufferEventArgs e) =>
        Callbacks.Run(IO.pure(e).Bind(read), new(sink, GetType(), nameof(PostProcessFrameBuffer)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conduits {
    // --- [LIFETIME]
    public static IO<Unit> Enable(ConduitDefinition definition, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(own =>
        from rows in definition.Binding.Switch(
            state: own, everywhere: static (_, _) => IO.pure(Seq<ViewportRef>()), bound: static (state, bound) => toSeq(bound.Sets).TraverseM(set => Viewports.ResolveViewports(bound.Document, set, state)).As().Map(static held => held.Flatten()))
        from conduits in toSeq(definition.Channels).TraverseM(channel => Enabled(channel, definition.Filter, definition.Binding, rows, sink, own)).As()
        select unit,
        retain);

    private static IO<DisplayConduit> Enabled(ConduitChannel channel, ConduitFilter filter, ConduitBinding binding, Seq<ViewportRef> rows, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => channel.Switch<IPlugInSink, DisplayConduit>(
            sink,
            calculateBoundingBox: static (held, bounds) => new CalculateBoundingBoxConduit(bounds.Box, held),
            calculateBoundingBoxZoomExtents: static (held, bounds) => new CalculateBoundingBoxZoomExtentsConduit(bounds.Box, held),
            objectCulling: static (held, culling) => new ObjectCullingConduit(culling.CullObject, held),
            preDrawObjects: static (held, phase) => new PreDrawObjectsConduit(phase.Draw, held),
            preDrawObject: static (held, phase) => new PreDrawObjectConduit(phase.DrawObject, held),
            postDrawObjects: static (held, phase) => new PostDrawObjectsConduit(phase.Draw, held),
            drawForeground: static (held, phase) => new DrawForegroundConduit(phase.Draw, held),
            drawOverlay: static (held, phase) => new DrawOverlayConduit(phase.Draw, held),
            postProcessFrameBuffer: static (held, frame) => new PostProcessFrameBufferConduit(frame.Read, held))),
        static held => IO.lift(() => { held.Enabled = false; }),
        conduit => {
            conduit.GeometryFilter = filter.GeometryFilter;
            conduit.SpaceFilter = filter.SpaceFilter;
            conduit.SetSelectionFilter(filter.SelectionFilter.IsSome, filter.SelectionFilter.Exists(identity));
            conduit.SetObjectIdFilter(filter.ObjectIds);
            Action<RhinoViewport> bind = binding.Switch<DisplayConduit, Action<RhinoViewport>>(
                conduit, everywhere: static (held, _) => held.Bind,
                bound: static (held, bound) => bound.Exclusive ? held.ExclusiveBind : held.Bind);
            _ = rows.Iter(row => bind(row.Viewport));
            conduit.Enabled = true;
            retain(new Disposal<DisplayConduit>(conduit, static held => held.Enabled = false));
        });

    // --- [PIPELINE]
    public static IO<TValue> WithState<TValue>(DisplayPipeline pipeline, Seq<RenderState> state, IO<TValue> draw) =>
        state.FoldBack(draw, (inner, render) => render.Switch(
            (Host: pipeline, Body: inner),
            depthTest: static (state, depth) => DisposalOps.Within(() => { state.Host.PushDepthTesting(depth.Enabled); return unit; }, _ => state.Body, _ => IO.lift(state.Host.PopDepthTesting)),
            depthWrite: static (state, depth) => DisposalOps.Within(() => { state.Host.PushDepthWriting(depth.Enabled); return unit; }, _ => state.Body, _ => IO.lift(state.Host.PopDepthWriting)),
            cullFace: static (state, cull) => DisposalOps.Within(() => { state.Host.PushCullFaceMode(cull.Mode); return unit; }, _ => state.Body, _ => IO.lift(state.Host.PopCullFaceMode)),
            model: static (state, model) => DisposalOps.Within(() => { state.Host.PushModelTransform(model.Xform); return unit; }, _ => state.Body, _ => IO.lift(state.Host.PopModelTransform)),
            projection2d: static (state, _) => DisposalOps.Within(() => { state.Host.Push2dProjection(); return unit; }, _ => state.Body, _ => IO.lift(state.Host.PopProjection)),
            depth: static (state, depth) => DisposalOps.Within(() => state.Host.DepthMode,
                _ => IO.lift(() => { state.Host.DepthMode = depth.Mode; }).Bind(_ => state.Body),
                prior => IO.lift(() => { state.Host.DepthMode = prior; })),
            zBias: static (state, bias) => DisposalOps.Within(() => state.Host.ZBiasMode,
                _ => IO.lift(() => { state.Host.ZBiasMode = bias.Mode; }).Bind(_ => state.Body),
                prior => IO.lift(() => { state.Host.ZBiasMode = prior; })),
            clippingPlane: static (state, plane) => DisposalOps.Within(() => state.Host.AddClippingPlane(plane.Point, plane.Normal), _ => state.Body, index => IO.lift(() => state.Host.RemoveClippingPlane(index)))));
}
