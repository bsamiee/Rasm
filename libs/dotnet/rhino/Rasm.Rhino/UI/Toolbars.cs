using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.UI;
using Riok.Mapperly.Abstractions;
using UUIDNext;

namespace Rasm.Rhino.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRuiSource {
    public static abstract RuiFile Rui { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ToolbarGroupState(Guid Id, Option<string> Name, bool Visible, bool IsDocked);

public sealed record ToolbarFileState(Guid Id, Option<string> Name, string Path, Seq<ToolbarGroupState> Groups, Seq<NamedId> Toolbars);

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ButtonFace {
    public static readonly ButtonFace Icon = new("control_only");
    public static readonly ButtonFace Text = new("text_only");
    public static readonly ButtonFace IconAndText = new("control_and_text");
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
}

public sealed record MenuExtension(Seq<string> Path, Seq<Seq<Command<Macro>>> Groups);

public sealed record RuiFile(Seq<ToolbarRow> Toolbars, Seq<MenuExtension> Menus);

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

public sealed record MenuPlacement(Guid Menu, string Path, Seq<MenuEntry> Entries);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ToolbarMapper {
    [MapProperty(nameof(ToolbarFile.Path), nameof(ToolbarFileState.Path), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(ToolbarFileState.Groups), Use = nameof(Groups))]
    [MapPropertyFromSource(nameof(ToolbarFileState.Toolbars), Use = nameof(Toolbars))]
    internal static partial ToolbarFileState ToState(ToolbarFile file);

    internal static partial ToolbarGroupState ToState(ToolbarGroup group);

    internal static partial NamedId ToState(Toolbar toolbar);

    [MapperIgnoreTarget(nameof(ToolbarRow.Display), Justification = "A linked toolbar shows icons alone")]
    internal static partial ToolbarRow ToRow(Command<Macro>.Menu menu);

    private static Seq<ToolbarGroupState> Groups(ToolbarFile file) =>
        Conversions.Rows(toSeq(Range(0, file.GroupCount)).Map(file.GetGroup)).Map(ToState).Strict();

    private static Seq<NamedId> Toolbars(ToolbarFile file) =>
        Conversions.Rows(toSeq(Range(0, file.ToolbarCount)).Map(file.GetToolbar)).Map(ToState).Strict();
}

public static class Toolbars {
    // --- [FILES]
    public static IO<Seq<ToolbarFileState>> Read() =>
        IO.lift(static () => Conversions.Rows(RhinoApp.ToolbarFiles).Map(ToolbarMapper.ToState).Strict());

    public static IO<ToolbarFileState> Find(Guid id) =>
        IO.lift(() => Conversions.Rows(RhinoApp.ToolbarFiles).Find(file => file.Id == id).Map(ToolbarMapper.ToState).ToFin(new RhinoFault(id.ToString("D", CultureInfo.InvariantCulture), RhinoReason.Missing)));

    // --- [DECLARATION]
    public static Option<RuiFile> Declared(Assembly plugIn) => Optional(plugIn.GetCustomAttribute<RuiAttribute>()).Map(static declared => declared.Rui);

    public static Fin<Guid> Identity(Assembly plugIn) =>
        Conversions.Found(Optional(plugIn.GetCustomAttribute<GuidAttribute>()).Bind(static held => parseGuid(held.Value)));

    public static Seq<ToolbarRow> Reached(Seq<ToolbarRow> toolbars) =>
        toolbars.Bind(static toolbar => toolbar.Cons(Reached(toolbar.Groups.Flatten().Bind(static command => command.Switch(
            run: static _ => Seq<ToolbarRow>(),
            choice: static _ => Seq<ToolbarRow>(),
            menu: static menu => Seq(ToolbarMapper.ToRow(menu)))))));

    // --- [MACROS]
    public static Seq<(Macro Macro, CommandFace Face)> Macros(Seq<Seq<Command<Macro>>> groups) =>
        groups.Flatten().Bind(static command => command.Switch(
            run: Invokes,
            choice: static choice => choice.Members.Bind(Invokes),
            menu: static menu => menu.Primary.ToSeq().Bind(Invokes) + Macros(menu.Groups)));

    private static Seq<(Macro Macro, CommandFace Face)> Invokes(Command<Macro>.Run run) =>
        (run.Invoke, run.Face).Cons(run.Alternate.Map(alternate => (alternate, Alternated(run.Face, run.Invoke, alternate))).ToSeq());

    private static CommandFace Alternated(CommandFace face, Macro primary, Macro alternate) =>
        alternate.Steps.Filter(step => primary.Steps.ForAll(held => held != step)).Choose(static step => step.EnglishName) switch {
            var names => face with { Caption = string.Join(", ", face.Caption.Cons(names)), Help = face.Help.Map(help => string.Join(", ", help.Cons(names))) },
        };

    // --- [IDENTIFIERS]
    public static Guid BarId(Guid file, ToolbarRow toolbar) => Uuid.NewNameBased(file, $"tool_bar/{toolbar.Face.Name}");

    public static Guid MacroId(Guid file, Macro macro) => Uuid.NewNameBased(file, $"macro_item/{macro.Text}");

    public static Guid IconId(Guid file, string resource) => Uuid.NewNameBased(file, $"icon/{resource}");

    public static Guid MenuId(Guid file) => Uuid.NewNameBased(file, "extend_rhino_menus");

    public static Guid ItemId(Guid parent, string scope, int index) => Uuid.NewNameBased(parent, string.Create(CultureInfo.InvariantCulture, $"{scope}/{index}"));

    // --- [MENUS]
    public static Seq<MenuPlacement> Extensions(Guid file, Seq<MenuExtension> menus) =>
        menus.Map(menu => string.Join("::", menu.Path) switch {
            var path => new MenuPlacement(MenuId(file), path, Entries(MenuId(file), path, menu.Groups, [])),
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
    public static readonly Func<IPlugInViews, IO<Unit>> Registered =
        static plugIn => Declared(plugIn.GetType().Assembly).Match(
            Some: declared =>
                from file in IO.lift(() => Identity(plugIn.GetType().Assembly))
                from live in use(static () => new CancellationTokenSource(), static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit))
                from registered in IO.lift(() => Callbacks.Each(
                    Extensions(file, declared.Menus).Bind(static placed => placed.Entries).Bind(static entry => entry.Flattened).Choose(static entry => Optional(entry as MenuEntry.Item)),
                    (item, index) => RhinoFault.Unless(RuiUpdateUi.RegisterMenuItem(file, item.Menu, item.Id, Handler(item, plugIn, live.Token)), index)))
                select unit,
            None: static () => IO.pure(unit));

    private static RuiUpdateUi.UpdateMenuItemEventHandler Handler(MenuEntry.Item item, IPlugInViews owner, CancellationToken live) =>
        (_, ui) => owner.Run(unless(live.IsCancellationRequested,
            from document in IO.lift(static () => Optional(RhinoDoc.ActiveDoc))
            from scope in RowScope.Open(owner, document, new CommitMode.Immediate())
            from holds in item.Gates.TraverseM(rule => rule.Holds(scope)).As()
            from marks in item.Run.Marked.Traverse(rule => rule.Holds(scope)).As()
            from written in IO.lift(() => {
                (ui.Enabled, ui.Text) = (holds.ForAll(static held => held), RowText.Localize(item.Run.Face.Caption, owner).Local);
                _ = marks.Iter(held => {
                    if (item.Radio) ui.RadioChecked = held;
                    else ui.Checked = held;
                });
            })
            select unit).As());
}
