using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ButtonRows {
    // --- [PRESSES]
    public static IO<Button> Push(Command command, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new Button()), owned)
        from faced in IO.lift(() => { (button.Text, button.ToolTip) = (command.ToolBarText, command.ToolTip); })
        from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, command, owned)
        select button, retain);

    public static IO<Button> Pick(Command command, IGlyph glyph, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in Push(command, owned)
        from sized in IO.lift(() => { (button.Text, button.MinimumSize) = ("", Size.Empty); })
        from themed in Icons.Themed(sink, glyph, IconSlot.PickButton, HostTheme.In(button), icon => button.Image = icon, owned)
        select button, retain);

    // --- [GLYPHS]
    public static IO<ImageButton> Image(Command command, IGlyph glyph, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new ImageButton { MaskImageWithBackgroundColorWhenDisabled = true }), owned)
        from faced in IO.lift(() => { button.ToolTip = command.ToolTip; })
        from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, command, owned)
        from themed in Icons.Themed(sink, glyph, slot, HostTheme.In(button), icon => button.Image = icon, owned)
        select button, retain);

    public static IO<SegmentedButton> Split(
        CheckCommand toggle, Seq<(RadioCommand Command, IGlyph Glyph)> modes, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from menu in ChoiceRows.Menu([modes.Map(static mode => (MenuEntry)new MenuEntry.Item(mode.Command))], [], None, sink, owned)
        from button in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), owned)
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
        select button, retain);

    // --- [GROUPS]
    public static IO<AddRemoveButton> AddRemove(Command add, Command remove, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from pair in DisposalOps.Acquire(IO.lift(static () => new AddRemoveButton()), owned)
        from faced in IO.lift(() => { (pair.AddToolTip, pair.RemoveToolTip, pair.AddEnabled, pair.RemoveEnabled) = (add.ToolTip, remove.ToolTip, add.Enabled, remove.Enabled); })
        from added in DisposalOps.Change(() => pair.AddCommand, value => pair.AddCommand = value, add, owned)
        from removed in DisposalOps.Change(() => pair.RemoveCommand, value => pair.RemoveCommand = value, remove, owned)
        select pair, retain);

    public static IO<SegmentedButton> Modes(Seq<(RadioCommand Command, Option<IGlyph> Glyph)> modes, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from strip in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), owned)
        from selected in IO.lift(() => strip.SelectionMode = SegmentedSelectionMode.Single)
        from segments in modes.TraverseM(mode => Segment(mode, strip, slot, sink, owned)).As()
        from added in IO.lift(() => strip.Items.AddRange(segments))
        select strip, retain);

    public static IO<StackLayout> Strip(Seq<Seq<Control>> clusters, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from rows in clusters.Filter(static cluster => !cluster.IsEmpty).TraverseM(cluster =>
            from row in Containers.Frame(static () => new RhinoButtonRow { VerticalContentAlignment = VerticalAlignment.Center }, owned)
            from filled in IO.lift(() => cluster.Iter(control => row.Items.Add(control)))
            select row).As()
        from strip in Containers.Frame(() => new StackLayout([.. rows]) {
            Orientation = Orientation.Horizontal,
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Panel).Width,
            VerticalContentAlignment = VerticalAlignment.Center,
        }, owned)
        select strip, retain);

    private static IO<SegmentedItem> Segment(
        (RadioCommand Command, Option<IGlyph> Glyph) mode, SegmentedButton strip, IconSlot slot, IPlugInSink sink, Action<IDisposable> owned) =>
        from segment in DisposalOps.Acquire(IO.lift(static () => new ButtonSegmentedItem()), Released,
            held => owned(new Disposal<SegmentedItem>(held, static item => Released(item).Run())))
        from faced in IO.lift(() => { (segment.Text, segment.ToolTip) = (mode.Glyph.Map(static _ => "").IfNone(mode.Command.ToolBarText), mode.Command.ToolTip); })
        from bound in DisposalOps.Change(() => segment.Command, value => segment.Command = value, mode.Command, owned)
        from themed in mode.Glyph.Traverse(glyph => Icons.Themed(sink, glyph, slot, HostTheme.In(strip), icon => segment.Image = icon, owned)).As()
        select (SegmentedItem)segment;

    // --- [MENUS]
    public static IO<SegmentedButton> Menu(ContextMenu menu, IGlyph glyph, IconSlot slot, string toolTip, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new SegmentedButton()), owned)
        from segment in DisposalOps.Acquire(IO.lift(static () => new MenuSegmentedItem()), Released,
            held => owned(new Disposal<SegmentedItem>(held, static item => Released(item).Run())))
        from placed in IO.lift(() => {
            (segment.ToolTip, segment.CanSelect, segment.Menu, button.SelectionMode) = (toolTip, false, menu, SegmentedSelectionMode.None);
            button.Items.Add(segment);
        })
        from themed in Icons.Themed(sink, glyph, slot, HostTheme.In(button), icon => segment.Image = icon, owned)
        select button, retain);

    // --- [RELEASE]
    internal static IO<Unit> Released(SegmentedItem held) =>
        Callbacks.Each(Seq(IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit);
}
