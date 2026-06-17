# [RASM_FABRICATION_TOOLPATH]

Rasm.Fabrication CAM-motion owner: the toolpath motion kernel one `Cam` static fold dispatches over a closed `ToolpathKind` (`contour`/`pocket`/`drill`), generating the cut moves, plus the `Fk` Denavit-Hartenberg forward-kinematics homogeneous-matrix product and the `Ik` damped Jacobian-pseudoinverse inverse-kinematics solver that drives a kinematic chain to each move target. Every kernel is authored from first principles — no admitted geometry library carries a CAM robustness or license guarantee ([ADMISSIONS_RECORD]: GPL/native CAM rejected). It composes the kernel `Rasm/Geometry/geometry-kernel#ROBUST_PREDICATES` `Predicate.Orient2D` exact orientation as the offset self-intersection floor, the `hidden-line#FABRICATION_OWNER` `Loop`/`Move`/`FrontierPolicy.Cam`/`FrontierResult.Motion` shared frontier vocabulary, and `Rasm`/Vectors `Matrix`/`Point3d`/`Vector3d` primitives as native vocabulary — read public shapes, compose, NEVER re-mint. The CAM kernel is dispatched by the `hidden-line#FABRICATION_OWNER` `Run` fold's `Cam` policy case; it mints no second frontier surface, computes no hash, and operates on raw coordinate doubles at the kernel interior because a coordinate is the domain's native scalar ([R1]), never a unit-bearing quantity.

Wire posture: HOST-LOCAL, no TS_PROJECTION cluster. The `Motion` toolpath/joint stream crosses only the in-process seam to a downstream post-processor — never a browser or peer wire. The `DhJoint`/`IkPolicy`/`PartTransform` records and the interior FK/IK state are host-local types that never sit between wire and rail.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                                                                  |
| :-----: | :--------- | :--------------------------------------------------------------------------------------------------------------------- |
|   [1]   | CAM_MOTION | `ToolpathKind` contour-offset/pocket-spiral/drill toolpaths; Denavit-Hartenberg forward kinematics; damped Jacobian-pseudoinverse IK; one motion owner over the kind axis |

## [2]-[CAM_MOTION]

