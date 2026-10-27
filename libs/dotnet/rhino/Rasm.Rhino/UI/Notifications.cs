using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
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
    [MapperIgnoreSource(nameof(Notice.Icon), Justification = "Notifications reads the icon as content image")]
    [MapperIgnoreTarget(nameof(Notification.ContentImage), Justification = "Notifications assigns the acquired icon")]
    internal static partial void Update(Notice notice, Notification banner);

    [UserMapping(Default = false)]
    private static string Revealed(Option<View.Panel> reveal) =>
        Conversions.Unset(reveal.Map(static panel => panel.Identity.GUID.ToString("D", CultureInfo.InvariantCulture)));
}

public static class Notifications {
    // --- [DOCK]
    public static IO<Application> Acquire() =>
        from dock in IO.lift(static () => Application.Instance)
        from cleared in use(IO.pure(dock), static held => IO.lift(() => {
            Taskbar.SetProgress(TaskbarProgressState.None);
            held.BadgeLabel = "";
        }).OnUiThread())
        select dock;

    public static IO<Unit> SetProgress<T>(Loading<T> work) where T : notnull =>
        work.Switch(
            pending: static pending => pending.Work.Match(
                Some: static progress => (State: TaskbarProgressState.Progress, Progress: (float)progress.Fraction),
                None: static () => (State: TaskbarProgressState.Indeterminate, Progress: 0f)),
            ready: static _ => (State: TaskbarProgressState.None, Progress: 0f),
            failed: static _ => (State: TaskbarProgressState.Error, Progress: 0f)) switch {
                var shown => IO.lift(() => Taskbar.SetProgress(shown.State, shown.Progress)).OnUiThread(),
            };

    public static IO<Unit> BadgeLabel(Application dock, Option<string> label) =>
        IO.lift(() => { dock.BadgeLabel = Conversions.Unset(label); }).OnUiThread();

    // --- [TRAY]
    public static IO<Unit> Tray(Command<Invoke>.Menu menu, RowScope scope) =>
        from tray in use(static () => new TrayIndicator())
        from themed in menu.Face.Icon.Traverse(icon => Icons.Themed(scope.Owner, icon, IconSlot.Small, HostTheme.Changed, frames => tray.Image = frames)).As()
        from popup in CommandRows.Menu(menu.Groups, scope)
        from assigned in IO.lift(() => tray.Menu = popup.Menu)
        from activated in menu.Primary.Traverse(primary => CommandRows.Press(primary, scope).Bind(press =>
            Subscriptions.Host<EventArgs>(h => tray.Activated += h, h => tray.Activated -= h).Inline(_ => press, scope.Owner))).As()
        from shown in IO.lift(tray.Show)
        from titled in IO.lift(() => tray.Title = RowText.Localize(menu.Face.Caption, scope.Owner).Local)
        select unit;

    // --- [BANNERS]
    public static IO<Unit> Show(Notice notice) =>
        IO.lift(static () => Application.Instance.IsActive).Bind(active => unless(active, Banner(notice)).As());

    private static IO<Unit> Banner(Notice notice) =>
        from image in notice.Icon.Traverse(static icon => Icons.Frames(icon, IconSlot.Banner)).As()
        from shown in (from banner in use(static () => new Notification())
                       from mapped in IO.lift(() => BannerMapper.Update(notice, banner))
                       from assigned in IO.lift(() => banner.ContentImage = image.ValueUnsafe())
                       from delivered in IO.lift(() => banner.Show())
                       select delivered).Bracket()
        select shown;

    // --- [ACTIVATION]
    public static IO<Unit> NotificationActivated(IPlugInViews plugIn) =>
        Subscriptions.Host<NotificationEventArgs>(static h => Application.Instance.NotificationActivated += h, static h => Application.Instance.NotificationActivated -= h)
            .Inline(args => IO.lift(() => Conversions.Found(Guid.TryParse(args.UserData, CultureInfo.InvariantCulture, out Guid id), id).Bind(plugIn.Views.Find<View.Panel>))
                .Bind(static found => found.TraverseM(Showing.Open).As()).Map(static _ => unit), plugIn);
}
