using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Numeric;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Editors;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RampPart {
    public sealed record Bar : RampPart;
    public sealed record Knot(int Index) : RampPart;
}

public sealed record RampGrip(Ramp Start, RampStop Held, Option<int> Index, double Raw);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GradientRampState {
    public sealed record Editor(Gamut Gamut, NumberText<double> Position, Option<Ramp> Shown, Option<int> Selected, Option<RampGrip> Grip) : GradientRampState;
    public sealed record Key(Option<AgXLook> Look) : GradientRampState;
}

public sealed class StopSelectedEventArgs(Option<int> index) : EventArgs {
    public Option<int> Index { get; } = index;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GradientRamp(IPlugInSink sink, string caption, GradientRampState initial)
    : ComponentControl<GradientRampState, RampPart, Ramp>(sink, initial, None) {
    // --- [SHOW]
    public event EventHandler<StopSelectedEventArgs>? Selected;

    public IO<Unit> Select(Option<int> index) =>
        Advance(state => new(state.Switch<Option<int>, GradientRampState>(index,
            editor: static (held, editor) => editor with { Selected = Within(held, editor.Shown) },
            key: static (_, legend) => legend), None));

    protected override GradientRampState Received(GradientRampState state, Option<Ramp> value) =>
        state.Switch<Option<Ramp>, GradientRampState>(value,
            editor: static (shown, editor) => editor with { Shown = shown, Selected = Within(editor.Selected, shown), Grip = None },
            key: static (_, legend) => legend);

    protected override IO<Unit> Reconcile(GradientRampState before, GradientRampState after, Interaction<RampPart> was, Interaction<RampPart> now) =>
        when(Selection(before) != Selection(after), IO.lift(fun(() => Selected?.Invoke(this, new StopSelectedEventArgs(Selection(after)))))).As();

    private static Option<int> Within(Option<int> index, Option<Ramp> shown) => index.Filter(at => shown.Exists(ramp => at < ramp.Stops.Count));

    private static Option<int> Selection(GradientRampState state) => state.Switch(editor: static editor => editor.Selected, key: static _ => Option<int>.None);

    // --- [LAYOUT]
    private const float BarHeight = 24f;
    private const float Gap = 1f;
    private const float KnotWidth = 8f;
    private const float KnotHeight = 7f;

    protected override Option<float> Fitted(GradientRampState state, float width) =>
        BarHeight + Gap + state.Switch(editor: static _ => KnotHeight, key: static _ => EtoFonts.SmallFont.LineHeight);

    protected override Seq<PlotMark<RampPart>> Layout(GradientRampState state, SizeF size, float scale) =>
        new PlotMark<RampPart>(new MarkShape.Band(Bar(state, size)), Frame, Some(new MarkPart<RampPart>(new RampPart.Bar(), 0f)))
            .Cons(state.Switch(size,
                editor: static (held, editor) => Knots(editor, Plane(Bar(editor, held))),
                key: static (_, _) => Seq<PlotMark<RampPart>>()));

    protected override PartFacet Facet(GradientRampState state, RampPart key) =>
        state.Switch((Key: key, Caption: caption),
            editor: static (held, editor) => held.Key.Switch((held.Caption, Editor: editor),
                bar: static (shown, _) => new PartFacet(PartRole.Button, Cursors.Crosshair, shown.Caption, None, None),
                knot: static (shown, knot) => new PartFacet(PartRole.Handle, Cursors.VerticalSplit, RowText.Localize("Stop {0}", arguments: [knot.Index + 1]).Local,
                    shown.Editor.Shown.Bind(ramp => ramp.Stops.At(knot.Index)).Map(stop => shown.Editor.Position.Shown(Some((double)stop.Position))), None)),
            key: static (held, legend) => new PartFacet(PartRole.Image, Cursors.Default, held.Caption, string.Join(", ", KeyLabels(legend.Look).Map(static label => label.Text)), None));

    private static RectangleF Bar(GradientRampState state, SizeF size) =>
        state.Switch(size,
            editor: static (held, _) => new RectangleF(KnotWidth / 2f, 0f, held.Width - KnotWidth, BarHeight),
            key: static (held, _) => new RectangleF(0f, 0f, held.Width, BarHeight));

    private static PlotPlane.Cartesian Plane(RectangleF plot) => new(plot, new Axis.Linear(AxisRange.Unit), new Axis.Linear(AxisRange.Unit));

    private static Seq<PlotMark<RampPart>> Knots(GradientRampState.Editor editor, PlotPlane.Cartesian plane) =>
        editor.Shown.ToSeq()
            .Bind(static ramp => ramp.Stops)
            .Map((stop, index) => (Stop: stop, Index: index, Current: editor.Selected.Exists(at => at == index)))
            .Partition(static knot => knot.Current) switch {
                var (current, rest) => (current + rest).Map(knot => new PlotMark<RampPart>(
                    Triangle(plane.Point((float)(double)knot.Stop.Position, 0f).X, plane.Plot.Bottom + Gap),
                    knot.Current ? KnotSelected : KnotStroke,
                    Some(new MarkPart<RampPart>(new RampPart.Knot(knot.Index), Plots.Reach)))),
            };

    private static MarkShape.Polygon Triangle(float x, float top) =>
        new([new PointF(x, top), new PointF(x + (KnotWidth / 2f), top + KnotHeight), new PointF(x - (KnotWidth / 2f), top + KnotHeight)]);

    // --- [PAINT]
    private const float Opaque = 0.25f;

    private static readonly MarkStyle Frame = new MarkStyle.Stroke(PaintSlot.ControlText, 0.7f, 1f);
    private static readonly MarkStyle KnotStroke = new MarkStyle.Stroke(PaintSlot.ControlText, 1f, 1f);
    private static readonly MarkStyle KnotSelected = new MarkStyle.Fill(PaintSlot.ControlText, 1f);
    private static readonly MarkStyle KnotFocused = new MarkStyle.Fill(PaintSlot.Highlight, 1f);

    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, GradientRampState state, Seq<PlotMark<RampPart>> marks, Interaction<RampPart> interaction) =>
        Bar(state, bounds.Size) switch {
            var bar =>
                from well in Plots.Well(canvas, bar)
                from fill in state.Switch((Canvas: canvas, Bar: bar, Bounds: bounds),
                    editor: static (held, editor) => editor.Shown.Match(
                        Some: ramp => Columns(held.Canvas, held.Bar, ramp.Tabulate(editor.Gamut), editor.Gamut),
                        None: static () => IO.pure(unit)),
                    key: static (held, legend) => Legend(held.Canvas, held.Bar, held.Bounds, legend.Look))
                from veil in Veiled(canvas, bar)
                from painted in Plots.Paint(canvas, marks.Map(mark => canvas.Focused && mark.Style == Some(KnotSelected) ? mark with { Style = KnotFocused } : mark))
                select unit,
        };

