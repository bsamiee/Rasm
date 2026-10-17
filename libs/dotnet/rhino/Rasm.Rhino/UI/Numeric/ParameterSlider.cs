using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SliderGrip<TKey>(double Knob, double Pointer, (TKey Low, TKey High) Track) where TKey : struct, INumber<TKey>;

public sealed record ParameterSliderState<TValue, TKey>(Option<TValue> Shown, Option<SliderGrip<TKey>> Grip)
    where TValue : notnull where TKey : struct, INumber<TKey>;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ParameterSlider<TValue, TKey>(IPlugInSink sink, ScalarText<TValue, TKey> text, string caption)
    : ComponentControl<ParameterSliderState<TValue, TKey>, Unit, TValue>(sink, new ParameterSliderState<TValue, TKey>(None, None), None)
    where TValue : notnull
    where TKey : struct, INumber<TKey> {
    // --- [METRICS]
    private static readonly MarkStyle Groove = new MarkStyle.Fill(PaintSlot.ControlText, 0.22f);
    private static readonly MarkStyle Span = new MarkStyle.Fill(PaintSlot.Highlight, 1f);
    private static readonly MarkStyle Tick = new MarkStyle.Stroke(PaintSlot.ControlText, 0.4f, 1f);
    private static readonly MarkStyle Face = new MarkStyle.Fill(PaintSlot.ControlBackground, 1f);
    private static readonly MarkStyle Focused = new MarkStyle.Stroke(PaintSlot.Highlight, 1f, 2f);

    private readonly float knob = EtoFonts.SmallFont.LineHeight;

    private RectangleF Bar(SizeF size) => new(knob / 2f, (knob - Plots.Inset) / 2f, float.Max(size.Width - knob, 0f), Plots.Inset);

    protected override Option<float> Fitted(ParameterSliderState<TValue, TKey> state, float width) => knob + Plots.Inset;

    // --- [LAYOUT]
    protected override Seq<PlotMark<Unit>> Layout(ParameterSliderState<TValue, TKey> state, SizeF size, float scale) =>
        (PlotPlane.Cartesian.Unit(Bar(size)), Position(state)) switch {
            var (plane, position) =>
                Seq(new PlotMark<Unit>(new MarkShape.Band(new RectangleF(size)), None, Some(new MarkPart<Unit>(unit, 0f))))
                    + Filled(state, plane, position)
                    + toSeq(Range(0, NumberText.TickIntervals + 1)).Map(index => plane.Point((float)index / NumberText.TickIntervals, 0f).X switch {
                        var x => new PlotMark<Unit>(new MarkShape.Rule(new PointF(x, knob), new PointF(x, knob + Plots.Inset)), Tick, None),
                    })
                    + Seq(new PlotMark<Unit>(new MarkShape.Disc(plane.Point((float)position, 0.5f), knob - 1f), Face, None),
                        new PlotMark<Unit>(new MarkShape.Disc(plane.Point((float)position, 0.5f), knob - 1f), MarkStyle.Ring, None)),
        };

    private Seq<PlotMark<Unit>> Filled(ParameterSliderState<TValue, TKey> state, PlotPlane.Cartesian plane, double position) =>
        text.Domain.Presentation.Stops.IsSome
            ? Seq<PlotMark<Unit>>()
            : new PlotMark<Unit>(new MarkShape.Band(plane.Plot), Groove, None).Cons(state.Shown.Map(_ => new PlotMark<Unit>(new MarkShape.Band(RectangleF.FromSides(
                plane.Point((float)double.Min(Origin(state), position), 0f).X, plane.Plot.Top,
                plane.Point((float)double.Max(Origin(state), position), 0f).X, plane.Plot.Bottom)), Span, None)).ToSeq());

    // --- [TRACK]
    private (TKey Low, TKey High) Track(ParameterSliderState<TValue, TKey> state) =>
        state.Grip.Match(
            Some: static grip => grip.Track,
            None: () => state.Shown.Match(Some: shown => text.Track(text.Domain.Key(shown)), None: () => text.Ends));

    private double Position(ParameterSliderState<TValue, TKey> state) =>
        state.Shown.Match(Some: shown => text.Domain.Presentation.Position(text.Domain.Key(shown), Track(state)), None: static () => 0d);

    private double Origin(ParameterSliderState<TValue, TKey> state) =>
        text.Domain.Presentation.Origin.Map(origin => text.Domain.Presentation.Position(origin, Track(state))).IfNone(0d);

    private TKey Clamped(TKey key, (TKey Low, TKey High) track) =>
        TKey.Clamp(key, text.Key(0d, track), text.Key(1d, track));

    private TKey Keyed(double position, (TKey Low, TKey High) track) =>
        Clamped(text.Key(position, track), track);

    private TKey Stepped(TKey from, double steps, Keys modifiers, (TKey Low, TKey High) track) =>
        Clamped(Snaps(modifiers) ? text.Snap(from, steps, track) : text.Moved(from, steps * Scale(modifiers)), track);

    private double Pointed(SizeF size, PointF point) => PlotPlane.Cartesian.Unit(Bar(size)).Value(point).First;

    // --- [TRANSITIONS]
    protected override ParameterSliderState<TValue, TKey> Received(ParameterSliderState<TValue, TKey> state, Option<TValue> value) => state with { Shown = value };

    private Transition<ParameterSliderState<TValue, TKey>, TValue> Moved(ParameterSliderState<TValue, TKey> state, TKey next, Func<TValue, Edit<TValue>> edit) =>
        state.Shown.Exists(shown => text.Domain.Key(shown) == next)
            ? new(state, None)
            : text.Domain.Create(next).Match(
                Succ: value => new Transition<ParameterSliderState<TValue, TKey>, TValue>(state with { Shown = Some(value) }, Some(edit(value))),
                Fail: _ => new Transition<ParameterSliderState<TValue, TKey>, TValue>(state, None));

    private Option<Transition<ParameterSliderState<TValue, TKey>, TValue>> Stepping(ParameterSliderState<TValue, TKey> state, Func<TKey, (TKey Low, TKey High), TKey> next) =>
        state.Shown.Map(shown => Moved(state, next(text.Domain.Key(shown), Track(state)), static value => new Edit<TValue>.Step(value)));

    protected override Option<Transition<ParameterSliderState<TValue, TKey>, TValue>> Pressed(
        ParameterSliderState<TValue, TKey> state, SizeF size, float scale, Option<Unit> part, MouseEventArgs e) =>
        part.Filter(_ => e.Buttons is MouseButtons.Primary).Map(_ => (Knob: Position(state), Pointer: double.Clamp(Pointed(size, e.Location), 0d, 1d)) switch {
            var press when Math.Abs(press.Pointer - press.Knob) * Bar(size).Width <= DragThreshold =>
                new Transition<ParameterSliderState<TValue, TKey>, TValue>(state with { Grip = new SliderGrip<TKey>(press.Knob, press.Pointer, Track(state)) }, None),
            var press => new SliderGrip<TKey>(press.Pointer, press.Pointer, Track(state)) switch {
                var grip => Moved(state with { Grip = grip }, Keyed(press.Pointer, grip.Track), static value => new Edit<TValue>.Preview(value)),
            },
        });

    protected override Transition<ParameterSliderState<TValue, TKey>, TValue> Dragged(
        ParameterSliderState<TValue, TKey> state, SizeF size, float scale, Option<Unit> part, PointF origin, PointF previous, MouseEventArgs e) =>
        state.Grip.Map(grip => (grip.Knob + ((Pointed(size, e.Location) - grip.Pointer) * Scale(e.Modifiers))) switch {
            var raw => Moved(state, Snaps(e.Modifiers) ? Clamped(text.Snap(Keyed(raw, grip.Track), 0d, grip.Track), grip.Track) : Keyed(raw, grip.Track),
                static value => new Edit<TValue>.Preview(value)),
        }).IfNone(new Transition<ParameterSliderState<TValue, TKey>, TValue>(state, None));

    protected override Transition<ParameterSliderState<TValue, TKey>, TValue> Released(
        ParameterSliderState<TValue, TKey> state, SizeF size, float scale, Option<Unit> part, MouseEventArgs e) =>
        new(state with { Grip = None }, None);

    protected override Option<Transition<ParameterSliderState<TValue, TKey>, TValue>> KeyPressed(
        ParameterSliderState<TValue, TKey> state, SizeF size, float scale, Option<Unit> part, KeyEventArgs e) =>
        e.Key switch {
            Keys.Right or Keys.Up or Keys.Left or Keys.Down =>
                Stepping(state, (from, track) => Stepped(from, e.Key is Keys.Right or Keys.Up ? 1d : -1d, e.Modifiers, track)),
            Keys.PageUp or Keys.PageDown =>
                Stepping(state, (from, track) => Keyed(text.Domain.Presentation.Position(from, track) + ((e.Key is Keys.PageUp ? 1d : -1d) / NumberText.TickIntervals), track)),
            Keys.Home or Keys.End => Stepping(state, (_, track) => Keyed(e.Key is Keys.Home ? 0d : 1d, track)),
            _ => None,
        };

    protected override Option<Transition<ParameterSliderState<TValue, TKey>, TValue>> Scrolled(
        ParameterSliderState<TValue, TKey> state, SizeF size, float scale, Option<Unit> part, MouseEventArgs e) =>
        Stepping(state, (from, track) => Stepped(from, Math.Sign(e.Delta.Height), e.Modifiers, track));

    protected override Option<Transition<ParameterSliderState<TValue, TKey>, TValue>> Stepped(
        ParameterSliderState<TValue, TKey> state, SizeF size, float scale, Unit part, int steps) =>
        Stepping(state, (from, track) => Stepped(from, steps, Keys.None, track));

    // --- [ACCESSIBILITY]
    protected override PartFacet Facet(ParameterSliderState<TValue, TKey> state, Unit key) =>
        new(PartRole.Slider, Cursors.Default, caption, Some(text.Shown(state.Shown.Map(text.Domain.Key))), None);

    // --- [PAINT]
    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, ParameterSliderState<TValue, TKey> state, Seq<PlotMark<Unit>> marks, Interaction<Unit> interaction) =>
        PlotPlane.Cartesian.Unit(Bar(bounds.Size)) switch {
            var plane =>
                from stops in when(text.Domain.Presentation.Stops.IsSome, Plots.Strip(canvas, plane, position => text.Domain.Presentation.Fill(text.Domain.Presentation.Key(position, Track(state), text.Domain.Bounds), text.Domain.Bounds)
                    .Map(static light => Plots.Encoded(Gamut.StandardRgb, light.AsVector3()))
                    .IfNone(Colors.Transparent))).As()
                from veil in Plots.Veiled(canvas, new MarkShape.Band(plane.Plot))
                from painted in Plots.Paint(canvas, marks.Map(mark => mark.Style == Some(MarkStyle.Ring) ? mark with { Style = Ringed(canvas.Focused, interaction) } : mark))
                select unit,
        };

    private static MarkStyle Ringed(bool focused, Interaction<Unit> cues) =>
        focused ? Focused : cues.Hovered.IsSome || cues.Pressed.IsSome ? MarkStyle.Lifted : MarkStyle.Ring;
}
