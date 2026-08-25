# [RASM_FABRICATION_CURVES]

`CurveAlgebra` owns manufacturing admission and witnessed lowering for free-form curves. Kernel curves, arc forests, and canonical `Loop` values retain their owning semantics across every seam.

## [01]-[INDEX]

- [02]-[CURVE_ALGEBRA]: `CurveSource`, `CurveLowering`, `CurveOp`, `CurveTrace`, and the single `CurveAlgebra.Apply` operation owner.

## [02]-[CURVE_ALGEBRA]

- Owner: `SampleClosure` replaces the raw closure knob with open and closed policy rows and owns canonical vertex and fitted-sample projection. `CurveSource` closes sample, arc-outline, and line-sourced chord admission. `CurveLowering` closes chord and recovered-arc egress. `CurveOp` contains only manufacturing concerns; consumers compose the kernel `Parametric.Apply` owner directly.
- Cases: `CurveOp` carries admission and lowering. `CurveTrace` carries fitted admission evidence or lowering evidence.
- Law: planar region resolution over free-form loops is the kernel `Arrangement` owner's, reached directly: a page-local case that forwarded `Parametric.Fill` and re-terminated its chains added no manufacturing decision, and the arc and line bands own the region walks every consumer actually issues. A pass distribution is likewise `Toolpath/turning`'s, whose `SweepKind` rows generate roughing and finishing passes from their own process law.
- Entry: `CurveAlgebra.Apply(CurveOp)` is the sole public operation, and every case carries its own `Op?` key — the kernel entry's own provenance shape, taken verbatim at the boundary it crosses.
- Law: `Narrowed` is the ONE kernel-union narrowing gate. Narrowing asks one question — is the returned case the requested one — so a generated total `Switch` whose every other arm returns the same refusal spells that question once per case; the type test spells it once per CALL, and a kernel union gaining a case grows this page by nothing.
- Auto: closed sample admission normalizes one closure vertex before appending exactly one closing sample. Outline admission composes `ArcProjection.Lower`; chord admission composes `ArcProjection.Recover`. Lowering measures each chord's midpoint deviation and optionally recovers residual biarcs under the same requested error.
- Result: `CurveAdmission` retains sample cardinality or the complete arc bridge evidence. `CurveOutput` discriminates chord-only and recovered-arc evidence without an optional recovery field.
- Packages: `Rasm.Parametric` supplies the complete `ParametricOp` and `ParametricResult` algebras, `Nurbs.Of`, `NurbsWire.CurveThrough`, and `Parametric.Apply`; `ArcAlgebra.Densify` supplies both exact-to-chord and chord-to-arc projection; `LanguageExt` supplies validation, traversal, immutable collections, and typed rails; `Thinktecture` generates every closed request, result, and value owner.
- Growth: a new kernel operation remains a `ParametricOp` case on its owning surface; a manufacturing-only modality adds one `CurveOp` and one `CurveTrace` case; a lowering form adds one generated case and one total dispatch arm without a new entrypoint or parallel carrier.
- Boundary: free-form fitting, evaluation, refinement, splitting, and arrangement stay kernel-owned. `CurveAlgebra` owns closure normalization, typed union projection, approximation evidence, and canonical `Loop` egress; no host or provider carrier escapes.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rasm.Parametric;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Geometry2D;

// --- [VOCABULARY] ----------------------------------------------------------------------
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

// --- [OWNERS] --------------------------------------------------------------------------
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
    public sealed record Lower(
        NurbsForm.Curve Path,
        CurveLowering Lowering,
        Context Tolerance,
        Op? Key) : CurveOp;
}

// --- [EVIDENCE] ------------------------------------------------------------------------
[Union]
public abstract partial record CurveAdmission {
    public sealed record Samples(int Input, int FitSamples, SampleClosure Closure) : CurveAdmission;
    public sealed record Outline(DensifyEvidence Evidence) : CurveAdmission;
    public sealed record Chords(RecoverEvidence Evidence) : CurveAdmission;
}

[Union]
public abstract partial record CurveOutput {
    public sealed record Chords(
        ParametricResult.Division Division,
        double MaximumMidpointDeviation) : CurveOutput;
    public sealed record Recovered(
        ParametricResult.Division Division,
        double MaximumMidpointDeviation,
        RecoverEvidence Recovery) : CurveOutput;
}

[Union]
public abstract partial record CurveTrace {
    public sealed record Fitted(NurbsForm.Curve Curve, CurveAdmission Admission) : CurveTrace;
    public sealed record Lowered(Loop Loop, CurveOutput Output) : CurveTrace;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveAlgebra {
    public static Fin<CurveTrace> Apply(CurveOp operation) => operation.Switch(
        admit: static request => Admit(request.Source, request.Key),
        lower: static request => Lower(request));

