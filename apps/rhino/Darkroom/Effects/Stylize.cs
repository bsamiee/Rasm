using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Stylize;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("72443a3b-a5f1-4018-8368-7ad55d5f6127")]
[CustomPostEffect(PostEffectType.Early, "Film Flicker")]
public sealed class ProjectorFlicker : StageEffect<ProjectorFlicker, FilmFlicker, FilmFlickerParameter>;

[Guid("3f5e6947-cf6e-4623-a607-5ddda6e05486")]
[CustomPostEffect(PostEffectType.Late, "Mosaic")]
public sealed class Pixelation : StageEffect<Pixelation, Mosaic, MosaicParameter>;

[Guid("6666a316-3ff8-49ce-a566-34d9531372ea")]
[CustomPostEffect(PostEffectType.Late, "Posterize")]
public sealed class Posterization : StageEffect<Posterization, Posterize, PosterizeParameter>;

[Guid("b0fd305b-80fb-4726-ab16-ca4223c7d61c")]
[CustomPostEffect(PostEffectType.Late, "Adaptive Palette")]
public sealed class AdaptivePalette : StageEffect<AdaptivePalette, PaletteReduction, PaletteReductionParameter>;

[Guid("fc33be5b-eb04-4bb7-b890-89e5778451c9")]
[CustomPostEffect(PostEffectType.Late, "Custom Palette")]
public sealed class CustomPalette : StageEffect<CustomPalette, PaletteMapping, PaletteMappingParameter>;

[Guid("d0635c79-6508-4cc8-85fc-4fbc0af55405")]
[CustomPostEffect(PostEffectType.Late, "Halftone")]
public sealed class HalftoneScreen : StageEffect<HalftoneScreen, Halftone, HalftoneParameter>;

[Guid("1aa387a7-1493-4d9f-aeb2-0c9a1b5773b8")]
[CustomPostEffect(PostEffectType.Late, "Glyphs")]
public sealed class GlyphGrid : StageEffect<GlyphGrid, GlyphCells, GlyphCellsParameter>;

[Guid("d9419256-acc5-4728-b874-d9b529d5cab4")]
[CustomPostEffect(PostEffectType.Late, "Pixel Sort")]
public sealed class PixelSorting : StageEffect<PixelSorting, PixelSort, PixelSortParameter>;

[Guid("dbf0553d-7010-4f70-b0c8-1df82c2b722b")]
[CustomPostEffect(PostEffectType.Late, "Kuwahara")]
public sealed class PaintFilter : StageEffect<PaintFilter, Painterly, PainterlyParameter>;

[Guid("965adbbd-2586-4504-8fc7-efff82b7983c")]
[CustomPostEffect(PostEffectType.Late, "Edge Detect")]
public sealed class EdgeFilter : StageEffect<EdgeFilter, EdgeDetect, EdgeDetectParameter>;

[Guid("8a1307d8-a9b1-4d95-b44c-64e8ab72b3d4")]
[CustomPostEffect(PostEffectType.Late, "Outline")]
public sealed class SceneOutline : StageEffect<SceneOutline, GeometryOutline, GeometryOutlineParameter>;

[Guid("2c3dfe8b-89b8-42d4-9bab-7c1f920b02e5")]
[CustomPostEffect(PostEffectType.Late, "Film Damage")]
public sealed class FilmWear : StageEffect<FilmWear, FilmDamage, FilmDamageParameter>;

[Guid("de4f9efc-1a2c-4668-814f-0f280ded7917")]
[CustomPostEffect(PostEffectType.Late, "JPEG Damage")]
public sealed class JpegArtifacts : StageEffect<JpegArtifacts, CodecDamage, CodecDamageParameter>, IEffectDefinition<CodecDamage, CodecDamageParameter> {
    public static EffectRow<CodecDamage, CodecDamageParameter> Row { get; } = new() {
        Looks = [("JPEG", CodecDamage.Jpeg)],
    };
}

[Guid("50657d74-77ee-4dc0-99c9-01b94b4ff0d0")]
[CustomPostEffect(PostEffectType.Late, "Glitch")]
public sealed class DigitalGlitch : StageEffect<DigitalGlitch, Glitch, GlitchParameter>, IEffectDefinition<Glitch, GlitchParameter> {
    public static EffectRow<Glitch, GlitchParameter> Row { get; } = new() {
        Looks = [("Corruption", Glitch.Corruption), ("Glitch", Glitch.Displaced)],
    };
}

[Guid("c33c6983-8c08-4ace-b768-dd7988d96500")]
[CustomPostEffect(PostEffectType.Late, "Analog Video")]
public sealed class VideoSignal : StageEffect<VideoSignal, AnalogVideo, AnalogVideoParameter>, IEffectDefinition<AnalogVideo, AnalogVideoParameter> {
    public static EffectRow<AnalogVideo, AnalogVideoParameter> Row { get; } = new() {
        Looks = [("Chroma Blur", AnalogVideo.ChromaBlur)],
    };
}

[Guid("0bfccb12-f88d-4843-8836-e91acfd439a2")]
[CustomPostEffect(PostEffectType.Late, "CRT Display")]
public sealed class TubeDisplay : StageEffect<TubeDisplay, CrtDisplay, CrtDisplayParameter>, IEffectDefinition<CrtDisplay, CrtDisplayParameter> {
    public static EffectRow<CrtDisplay, CrtDisplayParameter> Row { get; } = new() {
        Looks = [("CRT", CrtDisplay.Monitor), ("Scanlines", CrtDisplay.Scanlines)],
    };
}
