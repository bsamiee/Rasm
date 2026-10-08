using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Numeric;

namespace Rasm.Rhino.UI.Editors;

// --- [MODELS] --------------------------------------------------------------------------
[Union<TraceChannel, HueCurveKind>(T1Name = "Channel", T2Name = "Hue")]
public sealed partial class CurveKey;

public abstract record PlottedCurve {
    private PlottedCurve() { }

    public abstract CurveKey Key { get; }
    public abstract CurveAxis Input { get; }
    public abstract CurveAxis Output { get; }
    public abstract Seq<CurvePoint> Points { get; }
    public abstract PlottedCurve Identity { get; }
    public abstract MarkColor Paint { get; }
    public abstract Option<TraceChannel> Plane { get; }
    public abstract Func<double, double> Fit();
    public abstract Fin<PlottedCurve> With(Seq<CurvePoint> points);

    public bool Ascending => Input == Output;

    public sealed record Channel(TraceChannel Trace, PointCurve Value) : PlottedCurve {
        public override CurveKey Key => Trace;
        public override CurveAxis Input => CurveAxis.Stops;
        public override CurveAxis Output => CurveAxis.Stops;
        public override Seq<CurvePoint> Points => Value.Points;
        public override PlottedCurve Identity => this with { Value = PointCurve.Identity };
        public override MarkColor Paint => Trace;
        public override Option<TraceChannel> Plane => Some(Trace);
        public override Func<double, double> Fit() => Value.Fit();

        public override Fin<PlottedCurve> With(Seq<CurvePoint> points) =>
            PointCurve.Validate(points, out PointCurve? curve) is { } error ? error : (Fin<PlottedCurve>)(this with { Value = curve! });
    }

    public sealed record Hue<TKind>(HueCurve<TKind> Value) : PlottedCurve where TKind : IHueCurveKind {
        public override CurveKey Key => TKind.Kind;
        public override CurveAxis Input => TKind.Kind.Input.Axis;
        public override CurveAxis Output => TKind.Kind.Output;
        public override Seq<CurvePoint> Points => Value.Points;
        public override PlottedCurve Identity => this with { Value = HueCurve<TKind>.Identity };
        public override MarkColor Paint => PaintSlot.ControlText;
        public override Option<TraceChannel> Plane => Callbacks.Found(TKind.Kind.Input == InputAxis.Stops, TraceChannel.Luma);
        public override Func<double, double> Fit() => Value.Fit();

        public override Fin<PlottedCurve> With(Seq<CurvePoint> points) =>
            HueCurve<TKind>.Validate(points, out HueCurve<TKind>? curve) is { } error ? error : (Fin<PlottedCurve>)(this with { Value = curve! });
    }
}

public sealed record CurveAnchor(CurvePoint Point, CurveAxis Input, CurveAxis Output);

public sealed record CurveText(string Caption, Func<CurveKey, string> Curve, Func<CurveAxis, NumberText<double>> Axis);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurvePart {
    public sealed record Ground : CurvePart;
    public sealed record Fitted : CurvePart;
    public sealed record Knot(int Index) : CurvePart;
}

[Union]
public abstract partial record CurveEditorState {
    public sealed record Editing(CurveKey Active, HashMap<CurveKey, PlottedCurve> Curves, Seq<int> Selected, bool ShowsHistogram, Option<Traces> Histogram) : CurveEditorState {
        public Option<PlottedCurve> Current => Curves.Find(Active);

        public Editing Holding(PlottedCurve curve) => this with { Curves = Curves.AddOrUpdate(curve.Key, curve) };
    }

    public sealed record Graph(AxisRange Input, Option<Func<float, float>> Transfer, Seq<AxisLabel> Guides, Func<float, string> Output) : CurveEditorState;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class CurveEditor : ComponentControl<CurveEditorState, CurvePart, PlottedCurve> {
    // --- [ROWS]
    private const double AnchorShare = 12d;

    private readonly CurveText text;
    private readonly Command resetCurve;
    private readonly CheckCommand inputHistogram;

