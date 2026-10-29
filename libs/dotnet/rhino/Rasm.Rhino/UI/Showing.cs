using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
using Rhino.UI.Forms;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PageTarget(string EnglishTitle, PageDialog Dialog) {
    public static PageTarget Of(View.Page row) => new(row.Caption, row.Kind);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Showing {
    // --- [WINDOWS]
    public static IO<Form> Show(View.Window row, RhinoDoc doc) =>
        IO.lift(() => toSeq(EtoExtensions.WindowsFromDocument<HostedWindow>(doc)).Find(open => open.Row.Identity == row.Identity)).Bind(open => open.Match(
            Some: static found => IO.lift(found.BringToFront).Map(_ => (Form)found),
            None: () =>
                from window in IO.lift(() => new HostedWindow(row, doc))
                from prepared in Products.Transfer(window, held => Opened(held, doc).Scoped(held.Hosted.Scope))
                from shown in IO.lift(() => window.Show(doc))
                select (Form)window));

    private static IO<Unit> Opened(HostedWindow window, RhinoDoc doc) =>
        from held in window.Hosted.Realizing
        from main in IO.lift(() => Conversions.Required(RhinoEtoApp.MainWindowForDocument(doc)))
        from icon in window.Row.Icon.TraverseM(art => Icons.Themed(art, IconSlot.Small, window, frames => window.Icon = frames)).As()
        from configured in IO.lift(() => {
            window.Title = LocalText.Of(window.Row.Caption).Local;
            window.Content = held.Frame;
            window.UseRhinoStyle();
            window.ClientSize = window.Row.ClientSize.IfNone(() => Size.Ceiling(held.Frame.GetPreferredSize()));
            window.Location = Eto.Drawing.Point.Round((PointF)main.Bounds.Center - ((SizeF)window.Size / 2f));
            window.LocalizeAndRestore(window.Row.Identity);
        })
        from edges in ViewOps.WhileLoaded(window, held.Body.Showing)
        select unit;

    // --- [DIALOGS]
    public static IO<Suppressible<Unit>> ShowSemiModal(View.Dialog row, RhinoDoc doc) =>
        row.Suppression.Match(
            Some: setting => HostDialogs.Suppressing(setting, box => Presented(row, doc, Some(box))),
            None: () => Presented(row, doc, None).Map<Suppressible<Unit>>(static answer => new Suppressible<Unit>.Shown(answer.Value)));

    private static IO<(Unit Value, bool Suppress)> Presented(View.Dialog row, RhinoDoc doc, Option<CheckBox> box) =>
        (from attended in HostDialogs.Attended
         from dialog in use(static () => new CommandDialog())
         from body in ViewBody.Open(row, Some(doc), new CommitMode.Deferred(ValueSet.Empty), None)
         let scope = body.Scope
         let bindings = body.Group
         from configured in IO.lift(() => {
             dialog.Title = LocalText.Of(row.Caption).Local;
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
         from help in row.Help.Traverse(topic => Subscriptions.On<EventArgs>(h => dialog.HelpButtonClick += h, h => dialog.HelpButtonClick -= h).Attach(_ => topic.Show)).As()
         from accepted in IO.lift(() => Results.Ended(dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindowForDocument(doc))))
             .Catch(static error => error.Is(Errors.Cancelled), error => scope.Revert(bindings).Bind(_ => IO.fail<Unit>(error)))
         from committed in row.Store.Match(
             Some: store => store.Switch(
                 (Doc: doc, Name: LocalText.Of(row.Caption).Local, Apply: scope.Apply(bindings)),
                 applicationStores: static (state, _) => state.Apply,
                 documentStores: static (state, held) => Commits.Commit(state.Doc, state.Name, held.Redraw, state.Apply).Map(static done => done.Value)),
             None: () => scope.Apply(bindings))
         select (Value: unit, Suppress: box.Exists(static held => held.Checked is true))).Bracket();

    // --- [PAGES]
    public static IO<Option<Window>> WindowForPage(OptionsDialogPage page) =>
        IO.lift(() => (PageDialog.TryGet(page.OptionsPageType, out PageDialog? dialog) ? Optional(dialog) : None).Bind(held => Optional(held.WindowForPage(page))));

    public static IO<Unit> OpenPage(PageTarget target, RhinoDoc doc) =>
        from name in IO.lift(() => Conversions.Required(RhinoGet.StringToCommandOptionName(target.EnglishTitle)))
        from opened in Documents.RunScript(doc, $"! _{target.Dialog.Command} _{name}", echo: false, display: None)
        select opened;

    // --- [SCRIPTS]
    internal static IO<Unit> RunScript(View view, RowScope scope, RhinoDoc doc) =>
        from groups in Products.Each(ViewCatalog.Tree(view).Map(held => ViewOps.Bindings(held, scope)))
        let bindings = new BindingGroup(groups.Bind(static bound => bound.Bindings))
        let fields = bindings.Keys().Map(static key => (Caption: EntryKey.Name(key.Path), Key: key))
        let pass =
            from shown in scope.Shown(bindings)
            from ended in Getters.Choice(doc, new GetterRequest<GetOption, bool>(view.Caption) {
                Accept = new() { Nothing = Some(new NothingEntry<bool>(None, IO.pure(value: true))) },
                Options =
                    from field in fields
                    let text = shown.Entries.Find(field.Key)
                    select OptionSpecs.Plain(
                        field.Caption,
                        text.Bind(static held => Conversions.Present(held)),
                        hidden: false,
                        _ => Getters.Text(
                            doc,
                            new EntryRequest<GetString, string, bool>(field.Caption) { Default = text },
                            literal: true,
                            entered => scope.Commit(bindings, new ValueSet(HashMap((field.Key, entered.Value)))).Map(value: false))),
            })
            select ended
        from ended in pass.RepeatUntil(static ended => ended)
        select unit;

    // --- [PICKS]
    public static IO<T> PushPickButton<T>(Window window, IO<T> pick) => Picked(window.PushPickButton, pick);

    public static IO<T> PushPickButton<T>(Panel panel, IO<T> pick) => Picked(panel.PushPickButton, pick);

    private static IO<A> Picked<A>(Action<EventHandler<EventArgs>> push, IO<A> pick) =>
        cancelToken.Bind(token => Products.Captured<A>(answer => IO.lift(() => push((_, _) => answer(Session.RunSafe(pick, token))))));
}
