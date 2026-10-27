using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Linq;
using Rasm.Rhino;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Rui;

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class RuiFormat {
    public const int MajorVersion = 5;
    public const int MinorVersion = 0;
    public const int English = 1033;
    public const string DarkSuffix = "_darkmode";
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class RuiFiles {
    // --- [RESOLUTION]
    public static IO<Assembly> Load(string plugIn, Seq<string> references) =>
        references.Fold(HashMap<string, string>(), static (held, path) => held.TryAdd(Path.GetFileNameWithoutExtension(path), path)) switch {
            var located => IO.lift(() => {
                AssemblyLoadContext.Default.Resolving += (context, name) => Optional(name.Name).Bind(located.Find).Map(context.LoadFromAssemblyPath).ValueUnsafe();
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(plugIn));
            }),
        };

    // --- [DOCUMENT]
    public static IO<Option<XDocument>> Document(Assembly plugIn) =>
        IO.lift(() => Toolbars.Declared(plugIn).Traverse(declared =>
            from file in Toolbars.Identity(plugIn)
            let toolbars = Toolbars.Reached(declared.Toolbars)
            let macros = toSeq(Toolbars.Macros(declared.Toolbars.Bind(static toolbar => toolbar.Groups) + declared.Menus.Bind(static menu => menu.Groups))
                .DistinctBy(held => Toolbars.MacroId(file, held.Macro)))
            let arts = toSeq((toolbars.Choose(static toolbar => toolbar.Face.Icon) + macros.Choose(static held => held.Face.Icon)).DistinctBy(static art => art.Id, StringComparer.Ordinal))
            from icons in arts.Traverse(art => Icon(file, art)).As()
            from bars in toolbars.Traverse(toolbar => Bar(file, toolbar)).As()
            from items in macros.Traverse(held => MacroItem(file, held.Macro, held.Face)).As()
            select new XDocument(new XElement("RhinoUI",
                new XAttribute("major_ver", RuiFormat.MajorVersion), new XAttribute("minor_ver", RuiFormat.MinorVersion), new XAttribute("guid", file), new XAttribute("plug_in_guid", file),
                new XAttribute("localize", bool.FalseString), new XAttribute("default_language_id", RuiFormat.English),
                new XElement("extend_rhino_menus", declared.Menus.IsEmpty ? Seq<XElement>() : Seq(Extended(file, plugIn, Toolbars.Extensions(file, declared.Menus)))),
                new XElement("menus"),
                new XElement("tool_bar_groups"),
                new XElement("tool_bars", bars),
                new XElement("macros", items),
                new XElement("icons", icons),
                new XElement("bitmaps")))).As());

    public static IO<Unit> Write(Option<XDocument> document, string path) =>
        IO.lift(() => document.Match(Some: held => held.Save(path), None: () => File.Delete(path)));

    // --- [ELEMENTS]
    private static XElement Text(string name, string value) => new(name, new XElement($"locale_{RuiFormat.English}", value));

    private static Fin<XElement> Bar(Guid file, ToolbarRow toolbar) =>
        from groups in toolbar.Groups.Traverse(cluster => cluster.Traverse(command => Button(file, command)).As()).As()
        select new XElement("tool_bar",
            new XAttribute("guid", Toolbars.BarId(file, toolbar)),
            Bitmap(file, toolbar.Face).ToSeq(),
            toolbar.Display == ButtonFace.Icon ? Seq<XAttribute>() : Seq(new XAttribute("item_display_style", toolbar.Display.Key)),
            Text("text", toolbar.Face.Caption),
            groups.Intersperse(Seq(Spacer)).Flatten().Map((item, index) => item(Toolbars.ItemId(Toolbars.BarId(file, toolbar), "tool_bar_item", index))));

    private static Fin<Func<Guid, XElement>> Button(Guid file, Command<Macro> command) =>
        command.Switch(
            file,
            run: static (at, run) => Pressed(at, run.Face, run.Enabled, Some(run), None),
            choice: static (at, choice) => Pressed(at, choice.Face, choice.Enabled, None, None),
            menu: static (at, menu) => Pressed(at, menu.Face, menu.Enabled, menu.Primary, Some(new ToolbarRow(menu.Face, menu.Groups))));

    private static Fin<Func<Guid, XElement>> Pressed(Guid file, CommandFace face, Option<RowRule> gate, Option<Command<Macro>.Run> pressed, Option<ToolbarRow> link) =>
        pressed.Filter(run => gate.IsNone && run.Enabled.IsNone && run.Marked.IsNone)
            .ToFin(new RhinoFault(face.Name, RhinoReason.Unheld))
            .Map(run => (Func<Guid, XElement>)(id => new XElement("tool_bar_item",
                new XAttribute("guid", id),
                Bitmap(file, face).ToSeq(),
                Text("text", face.Caption),
                new XElement("left_macro_id", Toolbars.MacroId(file, run.Invoke)),
                run.Alternate.Map(alternate => new XElement("right_macro_id", Toolbars.MacroId(file, alternate))).ToSeq(),
                link.Map(bar => new XElement("link", new XAttribute("style", "normal"), Toolbars.BarId(file, bar))).ToSeq())));

    private static XElement Spacer(Guid id) => new("tool_bar_item", new XAttribute("guid", id), new XAttribute("button_style", "spacer"));

    private static Fin<XElement> MacroItem(Guid file, Macro macro, CommandFace face) =>
        new XElement("macro_item",
            new XAttribute("guid", Toolbars.MacroId(file, macro)),
            Bitmap(file, face).ToSeq(),
            Text("text", face.Caption),
            face.Help.Map(static help => Text("tooltip", help)).ToSeq(),
            Text("menu_text", face.Caption),
            new XElement("script", macro.Text));

    // --- [ICONS]
    private static Option<XAttribute> Bitmap(Guid file, CommandFace face) =>
        face.Icon.Map(art => new XAttribute("bitmap_id", Toolbars.IconId(file, art.Id)));

    private static Fin<XElement> Icon(Guid file, IconResource art) =>
        from light in Svg(art.Assembly, art.Id)
        from dark in toSeq(art.Assembly.GetManifestResourceNames()).Find(name => string.Equals(name, Dark(art.Id), StringComparison.Ordinal)).Traverse(name => Svg(art.Assembly, name)).As()
        select new XElement("icon",
            new XAttribute("guid", Toolbars.IconId(file, art.Id)),
            new XAttribute("name", art.Id),
            new XElement("light", light),
            dark.Map(static held => new XElement("dark", held)).ToSeq());

    private static string Dark(string resource) =>
        Path.GetExtension(resource) switch { var extension => $"{resource[..^extension.Length]}{RuiFormat.DarkSuffix}{extension}" };

    private static Fin<XElement> Svg(Assembly assembly, string key) {
        using Stream? stream = assembly.GetManifestResourceStream(key);
        return Optional(stream).Map(XElement.Load).ToFin(new RhinoFault(key, RhinoReason.Missing));
    }

    // --- [MENUS]
    private static XElement Extended(Guid file, Assembly plugIn, Seq<MenuPlacement> placed) =>
        new("menu", new XAttribute("guid", Toolbars.MenuId(file)), Text("text", $"{plugIn.GetName().Name}"),
            from placement in placed
            from entry in placement.Entries
            select Entry(file, entry, Seq(new XAttribute("ExtendRhinoMenu", placement.Path))));

    private static XElement Entry(Guid file, MenuEntry entry, Seq<XAttribute> placed) =>
        entry.Switch(
            (File: file, Placed: placed),
            separator: static (at, separator) => new XElement("menu_item", new XAttribute("guid", separator.Id), at.Placed, new XAttribute("item_type", "separator")),
            item: static (at, item) => new XElement("menu_item", new XAttribute("guid", item.Id), at.Placed, Text("text", item.Run.Face.Caption), new XElement("macro_id", Toolbars.MacroId(at.File, item.Run.Invoke))),
            submenu: static (at, submenu) => new XElement("menu", new XAttribute("guid", submenu.Id), at.Placed, Text("text", submenu.Menu.Face.Caption),
                submenu.Children.Map(child => Entry(at.File, child, []))));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal static class Program {
    public static int Main(string[] args) =>
        (from request in IO.lift(Fin<(string PlugIn, string Rui, string References)> () => args is [var plugIn, var rui, var references]
                ? (plugIn, rui, references)
                : Error.New("Pass the plug-in assembly, its RUI path, and the file listing its reference assembly paths"))
         from references in IO.liftAsync(env => File.ReadAllLinesAsync(request.References, env.Token))
         from plugIn in RuiFiles.Load(request.PlugIn, toSeq(references))
         from document in RuiFiles.Document(plugIn)
         from written in RuiFiles.Write(document, request.Rui)
         select written).RunSafe().Match(
            Succ: static _ => 0,
            Fail: static error => {
                Console.Error.WriteLine(string.Join('\n', error.AsIterable().Map(static leaf => leaf.IsExceptional ? leaf.ToException().ToString() : leaf.Message)));
                return 1;
            });
}
