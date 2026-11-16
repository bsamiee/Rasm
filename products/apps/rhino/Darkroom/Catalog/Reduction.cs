using Darkroom.Chain;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
internal sealed partial class KuwaharaKind {
    public static readonly KuwaharaKind Classic = new("classic", static pixels => Math.Floor(pixels), static (_, _, radius, frame, _) => Kuwahara.Quadrants(frame, radius));
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", static pixels => pixels, static (state, extent, radius, frame, progress) =>
        Kuwahara.Sectors(frame, radius, state.Uniformity.Pixels(extent) / 3d, state.Sharpness, state.Eccentricity, progress));

    [UseDelegateFromConstructor]
    public partial double Radius(double pixels);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Painterly state, PixelExtent extent, double radius, PixelFrame frame, Option<IProgress<int>> progress);
}

internal sealed record Posterize(int Levels, float RetainHue, float DitherStrength) : IPixelStage<Posterize> {
    public static Posterize Default { get; } = new(8 + 1, 0f, 0f);

    public Option<ShortSideLength> DitherCell { get; init; }

    public static Fin<Option<PixelPass>> Pass(Posterize state, PassContext context) =>
        Quantizer.Validated(state.Levels)
            .Bind(levels => Dither.Posterized(levels, state.DitherStrength, state.RetainHue, state.DitherCell.Map(cell => (float)cell.Pixels(context.Extent)).IfNone(1f)))
            .Map(static pass => Some(pass));
}

internal sealed record PaletteReduction(int Colors, PaletteDither Dithering, float DitherStrength) : IPixelStage<PaletteReduction> {
    public static PaletteReduction Default { get; } = new(QuantizerConstants.MaxColors, PaletteDither.FloydSteinberg, 1f);

    public static Fin<Option<PixelPass>> Pass(PaletteReduction state, PassContext context) =>
        Some(state.Dithering.Reduced(state.Colors, state.DitherStrength));
}

internal sealed record PaletteMapping(Seq<Unicolour> Palette, PaletteDither Dithering, float DitherStrength) : IPixelStage<PaletteMapping> {
    public static PaletteMapping Default { get; } = new(
        [.. Seq((0d, 0d, 0d), (1d, 0d, 0d), (0d, 1d, 0d), (0d, 0d, 1d), (1d, 1d, 0d), (1d, 0d, 1d), (0d, 1d, 1d), (1d, 1d, 1d)).Map(static corner => new Unicolour(ColourSpace.Rgb, corner))],
        PaletteDither.FloydSteinberg, 1f);

    public static Fin<Option<PixelPass>> Pass(PaletteMapping state, PassContext context) =>
        Some(state.Dithering.Mapped(RampLight.Lights(state.Palette, context), state.DitherStrength));
}

internal sealed record Painterly(KuwaharaKind Kind, ShortSideLength Size, ShortSideLength Uniformity, float Sharpness, float Eccentricity) : IPixelStage<Painterly> {
    public static Painterly Default { get; } = new(KuwaharaKind.Anisotropic, ShortSideLength.Create(6f / ReferenceFrame.Height), ShortSideLength.Create(4f / ReferenceFrame.Height), 1f, 1f);

    public static Fin<Option<PixelPass>> Pass(Painterly state, PassContext context) =>
        state.Kind.Radius(state.Size.Pixels(context.Extent)) switch {
            0d => None,
            var radius => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kind.Run(state, context.Extent, radius, frame, progress))),
        };
}
