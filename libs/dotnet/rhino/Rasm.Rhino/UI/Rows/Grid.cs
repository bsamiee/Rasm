using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.Resources;
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

public sealed record FieldFit(Control Field, float Width);

public sealed record RowHelp(Option<(string Low, string High)> Range, Option<string> Default) {
    public string Text(string help, IPlugInSink sink) =>
        string.Join(Environment.NewLine, [
            RowText.Localize(help, sink).Local,
            .. Range.Map(static range => RowText.Localize("{0} – {1}", arguments: [range.Low, range.High]).Local).ToSeq(),
            .. Default.Map(static value => RowText.Localize("Default {0}", arguments: [value]).Local).ToSeq(),
        ]);
}

public sealed record RowLine(Option<string> Caption, Control Value, Option<Control> Pick, Option<FieldFit> Field);

public sealed record RowCells(
    Control Value, Option<Control> Gate, Option<Control> Pick, Option<FieldFit> Field, Seq<RowLine> Heads, Seq<RowLine> Lines, RowHelp Help, IO<Unit> Show,
    Option<RowEdit> Edit, Option<Func<Seq<Seq<Command<RowRule, Invoke>>>, IO<Unit>, RowScope, Action<IDisposable>, IO<ContextMenu>>> Menu) {
    public IO<bool> NestedEditing { get; init; } = IO.pure(value: false);
    public Option<Func<bool, IO<Unit>>> Availability { get; init; }

    internal Seq<FieldFit> Fits => Field.ToSeq() + (Heads + Lines).Choose(static line => line.Field);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowGrid {
    // --- [STATE]
    private readonly RowScope scope;
    private readonly Seq<Place> places;
    private readonly HashMap<EntryKey, Seq<Place>> index;
    private readonly HashMap<Key, Place> slotted;
    private readonly Atom<HashMap<int, Mark>> marks = Atom(HashMap<int, Mark>());
    private readonly Atom<HashMap<int, Seat>> seats = Atom(HashMap<int, Seat>());
    private readonly Atom<float> widest = Atom(0f);
    private readonly Retained<IDisposable> owned;

    private RowGrid(RowScope scope, Seq<ControlRow> rows, Retained<IDisposable> owned) {
        (this.scope, this.owned) = (scope, owned);
        places = rows.Map(static (row, at) => new Place(at, row));
        index = RuleIndex.Of(places.Map(static place => (place.Reads, place)));
        slotted = toHashMap(places.Map(static place => (place.Slot, place)));
        View = new SceneView(scope, new Node.Box()) { Slots = Slotted };
    }

    public SceneView View { get; }

    // --- [REALIZE]
    public static IO<RowGrid> Realize(Seq<ControlRow> rows, RowScope scope, Action<IDisposable> retain) =>
        from owned in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), retain)
        from grid in DisposalOps.OnFailure(
            from made in IO.lift(() => new RowGrid(scope, rows, owned))
            from view in DisposalOps.Acquire(IO.pure(made.View), owned.Retain)
            from released in DisposalOps.Acquire(IO.pure(unit), _ => made.Emptied, owned.Retain)
            from attached in made.Attached
            select made,
            IO.lift(owned.Dispose))
        select grid;

    private IO<Unit> Attached =>
        from listened in scope.Listen(Refresh, owned.Retain)
        from watched in places.Bind(static place => place.Sources).Distinct().TraverseM(Watched).As()
        from shown in Refreshed(places)
        select unit;

    private IO<Unit> Watched(RowSource source) =>
        from signals in source.Signals(scope)
        let site = new CallbackSite(scope.Sink, typeof(RowGrid), nameof(Watched))
        let reading = places.Filter(place => place.Sources.Exists(read => read == source))
        from mailbox in Subscriptions.Idle<Unit, Unit>(_ => Refreshed(reading))(site, owned.Retain)
        let post = fun((Unit raised) => mailbox((raised, raised)))
        from held in signals.TraverseM(signal => signal.Inline(post, scope.Sink, owned.Retain)).As()
        select unit;

    // --- [REFRESH]
    public IO<bool> Editing =>
        seats.ValueIO
            .Bind(static held => Callbacks.Each(toSeq(held.Values).Bind(static seat => seat.Cells.Edit.Map(static edit => edit.Editing).ToSeq().Add(seat.Cells.NestedEditing))))
            .Map(static editing => editing.Exists(static held => held));

    public IO<Unit> Refresh(Seq<EntryKey> changed) => Refreshed(RuleIndex.Affected(index, changed));

    private IO<Unit> Refreshed(Seq<Place> targets) =>
        from parts in targets.Map<K<IO, (Place Place, Mark Mark)>>(place => Restated(place).Map(mark => (place, mark))).PartitionFallible().As()
        from marked in marks.SwapIO(held => parts.Succs.Fold(held, static (all, row) => all.AddOrUpdate(row.Place.At, row.Mark)))
        from prepared in parts.Succs.Filter(static row => row.Mark.Shown).Map(static row => row.Place).TraverseM(Prepared).As()
        from seated in seats.ValueIO
        from shown in View.Show(Drawn(marked, seated))
        from failed in unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()
        select unit;

    private IO<Mark> Restated(Place place) =>
        from enabled in RowRules.Holds(place.Row.Rules.Enabled, scope)
        from shown in RowRules.Holds(place.Row.Rules.Visible, scope)
        from seat in seats.ValueIO.Map(held => held.Find(place.At))
        from modified in seat.Bind(static held => held.Cells.Edit).Match(Some: static edit => edit.Modified, None: static () => IO.pure(value: false))
        from applied in seat.Traverse(held => Applied(held, shown, enabled)).As()
        select new Mark(shown, enabled, modified);

    private static IO<Unit> Applied(Seat seat, bool shown, bool enabled) =>
        from activated in IO.lift(() => { seat.Slot.Enabled = enabled; })
        from available in seat.Cells.Availability.Traverse(changed => changed(shown && enabled)).As()
        from showed in seat.Cells.Show
        select unit;

    // --- [SLOTS]
    private IO<Unit> Prepared(Place place) =>
        seats.ValueIO.Bind(held => place.Row.Cells.IsNone || held.ContainsKey(place.At) ? IO.pure(unit) : Seated(place, handed: false).Map(static _ => unit));

    private IO<Control> Slotted(Seq<Key> path) =>
        from place in IO.lift(() => path.Last.Bind(slotted.Find).ToFin(new Refused(nameof(SceneView.Slots), Refused.Absent)))
        from waiting in seats.ValueIO.Map(held => held.Find(place.At).Filter(static seat => !seat.Handed))
        from slot in waiting.Match(
            Some: seat => seats.SwapIO(held => held.AddOrUpdate(place.At, seat with { Handed = true })).Map(_ => seat.Slot),
            None: () => Seated(place, handed: true).Bind(made => Restated(place).Map(_ => made)))
        select (Control)slot;

    private IO<RowSlot> Seated(Place place, bool handed) =>
        from realize in IO.lift(() => place.Row.Cells.Map(static cells => cells.Realize).ToFin(new Refused(nameof(ControlRow.Cells), Refused.Absent)))
        from lease in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from slot in DisposalOps.OnFailure(
            from cells in realize(scope, lease.Retain)
            from trail in (from keys in place.Row.Keys.AsIterableNE() from source in place.Row.Bound from edit in cells.Edit select (Keys: keys, Source: source, Edit: edit))
                .Traverse(held => Trailed(held.Source, held.Keys, held.Edit, place.Row.Rules.Enabled, lease.Retain)).As()
            let groups = trail.Map(static held => held.Groups).IfNone(Seq<Seq<Command<RowRule, Invoke>>>())
            from body in Body(cells, trail, lease.Retain)
            from menu in cells.Menu.Match(
                Some: attach => attach(groups, Refreshed(Seq(place)), scope, lease.Retain),
                None: () => CommandRows.Menu(groups, scope, lease.Retain).Map(static held => held.Menu))
            from made in IO.lift(() => new RowSlot(body, lease, Released(place.At)) { ToolTip = cells.Help.Text(place.Row.Text.Help, scope.Sink), ContextMenu = menu })
            from fitted in Fitted(cells)
            from stored in seats.SwapIO(held => held.AddOrUpdate(place.At, new Seat(made, cells, (float)made.GetPreferredSize().Height, handed)))
            select made,
            IO.lift(lease.Dispose))
        select slot;

    private Action<RowSlot> Released(int at) =>
        slot => seats.Swap(held => held.Find(at).Exists(seat => ReferenceEquals(seat.Slot, slot)) ? held.Remove(at) : held);

    private IO<Unit> Emptied =>
        from held in seats.ValueIO
        from cleared in seats.SwapIO(static _ => HashMap<int, Seat>())
        from disposed in IO.lift(() => toSeq(held.Values).Filter(static seat => !seat.Handed).Iter(static seat => seat.Slot.Dispose()))
        select unit;

    private IO<Unit> Fitted(RowCells cells) =>
        from width in widest.SwapIO(held => cells.Fits.Fold(held, static (most, fit) => float.Max(most, fit.Width)))
        from seated in seats.ValueIO
        from sized in IO.lift(() => (toSeq(seated.Values).Bind(static seat => seat.Cells.Fits) + cells.Fits).Iter(fit => fit.Field.Width = (int)MathF.Ceiling(width)))
        select unit;

    // --- [BODY]
    private IO<Control> Body(RowCells cells, Option<RowTrail> trail, Action<IDisposable> retain) =>
        from heads in cells.Heads.TraverseM(line => Line(line, retain)).As()
        from main in Lined(cells.Gate.ToSeq().Map(static gate => new TableCell(gate)).Add(new TableCell(cells.Value, scaleWidth: true))
            + cells.Pick.Map(static pick => new TableCell(pick)).ToSeq()
            + trail.ToSeq().Bind(static held => held.Buttons).Map(static button => new TableCell(button)), retain)
        from lines in cells.Lines.TraverseM(line => Line(line, retain)).As()
        from stack in Containers.Frame(() => new StackLayout {
            Orientation = Eto.Forms.Orientation.Vertical,
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        }, retain)
        from filled in IO.lift(() => (heads.Add(main) + lines).Iter(line => stack.Items.Add(new StackLayoutItem(line, expand: false))))
        select (Control)stack;

    private IO<Control> Line(RowLine line, Action<IDisposable> retain) =>
        from caption in line.Caption.Traverse(text => DisposalOps.Acquire(
            IO.lift(() => new Label { Text = RowText.Localize(text, scope.Sink).Local, VerticalAlignment = VerticalAlignment.Center }), retain)).As()
        from lined in Lined(caption.Map(static label => new TableCell(label)).ToSeq().Add(new TableCell(line.Value, scaleWidth: true)) + line.Pick.Map(static pick => new TableCell(pick)).ToSeq(), retain)
        select lined;

    internal static IO<Control> Lined(Seq<TableCell> cells, Action<IDisposable> retain) =>
        Containers.Frame(() => new TableLayout(new TableRow(cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }, retain).Map(static table => (Control)table);

    private IO<RowTrail> Trailed(RowSource source, IterableNE<EntryKey> keys, RowEdit edit, Option<RowRule> enabled, Action<IDisposable> retain) =>
        from bound in IO.lift(() => new BindingGroup(Seq(source.Values(scope))))
        let reads = toSeq(keys)
        let restore = new Command<RowRule, Invoke>.Run(
            new CommandFace("row-reset", "Reset", None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_namedItemRestoreHotSvg))),
            CommandRows.Gate(enabled, Some(new RowRule(reads, [], _ => edit.Modified))), new Invoke.Effect(_ => edit.Reset))
        let copy = new Command<RowRule, Invoke>.Run(new CommandFace("row-copy", "Copy", None, None), enabled, new Invoke.Effect(held => DataTransfer.Copy(bound, Some(keys), held)))
        let paste = new Command<RowRule, Invoke>.Run(
            new CommandFace("row-paste", "Paste", None, None),
            CommandRows.Gate(enabled, Some(new RowRule([], [], static _ => IO.lift(static () => Clipboard.Instance.ContainsText)))),
            new Invoke.Effect(held => DataTransfer.Paste(bound, Some(keys), held).Map(unit)))
        let padlock = scope.Locks.Map(_ => CommandRows.Toggle(
            new CommandFace("row-lock", "Lock", None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_LockSmallClosedSvg))),
            enabled, new RowRule(reads, [], held => held.Locked.Map(locked => reads.ForAll(locked.Contains))), (held, locked) => held.Lock(reads, locked)))
        from reset in CommandRows.Button(restore, IconSlot.PanelButton, scope, retain)
        from shackle in padlock.Traverse(toggle => CommandRows.Button(toggle, IconSlot.PanelButton, scope, retain)).As()
        select new RowTrail([[restore, .. padlock.ToSeq()], [copy, paste]], reset.Control.Cons(shackle.Map(static held => held.Control).ToSeq()));

    // --- [SCENE]
    private Node Drawn(HashMap<int, Mark> marked, HashMap<int, Seat> seated) =>
        new Node.Flex(Rasm.UI.Orientation.Vertical, Segments(places.Filter(place => marked.Find(place.At).IfNone(Mark.Plain).Shown), marked, seated)) { Width = Sizing.Grow.Unit };

    private Seq<Node> Segments(Seq<Place> shown, HashMap<int, Mark> marked, HashMap<int, Seat> seated) =>
        shown.Fold(Seq<Seq<Place>>(), static (runs, place) => runs.Last.Filter(run => !place.Spans && !run.Exists(static held => held.Spans)).Match(
                Some: run => runs.Init.Add(run.Add(place)),
                None: () => runs.Add(Seq(place))))
            .Bind(run => run.Exists(static place => place.Spans)
                ? run.Map(place => Spanning(place, Marked(marked, place), seated))
                : Seq(FieldRow.Drawn(run.Map(place => new FieldRow(place.Named, Captioned(place), Valued(place, seated)) { Conditions = Conditioned(Marked(marked, place)) }))));

    private Node Spanning(Place place, Mark mark, HashMap<int, Seat> seated) =>
        new Node.Flex(Rasm.UI.Orientation.Vertical,
            Captioned(place).Map(caption => (Node)new Node.Text([caption]) { Role = Role.Label, Conditions = Conditioned(mark) })
                .Add(Valued(place, seated) with { Accessible = new Accessible { Label = Captioned(place).Map(static caption => caption.Text).Head } })) {
            Key = place.Named,
            Width = Sizing.Grow.Unit,
            Conditions = Conditioned(mark),
        };

    private Seq<Span> Captioned(Place place) => place.Labeled ? [RowText.Localize(place.Row.Text.Caption, scope.Sink).Local] : [];

    private Node Valued(Place place, HashMap<int, Seat> seated) =>
        place.Row.Cells.IsNone
            ? new Node.Box()
            : new Node.Slot {
                Key = place.Slot,
                Width = Sizing.Grow.Unit,
                Height = new Sizing.Fixed(seated.Find(place.At).Map(static seat => seat.Height).IfNone(0f)),
                Accessible = new Accessible { Help = RowText.Localize(place.Row.Text.Help, scope.Sink).Local },
            };

    private static Mark Marked(HashMap<int, Mark> marked, Place place) => marked.Find(place.At).IfNone(Mark.Plain);

    private static LanguageExt.HashSet<Condition> Conditioned(Mark mark) =>
        toHashSet(Seq((Held: !mark.Enabled, Condition: Condition.Disabled), (Held: mark.Modified, Condition: Condition.Modified)).Filter(static row => row.Held).Map(static row => row.Condition));

    // --- [ROWS]
    private sealed record Place(int At, ControlRow Row) {
        public Key Named => new Key.Named($"row-{At}");
        public Key Slot => new Key.Named($"slot-{At}");
        public bool Spans => Row.Cells.Exists(static cells => cells.Shape.Spans);
        public bool Labeled => Row.Cells.ForAll(static cells => cells.Shape.Labeled);
        public Seq<EntryKey> Reads => Row.Keys + Row.Rules.Each.Bind(static rule => rule.Reads);
        public Seq<RowSource> Sources => Row.Rules.Each.Bind(static rule => rule.Sources);
    }

    private sealed record Mark(bool Shown, bool Enabled, bool Modified) {
        public static Mark Plain { get; } = new(Shown: true, Enabled: true, Modified: false);
    }

    private sealed record Seat(RowSlot Slot, RowCells Cells, float Height, bool Handed);

    private sealed record RowTrail(Seq<Seq<Command<RowRule, Invoke>>> Groups, Seq<Control> Buttons);

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
