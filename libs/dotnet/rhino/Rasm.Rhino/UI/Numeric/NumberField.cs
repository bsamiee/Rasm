using System.Numerics;
using System.Runtime.CompilerServices;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI.Styling;
using Rhino;
using Rhino.Input;
using UnitsNet;

namespace Rasm.Rhino.UI.Numeric;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate Fin<float> PointerTravel(PointF at, PointF last);

// --- [MODELS] --------------------------------------------------------------------------
public abstract class NumberText<TKey> where TKey : notnull {
    public abstract (TKey Low, TKey High) Ends { get; }

    public abstract string Shown(Option<TKey> key);

    public abstract string Entry(TKey key);

    public abstract IO<TKey> Read(string entry);

    public abstract TKey Moved(TKey key, double steps);

    public abstract TKey Snapped(TKey key, double steps);

    public float TextWidth(Font font) =>
        Seq(Shown(Some(Ends.Low)), Shown(Some(Ends.High)), Shown(None)).Fold(0f, (widest, shown) => float.Max(widest, font.MeasureString(shown).Width));
}

public sealed class ScalarText<TValue, TKey> : NumberText<TKey> where TValue : notnull where TKey : struct, INumber<TKey> {
    private readonly (Func<TKey, string> Shown, Func<TKey, string> Entry) number;
    private readonly double stride;
    private readonly string varies;

    public ScalarText(NumericDomain<TValue, TKey> domain, string varies) {
        (Domain, Display, Ends, this.varies) = (domain, Quantities.Scalar(domain.Presentation, domain.Bounds), domain.Presentation.Soft.IfNone(domain.Bounds), varies);
        number = FieldTexts.Number(domain.Presentation, domain.Bounds);
        stride = Display.Step / (double.CreateChecked(Ends.High) - double.CreateChecked(Ends.Low));
    }

    public NumericDomain<TValue, TKey> Domain { get; }
    public ScalarDisplay Display { get; }
    public override (TKey Low, TKey High) Ends { get; }

    public override string Shown(Option<TKey> key) => key.Match(Some: number.Shown, None: () => varies);

    public override string Entry(TKey key) => number.Entry(key);

    public override IO<TKey> Read(string entry) =>
        UnitText.ParseNumber(entry, StringParserSettings.DefaultParseSettings).Map(shown => NumericRows.Narrowed<TKey>(Display.Key(shown)));

    public override TKey Moved(TKey key, double steps) =>
        Domain.Presentation.Scale == TrackScale.Linear || TKey.IsInteger(TKey.CreateSaturating(0.5))
            ? NumericRows.Narrowed<TKey>(double.CreateChecked(key) + (steps * Display.Step))
            : Key(Domain.Presentation.Position(key, Ends) + (steps * stride), Ends);

    public override TKey Snapped(TKey key, double steps) => Snap(key, steps, Track(key));

    public TKey Snap(TKey key, double steps, (TKey Low, TKey High) track) =>
        Domain.Presentation.Scale == TrackScale.Linear
            ? NumericRows.Narrowed<TKey>(double.Round((double.CreateChecked(key) + (steps * Display.Step)) / Display.Step, MidpointRounding.ToEven) * Display.Step)
            : Key(double.Clamp((double.Round(Domain.Presentation.Position(key, track) * NumberText.TickIntervals, MidpointRounding.ToEven) + steps) / NumberText.TickIntervals, 0d, 1d), track);

    public (TKey Low, TKey High) Track(TKey key) => Domain.Presentation.Track(key, Domain.Bounds);

    public TKey Key(double position, (TKey Low, TKey High) track) => NumericRows.Narrowed<TKey>(Domain.Presentation.Key(position, track, Domain.Bounds));
}

internal sealed class DistanceText(LengthUnit spaceUnit, DistanceDisplay display, (Length Low, Length High) ends, string varies) : NumberText<Length> {
    private readonly Length step = display.Resolution(spaceUnit);

    public override (Length Low, Length High) Ends => ends;

    public override string Shown(Option<Length> key) => key.Match(Some: Entry, None: () => varies);

    public override string Entry(Length key) => display.Format(key, spaceUnit, appendUnitSystemName: true);

    public override IO<Length> Read(string entry) => UnitText.ParseLength(entry, StringParserSettings.DefaultParseSettings, spaceUnit);

    public override Length Moved(Length key, double steps) => key + (step * steps);

    public override Length Snapped(Length key, double steps) => step * QuantityValue.Round((key + (step * steps)) / step, MidpointRounding.ToEven);
}

