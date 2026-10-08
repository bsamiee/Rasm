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

    // --- [MENUS]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(ToolbarFileRow row) =>
        (_, sink) =>
            from live in IO.lift(static () => Atom(true))
            let released = new Disposal<Atom<bool>>(live, static held => held.Swap(static _ => false))
            from registered in DisposalOps.OnFailure(
                IO.lift(() => Callbacks.Each(
                    row.MenuItems,
                    item => RuiUpdateUi.RegisterMenuItem(row.Id, item.Menu, item.Item, Handler(item, live, sink)),
                    nameof(RuiUpdateUi.RegisterMenuItem))),
                IO.lift(released.Dispose))
            select (IDisposable)released;

    private static RuiUpdateUi.UpdateMenuItemEventHandler Handler(MenuItemRow item, Atom<bool> live, IPlugInSink sink) =>
        Callbacks.Handler<RuiUpdateUi>(
            ui => Updated(item, ui, live, sink),
            new CallbackSite(sink, typeof(RuiUpdateUi), nameof(RuiUpdateUi.UpdateMenuItemEventHandler))).Invoke;

    private static IO<Unit> Updated(MenuItemRow item, RuiUpdateUi ui, Atom<bool> live, IPlugInSink sink) =>
        live.ValueIO.Bind(on => when(
            on,
            from document in IO.lift(static () => Optional(RhinoDoc.ActiveDoc))
            from scope in RowScope.Open(sink, document, new CommitMode.Immediate())
            from enabled in RowRules.Holds(item.Row.Enabled, scope)
            let caption = item.Row.Face.Wording.Shown(item.Row.Face.Caption, sink)
            from marked in item.Mark(scope, ui)
            from written in IO.lift(() => { (ui.Enabled, ui.Text) = (enabled, caption); })
            select unit).As());
}
