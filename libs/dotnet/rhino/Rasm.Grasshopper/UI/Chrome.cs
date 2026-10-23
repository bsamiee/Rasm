using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Eto.Drawing;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Toolbar;
using Grasshopper2.UI.Tooltip;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FloatingCommand(CommandRow Command, Either<FloatingPosition, PointF> Placement, Option<RowRule> Visible);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasChrome {
    // --- [TOOLBARS]
    public static IO<(BarControl Control, IO<Unit> Refresh)> Toolbar(Seq<Seq<CommandRow>> groups, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
        from projected in groups.TraverseM(@group => @group.TraverseM(command => Project(command, scope)).As().Map(static entries => entries.Flatten())).As()
        from bar in IO.lift(() => new Bar([.. projected
            .Map(static (@group, index) => (index is 0 ? Seq<BarItem>() : Seq<BarItem>(Spacer.Default())).Concat(@group.Map(static entry => entry.Item)))
            .Flatten()]) { Style = BarStyle.DefaultWithoutCategories })
        from control in DisposalOps.Acquire(IO.lift(() => new BarControl(bar)), keep)
        from refreshes in projected.Flatten().TraverseM(entry => entry.Link(control, keep)).As()
        let refresh = Callbacks.Each(refreshes.Add(IO.lift(bar.Invalidate))).Map(unit)
        from shown in refresh
        select (Control: control, Refresh: refresh), retain);

    private sealed record BarEntry(BarItem Item, Func<BarControl, Action<IDisposable>, IO<IO<Unit>>> Link);

    private static IO<Seq<BarEntry>> Project(CommandRow command, RowScope scope) =>
        command.Switch(
            scope,
            run: static (scope, run) => Pressed(run, scope).Map(static entry => Seq(entry)),
            check: static (scope, check) => Toggle(check, None, scope).Map(static entry => Seq(entry)),
            radio: static (scope, radio) => radio.Members switch {
                var members => members.ForAll(static member => member.Face.Glyph.IsSome)
                    ? members.TraverseM(member => Toggle(member, Some(radio.Face.Name), scope)).As()
                    : Listed(radio, scope).Map(static entry => Seq(entry)),
            });

    private static IO<BarEntry> Pressed(CommandRow.Run run, RowScope scope) =>
        from icon in IconOf(run.Face, scope.Sink)
        from button in IO.lift(() => new PushButton(icon.ValueUnsafe(), Named(run.Face, None, scope.Sink)) { KeyCode = Shortcut(run.Face, scope.Sink) })
        select new BarEntry(
            button,
            (_, retain) => Subscriptions.Attach<EventHandler>(
                handler => button.ButtonPressed += handler,
                handler => button.ButtonPressed -= handler,
                Callbacks.Handler<EventArgs>(_ => run.Execute(scope), new CallbackSite(scope.Sink, typeof(PushButton), run.Face.Name)).Invoke, retain)
            .Map(_ => RowRules.Holds(run.Enabled, scope).Bind(enabled => IO.lift(() => {
                button.Enabled = enabled;
            }))));

    private static IO<BarEntry> Toggle(CommandRow.Check check, Option<string> section, RowScope scope) =>
        from icon in IconOf(check.Face, scope.Sink)
        from toggle in IO.lift(() => new RadioToggle(icon.ValueUnsafe(), Named(check.Face, section, scope.Sink), state: false) { KeyCode = Shortcut(check.Face, scope.Sink) })
        let handler = Callbacks.Handler<bool>(state => check.Write(scope, state), new CallbackSite(scope.Sink, typeof(RadioToggle), check.Face.Name))
        select new BarEntry(
            toggle,
            (_, retain) => Subscriptions.Attach(add => toggle.StateChanged += add, remove => toggle.StateChanged -= remove, handler, retain)
            .Map(_ =>
            from enabled in RowRules.Holds(check.Enabled, scope)
            from held in check.Read(scope)
            from shown in Subscriptions.Detached(
                add => toggle.StateChanged += add,
                remove => toggle.StateChanged -= remove,
                handler,
                IO.lift(() => {
                    toggle.Enabled = enabled;
                    toggle.SetState(held);
                }))
            select unit));

    private static IO<BarEntry> Listed(CommandRow.Radio radio, RowScope scope) =>
        from icon in IconOf(radio.Face, scope.Sink)
        from button in IO.lift(() => new PushButton(icon.ValueUnsafe(), Named(radio.Face, None, scope.Sink)) {
            Style = IconTextStyle.Text,
            KeyCode = Shortcut(radio.Face, scope.Sink),
        })
        select new BarEntry(
            button,
            (owner, retain) => from commands in radio.Realize(scope, retain)
                               from menu in ChoiceRows.Menu(Seq(commands.Commands.Map<MenuEntry>(static member => new MenuEntry.Item(member.Command))), Seq(commands.Refresh), None, scope.Sink, retain)
                               let site = new CallbackSite(scope.Sink, typeof(PushButton), radio.Face.Name)
                               from linked in Subscriptions.Attach<EventHandler>(
                                       handler => button.ButtonPressed += handler,
                                       handler => button.ButtonPressed -= handler,
                                       Callbacks.Handler<EventArgs>(_ => IO.lift(() => menu.Show(owner, new PointF(button.Bounds.Left, button.Bounds.Bottom))), site).Invoke, retain)
                               select commands.Refresh.Bind(_ => RowRules.Holds(radio.Enabled, scope)).Bind(enabled => IO.lift(() => { button.Enabled = enabled; })));

    // --- [FLOATING]
    public static IO<IO<Unit>> Floating(IFlexControl control, Seq<FloatingCommand> commands, RowScope scope, Action<IDisposable> retain) {
        Seq<(FloatingCommand Command, Seq<FloatingAction> Actions)> rows = commands.Map(command => (Command: command, Actions: Actions(command.Command, scope.Sink)));
        return DisposalOps.Acquire(keep =>
            from held in rows.TraverseM(row => row.Actions.TraverseM(action => Added(control, row, action, scope, keep)).As()).As()
            let refresh = Callbacks.Each(from row in rows from action in row.Actions select Shown(control.FloatingButtons, row.Command, action, scope)).Map(unit)
            from shown in refresh
            select refresh, retain);
    }

    private sealed record FloatingAction(string Name, CommandFace Face, Option<RowRule> Enabled, Func<RowScope, IO<Unit>> Execute, Option<Func<RowScope, IO<bool>>> Checked);

    private static Seq<FloatingAction> Actions(CommandRow command, IPlugInSink sink) =>
        command.Switch(
            sink,
            run: static (sink, run) => Seq(new FloatingAction(run.Face.Wording.Shown(run.Face.Caption, sink), run.Face, run.Enabled, run.Execute, None)),
            check: static (sink, check) => Seq(new FloatingAction(
                check.Face.Wording.Shown(check.Face.Caption, sink), check.Face, check.Enabled, scope => check.Read(scope).Bind(held => check.Write(scope, !held)), Some(check.Read))),
            radio: static (sink, radio) => radio.Members.Map(member => new FloatingAction(
                member.Face.Wording.Shown(member.Face.Caption, sink), member.Face, member.Enabled, scope => member.Write(scope, true), Some(member.Read))));

    private static IO<Unit> Added(IFlexControl control, (FloatingCommand Command, Seq<FloatingAction> Actions) row, FloatingAction action, RowScope scope, Action<IDisposable> retain) =>
        from icon in IconOf(action.Face, scope.Sink)
        from held in action.Checked.Traverse(read => read(scope)).As()
        from added in DisposalOps.Acquire(IO.lift(() => {
            string info = action.Face.Wording.Shown(action.Face.Help, scope.Sink);
            IIcon? glyph = icon.ValueUnsafe();
            FloatingButtonHandler click = (button, _) => Callbacks.Answer(
                scope, at => action.Execute(at)
                    .Bind(_ => Callbacks.Each(row.Actions.Map(member => Shown(button.Control.FloatingButtons, row.Command, member, scope))))
                    .Map(Response.Handled),
                static () => Response.Handled,
                new CallbackSite(scope.Sink, typeof(FloatingButton), action.Face.Name));
            _ = row.Command.Placement.Match(
                Left: position => control.FloatingButtons.Add(position, action.Name, info, Shade(held), glyph, click),
                Right: anchor => control.FloatingButtons.AddAnchored(anchor, action.Name, info, Shade(held), glyph, click));
            return new Disposal<FloatingButton>(control.FloatingButtons[action.Name], static button => {
                button.Enabled = false;
                button.Close();
            });
        }), retain)
        select unit;

    private static IO<Unit> Shown(FloatingButtonCollection buttons, FloatingCommand command, FloatingAction action, RowScope scope) =>
        from enabled in RowRules.Holds(action.Enabled, scope)
        from visible in RowRules.Holds(command.Visible, scope)
        from checkedState in action.Checked.Traverse(read => read(scope)).As()
        from shown in IO.lift(() => {
            FloatingButton button = buttons[action.Name];
            button.Enabled = enabled;
            button.ModifyColour(Shade(checkedState));
            if (visible) button.Show(); else button.Hide();
            buttons.Control.ScheduleRedraw();
        })
        select unit;

    private static Color Shade(Option<bool> held) =>
        Skin.Current.Shades[held.Exists(static on => on) ? ShadeKind.NormalSelected : ShadeKind.Normal].Apex;

    // --- [TIPS]
    public static IO<Unit> Tip(
        IPlugInSink sink, Option<IGlyph> glyph, string caption, string help, bool warnings, bool errors,
        Either<Seq<(string Label, string Value)>, (Action<Context, Rectangle> Painter, Size Size)> body) =>
        from icon in glyph.Traverse(shown => CanvasIcons.Vector(sink, shown)).As()
        let local = fun((string text) => RowText.Localize(text, sink).Local)
        from shown in IO.lift(() => body.Match(
            Left: lines => Frame.Show(icon.ValueUnsafe(), local(caption), local(help), [.. lines.Map(line => new LazyStrings(local(line.Label), line.Value))], warnings, errors),
            Right: drawn => Frame.Show(icon.ValueUnsafe(), local(caption), local(help), drawn.Painter, drawn.Size, warnings, errors)))
        select unit;

    public static Action<Context, Rectangle> Painter(Control owner, CallbackSite site, Func<PlotCanvas, RectangleF, IO<Unit>> marks) =>
        (context, details) => Callbacks.Run(
            from canvas in PlotCanvas.Of(context.Graphics, context.Bitmap is not null ? context.Zoom : owner.ParentWindow.LogicalPixelSize, Skin.Current.Shape.FontWord, enabled: true, focused: false)
            from well in Plots.Well(canvas, details)
            from drawn in marks(canvas, details)
            select unit,
            site);

    // --- [FACES]
    private static IO<Option<IIcon>> IconOf(CommandFace face, IPlugInSink sink) =>
        face.Glyph.Traverse(glyph => CanvasIcons.Vector(sink, glyph)).As();

    private static Nomen Named(CommandFace face, Option<string> section, IPlugInSink sink) =>
        new(face.Wording.Shown(face.Caption, sink), face.Wording.Shown(face.Help, sink), section: section.ValueUnsafe());

    private static BarShortcut? Shortcut(CommandFace face, IPlugInSink sink) =>
        face.Shortcut.Map(keys => new BarShortcut(keys, face.Wording.Shown(face.Help, sink))).ValueUnsafe();
}
