using System.Drawing;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EffectJob : PostEffectJob {
    private readonly Func<Rectangle, PostEffectJobChannels, IO<Unit>> work;
    private readonly CallbackSite site;

    private EffectJob(Func<Rectangle, PostEffectJobChannels, IO<Unit>> work, CallbackSite site) => (this.work, this.site) = (work, site);

    public override PostEffectJob Clone() => new EffectJob(work, site);

    public override bool Execute(Rectangle rect, PostEffectJobChannels access) => Callbacks.Succeeded(work(rect, access), site);

    public static IO<Unit> Run(
        PostEffectPipeline pipeline, PostEffect effect, Rectangle rect, Seq<RenderWindow.StandardChannels> channels,
        Func<Rectangle, PostEffectJobChannels, IO<Unit>> work, CallbackSite site) =>
        DisposalOps.Within(() => new EffectJob(work, site), job =>
            DisposalOps.Found(pipeline.ThreadEngine, nameof(PostEffectPipeline.ThreadEngine), engine => IO.lift(() => Refused.Unless(
                engine.RunPostEffect(job, pipeline, effect, rect, Channels.Required(channels)), nameof(PostEffectThreadEngine.RunPostEffect)))));
}
