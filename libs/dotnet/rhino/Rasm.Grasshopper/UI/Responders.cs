using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.Undo;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<GrasshopperFault>]
public sealed partial class FrameLimits {
    public SizeF Minimum { get; }
    public SizeF Maximum { get; }
    public Padding Edges { get; }

    public static Padding ResizeBands(SizeF minimum, SizeF maximum, Padding edges) => new() {
        Left = minimum.Width == maximum.Width ? 0 : edges.Left,
        Right = minimum.Width == maximum.Width ? 0 : edges.Right,
        Top = minimum.Height == maximum.Height ? 0 : edges.Top,
        Bottom = minimum.Height == maximum.Height ? 0 : edges.Bottom,
    };

    static partial void ValidateFactoryArguments(ref GrasshopperFault? validationError, ref SizeF minimum, ref SizeF maximum, ref Padding edges) =>
        (validationError, edges) = (0f <= minimum.Width && minimum.Width <= maximum.Width && float.IsFinite(maximum.Width)
            && 0f <= minimum.Height && minimum.Height <= maximum.Height && float.IsFinite(maximum.Height)
            ? null
            : new GrasshopperFault($"(minimum, maximum) = ({minimum}, {maximum})", GrasshopperReason.Outside),
            ResizeBands(minimum, maximum, edges));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameResponder : Responses, IResponsive {
    // --- [STATE]
    private readonly FlexControl control;
    private readonly FrameLimits limits;
    private readonly Func<RectangleF, IO<Unit>> placed;
    private readonly IPlugInLifetime owner;
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Lease<Track>>> track = Ref(Option<Lease<Track>>.None);

    private FrameResponder(FlexControl control, RectangleF bounds, FrameLimits limits, Func<RectangleF, IO<Unit>> placed, IPlugInLifetime owner) {
        (this.control, this.limits, this.placed, this.owner, RegionBoundary) = (control, limits, placed, owner, bounds);
        _ = owner.Run(Subscriptions.Host<EventHandler, EventArgs>(h => LostFocus += h, h => LostFocus -= h, static handler => handler.Invoke)
            .Inline(_ => Cancelled, owner).Under(scope));
    }

    public Responses Responder => this;

    // --- [REGISTER]
    public static IO<FrameResponder> Register(FlexControl control, RectangleF bounds, FrameLimits limits, Func<RectangleF, IO<Unit>> placed, IPlugInLifetime owner) =>
        use(IO.lift(() => {
            FrameResponder frame = new(control, bounds, limits, placed, owner);
            control.RegisterIResponsive(frame);
            return frame;
        }), static held => Callbacks.Each(Seq(held.Cancelled, IO.lift(() => held.control.PopFocus(held)),
            IO.lift(() => held.control.UnregisterIResponsive(held)).Map(unit),
            IO.lift(() => { held.control.Cursor = Cursors.Default; }),
            IO.lift(held.scope.Dispose))).Map(unit));

    // --- [RESPONSES]
    public override Response MouseDown(ResponseMouseArgs e) =>
        Answered(() => base.MouseDown(e), e.Buttons is MouseButtons.Primary ? Began(e.ContentLocation) : IO.pure(Response.Ignored));

    public override Response MouseDoubleClick(ResponseMouseArgs e) =>
        Answered(() => base.MouseDoubleClick(e), e.Buttons is MouseButtons.Primary ? Began(e.ContentLocation) : IO.pure(Response.Ignored));

    public override Response MouseDrag(ResponseMouseArgs e) =>
        Answered(() => base.MouseDrag(e), During(current => current.Step(e.ContentLocation).Bind(Shown).Map(Response.Handled)));

    public override Response MouseUp(ResponseMouseArgs e) =>
        Answered(() => base.MouseUp(e), e.Buttons is MouseButtons.Primary ? Finished : During(static _ => IO.pure(Response.Handled)));

    public override Response KeyDown(KeyEventArgs e) =>
        Answered(() => base.KeyDown(e), During(current => (e.Key, e.Modifiers) switch {
            (Keys.Escape, _) => Cancelled.Map(Response.Release),
            (Keys.S, Keys.None) => IO.lift(static () => { Settings.CanvasSnapToObjects.Value = !Settings.CanvasSnapToObjects.Value; })
                .Bind(_ => current.Snapped.Traverse(SnapSession.End).As()).Map(Response.Handled),
            _ => IO.pure(Response.Handled),
        }));

    public override void MouseOver(ResponseMouseArgs e) =>
        owner.Run(Callbacks.Each(Seq(
            IO.lift(() => {
                control.Cursor = Optional(new ResizingFrame(RegionBoundary, limits.Minimum, limits.Maximum).CursorAt(e.ContentLocation, limits.Edges)).IfNone(Cursors.Default);
            }),
            IO.lift(() => base.MouseOver(e)))));

    public override void MouseLeave() =>
        owner.Run(Callbacks.Each(Seq(
            IO.lift(() => { control.Cursor = Cursors.Default; }), IO.lift(base.MouseLeave))));

    private Response Answered(Func<Response> host, IO<Response> own, [CallerMemberName] string member = "") =>
        owner.Answer(IO.lift(host).Bind(reply => reply is Response.Ignored ? own : IO.pure(reply)).Catch(error => Cancelled >> IO.fail<Response>(error)).As(), Response.Release, member);

    // --- [TRACKS]
    private IO<Response> Began(PointF point) =>
        (from cancelled in Cancelled
         from snapped in IO.lift(() =>
             from canvas in Optional(control as Canvas)
             from editor in Optional(Editor.Instance)
             where ReferenceEquals(canvas, editor.Canvas) && canvas.Document is not null
             select canvas)
         let start = RegionBoundary
         from frame in IO.lift(() => new ResizingFrame(
             start, limits.Minimum, limits.Maximum, snapped.Map(static canvas => SnappingConstraints.CreateFromDocument(canvas.Document)).ValueUnsafe(), SnappingSettings.Current))
         from sizing in IO.lift(() => frame.Begin(point, limits.Edges))
         from next in sizing
             ? IO.pure(new Track(frame.Original, location => IO.lift(() => frame.Continue(location)).Map(_ => frame.Resized), snapped))
             : control is Canvas { Document: { } document } canvas
                 ? SnapSession.Begin(document, start, Seq<Guid>(), None).Map(session => new Track(session.Start,
                     location => session.Step(canvas, new SizeF(location.X - point.X, location.Y - point.Y), FlexControl.ShiftKeyDown)
                         .Map(offset => session.Start.Move(offset.Width, offset.Height)), Some(canvas)))
                 : IO.pure(new Track(start, location => IO.pure(start.Move(location.X - point.X, location.Y - point.Y)), None))
         from held in Products.Swapped(track, Snapping(next)).Under(scope)
         select Response.Capture).Uninterruptible();

    private static IO<Track> Snapping(Track next) =>
        next.Snapped.Traverse(static canvas => use(IO.pure(canvas), SnapSession.End)).As().Map(_ => next);

    private IO<Response> During(Func<Track, IO<Response>> act) =>
        track.ValueIO.Bind(held => held.Match(Some: lease => act(lease.Value), None: static () => IO.pure(Response.Ignored)));

    private IO<Unit> Shown(RectangleF bounds) =>
        IO.lift(() => {
            RegionBoundary = bounds;
            OnRedrawRequired();
        });

    private IO<Option<Track>> Taken =>
        from held in track.ValueIO
        from cleared in Products.Cleared(track).Under(scope)
        select held.Map(static lease => lease.Value);

    private IO<Unit> Cancelled =>
        Taken.Bind(held => held.Traverse(current => Shown(current.Origin)).As()).Map(unit).Uninterruptible();

    private IO<Response> Finished =>
        Taken.Bind(held => held.Match(
            Some: current => IO.lift(() => RegionBoundary).Bind(placed).Map(Response.Release).Catch(error => Shown(current.Origin) >> IO.fail<Response>(error)).As(),
            None: static () => IO.pure(Response.Ignored)));

    private sealed record Track(RectangleF Origin, Func<PointF, IO<RectangleF>> Step, Option<Canvas> Snapped);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Responders {
    // --- [NUDGE]
    public static IO<Unit> Nudge(Canvas canvas, IPlugInLifetime owner) =>
        Subscriptions.During<KeyEventArgs>(h => canvas.KeyDown += h, h => canvas.KeyDown -= h)
            .Inline(args => Nudged(canvas, args, owner), owner);

    private static IO<Unit> Nudged(Canvas canvas, KeyEventArgs args, IPlugInLifetime owner) =>
        from move in IO.lift(() =>
            from document in Optional(canvas.Document)
            where !args.Handled
            from direction in args.Key switch {
                Keys.Left => Some(new SizeF(-1f, 0f)),
                Keys.Right => Some(new SizeF(1f, 0f)),
                Keys.Up => Some(new SizeF(0f, -1f)),
                Keys.Down => Some(new SizeF(0f, 1f)),
                _ => Option<SizeF>.None,
            }
            from step in args.Modifiers switch {
                Keys.None => Some(new SizeF(1f, 1f)),
                Keys.Shift => Some(new SizeF(SnappingSettings.Current.HorizontalGapSize, SnappingSettings.Current.VerticalGapSize)),
                _ => Option<SizeF>.None,
            }
            select (Document: document, Offset: direction * step))
        from moved in move.Traverse(held => CanvasLayout.Nudged(held.Document, held.Offset,
            new VerbNoun(RowText.Localize("Nudge", owner), RowText.Localize("Selection", owner)))).As()
        from shown in when(moved.Exists(static any => any), IO.lift(() => {
            args.Handled = true;
            canvas.Invalidate();
        })).As()
        select unit;
}
