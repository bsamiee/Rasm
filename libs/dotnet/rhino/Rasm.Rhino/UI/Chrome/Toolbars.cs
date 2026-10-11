using System.Runtime.CompilerServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.PlugIns;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI.Chrome;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ToolbarState(Guid Id, Option<string> Name);

public sealed record ToolbarGroupState(Guid Id, Option<string> Name, bool Visible, bool IsDocked);

public sealed record ToolbarFileState(Guid Id, Option<string> Name, string Path, Seq<ToolbarGroupState> Groups, Seq<ToolbarState> Toolbars);

[Union]
public abstract partial record MenuItemRow {
    private MenuItemRow(Guid menu, Guid item) => (Menu, Item) = (menu, item);

    public Guid Menu { get; }
    public Guid Item { get; }

    public abstract CommandRow Row { get; }

    public abstract IO<Unit> Mark(RowScope scope, RuiUpdateUi ui);

    public sealed record Run(Guid Menu, Guid Item, CommandRow.Run Command) : MenuItemRow(Menu, Item) {
        public override CommandRow Row => Command;
        public override IO<Unit> Mark(RowScope scope, RuiUpdateUi ui) => IO.pure(unit);
    }

    public sealed record Checked(Guid Menu, Guid Item, CommandRow.Check Check) : MenuItemRow(Menu, Item) {
        public override CommandRow Row => Check;
        public override IO<Unit> Mark(RowScope scope, RuiUpdateUi ui) => Check.Read(scope).Bind(on => IO.lift(() => { ui.Checked = on; }));
    }

    public sealed record Choice(Guid Menu, Guid Item, CommandRow.Check Member) : MenuItemRow(Menu, Item) {
        public override CommandRow Row => Member;
        public override IO<Unit> Mark(RowScope scope, RuiUpdateUi ui) => Member.Read(scope).Bind(on => IO.lift(() => { ui.RadioChecked = on; }));
    }
}

public sealed record ToolbarFileRow(Guid Id, Seq<MenuItemRow> MenuItems);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class ToolbarMapper {
    [MapProperty(nameof(ToolbarFile.Path), nameof(ToolbarFileState.Path), SuppressNullMismatchDiagnostic = true)]
    [MapPropertyFromSource(nameof(ToolbarFileState.Groups), Use = nameof(Groups))]
    [MapPropertyFromSource(nameof(ToolbarFileState.Toolbars), Use = nameof(Toolbars))]
    internal static partial ToolbarFileState ToState(ToolbarFile file);

    internal static partial ToolbarGroupState ToState(ToolbarGroup group);

    internal static partial ToolbarState ToState(Toolbar toolbar);

    private static Seq<ToolbarGroupState> Groups(ToolbarFile file) =>
        Conversions.Rows(toSeq(Range(0, file.GroupCount)).Map(file.GetGroup)).Map(ToState).Strict();

    private static Seq<ToolbarState> Toolbars(ToolbarFile file) =>
        Conversions.Rows(toSeq(Range(0, file.ToolbarCount)).Map(file.GetToolbar)).Map(ToState).Strict();
}

public static class Toolbars {
    // --- [FILES]
    public static IO<Seq<ToolbarFileState>> Read() =>
        IO.lift(static () => Conversions.Rows(RhinoApp.ToolbarFiles).Map(ToolbarMapper.ToState).Strict());

    public static IO<ToolbarFileState> Find(Guid id) =>
        IO.lift(() => Conversions.Rows(RhinoApp.ToolbarFiles).Find(file => file.Id == id).Map(ToolbarMapper.ToState).ToFin(new Missing(nameof(RhinoApp.ToolbarFiles))));

    // --- [COMMANDS]
    internal static IO<Unit> RegisterCommand(HostCommand command) =>
        command.Toolbar.TraverseM(target =>
            from file in IO.lift(() => Callbacks.Found(
                TryGetRuiFile(GlobalInstance(owner: null), target.File.IfNone(static () => DefaultRuiId(owner: null)), out object found),
                found).ToFin(new Missing(nameof(TryGetRuiFile))))
            from toolbar in IO.lift(() => Callbacks.Found(
                TryGetToolbar(file, target.Toolbar, out object found),
                found).ToFin(new Missing(nameof(TryGetToolbar))))
            from existing in IO.lift(() => toSeq((IEnumerable<object>)Items(toolbar))
                .Map(static (item, index) => (Item: item, Index: index))
                .Find(held => Id(held.Item) == command.Id))
            from placed in IO.lift(() => {
                object text = LocaleTextFromString(owner: null, command.EnglishName);
                Text(text, command.LocalName);
                object item = NewItem(command.Id);
                object macro = NewMacro(command.Id);
                Name(item, text);
                MenuText(macro, text);
                TooltipText(macro, text);
                Script(macro, "! _" + command.EnglishName);
                Left(item, macro);
                return existing.Match(
                    Some: held => Refused.Unless(TryRemove(toolbar, held.Item), nameof(TryRemove))
                        .Bind(_ => Refused.Unless(TryInsertAt(toolbar, item, held.Index), nameof(TryInsertAt))),
                    None: () => Refused.Unless(TryAdd(toolbar, CreateSeparator(owner: null)), nameof(TryAdd))
                        .Bind(_ => Refused.Unless(TryAdd(toolbar, item), nameof(TryAdd))));
            })
            from refreshed in IO.lift(static () => RefreshToolbars(owner: null))
            select unit)
        .As()
        .Map(static _ => unit);