    public CurveEditor(IPlugInSink sink, CurveEditorState initial, CurveText text, Option<IO<Option<Traces>>> histogram)
        : base(sink, initial, histogram.Map(static read => read.Map(static traces => (Func<CurveEditorState, CurveEditorState>)(state => Sampled(state, traces))))) {
        this.text = text;
        resetCurve = new Command(Callbacks.Handler<EventArgs>(_ => Advance(Reset), Site(nameof(Reset)))) { MenuText = RowText.Localize("Reset Curve").Local };
        inputHistogram = new CheckCommand(Callbacks.Handler<EventArgs>(_ => Advance(Toggled), Site(nameof(Toggled)))) { MenuText = RowText.Localize("Input Histogram").Local };
        _ = Commanded(initial);
    }

    public event EventHandler<Option<CurveAnchor>>? Anchored;

    public Seq<Command> Commands => Seq<Command>(resetCurve, inputHistogram);

    public IO<Unit> Choose(CurveKey key) =>
        Advance(state => new(state.Switch<CurveKey, CurveEditorState>(key,
            editing: static (chosen, editing) => editing with { Active = chosen, Selected = Seq<int>() },
            graph: static (_, graph) => graph), None));

    public IO<Unit> Enter(CurveAnchor anchor) =>
        Advance(state =>
            (from plotted in Plotting(state, Size)
             from index in plotted.State.Selected.Last
             from point in plotted.Curve.Points.At(index)
             from moved in plotted.Moving(Seq(index), anchor.Point.X - point.X, anchor.Point.Y - point.Y)
             select moved.Raised(static curve => new Edit<PlottedCurve>.Commit(curve)))
            .IfNone(new Transition<CurveEditorState, PlottedCurve>(state, None)));

    public IO<Unit> Pick(Func<CurveKey, double> input) =>
        Advance(state =>
            (from plotted in Plotting(state, Size)
             let placed = Picks(plotted.Curve, input(plotted.State.Active)).Fold(plotted, static (held, at) => held.Inserting(at).IfNone(held))
             where placed != plotted
             select placed.Raised(static curve => new Edit<PlottedCurve>.Commit(curve)))
            .IfNone(new Transition<CurveEditorState, PlottedCurve>(state, None)));

    public IO<Unit> Plot(AxisRange input, Option<Func<float, float>> transfer, Seq<AxisLabel> guides, Func<float, string> output) =>
        Advance(_ => new(new CurveEditorState.Graph(input, transfer, guides, output), None));

    protected override CurveEditorState Received(CurveEditorState state, Option<PlottedCurve> value) =>
        state.Switch<Option<PlottedCurve>, CurveEditorState>(value,
            editing: static (pushed, editing) => pushed.Match(
                Some: curve => editing.Holding(curve) switch {
                    var held => held with { Selected = held.Current.Map(shown => held.Selected.Filter(index => index < shown.Points.Count)).IfNone(Seq<int>()) },
                },
                None: () => editing with { Curves = HashMap<CurveKey, PlottedCurve>(), Selected = Seq<int>() }),
            graph: static (_, graph) => graph);

    protected override IO<Unit> Reconcile(CurveEditorState before, CurveEditorState after, Interaction<CurvePart> was, Interaction<CurvePart> now) =>
        from commands in IO.lift(() => Commanded(after))
        from anchored in when(Anchor(before) != Anchor(after), IO.lift(fun(() => Anchored?.Invoke(this, Anchor(after))))).As()
        select unit;

    private Unit Commanded(CurveEditorState state) {
        Option<PlottedCurve> current = Editing(state).Bind(static editing => editing.Current);
        resetCurve.Enabled = current.Exists(static curve => curve != curve.Identity);
        inputHistogram.Checked = Editing(state).Exists(static editing => editing.ShowsHistogram);
        inputHistogram.Enabled = current.Exists(static curve => curve.Plane.IsSome);
        return unit;
    }

    private static CurveEditorState Sampled(CurveEditorState state, Option<Traces> traces) =>
        state.Switch<Option<Traces>, CurveEditorState>(traces,
            editing: static (read, editing) => editing with { Histogram = read },
            graph: static (_, graph) => graph);