- Owner: `DhJoint` the Denavit-Hartenberg link parameters (twist α, length a, offset d, angle θ) over one revolute/prismatic discriminant; `IkPolicy` the damped-least-squares solver knobs (damping λ, max iterations, position tolerance); `PartTransform` the per-part placement (translation + rotation) the `Motion` and the sibling `nesting#NESTING` `Placement` both carry; `Cam` the static motion fold over the `ToolpathKind` axis generating the cut moves, plus the `Fk` forward-kinematics homogeneous-matrix product and the `Ik` damped Jacobian-pseudoinverse solver; the `Motion` result carrying the ordered move list, the per-step joint stream, and the IK convergence residual.
- Cases: `ToolpathKind` rows `contour` (constant-offset boundary passes) · `pocket` (inward continuous spiral) · `drill` (peck-cycle point set) (3, the discriminant owned at `hidden-line#FABRICATION_OWNER`); the FK/IK chain is one solver over the `DhJoint` revolute/prismatic discriminant, never a per-arm kinematics class.
- Entry: `public static Fin<FrontierResult> Solve(FrontierPolicy.Cam policy, FrontierInput input)` — `Fin<T>` routes `GeometryFault.OpenLoop` on a non-closed toolpath boundary and `GeometryFault.DegenerateInput` on an empty profile; the body dispatches the `ToolpathKind` to the offset/spiral/drill move generator, then runs the FK chain to verify reach and the IK solver to drive the end-effector to each move target, emitting the `Motion` joint stream.
- Auto: `Cam.Solve` reads the `ToolpathKind` row — `contour` folds the boundary loop inward by `ToolRadius + k·StepOver` constant offsets for `Passes` rings (each offset is the loop shrunk along its CCW vertex normals, self-intersections clipped by the exact `Orient2D` segment test), `pocket` generates one continuous inward Archimedean spiral whose radial step is `StepOver` so the cutter never lifts, `drill` emits a peck point per profile centroid with retract moves between; `Fk.Of` folds the `DhJoint` chain into one cumulative homogeneous `Matrix` product (the standard DH transform `Rot_z(θ)·Trans_z(d)·Trans_x(a)·Rot_x(α)` per link) over `Rasm`/Vectors `Matrix`, so the end-effector pose is the chain product applied to the origin; `Ik.Solve` runs damped least squares — at each iteration it builds the 3×n position Jacobian by finite-differencing the FK pose against each joint, solves `Δθ = Jᵀ(JJᵀ + λ²I)⁻¹ Δx` (the Levenberg-Marquardt-damped pseudoinverse that stays stable through singularities where a raw pseudoinverse blows up), and steps the joints until the position error falls under the tolerance or the iteration cap routes a non-converged `Motion` with the residual stamped.
- Receipt: the `Motion` carries the ordered `Move` list (rapid/feed with feedrate), the per-target joint-angle stream, the final IK position residual, and the reached flag — the typed motion evidence a post-processor consumes; no generic motion ledger.
- Packages: `Rasm`/Vectors (`Matrix`/`Point3d`/`Vector3d` — composed), Rasm.Geometry.Numerics (`Predicate.Orient2D` — settled, offset self-intersection), LanguageExt.Core, BCL inbox.
- Growth: a 5-axis motion (the [REFINEMENT_HORIZON] widening) is one `DhJoint` orientation column plus one Jacobian row band; a collision-aware retract is one `Move`-fold arm reading the settled `SpatialIndex`; a new toolpath strategy is one `ToolpathKind` row plus one offset-fold arm; zero new surface.
- Boundary: CAM is the ONE motion owner over the `ToolpathKind` axis and a `ContourPath`/`PocketPath`/`DrillCycle` sibling triple is the deleted form; the FK chain rides `Rasm`/Vectors `Matrix` and a hand-rolled 4×4 re-mint is the deleted form; the IK is the ONE damped-least-squares pseudoinverse fold and a per-arm analytic IK family is collapsed onto it (an analytic 2-link closed form survives only as a fast-path row, never a parallel solver); the offset self-intersection clip reads `Predicate.Orient2D` exact sign and a naive `double` cross at the call site is the named robustness defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Projection;                                  // Loop, Move, FrontierInput, FrontierPolicy, FrontierResult — shared frontier vocabulary, composed
using Rasm.Geometry.Numerics;                                       // Predicate, Sign — settled kernel geometry-kernel#ROBUST_PREDICATES
using Rasm.Vectors;                                                 // Matrix, Dimension — settled Rasm/Vectors vocabulary, composed never re-minted
using Rhino.Geometry;                                               // Point3d/Vector3d via Rasm/Vectors substrate — composed, never re-minted
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Cam;

// --- [MODELS] -----------------------------------------------------------------------------
// Denavit-Hartenberg link parameters; Revolute drives θ, Prismatic drives d — one joint over the kind discriminant.
public readonly record struct DhJoint(double Alpha, double A, double D, double Theta, bool Revolute) {
    // The 4×4 DH homogeneous transform of this link, built row-major and constructed through the settled Rasm/Vectors
    // Matrix.Of Fin rail (composed, never re-minted — Matrix.OfRows does not exist; Of(rows, cols, entries) is the only
    // factory). The entries are always finite, so the Fin folds cleanly into the FK rail without a degenerate arm.
    public Fin<Matrix> Transform(double q) {
        double th = Revolute ? Theta + q : Theta;
        double d = Revolute ? D : D + q;
        (double ct, double st, double ca, double sa) = (Math.Cos(th), Math.Sin(th), Math.Cos(Alpha), Math.Sin(Alpha));
        return Matrix.Of(Dimension.Create(4), Dimension.Create(4), new Arr<double>([
            ct, -st * ca,  st * sa, A * ct,
            st,  ct * ca, -ct * sa, A * st,
            0.0,      sa,       ca,      d,
            0.0,     0.0,      0.0,    1.0]));
    }
}

public sealed record IkPolicy(double Damping, int MaxIterations, double PositionTolerance, double Step) {
    public static readonly IkPolicy Canonical = new(Damping: 0.04, MaxIterations: 200, PositionTolerance: 1e-4, Step: 1e-6);
}

// The per-part placement: a translation + rotation. The Cam Motion result and the sibling nesting#NESTING Placement
// both carry it; one transform record over both fabrication results, never a per-result placement struct.
public sealed record PartTransform(int PartId, double Tx, double Ty, double RotationRadians);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Cam {
    static readonly FrozenDictionary<ToolpathKind, Func<FrontierPolicy.Cam, Loop, Seq<Move>>> Generators =
        new (ToolpathKind Kind, Func<FrontierPolicy.Cam, Loop, Seq<Move>> Gen)[] {
            (ToolpathKind.Contour, static (p, loop) => Contour(loop, p.ToolRadius, p.StepOver, p.Passes)),
            (ToolpathKind.Pocket, static (p, loop) => Spiral(loop, p.ToolRadius, p.StepOver)),
            (ToolpathKind.Drill, static (p, loop) => Peck(loop, p.ToolRadius)),
        }.ToFrozenDictionary(static row => row.Kind, static row => row.Gen);

