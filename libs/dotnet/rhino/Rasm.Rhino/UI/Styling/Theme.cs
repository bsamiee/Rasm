using Eto.Forms;
using Rhino.UI;

namespace Rasm.Rhino.UI.Styling;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostTheme {
    // --- [CHANGE]
    public static HostEvent<Unit> Changed =>
        Subscriptions.Merged(
            Subscriptions.On<EventHandler, EventArgs>(static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h, static handler => handler.Invoke).Choose(static _ => Some(unit)),
            HostPlatform.Current.Changed);

    // --- [APPLICATION]
    public static IO<Unit> Applied(IO<Unit> apply) => apply >> Changed.Attach(_ => apply);

    public static IO<Unit> Applied(Control control, IO<Unit> apply) =>
        ViewOps.WhileLoaded(control,
            from themed in Applied(apply)
            from window in IO.lift(() => Optional(control.ParentWindow))
            from scaled in window.Traverse(parent => Subscriptions.On<EventArgs>(h => parent.LogicalPixelSizeChanged += h, h => parent.LogicalPixelSizeChanged -= h).Attach(_ => apply)).As()
            select unit);
}