    private static IO<Unit> Columns(PlotCanvas canvas, RectangleF bar, RampTable table, Gamut gamut) =>
        from checker in Thumbnails.Checker(canvas, bar, Checkerboard.Content)
        from blended in Plots.Strip(canvas, Plane(bar with { Height = (1f - Opaque) * bar.Height }), x => table.Sample(x) switch {
            var light => new Color(Plots.Encoded(gamut, light.AsVector3()), light.W),
        })
        from opaque in Plots.Strip(canvas, Plane(RectangleF.FromSides(bar.Left, bar.Bottom - (Opaque * bar.Height), bar.Right, bar.Bottom)), x => Plots.Encoded(gamut, table.Sample(x).AsVector3()))
        select unit;

    private static IO<Unit> Legend(PlotCanvas canvas, RectangleF bar, RectangleF bounds, Option<AgXLook> look) =>
        FalseColor.Key(look).Ramp.Tabulate(Gamut.StandardRgb) switch {
            var table =>
                from bands in Plots.Strip(canvas, Plane(bar), x => Displayed(table.Sample(x)))
                from labels in Plots.Labels(canvas, Plane(bounds), LabelSide.Bottom, KeyLabels(look))
                select unit,
        };

    private static IO<Unit> Veiled(PlotCanvas canvas, RectangleF bar) =>
        IO.lift(() => HostTheme.Veil(canvas.Slots, canvas.Slots[PaintSlot.ControlBackground])
            .Filter(_ => !canvas.Enabled)
            .Iter(alpha => canvas.Graphics.FillRectangle(new Color(canvas.Slots[PaintSlot.ControlBackground], alpha), bar)));

    private static Color Displayed(Vector4 light) {
        Span<Vector4> row = [light];
        Imaging.Tone.Formations.Display.Rec1886.Encoding.Encode(row);
        return new Color(row[0].X, row[0].Y, row[0].Z);
    }

