# [RASM_FITTING_FIT]

`Fit` recovers the best-fit analytic primitive from a noisy `Point3d` cloud: one `FitOp` runs an efficient-RANSAC sampler under a truncated-cost robust consensus, then refines the winner to its orthogonal-distance minimum. Kind is DATA — `Kinds` arity alone separates a pinned single fit from a multi-kind competition under one shared cost threshold — and a cloud carrying no requested primitive routes the `GeometryFault` channel, never a fabricated best-fit.

Refinement INSTANTIATES the `Solving/solver` `Lm.Minimize` functor through a `Fit.Model : ILmModel` supplying the analytic-`Jacobian` model alone; the λ-ladder stays the functor's. Sampling composes the `Spatial/neighbors` kd-tree lane, `CloudKernel.CovarianceOf` the one cloud-PCA fold, and the `Numerics/matrix` owners every minimal solve; a `RequiresNormals` kind seeds off the `Rasm.Spatial` `VectorCloudMetric.OrientedNormals` field computed UPSTREAM of the boundary, and `Fitted`/`FitPrimitive` ARE the identities `Spatial/reconciliation` `Encode` content-addresses.

## [01]-[INDEX]

- [02]-[FITTING]: `Fit.Apply` folds kind vocabulary, consensus sampling, and orthogonal refine into one typed `Fitted`.

## [02]-[FITTING]

