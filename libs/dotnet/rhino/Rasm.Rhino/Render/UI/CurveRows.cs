using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rasm.UI.Styling;
using Rhino.UI.Controls;
using UnitsNet;

namespace Rasm.Rhino.Render.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveRows {
    // --- [POINT_CURVES]
    public static ControlRow Points(RowSource<PointCurves> source, string caption, string help, Option<StageInput> stage, RowRules rules) =>
        Plotted(source, IterableNE.create(TracePlane.Items[0], TracePlane.Items.Skip(1)), static trace => new CurveLane<TracePlane, PointCurves>(
                trace, new RowField<PointCurves>(trace.Curve.Parameter), (CurveAxis.Stops, CurveAxis.Stops), Histogrammed: true,
                held => trace.Curve.Curve.Get(held).Points,
                (held, points) => ValidationResults.ToFin(PointCurve.Validate(points, out PointCurve? curve), curve).Map(next => trace.Curve.Curve.Set(next, held)),
                held => trace.Curve.Curve.Get(held).Fit(),
                held => trace.Curve.Curve.Set(PointCurve.Identity, held),
                (held, mean, context) => held.Read(trace.Curve, mean, context.Working),
                static input => [input],
                (row, held, shown) => Grading.Curve(
                    trace.Curve.Curve.Get(held), trace, shown.Traced, toSeq(TracePlane.Items).Filter(other => other != trace).Map(other => other.Curve.Curve.Get(held)),
                    shown.Picked, Picking(row), row.Edited<PointCurve>((curve, record) => trace.Curve.Curve.Set(curve, record)))),
            caption, help, stage, rules);

    // --- [HUE_CURVES]
    private const double AnchorShare = 12d;

    public static ControlRow Hues(RowSource<HueCurves> source, string caption, string help, Option<StageInput> stage, RowRules rules) =>
        Plotted(source, IterableNE.create(HueCurveKind.Items[0], HueCurveKind.Items.Skip(1)), static kind => kind.Curve.Accept(new HueLane(kind)), caption, help, stage, rules);

    private sealed class HueLane(HueCurveKind kind) : IHueCurveVisitor<CurveLane<HueCurveKind, HueCurves>> {
        public CurveLane<HueCurveKind, HueCurves> Curve<TKind>(Lens<HueCurves, HueCurve<TKind>> lens) where TKind : IHueCurveKind =>
            new(kind, new RowField<HueCurves>(kind), (TKind.Kind.Input.Axis, TKind.Kind.Output), TKind.Kind.Input == InputAxis.Stops,
                held => lens.Get(held).Points,
                (held, points) => ValidationResults.ToFin(HueCurve<TKind>.Validate(points, out HueCurve<TKind>? curve), curve).Map(next => lens.Set(next, held)),
                held => lens.Get(held).Fit(),
                held => lens.Set(HueCurves.Neutral<TKind>(), held),
                static (held, mean, _) => held.Read(TKind.Kind, Hsy.From(mean)),
                static input => ((TKind.Kind.Input.Axis.Span.High - TKind.Kind.Input.Axis.Span.Low) / AnchorShare) switch {
                    var offset => Seq(input - offset, input + offset, input),
                },
                (row, held, shown) => Grading.Curve(lens.Get(held), shown.Traced, shown.Picked, Picking(row), row.Edited<HueCurve<TKind>>((curve, record) => lens.Set(curve, record))));
    }

    // --- [CURVE_BLOCKS]
    private static readonly Selection Unpicked = new Selection.Many([], None, None);

    private static readonly Func<(PixelFrame Frame, Gamut Working), Traces> Histograms =
        memo(static ((PixelFrame Frame, Gamut Working) key) => new ScopeRequest.Histogram(key.Working, None, None).Read(key.Frame).Bins);

    private static readonly CurveSide Input = new(
        "Input", static axes => axes.Input, Lens<CurvePoint, double>.New(static point => point.X, static x => point => point with { X = x }));

    private static readonly CurveSide Output = new(
        "Output", static axes => axes.Output, Lens<CurvePoint, double>.New(static point => point.Y, static y => point => point with { Y = y }));

    private static ControlRow Plotted<TItem, TRecord>(
        RowSource<TRecord> source, IterableNE<TItem> items, Func<TItem, CurveLane<TItem, TRecord>> lane, string caption, string help, Option<StageInput> stage, RowRules rules)
        where TItem : notnull
        where TRecord : notnull =>
        items.Map(lane) switch {
            var lanes => ControlRow.Of(source, lanes.Map(static held => held.Field), caption, help, RowShape.Block, (scope, retain) => Block(source, lanes, caption, stage, scope, retain), rules),
        };

