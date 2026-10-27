using System.Runtime.CompilerServices;
using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Role([CallerMemberName] string Name = "") {
    public static readonly Role Window = new();
    public static readonly Role Accent = new();
    public static readonly Role Panel = new();
    public static readonly Role Toolbar = new();
    public static readonly Role Button = new();
    public static readonly Role Field = new();
    public static readonly Role Divider = new();
    public static readonly Role Scrollbar = new();
    public static readonly Role Label = new();
    public static readonly Role Track = new();
    public static readonly Role Span = new();
    public static readonly Role Handle = new();
    public static readonly Role Grid = new();
    public static readonly Role Selection = new();
    public static readonly Role FocusRing = new();
    public static readonly Role Well = new();
    public static readonly Role Series = new();
    public static readonly Role Trace = new();
    public static readonly Role GridFine = new();
    public static readonly Role OverlayCaption = new();
    public static readonly Role Needle = new();
    public static readonly Role Stop = new();
    public static readonly Role Ring = new();
    public static readonly Role Title = new();
    public static readonly Role Headline = new();
    public static readonly Role Caption = new();
    public static readonly Role Code = new();
    public static readonly Role Tick = new();
    public static readonly Role Crosshair = new();
    public static readonly Role Readout = new();
    public static readonly Role Legend = new();
    public static readonly Role ListRow = new();
    public static readonly Role Spinner = new();
    public static readonly Role Badge = new();
    public static readonly Role Mark = new();
    public static readonly Role Keycap = new();
    public static readonly Role Scrim = new();
    public static readonly Role Popover = new();
    public static readonly Role Tooltip = new();
    public static readonly Role Menu = new();
}

public sealed record StyleSheet(HashMap<Role, Look> Looks) : Monoid<StyleSheet> {
    // --- [COLORS]
    private static readonly ColorValue Accent = new ColorValue.Host(Channel.Fill) { Role = Role.Accent, Fallback = Some<ColorValue>(new ColorValue.Fixed(new Unicolour(ColourSpace.Oklch, 0.62, 0.19, 264))) };
    private static readonly ColorValue PanelFill = Panel(Channel.Fill);
    private static readonly ColorValue PanelTint = Panel(Channel.Fill, 0.6f);
    private static readonly ColorValue PanelText = Panel(Channel.Text);
    private static readonly ColorValue PanelRule = Panel(Channel.Text, 0.08f);
    private static readonly ColorValue FieldFill = new ColorValue.Host(Channel.Fill) { Role = Role.Field, Fallback = Some(PanelFill) };
    private static readonly ColorValue ToolbarFill = new ColorValue.Host(Channel.Fill) { Role = Role.Toolbar, Fallback = Some(PanelFill) };
    private static readonly ColorValue SelectionFill = new ColorValue.Host(Channel.Fill) { Role = Role.Selection, Fallback = Some<ColorValue>(new ColorValue.Mixed(Accent, PanelFill, Fraction.Validated(0.6f).ThrowIfFail())) };
    private static readonly ColorValue.Host FocusTint = new(Channel.Stroke) { Role = Role.FocusRing, Fallback = Some(Accent) };
    private static readonly ColorValue Secondary = new ColorValue.Host(Channel.Text) { Alpha = Fraction.Validated(0.6f).ThrowIfFail() };
    private static readonly ColorValue AccentText = new ColorValue.Contrasting(Accent, [new ColorValue.Host(Channel.Text) { Role = Role.Window }, new ColorValue.Host(Channel.Fill) { Role = Role.Window }]);
    private static readonly ColorValue Success = Signal(Condition.Success, new Unicolour(ColourSpace.Oklch, 0.72, 0.17, 150));
    private static readonly ColorValue Warning = Signal(Condition.Warning, new Unicolour(ColourSpace.Oklch, 0.77, 0.17, 64));
    private static readonly ColorValue Faulted = Signal(Condition.Faulted, new Unicolour(ColourSpace.Oklch, 0.63, 0.24, 29));
    private static readonly ColorValue Umbra = new ColorValue.Fixed("#00000059");
    private static readonly ColorValue WellText = new ColorValue.Fixed("#FFFFFFB3");
    private static readonly ColorValue WellFill = new ColorValue.Fixed(Css.Black);
    private static readonly SKBlender Additive = SKBlender.CreateBlendMode(SKBlendMode.Plus);