- Owner: `FitKind` `[SmartEnum<string>]` binds the primitive-kind vocabulary, each row carrying its arity and `Dof` columns, its declared `IDrawLane` ordinal, its `Rasm.Domain` `FaultKind` for typed faults, and its `Solve`/`Rebuild` behavior delegates, so kind selection is vocabulary data; `FitPrimitive` `[Union]` carries the analytic geometry behind one generated-`Switch` fold per behavior; `ConsensusCost` and `SampleMode` `[SmartEnum]` name the ORTHOGONAL cost and sampling axes over one sampler; `FitPolicy` is the policy record whose `Of(Context)` factory mints its trusted unit-interval and `Dimension` count columns through generated `Create` and composes the `SolvePolicy` refine ladder, `FitOp` the ONE request record, `Fitted` the typed evidence, `Fit` the static surface nesting its `Candidate` consensus carrier and `Model` `ILmModel` instantiation as private implementation state.
- Entry: `Fit.Apply(FitOp, Context) → Fin<Fitted>` is the one fitting entrypoint; its `Context.Absolute` band scaled by `FitPolicy.InlierScale` is the inlier threshold while the refine ladder is one `SolvePolicy` minted off the same `Context`, never a domain-local epsilon; the two minimal-solver degeneracy gates read `ToleranceLane.Collinear` and `ToleranceLane.Cocircular` SQUARED, both determinants being second-order in the chart. Admission ACCUMULATES every defect — non-finite points, non-finite or tiny normals, normal arity, kind set, and the policy's open-unit `Confidence` and positive `InlierFloor` relations — through one `Validation<Error, T>` traverse exiting `.ToFin()`, each `GeometryFault.DegenerateInput` carrying its kind `FaultKind` and the offending index; a consensus never reaching `FitPolicy.InlierFloor` routes `GeometryFault.InsufficientInliers` carrying the MEASURED fraction, while a kind whose every trial burned routes `DegenerateInput` instead — no candidate measured a fraction to report.
- Auto: one `Apply` call internalizes the pipeline — kd-tree index built once, the PROSAC rank order derived only when `SampleMode.Prosac` is selected (random and NAPSAC draw off the natural index set), per-kind adaptive-budget draw and score, lowest cost kept across kinds, `InlierFloor` gated, winner refined — so a caller supplies data and policy alone, and the trial budget lowers monotonically, never below one, off the GREATEST support any trial reaches inside one bounded pure loop — support governs confidence while cost alone elects the candidate.
- Law: `Fitted` is typed `IValidityEvidence` over the refined primitive and its consensus evidence; `Inliers` rides the admitted INDEX roster so the value hashes structurally; `Rasm.Bim` reconstruction reads `Primitive`+`Inliers` to mint a `ReconstructionPrimitive`+`ElementPredicate`, and the learned-segmentation peer graduates onto this SAME shape.
- Packages: `Rasm.Spatial` (`CloudKernel.CovarianceOf`), `Rasm.Numerics` (`SymmetricMatrix.DecomposeEigenDetailed` + `EigenSolution.PairsIn`, `SymmetricMatrix.FlatIndex`, `Matrix.SolveDetailed`/`LeastSquaresDetailed`, `EpsilonPolicy`), `Rhino.Geometry` (`Point3d`/`Vector3d`/`Plane`/`Sphere`/`Cylinder`/`Circle`/`Line` carriers), `Rasm.Solving` (`Lm.Minimize`/`ILmModel`/`SolvePolicy`), `Spatial/neighbors` (`NeighborIndex.Of`/`NeighborSource.PointsCase`/`NeighborKernel.GraphOf`), `Rasm.Domain` (`Deterministic.Draw`/`Draw.At`/`Draw.State`/`Deterministic.NextBelow` — the bound draw and its lane suffixes — `UnitInterval`/`PositiveMagnitude`, and `IDrawLane<FitKind>`, the DECLARED lane the prefix reads), TYoshimura.DoubleDouble (`ddouble`/`ddouble.Sqrt`/`ddouble.Erf`/`ddouble.Exp`/`ddouble.Log` + `DoubleDoubleEnumerableExpand.Sum`), Thinktecture.Runtime.Extensions (`[Union]`/`[SmartEnum<string>]`/`[SmartEnum]`/`[UseDelegateFromConstructor]`, generated `Switch`), LanguageExt.Core (`Fin`/`Validation`/`Seq`/`Arr`/`Option`, indexed `Choose`, accumulating `Traverse`), BCL inbox (`[InlineArray]`).
- Growth: a new fittable primitive is ONE `FitKind` row and one `FitPrimitive` case with its fold arms — or, where its chart derivative resists hand differentiation, its `Distance` arm alone stated over the `Solving/solver` `Dual<ddouble>` scalar and admitted through an `IDualResidual` conformance beside `Fit.Model`, the derived Jacobian being exact on either route; a new consensus cost is ONE `ConsensusCost` row's `Cost` delegate, a new sampling strategy ONE `SampleMode` row, a new refine knob one `SolvePolicy` column on the shared ladder; multi-primitive extraction is a consumer fold over `Apply` with inlier masking, never a second sampler.
- Boundary: one `Fit.Apply` over one `FitOp` owns fitting entirely — never a per-kind fitter family nor a `Detect` surface — and every `FitPrimitive` dispatch is the compile-exhaustive generated `Switch`, so a new case breaks every fold arm loudly. Consensus defaults to the truncated-cost M-estimator (`Msac`) with the MLESAC mixture likelihood one `ConsensusCost` row (`Mlesac`) beside it, both under the two-gate law: distance band AND `Agreement ≥ NormalFloor` whenever the op carries normals, so a plane cutting a cylinder's diameter collects distance-near points whose normals disagree and charges them the saturated `t²`; the score folds accumulate at 106 bits and candidates compete on that `ddouble` cost — across trials and across kinds — narrowing only at the `Fitted` egress, so the likelihood row's cancelling terms survive a hundred-thousand-point reduce and two near-equal candidates never collapse to a tie. Bounded-support pruning is exact by saturation, private to the score, and gates on the cost row's own `Saturating` column — a saturating candidate whose kind projects a bounding ball (sphere, torus, circle) scores its ball and charges every outside point `t²`, while a non-saturating cost row (the mixture NLL, still rising past the band) and a kind projecting no ball both reduce the full cloud. Refinement minimizes true orthogonal distance with every `Jacobian` arm closed-form, and every draw reads a lane SUFFIX off ONE `Deterministic.Draw` whose prefix binds `FitPolicy.Seed` to the requested kinds' DECLARED `IDrawLane` ordinals at the `Apply` entry, so a fit replays across runtimes AND across roster edits — a positional `Items.IndexOf` lane re-seeds every standing fit on a mid-roster insert, which is why the ordinal is data on the row. Each kind's competition and each trial's minimal set are then INDEPENDENT sub-streams, decorrelated by their lane paths rather than by ordering inside one sequence, and each replays alone. `Apply` is total over `Fin`: declared refusals are `GeometryFault` cases, and a foreign raise crosses `Try.lift` unchanged; the score reduce, the minimal-draw rejection loops, and the `Jacobian` arms are the named span-kernel statement exemption.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DoubleDouble;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Solving;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FitKind : IDrawLane<FitKind> {
    public static readonly FitKind Plane    = new("plane",    lane: 0L, minimalSamples: 3, dof: 3, requiresNormals: false, faultKind: Kind.Plane,    SolvePlane,    RebuildPlane);
    public static readonly FitKind Sphere   = new("sphere",   lane: 1L, minimalSamples: 4, dof: 4, requiresNormals: false, faultKind: Kind.Sphere,   SolveSphere,   RebuildSphere);
    public static readonly FitKind Cylinder = new("cylinder", lane: 2L, minimalSamples: 6, dof: 6, requiresNormals: false, faultKind: Kind.Cylinder, SolveCylinder, RebuildCylinder);
    public static readonly FitKind Cone     = new("cone",     lane: 3L, minimalSamples: 7, dof: 6, requiresNormals: true,  faultKind: Kind.Cone,     SolveCone,     RebuildCone);
    public static readonly FitKind Torus    = new("torus",    lane: 4L, minimalSamples: 8, dof: 7, requiresNormals: true,  faultKind: Kind.Torus,    SolveTorus,    RebuildTorus);
    public static readonly FitKind Line     = new("line",     lane: 5L, minimalSamples: 2, dof: 4, requiresNormals: false, faultKind: Kind.Line,     SolveLine,     RebuildLine);
    public static readonly FitKind Circle   = new("circle",   lane: 6L, minimalSamples: 3, dof: 6, requiresNormals: false, faultKind: Kind.Circle,   SolveCircle,   RebuildCircle);

    public long Lane { get; }
    public int MinimalSamples { get; }
    internal int Dof { get; }
    public bool RequiresNormals { get; }
    internal Kind FaultKind { get; }

    [UseDelegateFromConstructor]
    internal partial Fin<FitPrimitive> Solve(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance);

    [UseDelegateFromConstructor]
    internal partial FitPrimitive Rebuild(ReadOnlySpan<double> parameters);

    // --- [MINIMAL_SOLVERS]
    static Fin<FitPrimitive> SolvePlane(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) {
        Point3d a = cloud[draw[0]], b = cloud[draw[1]], c = cloud[draw[2]];
        Vector3d normal = Vector3d.CrossProduct(b - a, c - a);
        return normal.IsTiny(EpsilonPolicy.ZeroTolerance)
            ? Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Plane, draw[0], "collinear-sample"))
            : Fin.Succ((FitPrimitive)new FitPrimitive.Plane(new Rhino.Geometry.Plane(a, normal)));
    }

    static Fin<FitPrimitive> SolveSphere(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) {
        Point3d a = cloud[draw[0]], b = cloud[draw[1]], c = cloud[draw[2]], d = cloud[draw[3]];
        return MatrixKernel.Dense(
                Dimension.Create(3), Dimension.Create(3),
                new Arr<double>([b.X - a.X, b.Y - a.Y, b.Z - a.Z, c.X - a.X, c.Y - a.Y, c.Z - a.Z, d.X - a.X, d.Y - a.Y, d.Z - a.Z]))
            .Bind(lhs => MatrixKernel.Solve(lhs, new Arr<double>([
                0.5 * (b.DistanceToSquared(Point3d.Origin) - a.DistanceToSquared(Point3d.Origin)),
                0.5 * (c.DistanceToSquared(Point3d.Origin) - a.DistanceToSquared(Point3d.Origin)),
                0.5 * (d.DistanceToSquared(Point3d.Origin) - a.DistanceToSquared(Point3d.Origin))])))
            .Map(solved => {
                Point3d origin = new(solved.Solution[0], solved.Solution[1], solved.Solution[2]);
                return (FitPrimitive)new FitPrimitive.Sphere(new Rhino.Geometry.Sphere(origin, origin.DistanceTo(a)));
            });
    }

    static Fin<FitPrimitive> SolveCylinder(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) {
        Vector3d axis = normals.Match(
            Some: field => Vector3d.CrossProduct(field[draw[0]], field[draw[1]]),
            None: () => cloud[draw[1]] - cloud[draw[0]]);
        if (axis.IsTiny(EpsilonPolicy.ZeroTolerance))
            return Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cylinder, draw[0], "degenerate-axis"));
        Vector3d n = Unit(axis);
        double azimuth = Math.Atan2(n.Y, n.X), polar = Math.Acos(Math.Clamp(n.Z, -1.0, 1.0));
        Vector3d u = AzimuthTangent(azimuth), v = PolarTangent(azimuth, polar);
        Option<(double U, double V)> section = normals.Match(
            Some: field => LineCross(
                InFrame(cloud[draw[0]] - Point3d.Origin, u, v), InFrame(field[draw[0]], u, v),
                InFrame(cloud[draw[1]] - Point3d.Origin, u, v), InFrame(field[draw[1]], u, v),
                band: tolerance.For(lane: ToleranceLane.Collinear).Value),
            None: () => Circumcenter(
                InFrame(cloud[draw[2]] - Point3d.Origin, u, v),
                InFrame(cloud[draw[3]] - Point3d.Origin, u, v),
                InFrame(cloud[draw[4]] - Point3d.Origin, u, v),
                band: tolerance.For(lane: ToleranceLane.Cocircular).Value));
        return section.Match(
            Some: c => {
                Point3d anchor = Point3d.Origin + c.U * u + c.V * v + ((cloud[draw[0]] - Point3d.Origin) * n) * n;
                Vector3d unit = Unit(n);
                double radius = 0.0;
                foreach (int index in draw) {
                    Vector3d rel = cloud[index] - anchor;
                    radius += (rel - (rel * unit) * unit).Length;
                }
                radius /= draw.Length;
                return radius <= 0.0
                    ? Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cylinder, draw[0], "zero-radius"))
                    : Fin.Succ((FitPrimitive)new FitPrimitive.Cylinder(
                        new Rhino.Geometry.Cylinder(new Circle(new Rhino.Geometry.Plane(anchor, n), radius))));
            },
            None: () => Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cylinder, draw[0], "degenerate-section")));
    }

    static Fin<FitPrimitive> SolveCone(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) =>
        normals.Match(
            Some: field => ApexFromNormals(cloud, draw, field).Bind(apex => {
                Vector3d u0 = Unit(cloud[draw[0]] - apex), u1 = Unit(cloud[draw[1]] - apex), u2 = Unit(cloud[draw[2]] - apex);
                Vector3d axis = Vector3d.CrossProduct(u1 - u0, u2 - u0);
                if (axis.IsTiny(EpsilonPolicy.ZeroTolerance))
                    return Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cone, draw[0], "degenerate-axis"));
                Vector3d unit = Unit(axis);
                double half = 0.0;
                foreach (int index in draw) {
                    Vector3d rel = cloud[index] - apex;
                    double along = Math.Abs(rel * unit);
                    half += Math.Atan2((rel - (rel * unit) * unit).Length, along);
                }
                return Fin.Succ((FitPrimitive)new FitPrimitive.Cone(apex, unit, half / draw.Length));
            }),
            None: () => Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Cone, None, "no-normal-field")));

    static Fin<FitPrimitive> SolveTorus(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) =>
        normals.Match(
            Some: field => {
                Vector3d cross = Vector3d.CrossProduct(field[draw[0]], field[draw[1]]);
                Vector3d axis = cross.IsTiny(EpsilonPolicy.ZeroTolerance) ? field[draw[0]] : cross;
                if (axis.IsTiny(EpsilonPolicy.ZeroTolerance))
                    return Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Torus, draw[0], "degenerate-axis"));
                Vector3d sum = Vector3d.Zero;
                foreach (int index in draw) sum += cloud[index] - Point3d.Origin;
                Point3d center = Point3d.Origin + sum / draw.Length;
                Vector3d unit = Unit(axis);
                double major = 0.0;
                double[] radial = new double[draw.Length];
                for (int i = 0; i < draw.Length; i++) {
                    Vector3d rel = cloud[draw[i]] - center;
                    radial[i] = (rel - (rel * unit) * unit).Length;
                    major += radial[i];
                }
                major /= draw.Length;
                double minor = 0.0;
                for (int i = 0; i < draw.Length; i++) {
                    Vector3d rel = cloud[draw[i]] - center;
                    double along = rel * unit;
                    double inPlane = radial[i] - major;
                    minor += Math.Sqrt(inPlane * inPlane + along * along);
                }
                return Fin.Succ((FitPrimitive)new FitPrimitive.Torus(center, unit, major, minor / draw.Length));
            },
            None: () => Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Torus, None, "no-normal-field")));

    static Fin<FitPrimitive> SolveLine(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) {
        Point3d a = cloud[draw[0]], b = cloud[draw[1]];
        Vector3d direction = b - a;
        return direction.IsTiny(EpsilonPolicy.ZeroTolerance)
            ? Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Line, draw[0], "coincident-sample"))
            : Fin.Succ((FitPrimitive)new FitPrimitive.Line(new Rhino.Geometry.Line(a, b)));
    }

    static Fin<FitPrimitive> SolveCircle(Point3d[] cloud, int[] draw, Option<Vector3d[]> normals, Context tolerance) {
        Point3d a = cloud[draw[0]], b = cloud[draw[1]], c = cloud[draw[2]];
        Vector3d normal = Vector3d.CrossProduct(b - a, c - a);
        if (normal.IsTiny(EpsilonPolicy.ZeroTolerance))
            return Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Circle, draw[0], "collinear-sample"));
        Vector3d n = Unit(normal);
        double azimuth = Math.Atan2(n.Y, n.X), polar = Math.Acos(Math.Clamp(n.Z, -1.0, 1.0));
        Vector3d u = AzimuthTangent(azimuth), v = PolarTangent(azimuth, polar);
        return Circumcenter(
                InFrame(a - Point3d.Origin, u, v), InFrame(b - Point3d.Origin, u, v), InFrame(c - Point3d.Origin, u, v),
                band: tolerance.For(lane: ToleranceLane.Cocircular).Value)
            .Match(
                Some: section => {
                    Point3d center = Point3d.Origin + section.U * u + section.V * v + (((a - Point3d.Origin) * n) * n);
                    return Fin.Succ((FitPrimitive)new FitPrimitive.Circle(
                        new Rhino.Geometry.Circle(new Rhino.Geometry.Plane(center, n), center.DistanceTo(a))));
                },
                None: () => Fin.Fail<FitPrimitive>(new GeometryFault.DegenerateInput(Kind.Circle, draw[0], "collinear-sample")));
    }

    static (double U, double V) InFrame(Vector3d w, Vector3d u, Vector3d v) => (w * u, w * v);

    static Option<(double U, double V)> LineCross((double U, double V) a, (double U, double V) da, (double U, double V) b, (double U, double V) db, double band) {
        double det = (da.U * db.V) - (da.V * db.U);
        if (Math.Abs(det) <= band * band) return None;
        double t = (((b.U - a.U) * db.V) - ((b.V - a.V) * db.U)) / det;
        return Some((a.U + t * da.U, a.V + t * da.V));
    }

    static Option<(double U, double V)> Circumcenter((double U, double V) a, (double U, double V) b, (double U, double V) c, double band) {
        double d = 2.0 * ((a.U * (b.V - c.V)) + (b.U * (c.V - a.V)) + (c.U * (a.V - b.V)));
        if (Math.Abs(d) <= band * band) return None;
        double a2 = (a.U * a.U) + (a.V * a.V), b2 = (b.U * b.U) + (b.V * b.V), c2 = (c.U * c.U) + (c.V * c.V);
        return Some((
            ((a2 * (b.V - c.V)) + (b2 * (c.V - a.V)) + (c2 * (a.V - b.V))) / d,
            ((a2 * (c.U - b.U)) + (b2 * (a.U - c.U)) + (c2 * (b.U - a.U))) / d));
    }

    static Fin<Point3d> ApexFromNormals(Point3d[] cloud, int[] draw, Vector3d[] normals) {
        int n = draw.Length;
        double[] lhs = new double[n * 3];
        double[] rhs = new double[n];
        for (int i = 0; i < n; i++) {
            Vector3d nrm = Unit(normals[draw[i]]);
            (lhs[i * 3], lhs[(i * 3) + 1], lhs[(i * 3) + 2]) = (nrm.X, nrm.Y, nrm.Z);
            rhs[i] = nrm.X * cloud[draw[i]].X + nrm.Y * cloud[draw[i]].Y + nrm.Z * cloud[draw[i]].Z;
        }
        return MatrixKernel.Dense(Dimension.Create(n), Dimension.Create(3), new Arr<double>(lhs))
            .Bind(design => MatrixKernel.LeastSquares(design, new Arr<double>(rhs)))
            .Map(solved => new Point3d(solved.Solution[0], solved.Solution[1], solved.Solution[2]));
    }

    // --- [CHART_REBUILD]
    static FitPrimitive RebuildPlane(ReadOnlySpan<double> p) {
        Vector3d foot = new(p[0], p[1], p[2]);
        Vector3d unit = foot.IsTiny(EpsilonPolicy.ZeroTolerance) ? Vector3d.ZAxis : Unit(foot);
        return new FitPrimitive.Plane(new Rhino.Geometry.Plane(Point3d.Origin + foot, unit));
    }

    static FitPrimitive RebuildSphere(ReadOnlySpan<double> p) =>
        new FitPrimitive.Sphere(new Rhino.Geometry.Sphere(new Point3d(p[0], p[1], p[2]), Math.Max(p[3], 0.0)));

    static FitPrimitive RebuildCylinder(ReadOnlySpan<double> p) =>
        new FitPrimitive.Cylinder(new Rhino.Geometry.Cylinder(
            new Circle(new Rhino.Geometry.Plane(new Point3d(p[0], p[1], p[2]), AxisFrom(p[3], p[4])), Math.Max(p[5], 0.0))));

    static FitPrimitive RebuildCone(ReadOnlySpan<double> p) =>
        new FitPrimitive.Cone(new Point3d(p[0], p[1], p[2]), AxisFrom(p[3], p[4]), p[5]);

    static FitPrimitive RebuildTorus(ReadOnlySpan<double> p) =>
        new FitPrimitive.Torus(new Point3d(p[0], p[1], p[2]), AxisFrom(p[3], p[4]), Math.Max(p[5], 0.0), Math.Max(p[6], 0.0));

    static FitPrimitive RebuildLine(ReadOnlySpan<double> p) {
        Vector3d direction = AxisFrom(p[2], p[3]);
        Point3d anchor = Point3d.Origin + p[0] * AzimuthTangent(p[2]) + p[1] * PolarTangent(p[2], p[3]);
        return new FitPrimitive.Line(new Rhino.Geometry.Line(anchor, anchor + direction));
    }

    static FitPrimitive RebuildCircle(ReadOnlySpan<double> p) =>
        new FitPrimitive.Circle(new Rhino.Geometry.Circle(
            new Rhino.Geometry.Plane(new Point3d(p[0], p[1], p[2]), AxisFrom(p[3], p[4])), Math.Max(p[5], 0.0)));

    internal static Vector3d AxisFrom(double azimuth, double polar) =>
        new(Math.Sin(polar) * Math.Cos(azimuth), Math.Sin(polar) * Math.Sin(azimuth), Math.Cos(polar));

    internal static Vector3d AzimuthTangent(double azimuth) => new(-Math.Sin(azimuth), Math.Cos(azimuth), 0.0);

    internal static Vector3d PolarTangent(double azimuth, double polar) =>
        new(Math.Cos(polar) * Math.Cos(azimuth), Math.Cos(polar) * Math.Sin(azimuth), -Math.Sin(polar));

    internal static Vector3d Unit(Vector3d v) { double len = v.Length; return len == 0.0 ? v : (1.0 / len) * v; }
}

