using System.Drawing;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class EffectJob : PostEffectJob {
    private readonly Func<Rectangle, PostEffectJobChannels, IO<Unit>> work;
    private readonly object owner;

    private EffectJob(Func<Rectangle, PostEffectJobChannels, IO<Unit>> work, object owner) => (this.work, this.owner) = (work, owner);

    public override PostEffectJob Clone() => new EffectJob(work, owner);

    public override bool Execute(Rectangle rect, PostEffectJobChannels access) => owner.Answer(work(rect, access).Map(value: true), failed: false);

    public static IO<Unit> Run(
        PostEffectPipeline pipeline, PostEffect effect, Rectangle rect, Seq<RenderWindow.StandardChannels> channels,
        Func<Rectangle, PostEffectJobChannels, IO<Unit>> work, object owner) =>
        (from job in use(() => new EffectJob(work, owner))
         from engine in Products.Made(pipeline.ThreadEngine())
         from ran in IO.lift(() => RhinoFault.Unless(engine.RunPostEffect(job, pipeline, effect, rect, Channels.Required(channels))))
         select ran).Bracket();
}
