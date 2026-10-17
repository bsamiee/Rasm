using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Styling;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TrayRow(string Caption, (IGlyph Glyph, int Master) Glyph, Seq<Seq<MenuEntry>> Menu);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class BannerMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(Notice.Description), nameof(Notification.Message))]
    [MapProperty(nameof(Notice.Reveal), nameof(Notification.UserData), Use = nameof(Revealed))]
    [MapperIgnoreSource(nameof(Notice.Message), Justification = "Rhino's notification center holds the detail text")]
    [MapperIgnoreSource(nameof(Notice.Severity), Justification = "The system banner states no severity")]
    [MapperIgnoreSource(nameof(Notice.Glyph), Justification = "Notifications reads it as the content image")]
    [MapperIgnoreTarget(nameof(Notification.ContentImage), Justification = "Notifications assigns the acquired glyph")]
    internal static partial void Update(Notice notice, Notification banner);

    [UserMapping(Default = false)]
    private static string Revealed(Option<View.Panel> reveal) =>
        Conversions.Unset(reveal.Map(static panel => panel.Identity.GUID.ToString("D", CultureInfo.InvariantCulture)));
}

public static class Notifications {
    // --- [DOCK]
    public static IO<Disposal<Application>> Acquire(Action<Disposal<Application>> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new Disposal<Application>(Application.Instance, Cleared)), retain);

    public static IO<Unit> SetProgress(Disposal<Application> hold, WorkState work, Option<TaskbarProgressState> state = default) =>
        IO.lift(() => hold.Held.Iter(_ => Taskbar.SetProgress(
            state.IfNone(work.Map(inactive: TaskbarProgressState.None, indeterminate: TaskbarProgressState.Indeterminate, counted: TaskbarProgressState.Progress)),
            work.Switch(inactive: static _ => 0f, indeterminate: static _ => 0f, counted: static counted => (float)counted.Done / counted.Total))));

    public static IO<Unit> BadgeLabel(Disposal<Application> hold, Option<string> label) =>
        IO.lift(() => hold.Held.Iter(app => app.BadgeLabel = Conversions.Unset(label)));

    private static void Cleared(Application app) =>
        Callbacks.Each(Seq(IO.lift(static () => Taskbar.SetProgress(TaskbarProgressState.None)), IO.lift(() => { app.BadgeLabel = ""; }))).Run();

    // --- [TRAY]
    public static IO<Unit> Tray(TrayRow row, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(acquired =>
            from tray in DisposalOps.Acquire(IO.lift(static () => new TrayIndicator()), acquired)
            from themed in Icons.Themed(sink, row.Glyph, IconSlot.MenuItem, HostTheme.Changed, icon => tray.Image = icon, acquired)
            from menu in ChoiceRows.Menu(row.Menu, Seq<IO<Unit>>(), None, sink, acquired)
            from assigned in IO.lift(() => tray.Menu = menu)
            from shown in IO.lift(tray.Show)
            from titled in IO.lift(() => tray.Title = RowText.Localize(row.Caption, sink).Local)
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
