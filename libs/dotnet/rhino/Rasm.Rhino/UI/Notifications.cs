using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rasm.UI.Components;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class BannerMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(Notice.Description), nameof(Notification.Message))]
    [MapProperty(nameof(Notice.Reveal), nameof(Notification.UserData), Use = nameof(Revealed))]
    [MapperIgnoreSource(nameof(Notice.Message), Justification = "Rhino notification center holds detail text")]
    [MapperIgnoreSource(nameof(Notice.Severity), Justification = "System banner shows no severity")]
    [MapperIgnoreSource(nameof(Notice.Glyph), Justification = "Notifications reads glyph as content image")]
    [MapperIgnoreTarget(nameof(Notification.ContentImage), Justification = "Notifications assigns acquired glyph")]
    internal static partial void Update(Notice notice, Notification banner);

    [UserMapping(Default = false)]
    private static string Revealed(Option<View.Panel> reveal) =>
        Conversions.Unset(reveal.Map(static panel => panel.Identity.GUID.ToString("D", CultureInfo.InvariantCulture)));
}

public static class Notifications {
    // --- [DOCK]
    public static IO<Disposal<Application>> Acquire(Action<Disposal<Application>> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new Disposal<Application>(Application.Instance, Cleared)), retain);

    public static IO<Unit> SetProgress<T>(Disposal<Application> hold, Content<T> work) where T : notnull =>
        work.Switch(
            pending: static pending => pending.Work.Match(
                Some: static progress => (State: TaskbarProgressState.Progress, Progress: (float)progress.Fraction),
                None: static () => (State: TaskbarProgressState.Indeterminate, Progress: 0f)),
            ready: static _ => (State: TaskbarProgressState.None, Progress: 0f),
            empty: static _ => (State: TaskbarProgressState.None, Progress: 0f),
            failed: static _ => (State: TaskbarProgressState.Error, Progress: 0f)) switch {
                var shown => IO.lift(() => hold.Held.Iter(_ => Taskbar.SetProgress(shown.State, shown.Progress))),
            };

    public static IO<Unit> BadgeLabel(Disposal<Application> hold, Option<string> label) =>
        IO.lift(() => hold.Held.Iter(app => app.BadgeLabel = Conversions.Unset(label)));

    private static void Cleared(Application app) =>
        Callbacks.Each(Seq(IO.lift(static () => Taskbar.SetProgress(TaskbarProgressState.None)), IO.lift(() => { app.BadgeLabel = ""; }))).Run();

    // --- [TRAY]
    public static IO<Unit> Tray(Command<RowRule, Invoke>.Menu menu, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(acquired =>
            from tray in DisposalOps.Acquire(IO.lift(static () => new TrayIndicator()), acquired)
            from themed in menu.Face.Glyph.Traverse(glyph => Icons.Themed(scope.Sink, glyph, IconSlot.Small, HostTheme.Changed, icon => tray.Image = icon, acquired)).As()
            from popup in CommandRows.Menu(menu.Groups, scope, acquired)
            from assigned in IO.lift(() => tray.Menu = popup.Menu)
            from activated in menu.Primary.Traverse(primary => CommandRows.Press(primary, scope, acquired).Bind(press => Subscriptions.Attach(
                h => tray.Activated += h, h => tray.Activated -= h,
                Callbacks.Handler<EventArgs>(_ => press, new CallbackSite(scope.Sink, typeof(TrayIndicator), nameof(TrayIndicator.Activated))), acquired))).As()
            from shown in IO.lift(tray.Show)
            from titled in IO.lift(() => tray.Title = RowText.Localize(menu.Face.Caption, scope.Sink).Local)
            select unit, retain);

    // --- [BANNERS]
    public static IO<Unit> Show(Notice notice) =>
        IO.lift(static () => Application.Instance.IsActive).Bind(active => unless(active, Banner(notice)).As());

    private static IO<Unit> Banner(Notice notice) =>
        from image in notice.Glyph.Traverse(static glyph => Icons.Frames(glyph, IconSlot.Banner)).As()
        from shown in DisposalOps.Within(static () => new Notification(), banner =>
            from mapped in IO.lift(() => BannerMapper.Update(notice, banner))
            from assigned in IO.lift(() => banner.ContentImage = image.ValueUnsafe())
            from delivered in IO.lift(() => banner.Show())
            select delivered)
        select shown;

    // --- [ACTIVATION]
    public static IO<Unit> NotificationActivated(IPlugInViews plugIn, Action<IDisposable> retain) =>
        Subscriptions.Host<NotificationEventArgs>(
                typeof(Application),
                static handler => Application.Instance.NotificationActivated += handler,
                static handler => Application.Instance.NotificationActivated -= handler,
                nameof(Application.NotificationActivated))
            .Inline(args => IO.lift(() => Callbacks.Found(Guid.TryParse(args.UserData, CultureInfo.InvariantCulture, out Guid id), id)
                .Bind(plugIn.Views.Find<View.Panel>))
                .Bind(static found => found.TraverseM(Showing.Open).As()).Map(static _ => unit), plugIn, retain);
}
