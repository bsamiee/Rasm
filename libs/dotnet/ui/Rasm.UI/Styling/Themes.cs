using System.Numerics;
using MathNet.Numerics.RootFinding;
using Rasm.UI.Media;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Token(Role Role, Channel Channel) {
    public static readonly Token Canvas = new(Role.Window, Channel.Fill);
    public static readonly Token CanvasText = new(Role.Window, Channel.Text);
    public static readonly Token AccentColor = new(Role.Accent, Channel.Fill);
    public static readonly Token AccentColorText = new(Role.Accent, Channel.Text);
    public static readonly Token Highlight = new(Role.Selection, Channel.Fill);
    public static readonly Token HighlightText = new(Role.Selection, Channel.Text);
    public static readonly Token ButtonFace = new(Role.Button, Channel.Fill);
    public static readonly Token ButtonText = new(Role.Button, Channel.Text);
    public static readonly Token ButtonBorder = new(Role.Button, Channel.Stroke);
    public static readonly Token Field = new(Role.Field, Channel.Fill);
    public static readonly Token FieldText = new(Role.Field, Channel.Text);
    internal static readonly Token GrayText = new(Role.Window, Channel.Text) { Condition = Styling.Condition.Disabled };

    internal static readonly HashMap<string, Token> SystemColors = HashMap(
        (nameof(Canvas), Canvas), ("Window", Canvas), (nameof(CanvasText), CanvasText), ("WindowText", CanvasText), (nameof(GrayText), GrayText),
        (nameof(AccentColor), AccentColor), ("LinkText", AccentColor), (nameof(AccentColorText), AccentColorText),
        (nameof(Highlight), Highlight), ("SelectedItem", Highlight), (nameof(HighlightText), HighlightText), ("SelectedItemText", HighlightText),
        (nameof(ButtonFace), ButtonFace), (nameof(ButtonText), ButtonText), (nameof(ButtonBorder), ButtonBorder), (nameof(Field), Field), (nameof(FieldText), FieldText));

    public Option<Condition> Condition { get; init; }
    public Option<Interaction> Interaction { get; init; }
}

public sealed record Seeds(Unicolour Background, Unicolour Foreground) {
    public Unicolour Accent { get; init; } = new("#007AFF");
    public float Control { get; init; } = StyleSheet.Control;
    public Option<float> Corner { get; init; }

    internal bool Dark => Background.Oklch.L < Foreground.Oklch.L;
}

[SmartEnum]
internal sealed partial class Tone {
    private const double GraphicFloor = 3d;
    private const double TextFloor = 4.5d;
    private const double EnhancedFloor = 7d;
    private const double DisabledAlpha = 0.38d;
    private const double HoverShift = 0.03d;
    private const double HoverChroma = 0.93d;

    public static readonly Tone Ground = Toned(0d, 0d, 1d, 1d);
    public static readonly Tone Raised = Toned(-0.03d, 0.07d, 1d, 1d);
    public static readonly Tone Subtle = Toned(0.02d, 0.04d, 1d, 1d);
    public static readonly Tone Element = Toned(0.05d, 0.11d, 1d, 1d);
    public static readonly Tone Hover = Toned(0.08d, 0.15d, 1d, 1d);
    public static readonly Tone Active = Toned(0.12d, 0.2d, 1d, 1d);
    public static readonly Tone Chosen = Toned(-0.03d, 0.3d, 1d, 1d);
    public static readonly Tone Separator = Toned(0.15d, 0.21d, 1d, GraphicFloor);
    public static readonly Tone Border = Toned(0.27d, 0.4d, GraphicFloor, TextFloor);
    public static readonly Tone Secondary = Toned(0.62d, 0.77d, TextFloor, EnhancedFloor);
    public static readonly Tone Text = Toned(1d, 1d, TextFloor, EnhancedFloor);
    public static readonly Tone Solid = new(GraphicFloor, TextFloor, static (seeds, _) => seeds.Accent);
    public static readonly Tone SolidHover = new(1d, 1d, static (_, tone) => Hovered(tone(Solid)));
    public static readonly Tone Tint = new(1d, 1d, static (_, tone) => Pinned(tone(Active), tone(Solid)));
    public static readonly Tone OnSolid = new(1d, 1d, static (seeds, tone) => seeds.Background.Contrast(tone(Solid)) >= seeds.Foreground.Contrast(tone(Solid)) ? seeds.Background : seeds.Foreground);
    public static readonly Tone Clear = new(1d, 1d, static (_, tone) => Faded(tone(Element), 0d));
    public static readonly Tone Muted = new(1d, 1d, static (seeds, _) => Faded(seeds.Foreground, DisabledAlpha));
    public static readonly Tone Success = Signal(new Unicolour(ColourSpace.Oklch, 0.72d, 0.17d, 150d));
    public static readonly Tone Warning = Signal(new Unicolour(ColourSpace.Oklch, 0.77d, 0.17d, 64d));
    public static readonly Tone Faulted = Signal(new Unicolour(ColourSpace.Oklch, 0.63d, 0.24d, 29d));

