using Rasm.Imaging.Filters.Denoise;
using Rasm.Rhino.Render.Effects;

using System.Runtime.InteropServices;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("7465f313-020e-49ce-acd3-58ade34c750a")]
[CustomPostEffect(PostEffectType.Early, Caption, executeWhileRenderingOption: PostEffectExecuteWhileRenderingOptions.UseExecutionControl)]
public sealed class GuidedDenoise : JobEffect<GuidedDenoise, DenoiseState, DenoiseParameter, InvalidDenoise, Denoiser>, IEffectDefinition<DenoiseState, DenoiseParameter> {
    public const string Caption = "Guided Denoise";

    public static EffectRow<DenoiseState, DenoiseParameter> Row { get; } = new(static parameter => parameter.Map(
        guides: ParameterText.Choice<DenoiseGuides, InvalidDenoise>(
            "Passes", "Render passes that tell the denoiser surface detail from noise",
            static passes => passes.Map(rgb: "None", rgbAlbedo: "Albedo", rgbAlbedoNormal: "Albedo and Normal")),
        prefilter: ParameterText.Enumeration<GuidePrefilter>(
            "Prefilter", "Cleans noise from the guide passes before they guide the image",
            static prefilter => prefilter == GuidePrefilter.Off ? "None" : prefilter.ToString()),
        quality: ParameterText.Choice<DenoiseQuality, InvalidDenoise>(
            "Quality", "Denoiser model, slower and sharper at higher quality",
            static quality => quality.Map(high: "High", balanced: "Balanced", fast: "Fast")),
        blend: ParameterText.Of("Blend", "Mixes the denoised image over the noisy render"))) { Amount = None };
}
