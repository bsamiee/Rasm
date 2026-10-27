using Eto.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.UI;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI.Rows;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RowShape {
    public static readonly RowShape Inline = new(labeled: true, spans: false, fills: false);
    public static readonly RowShape Unlabeled = new(labeled: false, spans: false, fills: false);
    public static readonly RowShape Block = new(labeled: true, spans: true, fills: false);
    public static readonly RowShape Filled = new(labeled: true, spans: true, fills: true);
    public static readonly RowShape Nested = new(labeled: false, spans: true, fills: false);

    public bool Labeled { get; }
    public bool Spans { get; }
    public bool Fills { get; }
}

public sealed record RowLine(Option<string> Caption, Control Value);

public sealed record RowCells(Control Value, IO<Unit> Show) {
    public Option<Control> Pick { get; init; }
    public Seq<RowLine> Lines { get; init; }
    public Seq<Seq<Command<RowRule, Invoke>>> Menu { get; init; }
}

public abstract record ControlRow(string Label, RowShape Shape) {
    public Option<string> Help { get; init; }
    public bool Visible { get; init; } = true;
    public bool Enabled { get; init; } = true;
    public bool Modified { get; init; }

    internal abstract IO<(Option<RowCells> Cells, Func<ControlRow, IO<bool>> Restated)> Seated(RowScope scope);

    public sealed record Group(string Label) : ControlRow(Label, RowShape.Nested) {
        internal override IO<(Option<RowCells> Cells, Func<ControlRow, IO<bool>> Restated)> Seated(RowScope scope) =>
            IO.pure<(Option<RowCells>, Func<ControlRow, IO<bool>>)>((None, static next => IO.pure(next is Group)));
    }

