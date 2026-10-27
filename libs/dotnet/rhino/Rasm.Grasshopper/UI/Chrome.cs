using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.UI;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.Skinning;
using Grasshopper2.UI.Toolbar;
using Grasshopper2.UI.Tooltip;
using Rasm.Grasshopper.UI.Surfaces;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.UI;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FloatingCommand(Command<RowRule, Invoke> Command, Either<FloatingPosition, PointF> Placement, Option<RowRule> Visible);

public sealed record CanvasBar(BarControl Control, IO<Unit> Refresh);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasChrome {
    // --- [TOOLBARS]
    private sealed record Projection(BarItem Item, IO<Unit> Refresh);

    public static IO<CanvasBar> Toolbar(Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        from projected in CommandRows.Localized(groups, scope.Owner).TraverseM(cluster => cluster.TraverseM(command => Projected(command, scope)).As().Map(static items => items.Flatten())).As()
        from bar in IO.lift(() => new Bar([.. projected
            .Map(static (cluster, index) => (index is 0 ? Seq<BarItem>() : Seq<BarItem>(Spacer.Default())).Concat(cluster.Map(static item => item.Item)))
            .Flatten()]) { Style = BarStyle.DefaultWithoutCategories })
        from control in use(() => new BarControl(bar))
        from refresh in CommandRows.Listened(projected.Flatten().Map(item => item.Refresh.Bind(_ => IO.lift(bar.Invalidate))), scope)
        select new CanvasBar(control, refresh);

    private static IO<Seq<Projection>> Projected(Command<RowRule, Invoke> command, RowScope scope) =>
        command.Switch(
            scope,
            run: static (at, run) => run.Marked.Match(
                    Some: mark => Toggled(run, run.Enabled, Some(mark), None, at),
                    None: () => CommandRows.Delivered(run, at).Bind(press => Pushed(run.Face, run.Enabled, press, Shortcut(run), at)))
                .Map(static item => Seq(item)),
            choice: static (at, choice) => choice.Members.ForAll(static member => member.Face.Glyph.IsSome)
                ? choice.Members.TraverseM(member => Toggled(member, CommandRows.Gate(choice.Enabled, member.Enabled), member.Marked, Some(choice.Face.Name), at)).As()
                : Dropped(choice, at).Map(static item => Seq(item)),
            menu: static (at, menu) => CommandRows.Menu(menu.Groups, at)
                .Bind(opened => Pushed(menu.Face, menu.Enabled, IO.lift(opened.Menu.Show), None, at))
                .Map(static item => Seq(item)));

    private static IO<Projection> Pushed(CommandFace face, Option<RowRule> gate, IO<Unit> press, Option<BarShortcut> key, RowScope scope) =>
        from icon in Icon(face)
        from button in IO.lift(() => new PushButton(icon.ValueUnsafe(), Named(face, None)) { KeyCode = key.ValueUnsafe() })
        from attached in Subscriptions.Host<EventHandler, EventArgs>(h => button.ButtonPressed += h, h => button.ButtonPressed -= h, static handler => handler.Invoke)
            .Inline(_ => press, scope.Owner)
        select new Projection(button, CommandRows.Holds(gate, scope).Bind(holds => IO.lift(() => { button.Enabled = holds; })));

    private static IO<Projection> Toggled(
        Command<RowRule, Invoke>.Run run, Option<RowRule> gate, Option<RowRule> mark, Option<string> section, RowScope scope) =>
        from press in CommandRows.Delivered(run, scope)
        from icon in Icon(run.Face)
        from toggle in IO.lift(() => new RadioToggle(icon.ValueUnsafe(), Named(run.Face, section), state: false) { KeyCode = Shortcut(run).ValueUnsafe(), Optional = section.IsNone })
        from quiet in IO.lift(static () => Atom(value: false))
        from attached in Subscriptions.During<bool>(h => toggle.StateChanged += h, h => toggle.StateChanged -= h)
            .Inline(state => when(!quiet.Value && (section.IsNone || state), press).As(), scope.Owner)
        select new Projection(toggle,
            from enabled in CommandRows.Holds(gate, scope)
            from held in mark.Traverse(rule => rule.Holds(scope)).As().Map(static holds => holds.IfNone(noneValue: false))
            from quieted in IO.lift(() => quiet.Swap(static _ => true))
            from shown in IO.lift(() => {
                toggle.Enabled = enabled;
                toggle.SetState(held);
            }).Finally(IO.lift(() => quiet.Swap(static _ => false)))
            select unit);

    private static IO<Projection> Dropped(Command<RowRule, Invoke>.Choice choice, RowScope scope) =>
        from presses in choice.Members.TraverseM(member => CommandRows.Delivered(member, scope)
            .Map(press => CommandRows.Holds(member.Enabled, scope).Bind(holds => when(holds, press).As()))).As()
        from drop in IO.lift(() => new DropDown<int>(Named(choice.Face, None)))
        from added in IO.lift(() => choice.Members.Iter((index, member) => drop.AddOption(index, Named(member.Face, None))))
        from attached in Subscriptions.Host<int>(h => drop.ValueChanged += h, h => drop.ValueChanged -= h).Inline(index => presses[index], scope.Owner)
        select new Projection(drop,
            from enabled in CommandRows.Holds(choice.Enabled, scope)
            from marks in choice.Members.TraverseM(member => member.Marked.Traverse(rule => rule.Holds(scope)).As()).As()
            from shown in IO.lift(() => {
                drop.Enabled = enabled;
                _ = marks.Map(static (held, index) => (On: held.IfNone(noneValue: false), Index: index)).Find(static held => held.On).Iter(held => drop.SetValue(held.Index));
            })
            select unit);

    // --- [MENUS]
    public static IO<Unit> Menu(FlexControl control, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope) =>
        from menus in use(static () => EnvIO.New(token: CancellationToken.None))
        from attached in GrasshopperEvents.PopulateContextMenu(control).Inline(args => Populated(args, groups, scope, menus), scope.Owner)
        select unit;

    private static IO<Unit> Populated(PopulateContextMenuEventArgs args, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, EnvIO menus) =>
        IO.lift(() => args.Control is not Canvas canvas
            || (args.IsMenu && canvas.ResolvePick(
                canvas.Map(args.MouseEvent.Location, CoordinateSystem.Control, CoordinateSystem.Content),
                includeGrips: false, includeForeground: true, includeBackground: true, includeWires: true, recursive: true).Kind is Pick.None))
            .Bind(joins => when(joins, Filled(args.Menu, groups, scope, menus)).As());

    private static IO<Unit> Filled(ContextMenu host, Seq<Seq<Command<RowRule, Invoke>>> groups, RowScope scope, EnvIO menus) =>
        IO.lift(static () => Ref(Option<Lease<Unit>>.None)).Bind(open => Products.Swapped(open,
            from filled in CommandRows.Fill(host.Items, groups, scope)
            from closed in Subscriptions.Host<EventArgs>(h => host.Closed += h, h => host.Closed -= h).Inline(_ => Products.Cleared(open).Under(menus), scope.Owner)
            select unit).Under(menus));

    // --- [FLOATING]
    public static IO<IO<Unit>> Floating(IFlexControl control, Seq<FloatingCommand> commands, RowScope scope) =>
        from added in commands.TraverseM(command => Floated(control, command, scope)).As()
        from refresh in CommandRows.Listened(added, scope)
        select refresh;

    private static IO<IO<Unit>> Floated(IFlexControl control, FloatingCommand command, RowScope scope) =>
        CommandRows.Localized(command.Command, scope.Owner) switch {
            var localized => localized.Switch(run: static run => run.Marked, choice: static _ => Option<RowRule>.None, menu: static _ => Option<RowRule>.None) switch {
                var mark =>
                    from press in CommandRows.Press(localized, scope)
                    from icon in Icon(localized.Face)
                    from button in use(IO.lift(() => {
                        FloatingButtonHandler click = (_, _) => scope.Owner.Answer(press.Map(static _ => Response.Handled), Response.Handled);
                        _ = command.Placement.Match(
                            Left: position => control.FloatingButtons.Add(position, localized.Face.Caption, Conversions.Unset(localized.Face.Help), Shade(selected: false), icon.ValueUnsafe(), click),
                            Right: anchor => control.FloatingButtons.AddAnchored(anchor, localized.Face.Caption, Conversions.Unset(localized.Face.Help), Shade(selected: false), icon.ValueUnsafe(), click));
                        return control.FloatingButtons[localized.Face.Caption];
                    }), static button => IO.lift(() => {
                        button.Enabled = false;
                        button.Close();
                    }))
                    select
                        from enabled in CommandRows.Holds(localized.Enabled, scope)
                        from visible in CommandRows.Holds(command.Visible, scope)
                        from marked in mark.Traverse(rule => rule.Holds(scope)).As()
                        from shown in IO.lift(() => {
                            button.Enabled = enabled;
                            button.ModifyColour(Shade(marked.IfNone(noneValue: false)));
                            if (visible) button.Show(); else button.Hide();
                            control.FloatingButtons.Control.ScheduleRedraw();
                        })
                        select unit,
            },
        };

    private static Color Shade(bool selected) =>
        Skin.Current.Shades[selected ? ShadeKind.NormalSelected : ShadeKind.Normal].Apex;

    // --- [TIPS]
    public static IO<Unit> Tip(
        IPlugInLifetime owner, IDocumentObject target, Either<Seq<(string Label, string Value)>, (Node Body, Size Size)> body, MessageLevel level = MessageLevel.None) =>
        IO.lift(() => body.Match(
            Left: lines => Frame.Show(target.Icon, target.Nomen.Name, target.Nomen.Info,
                [.. lines.Map(line => new LazyStrings(RowText.Localize(line.Label, owner).Local, line.Value))], level is MessageLevel.Warning, level is MessageLevel.Error),
            Right: drawn => Frame.Show(target.Icon, target.Nomen.Name, target.Nomen.Info,
                Guest.TooltipPainter(owner, drawn.Body), drawn.Size, level is MessageLevel.Warning, level is MessageLevel.Error)));

    // --- [FACES]
    private static IO<Option<IIcon>> Icon(CommandFace face) =>
        face.Glyph.Traverse(CanvasIcons.Vector).As();

    private static Nomen Named(CommandFace face, Option<string> section) =>
        new(face.Caption, Conversions.Unset(face.Help), section: section.ValueUnsafe());

    private static Option<BarShortcut> Shortcut(Command<RowRule, Invoke>.Run run) =>
        run.Invoke.Shortcut.Map(keys => new BarShortcut(keys, Conversions.Unset(run.Face.Help)));
}
