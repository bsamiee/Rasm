using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IEffectDefinition {
    public static abstract EffectKind Kind { get; }
}

public interface IEffectDefinition<TState, TParameter> : IEffectDefinition
    where TState : IStateRecord<TState>
    where TParameter : class, IStateParameter<TState> {
    public static virtual EffectRow<TState, TParameter> Row => new();
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class EffectStyles {
    public const PostEffectStyles Listed =
        PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectKind(Guid Id, PostEffectType Stage, Type State, Option<Func<ValueSet, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>>> Pass, Func<EffectSource, Seq<ValueBinding>> Binding) {
    public Option<BuiltinEffect> Replaces { get; init; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TEffect, TState, TParameter> : PostEffect
    where TEffect : DefinedEffect<TEffect, TState, TParameter>, IEffectDefinition<TState, TParameter>
    where TState : IStateRecord<TState, TParameter>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, ValidationError> {
    // --- [DEFINITION]
    private protected static EffectKind Defined(Option<Func<ValueSet, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>>> pass) =>
        new(typeof(TEffect).GUID, typeof(TEffect).GetCustomAttribute<CustomPostEffectAttribute>()?.PostEffectType ?? throw new UnreachableException(), typeof(TState), pass,
            static source => EffectStates.Binding<TEffect, TState, TParameter>(source, TEffect.Row));

    // --- [STATE]
    private protected DefinedEffect() =>
        Held = new EffectState<TState>(typeof(TEffect).GUID, TEffect.Row.Default, TEffect.Row.Amount);

    private protected EffectState<TState> Held { get; }

    // --- [PARAMETERS]
    public sealed override bool GetParam(string param, ref object v) {
        Option<string> text = Held.GetParam(param);
        v = text.Map(static held => (object)held).IfNone(v);
        return text.IsSome;
    }

    public sealed override bool SetParam(string param, object v) =>
        Callbacks.Succeeded(Held.SetParam(param, StoredText.Format(v), next => Changing(RenderContent.ChangeContexts.Program, next)), static () => false, CallbackSite.Of(this));

    public sealed override bool ReadState(PostEffectState state) =>
        Callbacks.Succeeded(Held.ReadState(state), CallbackSite.Of(this));

    public sealed override bool WriteState(ref PostEffectState state) =>
        Callbacks.Succeeded(Held.WriteState(state), CallbackSite.Of(this));

    public sealed override void ResetToFactoryDefaults() =>
        Callbacks.Run(Changing(RenderContent.ChangeContexts.Program, Held.ResetToFactoryDefaults()), CallbackSite.Of(this));

    private IO<Unit> Changing(RenderContent.ChangeContexts context, IO<Unit> next) =>
        DisposalOps.Within(() => { BeginChange(context); return unit; },
            _ => next.Bind(_ => IO.lift(Changed)),
            _ => IO.lift(EndChange).Map(unit));

    // --- [UI]
    private protected abstract Func<EtoPostEffectCollapsibleSection, View.Section> Section { get; }

    private protected virtual (Option<Atom<StageFrame>> Watched, IO<Option<ScopeFrames>> Scoped) Frames => (None, IO.pure(Option<ScopeFrames>.None));

    public sealed override void AddUISections(PostEffectUI ui) =>
        Callbacks.Run(Frames switch {
            var (watched, scoped) => Callbacks.Each((Seq(Section) + TEffect.Row.Sections.Map(extra => fun((EtoPostEffectCollapsibleSection section) => extra(section, scoped))))
                .Map(row => DisposalOps.Acquire(IO.lift(() => new DefinedEffectSection<TEffect>(this, row, watched)), ui.AddSection))).Map(unit),
        }, CallbackSite.Of(this));

    public sealed override bool DisplayHelp() =>
        Callbacks.Succeeded(TEffect.Row.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));
}

public abstract class StageEffect<TEffect, TState, TParameter> : DefinedEffect<TEffect, TState, TParameter>, IEffectDefinition<TState, TParameter>
    where TEffect : StageEffect<TEffect, TState, TParameter>, IEffectDefinition<TState, TParameter>
    where TState : IStateRecord<TState, TParameter>, IPixelStage<TState>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, ValidationError> {
    // --- [DEFINITION]
    static EffectKind IEffectDefinition.Kind { get; } =
        Defined(Some<Func<ValueSet, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>>>(static (set, context) =>
            from value in EffectStates.Recalled(EntryKey.TypeOwner(typeof(TEffect).GUID), set, TEffect.Row.Amount, TEffect.Row.Default)
            let pass = EffectPipeline.Staged(value, context)
            select (pass, pass.IsNone ? context : value.Record switch {
                ExposureState exposed => context with { Exposure = context.Exposure + Exposure.Applied(exposed.Exposure.Scale, (float)value.Amount.IfNone(AxisFraction.MaxValue)) },
                ToneMapping => context with { Signal = context.Display.Encoding.Transfer },
                _ => context,
            })));

    // --- [STATE]
    private readonly Atom<StageFrame> input = Atom(StageFrame.Unwatched);

    private readonly Derivations derivations = new();

    public sealed override void Dispose(bool bDisposing) {
        try {
            if (bDisposing) derivations.Dispose();
        } finally {
            base.Dispose(bDisposing);
        }
    }

    private protected sealed override Func<EtoPostEffectCollapsibleSection, View.Section> Section => SectionRows.Stage(this, Held, TEffect.Row.Looks, input);

    private protected sealed override (Option<Atom<StageFrame>> Watched, IO<Option<ScopeFrames>> Scoped) Frames => (Some(input), SectionRows.Scoped(PostEffectType, Held, input));

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TState.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Callbacks.Succeeded(
            EffectRegistry.Kinds.Of(IPlugInSink.Of(this)).Bind(kinds => EffectPipeline.Run(pipeline, Id, kinds, rect, PostEffectType, Held.Value, input, derivations)),
            CallbackSite.Of(this));

    public sealed override bool CanExecute(PostEffectPipeline pipeline) =>
        base.CanExecute(pipeline) && Callbacks.Answer(EffectPipeline.Runs(pipeline, PostEffectType, Held.Value, input, derivations), static () => false, CallbackSite.Of(this));
}
