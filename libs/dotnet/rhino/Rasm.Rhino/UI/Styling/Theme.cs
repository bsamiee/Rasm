using System.Runtime.Versioning;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Microsoft.Win32;
using Rasm.Rhino.Events;
using Rhino.UI;

namespace Rasm.Rhino.UI.Styling;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IThemeHandler {
    public DisplayOptions Accessibility { get; }

    public HostEvent<EventArgs> Changed { get; }

    public Font Digits(Font font);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DisplayOptions(bool ReduceMotion, bool IncreaseContrast, bool DifferentiateWithoutColor, bool ReduceTransparency, bool InvertColors);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PaintSlot {
    public static readonly PaintSlot Text = new(static () => ThemeSettings.Content.Text.Enabled);
    public static readonly PaintSlot TextDisabled = new(static () => ThemeSettings.Content.Text.Disabled);
    public static readonly PaintSlot TextSecondary = new(static () => ThemeSettings.Content.Text.Secondary);
    public static readonly PaintSlot EntryText = new(static () => ThemeSettings.Content.Entry.Enabled.Text);
    public static readonly PaintSlot EntryBorder = new(static () => ThemeSettings.Content.Entry.Enabled.Border);
    public static readonly PaintSlot EntryBackground = new(static () => ThemeSettings.Content.Entry.Enabled.Background);
    public static readonly PaintSlot ListText = new(static () => ThemeSettings.Content.List.Enabled.Text);
    public static readonly PaintSlot ListBackground = new(static () => ThemeSettings.Content.List.Enabled.Background);
    public static readonly PaintSlot InvalidText = new(static () => Colors.Red);
    public static readonly PaintSlot Background = new(static () => ThemeSettings.Content.Background);
    public static readonly PaintSlot Highlight = new(static () => ThemeSettings.Content.Highlight);
    public static readonly PaintSlot HighlightHover = new(static () => ThemeSettings.Content.HighlightHover);
    public static readonly PaintSlot FrameBackground = new(static () => ThemeSettings.Frame.Background);
    public static readonly PaintSlot FrameListBackground = new(static () => ThemeSettings.Frame.List.Enabled.Background);
    public static readonly PaintSlot FrameEdge = new(static () => ThemeSettings.Frame.Edge);

    [UseDelegateFromConstructor]
    public partial Color Read();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostTheme {
    // --- [METRIC]
    public static IO<float> Scale => IO.lift(static () => Screen.Screens.Max(static screen => screen.LogicalPixelSize));

    public static IO<System.Drawing.Size> Device(Size logical) =>
        Scale
            .Map(scale => Size.Ceiling(logical * scale))
            .Map(static device => new System.Drawing.Size(device.Width, device.Height));

    // --- [COLORS]
    public static Option<float> Veil(HashMap<PaintSlot, Color> slots) =>
        Offsets(slots[PaintSlot.Text], slots[PaintSlot.ListBackground]).Zip(Offsets(slots[PaintSlot.TextDisabled], slots[PaintSlot.ListBackground]))
            .Fold((Moved: 0f, Span: 0f), static (sum, pair) => (sum.Moved + (pair.First * (pair.First - pair.Second)), sum.Span + (pair.First * pair.First)))
            is { Span: > 0f } total
            ? Some(total.Moved / total.Span).Filter(static veil => veil is >= 0f and <= 1f)
            : None;

    private static Seq<float> Offsets(Color ink, Color ground) =>
        Seq(ink.R - ground.R, ink.G - ground.G, ink.B - ground.B).Map(offset => ink.A * offset);

    // --- [FONTS]
    public static Font Digits(Font font) => Handler.Match(Some: handler => handler.Digits(font), None: () => font);

    // --- [ACCESSIBILITY]
    public static DisplayOptions Accessibility => Handler.Match(Some: static handler => handler.Accessibility, None: static () => default);

    // --- [CHANGE]
    public static readonly HostEvent<Unit> Changed =
        Subscriptions.Host(typeof(HostTheme), static () => EventKind.ThemeChanged.Cons(Handler.Map(static handler => handler.Changed).ToSeq())
            .Concat(OperatingSystem.IsWindows()
                ? Seq(DisplayChangesOnWindows())
                : []))
            .Choose(static _ => Some(unit));

    [SupportedOSPlatform("windows")]
    private static HostEvent<EventArgs> DisplayChangesOnWindows() =>
        Subscriptions.Host<EventHandler, EventArgs>(typeof(SystemEvents),
            static handler => SystemEvents.DisplaySettingsChanged += handler,
            static handler => SystemEvents.DisplaySettingsChanged -= handler,
            static handler => handler.Invoke, nameof(SystemEvents.DisplaySettingsChanged));

    public static HostEvent<Unit> In(Control control) =>
        new(typeof(Control), nameof(Control.LoadComplete), (deliver, site, retain) =>
            DisposalOps.Acquire(owned =>
                from mounted in DisposalOps.Acquire(IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release)), owned)
                let mount = DisposalOps.Acquire(windowOwned =>
                    IO.lift(() => Optional(control.ParentWindow)).Bind(window => window.Traverse(parent =>
                        Subscriptions.Host<EventArgs>(typeof(Window),
                            handler => parent.LogicalPixelSizeChanged += handler,
                            handler => parent.LogicalPixelSizeChanged -= handler,
                            nameof(Window.LogicalPixelSizeChanged))
                        .Choose(static _ => Some(unit)).Inline(deliver, site.Sink, windowOwned)).As()).Map(unit),
                    (_, lease) => mounted.Replace(lease))
                from changed in Changed.Inline(deliver, site.Sink, owned)
                from loaded in Subscriptions.Attach(
                    handler => control.LoadComplete += handler, handler => control.LoadComplete -= handler,
                    Callbacks.Handler<EventArgs>(_ => mount.Bind(__ => deliver(unit)), site), owned)
                from unloaded in Subscriptions.Attach(
                    handler => control.UnLoad += handler, handler => control.UnLoad -= handler,
                    Callbacks.Handler<EventArgs>(_ => mounted.Drain, site), owned)
                from initial in when(control.Loaded, mount).As()
                select unit, retain));

    // --- [HOST]
    private static Option<IThemeHandler> Handler =>
        Platform.Instance.Supports<IThemeHandler>() ? Some(Platform.Instance.CreateShared<IThemeHandler>()) : None;
}
