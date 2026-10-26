using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Render.Effects;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Darkroom.Chain;

// --- [COMPOSITION] ---------------------------------------------------------------------
[Guid("439c2d1c-2cbb-4136-aba8-5bdb6e431ef1")]
[CustomPostEffect(PostEffectType.Early, "Darkroom",
    PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.ExecuteForViewportDisplay
    | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn)]
public sealed class DarkroomEffect() : DefinedEffect<ChainState>(ChainState.Default) {
    private readonly Derivations derivations = new();

    // --- [PIPELINE]
    public override Guid[] RequiredChannels =>
        Callbacks.Answer(State.Needs.Map(static needs => Channels.Required(needs.Map(static channel => channel.Key))), static () => Channels.Required([]), CallbackSite.Of(Owner, this));

    public override bool CanExecute(PostEffectPipeline pipeline) =>
        base.CanExecute(pipeline) && Callbacks.Answer(
            RenderRuns.Slot.Find(Owner).Map(runs => !State.Running.IsEmpty || (runs.IsSome && Watched(pipeline))), static () => false, CallbackSite.Of(Owner, this));

    public override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Callbacks.Succeeded(Formed(pipeline, rect), CallbackSite.Of(Owner, this));

    private IO<Unit> Formed(PostEffectPipeline pipeline, Rectangle rect) =>
        from size in IO.lift(() => Channels.Extent(pipeline) switch { var whole => PixelExtent.Validated(whole.Width, whole.Height).Map(extent => (Whole: whole, Extent: extent)) })
        let state = State
        from runs in RenderRuns.Slot.Find(Owner)
        from watched in IO.lift(() => rect == size.Whole && runs.IsSome && Watched(pipeline))
        from needs in state.Needs
        from guides in ChainScene.Unframed.Guides(needs, channel => ChainScene.Frame(pipeline.GetChannelForRead, channel.Key, size.Whole, size.Extent))
        from chain in state.Passes(ChainScene.Unframed.Context(size.Extent, guides, derivations, pipeline.IsRendering, None))
        let passes = chain.Passes.Map(static step => step.Pass)
        from ran in when(!passes.IsEmpty || watched,
            from frame in ChainScene.Frame(pipeline.GetChannelForRead, RenderWindow.StandardChannels.RGBA, passes.Exists(static pass => pass is PixelPass.Frame) ? size.Whole : rect, size.Extent)
            let input = watched ? Some(frame.Copy()) : None
            from staged in Ran(passes, frame, pipeline)
            from formed in input.Traverse(_ => Displayed(chain.Context, frame, pipeline)).As()
            from gamma in chain.Context.Formed ? DisplayFormation.NativeGamma(pipeline) : IO.pure(Option<Transfer.Gamma>.None)
            from compensated in Ran(gamma.Map(DisplayFormation.Compensated).ToSeq(), frame, pipeline)
            from written in when(!passes.IsEmpty, Written(pipeline, frame)).As()
            from after in input.IsSome ? After(pipeline) : IO.pure(Seq<(Guid Id, string Name)>())
            from recorded in (from held in runs from scene in input from shown in formed
                              select held.Recorded(pipeline.RenderingId, new RenderRun(scene, shown, ChainScene.Unframed, guides, after))).IfNone(IO.pure(unit))
            select unit).As()
        select unit;

    private static bool Watched(PostEffectPipeline pipeline) =>
        !pipeline.IsRendering && DisposalOps.Present(RenderWindow.FromSessionId(pipeline.RenderingId));

    private static IO<Unit> Ran(Seq<PixelPass> passes, PixelFrame frame, IProgress<int> progress) =>
        passes.TraverseM(pass => IO.lift(() => pass.Run(frame, progress))).As().Map(unit);

    private static IO<PixelFrame> Displayed(PassContext context, PixelFrame frame, IProgress<int> progress) =>
        frame.Copy() switch {
            var shown => Ran(DisplayFormation.Displayed(context), shown, progress).Map(shown),
        };

    private static IO<Unit> Written(PostEffectPipeline pipeline, PixelFrame frame) =>
        from memory in IO.lift(() => Refused.Unless(frame.View.TryGetMemory(out Memory<Vector4> held), held, nameof(Memory2D<>.TryGetMemory)))
        from written in Channels.Write(pipeline, new Rectangle(frame.Origin, new Size(frame.Size.Width, frame.Size.Height)), memory.Cast<Vector4, float>())
        select written;

    private IO<Seq<(Guid Id, string Name)>> After(PostEffectPipeline pipeline) =>
        IO.lift(() => toSeq(toSeq(pipeline.ExecutionOrder()).SkipWhile(id => id != Id).Skip(1))
            .Choose(id => Optional(((IPostEffects)pipeline).PostEffectFromId(id)))
            .Filter(effect => effect.CanExecute(pipeline))
            .Map(static effect => (effect.Id, effect.LocalName))
            .Strict());

    // --- [RELEASE]
    public override void Dispose(bool bDisposing) {
        try {
            if (bDisposing)
                derivations.Dispose();
        } finally {
            base.Dispose(bDisposing);
        }
    }
}
