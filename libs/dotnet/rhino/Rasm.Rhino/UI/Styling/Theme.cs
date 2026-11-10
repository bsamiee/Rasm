using Eto.Forms;
using Rhino.UI;

namespace Rasm.Rhino.UI.Styling;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class HostTheme {

    // --- [CHANGES]

    public static HostEvent<Unit> Changed =>
        Subscriptions.Merged(
            Subscriptions.On<EventHandler, EventArgs>(static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h, static handler => handler.Invoke).Choose(static _ => Some(unit)),
            IHostPlatform.Current.Changed);

    // --- [APPLICATION]

    public static IO<Unit> Applied(Option<Control> owner, IO<Unit> apply) =>
        (apply >> Changed.Attach(_ => apply)) switch {
            var themed => owner.Match(
                Some: control => ViewOps.Loaded(control,
                    from held in themed
                    from window in IO.lift(() => Optional(control.ParentWindow))
                    from scaled in window.Traverse(parent => Subscriptions.On<EventArgs>(h => parent.LogicalPixelSizeChanged += h, h => parent.LogicalPixelSizeChanged -= h).Attach(_ => apply)).As()
                    select unit),
                None: () => themed),
        };
}
