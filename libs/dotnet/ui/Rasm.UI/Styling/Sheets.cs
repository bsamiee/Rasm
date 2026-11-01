using System.Numerics;
using IconPacks.Lucide;
using Rasm.UI.Motion;
using Rasm.UI.Media;
using Rasm.UI.Typography;
using SkiaSharp;
using Topten.RichTextKit;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StyleSheet(HashMap<Role, Look> Looks) : Monoid<StyleSheet> {
    // --- [SCALE]
    internal const float Control = 24f;
    internal const float Corner = 6f;
    internal const float Text = 13f;
    internal const float Compact = 0.8f;
    internal static readonly Duration Brief = Duration.FromMilliseconds(120);

    // --- [COLORS]
    private static readonly ColorValue Accent = new ColorValue.Host(Token.AccentColor);
    private static readonly ColorValue AccentText = new ColorValue.Host(Token.AccentColorText);
    private static readonly ColorValue FieldFill = new ColorValue.Host(Token.Field);
    private static readonly ColorValue PanelFill = Chrome(Role.Panel, Channel.Fill);
    private static readonly ColorValue PanelText = Chrome(Role.Panel, Channel.Text);
    private static readonly ColorValue ToolbarFill = Chrome(Role.Toolbar, Channel.Fill);
    private static readonly ColorValue Chosen = Chrome(Role.Button, Channel.Fill, condition: Condition.Selected);
    private static readonly ColorValue Clear = Chrome(Role.Button, Channel.Fill, condition: Condition.Borderless);
    private static readonly ColorValue FocusTint = Chrome(Role.FocusRing, Channel.Stroke);
    private static readonly ColorValue Inherited = new ColorValue.Current(Channel.Text);
    private static readonly ColorValue Secondary = Inherited with { Alpha = Fraction.Validated(0.6f).ThrowIfFail() };
    private static readonly ColorValue WellFill = Fixed(Css.Black, 1f);
    private static readonly ColorValue WellText = Fixed(Css.White, 0.7f);

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
        colors: HashMap<string, ColorValue>(("base", Chrome(Role.Panel, Channel.Text, 0.12f)), ("sweep", new ColorValue.Host(Token.Highlight)))).ThrowIfFail();
    private static readonly Material Glow = Material.Drawn.Validated(
        "uniform float2 size; layout(color) uniform half4 base; layout(color) uniform half4 glow; uniform shader grain; half4 main(float2 p) { float2 uv = p / size; float halo = 1.0 - smoothstep(0.0, 1.1, distance(uv, float2(0.15, -0.2))); float noise = float(grain.eval(p).r) - 0.5; half3 rgb = mix(base.rgb, glow.rgb, half(halo) * glow.a) + half3(noise * 0.012); return half4(rgb * base.a, base.a); }",
        colors: HashMap(("base", PanelFill), ("glow", FocusTint with { Alpha = Fraction.Validated(0.35f).ThrowIfFail() })),
        children: HashMap(("grain", (Material)new Material.Noise(Frequency: 0.9f, Octaves: 2)))).ThrowIfFail();
    private static readonly Material Spin = Material.Drawn.Validated(
        "uniform float2 size; uniform float time; layout(color) uniform half4 tint; half4 main(float2 p) { float2 c = p - size * 0.5; float r = min(size.x, size.y) * 0.5; half ring = half(1.0 - smoothstep(r * 0.12, r * 0.12 + 1.0, abs(length(c) - r * 0.72))); half tail = half(fract(atan(c.y, c.x) * 0.15915494 - time)); half a = tint.a * ring * tail; return half4(tint.rgb * a, a); }",
        colors: HashMap(("tint", Inherited))).ThrowIfFail();
    private static readonly Material Frost = new Material.Glass(Clear: false, [Filter.Css.Validated(FilterFunction.Blur, 17f).ThrowIfFail(), Filter.Css.Validated(FilterFunction.Saturate, 1.8f).ThrowIfFail()]);

    // --- [LOOKS]
    public static StyleSheet Empty { get; } = new(HashMap<Role, Look>());

    private static readonly Seq<Stroke> Bare = Seq<Stroke>();
    private static readonly Seq<Stroke> Edged = [new Stroke(new ColorValue.Current(Channel.Stroke))];
    private static readonly Shape Pill = new Shape.Capsule();
    private static readonly Seq<Shadow> Contact = [new Shadow(Fixed(Css.Black, 0.2f), Blur: 1f) { Y = 0.5f }];
    private static readonly Look Outlined = new() { Fills = Seq<Material>(), Strokes = Seq(new Stroke(PanelText)), Text = PanelText };
    private static readonly Look Frosted = new() { Fills = Seq(Frost, new Material.Solid(new ColorValue.Current(Channel.Fill) { Alpha = Fraction.Validated(0.6f).ThrowIfFail() })), Strokes = Bare };
    private static readonly Look Flat = new() { Strokes = Bare };
    private static readonly Look Hueless = new() { Fills = Solid(Greyed(new ColorValue.Current(Channel.Fill))) };
    private static readonly Look HuelessStates = Hueless with {
        StateLayers = toHashMap(toSeq(Interaction.Items).Choose(static interaction => interaction.Emphasis.Map(_ => (interaction, (ColorValue)Greyed(new ColorValue.Current(Channel.Fill) { Interaction = interaction }))))),
    };
    private static readonly Look WellSelection = new() { Fills = Solid(Fixed(Css.White, 0.39f)), Strokes = Bare };
    private static readonly Look Ringed = new() { Strokes = Seq(new Stroke(FocusTint)) };
    private static readonly Look Lined = new() { Strokes = Seq(new Stroke(new ColorValue.Current(Channel.Stroke)) { Cap = SKStrokeCap.Butt }) };
    private static readonly Look Surfaced = Look.Unpainted with {
        Fills = Solid(new ColorValue.Current(Channel.Fill)),
        Text = PanelText,
        Shadows = Lifted(1f),
        Motion = Critical,
        When = HashMap((Condition.Glass, Frosted), (Condition.Dark, new Look { Shadows = Lifted(2f) })),
    };
    private static readonly Look Quiet = Look.Unpainted with { Text = Secondary };
    private static readonly HashMap<Condition, Look> Edits = HashMap(
        (Condition.Modified, new Look { Text = Accent }),
        (Condition.Pending, new Look { Face = new TextFace { Italic = true } }));
    private static readonly HashMap<Condition, Look> Signaled = Signals(static signal => new Look { Text = Signal(signal) });

    public static StyleSheet Base { get; } = new(HashMap(
        (Role.Panel, new Look {
            Strokes = Bare,
            Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f)),
            When = HashMap(
                (Condition.Glass, Frosted),
                (Condition.Drawn, new Look { Fills = Seq(Glow), Strokes = Edged })),
        }),
        (Role.Toolbar, Flat with {
            Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, Corner)),
            When = HashMap(
                (Condition.Glass, Frosted),
                (Condition.Drawn, new Look {
                    Fills = Seq<Material>(new Material.Gradient(new ColorRamp.Smooth([new Stop(ToolbarFill, Fraction.MinValue), new Stop(PanelFill, Fraction.MaxValue)]), new GradientShape.Linear(90f))),
                    Strokes = Edged,
                    Shadows = Seq(new Shadow(Fixed(Css.Black, 0.25f), Blur: 1f) { Y = 0.5f }),
                })),
        }),
        (Role.Header, Flat with { Face = SemiBold, Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 8f), (Metric.Gap, 6f)) }),
        (Role.Section, Flat with { Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f), (Metric.Radius, Corner + 2f)) }),
        (Role.TabList, Flat with { Metrics = HashMap((Metric.Inset, 2f), (Metric.Gap, 2f), (Metric.Radius, Corner + 2f)) }),
        (Role.Tab, Flat with { Motion = Eased, Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 10f), (Metric.Gap, 6f)) }),
        (Role.StatusBar, Flat with { Face = TextFace.Tabular, Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 8f), (Metric.Gap, 8f), (Metric.FontSize, 0.85f)) }),
        (Role.Card, new Look {
            Metrics = HashMap((Metric.Inset, 12f), (Metric.Gap, 8f), (Metric.Radius, Corner + 2f)),
            When = HashMap((Condition.Glass, Frosted), (Condition.Neutral, Flat)) + Emphasized(Ringed),
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
        (Role.Stack, Look.Unpainted with { Metrics = HashMap((Metric.Gap, 12f)) }),
        (Role.Scrim, Look.Unpainted with { Fills = Solid(Fixed(Css.Black, 0.32f)), Motion = Critical }),
        (Role.Button, new Look {
            Motion = Eased,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f), (Metric.Radius, Corner)),
            When = HashMap(
                (Condition.Borderless, Flat),
                (Condition.Neutral, Flat),
                (Condition.Default, Flat),
                (Condition.Destructive, new Look { Text = Signal(Condition.Faulted) })) + Edits,
        }),
        (Role.Field, new Look {
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f), (Metric.Radius, Corner), (Metric.Slop, 0f)),
            When = HashMap((Condition.Borderless, Flat), (Condition.Neutral, Flat)) + Edits + Signaled,
            Within = HashMap(
                (Role.Handle, Look.Unpainted with { Metrics = HashMap((Metric.Extent, 0f)) }),
                (Role.Span, Look.Unpainted with { Fills = Solid(Chosen), Metrics = HashMap((Metric.Radius, Corner)) })),
        }),
        (Role.Toggle, new Look {
            Motion = Eased,
            StateLayers = StateLayers(FieldFill, PanelText),
            Metrics = HashMap((Metric.Extent, Metric.Icon.Default), (Metric.Radius, Corner - 2f)),
            When = HashMap((Condition.Selected, Flat), (Condition.Mixed, Flat)),
        }),
        (Role.Switch, Flat with {
            Motion = Eased,
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, Metric.Icon.Default + 2f), (Metric.Inset, 2f)),
            Within = HashMap((Role.Handle, Look.Unpainted with {
                Fills = Solid(FieldFill),
                Shadows = Contact,
                Motion = Underdamped,
                Shape = Pill,
                Metrics = HashMap((Metric.Extent, Metric.Icon.Default - 2f)),
            })),
        }),
        (Role.Segment, Flat with {
            Metrics = HashMap((Metric.Inset, 2f), (Metric.Gap, 2f), (Metric.Radius, Corner)),
            Within = HashMap((Role.Button, new Look {
                Fills = Solid(Clear),
                Strokes = Bare,
                StateLayers = HashMap<Interaction, ColorValue>(
                    (Interaction.Hover, Chrome(Role.Button, Channel.Fill, condition: Condition.Borderless, interaction: Interaction.Hover)),
                    (Interaction.Pressed, Chrome(Role.Button, Channel.Fill, condition: Condition.Borderless, interaction: Interaction.Pressed))),
                Metrics = HashMap((Metric.Radius, Corner - 2f)),
                When = HashMap((Condition.Selected, new Look { Fills = Solid(Chosen), Shadows = Contact })) + Edits,
            })),
        }),
        (Role.Chip, Flat with {
            Motion = Eased,
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, Control - 2f), (Metric.Inset, 8f), (Metric.Gap, 4f)),
            When = HashMap((Condition.Neutral, HuelessStates)),
            Within = HashMap((Role.Dismiss, new Look { Metrics = HashMap((Metric.Icon, Metric.Icon.Default - 4f)) })),
        }),
        (Role.Indicator, Flat with {
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, 6f)),
            When = Signals(static signal => new Look { Fills = Solid(Signal(signal)) }),
        }),
        (Role.Divider, Flat with { Metrics = HashMap((Metric.Extent, 1f)), When = HashMap((Condition.Neutral, new Look { Fills = Seq<Material>() })) }),
        (Role.Scrollbar, Flat with { Shape = Pill, Metrics = HashMap((Metric.Extent, 4f)) }),
        (Role.Track, new Look {
            Fills = Seq<Material>(),
            Text = PanelText,
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, 4f)),
            When = HashMap((Condition.Busy, new Look { Fills = Seq(Shimmer) })),
        }),
        (Role.Span, new Look {
            Fills = Solid(ToolbarFill), Strokes = Bare, Text = PanelText, Metrics = HashMap((Metric.Reach, 4f), (Metric.Radius, 2f)),
        }),
        (Role.Ring, Outlined with { Metrics = HashMap((Metric.Extent, 12f), (Metric.Reach, 6f)) }),
        (Role.Needle, Outlined with { Metrics = HashMap((Metric.Stroke, 2f), (Metric.Reach, 6f)) }),
        (Role.Stop, new Look {
            Fills = Solid(PanelFill),
            Strokes = Seq(new Stroke(PanelText)),
            Text = PanelText,
            Metrics = HashMap((Metric.Extent, 10f), (Metric.Reach, 6f)),
            When = HashMap((Condition.Selected, new Look { Fills = Solid(PanelText), Text = PanelFill })),
        }),
        (Role.Handle, new Look {
            Fills = Solid(FieldFill),
            Strokes = Seq(new Stroke(PanelText)),
            Text = PanelText,
            StateLayers = StateLayers(FieldFill, PanelText),
            Motion = Underdamped,
            Metrics = HashMap((Metric.Extent, 8f), (Metric.Reach, 6f)),
            When = HashMap((Condition.Modified, new Look { Fills = Solid(Accent) })),
        }),
        (Role.Spinner, Look.Unpainted with {
            Strokes = Seq(new Stroke(Accent)),
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, Metric.Icon.Default), (Metric.Stroke, 2f)),
            When = HashMap((Condition.Busy, Look.Unpainted with { Fills = Seq(Spin) })),
        }),
        (Role.FocusRing, Look.Unpainted with { Shadows = Seq(new Shadow(FocusTint, Blur: 0f) { Spread = 3f }), Motion = Eased }),
        (Role.Label, new Look {
            Metrics = HashMap((Metric.FontSize, 0.73f), (Metric.Gap, 4f)),
            When = Edits + Signaled,
        }),
        (Role.Title, Look.Unpainted with { Metrics = HashMap((Metric.FontSize, 1.69f)) }),
        (Role.Headline, Look.Unpainted with { Face = SemiBold, Metrics = HashMap((Metric.FontSize, 1f)) }),
        (Role.Caption, Quiet with { Metrics = HashMap((Metric.FontSize, 0.77f)) }),
        (Role.Placeholder, Quiet),
        (Role.Code, Look.Unpainted with { Face = TextFace.Tabular }),
        (Role.OverlayCaption, Look.Unpainted with { Text = Fixed(Css.White, 1f), Shadows = Seq(new Shadow(Fixed(Css.Black, 0.5f), Blur: 0f) { X = 1f, Y = 1f }) }),
        (Role.Keycap, Quiet with { Face = TextFace.Tabular }),
        (Role.Badge, Look.Unpainted with {
            Fills = Solid(Accent),
            Text = AccentText,
            Face = SemiBold.Combine(TextFace.Tabular),
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, 8f), (Metric.Inset, 3f), (Metric.FontSize, 0.77f)),
        }),
        (Role.Mark, Look.Unpainted with { Face = Marked(Condition.Warning), When = Signals(static signal => new Look { Face = Marked(signal) }) }),
        (Role.Emphasis, Look.Unpainted with { Face = new TextFace { Italic = true } }),
        (Role.Strong, Look.Unpainted with { Face = SemiBold }),
        (Role.Link, Look.Unpainted with { Text = Accent, Face = new TextFace { Underline = UnderlineStyle.Gapped } }),
        (Role.Stat, Look.Unpainted with { Face = SemiBold.Combine(TextFace.Tabular), Metrics = HashMap((Metric.FontSize, 2.15f)), When = Signaled }),
        (Role.ListRow, new Look {
            Strokes = Bare,
            Motion = Underdamped,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 6f)),
            When = HashMap((Condition.Neutral, HuelessStates)) + Edits + Signaled,
        }),
        (Role.Cell, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Extent, Control), (Metric.Inset, 6f), (Metric.Gap, 4f)),
            When = HashMap((Condition.Neutral, HuelessStates)) + Edits + Signaled,
        }),
        (Role.Gutter, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 4f), (Metric.FontSize, 0.85f)),
            When = HashMap((Condition.Selected, new Look { Text = PanelText })),
        }),
        (Role.Line, Flat with {
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Extent, Control - 4f), (Metric.Inset, 4f)),
            When = Signals(static signal => new Look { Fills = Solid(Signal(signal, 0.16f)) }),
        }),
        (Role.Lane, Flat with {
            Metrics = HashMap((Metric.Extent, Control + 4f), (Metric.Inset, 4f), (Metric.Gap, 4f)),
            Within = HashMap((Role.Card, new Look { Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 4f), (Metric.Radius, Corner)) })),
        }),
        (Role.Port, Look.Unpainted with {
            Fills = Solid(new ColorValue.Data()),
            Strokes = Seq(new Stroke(new ColorValue.Host(Token.Canvas))),
            Shape = Pill,
            Metrics = HashMap((Metric.Extent, 10f), (Metric.Reach, 4f), (Metric.Stroke, 1.5f)),
            When = Emphasized(Ringed),
        }),
        (Role.Wire, Look.Unpainted with {
            Strokes = Seq(new Stroke(new ColorValue.Data())),
            Metrics = HashMap((Metric.Stroke, 2f), (Metric.Reach, 4f)),
            When = Emphasized(Ringed with { Metrics = HashMap((Metric.Stroke, 2.5f)) }),
        }),
        (Role.Group, new Look {
            Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f), (Metric.Radius, Corner + 2f)),
            When = HashMap((Condition.Neutral, Flat), (Condition.Selected, Ringed)),
        }),
        (Role.Selection, new Look { When = HashMap((Condition.Neutral, Hueless)) }),
        (Role.Grid, Lined),
        (Role.GridFine, Lined),
        (Role.Trace, new Look { Metrics = HashMap((Metric.Stroke, 1.5f)) }),
        (Role.Series, new Look { Fills = Solid(new ColorValue.Data()), Metrics = HashMap((Metric.Stroke, 1.5f)), When = HashMap((Condition.Highlighted, Ringed)) }),
        (Role.Tick, Quiet with { Face = TextFace.Tabular, Metrics = HashMap((Metric.FontSize, 0.77f), (Metric.Gap, 4f)) }),
        (Role.Legend, Look.Unpainted with { Metrics = HashMap((Metric.Extent, 10f)) }),
        (Role.Crosshair, Look.Unpainted with { Strokes = Seq(new Stroke(Chrome(Role.Panel, Channel.Text, 0.5f)) { Cap = SKStrokeCap.Butt }), Metrics = HashMap((Metric.Stroke, 1f)) }),
        (Role.Readout, Look.Unpainted with {
            Fills = Solid(Chrome(Role.Panel, Channel.Fill, 0.85f)),
            Text = PanelText,
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Inset, 4f), (Metric.Radius, 4f)),
            When = Signaled,
        }),
        (Role.Icon, Look.Unpainted with { Fills = Solid(Inherited) }),
        (Role.Chevron, Glyph(Regular.ChevronRight, Secondary) with { When = HashMap((Condition.Selected, new Look { Metrics = HashMap((Metric.Turn, 90f)) })) }),
        (Role.Checkmark, Glyph(Regular.Check, Inherited) with {
            Fills = Seq<Material>(),
            When = HashMap((Condition.Selected, new Look { Fills = Solid(Inherited) }), (Condition.Mixed, Glyph(Regular.Minus, Inherited))),
        }),
        (Role.Dismiss, Glyph(Regular.X, Secondary)),
        (Role.Magnifier, Glyph(Regular.Search, Secondary)),
        (Role.Add, Glyph(Regular.Plus, Inherited)),
        (Role.Subtract, Glyph(Regular.Minus, Inherited)),
        (Role.Overflow, Glyph(Regular.Ellipsis, Secondary)),
        (Role.Revert, Glyph(Regular.Undo2, Inherited)),
        (Role.Pin, Glyph(Regular.Pin, Secondary) with { When = HashMap((Condition.Selected, new Look { Fills = Solid(Accent) })) }),
        (Role.Visibility, Glyph(Regular.Eye, Inherited) with { When = HashMap((Condition.Disabled, Glyph(Regular.EyeOff, Secondary))) }),
        (Role.Lock, Glyph(Regular.Lock, Inherited) with { When = HashMap((Condition.Disabled, Glyph(Regular.LockOpen, Secondary))) }),
        (Role.Status, Glyph(Regular.Info, Accent) with {
            When = HashMap((Condition.Success, Regular.CircleCheck), (Condition.Warning, Regular.TriangleAlert), (Condition.Faulted, Regular.CircleX)).Map(static (signal, glyph) => Glyph(glyph, Signal(signal))),
        }),
        (Role.Sorter, Glyph(Regular.ArrowUpDown, Secondary) with {
            When = HashMap((Condition.Selected, Glyph(Regular.ArrowUp, Inherited)), (Condition.Descending, Glyph(Regular.ArrowUp, Inherited) with { Metrics = HashMap((Metric.Turn, 180f)) })),
        }),
        (Role.Grip, Glyph(Regular.GripVertical, Secondary)),
        (Role.Well, new Look {
            Fills = Solid(WellFill),
            Strokes = Seq(new Stroke(Fixed(Css.Black, 0.5f))),
            Text = WellText,
            Metrics = HashMap((Metric.Extent, 18f), (Metric.Reach, 6f), (Metric.Inset, 7.2f), (Metric.Gap, 4f), (Metric.Radius, 3f)),
            Within = HashMap(
                (Role.Field, new Look { Fills = Solid(Fixed(Css.White, 0.16f)), Strokes = Seq(new Stroke(Fixed(Css.White, 0.2f))), Text = WellText }),
                (Role.Handle, new Look {
                    Fills = Solid(WellFill),
                    Strokes = Seq(new Stroke(Fixed(Css.White, 0.75f))),
                    Text = WellText,
                    StateLayers = StateLayers(WellFill, Fixed(Css.White, 1f)),
                    Motion = Underdamped,
                }),
                (Role.Grid, Ruled(0.2f, 0.4f)),
                (Role.GridFine, Ruled(0.08f, 0.2f)),
                (Role.Trace, new Look { Fills = Solid(Fixed(Css.White, 0.25f)), Strokes = Seq(new Stroke(Fixed(Css.White, 0.75f))), Blend = Blenders.Plus }),
                (Role.Series, new Look {
                    Fills = Solid(new ColorValue.Data { Alpha = Fraction.Validated(0.75f).ThrowIfFail() }),
                    Strokes = Seq(new Stroke(new ColorValue.Data())),
                    Blend = Blenders.Plus,
                    When = HashMap((Condition.Highlighted, Ringed), (Condition.DifferentiateWithoutColor, new Look {
                        Strokes = Seq(new Stroke(new ColorValue.Data()) {
                            Patterns = [[PathEffect.Dash.Validated([4f, 2f, 1f, 2f]).ThrowIfFail()], [], [PathEffect.Dash.Validated([4f, 2f]).ThrowIfFail()], [PathEffect.Dash.Validated([1f, 2f]).ThrowIfFail()]],
                        }),
                    })),
                }),
                (Role.Selection, WellSelection with { When = HashMap((Condition.Neutral, WellSelection)) }),
                (Role.Card, new Look { Text = PanelText, Metrics = HashMap((Metric.Inset, 0f), (Metric.Extent, Control)) }),
                (Role.Label, Look.Unpainted with { Text = WellText, Face = TextFace.Tabular })),
        })));

    // --- [MONOID]
    public StyleSheet Combine(StyleSheet rhs) => new(Looks.Union(rhs.Looks, Merge: static (_, under, over) => under.Combine(over)));

    // --- [ROWS]
    private static ColorValue.Host Chrome(Role role, Channel channel, float alpha = 1f, Option<Condition> condition = default, Option<Interaction> interaction = default) =>
        new(new Token(role, channel) { Condition = condition, Interaction = interaction }) { Alpha = Fraction.Validated(alpha).ThrowIfFail() };

    private static ColorValue.Fixed Fixed(Unicolour color, float alpha) => new(color) { Alpha = Fraction.Validated(alpha).ThrowIfFail() };

    private static Seq<Material> Solid(ColorValue color) => [new Material.Solid(color)];

    private static ColorValue.Relative Greyed(ColorValue color) => new(color, ColourSpace.Oklch) { Scale = new Vector4(1f, 0f, 1f, 1f) };

    private static ColorValue.Host Signal(Condition signal, float alpha = 1f) => Chrome(Role.Window, Channel.Text, alpha, signal);

    private static TextFace Marked(Condition signal) => new() { Background = Signal(signal, 0.4f) };

    private static Seq<Shadow> Lifted(float weight) => [new Shadow(Fixed(Css.Black, 0.12f * weight), Blur: 1f), new Shadow(Fixed(Css.Black, 0.14f * weight), Blur: 4f) { Y = 4f }];

    private static Look Glyph(string geometry, ColorValue color) =>
        Look.Unpainted with { Shape = new Shape.Icon(IconSet.Lucide, Shape.Outlined.Validated(geometry).ThrowIfFail()), Fills = Solid(color) };

    private static Look Ruled(float resting, float enhanced) =>
        new() {
            Fills = Seq<Material>(),
            Strokes = Seq(new Stroke(Fixed(Css.White, resting)) { Cap = SKStrokeCap.Butt }),
            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(Fixed(Css.White, enhanced)) { Cap = SKStrokeCap.Butt }) })),
        };

    private static HashMap<Interaction, ColorValue> StateLayers(ColorValue from, ColorValue to) =>
        toHashMap(toSeq(Interaction.Items).Choose(interaction => interaction.Emphasis.Map(emphasis => (interaction, (ColorValue)new ColorValue.Mixed(from, to, emphasis) { Space = ColourSpace.Rgb }))));

    private static HashMap<Condition, Look> Emphasized(Look look) => HashMap((Condition.Selected, look), (Condition.Highlighted, look));

    private static HashMap<Condition, Look> Signals(Func<Condition, Look> look) => toHashMap(Seq(Condition.Success, Condition.Warning, Condition.Faulted).Map(signal => (signal, look(signal))));
}
