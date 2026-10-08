using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Numeric;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Editors;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RangeHandle {
    private static readonly MarkStyle TipStyle = new MarkStyle.Fill(PaintSlot.ControlText, 0.85f);
    private static readonly MarkStyle EdgeStyle = new MarkStyle.Stroke(PaintSlot.ControlText, 0.85f, Plots.Inset / 2f);

    public static readonly RangeHandle Band = new(PartRole.Slider, CursorType.Move, "{0} range", 1, None, new MarkStyle.Fill(PaintSlot.ControlText, 0.22f), 0f,
        static _ => 0f,
        static band => band.Center,
        static (band, key) => (key, band.Width, band.Softness),
        static texts => texts.Center,
        static (area, _, _) => area);
    public static readonly RangeHandle LowerSoftness = new(PartRole.Handle, CursorType.VerticalSplit, "{0} lower softness", -1, None, TipStyle, Plots.Reach,
        static band => -((band.Width / 2f) + band.Softness),
        static band => band.Softness,
        static (band, key) => (band.Center, band.Width, Fraction(key)),
        static texts => texts.Softness,
        static (_, ramp, x) => Tip(ramp, x));
    public static readonly RangeHandle UpperSoftness = new(PartRole.Handle, CursorType.VerticalSplit, "{0} upper softness", 1, None, TipStyle, Plots.Reach,
        static band => (band.Width / 2f) + band.Softness,
        static band => band.Softness,
        static (band, key) => (band.Center, band.Width, Fraction(key)),
        static texts => texts.Softness,
        static (_, ramp, x) => Tip(ramp, x));
    public static readonly RangeHandle LowerEdge = new(PartRole.Handle, CursorType.VerticalSplit, "{0} lower edge", -1, Some(LowerSoftness), EdgeStyle, Plots.Reach,
        static band => band.Width / -2f,
        static band => band.Width,
        static (band, key) => Edge(band, Fraction(key), -1f),
        static texts => texts.Width,
        static (_, ramp, x) => Rule(ramp, x));
    public static readonly RangeHandle UpperEdge = new(PartRole.Handle, CursorType.VerticalSplit, "{0} upper edge", 1, Some(UpperSoftness), EdgeStyle, Plots.Reach,
        static band => band.Width / 2f,
        static band => band.Width,
        static (band, key) => Edge(band, Fraction(key), 1f),
        static texts => texts.Width,
        static (_, ramp, x) => Rule(ramp, x));

    public PartRole Role { get; }
    public CursorType Cursor { get; }
    public string Template { get; }
    public int Sign { get; }
    public Option<RangeHandle> Alternate { get; }
    public MarkStyle Style { get; }
    public float Reach { get; }

    [UseDelegateFromConstructor]
    public partial float Offset(RangeBand band);

    [UseDelegateFromConstructor]
    public partial float Key(RangeBand band);

    [UseDelegateFromConstructor]
    public partial (float Center, float Width, float Softness) Placed(RangeBand start, float key);

    [UseDelegateFromConstructor]
    public partial NumberText<float> Text(BandTexts texts);

    [UseDelegateFromConstructor]
    public partial MarkShape Shape(MarkShape area, float ramp, float x);

    public RangeHandle Under(Keys modifiers) => modifiers.HasFlag(Keys.Alt) ? Alternate.IfNone(this) : this;

    internal static float Fraction(float key) =>
        key.CompareTo(AxisFraction.MinValue.ToValue()) < 0 ? AxisFraction.MinValue.ToValue()
        : key.CompareTo(AxisFraction.MaxValue.ToValue()) > 0 ? AxisFraction.MaxValue.ToValue()
        : key;

    private static (float Center, float Width, float Softness) Edge(RangeBand start, float width, float side) =>
        (start.Center + (side * (width - start.Width) / 2f), width, start.Softness);

    private static MarkShape Tip(float ramp, float x) =>
        new MarkShape.Polygon([
            new PointF(x, ramp + (1.5f * Plots.Inset)),
            new PointF(x + Plots.Inset, ramp + (3f * Plots.Inset)),
            new PointF(x - Plots.Inset, ramp + (3f * Plots.Inset)),
        ]);

    private static MarkShape Rule(float ramp, float x) => new MarkShape.Rule(new PointF(x, 0f), new PointF(x, ramp + (2f * Plots.Inset)));
}

public sealed record BandTexts(NumberText<float> Center, NumberText<float> Width, NumberText<float> Softness);

public sealed record RangeGrip(RangeHandle Handle, RangeBand Start, float Travel);

