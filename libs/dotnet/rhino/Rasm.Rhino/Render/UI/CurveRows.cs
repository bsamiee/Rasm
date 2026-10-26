using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.UI.Controls;
using UnitsNet;

namespace Rasm.Rhino.Render.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveRows {
    // --- [POINT_CURVES]
    public static ControlRow Points(RowSource<PointCurves> source, string caption, string help, Option<StageInput> stage, RowRules rules) =>
        Plotted(source, IterableNE.create(TraceChannel.Items[0], TraceChannel.Items.Skip(1)), static trace => new CurveLane<PointCurves>(
                new RowField<PointCurves>(trace.Curve.Parameter), trace,
                held => new PlottedCurve.Channel(trace, trace.Curve.Curve.Get(held)),
                (plotted, held) => plotted is PlottedCurve.Channel channel ? trace.Curve.Curve.Set(channel.Value, held) : held,
                (held, mean, context) => held.Read(trace.Curve, mean, context.Working)),
            caption, help, stage, rules);

    // --- [HUE_CURVES]
    public static ControlRow Hues(RowSource<HueCurves> source, string caption, string help, Option<StageInput> stage, RowRules rules) =>
        Plotted(source, IterableNE.create(HueCurveKind.Items[0], HueCurveKind.Items.Skip(1)), static kind => kind.Curve.Accept(new HueLane(new RowField<HueCurves>(kind))), caption, help, stage, rules);

    private sealed class HueLane(RowField<HueCurves> field) : IHueCurveVisitor<CurveLane<HueCurves>> {
        public CurveLane<HueCurves> Curve<TKind>(Lens<HueCurves, HueCurve<TKind>> lens) where TKind : IHueCurveKind =>
            new(field, TKind.Kind,
                held => new PlottedCurve.Hue<TKind>(lens.Get(held)),
                (plotted, held) => plotted is PlottedCurve.Hue<TKind> hue ? lens.Set(hue.Value, held) : held,
                static (held, mean, _) => held.Read(TKind.Kind, Hsy.From(mean)));
    }

    // --- [CURVE_BLOCKS]
    private static readonly Func<(PixelFrame Frame, Gamut Working), Traces> Histograms =
        memo(static ((PixelFrame Frame, Gamut Working) key) => new ScopeRequest.Histogram(key.Working, None).Read(key.Frame).Bins);

    private static readonly CurveSide Input = new(
        "Input", static curve => curve.Input, static anchor => anchor.Input,
        Lens<CurveAnchor, double>.New(static anchor => anchor.Point.X, static x => anchor => anchor with { Point = anchor.Point with { X = x } }));

    private static readonly CurveSide Output = new(
        "Output", static curve => curve.Output, static anchor => anchor.Output,
        Lens<CurveAnchor, double>.New(static anchor => anchor.Point.Y, static y => anchor => anchor with { Point = anchor.Point with { Y = y } }));

    private static ControlRow Plotted<TItem, TRecord>(
        RowSource<TRecord> source, IterableNE<TItem> items, Func<TItem, CurveLane<TRecord>> lane, string caption, string help, Option<StageInput> stage, RowRules rules)
        where TRecord : notnull =>
        items.Map(lane) switch {
            var lanes => ControlRow.Of(source, lanes.Map(static held => held.Field), caption, help, RowShape.Block, (scope, retain) => Block(source, lanes, caption, stage, scope, retain), rules),
        };

