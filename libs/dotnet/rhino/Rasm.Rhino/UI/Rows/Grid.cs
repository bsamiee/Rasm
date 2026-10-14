using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Drawing;
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
    Option<RowEdit> Edit, Option<Func<Option<Label>, Seq<Seq<Command>>, IO<Unit>, Action<IDisposable>, IO<Unit>>> Menu) {
    public IO<bool> NestedEditing { get; init; } = IO.pure(value: false);
    public Option<ComponentControl> Plot { get; init; }
    public Option<Func<bool, IO<Unit>>> Availability { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowGrid : Panel {
    // --- [STATE]
    private const float LabelShare = 0.4f;

    private readonly RowScope scope;
    private readonly Seq<RowPlace> places;
    private readonly RowMetrics metrics;
    private readonly HashMap<EntryKey, Seq<RowPlace>> index;
    private readonly Atom<RowForm> form;
    private readonly Ref<Seq<IDisposable>> acquired;
    private Seq<IDisposable> layouts;

    private RowGrid(RowScope scope, Seq<RowPlace> places, RowMetrics metrics, Ref<Seq<IDisposable>> acquired) {
        (Sink, this.scope, this.places, this.metrics, this.acquired) = (scope.Sink, scope, places, metrics, acquired);
        index = RuleIndex.Of(places.Map(static place => (place.Reads, place)));
        form = Atom(RowForm.Stacked);
    }

    public static string HandlerStyle { get; } = ComponentControl.StyleName(typeof(RowGrid));

    public IPlugInSink Sink { get; }

    // --- [REALIZE]
    public static IO<RowGrid> Realize(Seq<ControlRow> column, Seq<ControlRow> rows, RowScope scope, Action<IDisposable> retain) =>
        from acquired in IO.lift(static () => Ref(Seq<IDisposable>()))
        let keep = new Action<IDisposable>(held => atomic(() => acquired.Value = acquired.Value.Add(held)))
        let prefix = new Disposal<IO<Unit>>(
            atomicIO(() => {
                Seq<IDisposable> held = acquired.Value;
                acquired.Value = [];
                return held;
            }).Bind(DisposalOps.Release),
            static release => release.Run())
        from grid in DisposalOps.OnFailure(
            from cells in rows.TraverseM(row => Cells(row, scope, keep)).As()
            from placed in rows.Zip(cells).TraverseM(row => Placed(row.First, row.Second, scope, keep)).As()
            from metrics in Measured(column, placed, scope.Sink)
            from created in DisposalOps.Acquire(IO.lift(() => new RowGrid(scope, placed, metrics, acquired)), static owned => IO.lift(owned.Dispose), owned => {
                prefix.Relinquish();
                retain(owned);
            })
            select created,
            IO.lift(prefix.Dispose))
        from attached in DisposalOps.OnFailure(IO.lift(() => grid.Style = HandlerStyle).Bind(_ => grid.Attached), IO.lift(grid.Dispose))
        select grid;

    public Option<Label> Caption(Control value) =>
        places.Find(place => ReferenceEquals(place.Cells.Value, value)).Bind(static place => place.Label);

    public Option<ComponentControl> Plot(Seq<EntryKey> keys) =>
        toHashSet(keys) switch { var selected => places.Choose(place => place.Cells.Plot.Filter(_ => place.Row.Keys.Exists(selected.Contains))).Head };

    private static IO<RowCells> Cells(ControlRow row, RowScope scope, Action<IDisposable> retain) =>
        row.Switch(
            (Scope: scope, Retain: retain),
            field: static (held, field) => field.Realize(held.Scope, held.Retain),
            readout: static (held, readout) => readout.Realize(held.Scope, held.Retain),
            command: static (held, command) => command.Realize(held.Scope, held.Retain),
            group: static (held, @group) =>
                from separator in DisposalOps.Acquire(IO.lift(static () => new LabelSeparator()), static separator => IO.lift(separator.Dispose), held.Retain)
                from caption in IO.lift(() => separator.Text = @group.Wording.Shown(@group.Caption, held.Scope.Sink))
                select new RowCells(separator, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None));

    private static IO<RowPlace> Placed(ControlRow row, RowCells cells, RowScope scope, Action<IDisposable> retain) =>
        from label in row.Layout
            .Filter(shape => shape.Labeled && (!shape.Spans || cells.Heads.IsEmpty))
            .Traverse(shape => DisposalOps.Acquire(IO.lift(() => row.Wording.Shown(row.Caption, scope.Sink) switch {
                var caption => shape.Spans ? RhinoLayout.NewLabelSeparator(caption) : RhinoLayout.NewLabel(caption),
            }), static label => IO.lift(label.Dispose), retain)).As()
        let help = row.Help.Map(text => cells.Help.Text(text, row.Wording, scope.Sink))
        from tipped in IO.lift(() => label.Map(static held => (Control)held).ToSeq().Add(cells.Value).Iter(control => control.ToolTip = help.ValueUnsafe()))
        from first in (from keys in row.Keys.AsIterableNE() from source in row.Bound from edit in cells.Edit select Trailed(source, keys, edit, scope, retain)).Traverse(static trailed => trailed).As()
        from head in cells.Heads.Head.Traverse(line => GridLine.Of(line, row.Wording, scope.Sink, first, retain)).As()
        from tail in cells.Heads.Tail.TraverseM(line => GridLine.Of(line, row.Wording, scope.Sink, None, retain)).As()
        let heads = head.ToSeq() + tail
        from lines in cells.Lines.TraverseM(line => GridLine.Of(line, row.Wording, scope.Sink, None, retain)).As()
        let laid = row.Layout.Match(
            Some: shape => shape.Spans
                ? label.Map(caption => new GridLine(None, Columned: false, cells.Gate, caption, cells.Pick, first, Trailing: true, Fills: false)).ToSeq()
                  + heads
                  + Seq(new GridLine(None, Columned: false, None, cells.Value, None, None, Trailing: false, shape.Fills))
                  + lines
                : Seq(new GridLine(label, Columned: true, cells.Gate, cells.Value, cells.Pick, first, Trailing: true, Fills: false)),
            None: () => Seq(new GridLine(None, Columned: false, None, cells.Value, None, None, Trailing: false, Fills: false)))
        select new RowPlace(row, cells, label, laid, first);

    private static IO<RowTrail> Trailed(RowSource source, IterableNE<EntryKey> keys, RowEdit edit, RowScope scope, Action<IDisposable> retain) =>
        from bound in IO.lift(() => new BindingGroup(Seq(source.Values(scope))))
        from restore in IO.lift(static () => new Command { MenuText = RowText.Localize("Reset").Local })
        from copy in IO.lift(static () => new Command { MenuText = RowText.Localize("Copy").Local })
        from paste in IO.lift(static () => new Command { MenuText = RowText.Localize("Paste").Local })
        from reset in Sized(GlyphRole.Reset, retain)
        from assigned in IO.lift(() => reset.Command = restore)
        from padlock in scope.Locks
            .Traverse(_ => Sized(GlyphRole.LockOpen, retain).Map(static button => (Command: new CheckCommand { MenuText = RowText.Localize("Lock").Local }, Button: button)))
            .As()
        from held in (Seq((Command: restore, Run: edit.Reset), (Command: copy, Run: Transfer.Copy(bound, Some(keys), scope)), (Command: paste, Run: Transfer.Paste(bound, Some(keys), scope).Map(static _ => unit)))
                .Map(row => Subscriptions.Host<EventArgs>(typeof(Command), h => row.Command.Executed += h, h => row.Command.Executed -= h, nameof(Command.Executed)).Inline(_ => row.Run, scope.Sink, retain))
            + padlock.ToSeq().Bind(row => Seq(
                Subscriptions.Host<EventArgs>(typeof(CheckCommand), h => row.Command.Executed += h, h => row.Command.Executed -= h, nameof(CheckCommand.Executed))
                    .Inline(_ => scope.Lock(toSeq(keys), row.Command.Checked), scope.Sink, retain),
                Subscriptions.Host<EventArgs>(typeof(ImageButton), h => row.Button.Click += h, h => row.Button.Click -= h, nameof(ImageButton.Click))
                    .Inline(_ => scope.Lock(toSeq(keys), !row.Command.Checked), scope.Sink, retain))))
            .TraverseM(static attached => attached).As()
        select new RowTrail(keys, reset, restore, copy, paste, padlock);

    private static IO<ImageButton> Sized(GlyphRole glyph, Action<IDisposable> retain) =>
        from icon in Icons.Frames((glyph, IconSlot.PanelButton.Master), IconSlot.PanelButton)
        from button in DisposalOps.Acquire(IO.lift(static () => new ImageButton()), static button => IO.lift(button.Dispose), retain)
        from assigned in IO.lift(() => button.Image = icon)
        select button;

    // --- [FORM]
    private static bool Beside(float width, float labels, float values, float trailing, float spacing) =>
        labels <= LabelShare * width && labels + spacing + values + trailing <= width;

    private static IO<RowMetrics> Measured(Seq<ControlRow> column, Seq<RowPlace> places, IPlugInSink sink) =>
        from labels in Callbacks.Each(column
            .Filter(static row => row.Layout.Exists(static shape => shape.Labeled && !shape.Spans))
            .Map(row => DisposalOps.Within(() => RhinoLayout.NewLabel(row.Wording.Shown(row.Caption, sink)), static label => IO.lift(() => label.GetPreferredSize().Width), static label => IO.lift(label.Dispose))))
        let fields = places.Bind(static place => place.Cells.Field.ToSeq() + (place.Cells.Heads + place.Cells.Lines).Choose(static line => line.Field))
        let field = fields.Fold(0f, static (widest, fit) => MathF.Max(widest, fit.Width))
        from sized in IO.lift(() => fields.Iter(fit => fit.Field.Width = (int)MathF.Ceiling(field)))
        let lines = places.Bind(static place => place.Lines)
        from widths in IO.lift(() => (
            Values: Widest(lines.Filter(static line => line.Columned).Map(static line => line.Value)),
            Gate: Widest(lines.Choose(static line => line.Gate)),
            Pick: Widest(lines.Choose(static line => line.Pick))))
        from fonts in IO.lift(static () => SystemFonts.Label() switch {
            var regular => (Regular: regular, Emphasis: SystemFonts.Bold(regular.Size)),
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
        form.ValueIO.Bind(held => step(held) switch {
            var next => when(held != next, Laid(held, next)).As(),
        });

    private IO<Unit> Laid(RowForm before, RowForm shaped) =>
        from laid in DisposalOps.Replace(
                retain =>
                    from rows in places.Filter(place => !shaped.Hidden.Contains(place)).Bind(static place => place.Lines)
                        .TraverseM(line => Rendered(line, shaped.Beside, retain).Map(held => (Item: new StackLayoutItem(held.Control, expand: line.Fills), held.Attach))).As()
                    from stack in Stacked(rows.Map(static held => held.Item), retain)
                    select (Root: stack, Attach: Callbacks.Each(rows.Map(static held => held.Attach)).Map(static _ => unit)),
                (next, release) => {
                    Seq<IDisposable> prior = layouts;
                    Seq<(Control Part, Panel? Parent)> parents = places.Bind(static place => place.Parts).Distinct()
                        .Map(static part => (part, (Panel?)part.Parent));
                    Control? content = Content;
                    layouts = prior.Add(release);
                    IO<Unit> rejected = IO.lift(() => layouts = prior).Bind(_ => IO.lift(release.Dispose));
                    IO<Unit> restore = Callbacks.Each(parents.Map(static held => IO.lift(() => {
                        held.Part.Detach();
                        if (held.Parent is { } parent)
                            parent.Content = held.Part;
                    }))).Map(static _ => unit)
                        .Bind(_ => IO.lift(() => Content = content)).Bind(_ => form.SwapIO(_ => before)).Map(static _ => unit);
                    IO<Unit> apply = form.SwapIO(_ => shaped)
                        .Bind(_ => Callbacks.Each(parents.Map(static held => IO.lift(held.Part.Detach))))
                        .Bind(_ => next.Attach).Bind(_ => IO.lift(() => Content = next.Root))
                        .Map(static _ => unit);
                    return DisposalOps.OnFailure(apply, restore.Bind(_ => rejected))
                        .Bind(_ => IO.lift(() => layouts = Seq(release)))
                        .Bind(_ => DisposalOps.Release(prior));
                })
        select unit;

    private IO<(Control Control, IO<Unit> Attach)> Rendered(GridLine line, bool beside, Action<IDisposable> retain) =>
        from attachments in IO.lift(static () => new List<IO<Unit>>())
        from caption in Fixed(line.Caption.Map(static caption => (Control)caption), Some(metrics.Labels).Filter(_ => line.Columned && beside), retain)
        from gate in Fixed(line.Gate, metrics.Gate.Filter(_ => line.Columned || line.Gate.IsSome), retain)
        from pick in Fixed(line.Pick, metrics.Pick.Filter(_ => line.Trailing), retain)
        from reset in Fixed(line.Trail.Map(static trail => (Control)trail.Reset), metrics.Reset.Filter(_ => line.Trailing), retain)
        from padlock in Fixed(line.Trail.Bind(static trail => trail.Lock).Map(static held => (Control)held.Button), metrics.Lock.Filter(_ => line.Trailing), retain)
        from value in DisposalOps.Acquire(IO.lift(static () => new Panel()), static panel => IO.lift(panel.Dispose), retain)
        from table in DisposalOps.Acquire(IO.lift(static () => new TableLayout()), static table => IO.lift(table.Dispose), retain)
        from arranged in IO.lift(() => {
            table.Spacing = metrics.Spacing;
            table.Padding = 0;
            table.Rows.Add(new TableRow(caption.Map(static held => held.Cell) + gate.Map(static held => held.Cell) + Seq(new TableCell(value, scaleWidth: true))
                + pick.Map(static held => held.Cell) + reset.Map(static held => held.Cell) + padlock.Map(static held => held.Cell)) { ScaleHeight = line.Fills });
            attachments.AddRange((caption + gate + pick + reset + padlock).Map(static held => held.Attach));
            attachments.Add(IO.lift(() => value.Content = line.Value).Map(static _ => unit));
        })
        from rendered in line.Caption.Filter(_ => line.Columned && !beside).Match(
            Some: label => Headed(label, table, attachments, retain),
            None: () => IO.pure<Control>(table))
        select (rendered, Callbacks.Each(toSeq(attachments)).Map(static _ => unit));

    private IO<Control> Headed(Control label, TableLayout table, List<IO<Unit>> attachments, Action<IDisposable> retain) =>
        from panel in DisposalOps.Acquire(IO.lift(static () => new Panel()), static panel => IO.lift(panel.Dispose), retain)
        from assigned in IO.lift(() => attachments.Add(IO.lift(() => panel.Content = label).Map(static _ => unit)))
        from stack in Stacked(Seq(new StackLayoutItem(panel) { HorizontalAlignment = HorizontalAlignment.Left }, new StackLayoutItem(table)), retain)
        select (Control)stack;

    private static IO<Seq<(TableCell Cell, IO<Unit> Attach)>> Fixed(Option<Control> control, Option<float> width, Action<IDisposable> retain) =>
        width.Traverse(held => DisposalOps.Acquire(IO.lift(static () => new Panel()), static panel => IO.lift(panel.Dispose), retain)
            .Bind(panel => IO.lift(() => {
                panel.Width = (int)MathF.Ceiling(held);
                return (Cell: new TableCell(panel), Attach: IO.lift(() => panel.Content = control.ValueUnsafe()).Map(static _ => unit));
            }))).As().Map(static cell => cell.ToSeq());

    private IO<StackLayout> Stacked(Seq<StackLayoutItem> items, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new StackLayout()), static stack => IO.lift(stack.Dispose), retain).Bind(stack => IO.lift(() => {
            stack.Orientation = Orientation.Vertical;
            stack.Spacing = metrics.Spacing.Height;
            stack.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            _ = items.Iter(stack.Items.Add);
            return stack;
        }));

    // --- [REFRESH]
    public IO<bool> Editing =>
        Callbacks.Each(places.Bind(static place => place.Cells.Edit.Map(static edit => edit.Editing).ToSeq().Add(place.Cells.NestedEditing)))
            .Map(static held => held.Exists(static editing => editing));

    public IO<Unit> Refresh(Seq<EntryKey> changed) => Refreshed(RuleIndex.Affected(index, changed));

    private IO<Unit> Attached =>
        from held in Registrations.TraverseM(static attach => attach).As()
        from laid in form.ValueIO.Bind(shaped => Laid(shaped, shaped))
        from formed in Formed
        from shown in Refreshed(places)
        select unit;

    private Seq<IO<Unit>> Registrations =>
        Seq(scope.Listen(Refresh, Retain),
            HostTheme.Changed.Inline(_ => Refreshed(places), Sink, Retain),
            Subscriptions.Host<EventArgs>(typeof(RowGrid), h => SizeChanged += h, h => SizeChanged -= h, nameof(SizeChanged)).Inline(_ => Formed, Sink, Retain))
        + places.Choose(place => place.Cells.Menu.Map(attach => attach(place.Label, place.Trail.Map(static trail => trail.Groups).IfNone(Seq<Seq<Command>>()), Refreshed(Seq(place)), Retain)))
        + places.Bind(static place => place.Sources).Distinct()
            .Filter(source => !places.Exists(place => place.Row.Bound.Exists(bound => bound == source)))
            .Map(Watched);

    private IO<Unit> Watched(RowSource source) =>
        from signals in source.Signals(scope)
        let site = new CallbackSite(Sink, typeof(RowGrid), nameof(Watched))
        let reading = places.Filter(place => place.Sources.Exists(read => read == source))
        from mailbox in Subscriptions.Idle<Unit, Unit>(_ => Refreshed(reading))(site, Retain)
        let post = fun((Unit raised) => mailbox((raised, raised)))
        from held in signals.TraverseM(signal => signal.Inline(post, Sink, Retain)).As()
        select unit;

    private void Retain(IDisposable held) => atomic(() => acquired.Value = acquired.Value.Add(held));

    private IO<Unit> Refreshed(Seq<RowPlace> targets) =>
        from parts in targets.Map<K<IO, (RowPlace Place, bool Shown)>>(place => Restated(place).Map(shown => (place, shown))).PartitionFallible().As()
        from finished in Callbacks.Each(Seq(
            Shaped(held => held with { Hidden = parts.Succs.Fold(held.Hidden, static (hidden, row) => row.Shown ? hidden.Remove(row.Place) : hidden.TryAdd(row.Place)) }),
            unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()))
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
        from reset in place.Trail.Filter(_ => modified).Traverse(static _ => Icons.Frames((GlyphRole.Reset, IconSlot.PanelButton.Master), IconSlot.PanelButton)).As()
        from padlock in place.Trail.Bind(static trail => trail.Lock).Traverse(_ => Icons.Frames((locked ? GlyphRole.LockClosed : GlyphRole.LockOpen, IconSlot.PanelButton.Master), IconSlot.PanelButton)).As()
        from painted in IO.lift(() => Painted(place, enabled, modified, locked, ink, reset, padlock))
        from available in place.Cells.Availability.Traverse(changed => changed(shown && enabled)).As()
        from showed in place.Cells.Show
        select shown;

    private Unit Painted(RowPlace place, bool enabled, bool modified, bool locked, PaintSlot ink, Option<Icon> reset, Option<Icon> padlock) {
        _ = place.Lines.Bind(static line => line.Controls).Iter(control => control.Enabled = enabled);
        _ = place.Label.Iter(label => (label.Font, label.TextColor) = (modified ? metrics.Emphasis : metrics.Regular, ink.Read()));
        _ = place.Trail.Iter(trail => (trail.Reset.Image, trail.Restore.Enabled, trail.Copy.Enabled, trail.Paste.Enabled) = (reset.ValueUnsafe(), enabled && modified, enabled, enabled && Clipboard.Instance.ContainsText));
        return place.Trail.Bind(static trail => trail.Lock).Iter(held => (held.Button.Image, held.Button.Enabled, held.Command.Enabled, held.Command.Checked) = (padlock.ValueUnsafe(), enabled, enabled, locked));
    }

    protected override void Dispose(bool disposing) {
        if (disposing)
            _ = IO.lift(() => { Content = null; })
                .Bind(_ => Callbacks.Each(places.Bind(static place => place.Parts).Distinct().Map(static part => IO.lift(part.Detach))))
                .Bind(_ => IO.lift(() => base.Dispose(disposing)))
                .Bind(_ => IO.lift(() => {
                    Seq<IDisposable> released = layouts;
                    layouts = [];
                    return atomic(() => {
                        Seq<IDisposable> held = acquired.Value;
                        acquired.Value = [];
                        return held;
                    }) + released;
                })).Bind(DisposalOps.Release).Uninterruptible().Run();
        else
            base.Dispose(disposing);
    }

    private sealed record GridLine(Option<Label> Caption, bool Columned, Option<Control> Gate, Control Value, Option<Control> Pick, Option<RowTrail> Trail, bool Trailing, bool Fills) {
        public Seq<Control> Controls => Caption.Map(static caption => (Control)caption).ToSeq() + Gate.ToSeq().Add(Value) + Pick.ToSeq();

        public static IO<GridLine> Of(RowLine line, Wording wording, IPlugInSink sink, Option<RowTrail> trail, Action<IDisposable> retain) =>
            line.Caption.Traverse(caption => DisposalOps.Acquire(IO.lift(() => RhinoLayout.NewLabel(wording.Shown(caption, sink))), static label => IO.lift(label.Dispose), retain)).As()
                .Map(caption => new GridLine(caption, line.Caption.IsSome, None, line.Value, line.Pick, trail, Trailing: true, Fills: false));
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
