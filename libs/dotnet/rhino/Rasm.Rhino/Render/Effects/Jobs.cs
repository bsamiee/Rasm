using System.Drawing;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class JobEffect<TEffect, TState, TParameter, TError, TResource> : DefinedEffect<TEffect, TState, TParameter, TError>, IEffectDefinition
    where TEffect : JobEffect<TEffect, TState, TParameter, TError, TResource>, IEffectDefinition<TState, TParameter>
    where TState : IStateRecord<TState, TParameter, TError>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError>
    where TResource : notnull, IFrameJob<TResource, TState> {
    // --- [DEFINITION]
    static EffectKind IEffectDefinition.Kind { get; } =
        new(
            typeof(TEffect).GUID,
            EffectKind.Staged(typeof(TEffect)),
            TEffect.Row.Default,
            TEffect.Row.Amount,
            None,
            static doc => EffectStates.Binding<TEffect, TState, TParameter, TError>(doc, TEffect.Row));

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
    private protected sealed override Func<EtoPostEffectCollapsibleSection, Action<IDisposable>, IO<View.Section>> Section => SectionRows.Job(this, Held, ParameterText.Join<TState, TParameter, TError>(TEffect.Row.Text), TEffect.Row.Looks, work);

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TResource.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        CallbackSite.Of(this) switch {
            var site => Callbacks.Succeeded(
                from started in RenderRuns.Started(pipeline)
                from extent in IO.lift(EffectPipeline.Extent(pipeline.Dimensions()))
                let state = Held.Current
                let metres = RenderRuns.Find(pipeline.RenderingId).Bind(static run => run.Metres)
                from _ in Opening
                from queued in Queued(pipeline, rect, state, extent, metres, site)
                select queued,
                site),
        };

    private IO<Unit> Queued(PostEffectPipeline pipeline, Rectangle rect, TState state, PixelExtent extent, Option<float> metres, CallbackSite site) =>
        use(() => new Job(Rewrite(state, extent, metres, site)))
            .Bind(job => use(IO.lift(() => Missing.Unless(pipeline.ThreadEngine(), nameof(PostEffectPipeline.ThreadEngine))))
                .Bind(engine => IO.lift(() => Refused.Unless(
                    engine.RunPostEffect(job, pipeline, this, rect, EffectPipeline.Ids(TResource.Channels(state))), nameof(PostEffectThreadEngine.RunPostEffect))))
                .Bracket())
            .Bracket();

    // --- [JOBS]
    private Func<Rectangle, PostEffectJobChannels, bool> Rewrite(TState state, PixelExtent extent, Option<float> metres, CallbackSite site) =>
        (rect, access) => Callbacks.Succeeded(
            opened.Value.Bind(static held => held).Bind(static cell => cell.Held).Map(resource => (
                from rows in use(() => new WorkProgress(work, rect.Height))
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
