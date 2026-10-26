using System.Globalization;
using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
internal abstract partial record ArchChoice {
    private ArchChoice(string option, string label, string tooltip) => (Option, Label, Tooltip) = (option, label, tooltip);
    public string Option { get; }
    public string Label { get; }
    public string Tooltip { get; }
    public sealed record Spanned(string Option, string Label, string Tooltip, Func<ArchSpan, Action<IDisposable>, IO<Seq<Curve>>> Curves) : ArchChoice(Option, Label, Tooltip);
    public sealed record Raised(string Option, string Label, string Tooltip, Interval Ratios, Func<ArchSpan, Point3d, Action<IDisposable>, IO<Seq<Curve>>> Curves) : ArchChoice(Option, Label, Tooltip) {
        public Fin<Point3d> Apex(ArchSpan span, double height, Tolerances tolerances) =>
            Admitted(span, tolerances).Bind(admitted => admitted.IncludesParameter(double.Abs(height / span.Baseline.Length))
                ? Fin.Succ(span.Apex(height))
                : Error.New(ErrorCodes.ValidationFailed, admitted.Max is double.MaxValue
                    ? string.Create(CultureInfo.InvariantCulture, $"Arch rise {height:G4} is shorter than the least rise {admitted.Min * span.Baseline.Length:G4} on this span")
                    : string.Create(CultureInfo.InvariantCulture, $"Arch rise {height:G4} lies outside {admitted.Min * span.Baseline.Length:G4} to {admitted.Max * span.Baseline.Length:G4} on this span")));

        public Fin<Point3d> Nearest(ArchSpan span, double height, Tolerances tolerances) =>
            Admitted(span, tolerances).Map(admitted => span.Apex(double.CopySign(double.Clamp(double.Abs(height) / span.Baseline.Length, admitted.Min, admitted.Max), height) * span.Baseline.Length));

        private Fin<Interval> Admitted(ArchSpan span, Tolerances tolerances) =>
            (tolerances.Absolute / span.Baseline.Length) switch {
                var margin => new Interval(Ratios.T0 + margin, Ratios.T1 - margin) switch {
                    { IsIncreasing: true } admitted => admitted,
                    _ => Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"Arch span {span.Baseline.Length} admits no rise ratio inside {Ratios}")),
                },
            };
    }
    public sealed record Foiled(string Option, string Label, string Tooltip, Func<ArchSpan, bool, Action<IDisposable>, IO<Seq<Curve>>> Curves) : ArchChoice(Option, Label, Tooltip);
    public sealed record Counted(string Option, string Label, string Tooltip, int InitialCount) : ArchChoice(Option, Label, Tooltip);
}

