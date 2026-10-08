using System.Diagnostics;
using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Curves;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Imaging.Tone.Formations;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rhino.UI.Controls;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.UI.Editors;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveRows {
    // --- [POINT_CURVES]
    public static ControlRow Points(
        RowSource<PointCurves> source, Func<PointCurveParameter, RowField<PointCurves>> field, string caption, string help, StageInput stage, RowRules rules) =>
        Plotted(source, TraceChannel.Items, trace => new CurveLane<PointCurves>(
                field(trace.Curve.Parameter), trace,
                held => new PlottedCurve.Channel(trace, trace.Curve.Curve.Get(held)),
                (plotted, held) => plotted is PlottedCurve.Channel channel ? trace.Curve.Curve.Set(channel.Value, held) : held,
                (held, mean) => held.Read(trace.Curve, mean, stage.Working)),
            caption, help, stage, rules);

    // --- [HUE_CURVES]
    public static ControlRow Hues(
        RowSource<HueCurves> source, Func<HueCurveKind, RowField<HueCurves>> field, string caption, string help, StageInput stage, RowRules rules) =>
        Plotted(source, HueCurveKind.Items, kind => kind.Curve.Accept(new HueLane(field(kind))), caption, help, stage, rules);

    private sealed class HueLane(RowField<HueCurves> field) : IHueCurveVisitor<CurveLane<HueCurves>> {
        public CurveLane<HueCurves> Curve<TKind>(Lens<HueCurves, HueCurve<TKind>> lens) where TKind : IHueCurveKind =>
            new(field, TKind.Kind,
                held => new PlottedCurve.Hue<TKind>(lens.Get(held)),
                (plotted, held) => plotted is PlottedCurve.Hue<TKind> hue ? lens.Set(hue.Value, held) : held,
                static (held, mean) => held.Read(TKind.Kind, Hsy.From(mean)));
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
        RowSource<TRecord> source, IReadOnlyList<TItem> items, Func<TItem, CurveLane<TRecord>> lane, string caption, string help, StageInput stage, RowRules rules)
        where TRecord : notnull =>
        items switch {
            [var head, ..] => IterableNE.create(lane(head), toSeq(items).Tail.Map(lane).Strict()) switch {
                var lanes => ControlRow.Of(source, lanes.Map(static held => held.Field), caption, help, RowShape.Block, scope => Block(source, lanes, caption, stage, scope), rules),
            },
            [] => throw new UnreachableException(),
        };

    private static IO<RowCells> Block<TRecord>(RowSource<TRecord> source, IterableNE<CurveLane<TRecord>> lanes, string caption, StageInput stage, RowScope scope)
        where TRecord : notnull =>
        from keyed in IO.pure(toHashMap(lanes.Map(static lane => (lane.Key, lane))))
        let named = fun((CurveKey key) => Wording.English.Shown(keyed[key].Field.Caption, scope.Sink))
        from anchor in IO.lift(static () => Atom(Option<CurveAnchor>.None))
        from editor in IO.lift(() => new CurveEditor(
            scope.Sink,
            new CurveEditorState.Editing(lanes.Head.Key, HashMap<CurveKey, PlottedCurve>(), Seq<int>(), ShowsHistogram: true, None),
            new CurveText(Wording.English.Shown(caption, scope.Sink), named, Shown),
            Some(stage.Read.Map(frame => frame.Map(held => Histograms((held, stage.Working)))))))
        let site = new CallbackSite(scope.Sink, typeof(CurveEditor), nameof(CurveEditor.Edited))
        from strip in ChoiceRows.Strip(toSeq(lanes).Map(lane => new ChoiceItem<CurveKey>(lane.Key, named(lane.Key), None)), editor.Choose, scope.Sink)
        from chosen in DisposalOps.OnFailure(strip.Show(Some(lanes.Head.Key)), IO.lift(strip.Release.Dispose))
        from bound in DisposalOps.OnFailure(
            RowEdit.Bind(
                source, lanes.Map(static lane => lane.Field), h => editor.Edited += h, h => editor.Edited -= h,
                edit => Callbacks.Handler<Edit<PlottedCurve>>(change => edit.Take(change, (curve, held) => Fin.Succ(keyed[curve.Key].Write(curve, held))), site),
                held => held.Match(
                    Some: record => toSeq(lanes).TraverseM(lane => editor.Receive(Some(lane.Curve(record)))).As().Map(static _ => unit),
                    None: () => editor.Receive(None)),
                scope, site),
            IO.lift(strip.Release.Dispose))
        from pick in DisposalOps.OnFailure(
            ButtonRows.Pick(stage, "Pick a point from the frame", mean => scope.Read(source).Bind(record => editor.Pick(key => keyed[key].Input(record, mean))), scope),
            DisposalOps.Release(Seq(strip.Release, bound.Release)))
        let held = Seq(strip.Release, bound.Release, pick.Release)
        from input in DisposalOps.OnFailure(Coordinate(Input, lanes, source.Default, anchor.ValueIO, editor, Some<Control>(pick.Control), scope.Sink), DisposalOps.Release(held))
        from output in DisposalOps.OnFailure(Coordinate(Output, lanes, source.Default, anchor.ValueIO, editor, None, scope.Sink), DisposalOps.Release(held.Add(input.Release)))
        let lined = held + Seq(input.Release, output.Release)
        from anchored in DisposalOps.OnFailure(
            Subscriptions.Attach(
                h => editor.Anchored += h, h => editor.Anchored -= h,
                Callbacks.Handler<Option<CurveAnchor>>(
                    shown => anchor.SwapIO(_ => shown).Bind(_ => input.Show(shown)).Bind(_ => output.Show(shown)), site with { Member = nameof(CurveEditor.Anchored) })),
            DisposalOps.Release(lined))
        from stack in IO.lift(() => new StackLayout(new StackLayoutItem(strip.Control, HorizontalAlignment.Left), new StackLayoutItem(editor, HorizontalAlignment.Stretch)) {
            Orientation = Orientation.Vertical,
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Height,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        })
        select new RowCells(
            stack, None, None, None, [], Seq(input.Line, output.Line), new RowHelp(None, None), bound.Edit.Shown, Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(editor, Seq(editor.Commands.Map(static command => (MenuEntry)command)), [], scope.Sink)),
            DisposalOps.Composite(lined.Add(anchored), site));

    private static IO<(RowLine Line, Func<Option<CurveAnchor>, IO<Unit>> Show, IDisposable Release)> Coordinate<TRecord>(
        CurveSide side, IterableNE<CurveLane<TRecord>> lanes, TRecord record, IO<Option<CurveAnchor>> anchored, CurveEditor editor, Option<Control> pick, IPlugInSink sink)
        where TRecord : notnull =>
        from fields in IO.lift(() => toHashMap(toSeq(lanes).Map(lane => side.Plotted(lane.Curve(record))).Distinct()
            .Map(axis => (axis, Field: new NumberField<CurveCoordinate, double, InvalidGrade>(Shown(axis), sink) { Enabled = false }))))
        from panel in IO.lift(() => new Panel { Content = fields[side.Plotted(lanes.Head.Curve(record))] })
        let site = new CallbackSite(sink, typeof(NumberField<CurveCoordinate, double, InvalidGrade>), nameof(NumberField<,,>.Edited))
        let handler = Callbacks.Handler<Edit<CurveCoordinate>>(edit => Entered(edit, side.Coordinate, anchored, editor), site)
        from held in DisposalOps.AcquireAll(
            toSeq(fields.Values).Map(field => Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h, handler)), DisposalOps.Release)
        let width = fields.AsIterable().Fold(0f, static (widest, pair) => float.Max(widest, Shown(pair.Key).TextWidth(pair.Value.Font)))
        select (
            new RowLine(Some(side.Caption), panel, pick, Some(new FieldFit(panel, width + (2f * NumericRows.FieldInset)))),
            (Func<Option<CurveAnchor>, IO<Unit>>)(value => value.Match(
                Some: point => fields[side.Anchored(point)] switch {
                    var field => IO.lift(() => {
                        panel.Content = field;
                        field.Enabled = true;
                    }).Bind(_ => field.Receive(Some(CurveCoordinate.Create(side.Coordinate.Get(point))))),
                },
                None: () => toSeq(fields.Values).TraverseM(static field => IO.lift(() => { field.Enabled = false; }).Bind(_ => field.Receive(None))).As().Map(static _ => unit))),
            DisposalOps.Composite(held, site));

    private static IO<Unit> Entered(Edit<CurveCoordinate> edit, Lens<CurveAnchor, double> coordinate, IO<Option<CurveAnchor>> anchored, CurveEditor editor) =>
        from held in anchored
        from entered in (
            from value in edit.Switch(preview: static _ => None, step: static step => Some(step.Value), commit: static commit => Some(commit.Value), cancel: static _ => None)
            from point in held
            select editor.Enter(coordinate.Set(value, point))).IfNone(IO.pure(unit))
        select entered;

    private static NumberText<double> Shown(CurveAxis axis) => NumberText.Scalar(axis.Presentation, "");

    private sealed record CurveLane<TRecord>(
        RowField<TRecord> Field, CurveKey Key, Func<TRecord, PlottedCurve> Curve, Func<PlottedCurve, TRecord, TRecord> Write, Func<TRecord, Vector4, double> Input)
        where TRecord : notnull;

    private sealed record CurveSide(string Caption, Func<PlottedCurve, CurveAxis> Plotted, Func<CurveAnchor, CurveAxis> Anchored, Lens<CurveAnchor, double> Coordinate);

    // --- [FUNCTION_GRAPHS]
    private static readonly Presentation<Factor, float> Percent = new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) };

    private static readonly AxisRange Window =
        (LogSpace.AgXLog.Allocation.Inverse(Vector3.Zero), LogSpace.AgXLog.Allocation.Inverse(Vector3.One)) switch {
            var (low, high) => AxisRange.Create(float.Log2(low.X / Exposure.MiddleGrey), float.Log2(high.X / Exposure.MiddleGrey)),
        };

    public static ControlRow Graph<TStage>(RowSource<TStage> source, string caption, string help, StageInput stage, RowRules rules) where TStage : IPixelStage<TStage> =>
        new ControlRow.Readout(RowShape.Block, scope => Graphed(source, caption, stage, scope), caption, help, rules);

    private static IO<RowCells> Graphed<TStage>(RowSource<TStage> source, string caption, StageInput stage, RowScope scope) where TStage : IPixelStage<TStage> =>
        from text in IO.pure(NumberText.Scalar(Percent, ""))
        let guides = Guides(stage, text)
        let output = fun((float code) => text.Shown(Some(code)))
        from editor in IO.lift(() => new CurveEditor(
            scope.Sink, new CurveEditorState.Graph(Window, None, guides, output), new CurveText(Wording.English.Shown(caption, scope.Sink), static _ => "", Shown), None))
        let context = new PassContext(
            PixelExtent.Analysis, stage.Working, stage.Display, stage.Depth, TransferCurve.Linear, Exposure.Neutral,
            None, HashMap<GuideChannel, PixelFrame>(), None, SceneLights.Empty, new Derivations())
        let plot = scope.Read(source).Bind(state => editor.Plot(Window, Response(TStage.Pass(state, context)), guides, output))
        from heard in scope.Listen(keys => when(keys.Exists(key => string.Equals(key.Owner, source.Owner, StringComparison.Ordinal)), plot).As())
        select new RowCells(editor, None, None, None, [], [], new RowHelp(None, None), plot, None, None, heard);

    private static Option<Func<float, float>> Response(Option<PixelPass> pass) =>
        pass.Match(
            Some: static held => held.Switch(
                color: static color => Some<Func<float, float>>(stops => Sampled(color.Row, Linear(stops))),
                pointwise: static _ => None,
                frame: static _ => None),
            None: static () => Some(Linear));

    private static Seq<AxisLabel> Guides(StageInput stage, NumberText<float> text) =>
        toSeq(Range(-4, 7)).Map(static stop => float.ScaleB(Exposure.MiddleGrey, stop)).Add(1f)
            .Map(linear => new AxisLabel(Sampled(stage.Display.Encoding.Encode, linear), text.Shown(Some(linear))));

    private static float Sampled(Action<Span<Vector4>> kernel, float linear) {
        Span<Vector4> pixel = [new Vector4(linear, linear, linear, 1f)];
        kernel(pixel);
        return (pixel[0].X + pixel[0].Y + pixel[0].Z) / 3f;
    }

    private static float Linear(float stops) => float.Exp2(stops) * Exposure.MiddleGrey;

    // --- [LEVELS]
    public static ControlRow LevelPoints(
        RowSource<Levels> source, Func<LevelsParameter, RowField<Levels>> field, string caption, string help, StageInput stage, RowRules rules) =>
        (Black: field(LevelsParameter.Black), White: field(LevelsParameter.White)) switch {
            var points => ControlRow.Of(
                source, IterableNE.create(points.Black, points.White), caption, help, RowShape.Block, scope => Leveled(source, points, caption, stage, scope), rules),
        };

    private static IO<RowCells> Leveled(RowSource<Levels> source, (RowField<Levels> Black, RowField<Levels> White) points, string caption, StageInput stage, RowScope scope) =>
        from view in IO.lift(() => new ScopeView(
            scope.Sink, Wording.English.Shown(caption, scope.Sink), new ScopeEdit.Points(source.Default),
            stage.Read.Map(frame => frame.Map(held => new ScopeFrames(held, stage.Working, TransferCurve.Linear, None, None)))))
        let site = new CallbackSite(scope.Sink, typeof(ScopeView), nameof(ScopeView.Edited))
        from bound in RowEdit.Bind(
            source, IterableNE.create(points.Black, points.White), h => view.Edited += h, h => view.Edited -= h,
            edit => Callbacks.Handler<Edit<ScopeEdit>>(change => edit.Take(change, Into), site),
            held => view.Receive(held.Map(static levels => (ScopeEdit)new ScopeEdit.Points(levels))),
            scope, site)
        from black in DisposalOps.OnFailure(
            NumericRows.Line<Levels, BlackLevel, float, InvalidGrade>(
                source, points.Black, LevelsParameter.BlackPoint.Lens, LevelsParameter.BlackPoint.Presentation, bound.Edit, None, scope.Sink),
            IO.lift(bound.Release.Dispose))
        from white in DisposalOps.OnFailure(
            NumericRows.Line<Levels, Exposure, float, InvalidToneValue>(
                source, points.White, LevelsParameter.WhitePoint.Lens, LevelsParameter.WhitePoint.Presentation, bound.Edit, None, scope.Sink),
            DisposalOps.Release(Seq(bound.Release, black.Release)))
        let lined = Seq(bound.Release, black.Release, white.Release)
        from command in IO.lift(static () => RowText.Localize("Auto").Local switch {
            var shown => new Command { MenuText = shown, ToolBarText = shown, ToolTip = RowText.Localize("Set the black and white points from the frame").Local },
        })
        from executed in DisposalOps.OnFailure(
            Subscriptions.Attach(
                h => command.Executed += h, h => command.Executed -= h,
                Callbacks.Handler<EventArgs>(
                    _ => stage.Read.Bind(frame => frame.Match(Some: held => bound.Edit.Commit(Levels.Auto(held, stage.Working)), None: static () => IO.pure(unit))),
                    site with { Member = nameof(Command.Executed) })),
            DisposalOps.Release(lined))
        from auto in DisposalOps.OnFailure(ButtonRows.Push(command), DisposalOps.Release(lined.Add(executed)))
        let show =
            from bracketed in bound.Edit.Shown
            from record in scope.Read(source)
            from lower in black.Show(Some(record))
            from upper in white.Show(Some(record))
            select unit
        select new RowCells(
            view, None, None, None, [], Seq(black.Line, white.Line, new RowLine(None, auto, None, None)), new RowHelp(None, None), show,
            Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(view, Seq(Seq<MenuEntry>(command)), [], scope.Sink)),
            DisposalOps.Composite(lined.Add(executed), site));

    private static Fin<Levels> Into(ScopeEdit edit, Levels held) =>
        edit.Switch(held, chart: static (record, _) => record, points: static (_, points) => points.Value);
}
