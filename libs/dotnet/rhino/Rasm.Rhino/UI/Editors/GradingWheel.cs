using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Balance;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Numeric;

namespace Rasm.Rhino.UI.Editors;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class WheelPart {
    public static readonly WheelPart Disc = new(PartRole.Slider, CursorType.Crosshair, "{0} hue", MarkStyle.Ring, new MarkStyle.Stroke(PaintSlot.ControlText, 1f, 1f));
    public static readonly WheelPart Puck = new(PartRole.Handle, CursorType.Move, "{0} strength", new MarkStyle.Stroke(PaintSlot.ControlText, 1f, 1f), new MarkStyle.Stroke(PaintSlot.Highlight, 1f, 1f));

    public PartRole Role { get; }
    public CursorType Cursor { get; }
    public string Template { get; }
    public MarkStyle Rest { get; }
    public MarkStyle Cued { get; }
}

public sealed record ShownWheel(string Caption, NumberText<float> Hue, NumberText<float> Strength);

public sealed record GradingWheelState(ShownWheel Shown, Option<WheelOffset> Offset, Option<Vector2> Target);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GradingWheel(IPlugInSink sink, ShownWheel shown)
    : ComponentControl<GradingWheelState, WheelPart, WheelOffset>(sink, new GradingWheelState(shown, None, None), None) {
    // --- [STATE]
    private const float PuckDiameter = 14f;
    private const float CrossLength = 10f;
    private static readonly Axis.Periodic HueAxis = new(AxisRange.Radians);
    private static readonly Axis.Linear StrengthAxis = new(AxisRange.Unit);
    private Option<(int Diameter, Bitmap Raster)> raster;

    public IO<Unit> Show(ShownWheel wheel) => Advance(state => new(state with { Shown = wheel }, None));

    protected override GradingWheelState Received(GradingWheelState state, Option<WheelOffset> value) => state with { Offset = value };

    protected override Option<float> Fitted(GradingWheelState state, float width) => Some(width);

    // --- [PLANE]
    private static PlotPlane.Polar Plane(SizeF size) =>
        Plots.Plot(new RectangleF(size)) switch {
            var plot => float.Min(plot.Width, plot.Height) switch {
                var side => new(RectangleF.FromCenter(plot.Center, new SizeF(side, side)), HueAxis, StrengthAxis),
            },
        };

    private static Vector2 Spot(float hue, float strength) => MathF.SinCos(hue) switch { var (sin, cos) => strength * new Vector2(cos, sin) };

    private static Vector2 Target(PlotPlane plane, PointF point) => plane.Value(point) switch { var (hue, strength) => Spot(hue, strength) };

    protected override Seq<PlotMark<WheelPart>> Layout(GradingWheelState state, SizeF size, float scale) =>
        Plane(size) switch {
            var plane => new PlotMark<WheelPart>(new MarkShape.Disc(plane.Plot.Center, plane.Plot.Width), WheelPart.Disc.Rest, Some(new MarkPart<WheelPart>(WheelPart.Disc, Plots.Inset)))
                .Cons(Seq(new SizeF(CrossLength / 2f, 0f), new SizeF(0f, CrossLength / 2f))
                    .Map(arm => new PlotMark<WheelPart>(new MarkShape.Rule(plane.Plot.Center - arm, plane.Plot.Center + arm), WheelPart.Puck.Rest, None)))
                + state.Offset.Map(offset => new PlotMark<WheelPart>(
                    new MarkShape.Disc(plane.Point(offset.Hue, offset.Strength), PuckDiameter), WheelPart.Puck.Rest, Some(new MarkPart<WheelPart>(WheelPart.Puck, 0f)))).ToSeq(),
        };

    // --- [TRANSITIONS]
    private static WheelOffset Placed(WheelOffset held, float hue, WheelStrength strength) =>
        new(WheelHue.Create(float.Min(HueAxis.Value(HueAxis.Unit(hue)), WheelHue.MaxValue)), strength, held.Luma);

    private static float Shifted(NumberText<float> text, float value, double steps, Keys modifiers) =>
        Snaps(modifiers) ? text.Snapped(text.Moved(value, steps)) : text.Moved(value, steps * Scale(modifiers));

    private static WheelOffset Reached(WheelOffset held, Vector2 target, bool snaps, float least) =>
        MathF.Atan2(target.Y, target.X) switch {
            var angle => Placed(held, target == Vector2.Zero ? (float)held.Hue : snaps ? HueAxis.Snap(angle, 0, least) : angle,
                WheelStrength.Create(float.Min(target.Length(), StrengthAxis.Range.High))),
        };

    private static Transition<GradingWheelState, WheelOffset> Moved(GradingWheelState state, WheelOffset held, Vector2 target, Keys modifiers, SizeF size) =>
        Reached(held, target, Snaps(modifiers), 2f / Plane(size).Plot.Width) switch {
            var next => new(state with { Offset = Some(next), Target = Some(target) }, next == held ? None : Some<Edit<WheelOffset>>(new Edit<WheelOffset>.Preview(next))),
        };

    private static Option<Transition<GradingWheelState, WheelOffset>> Nudged(GradingWheelState state, WheelPart part, double steps, Keys modifiers) =>
        state.Offset.Bind(held => part.Switch((State: state, Held: held, Steps: steps, Modifiers: modifiers),
                disc: static nudge => Some(Placed(nudge.Held, Shifted(nudge.State.Shown.Hue, nudge.Held.Hue, nudge.Steps, nudge.Modifiers), nudge.Held.Strength)),
                puck: static nudge => Conversions.Validated<WheelStrength, float, InvalidGrade>(Shifted(nudge.State.Shown.Strength, nudge.Held.Strength, nudge.Steps, nudge.Modifiers))
                    .ToOption()
                    .Map(strength => Placed(nudge.Held, nudge.Held.Hue, strength))))
            .Map(next => new Transition<GradingWheelState, WheelOffset>(state with { Offset = Some(next) }, Some<Edit<WheelOffset>>(new Edit<WheelOffset>.Step(next))));

    protected override Option<Transition<GradingWheelState, WheelOffset>> Pressed(GradingWheelState state, SizeF size, float scale, Option<WheelPart> part, MouseEventArgs e) =>
        state.Offset.Filter(_ => e.Buttons == MouseButtons.Primary).Map(held =>
            part.Exists(static key => key == WheelPart.Puck)
                ? new Transition<GradingWheelState, WheelOffset>(state with { Target = Some(Spot(held.Hue, held.Strength)) }, None)
                : Moved(state, held, Target(Plane(size), e.Location), e.Modifiers, size));

    protected override Transition<GradingWheelState, WheelOffset> Dragged(GradingWheelState state, SizeF size, float scale, Option<WheelPart> part, PointF origin, PointF previous, MouseEventArgs e) =>
        (from held in state.Offset
         from target in state.Target
         let plane = Plane(size)
         select Moved(state, held, target + (Scale(e.Modifiers) * (Target(plane, e.Location) - Target(plane, previous))), e.Modifiers, size))
            .IfNone(new Transition<GradingWheelState, WheelOffset>(state, None));

    protected override Transition<GradingWheelState, WheelOffset> Released(GradingWheelState state, SizeF size, float scale, Option<WheelPart> part, MouseEventArgs e) =>
        new(state with { Target = None }, None);

    protected override Option<Transition<GradingWheelState, WheelOffset>> DoubleClicked(GradingWheelState state, SizeF size, float scale, Option<WheelPart> part, MouseEventArgs e) =>
        state.Offset.Map(held => held with { Hue = WheelHue.Origin, Strength = WheelStrength.Neutral })
            .Map(next => new Transition<GradingWheelState, WheelOffset>(state with { Offset = Some(next), Target = None }, Some<Edit<WheelOffset>>(new Edit<WheelOffset>.Commit(next))));

    protected override Option<Transition<GradingWheelState, WheelOffset>> KeyPressed(GradingWheelState state, SizeF size, float scale, Option<WheelPart> part, KeyEventArgs e) =>
        (e.Key switch {
            Keys.Right => Some((Part: WheelPart.Disc, Steps: 1d)),
            Keys.Left => Some((Part: WheelPart.Disc, Steps: -1d)),
            Keys.Up => Some((Part: WheelPart.Puck, Steps: 1d)),
            Keys.Down => Some((Part: WheelPart.Puck, Steps: -1d)),
            _ => Option<(WheelPart Part, double Steps)>.None,
        }).Bind(arrow => Nudged(state, arrow.Part, arrow.Steps, e.Modifiers));

    protected override Option<Transition<GradingWheelState, WheelOffset>> Stepped(GradingWheelState state, SizeF size, float scale, WheelPart part, int steps) =>
        Nudged(state, part, steps, Keys.None);

    // --- [ACCESSIBILITY]
    protected override PartFacet Facet(GradingWheelState state, WheelPart key) =>
        new(key.Role, Cursors.Cached(key.Cursor), RowText.Localize(key.Template, arguments: [state.Shown.Caption]).Local,
            Some(key.Switch(state,
                disc: static held => held.Shown.Hue.Shown(held.Offset.Map(static offset => (float)offset.Hue)),
                puck: static held => held.Shown.Strength.Shown(held.Offset.Map(static offset => (float)offset.Strength)))),
            None);

    // --- [PAINT]
    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, GradingWheelState state, Seq<PlotMark<WheelPart>> marks, Interaction<WheelPart> interaction) =>
        Plane(bounds.Size) switch {
            var plane =>
                from well in Plots.Well(canvas, bounds)
                from disc in Plots.Device(plane, canvas.Scale).Match(Succ: extent => Tinted(canvas, plane.Plot, extent), Fail: static _ => IO.pure(unit))
                from painted in Plots.Paint(canvas, marks.Map(mark => Cue(mark, interaction)) + (canvas.Focused ? Seq(Focus(bounds)) : Seq<PlotMark<WheelPart>>()))
                select unit,
        };

    private IO<Unit> Tinted(PlotCanvas canvas, RectangleF plot, PixelExtent extent) =>
        from image in IO.lift(() => Held(extent))
        from drawn in Plots.Raster(canvas, plot, image)
        from veil in IO.lift(() => Themes.Veil(canvas.Slots, canvas.Slots[PaintSlot.ControlBackground])
            .Filter(_ => !canvas.Enabled)
            .Iter(alpha => canvas.Graphics.FillEllipse(new Color(canvas.Slots[PaintSlot.ControlBackground], alpha), plot)))
        select unit;

    private Bitmap Held(PixelExtent extent) =>
        raster.Filter(held => held.Diameter == extent.Width).IfNone(() => Rebuilt(extent)).Raster;

    private (int Diameter, Bitmap Raster) Rebuilt(PixelExtent extent) {
        _ = raster.Iter(static held => held.Raster.Dispose());
        (int Diameter, Bitmap Raster) built = (extent.Width, Plots.Pixels(extent, Tints(extent.Width)));
        raster = built;
        return built;
    }

    private static Seq<int> Tints(int diameter) =>
        toSeq(Range(0, diameter * diameter)).Map(index => new Vector2((index % diameter) + 0.5f - (diameter / 2f), (diameter / 2f) - (index / diameter) - 0.5f) switch {
            var offset => float.Clamp((diameter / 2f) - offset.Length() + 0.5f, 0f, 1f) switch {
                0f => 0,
                var alpha => new Color(Plots.Encoded(Gamut.StandardRgb, Exposure.MiddleGrey * WheelOffset.Tint(Gamut.StandardRgb, offset / diameter)), alpha).ToArgb(),
            },
        });

    private static PlotMark<WheelPart> Cue(PlotMark<WheelPart> mark, Interaction<WheelPart> cues) =>
        mark.Part.Filter(part => cues.Hovered.Exists(key => key == part.Key) || cues.Pressed.Exists(key => key == part.Key))
            .Match(Some: part => mark with { Style = part.Key.Cued }, None: () => mark);

    private static PlotMark<WheelPart> Focus(RectangleF bounds) =>
        new(new MarkShape.Band(bounds), new MarkStyle.Stroke(PaintSlot.Highlight, 1f, 1f), None);

    // --- [LIFETIME]
    protected override void Dispose(bool disposing) {
        if (disposing)
            _ = raster.Iter(static held => held.Raster.Dispose());
        raster = None;
        base.Dispose(disposing);
    }
}
