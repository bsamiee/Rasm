using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using LanguageExt.ClassInstances;
using Rasm.Rhino;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using SuperLinq;
using UUIDNext;
using Translations = LanguageExt.HashMap<LanguageExt.ClassInstances.EqInt, int, LanguageExt.HashMap<LanguageExt.ClassInstances.EqStringOrdinal, string, string>>;

namespace Rasm.Rui;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class RuiText {
    // --- [LOCALIZATION]
    public static int English { get; } = CultureInfo.GetCultureInfo("en-US").LCID;

    extension(Translations tables) {
        public XElement Of(string name, string english) =>
            new(name, new XElement(Locale(English), english),
                tables.AsEnumerable().Choose(table => table.Value.Find(english).Map(local => new XElement(Locale(table.Key), local))));
    }

    private static string Locale(int lcid) => string.Create(CultureInfo.InvariantCulture, $"locale_{lcid}");

    // --- [RESOURCES]
    public static Translations Read(Assembly plugIn) =>
        new(toSeq(plugIn.GetManifestResourceNames())
            .Filter(static name => name.Contains(".Localization.", StringComparison.Ordinal) && name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .Choose(name => Culture(Path.GetExtension(Path.GetFileNameWithoutExtension(name)).TrimStart('.')).Map(lcid => (lcid, Table(plugIn, name)))));

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
                    from written in IO.lift(() => document.Match(Some: held => held.Save(rui), None: () => File.Delete(rui)))
                    select written,
                Fin: static handler => IO.lift(() => AssemblyLoadContext.Default.Resolving -= handler)),
        };

    // --- [DOCUMENT]
    private static IO<Option<XDocument>> Document(Assembly plugIn) =>
        IO.lift(() => Toolbars.Declared(plugIn).Traverse(declared =>
            from file in Toolbars.Identity(plugIn)
            let text = RuiText.Read(plugIn)
            let toolbars = toSeq(Reached(declared.Toolbars + declared.Groups.Bind(static toolbarGroup => toSeq(toolbarGroup.Tabs))).DistinctBy(toolbar => BarId(file, toolbar)))
            let placements = Toolbars.Placements(file, declared)
            let macros = toSeq(Macros(toolbars.Bind(static toolbar => toolbar.Groups) + declared.Extensions.Bind(static menu => menu.Groups) + declared.Menus.Bind(static menu => menu.Groups))
                .DistinctBy(held => MacroId(file, held.Macro)))
            let arts = toSeq((toolbars.Choose(static toolbar => toolbar.Face.Icon) + macros.Choose(static held => held.Face.Icon)
                + toolbars.Bind(static toolbar => toolbar.Groups.Flatten()).Choose(static command => command.Face.Icon)).DistinctBy(static art => art.Id, StringComparer.Ordinal))
            from elements in (arts.Traverse(art => Icon(file, art)).As(), toolbars.Traverse(toolbar => Bar(file, text, toolbar)).As())
                .Apply(static (icons, bars) => (Icons: icons, Bars: bars)).As()
            select new XDocument(new XElement("RhinoUI",
                new XAttribute("major_ver", 5), new XAttribute("minor_ver", 0), new XAttribute("guid", file), new XAttribute("plug_in_guid", file),
                new XAttribute("localize", bool.FalseString), new XAttribute("default_language_id", RuiText.English),
                new XElement("extend_rhino_menus", placements.Filter(static placed => placed.Path.IsSome).IsEmpty ? Seq<XElement>() : Seq(Extended(file, text, plugIn, placements))),
                new XElement("menus", placements.Filter(static placed => placed.Path.IsNone).Map(placed => Menu(file, text, placed))),
                new XElement("tool_bar_groups", declared.Groups.Map(toolbarGroup => Group(file, text, toolbarGroup))),
                new XElement("tool_bars", elements.Bars),
                new XElement("macros", macros.Map(held => MacroItem(file, text, held.Macro, held.Face))),
                new XElement("icons", elements.Icons),
                new XElement("bitmaps")))).As());

    // --- [REACH]
    private static Seq<ToolbarRow> Reached(Seq<ToolbarRow> toolbars) =>
        toolbars.Bind(static toolbar => toSeq(SuperEnumerable.TraverseDepthFirst(toolbar, static held => held.Groups.Flatten().Bind(static command => command.Switch(
            run: static _ => Seq<ToolbarRow>(),
            choice: static choice => Seq(Flyout(choice)),
            menu: static menu => Seq(new ToolbarRow(menu.Face, menu.Groups)))))));

    private static ToolbarRow Flyout(Command<Macro>.Choice choice) => new(choice.Face, Seq(choice.Members.Map(static member => (Command<Macro>)member)));

    private static Seq<(Macro Macro, CommandFace Face)> Macros(Seq<Seq<Command<Macro>>> groups) =>
        toSeq(groups.Flatten().SelectMany(static root => SuperEnumerable.TraverseDepthFirst(root, static command => command.Switch(
            run: static _ => Seq<Command<Macro>>(),
            choice: static choice => choice.Members.Map<Command<Macro>>(static member => member),
            menu: static menu => menu.Primary.ToSeq().Map<Command<Macro>>(static primary => primary) + menu.Groups.Flatten())))
            .OfType<Command<Macro>.Run>().SelectMany(static run => (run.Invoke, run.Face).Cons(run.Alternate.Map(static alternate => (alternate.Invoke, alternate.Face)).ToSeq())));

    // --- [IDENTIFIERS]
    private static Guid BarId(Guid file, ToolbarRow toolbar) => Uuid.NewNameBased(file, $"tool_bar/{toolbar.Face.Name}");

    private static Guid MacroId(Guid file, Macro macro) => Uuid.NewNameBased(file, $"macro_item/{macro.Text}");

    private static Guid IconId(Guid file, string resource) => Uuid.NewNameBased(file, $"icon/{resource}");

    // --- [ELEMENTS]
    private static Seq<XAttribute> Platforms(RuiPlatforms platforms) =>
        platforms == RuiPlatforms.Both ? Seq<XAttribute>() : Seq(new XAttribute("platforms", platforms.Key));

    private static XElement Group(Guid file, Translations text, ToolbarGroupRow group) =>
        Uuid.NewNameBased(file, $"tool_bar_group/{group.Face.Name}") switch {
            var id => new("tool_bar_group",
                new XAttribute("guid", id),
                Platforms(group.Platforms),
                text.Of("text", group.Face.Caption),
                new XElement("dock_bar_info",
                    new XAttribute("visible", group.Visible), new XAttribute("floating", group.Floating), new XAttribute("dock_location", group.Dock)),
                toSeq(group.Tabs).Map((tab, index) => new XElement("tool_bar_group_item",
                    new XAttribute("guid", Toolbars.ItemId(id, "tool_bar_group_item", index)),
                    text.Of("text", tab.Face.Caption),
                    new XElement("tool_bar_id", BarId(file, tab))))),
        };

    private static Fin<XElement> Bar(Guid file, Translations text, ToolbarRow toolbar) =>
        from groups in toolbar.Groups.Traverse(cluster => cluster.Traverse(command => Button(file, text, command)).As()).As()
        let id = BarId(file, toolbar)
        select new XElement("tool_bar",
            new XAttribute("guid", id),
            Bitmap(file, toolbar.Face).ToSeq(),
            Platforms(toolbar.Platforms),
            toolbar.Display == ButtonFace.Icon ? Seq<XAttribute>() : Seq(new XAttribute("item_display_style", toolbar.Display.Key)),
            text.Of("text", toolbar.Face.Caption),
            groups.Intersperse(Seq(Spacer)).Flatten().Map((item, index) => item(Toolbars.ItemId(id, "tool_bar_item", index))));

    private static Fin<Func<Guid, XElement>> Button(Guid file, Translations text, Command<Macro> command) =>
        command.Switch(
            (File: file, Text: text),
            run: static (at, run) => Pressed(at.File, at.Text, run.Face, run.Enabled, Some(run), None),
            choice: static (at, choice) => Pressed(at.File, at.Text, choice.Face, choice.Enabled, choice.Members.Head, Some(Flyout(choice))),
            menu: static (at, menu) => Pressed(at.File, at.Text, menu.Face, menu.Enabled, menu.Primary, Some(new ToolbarRow(menu.Face, menu.Groups))));

    private static Fin<Func<Guid, XElement>> Pressed(Guid file, Translations text, CommandFace face, Option<RowRule> gate, Option<Command<Macro>.Run> pressed, Option<ToolbarRow> link) =>
        pressed.Filter(run => gate.IsNone && run.Enabled.IsNone && run.Marked.IsNone)
            .ToFin(new RhinoFault(face.Name, RhinoReason.Unsupported))
            .Map(run => (Func<Guid, XElement>)(id => new XElement("tool_bar_item",
                new XAttribute("guid", id),
                Bitmap(file, face).ToSeq(),
                text.Of("text", face.Caption),
                new XElement("left_macro_id", MacroId(file, run.Invoke)),
                run.Alternate.Map(alternate => new XElement("right_macro_id", MacroId(file, alternate.Invoke))).ToSeq(),
                link.Map(bar => new XElement("link", new XAttribute("style", "normal"), BarId(file, bar))).ToSeq())));

    private static XElement Spacer(Guid id) => new("tool_bar_item", new XAttribute("guid", id), new XAttribute("button_style", "spacer"));

    private static XElement MacroItem(Guid file, Translations text, Macro macro, CommandFace face) =>
        new("macro_item",
            new XAttribute("guid", MacroId(file, macro)),
            Bitmap(file, face).ToSeq(),
            text.Of("text", face.Caption),
            face.Help.Map(help => text.Of("tooltip", help)).ToSeq(),
            face.Help.Map(help => text.Of("help_text", help)).ToSeq(),
            text.Of("button_text", face.Caption),
            text.Of("menu_text", face.Caption),
            new XElement("script", macro.Text));

    // --- [ICONS]
    private static Option<XAttribute> Bitmap(Guid file, CommandFace face) =>
        face.Icon.Map(art => new XAttribute("bitmap_id", IconId(file, art.Id)));

    private static Fin<XElement> Icon(Guid file, IconResource art) =>
        from light in Svg(art.Assembly, art.Id).ToFin(new RhinoFault(art.Id, RhinoReason.Missing))
        select new XElement("icon",
            new XAttribute("guid", IconId(file, art.Id)),
            new XAttribute("name", art.Id),
            new XElement("light", light),
            Svg(art.Assembly, Path.GetExtension(art.Id) switch { var extension => $"{art.Id[..^extension.Length]}_darkmode{extension}" }).Map(static held => new XElement("dark", held)).ToSeq());

    private static Option<XElement> Svg(Assembly assembly, string key) {
        using Stream? stream = assembly.GetManifestResourceStream(key);
        return Optional(stream).Map(XElement.Load);
    }

    // --- [MENUS]
    private static XElement Extended(Guid file, Translations text, Assembly plugIn, Seq<MenuPlacement> placed) =>
        new("menu", new XAttribute("guid", Toolbars.MenuId(file, None)),
            text.Of("text", Optional(plugIn.GetCustomAttribute<AssemblyTitleAttribute>()).Map(static title => title.Title).IfNone(() => plugIn.GetName().Name ?? "")),
            from placement in placed
            from path in placement.Path.ToSeq()
            from entry in placement.Entries
            select Entry(file, text, entry, Seq(new XAttribute("ExtendRhinoMenu", path)) + Platforms(placement.Platforms)));

    private static XElement Menu(Guid file, Translations text, MenuPlacement placement) =>
        new("menu", new XAttribute("guid", placement.Menu), Platforms(placement.Platforms), text.Of("text", placement.Face.Caption),
            placement.Entries.Map(entry => Entry(file, text, entry, [])));

    private static XElement Entry(Guid file, Translations text, MenuEntry entry, Seq<XAttribute> placed) {
        XElement root = Shell(entry, placed);
        SuperEnumerable.Consume(SuperEnumerable.TraverseDepthFirst((Entry: entry, Element: root), current =>
            (current.Entry is MenuEntry.Submenu menu ? menu.Children : []).Map(child => {
                XElement element = Shell(child, []);
                current.Element.Add(element);
                return (Entry: child, Element: element);
            })));
        return root;

        XElement Shell(MenuEntry row, Seq<XAttribute> attributes) => row.Switch(
            (File: file, Text: text, Placed: attributes),
            separator: static (at, separator) => new XElement("menu_item", new XAttribute("guid", separator.Id), at.Placed, new XAttribute("item_type", "separator")),
            item: static (at, item) => new XElement("menu_item", new XAttribute("guid", item.Id), at.Placed, at.Text.Of("text", item.Run.Face.Caption), new XElement("macro_id", MacroId(at.File, item.Run.Invoke))),
            submenu: static (at, submenu) => new XElement("menu", new XAttribute("guid", submenu.Id), at.Placed, at.Text.Of("text", submenu.Menu.Face.Caption)));
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static int Main(string[] args) =>
        (from request in IO.lift(Fin<(string PlugIn, string Rui, string References)> () => args is [var plugIn, var rui, var references]
                ? (plugIn, rui, references)
                : Error.New("Pass the plug-in assembly, its RUI path, and the file listing its reference assembly paths"))
         from references in IO.liftAsync(env => File.ReadAllLinesAsync(request.References, env.Token))
         from written in RuiFiles.Write(request.PlugIn, request.Rui, toSeq(references))
         select written).RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => {
                Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
                return 1;
            });
}
