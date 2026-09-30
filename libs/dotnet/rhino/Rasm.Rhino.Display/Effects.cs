using System.Drawing;
using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Document;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Display;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ParameterEffect<TValue, TRaw> : PostEffect
    where TValue : IObjectFactory<TValue, TRaw, ValidationFailure>, IConvertible<TRaw>
    where TRaw : notnull {
    // --- [STATE]
    internal static readonly string Key = typeof(TValue).Name;

    private readonly TValue factory;

    private readonly Func<TValue, PostEffectPipeline, IO<PixelPass>> pass;

    private readonly Func<ParameterEffect<TValue, TRaw>, ParameterSection<TValue, TRaw>> section;

    private readonly Atom<TValue> held;

    protected ParameterEffect(TValue factory, Func<TValue, PostEffectPipeline, IO<PixelPass>> pass, Func<ParameterEffect<TValue, TRaw>, ParameterSection<TValue, TRaw>> section) {
        (this.factory, this.pass, this.section) = (factory, pass, section);
        held = Atom(factory);
    }

    public TValue Value => held.Value;

    // --- [CALLBACKS]
    public sealed override bool Execute(PostEffectPipeline pipeline, Rectangle rect) =>
        Answers.Succeeded(pass(held.Value, pipeline).Bind(formed => EffectPipeline.Rewrite(pipeline, rect, formed)), ErrorOps.Report);

    public sealed override bool GetParam(string param, ref object v) {
        if (!string.Equals(param, Key, StringComparison.Ordinal))
            return false;
        v = held.Value.ToValue();
        return true;
    }

    public sealed override bool SetParam(string param, object v) =>
        string.Equals(param, Key, StringComparison.Ordinal) && Written(IO.lift(() => Converted(v)));

    public sealed override bool ReadState(PostEffectState state) =>
        Answers.Succeeded(Swapped(IO.lift(() => state.TryGetValue(Key, out TRaw stored) ? Answers.Validated<TValue, TRaw>(stored) : factory)), ErrorOps.Report);

    public sealed override bool WriteState(ref PostEffectState state) =>
        state.SetValue(Key, held.Value.ToValue());

    public sealed override void ResetToFactoryDefaults() => _ = Written(IO.pure(factory));

    private bool Written(IO<TValue> next) =>
        Answers.Succeeded(
            IO.lift(() => BeginChange(RenderContent.ChangeContexts.Program))
                .Bracket(Use: _ => Swapped(next), Fin: _ => IO.lift(() => Refused.Unless(EndChange(), nameof(EndChange))))
                .Bind(_ => IO.lift(Changed)),
            ErrorOps.Report);

    private IO<Unit> Swapped(IO<TValue> next) =>
        next.Map(value => ignore(held.Swap(_ => value)));

    private static Fin<TValue> Converted(object raw) =>
        Answers.Validated<TValue, TRaw>((TRaw)Convert.ChangeType(raw, typeof(TRaw), CultureInfo.InvariantCulture));

    // --- [UI]
    public sealed override void AddUISections(PostEffectUI ui) => ui.AddSection(section(this));

    public sealed override bool DisplayHelp() => false;
}

public abstract class ParameterSection<TValue, TRaw> : EtoPostEffectCollapsibleSection
    where TValue : IObjectFactory<TValue, TRaw, ValidationFailure>, IConvertible<TRaw>
    where TRaw : notnull {
    private readonly ParameterEffect<TValue, TRaw> effect;

    protected ParameterSection(ParameterEffect<TValue, TRaw> effect) {
        this.effect = effect;
        DataChanged += (_, _) => UpdateBindings(BindingUpdateMode.Destination);
    }

    public sealed override Guid PostEffectId => effect.Id;

    public sealed override LocalizeStringPair Caption { get; } = new(ParameterEffect<TValue, TRaw>.Key, ParameterEffect<TValue, TRaw>.Key);

    public sealed override int SectionHeight => Content.Height;

    public sealed override bool Hidden => !(effect.IsSelected && (effect.Shown || effect.PostEffectType == PostEffectType.ToneMapping));

    protected void Write(TRaw raw) => _ = SetParameter(ParameterEffect<TValue, TRaw>.Key, raw);
}
