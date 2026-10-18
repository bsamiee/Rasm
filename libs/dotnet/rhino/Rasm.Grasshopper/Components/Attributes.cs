using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Doc.Attributes;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Primitives;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Rasm.Grasshopper.UI;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Grasshopper.Components;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlotBody<TReading>(Extent Width, Extent Height, Func<TReading, PlotCanvas, RectangleF, IO<Unit>> Paint) {
    public Padding ResizeEdges { get; init; } = new(ResizableAttributes<Component>.EdgeSize);
    public Option<Action<PlotAttributes<TReading>>> Configure { get; init; }

    public static Fin<PlotBody<TReading>> Of(
        (float Minimum, float Ideal, float Maximum) width, (float Minimum, float Ideal, float Maximum) height,
        Func<TReading, PlotCanvas, RectangleF, IO<Unit>> paint) =>
        (Axis(width), Axis(height))
            .Apply((wide, high) => new PlotBody<TReading>(wide, high, paint)).As();

    private static Fin<Extent> Axis((float Minimum, float Ideal, float Maximum) axis) =>
        (CanvasLength.Validated(axis.Minimum),
         CanvasLength.Validated(axis.Ideal),
         CanvasLength.Validated(axis.Maximum))
            .Apply(static (minimum, ideal, maximum) => (Minimum: minimum, Ideal: ideal, Maximum: maximum))
            .As()
            .Bind(static lengths => ValidationResults.ToFin(Extent.Validate(lengths.Minimum, lengths.Ideal, lengths.Maximum, out Extent item), item));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PlotAttributes<TReading> : ComponentAttributes, IResizableAttributes, ICursorAwareAttributes {
    // --- [STATE]
    private const string SizeKey = "Attr.Size";
    private const float Rim = 2f * (StandardLayout.IconPadding + Plots.Inset);
    private readonly ModularComponent component;
    private readonly IPlugInSink sink;
    private readonly Option<(PlotBody<TReading> Body, Atom<Option<TReading>> Reading)> plot;
    private readonly Ref<Option<Either<Resizing, IDisposable>>> track = Ref(Option<Either<Resizing, IDisposable>>.None);
    private Option<(ComponentControl Control, Func<(SizeF Size, float Scale)> Surface)> hosted;
    private SlabF silhouette;

    public Option<Func<PointF, ObjectSolutionState, IO<object>>> Tooltip { get; init; }
    public Option<Func<PointF, IO<Option<Cursor>>>> Cursor { get; init; }

    public PlotAttributes(ModularComponent owner, IPlugInSink sink, PlotBody<TReading> body, Atom<Option<TReading>> reading) : this(owner, sink) {
        plot = Some((body, reading));
        _ = body.Configure.Iter(configure => configure(this));
    }

    public PlotAttributes(ModularComponent owner, IPlugInSink sink) : base(owner) {
        (component, this.sink) = (owner, sink);
        Responder.MouseDownHook += Reply<ResponseMouseArgs>(args => args.Buttons is MouseButtons.Primary ? Began(args.ContentLocation) : IO.pure(Response.Ignored), nameof(Responses.MouseDown));
        Responder.MouseDragHook += Reply<ResponseMouseArgs>(args => Sizing(current => IO.lift(() => {
            current.Frame.Continue(args.ContentLocation);
            ((IResizableAttributes)this).Size = current.Frame.Resized.Size - current.Margins.Size - new SizeF(Rim, Rim);
            Pivot = RectangleF.Inset(current.Frame.Resized, current.Margins).Center;
        }).Map(Response.Handled)), nameof(Responses.MouseDrag));
        Responder.MouseUpHook += Reply<ResponseMouseArgs>(args => args.Buttons is MouseButtons.Primary ? Finished : IO.pure(Response.Ignored), nameof(Responses.MouseUp));
        Responder.KeyDownHook += Reply<KeyEventArgs>(args => args.Key is Keys.Escape
            ? track.ValueIO.Bind(held => held.Match(Some: _ => Released, None: static () => IO.pure(Response.Ignored)))
            : Sizing(_ => args.Key is Keys.S && args.Modifiers is Keys.None ? IO.lift(() => {
                Grasshopper2.Settings.CanvasSnapToObjects.Value = !Grasshopper2.Settings.CanvasSnapToObjects.Value;
                args.Handled = true;
            }).Bind(static _ => SnapSession.End(Editor.Instance.Canvas)).Map(Response.Handled) : IO.pure(Response.Ignored)), nameof(Responses.KeyDown));
        Responder.LostFocus += Callbacks.Handler<EventArgs>(_ => Cancelled, Site(nameof(Responses.LostFocus))).Invoke;
    }

    // --- [LAYOUT]
    SizeF IResizableAttributes.Size {
        get {
            SizeF held = Owner.CustomValues.Get(SizeKey, plot.Match(
                Some: static body => new SizeF(body.Body.Width.Ideal, body.Body.Height.Ideal),
                None: static () => new SizeF(StandardLayout.LargeObjectWidth, StandardLayout.LargeObjectHeight)));
            SizeF shown = plot.IsNone
                ? hosted.Bind(control => control.Control.PreferredHeight(held.Width + (2f * Plots.Inset)))
                    .Map(height => new SizeF(held.Width, height - (2f * Plots.Inset))).IfNone(held)
                : held;
            return SizeF.Max(Minimum, SizeF.Min(shown, Maximum));
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
        Callbacks.Run(hosted.Match(Some: static held => held.Control.SurfaceChanged, None: static () => IO.pure(unit)), Site());
    }

    protected override void PivotMoved(PointF oldPivot, PointF newPivot) {
        base.PivotMoved(oldPivot, newPivot);
        silhouette = silhouette.Move(newPivot.X - oldPivot.X, newPivot.Y - oldPivot.Y);
    }

    public override bool IsCoincident(PointF point) => silhouette.Contains(point);

    private SizeF Minimum => SizeF.Max(plot.Match(Some: static held => new SizeF(held.Body.Width.Minimum, held.Body.Height.Minimum),
        None: static () => new SizeF(CanvasLength.MinValue, CanvasLength.MinValue)),
        new SizeF(0f, (StandardLayout.ParameterMinHeight * int.Max(component.ModularInputs.VisibleCount, component.ModularOutputs.VisibleCount)) - Rim));

    private SizeF Maximum => SizeF.Max(Minimum, plot.Match(Some: static held => new SizeF(held.Body.Width.Maximum, held.Body.Height.Maximum),
        None: static () => new SizeF(CanvasLength.MaxValue, CanvasLength.MaxValue)));

    private PaddingF Margins => new(CentralBox.Left - Bounds.Left, CentralBox.Top - Bounds.Top, Bounds.Right - CentralBox.Right, Bounds.Bottom - CentralBox.Bottom);

    private (SizeF Minimum, SizeF Maximum, Padding Bands, PaddingF Margins) Frame {
        get {
            PaddingF margins = Margins;
            SizeF surround = margins.Size + new SizeF(Rim, Rim);
            SizeF minimum = Minimum + surround;
            SizeF maximum = Maximum + surround;
            Padding edges = plot.Match(Some: static held => held.Body.ResizeEdges, None: static () => new Padding(ResizableAttributes<Component>.EdgeSize));
            return (minimum, maximum, FrameLimits.ResizeBands(minimum, maximum,
                plot.IsNone && hosted.Bind(control => control.Control.PreferredHeight(ContentBox.Width)).IsSome
                    ? new Padding(edges.Left, 0, edges.Right, 0) : edges), margins);
        }
    }

    // --- [DRAWING]
    protected override void DrawBackground(Context context, Skin skin, Capsule capsule, Shade shade) {
        silhouette = capsule.Slab;
        base.DrawBackground(context, skin, capsule, shade);
    }

    protected override void DrawContent(Context context, Skin skin, Capsule capsule, Shade shade) =>
        Callbacks.Run(CanvasPainting.Plotted(context, skin.Shape.FontWord, Responder.HasFocus, canvas =>
            from well in Plots.Well(canvas, ContentBox)
            from figure in Figure(canvas, ContentBox)
            from edge in IO.lift(() => context.Graphics.DrawRectangle(shade.Edge, ContentBox))
            select unit), Site());

    protected override void DrawForeground(Context context, Skin skin, Capsule capsule, Shade shade) {
        Callbacks.Run(DisposalOps.Within(
            () => capsule.Slab.ApexPath,
            path => IO.lift(() => context.Graphics.SetClip(path)).Bind(_ => IO.lift(() =>
                (from side in Sides from parameter in side.Parameters select (side.Side, Parameter: parameter)).Iter(held => {
                    if (!ModularList.ResolveColour(held.Parameter, out Color color))
                        return;

                    const float width = 3f;
                    RectangleF bounds = held.Parameter.Attributes.Bounds;
                    context.Graphics.FillRectangle(color, new RectangleF(held.Side is PlugSide.Input ? bounds.Left : bounds.Right - width, bounds.Top, width, bounds.Height));
                }))),
            path => Callbacks.Each(Seq(IO.lift(context.Graphics.ResetClip), IO.lift(path.Dispose))).Map(unit)), Site());
        base.DrawForeground(context, skin, capsule, shade);
    }

    protected override void DrawForegroundDecorations(Context context, Skin skin, Capsule capsule, Shade shade) =>
        Callbacks.Run(IO.lift(() => {
            Seq<(PlugSide Side, RectangleF Bounds)> separators = Sides.Bind(static side => side.Parameters.Zip(side.Parameters.Tail).Choose(pair => {
                _ = ModularList.ResolveCategory(pair.First, out string before);
                _ = ModularList.ResolveCategory(pair.Second, out string after);
                return string.Equals(before, after, StringComparison.Ordinal) ? None : Some((side.Side, pair.Second.Attributes.Bounds));
            }));
            _ = separators.Iter(separator => {
                const float radius = 4f;
                const float halfPixel = 0.5f;
                const float gap = 1f;
                RectangleF bounds = separator.Bounds;
                float direction = separator.Side is PlugSide.Input ? 1f : -1f;
                float outer = (separator.Side is PlugSide.Input ? bounds.Left : bounds.Right) + (direction * halfPixel);
                float inner = outer + (direction * radius);
                float top = bounds.Top - gap;
                float bottom = bounds.Top + gap;
                using GraphicsPath path = new();
                path.AddArc(new PointF(outer, top - radius), new PointF(inner, top), new SizeF(0f, 1f));
                path.AddArc(new PointF(inner, top), new PointF(inner, bottom), new SizeF(direction, 0f));
                path.AddArc(new PointF(inner, bottom), new PointF(outer, bottom + radius), new SizeF(-direction, 0f));
                context.Graphics.FillPath(shade.Edge, path);
            });
        }), Site());

    private Seq<(PlugSide Side, Seq<IParameter> Parameters)> Sides => Seq(
        (PlugSide.Input, toSeq(component.ModularInputs.VisibleParameters)),
        (PlugSide.Output, toSeq(component.ModularOutputs.VisibleParameters)));

    private IO<Unit> Figure(PlotCanvas canvas, RectangleF well) =>
        hosted.Match(
            Some: control => DisposalOps.Within(() => {
                canvas.Graphics.SaveTransform();
                return canvas.Graphics;
            }, graphics => IO.lift(() => graphics.TranslateTransform(well.Location)).Bind(_ => control.Control.Render(canvas, well.Size, interactive: true)),
                static graphics => IO.lift(graphics.RestoreTransform)),
            None: () => plot.Match(Some: held => held.Reading.ValueIO.Bind(value => value.Match(
                Some: shown => held.Body.Paint(shown, canvas, well), None: static () => IO.pure(unit))), None: static () => IO.pure(unit)));

    // --- [HOSTING]
    public IO<Unit> Bind(RowScope scope, Seq<ControlRow> rows, Seq<EntryKey> keys, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from grid in RowGrid.Realize(rows, rows.Filter(row => row.Keys.Exists(key => keys.Exists(key.Equals))), scope.Hosted(Editor.Instance.Canvas), owned)
            from bound in grid.Plot(keys).Traverse(control => Bind(control, owned)).As()
            select unit, retain);

    public IO<Unit> Bind(ComponentControl control, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep => {
            Canvas canvas = Editor.Instance.Canvas;
            float Scale() => canvas.ParentWindow.LogicalPixelSize * canvas.Projection.Zoom;
            IO<Response> Mouse(ResponseMouseArgs args, Func<SizeF, float, MouseEventArgs, IO<Unit>> act) =>
                from local in IO.lift(() => new MouseEventArgs(args.Buttons, args.Modifiers, new PointF(args.ContentLocation.X - ContentBox.X, args.ContentLocation.Y - ContentBox.Y),
                    args.Delta, args.Pressure, args.UnderlyingEtoEventArgs.IsDirectionInverted))
                from acted in act(ContentBox.Size, Scale(), local)
                from answered in IO.lift(() => {
                    args.Handled |= local.Handled;
                    return control.Captured ? Response.Capture : local.Handled ? Response.Handled : Response.Ignored;
                })
                select answered;
            Option<(ComponentControl Control, Func<(SizeF Size, float Scale)> Surface)> prior = hosted;
            Func<(SizeF Size, float Scale)> surface = () => (ContentBox.Size, Scale());
            IO<Unit> cancel = IO.lift(() => ContentBox.Size).Bind(size => control.Cancel(size, Scale()));
            Func<ResponseMouseArgs, Response> press = Reply<ResponseMouseArgs>(args =>
                DisposalOps.OnFailure(Mouse(args, control.Press).Bind(reply => reply is Response.Capture
                    ? Capture(owned => DisposalOps.Acquire(IO.lift(() => new Disposal<IO<Unit>>(cancel, static effect => effect.Run())), owned).Map(unit))
                    : IO.pure(reply)), cancel));
            Func<ResponseMouseArgs, Response> drag = Reply<ResponseMouseArgs>(args => Mouse(args, control.Move));
            Func<ResponseMouseArgs, Response> up = Reply<ResponseMouseArgs>(args =>
                from captured in IO.lift(() => control.Captured)
                from reply in Mouse(args, control.Release)
                from released in captured ? Released : IO.pure(reply)
                select released);
            Func<ResponseMouseArgs, Response> twice = Reply<ResponseMouseArgs>(args => Mouse(args, control.DoubleClick));
            Func<ResponseMouseArgs, Response> wheel = Reply<ResponseMouseArgs>(args => Mouse(args, control.Scroll));
            Action<ResponseMouseArgs> over = Observe<ResponseMouseArgs>(args => Mouse(args, control.Move).Map(unit));
            System.Action leave = () => Callbacks.Started(DisposalOps.OnFailure(control.Leave(ContentBox.Size, Scale()), Cancelled), Site());
            Func<KeyEventArgs, Response> key = Reply<KeyEventArgs>(args => control.Key(ContentBox.Size, Scale(), args)
                .Map(_ => control.Captured ? Response.Capture : args.Handled ? Response.Handled : Response.Ignored));
            EventHandler redraw = (_, _) => Owner.OnDisplayChanged();
            EventHandler preferred = (_, _) => { InvalidateLayout(); Owner.OnLayoutChanged(); };
            return from initial in IO.lift(() => Layout(Skin.Current.Shape))
                   from borrowed in control.Hosted(canvas, surface, keep)
                   from down in Subscriptions.Attach(add => Responder.MouseDownHook += add, remove => Responder.MouseDownHook -= remove, press, keep)
                   from moved in Subscriptions.Attach(add => Responder.MouseDragHook += add, remove => Responder.MouseDragHook -= remove, drag, keep)
                   from raised in Subscriptions.Attach(add => Responder.MouseUpHook += add, remove => Responder.MouseUpHook -= remove, up, keep)
                   from doubled in Subscriptions.Attach(add => Responder.MouseDoubleClickHook += add, remove => Responder.MouseDoubleClickHook -= remove, twice, keep)
                   from scrolled in Subscriptions.Attach(add => Responder.MouseWheelHook += add, remove => Responder.MouseWheelHook -= remove, wheel, keep)
                   from hovered in Subscriptions.Attach(add => Responder.MouseOverHook += add, remove => Responder.MouseOverHook -= remove, over, keep)
                   from left in Subscriptions.Attach(add => Responder.MouseLeaveHook += add, remove => Responder.MouseLeaveHook -= remove, leave, keep)
                   from keyed in Subscriptions.Attach(add => Responder.KeyDownHook += add, remove => Responder.KeyDownHook -= remove, key, keep)
                   from invalidated in Subscriptions.Attach(add => control.Redraw += add, remove => control.Redraw -= remove, redraw, keep)
                   from fitted in Subscriptions.Attach(add => control.PreferredSizeChanged += add, remove => control.PreferredSizeChanged -= remove, preferred, keep)
                   from bound in DisposalOps.Acquire(IO.lift(() => new Disposal<ComponentControl>(control, released => {
                       bool current = hosted.Exists(bound => ReferenceEquals(bound.Surface, surface));
                       hosted = hosted.Filter(_ => !current);
                       _ = (current ? Callbacks.Each(Seq(Cancelled, released.Leave(ContentBox.Size, Scale()))).Map(unit) : cancel).Uninterruptible().Run();
                   })), keep)
                   from arranged in DisposalOps.OnFailure(IO.lift(() => {
                       hosted = Some((control, surface));
                       InvalidateLayout();
                       Layout(Skin.Current.Shape);
                   }), IO.lift(() => { hosted = prior; InvalidateLayout(); }))
                   select unit;
        }, retain);

    // --- [INTERACTION]
    public Func<TArgs, Response> Reply<TArgs>(Func<TArgs, IO<Response>> respond, [CallerMemberName] string member = "") =>
        args => Callbacks.Answer(args, value => DisposalOps.OnFailure(IO.pure(value).Bind(respond), Cancelled), static () => Response.Release, Site(member));

    public Action<TArgs> Observe<TArgs>(Func<TArgs, IO<Unit>> observe, [CallerMemberName] string member = "") =>
        args => Callbacks.Started(DisposalOps.OnFailure(IO.pure(args).Bind(observe), Cancelled), Site(member));

    public IO<Response> Capture(Func<Action<IDisposable>, IO<Unit>> begin) =>
        DisposalOps.Replace(begin, (_, release) => atomic(() => {
            Option<Either<Resizing, IDisposable>> prior = track.Value;
            track.Value = Some(Right<Resizing, IDisposable>(release));
            return Cleaned(prior);
        })).Map(Response.Capture);

    public IO<Response> Released => Cancelled.Map(Response.Release);

    private CallbackSite Site([CallerMemberName] string member = "") => new(sink, typeof(PlotAttributes<TReading>), member);

    private IO<Response> Sizing(Func<Resizing, IO<Response>> respond) =>
        track.ValueIO.Bind(held => held.Match(
            Some: current => current.Match(Left: respond, Right: static _ => IO.pure(Response.Ignored)),
            None: static () => IO.pure(Response.Ignored)));

    private IO<Option<Either<Resizing, IDisposable>>> Taken => atomicIO(() => {
        Option<Either<Resizing, IDisposable>> held = track.Value;
        track.Value = None;
        return held;
    });

    private IO<Unit> Cancelled => Taken.Bind(Cleaned).Uninterruptible();

    private IO<Unit> Cleaned(Option<Either<Resizing, IDisposable>> held) => held.Match(
        Some: current => current.Match(
            Left: resize => Callbacks.Each(Seq(SnapSession.End(Editor.Instance.Canvas),
                IO.lift(() => { ((IResizableAttributes)this).Size = resize.Undo.Size; }), IO.lift(() => { Pivot = resize.Undo.Pivot; }))).Map(unit),
            Right: static release => IO.lift(release.Dispose)),
        None: static () => IO.pure(unit));

    // --- [RESIZING]
    private IO<Response> Began(PointF point) =>
        IO.lift(() => {
            (SizeF minimum, SizeF maximum, Padding bands, PaddingF margins) = Frame;
            ResizingFrame frame = new(Bounds, minimum, maximum, SnappingConstraints.CreateFromDocument(Owner.Document, Owner.InstanceId), SnappingSettings.Current);
            return frame.Begin(point, bands) ? Some((Frame: frame, Margins: margins)) : None;
        }).Bind(held => held.Match(
            Some: current => Cancelled.Bind(_ => IO.lift(() => new ResizeAction(Owner)))
                .Bind(undo => atomicIO(() => track.Value = Some<Either<Resizing, IDisposable>>(new Resizing(current.Frame, undo, current.Margins))))
                .Map(Response.Capture),
            None: static () => IO.pure(Response.Ignored))).Uninterruptible();

    private IO<Response> Finished =>
        atomicIO(() => track.Value.Bind(current => current.Match(
            Left: resize => { track.Value = None; return Some(resize); },
            Right: static _ => Option<Resizing>.None)))
        .Bind(held => held.Match(
            Some: done => DisposalOps.OnFailure(
                SnapSession.End(Editor.Instance.Canvas).Bind(_ => IO.lift(() => Optional(Owner.Document).Iter(document =>
                    document.Undo.Do(new VerbNoun(RowText.Localize("Resize", sink), Owner.InstanceId.ToString()), done.Undo))))
                    .Map(Response.Release),
                Cleaned(Some<Either<Resizing, IDisposable>>(done))),
            None: static () => IO.pure(Response.Ignored))).Uninterruptible();

    private sealed record Resizing(ResizingFrame Frame, ResizeAction Undo, PaddingF Margins);

    // --- [HINTS]
    Cursor? ICursorAwareAttributes.CursorAt(PointF point) => Callbacks.Answer(
        IO.lift(() => {
            (SizeF Minimum, SizeF Maximum, Padding Bands, PaddingF Margins) frame = Frame;
            return Optional(new ResizingFrame(Bounds, frame.Minimum, frame.Maximum).CursorAt(point, frame.Bands));
        }).Bind(native => native.Match(
            Some: static cursor => IO.pure(Some(cursor)),
            None: () => hosted.Filter(_ => ContentBox.Contains(point)).Match(Some: static control => IO.pure(Optional(control.Control.Cursor)),
                None: () => Cursor.Match(Some: custom => IO.pure(point).Bind(custom), None: static () => IO.pure(Option<Cursor>.None)))))
        .Map(static cursor => cursor.ValueUnsafe()), static () => null, Site(nameof(ICursorAwareAttributes.CursorAt)));

    protected override object? TooltipDetails(PointF point, ObjectSolutionState state) => Callbacks.Answer(
        Tooltip.Map(custom => IO.pure(point).Bind(location => custom(location, state)).Map(Some)),
        () => hosted.IsSome || plot.Exists(static held => held.Reading.Value.IsSome)
            ? Some<object>(CanvasChrome.Painter(Editor.Instance.Canvas, Site(nameof(TooltipDetails)), Figure)) : Optional(base.TooltipDetails(point, state)),
        static () => None, Site()).ValueUnsafe();
}
