using System.Drawing;
using System.Reflection;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Views;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render.Effects;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IEffectRows<TState, TParameter> where TParameter : class, IStateParameter<TState> {
    public static abstract ParameterText Text(TParameter parameter);

    public static virtual Option<HelpTopic> Help => None;

    public static virtual Seq<(string Name, EffectLook<TState, TParameter> Look)> Looks => [];
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class EffectStyles {
    public const PostEffectStyles Listed =
        PostEffectStyles.ExecuteForProductionRendering | PostEffectStyles.ExecuteForRealtimeRendering | PostEffectStyles.DefaultShown | PostEffectStyles.DefaultOn;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EffectKind(Guid Id, PostEffectType Stage, Type State, Func<ValueSet, PassContext, IO<Option<PixelPass>>> Pass, Func<RhinoDoc, ValueBinding> Binding) {
    public Option<BuiltinEffect> Replaces { get; init; }

    public static EffectKind Of<TEffect, TState, TParameter, TError>()
        where TEffect : StageEffect<TEffect, TState, TParameter, TError>, IEffectRows<TState, TParameter>
        where TState : IStateRecord<TState, TParameter, TError>, IPixelStage<TState>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError> =>
        Staged(typeof(TEffect)) switch {
            var stage => new(
                typeof(TEffect).GUID,
                stage,
                typeof(TState),
                (set, context) => EffectStates.Recalled<TState>(EntryKey.TypeOwner(typeof(TEffect).GUID), set, EffectStates.Mixed(stage)).Map(value => EffectPipeline.Staged(value, context)),
                doc => EffectStates.Binding<TEffect, TState, TParameter, TError>(doc, EffectStates.Mixed(stage))),
        };

    public static EffectKind Of<TEffect, TState, TParameter, TError, TResource>()
        where TEffect : JobEffect<TEffect, TState, TParameter, TError, TResource>, IEffectRows<TState, TParameter>
        where TState : IStateRecord<TState, TParameter, TError>
        where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
        where TError : Error, IValidationError<TError>
        where TResource : notnull, IFrameJob<TResource, TState> =>
        new(
            typeof(TEffect).GUID,
            Staged(typeof(TEffect)),
            typeof(TState),
            static (_, _) => IO.pure(Option<PixelPass>.None),
            static doc => EffectStates.Binding<TEffect, TState, TParameter, TError>(doc, None));

    internal static PostEffectType Staged(Type effect) =>
        Optional(effect.GetCustomAttribute<CustomPostEffectAttribute>()).Map(static attribute => attribute.PostEffectType).ValueUnsafe();
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedEffect<TEffect, TState, TParameter, TError> : PostEffect
    where TEffect : DefinedEffect<TEffect, TState, TParameter, TError>, IEffectRows<TState, TParameter>
    where TState : IStateRecord<TState, TParameter, TError>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError> {
    // --- [STATE]
    private protected DefinedEffect(Option<Mix> amount) => Held = new(typeof(TEffect).GUID, amount);

    private protected EffectState<TState> Held { get; }

    private protected static Seq<(TParameter Parameter, ParameterText Text)> Texts => ParameterText.Join<TState, TParameter, TError>(TEffect.Text);

    // --- [PARAMETERS]
    public sealed override bool GetParam(string param, ref object v) {
        Option<string> text = Held.GetParam(param);
        v = text.Map(static held => (object)held).IfNone(v);
        return text.IsSome;
    }

    public sealed override bool SetParam(string param, object v) =>
        Callbacks.Succeeded(Held.SetParam(param, StoredText.Format(v)).Map(next => Changing(RenderContent.ChangeContexts.Program, next)), static () => false, CallbackSite.Of(this));

    public sealed override bool ReadState(PostEffectState state) =>
        Callbacks.Succeeded(Held.ReadState(state), CallbackSite.Of(this));

    public sealed override bool WriteState(ref PostEffectState state) =>
        Callbacks.Succeeded(Held.WriteState(state), CallbackSite.Of(this));

    public sealed override void ResetToFactoryDefaults() =>
        _ = Callbacks.Answer(Changing(RenderContent.ChangeContexts.Program, Held.ResetToFactoryDefaults()), static () => unit, CallbackSite.Of(this));

    private protected IO<Unit> Changing(RenderContent.ChangeContexts context, IO<Unit> next) =>
        IO.lift(() => BeginChange(context))
            .Bracket(Use: _ => next, Fin: _ => IO.lift(() => Refused.Unless(EndChange(), nameof(EndChange))))
            .Bind(_ => IO.lift(Changed));

    // --- [UI]
    private protected abstract Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Section { get; }

    public sealed override void AddUISections(PostEffectUI ui) =>
        _ = Callbacks.Answer(IO.lift(() => ui.AddSection(new DefinedEffectSection<TEffect>(this, Section))), static () => unit, CallbackSite.Of(this));

    public sealed override bool DisplayHelp() =>
        Callbacks.Succeeded(TEffect.Help.Map(static topic => topic.Show), static () => false, CallbackSite.Of(this));
}

public abstract class StageEffect<TEffect, TState, TParameter, TError> : DefinedEffect<TEffect, TState, TParameter, TError>
    where TEffect : StageEffect<TEffect, TState, TParameter, TError>, IEffectRows<TState, TParameter>
    where TState : IStateRecord<TState, TParameter, TError>, IPixelStage<TState>
    where TParameter : class, IStateParameter<TState>, ISmartEnum<string, TParameter, TError>
    where TError : Error, IValidationError<TError> {
    protected StageEffect() : base(EffectStates.Mixed(EffectKind.Staged(typeof(TEffect)))) { }

    private readonly Atom<StageFrame> input = Atom<StageFrame>(new StageFrame.Unread());

    private readonly Derivations derivations = new();

    private static Seq<EffectKind> Kinds => ((IPlugInRendering)PlugIn.Find(typeof(TEffect).Assembly)).Effects;

    private protected sealed override Func<EtoPostEffectCollapsibleSection, IO<View.Section>> Section => SectionRows.Stage(this, Held, Texts, TEffect.Looks, input, Changing);

    public sealed override Guid[] RequiredChannels => EffectPipeline.Ids(TState.Channels(Held.Current));

    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Callbacks.Succeeded(EffectPipeline.Run(pipeline, Kinds, rect, PostEffectType, Held.Value, input, derivations), CallbackSite.Of(this));

    public sealed override bool CanExecute(PostEffectPipeline pipeline) =>
        base.CanExecute(pipeline) && Callbacks.Answer(EffectPipeline.Runs(pipeline, Kinds, PostEffectType, Held.Value, input, derivations), static () => false, CallbackSite.Of(this));
}
