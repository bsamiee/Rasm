using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Render.Effects;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("439c2d1c-2cbb-4136-aba8-5bdb6e431ef1")]
[CustomPostEffect(PostEffectType.Early, "Darkroom",
    PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.ExecuteForViewportDisplay
    | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn)]
public sealed class DarkroomEffect : DefinedEffect<DarkroomEffect, ChainState>, IDefined<EffectDefinition<ChainState>> {
    public static Derivations Derived { get; } = new();

    public static EffectDefinition<ChainState> Definition { get; } = new(ChainState.Default, Formed) {
        Channels = static state => state.Needs.Match(Succ: static needs => needs.Map(static channel => channel.Key), Fail: static _ => Seq<RenderWindow.StandardChannels>()),
        Runs = static state => state.Entries.Exists(static entry => entry.On),
        Scope = static _ => EffectScope.Whole,
    };

    // --- [PIPELINE]
    private static IO<Unit> Formed(ChainState state, EffectFrame frame) =>
        from extent in IO.lift(PixelExtent.Validated(frame.Extent.Width, frame.Extent.Height))
        from size in IO.lift(PixelExtent.Validated(frame.Rect.Width, frame.Rect.Height))
        from doc in IO.lift(static () => Conversions.Required(RhinoDoc.ActiveDoc))
        from scene in ChainScene.Sourced(doc, extent, None)
        from guides in IO.lift(state.Needs.Map(needs => needs.Choose(channel => Guide(frame, channel, size, extent, scene.Meters))))
        from chain in IO.lift(state.Passes(scene.Context(extent, toHashMap(guides), Derived, None) with { Rendering = frame.Rendering }))
        from gamma in chain.Context.Formed
            ? Lights.PostProcessGamma(doc).Bind(static exponent => IO.lift(exponent.Traverse(static held => Transfer.Gamma.Of(held)).As()))
            : IO.pure(Option<Transfer.Gamma>.None)
        let passes = chain.Passes.Map(static step => step.Pass) + gamma.Map(static compensated => (PixelPass)new PixelPass.Point(compensated.Decode.Apply)).ToSeq()
        from ran in when(!passes.IsEmpty, IO.lift(() => Ran(passes, frame, size, extent))).As()
        select unit;

    private static Fin<Unit> Ran(Seq<PixelPass> passes, EffectFrame frame, PixelExtent size, PixelExtent extent) =>
        new PixelFrame(frame.Rect.Location, size, extent, block => frame.Rgba.Span.CopyTo(block)) switch {
            var pixels => passes.TraverseM(pass => pass.Run(pixels)).As().Map(_ => {
                pixels.Block.AsSpan2D(size.Height, 4 * size.Width).CopyTo(frame.Rgba.Span);
                return unit;
            }),
        };

    private static Option<(GuideChannel Channel, PixelFrame Frame)> Guide(EffectFrame frame, GuideChannel channel, PixelExtent size, PixelExtent extent, Option<float> meters) =>
        from plane in frame.Inputs.Find(channel.Key)
        let rgba = Channels.Widened(plane.Span, size.Width)
        let read = new PixelFrame(frame.Rect.Location, size, extent, pixels => rgba.Span.CopyTo(pixels))
        select (channel, meters.Map(scale => channel.Measured(read, scale)).IfNone(read));
}
