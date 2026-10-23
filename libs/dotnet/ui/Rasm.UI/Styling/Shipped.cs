using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record AppearanceFile(
    Dictionary<Family, Dictionary<ColorRole, Swatch>> Families,
    Dictionary<string, Dictionary<Role, Dictionary<InteractionState, LookFile>>> Appearances);

internal sealed record LookFile(MaterialFile Fill, Radius Radius, Elevation Elevation, Ink Ink, TypeRole Type, Duration Motion, Border? Border = null, Ink? StateLayer = null);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SolidFile), "solid")]
[JsonDerivedType(typeof(GradientFile), "gradient")]
[JsonDerivedType(typeof(DrawnFile), "drawn")]
[JsonDerivedType(typeof(BackdropFile), "backdrop")]
internal abstract partial record MaterialFile {
    internal sealed record SolidFile(Ink Ink) : MaterialFile;
    internal sealed record GradientFile(ImmutableArray<Stop> Stops, float Angle) : MaterialFile;
    internal sealed record DrawnFile(string Shader, ImmutableDictionary<string, Ink> Inks) : MaterialFile;
    internal sealed record BackdropFile(Glass Glass, Ink Fallback) : MaterialFile;
}

internal sealed record Shipped(HashMap<string, Appearance> Named, Appearance Native, FrozenDictionary<Role, Look> Rests, FrozenDictionary<Family, Palette> Palettes) {
    private const string NativeName = "native";

    public static Fin<Shipped> Read() {
        using Stream stream = typeof(Shipped).Assembly.GetManifestResourceStream(typeof(Shipped), "Appearances.json")!;
        return Decoded(JsonSerializer.Deserialize(stream, ShippedContext.Default.AppearanceFile)!);
    }

    private static Fin<Shipped> Decoded(AppearanceFile file) =>
        from palettes in toSeq(Family.Items).Traverse(family => Optional(file.Families.GetValueOrDefault(family)).ToFin(Error.New($"appearance file lacks family {family}"))
                .Bind(Palette.Of).Map(palette => (family, palette))).As()
        from appearances in toSeq(file.Appearances).Traverse(static named => Of(named.Key, named.Value).Map(appearance => (named.Key, appearance))).As().Map(static named => toHashMap(named))
        from native in appearances.Find(NativeName).ToFin(Error.New($"appearance file lacks the {NativeName} appearance"))
        from rests in toSeq(Role.Items).Traverse(role => native.Looks.Find((role, InteractionState.Rest)).ToFin(Error.New($"{NativeName} appearance lacks a rest look for {role}"))
                .Map(look => (role, look))).As()
        select new Shipped(appearances.Remove(NativeName), native,
            rests.ToFrozenDictionary(static entry => entry.role, static entry => entry.look),
            palettes.ToFrozenDictionary(static entry => entry.family, static entry => entry.palette));

    private static Fin<Appearance> Of(string name, Dictionary<Role, Dictionary<InteractionState, LookFile>> looks) =>
        toSeq(from role in looks from state in role.Value select (Key: (role.Key, state.Key), Look: state.Value))
            .Traverse(static entry => Decoded(entry.Look).Map(look => (entry.Key, look))).As()
            .Map(entries => new Appearance(name, toHashMap(entries)));

    private static Fin<Look> Decoded(LookFile look) =>
        look.Fill.Switch(
                solidFile: static solid => new Material.Solid(solid.Ink),
                gradientFile: static gradient => new Material.Gradient(toSeq(gradient.Stops), gradient.Angle),
                drawnFile: static drawn => Material.Drawn.Of(drawn.Shader, toHashMap(drawn.Inks.Select(static ink => (ink.Key, ink.Value)))),
                backdropFile: static backdrop => new Material.Backdrop(backdrop.Glass, backdrop.Fallback))
            .Map(fill => new Look(fill, Optional(look.Border), look.Radius, look.Elevation, look.Ink, look.Type, look.Motion, Optional(look.StateLayer)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true, Converters = [typeof(HexColor)])]
[JsonSerializable(typeof(AppearanceFile))]
[JsonSerializable(typeof(LookFile))]
[JsonSerializable(typeof(Swatch.Fixed), TypeInfoPropertyName = "FixedSwatch")]
[JsonSerializable(typeof(Ink.Fixed), TypeInfoPropertyName = "FixedInk")]
internal sealed partial class ShippedContext : JsonSerializerContext;
