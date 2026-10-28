using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Typography;
using SkiaSharp;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Token(Role Role, Channel Channel) {
    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

public sealed record Seeds(Unicolour Background, Unicolour Foreground) {
    public Unicolour Accent { get; init; } = new("#007AFF");
    public Factor Density { get; init; } = Factor.Unit;
    public float Rounding { get; init; } = 1f;
    public Factor Pace { get; init; } = Factor.Unit;

    internal bool Dark => Background.Oklch.L < Foreground.Oklch.L;
}

[SmartEnum]
internal sealed partial class Step {
    private const double DisabledAlpha = 0.38d;
    private const double HoverShift = 0.03d;
    private const double HoverChroma = 0.93d;

    public static readonly Step Ground = Toned(0d, 0d, 1d, 1d);
    public static readonly Step Raised = Toned(-0.03d, 0.07d, 1d, 1d);
    public static readonly Step Subtle = Toned(0.02d, 0.04d, 1d, 1d);
    public static readonly Step Element = Toned(0.05d, 0.11d, 1d, 1d);
    public static readonly Step Hover = Toned(0.08d, 0.15d, 1d, 1d);
    public static readonly Step Active = Toned(0.12d, 0.2d, 1d, 1d);
    public static readonly Step Chosen = Toned(-0.03d, 0.3d, 1d, 1d);
    public static readonly Step Separator = Toned(0.15d, 0.21d, 1d, Theme.GraphicFloor);
    public static readonly Step Border = Toned(0.27d, 0.4d, Theme.GraphicFloor, Theme.TextFloor);
    public static readonly Step Secondary = Toned(0.62d, 0.77d, Theme.TextFloor, Theme.EnhancedTextFloor);
    public static readonly Step Text = Toned(1d, 1d, Theme.TextFloor, Theme.EnhancedTextFloor);
    public static readonly Step Solid = new(Theme.GraphicFloor, Theme.TextFloor, static (seeds, _, _) => seeds.Accent);
    public static readonly Step SolidHover = new(1d, 1d, static (_, _, step) => Hovered(step(Solid)));
    public static readonly Step Tint = new(1d, 1d, static (_, _, step) => Pinned(step(Active), step(Solid)));
    public static readonly Step OnSolid = new(1d, 1d, static (seeds, _, step) => seeds.Background.Contrast(step(Solid)) >= seeds.Foreground.Contrast(step(Solid)) ? seeds.Background : seeds.Foreground);
    public static readonly Step Clear = new(1d, 1d, static (_, _, step) => Faded(step(Element), 0d));
    public static readonly Step Muted = new(1d, 1d, static (seeds, _, _) => Faded(seeds.Foreground, DisabledAlpha));
    public static readonly Step Success = Signal(new Unicolour(ColourSpace.Oklch, 0.72d, 0.17d, 150d));
    public static readonly Step Warning = Signal(new Unicolour(ColourSpace.Oklch, 0.77d, 0.17d, 64d));
    public static readonly Step Faulted = Signal(new Unicolour(ColourSpace.Oklch, 0.63d, 0.24d, 29d));

    private readonly double standard;
    private readonly double increased;

    internal double Floor(bool raised) => raised ? increased : standard;

    [UseDelegateFromConstructor]
    internal partial Unicolour Derived(Seeds seeds, bool dark, Func<Step, Unicolour> step);

    private static Step Toned(double light, double dark, double standard, double increased) =>
        new(standard, increased, (seeds, shaded, _) => Shade(seeds, shaded ? dark : light));

    private static Step Signal(Unicolour hue) => new(Theme.TextFloor, Theme.EnhancedTextFloor, (_, _, _) => hue);

    private static Unicolour Shade(Seeds seeds, double at) {
        (double fromL, double fromC, double fromH) = seeds.Background.Oklch.Tuple;
        (double toL, double toC, double toH) = seeds.Foreground.Oklch.Tuple;
        return new Unicolour(ColourSpace.Oklch, double.Clamp(double.Lerp(fromL, toL, at), 0d, 1d), double.Max(0d, double.Lerp(fromC, toC, at)), double.IsNaN(fromH) ? Defined(toH) : fromH);
    }

    private static Unicolour Hovered(Unicolour solid) {
        (double lightness, double chroma, double hue) = solid.Oklch.Tuple;
        double shift = HoverShift / (lightness + 0.1d);
        return new Unicolour(ColourSpace.Oklch, lightness > 0.4d ? lightness - shift : lightness + shift, chroma * HoverChroma, Defined(hue));
    }

    private static Unicolour Pinned(Unicolour lightness, Unicolour hue) => new(ColourSpace.Oklch, lightness.Oklch.L, Defined(hue.Oklch.C), Defined(hue.Oklch.H));

    private static Unicolour Faded(Unicolour color, double alpha) => new(ColourSpace.Rgb, color.Rgb.R, color.Rgb.G, color.Rgb.B, alpha);

    private static double Defined(double channel) => double.IsNaN(channel) ? 0d : channel;
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Theme {
    // --- [THEME]
    internal const double GraphicFloor = 3d;
    internal const double TextFloor = 4.5d;
    internal const double EnhancedTextFloor = 7d;

    private static readonly Token WindowFill = new(Role.Window, Channel.Fill);
    private static readonly Token WindowText = new(Role.Window, Channel.Text);
    private static readonly Token WindowDisabled = new(Role.Window, Channel.Text) { Condition = Condition.Disabled };
    private static readonly Token AccentFill = new(Role.Accent, Channel.Fill);

    private static readonly HashMap<Token, Step> Painted = HashMap<Token, Step>(
        (WindowFill, Step.Ground), (WindowText, Step.Text), (WindowDisabled, Step.Muted),
        (new(Role.Window, Channel.Text) { Condition = Condition.Success }, Step.Success),
        (new(Role.Window, Channel.Text) { Condition = Condition.Warning }, Step.Warning),
        (new(Role.Window, Channel.Text) { Condition = Condition.Faulted }, Step.Faulted),
        (new(Role.Panel, Channel.Fill), Step.Subtle), (new(Role.Panel, Channel.Text), Step.Text),
        (new(Role.Toolbar, Channel.Fill), Step.Subtle), (new(Role.Header, Channel.Fill), Step.Subtle), (new(Role.StatusBar, Channel.Fill), Step.Subtle),
        (new(Role.StatusBar, Channel.Text), Step.Secondary), (new(Role.Gutter, Channel.Fill), Step.Subtle), (new(Role.Gutter, Channel.Text), Step.Secondary),
        (new(Role.TabList, Channel.Fill), Step.Subtle), (new(Role.Tab, Channel.Fill), Step.Clear), (new(Role.Tab, Channel.Text), Step.Secondary),
        (new(Role.Tab, Channel.Fill) { Interaction = Interaction.Hover }, Step.Element), (new(Role.Tab, Channel.Fill) { Condition = Condition.Selected }, Step.Chosen),
        (new(Role.Tab, Channel.Text) { Condition = Condition.Selected }, Step.Text),
        (new(Role.Section, Channel.Fill), Step.Element), (new(Role.Segment, Channel.Fill), Step.Element), (new(Role.Group, Channel.Fill), Step.Element),
        (new(Role.Group, Channel.Stroke), Step.Separator), (new(Role.Keycap, Channel.Fill), Step.Element),
        (new(Role.Chip, Channel.Fill), Step.Element), (new(Role.Chip, Channel.Fill) { Interaction = Interaction.Hover }, Step.Hover),
        (new(Role.Chip, Channel.Fill) { Condition = Condition.Selected }, Step.Tint),
        (new(Role.Card, Channel.Fill), Step.Raised), (new(Role.Card, Channel.Stroke), Step.Separator), (new(Role.Dialog, Channel.Fill), Step.Raised),
        (new(Role.Sheet, Channel.Fill), Step.Raised), (new(Role.Popover, Channel.Fill), Step.Raised), (new(Role.Menu, Channel.Fill), Step.Raised),
        (new(Role.Tooltip, Channel.Fill), Step.Raised), (new(Role.Hud, Channel.Fill), Step.Raised),
        (new(Role.Field, Channel.Fill), Step.Raised), (new(Role.Field, Channel.Text), Step.Text), (new(Role.Field, Channel.Stroke), Step.Separator),
        (new(Role.Button, Channel.Fill), Step.Element), (new(Role.Button, Channel.Text), Step.Text), (new(Role.Button, Channel.Stroke), Step.Separator),
        (new(Role.Button, Channel.Fill) { Interaction = Interaction.Hover }, Step.Hover), (new(Role.Button, Channel.Fill) { Interaction = Interaction.Pressed }, Step.Active),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Default }, Step.Solid), (new(Role.Button, Channel.Text) { Condition = Condition.Default }, Step.OnSolid),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Default, Interaction = Interaction.Hover }, Step.SolidHover),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Default, Interaction = Interaction.Pressed }, Step.SolidHover),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Borderless }, Step.Clear),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Borderless, Interaction = Interaction.Hover }, Step.Element),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Borderless, Interaction = Interaction.Pressed }, Step.Hover),
        (new(Role.Button, Channel.Fill) { Condition = Condition.Selected }, Step.Chosen),
        (new(Role.Toggle, Channel.Fill), Step.Raised), (new(Role.Toggle, Channel.Stroke), Step.Border),
        (new(Role.Toggle, Channel.Fill) { Condition = Condition.Selected }, Step.Solid), (new(Role.Toggle, Channel.Text) { Condition = Condition.Selected }, Step.OnSolid),
        (new(Role.Toggle, Channel.Fill) { Condition = Condition.Mixed }, Step.Solid), (new(Role.Toggle, Channel.Text) { Condition = Condition.Mixed }, Step.OnSolid),
        (new(Role.Switch, Channel.Fill), Step.Border), (new(Role.Switch, Channel.Fill) { Condition = Condition.Selected }, Step.Solid),
        (new(Role.ListRow, Channel.Fill), Step.Clear), (new(Role.ListRow, Channel.Fill) { Interaction = Interaction.Hover }, Step.Element),
        (new(Role.ListRow, Channel.Fill) { Interaction = Interaction.Pressed }, Step.Hover), (new(Role.ListRow, Channel.Fill) { Condition = Condition.Selected }, Step.Tint),
        (new(Role.ListRow, Channel.Fill) { Condition = Condition.Selected, Interaction = Interaction.Hover }, Step.Tint),
        (new(Role.Cell, Channel.Fill), Step.Clear), (new(Role.Cell, Channel.Fill) { Interaction = Interaction.Hover }, Step.Element),
        (new(Role.Cell, Channel.Fill) { Condition = Condition.Selected }, Step.Tint),
        (new(Role.Lane, Channel.Fill), Step.Clear), (new(Role.Lane, Channel.Fill) { Interaction = Interaction.Hover }, Step.Element),
        (new(Role.Lane, Channel.Fill) { Condition = Condition.Selected }, Step.Hover),
        (new(Role.Line, Channel.Fill), Step.Clear), (new(Role.Line, Channel.Fill) { Condition = Condition.Selected }, Step.Element),
        (new(Role.Selection, Channel.Fill), Step.Tint), (new(Role.Selection, Channel.Text), Step.Text),
        (new(Role.Accent, Channel.Fill), Step.Solid), (new(Role.Accent, Channel.Text), Step.OnSolid), (new(Role.Accent, Channel.Fill) { Interaction = Interaction.Hover }, Step.SolidHover),
        (new(Role.Indicator, Channel.Fill), Step.Solid), (new(Role.FocusRing, Channel.Stroke), Step.Solid),
        (new(Role.Divider, Channel.Fill), Step.Separator),
        (new(Role.Grid, Channel.Stroke), Step.Separator), (new(Role.GridFine, Channel.Stroke), Step.Active),
        (new(Role.Scrollbar, Channel.Fill), Step.Border), (new(Role.Scrollbar, Channel.Fill) { Interaction = Interaction.Hover }, Step.Secondary),
        (new(Role.Track, Channel.Fill), Step.Active));

    public HashMap<Token, SKColorF> Chrome { get; }
    internal HashSet<Condition> Conditions { get; }
    public TextFace Face { get; }
    public float FontSize { get; }
    public Configuration Configuration { get; }
    public Factor Density { get; }
    public float Rounding { get; }
    public Factor Pace { get; }

    public SKColorF Background => Chrome[WindowFill];

    public static Fin<Theme> Validated(Seeds seeds, TextFace face, float fontSize, HashMap<Token, SKColorF> stated = default, HashSet<Condition> conditions = default, Option<Configuration> configuration = default) =>
        (UIFault.Unless(face.Family.IsSome, UIReason.Missing, face.Family), UIFault.Unless(float.IsFinite(seeds.Rounding) && seeds.Rounding >= 0f, UIReason.Range, seeds.Rounding))
            .Apply((_, _) => configuration.IfNone(Configuration.Default)).As()
            .Map(gamut => Stated(seeds, stated) switch {
                var held => new Theme(
                    Derived(held, conditions.Contains(Condition.IncreaseContrast), gamut).Union(stated, Merge: static (_, _, over) => over),
                    held.Dark ? conditions.TryAdd(Condition.Dark) : conditions.Remove(Condition.Dark),
                    face, fontSize, gamut, held.Density, held.Rounding, held.Pace),
            });

    private static Seeds Stated(Seeds seeds, HashMap<Token, SKColorF> stated) =>
        seeds with {
            Background = stated.Find(WindowFill).Map(static color => color.ToUnicolour()).IfNone(seeds.Background),
            Foreground = stated.Find(WindowText).Map(static color => color.ToUnicolour()).IfNone(seeds.Foreground),
            Accent = stated.Find(AccentFill).Map(static color => color.ToUnicolour()).IfNone(seeds.Accent),
        };

    private static HashMap<Token, SKColorF> Derived(Seeds seeds, bool increased, Configuration gamut) {
        SKColorF ground = seeds.Background.Mapped(gamut);
        HashMap<Step, SKColorF> ramp = toSeq(Step.Items).Fold(HashMap<Step, SKColorF>(), (held, step) =>
            held.Add(step, Lifted((step.Derived(seeds, seeds.Dark, prior => held[prior].ToUnicolour()).Mapped(gamut), ground, step.Floor(increased), gamut))));
        return Painted.Map(step => ramp[step]);
    }

    // --- [DISABLED]
    internal float DisabledOpacity(SKColorSpace target) {
        Vector3 text = Offset(Chrome[WindowText], Chrome[WindowFill], target);
        float span = Vector3.Dot(text, text);
        return span > 0f ? float.Clamp(Vector3.Dot(text, Offset(Chrome[WindowDisabled], Chrome[WindowFill], target)) / span, 0f, 1f) : 1f;
    }

    private static Vector3 Offset(SKColorF color, SKColorF background, SKColorSpace target) => (Channels(color, target) - Channels(background, target)) * color.Alpha;

    private static Vector3 Channels(SKColorF color, SKColorSpace target) =>
        (target.GammaIsLinear ? color.ToUnicolour().RgbLinear.Triplet : color.ToUnicolour().Rgb.Triplet) switch {
            var channels => new((float)channels.First, (float)channels.Second, (float)channels.Third),
        };

    // --- [COLORS]
    private static readonly Func<(Unicolour Color, Configuration Gamut), SKColorF> Gamuted = memo<(Unicolour Color, Configuration Gamut), SKColorF>(static held => held.Color.Mapped(held.Gamut));

    internal Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<DataSeries> series = default) =>
        value.Switch(
            (Theme: this, Style: style, Series: series),
            @fixed: static (held, @fixed) => Some(Faded(Gamuted((@fixed.Color, held.Theme.Configuration)), @fixed.Alpha)),
            host: static (held, host) => held.Theme.HostColor(host, held.Style, held.Series),
            data: static (held, data) => held.Series.Bind(series => held.Theme.SeriesColor(series, held.Style)).Map(color => Faded(color, data.Alpha)),
            mixed: static (held, mixed) =>
                from start in held.Theme.Color(mixed.From, held.Style, held.Series)
                from end in held.Theme.Color(mixed.To, held.Style, held.Series)
                select start.ToUnicolour().Mix(end.ToUnicolour(), mixed.Space, (float)mixed.Amount, mixed.Hue).Mapped(held.Theme.Configuration),
            contrasting: static (held, contrasting) => held.Theme.Color(contrasting.Over, held.Style, held.Series).Bind(over =>
                contrasting.Candidates.Choose(candidate => held.Theme.Color(candidate, held.Style, held.Series))
                    .Fold(Option<SKColorF>.None, (best, next) => best.Filter(kept => Styles.Ratio(kept, over) >= Styles.Ratio(next, over)) | Some(next))),
            relative: static (held, relative) => held.Theme.Color(relative.From, held.Style, held.Series).Map(origin => relative.Shifted(origin).Mapped(held.Theme.Configuration)));

    private Option<SKColorF> HostColor(ColorValue.Host host, RoleStyle style, Option<DataSeries> series) =>
        (host.Role.Match(Some: role => Chrome.Find(new Token(role, host.Channel) { Condition = host.Condition, Interaction = host.Interaction }), None: () => RoleColor(host, style, host.Interaction))
            | (host.Role.IsNone && host.Fallback.IsNone && host.Channel.Inherited ? style.Enclosing.Bind(enclosing => Color(enclosing.Text, enclosing, series)) : None)
            | host.Fallback.Bind(fallback => Color(fallback, style, series)))
        .Map(color => Faded(color, host.Alpha));

    private Option<SKColorF> RoleColor(ColorValue.Host host, RoleStyle style, Option<Interaction> interaction) =>
        host.Condition.Match(
                Some: static condition => Seq(Some(condition)),
                None: () => toSeq(Condition.Items).Rev().Filter(style.Shown.Contains).Map(static condition => Some(condition)) + Seq(Option<Condition>.None))
            .Choose(condition => Chrome.Find(new Token(style.Role, host.Channel) { Condition = condition, Interaction = interaction })).Head;

    private static SKColorF Faded(SKColorF color, Fraction alpha) => color.WithAlpha(color.Alpha * (float)alpha);

    // --- [STATE_LAYERS]
    private const int StateLayerSteps = 16;

    private static readonly Func<(SKColorF From, SKColorF To, Configuration Gamut), Seq<SKColorF>> Ramped = memo<(SKColorF From, SKColorF To, Configuration Gamut), Seq<SKColorF>>(static ramp =>
        toSeq(ramp.From.ToUnicolour().Palette(ramp.To.ToUnicolour(), ColourSpace.Oklab, StateLayerSteps + 1)).Map(colour => colour.Mapped(ramp.Gamut)).Strict());

    internal Option<SKColorF> WithStateLayers(ColorValue value, RoleStyle style, HashMap<Interaction, float> layers, HashMap<Interaction, ColorValue> targets = default, Option<DataSeries> series = default) =>
        toSeq(Interaction.Items).Choose(interaction => layers.Find(interaction).Map(weight => (Interaction: interaction, Weight: weight))).Fold(Color(value, style, series), (rest, layer) =>
            from start in rest
            select (InteractionColor(value, style, layer.Interaction) | targets.Find(layer.Interaction).Bind(target => Color(target, style, series)))
                .Map(end => Ramped((start, end, Configuration))[(int)MathF.Round(layer.Weight * StateLayerSteps, MidpointRounding.ToEven)]).IfNone(start));

    private Option<SKColorF> InteractionColor(ColorValue value, RoleStyle style, Interaction interaction) =>
        value is ColorValue.Host { Role.IsNone: true, Interaction.IsNone: true } host ? RoleColor(host, style, interaction).Map(color => Faded(color, host.Alpha)) : None;

    // --- [SERIES]
    private static readonly Seq<Unicolour> Categorical = Seq("#0072B2", "#E69F00", "#009E73", "#CC79A7", "#56B4E9", "#D55E00", "#F0E442", "#999999").Map(static hex => new Unicolour(hex)).Strict();

    private Option<SKColorF> SeriesColor(DataSeries series, RoleStyle style) =>
        series.Color.Bind(color => Color(color, style)) | series.Value.Match(
            Some: value => double.IsFinite(value) ? style.Look.Scale.Bind(scale => scale.Valued(value)).Bind(color => Color(color, style)) | Ordinal(series, style) : None,
            None: () => Ordinal(series, style));

    private Option<SKColorF> Ordinal(DataSeries series, RoleStyle style) =>
        style.Look.Palette.Filter(static palette => !palette.IsEmpty).Bind(palette => Color(palette[(int)(series.Ordinal % (uint)palette.Count)], style))
        | Gamuted((Categorical[(int)(series.Ordinal % (uint)Categorical.Count)], Configuration));

    // --- [LEGIBILITY]
    private const double Accuracy = 1e-6;

    private static readonly Func<(SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut), SKColorF> Lifted =
        memo<(SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut), SKColorF>(Lift);

    internal double Floor(bool text) => text ? (Conditions.Contains(Condition.IncreaseContrast) ? EnhancedTextFloor : TextFloor) : GraphicFloor;

    internal SKColorF Legible(RoleStyle style, SKColorF under, SKColorF color, bool text) =>
        style.Conditions.Contains(Condition.Disabled) ? color : Lifted((color, under, Floor(text), Configuration));

    private static SKColorF Lift((SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut) held) {
        if (Styles.Ratio(held.Color, held.Ground) >= held.Ratio) return held.Color;
        (double lightness, double chroma, double hue) = held.Color.ToUnicolour().Oklch.Tuple;
        double floor = held.Ground.WithAlpha(1f).ToUnicolour().Oklch.L;
        Seq<(double From, double To)> spans = lightness >= floor ? [(lightness, 1d), (floor, 0d)] : [(lightness, 0d), (floor, 1d)];
        return spans.Find(span => Margin(span.From) < 0d && Margin(span.To) >= 0d).Bind(span => Brent.TryFindRoot(Margin, span.From, span.To, Accuracy, 100, out double shade) ? Some(At(shade)) : None)
            .IfNone(() => spans.Map(static span => span.To).Map(At).MaxBy(color => Styles.Ratio(color, held.Ground)));

        SKColorF At(double shade) => new Unicolour(ColourSpace.Oklch, shade, chroma, double.IsNaN(hue) ? 0d : hue, held.Color.Alpha).Mapped(held.Gamut);

        double Margin(double shade) => Styles.Ratio(At(shade), held.Ground) - held.Ratio - Accuracy;
    }
}

