using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Role(string Key) {
    public static readonly Role Window = new("window");
    public static readonly Role Accent = new("accent");
    public static readonly Role Panel = new("panel");
    public static readonly Role Toolbar = new("toolbar");
    public static readonly Role Button = new("button");
    public static readonly Role Field = new("field");
    public static readonly Role Divider = new("divider");
    public static readonly Role Scrollbar = new("scrollbar");
    public static readonly Role Label = new("label");
    public static readonly Role Track = new("track");
    public static readonly Role Span = new("span");
    public static readonly Role Handle = new("handle");
    public static readonly Role Grid = new("grid");
    public static readonly Role Selection = new("selection");
    public static readonly Role FocusRing = new("focus-ring");
    public static readonly Role Well = new("well");
    public static readonly Role Series = new("series");
    public static readonly Role Trace = new("trace");
    public static readonly Role GridFine = new("grid-fine");
    public static readonly Role OverlayCaption = new("overlay-caption");
    public static readonly Role Needle = new("needle");
    public static readonly Role Stop = new("stop");
    public static readonly Role Ring = new("ring");
    public static readonly Role Title = new("title");
    public static readonly Role Headline = new("headline");
    public static readonly Role Caption = new("caption");
    public static readonly Role Code = new("code");
    public static readonly Role Tick = new("tick");
    public static readonly Role Crosshair = new("crosshair");
    public static readonly Role Readout = new("readout");
    public static readonly Role Legend = new("legend");
    public static readonly Role ListRow = new("list-row");
    public static readonly Role Spinner = new("spinner");
    public static readonly Role Badge = new("badge");
    public static readonly Role Mark = new("mark");
    public static readonly Role Keycap = new("keycap");
    public static readonly Role Veil = new("veil");
    public static readonly Role Popover = new("popover");
    public static readonly Role Tooltip = new("tooltip");
    public static readonly Role Menu = new("menu");
}

public sealed record Appearance(string Key) {
    public static readonly Appearance Native = new("native");
    public static readonly Appearance Glass = new("glass");
    public static readonly Appearance Drawn = new("drawn");
    public static readonly Appearance Borderless = new("borderless");
}

public sealed record ControlSize(string Key) {
    public static readonly ControlSize Regular = new("regular");
    public static readonly ControlSize Compact = new("compact");
}

public sealed record DefaultStyles(HashMap<Appearance, HashMap<Role, Look>> Appearances, HashMap<ControlSize, HashMap<Role, HashMap<Metric, float>>> Sizes) : Monoid<DefaultStyles> {
    // --- [COLORS]
    internal static readonly ColorValue Accent = new ColorValue.Host(Channel.Fill) { Role = Role.Accent, Fallback = Some<ColorValue>(new ColorValue.Fixed(new Unicolour(ColourSpace.Oklch, 0.62, 0.19, 264))) };

