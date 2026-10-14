using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Filters.Stylize;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.UI.Rows;
using Rhino.Render.PostEffects;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Darkroom.Effects;

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class StylizeText {
    public static readonly ParameterText DitherAmount = ParameterText.Of("Dither Amount", "Share of the dither offset, 0 rounding each pixel to its nearest level or color");
    public static readonly ParameterText Dithering = ParameterText.Choice<PaletteDither, InvalidStylize>(
        "Dither", "Ordered matrix or error diffusion kernel spreading each pixel's rounding error",
        static dither => dither.Map(
            bayer2: "Bayer 2 × 2", ordered3: "Ordered 3 × 3", bayer4: "Bayer 4 × 4", bayer8: "Bayer 8 × 8", bayer16: "Bayer 16 × 16",
            floydSteinberg: "Floyd-Steinberg", atkinson: "Atkinson", burkes: "Burkes", jarvisJudiceNinke: "Jarvis-Judice-Ninke", stucki: "Stucki",
            sierra2: "Sierra 2", sierra3: "Sierra 3", sierraLite: "Sierra Lite", stevensonArce: "Stevenson-Arce"));
    public static readonly ParameterText Matrix = ParameterText.Choice<YuvMatrix, InvalidStylize>(
        "Matrix", "Video standard whose luma weights split the picture into Y, Cb, and Cr", static matrix => matrix.Map(rec601: "Rec. 601", rec709: "Rec. 709"));
    public static readonly ParameterText Blending = ParameterText.Choice<BlendingMode, InvalidGrade>("Blend Mode", "How the drawn layer composites over the frame", ToneText.BlendText);
    public static ParameterText Lines(string caption, string help) =>
        ParameterText.Choice<LineClass, InvalidStylize>(caption, help, "Off", static line => line.Map(outer: "Outer", inner: "Inner"));

    public static ParameterText Screen(string ink) => ParameterText.Of(ink, "Angle and phase of this ink's screen") with {
        Parameters = ParameterText.Fields<InkScreen, InkScreenParameter, InvalidStylize>(parameter => parameter.Map(
            angle: ParameterText.Of($"{ink} Angle", "Turn of this ink's screen, counterclockwise"),
            u: ParameterText.Of($"{ink} Phase U", "Shift of this ink's screen along its first axis, in cells"),
            v: ParameterText.Of($"{ink} Phase V", "Shift of this ink's screen along its second axis, in cells"))),
    };

    public static ParameterText Mark(string caption) => ParameterText.Of(caption, "Show timing and polarity of these marks") with {
        Parameters = ParameterText.Fields<DamageMark, DamageMarkParameter, InvalidStylize>(parameter => parameter.Map(
            likelihood: ParameterText.Of($"{caption} Likelihood", "Share of held periods showing these marks"),
            hold: NoiseText.Hold,
            white: ParameterText.Of($"{caption} White", "Share of marks printing white, the remainder printing black"))),
    };

    public static ParameterText Spots(string caption) => ParameterText.Of(caption, "Independent timing, polarity, and point field of these marks") with {
        Parameters = ParameterText.Fields<FilmSpots, FilmSpotsParameter, InvalidStylize>(parameter => parameter.Map(
            mark: Mark(caption) with { Caption = "Timing and Polarity" },
            cells: ParameterText.Of($"{caption} Cells", "Spacing, density, size, variation, spread, and softness of these marks") with {
                Parameters = ParameterText.Fields<PointCells, PointCellsParameter, InvalidGenerator>(item => NoiseText.PointCells(item, $"{caption} ")),
            })),
    };
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("72443a3b-a5f1-4018-8368-7ad55d5f6127")]
[CustomPostEffect(PostEffectType.Early, Caption)]
public sealed class ProjectorFlicker : StageEffect<ProjectorFlicker, FilmFlicker, FilmFlickerParameter, InvalidStylize>, IEffectDefinition<FilmFlicker, FilmFlickerParameter> {
    public const string Caption = "Film Flicker";

