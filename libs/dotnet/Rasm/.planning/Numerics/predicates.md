# [RASM_NUMERICS_PREDICATES]

`Predicate` owns the adaptive-precision exact-sign floor every higher geometry owner composes, returning a total `Sign` for explicit and constructed points alike. Constructed intersection points travel as defining-point carriage with exact coordinates derived on demand, so no rounded coordinate enters a predicate and rounding happens once at the consumer's emission seam.

Every fold is one polynomial instantiated at both the `Interval` filter and `Expansion` exact carriers through the `IExact<TSelf>` algebra, so the filter never tests a different polynomial than the exact branch decides. Degeneracy is a verdict of that algebra and never of a rounded readout: `Sign.Zero` from the exact fold, `Expansion.SignOf(Lambda)` at the emission seam, and a refusal on the `Op` rail where the family's one float-typed admission cannot pick a plane at all — no branch on this page reads a floating result's finiteness or magnitude to decide whether a configuration is degenerate (scar `EXACT_ORACLE_INFERRED_FROM_RESULT`).

## [01]-[INDEX]

- [02]-[ROBUST_PREDICATES]: `Predicate` folds the direct ladders and the constructed-point family to a total `Sign`, and clips one convex ring against one `Halfplane` on that same exact side test.
- [03]-[INTERIOR_NUMERICS]: `IExact` carriers stack the filter, refine, and exact-rational tiers under `NumericsPolicy`.

## [02]-[ROBUST_PREDICATES]

