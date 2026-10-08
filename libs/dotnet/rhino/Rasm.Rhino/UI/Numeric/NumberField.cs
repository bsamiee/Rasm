using System.Numerics;
using System.Runtime.CompilerServices;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rhino;
using Rhino.Input;
using UnitsNet;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NumberText<TKey>(
    Func<Option<TKey>, string> Shown, Func<TKey, string> Entry, Func<string, IO<TKey>> Read,
    Func<TKey, double, TKey> Moved, Func<TKey, TKey> Snapped, (TKey Low, TKey High) Ends) where TKey : notnull {
    public float TextWidth(Font font) =>
        Seq(Shown(Some(Ends.Low)), Shown(Some(Ends.High)), Shown(None)).Fold(0f, (widest, shown) => float.Max(widest, font.MeasureString(shown).Width));
}

public static class NumberText {
    public static NumberText<TKey> Scalar<TValue, TKey>(Presentation<TValue, TKey> presentation, string varies)
        where TValue : IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : struct, INumber<TKey> {
        ScalarDisplay display = Quantities.Scalar(presentation);
        (Func<TKey, string> number, Func<TKey, string> typed) = FieldTexts.Number(presentation);
        return new(
            key => key.Match(Some: number, None: () => varies),
            typed,
            entry => UnitText.ParseNumber(entry, StringParserSettings.DefaultParseSettings).Map(shown => NumericRows.Narrowed<TKey>(display.Key(shown))),
            (key, steps) => NumericRows.Narrowed<TKey>(double.CreateChecked(key) + (steps * display.Step)),
            key => NumericRows.Narrowed<TKey>(double.Round(double.CreateChecked(key) / display.Step, MidpointRounding.ToEven) * display.Step),
            presentation.Soft);
    }

    public static NumberText<Length> Distance(LengthUnit spaceUnit, DistanceDisplay display, (Length Low, Length High) ends, string varies) {
        Length step = display.Resolution(spaceUnit);
        return new(
            length => length.Match(Some: held => display.Format(held, spaceUnit, appendUnitSystemName: true), None: () => varies),
            length => display.Format(length, spaceUnit, appendUnitSystemName: true),
            entry => UnitText.ParseLength(entry, StringParserSettings.DefaultParseSettings, spaceUnit),
            (length, steps) => length + (step * steps),
            length => step * QuantityValue.Round(length / step, MidpointRounding.ToEven),
            ends);
    }
}

[Union]
internal abstract partial record FieldState<TValue> where TValue : notnull {
    public sealed record Resting(Option<TValue> Shown) : FieldState<TValue>;
    public sealed record Pressed(TValue Shown, PointF At) : FieldState<TValue>;
    public sealed record Scrubbing(TValue Shown, float Travel, PointF Last, Option<TValue> Moved) : FieldState<TValue>;
    public sealed record Editing(Option<TValue> Shown, Option<TValue> Typed) : FieldState<TValue>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class NumberField : TextBox {
    public static string HandlerStyle { get; } = ComponentControl.StyleName(typeof(NumberField));

    protected NumberField(IPlugInSink sink) {
        Sink = sink;
        Style = HandlerStyle;
    }

    public IPlugInSink Sink { get; }
    public event EventHandler<EventArgs>? ScrubStarted;
    public event EventHandler<EventArgs>? ScrubEnded;
    public abstract bool Adjust(int steps);

    protected IO<Unit> Started => IO.lift(() => ScrubStarted?.Invoke(this, EventArgs.Empty));

    protected IO<Unit> Ended => IO.lift(() => ScrubEnded?.Invoke(this, EventArgs.Empty));

