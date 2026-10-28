using System.Runtime.CompilerServices;
using IconPacks.Lucide;
using Rasm.UI.Typography;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Role([CallerMemberName] string Name = "") {
    // --- [SURFACES]
    public static readonly Role Window = new();
    public static readonly Role Accent = new();
    public static readonly Role Panel = new();
    public static readonly Role Toolbar = new();
    public static readonly Role Header = new();
    public static readonly Role Section = new();
    public static readonly Role Tab = new();
    public static readonly Role TabList = new();
    public static readonly Role StatusBar = new();
    public static readonly Role Card = new();
    public static readonly Role Dialog = new();
    public static readonly Role Sheet = new();
    public static readonly Role Hud = new();
    public static readonly Role Scrim = new();
    public static readonly Role Popover = new();
    public static readonly Role Tooltip = new();
    public static readonly Role Menu = new();
    public static readonly Role Well = new();
    public static readonly Role Stack = new();

    // --- [CONTROLS]
    public static readonly Role Button = new();
    public static readonly Role Field = new();
    public static readonly Role Toggle = new();
    public static readonly Role Switch = new();
    public static readonly Role Segment = new();
    public static readonly Role Chip = new();
    public static readonly Role Indicator = new();
    public static readonly Role Divider = new();
    public static readonly Role Scrollbar = new();
    public static readonly Role Track = new();
    public static readonly Role Span = new();
    public static readonly Role Handle = new();
    public static readonly Role Needle = new();
    public static readonly Role Stop = new();
    public static readonly Role Ring = new();
    public static readonly Role Spinner = new();
    public static readonly Role Selection = new();
    public static readonly Role FocusRing = new();

    // --- [TEXT]
    public static readonly Role Label = new();
    public static readonly Role Title = new();
    public static readonly Role Headline = new();
    public static readonly Role Caption = new();
    public static readonly Role Placeholder = new();
    public static readonly Role Code = new();
    public static readonly Role OverlayCaption = new();
    public static readonly Role Keycap = new();
    public static readonly Role Badge = new();
    public static readonly Role Mark = new();
    public static readonly Role Stat = new();
    public static readonly Role Emphasis = new();
    public static readonly Role Strong = new();
    public static readonly Role Link = new();

    // --- [DATA]
    public static readonly Role ListRow = new();
    public static readonly Role Cell = new();
    public static readonly Role Gutter = new();
    public static readonly Role Line = new();
    public static readonly Role Lane = new();
    public static readonly Role Port = new();
    public static readonly Role Wire = new();
    public static readonly Role Group = new();
    public static readonly Role Grid = new();
    public static readonly Role GridFine = new();
    public static readonly Role Series = new();
    public static readonly Role Trace = new();
    public static readonly Role Tick = new();
    public static readonly Role Crosshair = new();
    public static readonly Role Readout = new();
    public static readonly Role Legend = new();

    // --- [GLYPHS]
    public static readonly Role Icon = new();
    public static readonly Role Chevron = new();
    public static readonly Role Checkmark = new();
    public static readonly Role Dismiss = new();
    public static readonly Role Magnifier = new();
    public static readonly Role Add = new();
    public static readonly Role Subtract = new();
    public static readonly Role Overflow = new();
    public static readonly Role Revert = new();
    public static readonly Role Pin = new();
    public static readonly Role Visibility = new();
    public static readonly Role Lock = new();
    public static readonly Role Status = new();
    public static readonly Role Sorter = new();
    public static readonly Role Grip = new();
}

public sealed record StyleSheet(HashMap<Role, Look> Looks) : Monoid<StyleSheet> {
    // --- [SCALE]
    internal const float Control = 24f;
    internal const float Corner = 6f;
    internal static readonly Duration Brief = Duration.FromMilliseconds(120);

