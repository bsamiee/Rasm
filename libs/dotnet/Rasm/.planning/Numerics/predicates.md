# [RASM_NUMERICS_PREDICATES]

`Predicate` owns the adaptive-precision exact-sign floor every higher geometry owner composes, returning a total `Sign` for explicit and constructed points alike. Constructed intersection points travel as defining-point carriage with exact coordinates derived on demand, so no rounded coordinate enters a predicate and rounding happens once at the consumer's emission boundary.

Every constructed-point fold is one polynomial instantiated at both the `Interval` filter and `Dyadic` exact carriers through `IExact<TSelf>`, so the filter never tests a different polynomial than the exact branch decides; direct predicates retain precision-specific `double`, `ddouble`, and `Dyadic` kernels. Degeneracy is a verdict of that algebra and never of a rounded readout: `Sign.Zero` from the exact fold, `Dyadic.Sign` at the emission boundary, and a refusal on the typed fault channel where the family's one float-typed admission cannot pick a plane at all — no branch on this page reads a floating result's finiteness or magnitude to decide whether a configuration is degenerate (scar `EXACT_ORACLE_INFERRED_FROM_RESULT`).

## [01]-[INDEX]

- [02]-[ROBUST_PREDICATES]: `Predicate` folds the direct ladders and the constructed-point family to a total `Sign`, and clips one convex ring against one `HalfPlane` on that same exact side test.
- [03]-[INTERIOR_NUMERICS]: `IExact` carriers stack the `Interval` filter and the exact `Dyadic` tier over `EFloat` arithmetic and `Predicate`'s private generic `ErrorBound` filter rows.

## [02]-[ROBUST_PREDICATES]

