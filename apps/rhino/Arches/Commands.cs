using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;

namespace Arches;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
internal abstract partial record ArchStage {
    public sealed record Starting(ArchChoice Choice, bool BothSides) : ArchStage;

    public sealed record Ending(ArchChoice Choice, bool BothSides, Point3d Start, Vector3d Normal, bool Flip) : ArchStage {
        public Line Baseline(Point3d end) => new(BothSides ? Start + (Start - end) : Start, end);

        public Fin<ArchSpan> Span(Point3d end) => ArchSpan.Of(Baseline(end), Flip ? -Normal : Normal);
    }

    public sealed record Rising(PlainArch Arch, ArchSpan Span) : ArchStage {
        public Fin<Point3d> Apex(double height, Tolerances tolerances) =>
            from ratios in AllowedRatios(tolerances)
            let least = ratios.T0 * Span.Baseline.Length
            let most = ratios.T1 * Span.Baseline.Length
            from plain in ArchFault.Unless(ratios.IncludesParameter(double.Abs(height) / Span.Baseline.Length), ArchReason.RiseOutside, (height, least, most))
            select Span.Apex(height);

        public Fin<Point3d> Nearest(Point3d cursor, Tolerances tolerances) =>
            Span.Height(cursor) switch {
                var height => AllowedRatios(tolerances).Map(ratios => Span.Apex(double.CopySign(double.Clamp(double.Abs(height) / Span.Baseline.Length, ratios.T0, ratios.T1), height) * Span.Baseline.Length)),
            };

        private Fin<Interval> AllowedRatios(Tolerances tolerances) =>
            (tolerances.Absolute / Span.Baseline.Length) switch {
                var margin => new Interval(Arch.Ratios.T0 + margin, Arch.Ratios.T1 - margin) switch {
                    var ratios => ArchFault.Unless(ratios.IsIncreasing, ArchReason.SpanNarrow, (length: Span.Baseline.Length, Arch.Ratios)).Map(_ => ratios),
                },
            };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ArchPrompts {
    // --- [OPTIONS]
    public const string Type = "Type";
    public const string BothSides = "BothSides";

    // --- [COMMAND]
    public static IO<Unit> Run(RhinoDoc doc, ArchFamily family) =>
        Tolerances.Read(doc, Space.Model).Bind(tolerances =>
            Getters.Stages<ArchStage, Unit>(new ArchStage.Starting(family.Choices.Head, BothSides: false), (stage, kit) => Prompt(doc, family, tolerances, stage, kit)));

    // --- [STAGES]
    private static IO<StageStep<ArchStage, Unit>> Prompt(RhinoDoc doc, ArchFamily family, Tolerances tolerances, ArchStage stage, StageKit<ArchStage, Unit> kit) =>
        stage.Switch((Doc: doc, Family: family, Tolerances: tolerances, Kit: kit),
            starting: static (at, start) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Unit>>(start.BothSides ? "Middle of span" : "Start of span") {
                Accept = at.Kit.Accept,
                Options = Seq(
                    OptionSpecs.Of(OptionType.List(toSeq(at.Family.Choices), static choice => choice.Option), start.Choice, choice => at.Kit.Replace(start with { Choice = choice }), caption: Type),
                    Toggle(at.Kit, BothSides, LabelPair.NoYes, start.BothSides, both => start with { BothSides = both })),
            }, pick => Conversions.Required(pick.Pick.View).Match(
                Succ: view => at.Kit.Advance(new ArchStage.Ending(start.Choice, start.BothSides, pick.Pick.Point, view.ConstructionPlane.Normal, Flip: false)),
                Fail: at.Kit.Retry)),
            ending: static (at, end) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Unit>>("End of span") {
                Accept = at.Kit.Accept,
                Configure = [getter => IO.lift(() => getter.SetBasePoint(end.Start, showDistanceInStatusBar: true))],
                Options = Seq(
                        Toggle(at.Kit, BothSides, LabelPair.NoYes, end.BothSides, both => end with { BothSides = both }),
                        Toggle(at.Kit, "Flip", LabelPair.NoYes, end.Flip, flip => end with { Flip = flip }))
                    + end.Choice.Switch((at.Kit, End: end),
                        plain: static (_, _) => Seq<OptionSpec<StageStep<ArchStage, Unit>>>(),
                        foiled: static (held, foiled) => Seq(Toggle(held.Kit, "Shape", new LabelPair("Rounded", "Pointed"), foiled.Pointed, pointed => held.End with { Choice = foiled with { Pointed = pointed } }))
                            + (toSeq(foiled.Arch.Lobes) switch {
                                var counts when counts.Tail.IsEmpty => Seq<OptionSpec<StageStep<ArchStage, Unit>>>(),
                                var counts => Seq(OptionSpecs.Of(OptionType.Integer(counts.Head, counts.Last), foiled.Lobes, lobes =>
                                    ArchFault.Unless(counts.Exists(count => count == lobes), ArchReason.LobesOutside, lobes).Match(
                                        Succ: _ => held.Kit.Replace(held.End with { Choice = foiled with { Lobes = lobes } }),
                                        Fail: held.Kit.Retry))),
                            })),
                Handlers = [PointHandler.DynamicDraw(args => end.Span(args.CurrentPoint).Match(
                    Succ: span => Drawn(args.Display, span, end.Choice.Spanned(span).ToOption()),
                    Fail: _ => IO.lift(() => Ends(args.Display, end.Baseline(args.CurrentPoint)))))],
            }, pick => end.Span(pick.Pick.Point).Match(
                Succ: span => end.Choice.Spanned(span).Match(
                    Left: arch => at.Kit.Advance(new ArchStage.Rising(arch, span)),
                    Right: build => Finished(at.Doc, at.Tolerances, at.Kit, build)),
                Fail: at.Kit.Retry)),
            rising: static (at, rise) => Getters.Point(at.Doc, new PointRequest<StageStep<ArchStage, Unit>>("Apex point or rise") {
                Accept = at.Kit.Accept with {
                    Number = new NumberEntry<StageStep<ArchStage, Unit>>.Plain(Zero: false, height => rise.Apex(height, at.Tolerances).Match(
                        Succ: apex => Finished(at.Doc, at.Tolerances, at.Kit, rise.Arch.Curves(rise.Span, apex)),
                        Fail: at.Kit.Retry)),
                },
                Configure = [getter =>
                    from based in IO.lift(() => {
                        getter.SetBasePoint(rise.Span.Midpoint, showDistanceInStatusBar: true);
                        getter.DrawLineFromPoint(rise.Span.Midpoint, showDistanceInStatusBar: true);
                    })
                    from constrained in Products.Accepted(() => getter.Constrain(new Line(rise.Span.Midpoint, rise.Span.Frame.YAxis, 1)))
                    select unit],
                Handlers = [PointHandler.DynamicDraw(args =>
                    Drawn(args.Display, rise.Span, rise.Nearest(args.CurrentPoint, at.Tolerances).ToOption().Map(apex => rise.Arch.Curves(rise.Span, apex))))],
            }, pick => rise.Nearest(pick.Pick.Point, at.Tolerances).Match(
                Succ: apex => Finished(at.Doc, at.Tolerances, at.Kit, rise.Arch.Curves(rise.Span, apex)),
                Fail: at.Kit.Retry)));

