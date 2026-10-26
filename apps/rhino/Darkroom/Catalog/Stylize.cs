using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text.Json.Serialization;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using Darkroom.Chain;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Color;
using Rasm.Imaging.Filtering;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Warping;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using TinyEXR;
using Wacton.Unicolour;

namespace Darkroom.Catalog;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PaletteDither {
    [Display(Name = "Bayer 2 × 2")] public static readonly PaletteDither Bayer2 = new("bayer-2", KnownDitherings.Bayer2x2);
    [Display(Name = "Ordered 3 × 3")] public static readonly PaletteDither Ordered3 = new("ordered-3", KnownDitherings.Ordered3x3);
    [Display(Name = "Bayer 4 × 4")] public static readonly PaletteDither Bayer4 = new("bayer-4", KnownDitherings.Bayer4x4);
    [Display(Name = "Bayer 8 × 8")] public static readonly PaletteDither Bayer8 = new("bayer-8", KnownDitherings.Bayer8x8);
    [Display(Name = "Bayer 16 × 16")] public static readonly PaletteDither Bayer16 = new("bayer-16", KnownDitherings.Bayer16x16);
    [Display(Name = "Floyd-Steinberg")] public static readonly PaletteDither FloydSteinberg = new("floyd-steinberg", KnownDitherings.FloydSteinberg);
    public static readonly PaletteDither Atkinson = new("atkinson", KnownDitherings.Atkinson);
    public static readonly PaletteDither Burkes = new("burkes", KnownDitherings.Burks);
    [Display(Name = "Jarvis-Judice-Ninke")] public static readonly PaletteDither JarvisJudiceNinke = new("jarvis-judice-ninke", KnownDitherings.JarvisJudiceNinke);
    public static readonly PaletteDither Stucki = new("stucki", KnownDitherings.Stucki);
    public static readonly PaletteDither Sierra2 = new("sierra-2", KnownDitherings.Sierra2);
    public static readonly PaletteDither Sierra3 = new("sierra-3", KnownDitherings.Sierra3);
    public static readonly PaletteDither SierraLite = new("sierra-lite", KnownDitherings.SierraLite);
    [Display(Name = "Stevenson-Arce")] public static readonly PaletteDither StevensonArce = new("stevenson-arce", KnownDitherings.StevensonArce);

    public IDither Kernel { get; }

    public QuantizerOptions Options(int colors, float strength) =>
        new() { MaxColors = colors, Dither = strength is 0f ? null : Kernel, DitherScale = strength, ColorMatchingMode = ColorMatchingMode.Exact };
}

[SmartEnum<string>]
public sealed partial class VideoStandard {
    [Display(Name = "Rec.601")] public static readonly VideoStandard Rec601 = new("rec601", new Gamut(RgbConfiguration.Ntsc));
    [Display(Name = "Rec.709")] public static readonly VideoStandard Rec709 = new("rec709", Gamut.StandardRgb);

    public Gamut Signal { get; }
}

[SmartEnum<string>]
public sealed partial class EdgeRendering {
    [Display(Name = "Response")] public static readonly EdgeRendering Magnitude = new("magnitude", static (_, _, frame, edges, progress) => EdgeResponse.Magnitudes(frame, edges, progress));
    public static readonly EdgeRendering Lines = new("lines", static (state, context, frame, edges, progress) => state.Stroked(context, frame, edges, state.Line, 0f, progress));
    public static readonly EdgeRendering LinesOnFill = new("lines-on-fill", static (state, context, frame, edges, progress) => state.Stroked(context, frame, edges, state.Fill, 1f, progress));

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Render(EdgeDetect state, PassContext context, PixelFrame frame, Mat edges, IProgress<int> progress);
}

[SmartEnum<string>]
public sealed partial class LineClass {
    public static readonly LineClass Outer = new("outer", static outline => outline.Outer, static outline => outline.Placement);
    public static readonly LineClass Inner = new("inner", static outline => outline.Inner, static _ => StrokePlacement.Center);

    [UseDelegateFromConstructor]
    public partial ContourStroke Stroke(GeometryOutline state);

    [UseDelegateFromConstructor]
    public partial StrokePlacement Placement(GeometryOutline state);
}

[SmartEnum<string>]
public sealed partial class OutlineSource {
    public static readonly OutlineSource Silhouette = new("silhouette", GuideChannel.Depth, static outline => outline.Silhouette,
        static (state, context) => context.Camera.Map(camera => GuideEdges.Silhouette(camera.Frustum, state.Step)));
    public static readonly OutlineSource Crease = new("crease", GuideChannel.Normal, static outline => outline.Crease, static (state, _) => Some(GuideEdges.Crease(state.CreaseAngle)));
    public static readonly OutlineSource Object = new("object", GuideChannel.ObjectId, static outline => outline.Object, static (_, _) => Some<ContourMarks>(GuideEdges.Identity));
    public static readonly OutlineSource Material = new("material", GuideChannel.MaterialId, static outline => outline.Material, static (_, _) => Some<ContourMarks>(GuideEdges.Identity));

    public GuideChannel Channel { get; }

    [UseDelegateFromConstructor]
    public partial LineClass? Route(GeometryOutline state);

    [UseDelegateFromConstructor]
    public partial Option<ContourMarks> Marks(GeometryOutline state, PassContext context);
}

[SmartEnum<string>]
public sealed partial class KuwaharaKind {
    public static readonly KuwaharaKind Classic = new("classic", static pixels => Math.Floor(pixels), static (_, _, radius, frame, progress) => Kuwahara.Quadrants(frame, radius, progress));
    public static readonly KuwaharaKind Anisotropic = new("anisotropic", static pixels => pixels, static (state, extent, radius, frame, progress) =>
        Kuwahara.Sectors(frame, radius, state.Uniformity.Pixels(extent), state.Sharpness, state.Eccentricity, progress));

    [UseDelegateFromConstructor]
    public partial double Radius(double pixels);

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Painterly state, PixelExtent extent, double radius, PixelFrame frame, IProgress<int> progress);
}

[SmartEnum<string>]
public sealed partial class Separation {
    [Display(Name = "Single ink")]
    public static readonly Separation Mono = new("mono", static (rgb, weights) => new Vector4(Vector3.Zero, 1f - (float)double.Clamp(LuminanceSamples.Luminance(rgb.X, rgb.Y, rgb.Z, weights), 0d, 1d)));
    [Display(Name = "CMYK")]
    public static readonly Separation Process = new("process", static (rgb, _) => Vector3.Clamp(rgb, Vector3.Zero, Vector3.One) switch {
        var saturated => float.Max(saturated.X, float.Max(saturated.Y, saturated.Z)) switch {
            > 0f and var most => new Vector4(Vector3.One - (saturated / most), 1f - most),
            _ => Vector4.UnitW,
        },
    });

    [UseDelegateFromConstructor]
    public partial Vector4 Levels(Vector3 rgb, (double R, double G, double B) weights);
}

[SmartEnum<string>]
public sealed partial class MosaicCell {
    public static readonly MosaicCell Square = new("square", static (state, context, frame, progress) => frame.Filter(header => {
        double cell = state.Size.Pixels(context.Extent);
        CellGrid grid = CellGrid.Fit(context.Extent, cell, cell);
        using Mat source = header.Clone();
        Func<(long Column, long Row), Vector3> colors = memoUnsafe(grid.Means(frame, source));
        return new PixelPass.Pointwise((row, column, line) => {
            long down = grid.Row(context.Extent.Height - 1L - line, context.Extent.Height).Cell;
            for (int x = 0; x < row.Length; x++)
                row[x] = new Vector4(colors((grid.Column(column + x, context.Extent.Width).Cell, down)), row[x].W);
        }).Run(frame, progress);
    }));
    public static readonly MosaicCell Cellular = new("cellular", static (state, context, frame, progress) => {
        PixelFrame copy = frame.Copy();
        (double centerX, double centerY, double pitch) = (context.Extent.Width / 2d, context.Extent.Height / 2d, state.Size.Pixels(context.Extent));
        uint field = state.Seed.Field(4u, state.Hold.Period(state.Timing.At(context)));
        return new PixelPass.Pointwise((row, column, line) => {
            ReadOnlySpan2D<Vector4> source = copy.View.Span;
            for (int x = 0; x < row.Length; x++) {
                (double X, double Y) = NoiseDimensions.PlanarF1(
                    ((((2L * (column + x)) + 1L) / 2d) - centerX) / pitch, ((((2L * (context.Extent.Height - line)) - 1L) / 2d) - centerY) / pitch, state.Cells, field).Site;
                Vector4 color = source.Sample(
                    (double.Floor((X * pitch) + centerX) + 0.5d, double.Floor((Y * pitch) + centerY) + 0.5d),
                    (WrapMode.Clamp, WrapMode.Clamp), Vector4.Zero, static (sum, pixel, weight) => sum + (pixel * (float)weight), Some((frame.Origin, frame.Extent)));
                row[x] = new Vector4(color.AsVector3(), row[x].W);
            }
        }).Run(frame, progress);
    });

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Run(Mosaic state, PassContext context, PixelFrame frame, IProgress<int> progress);
}

[SmartEnum<string>]
public sealed partial class GlyphColoring {
    [Display(Name = "Image")] public static readonly GlyphColoring Input = new("input", static (_, mean, _, _) => mean);
    [Display(Name = "Text color")] public static readonly GlyphColoring Text = new("text", static (state, _, _, _) => Presets.Shown(state.Text));
    [Display(Name = "Random per cell")] public static readonly GlyphColoring PerCell = new("per-cell", static (state, _, gray, tint) => Vector3.Lerp(new Vector3(gray), tint, state.Saturation));

