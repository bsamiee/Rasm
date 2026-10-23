using System.Runtime.InteropServices;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("f2d92349-c684-4276-86dd-587c71634aff")]
[CustomPostEffect(PostEffectType.Late, "Output Dither", EffectStyles.Listed)]
public sealed class OutputDither : StageEffect<OutputDither, Dither, DitherParameter>;
