using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI;
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
    private readonly Ref<Option<Option<Disposal<TResource>>>> opened = Ref(Some(Option<Disposal<TResource>>.None));

    private readonly Atom<WorkState> work = Atom<WorkState>(new WorkState.Inactive());

    private IO<Unit> Opening =>
        IO.lift(() => opened.Value.Exists(static held => held.IsNone)).Bind(absent => when(absent,
            DisposalOps.Acquire(TResource.Open, (resource, release) => {
                Disposal<TResource> fresh = new(resource, _ => release.Dispose());
                bool accepted = atomic(() => {
                    Option<Option<Disposal<TResource>>> prior = opened.Value;
                    opened.Value = prior.Map(held => held | Some(fresh));
                    return prior.Exists(static held => held.IsNone);
                });
                if (!accepted) throw Errors.Cancelled.ToException();
            }).Map(unit)).As());

    public sealed override void Dispose(bool bDisposing) {
        if (bDisposing)
            Callbacks.Run(IO.lift(() => atomic(() => {
                Option<Option<Disposal<TResource>>> prior = opened.Value;
                opened.Value = None;
                return prior.Flatten().ToSeq();
            })).Bind(DisposalOps.Release), CallbackSite.Of(this));
        base.Dispose(bDisposing);
    }

    // --- [PIPELINE]
    private protected sealed override Func<EtoPostEffectCollapsibleSection, View.Section> Section => SectionRows.Job(this, Held, TEffect.Row.Looks, work);

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TResource.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        CallbackSite.Of(this) switch {
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
            opened.Value.Bind(static held => held).Bind(static cell => cell.Held).Map(resource => DisposalOps.Within(() => new WorkProgress(work, rect.Height), rows =>
                from started in work.SwapIO(static _ => new WorkState.Indeterminate())
                from color in EffectPipeline.Read(access.GetChannel, extent, RenderWindow.StandardChannels.RGBA, rect)
                from guides in EffectPipeline.Guides(access.GetChannel, extent, rect, TResource.Channels(state), meters)
                from result in resource.Run(state, color, guides, rows)
                from written in EffectPipeline.Opened(access.GetChannel, RenderWindow.ChannelId(RenderWindow.StandardChannels.RGBA), (_, cpu) => EffectPipeline.Put(cpu, result))
                select unit)),
            static () => false,
            site);

    private sealed class Job(Func<Rectangle, PostEffectJobChannels, bool> execute) : PostEffectJob {
        public override PostEffectJob Clone() => new Job(execute);

        public override bool Execute(Rectangle rect, PostEffectJobChannels access) => execute(rect, access);
    }
}