    private readonly double standard;
    private readonly double increased;

    internal double Floor(bool raised) => raised ? increased : standard;

    [UseDelegateFromConstructor]
    internal partial Unicolour Derived(Seeds seeds, Func<Tone, Unicolour> tone);

    private static Tone Toned(double light, double dark, double standard, double increased) =>
        new(standard, increased, (seeds, _) => seeds.Background.Mix(seeds.Foreground, ColourSpace.Oklab, seeds.Dark ? dark : light, premultiplyAlpha: false));

    private static Tone Signal(Unicolour hue) => new(TextFloor, EnhancedFloor, (_, _) => hue);

    private static Unicolour Hovered(Unicolour solid) {
        (double lightness, double chroma, double hue) = solid.Oklch.Tuple;
        double shift = HoverShift / (lightness + 0.1d);
        return new Unicolour(ColourSpace.Oklch, lightness > 0.4d ? lightness - shift : lightness + shift, chroma * HoverChroma, hue);
    }

    private static Unicolour Pinned(Unicolour lightness, Unicolour hue) => new(ColourSpace.Oklch, lightness.Oklch.L, hue.Oklch.C, hue.Oklch.H);

    private static Unicolour Faded(Unicolour color, double alpha) => new(ColourSpace.Rgb, color.Rgb.R, color.Rgb.G, color.Rgb.B, alpha);
}

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Theme {
    // --- [THEME]
    private static readonly HashMap<Token, Tone> Painted = toHashMap(Seq<(Tone Tone, Seq<Token> Tokens)>(
        (Tone.Ground, [Token.Canvas]),
        (Tone.Raised, Token.Field.Cons(Rows(Channel.Fill, [Role.Card, Role.Dialog, Role.Sheet, Role.Popover, Role.Menu, Role.Tooltip, Role.Hud, Role.Toggle]))),
        (Tone.Subtle, Rows(Channel.Fill, [Role.Panel, Role.Toolbar, Role.Header, Role.StatusBar, Role.Gutter, Role.TabList])),
        (Tone.Element, Token.ButtonFace.Cons(Rows(Channel.Fill, [Role.Section, Role.Segment, Role.Group, Role.Keycap, Role.Chip]))
            + Rows(Channel.Fill, [Role.Tab, Role.ListRow, Role.Cell, Role.Lane], interactions: [Interaction.Hover])
            + Rows(Channel.Fill, [Role.Button], [Condition.Borderless], [Interaction.Hover]) + Rows(Channel.Fill, [Role.Line], [Condition.Selected])),
        (Tone.Hover, Rows(Channel.Fill, [Role.Chip, Role.Button], interactions: [Interaction.Hover]) + Rows(Channel.Fill, [Role.ListRow], interactions: [Interaction.Pressed])
            + Rows(Channel.Fill, [Role.Button], [Condition.Borderless], [Interaction.Pressed]) + Rows(Channel.Fill, [Role.Lane], [Condition.Selected])),
        (Tone.Active, Rows(Channel.Fill, [Role.Track]) + Rows(Channel.Stroke, [Role.GridFine]) + Rows(Channel.Fill, [Role.Button], interactions: [Interaction.Pressed])),
        (Tone.Chosen, Rows(Channel.Fill, [Role.Tab, Role.Button], [Condition.Selected])),
        (Tone.Separator, Token.ButtonBorder.Cons(Rows(Channel.Stroke, [Role.Group, Role.Card, Role.Field, Role.Grid, Role.Panel, Role.Toolbar])) + Rows(Channel.Fill, [Role.Divider])),
        (Tone.Border, Rows(Channel.Fill, [Role.Switch, Role.Scrollbar]) + Rows(Channel.Stroke, [Role.Toggle, Role.Track])),
        (Tone.Secondary, Rows(Channel.Text, [Role.StatusBar, Role.Gutter, Role.Tab]) + Rows(Channel.Fill, [Role.Scrollbar], interactions: [Interaction.Hover])),
        (Tone.Text, [Token.CanvasText, Token.FieldText, Token.ButtonText, Token.HighlightText, .. Rows(Channel.Text, [Role.Panel]), .. Rows(Channel.Text, [Role.Tab], [Condition.Selected])]),
        (Tone.Solid, Token.AccentColor.Cons(Rows(Channel.Fill, [Role.Indicator]) + Rows(Channel.Stroke, [Role.FocusRing]) + Rows(Channel.Fill, [Role.Button], [Condition.Default])
            + Rows(Channel.Fill, [Role.Toggle, Role.Switch], [Condition.Selected]) + Rows(Channel.Fill, [Role.Toggle], [Condition.Mixed]))),
        (Tone.SolidHover, Rows(Channel.Fill, [Role.Button], [Condition.Default], [Interaction.Hover, Interaction.Pressed]) + Rows(Channel.Fill, [Role.Accent], interactions: [Interaction.Hover])),
        (Tone.Tint, Token.Highlight.Cons(Rows(Channel.Fill, [Role.Chip, Role.ListRow, Role.Cell], [Condition.Selected])) + Rows(Channel.Fill, [Role.ListRow], [Condition.Selected], [Interaction.Hover])),
        (Tone.OnSolid, Token.AccentColorText.Cons(Rows(Channel.Text, [Role.Button], [Condition.Default]) + Rows(Channel.Text, [Role.Toggle], [Condition.Selected, Condition.Mixed]))),
        (Tone.Clear, Rows(Channel.Fill, [Role.Tab, Role.ListRow, Role.Cell, Role.Lane, Role.Line]) + Rows(Channel.Fill, [Role.Button], [Condition.Borderless])),
        (Tone.Muted, [Token.GrayText]),
        (Tone.Success, Rows(Channel.Text, [Role.Window], [Condition.Success])),
        (Tone.Warning, Rows(Channel.Text, [Role.Window], [Condition.Warning])),
        (Tone.Faulted, Rows(Channel.Text, [Role.Window], [Condition.Faulted])))
        .SelectMany(static row => row.Tokens, static (row, token) => (token, row.Tone)));

    public HashMap<Token, SKColorF> Chrome { get; }
    internal HashSet<Condition> Conditions { get; }
    public Seeds Seeds { get; }
    public TextFace Face { get; }
    public float FontSize { get; }
    public Configuration Configuration { get; }

    public SKColorF Background => Chrome[Token.Canvas];

    internal float Density => Seeds.Control / StyleSheet.Control * Typed;

    internal float Rounding => Seeds.Corner.Map(corner => corner / StyleSheet.Corner * Typed).IfNone(Density);

    private float Typed => float.Max(1f, FontSize / StyleSheet.Text);

    public static Fin<Theme> Validated(Seeds seeds, TextFace face, float fontSize, HashMap<Token, SKColorF> stated = default, HashSet<Condition> conditions = default, Option<Configuration> configuration = default) =>
        (UIFault.Unless(face.Family.IsSome, UIReason.Missing, face.Family),
         UIFault.Unless(float.IsFinite(fontSize) && fontSize > 0f && float.IsFinite(seeds.Control) && seeds.Control > 0f && seeds.Corner.ForAll(static corner => float.IsFinite(corner) && corner >= 0f),
             UIReason.Range, (fontSize, seeds.Control, seeds.Corner)))
            .Apply((_, _) => Stated(seeds, stated) switch {
                var held => configuration.IfNone(Configuration.Default) switch {
                    var gamut => new Theme(
                        Derived(held, conditions.Contains(Condition.IncreaseContrast), gamut).Union(stated, Merge: static (_, _, over) => over),
                        held.Dark ? conditions.TryAdd(Condition.Dark) : conditions.Remove(Condition.Dark),
                        held, face, fontSize, gamut),
                },
            }).As();

    private static Seeds Stated(Seeds seeds, HashMap<Token, SKColorF> stated) =>
        seeds with {
            Background = stated.Find(Token.Canvas).Map(static color => color.ToUnicolour()).IfNone(seeds.Background),
            Foreground = stated.Find(Token.CanvasText).Map(static color => color.ToUnicolour()).IfNone(seeds.Foreground),
            Accent = stated.Find(Token.AccentColor).Map(static color => color.ToUnicolour()).IfNone(seeds.Accent),
        };

    private static HashMap<Token, SKColorF> Derived(Seeds seeds, bool increased, Configuration gamut) {
        SKColorF ground = seeds.Background.Mapped(gamut);
        HashMap<Tone, SKColorF> ramp = toSeq(Tone.Items).Fold(HashMap<Tone, SKColorF>(), (held, tone) =>
            held.Add(tone, Lifted((tone.Derived(seeds, prior => held[prior].ToUnicolour()).Mapped(gamut), ground, tone.Floor(increased), gamut))));
        return Painted.Map(tone => ramp[tone]);
    }

    private static Seq<Token> Rows(Channel channel, Seq<Role> roles, Seq<Option<Condition>> conditions = default, Seq<Option<Interaction>> interactions = default) =>
        toSeq(from role in roles
              from condition in conditions.DefaultIfEmpty()
              from interaction in interactions.DefaultIfEmpty()
              select new Token(role, channel) { Condition = condition, Interaction = interaction });

    // --- [DISABLED]
    internal float DisabledOpacity(SKColorSpace target) {
        Vector3 text = Offset(Chrome[Token.CanvasText], Chrome[Token.Canvas], target);
        float span = Vector3.Dot(text, text);
        return span > 0f ? float.Clamp(Vector3.Dot(text, Offset(Chrome[Token.GrayText], Chrome[Token.Canvas], target)) / span, 0f, 1f) : 1f;
    }

    private static Vector3 Offset(SKColorF color, SKColorF background, SKColorSpace target) => (Channels(color, target) - Channels(background, target)) * color.Alpha;

    private static Vector3 Channels(SKColorF color, SKColorSpace target) =>
        (target.GammaIsLinear ? color.ToUnicolour().RgbLinear.Triplet : color.ToUnicolour().Rgb.Triplet) switch {
            var channels => new((float)channels.First, (float)channels.Second, (float)channels.Third),
        };

    // --- [COLORS]
    private static readonly Func<(Unicolour Color, Configuration Gamut), SKColorF> Gamuted = memo<(Unicolour Color, Configuration Gamut), SKColorF>(static held => held.Color.Mapped(held.Gamut));

    internal Option<SKColorF> Color(ColorValue value, RoleStyle style, Option<DataSeries> series = default) =>
        (value.Switch(
                (Theme: this, Style: style, Series: series),
                @fixed: static (held, @fixed) => Some(Gamuted((@fixed.Color, held.Theme.Configuration))),
                host: static (held, host) => held.Theme.Chrome.Find(host.Token),
                current: static (held, current) => held.Theme.RoleColor(current, held.Style, current.Interaction)
                    | (current.Channel.Inherited ? held.Style.Enclosing.Bind(enclosing => held.Theme.Color(enclosing.Text, enclosing, held.Series)) : None),
                data: static (held, _) => held.Series.Bind(series => held.Theme.SeriesColor(series, held.Style)),
                mixed: static (held, mixed) =>
                    from start in held.Theme.Color(mixed.From, held.Style, held.Series)
                    from end in held.Theme.Color(mixed.To, held.Style, held.Series)
                    select start.ToUnicolour().Mix(end.ToUnicolour(), mixed.Space, (float)mixed.Amount, mixed.Hue).Mapped(held.Theme.Configuration),
                contrasting: static (held, contrasting) => held.Theme.Color(contrasting.Over, held.Style, held.Series).Bind(over =>
                    toSeq(contrasting.Candidates.Choose(candidate => held.Theme.Color(candidate, held.Style, held.Series)).MaxItemsBy(color => color.Contrast(over))).Head),
                relative: static (held, relative) => held.Theme.Color(relative.From, held.Style, held.Series).Map(origin => relative.Shifted(origin).Mapped(held.Theme.Configuration)))
            | value.Fallback.Bind(fallback => Color(fallback, style, series)))
        .Map(color => Faded(color, value.Alpha));

    private Option<SKColorF> RoleColor(ColorValue.Current current, RoleStyle style, Option<Interaction> interaction) =>
        current.Condition.Match(
                Some: static condition => Seq(Some(condition)),
                None: () => toSeq(Condition.Items).Rev().Filter(style.Shown.Contains).Map(static condition => Some(condition)) + Seq(Option<Condition>.None))
            .Choose(condition => Chrome.Find(new Token(style.Role, current.Channel) { Condition = condition, Interaction = interaction })).Head;

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
        value is ColorValue.Current { Interaction.IsNone: true } current ? RoleColor(current, style, interaction).Map(color => Faded(color, current.Alpha)) : None;

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

    internal double Floor(bool text) => (text ? Tone.Text : Tone.Border).Floor(Conditions.Contains(Condition.IncreaseContrast));

    internal SKColorF Legible(RoleStyle style, SKColorF under, SKColorF color, bool text) =>
        style.Conditions.Contains(Condition.Disabled) ? color : Lifted((color, under, Floor(text), Configuration));

    private static SKColorF Lift((SKColorF Color, SKColorF Ground, double Ratio, Configuration Gamut) held) {
        if (held.Color.Contrast(held.Ground) >= held.Ratio) return held.Color;
        (double lightness, double chroma, double hue) = held.Color.ToUnicolour().Oklch.Tuple;
        double floor = held.Ground.WithAlpha(1f).ToUnicolour().Oklch.L;
        Seq<(double From, double To)> spans = lightness >= floor ? [(lightness, 1d), (floor, 0d)] : [(lightness, 0d), (floor, 1d)];
        return spans.Find(span => Margin(span.From) < 0d && Margin(span.To) >= 0d).Bind(span => Brent.TryFindRoot(Margin, span.From, span.To, Accuracy, 100, out double shade) ? Some(At(shade)) : None)
            .IfNone(() => spans.Map(static span => span.To).Map(At).MaxBy(color => color.Contrast(held.Ground)));

        SKColorF At(double shade) => new Unicolour(ColourSpace.Oklch, shade, chroma, hue, held.Color.Alpha).Mapped(held.Gamut);

        double Margin(double shade) => At(shade).Contrast(held.Ground) - held.Ratio - Accuracy;
    }
}

public sealed record Scheme {
    public static readonly Scheme Glass = new() { Conditions = [Condition.Glass] };
    public static readonly Scheme Neutral =
        new Seeds(new Unicolour("#EBEBEB"), new Unicolour("#282828")) { Accent = Css.RoyalBlue, Control = 20f, Corner = 4f } switch {
            var light => new Scheme {
                Light = light,
                Dark = light with { Background = new Unicolour("#2D2D2D"), Foreground = Css.Gainsboro },
                Face = new TextFace { Weight = SKFontStyleWeight.Light },
                Conditions = [Condition.Neutral],
            },
        };

    public Option<Seeds> Light { get; init; }
    public Option<Seeds> Dark { get; init; }
    public TextFace Face { get; init; } = TextFace.Empty;
    public HashSet<Condition> Conditions { get; init; }

    public Fin<Theme> Over(Theme host) =>
        (host.Conditions.Contains(Condition.Dark) ? Dark : Light).Match(Some: static seeds => (seeds, HashMap<Token, SKColorF>()), None: () => (host.Seeds, host.Chrome)) switch {
            var (seeds, stated) => Theme.Validated(seeds, host.Face.Combine(Face), host.FontSize, stated, host.Conditions.Union(Conditions), host.Configuration),
        };
}
