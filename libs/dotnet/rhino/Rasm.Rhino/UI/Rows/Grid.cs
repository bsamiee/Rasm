using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Rows;

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

    internal abstract IO<(Option<RowCells> Cells, Func<ControlRow, IO<bool>> Restated)> Seated(RowScope scope, Action<IDisposable> retain);

    public sealed record Group(string Label) : ControlRow(Label, RowShape.Nested) {
        internal override IO<(Option<RowCells> Cells, Func<ControlRow, IO<bool>> Restated)> Seated(RowScope scope, Action<IDisposable> retain) =>
            IO.pure<(Option<RowCells>, Func<ControlRow, IO<bool>>)>((None, static next => IO.pure(next is Group)));
    }

    public sealed record Bound<TRow>(string Label, RowShape Shape, TRow Row, Func<IO<Bound<TRow>>, RowScope, Action<IDisposable>, IO<RowCells>> Realize)
        : ControlRow(Label, Shape) where TRow : notnull {
        internal override IO<(Option<RowCells> Cells, Func<ControlRow, IO<bool>> Restated)> Seated(RowScope scope, Action<IDisposable> retain) =>
            from latest in atomIO(this)
            from cells in Realize(latest.ValueIO, scope, retain)
            from shown in cells.Show
            select (Some(cells), (Func<ControlRow, IO<bool>>)(next => next is Bound<TRow> held && held.Realize.Method == Realize.Method
                ? writeIO(latest, held).Bind(_ => cells.Show).Map(static _ => true)
                : IO.pure(value: false)));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowGrid {
    // --- [STATE]
    private readonly RowScope scope;
    private readonly Retained<IDisposable> owned;
    private readonly Panel view;
    private Seating seating = Seating.Empty;

    private RowGrid(RowScope scope, Retained<IDisposable> owned, Panel view) => (this.scope, this.owned, this.view) = (scope, owned, view);

    public Control View => view;

    // --- [REALIZE]
    public static IO<RowGrid> Realize(ControlRows rows, RowScope scope, Action<IDisposable> retain) =>
        from owned in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from grid in DisposalOps.OnFailure(
            from view in Containers.Frame(static () => new Panel(), owned.Retain)
            let made = new RowGrid(scope, owned, view)
            from released in DisposalOps.Acquire(IO.pure(unit), _ => made.Emptied, owned.Retain)
            from sized in Subscriptions.Host<EventArgs>(typeof(Control), h => view.SizeChanged += h, h => view.SizeChanged -= h, nameof(Control.SizeChanged))
                .Inline(_ => made.Fitted, scope.Sink, owned.Retain)
            from attached in made.Attached(rows)
            select made,
            IO.lift(owned.Dispose))
        select grid;

    private IO<Unit> Attached(ControlRows rows) =>
        from site in IO.pure(CallbackSite.Of(scope.Sink, this))
        let read = rows.Read(scope).Bind(Show)
        from mailbox in Subscriptions.Idle<Unit, Unit>(_ => read)(site, owned.Retain)
        let post = fun((Unit raised) => mailbox((raised, raised)))
        from signals in rows.Signals(scope)
        from held in signals.TraverseM(signal => signal.Inline(post, scope.Sink, owned.Retain)).As()
        from heard in scope.Listen(_ => post(unit), owned.Retain)
        from shown in read
        select unit;

    // --- [SHOW]
    public IO<Unit> Show(Seq<ControlRow> rows) =>
        from prior in IO.lift(() => seating)
        from seated in rows.Map(static (row, at) => (At: at, Row: row)).Filter(static entry => entry.Row.Visible)
            .TraverseM(entry => Restated(prior.Seats.Find(entry.At), entry.Row).Map(held => (entry.At, held.Seat, held.Placed, held.Retired))).As()
        from laid in Arranged(prior.Laid, seated.Map(static entry => entry.Placed), prior.Laid.Map(static arrangement => arrangement.Beside).IfNone(noneValue: true))
        from stored in IO.lift(() => { seating = new Seating(prior.Seats.Filter((at, _) => at < rows.Count).AddOrUpdateRange(seated.Map(static entry => (entry.At, entry.Seat))), Some(laid)); })
        from released in (seated.Choose(static entry => entry.Retired) + toSeq(prior.Seats.Filter((at, _) => at >= rows.Count).Values)).TraverseM(static seat => seat.Lease.Close).As()
        select unit;

    private IO<(Seat Seat, Placed Placed, Option<Seat> Retired)> Restated(Option<Seat> held, ControlRow row) =>
        from kept in held.Match(Some: seat => seat.Restated(row).Map(same => same ? Some(seat) : None), None: static () => IO.pure(Option<Seat>.None))
        from seat in kept.Match(Some: static seat => IO.pure(seat), None: () => Seated(row))
        from faced in IO.lift(() => Faced(seat, row))
        select (seat, new Placed(row.Shape, seat.Caption.Map(static caption => caption.Control), seat.Value), held.Filter(_ => kept.IsNone));

    private IO<Seat> Seated(ControlRow row) =>
        from lease in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from seat in DisposalOps.OnFailure(
            from made in row.Seated(scope, lease.Retain)
            from value in made.Cells.Traverse(cells => Valued(cells, row, lease.Retain)).As()
            from caption in Captioned(row, value.IsNone, lease.Retain)
            select new Seat(caption, value, made.Restated, lease),
            lease.Close)
        select seat;

    private IO<Option<Caption>> Captioned(ControlRow row, bool heading, Action<IDisposable> retain) =>
        heading
            ? DisposalOps.Acquire(IO.lift(static () => new LabelSeparator()), retain).Map(rule => Some(new Caption(rule, next => rule.Text = Localized(next.Label))))
            : row.Shape.Labeled
                ? DisposalOps.Acquire(IO.lift(static () => new Label { VerticalAlignment = VerticalAlignment.Center }), retain).Map(label => Some(new Caption(label, next =>
                    (label.Text, label.Font, label.Enabled) = (Localized(next.Label), next.Modified ? SystemFonts.Bold(SystemFonts.Label().Size) : SystemFonts.Label(), next.Enabled))))
                : IO.pure(Option<Caption>.None);

    private void Faced(Seat seat, ControlRow row) {
        _ = seat.Caption.Iter(caption => caption.Faced(row));
        _ = seat.Value.Iter(value => (value.Enabled, value.ToolTip) = (row.Enabled, row.Help.Map(Localized).ValueUnsafe()));
    }

    private string Localized(string text) => RowText.Localize(text, scope.Sink).Local;

    private IO<Unit> Emptied =>
        from prior in IO.lift(() => seating)
        from cleared in prior.Laid.Traverse(Cleared).As()
        from reset in IO.lift(() => { seating = Seating.Empty; })
        from released in toSeq(prior.Seats.Values).TraverseM(static seat => seat.Lease.Close).As()
        select unit;

    // --- [ARRANGE]
    private IO<Unit> Fitted =>
        from prior in IO.lift(() => seating)
        from laid in prior.Laid.Traverse(held => IO.lift(() => view.Width >= Needed(held.Rows)).Bind(beside => Arranged(prior.Laid, held.Rows, beside))).As()
        from stored in IO.lift(() => { seating = prior with { Laid = laid }; })
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
    private IO<Control> Valued(RowCells cells, ControlRow row, Action<IDisposable> retain) =>
        from body in Body(cells, row, retain)
        from menu in cells.Menu.IsEmpty
            ? IO.pure(Option<ContextMenu>.None)
            : CommandRows.Menu(cells.Menu, scope, retain).Map(static held => Some(held.Menu))
        from attached in IO.lift(() => menu.Iter(held => body.ContextMenu = held))
        select body;

    private IO<Control> Body(RowCells cells, ControlRow row, Action<IDisposable> retain) =>
        from main in Lined(Seq(new TableCell(cells.Value, scaleWidth: true)) + cells.Pick.Map(static pick => new TableCell(pick)).ToSeq(), retain)
        from lines in cells.Lines.TraverseM(line => Line(line, retain)).As()
        from stack in Containers.Frame(static () => new StackLayout {
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        }, retain)
        from filled in IO.lift(() => main.Cons(lines).Iter(line => stack.Items.Add(new StackLayoutItem(line, expand: row.Shape.Fills))))
        select (Control)stack;

    private IO<Control> Line(RowLine line, Action<IDisposable> retain) =>
        from caption in line.Caption.Traverse(text => DisposalOps.Acquire(IO.lift(() => new Label { Text = Localized(text), VerticalAlignment = VerticalAlignment.Center }), retain)).As()
        from lined in Lined(caption.Map(static label => new TableCell(label)).ToSeq().Add(new TableCell(line.Value, scaleWidth: true)), retain)
        select lined;

    internal static IO<Control> Lined(Seq<TableCell> cells, Action<IDisposable> retain) =>
        Containers.Frame(() => new TableLayout(new TableRow(cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }, retain).Map(static table => (Control)table);

    // --- [ROWS]
    private sealed record Placed(RowShape Shape, Option<Control> Caption, Option<Control> Value);

    private sealed record Arrangement(Seq<Placed> Rows, bool Beside, TableLayout Table, Seq<TableLayout> Runs);

    private sealed record Seating(HashMap<int, Seat> Seats, Option<Arrangement> Laid) {
        public static Seating Empty { get; } = new([], None);
    }

    private sealed record Caption(Control Control, Action<ControlRow> Faced);

    private sealed record Seat(Option<Caption> Caption, Option<Control> Value, Func<ControlRow, IO<bool>> Restated, Retained<IDisposable> Lease);
}
