using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Imaging.Scopes;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ScopeTargets(SKColorF Red, SKColorF Yellow, SKColorF Green, SKColorF Cyan, SKColorF Blue, SKColorF Magenta);

public sealed record ScopePalette(
    SKColorF Red,
    SKColorF Green,
    SKColorF Blue,
    float HistogramFill,
    SKColorF Extents,
    SKColorF Disc,
    SKColorF Rings,
    SKColorF Crosshair,
    ScopeTargets Targets,
    SKColorF TargetBox,
    SKColorF Skin,
    float SkinAngle,
    SKColorF Broadcast,
    SKColorF Over,
    SKColorF Under) {
    public static ScopePalette Shipped { get; } = Read();

    private static ScopePalette Read() {
        using Stream stream = typeof(ScopePalette).Assembly.GetManifestResourceStream(typeof(ScopePalette), "Scopes.json")!;
        return JsonSerializer.Deserialize(stream, ScopesContext.Default.ScopePalette)!;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true, Converters = [typeof(HexColor)])]
[JsonSerializable(typeof(ScopePalette))]
internal sealed partial class ScopesContext : JsonSerializerContext;
