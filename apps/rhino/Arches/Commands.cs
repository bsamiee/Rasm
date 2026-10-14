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
internal sealed record ArchSpan(Line Baseline, Plane Frame, Tolerances Tolerances);

internal sealed record ArchRise(Interval Ratios, bool OpenMinimum);

[Union]
internal abstract partial record ArchForm {
    private ArchForm(string caption) => Caption = caption;
    public string Caption { get; }
    public sealed record Spanned(string Caption, Func<ArchSpan, Action<IDisposable>, IO<Seq<Curve>>> Build) : ArchForm(Caption);
    public sealed record Raised(string Caption, Func<ArchSpan, Point3d, Action<IDisposable>, IO<Seq<Curve>>> Build, ArchRise Rise) : ArchForm(Caption);
    public sealed record Foiled(string Caption, Func<ArchSpan, bool, Action<IDisposable>, IO<Seq<Curve>>> Build) : ArchForm(Caption);
    public sealed record Counted(string Caption, Func<ArchSpan, bool, int, Action<IDisposable>, IO<Seq<Curve>>> Build, int InitialCount) : ArchForm(Caption);
}

[Union]
internal abstract partial record ArchStage {
    public sealed record Choosing : ArchStage;
    public sealed record Starting(ArchForm Form, bool BothSides) : ArchStage;
    public sealed record Ending(ArchForm Form, Point3d Start, Vector3d Normal, bool BothSides, bool Flip, bool Pointed) : ArchStage;
    public sealed record Rising(ArchForm.Raised Form, ArchSpan Span, Option<Point3d> Apex) : ArchStage;
    public sealed record Counting(ArchForm.Counted Form, ArchSpan Span, bool Pointed, int Count) : ArchStage;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchPrompts {
    // --- [PROMPTS]
    internal static IO<StageStep<ArchStage, Seq<Guid>>> Pick(RhinoDoc doc, CallbackSite site, string prompt, IterableNE<ArchForm> forms, Tolerances tolerances, ArchStage stage, Accepts<StageStep<ArchStage, Seq<Guid>>> accept) =>
        Getters.Point(doc, stage.Switch((Doc: doc, Site: site, Prompt: prompt, Forms: forms, Tolerances: tolerances, Accept: new Accepts<IO<StageStep<ArchStage, Seq<Guid>>>> { Undo = accept.Undo.Map(static next => IO.pure(next)) }),
            choosing: static (at, _) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>(at.Prompt, at.Site) {
                Accept = at.Accept,
                Options = toSeq(at.Forms.Map(static form => OptionSpec.Plain(form.Caption, None, hidden: false, _ => IO.pure(IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(new ArchStage.Starting(form, BothSides: false))))))),
            },
            starting: static (at, start) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("First end point", at.Site) {
                Accept = at.Accept,
                Options = Seq(Toggle("BothSides", LabelPair.NoYes, start.BothSides, value => start with { BothSides = value })),
            },
            ending: static (at, end) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("Second end point", at.Site) {
                Accept = at.Accept,
                Configure = getter => IO.lift(() => getter.SetBasePoint(end.Start, showDistanceInStatusBar: true)),
                Options = Seq(Toggle("BothSides", LabelPair.NoYes, end.BothSides, value => end with { BothSides = value }))
                    + (end.Form is ArchForm.Raised ? Seq<OptionSpec<IO<StageStep<ArchStage, Seq<Guid>>>>>() : Seq(Toggle("Flip", LabelPair.NoYes, end.Flip, value => end with { Flip = value })))
                    + (end.Form is ArchForm.Foiled or ArchForm.Counted ? Seq(Toggle("Shape", new LabelPair("Rounded", "Pointed"), end.Pointed, value => end with { Pointed = value })) : Seq<OptionSpec<IO<StageStep<ArchStage, Seq<Guid>>>>>()),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => Span(end, args.CurrentPoint, at.Tolerances).Match(
                    Succ: span => ArchFeedback.Draw(args.Display, span.Baseline, span.Frame, Candidate(end.Form.Switch((Span: span, end.Pointed),
                        spanned: static (candidate, form) => Some(fun((Action<IDisposable> retain) => form.Build(candidate.Span, retain))),
                        raised: static (_, _) => Option<Func<Action<IDisposable>, IO<Seq<Curve>>>>.None,
                        foiled: static (candidate, form) => Some(fun((Action<IDisposable> retain) => form.Build(candidate.Span, candidate.Pointed, retain))),
                        counted: static (candidate, form) => Some(fun((Action<IDisposable> retain) => form.Build(candidate.Span, candidate.Pointed, form.InitialCount, retain)))))),
                    Fail: _ => ArchFeedback.DrawEnds(args.Display, Baseline(end, args.CurrentPoint))))),
            },
            rising: static (at, rise) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("Apex point or rise", at.Site) {
                Default = rise.Apex,
                Accept = at.Accept with {
                    Number = new NumberEntry<IO<StageStep<ArchStage, Seq<Guid>>>>.Plain(Zero: true, number => IO.pure(IO.pure(Risen(rise, number).Match<StageStep<ArchStage, Seq<Guid>>>(
                        Succ: apex => new StageStep<ArchStage, Seq<Guid>>.Advance(rise with { Apex = Some(apex) }),
                        Fail: static error => new StageStep<ArchStage, Seq<Guid>>.Retry(error))))),
                },
                Configure = getter => IO.lift(() => {
                    Point3d midpoint = rise.Span.Baseline.PointAt(0.5);
                    getter.SetBasePoint(midpoint, showDistanceInStatusBar: true);
                    return Refused.Unless(getter.Constrain(new Line(midpoint, rise.Span.Frame.YAxis)), nameof(GetPoint.Constrain));
                }),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, rise.Span.Baseline, rise.Span.Frame,
                    Candidate(Some(fun((Action<IDisposable> retain) => rise.Form.Build(rise.Span, rise.Apex.IfNone(() => Raised(rise, args.CurrentPoint)), retain))))))),
            },
            counting: static (at, count) => new PointRequest<IO<StageStep<ArchStage, Seq<Guid>>>>("Foil count", at.Site) {
                Accept = at.Accept with {
                    Nothing = (Some(count.Count.ToString(RowText.Culture)), IO.pure(Finish(at.Doc, retain => count.Form.Build(count.Span, count.Pointed, count.Count, retain), at.Tolerances))),
                    Number = new NumberEntry<IO<StageStep<ArchStage, Seq<Guid>>>>.Plain(Zero: false, number => IO.pure(IO.pure(Count(number).Match<StageStep<ArchStage, Seq<Guid>>>(
                        Succ: value => new StageStep<ArchStage, Seq<Guid>>.Advance(count with { Count = value }),
                        Fail: static error => new StageStep<ArchStage, Seq<Guid>>.Retry(error))))),
                },
                Configure = getter => IO.lift(() => getter.SetBasePoint(count.Span.Baseline.From, showDistanceInStatusBar: true)),
                Handlers = Seq<PointHandler>(new PointHandler.DynamicDraw(args => ArchFeedback.Draw(args.Display, count.Span.Baseline, count.Span.Frame, Candidate(Some(fun((Action<IDisposable> retain) => count.Form.Build(count.Span, count.Pointed, count.Count, retain))))))),
            }), pick => IO.pure(stage.Switch((Doc: doc, pick.Pick, Forms: forms, Tolerances: tolerances),
            choosing: static (at, _) => Anchor(at.Pick, at.Forms.Head, bothSides: false),
            starting: static (at, start) => Anchor(at.Pick, start.Form, start.BothSides),
            ending: static (at, end) => Span(end, at.Pick.Point, at.Tolerances).Match(
                Succ: span => end.Form.Switch((at.Doc, Span: span, end.Pointed, at.Tolerances),
                    spanned: static (candidate, form) => Finish(candidate.Doc, retain => form.Build(candidate.Span, retain), candidate.Tolerances),
                    raised: static (candidate, form) => IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(new ArchStage.Rising(form, candidate.Span, None))),
                    foiled: static (candidate, form) => Finish(candidate.Doc, retain => form.Build(candidate.Span, candidate.Pointed, retain), candidate.Tolerances),
                    counted: static (candidate, form) => IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(new ArchStage.Counting(form, candidate.Span, candidate.Pointed, form.InitialCount)))),
                Fail: static error => IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Retry(error))),
            rising: static (at, rise) => Finish(at.Doc, retain => rise.Form.Build(rise.Span, rise.Apex.IfNone(() => Raised(rise, at.Pick.Point)), retain), at.Tolerances),
            counting: static (at, count) => Finish(at.Doc, retain => count.Form.Build(count.Span, count.Pointed, count.Count, retain), at.Tolerances))))
            .Bind(static next => next);

    // --- [INPUT_BOUNDARIES]
    private static IO<StageStep<ArchStage, Seq<Guid>>> Anchor(PointPick pick, ArchForm form, bool bothSides) =>
        IO.pure(pick.View.Map<StageStep<ArchStage, Seq<Guid>>>(view => new StageStep<ArchStage, Seq<Guid>>.Advance(
                new ArchStage.Ending(form, pick.Point, view.ConstructionPlane.Normal, bothSides, Flip: false, Pointed: false)))
            .IfNone(static () => new StageStep<ArchStage, Seq<Guid>>.Retry(new Missing(nameof(PointPick.View)))));

    private static Line Baseline(ArchStage.Ending stage, Point3d end) =>
        new(stage.BothSides ? stage.Start + (stage.Start - end) : stage.Start, end);

    private static Fin<ArchSpan> Span(ArchStage.Ending stage, Point3d end, Tolerances tolerances) =>
        Baseline(stage, end) switch {
            var baseline => new Plane(baseline.From, baseline.Direction, Vector3d.CrossProduct(stage.Flip ? -stage.Normal : stage.Normal, baseline.Direction)) switch {
                { IsValid: true } frame => new ArchSpan(baseline, frame, tolerances),
                _ => new InvalidRhinoValue(),
            },
        };

    private static Fin<Point3d> Risen(ArchStage.Rising stage, double height) =>
        (double.Abs(height) / stage.Span.Baseline.Length) switch {
            var ratio => stage.Form.Rise.Ratios.IncludesParameter(ratio) && (!stage.Form.Rise.OpenMinimum || ratio > stage.Form.Rise.Ratios.T0)
                ? stage.Span.Baseline.PointAt(0.5) + (stage.Span.Frame.YAxis * height)
                : new InvalidRhinoValue(),
        };

    private static Point3d Raised(ArchStage.Rising stage, Point3d cursor) {
        Point3d midpoint = stage.Span.Baseline.PointAt(0.5);
        double height = (cursor - midpoint) * stage.Span.Frame.YAxis;
        double minimum = (stage.Form.Rise.Ratios.T0 * stage.Span.Baseline.Length) + (stage.Form.Rise.OpenMinimum ? stage.Span.Tolerances.Absolute : 0);
        double maximum = stage.Form.Rise.Ratios.T1 * stage.Span.Baseline.Length;
        return midpoint + (stage.Span.Frame.YAxis * ((height < 0 ? -1 : 1) * double.Min(double.Max(double.Abs(height), minimum), maximum)));
    }

    private static Fin<int> Count(double value) =>
        double.IsInteger(value) && value is >= 1 and <= int.MaxValue
            ? (int)value
            : new InvalidRhinoValue();

    // --- [TRANSITIONS]
    private static OptionSpec<IO<StageStep<ArchStage, Seq<Guid>>>> Toggle(string caption, LabelPair labels, bool initial, Func<bool, ArchStage> next) =>
        OptionSpec.Of(caption, (getter, name, sink, value, retain) => OptionType.Toggle(labels, getter, name, sink, value, retain), initial, value => IO.pure(IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Advance(next(value)))));

    private static IO<StageStep<ArchStage, Seq<Guid>>> Finish(RhinoDoc doc, Func<Action<IDisposable>, IO<Seq<Curve>>> build, Tolerances tolerances) =>
        envIO.Bind(environment =>
            DisposalOps.Within((Action<IDisposable> joined) => DisposalOps.Within((Action<IDisposable> parts) =>
                    (from built in IO.liftVAsync(() => build(parts).RunAsync(environment))
                     from checkedToken in IO.lift(environment.Token.ThrowIfCancellationRequested)
                     from curves in CurveConstruction.JoinCurves(built, tolerances, preserveDirection: true, simpleJoin: false, retain: joined)
                     select curves.Map(static row => row.Value))
                    .Map(curves => TableOps.Apply(doc, new TableOp.Add(curves.Map(static curve => (Row: new GeometryPair(curve, None), History: Option<HistoryRecord>.None)), Reference: false))
                        .Map<StageStep<ArchStage, Seq<Guid>>>(static ids => new StageStep<ArchStage, Seq<Guid>>.Done(ids)))
                    .Catch(Rejected, static error => IO.pure(IO.pure<StageStep<ArchStage, Seq<Guid>>>(new StageStep<ArchStage, Seq<Guid>>.Retry(error)))), DisposalOps.Release)
                .Bind(static next => next), DisposalOps.Release)
                .Uninterruptible());

    // --- [CANDIDATES]
    private static Func<Action<IDisposable>, IO<Option<Seq<Curve>>>> Candidate(Option<Func<Action<IDisposable>, IO<Seq<Curve>>>> build) =>
        retain => build.Match(
            Some: effect => effect(retain).Map(static curves => Some(curves)).Catch(Rejected, static _ => IO.pure(Option<Seq<Curve>>.None)),
            None: static () => IO.pure(Option<Seq<Curve>>.None));

    private static bool Rejected(Error error) =>
        error switch {
            ManyErrors aggregate => aggregate.Errors.ForAll(Rejected),
            _ => error is Missing or Refused or RefusedElement or InvalidGeometry or InvalidGeometryElement,
        };
}

// --- [COMPOSITION] ---------------------------------------------------------------------
internal sealed class ArchCommand(IPlugInSink sink, Guid id, string englishName, string prompt, IterableNE<ArchForm> forms) : HostCommand(sink, id, englishName, None) {
    protected override IO<Unit> Run(RhinoDoc doc, RunMode mode, CallbackSite site) =>
        Tolerances.Read(doc, modelUnits: true).Bind(tolerances => Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: true),
            Getters.Stages<ArchStage, Seq<Guid>>(new ArchStage.Choosing(), (stage, accept) => ArchPrompts.Pick(doc, site, prompt, forms, tolerances, stage, accept))))
            .Map(static _ => unit);
}