    public static EffectRow<FilmFlicker, FilmFlickerParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            depth: ParameterText.Of("Depth", "Peak exposure swing of the flicker, in stops"),
            rate: ParameterText.Of("Rate", "Flicker noise cells per second"),
            seed: NoiseText.Seed,
            timing: NoiseText.Timing));
}

[Guid("3f5e6947-cf6e-4623-a607-5ddda6e05486")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class Pixelation : StageEffect<Pixelation, Mosaic, MosaicParameter, InvalidStylize>, IEffectDefinition<Mosaic, MosaicParameter> {
    public const string Caption = "Mosaic";

    public static EffectRow<Mosaic, MosaicParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            cell: ParameterText.Choice<MosaicCell, InvalidStylize>(
                "Cells", "Square blocks holding their mean color, or irregular cells holding the color at their site",
                static cell => cell.Map(square: "Square", cellular: "Cellular")),
            size: ParameterText.Of("Cell Size", "Cell height as a share of the frame's short side"),
            cells: ParameterText.Of("Cells", "Cell distance and site placement") with {
                Parameters = ParameterText.Fields<Cellular, CellularParameter, InvalidGenerator>(static item => NoiseText.Cellular(item)),
            },
            seed: NoiseText.Seed,
            timing: NoiseText.Timing,
            hold: NoiseText.Hold));
}

[Guid("6666a316-3ff8-49ce-a566-34d9531372ea")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class Posterization : StageEffect<Posterization, Posterize, PosterizeParameter, InvalidStylize>, IEffectDefinition<Posterize, PosterizeParameter> {
    public const string Caption = "Posterize";

    public static EffectRow<Posterize, PosterizeParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            levels: ParameterText.Of("Levels", "Output levels per channel, evenly spaced from black to white"),
            retainHue: ParameterText.Of("Retain Hue", "Share of the input's hue kept at the rounded saturation and luma"),
            ditherStrength: StylizeText.DitherAmount,
            ditherCell: ParameterText.Of("Dither Cell", "Ordered dither cell as a share of the frame's short side, off drawing one cell per pixel")));
}

[Guid("b0fd305b-80fb-4726-ab16-ca4223c7d61c")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class AdaptivePalette : StageEffect<AdaptivePalette, PaletteReduction, PaletteReductionParameter, InvalidStylize>, IEffectDefinition<PaletteReduction, PaletteReductionParameter> {
    public const string Caption = "Adaptive Palette";

    public static EffectRow<PaletteReduction, PaletteReductionParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            colors: ParameterText.Of("Colors", "Palette size the frame's colors split into by variance"),
            dithering: StylizeText.Dithering,
            ditherStrength: StylizeText.DitherAmount));
}

[Guid("fc33be5b-eb04-4bb7-b890-89e5778451c9")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class CustomPalette : StageEffect<CustomPalette, PaletteMapping, PaletteMappingParameter, InvalidStylize>, IEffectDefinition<PaletteMapping, PaletteMappingParameter> {
    public const string Caption = "Custom Palette";

    public static EffectRow<PaletteMapping, PaletteMappingParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            palette: ParameterText.Of("Palette", "Display colors each pixel maps to the nearest of"),
            dithering: StylizeText.Dithering,
            ditherStrength: StylizeText.DitherAmount));
}

[Guid("d0635c79-6508-4cc8-85fc-4fbc0af55405")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class HalftoneScreen : StageEffect<HalftoneScreen, Halftone, HalftoneParameter, InvalidStylize>, IEffectDefinition<Halftone, HalftoneParameter> {
    public const string Caption = "Halftone";

    public static EffectRow<Halftone, HalftoneParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            shape: ParameterText.Choice<ScreenShape, InvalidStylize>(
                "Shape", "Spot each screen cell inks, its inked area the cell's tone",
                static shape => shape.Map(round: "Round", ellipse: "Ellipse", diamond: "Diamond", square: "Square", line: "Line", wavyLine: "Wavy Line", cross: "Cross")),
            size: ParameterText.Of("Cell Size", "Screen cell height as a share of the frame's short side"),
            separation: ParameterText.Choice<Separation, InvalidStylize>(
                "Separation", "One ink screened on luma, or cyan, magenta, yellow, and key screens", static separation => separation.Map(mono: "Single Ink", process: "CMYK")),
            cyan: StylizeText.Screen("Cyan"),
            magenta: StylizeText.Screen("Magenta"),
            yellow: StylizeText.Screen("Yellow"),
            keyScreen: StylizeText.Screen("Key"),
            crossOnset: ParameterText.Of("Crosshatch", "Ink level where a perpendicular screen joins, 1 drawing none"),
            ink: ParameterText.Of("Ink", "Color the key screen prints"),
            paper: ParameterText.Of("Paper", "Sheet color the inks print on, off printing over the image"),
            mode: StylizeText.Blending));
}

