using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.UI;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentBody<TReading>(FrameLimits Limits, SizeF Ideal, Func<Context, Capsule, Shade, RectangleF, TReading, IO<Unit>> Paint) {
    public static Fin<ContentBody<TReading>> Of(SizeF minimum, SizeF ideal, SizeF maximum, Func<Context, Capsule, Shade, RectangleF, TReading, IO<Unit>> paint) =>
        (FrameLimits.Validated(minimum, maximum, new Padding(ResizableAttributes<Component>.EdgeSize)),
         GrasshopperFault.Unless(
             minimum.Width <= ideal.Width && ideal.Width <= maximum.Width && minimum.Height <= ideal.Height && ideal.Height <= maximum.Height,
             GrasshopperReason.Outside, (minimum, ideal, maximum)))
            .Apply((limits, _) => new ContentBody<TReading>(limits, ideal, paint)).As();
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ContentAttributes<TReading> : ComponentAttributes, IResizableAttributes, ICursorAwareAttributes {
    // --- [STATE]
    private const string SizeKey = "Attr.Size";
    private const float Rim = 2f * StandardLayout.IconPadding;
    private readonly ModularComponent component;
    private readonly IGrasshopperPlugin plugin;
    private readonly ContentBody<TReading> body;
    private readonly Atom<Option<TReading>> reading;
    private readonly object modular;
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Lease<Resize>>> track = Ref(Option<Lease<Resize>>.None);
    private SlabF silhouette;

    public ContentAttributes(ModularComponent owner, IGrasshopperPlugin plugin, ContentBody<TReading> body, Atom<Option<TReading>> reading) : base(owner) {
        (component, this.plugin, this.body, this.reading) = (owner, plugin, body, reading);
        modular = ModularAttributes.Create(owner);
        Responder.MouseDownHook += Pressed;
        Responder.MouseDragHook += Dragged;
        Responder.MouseUpHook += Lifted;
        Responder.KeyDownHook += Keyed;
        _ = plugin.Session.Run(Hooks.On(h => Responder.LostFocus += h, h => Responder.LostFocus -= h, Cancelled, plugin).Under(scope));
    }

    // --- [LAYOUT]
    SizeF IResizableAttributes.Size {
        get => SizeF.Max(Minimum, SizeF.Min(Owner.CustomValues.Get(SizeKey, body.Ideal), Maximum));
        set {
            SizeF held = SizeF.Max(Minimum, SizeF.Min(Size.Round(value), Maximum));
            if (held == ((IResizableAttributes)this).Size)
                return;

            Owner.CustomValues.Set(SizeKey, held);
            InvalidateLayout();
            Owner.OnLayoutChanged();
        }
    }

    protected override void LayoutCentralBox(Font font) {
        CentralBox = RectangleF.FromCenter(Pivot, ((IResizableAttributes)this).Size + new SizeF(Rim, Rim));
        ContentBox = RectangleF.Inset(CentralBox, new PaddingF(StandardLayout.IconPadding));
    }

    protected override void LayoutInputParameters(Font font) {
        LayoutInputParameters(font, component.ModularInputs.VisibleParameters, component.ModularInputs.VisibleCount);
        _ = toSeq(component.ModularInputs.HiddenParameters).Iter(hidden => Optional(hidden.Attributes as InputParameterAttributes)
            .Iter(held => held.AssignBounds(new RectangleF(Pivot, SizeF.Empty))));
    }

    protected override void LayoutOutputParameters(Font font) {
        LayoutOutputParameters(font, component.ModularOutputs.VisibleParameters, component.ModularOutputs.VisibleCount);
        _ = toSeq(component.ModularOutputs.HiddenParameters).Iter(hidden => Optional(hidden.Attributes as OutputParameterAttributes)
            .Iter(held => held.AssignBounds(new RectangleF(Pivot, SizeF.Empty))));
    }

    protected override void LayoutBounds(Shape shape) {
        base.LayoutBounds(shape);
        silhouette = new SlabF(Bounds, shape.Thickness, shape.Corners);
    }

    protected override void PivotMoved(PointF oldPivot, PointF newPivot) {
        base.PivotMoved(oldPivot, newPivot);
        silhouette = silhouette.Move(newPivot.X - oldPivot.X, newPivot.Y - oldPivot.Y);
    }

    public override bool IsCoincident(PointF point) => silhouette.Contains(point);

    private SizeF Minimum => SizeF.Max(body.Limits.Minimum,
        new SizeF(0f, (StandardLayout.ParameterMinHeight * int.Max(component.ModularInputs.VisibleCount, component.ModularOutputs.VisibleCount)) - Rim));

    private SizeF Maximum => SizeF.Max(Minimum, body.Limits.Maximum);

    private PaddingF Margins => new(CentralBox.Left - Bounds.Left, CentralBox.Top - Bounds.Top, Bounds.Right - CentralBox.Right, Bounds.Bottom - CentralBox.Bottom);

    private Fin<(FrameLimits Limits, PaddingF Margins)> Frame {
        get {
            PaddingF margins = Margins;
            SizeF surround = margins.Size + new SizeF(Rim, Rim);
            return FrameLimits.Validated(Minimum + surround, Maximum + surround, body.Limits.Edges).Map(limits => (limits, margins));
        }
    }

    // --- [DRAWING]
    protected override void DrawBackground(Context context, Skin skin, Capsule capsule, Shade shade) {
        silhouette = capsule.Slab;
        base.DrawBackground(context, skin, capsule, shade);
    }

    protected override void DrawContent(Context context, Skin skin, Capsule capsule, Shade shade) =>
        plugin.Session.Run(reading.ValueIO.Bind(current => current.Traverse(value => body.Paint(context, capsule, shade, ContentBox, value)).As()));

    protected override void DrawForeground(Context context, Skin skin, Capsule capsule, Shade shade) {
        _ = plugin.Session.Run(IO.lift(() => {
            ModularAttributes.ColourInputCategories(modular, context, capsule, component.ModularInputs);
            ModularAttributes.ColourOutputCategories(modular, context, capsule, component.ModularOutputs);
        }));
        base.DrawForeground(context, skin, capsule, shade);
    }

    protected override void DrawForegroundDecorations(Context context, Skin skin, Capsule capsule, Shade shade) =>
        plugin.Session.Run(IO.lift(() => ModularAttributes.DrawForegroundDecorations(modular, context, skin, capsule, shade)));

    // --- [RESIZING]
    private Response Replied(IO<Response> respond, [CallerMemberName] string member = "") =>
        plugin.Session.Answer(respond.Catch(error => Cancelled >> IO.fail<Response>(error)).As(), Response.Release, member);

    private Response Pressed(ResponseMouseArgs args) => Replied(args.Buttons is MouseButtons.Primary ? Began(args.ContentLocation) : IO.pure(Response.Ignored));

    private Response Dragged(ResponseMouseArgs args) =>
        Replied(During(current => IO.lift(() => {
            current.Frame.Continue(args.ContentLocation);
            ((IResizableAttributes)this).Size = current.Frame.Resized.Size - current.Margins.Size - new SizeF(Rim, Rim);
            Pivot = RectangleF.Inset(current.Frame.Resized, current.Margins).Center;
        }).Map(Response.Handled)));

    private Response Lifted(ResponseMouseArgs args) => Replied(args.Buttons is MouseButtons.Primary ? Finished : IO.pure(Response.Ignored));

    private Response Keyed(KeyEventArgs args) =>
        Replied((args.Key, args.Modifiers) switch {
            (Keys.Escape, _) => During(_ => Cancelled.Map(Response.Release)),
            (Keys.S, Keys.None) => During(_ => Resizing.SnapToggled(args).Bind(static _ => Ended).Map(Response.Handled)),
            _ => IO.pure(Response.Ignored),
        });

    private IO<Response> During(Func<Resize, IO<Response>> respond) =>
        track.ValueIO.Bind(held => held.Match(Some: lease => respond(lease.Value), None: static () => IO.pure(Response.Ignored)));

    private static IO<Unit> Ended => IO.lift(static () => Optional(Editor.Instance).Iter(static editor => SnapSession.End(editor.Canvas)));

    private IO<Unit> Cancelled =>
        (from held in track.ValueIO
         from restored in held.Map(static lease => lease.Value).Traverse(Restored).As()
         from cleared in Leases.Cleared(track).Under(scope)
         select unit).Uninterruptible();

    private IO<Unit> Restored(Resize resize) =>
        Ended >> IO.lift(() => {
            ((IResizableAttributes)this).Size = resize.Undo.Size;
            Pivot = resize.Undo.Pivot;
        });

    private IO<Response> Began(PointF point) =>
        (from frame in IO.lift(() => Frame)
         from sizing in Resizing.Began(Bounds, frame.Limits, Some(SnappingConstraints.CreateFromDocument(Owner.Document, Owner.InstanceId)), point)
         from response in sizing.Match(
             Some: current => Cancelled
                 .Bind(_ => Leases.Swapped(track, IO.lift(() => new Resize(current, new ResizeAction(Owner), frame.Margins))).Under(scope))
                 .Map(Response.Capture),
             None: static () => IO.pure(Response.Ignored))
         select response).Uninterruptible();

    private IO<Response> Finished =>
        track.ValueIO.Bind(held => held.Match(
            Some: lease =>
                (from cleared in Leases.Cleared(track).Under(scope)
                 from ended in Ended
                 from recorded in IO.lift(() => Optional(Owner.Document).Iter(document => document.Undo.Do(new VerbNoun("Resize", Owner.Nomen.Name), lease.Value.Undo)))
                 select Response.Release)
                .Catch(error => Restored(lease.Value) >> IO.fail<Response>(error)).As(),
            None: static () => IO.pure(Response.Ignored))).Uninterruptible();

    private sealed record Resize(ResizingFrame Frame, ResizeAction Undo, PaddingF Margins);

    // --- [HINTS]
    Cursor? ICursorAwareAttributes.CursorAt(PointF point) =>
        plugin.Session.Answer(IO.lift(() => Frame).Map(frame => Resizing.CursorAt(Bounds, frame.Limits, point).ValueUnsafe()), failed: null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ModularAttributes {
    // --- [HOST]
    private const string Host = "Grasshopper2.Doc.Attributes.ModularComponentAttributes, Grasshopper2";

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(Host)]
    internal static extern object Create(ModularComponent component);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ColourInputCategories))]
    internal static extern void ColourInputCategories([UnsafeAccessorType(Host)] object attributes, Context context, Capsule capsule, ModularList input);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ColourOutputCategories))]
    internal static extern void ColourOutputCategories([UnsafeAccessorType(Host)] object attributes, Context context, Capsule capsule, ModularList output);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(DrawForegroundDecorations))]
    internal static extern void DrawForegroundDecorations([UnsafeAccessorType(Host)] object attributes, Context context, Skin skin, Capsule capsule, Shade shade);
}
