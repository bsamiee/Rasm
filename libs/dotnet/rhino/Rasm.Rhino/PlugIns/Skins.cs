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
using Platform = Eto.Platform;
using SkinMembers = (
    string ApplicationName,
    System.Drawing.Bitmap? MainRhinoIcon,
    System.Action<int> OnBeginLoadAtStartPlugIns,
    System.Action<string> OnBeginLoadPlugIn,
    System.Action OnEndLoadPlugIn,
    System.Action OnEndLoadAtStartPlugIns,
    System.Action OnMainFrameWindowCreated,
    System.Action OnLicenseCheckCompleted,
    System.Action OnBuiltInCommandsRegistered,
    System.Action ShowSplash,
    System.Action HideSplash,
    System.Action ShowHelp,
    System.Action ShowChooseTemplate,
    System.Action ShowChooseRecent);

namespace Rasm.Rhino.PlugIns;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record Splash(Form Window, Label Status, ProgressBar Bar);

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed partial class SkinBody<TDefinition> where TDefinition : IDefined<SkinDefinition> {

    // --- [STATE]

    [Union]
    private abstract partial record SkinLoad {
        public SkinLoad Begin(string plugIn) =>
            Switch<string, SkinLoad>(
                plugIn,
                starting: static (_, starting) => starting,
                loading: static (begun, loading) => loading with { Current = Some(begun) },
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

        public sealed record Loading(int Expected, Option<string> Current, int Ended) : SkinLoad;

        public sealed record Ready : SkinLoad;
    }

    private readonly Atom<SkinLoad> load = Atom<SkinLoad>(new SkinLoad.Starting());
    private readonly Ref<Option<Owned<Splash>>> splash = Ref(Option<Owned<Splash>>.None);

    private readonly Func<bool> hasBootSplash;
    private readonly Action<int> setBootSplashProgress;
    private readonly Action dismissBootSplash;
    private readonly Func<string, RhinoDoc?> openNewDocument;

    public SkinBody(Skin host, Func<bool> hasBootSplash, Action<int> setBootSplashProgress, Action dismissBootSplash, Func<string, RhinoDoc?> openNewDocument) {
        (this.hasBootSplash, this.setBootSplashProgress, this.dismissBootSplash, this.openNewDocument) = (hasBootSplash, setBootSplashProgress, dismissBootSplash, openNewDocument);
        Platform.Instance.LoadAssembly(typeof(SkinBody<TDefinition>).Assembly);
        _ = Session.RunSafe(
            Session.Bind(host, typeof(TDefinition), TDefinition.Definition.Logging).Bind(static _ => Subscriptions.Inline<EventHandler, EventArgs>(static h => RhinoApp.Closing += h, static h => RhinoApp.Closing -= h, static handler => handler.Invoke)
                .Attach(static _ => Session.Close).Scoped()),
            CancellationToken.None).ThrowIfFail();
    }

    // --- [MEMBERS]

    public SkinMembers Members =>
        (ApplicationName, MainRhinoIcon, OnBeginLoadAtStartPlugIns, OnBeginLoadPlugIn, OnEndLoadPlugIn, OnEndLoadAtStartPlugIns, OnMainFrameWindowCreated,
            OnLicenseCheckCompleted, OnBuiltInCommandsRegistered, ShowSplash, HideSplash, ShowHelp, ShowChooseTemplate, ShowChooseRecent);

    // --- [IDENTITY]

    private static string ApplicationName =>
        TDefinition.Definition.ApplicationName.IfNone(static () => typeof(TDefinition).Assembly switch {
            var assembly => Optional(assembly.GetCustomAttribute<AssemblyTitleAttribute>()).Map(static title => title.Title).IfNone(() => assembly.GetName().Name ?? typeof(TDefinition).Name),
        });

    private static System.Drawing.Bitmap? MainRhinoIcon =>
        IconResource.PlugIn(typeof(TDefinition).Assembly).Traverse(static art => IconSlot.Application.Square.Bind(pixels => Icons.Raster(art, pixels))).As().Map(static bitmap => bitmap.ValueUnsafe()).Run();

    private static IO<Option<Control>> Mark =>
        TDefinition.Definition.Splash.Match(
            Some: static content => content.Map(static control => Some(control)),
            None: static () =>
                IconResource.PlugIn(typeof(TDefinition).Assembly).TraverseM(static art => Icons.Frames(art, IconSlot.Splash).Bind(static icon => use(() => new ImageView { Image = icon })))
                    .As().Map(static view => view.Map(static image => (Control)image)));

    // --- [LOADS]

    private void OnBeginLoadAtStartPlugIns(int expectedCount) =>
        _ = Session.Run(Advanced(current => expectedCount > 0 ? new SkinLoad.Loading(expectedCount, None, Ended: 0) : current) >> PlugInRegistry.LoadPlugIns(TDefinition.Definition.PlugIns));

    private void OnBeginLoadPlugIn(string description) => _ = Session.Run(Advanced(current => current.Begin(description)));

    private void OnEndLoadPlugIn() =>
        _ = Session.Run(Advanced(static current => current.End()).Bind(next => IO.lift(() => next.Percent.Iter(setBootSplashProgress))));

    private void OnEndLoadAtStartPlugIns() => _ = Session.Run(Advanced(static _ => new SkinLoad.Ready()).Bind(_ => IO.lift(dismissBootSplash)));

    private IO<SkinLoad> Advanced(Func<SkinLoad, SkinLoad> transition) =>
        from next in load.SwapIO(transition)
        from shown in IO.lift(() => {
            _ = splash.Value.Iter(owned => {
                Refresh(next, owned.Value.Status, owned.Value.Bar);
                Placed(owned.Value.Window, Size.Max(owned.Value.Window.ClientSize, Size.Ceiling(owned.Value.Window.Content.GetPreferredSize())));
            });
            Application.Instance.RunIteration();
        })
        select next;

    // --- [HOOKS]

    private void OnMainFrameWindowCreated() => _ = Session.Run(TDefinition.Definition.MainFrameCreated.IfNone(IO.pure(unit)));

    private void OnLicenseCheckCompleted() => _ = Session.Run(TDefinition.Definition.LicenseChecked.IfNone(IO.pure(unit)));

    private void OnBuiltInCommandsRegistered() => _ = Session.Run(TDefinition.Definition.CommandsRegistered.IfNone(IO.pure(unit)));

    // --- [SPLASH]

    private void ShowSplash() =>
        _ = Session.Run(unless(hasBootSplash(), Products.Replace(splash, Splashed).Scoped().Bind(static _ => IO.lift(static () => Application.Instance.RunIteration()))).As());

    private void HideSplash() => _ = Session.Run(Products.Clear(splash).Scoped());

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

    private void ShowHelp() =>
        _ = Session.Run(
            from mark in Mark
            from detail in TDefinition.Definition.About.IfNone(static () =>
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

    private void ShowChooseTemplate() =>
        _ = Session.Run(
            Chosen(
                from paths in HostDialogs.ShowOpenFileDialog(static dialog => {
                    dialog.Title = LocalText.Of("New from Template").Local;
                    dialog.Filter = HostDialogs.Filter(Seq((LocalText.Of("Rhino Templates").Local, Seq(".3dm"))));
                    dialog.InitialDirectory = TDefinition.Definition.TemplateFolder.IfNone(static () => FileSettings.TemplateFolder);
                })
                from opened in paths.TraverseM(path => IO.lift(() => Conversions.Required(openNewDocument(path)))).As()
                select unit));

    private void ShowChooseRecent() =>
        _ = Session.Run(
            Chosen(
                from recent in TDefinition.Definition.Recent.IfNone(static () => IO.lift(static () => toSeq(FileSettings.RecentlyOpenedFiles())))
                from files in IO.pure(recent.Map(static path => new ChoiceItem<string>(path, path)).AsIterableNE())
                from opened in files.TraverseM(Reopened).As()
                select unit));

    private static IO<Unit> Chosen(IO<Unit> choice) => choice | @catch(Errors.Cancelled, IO.pure(unit));

    private static IO<Unit> Reopened(IterableNE<ChoiceItem<string>> items) =>
        from path in HostDialogs.ShowListBox(LocalText.Of("Open Recent"), LocalText.Of("Choose a file to open"), items, None)
        from existing in IO.lift(InputPath.Of(path))
        from document in Documents.With(new DocumentSource.Opened(existing), static _ => IO.pure(unit))
        select document;

    // --- [LAYOUT]

    private static void Refresh(SkinLoad state, Label status, ProgressBar bar) {
        status.Text = state.Switch(
            starting: static _ => LocalText.Of("Starting"),
            loading: static loading => loading.Current.Match(
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

    private static IO<StackLayout> Layout(Option<Control> mark, Control detail) =>
        from heading in use(static () => new Label { Text = ApplicationName, Font = EtoFonts.HeadingFont })
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

// --- [COMPOSITION] ---------------------------------------------------------------------

public static partial class PlugInShell {

    // --- [SKINS]

    public static SkinMembers Skin<TDefinition>(Skin host, Func<bool> hasBootSplash, Action<int> setBootSplashProgress, Action dismissBootSplash, Func<string, RhinoDoc?> openNewDocument)
        where TDefinition : IDefined<SkinDefinition> =>
        new SkinBody<TDefinition>(host, hasBootSplash, setBootSplashProgress, dismissBootSplash, openNewDocument).Members;
}