[Guid("1aa387a7-1493-4d9f-aeb2-0c9a1b5773b8")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class GlyphGrid : StageEffect<GlyphGrid, GlyphCells, GlyphCellsParameter, InvalidStylize>, IEffectDefinition<GlyphCells, GlyphCellsParameter> {
    public const string Caption = "Glyphs";

    public static EffectRow<GlyphCells, GlyphCellsParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            glyphs: ParameterText.Of("Characters", "Characters the cells draw, ranked by the ink each covers"),
            font: ParameterText.Enumeration<HersheyFonts>(
                "Font", "Stroke font the characters draw in",
                static font => font switch {
                    HersheyFonts.Simplex => "Simplex",
                    HersheyFonts.Plain => "Plain",
                    HersheyFonts.Duplex => "Duplex",
                    HersheyFonts.Complex => "Complex",
                    HersheyFonts.Triplex => "Triplex",
                    HersheyFonts.ComplexSmall => "Complex Small",
                    HersheyFonts.ScriptSimplex => "Script Simplex",
                    HersheyFonts.ScriptComplex => "Script Complex",
                }),
            size: ParameterText.Of("Cell Size", "Character cell height as a share of the frame's short side"),
            coloring: ParameterText.Choice<GlyphColoring, InvalidStylize>(
                "Coloring", "Color each character draws in", static coloring => coloring.Map(input: "Image", text: "Text Color", perCell: "Random per Cell")),
            text: ParameterText.Of("Text Color", "Character color under text coloring"),
            background: ParameterText.Of("Background", "Color behind every character"),
            saturation: ParameterText.Of("Saturation", "Saturation of random cell colors"),
            jitter: ParameterText.Of("Jitter", "Range of random offsets added to cell tones before choosing characters"),
            seed: NoiseText.Seed,
            timing: NoiseText.Timing,
            hold: NoiseText.Hold,
            mode: StylizeText.Blending));
}

[Guid("d9419256-acc5-4728-b874-d9b529d5cab4")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class PixelSorting : StageEffect<PixelSorting, PixelSort, PixelSortParameter, InvalidStylize>, IEffectDefinition<PixelSort, PixelSortParameter> {
    public const string Caption = "Pixel Sort";

    public static EffectRow<PixelSort, PixelSortParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            sortKey: ParameterText.Choice<SortKey, InvalidStylize>("Sort By", "Value each span sorts on", static key => key.Map(luma: "Luma", hue: "Hue", saturation: "Saturation")),
            low: ParameterText.Of("Lower Bound", "Key value where sorted spans start, above the upper bound wrapping the band"),
            high: ParameterText.Of("Upper Bound", "Key value where sorted spans end"),
            direction: ParameterText.Choice<SortDirection, InvalidStylize>(
                "Direction", "Direction sorted values rise along", static direction => direction.Map(right: "Right", left: "Left", down: "Down", up: "Up"))));
}

[Guid("dbf0553d-7010-4f70-b0c8-1df82c2b722b")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class PaintFilter : StageEffect<PaintFilter, Painterly, PainterlyParameter, InvalidStylize>, IEffectDefinition<Painterly, PainterlyParameter> {
    public const string Caption = "Kuwahara";

    public static EffectRow<Painterly, PainterlyParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            kuwahara: ParameterText.Choice<KuwaharaKind, InvalidStylize>(
                "Type", "Square quadrants, or sectors following the image's strokes", static kind => kind.Map(classic: "Classic", anisotropic: "Anisotropic")),
            size: ParameterText.Of("Size", "Filter radius as a share of the frame's short side"),
            uniformity: ParameterText.Of("Uniformity", "Smoothing radius of the stroke direction as a share of the frame's short side"),
            sharpness: ParameterText.Of("Sharpness", "Weight toward the least varied sector"),
            eccentricity: ParameterText.Of("Eccentricity", "Elongation of the sectors along strokes")));
}

