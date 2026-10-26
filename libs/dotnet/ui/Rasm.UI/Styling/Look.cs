using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarfBuzzSharp;
using SkiaSharp;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Glass {
    public static readonly Glass Regular = new("regular");
    public static readonly Glass Clear = new("clear");
}

public sealed record CssChannel(string Keyword, double Scale, Option<double> Percent) {
    internal static readonly CssChannel Alpha = new("alpha", 1d, Some(1d));

    private const string Missing = "none";

    private static readonly Seq<(string Unit, double Degrees)> Angles = [("deg", 1d), ("grad", 0.9d), ("rad", 180d / double.Pi), ("turn", 360d)];

    internal Option<double> Read(string token) =>
        string.Equals(token, Missing, StringComparison.Ordinal) ? Some(0d)
        : token.EndsWith('%') ? from reference in Percent from value in Invariant.Number<double>(token.AsSpan()[..^1]) select value * reference / 100d
        : Percent.IsSome ? Invariant.Number<double>(token)
        : Degrees(token) | Invariant.Number<double>(token);

    internal static Option<double> Degrees(string token) =>
        from angle in Angles.Find(held => token.EndsWith(held.Unit, StringComparison.Ordinal))
        from value in Invariant.Number<double>(token.AsSpan()[..^angle.Unit.Length])
        select value * angle.Degrees;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CssSpace {
    public static readonly CssSpace Srgb = new("srgb", ColourSpace.Rgb, function: false, [new("r", 1d, Some(1d)), new("g", 1d, Some(1d)), new("b", 1d, Some(1d))]);
    public static readonly CssSpace SrgbLinear = new("srgb-linear", ColourSpace.RgbLinear, function: false, [new("r", 1d, Some(1d)), new("g", 1d, Some(1d)), new("b", 1d, Some(1d))]);
    public static readonly CssSpace Xyz = new("xyz", ColourSpace.Xyz, function: false, [new("x", 1d, Some(1d)), new("y", 1d, Some(1d)), new("z", 1d, Some(1d))]);
    public static readonly CssSpace Rgb = new("rgb", ColourSpace.Rgb, function: true, [new("r", 255d, Some(255d)), new("g", 255d, Some(255d)), new("b", 255d, Some(255d))]);
    public static readonly CssSpace Hsl = new("hsl", ColourSpace.Hsl, function: true, [new("h", 1d, None), new("s", 100d, Some(100d)), new("l", 100d, Some(100d))]);
    public static readonly CssSpace Hwb = new("hwb", ColourSpace.Hwb, function: true, [new("h", 1d, None), new("w", 100d, Some(100d)), new("b", 100d, Some(100d))]);
    public static readonly CssSpace Lab = new("lab", ColourSpace.Lab, function: true, [new("l", 1d, Some(100d)), new("a", 1d, Some(125d)), new("b", 1d, Some(125d))]);
    public static readonly CssSpace Lch = new("lch", ColourSpace.Lchab, function: true, [new("l", 1d, Some(100d)), new("c", 1d, Some(150d)), new("h", 1d, None)]);
    public static readonly CssSpace Oklab = new("oklab", ColourSpace.Oklab, function: true, [new("l", 1d, Some(1d)), new("a", 1d, Some(0.4d)), new("b", 1d, Some(0.4d))]);
    public static readonly CssSpace Oklch = new("oklch", ColourSpace.Oklch, function: true, [new("l", 1d, Some(1d)), new("c", 1d, Some(0.4d)), new("h", 1d, None)]);

    public ColourSpace Space { get; }
    public bool Function { get; }
    public Seq<CssChannel> Channels { get; }

    internal Unicolour Colour(Seq<double> channels, double alpha) =>
        new(Space, channels[0] / Channels[0].Scale, channels[1] / Channels[1].Scale, channels[2] / Channels[2].Scale, alpha);

    internal Seq<double> Channeled(Unicolour colour) =>
        colour.GetRepresentation(Space).Tuple switch {
            var (first, second, third) => Seq(first, second, third).Zip(Channels).Map(static held => double.IsNaN(held.First) ? 0d : held.First * held.Second.Scale),
        };

    internal string Written(Option<Ink> from, string channels) =>
        (Function, from.Map(static origin => $"from {origin.ToValue()} ").IfNone("")) switch {
            (true, var origin) => $"{Key}({origin}{channels})",
            (false, var origin) => $"color({origin}{Key} {channels})",
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Component {
    public sealed record Kept : Component;

    public sealed record Literal(double Value) : Component;

    public sealed record Offset(double By) : Component;

    public sealed record Scaled(double By) : Component;

    internal double Applied(double kept) =>
        Switch(kept, kept: static (held, _) => held, literal: static (_, literal) => literal.Value, offset: static (held, offset) => held + offset.By, scaled: static (held, scaled) => held * scaled.By);

    internal string Written(CssChannel channel) =>
        Switch(
            channel.Keyword,
            kept: static (keyword, _) => keyword,
            literal: static (_, literal) => Ink.Written(literal.Value),
            offset: static (keyword, offset) => offset.By >= 0d
                ? $"calc({keyword} {CalcOperator.Plus.Key} {Ink.Written(offset.By)})"
                : $"calc({keyword} {CalcOperator.Minus.Key} {Ink.Written(-offset.By)})",
            scaled: static (keyword, scaled) => $"calc({keyword} {CalcOperator.Times.Key} {Ink.Written(scaled.By)})");

    internal static Option<Component> Read(CssChannel channel, string token) =>
        string.Equals(token, channel.Keyword, StringComparison.Ordinal)
            ? Some<Component>(new Kept())
            : Ink.Called(token).Filter(static call => call.Name is "calc").Match(
                Some: calc => Calculated(channel, Ink.Split(calc.Body, char.IsWhiteSpace)),
                None: () => channel.Read(token).Map(static value => (Component)new Literal(value)));

    private static Option<Component> Calculated(CssChannel channel, string[] terms) =>
        from held in terms is [var keyword, var sign, var operand] ? Some((Keyword: keyword, Operation: sign, Operand: operand)) : None
        where string.Equals(held.Keyword, channel.Keyword, StringComparison.Ordinal)
        from operation in CalcOperator.Validated(held.Operation).ToOption()
        from step in channel.Read(held.Operand)
        from component in operation.Made(step)
        select component;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CalcOperator {
    public static readonly CalcOperator Plus = new("+", static by => Some<Component>(new Component.Offset(by)));
    public static readonly CalcOperator Minus = new("-", static by => Some<Component>(new Component.Offset(-by)));
    public static readonly CalcOperator Times = new("*", static by => Some<Component>(new Component.Scaled(by)));
    public static readonly CalcOperator Over = new("/", static by => by is 0d ? None : Some<Component>(new Component.Scaled(1d / by)));

    [UseDelegateFromConstructor]
    public partial Option<Component> Made(double by);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record Ink {
    private const string DataKey = "data";

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Ink? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : Read(value.Trim()).Match(
                Some: static ink => ((Ink?)ink, (ValidationError?)null),
                None: () => (null, new ValidationError($"ink '{value}' is none of #RRGGBB[AA], a CSS colour name, rgb(), hsl(), hwb(), lab(), lch(), oklab(), oklch() or color() with optional from, color-mix(), color-contrast(), var(), [role.]channel[:condition][/alpha], data[/alpha]")));
        return fault;
    }

    public string ToValue() =>
        Switch(
            @fixed: static @fixed => Bytes(@fixed.Color)
                ? @fixed.Color.ToUnicolour() switch { var colour => $"{colour.Rgb.Byte255.Clipped.Hex}{colour.Alpha.Clipped.Hex}" }
                : $"color(srgb {Written(@fixed.Color.Red)} {Written(@fixed.Color.Green)} {Written(@fixed.Color.Blue)} / {Written(@fixed.Color.Alpha)})",
            host: static host => host.Fallback.Match(Some: fallback => $"var({host.Reference}, {fallback.ToValue()})", None: () => host.Reference),
            data: static data => $"{DataKey}{Faded(data.Alpha)}",
            mixed: static mixed =>
                $"color-mix(in {mixed.Space.Key}{(mixed.Hue is HueSpan.Shorter ? "" : $" {JsonNamingPolicy.KebabCaseLower.ConvertName(mixed.Hue.ToString())} hue")}, {mixed.From.ToValue()}, {mixed.To.ToValue()} {(decimal.Parse(Written((float)mixed.Amount), NumberStyles.Float, CultureInfo.InvariantCulture) * 100m).ToString("G29", CultureInfo.InvariantCulture)}%)",
            contrasting: static contrasting => $"color-contrast({contrasting.Over.ToValue()} vs {string.Join(", ", contrasting.Candidates.Map(static candidate => candidate.ToValue()))})",
            relative: static relative => relative.Space.Written(Some(relative.From),
                $"{string.Join(' ', relative.Space.Channels.Zip(relative.Channels).Map(static held => held.Second.Written(held.First)))}{relative.Alpha.Map(static alpha => $" / {alpha.Written(CssChannel.Alpha)}").IfNone("")}"));

    // --- [WIRE]
    internal static string Written(double value) => value.ToString("R", CultureInfo.InvariantCulture);

    internal static string Written(float value) => value.ToString("R", CultureInfo.InvariantCulture);

    private static string Faded(Fraction alpha) => (float)alpha < 1f ? $"/{Written((float)alpha)}" : "";

    private static bool Bytes(SKColorF color) =>
        Seq(color.Red, color.Green, color.Blue, color.Alpha).ForAll(static channel => (float)(double.Round(channel * 255d, MidpointRounding.ToEven) / 255d) == channel);

    internal static string[] Split(string text, Func<char, bool> separator) {
        List<string> parts = [];
        (int depth, int start) = (0, 0);
        for (int index = 0; index < text.Length; index++) {
            depth += text[index] switch { '(' => 1, ')' => -1, _ => 0 };
            if (depth is 0 && separator(text[index])) {
                parts.Add(text[start..index]);
                start = index + 1;
            }
        }
        parts.Add(text[start..]);
        return [.. parts.Select(static part => part.Trim()).Where(static part => part.Length > 0)];
    }

    internal static Option<(string Name, string Body)> Called(string text) =>
        text.IndexOf('(', StringComparison.Ordinal) switch {
            var open when open > 0 && text[^1] is ')' && text[..open].All(static letter => char.IsAsciiLetterLower(letter) || letter is '-') && Balanced(text[(open + 1)..^1]) =>
                Some((text[..open], text[(open + 1)..^1])),
            _ => None,
        };

    private static bool Balanced(string body) =>
        body.Aggregate((Depth: 0, Held: true), static (state, letter) => (state.Depth + letter switch { '(' => 1, ')' => -1, _ => 0 }) switch {
            var depth => (depth, state.Held && depth >= 0),
        }) is { Depth: 0, Held: true };

    private static Option<Ink> Read(string text) =>
        text switch {
            ['#', not '#', ..] => Try.lift(() => new Unicolour(text)).Run().ToOption().Map(static colour => (Ink)new Fixed(colour.ToSKColorF())),
            _ => Called(text).Match(
                Some: static call => Function(call.Name, call.Body),
                None: () => Referenced(text) | (text.All(char.IsAsciiLetterLower) ? Optional(Css.FromName(text)).Map(static colour => (Ink)new Fixed(colour.ToSKColorF())) : None)),
        };

    private static Option<Ink> Function(string name, string body) =>
        CssFunction.Validated(name).ToOption().Match(
            Some: function => function.Read(body),
            None: () => Spaced(name, function: true).Bind(space => Spaced(space, body)));

    internal static Option<Ink> Varied(string body) =>
        Split(body, static letter => letter is ',') switch {
            [var reference] => Hosted(reference).Map(static host => (Ink)host),
            [var reference, var fallback] => from host in Hosted(reference) from otherwise in Read(fallback) select (Ink)(host with { Fallback = Some(otherwise) }),
            _ => None,
        };

    internal static Option<Ink> Colored(string body) =>
        Originated(Split(body, char.IsWhiteSpace)) switch {
            (var origin, { Length: > 0 } rest) => Spaced(rest[0], function: false).Bind(space => Channeled(space, (origin, rest[1..]))),
            _ => None,
        };

    internal static Option<Ink> Spaced(CssSpace space, string body) =>
        Split(body, static letter => letter is ',') switch {
            { Length: 3 } legacy => Channeled(space, (None, legacy)),
            { Length: 4 } legacy => Channeled(space, (None, [legacy[0], legacy[1], legacy[2], "/", legacy[3]])),
            _ => Channeled(space, Originated(Split(body, char.IsWhiteSpace))),
        };

    private static Option<CssSpace> Spaced(string name, bool function) => CssSpace.Validated(name).ToOption().Filter(space => space.Function == function);

    private static (Option<string> Origin, string[] Tokens) Originated(string[] tokens) =>
        tokens is ["from", var origin, ..] ? (Some(origin), Slashed(tokens[2..])) : (None, Slashed(tokens));

    private static string[] Slashed(string[] tokens) =>
        [.. tokens.SelectMany(static token => token.Contains('(', StringComparison.Ordinal)
            ? Seq(token)
            : toSeq(token.Split('/').SelectMany(static (part, index) => index is 0 ? Seq(part) : Seq("/", part)).Where(static part => part.Length > 0)))];

    private static Option<Ink> Channeled(CssSpace space, (Option<string> Origin, string[] Tokens) held) =>
        from split in held.Tokens switch {
            [var first, var second, var third] => Some((Channels: Seq(first, second, third), Alpha: Option<string>.None)),
            [var first, var second, var third, "/", var alpha] => Some((Channels: Seq(first, second, third), Alpha: Some(alpha))),
            _ => None,
        }
        from ink in held.Origin.Match(Some: reference => Derived(space, reference, split.Channels, split.Alpha), None: () => Stated(space, split.Channels, split.Alpha))
        select ink;

    private static Option<Ink> Derived(CssSpace space, string reference, Seq<string> channels, Option<string> alpha) =>
        from source in Read(reference)
        from components in space.Channels.Zip(channels).Traverse(static pair => Component.Read(pair.First, pair.Second)).As()
        from shift in alpha.Match(Some: static token => Component.Read(CssChannel.Alpha, token).Map(Some), None: static () => Some(Option<Component>.None))
        select (Ink)new Relative(source, space, components, shift);

    private static Option<Ink> Stated(CssSpace space, Seq<string> channels, Option<string> alpha) =>
        from values in space.Channels.Zip(channels).Traverse(static pair => pair.First.Read(pair.Second)).As()
        from opacity in alpha.Match(Some: static token => CssChannel.Alpha.Read(token), None: static () => Some(1d))
        select (Ink)new Fixed(space.Colour(values, double.Clamp(opacity, 0d, 1d)).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF());

    internal static Option<Ink> Mixing(string body) =>
        Split(body, static letter => letter is ',') switch {
            [var method, var left, var right] =>
                from interpolation in Split(method, char.IsWhiteSpace) switch {
                    ["in", var space] => CssSpace.Validated(space).ToOption().Map(static held => (Space: held, Hue: HueSpan.Shorter)),
                    ["in", var space, var hue, "hue"] => from held in CssSpace.Validated(space).ToOption() from span in Spanned(hue) select (Space: held, Hue: span),
                    _ => None,
                }
                from start in Weighted(left)
                from end in Weighted(right)
                from amount in (start.Percent, end.Percent) switch {
                    ( { IsNone: true }, { IsNone: true }) => Some(0.5d),
                    (var first, { IsNone: true }) => first.Map(static percent => (100d - percent) / 100d),
                    ( { IsNone: true }, var second) => second.Map(static percent => percent / 100d),
                    (var first, var second) => from low in first from high in second where low + high is 100d select high / 100d,
                }
                from fraction in Fraction.Validated((float)amount).ToOption()
                select (Ink)new Mixed(start.Ink, end.Ink, fraction) { Space = interpolation.Space, Hue = interpolation.Hue },
            _ => None,
        };

    private static Option<HueSpan> Spanned(string hue) =>
        toSeq(Enum.GetValues<HueSpan>()).Find(span => string.Equals(JsonNamingPolicy.KebabCaseLower.ConvertName(span.ToString()), hue, StringComparison.Ordinal));

    private static Option<(Ink Ink, Option<double> Percent)> Weighted(string part) =>
        Split(part, char.IsWhiteSpace) switch {
            [var ink] => Read(ink).Map(static held => (Ink: held, Percent: Option<double>.None)),
            [var ink, var percent] =>
                from held in Read(ink)
                from share in percent.EndsWith('%') ? Invariant.Number<double>(percent.AsSpan()[..^1]) : None
                where share is >= 0d and <= 100d
                select (Ink: held, Percent: Some(share)),
            _ => None,
        };

    internal static Option<Ink> Contrasted(string body) =>
        Split(body, static letter => letter is ',') switch {
            { Length: > 0 } parts => Split(parts[0], char.IsWhiteSpace) switch {
                [var over, "vs", var first] =>
                    from ground in Read(over)
                    from candidates in (Seq(first) + toSeq(parts[1..])).Traverse(static candidate => Read(candidate)).As()
                    select (Ink)new Contrasting(ground, candidates),
                _ => None,
            },
            _ => None,
        };

    private static Option<Ink> Referenced(string value) =>
        Faded(value).Bind(static faded => string.Equals(faded.Reference, DataKey, StringComparison.Ordinal)
            ? Some<Ink>(new Data(faded.Alpha))
            : Hosted(faded.Reference, faded.Alpha).Map(static host => (Ink)host));

    private static Option<Host> Hosted(string value) => Faded(value).Bind(static faded => Hosted(faded.Reference, faded.Alpha));

    private static Option<(string Reference, Fraction Alpha)> Faded(string value) =>
        value.Split('/') switch {
            [var reference] => Some((Reference: reference, Alpha: Fraction.MaxValue)),
            [var reference, var alpha] => from scale in Invariant.Number<double>(alpha) from held in Fraction.Validated((float)scale).ToOption() select (Reference: reference, Alpha: held),
            _ => None,
        };

    private static Option<Host> Hosted(string reference, Fraction alpha) =>
        from held in reference.Split(':') switch {
            [var path] => Some((Path: path, Condition: Option<Condition>.None)),
            [var path, var condition] => Condition.Validated(condition).ToOption().Map(stated => (Path: path, Condition: Some(stated))),
            _ => None,
        }
        from host in held.Path.Split('.') switch {
            [var channel] => from shown in Channel.Validated(channel).ToOption() select new Host(None, shown, alpha),
            [var role, var channel] => from owner in Role.Validated(role).ToOption() from shown in Channel.Validated(channel).ToOption() select new Host(Some(owner), shown, alpha),
            _ => None,
        }
        select host with { Condition = held.Condition };

    public sealed record Fixed(SKColorF Color) : Ink {
        public static Fin<SKColorF> Parsed(string text) =>
            Read(text.Trim()).Bind(static ink => ink is Fixed held ? Some(held.Color) : None).ToFin(Errors.ParseError($"'{text}' is no theme-free colour literal"));
    }

    public sealed record Host(Option<Role> Role, Channel Channel, Fraction Alpha) : Ink {
        public Option<Condition> Condition { get; init; }
        public Option<Ink> Fallback { get; init; }

        internal string Reference =>
            $"{Role.Map(static role => $"{role}.").IfNone("")}{Channel.Key}{Condition.Map(static condition => $":{condition.Key}").IfNone("")}{Faded(Alpha)}";
    }

    public sealed record Data(Fraction Alpha) : Ink;

    public sealed record Mixed(Ink From, Ink To, Fraction Amount) : Ink {
        public CssSpace Space { get; init; } = CssSpace.Oklab;
        public HueSpan Hue { get; init; } = HueSpan.Shorter;
    }

    public sealed record Contrasting(Ink Over, Seq<Ink> Candidates) : Ink;

    public sealed record Relative(Ink From, CssSpace Space, Seq<Component> Channels, Option<Component> Alpha) : Ink {
        internal SKColorF Shifted(SKColorF origin) =>
            origin.ToUnicolour() switch {
                var colour => Space.Colour(
                        Space.Channeled(colour).Zip(Channels).Map(static held => held.Second.Applied(held.First)),
                        double.Clamp(Alpha.Map(alpha => alpha.Applied(colour.Alpha.A)).IfNone(colour.Alpha.A), 0d, 1d))
                    .MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF(),
            };
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CssFunction {
    public static readonly CssFunction Var = new("var", Ink.Varied);
    public static readonly CssFunction Mix = new("color-mix", Ink.Mixing);
    public static readonly CssFunction Contrast = new("color-contrast", Ink.Contrasted);
    public static readonly CssFunction Color = new("color", Ink.Colored);
    public static readonly CssFunction Rgba = new("rgba", static body => Ink.Spaced(CssSpace.Rgb, body));
    public static readonly CssFunction Hsla = new("hsla", static body => Ink.Spaced(CssSpace.Hsl, body));

    [UseDelegateFromConstructor]
    public partial Option<Ink> Read(string body);
}

public sealed record Series(uint Ordinal) {
    public Option<SKColorF> Color { get; init; }
}

public sealed record Halo(Ink Ink, float Width, float Blur);

public sealed record TextFace : Monoid<TextFace> {
    public static TextFace Empty { get; } = new();

    public Option<string> Family { get; init; }
    public Option<SKFontStyleWeight> Weight { get; init; }
    public Option<SKFontStyleWidth> Width { get; init; }
    public Option<bool> Italic { get; init; }
    public Option<float> LetterSpacing { get; init; }
    public Option<float> LineHeight { get; init; }
    public Option<Topten.RichTextKit.UnderlineStyle> Underline { get; init; }
    public Option<Topten.RichTextKit.StrikeThroughStyle> StrikeThrough { get; init; }
    public Option<Topten.RichTextKit.FontVariant> Variant { get; init; }
    public Option<Seq<Feature>> Features { get; init; }
    public Option<Halo> Halo { get; init; }

    public TextFace Combine(TextFace rhs) =>
        new() {
            Family = rhs.Family | Family,
            Weight = rhs.Weight | Weight,
            Width = rhs.Width | Width,
            Italic = rhs.Italic | Italic,
            LetterSpacing = rhs.LetterSpacing | LetterSpacing,
            LineHeight = rhs.LineHeight | LineHeight,
            Underline = rhs.Underline | Underline,
            StrikeThrough = rhs.StrikeThrough | StrikeThrough,
            Variant = rhs.Variant | Variant,
            Features = rhs.Features | Features,
            Halo = rhs.Halo | Halo,
        };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FilterFunction {
    private static readonly Vector3 Luminance = new(0.2126f, 0.7152f, 0.0722f);
    private static readonly Vector3 Saturation = new(0.213f, 0.715f, 0.072f);
    private static readonly (Vector3 Red, Vector3 Green, Vector3 Blue) Aged = (new(0.393f, 0.769f, 0.189f), new(0.349f, 0.686f, 0.168f), new(0.272f, 0.534f, 0.131f));
    private static readonly (Vector3 Red, Vector3 Green, Vector3 Blue) Quadrature = (new(-0.213f, -0.715f, 0.928f), new(0.143f, 0.140f, -0.283f), new(-0.787f, 0.715f, 0.072f));

    public static readonly FilterFunction Blur = new("blur", "px", Length, NonNegative, static (amount, input) => SKImageFilter.CreateBlur(amount, amount, input));
    public static readonly FilterFunction Grayscale = new("grayscale", "", Proportion, NonNegative,
        static (amount, input) => Matrixed(Toward((Luminance, Luminance, Luminance), 1f - float.Min(amount, 1f)), Vector3.Zero, input));
    public static readonly FilterFunction Saturate = new("saturate", "", Proportion, NonNegative, static (amount, input) => Matrixed(Toward((Saturation, Saturation, Saturation), amount), Vector3.Zero, input));
    public static readonly FilterFunction Brightness = new("brightness", "", Proportion, NonNegative, static (amount, input) => Matrixed(Scaled(amount), Vector3.Zero, input));
    public static readonly FilterFunction Contrast = new("contrast", "", Proportion, NonNegative, static (amount, input) => Matrixed(Scaled(amount), new Vector3(0.5f - (0.5f * amount)), input));
    public static readonly FilterFunction HueRotate = new("hue-rotate", "deg", Angle, float.IsFinite, static (amount, input) => Matrixed(Turned(float.DegreesToRadians(amount)), Vector3.Zero, input));
    public static readonly FilterFunction Invert = new("invert", "", Proportion, NonNegative,
        static (amount, input) => float.Min(amount, 1f) switch { var held => Matrixed(Scaled(1f - (2f * held)), new Vector3(held), input) });
    public static readonly FilterFunction Sepia = new("sepia", "", Proportion, NonNegative, static (amount, input) => Matrixed(Toward(Aged, 1f - float.Min(amount, 1f)), Vector3.Zero, input));

    public string Unit { get; }

    [UseDelegateFromConstructor]
    internal partial Option<double> Read(string token);

    [UseDelegateFromConstructor]
    public partial bool Accepts(float amount);

    [UseDelegateFromConstructor]
    internal partial SKImageFilter Built(float amount, SKImageFilter? input);

    private static Option<double> Length(string token) => token.EndsWith("px", StringComparison.Ordinal) ? Invariant.Number<double>(token.AsSpan()[..^2]) : Invariant.Number<double>(token).Filter(static held => held is 0d);

    private static Option<double> Proportion(string token) => token.EndsWith('%') ? Invariant.Number<double>(token.AsSpan()[..^1]).Map(static held => held / 100d) : Invariant.Number<double>(token);

    private static Option<double> Angle(string token) => CssChannel.Degrees(token) | Invariant.Number<double>(token).Filter(static held => held is 0d);

    private static bool NonNegative(float amount) => amount >= 0f && float.IsFinite(amount);

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Toward((Vector3 Red, Vector3 Green, Vector3 Blue) target, float kept) =>
        (Vector3.Lerp(target.Red, Vector3.UnitX, kept), Vector3.Lerp(target.Green, Vector3.UnitY, kept), Vector3.Lerp(target.Blue, Vector3.UnitZ, kept));

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Scaled(float slope) => (Vector3.UnitX * slope, Vector3.UnitY * slope, Vector3.UnitZ * slope);

    private static (Vector3 Red, Vector3 Green, Vector3 Blue) Turned(float radians) =>
        MathF.SinCos(radians) switch {
            var (sin, cos) => (
                Vector3.Lerp(Saturation, Vector3.UnitX, cos) + (Quadrature.Red * sin),
                Vector3.Lerp(Saturation, Vector3.UnitY, cos) + (Quadrature.Green * sin),
                Vector3.Lerp(Saturation, Vector3.UnitZ, cos) + (Quadrature.Blue * sin)),
        };

    private static SKImageFilter Matrixed((Vector3 Red, Vector3 Green, Vector3 Blue) rows, Vector3 offset, SKImageFilter? input) {
        using SKColorFilter color = SKColorFilter.CreateColorMatrix([
            rows.Red.X, rows.Red.Y, rows.Red.Z, 0f, offset.X,
            rows.Green.X, rows.Green.Y, rows.Green.Z, 0f, offset.Y,
            rows.Blue.X, rows.Blue.Y, rows.Blue.Z, 0f, offset.Z,
            0f, 0f, 0f, 1f, 0f,
        ]);
        return SKImageFilter.CreateColorFilter(color, input);
    }
}

[ComplexValueObject]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public sealed partial class Filter {
    public FilterFunction Function { get; }
    public float Amount { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref FilterFunction function, ref float amount) =>
        validationError = function.Accepts(amount) ? null : new ValidationError($"{function.Key}() takes no amount {Ink.Written(amount)}");

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Filter? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : (from call in Ink.Called(value.Trim())
               from function in FilterFunction.Validated(call.Name).ToOption()
               from amount in function.Read(call.Body.Trim())
               select (Function: function, Amount: (float)amount)).Match(
                Some: static held => Validate(held.Function, held.Amount, out Filter? made) switch { var rejected => (rejected is null ? made : null, rejected) },
                None: () => (null, new ValidationError(
                    $"filter '{value}' is none of {string.Join(", ", FilterFunction.Items.Select(static function => $"{function.Key}(<amount>{function.Unit})"))}")));
        return fault;
    }

    public string ToValue() => $"{Function.Key}({Ink.Written(Amount)}{Function.Unit})";

    internal static Option<SKImageFilter> Chained(Seq<Filter> filters) =>
        filters.Fold(Option<SKImageFilter>.None, static (input, filter) => input.Match(
            Some: inner => {
                using SKImageFilter held = inner;
                return Some(filter.Function.Built(filter.Amount, held));
            },
            None: () => Some(filter.Function.Built(filter.Amount, input: null))));
}

public sealed record Stop<TColor>(TColor Color, Fraction At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Solid), "solid")]
[JsonDerivedType(typeof(Gradient), "gradient")]
[JsonDerivedType(typeof(Drawn), "drawn")]
[JsonDerivedType(typeof(Backdrop), "backdrop")]
[JsonDerivedType(typeof(Blurred), "blurred")]
public abstract partial record Material {
    public sealed record Solid(Ink Ink) : Material;

    public sealed record Gradient(Seq<Stop<Ink>> Stops, float Angle) : Material;

    public sealed record Drawn(SKRuntimeEffect Effect, HashMap<string, Ink> Inks) : Material {
        internal const string Size = "size";
        internal const string Clock = "time";

        public bool Timed => Effect.Uniforms.Contains(Clock, StringComparer.Ordinal);
    }

    public sealed record Backdrop(Glass Glass, Ink Fallback) : Material {
        public Option<Ink> Tint { get; init; }
    }

    public sealed record Blurred(float Sigma, Ink Tint) : Material;
}

public sealed record Stroke(Ink Ink, Seq<Seq<float>> Dash = default) {
    public Seq<float> Rhythm(uint ordinal) => Dash.IsEmpty ? [] : Dash[(int)(ordinal % (uint)Dash.Count)];
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Drop), "drop")]
[JsonDerivedType(typeof(Inset), "inset")]
public abstract partial record Shadow {
    public required Ink Ink { get; init; }
    public required float Blur { get; init; }
    public float X { get; init; }
    public float Y { get; init; }
    public float Spread { get; init; }

    internal abstract SKClipOperation Clip { get; }

    internal abstract float Overhang { get; }

    internal abstract void Cast(SKCanvas canvas, Func<float, SKPath> outline, SKColorF color);

    internal abstract SKImageFilter Filtered(SKColorF color, float scale);

    private protected T Masked<T>(SKColorF color, Func<SKPaint, T> use) {
        using SKMaskFilter? blur = Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Blur) : null;
        using SKPaint paint = new() { IsAntialias = true, ColorF = color, MaskFilter = blur };
        return use(paint);
    }

    private protected static SKImageFilter? Morphed(float by) =>
        by switch {
            > 0f => SKImageFilter.CreateDilate(by, by),
            < 0f => SKImageFilter.CreateErode(-by, -by),
            _ => null,
        };

    public sealed record Drop : Shadow {
        internal override SKClipOperation Clip => SKClipOperation.Difference;

        internal override float Overhang => Masked(SKColorF.Empty, paint => FastBounds.Beyond(paint, Spread, new SKPoint(X, Y)));

        internal override SKImageFilter Filtered(SKColorF color, float scale) {
            using SKImageFilter? grown = Morphed(Spread * scale);
            return SKImageFilter.CreateDropShadowOnly(X * scale, Y * scale, Blur * scale, Blur * scale, (SKColor)color, grown);
        }

        internal override void Cast(SKCanvas canvas, Func<float, SKPath> outline, SKColorF color) =>
            _ = Masked(color, paint => {
                using SKPath shape = outline(Spread);
                shape.Offset(X, Y);
                canvas.DrawPath(shape, paint);
                return unit;
            });
    }

    public sealed record Inset : Shadow {
        internal override SKClipOperation Clip => SKClipOperation.Intersect;

        internal override float Overhang => 0f;

        internal override SKImageFilter Filtered(SKColorF color, float scale) {
            using SKImageFilter? kept = Morphed(-Spread * scale);
            using SKColorFilter hollow = SKColorFilter.CreateColorMatrix([
                0f, 0f, 0f, 0f, color.Red,
                0f, 0f, 0f, 0f, color.Green,
                0f, 0f, 0f, 0f, color.Blue,
                0f, 0f, 0f, -color.Alpha, color.Alpha,
            ]);
            using SKImageFilter outside = SKImageFilter.CreateColorFilter(hollow, kept);
            using SKImageFilter moved = SKImageFilter.CreateOffset(X * scale, Y * scale, outside);
            using SKImageFilter blurred = SKImageFilter.CreateBlur(Blur * scale, Blur * scale, moved);
            return SKImageFilter.CreateBlendMode(SKBlendMode.SrcIn, background: null, foreground: blurred);
        }

        internal override void Cast(SKCanvas canvas, Func<float, SKPath> outline, SKColorF color) =>
            _ = Masked(color, paint => {
                using SKPath frame = outline(0f);
                using SKPath hole = outline(-Spread);
                hole.Offset(X, Y);
                _ = paint.GetFastBounds(frame.Bounds, out SKRect reach);
                using SKPath ring = Figure.Traced(path => {
                    path.FillType = SKPathFillType.EvenOdd;
                    path.AddRect(SKRect.Union(reach, hole.Bounds), SKPathDirection.Clockwise);
                    path.AddPath(hole, SKPathAddMode.Append);
                });
                canvas.DrawPath(ring, paint);
                return unit;
            });
    }
}

public sealed record Look : Monoid<Look> {
    public static Look Empty { get; } = new();