    public sealed record Bound<TRow>(string Label, RowShape Shape, TRow Row, Func<IO<Bound<TRow>>, RowScope, IO<RowCells>> Realize)
        : ControlRow(Label, Shape) where TRow : notnull {
        internal override IO<(Option<RowCells> Cells, Func<ControlRow, IO<bool>> Restated)> Seated(RowScope scope) =>
            from latest in atomIO(this)
            from cells in Realize(latest.ValueIO, scope)
            from shown in cells.Show
            select (Some(cells), (Func<ControlRow, IO<bool>>)(next => next is Bound<TRow> held && held.Realize.Method == Realize.Method
                ? writeIO(latest, held).Bind(_ => cells.Show).Map(static _ => true)
                : IO.pure(value: false)));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ControlChange<TArgs> {
    // --- [STATE]
    private readonly Ref<Option<Lease<Unit>>> attached = Ref(Option<Lease<Unit>>.None);
    private readonly HostEvent<TArgs> changes;
    private readonly Func<TArgs, IO<Unit>> deliver;
    private readonly IPlugInLifetime owner;
    private readonly EnvIO scope;

    private ControlChange(HostEvent<TArgs> changes, Func<TArgs, IO<Unit>> deliver, IPlugInLifetime owner, EnvIO scope) =>
        (this.changes, this.deliver, this.owner, this.scope) = (changes, deliver, owner, scope);

    // --- [ATTACH]
    public static IO<ControlChange<TArgs>> Attached(HostEvent<TArgs> changes, Func<TArgs, IO<Unit>> deliver, IPlugInLifetime owner) =>
        from scope in use(static () => EnvIO.New(token: CancellationToken.None))
        let change = new ControlChange<TArgs>(changes, deliver, owner, scope)
        from listening in change.Listening
        select change;

    public IO<T> Detached<T>(IO<T> write) =>
        Products.Cleared(attached).Under(scope).Bind(_ => write.Finally(Listening));

    private IO<Unit> Listening => Products.Swapped(attached, changes.Inline(deliver, owner)).Under(scope);
}

public sealed class RowGrid {
    // --- [STATE]
    private readonly Ref<Seating> seating = Ref(Seating.Empty);
    private readonly RowScope scope;
    private readonly Panel view;

    private RowGrid(RowScope scope, Panel view) => (this.scope, this.view) = (scope, view);

    public Control View => view;

    // --- [REALIZE]
    public static IO<RowGrid> Realize(ControlRows rows, RowScope scope) =>
        from made in use(() => new RowGrid(scope, new Panel()), static grid => grid.Released)
        from sized in Subscriptions.Host<EventArgs>(h => made.view.SizeChanged += h, h => made.view.SizeChanged -= h).Inline(_ => made.Fitted, scope.Owner)
        from attached in made.Attached(rows)
        select made;

    private IO<Unit> Attached(ControlRows rows) =>
        from post in Mailboxes.Idle<Unit, Unit>(_ => rows.Read(scope).Bind(Show))(scope.Owner)
        from signals in rows.Signals(scope)
        from held in signals.TraverseM(signal => signal.Inline(raised => post(raised, raised), scope.Owner)).As()
        from heard in scope.Listen(_ => post(unit, unit))
        from shown in rows.Read(scope).Bind(Show)
        select unit;

    // --- [SHOW]
    public IO<Unit> Show(Seq<ControlRow> rows) =>
        from prior in seating.ValueIO
        from seated in rows.Map(static (row, at) => (At: at, Row: row)).Filter(static entry => entry.Row.Visible)
            .TraverseM(entry => Restated(prior.Seats.Find(entry.At), entry.Row).Map(held => (entry.At, held.Seat, held.Placed, held.Retired))).As()
        from laid in Arranged(prior.Laid, seated.Map(static entry => entry.Placed), prior.Laid.Map(static arrangement => arrangement.Beside).IfNone(noneValue: true))
        from stored in atomicIO(() => seating.Value = new Seating(prior.Seats.Filter((at, _) => at < rows.Count).AddOrUpdateRange(seated.Map(static entry => (entry.At, entry.Seat))), Some(laid)))
        from released in (seated.Choose(static entry => entry.Retired) + toSeq(prior.Seats.Filter((at, _) => at >= rows.Count).Values)).TraverseM(static seat => seat.Released).As()
        select unit;

    private IO<(Seat Seat, Placed Placed, Option<Seat> Retired)> Restated(Option<Seat> held, ControlRow row) =>
        from kept in held.Match(Some: seat => seat.Restated(row).Under(seat.Scope).Map(same => same ? Some(seat) : None), None: static () => IO.pure(Option<Seat>.None))
        from seat in kept.Match(Some: static seat => IO.pure(seat), None: () => Seated(row))
        from faced in IO.lift(() => Faced(seat, row))
        select (seat, new Placed(row.Shape, seat.Caption.Map(static caption => caption.Control), seat.Value), held.Filter(_ => kept.IsNone));

    private IO<Seat> Seated(ControlRow row) =>
        from lease in IO.lift(static () => EnvIO.New(token: CancellationToken.None))
        from seat in (
                from made in row.Seated(scope)
                from value in made.Cells.Traverse(cells => Valued(cells, row)).As()
                from caption in Captioned(row, value.IsNone)
                select new Seat(caption, value, made.Restated, lease))
            .Under(lease)
            .Catch(error => IO.lift(lease.Dispose) >> IO.fail<Seat>(error))
            .As()
        select seat;

    private IO<Option<Caption>> Captioned(ControlRow row, bool heading) =>
        heading
            ? use(static () => new LabelSeparator()).Map(rule => Some(new Caption(rule, next => rule.Text = Localized(next.Label))))
            : row.Shape.Labeled
                ? use(static () => new Label { VerticalAlignment = VerticalAlignment.Center }).Map(label => Some(new Caption(label, next =>
                    (label.Text, label.Font, label.Enabled) = (Localized(next.Label), next.Modified ? SystemFonts.Bold(SystemFonts.Label().Size) : SystemFonts.Label(), next.Enabled))))
                : IO.pure(Option<Caption>.None);

    private void Faced(Seat seat, ControlRow row) {
        _ = seat.Caption.Iter(caption => caption.Faced(row));
        _ = seat.Value.Iter(value => (value.Enabled, value.ToolTip) = (row.Enabled, row.Help.Map(Localized).ValueUnsafe()));
    }

    private string Localized(string text) => RowText.Localize(text, scope.Owner).Local;

    private IO<Unit> Released =>
        from prior in seating.ValueIO
        from cleared in prior.Laid.Traverse(Cleared).As()
        from reset in atomicIO(() => seating.Value = Seating.Empty)
        from released in toSeq(prior.Seats.Values).TraverseM(static seat => seat.Released).As()
        from disposed in IO.lift(view.Dispose)
        select unit;

    // --- [ARRANGE]
    private IO<Unit> Fitted =>
        from prior in seating.ValueIO
        from laid in prior.Laid.Traverse(held => IO.lift(() => view.Width >= Needed(held.Rows)).Bind(beside => Arranged(prior.Laid, held.Rows, beside))).As()
        from stored in atomicIO(() => seating.Value = prior with { Laid = laid })
        select unit;

    private IO<Arrangement> Arranged(Option<Arrangement> current, Seq<Placed> rows, bool beside) =>
        current.Filter(held => held.Rows == rows && held.Beside == beside).Match(Some: static held => IO.pure(held), None: () => Relaid(current, rows, beside));

    private IO<Arrangement> Relaid(Option<Arrangement> prior, Seq<Placed> rows, bool beside) =>
        from cleared in prior.Traverse(Cleared).As()
        from arrangement in IO.lift(() => Table(rows, beside))
        from shown in IO.lift(() => { view.Content = arrangement.Table; })
        select arrangement;

    private IO<Unit> Cleared(Arrangement held) =>
        IO.lift(() => {
            view.Content = null;
            held.Table.RemoveAll();
            _ = held.Runs.Iter(static run => {
                run.RemoveAll();
                run.Dispose();
            });
            held.Table.Dispose();
        });

    private static Arrangement Table(Seq<Placed> rows, bool beside) {
        Size spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table);
        Seq<(Seq<TableRow> Rows, Option<TableLayout> Run)> segments = Runs(rows).Map(run => beside && !run.Exists(static row => row.Shape.Spans)
            ? Paired(run, spacing) switch { var paired => (Seq(new TableRow(paired)), Some(paired)) }
            : (run.Bind(Stacked), Option<TableLayout>.None));
        Seq<TableRow> filler = rows.Exists(static row => row.Shape.Fills) ? [] : [new TableRow { ScaleHeight = true }];
        return new Arrangement(rows, beside, new TableLayout(segments.Bind(static segment => segment.Rows) + filler) { Padding = 0, Spacing = spacing }, segments.Choose(static segment => segment.Run));
    }

    private static Seq<Seq<Placed>> Runs(Seq<Placed> rows) =>
        rows.Fold(Seq<Seq<Placed>>(), static (runs, row) => runs.Last.Filter(run => !row.Shape.Spans && !run.Exists(static prior => prior.Shape.Spans)).Match(
            Some: run => runs.Init.Add(run.Add(row)),
            None: () => runs.Add(Seq(row))));

    private static TableLayout Paired(Seq<Placed> run, Size spacing) =>
        new(run.Map(static row => new TableRow(
            row.Caption.Match(Some: static caption => new TableCell(caption), None: static () => new TableCell()),
            row.Value.Match(Some: static value => new TableCell(value, scaleWidth: true), None: static () => new TableCell { ScaleWidth = true })))) { Padding = 0, Spacing = spacing };

    private static Seq<TableRow> Stacked(Placed row) =>
        row.Caption.Map(static caption => new TableRow(caption)).ToSeq() + row.Value.Map(value => new TableRow(value) { ScaleHeight = row.Shape.Fills }).ToSeq();

    private static int Needed(Seq<Placed> rows) =>
        rows.Filter(static row => !row.Shape.Spans) switch {
            var inline => Widest(inline.Choose(static row => row.Caption)) + RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Width + Widest(inline.Choose(static row => row.Value)),
        };

    private static int Widest(Seq<Control> controls) => controls.Fold(0, static (widest, control) => int.Max(widest, (int)MathF.Ceiling(control.GetPreferredSize().Width)));

    // --- [BODY]
    private IO<Control> Valued(RowCells cells, ControlRow row) =>
        from body in Body(cells, row)
        from menu in cells.Menu.IsEmpty
            ? IO.pure(Option<ContextMenu>.None)
            : CommandRows.Menu(cells.Menu, scope).Map(static held => Some(held.Menu))
        from attached in IO.lift(() => menu.Iter(held => body.ContextMenu = held))
        select body;

    private IO<Control> Body(RowCells cells, ControlRow row) =>
        from main in Lined(Seq(new TableCell(cells.Value, scaleWidth: true)) + cells.Pick.Map(static pick => new TableCell(pick)).ToSeq())
        from lines in cells.Lines.TraverseM(Line).As()
        from stack in Containers.Frame(static () => new StackLayout {
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        })
        from filled in IO.lift(() => main.Cons(lines).Iter(line => stack.Items.Add(new StackLayoutItem(line, expand: row.Shape.Fills))))
        select (Control)stack;

    private IO<Control> Line(RowLine line) =>
        from caption in line.Caption.Traverse(text => use(() => new Label { Text = Localized(text), VerticalAlignment = VerticalAlignment.Center })).As()
        from lined in Lined(caption.Map(static label => new TableCell(label)).ToSeq().Add(new TableCell(line.Value, scaleWidth: true)))
        select lined;

    internal static IO<Control> Lined(Seq<TableCell> cells) =>
        Containers.Frame(() => new TableLayout(new TableRow(cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }).Map(static table => (Control)table);

    // --- [ROWS]
    private sealed record Placed(RowShape Shape, Option<Control> Caption, Option<Control> Value);

    private sealed record Arrangement(Seq<Placed> Rows, bool Beside, TableLayout Table, Seq<TableLayout> Runs);

    private sealed record Seating(HashMap<int, Seat> Seats, Option<Arrangement> Laid) {
        public static Seating Empty { get; } = new([], None);
    }

    private sealed record Caption(Control Control, Action<ControlRow> Faced);

    private sealed record Seat(Option<Caption> Caption, Option<Control> Value, Func<ControlRow, IO<bool>> Restated, EnvIO Scope) {
        public IO<Unit> Released => IO.lift(Scope.Dispose);
    }
}