[Guid("965adbbd-2586-4504-8fc7-efff82b7983c")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class EdgeFilter : StageEffect<EdgeFilter, EdgeDetect, EdgeDetectParameter, InvalidStylize>, IEffectDefinition<EdgeDetect, EdgeDetectParameter> {
    public const string Caption = "Edge Detect";

    public static EffectRow<EdgeDetect, EdgeDetectParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            @operator: ParameterText.Choice<EdgeOperator, InvalidStylize>(
                "Type", "Kernel measuring the change around each pixel",
                static kernel => kernel.Map(laplace: "Laplace", sobel: "Sobel", prewitt: "Prewitt", kirsch: "Kirsch", emboss: "Emboss")),
            source: ParameterText.Choice<EdgeSource, InvalidStylize>(
                "Channel", "Color or the one channel the kernel reads",
                static source => source.Map(color: "Color", luma: "Luma", hue: "Hue", saturation: "Saturation", value: "Value", alpha: "Alpha")),
            softening: ParameterText.Of("Softening", "Blur ahead of the kernel as a share of the frame's short side"),
            rendering: ParameterText.Choice<EdgeRendering, InvalidStylize>(
                "Output", "Kernel response itself, lines, or lines on a fill", static rendering => rendering.Map(magnitude: "Response", lines: "Lines", linesOnFill: "Lines on Fill")),
            threshold: ParameterText.Of("Threshold", "Share of the kernel's step response a line pixel exceeds"),
            width: ParameterText.Of("Line Width", "Stroke width as a share of the frame's short side"),
            line: ParameterText.Of("Line Color", "Color the lines draw in"),
            fill: ParameterText.Of("Fill Color", "Color under the lines when they draw on a fill"),
            mode: StylizeText.Blending));
}

[Guid("8a1307d8-a9b1-4d95-b44c-64e8ab72b3d4")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class SceneOutline : StageEffect<SceneOutline, GeometryOutline, GeometryOutlineParameter, InvalidStylize>, IEffectDefinition<GeometryOutline, GeometryOutlineParameter> {
    public const string Caption = "Outline";

    public static EffectRow<GeometryOutline, GeometryOutlineParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            silhouette: StylizeText.Lines("Silhouettes", "Line class drawing depth steps where one surface occludes another"),
            crease: StylizeText.Lines("Creases", "Line class drawing where neighboring normals turn past the crease angle"),
            @object: StylizeText.Lines("Object Edges", "Line class drawing where object IDs change"),
            material: StylizeText.Lines("Material Edges", "Line class drawing where material IDs change"),
            step: ParameterText.Of("Depth Threshold", "Relative depth curvature marking a silhouette; across flat regions, 1 minus near depth divided by far depth"),
            creaseAngle: ParameterText.Of("Crease Angle", "Turn between neighboring normals marking a crease"),
            outerWidth: ParameterText.Of("Outer Width", "Outer line width as a share of the frame's short side, the cap under a world width"),
            outerWorld: ParameterText.Of("Outer World Width", "Outer line width on the surface it marks, narrowing with distance"),
            outerColor: ParameterText.Of("Outer Color", "Color of outer lines"),
            placement: ParameterText.Choice<StrokePlacement, InvalidStylize>(
                "Placement", "Side of its contour an outer line draws on", static placement => placement.Map(center: "Center", inside: "Inside", outside: "Outside")),
            innerWidth: ParameterText.Of("Inner Width", "Inner line width as a share of the frame's short side, the cap under a world width"),
            innerWorld: ParameterText.Of("Inner World Width", "Inner line width on the surface it marks, narrowing with distance"),
            innerColor: ParameterText.Of("Inner Color", "Color of inner lines"),
            taperNear: ParameterText.Of("Taper Near", "Near depth divided by each contour's depth to locate its width on the ramp, off keeping full width"),
            taperRamp: ParameterText.Of("Taper Ramp", "Ramp value scaling line width from distant contours at the left to near contours at the right"),
            paper: ParameterText.Of("Paper", "Color the lines draw on, off drawing over the image"),
            mode: StylizeText.Blending));
}