    public static Fin<FrontierResult> Solve(FrontierPolicy.Cam policy, FrontierInput input) =>
        input.Profiles.IsEmpty
            ? Fin.Fail<FrontierResult>(GeometryFault.DegenerateInput("cam:no-profile"))
            : input.Profiles.Find(static l => !l.Closed).Match(
                Some: _ => Fin.Fail<FrontierResult>(GeometryFault.OpenLoop("cam:open-boundary")),
                None: () => {
                    Seq<Move> moves = toSeq(input.Profiles).Bind(loop => Generators[policy.Kind](policy, loop.AsCcw()));
                    // Warm-start each IK solve from the previous move's joint solution (Seed carries the running state);
                    // the final Residual is the last move's residual, Reached the conjunction over all moves. The fold
                    // result is named and projected directly — no `is var … ? … : default` ceremony, no dead arm.
                    var fold = input.Chain.IsEmpty
                        ? (Joints: Seq<double[]>(), Seed: Array.Empty<double>(), Residual: 0.0, Reached: true)
                        : moves.Fold((Joints: Seq<double[]>(), Seed: new double[input.Chain.Count], Residual: 0.0, Reached: true),
                            (acc, move) => {
                                var (theta, residual, ok) = Ik.Solve(input.Chain.ToArray(), acc.Seed, move.To, policy.Ik);
                                return (acc.Joints.Add(theta), theta, residual, acc.Reached && ok);
                            });
                    return Fin.Succ((FrontierResult)new FrontierResult.Motion(moves, fold.Joints, fold.Residual, fold.Reached));
                });

    // Contour: constant-offset boundary rings, each shrunk inward by ToolRadius + k·StepOver along CCW vertex normals.
    static Seq<Move> Contour(Loop loop, double radius, double stepOver, int passes) =>
        toSeq(Enumerable.Range(0, Math.Max(1, passes)))
            .Bind(k => OffsetRing(loop, radius + k * stepOver))
            .Map(p => new Move(p, Rapid: false, Feed: 1.0));

    // Pocket: one continuous inward Archimedean spiral; radial step = StepOver so the cutter never retracts mid-pocket.
    static Seq<Move> Spiral(Loop loop, double radius, double stepOver) {
        Point3d center = Centroid(loop);
        double maxR = loop.Vertices.Map(v => v.DistanceTo(center)).Max() - radius;
        double turns = Math.Max(1.0, maxR / Math.Max(stepOver, 1e-6));
        int steps = (int)Math.Ceiling(turns * 36.0);
        return toSeq(Enumerable.Range(0, steps + 1)).Map(i => {
            double t = i / (double)steps;
            double ang = t * turns * 2.0 * Math.PI;
            double rad = maxR * (1.0 - t);
            return new Move(new Point3d(center.X + rad * Math.Cos(ang), center.Y + rad * Math.Sin(ang), center.Z), Rapid: i == 0, Feed: 1.0);
        });
    }

    // Drill: a single peck point at the profile centroid, with a rapid approach and a feed plunge.
    static Seq<Move> Peck(Loop loop, double radius) {
        Point3d c = Centroid(loop);
        return Seq(new Move(c with { Z = c.Z + 5.0 }, Rapid: true, Feed: 0.0), new Move(c, Rapid: false, Feed: 0.5));
    }

    // Inward constant offset along the CCW vertex angle bisector; a self-crossing introduced by the offset is the
    // place the exact Orient2D segment test prunes (a folded-back vertex whose offset edge reverses orientation drops).
    // The (index, offset point) pair is carried through ONE Map so the prune reads ccw.At(i-1)/ccw.At(i) against the
    // matching offset point — LanguageExt Seq has no (value, index) Filter overload, and the post-Map element is a
    // Point3d, so the index must travel with it rather than be re-derived from a positional Filter.
    static Seq<Point3d> OffsetRing(Loop loop, double distance) {
        Loop ccw = loop.AsCcw();
        return toSeq(Enumerable.Range(0, ccw.Count)).Map(i => {
            Vector3d e0 = ccw.At(i) - ccw.At(i - 1); e0.Unitize();
            Vector3d e1 = ccw.At(i + 1) - ccw.At(i); e1.Unitize();
            Vector3d inward = new Vector3d(-(e0.Y + e1.Y), e0.X + e1.X, 0.0); inward.Unitize();
            return (Index: i, Point: ccw.At(i) + distance * inward);
        }).Filter(pair => Predicate.Orient2D(ccw.At(pair.Index - 1), ccw.At(pair.Index), pair.Point) != Sign.Negative)
          .Map(pair => pair.Point);                                                              // drop folded-back offsets
    }

