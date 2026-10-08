using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI.Chrome;
using Rasm.Rhino.UI.Views;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class JobEffect<TEffect, TState, TParameter, TError, TResource> : DefinedEffect<TEffect, TState, TParameter, TError>
    where TEffect : JobEffect<TEffect, TState, TParameter, TError, TResource>, IEffectRows<TState, TParameter>
    where TState : IStateRecord<TState, TParameter, TError>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError>
    where TResource : notnull, IFrameJob<TResource, TState> {
    // --- [RESOURCE]
    protected JobEffect() : base(None) { }

    private readonly Atom<Option<Disposal<TResource>>> opened = Atom(Option<Disposal<TResource>>.None);

    private readonly Atom<WorkState> work = Atom<WorkState>(new WorkState.Inactive());

    private IO<Unit> Opening =>
        IO.lift(() => opened.Value.IsNone).Bind(absent => when(absent,
            from resource in TResource.Open()
            let fresh = new Disposal<TResource>(resource, static held => held.Dispose())
            from _ in IO.lift(() => opened.Swap(held => held | Some(fresh)).Filter(kept => !ReferenceEquals(kept, fresh)).Iter(_ => fresh.Dispose()))
            select unit).As());

    public sealed override void Dispose(bool bDisposing) {
        if (bDisposing)
            _ = Callbacks.Answer(DisposalOps.Release(opened.Value.ToSeq()), static () => unit, CallbackSite.Of(this));
        base.Dispose(bDisposing);
    }

    // --- [PIPELINE]
    private protected sealed override Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Section => SectionRows.Job(this, Held, Texts, TEffect.Looks, work, Changing);

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TResource.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        CallbackSite.Of(this) switch {
            var site => Callbacks.Succeeded(
                from extent in IO.lift(EffectPipeline.Extent(pipeline.Dimensions()))
                let state = Held.Current
                let metres = RenderRuns.Find(pipeline.RenderingId).Bind(static run => run.Metres)
                from _ in Opening
                from queued in use(() => new Job(Rewrite(state, extent, metres, site)))
                    .Bind(job => use(IO.lift(() => Missing.Unless(pipeline.ThreadEngine(), nameof(PostEffectPipeline.ThreadEngine))))
                        .Bind(engine => IO.lift(() => Refused.Unless(
                            engine.RunPostEffect(job, pipeline, this, rect, EffectPipeline.Ids(TResource.Channels(state))), nameof(PostEffectThreadEngine.RunPostEffect))))
                        .Bracket())
                    .Bracket()
                select queued,
                site),
        };

    // --- [JOBS]
    private Func<Rectangle, PostEffectJobChannels, bool> Rewrite(TState state, PixelExtent extent, Option<float> metres, CallbackSite site) =>
        (rect, access) => Callbacks.Succeeded(
            opened.Value.Bind(static cell => cell.Held).Map(resource => (
                from rows in use(() => new WorkProgress(work, rect.Height, site))
                from _ in work.SwapIO(static _ => new WorkState.Indeterminate())
                from color in EffectPipeline.Read(access.GetChannel, extent, RenderWindow.StandardChannels.RGBA, rect)
                from guides in EffectPipeline.Guides(access.GetChannel, extent, rect, TResource.Channels(state), metres)
                from result in resource.Run(state, color, guides, rows)
                from written in EffectPipeline.Opened(access.GetChannel, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (_, cpu) => EffectPipeline.Put(cpu, result))
                select unit).Bracket()),
            static () => false,
            site);

    private sealed class Job(Func<Rectangle, PostEffectJobChannels, bool> execute) : PostEffectJob {
        public override PostEffectJob Clone() => new Job(execute);

        public override bool Execute(Rectangle rect, PostEffectJobChannels access) => execute(rect, access);
    }
}
