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
}