    [UseDelegateFromConstructor]
    public partial Vector3 Tint(GlyphCells state, Vector3 mean, float gray, Vector3 tint);
}

public sealed record EdgeDetect(
    [property: Display(Name = "Type", Description = "Kernel measuring local change")] EdgeOperator Operator,
    [property: Display(Name = "Channel", Description = "Color or one channel the kernel reads")] EdgeSource Source,
    [property: Display(Name = "Softening", Description = "Blur before kernel in frame short sides")][property: Range(0d, 0.01d)] ShortSideLength Softening,
    [property: Display(Name = "Output", Description = "Response, lines, or lines on fill")] EdgeRendering Rendering,
    [property: Display(Name = "Threshold", Description = "Kernel step response share a line pixel exceeds")][property: Range(0d, 1d)] float Threshold,
    [property: Display(Name = "Line width", Description = "Stroke width in frame short sides")][property: Range(0d, 0.01d)] ShortSideLength Width,
    [property: Display(Name = "Line color", Description = "Drawn line color")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Line,
    [property: Display(Name = "Fill color", Description = "Color under lines on fill")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Fill,
    [property: Display(Name = "Blend mode", Description = "Drawn layer blend over frame")] BlendingMode Mode) : IPixelStage<EdgeDetect> {
    public static EdgeDetect Default { get; } = new(
        EdgeOperator.Kirsch, EdgeSource.Luma, ShortSideLength.Zero, EdgeRendering.LinesOnFill, 0.1f, ShortSideLength.Create(1f / ReferenceFrame.Height),
        new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), BlendingMode.Mix);

    public static Option<PixelPass> Pass(EdgeDetect state, PassContext context) =>
        state.Rendering == EdgeRendering.Lines && state.Width == ShortSideLength.Zero ? None
            : Some<PixelPass>(new PixelPass.Frame((frame, progress) => EdgeResponse.Detected(frame, state.Operator, state.Source, 3d * state.Softening.Pixels(context.Extent), context.Working,
                edges => state.Rendering.Render(state, context, frame, edges, progress))));

    public Fin<Unit> Stroked(PassContext context, PixelFrame frame, Mat edges, Unicolour ground, float floor, IProgress<int> progress) =>
        EdgeResponse.Lines(frame, edges, Threshold * Operator.Gain, Width.Pixels(context.Extent), Presets.Shown(Line), Presets.Shown(ground), floor, Mode, context.Working, progress);
}

public sealed record ContourStroke(
    [property: Display(Name = "Width", Description = "Line width in frame short sides, capping world width")][property: Range(0d, 0.01d)] ShortSideLength Width,
    [property: Display(Name = "World width", Description = "Surface line width in meters, narrowing with distance")][property: Range(0d, 0.0254d)] double? World,
    [property: Display(Name = "Color", Description = "Line color")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Color) {
    public static ContourStroke Default { get; } = new(ShortSideLength.Create(3f / ReferenceFrame.Height), World: null, new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d));
}

public sealed record GeometryOutline(
    [property: Display(Name = "Silhouettes", Description = "Lines at depth steps where one surface occludes another")] LineClass? Silhouette,
    [property: Display(Name = "Creases", Description = "Lines where neighboring normals turn past the crease angle")] LineClass? Crease,
    [property: Display(Name = "Object edges", Description = "Lines where object IDs change")] LineClass? Object,
    [property: Display(Name = "Material edges", Description = "Lines where material IDs change")] LineClass? Material,
    [property: Display(Name = "Depth threshold", Description = "Silhouette depth curvature, 1 minus near over far depth across flat regions")][property: Range(0d, 0.2d)] float Step,
    [property: Display(Name = "Crease angle", Description = "Turn between neighboring normals marking a crease")][property: Range(0d, double.Pi / 2d)] float CreaseAngle,
    [property: Display(Name = "Outer lines", Description = "Width and color of outer lines")] ContourStroke Outer,
    [property: Display(Name = "Placement", Description = "Contour side outer lines draw on")] StrokePlacement Placement,
    [property: Display(Name = "Inner lines", Description = "Width and color of inner lines")] ContourStroke Inner,
    [property: Display(Name = "Taper near", Description = "Near depth in meters divided by contour depth places width on ramp, off keeping full width")][property: Range(0d, 304.8d)] double? TaperNear,
    [property: Display(Name = "Taper ramp", Description = "Line width scale from distant contours at left to near contours at right")] ImmutableArray<RampStop> TaperRamp,
    [property: Display(Name = "Paper", Description = "Color under lines, off drawing over image")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour? Paper,
    [property: Display(Name = "Blend mode", Description = "Drawn layer blend over frame")] BlendingMode Mode) : IPixelStage<GeometryOutline> {
    public static GeometryOutline Default { get; } = new(
        LineClass.Outer, LineClass.Inner, LineClass.Outer, Material: null, 0.05f, float.DegreesToRadians(180f - 134.4f), ContourStroke.Default, StrokePlacement.Center,
        ContourStroke.Default with { Width = ShortSideLength.Create(1.5f / ReferenceFrame.Height) }, TaperNear: null, Presets.Grayscale, Paper: null, BlendingMode.Mix);

    public static Seq<GuideChannel> Channels(GeometryOutline state) =>
        GuideChannel.Depth.Cons(toSeq(OutlineSource.Items).Filter(source => source.Route(state) is not null).Map(static source => source.Channel)).Distinct();

    public static Option<PixelPass> Pass(GeometryOutline state, PassContext context) =>
        from depth in context.Guides.Find(GuideChannel.Depth)
        let routes = toSeq(OutlineSource.Items).Choose(source => Optional(source.Route(state)).Map(line => (Source: source, Line: line)))
        where !routes.IsEmpty || state.Paper is not null
        let taper = RampLight.Of(state.TaperRamp, context.Working)
        let lined = routes.ToLookup(static route => route.Line)
        select (PixelPass)new PixelPass.Frame((frame, progress) =>
            from lines in toSeq(LineClass.Items).Traverse(line => line.Stroke(state) switch {
                var stroke => Contours.Width(stroke.Width.Pixels(context.Extent), Optional(stroke.World), context.Camera, context.Extent,
                    Optional(state.TaperNear).Map(near => (near, (Func<float, float>)(at => taper.Sample(at) switch { var scale => float.Max(scale.X, float.Max(scale.Y, scale.Z)) })))).Map(width =>
                    new ContourLine(toSeq(lined[line]).Map(route =>
                            from guide in context.Guides.Find(route.Source.Channel)
                            from marks in route.Source.Marks(state, context)
                            select (marks, guide)).Somes(),
                        line.Placement(state), width, Presets.Shown(stroke.Color))),
            }).As()
            from drawn in Contours.Outlined(frame, depth, lines, Optional(state.Paper).Map(static paper => new Vector4(Presets.Shown(paper), (float)paper.Alpha.A)), state.Mode, context.Working, progress)
            select drawn);
}

public sealed record Painterly(
    [property: Display(Name = "Type", Description = "Square quadrants or stroke-following sectors")] KuwaharaKind Kind,
    [property: Display(Name = "Size", Description = "Filter radius in frame short sides")][property: Range(0d, 0.02d)] ShortSideLength Size,
    [property: Display(Name = "Uniformity", Description = "Stroke direction smoothing radius in frame short sides")][property: Range(0d, 0.01d)] ShortSideLength Uniformity,
    [property: Display(Name = "Sharpness", Description = "Weight toward the least varied sector")][property: Range(0d, 1d)] float Sharpness,
    [property: Display(Name = "Eccentricity", Description = "Sector elongation along strokes")][property: Range(0d, 2d)] float Eccentricity) : IPixelStage<Painterly> {
    public static Painterly Default { get; } = new(KuwaharaKind.Anisotropic, ShortSideLength.Create(6f / ReferenceFrame.Height), ShortSideLength.Create(4f / ReferenceFrame.Height), 1f, 1f);

    public static Option<PixelPass> Pass(Painterly state, PassContext context) =>
        state.Kind.Radius(state.Size.Pixels(context.Extent)) switch {
            0d => None,
            var radius => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Kind.Run(state, context.Extent, radius, frame, progress))),
        };
}

public sealed record Posterize(
    [property: Display(Name = "Levels", Description = "Even output levels per channel from black to white")][property: Range(2, 256)] int Levels,
    [property: Display(Name = "Retain hue", Description = "Share of input hue kept at rounded saturation and luma")][property: Range(0d, 1d)] float RetainHue,
    [property: Display(Name = "Dither amount", Description = "Dither offset, 0% rounding to nearest level or color")][property: Range(0d, 1d)] float DitherStrength,
    [property: Display(Name = "Dither cell", Description = "Ordered dither cell in frame short sides, off drawing one cell per pixel")][property: Range(0.001d, 0.25d)] ShortSideExtent? DitherCell)
    : IPixelStage<Posterize> {
    public static Posterize Default { get; } = new(8 + 1, 0f, 0f, DitherCell: null);

    public static Option<PixelPass> Pass(Posterize state, PassContext context) =>
        Some(Reduction.Posterized(state.Levels, state.DitherStrength, state.RetainHue,
                Optional(state.DitherCell).Map(cell => cell.Pixels(context.Extent)).IfNone(1d), context.Extent.Height).Match<PixelPass>(
            Succ: static rows => new PixelPass.Pointwise(rows),
            Fail: static error => new PixelPass.Frame((_, _) => error)));
}