    private static Option<CurveAnchor> Anchor(CurveEditorState state) =>
        from editing in Editing(state)
        from curve in editing.Current
        from index in editing.Selected.Last
        from point in curve.Points.At(index)
        select new CurveAnchor(point, curve.Input, curve.Output);

    private static Transition<CurveEditorState, PlottedCurve> Reset(CurveEditorState state) =>
        (from editing in Editing(state)
         from curve in editing.Current
         select new Transition<CurveEditorState, PlottedCurve>(
             editing.Holding(curve.Identity) with { Selected = Seq<int>() },
             new Edit<PlottedCurve>.Commit(curve.Identity)))
        .IfNone(new Transition<CurveEditorState, PlottedCurve>(state, None));

    private static Transition<CurveEditorState, PlottedCurve> Toggled(CurveEditorState state) =>
        new(state.Switch<CurveEditorState>(editing: static editing => editing with { ShowsHistogram = !editing.ShowsHistogram }, graph: static graph => graph), None);

    private static Seq<double> Picks(PlottedCurve curve, double input) =>
        curve.Key.Switch(
            (Input: input, Offset: (curve.Input.Span.High - curve.Input.Span.Low) / AnchorShare),
            channel: static (held, _) => Seq(held.Input),
            hue: static (held, _) => Seq(held.Input - held.Offset, held.Input + held.Offset, held.Input));

    // --- [LAYOUT]
    private const float GridSpacing = 30f;
    private const float KnotDiameter = 6f;
    private const float ActiveWidth = 1.5f;
    private const float OtherAlpha = 0.5f;
    private const float HistogramAlpha = 0.15f;

    private static readonly MarkStyle Neutral = new MarkStyle.Stroke(PaintSlot.ControlText, 0.14f, 1f);
    private static readonly MarkStyle Hollow = new MarkStyle.Fill(PaintSlot.ControlBackground, 1f);

    protected override Option<float> Fitted(CurveEditorState state, float width) => Some(width);

    protected override Seq<PlotMark<CurvePart>> Layout(CurveEditorState state, SizeF size, float scale) =>
        new PlotMark<CurvePart>(new MarkShape.Band(new RectangleF(size)), None, Some(new MarkPart<CurvePart>(new CurvePart.Ground(), 0f)))
            .Cons(state.Switch((Size: size, Scale: scale),
                editing: static (held, editing) => Plotted.Of(editing, held.Size).ToSeq().Bind(plotted => plotted.Marks(held.Scale)),
                graph: static (held, graph) => Graphed(graph, held.Size, held.Scale)));

    protected override PartFacet Facet(CurveEditorState state, CurvePart key) =>
        state.Switch((Key: key, Text: text, Size: Size, Pointer: Pointer),
            editing: static (held, editing) => Indexed(held.Key).Match(
                Some: index => new PartFacet(
                    PartRole.Handle, Cursors.Pointer, RowText.Localize("{0} point {1}", arguments: [held.Text.Curve(editing.Active), index + 1]).Local,
                    from curve in editing.Current
                    from point in curve.Points.At(index)
                    select string.Join(", ", held.Text.Axis(curve.Input).Shown(Some(point.X)), held.Text.Axis(curve.Output).Shown(Some(point.Y))),
                    None),
                None: () => new PartFacet(PartRole.Image, Cursors.Crosshair, held.Text.Curve(editing.Active), None, None)),
            graph: static (held, graph) => new PartFacet(
                PartRole.Image, Cursors.Default, held.Text.Caption,
                from at in held.Pointer
                from transfer in graph.Transfer
                select graph.Output(transfer(GraphPlane(graph, held.Size).Value(at).First)),
                None));

    private static Option<CurveEditorState.Editing> Editing(CurveEditorState state) =>
        state.Switch<Option<CurveEditorState.Editing>>(editing: static editing => editing, graph: static _ => None);

    private static Option<Plotted> Plotting(CurveEditorState state, SizeF size) => Editing(state).Bind(editing => Plotted.Of(editing, size));

