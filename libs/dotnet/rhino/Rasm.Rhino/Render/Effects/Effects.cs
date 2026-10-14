using System.Drawing;
using System.Reflection;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rhino;
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
    public static abstract EffectRow<TState, TParameter> Row { get; }
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class EffectStyles {
    public const PostEffectStyles Listed =
        PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectKind(Guid Id, PostEffectType Stage, object Default, Option<Mix> Amount, Option<Func<ValueSet, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>>> Pass, Func<RhinoDoc, ValueBinding> Binding) {
    public Type State => Default.GetType();

    public Option<BuiltinEffect> Replaces { get; init; }

    internal static PostEffectType Staged(Type effect) =>
        Optional(effect.GetCustomAttribute<CustomPostEffectAttribute>()).Map(static attribute => attribute.PostEffectType).ValueUnsafe();
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TEffect, TState, TParameter, TError> : PostEffect
    where TEffect : DefinedEffect<TEffect, TState, TParameter, TError>, IEffectDefinition<TState, TParameter>
    where TState : IStateRecord<TState, TParameter, TError>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError> {
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
            _ => IO.lift(EndChange).Map(static _ => unit));

    // --- [UI]
    private protected abstract Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Section { get; }

    public sealed override void AddUISections(PostEffectUI ui) =>
        Callbacks.Run(DisposalOps.Acquire(
            IO.lift(() => new DefinedEffectSection<TEffect>(this, Section)), static section => IO.lift(section.Dispose), ui.AddSection),
            CallbackSite.Of(this));

    public sealed override bool DisplayHelp() =>
        Callbacks.Succeeded(TEffect.Row.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));
}

public abstract class StageEffect<TEffect, TState, TParameter, TError> : DefinedEffect<TEffect, TState, TParameter, TError>, IEffectDefinition
    where TEffect : StageEffect<TEffect, TState, TParameter, TError>, IEffectDefinition<TState, TParameter>
    where TState : IStateRecord<TState, TParameter, TError>, IPixelStage<TState>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError> {
    // --- [DEFINITION]
    static EffectKind IEffectDefinition.Kind { get; } =
        new(
            typeof(TEffect).GUID,
            EffectKind.Staged(typeof(TEffect)),
            TEffect.Row.Default,
            TEffect.Row.Amount,
            Some<Func<ValueSet, PassContext, IO<(Option<PixelPass> Pass, PassContext Context)>>>(static (set, context) => EffectStates.Recalled(EntryKey.TypeOwner(typeof(TEffect).GUID), set, TEffect.Row.Amount, TEffect.Row.Default)
                .Map(value => EffectPipeline.Staged(value, context) switch {
                    var pass => (pass, pass.IsNone ? context : value.Record switch {
                        ExposureState exposed => context with { Exposure = context.Exposure + Exposure.Applied(exposed.Exposure.Scale, (float)value.Amount.IfNone(Mix.Full)) },
                        ToneMapping => context with { Signal = context.Display.Encoding.Transfer },
                        _ => context,
                    }),
                })),
            static doc => EffectStates.Binding<TEffect, TState, TParameter, TError>(doc, TEffect.Row));

    // --- [STATE]
    private readonly Atom<StageFrame> input = Atom<StageFrame>(new StageFrame.Unread());

    private readonly Derivations derivations = new();

    public sealed override void Dispose(bool bDisposing) {
        try {
            if (bDisposing) derivations.Dispose();
        } finally {
            base.Dispose(bDisposing);
        }
    }

    private protected sealed override Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Section => SectionRows.Stage(this, Held, ParameterText.Join<TState, TParameter, TError>(TEffect.Row.Text), TEffect.Row.Looks, input);

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TState.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Callbacks.Succeeded(
            IO.lift(() => EffectRegistry.Kinds.Of(IPlugInSink.Of(this))).Bind(kinds => EffectPipeline.Run(pipeline, Id, kinds, rect, PostEffectType, Held.Value, input, derivations)),
            CallbackSite.Of(this));

    public sealed override bool CanExecute(PostEffectPipeline pipeline) =>
        base.CanExecute(pipeline) && Callbacks.Answer(EffectPipeline.Runs(pipeline, PostEffectType, Held.Value, input, derivations), static () => false, CallbackSite.Of(this));
}
