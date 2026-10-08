using System.Numerics;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI.Components;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
internal readonly record struct SliderGrip<TValue, TKey>(double Pressed, (TKey Low, TKey High) Track, Option<TValue> Open)
    where TValue : notnull where TKey : struct, INumber<TKey>;

internal readonly record struct ParameterSliderState<TValue, TKey>(Option<TValue> Shown, Option<SliderGrip<TValue, TKey>> Grip)
    where TValue : notnull where TKey : struct, INumber<TKey>;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ParameterSlider : Slider {
    protected const int TrackSteps = 1_000_000;
    protected const int TickIntervals = 10;

    public static string HandlerStyle { get; } = ComponentControl.StyleName(typeof(ParameterSlider));

    protected ParameterSlider(IPlugInSink sink) {
        Sink = sink;
        Style = HandlerStyle;
        MaxValue = TrackSteps;
        TickFrequency = TrackSteps / TickIntervals;
    }

    public IPlugInSink Sink { get; }
    public abstract bool Mixed { get; }
    public abstract Option<double> Origin { get; }
    public abstract string ValueText { get; }
    public abstract Option<Vector4> Fill(double position);
    public abstract bool Adjust(int steps);
}

public sealed class ParameterSlider<TValue, TKey, TError> : ParameterSlider
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
    where TKey : struct, INumber<TKey>
    where TError : Error, IValidationError<TError> {
    // --- [STATE]
    private readonly Presentation<TValue, TKey> presentation;
    private readonly NumberText<TKey> text;
    private ParameterSliderState<TValue, TKey> state = new(None, None);

    public ParameterSlider(Presentation<TValue, TKey> presentation, NumberText<TKey> text, IPlugInSink sink) : base(sink) =>
        (this.presentation, this.text) = (presentation, text);

    public IO<Unit> Receive(Option<TValue> value) => IO.lift(() => state).Bind(held => Placed(held with { Shown = value }));

    private IO<Unit> Placed(ParameterSliderState<TValue, TKey> next) =>
        IO.lift(() => {
            state = next;
            Value = Position(next);
        });

    private IO<Unit> Moved(ParameterSliderState<TValue, TKey> held, TKey key, Func<TValue, Edit<TValue>> edit, Func<TValue, ParameterSliderState<TValue, TKey>> next) =>
        held.Shown.Exists(shown => shown.ToValue() == key)
            ? Placed(held)
            : (from value in IO.lift(Conversions.Validated<TValue, TKey, TError>(key))
               from _ in Placed(next(value))
               from __ in Raised(edit(value))
               select unit)
                .Catch(static error => error.IsType<TError>(), _ => Placed(held));

    private IO<Unit> Previewed(ParameterSliderState<TValue, TKey> held, SliderGrip<TValue, TKey> grip, TKey key) =>
        Moved(held with { Grip = grip }, key, static value => new Edit<TValue>.Preview(value), value => held with { Shown = value, Grip = grip with { Open = value } });

    private IO<Unit> Stepping(ParameterSliderState<TValue, TKey> held, Func<TKey, (TKey Low, TKey High), TKey> next) =>
        held.Shown.Match(
            Some: shown => Moved(held, next(shown.ToValue(), Track(held)), static value => new Edit<TValue>.Step(value), value => held with { Shown = value }),
            None: () => Placed(held));

    private IO<Unit> Released(ParameterSliderState<TValue, TKey> held, Func<TValue, Edit<TValue>> close) =>
        held.Grip.Match(
            Some: grip => Placed(held with { Grip = None }).Bind(_ => grip.Open.Match(Some: open => Raised(close(open)), None: static () => IO.pure(unit))),
            None: static () => IO.pure(unit));

    // --- [EDITS]
    public event EventHandler<Edit<TValue>>? Edited;

    private IO<Unit> Raised(Edit<TValue> edit) => IO.lift(() => Edited?.Invoke(this, edit));

    private static Edit<TValue> Cancelled(TValue _) => new Edit<TValue>.Cancel();

    // --- [TRACK]
    private double Native => (double)Value / TrackSteps;

    private (TKey Low, TKey High) Track(ParameterSliderState<TValue, TKey> held) =>
        held.Grip.Match(
            Some: static grip => grip.Track,
            None: () => held.Shown.Match(Some: shown => presentation.Track(shown.ToValue()), None: () => presentation.Soft));

    private int Position(ParameterSliderState<TValue, TKey> held) =>
        held.Shown.Match(Some: shown => (int)Math.Round(presentation.Position(shown.ToValue(), Track(held)) * TrackSteps, MidpointRounding.ToEven), None: static () => 0);

    private TKey Keyed(double position, (TKey Low, TKey High) track) =>
        NumericRows.Narrowed<TKey>(presentation.Key(position, track));

    private TKey Dragged(SliderGrip<TValue, TKey> grip, double raw, Keys modifiers) =>
        ComponentControl.Snaps(modifiers)
            ? TKey.Clamp(text.Snapped(Keyed(raw, grip.Track)), grip.Track.Low, grip.Track.High)
            : Keyed(grip.Pressed + ((raw - grip.Pressed) * ComponentControl.Scale(modifiers)), grip.Track);

    private TKey Stepped(TKey from, double steps, Keys modifiers, (TKey Low, TKey High) track) =>
        TKey.Clamp(
            ComponentControl.Snaps(modifiers) ? text.Snapped(text.Moved(from, steps)) : text.Moved(from, steps * ComponentControl.Scale(modifiers)),
            track.Low,
            track.High);

    // --- [INPUT]
    protected override void OnValueChanged(EventArgs e) {
        _ = Run(held => unless(Value == Position(held), Changed(held, Native)).As());
        base.OnValueChanged(e);
    }

    protected override void OnMouseDown(MouseEventArgs e) {
        _ = Run(held => Pressing(held, e));
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        _ = Run(held => Released(held, e.Buttons == MouseButtons.None ? Cancelled : static open => new Edit<TValue>.Commit(open)));
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        _ = Run(held => Claimed(
            HasFocus && e.Delta.Height != 0 ? Some(Stepping(held, (from, track) => Stepped(from, Math.Sign(e.Delta.Height), e.Modifiers, track))) : None,
            () => e.Handled = true));
        if (e.Handled) return;

        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        _ = Run(held => Claimed(Keying(held, e), () => e.Handled = true));
        if (e.Handled) return;

        base.OnKeyDown(e);
    }

    private IO<Unit> Changed(ParameterSliderState<TValue, TKey> held, double raw) =>
        held.Grip.Match(
            Some: grip => grip.Open.IsNone && Math.Abs(raw - grip.Pressed) * Width <= ComponentControl.DragThreshold
                ? Placed(held)
                : Previewed(held, grip, Dragged(grip, raw, Keyboard.Modifiers)),
            None: () => Stepping(held, (_, track) => Keyed(raw, track)));

    private IO<Unit> Pressing(ParameterSliderState<TValue, TKey> held, MouseEventArgs e) =>
        (held.Grip.IsSome, e.Buttons == MouseButtons.Primary) switch {
            (false, true) => Gripped(held, Native, e.Location.X / Width),
            (true, false) => Released(held, Cancelled),
            _ => IO.pure(unit),
        };

    private IO<Unit> Gripped(ParameterSliderState<TValue, TKey> held, double knob, double press) =>
        IO.lift(Focus).Bind(_ => Math.Abs(press - knob) * Width > ComponentControl.DragThreshold
            ? new SliderGrip<TValue, TKey>(press, Track(held), None) switch { var grip => Previewed(held, grip, Keyed(press, grip.Track)) }
            : Placed(held with { Grip = new SliderGrip<TValue, TKey>(knob, Track(held), None) }));

    private Option<IO<Unit>> Keying(ParameterSliderState<TValue, TKey> held, KeyEventArgs e) =>
        e.Key switch {
            Keys.Escape when held.Grip.Exists(static grip => grip.Open.IsSome) => Released(held, Cancelled),
            Keys.Right or Keys.Up or Keys.Left or Keys.Down => Stepping(held, (from, track) => Stepped(from, e.Key is Keys.Right or Keys.Up ? 1 : -1, e.Modifiers, track)),
            Keys.PageUp or Keys.PageDown => Stepping(held, (from, track) => Keyed(presentation.Position(from, track) + ((e.Key == Keys.PageUp ? 1d : -1d) / TickIntervals), track)),
            Keys.Home or Keys.End => Stepping(held, (_, track) => e.Key == Keys.Home ? track.Low : track.High),
            _ => None,
        };

    private static IO<Unit> Claimed(Option<IO<Unit>> answer, Action claim) =>
        answer.Match(Some: step => IO.lift(claim).Bind(_ => step), None: static () => IO.pure(unit));

    // --- [PLATFORM]
    public override bool Mixed => state.Shown.IsNone;
    public override Option<double> Origin => presentation.Origin.Map(origin => presentation.Position(origin, Track(state)));
    public override string ValueText => text.Shown(state.Shown.Map(static shown => shown.ToValue()));
    public override Option<Vector4> Fill(double position) => presentation.Fill(presentation.Key(position, Track(state)));

    public override bool Adjust(int steps) =>
        !Mixed && Run(held => Stepping(held, (from, track) => Stepped(from, steps, Keys.None, track)));

    // --- [LIFETIME]
    protected override void OnLostFocus(EventArgs e) {
        _ = Run(held => Released(held, Cancelled));
        base.OnLostFocus(e);
    }

    protected override void OnEnabledChanged(EventArgs e) {
        _ = Run(held => unless(Enabled, Released(held, Cancelled)).As());
        base.OnEnabledChanged(e);
    }

    protected override void OnUnLoad(EventArgs e) {
        _ = Run(held => Released(held, Cancelled));
        base.OnUnLoad(e);
    }

    private bool Run(Func<ParameterSliderState<TValue, TKey>, IO<Unit>> transition, [CallerMemberName] string member = "") =>
        Callbacks.Succeeded(IO.lift(() => state).Bind(transition), new CallbackSite(Sink, GetType(), member));
}