    private static IO<RowCells> Block<TRecord>(RowSource<TRecord> source, IterableNE<CurveLane<TRecord>> lanes, string caption, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from keyed in IO.pure(toHashMap(lanes.Map(static lane => (lane.Key, lane))))
        let named = fun((CurveKey key) => RowText.Localize(keyed[key].Field.Caption, scope.Sink).Local)
        from anchor in atomIO(Option<CurveAnchor>.None)
        from editor in DisposalOps.Acquire(IO.lift(() => new CurveEditor(
            scope.Sink,
            new CurveEditorState.Editing(lanes.Head.Key, HashMap<CurveKey, PlottedCurve>(), Seq<int>(), ShowsHistogram: true, None),
            new CurveText(RowText.Localize(caption, scope.Sink).Local, named, Shown),
            stage.Map(static input => input.Read.Map(frame => frame.Map(held => Histograms((held, input.Context.Working))))))), retain)
        let site = new CallbackSite(scope.Sink, typeof(CurveEditor), nameof(CurveEditor.Edited))
        from strip in ChoiceRows.Strip(toSeq(lanes).Map(lane => new ChoiceItem<CurveKey>(lane.Key, named(lane.Key), None)), editor.Choose, scope.Sink, retain)
        from chosen in strip.Show(Some(lanes.Head.Key))
        from bound in RowEdit.Bind(
                source, lanes.Map(static lane => lane.Field), h => editor.Edited += h, h => editor.Edited -= h,
                edit => Callbacks.Handler<EditEventArgs<PlottedCurve>>(args => edit.Take(args.Edit, (curve, held) => keyed[curve.Key].Write(curve, held)).Map(unit), site),
                held => held.Match(
                    Some: record => toSeq(lanes).TraverseM(lane => editor.Receive(Some(lane.Curve(record)))).As().Map(unit),
                    None: () => editor.Receive(None)),
                scope, site, retain)
        from pick in stage.Traverse(input => PreviewRows.Pick(input, "Pick point from frame", mean => scope.Read(source).Bind(record => editor.Pick(key => keyed[key].Input(record, mean, input.Context))), scope, retain)).As()
        from input in Coordinate(Input, lanes, source.Default, anchor.ValueIO, editor, pick.Map(static button => (Control)button), scope.Sink, retain)
        from output in Coordinate(Output, lanes, source.Default, anchor.ValueIO, editor, None, scope.Sink, retain)
        from anchored in Subscriptions.Attach(
                h => editor.Anchored += h, h => editor.Anchored -= h,
                Callbacks.Handler<AnchorShownEventArgs>(
                    shown => anchor.SwapIO(_ => shown.Anchor).Bind(_ => input.Show(shown.Anchor)).Bind(_ => output.Show(shown.Anchor)), site with { Member = nameof(CurveEditor.Anchored) }), retain)
        from stack in Containers.Frame(() => new StackLayout(new StackLayoutItem(strip.Control, HorizontalAlignment.Left), new StackLayoutItem(editor, HorizontalAlignment.Stretch)), retain)
        from arranged in IO.lift(() => {
            stack.Orientation = Orientation.Vertical;
            stack.Spacing = RhinoLayout.StackedSpacing(RhinoLayout.SpacingType.Table);
            stack.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        })
        select new RowCells(
            stack, None, None, None, [], Seq(input.Line, output.Line), new RowHelp(None, None), bound.Shown, Some<RowEdit>(bound),
            Some(ChoiceRows.Context(editor, Seq(editor.Commands.Map(static command => (MenuEntry)command)), [], scope.Sink))) { Plot = Some<ComponentControl>(editor) };

    private static IO<(RowLine Line, Func<Option<CurveAnchor>, IO<Unit>> Show)> Coordinate<TRecord>(
        CurveSide side, IterableNE<CurveLane<TRecord>> lanes, TRecord record, IO<Option<CurveAnchor>> anchored, CurveEditor editor, Option<Control> pick, IPlugInSink sink, Action<IDisposable> retain)
        where TRecord : notnull =>
        from entries in toSeq(lanes).Map(lane => side.Plotted(lane.Curve(record))).Distinct()
            .TraverseM(axis =>
                from text in IO.pure(Shown(axis))
                from field in DisposalOps.Acquire(IO.lift(() => new NumberField<CurveCoordinate, double>(text, text.Domain.Create, text.Domain.Key, text.Domain.Bounds, sink) { Enabled = false }), retain)
                select (axis, field)).As()
        let fields = toHashMap(entries)
        from panel in Containers.Frame(static () => new Panel(), retain)
        from placed in IO.lift(() => panel.Content = fields[side.Plotted(lanes.Head.Curve(record))])
        let site = new CallbackSite(sink, typeof(NumberField<CurveCoordinate, double>), nameof(NumberField<,>.Edited))
        let handler = Callbacks.Handler<EditEventArgs<CurveCoordinate>>(args => Entered(args.Edit, side.Coordinate, anchored, editor), site)
        from held in toSeq(fields.Values).TraverseM(field => Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h, handler, retain)).As()
        let width = toSeq(fields.Values).Fold(0f, static (widest, field) => float.Max(widest, field.Fit.Width))
        select (
            new RowLine(Some(side.Caption), panel, pick, Some(new FieldFit(panel, width))),
            (Func<Option<CurveAnchor>, IO<Unit>>)(value => value.Match(
                Some: point => fields[side.Anchored(point)] switch {
                    var field => IO.lift(() => {
                        panel.Content = field;
                        field.Enabled = true;
                    }).Bind(_ => field.Receive(Some(CurveCoordinate.Create(side.Coordinate.Get(point))))),
                },
                None: () => toSeq(fields.Values).TraverseM(static field => IO.lift(() => field.Enabled = false).Bind(_ => field.Receive(None))).As().Map(unit))));