    public Option<Seq<Material>> Fills { get; init; }
    public Option<Seq<Stroke>> Strokes { get; init; }
    public Option<Ink> Text { get; init; }
    public TextFace Face { get; init; } = TextFace.Empty;
    public Option<Seq<Shadow>> Shadows { get; init; }
    public Option<Seq<Filter>> Filters { get; init; }
    public Option<Seq<Ink>> Palette { get; init; }
    public HashMap<Interaction, Ink> Washes { get; init; }
    public Option<SKBlendMode> Blend { get; init; }
    public Option<TimingCurve> Motion { get; init; }
    public HashMap<Condition, Look> When { get; init; }
    public HashMap<Role, Look> Within { get; init; }

    public Look Combine(Look rhs) =>
        new() {
            Fills = rhs.Fills | Fills,
            Strokes = rhs.Strokes | Strokes,
            Text = rhs.Text | Text,
            Face = Face.Combine(rhs.Face),
            Shadows = rhs.Shadows | Shadows,
            Filters = rhs.Filters | Filters,
            Palette = rhs.Palette | Palette,
            Washes = Washes.Union(rhs.Washes, Merge: static (_, _, over) => over),
            Blend = rhs.Blend | Blend,
            Motion = rhs.Motion | Motion,
            When = When.Union(rhs.When, Merge: static (_, under, over) => under.Combine(over)),
            Within = Within.Union(rhs.Within, Merge: static (_, under, over) => under.Combine(over)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FastBounds {
    private static readonly SKRect Probe = SKRect.Create(1f, 1f);

    internal static float Beyond(SKPaint paint, float grown, SKPoint offset) {
        SKRect drawn = SKRect.Inflate(Probe, grown, grown);
        drawn.Offset(offset);
        _ = paint.GetFastBounds(drawn, out SKRect bounds);
        return float.Max(float.Max(Probe.Left - bounds.Left, Probe.Top - bounds.Top), float.Max(bounds.Right - Probe.Right, bounds.Bottom - Probe.Bottom));
    }
}

internal static class Colours {
    extension(SKColorF color) {
        internal Unicolour ToUnicolour() => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);
    }

    extension(Unicolour colour) {
        internal SKColorF ToSKColorF() => new((float)colour.Rgb.R, (float)colour.Rgb.G, (float)colour.Rgb.B, (float)colour.Alpha.A);
    }
}

internal sealed class RuntimeEffectJsonConverter : JsonConverter<SKRuntimeEffect> {
    public override SKRuntimeEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(reader.GetString(), out string errors);
        return errors is null ? effect : throw new JsonException($"drawn effect fails to compile: {errors}");
    }

    public override void Write(Utf8JsonWriter writer, SKRuntimeEffect value, JsonSerializerOptions options) =>
        throw new NotSupportedException("a compiled drawn effect keeps no SkSL source to write");
}

internal sealed class FeatureJsonConverter : JsonConverter<Feature> {
    public override Feature Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch {
            var tag when Feature.TryParse(tag, out Feature feature) => feature,
            var tag => throw new JsonException($"'{tag}' is no OpenType feature"),
        };

    public override void Write(Utf8JsonWriter writer, Feature value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