public sealed record PaletteReduction(
    [property: Display(Name = "Colors", Description = "Palette size the frame's colors split into by variance")][property: Range(QuantizerConstants.MinColors, QuantizerConstants.MaxColors)] int Colors,
    [property: Display(Name = "Dither", Description = "Ordered matrix or error diffusion kernel spreading rounding error")] PaletteDither Dithering,
    [property: Display(Name = "Dither amount", Description = "Dither offset, 0% rounding to nearest level or color")][property: Range(0d, 1d)] float DitherStrength) : IPixelStage<PaletteReduction> {
    public static PaletteReduction Default { get; } = new(QuantizerConstants.MaxColors, PaletteDither.FloydSteinberg, 1f);

    public static Option<PixelPass> Pass(PaletteReduction state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => Reduction.Quantized(frame, new WuQuantizer(state.Dithering.Options(state.Colors, state.DitherStrength)), progress)));
}

public sealed record PaletteMapping(
    [property: Display(Name = "Palette", Description = "Colors each pixel maps to by nearest match")] ImmutableArray<RampStop> Palette,
    [property: Display(Name = "Dither", Description = "Ordered matrix or error diffusion kernel spreading rounding error")] PaletteDither Dithering,
    [property: Display(Name = "Dither amount", Description = "Dither offset, 0% rounding to nearest level or color")][property: Range(0d, 1d)] float DitherStrength) : IPixelStage<PaletteMapping> {
    public static PaletteMapping Default { get; } = new(
        [.. Seq((0d, 0d, 0d), (1d, 0d, 0d), (0d, 1d, 0d), (0d, 0d, 1d), (1d, 1d, 0d), (1d, 0d, 1d), (0d, 1d, 1d), (1d, 1d, 1d)).Map(static (corner, index) => new RampStop(new Unicolour(ColourSpace.Rgb, corner), index / 7f))],
        PaletteDither.FloydSteinberg, 1f);

    public static Option<PixelPass> Pass(PaletteMapping state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) => Reduction.Quantized(frame, new PaletteQuantizer(
            state.Palette.Select(static stop => Color.FromScaledVector(new Vector4(Presets.Shown(stop.Color), (float)stop.Color.Alpha.A))).ToArray(),
            state.Dithering.Options(QuantizerConstants.MaxColors, state.DitherStrength)), progress)));
}

public sealed record InkScreen(
    [property: Display(Name = "Angle", Description = "Counterclockwise turn of ink screen")][property: Range(-double.Pi, double.Pi)] float Angle,
    [property: Display(Name = "Phase U", Description = "Ink screen shift along its first axis in cells")][property: Range(0d, 1d)] float U,
    [property: Display(Name = "Phase V", Description = "Ink screen shift along its second axis in cells")][property: Range(0d, 1d)] float V) {
    private const float ChannelScale = 4f;

    public static InkScreen Default { get; } = new(float.Pi / 4f, 0f, 0f);
    public static InkScreen Cyan { get; } = Default with { U = 0.193f * ChannelScale, V = 0.052f * ChannelScale };
    public static InkScreen Magenta { get; } = Default with { U = 0.052f * ChannelScale, V = 0.193f * ChannelScale };
    public static InkScreen Yellow { get; } = Default with { U = 0f, V = 0.2f * ChannelScale };
    public static InkScreen Key { get; } = Default with { U = 0.1414f * ChannelScale, V = 0.1414f * ChannelScale };

    public HalftoneScreen Screen(double pitch, ScreenShape shape, float turn, float onset) =>
        new(Matrix3x2.CreateRotation(Angle + turn) * Matrix3x2.CreateTranslation(U, V), pitch, shape, onset);
}

public sealed record Halftone(
    [property: Display(Name = "Shape", Description = "Screen cell spot, inked area matching cell tone")] ScreenShape Shape,
    [property: Display(Name = "Cell size", Description = "Screen cell height in frame short sides")][property: Range(1d / (2d * 64d) * ReferenceFrame.GreaterSide, 0.25d)] ShortSideExtent Size,
    [property: Display(Name = "Separation", Description = "One ink screened on luma, or cyan, magenta, yellow, and key screens")] Separation Separation,
    [property: Display(Name = "Cyan", Description = "Ink screen angle and phase")] InkScreen Cyan,
    [property: Display(Name = "Magenta", Description = "Ink screen angle and phase")] InkScreen Magenta,
    [property: Display(Name = "Yellow", Description = "Ink screen angle and phase")] InkScreen Yellow,
    [property: Display(Name = "Key", Description = "Ink screen angle and phase")] InkScreen Key,
    [property: Display(Name = "Crosshatch", Description = "Ink level where a perpendicular screen joins, 100% drawing none")][property: Range(0d, 1d)] float CrossOnset,
    [property: Display(Name = "Ink", Description = "Color the key screen prints")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Ink,
    [property: Display(Name = "Paper", Description = "Color under inks, off printing over image")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour? Paper,
    [property: Display(Name = "Blend mode", Description = "Drawn layer blend over frame")] BlendingMode Mode) : IPixelStage<Halftone> {
    public static Halftone Default { get; } = new(
        ScreenShape.Round, ShortSideExtent.Create(1f / (2f * 2f * 4f) * ReferenceFrame.GreaterSide), Separation.Mono, InkScreen.Cyan, InkScreen.Magenta, InkScreen.Yellow, InkScreen.Key,
        1f, new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d), new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), BlendingMode.Mix);

    public static Option<PixelPass> Pass(Halftone state, PassContext context) => Blend.Mixed(new PixelPass.Pointwise(Print(state, context)), state.Mode, 1f, [], context.Working);

    private static Action<Span<Vector4>, long, long> Print(Halftone state, PassContext context) {
        (double pitch, PixelExtent extent, (double R, double G, double B) weights) = (state.Size.Pixels(context.Extent), context.Extent, context.Working.Luminance);
        Arr<(Arr<HalftoneScreen> Screens, Vector3 Absorbance, Vector4 Channel)> plates = [
            Plate(state.Cyan, Vector3.UnitX, Vector4.UnitX), Plate(state.Magenta, Vector3.UnitY, Vector4.UnitY), Plate(state.Yellow, Vector3.UnitZ, Vector4.UnitZ),
            Plate(state.Key, Vector3.One - Presets.Shown(state.Ink), Vector4.UnitW)];
        Option<Vector3> paper = Optional(state.Paper).Map(Presets.Shown);
        return (row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                ((double X, double Y) point, Vector4 levels) = ((column + x + 0.5d - (extent.Width / 2d), extent.Height - line - 0.5d - (extent.Height / 2d)), state.Separation.Levels(row[x].AsVector3(), weights));
                Vector3 transmitted = Vector3.One;
                foreach ((Arr<HalftoneScreen> screens, Vector3 absorbance, Vector4 channel) in plates.AsSpan()) {
                    float level = Vector4.Dot(levels, channel);
                    float clear = level > 0f ? screens.Fold(1f, (held, screen) => held * (1f - screen.Coverage(point, level))) : 1f;
                    transmitted *= Vector3.One - ((1f - clear) * absorbance);
                }
                row[x] = new Vector4(paper.IfNone(row[x].AsVector3()) * transmitted, row[x].W);
            }
        };

        (Arr<HalftoneScreen> Screens, Vector3 Absorbance, Vector4 Channel) Plate(InkScreen ink, Vector3 absorbance, Vector4 channel) =>
            (state.CrossOnset < 1f ? [ink.Screen(pitch, state.Shape, 0f, 0f), ink.Screen(pitch, state.Shape, float.Pi / 2f, state.CrossOnset)] : [ink.Screen(pitch, state.Shape, 0f, 0f)],
                absorbance, channel);
    }
}

public sealed record Mosaic(
    [property: Display(Name = "Cells", Description = "Square blocks of mean color, or irregular cells of site color")] MosaicCell Cell,
    [property: Display(Name = "Cell size", Description = "Cell height in frame short sides")][property: Range(0.001d, 0.1d * ReferenceFrame.GreaterSide)] ShortSideExtent Size,
    [property: Display(Name = "Cells", Description = "Cell distance and site placement")] VoronoiCells Cells,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold) : IPixelStage<Mosaic> {
    public static Mosaic Default { get; } = new(
        MosaicCell.Square, ShortSideExtent.Create(0.1f * 0.05f * ReferenceFrame.GreaterSide), NoiseBasis.F1.Default.Cells, Seed.MinValue, Timing.Default with { Pace = 0f }, Hold.MinValue);

    public static Option<PixelPass> Pass(Mosaic state, PassContext context) => Some<PixelPass>(new PixelPass.Frame((frame, progress) => state.Cell.Run(state, context, frame, progress)));
}

