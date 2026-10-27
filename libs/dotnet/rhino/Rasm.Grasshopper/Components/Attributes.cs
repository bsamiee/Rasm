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
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.UI;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using SkiaSharp;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlotBody<TReading>(Sizing.Fixed Width, Sizing.Fixed Height, Func<TReading, Node> View) {
    public Padding ResizeEdges { get; init; } = new(ResizableAttributes<Component>.EdgeSize);
    public Option<Action<PlotAttributes<TReading>>> Configure { get; init; }

    public static Fin<PlotBody<TReading>> Of(
        (float Minimum, float Ideal, float Maximum) width, (float Minimum, float Ideal, float Maximum) height, Func<TReading, Node> view) =>
        (Axis(width), Axis(height))
            .Apply((wide, high) => new PlotBody<TReading>(wide, high, view)).As();

    private static Fin<Sizing.Fixed> Axis((float Minimum, float Ideal, float Maximum) axis) =>
        Limits.Validated(axis.Minimum, axis.Maximum).Map(limits => new Sizing.Fixed(axis.Ideal) { Limits = limits });
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlotAttributes<TReading> : ComponentAttributes, IResizableAttributes, ICursorAwareAttributes, IDisposable {
    // --- [STATE]
    private const string SizeKey = "Attr.Size";
    private const float Rim = 2f * StandardLayout.IconPadding;
    private readonly ModularComponent component;
    private readonly IPlugInLifetime plugin;
    private readonly PlotBody<TReading> body;
    private readonly Atom<Option<TReading>> reading;
    private readonly object modular;
    private readonly Guest guest;
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Lease<Option<Resizing>>>> track = Ref(Option<Lease<Option<Resizing>>>.None);
    private Option<TReading> read;
    private Option<Node> view;
    private SlabF silhouette;

    public Option<Func<PointF, ObjectSolutionState, IO<object>>> Tooltip { get; init; }
    public Option<Func<PointF, IO<Option<Cursor>>>> Cursor { get; init; }

    public PlotAttributes(ModularComponent owner, IPlugInLifetime plugin, PlotBody<TReading> body, Atom<Option<TReading>> reading) : base(owner) {
        (component, this.plugin, this.body, this.reading) = (owner, plugin, body, reading);
        modular = ModularAttributes.Create(owner);
        guest = new Guest(plugin, IO.lift(Owner.OnDisplayChanged));
        Responder.MouseDownHook += Pressed;
        Responder.MouseDoubleClickHook += Clicked;
        Responder.MouseDragHook += Dragged;
        Responder.MouseUpHook += Lifted;
        Responder.MouseWheelHook += Wheeled;
        Responder.MouseOverHook += Hovered;
        Responder.MouseLeaveHook += Departed;
        Responder.KeyDownHook += Keyed;
        _ = plugin.Run((
            from focused in Subscriptions.Host<EventHandler, EventArgs>(h => Responder.GotFocus += h, h => Responder.GotFocus -= h, static handler => handler.Invoke)
                .Inline(_ => guest.Rehost(Visibility.Active), plugin)
            from blurred in Subscriptions.Host<EventHandler, EventArgs>(h => Responder.LostFocus += h, h => Responder.LostFocus -= h, static handler => handler.Invoke)
                .Inline(_ => Cancelled.Bind(__ => guest.Rehost(Visibility.Shown)), plugin)
            select unit).Under(scope));
        _ = body.Configure.Iter(configure => configure(this));
    }

    // --- [LAYOUT]
    SizeF IResizableAttributes.Size {
        get {
            SizeF held = Owner.CustomValues.Get(SizeKey, new SizeF(body.Width.Extent, body.Height.Extent));
            SizeF preferred = guest.Preferred(new SKSize(held.Width, held.Height)).Map(static size => new SizeF(size.Width, size.Height)).IfNone(held);
            (bool wide, bool high) = Stretched;
            return SizeF.Max(Minimum, SizeF.Min(new SizeF(wide ? held.Width : preferred.Width, high ? held.Height : preferred.Height), Maximum));
        }
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

    private (bool Wide, bool High) Stretched => view.Map(static node => (Stretches(node.Width), Stretches(node.Height))).IfNone((true, true));

    private static bool Stretches(Option<Sizing> sizing) => sizing.Exists(static held => held is Sizing.Grow or Sizing.Percent);

    private SizeF Minimum => SizeF.Max(new SizeF(body.Width.Limits.Min, body.Height.Limits.Min),
        new SizeF(0f, (StandardLayout.ParameterMinHeight * int.Max(component.ModularInputs.VisibleCount, component.ModularOutputs.VisibleCount)) - Rim));

    private SizeF Maximum => SizeF.Max(Minimum, new SizeF(body.Width.Limits.Max, body.Height.Limits.Max));

    private PaddingF Margins => new(CentralBox.Left - Bounds.Left, CentralBox.Top - Bounds.Top, Bounds.Right - CentralBox.Right, Bounds.Bottom - CentralBox.Bottom);

    private (SizeF Minimum, SizeF Maximum, Padding Bands, PaddingF Margins) Frame {
        get {
            PaddingF margins = Margins;
            SizeF surround = margins.Size + new SizeF(Rim, Rim);
            SizeF minimum = Minimum + surround;
            SizeF maximum = Maximum + surround;
            (bool wide, bool high) = Stretched;
            Padding edges = body.ResizeEdges;
            return (minimum, maximum, FrameLimits.ResizeBands(minimum, maximum,
                new Padding(wide ? edges.Left : 0, high ? edges.Top : 0, wide ? edges.Right : 0, high ? edges.Bottom : 0)), margins);
        }
    }

    // --- [DRAWING]
    protected override void DrawBackground(Context context, Skin skin, Capsule capsule, Shade shade) {
        silhouette = capsule.Slab;
        base.DrawBackground(context, skin, capsule, shade);
    }

    protected override void DrawContent(Context context, Skin skin, Capsule capsule, Shade shade) =>
        plugin.Run(
            from before in IO.lift(() => ((IResizableAttributes)this).Size)
            from current in reading.ValueIO
            from viewed in IO.lift(() => Viewed(current))
            from painted in viewed.Traverse(shown =>
                from device in IO.lift(() => Guest.Device(context))
                from drawn in guest.Paint(context.Graphics, device, ContentBox, Responder.HasFocus ? Visibility.Active : Visibility.Shown, shown)
                select unit).As()
            from refit in IO.lift(() => Refit(before))
            select unit);

    private Option<Node> Viewed(Option<TReading> current) {
        if (current.Equals(read)) return view;
        read = current;
        view = current.Map(body.View);
        return view;
    }

    private Unit Refit(SizeF before) {
        if (((IResizableAttributes)this).Size == before) return unit;
        InvalidateLayout();
        Owner.OnLayoutChanged();
        return unit;
    }

    protected override void DrawForeground(Context context, Skin skin, Capsule capsule, Shade shade) {
        _ = plugin.Run(IO.lift(() => {
            ModularAttributes.ColourInputCategories(modular, context, capsule, component.ModularInputs);
            ModularAttributes.ColourOutputCategories(modular, context, capsule, component.ModularOutputs);
        }));
        base.DrawForeground(context, skin, capsule, shade);
    }

    protected override void DrawForegroundDecorations(Context context, Skin skin, Capsule capsule, Shade shade) =>
        plugin.Run(IO.lift(() => ModularAttributes.DrawForegroundDecorations(modular, context, skin, capsule, shade)));

    // --- [INTERACTION]
    public Func<TArgs, Response> Reply<TArgs>(Func<TArgs, IO<Response>> respond, [CallerMemberName] string member = "") =>
        args => Replied(IO.pure(args).Bind(respond), member);

    public Action<TArgs> Observe<TArgs>(Func<TArgs, IO<Unit>> observe, [CallerMemberName] string member = "") =>
        args => plugin.Start(IO.pure(args).Bind(observe).Catch(error => Cancelled >> IO.fail<Unit>(error)).As(), member);

    public IO<Response> Capture(IO<Unit> begin) =>
        Cancelled.Bind(_ => Products.Swapped(track, begin.Map(static _ => Option<Resizing>.None)).Under(scope)).Map(Response.Capture);

    public IO<Response> Released => Cancelled.Map(Response.Release);

    private Response Replied(IO<Response> respond, [CallerMemberName] string member = "") =>
        plugin.Answer(respond.Catch(error => Cancelled >> IO.fail<Response>(error)).As(), Response.Release, member);

    private Response Pressed(ResponseMouseArgs args) =>
        Replied((args.Buttons is MouseButtons.Primary ? Began(args.ContentLocation) : IO.pure(Response.Ignored))
            .Bind(resized => Or(resized, Guided(args, static (local, clicks) => HostInput.Pressed(local, clicks)))));

    private Response Clicked(ResponseMouseArgs args) => Replied(Guided(args, static (local, clicks) => HostInput.Pressed(local, clicks)));

    private Response Dragged(ResponseMouseArgs args) =>
        Replied(During(current => IO.lift(() => {
            current.Frame.Continue(args.ContentLocation);
            ((IResizableAttributes)this).Size = current.Frame.Resized.Size - current.Margins.Size - new SizeF(Rim, Rim);
            Pivot = RectangleF.Inset(current.Frame.Resized, current.Margins).Center;
        }).Map(Response.Handled)).Bind(resized => Or(resized, Guided(args, static (local, clicks) => HostInput.Moved(local, clicks)))));

    private Response Lifted(ResponseMouseArgs args) =>
        Replied((args.Buttons is MouseButtons.Primary ? Finished : IO.pure(Response.Ignored)).Bind(resized => Or(resized, Guided(args, HostInput.Released))));

    private Response Wheeled(ResponseMouseArgs args) => Replied(Guided(args, static (local, _) => Some(HostInput.Wheeled(local))));

    private void Hovered(ResponseMouseArgs args) =>
        plugin.Start(Guided(args, static (local, clicks) => HostInput.Moved(local, clicks)).Catch(error => Cancelled >> IO.fail<Response>(error)).As());

    private void Departed() => plugin.Start(guest.Receive(new Input.Hover(None)));

    private Response Keyed(KeyEventArgs args) =>
        Replied((args.Key, args.Modifiers) switch {
            (Keys.Escape, _) => track.ValueIO.Bind(held => held.Match(Some: _ => Released, None: () => guest.Keyed(args))),
            (Keys.S, Keys.None) => During(_ => IO.lift(() => {
                Grasshopper2.Settings.CanvasSnapToObjects.Value = !Grasshopper2.Settings.CanvasSnapToObjects.Value;
                args.Handled = true;
            }).Bind(static _ => Editors.Canvas.Bind(SnapSession.End)).Map(Response.Handled)).Bind(resized => Or(resized, guest.Keyed(args))),
            _ => guest.Keyed(args),
        });

    private IO<Response> Guided(ResponseMouseArgs args, Func<MouseEventArgs, int, Option<Input>> convert) =>
        IO.lift(() => HostInput.Clicks.Map(clicks => convert(Guest.Local(args, ContentBox.TopLeft), clicks)))
            .Bind(input => input.Match(Some: guest.Receive, None: static () => IO.pure(Response.Ignored)));

    private static IO<Response> Or(Response resized, IO<Response> guided) => resized is Response.Ignored ? guided : IO.pure(resized);

    private IO<Response> During(Func<Resizing, IO<Response>> respond) =>
        track.ValueIO.Bind(held => held.Bind(static lease => lease.Value).Match(Some: respond, None: static () => IO.pure(Response.Ignored)));

    private IO<Unit> Cancelled =>
        (from held in track.ValueIO
         from restored in held.Bind(static lease => lease.Value).Traverse(Restored).As()
         from cleared in Products.Cleared(track).Under(scope)
         select unit).Uninterruptible();

    private IO<Unit> Restored(Resizing resize) =>
        Callbacks.Each(Seq(Editors.Canvas.Bind(SnapSession.End),
            IO.lift(() => { ((IResizableAttributes)this).Size = resize.Undo.Size; }), IO.lift(() => { Pivot = resize.Undo.Pivot; }))).Map(unit);

    // --- [RESIZING]
    private IO<Response> Began(PointF point) =>
        IO.lift(() => {
            (SizeF minimum, SizeF maximum, Padding bands, PaddingF margins) = Frame;
            ResizingFrame frame = new(Bounds, minimum, maximum, SnappingConstraints.CreateFromDocument(Owner.Document, Owner.InstanceId), SnappingSettings.Current);
            return frame.Begin(point, bands) ? Some((Frame: frame, Margins: margins)) : None;
        }).Bind(held => held.Match(
            Some: current => Cancelled.Bind(_ => IO.lift(() => new ResizeAction(Owner)))
                .Bind(undo => Products.Swapped(track, IO.pure(Some(new Resizing(current.Frame, undo, current.Margins)))).Under(scope))
                .Map(Response.Capture),
            None: static () => IO.pure(Response.Ignored))).Uninterruptible();

    private IO<Response> Finished =>
        track.ValueIO.Bind(held => held.Bind(static lease => lease.Value).Match(
            Some: done =>
                (from cleared in Products.Cleared(track).Under(scope)
                 from ended in Editors.Canvas.Bind(SnapSession.End)
                 from recorded in IO.lift(() => Optional(Owner.Document).Iter(document =>
                     document.Undo.Do(new(RowText.Localize("Resize", plugin), Owner.InstanceId.ToString()), done.Undo)))
                 select Response.Release)
                .Catch(error => Restored(done) >> IO.fail<Response>(error)).As(),
            None: static () => IO.pure(Response.Ignored))).Uninterruptible();

    private sealed record Resizing(ResizingFrame Frame, ResizeAction Undo, PaddingF Margins);

    // --- [HINTS]
    Cursor? ICursorAwareAttributes.CursorAt(PointF point) => plugin.Answer(
        IO.lift(() => {
            (SizeF Minimum, SizeF Maximum, Padding Bands, PaddingF Margins) frame = Frame;
            return Optional(new ResizingFrame(Bounds, frame.Minimum, frame.Maximum).CursorAt(point, frame.Bands));
        }).Bind(native => native.Match(
            Some: static cursor => IO.pure(Some(cursor)),
            None: () => guest.Cursor.Filter(_ => ContentBox.Contains(point)).Match(
                Some: static drawn => IO.lift(() => drawn).Map(static cursor => Some(cursor)),
                None: () => Cursor.Match(Some: custom => IO.pure(point).Bind(custom), None: static () => IO.pure(Option<Cursor>.None)))))
        .Map(static cursor => cursor.ValueUnsafe()), failed: null);

    protected override object? TooltipDetails(PointF point, ObjectSolutionState state) =>
        Tooltip.Match(
            Some: custom => plugin.Answer(IO.pure(point).Bind(location => custom(location, state)).Map(static details => Some(details)), Option<object>.None),
            None: () => guest.Help.Map(static help => (object)help)
                | view.Map(shown => (object)Guest.TooltipPainter(plugin, shown))
                | Optional(base.TooltipDetails(point, state)))
        .ValueUnsafe();

    public void Dispose() {
        _ = plugin.Run(Cancelled);
        _ = plugin.Run(IO.lift(scope.DisposeResources));
        scope.Dispose();
        guest.Dispose();
    }
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