[Guid("2c3dfe8b-89b8-42d4-9bab-7c1f920b02e5")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class FilmWear : StageEffect<FilmWear, FilmDamage, FilmDamageParameter, InvalidStylize>, IEffectDefinition<FilmDamage, FilmDamageParameter> {
    public const string Caption = "Film Damage";

    public static EffectRow<FilmDamage, FilmDamageParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            specks: StylizeText.Spots("Specks"),
            dirt: StylizeText.Spots("Dirt"),
            blotches: StylizeText.Spots("Blotches"),
            scratches: StylizeText.Mark("Scratches"),
            scratchCount: ParameterText.Of("Scratch Count", "Scratch lines across the frame width"),
            scratchWidth: ParameterText.Of("Scratch Width", "Scratch stroke width as a share of the frame's short side"),
            scratchRate: ParameterText.Of("Scratch Rate", "Scratch field evolution per second"),
            hairs: StylizeText.Mark("Hairs"),
            hairCount: ParameterText.Of("Hair Count", "Mean hair strands per frame, including fractional counts"),
            hairWidth: ParameterText.Of("Hair Width", "Hair stroke width as a share of the frame's short side"),
            hairLength: ParameterText.Of("Hair Length", "Hair cell size as a share of the frame's short side"),
            hairCurl: ParameterText.Of("Hair Curl", "Bend of the hair field in hair cell sizes"),
            hairRate: ParameterText.Of("Hair Rate", "Hair field evolution per second"),
            weaveX: ParameterText.Of("Weave X", "Horizontal gate weave as a share of the frame's short side"),
            weaveY: ParameterText.Of("Weave Y", "Vertical gate weave as a share of the frame's short side"),
            weaveRate: ParameterText.Of("Weave Rate", "Gate weave noise cells per second"),
            seed: NoiseText.Seed,
            timing: NoiseText.Timing));
}

[Guid("de4f9efc-1a2c-4668-814f-0f280ded7917")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class JpegArtifacts : StageEffect<JpegArtifacts, CodecDamage, CodecDamageParameter, InvalidStylize>, IEffectDefinition<CodecDamage, CodecDamageParameter> {
    public const string Caption = "JPEG Damage";

    public static EffectRow<CodecDamage, CodecDamageParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            quality: ParameterText.Of("Quality", "JPEG quality each generation encodes at"),
            colorType: ParameterText.Enumeration<JpegColorType>(
                "Subsampling", "Color model and chroma subsampling the encoder codes",
                static type => type switch {
                    JpegColorType.YCbCrRatio420 => "4:2:0",
                    JpegColorType.YCbCrRatio444 => "4:4:4",
                    JpegColorType.YCbCrRatio422 => "4:2:2",
                    JpegColorType.YCbCrRatio411 => "4:1:1",
                    JpegColorType.YCbCrRatio410 => "4:1:0",
                    JpegColorType.Luminance => "Luminance",
                    JpegColorType.Rgb => "RGB",
                    JpegColorType.Cmyk => "CMYK",
                    JpegColorType.Ycck => "YCCK",
                }),
            generations: ParameterText.Of("Generations", "Encode and decode cycles, each recompressing the last"),
            coded: ParameterText.Of("Coded Lines", "Picture height the codec works at, off coding at the frame's height"))) {
        Looks = [("JPEG", new EffectLook<CodecDamage, CodecDamageParameter>.Whole(CodecDamage.Jpeg))],
    };
}

