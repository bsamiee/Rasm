using Eto.Drawing;
using Eto.Forms;
using Rhino.UI.Controls;
using SuperLinq;

namespace Rasm.Rhino.UI.Rows;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class RowTable {

    // --- [STATE]

    private readonly Ref<(Seq<RealizedRow> Realized, Option<Arrangement> Laid)> state = Ref((Realized: Seq<RealizedRow>(), Laid: Option<Arrangement>.None));
    private readonly RowScope scope;
    private readonly Panel view;

    private RowTable(RowScope scope, Panel view) => (this.scope, this.view) = (scope, view);

    public Control View => view;

    // --- [REALIZATION]

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
        from post in Mailboxes.Idle<Unit, Unit>(_ => made.Fitted)
        from sized in Subscriptions.On<EventArgs>(h => made.view.SizeChanged += h, h => made.view.SizeChanged -= h).Attach(_ => post(unit, unit))
        select made;

    // --- [DISPLAY]

    public IO<Unit> Show(Seq<Row> rows) =>
        (from shown in rows.TraverseM(row => Holds(row.Visible).Map(visible => (Row: row, Visible: visible))).As()
         let visible = Identified(shown.Filter(static entry => entry.Visible).Map(static entry => entry.Row))
         from prior in atomicIO(() => state.Value)
         let existing = toHashMap(prior.Realized.Map(static row => (row.Id, row)))
         from current in visible.TraverseM(entry => Restated(existing.Find(entry.Id), entry.Id, entry.Row)).As()
         from laid in Arranged(prior.Laid, current.Map(static held => held.Placed), prior.Laid.Map(static held => held.Beside).IfNone(noneValue: true))
         let realized = current.Map(static held => held.Row)
         let kept = toHashMap(realized.Map(static row => (row.Id, row)))
         from stored in atomicIO(() => state.Value = (Realized: realized, Laid: Some(laid)))
         select prior.Realized.Filter(row => !kept.Find(row.Id).Exists(current => ReferenceEquals(current, row))))
        .BracketFail().Bind(static retired => Products.Each(retired.Map(static row => row.Content.Release))).Map(unit);

    private static Seq<((Type Kind, string Label, int Ordinal) Id, Row Row)> Identified(Seq<Row> rows) =>
        rows.Fold((Seen: HashMap<(Type Kind, string Label), int>(), Rows: Seq<((Type Kind, string Label, int Ordinal) Id, Row Row)>()), static (state, row) =>
            ((Kind: row.Kind.GetType(), row.Label), state.Seen.Find((row.Kind.GetType(), row.Label)).IfNone(0)) switch {
                var (key, ordinal) => (state.Seen.AddOrUpdate(key, ordinal + 1), state.Rows.Add((Id: (key.Kind, key.Label, ordinal), Row: row))),
            }).Rows;

    private IO<(Seq<RealizedRow> Realized, Option<Arrangement> Laid)> Taken =>
        atomicIO(() => {
            (Seq<RealizedRow> Realized, Option<Arrangement> Laid) held = state.Value;
            state.Value = (Realized: Seq<RealizedRow>(), Laid: None);
            return held;
        });

    private IO<(RealizedRow Row, (RowShape Shape, Option<Label> Caption, Control Value) Placed)> Restated(Option<RealizedRow> held, (Type Kind, string Label, int Ordinal) id, Row row) =>
        from kept in held.Match(Some: existing => existing.Content.Value.Restate(row).Scoped(existing.Content.Scope).Map(same => Conversions.Present(same, existing)), None: static () => IO.pure(Option<RealizedRow>.None))
        from current in kept.Match(Some: static existing => IO.pure(existing), None: () => Realized(id, row))
        from faced in Faced(current, row)
        select (current, (row.Shape, current.Content.Value.Caption, current.Content.Value.Control));

