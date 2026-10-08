using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AngleDialState<TValue>(Option<TValue> Shown, Option<double> Held) where TValue : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class AngleDial<TValue, TKey, TError>(IPlugInSink sink, NumberText<TKey> text, string caption)
    : ComponentControl<AngleDialState<TValue>, Unit, TValue>(sink, new AngleDialState<TValue>(None, None), None)
    where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
    where TKey : struct, INumber<TKey>
    where TError : Error, IValidationError<TError> {
    // --- [GEOMETRY]
    private const double AngleSnap = Math.Tau / 72d;
    private static readonly Axis.Periodic AngleAxis = new(AxisRange.Radians);
    private static readonly Axis.Linear RadiusAxis = new(AxisRange.Unit);
    private static readonly MarkStyle Lifted = new MarkStyle.Stroke(PaintSlot.ControlText, 1f, 1f);
    private static readonly MarkStyle Needle = new MarkStyle.Stroke(PaintSlot.ControlText, 1f, 2f);

    private static PlotPlane.Polar Plane(SizeF size) => new(new RectangleF(size), AngleAxis, RadiusAxis);

    private static float Plotted(TKey key) => (float)((Math.PI / 2d) - double.CreateSaturating(key));

    private static double Pointed(PlotPlane.Polar plane, PointF point) => (Math.PI / 2d) - plane.Value(point).First;

    private static double Signed(double delta) => delta - (Math.Tau * Math.Round(delta / Math.Tau, MidpointRounding.ToEven));

    private static TKey Wrapped(double angle) =>
        double.CreateSaturating(TValue.MinValue.ToValue()) switch {
            var low => NumericRows.Narrowed<TKey>(angle - (Math.Tau * Math.Floor((angle - low) / Math.Tau))),
        };

    private static TKey Placed(double held, Keys modifiers) => Wrapped(Snaps(modifiers) ? Math.Round(held / AngleSnap, MidpointRounding.ToEven) * AngleSnap : held);

    protected override Seq<PlotMark<Unit>> Layout(AngleDialState<TValue> state, SizeF size, float scale) =>
        (Plane(size), float.Min(size.Width, size.Height)) switch {
            var (plane, diameter) => new PlotMark<Unit>(new MarkShape.Disc(plane.Plot.Center, diameter - 1f), MarkStyle.Ring, Some(new MarkPart<Unit>(unit, 0f)))
                .Cons(state.Shown.Map(value => new PlotMark<Unit>(
                    new MarkShape.Rule(plane.Plot.Center, plane.Point(Plotted(value.ToValue()), (diameter - 4f) / diameter)), Needle, None)).ToSeq()),
        };

    protected override Option<float> Fitted(AngleDialState<TValue> state, float width) => Some(width);

    // --- [TRANSITIONS]
    protected override AngleDialState<TValue> Received(AngleDialState<TValue> state, Option<TValue> value) => state with { Shown = value, Held = None };

    private static Transition<AngleDialState<TValue>, TValue> Crossed(AngleDialState<TValue> state, Option<double> held, TKey key, Func<TValue, Edit<TValue>> edit) =>
        Conversions.Validated<TValue, TKey, TError>(key).Match(
            Succ: value => new Transition<AngleDialState<TValue>, TValue>(
                state with { Shown = Some(value), Held = held },
                state.Shown.Exists(shown => EqualityComparer<TValue>.Default.Equals(shown, value)) ? None : Some(edit(value))),
            Fail: _ => new Transition<AngleDialState<TValue>, TValue>(state with { Held = held }, None));

    private Option<Transition<AngleDialState<TValue>, TValue>> Moved(AngleDialState<TValue> state, double steps, Keys modifiers) =>
        state.Shown.Map(value => value.ToValue() switch {
            var key => Crossed(state, None,
                Wrapped(double.CreateSaturating(Snaps(modifiers) ? text.Snapped(text.Moved(key, steps)) : text.Moved(key, steps * Scale(modifiers)))),
                static next => new Edit<TValue>.Step(next)),
        });

    protected override Option<Transition<AngleDialState<TValue>, TValue>> Pressed(AngleDialState<TValue> state, SizeF size, float scale, Option<Unit> part, MouseEventArgs e) =>
        part.Filter(_ => e.Buttons == MouseButtons.Primary).Map(_ => Pointed(Plane(size), e.Location) switch {
            var held => Crossed(state, Some(held), Placed(held, e.Modifiers), static next => new Edit<TValue>.Preview(next)),
        });

    protected override Transition<AngleDialState<TValue>, TValue> Dragged(AngleDialState<TValue> state, SizeF size, float scale, Option<Unit> part, PointF origin, PointF previous, MouseEventArgs e) =>
        state.Held.Map(held => (held + (Signed(Pointed(Plane(size), e.Location) - Pointed(Plane(size), previous)) * Scale(e.Modifiers))) switch {
            var next => Crossed(state, Some(next), Placed(next, e.Modifiers), static moved => new Edit<TValue>.Preview(moved)),
        }).IfNone(new Transition<AngleDialState<TValue>, TValue>(state, None));

    protected override Transition<AngleDialState<TValue>, TValue> Released(AngleDialState<TValue> state, SizeF size, float scale, Option<Unit> part, MouseEventArgs e) =>
        new(state with { Held = None }, None);

    protected override Option<Transition<AngleDialState<TValue>, TValue>> KeyPressed(AngleDialState<TValue> state, SizeF size, float scale, Option<Unit> part, KeyEventArgs e) =>
        (e.Key switch {
            Keys.Up or Keys.Right => Some(1d),
            Keys.Down or Keys.Left => Some(-1d),
            _ => Option<double>.None,
        }).Bind(steps => Moved(state, steps, e.Modifiers));

    protected override Option<Transition<AngleDialState<TValue>, TValue>> Scrolled(AngleDialState<TValue> state, SizeF size, float scale, Option<Unit> part, MouseEventArgs e) =>
        Moved(state, Math.Sign(e.Delta.Height), e.Modifiers);

    protected override Option<Transition<AngleDialState<TValue>, TValue>> Stepped(AngleDialState<TValue> state, SizeF size, float scale, Unit part, int steps) =>
        Moved(state, steps, Keys.None);

    // --- [ACCESSIBILITY]
    protected override PartFacet Facet(AngleDialState<TValue> state, Unit key) =>
        new(PartRole.Slider, Cursors.Default, caption, Some(text.Shown(state.Shown.Map(static value => value.ToValue()))), None);

    // --- [PAINT]
    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, AngleDialState<TValue> state, Seq<PlotMark<Unit>> marks, Interaction<Unit> interaction) =>
        Plots.Paint(canvas, marks.Map(mark =>
            mark.Part.IsSome && (interaction.Hovered.IsSome || interaction.Pressed.IsSome || canvas.Focused) ? mark with { Style = Lifted } : mark));
}
