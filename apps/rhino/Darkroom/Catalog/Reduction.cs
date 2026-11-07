using Darkroom.Chain;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class PaletteDither {
    public static readonly PaletteDither Bayer2 = new("bayer-2", KnownDitherings.Bayer2x2);
    public static readonly PaletteDither Ordered3 = new("ordered-3", KnownDitherings.Ordered3x3);
    public static readonly PaletteDither Bayer4 = new("bayer-4", KnownDitherings.Bayer4x4);
    public static readonly PaletteDither Bayer8 = new("bayer-8", KnownDitherings.Bayer8x8);
    public static readonly PaletteDither Bayer16 = new("bayer-16", KnownDitherings.Bayer16x16);
    public static readonly PaletteDither FloydSteinberg = new("floyd-steinberg", KnownDitherings.FloydSteinberg);
    public static readonly PaletteDither Atkinson = new("atkinson", KnownDitherings.Atkinson);
    public static readonly PaletteDither Burkes = new("burkes", KnownDitherings.Burks);
    public static readonly PaletteDither JarvisJudiceNinke = new("jarvis-judice-ninke", KnownDitherings.JarvisJudiceNinke);
    public static readonly PaletteDither Stucki = new("stucki", KnownDitherings.Stucki);
    public static readonly PaletteDither Sierra2 = new("sierra-2", KnownDitherings.Sierra2);
    public static readonly PaletteDither Sierra3 = new("sierra-3", KnownDitherings.Sierra3);
    public static readonly PaletteDither SierraLite = new("sierra-lite", KnownDitherings.SierraLite);
    public static readonly PaletteDither StevensonArce = new("stevenson-arce", KnownDitherings.StevensonArce);

    public IDither Kernel { get; }

    public PixelPass Pass(Func<QuantizerOptions, IQuantizer> quantizer, int colors, float strength) =>
        new PixelPass.Frame((frame, progress) =>
            frame.Mutate(image => image.Mutate(context => context.Quantize(quantizer(
                    new QuantizerOptions { MaxColors = colors, Dither = strength is 0f ? null : Kernel, DitherScale = strength, ColorMatchingMode = ColorMatchingMode.Exact }))))
                .Map(_ => progress.Iter(report => report.Report(frame.Size.Height))));
}

[SmartEnum<string>]
public sealed partial class KuwaharaKind {
    public static readonly KuwaharaKind Classic = new("classic", static pixels => Math.Floor(pixels), static (_, _, radius, frame, _) => Kuwahara.Quadrants(frame, radius));
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", static pixels => pixels, static (state, extent, radius, frame, progress) =>
        Kuwahara.Sectors(frame, radius, state.Uniformity.Pixels(extent) / 3d, state.Sharpness, state.Eccentricity, progress));

    [UseDelegateFromConstructor]
    public partial double Radius(double pixels);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Painterly state, PixelExtent extent, double radius, PixelFrame frame, Option<IProgress<int>> progress);
}

public sealed record Posterize(int Levels, float RetainHue, float DitherStrength) : IPixelStage<Posterize> {
    public static Posterize Default { get; } = new(8 + 1, 0f, 0f);

    public Option<ShortSideLength> DitherCell { get; init; }

    public static Fin<Option<PixelPass>> Pass(Posterize state, PassContext context) =>
        Quantizer.Validated(state.Levels)
            .Bind(levels => Dither.Posterized(levels, state.DitherStrength, state.RetainHue, state.DitherCell.Map(cell => (float)cell.Pixels(context.Extent)).IfNone(1f)))
            .Map(static pass => Some(pass));
}

public sealed record PaletteReduction(int Colors, PaletteDither Dithering, float DitherStrength) : IPixelStage<PaletteReduction> {
    public static PaletteReduction Default { get; } = new(QuantizerConstants.MaxColors, PaletteDither.FloydSteinberg, 1f);

    public static Fin<Option<PixelPass>> Pass(PaletteReduction state, PassContext context) =>
        Some(state.Dithering.Pass(static options => new WuQuantizer(options), state.Colors, state.DitherStrength));
}

public sealed record PaletteMapping(Seq<Unicolour> Palette, PaletteDither Dithering, float DitherStrength) : IPixelStage<PaletteMapping> {
    public static PaletteMapping Default { get; } = new(
        [.. Seq((0d, 0d, 0d), (1d, 0d, 0d), (0d, 1d, 0d), (0d, 0d, 1d), (1d, 1d, 0d), (1d, 0d, 1d), (0d, 1d, 1d), (1d, 1d, 1d)).Map(static corner => new Unicolour(ColourSpace.Rgb, corner))],
        PaletteDither.FloydSteinberg, 1f);

    public static Fin<Option<PixelPass>> Pass(PaletteMapping state, PassContext context) =>
        RampLight.Lights(state.Palette, context).Select(static light => Color.FromScaledVector(light)).ToArray() switch {
            var palette => Some(state.Dithering.Pass(options => new PaletteQuantizer(palette, options), QuantizerConstants.MaxColors, state.DitherStrength)),
        };
}

public sealed record Painterly(KuwaharaKind Kind, ShortSideLength Size, ShortSideLength Uniformity, float Sharpness, float Eccentricity) : IPixelStage<Painterly> {
    public static Painterly Default { get; } = new(KuwaharaKind.Anisotropic, ShortSideLength.Create(6f / ReferenceFrame.Height), ShortSideLength.Create(4f / ReferenceFrame.Height), 1f, 1f);

    public static Fin<Option<PixelPass>> Pass(Painterly state, PassContext context) =>
        state.Kind.Radius(state.Size.Pixels(context.Extent)) switch {
            0d => None,
            var radius => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kind.Run(state, context.Extent, radius, frame, progress))),
        };
}