    private static Fin<CurveTrace> Admit(CurveSource source, Op? key) => source.Switch(
        samples: request =>
            from samples in Fin.Succ(request.Closure.Samples(request.Points, request.Tolerance))
            from fitted in Fit(
                samples,
                request.Closure,
                request.Fit,
                key,
                new CurveAdmission.Samples(
                    request.Points.Count,
                    samples.Count,
                    request.Closure))
            select fitted,
        outline: request =>
            from trace in ArcAlgebra.Densify(new ArcProjection.Lower(request.Profile, request.ChordError))
            from result in trace.Lowering(
                new KernelFault.InvalidValue("curves", "curve-admit:outline"))
            let closure = SampleClosure.From(request.Profile.Closed)
            from fitted in Fit(
                closure.Samples(result.Output.Vertices, request.Profile.Tolerance),
                closure,
                request.Fit,
                key,
                new CurveAdmission.Outline(result))
            select fitted,
        chords: request =>
            from trace in ArcAlgebra.Densify(new ArcProjection.Recover(
                request.Profile, request.FitError, request.ProbeFloor))
            from result in trace.Recovery(
                new KernelFault.InvalidValue("curves", "curve-admit:chords"))
            let closure = SampleClosure.From(request.Profile.Closed)
            from fitted in Fit(
                closure.Samples(result.Output.Vertices, request.Profile.Tolerance),
                closure,
                request.Fit,
                key,
                new CurveAdmission.Chords(result))
            select fitted);

    private static Fin<CurveTrace> Fit(
        Arr<Point3d> points,
        SampleClosure closure,
        FitPolicy policy,
        Op? key,
        CurveAdmission result) =>
        points.Count < policy.Degree + 1
            ? Fin.Fail<CurveTrace>(new GeometryFault.DegenerateInput(Kind.Curve, None, "curve-admit:samples"))
            : Nurbs.Of(new NurbsWire.CurveThrough(points, policy), key)
                .Bind(static form => Narrowed<NurbsForm, NurbsForm.Curve>(form, "curve-admit:form"))
                .Bind(curve => closure.IsClosed && !curve.IsClosed
                    ? Fin.Fail<NurbsForm.Curve>(new GeometryFault.DegenerateInput(Kind.Curve, None, "curve-admit:closure"))
                    : Fin.Succ(curve))
                .Map<CurveTrace>(curve => new CurveTrace.Fitted(curve, result));

    private static Fin<CurveTrace> Lower(CurveOp.Lower request) => request.Lowering.Switch(
        chords: lowering => Divide(request, lowering.Rule)
            .Map<CurveTrace>(row => new CurveTrace.Lowered(
                row.Chords,
                new CurveOutput.Chords(row.Division, row.MaximumMidpointDeviation))),
        recovered: lowering =>
            from row in Divide(request, lowering.Rule)
            from trace in ArcAlgebra.Densify(new ArcProjection.Recover(
                row.Chords,
                lowering.Error,
                lowering.ProbeFloor))
            from result in trace.Recovery(
                new KernelFault.InvalidValue("curves", "curve-lower:recover"))
            select (CurveTrace)new CurveTrace.Lowered(
                result.Output,
                new CurveOutput.Recovered(
                    row.Division,
                    row.MaximumMidpointDeviation,
                    result)));

    private static Fin<(
        ParametricResult.Division Division,
        Loop Chords,
        double MaximumMidpointDeviation)> Divide(
        CurveOp.Lower request,
        DivideRule rule) =>
        from result in Parametric.Apply(new ParametricOp.Divide(request.Path, rule), request.Key)
        from division in Narrowed<ParametricResult, ParametricResult.Division>(result, "curve-lower:division")
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
            : Range(0, division.Parameters.Count - 1).ToSeq().Max(index => {
                double parameter = (division.Parameters[index] + division.Parameters[index + 1]) / 2.0;
                Point3d point = curve.PointAt(parameter);
                Line chord = new(division.Points[index], division.Points[index + 1]);
                return chord.DistanceTo(point, limitToFiniteSegment: true);
            });
        return double.IsFinite(deviation)
            ? Fin.Succ(deviation)
            : Fin.Fail<double>(new GeometryFault.DegenerateInput(Kind.Curve, None, "curve-lower:deviation"));
    }

    private static Fin<TCase> Narrowed<TResult, TCase>(TResult result, string locus)
        where TCase : class, TResult =>
        result is TCase typed
            ? Fin.Succ(typed)
            : Fin.Fail<TCase>(new KernelFault.InvalidValue("curves", locus));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
