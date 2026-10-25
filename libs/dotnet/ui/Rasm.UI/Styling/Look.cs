using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarfBuzzSharp;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<float>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Opacity : IMinMaxValue<Opacity> {
    public static Opacity MinValue { get; } = new(0f);
    public static Opacity MaxValue { get; } = new(1f);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Glass {
    public static readonly Glass Regular = new("regular");
    public static readonly Glass Clear = new("clear");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record Ink {
    private const string DataKey = "data";

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Ink? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : (value is ['#', not '#', ..] ? Hex(value) : Referenced(value).ToFin(Error.New("is neither #RRGGBB[AA], [role.]channel[/alpha], nor data[/alpha] with alpha in [0, 1]")))
                .Match(Succ: static ink => ((Ink?)ink, (ValidationError?)null), Fail: error => (null, new ValidationError($"ink '{value}' {error.Message}")));
        return fault;
    }

    public string ToValue() =>
        Switch(
            @fixed: static @fixed => @fixed.Color.ToUnicolour() switch {
                var colour => $"{colour.Rgb.Byte255.Clipped.Hex}{colour.Alpha.Clipped.Hex}",
            },
            host: static host => string.Create(CultureInfo.InvariantCulture, $"{host.Role.Map(static role => $"{role}.").IfNone("")}{host.Channel.Key}/{host.Alpha}"),
            data: static data => string.Create(CultureInfo.InvariantCulture, $"{DataKey}/{data.Alpha}"));

    private static Fin<Ink> Hex(string hex) =>
        Try.lift(() => new Unicolour(hex)).Run()
            .Map(static colour => (Ink)new Fixed(colour.ToSKColorF()));

    private static Option<Ink> Referenced(string value) =>
        value.Split('/') switch {
            [var reference] => Referenced(reference, Opacity.MaxValue),
            [var reference, var alpha] => Scaled(alpha).Bind(scale => Referenced(reference, scale)),
            _ => None,
        };

    private static Option<Opacity> Scaled(string text) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale) ? Opacity.Validated(scale).ToOption() : None;

    private static Option<Ink> Referenced(string reference, Opacity alpha) =>
        reference.Split('.') switch {
            [DataKey] => new Data(alpha),
            [var channel] => from held in Channel.Validated(channel).ToOption() select (Ink)new Host(None, held, alpha),
            [var role, var channel] => from owner in Role.Validated(role).ToOption() from held in Channel.Validated(channel).ToOption() select (Ink)new Host(Some(owner), held, alpha),
            _ => None,
        };

    public sealed record Fixed(SKColorF Color) : Ink;

    public sealed record Host(Option<Role> Role, Channel Channel, Opacity Alpha) : Ink;

    public sealed record Data(Opacity Alpha) : Ink;
}

public sealed record Series(uint Ordinal, SKColorF Color);

public sealed record Stop(Ink Ink, float At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Solid), "solid")]
[JsonDerivedType(typeof(Gradient), "gradient")]
[JsonDerivedType(typeof(Drawn), "drawn")]
[JsonDerivedType(typeof(Backdrop), "backdrop")]
[JsonDerivedType(typeof(Blurred), "blurred")]
public abstract partial record Material {
    public sealed record Solid(Ink Ink) : Material;

    public sealed record Gradient(Seq<Stop> Stops, float Angle) : Material;

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

public sealed record Shadow(Ink Ink, float Blur, float X = 0f, float Y = 0f, float Spread = 0f);

public sealed record Look : Monoid<Look> {
    public static Look Empty { get; } = new();

    public Option<Seq<Material>> Fills { get; init; }
    public Option<Seq<Stroke>> Strokes { get; init; }
    public Option<Ink> Text { get; init; }
    public Option<Seq<Shadow>> Shadows { get; init; }
    public HashMap<Interaction, Ink> Washes { get; init; }
    public Option<SKFontStyleWeight> Weight { get; init; }
    public Option<Seq<Feature>> Features { get; init; }
    public Option<SKBlendMode> Blend { get; init; }
    public Option<TimingCurve> Motion { get; init; }
    public HashMap<Condition, Look> When { get; init; }
    public HashMap<Role, Look> Within { get; init; }

    public Look Combine(Look rhs) =>
        new() {
            Fills = rhs.Fills | Fills,
            Strokes = rhs.Strokes | Strokes,
            Text = rhs.Text | Text,
            Shadows = rhs.Shadows | Shadows,
            Washes = Washes.Union(rhs.Washes, Merge: static (_, _, over) => over),
            Weight = rhs.Weight | Weight,
            Features = rhs.Features | Features,
            Blend = rhs.Blend | Blend,
            Motion = rhs.Motion | Motion,
            When = When.Union(rhs.When, Merge: static (_, under, over) => under.Combine(over)),
            Within = Within.Union(rhs.Within, Merge: static (_, under, over) => under.Combine(over)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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
