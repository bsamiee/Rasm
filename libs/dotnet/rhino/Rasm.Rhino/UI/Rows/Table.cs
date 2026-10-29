using Eto.Drawing;
using Eto.Forms;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Rows;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowTable {
    // --- [STATE]
    private readonly Ref<(Seq<Seat> Seats, Option<Arrangement> Laid)> seating = Ref((Seats: Seq<Seat>(), Laid: Option<Arrangement>.None));
    private readonly RowScope scope;
    private readonly Panel view;

    private RowTable(RowScope scope, Panel view) => (this.scope, this.view) = (scope, view);

    public Control View => view;

    // --- [REALIZE]
    public static IO<RowTable> Realize(ControlRows rows, RowScope scope) =>
        from made in Made(scope)
        from post in Mailboxes.Idle<Unit, Unit>(_ => rows.Read(scope).Bind(made.Show))
        from signals in rows.Signals(scope)
        from held in signals.TraverseM(signal => signal.Attach(raised => post(raised, raised))).As()
        from heard in scope.Listen(_ => post(unit, unit))
        from shown in rows.Read(scope).Bind(made.Show)
        select made;

    internal static IO<RowTable> Made(RowScope scope) =>
        from made in use(() => new RowTable(scope, new Panel()), static table => table.Released)
        from sized in Subscriptions.On<EventArgs>(h => made.view.SizeChanged += h, h => made.view.SizeChanged -= h).Attach(_ => made.Fitted)
        select made;

    // --- [SHOW]
    public IO<Unit> Show(Seq<Row> rows) =>
        from shown in rows.TraverseM(row => Holds(row.Visible).Map(visible => (Row: row, Visible: visible))).As()
        let visible = Identified(shown.Filter(static entry => entry.Visible).Map(static entry => entry.Row))
        from prior in Taken
        from seated in visible.TraverseM(entry => Restated(prior.Seats.Find(seat => seat.Id == entry.Id), entry.Id, entry.Row)).As()
        from laid in Arranged(prior.Laid, seated.Map(static seat => seat.Placed), prior.Laid.Map(static held => held.Beside).IfNone(noneValue: true))
        from stored in atomicIO(() => seating.Value = (Seats: seated.Map(static seat => seat.Seat), Laid: Some(laid)))
        from released in prior.Seats.Filter(seat => !seated.Exists(kept => ReferenceEquals(kept.Seat, seat))).TraverseM(static seat => seat.Released).As()
        select unit;

    private static Seq<((Type Kind, string Label, int Ordinal) Id, Row Row)> Identified(Seq<Row> rows) =>
        rows.Fold((Seen: HashMap<(Type Kind, string Label), int>(), Rows: Seq<((Type Kind, string Label, int Ordinal) Id, Row Row)>()), static (state, row) =>
            ((Kind: row.Kind.GetType(), row.Label), state.Seen.Find((row.Kind.GetType(), row.Label)).IfNone(0)) switch {
                var (key, ordinal) => (state.Seen.AddOrUpdate(key, ordinal + 1), state.Rows.Add((Id: (key.Kind, key.Label, ordinal), Row: row))),
            }).Rows;

    private IO<(Seq<Seat> Seats, Option<Arrangement> Laid)> Taken =>
        atomicIO(() => {
            (Seq<Seat> Seats, Option<Arrangement> Laid) held = seating.Value;
            seating.Value = (Seats: Seq<Seat>(), Laid: None);
            return held;
        });

    private IO<(Seat Seat, (RowShape Shape, Option<Label> Caption, Control Value) Placed)> Restated(Option<Seat> held, (Type Kind, string Label, int Ordinal) id, Row row) =>
        from kept in held.Match(Some: seat => seat.Restated(row).Scoped(seat.Scope).Map(same => Conversions.Present(same, seat)), None: static () => IO.pure(Option<Seat>.None))
        from seat in kept.Match(Some: static seat => IO.pure(seat), None: () => Seated(id, row))
        from faced in Faced(seat, row)
        select (seat, (row.Shape, seat.Caption, seat.Value));

    private IO<Seat> Seated((Type Kind, string Label, int Ordinal) id, Row row) =>
        Products.Transfer(static () => EnvIO.New(token: CancellationToken.None), owned =>
            (from made in row.Seated(LocalText.Of(row.Label), scope)
             from value in Valued(made.Cells, row)
             from caption in row.Shape.Labeled ? use(static () => new Label()).Map(Some) : IO.pure(Option<Label>.None)
             select new Seat(id, caption, value, made.Restated, owned))
            .Scoped(owned));

    private IO<Unit> Faced(Seat seat, Row row) =>
        from enabled in Holds(row.Enabled)
        from faced in IO.lift(() => {
            _ = seat.Caption.Iter(label => (label.Text, label.Font, label.Enabled) = (LocalText.Of(row.Label).Local, row.Modified ? SystemFonts.Bold(SystemFonts.Label().Size) : SystemFonts.Label(), enabled));
            (seat.Value.Enabled, seat.Value.ToolTip) = (enabled, row.Help.Map(static help => LocalText.Of(help).Local).ValueUnsafe());
        })
        select unit;

    private IO<bool> Holds(Option<RowRule> rule) => rule.Match(Some: held => held.Holds(scope), None: static () => IO.pure(value: true));

    private IO<Unit> Released =>
        from prior in Taken
        from cleared in prior.Laid.Traverse(Cleared).As()
        from released in prior.Seats.TraverseM(static seat => seat.Released).As()
        from disposed in IO.lift(view.Dispose)
        select unit;

    // --- [ARRANGE]
    private IO<Unit> Fitted =>
        from prior in atomicIO(() => {
            (Seq<Seat> Seats, Option<Arrangement> Laid) held = seating.Value;
            seating.Value = held with { Laid = None };
            return held.Laid;
        })
        from laid in prior.Traverse(held => IO.lift(() => view.Width >= Needed(held.Rows)).Bind(beside => Arranged(prior, held.Rows, beside))).As()
        from stored in atomicIO(() => seating.Value = seating.Value with { Laid = laid })
        select unit;

    private IO<Arrangement> Arranged(Option<Arrangement> current, Seq<(RowShape Shape, Option<Label> Caption, Control Value)> rows, bool beside) =>
        current.Filter(held => held.Rows == rows && held.Beside == beside).Match(Some: static held => IO.pure(held), None: () => Relaid(current, rows, beside));

    private IO<Arrangement> Relaid(Option<Arrangement> prior, Seq<(RowShape Shape, Option<Label> Caption, Control Value)> rows, bool beside) =>
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

    private static Arrangement Table(Seq<(RowShape Shape, Option<Label> Caption, Control Value)> rows, bool beside) {
        Size spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table);
        Seq<(Seq<TableRow> Rows, Option<TableLayout> Run)> segments = Runs(rows).Map(run => beside && !run.Exists(static row => row.Shape.Spans)
            ? Paired(run, spacing) switch { var paired => (Seq(new TableRow(paired)), Some(paired)) }
            : (run.Bind(Stacked), Option<TableLayout>.None));
        Seq<TableRow> filler = rows.Exists(static row => row.Shape.Fills) ? [] : [new TableRow { ScaleHeight = true }];
        return new Arrangement(rows, beside, new TableLayout(segments.Bind(static segment => segment.Rows) + filler) { Padding = 0, Spacing = spacing }, segments.Choose(static segment => segment.Run));
    }

    private static Seq<Seq<(RowShape Shape, Option<Label> Caption, Control Value)>> Runs(Seq<(RowShape Shape, Option<Label> Caption, Control Value)> rows) =>
        rows.Fold(Seq<Seq<(RowShape Shape, Option<Label> Caption, Control Value)>>(), static (runs, row) => runs.Last.Filter(run => !row.Shape.Spans && !run.Exists(static prior => prior.Shape.Spans)).Match(
            Some: run => runs.Init.Add(run.Add(row)),
            None: () => runs.Add(Seq(row))));

    private static TableLayout Paired(Seq<(RowShape Shape, Option<Label> Caption, Control Value)> run, Size spacing) =>
        new(run.Map(static row => row.Caption.Match(
            Some: caption => RhinoLayout.LabelRow(caption, row.Value, stretch: true),
            None: () => new TableRow(new TableCell(), new TableCell(row.Value, scaleWidth: true))))) { Padding = 0, Spacing = spacing };

    private static Seq<TableRow> Stacked((RowShape Shape, Option<Label> Caption, Control Value) row) =>
        row.Caption.Map(static caption => {
            caption.TextAlignment = TextAlignment.Left;
            return new TableRow(caption);
        }).ToSeq().Add(new TableRow(row.Value) { ScaleHeight = row.Shape.Fills });

    private static int Needed(Seq<(RowShape Shape, Option<Label> Caption, Control Value)> rows) =>
        rows.Filter(static row => !row.Shape.Spans) switch {
            var inline => Widest(inline.Choose(static row => row.Caption.Map(static caption => (Control)caption))) + RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Width + Widest(inline.Map(static row => row.Value)),
        };

    private static int Widest(Seq<Control> controls) => controls.Fold(0, static (widest, control) => int.Max(widest, (int)MathF.Ceiling(control.GetPreferredSize().Width)));

    // --- [BODY]
    private IO<Control> Valued(RowCells cells, Row row) =>
        from body in Body(cells, row)
        from menu in cells.Menu.IsEmpty
            ? IO.pure(Option<ContextMenu>.None)
            : CommandRows.Menu(cells.Menu, scope).Map(static held => Some(held.Menu))
        from attached in IO.lift(() => menu.Iter(held => body.ContextMenu = held))
        select body;

    private static IO<Control> Body(RowCells cells, Row row) =>
        from main in Lined(Seq(new TableCell(cells.Value, scaleWidth: true)) + cells.Pick.Map(static pick => new TableCell(pick)).ToSeq())
        from lines in cells.Lines.TraverseM(Line).As()
        from stack in Containers.Frame(static () => new StackLayout {
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        })
        from filled in IO.lift(() => main.Cons(lines).Iter(line => stack.Items.Add(new StackLayoutItem(line, expand: row.Shape.Fills))))
        select (Control)stack;

    private static IO<Control> Line(RowLine line) =>
        line.Caption.Match(
            Some: text => Containers.Frame(() => RhinoLayout.LabelTableLayout(RhinoLayout.NewLabel(LocalText.Of(text).Local), line.Value, stretch: true, RhinoLayout.SpacingType.Table)).Map(static table => (Control)table),
            None: () => IO.pure(line.Value));

    internal static IO<Control> Lined(Seq<TableCell> cells) =>
        Containers.Frame(() => new TableLayout(new TableRow(cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }).Map(static table => (Control)table);

    // --- [ROWS]
    private sealed record Arrangement(Seq<(RowShape Shape, Option<Label> Caption, Control Value)> Rows, bool Beside, TableLayout Table, Seq<TableLayout> Runs);

    private sealed record Seat((Type Kind, string Label, int Ordinal) Id, Option<Label> Caption, Control Value, Func<Row, IO<bool>> Restated, EnvIO Scope) {
        public IO<Unit> Released => IO.lift(Scope.Dispose);
    }
}