[SmartEnum]
public sealed partial class ConsensusCost {
    public static readonly ConsensusCost Msac   = new(saturating: true, Truncated);
    public static readonly ConsensusCost Mlesac = new(saturating: false, MixtureNll);

    public bool Saturating { get; }

    [UseDelegateFromConstructor]
    public partial ddouble Cost(double squaredDistance, double squaredThreshold);

    const double MixturePrior = 0.5;
    static readonly ddouble BandNormalization = ddouble.Erf(ddouble.Sqrt(2.0));

    static ddouble Truncated(double d2, double t2) => Math.Min(d2, t2);

    static ddouble MixtureNll(double d2, double t2) {
        double sigma2 = t2 / 4.0;
        ddouble inlier = ddouble.Exp(-(ddouble)d2 / (2.0 * sigma2)) / (ddouble.Sqrt(Math.Tau * sigma2) * BandNormalization);
        ddouble outlier = 0.5 / ddouble.Sqrt(t2);
        return -ddouble.Log((MixturePrior * inlier) + ((1.0 - MixturePrior) * outlier));
    }
}

[SmartEnum]
public sealed partial class SampleMode {
    public static readonly SampleMode Random = new();
    public static readonly SampleMode Prosac = new();
    public static readonly SampleMode Napsac = new();
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FitPolicy(
    ConsensusCost Cost,
    SampleMode Sampling,
    UnitInterval InlierFloor,
    UnitInterval Confidence,
    PositiveMagnitude InlierScale,
    UnitInterval NormalFloor,
    Dimension MaxTrials,
    long Seed,
    Dimension Neighbors,
    SolvePolicy Refine) {
    public static Fin<FitPolicy> Of(Context context, Option<SolvePolicy> refine = default) =>
        refine.Match(Some: Fin.Succ, None: () => SolvePolicy.Of(context: context))
            .Map(solve => new FitPolicy(
                ConsensusCost.Msac, SampleMode.Random,
                UnitInterval.Create(0.5), UnitInterval.Create(0.999),
                PositiveMagnitude.Create(2.5), UnitInterval.Create(0.9),
                Dimension.Create(TrialCeiling), 0x5EED, Dimension.Create(32), solve));

    const int TrialCeiling = 1 << 16;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FitPrimitive {
    private FitPrimitive() { }

    [InlineArray(JacobianRow.Arity)]
    internal struct JacobianRow {
        internal const int Arity = 7;
        double element0;
    }

    public sealed record Plane(Rhino.Geometry.Plane Surface) : FitPrimitive;
    public sealed record Sphere(Rhino.Geometry.Sphere Surface) : FitPrimitive;
    public sealed record Cylinder(Rhino.Geometry.Cylinder Surface) : FitPrimitive;
    public sealed record Cone(Point3d Apex, Vector3d Axis, double HalfAngle) : FitPrimitive;
    public sealed record Torus(Point3d Center, Vector3d Axis, double Major, double Minor) : FitPrimitive;
    public sealed record Line(Rhino.Geometry.Line Axis) : FitPrimitive;
    public sealed record Circle(Rhino.Geometry.Circle Curve) : FitPrimitive;

    public FitKind Kind =>
        Switch(
            plane:    static _ => FitKind.Plane,
            sphere:   static _ => FitKind.Sphere,
            cylinder: static _ => FitKind.Cylinder,
            cone:     static _ => FitKind.Cone,
            torus:    static _ => FitKind.Torus,
            line:     static _ => FitKind.Line,
            circle:   static _ => FitKind.Circle);

    public double Distance(Point3d query) =>
        Switch(
            state: query,
            plane:    static (q, pl) => pl.Surface.DistanceTo(q),
            sphere:   static (q, s) => q.DistanceTo(s.Surface.Center) - s.Surface.Radius,
            cylinder: static (q, c) => AxisDistance(c.Surface.Center, c.Surface.Axis, q) - c.Surface.Radius,
            cone:     static (q, k) => ConeDistance(k.Apex, k.Axis, k.HalfAngle, q),
            torus:    static (q, t) => TorusDistance(t.Center, t.Axis, t.Major, t.Minor, q),
            line:     static (q, ln) => AxisDistance(ln.Axis.From, ln.Axis.Direction, q),
            circle:   static (q, c) => TorusDistance(c.Curve.Center, c.Curve.Normal, c.Curve.Radius, 0.0, q));

    internal JacobianRow Jacobian(Point3d query) =>
        Switch(
            state: query,
            plane:    static (q, pl) => PlaneJacobian(q, pl),
            sphere:   static (q, s) => SphereJacobian(q, s),
            cylinder: static (q, c) => CylinderJacobian(q, c),
            cone:     static (q, k) => ConeJacobian(q, k),
            torus:    static (q, t) => RevolveJacobian(t.Center, t.Axis, t.Major, Some(t.Minor), q),
            line:     static (q, ln) => LineJacobian(q, ln),
            circle:   static (q, c) => RevolveJacobian(c.Curve.Center, c.Curve.Normal, c.Curve.Radius, None, q));

    public double[] Pack() =>
        Switch(
            plane:    static pl => PackPlane(pl.Surface),
            sphere:   static s => [s.Surface.Center.X, s.Surface.Center.Y, s.Surface.Center.Z, s.Surface.Radius],
            cylinder: static c => [c.Surface.Center.X, c.Surface.Center.Y, c.Surface.Center.Z, Math.Atan2(c.Surface.Axis.Y, c.Surface.Axis.X), Math.Acos(Math.Clamp(FitKind.Unit(c.Surface.Axis).Z, -1.0, 1.0)), c.Surface.Radius],
            cone:     static k => [k.Apex.X, k.Apex.Y, k.Apex.Z, Math.Atan2(k.Axis.Y, k.Axis.X), Math.Acos(Math.Clamp(FitKind.Unit(k.Axis).Z, -1.0, 1.0)), k.HalfAngle],
            torus:    static t => [t.Center.X, t.Center.Y, t.Center.Z, Math.Atan2(t.Axis.Y, t.Axis.X), Math.Acos(Math.Clamp(FitKind.Unit(t.Axis).Z, -1.0, 1.0)), t.Major, t.Minor],
            line:     static ln => PackLine(ln.Axis),
            circle:   static c => [c.Curve.Center.X, c.Curve.Center.Y, c.Curve.Center.Z, Math.Atan2(c.Curve.Normal.Y, c.Curve.Normal.X), Math.Acos(Math.Clamp(FitKind.Unit(c.Curve.Normal).Z, -1.0, 1.0)), c.Curve.Radius]);

    public double Agreement(Point3d query, Vector3d normal) =>
        Switch(
            state: (Query: query, Normal: normal),
            plane:    static (s, pl) => Math.Abs(FitKind.Unit(s.Normal) * FitKind.Unit(pl.Surface.Normal)),
            sphere:   static (s, sp) => Math.Abs(FitKind.Unit(s.Normal) * FitKind.Unit(s.Query - sp.Surface.Center)),
            cylinder: static (s, c) => Math.Abs(FitKind.Unit(s.Normal) * AxisFrame(c.Surface.Center, FitKind.Unit(c.Surface.Axis), s.Query).Dir),
            cone:     static (s, k) => Math.Abs(FitKind.Unit(s.Normal) * ConeNormal(s.Query, k)),
            torus:    static (s, t) => Math.Abs(FitKind.Unit(s.Normal) * TorusNormal(s.Query, t)),
            line:     static (s, ln) => Perpendicularity(FitKind.Unit(s.Normal) * FitKind.Unit(ln.Axis.Direction)),
            circle:   static (s, c) => Perpendicularity(FitKind.Unit(s.Normal) * CircleTangent(s.Query, c)));

    // --- [JACOBIAN_ARMS]
    static JacobianRow PlaneJacobian(Point3d query, Plane pl) {
        JacobianRow row = new();
        Vector3d f = pl.Surface.Origin - Point3d.Origin;
        double rho = Math.Max(f.Length, EpsilonPolicy.ZeroTolerance);
        Vector3d u = (1.0 / rho) * f;
        Vector3d qv = query - Point3d.Origin;
        Vector3d perp = qv - (u * qv) * u;
        row[0] = perp.X / rho - u.X;
        row[1] = perp.Y / rho - u.Y;
        row[2] = perp.Z / rho - u.Z;
        return row;
    }

    static JacobianRow SphereJacobian(Point3d query, Sphere s) {
        JacobianRow row = new();
        Vector3d e = query - s.Surface.Center;
        double rho = Math.Max(e.Length, EpsilonPolicy.ZeroTolerance);
        row[0] = -e.X / rho;
        row[1] = -e.Y / rho;
        row[2] = -e.Z / rho;
        row[3] = -1.0;
        return row;
    }

    static JacobianRow CylinderJacobian(Point3d query, Cylinder c) {
        JacobianRow row = new();
        Vector3d axis = FitKind.Unit(c.Surface.Axis);
        (double along, double radial, Vector3d dir, Vector3d rel) = AxisFrame(c.Surface.Center, axis, query);
        double rg = Math.Max(radial, EpsilonPolicy.ZeroTolerance);
        Vector3d az = AxisAzimuth(axis), pol = AxisPolar(axis);
        row[0] = -dir.X;
        row[1] = -dir.Y;
        row[2] = -dir.Z;
        row[3] = -along * (rel * az) / rg;
        row[4] = -along * (rel * pol) / rg;
        row[5] = -1.0;
        return row;
    }

    static JacobianRow ConeJacobian(Point3d query, Cone k) {
        JacobianRow row = new();
        Vector3d axis = FitKind.Unit(k.Axis);
        (double along, double radial, Vector3d dir, Vector3d rel) = AxisFrame(k.Apex, axis, query);
        double rg = Math.Max(radial, EpsilonPolicy.ZeroTolerance);
        double cos = Math.Cos(k.HalfAngle), sin = Math.Sin(k.HalfAngle);
        Vector3d az = AxisAzimuth(axis), pol = AxisPolar(axis);
        double angular = cos * along / rg + sin;
        row[0] = -cos * dir.X + sin * axis.X;
        row[1] = -cos * dir.Y + sin * axis.Y;
        row[2] = -cos * dir.Z + sin * axis.Z;
        row[3] = -(rel * az) * angular;
        row[4] = -(rel * pol) * angular;
        row[5] = -sin * radial - cos * along;
        return row;
    }

    static JacobianRow RevolveJacobian(Point3d center, Vector3d axis, double major, Option<double> minor, Point3d query) {
        JacobianRow row = new();
        Vector3d unit = FitKind.Unit(axis);
        (double along, double radial, Vector3d dir, Vector3d rel) = AxisFrame(center, unit, query);
        double inPlane = radial - major;
        double w = Math.Max(Math.Sqrt((inPlane * inPlane) + (along * along)), EpsilonPolicy.ZeroTolerance);
        double rg = Math.Max(radial, EpsilonPolicy.ZeroTolerance);
        Vector3d az = AxisAzimuth(unit), pol = AxisPolar(unit);
        double angular = along * major / (w * rg);
        row[0] = -((inPlane * dir.X) + (along * unit.X)) / w;
        row[1] = -((inPlane * dir.Y) + (along * unit.Y)) / w;
        row[2] = -((inPlane * dir.Z) + (along * unit.Z)) / w;
        row[3] = (rel * az) * angular;
        row[4] = (rel * pol) * angular;
        row[5] = -inPlane / w;
        if (minor.IsSome) { row[6] = -1.0; }
        return row;
    }

    static JacobianRow LineJacobian(Point3d query, Line ln) {
        JacobianRow row = new();
        double[] p = PackLine(ln.Axis);
        Vector3d n = FitKind.AxisFrom(p[2], p[3]);
        Vector3d u = FitKind.AzimuthTangent(p[2]);
        Vector3d v = FitKind.PolarTangent(p[2], p[3]);
        Point3d anchor = Point3d.Origin + p[0] * u + p[1] * v;
        (double along, double radial, Vector3d dir, Vector3d rel) = AxisFrame(anchor, n, query);
        double rg = Math.Max(radial, EpsilonPolicy.ZeroTolerance);
        double sinPolar = Math.Sin(p[3]), cosPolar = Math.Cos(p[3]);
        Vector3d w = new(Math.Cos(p[2]), Math.Sin(p[2]), 0.0);
        row[0] = -(dir * u);
        row[1] = -(dir * v);
        row[2] = dir * (p[0] * w - p[1] * cosPolar * u) - along * sinPolar * (rel * u) / rg;
        row[3] = -along * (rel * v) / rg;
        return row;
    }

    // --- [DISTANCE_KERNELS]
    static double AxisDistance(Point3d origin, Vector3d axis, Point3d query) {
        Vector3d rel = query - origin;
        Vector3d unit = FitKind.Unit(axis);
        double along = rel * unit;
        return (rel - along * unit).Length;
    }

    static double ConeDistance(Point3d apex, Vector3d axis, double halfAngle, Point3d query) {
        Vector3d rel = query - apex;
        Vector3d unit = FitKind.Unit(axis);
        double along = rel * unit;
        double radial = (rel - along * unit).Length;
        return Math.Cos(halfAngle) * radial - Math.Sin(halfAngle) * along;
    }

    static double TorusDistance(Point3d center, Vector3d axis, double major, double minor, Point3d query) {
        Vector3d rel = query - center;
        Vector3d unit = FitKind.Unit(axis);
        double along = rel * unit;
        double radial = (rel - along * unit).Length;
        double inPlane = radial - major;
        return Math.Sqrt(inPlane * inPlane + along * along) - minor;
    }

    static Vector3d ConeNormal(Point3d query, Cone k) {
        Vector3d axis = FitKind.Unit(k.Axis);
        (_, _, Vector3d dir, _) = AxisFrame(k.Apex, axis, query);
        return Math.Cos(k.HalfAngle) * dir - Math.Sin(k.HalfAngle) * axis;
    }

    static Vector3d TorusNormal(Point3d query, Torus t) {
        Vector3d axis = FitKind.Unit(t.Axis);
        (double along, double radial, Vector3d dir, _) = AxisFrame(t.Center, axis, query);
        double inPlane = radial - t.Major;
        double w = Math.Max(Math.Sqrt(inPlane * inPlane + along * along), EpsilonPolicy.ZeroTolerance);
        return (inPlane / w) * dir + (along / w) * axis;
    }

    static Vector3d CircleTangent(Point3d query, Circle c) {
        Vector3d axis = FitKind.Unit(c.Curve.Normal);
        (_, _, Vector3d dir, _) = AxisFrame(c.Curve.Center, axis, query);
        return Vector3d.CrossProduct(axis, dir);
    }

    static double Perpendicularity(double alignment) => Math.Sqrt(Math.Max(0.0, 1.0 - alignment * alignment));

    static (double Along, double Radial, Vector3d Dir, Vector3d Rel) AxisFrame(Point3d origin, Vector3d axis, Point3d query) {
        Vector3d rel = query - origin;
        double along = rel * axis;
        Vector3d g = rel - along * axis;
        double radial = g.Length;
        Vector3d dir = radial < EpsilonPolicy.ZeroTolerance ? Vector3d.Zero : (1.0 / radial) * g;
        return (along, radial, dir, rel);
    }

    static Vector3d AxisAzimuth(Vector3d axis) => new(-axis.Y, axis.X, 0.0);

    static Vector3d AxisPolar(Vector3d axis) {
        double rxy = Math.Max(Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y), EpsilonPolicy.ZeroTolerance);
        return new Vector3d(axis.Z * axis.X / rxy, axis.Z * axis.Y / rxy, -rxy);
    }

    static double[] PackPlane(Rhino.Geometry.Plane plane) {
        Vector3d normal = FitKind.Unit(plane.Normal);
        double offset = normal * (plane.Origin - Point3d.Origin);
        return [normal.X * offset, normal.Y * offset, normal.Z * offset];
    }

    static double[] PackLine(Rhino.Geometry.Line line) {
        Vector3d n = FitKind.Unit(line.Direction);
        double azimuth = Math.Atan2(n.Y, n.X);
        double polar = Math.Acos(Math.Clamp(n.Z, -1.0, 1.0));
        Vector3d from = line.From - Point3d.Origin;
        Vector3d foot = from - (from * n) * n;
        return [foot * FitKind.AzimuthTangent(azimuth), foot * FitKind.PolarTangent(azimuth, polar), azimuth, polar];
    }
}

public sealed record FitOp(Seq<FitKind> Kinds, Point3d[] Cloud, Option<Vector3d[]> Normals, FitPolicy Policy);

public sealed record Fitted(
    FitPrimitive Primitive,
    Arr<int> Inliers,
    double Rms,
    UnitInterval Consensus,
    int Trial,
    int Iterations) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Primitive is not null,
        ValidityClaim.CountAtLeast(count: Inliers.Count, floor: 1),
        ValidityClaim.Finite(Rms),
        ValidityClaim.Nonnegative(Rms),
        ValidityClaim.CountAtLeast(count: Trial, floor: 1),
        ValidityClaim.CountAtLeast(count: Iterations, floor: 0));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Fit {
    private readonly record struct Candidate(FitPrimitive Primitive, Arr<int> Inliers, ddouble Cost, int Trial);

    private sealed class Model(FitPrimitive template, Point3d[] cloud, Arr<int> inliers) : ILmModel {
        public int Dof => template.Kind.Dof;

        public double[] Seed { get; } = template.Pack();

        public ddouble Norm(ReadOnlySpan<double> parameters) {
            FitPrimitive at = template.Kind.Rebuild(parameters);
            return ddouble.Sqrt(inliers
                .Select(index => { double d = at.Distance(cloud[index]); return (ddouble)d * d; })
                .Sum());
        }

        public (double[] PackedNormal, double[] Gradient) Linearize(ReadOnlySpan<double> parameters) {
            int m = Dof;
            double[] normal = new double[m * (m + 1) / 2];
            double[] gradient = new double[m];
            FitPrimitive at = template.Kind.Rebuild(parameters);
            foreach (int index in inliers) {
                Point3d q = cloud[index];
                double residual = at.Distance(q);
                FitPrimitive.JacobianRow partials = at.Jacobian(q);
                for (int a = 0; a < m; a++) {
                    gradient[a] += partials[a] * residual;
                    for (int b = a; b < m; b++) normal[SymmetricMatrix.FlatIndex(m, a, b)] += partials[a] * partials[b];
                }
            }
            return (normal, gradient);
        }
    }

    public static Fin<Fitted> Apply(FitOp op, Context tolerance) {
        return Try.lift(() => {
            Deterministic.Draw draw = new(Seed: op.Policy.Seed, Prefix: [.. op.Kinds.Map(static kind => kind.Lane)]);
            int[] whole = [.. Enumerable.Range(0, op.Cloud.Length)];
            return Validate()
                .Bind(_ => NeighborIndex.Of(new NeighborSource.PointsCase(toSeq(op.Cloud)), ok))
                .Bind(index => Order(op.Cloud, op.Normals, op.Policy, tolerance, ok)
                    .Bind(order => op.Kinds
                        .Fold(Option<Candidate>.None, (best, kind) => Draw(op.Cloud, op.Normals, index, order, whole, kind, op.Policy, tolerance, draw, ok).Match(
                            Some: next => Some(best.Filter(held => held.Cost <= next.Cost).IfNone(next)),
                            None: () => best))
                        .ToFin(new GeometryFault.DegenerateInput(Kind.PointCloud, None, "no-candidate")))
                    .Bind(best => {
                        UnitInterval fraction = UnitInterval.Create((double)best.Inliers.Count / op.Cloud.Length);
                        return fraction.Value < op.Policy.InlierFloor.Value
                            ? Fin.Fail<Fitted>(new GeometryFault.InsufficientInliers(fraction, op.Policy.InlierFloor))
                            : Refine(best, op.Cloud, op.Normals, index, whole, op.Policy, tolerance, ok);
                    }));
        }).Run().Bind(static inner => inner);
    }

    static Fin<Unit> Validate(FitOp op) {
        int minimal = op.Kinds.Map(static kind => kind.MinimalSamples).Fold(0, Math.Max);
        Seq<Validation<Error, Unit>> probes =
            (op.Kinds.IsEmpty
                ? Seq((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, None, "empty-kind-set"))
                : Seq<Validation<Error, Unit>>())
            + (op.Cloud.Length < minimal
                ? Seq((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, None, $"fewer-than-minimal:{op.Cloud.Length}<{minimal}"))
                : Seq<Validation<Error, Unit>>())
            + toSeq(op.Cloud).Choose((index, point) => point.IsValid
                ? Option<Validation<Error, Unit>>.None
                : Some((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.Point, index, "non-finite")))
            + op.Normals.Match(
                Some: field =>
                    (field.Length != op.Cloud.Length
                        ? Seq((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, None, $"normals-arity:{field.Length}!={op.Cloud.Length}"))
                        : Seq<Validation<Error, Unit>>())
                    + toSeq(field).Choose((index, normal) => normal.IsValid && !normal.IsTiny(EpsilonPolicy.ZeroTolerance)
                        ? Option<Validation<Error, Unit>>.None
                        : Some((Validation<Error, Unit>)new GeometryFault.DegenerateInput(Kind.PointCloud, index, "invalid-normal"))),
                None: static () => Seq<Validation<Error, Unit>>())
            + (op.Normals.IsNone
                ? op.Kinds.Filter(static kind => kind.RequiresNormals).Map(kind =>
                    (Validation<Error, Unit>)new GeometryFault.DegenerateInput(kind.FaultKind, None, "no-normal-field"))
                : Seq<Validation<Error, Unit>>())
            + Seq(
                AdmissionSlots.Gate(op.Policy.Confidence.Value is > 0.0 and < 1.0,
                    new GeometryFault.DegenerateInput(Kind.PointCloud, None, "confidence-open-unit")),
                AdmissionSlots.Gate(op.Policy.InlierFloor.Value > 0.0,
                    new GeometryFault.DegenerateInput(Kind.PointCloud, None, "inlier-floor-positive")));
        return AdmissionSlots.Accumulate(probes).ToFin();
    }

    // --- [CONSENSUS]
    const long TrialLane = 0L;

    static Option<Candidate> Draw(
        Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] order, int[] whole,
        FitKind kind, FitPolicy policy, Context tolerance, Deterministic.Draw draw) {
        double threshold = policy.InlierScale.Value * tolerance.Absolute.Value;
        double t2 = threshold * threshold;
        Deterministic.Draw lane = draw.At(TrialLane, kind.Lane);
        Option<Candidate> best = None;
        int budget = policy.MaxTrials.Value, support = 0;
        for (int trial = 0; trial < budget; trial++) {
            int[] sample = Sample(order, cloud, index, kind, policy, trial, lane);
            if (kind.Solve(cloud, sample, normals, tolerance).Case is not FitPrimitive primitive) continue;
            (ddouble cost, Arr<int> inliers) = Score(primitive, cloud, normals, index, whole, policy, t2, threshold);
            if (inliers.Count > support) {
                support = inliers.Count;
                double fraction = (double)support / cloud.Length;
                double miss = 1.0 - Math.Pow(fraction, kind.MinimalSamples);
                int estimate = miss <= 0.0 ? 1 : miss >= 1.0 ? policy.MaxTrials.Value
                    : (int)Math.Min(policy.MaxTrials.Value, Math.Ceiling(Math.Log(1.0 - policy.Confidence.Value) / Math.Log(miss)));
                budget = Math.Min(budget, Math.Max(1, estimate));
            }
            best = Some(best.Filter(held => held.Cost <= cost).IfNone(new Candidate(primitive, inliers, cost, trial + 1)));
        }
        return best;
    }

    static (ddouble Cost, Arr<int> Inliers) Score(
        FitPrimitive primitive, Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] whole,
        FitPolicy policy, double t2, double threshold) {
        if (!policy.Cost.Saturating)
            return Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold);
        Option<(Point3d Center, double Reach)> support = primitive.Switch(
            state: threshold,
            plane:    static (_, _) => Option<(Point3d, double)>.None,
            sphere:   static (t, s) => Some((s.Surface.Center, s.Surface.Radius + t)),
            cylinder: static (_, _) => Option<(Point3d, double)>.None,
            cone:     static (_, _) => Option<(Point3d, double)>.None,
            torus:    static (t, r) => Some((r.Center, r.Major + r.Minor + t)),
            line:     static (_, _) => Option<(Point3d, double)>.None,
            circle:   static (t, c) => Some((c.Curve.Center, c.Curve.Radius + t)));
        return support.Match(
            Some: ball => NeighborKernel.GraphOf(index, [ball.Center], Option<Dimension>.None,
                Some(PositiveMagnitude.Create(ball.Reach))).Match(
                Succ: graph => Scored(primitive, cloud, graph.Ids[0],
                    policy.Cost.Cost(t2, t2) * (cloud.Length - graph.Ids[0].Length), normals, policy, t2, threshold),
                Fail: _ => Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold)),
            None: () => Scored(primitive, cloud, whole, ddouble.Zero, normals, policy, t2, threshold));
    }

    static (ddouble Cost, Arr<int> Inliers) Scored(
        FitPrimitive primitive, Point3d[] cloud, int[] domain, ddouble preCharge,
        Option<Vector3d[]> normals, FitPolicy policy, double t2, double threshold) {
        Func<int, bool> agrees = normals.Match(
            Some: field => (Func<int, bool>)(i => primitive.Agreement(cloud[i], field[i]) >= policy.NormalFloor.Value),
            None: static () => static _ => true);
        ddouble cost = preCharge;
        List<int> inliers = new(domain.Length);
        foreach (int i in domain) {
            double d = primitive.Distance(cloud[i]);
            bool held = agrees(i);
            cost += held ? policy.Cost.Cost(d * d, t2) : policy.Cost.Cost(t2, t2);
            if (held && Math.Abs(d) <= threshold) { inliers.Add(i); }
        }
        return (cost, new Arr<int>(inliers));
    }

    static Fin<int[]> Order(Point3d[] cloud, Option<Vector3d[]> normals, FitPolicy policy, Context tolerance) {
        int[] indices = [.. Enumerable.Range(0, cloud.Length)];
        if (policy.Sampling != SampleMode.Prosac) return Acceptance.Value(indices);
        return normals.Match(
                Some: field => {
                    Vector3d mean = Vector3d.Zero;
                    foreach (Vector3d normal in field) mean += normal;
                    Vector3d mode = FitKind.Unit(mean);
                    double[] rank = new double[field.Length];
                    for (int i = 0; i < field.Length; i++) rank[i] = Math.Abs(field[i] * mode);
                    return Acceptance.Value(rank);
                },
                None: () => CloudKernel.CovarianceOf(toSeq(cloud), Option<Arr<double>>.None)
                    .Bind(stats => stats.Cov.DecomposeEigenDetailed()
                        .Map(static solved => solved.Pairs)
                        .Map(eigen => (stats.Mean, Eigen: eigen)))
                    .Bind(pca => {
                        if (pca.Eigen.Count < 3) return Fin.Fail<double[]>(new KernelFault.InvalidResult());
                        Vector3d axis = new(pca.Eigen[2].Eigenvector[0], pca.Eigen[2].Eigenvector[1], pca.Eigen[2].Eigenvector[2]);
                        double floor = Math.Max(Math.Sqrt(Math.Abs(pca.Eigen[2].Eigenvalue)), tolerance.Absolute.Value);
                        double[] rank = new double[cloud.Length];
                        for (int i = 0; i < cloud.Length; i++) {
                            Vector3d rel = cloud[i] - (Point3d.Origin + pca.Mean);
                            rank[i] = 1.0 / (1.0 + Math.Abs(rel * axis) / floor);
                        }
                        return Acceptance.Value(rank);
                    }))
            .Map(rank => {
                System.Array.Sort(indices, (a, b) => rank[b].CompareTo(rank[a]));
                return indices;
            });
    }

    static int[] Sample(int[] order, Point3d[] cloud, NeighborIndex index, FitKind kind, FitPolicy policy, int trial, Deterministic.Draw lane) =>
        policy.Sampling.Switch(
            state: (Order: order, Cloud: cloud, Index: index, Kind: kind, Policy: policy, Trial: trial, State: lane.At(trial).State),
            random: static s => {
                ulong draw = s.State;
                return UniformDraw(s.Order, s.Kind.MinimalSamples, ref draw);
            },
            prosac: static s => {
                int window = Math.Min(s.Order.Length, s.Kind.MinimalSamples + s.Trial);
                int[] sample = new int[s.Kind.MinimalSamples];
                sample[0] = s.Order[window - 1];
                ulong draw = s.State;
                for (int i = 1; i < sample.Length; i++) {
                    int pick;
                    do { pick = s.Order[Deterministic.NextBelow(state: ref draw, exclusiveCeiling: window - 1)]; }
                    while (System.Array.IndexOf(sample, pick, 0, i) >= 0);
                    sample[i] = pick;
                }
                return sample;
            },
            napsac: static s => {
                ulong draw = s.State;
                int seed = s.Order[Deterministic.NextBelow(state: ref draw, exclusiveCeiling: s.Order.Length)];
                return NeighborKernel.GraphOf(s.Index, [s.Cloud[seed]], Some(s.Policy.Neighbors), Option<PositiveMagnitude>.None, s.Key).Match(
                    Succ: graph => {
                        int[] pool = graph.Ids[0].Where(id => id != seed).ToArray();
                        if (pool.Length < s.Kind.MinimalSamples - 1) return UniformDraw(s.Order, s.Kind.MinimalSamples, ref draw);
                        int[] sample = new int[s.Kind.MinimalSamples];
                        sample[0] = seed;
                        for (int i = 1; i < sample.Length; i++) {
                            int pick = Deterministic.NextBelow(state: ref draw, exclusiveCeiling: pool.Length - i + 1);
                            sample[i] = pool[pick];
                            (pool[pick], pool[pool.Length - i]) = (pool[pool.Length - i], pool[pick]);
                        }
                        return sample;
                    },
                    Fail: _ => UniformDraw(s.Order, s.Kind.MinimalSamples, ref draw));
            });

    static int[] UniformDraw(int[] order, int count, ref ulong state) {
        int[] sample = new int[count];
        for (int i = 0; i < count; i++) {
            int pick;
            do { pick = order[Deterministic.NextBelow(state: ref state, exclusiveCeiling: order.Length)]; } while (System.Array.IndexOf(sample, pick, 0, i) >= 0);
            sample[i] = pick;
        }
        return sample;
    }

    // --- [REFINE]
    static Fin<Fitted> Refine(Candidate seed, Point3d[] cloud, Option<Vector3d[]> normals, NeighborIndex index, int[] whole, FitPolicy policy, Context tolerance) =>
        Lm.Minimize(new Model(seed.Primitive, cloud, seed.Inliers), policy.Refine).Bind(result => {
            FitPrimitive refined = seed.Primitive.Kind.Rebuild(result.Parameters.AsSpan());
            double threshold = policy.InlierScale.Value * tolerance.Absolute.Value;
            (ddouble _, Arr<int> mask) = Score(refined, cloud, normals, index, whole, policy, threshold * threshold, threshold);
            return mask.IsEmpty
                ? Fin.Fail<Fitted>(new KernelFault.InvalidResult())
                : Acceptance.Value(new Fitted(
                    refined, mask,
                    (double)ddouble.Sqrt(mask.Select(i => { double d = refined.Distance(cloud[i]); return (ddouble)d * d; }).Sum()) / Math.Sqrt(mask.Count),
                    UnitInterval.Create((double)mask.Count / cloud.Length), seed.Trial, result.Iterations));
        });
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Primitive fit dispatch
    accDescr: One fit request admits, shares a kd-tree lane between draw and prefilter, competes kinds on cost, refines through the LM functor, and returns typed evidence.
    Op["FitOp (Seq&lt;FitKind&gt; · cloud · normals · policy)"] -->|accumulating Validation| Admit
    Admit -->|"NeighborIndex.Of(PointsCase) once"| KdTree["Supercluster kd-tree lane"]
    KdTree -->|"NAPSAC seed → GraphOf neighborhood"| Draw["normal-gated truncated-cost sampler per kind"]
    KdTree -->|"bounding ball → GraphOf radius"| Shell["exact shell prefilter (sphere/torus/circle)"]
    Draw -->|"lowest cost across kinds"| Best[Candidate]
    Shell --> Draw
    Best -->|"Fit.Model : ILmModel"| Lm["Solving/solver Lm.Minimize"]
    Lm -->|"refined primitive + mask"| Fitted
    Fitted -->|"Primitive + Inliers"| Bim["Rasm.Bim reconstruction"]
    Op -.->|"low inlier / degenerate"| GeometryFault
