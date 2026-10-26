using HarfBuzzSharp;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DefaultStyles(HashMap<Appearance, HashMap<Role, Look>> Appearances, HashMap<ControlSize, HashMap<Role, HashMap<Metric, float>>> Sizes) : Monoid<DefaultStyles> {
    private const string Sweep =
        "uniform float2 size; uniform float time; layout(color) uniform half4 base; layout(color) uniform half4 sweep; half4 main(float2 p) { float head = fract(time * 0.8) * 1.6 - 0.3; half a = half(1.0 - smoothstep(0.0, 0.15, abs(p.x / max(size.x, 1.0) - head))); half4 c = mix(base, sweep, a); return half4(c.rgb * c.a, c.a); }";
    private const string Glow =
        "uniform float2 size; layout(color) uniform half4 base; layout(color) uniform half4 glow; half4 main(float2 p) { float2 uv = p / size; float halo = 1.0 - smoothstep(0.0, 1.1, distance(uv, float2(0.15, -0.2))); float grain = fract(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453) - 0.5; half3 rgb = mix(base.rgb, glow.rgb, half(halo) * glow.a) + half3(grain * 0.012); return half4(rgb * base.a, base.a); }";

    private static readonly ColorValue PanelFill = new ColorValue.Host(Role.Panel, Channel.Fill);
    private static readonly ColorValue PanelText = new ColorValue.Host(Role.Panel, Channel.Text);
    private static readonly ColorValue PanelRule = new ColorValue.Host(Role.Panel, Channel.Text) { Alpha = Fraction.Create(0.08f) };
    private static readonly ColorValue FieldFill = new ColorValue.Host(Role.Field, Channel.Fill);
    private static readonly ColorValue SelectionFill = new ColorValue.Host(Role.Selection, Channel.Fill);
    private static readonly ColorValue Accent = new ColorValue.Host(Role.Accent, Channel.Fill) { Fallback = Some<ColorValue>(new ColorValue.Fixed(new Unicolour(ColourSpace.Oklch, 0.62, 0.19, 264))) };
    private static readonly ColorValue Warning = new ColorValue.Host(Role.Window, Channel.Text) { Condition = Condition.Warning, Fallback = Some<ColorValue>(new ColorValue.Fixed(new Unicolour(ColourSpace.Oklch, 0.77, 0.17, 64))) };
    private static readonly ColorValue Faulted = new ColorValue.Host(Role.Window, Channel.Text) { Condition = Condition.Faulted, Fallback = Some<ColorValue>(new ColorValue.Fixed(new Unicolour(ColourSpace.Oklch, 0.63, 0.24, 29))) };
    private static readonly ColorValue WellText = new ColorValue.Fixed("#FFFFFFB3");
    private static readonly TimingCurve Eased = new TimingCurve.Eased(Duration.FromMilliseconds(120), Easing.EaseOut);
    private static readonly Seq<Material> Unfilled = [];
    private static readonly Seq<Stroke> Unstroked = [];
    private static readonly TextFace SemiBold = new() { Weight = SKFontStyleWeight.SemiBold };

    public static DefaultStyles Empty { get; } = new([], []);

    public static DefaultStyles Base { get; } = new(
        HashMap(
            (Appearance.Native, HashMap(
                (Role.Button, new Look {
                    When = HashMap(
                        (Condition.Default, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(None, Channel.Fill) { Condition = Condition.Default, Fallback = Some(Accent) })),
                            Text = new ColorValue.Host(None, Channel.Text) {
                                Condition = Condition.Default,
                                Fallback = Some<ColorValue>(new ColorValue.Contrasting(Accent, [new ColorValue.Host(Role.Window, Channel.Text), new ColorValue.Host(Role.Window, Channel.Fill)])),
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
                        (Condition.Warning, new Look { Fills = Unfilled, Strokes = Unstroked, Text = Warning }),
                        (Condition.Faulted, new Look { Fills = Unfilled, Strokes = Unstroked, Text = Faulted })),
                }),
                (Role.Track, new Look {
                    Fills = Unfilled,
                    Strokes = Seq(new Stroke(new ColorValue.Host(Role.Panel, Channel.Text) { Alpha = Fraction.Create(0.2f) })),
                    Text = PanelText,
                    When = HashMap((Condition.Busy, new Look {
                        Fills = Seq<Material>(new Material.Drawn(SKRuntimeEffect.CreateShader(Sweep, out string _), HashMap(
                            ("base", new ColorValue.Host(Role.Panel, Channel.Text) { Alpha = Fraction.Create(0.12f) }),
                            ("sweep", SelectionFill)))),
                    })),
                }),
                (Role.Span, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Role.Toolbar, Channel.Fill))), Strokes = Unstroked, Text = PanelText }),
                (Role.Ring, new Look { Fills = Unfilled, Strokes = Seq(new Stroke(PanelText)), Text = PanelText }),
                (Role.Needle, new Look { Fills = Unfilled, Strokes = Seq(new Stroke(PanelText)), Text = PanelText }),
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
                    Washes = HashMap(
                        (Interaction.Hover, (ColorValue)new ColorValue.Mixed(FieldFill, PanelText, Fraction.Create(0.16f)) { Space = ColourSpace.Rgb }),
                        (Interaction.Pressed, new ColorValue.Mixed(FieldFill, PanelText, Fraction.Create(0.24f)) { Space = ColourSpace.Rgb })),
                    Motion = Eased,
                    When = HashMap((Condition.Modified, new Look { Fills = Seq<Material>(new Material.Solid(SelectionFill)) })),
                }),
                (Role.FocusRing, new Look {
                    Fills = Unfilled,
                    Strokes = Unstroked,
                    Shadows = Seq<Shadow>(new Shadow.Drop { Color = new ColorValue.Host(Role.FocusRing, Channel.Stroke), Blur = 0f, Spread = 3f }),
                    Motion = Eased,
                }),
                (Role.OverlayCaption, new Look {
                    Fills = Unfilled,
                    Strokes = Unstroked,
                    Text = new ColorValue.Fixed("#FFFFFFFF"),
                    Shadows = Seq<Shadow>(new Shadow.Drop { Color = new ColorValue.Fixed("#00000080"), Blur = 0f, X = 1f, Y = 1f }),
                }),
                (Role.Series, new Look {
                    Palette = Seq<ColorValue>(
                        new ColorValue.Fixed("#0072B2"), new ColorValue.Fixed("#E69F00"), new ColorValue.Fixed("#009E73"), new ColorValue.Fixed("#CC79A7"),
                        new ColorValue.Fixed("#56B4E9"), new ColorValue.Fixed("#D55E00"), new ColorValue.Fixed("#F0E442"), new ColorValue.Fixed("#999999")),
                }),
                (Role.Well, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#000000FF"))),
                    Strokes = Seq(new Stroke(new ColorValue.Fixed("#00000080"))),
                    Text = WellText,
                    Within = HashMap(
                        (Role.Field, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#292929FF"))), Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))), Text = WellText,
                        }),
                        (Role.Handle, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#000000FF"))),
                            Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFFBF"))),
                            Text = WellText,
                            Washes = HashMap(
                                (Interaction.Hover, (ColorValue)new ColorValue.Mixed(new ColorValue.Fixed("#000000FF"), new ColorValue.Fixed("#FFFFFF"), Fraction.Create(0.16f)) { Space = ColourSpace.Rgb }),
                                (Interaction.Pressed, new ColorValue.Mixed(new ColorValue.Fixed("#000000FF"), new ColorValue.Fixed("#FFFFFF"), Fraction.Create(0.24f)) { Space = ColourSpace.Rgb })),
                            Motion = Eased,
                        }),
                        (Role.Grid, new Look {
                            Fills = Unfilled,
                            Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))),
                            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF66"))) })),
                        }),
                        (Role.GridFine, new Look {
                            Fills = Unfilled,
                            Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF14"))),
                            When = HashMap((Condition.IncreaseContrast, new Look { Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFF33"))) })),
                        }),
                        (Role.Trace, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#FFFFFF40"))), Strokes = Seq(new Stroke(new ColorValue.Fixed("#FFFFFFBF"))), Blend = SKBlendMode.Plus,
                        }),
                        (Role.Series, new Look {
                            Fills = Seq<Material>(new Material.Solid(new ColorValue.Data { Alpha = Fraction.Create(0.75f) })),
                            Strokes = Seq(new Stroke(new ColorValue.Data())),
                            Blend = SKBlendMode.Plus,
                            When = HashMap((Condition.DifferentiateWithoutColor, new Look { Strokes = Seq(new Stroke(new ColorValue.Data(), [[4f, 2f, 1f, 2f], [], [4f, 2f], [1f, 2f]])) })),
                        }),
                        (Role.Selection, new Look { Fills = Seq<Material>(new Material.Solid(new ColorValue.Fixed("#FFFFFF63"))), Strokes = Unstroked }),
                        (Role.Label, new Look { Fills = Unfilled, Strokes = Unstroked, Text = WellText, Face = new TextFace { Features = Seq(new Feature(Tag.Parse("tnum"))) } })),
                }))),
            (Appearance.Glass, HashMap(
                (Role.Panel, new Look { Fills = Seq<Material>(new Material.Backdrop(Clear: false, PanelFill)), Strokes = Unstroked }),
                (Role.Toolbar, new Look { Fills = Seq<Material>(new Material.Backdrop(Clear: false, PanelFill)), Strokes = Unstroked }))),
            (Appearance.Drawn, HashMap(
                (Role.Panel, new Look {
                    Fills = Seq<Material>(new Material.Drawn(SKRuntimeEffect.CreateShader(Glow, out string _), HashMap(
                        ("base", PanelFill),
                        ("glow", new ColorValue.Host(Role.FocusRing, Channel.Text) { Alpha = Fraction.Create(0.35f) })))),
                    Strokes = Seq(new Stroke(PanelRule)),
                }),
                (Role.Toolbar, new Look {
                    Fills = Seq<Material>(new Material.Gradient([new Stop<ColorValue>(new ColorValue.Host(Role.Toolbar, Channel.Fill), Fraction.MinValue), new Stop<ColorValue>(PanelFill, Fraction.MaxValue)], 90f)),
                    Strokes = Seq(new Stroke(PanelRule)),
                    Shadows = Seq<Shadow>(new Shadow.Drop { Color = new ColorValue.Fixed("#00000040"), Blur = 1f, Y = 0.5f }),
                }))),
            (Appearance.Borderless, HashMap(
                (Role.Button, new Look { Strokes = Unstroked }),
                (Role.Field, new Look { Strokes = Unstroked })))),
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
}