- Owner: `Sign` `[SmartEnum<int>]` is the closed ternary verdict every predicate returns, carrying the `Flip`/`Times` parity algebra; `Axis` `[SmartEnum<int>]` is the closed coordinate vocabulary and the ONE generator every axis-projected member spans its three planes over, its `U`/`V` deferred row references and its `Read`/`Along`/`Pick` coordinate columns replacing every ordinal a consumer once re-resolved; `Implicit` `[Union<Point3d, Ssi, Lpi, Tpi>]` carries a constructed point as DEFINING POINTS ONLY, its exact homogeneous coordinates derived on demand through `Homogeneous<T>`; `Halfplane` `[Union]` is the cut a ring clip folds against, its `Frame` case reading the exact `Orient2D` ladder and its `Affine` case the functional a caller already holds; `Predicate` is the ONE static surface owning the direct ladders, the implicit folds, and the one half-plane ring clip built over them.
- Cases: `Sign`, `Axis`, and the four `Implicit` constructions are the closed vocabularies; `Predicate` carries the four direct members `Orient2D`/`Orient3D`/`InCircle`/`InSphere` beside `Orient2D(in Implicit, in Implicit, in Implicit, Axis)` and `Orient3D(in Implicit, in Implicit, in Implicit, in Implicit)` spanning every explicit/implicit combination × projection plane, `Compare(in Implicit, in Implicit, Axis)` the exact per-coordinate order key, the in-circum `InCircle`/`InSphere` implicit queries, and `ClipHalfplane` the one Sutherland-Hodgman ring fold over `Halfplane.Side`; `Axis` carries `Read`/`Along` for points and vectors, `Pick` for the exact homogeneous quadruple, the `Basis` lift column, and `BitKey`, the exact signed-zero-folded IEEE ordinal every arena interns explicit coordinates by.
- Entry: every VERDICT member is a total pure exact function returning `Sign` with no rail; the two non-verdict members state their own rail — `Axis.DominantOf` its plane-selection refusal and `ClipHalfplane` its span-arity refusal — and nothing else on the family carries one. The raw-`double` direct entries are the core cross-package consumers bind, since the Compute lane bars host value types on interior signatures, and the `Point3d` overloads are the ONE adaptation seam — every refine and exact tier below an entry takes that entry's own raw doubles, so no escalation rebuilds the host value the entry exists to keep out. Implicit entries discriminate on the carrier's case shape and the `Axis` row. Degenerate constructions (`lambda = 0`) yield `Sign.Zero` through the `Times` flip algebra, the degeneracy witness the consumer's recovery reads. `Axis.DominantOf` is the family's three-arity `[BoundaryAdapter]` admission — the only member taking host vectors and the only one carrying the `Op? key = null` tail every other member refuses.
- Auto: each direct member filters in `double`, refines at 106-bit `ddouble`, then folds the sign-exact `Expansion`; each implicit member opens at the `Interval` directed-rounding filter over the SAME polynomial — a rounded-coordinate `double` filter cannot exist for a point whose coordinates are derived, so there is no cheaper tier below it — escalating the indeterminate residue to `Expansion` and the in-circum queries on to `RationalOracle.InCircum`. Every member walks its tiers inline as one `??`-chain over the uniform `Sign?`-or-escalate protocol, allocation-free with no captured thunk; every tier is monotone and sign-consistent, so the verdict is always the true sign.
- Law: `Axis.DominantOf` SELECTS a projection plane and never decides degeneracy — its float cross/Newell normal is a heuristic barred from every exact carrier, and where that normal is non-finite, zero, or over the split ceiling it REFUSES onto the `Op` rail rather than silently taking a max component a NaN wins. `Implicit.Round()` decides existence by `Expansion.SignOf(Lambda)` on the defining points, so a construction with no point returns `None`; absence never crosses the boundary as a non-finite `Point3d` sentinel for a consumer's freeze gate to catch.
- Law: a `Sign` verdict carries no residual. Two emission-side materializations exist and both are evidence-bearing: `ClipHalfplane` writes clipped coordinates beside a per-vertex FABRICATED mark, so a midpoint standing in for a vanished crossing is never mistaken for a measured one, and `Implicit.Round()`, whose `Option<Point3d>` a consumer emits at its own seam and never a value any predicate reads back; `None` folds the two causes a consumer answers identically — the exact non-existence the lambda sign proves and an over-range double readout at the rounding step — the exact cause being the one the predicate rail already reports as `Sign.Zero`.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`), RhinoCommon (`Point3d`), TYoshimura.DoubleDouble (106-bit refine), ExtendedNumerics.BigRational (exact-rational oracle), PeterO.Numbers (interval filter, second-source adjudicator), Rasm.Domain (`Op`/`KernelFault.InvalidInput`, the `DominantOf` refusal rail), BCL inbox (`FusedMultiplyAdd`, intrinsics probes, `BigInteger`).
- Growth: a new implicit construction is one `Implicit` case carrying its defining points and one `Homogeneous<T>` arm, every fold and emission member widening by that arm with the generated dispatch breaking loudly; a new direct predicate is one member and one `ErrorBound` row; a new cut shape is one `Halfplane` case with its `Side`/`Offset` arms and zero clip edits; a new precision stage is one `IExact` carrier with one `??`-chain link in each member tail, since the ladder IS the chain and a parallel tier vocabulary beside it names stages nothing reads. The multi-implicit in-circum widening is ONE derivation away and states it here: scaling homogeneous row i by `(la*li)^2` makes every entry an exact polynomial and the determinant scale an EVEN power, so the verdict composes each lambda's sign twice and a zero one gates to Zero — landing it is one `*Numerator` widening per member with no new surface, held only until its four-way differential proves the parity.
- Boundary: the whole family lives on ONE `Predicate` static owner — a per-predicate class or a `FastOrient2D`/`ExactOrient2D` pair is the deleted form. Verdicts are the closed `Sign` and a raw `int`/`double` sign crossing a public signature is the named defect; coordinates are `Point3d` read at the seam, a domain-local point struct the deleted form. Constructed points travel as `Implicit` defining-point carriage rounded ONCE at `Round()` — a `Denominator`-as-`double` field or an `Estimate()` inside an exact carrier is the named robustness defect; `ClipHalfplane`'s emitted crossings are the one deliberate exception, a ring fold whose product IS coordinates, and they carry their fabrication mark precisely because no exact carriage survives the divide — and derived `Plane` inputs are dead, so a three-plane point is its NINE points. `DominantOf` is the ONE geometry admission, its `NumericsPolicy.SplitCeiling` gate the exact carriers' operand domain, so an over-ceiling coordinate refuses on the `Op` rail rather than reaching a `TwoProduct` row that hands back a NaN error component; every leaf difference rides the error-free `IExact.Diff`, a raw `double` subtraction wrapped in an exact type the deleted rounded-leaf form. Loosening a filter band to pass a near-degenerate case instead of taking the exact branch is the named correctness defect — a sign verdict is exact or it is a defect. `Implicit` keeps its bare name against every upper-folder twin — the discriminant is the CARRIAGE regime, a defining-point construction whose coordinates are derived exactly on demand, where an upper folder's same-named type carries evaluated values; a rename to `ImplicitPoint` names the payload the union already types.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Numerics;
using DoubleDouble;
using ExtendedNumerics;
using LanguageExt;
using PeterO.Numbers;
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
    public static Sign Of(int value) => value < 0 ? Negative : value > 0 ? Positive : Zero;

    public Sign Flip => Switch(negative: static _ => Positive, zero: static _ => Zero, positive: static _ => Negative);
    public Sign Times(Sign other) => Of(Key * other.Key);
}

[SmartEnum<int>]
public sealed partial class Axis {
    public static readonly Axis X = new(0, u: static () => Y, v: static () => Z, basis: Vector3d.XAxis, read: static p => p.X, along: static d => d.X);
    public static readonly Axis Y = new(1, u: static () => Z, v: static () => X, basis: Vector3d.YAxis, read: static p => p.Y, along: static d => d.Y);
    public static readonly Axis Z = new(2, u: static () => X, v: static () => Y, basis: Vector3d.ZAxis, read: static p => p.Z, along: static d => d.Z);

    public Vector3d Basis { get; }

    [UseDelegateFromConstructor] private partial Axis NormalU();
    [UseDelegateFromConstructor] private partial Axis NormalV();
    public Axis U => NormalU();
    public Axis V => NormalV();
    [UseDelegateFromConstructor] public partial double Read(Point3d p);
    [UseDelegateFromConstructor] public partial double Along(Vector3d d);

    internal T Pick<T>(in (T X, T Y, T Z, T Lambda) h) where T : struct, IExact<T> =>
        Key == 0 ? h.X : Key == 1 ? h.Y : h.Z;

    public static (long X, long Y, long Z) BitKey(Point3d p) => (Bits(p.X), Bits(p.Y), Bits(p.Z));

    static long Bits(double v) => BitConverter.DoubleToInt64Bits(v == 0.0 ? 0.0 : v);

    [BoundaryAdapter]
    public static Fin<Axis> DominantOf(Vector3d d, Op? key = null) =>
        d.IsValid && !d.IsZero && Representable(d)
            ? Fin.Succ(Dominant(Math.Abs(d.X), Math.Abs(d.Y), Math.Abs(d.Z)))
            : Fin.Fail<Axis>(key.OrDefault().InvalidInput());

    [BoundaryAdapter]
    public static Fin<Axis> DominantOf(Point3d a, Point3d b, Point3d c, Op? key = null) =>
        DominantOf(Vector3d.CrossProduct(b - a, c - a), key);

    [BoundaryAdapter]
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

    static bool Representable(Vector3d d) =>
        Math.Abs(d.X) <= NumericsPolicy.SplitCeiling
        && Math.Abs(d.Y) <= NumericsPolicy.SplitCeiling
        && Math.Abs(d.Z) <= NumericsPolicy.SplitCeiling;
}

[Union<Point3d, Ssi, Lpi, Tpi>(T1Name = "Explicit", T2Name = "Ssi", T3Name = "Lpi", T4Name = "Tpi")]
public readonly partial struct Implicit {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> =>
        Switch(
            @explicit: static e => (T.Of(e.X), T.Of(e.Y), T.Of(e.Z), T.Of(1.0)),
            ssi:       static s => s.Homogeneous<T>(),
            lpi:       static l => l.Homogeneous<T>(),
            tpi:       static t => t.Homogeneous<T>());

    public Option<Point3d> Round() =>
        IsExplicit ? Some(AsExplicit) : Materialized(Homogeneous<Expansion>());

    static Option<Point3d> Materialized((Expansion X, Expansion Y, Expansion Z, Expansion Lambda) h) {
        if (Expansion.SignOf(h.Lambda) == Sign.Zero) { return Option<Point3d>.None; }
        double lambda = h.Lambda.Estimate();
        Point3d rounded = new(h.X.Estimate() / lambda, h.Y.Estimate() / lambda, h.Z.Estimate() / lambda);
        return rounded.IsValid ? Some(rounded) : Option<Point3d>.None;
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Ssi(Point3d P, Point3d Q, Point3d R, Point3d S, Axis Plane) {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> {
        (Axis u, Axis v) = (Plane.U, Plane.V);
        T lambda = T.Diff(u.Read(S), u.Read(R)).Mul(T.Diff(v.Read(Q), v.Read(P)))
            .Sub(T.Diff(v.Read(S), v.Read(R)).Mul(T.Diff(u.Read(Q), u.Read(P))));
        T n = T.Diff(u.Read(S), u.Read(P)).Mul(T.Diff(v.Read(Q), v.Read(P)))
            .Sub(T.Diff(v.Read(S), v.Read(P)).Mul(T.Diff(u.Read(Q), u.Read(P))));
        return (Parametric(lambda, n, P.X, Q.X), Parametric(lambda, n, P.Y, Q.Y), Parametric(lambda, n, P.Z, Q.Z), lambda);

        static T Parametric(T lambda, T n, double at, double head) =>
            lambda.Scale(at).Add(n.Mul(T.Diff(head, at)));
    }
}

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Halfplane {
    private Halfplane() { }

    public sealed record Frame(Point3d Origin, Point3d Along, Axis Plane) : Halfplane;
    public sealed record Affine(Vector3d Normal, double Constant) : Halfplane;

    public Sign Side(Point3d p) =>
        Switch(
            state:  p,
            frame:  static (q, f) => Predicate.Orient2D(
                        f.Plane.U.Read(f.Origin), f.Plane.V.Read(f.Origin),
                        f.Plane.U.Read(f.Along), f.Plane.V.Read(f.Along),
                        f.Plane.U.Read(q), f.Plane.V.Read(q)),
            affine: static (q, a) => Sign.Of((a.Normal * (Vector3d)q) - a.Constant));

    public double Offset(Point3d p) =>
        Switch(
            state:  p,
            frame:  static (q, f) => Cross(f.Origin, f.Along, q, f.Plane),
            affine: static (q, a) => (a.Normal * (Vector3d)q) - a.Constant);

    static double Cross(Point3d origin, Point3d along, Point3d q, Axis plane) {
        (Axis u, Axis v) = (plane.U, plane.V);
        (double ou, double ov) = (u.Read(origin) - u.Read(q), v.Read(origin) - v.Read(q));
        (double au, double av) = (u.Read(along) - u.Read(q), v.Read(along) - v.Read(q));
        return (ou * av) - (ov * au);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Predicate {
    // --- [COORDINATE_CORE]

    // --- [ORIENT_2D]
    public static Sign Orient2D(Point3d a, Point3d b, Point3d c) => Orient2D(a.X, a.Y, b.X, b.Y, c.X, c.Y);

    public static Sign Orient2D(double ax, double ay, double bx, double by, double cx, double cy) {
        double acx = ax - cx, bcx = bx - cx, acy = ay - cy, bcy = by - cy;
        double detLeft = acx * bcy, detRight = acy * bcx;
        double det = detLeft - detRight;
        double detsum = Math.Abs(detLeft) + Math.Abs(detRight);
        return ErrorBound.Orient2D.Of(det, detsum)
            ?? RefineOrient2D(ax, ay, bx, by, cx, cy)
            ?? Orient2DExact(ax, ay, bx, by, cx, cy);
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
            ?? RefineOrient3D(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz)
            ?? Orient3DExact(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz);
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
            ?? RefineInCircle(ax, ay, bx, by, cx, cy, dx, dy)
            ?? InCircleExact(ax, ay, bx, by, cx, cy, dx, dy);
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
            ?? RefineInSphere(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz, ex, ey, ez)
            ?? InSphereExact(ax, ay, az, bx, by, bz, cx, cy, cz, dx, dy, dz, ex, ey, ez);
    }

    // --- [IMPLICIT_ORIENT]
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

    public static Sign Orient3D(in Implicit a, in Implicit b, in Implicit c, in Implicit d) {
        if (a.IsExplicit && b.IsExplicit && c.IsExplicit && d.IsExplicit) {
            return Orient3D(a.AsExplicit, b.AsExplicit, c.AsExplicit, d.AsExplicit);
        }
        (Interval N, Interval La, Interval Lb, Interval Lc, Interval Ld) f = OrientNumerator3<Interval>(in a, in b, in c, in d);
        if (f.N.Verdict is { } filtered && f.La.Verdict is { } fa && f.Lb.Verdict is { } fb
            && f.Lc.Verdict is { } fc && f.Ld.Verdict is { } fd) {
            return filtered.Times(fa).Times(fb).Times(fc).Times(fd);
        }
        (Expansion N, Expansion La, Expansion Lb, Expansion Lc, Expansion Ld) e = OrientNumerator3<Expansion>(in a, in b, in c, in d);
        return Expansion.SignOf(e.N)
            .Times(Expansion.SignOf(e.La)).Times(Expansion.SignOf(e.Lb))
            .Times(Expansion.SignOf(e.Lc)).Times(Expansion.SignOf(e.Ld));
    }

    // --- [IMPLICIT_COMPARE]
    public static Sign Compare(in Implicit a, in Implicit b, Axis axis) {
        if (a.IsExplicit && b.IsExplicit) {
            return Sign.Of(axis.Read(a.AsExplicit).CompareTo(axis.Read(b.AsExplicit)));
        }
        (Interval N, Interval La, Interval Lb) f = CompareNumerator<Interval>(in a, in b, axis);
        if (f.N.Verdict is { } filtered && f.La.Verdict is { } fa && f.Lb.Verdict is { } fb) {
            return filtered.Times(fa).Times(fb);
        }
        (Expansion N, Expansion La, Expansion Lb) e = CompareNumerator<Expansion>(in a, in b, axis);
        return Expansion.SignOf(e.N).Times(Expansion.SignOf(e.La)).Times(Expansion.SignOf(e.Lb));
    }

    // --- [IMPLICIT_IN_CIRCUM]
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

    // --- [HALFPLANE_CLIP]
    public static Fin<(int Written, int Fabricated)> ClipHalfplane(
        ReadOnlySpan<Point3d> ring, ReadOnlySpan<int> labels, Halfplane cut, Sign keep, double band, double denomFloor,
        int cutLabel, Span<Point3d> target, Span<int> targetLabels, Span<bool> targetFabricated) {
        int room = ring.Length + 2;
        if (ring.Length < 3 || labels.Length < ring.Length
            || target.Length < room || targetLabels.Length < room || targetFabricated.Length < room) {
            return Fin.Fail<(int, int)>(new GeometryFault.DegenerateInput(Kind.Polyline, None, "clip ring or target span too short"));
        }
        int written = 0, fabricated = 0;
        Point3d prev = ring[^1];
        (Sign sidePrev, double offPrev, int labelPrev) = (cut.Side(prev), cut.Offset(prev), labels[ring.Length - 1]);
        for (int k = 0; k < ring.Length; k++) {
            Point3d cur = ring[k];
            (Sign sideCur, double offCur) = (cut.Side(cur), cut.Offset(cur));
            if (sidePrev.Times(sideCur) == Sign.Negative) {
                double denom = offPrev - offCur;
                bool forged = Math.Abs(denom) < denomFloor;
                double t = forged ? 0.5 : offPrev / denom;
                (targetLabels[written], targetFabricated[written]) = (sidePrev == keep ? cutLabel : labelPrev, forged);
                target[written++] = prev + (t * (cur - prev));
                fabricated += forged ? 1 : 0;
            }
            if (sideCur != keep.Flip || Math.Abs(offCur) <= band) {
                (targetLabels[written], targetFabricated[written]) = (labels[k], false);
                target[written++] = cur;
            }
            (prev, sidePrev, offPrev, labelPrev) = (cur, sideCur, offCur, labels[k]);
        }
        return Fin.Succ((written, fabricated));
    }

    // --- [HOMOGENEOUS_FOLDS]
    static (T N, T La, T Lb, T Lc) OrientNumerator<T>(in Implicit a, in Implicit b, in Implicit c, Axis axis)
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

    static (T N, T La, T Lb, T Lc, T Ld) OrientNumerator3<T>(in Implicit a, in Implicit b, in Implicit c, in Implicit d)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hc = c.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hd = d.Homogeneous<T>();
        (T X, T Y, T Z) rb = Row(in ha, in hb);
        (T X, T Y, T Z) rc = Row(in ha, in hc);
        (T X, T Y, T Z) rd = Row(in ha, in hd);
        return (Tpi.Det3(rb, rc, rd), ha.Lambda, hb.Lambda, hc.Lambda, hd.Lambda);

        static (T X, T Y, T Z) Row(in (T X, T Y, T Z, T Lambda) anchor, in (T X, T Y, T Z, T Lambda) point) =>
            (point.X.Mul(anchor.Lambda).Sub(anchor.X.Mul(point.Lambda)),
             point.Y.Mul(anchor.Lambda).Sub(anchor.Y.Mul(point.Lambda)),
             point.Z.Mul(anchor.Lambda).Sub(anchor.Z.Mul(point.Lambda)));
    }

    static (T N, T La, T Lb) CompareNumerator<T>(in Implicit a, in Implicit b, Axis axis)
        where T : struct, IExact<T> {
        (T X, T Y, T Z, T Lambda) ha = a.Homogeneous<T>();
        (T X, T Y, T Z, T Lambda) hb = b.Homogeneous<T>();
        return (axis.Pick(in ha).Mul(hb.Lambda).Sub(axis.Pick(in hb).Mul(ha.Lambda)), ha.Lambda, hb.Lambda);
    }

    static (T Det, T Lambda) InCircleNumerator<T>(Point3d a, Point3d b, Point3d c, in Implicit d, Axis axis)
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

    static Point3d Swizzled(Point3d p, Axis axis) => new(axis.U.Read(p), axis.V.Read(p), 0.0);

    // --- [EXACT_FALLBACKS]
    static Sign Orient2DExact(double ax, double ay, double bx, double by, double cx, double cy) =>
        Expansion.SignOf(Expansion.Difference(
            Expansion.Multiply(Expansion.Diff(ax, cx), Expansion.Diff(by, cy)),
            Expansion.Multiply(Expansion.Diff(ay, cy), Expansion.Diff(bx, cx))));

    static Sign Orient3DExact(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
        Expansion bc = Expansion.Difference(Expansion.Multiply(Expansion.Diff(bx, dx), Expansion.Diff(cy, dy)), Expansion.Multiply(Expansion.Diff(cx, dx), Expansion.Diff(by, dy)));
        Expansion ca = Expansion.Difference(Expansion.Multiply(Expansion.Diff(cx, dx), Expansion.Diff(ay, dy)), Expansion.Multiply(Expansion.Diff(ax, dx), Expansion.Diff(cy, dy)));
        Expansion ab = Expansion.Difference(Expansion.Multiply(Expansion.Diff(ax, dx), Expansion.Diff(by, dy)), Expansion.Multiply(Expansion.Diff(bx, dx), Expansion.Diff(ay, dy)));
        Expansion det = Expansion.Sum(
            Expansion.Multiply(bc, Expansion.Diff(az, dz)),
            Expansion.Sum(Expansion.Multiply(ca, Expansion.Diff(bz, dz)), Expansion.Multiply(ab, Expansion.Diff(cz, dz))));
        return Expansion.SignOf(det);
    }

    static Sign InCircleExact(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
        (Expansion adx, Expansion ady) = (Expansion.Diff(ax, dx), Expansion.Diff(ay, dy));
        (Expansion bdx, Expansion bdy) = (Expansion.Diff(bx, dx), Expansion.Diff(by, dy));
        (Expansion cdx, Expansion cdy) = (Expansion.Diff(cx, dx), Expansion.Diff(cy, dy));
        Expansion bc = Expansion.Difference(Expansion.Multiply(bdx, cdy), Expansion.Multiply(cdx, bdy));
        Expansion ca = Expansion.Difference(Expansion.Multiply(cdx, ady), Expansion.Multiply(adx, cdy));
        Expansion ab = Expansion.Difference(Expansion.Multiply(adx, bdy), Expansion.Multiply(bdx, ady));
        Expansion det = Expansion.Sum(
            Expansion.Multiply(Lift2(adx, ady), bc),
            Expansion.Sum(Expansion.Multiply(Lift2(bdx, bdy), ca), Expansion.Multiply(Lift2(cdx, cdy), ab)));
        return Expansion.SignOf(det);
    }

    static Sign InSphereExact(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
        (Point3d a, Point3d b, Point3d c) = (new(ax, ay, az), new(bx, by, bz), new(cx, cy, cz));
        (Point3d d, Point3d e) = (new(dx, dy, dz), new(ex, ey, ez));
        Expansion abc = Minor3(b, c, d_: a, e), bcd = Minor3(c, d, d_: b, e), cda = Minor3(d, a, d_: c, e), dab = Minor3(a, b, d_: d, e);
        Expansion det = Expansion.Sum(
            Expansion.Difference(Expansion.Multiply(Lift3(d, e), abc), Expansion.Multiply(Lift3(c, e), dab)),
            Expansion.Difference(Expansion.Multiply(Lift3(b, e), cda), Expansion.Multiply(Lift3(a, e), bcd)));
        return Expansion.SignOf(det);
    }

    // --- [DOUBLE_DOUBLE_REFINE]
    static Sign? RefineOrient2D(double ax, double ay, double bx, double by, double cx, double cy) {
        (_, (ddouble acx, ddouble acy)) = ddouble.AdjustScale(0, ((ddouble)ax - cx, (ddouble)ay - cy));
        (_, (ddouble bcx, ddouble bcy)) = ddouble.AdjustScale(0, ((ddouble)bx - cx, (ddouble)by - cy));
        ddouble detLeft = acx * bcy, detRight = acy * bcx;
        return ErrorBound.Orient2D.Refine(detLeft - detRight, ddouble.Abs(detLeft) + ddouble.Abs(detRight));
    }

    static Sign? RefineOrient3D(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz) {
        (_, (ddouble adx, ddouble ady, ddouble adz)) = ddouble.AdjustScale(0, ((ddouble)ax - dx, (ddouble)ay - dy, (ddouble)az - dz));
        (_, (ddouble bdx, ddouble bdy, ddouble bdz)) = ddouble.AdjustScale(0, ((ddouble)bx - dx, (ddouble)by - dy, (ddouble)bz - dz));
        (_, (ddouble cdx, ddouble cdy, ddouble cdz)) = ddouble.AdjustScale(0, ((ddouble)cx - dx, (ddouble)cy - dy, (ddouble)cz - dz));
        ddouble bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, cdxady = cdx * ady, adxcdy = adx * cdy, adxbdy = adx * bdy, bdxady = bdx * ady;
        ddouble det = adz * (bdxcdy - cdxbdy) + bdz * (cdxady - adxcdy) + cdz * (adxbdy - bdxady);
        ddouble permanent =
            (ddouble.Abs(bdxcdy) + ddouble.Abs(cdxbdy)) * ddouble.Abs(adz)
            + (ddouble.Abs(cdxady) + ddouble.Abs(adxcdy)) * ddouble.Abs(bdz)
            + (ddouble.Abs(adxbdy) + ddouble.Abs(bdxady)) * ddouble.Abs(cdz);
        return ErrorBound.Orient3D.Refine(det, permanent);
    }

    static Sign? RefineInCircle(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy) {
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
        return ErrorBound.InCircle.Refine(det, permanent);
    }

    static Sign? RefineInSphere(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double dx, double dy, double dz, double ex, double ey, double ez) {
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
        return ErrorBound.InSphere.Refine(det, permanent);
    }

    // --- [LIFTS_AND_MINORS]
    static Expansion Lift2(Expansion x, Expansion y) =>
        Expansion.Sum(Expansion.Multiply(x, x), Expansion.Multiply(y, y));

    static Expansion Lift3(Point3d p, Point3d anchor) {
        (Expansion x, Expansion y, Expansion z) = (Expansion.Diff(p.X, anchor.X), Expansion.Diff(p.Y, anchor.Y), Expansion.Diff(p.Z, anchor.Z));
        return Expansion.Sum(Expansion.Multiply(x, x), Expansion.Sum(Expansion.Multiply(y, y), Expansion.Multiply(z, z)));
    }

    static Expansion Minor3(Point3d p, Point3d q, Point3d d_, Point3d e) {
        Expansion pq = Expansion.Difference(Expansion.Multiply(Expansion.Diff(p.X, e.X), Expansion.Diff(q.Y, e.Y)), Expansion.Multiply(Expansion.Diff(q.X, e.X), Expansion.Diff(p.Y, e.Y)));
        Expansion qd = Expansion.Difference(Expansion.Multiply(Expansion.Diff(q.X, e.X), Expansion.Diff(d_.Y, e.Y)), Expansion.Multiply(Expansion.Diff(d_.X, e.X), Expansion.Diff(q.Y, e.Y)));
        Expansion dp = Expansion.Difference(Expansion.Multiply(Expansion.Diff(d_.X, e.X), Expansion.Diff(p.Y, e.Y)), Expansion.Multiply(Expansion.Diff(p.X, e.X), Expansion.Diff(d_.Y, e.Y)));
        return Expansion.Sum(
            Expansion.Multiply(pq, Expansion.Diff(d_.Z, e.Z)),
            Expansion.Sum(Expansion.Multiply(qd, Expansion.Diff(p.Z, e.Z)), Expansion.Multiply(dp, Expansion.Diff(q.Z, e.Z))));
    }
}

public static class RationalOracle {
    public static Sign InCircum(Expansion det, Expansion lambda, int lambdaDegree) {
        Sign fl = Sign.Of(lambda.ToFraction().Sign);
        Sign parity = (lambdaDegree & 1) == 0 ? fl.Times(fl) : fl;
        return Sign.Of(det.ToFraction().Sign).Times(parity);
    }

    internal static Sign RationalOf(Expansion det, Expansion lambda, int lambdaDegree) {
        static ERational Lift(Expansion e) {
            ERational acc = ERational.FromEInteger(EInteger.Zero);
            foreach (double component in e.Components) acc = acc.Add(ERational.FromDouble(component));
            return acc;
        }
        Sign fl = Sign.Of(Lift(lambda).Sign);
        Sign parity = (lambdaDegree & 1) == 0 ? fl.Times(fl) : fl;
        return Sign.Of(Lift(det).Sign).Times(parity);
    }

    public static Sign? BinaryOf(Expansion e) {
        EContext exact = EContext.Unlimited.WithBlankFlags();
        EFloat acc = EFloat.Zero;
        foreach (double component in e.Components) acc = acc.Add(EFloat.FromDouble(component), exact);
        return exact.HasFlags && (exact.Flags & EContext.FlagInexact) != 0 ? null : Sign.Of(acc.Sign);
    }
}
```

