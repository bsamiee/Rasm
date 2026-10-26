using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Toolbar;
using Grasshopper2.UI.Tooltip;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FloatingCommand(Rasm.UI.Command<RowRule, Invoke> Command, Either<FloatingPosition, PointF> Placement, Option<RowRule> Visible);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasChrome {
    // --- [TOOLBARS]
    public static IO<(BarControl Control, IO<Unit> Refresh)> Toolbar(Seq<Seq<Rasm.UI.Command<RowRule, Invoke>>> groups, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from projected in CommandRows.Localized(groups, scope.Sink).TraverseM(cluster => cluster.TraverseM(command => Projected(command, scope, keep)).As().Map(static items => items.Flatten())).As()
            from bar in IO.lift(() => new Bar([.. projected
                .Map(static (cluster, index) => (index is 0 ? Seq<BarItem>() : Seq<BarItem>(Spacer.Default())).Concat(cluster.Map(static item => item.Item)))
                .Flatten()]) { Style = BarStyle.DefaultWithoutCategories })
            from control in DisposalOps.Acquire(IO.lift(() => new BarControl(bar)), keep)
            from refresh in CommandRows.Listened(projected.Flatten().Map(item => item.Watched with { Refresh = item.Watched.Refresh.Bind(_ => IO.lift(bar.Invalidate)) }), scope, keep)
            select (control, refresh), retain);

    private static IO<Seq<(BarItem Item, Watched Watched)>> Projected(Rasm.UI.Command<RowRule, Invoke> command, RowScope scope, Action<IDisposable> retain) =>
        command.Switch(
            (Scope: scope, Retain: retain),
            run: static (at, run) => run.Marked.Match(
                    Some: mark => Toggled(run, run.Enabled, Some(mark), None, at.Scope, at.Retain),
                    None: () => CommandRows.Delivered(run, at.Scope, at.Retain).Bind(press => Pushed(run.Face, run.Enabled, press, Shortcut(run), at.Scope, at.Retain)))
                .Map(static item => Seq(item)),
            choice: static (at, choice) => choice.Members.ForAll(static member => member.Face.Glyph.IsSome)
                ? choice.Members.TraverseM(member => Toggled(member, CommandRows.Gate(choice.Enabled, member.Enabled), member.Marked, Some(choice.Face.Name), at.Scope, at.Retain)).As()
                : Dropped(choice, at.Scope, at.Retain).Map(static item => Seq(item)),
            menu: static (at, menu) => CommandRows.Menu(menu.Groups, at.Scope, at.Retain)
                .Bind(opened => Pushed(menu.Face, menu.Enabled, IO.lift(opened.Menu.Show), None, at.Scope, at.Retain))
                .Map(static item => Seq(item)));

    private static IO<(BarItem Item, Watched Watched)> Pushed(
        Rasm.UI.CommandFace face, Option<RowRule> gate, IO<Unit> press, Option<BarShortcut> key, RowScope scope, Action<IDisposable> retain) =>
        from icon in Icon(face, scope.Sink)
        from button in IO.lift(() => new PushButton(icon.ValueUnsafe(), Named(face, None)) { KeyCode = key.ValueUnsafe() })
        from attached in Subscriptions.Attach<EventHandler>(
            handler => button.ButtonPressed += handler,
            handler => button.ButtonPressed -= handler,
            Callbacks.Handler<EventArgs>(_ => press, new CallbackSite(scope.Sink, typeof(PushButton), face.Name)).Invoke, retain)
        select ((BarItem)button, new Watched(gate.ToSeq(), RowRules.Holds(gate, scope).Bind(holds => IO.lift(() => { button.Enabled = holds; }))));

    private static IO<(BarItem Item, Watched Watched)> Toggled(
        Rasm.UI.Command<RowRule, Invoke>.Run run, Option<RowRule> gate, Option<RowRule> mark, Option<string> section, RowScope scope, Action<IDisposable> retain) =>
        from press in CommandRows.Delivered(run, scope, retain)
        from icon in Icon(run.Face, scope.Sink)
        from toggle in IO.lift(() => new RadioToggle(icon.ValueUnsafe(), Named(run.Face, section), state: false) { KeyCode = Shortcut(run).ValueUnsafe(), Optional = section.IsNone })
        let handler = Callbacks.Handler<bool>(state => when(section.IsNone || state, press).As(), new CallbackSite(scope.Sink, typeof(RadioToggle), run.Face.Name))
        from attached in Subscriptions.Attach(add => toggle.StateChanged += add, remove => toggle.StateChanged -= remove, handler, retain)
        select ((BarItem)toggle, new Watched(gate.ToSeq() + mark.ToSeq(),
            from enabled in RowRules.Holds(gate, scope)
            from held in mark.Traverse(rule => rule.Holds(scope)).As().Map(static holds => holds.IfNone(noneValue: false))
            from shown in Subscriptions.Detached(
                add => toggle.StateChanged += add,
                remove => toggle.StateChanged -= remove,
                handler,
                IO.lift(() => {
                    toggle.Enabled = enabled;
                    toggle.SetState(held);
                }))
            select unit));

    private static IO<(BarItem Item, Watched Watched)> Dropped(Rasm.UI.Command<RowRule, Invoke>.Choice choice, RowScope scope, Action<IDisposable> retain) =>
        from presses in choice.Members.TraverseM(member => CommandRows.Delivered(member, scope, retain)
            .Map(press => RowRules.Holds(member.Enabled, scope).Bind(holds => when(holds, press).As()))).As()
        from drop in IO.lift(() => new DropDown<int>(Named(choice.Face, None)))
        from added in IO.lift(() => choice.Members.Iter((index, member) => drop.AddOption(index, Named(member.Face, None))))
        from attached in Subscriptions.Attach(
            handler => drop.ValueChanged += handler,
            handler => drop.ValueChanged -= handler,
            Callbacks.Handler<int>(index => presses[index], new CallbackSite(scope.Sink, typeof(DropDown<int>), choice.Face.Name)), retain)
        select ((BarItem)drop, new Watched(choice.Enabled.ToSeq() + choice.Members.Bind(static member => member.Marked.ToSeq()),
            from enabled in RowRules.Holds(choice.Enabled, scope)
            from marks in choice.Members.TraverseM(member => member.Marked.Traverse(rule => rule.Holds(scope)).As()).As()
            from shown in IO.lift(() => {
                drop.Enabled = enabled;
                _ = marks.Map(static (held, index) => (On: held.IfNone(noneValue: false), Index: index)).Find(static held => held.On).Iter(held => drop.SetValue(held.Index));
            })
            select unit));

    // --- [FLOATING]
    public static IO<IO<Unit>> Floating(IFlexControl control, Seq<FloatingCommand> commands, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
            from added in commands.TraverseM(command => Floated(control, command, scope, keep)).As()
            from refresh in CommandRows.Listened(added, scope, keep)
            select refresh, retain);

    private static IO<Watched> Floated(IFlexControl control, FloatingCommand command, RowScope scope, Action<IDisposable> retain) =>
        CommandRows.Localized(command.Command, scope.Sink) switch {
            var localized => localized.Switch(run: static run => run.Marked, choice: static _ => Option<RowRule>.None, menu: static _ => Option<RowRule>.None) switch {
                var mark =>
                    from press in CommandRows.Press(localized, scope, retain)
                    from icon in Icon(localized.Face, scope.Sink)
                    from button in DisposalOps.Acquire(IO.lift(() => {
                        FloatingButtonHandler click = (_, _) => Callbacks.Answer(press.Map(static _ => Response.Handled), static () => Response.Handled,
                            new CallbackSite(scope.Sink, typeof(FloatingButton), localized.Face.Name));
                        _ = command.Placement.Match(
                            Left: position => control.FloatingButtons.Add(position, localized.Face.Caption, Conversions.Unset(localized.Face.Help), Shade(selected: false), icon.ValueUnsafe(), click),
                            Right: anchor => control.FloatingButtons.AddAnchored(anchor, localized.Face.Caption, Conversions.Unset(localized.Face.Help), Shade(selected: false), icon.ValueUnsafe(), click));
                        return control.FloatingButtons[localized.Face.Caption];
                    }), static button => IO.lift(() => {
                        button.Enabled = false;
                        button.Close();
                    }), retain)
                    select new Watched(localized.Enabled.ToSeq() + mark.ToSeq() + command.Visible.ToSeq(),
                        from enabled in RowRules.Holds(localized.Enabled, scope)
                        from visible in RowRules.Holds(command.Visible, scope)
                        from marked in mark.Traverse(rule => rule.Holds(scope)).As()
                        from shown in IO.lift(() => {
                            button.Enabled = enabled;
                            button.ModifyColour(Shade(marked.IfNone(noneValue: false)));
                            if (visible) button.Show(); else button.Hide();
                            control.FloatingButtons.Control.ScheduleRedraw();
                        })
                        select unit),
            },
        };

    private static Color Shade(bool selected) =>
        Skin.Current.Shades[selected ? ShadeKind.NormalSelected : ShadeKind.Normal].Apex;

    // --- [TIPS]
    public static IO<Unit> Tip(
        IPlugInSink sink, IDocumentObject owner, Either<Seq<(string Label, string Value)>, (Rasm.UI.Node Body, Size Size)> body, MessageLevel level = MessageLevel.None) =>
        IO.lift(() => body.Match(
            Left: lines => Frame.Show(owner.Icon, owner.Nomen.Name, owner.Nomen.Info,
                [.. lines.Map(line => new LazyStrings(RowText.Localize(line.Label, sink).Local, line.Value))], level is MessageLevel.Warning, level is MessageLevel.Error),
            Right: drawn => Frame.Show(owner.Icon, owner.Nomen.Name, owner.Nomen.Info,
                Guest.TooltipPainter(sink, drawn.Body, new CallbackSite(sink, typeof(Frame), nameof(Frame.Show))), drawn.Size, level is MessageLevel.Warning, level is MessageLevel.Error)));

    // --- [FACES]
    private static IO<Option<IIcon>> Icon(Rasm.UI.CommandFace face, IPlugInSink sink) =>
        face.Glyph.Traverse(glyph => CanvasIcons.Vector(sink, glyph)).As();

    private static Nomen Named(Rasm.UI.CommandFace face, Option<string> section) =>
        new(face.Caption, Conversions.Unset(face.Help), section: section.ValueUnsafe());

    private static Option<BarShortcut> Shortcut(Rasm.UI.Command<RowRule, Invoke>.Run run) =>
        run.Invoke.Shortcut.Map(keys => new BarShortcut(keys, Conversions.Unset(run.Face.Help)));
}