public sealed record GlyphCells(
    [property: Display(Name = "Characters", Description = "Cell characters ranked by ink coverage")] CharacterSet Characters,
    [property: Display(Name = "Font", Description = "Character stroke font")] HersheyFonts Font,
    [property: Display(Name = "Cell size", Description = "Character cell height in frame short sides")][property: Range(1d / 128d, 0.25d)] ShortSideExtent Size,
    [property: Display(Name = "Coloring", Description = "Color each character draws in")] GlyphColoring Coloring,
    [property: Display(Name = "Text color", Description = "Character color when coloring is Text color")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Text,
    [property: Display(Name = "Background", Description = "Color behind every character")][property: JsonConverter(typeof(UnicolourConverter))] Unicolour Background,
    [property: Display(Name = "Saturation", Description = "Saturation of random cell colors")][property: Range(0d, 1d)] float Saturation,
    [property: Display(Name = "Jitter", Description = "Random offset range on cell tones before character choice")][property: Range(0d, 1d)] float Jitter,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "Blend mode", Description = "Drawn layer blend over frame")] BlendingMode Mode) : IPixelStage<GlyphCells> {
    public static GlyphCells Default { get; } = new(
        CharacterSet.Create(" .:-=+*#%@"), HersheyFonts.Simplex, ShortSideExtent.Create(1f / 24f), GlyphColoring.Input, new Unicolour(ColourSpace.Rgb, 1d, 1d, 1d), new Unicolour(ColourSpace.Rgb, 0d, 0d, 0d),
        1f, 0.1f, Seed.MinValue, Timing.Default, Hold.Create(6f / 24f), BlendingMode.Mix);

    public static GlyphCells Binary { get; } = Default with { Characters = CharacterSet.Create(" 01") };
    public static GlyphCells Numbers { get; } = Default with { Characters = CharacterSet.Create(" 0123456789") };
    public static GlyphCells Arithmetic { get; } = Default with { Characters = CharacterSet.Create(" +-*/=<>^%") };
    public static GlyphCells Latin { get; } = Default with { Characters = CharacterSet.Create(" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz") };
    public static GlyphCells Mixed { get; } = Default with { Characters = CharacterSet.Create(" .:-=+*#%@01xX") };

    public static Seq<(string Name, Func<GlyphCells, GlyphCells> Look)> Looks { get; } = [
        (nameof(Binary), static _ => Binary), (nameof(Numbers), static _ => Numbers), (nameof(Arithmetic), static _ => Arithmetic), (nameof(Latin), static _ => Latin), (nameof(Mixed), static _ => Mixed)];

    public static Option<PixelPass> Pass(GlyphCells state, PassContext context) => Blend.Mixed(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress)), state.Mode, 1f, [], context.Working);

    private static Fin<Unit> Kernel(GlyphCells state, PassContext context, PixelFrame frame, IProgress<int> progress) => frame.Filter(header => {
        GlyphAtlas atlas = new(state.Characters, state.Font, state.Size.Pixels(context.Extent), context.Extent);
        using Mat source = header.Clone();
        Func<(long Column, long Row), Vector3> mean = atlas.Grid.Means(frame, source);
        uint field = state.Seed.Field(5u, state.Hold.Period(state.Timing.At(context)));
        (uint gray, uint tint, (double R, double G, double B) weights) = (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), context.Working.Luminance);
        Func<(long Column, long Row), (int Glyph, Vector3 Color)> cells = memoUnsafe<(long Column, long Row), (int Glyph, Vector3 Color)>(cell => {
            Vector3 color = mean(cell);
            (Vector4 u, Vector4 c) = (CoordinateHash.Uniforms(cell.Column, cell.Row, gray), CoordinateHash.Uniforms(cell.Column, cell.Row, tint));
            float level = (float)double.Clamp(double.Clamp(LuminanceSamples.Luminance(color.X, color.Y, color.Z, weights), 0d, 1d) + (state.Jitter * (u.X - 0.5d)), 0d, 1d);
            return (atlas.Glyph(level), state.Coloring.Tint(state, color, u.Y, c.AsVector3()));
        });
        (int width, int height, Vector3 background) = (context.Extent.Width, context.Extent.Height, Presets.Shown(state.Background));
        return new PixelPass.Pointwise((row, column, line) => {
            (long down, int across) = atlas.Grid.Row(height - 1L - line, height);
            int maskRow = (int)((long)across * atlas.Tile.Height / height);
            for (int x = 0; x < row.Length; x++) {
                (long right, int inside) = atlas.Grid.Column(column + x, width);
                (int glyph, Vector3 color) = cells((right, down));
                row[x] = new Vector4(Vector3.Lerp(background, color, atlas.Coverage(glyph, maskRow, (int)((long)inside * atlas.Tile.Width / width))), row[x].W);
            }
        }).Run(frame, progress);
    });
}

public sealed record PixelSort(
    [property: Display(Name = "Sort by", Description = "Value each span sorts on")] SortMetric Key,
    [property: Display(Name = "Lower bound", Description = "Key value where sorted spans start, above upper bound wrapping the band")][property: Range(0d, 1d)] float Low,
    [property: Display(Name = "Upper bound", Description = "Key value where sorted spans end")][property: Range(0d, 1d)] float High,
    [property: Display(Name = "Direction", Description = "Direction sorted values rise along")] SortDirection Direction) : IPixelStage<PixelSort> {
    public static PixelSort Default { get; } = new(SortMetric.Luma, 0f, 0.3f, SortDirection.Right);

    public static Option<PixelPass> Pass(PixelSort state, PassContext context) => Some(state.Direction.Sorted(state.Key, state.Low, state.High, context.Working));
}

public sealed record CrtDisplay(
    [property: Display(Name = "Mask", Description = "Phosphor layout behind the glass")] PhosphorMask Mask,
    [property: Display(Name = "Triad pitch", Description = "Phosphor triad width in frame short sides")][property: Range(0.002d, 0.02d)] ShortSideExtent Pitch,
    [property: Display(Name = "Mask strength", Description = "Light the mask moves onto each phosphor")][property: Range(0d, 1d)] float MaskStrength,
    [property: Display(Name = "Lines", Description = "Scanlines across the frame height")][property: Range(120, 1080)] int Lines,
    [property: Display(Name = "Interlaced", Description = "Lights alternate lines on alternate frames")] bool Interlaced,
    [property: Display(Name = "Scanline depth", Description = "Darkening between scanlines")][property: Range(0d, 1d)] float ScanlineDepth,
    [property: Display(Name = "Collapse", Description = "Raster shrink to line then dot on power loss")][property: Range(0d, 1d)] float Collapse,
    [property: Display(Name = "Glow", Description = "Beam light spread in frame short sides")][property: Range(0d, 0.005d)] ShortSideLength Glow,
    [property: Display(Name = "Curvature", Description = "Tube glass bulge and the warped frame scale filling edges or corners")] RadialDistortion Curvature,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<CrtDisplay> {
    public static CrtDisplay Default { get; } = new(
        PhosphorMask.SlotMask, ShortSideExtent.Create(8f / ReferenceFrame.Height), 0.8f, 480, Interlaced: false, 1f, 0f, ShortSideLength.Zero,
        RadialDistortion.Create(0d, 0d, LensFraming.Edges, LensDirection.Distort), Timing.Default);

    public static CrtDisplay CathodeRayTube { get; } = Default with {
        Mask = PhosphorMask.ApertureGrille,
        Pitch = ShortSideExtent.Create(ReferenceFrame.GreaterSide / 640f),
        MaskStrength = 0.705394f,
        Lines = 240,
        ScanlineDepth = 0.5f,
        Glow = ShortSideLength.Create(0.01f * ReferenceFrame.GreaterSide / 3f),
        Curvature = RadialDistortion.Create(0.0497677d, 0d, LensFraming.Edges, LensDirection.Distort),
    };

    public static CrtDisplay Scanlines { get; } = Default with { MaskStrength = 0f, ScanlineDepth = 0.4f };

    public static Seq<(string Name, Func<CrtDisplay, CrtDisplay> Look)> Looks { get; } = [(nameof(CathodeRayTube), static _ => CathodeRayTube), (nameof(Scanlines), static _ => Scanlines)];

    private bool Lit => MaskStrength is not 0f || ScanlineDepth is not 0f || Collapse is not 0f;

    private bool Warped => Curvature.Coefficient is not 0d;

    private (double Width, double Height) Scales =>
        (2d * Collapse, 1d / Lines) switch {
            var (collapse, line) => (double.Min(1d, double.Max(2d - collapse, line)), double.Max(1d - collapse, line)),
        };

    public static Option<PixelPass> Pass(CrtDisplay state, PassContext context) =>
        (state.Lit, state.Collapse is 0f && state.Glow == ShortSideLength.Zero && !state.Warped) switch {
            (false, true) => None,
            (true, true) => Some<PixelPass>(new PixelPass.Pointwise(Raster(state, context))),
            _ => Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
                from collapsed in state.Collapse is 0f ? unit : new CoordinateMap.Analytic(Deflection(state.Scales, context.Extent), Sampling.Linear, (WrapMode.Black, WrapMode.Black)).Apply(frame, progress)
                from glowed in state.Glow == ShortSideLength.Zero ? unit : VideoKernels.Glow(frame, state.Glow.Pixels(context.Extent))
                from rastered in state.Lit ? new PixelPass.Pointwise(Raster(state, context)).Run(frame, progress) : unit
                from warped in state.Warped ? state.Curvature.Map(WrapMode.Black).Apply(frame, progress) : unit
                select warped)),
        };

    private static Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Deflection((double Width, double Height) scale, PixelExtent extent) {
        (double X, double Y) = (extent.Width / 2d, extent.Height / 2d);
        return (points, _, _) => {
            foreach (ref (double X, double Y) point in points)
                point = (Sampling.Linear.Point(X + ((point.X - X) / scale.Width), WrapMode.Black, extent.Width), Sampling.Linear.Point(Y + ((point.Y - Y) / scale.Height), WrapMode.Black, extent.Height));
        };
    }

    private static Action<Span<Vector4>, long, long> Raster(CrtDisplay state, PassContext context) =>
        VideoKernels.Raster(context.Extent, state.Lines, state.Interlaced, SystemM.Frame(state.Timing.At(context)), state.Scales, state.Mask,
            state.Pitch.Pixels(context.Extent), state.MaskStrength, state.ScanlineDepth);
}

