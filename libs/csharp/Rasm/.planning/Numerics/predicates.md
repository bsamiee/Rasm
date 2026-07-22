# [RASM_NUMERICS_PREDICATES]

`Predicate` owns the adaptive-precision exact-sign floor every higher geometry owner composes, returning a total `Sign` for explicit and constructed points alike. A constructed intersection point travels as defining-point carriage with exact coordinates derived on demand, so no rounded coordinate enters a predicate and rounding happens once at the consumer's emission seam.

Every fold is one polynomial instantiated at both the `Interval` filter and `Expansion` exact carriers through the `IExact<TSelf>` algebra, so the filter never tests a different polynomial than the exact branch decides.

## [01]-[INDEX]

- [02]-[ROBUST_PREDICATES]: `Predicate` folds the direct ladders and the constructed-point family to a total `Sign`.
- [03]-[INTERIOR_NUMERICS]: `IExact` carriers stack the filter, refine, and exact-rational tiers under `NumericsPolicy`.

## [02]-[ROBUST_PREDICATES]

- Owner: `Sign` `[SmartEnum<int>]` is the closed ternary verdict every predicate returns, carrying the `Flip`/`Times` parity algebra; `Axis` `[SmartEnum<int>]` is the closed coordinate vocabulary and the ONE generator every axis-projected member spans its three planes over; `Implicit` `[Union<Point3d, Ssi, Lpi, Tpi>]` carries a constructed point as DEFINING POINTS ONLY, its exact homogeneous coordinates derived on demand through `Homogeneous<T>`; `Predicate` is the ONE static surface owning both the direct ladders and the implicit folds.
- Cases: `Sign`, `Axis`, and the four `Implicit` constructions are the closed vocabularies; `Predicate` carries the four direct members `Orient2D`/`Orient3D`/`InCircle`/`InSphere` beside `Orient2D(in Implicit, in Implicit, in Implicit, Axis)` spanning every explicit/implicit combination × projection plane, `Compare(in Implicit, in Implicit, Axis)` the exact per-coordinate order key, and the in-circum `InCircle`/`InSphere` implicit queries.
- Entry: every member is a total pure exact function returning `Sign` with no rail; the raw-`double` direct entries are the core cross-package consumers bind, since the Compute lane bars host value types on interior signatures, and the `Point3d` overloads adapt at the seam. Implicit entries discriminate on the carrier's case shape and the `Axis` row. A degenerate construction (`lambda = 0`) yields `Sign.Zero` through the `Times` flip algebra, the degeneracy witness the consumer's recovery reads.
- Auto: each direct member filters in `double`, refines at 106-bit `ddouble`, then folds the sign-exact `Expansion`; each implicit member runs the `Interval` directed-rounding filter on the SAME polynomial first, escalating the indeterminate residue to `Expansion` and the in-circum queries on to `RationalOracle.InCircum`. Every member walks its tiers inline as one `??`-chain over the uniform `Sign?`-or-escalate protocol, allocation-free with no captured thunk; every tier is monotone and sign-consistent, so the verdict is always the true sign.
- Receipt: none — a `Sign` verdict carries no residual. Its one emission-side materialization is `Implicit.Round()`, the rounded `Point3d` a consumer emits at its own seam, never a value any predicate reads back.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`), RhinoCommon (`Point3d`), TYoshimura.DoubleDouble (106-bit refine), ExtendedNumerics.BigRational (exact-rational oracle), PeterO.Numbers (interval filter, second-source adjudicator), Rasm.Domain (`Op`/`Fault.InvalidInput`, the `DominantOf` refusal rail), BCL inbox (`FusedMultiplyAdd`, intrinsics probes, `BigInteger`).
- Growth: a new implicit construction is one `Implicit` case carrying its defining points and one `Homogeneous<T>` arm, every fold and emission member widening by that arm with the generated dispatch breaking loudly; a new direct predicate is one member and one `ErrorBound` row; a new precision stage is one `PrecisionTier` row with one escalation arm per member tail. Multi-implicit in-circum combinations and the 3D multi-implicit `Orient3D`/`InSphere` family are CDTet-gated arms on the existing members — zero new surface.
- Boundary: the whole family lives on ONE `Predicate` static owner — a per-predicate class or a `FastOrient2D`/`ExactOrient2D` pair is the deleted form. Verdicts are the closed `Sign` and a raw `int`/`double` sign crossing a public signature is the named defect; coordinates are `Point3d` read at the seam, a domain-local point struct the deleted form. A constructed point travels as `Implicit` defining-point carriage rounded ONCE at `Round()` — a `Denominator`-as-`double` field or an `Estimate()` inside an exact carrier is the named robustness defect — and derived `Plane` inputs are dead, so a three-plane point is its NINE points. `DominantOf` is the ONE geometry admission, its float normals barred from every exact carrier; every leaf difference rides the error-free `IExact.Diff`, a raw `double` subtraction wrapped in an exact type the deleted rounded-leaf form. Loosening a filter band to pass a near-degenerate case instead of taking the exact branch is the named correctness defect — a sign verdict is exact or it is a defect.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using DoubleDouble;
using ExtendedNumerics;
using PeterO.Numbers;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Numerics;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class Sign {
    public static readonly Sign Negative = new(-1);
    public static readonly Sign Zero = new(0);
    public static readonly Sign Positive = new(1);

    public static Sign Of(double value) => value < 0.0 ? Negative : value > 0.0 ? Positive : Zero;
    public static Sign Of(int value) => value < 0 ? Negative : value > 0 ? Positive : Zero;

    public Sign Flip => Switch(negative: static _ => Positive, zero: static _ => Zero, positive: static _ => Negative);
    public Sign Times(Sign other) => Of(Key * other.Key);
}

// A rounded-coordinate Double filter cannot exist for a constructed point, so implicit members open at Interval.
[SmartEnum<int>]
public sealed partial class PrecisionTier {
    public static readonly PrecisionTier Double       = new(0); // IEEE-754 forward-error filter (direct only)
    public static readonly PrecisionTier DoubleDouble = new(1); // ddouble 106-bit error-free-transform refine (direct only)
    public static readonly PrecisionTier Interval     = new(2); // directed-rounding EFloat bracket filter (implicit only)
    public static readonly PrecisionTier Expansion    = new(3); // sign-exact nonoverlapping expansion
    public static readonly PrecisionTier Rational     = new(4); // Fraction BigInteger exact-rational oracle
}

// Key = the axis' coordinate ordinal (the Compare column); U/V = the plane NORMAL to it (the
// Orient2D/InCircle projection plane). DominantOf is the ONE geometry admission — a float HEURISTIC
// picking a projection plane, its cross/Newell normals barred from every exact carrier below. It
// returns Fin<Axis> because a non-finite or zero normal has no dominant axis and refuses onto the
// Op-keyed rail, never the silent max-component fallback a NaN component would pick; the vector
// arity is the one gate the triangle/quad arities delegate to. Op? key = null threads the caller's Op.
[SmartEnum<int>]
public sealed partial class Axis {
    public static readonly Axis X = new(0, u: 1, v: 2);
    public static readonly Axis Y = new(1, u: 2, v: 0);
    public static readonly Axis Z = new(2, u: 0, v: 1);

    public int U { get; }
    public int V { get; }

    public static double Coord(Point3d p, int ordinal) => ordinal == 0 ? p.X : ordinal == 1 ? p.Y : p.Z;

    public static Fin<Axis> DominantOf(Vector3d d, Op? key = null) =>
        d.IsValid && !d.IsZero
            ? Fin.Succ(Dominant(Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z)))
            : Fin.Fail<Axis>(key.OrDefault().InvalidInput());

    public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c, Op? key = null) =>
        DominantOf(Vector3d.CrossProduct(b - a, c - a), key);

    // Newell normal over the quad cycle — robust for the nonplanar quad one corner cross is not.
    public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c, Point3d d, Op? key = null) {
        Span<Point3d> ring = [a, b, c, d];
        double nx = 0.0, ny = 0.0, nz = 0.0;
        for (int i = 0; i < 4; i++) {
            (Point3d p, Point3d q) = (ring[i], ring[(i + 1) & 3]);
            nx += (p.Y - q.Y) * (p.Z + q.Z);
            ny += (p.Z - q.Z) * (p.X + q.X);
            nz += (p.X - q.X) * (p.Y + q.Y);
        }
        return DominantOf(new Vector3d(nx, ny, nz), key);
    }

    static Axis Dominant(double x, double y, double z) => x >= y && x >= z ? X : y >= z ? Y : Z;
}

[Union<Point3d, Ssi, Lpi, Tpi>(T1Name = "Explicit", T2Name = "Ssi", T3Name = "Lpi", T4Name = "Tpi")]
public readonly partial struct Implicit {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> =>
        Switch(
            @explicit: static e => (T.Of(e.X), T.Of(e.Y), T.Of(e.Z), T.Of(1.0)),
            ssi:       static s => s.Homogeneous<T>(),
            lpi:       static l => l.Homogeneous<T>(),
            tpi:       static t => t.Homogeneous<T>());

    // A zero-lambda construction rounds to non-finite coordinates the freeze gate rejects; rounding never
    // invents a point the construction lacks.
    public Point3d Round() =>
        IsExplicit ? AsExplicit : Materialized(Homogeneous<Expansion>());

    static Point3d Materialized((Expansion X, Expansion Y, Expansion Z, Expansion Lambda) h) {
        double lambda = h.Lambda.Estimate();
        return new Point3d(h.X.Estimate() / lambda, h.Y.Estimate() / lambda, h.Z.Estimate() / lambda);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// SSI: two coplanar segments (P,Q)×(R,S) crossing — the parametric numerators ride the (P,Q) segment in all three coordinates.
public readonly record struct Ssi(Point3d P, Point3d Q, Point3d R, Point3d S, Axis Plane) {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
        (int u, int v) = (Plane.U, Plane.V);
        T lambda = T.Diff(Axis.Coord(S, u), Axis.Coord(R, u)).Mul(T.Diff(Axis.Coord(Q, v), Axis.Coord(P, v)))
            .Sub(T.Diff(Axis.Coord(S, v), Axis.Coord(R, v)).Mul(T.Diff(Axis.Coord(Q, u), Axis.Coord(P, u))));
        T n = T.Diff(Axis.Coord(S, u), Axis.Coord(P, u)).Mul(T.Diff(Axis.Coord(Q, v), Axis.Coord(P, v)))
            .Sub(T.Diff(Axis.Coord(S, v), Axis.Coord(P, v)).Mul(T.Diff(Axis.Coord(Q, u), Axis.Coord(P, u))));
        return (Parametric(lambda, n, P.X, Q.X), Parametric(lambda, n, P.Y, Q.Y), Parametric(lambda, n, P.Z, Q.Z), lambda);

        static T Parametric(T lambda, T n, double at, double head) =>
            lambda.Scale(at).Add(n.Mul(T.Diff(head, at)));
    }
}

// LPI: the crossing of line (P,Q) with the plane through (A,B,C) — five points, three numerators.
public readonly record struct Lpi(Point3d P, Point3d Q, Point3d A, Point3d B, Point3d C) {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
        (T ax, T ay, T az) = (T.Diff(P.X, Q.X), T.Diff(P.Y, Q.Y), T.Diff(P.Z, Q.Z));
        (T bx, T by, T bz) = (T.Diff(B.X, A.X), T.Diff(B.Y, A.Y), T.Diff(B.Z, A.Z));
        (T cx, T cy, T cz) = (T.Diff(C.X, A.X), T.Diff(C.Y, A.Y), T.Diff(C.Z, A.Z));
        (T m1, T m2, T m3) = (by.Mul(cz).Sub(bz.Mul(cy)), bx.Mul(cz).Sub(bz.Mul(cx)), bx.Mul(cy).Sub(by.Mul(cx)));
        T lambda = ax.Mul(m1).Sub(ay.Mul(m2)).Add(az.Mul(m3));
        T n = T.Diff(P.X, A.X).Mul(m1).Sub(T.Diff(P.Y, A.Y).Mul(m2)).Add(T.Diff(P.Z, A.Z).Mul(m3));
        return (lambda.Scale(P.X).Sub(ax.Mul(n)), lambda.Scale(P.Y).Sub(ay.Mul(n)), lambda.Scale(P.Z).Sub(az.Mul(n)), lambda);
    }
}

// TPI: three planes, EACH by three points.
public readonly record struct Tpi(
    Point3d P1, Point3d P2, Point3d P3,
    Point3d Q1, Point3d Q2, Point3d Q3,
    Point3d R1, Point3d R2, Point3d R3) {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
        ((T, T, T) np, T dp) = PlaneRow<T>(P1, P2, P3);
        ((T, T, T) nq, T dq) = PlaneRow<T>(Q1, Q2, Q3);
        ((T, T, T) nr, T dr) = PlaneRow<T>(R1, R2, R3);
        return (
            Det3((dp, np.Item2, np.Item3), (dq, nq.Item2, nq.Item3), (dr, nr.Item2, nr.Item3)),
            Det3((np.Item1, dp, np.Item3), (nq.Item1, dq, nq.Item3), (nr.Item1, dr, nr.Item3)),
            Det3((np.Item1, np.Item2, dp), (nq.Item1, nq.Item2, dq), (nr.Item1, nr.Item2, dr)),
            Det3(np, nq, nr));
    }

    static ((T X, T Y, T Z) Normal, T Offset) PlaneRow<T>(Point3d a, Point3d b, Point3d c) where T : struct, IExact<T> {
        (T ux, T uy, T uz) = (T.Diff(b.X, a.X), T.Diff(b.Y, a.Y), T.Diff(b.Z, a.Z));
        (T vx, T vy, T vz) = (T.Diff(c.X, a.X), T.Diff(c.Y, a.Y), T.Diff(c.Z, a.Z));
        (T nx, T ny, T nz) = (uy.Mul(vz).Sub(uz.Mul(vy)), uz.Mul(vx).Sub(ux.Mul(vz)), ux.Mul(vy).Sub(uy.Mul(vx)));
        return ((nx, ny, nz), nx.Scale(a.X).Add(ny.Scale(a.Y)).Add(nz.Scale(a.Z)));
    }

    internal static T Det3<T>((T X, T Y, T Z) r1, (T X, T Y, T Z) r2, (T X, T Y, T Z) r3) where T : struct, IExact<T> =>
        r1.X.Mul(r2.Y.Mul(r3.Z).Sub(r2.Z.Mul(r3.Y)))
            .Sub(r1.Y.Mul(r2.X.Mul(r3.Z).Sub(r2.Z.Mul(r3.X))))
            .Add(r1.Z.Mul(r2.X.Mul(r3.Y).Sub(r2.Y.Mul(r3.X))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Predicate {
    // A captured-closure fold would allocate two delegates per call on the hottest path — hence the inline `??`-chain.
    // Middle and exact tiers share the `TwoProduct`/`TwoSum` model, so they never disagree on a sign both resolve.

    // --- [COORDINATE_CORE]

    // --- [ORIENT_2D]
    public static Sign Orient2D(Point3d a, Point3d b, Point3d c) => Orient2D(a.X, a.Y, b.X, b.Y, c.X, c.Y);

    public static Sign Orient2D(double ax, double ay, double bx, double by, double cx, double cy) {
        double acx = ax - cx, bcx = bx - cx, acy = ay - cy, bcy = by - cy;
        double detLeft = acx * bcy, detRight = acy * bcx;
        double det = detLeft - detRight;
        double detsum = Math.Abs(detLeft) + Math.Abs(detRight);
        return ErrorBound.Orient2D.Of(det, detsum)
            ?? RefineOrient2D(new Point3d(ax, ay, 0.0), new Point3d(bx, by, 0.0), new Point3d(cx, cy, 0.0))
            ?? Orient2DExact(new Point3d(ax, ay, 0.0), new Point3d(bx, by, 0.0), new Point3d(cx, cy, 0.0));
    }

    // --- [ORIENT_3D]
    public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d) => Orient3D(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z);

    public static Sign Orient3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
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
        return ErrorBound.Orient3D.Of(det, permanent)
            ?? RefineOrient3D(new Point3d(ax, ay, az), new Point3d(bx, by, bz), new Point3d(cx, cy, cz), new Point3d(dx, dy, dz))
            ?? Orient3DExact(new Point3d(ax, ay, az), new Point3d(bx, by, bz), new Point3d(cx, cy, cz), new Point3d(dx, dy, dz));
    }

    // --- [IN_CIRCLE]
    public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d) => InCircle(a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y);

    public static Sign InCircle(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
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
        return ErrorBound.InCircle.Of(det, permanent)
            ?? RefineInCircle(new Point3d(ax, ay, 0.0), new Point3d(bx, by, 0.0), new Point3d(cx, cy, 0.0), new Point3d(dx, dy, 0.0))
            ?? InCircleExact(new Point3d(ax, ay, 0.0), new Point3d(bx, by, 0.0), new Point3d(cx, cy, 0.0), new Point3d(dx, dy, 0.0));
    }

    // --- [IN_SPHERE]
    public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) =>
        InSphere(a.X, a.Y, a.Z, b.X, b.Y, b.Z, c.X, c.Y, c.Z, d.X, d.Y, d.Z, e.X, e.Y, e.Z);

    public static Sign InSphere(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
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
        return ErrorBound.InSphere.Of(det, permanent)
            ?? RefineInSphere(new Point3d(ax, ay, az), new Point3d(bx, by, bz), new Point3d(cx, cy, cz), new Point3d(dx, dy, dz), new Point3d(ex, ey, ez))
            ?? InSphereExact(new Point3d(ax, ay, az), new Point3d(bx, by, bz), new Point3d(cx, cy, cz), new Point3d(dx, dy, dz), new Point3d(ex, ey, ez));
    }

    // --- [IMPLICIT_ORIENT]
    // Verdicts compose the exact lambda signs through Sign.Times at each lambda's parity — la/lb ODD (flip),
    // lc SQUARED (Zero-gates, no flip) — so a negative denominator never mis-signs and a zero one yields Zero.
    public static Sign Orient2D(in Implicit a, in Implicit b, in Implicit c, Axis axis) {
        if (a.IsExplicit && b.IsExplicit && c.IsExplicit) {
            return Orient2D(Swizzled(a.AsExplicit, axis), Swizzled(b.AsExplicit, axis), Swizzled(c.AsExplicit, axis));
        }
        (Interval N, Interval La, Interval Lb, Interval Lc) f = OrientNumerator<Interval>(in a, in b, in c, axis);
        if (f.N.Verdict is { } filtered && f.La.Verdict is { } fa && f.Lb.Verdict is { } fb && f.Lc.Verdict is { } fc) {
            return filtered.Times(fa).Times(fb).Times(fc).Times(fc);
        }
        (Expansion N, Expansion La, Expansion Lb, Expansion Lc) e = OrientNumerator<Expansion>(in a, in b, in c, axis);
        (Sign ea, Sign eb, Sign ec) = (Expansion.SignOf(e.La), Expansion.SignOf(e.Lb), Expansion.SignOf(e.Lc));
        return Expansion.SignOf(e.N).Times(ea).Times(eb).Times(ec).Times(ec);
    }

    // --- [IMPLICIT_COMPARE]
    // Exact per-coordinate order key: Negative = a-before-b.
    public static Sign Compare(in Implicit a, in Implicit b, Axis axis) {
        if (a.IsExplicit && b.IsExplicit) {
            return Sign.Of(Axis.Coord(a.AsExplicit, axis.Key).CompareTo(Axis.Coord(b.AsExplicit, axis.Key)));
        }
        (Interval N, Interval La, Interval Lb) f = CompareNumerator<Interval>(in a, in b, axis);
        if (f.N.Verdict is { } filtered && f.La.Verdict is { } fa && f.Lb.Verdict is { } fb) {
            return filtered.Times(fa).Times(fb);
        }
        (Expansion N, Expansion La, Expansion Lb) e = CompareNumerator<Expansion>(in a, in b, axis);
        return Expansion.SignOf(e.N).Times(Expansion.SignOf(e.La)).Times(Expansion.SignOf(e.Lb));
    }

    // --- [IMPLICIT_IN_CIRCUM]
    // Explicit-row lifts clear by lambda^2, and the lambda parity is a structural constant — in-circle EVEN
    // power 4 (Zero-gate, no flip), in-sphere ODD power 5 (one flip) — composed exactly, filter and oracle alike.
    public static Sign InCircle(Point3d a, Point3d b, Point3d c, in Implicit d, Axis axis) {
        if (d.IsExplicit) {
            return InCircle(Swizzled(a, axis), Swizzled(b, axis), Swizzled(c, axis), Swizzled(d.AsExplicit, axis));
        }
        (Interval Det, Interval Lambda) f = InCircleNumerator<Interval>(a, b, c, in d, axis);
        if (f.Det.Verdict is { } filtered && f.Lambda.Verdict is { } fl) return filtered.Times(fl).Times(fl);
        (Expansion Det, Expansion Lambda) e = InCircleNumerator<Expansion>(a, b, c, in d, axis);
        return RationalOracle.InCircum(e.Det, e.Lambda, lambdaDegree: 4);
    }

    public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, in Implicit e) {
        if (e.IsExplicit) return InSphere(a, b, c, d, e.AsExplicit);
        (Interval Det, Interval Lambda) f = InSphereNumerator<Interval>(a, b, c, d, in e);
        if (f.Det.Verdict is { } filtered && f.Lambda.Verdict is { } fl) return filtered.Times(fl);
        (Expansion Det, Expansion Lambda) x = InSphereNumerator<Expansion>(a, b, c, d, in e);
        return RationalOracle.InCircum(x.Det, x.Lambda, lambdaDegree: 5);
    }

    // --- [HOMOGENEOUS_FOLDS]
    static (T N, T La, T Lb, T Lc) OrientNumerator<T>(in Implicit a, in Implicit b, in Implicit c, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hc = c.Homogeneous<T>();
        (T ua, T va, T la) = (Pick(ha, axis.U), Pick(ha, axis.V), ha.Lambda);
        (T ub, T vb, T lb) = (Pick(hb, axis.U), Pick(hb, axis.V), hb.Lambda);
        (T uc, T vc, T lc) = (Pick(hc, axis.U), Pick(hc, axis.V), hc.Lambda);
        T n = ua.Mul(lc).Sub(uc.Mul(la)).Mul(vb.Mul(lc).Sub(vc.Mul(lb)))
            .Sub(va.Mul(lc).Sub(vc.Mul(la)).Mul(ub.Mul(lc).Sub(uc.Mul(lb))));
        return (n, la, lb, lc);
    }

    static (T N, T La, T Lb) CompareNumerator<T>(in Implicit a, in Implicit b, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        return (Pick(ha, axis.Key).Mul(hb.Lambda).Sub(Pick(hb, axis.Key).Mul(ha.Lambda)), ha.Lambda, hb.Lambda);
    }

    static (T Det, T Lambda) InCircleNumerator<T>(Point3d a, Point3d b, Point3d c, in Implicit d, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) h = d.Homogeneous<T>();
        (int u, int v) = (axis.U, axis.V);
        T l = h.Lambda;
        T eu = Pick(h, u).Sub(l.Scale(Axis.Coord(a, u)));
        T ev = Pick(h, v).Sub(l.Scale(Axis.Coord(a, v)));
        T bu = l.Mul(T.Diff(Axis.Coord(b, u), Axis.Coord(a, u)));
        T bv = l.Mul(T.Diff(Axis.Coord(b, v), Axis.Coord(a, v)));
        T cu = l.Mul(T.Diff(Axis.Coord(c, u), Axis.Coord(a, u)));
        T cv = l.Mul(T.Diff(Axis.Coord(c, v), Axis.Coord(a, v)));
        T eLift = eu.Mul(eu).Add(ev.Mul(ev));
        T bLift = bu.Mul(bu).Add(bv.Mul(bv));
        T cLift = cu.Mul(cu).Add(cv.Mul(cv));
        // Minor order composes the direct ladder's det4 sign: the anchored-at-a row reduction flips the
        // 3×3 once, folded into each negated 2×2 — so InCircle(a,b,c, Explicit d) equals the direct arm.
        T det = eLift.Mul(cu.Mul(bv).Sub(bu.Mul(cv)))
            .Add(bLift.Mul(eu.Mul(cv).Sub(cu.Mul(ev))))
            .Add(cLift.Mul(bu.Mul(ev).Sub(eu.Mul(bv))));
        return (det, l);
    }

    static (T Det, T Lambda) InSphereNumerator<T>(Point3d a, Point3d b, Point3d c, Point3d d, in Implicit e)
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
        T det = eLift.Mul(Tpi.Det3(bp, cp, dp))
            .Sub(bLift.Mul(Tpi.Det3((ex, ey, ez), cp, dp)))
            .Add(cLift.Mul(Tpi.Det3((ex, ey, ez), bp, dp)))
            .Sub(dLift.Mul(Tpi.Det3((ex, ey, ez), bp, cp)));
        return (det, l);

        static (T X, T Y, T Z) Row(Point3d p, Point3d anchor, T l) =>
            (l.Mul(T.Diff(p.X, anchor.X)), l.Mul(T.Diff(p.Y, anchor.Y)), l.Mul(T.Diff(p.Z, anchor.Z)));

        static T Lift((T X, T Y, T Z) r) => r.X.Mul(r.X).Add(r.Y.Mul(r.Y)).Add(r.Z.Mul(r.Z));
    }

    static T Pick<T>((T X, T Y, T Z, T Lambda) h, int ordinal) where T : struct, IExact<T> =>
        ordinal == 0 ? h.X : ordinal == 1 ? h.Y : h.Z;

    static Point3d Swizzled(Point3d p, Axis axis) =>
        new(Axis.Coord(p, axis.U), Axis.Coord(p, axis.V), 0.0);

    // --- [EXACT_FALLBACKS]
    // A raw rounded `a.X - c.X` would decide the sign of a DIFFERENT determinant exactly — hence the TwoSum leaf.
    static Expansion Diff(double p, double q) => Expansion.TwoSum(p, -q);

    static Sign Orient2DExact(Point3d a, Point3d b, Point3d c) =>
        Expansion.SignOf(Expansion.Difference(
            Expansion.Multiply(Diff(a.X, c.X), Diff(b.Y, c.Y)),
            Expansion.Multiply(Diff(a.Y, c.Y), Diff(b.X, c.X))));

    static Sign Orient3DExact(Point3d a, Point3d b, Point3d c, Point3d d) {
        Expansion bc = Expansion.Difference(Expansion.Multiply(Diff(b.X, d.X), Diff(c.Y, d.Y)), Expansion.Multiply(Diff(c.X, d.X), Diff(b.Y, d.Y)));
        Expansion ca = Expansion.Difference(Expansion.Multiply(Diff(c.X, d.X), Diff(a.Y, d.Y)), Expansion.Multiply(Diff(a.X, d.X), Diff(c.Y, d.Y)));
        Expansion ab = Expansion.Difference(Expansion.Multiply(Diff(a.X, d.X), Diff(b.Y, d.Y)), Expansion.Multiply(Diff(b.X, d.X), Diff(a.Y, d.Y)));
        Expansion det = Expansion.Sum(
            Expansion.Multiply(bc, Diff(a.Z, d.Z)),
            Expansion.Sum(Expansion.Multiply(ca, Diff(b.Z, d.Z)), Expansion.Multiply(ab, Diff(c.Z, d.Z))));
        return Expansion.SignOf(det);
    }

    static Sign InCircleExact(Point3d a, Point3d b, Point3d c, Point3d d) {
        (Expansion adx, Expansion ady) = (Diff(a.X, d.X), Diff(a.Y, d.Y));
        (Expansion bdx, Expansion bdy) = (Diff(b.X, d.X), Diff(b.Y, d.Y));
        (Expansion cdx, Expansion cdy) = (Diff(c.X, d.X), Diff(c.Y, d.Y));
        Expansion bc = Expansion.Difference(Expansion.Multiply(bdx, cdy), Expansion.Multiply(cdx, bdy));
        Expansion ca = Expansion.Difference(Expansion.Multiply(cdx, ady), Expansion.Multiply(adx, cdy));
        Expansion ab = Expansion.Difference(Expansion.Multiply(adx, bdy), Expansion.Multiply(bdx, ady));
        Expansion det = Expansion.Sum(
            Expansion.Multiply(Lift2(adx, ady), bc),
            Expansion.Sum(Expansion.Multiply(Lift2(bdx, bdy), ca), Expansion.Multiply(Lift2(cdx, cdy), ab)));
        return Expansion.SignOf(det);
    }

    static Sign InSphereExact(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) {
        Expansion abc = Minor3(b, c, d_: a, e), bcd = Minor3(c, d, d_: b, e), cda = Minor3(d, a, d_: c, e), dab = Minor3(a, b, d_: d, e);
        Expansion det = Expansion.Sum(
            Expansion.Difference(Expansion.Multiply(Lift3(d, e), abc), Expansion.Multiply(Lift3(c, e), dab)),
            Expansion.Difference(Expansion.Multiply(Lift3(b, e), cda), Expansion.Multiply(Lift3(a, e), bcd)));
        return Expansion.SignOf(det);
    }

    // --- [DOUBLE_DOUBLE_REFINE]
    // MULTILINEAR orient determinants align each difference vector to one exponent through `ddouble.AdjustScale`,
    // a positive per-row scale preserving a multilinear sign; the LIFTED in-circum determinants are
    // scale-INHOMOGENEOUS (a row scale does not commute with the quadratic lift), so their recompute runs raw.
    static Sign? RefineOrient2D(Point3d a, Point3d b, Point3d c) {
        (_, (ddouble acx, ddouble acy)) = ddouble.AdjustScale(0, ((ddouble)a.X - c.X, (ddouble)a.Y - c.Y));
        (_, (ddouble bcx, ddouble bcy)) = ddouble.AdjustScale(0, ((ddouble)b.X - c.X, (ddouble)b.Y - c.Y));
        ddouble detLeft = acx * bcy, detRight = acy * bcx;
        return ErrorBound.Orient2D.Refine(detLeft - detRight, ddouble.Abs(detLeft) + ddouble.Abs(detRight));
    }

    static Sign? RefineOrient3D(Point3d a, Point3d b, Point3d c, Point3d d) {
        (_, (ddouble adx, ddouble ady, ddouble adz)) = ddouble.AdjustScale(0, ((ddouble)a.X - d.X, (ddouble)a.Y - d.Y, (ddouble)a.Z - d.Z));
        (_, (ddouble bdx, ddouble bdy, ddouble bdz)) = ddouble.AdjustScale(0, ((ddouble)b.X - d.X, (ddouble)b.Y - d.Y, (ddouble)b.Z - d.Z));
        (_, (ddouble cdx, ddouble cdy, ddouble cdz)) = ddouble.AdjustScale(0, ((ddouble)c.X - d.X, (ddouble)c.Y - d.Y, (ddouble)c.Z - d.Z));
        ddouble bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, cdxady = cdx * ady, adxcdy = adx * cdy, adxbdy = adx * bdy, bdxady = bdx * ady;
        ddouble det = adz * (bdxcdy - cdxbdy) + bdz * (cdxady - adxcdy) + cdz * (adxbdy - bdxady);
        ddouble permanent =
            (ddouble.Abs(bdxcdy) + ddouble.Abs(cdxbdy)) * ddouble.Abs(adz)
            + (ddouble.Abs(cdxady) + ddouble.Abs(adxcdy)) * ddouble.Abs(bdz)
            + (ddouble.Abs(adxbdy) + ddouble.Abs(bdxady)) * ddouble.Abs(cdz);
        return ErrorBound.Orient3D.Refine(det, permanent);
    }

    static Sign? RefineInCircle(Point3d a, Point3d b, Point3d c, Point3d d) {
        ddouble adx = (ddouble)a.X - d.X, ady = (ddouble)a.Y - d.Y;
        ddouble bdx = (ddouble)b.X - d.X, bdy = (ddouble)b.Y - d.Y;
        ddouble cdx = (ddouble)c.X - d.X, cdy = (ddouble)c.Y - d.Y;
        ddouble bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, alift = adx * adx + ady * ady;
        ddouble cdxady = cdx * ady, adxcdy = adx * cdy, blift = bdx * bdx + bdy * bdy;
        ddouble adxbdy = adx * bdy, bdxady = bdx * ady, clift = cdx * cdx + cdy * cdy;
        ddouble det = alift * (bdxcdy - cdxbdy) + blift * (cdxady - adxcdy) + clift * (adxbdy - bdxady);
        ddouble permanent =
            (ddouble.Abs(bdxcdy) + ddouble.Abs(cdxbdy)) * alift
            + (ddouble.Abs(cdxady) + ddouble.Abs(adxcdy)) * blift
            + (ddouble.Abs(adxbdy) + ddouble.Abs(bdxady)) * clift;
        return ErrorBound.InCircle.Refine(det, permanent);
    }

    static Sign? RefineInSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) {
        ddouble aex = (ddouble)a.X - e.X, aey = (ddouble)a.Y - e.Y, aez = (ddouble)a.Z - e.Z;
        ddouble bex = (ddouble)b.X - e.X, bey = (ddouble)b.Y - e.Y, bez = (ddouble)b.Z - e.Z;
        ddouble cex = (ddouble)c.X - e.X, cey = (ddouble)c.Y - e.Y, cez = (ddouble)c.Z - e.Z;
        ddouble dex = (ddouble)d.X - e.X, dey = (ddouble)d.Y - e.Y, dez = (ddouble)d.Z - e.Z;
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
        return ErrorBound.InSphere.Refine(det, permanent);
    }

    // --- [LIFTS_AND_MINORS]
    static Expansion Lift2(Expansion x, Expansion y) =>
        Expansion.Sum(Expansion.Multiply(x, x), Expansion.Multiply(y, y));

    static Expansion Lift3(Point3d p, Point3d anchor) {
        (Expansion x, Expansion y, Expansion z) = (Diff(p.X, anchor.X), Diff(p.Y, anchor.Y), Diff(p.Z, anchor.Z));
        return Expansion.Sum(Expansion.Multiply(x, x), Expansion.Sum(Expansion.Multiply(y, y), Expansion.Multiply(z, z)));
    }

    static Expansion Minor3(Point3d p, Point3d q, Point3d d_, Point3d e) {
        Expansion pq = Expansion.Difference(Expansion.Multiply(Diff(p.X, e.X), Diff(q.Y, e.Y)), Expansion.Multiply(Diff(q.X, e.X), Diff(p.Y, e.Y)));
        Expansion qd = Expansion.Difference(Expansion.Multiply(Diff(q.X, e.X), Diff(d_.Y, e.Y)), Expansion.Multiply(Diff(d_.X, e.X), Diff(q.Y, e.Y)));
        Expansion dp = Expansion.Difference(Expansion.Multiply(Diff(d_.X, e.X), Diff(p.Y, e.Y)), Expansion.Multiply(Diff(p.X, e.X), Diff(d_.Y, e.Y)));
        return Expansion.Sum(
            Expansion.Multiply(pq, Diff(d_.Z, e.Z)),
            Expansion.Sum(Expansion.Multiply(qd, Diff(p.Z, e.Z)), Expansion.Multiply(dp, Diff(q.Z, e.Z))));
    }
}

public static class RationalOracle {
    // Topmost exact-rational tier: each ordinate widens losslessly through `(Fraction)double` and the
    // verdict is `Fraction.Sign` — never a `double`/`decimal` readout.
    public static Sign Orient2D(Point3d a, Point3d b, Point3d c) {
        Fraction acx = (Fraction)a.X - (Fraction)c.X, acy = (Fraction)a.Y - (Fraction)c.Y;
        Fraction bcx = (Fraction)b.X - (Fraction)c.X, bcy = (Fraction)b.Y - (Fraction)c.Y;
        return Sign.Of(Fraction.Subtract(acx * bcy, acy * bcx).Sign);
    }

    public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d) {
        Fraction adx = (Fraction)a.X - (Fraction)d.X, ady = (Fraction)a.Y - (Fraction)d.Y, adz = (Fraction)a.Z - (Fraction)d.Z;
        Fraction bdx = (Fraction)b.X - (Fraction)d.X, bdy = (Fraction)b.Y - (Fraction)d.Y, bdz = (Fraction)b.Z - (Fraction)d.Z;
        Fraction cdx = (Fraction)c.X - (Fraction)d.X, cdy = (Fraction)c.Y - (Fraction)d.Y, cdz = (Fraction)c.Z - (Fraction)d.Z;
        Fraction det = adz * (bdx * cdy - cdx * bdy) + bdz * (cdx * ady - adx * cdy) + cdz * (adx * bdy - bdx * ady);
        return Sign.Of(det.Sign);
    }

    public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d) {
        Fraction adx = (Fraction)a.X - (Fraction)d.X, ady = (Fraction)a.Y - (Fraction)d.Y;
        Fraction bdx = (Fraction)b.X - (Fraction)d.X, bdy = (Fraction)b.Y - (Fraction)d.Y;
        Fraction cdx = (Fraction)c.X - (Fraction)d.X, cdy = (Fraction)c.Y - (Fraction)d.Y;
        Fraction det =
            (adx * adx + ady * ady) * (bdx * cdy - cdx * bdy)
            + (bdx * bdx + bdy * bdy) * (cdx * ady - adx * cdy)
            + (cdx * cdx + cdy * cdy) * (adx * bdy - bdx * ady);
        return Sign.Of(det.Sign);
    }

    public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) {
        (Fraction x, Fraction y, Fraction z, Fraction lift) Row(Point3d p) {
            Fraction px = (Fraction)p.X - (Fraction)e.X, py = (Fraction)p.Y - (Fraction)e.Y, pz = (Fraction)p.Z - (Fraction)e.Z;
            return (px, py, pz, px * px + py * py + pz * pz);
        }
        ((Fraction x, Fraction y, Fraction z, Fraction lift) ra, (Fraction x, Fraction y, Fraction z, Fraction lift) rb, (Fraction x, Fraction y, Fraction z, Fraction lift) rc, (Fraction x, Fraction y, Fraction z, Fraction lift) rd) = (Row(a), Row(b), Row(c), Row(d));
        Fraction Det3((Fraction x, Fraction y, Fraction z, Fraction lift) u, (Fraction x, Fraction y, Fraction z, Fraction lift) v, (Fraction x, Fraction y, Fraction z, Fraction lift) w) =>
            u.x * (v.y * w.z - v.z * w.y) - u.y * (v.x * w.z - v.z * w.x) + u.z * (v.x * w.y - v.y * w.x);
        Fraction det = rd.lift * Det3(ra, rb, rc) - rc.lift * Det3(ra, rb, rd) + rb.lift * Det3(ra, rc, rd) - ra.lift * Det3(rb, rc, rd);
        return Sign.Of(det.Sign);
    }

    // `lambda.ToFraction().Sign` reads the denominator sign exactly — never a rounded estimate; `lambdaDegree`
    // is the structural lambda power the Sign.Times parity composes.
    public static Sign InCircum(Expansion det, Expansion lambda, int lambdaDegree) {
        Sign fl = Sign.Of(lambda.ToFraction().Sign);
        Sign parity = (lambdaDegree & 1) == 0 ? fl.Times(fl) : fl;
        return Sign.Of(det.ToFraction().Sign).Times(parity);
    }

    // `EFloat.FromDouble` sums the `Expansion` determinant to an exact binary value under `EContext.Unlimited`,
    // dyadic-lossless. A raised `FlagInexact` (unreachable for dyadic sums under `Unlimited`) is a FAILED proof
    // returning null, never a fabricated Zero — self-certifying exact.
    public static Sign? BinaryOf(Expansion e) {
        EContext exact = EContext.Unlimited.WithBlankFlags();
        EFloat acc = EFloat.Zero;
        foreach (double component in e.Components) acc = acc.Add(EFloat.FromDouble(component), exact);
        return exact.HasFlags && (exact.Flags & EContext.FlagInexact) != 0 ? null : Sign.Of(acc.Sign);
    }
}
```

