using AppKit;
using Eto.Drawing;
using Eto.Mac.Drawing;
using Foundation;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WorkspaceTheme : IThemeHandler {
    // --- [CONDITIONS]
    private static readonly Seq<(Func<NSWorkspace, bool> Should, Rasm.UI.Styling.Condition Condition)> Displays = [
        (static workspace => workspace.AccessibilityDisplayShouldReduceMotion, Rasm.UI.Styling.Condition.ReduceMotion),
        (static workspace => workspace.AccessibilityDisplayShouldIncreaseContrast, Rasm.UI.Styling.Condition.IncreaseContrast),
        (static workspace => workspace.AccessibilityDisplayShouldDifferentiateWithoutColor, Rasm.UI.Styling.Condition.DifferentiateWithoutColor),
        (static workspace => workspace.AccessibilityDisplayShouldReduceTransparency, Rasm.UI.Styling.Condition.ReduceTransparency),
    ];

    public LanguageExt.HashSet<Rasm.UI.Styling.Condition> Conditions =>
        toHashSet(Displays.Filter(static row => row.Should(NSWorkspace.SharedWorkspace)).Map(static row => row.Condition));

    // --- [INKS]
    public Color Faulted => Eto.Mac.MacConversions.ToEto(NSColor.SystemRed);

    public Color Warning => Eto.Mac.MacConversions.ToEto(NSColor.SystemOrange);

    // --- [CHANGE]
    public HostEvent<EventArgs> Changed { get; } =
        new(typeof(NSWorkspace), nameof(NSWorkspace.DisplayOptionsDidChangeNotification), static (deliver, site, retain) =>
            DisposalOps.Acquire(owned =>
                from callback in IO.pure(Callbacks.Handler<NSNotificationEventArgs>(_ => deliver(EventArgs.Empty), site))
                from accessibility in DisposalOps.Acquire(IO.lift(() => NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                    NSWorkspace.DisplayOptionsDidChangeNotification, notification => callback(notification.Object, new NSNotificationEventArgs(notification)))), owned)
                from colors in DisposalOps.Acquire(IO.lift(() => NSColor.Notifications.ObserveSystemColorsChanged(callback)), owned)
                from displays in DisposalOps.Acquire(IO.lift(() => NSApplication.Notifications.ObserveDidChangeScreenParameters(callback)), owned)
                select unit, retain));

    // --- [FONTS]
    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public Font Digits(Font font) =>
        faces.FindOrAdd((font.Size, font.Bold), () => new Font(new FontHandler(
            NSFont.MonospacedDigitSystemFontOfSize(font.Size, font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular))));
}
