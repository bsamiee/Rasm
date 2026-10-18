using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.Undo;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct FrameLimits {
    public SizeF Minimum { get; }
    public SizeF Maximum { get; }
    public Padding Edges { get; }

    public static Padding ResizeBands(SizeF minimum, SizeF maximum, Padding edges) => new() {
        Left = minimum.Width == maximum.Width ? 0 : edges.Left,
        Right = minimum.Width == maximum.Width ? 0 : edges.Right,
        Top = minimum.Height == maximum.Height ? 0 : edges.Top,
        Bottom = minimum.Height == maximum.Height ? 0 : edges.Bottom,
    };

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref SizeF minimum, ref SizeF maximum, ref Padding edges) =>
        (validationError, edges) = (0f <= minimum.Width && minimum.Width <= maximum.Width && float.IsFinite(maximum.Width)
            && 0f <= minimum.Height && minimum.Height <= maximum.Height && float.IsFinite(maximum.Height)
            ? null : new ValidationError("resize limits need finite non-negative minimums no larger than their maximums"), ResizeBands(minimum, maximum, edges));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class FrameResponder : Responses, IResponsive {
    // --- [STATE]
    private readonly FlexControl control;
    private readonly FrameLimits limits;
    private readonly Func<RectangleF, IO<Unit>> placed;
    private readonly IPlugInSink sink;
    private readonly Ref<Option<Track>> track = Ref(Option<Track>.None);

    private FrameResponder(FlexControl control, RectangleF bounds, FrameLimits limits, Func<RectangleF, IO<Unit>> placed, IPlugInSink sink) {
        (this.control, this.limits, this.placed, this.sink, RegionBoundary) = (control, limits, placed, sink, bounds);
        LostFocus += Callbacks.Handler<EventArgs>(_ => Cancelled, Site(nameof(LostFocus))).Invoke;
    }

    public Responses Responder => this;

    // --- [REGISTER]
    public static IO<FrameResponder> Register(
        FlexControl control, RectangleF bounds, FrameLimits limits, Func<RectangleF, IO<Unit>> placed, IPlugInSink sink, Action<IDisposable> retain) =>
        (from frame in IO.lift(() => new FrameResponder(control, bounds, limits, placed, sink))
         let lease = new Disposal<FrameResponder>(frame, static held =>
             Callbacks.Each(Seq(held.Cancelled, IO.lift(() => held.control.PopFocus(held)),
                 IO.lift(() => held.control.UnregisterIResponsive(held)).Map(unit),
                 IO.lift(() => { held.control.Cursor = Cursors.Default; }))).Map(static _ => unit).Uninterruptible().Run())
         from registered in DisposalOps.OnFailure(IO.lift(() => { control.RegisterIResponsive(frame); retain(lease); }), IO.lift(lease.Dispose))
         select frame).Uninterruptible();

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
                .Bind(_ => current.Cleared).Map(Response.Handled),
            _ => IO.pure(Response.Handled),
        }));

    public override void MouseOver(ResponseMouseArgs e) =>
        Callbacks.Run(Callbacks.Each(Seq(
            IO.lift(() => {
                control.Cursor = Optional(new ResizingFrame(RegionBoundary, limits.Minimum, limits.Maximum).CursorAt(e.ContentLocation, limits.Edges)).IfNone(Cursors.Default);
            }),
            IO.lift(() => base.MouseOver(e)))).Map(unit), Site());

    public override void MouseLeave() =>
        Callbacks.Run(Callbacks.Each(Seq(
            IO.lift(() => { control.Cursor = Cursors.Default; }), IO.lift(base.MouseLeave))).Map(unit), Site());

    private Response Answered(Func<Response> host, IO<Response> own, [CallerMemberName] string member = "") =>
        Callbacks.Answer(DisposalOps.OnFailure(IO.lift(host).Bind(reply => reply is Response.Ignored ? own : IO.pure(reply)), Cancelled), static () => Response.Release, Site(member));

    private CallbackSite Site([CallerMemberName] string member = "") => new(sink, typeof(FrameResponder), member);

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
             ? IO.pure(new Track(frame.Original, location => IO.lift(() => frame.Continue(location)).Map(_ => frame.Resized),
                 snapped.Match(Some: SnapSession.End, None: static () => unitIO)))
             : control is Canvas { Document: { } document } canvas
                 ? SnapSession.Begin(document, start, Seq<Guid>(), None).Map(session => new Track(session.Start,
                     location => session.Step(canvas, new SizeF(location.X - point.X, location.Y - point.Y), FlexControl.ShiftKeyDown)
                         .Map(offset => session.Start.Move(offset.Width, offset.Height)), SnapSession.End(canvas)))
                 : IO.pure(new Track(start, location => IO.pure(start.Move(location.X - point.X, location.Y - point.Y)), unitIO))
         from held in atomicIO(() => track.Value = Some(next))
         select Response.Capture).Uninterruptible();

    private IO<Response> During(Func<Track, IO<Response>> act) =>
        track.ValueIO.Bind(held => held.Match(Some: act, None: static () => IO.pure(Response.Ignored)));

    private IO<Unit> Shown(RectangleF bounds) =>
        IO.lift(() => {
            RegionBoundary = bounds;
            OnRedrawRequired();
        });

    private IO<Option<Track>> Taken =>
        atomicIO(() => {
            Option<Track> held = track.Value;
            track.Value = None;
            return held;
        });

    private IO<Unit> Cancelled =>
        Taken.Bind(held => held.Match(
            Some: current => Callbacks.Each(Seq(current.Cleared, Shown(current.Origin))).Map(unit),
            None: static () => IO.pure(unit))).Uninterruptible();

    private IO<Response> Finished =>
        DisposalOps.Transacted(
            Taken,
            held => held.Match(
                Some: current => current.Cleared.Bind(_ => IO.lift(() => RegionBoundary)).Bind(placed).Map(Response.Release),
                None: static () => IO.pure(Response.Ignored)),
            held => held.Match(Some: current => Shown(current.Origin), None: static () => unitIO),
            static (_, response) => IO.pure(response));

    private sealed record Track(RectangleF Origin, Func<PointF, IO<RectangleF>> Step, IO<Unit> Cleared);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Responders {
    // --- [MENUS]
    public static IO<Unit> Menu(FlexControl control, Seq<Seq<CommandRow>> groups, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from menus in IO.lift(static () => Ref(Some(HashMap<ContextMenu, IDisposable>())))
            from live in DisposalOps.Acquire(IO.lift(() => new Disposal<Ref<Option<HashMap<ContextMenu, IDisposable>>>>(menus, static held =>
                atomicIO(() => Taken(held)).Bind(DisposalOps.Release).Uninterruptible().Run())), keep)
            from attached in GrasshopperEvents.PopulateContextMenu(control).Inline(args => Populated(args, groups, scope, menus), scope.Sink, keep)
            select unit, retain);

    private static Seq<IDisposable> Taken(Ref<Option<HashMap<ContextMenu, IDisposable>>> held) {
        Option<HashMap<ContextMenu, IDisposable>> taken = held.Value;
        held.Value = None;
        return taken.ToSeq().Bind(static map => toSeq(map.Values));
    }

    private static IO<Unit> Populated(PopulateContextMenuEventArgs args, Seq<Seq<CommandRow>> groups, RowScope scope, Ref<Option<HashMap<ContextMenu, IDisposable>>> menus) =>
        IO.lift(() => args.Control is not Canvas canvas
            || (args.IsMenu && canvas.ResolvePick(
                canvas.Map(args.MouseEvent.Location, CoordinateSystem.Control, CoordinateSystem.Content),
                includeGrips: false, includeForeground: true, includeBackground: true, includeWires: true, recursive: true).Kind is Pick.None))
            .Bind(joins => when(joins, Filled(args.Menu, groups, scope, menus)).As());

    private static IO<Unit> Filled(ContextMenu host, Seq<Seq<CommandRow>> groups, RowScope scope, Ref<Option<HashMap<ContextMenu, IDisposable>>> menus) =>
        DisposalOps.Replace(keep =>
            from site in IO.pure(new CallbackSite(scope.Sink, typeof(ContextMenu), nameof(ContextMenu.Closed)))
            from realized in groups.TraverseM(@group => @group.TraverseM(command => command.Commands(scope, keep)).As()).As()
            let menu = CommandRow.Menu(realized)
            from filled in ChoiceRows.Populate(host,
                menu.Groups.Map(static @group => @group.Map<MenuEntry>(static command => new MenuEntry.Item(command))), Seq(menu.Refresh), None, scope.Sink, keep)
            from closed in Subscriptions.Attach(
                handler => host.Closed += handler, handler => host.Closed -= handler,
                Callbacks.Handler<EventArgs>(_ => atomicIO(() => {
                    Option<IDisposable> held = menus.Value.Bind(map => map.Find(host));
                    menus.Value = menus.Value.Map(map => map.Remove(host));
                    return held;
                }).Bind(static held => IO.lift(() => held.Iter(static lease => lease.Dispose()))).Uninterruptible(), site), keep)
            select unit, (_, lease) => atomicIO(Fin<Unit> () => {
                Option<HashMap<ContextMenu, IDisposable>> held = menus.Value;
                menus.Value = held.Map(map => map.Add(host, lease));
                return held.IsSome ? unit : Errors.Cancelled;
            }).Bind(IO.lift));

    // --- [NUDGE]
    public static IO<Unit> Nudge(Canvas canvas, IPlugInSink sink, Action<IDisposable> retain) =>
        Subscriptions.Host<KeyEventArgs>(typeof(Control), handler => canvas.KeyDown += handler, handler => canvas.KeyDown -= handler, nameof(Control.KeyDown))
            .Inline(args => Nudged(canvas, args, sink), sink, retain);

    private static IO<Unit> Nudged(Canvas canvas, KeyEventArgs args, IPlugInSink sink) =>
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
            new VerbNoun(RowText.Localize("Nudge", sink), RowText.Localize("Selection", sink)))).As()
        from shown in when(moved.Exists(static any => any), IO.lift(() => {
            args.Handled = true;
            canvas.Invalidate();
        })).As()
        select unit;
}
