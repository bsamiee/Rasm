using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;
using Rhino.Runtime;
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
        string.Join(Environment.NewLine, [
            wording.Shown(help, sink),
            .. Range.Map(static range => RowText.Localize("{0} – {1}", arguments: [range.Low, range.High]).Local).ToSeq(),
            .. Default.Map(static value => RowText.Localize("Default {0}", arguments: [value]).Local).ToSeq(),
        ]);
}

public sealed record RowLine(Option<string> Caption, Control Value, Option<Control> Pick, Option<FieldFit> Field);

public sealed record RowCells(
    Control Value, Option<Control> Gate, Option<Control> Pick, Option<FieldFit> Field, Seq<RowLine> Heads, Seq<RowLine> Lines, RowHelp Help, IO<Unit> Show,
    Option<RowEdit> Edit, Option<Func<Seq<Seq<Command>>, IO<Unit>, Action<IDisposable>, IO<ContextMenu>>> Menu) {
    public IO<bool> NestedEditing { get; init; } = IO.pure(value: false);
    public Option<ComponentControl> Plot { get; init; }
    public Option<Func<bool, IO<Unit>>> Availability { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RowGrid : Drawable {
    // --- [STATE]
    private const float LabelShare = 0.4f;

    private readonly RowScope scope;
    private readonly Seq<RowPlace> places;
    private readonly RowMetrics metrics;
    private readonly StackLayout stack;
    private readonly HashMap<EntryKey, Seq<RowPlace>> index;
    private readonly Atom<RowForm> form = Atom(RowForm.Initial);
    private readonly Atom<RowMarks> marks = Atom(RowMarks.Initial);
    private readonly Retained<IDisposable> acquired;

    private RowGrid(RowScope scope, Seq<RowPlace> places, RowMetrics metrics, StackLayout stack, Retained<IDisposable> acquired) {
        (Sink, this.scope, this.places, this.metrics, this.stack, this.acquired) = (scope.Sink, scope, places, metrics, stack, acquired);
        index = RuleIndex.Of(places.Map(static place => (place.Reads, place)));
        Content = stack;
    }

    public static string HandlerStyle { get; } = ComponentControl.StyleName(typeof(RowGrid));

    public IPlugInSink Sink { get; }

    // --- [REALIZE]
    public static IO<RowGrid> Realize(Seq<ControlRow> column, Seq<ControlRow> rows, RowScope scope, Action<IDisposable> retain) =>
        from acquired in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from grid in DisposalOps.OnFailure(
            from placed in rows.Map(static (row, at) => (Row: row, At: at)).TraverseM(row => Placed(row.At, row.Row, scope, acquired.Retain)).As()
            from metrics in Measured(column, placed, scope.Sink, acquired.Retain)
            from stack in DisposalOps.Acquire(IO.lift(() => new StackLayout {
                Orientation = Orientation.Vertical,
                Spacing = metrics.Spacing.Height,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }), acquired.Retain)
            from created in DisposalOps.Acquire(IO.lift(() => new RowGrid(scope, placed, metrics, stack, acquired)), retain)
            select created,
            IO.lift(acquired.Dispose))
        from attached in DisposalOps.OnFailure(IO.lift(() => grid.Style = HandlerStyle).Bind(_ => grid.Attached), IO.lift(grid.Dispose))
        select grid;

    public Option<string> Caption(Control value) =>
        places.Find(place => place.Cells.Exists(cells => ReferenceEquals(cells.Value, value))).Bind(static place => place.Caption);

    public Option<ComponentControl> Plot(Seq<EntryKey> keys) =>
        toHashSet(keys) switch { var selected => places.Choose(place => place.Cells.Bind(static cells => cells.Plot).Filter(_ => place.Row.Keys.Exists(selected.Contains))).Head };

    private static IO<RowPlace> Placed(int at, ControlRow row, RowScope scope, Action<IDisposable> retain) =>
        row.Switch<(RowScope Scope, Action<IDisposable> Retain), Option<IO<RowCells>>>(
                (scope, retain),
                field: static (held, field) => Some(field.Realize(held.Scope, held.Retain)),
                readout: static (held, readout) => Some(readout.Realize(held.Scope, held.Retain)),
                group: static (_, _) => None)
            .Traverse(static cells => cells).As()
            .Bind(cells => Lined(at, row, cells, scope, retain));

    private static IO<RowPlace> Lined(int at, ControlRow row, Option<RowCells> cells, RowScope scope, Action<IDisposable> retain) =>
        cells.Match(
            Some: held =>
                from tipped in IO.lift(() => held.Value.ToolTip = Helped(row, held, scope.Sink).ValueUnsafe())
                from first in (from keys in row.Keys.AsIterableNE() from source in row.Bound from edit in held.Edit select Trailed(source, keys, edit, scope, retain)).Traverse(static trailed => trailed).As()
                let caption = row.Layout.Filter(shape => shape.Labeled && (!shape.Spans || held.Heads.IsEmpty)).Map(_ => row.Wording.Shown(row.Caption, scope.Sink))
                let heads = held.Heads.Map((line, index) => Headed(line, row.Wording, scope.Sink, first.Filter(_ => index is 0)))
                let lines = row.Layout.Match(
                    Some: shape => shape.Spans
                        ? caption.Map(text => new LineSpec(Some(new LineCaption(text, CaptionCell.Span, Own: true)), Columned: false, held.Gate, None, held.Pick, first, Trailing: true, Fills: false)).ToSeq()
                          + heads
                          + Seq(new LineSpec(None, Columned: false, None, Some(held.Value), None, None, Trailing: false, shape.Fills))
                          + held.Lines.Map(line => Headed(line, row.Wording, scope.Sink, None))
                        : Seq(new LineSpec(caption.Map(static text => new LineCaption(text, CaptionCell.Column, Own: true)), Columned: true, held.Gate, Some(held.Value), held.Pick, first, Trailing: true, Fills: false)),
                    None: () => Seq(new LineSpec(None, Columned: false, None, Some(held.Value), None, None, Trailing: false, Fills: false)))
                from hosted in lines.TraverseM(spec => Hosted(spec, retain)).As()
                select new RowPlace(at, row, cells, caption, Helped(row, held, scope.Sink), hosted, first),
            None: () =>
                from hosted in Hosted(new LineSpec(Some(new LineCaption(row.Wording.Shown(row.Caption, scope.Sink), CaptionCell.Rule, Own: true)), Columned: false, None, None, None, None, Trailing: false, Fills: false), retain)
                select new RowPlace(at, row, None, None, row.Help.Map(text => row.Wording.Shown(text, scope.Sink)), Seq(hosted), None));

    private static Option<string> Helped(ControlRow row, RowCells cells, IPlugInSink sink) => row.Help.Map(text => cells.Help.Text(text, row.Wording, sink));

    private static LineSpec Headed(RowLine line, Wording wording, IPlugInSink sink, Option<RowTrail> trail) =>
        new(line.Caption.Map(caption => new LineCaption(wording.Shown(caption, sink), CaptionCell.Column, Own: false)), line.Caption.IsSome, None, Some(line.Value), line.Pick, trail, Trailing: true, Fills: false);

    private static IO<GridLine> Hosted(LineSpec spec, Action<IDisposable> retain) =>
        from host in DisposalOps.Acquire(IO.lift(static () => new Panel()), static held => IO.lift(() => { held.Content = null; held.Dispose(); }), retain)
        from content in spec.Gate.IsNone && spec.Pick.IsNone
            ? IO.pure(spec.Value)
            : DisposalOps.Acquire(IO.lift(() => new TableLayout(new TableRow(spec.Cells)) { Padding = 0, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) }),
                static held => IO.lift(() => { held.Rows.Clear(); held.Dispose(); }), retain).Map(static table => Some<Control>(table))
        from placed in IO.lift(() => host.Content = content.ValueUnsafe())
        select new GridLine(spec, host);

    private static IO<RowTrail> Trailed(RowSource source, IterableNE<EntryKey> keys, RowEdit edit, RowScope scope, Action<IDisposable> retain) =>
        from bound in IO.lift(() => new BindingGroup(Seq(source.Values(scope))))
        from restore in IO.lift(static () => new Command { MenuText = RowText.Localize("Reset").Local })
        from copy in IO.lift(static () => new Command { MenuText = RowText.Localize("Copy").Local })
        from paste in IO.lift(static () => new Command { MenuText = RowText.Localize("Paste").Local })
        let padlock = scope.Locks.Map(static _ => new CheckCommand { MenuText = RowText.Localize("Lock").Local })
        from held in (Seq((Command: restore, Run: edit.Reset), (Command: copy, Run: Transfer.Copy(bound, Some(keys), scope)), (Command: paste, Run: Transfer.Paste(bound, Some(keys), scope).Map(unit)))
                .Map(row => Subscriptions.Host<EventArgs>(typeof(Command), h => row.Command.Executed += h, h => row.Command.Executed -= h, nameof(Command.Executed)).Inline(_ => row.Run, scope.Sink, retain))
            + padlock.ToSeq().Map(command => Subscriptions.Host<EventArgs>(typeof(CheckCommand), h => command.Executed += h, h => command.Executed -= h, nameof(CheckCommand.Executed))
                .Inline(_ => scope.Lock(toSeq(keys), command.Checked), scope.Sink, retain)))
            .TraverseM(static attached => attached).As()
        select new RowTrail(keys, restore, copy, paste, padlock);

    // --- [FORM]
    private static IO<RowMetrics> Measured(Seq<ControlRow> column, Seq<RowPlace> places, IPlugInSink sink, Action<IDisposable> retain) =>
        from regular in DisposalOps.Acquire(IO.lift(static () => SystemFonts.Label()), retain)
        from emphasis in DisposalOps.Acquire(IO.lift(() => SystemFonts.Bold(regular.Size)), retain)
        let labels = column
            .Filter(static row => row.Layout.Exists(static shape => shape.Labeled && !shape.Spans))
            .Map(row => emphasis.MeasureString(row.Wording.Shown(row.Caption, sink)).Width)
            .Fold(0f, MathF.Max)
        let fields = places.Bind(static place => place.Cells.ToSeq()).Bind(static cells => cells.Field.ToSeq() + (cells.Heads + cells.Lines).Choose(static line => line.Field))
        let field = fields.Fold(0f, static (widest, fit) => MathF.Max(widest, fit.Width))
        from sized in IO.lift(() => fields.Iter(fit => fit.Field.Width = (int)MathF.Ceiling(field)))
        let lines = places.Bind(static place => place.Lines).Map(static line => line.Spec)
        from slot in IconSlot.PanelButton.Size()
        from widths in IO.lift(() => (
            Values: Widest(lines.Filter(static line => line.Columned).Choose(static line => line.Value)),
            Gate: Widest(lines.Choose(static line => line.Gate)),
            Pick: Widest(lines.Choose(static line => line.Pick))))
        from aligned in IO.lift(() => {
            _ = lines.Choose(static line => line.Gate).Iter(gate => gate.Width = (int)MathF.Ceiling(widths.Gate.IfNone(0f)));
            _ = lines.Choose(static line => line.Pick).Iter(pick => pick.Width = (int)MathF.Ceiling(widths.Pick.IfNone(0f)));
        })
        select new RowMetrics(
            MathF.Ceiling(labels),
            widths.Gate,
            widths.Pick,
            Some((float)slot).Filter(_ => places.Exists(static place => place.Trail.IsSome)),
            Some((float)slot).Filter(_ => places.Exists(static place => place.Trail.Exists(static trail => trail.Lock.IsSome))),
            widths.Values.IfNone(0f),
            RhinoLayout.Spacing(RhinoLayout.SpacingType.Table),
            MathF.Ceiling(regular.LineHeight),
            regular,
            emphasis);

    private static Option<float> Widest(Seq<Control> controls) =>
        controls.Map(static control => control.GetPreferredSize().Width).Fold(Option<float>.None, static (widest, width) => Some(MathF.Max(widest.IfNone(width), width)));

    private IO<Unit> Formed =>
        IO.lift(() => (float)Width).Bind(width => Shaped(held => held with {
            Beside = metrics.Labels <= LabelShare * width && metrics.Labels + metrics.Spacing.Width + metrics.Values + metrics.Trailing <= width,
        }));

    private IO<Unit> Shaped(Func<RowForm, RowForm> step) =>
        form.SwapIO(step).Bind(Laid);

    private IO<Unit> Laid(RowForm shaped) =>
        IO.lift(() => {
            Seq<GridLine> shown = places.Filter(place => !shaped.Hidden.Contains(place.At)).Bind(static place => place.Lines);
            if (!stack.Items.Select(static item => item.Control).SequenceEqual(shown.Map(static line => (Control)line.Host))) {
                stack.Items.Clear();
                _ = shown.Iter(line => stack.Items.Add(new StackLayoutItem(line.Host, expand: line.Spec.Fills)));
            }
            _ = places.Bind(static place => place.Lines).Iter(line => (line.Host.Padding, line.Host.MinimumSize) = metrics.Spaced(line.Spec, shaped.Beside));
            Invalidate();
        });

    // --- [REFRESH]
    public IO<bool> Editing =>
        Callbacks.Each(places.Bind(static place => place.Cells.ToSeq()).Bind(static cells => cells.Edit.Map(static edit => edit.Editing).ToSeq().Add(cells.NestedEditing)))
            .Map(static held => held.Exists(static editing => editing));

    public IO<Unit> Refresh(Seq<EntryKey> changed) => Refreshed(RuleIndex.Affected(index, changed));

    private IO<Unit> Attached =>
        from held in Registrations.TraverseM(static attach => attach).As()
        from laid in form.ValueIO.Bind(Laid)
        from formed in Formed
        from shown in Refreshed(places)
        select unit;

    private Seq<IO<Unit>> Registrations =>
        Seq(scope.Listen(Refresh, acquired.Retain),
            HostTheme.Changed.Inline(_ => Refreshed(places), Sink, acquired.Retain),
            Subscriptions.Host<EventArgs>(typeof(RowGrid), h => SizeChanged += h, h => SizeChanged -= h, nameof(SizeChanged)).Inline(_ => Formed, Sink, acquired.Retain),
            Subscriptions.Host<EventArgs>(typeof(StackLayout), h => stack.SizeChanged += h, h => stack.SizeChanged -= h, nameof(SizeChanged)).Inline(_ => IO.lift(Invalidate), Sink, acquired.Retain),
            Subscriptions.Host<AtomChangedEvent<RowMarks>, RowMarks>(typeof(Atom<RowMarks>), h => marks.Change += h, h => marks.Change -= h,
                static handler => held => handler(sender: null, held), nameof(marks.Change)).Inline(held => IO.lift(() => {
                    ToolTip = held.Hovered.Bind(Tip).ValueUnsafe();
                    Invalidate();
                }), Sink, acquired.Retain))
        + places.Choose(place => place.Cells.Bind(static cells => cells.Menu).Map(attach =>
            attach(place.Trail.Map(static trail => trail.Groups).IfNone(Seq<Seq<Command>>()), Refreshed(Seq(place)), acquired.Retain)
                .Bind(menu => marks.SwapIO(held => held with { Menus = held.Menus.AddOrUpdate(place.At, menu) })).Map(unit)))
        + places.Bind(static place => place.Sources).Distinct()
            .Filter(source => !places.Exists(place => place.Row.Bound.Exists(bound => bound == source)))
            .Map(Watched);

    private IO<Unit> Watched(RowSource source) =>
        from signals in source.Signals(scope)
        let site = new CallbackSite(Sink, typeof(RowGrid), nameof(Watched))
        let reading = places.Filter(place => place.Sources.Exists(read => read == source))
        from mailbox in Subscriptions.Idle<Unit, Unit>(_ => Refreshed(reading))(site, acquired.Retain)
        let post = fun((Unit raised) => mailbox((raised, raised)))
        from held in signals.TraverseM(signal => signal.Inline(post, Sink, acquired.Retain)).As()
        select unit;

    private IO<Unit> Refreshed(Seq<RowPlace> targets) =>
        from parts in targets.Map<K<IO, (RowPlace Place, bool Shown, RowPaint Paint)>>(place => Restated(place).Map(state => (place, state.Shown, state.Paint))).PartitionFallible().As()
        from painted in marks.SwapIO(held => held with { Paints = parts.Succs.Fold(held.Paints, static (paints, row) => paints.AddOrUpdate(row.Place.At, row.Paint)) })
        from finished in Callbacks.Each(Seq(
            Shaped(held => held with { Hidden = parts.Succs.Fold(held.Hidden, static (hidden, row) => row.Shown ? hidden.Remove(row.Place.At) : hidden.TryAdd(row.Place.At)) }),
            unless(parts.Fails.IsEmpty, IO.fail<Unit>(Error.Many(parts.Fails))).As()))
        select unit;

    private IO<(bool Shown, RowPaint Paint)> Restated(RowPlace place) =>
        from enabled in place.Row.Rules.Enables(scope)
        from shown in place.Row.Rules.Shows(scope)
        from modified in place.Cells.Bind(static cells => cells.Edit).Match(Some: static edit => edit.Modified, None: static () => IO.pure(value: false))
        from locked in place.Trail.Match(Some: trail => scope.Locked.Map(held => trail.Keys.ForAll(held.Contains)), None: static () => IO.pure(value: false))
        from painted in IO.lift(() => Activated(place, enabled, modified))
        from available in place.Cells.Bind(static cells => cells.Availability).Traverse(changed => changed(shown && enabled)).As()
        from showed in place.Cells.Match(Some: static cells => cells.Show, None: static () => IO.pure(unit))
        select (shown, new RowPaint(enabled, modified, locked));

    private static Unit Activated(RowPlace place, bool enabled, bool modified) {
        _ = place.Lines.Bind(static line => line.Spec.Controls).Iter(control => control.Enabled = enabled);
        return place.Trail.Iter(trail => {
            (trail.Restore.Enabled, trail.Copy.Enabled, trail.Paste.Enabled) = (enabled && modified, enabled, enabled && Clipboard.Instance.ContainsText);
            _ = trail.Lock.Iter(command => command.Enabled = enabled);
        });
    }

    // --- [PAINT]
    protected override void OnPaint(PaintEventArgs e) {
        Callbacks.Run(IO.lift(() => Painted(e.Graphics, marks.Value, form.Value.Beside)), new CallbackSite(Sink, typeof(RowGrid), nameof(OnPaint)));
        base.OnPaint(e);
    }

    private Unit Painted(Graphics graphics, RowMarks held, bool beside) =>
        Placements(beside).Iter(drawn => {
            RowPaint paint = held.Paints.Find(drawn.Place.At).IfNone(RowPaint.Plain);
            _ = drawn.Caption.Iter(caption => {
                Font font = caption.Text.Own && paint.Modified ? metrics.Emphasis : metrics.Regular;
                Color ink = (paint.Enabled, caption.Text.Own && paint.Modified) switch {
                    (false, _) => PaintSlot.ContentTextDisabled.Read(),
                    (true, true) => PaintSlot.ContentHighlight.Read(),
                    (true, false) => PaintSlot.ContentTextEnabled.Read(),
                };
                SizeF measured = font.MeasureString(caption.Text.Text);
                float x = caption.Text.Cell is CaptionCell.Column && beside && HostUtils.RunningOnOSX ? caption.Bounds.Right - measured.Width : caption.Bounds.X;
                PointF origin = new(x, caption.Bounds.Y + ((caption.Bounds.Height - measured.Height) / 2f));
                graphics.DrawText(font, ink, origin, caption.Text.Text);
                if (caption.Text.Cell is CaptionCell.Rule)
                    graphics.DrawLine(PaintSlot.FrameEdge.Read(), new PointF(origin.X + measured.Width + metrics.Spacing.Width, caption.Bounds.Center.Y), new PointF(caption.Bounds.Right, caption.Bounds.Center.Y));
            });
            _ = drawn.Reset.Filter(_ => paint.Enabled && paint.Modified).Iter(bounds => Glyph(graphics, held, new RowHit(drawn.Place.At, HitCell.Reset), GlyphRole.Reset, bounds));
            _ = drawn.Lock.Iter(bounds => Glyph(graphics, held, new RowHit(drawn.Place.At, HitCell.Lock), paint.Locked ? GlyphRole.LockClosed : GlyphRole.LockOpen, bounds));
        });

    private static Unit Glyph(Graphics graphics, RowMarks held, RowHit hit, GlyphRole glyph, RectangleF bounds) {
        if (held.Hovered == Some(hit) || held.Pressed == Some(hit))
            graphics.FillRectangle(new Color(PaintSlot.ControlText.Read(), held.Pressed == Some(hit) ? 0.25f : 0.12f), bounds);
        graphics.DrawImage(Icons.Frame(glyph, IconSlot.PanelButton.Master, new System.Drawing.Size((int)MathF.Ceiling(bounds.Width), (int)MathF.Ceiling(bounds.Height))), bounds);
        return unit;
    }

    private Seq<DrawnLine> Placements(bool beside) =>
        from place in places
        where !form.Value.Hidden.Contains(place.At)
        from line in place.Lines
        select metrics.Drawn(place, line, beside);

    // --- [POINTER]
    private Option<RowHit> Hit(PointF at) =>
        Placements(form.Value.Beside).Bind(drawn =>
            drawn.Caption.Filter(caption => caption.Bounds.Contains(at)).Map(_ => new RowHit(drawn.Place.At, HitCell.Caption)).ToSeq()
            + drawn.Reset.Filter(bounds => bounds.Contains(at) && marks.Value.Paints.Find(drawn.Place.At).Exists(static paint => paint.Enabled && paint.Modified)).Map(_ => new RowHit(drawn.Place.At, HitCell.Reset)).ToSeq()
            + drawn.Lock.Filter(bounds => bounds.Contains(at)).Map(_ => new RowHit(drawn.Place.At, HitCell.Lock)).ToSeq())
        .Head;

    private IO<Unit> Pointed(Func<RowMarks, RowMarks> step) =>
        marks.SwapMaybeIO(held => Some(step(held)).Filter(next => next.Hovered != held.Hovered || next.Pressed != held.Pressed)).Map(unit);

    private Option<string> Tip(RowHit hit) =>
        places.Find(place => place.At == hit.Place).Bind(place => hit.Cell switch {
            HitCell.Caption => place.Help,
            HitCell.Reset => Some(RowText.Localize("Reset").Local),
            HitCell.Lock => Some(RowText.Localize("Lock").Local),
        });

    protected override void OnMouseMove(MouseEventArgs e) {
        Callbacks.Run(Pointed(held => held with { Hovered = Hit(e.Location) }), new CallbackSite(Sink, typeof(RowGrid), nameof(OnMouseMove)));
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        Callbacks.Run(Pointed(static held => held with { Hovered = None, Pressed = None }), new CallbackSite(Sink, typeof(RowGrid), nameof(OnMouseLeave)));
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        Callbacks.Run(
            from hit in IO.lift(() => Hit(e.Location))
            from claimed in IO.lift(() => { e.Handled = hit.IsSome; })
            from acted in hit.Traverse(found => e.Buttons is MouseButtons.Alternate
                ? IO.lift(() => marks.Value.Menus.Find(found.Place).Iter(menu => menu.Show(this, e.Location)))
                : Pointed(held => held with { Pressed = Some(found).Filter(static pressed => pressed.Cell is not HitCell.Caption) })).As()
            select unit, new CallbackSite(Sink, typeof(RowGrid), nameof(OnMouseDown)));
        if (!e.Handled)
            base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        Callbacks.Run(
            from released in IO.lift(() => marks.Value.Pressed.Filter(pressed => Hit(e.Location) == Some(pressed)))
            from cleared in Pointed(static held => held with { Pressed = None })
            from acted in released.Traverse(Acted).As()
            select unit, new CallbackSite(Sink, typeof(RowGrid), nameof(OnMouseUp)));
        base.OnMouseUp(e);
    }

    private IO<Unit> Acted(RowHit hit) =>
        places.Find(place => place.At == hit.Place).Bind(static place => place.Trail).Match(
            Some: trail => hit.Cell switch {
                HitCell.Caption => IO.pure(unit),
                HitCell.Reset => IO.lift(trail.Restore.Execute),
                HitCell.Lock => scope.Locked.Bind(held => scope.Lock(toSeq(trail.Keys), !trail.Keys.ForAll(held.Contains))),
            },
            None: static () => IO.pure(unit));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try { if (disposing) acquired.Dispose(); } finally { base.Dispose(disposing); }
    }

    // --- [ROWS]
    private enum CaptionCell {
        Column,
        Span,
        Rule,
    }

    private enum HitCell {
        Caption,
        Reset,
        Lock,
    }

    private sealed record LineCaption(string Text, CaptionCell Cell, bool Own);

    private sealed record LineSpec(Option<LineCaption> Caption, bool Columned, Option<Control> Gate, Option<Control> Value, Option<Control> Pick, Option<RowTrail> Trail, bool Trailing, bool Fills) {
        public Seq<Control> Controls => Gate.ToSeq() + Value.ToSeq() + Pick.ToSeq();

        public Seq<TableCell> Cells =>
            Gate.Map(static gate => new TableCell(gate)).ToSeq()
                .Add(Value.Match(Some: static value => new TableCell(value, scaleWidth: true), None: static () => new TableCell { ScaleWidth = true }))
            + Pick.Map(static pick => new TableCell(pick)).ToSeq();
    }

    private sealed record GridLine(LineSpec Spec, Panel Host);

    private sealed record RowTrail(IterableNE<EntryKey> Keys, Command Restore, Command Copy, Command Paste, Option<CheckCommand> Lock) {
        public Seq<Seq<Command>> Groups => [[Restore, .. Lock.ToSeq()], [Copy, Paste]];
    }

    private sealed record RowPlace(int At, ControlRow Row, Option<RowCells> Cells, Option<string> Caption, Option<string> Help, Seq<GridLine> Lines, Option<RowTrail> Trail) {
        public Seq<EntryKey> Reads => Row.Keys + Row.Rules.Each.Bind(static rule => rule.Reads);

        public Seq<RowSource> Sources => Row.Rules.Each.Bind(static rule => rule.Sources);
    }

    private sealed record RowPaint(bool Enabled, bool Modified, bool Locked) {
        public static RowPaint Plain { get; } = new(Enabled: true, Modified: false, Locked: false);
    }

    private readonly record struct RowHit(int Place, HitCell Cell);

    private sealed record RowMarks(HashMap<int, RowPaint> Paints, HashMap<int, ContextMenu> Menus, Option<RowHit> Hovered, Option<RowHit> Pressed) {
        public static RowMarks Initial { get; } = new([], [], None, None);
    }

    private sealed record DrawnLine(RowPlace Place, Option<(LineCaption Text, RectangleF Bounds)> Caption, Option<RectangleF> Reset, Option<RectangleF> Lock);

    private sealed record RowForm(bool Beside, LanguageExt.HashSet<int> Hidden) {
        public static RowForm Initial { get; } = new(Beside: false, []);
    }

    private sealed record RowMetrics(
        float Labels, Option<float> Gate, Option<float> Pick, Option<float> Reset, Option<float> Lock, float Values, Size Spacing, float CaptionHeight, Font Regular, Font Emphasis) {
        public float Trailing => Seq(Gate, Pick, Reset, Lock).Somes().Fold(0f, (sum, width) => sum + width + Spacing.Width);

        private bool Gated(LineSpec line) => Gate.IsSome && (line.Columned || line.Gate.IsSome);

        public (Padding Padding, Size Minimum) Spaced(LineSpec line, bool beside) =>
            (new Padding(
                    (int)MathF.Ceiling((line.Columned && beside ? Labels + Spacing.Width : 0f) + (Gated(line) && line.Gate.IsNone ? Gate.IfNone(0f) + Spacing.Width : 0f)),
                    (int)MathF.Ceiling(line.Columned && !beside && line.Caption.IsSome ? CaptionHeight + Spacing.Height : 0f),
                    (int)MathF.Ceiling(line.Trailing ? Seq(Pick.Filter(_ => line.Pick.IsNone), Reset, Lock).Somes().Fold(0f, (sum, width) => sum + width + Spacing.Width) : 0f),
                    0),
                new Size(0, (int)MathF.Ceiling(MathF.Max(
                    line.Caption.IsSome && !(line.Columned && !beside) ? CaptionHeight : 0f,
                    line.Trailing ? Reset.IfNone(0f) : 0f))));

        public DrawnLine Drawn(RowPlace place, GridLine line, bool beside) =>
            (Bounds: (RectangleF)line.Host.Bounds, line.Host.Padding) switch {
                var (bounds, padding) => new RectangleF(bounds.X + padding.Left, bounds.Y + padding.Top, bounds.Width - padding.Horizontal, bounds.Height - padding.Vertical) switch {
                    var content => new DrawnLine(
                        place,
                        line.Spec.Caption.Map(caption => (caption, caption.Cell switch {
                            CaptionCell.Column when beside => new RectangleF(bounds.X, content.Y, Labels, content.Height),
                            CaptionCell.Column => new RectangleF(bounds.X, bounds.Y, bounds.Width, CaptionHeight),
                            CaptionCell.Span or CaptionCell.Rule => (Gated(line.Spec) && line.Spec.Gate.IsSome ? Gate.IfNone(0f) + Spacing.Width : 0f) switch {
                                var lead => new RectangleF(content.X + lead, content.Y,
                                    content.Width - lead - (line.Spec.Pick.IsSome ? Pick.IfNone(0f) + Spacing.Width : 0f), content.Height),
                            },
                        })),
                        Reset.Filter(_ => line.Spec.Trailing && line.Spec.Trail.IsSome)
                            .Map(width => new RectangleF(bounds.Right - Lock.Map(held => held + Spacing.Width).IfNone(0f) - width, content.Y + ((content.Height - width) / 2f), width, width)),
                        Lock.Filter(_ => line.Spec.Trailing && line.Spec.Trail.Exists(static trail => trail.Lock.IsSome))
                            .Map(width => new RectangleF(bounds.Right - width, content.Y + ((content.Height - width) / 2f), width, width))),
                },
            };
    }
}
