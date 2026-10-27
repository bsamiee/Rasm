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

[SmartEnum]
public sealed partial class Condition {
    public static readonly Condition IncreaseContrast = new(inherited: false, identity);
    public static readonly Condition DifferentiateWithoutColor = new(inherited: false, identity);
    public static readonly Condition ReduceTransparency = new(inherited: false,
        static look => look with { Fills = look.Fills.Map(static fills => fills.Map(static fill => fill.Opaqued)) });
    public static readonly Condition ReduceMotion = new(inherited: false, static look => look with { Motion = Some<TimingCurve>(new TimingCurve.Instant()) });
    public static readonly Condition Dark = new(inherited: false, identity);
    public static readonly Condition Default = new(inherited: false, identity);
    public static readonly Condition Destructive = new(inherited: false, identity);
    public static readonly Condition Selected = new(inherited: false, identity);
    public static readonly Condition Modified = new(inherited: false, identity);
    public static readonly Condition Mixed = new(inherited: false, identity);
    public static readonly Condition Busy = new(inherited: false, identity);
    public static readonly Condition Warning = new(inherited: false, identity);
    public static readonly Condition Faulted = new(inherited: false, identity);
    public static readonly Condition Disabled = new(inherited: true, identity);

    internal bool Inherited { get; }

    [UseDelegateFromConstructor]
    internal partial Look Restyled(Look look);
}

[SmartEnum]
public sealed partial class ContrastLevel {
    public static readonly ContrastLevel Minimum = new(text: 4.5, graphic: 3.0);
    public static readonly ContrastLevel Enhanced = new(text: 7.0, graphic: 3.0);

    public double Text { get; }
    public double Graphic { get; }
}

[SmartEnum]
public sealed partial class Interaction {
    public static readonly Interaction Hover = new(ring: false);
    public static readonly Interaction Pressed = new(ring: false);
    public static readonly Interaction Focused = new(ring: true);

    public bool Ring { get; }
}

[SmartEnum]
public sealed partial class Channel {
    public static readonly Channel Fill = new(inherited: false);
    public static readonly Channel Stroke = new(inherited: false);
    public static readonly Channel Text = new(inherited: true);

    internal bool Inherited { get; }
}

[SmartEnum]
public sealed partial class Metric {
    public static readonly Metric Extent = new(0f);
    public static readonly Metric Reach = new(0f);
    public static readonly Metric Inset = new(0f);
    public static readonly Metric Gap = new(0f);
    public static readonly Metric Radius = new(0f);
    public static readonly Metric Smoothing = new(0f);
    public static readonly Metric Stroke = new(1f);
    public static readonly Metric Icon = new(16f);
    public static readonly Metric FontSize = new(1f);
    public static readonly Metric Spacing = new(32f);
    public static readonly Metric Slop = new(3f);
    public static readonly Metric Fine = new(0.1f);
    public static readonly Metric Notch = new(1.2f);
    public static readonly Metric EdgeFade = new(0f);

    public float Default { get; }
}

public sealed record DefaultStyles(HashMap<Appearance, HashMap<Role, Look>> Appearances, HashMap<ControlSize, HashMap<Role, HashMap<Metric, float>>> Sizes) : Monoid<DefaultStyles> {
    private static readonly SKRuntimeShaderBuilder Sweep = SKRuntimeEffect.BuildShader(
        "uniform float2 size; uniform float time; layout(color) uniform half4 base; layout(color) uniform half4 sweep; half4 main(float2 p) { float head = fract(time * 0.8) * 1.6 - 0.3; half a = half(1.0 - smoothstep(0.0, 0.15, abs(p.x / max(size.x, 1.0) - head))); half4 c = mix(base, sweep, a); return half4(c.rgb * c.a, c.a); }");
    private static readonly SKRuntimeShaderBuilder Glow = SKRuntimeEffect.BuildShader(
        "uniform float2 size; layout(color) uniform half4 base; layout(color) uniform half4 glow; half4 main(float2 p) { float2 uv = p / size; float halo = 1.0 - smoothstep(0.0, 1.1, distance(uv, float2(0.15, -0.2))); float grain = fract(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453) - 0.5; half3 rgb = mix(base.rgb, glow.rgb, half(halo) * glow.a) + half3(grain * 0.012); return half4(rgb * base.a, base.a); }");

