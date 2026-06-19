# [RASM_FABRICATION_SERIAL_CHAIN]

The serial-chain kinematics owner: `Fk` the Denavit-Hartenberg forward-kinematics homogeneous-matrix product over a `DhJoint` chain, and `Ik` the one damped-least-squares Jacobian-pseudoinverse inverse-kinematics solver that drives the chain to a target through kinematic singularities. The FK chain rides the `Rasm`/Vectors `Matrix` factory and `Multiply` rail — a hand-rolled 4×4 re-mint is the deleted form. The IK is the ONE damped-least-squares (Levenberg-Marquardt) pseudoinverse fold: the damping λ keeps the `JJᵀ + λ²I` system SPD and non-singular at singularities where a raw pseudoinverse diverges, so a per-arm analytic IK family is collapsed onto it (an analytic closed form survives only as a fast-path row, never a parallel solver). The kernel composes the `Process/owner#FABRICATION_OWNER` `Move`/`FabricationInput` shared vocabulary and is driven by the `Toolpath/motion#CAM_MOTION` `Cam` fold, which warm-starts each IK solve from the previous move's joint solution. It computes no hash and operates on raw coordinate doubles at the interior.

Wire posture: HOST-LOCAL. The joint-angle stream crosses only the in-process seam to the `Toolpath/motion#CAM_MOTION` `Motion` result — never a browser or peer wire. The `DhJoint`/`IkPolicy` records and the interior FK/IK state are host-local types that never sit between wire and rail.

## [01]-[INDEX]

- [01]-[SERIAL_CHAIN]: owns the `DhJoint`/`IkPolicy` records, the `Fk` Denavit-Hartenberg forward-kinematics matrix product, and the one damped-least-squares `Ik` Jacobian-pseudoinverse solver through singularities.

## [02]-[SERIAL_CHAIN]