    private static Option<int> Indexed(CurvePart part) =>
        part.Switch<Option<int>>(ground: static _ => None, fitted: static _ => None, knot: static knot => knot.Index);

    private static float Step(Axis axis, float side) => axis.Range.Span / side;

    private static PlotPlane.Cartesian GraphPlane(CurveEditorState.Graph graph, SizeF size) =>
        new(Plots.Plot(new RectangleF(size)), new Axis.Linear(graph.Input), new Axis.Linear(AxisRange.Unit));

    private static Seq<float> GraphGrid(PlotPlane.Cartesian plane) => plane.X.Grid(GridSpacing * Step(plane.X, plane.Plot.Width));

    private static Seq<PlotMark<CurvePart>> Graphed(CurveEditorState.Graph graph, SizeF size, float scale) =>
        GraphPlane(graph, size) switch {
            var plane => Plots.Rules(plane, Orientation.Vertical, GraphGrid(plane))
                .Concat(Plots.Rules(plane, Orientation.Horizontal, graph.Guides.Map(static guide => guide.Value)))
                .Map(static rule => new PlotMark<CurvePart>(rule, MarkStyle.Grid, None))
                .Concat(graph.Transfer.ToSeq().Map(transfer => new PlotMark<CurvePart>(Plots.Sampled(plane, scale, transfer), new MarkStyle.Stroke(PaintSlot.ControlText, 1f, ActiveWidth), None))),
        };

    private static Func<float, float> Sampler(PlottedCurve curve) =>
        curve.Fit() switch {
            var fit => input => (float)fit(input),
        };

    private sealed record Plotted(CurveEditorState.Editing State, PlottedCurve Curve, PlotPlane.Cartesian Placed, PlotPlane.Cartesian Traced) {
        // --- [PLANES]
        public float StepX => Step(Placed.X, Placed.Plot.Width);
        public float StepY => Step(Placed.Y, Placed.Plot.Height);
        public float LeastX => GridSpacing * StepX;
        public float LeastY => GridSpacing * StepY;
        public Option<CurvePoint> Anchor => State.Selected.Last.Bind(index => Curve.Points.At(index));

        public static Option<Plotted> Of(CurveEditorState.Editing state, SizeF size) =>
            state.Current.Map(curve => Plots.Plot(new RectangleF(size)) switch {
                var plot => new Plotted(
                    state, curve,
                    new PlotPlane.Cartesian(plot, Placing(curve.Input), Placing(curve.Output)),
                    new PlotPlane.Cartesian(plot, new Axis.Linear(AxisRange.Of(curve.Input)), new Axis.Linear(AxisRange.Of(curve.Output)))),
            });

        private static Axis Placing(CurveAxis axis) => axis.Periodic ? new Axis.Periodic(AxisRange.Of(axis)) : new Axis.Linear(AxisRange.Of(axis));

        // --- [MARKS]
        public Seq<PlotMark<CurvePart>> Marks(float scale) =>
            Histogram(scale).ToSeq()
                .Concat(Plots.Rules(Placed, Orientation.Vertical, Placed.X.Grid(LeastX))
                    .Concat(Plots.Rules(Placed, Orientation.Horizontal, Placed.Y.Grid(LeastY)))
                    .Map(static rule => new PlotMark<CurvePart>(rule, MarkStyle.Grid, None)))
                .Concat(Plots.Rules(Placed, Orientation.Vertical, Curve.Input.Origin.Map(static origin => (float)origin).ToSeq())
                    .Concat(Plots.Rules(Placed, Orientation.Horizontal, Curve.Output.Origin.Map(static origin => (float)origin).ToSeq()))
                    .Map(static rule => new PlotMark<CurvePart>(rule, Neutral, None)))
                .Concat(Curve.Ascending
                    ? Seq(new PlotMark<CurvePart>(new MarkShape.Rule(Traced.Point(Traced.X.Range.Low, Traced.Y.Range.Low), Traced.Point(Traced.X.Range.High, Traced.Y.Range.High)), MarkStyle.Grid, None))
                    : Seq<PlotMark<CurvePart>>())
                .Concat(toSeq(State.Curves.Remove(Curve.Key).Values)
                    .Filter(other => other.Input == Curve.Input && other.Output == Curve.Output)
                    .Map(other => new PlotMark<CurvePart>(Plots.Sampled(Traced, scale, Sampler(other)), new MarkStyle.Stroke(other.Paint, OtherAlpha, 1f), None)))
                .Add(new PlotMark<CurvePart>(
                    Plots.Sampled(Traced, scale, Sampler(Curve)), new MarkStyle.Stroke(Curve.Paint, 1f, ActiveWidth), new MarkPart<CurvePart>(new CurvePart.Fitted(), Plots.Reach)))
                .Concat(Knots);