    private static IO<Unit> Entered(Edit<CurveCoordinate> edit, Lens<CurveAnchor, double> coordinate, IO<Option<CurveAnchor>> anchored, CurveEditor editor) =>
        from held in anchored
        from entered in (
            from value in edit.Settled
            from point in held
            select editor.Enter(coordinate.Set(value, point))).IfNone(IO.pure(unit))
        select entered;

    private static ScalarText<CurveCoordinate, double> Shown(CurveAxis axis) => NumberText.Scalar(NumericDomain.Bounded<CurveCoordinate, double>(axis.Presentation), "");

    private sealed record CurveLane<TRecord>(
        RowField<TRecord> Field, CurveKey Key, Func<TRecord, PlottedCurve> Curve, Func<PlottedCurve, TRecord, TRecord> Write, Func<TRecord, Vector4, PassContext, double> Input)
        where TRecord : notnull;

    private sealed record CurveSide(string Caption, Func<PlottedCurve, CurveAxis> Plotted, Func<CurveAnchor, CurveAxis> Anchored, Lens<CurveAnchor, double> Coordinate);

    // --- [FUNCTION_GRAPHS]
    private static readonly AxisRange Window =
        (LogSpace.AgXLog.Allocation.Inverse(Vector3.Zero), LogSpace.AgXLog.Allocation.Inverse(Vector3.One)) switch {
            var (low, high) => AxisRange.Create(float.Log2(low.X / Exposure.MiddleGray), float.Log2(high.X / Exposure.MiddleGray)),
        };

    public static ControlRow Graph<TStage>(RowSource<TStage> source, string caption, string help, StageInput stage, RowRules rules) where TStage : IPixelStage<TStage> =>
        new ControlRow.Readout(RowShape.Block, (scope, retain) => Graphed(source, caption, stage, scope, retain), caption, help, rules);

    private static IO<RowCells> Graphed<TStage>(RowSource<TStage> source, string caption, StageInput stage, RowScope scope, Action<IDisposable> retain) where TStage : IPixelStage<TStage> =>
        from text in IO.pure(NumberText.Scalar(NumericDomain.Bounded<AxisFraction, float>(AxisFraction.Presentation), ""))
        let guides = Guides(stage, text)
        let output = fun((float code) => text.Shown(Some(code)))
        from editor in DisposalOps.Acquire(IO.lift(() => new CurveEditor(
            scope.Sink, new CurveEditorState.Graph(Window, None, guides, output), new CurveText(RowText.Localize(caption, scope.Sink).Local, static _ => "", Shown), None)), retain)
        let plot = scope.Read(source).Bind(state => editor.Plot(Window, Response(TStage.Pass(state, stage.Context)), guides, output))
        from heard in scope.Listen(keys => when(keys.Exists(key => string.Equals(key.Owner, source.Owner, StringComparison.Ordinal)), plot).As(), retain)
        select new RowCells(editor, None, None, None, [], [], new RowHelp(None, None), plot, None, None);

    private static Option<Func<float, float>> Response(Option<PixelPass> pass) =>
        pass.Match(
            Some: static held => held.Switch(
                color: static color => Some<Func<float, float>>(stops => Sampled(color.Row, Linear(stops))),
                pointwise: static _ => None,
                frame: static _ => None),
            None: static () => Some(Linear));

    private static Seq<AxisLabel> Guides(StageInput stage, NumberText<float> text) =>
        toSeq(Range(-4, 7)).Map(static stop => float.ScaleB(Exposure.MiddleGray, stop)).Add(1f)
            .Map(linear => new AxisLabel(Sampled(stage.Context.Display.Encoding.Encode, linear), text.Shown(Some(linear))));

