using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ButtonRows {
    // --- [PRESSES]
    public static IO<Button> Push(Command command) =>
        IO.lift(() => new Button { Text = command.ToolBarText, ToolTip = command.ToolTip, Command = command });

    public static IO<(Button Control, IDisposable Release)> Pick(Command command, IGlyph glyph, IPlugInSink sink) =>
        from pick in IO.lift(() => new Button { MinimumSize = Size.Empty, ToolTip = command.ToolTip, Command = command })
        from themed in Icons.Themed(sink, glyph, IconSlot.PickButton, icon => pick.Image = icon)
        select (pick, themed);

    public static IO<(Button Control, IDisposable Release)> Pick(StageInput stage, string caption, Func<Vector4, IO<Unit>> picked, RowScope scope) =>
        Started(stage, caption, PickKind.Value, (picker, pixel) =>
            from square in picker.Sample(scope)
            from frame in stage.Read
            from done in frame.Bind(held => square.Mean(held, pixel)).Match(Some: picked, None: static () => IO.pure(unit))
            select done, scope);

    public static IO<(Button Control, IDisposable Release)> Place(
        StageInput stage, string caption, Func<System.Drawing.Point, PixelExtent, IO<Unit>> placed, RowScope scope) =>
        Started(stage, caption, PickKind.Position, (_, pixel) =>
            stage.Read.Bind(frame => frame.Match(Some: held => placed(pixel, held.Extent), None: static () => IO.pure(unit))), scope);

    private static IO<(Button Control, IDisposable Release)> Started(
        StageInput stage, string caption, PickKind kind, Func<FramePicker, System.Drawing.Point, IO<Unit>> continuation, RowScope scope) =>
        from command in IO.lift(() => new Command { ToolTip = RowText.Localize(caption).Local, Enabled = stage.Pick.IsSome })
        let site = new CallbackSite(scope.Sink, typeof(Command), nameof(Command.Executed))
        from executed in DisposalOps.AcquireAll(
            stage.Pick.ToSeq().Map(picker => Subscriptions.Attach<EventHandler<EventArgs>>(
                h => command.Executed += h, h => command.Executed -= h,
                Callbacks.Handler<EventArgs>(
                    _ => IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
                        .Bind(document => picker.Start(document, kind, pixel => continuation(picker, pixel))),
                    site))),
            DisposalOps.Release)
        from button in DisposalOps.OnFailure(Pick(command, GlyphRole.Pick, scope.Sink), DisposalOps.Release(executed))
        select (button.Control, DisposalOps.Composite(executed.Add(button.Release), site));

    // --- [GLYPHS]
    public static IO<(ImageButton Control, IDisposable Release)> Image(Command command, IGlyph glyph, IconSlot slot, IPlugInSink sink) =>
        from button in IO.lift(() => new ImageButton { ToolTip = command.ToolTip, MaskImageWithBackgroundColorWhenDisabled = true, Command = command })
        let bound = new Disposal<ImageButton>(button, static held => held.Command = null)
        from themed in DisposalOps.OnFailure(Icons.Themed(sink, glyph, slot, icon => button.Image = icon), IO.lift(bound.Dispose))
        select (button, DisposalOps.Composite(Seq<IDisposable>(bound, themed), new CallbackSite(sink, typeof(ImageButton), nameof(ImageButton.Command))));

    public static IO<(SegmentedButton Control, IDisposable Release)> Split(
        CheckCommand toggle, Seq<(RadioCommand Command, IGlyph Glyph)> modes, IconSlot slot, IPlugInSink sink) =>
        from menu in ChoiceRows.Menu([modes.Map(static mode => (MenuEntry)mode.Command)], [], None, sink)
        from pressed in IO.lift(() => new ButtonSegmentedItem { ToolTip = toggle.ToolTip, Command = toggle })
        let site = new CallbackSite(sink, typeof(RadioCommand), nameof(RadioCommand.CheckedChanged))
        let held = Seq(menu.Release)
        let shown = IO.lift(() => modes.Find(static mode => mode.Command.Checked))
            .Bind(mode => mode.Traverse(checkedMode => Icons.Frames(sink, checkedMode.Glyph, slot)).As())
            .Bind(icon => IO.lift(() => icon.Iter(frames => pressed.Image = frames)))
        from heard in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                modes.Map(mode => Subscriptions.Attach<EventHandler<EventArgs>>(
                        h => mode.Command.CheckedChanged += h, h => mode.Command.CheckedChanged -= h, Callbacks.Handler<EventArgs>(_ => shown, site)))
                    .Add(Themes.Changed.Inline(_ => shown, sink)),
                DisposalOps.Release),
            DisposalOps.Release(held))
        from painted in DisposalOps.OnFailure(shown, DisposalOps.Release(held + heard))
        from button in IO.lift(() => new SegmentedButton {
            SelectionMode = SegmentedSelectionMode.Multiple,
            Items = { pressed, new MenuSegmentedItem { CanSelect = false, Menu = menu.Menu } },
        })
        select (button, DisposalOps.Composite(held + heard, site));

    // --- [GROUPS]
    public static IO<(AddRemoveButton Control, IDisposable Release)> AddRemove(Command add, Command remove) =>
        IO.lift(() => new AddRemoveButton {
                AddToolTip = add.ToolTip, RemoveToolTip = remove.ToolTip,
                AddCommand = add, RemoveCommand = remove,
                AddEnabled = add.Enabled, RemoveEnabled = remove.Enabled,
            })
            .Map(static pair => (pair, (IDisposable)new Disposal<AddRemoveButton>(pair, static held => (held.AddCommand, held.RemoveCommand) = (null, null))));

    public static IO<(SegmentedButton Control, IDisposable Release)> Modes(Seq<(RadioCommand Command, Option<IGlyph> Glyph)> modes, IconSlot slot, IPlugInSink sink) =>
        from segments in IO.lift(() => modes.Map(static mode => (mode.Glyph, Segment: new ButtonSegmentedItem {
            Text = mode.Glyph.Map(static _ => string.Empty).IfNone(mode.Command.ToolBarText), ToolTip = mode.Command.ToolTip, Command = mode.Command,
        })).Strict())
        from strip in IO.lift(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.Single })
        from added in IO.lift(() => strip.Items.AddRange(segments.Map(static held => (SegmentedItem)held.Segment)))
        from themed in DisposalOps.AcquireAll(
            segments.Bind(held => held.Glyph.Map(glyph => Icons.Themed(sink, glyph, slot, icon => held.Segment.Image = icon)).ToSeq()), DisposalOps.Release)
        select (strip, DisposalOps.Composite(themed, new CallbackSite(sink, typeof(SegmentedItem), nameof(SegmentedItem.Image))));

    public static IO<RhinoButtonRow> Strip(Seq<Control> actions, Seq<Control> toggles, Option<Control> modes) =>
        from strip in IO.lift(static () => new RhinoButtonRow())
        from added in IO.lift(() => Seq(actions, toggles, modes.ToSeq())
            .Filter(static group => !group.IsEmpty)
            .Fold(Seq<StackLayoutItem>(), static (items, group) =>
                (items.IsEmpty ? items : items.Add(new StackLayoutItem(new Divider(), VerticalAlignment.Stretch))) + group.Map(static control => new StackLayoutItem(control)))
            .Iter(strip.Items.Add))
        select strip;

    // --- [MENUS]
    public static IO<(SegmentedButton Control, IDisposable Release)> Menu(ContextMenu menu, IGlyph glyph, IconSlot slot, string toolTip, IPlugInSink sink) =>
        from segment in IO.lift(() => new MenuSegmentedItem { ToolTip = toolTip, CanSelect = false, Menu = menu })
        from button in IO.lift(() => new SegmentedButton { SelectionMode = SegmentedSelectionMode.None, Items = { segment } })
        from themed in Icons.Themed(sink, glyph, slot, icon => segment.Image = icon)
        select (button, themed);
}
