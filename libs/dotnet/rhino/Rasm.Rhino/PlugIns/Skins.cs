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

    public PlugInSession Session { get; } = new(unitIO, logging);
    private readonly Atom<SkinLoad> load = Atom<SkinLoad>(new SkinLoad.Starting());
    private readonly Ref<Option<(Form Window, Label Status, ProgressBar Bar, IDisposable Lease)>> splash = Ref<Option<(Form Window, Label Status, ProgressBar Bar, IDisposable Lease)>>(None);

    // --- [IDENTITY]
    protected sealed override string ApplicationName => GetType().Assembly.GetCustomAttributes<AssemblyTitleAttribute>().Single().Title;

    protected sealed override System.Drawing.Bitmap? MainRhinoIcon =>
        Callbacks.Answer(
            (HostUtils.RunningOnWindows
                ? Icons.PlugInImage(this, static (assets, name) => assets.IconBitmaps.Get(name, new System.Drawing.Size(256, 256)))
                : IO.pure(Option<System.Drawing.Bitmap>.None)).Map(static bitmap => bitmap.ValueUnsafe()),
            static () => null,
            CallbackSite.Of(this));

    private IO<Option<Icon>> Mark => Icons.PlugInImage(this, static (assets, name) => assets.Native.Eto.Icons.Get(name, new Size(128, 128)));

    // --- [LOAD]
    protected sealed override void OnBeginLoadAtStartPlugIns(int expectedCount) =>
        Callbacks.Run(
            Session.Open(this, [static (owner, retain) => EventKind.RhinoAppClosing.Inline(_ => owner.Session.Close, owner, retain)])
                .Bind(_ => Advanced(current => expectedCount > 0 ? new SkinLoad.Loading(expectedCount, Seq<string>()) : current)),
            CallbackSite.Of(this));

    protected sealed override void OnBeginLoadPlugIn(string description) =>
        Callbacks.Run(Advanced(current => current.Begin(description)).Bind(static next => IO.lift(() => next.Percent.Iter(SetBootSplashProgress))), CallbackSite.Of(this));

    protected sealed override void OnEndLoadAtStartPlugIns() =>
        Callbacks.Run(Advanced(static _ => new SkinLoad.Ready()).Bind(static _ => IO.lift(DismissBootSplash)), CallbackSite.Of(this));

    private IO<SkinLoad> Advanced(Func<SkinLoad, SkinLoad> transition) =>
        from next in load.SwapIO(transition)
        from shown in IO.lift(() => {
            _ = atomic(() => splash.Value).Iter(held => {
                Refresh(next, held.Status, held.Bar);
                Placed(held.Window, Size.Max(held.Window.ClientSize, Size.Ceiling(held.Window.Content.GetPreferredSize())));
            });
            Application.Instance.RunIteration();
        })
        select next;

    // --- [SPLASH]
    protected sealed override void ShowSplash() =>
        Callbacks.Run(
            unless(HasBootSplash,
                DisposalOps.Replace(
                    owned =>
                        from mark in Mark
                        from status in DisposalOps.Acquire(IO.lift(static () => new Label { Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center }), owned)
                        from bar in DisposalOps.Acquire(IO.lift(static () => new ProgressBar()), owned)
                        from detail in DisposalOps.Acquire(IO.lift(() => new StackLayout(status, bar) {
                            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Dialog),
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        }), owned)
                        from content in Layout(mark, detail, owned)
                        from window in DisposalOps.Acquire(IO.lift(() => new Form {
                            Title = ApplicationName, WindowStyle = WindowStyle.None, Resizable = false, Minimizable = false, Maximizable = false, ShowInTaskbar = false, Topmost = true,
                            Content = content,
                        }), owned)
                        from shown in IO.lift(() => {
                            Refresh(load.Value, status, bar);
                            window.UseRhinoStyle();
                            Placed(window, Size.Ceiling(content.GetPreferredSize()));
                            window.Show();
                        })
                        select (Window: window, Status: status, Bar: bar),
                    (held, lease) => Swapped(Some((held.Window, held.Status, held.Bar, lease))))
                    .Bind(static _ => IO.lift(static () => Application.Instance.RunIteration()))).As(),
            CallbackSite.Of(this));

    protected sealed override void HideSplash() =>
        Callbacks.Run(Swapped(None), CallbackSite.Of(this));

    private IO<Unit> Swapped(Option<(Form Window, Label Status, ProgressBar Bar, IDisposable Lease)> next) =>
        IO.lift(() => atomic(() => {
            Option<(Form Window, Label Status, ProgressBar Bar, IDisposable Lease)> previous = splash.Value;
            splash.Value = next;
            return previous;
        })).Bind(static previous => previous.TraverseM(static held =>
            Callbacks.Each(Seq(IO.lift(held.Window.Close), IO.lift(held.Lease.Dispose)))).As()).Map(unit).Uninterruptible();

    // --- [HELP]
    protected sealed override void ShowHelp() =>
        Callbacks.Run(
            DisposalOps.Within<IDisposable, Unit>(
                owned =>
                    from mark in Mark
                    from detail in DisposalOps.Acquire(IO.lift(static () => new Label { Text = RowText.Localize("Rhino {0}", arguments: [RhinoApp.Version]).Local, Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center }), owned)
                    from content in Layout(mark, detail, owned)
                    from dialog in DisposalOps.Acquire(IO.lift(() => new Dialog { Title = ApplicationName, Resizable = false, Content = content }), owned)
                    from shown in IO.lift(() => {
                        dialog.UseRhinoStyle();
                        dialog.ShowModal(RhinoEtoApp.MainWindow);
                    })
                    select shown,
                DisposalOps.Release),
            CallbackSite.Of(this));

    // --- [DOCUMENTS]
    protected sealed override void ShowChooseTemplate() =>
        Callbacks.Run(
            Chosen(
                from paths in IO.lift(static () => HostDialogs.ShowOpenDialog(new() {
                    Title = RowText.Localize("New from Template").Local,
                    Filter = HostDialogs.Filter(Seq((RowText.Localize("Rhino Templates").Local, Seq(".3dm")))),
                    InitialDirectory = FileSettings.TemplateFolder,
                })).Bind(static dialog => dialog)
                from opened in paths.TraverseM(static path => IO.lift(() => Refused.UnlessFound(OpenNewDocument(path), nameof(OpenNewDocument)))).As()
                select unit),
            CallbackSite.Of(this));

    protected sealed override void ShowChooseRecent() =>
        Callbacks.Run(
            Chosen(
                from files in IO.lift(static () => toSeq(FileSettings.RecentlyOpenedFiles()).Map(static path => new Captioned<string>(path, new LocalizeStringPair(path, path))).AsIterableNE())
                from opened in files.TraverseM(Reopened).As()
                select unit),
            CallbackSite.Of(this));

    private static IO<Unit> Chosen(IO<Unit> choice) => choice | @catch(Errors.Cancelled, IO.pure(unit));

    private static IO<Unit> Reopened(IterableNE<Captioned<string>> items) =>
        from path in HostDialogs.ShowListBox(RowText.Localize("Open Recent"), RowText.Localize("Choose a file to open"), items, None)
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from document in Documents.WithDocument(new DocumentSource.Opened(existing), static _ => IO.pure(unit))
        select document;

    // --- [SINK]
    public void Report(Error error, Type owner, string member) => Session.Report(error, owner, member);

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

    private IO<StackLayout> Layout(Option<Icon> mark, Control detail, Action<IDisposable> retain) =>
        from heading in DisposalOps.Acquire(IO.lift(() => new Label { Text = ApplicationName, Font = EtoFonts.HeadingFont }), retain)
        from image in mark.TraverseM(icon => DisposalOps.Acquire(IO.lift(() => new ImageView { Image = icon }), retain)).As()
        from layout in DisposalOps.Acquire(IO.lift(() => new StackLayout([.. image.ToSeq().Map<StackLayoutItem>(static view => view), heading, new StackLayoutItem(detail, HorizontalAlignment.Stretch)]) {
            Padding = RhinoLayout.Padding(RhinoLayout.PaddingType.Dialog),
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Dialog),
            HorizontalContentAlignment = HorizontalAlignment.Center,
        }), retain)
        select layout;

    private static void Placed(Form window, Size client) {
        window.ClientSize = client;
        window.Location = Eto.Drawing.Point.Round(Screen.PrimaryScreen.WorkingArea.Center - ((SizeF)window.Size / 2f));
    }
}
