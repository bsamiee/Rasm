# [RASM_FABRICATION_CURVES]

`CurveAlgebra` owns manufacturing admission, generated side-pass fields, planar region projection, and witnessed lowering for free-form curves. Kernel curves, arc forests, and canonical `Loop` values retain their owning semantics across every seam.

## [01]-[INDEX]

- [02]-[CURVE_ALGEBRA]: `CurveSource`, `PassPolicy`, `CurveLowering`, `CurveOp`, `CurveTrace`, and the single `CurveAlgebra.Apply` operation owner.

## [02]-[CURVE_ALGEBRA]

- Owner: `SampleClosure` replaces the raw closure knob with open and closed policy rows and owns canonical vertex and fitted-sample projection. `CurveSource` closes sample, arc-outline, and line-sourced chord admission. `PassPolicy` admits one progression generator instead of a caller-authored distance roster, and carries the generated distances it admitted. `CurveLowering` closes chord and recovered-arc egress. `CurveOp` contains only manufacturing concerns; consumers compose the kernel `Parametric.Apply` owner directly.
- Cases: `PassProgression` carries count, maximum-step, and geometric generators. `CurveOp` carries admission, passes, region resolution, and lowering. `CurveTrace` carries fitted admission evidence, pass provenance, oriented regions, or lowering evidence.
- Entry: `CurveAlgebra.Apply(CurveOp)` is the sole public operation, and every case carries its own `Op?` key. `CurveAlgebra` narrows kernel unions only through generated-total `Switch` expressions.
- Auto: closed sample admission normalizes one closure vertex before appending exactly one closing sample. Outline admission composes `ArcProjection.Lower`; chord admission composes `ArcProjection.Recover`. `PassPolicy` generates its admitted distances, and factory validation regenerates the progression to reject divergent stored state; uses read `Distances` without derivation. Every split offset retains pass and output ordinals. Lowering measures each chord's midpoint deviation and optionally recovers residual biarcs under the same requested error.
- Region: `Parametric.Fill` resolves nonzero-winding regions through `ArrangementResult.Overlay`; generated-total narrowing rejects volumetric result cases. Every oriented `Chain` removes only one tolerance-equal closure duplicate and re-enters through `Loop.Admit`, preserving outer and hole orientation with the kernel `BooleanReceipt`.
- Receipt: `CurveAdmissionReceipt` retains sample cardinality or the complete arc bridge receipt. `PassReceipt` retains generated distances, all `RefineReceipt` values, split-output provenance, and trim census. `CurveLoweringReceipt` discriminates chord-only and recovered-arc evidence without an optional recovery field.
- Packages: `Rasm.Parametric` supplies the complete `ParametricOp` and `ParametricResult` algebras, `Nurbs.Of`, `NurbsWire.CurveThrough`, `Parametric.Apply`, and `Parametric.Fill`; `Rasm.Meshing` supplies `ArrangementResult`, `Chain`, and `BooleanReceipt`; `ArcAlgebra.Densify` supplies both exact-to-chord and chord-to-arc projection; `System.Numerics.Tensors` supplies batch finite gates; `LanguageExt` supplies validation, traversal, immutable collections, and typed rails; `Thinktecture` generates every closed request, result, progression, and value owner.
- Growth: a new kernel operation remains a `ParametricOp` case on its owning surface. A manufacturing-only modality adds one `CurveOp` and one `CurveTrace` case. A pass distribution or lowering form adds one generated case and one total dispatch arm without a new entrypoint or parallel carrier.
- Boundary: free-form fitting, evaluation, refinement, splitting, and arrangement stay kernel-owned. `CurveAlgebra` owns closure normalization, manufacturing progression, typed union projection, approximation evidence, and canonical `Loop` egress; no host or provider carrier escapes.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Linq;
using System.Numerics.Tensors;
using Foundation.CSharp.Analyzers.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PassSide {
    public static readonly PassSide Left = new("left", 1.0);
    public static readonly PassSide Right = new("right", -1.0);

    private double Sign { get; }

    internal double Signed(double distance) => Sign * distance;
}

[SmartEnum<string>]
public sealed partial class SampleClosure {
    public static readonly SampleClosure Open = new("open", false);
    public static readonly SampleClosure Closed = new("closed", true);