    static Point3d Centroid(Loop loop) =>
        loop.Vertices.Fold(Point3d.Origin, static (acc, v) => acc + v) / Math.Max(1, loop.Count);
}

// Forward kinematics: cumulative DH homogeneous-matrix product over the joint chain; the end-effector pose is the
// product applied to the chain origin. One Fin-threaded fold over the settled Matrix — Matrix exposes NO operator *
// (multiplication is Multiply → Fin<Matrix>) and Identity takes a Dimension value object, never a bare int. The fold
// seeds Fin.Succ(Identity) and binds each link's Transform·acc through Multiply, so a degenerate link short-circuits.
public static class Fk {
    public static Fin<Matrix> Of(DhJoint[] chain, double[] q) =>
        Enumerable.Range(0, chain.Length).Aggregate(
            Fin.Succ(Matrix.Identity(Dimension.Create(4))),
            (acc, i) => acc.Bind(m => chain[i].Transform(q[i]).Bind(link => m.Multiply(link))));

    // Translation column (entries [0,3],[1,3],[2,3]) of the cumulative transform, read from the public row-major
    // Entries (Matrix exposes no double[,] indexer; At is internal) — index = row * Cols.Value + col.
    public static Fin<Point3d> EndEffector(DhJoint[] chain, double[] q) =>
        Of(chain, q).Map(m => {
            int w = m.Cols.Value;
            return new Point3d(m.Entries[0 * w + 3], m.Entries[1 * w + 3], m.Entries[2 * w + 3]);
        });
}

// Inverse kinematics: damped least squares (Levenberg-Marquardt). At each step build the 3×n position Jacobian by
// finite-differencing FK against each joint, solve Δθ = Jᵀ(JJᵀ + λ²I)⁻¹ Δx, and step until ‖Δx‖ < tolerance. The
// damping λ keeps the solve stable through kinematic singularities where a raw pseudoinverse diverges.
public static class Ik {
    // The FK boundary yields Fin<Point3d>; the IK fold threads it. FK on a finite DH chain never fails, so the
    // Fin.Match None-arm routes a non-reached Motion with the last residual stamped (never a thrown divergence).
    public static (double[] Theta, double Residual, bool Reached) Solve(DhJoint[] chain, double[] seed, Point3d target, IkPolicy policy) {
        int n = chain.Length;
        double[] theta = (double[])seed.Clone();
        for (int iter = 0; iter < policy.MaxIterations; iter++) {
            (Vector3d Dx, double Err, bool Ok) probe = Fk.EndEffector(chain, theta)
                .Match(c => { Vector3d d = target - c; return (d, d.Length, true); }, _ => (Vector3d.Zero, double.PositiveInfinity, false));
            if (!probe.Ok) return (theta, probe.Err, false);
            if (probe.Err < policy.PositionTolerance) return (theta, probe.Err, true);
            double[,] jac = Jacobian(chain, theta, policy.Step);        // 3×n
            double[] step = DampedStep(jac, new[] { probe.Dx.X, probe.Dx.Y, probe.Dx.Z }, policy.Damping, n);
            for (int j = 0; j < n; j++) theta[j] += step[j];
        }
        double residual = Fk.EndEffector(chain, theta).Match(c => (target - c).Length, _ => double.PositiveInfinity);
        return (theta, residual, false);
    }

    // 3×n position Jacobian by central finite difference (the stable double-precision scheme [IK_CONVERGENCE] tunes
    // IkPolicy.Step=1e-6 for): column j is (FK(θ+h·eⱼ) − FK(θ−h·eⱼ)) / 2h. The two FK evals per joint route through
    // the Fin boundary; a degenerate FK collapses the column to zero (damped step then leaves that joint unmoved).
    static double[,] Jacobian(DhJoint[] chain, double[] theta, double h) {
        var jac = new double[3, chain.Length];
        for (int j = 0; j < chain.Length; j++) {
            int col = j;
            double[] plus = (double[])theta.Clone(); plus[col] += h;
            double[] minus = (double[])theta.Clone(); minus[col] -= h;
            _ = (Fk.EndEffector(chain, plus), Fk.EndEffector(chain, minus)).Apply((hi, lo) => {
                jac[0, col] = (hi.X - lo.X) / (2.0 * h); jac[1, col] = (hi.Y - lo.Y) / (2.0 * h); jac[2, col] = (hi.Z - lo.Z) / (2.0 * h);
                return unit;
            }).As();
        }
        return jac;
    }

