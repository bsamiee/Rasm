using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class FrameLimits {
    public SizeF Minimum { get; }
    public SizeF Maximum { get; }
    public Padding Edges { get; }

    public static Fin<FrameLimits> Validated(SizeF minimum, SizeF maximum, Padding edges) =>
        GrasshopperFault.Unless(
            0f <= minimum.Width && minimum.Width <= maximum.Width && float.IsFinite(maximum.Width)
            && 0f <= minimum.Height && minimum.Height <= maximum.Height && float.IsFinite(maximum.Height),
            GrasshopperReason.Outside, (minimum, maximum))
        .Map(_ => new FrameLimits(minimum, maximum, new Padding(
            minimum.Width == maximum.Width ? 0 : edges.Left,
            minimum.Height == maximum.Height ? 0 : edges.Top,
            minimum.Width == maximum.Width ? 0 : edges.Right,
            minimum.Height == maximum.Height ? 0 : edges.Bottom)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameResponder : Responses, IResponsive {
    // --- [STATE]
    private readonly FlexControl control;
    private readonly FrameLimits limits;
    private readonly Func<RectangleF, IO<Unit>> placed;
    private readonly IGrasshopperPlugin owner;
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Lease<Track>>> track = Ref(Option<Lease<Track>>.None);

    private FrameResponder(FlexControl control, RectangleF bounds, FrameLimits limits, Func<RectangleF, IO<Unit>> placed, IGrasshopperPlugin owner) {
        (this.control, this.limits, this.placed, this.owner, RegionBoundary) = (control, limits, placed, owner, bounds);
        _ = owner.Session.Run(Hooks.On(h => LostFocus += h, h => LostFocus -= h, Cancelled, owner).Under(scope));
    }

    public Responses Responder => this;

    // --- [REGISTER]
    public static IO<FrameResponder> Register(FlexControl control, RectangleF bounds, FrameLimits limits, Func<RectangleF, IO<Unit>> placed, IGrasshopperPlugin owner) =>
        use(IO.lift(() => {
            FrameResponder frame = new(control, bounds, limits, placed, owner);
            control.RegisterIResponsive(frame);
            return frame;
        }), static held =>
            held.Cancelled
            >> IO.lift(() => held.control.PopFocus(held))
            >> IO.lift(() => held.control.UnregisterIResponsive(held))
            >> IO.lift(() => { held.control.Cursor = Cursors.Default; })
            >> IO.lift(held.scope.Dispose));

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
            (Keys.S, Keys.None) => Resizing.SnapToggled(e).Bind(_ => current.Snapped.Traverse(SnapSession.End).As()).Map(Response.Handled),
            _ => IO.pure(Response.Handled),
        }));

    public override void MouseOver(ResponseMouseArgs e) =>
        owner.Session.Run(IO.lift(() => { control.Cursor = Resizing.CursorAt(RegionBoundary, limits, e.ContentLocation).IfNone(Cursors.Default); }) >> IO.lift(() => base.MouseOver(e)));

    public override void MouseLeave() =>
        owner.Session.Run(IO.lift(() => { control.Cursor = Cursors.Default; }) >> IO.lift(base.MouseLeave));

    private Response Answered(Func<Response> host, IO<Response> own, [CallerMemberName] string member = "") =>
        owner.Session.Answer(IO.lift(host).Bind(reply => reply is Response.Ignored ? own : IO.pure(reply)).Catch(error => Cancelled >> IO.fail<Response>(error)).As(), Response.Release, member);

    // --- [TRACKS]
    private IO<Response> Began(PointF point) =>
        (from cancelled in Cancelled
         from snapped in IO.lift(() =>
             from canvas in Optional(control as Canvas)
             from editor in Optional(Editor.Instance)
             where ReferenceEquals(canvas, editor.Canvas) && canvas.Document is not null
             select canvas)
         let start = RegionBoundary
         from frame in Resizing.Began(start, limits, snapped.Map(static canvas => SnappingConstraints.CreateFromDocument(canvas.Document)), point)
         from next in frame.Match(
             Some: sizing => IO.pure(new Track(sizing.Original, location => IO.lift(() => sizing.Continue(location)).Map(_ => sizing.Resized), snapped)),
             None: () => control is Canvas { Document: { } document } canvas
                 ? SnapSession.Begin(document, start, Seq<Guid>(), None).Map(session => new Track(session.Start,
                     location => session.Step(canvas, new SizeF(location.X - point.X, location.Y - point.Y), FlexControl.ShiftKeyDown)
                         .Map(offset => session.Start.Move(offset.Width, offset.Height)), Some(canvas)))
                 : IO.pure(new Track(start, location => IO.pure(start.Move(location.X - point.X, location.Y - point.Y)), None)))
         from held in Leases.Swapped(track, Snapping(next)).Under(scope)
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
        from cleared in Leases.Cleared(track).Under(scope)
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
public static class Resizing {
    // --- [FRAMES]
    public static IO<Option<ResizingFrame>> Began(RectangleF bounds, FrameLimits limits, Option<SnappingConstraints> snapping, PointF point) =>
        IO.lift(() => Some(new ResizingFrame(bounds, limits.Minimum, limits.Maximum, snapping.ValueUnsafe(), SnappingSettings.Current)).Filter(frame => frame.Begin(point, limits.Edges)));

    public static Option<Cursor> CursorAt(RectangleF bounds, FrameLimits limits, PointF point) =>
        Optional(new ResizingFrame(bounds, limits.Minimum, limits.Maximum).CursorAt(point, limits.Edges));

    public static IO<Unit> SnapToggled(KeyEventArgs e) =>
        IO.lift(() => {
            Settings.CanvasSnapToObjects.Value = !Settings.CanvasSnapToObjects.Value;
            e.Handled = true;
        });
}