[Guid("50657d74-77ee-4dc0-99c9-01b94b4ff0d0")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class DigitalGlitch : StageEffect<DigitalGlitch, Glitch, GlitchParameter, InvalidStylize>, IEffectDefinition<Glitch, GlitchParameter> {
    public const string Caption = "Glitch";

    public static EffectRow<Glitch, GlitchParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            blockWidth: ParameterText.Of("Block Width", "Block width as a share of the frame's short side"),
            blockHeight: ParameterText.Of("Block Height", "Block height as a share of the frame's short side"),
            blockDensity: ParameterText.Of("Block Density", "Share of blocks selected for displacement or corruption"),
            blockShift: ParameterText.Of("Block Shift", "Largest sideways block displacement as a share of the frame's short side"),
            blockLift: ParameterText.Of("Block Lift", "Largest vertical block displacement as a share of the frame's short side"),
            tear: ParameterText.Of("Tear", "Full range of sideways displacement across torn bands as a share of the frame's short side"),
            tearPeriod: ParameterText.Of("Tear Period", "Height of each torn band as a share of the frame's short side"),
            split: ParameterText.Of("RGB Split", "Offset of red and blue in opposite directions as a share of the frame's short side"),
            splitDirection: ParameterText.Of("Split Angle", "Direction of the split, counterclockwise from right"),
            splitJitter: ParameterText.Of("Split Jitter", "Mix between a fixed split and a random signed split each held period"),
            slide: ParameterText.Of("Slide", "Share of the split each moved channel smears across"),
            matrix: StylizeText.Matrix,
            lumaCorruption: ParameterText.Of("Luma Corruption", "Mix toward luma raised to a random power in selected blocks"),
            chromaCorruption: ParameterText.Of("Chroma Corruption", "Mix toward chroma raised to a random power in selected blocks"),
            wrap: ParameterText.Choice<WrapMode, InvalidPixelValue>(
                "Edges", "Fill of pixels a displacement uncovers", ToneText.WrapText),
            seed: NoiseText.Seed,
            timing: NoiseText.Timing,
            hold: NoiseText.Hold)) {
        Looks = [("Corruption", new EffectLook<Glitch, GlitchParameter>.Whole(Glitch.Corruption)), ("Glitch", new EffectLook<Glitch, GlitchParameter>.Whole(Glitch.Displaced))],
    };
}

