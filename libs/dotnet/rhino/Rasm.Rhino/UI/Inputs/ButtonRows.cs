using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ButtonRows {
    // --- [PRESSES]
    public static IO<Button> Push(Command command, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new Button()), static held => IO.lift(held.Dispose), owned)
        from faced in IO.lift(() => { (button.Text, button.ToolTip) = (command.ToolBarText, command.ToolTip); })
        from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, command, owned)
        select button, (_, lease) => retain(lease));

    public static IO<Button> Pick(Command command, (IGlyph Glyph, int Master) glyph, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in Push(command, owned)
        from sized in IO.lift(() => { (button.Text, button.MinimumSize) = ("", Size.Empty); })
        from themed in Icons.Themed(sink, glyph, IconSlot.PickButton, HostTheme.In(button), icon => button.Image = icon, owned)
        select button, (_, lease) => retain(lease));

    // --- [GLYPHS]
    public static IO<ImageButton> Image(Command command, (IGlyph Glyph, int Master) glyph, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new ImageButton()), static held => IO.lift(held.Dispose), owned)
        from faced in IO.lift(() => { (button.ToolTip, button.MaskImageWithBackgroundColorWhenDisabled) = (command.ToolTip, true); })
        from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, command, owned)
        from themed in Icons.Themed(sink, glyph, slot, HostTheme.In(button), icon => button.Image = icon, owned)
        select button, (_, lease) => retain(lease));

    public static IO<SegmentedButton> Split(
        CheckCommand toggle, Seq<(RadioCommand Command, (IGlyph Glyph, int Master) Glyph)> modes, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from menu in ChoiceRows.Menu([modes.Map(static mode => (MenuEntry)new MenuEntry.Item(mode.Command))], [], None, sink, owned)
        from button in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), static held => IO.lift(held.Dispose), owned)
        from pressed in DisposalOps.Acquire(IO.lift(static () => new ButtonSegmentedItem()), Released,
            held => owned(new Disposal<SegmentedItem>(held, static item => Released(item).Run())))
        from bound in DisposalOps.Change(() => pressed.Command, value => pressed.Command = value, toggle, owned)
        from dropdown in DisposalOps.Acquire(IO.lift(static () => new MenuSegmentedItem()), Released,
            held => owned(new Disposal<SegmentedItem>(held, static item => Released(item).Run())))
        from placed in IO.lift(() => {
            (pressed.ToolTip, dropdown.CanSelect, dropdown.Menu, button.SelectionMode) = (toggle.ToolTip, false, menu, SegmentedSelectionMode.Multiple);
            button.Items.AddRange([pressed, dropdown]);
        })
        let shown = IO.lift(() => modes.Find(static mode => mode.Command.Checked))
            .Bind(mode => mode.Traverse(checkedMode => Icons.Frames(checkedMode.Glyph, slot)).As())
            .Bind(icon => IO.lift(() => icon.Iter(frames => pressed.Image = frames)))
        from heard in Callbacks.Each(modes.Map(mode => Subscriptions.Attach(
            h => mode.Command.CheckedChanged += h, h => mode.Command.CheckedChanged -= h,
            Callbacks.Handler<EventArgs>(_ => shown, new CallbackSite(sink, typeof(RadioCommand), nameof(RadioCommand.CheckedChanged))), owned))
            .Add(HostTheme.In(button).Inline(_ => shown, sink, owned)))
        from painted in shown
        select button, (_, lease) => retain(lease));

    // --- [GROUPS]
    public static IO<AddRemoveButton> AddRemove(Command add, Command remove, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from pair in DisposalOps.Acquire(IO.lift(static () => new AddRemoveButton()), static held => IO.lift(held.Dispose), owned)
        from faced in IO.lift(() => { (pair.AddToolTip, pair.RemoveToolTip, pair.AddEnabled, pair.RemoveEnabled) = (add.ToolTip, remove.ToolTip, add.Enabled, remove.Enabled); })
        from added in DisposalOps.Change(() => pair.AddCommand, value => pair.AddCommand = value, add, owned)
        from removed in DisposalOps.Change(() => pair.RemoveCommand, value => pair.RemoveCommand = value, remove, owned)
        select pair, (_, lease) => retain(lease));

    public static IO<SegmentedButton> Modes(Seq<(RadioCommand Command, Option<(IGlyph Glyph, int Master)> Glyph)> modes, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), static held => IO.lift(held.Dispose), owned)
        from selected in IO.lift(() => strip.SelectionMode = SegmentedSelectionMode.Single)
        from segments in modes.TraverseM(mode => Segment(mode, strip, slot, sink, owned)).As()
        from added in IO.lift(() => strip.Items.AddRange(segments))
        select strip, (_, lease) => retain(lease));

    public static IO<RhinoButtonRow> Strip(Seq<Control> actions, Seq<Control> toggles, Option<Control> modes, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new RhinoButtonRow()), static held => IO.lift(held.Dispose), owned)
        from groups in Seq(actions, toggles, modes.ToSeq()).Filter(static cluster => !cluster.IsEmpty).TraverseM(cluster => Cluster(strip, cluster, owned)).As()
        select strip, (_, lease) => retain(lease));

    private static IO<SegmentedItem> Segment(
        (RadioCommand Command, Option<(IGlyph Glyph, int Master)> Glyph) mode, SegmentedButton strip, IconSlot slot, IPlugInSink sink, Action<IDisposable> owned) =>
        from segment in DisposalOps.Acquire(IO.lift(static () => new ButtonSegmentedItem()), Released,
            held => owned(new Disposal<SegmentedItem>(held, static item => Released(item).Run())))
        from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (mode.Glyph.Map(static _ => "").IfNone(mode.Command.ToolBarText), mode.Command.ToolTip); })
        from bound in DisposalOps.Change(() => segment.Command, value => segment.Command = value, mode.Command, owned)
        from themed in mode.Glyph.Traverse(glyph => Icons.Themed(sink, glyph, slot, HostTheme.In(strip), icon => segment.Image = icon, owned)).As()
        select (SegmentedItem)segment;

    private static IO<Unit> Cluster(RhinoButtonRow strip, Seq<Control> cluster, Action<IDisposable> owned) =>
        from divider in strip.Items.Count == 0
            ? IO.pure(Option<Divider>.None)
            : DisposalOps.Acquire(IO.lift(static () => new Divider()), static held => IO.lift(held.Dispose), owned).Map(static held => Some(held))
        from added in IO.lift(() => {
            _ = divider.Iter(held => strip.Items.Add(new StackLayoutItem(held, VerticalAlignment.Stretch)));
            _ = cluster.Iter(control => strip.Items.Add(new StackLayoutItem(control)));
        })
        select unit;

    // --- [MENUS]
    public static IO<SegmentedButton> Menu(ContextMenu menu, (IGlyph Glyph, int Master) glyph, IconSlot slot, string toolTip, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), static held => IO.lift(held.Dispose), owned)
        from segment in DisposalOps.Acquire(IO.lift(static () => new MenuSegmentedItem()), Released,
            held => owned(new Disposal<SegmentedItem>(held, static item => Released(item).Run())))
        from placed in IO.lift(() => {
            (segment.ToolTip, segment.CanSelect, segment.Menu, button.SelectionMode) = (toolTip, false, menu, SegmentedSelectionMode.None);
            button.Items.Add(segment);
        })
        from themed in Icons.Themed(sink, glyph, slot, HostTheme.In(button), icon => segment.Image = icon, owned)
        select button, (_, lease) => retain(lease));

    // --- [RELEASE]
    internal static IO<Unit> Released(SegmentedItem held) =>
        Callbacks.Each(Seq(IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(static _ => unit);
}