    // --- [COLORS]
    private static readonly ColorValue Accent = new ColorValue.Host(Channel.Fill) { Role = Role.Accent };
    private static readonly ColorValue AccentText = new ColorValue.Host(Channel.Text) { Role = Role.Accent };
    private static readonly ColorValue PanelFill = Panel(Channel.Fill);
    private static readonly ColorValue PanelTint = Panel(Channel.Fill, 0.6f);
    private static readonly ColorValue PanelText = Panel(Channel.Text);
    private static readonly ColorValue Hairline = Panel(Channel.Text, 0.08f);
    private static readonly ColorValue FieldFill = new ColorValue.Host(Channel.Fill) { Role = Role.Field };
    private static readonly ColorValue ToolbarFill = new ColorValue.Host(Channel.Fill) { Role = Role.Toolbar };
    private static readonly ColorValue SelectionFill = new ColorValue.Host(Channel.Fill) { Role = Role.Selection };
    private static readonly ColorValue Chosen = new ColorValue.Host(Channel.Fill) { Role = Role.Button, Condition = Condition.Selected };
    private static readonly ColorValue Clear = new ColorValue.Host(Channel.Fill) { Role = Role.Button, Condition = Condition.Borderless };
    private static readonly ColorValue.Host FocusTint = new(Channel.Stroke) { Role = Role.FocusRing };
    private static readonly ColorValue Inherited = new ColorValue.Host(Channel.Text);
    private static readonly ColorValue Secondary = new ColorValue.Host(Channel.Text) { Alpha = Fraction.Validated(0.6f).ThrowIfFail() };
    private static readonly ColorValue Success = Signal(Condition.Success);
    private static readonly ColorValue Warning = Signal(Condition.Warning);
    private static readonly ColorValue Faulted = Signal(Condition.Faulted);
    private static readonly ColorValue WellFill = Black(1f);
    private static readonly ColorValue WellText = White(0.7f);
    private static readonly SKBlender Additive = SKBlender.CreateBlendMode(SKBlendMode.Plus);

    // --- [MOTION]
    private static readonly Factor Rest = Factor.Validated(0.001f).ThrowIfFail();
    private static readonly TimingCurve Eased = new TimingCurve.Eased(Brief, Easing.EaseOut);
    private static readonly TimingCurve Critical = new TimingCurve.Spring(Duration.FromMilliseconds(500), Factor.Unit, Rest);
    private static readonly TimingCurve Underdamped = new TimingCurve.Spring(Duration.FromMilliseconds(500), Factor.Validated(0.85f).ThrowIfFail(), Rest);

    // --- [FACES]
    private static readonly TextFace SemiBold = new() { Weight = SKFontStyleWeight.SemiBold };

    // --- [MATERIALS]
    private static readonly Material Shimmer = Material.Drawn.Validated(
        "uniform float2 size; uniform float time; layout(color) uniform half4 base; layout(color) uniform half4 sweep; half4 main(float2 p) { float head = fract(time * 0.8) * 1.6 - 0.3; half a = half(1.0 - smoothstep(0.0, 0.15, abs(p.x / max(size.x, 1.0) - head))); half4 c = mix(base, sweep, a); return half4(c.rgb * c.a, c.a); }",
        colors: HashMap(("base", Panel(Channel.Text, 0.12f)), ("sweep", SelectionFill))).ThrowIfFail();
    private static readonly Material Glow = Material.Drawn.Validated(
        "uniform float2 size; layout(color) uniform half4 base; layout(color) uniform half4 glow; uniform shader grain; half4 main(float2 p) { float2 uv = p / size; float halo = 1.0 - smoothstep(0.0, 1.1, distance(uv, float2(0.15, -0.2))); float noise = float(grain.eval(p).r) - 0.5; half3 rgb = mix(base.rgb, glow.rgb, half(halo) * glow.a) + half3(noise * 0.012); return half4(rgb * base.a, base.a); }",
        colors: HashMap(("base", PanelFill), ("glow", FocusTint with { Alpha = Fraction.Validated(0.35f).ThrowIfFail() })),
        children: HashMap(("grain", (Material)new Material.Noise(Frequency: 0.9f, Octaves: 2)))).ThrowIfFail();
    private static readonly Material Spin = Material.Drawn.Validated(
        "uniform float2 size; uniform float time; layout(color) uniform half4 tint; half4 main(float2 p) { float2 c = p - size * 0.5; float r = min(size.x, size.y) * 0.5; half ring = half(1.0 - smoothstep(r * 0.12, r * 0.12 + 1.0, abs(length(c) - r * 0.72))); half tail = half(fract(atan(c.y, c.x) * 0.15915494 - time)); half a = tint.a * ring * tail; return half4(tint.rgb * a, a); }",
        colors: HashMap(("tint", Inherited))).ThrowIfFail();
    private static readonly Material Frost = new Material.Glass(Clear: false, [Filter.Css.Validated(FilterFunction.Blur, 20f).ThrowIfFail(), Filter.Css.Validated(FilterFunction.Saturate, 1.8f).ThrowIfFail()]);

