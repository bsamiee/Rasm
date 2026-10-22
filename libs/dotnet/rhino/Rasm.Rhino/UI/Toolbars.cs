using System.Runtime.CompilerServices;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ToolbarGroupState(Guid Id, Option<string> Name, bool Visible, bool IsDocked);

public sealed record ToolbarFileState(Guid Id, Option<string> Name, string Path, Seq<ToolbarGroupState> Groups, Seq<NamedId> Toolbars);

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

    // --- [COMMANDS]
    private static readonly HostEvent<Unit> RuiFileAdded = new(typeof(Toolbars), RuiFileEvent, static (deliver, site, retain) =>
        Subscriptions.Attach(
            static heard => Refused.Unless(Listen(Queue(owner: null), RuiFileEvent, heard, static held => held(), priority: 1), nameof(Listen)).ThrowIfFail(),
            static heard => Ignore(Queue(owner: null), RuiFileEvent, heard),
            () => Callbacks.Started(deliver(unit), site),
            retain));

    public static IO<Unit> PlaceCommands(IPlugInViews plugIn, Action<IDisposable> retain) =>
        IO.lift(() => toSeq(plugIn.PlugIn.GetCommands().OfType<HostCommand>()).Filter(static command => command.Toolbar.IsSome).Strict())
            .Bind(commands => unless(commands.IsEmpty,
                from placed in Reconciled(commands)
                from heard in RuiFileAdded.Inline(_ => Reconciled(commands), plugIn, retain)
                select unit).As());

    private static IO<Unit> Reconciled(Seq<HostCommand> commands) =>
        commands.TraverseM(static command => command.Toolbar.TraverseM(target =>
            from file in IO.lift(() => Callbacks.Found(
                TryGetRuiFile(GlobalInstance(owner: null), target.File.IfNone(static () => DefaultRuiId(owner: null)), out object found),
                found))
            from placed in file.TraverseM(loaded => Placed(command, loaded, target.Toolbar)).As()
            select unit).As()).As().Map(unit);

    private static IO<Unit> Placed(HostCommand command, object file, Guid id) =>
        from toolbar in IO.lift(() => Callbacks.Found(
            TryGetToolbar(file, id, out object found),
            found).ToFin(new Refused(nameof(TryGetToolbar), Refused.Absent)))
        from existing in IO.lift(() => toSeq((IEnumerable<object>)Items(toolbar))
            .Map(static (item, index) => (Item: item, Index: index))
            .Find(held => Optional(PrivateLeft(held.Item)).Exists(macro => MacroId(macro) == command.Id)))
        from inserted in IO.lift(() => Inserted(command, toolbar, existing))
        from refreshed in IO.lift(static () => RefreshToolbars(owner: null))
        select unit;

    private static Fin<Unit> Inserted(HostCommand command, object toolbar, Option<(object Item, int Index)> existing) {
        object text = LocaleTextFromString(owner: null, command.EnglishName);
        Text(text, command.LocalName);
        object item = NewItem(existing.Map(static held => Id(held.Item)).IfNone(Guid.NewGuid));
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
    }

    // --- [MENUS]
    public static Func<IPlugInSink, Action<IDisposable>, IO<Unit>> Register(ToolbarFileRow row) =>
        (sink, retain) => DisposalOps.Acquire(acquired =>
            from live in DisposalOps.Acquire(IO.lift(static () => new CancellationTokenSource()), static held => IO.lift(() => {
                held.Cancel();
                held.Dispose();
            }), acquired)
            from registered in
                IO.lift(() => Callbacks.Each(
                    row.MenuItems,
                    item => RuiUpdateUi.RegisterMenuItem(row.Id, item.Menu, item.Item, Handler(item, sink, live.Token)),
                    nameof(RuiUpdateUi.RegisterMenuItem)))
            select unit, retain);

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
    private const string ToolbarItem = "Rhino.UI.Internal.TabPanels.IO.Structures.ToolbarItem, Rhino.UI";
    private const string ToolbarMacro = "Rhino.UI.Internal.TabPanels.IO.Structures.ToolbarMacro, Rhino.UI";
    private const string LocaleText = "Rhino.UI.Internal.TabPanels.IO.LocaleText, Rhino.UI";
    private const string ToolbarNode = "Rhino.UI.Internal.TabPanels.IO.Structures.IToolbarNode, Rhino.UI";
    private const string EventQueue = "Rhino.UI.Internal.Queues.EventQueue, Rhino.UI";
    private const string RuiFileEvent = "RuiFile";

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "get_GlobalInstance")]
    [return: UnsafeAccessorType(EventQueue)]
    private static extern object Queue([UnsafeAccessorType(EventQueue)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool Listen<TThing>([UnsafeAccessorType(EventQueue)] object queue, string eventKey, TThing instanceOfThing, Action<TThing> closure, int priority) where TThing : class;

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void Ignore([UnsafeAccessorType(EventQueue)] object queue, string eventKey, object instanceOfThing);

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
    [return: UnsafeAccessorType("System.Collections.Generic.IReadOnlyList`1[[" + ToolbarItem + "]]")]
    private static extern object Items([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid Id([UnsafeAccessorType(ToolbarItem)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivateLeft")]
    [return: UnsafeAccessorType(ToolbarMacro)]
    private static extern object? PrivateLeft([UnsafeAccessorType(ToolbarItem)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Id")]
    private static extern Guid MacroId([UnsafeAccessorType(ToolbarMacro)] object macro);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryAdd([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar, [UnsafeAccessorType(ToolbarItem)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryRemove([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar, [UnsafeAccessorType(ToolbarNode)] object item);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern bool TryInsertAt([UnsafeAccessorType(NewToolBar + ", Rhino.UI")] object toolbar, [UnsafeAccessorType(ToolbarNode)] object item, int index);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    [return: UnsafeAccessorType(ToolbarItem)]
    private static extern object CreateSeparator([UnsafeAccessorType(ToolbarItem)] object? owner);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(ToolbarItem)]
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
    private static extern void Name([UnsafeAccessorType(ToolbarItem)] object item, [UnsafeAccessorType(LocaleText)] object text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_MenuText")]
    private static extern void MenuText([UnsafeAccessorType(ToolbarMacro)] object macro, [UnsafeAccessorType(LocaleText)] object text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_TooltipText")]
    private static extern void TooltipText([UnsafeAccessorType(ToolbarMacro)] object macro, [UnsafeAccessorType(LocaleText)] object text);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Script")]
    private static extern void Script([UnsafeAccessorType(ToolbarMacro)] object macro, string script);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Left")]
    private static extern void Left([UnsafeAccessorType(ToolbarItem)] object item, [UnsafeAccessorType(ToolbarMacro)] object macro);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern void RefreshToolbars([UnsafeAccessorType("Rhino.UI.Internal.TabPanels.OldCodeExtensions, Rhino.UI")] object? owner);
}