public sealed record AnalogVideo(
    [property: Display(Name = "Matrix", Description = "Video standard for Y, Cb, and Cr weights")] VideoStandard Matrix,
    [property: Display(Name = "Lines", Description = "Raster lines across the frame height")][property: Range(120, 1080)] int Lines,
    [property: Display(Name = "Luma blur", Description = "Luma band limit along each line in frame short sides")][property: Range(0d, 0.005d)] ShortSideLength LumaBandwidth,
    [property: Display(Name = "Chroma blur", Description = "Chroma band limit along each line in frame short sides")][property: Range(0d, 0.005d)] ShortSideLength ChromaBandwidth,
    [property: Display(Name = "Chroma vertical blur", Description = "Chroma blur across lines in frame short sides")][property: Range(0d, 0.005d)] ShortSideLength ChromaVertical,
    [property: Display(Name = "Chroma sampling", Description = "Chroma sample spacing along each line in frame short sides")][property: Range(0d, 0.01d)] ShortSideLength ChromaPitch,
    [property: Display(Name = "Chroma delay", Description = "Shift of both color difference signals against luma")][property: Range(-0.02d, 0.02d)] float ChromaDelay,
    [property: Display(Name = "Smear", Description = "Luma trail length right of each edge")][property: Range(0d, 0.005d)] ShortSideLength Smear,
    [property: Display(Name = "Smear amount", Description = "Trail share mixed into luma")][property: Range(0d, 1d)] float SmearMix,
    [property: Display(Name = "Ringing", Description = "Overshoot beside edges at luma band scale")][property: Range(0d, 2d)] float Ringing,
    [property: Display(Name = "Ringing delay", Description = "Signed shift of ringing from image edges")][property: Range(-0.02d, 0.02d)] float RingingDelay,
    [property: Display(Name = "Emboss", Description = "Relief from band-limited luma slope")][property: Range(0d, 2d)] float Emboss,
    [property: Display(Name = "Ghost", Description = "Strength of delayed signal copy")][property: Range(0d, 1d)] float Ghost,
    [property: Display(Name = "Ghost delay", Description = "Delayed copy shift")][property: Range(-0.05d, 0.05d)] float GhostDelay,
    [property: Display(Name = "Cross color", Description = "Rainbow fringes fine luma detail leaks into chroma")][property: Range(0d, 1d)] float CrossColor,
    [property: Display(Name = "Luma noise", Description = "Gaussian noise deviation in luma before band limit")][property: Range(0d, 0.2d)] float LumaNoise,
    [property: Display(Name = "Chroma noise", Description = "Independent Gaussian noise deviation in each chroma signal before band limit")][property: Range(0d, 0.2d)] float ChromaNoise,
    [property: Display(Name = "Noise length", Description = "Gaussian noise correlation along each line in frame short sides")][property: Range(0.001d, 0.5d)] ShortSideExtent NoiseLength,
    [property: Display(Name = "Snow", Description = "Brightness of snow streaks")][property: Range(0d, 1d)] float Snow,
    [property: Display(Name = "Snow density", Description = "Share of line cells lit as snow")][property: Range(0d, 1d)] float SnowDensity,
    [property: Display(Name = "Snow length", Description = "Snow cell fading head length in frame short sides")][property: Range(0.001d, 0.5d)] ShortSideExtent SnowLength,
    [property: Display(Name = "Interlace", Description = "Shift of odd lines against even lines")][property: Range(-0.01d, 0.01d)] float Interlace,
    [property: Display(Name = "Line jitter", Description = "Standard deviation of Gaussian sideways line shifts")][property: Range(0d, 0.01d)] ShortSideLength Jitter,
    [property: Display(Name = "Jitter period", Description = "Height over which line shifts change")][property: Range(0.001d, 0.5d)] ShortSideExtent JitterPeriod,
    [property: Display(Name = "Wrinkle", Description = "Sideways shift inside rolling wrinkle band")][property: Range(-0.05d, 0.05d)] float Wrinkle,
    [property: Display(Name = "Wrinkle height", Description = "Wrinkle band half height")][property: Range(0.001d, 0.5d)] ShortSideExtent WrinkleHeight,
    [property: Display(Name = "Wrinkle rate", Description = "Upward wrinkle band passes per second")][property: Range(-1d, 1d)] float WrinkleRate,
    [property: Display(Name = "Head switching", Description = "Bottom band skew where tape heads switch")][property: Range(-0.1d, 0.1d)] float HeadSwitch,
    [property: Display(Name = "Head switching height", Description = "Skewed band height")][property: Range(0.001d, 0.1d)] ShortSideExtent HeadSwitchHeight,
    [property: Display(Name = "Bounce", Description = "Largest vertical picture jump")][property: Range(0d, 0.02d)] ShortSideLength Bounce,
    [property: Display(Name = "Roll rate", Description = "Frame heights the picture rolls up per second")][property: Range(-1d, 1d)] float RollRate,
    [property: Display(Name = "Hum", Description = "Hum bar luma darkening, chroma kept")][property: Range(0d, 1d)] float Hum,
    [property: Display(Name = "Hum rate", Description = "Upward hum bar passes per second")][property: Range(-1d, 1d)] float HumRate,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<AnalogVideo> {
    public static AnalogVideo Default { get; } = new(
        VideoStandard.Rec601, 480,
        ShortSideLength.Zero, ShortSideLength.Zero, ShortSideLength.Zero, ShortSideLength.Zero, 0f,
        ShortSideLength.Zero, 0.3f * 0.25f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, ShortSideExtent.Create(ReferenceFrame.GreaterSide / 40f), 0f, 0f, ShortSideExtent.Create(0.04f * ReferenceFrame.GreaterSide),
        0f, ShortSideLength.Zero, ShortSideExtent.Create(1f / 65f), 0f, ShortSideExtent.Create(0.15f), 0f,
        0f, ShortSideExtent.Create(0.31f / (32f - (28.8f * 0.5f))), ShortSideLength.Zero, 0f, 0f, 60f / 1001f,
        Seed.MinValue, Timing.Default);

    public static AnalogVideo ChromaBlur { get; } =
        Default with { ChromaBandwidth = ShortSideLength.Create(128f / ReferenceFrame.Height / 3f), ChromaVertical = ShortSideLength.Create(12.8f / ReferenceFrame.Height / 3f) };

    public static Seq<(string Name, Func<AnalogVideo, AnalogVideo> Look)> Looks { get; } = [(nameof(ChromaBlur), static _ => ChromaBlur)];

    private bool Moves =>
        Interlace is not 0f || Jitter != ShortSideLength.Zero || Wrinkle is not 0f
        || HeadSwitch is not 0f || Bounce != ShortSideLength.Zero || RollRate is not 0f;

    private bool Speckled => (Snow is not 0f && SnowDensity is not 0f) || Hum is not 0f;

    private bool Filters(PixelExtent extent) =>
        LumaBandwidth != ShortSideLength.Zero || ChromaBandwidth != ShortSideLength.Zero || ChromaVertical != ShortSideLength.Zero || ChromaPitch.Pixels(extent) > 1d
        || ChromaDelay is not 0f || (Smear != ShortSideLength.Zero && SmearMix is not 0f) || Ghost is not 0f || CrossColor is not 0f
        || LumaNoise is not 0f || ChromaNoise is not 0f;

    public static Option<PixelPass> Pass(AnalogVideo state, PassContext context) =>
        state.Moves || state.Filters(context.Extent) || state.Speckled ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))) : None;

    private static Fin<Unit> Kernel(AnalogVideo state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        (PixelExtent extent, float now) = (context.Extent, state.Timing.At(context));
        uint field = state.Seed.Field(2u, BitConverter.SingleToUInt32Bits(now));
        return (state.Moves || state.Filters(extent)
                ? frame.Filter(header =>
                    from synced in state.Moves ? Sync(state, extent, now, field, frame, progress) : unit
                    from signaled in state.Filters(extent) ? Signal(state, extent, now, field, frame, header, progress) : unit
                    select signaled)
                : unit)
            .Bind(_ => state.Speckled
                ? new PixelPass.Pointwise(VideoKernels.Snow(extent, state.Lines, state.SnowLength.Pixels(extent), state.Snow, state.SnowDensity, state.Hum,
                    ((double)state.HumRate * now) switch { var turns => turns - double.Floor(turns) }, state.Matrix.Signal, CoordinateHash.Branch(field, 1u))).Run(frame, progress)
                : unit);
    }

    private static Fin<Unit> Sync(AnalogVideo state, PixelExtent extent, float now, uint field, PixelFrame frame, IProgress<int> progress) {
        double sigma = state.JitterPeriod.Pixels(extent) * state.Lines / extent.Height;
        uint jitterDraws = CoordinateHash.Branch(field, 2u);
        double rise = ((double)state.RollRate * now) switch { var turns => (turns - double.Floor(turns)) * extent.Height };
        double jump = state.Bounce.Pixels(extent) * ((2d * CoordinateHash.White(Vector256<double>.Zero, CoordinateHash.Branch(field, 3u)).X) - 1d);
        double crest = ((double)state.WrinkleRate * now) switch { var turns => (1d - (turns - double.Floor(turns))) * extent.Height };
        (double interlace, double jitter, double wrinkle, double wrinkleBand) =
            ((double)state.Interlace * extent.ShortSide, state.Jitter.Pixels(extent), (double)state.Wrinkle * extent.ShortSide, state.WrinkleHeight.Pixels(extent));
        (double switching, double switchBand) = ((double)state.HeadSwitch * extent.ShortSide, state.HeadSwitchHeight.Pixels(extent));
        return (state.Jitter == ShortSideLength.Zero ? Gaussian.Kernel(0d) : VideoKernels.Unit(sigma)).Map(static taps => {
            using (taps) {
                double[] weights = new double[taps.Rows];
                taps.CopyTo(weights);
                return weights;
            }
        }).Bind(weights => new CoordinateMap.Composed(
            new CoordinateMap.Composed(
                new CoordinateMap.Analytic(
                    (points, _, _) => {
                        foreach (ref (double X, double Y) point in points)
                            point.Y = Sampling.Linear.Point(point.Y + rise, WrapMode.Periodic, extent.Height);
                    }, Sampling.Linear, (WrapMode.Periodic, WrapMode.Periodic)),
                new CoordinateMap.Analytic(
                    (points, _, _) => {
                        foreach (ref (double X, double Y) point in points)
                            point.Y = Sampling.Linear.Point(point.Y - jump, WrapMode.Black, extent.Height);
                    }, Sampling.Linear, (WrapMode.Black, WrapMode.Black))),
            new CoordinateMap.Analytic(
                (points, _, _) => {
                    const double crestShare = 0.9d;
                    long line = SystemM.Line((long)double.Floor(points[0].Y), state.Lines, extent.Height);
                    double draw = 0d;
                    for (int tap = 0; tap < weights.Length; tap++)
                        draw += weights[tap] * CoordinateHash.Normals(0L, line + tap - (weights.Length / 2), jitterDraws).X;
                    foreach (ref (double X, double Y) point in points) {
                        double distance = point.Y - crest;
                        distance -= double.Floor((distance / extent.Height) + 0.5d) * extent.Height;
                        double band = (extent.Height - point.Y) / switchBand;
                        double head = band is >= 0d and < 1d ? switching * (band < crestShare ? band / crestShare : (1d - band) / (1d - crestShare)) : 0d;
                        double offset = (long.IsEvenInteger(line) ? 0d : interlace) + (jitter * draw) + (wrinkle * double.Max(0d, 1d - (double.Abs(distance) / wrinkleBand))) + head;
                        point.X = Sampling.Linear.Point(point.X - offset, WrapMode.Clamp, extent.Width);
                    }
                }, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp))).Apply(frame, progress));
    }

    private static Fin<Unit> Signal(AnalogVideo state, PixelExtent extent, float now, uint field, PixelFrame frame, Mat header, IProgress<int> progress) =>
        VideoKernels.Decomposed(header, state.Matrix.Signal, (luma, blue, red) =>
            (state.CrossColor is 0f ? unit : VideoKernels.CrossColor(luma, blue, red, frame, state.Lines, SystemM.Frame(now), state.CrossColor))
                .Bind(_ => state is { LumaNoise: 0f, ChromaNoise: 0f } ? unit
                    : VideoKernels.LineNoise(luma, blue, red, frame, state.Lines, state.NoiseLength.Pixels(extent), state.LumaNoise, state.ChromaNoise, CoordinateHash.Branch(field, 0u)))
                .Bind(_ => Luma(state, extent, luma))
                .Bind(_ => Chroma(state, extent, blue))
                .Bind(_ => Chroma(state, extent, red)))
            .Map(_ => {
                progress.Report(frame.Size.Height);
                return unit;
            });

    private static Fin<Unit> Luma(AnalogVideo state, PixelExtent extent, Mat luma) {
        (double sigma, double smear) = (state.LumaBandwidth.Pixels(extent), state.Smear.Pixels(extent));
        (bool rings, bool embosses) = (sigma > 0d && state.Ringing is not 0f, sigma > 0d && state.Emboss is not 0f);
        using Mat ring = new();
        using Mat relief = new();
        return (sigma > 0d ? VideoKernels.Filter(luma, Gaussian.Kernel(sigma)) : unit)
            .Bind(_ => {
                if (rings)
                    CvInvoke.Sobel(luma, ring, DepthType.Cv64F, 2, 0, 1, 1d, 0d, BorderType.Replicate);
                if (embosses)
                    CvInvoke.Sobel(luma, relief, DepthType.Cv64F, 1, 0, 1, 1d, 0d, BorderType.Replicate);
                return rings ? VideoKernels.Filter(ring, VideoKernels.Echo(0d, (double)state.RingingDelay * extent.ShortSide, 1f)) : unit;
            })
            .Bind(_ => smear > 0d && state.SmearMix is not 0f ? VideoKernels.Filter(luma, VideoKernels.Echo(smear, smear, state.SmearMix)) : unit)
            .Bind(_ => {
                if (rings)
                    CvInvoke.ScaleAdd(ring, -state.Ringing * sigma * sigma, luma, luma);
                if (embosses)
                    CvInvoke.ScaleAdd(relief, -state.Emboss * sigma / 2d, luma, luma);
                return state.Ghost is 0f ? unit : VideoKernels.Filter(luma, VideoKernels.Echo(0d, (double)state.GhostDelay * extent.ShortSide, state.Ghost));
            });
    }

    private static Fin<Unit> Chroma(AnalogVideo state, PixelExtent extent, Mat plane) {
        double pitch = state.ChromaPitch.Pixels(extent);
        if (pitch > 1d) {
            int samples = int.Max(1, (int)Math.Round(plane.Cols / pitch, MidpointRounding.ToEven));
            using Mat signal = new();
            using SpanOwner<float> coarse = SpanOwner<float>.Allocate(samples * plane.Rows);
            plane.ConvertTo(signal, DepthType.Cv32F);
            ImageProcessing.Resize(signal.GetSpan<float>(), signal.Cols, signal.Rows, coarse.Span, samples, signal.Rows, 1, ResizeFilter.Box);
            ImageProcessing.Resize(coarse.Span, samples, signal.Rows, signal.GetSpan<float>(), signal.Cols, signal.Rows, 1, ResizeFilter.Triangle);
            signal.ConvertTo(plane, DepthType.Cv64F);
        }
        return (state.ChromaBandwidth == ShortSideLength.Zero ? unit : VideoKernels.Filter(plane, Gaussian.Kernel(state.ChromaBandwidth.Pixels(extent))))
            .Bind(_ => state.ChromaVertical == ShortSideLength.Zero ? unit : VideoKernels.Filter(plane, Gaussian.Kernel(state.ChromaVertical.Pixels(extent)), vertical: true))
            .Bind(_ => state.ChromaDelay is 0f ? unit : VideoKernels.Filter(plane, VideoKernels.Echo(0d, (double)state.ChromaDelay * extent.ShortSide, 1f)))
            .Bind(_ => state.Ghost is 0f ? unit : VideoKernels.Filter(plane, VideoKernels.Echo(0d, (double)state.GhostDelay * extent.ShortSide, state.Ghost)));
    }
}

