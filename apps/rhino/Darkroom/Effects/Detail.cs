using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Detail;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("ce14bc1b-2e14-41c0-91d5-54e4886e3d1d")]
[CustomPostEffect(PostEffectType.Early, "Sharpen")]
public sealed class AdaptiveSharpening : StageEffect<AdaptiveSharpening, Sharpen, SharpenParameter>;

[Guid("b03f5ab9-d399-4e1e-95ac-ece13e441ced")]
[CustomPostEffect(PostEffectType.Early, "Film Grain")]
public sealed class EmulsionGrain : StageEffect<EmulsionGrain, FilmGrain, FilmGrainParameter>;

[Guid("49dd6485-0396-4b83-8744-516aecbffe53")]
[CustomPostEffect(PostEffectType.Early, "Local Exposure")]
public sealed class LocalExposure : StageEffect<LocalExposure, LocalContrast, LocalContrastParameter>;

[Guid("a9cab48c-9612-4712-85f5-eafd71be51b6")]
[CustomPostEffect(PostEffectType.Early, "Sensor Noise")]
public sealed class ImageNoise : StageEffect<ImageNoise, SensorNoise, SensorNoiseParameter>;