[Guid("c33c6983-8c08-4ace-b768-dd7988d96500")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class VideoSignal : StageEffect<VideoSignal, AnalogVideo, AnalogVideoParameter, InvalidStylize>, IEffectDefinition<AnalogVideo, AnalogVideoParameter> {
    public const string Caption = "Analog Video";

    public static EffectRow<AnalogVideo, AnalogVideoParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            matrix: StylizeText.Matrix,
            lines: ParameterText.Of("Lines", "Raster lines across the frame height"),
            lumaBandwidth: ParameterText.Of("Luma Blur", "Luma band limit along each line as a share of the frame's short side"),
            chromaBandwidth: ParameterText.Of("Chroma Blur", "Chroma band limit along each line as a share of the frame's short side"),
            chromaVertical: ParameterText.Of("Chroma Vertical Blur", "Chroma blur across lines as a share of the frame's short side"),
            chromaPitch: ParameterText.Of("Chroma Sampling", "Chroma sample spacing along each line as a share of the frame's short side"),
            chromaDelay: ParameterText.Of("Chroma Delay", "Shift of both color difference signals against luma"),
            smear: ParameterText.Of("Smear", "Length of the luma trail right of each edge"),
            smearMix: ParameterText.Of("Smear Amount", "Share of the trail mixed into luma"),
            ringing: ParameterText.Of("Ringing", "Overshoot beside edges at the luma band's own scale"),
            ringingDelay: ParameterText.Of("Ringing Delay", "Signed shift of ringing from image edges"),
            emboss: ParameterText.Of("Emboss", "Relief from the band-limited luma slope"),
            ghost: ParameterText.Of("Ghost", "Strength of a delayed copy of the signal"),
            ghostDelay: ParameterText.Of("Ghost Delay", "Shift of the delayed copy"),
            crossColor: ParameterText.Of("Cross Color", "Rainbow fringes fine luma detail leaks into chroma"),
            lumaNoise: ParameterText.Of("Luma Noise", "Deviation of Gaussian noise entering luma before its band limit"),
            chromaNoise: ParameterText.Of("Chroma Noise", "Deviation of independent Gaussian noise entering each chroma signal before its band limit"),
            noiseLength: ParameterText.Of("Noise Length", "Gaussian noise correlation along each line as a share of the frame's short side"),
            snow: ParameterText.Of("Snow", "Brightness of snow streaks"),
            snowDensity: ParameterText.Of("Snow Density", "Share of line cells lit as snow"),
            snowLength: ParameterText.Of("Snow Length", "Length of each lit snow cell's fading head as a share of the frame's short side"),
            interlace: ParameterText.Of("Interlace", "Shift of odd lines against even lines"),
            jitter: ParameterText.Of("Line Jitter", "Standard deviation of Gaussian sideways line shifts"),
            jitterPeriod: ParameterText.Of("Jitter Period", "Height over which line shifts change"),
            wrinkle: ParameterText.Of("Wrinkle", "Sideways shift inside the rolling wrinkle band"),
            wrinkleHeight: ParameterText.Of("Wrinkle Height", "Half height of the wrinkle band"),
            wrinkleRate: ParameterText.Of("Wrinkle Rate", "Passes of the wrinkle band up the frame per second"),
            headSwitch: ParameterText.Of("Head Switching", "Sideways skew of the bottom band where the tape heads switch"),
            headSwitchHeight: ParameterText.Of("Head Switching Height", "Height of the head switching band"),
            bounce: ParameterText.Of("Bounce", "Largest vertical jump of the picture"),
            rollRate: ParameterText.Of("Roll Rate", "Frame heights the picture rolls up per second"),
            hum: ParameterText.Of("Hum", "Luma darkening of the hum bar, preserving color difference signals"),
            humRate: ParameterText.Of("Hum Rate", "Passes of the hum bar up the frame per second"),
            seed: NoiseText.Seed,
            timing: NoiseText.Timing)) {
        Looks = [("Chroma Blur", new EffectLook<AnalogVideo, AnalogVideoParameter>.Whole(AnalogVideo.ChromaBlur))],
    };
}

[Guid("0bfccb12-f88d-4843-8836-e91acfd439a2")]
[CustomPostEffect(PostEffectType.Late, Caption)]
public sealed class TubeDisplay : StageEffect<TubeDisplay, CrtDisplay, CrtDisplayParameter, InvalidStylize>, IEffectDefinition<CrtDisplay, CrtDisplayParameter> {
    public const string Caption = "CRT Display";

    public static EffectRow<CrtDisplay, CrtDisplayParameter> Row { get; } = new(static parameter =>
        parameter.Map(
            mask: ParameterText.Choice<PhosphorMask, InvalidStylize>(
                "Mask", "Phosphor layout behind the glass",
                static mask => mask.Map(apertureGrille: "Aperture Grille", slotMask: "Slot Mask", shadowMask: "Shadow Mask")),
            pitch: ParameterText.Of("Triad Pitch", "Phosphor triad width as a share of the frame's short side"),
            maskStrength: ParameterText.Of("Mask Strength", "Share of the light the mask moves onto each phosphor"),
            lines: ParameterText.Of("Lines", "Scanlines across the frame height"),
            interlaced: ParameterText.Of("Interlaced", "Lights alternate lines on alternate frames"),
            scanlineDepth: ParameterText.Of("Scanline Depth", "Darkening between scanlines"),
            collapse: ParameterText.Of("Collapse", "Raster shrinking toward a line and then a dot, a tube losing power"),
            glow: ParameterText.Of("Glow", "Spread of the beam's light as a share of the frame's short side"),
            curvature: ParameterText.Of("Curvature", "Bulge of the tube's glass"),
            framing: LensText.Framing,
            timing: NoiseText.Timing)) {
        Looks = [("CRT", new EffectLook<CrtDisplay, CrtDisplayParameter>.Whole(CrtDisplay.Monitor)), ("Scanlines", new EffectLook<CrtDisplay, CrtDisplayParameter>.Whole(CrtDisplay.Scanlines))],
    };
}
