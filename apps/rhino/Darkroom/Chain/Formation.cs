using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayFormation {
    // --- [OUTPUT]
    public static Gamut Working => Gamut.StandardRgb;

    public static DisplayDevice Display => DisplayDevice.Srgb;

    public static Seq<PixelPass> Displayed(PassContext context) =>
        new ColorEncoding(context.Working, context.Signal).To(context.Display.Encoding).Map<PixelPass>(static step => new PixelPass.Color(step));

    // --- [NATIVE]
    public static IO<Option<Transfer.Gamma>> NativeGamma(PostEffectPipeline pipeline) =>
        IO.lift(() => Optional(((IPostEffects)pipeline).PostEffectFromId(PostEffectUuids.Gamma))
            .Filter(gamma => gamma.CanExecute(pipeline))
            .Traverse(static gamma => {
                object? value = null;
                return Refused.Unless(gamma.GetParam("gamma", ref value), nameof(PostEffect.GetParam))
                    .Bind(_ => value is float exponent ? Transfer.Gamma.Of(exponent) : new Refused(nameof(PostEffect.GetParam), Refused.Invalid));
            })
            .As());

    public static PixelPass Compensated(Transfer.Gamma gamma) => new PixelPass.Color(gamma.Decode);
}
