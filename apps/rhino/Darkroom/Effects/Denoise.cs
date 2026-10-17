using System.Runtime.InteropServices;
using Rasm.Imaging.Filters.Denoise;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("7465f313-020e-49ce-acd3-58ade34c750a")]
[CustomPostEffect(PostEffectType.Early, "Guided Denoise", executeWhileRenderingOption: PostEffectExecuteWhileRenderingOptions.UseExecutionControl)]
public sealed class GuidedDenoise : JobEffect<GuidedDenoise, DenoiseState, DenoiseParameter, Denoiser>, IEffectDefinition<DenoiseState, DenoiseParameter> {
    public static EffectRow<DenoiseState, DenoiseParameter> Row { get; } = new() { Amount = None };
}