public sealed record CodecDamage(
    [property: Display(Name = "Quality", Description = "JPEG quality each generation encodes at")][property: Range(1, 100)] int Quality,
    [property: Display(Name = "Subsampling", Description = "Color model and chroma subsampling the encoder codes")] JpegColorType ColorType,
    [property: Display(Name = "Generations", Description = "Encode and decode cycles, each recompressing the last")][property: Range(1, 8)] int Generations,
    [property: Display(Name = "Coded lines", Description = "Coded picture height, off using frame height")][property: Range(120, 1080)] int? Coded) : IPixelStage<CodecDamage> {
    public static CodecDamage Default { get; } = new(75, JpegColorType.YCbCrRatio420, 1, Coded: null);

    public static CodecDamage Jpeg { get; } = Default with { Quality = 60, Coded = 360 };

    public static Seq<(string Name, Func<CodecDamage, CodecDamage> Look)> Looks { get; } = [(nameof(Jpeg), static _ => Jpeg)];

    public static Option<PixelPass> Pass(CodecDamage state, PassContext context) =>
        Some<PixelPass>(new PixelPass.Frame((frame, progress) =>
            VideoKernels.Recompressed(frame, new JpegEncoder { Quality = state.Quality, ColorType = state.ColorType }, state.Generations, Optional(state.Coded), progress)));
}

