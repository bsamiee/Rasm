using SkiaSharp;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DisplayOptions(bool ReduceMotion, bool IncreaseContrast, bool DifferentiateWithoutColor, bool ReduceTransparency, bool InvertColors);

public sealed record Theme(Chrome Chrome, Metrics Metrics, string FontFamily, float FontSize, DisplayOptions Display) {
    public SKColorF Color(Family family, Ink ink) =>
        ink.Switch(
            (Theme: this, Family: family),
            themed: static (held, themed) => held.Family.Palette.Swatch(themed.Role).Resolve(held.Theme.Chrome) switch {
                var color => color.WithAlpha(color.Alpha * themed.Alpha),
            },
            @fixed: static (_, @fixed) => @fixed.Color);

    public Option<float> Veil(Family family) =>
        (Color(family, new Ink.Themed(ColorRole.Text, 1f)), Color(family, new Ink.Themed(ColorRole.Disabled, 1f)), Color(family, new Ink.Themed(ColorRole.Ground, 1f))) switch {
            var (text, disabled, ground) => Offsets(text, ground).Zip(Offsets(disabled, ground))
                .Fold((Moved: 0f, Span: 0f), static (sum, pair) => (sum.Moved + (pair.First * (pair.First - pair.Second)), sum.Span + (pair.First * pair.First)))
                is { Span: > 0f } total
                ? Some(total.Moved / total.Span).Filter(static veil => veil is >= 0f and <= 1f)
                : None,
        };

    public static Theme Interpolate(Theme from, Theme to, float amount) => to with { Chrome = Chrome.Interpolate(from.Chrome, to.Chrome, amount) };

    private static Seq<float> Offsets(SKColorF ink, SKColorF ground) =>
        Seq(ink.Red - ground.Red, ink.Green - ground.Green, ink.Blue - ground.Blue).Map(offset => ink.Alpha * offset);
}
