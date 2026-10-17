using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Warp;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("f7d32275-fcbc-4c8a-ad36-8826d09678a5")]
[CustomPostEffect(PostEffectType.Early, "Reprojection")]
public sealed class ViewReprojection : StageEffect<ViewReprojection, Panorama, PanoramaParameter>;

[Guid("30219496-aff4-482e-91cc-8059f0a1532e")]
[CustomPostEffect(PostEffectType.Early, "Twirl")]
public sealed class Swirl : StageEffect<Swirl, Twirl, TwirlParameter>;

[Guid("44c0cd28-8f52-44c7-b508-946cf6cd29cc")]
[CustomPostEffect(PostEffectType.Early, "Spherize")]
public sealed class Spherize : StageEffect<Spherize, Bulge, BulgeParameter>;

[Guid("651cc9ac-db9d-48b7-935b-142916a94f16")]
[CustomPostEffect(PostEffectType.Early, "Wave")]
public sealed class WaveWarp : StageEffect<WaveWarp, Wave, WaveParameter>;

[Guid("3017b4d2-68e3-46f7-984e-daafecefe685")]
[CustomPostEffect(PostEffectType.Early, "Turbulent Displace")]
public sealed class TurbulentDisplace : StageEffect<TurbulentDisplace, Turbulence, TurbulenceParameter>;

[Guid("3833180c-a491-44a2-961d-a875bc57874b")]
[CustomPostEffect(PostEffectType.Early, "Displace")]
public sealed class Displace : StageEffect<Displace, MapDisplacement, MapDisplacementParameter>;

[Guid("52c0caf8-262f-49d1-90d0-0bac92cedbc8")]
[CustomPostEffect(PostEffectType.Early, "Glass")]
public sealed class Glass : StageEffect<Glass, Refraction, RefractionParameter>;

[Guid("d21a93e6-91f0-497e-b9f9-21f030037564")]
[CustomPostEffect(PostEffectType.Early, "Kaleidoscope")]
public sealed class Kaleida : StageEffect<Kaleida, Kaleidoscope, KaleidoscopeParameter>;

[Guid("1f0dfd6b-db2d-41e6-8b94-0bc199d18cab")]
[CustomPostEffect(PostEffectType.Early, "Polar Coordinates")]
public sealed class PolarWarp : StageEffect<PolarWarp, PolarCoordinates, PolarCoordinatesParameter>;

[Guid("2840e7bd-5137-4e50-99c2-82efcea276b7")]
[CustomPostEffect(PostEffectType.Early, "Transform")]
public sealed class ImageTransform : StageEffect<ImageTransform, AffineTransform, AffineTransformParameter>;

[Guid("106fb69d-480f-430a-95f0-cc9ee4bea176")]
[CustomPostEffect(PostEffectType.Early, "Corner Pin")]
public sealed class CornerPin2D : StageEffect<CornerPin2D, CornerPin, CornerPinParameter>;

[Guid("4c2a5565-2369-462c-91cf-2b52ce565040")]
[CustomPostEffect(PostEffectType.Early, "Camera Shake")]
public sealed class CameraShake : StageEffect<CameraShake, ShakeWarp, ShakeWarpParameter>;