    private static Seq<AxisLabel> KeyLabels(Option<AgXLook> look) =>
        (NumberText.Scalar(Exposure.Presentation, ""), FalseColor.Key(look)) switch {
            var (text, key) => key.Ramp.Stops.Zip(key.Stops)
                .Map(band => band.Second.Map(stop => new AxisLabel((float)(double)band.First.Position, text.Shown(Some(stop)))))
                .Somes(),
        };

    // --- [STOPS]
    private static Option<GradientRampState.Editor> Editing(GradientRampState state) =>
        state.Switch<Option<GradientRampState.Editor>>(editor: static editor => editor, key: static _ => None);

    private static Option<RampPosition> Located(double raw) => Conversions.Validated<RampPosition, double, InvalidPixelValue>(raw).ToOption();

    private static Fin<(Ramp Ramp, int Index, RampStop Stop)> Added(GradientRampState.Editor editor, Ramp ramp, RampPosition at) =>
        from stop in RampStop.Validate(at, ramp.Tabulate(editor.Gamut).Sample(at), out RampStop item) is { } error ? error : (Fin<RampStop>)item
        from added in Inserted(ramp, stop)
        select (added.Ramp, added.Index, stop);

    private static Fin<(Ramp Ramp, int Index)> Inserted(Ramp ramp, RampStop stop) =>
        ramp.Stops.Filter(held => held.Position <= stop.Position).Count switch {
            var at => Ramp.Validate(ramp.Stops.Take(at).Add(stop).Concat(ramp.Stops.Skip(at)), ramp.Interpolation, out Ramp? added) is { } error
                ? error
                : (Fin<(Ramp Ramp, int Index)>)(added!, at),
        };

    private static Fin<(GradientRampState.Editor Editor, Ramp Remaining)> Dropped(GradientRampState.Editor editor, Ramp ramp, int index) =>
        Ramp.Validate(ramp.Stops.Take(index).Concat(ramp.Stops.Skip(index + 1)), ramp.Interpolation, out Ramp? rest) is { } error
            ? error
            : (Fin<(GradientRampState.Editor Editor, Ramp Remaining)>)(editor with { Shown = rest!, Selected = int.Max(index - 1, 0) }, rest!);

    private static Option<Transition<GradientRampState, Ramp>> Stepping(GradientRampState.Editor editor, Ramp ramp, int index, double steps) =>
        Located(editor.Position.Moved(ramp.Stops[index].Position, steps)).Map(position => ramp.Moved(index, position) switch {
            var (moved, at) => new Transition<GradientRampState, Ramp>(editor with { Shown = moved, Selected = at }, new Edit<Ramp>.Step(moved)),
        });

    // --- [POINTER]
    protected override Option<Transition<GradientRampState, Ramp>> Pressed(GradientRampState state, SizeF size, float scale, Option<RampPart> part, MouseEventArgs e) =>
        from editor in Editing(state)
        from ramp in editor.Shown
        from hit in part
        from at in Located(Plane(Bar(editor, size)).Value(e.Location).First)
        from next in hit.Switch((Editor: editor, Ramp: ramp, At: at),
            bar: static (held, _) => Added(held.Editor, held.Ramp, held.At).ToOption().Map(added => new Transition<GradientRampState, Ramp>(
                held.Editor with { Shown = added.Ramp, Selected = added.Index, Grip = new RampGrip(held.Ramp, added.Stop, added.Index, held.At) },
                new Edit<Ramp>.Preview(added.Ramp))),
            knot: static (held, knot) => Some(new Transition<GradientRampState, Ramp>(
                held.Editor with { Selected = knot.Index, Grip = new RampGrip(held.Ramp, held.Ramp.Stops[knot.Index], knot.Index, held.Ramp.Stops[knot.Index].Position) },
                None)))
        select next;

    protected override Transition<GradientRampState, Ramp> Dragged(GradientRampState state, SizeF size, float scale, Option<RampPart> part, PointF origin, PointF previous, MouseEventArgs e) =>
        (from editor in Editing(state)
         from ramp in editor.Shown
         from grip in editor.Grip.Map(held => held with { Raw = held.Raw + ((e.Location.X - previous.X) / Bar(editor, size).Width * Scale(e.Modifiers)) })
         select Held(editor, ramp, grip, Inside(size, e.Location), Located(Snaps(e.Modifiers) ? editor.Position.Snapped(grip.Raw) : grip.Raw)))
        .IfNone(new Transition<GradientRampState, Ramp>(state, None));