- Owner: `Sign` `[SmartEnum<int>]` is the closed ternary verdict every predicate returns, carrying the `Times` parity algebra; `Axis` `[SmartEnum<int>]` is the closed coordinate vocabulary and the ONE generator every axis-projected member spans its three planes over, its `U`/`V` perpendicular projections dispatched exhaustively over its own rows and its `Read`/`Along`/`Pick` coordinate columns replacing every ordinal a consumer once re-resolved; `ImplicitPoint` `[Union<Point3d, ImplicitPoint.SegmentIntersection, ImplicitPoint.LinePlaneIntersection, ImplicitPoint.ThreePlaneIntersection>]` carries a constructed point as DEFINING POINTS ONLY, its exact homogeneous coordinates derived on demand through `Homogeneous<T>`; `HalfPlane` `[Union]` owns both the exact side evaluation and the convex-ring `Clip`, its `DirectedLine` case reading the exact `Orient2D` ladder and its `Affine` case the functional a caller already holds; `Predicate` is the ONE static surface owning the direct ladders and implicit folds.
- Cases: `Sign`, `Axis`, and the four `ImplicitPoint` constructions are the closed vocabularies; `Predicate` carries the four direct members `Orient2D`/`Orient3D`/`InCircle`/`InSphere` beside `Orient2D(in ImplicitPoint, in ImplicitPoint, in ImplicitPoint, Axis)` and `Orient3D(in ImplicitPoint, in ImplicitPoint, in ImplicitPoint, in ImplicitPoint)` spanning every explicit/implicit combination × projection plane, `Compare(in ImplicitPoint, in ImplicitPoint, Axis)` the exact per-coordinate order key, and the in-circum `InCircle`/`InSphere` implicit queries; `Axis` carries `Read`/`Along` for points and vectors, `Pick` for the exact homogeneous quadruple, and the `Basis` lift column.
- Entry: every VERDICT member is a total pure exact function returning `Sign` with no gate; the two non-verdict members state their own type — `Axis.DominantOf` its plane-selection refusal and `HalfPlane.Clip` its span-arity refusal — and nothing else on the family carries one. Consumers enter the direct ladders through `Point3d`; the raw-`double` scalar kernels remain private precision details, and every refine and exact tier takes those raw ordinates without rebuilding a host value. Implicit-point entries discriminate on the carrier's case shape and the `Axis` row; the ad-hoc union's generated implicit conversion absorbs a bare `Point3d` at every implicit entry, so a consumer never spells the explicit case. Degenerate constructions (`lambda = 0`) yield `Sign.Zero` through the `Times` parity algebra, the degeneracy witness the consumer's recovery reads. `Axis.DominantOf` is the family's one vector admission.
- Auto: each direct member filters in `double`, refines at 106-bit `ddouble`, then folds the sign-exact `Dyadic`; each implicit member opens at the `Interval` directed-rounding filter over the SAME polynomial — a rounded-coordinate `double` filter cannot exist for a point whose coordinates are derived, so there is no cheaper tier below it — escalating the indeterminate residue to exact context-free `EFloat` arithmetic. Direct members retain lazy escalation through `Option<Sign>.IfNone`; implicit members allocate the exact dyadic tier only when the bracket is indeterminate; every tier is monotone and sign-consistent, so the verdict is always the true sign.
- Law: `Axis.DominantOf` SELECTS a projection plane and never decides degeneracy — its float cross-product normal is a heuristic barred from every exact carrier, and an invalid or zero normal REFUSES onto the typed fault channel rather than silently taking a max component a NaN wins. `ImplicitPoint.Round()` decides existence by `Dyadic.Sign` on the defining points, so a construction with no point returns `None`; absence never crosses the boundary as a non-finite `Point3d` sentinel for a consumer's freeze gate to catch.
- Law: a `Sign` verdict carries no residual. Two emission-side materializations exist and both are evidence-bearing: `HalfPlane.Clip` writes clipped coordinates beside a per-vertex MIDPOINT-FALLBACK mark, so the midpoint standing in for a crossing whose denominator fell under the floor is never mistaken for a measured one, and `ImplicitPoint.Round()`, whose `Option<Point3d>` a consumer emits at its own boundary and never a value any predicate reads back; `None` folds the two causes a consumer answers identically — the exact non-existence the lambda sign proves and an over-range double readout at the rounding step — the exact cause being the one the predicate owner already reports as `Sign.Zero`.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`), RhinoCommon (`Point3d`, `Vector3d.CrossProduct`), TYoshimura.DoubleDouble (106-bit refine), PeterO.Numbers (directed interval and exact dyadic tiers), Rasm.Domain (`KernelFault.InvalidInput`, the `DominantOf` refusal channel), BCL inbox (`INumber<T>` generic filter).
- Growth: a new implicit-point construction is one `ImplicitPoint` case carrying its defining points and one `Homogeneous<T>` arm, every fold and emission member widening by that arm with the generated dispatch breaking loudly; a new direct predicate is one member and one `ErrorBound` row; a new cut shape is one `HalfPlane` case and one local `Evaluate` arm inside `Clip`; a new precision stage is one `IExact` carrier with one lazy escalation link in each member tail. The multi-implicit in-circum widening is ONE derivation away and states it here: scaling homogeneous row i by `(la*li)^2` makes every entry an exact polynomial and the determinant scale an EVEN power, so the verdict composes each lambda's sign twice and a zero one gates to Zero — landing it is one `*Numerator` widening per member with no new surface, held only until a differential recomputed from original inputs in the test assembly proves the parity.
- Boundary: the whole family lives on ONE `Predicate` static owner — a per-predicate class or a `FastOrient2D`/`ExactOrient2D` pair is the deleted form. Verdicts are the closed `Sign` and a raw `int`/`double` sign crossing a public signature is the named defect; coordinates are `Point3d` read at the boundary, a domain-local point struct the deleted form. Constructed points travel as `ImplicitPoint` defining-point carriage rounded ONCE at `Round()` — a `Denominator`-as-`double` field or separately rounded numerator and denominator inside an exact carrier is the named robustness defect; `HalfPlane.Clip`'s emitted crossings are the one deliberate exception, a ring fold whose product IS coordinates, and they carry their midpoint-fallback mark precisely because no exact carriage survives the divide — and derived `Plane` inputs are dead, so a three-plane point is its NINE points. `DominantOf` is the ONE geometry admission and exact `EFloat` arithmetic imposes no operand-magnitude ceiling; every leaf difference rides exact `IExact.Diff`, a raw `double` subtraction wrapped in an exact type the deleted rounded-leaf form. Loosening a filter band to pass a near-degenerate case instead of taking the exact branch is the named correctness defect — a sign verdict is exact or it is a defect.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Numerics;
using DoubleDouble;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Sign {
    public static readonly Sign Negative = new(-1);
    public static readonly Sign Zero = new(0);
    public static readonly Sign Positive = new(1);

    public static Sign Of(double value) => value < 0.0 ? Negative : value > 0.0 ? Positive : Zero;

    public Sign Times(Sign other) => Of(Key * other.Key);
}

[SmartEnum<int>]
public sealed partial class Axis {
    public static readonly Axis X = new(0, basis: Vector3d.XAxis, read: static p => p.X, along: static d => d.X);
    public static readonly Axis Y = new(1, basis: Vector3d.YAxis, read: static p => p.Y, along: static d => d.Y);
    public static readonly Axis Z = new(2, basis: Vector3d.ZAxis, read: static p => p.Z, along: static d => d.Z);

    public Vector3d Basis { get; }

    public Axis U => Switch(x: static _ => Y, y: static _ => Z, z: static _ => X);
    public Axis V => Switch(x: static _ => Z, y: static _ => X, z: static _ => Y);
    [UseDelegateFromConstructor] public partial double Read(Point3d p);
    [UseDelegateFromConstructor] public partial double Along(Vector3d d);

    internal T Pick<T>(in (T X, T Y, T Z, T Lambda) h) where T : struct, IExact<T> =>
        Switch(state: h,
            x: static (value, _) => value.X,
            y: static (value, _) => value.Y,
            z: static (value, _) => value.Z);

    public static Fin<Axis> DominantOf(Vector3d d) {
        (double x, double y, double z) = (Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z));
        return d.IsValid && !d.IsZero
            ? Fin.Succ(x >= y && x >= z ? X : y >= z ? Y : Z)
            : Fin.Fail<Axis>(new KernelFault.InvalidInput());
    }

}

[Union<Point3d, ImplicitPoint.SegmentIntersection, ImplicitPoint.LinePlaneIntersection, ImplicitPoint.ThreePlaneIntersection>(
    T1Name = "Explicit", T2Name = "SegmentIntersection", T3Name = "LinePlaneIntersection", T4Name = "ThreePlaneIntersection")]
public readonly partial struct ImplicitPoint {
    internal (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> =>
        Switch(
            @explicit: static e => (T.Of(e.X), T.Of(e.Y), T.Of(e.Z), T.Of(1.0)),
            segmentIntersection:    static point => point.Homogeneous<T>(),
            linePlaneIntersection:  static point => point.Homogeneous<T>(),
            threePlaneIntersection: static point => point.Homogeneous<T>());

    public Option<Point3d> Round() {
        if (IsExplicit) return AsExplicit.IsValid ? Some(AsExplicit) : None;
        (Dyadic x, Dyadic y, Dyadic z, Dyadic lambda) = Homogeneous<Dyadic>();
        if (lambda.Sign == Sign.Zero) return None;
        Point3d rounded = new(x.Quotient(lambda), y.Quotient(lambda), z.Quotient(lambda));
        return rounded.IsValid ? Some(rounded) : None;
    }

    public readonly record struct SegmentIntersection(
        Point3d FirstStart, Point3d FirstEnd, Point3d SecondStart, Point3d SecondEnd, Axis Projection) {
        internal (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
            (Axis u, Axis v) = (Projection.U, Projection.V);
            T lambda = T.Diff(u.Read(SecondEnd), u.Read(SecondStart)).Mul(T.Diff(v.Read(FirstEnd), v.Read(FirstStart)))
                .Sub(T.Diff(v.Read(SecondEnd), v.Read(SecondStart)).Mul(T.Diff(u.Read(FirstEnd), u.Read(FirstStart))));
            T n = T.Diff(u.Read(SecondEnd), u.Read(FirstStart)).Mul(T.Diff(v.Read(FirstEnd), v.Read(FirstStart)))
                .Sub(T.Diff(v.Read(SecondEnd), v.Read(FirstStart)).Mul(T.Diff(u.Read(FirstEnd), u.Read(FirstStart))));
            return (
                Parametric(lambda, n, FirstStart.X, FirstEnd.X),
                Parametric(lambda, n, FirstStart.Y, FirstEnd.Y),
                Parametric(lambda, n, FirstStart.Z, FirstEnd.Z),
                lambda);

            static T Parametric(T lambda, T n, double at, double head) =>
                lambda.Scale(at).Add(n.Mul(T.Diff(head, at)));
        }
    }

    public readonly record struct LinePlaneIntersection(
        Point3d LineStart, Point3d LineEnd, Point3d PlaneA, Point3d PlaneB, Point3d PlaneC) {
        internal (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
            (T ax, T ay, T az) = (T.Diff(LineStart.X, LineEnd.X), T.Diff(LineStart.Y, LineEnd.Y), T.Diff(LineStart.Z, LineEnd.Z));
            (T bx, T by, T bz) = (T.Diff(PlaneB.X, PlaneA.X), T.Diff(PlaneB.Y, PlaneA.Y), T.Diff(PlaneB.Z, PlaneA.Z));
            (T cx, T cy, T cz) = (T.Diff(PlaneC.X, PlaneA.X), T.Diff(PlaneC.Y, PlaneA.Y), T.Diff(PlaneC.Z, PlaneA.Z));
            (T m1, T m2, T m3) = (by.Mul(cz).Sub(bz.Mul(cy)), bx.Mul(cz).Sub(bz.Mul(cx)), bx.Mul(cy).Sub(by.Mul(cx)));
            T lambda = ax.Mul(m1).Sub(ay.Mul(m2)).Add(az.Mul(m3));
            T n = T.Diff(LineStart.X, PlaneA.X).Mul(m1).Sub(T.Diff(LineStart.Y, PlaneA.Y).Mul(m2)).Add(T.Diff(LineStart.Z, PlaneA.Z).Mul(m3));
            return (lambda.Scale(LineStart.X).Sub(ax.Mul(n)), lambda.Scale(LineStart.Y).Sub(ay.Mul(n)), lambda.Scale(LineStart.Z).Sub(az.Mul(n)), lambda);
        }
    }

    public readonly record struct ThreePlaneIntersection(
        Point3d FirstA, Point3d FirstB, Point3d FirstC,
        Point3d SecondA, Point3d SecondB, Point3d SecondC,
        Point3d ThirdA, Point3d ThirdB, Point3d ThirdC) {
        internal (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
            ((T, T, T) np, T dp) = PlaneRow(FirstA, FirstB, FirstC);
            ((T, T, T) nq, T dq) = PlaneRow(SecondA, SecondB, SecondC);
            ((T, T, T) nr, T dr) = PlaneRow(ThirdA, ThirdB, ThirdC);
            return (
                Predicate.Determinant3x3((dp, np.Item2, np.Item3), (dq, nq.Item2, nq.Item3), (dr, nr.Item2, nr.Item3)),
                Predicate.Determinant3x3((np.Item1, dp, np.Item3), (nq.Item1, dq, nq.Item3), (nr.Item1, dr, nr.Item3)),
                Predicate.Determinant3x3((np.Item1, np.Item2, dp), (nq.Item1, nq.Item2, dq), (nr.Item1, nr.Item2, dr)),
                Predicate.Determinant3x3(np, nq, nr));

            static ((T X, T Y, T Z) Normal, T Offset) PlaneRow(Point3d a, Point3d b, Point3d c) {
                (T ux, T uy, T uz) = (T.Diff(b.X, a.X), T.Diff(b.Y, a.Y), T.Diff(b.Z, a.Z));
                (T vx, T vy, T vz) = (T.Diff(c.X, a.X), T.Diff(c.Y, a.Y), T.Diff(c.Z, a.Z));
                (T nx, T ny, T nz) = (uy.Mul(vz).Sub(uz.Mul(vy)), uz.Mul(vx).Sub(ux.Mul(vz)), ux.Mul(vy).Sub(uy.Mul(vx)));
                return ((nx, ny, nz), nx.Scale(a.X).Add(ny.Scale(a.Y)).Add(nz.Scale(a.Z)));
            }
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HalfPlane {
    private HalfPlane() { }

    public sealed record DirectedLine(Point3d Start, Point3d End, Axis Projection) : HalfPlane;
    public sealed record Affine(Vector3d Normal, double Constant) : HalfPlane;

    public Fin<int> Clip(
        ReadOnlySpan<Point3d> ring, ReadOnlySpan<int> labels, Point3d interior, double band, double denominatorFloor,
        int cutLabel, Span<Point3d> target, Span<int> targetLabels, Span<bool> targetMidpointFallback) {
        int room = ring.Length + 1;
        if (ring.Length < 3 || labels.Length < ring.Length
            || target.Length < room || targetLabels.Length < room || targetMidpointFallback.Length < room) {
            return Fin.Fail<int>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "clip ring or target span too short"));
        }
        Sign keep = Evaluate(this, interior).Side;
        int written = 0;
        Point3d prev = ring[^1];
        (Sign sidePrev, double offPrev) = Evaluate(this, prev);
        int labelPrev = labels[ring.Length - 1];
        for (int k = 0; k < ring.Length; k++) {
            Point3d cur = ring[k];
            (Sign sideCur, double offCur) = Evaluate(this, cur);
            if (sidePrev.Times(sideCur) == Sign.Negative) {
                double denominator = offPrev - offCur;
                bool midpointFallback = Math.Abs(denominator) < denominatorFloor;
                double t = midpointFallback ? 0.5 : offPrev / denominator;
                (targetLabels[written], targetMidpointFallback[written]) = (sidePrev == keep ? cutLabel : labelPrev, midpointFallback);
                target[written++] = prev + (t * (cur - prev));
            }
            if (sideCur != keep.Times(Sign.Negative) || Math.Abs(offCur) <= band) {
                (targetLabels[written], targetMidpointFallback[written]) = (labels[k], false);
                target[written++] = cur;
            }
            (prev, sidePrev, offPrev, labelPrev) = (cur, sideCur, offCur, labels[k]);
        }
        return Fin.Succ(written);

        static (Sign Side, double Offset) Evaluate(HalfPlane cut, Point3d point) =>
            cut.Switch(
                state: point,
                directedLine: static (q, line) => {
                    (Axis u, Axis v) = (line.Projection.U, line.Projection.V);
                    (double originU, double originV) = (u.Read(line.Start), v.Read(line.Start));
                    (double alongU, double alongV) = (u.Read(line.End), v.Read(line.End));
                    (double pointU, double pointV) = (u.Read(q), v.Read(q));
                    Sign side = Predicate.Orient2D(line.Start, line.End, q, line.Projection);
                    double offset = ((originU - pointU) * (alongV - pointV)) - ((originV - pointV) * (alongU - pointU));
                    return (side, offset);
                },
                affine: static (q, affine) => {
                    double offset = (affine.Normal * (Vector3d)q) - affine.Constant;
                    Dyadic exact = Dyadic.Of(affine.Normal.X).Mul(Dyadic.Of(q.X))
                        .Add(Dyadic.Of(affine.Normal.Y).Mul(Dyadic.Of(q.Y)))
                        .Add(Dyadic.Of(affine.Normal.Z).Mul(Dyadic.Of(q.Z)))
                        .Sub(Dyadic.Of(affine.Constant));
                    return (exact.Sign, offset);
                });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Predicate {
    // --- [ERROR_BOUND]
    private readonly struct ErrorBound(double alpha, double beta) {
        const double DoubleRoundoff = 1.0 / (1L << 53);
        const double DoubleDoubleRoundoff = DoubleRoundoff * DoubleRoundoff;

        public static readonly ErrorBound Orient2D = new(3.0, 16.0);
        public static readonly ErrorBound Orient3D = new(7.0, 56.0);
        public static readonly ErrorBound InCircle = new(10.0, 96.0);
        public static readonly ErrorBound InSphere = new(16.0, 224.0);

        public Option<Sign> Filter<T>(T determinant, T permanent, T roundoff) where T : struct, INumber<T> =>
            T.Abs(determinant) > (T.CreateChecked(alpha) + T.CreateChecked(beta) * roundoff) * roundoff * permanent
                ? Some(Sign.Of(T.Sign(determinant)))
                : None;
    }

    // --- [ORIENT_2D]
    public static Sign Orient2D(Point3d a, Point3d b, Point3d c) => Orient2D(a.X, a.Y, b.X, b.Y, c.X, c.Y);

    private static Sign Orient2D(double ax, double ay, double bx, double by, double cx, double cy) {
        double acx = ax - cx, bcx = bx - cx, acy = ay - cy, bcy = by - cy;
        double detLeft = acx * bcy, detRight = acy * bcx;
        double det = detLeft - detRight;
        double detsum = Math.Abs(detLeft) + Math.Abs(detRight);
        return ErrorBound.Orient2D.Filter(det, detsum, ErrorBound.DoubleRoundoff)
            .IfNone(() => RefineOrient2D(ax, ay, bx, by, cx, cy)
                .IfNone(() => Orient2DExact(ax, ay, bx, by, cx, cy)));
    }

    // --- [ORIENT_3D]
    public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d) => Orient3D(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z);

    private static Sign Orient3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
        double adx = ax - dx, bdx = bx - dx, cdx = cx - dx;
        double ady = ay - dy, bdy = by - dy, cdy = cy - dy;
        double adz = az - dz, bdz = bz - dz, cdz = cz - dz;
        double bdxcdy = bdx * cdy, cdxbdy = cdx * bdy;
        double cdxady = cdx * ady, adxcdy = adx * cdy;
        double adxbdy = adx * bdy, bdxady = bdx * ady;
        double det = adz * (bdxcdy - cdxbdy) + bdz * (cdxady - adxcdy) + cdz * (adxbdy - bdxady);
        double permanent =
            (Math.Abs(bdxcdy) + Math.Abs(cdxbdy)) * Math.Abs(adz)
            + (Math.Abs(cdxady) + Math.Abs(adxcdy)) * Math.Abs(bdz)
            + (Math.Abs(adxbdy) + Math.Abs(bdxady)) * Math.Abs(cdz);
        return ErrorBound.Orient3D.Filter(det, permanent, ErrorBound.DoubleRoundoff)
            .IfNone(() => RefineOrient3D(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz)
                .IfNone(() => Orient3DExact(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz)));
    }

    // --- [IN_CIRCLE]
    public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d) => InCircle(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y);

    private static Sign InCircle(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
        double adx = ax - dx, bdx = bx - dx, cdx = cx - dx;
        double ady = ay - dy, bdy = by - dy, cdy = cy - dy;
        double bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, alift = adx * adx + ady * ady;
        double cdxady = cdx * ady, adxcdy = adx * cdy, blift = bdx * bdx + bdy * bdy;
        double adxbdy = adx * bdy, bdxady = bdx * ady, clift = cdx * cdx + cdy * cdy;
        double det = alift * (bdxcdy - cdxbdy) + blift * (cdxady - adxcdy) + clift * (adxbdy - bdxady);
        double permanent =
            (Math.Abs(bdxcdy) + Math.Abs(cdxbdy)) * alift
            + (Math.Abs(cdxady) + Math.Abs(adxcdy)) * blift
            + (Math.Abs(adxbdy) + Math.Abs(bdxady)) * clift;
        return ErrorBound.InCircle.Filter(det, permanent, ErrorBound.DoubleRoundoff)
            .IfNone(() => RefineInCircle(ax, ay, bx, by, cx, cy, dx, dy)
                .IfNone(() => InCircleExact(ax, ay, bx, by, cx, cy, dx, dy)));
    }

    // --- [IN_SPHERE]
    public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) =>
        InSphere(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z, e.X, e.Y, e.Z);

    private static Sign InSphere(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
        double aex = ax - ex, bex = bx - ex, cex = cx - ex, dex = dx - ex;
        double aey = ay - ey, bey = by - ey, cey = cy - ey, dey = dy - ey;
        double aez = az - ez, bez = bz - ez, cez = cz - ez, dez = dz - ez;
        double ab = aex * bey - bex * aey, bc = bex * cey - cex * bey, cd = cex * dey - dex * cey;
        double da = dex * aey - aex * dey, ac = aex * cey - cex * aey, bd = bex * dey - dex * bey;
        double abc = aez * bc - bez * ac + cez * ab;
        double bcd = bez * cd - cez * bd + dez * bc;
        double cda = cez * da + dez * ac + aez * cd;
        double dab = dez * ab + aez * bd + bez * da;
        double alift = aex * aex + aey * aey + aez * aez;
        double blift = bex * bex + bey * bey + bez * bez;
        double clift = cex * cex + cey * cey + cez * cez;
        double dlift = dex * dex + dey * dey + dez * dez;
        double det = (dlift * abc - clift * dab) + (blift * cda - alift * bcd);
        double aezAbs = Math.Abs(aez), bezAbs = Math.Abs(bez), cezAbs = Math.Abs(cez), dezAbs = Math.Abs(dez);
        double abAbs = Math.Abs(ab), bcAbs = Math.Abs(bc), cdAbs = Math.Abs(cd);
        double daAbs = Math.Abs(da), acAbs = Math.Abs(ac), bdAbs = Math.Abs(bd);
        double abcAbs = aezAbs * bcAbs + bezAbs * acAbs + cezAbs * abAbs;
        double bcdAbs = bezAbs * cdAbs + cezAbs * bdAbs + dezAbs * bcAbs;
        double cdaAbs = cezAbs * daAbs + dezAbs * acAbs + aezAbs * cdAbs;
        double dabAbs = dezAbs * abAbs + aezAbs * bdAbs + bezAbs * daAbs;
        double permanent = (dlift * abcAbs + clift * dabAbs) + (blift * cdaAbs + alift * bcdAbs);
        return ErrorBound.InSphere.Filter(det, permanent, ErrorBound.DoubleRoundoff)
            .IfNone(() => RefineInSphere(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz, ex, ey, ez)
                .IfNone(() => InSphereExact(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz, ex, ey, ez)));
    }

    // --- [IMPLICIT_ORIENT]
    public static Sign Orient2D(in ImplicitPoint a, in ImplicitPoint b, in ImplicitPoint c, Axis axis) {
        if (a.IsExplicit && b.IsExplicit && c.IsExplicit) {
            (Axis u, Axis v) = (axis.U, axis.V);
            return Orient2D(u.Read(a.AsExplicit), v.Read(a.AsExplicit), u.Read(b.AsExplicit), v.Read(b.AsExplicit), u.Read(c.AsExplicit), v.Read(c.AsExplicit));
        }
        (Interval N, Interval La, Interval Lb, Interval Lc) f = OrientNumerator<Interval>(in a, in b, in c, axis);
        if (f.N.Verdict.Case is Sign filtered && f.La.Verdict.Case is Sign fa && f.Lb.Verdict.Case is Sign fb && f.Lc.Verdict.Case is Sign fc) {
            return filtered.Times(fa).Times(fb).Times(fc).Times(fc);
        }
        (Dyadic N, Dyadic La, Dyadic Lb, Dyadic Lc) e = OrientNumerator<Dyadic>(in a, in b, in c, axis);
        (Sign ea, Sign eb, Sign ec) = (e.La.Sign, e.Lb.Sign, e.Lc.Sign);
        return e.N.Sign.Times(ea).Times(eb).Times(ec).Times(ec);
    }

    public static Sign Orient3D(in ImplicitPoint a, in ImplicitPoint b, in ImplicitPoint c, in ImplicitPoint d) {
        if (a.IsExplicit && b.IsExplicit && c.IsExplicit && d.IsExplicit) {
            return Orient3D(a.AsExplicit, b.AsExplicit, c.AsExplicit, d.AsExplicit);
        }
        (Interval N, Interval La, Interval Lb, Interval Lc, Interval Ld) f = OrientNumerator3<Interval>(in a, in b, in c, in d);
        if (f.N.Verdict.Case is Sign filtered && f.La.Verdict.Case is Sign fa && f.Lb.Verdict.Case is Sign fb
            && f.Lc.Verdict.Case is Sign fc && f.Ld.Verdict.Case is Sign fd) {
            return filtered.Times(fa).Times(fb).Times(fc).Times(fd);
        }
        (Dyadic N, Dyadic La, Dyadic Lb, Dyadic Lc, Dyadic Ld) e = OrientNumerator3<Dyadic>(in a, in b, in c, in d);
        return e.N.Sign
            .Times(e.La.Sign).Times(e.Lb.Sign)
            .Times(e.Lc.Sign).Times(e.Ld.Sign);
    }

    // --- [IMPLICIT_COMPARE]
    public static Sign Compare(in ImplicitPoint a, in ImplicitPoint b, Axis axis) {
        if (a.IsExplicit && b.IsExplicit) {
            return Sign.Of(axis.Read(a.AsExplicit).CompareTo(axis.Read(b.AsExplicit)));
        }
        (Interval N, Interval La, Interval Lb) f = CompareNumerator<Interval>(in a, in b, axis);
        if (f.N.Verdict.Case is Sign filtered && f.La.Verdict.Case is Sign fa && f.Lb.Verdict.Case is Sign fb) {
            return filtered.Times(fa).Times(fb);
        }
        (Dyadic N, Dyadic La, Dyadic Lb) e = CompareNumerator<Dyadic>(in a, in b, axis);
        return e.N.Sign.Times(e.La.Sign).Times(e.Lb.Sign);
    }

    // --- [IMPLICIT_IN_CIRCUM]
    public static Sign InCircle(Point3d a, Point3d b, Point3d c, in ImplicitPoint d, Axis axis) {
        if (d.IsExplicit) {
            (Axis u, Axis v) = (axis.U, axis.V);
            return InCircle(
                u.Read(a), v.Read(a), u.Read(b), v.Read(b),
                u.Read(c), v.Read(c), u.Read(d.AsExplicit), v.Read(d.AsExplicit));
        }
        (Interval Det, Interval Lambda) f = InCircleNumerator<Interval>(a, b, c, in d, axis);
        if (f.Det.Verdict.Case is Sign filtered && f.Lambda.Verdict.Case is Sign fl) return filtered.Times(fl).Times(fl);
        (Dyadic Det, Dyadic Lambda) exact = InCircleNumerator<Dyadic>(a, b, c, in d, axis);
        Sign lambda = exact.Lambda.Sign;
        return exact.Det.Sign.Times(lambda).Times(lambda);
    }

    public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, in ImplicitPoint e) {
        if (e.IsExplicit) return InSphere(a, b, c, d, e.AsExplicit);
        (Interval Det, Interval Lambda) f = InSphereNumerator<Interval>(a, b, c, d, in e);
        if (f.Det.Verdict.Case is Sign filtered && f.Lambda.Verdict.Case is Sign fl) return filtered.Times(fl);
        (Dyadic Det, Dyadic Lambda) exact = InSphereNumerator<Dyadic>(a, b, c, d, in e);
        return exact.Det.Sign.Times(exact.Lambda.Sign);
    }

    // --- [HOMOGENEOUS_FOLDS]
    internal static T Determinant3x3<T>((T X, T Y, T Z) r1, (T X, T Y, T Z) r2, (T X, T Y, T Z) r3) where T : struct, IExact<T> =>
        r1.X.Mul(r2.Y.Mul(r3.Z).Sub(r2.Z.Mul(r3.Y)))
            .Sub(r1.Y.Mul(r2.X.Mul(r3.Z).Sub(r2.Z.Mul(r3.X))))
            .Add(r1.Z.Mul(r2.X.Mul(r3.Y).Sub(r2.Y.Mul(r3.X))));

    static (T N, T La, T Lb, T Lc) OrientNumerator<T>(in ImplicitPoint a, in ImplicitPoint b, in ImplicitPoint c, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hc = c.Homogeneous<T>();
        (T ua, T va, T la) = (axis.U.Pick(in ha), axis.V.Pick(in ha), ha.Lambda);
        (T ub, T vb, T lb) = (axis.U.Pick(in hb), axis.V.Pick(in hb), hb.Lambda);
        (T uc, T vc, T lc) = (axis.U.Pick(in hc), axis.V.Pick(in hc), hc.Lambda);
        T n = ua.Mul(lc).Sub(uc.Mul(la)).Mul(vb.Mul(lc).Sub(vc.Mul(lb)))
            .Sub(va.Mul(lc).Sub(vc.Mul(la)).Mul(ub.Mul(lc).Sub(uc.Mul(lb))));
        return (n, la, lb, lc);
    }

    static (T N, T La, T Lb, T Lc, T Ld) OrientNumerator3<T>(in ImplicitPoint a, in ImplicitPoint b, in ImplicitPoint c, in ImplicitPoint d)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hc = c.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hd = d.Homogeneous<T>();
        (T X, T Y, T Z) rb = Row(in ha, in hb);
        (T X, T Y, T Z) rc = Row(in ha, in hc);
        (T X, T Y, T Z) rd = Row(in ha, in hd);
        return (Determinant3x3(rb, rc, rd), ha.Lambda, hb.Lambda, hc.Lambda, hd.Lambda);

        static (T X, T Y, T Z) Row(in (T X, T Y, T Z, T Lambda) anchor, in (T X, T Y, T Z, T Lambda) point) =>
            (point.X.Mul(anchor.Lambda).Sub(anchor.X.Mul(point.Lambda)),
             point.Y.Mul(anchor.Lambda).Sub(anchor.Y.Mul(point.Lambda)),
             point.Z.Mul(anchor.Lambda).Sub(anchor.Z.Mul(point.Lambda)));
    }

    static (T N, T La, T Lb) CompareNumerator<T>(in ImplicitPoint a, in ImplicitPoint b, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        return (axis.Pick(in ha).Mul(hb.Lambda).Sub(axis.Pick(in hb).Mul(ha.Lambda)), ha.Lambda, hb.Lambda);
    }

    static (T Det, T Lambda) InCircleNumerator<T>(Point3d a, Point3d b, Point3d c, in ImplicitPoint d, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) h = d.Homogeneous<T>();
        (Axis u, Axis v) = (axis.U, axis.V);
        T l = h.Lambda;
        T eu = u.Pick(in h).Sub(l.Scale(u.Read(a)));
        T ev = v.Pick(in h).Sub(l.Scale(v.Read(a)));
        T bu = l.Mul(T.Diff(u.Read(b), u.Read(a)));
        T bv = l.Mul(T.Diff(v.Read(b), v.Read(a)));
        T cu = l.Mul(T.Diff(u.Read(c), u.Read(a)));
        T cv = l.Mul(T.Diff(v.Read(c), v.Read(a)));
        T eLift = eu.Mul(eu).Add(ev.Mul(ev));
        T bLift = bu.Mul(bu).Add(bv.Mul(bv));
        T cLift = cu.Mul(cu).Add(cv.Mul(cv));
        T det = eLift.Mul(cu.Mul(bv).Sub(bu.Mul(cv)))
            .Add(bLift.Mul(eu.Mul(cv).Sub(cu.Mul(ev))))
            .Add(cLift.Mul(bu.Mul(ev).Sub(eu.Mul(bv))));
        return (det, l);
    }

    static (T Det, T Lambda) InSphereNumerator<T>(Point3d a, Point3d b, Point3d c, Point3d d, in ImplicitPoint e)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) h = e.Homogeneous<T>();
        T l = h.Lambda;
        (T ex, T ey, T ez) = (h.X.Sub(l.Scale(a.X)), h.Y.Sub(l.Scale(a.Y)), h.Z.Sub(l.Scale(a.Z)));
        (T, T, T) bp = Row(b, a, l);
        (T, T, T) cp = Row(c, a, l);
        (T, T, T) dp = Row(d, a, l);
        T eLift = ex.Mul(ex).Add(ey.Mul(ey)).Add(ez.Mul(ez));
        T bLift = Lift(bp);
        T cLift = Lift(cp);
        T dLift = Lift(dp);
        T det = eLift.Mul(Determinant3x3(bp, cp, dp))
            .Sub(bLift.Mul(Determinant3x3((ex, ey, ez), cp, dp)))
            .Add(cLift.Mul(Determinant3x3((ex, ey, ez), bp, dp)))
            .Sub(dLift.Mul(Determinant3x3((ex, ey, ez), bp, cp)));
        return (det, l);

        static (T X, T Y, T Z) Row(Point3d p, Point3d anchor, T l) =>
            (l.Mul(T.Diff(p.X, anchor.X)), l.Mul(T.Diff(p.Y, anchor.Y)), l.Mul(T.Diff(p.Z, anchor.Z)));

        static T Lift((T X, T Y, T Z) r) => r.X.Mul(r.X).Add(r.Y.Mul(r.Y)).Add(r.Z.Mul(r.Z));
    }

    // --- [EXACT_FALLBACKS]
    static Sign Orient2DExact(double ax, double ay, double bx, double by, double cx, double cy) =>
        Dyadic.Diff(ax, cx).Mul(Dyadic.Diff(by, cy))
            .Sub(Dyadic.Diff(ay, cy).Mul(Dyadic.Diff(bx, cx))).Sign;

    static Sign Orient3DExact(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
        Dyadic bc = Dyadic.Diff(bx, dx).Mul(Dyadic.Diff(cy, dy)).Sub(Dyadic.Diff(cx, dx).Mul(Dyadic.Diff(by, dy)));
        Dyadic ca = Dyadic.Diff(cx, dx).Mul(Dyadic.Diff(ay, dy)).Sub(Dyadic.Diff(ax, dx).Mul(Dyadic.Diff(cy, dy)));
        Dyadic ab = Dyadic.Diff(ax, dx).Mul(Dyadic.Diff(by, dy)).Sub(Dyadic.Diff(bx, dx).Mul(Dyadic.Diff(ay, dy)));
        return bc.Mul(Dyadic.Diff(az, dz))
            .Add(ca.Mul(Dyadic.Diff(bz, dz)))
            .Add(ab.Mul(Dyadic.Diff(cz, dz))).Sign;
    }

    static Sign InCircleExact(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
        (Dyadic adx, Dyadic ady) = (Dyadic.Diff(ax, dx), Dyadic.Diff(ay, dy));
        (Dyadic bdx, Dyadic bdy) = (Dyadic.Diff(bx, dx), Dyadic.Diff(by, dy));
        (Dyadic cdx, Dyadic cdy) = (Dyadic.Diff(cx, dx), Dyadic.Diff(cy, dy));
        Dyadic bc = bdx.Mul(cdy).Sub(cdx.Mul(bdy));
        Dyadic ca = cdx.Mul(ady).Sub(adx.Mul(cdy));
        Dyadic ab = adx.Mul(bdy).Sub(bdx.Mul(ady));
        return Lift2(adx, ady).Mul(bc)
            .Add(Lift2(bdx, bdy).Mul(ca))
            .Add(Lift2(cdx, cdy).Mul(ab)).Sign;
    }

    static Sign InSphereExact(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
        (Point3d a, Point3d b, Point3d c) = (new(ax, ay, az), new(bx, by, bz), new(cx, cy, cz));
        (Point3d d, Point3d e) = (new(dx, dy, dz), new(ex, ey, ez));
        Dyadic abc = Minor3(b, c, d_: a, e), bcd = Minor3(c, d, d_: b, e), cda = Minor3(d, a, d_: c, e), dab = Minor3(a, b, d_: d, e);
        return Lift3(d, e).Mul(abc).Sub(Lift3(c, e).Mul(dab))
            .Add(Lift3(b, e).Mul(cda).Sub(Lift3(a, e).Mul(bcd))).Sign;
    }

    // --- [DOUBLE_DOUBLE_REFINE]
    static Option<Sign> RefineOrient2D(double ax, double ay, double bx, double by, double cx, double cy) {
        (_, (ddouble acx, ddouble acy)) = ddouble.AdjustScale(0, ((ddouble)ax - cx, (ddouble)ay - cy));
        (_, (ddouble bcx, ddouble bcy)) = ddouble.AdjustScale(0, ((ddouble)bx - cx, (ddouble)by - cy));
        ddouble detLeft = acx * bcy, detRight = acy * bcx;
        return ErrorBound.Orient2D.Filter(detLeft - detRight, ddouble.Abs(detLeft) + ddouble.Abs(detRight), ErrorBound.DoubleDoubleRoundoff);
    }

    static Option<Sign> RefineOrient3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
        (_, (ddouble adx, ddouble ady, ddouble adz)) = ddouble.AdjustScale(0, ((ddouble)ax - dx, (ddouble)ay - dy, (ddouble)az - dz));
        (_, (ddouble bdx, ddouble bdy, ddouble bdz)) = ddouble.AdjustScale(0, ((ddouble)bx - dx, (ddouble)by - dy, (ddouble)bz - dz));
        (_, (ddouble cdx, ddouble cdy, ddouble cdz)) = ddouble.AdjustScale(0, ((ddouble)cx - dx, (ddouble)cy - dy, (ddouble)cz - dz));
        ddouble bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, cdxady = cdx * ady, adxcdy = adx * cdy, adxbdy = adx * bdy, bdxady = bdx * ady;
        ddouble det = adz * (bdxcdy - cdxbdy) + bdz * (cdxady - adxcdy) + cdz * (adxbdy - bdxady);
        ddouble permanent =
            (ddouble.Abs(bdxcdy) + ddouble.Abs(cdxbdy)) * ddouble.Abs(adz)
            + (ddouble.Abs(cdxady) + ddouble.Abs(adxcdy)) * ddouble.Abs(bdz)
            + (ddouble.Abs(adxbdy) + ddouble.Abs(bdxady)) * ddouble.Abs(cdz);
        return ErrorBound.Orient3D.Filter(det, permanent, ErrorBound.DoubleDoubleRoundoff);
    }

    static Option<Sign> RefineInCircle(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
        ddouble adx = (ddouble)ax - dx, ady = (ddouble)ay - dy;
        ddouble bdx = (ddouble)bx - dx, bdy = (ddouble)by - dy;
        ddouble cdx = (ddouble)cx - dx, cdy = (ddouble)cy - dy;
        ddouble bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, alift = adx * adx + ady * ady;
        ddouble cdxady = cdx * ady, adxcdy = adx * cdy, blift = bdx * bdx + bdy * bdy;
        ddouble adxbdy = adx * bdy, bdxady = bdx * ady, clift = cdx * cdx + cdy * cdy;
        ddouble det = alift * (bdxcdy - cdxbdy) + blift * (cdxady - adxcdy) + clift * (adxbdy - bdxady);
        ddouble permanent =
            (ddouble.Abs(bdxcdy) + ddouble.Abs(cdxbdy)) * alift
            + (ddouble.Abs(cdxady) + ddouble.Abs(adxcdy)) * blift
            + (ddouble.Abs(adxbdy) + ddouble.Abs(bdxady)) * clift;
        return ErrorBound.InCircle.Filter(det, permanent, ErrorBound.DoubleDoubleRoundoff);
    }

    static Option<Sign> RefineInSphere(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
        ddouble aex = (ddouble)ax - ex, aey = (ddouble)ay - ey, aez = (ddouble)az - ez;
        ddouble bex = (ddouble)bx - ex, bey = (ddouble)by - ey, bez = (ddouble)bz - ez;
        ddouble cex = (ddouble)cx - ex, cey = (ddouble)cy - ey, cez = (ddouble)cz - ez;
        ddouble dex = (ddouble)dx - ex, dey = (ddouble)dy - ey, dez = (ddouble)dz - ez;
        ddouble ab = aex * bey - bex * aey, bc = bex * cey - cex * bey, cd = cex * dey - dex * cey;
        ddouble da = dex * aey - aex * dey, ac = aex * cey - cex * aey, bd = bex * dey - dex * bey;
        ddouble abc = aez * bc - bez * ac + cez * ab, bcd = bez * cd - cez * bd + dez * bc;
        ddouble cda = cez * da + dez * ac + aez * cd, dab = dez * ab + aez * bd + bez * da;
        ddouble alift = aex * aex + aey * aey + aez * aez, blift = bex * bex + bey * bey + bez * bez;
        ddouble clift = cex * cex + cey * cey + cez * cez, dlift = dex * dex + dey * dey + dez * dez;
        ddouble det = dlift * abc - clift * dab + blift * cda - alift * bcd;
        ddouble permanent =
            dlift * (ddouble.Abs(aez) * ddouble.Abs(bc) + ddouble.Abs(bez) * ddouble.Abs(ac) + ddouble.Abs(cez) * ddouble.Abs(ab))
            + clift * (ddouble.Abs(dez) * ddouble.Abs(ab) + ddouble.Abs(aez) * ddouble.Abs(bd) + ddouble.Abs(bez) * ddouble.Abs(da))
            + blift * (ddouble.Abs(cez) * ddouble.Abs(da) + ddouble.Abs(dez) * ddouble.Abs(ac) + ddouble.Abs(aez) * ddouble.Abs(cd))
            + alift * (ddouble.Abs(bez) * ddouble.Abs(cd) + ddouble.Abs(cez) * ddouble.Abs(bd) + ddouble.Abs(dez) * ddouble.Abs(bc));
        return ErrorBound.InSphere.Filter(det, permanent, ErrorBound.DoubleDoubleRoundoff);
    }

    // --- [LIFTS_AND_MINORS]
    static Dyadic Lift2(Dyadic x, Dyadic y) => x.Mul(x).Add(y.Mul(y));

    static Dyadic Lift3(Point3d p, Point3d anchor) {
        (Dyadic x, Dyadic y, Dyadic z) = (Dyadic.Diff(p.X, anchor.X), Dyadic.Diff(p.Y, anchor.Y), Dyadic.Diff(p.Z, anchor.Z));
        return x.Mul(x).Add(y.Mul(y)).Add(z.Mul(z));
    }

    static Dyadic Minor3(Point3d p, Point3d q, Point3d d_, Point3d e) {
        Dyadic pq = Dyadic.Diff(p.X, e.X).Mul(Dyadic.Diff(q.Y, e.Y)).Sub(Dyadic.Diff(q.X, e.X).Mul(Dyadic.Diff(p.Y, e.Y)));
        Dyadic qd = Dyadic.Diff(q.X, e.X).Mul(Dyadic.Diff(d_.Y, e.Y)).Sub(Dyadic.Diff(d_.X, e.X).Mul(Dyadic.Diff(q.Y, e.Y)));
        Dyadic dp = Dyadic.Diff(d_.X, e.X).Mul(Dyadic.Diff(p.Y, e.Y)).Sub(Dyadic.Diff(p.X, e.X).Mul(Dyadic.Diff(d_.Y, e.Y)));
        return pq.Mul(Dyadic.Diff(d_.Z, e.Z))
            .Add(qd.Mul(Dyadic.Diff(p.Z, e.Z)))
            .Add(dp.Mul(Dyadic.Diff(q.Z, e.Z)));
    }
}
```