    protected float Travel(MouseEventArgs move, PointF last) =>
        Optional(Platform.Instance.Find<Func<IPlugInSink, MouseEventArgs, PointF, float>>()).Match(Some: create => create()(Sink, move, last), None: () => move.Location.X - last.X);
}

public sealed class NumberField<TValue, TKey, TError> : NumberField
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
    where TKey : notnull, IComparable<TKey>
    where TError : Error, IValidationError<TError> {
    // --- [STATE]
    private readonly NumberText<TKey> text;
    private FieldState<TValue> state = new FieldState<TValue>.Resting(None);

    public NumberField(NumberText<TKey> text, IPlugInSink sink) : base(sink) =>
        (this.text, PlaceholderText, TextAlignment, Cursor) = (text, text.Shown(None), TextAlignment.Right, Cursors.VerticalSplit);

    public event EventHandler<EditEventArgs<TValue>>? Edited;

    public IO<Unit> Receive(Option<TValue> value) =>
        Current.Bind(held => held.Switch(
            (Field: this, Value: value),
            resting: static (s, _) => s.Field.Become(new FieldState<TValue>.Resting(s.Value), Some(s.Field.Showing(s.Value))),
            pressed: static (s, pressed) => s.Value.Match(
                Some: shown => s.Field.Become(pressed with { Shown = shown }, Some(s.Field.Showing(s.Value))),
                None: () => s.Field.Released(None)),
            scrubbing: static (s, scrubbing) => s.Value.Match(
                Some: shown => s.Field.Become(scrubbing with { Shown = shown }, Some(s.Field.Showing(scrubbing.Moved | s.Value))),
                None: () => s.Field.Unscrubbed(None, s.Field.Raised(new Edit<TValue>.Cancel()))),
            editing: static (s, _) => s.Field.Entered(s.Value)));

    public override bool Adjust(int steps) =>
        Callbacks.Succeeded(Stepping(state, steps, Keys.None), static () => false, new CallbackSite(Sink, GetType(), nameof(Adjust)));

    private IO<FieldState<TValue>> Current => IO.lift(() => state);

    private IO<Unit> Become(FieldState<TValue> next, Option<string> written) =>
        IO.lift(() => {
            (state, TextColor, Cursor) = (next, PaintSlot.ControlText.Read(), next.Map(
                resting: Cursors.VerticalSplit, pressed: Cursors.VerticalSplit, scrubbing: Cursors.VerticalSplit, editing: Cursors.IBeam));
            _ = written.Iter(shown => Text = shown);
        });

    private IO<Unit> Entered(Option<TValue> shown) =>
        Become(new FieldState<TValue>.Editing(shown, None), Some(Entering(shown))).Bind(_ => IO.lift(SelectAll));

    private IO<Unit> Pressing(TValue shown, MouseEventArgs e) =>
        from claimed in IO.lift(() => { e.Handled = true; })
        from pressed in Become(new FieldState<TValue>.Pressed(shown, e.Location), None)
        from captured in IO.lift(CaptureMouse)
        select unit;

    private IO<Unit> Released(Option<TValue> shown) =>
        Become(new FieldState<TValue>.Resting(shown), Some(Showing(shown))).Bind(_ => IO.lift(ReleaseMouseCapture));

    private IO<Unit> Unscrubbed(Option<TValue> shown, IO<Unit> raised) =>
        from released in Released(shown)
        from ended in Ended
        from edited in raised
        select edited;

    private IO<Unit> Interrupted =>
        Current.Bind(held => held.Switch(
            this,
            resting: static (_, _) => IO.pure(unit),
            pressed: static (owner, pressed) => owner.Released(Some(pressed.Shown)),
            scrubbing: static (owner, scrubbing) => owner.Unscrubbed(Some(scrubbing.Shown), owner.Raised(new Edit<TValue>.Cancel())),
            editing: static (_, _) => IO.pure(unit)));

    private Option<IO<Unit>> Stepping(FieldState<TValue> held, double steps, Keys modifiers) =>
        held.Switch(
            (Field: this, Steps: steps, Modifiers: modifiers),
            resting: static (s, resting) => resting.Shown.Bind(shown => s.Field.Stepped(shown, s.Steps, s.Modifiers).ToOption()).Map(stepped =>
                s.Field.Become(new FieldState<TValue>.Resting(Some(stepped)), Some(s.Field.Showing(Some(stepped)))).Bind(_ => s.Field.Raised(new Edit<TValue>.Step(stepped)))),
            pressed: static (_, _) => Option<IO<Unit>>.None,
            scrubbing: static (_, _) => Option<IO<Unit>>.None,
            editing: static (s, editing) => (editing.Typed | editing.Shown).Bind(shown => s.Field.Stepped(shown, s.Steps, s.Modifiers).ToOption()).Map(stepped =>
                from typed in s.Field.Become(editing with { Typed = Some(stepped) }, Some(s.Field.Entering(Some(stepped))))
                from selected in IO.lift(s.Field.SelectAll)
                from raised in s.Field.Raised(new Edit<TValue>.Step(stepped))
                select raised));

    // --- [VALUES]
    private string Showing(Option<TValue> value) => value.Match(Some: held => text.Shown(Some(held.ToValue())), None: static () => "");

    private string Entering(Option<TValue> value) => value.Match(Some: held => text.Entry(held.ToValue()), None: static () => "");

    private IO<Unit> Typed(FieldState<TValue>.Editing editing, string entry) =>
        text.Read(entry)
            .Bind(static key => IO.lift(Conversions.Validated<TValue, TKey, TError>(key)))
            .Bind(value => Accepted(editing, value))
            .Catch(static error => error.IsType<UnparsedText>() || error.IsType<TError>(), _ => Marked(PaintSlot.InvalidText));

    private Fin<TValue> Stepped(TValue shown, double steps, Keys modifiers) =>
        Conversions.Validated<TValue, TKey, TError>(Clamped(ComponentControl.Snaps(modifiers)
            ? text.Snapped(text.Moved(shown.ToValue(), steps))
            : text.Moved(shown.ToValue(), steps * ComponentControl.Scale(modifiers))));

    private static TKey Clamped(TKey key) =>
        key.CompareTo(TValue.MinValue.ToValue()) < 0 ? TValue.MinValue.ToValue()
        : key.CompareTo(TValue.MaxValue.ToValue()) > 0 ? TValue.MaxValue.ToValue()
        : key;

    private IO<Unit> Accepted(FieldState<TValue>.Editing editing, TValue value) =>
        Become(editing with { Typed = Some(value) }, None).Bind(_ => Raised(new Edit<TValue>.Preview(value)));

    private IO<Unit> Marked(PaintSlot slot) => IO.lift(() => { TextColor = slot.Read(); });

    private IO<Unit> Committed(Option<TValue> value) => Raised(value.Map<Edit<TValue>>(static held => new Edit<TValue>.Commit(held)));

    private IO<Unit> Raised(Option<Edit<TValue>> edit) => IO.lift(() => edit.Iter(held => Edited?.Invoke(this, new EditEventArgs<TValue>(held))));

    private static IO<Unit> Claimed(Action claim, IO<Unit> effect) => IO.lift(claim).Bind(_ => effect);

    // --- [INPUT]
    protected override void OnMouseDown(MouseEventArgs e) {
        Run(Current.Bind(held => held.Switch(
            (Field: this, Args: e),
            resting: static (s, resting) => resting.Shown.Filter(_ => s.Args.Buttons == MouseButtons.Primary && !s.Field.HasFocus)
                .Match(Some: shown => s.Field.Pressing(shown, s.Args), None: static () => IO.pure(unit)),
            pressed: static (_, _) => IO.pure(unit),
            scrubbing: static (s, _) => when(s.Args.Buttons == MouseButtons.Alternate, Claimed(() => s.Args.Handled = true, s.Field.Interrupted)).As(),
            editing: static (_, _) => IO.pure(unit))));
        if (e.Handled) return;

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        Run(Current.Bind(held => held.Switch(
            (Field: this, Args: e),
            resting: static (_, _) => IO.pure(unit),
            pressed: static (s, pressed) => when(
                PointF.Distance(pressed.At, s.Args.Location) > ComponentControl.DragThreshold,
                s.Field.Become(new FieldState<TValue>.Scrubbing(pressed.Shown, s.Field.Travel(s.Args, pressed.At), s.Args.Location, None), None).Bind(_ => s.Field.Started)).As(),
            scrubbing: static (s, scrubbing) =>
                from travel in IO.pure(scrubbing.Travel + s.Field.Travel(s.Args, scrubbing.Last))
                let moved = s.Field.Stepped(scrubbing.Shown, float.Truncate(travel), s.Args.Modifiers).ToOption().Filter(value => scrubbing.Moved != Some(value))
                from shown in s.Field.Become(scrubbing with { Travel = travel, Last = s.Args.Location, Moved = moved | scrubbing.Moved }, moved.Map(value => s.Field.Showing(Some(value))))
                from raised in s.Field.Raised(moved.Map<Edit<TValue>>(static value => new Edit<TValue>.Preview(value)))
                select raised,
            editing: static (_, _) => IO.pure(unit))));
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        Run(Current.Bind(held => held.Switch(
            this,
            resting: static (_, _) => IO.pure(unit),
            pressed: static (field, pressed) => field.Released(Some(pressed.Shown)).Bind(_ => IO.lift(field.Focus)),
            scrubbing: static (field, scrubbing) => field.Unscrubbed(scrubbing.Moved | Some(scrubbing.Shown), field.Committed(scrubbing.Moved)),
            editing: static (_, _) => IO.pure(unit))));
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        Run(Current.Bind(held => held.Switch(
            (Field: this, Args: e),
            resting: static (_, _) => IO.pure(unit),
            pressed: static (_, _) => IO.pure(unit),
            scrubbing: static (_, _) => IO.pure(unit),
            editing: static (s, editing) => Claimed(() => s.Args.Handled = true,
                s.Field.Stepping(editing, Math.Sign(s.Args.Delta.Height), s.Args.Modifiers).IfNone(IO.pure(unit))))));
        if (e.Handled) return;

        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        Run(Current.Bind(held => held.Switch(
            (Field: this, Args: e),
            resting: static (_, _) => IO.pure(unit),
            pressed: static (_, _) => IO.pure(unit),
            scrubbing: static (s, _) => when(s.Args.Key == Keys.Escape, Claimed(() => s.Args.Handled = true, s.Field.Interrupted)).As(),
            editing: static (s, editing) => s.Args.Key switch {
                Keys.Up => Claimed(() => s.Args.Handled = true, s.Field.Stepping(editing, 1, s.Args.Modifiers).IfNone(IO.pure(unit))),
                Keys.Down => Claimed(() => s.Args.Handled = true, s.Field.Stepping(editing, -1, s.Args.Modifiers).IfNone(IO.pure(unit))),
                Keys.Enter => Claimed(() => s.Args.Handled = true,
                    s.Field.Become(new FieldState<TValue>.Editing(editing.Typed | editing.Shown, None), None).Bind(_ => s.Field.Committed(editing.Typed))),
                Keys.Escape => Claimed(() => s.Args.Handled = true,
                    s.Field.Become(editing with { Typed = None }, None).Bind(_ => s.Field.Raised(new Edit<TValue>.Cancel()))),
                _ => IO.pure(unit),
            })));
        if (e.Handled) return;

        base.OnKeyDown(e);
    }

    protected override void OnTextChanging(TextChangingEventArgs e) {
        Run(Current.Bind(held => held.Switch(
            (Field: this, Args: e),
            resting: static (_, _) => IO.pure(unit),
            pressed: static (_, _) => IO.pure(unit),
            scrubbing: static (_, _) => IO.pure(unit),
            editing: static (s, editing) => when(s.Args.FromUser, s.Field.Typed(editing, s.Args.NewText)).As())));
        base.OnTextChanging(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        Run(Current.Bind(held => held.Switch(
            this,
            resting: static (field, resting) => field.Entered(resting.Shown),
            pressed: static (_, _) => IO.pure(unit),
            scrubbing: static (_, _) => IO.pure(unit),
            editing: static (_, _) => IO.pure(unit))));
    }

    protected override void OnLostFocus(EventArgs e) {
        Run(Current.Bind(held => held.Switch(
            this,
            resting: static (_, _) => IO.pure(unit),
            pressed: static (_, _) => IO.pure(unit),
            scrubbing: static (_, _) => IO.pure(unit),
            editing: static (field, editing) => (editing.Typed | editing.Shown) switch {
                var shown => field.Become(new FieldState<TValue>.Resting(shown), Some(field.Showing(shown))).Bind(_ => field.Committed(editing.Typed)),
            })));
        base.OnLostFocus(e);
    }

    // --- [LIFETIME]
    protected override void OnEnabledChanged(EventArgs e) {
        Run(Interrupted);
        base.OnEnabledChanged(e);
    }

    protected override void OnUnLoad(EventArgs e) {
        Run(Interrupted);
        base.OnUnLoad(e);
    }

    private void Run(IO<Unit> effect, [CallerMemberName] string member = "") =>
        _ = Callbacks.Answer(effect, static () => unit, new CallbackSite(Sink, GetType(), member));
}
