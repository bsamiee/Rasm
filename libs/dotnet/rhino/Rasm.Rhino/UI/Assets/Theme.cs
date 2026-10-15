using System.Runtime.Versioning;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Microsoft.Win32;
using Rasm.Rhino.Events;
using Rhino.UI;

namespace Rasm.Rhino.UI.Assets;

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
    public static readonly PaintSlot ControlText = new(static () => SystemColors.ControlText);
    public static readonly PaintSlot DisabledText = new(static () => SystemColors.DisabledText);
    public static readonly PaintSlot HighlightText = new(static () => SystemColors.HighlightText);
    public static readonly PaintSlot SelectionText = new(static () => SystemColors.SelectionText);
    public static readonly PaintSlot LinkText = new(static () => SystemColors.LinkText);
    public static readonly PaintSlot InvalidText = new(static () => Colors.Red);
    public static readonly PaintSlot Control = new(static () => SystemColors.Control);
    public static readonly PaintSlot ControlBackground = new(static () => SystemColors.ControlBackground);
    public static readonly PaintSlot WindowBackground = new(static () => SystemColors.WindowBackground);
    public static readonly PaintSlot Highlight = new(static () => SystemColors.Highlight);
    public static readonly PaintSlot Selection = new(static () => SystemColors.Selection);
    public static readonly PaintSlot ContentTextEnabled = new(static () => ThemeSettings.Content.Text.Enabled);
    public static readonly PaintSlot ContentTextDisabled = new(static () => ThemeSettings.Content.Text.Disabled);
    public static readonly PaintSlot ContentTextSecondary = new(static () => ThemeSettings.Content.Text.Secondary);
    public static readonly PaintSlot ContentEntryEnabledText = new(static () => ThemeSettings.Content.Entry.Enabled.Text);
    public static readonly PaintSlot ContentListEnabledText = new(static () => ThemeSettings.Content.List.Enabled.Text);
    public static readonly PaintSlot ContentHighlight = new(static () => ThemeSettings.Content.Highlight);
    public static readonly PaintSlot ContentHighlightHover = new(static () => ThemeSettings.Content.HighlightHover);
    public static readonly PaintSlot ContentBackground = new(static () => ThemeSettings.Content.Background);
    public static readonly PaintSlot ContentEntryEnabledBackground = new(static () => ThemeSettings.Content.Entry.Enabled.Background);
    public static readonly PaintSlot ContentListEnabledBackground = new(static () => ThemeSettings.Content.List.Enabled.Background);
    public static readonly PaintSlot FrameBackground = new(static () => ThemeSettings.Frame.Background);
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
    public static Option<float> Veil(HashMap<PaintSlot, Color> slots, Color ground) =>
        Offsets(slots[PaintSlot.ControlText], ground).Zip(Offsets(slots[PaintSlot.DisabledText], ground))
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
                from mounted in IO.lift(static () => Ref(Some(Option<IDisposable>.None)))
                let exchange = fun((Option<Option<IDisposable>> next) => atomicIO(() => {
                    Option<Option<IDisposable>> prior = mounted.Value;
                    mounted.Value = prior.Bind(_ => next);
                    return prior.Flatten();
                }).Bind(static prior => IO.lift(() => prior.Iter(static lease => lease.Dispose()))).Uninterruptible())
                let mount = DisposalOps.Replace(windowOwned =>
                    IO.lift(() => Optional(control.ParentWindow)).Bind(window => window.Traverse(parent =>
                        Subscriptions.Host<EventArgs>(typeof(Window),
                            handler => parent.LogicalPixelSizeChanged += handler,
                            handler => parent.LogicalPixelSizeChanged -= handler,
                            nameof(Window.LogicalPixelSizeChanged))
                        .Choose(static _ => Some(unit)).Inline(deliver, site.Sink, windowOwned)).As()).Map(unit),
                    (_, lease) => {
                        Option<IDisposable> prior = atomic(() => mounted.Value.Match(
                            Some: current => {
                                mounted.Value = Some(Some(lease));
                                return current;
                            },
                            None: static () => throw Errors.Cancelled.ToException()));
                        return IO.lift(() => prior.Iter(static held => held.Dispose()));
                    })
                from lifetime in DisposalOps.Acquire(
                    IO.lift(() => new Disposal<Ref<Option<Option<IDisposable>>>>(mounted, _ => exchange(None).Run())),
                    static lease => IO.lift(lease.Dispose), owned)
                from changed in Changed.Inline(deliver, site.Sink, owned)
                from loaded in Subscriptions.Attach(
                    handler => control.LoadComplete += handler, handler => control.LoadComplete -= handler,
                    Callbacks.Handler<EventArgs>(_ => mount.Bind(__ => deliver(unit)), site), owned)
                from unloaded in Subscriptions.Attach(
                    handler => control.UnLoad += handler, handler => control.UnLoad -= handler,
                    Callbacks.Handler<EventArgs>(_ => exchange(Some(Option<IDisposable>.None)), site), owned)
                from initial in when(control.Loaded, mount).As()
                select unit, (_, lease) => retain(lease)));

    // --- [HOST]
    private static Option<IThemeHandler> Handler =>
        Platform.Instance.Supports<IThemeHandler>() ? Some(Platform.Instance.CreateShared<IThemeHandler>()) : None;
}
