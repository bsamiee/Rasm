using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Forms;

namespace Rasm.Rhino.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public enum TabVisibility { Hidden, Visible, Selected }

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<OptionsDialogPage.PageType>(SkipIComparable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PageDialog {
    public static readonly PageDialog Options = new(OptionsDialogPage.PageType.Options, "OptionsPage", RhinoEtoApp.ApplicationPreferencesWindowForPage);
    public static readonly PageDialog DocumentProperties = new(OptionsDialogPage.PageType.DocumentProperties, "DocumentPropertiesPage", RhinoEtoApp.DocumentPropertiesWindowForPage);

    public string Command { get; }

    [UseDelegateFromConstructor]
    public partial Window? WindowForPage(OptionsDialogPage page);
}

public sealed record PageTarget(string EnglishTitle, PageDialog Dialog) {
    public static PageTarget Of(View.OptionsPage row) => new(row.Caption, PageDialog.Options);

    public static PageTarget Of(View.DocumentPage row) => new(row.Caption, PageDialog.DocumentProperties);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Showing {
    // --- [WINDOWS]
    public static IO<TWindow> Show<TWindow>(RhinoDoc doc) where TWindow : DefinedWindow, new() =>
        IO.lift(() => toSeq(EtoExtensions.WindowsFromDocument<TWindow>(doc)).Head).Bind(open => open.Match(
            Some: static found => IO.lift(found.BringToFront).Map(found),
            None: () => DisposalOps.Acquire(owned =>
                from window in DisposalOps.Acquire(IO.lift(static () => new TWindow()), owned)
                from prepared in Opened(window, doc)
                select window, (window, _) => window.Show(doc))));

    private static IO<Unit> Opened<TWindow>(TWindow window, RhinoDoc doc) where TWindow : DefinedWindow =>
        from owner in IO.lift(() => Conversions.Cast<IPlugInViews>(IPlugInSink.Of(window)))
        from main in Appearance.InitialMainWindowPosition()
        from frame in DisposalOps.Acquire(IO.lift(static () => new ViewHost()), window.Owned.Retain)
        from body in DisposalOps.Acquire(ViewBody.Open(owner, window.Row, Some(doc), new CommitMode.Immediate(), None), frame.Owned.Retain)
        from icon in window.Row.Icon.TraverseM(glyph => Icons.Themed(owner, glyph, IconSlot.Small, HostTheme.In(window), frames => window.Icon = frames, window.Owned.Retain)).As()
        from seated in frame.Seat(window.Row, body.Content)
        from configured in IO.lift(() => {
            window.Title = RowText.Localize(window.Row.Caption, owner).Local;
            window.Content = frame;
            window.UseRhinoStyle();
            window.ClientSize = window.Row.Size.IfNone(() => Size.Ceiling(frame.GetPreferredSize()));
            _ = main.Iter(bounds => window.Location = Eto.Drawing.Point.Round(bounds.ToEtoScreen().Center - ((SizeF)window.Size / 2f)));
            window.LocalizeAndRestore(window.Row.Identity);
        })
        from edges in ViewOps.WhileLoaded(window, keep => DisposalOps.Acquire(body.Shown(visible: true), _ => body.Shown(visible: false), keep),
            CallbackSite.Of(window, nameof(window.LoadComplete)), window.Owned.Retain)
        select unit;

    // --- [DIALOGS]
    public static IO<Suppressible<Unit>> ShowSemiModal(IPlugInViews owner, View.Dialog row, RhinoDoc doc) =>
        row.Suppression.Match(
            Some: setting => HostDialogs.Suppressing(owner.Settings, setting, box => Presented(owner, row, doc, Some(box))),
            None: () => Presented(owner, row, doc, None).Map<Suppressible<Unit>>(static answer => new Suppressible<Unit>.Shown(answer.Value)));

    private static IO<(Unit Value, bool Suppress)> Presented(IPlugInViews owner, View.Dialog row, RhinoDoc doc, Option<CheckBox> box) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
            from attended in HostDialogs.Attended
            from dialog in DisposalOps.Acquire(IO.lift(static () => new CommandDialog()), retain)
            from body in DisposalOps.Acquire(ViewBody.Open(owner, row, Some(doc), new CommitMode.Deferred(ValueSet.Empty), None), retain)
            let scope = body.Scope
            let bindings = body.Group
            from configured in IO.lift(() => {
                dialog.Title = RowText.Localize(row.Caption, owner).Local;
                dialog.SavePosition = false;
                dialog.Buttons = (row.Cancelable, row.Help.IsSome) switch {
                    (true, true) => CommandDialog.ShowButtons.OKCancelHelp,
                    (true, false) => CommandDialog.ShowButtons.OKCancel,
                    (false, true) => CommandDialog.ShowButtons.CloseHelp,
                    (false, false) => CommandDialog.ShowButtons.Close,
                };
                dialog.Content = body.Content;
                _ = box.Iter(held => dialog.ButtonOptions = held);
                _ = row.DisplayMode.Iter(mode => dialog.DisplayMode = mode);
                dialog.UseRhinoStyle();
                dialog.LocalizeAndRestore(row.Identity);
            })
            from shown in body.Shown(visible: true)
            from help in row.Help.Traverse(topic => Subscriptions.Attach(
                handler => dialog.HelpButtonClick += handler, handler => dialog.HelpButtonClick -= handler,
                Callbacks.Handler<EventArgs>(_ => topic.Show, new CallbackSite(owner, row.Identity, nameof(CommandDialog.HelpButtonClick))), retain)).As()
            from result in IO.lift(() => dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindowForDocument(doc)))
            from accepted in IO.lift(Conversions.FromResult(result, nameof(EtoExtensions.ShowSemiModal)))
                .Catch(static error => error.Is(Errors.Cancelled), error => scope.Revert(bindings).Bind(_ => IO.fail<Unit>(error)))
            from committed in row.Store.Match(
                Some: store => store.Switch(
                    (Doc: doc, Name: RowText.Localize(row.Caption, owner).Local, Apply: scope.Apply(bindings)),
                    applicationStores: static (state, _) => state.Apply,
                    documentStores: static (state, held) => Commits.Commit(state.Doc, state.Name, held.Redraw, state.Apply).Map(static done => done.Value)),
                None: () => scope.Apply(bindings))
            select (Value: unit, Suppress: box.Exists(static held => held.Checked is true)), DisposalOps.Release);

    // --- [PANELS]
    public static IO<TabVisibility> Shown(View.Panel row) =>
        IO.lift(() => Panels.IsPanelVisible(row.Identity, isSelectedTab: true)
            ? TabVisibility.Selected
            : Panels.IsPanelVisible(row.Identity, isSelectedTab: false) ? TabVisibility.Visible : TabVisibility.Hidden);

    public static IO<Unit> Close(View.Panel row, RhinoDoc doc) =>
        IO.lift(() => Panels.ClosePanel(row.Identity.GUID, doc));

    public static IO<Unit> Open(View.Panel row) =>
        IO.lift(() => Panels.OpenPanel(row.Identity, makeSelectedPanel: true));

    public static IO<Unit> Toggle(View.Panel row, RhinoDoc doc) =>
        Shown(row).Bind(shown => shown is TabVisibility.Selected
            ? Close(row, doc)
            : Open(row));

    // --- [PAGES]
    public static IO<Option<Window>> WindowForPage(OptionsDialogPage page) =>
        IO.lift(() => PageDialog.Validated(page.OptionsPageType)
            .Map(dialog => Optional(dialog.WindowForPage(page))));

    public static IO<Unit> SetActivePageTo(StackedDialogPage page, PageTarget target) =>
        IO.lift(() => Refused.Unless(
            page.SetActivePageTo(target.EnglishTitle, target.Dialog == PageDialog.DocumentProperties),
            nameof(StackedDialogPage.SetActivePageTo)));

    public static IO<Unit> OpenPage(PageTarget target, RhinoDoc doc) =>
        from name in IO.lift(() => Refused.UnlessFound(RhinoGet.StringToCommandOptionName(target.EnglishTitle), nameof(RhinoGet.StringToCommandOptionName)))
        from opened in Documents.RunScript(doc, $"! _{target.Dialog.Command} _{name}", echo: false, display: None)
        select opened;

    // --- [SCRIPTS]
    public static IO<Unit> RunScript(IPlugInViews owner, View view, RowScope scope, RhinoDoc doc) =>
        from groups in ViewCatalog.Tree(view).Map<K<IO, BindingGroup>>(held => ViewOps.Bindings(owner, held, scope)).PartitionFallible().As()
        from bindings in groups.Fails.IsEmpty
            ? IO.pure(new BindingGroup(groups.Succs.Bind(static bound => bound.Bindings)))
            : IO.fail<BindingGroup>(Error.Many(groups.Fails))
        let site = new CallbackSite(owner, view.Identity, nameof(RunScript))
        let fields = ViewCatalog.Tree(view)
            .Bind(static held => held.Children)
            .Bind(static child => child.Controls)
            .Choose(static row => row.Keys is [var key] ? Some((row.Caption, Key: key)) : None)
        let pass =
            from shown in scope.Shown(bindings)
            from ended in Getters.Choice(doc, new GetterRequest<GetOption, bool>(view.Caption, site) {
                Accept = new() { Nothing = Some((Shown: Option<string>.None, Then: IO.pure(value: true))) },
                Options =
                    from field in fields
                    let text = shown.Entries.Find(field.Key)
                    select OptionSpec.Plain(
                        field.Caption,
                        text.Bind(static held => Conversions.Present(held)),
                        hidden: false,
                        _ => Getters.Text(
                            doc,
                            new EntryRequest<GetString, string, bool>(field.Caption, site) { Default = text },
                            literal: true,
                            entered => scope.Commit(bindings, new ValueSet(HashMap((field.Key, entered.Value)))).Map(value: false))),
            })
            select ended
        from ended in pass.RepeatUntil(static ended => ended)
        select unit;

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

    // --- [RENDER_PANELS]
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "FindExistingPanelData")]
    [return: UnsafeAccessorType("Rhino.Render.RenderPanelData, RhinoCommon")]
    private static extern object? FindExistingPanelData(RenderPanels? owner, Guid pluginId, Guid tabId);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SidePaneUiIdFromPanelObject")]
    private static extern Guid SidePaneUiIdFromPanelObject([UnsafeAccessorType("Rhino.Render.RenderPanelInstances, RhinoCommon")] object instances, object panelObject);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PanelObject")]
    private static extern object PanelObject([UnsafeAccessorType("Rhino.UI.PanelInstance, RhinoCommon")] object instance);

    // --- [PICKS]
    public static IO<T> PushPickButton<T>(Window window, IO<T> pick) => Picked(window.PushPickButton, pick);

    public static IO<T> PushPickButton<T>(Panel panel, IO<T> pick) => Picked(panel.PushPickButton, pick);

    private static IO<A> Picked<A>(Action<EventHandler<EventArgs>> push, IO<A> pick) =>
        cancelToken.Bind(token =>
            Callbacks.Captured<A>(answer => IO.lift(() => push((_, _) => answer(Callbacks.Settled(pick, token)))), nameof(EtoExtensions.PushPickButton)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed class PanelCommand(IPlugInSink sink, Guid id, View.Panel panel) : HostCommand(sink, id, RhinoGet.StringToCommandOptionName(panel.Caption), None) {
    protected override Option<HelpTopic> Help => panel.Help;

    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) => Showing.Toggle(panel, doc);
}