## [03]-[INTERIOR_NUMERICS]

- Owner: `IExact<TSelf>` is the static-abstract exact-carrier algebra letting every construction and determinant polynomial be written ONCE and instantiated at both carriers; `Dyadic` is the exact context-free `EFloat` carrier whose default value projects to `EFloat.Zero`; `Interval` is the directed-rounding `EFloat` bracket whose `Verdict` resolves exactly when the bracket excludes zero, at fixed bounded cost per operation — the software directed rounding the runtime cannot express through FPU mode switches; `ErrorBound` is `Predicate`'s private generic permanence-coefficient row, seating the `double` and `ddouble` roundoff constants its one `Filter<T>` evaluates.
- Cases: `IExact` is the algebra contract both carriers implement, and it declares only what a polynomial on this page calls — `Of`/`Diff` static and `Add`/`Sub`/`Mul`/`Scale` instance — because an unread `static abstract` taxes every future conformance forever; `ErrorBound` carries one row per direct predicate, never a parallel threshold owner.
- Entry: `Dyadic.Of` and `Dyadic.Diff` lift every binary64 operand losslessly through `EFloat.FromDouble`; context-free `Add`/`Subtract`/`Multiply` remain exact, `Sign` reads the exact verdict, and `Quotient` performs the one terminal rounding under `EContext.Binary64`; `ErrorBound.Filter<T>` is the one generic filter projection returning a determinate `Sign` or `Option.None` escalation.
- Auto: exact dyadic arithmetic preserves every determinant polynomial without an operand-magnitude ceiling; `Interval.Mul` brackets all four endpoint products under both directed contexts, so a resolved `Verdict` is a PROOF of the exact sign — the filter accepts or escalates, never mis-decides.
- Law: the `Interval` bracket composes the PeterO members the catalogue verifies and nothing else — `EContext.ForPrecisionAndRounding(53, ERounding.Floor|Ceiling)` beside `WithPrecisionInBits(true)` (`api-petero-numbers.md [03]` `EContext` row `[02]` and `[ECONTEXT_BUILDERS]`), the `EFloat.FromDouble` dyadic lift (`[03]` `EFloat` row `[01]`), the context-taking `Add`/`Subtract`/`Multiply` arities (`[EFLOAT_ARITHMETIC]`), and `Sign`/`IsZero` for the verdict (`[03]` row `[06]`, `[EFLOAT_CLASSIFY]`). PeterO publishes NO `Min`/`Max` member on `EFloat` — the catalogue's whole ordering surface is `CompareTo`/`CompareToTotal` (`[03]` rows `[07]`/`[08]`, `[IMPLEMENTATION_LAW] [TOPOLOGY]` "every ordering read spells `CompareTo`") and the type ships no relational operators — so the four-endpoint `Mul` folds its bounds through two `CompareTo` reductions rather than a member that does not exist.
- Exemption: `NextPlus`/`NextMinus` and every analytic member require the bracket's finite bounded context and return NaN under `EContext.Unlimited` (`api-petero-numbers.md [EFLOAT_NEIGHBOUR]`), so neither enters the ladder, and no member on this page reads `Unlimited` at all.
- Law: only `Sign`, `Axis`, `ImplicitPoint`, `HalfPlane`, and `Predicate` cross the module boundary — `IExact`, `Dyadic`, `Interval`, and every `Homogeneous<T>` are `internal`, so interior arithmetic, filters, and constants cross no public signature and the exact result is the `Sign` the predicate returns. `Dyadic.Sign` over the exact terminal value IS the verdict; a differential oracle recomputes from original inputs in the test assembly and never ships inside a verdict.
- Packages: TYoshimura.DoubleDouble (106-bit refine), PeterO.Numbers (the interval and exact dyadic tiers), BCL inbox (`INumber<T>`); no external geometry dependency.
- Growth: a new exact carrier is one `IExact` conformance every construction instantiates with zero polynomial edits; a new predicate's filter is one `ErrorBound` row; longer exact computations remain within `EFloat` rather than growing a second arithmetic engine.
- Boundary: `Dyadic` is ONE owner for sign-exact arithmetic — a free error-transform set or a parallel `BigFloat`/`MPFR` type is the deleted form. `Interval` is ONE owner for the directed-rounding bracket and keeps its bare name against every upper-folder twin — the discriminant is the CARRIED PROOF, a pair of directed-rounded `EFloat` endpoints whose `Verdict` is a sign proof, where an upper folder's same-named type is a scalar range; a per-predicate epsilon-inflation filter is the deleted form, the bracket sound by construction where an epsilon guess is a tuned lie. `ErrorBound` is the single permanence-coefficient row, private to `Predicate` — no key, roster, generated dispatch, or record equality, since no reader looks a row up, compares one, or renders one — each row carrying the published `(alpha, beta)` pair and deriving `(alpha + beta*eps)*eps` at whatever typed roundoff a tier hands it, so a precision stage is ONE argument and the two roundoff constants seat on the row that consumes them.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using DoubleDouble;
using LanguageExt;
using PeterO.Numbers;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface IExact<TSelf> where TSelf : struct, IExact<TSelf> {
    static abstract TSelf Of(double value);
    static abstract TSelf Diff(double a, double b);
    TSelf Add(TSelf other);
    TSelf Sub(TSelf other);
    TSelf Mul(TSelf other);
    TSelf Scale(double exact);
}