        private Seq<PlotMark<CurvePart>> Knots =>
            from knot in Curve.Points.Map(static (point, index) => (Point: point, Index: index))
            from shift in Curve.Input.Periodic ? Seq(0f, -Traced.X.Range.Span, Traced.X.Range.Span) : Seq(0f)
            let disc = new MarkShape.Disc(Traced.Point((float)knot.Point.X + shift, (float)knot.Point.Y), KnotDiameter)
            let part = new MarkPart<CurvePart>(new CurvePart.Knot(knot.Index), Plots.Reach)
            from mark in State.Selected.Exists(index => index == knot.Index)
                ? Seq(new PlotMark<CurvePart>(disc, new MarkStyle.Fill(Curve.Paint, 1f), part))
                : Seq(new PlotMark<CurvePart>(disc, Hollow, None), new PlotMark<CurvePart>(disc, new MarkStyle.Stroke(Curve.Paint, 1f, 1f), part))
            select mark;

        private Option<PlotMark<CurvePart>> Histogram(float scale) =>
            from plane in Curve.Plane
            from traces in State.Histogram
            where State.ShowsHistogram
            select new PlotMark<CurvePart>(Columns(plane.Bins(traces), (int)MathF.Ceiling(Placed.Plot.Width * scale)), new MarkStyle.Fill(plane, HistogramAlpha), None);

        private MarkShape.Polygon Columns(BinGrid bins, int columns) =>
            toSeq(bins.Counts.ToArray())
                .Map((count, bin) => (Column: int.Clamp((int)(Placed.X.Unit((float)CurveAxis.ToStops(HistogramAxis.LowerBound(bin))) * columns), 0, columns - 1), Count: count))
                .Fold(HashMap<int, int>(), static (held, bin) => held.AddOrUpdate(bin.Column, total => total + bin.Count, bin.Count)) switch {
                var counts => counts.Fold(1, int.Max) switch {
                    var tallest => new MarkShape.Polygon([
                        new PointF(Placed.Plot.Left, Placed.Plot.Bottom),
                        .. toSeq(Prelude.Range(0, columns)).Map(column => new PointF(
                            Placed.Plot.Left + ((column + 0.5f) * Placed.Plot.Width / columns),
                            Placed.Plot.Bottom - ((float)counts.Find(column).IfNone(0) / tallest * Placed.Plot.Height))),
                        new PointF(Placed.Plot.Right, Placed.Plot.Bottom),
                    ]),
                },
            };

        // --- [EDITS]
        public Transition<CurveEditorState, PlottedCurve> Raised(Func<PlottedCurve, Edit<PlottedCurve>> edit) => new(State, Some(edit(Curve)));

        public Option<Plotted> Rewritten(Seq<CurvePoint> points, Seq<int> selected) =>
            Curve.With(points).ToOption().Map(curve => this with { Curve = curve, State = State.Holding(curve) with { Selected = selected } });

        public Option<Plotted> Inserting(double input) =>
            Wrapped(Curve.Input, input) switch {
                var x when Curve.Points.Exists(point => double.Abs(point.X - x) < StepX) => None,
                var x => Curve.Points.Filter(point => point.X < x).Count switch {
                    var index => Rewritten(
                        Curve.Points.Take(index).Add(new CurvePoint(x, double.Clamp(Curve.Fit()(x), Curve.Output.Span.Low, Curve.Output.Span.High))).Concat(Curve.Points.Skip(index)),
                        Seq(index)),
                },
            };