public sealed record Glitch(
    [property: Display(Name = "Block width", Description = "Block width in frame short sides")][property: Range(0.01d, 4d)] ShortSideExtent BlockWidth,
    [property: Display(Name = "Block height", Description = "Block height in frame short sides")][property: Range(0.01d, 4d)] ShortSideExtent BlockHeight,
    [property: Display(Name = "Block density", Description = "Share of blocks selected for displacement or corruption")][property: Range(0d, 1d)] float BlockDensity,
    [property: Display(Name = "Block shift", Description = "Largest sideways block displacement in frame short sides")][property: Range(0d, 0.5d)] ShortSideLength BlockShift,
    [property: Display(Name = "Block lift", Description = "Largest vertical block displacement in frame short sides")][property: Range(0d, 0.1d)] ShortSideLength BlockLift,
    [property: Display(Name = "Tear", Description = "Torn band sideways shift range in frame short sides")][property: Range(0d, 0.1d)] ShortSideLength Tear,
    [property: Display(Name = "Tear period", Description = "Height of each torn band in frame short sides")][property: Range(0.01d, 1d)] ShortSideExtent TearPeriod,
    [property: Display(Name = "RGB split", Description = "Opposed red and blue offset in frame short sides")][property: Range(-0.05d, 0.05d)] float Split,
    [property: Display(Name = "Split angle", Description = "Counterclockwise from right")][property: Range(-double.Pi, double.Pi)] float SplitDirection,
    [property: Display(Name = "Split jitter", Description = "Mix of fixed and random signed split per held period")][property: Range(0d, 1d)] float SplitJitter,
    [property: Display(Name = "Slide", Description = "Share of the split each moved channel smears across")][property: Range(0d, 1d)] float Slide,
    [property: Display(Name = "Matrix", Description = "Video standard for Y, Cb, and Cr weights")] VideoStandard Matrix,
    [property: Display(Name = "Luma corruption", Description = "Mix toward luma raised to a random power in selected blocks")][property: Range(0d, 1d)] float LumaCorruption,
    [property: Display(Name = "Chroma corruption", Description = "Mix toward chroma raised to a random power in selected blocks")][property: Range(0d, 1d)] float ChromaCorruption,
    [property: Display(Name = "Edges", Description = "Fill of pixels a displacement uncovers")] WrapMode Wrap,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold) : IPixelStage<Glitch> {
    private static readonly GaussLegendreRule Smear = new(0d, 1d, 1 << 6);

    public static Glitch Default { get; } = new(
        ShortSideExtent.Create(1f / (5f * 0.1f)), ShortSideExtent.Create(1f / 5f), 0f, ShortSideLength.Zero, ShortSideLength.Zero,
        ShortSideLength.Zero, ShortSideExtent.Create(MathF.Tau / 20f), 0f, 0f, 0f, 0f, VideoStandard.Rec601, 0f, 0f, WrapMode.Periodic, Seed.MinValue, Timing.Default, Hold.MinValue);

    public static Glitch Corruption { get; } = Default with {
        BlockWidth = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide),
        BlockHeight = ShortSideExtent.Create(0.03f * ReferenceFrame.GreaterSide),
        BlockDensity = 0.2f,
        BlockShift = ShortSideLength.Create(0.004f * ReferenceFrame.GreaterSide),
        ChromaCorruption = 0.6f,
        Hold = Hold.Create(12f / 24f),
    };

    public static Glitch BlockGlitch { get; } = Default with {
        BlockDensity = 0.2f,
        BlockShift = ShortSideLength.Create(0.05f * ReferenceFrame.GreaterSide),
        Split = 0.03f * ReferenceFrame.GreaterSide,
        Hold = Hold.Create(2f / 24f),
    };

    public static Seq<(string Name, Func<Glitch, Glitch> Look)> Looks { get; } = [(nameof(Corruption), static _ => Corruption), (nameof(BlockGlitch), static _ => BlockGlitch)];

    private bool Blocks => BlockDensity is not 0f && (BlockShift != ShortSideLength.Zero || BlockLift != ShortSideLength.Zero);

    private bool Moves => Blocks || Tear != ShortSideLength.Zero;

    private bool Corrupts => BlockDensity is not 0f && (LumaCorruption is not 0f || ChromaCorruption is not 0f);

    public static Option<PixelPass> Pass(Glitch state, PassContext context) =>
        state.Moves || state.Split is not 0f || state.Corrupts ? Some<PixelPass>(new PixelPass.Frame((frame, progress) => Kernel(state, context, frame, progress))) : None;

    private static Fin<Unit> Kernel(Glitch state, PassContext context, PixelFrame frame, IProgress<int> progress) {
        PixelExtent extent = context.Extent;
        uint field = state.Seed.Field(3u, state.Hold.Period(state.Timing.At(context)));
        Vector4 draw = CoordinateHash.White(Vector256<double>.Zero, CoordinateHash.Branch(field, 3u));
        CoordinateMap blocks = new CoordinateMap.Analytic(Displacement(state, extent, field, draw.X), Sampling.Nearest, (state.Wrap, state.Wrap));
        return (state.Split is not 0f, state.Moves) switch {
            (true, _) => Splits(state, extent, draw.Y, blocks, frame, progress),
            (false, true) => blocks.Apply(frame, progress),
            (false, false) => unit,
        } switch {
            var moved => moved.Bind(_ => state.Corrupts
                ? new PixelPass.Pointwise(VideoKernels.Corrupted(state.Matrix.Signal, state.LumaCorruption, state.ChromaCorruption, extent,
                    point => state.Cell(extent, point, CoordinateHash.Branch(field, 1u)) switch { (true, var site, _) => Some(site), _ => None }, CoordinateHash.Branch(field, 2u))).Run(frame, progress)
                : unit),
        };
    }

    private static Action<Span<(double X, double Y)>, Span<float>, PixelExtent> Displacement(Glitch state, PixelExtent extent, uint field, float phase) {
        (double centerX, double centerY) = (extent.Width / 2d, extent.Height / 2d);
        uint draws = CoordinateHash.Branch(field, 1u);
        (double shift, double lift, double tear, double period) = (state.BlockShift.Pixels(extent), state.BlockLift.Pixels(extent), state.Tear.Pixels(extent), state.TearPeriod.Pixels(extent));
        return (points, _, _) => {
            foreach (ref (double X, double Y) point in points) {
                (double X, double Y) q = (point.X - centerX, point.Y - centerY);
                (double across, double down) = state.Blocks && state.Cell(extent, q, draws) is (true, _, var cell) ? (shift * ((2d * cell.Y) - 1d), lift * ((2d * cell.Z) - 1d)) : (0d, 0d);
                double band = ((q.Y / period) + phase) switch { var turns => turns - double.Floor(turns) - 0.5d };
                point = (Sampling.Nearest.Point(point.X - across - (tear * band), state.Wrap, extent.Width), Sampling.Nearest.Point(point.Y - down, state.Wrap, extent.Height));
            }
        };
    }

    private static Fin<Unit> Splits(Glitch state, PixelExtent extent, float draw, CoordinateMap blocks, PixelFrame frame, IProgress<int> progress) {
        (double jitter, double slide) = (state.SplitJitter, state.Slide);
        double reach = (double)state.Split * extent.ShortSide * (1d - jitter + (jitter * ((2d * draw) - 1d)));
        (double X, double Y) = double.SinCos(state.SplitDirection) switch { var (sin, cos) => (cos, -sin) };
        return reach is 0d ? state.Moves ? blocks.Apply(frame, progress) : unit : CoordinateMap.Integrated(
            Smear,
            tau => Subframe(reach * (1d - slide + (slide * tau)), Vector4.UnitX),
            tau => Subframe(-reach * (1d - slide + (slide * tau)), Vector4.UnitZ),
            _ => Subframe(0d, new Vector4(0f, 1f, 0f, 1f)))(frame, progress);

        (CoordinateMap Map, Vector4 Gains) Subframe(double shift, Vector4 gains) {
            (double X, double Y) offset = (shift * X, shift * Y);
            CoordinateMap moved = new CoordinateMap.Analytic(
                (points, _, _) => {
                    foreach (ref (double X, double Y) point in points)
                        point = (Sampling.Linear.Point(point.X - offset.X, state.Wrap, extent.Width), Sampling.Linear.Point(point.Y - offset.Y, state.Wrap, extent.Height));
                }, Sampling.Linear, (state.Wrap, state.Wrap));
            return (state.Moves ? new CoordinateMap.Composed(blocks, moved) : moved, gains);
        }
    }

    private (bool Active, (long X, long Y) Site, Vector4 Draw) Cell(PixelExtent extent, (double X, double Y) q, uint draws) {
        (long X, long Y) site = ((long)double.Floor(q.X / BlockWidth.Pixels(extent)), (long)double.Floor(q.Y / BlockHeight.Pixels(extent)));
        Vector4 draw = CoordinateHash.Uniforms(site.X, site.Y, draws);
        return (draw.X < BlockDensity, site, draw);
    }
}

public sealed record DamageMark(
    [property: Display(Name = "Likelihood", Description = "Held periods showing marks")][property: Range(0d, 1d)] float Likelihood,
    [property: Display(Name = "Hold", Description = "Seconds between pattern redraws")] Hold Hold,
    [property: Display(Name = "White", Description = "Share of marks printing white, rest black")][property: Range(0d, 1d)] float White) {
    public static DamageMark Default { get; } = new(0f, Hold.Create(6f / 24f), 0f);
}

public sealed record FilmSpots(
    [property: Display(Name = "Timing and polarity", Description = "Mark timing and print tone")] DamageMark Mark,
    [property: Display(Name = "Cells", Description = "Mark spacing, density, size, variation, spread, and softness")] PointCells Cells) {
    public static FilmSpots Default { get; } = new(DamageMark.Default, new(ShortSideExtent.Create(0.02f * ReferenceFrame.GreaterSide), 0.02f, 2f * 0.2f, 1f, 0f, 0.3f));
}

