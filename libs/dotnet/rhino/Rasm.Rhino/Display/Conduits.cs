using System.Drawing;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ConduitChannel {
    private ConduitChannel(Func<ConduitDefinition, Func<RhinoViewport, bool>, IO<Option<DisplayConduit>>> attach) => Attach = attach;

    internal Func<ConduitDefinition, Func<RhinoViewport, bool>, IO<Option<DisplayConduit>>> Attach { get; }

    // --- [CONDUITS]
    public static ConduitChannel BoundingBox(Func<CalculateBoundingBoxEventArgs, Option<BoundingBox>> box) => Conduit(() => new BoundingBoxConduit(box));

    public static ConduitChannel ZoomExtents(Func<CalculateBoundingBoxEventArgs, Option<BoundingBox>> box) => Conduit(() => new ZoomExtentsConduit(box));

    public static ConduitChannel Culling(Func<CullObjectEventArgs, bool> culled) => Conduit(() => new CullingConduit(culled));

    public static ConduitChannel PreDrawObjects(Action<DrawEventArgs> draw) => Conduit(() => new PreDrawObjectsConduit(draw));

    public static ConduitChannel PreDrawObject(Func<DrawObjectEventArgs, bool> drawn) => Conduit(() => new PreDrawObjectConduit(drawn));

    public static ConduitChannel PostDrawObjects(Action<DrawEventArgs> draw) => Conduit(() => new PostDrawObjectsConduit(draw));

    public static ConduitChannel Foreground(Action<DrawEventArgs> draw) => Conduit(() => new ForegroundConduit(draw));

    public static ConduitChannel Overlay(Action<DrawEventArgs> draw) => Conduit(() => new OverlayConduit(draw));

    public static ConduitChannel FrameBuffer(Action<Size, ReadOnlySpan<byte>> read) => Conduit(() => new FrameBufferConduit(read));

    // --- [EVENTS]
    public static ConduitChannel PreDrawTransparentObjects(Action<DrawEventArgs> draw) =>
        Event(static h => DisplayPipeline.PreDrawTransparentObjects += h, static h => DisplayPipeline.PreDrawTransparentObjects -= h, static args => args.Viewport, static _ => null, draw);

    public static ConduitChannel PostDrawObject(Action<DrawObjectEventArgs> draw) =>
        Event(static h => DisplayPipeline.PostDrawObject += h, static h => DisplayPipeline.PostDrawObject -= h, static args => args.Viewport, static args => args.RhinoObject, draw);

    public static ConduitChannel InitFrameBuffer(Action<InitFrameBufferEventArgs> draw) =>
        Event(static h => DisplayPipeline.InitFrameBuffer += h, static h => DisplayPipeline.InitFrameBuffer -= h, static _ => null, static _ => null, draw);

    private static ConduitChannel Conduit(Func<DisplayConduit> made) => new((_, _) => IO.lift(() => Some(made())));

    private static ConduitChannel Event<TArgs>(
        Action<EventHandler<TArgs>> add, Action<EventHandler<TArgs>> remove, Func<TArgs, RhinoViewport?> viewport, Func<TArgs, RhinoObject?> drawn, Action<TArgs> draw) =>
        new((definition, shows) => use(() => {
            HashSet<Guid> ids = toHashSet(definition.Objects);
            EventHandler<TArgs> handler = (_, args) => {
                if ((viewport(args) is not { } port || (shows(port) && definition.Holds(port))) && (drawn(args) is not { } obj || definition.Holds(obj, ids)))
                    draw(args);
            };
            add(handler);
            return handler;
        }, remove).Map(static _ => Option<DisplayConduit>.None));
}

[Union]
public abstract partial record ConduitBinding {
    public sealed record Everywhere : ConduitBinding;

    public sealed record Bound(RhinoDoc Document, IterableNE<ViewportSet> Sets, bool Exclusive) : ConduitBinding;

    internal IO<Seq<RhinoDoc>> Documents =>
        IO.lift(() => Switch(everywhere: static _ => Conversions.Rows(RhinoDoc.OpenDocuments()), bound: static bound => Seq(bound.Document)));
}

public sealed record ConduitDefinition(Seq<ConduitChannel> Channels) {
    public ConduitBinding Binding { get; init; } = new ConduitBinding.Everywhere();
    public ObjectType Geometry { get; init; } = ObjectType.AnyObject;
    public ActiveSpace Space { get; init; } = ActiveSpace.None;
    public Option<bool> Selected { get; init; }
    public Seq<Guid> Objects { get; init; }

    internal bool Holds(RhinoViewport viewport) =>
        Space is ActiveSpace.None || Space == (viewport.ViewportType is ViewportType.PageViewMainViewport ? ActiveSpace.PageSpace : ActiveSpace.ModelSpace);

    internal bool Holds(RhinoObject obj, HashSet<Guid> ids) =>
        (Geometry is ObjectType.AnyObject || (obj.ObjectType & Geometry) is not ObjectType.None)
        && (Selected.IsNone || obj.IsSelected(Selected.Exists(identity)) > 0)
        && (ids.IsEmpty || ids.Contains(obj.Id));
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class BoundingBoxConduit(Func<CalculateBoundingBoxEventArgs, Option<BoundingBox>> box) : DisplayConduit {
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) => _ = box(e).Iter(e.IncludeBoundingBox);
}

internal sealed class ZoomExtentsConduit(Func<CalculateBoundingBoxEventArgs, Option<BoundingBox>> box) : DisplayConduit {
    protected override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) => _ = box(e).Iter(e.IncludeBoundingBox);
}