    // --- [LOOKS]
    private static readonly Seq<Stroke> Bare = Seq<Stroke>();
    private static readonly Seq<Shadow> Lifted = [new Shadow(Black(0.12f), Blur: 1f), new Shadow(Black(0.14f), Blur: 4f) { Y = 4f }];
    private static readonly Seq<Shadow> LiftedDark = [new Shadow(Black(0.24f), Blur: 1f), new Shadow(Black(0.28f), Blur: 4f) { Y = 4f }];
    private static readonly Seq<Shadow> Contact = [new Shadow(Black(0.2f), Blur: 1f) { Y = 0.5f }];
    private static readonly Look Outlined = new() { Fills = Seq<Material>(), Strokes = Seq(new Stroke(PanelText)), Text = PanelText };
    private static readonly Look Frosted = new() { Fills = Seq(Frost, new Material.Solid(PanelTint)), Strokes = Bare };
    private static readonly Look Flat = new() { Strokes = Bare };
    private static readonly Look Ringed = new() { Strokes = Seq(new Stroke(FocusTint)) };
    private static readonly Look Surfaced = Look.Unpainted with {
        Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill))),
        Text = PanelText,
        Shadows = Lifted,
        Motion = Critical,
        When = HashMap((Condition.Glass, Frosted), (Condition.Dark, new Look { Shadows = LiftedDark })),
    };
    private static readonly Look Quiet = Look.Unpainted with { Text = Secondary };
    private static readonly HashMap<Condition, Look> Edits = HashMap(
        (Condition.Modified, new Look { Text = Accent }),
        (Condition.Pending, new Look { Face = new TextFace { Italic = true } }));

    public static StyleSheet Empty { get; } = new(HashMap<Role, Look>());

    public static StyleSheet Base { get; } = new(HashMap(
        (Role.Panel, new Look {
            Strokes = Bare,
            Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 4f)) }),
                (Condition.Glass, Frosted),
                (Condition.Drawn, new Look { Fills = Seq(Glow), Strokes = Seq(new Stroke(Hairline)) })),
        }),
        (Role.Toolbar, Flat with {
            Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, Corner)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control)) }),
                (Condition.Glass, Frosted),
                (Condition.Drawn, new Look {
                    Fills = Seq<Material>(new Material.Gradient(new ColorRamp.Smooth([new Stop(ToolbarFill, Fraction.MinValue), new Stop(PanelFill, Fraction.MaxValue)]), new GradientShape.Linear(90f))),
                    Strokes = Seq(new Stroke(Hairline)),
                    Shadows = Seq(new Shadow(Black(0.25f), Blur: 1f) { Y = 0.5f }),
                })),
        }),
        (Role.Header, Flat with {
            Face = SemiBold,
            Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 8f), (Metric.Gap, 6f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f)) })),
        }),
        (Role.Section, Flat with {
            Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f), (Metric.Radius, Corner + 2f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Inset, 6f), (Metric.Gap, 4f)) })),
        }),
        (Role.TabList, Flat with { Metrics = HashMap((Metric.Inset, 2f), (Metric.Gap, 2f), (Metric.Radius, Corner + 2f)) }),
        (Role.Tab, Flat with {
            Motion = Eased,
            Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 10f), (Metric.Gap, 6f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 8f)) })),
        }),
        (Role.StatusBar, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 8f), (Metric.Gap, 8f), (Metric.FontSize, 0.85f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 6f)) })),
        }),
        (Role.Card, new Look {
            Metrics = HashMap((Metric.Inset, 12f), (Metric.Gap, 8f), (Metric.Radius, Corner + 2f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f)) }),
                (Condition.Selected, Ringed),
                (Condition.Highlighted, Ringed),
                (Condition.Glass, Frosted)),
        }),
        (Role.Dialog, Surfaced with { Metrics = HashMap((Metric.Inset, 16f), (Metric.Gap, 12f), (Metric.Radius, Corner * 2f), (Metric.Measure, Control * 18f)) }),
        (Role.Sheet, Surfaced with { Metrics = HashMap((Metric.Inset, 12f), (Metric.Gap, 8f), (Metric.Measure, Control * 14f)) }),
        (Role.Hud, Surfaced with {
            Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, Corner + 2f)),
            Within = HashMap((Role.Well, new Look { Metrics = HashMap((Metric.Extent, Control * 4f)) })),
        }),
        (Role.Popover, Surfaced with { Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 4f), (Metric.Radius, Corner + 4f), (Metric.Measure, Control * 24f)) }),
        (Role.Tooltip, Surfaced with { Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, Corner - 1f), (Metric.FontSize, 0.85f), (Metric.Measure, Control * 12f)) }),
        (Role.Menu, Surfaced with { Metrics = HashMap((Metric.Inset, 4f), (Metric.Radius, Corner)) }),
        (Role.Stack, Look.Unpainted with { Metrics = HashMap((Metric.Gap, 12f)), When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Gap, 8f)) })) }),
        (Role.Scrim, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(Black(0.32f))), Motion = Critical }),
        (Role.Button, new Look {
            Motion = Eased,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f), (Metric.Radius, Corner)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 4f)) }),
                (Condition.Borderless, Flat),
                (Condition.Default, Flat),
                (Condition.Destructive, new Look { Text = Faulted })) + Edits,
        }),
        (Role.Field, new Look {
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f), (Metric.Radius, Corner), (Metric.Slop, 0f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 4f), (Metric.Gap, 2f)) }),
                (Condition.Borderless, Flat)) + Edits + Signals(Look.Empty),
            Within = HashMap(
                (Role.Handle, Look.Unpainted with { Metrics = HashMap((Metric.Extent, 0f)) }),
                (Role.Span, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(Chosen)), Metrics = HashMap((Metric.Radius, Corner)) })),
        }),
        (Role.Toggle, new Look {
            Motion = Eased,
            StateLayers = StateLayers(FieldFill, PanelText),
            Metrics = HashMap((Metric.Extent, Metric.Icon.Default), (Metric.Radius, Corner - 2f)),
            When = HashMap((Condition.Selected, Flat), (Condition.Mixed, Flat)),
        }),
        (Role.Switch, Flat with {
            Motion = Eased,
            Metrics = HashMap((Metric.Extent, Metric.Icon.Default + 2f), (Metric.Inset, 2f), (Metric.Radius, (Metric.Icon.Default + 2f) / 2f)),
            Within = HashMap((Role.Handle, Look.Unpainted with {
                Fills = Seq<Material>(new Material.Solid(FieldFill)),
                Shadows = Contact,
                Motion = Underdamped,
                Metrics = HashMap((Metric.Extent, Metric.Icon.Default - 2f), (Metric.Radius, (Metric.Icon.Default - 2f) / 2f)),
            })),
        }),
        (Role.Segment, Flat with {
            Metrics = HashMap((Metric.Inset, 2f), (Metric.Gap, 2f), (Metric.Radius, Corner)),
            Within = HashMap((Role.Button, new Look {
                Fills = Seq<Material>(new Material.Solid(Clear)),
                Strokes = Bare,
                StateLayers = HashMap<Interaction, ColorValue>(
                    (Interaction.Hover, new ColorValue.Host(Channel.Fill) { Role = Role.Button, Condition = Condition.Borderless, Interaction = Interaction.Hover }),
                    (Interaction.Pressed, new ColorValue.Host(Channel.Fill) { Role = Role.Button, Condition = Condition.Borderless, Interaction = Interaction.Pressed })),
                Metrics = HashMap((Metric.Radius, Corner - 2f)),
                When = HashMap((Condition.Selected, new Look { Fills = Seq<Material>(new Material.Solid(Chosen)), Shadows = Contact })) + Edits,
            })),
        }),
        (Role.Chip, Flat with {
            Motion = Eased,
            Metrics = HashMap((Metric.Extent, Control - 2f), (Metric.Inset, 8f), (Metric.Gap, 4f), (Metric.Radius, (Control - 2f) / 2f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 6f), (Metric.Inset, 6f), (Metric.Radius, (Control - 6f) / 2f)) })),
            Within = HashMap((Role.Dismiss, new Look { Metrics = HashMap((Metric.Icon, Metric.Icon.Default - 4f)) })),
        }),
        (Role.Indicator, Flat with {
            Metrics = HashMap((Metric.Extent, 6f), (Metric.Radius, 3f)),
            When = HashMap(
                (Condition.Success, new Look { Fills = Seq<Material>(new Material.Solid(Success)) }),
                (Condition.Warning, new Look { Fills = Seq<Material>(new Material.Solid(Warning)) }),
                (Condition.Faulted, new Look { Fills = Seq<Material>(new Material.Solid(Faulted)) })),
        }),
        (Role.Divider, Flat with { Metrics = HashMap((Metric.Extent, 1f)) }),
        (Role.Scrollbar, Flat with { Metrics = HashMap((Metric.Extent, 4f), (Metric.Radius, 2f)) }),
        (Role.Track, new Look {
            Fills = Seq<Material>(),
            Strokes = Seq(new Stroke(Panel(Channel.Text, 0.2f))),
            Text = PanelText,
            Metrics = HashMap((Metric.Extent, 4f), (Metric.Radius, 2f)),
            When = HashMap((Condition.Busy, new Look { Fills = Seq(Shimmer) })),
        }),
        (Role.Span, new Look {
            Fills = Seq<Material>(new Material.Solid(ToolbarFill)), Strokes = Bare, Text = PanelText, Metrics = HashMap((Metric.Reach, 4f), (Metric.Radius, 2f)),
        }),
        (Role.Ring, Outlined with { Metrics = HashMap((Metric.Extent, 12f), (Metric.Reach, 6f)) }),
        (Role.Needle, Outlined with { Metrics = HashMap((Metric.Stroke, 2f), (Metric.Reach, 6f)) }),
        (Role.Stop, new Look {
            Fills = Seq<Material>(new Material.Solid(PanelFill)),
            Strokes = Seq(new Stroke(PanelText)),
            Text = PanelText,
            Metrics = HashMap((Metric.Extent, 10f), (Metric.Reach, 6f)),
            When = HashMap((Condition.Selected, new Look { Fills = Seq<Material>(new Material.Solid(PanelText)), Text = PanelFill })),
        }),
        (Role.Handle, new Look {
            Fills = Seq<Material>(new Material.Solid(FieldFill)),
            Strokes = Seq(new Stroke(PanelText)),
            Text = PanelText,
            StateLayers = StateLayers(FieldFill, PanelText),
            Motion = Underdamped,
            Metrics = HashMap((Metric.Extent, 8f), (Metric.Reach, 6f)),
            When = HashMap((Condition.Modified, new Look { Fills = Seq<Material>(new Material.Solid(Accent)) })),
        }),
        (Role.Spinner, Look.Unpainted with {
            Strokes = Seq(new Stroke(Accent)),
            Metrics = HashMap((Metric.Extent, Metric.Icon.Default), (Metric.Radius, Metric.Icon.Default / 2f), (Metric.Stroke, 2f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, 12f), (Metric.Radius, 6f)) }),
                (Condition.Busy, Look.Unpainted with { Fills = Seq(Spin) })),
        }),
        (Role.FocusRing, Look.Unpainted with { Shadows = Seq(new Shadow(FocusTint, Blur: 0f) { Spread = 3f }), Motion = Eased }),
        (Role.Label, new Look {
            Metrics = HashMap((Metric.FontSize, 0.73f), (Metric.Gap, 4f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Gap, 2f)) })) + Edits + Signals(Look.Unpainted),
        }),
        (Role.Title, Look.Unpainted with { Metrics = HashMap((Metric.FontSize, 1.69f)) }),
        (Role.Headline, Look.Unpainted with { Face = SemiBold, Metrics = HashMap((Metric.FontSize, 1f)) }),
        (Role.Caption, Quiet with { Metrics = HashMap((Metric.FontSize, 0.77f)) }),
        (Role.Placeholder, Quiet),
        (Role.Code, Look.Unpainted with { Face = TextFace.Tabular }),
        (Role.OverlayCaption, Look.Unpainted with { Text = White(1f), Shadows = Seq(new Shadow(Black(0.5f), Blur: 0f) { X = 1f, Y = 1f }) }),
        (Role.Keycap, Quiet with { Face = TextFace.Tabular }),
        (Role.Badge, Look.Unpainted with {
            Fills = Seq<Material>(new Material.Solid(Accent)),
            Text = AccentText,
            Face = SemiBold.Combine(TextFace.Tabular),
            Metrics = HashMap((Metric.Extent, 8f), (Metric.Inset, 3f), (Metric.Radius, 8f), (Metric.FontSize, 0.77f)),
        }),
        (Role.Mark, Look.Unpainted with { Face = Marked(Condition.Warning), When = toHashMap(Seq(Condition.Success, Condition.Faulted).Map(static signal => (signal, new Look { Face = Marked(signal) }))) }),
        (Role.Emphasis, Look.Unpainted with { Face = new TextFace { Italic = true } }),
        (Role.Strong, Look.Unpainted with { Face = SemiBold }),
        (Role.Link, Look.Unpainted with { Text = Accent, Face = new TextFace { Underline = UnderlineStyle.Gapped } }),
        (Role.Stat, Look.Unpainted with { Face = SemiBold.Combine(TextFace.Tabular), Metrics = HashMap((Metric.FontSize, 2.15f)), When = Signals(Look.Unpainted) }),
        (Role.ListRow, new Look {
            Strokes = Bare,
            Motion = Underdamped,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 6f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 4f)) })) + Edits + Signals(Look.Empty),
        }),
        (Role.Cell, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 4f)) })) + Edits + Signals(Look.Empty),
        }),
        (Role.Gutter, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 4f), (Metric.FontSize, 0.85f)),
            When = HashMap((Condition.Selected, new Look { Text = PanelText })),
        }),
        (Role.Line, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 4f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 6f)) }),
                (Condition.Success, new Look { Fills = Seq<Material>(new Material.Solid(Signal(Condition.Success, 0.16f))) }),
                (Condition.Faulted, new Look { Fills = Seq<Material>(new Material.Solid(Signal(Condition.Faulted, 0.16f))) })),
        }),
        (Role.Lane, Flat with {
            Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 4f), (Metric.Gap, 4f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, Control - 2f)) })),
            Within = HashMap((Role.Card, new Look {
                Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 4f), (Metric.Radius, Corner)),
                When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Inset, 2f)) })),
            })),
        }),
        (Role.Port, Look.Unpainted with {
            Fills = Seq<Material>(new Material.Solid(new ColorValue.Data())),
            Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Fill) { Role = Role.Window })),
            Metrics = HashMap((Metric.Extent, 10f), (Metric.Radius, 5f), (Metric.Reach, 4f), (Metric.Stroke, 1.5f)),
            When = HashMap((Condition.Selected, Ringed), (Condition.Highlighted, Ringed)),
        }),
        (Role.Wire, Look.Unpainted with {
            Strokes = Seq(new Stroke(new ColorValue.Data())),
            Metrics = HashMap((Metric.Stroke, 2f), (Metric.Reach, 4f)),
            When = HashMap((Condition.Selected, Ringed with { Metrics = HashMap((Metric.Stroke, 2.5f)) }), (Condition.Highlighted, Ringed with { Metrics = HashMap((Metric.Stroke, 2.5f)) })),
        }),
        (Role.Group, new Look {
            Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f), (Metric.Radius, Corner + 2f)),
            When = HashMap((Condition.Selected, Ringed)),
        }),
        (Role.Grid, new Look { Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Stroke)) { Cap = SKStrokeCap.Butt }) }),
        (Role.GridFine, new Look { Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Stroke)) { Cap = SKStrokeCap.Butt }) }),
        (Role.Trace, new Look { Metrics = HashMap((Metric.Stroke, 1.5f)) }),
        (Role.Series, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Data())), Metrics = HashMap((Metric.Stroke, 1.5f)), When = HashMap((Condition.Highlighted, Ringed)) }),
        (Role.Tick, Quiet with { Face = TextFace.Tabular, Metrics = HashMap((Metric.FontSize, 0.77f), (Metric.Gap, 4f)) }),
        (Role.Legend, Look.Unpainted with { Metrics = HashMap((Metric.Extent, 10f)) }),
        (Role.Crosshair, Look.Unpainted with { Strokes = Seq(new Stroke(Panel(Channel.Text, 0.5f)) { Cap = SKStrokeCap.Butt }), Metrics = HashMap((Metric.Stroke, 1f)) }),
        (Role.Readout, Look.Unpainted with {
            Fills = Seq<Material>(new Material.Solid(Panel(Channel.Fill, 0.85f))),
            Text = PanelText,
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Inset, 4f), (Metric.Radius, 4f)),
            When = Signals(Look.Empty),
        }),
        (Role.Icon, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(Inherited)) }),
        (Role.Chevron, Glyph(Regular.ChevronRight, Secondary) with { When = HashMap((Condition.Selected, new Look { Metrics = HashMap((Metric.Turn, 90f)) })) }),
        (Role.Checkmark, Glyph(Regular.Check, Inherited) with {
            Fills = Seq<Material>(),
            When = HashMap((Condition.Selected, new Look { Fills = Seq<Material>(new Material.Solid(Inherited)) }), (Condition.Mixed, Glyph(Regular.Minus, Inherited))),
        }),
        (Role.Dismiss, Glyph(Regular.X, Secondary)),
        (Role.Magnifier, Glyph(Regular.Search, Secondary)),
        (Role.Add, Glyph(Regular.Plus, Inherited)),
        (Role.Subtract, Glyph(Regular.Minus, Inherited)),
        (Role.Overflow, Glyph(Regular.Ellipsis, Secondary)),
        (Role.Revert, Glyph(Regular.Undo2, Inherited)),
        (Role.Pin, Glyph(Regular.Pin, Secondary) with { When = HashMap((Condition.Selected, new Look { Fills = Seq<Material>(new Material.Solid(Accent)) })) }),
        (Role.Visibility, Glyph(Regular.Eye, Inherited) with { When = HashMap((Condition.Disabled, Glyph(Regular.EyeOff, Secondary))) }),
        (Role.Lock, Glyph(Regular.Lock, Inherited) with { When = HashMap((Condition.Disabled, Glyph(Regular.LockOpen, Secondary))) }),
        (Role.Status, Glyph(Regular.Info, Accent) with {
            When = HashMap((Condition.Success, Glyph(Regular.CircleCheck, Success)), (Condition.Warning, Glyph(Regular.TriangleAlert, Warning)), (Condition.Faulted, Glyph(Regular.CircleX, Faulted))),
        }),
        (Role.Sorter, Glyph(Regular.ArrowUpDown, Secondary) with {
            When = HashMap((Condition.Selected, Glyph(Regular.ArrowUp, Inherited)), (Condition.Descending, Glyph(Regular.ArrowUp, Inherited) with { Metrics = HashMap((Metric.Turn, 180f)) })),
        }),
        (Role.Grip, Glyph(Regular.GripVertical, Secondary)),
        (Role.Well, new Look {
            Fills = Seq<Material>(new Material.Solid(WellFill)),
            Strokes = Seq(new Stroke(Black(0.5f))),
            Text = WellText,
            Metrics = HashMap((Metric.Extent, 18f), (Metric.Reach, 6f), (Metric.Inset, 7.2f), (Metric.Gap, 4f), (Metric.Radius, 3f)),
            Within = HashMap(
                (Role.Field, new Look { Fills = Seq<Material>(new Material.Solid(White(0.16f))), Strokes = Seq(new Stroke(White(0.2f))), Text = WellText }),
                (Role.Handle, new Look {
                    Fills = Seq<Material>(new Material.Solid(WellFill)),
                    Strokes = Seq(new Stroke(White(0.75f))),
                    Text = WellText,
                    StateLayers = StateLayers(WellFill, White(1f)),
                    Motion = Underdamped,
                }),
                (Role.Grid, Ruled(0.2f, 0.4f)),
                (Role.GridFine, Ruled(0.08f, 0.2f)),
                (Role.Trace, new Look { Fills = Seq<Material>(new Material.Solid(White(0.25f))), Strokes = Seq(new Stroke(White(0.75f))), Blend = Additive }),
                (Role.Series, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Data { Alpha = Fraction.Validated(0.75f).ThrowIfFail() })),
                    Strokes = Seq(new Stroke(new ColorValue.Data())),
                    Blend = Additive,
                    When = HashMap((Condition.Highlighted, Ringed), (Condition.DifferentiateWithoutColor, new Look {
                        Strokes = Seq(new Stroke(new ColorValue.Data()) {
                            Patterns = [[PathEffect.Dash.Validated([4f, 2f, 1f, 2f]).ThrowIfFail()], [], [PathEffect.Dash.Validated([4f, 2f]).ThrowIfFail()], [PathEffect.Dash.Validated([1f, 2f]).ThrowIfFail()]],
                        }),
                    })),
                }),
                (Role.Selection, new Look { Fills = Seq<Material>(new Material.Solid(White(0.39f))), Strokes = Bare }),
                (Role.Card, new Look { Text = PanelText, Metrics = HashMap((Metric.Inset, 0f), (Metric.Extent, Control)) }),
                (Role.Label, Look.Unpainted with { Text = WellText, Face = TextFace.Tabular })),
        })));

    // --- [MONOID]
    public StyleSheet Combine(StyleSheet rhs) => new(Looks.Union(rhs.Looks, Merge: static (_, under, over) => under.Combine(over)));

    // --- [ROWS]
    private static ColorValue.Host Panel(Channel channel, float alpha = 1f) => new(channel) { Role = Role.Panel, Alpha = Fraction.Validated(alpha).ThrowIfFail() };

    private static ColorValue.Host Signal(Condition condition, float alpha = 1f) => new(Channel.Text) { Role = Role.Window, Condition = condition, Alpha = Fraction.Validated(alpha).ThrowIfFail() };

    private static TextFace Marked(Condition signal) => new() { Background = Some<ColorValue>(Signal(signal, 0.4f)) };

    private static ColorValue.Fixed White(float alpha) => new(Css.White) { Alpha = Fraction.Validated(alpha).ThrowIfFail() };

    private static ColorValue.Fixed Black(float alpha) => new(Css.Black) { Alpha = Fraction.Validated(alpha).ThrowIfFail() };

    private static Look Glyph(string geometry, ColorValue color) => Look.Unpainted with { Shape = new Shape.Icon(IconSet.Lucide, geometry), Fills = Seq<Material>(new Material.Solid(color)) };

    private static Look Ruled(float resting, float enhanced) =>
        new() {
            Fills = Seq<Material>(),
            Strokes = Seq(new Stroke(White(resting)) { Cap = SKStrokeCap.Butt }),
            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(White(enhanced)) { Cap = SKStrokeCap.Butt }) })),
        };

    private static HashMap<Interaction, ColorValue> StateLayers(ColorValue from, ColorValue to) =>
        toHashMap(toSeq(Interaction.Items).Choose(interaction => interaction.Emphasis.Map(emphasis => (interaction, (ColorValue)new ColorValue.Mixed(from, to, emphasis) { Space = ColourSpace.Rgb }))));

    private static HashMap<Condition, Look> Signals(Look under) =>
        HashMap((Condition.Success, under with { Text = Success }), (Condition.Warning, under with { Text = Warning }), (Condition.Faulted, under with { Text = Faulted }));
}
