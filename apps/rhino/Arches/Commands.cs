using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Modeling.Curves;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
internal abstract partial record ArchChoice {
    private ArchChoice(string caption) => Caption = caption;
    public string Caption { get; }
    public sealed record Spanned(string Caption, SpannedArch Kind) : ArchChoice(Caption);
    public sealed record Raised(string Caption, RaisedArch Kind) : ArchChoice(Caption);
    public sealed record Foiled(string Caption, FoiledArch Kind) : ArchChoice(Caption);
    public sealed record Counted(string Caption, FoilCount InitialCount) : ArchChoice(Caption);
}

[Union]
internal abstract partial record ArchStage {
    public sealed record Choosing : ArchStage;
    public sealed record Starting(ArchChoice Choice, bool BothSides) : ArchStage;
    public sealed record Ending(ArchChoice Choice, Point3d Start, Vector3d Normal, bool BothSides, bool Flip, bool Pointed) : ArchStage;
    public sealed record Rising(RaisedArch Kind, ArchSpan Span, Option<double> Rise) : ArchStage;
    public sealed record Counting(ArchSpan Span, bool Pointed, FoilCount Count) : ArchStage;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchPrompts {
    // --- [PROMPTS]
    internal static IO<StageStep<ArchStage, Seq<Guid>>> Pick(RhinoDoc doc, CallbackSite site, string prompt, IterableNE<ArchChoice> choices, Tolerances tolerances, ArchStage stage, Accepts<StageStep<ArchStage, Seq<Guid>>> accept) =>
        Getters.Point(doc, stage.Switch((Doc: doc, Site: site, Prompt: prompt, Choices: choices, Tolerances: tolerances, Accept: new Accepts<IO<StageStep<ArchStage, Seq<Guid>>>> { Undo = accept.Undo.Map(static next => IO.pure(next)) }),
            choosing: static (at, _) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>(at.Prompt, at.Site) {
                Accept = at.Accept,
                Options = toSeq(at.Choices.Map(static choice => OptionSpec.Plain(choice.Caption, None, hidden: false, _ => IO.pure(IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(new ArchStage.Starting(choice, BothSides: false))))))),
            },
            starting: static (at, start) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("First end point", at.Site) {
                Accept = at.Accept,
                Options = Seq(Toggle("BothSides", LabelPair.NoYes, start.BothSides, value => start with { BothSides = value })),
            },
            ending: static (at, end) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("Second end point", at.Site) {
                Accept = at.Accept,
                Configure = getter => IO.lift(() => getter.SetBasePoint(end.Start, showDistanceInStatusBar: true)),
                Options = Seq(Toggle("BothSides", LabelPair.NoYes, end.BothSides, value => end with { BothSides = value }))
                    + (end.Choice is ArchChoice.Raised ? Seq<OptionSpec<IO<StageStep<ArchStage, Seq<Guid>>>>>() : Seq(Toggle("Flip", LabelPair.NoYes, end.Flip, value => end with { Flip = value })))
                    + (end.Choice is ArchChoice.Foiled or ArchChoice.Counted ? Seq(Toggle("Shape", new LabelPair("Rounded", "Pointed"), end.Pointed, value => end with { Pointed = value })) : Seq<OptionSpec<IO<StageStep<ArchStage, Seq<Guid>>>>>()),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => Span(end, args.CurrentPoint).Match(
                    Succ: span => ArchFeedback.Draw(args.Display, span.Baseline, span.Frame, Candidate(span, Previewed(end), at.Tolerances)),
                    Fail: _ => ArchFeedback.DrawEnds(args.Display, Baseline(end, args.CurrentPoint))))),
            },
            rising: static (at, rise) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("Apex point or rise", at.Site) {
                Default = rise.Rise.Map(rise.Span.Apex),
                Accept = at.Accept with {
                    Number = new NumberEntry<IO<StageStep<ArchStage, Seq<Guid>>>>.Plain(Zero: true, number => IO.pure(IO.pure(rise.Kind.Rise(rise.Span, number).Match<StageStep<ArchStage, Seq<Guid>>>(
                        Succ: height => new StageStep<ArchStage, Seq<Guid>>.Advance(rise with { Rise = Some(height) }),
                        Fail: static error => new StageStep<ArchStage, Seq<Guid>>.Retry(error))))),
                },
                Configure = getter => IO.lift(() => {
                    getter.SetBasePoint(rise.Span.Midpoint, showDistanceInStatusBar: true);
                    return Refused.Unless(getter.Constrain(new Line(rise.Span.Midpoint, rise.Span.Frame.YAxis)), nameof(GetPoint.Constrain));
                }),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, rise.Span.Baseline, rise.Span.Frame,
                    Candidate(rise.Span, Raised(rise, args.CurrentPoint, at.Tolerances), at.Tolerances)))),
            },
            counting: static (at, count) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("Foil count", at.Site) {
                Accept = at.Accept with {
                    Nothing = (Some(((int)count.Count).ToString(RowText.Culture)), IO.pure(Finish(at.Doc, count.Span, new ArchForm.Multifoil(count.Count, count.Pointed), at.Tolerances))),
                    Number = new NumberEntry<IO<StageStep<ArchStage, Seq<Guid>>>>.Plain(Zero: false, number => IO.pure(IO.pure(Count(number).Match<StageStep<ArchStage, Seq<Guid>>>(
                        Succ: value => new StageStep<ArchStage, Seq<Guid>>.Advance(count with { Count = value }),
                        Fail: static error => new StageStep<ArchStage, Seq<Guid>>.Retry(error))))),
                },
                Configure = getter => IO.lift(() => getter.SetBasePoint(count.Span.Baseline.From, showDistanceInStatusBar: true)),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, count.Span.Baseline, count.Span.Frame,
                    Candidate(count.Span, new ArchForm.Multifoil(count.Count, count.Pointed), at.Tolerances)))),
            }), pick => IO.pure(stage.Switch((Doc: doc, pick.Pick, Choices: choices, Tolerances: tolerances),
            choosing: static (at, _) => Anchor(at.Pick, at.Choices.Head, bothSides: false),
            starting: static (at, start) => Anchor(at.Pick, start.Choice, start.BothSides),
            ending: static (at, end) => Span(end, at.Pick.Point).Match(
                Succ: span => end.Choice.Switch((at.Doc, Span: span, end.Pointed, at.Tolerances),
                    spanned: static (candidate, choice) => Finish(candidate.Doc, candidate.Span, new ArchForm.Spanned(choice.Kind), candidate.Tolerances),
                    raised: static (candidate, choice) => IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(new ArchStage.Rising(choice.Kind, candidate.Span, None))),
                    foiled: static (candidate, choice) => Finish(candidate.Doc, candidate.Span, new ArchForm.Foiled(choice.Kind, candidate.Pointed), candidate.Tolerances),
                    counted: static (candidate, choice) => IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(new ArchStage.Counting(candidate.Span, candidate.Pointed, choice.InitialCount)))),
                Fail: static error => IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Retry(error))),
            rising: static (at, rise) => Finish(at.Doc, rise.Span, Raised(rise, at.Pick.Point, at.Tolerances), at.Tolerances),
            counting: static (at, count) => Finish(at.Doc, count.Span, new ArchForm.Multifoil(count.Count, count.Pointed), at.Tolerances))))
            .Flatten();

    // --- [INPUT_BOUNDARIES]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Anchor(PointPick pick, ArchChoice choice, bool bothSides) =>
        IO.pure(pick.View.Map<StageStep<ArchStage, Seq<Guid>>>(view => new StageStep<ArchStage, Seq<Guid>>.Advance(
                new ArchStage.Ending(choice, pick.Point, view.ConstructionPlane.Normal, bothSides, Flip: false, Pointed: false)))
            .IfNone(static () => new StageStep<ArchStage, Seq<Guid>>.Retry(new Missing(nameof(PointPick.View)))));

    private static Line Baseline(ArchStage.Ending stage, Point3d end) =>
        new(stage.BothSides ? stage.Start + (stage.Start - end) : stage.Start, end);

    private static Fin<ArchSpan> Span(ArchStage.Ending stage, Point3d end) =>
        ArchSpan.Of(Baseline(stage, end), stage.Flip ? -stage.Normal : stage.Normal);

    private static ArchForm.Raised Raised(ArchStage.Rising stage, Point3d cursor, Tolerances tolerances) =>
        new(stage.Kind, stage.Rise.IfNone(() => stage.Kind.Nearest(stage.Span, stage.Span.Height(cursor), tolerances)));

    private static Option<ArchForm> Previewed(ArchStage.Ending stage) =>
        stage.Choice.Switch<bool, Option<ArchForm>>(stage.Pointed,
            spanned: static (_, choice) => new ArchForm.Spanned(choice.Kind),
            raised: static (_, _) => None,
            foiled: static (pointed, choice) => new ArchForm.Foiled(choice.Kind, pointed),
            counted: static (pointed, choice) => new ArchForm.Multifoil(choice.InitialCount, pointed));

    private static Fin<FoilCount> Count(double value) =>
        double.IsInteger(value) && value is >= int.MinValue and <= int.MaxValue
            ? Conversions.Validated<FoilCount, int, InvalidRhinoValue>((int)value)
            : new InvalidRhinoValue();

    // --- [TRANSITIONS]
    private static OptionSpec<IO<StageStep<ArchStage, Seq<Guid>>>> Toggle(string caption, LabelPair labels, bool initial, Func<bool, ArchStage> next) =>
        OptionSpec.Of(caption, (getter, name, sink, value, retain) => OptionType.Toggle(labels, getter, name, sink, value, retain), initial, value => IO.pure(IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(next(value)))));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Finish(RhinoDoc doc, ArchSpan span, ArchForm form, Tolerances tolerances) =>
        envIO.Bind(environment =>
            DisposalOps.Within((Action<IDisposable> joined) => DisposalOps.Within((Action<IDisposable> parts) =>
                    (from built in IO.liftVAsync(() => CurveConstruction.CreateArch(span, form, tolerances, parts).RunAsync(environment))
                     from checkedToken in IO.lift(environment.Token.ThrowIfCancellationRequested)
                     from curves in CurveConstruction.JoinCurves(built, tolerances, preserveDirection: true, simpleJoin: false, retain: joined)
                     select curves.Map(static row => row.Value))
                    .Map(curves => TableOps.Apply(doc, new TableOp.Add(curves.Map(static curve => (Row: new GeometryPair(curve, None), History: Option<HistoryRecord>.None)), Reference: false))
                        .Map<StageStep<ArchStage, Seq<Guid>>>(static ids => new StageStep<ArchStage, Seq<Guid>>.Done(ids)))
                    .Catch(Rejected, static error => IO.pure(IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Retry(error)))), DisposalOps.Release)
                .Flatten(), DisposalOps.Release)
                .Uninterruptible());

    // --- [CANDIDATES]
    private static Func<Action<IDisposable>, IO<Option<Seq<Curve>>>> Candidate(ArchSpan span, Option<ArchForm> form, Tolerances tolerances) =>
        retain => form.Match(
            Some: held => CurveConstruction.CreateArch(span, held, tolerances, retain).Map(static curves => Some(curves)).Catch(Rejected, static _ => IO.pure(Option<Seq<Curve>>.None)),
            None: static () => IO.pure(Option<Seq<Curve>>.None));

    private static bool Rejected(Error error) =>
        error switch {
            ManyErrors aggregate => aggregate.Errors.ForAll(Rejected),
            _ => error is Missing or Refused or RefusedElement or InvalidRhinoValue or InvalidGeometry or InvalidGeometryElement,
        };
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal sealed class ArchCommand(IPlugInSink sink, Guid id, string englishName, string prompt, IterableNE<ArchChoice> choices) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Tolerances.Read(doc, modelUnits: true).Bind(tolerances => Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: true),
            Getters.Stages<ArchStage, Seq<Guid>>(new ArchStage.Choosing(), (stage, accept) => ArchPrompts.Pick(doc, site, prompt, choices, tolerances, stage, accept))))
            .Map(unit);
}
