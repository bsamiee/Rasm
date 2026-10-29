using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.UI;
using UUIDNext;

namespace Rasm.Rhino.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRuiSource {
    public static abstract RuiFile Rui { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ButtonFace {
    public static readonly ButtonFace Icon = new("control_only");
    public static readonly ButtonFace Text = new("text_only");
    public static readonly ButtonFace IconAndText = new("control_and_text");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuiPlatforms {
    public static readonly RuiPlatforms Both = new("Both");
    public static readonly RuiPlatforms Mac = new("Mac");
    public static readonly RuiPlatforms Windows = new("Win");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToolbarDock {
    public static readonly ToolbarDock Top = new("Top", floats: false);
    public static readonly ToolbarDock Left = new("Left", floats: false);
    public static readonly ToolbarDock Right = new("Right", floats: false);
    public static readonly ToolbarDock Bottom = new("Bottom", floats: false);
    public static readonly ToolbarDock Floating = new("Top", floats: true);

    public bool Floats { get; }
}

[Union]
public abstract partial record ScriptStep {
    public string Text => Switch(pick: static held => $"_{held.Option}", assign: static held => $"_{held.Option}=_{held.Value}", pause: static _ => "_Pause");

    public Option<string> EnglishName => Switch(pick: static held => Some(held.Option), assign: static held => Some(held.Option), pause: static _ => Option<string>.None);

    public sealed record Pick(string Option) : ScriptStep;

    public sealed record Assign(string Option, string Value) : ScriptStep;

    public sealed record Pause : ScriptStep;
}

public sealed record Macro(string Command, Seq<ScriptStep> Steps) {
    public string Text => string.Join(' ', Seq("!", $"_{Command}") + Steps.Map(static step => step.Text));
}

public sealed record ToolbarRow(CommandFace Face, Seq<Seq<Command<Macro>>> Groups) {
    public ButtonFace Display { get; init; } = ButtonFace.Icon;
    public RuiPlatforms Platforms { get; init; } = RuiPlatforms.Both;
}

public sealed record ToolbarGroupRow(CommandFace Face, IterableNE<ToolbarRow> Tabs) {
    public ToolbarDock Dock { get; init; } = ToolbarDock.Top;
    public bool Visible { get; init; } = true;
    public RuiPlatforms Platforms { get; init; } = RuiPlatforms.Both;
}

public sealed record MenuExtension(Seq<string> Path, Seq<Seq<Command<Macro>>> Groups) {
    public RuiPlatforms Platforms { get; init; } = RuiPlatforms.Both;
}

public sealed record MenuRow(CommandFace Face, Seq<Seq<Command<Macro>>> Groups) {
    public RuiPlatforms Platforms { get; init; } = RuiPlatforms.Both;
}

public sealed record RuiFile(Seq<ToolbarRow> Toolbars, Seq<MenuExtension> Extensions) {
    public Seq<ToolbarGroupRow> Groups { get; init; }
    public Seq<MenuRow> Menus { get; init; }
}

[AttributeUsage(AttributeTargets.Assembly)]
public abstract class RuiAttribute : Attribute {
    public abstract RuiFile Rui { get; }
}

public sealed class RuiFileAttribute<TSource> : RuiAttribute where TSource : IRuiSource {
    public override RuiFile Rui => TSource.Rui;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MenuEntry {
    private MenuEntry(Guid id) => Id = id;

    public Guid Id { get; }

    public Seq<MenuEntry> Flattened => this.Cons(Switch(separator: static _ => Seq<MenuEntry>(), item: static _ => Seq<MenuEntry>(), submenu: static held => held.Children.Bind(static child => child.Flattened)));

    public sealed record Separator(Guid Id) : MenuEntry(Id);

    public sealed record Item(Guid Menu, Guid Id, Command<Macro>.Run Run, Seq<RowRule> Gates, bool Radio) : MenuEntry(Id);

    public sealed record Submenu(Guid Id, Command<Macro>.Menu Menu, Seq<MenuEntry> Children) : MenuEntry(Id);
}

public sealed record MenuPlacement(Guid Menu, Option<string> Path, CommandFace Face, RuiPlatforms Platforms, Seq<MenuEntry> Entries);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Toolbars {
    // --- [DECLARATION]
    public static Option<RuiFile> Declared(Assembly plugIn) => Optional(plugIn.GetCustomAttribute<RuiAttribute>()).Map(static declared => declared.Rui);

    public static Fin<Guid> Identity(Assembly plugIn) =>
        Conversions.Required(Optional(plugIn.GetCustomAttribute<GuidAttribute>()).Bind(static held => parseGuid(held.Value)));

    // --- [IDENTIFIERS]
    public static Guid MenuId(Guid file, Option<string> caption) =>
        Uuid.NewNameBased(file, caption.Match(Some: static held => $"menu/{held}", None: static () => "extend_rhino_menus"));

    public static Guid ItemId(Guid parent, string scope, int index) => Uuid.NewNameBased(parent, string.Create(CultureInfo.InvariantCulture, $"{scope}/{index}"));

    // --- [MENUS]
    public static Seq<MenuPlacement> Placements(Guid file, RuiFile rui) =>
        rui.Extensions.Map(menu => string.Join("::", menu.Path) switch {
            var path => new MenuPlacement(MenuId(file, None), Some(path), new CommandFace(path, path), menu.Platforms, Entries(MenuId(file, None), path, menu.Groups, [])),
        })
            + rui.Menus.Map(menu => MenuId(file, Some(menu.Face.Name)) switch {
                var id => new MenuPlacement(id, None, menu.Face, menu.Platforms, Entries(id, "menu_item", menu.Groups, [])),
            });

    private static Seq<MenuEntry> Entries(Guid menu, string scope, Seq<Seq<Command<Macro>>> groups, Seq<RowRule> gates) =>
        groups.Map(group => group.Bind(command => command.Switch(
            (Menu: menu, Gates: gates + command.Enabled.ToSeq()),
            run: static (at, run) => Seq<Func<Guid, MenuEntry>>(id => new MenuEntry.Item(at.Menu, id, run, at.Gates, Radio: false)),
            choice: static (at, choice) => choice.Members.Map(member => (Func<Guid, MenuEntry>)(id => new MenuEntry.Item(at.Menu, id, member, at.Gates + member.Enabled.ToSeq(), Radio: true))),
            menu: static (at, sub) => Seq<Func<Guid, MenuEntry>>(id => new MenuEntry.Submenu(id, sub, Entries(id, "menu_item", sub.Groups, at.Gates))))))
            .Intersperse(Seq<Func<Guid, MenuEntry>>(static id => new MenuEntry.Separator(id)))
            .Flatten()
            .Map((entry, index) => entry(ItemId(menu, scope, index)));

    // --- [RUNTIME]
    internal static IO<Unit> Registered() =>
        Declared(Session.Assembly).Match(
            Some: static declared =>
                from file in IO.lift(static () => Identity(Session.Assembly))
                let items = toHashMap(Placements(file, declared).Bind(static placed => placed.Entries).Bind(static entry => entry.Flattened)
                    .Choose(static entry => Optional(entry as MenuEntry.Item)).Map(static item => (item.Id, item)))
                from live in use(static () => new CancellationTokenSource(), static source => Products.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit))
                let handler = Updated(items, live.Token)
                from registered in IO.lift(() => Products.Each(items.Values.ToSeq(), (item, index) => RhinoFault.Unless(RuiUpdateUi.RegisterMenuItem(file, item.Menu, item.Id, handler), RhinoReason.Declined, item.Id, index)))
                select unit,
            None: static () => IO.pure(unit));

    private static RuiUpdateUi.UpdateMenuItemEventHandler Updated(HashMap<Guid, MenuEntry.Item> items, CancellationToken live) =>
        (_, ui) => Session.Run(unless(live.IsCancellationRequested, items.Find(ui.MenuItemId).Traverse(item =>
            from scope in RowScope.Open(Optional(RhinoDoc.ActiveDoc), new CommitMode.Immediate())
            from holds in item.Gates.TraverseM(rule => rule.Holds(scope)).As()
            from marks in item.Run.Marked.Traverse(rule => rule.Holds(scope)).As()
            from written in IO.lift(() => {
                (ui.Enabled, ui.Text) = (holds.ForAll(static held => held), LocalText.Of(item.Run.Face.Caption).Local);
                _ = marks.Iter(held => {
                    if (item.Radio) ui.RadioChecked = held;
                    else ui.Checked = held;
                });
            })
            select unit).As().Map(static _ => unit)).As());

    // --- [HOST_FILES]
    public static IO<ToolbarFile> Own => IO.lift(static () => Identity(Session.Assembly)).Bind(File);

    public static IO<ToolbarFile> File(Guid id) =>
        IO.lift(() => Conversions.Rows(RhinoApp.ToolbarFiles).Find(file => file.Id == id).ToFin(new RhinoFault(id.ToString("D", CultureInfo.InvariantCulture), RhinoReason.Missing)));

    public static IO<Seq<ToolbarGroup>> Groups(ToolbarFile file) =>
        IO.lift(() => Conversions.Rows(toSeq(Range(0, file.GroupCount)).Map(file.GetGroup)).Strict());

    public static Preference<bool> Sidebar { get; } =
        Preferences.Property(static () => ToolbarFileCollection.SidebarIsVisible, static shown => ToolbarFileCollection.SidebarIsVisible = shown);
}