        public Option<Plotted> Moving(Seq<int> moving, double dx, double dy) =>
            moving.Map(index => Room(moving, index)).Fold(
                    (Left: double.NegativeInfinity, Right: double.PositiveInfinity, Low: double.NegativeInfinity, High: double.PositiveInfinity),
                    static (held, room) => (double.Max(held.Left, room.Left), double.Min(held.Right, room.Right), double.Max(held.Low, room.Low), double.Min(held.High, room.High))) switch {
                var room => (X: double.Max(room.Left, double.Min(room.Right, dx)), Y: double.Max(room.Low, double.Min(room.High, dy))) switch {
                    var travel => Rewritten(
                        Curve.Points.Map((point, index) => moving.Exists(held => held == index) ? new CurvePoint(point.X + travel.X, point.Y + travel.Y) : point),
                        State.Selected),
                },
            };

        public (double X, double Y) Snapped((float First, float Second) at) =>
            ((at.First + at.Second) / 2f) switch {
                var diagonal when Curve.Ascending && PointF.Distance(Placed.Point(at.First, at.Second), Placed.Point(diagonal, diagonal)) <= Plots.Reach => (diagonal, diagonal),
                _ => (Placed.X.Snap(at.First, 0, LeastX), Placed.Y.Snap(at.Second, 0, LeastY)),
            };

        public Option<Transition<CurveEditorState, PlottedCurve>> Nudged(Orientation along, int steps, Keys modifiers) =>
            from anchor in Anchor
            let horizontal = along == Orientation.Horizontal
            let value = (float)(horizontal ? anchor.X : anchor.Y)
            let travel = Snaps(modifiers)
                ? (horizontal ? Placed.X.Snap(value, steps, LeastX) : Placed.Y.Snap(value, steps, LeastY)) - value
                : steps * (horizontal ? StepX : StepY) * Scale(modifiers)
            from moved in Moving(State.Selected, horizontal ? travel : 0d, horizontal ? 0d : travel)
            select moved.Raised(static curve => new Edit<PlottedCurve>.Step(curve));

        public Option<Transition<CurveEditorState, PlottedCurve>> Deleted() =>
            State.Selected.Filter(index => Curve.Input.Periodic || (index > 0 && index < Curve.Points.Count - 1)).Take(Curve.Points.Count - 2) switch {
                var removed when removed.IsEmpty => None,
                var removed => Rewritten(
                        Curve.Points.Map(static (point, index) => (Point: point, Index: index)).Filter(held => !removed.Exists(gone => gone == held.Index)).Map(static held => held.Point),
                        Seq<int>())
                    .Map(static held => held.Raised(static curve => new Edit<PlottedCurve>.Commit(curve))),
            };

        private (double Left, double Right, double Low, double High) Room(Seq<int> moving, int index) =>
            Curve.Points.Map(static (point, at) => (Point: point, At: at)).Filter(held => !moving.Exists(gone => gone == held.At)) switch {
                var free => (Before: free.Filter(held => held.At < index).Last.Map(static held => held.Point), After: free.Find(held => held.At > index).Map(static held => held.Point), Point: Curve.Points[index]) switch {
                    var near => (
                        near.Before.Map(neighbour => neighbour.X + StepX).IfNone(Curve.Input.Span.Low) - near.Point.X,
                        near.After.Map(neighbour => neighbour.X - StepX).IfNone(Curve.Input.Periodic ? double.BitDecrement(Curve.Input.Span.High) : Curve.Input.Span.High) - near.Point.X,
                        near.Before.Filter(_ => Curve.Ascending).Map(static neighbour => neighbour.Y).IfNone(Curve.Output.Span.Low) - near.Point.Y,
                        near.After.Filter(_ => Curve.Ascending).Map(static neighbour => neighbour.Y).IfNone(Curve.Output.Span.High) - near.Point.Y),
                },
            };

