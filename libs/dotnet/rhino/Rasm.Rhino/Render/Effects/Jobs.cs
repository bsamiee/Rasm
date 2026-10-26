using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class JobEffect<TEffect, TState, TParameter, TResource> : DefinedEffect<TEffect, TState, TParameter>, IEffectDefinition<TState, TParameter>
    where TEffect : JobEffect<TEffect, TState, TParameter, TResource>, IEffectDefinition<TState, TParameter>
    where TState : IStateRecord<TState, TParameter>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, ValidationError>
    where TResource : notnull, IFrameJob<TResource, TState> {
    // --- [DEFINITION]
    static EffectKind IEffectDefinition.Kind { get; } = Defined(None);

    // --- [RESOURCE]
    private readonly Retained<Disposal<TResource>> opened;

    private readonly Atom<Content<Unit>> work = Atom<Content<Unit>>(new Content<Unit>.Empty("Effect has not run"));

    protected JobEffect() =>
        opened = new(held => DisposalOps.Release(held).IfFail(error => IO.lift(() => Owner.Report(error, typeof(TResource), nameof(IDisposable.Dispose)))));

    private IO<Unit> Opening =>
        IO.lift(() => opened.Held.Exists(static held => held.IsEmpty)).Bind(absent => when(absent,
            DisposalOps.Acquire(TResource.Open, (resource, lease) => opened.Admit(new Disposal<TResource>(resource, _ => lease.Dispose()))).Map(unit)).As());

    public sealed override void Dispose(bool bDisposing) {
        if (bDisposing) opened.Dispose();
        base.Dispose(bDisposing);
    }

    // --- [PIPELINE]
    private protected sealed override Func<EtoPostEffectCollapsibleSection, View.Section> Section => SectionRows.Job(this, Held, TEffect.Row.Looks, work);

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TResource.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        CallbackSite.Of(Owner, this) switch {
            var site => Callbacks.Succeeded(
                from started in RenderRuns.Started(pipeline)
                from extent in IO.lift(EffectPipeline.Extent(pipeline.Dimensions()))
                let state = Held.Current
                let meters = RenderRuns.Find(pipeline.RenderingId).Bind(static run => run.Meters)
                from _ in Opening
                from queued in Queued(pipeline, rect, state, extent, meters, site)
                select queued,
                site),
        };

    private IO<Unit> Queued(PostEffectPipeline pipeline, Rectangle rect, TState state, PixelExtent extent, Option<float> meters, CallbackSite site) =>
        DisposalOps.Within(() => new Job(Rewrite(state, extent, meters, site)), job =>
            DisposalOps.Found(pipeline.ThreadEngine, nameof(PostEffectPipeline.ThreadEngine), engine => IO.lift(() => Refused.Unless(
                engine.RunPostEffect(job, pipeline, this, rect, EffectPipeline.Ids(TResource.Channels(state))), nameof(PostEffectThreadEngine.RunPostEffect)))));

    // --- [JOBS]
    private Func<Rectangle, PostEffectJobChannels, bool> Rewrite(TState state, PixelExtent extent, Option<float> meters, CallbackSite site) =>
        (rect, access) => Callbacks.Succeeded(
            opened.Held.ToOption().Bind(static held => held.Head).Bind(static cell => cell.Held).Map(resource =>
                from started in work.SwapIO(static held => held.Requested(None))
                from arrival in (
                    from color in EffectPipeline.Read(access.GetChannel, extent, RenderWindow.StandardChannels.RGBA, rect)
                    from guides in EffectPipeline.Guides(access.GetChannel, extent, rect, TResource.Channels(state), meters)
                    from result in resource.Run(state, color, guides, new WorkProgress<Unit>(work, rect.Height))
                    from written in EffectPipeline.Opened(access.GetChannel, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (_, cpu) => EffectPipeline.Put(cpu, result))
                    select unit).Try().Run().As()
                from arrived in work.SwapIO(_ => Content<Unit>.Arrived(arrival))
                from settled in IO.lift(arrival)
                select settled),
            static () => false,
            site);

    private sealed class Job(Func<Rectangle, PostEffectJobChannels, bool> execute) : PostEffectJob {
        public override PostEffectJob Clone() => new Job(execute);

        public override bool Execute(Rectangle rect, PostEffectJobChannels access) => execute(rect, access);
    }
}
