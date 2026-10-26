using System.Runtime.Versioning;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Microsoft.Win32;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI.Styling;
using Rhino.Display;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Theme;
using SkiaSharp;

namespace Rasm.Rhino.UI.Styling;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IThemeHandler {
    public LanguageExt.HashSet<Condition> Conditions { get; }

    public Color Faulted { get; }

    public Color Warning { get; }

    public HostEvent<EventArgs> Changed { get; }

    public Font Digits(Font font);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HostTheme {
    // --- [FONTS]
    public static Font Digits(Font font) => Handler.Match(Some: handler => handler.Digits(font), None: () => font);

    // --- [CONDITIONS]
    public static LanguageExt.HashSet<Condition> Conditions => Handler.Match(Some: static handler => handler.Conditions, None: static () => []);

    // --- [PROJECTION]
    public static bool Compact(Control control) =>
        toSeq(control.Parents).Exists(static parent => Optional(parent.Style).Exists(static style => style.Split(' ').Contains(Panels.EtoPanelStyleName, StringComparer.Ordinal)));

    public static ColorScheme Projected(bool compact) =>
        ColorScheme.Create(toHashMap(Chrome), Conditions, new TextFace { Family = EtoFonts.NormalFont.FamilyName }, (compact ? EtoFonts.SmallFont : EtoFonts.NormalFont).Size);

    public static DefaultStyles Stated(bool compact) =>
        PanelRoles.Defaults.Combine(new DefaultStyles([], HashMap((ControlSize.Regular,
            (compact ? PanelRoles.Defaults.Sizes.Find(ControlSize.Compact).IfNone([]) : []).AddOrUpdate(Role.Scrollbar,
                HashMap((Metric.Extent, (float)ThemeSettings.Content.Scrollbar.Size), (Metric.Radius, (float)ThemeSettings.Content.Scrollbar.Radius)))))));

    private static Seq<(Token Token, SKColorF Color)> Chrome =>
        Seq(
            Element(Defaulted(Role.Button, ThemeSettings.Content.Button), Channels),
            Element(Checked(PanelRoles.Tab, ThemeSettings.Content.Tab), Channels),
            Element(Plain(Role.Field, ThemeSettings.Content.Entry), Channels),
            Element(Checked(PanelRoles.ListRow, ThemeSettings.Content.List).Add((ThemeSettings.Content.List.Focus, Shaped(PanelRoles.ListRow, None, Interaction.Focused))), Channels),
            Element(Plain(Role.Scrollbar, ThemeSettings.Content.Scrollbar), static state => [(Channel.Fill, state.Thumb), (Channel.Stroke, state.Border)]),
            Seq<(Token Token, Color Color)>(
                    (new Token(Role.Window, Channel.Fill), ThemeSettings.Content.Background),
                    (new Token(Role.Window, Channel.Text), ThemeSettings.Content.Text.Enabled),
                    (new Token(Role.Window, Channel.Text) { Condition = Condition.Disabled }, ThemeSettings.Content.Text.Disabled),
                    (new Token(Role.Panel, Channel.Fill), ThemeSettings.Content.Background),
                    (new Token(Role.Panel, Channel.Text), ThemeSettings.Content.Text.Enabled),
                    (new Token(Role.Panel, Channel.Text) { Condition = Condition.Disabled }, ThemeSettings.Content.Text.Disabled),
                    (new Token(Role.Accent, Channel.Fill), SystemColors.Highlight),
                    (new Token(Role.Label, Channel.Text), ThemeSettings.Content.Text.Enabled),
                    (new Token(Role.Label, Channel.Text) { Condition = Condition.Disabled }, ThemeSettings.Content.Text.Disabled),
                    (new Token(Role.Divider, Channel.Fill), ThemeSettings.Content.Divider),
                    (new Token(Role.Divider, Channel.Stroke), ThemeSettings.Content.Divider),
                    (new Token(Role.Selection, Channel.Fill), SystemColors.Highlight),
                    (new Token(Role.Selection, Channel.Text), SystemColors.HighlightText),
                    (new Token(Role.FocusRing, Channel.Stroke), SystemColors.Highlight),
                    (new Token(Role.Toolbar, Channel.Fill), ThemeSettings.Frame.Background),
                    (new Token(Role.Toolbar, Channel.Stroke), ThemeSettings.Frame.Edge),
                    (new Token(Role.Toolbar, Channel.Text), ThemeSettings.Frame.Text.Enabled),
                    (new Token(Role.Toolbar, Channel.Text) { Condition = Condition.Disabled }, ThemeSettings.Frame.Text.Disabled))
                .Concat(Handler.ToSeq().Bind(static handler => Seq<(Token Token, Color Color)>(
                    (new Token(Role.Window, Channel.Text) { Condition = Condition.Faulted }, handler.Faulted),
                    (new Token(Role.Window, Channel.Text) { Condition = Condition.Warning }, handler.Warning))))
                .Map(static row => (row.Token, Color: Scened(row.Color))))
        .Bind(static rows => rows);

    private static Seq<(Token Token, SKColorF Color)> Element<TState>(Seq<(TState State, Func<Channel, Token> Token)> states, Func<TState, Seq<(Channel Channel, Color Color)>> channels) =>
        from held in states
        from channel in channels(held.State)
        select (held.Token(channel.Channel), Scened(channel.Color));

    private static SKColorF Scened(Color color) => Color4f.FromArgb(color.A, color.R, color.G, color.B).ToSKColorF();

    private static Func<Channel, Token> Shaped(Role role, Option<Condition> condition, Option<Interaction> interaction) =>
        channel => new Token(role, channel) { Condition = condition, Interaction = interaction };

    private static Seq<(TState State, Func<Channel, Token> Token)> Plain<TState>(Role role, ThemeElement<TState> element) where TState : ThemeStateBase, new() =>
        [(element.Enabled, Shaped(role, None, None)), (element.EnabledHover, Shaped(role, None, Interaction.Hover)), (element.EnabledPressed, Shaped(role, None, Interaction.Pressed)),
         (element.Disabled, Shaped(role, Condition.Disabled, None))];

    private static Seq<(TState State, Func<Channel, Token> Token)> Checked<TState>(Role role, CheckedThemeElement<TState> element) where TState : CheckedThemeState, new() =>
        Plain(role, element) + [(element.Checked, Shaped(role, Condition.Selected, None)), (element.CheckedHover, Shaped(role, Condition.Selected, Interaction.Hover)),
                                (element.CheckedPressed, Shaped(role, Condition.Selected, Interaction.Pressed))];

    private static Seq<(ButtonThemeState State, Func<Channel, Token> Token)> Defaulted(Role role, ButtonThemeElement element) =>
        Checked(role, element) + [(element.Default, Shaped(role, Condition.Default, None)), (element.DefaultHover, Shaped(role, Condition.Default, Interaction.Hover)),
                                  (element.DefaultPressed, Shaped(role, Condition.Default, Interaction.Pressed))];

    private static Seq<(Channel Channel, Color Color)> Channels(ThemeState state) => [(Channel.Stroke, state.Border), (Channel.Fill, state.Background), (Channel.Text, state.Text)];

    // --- [CHANGE]
    public static readonly HostEvent<Unit> Changed =
        Subscriptions.Host(typeof(HostTheme), static () => Seq(EventKind.ThemeChanged, EventKind.AppSettingsChanged).Concat(Handler.Map(static handler => handler.Changed).ToSeq())
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
