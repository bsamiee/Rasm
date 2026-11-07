using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.UI;
using SuperLinq;
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
public sealed partial class ToolbarSide {
    public static readonly ToolbarSide Top = new("Top", 59419u);
    public static readonly ToolbarSide Left = new("Left", 59420u);
    public static readonly ToolbarSide Right = new("Right", 59421u);
    public static readonly ToolbarSide Bottom = new("Bottom", 59422u);

    public uint Site { get; }
}

[SmartEnum]
public sealed partial class MacroEntry {
    public static readonly MacroEntry Cancelling = new("! ");
    public static readonly MacroEntry Nested = new("'");
    public static readonly MacroEntry Input = new("");

    public string Prefix { get; }
}

[SmartEnum]
public sealed partial class ScriptWord {
    public static readonly ScriptWord Named = new(static value => $"_{value}");
    public static readonly ScriptWord Quoted = new(static value => $"""
        "{value}"
        """);

    [UseDelegateFromConstructor]
    public partial string Written(string value);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptValue {
    public string Text =>
        Switch(
            word: static held => held.Form.Written(held.Value),
            number: static held => held.Value.ToString("R", CultureInfo.InvariantCulture));

    public sealed record Word(ScriptWord Form, string Value) : ScriptValue;

    public sealed record Number(double Value) : ScriptValue;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptStep {
    public static ScriptStep Pause { get; } = new Pick(nameof(Pause));
    public static ScriptStep Enter { get; } = new Pick(nameof(Enter));

    public string Text =>
        Switch(
            pick: static held => $"_{held.Option}",
            assign: static held => $"_{held.Option}={held.Value.Text}",
            literal: static held => held.Value.Text);

    public Option<string> EnglishName =>
        Switch(
            pick: static held => Some(held.Option),
            assign: static held => Some(held.Option),
            literal: static _ => Option<string>.None);

    public sealed record Pick(string Option) : ScriptStep;

    public sealed record Assign(string Option, ScriptValue Value) : ScriptStep;

    public sealed record Literal(ScriptValue Value) : ScriptStep;
}

public sealed record Macro(string Command, Seq<ScriptStep> Steps) {
    public MacroEntry Entry { get; init; } = MacroEntry.Cancelling;
    public bool Scripted { get; init; }

    public string Text => string.Join(' ', Seq(Scripted ? $"{Entry.Prefix}_-{Command}" : $"{Entry.Prefix}_{Command}") + Steps.Map(static step => step.Text));
}

public sealed record ToolbarRow(CommandFace Face, Seq<Seq<Command<Macro>>> Groups);

public sealed record ToolbarTab(ToolbarRow Toolbar) {
    public Option<ToolbarGroupRow> Sidebar { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToolbarPlacement {
    private ToolbarPlacement(ToolbarSide side) => Side = side;

    public ToolbarSide Side { get; }

    public sealed record Docked(ToolbarSide Side) : ToolbarPlacement(Side);

    public sealed record Floating(ToolbarSide Side, Option<Rectangle> Bounds = default) : ToolbarPlacement(Side);
}

public sealed record ToolbarGroupRow(CommandFace Face, IterableNE<ToolbarTab> Tabs) {
    public ToolbarPlacement Placement { get; init; } = new ToolbarPlacement.Docked(ToolbarSide.Top);
    public bool Visible { get; init; } = true;
    public bool HideSingleTab { get; init; }
    public Option<ToolbarRow> Selected { get; init; }
}

public sealed record MenuExtension(Seq<string> Path, Seq<Seq<Command<Macro>>> Groups) {
    public RuiPlatforms Platforms { get; init; } = RuiPlatforms.Both;

    public string Text => string.Join("::", Path);
}

public sealed record MenuRow(CommandFace Face, Seq<Seq<Command<Macro>>> Groups);

public sealed record RuiFile {
    public Seq<ToolbarRow> Toolbars { get; init; }
    public Seq<ToolbarGroupRow> Groups { get; init; }
    public Seq<MenuExtension> Extensions { get; init; }
    public Seq<MenuRow> Menus { get; init; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MenuEntry {
    private MenuEntry(Guid id) => Id = id;

    public Guid Id { get; }

    public Seq<MenuEntry> Flattened =>
        toSeq(SuperEnumerable.TraverseDepthFirst(this, static entry => entry.Switch(separator: static _ => Seq<MenuEntry>(), item: static _ => Seq<MenuEntry>(), submenu: static held => held.Children)));

    public sealed record Separator(Guid Id) : MenuEntry(Id);

    public sealed record Item(Guid Menu, Guid Id, Command<Macro>.Run Run, Seq<RowRule> Gates, bool Radio) : MenuEntry(Id);

    public sealed record Submenu(Guid Id, Command<Macro>.Menu Menu, Seq<MenuEntry> Children) : MenuEntry(Id);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Toolbars {

    // --- [DECLARATIONS]

    public static Option<RuiFile> Declared(Assembly plugIn) =>
        toSeq(plugIn.GetExportedTypes()).Find(static type => type.IsAssignableTo(typeof(IRuiSource)))
            .Bind(static type => Optional(type.GetProperty(nameof(IRuiSource.Rui), BindingFlags.Public | BindingFlags.Static)?.GetValue(obj: null) as RuiFile));

    public static Fin<Guid> Identity(Assembly plugIn) =>
        Conversions.Required(Optional(plugIn.GetCustomAttribute<GuidAttribute>()).Bind(static held => parseGuid(held.Value)));

    // --- [IDENTIFIERS]

    public static Guid ExtensionsId(Guid file) => Uuid.NewNameBased(file, "extend_rhino_menus");

    public static Guid MenuId(Guid file, MenuRow menu) => Uuid.NewNameBased(file, $"menu/{menu.Face.Name}");

    public static Guid ItemId(Guid parent, string scope, int index) => Uuid.NewNameBased(parent, string.Create(CultureInfo.InvariantCulture, $"{scope}/{index}"));

    // --- [MENUS]

    public static Seq<MenuEntry> Entries(Guid file, MenuExtension extension) => Entries(ExtensionsId(file), extension.Text, extension.Groups, []);

    public static Seq<MenuEntry> Entries(Guid file, MenuRow menu) => Entries(MenuId(file, menu), "menu_item", menu.Groups, []);

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
                let items = toHashMap((declared.Extensions.Bind(extension => Entries(file, extension)) + declared.Menus.Bind(menu => Entries(file, menu))).Bind(static entry => entry.Flattened)
                    .Choose(static entry => Optional(entry as MenuEntry.Item)).Map(static item => (item.Id, item)))
                from live in use(static () => new CancellationTokenSource(), static source => Products.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit))
                let handler = Updated(items, live.Token)
                from registered in IO.lift(Products.Each(items.Values.ToSeq(), (item, index) => RhinoFault.Unless(RuiUpdateUi.RegisterMenuItem(file, item.Menu, item.Id, handler), RhinoReason.Declined, item.Id, index)))
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
