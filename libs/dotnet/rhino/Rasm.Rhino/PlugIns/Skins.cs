using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
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
public abstract partial class DefinedSkin(Seq<Action<ILoggingBuilder>> logging) : Skin, IPlugInLifetime {
    // --- [STATE]
    [Union]
    private abstract partial record SkinLoad {
        public SkinLoad Begin(string plugIn) =>
            Switch<string, SkinLoad>(
                plugIn,
                starting: static (_, starting) => starting,
                loading: static (begun, loading) => loading with { Begun = loading.Begun.Add(begun) },
                ready: static (_, ready) => ready);

        public Option<int> Percent =>
            Switch(
                starting: static _ => Option<int>.None,
                loading: static loading => Some(loading.Begun.Count * 100 / loading.Expected),
                ready: static _ => Option<int>.None);

        public sealed record Starting : SkinLoad;

        public sealed record Loading(int Expected, Seq<string> Begun) : SkinLoad;

        public sealed record Ready : SkinLoad;
    }

    public PlugInSession Session { get; } = new(logging);
    private readonly Atom<SkinLoad> load = Atom<SkinLoad>(new SkinLoad.Starting());
    private readonly Ref<Option<Lease<Splash>>> splash = Ref(Option<Lease<Splash>>.None);

    // --- [IDENTITY]
    protected sealed override string ApplicationName => GetType().Assembly.GetCustomAttributes<AssemblyTitleAttribute>().Single().Title;

    protected sealed override System.Drawing.Bitmap? MainRhinoIcon =>
        this.Answer(
            (HostUtils.RunningOnWindows
                ? Icons.PlugInImage(this).Bind(static glyph => glyph.Traverse(static art => Icons.Raster(art, new System.Drawing.Size(256, 256))).As())
                : IO.pure(Option<System.Drawing.Bitmap>.None)).Map(static bitmap => bitmap.ValueUnsafe()),
            failed: null);

    private IO<Option<Icon>> Mark => Icons.PlugInImage(this).Bind(static glyph => glyph.Traverse(static art => Icons.Frames(art, new Size(128, 128))).As());

    // --- [LOAD]
    protected sealed override void OnBeginLoadAtStartPlugIns(int expectedCount) =>
        _ = this.Run(
            Session.Open(this, [static owner => EventKind.RhinoAppClosing.Inline(_ => owner.Session.Close, owner)])
                .Bind(_ => Advanced(current => expectedCount > 0 ? new SkinLoad.Loading(expectedCount, Seq<string>()) : current)));

    protected sealed override void OnBeginLoadPlugIn(string description) =>
        _ = this.Run(Advanced(current => current.Begin(description)).Bind(static next => IO.lift(() => next.Percent.Iter(SetBootSplashProgress))));

    protected sealed override void OnEndLoadAtStartPlugIns() =>
        _ = this.Run(Advanced(static _ => new SkinLoad.Ready()).Bind(static _ => IO.lift(DismissBootSplash)));

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

    // --- [SPLASH]
    protected sealed override void ShowSplash() =>
        _ = this.Run(unless(HasBootSplash, Session.Held(Products.Swapped(splash, Splashed)).Bind(static _ => IO.lift(static () => Application.Instance.RunIteration()))).As());

    protected sealed override void HideSplash() => _ = this.Run(Session.Held(Products.Cleared(splash)));

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
        _ = this.Run(
            from mark in Mark
            from detail in use(static () => new Label { Text = RowText.Localize("Rhino {0}", arguments: [RhinoApp.Version]).Local, Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center })
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
        _ = this.Run(
            Chosen(
                from paths in IO.lift(static () => HostDialogs.ShowOpenDialog(new() {
                    Title = RowText.Localize("New from Template").Local,
                    Filter = HostDialogs.Filter(Seq((RowText.Localize("Rhino Templates").Local, Seq(".3dm")))),
                    InitialDirectory = FileSettings.TemplateFolder,
                })).Bind(static dialog => dialog)
                from opened in paths.TraverseM(static path => IO.lift(() => Conversions.Found(OpenNewDocument(path)))).As()
                select unit));

    protected sealed override void ShowChooseRecent() =>
        _ = this.Run(
            Chosen(
                from files in IO.lift(static () => toSeq(FileSettings.RecentlyOpenedFiles()).Map(static path => new Captioned<string>(path, new LocalizeStringPair(path, path))).AsIterableNE())
                from opened in files.TraverseM(Reopened).As()
                select unit));

    private static IO<Unit> Chosen(IO<Unit> choice) => choice | @catch(Errors.Cancelled, IO.pure(unit));

    private static IO<Unit> Reopened(IterableNE<Captioned<string>> items) =>
        from path in HostDialogs.ShowListBox(RowText.Localize("Open Recent"), RowText.Localize("Choose a file to open"), items, None)
        from existing in IO.lift(() => InputPath.Of(path))
        from document in Documents.WithDocument(new DocumentSource.Opened(existing), static _ => IO.pure(unit))
        select document;

    // --- [LAYOUT]
    private static void Refresh(SkinLoad state, Label status, ProgressBar bar) {
        status.Text = state.Switch(
            starting: static _ => RowText.Localize("Starting"),
            loading: static loading => loading.Begun.Last.Match(
                Some: static plugIn => RowText.Localize("Loading {0}", arguments: [plugIn]),
                None: static () => RowText.Localize("Loading plug-ins")),
            ready: static _ => RowText.Localize("Preparing viewports")).Local;
        _ = state.Percent.Match(
            Some: percent => {
                bar.Indeterminate = false;
                bar.Value = percent;
            },
            None: () => bar.Indeterminate = true);
    }

    private IO<StackLayout> Layout(Option<Icon> mark, Control detail) =>
        from heading in use(() => new Label { Text = ApplicationName, Font = EtoFonts.HeadingFont })
        from image in mark.TraverseM(static icon => use(() => new ImageView { Image = icon })).As()
        from layout in use(() => new StackLayout([.. image.ToSeq().Map<StackLayoutItem>(static view => view), heading, new StackLayoutItem(detail, HorizontalAlignment.Stretch)]) {
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
