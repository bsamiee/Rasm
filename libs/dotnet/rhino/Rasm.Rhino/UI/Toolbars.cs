using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
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

public sealed record ToolbarRow(CommandFace Face, Seq<Seq<Command<RowRule, Macro>>> Groups) {
    public ButtonFace Display { get; init; } = ButtonFace.Icon;
}

public sealed record MenuExtension(Seq<string> Path, Seq<Seq<Command<RowRule, Macro>>> Groups);

public sealed record RuiFile(Seq<ToolbarRow> Toolbars, Seq<MenuExtension> Menus);

[AttributeUsage(AttributeTargets.Assembly)]
public abstract class RuiFileAttribute : Attribute {
    public abstract RuiFile Rui { get; }
}

public sealed class RuiFileAttribute<TSource> : RuiFileAttribute where TSource : IRuiSource {
    public override RuiFile Rui => TSource.Rui;
}

internal sealed record MenuItemUpdate(Guid Menu, Guid Item, Func<RowScope, RuiUpdateUi, IO<Unit>> Write);

internal sealed record MenuPlacement(XElement Element, Seq<MenuItemUpdate> Updates);

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

    public static Fin<Option<XDocument>> Document(Assembly plugIn) =>
        Optional(plugIn.GetCustomAttribute<RuiFileAttribute>()).Traverse(declared =>
            from file in Identity(plugIn)
            let toolbars = Reached(declared.Rui.Toolbars)
            let macros = toSeq(Macros(declared.Rui.Toolbars.Bind(static toolbar => toolbar.Groups) + declared.Rui.Menus.Bind(static menu => menu.Groups))
                .DistinctBy(held => MacroId(file, held.Macro)))
            from arts in (toolbars.Choose(static toolbar => toolbar.Face.Glyph) + macros.Choose(static held => held.Face.Glyph)).Traverse(Paired).As()
            from icons in toSeq(arts.DistinctBy(static art => art.Light, StringComparer.Ordinal)).Traverse(art => Icon(file, art)).As()
            from bars in toolbars.Traverse(toolbar => Bar(file, toolbar)).As()
            from items in macros.Traverse(held => MacroItem(file, held.Macro, held.Face)).As()
            select new XDocument(new XElement("RhinoUI",
                new XAttribute("major_ver", 5), new XAttribute("minor_ver", 0), new XAttribute("guid", file), new XAttribute("plug_in_guid", file),
                new XAttribute("localize", bool.FalseString), new XAttribute("default_language_id", English),
                new XElement("extend_rhino_menus", declared.Rui.Menus.IsEmpty ? Seq<XElement>() : Seq(Extended(file, plugIn, Extensions(file, declared.Rui.Menus)))),
                new XElement("menus"),
                new XElement("tool_bar_groups"),
                new XElement("tool_bars", bars),
                new XElement("macros", items),
                new XElement("icons", icons),
                new XElement("bitmaps")))).As();

    private static Fin<Guid> Identity(Assembly plugIn) =>
        Optional(plugIn.GetCustomAttribute<GuidAttribute>()).Bind(static held => parseGuid(held.Value))
            .ToFin(new NotFound("plug-in guid", $"{plugIn.GetName().Name}"));

    private static Seq<ToolbarRow> Reached(Seq<ToolbarRow> toolbars) =>
        toolbars.Bind(static toolbar => toolbar.Cons(Reached(toolbar.Groups.Flatten().Bind(static command => command.Switch(
            run: static _ => Seq<ToolbarRow>(),
            choice: static _ => Seq<ToolbarRow>(),
            menu: static menu => Seq(new ToolbarRow(menu.Face, menu.Groups)))))));

    // --- [MACROS]
    private static Seq<(Macro Macro, CommandFace Face)> Macros(Seq<Seq<Command<RowRule, Macro>>> groups) =>
        groups.Flatten().Bind(static command => command.Switch(
            run: Invokes,
            choice: static choice => choice.Members.Bind(Invokes),
            menu: static menu => menu.Primary.ToSeq().Bind(Invokes) + Macros(menu.Groups)));

    private static Seq<(Macro Macro, CommandFace Face)> Invokes(Command<RowRule, Macro>.Run run) =>
        (run.Invoke, run.Face).Cons(run.Alternate.Map(alternate => (alternate, Alternated(run.Face, run.Invoke, alternate))).ToSeq());

    private static CommandFace Alternated(CommandFace face, Macro primary, Macro alternate) =>
        alternate.Steps.Filter(step => primary.Steps.ForAll(held => held != step)).Choose(static step => step.EnglishName) switch {
            var names => face with { Caption = string.Join(", ", face.Caption.Cons(names)), Help = face.Help.Map(help => string.Join(", ", help.Cons(names))) },
        };

    // --- [IDENTIFIERS]
    private static Guid BarId(Guid file, ToolbarRow toolbar) => Uuid.NewNameBased(file, $"tool_bar/{toolbar.Face.Name}");

    private static Guid MacroId(Guid file, Macro macro) => Uuid.NewNameBased(file, $"macro_item/{macro.Text}");

    private static Guid IconId(Guid file, GlyphSource.Paired art) => Uuid.NewNameBased(file, $"icon/{art.Light}");

    private static Guid MenuId(Guid file) => Uuid.NewNameBased(file, "extend_rhino_menus");

    private static Guid ItemId(Guid parent, string scope, int index) => Uuid.NewNameBased(parent, string.Create(CultureInfo.InvariantCulture, $"{scope}/{index}"));

    // --- [ELEMENTS]
    private static XElement Text(string name, string value) => new(name, new XElement($"locale_{English}", value));

    private static Fin<XElement> Bar(Guid file, ToolbarRow toolbar) =>
        from bitmap in toolbar.Face.Glyph.Traverse(glyph => Bitmap(file, glyph)).As()
        from groups in toolbar.Groups.Traverse(@group => @group.Traverse(command => Button(file, command)).As()).As()
        select new XElement("tool_bar",
            new XAttribute("guid", BarId(file, toolbar)),
            bitmap.ToSeq(),
            toolbar.Display == ButtonFace.Icon ? Seq<XAttribute>() : Seq(new XAttribute("item_display_style", toolbar.Display.Key)),
            Text("text", toolbar.Face.Caption),
            groups.Intersperse(Seq(Spacer)).Flatten().Map((item, index) => item(ItemId(BarId(file, toolbar), "tool_bar_item", index))));

    private static Fin<Func<Guid, XElement>> Button(Guid file, Command<RowRule, Macro> command) =>
        command.Switch(
            file,
            run: static (at, run) => Pressed(at, run.Face, run.Enabled, Some(run), None),
            choice: static (at, choice) => Pressed(at, choice.Face, choice.Enabled, None, None),
            menu: static (at, menu) => Pressed(at, menu.Face, menu.Enabled, menu.Primary, Some(new ToolbarRow(menu.Face, menu.Groups))));

    private static Fin<Func<Guid, XElement>> Pressed(Guid file, CommandFace face, Option<RowRule> gate, Option<Command<RowRule, Macro>.Run> pressed, Option<ToolbarRow> link) =>
        from run in pressed.Filter(run => gate.IsNone && run.Enabled.IsNone && run.Marked.IsNone)
            .ToFin(new Refused(nameof(Document), $"realizes toolbar entry {face.Name} only as one run with no gate or mark"))
        from icon in face.Glyph.Traverse(glyph => Bitmap(file, glyph)).As()
        select (Func<Guid, XElement>)(id => new XElement("tool_bar_item",
            new XAttribute("guid", id),
            icon.ToSeq(),
            Text("text", face.Caption),
            new XElement("left_macro_id", MacroId(file, run.Invoke)),
            run.Alternate.Map(alternate => new XElement("right_macro_id", MacroId(file, alternate))).ToSeq(),
            link.Map(bar => new XElement("link", new XAttribute("style", "normal"), BarId(file, bar))).ToSeq()));

    private static XElement Spacer(Guid id) => new("tool_bar_item", new XAttribute("guid", id), new XAttribute("button_style", "spacer"));

    private static Fin<XElement> MacroItem(Guid file, Macro macro, CommandFace face) =>
        face.Glyph.Traverse(glyph => Bitmap(file, glyph)).As().Map(bitmap => new XElement("macro_item",
            new XAttribute("guid", MacroId(file, macro)),
            bitmap.ToSeq(),
            Text("text", face.Caption),
            face.Help.Map(static help => Text("tooltip", help)).ToSeq(),
            Text("menu_text", face.Caption),
            new XElement("script", macro.Text)));

    // --- [ICONS]
    private static Fin<GlyphSource.Paired> Paired(GlyphSource glyph) =>
        glyph.Switch<Fin<GlyphSource.Paired>>(
            tinted: static _ => new Refused(nameof(Document), $"holds no {nameof(GlyphSource.Tinted)} glyph"),
            paired: static held => held,
            picture: static _ => new Refused(nameof(Document), $"holds no {nameof(GlyphSource.Picture)} glyph"));

    private static Fin<XAttribute> Bitmap(Guid file, GlyphSource glyph) =>
        Paired(glyph).Map(art => new XAttribute("bitmap_id", IconId(file, art)));

    private static Fin<XElement> Icon(Guid file, GlyphSource.Paired art) =>
        from light in Svg(art.Assembly, art.Light)
        from dark in (string.Equals(art.Dark, art.Light, StringComparison.Ordinal) ? Seq<string>() : Seq(art.Dark)).Traverse(name => Svg(art.Assembly, name)).As()
        select new XElement("icon",
            new XAttribute("guid", IconId(file, art)),
            new XAttribute("name", art.Light),
            new XElement("light", light),
            dark.Map(static held => new XElement("dark", held)));

    private static Fin<XElement> Svg(Assembly assembly, string key) {
        using Stream? stream = assembly.GetManifestResourceStream(key);
        return Optional(stream).Map(XElement.Load).ToFin(new NotFound("embedded SVG", key));
    }

    // --- [MENUS]
    private static XElement Extended(Guid file, Assembly plugIn, Seq<MenuPlacement> placed) =>
        new("menu", new XAttribute("guid", MenuId(file)), Text("text", $"{plugIn.GetName().Name}"), placed.Map(static held => held.Element));

    private static Seq<MenuPlacement> Extensions(Guid file, Seq<MenuExtension> menus) =>
        menus.Bind(menu => string.Join("::", menu.Path) switch {
            var path => (Seq(Separator) + Entries(file, menu.Groups, []))
                .Map((entry, index) => entry(MenuId(file), ItemId(MenuId(file), path, index), Seq(new XAttribute("ExtendRhinoMenu", path)))),
        });

    private static Seq<Func<Guid, Guid, Seq<XAttribute>, MenuPlacement>> Entries(Guid file, Seq<Seq<Command<RowRule, Macro>>> groups, Seq<RowRule> gates) =>
        groups.Map(group => group.Bind(command => command.Switch(
            (File: file, Gates: gates + command.Enabled.ToSeq()),
            run: static (at, run) => Seq(Item(at.File, run, at.Gates, static (ui, on) => ui.Checked = on)),
            choice: static (at, choice) => choice.Members.Map(member => Item(at.File, member, at.Gates + member.Enabled.ToSeq(), static (ui, on) => ui.RadioChecked = on)),
            menu: static (at, menu) => Seq(Submenu(at.File, menu, at.Gates)))))
            .Intersperse(Seq(Separator)).Flatten();

    private static Func<Guid, Guid, Seq<XAttribute>, MenuPlacement> Item(Guid file, Command<RowRule, Macro>.Run run, Seq<RowRule> gates, Action<RuiUpdateUi, bool> mark) =>
        (menu, id, placed) => new(
            new XElement("menu_item", new XAttribute("guid", id), placed, Text("text", run.Face.Caption), new XElement("macro_id", MacroId(file, run.Invoke))),
            [new MenuItemUpdate(menu, id, (scope, ui) =>
                from holds in gates.TraverseM(rule => rule.Holds(scope)).As()
                from marks in run.Marked.Traverse(rule => rule.Holds(scope)).As()
                from written in IO.lift(() => {
                    (ui.Enabled, ui.Text) = (holds.ForAll(static held => held), RowText.Localize(run.Face.Caption, scope.Sink).Local);
                    _ = marks.Iter(held => mark(ui, held));
                })
                select unit)]);

    private static Func<Guid, Guid, Seq<XAttribute>, MenuPlacement> Submenu(Guid file, Command<RowRule, Macro>.Menu menu, Seq<RowRule> gates) =>
        (_, id, placed) => Entries(file, menu.Groups, gates).Map((entry, index) => entry(id, ItemId(id, "menu_item", index), [])) switch {
            var children => new(
                new XElement("menu", new XAttribute("guid", id), placed, Text("text", menu.Face.Caption), children.Map(static child => child.Element)),
                children.Bind(static child => child.Updates)),
        };

    private static MenuPlacement Separator(Guid _, Guid id, Seq<XAttribute> placed) =>
        new(new XElement("menu_item", new XAttribute("guid", id), placed, new XAttribute("item_type", "separator")), []);

    // --- [RUNTIME]
    public static readonly Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Registered =
        static (plugIn, retain) => Optional(plugIn.GetType().Assembly.GetCustomAttribute<RuiFileAttribute>()).Match(
            Some: declared => DisposalOps.Acquire(acquired =>
                from file in IO.lift(() => Identity(plugIn.GetType().Assembly))
                from live in DisposalOps.Acquire(IO.lift(static () => new CancellationTokenSource()),
                    static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit), acquired)
                from registered in IO.lift(() => Callbacks.Each(
                    Extensions(file, declared.Rui.Menus).Bind(static held => held.Updates),
                    update => RuiUpdateUi.RegisterMenuItem(file, update.Menu, update.Item, Handler(update.Write, plugIn, live.Token)),
                    nameof(RuiUpdateUi.RegisterMenuItem)))
                select unit, retain),
            None: static () => IO.pure(unit));

    private static RuiUpdateUi.UpdateMenuItemEventHandler Handler(Func<RowScope, RuiUpdateUi, IO<Unit>> write, IPlugInSink sink, CancellationToken live) =>
        Callbacks.Handler<RuiUpdateUi>(
            ui => unless(IO.lift(() => live.IsCancellationRequested),
                from document in IO.lift(static () => Optional(RhinoDoc.ActiveDoc))
                from scope in RowScope.Open(sink, document, new CommitMode.Immediate())
                from written in write(scope, ui)
                select unit).As(),
            new CallbackSite(sink, typeof(RuiUpdateUi), nameof(RuiUpdateUi.UpdateMenuItemEventHandler))).Invoke;
}
