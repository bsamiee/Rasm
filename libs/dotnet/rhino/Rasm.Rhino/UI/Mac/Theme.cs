using AppKit;
using Eto.Drawing;
using Eto.Mac.Drawing;
using Foundation;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI.Styling;

namespace Rasm.RhinoCommon.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class WorkspaceTheme : IThemeHandler {
    // --- [CONDITIONS]
    private static readonly Seq<(Func<NSWorkspace, bool> Should, Condition Condition)> Displays = [
        (static workspace => workspace.AccessibilityDisplayShouldReduceMotion, Condition.ReduceMotion),
        (static workspace => workspace.AccessibilityDisplayShouldIncreaseContrast, Condition.IncreaseContrast),
        (static workspace => workspace.AccessibilityDisplayShouldDifferentiateWithoutColor, Condition.DifferentiateWithoutColor),
        (static workspace => workspace.AccessibilityDisplayShouldReduceTransparency, Condition.ReduceTransparency),
    ];

    public HashSet<Condition> Conditions =>
        toHashSet(Displays.Filter(static row => row.Should(NSWorkspace.SharedWorkspace)).Map(static row => row.Condition));

    // --- [INKS]
    public Color Faulted => Eto.Mac.MacConversions.ToEto(NSColor.SystemRed);

    public Color Warning => Eto.Mac.MacConversions.ToEto(NSColor.SystemOrange);

    // --- [CHANGE]
    public HostEvent<EventArgs> Changed { get; } =
        Subscriptions.Merged(
                DrawnViews.Observed(static handler => NSWorkspace.SharedWorkspace.NotificationCenter.AddObserver(
                    NSWorkspace.DisplayOptionsDidChangeNotification, notification => handler(notification.Object, new NSNotificationEventArgs(notification)))),
                DrawnViews.Observed(NSColor.Notifications.ObserveSystemColorsChanged),
                DrawnViews.Observed(NSApplication.Notifications.ObserveDidChangeScreenParameters))
            .Choose(static _ => Some(EventArgs.Empty));

    // --- [FONTS]
    private readonly AtomHashMap<(float Size, bool Bold), Font> faces = AtomHashMap<(float Size, bool Bold), Font>();

    public Font Digits(Font font) =>
        faces.FindOrAdd((font.Size, font.Bold), () => new Font(new FontHandler(
            NSFont.MonospacedDigitSystemFontOfSize(font.Size, font.Bold ? NSFontWeight.Bold : NSFontWeight.Regular))));
}
