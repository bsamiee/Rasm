using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Styling;
using Rhino.Runtime;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record SliderGrip<TKey>(double Knob, double Pointer, (TKey Low, TKey High) Track) where TKey : struct, INumber<TKey>;

internal sealed record ParameterSliderState<TValue, TKey>(Option<TValue> Shown, Option<SliderGrip<TKey>> Grip)
    where TValue : notnull where TKey : struct, INumber<TKey>;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ParameterSlider<TValue, TKey>(IPlugInSink sink, ScalarText<TValue, TKey> text, string caption)
    : ComponentControl<ParameterSliderState<TValue, TKey>, Unit, TValue>(sink, new ParameterSliderState<TValue, TKey>(None, None), None)
    where TValue : notnull
    where TKey : struct, INumber<TKey> {
    // --- [METRICS]
    private const float Corner = 4f;
    private const float Marker = 15f;

    private static readonly MarkStyle Border = new MarkStyle.Stroke(PaintSlot.Text, 0.2f, 1f);
    private static readonly MarkStyle Focused = new MarkStyle.Stroke(PaintSlot.Text, 0.4f, 1f);
    private static readonly MarkStyle Span = new MarkStyle.Fill(PaintSlot.FrameBackground, 1f);
    private static readonly MarkStyle Knob = new MarkStyle.Fill(PaintSlot.Highlight, 1f);
    private static readonly MarkStyle Rim = new MarkStyle.Stroke(PaintSlot.Text, 0.6f, 1f);

    private static RectangleF Groove(SizeF size) => new(size);

    private static RectangleF Bar(SizeF size) => new(Marker / 2f, 0f, float.Max(size.Width - Marker, 0f), size.Height);

    protected override Option<float> Fitted(ParameterSliderState<TValue, TKey> state, float width) => HostUtils.RunningOnOSX ? 17f : 18f;

    // --- [LAYOUT]
    protected override Seq<PlotMark<Unit>> Layout(ParameterSliderState<TValue, TKey> state, SizeF size, float scale) =>
        (PlotPlane.Cartesian.Unit(Bar(size)), Position(state)) switch {
            var (plane, position) =>
                Seq(new PlotMark<Unit>(new MarkShape.Band(Groove(size), Corner), None, Some(new MarkPart<Unit>(unit, 0f))))
                    + Filled(state, plane, position)
                    + Seq(new PlotMark<Unit>(new MarkShape.Band(RectangleF.Inset(Groove(size), new PaddingF(0.5f)), Corner), Border, None))
                    + state.Shown.ToSeq().Bind(_ => RectangleF.FromCenter(plane.Point((float)position, 0.5f), new SizeF(Marker, size.Height)) switch {
                        var knob => Seq(new PlotMark<Unit>(new MarkShape.Band(knob, Corner), Knob, None), new PlotMark<Unit>(new MarkShape.Band(RectangleF.Inset(knob, new PaddingF(0.5f)), Corner), Rim, None)),
                    }),
        };

    private Seq<PlotMark<Unit>> Filled(ParameterSliderState<TValue, TKey> state, PlotPlane.Cartesian plane, double position) {
        double origin = text.Domain.Presentation.Origin.Map(key => text.Domain.Presentation.Position(key, Track(state))).IfNone(0d);
        return text.Domain.Presentation.Stops.IsSome
            ? Seq<PlotMark<Unit>>()
            : state.Shown.Map(_ => new PlotMark<Unit>(new MarkShape.Band(RectangleF.FromSides(
                plane.Point((float)double.Min(origin, position), 0f).X - (Marker / 2f), plane.Plot.Top,
                plane.Point((float)double.Max(origin, position), 0f).X + (Marker / 2f), plane.Plot.Bottom), Corner), Span, None)).ToSeq();
    }

    // --- [TRACK]
    private (TKey Low, TKey High) Track(ParameterSliderState<TValue, TKey> state) =>
        state.Grip.Match(
            Some: static grip => grip.Track,
            None: () => state.Shown.Match(Some: shown => text.Track(text.Domain.Key(shown)), None: () => text.Ends));

    private double Position(ParameterSliderState<TValue, TKey> state) =>
        state.Shown.Match(Some: shown => text.Domain.Presentation.Position(text.Domain.Key(shown), Track(state)), None: static () => 0d);

    private TKey Clamped(TKey key, (TKey Low, TKey High) track) =>
        TKey.Clamp(key, text.Key(0d, track), text.Key(1d, track));

    private TKey Keyed(double position, (TKey Low, TKey High) track) =>
        Clamped(text.Key(position, track), track);

    private TKey Stepped(TKey from, double steps, Keys modifiers, (TKey Low, TKey High) track) =>
        Clamped(Snaps(modifiers) ? text.Snap(from, steps, track) : text.Moved(from, steps * Scale(modifiers)), track);

    private static double Pointed(SizeF size, PointF point) => PlotPlane.Cartesian.Unit(Bar(size)).Value(point).First;

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
        (Groove(bounds.Size), Bar(bounds.Size)) switch {
            var (groove, bar) =>
                from stops in when(text.Domain.Presentation.Stops.IsSome, DisposalOps.Within(
                    () => new MarkShape.Band(groove, Corner).Figure(),
                    path => IO.lift(() => canvas.Graphics.SetClip(path)).Bind(_ => Plots.Strip(canvas, PlotPlane.Cartesian.Unit(groove), fraction =>
                        text.Domain.Presentation.Fill(text.Domain.Presentation.Key(float.Clamp(((fraction * groove.Width) - bar.Left) / bar.Width, 0f, 1f), Track(state), text.Domain.Bounds), text.Domain.Bounds)
                            .Map(static light => Plots.Encoded(Gamut.StandardRgb, light.AsVector3()))
                            .IfNone(Colors.Transparent))),
                    path => IO.lift(() => {
                        path.Dispose();
                        canvas.Graphics.ResetClip();
                    }))).As()
                from veil in Plots.Veiled(canvas, new MarkShape.Band(groove, Corner))
                from painted in Plots.Paint(canvas, marks.Map(mark => mark.Style.Exists(static style => style == Border) && canvas.Focused
                    ? mark with { Style = Focused }
                    : mark.Style.Exists(static style => style == Rim) && (interaction.Hovered.IsSome || interaction.Pressed.IsSome) ? mark with { Style = MarkStyle.Lifted } : mark))
                select unit,
        };
}
