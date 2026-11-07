using System.Drawing;
using CommunityToolkit.HighPerformance;
using Rhino.Render;
using Rhino.Render.PostEffects;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class EffectJob(Func<Rectangle, PostEffectJobChannels, IO<Unit>> work) : PostEffectJob {

    // --- [CALLBACKS]

    public override PostEffectJob Clone() => new EffectJob(work);

    public override bool Execute(Rectangle rect, PostEffectJobChannels access) => Session.Run(work(rect, access).Map(static _ => true), failed: false);

    // --- [EXECUTION]

    public static IO<Unit> Run<TState>(PostEffect effect, PostEffectPipeline pipeline, Rectangle rect, TState state, EffectDefinition<TState> definition) where TState : notnull =>
        from held in IO.lift(() => (Channels.Extent(pipeline), definition.Scope(state)) switch {
            var (extent, scope) => (Extent: extent, Area: scope.Area(extent, rect), scope.Parallel, Rendering: pipeline.IsRendering, Channels: definition.Channels(state)),
        })
        let execute = fun((Rectangle area, HashMap<RenderWindow.StandardChannels, ReadOnlyMemory2D<float>> inputs, Memory2D<float> rgba) =>
            definition.Execute(state, new EffectFrame(area, held.Extent, held.Rendering, inputs, rgba)))
        from ran in held.Parallel
            ? (from job in use(() => new EffectJob((area, access) => Pass(access.GetChannel, access.GetChannel, area, held.Channels, execute, commit: false)))
               from engine in Products.Of(pipeline.ThreadEngine)
               from ran in Products.Accepted(() => engine.RunPostEffect(job, pipeline, effect, held.Area, Channels.Required(held.Channels)))
               select ran).Bracket()
            : Pass(pipeline.GetChannelForRead, pipeline.GetChannelForWrite, held.Area, held.Channels, execute, commit: true)
        select ran;

    private static IO<Unit> Pass(
        Func<Guid, PostEffectChannel?> read, Func<Guid, PostEffectChannel?> write, Rectangle rect, Seq<RenderWindow.StandardChannels> channels,
        Func<Rectangle, HashMap<RenderWindow.StandardChannels, ReadOnlyMemory2D<float>>, Memory2D<float>, IO<Unit>> execute, bool commit) =>
        from rgba in Channels.Read(read, RenderWindow.StandardChannels.RGBA, rect)
        from inputs in channels.Filter(static channel => channel is not RenderWindow.StandardChannels.RGBA).Distinct()
            .TraverseM(channel => Channels.Offered(read, channel, rect).Map(pixels => pixels.Map(offered => (channel, (ReadOnlyMemory2D<float>)offered)))).As()
        from ran in execute(rect, toHashMap(inputs.Somes()), rgba)
        from written in Channels.Write(write, rect, rgba, commit)
        select unit;
}
