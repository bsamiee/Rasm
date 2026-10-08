using System.Reflection;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Plugin;

// --- [COMPOSITION] ---------------------------------------------------------------------
public abstract partial class DefinedSkin : Skin, IPlugInSink {
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

    private readonly Atom<SkinLoad> load = Atom<SkinLoad>(new SkinLoad.Starting());
    private readonly Atom<Option<(Form Window, Label Status, ProgressBar Bar)>> splash = Atom<Option<(Form Window, Label Status, ProgressBar Bar)>>(None);

    // --- [IDENTITY]
    private static readonly Size MarkSize = new(128, 128);

    protected sealed override string ApplicationName => GetType().Assembly.GetCustomAttributes<AssemblyTitleAttribute>().Single().Title;

    protected sealed override System.Drawing.Bitmap? MainRhinoIcon =>
        Callbacks.Answer<System.Drawing.Bitmap?>(
            from icon in HostUtils.RunningOnWindows ? Icons.PlugInIcon(this) : IO.pure(Option<System.Drawing.Icon>.None)
            from bitmap in icon.TraverseM(static frames => use(() => new System.Drawing.Icon(frames, 256, 256)).Map(static sized => sized.ToBitmap()).Bracket()).As()
            select bitmap.ValueUnsafe(),
            static () => null,
            CallbackSite.Of(this));

    private IO<Option<Icon>> Mark => Icons.PlugInIcon(this).Map(static icon => icon.Map(static frames => frames.ToEto().WithSize(MarkSize)));

    // --- [LOAD]
    protected sealed override void OnBeginLoadAtStartPlugIns(int expectedCount) =>
        _ = Callbacks.Answer(Advanced(current => expectedCount > 0 ? new SkinLoad.Loading(expectedCount, Seq<string>()) : current).Map(static _ => unit), static () => unit, CallbackSite.Of(this));

    protected sealed override void OnBeginLoadPlugIn(string description) =>
        _ = Callbacks.Answer(Advanced(current => current.Begin(description)).Bind(static next => IO.lift(() => next.Percent.Iter(SetBootSplashProgress))), static () => unit, CallbackSite.Of(this));

    protected sealed override void OnEndLoadAtStartPlugIns() =>
        _ = Callbacks.Answer(Advanced(static _ => new SkinLoad.Ready()).Bind(static _ => IO.lift(DismissBootSplash)), static () => unit, CallbackSite.Of(this));

    private IO<SkinLoad> Advanced(Func<SkinLoad, SkinLoad> transition) =>
        from next in IO.lift(() => load.Swap(transition))
        from shown in IO.lift(() => {
            _ = splash.Value.Iter(held => {
                Refresh(next, held.Status, held.Bar);
                Placed(held.Window, Size.Max(held.Window.ClientSize, Size.Ceiling(held.Window.Content.GetPreferredSize())));
            });
            Application.Instance.RunIteration();
        })
        select next;

    // --- [SPLASH]
    protected sealed override void ShowSplash() =>
        _ = Callbacks.Answer(
            unless(HasBootSplash,
                from mark in Mark
                from shown in IO.lift(() => {
                    Label status = new() { Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center };
                    ProgressBar bar = new();
                    Refresh(load.Value, status, bar);
                    Form window = new() {
                        Title = ApplicationName, WindowStyle = WindowStyle.None, Resizable = false, Minimizable = false, Maximizable = false, ShowInTaskbar = false, Topmost = true,
                        Content = Layout(mark, new StackLayout(status, bar) {
                            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Dialog),
                            HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        }),
                    };
                    window.UseRhinoStyle();
                    Placed(window, Size.Ceiling(window.Content.GetPreferredSize()));
                    window.Show();
                    _ = splash.Swap(_ => Some((window, status, bar)));
                    Application.Instance.RunIteration();
                })
                select shown).As(),
            static () => unit,
            CallbackSite.Of(this));

    protected sealed override void HideSplash() =>
        _ = Callbacks.Answer(
            IO.lift(() => {
                _ = splash.Value.Iter(static held => {
                    held.Window.Close();
                    held.Window.Dispose();
                });
                _ = splash.Swap(static _ => None);
            }),
            static () => unit,
            CallbackSite.Of(this));

    // --- [HELP]
    protected sealed override void ShowHelp() =>
        _ = Callbacks.Answer(
            from mark in Mark
            from shown in use(() => new Dialog {
                Title = ApplicationName, Resizable = false,
                Content = Layout(mark, new Label { Text = RowText.Localize("Rhino {0}", arguments: [RhinoApp.Version]).Local, Font = EtoFonts.NormalFont, TextAlignment = TextAlignment.Center }),
            })
                .Bind(static dialog => IO.lift(() => {
                    dialog.UseRhinoStyle();
                    dialog.ShowModal(RhinoEtoApp.MainWindow);
                }))
                .Bracket()
            select shown,
            static () => unit,
            CallbackSite.Of(this));

    // --- [DOCUMENTS]
    protected sealed override void ShowChooseTemplate() =>
        _ = Callbacks.Answer(
            Chosen(
                from paths in IO.lift(static () => new global::Rhino.UI.OpenFileDialog {
                    Title = RowText.Localize("New from Template").Local,
                    Filter = HostDialogs.Filter(Seq(new FileTypeRow(RhinoApp.CurrentRhinoId, RowText.Localize("Rhino Templates").Local, Seq(".3dm")))),
                    InitialDirectory = FileSettings.TemplateFolder,
                })
                    .Bind(HostDialogs.ShowOpenDialog)
                from opened in paths.TraverseM(static path => IO.lift(() => Missing.Unless(OpenNewDocument(path), nameof(OpenNewDocument)))).As()
                select unit),
            static () => unit,
            CallbackSite.Of(this));

    protected sealed override void ShowChooseRecent() =>
        _ = Callbacks.Answer(
            Chosen(
                from files in IO.lift(static () => toSeq(FileSettings.RecentlyOpenedFiles()).Map(static path => new Captioned<string>(path, new LocalizeStringPair(path, path))).AsIterableNE())
                from opened in files.TraverseM(static items =>
                    from path in HostDialogs.ShowListBox(RowText.Localize("Open Recent"), RowText.Localize("Choose a file to open"), items, None)
                    from existing in IO.lift(() => Exchange.ExistingPath(path))
                    from document in Documents.WithDocument(new DocumentSource.Opened(existing), static _ => IO.pure(unit))
                    select document).As()
                select unit),
            static () => unit,
            CallbackSite.Of(this));

    private static IO<Unit> Chosen(IO<Unit> choice) => choice | @catch(Errors.Cancelled, IO.pure(unit));

    // --- [SINK]
    public void Report(Error error, Type owner, string member) =>
        _ = ErrorOps.Report(error, owner, member).RunSafe();

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

    private StackLayout Layout(Option<Icon> mark, Control detail) =>
        new([.. mark.ToSeq().Map<StackLayoutItem>(static icon => icon), new Label { Text = ApplicationName, Font = EtoFonts.HeadingFont }, new StackLayoutItem(detail, HorizontalAlignment.Stretch)]) {
            Padding = RhinoLayout.Padding(RhinoLayout.PaddingType.Dialog),
            Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Dialog),
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };

    private static void Placed(Form window, Size client) {
        window.ClientSize = client;
        window.Location = Eto.Drawing.Point.Round(Screen.PrimaryScreen.WorkingArea.Center - ((SizeF)window.Size / 2f));
    }
}
