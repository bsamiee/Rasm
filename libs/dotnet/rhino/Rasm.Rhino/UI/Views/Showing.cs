using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Forms;

namespace Rasm.Rhino.UI.Views;

// --- [TYPES] ---------------------------------------------------------------------------
public enum TabVisibility { Hidden, Visible, Selected }

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<OptionsDialogPage.PageType>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public sealed partial class PageDialog {
    public static readonly PageDialog Options = new(OptionsDialogPage.PageType.Options, "OptionsPage", RhinoEtoApp.ApplicationPreferencesWindowForPage);
    public static readonly PageDialog DocumentProperties = new(OptionsDialogPage.PageType.DocumentProperties, "DocumentPropertiesPage", RhinoEtoApp.DocumentPropertiesWindowForPage);

    public string Command { get; }

    [UseDelegateFromConstructor]
    public partial Window? WindowForPage(OptionsDialogPage page);

    public static bool operator <(PageDialog left, PageDialog right) => Comparer<PageDialog>.Default.Compare(left, right) < 0;
    public static bool operator <=(PageDialog left, PageDialog right) => Comparer<PageDialog>.Default.Compare(left, right) <= 0;
    public static bool operator >(PageDialog left, PageDialog right) => Comparer<PageDialog>.Default.Compare(left, right) > 0;
    public static bool operator >=(PageDialog left, PageDialog right) => Comparer<PageDialog>.Default.Compare(left, right) >= 0;
}

public sealed record PageTarget(string EnglishTitle, PageDialog Dialog) {
    public static PageTarget Of(View.OptionsPage row) => new(row.Caption, PageDialog.Options);

    public static PageTarget Of(View.DocumentPage row) => new(row.Caption, PageDialog.DocumentProperties);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Showing {
    // --- [CALLBACKS]
    private const string IsTabVisible = "Rhino.UI.Internal.NamedCallbacks.RhinoUiIsTabVisible";
    private const string CloseDockbarTab = "Rhino.UI.Internal.NamedCallbacks.RhinoUiCloseDockbarTab";
    private const string FactoryEntry = "factoryId";
    private const string SelectedEntry = "isSelectedTab";
    private const string DocumentEntry = "documentSerialNumber";
    private const string VisibleEntry = "isVisible";

    // --- [WINDOWS]
    public static IO<TWindow> Show<TWindow>(RhinoDoc doc) where TWindow : DefinedWindow, new() =>
        from open in IO.lift(() => toSeq(EtoExtensions.WindowsFromDocument<TWindow>(doc)).Head)
        from window in open.Match(
            Some: static found => IO.lift(found.BringToFront).Map(_ => found),
            None: IO.lift(static () => new TWindow()).Bind(created => DisposalOps.OnFailure(Opened(created, doc), IO.lift(created.Dispose))))
        select window;

    private static IO<TWindow> Opened<TWindow>(TWindow window, RhinoDoc doc) where TWindow : DefinedWindow =>
        from owner in IO.lift(() => WrongType.Unless<IPlugInViews>(IPlugInSink.Of(window)))
        from main in Appearance.InitialMainWindowPosition()
        from view in RealizedView.Open(owner, window.Row, Some(doc), new CommitMode.Immediate(), None)
        from icon in DisposalOps.OnFailure(
            window.Row.Icon.TraverseM(glyph => Icons.Themed(owner, glyph, IconSlot.Window, frames => window.Icon = frames)).As(),
            IO.lift(view.Dispose))
        let release = Callbacks.Each(Seq(DisposalOps.Release(icon.ToSeq()), Released(view))).Map(static _ => unit)
        from shown in DisposalOps.OnFailure(
            IO.lift(() => {
                window.Title = RowText.Localize(window.Row.Caption, table: Some<object>(owner)).Local;
                window.Content = new RhinoScrollableDialogPanel { Content = view };
                window.UseRhinoStyle();
                window.ClientSize = Size.Ceiling(window.Content.GetPreferredSize());
                _ = main.Iter(bounds => window.Location = Eto.Drawing.Point.Round(bounds.ToEtoScreen().Center - (window.Size / 2f)));
                window.LocalizeAndRestore(window.Row.Identity);
                window.Show(doc);
                _ = view.Shown(visible: true);
            }),
            release)
        from closed in Subscriptions.Attach(
            handler => window.Closed += handler,
            handler => window.Closed -= handler,
            Callbacks.Handler<EventArgs>(static _ => release, new CallbackSite(owner, window.GetType(), nameof(window.Closed))))
        select window;

    private static IO<Unit> Released(RealizedView body) =>
        IO.lift(() => {
            _ = body.Shown(visible: false);
            body.Dispose();
        });

    // --- [DIALOGS]
    public static IO<Suppressible<Unit>> ShowSemiModal(IPlugInViews owner, View.Dialog row, RhinoDoc doc) =>
        row.Suppression.Match(
            Some: setting => HostDialogs.Suppressing(owner.Settings, setting, box => Presented(owner, row, doc, Some(box))),
            None: () => Presented(owner, row, doc, None).Map<Suppressible<Unit>>(static answer => new Suppressible<Unit>.Shown(answer.Value)));

    private static IO<(Unit Value, bool Suppress)> Presented(IPlugInViews owner, View.Dialog row, RhinoDoc doc, Option<CheckBox> box) =>
        (from attended in HostDialogs.Attended
         from view in RealizedView.Open(owner, row, Some(doc), new CommitMode.Deferred(ValueSet.Empty), None)
         let scope = view.Scope
         let bindings = view.Group
         from dialog in use(() => new CommandDialog {
             Title = RowText.Localize(row.Caption, table: Some<object>(owner)).Local,
             SavePosition = false,
             Buttons = (row.Cancelable, row.Help.IsSome) switch {
                 (true, true) => CommandDialog.ShowButtons.OKCancelHelp,
                 (true, false) => CommandDialog.ShowButtons.OKCancel,
                 (false, true) => CommandDialog.ShowButtons.CloseHelp,
                 (false, false) => CommandDialog.ShowButtons.Close,
             },
         })
         from answer in IO.pure(view).Bracket(
             Use: body =>
                 from framed in IO.lift(() => {
                     dialog.Content = body;
                     _ = body.Shown(visible: true);
                     _ = box.Iter(held => dialog.ButtonOptions = held);
                     _ = row.DisplayMode.Iter(mode => dialog.DisplayMode = mode);
                     _ = row.Help.Iter(topic => dialog.HelpButtonClick += Callbacks.Handler<EventArgs>(_ => topic.Show, new CallbackSite(owner, row.Identity, nameof(CommandDialog.HelpButtonClick))));
                     dialog.UseRhinoStyle();
                     dialog.LocalizeAndRestore(row.Identity);
                 })
                 from result in IO.lift(() => dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindowForDocument(doc)))
                 from accepted in IO.lift(Conversions.FromResult(result, nameof(EtoExtensions.ShowSemiModal)))
                     .Catch(static error => error.Is(Errors.Cancelled), error => scope.Revert(bindings).Bind(_ => IO.fail<Unit>(error)))
                 from committed in row.Store.Match(
                     Some: store => store.Switch(
                         (Doc: doc, Name: RowText.Localize(row.Caption, table: Some<object>(owner)).Local, Apply: scope.Apply(bindings)),
                         applicationStores: static (state, _) => state.Apply,
                         documentStores: static (state, held) => Commits.Commit(state.Doc, state.Name, held.Redraw, state.Apply).Map(static done => done.Value)),
                     None: () => scope.Apply(bindings))
                 select (Value: unit, Suppress: box.Exists(static held => held.Checked == true)),
             Fin: Released)
         select answer).Bracket();

