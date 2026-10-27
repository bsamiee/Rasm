using Rasm.UI;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Styling;

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class PanelRoles {
    public static readonly Role ListRow = Role.Create("list-row");
    public static readonly Role Readout = Role.Create("readout");
    public static readonly Role Rail = Role.Create("rail");
    public static readonly Role Popover = Role.Create("popover");
    public static readonly Role Toast = Role.Create("toast");
    public static readonly Role HudCapsule = Role.Create("hud-capsule");
    public static readonly Role SectionHeader = Role.Create("section-header");
    public static readonly Role Tab = Role.Create("tab");

    private static readonly ColorValue PanelFill = new ColorValue.Host(Channel.Fill) { Role = Role.Panel };
    private static readonly ColorValue PanelText = new ColorValue.Host(Channel.Text) { Role = Role.Panel };
    private static readonly ColorValue Umbra = new ColorValue.Fixed("#00000059");
    private static readonly TextFace SemiBold = new() { Weight = SKFontStyleWeight.SemiBold };
    private static readonly TimingCurve Shown = new TimingCurve.Eased(Duration.FromMilliseconds(200), Easing.EaseOut);

    private static readonly Look Raised = new() {
        Fills = Seq<Material>(new Material.Solid(PanelFill)), Strokes = Seq<Stroke>(), Text = PanelText, Shadows = Seq(new Shadow(Umbra, Blur: 3f) { Y = 1f }),
    };

    private static readonly Look Frosted = new() { Fills = Seq<Material>(new Material.Backdrop(Clear: false, PanelFill)), Strokes = Seq<Stroke>(), Text = PanelText, Shadows = Seq<Shadow>() };

    public static DefaultStyles Defaults { get; } = DefaultStyles.Base.Combine(new DefaultStyles(
        HashMap(
            (Appearance.Native, HashMap(
                (ListRow, new Look { When = HashMap((Condition.Modified, new Look { Text = new ColorValue.Host(Channel.Fill) { Role = Role.Selection }, Face = SemiBold })) }),
                (Readout, new Look {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Role = Role.Panel, Alpha = Fraction.Create(0.85f) })),
                    Strokes = Seq<Stroke>(),
                    Text = PanelText,
                    Face = TextFace.Tabular,
                    When = HashMap(
                        (Condition.Warning, new Look { Text = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Warning } }),
                        (Condition.Faulted, new Look { Text = new ColorValue.Host(Channel.Text) { Role = Role.Window, Condition = Condition.Faulted } })),
                }),
                (Rail, Raised),
                (Popover, Raised),
                (Toast, Raised with { Shadows = Seq(new Shadow(Umbra, Blur: 6f) { Y = 2f }), Motion = Shown }),
                (HudCapsule, Raised with {
                    Fills = Seq<Material>(new Material.Solid(new ColorValue.Host(Channel.Fill) { Role = Role.Panel, Alpha = Fraction.Create(0.9f) })),
                    Shadows = Seq(new Shadow(Umbra, Blur: 1f) { Y = 0.5f }),
                }),
                (SectionHeader, Look.Bare with { Text = PanelText, Face = SemiBold }))),
            (Appearance.Glass, HashMap(
                (Rail, Frosted),
                (Popover, Frosted),
                (Toast, Frosted with { Motion = Shown }),
                (HudCapsule, Frosted with { Fills = Seq<Material>(new Material.Backdrop(Clear: true, PanelFill)) }))),
            (Appearance.Borderless, HashMap((Tab, new Look { Strokes = Seq<Stroke>() })))),
        HashMap(
            (ControlSize.Regular, HashMap(
                (ListRow, HashMap((Metric.Extent, 20f), (Metric.Inset, 4f))),
                (Readout, HashMap((Metric.Inset, 4f), (Metric.Radius, 4f))),
                (Rail, HashMap((Metric.Inset, 4f), (Metric.Gap, 4f), (Metric.Radius, 10f))),
                (Popover, HashMap((Metric.Inset, 8f), (Metric.Gap, 4f), (Metric.Radius, 10f))),
                (Toast, HashMap((Metric.Inset, 8f), (Metric.Gap, 6f), (Metric.Radius, 10f))),
                (HudCapsule, HashMap((Metric.Extent, 24f), (Metric.Inset, 6f), (Metric.Gap, 4f), (Metric.Radius, 12f))),
                (SectionHeader, HashMap((Metric.Extent, 24f), (Metric.Inset, 4f))),
                (Tab, HashMap((Metric.Extent, 22f), (Metric.Inset, 4f), (Metric.Radius, 5f))))),
            (ControlSize.Compact, HashMap(
                (ListRow, HashMap((Metric.Extent, 17f))),
                (SectionHeader, HashMap((Metric.Extent, 20f))),
                (Tab, HashMap((Metric.Extent, 18f), (Metric.Inset, 3f))))))));
}