[Union]
internal abstract partial record ArchStage {
    public sealed record Choosing : ArchStage;
    public sealed record Starting(ArchChoice Choice, bool BothSides) : ArchStage;
    public sealed record Ending(ArchChoice Choice, Point3d Start, Vector3d Normal, bool BothSides, bool Flip, bool Pointed) : ArchStage;
    public sealed record Rising(ArchChoice.Raised Choice, ArchSpan Span) : ArchStage {
        public Func<Action<IDisposable>, IO<Seq<Curve>>> Built(Point3d apex) => retain => Choice.Curves(Span, apex, retain);
    }
    public sealed record Counting(ArchSpan Span, int Count, bool Pointed) : ArchStage {
        public Func<Action<IDisposable>, IO<Seq<Curve>>> Built(Tolerances tolerances) => retain => ArchGeometry.Multifoil(Span, Pointed, Count, tolerances, retain);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchPrompts {
    // --- [OPTIONS]
    internal const string BothSides = "BothSides";

    // --- [PROMPTS]
    internal static IO<StageStep<ArchStage, Seq<Guid>>> Pick(RhinoDoc doc, CallbackSite site, string prompt, IterableNE<ArchChoice> choices, Tolerances tolerances, ArchStage stage, Accepts<StageStep<ArchStage, Seq<Guid>>> accept) =>
        stage.Switch((Doc: doc, Site: site, Prompt: prompt, Choices: choices, Tolerances: tolerances, Accept: accept),
            choosing: static (at, _) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(at.Prompt, at.Site) {
                Accept = at.Accept,
                Options = toSeq(at.Choices.Map(static choice => OptionSpecs.Plain(choice.Option, None, hidden: false, _ => Advance(new ArchStage.Starting(choice, BothSides: false))))),
            }, pick => Anchor(pick.Pick, at.Choices.Head, bothSides: false)),
            starting: static (at, start) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(start.BothSides ? "Middle of span" : "First end point", at.Site) {
                Accept = at.Accept,
                Options = Seq(Toggle(BothSides, LabelPair.NoYes, start.BothSides, value => start with { BothSides = value })),
            }, pick => Anchor(pick.Pick, start.Choice, start.BothSides)),
            ending: static (at, end) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(end.BothSides ? "End of span" : "Second end point", at.Site) {
                Accept = at.Accept,
                Configure = [getter => IO.lift(() => getter.SetBasePoint(end.Start, showDistanceInStatusBar: true))],
                Options = Seq(Toggle(BothSides, LabelPair.NoYes, end.BothSides, value => end with { BothSides = value }))
                    + (end.Choice is ArchChoice.Raised ? [] : Seq(Toggle("Flip", LabelPair.NoYes, end.Flip, value => end with { Flip = value })))
                    + (end.Choice is ArchChoice.Foiled or ArchChoice.Counted ? Seq(Toggle("Shape", new LabelPair("Rounded", "Pointed"), end.Pointed, value => end with { Pointed = value })) : []),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => Span(end, args.CurrentPoint).Match(
                    Succ: span => ArchFeedback.Draw(args.Display, span, Previewed(end, span, at.Tolerances)),
                    Fail: _ => ArchFeedback.DrawEnds(args.Display, Baseline(end, args.CurrentPoint))))),
            }, pick => Span(end, pick.Pick.Point).Match(
                Succ: span => end.Choice.Switch((at.Doc, Span: span, end.Pointed, at.Tolerances),
                    spanned: static (candidate, choice) => Finish(candidate.Doc, retain => choice.Curves(candidate.Span, retain), candidate.Tolerances),
                    raised: static (candidate, choice) => Advance(new ArchStage.Rising(choice, candidate.Span)),
                    foiled: static (candidate, choice) => Finish(candidate.Doc, retain => choice.Curves(candidate.Span, candidate.Pointed, retain), candidate.Tolerances),
                    counted: static (candidate, choice) => Advance(new ArchStage.Counting(candidate.Span, choice.InitialCount, candidate.Pointed))),
                Fail: Retry)),
            rising: static (at, rise) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Apex point or rise", at.Site) {
                Accept = at.Accept with {
                    Number = new NumberEntry<StageStep<ArchStage, Seq<Guid>>>.Plain(Zero: true, number => rise.Choice.Apex(rise.Span, number, at.Tolerances).Match(
                        Succ: apex => Finish(at.Doc, rise.Built(apex), at.Tolerances),
                        Fail: Retry)),
                },
                Configure = [getter => IO.lift(() => {
                    getter.SetBasePoint(rise.Span.Midpoint, showDistanceInStatusBar: true);
                    getter.ConstrainDistanceFromBasePoint(0.0);
                    return Refused.Unless(getter.Constrain(new Line(rise.Span.Midpoint, rise.Span.Frame.YAxis)), nameof(GetPoint.Constrain));
                })],
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, rise.Span, Raised(rise, args.CurrentPoint, at.Tolerances).ToOption().Map(rise.Built)))),
            }, pick => Raised(rise, pick.Pick.Point, at.Tolerances).Match(Succ: apex => Finish(at.Doc, rise.Built(apex), at.Tolerances), Fail: Retry)),
            counting: static (at, count) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Foil count", at.Site) {
                Accept = at.Accept with {
                    Nothing = (Some(count.Count.ToString(RowText.Culture)), Finish(at.Doc, count.Built(at.Tolerances), at.Tolerances)),
                    Number = new NumberEntry<StageStep<ArchStage, Seq<Guid>>>.Plain(Zero: false, number => Count(number).Match(
                        Succ: value => Advance(count with { Count = value }),
                        Fail: Retry)),
                },
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, count.Span, Some(count.Built(at.Tolerances))))),
            }, _ => Finish(at.Doc, count.Built(at.Tolerances), at.Tolerances)));

    // --- [INPUT_BOUNDARIES]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Anchor(PointPick pick, ArchChoice choice, bool bothSides) =>
        Next(pick.View.Map(view => new ArchStage.Ending(choice, pick.Point, view.ConstructionPlane.Normal, bothSides, Flip: false, Pointed: false))
            .ToFin(new Refused(nameof(PointPick.View), Refused.Absent)));

    private static Line Baseline(ArchStage.Ending stage, Point3d end) =>
        new(stage.BothSides ? stage.Start + (stage.Start - end) : stage.Start, end);

    private static Fin<ArchSpan> Span(ArchStage.Ending stage, Point3d end) =>
        ArchSpan.Of(Baseline(stage, end), stage.Flip ? -stage.Normal : stage.Normal);

    private static Fin<Point3d> Raised(ArchStage.Rising stage, Point3d cursor, Tolerances tolerances) =>
        stage.Choice.Nearest(stage.Span, stage.Span.Height(cursor), tolerances);

    private static Option<Func<Action<IDisposable>, IO<Seq<Curve>>>> Previewed(ArchStage.Ending stage, ArchSpan span, Tolerances tolerances) =>
        stage.Choice.Switch<(ArchSpan Span, bool Pointed, Tolerances Tolerances), Option<Func<Action<IDisposable>, IO<Seq<Curve>>>>>((span, stage.Pointed, tolerances),
            spanned: static (at, choice) => Some<Func<Action<IDisposable>, IO<Seq<Curve>>>>(retain => choice.Curves(at.Span, retain)),
            raised: static (_, _) => None,
            foiled: static (at, choice) => Some<Func<Action<IDisposable>, IO<Seq<Curve>>>>(retain => choice.Curves(at.Span, at.Pointed, retain)),
            counted: static (at, choice) => Some<Func<Action<IDisposable>, IO<Seq<Curve>>>>(retain => ArchGeometry.Multifoil(at.Span, at.Pointed, choice.InitialCount, at.Tolerances, retain)));

    private static Fin<int> Count(double value) =>
        double.IsOddInteger(value) && value is >= 7 and <= 17
            ? (int)value
            : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"Foil count {value} is no odd whole number from 7 to 17, a multifoil arch keys a lobe at the crown"));

    // --- [TRANSITIONS]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Advance(ArchStage next) =>
        IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(next));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Retry(Error rejection) =>
        IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Retry(rejection));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Next<TStage>(Fin<TStage> next) where TStage : ArchStage =>
        next.Match(Succ: static stage => Advance(stage), Fail: Retry);

    private static OptionSpec<StageStep<ArchStage, Seq<Guid>>> Toggle(string caption, LabelPair labels, bool initial, Func<bool, ArchStage> next) =>
        OptionSpecs.Of(caption, OptionType.Toggle(labels), initial, value => Advance(next(value)));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Finish(RhinoDoc doc, Func<Action<IDisposable>, IO<Seq<Curve>>> build, Tolerances tolerances) =>
        TableOps.Place(doc, retain =>
                from built in build(retain)
                from joined in CurveConstruction.JoinCurves(tolerances, built, preserveDirection: true, simpleJoin: false, retain)
                select joined.Map(static row => (Row: new GeometryPair(row.Value, None), History: Option<HistoryRecord>.None)), reference: false)
            .Map<StageStep<ArchStage, Seq<Guid>>>(static ids => new StageStep<ArchStage, Seq<Guid>>.Done(ids))
            .Catch(Rejected, Retry);

    internal static bool Rejected(Error error) =>
        error.AsIterable().ForAll(static leaf => leaf is Refused or { Code: ErrorCodes.ValidationFailed });
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal sealed class ArchCommand(IPlugInSink sink, Guid id, string englishName, string prompt, IterableNE<ArchChoice> choices) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Tolerances.Read(doc, modelUnits: true).Bind(tolerances => Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: true),
            Getters.Stages<ArchStage, Seq<Guid>>(new ArchStage.Choosing(), (stage, accept) => ArchPrompts.Pick(doc, site, prompt, choices, tolerances, stage, accept))))
            .Map(unit);
}
