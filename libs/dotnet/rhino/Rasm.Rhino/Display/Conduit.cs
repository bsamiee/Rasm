using System.Drawing;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Events;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ConduitChannel {
    public sealed record CalculateBoundingBox(Func<CalculateBoundingBoxEventArgs, IO<Option<BoundingBox>>> Box) : ConduitChannel;

    public sealed record CalculateBoundingBoxZoomExtents(Func<CalculateBoundingBoxEventArgs, IO<Option<BoundingBox>>> Box) : ConduitChannel;

    public sealed record ObjectCulling(Func<CullObjectEventArgs, IO<bool>> CullObject) : ConduitChannel;

    public sealed record PreDrawObjects(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record PreDrawObject(Func<DrawObjectEventArgs, IO<bool>> DrawObject) : ConduitChannel;

    public sealed record PostDrawObjects(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record DrawForeground(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record DrawOverlay(Func<DrawEventArgs, IO<Unit>> Draw) : ConduitChannel;

    public sealed record PostProcessFrameBuffer(Func<PostProcessFrameBufferEventArgs, IO<Unit>> Read) : ConduitChannel;
}

[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class ConduitFilter {
    // --- [VALUES]
    public static ConduitFilter Unfiltered { get; } = new(Option<bool>.None, Seq<Guid>(), ObjectType.AnyObject, ActiveSpace.None);

    public Option<bool> SelectionFilter { get; }

    public Seq<Guid> ObjectIds { get; }

    public ObjectType GeometryFilter { get; }

    public ActiveSpace SpaceFilter { get; }

    // --- [FACTORY]
    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Option<bool> selectionFilter, ref Seq<Guid> objectIds, ref ObjectType geometryFilter, ref ActiveSpace spaceFilter) =>
        validationError = spaceFilter is ActiveSpace.None or ActiveSpace.ModelSpace or ActiveSpace.PageSpace ? null : new RhinoFault($"space filter {spaceFilter} is not None, ModelSpace, or PageSpace");
}

[Union]
public abstract partial record ConduitBinding {
    public sealed record Everywhere() : ConduitBinding;

    public sealed record Bound(RhinoDoc Document, IterableNE<ViewportSet> Sets, bool Exclusive) : ConduitBinding;
}

public sealed record ConduitDefinition(IterableNE<ConduitChannel> Channels, ConduitFilter Filter, ConduitBinding Binding);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameReadback {
    private byte[] buffer = [];

    public IO<Option<T>> Read<T>(PostProcessFrameBufferEventArgs e, Func<Size, ReadOnlyMemory<byte>, T> read) =>
        IO.lift(() => e.FrameBufferSize switch {
            { IsEmpty: true } => Option<T>.None,
            var size => (Stride: size.Width * 4, Length: size.Width * 4 * size.Height) switch {
                var frame => Refused.Unless(e.TryCopyFrameBuffer(Marshal.UnsafeAddrOfPinnedArrayElement(Sized(frame.Length), 0), frame.Stride, frame.Length), nameof(e.TryCopyFrameBuffer))
                    .Map(_ => Some(read(size, buffer.AsMemory(0, frame.Length)))),
            },
        });

    private byte[] Sized(int length) =>
        buffer.Length >= length ? buffer : buffer = GC.AllocateUninitializedArray<byte>(length, pinned: true);
}

internal abstract class SinkConduit : DisplayConduit {
    protected SinkConduit(IPlugInSink sink, string member) => Site = new CallbackSite(sink, GetType(), member);

    protected CallbackSite Site { get; }
}

internal sealed class CalculateBoundingBoxConduit(Func<CalculateBoundingBoxEventArgs, IO<Option<BoundingBox>>> box, IPlugInSink sink) : SinkConduit(sink, nameof(CalculateBoundingBox)) {
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) => Callbacks.Run(IO.pure(e).Bind(box).Bind(found => IO.lift(() => found.Iter(e.IncludeBoundingBox))), Site);
}

internal sealed class CalculateBoundingBoxZoomExtentsConduit(Func<CalculateBoundingBoxEventArgs, IO<Option<BoundingBox>>> box, IPlugInSink sink) : SinkConduit(sink, nameof(CalculateBoundingBoxZoomExtents)) {
    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) => Callbacks.Run(IO.pure(e).Bind(box).Bind(found => IO.lift(() => found.Iter(e.IncludeBoundingBox))), Site);
}

internal sealed class ObjectCullingConduit(Func<CullObjectEventArgs, IO<bool>> cullObject, IPlugInSink sink) : SinkConduit(sink, nameof(ObjectCulling)) {
    protected override void ObjectCulling(CullObjectEventArgs e) {
        if (Callbacks.Answer(e, cullObject, static () => false, Site))
            e.CullObject = true;
    }
}

internal sealed class PreDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : SinkConduit(sink, nameof(PreDrawObjects)) {
    protected override void PreDrawObjects(DrawEventArgs e) => Callbacks.Run(IO.pure(e).Bind(draw), Site);
}

internal sealed class PreDrawObjectConduit(Func<DrawObjectEventArgs, IO<bool>> drawObject, IPlugInSink sink) : SinkConduit(sink, nameof(PreDrawObject)) {
    protected override void PreDrawObject(DrawObjectEventArgs e) {
        if (!Callbacks.Answer(e, drawObject, static () => true, Site))
            e.DrawObject = false;
    }
}

internal sealed class PostDrawObjectsConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : SinkConduit(sink, nameof(PostDrawObjects)) {
    protected override void PostDrawObjects(DrawEventArgs e) => Callbacks.Run(IO.pure(e).Bind(draw), Site);
}

internal sealed class DrawForegroundConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : SinkConduit(sink, nameof(DrawForeground)) {
    protected override void DrawForeground(DrawEventArgs e) => Callbacks.Run(IO.pure(e).Bind(draw), Site);
}

internal sealed class DrawOverlayConduit(Func<DrawEventArgs, IO<Unit>> draw, IPlugInSink sink) : SinkConduit(sink, nameof(DrawOverlay)) {
    protected override void DrawOverlay(DrawEventArgs e) => Callbacks.Run(IO.pure(e).Bind(draw), Site);
}

internal sealed class PostProcessFrameBufferConduit(Func<PostProcessFrameBufferEventArgs, IO<Unit>> read, IPlugInSink sink) : SinkConduit(sink, nameof(PostProcessFrameBuffer)) {
    protected override void PostProcessFrameBuffer(PostProcessFrameBufferEventArgs e) => Callbacks.Run(IO.pure(e).Bind(read), Site);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conduits {
    // --- [LIFETIME]
    public static IO<Unit> Enable(ConduitDefinition definition, RedrawPolicy redraw, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(own =>
            from conduits in DisposalOps.Acquire(
                IO.lift(() => toSeq(definition.Channels).Map(channel => Made(channel, definition.Filter, sink)).Strict()),
                held => Redrawn(definition.Binding, redraw, IO.lift(() => held.Iter(static conduit => conduit.Enabled = false))), own)
            from enabled in Redrawn(definition.Binding, redraw, definition.Binding.Switch(
                (Conduits: conduits, Sink: sink, Retain: own),
                everywhere: static (state, _) => IO.lift(() => state.Conduits.Iter(static conduit => conduit.Enabled = true)),
                bound: static (state, bound) =>
                    from rows in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), state.Retain)
                    from first in Rebound(state.Conduits, bound, rows)
                    from watched in Subscriptions.Host(typeof(Conduits), () => Seq(EventKind.RhinoViewCreate, EventKind.RhinoViewDestroy, EventKind.RhinoViewSetActive, EventKind.RhinoViewRename)
                            .Map(row => row.In(bound.Document.RuntimeSerialNumber)))
                        .Inline(_ => Rebound(state.Conduits, bound, rows), state.Sink, state.Retain)
                    select unit))
            select unit,
            retain);

    private static DisplayConduit Made(ConduitChannel channel, ConduitFilter filter, IPlugInSink sink) =>
        Filtered(channel.Switch<IPlugInSink, DisplayConduit>(
            sink,
            calculateBoundingBox: static (held, bounds) => new CalculateBoundingBoxConduit(bounds.Box, held),
            calculateBoundingBoxZoomExtents: static (held, bounds) => new CalculateBoundingBoxZoomExtentsConduit(bounds.Box, held),
            objectCulling: static (held, culling) => new ObjectCullingConduit(culling.CullObject, held),
            preDrawObjects: static (held, phase) => new PreDrawObjectsConduit(phase.Draw, held),
            preDrawObject: static (held, phase) => new PreDrawObjectConduit(phase.DrawObject, held),
            postDrawObjects: static (held, phase) => new PostDrawObjectsConduit(phase.Draw, held),
            drawForeground: static (held, phase) => new DrawForegroundConduit(phase.Draw, held),
            drawOverlay: static (held, phase) => new DrawOverlayConduit(phase.Draw, held),
            postProcessFrameBuffer: static (held, frame) => new PostProcessFrameBufferConduit(frame.Read, held)), filter);

    private static DisplayConduit Filtered(DisplayConduit conduit, ConduitFilter filter) {
        conduit.GeometryFilter = filter.GeometryFilter;
        conduit.SpaceFilter = filter.SpaceFilter;
        conduit.SetSelectionFilter(filter.SelectionFilter.IsSome, filter.SelectionFilter.Exists(identity));
        conduit.SetObjectIdFilter(filter.ObjectIds);
        return conduit;
    }

    private static IO<Unit> Rebound(Seq<DisplayConduit> conduits, ConduitBinding.Bound bound, Retained<IDisposable> rows) =>
        DisposalOps.Acquire(
            held =>
                from resolved in toSeq(bound.Sets).TraverseM(set => Viewports.Resolve(bound.Document, set, held)).As().Map(static sets => sets.Flatten())
                from rebound in IO.lift(() => conduits.Iter(conduit => {
                    conduit.UnbindAll();
                    _ = resolved.Iter(row => (bound.Exclusive ? (Action<RhinoViewport>)conduit.ExclusiveBind : conduit.Bind)(row.Viewport));
                    conduit.Enabled = !resolved.IsEmpty;
                }))
                select rebound,
            (_, lease) => rows.Replace(lease));

    private static IO<Unit> Redrawn(ConduitBinding binding, RedrawPolicy redraw, IO<Unit> change) =>
        IO.lift(() => binding.Switch(everywhere: static _ => toSeq(RhinoDoc.OpenDocuments()), bound: static bound => Seq(bound.Document)))
            .Bind(docs => docs.Fold(change, (body, doc) => Commits.WithinRedraw(doc, redraw, body)));
}