## [03]-[INTERIOR_NUMERICS]

- Owner: `IExact<TSelf>` is the static-abstract exact-carrier algebra letting every construction and determinant polynomial be written ONCE and instantiated at both carriers; `Expansion` is the nonoverlapping floating-point expansion whose `Verdict` is ALWAYS determined; `Interval` is the directed-rounding `EFloat` bracket whose `Verdict` resolves exactly when the bracket excludes zero, at fixed bounded cost per operation — the software directed rounding the runtime cannot express through FPU mode switches; `ErrorBound` is the per-tier permanence filter-row table; `RationalOracle` is the exact adjudicator set — a PRIMARY `Fraction` verdict in the shipping path beside two INDEPENDENT sources declared for the proof estate's differential alone; `NumericsPolicy` owns the strict-IEEE-754 invariant, the interior-`double` scope, the error-bound constants, and the FMA capability gate.
- Cases: `IExact` is the algebra contract both carriers implement, and it declares only what a polynomial on this page calls — `Of`/`Diff` static, `Add`/`Sub`/`Mul`/`Scale`/`Verdict` instance — because an unread `static abstract` taxes every future conformance forever; `ErrorBound` carries one row per direct predicate, never a parallel threshold owner; `TwoProduct` carries the FMA row and the Dekker-split row, selected once by the RID capability gate, never per call site, and interchangeable over the admitted operand domain rather than universally.
- Entry: `TwoProduct` is the exact two-component product — `FusedMultiplyAdd` on FMA-capable RIDs, the Dekker split otherwise, the branch a JIT-constant `HardwareFma` read once and dead-code-eliminated after tiering, and the two rows bit-identical over every operand `NumericsPolicy.SplitCeiling` admits, so the branch is invisible to the verdict; `TwoSum` is the exact two-component sum with Knuth's rounding-error recovery; `ErrorBound.Of`/`Refine` are the two filter projections over one verdict protocol — a determinate `Sign` or `null`-escalate.
- Auto: the error-free transforms and Shewchuk's fast-expansion-sum and scale-expansion hold the nonoverlapping invariant, so `SignOf` reads the true sign from the top nonzero term; `Interval.Mul` brackets all four endpoint products under both directed contexts, so a resolved `Verdict` is a PROOF of the exact sign — the filter accepts or escalates, never mis-decides.
- Law: the `Interval` bracket composes the PeterO members the catalogue verifies and nothing else — `EContext.ForPrecisionAndRounding(53, ERounding.Floor|Ceiling)` beside `WithPrecisionInBits(true)` (`api-petero-numbers.md [03]` `EContext` row `[02]` and `[ECONTEXT_BUILDERS]`), the `EFloat.FromDouble` dyadic lift (`[03]` `EFloat` row `[01]`), the context-taking `Add`/`Subtract`/`Multiply` arities (`[EFLOAT_ARITHMETIC]`), and `Sign`/`IsZero` for the verdict (`[03]` row `[06]`, `[EFLOAT_CLASSIFY]`). PeterO publishes NO `Min`/`Max` member on `EFloat` — the catalogue's whole ordering surface is `CompareTo`/`CompareToTotal` (`[03]` rows `[07]`/`[08]`, `[IMPLEMENTATION_LAW] [TOPOLOGY]` "every ordering read spells `CompareTo`") and the type ships no relational operators — so the four-endpoint `Mul` folds its bounds through two `CompareTo` reductions rather than a member that does not exist.
- Exemption: `EContext.Unlimited` governs the accumulate-and-read-sign path alone; `NextPlus`/`NextMinus` and every analytic member require the bracket's finite bounded context and return NaN under `Unlimited` (`api-petero-numbers.md [EFLOAT_NEIGHBOUR]`), so neither enters the ladder.
- Law: interior arithmetic, filters, and policy cross no public signature; the exact result is the `Sign` the predicate returns. `Expansion.Components`, `RationalOracle.RationalOf`, and `RationalOracle.BinaryOf` are the members whose only consumer is the proof estate's four-way differential, and each is declared for exactly that reader; running a second bignum oracle inside a shipping verdict doubled the estate's most expensive tier and resolved its own disagreement by throwing, so the differential lives where differentials belong.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`), TYoshimura.DoubleDouble (106-bit refine), ExtendedNumerics.BigRational (the PRIMARY exact-rational tier over `BigInteger`), PeterO.Numbers (the interval tier and the INDEPENDENT adjudicator whose `EInteger` bignum shares no representation with `Fraction`'s `BigInteger`), BCL inbox (`FusedMultiplyAdd`, FMA/AdvSimd capability statics, `double.Epsilon`); no external geometry dependency.
- Growth: a new exact carrier (a hardware `Float128` bracket) is one `IExact` conformance every construction instantiates with zero polynomial edits; a new predicate's filter is one `ErrorBound` row; a longer computation grows the `Expansion` component buffer, never a parallel arbitrary-precision type; the interior-`double` scope widens to a new kernel only by naming it in `NumericsPolicy`.
- Boundary: `Expansion` is ONE owner for sign-exact arithmetic — a free `TwoSum`/`TwoProduct` set or a parallel `BigFloat`/`MPFR` type is the deleted form. `Interval` is ONE owner for the directed-rounding bracket and keeps its bare name against every upper-folder twin — the discriminant is the CARRIED PROOF, a pair of directed-rounded `EFloat` endpoints whose `Verdict` is a sign proof, where an upper folder's same-named type is a scalar range; a per-predicate epsilon-inflation filter is the deleted form, the bracket sound by construction where an epsilon guess is a tuned lie. Both `TwoProduct` rows share one member gated once on `NumericsPolicy.HardwareFma`; a per-call-site FMA probe or a second product type is the deleted form, and the row selection never reaches a verdict because `SplitCeiling` bounds the admitted domain to where the rows agree bit for bit. `ErrorBound` is the single permanence-threshold table — a keyed roster like every other closed vocabulary here, each row carrying the published `(Alpha, Beta)` pair and deriving `(Alpha + Beta*eps)*eps` at whatever roundoff a tier hands it, so a precision stage is ONE argument and a per-tier constant beside a policy owner is the named defect. `NumericsPolicy` states the strict-IEEE-754/RID invariant as the floor the forward-error coefficients derive against, and a runtime violating it is outside the support matrix, not a tolerated mode; under that same invariant the Dekker row is pure binary64 that RyuJIT never contracts into an `fmadd`, so it carries no RID dependence of its own, and its ONE residual difference from the FMA row — the sign of a zero low word in the underflow regime — is invisible to a sign verdict because `Expansion.Single` drops a zero component, while a component-wise bit comparison across the two rows is the one read that must not assume they byte-match.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Numerics;
using CommunityToolkit.HighPerformance.Buffers;
using DoubleDouble;
using ExtendedNumerics;
using PeterO.Numbers;
using Thinktecture;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IExact<TSelf> where TSelf : struct, IExact<TSelf> {
    static abstract TSelf Of(double value);
    static abstract TSelf Diff(double a, double b);
    TSelf Add(TSelf other);
    TSelf Sub(TSelf other);
    TSelf Mul(TSelf other);
    TSelf Scale(double exact);
    Sign? Verdict { get; }
}

// --- [CONSTANTS] -----------------------------------------------------------------------
public static class NumericsPolicy {
    public const double Epsilon = 1.0 / (1L << 53);
    public const double DoubleDoubleEpsilon = Epsilon * Epsilon * 0.5;
    public const double Splitter = (1 << 27) + 1;
    public const double SplitCeiling = double.MaxValue / Splitter;

    public static readonly bool HardwareFma =
        System.Runtime.Intrinsics.X86.Fma.IsSupported || System.Runtime.Intrinsics.Arm.AdvSimd.Arm64.IsSupported;

}

// --- [MODELS] --------------------------------------------------------------------------
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

    // --- [EXPANSION_SUM]
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
        (Expansion wide, Expansion narrow) = left.length >= right.length ? (left, right) : (right, left);
        Expansion acc = Scale(wide, narrow.components[0]);
        for (int i = 1; i < narrow.length; i++) acc = Sum(acc, Scale(wide, narrow.components[i]));
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
    internal static Expansion Diff(double a, double b) => TwoSum(a, -b);
    static Expansion IExact<Expansion>.Of(double value) => Single(value);
    static Expansion IExact<Expansion>.Diff(double a, double b) => Diff(a, b);
    Expansion IExact<Expansion>.Add(Expansion other) => Sum(this, other);
    Expansion IExact<Expansion>.Sub(Expansion other) => Difference(this, other);
    Expansion IExact<Expansion>.Mul(Expansion other) => Multiply(this, other);
    Expansion IExact<Expansion>.Scale(double exact) => Scale(this, exact);
    Sign? IExact<Expansion>.Verdict => SignOf(this);

    // --- [RATIONAL_LIFT]
    public Fraction ToFraction() {
        int shared = int.MaxValue;
        using MemoryOwner<int> pooled = length <= StackExponents ? MemoryOwner<int>.Empty : MemoryOwner<int>.Allocate(length);
        Span<int> exponents = length <= StackExponents ? stackalloc int[StackExponents] : pooled.Span;
        for (int i = 0; i < length; i++) {
            if (components[i] == 0.0) continue;
            exponents[i] = Math.ILogB(components[i]) - 52;
            shared = Math.Min(shared, exponents[i]);
        }
        if (shared == int.MaxValue) return Fraction.Zero;
        BigInteger numerator = BigInteger.Zero;
        for (int i = 0; i < length; i++) {
            if (components[i] == 0.0) continue;
            numerator += new BigInteger((long)Math.ScaleB(components[i], -exponents[i])) << (exponents[i] - shared);
        }
        return shared >= 0
            ? new Fraction(numerator << shared, BigInteger.One)
            : new Fraction(numerator, BigInteger.One << -shared);
    }

    public ReadOnlySpan<double> Components => components.AsSpan(0, length);

    static Expansion Pair(double small, double large) =>
        small == 0.0 ? Single(large) : new Expansion(new[] { small, large }, 2);

    private const int StackExponents = 128;
}

public readonly struct Interval : IExact<Interval> {
    static readonly EContext Down = EContext.ForPrecisionAndRounding(53, ERounding.Floor).WithPrecisionInBits(true);
    static readonly EContext Up = EContext.ForPrecisionAndRounding(53, ERounding.Ceiling).WithPrecisionInBits(true);

    public readonly EFloat Lo;
    public readonly EFloat Hi;

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ErrorBound {
    public static readonly ErrorBound Orient2D = new("orient-2d", alpha: 3.0,  beta: 16.0);
    public static readonly ErrorBound Orient3D = new("orient-3d", alpha: 7.0,  beta: 56.0);
    public static readonly ErrorBound InCircle = new("in-circle", alpha: 10.0, beta: 96.0);
    public static readonly ErrorBound InSphere = new("in-sphere", alpha: 16.0, beta: 224.0);

    public double Alpha { get; }
    public double Beta { get; }

    public double Bound(double roundoff) => (Alpha + (Beta * roundoff)) * roundoff;

    public Sign? Of(double det, double permanent) =>
        Math.Abs(det) > Bound(NumericsPolicy.Epsilon) * permanent ? Sign.Of(det) : null;

    public Sign? Refine(ddouble det, ddouble permanent) =>
        ddouble.Abs(det) > Bound(NumericsPolicy.DoubleDoubleEpsilon) * permanent ? Sign.Of(ddouble.Sign(det)) : null;
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
