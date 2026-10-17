using System.Runtime.InteropServices;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Render.Effects;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("3213d575-58cc-4a78-b13a-d130c437f053")]
[CustomPostEffect(PostEffectType.Early, "Exposure")]
public sealed class SceneExposure : StageEffect<SceneExposure, ExposureState, ExposureParameter>;

[Guid("72433230-65e7-4c28-b1a9-0afa4e6f32e2")]
[CustomPostEffect(PostEffectType.ToneMapping, "Darkroom", EffectStyles.Listed)]
public sealed class ImageFormation : StageEffect<ImageFormation, ToneMapping, ToneMappingParameter>, IEffectDefinition<ToneMapping, ToneMappingParameter> {
    public static EffectRow<ToneMapping, ToneMappingParameter> Row { get; } = new() {
        Amount = None,
        Looks = toSeq(ToneMappingParameter.Items).Filter(static item => item.Formations.Equals(Seq(Formation.Film))) switch {
            var keys => toSeq(FilmPreset.Items).Map<(string, EffectLook<ToneMapping, ToneMappingParameter>)>(preset => (
                preset.Map(@default: "Default", nostalgia: "Nostalgia", silver: "Silver"),
                new EffectLook<ToneMapping, ToneMappingParameter>.Partial(ToneMapping.Default with { Film = preset.State }, keys))),
        },
    };
}