    private static float Sampled(Action<Span<Vector4>> kernel, float linear) {
        Span<Vector4> pixel = [new Vector4(linear, linear, linear, 1f)];
        kernel(pixel);
        return (pixel[0].X + pixel[0].Y + pixel[0].Z) / 3f;
    }

    private static float Linear(float stops) => float.Exp2(stops) * Exposure.MiddleGray;

    // --- [LEVELS]
    public static ControlRow LevelPoints(RowSource<Levels> source, string caption, string help, Option<StageInput> stage, RowRules rules) =>
        (Black: new RowField<Levels>(LevelsParameter.Black), White: new RowField<Levels>(LevelsParameter.White)) switch {
            var points => ControlRow.Of(
                source, IterableNE.create(points.Black, points.White), caption, help, RowShape.Block, (scope, retain) => Leveled(source, points, caption, stage, scope, retain), rules),
        };

    private static IO<RowCells> Leveled(RowSource<Levels> source, (RowField<Levels> Black, RowField<Levels> White) points, string caption, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain) =>
        from view in DisposalOps.Acquire(IO.lift(() => new ScopeView(
            scope.Sink, RowText.Localize(caption, scope.Sink).Local, new ScopeEdit.Points(source.Default),
            stage.Match(Some: static input => input.Read.Map(frame => frame.Map(held => new ScopeFrames(held, input.Context.Working, input.Context.Signal, None, None, None))),
                None: static () => IO.pure(Option<ScopeFrames>.None)))), retain)
        let site = new CallbackSite(scope.Sink, typeof(ScopeView), nameof(ScopeView.Edited))
        from bound in RowEdit.Bind(
            source, IterableNE.create(points.Black, points.White), h => view.Edited += h, h => view.Edited -= h,
            edit => Callbacks.Handler<EditEventArgs<ScopeEdit>>(args => edit.Take(args.Edit, Into).Map(static _ => unit), site),
            held => view.Receive(held.Map(static levels => (ScopeEdit)new ScopeEdit.Points(levels))),
            scope, site, retain)
        from black in
            NumericRows.Line(
                source, points.Black.Caption, LevelsParameter.BlackPoint.Lens, LevelsParameter.BlackPoint.Presentation, bound, None, scope.Sink, retain)
        from white in
            NumericRows.Line(
                source, points.White.Caption, LevelsParameter.WhitePoint, Exposure.Presentation, bound, None, scope.Sink, retain)
        from command in IO.lift(static () => new Command())
        from named in IO.lift(() => {
            string shown = RowText.Localize("Auto").Local;
            command.MenuText = shown;
            command.ToolBarText = shown;
            command.ToolTip = RowText.Localize("Set black and white points from frame").Local;
        })
        from executed in Subscriptions.Attach(
                h => command.Executed += h, h => command.Executed -= h,
                Callbacks.Handler<EventArgs>(
                    _ => stage.Traverse(input => input.Read.Bind(frame => frame.Match(Some: held => bound.Commit(Levels.Auto(held, input.Context.Working)).Map(unit), None: static () => IO.pure(unit)))).As().Map(unit),
                    site with { Member = nameof(Command.Executed) }), retain)
        from auto in ButtonRows.Push(command, None, scope.Sink, retain)
        let show =
            from available in stage.Traverse(static input => input.Read.Map(static frame => frame.IsSome)).As()
            from enabled in IO.lift(() => command.Enabled = available.IfNone(noneValue: false))
            from editing in bound.Editing
            from shown in when(!editing,
                from bracketed in bound.Shown
                from record in scope.Read(source)
                from lower in black.Show(Some(record))
                from upper in white.Show(Some(record))
                select unit).As()
            select unit
        select new RowCells(
            view, None, None, None, [], Seq(black.Line, white.Line, new RowLine(None, auto, None, None)), new RowHelp(None, None), show,
            Some<RowEdit>(bound),
            Some(ChoiceRows.Context(view, Seq(Seq<MenuEntry>(command)), [], scope.Sink)));

    private static Fin<Levels> Into(ScopeEdit edit, Levels held) =>
        edit.Switch(held, chart: static (record, _) => record, points: static (_, points) => points.Value);
}