    private static IO<RowCells> Block<TItem, TRecord>(
        RowSource<TRecord> source, IterableNE<CurveLane<TItem, TRecord>> lanes, string caption, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TItem : notnull
        where TRecord : notnull =>
        from keyed in IO.pure(toHashMap(lanes.Map(static lane => (lane.Item, lane))))
        let named = fun((TItem item) => RowText.Localize(keyed[item].Field.Caption, scope.Sink).Local)
        let shown = RowText.Localize(caption, scope.Sink).Local
        let site = new CallbackSite(scope.Sink, typeof(CurveRows), nameof(Block))
        from input in Coordinate(Input, lanes, scope.Sink, retain)
        from output in Coordinate(Output, lanes, scope.Sink, retain)
        from scened in SceneRow<TRecord, CurveShown<TItem>>.Bind(
            source, lanes.Map(static lane => lane.Field), new CurveShown<TItem>(lanes.Head.Item, Unpicked, Histogram: true, None),
            (row, record, held) => keyed[held.Active].Drawn(row, record, held) switch {
                var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = named(held.Active) }) },
            },
            (_, record, held) => Anchor(keyed[held.Active], record, held.Picked) switch {
                var anchor => input.Show(anchor).Bind(_ => output.Show(anchor)),
            },
            scope, site, retain)
        let row = scened.Row
        from entered in Callbacks.Each(Seq((Side: Input, input.Fields), (Side: Output, output.Fields)).Bind(side => side.Fields.Map(field =>
            Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h,
                Callbacks.Handler<Rhino.UI.Drawing.EditEventArgs<CurveCoordinate>>(args => Entered(args.Edit, side.Side, row, keyed),
                    site with { Member = nameof(NumberField<,>.Edited) }), retain))))
        from strip in ChoiceRows.Strip(toSeq(lanes).Map(lane => new ChoiceItem<TItem>(lane.Item, named(lane.Item), None)),
            item => row.Change(held => held with { Active = item, Picked = Unpicked }), scope.Sink, retain)
        from chosen in strip.Show(Some(lanes.Head.Item))
        from pick in stage.Traverse(held => PreviewRows.Pick(held, "Pick point from frame", mean => Picked(row, keyed, held.Context, mean), scope, retain)).As()
        from sampled in stage.Traverse(held => Rhino.UI.Drawing.FrameClocks.Sample(row.View, Traced(row, held), site with { Member = nameof(Histograms) }, retain)).As()
        from stack in Containers.Frame(() => new StackLayout(new StackLayoutItem(strip.Control, HorizontalAlignment.Left), new StackLayoutItem(row.View, HorizontalAlignment.Stretch)), retain)
        from arranged in IO.lift(() => {
            stack.Orientation = Eto.Forms.Orientation.Vertical;
            stack.Spacing = RhinoLayout.StackedSpacing(RhinoLayout.SpacingType.Table);
            stack.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        })
        select new RowCells(
            stack, None, None, None, [], Seq(input.Line with { Pick = pick }, output.Line), new RowHelp(None, None), scened.Edit.Shown, Some<RowEdit>(scened.Edit),
            Some(ChoiceRows.Context(row.View, [Commands(row, keyed)], [])));

    private static Func<Edit<Selection>, IO<Unit>> Picking<TItem, TRecord>(SceneRow<TRecord, CurveShown<TItem>> row) where TItem : notnull where TRecord : notnull =>
        edit => row.Change(held => held with { Picked = edit.Carried.IfNone(held.Picked) });

    private static Seq<Command<RowRule, Invoke>> Commands<TItem, TRecord>(SceneRow<TRecord, CurveShown<TItem>> row, HashMap<TItem, CurveLane<TItem, TRecord>> keyed)
        where TItem : notnull
        where TRecord : notnull =>
        [
            new Command<RowRule, Invoke>.Run(new CommandFace("curve-reset", RowText.Localize("Reset Curve").Local, None, None),
                new RowRule([], [], _ => row.Shown.Map(shown => shown.Exists(record => keyed[row.Held.Active] switch {
                    var lane => !lane.Points(lane.Reset(record)).Equals(lane.Points(record)),
                }))),
                new Invoke.Effect(_ => row.Edited<Unit>((_, record) => keyed[row.Held.Active].Reset(record))(new Edit<Unit>.Commit(unit))
                    .Bind(_ => row.Change(static held => held with { Picked = Unpicked })))),
            new Command<RowRule, Invoke>.Run(new CommandFace("curve-histogram", RowText.Localize("Input Histogram").Local, None, None),
                new RowRule([], [], _ => IO.lift(() => keyed[row.Held.Active].Histogrammed)),
                new Invoke.Effect(_ => row.Change(static held => held with { Histogram = !held.Histogram }))) {
                Marked = new RowRule([], [], _ => IO.lift(() => row.Held.Histogram)),
            },
        ];

    private static IO<Unit> Traced<TItem, TRecord>(SceneRow<TRecord, CurveShown<TItem>> row, StageInput stage) where TItem : notnull where TRecord : notnull =>
        from frame in stage.Read
        let traces = frame.Map(held => Histograms((held, stage.Context.Working)))
        from changed in when(!row.Held.Traces.Match(Some: held => traces.Exists(next => ReferenceEquals(held, next)), None: () => traces.IsNone),
            row.Change(held => held with { Traces = traces })).As()
        select unit;

    private static IO<Unit> Picked<TItem, TRecord>(SceneRow<TRecord, CurveShown<TItem>> row, HashMap<TItem, CurveLane<TItem, TRecord>> keyed, PassContext context, Vector4 mean)
        where TItem : notnull
        where TRecord : notnull =>
        keyed[row.Held.Active] switch {
            var lane =>
                from shown in row.Shown
                let placed = shown.Map(record => lane.Picks(lane.Input(record, mean, context)).Fold((Points: lane.Points(record), At: Option<int>.None), (held, x) =>
                    Inserted(held.Points, x, lane.Fit(record), lane.Axes).Match(Some: static inserted => (inserted.Points, Some(inserted.At)), None: () => held)))
                from written in placed.Filter(static held => held.At.IsSome).Traverse(held =>
                    from taken in row.Edited<Unit>((_, record) => lane.Rewritten(record, held.Points))(new Edit<Unit>.Commit(unit))
                    from chosen in row.Change(state => state with { Picked = held.At.Map(Lead).IfNone(Unpicked) })
                    select unit).As()
                select unit,
        };

    private static Option<(Seq<CurvePoint> Points, int At)> Inserted(Seq<CurvePoint> points, double input, Func<double, double> fit, (CurveAxis Input, CurveAxis Output) axes) =>
        (axes.Input.Span.High - axes.Input.Span.Low) switch {
            var span => (axes.Input.Periodic ? axes.Input.Span.Low + ((((input - axes.Input.Span.Low) % span) + span) % span) : double.Clamp(input, axes.Input.Span.Low, axes.Input.Span.High)) switch {
                var x when points.Exists(point => double.Abs(point.X - x) < span / RampTable.Texels) => None,
                var x => points.Filter(point => point.X < x).Count switch {
                    var at => Some((points.Take(at).Add(new CurvePoint(x, double.Clamp(fit(x), axes.Output.Span.Low, axes.Output.Span.High))).Concat(points.Skip(at)), at)),
                },
            },
        };

    private static Selection Lead(int index) => (Key)new Key.Ordinal((uint)index) switch { var key => new Selection.Many([key], Some(key), Some(key)) };

    private static Option<int> Index(Selection picked) => picked.Lead.Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None);

    private static Option<CurveAnchor> Anchor<TItem, TRecord>(CurveLane<TItem, TRecord> lane, Option<TRecord> record, Selection picked) where TItem : notnull where TRecord : notnull =>
        from held in record
        from index in Index(picked)
        from point in lane.Points(held).At(index)
        select new CurveAnchor(point, lane.Axes);

    private static IO<(RowLine Line, Func<Option<CurveAnchor>, IO<Unit>> Show, Seq<NumberField<CurveCoordinate, double>> Fields)> Coordinate<TItem, TRecord>(
        CurveSide side, IterableNE<CurveLane<TItem, TRecord>> lanes, IPlugInSink sink, Action<IDisposable> retain)
        where TItem : notnull
        where TRecord : notnull =>
        from entries in toSeq(lanes).Map(lane => side.Axis(lane.Axes)).Distinct()
            .TraverseM(axis =>
                from text in IO.pure(Shown(axis))
                from field in DisposalOps.Acquire(IO.lift(() => new NumberField<CurveCoordinate, double>(text, text.Domain.Create, text.Domain.Key, text.Domain.Bounds, sink) { Enabled = false }), retain)
                select (axis, field)).As()
        let fields = toHashMap(entries)
        from panel in Containers.Frame(static () => new Panel(), retain)
        from placed in IO.lift(() => panel.Content = fields[side.Axis(lanes.Head.Axes)])
        let width = toSeq(fields.Values).Fold(0f, static (widest, field) => float.Max(widest, field.Fit.Width))
        select (
            new RowLine(Some(side.Caption), panel, None, Some(new FieldFit(panel, width))),
            (Func<Option<CurveAnchor>, IO<Unit>>)(value => value.Match(
                Some: anchor => fields[side.Axis(anchor.Axes)] switch {
                    var field => IO.lift(() => {
                        panel.Content = field;
                        field.Enabled = true;
                    }).Bind(_ => field.Receive(Some(CurveCoordinate.Create(side.Coordinate.Get(anchor.Point))))),
                },
                None: () => toSeq(fields.Values).TraverseM(static field => IO.lift(() => field.Enabled = false).Bind(_ => field.Receive(None))).As().Map(unit))),
            toSeq(fields.Values));

    private static IO<Unit> Entered<TItem, TRecord>(
        Rhino.UI.Drawing.Edit<CurveCoordinate> edit, CurveSide side, SceneRow<TRecord, CurveShown<TItem>> row, HashMap<TItem, CurveLane<TItem, TRecord>> keyed)
        where TItem : notnull
        where TRecord : notnull =>
        (from value in edit.Settled
         from index in Index(row.Held.Picked)
         let lane = keyed[row.Held.Active]
         select row.Edited<Unit>((_, record) =>
             from point in lane.Points(record).At(index).ToFin(new Refused(nameof(Selection.Lead), Refused.Absent))
             from moved in lane.Rewritten(record, Moved(lane.Points(record), index, side.Coordinate.Set(value, point), lane.Axes))
             select moved)(new Edit<Unit>.Commit(unit))).IfNone(IO.pure(unit));

    private static Seq<CurvePoint> Moved(Seq<CurvePoint> points, int index, CurvePoint entered, (CurveAxis Input, CurveAxis Output) axes) =>
        (Before: points.At(index - 1), After: points.At(index + 1)) switch {
            var near => points.Map((point, at) => at == index
                ? new CurvePoint(
                    double.Clamp(entered.X,
                        near.Before.Filter(_ => !axes.Input.Periodic).Map(static held => double.BitIncrement(held.X)).IfNone(axes.Input.Span.Low),
                        near.After.Filter(_ => !axes.Input.Periodic).Map(static held => double.BitDecrement(held.X)).IfNone(axes.Input.Span.High)),
                    double.Clamp(entered.Y, axes.Output.Span.Low, axes.Output.Span.High))
                : point),
        };

    private static ScalarText<CurveCoordinate, double> Shown(CurveAxis axis) => NumberText.Scalar(NumericDomain.Bounded<CurveCoordinate, double>(axis.Presentation), "");

    private sealed record CurveShown<TItem>(TItem Active, Selection Picked, bool Histogram, Option<Traces> Traces) where TItem : notnull {
        public Option<Traces> Traced => Traces.Filter(_ => Histogram);
    }

    private sealed record CurveAnchor(CurvePoint Point, (CurveAxis Input, CurveAxis Output) Axes);

    private sealed record CurveLane<TItem, TRecord>(
        TItem Item, RowField<TRecord> Field, (CurveAxis Input, CurveAxis Output) Axes, bool Histogrammed,
        Func<TRecord, Seq<CurvePoint>> Points, Func<TRecord, Seq<CurvePoint>, Fin<TRecord>> Rewritten, Func<TRecord, Func<double, double>> Fit, Func<TRecord, TRecord> Reset,
        Func<TRecord, Vector4, PassContext, double> Input, Func<double, Seq<double>> Picks,
        Func<SceneRow<TRecord, CurveShown<TItem>>, TRecord, CurveShown<TItem>, Node> Drawn)
        where TItem : notnull
        where TRecord : notnull;

    private sealed record CurveSide(string Caption, Func<(CurveAxis Input, CurveAxis Output), CurveAxis> Axis, Lens<CurvePoint, double> Coordinate);

    // --- [FUNCTION_GRAPHS]
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));

    private static readonly AxisRange Window =
        (LogSpace.AgXLog.Allocation.Inverse(Vector3.Zero), LogSpace.AgXLog.Allocation.Inverse(Vector3.One)) switch {
            var (low, high) => AxisRange.Create(float.Log2(low.X / Exposure.MiddleGray), float.Log2(high.X / Exposure.MiddleGray)),
        };

    public static ControlRow Graph<TStage>(RowSource<TStage> source, string caption, string help, StageInput stage, RowRules rules) where TStage : IPixelStage<TStage> =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) => Graphed(source, caption, stage, scope, retain), caption, help, rules);

    private static IO<RowCells> Graphed<TStage>(RowSource<TStage> source, string caption, StageInput stage, RowScope scope, Action<IDisposable> retain) where TStage : IPixelStage<TStage> =>
        from text in IO.pure(NumberText.Scalar(NumericDomain.Bounded<AxisFraction, float>(AxisFraction.Presentation), ""))
        let stops = Shown(CurveAxis.Stops)
        let face = new GraphFace(
            RowText.Localize(caption, scope.Sink).Local,
            toSeq(Range((int)MathF.Ceiling(Window.From), (int)MathF.Floor(Window.To) - (int)MathF.Ceiling(Window.From) + 1)).Map(stop => ((float)stop, stops.Shown(Some((double)stop)))),
            Guides(stage, text), code => text.Shown(Some(code)))
        from state in atomIO((Transfer: Option<Func<float, float>>.None, Pointer: Option<Vector2>.None))
        from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
        let plot = scope.Read(source).Bind(record => state.SwapIO(current => current with { Transfer = Response(TStage.Pass(record, stage.Context)) })).Bind(_ => Charting(view, state, face))
        from heard in scope.Listen(keys => when(keys.Exists(key => string.Equals(key.Owner, source.Owner, StringComparison.Ordinal)), plot).As(), retain)
        select new RowCells(view, None, None, None, [], [], new RowHelp(None, None), plot, None, None);

    private static IO<Unit> Charting(SceneView view, Atom<(Option<Func<float, float>> Transfer, Option<Vector2> Pointer)> state, GraphFace face) =>
        state.ValueIO.Bind(held => view.Show(Charted(face, held.Transfer, held.Pointer,
            pointer => state.SwapIO(current => current with { Pointer = pointer }).Bind(_ => Charting(view, state, face)))));

    private static Node Charted(GraphFace face, Option<Func<float, float>> transfer, Option<Vector2> pointer, Func<Option<Vector2>, IO<Unit>> hover) =>
        new Handles<Unit>(unit, new Points<Unit>(static _ => [], static (held, _, _) => (held, []), static _ => ""), static _ => IO.pure(unit)) {
            Hover = Some(hover),
        }.Drawn(new PlotPlane.Cartesian(new Axis.Linear(Window), new Axis.Linear(AxisRange.Create(0f, 1f))),
            Seq<Node>(new Node.Mark(new Figure.Grid(Rasm.UI.Orientation.Vertical)) { Role = Role.Grid })
            + face.Guides.Map(static guide => (Node)new Node.Mark(new Figure.Rule(new Vector2(Window.From, guide.At), new Vector2(Window.To, guide.At))) { Role = Role.Grid })
            + transfer.Map(static curve => (Node)new Node.Mark(new Figure.Curve(Window, toSeq(Range(0, RampTable.Texels)).Map(sample =>
                curve(Window.From + (sample * Window.Span / (RampTable.Texels - 1)))))) { Role = ImagingRoles.Trace }).ToSeq()
            + face.Stops.Map(static stop => (Node)new Node.Pin(new Vector2(stop.At, 0f), new Node.Text([stop.Text]) { Role = Role.Label }) { Align = UnitPoint.Create(0.5f, 1f) })
            + face.Guides.Map(static guide => (Node)new Node.Pin(new Vector2(Window.From, guide.At), new Node.Text([guide.Text]) { Role = Role.Label }) { Align = UnitPoint.Create(0f, 0.5f) })
            + (from at in pointer
               from curve in transfer
               select (Node)new Node.Pin(new Vector2(at.X, curve(at.X)), new Node.Text([face.Output(curve(at.X))]) { Role = ImagingRoles.Readout }) { Align = UnitPoint.Create(0f, 1f) }).ToSeq()) with {
            Role = ImagingRoles.Well,
            Width = Grown,
            Aspect = Factor.Create(1f),
            Accessible = new Accessible { Label = face.Caption },
        };

    private static Option<Func<float, float>> Response(Option<PixelPass> pass) =>
        pass.Match(
            Some: static held => held.Switch(
                color: static color => Some<Func<float, float>>(stops => Sampled(color.Row, Linear(stops))),
                pointwise: static _ => None,
                frame: static _ => None),
            None: static () => Some(Linear));

    private static Seq<(float At, string Text)> Guides(StageInput stage, NumberText<float> text) =>
        toSeq(Range(-4, 7)).Map(static stop => float.ScaleB(Exposure.MiddleGray, stop)).Add(1f)
            .Map(linear => (Sampled(stage.Context.Display.Encoding.Encode, linear), text.Shown(Some(linear))));

    private static float Sampled(Action<Span<Vector4>> kernel, float linear) {
        Span<Vector4> pixel = [new Vector4(linear, linear, linear, 1f)];
        kernel(pixel);
        return (pixel[0].X + pixel[0].Y + pixel[0].Z) / 3f;
    }

    private static float Linear(float stops) => float.Exp2(stops) * Exposure.MiddleGray;

    private sealed record GraphFace(string Caption, Seq<(float At, string Text)> Stops, Seq<(float At, string Text)> Guides, Func<float, string> Output);

    // --- [LEVELS]
    public static ControlRow LevelPoints(RowSource<Levels> source, string caption, string help, Option<StageInput> stage, RowRules rules) =>
        (Black: new RowField<Levels>(LevelsParameter.Black), White: new RowField<Levels>(LevelsParameter.White)) switch {
            var points => ControlRow.Of(
                source, IterableNE.create(points.Black, points.White), caption, help, RowShape.Block, (scope, retain) => Leveled(source, points, caption, stage, scope, retain), rules),
        };

    private static IO<RowCells> Leveled(RowSource<Levels> source, (RowField<Levels> Black, RowField<Levels> White) points, string caption, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain) =>
        from named in IO.pure(RowText.Localize(caption, scope.Sink).Local)
        let site = new CallbackSite(scope.Sink, typeof(CurveRows), nameof(LevelPoints))
        from scened in SceneRow<Levels, ScopeShown>.Bind(
            source, IterableNE.create(points.Black, points.White), ScopeShown.Initial,
            (row, levels, shown) => Scopes.Levels(levels, shown.Traces, row.Edited<Levels>(static (next, _) => next)) switch {
                var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = named }) },
            },
            static (_, _, _) => IO.pure(unit), scope, site, retain)
        let view = scened.Row.View
        let bound = scened.Edit
        from traced in stage.Traverse(input => ChartRows.Traced(
            scened.Row, Lens<ScopeShown, ScopeShown>.New(static shown => shown, static next => _ => next),
            _ => input.Read.Map(frame => frame.Map(held => new ScopeFrames(held, new ColorEncoding(input.Context.Working, input.Context.Signal, Nits.ReferenceWhite), None, input.Context.Working, None))),
            static frames => IO.pure<ScopeRequest>(new ScopeRequest.Histogram(frames.Working, None, None)), scope, site, retain)).As()
        from black in
            NumericRows.Line(
                source, points.Black.Caption, LevelsParameter.BlackPoint.Lens, LevelsParameter.BlackPoint.Presentation, bound, None, scope.Sink, retain)
        from white in
            NumericRows.Line(
                source, points.White.Caption, LevelsParameter.WhitePoint, Exposure.Presentation, bound, None, scope.Sink, retain)
        let command = new Command<RowRule, Invoke>.Run(
            new CommandFace("levels-auto", "Auto", Some("Set black and white points from frame"), None),
            new RowRule([], [], _ => stage.Traverse(static input => input.Read.Map(static frame => frame.IsSome)).As().Map(static available => available.IfNone(noneValue: false))),
            new Invoke.Effect(_ => stage.Traverse(input => input.Read.Bind(frame => frame.Match(Some: held => bound.Commit(Levels.Auto(held, input.Context.Working)).Map(unit), None: static () => IO.pure(unit)))).As().Map(unit)))
        from auto in CommandRows.Button(command, IconSlot.PanelButton, scope, retain)
        let show =
            from enabled in auto.Refresh
            from editing in bound.Editing
            from shown in when(!editing,
                from bracketed in bound.Shown
                from record in scope.Read(source)
                from lower in black.Show(Some(record))
                from upper in white.Show(Some(record))
                select unit).As()
            select unit
        select new RowCells(
            view, None, None, None, [], Seq(black.Line, white.Line, new RowLine(None, auto.Control, None, None)), new RowHelp(None, None), show,
            Some<RowEdit>(bound),
            Some(ChoiceRows.Context(view, [[command]], [])));
}
