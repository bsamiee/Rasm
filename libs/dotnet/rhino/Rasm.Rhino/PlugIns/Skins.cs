using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Splash(Form Window, Label Status, ProgressBar Bar);

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract partial class DefinedSkin<TSelf> : Skin where TSelf : DefinedSkin<TSelf>, IDefined<SkinDefinition> {
    // --- [STATE]
    [Union]
    private abstract partial record SkinLoad {
        public SkinLoad Begin(string plugIn) =>
            Switch<string, SkinLoad>(
                plugIn,
                starting: static (_, starting) => starting,
                loading: static (begun, loading) => loading with { Begun = loading.Begun.Add(begun) },
                ready: static (_, ready) => ready);

        public SkinLoad End() =>
            Switch<SkinLoad>(
                starting: static starting => starting,
                loading: static loading => loading with { Ended = loading.Ended + 1 },
                ready: static ready => ready);

        public Option<int> Percent =>
            Switch(
                starting: static _ => Option<int>.None,
                loading: static loading => Some(Math.Min(loading.Ended * 100 / loading.Expected, 100)),
                ready: static _ => Option<int>.None);

        public sealed record Starting : SkinLoad;

        public sealed record Loading(int Expected, Seq<string> Begun, int Ended) : SkinLoad;

        public sealed record Ready : SkinLoad;
    }

    private readonly Atom<SkinLoad> load = Atom<SkinLoad>(new SkinLoad.Starting());
    private readonly Ref<Option<Owned<Splash>>> splash = Ref(Option<Owned<Splash>>.None);

    protected DefinedSkin() =>
        _ = Session.Run(Session.Bind(this, TSelf.Definition.Logging).Bind(static _ => Subscriptions.Inline<EventHandler, EventArgs>(static h => RhinoApp.Closing += h, static h => RhinoApp.Closing -= h, static handler => handler.Invoke)
            .Attach(static _ => Session.Close).Scoped(Session.Scope)));

    // --- [IDENTITY]
    protected sealed override string ApplicationName =>
        TSelf.Definition.ApplicationName.IfNone(static () => typeof(TSelf).Assembly switch {
            var assembly => Optional(assembly.GetCustomAttribute<AssemblyTitleAttribute>()).Map(static title => title.Title).IfNone(() => assembly.GetName().Name ?? typeof(TSelf).Name),
        });

    protected sealed override System.Drawing.Bitmap? MainRhinoIcon =>
        HostUtils.RunningOnWindows
            ? Session.Run(Icons.PlugInImage().Bind(static glyph => glyph.Traverse(static art => Icons.Raster(art, new System.Drawing.Size(256, 256))).As()).Map(static bitmap => bitmap.ValueUnsafe()), failed: null)
            : null;

    private static IO<Option<Control>> Mark =>
        TSelf.Definition.Splash.Match(
            Some: static content => content.Map(static control => Some(control)),
            None: static () =>
                from glyph in Icons.PlugInImage()
                from view in glyph.TraverseM(static art => Icons.Frames(art, new Size(128, 128)).Bind(static icon => use(() => new ImageView { Image = icon }))).As()
                select view.Map(static image => (Control)image));

    // --- [LOAD]
    protected sealed override void OnBeginLoadAtStartPlugIns(int expectedCount) =>
        _ = Session.Run(Advanced(current => expectedCount > 0 ? new SkinLoad.Loading(expectedCount, Seq<string>(), Ended: 0) : current));

    protected sealed override void OnBeginLoadPlugIn(string description) => _ = Session.Run(Advanced(current => current.Begin(description)));

    protected sealed override void OnEndLoadPlugIn() =>
        _ = Session.Run(Advanced(static current => current.End()).Bind(static next => IO.lift(() => next.Percent.Iter(SetBootSplashProgress))));

    protected sealed override void OnEndLoadAtStartPlugIns() => _ = Session.Run(Advanced(static _ => new SkinLoad.Ready()).Bind(static _ => IO.lift(DismissBootSplash)));

    private IO<SkinLoad> Advanced(Func<SkinLoad, SkinLoad> transition) =>
        from next in load.SwapIO(transition)
        from shown in IO.lift(() => {
            _ = splash.Value.Iter(lease => {
                Refresh(next, lease.Value.Status, lease.Value.Bar);
                Placed(lease.Value.Window, Size.Max(lease.Value.Window.ClientSize, Size.Ceiling(lease.Value.Window.Content.GetPreferredSize())));
            });
            Application.Instance.RunIteration();
        })
        select next;

    // --- [HOOKS]
    protected sealed override void OnMainFrameWindowCreated() => _ = Session.Run(TSelf.Definition.MainFrameCreated.IfNone(IO.pure(unit)));

    protected sealed override void OnLicenseCheckCompleted() => _ = Session.Run(TSelf.Definition.LicenseChecked.IfNone(IO.pure(unit)));

    protected sealed override void OnBuiltInCommandsRegistered() => _ = Session.Run(TSelf.Definition.CommandsRegistered.IfNone(IO.pure(unit)));

    // --- [SPLASH]
    protected sealed override void ShowSplash() =>
        _ = Session.Run(unless(HasBootSplash, Products.Replace(splash, Splashed).Scoped(Session.Scope).Bind(static _ => IO.lift(static () => Application.Instance.RunIteration()))).As());

    protected sealed override void HideSplash() => _ = Session.Run(Products.Clear(splash).Scoped(Session.Scope));

    private IO<Splash> Splashed =>
        from mark in Mark
        from status in use(static () => new Label { Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center })
        from bar in use(static () => new ProgressBar())
        from detail in use(() => new StackLayout(status, bar) {
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Dialog),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        })
        from content in Layout(mark, detail)
        from window in use(() => new Form {
            Title = ApplicationName, WindowStyle = WindowStyle.None, Resizable = false, Minimizable = false, Maximizable = false, ShowInTaskbar = false, Topmost = true,
            Content = content,
        }, static form => {
            form.Close();
            form.Content = null;
            form.Dispose();
        })
        from shown in IO.lift(() => {
            Refresh(load.Value, status, bar);
            window.UseRhinoStyle();
            Placed(window, Size.Ceiling(content.GetPreferredSize()));
            window.Show();
        })
        select new Splash(window, status, bar);

    // --- [HELP]
    protected sealed override void ShowHelp() =>
        _ = Session.Run(
            from mark in Mark
            from detail in TSelf.Definition.About.IfNone(static () =>
                use(static () => new Label { Text = LocalText.Of("Rhino {0}", arguments: [RhinoApp.Version]).Local, Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center })
                    .Map(static label => (Control)label))
            from content in Layout(mark, detail)
            from dialog in use(() => new Dialog { Title = ApplicationName, Resizable = false, Content = content }, static dialog => {
                dialog.Content = null;
                dialog.Dispose();
            })
            from shown in IO.lift(() => {
                dialog.UseRhinoStyle();
                dialog.ShowModal(RhinoEtoApp.MainWindow);
            })
            select shown);

    // --- [DOCUMENTS]
    protected sealed override void ShowChooseTemplate() =>
        _ = Session.Run(
            Chosen(
                from paths in HostDialogs.Ask(static () => new global::Rhino.UI.OpenFileDialog {
                    Title = LocalText.Of("New from Template").Local,
                    Filter = HostDialogs.Filter(Seq((LocalText.Of("Rhino Templates").Local, Seq(".3dm")))),
                    InitialDirectory = TSelf.Definition.TemplateFolder.IfNone(static () => FileSettings.TemplateFolder),
                } switch {
                    var dialog => (dialog.ShowOpenDialog(), toSeq(dialog.FileNames)),
                })
                from opened in paths.TraverseM(static path => IO.lift(() => Conversions.Required(OpenNewDocument(path)))).As()
                select unit));

    protected sealed override void ShowChooseRecent() =>
        _ = Session.Run(
            Chosen(
                from recent in TSelf.Definition.Recent.IfNone(static () => IO.lift(static () => toSeq(FileSettings.RecentlyOpenedFiles())))
                from files in IO.pure(recent.Map(static path => new ChoiceItem<string>(path, path)).AsIterableNE())
                from opened in files.TraverseM(Reopened).As()
                select unit));

    private static IO<Unit> Chosen(IO<Unit> choice) => choice | @catch(Errors.Cancelled, IO.pure(unit));

    private static IO<Unit> Reopened(IterableNE<ChoiceItem<string>> items) =>
        from path in HostDialogs.ShowListBox(LocalText.Of("Open Recent"), LocalText.Of("Choose a file to open"), items, None)
        from existing in IO.lift(() => InputPath.Of(path))
        from document in Documents.With(new DocumentSource.Opened(existing), static _ => IO.pure(unit))
        select document;

    // --- [LAYOUT]
    private static void Refresh(SkinLoad state, Label status, ProgressBar bar) {
        status.Text = state.Switch(
            starting: static _ => LocalText.Of("Starting"),
            loading: static loading => loading.Begun.Last.Match(
                Some: static plugIn => LocalText.Of("Loading {0}", arguments: [plugIn]),
                None: static () => LocalText.Of("Loading plug-ins")),
            ready: static _ => LocalText.Of("Preparing viewports")).Local;
        _ = state.Percent.Match(
            Some: percent => {
                bar.Indeterminate = false;
                bar.Value = percent;
            },
            None: () => bar.Indeterminate = true);
    }

    private IO<StackLayout> Layout(Option<Control> mark, Control detail) =>
        from heading in use(() => new Label { Text = ApplicationName, Font = EtoFonts.HeadingFont })
        from layout in use(() => new StackLayout([.. mark.ToSeq().Map<StackLayoutItem>(static control => control), heading, new StackLayoutItem(detail, HorizontalAlignment.Stretch)]) {
            Padding = RhinoLayout.Padding(RhinoLayout.PaddingType.Dialog),
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Dialog),
            HorizontalContentAlignment = HorizontalAlignment.Center,
        })
        select layout;

    private static void Placed(Form window, Size client) {
        window.ClientSize = client;
        window.Location = Eto.Drawing.Point.Round(Screen.PrimaryScreen.WorkingArea.Center - ((SizeF)window.Size / 2f));
    }
}
