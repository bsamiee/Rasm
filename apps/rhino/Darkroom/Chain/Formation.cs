using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.RhinoCommon;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DisplayFormation {
    // --- [OUTPUT]
    public static Gamut Working { get; } = Gamut.StandardRgb;

    public static DisplayDevice Display { get; } = DisplayDevice.Srgb;

    public static Seq<PixelPass> Displayed(PassContext context) =>
        new ColorEncoding(context.Working, context.Signal).To(context.Display.Encoding).Map<PixelPass>(static step => new PixelPass.Color(step));

    // --- [NATIVE]
    public static IO<Option<Transfer.Gamma>> NativeGamma(PostEffectPipeline pipeline) =>
        IO.lift(() => Optional(((IPostEffects)pipeline).PostEffectFromId(PostEffectUuids.Gamma))
            .Filter(gamma => gamma.CanExecute(pipeline))
            .Traverse(static gamma => {
                object? value = null;
                return RhinoFault.Unless(gamma.GetParam("gamma", ref value)).Bind(_ => Conversions.Cast<float>(value)).Bind(static exponent => Transfer.Gamma.Of(exponent));
            })
            .As());

    public static PixelPass Compensated(Transfer.Gamma gamma) => new PixelPass.Color(gamma.Decode);
}
