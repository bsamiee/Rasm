using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuiPlatform {
    public static readonly RuiPlatform Win = new("Win");
    public static readonly RuiPlatform Mac = new("Mac");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ButtonFace {
    public static readonly ButtonFace Icon = new("control_only");
    public static readonly ButtonFace Text = new("text_only");
    public static readonly ButtonFace IconAndText = new("control_and_text");
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlyoutStyle {
    public static readonly FlyoutStyle Normal = new("normal");
    public static readonly FlyoutStyle FloatToTop = new("float_to_top");
}

[Union]
public abstract partial record ScriptStep {
    public string Text => Switch(pick: static held => $"_{held.Option}", assign: static held => $"_{held.Option}=_{held.Value}", pause: static _ => "_Pause");

    public sealed record Pick(string Option) : ScriptStep;

    public sealed record Assign(string Option, string Value) : ScriptStep;

    public sealed record Pause : ScriptStep;
}

public sealed record CommandScript(string Command, Seq<ScriptStep> Steps) {
    public string Text => string.Join(' ', Seq("!", $"_{Command}") + Steps.Map(static step => step.Text));
}

public sealed record Macro(CommandScript Script, CommandFace Face);

public sealed record Flyout(ToolbarRow Target, FlyoutStyle Style);

[Union]
public abstract partial record ToolbarItemRow {
    public sealed record Button(Macro Left) : ToolbarItemRow {
        public Option<Macro> Right { get; init; }
        public Option<Flyout> Flyout { get; init; }
        public Option<RuiPlatform> Platform { get; init; }
    }

    public sealed record Separator : ToolbarItemRow;
}

public sealed record ToolbarRow(string Caption, IGlyph Icon, Seq<ToolbarItemRow> Items) {
    public ButtonFace Face { get; init; } = ButtonFace.Icon;
}

[Union]
public abstract partial record MenuItemRow {
    public Option<RuiPlatform> Platform { get; init; }

    public Option<Macro> Macro =>
        Switch(
            run: static held => Some(held.Command),
            check: static held => Some(held.Command),
            radio: static held => Some(held.Command),
            separator: static _ => Option<Macro>.None,
            submenu: static _ => Option<Macro>.None);

    public sealed record Run(Macro Command) : MenuItemRow;

    public sealed record Check(Macro Command, Option<RowRule> Enabled, Func<RowScope, IO<bool>> Read) : MenuItemRow;

    public sealed record Radio(Macro Command, Option<RowRule> Enabled, Func<RowScope, IO<bool>> Read) : MenuItemRow;

    public sealed record Separator : MenuItemRow;

    public sealed record Submenu(string Caption, Seq<MenuItemRow> Entries) : MenuItemRow;
}

public sealed record MenuExtension(Seq<string> Path, Seq<MenuItemRow> Entries);

public sealed record RuiFile(Seq<ToolbarRow> Toolbars, Seq<MenuExtension> Menus);

[AttributeUsage(AttributeTargets.Assembly)]
public abstract class RuiFileAttribute : Attribute {
    public abstract RuiFile Rui { get; }
}

public sealed class RuiFileAttribute<TSource> : RuiFileAttribute where TSource : IRuiSource {
    public override RuiFile Rui => TSource.Rui;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ToolbarMapper {
    [MapProperty(nameof(ToolbarFile.Path), nameof(ToolbarFileState.Path), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(ToolbarFileState.Groups), Use = nameof(Groups))]
    [MapPropertyFromSource(nameof(ToolbarFileState.Toolbars), Use = nameof(Toolbars))]
    internal static partial ToolbarFileState ToState(ToolbarFile file);

    internal static partial ToolbarGroupState ToState(ToolbarGroup group);

    internal static partial NamedId ToState(Toolbar toolbar);

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
        IO.lift(() => Conversions.Rows(RhinoApp.ToolbarFiles).Find(file => file.Id == id).Map(ToolbarMapper.ToState).ToFin(new Refused(nameof(RhinoApp.ToolbarFiles), Refused.Absent)));

    // --- [DOCUMENT]
    private const int English = 1033;
    private const string DarkVariant = "_darkmode";

    public static Fin<Option<XDocument>> Document(Assembly plugIn) =>
        Optional(plugIn.GetCustomAttribute<RuiFileAttribute>()).Traverse(declared =>
            from file in Identity(plugIn)
            let toolbars = Reached(declared.Rui.Toolbars)
            let macros = toSeq((toolbars.Bind(static toolbar => toolbar.Items).Bind(static entry => entry.Switch(
                        button: static button => button.Left.Cons(button.Right.ToSeq()),
                        separator: static _ => Seq<Macro>()))
                    + declared.Rui.Menus.Bind(static menu => Flattened(menu.Entries)).Choose(static entry => entry.Macro))
                .DistinctBy(macro => MacroId(file, macro.Script)))
            from icons in toSeq((toolbars.Map(static toolbar => toolbar.Icon) + macros.Choose(static macro => macro.Face.Glyph)).DistinctBy(static glyph => glyph.Key, StringComparer.Ordinal))
                .Traverse(glyph => Icon(file, glyph)).As()
            select new XDocument(new XElement("RhinoUI",
                new XAttribute("major_ver", 5), new XAttribute("minor_ver", 0), new XAttribute("guid", file), new XAttribute("plug_in_guid", file),
                new XAttribute("localize", bool.FalseString), new XAttribute("default_language_id", English),
                new XElement("extend_rhino_menus", declared.Rui.Menus.IsEmpty ? Seq<XElement>() : Seq(Extended(file, plugIn, declared.Rui.Menus))),
                new XElement("menus"),
                new XElement("tool_bar_groups"),
                new XElement("tool_bars", toolbars.Map(toolbar => Bar(file, toolbar))),
                new XElement("macros", macros.Map(macro => MacroItem(file, macro))),
                new XElement("icons", icons),
                new XElement("bitmaps")))).As();

    private static Fin<Guid> Identity(Assembly plugIn) =>
        Optional(plugIn.GetCustomAttribute<GuidAttribute>()).Bind(static held => parseGuid(held.Value))
            .ToFin(new NotFound("plug-in guid", $"{plugIn.GetName().Name}"));

    private static Seq<ToolbarRow> Reached(Seq<ToolbarRow> toolbars) =>
        toSeq((toolbars + toolbars.Bind(static toolbar => Reached(toolbar.Items.Bind(static entry => entry.Switch(
            button: static button => button.Flyout.Map(static flyout => flyout.Target).ToSeq(),
            separator: static _ => Seq<ToolbarRow>()))))).DistinctBy(static toolbar => toolbar.Caption, StringComparer.Ordinal));

    // --- [IDENTIFIERS]
    private static Guid BarId(Guid file, ToolbarRow toolbar) => Uuid.NewNameBased(file, $"tool_bar/{toolbar.Caption}");

    private static Guid MacroId(Guid file, CommandScript script) => Uuid.NewNameBased(file, $"macro_item/{script.Text}");

    private static Guid IconId(Guid file, IGlyph glyph) => Uuid.NewNameBased(file, $"icon/{glyph.Key}");

    private static Guid MenuId(Guid file) => Uuid.NewNameBased(file, "extend_rhino_menus");

    private static Guid ItemId(Guid parent, string scope, int index) => Uuid.NewNameBased(parent, string.Create(CultureInfo.InvariantCulture, $"{scope}/{index}"));

    // --- [ELEMENTS]
    private static XElement Text(string name, string value) => new(name, new XElement($"locale_{English}", value));

    private static Seq<XAttribute> Platforms(Option<RuiPlatform> platform) => platform.Map(static held => new XAttribute("platforms", held.Key)).ToSeq();

    private static XElement Bar(Guid file, ToolbarRow toolbar) =>
        new("tool_bar",
            new XAttribute("guid", BarId(file, toolbar)),
            new XAttribute("bitmap_id", IconId(file, toolbar.Icon)),
            toolbar.Face == ButtonFace.Icon ? Seq<XAttribute>() : Seq(new XAttribute("item_display_style", toolbar.Face.Key)),
            Text("text", toolbar.Caption),
            toolbar.Items.Map((entry, index) => entry.Switch(
                (File: file, Item: ItemId(BarId(file, toolbar), "tool_bar_item", index)),
                button: static (at, button) => new XElement("tool_bar_item",
                    new XAttribute("guid", at.Item),
                    button.Left.Face.Glyph.Map(glyph => new XAttribute("bitmap_id", IconId(at.File, glyph))).ToSeq(),
                    Platforms(button.Platform),
                    Text("text", button.Left.Face.Caption),
                    new XElement("left_macro_id", MacroId(at.File, button.Left.Script)),
                    button.Right.Map(right => new XElement("right_macro_id", MacroId(at.File, right.Script))).ToSeq(),
                    button.Flyout.Map(flyout => new XElement("link", new XAttribute("style", flyout.Style.Key), BarId(at.File, flyout.Target))).ToSeq()),
                separator: static (at, _) => new XElement("tool_bar_item", new XAttribute("guid", at.Item), new XAttribute("button_style", "spacer")))));

    private static XElement MacroItem(Guid file, Macro macro) =>
        new("macro_item",
            new XAttribute("guid", MacroId(file, macro.Script)),
            macro.Face.Glyph.Map(glyph => new XAttribute("bitmap_id", IconId(file, glyph))).ToSeq(),
            Text("text", macro.Face.Caption),
            Text("tooltip", macro.Face.Help),
            Text("menu_text", macro.Face.Caption),
            new XElement("script", macro.Script.Text));

    private static Fin<XElement> Icon(Guid file, IGlyph glyph) =>
        Svg(glyph.Assets.Assembly, glyph.Key).ToFin(new NotFound("embedded SVG", glyph.Key)).Map(light => new XElement("icon",
            new XAttribute("guid", IconId(file, glyph)),
            new XAttribute("name", glyph.Key),
            new XElement("light", light),
            Svg(glyph.Assets.Assembly, $"{Path.GetFileNameWithoutExtension(glyph.Key)}{DarkVariant}{Path.GetExtension(glyph.Key)}").Map(static dark => new XElement("dark", dark)).ToSeq()));

    private static Option<XElement> Svg(Assembly assembly, string key) {
        using Stream? stream = assembly.GetManifestResourceStream(key);
        return Optional(stream).Map(XElement.Load);
    }

    // --- [MENUS]
    private static Seq<MenuItemRow> Flattened(Seq<MenuItemRow> entries) =>
        entries.Bind(static entry => entry is MenuItemRow.Submenu submenu ? Flattened(submenu.Entries) : Seq(entry));

    private static XElement Extended(Guid file, Assembly plugIn, Seq<MenuExtension> menus) =>
        new("menu",
            new XAttribute("guid", MenuId(file)),
            Text("text", $"{plugIn.GetName().Name}"),
            from menu in menus
            let path = string.Join("::", menu.Path)
            from placed in Placed(MenuId(file), path, menu.Entries)
            select Entry(file, placed.Id, placed.Entry, Some(path)));

    private static Seq<(Guid Id, MenuItemRow Entry)> Placed(Guid parent, string scope, Seq<MenuItemRow> entries) =>
        entries.Map((entry, index) => (Id: ItemId(parent, scope, index), Entry: entry));

    private static XElement Entry(Guid file, Guid id, MenuItemRow entry, Option<string> extends) =>
        (Platforms(entry.Platform) + extends.Map(static path => new XAttribute("ExtendRhinoMenu", path)).ToSeq()) switch {
            var placed => entry.Switch(
                (File: file, Id: id, Placed: placed, entry.Macro),
                run: static (held, _) => Item(held.File, held.Id, held.Placed, held.Macro),
                check: static (held, _) => Item(held.File, held.Id, held.Placed, held.Macro),
                radio: static (held, _) => Item(held.File, held.Id, held.Placed, held.Macro),
                separator: static (held, _) => new XElement("menu_item", new XAttribute("guid", held.Id), held.Placed, new XAttribute("item_type", "separator")),
                submenu: static (held, submenu) => new XElement("menu",
                    new XAttribute("guid", held.Id), held.Placed, Text("text", submenu.Caption),
                    Placed(held.Id, "menu_item", submenu.Entries).Map(child => Entry(held.File, child.Id, child.Entry, None)))),
        };

    private static XElement Item(Guid file, Guid id, Seq<XAttribute> placed, Option<Macro> macro) =>
        new("menu_item",
            new XAttribute("guid", id), placed,
            macro.Map(static held => Text("text", held.Face.Caption)).ToSeq(),
            macro.Map(held => new XElement("macro_id", MacroId(file, held.Script))).ToSeq());

    private static Seq<(Guid Menu, Guid Item, RuiUpdateUi.UpdateMenuItemEventHandler Handler)> Stateful(
        Guid file, Seq<MenuExtension> menus, IPlugInSink sink, CancellationToken live) =>
        menus.Bind(menu => Stateful(MenuId(file), string.Join("::", menu.Path), menu.Entries, sink, live));

    private static Seq<(Guid Menu, Guid Item, RuiUpdateUi.UpdateMenuItemEventHandler Handler)> Stateful(
        Guid parent, string scope, Seq<MenuItemRow> entries, IPlugInSink sink, CancellationToken live) =>
        Placed(parent, scope, entries).Bind(held => held.Entry.Switch(
            (Menu: parent, Item: held.Id, Sink: sink, Live: live),
            run: static (_, _) => Seq<(Guid Menu, Guid Item, RuiUpdateUi.UpdateMenuItemEventHandler Handler)>(),
            check: static (at, check) => Seq((at.Menu, at.Item, Handler(check.Command.Face, check.Enabled, check.Read, static (ui, on) => ui.Checked = on, at.Sink, at.Live))),
            radio: static (at, radio) => Seq((at.Menu, at.Item, Handler(radio.Command.Face, radio.Enabled, radio.Read, static (ui, on) => ui.RadioChecked = on, at.Sink, at.Live))),
            separator: static (_, _) => Seq<(Guid Menu, Guid Item, RuiUpdateUi.UpdateMenuItemEventHandler Handler)>(),
            submenu: static (at, submenu) => Stateful(at.Item, "menu_item", submenu.Entries, at.Sink, at.Live)));

    // --- [RUNTIME]
    public static readonly Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Registered =
        static (plugIn, retain) => Optional(plugIn.GetType().Assembly.GetCustomAttribute<RuiFileAttribute>()).Match(
            Some: declared => DisposalOps.Acquire(acquired =>
                from file in IO.lift(() => Identity(plugIn.GetType().Assembly))
                from live in DisposalOps.Acquire(IO.lift(static () => new CancellationTokenSource()), Ended,
                    held => acquired(new Disposal<CancellationTokenSource>(held, static source => Ended(source).Run())))
                from registered in IO.lift(() => Callbacks.Each(
                    Stateful(file, declared.Rui.Menus, plugIn, live.Token),
                    item => RuiUpdateUi.RegisterMenuItem(file, item.Menu, item.Item, item.Handler),
                    nameof(RuiUpdateUi.RegisterMenuItem)))
                select unit, retain),
            None: static () => IO.pure(unit));

    private static IO<Unit> Ended(CancellationTokenSource live) =>
        Callbacks.Each(Seq(IO.lift(live.Cancel), IO.lift(live.Dispose))).Map(unit);

    private static RuiUpdateUi.UpdateMenuItemEventHandler Handler(
        CommandFace face, Option<RowRule> enabled, Func<RowScope, IO<bool>> read, Action<RuiUpdateUi, bool> mark, IPlugInSink sink, CancellationToken live) =>
        Callbacks.Handler<RuiUpdateUi>(
            ui => unless(IO.lift(() => live.IsCancellationRequested),
                from document in IO.lift(static () => Optional(RhinoDoc.ActiveDoc))
                from scope in RowScope.Open(sink, document, new CommitMode.Immediate())
                from holds in RowRules.Holds(enabled, scope)
                from checks in read(scope)
                from written in IO.lift(() => {
                    (ui.Enabled, ui.Text) = (holds, face.Wording.Shown(face.Caption, sink));
                    mark(ui, checks);
                })
                select unit).As(),
            new CallbackSite(sink, typeof(RuiUpdateUi), nameof(RuiUpdateUi.UpdateMenuItemEventHandler))).Invoke;
}
