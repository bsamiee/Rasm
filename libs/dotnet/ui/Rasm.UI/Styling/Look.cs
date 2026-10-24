using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using HarfBuzzSharp;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
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
        (item, ValidationError? fault) = (value is ['#', not '#', ..] ? Hex(value) : Referenced(value ?? "").ToFin(Error.New("is neither #RRGGBB[AA], [role.]channel[/alpha], nor data[/alpha] with alpha in [0, 1]")))
            .Match(Succ: static ink => ((Ink?)ink, (ValidationError?)null), Fail: error => (null, new ValidationError($"ink '{value}' {error.Message}")));
        return fault;
    }

    public string ToValue() =>
        Switch(
            @fixed: static @fixed => Colour(@fixed.Color) switch {
                var colour => $"{colour.Rgb.Byte255.Clipped.Hex}{colour.Alpha.Clipped.Hex}",
            },
            host: static host => string.Create(CultureInfo.InvariantCulture, $"{host.Role.Map(static role => $"{role.Key}.").IfNone("")}{host.Channel.Key}/{host.Alpha}"),
            data: static data => string.Create(CultureInfo.InvariantCulture, $"{DataKey}/{data.Alpha}"));

    private static Fin<Ink> Hex(string hex) =>
        Try.lift(() => new Unicolour(hex)).Run()
            .Map(static colour => (Ink)new Fixed(Rgb(colour)));

    internal static Unicolour Colour(SKColorF color) => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);

    internal static SKColorF Rgb(Unicolour colour) => new((float)colour.Rgb.R, (float)colour.Rgb.G, (float)colour.Rgb.B, (float)colour.Alpha.A);

    private static Option<Ink> Referenced(string value) =>
        value.Split('/') switch {
            [var reference] => Referenced(reference, 1f),
            [var reference, var alpha] => Scaled(alpha).Bind(scale => Referenced(reference, scale)),
            _ => None,
        };

    private static Option<float> Scaled(string text) =>
        (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale) ? Some(scale) : None).Filter(static scale => scale is >= 0f and <= 1f);

    private static Option<Ink> Referenced(string reference, float alpha) =>
        reference.Split('.') switch {
            [DataKey] => new Data(alpha),
            [var channel] => from held in Found<Channel>(channel) select (Ink)new Host(None, held, alpha),
            [var role, var channel] => from owner in Found<Role>(role) from held in Found<Channel>(channel) select (Ink)new Host(Some(owner), held, alpha),
            _ => None,
        };

    private static Option<T> Found<T>(string key) where T : class, ISmartEnum<string, T, ValidationError> => T.TryGet(key, out T? item) ? Some(item) : None;

    public sealed record Fixed(SKColorF Color) : Ink;

    public sealed record Host(Option<Role> Role, Channel Channel, float Alpha) : Ink;

    public sealed record Data(float Alpha) : Ink;
}

public sealed record Series(uint Ordinal, SKColorF Color);

public sealed record Stop(Ink Ink, float At);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Solid), "solid")]
[JsonDerivedType(typeof(Gradient), "gradient")]
[JsonDerivedType(typeof(Drawn), "drawn")]
[JsonDerivedType(typeof(Backdrop), "backdrop")]
public abstract partial record Material {
    public sealed record Solid(Ink Ink) : Material;

    public sealed record Gradient(Seq<Stop> Stops, float Angle) : Material;

    public sealed record Drawn(SKRuntimeEffect Effect, HashMap<string, Ink> Inks) : Material {
        internal const string Clock = "time";

        public bool Timed => Effect.Uniforms.Contains(Clock, StringComparer.Ordinal);
    }

    public sealed record Backdrop(Glass Glass, Ink Fallback) : Material;
}

public sealed record Stroke(Ink Ink, Seq<Seq<float>> Dash = default) {
    public Seq<float> Rhythm(uint ordinal) => Dash.IsEmpty ? [] : Dash[(int)(ordinal % (uint)Dash.Count)];
}

public sealed record Shadow(Ink Ink, float Blur, float X = 0f, float Y = 0f, float Spread = 0f);

public sealed record Look {
    public static Look Native { get; } = new();

    public Option<Material> Fill { get; init; } = Some<Material>(new Material.Solid(new Ink.Host(None, Channel.Fill, 1f)));
    public Option<Stroke> Stroke { get; init; } = Some(new Stroke(new Ink.Host(None, Channel.Edge, 1f)));
    public Ink Text { get; init; } = new Ink.Host(None, Channel.Text, 1f);
    public Seq<Shadow> Shadows { get; init; }
    public HashMap<Interaction, Ink> Washes { get; init; }
    public int Weight { get; init; } = 400;
    public Seq<Feature> Features { get; init; }
    public SKBlendMode Blend { get; init; } = SKBlendMode.SrcOver;
    public TimingCurve Motion { get; init; } = new TimingCurve.Instant();

    internal Option<Ink> Solid => Fill.Bind(static fill => Optional(fill as Material.Solid)).Map(static solid => solid.Ink);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal sealed class Sksl : JsonConverter<SKRuntimeEffect> {
    public override SKRuntimeEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        SKRuntimeEffect effect = SKRuntimeEffect.CreateShader(reader.GetString(), out string errors);
        return errors is null ? effect : throw new JsonException($"drawn effect fails to compile: {errors}");
    }

    public override void Write(Utf8JsonWriter writer, SKRuntimeEffect value, JsonSerializerOptions options) =>
        throw new NotSupportedException("a compiled drawn effect keeps no SkSL source to write");
}

internal sealed class Tags : JsonConverter<Feature> {
    public override Feature Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() switch {
            var tag when Feature.TryParse(tag, out Feature feature) => feature,
            var tag => throw new JsonException($"'{tag}' is no OpenType feature"),
        };

    public override void Write(Utf8JsonWriter writer, Feature value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
