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
    public sealed record Rising(RaisedArch Kind, ArchSpan Span, Option<ArchForm.Raised> Form) : ArchStage;
    public sealed record Counting(ArchSpan Span, ArchForm.Multifoil Form) : ArchStage;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchPrompts {
    // --- [PROMPTS]
    internal static IO<StageStep<ArchStage, Seq<Guid>>> Pick(RhinoDoc doc, CallbackSite site, string prompt, IterableNE<ArchChoice> choices, Tolerances tolerances, ArchStage stage, Accepts<StageStep<ArchStage, Seq<Guid>>> accept) =>
        stage.Switch((Doc: doc, Site: site, Prompt: prompt, Choices: choices, Tolerances: tolerances, Accept: accept),
            choosing: static (at, _) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>(at.Prompt, at.Site) {
                Accept = at.Accept,
                Options = toSeq(at.Choices.Map(static choice => OptionSpec.Plain(choice.Caption, None, hidden: false, _ => Advance(new ArchStage.Starting(choice, BothSides: false))))),
            }, pick => Anchor(pick.Pick, at.Choices.Head, bothSides: false)),
            starting: static (at, start) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("First end point", at.Site) {
                Accept = at.Accept,
                Options = Seq(Toggle("BothSides", LabelPair.NoYes, start.BothSides, value => start with { BothSides = value })),
            }, pick => Anchor(pick.Pick, start.Choice, start.BothSides)),
            ending: static (at, end) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Second end point", at.Site) {
                Accept = at.Accept,
                Configure = [getter => IO.lift(() => getter.SetBasePoint(end.Start, showDistanceInStatusBar: true))],
                Options = Seq(Toggle("BothSides", LabelPair.NoYes, end.BothSides, value => end with { BothSides = value }))
                    + (end.Choice is ArchChoice.Raised ? [] : Seq(Toggle("Flip", LabelPair.NoYes, end.Flip, value => end with { Flip = value })))
                    + (end.Choice is ArchChoice.Foiled or ArchChoice.Counted ? Seq(Toggle("Shape", new LabelPair("Rounded", "Pointed"), end.Pointed, value => end with { Pointed = value })) : []),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => Span(end, args.CurrentPoint).Match(
                    Succ: span => ArchFeedback.Draw(args.Display, span, Previewed(end), at.Tolerances),
                    Fail: _ => ArchFeedback.DrawEnds(args.Display, Baseline(end, args.CurrentPoint))))),
            }, pick => Span(end, pick.Pick.Point).Match(
                Succ: span => end.Choice.Switch((at.Doc, Span: span, end.Pointed, at.Tolerances),
                    spanned: static (candidate, choice) => Finish(candidate.Doc, candidate.Span, new ArchForm.Spanned(choice.Kind), candidate.Tolerances),
                    raised: static (candidate, choice) => Advance(new ArchStage.Rising(choice.Kind, candidate.Span, None)),
                    foiled: static (candidate, choice) => Finish(candidate.Doc, candidate.Span, new ArchForm.Foiled(choice.Kind, candidate.Pointed), candidate.Tolerances),
                    counted: static (candidate, choice) => Advance(new ArchStage.Counting(candidate.Span, new ArchForm.Multifoil(choice.InitialCount, candidate.Pointed)))),
                Fail: Retry)),
            rising: static (at, rise) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Apex point or rise", at.Site) {
                Default = rise.Form.Map(form => form.Apex(rise.Span)),
                Accept = at.Accept with {
                    Number = new NumberEntry<StageStep<ArchStage, Seq<Guid>>>.Plain(Zero: true, number => Next(ArchForm.Raised.Of(rise.Kind, number / rise.Span.Baseline.Length).Map(form => rise with { Form = Some(form) }))),
                },
                Configure = [getter => IO.lift(() => {
                    getter.SetBasePoint(rise.Span.Midpoint, showDistanceInStatusBar: true);
                    return Refused.Unless(getter.Constrain(new Line(rise.Span.Midpoint, rise.Span.Frame.YAxis)), nameof(GetPoint.Constrain));
                })],
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, rise.Span, Raised(rise, args.CurrentPoint, at.Tolerances), at.Tolerances))),
            }, pick => Finish(at.Doc, rise.Span, Raised(rise, pick.Pick.Point, at.Tolerances), at.Tolerances)),
            counting: static (at, count) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Seq<Guid>>>("Foil count", at.Site) {
                Accept = at.Accept with {
                    Nothing = (Some(((int)count.Form.Count).ToString(RowText.Culture)), Finish(at.Doc, count.Span, count.Form, at.Tolerances)),
                    Number = new NumberEntry<StageStep<ArchStage, Seq<Guid>>>.Plain(Zero: false, number => Next(Count(number).Map(value => count with { Form = count.Form with { Count = value } }))),
                },
                Configure = [getter => IO.lift(() => getter.SetBasePoint(count.Span.Baseline.From, showDistanceInStatusBar: true))],
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, count.Span, count.Form, at.Tolerances))),
            }, _ => Finish(at.Doc, count.Span, count.Form, at.Tolerances)));

    // --- [INPUT_BOUNDARIES]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Anchor(PointPick pick, ArchChoice choice, bool bothSides) =>
        Next(pick.View.Map(view => new ArchStage.Ending(choice, pick.Point, view.ConstructionPlane.Normal, bothSides, Flip: false, Pointed: false))
            .ToFin(new Refused(nameof(PointPick.View), Refused.Absent)));

    private static Line Baseline(ArchStage.Ending stage, Point3d end) =>
        new(stage.BothSides ? stage.Start + (stage.Start - end) : stage.Start, end);

    private static Fin<ArchSpan> Span(ArchStage.Ending stage, Point3d end) =>
        ArchSpan.Of(Baseline(stage, end), stage.Flip ? -stage.Normal : stage.Normal);

    private static ArchForm.Raised Raised(ArchStage.Rising stage, Point3d cursor, Tolerances tolerances) =>
        stage.Form.IfNone(() => ArchForm.Raised.Nearest(stage.Kind, stage.Span, stage.Span.Height(cursor), tolerances));

    private static Option<ArchForm> Previewed(ArchStage.Ending stage) =>
        stage.Choice.Switch<bool, Option<ArchForm>>(stage.Pointed,
            spanned: static (_, choice) => new ArchForm.Spanned(choice.Kind),
            raised: static (_, _) => None,
            foiled: static (pointed, choice) => new ArchForm.Foiled(choice.Kind, pointed),
            counted: static (pointed, choice) => new ArchForm.Multifoil(choice.InitialCount, pointed));

    private static Fin<FoilCount> Count(double value) =>
        double.IsInteger(value) && value is >= int.MinValue and <= int.MaxValue
            ? FoilCount.Validated((int)value)
            : Error.New(ErrorCodes.ValidationFailed, "foil count is no whole number");

    // --- [TRANSITIONS]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Advance(ArchStage next) =>
        IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(next));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Retry(Error rejection) =>
        IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Retry(rejection));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Next<TStage>(Fin<TStage> next) where TStage : ArchStage =>
        next.Match(Succ: static stage => Advance(stage), Fail: Retry);

    private static OptionSpec<StageStep<ArchStage, Seq<Guid>>> Toggle(string caption, LabelPair labels, bool initial, Func<bool, ArchStage> next) =>
        OptionSpec.Of(caption, OptionType.Toggle(labels), initial, value => Advance(next(value)));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Finish(RhinoDoc doc, ArchSpan span, ArchForm form, Tolerances tolerances) =>
        TableOps.Place(doc, retain =>
                from built in CurveConstruction.CreateArch(span, form, tolerances, retain)
                from joined in CurveConstruction.JoinCurves(built, tolerances, preserveDirection: true, simpleJoin: false, retain)
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