    // --- [PANELS]
    public static IO<TabVisibility> Shown(View.Panel row, RhinoDoc doc) =>
        from selected in TabVisible(row, doc, isSelectedTab: true)
        from shown in selected
            ? IO.pure(TabVisibility.Selected)
            : TabVisible(row, doc, isSelectedTab: false).Map(static visible => visible ? TabVisibility.Visible : TabVisibility.Hidden)
        select shown;

    public static IO<Unit> Close(View.Panel row, RhinoDoc doc) =>
        NamedCallbacks.Execute(
            CloseDockbarTab,
            NamedCallbacks.Handled,
            args => args.Set(FactoryEntry, row.Identity.GUID),
            args => args.Set(DocumentEntry, doc.RuntimeSerialNumber));

    public static IO<Guid> OpenBeside(View.Panel row, Guid siblingPanelId, bool makeSelectedPanel) =>
        from dockBar in IO.lift(() => Conversions.Required(Panels.PanelDockBar(siblingPanelId), nameof(Panels.PanelDockBar)))
        from opened in IO.lift(() => Conversions.Required(Panels.OpenPanel(dockBar, row.Identity, makeSelectedPanel), nameof(Panels.OpenPanel)))
        select opened;

    public static IO<Unit> Toggle(View.Panel row, RhinoDoc doc) =>
        Shown(row, doc).Bind(shown => shown == TabVisibility.Selected
            ? Close(row, doc)
            : IO.lift(() => Panels.OpenPanel(row.Identity, makeSelectedPanel: true)));

    public static IO<Seq<TPanel>> GetPanels<TPanel>(RhinoDoc doc) where TPanel : DefinedPanel =>
        IO.lift(() => Conversions.Rows(Panels.GetPanels<TPanel>(doc)));

    private static IO<bool> TabVisible(View.Panel row, RhinoDoc doc, bool isSelectedTab) =>
        NamedCallbacks.Execute(
            IsTabVisible,
            static args => Callbacks.Found(args.TryGetBool(VisibleEntry, out bool visible), visible),
            args => args.Set(FactoryEntry, row.Identity.GUID),
            args => args.Set(SelectedEntry, isSelectedTab),
            args => args.Set(DocumentEntry, doc.RuntimeSerialNumber));

