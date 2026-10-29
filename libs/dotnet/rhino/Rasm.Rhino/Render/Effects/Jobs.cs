using System.Drawing;
using CommunityToolkit.HighPerformance;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class EffectJob(Func<Rectangle, PostEffectJobChannels, IO<Unit>> work) : PostEffectJob {
    public override PostEffectJob Clone() => new EffectJob(work);

    public override bool Execute(Rectangle rect, PostEffectJobChannels access) => Session.Run(work(rect, access).Map(static _ => true), failed: false);

    // --- [RUNS]
    public static IO<Unit> Run<TState>(PostEffect effect, PostEffectPipeline pipeline, Rectangle rect, TState state, EffectDefinition<TState> definition) where TState : notnull =>
        definition.Threaded
            ? (from job in use(() => new EffectJob((area, access) => Pass(access.GetChannel, access.GetChannel, area, state, definition, commit: false)))
               from engine in Products.Of(pipeline.ThreadEngine)
               from ran in Products.Accepted(() => engine.RunPostEffect(job, pipeline, effect, rect, Channels.Required(definition.Channels)))
               select ran).Bracket()
            : Pass(pipeline.GetChannelForRead, pipeline.GetChannelForWrite, rect, state, definition, commit: true);

    private static IO<Unit> Pass<TState>(
        Func<Guid, PostEffectChannel?> read, Func<Guid, PostEffectChannel?> write, Rectangle rect, TState state, EffectDefinition<TState> definition, bool commit) where TState : notnull =>
        from rgba in Channels.Read(read, RenderWindow.StandardChannels.RGBA, rect)
        from inputs in definition.Channels.Filter(static channel => channel is not RenderWindow.StandardChannels.RGBA).Distinct()
            .TraverseM(channel => Channels.Read(read, channel, rect).Map(pixels => (channel, (ReadOnlyMemory2D<float>)pixels))).As()
        from ran in IO.lift(() => definition.Execute(state, new EffectFrame(rect, toHashMap(inputs), rgba)))
        from written in Channels.Write(write, rect, rgba, commit)
        select unit;
}
