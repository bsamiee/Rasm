using System.Text.Json;
using System.Text.Json.Serialization;
using SkiaSharp;
using Thinktecture.Text.Json.Serialization;
using Wacton.Unicolour;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class ChromeToken {
    public static readonly ChromeToken Background = new("background");
    public static readonly ChromeToken Surface = new("surface");
    public static readonly ChromeToken Field = new("field");
    public static readonly ChromeToken List = new("list");
    public static readonly ChromeToken Highlight = new("highlight");
    public static readonly ChromeToken Edge = new("edge");
    public static readonly ChromeToken Text = new("text");
    public static readonly ChromeToken Secondary = new("secondary");
    public static readonly ChromeToken Disabled = new("disabled");
    public static readonly ChromeToken Accent = new("accent");
    public static readonly ChromeToken SelectedText = new("selected-text");
}

public sealed record Chrome {
    private Chrome(HashMap<ChromeToken, SKColorF> colors) => Colors = colors;

    private HashMap<ChromeToken, SKColorF> Colors { get; }

    public SKColorF Color(ChromeToken token) => Colors[token];

    public static Chrome Of(Func<ChromeToken, SKColorF> read) => new(toHashMap(ChromeToken.Items.Select(token => (token, read(token)))));

    public static Chrome Interpolate(Chrome from, Chrome to, float amount) => Of(token => Styling.Colors.Mix(from.Color(token), to.Color(token), amount));
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class ColorRole {
    public static readonly ColorRole Ground = new("ground");
    public static readonly ColorRole Surface = new("surface");
    public static readonly ColorRole Field = new("field");
    public static readonly ColorRole List = new("list");
    public static readonly ColorRole Highlight = new("highlight");
    public static readonly ColorRole Edge = new("edge");
    public static readonly ColorRole Text = new("text");
    public static readonly ColorRole Secondary = new("secondary");
    public static readonly ColorRole Disabled = new("disabled");
    public static readonly ColorRole Accent = new("accent");
    public static readonly ColorRole Selection = new("selection");
    public static readonly ColorRole SelectedText = new("selected-text");
    public static readonly ColorRole Grid = new("grid");
    public static readonly ColorRole GridFine = new("grid-fine");
    public static readonly ColorRole Major = new("major");
    public static readonly ColorRole Zero = new("zero");
    public static readonly ColorRole Trace = new("trace");
    public static readonly ColorRole Label = new("label");
    public static readonly ColorRole Rim = new("rim");
    public static readonly ColorRole Shadow = new("shadow");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Host), "host")]
[JsonDerivedType(typeof(Fixed), "fixed")]
public abstract partial record Swatch {
    public abstract SKColorF Resolve(Chrome chrome);

    public sealed record Host(ChromeToken Token, float Alpha) : Swatch {
        public override SKColorF Resolve(Chrome chrome) => chrome.Color(Token) switch {
            var color => color.WithAlpha(color.Alpha * Alpha),
        };
    }

    public sealed record Fixed(SKColorF Color) : Swatch {
        public override SKColorF Resolve(Chrome chrome) => Color;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Themed), "themed")]
[JsonDerivedType(typeof(Fixed), "fixed")]
public abstract partial record Ink {
    public abstract Ink Faded(float alpha);

    public sealed record Themed(ColorRole Role, float Alpha) : Ink {
        public override Ink Faded(float alpha) => this with { Alpha = Alpha * alpha };
    }

    public sealed record Fixed(SKColorF Color) : Ink {
        public override Ink Faded(float alpha) => this with { Color = Color.WithAlpha(Color.Alpha * alpha) };
    }
}

public sealed record Palette {
    private Palette(HashMap<ColorRole, Swatch> swatches) => Swatches = swatches;

    private HashMap<ColorRole, Swatch> Swatches { get; }

    public Swatch Swatch(ColorRole role) => Swatches[role];

    public static Fin<Palette> Of(IReadOnlyDictionary<ColorRole, Swatch> swatches) =>
        toSeq(ColorRole.Items).Traverse(role => Optional(swatches.GetValueOrDefault(role)).ToFin(Error.New($"palette lacks color role {role}")).Map(swatch => (role, swatch))).As()
            .Map(static entries => new Palette(toHashMap(entries)));
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class Family {
    public static readonly Family Chrome = new("chrome");
    public static readonly Family Well = new("well");

    public Palette Palette => Appearances.Palette(this);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Colors {
    public static SKColorF Mix(SKColorF from, SKColorF to, float amount) =>
        Encoded(Colour(from).Mix(Colour(to), ColourSpace.Oklab, amount));

    public static SKColorF Parse(string hex) => Encoded(new Unicolour(hex));

    private static Unicolour Colour(SKColorF color) => new(ColourSpace.Rgb, color.Red, color.Green, color.Blue, color.Alpha);

    private static SKColorF Encoded(Unicolour colour) =>
        colour.Rgb.Clipped switch {
            var rgb => new SKColorF((float)rgb.R, (float)rgb.G, (float)rgb.B, (float)colour.Alpha.Clipped.A),
        };
}

public sealed class HexColor : JsonConverter<SKColorF> {
    public override SKColorF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Colors.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, SKColorF value, JsonSerializerOptions options) =>
        writer.WriteStringValue((SKColor)value switch {
            var color => $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}{color.Alpha:X2}",
        });
}