public sealed record HueRangeBarState(Option<HueRange> Range, Option<Func<HueRange, Vector4, Vector4>> Grade, Option<RangeGrip> Grip);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class HueRangeBar(IPlugInSink sink, RangeAxis axis, string caption, BandTexts texts)
    : ComponentControl<HueRangeBarState, RangeHandle, HueRange>(sink, new HueRangeBarState(None, None, None), None) {
    // --- [METRICS]
    private readonly float ramp = EtoFonts.SmallFont.LineHeight;
    private readonly Axis along = axis.Input.Axis.Periodic ? new Axis.Periodic(AxisRange.Of(axis.Input.Axis)) : new Axis.Linear(AxisRange.Of(axis.Input.Axis));

    // --- [SHOW]
    public IO<Unit> Graded(Option<Func<HueRange, Vector4, Vector4>> grade) => Advance(state => new(state with { Grade = grade }, None));

    // --- [LAYOUT]
    protected override Seq<PlotMark<RangeHandle>> Layout(HueRangeBarState state, SizeF size, float scale) =>
        (from range in state.Range
         let band = axis.Band.Get(range)
         let plane = Lane(new RectangleF(0f, Plots.Inset / 2f, size.Width, ramp + Plots.Inset))
         let area = Plots.Area(plane, scale, value => band.Membership(axis.Distance(along.Unit(value), band.Center)))
         select toSeq(RangeHandle.Items).Map(handle => new PlotMark<RangeHandle>(
             handle.Shape(area, ramp, plane.Point(along.Value(band.Center + handle.Offset(band)), 0f).X),
             handle.Style,
             Some(new MarkPart<RangeHandle>(handle, handle.Reach)))))
        .ToSeq()
        .Flatten();

    protected override Option<float> Fitted(HueRangeBarState state, float width) =>
        state.Grade.IsSome ? (2f * ramp) + (4f * Plots.Inset) : ramp + (3f * Plots.Inset);

    private PlotPlane.Cartesian Lane(RectangleF plot) => new(plot, along, new Axis.Linear(AxisRange.Unit));

    // --- [SAMPLING]
    private Vector4 Input(Hsy reference, float value) =>
        axis.Map(hue: true, saturation: false, luma: false) ? Plots.HueRamp(value) : axis.Replaced(reference, along.Unit(value)).ToRgb(1f);

    private static Hsy Reference(HueRange range) =>
        toSeq(RangeAxis.Items).Fold(new Hsy(0f, 0f, 0f), (hsy, item) => item.Replaced(hsy, item.Band.Get(range).Center));

    private static new Color Shown(Vector4 linear) => Plots.Encoded(Gamut.StandardRgb, linear.AsVector3());

    // --- [TRANSITIONS]
    protected override HueRangeBarState Received(HueRangeBarState state, Option<HueRange> value) =>
        state with { Range = value, Grip = None };

    protected override Option<Transition<HueRangeBarState, HueRange>> Pressed(HueRangeBarState state, SizeF size, float scale, Option<RangeHandle> part, MouseEventArgs e) =>
        from range in state.Range
        from hit in part
        where e.Buttons == MouseButtons.Primary
        select new Transition<HueRangeBarState, HueRange>(state with { Grip = new RangeGrip(hit.Under(e.Modifiers), axis.Band.Get(range), 0f) }, None);

    protected override Transition<HueRangeBarState, HueRange> Dragged(HueRangeBarState state, SizeF size, float scale, Option<RangeHandle> part, PointF origin, PointF previous, MouseEventArgs e) =>
        (from range in state.Range
         from grip in state.Grip
         let held = grip with { Travel = grip.Travel + ((e.Location.X - previous.X) / size.Width * Scale(e.Modifiers)) }
         let key = held.Handle.Key(held.Start) + (held.Handle.Sign * held.Travel)
         select Moved(state with { Grip = held }, range, held.Handle, held.Start, Snaps(e.Modifiers) ? Snapped(key, 0, size) : key, static next => new Edit<HueRange>.Preview(next)))
        .IfNone(new Transition<HueRangeBarState, HueRange>(state, None));

    protected override Transition<HueRangeBarState, HueRange> Released(HueRangeBarState state, SizeF size, float scale, Option<RangeHandle> part, MouseEventArgs e) =>
        new(state with { Grip = None }, None);

    protected override Option<Transition<HueRangeBarState, HueRange>> DoubleClicked(HueRangeBarState state, SizeF size, float scale, Option<RangeHandle> part, MouseEventArgs e) =>
        from range in state.Range
        from _ in part
        let next = axis.Band.Set(axis.Band.Get(HueRange.All), range)
        select new Transition<HueRangeBarState, HueRange>(state with { Range = next, Grip = None }, new Edit<HueRange>.Commit(next));

    protected override Option<Transition<HueRangeBarState, HueRange>> KeyPressed(HueRangeBarState state, SizeF size, float scale, Option<RangeHandle> part, KeyEventArgs e) =>
        from direction in e.Key switch { Keys.Left => Some(-1), Keys.Right => Some(1), _ => None }
        let handle = part.IfNone(RangeHandle.Band).Under(e.Modifiers)
        from next in Stepping(state, handle, start => Snaps(e.Modifiers)
            ? Snapped(handle.Key(start), handle.Sign * direction, size)
            : handle.Text(texts).Moved(handle.Key(start), handle.Sign * direction * Scale(e.Modifiers)))
        select next;

    protected override Option<Transition<HueRangeBarState, HueRange>> Stepped(HueRangeBarState state, SizeF size, float scale, RangeHandle part, int steps) =>
        Stepping(state, part, start => part.Text(texts).Moved(part.Key(start), part.Sign * steps));

    private float Snapped(float key, int steps, SizeF size) =>
        (along.Snap(along.Value(key), steps, Plots.Reach * along.Range.Span / size.Width) - along.Range.Low) / along.Range.Span;

    private Option<Transition<HueRangeBarState, HueRange>> Stepping(HueRangeBarState state, RangeHandle handle, Func<RangeBand, float> key) =>
        state.Range.Map(range => axis.Band.Get(range) switch {
            var start => Moved(state, range, handle, start, key(start), static next => new Edit<HueRange>.Step(next)),
        });

    private Transition<HueRangeBarState, HueRange> Moved(HueRangeBarState state, HueRange range, RangeHandle handle, RangeBand start, float key, Func<HueRange, Edit<HueRange>> edit) =>
        (handle.Placed(start, key) switch {
            var (center, width, softness) => (
                Conversions.Validated<AxisFraction, float, InvalidGrade>(RangeHandle.Fraction(along.Unit(along.Value(center)))),
                Conversions.Validated<AxisFraction, float, InvalidGrade>(width),
                Conversions.Validated<AxisFraction, float, InvalidGrade>(softness)),
        }).Apply((c, w, s) => axis.Band.Set(new RangeBand(c, w, s), range)).As().Match(
            Succ: next => new Transition<HueRangeBarState, HueRange>(state with { Range = next }, edit(next)),
            Fail: _ => new Transition<HueRangeBarState, HueRange>(state, None));

    // --- [ACCESSIBILITY]
    protected override PartFacet Facet(HueRangeBarState state, RangeHandle key) =>
        new(key.Role, Cursors.Cached(key.Cursor), RowText.Localize(key.Template, arguments: [caption]).Local,
            key.Text(texts).Shown(state.Range.Map(range => key.Key(axis.Band.Get(range)))), None);

    // --- [PAINT]
    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, HueRangeBarState state, Seq<PlotMark<RangeHandle>> marks, Interaction<RangeHandle> interaction) =>
        Reference(state.Range.IfNone(HueRange.All)) switch {
            var reference =>
                from input in Plots.Strip(canvas, Lane(new RectangleF(0f, Plots.Inset, bounds.Width, ramp)), value => Shown(Input(reference, value)))
                from parts in Plots.Paint(canvas, marks.Map(mark => Cued(mark, canvas.Focused, interaction)))
                from adjusted in (state.Range, state.Grade).Apply(static (range, grade) => (Range: range, Grade: grade)).As()
                    .TraverseM(shown => Plots.Strip(canvas, Lane(new RectangleF(0f, ramp + (4f * Plots.Inset), bounds.Width, ramp)), value => Shown(shown.Grade(shown.Range, Input(reference, value)))))
                    .As()
                select unit,
        };

    private static PlotMark<RangeHandle> Cued(PlotMark<RangeHandle> mark, bool focused, Interaction<RangeHandle> cues) =>
        mark with {
            Style = from style in mark.Style
                    from part in mark.Part
                    select Restyled(
                        style,
                        focused && part.Key.Role == PartRole.Slider ? new MarkColor.Themed(PaintSlot.Highlight) : style.Paint,
                        part.Key.Role == PartRole.Handle && (cues.Hovered == Some(part.Key) || cues.Pressed == Some(part.Key)) ? 1f : style.Alpha),
        };

    private static MarkStyle Restyled(MarkStyle style, MarkColor paint, float alpha) =>
        style.Switch<(MarkColor Paint, float Alpha), MarkStyle>(
            (paint, alpha),
            fill: static (look, _) => new MarkStyle.Fill(look.Paint, look.Alpha),
            stroke: static (look, stroke) => new MarkStyle.Stroke(look.Paint, look.Alpha, stroke.Width));
}