    public bool IsClosed { get; }

    internal static SampleClosure From(bool closed) => closed ? Closed : Open;

    internal Arr<Point3d> Vertices(Arr<Point3d> points, Context tolerance) =>
        IsClosed && points.Count > 1
        && points[0].DistanceTo(points[points.Count - 1]) <= tolerance.Absolute.Value
            ? new Arr<Point3d>([.. points.Take(points.Count - 1)])
            : points;

    internal Arr<Point3d> Samples(Arr<Point3d> points, Context tolerance) {
        Arr<Point3d> vertices = Vertices(points, tolerance);
        return IsClosed && !vertices.IsEmpty
            ? new Arr<Point3d>([.. vertices, vertices[0]])
            : vertices;
    }
}

[Union]
public abstract partial record PassProgression {
    public sealed record Count(int Passes) : PassProgression;
    public sealed record MaximumStep(double Step) : PassProgression;
    public sealed record Geometric(int Passes, double Ratio) : PassProgression;
}

// Progression owns the distance sequence; admission rejects every supplied sequence that diverges from it.
[ComplexValueObject]
public sealed partial class PassPolicy {
    public double Total { get; }
    public PassProgression Progression { get; }
    public RefinePolicy Refine { get; }
    public Arr<double> Distances { get; }

    public static Validation<Error, PassPolicy> Admit(
        double total,
        PassProgression progression,
        RefinePolicy refine) =>
        Generate(total, progression)
            .ToValidation()
            .Bind(distances =>
                Validate(total, progression, refine, distances, out PassPolicy? policy) is null
                    ? Validation<Error, PassPolicy>.Success(policy!)
                    : Validation<Error, PassPolicy>.Fail(
                        new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-passes:policy").ToError()));

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double total,
        ref PassProgression progression,
        ref RefinePolicy refine,
        ref Arr<double> distances) =>
        validationError = double.IsFinite(total)
            && total > 0.0
            && progression is not null
            && refine is not null
            && refine.IsValid
            && Generated(total, progression, distances)
                ? null
                : new ValidationError(message: "Pass policy requires a finite total, generated distances, and refinement.");

    private static bool Generated(double total, PassProgression progression, Arr<double> distances) =>
        Generate(total, progression).Match(
            Succ: expected => expected.SequenceEqual(distances),
            Fail: static _ => false);

    private static Fin<Arr<double>> Generate(double total, PassProgression progression) => progression is null
        ? Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-passes:progression").ToError())
        : progression.Switch(
            count: request => CountDistances(total, request.Passes),
            maximumStep: request => MaximumDistances(total, request.Step),
            geometric: request => GeometricDistances(total, request.Passes, request.Ratio));

    private static Fin<Arr<double>> CountDistances(double total, int passes) =>
        passes is >= 1 and <= Array.MaxLength
            ? AdmitDistances(Range(1, passes).Map(pass => total * ((double)pass / passes)).ToArr(), total)
            : Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-passes:count").ToError());

    private static Fin<Arr<double>> MaximumDistances(double total, double step) {
        double passes = Math.Ceiling(total / step);
        return double.IsFinite(step) && step > 0.0
            && double.IsFinite(passes) && passes is >= 1.0 and <= Array.MaxLength
                ? CountDistances(total, (int)passes)
                : Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-passes:step").ToError());
    }

    private static Fin<Arr<double>> GeometricDistances(double total, int passes, double ratio) {
        if (passes is < 1 or > Array.MaxLength || !double.IsFinite(ratio) || ratio <= 0.0)
            return Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-passes:geometric").ToError());
        Arr<double> weights = ratio <= 1.0
            ? Range(0, passes).Map(pass => Math.Pow(ratio, pass)).ToArr()
            : Range(0, passes).Map(pass => Math.Pow(ratio, pass - passes + 1)).ToArr();
        double scale = total / weights.Sum();
        Arr<double> generated = weights
            .Scan(0.0, (distance, weight) => distance + (weight * scale))
            .Tail
            .ToArr();
        Arr<double> distances = new([.. generated.Take(generated.Count - 1), total]);
        return AdmitDistances(distances, total);
    }

    private static Fin<Arr<double>> AdmitDistances(Arr<double> distances, double total) =>
        Admissible(distances, total)
            ? Fin.Succ(distances)
            : Fin.Fail<Arr<double>>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-passes:distances").ToError());

    private static bool Admissible(Arr<double> distances, double total) =>
        !distances.IsEmpty
        && TensorPrimitives.IsFiniteAll<double>(distances.ToArray())
        && distances.ForAll(static distance => distance > 0.0)
        && Range(1, distances.Count - 1).ForAll(index => distances[index] > distances[index - 1])
        && distances[distances.Count - 1] == total;

}

// --- [OWNERS] -------------------------------------------------------------------------------------------------------------------------------------
[Union]
public abstract partial record CurveSource {
    public sealed record Samples(
        Arr<Point3d> Points,
        SampleClosure Closure,
        Context Tolerance,
        FitPolicy Fit) : CurveSource;
    public sealed record Outline(Loop Profile, double ChordError, FitPolicy Fit) : CurveSource;
    public sealed record Chords(Loop Profile, double FitError, int ProbeFloor, FitPolicy Fit) : CurveSource;
}

[Union]
public abstract partial record CurveLowering {
    public sealed record Chords(DivideRule Rule) : CurveLowering;
    public sealed record Recovered(DivideRule Rule, double Error, int ProbeFloor) : CurveLowering;
}

[Union]
public abstract partial record CurveOp {
    public sealed record Admit(CurveSource Source, Op? Key) : CurveOp;
    public sealed record Passes(
        NurbsForm.Curve Profile,
        Plane Frame,
        PassSide Side,
        PassPolicy Policy,
        Op? Key) : CurveOp;
    public sealed record Region(
        Arr<NurbsForm.Curve> Loops,
        Axis Plane,
        Context Tolerance,
        ArrangementPolicy Policy,
        Op? Key) : CurveOp;
    public sealed record Lower(
        NurbsForm.Curve Path,
        CurveLowering Lowering,
        Context Tolerance,
        Op? Key) : CurveOp;
}

// --- [EVIDENCE] -----------------------------------------------------------------------------------------------------------------------------------
[Union]
public abstract partial record CurveAdmissionReceipt {
    public sealed record Samples(int Input, int FitSamples, SampleClosure Closure) : CurveAdmissionReceipt;
    public sealed record Outline(DensifyReceipt Receipt) : CurveAdmissionReceipt;
    public sealed record Chords(RecoverReceipt Receipt) : CurveAdmissionReceipt;
}

public sealed record PassCurve(int Pass, int Output, double Distance, NurbsForm.Curve Curve);

public sealed record PassReceipt(
    Arr<double> Distances,
    Arr<RefineReceipt> Refinements,
    int TrimmedCrossings,
    int KeptSegments);

[Union]
public abstract partial record CurveLoweringReceipt {
    public sealed record Chords(
        ParametricResult.Division Division,
        double MaximumMidpointDeviation) : CurveLoweringReceipt;
    public sealed record Recovered(
        ParametricResult.Division Division,
        double MaximumMidpointDeviation,
        RecoverReceipt Recovery) : CurveLoweringReceipt;
}

[Union]
public abstract partial record CurveTrace {
    public sealed record Fitted(NurbsForm.Curve Curve, CurveAdmissionReceipt Receipt) : CurveTrace;
    public sealed record Passes(Arr<PassCurve> Curves, PassReceipt Receipt) : CurveTrace;
    public sealed record Regions(Seq<Loop> Loops, BooleanReceipt Receipt) : CurveTrace;
    public sealed record Lowered(Loop Loop, CurveLoweringReceipt Receipt) : CurveTrace;
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class CurveAlgebra {
    public static Fin<CurveTrace> Apply(CurveOp operation) => operation.Switch(
        admit: static request => Admit(request.Source, request.Key),
        passes: static request => Passes(request),
        region: static request => Region(request),
        lower: static request => Lower(request));

    private static Fin<CurveTrace> Admit(CurveSource source, Op? key) => source.Switch(
        samples: request =>
            from samples in Fin.Succ(request.Closure.Samples(request.Points, request.Tolerance))
            from fitted in Fit(
                samples,
                request.Closure,
                request.Fit,
                key,
                new CurveAdmissionReceipt.Samples(
                    request.Points.Count,
                    samples.Count,
                    request.Closure))
            select fitted,
        outline: request =>
            from trace in ArcAlgebra.Densify(new ArcProjection.Lower(request.Profile, request.ChordError))
            from receipt in trace.DensifiedReceipt.ToFin(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-admit:outline").ToError())
            let closure = SampleClosure.From(request.Profile.Closed)
            from fitted in Fit(
                closure.Samples(receipt.Result.Vertices, request.Profile.Tolerance),
                closure,
                request.Fit,
                key,
                new CurveAdmissionReceipt.Outline(receipt))
            select fitted,
        chords: request =>
            from trace in ArcAlgebra.Densify(new ArcProjection.Recover(
                request.Profile, request.FitError, request.ProbeFloor))
            from receipt in trace.RecoveredReceipt.ToFin(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-admit:chords").ToError())
            let closure = SampleClosure.From(request.Profile.Closed)
            from fitted in Fit(
                closure.Samples(receipt.Result.Vertices, request.Profile.Tolerance),
                closure,
                request.Fit,
                key,
                new CurveAdmissionReceipt.Chords(receipt))
            select fitted);

    private static Fin<CurveTrace> Fit(
        Arr<Point3d> points,
        SampleClosure closure,
        FitPolicy policy,
        Op? key,
        CurveAdmissionReceipt receipt) =>
        points.Count < policy.Degree + 1
            ? Fin.Fail<CurveTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-admit:samples").ToError())
            : Nurbs.Of(new NurbsWire.CurveThrough(points, policy), key)
                .Bind(form => AsCurve(form, "curve-admit:form"))
                .Bind(curve => closure.IsClosed && !curve.IsClosed
                    ? Fin.Fail<NurbsForm.Curve>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-admit:closure").ToError())
                    : Fin.Succ(curve))
                .Map<CurveTrace>(curve => new CurveTrace.Fitted(curve, receipt));

    private static Fin<CurveTrace> Passes(CurveOp.Passes request) =>
        request.Policy.Distances
            .MapIndexed((pass, distance) => Parametric.Apply(new ParametricOp.Offset(
                    request.Profile,
                    request.Frame,
                    request.Side.Signed(distance),
                    request.Policy.Refine), request.Key)
                .Bind(result => AsOffsets(result, "curve-passes:offset"))
                .Map(offset => (Pass: pass, Distance: distance, Offset: offset)))
            .Traverse(static result => result)
            .As()
            .Map<CurveTrace>(results => new CurveTrace.Passes(
                new Arr<PassCurve>([.. results.SelectMany(result => result.Offset.Curves
                    .Select((curve, output) => new PassCurve(result.Pass, output, result.Distance, curve)))]),
                new PassReceipt(
                    request.Policy.Distances,
                    results.Map(static result => result.Offset.Receipt).ToArr(),
                    results.Sum(static result => result.Offset.TrimmedCrossings),
                    results.Sum(static result => result.Offset.KeptSegments))));

    private static Fin<CurveTrace> Region(CurveOp.Region request) =>
        request.Loops.IsEmpty || request.Loops.Exists(static curve => !curve.IsClosed)
            ? Fin.Fail<CurveTrace>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-region:open-loop").ToError())
            : from result in Parametric.Fill(request.Loops, request.Plane, request.Policy, request.Key)
              from overlay in AsOverlay(result, "curve-region:overlay")
              from loops in overlay.Loops.Traverse(chain => LowerChain(chain, request.Tolerance)).As()
              select (CurveTrace)new CurveTrace.Regions(loops.ToSeq(), overlay.Receipt);

    private static Fin<CurveTrace> Lower(CurveOp.Lower request) => request.Lowering.Switch(
        chords: lowering => Divide(request, lowering.Rule)
            .Map<CurveTrace>(row => new CurveTrace.Lowered(
                row.Chords,
                new CurveLoweringReceipt.Chords(row.Division, row.MaximumMidpointDeviation))),
        recovered: lowering =>
            from row in Divide(request, lowering.Rule)
            from trace in ArcAlgebra.Densify(new ArcProjection.Recover(
                row.Chords,
                lowering.Error,
                lowering.ProbeFloor))
            from receipt in trace.RecoveredReceipt.ToFin(
                new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-lower:recover").ToError())
            select (CurveTrace)new CurveTrace.Lowered(
                receipt.Result,
                new CurveLoweringReceipt.Recovered(
                    row.Division,
                    row.MaximumMidpointDeviation,
                    receipt)));

    private static Fin<(
        ParametricResult.Division Division,
        Loop Chords,
        double MaximumMidpointDeviation)> Divide(
        CurveOp.Lower request,
        DivideRule rule) =>
        from result in Parametric.Apply(new ParametricOp.Divide(request.Path, rule), request.Key)
        from division in AsDivision(result, "curve-lower:division")
        let vertices = SampleClosure.From(request.Path.IsClosed).Vertices(division.Points, request.Tolerance)
        from chords in Loop.Admit(
            vertices,
            request.Path.IsClosed,
            toArr(Enumerable.Repeat(0.0, vertices.Count)),
            request.Tolerance)
        from deviation in MaximumMidpointDeviation(request.Path, division)
        select (division, chords, deviation);

    private static Fin<double> MaximumMidpointDeviation(
        NurbsForm.Curve curve,
        ParametricResult.Division division) {
        double deviation = division.Parameters.Count < 2
            ? 0.0
            : Range(0, division.Parameters.Count - 1).Map(index => {
                double parameter = (division.Parameters[index] + division.Parameters[index + 1]) / 2.0;
                Point3d point = curve.PointAt(parameter);
                Line chord = new(division.Points[index], division.Points[index + 1]);
                return chord.DistanceTo(point, limitToFiniteSegment: true);
            }).Max();
        return double.IsFinite(deviation)
            ? Fin.Succ(deviation)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-lower:deviation").ToError());
    }

    private static Fin<Loop> LowerChain(Chain chain, Context tolerance) {
        Arr<Point3d> points = new([.. chain.Points]);
        Arr<Point3d> vertices = SampleClosure.From(chain.Closed).Vertices(points, tolerance);
        return chain.Closed
            ? Loop.Admit(vertices, closed: true, toArr(Enumerable.Repeat(0.0, vertices.Count)), tolerance)
            : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, -1, "curve-region:open-result").ToError());
    }

    private static Fin<NurbsForm.Curve> AsCurve(NurbsForm form, string locus) => form.Switch(
        state: new GeometryFault.DegenerateInput(Kind.Curve, -1, locus).ToError(),
        curve: static (_, curve) => Fin.Succ(curve),
        surface: static (error, _) => Fin.Fail<NurbsForm.Curve>(error));

    private static Fin<ParametricResult.Offsets> AsOffsets(ParametricResult result, string locus) => result.Switch(
        state: new GeometryFault.DegenerateInput(Kind.Curve, -1, locus).ToError(),
        sample: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error),
        measured: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error),
        division: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error),
        stationField: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error),
        pieces: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error),
        refit: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error),
        offsets: static (_, offsets) => Fin.Succ(offsets),
        crossings: static (error, _) => Fin.Fail<ParametricResult.Offsets>(error));

    private static Fin<ParametricResult.Division> AsDivision(ParametricResult result, string locus) => result.Switch(
        state: new GeometryFault.DegenerateInput(Kind.Curve, -1, locus).ToError(),
        sample: static (error, _) => Fin.Fail<ParametricResult.Division>(error),
        measured: static (error, _) => Fin.Fail<ParametricResult.Division>(error),
        division: static (_, division) => Fin.Succ(division),
        stationField: static (error, _) => Fin.Fail<ParametricResult.Division>(error),
        pieces: static (error, _) => Fin.Fail<ParametricResult.Division>(error),
        refit: static (error, _) => Fin.Fail<ParametricResult.Division>(error),
        offsets: static (error, _) => Fin.Fail<ParametricResult.Division>(error),
        crossings: static (error, _) => Fin.Fail<ParametricResult.Division>(error));

    private static Fin<ArrangementResult.Overlay> AsOverlay(ArrangementResult result, string locus) => result.Switch(
        state: new GeometryFault.DegenerateInput(Kind.Curve, -1, locus).ToError(),
        boolean: static (error, _) => Fin.Fail<ArrangementResult.Overlay>(error),
        overlay: static (_, overlay) => Fin.Succ(overlay),
        complex: static (error, _) => Fin.Fail<ArrangementResult.Overlay>(error));

}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
