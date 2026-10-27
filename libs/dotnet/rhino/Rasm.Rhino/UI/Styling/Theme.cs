using System.Runtime.Versioning;
using Eto.Forms;
using Microsoft.Win32;
using Rasm.Rhino.Events;

namespace Rasm.Rhino.UI.Styling;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostTheme {
    // --- [CHANGE]
    public static HostEvent<Unit> Changed =>
        Subscriptions.Merged([
                EventKind.ThemeChanged,
                EventKind.AppSettingsChanged,
                HostPlatform.Current.Changed,
                .. OperatingSystem.IsWindows() ? Seq(DisplayChangesOnWindows()) : Seq<HostEvent<EventArgs>>(),
            ])
            .Choose(static _ => Some(unit));

    [SupportedOSPlatform("windows")]
    private static HostEvent<EventArgs> DisplayChangesOnWindows() =>
        Subscriptions.Host<EventHandler, EventArgs>(static handler => SystemEvents.DisplaySettingsChanged += handler, static handler => SystemEvents.DisplaySettingsChanged -= handler, static handler => handler.Invoke);

    public static HostEvent<Unit> In(Control control) =>
        ViewOps.Loaded(control) switch {
            var loaded => Subscriptions.Merged(Changed, new HostEvent<Unit>(loaded.Label, (deliver, owner) =>
                from scope in IO.env
                from mounted in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
                let mount = Products.Swapped(mounted, IO.lift(() => Optional(control.ParentWindow)).Bind(window => window.Traverse(parent =>
                        Subscriptions.Host<EventArgs>(h => parent.LogicalPixelSizeChanged += h, h => parent.LogicalPixelSizeChanged -= h).Inline(_ => deliver(unit), owner)).As()).Map(unit))
                    .Under(scope)
                from loads in loaded.Inline(shown => shown ? mount >> deliver(unit) : Products.Cleared(mounted).Under(scope), owner)
                from initial in when(control.Loaded, mount).As()
                select unit)),
        };
}
