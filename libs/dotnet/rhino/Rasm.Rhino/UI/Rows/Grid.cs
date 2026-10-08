using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
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
    public string Text(string help, Wording wording, IPlugInSink sink) =>
        string.Join(Environment.NewLine, wording.Shown(help, sink).Cons(
            Range.Map(static range => RowText.Localize("{0} – {1}", arguments: [range.Low, range.High]).Local).ToSeq()
                .Concat(Default.Map(static value => RowText.Localize("Default {0}", arguments: [value]).Local).ToSeq())));
}

public sealed record RowLine(Option<string> Caption, Control Value, Option<Control> Pick, Option<FieldFit> Field);

public sealed record RowCells(
    Control Value, Option<Control> Gate, Option<Control> Pick, Option<FieldFit> Field, Seq<RowLine> Heads, Seq<RowLine> Lines, RowHelp Help, IO<Unit> Show,
    Option<RowEdit> Edit, Option<Func<Option<Label>, Seq<Seq<Command>>, IO<Unit>, IO<IDisposable>>> Menu, IDisposable Release);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowGrid : Panel {
    // --- [STATE]
    private const float LabelShare = 0.4f;

    private readonly RowScope scope;
    private readonly Seq<RowPlace> places;
    private readonly RowMetrics metrics;
    private readonly HashMap<EntryKey, Seq<RowPlace>> index;
    private readonly Atom<(RowForm Before, RowForm After)> form;
    private readonly Atom<(Seq<IDisposable> Held, Seq<IDisposable> Taken)> acquired;

    private RowGrid(RowScope scope, Seq<RowPlace> places, RowMetrics metrics, Seq<IDisposable> held) {
        (Sink, this.scope, this.places, this.metrics) = (scope.Sink, scope, places, metrics);
        index = RuleIndex.Of(places.Map(static place => (place.Reads, place)));
        form = Atom((Before: RowForm.Stacked, After: RowForm.Stacked));
        acquired = Atom((Held: held, Taken: Seq<IDisposable>()));
        Style = HandlerStyle;
    }

    public static string HandlerStyle { get; } = ComponentControl.StyleName(typeof(RowGrid));

    public IPlugInSink Sink { get; }

    // --- [REALIZE]
    public static IO<RowGrid> Realize(Seq<ControlRow> column, Seq<ControlRow> rows, RowScope scope) =>
        from cells in DisposalOps.AcquireAll(rows.Map(row => Cells(row, scope)), static held => DisposalOps.Release(held.Map(static cell => cell.Release)))
        from placed in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(rows.Zip(cells).Map(row => Placed(row.First, row.Second, scope)), static held => DisposalOps.Release(held.Bind(static place => place.Held))),
            DisposalOps.Release(cells.Map(static cell => cell.Release)))
        let held = cells.Map(static cell => cell.Release) + placed.Bind(static place => place.Held)
        from metrics in DisposalOps.OnFailure(Measured(column, placed.Map(static place => place.Place), scope.Sink), DisposalOps.Release(held))
        from grid in IO.lift(() => new RowGrid(scope, placed.Map(static place => place.Place), metrics, held + Seq<IDisposable>(metrics.Regular, metrics.Emphasis)))
        from attached in DisposalOps.OnFailure(grid.Attached, IO.lift(grid.Dispose))
        select grid;

    public Option<Label> Caption(Control value) =>
        places.Find(place => ReferenceEquals(place.Cells.Value, value)).Bind(static place => place.Label);

    private static IO<RowCells> Cells(ControlRow row, RowScope scope) =>
        row.Switch(
            scope,
            field: static (held, field) => field.Realize(held),
            readout: static (held, readout) => readout.Realize(held),
            command: static (held, command) => command.Realize(held),
            group: static (held, group) => IO.lift(() => new LabelSeparator { Text = group.Wording.Shown(group.Caption, held.Sink) })
                .Map(static separator => new RowCells(separator, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None, separator)));

    private static IO<(RowPlace Place, Seq<IDisposable> Held)> Placed(ControlRow row, RowCells cells, RowScope scope) =>
        from label in IO.lift(() => row.Layout
            .Filter(shape => shape.Labeled && (!shape.Spans || cells.Heads.IsEmpty))
            .Map(shape => row.Wording.Shown(row.Caption, scope.Sink) switch {
                var caption => shape.Spans ? RhinoLayout.NewLabelSeparator(caption) : RhinoLayout.NewLabel(caption),
            }))
        let help = row.Help.Map(text => cells.Help.Text(text, row.Wording, scope.Sink))
        from tipped in IO.lift(() => label.Map(static held => (Control)held).ToSeq().Add(cells.Value).Iter(control => control.ToolTip = help.ValueUnsafe()))
        from trail in (from keys in row.Keys.AsIterableNE() from source in row.Bound from edit in cells.Edit select Trailed(source, keys, edit, scope)).Traverse(static trailed => trailed).As()
        let first = trail.Map(static held => held.Trail)
        from heads in IO.lift(() => cells.Heads.Head.Map(line => GridLine.Of(line, row.Wording, scope.Sink, first)).ToSeq() + cells.Heads.Tail.Map(line => GridLine.Of(line, row.Wording, scope.Sink, None)))
        from lines in IO.lift(() => cells.Lines.Map(line => GridLine.Of(line, row.Wording, scope.Sink, None)))
        let laid = row.Layout.Match(
            Some: shape => shape.Spans
                ? label.Map(caption => new GridLine(None, Columned: false, cells.Gate, caption, cells.Pick, first, Trailing: true, Fills: false)).ToSeq()
                  + heads
                  + Seq(new GridLine(None, Columned: false, None, cells.Value, None, None, Trailing: false, shape.Fills))
                  + lines
                : Seq(new GridLine(label, Columned: true, cells.Gate, cells.Value, cells.Pick, first, Trailing: true, Fills: false)),
            None: () => Seq(new GridLine(None, Columned: false, None, cells.Value, None, None, Trailing: false, Fills: false)))
        select (new RowPlace(row, cells, label, laid, first), trail.ToSeq().Bind(static held => held.Held));

    private static IO<(RowTrail Trail, Seq<IDisposable> Held)> Trailed(RowSource source, IterableNE<EntryKey> keys, RowEdit edit, RowScope scope) =>
        from group in IO.lift(BindingGroup.Of(Seq(source.Values(scope))).ToFin())
        from restore in IO.lift(static () => new Command { MenuText = RowText.Localize("Reset").Local })
        from copy in IO.lift(static () => new Command { MenuText = RowText.Localize("Copy").Local })
        from paste in IO.lift(static () => new Command { MenuText = RowText.Localize("Paste").Local })
        from reset in Sized(scope.Sink, GlyphRole.Reset, new ImageButton { Command = restore })
        from padlock in scope.Locks
            .Traverse(_ => Sized(scope.Sink, GlyphRole.LockOpen, new ImageButton()).Map(static button => (Command: new CheckCommand { MenuText = RowText.Localize("Lock").Local }, Button: button)))
            .As()
        from held in DisposalOps.AcquireAll(
            Seq((Command: restore, Run: edit.Reset), (Command: copy, Run: Transfer.Copy(group, Some(keys), scope)), (Command: paste, Run: Transfer.Paste(group, Some(keys), scope).Map(static _ => unit)))
                .Map(row => Subscriptions.Host<EventArgs>(typeof(Command), h => row.Command.Executed += h, h => row.Command.Executed -= h, nameof(Command.Executed)).Inline(_ => row.Run, scope.Sink))
            + padlock.ToSeq().Bind(row => Seq(
                Subscriptions.Host<EventArgs>(typeof(CheckCommand), h => row.Command.CheckedChanged += h, h => row.Command.CheckedChanged -= h, nameof(CheckCommand.CheckedChanged))
                    .Inline(_ => scope.Lock(toSeq(keys), row.Command.Checked), scope.Sink),
                Subscriptions.Host<EventArgs>(typeof(ImageButton), h => row.Button.Click += h, h => row.Button.Click -= h, nameof(ImageButton.Click))
                    .Inline(_ => scope.Lock(toSeq(keys), !row.Command.Checked), scope.Sink))),
            DisposalOps.Release)
        select(new RowTrail(keys, reset, restore, copy, paste, padlock), held);

    private static IO<ImageButton> Sized(IPlugInSink sink, GlyphRole glyph, ImageButton button) =>
        Icons.Frames(sink, glyph, IconSlot.PanelButton).Bind(icon => IO.lift(() => {
            button.Image = icon;
            button.Size = button.Size;
            return button;
        }));

    // --- [FORM]
    private static bool Beside(float width, float labels, float values, float trailing, float spacing) =>
        labels <= LabelShare * width && labels + spacing + values + trailing <= width;

    private static IO<RowMetrics> Measured(Seq<ControlRow> column, Seq<RowPlace> places, IPlugInSink sink) =>
        from labels in column
            .Filter(static row => row.Layout.Exists(static shape => shape.Labeled && !shape.Spans))
            .TraverseM(row => use(() => RhinoLayout.NewLabel(row.Wording.Shown(row.Caption, sink))).Map(static label => label.GetPreferredSize().Width).Bracket())
            .As()
        let fields = places.Bind(static place => place.Cells.Field.ToSeq() + (place.Cells.Heads + place.Cells.Lines).Choose(static line => line.Field))
        let field = fields.Fold(0f, static (widest, fit) => MathF.Max(widest, fit.Width))
        from sized in IO.lift(() => fields.Iter(fit => fit.Field.Width = (int)MathF.Ceiling(field)))
        let lines = places.Bind(static place => place.Lines)
        from widths in IO.lift(() => (
            Values: Widest(lines.Filter(static line => line.Columned).Map(static line => line.Value)),
            Gate: Widest(lines.Choose(static line => line.Gate)),
            Pick: Widest(lines.Choose(static line => line.Pick))))
        from fonts in IO.lift(static () => SystemFonts.Label() switch {
            var regular => (Regular: regular, Emphasis: new Font(regular.Family, regular.Size, FontStyle.Bold)),
        })
        select new RowMetrics(
            labels.Fold(0f, MathF.Max),
            widths.Gate,
            widths.Pick,
            places.Choose(static place => place.Trail).Head.Map(static trail => (float)trail.Reset.Width),
            places.Choose(static place => place.Trail).Choose(static trail => trail.Lock).Head.Map(static held => (float)held.Button.Width),
            widths.Values.IfNone(0f),
            RhinoLayout.Spacing(RhinoLayout.SpacingType.Table),
            fonts.Regular,
            fonts.Emphasis);

    private static Option<float> Widest(Seq<Control> controls) =>
        controls.Map(static control => control.GetPreferredSize().Width).Fold(Option<float>.None, static (widest, width) => Some(MathF.Max(widest.IfNone(width), width)));

    private IO<Unit> Formed =>
        IO.lift(() => (float)Width).Bind(width => Shaped(held => held with { Beside = Beside(width, metrics.Labels, metrics.Values, metrics.Trailing, metrics.Spacing.Width) }));

    private IO<Unit> Shaped(Func<RowForm, RowForm> step) =>
        form.SwapIO(held => (held.After, step(held.After))).Bind(held => when(held.Before != held.After, Laid(held.After)).As());

    private IO<Unit> Laid(RowForm shaped) =>
        from old in IO.lift(() => Optional(Content).ToSeq())
        from detached in IO.lift(() => places.Bind(static place => place.Parts).Iter(static part => part.Detach()))
        from content in IO.lift(() => Content = Stacked(places
            .Filter(place => !shaped.Hidden.Contains(place))
            .Bind(static place => place.Lines)
            .Map(line => new StackLayoutItem(Rendered(line, shaped.Beside), expand: line.Fills))))
        from released in DisposalOps.Release(old)
        select unit;

    private Control Rendered(GridLine line, bool beside) =>
        Table(
            Fixed(line.Caption.Map(static caption => (Control)caption), Some(metrics.Labels).Filter(_ => line.Columned && beside))
            + Fixed(line.Gate, metrics.Gate.Filter(_ => line.Columned || line.Gate.IsSome))
            + Seq(new TableCell(line.Value, scaleWidth: true))
            + Fixed(line.Pick, metrics.Pick.Filter(_ => line.Trailing))
            + Fixed(line.Trail.Map(static trail => (Control)trail.Reset), metrics.Reset.Filter(_ => line.Trailing))
            + Fixed(line.Trail.Bind(static trail => trail.Lock).Map(static held => (Control)held.Button), metrics.Lock.Filter(_ => line.Trailing)),
            line.Fills) switch {
                var table => line.Caption.Filter(_ => line.Columned && !beside).Match(
                    Some: caption => (Control)Stacked(Seq(new StackLayoutItem(caption, HorizontalAlignment.Left), new StackLayoutItem(table))),
                    None: () => table),
            };

    private static Seq<TableCell> Fixed(Option<Control> control, Option<float> width) =>
        width.Map(held => new TableCell(new Panel { Width = (int)MathF.Ceiling(held), Content = control.ValueUnsafe() })).ToSeq();

    private TableLayout Table(Seq<TableCell> cells, bool fills) =>
        new(new TableRow(cells) { ScaleHeight = fills }) { Spacing = metrics.Spacing, Padding = 0 };

    private StackLayout Stacked(Seq<StackLayoutItem> items) =>
        new([.. items]) {
            Orientation = Orientation.Vertical,
            Spacing = metrics.Spacing.Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

    // --- [REFRESH]
    public IO<Unit> Refresh(Seq<EntryKey> changed) => Refreshed(RuleIndex.Affected(index, changed));

    private IO<Unit> Attached =>
        from held in DisposalOps.AcquireAll(Registrations, DisposalOps.Release)
        from kept in acquired.SwapIO(state => state with { Held = state.Held + held })
        from laid in form.ValueIO.Bind(shaped => Laid(shaped.After))
        from formed in Formed
        from shown in Refreshed(places)
        select unit;

    private Seq<IO<IDisposable>> Registrations =>
        Seq(scope.Listen(Refresh),
            Themes.Changed.Inline(_ => Refreshed(places), Sink),
            Subscriptions.Host<EventArgs>(typeof(RowGrid), h => SizeChanged += h, h => SizeChanged -= h, nameof(SizeChanged)).Inline(_ => Formed, Sink))
        + places.Choose(place => place.Cells.Menu.Map(attach => attach(place.Label, place.Trail.Map(static trail => trail.Groups).IfNone(Seq<Seq<Command>>()), Refreshed(Seq(place)))))
        + places.Bind(static place => place.Sources).Distinct()
            .Filter(source => !places.Exists(place => place.Row.Bound.Exists(bound => bound == source)))
            .Map(Watched);

    private IO<IDisposable> Watched(RowSource source) =>
        from signals in source.Signals(scope)
        let site = new CallbackSite(Sink, typeof(RowGrid), nameof(Watched))
        let reading = places.Filter(place => place.Sources.Exists(read => read == source))
        from mailbox in Subscriptions.Idle<Unit, Unit>(_ => Refreshed(reading))(site)
        let post = fun((Unit raised) => mailbox.Post((raised, raised)))
        from held in DisposalOps.OnFailure(DisposalOps.AcquireAll(signals.Map(signal => signal.Inline(post, Sink)), DisposalOps.Release), IO.lift(mailbox.Release.Dispose))
        select DisposalOps.Composite(mailbox.Release.Cons(held), site);

    private IO<Unit> Refreshed(Seq<RowPlace> targets) =>
        from parts in targets.Map<K<IO, (RowPlace Place, bool Shown)>>(place => Restated(place).Map(shown => (place, shown))).PartitionFallible().As()
        from laid in Shaped(held => held with { Hidden = parts.Succs.Fold(held.Hidden, static (hidden, row) => row.Shown ? hidden.Remove(row.Place) : hidden.TryAdd(row.Place)) })
        from failed in unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()
        select unit;

    private IO<bool> Restated(RowPlace place) =>
        from enabled in place.Row.Rules.Enables(scope)
        from shown in place.Row.Rules.Shows(scope)
        from modified in place.Cells.Edit.Match(Some: static edit => edit.Modified, None: static () => IO.pure(value: false))
        from locked in place.Trail.Match(Some: trail => scope.Locked.Map(held => trail.Keys.ForAll(held.Contains)), None: static () => IO.pure(value: false))
        let ink = (enabled, modified) switch {
            (false, _) => PaintSlot.ContentTextDisabled,
            (true, true) => PaintSlot.ContentHighlight,
            (true, false) => PaintSlot.ContentTextEnabled,
        }
        from reset in place.Trail.Filter(_ => modified).Traverse(_ => Icons.Frames(Sink, GlyphRole.Reset, IconSlot.PanelButton)).As()
        from padlock in place.Trail.Bind(static trail => trail.Lock).Traverse(_ => Icons.Frames(Sink, locked ? GlyphRole.LockClosed : GlyphRole.LockOpen, IconSlot.PanelButton)).As()
        from painted in IO.lift(() => Painted(place, enabled, modified, locked, ink, reset, padlock))
        from showed in place.Cells.Show
        select shown;

    private Unit Painted(RowPlace place, bool enabled, bool modified, bool locked, PaintSlot ink, Option<Icon> reset, Option<Icon> padlock) {
        _ = place.Lines.Bind(static line => line.Controls).Iter(control => control.Enabled = enabled);
        _ = place.Label.Iter(label => (label.Font, label.TextColor) = (modified ? metrics.Emphasis : metrics.Regular, ink.Read()));
        _ = place.Trail.Iter(trail => (trail.Reset.Image, trail.Restore.Enabled, trail.Paste.Enabled) = (reset.ValueUnsafe(), enabled && modified, Clipboard.Instance.ContainsText));
        return place.Trail.Bind(static trail => trail.Lock).Iter(held => (held.Button.Image, held.Button.Enabled, held.Command.Enabled, held.Command.Checked) = (padlock.ValueUnsafe(), enabled, enabled, locked));
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            DisposalOps.Composite(acquired.Swap(static state => (Seq<IDisposable>(), state.Held)).Taken, new CallbackSite(Sink, typeof(RowGrid), nameof(Dispose))).Dispose();
        base.Dispose(disposing);
    }

    private sealed record GridLine(Option<Label> Caption, bool Columned, Option<Control> Gate, Control Value, Option<Control> Pick, Option<RowTrail> Trail, bool Trailing, bool Fills) {
        public Seq<Control> Controls => Caption.Map(static caption => (Control)caption).ToSeq() + Gate.ToSeq().Add(Value) + Pick.ToSeq();

        public static GridLine Of(RowLine line, Wording wording, IPlugInSink sink, Option<RowTrail> trail) =>
            new(line.Caption.Map(caption => RhinoLayout.NewLabel(wording.Shown(caption, sink))), line.Caption.IsSome, None, line.Value, line.Pick, trail, Trailing: true, Fills: false);
    }

    private sealed record RowTrail(IterableNE<EntryKey> Keys, ImageButton Reset, Command Restore, Command Copy, Command Paste, Option<(CheckCommand Command, ImageButton Button)> Lock) {
        public Seq<Seq<Command>> Groups => Seq(Restore.Cons(Lock.Map(static held => (Command)held.Command).ToSeq()), Seq(Copy, Paste));

        public Seq<Control> Buttons => Seq<Control>(Reset) + Lock.Map(static held => (Control)held.Button).ToSeq();
    }

    private sealed record RowPlace(ControlRow Row, RowCells Cells, Option<Label> Label, Seq<GridLine> Lines, Option<RowTrail> Trail) {
        public Seq<EntryKey> Reads => Row.Keys + Row.Rules.Each.Bind(static rule => rule.Reads);

        public Seq<RowSource> Sources => Row.Rules.Each.Bind(static rule => rule.Sources);

        public Seq<Control> Parts => Lines.Bind(static line => line.Controls) + Trail.ToSeq().Bind(static trail => trail.Buttons);
    }

    private sealed record RowMetrics(float Labels, Option<float> Gate, Option<float> Pick, Option<float> Reset, Option<float> Lock, float Values, Size Spacing, Font Regular, Font Emphasis) {
        public float Trailing => Seq(Gate, Pick, Reset, Lock).Somes().Fold(0f, (sum, width) => sum + width + Spacing.Width);
    }

    private sealed record RowForm(bool Beside, LanguageExt.HashSet<RowPlace> Hidden) {
        public static RowForm Stacked { get; } = new(Beside: false, []);
    }
}