[SmartEnum]
public sealed partial class Scheme {
    public static readonly Scheme Indigo = Of(new Seeds(Css.White, new Unicolour("#282A30")) { Accent = new Unicolour("#5E6AD2") }, new Unicolour("#08090A"), new Unicolour("#F7F8F8"), TextFace.Empty);
    public static readonly Scheme Monochrome = new(
        new Seeds(Css.White, new Unicolour("#171717")) { Accent = new Unicolour("#0070F7"), Density = Dense(32f) },
        new Seeds(new Unicolour("#0A0A0A"), new Unicolour("#EDEDED")) { Accent = new Unicolour("#0071F6"), Density = Dense(32f) },
        TextFace.Empty);
    public static readonly Scheme Layered = Of(new Seeds(new Unicolour("#E9EAEE"), new Unicolour("#191A1C")) { Accent = new Unicolour("#3871E1"), Rounding = Rounded(8f) }, new Unicolour("#191A1C"), new Unicolour("#D1D3D9"), TextFace.Empty);
    public static readonly Scheme Studio = Single(new Seeds(new Unicolour("#303030"), new Unicolour("#E6E6E6")) { Accent = new Unicolour("#4772B3"), Density = Dense(20f), Rounding = Rounded(4f) });
    public static readonly Scheme Graphite = Of(
        new Seeds(new Unicolour("#EBEBEB"), new Unicolour("#282828")) { Accent = new Unicolour("#4169E1"), Density = Dense(22f), Rounding = Rounded(4f), Pace = Paced(180d) },
        new Unicolour("#2D2D2D"), new Unicolour("#DCDCDC"), new TextFace { Weight = SKFontStyleWeight.Light });

    public Seeds Light { get; }
    public Seeds Dark { get; }
    public TextFace Face { get; }

    public Fin<Theme> Over(Theme host) =>
        Theme.Validated(host.Conditions.Contains(Condition.Dark) ? Dark : Light, host.Face.Combine(Face), host.FontSize, conditions: host.Conditions, configuration: host.Configuration);

    private static Scheme Single(Seeds seeds) => new(seeds, seeds, TextFace.Empty);

    private static Scheme Of(Seeds light, Unicolour background, Unicolour foreground, TextFace face) => new(light, light with { Background = background, Foreground = foreground }, face);

    private static Factor Dense(float control) => Factor.Validated(control / StyleSheet.Control).ThrowIfFail();

    private static float Rounded(float corner) => corner / StyleSheet.Corner;

    private static Factor Paced(double milliseconds) => Factor.Validated((float)(Duration.FromMilliseconds(milliseconds) / StyleSheet.Brief)).ThrowIfFail();
}