        private static double Wrapped(CurveAxis axis, double value) =>
            axis.Span switch {
                var (low, high) when axis.Periodic => low + ((((value - low) % (high - low)) + (high - low)) % (high - low)),
                var (low, high) => double.Clamp(value, low, high),
            };
    }

    // --- [PAINT]
    private const float CueDiameter = 8f;
    private const float HaloDiameter = 24f;

    private static readonly MarkStyle Halo = new MarkStyle.Fill(PaintSlot.ControlText, 0.1f);
    private static readonly MarkStyle Focus = new MarkStyle.Stroke(PaintSlot.Highlight, 1f, 1f);

    protected override IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, CurveEditorState state, Seq<PlotMark<CurvePart>> marks, Interaction<CurvePart> interaction) =>
        from well in Plots.Well(canvas, bounds)
        from ramp in Plotting(state, bounds.Size).Filter(static plotted => plotted.Curve.Input == CurveAxis.Hue).Match(
            Some: plotted => Plots.Strip(canvas, plotted.Placed, static value => Plots.Encoded(Gamut.StandardRgb, Plots.HueRamp(value).AsVector3())),
            None: static () => IO.pure(unit))
        from painted in Plots.Paint(canvas, marks)
        from labels in Labelled(canvas, state, bounds.Size)
        from cue in Plots.Paint(canvas, Cue(state, bounds.Size, interaction))
        from edge in Plots.Paint(canvas, canvas.Focused ? Seq(new PlotMark<CurvePart>(new MarkShape.Band(bounds), Focus, None)) : Seq<PlotMark<CurvePart>>())
        select unit;

    private IO<Unit> Labelled(PlotCanvas canvas, CurveEditorState state, SizeF size) =>
        state.Switch((Canvas: canvas, Size: size, Text: text),
            editing: static (held, editing) => Plotted.Of(editing, held.Size).Match(
                Some: plotted =>
                    from bottom in Plots.Labels(held.Canvas, plotted.Placed, LabelSide.Bottom, Ticks(plotted.Placed.X.Grid(plotted.LeastX), held.Text.Axis(plotted.Curve.Input)))
                    from left in Plots.Labels(held.Canvas, plotted.Placed, LabelSide.Left, Ticks(plotted.Placed.Y.Grid(plotted.LeastY), held.Text.Axis(plotted.Curve.Output)))
                    select unit,
                None: static () => IO.pure(unit)),
            graph: static (held, graph) => GraphPlane(graph, held.Size) switch {
                var plane =>
                    from bottom in Plots.Labels(held.Canvas, plane, LabelSide.Bottom, Ticks(GraphGrid(plane), held.Text.Axis(CurveAxis.Stops)))
                    from left in Plots.Labels(held.Canvas, plane, LabelSide.Left, graph.Guides)
                    select unit,
            });

    private static Seq<AxisLabel> Ticks(Seq<float> values, NumberText<double> shown) =>
        values.Map(value => new AxisLabel(value, shown.Shown(Some((double)value))));

    private static Seq<PlotMark<CurvePart>> Cue(CurveEditorState state, SizeF size, Interaction<CurvePart> interaction) =>
        from plotted in Plotting(state, size).ToSeq()
        from index in (interaction.Pressed | interaction.Hovered).Bind(Indexed).ToSeq()
        from point in plotted.Curve.Points.At(index).ToSeq()
        let at = plotted.Traced.Point((float)point.X, (float)point.Y)
        from mark in Seq(
            new PlotMark<CurvePart>(new MarkShape.Disc(at, HaloDiameter), Halo, None),
            new PlotMark<CurvePart>(new MarkShape.Disc(at, CueDiameter), new MarkStyle.Fill(plotted.Curve.Paint, 1f), None))
        select mark;

