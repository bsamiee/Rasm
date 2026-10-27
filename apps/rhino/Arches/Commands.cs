using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.RhinoCommon;
using Rasm.RhinoCommon.Commands;
using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.Modeling.Curves;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;

namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
internal abstract partial record ArchChoice {
    private ArchChoice(string option, string label, string tooltip) => (Option, Label, Tooltip) = (option, label, tooltip);
    public string Option { get; }
    public string Label { get; }
    public string Tooltip { get; }
    public sealed record Spanned(string Option, string Label, string Tooltip, Func<ArchSpan, IO<Seq<Curve>>> Curves) : ArchChoice(Option, Label, Tooltip);
    public sealed record Raised(string Option, string Label, string Tooltip, Interval Ratios, Func<ArchSpan, Point3d, IO<Seq<Curve>>> Curves) : ArchChoice(Option, Label, Tooltip) {
        public Fin<Point3d> Apex(ArchSpan span, double height, Tolerances tolerances) =>
            from admitted in Admitted(span, tolerances)
            let accepted = admitted.IncludesParameter(double.Abs(height / span.Baseline.Length))
            let least = admitted.Min * span.Baseline.Length
            from risen in admitted.Max is double.MaxValue
                ? ArchFault.Unless(accepted, ArchReason.RiseBelow, (height, least))
                : ArchFault.Unless(accepted, ArchReason.RiseOutside, (height, least, most: admitted.Max * span.Baseline.Length))
            select span.Apex(height);

        public Fin<Point3d> Nearest(ArchSpan span, double height, Tolerances tolerances) =>
            Admitted(span, tolerances).Map(admitted => span.Apex(double.CopySign(double.Clamp(double.Abs(height) / span.Baseline.Length, admitted.Min, admitted.Max), height) * span.Baseline.Length));

        private Fin<Interval> Admitted(ArchSpan span, Tolerances tolerances) =>
            (tolerances.Absolute / span.Baseline.Length) switch {
                var margin => new Interval(Ratios.T0 + margin, Ratios.T1 - margin) switch {
                    var admitted => ArchFault.Unless(admitted.IsIncreasing, ArchReason.SpanNarrow, (span.Baseline.Length, Ratios.T0, Ratios.T1)).Map(_ => admitted),
                },
            };
    }
    public sealed record Foiled(string Option, string Label, string Tooltip, Func<ArchSpan, bool, IO<Seq<Curve>>> Curves) : ArchChoice(Option, Label, Tooltip);
    public sealed record Counted(string Option, string Label, string Tooltip, int InitialCount) : ArchChoice(Option, Label, Tooltip);
}

[Union]
internal abstract partial record ArchStage {
    public sealed record Choosing : ArchStage;
    public sealed record Starting(ArchChoice Choice, bool BothSides) : ArchStage;
    public sealed record Ending(ArchChoice Choice, Point3d Start, Vector3d Normal, bool BothSides, bool Flip, bool Pointed) : ArchStage;
    public sealed record Rising(ArchChoice.Raised Choice, ArchSpan Span) : ArchStage {
        public IO<Seq<Curve>> Built(Point3d apex) => Choice.Curves(Span, apex);
    }
    public sealed record Counting(ArchSpan Span, int Count, bool Pointed) : ArchStage {
        public IO<Seq<Curve>> Built(Tolerances tolerances) => ArchGeometry.Multifoil(Span, Pointed, Count, tolerances);
    }
}

// --- [ERRORS] --------------------------------------------------------------------------
[SmartEnum]
internal sealed partial class ArchReason {
    public static readonly ArchReason RiseBelow = new("rise below the least rise the span admits");
    public static readonly ArchReason RiseOutside = new("rise outside the rises the span admits");
    public static readonly ArchReason SpanNarrow = new("span admitting no rise ratio of the arch type");
    public static readonly ArchReason Planeless = new("baseline and normal spanning no plane");
    public static readonly ArchReason FoilCount = new("foil count other than an odd whole number from 7 to 17, a multifoil arch keys a lobe at the crown");
    public static readonly ArchReason LobeShort = new("foil count making lobes shorter than the absolute tolerance");

    public string Message { get; }
}

