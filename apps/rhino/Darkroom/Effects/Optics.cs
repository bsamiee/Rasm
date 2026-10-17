using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Optics;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("7400690e-f17b-4539-a88a-38b5bfa154fd")]
[CustomPostEffect(PostEffectType.Early, "Lens Blur")]
public sealed class LensBlur : StageEffect<LensBlur, Defocus, DefocusParameter>;

[Guid("944522ca-dde2-4e3e-81b0-82565728951b")]
[CustomPostEffect(PostEffectType.Early, "Lens Distortion")]
public sealed class RadialDistortion : StageEffect<RadialDistortion, LensDistortion, LensDistortionParameter>;

[Guid("106bf35b-4759-4e08-a0a3-38f02c7be061")]
[CustomPostEffect(PostEffectType.Early, "Axial Fringing")]
public sealed class AxialFringing : StageEffect<AxialFringing, AxialAberration, AxialAberrationParameter>;

[Guid("b8bf3b1d-0d30-4019-b5d5-417dba7936db")]
[CustomPostEffect(PostEffectType.Early, "Lens Bloom")]
public sealed class LensBloom : StageEffect<LensBloom, Bloom, BloomParameter>;

[Guid("134e641a-47aa-4f42-9084-8040f3568e94")]
[CustomPostEffect(PostEffectType.Early, "Veiling Flare")]
public sealed class VeilingFlare : StageEffect<VeilingFlare, VeilingGlare, VeilingGlareParameter>;

[Guid("166dcfdb-9388-4cde-ac2b-2c5fe76e4604")]
[CustomPostEffect(PostEffectType.Early, "Aperture Diffraction")]
public sealed class ApertureDiffraction : StageEffect<ApertureDiffraction, StarGlare, StarGlareParameter>;

[Guid("d3df8c60-5cc3-4c70-bbf7-2996bd2d7fd0")]
[CustomPostEffect(PostEffectType.Early, "Lens Reflections")]
public sealed class LensReflections : StageEffect<LensReflections, Ghosts, GhostsParameter>;

[Guid("f4afa240-2e97-469d-8f23-ce02aab2f499")]
[CustomPostEffect(PostEffectType.Early, "Natural Vignetting")]
public sealed class NaturalVignetting : StageEffect<NaturalVignetting, Vignette, VignetteParameter>;

[Guid("6a47cbbf-f6a2-4b53-9b6d-6ccd6c5ede48")]
[CustomPostEffect(PostEffectType.Early, "Halation")]
public sealed class FilmHalation : StageEffect<FilmHalation, Halation, HalationParameter>;