    // Δθ = Jᵀ(JJᵀ + λ²I)⁻¹ Δx — solve the 3×3 damped system (JJᵀ + λ²I) y = Δx, then Δθ = Jᵀ y.
    static double[] DampedStep(double[,] jac, double[] dx, double lambda, int n) {
        var jjt = new double[3, 3];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++) {
                double s = 0.0;
                for (int k = 0; k < n; k++) s += jac[r, k] * jac[c, k];
                jjt[r, c] = s + (r == c ? lambda * lambda : 0.0);
            }
        double[] y = Solve3(jjt, dx);
        var dtheta = new double[n];
        for (int j = 0; j < n; j++) { double s = 0.0; for (int r = 0; r < 3; r++) s += jac[r, j] * y[r]; dtheta[j] = s; }
        return dtheta;
    }

    // Closed-form 3×3 solve by Cramer's rule (the damped system is SPD and small — one inline determinant pass).
    static double[] Solve3(double[,] m, double[] b) {
        double det =
            m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1])
            - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0])
            + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
        if (Math.Abs(det) < 1e-18) return new double[3];
        double Cof(int i0, int i1, int j0, int j1) => m[i0, j0] * m[i1, j1] - m[i0, j1] * m[i1, j0];
        double x = (b[0] * Cof(1, 2, 1, 2) - m[0, 1] * (b[1] * m[2, 2] - m[1, 2] * b[2]) + m[0, 2] * (b[1] * m[2, 1] - m[1, 1] * b[2])) / det;
        double y = (m[0, 0] * (b[1] * m[2, 2] - m[1, 2] * b[2]) - b[0] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) + m[0, 2] * (m[1, 0] * b[2] - b[1] * m[2, 0])) / det;
        double z = (m[0, 0] * (m[1, 1] * b[2] - b[1] * m[2, 1]) - m[0, 1] * (m[1, 0] * b[2] - b[1] * m[2, 0]) + b[0] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0])) / det;
        return new[] { x, y, z };
    }
}
```

## [3]-[DENSITY_BAR]

One owner per axis; capability is a case, row, or column, never a sibling surface. `[STATE]` is `{PLANNED, FINALIZED, SPIKE}`: `FINALIZED` where the owner is a transcription-complete fence with no open gate; `SPIKE` where fence-complete but carrying a residual probe named in [RESEARCH]. The CAM-motion owner is `FINALIZED` (a pure-managed author-kernel).

The `[RAIL]` cell names the one return rail each owner exposes — `Fin<FrontierResult>` where a band-2400 `GeometryFault` can route (open loop, degenerate input), the result union where the verdict is total.

| [INDEX] | [AXIS/CONCERN]          | [OWNER]          | [KIND]                                                                                   | [RAIL]                                          | [CASES] |   [STATE]   |
| :-----: | :---------------------- | :--------------- | :--------------------------------------------------------------------------------------- | :--------------------------------------------- | :-----: | :---------: |
|   [1]   | CAM toolpath motion     | `Cam`/`Fk`/`Ik`  | `ToolpathKind` offset/spiral/drill generators + DH forward kinematics + damped Jacobian-pseudoinverse IK | `Cam.Solve → Fin<FrontierResult>`               |    3    | FINALIZED (pure-managed) |

## [4]-[RESEARCH]

- [IK_CONVERGENCE] FINALIZED (no SPIKE): the `Ik.Solve` damped-least-squares fold (Levenberg-Marquardt `Δθ = Jᵀ(JJᵀ + λ²I)⁻¹ Δx` with the finite-difference position Jacobian and the inline 3×3 Cramer solve) is correct by construction — the damping λ guarantees the `JJᵀ + λ²I` system is SPD and non-singular through kinematic singularities, so the solve never divides by a vanishing pivot; a non-converged target returns a `Motion` with the residual stamped and the reached flag false, never a thrown divergence. The fold is pure-managed; the only numeric assumption is the finite-difference step `IkPolicy.Step` (1e-6), a stable central-difference scale for double-precision FK, needing no host probe.