public sealed record FilmDamage(
    [property: Display(Name = "Specks", Description = "Own timing, polarity, and point field")] FilmSpots Specks,
    [property: Display(Name = "Dirt", Description = "Own timing, polarity, and point field")] FilmSpots Dirt,
    [property: Display(Name = "Blotches", Description = "Own timing, polarity, and point field")] FilmSpots Blotches,
    [property: Display(Name = "Scratches", Description = "Mark timing and print tone")] DamageMark Scratches,
    [property: Display(Name = "Scratch count", Description = "Scratch lines across the frame width")][property: Range(1d, 20d)] float ScratchCount,
    [property: Display(Name = "Scratch width", Description = "Scratch stroke width in frame short sides")][property: Range(0d, 0.005d * ReferenceFrame.GreaterSide)] ShortSideLength ScratchWidth,
    [property: Display(Name = "Scratch rate", Description = "Scratch field evolution per second")][property: Range(0d, 24d)] float ScratchRate,
    [property: Display(Name = "Hairs", Description = "Mark timing and print tone")] DamageMark Hairs,
    [property: Display(Name = "Hair count", Description = "Mean hair strands per frame, fractions allowed")][property: Range(0d, 10d)] float HairCount,
    [property: Display(Name = "Hair width", Description = "Hair stroke width in frame short sides")][property: Range(0d, 0.005d * ReferenceFrame.GreaterSide)] ShortSideLength HairWidth,
    [property: Display(Name = "Hair length", Description = "Hair cell size in frame short sides")][property: Range(0.01d * ReferenceFrame.GreaterSide, 0.3d * ReferenceFrame.GreaterSide)] ShortSideExtent HairLength,
    [property: Display(Name = "Hair curl", Description = "Hair bend in hair cell sizes")][property: Range(0d, 4d)] float HairCurl,
    [property: Display(Name = "Hair rate", Description = "Hair field evolution per second")][property: Range(0d, 24d)] float HairRate,
    [property: Display(Name = "Weave X", Description = "Horizontal gate weave in frame short sides")][property: Range(0d, 0.01d)] ShortSideLength WeaveX,
    [property: Display(Name = "Weave Y", Description = "Vertical gate weave in frame short sides")][property: Range(0d, 0.01d)] ShortSideLength WeaveY,
    [property: Display(Name = "Weave rate", Description = "Gate weave noise cells per second")][property: Range(0.1d, 20d)] float WeaveRate,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<FilmDamage> {
    private static readonly Progress<int> Unreported = new();

    public static FilmDamage Default { get; } = new(
        FilmSpots.Default with { Mark = DamageMark.Default with { White = 0.5f } }, FilmSpots.Default, FilmSpots.Default,
        DamageMark.Default with { White = 0.7f }, 3f, ShortSideLength.Create(0.0008f * ReferenceFrame.GreaterSide), 0.05f * 24f,
        DamageMark.Default, 1f, ShortSideLength.Create(0.0012f * ReferenceFrame.GreaterSide), ShortSideExtent.Create(0.08f * ReferenceFrame.GreaterSide), 1f,
        0.1f * 24f, ShortSideLength.Zero, ShortSideLength.Zero, 5f * 0.05f * 0.5f * 24f, Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmDamage state, PassContext context) {
        (float clock, PixelExtent extent) = (state.Timing.At(context), context.Extent);
        long denominator = 2L * extent.ShortSide;
        Seq<(DamageMark Mark, bool Held, Func<uint, Func<long, long, (float Coverage, float Value)>> Field)> marks = [
            .. Seq(state.Specks, state.Dirt, state.Blotches).Map(spots => (spots.Mark, true, new Func<uint, Func<long, long, (float Coverage, float Value)>>(field => (column, line) =>
                spots.Cells.Disc((((2L * column) + 1 - extent.Width) / (double)denominator, ((2L * line) + 1 - extent.Height) / (double)denominator), extent.ShortSide, field,
                    tone => tone < spots.Mark.White ? 1f : 0f)))),
            (state.Scratches, false, field => Scratch(state, extent, clock, field)),
            (state.Hairs, false, field => Hair(state, extent, clock, field))];
        Func<long, long, (float Coverage, float Value)>[] shown = [.. marks
            .Map((mark, index) => (Mark: mark, Period: mark.Mark.Hold.Period(clock), Index: (uint)index))
            .Filter(drawn => CoordinateHash.White(Vector256<double>.Zero, Shot(drawn.Period, drawn.Index)).X < drawn.Mark.Mark.Likelihood)
            .Map(drawn => drawn.Mark.Field(Shot(drawn.Mark.Held ? drawn.Period : 0u, drawn.Index)))];
        Option<PixelPass> composite = shown is [] ? None : Some<PixelPass>(Composite(shown, extent));
        return (state.WeaveX, state.WeaveY) == (ShortSideLength.Zero, ShortSideLength.Zero) ? composite : Weave(state, extent, clock, (uint)marks.Count) switch {
            var map => Some<PixelPass>(new PixelPass.Frame((frame, progress) => composite.Map(pass => pass.Run(frame, Unreported)).IfNone(unit).Bind(_ => map.Apply(frame, progress)))),
        };

        uint Shot(uint term, uint index) => CoordinateHash.Branch(state.Seed.Field(6u, term), index);
    }

    private static PixelPass.Pointwise Composite(Func<long, long, (float Coverage, float Value)>[] marks, PixelExtent extent) => new((row, column, line) => {
        foreach (Func<long, long, (float Coverage, float Value)> field in marks) {
            for (int x = 0; x < row.Length; x++) {
                (float coverage, float value) = field(column + x, (long)extent.Height - 1 - line);
                row[x] = coverage switch {
                    0f => row[x],
                    1f => new Vector4(new Vector3(value), row[x].W),
                    _ => new Vector4((row[x].AsVector3() * (1f - coverage)) + new Vector3(value), row[x].W),
                };
            }
        }
    });

    private static CoordinateMap.Analytic Weave(FilmDamage state, PixelExtent extent, float clock, uint axes) {
        uint field = state.Seed.Field(6u, 0u);
        Vector256<double> term = Vector256.Create((double)clock * state.WeaveRate, 0d, 0d, 0d);
        (double X, double Y) = (
            state.WeaveX.Pixels(extent) * NoiseDimensions.One.Fbm(term, NoiseBasis.Fbm.Default.Octaves, CoordinateHash.Branch(field, axes)),
            state.WeaveY.Pixels(extent) * NoiseDimensions.One.Fbm(term, NoiseBasis.Fbm.Default.Octaves, CoordinateHash.Branch(field, axes + 1u)));
        return new((points, _, _) => { foreach (ref (double X, double Y) point in points) point = (point.X - X, point.Y - Y); }, Sampling.Linear, (WrapMode.Clamp, WrapMode.Clamp));
    }

    private static Func<long, long, (float Coverage, float Value)> Scratch(FilmDamage state, PixelExtent extent, float clock, uint field) {
        (double count, double drift) = (state.ScratchCount, (double)clock * state.ScratchRate);
        (double width, uint sites, uint tones) = (state.ScratchWidth.Pixels(extent), CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u));
        return (column, _) => NoiseDimensions.PlanarF1(((2L * column) + 1 - extent.Width) / (2d * extent.Width) * count, drift, NoiseBasis.F1.Default.Cells, sites) switch {
            var feature => ShapeEdge.Strip(double.Abs(feature.Delta.X) / count * extent.Width, width, 1d) switch {
                var coverage => (coverage, CoordinateHash.Uniforms(feature.Cell.X, feature.Cell.Y, tones).X < state.Scratches.White ? coverage : 0f),
            },
        };
    }

    private static Func<long, long, (float Coverage, float Value)> Hair(FilmDamage state, PixelExtent extent, float clock, uint field) {
        (double pixels, double width) = (state.HairLength.Pixels(extent), state.HairWidth.Pixels(extent));
        (double curl, double drift) = (state.HairCurl, (double)clock * state.HairRate);
        (double share, Octaves octaves) = (state.HairCount * pixels * pixels / ((double)extent.Width * extent.Height), NoiseBasis.Fbm.Default.Octaves);
        (uint across, uint down, uint sites, uint tones, uint strands) =
            (CoordinateHash.Branch(field, 0u), CoordinateHash.Branch(field, 1u), CoordinateHash.Branch(field, 2u), CoordinateHash.Branch(field, 3u), CoordinateHash.Branch(field, 4u));
        return (column, line) => {
            Vector256<double> strand = Vector256.Create(((2L * column) + 1 - extent.Width) / 2d / pixels, ((2L * line) + 1 - extent.Height) / 2d / pixels, 0d, 0d);
            Vector256<double> bent = Vector256.Create(strand[0] + (curl * NoiseDimensions.Two.Fbm(strand, octaves, across)), strand[1] + (curl * NoiseDimensions.Two.Fbm(strand, octaves, down)), drift, 0d);
            (float _, float edgeDistance, Vector256<long> cell) = NoiseDimensions.Three.Voronoi(bent, NoiseBasis.F1.Default.Cells, sites);
            Vector4 draw = CoordinateHash.Uniforms(CoordinateHash.Hash(cell, tones));
            float edge = ShapeEdge.Strip(edgeDistance * pixels, width, 1d);
            float open = ShapeEdge.Smooth(float.Clamp((0.5f + (0.5f * NoiseDimensions.Two.Fbm(strand, octaves, strands)) - 0.45f) / 0.1f, 0f, 1f));
            float coverage = draw.X < share ? edge * open : 0f;
            return (coverage, draw.Y < state.Hairs.White ? coverage : 0f);
        };
    }
}

public sealed record FilmFlicker(
    [property: Display(Name = "Depth", Description = "Peak flicker exposure swing in stops")][property: Range(0d, 1d)] float Depth,
    [property: Display(Name = "Rate", Description = "Flicker noise cells per second")][property: Range(1d, 60d)] float Rate,
    [property: Display(Name = "Seed", Description = "Number selecting pattern")] Seed Seed,
    [property: Display(Name = "Timing", Description = "Start time and pace over recorded frames")] Timing Timing) : IPixelStage<FilmFlicker> {
    public static FilmFlicker Default { get; } = new(0f, 5f * 0.5f * 24f, Seed.MinValue, Timing.Default);

    public static Option<PixelPass> Pass(FilmFlicker state, PassContext context) =>
        state.Depth is 0f ? None
            : new Vector3(float.Exp2(state.Depth * NoiseDimensions.One.Fbm(
                Vector256.Create((double)state.Timing.At(context) * state.Rate, 0d, 0d, 0d),
                NoiseBasis.Fbm.Default.Octaves, state.Seed.Field(7u, 0u)))) switch {
                    var gain => Some<PixelPass>(new PixelPass.Color(row => {
                        foreach (ref Vector4 pixel in row)
                            pixel = new Vector4(pixel.AsVector3() * gain, pixel.W);
                    })),
                };
}