    private IO<RealizedRow> Realized((Type Kind, string Label, int Ordinal) id, Row row) =>
        Products.Owned(
            from made in row.Realize(LocalText.Of(row.Label), scope)
            from value in Valued(made.Cells, row)
            from caption in row.Shape.Labeled ? use(static () => new Label()).Map(Some) : IO.pure(Option<Label>.None)
            select (Caption: caption, Control: value, Restate: made.Restated))
            .Map(content => new RealizedRow(id, content));

    private IO<Unit> Faced(RealizedRow current, Row row) =>
        from enabled in Holds(row.Enabled)
        from faced in IO.lift(() => {
            _ = current.Content.Value.Caption.Iter(label => (label.Text, label.Font, label.Enabled) = (LocalText.Of(row.Label).Local, row.Modified ? SystemFonts.Bold(SystemFonts.Label().Size) : SystemFonts.Label(), enabled));
            (current.Content.Value.Control.Enabled, current.Content.Value.Control.ToolTip) = (enabled, row.Help.Map(static help => LocalText.Of(help).Local).ValueUnsafe());
        })
        select unit;

    private IO<bool> Holds(Option<RowRule> rule) => rule.Match(Some: held => held.Holds(scope), None: static () => IO.pure(value: true));

    private IO<Unit> Released =>
        from prior in Taken
        from cleared in prior.Laid.Traverse(Cleared).As()
        from released in Products.Each(prior.Realized.Map(static row => row.Content.Release))
        from disposed in IO.lift(view.Dispose)
        select unit;

    // --- [ARRANGEMENT]

    private IO<Unit> Fitted =>
        from prior in atomicIO(() => state.Value.Laid)
        from laid in prior.Traverse(held => IO.lift(() => view.Width >= Needed(held.Rows)).Bind(beside => Arranged(prior, held.Rows, beside))).As()
        from stored in atomicIO(() => state.Value = state.Value with { Laid = laid })
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
        toSeq(rows.Segment(static (current, previous, _) => current.Shape.Spans || previous.Shape.Spans)).Map(static run => toSeq(run));

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

    // --- [BODIES]

    private IO<Control> Valued(RowCells cells, Row row) =>
        from body in Body(cells, row)
        from menu in cells.Menu.IsEmpty
            ? IO.pure(Option<ContextMenu>.None)
            : CommandRows.Menu(cells.Menu, scope).Map(static held => Some(held.Menu))
        from attached in IO.lift(() => menu.Iter(held => body.ContextMenu = held))
        select body;

    private static IO<Control> Body(RowCells cells, Row row) =>
        from main in cells.Pick.Match(Some: pick => Lined([new TableCell(cells.Value, scaleWidth: true), new TableCell(pick)]), None: () => IO.pure(cells.Value))
        from body in cells.Lines.IsEmpty
            ? IO.pure(main)
            : from lines in cells.Lines.TraverseM(Line).As()
              from stack in Containers.Frame(static () => new StackLayout {
                  Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Height,
                  HorizontalContentAlignment = HorizontalAlignment.Stretch,
              })
              from filled in IO.lift(() => main.Cons(lines).Iter(line => stack.Items.Add(new StackLayoutItem(line, expand: row.Shape.Fills))))
              select (Control)stack
        select body;

    private static IO<Control> Line(RowLine line) =>
        line.Caption.Match(
            Some: text => Containers.Frame(() => RhinoLayout.LabelTableLayout(RhinoLayout.NewLabel(LocalText.Of(text).Local), line.Value, stretch: true, RhinoLayout.SpacingType.Table)).Map(static table => (Control)table),
            None: () => IO.pure(line.Value));

    internal static IO<Control> Lined(Seq<TableCell> cells) =>
        Containers.Frame(() => new TableLayout(new TableRow(cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }).Map(static table => (Control)table);

    // --- [ROWS]

    private sealed record Arrangement(Seq<(RowShape Shape, Option<Label> Caption, Control Value)> Rows, bool Beside, TableLayout Table, Seq<TableLayout> Runs);

    private sealed record RealizedRow((Type Kind, string Label, int Ordinal) Id, Owned<(Option<Label> Caption, Control Control, Func<Row, IO<bool>> Restate)> Content);
}