    // --- [MENUS]
    public static Func<PlugIn, IPlugInSink, Action<IDisposable>, IO<Unit>> Register(ToolbarFileRow row) =>
        (_, sink, retain) => DisposalOps.Acquire(acquired =>
            from live in DisposalOps.Acquire(IO.lift(static () => new CancellationTokenSource()), static held => IO.lift(() => {
                held.Cancel();
                held.Dispose();
            }), acquired)
            from registered in
                IO.lift(() => Callbacks.Each(
                    row.MenuItems,
                    item => RuiUpdateUi.RegisterMenuItem(row.Id, item.Menu, item.Item, Handler(item, sink, live.Token)),
                    nameof(RuiUpdateUi.RegisterMenuItem)))
            select unit, (_, held) => retain(held));

    private static RuiUpdateUi.UpdateMenuItemEventHandler Handler(MenuItemRow item, IPlugInSink sink, CancellationToken live) =>
        Callbacks.Handler<RuiUpdateUi>(
            ui => Updated(item, ui, sink, live),
            new CallbackSite(sink, typeof(RuiUpdateUi), nameof(RuiUpdateUi.UpdateMenuItemEventHandler))).Invoke;

    private static IO<Unit> Updated(MenuItemRow item, RuiUpdateUi ui, IPlugInSink sink, CancellationToken live) =>
        unless(IO.lift(() => live.IsCancellationRequested),
            from document in IO.lift(static () => Optional(RhinoDoc.ActiveDoc))
            from scope in RowScope.Open(sink, document, new CommitMode.Immediate())
            from enabled in RowRules.Holds(item.Row.Enabled, scope)
            let caption = item.Row.Face.Wording.Shown(item.Row.Face.Caption, sink)
            from marked in item.Mark(scope, ui)
            from written in IO.lift(() => { (ui.Enabled, ui.Text) = (enabled, caption); })
            select unit).As();

    // --- [RUI_ACCESSORS]
    private const string RuiCache = "Rhino.UI.Internal.TabPanels.IO.RuiCache, Rhino.UI";
    private const string RuiFile = "Rhino.UI.Internal.TabPanels.IO.RuiFile";
    private const string NewToolBar = "Rhino.UI.Internal.TabPanels.IO.Structures.NewToolBar";
    private const string ToolbarItem = "Rhino.UI.Internal.TabPanels.IO.Structures.ToolbarItem";
    private const string ToolbarMacro = "Rhino.UI.Internal.TabPanels.IO.Structures.ToolbarMacro, Rhino.UI";
    private const string LocaleText = "Rhino.UI.Internal.TabPanels.IO.LocaleText, Rhino.UI";
    private const string ToolbarNode = "Rhino.UI.Internal.TabPanels.IO.Structures.IToolbarNode, Rhino.UI";

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_GlobalInstance")]
    [return: UnsafeAccessorType(RuiCache)]
    private static extern object GlobalInstance([UnsafeAccessorType(RuiCache)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_DefaultRuiId")]
    private static extern Guid DefaultRuiId([UnsafeAccessorType(RuiFile + ", Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryGetRuiFile([UnsafeAccessorType(RuiCache)] object cache, Guid id, [UnsafeAccessorType(RuiFile + "&, Rhino.UI")] out object file);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryGetToolbar([UnsafeAccessorType(RuiFile + ", Rhino.UI")] object file, Guid id, [UnsafeAccessorType(NewToolBar + "&, Rhino.UI")] out object toolbar);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Items")]
    [return: UnsafeAccessorType("System.Collections.Generic.IReadOnlyList`1[[" + ToolbarItem + ", Rhino.UI]]")]
    private static extern object Items([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid Id([UnsafeAccessorType(ToolbarItem + ", Rhino.UI")] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryAdd([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar, [UnsafeAccessorType(ToolbarItem + ", Rhino.UI")] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryRemove([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar, [UnsafeAccessorType(ToolbarNode)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryInsertAt([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar, [UnsafeAccessorType(ToolbarNode)] object item, int index);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    [return: UnsafeAccessorType(ToolbarItem + ", Rhino.UI")]
    private static extern object CreateSeparator([UnsafeAccessorType(ToolbarItem + ", Rhino.UI")] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(ToolbarItem + ", Rhino.UI")]
    private static extern object NewItem(Guid id);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(ToolbarMacro)]
    private static extern object NewMacro(Guid id);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "op_Implicit")]
    [return: UnsafeAccessorType(LocaleText)]
    private static extern object LocaleTextFromString([UnsafeAccessorType(LocaleText)] object? owner, string text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Text")]
    private static extern void Text([UnsafeAccessorType(LocaleText)] object text, string value);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Name")]
    private static extern void Name([UnsafeAccessorType(ToolbarItem + ", Rhino.UI")] object item, [UnsafeAccessorType(LocaleText)] object text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MenuText")]
    private static extern void MenuText([UnsafeAccessorType(ToolbarMacro)] object macro, [UnsafeAccessorType(LocaleText)] object text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_TooltipText")]
    private static extern void TooltipText([UnsafeAccessorType(ToolbarMacro)] object macro, [UnsafeAccessorType(LocaleText)] object text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Script")]
    private static extern void Script([UnsafeAccessorType(ToolbarMacro)] object macro, string script);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Left")]
    private static extern void Left([UnsafeAccessorType(ToolbarItem + ", Rhino.UI")] object item, [UnsafeAccessorType(ToolbarMacro)] object macro);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void RefreshToolbars([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.OldCodeExtensions, Rhino.UI")] object? owner);
}