```

## [03]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or fold arm, never a sibling surface. Each `[RESULT]` cell names one return type: `Fin` over `GeometryFault` where a post-condition can fail, pure carriers for the projections.

| [INDEX] | [AXIS_CONCERN]    | [OWNER]          | [RESULT]                                  |
| :-----: | :---------------- | :--------------- | :---------------------------------------- |
|  [01]   | Primitive fit     | `Fit` + `FitOp`  | `Fit.Apply → Fin<Fitted>`                 |
|  [02]   | Fit kind          | `FitKind`        | `FitKind.Solve → Fin<FitPrimitive>`       |
|  [03]   | Fitted geometry   | `FitPrimitive`   | `FitPrimitive.Distance`/`Jacobian` (pure) |
|  [04]   | Consensus cost    | `ConsensusCost`  | `ConsensusCost.Cost` (pure, `ddouble`)    |
|  [05]   | Sampling mode     | `SampleMode`     | dispatch row (pure)                       |
|  [06]   | Orthogonal refine | `Fit.Model`      | `Lm.Minimize → Fin<LmResult>` (composed)  |

- [01]-[PRIMITIVE_FIT]: one static entry over one request record.
- [02]-[FIT_KIND]: `[SmartEnum<string>]` rows carrying arity and `Dof` columns, the declared draw lane, and the `Solve`/`Rebuild` behavior delegates.
- [03]-[FITTED_GEOMETRY]: `[Union]` with one generated-`Switch` fold per analytic behavior; the nested `JacobianRow` inline array is the chart-width carrier every `Jacobian` arm fills.
- [04]-[CONSENSUS_COST]: keyless `[SmartEnum]` row binding its per-point 106-bit `Cost` delegate.
- [05]-[SAMPLING_MODE]: keyless `[SmartEnum]` rows over one sampler reading a `Deterministic.Draw` lane — cost and sampling stay orthogonal.
- [06]-[ORTHOGONAL_REFINE]: private nested `ILmModel` instantiation — packed `JᵀJ`/`Jᵀr` scatter at 106-bit `Σd²`.

Every owner is pure-managed author-kernel composing the `Spatial/neighbors`, `Numerics/matrix`, and `Solving/solver` substrate; no live-host member beyond the stable native `Rhino.Geometry` value surface the `Numerics/atoms` substrate pins.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
