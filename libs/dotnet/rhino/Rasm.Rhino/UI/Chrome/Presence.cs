using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Views;
using Rhino.PlugIns;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.UI.Chrome;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct BadgeCount : System.Numerics.IMinMaxValue<BadgeCount> {
    public static BadgeCount MinValue { get; } = new(1);
    public static BadgeCount MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class DockPhase {
    public static readonly DockPhase Working = new(static work => work.Map(inactive: TaskbarProgressState.None, indeterminate: TaskbarProgressState.Indeterminate, counted: TaskbarProgressState.Progress));
    public static readonly DockPhase Failed = new(static _ => TaskbarProgressState.Error);

    [UseDelegateFromConstructor]
    public partial TaskbarProgressState State(WorkState work);
}

public sealed record DockState(WorkState Work, DockPhase Phase);

public sealed record TrayRow(string Caption, IGlyph Glyph, Seq<Seq<MenuEntry>> Menu);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class BannerMapper {
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(Notice.Description), nameof(Notification.Message))]
    [MapProperty(nameof(Notice.Reveal), nameof(Notification.UserData), Use = nameof(Revealed))]
    [MapperIgnoreSource(nameof(Notice.Message), Justification = "Rhino's notification center holds the detail text")]
    [MapperIgnoreSource(nameof(Notice.Severity), Justification = "The system banner states no severity")]
    [MapperIgnoreSource(nameof(Notice.Glyph), Justification = "Presence reads it as the content image")]
    internal static partial Notification ToBanner(Notice notice, Eto.Drawing.Image? contentImage);

    [UserMapping(Default = false)]
    private static string Revealed(Option<View.Panel> reveal) =>
        Conversions.Unset(reveal.Map(static panel => panel.Identity.GUID.ToString("D", CultureInfo.InvariantCulture)));
}

public static class Presence {
    // --- [DOCK]
    public static IO<Disposal<Application>> Acquire() =>
        IO.lift(static () => new Disposal<Application>(Application.Instance, Cleared));

    public static IO<Unit> SetProgress(Disposal<Application> hold, DockState state) =>
        IO.lift(() => hold.Held.Iter(_ => Taskbar.SetProgress(
            state.Phase.State(state.Work),
            state.Work.Switch(inactive: static _ => 0f, indeterminate: static _ => 0f, counted: static counted => (float)counted.Work.Done / counted.Work.Total))));

    public static IO<Unit> BadgeLabel(Disposal<Application> hold, Option<BadgeCount> count) =>
        IO.lift(() => hold.Held.Iter(app => app.BadgeLabel = Label(count)));

    private static void Cleared(Application app) {
        Taskbar.SetProgress(TaskbarProgressState.None);
        app.BadgeLabel = Label(None);
    }

    private static string Label(Option<BadgeCount> count) =>
        Conversions.Unset(count.Map(static shown => shown.ToString(format: null, RowText.Culture)));

    // --- [TRAY]
    public static IO<IDisposable> Tray(TrayRow row, IPlugInSink sink) =>
        from tray in IO.lift(static () => new TrayIndicator())
        from held in DisposalOps.AcquireAll(
            Seq(
                IO.pure<IDisposable>(tray),
                Icons.Themed(sink, row.Glyph, IconSlot.MenuItem, icon => tray.Image = icon),
                from menu in ChoiceRows.Menu(row.Menu, Seq<IO<Unit>>(), None, sink)
                from set in DisposalOps.OnFailure(IO.lift(() => tray.Menu = menu.Menu), IO.lift(menu.Release.Dispose))
                select menu.Release),
            DisposalOps.Release)
        from titled in DisposalOps.OnFailure(
            IO.lift(tray.Show).Bind(_ => IO.lift(() => Callbacks.Thrown<ArgumentOutOfRangeException, string>(
                () => tray.Title = RowText.Localize(row.Caption, table: Some<object>(sink)).Local,
                nameof(TrayIndicator.Title)))),
            DisposalOps.Release(held))
        select DisposalOps.Composite(held, new CallbackSite(sink, typeof(TrayIndicator), nameof(TrayIndicator.Dispose)));

    // --- [BANNERS]
    public static IO<Unit> Show(Notice notice, IPlugInSink sink) =>
        IO.lift(static () => Application.Instance.IsActive).Bind(active => unless(active, Banner(notice, sink)).As());

    private static IO<Unit> Banner(Notice notice, IPlugInSink sink) =>
        from image in notice.Glyph.Traverse(glyph => Icons.Frames(sink, glyph, IconSlot.Banner)).As()
        from shown in use(() => BannerMapper.ToBanner(notice, image.ValueUnsafe())).Bind(static banner => IO.lift(() => banner.Show())).Bracket()
        select shown;

    // --- [ACTIVATION]
    public static IO<IDisposable> NotificationActivated(PlugIn plugIn, IPlugInSink sink) =>
        Subscriptions.Host<NotificationEventArgs>(
                typeof(Application),
                static handler => Application.Instance.NotificationActivated += handler,
                static handler => Application.Instance.NotificationActivated -= handler,
                nameof(Application.NotificationActivated))
            .Inline(args => IO.lift(() => Callbacks.Found(Guid.TryParse(args.UserData, CultureInfo.InvariantCulture, out Guid id), id)
                .Bind(((IPlugInViews)plugIn).Views.Find<View.Panel>)
                .Iter(static panel => Panels.OpenPanel(panel.Identity, makeSelectedPanel: true))), sink);
}