// --- [MODELS] --------------------------------------------------------------------------
internal readonly struct Dyadic : IExact<Dyadic> {
    readonly EFloat? value;
    EFloat Value => value ?? EFloat.Zero;

    Dyadic(EFloat value) => this.value = value;

    public static Dyadic Of(double value) => new(EFloat.FromDouble(value));
    public static Dyadic Diff(double a, double b) => new(EFloat.FromDouble(a).Subtract(EFloat.FromDouble(b)));
    public Dyadic Add(Dyadic other) => new(Value.Add(other.Value));
    public Dyadic Sub(Dyadic other) => new(Value.Subtract(other.Value));
    public Dyadic Mul(Dyadic other) => new(Value.Multiply(other.Value));
    public Dyadic Scale(double exact) => Mul(Of(exact));
    internal Sign Sign => Rasm.Numerics.Sign.Of(Value.Sign);
    internal double Quotient(Dyadic denominator) => Value.Divide(denominator.Value, EContext.Binary64).ToDouble();
}

internal readonly struct Interval : IExact<Interval> {
    static readonly EContext Down = EContext.ForPrecisionAndRounding(53, ERounding.Floor).WithPrecisionInBits(true);
    static readonly EContext Up = EContext.ForPrecisionAndRounding(53, ERounding.Ceiling).WithPrecisionInBits(true);

    private readonly EFloat Lo;
    private readonly EFloat Hi;

    Interval(EFloat lo, EFloat hi) { Lo = lo; Hi = hi; }

    public static Interval Of(double value) => new(EFloat.FromDouble(value), EFloat.FromDouble(value));

    public static Interval Diff(double a, double b) =>
        new(EFloat.FromDouble(a).Subtract(EFloat.FromDouble(b), Down), EFloat.FromDouble(a).Subtract(EFloat.FromDouble(b), Up));

    public Interval Add(Interval other) => new(Lo.Add(other.Lo, Down), Hi.Add(other.Hi, Up));

    public Interval Sub(Interval other) => new(Lo.Subtract(other.Hi, Down), Hi.Subtract(other.Lo, Up));

    public Interval Mul(Interval other) {
        EFloat lo = Least(Lo.Multiply(other.Lo, Down), Lo.Multiply(other.Hi, Down), Hi.Multiply(other.Lo, Down), Hi.Multiply(other.Hi, Down));
        EFloat hi = Greatest(Lo.Multiply(other.Lo, Up), Lo.Multiply(other.Hi, Up), Hi.Multiply(other.Lo, Up), Hi.Multiply(other.Hi, Up));
        return new Interval(lo, hi);

        static EFloat Least(EFloat a, EFloat b, EFloat c, EFloat d) => Min(Min(a, b), Min(c, d));
        static EFloat Greatest(EFloat a, EFloat b, EFloat c, EFloat d) => Max(Max(a, b), Max(c, d));
        static EFloat Min(EFloat a, EFloat b) => a.CompareTo(b) <= 0 ? a : b;
        static EFloat Max(EFloat a, EFloat b) => a.CompareTo(b) >= 0 ? a : b;
    }

    public Interval Scale(double exact) => Mul(Of(exact));

    public Option<Sign> Verdict =>
        Lo.Sign > 0 ? Some(Sign.Positive)
        : Hi.Sign < 0 ? Some(Sign.Negative)
        : Lo.IsZero && Hi.IsZero ? Some(Sign.Zero)
        : None;
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