- Owner: `DhJoint` the Denavit-Hartenberg link parameters (twist α, length a, offset d, angle θ) over one revolute/prismatic discriminant; `IkPolicy` the damped-least-squares solver knobs (damping λ, max iterations, position tolerance, finite-difference step, the reach-strict flag); `Fk` the forward-kinematics cumulative homogeneous-matrix product; `Ik` the damped Jacobian-pseudoinverse solver driving the chain to a target.
- Cases: the FK/IK chain is one solver over the `DhJoint` revolute/prismatic discriminant, never a per-arm kinematics class; the IK is one damped-least-squares fold, an analytic 2-link closed form surviving only as a fast-path row; `Ik.Solve` is total — it returns the stamped `(Theta, Residual, Reached)` triple for every input and never decides the reach contract, so the reach-strict-vs-permissive verdict is the caller's `IkPolicy.ReachStrict` policy read at `Toolpath/motion#CAM_MOTION`, never a fault thrown inside the solver.
- Entry: `public static Fin<Matrix> Fk.Of(DhJoint[] chain, double[] q)`, `public static Fin<Point3d> Fk.EndEffector(DhJoint[] chain, double[] q)`, and `public static (double[] Theta, double Residual, bool Reached) Ik.Solve(DhJoint[] chain, double[] seed, Point3d target, IkPolicy policy)` — `Fk` routes a degenerate-link `GeometryFault` through the `Matrix` `Fin` rail; `Ik` returns the joint solution, the final position residual, and the reached flag, a non-converged target stamping the residual rather than throwing.
- Auto: `Fk.Of` folds the `DhJoint` chain into one cumulative homogeneous `Matrix` product (the standard DH transform `Rot_z(θ)·Trans_z(d)·Trans_x(a)·Rot_x(α)` per link) over the settled `Rasm`/Vectors `Matrix`, so the end-effector pose is the chain product applied to the origin; `Ik.Solve` runs damped least squares — at each iteration it builds the 3×n position Jacobian by central finite-differencing the FK pose against each joint, solves `Δθ = Jᵀ(JJᵀ + λ²I)⁻¹ Δx` (the Levenberg-Marquardt-damped pseudoinverse stable through singularities), and steps the joints until the position error falls under the tolerance or the iteration cap routes a non-converged result with the residual stamped.
- Receipt: `Fk` returns the `Matrix` pose or the `Point3d` end-effector; `Ik` returns the joint solution, the position residual, and the reached flag — the typed kinematics evidence the CAM fold threads into the `Motion` joint stream; no generic kinematics ledger.
- Packages: `Rasm`/Vectors (`Matrix`/`Dimension`/`Point3d`/`Vector3d` — composed), LanguageExt.Core, BCL inbox.
- Growth: a 5-axis motion is one `DhJoint` orientation column plus one Jacobian row band (a 6×n pose Jacobian for full pose IK); an analytic 2-link fast-path is one row beside the damped solver; zero new surface.
- Boundary: the FK chain rides `Rasm`/Vectors `Matrix` and a hand-rolled 4×4 re-mint is the deleted form — `Matrix` exposes no `operator *` (multiplication is `Multiply → Fin<Matrix>`) and `Identity` takes a `Dimension` value object, never a bare `int`; the IK is the ONE damped-least-squares pseudoinverse fold and a per-arm analytic IK family is collapsed onto it; the translation column reads the public row-major `Matrix.Entries` (`Matrix` exposes no `double[,]` indexer, `At` is internal) at `row * Cols.Value + col`; the per-iteration `JJᵀ + λ²I` solve is the inline fixed-3×3 Cramer kernel (`Solve3`) on raw `double[]`, the `algorithms.md` tiny-system exemption — a 3×3 SPD system below the dense crossover where the per-iteration MathNet `DenseMatrix`/`Cholesky` allocation loses to the direct solve — and the in-place Jacobian and the joint-step loops are the named measured-kernel statement exemption, never a `Rasm.Compute` solver-lane round-trip.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Vectors;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Kinematics;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct DhJoint(double Alpha, double A, double D, double Theta, bool Revolute) {
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

public sealed record IkPolicy(double Damping, int MaxIterations, double PositionTolerance, double Step, bool ReachStrict) {
    public static readonly IkPolicy Canonical = new(Damping: 0.04, MaxIterations: 200, PositionTolerance: 1e-4, Step: 1e-6, ReachStrict: false);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Fk {
    public static Fin<Matrix> Of(DhJoint[] chain, double[] q) =>
        Enumerable.Range(0, chain.Length).Aggregate(
            Fin.Succ(Matrix.Identity(Dimension.Create(4))),
            (acc, i) => acc.Bind(m => chain[i].Transform(q[i]).Bind(link => m.Multiply(link))));

    public static Fin<Point3d> EndEffector(DhJoint[] chain, double[] q) =>
        Of(chain, q).Map(m => {
            int w = m.Cols.Value;
            return new Point3d(m.Entries[0 * w + 3], m.Entries[1 * w + 3], m.Entries[2 * w + 3]);
        });
}

public static class Ik {
    public static (double[] Theta, double Residual, bool Reached) Solve(DhJoint[] chain, double[] seed, Point3d target, IkPolicy policy) {
        int n = chain.Length;
        double[] theta = (double[])seed.Clone();
        for (int iter = 0; iter < policy.MaxIterations; iter++) {
            (Vector3d Dx, double Err, bool Ok) probe = Fk.EndEffector(chain, theta)
                .Match(c => { Vector3d d = target - c; return (d, d.Length, true); }, _ => (Vector3d.Zero, double.PositiveInfinity, false));
            if (!probe.Ok) return (theta, probe.Err, false);
            if (probe.Err < policy.PositionTolerance) return (theta, probe.Err, true);
            double[,] jac = Jacobian(chain, theta, policy.Step);
            double[] step = DampedStep(jac, new[] { probe.Dx.X, probe.Dx.Y, probe.Dx.Z }, policy.Damping, n);
            for (int j = 0; j < n; j++) theta[j] += step[j];
        }
        double residual = Fk.EndEffector(chain, theta).Match(c => (target - c).Length, _ => double.PositiveInfinity);
        return (theta, residual, false);
    }

    static double[,] Jacobian(DhJoint[] chain, double[] theta, double h) {
        var jac = new double[3, chain.Length];
        for (int j = 0; j < chain.Length; j++) {
            double[] plus = (double[])theta.Clone(); plus[j] += h;
            double[] minus = (double[])theta.Clone(); minus[j] -= h;
            _ = (Fk.EndEffector(chain, plus), Fk.EndEffector(chain, minus)).Apply((hi, lo) => {
                jac[0, j] = (hi.X - lo.X) / (2.0 * h); jac[1, j] = (hi.Y - lo.Y) / (2.0 * h); jac[2, j] = (hi.Z - lo.Z) / (2.0 * h);
                return unit;
            }).As();
        }
        return jac;
    }

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

    static double[] Solve3(double[,] m, double[] b) {
        double det =
            m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1])
            - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0])
            + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
        if (Math.Abs(det) < 1e-18) return new double[3];
        double x = (b[0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) - m[0, 1] * (b[1] * m[2, 2] - m[1, 2] * b[2]) + m[0, 2] * (b[1] * m[2, 1] - m[1, 1] * b[2])) / det;
        double y = (m[0, 0] * (b[1] * m[2, 2] - m[1, 2] * b[2]) - b[0] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) + m[0, 2] * (m[1, 0] * b[2] - b[1] * m[2, 0])) / det;
        double z = (m[0, 0] * (m[1, 1] * b[2] - b[1] * m[2, 1]) - m[0, 1] * (m[1, 0] * b[2] - b[1] * m[2, 0]) + b[0] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0])) / det;
        return new[] { x, y, z };
    }
}
```

## [03]-[RESEARCH]

- [IK_CONVERGENCE] The `Ik.Solve` damped-least-squares fold (Levenberg-Marquardt `Δθ = Jᵀ(JJᵀ + λ²I)⁻¹ Δx` with the central finite-difference position Jacobian and the inline 3×3 Cramer solve) is correct by construction: the damping λ guarantees the `JJᵀ + λ²I` system is SPD and non-singular through kinematic singularities, so the solve never divides by a vanishing pivot; a non-converged target returns the residual and a false reached flag, never a thrown divergence. The one numeric assumption is the finite-difference step `IkPolicy.Step` (1e-6), a stable central-difference scale for double-precision FK.