    private static readonly ColorValue PanelFill = new ColorValue.Host(Channel.Fill) { Role = Role.Panel };
    private static readonly ColorValue PanelText = new ColorValue.Host(Channel.Text) { Role = Role.Panel };
    private static readonly ColorValue PanelRule = new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Validated(0.08f).ThrowIfFail() };
    private static readonly ColorValue FieldFill = new ColorValue.Host(Channel.Fill) { Role = Role.Field };
    private static readonly ColorValue SelectionFill = new ColorValue.Host(Channel.Fill) { Role = Role.Selection };
    private static readonly ColorValue Accent = new ColorValue.Host(Channel.Fill) { Role = Role.Accent, Fallback = Some<ColorValue>(new ColorValue.Fixed(new Unicolour(ColourSpace.Oklch, 0.62, 0.19, 264))) };
    private static readonly ColorValue Warning = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Warning };
    private static readonly ColorValue Faulted = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Faulted };
    private static readonly ColorValue WellText = new ColorValue.Fixed("#FFFFFFB3");
    private static readonly ColorValue WellFill = new ColorValue.Fixed("#000000FF");
    private static readonly TimingCurve Eased = new TimingCurve.Eased(Duration.FromMilliseconds(120), Easing.EaseOut);
    private static readonly TextFace SemiBold = new() { Weight = SKFontStyleWeight.SemiBold };
    private static readonly Look Outlined = new() { Fills = Seq<Material>(), Strokes = Seq(new Stroke(PanelText)), Text = PanelText };
    private static readonly Look Frosted = new() { Fills = Seq<Material>(new Material.Backdrop(Clear: false, PanelFill)), Strokes = Seq<Stroke>() };
    private static readonly Look Unstroked = new() { Strokes = Seq<Stroke>() };

    public static DefaultStyles Empty { get; } = new([], []);

    public static DefaultStyles Base { get; } = new(
        HashMap(
            (Appearance.Native, HashMap(
                (Role.Button, new Look {
                    When = HashMap(
                        (Condition.Default, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Condition = Condition.Default, Fallback = Some(Accent) })),
                            Text = new ColorValue.Host(Channel.Text) {
                                Condition = Condition.Default,
                                Fallback = Some<ColorValue>(new ColorValue.Contrasting(Accent, [new ColorValue.Host(Channel.Text) { Role = Role.Window }, new ColorValue.Host(Channel.Fill) { Role = Role.Window }])),
                            },
                        }),
                        (Condition.Destructive, new Look { Text = Faulted })),
                }),
                (Role.Field, new Look {
                    When = HashMap(
                        (Condition.Modified, new Look { Text = SelectionFill, Face = SemiBold }),
                        (Condition.Warning, new Look { Text = Warning }),
                        (Condition.Faulted, new Look { Text = Faulted })),
                }),
                (Role.Label, new Look {
                    When = HashMap(
                        (Condition.Modified, new Look { Text = SelectionFill, Face = SemiBold }),
                        (Condition.Warning, Look.Bare with { Text = Warning }),
                        (Condition.Faulted, Look.Bare with { Text = Faulted })),
                }),
                (Role.Track, new Look {
                    Fills = Seq<Material>(),
                    Strokes = Seq(new Stroke(new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Validated(0.2f).ThrowIfFail() })),
                    Text = PanelText,
                    When = HashMap((Condition.Busy, new Look {
                        Fills = Seq<Material>(new Material.Drawn(Sweep.Effect, HashMap(
                            ("base", new ColorValue.Host(Channel.Text) { Role = Role.Panel, Alpha = Fraction.Validated(0.12f).ThrowIfFail() }),
                            ("sweep", SelectionFill)))),
                    })),
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
                    Washes = Washed(FieldFill, PanelText),
                    Motion = Eased,
                    When = HashMap((Condition.Modified, new Look { Fills = Seq<Material>(new Material.Solid(SelectionFill)) })),
                }),
                (Role.FocusRing, Look.Bare with { Shadows = Seq(new Shadow(new ColorValue.Host(Channel.Stroke) { Role = Role.FocusRing }, Blur: 0f) { Spread = 3f }), Motion = Eased }),
                (Role.OverlayCaption, Look.Bare with {
                    Text = new ColorValue.Fixed("#FFFFFFFF"),
                    Shadows = Seq(new Shadow(new ColorValue.Fixed("#00000080"), Blur: 0f) { X = 1f, Y = 1f }),
                }),
                (Role.Series, new Look {
                    Palette = Seq<ColorValue>(
                        new ColorValue.Fixed("#0072B2"), new ColorValue.Fixed("#E69F00"), new ColorValue.Fixed("#009E73"), new ColorValue.Fixed("#CC79A7"),
                        new ColorValue.Fixed("#56B4E9"), new ColorValue.Fixed("#D55E00"), new ColorValue.Fixed("#F0E442"), new ColorValue.Fixed("#999999")),
                }),
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
                            Washes = Washed(WellFill, new ColorValue.Fixed("#FFFFFF")),
                            Motion = Eased,
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
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#FFFFFF40"))), Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFFBF"))), Blend = SKBlendMode.Plus,
                        }),
                        (Role.Series, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Data { Alpha = Fraction.Validated(0.75f).ThrowIfFail() })),
                            Strokes = Seq(new Stroke(new ColorValue.Data())),
                            Blend = SKBlendMode.Plus,
                            When = HashMap((Condition.DifferentiateWithoutColor, new Look { Strokes = Seq(new Stroke(new ColorValue.Data(), [[4f, 2f, 1f, 2f], [], [4f, 2f], [1f, 2f]])) })),
                        }),
                        (Role.Selection, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#FFFFFF63"))), Strokes = Seq<Stroke>() }),
                        (Role.Label, Look.Bare with { Text = WellText, Face = TextFace.Tabular })),
                }))),
            (Appearance.Glass, HashMap(
                (Role.Panel, Frosted),
                (Role.Toolbar, Frosted))),
            (Appearance.Drawn, HashMap(
                (Role.Panel, new Look {
                    Fills = Seq<Material>(new Material.Drawn(Glow.Effect, HashMap(
                        ("base", PanelFill),
                        ("glow", new ColorValue.Host(Channel.Text) { Role = Role.FocusRing, Alpha = Fraction.Validated(0.35f).ThrowIfFail() })))),
                    Strokes = Seq(new Stroke(PanelRule)),
                }),
                (Role.Toolbar, new Look {
                    Fills = Seq<Material>(new Material.Gradient([new Stop<ColorValue>(new ColorValue.Host(Channel.Fill) { Role = Role.Toolbar }, Fraction.MinValue), new Stop<ColorValue>(PanelFill, Fraction.MaxValue)], 90f)),
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
                (Role.Label, HashMap((Metric.FontSize, 0.73f))))),
            (ControlSize.Compact, HashMap(
                (Role.Panel, HashMap((Metric.Inset, 4f), (Metric.Gap, 4f))),
                (Role.Toolbar, HashMap((Metric.Extent, 22f))),
                (Role.Button, HashMap((Metric.Extent, 18f), (Metric.Inset, 3f))),
                (Role.Field, HashMap((Metric.Extent, 18f), (Metric.Inset, 3f)))))));

    public DefaultStyles Combine(DefaultStyles rhs) =>
        new(Appearances.Union(rhs.Appearances, Merge: static (_, under, over) => under.Union(over, Merge: static (_, below, above) => below.Combine(above))),
            Sizes.Union(rhs.Sizes, Merge: static (_, under, over) => under.Union(over, Merge: static (_, below, above) => below.Union(above, Merge: static (_, _, held) => held))));

    private static HashMap<Interaction, ColorValue> Washed(ColorValue from, ColorValue to) =>
        HashMap(
            (Interaction.Hover, (ColorValue)new ColorValue.Mixed(from, to, Fraction.Validated(0.16f).ThrowIfFail()) { Space = ColourSpace.Rgb }),
            (Interaction.Pressed, new ColorValue.Mixed(from, to, Fraction.Validated(0.24f).ThrowIfFail()) { Space = ColourSpace.Rgb }));
}