public static class NumberText {
    public const int TickIntervals = 10;

    public static ScalarText<TValue, TKey> Scalar<TValue, TKey>(NumericDomain<TValue, TKey> domain, string varies)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        new(domain, varies);
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record FieldState<TValue> where TValue : notnull {
    public sealed record Resting(Option<TValue> Shown) : FieldState<TValue>;
    public sealed record Pressed(TValue Shown, PointF At) : FieldState<TValue>;
    public sealed record Scrubbing(TValue Shown, float Travel, PointF Last, Option<TValue> Moved) : FieldState<TValue>;
    public sealed record Editing(Option<TValue> Shown, Option<TValue> Typed, bool Faulted) : FieldState<TValue>;
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class NumberField : TextBox {
    public static string HandlerStyle { get; } = $"{typeof(NumberField).Assembly.GetName().Name}.{nameof(NumberField)}";

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

    protected static Fin<float> Travel(PointF at, PointF last) =>
        Refused.UnlessFound(Platform.Instance.Find<PointerTravel>(), nameof(PointerTravel)).Bind(row => row()(at, last));
}

public sealed class NumberField<TValue, TKey> : NumberField
    where TValue : notnull
    where TKey : notnull, IComparable<TKey> {
    // --- [STATE]
    private sealed record Styled(Color Text, Color Faulted, float Fine, float Slop) {
        public Color Ink(FieldState<TValue> state) =>
            state.SwitchPartially(this, @default: static (held, _) => held.Text, editing: static (held, editing) => editing.Faulted ? held.Faulted : held.Text);
    }

    private const float Inset = 5f;
    private NumberText<TKey> text;
    private readonly Func<TKey, Fin<TValue>> create;
    private readonly Func<TValue, TKey> key;
    private readonly (TKey Low, TKey High) bounds;
    private FieldState<TValue> state = new FieldState<TValue>.Resting(None);
    private Option<Styled> styled;
    private Option<IDisposable> listening;

    public NumberField(NumberText<TKey> text, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, IPlugInSink sink) : base(sink) {
        (this.text, this.create, this.key, this.bounds, PlaceholderText, TextAlignment, Cursor) = (text, create, key, bounds, text.Shown(None), TextAlignment.Right, Cursors.VerticalSplit);
        Run(ViewOps.WhileLoaded(this, Listened, new CallbackSite(sink, typeof(NumberField<TValue, TKey>), nameof(ViewOps.WhileLoaded)), lease => listening = Some(lease)));
    }

    public event EventHandler<EditEventArgs<TValue>>? Edited;

    public FieldFit Fit => new(this, text.TextWidth(Font) + (2f * Inset));

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

    public IO<Unit> Retext(NumberText<TKey> next) =>
        from written in IO.lift(() => { (text, PlaceholderText) = (next, next.Shown(None)); })
        from held in Current
        from shown in Receive(held.Switch(
            resting: static resting => resting.Shown, pressed: static pressed => Some(pressed.Shown),
            scrubbing: static scrubbing => Some(scrubbing.Shown), editing: static editing => editing.Shown))
        select shown;

    public override bool Adjust(int steps) =>
        Callbacks.Succeeded(Stepping(state, steps, Keys.None), static () => false, new CallbackSite(Sink, typeof(NumberField<TValue, TKey>), nameof(Adjust)));

    private IO<FieldState<TValue>> Current => IO.lift(() => state);

    private IO<Unit> Become(FieldState<TValue> next, Option<string> written) =>
        IO.lift(() => {
            (state, Cursor) = (next, next.MapPartially(@default: Cursors.VerticalSplit, editing: Cursors.IBeam));
            _ = styled.Iter(held => TextColor = held.Ink(next));
            _ = written.Iter(shown => Text = shown);
        });

    private IO<Unit> Entered(Option<TValue> shown) =>
        Become(new FieldState<TValue>.Editing(shown, None, Faulted: false), Some(Entering(shown))).Bind(_ => IO.lift(SelectAll));

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
        Current.Bind(held => held.SwitchPartially(
            this,
            @default: static (_, _) => IO.pure(unit),
            pressed: static (owner, pressed) => owner.Released(Some(pressed.Shown)),
            scrubbing: static (owner, scrubbing) => owner.Unscrubbed(Some(scrubbing.Shown), owner.Raised(new Edit<TValue>.Cancel()))));

    private Option<IO<Unit>> Stepping(FieldState<TValue> held, double steps, Keys modifiers) =>
        held.SwitchPartially(
            (Field: this, Steps: steps, Modifiers: modifiers),
            @default: static (_, _) => Option<IO<Unit>>.None,
            resting: static (s, resting) => resting.Shown.Bind(shown => s.Field.Stepped(shown, s.Steps, s.Modifiers).ToOption()).Map(stepped =>
                s.Field.Become(new FieldState<TValue>.Resting(Some(stepped)), Some(s.Field.Showing(Some(stepped)))).Bind(_ => s.Field.Raised(new Edit<TValue>.Step(stepped)))),
            editing: static (s, editing) => (editing.Typed | editing.Shown).Bind(shown => s.Field.Stepped(shown, s.Steps, s.Modifiers).ToOption()).Map(stepped =>
                from typed in s.Field.Become(editing with { Typed = Some(stepped), Faulted = false }, Some(s.Field.Entering(Some(stepped))))
                from selected in IO.lift(s.Field.SelectAll)
                from raised in s.Field.Raised(new Edit<TValue>.Step(stepped))
                select raised));

    // --- [VALUES]
    private string Showing(Option<TValue> value) => value.Match(Some: held => text.Shown(Some(key(held))), None: static () => "");

    private string Entering(Option<TValue> value) => value.Match(Some: held => text.Entry(key(held)), None: static () => "");

    private IO<Unit> Typed(FieldState<TValue>.Editing editing, string entry) =>
        text.Read(entry)
            .Bind(raw => create(raw).Match(Succ: value => Accepted(editing, value), Fail: _ => Faulted(editing)))
            .Catch(static error => error.HasCode(ErrorCodes.ParseError), _ => Faulted(editing));

    private Fin<TValue> Stepped(TValue shown, double steps, Keys keys) =>
        from held in styled.ToFin(new Refused(nameof(Restyled), Refused.Absent))
        let modifiers = HostInput.Modifiers(keys)
        from value in create(Clamped(modifiers.Contains(Rasm.UI.Modifier.Shift)
            ? text.Snapped(key(shown), steps)
            : text.Moved(key(shown), modifiers.Contains(Rasm.UI.Modifier.Common) ? steps * held.Fine : steps)))
        select value;

    private TKey Clamped(TKey key) =>
        key.CompareTo(bounds.Low) < 0 ? bounds.Low
        : key.CompareTo(bounds.High) > 0 ? bounds.High
        : key;

    private IO<Unit> Accepted(FieldState<TValue>.Editing editing, TValue value) =>
        Become(editing with { Typed = Some(value), Faulted = false }, None).Bind(_ => Raised(new Edit<TValue>.Preview(value)));

    private IO<Unit> Faulted(FieldState<TValue>.Editing editing) => Become(editing with { Faulted = true }, None);

    private IO<Unit> Committed(Option<TValue> value) => Raised(value.Map<Edit<TValue>>(static held => new Edit<TValue>.Commit(held)));

    private IO<Unit> Raised(Option<Edit<TValue>> edit) => IO.lift(() => edit.Iter(held => Edited?.Invoke(this, new EditEventArgs<TValue>(held))));

    private static IO<Unit> Claimed(Action claim, IO<Unit> effect) => IO.lift(claim).Bind(_ => effect);

    // --- [STYLE]
    private IO<Unit> Listened(Action<IDisposable> retain) =>
        from restyled in Restyled
        from changed in HostTheme.Changed.Inline(_ => Restyled, Sink, retain)
        select unit;

    private IO<Unit> Restyled =>
        from host in SceneView.Shipping.Of(Sink)
        from compact in IO.lift(() => HostTheme.Compact(this))
        from stated in IO.lift(() => HostTheme.Stated(host.Shipped, compact))
        from cascade in IO.lift(() => Styles.Root(HostTheme.Projected(compact), stated))
        from ink in IO.lift(() => Inked(cascade.Theme, Styles.Resolve(cascade, Role.Field, [])))
        from faulted in IO.lift(() => Inked(cascade.Theme, Styles.Resolve(cascade, Role.Field, [Condition.Faulted])))
        let metrics = Styles.Resolve(cascade, Role.Handle, []).Metrics
        from applied in IO.lift(() => {
            Styled held = new(ink, faulted, metrics[Metric.Fine], metrics[Metric.Slop]);
            (styled, TextColor) = (Some(held), held.Ink(state));
        })
        select unit;

    private static Fin<Color> Inked(Rasm.UI.Styling.Theme theme, Rasm.UI.Styling.Style style) =>
        theme.Color(style.Text, style, None, None)
            .Map(static ink => new Color(ink.Red, ink.Green, ink.Blue, ink.Alpha))
            .ToFin(new Refused(nameof(Rasm.UI.Styling.Theme.Color), Refused.Absent));

    // --- [INPUT]
    protected override void OnMouseDown(MouseEventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            resting: static (s, resting) => resting.Shown.Filter(_ => s.Args.Buttons is MouseButtons.Primary && !s.Field.HasFocus)
                .Match(Some: shown => s.Field.Pressing(shown, s.Args), None: static () => IO.pure(unit)),
            scrubbing: static (s, _) => when(s.Args.Buttons is MouseButtons.Alternate, Claimed(() => s.Args.Handled = true, s.Field.Interrupted)).As())));
        if (e.Handled) return;

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            pressed: static (s, pressed) => when(
                s.Field.styled.Exists(held => PointF.Distance(pressed.At, s.Args.Location) > held.Slop),
                from travel in IO.lift(() => Travel(s.Args.Location, pressed.At))
                from scrubbed in s.Field.Become(new FieldState<TValue>.Scrubbing(pressed.Shown, travel, s.Args.Location, None), None)
                from started in s.Field.Started
                select unit).As(),
            scrubbing: static (s, scrubbing) =>
                from delta in IO.lift(() => Travel(s.Args.Location, scrubbing.Last))
                let travel = scrubbing.Travel + delta
                let moved = s.Field.Stepped(scrubbing.Shown, float.Truncate(travel), s.Args.Modifiers).ToOption().Filter(value => scrubbing.Moved != Some(value))
                from shown in s.Field.Become(scrubbing with { Travel = travel, Last = s.Args.Location, Moved = moved | scrubbing.Moved }, moved.Map(value => s.Field.Showing(Some(value))))
                from raised in s.Field.Raised(moved.Map<Edit<TValue>>(static value => new Edit<TValue>.Preview(value)))
                select raised)));
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            this,
            @default: static (_, _) => IO.pure(unit),
            pressed: static (field, pressed) => field.Released(Some(pressed.Shown)).Bind(_ => IO.lift(field.Focus)),
            scrubbing: static (field, scrubbing) => field.Unscrubbed(scrubbing.Moved | Some(scrubbing.Shown), field.Committed(scrubbing.Moved)))));
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            editing: static (s, editing) => Claimed(() => s.Args.Handled = true,
                s.Field.Stepping(editing, Math.Sign(s.Args.Delta.Height), s.Args.Modifiers).IfNone(IO.pure(unit))))));
        if (e.Handled) return;

        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            scrubbing: static (s, _) => when(s.Args.Key is Keys.Escape, Claimed(() => s.Args.Handled = true, s.Field.Interrupted)).As(),
            editing: static (s, editing) => s.Args.Key switch {
                Keys.Up => Claimed(() => s.Args.Handled = true, s.Field.Stepping(editing, 1, s.Args.Modifiers).IfNone(IO.pure(unit))),
                Keys.Down => Claimed(() => s.Args.Handled = true, s.Field.Stepping(editing, -1, s.Args.Modifiers).IfNone(IO.pure(unit))),
                Keys.Enter => Claimed(() => s.Args.Handled = true,
                    s.Field.Become(editing with { Shown = editing.Typed | editing.Shown, Typed = None }, None).Bind(_ => s.Field.Committed(editing.Typed))),
                Keys.Escape => Claimed(() => s.Args.Handled = true,
                    s.Field.Become(editing with { Typed = None }, None).Bind(_ => s.Field.Raised(new Edit<TValue>.Cancel()))),
                _ => IO.pure(unit),
            })));
        if (e.Handled) return;

        base.OnKeyDown(e);
    }

    protected override void OnTextChanging(TextChangingEventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            editing: static (s, editing) => when(s.Args.FromUser, s.Field.Typed(editing, s.Args.NewText)).As())));
        base.OnTextChanging(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        Run(Current.Bind(held => held.SwitchPartially(this, @default: static (_, _) => IO.pure(unit), resting: static (field, resting) => field.Entered(resting.Shown))));
    }

    protected override void OnLostFocus(EventArgs e) {
        Run(Current.Bind(held => held.SwitchPartially(
            this,
            @default: static (_, _) => IO.pure(unit),
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

    protected override void Dispose(bool disposing) {
        try {
            if (disposing) _ = listening.Iter(static lease => lease.Dispose());
        } finally {
            base.Dispose(disposing);
        }
    }

    private void Run(IO<Unit> effect, [CallerMemberName] string member = "") =>
        Callbacks.Run(effect, new CallbackSite(Sink, typeof(NumberField<TValue, TKey>), member));
}
