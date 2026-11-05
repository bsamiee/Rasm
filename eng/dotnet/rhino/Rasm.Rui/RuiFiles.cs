using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LanguageExt.ClassInstances;
using Rasm.Rhino;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
using SuperLinq;
using UUIDNext;

namespace Rasm.Rui;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Translations(HashMap<EqInt, int, HashMap<EqStringOrdinal, string, string>> Tables) {
    // --- [LOCALIZATION]
    public static int English { get; } = CultureInfo.GetCultureInfo("en-US").LCID;

    public XElement Of(string name, string english) =>
        new(name, new XElement(Locale(English), english),
            Tables.AsEnumerable().Choose(table => table.Value.Find(english).Map(local => new XElement(Locale(table.Key), local))));

    private static string Locale(int lcid) => string.Create(CultureInfo.InvariantCulture, $"locale_{lcid}");

    // --- [RESOURCES]
    public static Translations Read(Assembly plugIn) =>
        new(new HashMap<EqInt, int, HashMap<EqStringOrdinal, string, string>>(toSeq(plugIn.GetManifestResourceNames())
            .Filter(static name => name.Contains(".Localization.", StringComparison.Ordinal) && name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .Choose(name => Culture(Path.GetExtension(Path.GetFileNameWithoutExtension(name)).TrimStart('.')).Map(lcid => (lcid, Table(plugIn, name))))));

    private static Option<int> Culture(string segment) =>
        parseInt(segment) || Try.lift(() => CultureInfo.GetCultureInfo(segment).LCID).Run().ToOption();

    private static HashMap<EqStringOrdinal, string, string> Table(Assembly plugIn, string resource) {
        using Stream stream = plugIn.GetManifestResourceStream(resource)!;
        return new(toSeq(XDocument.Load(stream).Descendants("RMASTRING"))
            .Choose(static row => from english in Optional(row.Attribute("English")?.Value)
                                  from local in Optional(row.Attribute("Localized")?.Value)
                                  select (Key: english.Split("[[", 2)[0], Local: local)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class RuiFiles {
    // --- [FILES]
    public static IO<Unit> Write(string plugIn, string rui, Seq<string> references) =>
        references.Fold(HashMap<EqStringOrdinal, string, string>(), static (held, path) => held.TryAdd(Path.GetFileNameWithoutExtension(path), path)) switch {
            var located => IO.lift(() => {
                Func<AssemblyLoadContext, AssemblyName, Assembly?> handler = (context, name) => Optional(name.Name).Bind(located.Find).Map(context.LoadFromAssemblyPath).ValueUnsafe();
                AssemblyLoadContext.Default.Resolving += handler;
                return handler;
            }).Bracket(
                Use: _ =>
                    from loaded in IO.lift(() => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(plugIn)))
                    from document in Document(loaded)
                    from written in IO.lift(() => document.Match(
                        Some: held => {
                            using XmlWriter writer = XmlWriter.Create(rui, new XmlWriterSettings { Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), OmitXmlDeclaration = true, Indent = true });
                            held.Save(writer);
                        },
                        None: () => File.Delete(rui)))
                    select written,
                Fin: static handler => IO.lift(() => AssemblyLoadContext.Default.Resolving -= handler)),
        };

    // --- [DOCUMENT]
    private static IO<Option<XDocument>> Document(Assembly plugIn) =>
        IO.lift(() => Toolbars.Declared(plugIn).Traverse(declared =>
            from file in Toolbars.Identity(plugIn)
            let text = Translations.Read(plugIn)
            let groups = declared.Groups + declared.Groups.Bind(static toolbarGroup => toSeq(toolbarGroup.Tabs)).Choose(static tab => tab.Sidebar)
            let toolbars = Reached(declared.Toolbars + groups.Bind(static toolbarGroup => toSeq(toolbarGroup.Tabs)).Map(static tab => tab.Toolbar))
                .Map(toolbar => toolbar with { Face = toolbar.Face with { Icon = toolbar.Face.Icon | IconResource.PlugIn(plugIn) } })
            let macros = Macros(toolbars.Bind(static toolbar => toolbar.Groups) + declared.Extensions.Bind(static menu => menu.Groups) + declared.Menus.Bind(static menu => menu.Groups))
            let arts = toSeq((toolbars.Choose(static toolbar => toolbar.Face.Icon) + macros.Choose(static held => held.Face.Icon)).DistinctBy(art => IconId(file, art)))
            from elements in (
                    arts.Traverse(art => Icon(file, plugIn, art)).As(),
                    toolbars.Traverse(toolbar => Bar(file, text, toolbar)).As().Bind(Unique),
                    Unique(groups.Map(toolbarGroup => Group(file, text, toolbarGroup))),
                    Unique(macros.Map(held => MacroItem(file, text, held.Macro, held.Face))))
                .Apply(static (icons, bars, tabbed, scripts) => (Icons: icons, Bars: bars, Groups: tabbed, Macros: scripts)).As()
            select new XDocument(new XElement("RhinoUI",
                new XAttribute("major_ver", 5), new XAttribute("minor_ver", 0), new XAttribute("guid", file), new XAttribute("plug_in_guid", file),
                new XAttribute("localize", bool.FalseString), new XAttribute("default_language_id", Translations.English),
                new XElement("extend_rhino_menus", declared.Extensions.IsEmpty ? Seq<XElement>() : Seq(Extended(file, text, declared.Extensions))),
                new XElement("menus", declared.Menus.Map(menu => Menu(file, text, menu))),
                new XElement("tool_bar_groups", elements.Groups),
                new XElement("tool_bars", elements.Bars),
                new XElement("macros", elements.Macros),
                new XElement("icons", elements.Icons),
                new XElement("bitmaps")))).As());

    private static Fin<Seq<XElement>> Unique(Seq<XElement> elements) =>
        toSeq(elements.GroupBy(static element => element.Attribute("guid")?.Value, StringComparer.Ordinal)).Traverse(static held => held.Distinct<XElement>(XNode.EqualityComparer).ToArray() switch {
            [var only] => (Fin<XElement>)only,
            var repeated => new RhinoFault(repeated[0].Elements("text").First().Value, RhinoReason.Repeated),
        }).As();

    // --- [REACH]
    private static Seq<ToolbarRow> Reached(Seq<ToolbarRow> toolbars) =>
        toolbars.Bind(static toolbar => toSeq(SuperEnumerable.TraverseDepthFirst(toolbar, static held => held.Groups.Flatten().Choose(Linked))));

    private static Option<ToolbarRow> Linked(Command<Macro> command) =>
        command.Switch(
            run: static _ => Option<ToolbarRow>.None,
            choice: static choice => Some(new ToolbarRow(choice.Face with { Display = None }, Seq(choice.Members.Map(static member => (Command<Macro>)member)))),
            menu: static menu => Some(new ToolbarRow(menu.Face with { Display = None }, menu.Groups)));

    private static (CommandFace Face, Option<Command<Macro>.Run> Pressed) Press(Command<Macro> command) =>
        command.Switch(
            run: static run => (run.Face, Some(run)),
            choice: static choice => (choice.Face, choice.Members.Head),
            menu: static menu => (menu.Face, menu.Primary));

    private static Seq<(Option<Macro> Macro, CommandFace Face)> Macros(Seq<Seq<Command<Macro>>> groups) =>
        toSeq(groups.Flatten().SelectMany(static root => SuperEnumerable.TraverseDepthFirst(root, static command => command.Switch(
            run: static _ => Seq<Command<Macro>>(),
            choice: static choice => choice.Members.Map<Command<Macro>>(static member => member),
            menu: static menu => menu.Groups.Flatten())))
            .SelectMany(static command => Press(command) switch {
                var (face, pressed) => (pressed.Map(static run => run.Invoke), face).Cons(pressed.Bind(static run => run.Alternate).Map(static alternate => (Some(alternate.Invoke), alternate.Face)).ToSeq()),
            }));

    // --- [IDENTIFIERS]
    private static Guid BarId(Guid file, ToolbarRow toolbar) => Uuid.NewNameBased(file, $"tool_bar/{toolbar.Face.Name}");

    private static Guid GroupId(Guid file, ToolbarGroupRow group) => Uuid.NewNameBased(file, $"tool_bar_group/{group.Face.Name}");

    private static Guid MacroId(Guid file, Option<Macro> macro, CommandFace face) => Uuid.NewNameBased(file, $"macro_item/{face.Name}/{macro.Map(static held => held.Text).IfNone("")}");

    private static Guid IconId(Guid file, IconResource art) => Uuid.NewNameBased(file, $"icon/{art.Assembly.GetName().Name}/{art.Id}");

    // --- [ELEMENTS]
    private static Seq<XAttribute> Platforms(RuiPlatforms platforms) =>
        platforms == RuiPlatforms.Both ? Seq<XAttribute>() : Seq(new XAttribute("platforms", platforms.Key));

    private static RuiPlatforms Within(RuiPlatforms scope, RuiPlatforms own) => scope == RuiPlatforms.Both ? own : scope;

    private static XElement Group(Guid file, Translations text, ToolbarGroupRow group) =>
        (Id: GroupId(file, group), Tabs: toSeq(group.Tabs)) switch {
            var (id, tabs) => new("tool_bar_group",
                new XAttribute("guid", id),
                new XAttribute("dock_bar_guid64", Uuid.NewNameBased(id, "dock_bar")),
                Platforms(group.Face.Platforms),
                new XAttribute("hide_single_tab", group.HideSingleTab),
                group.Selected.Bind(selected => tabs.Map(static (tab, index) => (tab, index)).Find(held => BarId(file, held.tab.Toolbar) == BarId(file, selected)))
                    .Map(held => new XAttribute("active_tool_bar_group", Toolbars.ItemId(id, "tool_bar_group_item", held.index))).ToSeq(),
                text.Of("text", group.Face.Caption),
                new XElement("dock_bar_info",
                    new XAttribute("visible", group.Visible),
                    new XAttribute("floating", group.Placement is ToolbarPlacement.Floating),
                    new XAttribute("dock_location_u", group.Placement.Side.Site),
                    new XAttribute("dock_location", group.Placement.Side.Key),
                    (group.Placement as ToolbarPlacement.Floating)?.Bounds.ToSeq().Bind(static bounds => Seq(
                        new XAttribute("float_point", string.Create(CultureInfo.InvariantCulture, $"{bounds.X},{bounds.Y}")),
                        new XAttribute("float_size", string.Create(CultureInfo.InvariantCulture, $"{bounds.Width},{bounds.Height}"))))),
                tabs.Map((tab, index) => new XElement("tool_bar_group_item",
                    new XAttribute("guid", Toolbars.ItemId(id, "tool_bar_group_item", index)),
                    Platforms(tab.Toolbar.Face.Platforms),
                    text.Of("text", tab.Toolbar.Face.Caption),
                    tab.Sidebar.Map(sidebar => new XElement("sidebar", GroupId(file, sidebar))).ToSeq(),
                    new XElement("tool_bar_id", BarId(file, tab.Toolbar))))),
        };

    private static Fin<XElement> Bar(Guid file, Translations text, ToolbarRow toolbar) =>
        from groups in toolbar.Groups.Traverse(cluster => cluster.Traverse(command => Button(file, text, command)).As()).As()
        let id = BarId(file, toolbar)
        select new XElement("tool_bar",
            new XAttribute("guid", id),
            Bitmap(file, toolbar.Face).ToSeq(),
            Platforms(toolbar.Face.Platforms),
            toolbar.Face.Display.Map(static display => new XAttribute("item_display_style", display.Key)).ToSeq(),
            text.Of("text", toolbar.Face.Caption),
            groups.Intersperse(Seq(Spacer)).Flatten().Map((item, index) => item(Toolbars.ItemId(id, "tool_bar_item", index))));

    private static Fin<Func<Guid, XElement>> Button(Guid file, Translations text, Command<Macro> command) =>
        Press(command) switch {
            var (face, pressed) => RhinoFault.Unless(command.Enabled.IsNone && pressed.ForAll(static run => run.Enabled.IsNone && run.Marked.IsNone), RhinoReason.Unsupported, face.Name)
                .Map(_ => (Func<Guid, XElement>)(id => new XElement("tool_bar_item",
                    new XAttribute("guid", id),
                    face.Presented.ToSeq().Bind(static display => Seq(new XAttribute("button_display_mode", display.Key), new XAttribute("display_style_from_parent", bool.FalseString))),
                    Platforms(face.Platforms),
                    text.Of("text", face.Caption),
                    new XElement("left_macro_id", MacroId(file, pressed.Map(static run => run.Invoke), face)),
                    pressed.Bind(static run => run.Alternate).Map(alternate => new XElement("right_macro_id", MacroId(file, alternate.Invoke, alternate.Face))).ToSeq(),
                    Linked(command).Map(bar => new XElement("link", new XAttribute("style", command is Command<Macro>.Choice ? "float_to_top" : "normal"), BarId(file, bar))).ToSeq()))),
        };

    private static XElement Spacer(Guid id) => new("tool_bar_item", new XAttribute("guid", id), new XAttribute("button_style", "spacer"));

    private static XElement MacroItem(Guid file, Translations text, Option<Macro> macro, CommandFace face) =>
        new("macro_item",
            new XAttribute("guid", MacroId(file, macro, face)),
            Bitmap(file, face).ToSeq(),
            Platforms(face.Platforms),
            text.Of("text", face.Caption),
            face.Help.Map(help => text.Of("tooltip", help)).ToSeq(),
            face.Help.Map(help => text.Of("help_text", help)).ToSeq(),
            text.Of("button_text", face.Caption),
            text.Of("menu_text", face.Caption),
            macro.Map(static held => new XElement("script", held.Text)).ToSeq());

    // --- [ICONS]
    private static Option<XAttribute> Bitmap(Guid file, CommandFace face) =>
        face.Icon.Map(art => new XAttribute("bitmap_id", IconId(file, art)));

    private static Fin<XElement> Icon(Guid file, Assembly plugIn, IconResource art) {
        Dictionary<string, ResourceFile> scanned = Internal.ScanEmbeddedResources(art.Assembly);
        string drawn = (plugIn.GetCustomAttribute<TargetPlatformAttribute>()?.PlatformName.StartsWith("Windows", StringComparison.OrdinalIgnoreCase) is true
            ? scanned.GetValueOrDefault(art.Id)?.WindowsResourceName
            : scanned.GetValueOrDefault(art.Id)?.MacResourceName) ?? art.Id;
        return from light in Svg(art.Assembly, drawn).ToFin(new RhinoFault(art.Id, RhinoReason.Missing))
               select new XElement("icon",
                   new XAttribute("guid", IconId(file, art)),
                   new XAttribute("name", art.Id),
                   new XElement("light", light),
                   Optional(scanned.GetValueOrDefault(drawn)?.DarkModeResourceName).Bind(dark => Svg(art.Assembly, dark)).Map(static dark => new XElement("dark", dark)).ToSeq());
    }

    private static Option<XElement> Svg(Assembly assembly, string key) {
        using Stream? stream = assembly.GetManifestResourceStream(key);
        return Optional(stream).Map(XElement.Load);
    }

    // --- [MENUS]
    private static XElement Extended(Guid file, Translations text, Seq<MenuExtension> extensions) =>
        new("menu", new XAttribute("guid", Toolbars.ExtensionsId(file)),
            from extension in extensions
            from entry in Toolbars.Entries(file, extension)
            select Entry(file, text, entry, Some(extension.Text), extension.Platforms));

    private static XElement Menu(Guid file, Translations text, MenuRow menu) =>
        new("menu", new XAttribute("guid", Toolbars.MenuId(file, menu)), Platforms(menu.Face.Platforms), text.Of("text", menu.Face.Caption),
            Toolbars.Entries(file, menu).Map(entry => Entry(file, text, entry, None, RuiPlatforms.Both)));

    private static XElement Entry(Guid file, Translations text, MenuEntry entry, Option<string> path, RuiPlatforms scope) =>
        entry.Switch(
            (File: file, Text: text, Placed: path.Map(static held => new XAttribute("ExtendRhinoMenu", held)).ToSeq(), Scope: scope),
            separator: static (at, separator) => new XElement("menu_item", new XAttribute("guid", separator.Id), at.Placed, Platforms(at.Scope), new XAttribute("item_type", "separator")),
            item: static (at, item) => new XElement("menu_item",
                new XAttribute("guid", item.Id),
                at.Placed,
                Platforms(Within(at.Scope, item.Run.Face.Platforms)),
                at.Text.Of("text", item.Run.Face.Caption),
                new XElement("macro_id", MacroId(at.File, item.Run.Invoke, item.Run.Face))),
            submenu: static (at, submenu) => new XElement("menu",
                new XAttribute("guid", submenu.Id),
                at.Placed,
                Platforms(Within(at.Scope, submenu.Menu.Face.Platforms)),
                at.Text.Of("text", submenu.Menu.Face.Caption),
                submenu.Children.Map(child => Entry(at.File, at.Text, child, None, RuiPlatforms.Both))));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static int Main(string[] args) =>
        (from request in IO.lift(Fin<(string PlugIn, string Rui, string References)> () => args is [var plugIn, var rui, ['@', .. var references]]
                ? (plugIn, rui, references)
                : Error.New(string.Create(CultureInfo.InvariantCulture, $"Expected <plug-in> <rui> @<references>, received {args.Length}: {string.Join(' ', args)}")))
         from references in IO.liftAsync(env => File.ReadAllLinesAsync(request.References, env.Token))
         from written in RuiFiles.Write(request.PlugIn, request.Rui, toSeq(references))
         select written).RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => {
                Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
                return 1;
            });
}