    protected override Transition<GradientRampState, Ramp> Released(GradientRampState state, SizeF size, float scale, Option<RampPart> part, MouseEventArgs e) =>
        (from editor in Editing(state)
         from grip in editor.Grip
         select new Transition<GradientRampState, Ramp>(
             editor with { Grip = None },
             editor.Shown == Some(grip.Start) ? Some<Edit<Ramp>>(new Edit<Ramp>.Cancel()) : None))
        .IfNone(new Transition<GradientRampState, Ramp>(state, None));

    protected override Option<Transition<GradientRampState, Ramp>> DoubleClicked(GradientRampState state, SizeF size, float scale, Option<RampPart> part, MouseEventArgs e) =>
        from editor in Editing(state)
        from _ in part
        select new Transition<GradientRampState, Ramp>(editor, None);

    private static bool Inside(SizeF size, PointF point) => RectangleF.Inflate(new RectangleF(size), new SizeF(Plots.Reach, Plots.Reach)).Contains(point);

    private static Transition<GradientRampState, Ramp> Held(GradientRampState.Editor editor, Ramp ramp, RampGrip grip, bool inside, Option<RampPosition> at) =>
        (inside
            ? from position in at
              from placed in grip.Index.Match(Some: index => Some(ramp.Moved(index, position)), None: () => Inserted(ramp, grip.Held.At(position)).ToOption())
              select new Transition<GradientRampState, Ramp>(
                  editor with { Shown = placed.Ramp, Selected = placed.Index, Grip = grip with { Index = placed.Index } },
                  new Edit<Ramp>.Preview(placed.Ramp))
            : from index in grip.Index
              from dropped in Dropped(editor, ramp, index).ToOption()
              select new Transition<GradientRampState, Ramp>(dropped.Editor with { Grip = grip with { Index = None } }, new Edit<Ramp>.Preview(dropped.Remaining)))
        .IfNone(new Transition<GradientRampState, Ramp>(editor with { Grip = grip }, None));

    // --- [KEYS]
    protected override Option<Transition<GradientRampState, Ramp>> KeyPressed(GradientRampState state, SizeF size, float scale, Option<RampPart> part, KeyEventArgs e) =>
        from editor in Editing(state)
        from ramp in editor.Shown
        from index in editor.Selected
        from next in e.Key switch {
            Keys.Left => Stepping(editor, ramp, index, -Scale(e.Modifiers)),
            Keys.Right => Stepping(editor, ramp, index, Scale(e.Modifiers)),
            Keys.Delete or Keys.Backspace => Some(Dropped(editor, ramp, index).Match(
                Succ: static dropped => new Transition<GradientRampState, Ramp>(dropped.Editor, new Edit<Ramp>.Step(dropped.Remaining)),
                Fail: _ => new Transition<GradientRampState, Ramp>(editor, None))),
            _ => None,
        }
        select next;

    // --- [ACCESSIBILITY]
    protected override Option<Transition<GradientRampState, Ramp>> Stepped(GradientRampState state, SizeF size, float scale, RampPart part, int steps) =>
        from editor in Editing(state)
        from ramp in editor.Shown
        from index in part.Switch<Option<int>>(bar: static _ => None, knot: static knot => knot.Index)
        from next in Stepping(editor, ramp, index, steps)
        select next;

    protected override Option<Transition<GradientRampState, Ramp>> Activated(GradientRampState state, SizeF size, float scale, RampPart part) =>
        from editor in Editing(state)
        from ramp in editor.Shown
        from at in Midway(ramp, editor.Selected)
        from added in Added(editor, ramp, at).ToOption()
        select new Transition<GradientRampState, Ramp>(editor with { Shown = added.Ramp, Selected = added.Index }, new Edit<Ramp>.Step(added.Ramp));

    private static Option<RampPosition> Midway(Ramp ramp, Option<int> selected) =>
        Located(ramp.Stops.Count == 1
            ? ((double)RampPosition.MinValue + RampPosition.MaxValue) / 2d
            : selected.Filter(static index => index > 0).Match(
                Some: index => ((double)ramp.Stops[index - 1].Position + ramp.Stops[index].Position) / 2d,
                None: () => ((double)ramp.Stops[0].Position + ramp.Stops[1].Position) / 2d));
}
