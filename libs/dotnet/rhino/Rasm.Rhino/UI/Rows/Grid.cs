using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
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

    internal abstract Option<IO<(RowCells Cells, Func<ControlRow, IO<bool>> Restated)>> Seated(RowScope scope, Action<IDisposable> retain);

    public sealed record Group(string Label) : ControlRow(Label, RowShape.Nested) {
        internal override Option<IO<(RowCells Cells, Func<ControlRow, IO<bool>> Restated)>> Seated(RowScope scope, Action<IDisposable> retain) => None;
    }

    public sealed record Bound<TRow>(string Label, RowShape Shape, TRow Row, Func<IO<Bound<TRow>>, RowScope, Action<IDisposable>, IO<RowCells>> Realize)
        : ControlRow(Label, Shape) where TRow : notnull {
        internal override Option<IO<(RowCells Cells, Func<ControlRow, IO<bool>> Restated)>> Seated(RowScope scope, Action<IDisposable> retain) =>
            Some(from latest in atomIO(this)
                 from cells in Realize(latest.ValueIO, scope, retain)
                 from shown in cells.Show
                 select (cells, (Func<ControlRow, IO<bool>>)(next => next is Bound<TRow> held && held.Realize.Method == Realize.Method
                     ? writeIO(latest, held).Bind(_ => cells.Show).Map(static _ => true)
                     : IO.pure(value: false))));
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowGrid {
    // --- [STATE]
    private readonly RowScope scope;
    private readonly Retained<IDisposable> owned;
    private readonly Atom<HashMap<int, Seat>> seats = Atom(HashMap<int, Seat>());
    private readonly Atom<int> serials = Atom(0);

    private RowGrid(RowScope scope, Retained<IDisposable> owned) {
        (this.scope, this.owned) = (scope, owned);
        View = new SceneView(scope, new Node.Box()) { Slots = Slotted };
    }

    public SceneView View { get; }

    // --- [REALIZE]
    public static IO<RowGrid> Realize(ControlRows rows, RowScope scope, Action<IDisposable> retain) =>
        from owned in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from grid in DisposalOps.OnFailure(
            from made in IO.lift(() => new RowGrid(scope, owned))
            from view in DisposalOps.Acquire(IO.pure(made.View), owned.Retain)
            from released in DisposalOps.Acquire(IO.pure(unit), _ => made.Emptied, owned.Retain)
            from attached in made.Attached(rows)
            select made,
            IO.lift(owned.Dispose))
        select grid;

    private IO<Unit> Attached(ControlRows rows) =>
        from site in IO.pure(new CallbackSite(scope.Sink, typeof(RowGrid), nameof(Attached)))
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
        from seated in rows.Map(static (row, at) => new Place(at, row)).Filter(static place => place.Row.Visible).TraverseM(place => Restated(place.At, place.Row)).As()
        from current in seats.ValueIO
        from shown in View.Show(Drawn(rows, current))
        select unit;

    private IO<Unit> Restated(int at, ControlRow row) =>
        from held in seats.ValueIO.Map(current => current.Find(at))
        from kept in held.Match(Some: seat => seat.Restated(row), None: static () => IO.pure(value: false))
        from placed in kept ? IO.pure(unit) : Replaced(at, held, row)
        from applied in seats.ValueIO.Bind(current => current.Find(at).Traverse(seat => IO.lift(() => { seat.Slot.Enabled = row.Enabled; })).As())
        select unit;

    private IO<Unit> Replaced(int at, Option<Seat> prior, ControlRow row) =>
        from released in seats.SwapIO(current => current.Remove(at))
        from disposed in IO.lift(() => prior.Filter(static seat => !seat.Handed).Iter(static seat => seat.Slot.Dispose()))
        from seated in row.Seated(scope, owned.Retain).Traverse(made => Seated(at, row, made)).As()
        select unit;

    private IO<Unit> Seated(int at, ControlRow row, IO<(RowCells Cells, Func<ControlRow, IO<bool>> Restated)> made) =>
        from lease in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from serial in serials.SwapIO(static held => held + 1)
        from slot in DisposalOps.OnFailure(
            from realized in made
            from body in Body(realized.Cells, row, lease.Retain)
            from menu in realized.Cells.Menu.IsEmpty
                ? IO.pure(Option<ContextMenu>.None)
                : CommandRows.Menu(realized.Cells.Menu, scope, lease.Retain).Map(static held => Some(held.Menu))
            from placed in IO.lift(() => new RowSlot(body, lease, Released(at)) { ToolTip = row.Help.Map(help => RowText.Localize(help, scope.Sink).Local).ValueUnsafe(), ContextMenu = menu.ValueUnsafe() })
            from stored in seats.SwapIO(held => held.AddOrUpdate(at, new Seat(serial, placed, (float)placed.GetPreferredSize().Height, Handed: false, realized.Restated)))
            select placed,
            IO.lift(lease.Dispose))
        select unit;

    private Action<RowSlot> Released(int at) =>
        slot => seats.Swap(held => held.Find(at).Exists(seat => ReferenceEquals(seat.Slot, slot)) ? held.Remove(at) : held);

    private IO<Control> Slotted(Seq<Key> path) =>
        from seat in seats.ValueIO.Map(held => toSeq(held.AsIterable()).Find(entry => path.Last == Some<Key>(Slot(entry.Value.Serial))))
        from found in IO.lift(() => seat.ToFin(new Refused(nameof(SceneView.Slots), Refused.Absent)))
        from handed in seats.SwapIO(held => held.AddOrUpdate(found.Key, found.Value with { Handed = true }))
        select (Control)found.Value.Slot;

    private IO<Unit> Emptied =>
        from held in seats.ValueIO
        from cleared in seats.SwapIO(static _ => HashMap<int, Seat>())
        from disposed in IO.lift(() => toSeq(held.Values).Filter(static seat => !seat.Handed).Iter(static seat => seat.Slot.Dispose()))
        select unit;

    // --- [BODY]
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
        from caption in line.Caption.Traverse(text => DisposalOps.Acquire(
            IO.lift(() => new Label { Text = RowText.Localize(text, scope.Sink).Local, VerticalAlignment = VerticalAlignment.Center }), retain)).As()
        from lined in Lined(caption.Map(static label => new TableCell(label)).ToSeq().Add(new TableCell(line.Value, scaleWidth: true)), retain)
        select lined;

    internal static IO<Control> Lined(Seq<TableCell> cells, Action<IDisposable> retain) =>
        Containers.Frame(() => new TableLayout(new TableRow(cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }, retain).Map(static table => (Control)table);

    // --- [SCENE]
    private Node.Flex Drawn(Seq<ControlRow> rows, HashMap<int, Seat> seated) =>
        new(PhysicalAxis.Vertical, Segments(rows.Map(static (row, at) => new Place(at, row)).Filter(static place => place.Row.Visible), seated)) { Width = Sizing.Grow.Unit };

    private Seq<Node> Segments(Seq<Place> shown, HashMap<int, Seat> seated) =>
        shown.Fold(Seq<Seq<Place>>(), static (runs, place) => runs.Last.Filter(run => !place.Row.Shape.Spans && !run.Exists(static prior => prior.Row.Shape.Spans)).Match(
                Some: run => runs.Init.Add(run.Add(place)),
                None: () => runs.Add(Seq(place))))
            .Bind(run => run.Exists(static place => place.Row.Shape.Spans)
                ? run.Map(place => (Node)Spanning(place.Row, place.At, seated))
                : Seq(FieldRow.Drawn(run.Map(place => new FieldRow(Captioned(place.Row), Valued(place.Row, place.At, seated)) { Conditions = Conditioned(place.Row) }))));

    private Node.Flex Spanning(ControlRow row, int at, HashMap<int, Seat> seated) =>
        new(PhysicalAxis.Vertical,
            (row.Shape.Labeled ? Seq<Node>(new Node.Text([Captioned(row)]) { Role = Role.Label, Conditions = Conditioned(row) }) : [])
                .Add(Valued(row, at, seated) with { Accessible = new Accessible { Label = Captioned(row) } })) {
            Key = new Key.Named(string.Create(CultureInfo.InvariantCulture, $"row-{at}")),
            Width = Sizing.Grow.Unit,
            Conditions = Conditioned(row),
        };

    private string Captioned(ControlRow row) => row.Shape.Labeled ? RowText.Localize(row.Label, scope.Sink).Local : "";

    private Node Valued(ControlRow row, int at, HashMap<int, Seat> seated) =>
        seated.Find(at).Match(
            Some: seat => (Node)new Node.Slot {
                Key = Slot(seat.Serial),
                Width = Sizing.Grow.Unit,
                Height = new Sizing.Fixed(seat.Height),
                Accessible = new Accessible { Help = row.Help.Map(help => RowText.Localize(help, scope.Sink).Local) },
            },
            None: static () => new Node.Box());

    private static Key.Named Slot(int serial) => new(string.Create(CultureInfo.InvariantCulture, $"slot-{serial}"));

    private static LanguageExt.HashSet<Condition> Conditioned(ControlRow row) =>
        toHashSet(Seq((Held: !row.Enabled, Condition: Condition.Disabled), (Held: row.Modified, Condition: Condition.Modified)).Filter(static held => held.Held).Map(static held => held.Condition));

    // --- [ROWS]
    private sealed record Place(int At, ControlRow Row);

    private sealed record Seat(int Serial, RowSlot Slot, float Height, bool Handed, Func<ControlRow, IO<bool>> Restated);

    private sealed class RowSlot : Panel {
        private readonly Retained<IDisposable> lease;
        private readonly Action<RowSlot> released;

        internal RowSlot(Control body, Retained<IDisposable> lease, Action<RowSlot> released) {
            (this.lease, this.released) = (lease, released);
            Content = body;
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    Content = null;
                    released(this);
                    lease.Dispose();
                }
            } finally {
                base.Dispose(disposing);
            }
        }
    }
}
