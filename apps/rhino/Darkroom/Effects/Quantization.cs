using System.Runtime.InteropServices;
using Rasm.Imaging.Quantization;
using Rasm.Rhino.Render.Effects;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.UI.Rows;
using Rhino.Render.PostEffects;

namespace Darkroom.Effects;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("f2d92349-c684-4276-86dd-587c71634aff")]
[CustomPostEffect(PostEffectType.Late, Caption, EffectStyles.Listed)]
public sealed class OutputDither : StageEffect<OutputDither, Dither, DitherParameter, InvalidQuantization>, IEffectDefinition<Dither, DitherParameter> {
    public const string Caption = "Dither";

    public static EffectRow<Dither, DitherParameter> Row { get; } = new(static parameter =>
        parameter.Map(method: ParameterText.Choice<Dither, InvalidQuantization>(
            "Method", "Noise or diffused error added before integer quantization", TargetRows.DitherText)));
}