internal sealed class CullingConduit(Func<CullObjectEventArgs, bool> culled) : DisplayConduit {
    protected override void ObjectCulling(CullObjectEventArgs e) => e.CullObject |= culled(e);
}

internal sealed class PreDrawObjectsConduit(Action<DrawEventArgs> draw) : DisplayConduit {
    protected override void PreDrawObjects(DrawEventArgs e) => draw(e);
}

internal sealed class PreDrawObjectConduit(Func<DrawObjectEventArgs, bool> drawn) : DisplayConduit {
    protected override void PreDrawObject(DrawObjectEventArgs e) => e.DrawObject &= drawn(e);
}

internal sealed class PostDrawObjectsConduit(Action<DrawEventArgs> draw) : DisplayConduit {
    protected override void PostDrawObjects(DrawEventArgs e) => draw(e);
}

internal sealed class ForegroundConduit(Action<DrawEventArgs> draw) : DisplayConduit {
    protected override void DrawForeground(DrawEventArgs e) => draw(e);
}

internal sealed class OverlayConduit(Action<DrawEventArgs> draw) : DisplayConduit {
    protected override void DrawOverlay(DrawEventArgs e) => draw(e);
}

internal sealed class FrameBufferConduit(Action<Size, ReadOnlySpan<byte>> read) : DisplayConduit {
    private byte[] frame = [];

    protected override void PostProcessFrameBuffer(PostProcessFrameBufferEventArgs e) {
        Size size = e.FrameBufferSize;
        int stride = size.Width * 4;
        int length = stride * size.Height;
        if (length > frame.Length)
            frame = GC.AllocateUninitializedArray<byte>(length, pinned: true);
        if (length > 0 && e.TryCopyFrameBuffer(Marshal.UnsafeAddrOfPinnedArrayElement(frame, 0), stride, length))
            read(size, frame.AsSpan(0, length));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Conduits {
    // --- [LIFETIME]
    public static IO<Unit> Enable(ConduitDefinition definition, RedrawPolicy redraw) =>
        from scope in IO.env
        from shown in IO.lift(static () => Atom(HashSet<Guid>()))
        from made in definition.Channels.TraverseM(channel => channel.Attach(definition, viewport => Shows(definition.Binding, shown.Value, viewport))).As()
        let conduits = made.Somes().Map(conduit => Filtered(conduit, definition)).Strict()
        from released in Products.Released(() => conduits.Iter(static conduit => conduit.Enabled = false))
        from enabled in Redrawn(definition.Binding, redraw, definition.Binding.Switch(
            (Conduits: conduits, Shown: shown, Scope: scope),
            everywhere: static (state, _) => IO.lift(() => state.Conduits.Iter(static conduit => conduit.Enabled = true)),
            bound: static (state, bound) =>
                from rows in IO.lift(static () => Ref(Option<Owned<Unit>>.None))
                from first in Rebound(state.Conduits, state.Shown, bound, rows)
                from watched in Subscriptions.Merged(
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Create += h, static h => RhinoView.Create -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Destroy += h, static h => RhinoView.Destroy -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.SetActive += h, static h => RhinoView.SetActive -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h))
                    .Documents(static args => args.View.Document).In(bound.Document.RuntimeSerialNumber)
                    .Attach(_ => Rebound(state.Conduits, state.Shown, bound, rows).Scoped(state.Scope))
                select unit))
        select unit;

    private static bool Shows(ConduitBinding binding, HashSet<Guid> shown, RhinoViewport viewport) =>
        binding.Switch(
            (Shown: shown, Viewport: viewport),
            everywhere: static (_, _) => true,
            bound: static (state, bound) => state.Viewport.ParentView?.Document?.RuntimeSerialNumber == bound.Document.RuntimeSerialNumber
                && state.Shown.Contains(state.Viewport.Id));

    private static DisplayConduit Filtered(DisplayConduit conduit, ConduitDefinition definition) {
        conduit.GeometryFilter = definition.Geometry;
        conduit.SpaceFilter = definition.Space;
        conduit.SetSelectionFilter(definition.Selected.IsSome, definition.Selected.Exists(identity));
        conduit.SetObjectIdFilter(definition.Objects);
        return conduit;
    }

    private static IO<Unit> Rebound(Seq<DisplayConduit> conduits, Atom<HashSet<Guid>> shown, ConduitBinding.Bound bound, Ref<Option<Owned<Unit>>> rows) =>
        Products.Replace(rows,
            from resolved in toSeq(bound.Sets).TraverseM(set => Viewports.Resolve(bound.Document, set)).As().Map(static sets => sets.Flatten())
            from marked in IO.lift(() => shown.Swap(_ => toHashSet(resolved.Map(static row => row.Viewport.Id))))
            from rebound in IO.lift(() => conduits.Iter(conduit => {
                conduit.UnbindAll();
                _ = resolved.Iter(row => (bound.Exclusive ? (Action<RhinoViewport>)conduit.ExclusiveBind : conduit.Bind)(row.Viewport));
                conduit.Enabled = !resolved.IsEmpty;
            }))
            select rebound);

    private static IO<Unit> Redrawn(ConduitBinding binding, RedrawPolicy redraw, IO<Unit> change) =>
        binding.Documents.Bind(docs => docs.Fold(change, (body, doc) => Commits.WithinRedraw(doc, redraw, body)));
}
