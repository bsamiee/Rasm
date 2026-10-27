using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rhino;
using Rhino.Input;
using UnitsNet;

namespace Rasm.RhinoCommon.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
public abstract class NumberText<TKey> where TKey : notnull {
    public abstract (TKey Low, TKey High) Ends { get; }

    public abstract string Shown(TKey key);

    public abstract string Entry(TKey key);

    public abstract IO<TKey> Read(string entry);

    public abstract TKey Moved(TKey key, double steps);

    public abstract TKey Snapped(TKey key, int steps);

    public float TextWidth(Font font) => Seq(Shown(Ends.Low), Shown(Ends.High)).Fold(0f, (widest, shown) => float.Max(widest, font.MeasureString(shown).Width));
}

public sealed class ScalarText(Axis track, CultureInfo culture) : NumberText<float> {
    private const float Hundredth = 0.01f;

    public Option<float> Step { get; init; }
    public Option<int> Decimals { get; init; }
    public Option<string> Suffix { get; init; }

    private int Digits => (Decimals | Step.Map(static held => int.Max(0, (int)MathF.Ceiling(-MathF.Log10(held))))).IfNone(track.Decimals);

    public override (float Low, float High) Ends => (track.Domain.Low, track.Domain.High);

    public override string Shown(float key) =>
        key.ToString(string.Create(CultureInfo.InvariantCulture, $"F{Digits}"), culture) switch {
            var number => Suffix.Match(Some: held => $"{number} {held}", None: () => number),
        };

    public override string Entry(float key) => MathF.Round(key, Digits, MidpointRounding.ToEven).ToString("R", CultureInfo.InvariantCulture);

    public override IO<float> Read(string entry) => UnitText.ParseNumber(entry, StringParserSettings.DefaultParseSettings).Map(static parsed => (float)parsed);

    public override float Moved(float key, double steps) =>
        Step.Match(Some: held => key + (float)(steps * held), None: () => track.Value(track.Fraction(key) + (float)(steps * Hundredth)));

    public override float Snapped(float key, int steps) => track.Snapped(key, steps, Hundredth);
}

internal sealed class DistanceText(LengthUnit spaceUnit, DistanceDisplay display, (Length Low, Length High) ends) : NumberText<Length> {
    private readonly Length step = display.Resolution(spaceUnit);

    public override (Length Low, Length High) Ends => ends;

    public override string Shown(Length key) => Entry(key);

    public override string Entry(Length key) => display.Format(key, spaceUnit, appendUnitSystemName: true);

    public override IO<Length> Read(string entry) => UnitText.ParseLength(entry, StringParserSettings.DefaultParseSettings, spaceUnit);

    public override Length Moved(Length key, double steps) => key + (step * steps);