internal sealed record ArchFault(string Subject, ArchReason Reason, Option<int> Index = default, Option<Error> Inner = default)
    : Expected(Index.Match(at => string.Create(CultureInfo.InvariantCulture, $"{Reason.Message}: {Subject} at index {at}"), () => $"{Reason.Message}: {Subject}"), ErrorCodes.ValidationFailed, Inner) {
    public static Fin<Unit> Unless<T>(bool accepted, ArchReason reason, T rejected, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        accepted ? unit : new ArchFault($"{names} = {Shown(rejected)}", reason);

    private static string Shown<T>(T rejected) =>
        rejected is ITuple members
            ? $"({string.Join(", ", Range(0, members.Length).ToSeq().Map(index => Convert.ToString(members[index], CultureInfo.InvariantCulture)))})"
            : Convert.ToString(rejected, CultureInfo.InvariantCulture) ?? "";
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchPrompts {
    // --- [OPTIONS]
    internal const string BothSides = "BothSides";

    // --- [PROMPTS]
    internal static IO<StageStep<ArchStage, Seq<Guid>>> Pick(
        RhinoDoc doc, PlugIn plugIn, string prompt, IterableNE<ArchChoice> choices, Tolerances tolerances, ArchStage stage, StageKit<ArchStage, Seq<Guid>> kit) =>
        stage.Switch((Doc: doc, PlugIn: plugIn, Prompt: prompt, Choices: choices, Tolerances: tolerances, Kit: kit),
            choosing: static (at, _) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(at.Prompt, at.PlugIn) {
                Accept = at.Kit.Accept,
                Options = toSeq(at.Choices.Map(choice => OptionSpecs.Plain(choice.Option, None, hidden: false, _ => at.Kit.Advance(new ArchStage.Starting(choice, BothSides: false))))),
            }, pick => Anchor(at.Kit, pick.Pick, at.Choices.Head, bothSides: false)),
            starting: static (at, start) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(start.BothSides ? "Middle of span" : "First end point", at.PlugIn) {
                Accept = at.Kit.Accept,
                Options = Seq(Toggle(at.Kit, BothSides, LabelPair.NoYes, start.BothSides, value => start with { BothSides = value })),
            }, pick => Anchor(at.Kit, pick.Pick, start.Choice, start.BothSides)),
            ending: static (at, end) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(end.BothSides ? "End of span" : "Second end point", at.PlugIn) {
                Accept = at.Kit.Accept,
                Configure = [getter => IO.lift(() => getter.SetBasePoint(end.Start, showDistanceInStatusBar: true))],
                Options = Seq(Toggle(at.Kit, BothSides, LabelPair.NoYes, end.BothSides, value => end with { BothSides = value }))
                    + (end.Choice is ArchChoice.Raised ? [] : Seq(Toggle(at.Kit, "Flip", LabelPair.NoYes, end.Flip, value => end with { Flip = value })))
                    + (end.Choice is ArchChoice.Foiled or ArchChoice.Counted ? Seq(Toggle(at.Kit, "Shape", new LabelPair("Rounded", "Pointed"), end.Pointed, value => end with { Pointed = value })) : []),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => Span(end, args.CurrentPoint).Match(
                    Succ: span => ArchFeedback.Draw(args.Display, span, Previewed(end, span, at.Tolerances)),
                    Fail: _ => ArchFeedback.DrawEnds(args.Display, Baseline(end, args.CurrentPoint))))),
            }, pick => Span(end, pick.Pick.Point).Match(
                Succ: span => end.Choice.Switch((at.Doc, at.Kit, Span: span, end.Pointed, at.Tolerances),
                    spanned: static (candidate, choice) => Finish(candidate.Kit, candidate.Doc, choice.Curves(candidate.Span), candidate.Tolerances),
                    raised: static (candidate, choice) => candidate.Kit.Advance(new ArchStage.Rising(choice, candidate.Span)),
                    foiled: static (candidate, choice) => Finish(candidate.Kit, candidate.Doc, choice.Curves(candidate.Span, candidate.Pointed), candidate.Tolerances),
                    counted: static (candidate, choice) => candidate.Kit.Advance(new ArchStage.Counting(candidate.Span, choice.InitialCount, candidate.Pointed))),
                Fail: at.Kit.Retry)),
            rising: static (at, rise) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Apex point or rise", at.PlugIn) {
                Accept = at.Kit.Accept with {
                    Number = new NumberEntry<StageStep<ArchStage, Seq<Guid>>>.Plain(Zero: true, number => rise.Choice.Apex(rise.Span, number, at.Tolerances).Match(
                        Succ: apex => Finish(at.Kit, at.Doc, rise.Built(apex), at.Tolerances),
                        Fail: at.Kit.Retry)),
                },
                Configure = [getter => IO.lift(() => {
                    getter.SetBasePoint(rise.Span.Midpoint, showDistanceInStatusBar: true);
                    getter.ConstrainDistanceFromBasePoint(0.0);
                    return RhinoFault.Unless(getter.Constrain(new Line(rise.Span.Midpoint, rise.Span.Frame.YAxis)));
                })],
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, rise.Span, Raised(rise, args.CurrentPoint, at.Tolerances).ToOption().Map(rise.Built)))),
            }, pick => Raised(rise, pick.Pick.Point, at.Tolerances).Match(Succ: apex => Finish(at.Kit, at.Doc, rise.Built(apex), at.Tolerances), Fail: at.Kit.Retry)),
            counting: static (at, count) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Foil count", at.PlugIn) {
                Accept = at.Kit.Accept with {
                    Nothing = new NothingEntry<StageStep<ArchStage, Seq<Guid>>>(Some(count.Count.ToString(RowText.Culture)), Finish(at.Kit, at.Doc, count.Built(at.Tolerances), at.Tolerances)),
                    Number = new NumberEntry<StageStep<ArchStage, Seq<Guid>>>.Plain(Zero: false, number => Count(number).Match(
                        Succ: value => at.Kit.Advance(count with { Count = value }),
                        Fail: at.Kit.Retry)),
                },
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, count.Span, Some(count.Built(at.Tolerances))))),
            }, _ => Finish(at.Kit, at.Doc, count.Built(at.Tolerances), at.Tolerances)));

    // --- [INPUT_BOUNDARIES]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Anchor(StageKit<ArchStage, Seq<Guid>> kit, PointPick pick, ArchChoice choice, bool bothSides) =>
        Conversions.Found(pick.View).Match(
            Succ: view => kit.Advance(new ArchStage.Ending(choice, pick.Point, view.ConstructionPlane.Normal, bothSides, Flip: false, Pointed: false)),
            Fail: kit.Retry);

    private static Line Baseline(ArchStage.Ending stage, Point3d end) =>
        new(stage.BothSides ? stage.Start + (stage.Start - end) : stage.Start, end);

    private static Fin<ArchSpan> Span(ArchStage.Ending stage, Point3d end) =>
        ArchSpan.Of(Baseline(stage, end), stage.Flip ? -stage.Normal : stage.Normal);

    private static Fin<Point3d> Raised(ArchStage.Rising stage, Point3d cursor, Tolerances tolerances) =>
        stage.Choice.Nearest(stage.Span, stage.Span.Height(cursor), tolerances);

    private static Option<IO<Seq<Curve>>> Previewed(ArchStage.Ending stage, ArchSpan span, Tolerances tolerances) =>
        stage.Choice.Switch<(ArchSpan Span, bool Pointed, Tolerances Tolerances), Option<IO<Seq<Curve>>>>((span, stage.Pointed, tolerances),
            spanned: static (at, choice) => Some(choice.Curves(at.Span)),
            raised: static (_, _) => None,
            foiled: static (at, choice) => Some(choice.Curves(at.Span, at.Pointed)),
            counted: static (at, choice) => Some(ArchGeometry.Multifoil(at.Span, at.Pointed, choice.InitialCount, at.Tolerances)));

    private static Fin<int> Count(double value) =>
        ArchFault.Unless(double.IsOddInteger(value) && value is >= 7 and <= 17, ArchReason.FoilCount, value).Map(_ => (int)value);

    // --- [TRANSITIONS]
    private static OptionSpec<StageStep<ArchStage, Seq<Guid>>> Toggle(StageKit<ArchStage, Seq<Guid>> kit, string caption, LabelPair labels, bool initial, Func<bool, ArchStage> next) =>
        OptionSpecs.Of(caption, OptionType.Toggle(labels), initial, value => kit.Advance(next(value)));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Finish(StageKit<ArchStage, Seq<Guid>> kit, RhinoDoc doc, IO<Seq<Curve>> build, Tolerances tolerances) =>
        TableOps.Place(doc,
                from built in build
                from joined in CurveConstruction.JoinCurves(tolerances, built, preserveDirection: true, simpleJoin: false)
                select joined.Map(static row => (Row: new GeometryPair(row.Value, None), History: Option<HistoryRecord>.None)), reference: false)
            .Bind(kit.Done);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal sealed class ArchCommand(Guid id, string englishName, string prompt, IterableNE<ArchChoice> choices) : HostCommand(id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode) =>
        Tolerances.Read(doc, modelUnits: true).Bind(tolerances => Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: true),
            Getters.Stages<ArchStage, Seq<Guid>>(new ArchStage.Choosing(), (stage, kit) => ArchPrompts.Pick(doc, PlugIn, prompt, choices, tolerances, stage, kit))))
            .Map(unit);
}