## [03]-[INTERIOR_NUMERICS]

- Owner: `IExact<TSelf>` is the static-abstract exact-carrier algebra letting every construction and determinant polynomial be written ONCE and instantiated at both carriers; `Expansion` is the nonoverlapping floating-point expansion whose `Verdict` is ALWAYS determined; `Interval` is the directed-rounding `EFloat` bracket whose `Verdict` resolves exactly when the bracket excludes zero, at fixed bounded cost per operation — the software directed rounding the runtime cannot express through FPU mode switches; `ErrorBound` is the per-tier permanence filter-row table; `RationalOracle` is the exact adjudicator set, a PRIMARY `Fraction` beside an INDEPENDENT second source; `NumericsPolicy` owns the strict-IEEE-754 invariant, the interior-`double` scope, the error-bound constants, and the FMA capability gate.
- Cases: `IExact` is the algebra contract both carriers implement; `ErrorBound` carries one row per direct predicate, never a parallel threshold owner; `TwoProduct` carries the FMA row and the Dekker-split row, selected once by the RID capability gate, never per call site.
- Entry: `TwoProduct` is the exact two-component product — `FusedMultiplyAdd` on FMA-capable RIDs, the Dekker split otherwise, the branch a JIT-constant `HardwareFma` read once and dead-code-eliminated after tiering; `TwoSum` is the exact two-component sum with Knuth's rounding-error recovery; `ErrorBound.Of`/`Refine` are the two filter projections over one verdict protocol — a determinate `Sign` or `null`-escalate.
- Auto: the error-free transforms and Shewchuk's fast-expansion-sum and scale-expansion hold the nonoverlapping invariant, so `SignOf` reads the true sign from the top nonzero term; `Interval.Mul` brackets all four endpoint products under both directed contexts, so a resolved `Verdict` is a PROOF of the exact sign — the filter accepts or escalates, never mis-decides.
- Receipt: none — interior arithmetic, filters, and policy cross no public signature; the exact result is the `Sign` the predicate returns.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`), TYoshimura.DoubleDouble (106-bit refine), ExtendedNumerics.BigRational (the PRIMARY exact-rational tier over `BigInteger`), PeterO.Numbers (the interval tier and the INDEPENDENT adjudicator whose `EInteger` bignum shares no representation with `Fraction`'s `BigInteger`), BCL inbox (`FusedMultiplyAdd`, FMA/AdvSimd capability statics, `double.Epsilon`); no external geometry dependency.
- Growth: a new exact carrier (a hardware `Float128` bracket) is one `IExact` conformance every construction instantiates with zero polynomial edits; a new predicate's filter is one `ErrorBound` row; a longer computation grows the `Expansion` component buffer, never a parallel arbitrary-precision type; the interior-`double` scope widens to a new kernel only by naming it in `NumericsPolicy`.
- Boundary: `Expansion` is ONE owner for sign-exact arithmetic — a free `TwoSum`/`TwoProduct` set or a parallel `BigFloat`/`MPFR` type is the deleted form. `Interval` is ONE owner for the directed-rounding bracket — a per-predicate epsilon-inflation filter is the deleted form, the bracket sound by construction where an epsilon guess is a tuned lie. Both `TwoProduct` rows share one member gated once on `NumericsPolicy.HardwareFma`; a per-call-site FMA probe or a second product type is the deleted form. `ErrorBound` is the single permanence-threshold table, an inlined magic-number literal the named defect. `NumericsPolicy` states the strict-IEEE-754/RID invariant as the floor the forward-error coefficients derive against, and a runtime violating it is outside the support matrix, not a tolerated mode.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
// Struct-constrained static-abstract dispatch, JIT-specialized, zero boxing.
public interface IExact<TSelf> where TSelf : struct, IExact<TSelf> {
    static abstract TSelf Of(double value);
    static abstract TSelf Diff(double a, double b);
    static abstract TSelf Prod(double a, double b);
    TSelf Add(TSelf other);
    TSelf Sub(TSelf other);
    TSelf Mul(TSelf other);
    TSelf Scale(double exact);
    Sign? Verdict { get; }
}

// --- [CONSTANTS] --------------------------------------------------------------------------
// Strict-IEEE/RID invariant: the runtime evaluates double at binary64 — round-to-nearest-even, no FTZ/DAZ,
// no x87 spill — on every supported RID, the model the coefficients below derive against.
public static class NumericsPolicy {
    public const double Epsilon = 1.1102230246251565e-16;            // binary64 unit roundoff 2^-53
    public const double DoubleDoubleEpsilon = 6.162975822039155e-33; // 2^-107, the double-double unit roundoff
    public const double Splitter = 134_217_729.0;                    // 2^27+1 — the Dekker split constant the FMA-free row consumes

    public static readonly bool HardwareFma =
        System.Runtime.Intrinsics.X86.Fma.IsSupported || System.Runtime.Intrinsics.Arm.AdvSimd.Arm64.IsSupported;

    public const double Orient2DBound = (3.0 + 16.0 * Epsilon) * Epsilon;
    public const double Orient3DBound = (7.0 + 56.0 * Epsilon) * Epsilon;
    public const double InCircleBound = (10.0 + 96.0 * Epsilon) * Epsilon;
    public const double InSphereBound = (16.0 + 224.0 * Epsilon) * Epsilon;

    // 106-bit refine bounds: the same permanence coefficients re-derived against the double-double
    // roundoff, resolving a sign where the double filter brackets it but the determinant clears the 2^-107 floor.
    public const double Orient2DRefine = (3.0 + 16.0 * DoubleDoubleEpsilon) * DoubleDoubleEpsilon;
    public const double Orient3DRefine = (7.0 + 56.0 * DoubleDoubleEpsilon) * DoubleDoubleEpsilon;
    public const double InCircleRefine = (10.0 + 96.0 * DoubleDoubleEpsilon) * DoubleDoubleEpsilon;
    public const double InSphereRefine = (16.0 + 224.0 * DoubleDoubleEpsilon) * DoubleDoubleEpsilon;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly struct Expansion : IExact<Expansion> {
    private readonly double[] components;
    private readonly int length;

    private Expansion(double[] components, int length) { this.components = components; this.length = length; }

    public static Expansion Single(double value) => new(new[] { value }, value == 0.0 ? 0 : 1);

    // --- [TWO_SUM]
    public static Expansion TwoSum(double a, double b) { (double hi, double lo) = TwoSumCore(a, b); return Pair(lo, hi); }

    static (double Hi, double Lo) TwoSumCore(double a, double b) {
        double x = a + b;
        double bv = x - a;
        return (x, (a - (x - bv)) + (b - bv));
    }

    // --- [TWO_PRODUCT]
    public static Expansion TwoProduct(double a, double b) { (double hi, double lo) = TwoProductCore(a, b); return Pair(lo, hi); }

    static (double Hi, double Lo) TwoProductCore(double a, double b) {
        double x = a * b;
        if (NumericsPolicy.HardwareFma) return (x, Math.FusedMultiplyAdd(a, b, -x));
        (double ah, double al) = Split(a);
        (double bh, double bl) = Split(b);
        return (x, al * bl - (((x - ah * bh) - al * bh) - ah * bl));
    }

    static (double Hi, double Lo) Split(double value) {
        double c = NumericsPolicy.Splitter * value;
        double hi = c - (c - value);
        return (hi, value - hi);
    }

    // --- [GROW]
    public static Expansion Grow(Expansion e, double scalar) => Sum(e, Single(scalar));

    // --- [EXPANSION_SUM]
    // Carry/low splitting reads TwoSumCore directly — the Pair-compressed TwoSum would double-count the high word.
    public static Expansion Sum(Expansion left, Expansion right) {
        double[] merged = new double[left.length + right.length + 1];
        int li = 0, ri = 0, written = 0;
        double carry = 0.0;
        while (li < left.length || ri < right.length) {
            double next =
                li >= left.length ? right.components[ri++]
                : ri >= right.length ? left.components[li++]
                : Math.Abs(left.components[li]) < Math.Abs(right.components[ri]) ? left.components[li++]
                : right.components[ri++];
            (carry, double low) = TwoSumCore(carry, next);
            if (low != 0.0) merged[written++] = low;
        }
        if (carry != 0.0 || written == 0) merged[written++] = carry;
        return new Expansion(merged, written);
    }

    public static Expansion Difference(Expansion left, Expansion right) => Sum(left, Negate(right));

    // --- [SCALE_EXPANSION]
    // Shewchuk's linear scale-expansion, carry threading forward — never a quadratic Sum-per-component accumulate.
    public static Expansion Scale(Expansion e, double scalar) {
        if (e.length == 0 || scalar == 0.0) return Single(0.0);
        double[] scaled = new double[2 * e.length];
        int written = 0;
        (double q, double h) = TwoProductCore(e.components[0], scalar);
        if (h != 0.0) scaled[written++] = h;
        for (int i = 1; i < e.length; i++) {
            (double t, double tLo) = TwoProductCore(e.components[i], scalar);
            (double qMid, double h1) = TwoSumCore(q, tLo);
            if (h1 != 0.0) scaled[written++] = h1;
            (q, h) = TwoSumCore(t, qMid);
            if (h != 0.0) scaled[written++] = h;
        }
        if (q != 0.0 || written == 0) scaled[written++] = q;
        return new Expansion(scaled, written);
    }

    // --- [MULTIPLY]
    public static Expansion Multiply(Expansion left, Expansion right) {
        if (left.length == 0 || right.length == 0) return Single(0.0);
        Expansion acc = Scale(left, right.components[0]);
        for (int i = 1; i < right.length; i++) acc = Sum(acc, Scale(left, right.components[i]));
        return acc;
    }

    public static Expansion Negate(Expansion e) {
        double[] flipped = new double[e.length];
        for (int i = 0; i < e.length; i++) flipped[i] = -e.components[i];
        return new Expansion(flipped, e.length);
    }

    // --- [ESTIMATE]
    public double Estimate() {
        double acc = 0.0;
        for (int i = 0; i < length; i++) acc += components[i];
        return acc;
    }

    // --- [SIGN]
    public static Sign SignOf(Expansion e) {
        for (int i = e.length - 1; i >= 0; i--)
            if (e.components[i] != 0.0) return Sign.Of(e.components[i]);
        return Sign.Zero;
    }

    // --- [EXACT_ALGEBRA]
    // Leaf lifts are error-free transforms, so the polynomial instantiated at Expansion is bit-for-bit sign-exact.
    static Expansion IExact<Expansion>.Of(double value) => Single(value);
    static Expansion IExact<Expansion>.Diff(double a, double b) => TwoSum(a, -b);
    static Expansion IExact<Expansion>.Prod(double a, double b) => TwoProduct(a, b);
    Expansion IExact<Expansion>.Add(Expansion other) => Sum(this, other);
    Expansion IExact<Expansion>.Sub(Expansion other) => Difference(this, other);
    Expansion IExact<Expansion>.Mul(Expansion other) => Multiply(this, other);
    Expansion IExact<Expansion>.Scale(double exact) => Scale(this, exact);
    Sign? IExact<Expansion>.Verdict => SignOf(this);

    // --- [RATIONAL_LIFT]
    // Each nonoverlapping component is a lossless `(Fraction)double`, so the exact value crosses to the BigInteger tier unrounded.
    public Fraction ToFraction() {
        Fraction acc = Fraction.Zero;
        for (int i = 0; i < length; i++) acc = Fraction.Add(acc, (Fraction)components[i]);
        return acc;
    }

    // Nonoverlapping components in magnitude order — the exact bridge `RationalOracle.BinaryOf` lifts losslessly.
    public ReadOnlySpan<double> Components => components.AsSpan(0, length);

    static Expansion Pair(double small, double large) =>
        small == 0.0 ? Single(large) : new Expansion(new[] { small, large }, 2);
}

// 53-bit EFloat endpoints widen conservatively under frozen Floor/Ceiling contexts — fixed cost per
// operation, no BigInteger growth.
public readonly struct Interval : IExact<Interval> {
    static readonly EContext Down = EContext.ForPrecisionAndRounding(53, ERounding.Floor).WithPrecisionInBits(true);
    static readonly EContext Up = EContext.ForPrecisionAndRounding(53, ERounding.Ceiling).WithPrecisionInBits(true);

    public readonly EFloat Lo;
    public readonly EFloat Hi;

    Interval(EFloat lo, EFloat hi) { Lo = lo; Hi = hi; }

    public static Interval Of(double value) => new(EFloat.FromDouble(value), EFloat.FromDouble(value));

    public static Interval Diff(double a, double b) =>
        new(EFloat.FromDouble(a).Subtract(EFloat.FromDouble(b), Down), EFloat.FromDouble(a).Subtract(EFloat.FromDouble(b), Up));

    public static Interval Prod(double a, double b) =>
        new(EFloat.FromDouble(a).Multiply(EFloat.FromDouble(b), Down), EFloat.FromDouble(a).Multiply(EFloat.FromDouble(b), Up));

    public Interval Add(Interval other) => new(Lo.Add(other.Lo, Down), Hi.Add(other.Hi, Up));

    public Interval Sub(Interval other) => new(Lo.Subtract(other.Hi, Down), Hi.Subtract(other.Lo, Up));

    public Interval Mul(Interval other) {
        EFloat lo = Least(Lo.Multiply(other.Lo, Down), Lo.Multiply(other.Hi, Down), Hi.Multiply(other.Lo, Down), Hi.Multiply(other.Hi, Down));
        EFloat hi = Greatest(Lo.Multiply(other.Lo, Up), Lo.Multiply(other.Hi, Up), Hi.Multiply(other.Lo, Up), Hi.Multiply(other.Hi, Up));
        return new Interval(lo, hi);
    }

    public Interval Scale(double exact) => Mul(Of(exact));

    public Sign? Verdict =>
        Lo.Sign > 0 ? Sign.Positive
        : Hi.Sign < 0 ? Sign.Negative
        : Lo.IsZero && Hi.IsZero ? Sign.Zero
        : null;

    static EFloat Least(EFloat a, EFloat b, EFloat c, EFloat d) => Min(Min(a, b), Min(c, d));
    static EFloat Greatest(EFloat a, EFloat b, EFloat c, EFloat d) => Max(Max(a, b), Max(c, d));
    static EFloat Min(EFloat a, EFloat b) => a.CompareTo(b) <= 0 ? a : b;
    static EFloat Max(EFloat a, EFloat b) => a.CompareTo(b) >= 0 ? a : b;
}

public sealed record ErrorBound(double Coefficient, double RefineCoefficient) {
    public static readonly ErrorBound Orient2D = new(NumericsPolicy.Orient2DBound, NumericsPolicy.Orient2DRefine);
    public static readonly ErrorBound Orient3D = new(NumericsPolicy.Orient3DBound, NumericsPolicy.Orient3DRefine);
    public static readonly ErrorBound InCircle = new(NumericsPolicy.InCircleBound, NumericsPolicy.InCircleRefine);
    public static readonly ErrorBound InSphere = new(NumericsPolicy.InSphereBound, NumericsPolicy.InSphereRefine);

    public Sign? Of(double det, double permanent) =>
        Math.Abs(det) > Coefficient * permanent ? Sign.Of(det) : null;

    public Sign? Refine(ddouble det, ddouble permanent) =>
        ddouble.Abs(det) > RefineCoefficient * permanent ? Sign.Of(ddouble.Sign(det)) : null;
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

- [FMA_FREE_TWOPRODUCT]-[OPEN]: does the Dekker-split `TwoProduct` row emit error components bit-identical to the FMA row on a genuinely FMA-free RID; verify by a differential run of both rows on an FMA-free target.