    private static readonly ColorValue PanelFill = new ColorValue.Host(Channel.Fill) { Role = Role.Panel };
    private static readonly ColorValue PanelTint = new ColorValue.Host(Channel.Fill) { Role = Role.Panel, Alpha = Fraction.Validated(0.6f).ThrowIfFail() };
    private static readonly ColorValue PanelText = new ColorValue.Host(Channel.Text) { Role = Role.Panel };
    private static readonly ColorValue PanelRule = new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Validated(0.08f).ThrowIfFail() };
    private static readonly ColorValue FieldFill = new ColorValue.Host(Channel.Fill) { Role = Role.Field };
    private static readonly ColorValue SelectionFill = new ColorValue.Host(Channel.Fill) { Role = Role.Selection };
    private static readonly ColorValue Secondary = new ColorValue.Host(Channel.Text) { Alpha = Fraction.Validated(0.6f).ThrowIfFail() };
    private static readonly ColorValue AccentText = new ColorValue.Contrasting(Accent, [new ColorValue.Host(Channel.Text) { Role = Role.Window }, new ColorValue.Host(Channel.Fill) { Role = Role.Window }]);
    private static readonly ColorValue Success = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Success };
    private static readonly ColorValue Warning = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Warning };
    private static readonly ColorValue Faulted = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Faulted };
    private static readonly ColorValue Umbra = new ColorValue.Fixed("#00000059");
    private static readonly ColorValue WellText = new ColorValue.Fixed("#FFFFFFB3");
    private static readonly ColorValue WellFill = new ColorValue.Fixed("#000000FF");
    private static readonly SKBlender Additive = SKBlender.CreateBlendMode(SKBlendMode.Plus);

    // --- [MOTION]
    private static readonly Factor Rest = Factor.Validated(0.001f).ThrowIfFail();
    private static readonly TimingCurve Eased = new TimingCurve.Eased(Duration.FromMilliseconds(120), Easing.EaseOut);
    private static readonly TimingCurve Critical = new TimingCurve.Spring(Duration.FromMilliseconds(500), Factor.Unit, Rest);
    private static readonly TimingCurve Underdamped = new TimingCurve.Spring(Duration.FromMilliseconds(500), Factor.Validated(0.85f).ThrowIfFail(), Rest);

    // --- [FACES]
    private static readonly TextFace SemiBold = new() { Weight = SKFontStyleWeight.SemiBold };
    private static readonly TextFace Mono = new() { Mono = true };

    // --- [MATERIALS]
    private static readonly Material Shimmer = Material.Drawn.Validated(
        "uniform float2 size; uniform float time; layout(color) uniform half4 base; layout(color) uniform half4 sweep; half4 main(float2 p) { float head = fract(time * 0.8) * 1.6 - 0.3; half a = half(1.0 - smoothstep(0.0, 0.15, abs(p.x / max(size.x, 1.0) - head))); half4 c = mix(base, sweep, a); return half4(c.rgb * c.a, c.a); }",
        colors: HashMap(("base", new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Validated(0.12f).ThrowIfFail() }), ("sweep", SelectionFill))).ThrowIfFail();
    private static readonly Material Glow = Material.Drawn.Validated(
        "uniform float2 size; layout(color) uniform half4 base; layout(color) uniform half4 glow; uniform shader grain; half4 main(float2 p) { float2 uv = p / size; float halo = 1.0 - smoothstep(0.0, 1.1, distance(uv, float2(0.15, -0.2))); float noise = float(grain.eval(p).r) - 0.5; half3 rgb = mix(base.rgb, glow.rgb, half(halo) * glow.a) + half3(noise * 0.012); return half4(rgb * base.a, base.a); }",
        colors: HashMap(("base", PanelFill), ("glow", new ColorValue.Host(Channel.Text) { Role = Role.FocusRing, Alpha = Fraction.Validated(0.35f).ThrowIfFail() })),
        children: HashMap(("grain", (Child)new Child.Noise(Frequency: 0.9f, Octaves: 2)))).ThrowIfFail();
    private static readonly Material Spin = Material.Drawn.Validated(
        "uniform float2 size; uniform float time; layout(color) uniform half4 tint; half4 main(float2 p) { float2 c = p - size * 0.5; float r = min(size.x, size.y) * 0.5; half ring = half(1.0 - smoothstep(r * 0.12, r * 0.12 + 1.0, abs(length(c) - r * 0.72))); half tail = half(fract(atan(c.y, c.x) * 0.15915494 - time)); half a = tint.a * ring * tail; return half4(tint.rgb * a, a); }",
        colors: HashMap(("tint", (ColorValue)new ColorValue.Host(Channel.Text)))).ThrowIfFail();
    private static readonly Material Frost = new Material.Glass(Clear: false, [Filter.Css.Validated(FilterFunction.Blur, 20f).ThrowIfFail(), Filter.Css.Validated(FilterFunction.Saturate, 1.8f).ThrowIfFail()]);

    // --- [LOOKS]
    private static readonly Look Outlined = new() { Fills = Seq<Material>(), Strokes = Seq(new Stroke(PanelText)), Text = PanelText };
    private static readonly Look Frosted = new() { Fills = Seq(Frost, new Material.Solid(PanelTint)), Strokes = Seq<Stroke>() };
    private static readonly Look Unstroked = new() { Strokes = Seq<Stroke>() };
    private static readonly Look Surfaced = Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(PanelFill)), Text = PanelText, Shadows = Seq(new Shadow(Umbra, Blur: 3f) { Y = 1f }), Motion = Critical };
    private static readonly Look Quiet = Look.Unpainted with { Text = Secondary };
    private static readonly HashMap<Condition, Look> Edits = HashMap(
        (Condition.Modified, new Look { Text = SelectionFill, Face = SemiBold }),
        (Condition.Pending, new Look { Face = new TextFace { Italic = true } }));

    public static DefaultStyles Empty { get; } = new([], []);

    public static DefaultStyles Base { get; } = new(
        HashMap(
            (Appearance.Native, HashMap(
                (Role.Button, new Look {
                    When = HashMap(
                        (Condition.Default, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Condition = Condition.Default, Fallback = Some(Accent) })),
                            Text = new ColorValue.Host(Channel.Text) { Condition = Condition.Default, Fallback = Some(AccentText) },
                        }),
                        (Condition.Destructive, new Look { Text = Faulted })),
                }),
                (Role.Field, new Look { When = Edits + Signals(Look.Empty) }),
                (Role.Label, new Look { When = Edits + Signals(Look.Unpainted) }),
                (Role.Track, new Look {
                    Fills = Seq<Material>(),
                    Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Validated(0.2f).ThrowIfFail() })),
                    Text = PanelText,
                    When = HashMap((Condition.Busy, new Look { Fills = Seq(Shimmer) })),
                }),
                (Role.Span, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Role = Role.Toolbar })), Strokes = Seq<Stroke>(), Text = PanelText }),
                (Role.Ring, Outlined),
                (Role.Needle, Outlined),
                (Role.Stop, new Look {
                    Fills = Seq<Material>(new Material.Solid(PanelFill)),
                    Strokes = Seq(new Stroke(PanelText)),
                    Text = PanelText,
                    When = HashMap((Condition.Selected, new Look { Fills = Seq<Material>(new Material.Solid(PanelText)), Text = PanelFill })),
                }),
                (Role.Handle, new Look {
                    Fills = Seq<Material>(new Material.Solid(FieldFill)),
                    Strokes = Seq(new Stroke(PanelText)),
                    Text = PanelText,
                    StateLayers = StateLayers(FieldFill, PanelText),
                    Motion = Underdamped,
                    When = HashMap((Condition.Modified, new Look { Fills = Seq<Material>(new Material.Solid(SelectionFill)) })),
                }),
                (Role.FocusRing, Look.Unpainted with { Shadows = Seq(new Shadow(new ColorValue.Host(Channel.Stroke) { Role = Role.FocusRing }, Blur: 0f) { Spread = 3f }), Motion = Eased }),
                (Role.OverlayCaption, Look.Unpainted with {
                    Text = new ColorValue.Fixed("#FFFFFFFF"),
                    Shadows = Seq(new Shadow(new ColorValue.Fixed("#00000080"), Blur: 0f) { X = 1f, Y = 1f }),
                }),
                (Role.Series, new Look {
                    Palette = Seq<ColorValue>(
                        new ColorValue.Fixed("#0072B2"), new ColorValue.Fixed("#E69F00"), new ColorValue.Fixed("#009E73"), new ColorValue.Fixed("#CC79A7"),
                        new ColorValue.Fixed("#56B4E9"), new ColorValue.Fixed("#D55E00"), new ColorValue.Fixed("#F0E442"), new ColorValue.Fixed("#999999")),
                }),
                (Role.Title, Look.Unpainted),
                (Role.Headline, Look.Unpainted with { Face = SemiBold }),
                (Role.Caption, Quiet),
                (Role.Code, Look.Unpainted with { Face = Mono }),
                (Role.Tick, Quiet with { Face = TextFace.Tabular }),
                (Role.Keycap, Quiet with { Face = TextFace.Tabular }),
                (Role.Legend, Look.Unpainted),
                (Role.Crosshair, Look.Unpainted with { Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Half })) }),
                (Role.Readout, Look.Unpainted with {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Role = Role.Panel, Alpha = Fraction.Validated(0.85f).ThrowIfFail() })),
                    Text = PanelText,
                    Face = TextFace.Tabular,
                    When = Signals(Look.Empty),
                }),
                (Role.ListRow, new Look { Motion = Underdamped, When = Edits + Signals(Look.Empty) }),
                (Role.Spinner, Look.Unpainted with { Strokes = Seq(new Stroke(Accent)), When = HashMap((Condition.Busy, Look.Unpainted with { Fills = Seq(Spin) })) }),
                (Role.Badge, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(Accent)), Text = AccentText, Face = SemiBold.Combine(TextFace.Tabular) }),
                (Role.Mark, Look.Unpainted with { Face = new TextFace { Background = Some<ColorValue>(new ColorValue.Fixed("#FFD60A66")) } }),
                (Role.Veil, Look.Unpainted with { Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#0000004D"))), Motion = Critical }),
                (Role.Popover, Surfaced),
                (Role.Tooltip, Surfaced),
                (Role.Menu, Surfaced),
                (Role.Well, new Look {
                    Fills = Seq<Material>(new Material.Solid(WellFill)),
                    Strokes = Seq(new Stroke(new ColorValue.Fixed("#00000080"))),
                    Text = WellText,
                    Within = HashMap(
                        (Role.Field, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#292929FF"))), Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))), Text = WellText,
                        }),
                        (Role.Handle, new Look {
                            Fills = Seq<Material>(new Material.Solid(WellFill)),
                            Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFFBF"))),
                            Text = WellText,
                            StateLayers = StateLayers(WellFill, new ColorValue.Fixed("#FFFFFF")),
                            Motion = Underdamped,
                        }),
                        (Role.Grid, new Look {
                            Fills = Seq<Material>(),
                            Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))),
                            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF66"))) })),
                        }),
                        (Role.GridFine, new Look {
                            Fills = Seq<Material>(),
                            Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF14"))),
                            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))) })),
                        }),
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
                }))),
            (Appearance.Glass, HashMap(
                (Role.Panel, Frosted),
                (Role.Toolbar, Frosted),
                (Role.Popover, Frosted),
                (Role.Tooltip, Frosted),
                (Role.Menu, Frosted))),
            (Appearance.Drawn, HashMap(
                (Role.Panel, new Look { Fills = Seq(Glow), Strokes = Seq(new Stroke(PanelRule)) }),
                (Role.Toolbar, new Look {
                    Fills = Seq<Material>(new Material.Gradient(
                        [new Stop<ColorValue>(new ColorValue.Host(Channel.Fill) { Role = Role.Toolbar }, Fraction.MinValue), new Stop<ColorValue>(PanelFill, Fraction.MaxValue)], new GradientShape.Linear(90f))),
                    Strokes = Seq(new Stroke(PanelRule)),
                    Shadows = Seq(new Shadow(new ColorValue.Fixed("#00000040"), Blur: 1f) { Y = 0.5f }),
                }))),
            (Appearance.Borderless, HashMap(
                (Role.Button, Unstroked),
                (Role.Field, Unstroked)))),
        HashMap(
            (ControlSize.Regular, HashMap(
                (Role.Panel, HashMap((Metric.Inset, 8f), (Metric.Gap, 6f))),
                (Role.Toolbar, HashMap((Metric.Extent, 28f), (Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, 6f))),
                (Role.Button, HashMap((Metric.Extent, 22f), (Metric.Inset, 4f), (Metric.Radius, 5f))),
                (Role.Field, HashMap((Metric.Extent, 22f), (Metric.Inset, 4f), (Metric.Radius, 4f), (Metric.Slop, 0f))),
                (Role.Divider, HashMap((Metric.Extent, 1f))),
                (Role.Scrollbar, HashMap((Metric.Extent, 4f), (Metric.Radius, 2f))),
                (Role.Handle, HashMap((Metric.Extent, 8f), (Metric.Reach, 6f))),
                (Role.Needle, HashMap((Metric.Stroke, 2f), (Metric.Reach, 6f))),
                (Role.Stop, HashMap((Metric.Extent, 10f), (Metric.Reach, 6f))),
                (Role.Ring, HashMap((Metric.Extent, 12f), (Metric.Reach, 6f))),
                (Role.Track, HashMap((Metric.Extent, 4f), (Metric.Radius, 2f))),
                (Role.Span, HashMap((Metric.Reach, 4f), (Metric.Radius, 2f))),
                (Role.Well, HashMap((Metric.Extent, 18f), (Metric.Reach, 6f), (Metric.Inset, 7.2f), (Metric.Gap, 4f), (Metric.Radius, 3f))),
                (Role.Trace, HashMap((Metric.Stroke, 1.5f))),
                (Role.Series, HashMap((Metric.Stroke, 1.5f))),
                (Role.Label, HashMap((Metric.FontSize, 0.73f))),
                (Role.Title, HashMap((Metric.FontSize, 1.69f))),
                (Role.Headline, HashMap((Metric.FontSize, 1f))),
                (Role.Caption, HashMap((Metric.FontSize, 0.77f))),
                (Role.Tick, HashMap((Metric.FontSize, 0.77f), (Metric.Gap, 4f))),
                (Role.Legend, HashMap((Metric.Extent, 10f))),
                (Role.Crosshair, HashMap((Metric.Stroke, 1f))),
                (Role.Readout, HashMap((Metric.Inset, 4f), (Metric.Radius, 4f))),
                (Role.ListRow, HashMap((Metric.Extent, 20f), (Metric.Inset, 4f))),
                (Role.Spinner, HashMap((Metric.Extent, 16f), (Metric.Radius, 8f), (Metric.Stroke, 2f))),
                (Role.Badge, HashMap((Metric.Extent, 8f), (Metric.Inset, 3f), (Metric.Radius, 8f), (Metric.FontSize, 0.77f))),
                (Role.Popover, HashMap((Metric.Inset, 8f), (Metric.Gap, 4f), (Metric.Radius, 10f))),
                (Role.Tooltip, HashMap((Metric.Inset, 4f), (Metric.Gap, 2f), (Metric.Radius, 5f), (Metric.FontSize, 0.85f))),
                (Role.Menu, HashMap((Metric.Inset, 4f), (Metric.Radius, 6f))))),
            (ControlSize.Compact, HashMap(
                (Role.Panel, HashMap((Metric.Inset, 4f), (Metric.Gap, 4f))),
                (Role.Toolbar, HashMap((Metric.Extent, 22f))),
                (Role.Button, HashMap((Metric.Extent, 18f), (Metric.Inset, 3f))),
                (Role.Field, HashMap((Metric.Extent, 18f), (Metric.Inset, 3f))),
                (Role.ListRow, HashMap((Metric.Extent, 17f))),
                (Role.Spinner, HashMap((Metric.Extent, 12f), (Metric.Radius, 6f)))))));

    // --- [MONOID]
    public DefaultStyles Combine(DefaultStyles rhs) =>
        new(Appearances.Union(rhs.Appearances, Merge: static (_, under, over) => under.Union(over, Merge: static (_, below, above) => below.Combine(above))),
            Sizes.Union(rhs.Sizes, Merge: static (_, under, over) => under.Union(over, Merge: static (_, below, above) => below.Union(above, Merge: static (_, _, held) => held))));

    // --- [ROWS]
    private static HashMap<Interaction, ColorValue> StateLayers(ColorValue from, ColorValue to) =>
        HashMap(
            (Interaction.Hover, (ColorValue)new ColorValue.Mixed(from, to, Fraction.Validated(0.16f).ThrowIfFail()) { Space = ColourSpace.Rgb }),
            (Interaction.Pressed, new ColorValue.Mixed(from, to, Fraction.Validated(0.24f).ThrowIfFail()) { Space = ColourSpace.Rgb }));

    private static HashMap<Condition, Look> Signals(Look under) =>
        HashMap((Condition.Success, under with { Text = Success }), (Condition.Warning, under with { Text = Warning }), (Condition.Faulted, under with { Text = Faulted }));
}