    // --- [MOTION]
    private static readonly Factor Rest = Factor.Validated(0.001f).ThrowIfFail();
    private static readonly TimingCurve Eased = new TimingCurve.Eased(Duration.FromMilliseconds(120), Easing.EaseOut);
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
        colors: HashMap(("tint", (ColorValue)new ColorValue.Host(Channel.Text)))).ThrowIfFail();
    private static readonly Material Frost = new Material.Glass(Clear: false, [Filter.Css.Validated(FilterFunction.Blur, 20f).ThrowIfFail(), Filter.Css.Validated(FilterFunction.Saturate, 1.8f).ThrowIfFail()]);

    // --- [LOOKS]
    private static readonly Look Outlined = new() { Fills = Seq<Material>(), Strokes = Seq(new Stroke(PanelText)), Text = PanelText };
    private static readonly Look Frosted = new() { Fills = Seq(Frost, new Material.Solid(PanelTint)), Strokes = Seq<Stroke>() };
    private static readonly Look Unstroked = new() { Strokes = Seq<Stroke>() };
    private static readonly Look Surfaced = Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(PanelFill)), Text = PanelText, Shadows = Seq(new Shadow(Umbra, Blur: 3f) { Y = 1f }), Motion = Critical, When = HashMap((Condition.Glass, Frosted)) };
    private static readonly Look Quiet = Look.Unpainted with { Text = Secondary };
    private static readonly HashMap<Condition, Look> Edits = HashMap(
        (Condition.Modified, new Look { Text = SelectionFill, Face = SemiBold }),
        (Condition.Pending, new Look { Face = new TextFace { Italic = true } }));

    public static StyleSheet Empty { get; } = new(HashMap<Role, Look>());

    public static StyleSheet Base { get; } = new(HashMap(
        (Role.Panel, new Look {
            Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 6f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 4f)) }),
                (Condition.Glass, Frosted),
                (Condition.Drawn, new Look { Fills = Seq(Glow), Strokes = Seq(new Stroke(PanelRule)) })),
        }),
        (Role.Toolbar, new Look {
            Metrics = HashMap((Metric.Extent, 28f), (Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, 6f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, 22f)) }),
                (Condition.Glass, Frosted),
                (Condition.Drawn, new Look {
                    Fills = Seq<Material>(new Material.Gradient(
                        new ColorRamp.Smooth([new Stop(ToolbarFill, Fraction.MinValue), new Stop(PanelFill, Fraction.MaxValue)]), new GradientShape.Linear(90f))),
                    Strokes = Seq(new Stroke(PanelRule)),
                    Shadows = Seq(new Shadow(new ColorValue.Fixed("#00000040"), Blur: 1f) { Y = 0.5f }),
                })),
        }),
        (Role.Button, new Look {
            Metrics = HashMap((Metric.Extent, 22f), (Metric.Inset, 4f), (Metric.Radius, 5f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, 18f), (Metric.Inset, 3f)) }),
                (Condition.Borderless, Unstroked),
                (Condition.Default, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Condition = Condition.Default, Fallback = Some(Accent) })),
                    Text = new ColorValue.Host(Channel.Text) { Condition = Condition.Default, Fallback = Some(AccentText) },
                }),
                (Condition.Destructive, new Look { Text = Faulted })),
        }),
        (Role.Field, new Look {
            Metrics = HashMap((Metric.Extent, 22f), (Metric.Inset, 4f), (Metric.Radius, 4f), (Metric.Slop, 0f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, 18f), (Metric.Inset, 3f)) }),
                (Condition.Borderless, Unstroked)) + Edits + Signals(Look.Empty),
        }),
        (Role.Divider, new Look { Metrics = HashMap((Metric.Extent, 1f)) }),
        (Role.Scrollbar, new Look { Metrics = HashMap((Metric.Extent, 4f), (Metric.Radius, 2f)) }),
        (Role.Label, new Look { Metrics = HashMap((Metric.FontSize, 0.73f)), When = Edits + Signals(Look.Unpainted) }),
        (Role.Track, new Look {
            Fills = Seq<Material>(),
            Strokes = Seq(new Stroke(Panel(Channel.Text, 0.2f))),
            Text = PanelText,
            Metrics = HashMap((Metric.Extent, 4f), (Metric.Radius, 2f)),
            When = HashMap((Condition.Busy, new Look { Fills = Seq(Shimmer) })),
        }),
        (Role.Span, new Look {
            Fills = Seq<Material>(new Material.Solid(ToolbarFill)), Strokes = Seq<Stroke>(), Text = PanelText, Metrics = HashMap((Metric.Reach, 4f), (Metric.Radius, 2f)),
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
            When = HashMap((Condition.Modified, new Look { Fills = Seq<Material>(new Material.Solid(SelectionFill)) })),
        }),
        (Role.Grid, new Look { Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Stroke)) { Cap = SKStrokeCap.Butt }) }),
        (Role.GridFine, new Look { Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Stroke)) { Cap = SKStrokeCap.Butt }) }),
        (Role.FocusRing, Look.Unpainted with { Shadows = Seq(new Shadow(FocusTint, Blur: 0f) { Spread = 3f }), Motion = Eased }),
        (Role.OverlayCaption, Look.Unpainted with {
            Text = new ColorValue.Fixed(Css.White),
            Shadows = Seq(new Shadow(new ColorValue.Fixed("#00000080"), Blur: 0f) { X = 1f, Y = 1f }),
        }),
        (Role.Trace, new Look { Metrics = HashMap((Metric.Stroke, 1.5f)) }),
        (Role.Series, new Look { Metrics = HashMap((Metric.Stroke, 1.5f)) }),
        (Role.Title, Look.Unpainted with { Metrics = HashMap((Metric.FontSize, 1.69f)) }),
        (Role.Headline, Look.Unpainted with { Face = SemiBold, Metrics = HashMap((Metric.FontSize, 1f)) }),
        (Role.Caption, Quiet with { Metrics = HashMap((Metric.FontSize, 0.77f)) }),
        (Role.Code, Look.Unpainted with { Face = TextFace.Tabular }),
        (Role.Tick, Quiet with { Face = TextFace.Tabular, Metrics = HashMap((Metric.FontSize, 0.77f), (Metric.Gap, 4f)) }),
        (Role.Keycap, Quiet with { Face = TextFace.Tabular }),
        (Role.Legend, Look.Unpainted with { Metrics = HashMap((Metric.Extent, 10f)) }),
        (Role.Crosshair, Look.Unpainted with {
            Strokes = Seq(new Stroke(Panel(Channel.Text, 0.5f)) { Cap = SKStrokeCap.Butt }),
            Metrics = HashMap((Metric.Stroke, 1f)),
        }),
        (Role.Readout, Look.Unpainted with {
            Fills = Seq<Material>(new Material.Solid(Panel(Channel.Fill, 0.85f))),
            Text = PanelText,
            Face = TextFace.Tabular,
            Metrics = HashMap((Metric.Inset, 4f), (Metric.Radius, 4f)),
            When = Signals(Look.Empty),
        }),
        (Role.ListRow, new Look {
            Motion = Underdamped,
            Metrics = HashMap((Metric.Extent, 20f), (Metric.Inset, 4f)),
            When = HashMap((Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, 17f)) })) + Edits + Signals(Look.Empty),
        }),
        (Role.Spinner, Look.Unpainted with {
            Strokes = Seq(new Stroke(Accent)),
            Metrics = HashMap((Metric.Extent, 16f), (Metric.Radius, 8f), (Metric.Stroke, 2f)),
            When = HashMap(
                (Condition.Compact, new Look { Metrics = HashMap((Metric.Extent, 12f), (Metric.Radius, 6f)) }),
                (Condition.Busy, Look.Unpainted with { Fills = Seq(Spin) })),
        }),
        (Role.Badge, Look.Unpainted with {
            Fills = Seq<Material>(new Material.Solid(Accent)),
            Text = AccentText,
            Face = SemiBold.Combine(TextFace.Tabular),
            Metrics = HashMap((Metric.Extent, 8f), (Metric.Inset, 3f), (Metric.Radius, 8f), (Metric.FontSize, 0.77f)),
        }),
        (Role.Mark, Look.Unpainted with { Face = new TextFace { Background = Some<ColorValue>(new ColorValue.Fixed("#FFD60A66")) } }),
        (Role.Scrim, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#0000004D"))), Motion = Critical }),
        (Role.Popover, Surfaced with { Metrics = HashMap((Metric.Inset, 8f), (Metric.Gap, 4f), (Metric.Radius, 10f)) }),
        (Role.Tooltip, Surfaced with { Metrics = HashMap((Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, 5f), (Metric.FontSize, 0.85f)) }),
        (Role.Menu, Surfaced with { Metrics = HashMap((Metric.Inset, 4f), (Metric.Radius, 6f)) }),
        (Role.Well, new Look {
            Fills = Seq<Material>(new Material.Solid(WellFill)),
            Strokes = Seq(new Stroke(new ColorValue.Fixed("#00000080"))),
            Text = WellText,
            Metrics = HashMap((Metric.Extent, 18f), (Metric.Reach, 6f), (Metric.Inset, 7.2f), (Metric.Gap, 4f), (Metric.Radius, 3f)),
            Within = HashMap(
                (Role.Field, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#292929FF"))), Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))), Text = WellText,
                }),
                (Role.Handle, new Look {
                    Fills = Seq<Material>(new Material.Solid(WellFill)),
                    Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFFBF"))),
                    Text = WellText,
                    StateLayers = StateLayers(WellFill, new ColorValue.Fixed(Css.White)),
                    Motion = Underdamped,
                }),
                (Role.Grid, Ruled("#FFFFFF33", "#FFFFFF66")),
                (Role.GridFine, Ruled("#FFFFFF14", "#FFFFFF33")),
                (Role.Trace, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#FFFFFF40"))), Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFFBF"))), Blend = Additive,
                }),
                (Role.Series, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Data { Alpha = Fraction.Validated(0.75f).ThrowIfFail() })),
                    Strokes = Seq(new Stroke(new ColorValue.Data())),
                    Blend = Additive,
                    When = HashMap((Condition.DifferentiateWithoutColor, new Look {
                        Strokes = Seq(new Stroke(new ColorValue.Data()) {
                            Patterns = [[PathEffect.Dash.Validated([4f, 2f, 1f, 2f]).ThrowIfFail()], [], [PathEffect.Dash.Validated([4f, 2f]).ThrowIfFail()], [PathEffect.Dash.Validated([1f, 2f]).ThrowIfFail()]],
                        }),
                    })),
                }),
                (Role.Selection, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#FFFFFF63"))), Strokes = Seq<Stroke>() }),
                (Role.Label, Look.Unpainted with { Text = WellText, Face = TextFace.Tabular })),
        })));

    // --- [MONOID]
    public StyleSheet Combine(StyleSheet rhs) => new(Looks.Union(rhs.Looks, Merge: static (_, under, over) => under.Combine(over)));

    // --- [ROWS]
    private static ColorValue.Host Panel(Channel channel, float alpha = 1f) =>
        new(channel) { Role = Role.Panel, Alpha = Fraction.Validated(alpha).ThrowIfFail(), Fallback = Some<ColorValue>(new ColorValue.Host(channel) { Role = Role.Window }) };

    private static ColorValue.Host Signal(Condition condition, Unicolour fallback) =>
        new(Channel.Text) { Role = Role.Window, Condition = condition, Fallback = Some<ColorValue>(new ColorValue.Fixed(fallback)) };

    private static Look Ruled(string resting, string enhanced) =>
        new() {
            Fills = Seq<Material>(),
            Strokes = Seq(new Stroke(new ColorValue.Fixed(resting)) { Cap = SKStrokeCap.Butt }),
            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(new ColorValue.Fixed(enhanced)) { Cap = SKStrokeCap.Butt }) })),
        };

    private static HashMap<Interaction, ColorValue> StateLayers(ColorValue from, ColorValue to) =>
        toHashMap(toSeq(Interaction.Items).Choose(interaction => interaction.Emphasis.Map(emphasis => (interaction, (ColorValue)new ColorValue.Mixed(from, to, emphasis) { Space = ColourSpace.Rgb }))));

    private static HashMap<Condition, Look> Signals(Look under) =>
        HashMap((Condition.Success, under with { Text = Success }), (Condition.Warning, under with { Text = Warning }), (Condition.Faulted, under with { Text = Faulted }));
}