    public override Length Snapped(Length key, int steps) => step * QuantityValue.Round((key + (step * steps)) / step, MidpointRounding.ToEven);
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record FieldState<TKey> where TKey : notnull {
    public abstract Cursor Pointer { get; }

    public sealed record Resting(Option<TKey> Shown) : FieldState<TKey> {
        public override Cursor Pointer => Cursors.VerticalSplit;
    }

    public sealed record Pressed(TKey Shown, PointF At) : FieldState<TKey> {
        public override Cursor Pointer => Cursors.VerticalSplit;
    }

    public sealed record Scrubbing(TKey Shown, float Travel, PointF Last, Option<TKey> Moved) : FieldState<TKey> {
        public override Cursor Pointer => Cursors.VerticalSplit;
    }

    public sealed record Editing(Option<TKey> Shown, Option<TKey> Typed, bool Faulted) : FieldState<TKey> {
        public override Cursor Pointer => Cursors.IBeam;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class NumberBox : TextBox {
    public static string HandlerStyle { get; } = $"{typeof(NumberBox).Assembly.GetName().Name}.{nameof(NumberBox)}";

    protected NumberBox(IPlugInLifetime owner) {
        Owner = owner;
        Style = HandlerStyle;
    }

    public IPlugInLifetime Owner { get; }
    public event EventHandler<EventArgs>? ScrubStarted;
    public event EventHandler<EventArgs>? ScrubEnded;
    public abstract bool Adjust(int steps);

    protected IO<Unit> Started => IO.lift(() => ScrubStarted?.Invoke(this, EventArgs.Empty));

    protected IO<Unit> Ended => IO.lift(() => ScrubEnded?.Invoke(this, EventArgs.Empty));
}

public sealed class NumberField<TKey> : NumberBox where TKey : notnull, IComparable<TKey> {
    // --- [STATE]
    private sealed record Styled(Color Text, Color Faulted, float Fine, float Slop) {
        public Color Shown(FieldState<TKey> state) =>
            state.SwitchPartially(this, @default: static (held, _) => held.Text, editing: static (held, editing) => editing.Faulted ? held.Faulted : held.Text);
    }

    private const float Inset = 5f;
    private NumberText<TKey> text;
    private (TKey Low, TKey High) bounds;
    private readonly Func<Edit<TKey>, IO<Unit>> edited;
    private FieldState<TKey> state = new FieldState<TKey>.Resting(None);
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private Option<Styled> styled;

    public NumberField(NumberText<TKey> text, (TKey Low, TKey High) bounds, Func<Edit<TKey>, IO<Unit>> edited, IPlugInLifetime owner) : base(owner) {
        (this.text, this.bounds, this.edited, TextAlignment, Cursor) = (text, bounds, edited, TextAlignment.Right, Cursors.VerticalSplit);
        _ = owner.Run(ViewOps.WhileLoaded(this, Listened, owner).Under(scope));
    }

    public IO<Unit> Receive(TKey value) =>
        Current.Bind(held => held.Switch(
            (Field: this, Value: value),
            resting: static (s, _) => s.Field.Become(new FieldState<TKey>.Resting(Some(s.Value)), Some(s.Field.text.Shown(s.Value))),
            pressed: static (s, pressed) => s.Field.Become(pressed with { Shown = s.Value }, Some(s.Field.text.Shown(s.Value))),
            scrubbing: static (s, scrubbing) =>
                unless(Some(s.Value) == scrubbing.Moved, s.Field.Become(scrubbing with { Shown = s.Value }, Some(s.Field.text.Shown(scrubbing.Moved.IfNone(s.Value))))).As(),
            editing: static (s, editing) => unless(Some(s.Value) == editing.Typed, s.Field.Entered(Some(s.Value))).As()));

    public IO<Unit> Retext(NumberText<TKey> next, (TKey Low, TKey High) clamp) =>
        from written in IO.lift(() => { (text, bounds) = (next, clamp); })
        from held in Current
        from sized in Sized
        from shown in held.Switch(
                resting: static resting => resting.Shown, pressed: static pressed => Some(pressed.Shown),
                scrubbing: static scrubbing => Some(scrubbing.Shown), editing: static editing => editing.Shown)
            .Traverse(Receive).As()
        select unit;

    public override bool Adjust(int steps) =>
        Owner.Answer(Stepping(state, steps, Keys.None).Map(static stepped => stepped.Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false);

    private IO<FieldState<TKey>> Current => IO.lift(() => state);

    private IO<Unit> Sized => IO.lift(() => { Width = (int)MathF.Ceiling(text.TextWidth(Font) + (2f * Inset)); });

    private IO<Unit> Become(FieldState<TKey> next, Option<string> written) =>
        IO.lift(() => {
            (state, Cursor) = (next, next.Pointer);
            _ = styled.Iter(held => TextColor = held.Shown(next));
            _ = written.Iter(shown => Text = shown);
        });

    private IO<Unit> Entered(Option<TKey> shown) =>
        Become(new FieldState<TKey>.Editing(shown, None, Faulted: false), Some(shown.Map(text.Entry).IfNone(""))).Bind(_ => IO.lift(SelectAll));

    private IO<Unit> Pressing(TKey shown, MouseEventArgs e) =>
        from claimed in IO.lift(() => { e.Handled = true; })
        from pressed in Become(new FieldState<TKey>.Pressed(shown, e.Location), None)
        from captured in IO.lift(CaptureMouse)
        select unit;

    private IO<Unit> Released(Option<TKey> shown) =>
        Become(new FieldState<TKey>.Resting(shown), Some(shown.Map(text.Shown).IfNone(""))).Bind(_ => IO.lift(ReleaseMouseCapture));

    private IO<Unit> Unscrubbed(Option<TKey> shown, IO<Unit> raised) =>
        from released in Released(shown)
        from ended in Ended
        from edited in raised
        select edited;

    private IO<Unit> Interrupted =>
        Current.Bind(held => held.SwitchPartially(
            this,
            @default: static (_, _) => IO.pure(unit),
            pressed: static (owner, pressed) => owner.Released(Some(pressed.Shown)),
            scrubbing: static (owner, scrubbing) => owner.Unscrubbed(Some(scrubbing.Shown), owner.Raised(Edit.Cancel<TKey>()))));

    private Option<IO<Unit>> Stepping(FieldState<TKey> held, double steps, Keys modifiers) =>
        held.SwitchPartially(
            (Field: this, Steps: steps, Modifiers: modifiers),
            @default: static (_, _) => Option<IO<Unit>>.None,
            resting: static (s, resting) => resting.Shown.Bind(shown => s.Field.Stepped(shown, s.Steps, s.Modifiers)).Map(stepped =>
                s.Field.Become(new FieldState<TKey>.Resting(Some(stepped)), Some(s.Field.text.Shown(stepped))).Bind(_ => s.Field.Raised(Edit.Step(stepped)))),
            editing: static (s, editing) => (editing.Typed | editing.Shown).Bind(shown => s.Field.Stepped(shown, s.Steps, s.Modifiers)).Map(stepped =>
                from typed in s.Field.Become(editing with { Typed = Some(stepped), Faulted = false }, Some(s.Field.text.Entry(stepped)))
                from selected in IO.lift(s.Field.SelectAll)
                from raised in s.Field.Raised(Edit.Step(stepped))
                select raised));

    // --- [VALUES]
    private IO<Unit> Typed(FieldState<TKey>.Editing editing, string entry) =>
        text.Read(entry)
            .Bind(raw => Clamped(raw).CompareTo(raw) is 0 ? Accepted(editing, raw) : Faulted(editing))
            .Catch(static error => error.HasCode(ErrorCodes.ParseError), _ => Faulted(editing));

    private Option<TKey> Stepped(TKey shown, double steps, Keys keys) =>
        from held in styled
        let modifiers = HostInput.Modifiers(keys)
        select Clamped(modifiers.Shift ? text.Snapped(shown, (int)steps) : text.Moved(shown, modifiers.Common ? steps * held.Fine : steps));

    private TKey Clamped(TKey key) =>
        key.CompareTo(bounds.Low) < 0 ? bounds.Low
        : key.CompareTo(bounds.High) > 0 ? bounds.High
        : key;

    private IO<Unit> Accepted(FieldState<TKey>.Editing editing, TKey value) =>
        Become(editing with { Typed = Some(value), Faulted = false }, None).Bind(_ => Raised(Edit.Preview(value)));

    private IO<Unit> Faulted(FieldState<TKey>.Editing editing) => Become(editing with { Faulted = true }, None);

    private IO<Unit> Committed(Option<TKey> value) => Raised(value.Map(Edit.Commit));

    private IO<Unit> Raised(Option<Edit<TKey>> edit) => edit.Traverse(edited).As().Map(static _ => unit);

    private static IO<Unit> Claimed(Action claim, IO<Unit> effect) => IO.lift(claim).Bind(_ => effect);

    // --- [STYLE]
    private IO<Unit> Listened =>
        from restyled in Restyled
        from changed in HostTheme.Changed.Inline(_ => Restyled, Owner)
        select unit;

    private IO<Unit> Restyled =>
        from compact in IO.lift(() => HostTheme.Compact(this))
        let cascade = Styles.Root(HostTheme.Projected(compact), HostTheme.Stated(compact), HostPlacement.Panel.Root)
        from text in IO.lift(() => Colored(cascade.Theme, Styles.Resolve(cascade, Role.Field, [])))
        from faulted in IO.lift(() => Colored(cascade.Theme, Styles.Resolve(cascade, Role.Field, [Condition.Faulted])))
        let metrics = Styles.Resolve(cascade, Role.Handle, []).Metrics
        from applied in IO.lift(() => {
            Styled held = new(text, faulted, metrics[Metric.Fine], metrics[Metric.Slop]);
            (styled, TextColor) = (Some(held), held.Shown(state));
        })
        from sized in Sized
        select unit;

    private static Fin<Color> Colored(ColorScheme theme, RoleStyle style) =>
        Conversions.Found(theme.Color(style.Text, style).Map(static color => new Color(color.Red, color.Green, color.Blue, color.Alpha)));

    // --- [INPUT]
    protected override void OnMouseDown(MouseEventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            resting: static (s, resting) => resting.Shown.Filter(_ => s.Args.Buttons is MouseButtons.Primary && !s.Field.HasFocus)
                .Match(Some: shown => s.Field.Pressing(shown, s.Args), None: static () => IO.pure(unit)),
            scrubbing: static (s, _) => when(s.Args.Buttons is MouseButtons.Alternate, Claimed(() => s.Args.Handled = true, s.Field.Interrupted)).As())));
        if (e.Handled) return;

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            pressed: static (s, pressed) => when(
                s.Field.styled.Exists(held => PointF.Distance(pressed.At, s.Args.Location) > held.Slop),
                from travel in IO.lift(() => SceneView.Drawn.PointerTravel(s.Args.Location, pressed.At))
                from scrubbed in s.Field.Become(new FieldState<TKey>.Scrubbing(pressed.Shown, travel, s.Args.Location, None), None)
                from started in s.Field.Started
                select unit).As(),
            scrubbing: static (s, scrubbing) =>
                from delta in IO.lift(() => SceneView.Drawn.PointerTravel(s.Args.Location, scrubbing.Last))
                let travel = scrubbing.Travel + delta
                let moved = s.Field.Stepped(scrubbing.Shown, float.Truncate(travel), s.Args.Modifiers).Filter(value => scrubbing.Moved != Some(value))
                from shown in s.Field.Become(scrubbing with { Travel = travel, Last = s.Args.Location, Moved = moved | scrubbing.Moved }, moved.Map(s.Field.text.Shown))
                from raised in s.Field.Raised(moved.Map(Edit.Preview))
                select raised)));
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            this,
            @default: static (_, _) => IO.pure(unit),
            pressed: static (field, pressed) => field.Released(Some(pressed.Shown)).Bind(_ => IO.lift(field.Focus)),
            scrubbing: static (field, scrubbing) => field.Unscrubbed(scrubbing.Moved | Some(scrubbing.Shown), field.Committed(scrubbing.Moved)))));
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            editing: static (s, editing) => Claimed(() => s.Args.Handled = true,
                s.Field.Stepping(editing, Math.Sign(s.Args.Delta.Height), s.Args.Modifiers).IfNone(IO.pure(unit))))));
        if (e.Handled) return;

        base.OnMouseWheel(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            scrubbing: static (s, _) => when(s.Args.Key is Keys.Escape, Claimed(() => s.Args.Handled = true, s.Field.Interrupted)).As(),
            editing: static (s, editing) => s.Args.Key switch {
                Keys.Up => Claimed(() => s.Args.Handled = true, s.Field.Stepping(editing, 1, s.Args.Modifiers).IfNone(IO.pure(unit))),
                Keys.Down => Claimed(() => s.Args.Handled = true, s.Field.Stepping(editing, -1, s.Args.Modifiers).IfNone(IO.pure(unit))),
                Keys.Enter => Claimed(() => s.Args.Handled = true,
                    s.Field.Become(editing with { Shown = editing.Typed | editing.Shown, Typed = None }, None).Bind(_ => s.Field.Committed(editing.Typed))),
                Keys.Escape => Claimed(() => s.Args.Handled = true,
                    s.Field.Become(editing with { Typed = None }, None).Bind(_ => s.Field.Raised(Edit.Cancel<TKey>()))),
                _ => IO.pure(unit),
            })));
        if (e.Handled) return;

        base.OnKeyDown(e);
    }

    protected override void OnTextChanging(TextChangingEventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            (Field: this, Args: e),
            @default: static (_, _) => IO.pure(unit),
            editing: static (s, editing) => when(s.Args.FromUser, s.Field.Typed(editing, s.Args.NewText)).As())));
        base.OnTextChanging(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(this, @default: static (_, _) => IO.pure(unit), resting: static (field, resting) => field.Entered(resting.Shown))));
    }

    protected override void OnLostFocus(EventArgs e) {
        _ = Owner.Run(Current.Bind(held => held.SwitchPartially(
            this,
            @default: static (_, _) => IO.pure(unit),
            editing: static (field, editing) => (editing.Typed | editing.Shown) switch {
                var shown => field.Become(new FieldState<TKey>.Resting(shown), Some(shown.Map(field.text.Shown).IfNone(""))).Bind(_ => field.Committed(editing.Typed)),
            })));
        base.OnLostFocus(e);
    }

    // --- [LIFETIME]
    protected override void OnEnabledChanged(EventArgs e) {
        _ = Owner.Run(Interrupted);
        base.OnEnabledChanged(e);
    }

    protected override void OnUnLoad(EventArgs e) {
        _ = Owner.Run(Interrupted);
        base.OnUnLoad(e);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = Owner.Run(IO.lift(scope.DisposeResources));
            scope.Dispose();
        }
        base.Dispose(disposing);
    }
}