    // --- [TRANSITIONS]
    private static OptionSpec<StageStep<ArchStage, Unit>> Toggle(StageKit<ArchStage, Unit> kit, string caption, LabelPair labels, bool initial, Func<bool, ArchStage> next) =>
        OptionSpecs.Of(OptionType.Toggle(labels), initial, value => kit.Replace(next(value)), caption);

    private static IO<StageStep<ArchStage, Unit>> Finished(RhinoDoc doc, Tolerances tolerances, StageKit<ArchStage, Unit> kit, IO<Seq<Curve>> build) =>
        Commits.WithinRedraw(doc, new RedrawPolicy.AllViews(Deferred: true),
                from built in build
                from joined in Products.Many(() => Curve.JoinCurves(built, tolerances.Absolute, preserveDirection: true))
                from added in IO.lift(Products.Each(joined, (curve, index) => Conversions.Required(Conversions.Present(doc.Objects.AddCurve(curve)), index)))
                select unit)
            .Bracket()
            .Bind(kit.Done);

    // --- [FEEDBACK]
    private static IO<Unit> Drawn(DisplayPipeline display, ArchSpan span, Option<IO<Seq<Curve>>> build) =>
        (from curves in build.Traverse(static held => held).As() | @catch(Getters.Retried, static _ => IO.pure(Option<Seq<Curve>>.None))
         from drawn in IO.lift(() => {
             Ends(display, span.Baseline);
             display.DrawDottedLine(span.Apex(-span.Baseline.Length), span.Apex(span.Baseline.Length), AppearanceSettings.TrackingColor);
             display.DrawPoint(span.Midpoint, AppearanceSettings.FeedbackColor);
             foreach (Curve curve in curves.ToSeq().Flatten())
                 Figure(display, curve);
         })
         select drawn).Bracket();

    private static void Figure(DisplayPipeline display, Curve curve) {
        display.DrawCurve(curve, AppearanceSettings.FeedbackColor);
        if (curve is not ArcCurve { Arc: var arc }) return;
        display.DrawPoint(arc.Center, AppearanceSettings.FeedbackColor);
        display.DrawPoint(arc.MidPoint, AppearanceSettings.FeedbackColor);
        Ends(display, new Line(arc.StartPoint, arc.EndPoint));
    }

    private static void Ends(DisplayPipeline display, Line chord) {
        display.DrawLine(chord, AppearanceSettings.TrackingColor);
        display.DrawPoint(chord.From, AppearanceSettings.TrackingColor);
        display.DrawPoint(chord.To, AppearanceSettings.TrackingColor);
    }
}