    // --- [PAGES]
    public static IO<Option<Window>> WindowForPage(OptionsDialogPage page) =>
        IO.lift(() => Conversions.Validated<PageDialog, OptionsDialogPage.PageType, InvalidRhinoValue>(page.OptionsPageType)
            .Map(dialog => Optional(dialog.WindowForPage(page))));

    public static IO<Unit> SetActivePageTo(StackedDialogPage page, PageTarget target) =>
        IO.lift(() => Refused.Unless(
            page.SetActivePageTo(target.EnglishTitle, target.Dialog == PageDialog.DocumentProperties),
            nameof(StackedDialogPage.SetActivePageTo)));

    public static IO<Unit> OpenPage(PageTarget target, RhinoDoc doc) =>
        from name in IO.lift(() => Missing.Unless(RhinoGet.StringToCommandOptionName(target.EnglishTitle), nameof(RhinoGet.StringToCommandOptionName)))
        from opened in Documents.RunScript(doc, $"! _{target.Dialog.Command} _{name}", echo: false, display: None)
        select opened;

    // --- [SCRIPTS]
    public static IO<Unit> RunScript(IPlugInViews owner, View view, RowScope scope, RhinoDoc doc) =>
        from bindings in IO.lift(ViewCollection.Tree(view).Traverse(held => ViewOps.Bindings(owner, held, scope)).As()
            .Map(static groups => groups.Bind(static bound => bound.Bindings))
            .Bind(BindingGroup.Of)
            .ToFin())
        let site = new CallbackSite(owner, view.Identity, nameof(RunScript))
        let fields = ViewCollection.Tree(view)
            .Bind(static held => held.Children)
            .Bind(static child => child.Controls)
            .Choose(static row => row.Switch<Option<(string Caption, EntryKey Key)>>(
                field: static field => field.Keys is [var key] ? (field.Caption, key) : None,
                readout: static _ => None,
                command: static _ => None,
                group: static _ => None))
        static from ended in pass.RepeatUntil(static ended => ended)
        static select unit;

    // --- [RENDERING]
    public static IO<Option<TBody>> TabFromRenderSessionId<TBody>(Guid renderSessionId) where TBody : DefinedTab =>
        IO.lift(() => Optional((TBody?)RenderTabs.FromRenderSessionId(PlugIn.Find(typeof(TBody).Assembly), typeof(TBody), renderSessionId)));

    public static IO<Option<TBody>> PaneFromRenderSessionId<TBody>(Guid renderSessionId) where TBody : DefinedPane =>
        IO.lift(() => Optional(RenderPanels.FromRenderSessionId(PlugIn.Find(typeof(TBody).Assembly), typeof(TBody), renderSessionId))
            .Map(static instance => (TBody)PanelObject(instance)));

    public static IO<Option<RenderWindow>> Window(DefinedTab tab) =>
        IO.lift(() => Conversions.Present(RenderTabs.SidePaneUiIdFromTab(tab)).Bind(static id => Optional(RenderWindow.FromSessionId(id))));

    public static IO<Option<RenderWindow>> Window(DefinedPane pane) =>
        IO.lift(() => Optional(FindExistingPanelData(owner: null, PlugIn.Find(pane.GetType().Assembly).Id, pane.GetType().GUID))
            .Bind(data => Conversions.Present(SidePaneUiIdFromPanelObject(data, pane)))
            .Bind(static id => Optional(RenderWindow.FromSessionId(id))));

    // --- [PANEL_INSTANCES]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FindExistingPanelData")]
    [return: UnsafeAccessorType("Rhino.Render.RenderPanelData, RhinoCommon")]
    private static extern object? FindExistingPanelData(RenderPanels? owner, Guid pluginId, Guid tabId);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SidePaneUiIdFromPanelObject")]
    private static extern Guid SidePaneUiIdFromPanelObject([UnsafeAccessorType("Rhino.Render.RenderPanelInstances, RhinoCommon")] object instances, object panelObject);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PanelObject")]
    private static extern object PanelObject([UnsafeAccessorType("Rhino.UI.PanelInstance, RhinoCommon")] object instance);

    // --- [PICKS]
    public static IO<A> PushPickButton<A>(Window window, IO<A> pick) => Picked(window.PushPickButton, pick);

    public static IO<A> PushPickButton<A>(Panel panel, IO<A> pick) => Picked(panel.PushPickButton, pick);

    private static IO<A> Picked<A>(Action<EventHandler<EventArgs>> push, IO<A> pick) =>
        Callbacks.Captured<A>(answer => IO.lift(() => push((_, _) => answer(Try.lift(pick.Run).Run()))), nameof(EtoExtensions.PushPickButton));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class PanelCommand(IPlugInSink sink, Guid id, View.Panel panel) : HostCommand(sink, id, RhinoGet.StringToCommandOptionName(panel.Caption), None) {
    protected override string CommandContextHelpUrl => panel.HelpUrl;

    protected override IO<Unit> RunAsync(RhinoDoc doc, RunMode mode, CallbackSite site) => Showing.Toggle(panel, doc);
}