    // --- [INPUT]
    protected override Option<Transition<CurveEditorState, PlottedCurve>> Pressed(CurveEditorState state, SizeF size, float scale, Option<CurvePart> part, MouseEventArgs e) =>
        from editing in Editing(state)
        from hit in part
        where e.Buttons == MouseButtons.Primary
        from next in hit.Switch(
            (Editing: editing, Size: size, At: e.Location, Toggles: (e.Modifiers & Application.Instance.CommonModifier) != Keys.None),
            ground: static (held, _) => Some(new Transition<CurveEditorState, PlottedCurve>(held.Editing with { Selected = Seq<int>() }, None)),
            fitted: static (held, _) =>
                from plotted in Plotted.Of(held.Editing, held.Size)
                from inserted in plotted.Inserting(plotted.Placed.Value(held.At).First)
                select inserted.Raised(static curve => new Edit<PlottedCurve>.Preview(curve)),
            knot: static (held, knot) => Some(new Transition<CurveEditorState, PlottedCurve>(held.Editing with { Selected = Chosen(held.Editing.Selected, knot.Index, held.Toggles) }, None)))
        select next;

    protected override Transition<CurveEditorState, PlottedCurve> Dragged(
        CurveEditorState state, SizeF size, float scale, Option<CurvePart> part, PointF origin, PointF previous, MouseEventArgs e) =>
        (from plotted in Plotting(state, size)
         from held in plotted.State.Selected.IsEmpty && part.Exists(static hit => hit.Map(ground: true, fitted: false, knot: false))
             ? plotted.Inserting(plotted.Placed.Value(origin).First)
             : Some(plotted)
         from anchor in held.Anchor
         let travel = Snaps(e.Modifiers)
             ? held.Snapped(held.Placed.Value(e.Location)) switch {
                 var (x, y) => (X: x - anchor.X, Y: y - anchor.Y),
             }
             : (held.Placed.Value(e.Location), held.Placed.Value(previous)) switch {
                 var ((x, y), (lastX, lastY)) => (X: (double)((x - lastX) * Scale(e.Modifiers)), Y: (double)((y - lastY) * Scale(e.Modifiers))),
             }
         from moved in held.Moving(held.State.Selected, travel.X, travel.Y)
         select moved.Raised(static curve => new Edit<PlottedCurve>.Preview(curve)))
        .IfNone(new Transition<CurveEditorState, PlottedCurve>(state, None));

    protected override Option<Transition<CurveEditorState, PlottedCurve>> DoubleClicked(CurveEditorState state, SizeF size, float scale, Option<CurvePart> part, MouseEventArgs e) =>
        from editing in Editing(state)
        from hit in part
        where hit.Map(ground: true, fitted: false, knot: false)
        select Reset(editing);

    protected override Option<Transition<CurveEditorState, PlottedCurve>> KeyPressed(CurveEditorState state, SizeF size, float scale, Option<CurvePart> part, KeyEventArgs e) =>
        from plotted in Plotting(state, size)
        from next in e.Key switch {
            Keys.Left => plotted.Nudged(Orientation.Horizontal, -1, e.Modifiers),
            Keys.Right => plotted.Nudged(Orientation.Horizontal, 1, e.Modifiers),
            Keys.Down => plotted.Nudged(Orientation.Vertical, -1, e.Modifiers),
            Keys.Up => plotted.Nudged(Orientation.Vertical, 1, e.Modifiers),
            Keys.Delete or Keys.Backspace => plotted.Deleted(),
            _ => None,
        }
        select next;

    protected override Option<Transition<CurveEditorState, PlottedCurve>> Stepped(CurveEditorState state, SizeF size, float scale, CurvePart part, int steps) =>
        from plotted in Plotting(state, size)
        from index in Indexed(part)
        from moved in plotted.Moving(Seq(index), 0d, steps * plotted.StepY)
        select moved.Raised(static curve => new Edit<PlottedCurve>.Step(curve));

    private static Seq<int> Chosen(Seq<int> selected, int index, bool toggles) =>
        (selected.Exists(held => held == index), toggles) switch {
            (true, true) => selected.Filter(held => held != index),
            (false, true) => selected.Add(index),
            (true, false) => selected.Filter(held => held != index).Add(index),
            (false, false) => Seq(index),
        };
}
