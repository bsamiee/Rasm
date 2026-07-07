# [RASM_NUMERICS_PREDICATES]

The adaptive-precision exact-predicate floor every higher geometry owner rides, authored from first principles because no maintained, NuGet-published, indirect-complete predicate library exists — the float-epsilon geometry packages carry no robustness guarantee and no implicit-point family at all. The page owns the closed `Predicate` family in `Rasm.Numerics`: the four DIRECT members `Orient2D`/`Orient3D`/`InCircle`/`InSphere`, each an adaptive ladder walking four of the five `PrecisionTier` rows (direct members skip `Interval` — the directed-rounding tier only implicit members consume) — a fast IEEE-754 `double` filter gated by a forward error bound, escalating to a `TYoshimura.DoubleDouble` `ddouble` 106-bit refinement only where the filtered interval straddles zero, then to the sign-exact `Expansion` branch, with the `ExtendedNumerics.BigRational` `Fraction` `BigInteger` exact-rational oracle as the topmost adjudicator and the differential ground truth — plus the IMPLICIT family over CONSTRUCTED intersection points that are never materialized as rounded coordinates: `Implicit`, the closed four-case point carrier (`Explicit` | `Ssi` segment-segment | `Lpi` line-plane | `Tpi` three-plane, carrying DEFINING POINTS only), folded by the axis-projected `Orient2D`, the coordinate `Compare` order key, and the in-circum `InCircle`/`InSphere` implicit-query members. Every implicit member runs a directed-rounding INTERVAL filter (`PeterO.Numbers` `EFloat` under `ERounding.Floor`/`Ceiling`) before the `Expansion` fold — the cheap tier that resolves the bulk of implicit signs without arbitrary-precision work — and the in-circum pair escalates its highest-degree homogeneous determinant to the `Fraction` oracle. Every member returns an exact `Sign` verdict in every case.

Under the family sit `Expansion`, the sign-exact nonoverlapping floating-point expansion kernel whose FMA `TwoProduct`/Knuth `TwoSum` transforms the `ddouble` middle tier shares so the two never disagree on a sign both resolve; `Interval`, the directed-rounding 53-bit `EFloat` bracket arithmetic of the implicit filter tier; `IExact<TSelf>`, the static-abstract algebra both exact carriers implement so every construction and determinant polynomial is written ONCE and instantiated per carrier; the `Fraction` exact-rational oracle the implicit in-circum determinants escalate to and the law-matrix proves every faster tier against; `ErrorBound`, the per-tier static-permanence error-bound row table; and `NumericsPolicy`, the strict-IEEE-754/RID invariant owner naming the filter-then-refine-then-exact contract, the sanctioned interior-double scope, and the FMA capability gate. The kernel composes `Rhino.Geometry` `Point3d` coordinates at the seam, emits only `Sign` verdicts (plus the ONE `Round()` emission-seam materialization on the implicit carrier), computes no hash, and mints no identity.

`Expansion`, `Interval`, and `ErrorBound` operate on raw `double` because robust arithmetic is mathematically defined over IEEE-754 doubles, never unit-bearing quantities. This kernel and the `Rasm.Meshing` arena's weld inner loop are the only sanctioned interior-double owners; `double` crosses a public signature only as a `Point3d` coordinate (the domain's native scalar) at the seam. An interior `double` escaping a public predicate signature elsewhere is the named seam violation. The public `Orient3D`/`InSphere` members are the package-boundary export the `Rasm.Compute` CDTet consumer binds by shape, and `Orient2D`/`Orient3D` exact verdicts are the Fabrication Posting wire — both seams consume the same four direct members, never a parallel export surface.

## [01]-[INDEX]

- [01]-[ROBUST_PREDICATES]: Closed `Predicate` family — `Orient2D`/`Orient3D`/`InCircle`/`InSphere` direct ladders (four of the five `PrecisionTier` rows — `Interval` is implicit-only) plus the widened implicit family over the `Implicit` carrier: axis-projected `Orient2D` (every explicit/implicit combination × the three `Axis` projection planes), `Compare` (the exact per-coordinate order key), and the in-circum `InCircle`/`InSphere` implicit-query members — interval-filtered, `Expansion`-exact, `Fraction`-adjudicated, returning an exact `Sign`.
- [02]-[INTERIOR_NUMERICS]: `PrecisionTier` five-tier ladder discriminant; `IExact<TSelf>` the one-polynomial-two-carriers algebra; `Expansion` sign-exact expansion arithmetic with its `ToFraction` exact lift; `Interval` the directed-rounding `EFloat` filter carrier; `ErrorBound` per-tier static-permanence filter/refine rows; `RationalOracle` the `Fraction` exact-rational adjudicator plus the independent `PeterO` binary adjudicator; `Ssi`/`Lpi`/`Tpi` the defining-point implicit constructions; `NumericsPolicy` the strict-IEEE/RID invariant and interior-double scope owner with the FMA-gated `TwoProduct` rows.

## [02]-[ROBUST_PREDICATES]

- Owner: `Sign` `[SmartEnum<int>]` the closed ternary verdict (`Negative`/`Zero`/`Positive`, key `-1`/`0`/`+1`) every predicate returns, with the `Flip`/`Times` verdict algebra the homogeneous denominator flips ride; `Axis` `[SmartEnum<int>]` the closed coordinate vocabulary (`X`/`Y`/`Z`, key = coordinate ordinal) carrying the projected-plane ordinal columns `U`/`V` (the plane NORMAL to the axis: `X → (y,z)`, `Y → (z,x)`, `Z → (x,y)`) — the ONE generator over which every axis-projected member spans its three planes, never three enumerated member names per plane; `Implicit` the closed `[Union<Point3d, Ssi, Lpi, Tpi>]` ad-hoc point carrier — `Explicit` (an ordinary coordinate point), `Ssi` (segment-segment crossing, FOUR defining points + the construction plane), `Lpi` (line-plane crossing, FIVE defining points), `Tpi` (three-plane crossing, NINE defining points) — implicit conversions make every `Point3d` call site lift without ceremony, and the carrier stores DEFINING POINTS ONLY: the exact homogeneous coordinates (`X`/`Y`/`Z` numerators + `Lambda` denominator) derive on demand through `Homogeneous<T>()` in whichever `IExact<T>` carrier the tier needs, so no rounded coordinate and no rounded denominator ever sits inside the carrier; `Predicate` the ONE static surface owning the direct ladders and the implicit folds.
- Cases: `Sign` rows 3; `Axis` rows 3; `Implicit` cases 4; `Predicate` members — `Orient2D`/`Orient3D`/`InCircle`/`InSphere` each a raw-coordinate CORE (`double` tuples — the entry the cross-package consumers bind, since the Rasm.Compute lane law bars host value types on interior signatures) plus a thin `Point3d` seam adapter (4 cores + 4 adapters, each core one inline allocation-free `ErrorBound`-gated `PrecisionTier` escalation, never a sibling fast/exact pair) · `Orient2D(in Implicit, in Implicit, in Implicit, Axis)` — ONE member spanning every explicit/implicit combination {EEE, IEE, IIE, III} × {xy, yz, zx} by the carrier's case shape and the axis row (the all-explicit shape routes the direct filtered ladder on swizzled coordinates; any implicit shape routes interval-filter → exact) · `Compare(in Implicit, in Implicit, Axis)` — the exact per-coordinate three-way order key spanning {EE, IE, II} × {X, Y, Z} (the honest sort key replacing every exact-point-over-rounded-coordinate ordering) · `InCircle(Point3d, Point3d, Point3d, in Implicit, Axis)` and `InSphere(Point3d, Point3d, Point3d, Point3d, in Implicit)` — the in-circum implicit-query members (explicit query routes the direct ladder; a constructed query runs the homogeneous lifted determinant with the `lambda^2`-cleared lifts, the verdict composing the exact `lambda` sign at the polynomial's structural parity — the in-circle degree-4 EVEN power Zero-gates without flipping, the in-sphere degree-5 ODD power flips — adjudicated by `RationalOracle.InCircum`) (8 public members spanning the ~21-member mandated capability lattice). The multi-implicit in-circum combinations ({IIEE..IIII}) and the 3D multi-implicit `Orient3D`/`InSphere` family are RECORDED GROWTH gated on a CDTet consumer — one further case arm per member on the same folds, never new surfaces.
- Entry: `public static Sign Orient2D(double ax, double ay, double bx, double by, double cx, double cy)` (with the `Point3d` adapter over it) the exact sign of the `(b-a)×(c-a)` 2D cross-product determinant — positive for a counter-clockwise turn; `public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d)` the exact sign of the `3×3` determinant placing `d` relative to the oriented plane `abc`; `public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d)` the exact sign of the `4×4` lifted determinant testing `d` inside the circumcircle of `abc`; `public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e)` the exact `5×5` lifted in-sphere determinant; `public static Sign Orient2D(in Implicit a, in Implicit b, in Implicit c, Axis axis)` the exact orientation of three possibly-constructed points projected on the axis plane; `public static Sign Compare(in Implicit a, in Implicit b, Axis axis)` the exact three-way comparison of one coordinate (`Negative` = `a < b`); `public static Sign InCircle(Point3d a, Point3d b, Point3d c, in Implicit d, Axis axis)` / `public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, in Implicit e)` the exact in-circum verdicts for a constructed query point — every member total, pure, exact, no rail; a degenerate construction (`lambda = 0`: parallel segments, a line parallel to its plane, near-parallel plane triples) yields `Sign.Zero` through the `Times` flip algebra, the degeneracy witness the consumer's recovery reads.
- Auto: each direct predicate computes the approximate determinant and its permanence magnitude in one `double` expression each, then walks the `PrecisionTier` ladder INLINE as one `??`-chain over the uniform `Sign?`-or-escalate verdict protocol — `ErrorBound.<Kind>.Of(det, permanent) ?? Refine<Kind>(…) ?? <Kind>Exact(…)` — allocation-free with no captured-closure thunk. `PrecisionTier.Double` proves `|det|` clears the permanence-scaled `ErrorBound.Coefficient` threshold (the common fast path); on a straddle the `Refine<Kind>` recompute evaluates the determinant in `ddouble` (the multilinear orient recomputes align per-vertex difference vectors to one binary exponent through `ddouble.AdjustScale`; the lifted in-circum recomputes run raw — a per-row scale does not commute with the quadratic lift) and reads `ErrorBound.Refine`; only the still-indeterminate residue falls through to the `Expansion` fold. Each IMPLICIT member instead runs the `PrecisionTier.Interval` filter first — the SAME polynomial instantiated at `IExact<Interval>` (53-bit `EFloat` endpoints under `ERounding.Floor`/`Ceiling` contexts) whose `Verdict` resolves whenever the numerator bracket excludes zero AND every `Lambda` bracket resolves its sign (the even-power denominator must still prove itself nonzero for the zero-gate) — and only the indeterminate residue re-instantiates the polynomial at `IExact<Expansion>`, reading `Expansion.SignOf` with `Sign.Times` denominator flips; the in-circum members escalate the exact determinant to `RationalOracle.InCircum` — the `Fraction` `BigInteger` verdict via `Expansion.ToFraction`, the denominator sign read exactly from `lambda.ToFraction().Sign` (never a `double` estimate that can mis-sign near zero) and composed at the polynomial's structural `lambda` parity through `Sign.Times`, so a zero denominator always yields `Zero` and an odd power alone flips. A per-member rounded-coordinate `double` filter remains impossible for constructed points (it would materialize the coordinate the homogeneous form exists to avoid) — the interval filter is the honest fast tier that replaces it. Every tier is monotone and sign-consistent, so the verdict is always the true sign of the real-arithmetic determinant.
- Receipt: none — a predicate returns a `Sign` verdict, the most refined receipt a total exact test admits; a sign carries no residual. The ONE emission-side materialization is `Implicit.Round()` — the rounded `Point3d` a consumer emits AT ITS OWN emission seam (a `Polyline` vertex, a tessellation output), never a value any predicate reads back.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`, `[Union<T1,...>]` ad-hoc), RhinoCommon (`Point3d` vocabulary), TYoshimura.DoubleDouble (`ddouble.Sign`/`Abs`/`AdjustScale` — the 106-bit middle tier), ExtendedNumerics.BigRational (`Fraction.Sign`/`Subtract`/`Multiply`, `(Fraction)double` lossless IEEE decomposition — the exact-rational oracle), PeterO.Numbers (`EFloat.FromDouble`/`Add`/`Subtract`/`Multiply` under `EContext.ForPrecisionAndRounding` + `ERounding.Floor`/`Ceiling` — the directed-rounding interval-filter tier; `EContext.Unlimited`/`WithBlankFlags`/`FlagInexact` + `ERational` — the independent exact adjudicators), BCL inbox (`System.Math.FusedMultiplyAdd`, `System.Runtime.Intrinsics` capability probes, `System.Numerics.BigInteger` under `Fraction`).
- Growth: a new implicit-point construction (a sphere-line crossing, a torus section point) is one `Implicit` case carrying its defining points plus one `Homogeneous<T>` arm — every fold, filter, and emission member widens by that one arm with the generated dispatch breaking loudly; a new direct predicate is one member riding the same `ErrorBound` stage table and `Expansion` fold with one `ErrorBound` row; a new precision stage is one `PrecisionTier` row plus one escalation arm woven into each member tail; the multi-implicit in-circum and 3D multi-implicit orientation families are the recorded CDTet-gated growth arms on the EXISTING members; zero new surface, no sibling fast/exact predicate types.
- Boundary: the whole family lives on ONE `Predicate` static owner and a per-predicate class or a `FastOrient2D`/`ExactOrient2D` pair is the deleted form — tiers are branches inside one member, gated by the one `Sign?`-or-escalate verdict protocol (`ErrorBound.Of`/`Refine`, `Interval.Verdict`), never two surfaces; the verdict union is the closed `Sign` SmartEnum and a raw `int`/`double` sign leaking across a public signature is the named defect; coordinates are `Rhino.Geometry` `Point3d` read at the seam and a domain-local point struct is the deleted form; a constructed point travels as `Implicit` defining-point carriage and a rounded `Point3d` materialized at construction (a `Denominator`-as-`double` field, an `Estimate()` readout inside an exact carrier) is the named robustness defect and the deleted form — rounding happens ONCE, at `Round()`, on the consumer's emission seam; derived `Plane` inputs are dead — a three-plane point is defined by its NINE points, so the exact carriers see original coordinates, never pre-rounded normals from `Vector3d.CrossProduct`; every leaf difference inside an exact carrier rides the error-free `IExact.Diff` transform and a raw `double` subtraction wrapped in an exact type is the deleted rounded-leaf form; the axis-projected family is generated by the `Axis` vocabulary and three per-plane member names are the rejected enumerated roster; loosening a filter band to pass a near-degenerate case instead of taking the exact branch is the named correctness defect — a sign verdict is exact or it is a defect; the interior `double` arithmetic inside the filter, `Expansion`, and `Interval` is the sanctioned scope owned by `NumericsPolicy`; the public `Orient3D`/`InSphere` shape is the Compute-consumable export and the `Orient2D`/`Orient3D` verdicts are the Fabrication wire — the package boundary consumes the same members every interior owner does.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using DoubleDouble;
using ExtendedNumerics;
using PeterO.Numbers;
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

// The ordered precision ladder: the Key is the tier ordinal so a reader scans the escalation order.
// Direct members walk Double → DoubleDouble → Expansion (→ Rational for the law matrix); implicit
// members walk Interval → Expansion → Rational (a rounded-coordinate Double filter cannot exist for
// a constructed point). A new tier is one row plus one escalation arm per member.
[SmartEnum<int>]
public sealed partial class PrecisionTier {
    public static readonly PrecisionTier Double       = new(0); // IEEE-754 forward-error filter (direct only)
    public static readonly PrecisionTier DoubleDouble = new(1); // ddouble 106-bit error-free-transform refine (direct only)
    public static readonly PrecisionTier Interval     = new(2); // directed-rounding EFloat bracket filter (implicit only)
    public static readonly PrecisionTier Expansion    = new(3); // sign-exact nonoverlapping expansion
    public static readonly PrecisionTier Rational     = new(4); // Fraction BigInteger exact-rational oracle
}

// The closed coordinate vocabulary: Key = the axis' own coordinate ordinal (the Compare column);
// U/V = the ordinals of the plane NORMAL to the axis (the Orient2D/InCircle projection plane).
[SmartEnum<int>]
public sealed partial class Axis {
    public static readonly Axis X = new(0, u: 1, v: 2);
    public static readonly Axis Y = new(1, u: 2, v: 0);
    public static readonly Axis Z = new(2, u: 0, v: 1);

    public int U { get; }
    public int V { get; }

    public static double Coord(Point3d p, int ordinal) => ordinal == 0 ? p.X : ordinal == 1 ? p.Y : p.Z;
}

// The constructed-point carrier: defining points only, exact coordinates derived on demand.
// Implicit conversions lift every Point3d call site; struct members store inline, allocation-free.
[Union<Point3d, Ssi, Lpi, Tpi>(T1Name = "Explicit", T2Name = "Ssi", T3Name = "Lpi", T4Name = "Tpi")]
public readonly partial struct Implicit {
    public (T X, T Y, T Z, T Lambda) Homogeneous<T>() where T : struct, IExact<T> =>
        Switch(
            @explicit: static e => (T.Of(e.X), T.Of(e.Y), T.Of(e.Z), T.Of(1.0)),
            ssi:       static s => s.Homogeneous<T>(),
            lpi:       static l => l.Homogeneous<T>(),
            tpi:       static t => t.Homogeneous<T>());

    // The ONE emission-seam materialization: consumers round HERE (a Polyline vertex, a tessellation
    // output), never inside a predicate. A zero-lambda construction rounds to non-finite coordinates
    // the consumer's freeze gate rejects — rounding never invents a point the construction lacks.
    public Point3d Round() =>
        IsExplicit ? AsExplicit : Materialized(Homogeneous<Expansion>());

    static Point3d Materialized((Expansion X, Expansion Y, Expansion Z, Expansion Lambda) h) {
        double lambda = h.Lambda.Estimate();
        return new Point3d(h.X.Estimate() / lambda, h.Y.Estimate() / lambda, h.Z.Estimate() / lambda);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// SSI: the crossing of two coplanar segments (P,Q)×(R,S); the 2×2 crossing system solves on the
// construction plane, the parametric numerators ride the (P,Q) segment in all three coordinates.
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

// TPI: the crossing of three planes, EACH DEFINED BY THREE POINTS — nine points; a pre-rounded
// Plane normal (Vector3d.CrossProduct at double precision) is the dead derived-input form.
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

    // exact plane row: normal = (b−a)×(c−a) in T, offset = normal·a with exact-coordinate scaling
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
    // Each direct predicate walks the precision ladder INLINE and allocation-free as one `??`-chain over the
    // uniform Sign?-or-escalate verdict protocol: `ErrorBound.<Kind>.Of` (forward-error double filter)
    // ?? `Refine<Kind>` (ddouble 106-bit recompute) ?? `<Kind>Exact` (sign-exact Expansion). The escalation is
    // spelled inline, never a captured-closure fold — a `Func<Sign?>`/`Func<Sign>` thunk would allocate two
    // delegates per call on the kernel's hottest path. The middle and exact tiers share the `TwoProduct`/`TwoSum`
    // rounding model, so they never disagree on a sign both resolve.

    // --- [COORDINATE_CORE]
    // The raw-coordinate entries are the CORE the cross-package consumers bind (the Rasm.Compute
    // discretization lane law bars host value types on interior signatures): each owns the tier-1
    // forward-error filter inline on raw doubles — the hot, allocation-free path — and lifts into the
    // shared kernel-interior Refine/Exact tiers only at the rare escalation boundary. The Point3d
    // entries below are thin seam adapters over these cores.

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
    // ONE member spans {EEE, IEE, IIE, III} × {xy, yz, zx}: the carrier's case shape is the combo,
    // the Axis row is the plane. All-explicit routes the direct filtered ladder on swizzled
    // coordinates; any constructed point routes interval-filter → exact. Verdicts compose the exact
    // lambda signs through Sign.Times at each lambda's polynomial parity — la/lb enter ODD (flip),
    // lc enters SQUARED (even: Zero-gates without flipping) — so a negative denominator can never
    // mis-sign and a zero denominator (a degenerate construction) always yields Zero.
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
    // The exact per-coordinate order key: Negative = a-before-b on the axis coordinate. Replaces
    // every exact-point-over-rounded-coordinate sort in chain merge, plane ordering, and sweep code.
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
    // Constructed query point: homogeneous lifted determinant, explicit-row lifts cleared by lambda^2.
    // The lambda parity is a structural constant of each polynomial — the anchored in-circle rows
    // carry lambda to the EVEN power 4 (Zero-gate, no flip), the anchored in-sphere rows to the ODD
    // power 5 (one flip) — and the verdict composes that parity exactly, filter and oracle alike.
    // Interval-filtered, then Expansion → Fraction adjudication (the runtime terminal tier).
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
    // One polynomial per fold, two carriers: T = Interval is the filter tier, T = Expansion the exact
    // tier — the same IExact algebra, so the filter can never test a different polynomial than the
    // one the exact branch decides.
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
        // 3×3 once, folded into each negated 2×2 — so InCircle(a,b,c, Explicit d) ≡ the direct arm.
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
    // Every leaf difference is the 2-component TwoSum expansion — a raw rounded `a.X - c.X` here would
    // decide the sign of a DIFFERENT determinant exactly (the rounded-leaf defect the Boundary law names),
    // and the law matrix would catch the disagreement against the Fraction oracle over original ordinates.
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
    // The middle tier: recompute the same determinant at 106-bit and read the verdict through
    // `ErrorBound.Refine` (null escalates to the Expansion exact branch). The MULTILINEAR orient
    // determinants align each difference vector to one binary exponent through `ddouble.AdjustScale` —
    // a positive per-row scale preserves a multilinear sign; the LIFTED in-circum determinants are
    // scale-INHOMOGENEOUS (a row scale does not commute with the quadratic lift), so their recompute
    // runs on raw 106-bit differences — per-row scaling there would decide a different polynomial.
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
    // The topmost exact-rational tier — the `Fraction` BigInteger oracle the predicate determinant reduces to
    // as the runtime last resort AND the law-matrix ground truth. Each ordinate widens losslessly through the
    // explicit `(Fraction)double` IEEE decomposition, the determinant runs in infinite-precision rational
    // arithmetic, and the verdict is `Fraction.Sign` — never a `double`/`decimal` readout.
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

    // The implicit in-circum adjudicator: the homogeneous Expansion determinant lifted exactly into a
    // single rational via `Expansion.ToFraction`, the denominator sign read from the exact
    // `lambda.ToFraction().Sign` — never a rounded `double` estimate that can mis-sign near zero.
    // `lambdaDegree` is the polynomial's structural lambda power (in-circle 4, in-sphere 5): an even
    // power Zero-gates without flipping, an odd power flips — the Sign.Times parity composition.
    public static Sign InCircum(Expansion det, Expansion lambda, int lambdaDegree) {
        Sign fl = Sign.Of(lambda.ToFraction().Sign);
        Sign parity = (lambdaDegree & 1) == 0 ? fl.Times(fl) : fl;
        return Sign.Of(det.ToFraction().Sign).Times(parity);
    }

    // The INDEPENDENT second-source binary adjudicator: `PeterO.Numbers.EFloat` under `EContext.Unlimited` over
    // the same `Expansion` determinant lifted to an exact binary `mantissa*2^exp` value through the dyadic-lossless
    // per-component `EFloat.FromDouble` sum. `EFloat`'s self-contained `EInteger` `uint[]` bignum shares NO
    // underlying type with the `Fraction` `BigInteger` oracle above, so this is a sign confirmed by a SECOND
    // unrelated arbitrary-precision implementation — the differential the law-matrix asserts agrees with
    // `ToFraction().Sign`. The verdict speaks the uniform Sign?-or-escalate protocol: a raised `FlagInexact`
    // (unreachable for dyadic sums under `Unlimited`) is a FAILED exactness proof and returns null — never a
    // fabricated Zero — so the adjudication is self-certifying exact rather than assumed.
    public static Sign? BinaryOf(Expansion e) {
        EContext exact = EContext.Unlimited.WithBlankFlags();
        EFloat acc = EFloat.Zero;
        foreach (double component in e.Components) acc = acc.Add(EFloat.FromDouble(component), exact);
        return exact.HasFlags && (exact.Flags & EContext.FlagInexact) != 0 ? null : Sign.Of(acc.Sign);
    }
}
```

## [03]-[INTERIOR_NUMERICS]

- Owner: `IExact<TSelf>` the static-abstract exact-carrier algebra — `Of` (exact double lift), `Diff` (the error-free leaf difference — a raw rounded `double` subtraction wrapped in an exact type is the deleted defect this member exists to forbid), `Prod` (the error-free leaf product), instance `Add`/`Sub`/`Mul`/`Scale` (scale by an EXACT input coordinate only), and `Sign? Verdict` — the one contract letting every construction and determinant polynomial be written ONCE and instantiated at both carriers; `Expansion` the `readonly struct` nonoverlapping floating-point expansion (most significant component last) with the `TwoSum`/`TwoProduct`/`Grow`/`Scale`/`Multiply`/`Sum`/`Difference` construction folds, the `Estimate`/`SignOf` projections, the `ToFraction` exact rational lift, and its `IExact<Expansion>` conformance whose `Verdict` is ALWAYS determined; `Interval` the `readonly struct` directed-rounding bracket — `EFloat` `Lo`/`Hi` endpoints under two frozen 53-bit contexts (`ERounding.Floor` down, `ERounding.Ceiling` up), interval `Add`/`Sub`/`Mul` (eight directed products, min/max endpoints), and a `Verdict` that resolves exactly when the bracket excludes zero — the software directed rounding the runtime cannot express through FPU mode switches, at fixed bounded cost per operation (53-bit mantissas, never `BigInteger` growth); `ErrorBound` the per-tier static-permanence filter-row table (`Orient2D`/`Orient3D`/`InCircle`/`InSphere` rows with `double` `Coefficient` + 106-bit `RefineCoefficient`); `RationalOracle` the exact adjudicator set (PRIMARY `Fraction`, INDEPENDENT `EFloat`/`ERational`); `Ssi`/`Lpi`/`Tpi` the defining-point constructions ([02] fence); `NumericsPolicy` the invariant owner — the strict-IEEE-754 statement, the interior-double scope, the error-bound constants, and the FMA capability gate with the Dekker `Splitter`.
- Cases: `IExact` members 8; `Expansion` construction folds `TwoSum` · `TwoProduct` · `Grow` · `Sum` · `Difference` · `Scale` · `Multiply` (7) plus `Estimate`/`SignOf`/`ToFraction`/`Components`; `Interval` ops `Add` · `Sub` · `Mul` · `Scale` (4) + `Verdict`; `ErrorBound` rows 4 — the permanence axis is one row per direct predicate, never a parallel threshold owner; `TwoProduct` rows 2 — the FMA row and the Dekker-split row, selected once by the RID capability gate, never per call site.
- Entry: `public static Expansion TwoProduct(double a, double b)` the exact two-component product — `Math.FusedMultiplyAdd` on FMA-capable RIDs (the error component is `fma(a,b,-(a*b))`), the Dekker `Splitter` split on FMA-free RIDs (`NumericsPolicy.HardwareFma` reads the `Fma.IsSupported`/`AdvSimd.Arm64.IsSupported` intrinsics capability once; the branch is a JIT-constant static readonly, dead-code-eliminated after tiering); `public static Expansion TwoSum(double a, double b)` the exact two-component sum with Knuth's branch-free rounding-error recovery; `Sum`/`Difference`/`Scale`/`Multiply` the expansion folds; `public static Sign SignOf(Expansion e)` the top-nonzero-component sign; `Interval` construction through the same `IExact` members; `public Sign? Of(double det, double permanent)` and `public Sign? Refine(ddouble det, ddouble permanent)` the two filter projections — one verdict protocol, a determinate `Sign` or `null`-escalate.
- Auto: `TwoProduct` and `TwoSum` are error-free transforms — the two returned components sum to the exact real result; `Sum`/`Scale` thread Shewchuk's `fast-expansion-sum` and `scale-expansion` maintaining the nonoverlapping invariant so `SignOf` reads the true sign from the top nonzero term; `ErrorBound.Of` compares `|det|` against `coefficient * permanent` with per-row coefficients derived once from `Epsilon` (`2^-53`); `ErrorBound.Refine` re-applies the permanence test at `DoubleDoubleEpsilon` (`2^-107`); `Interval.Mul` evaluates all four endpoint products under BOTH directed contexts and keeps the extremes, so the bracket always contains the real product — a resolved `Interval.Verdict` is therefore a PROOF of the exact sign (the filter can accept, never mis-decide), and an unresolved bracket escalates; `Expansion.ToFraction` lifts a homogeneous determinant losslessly into the `RationalOracle` `Fraction` adjudicator.
- Receipt: none — interior arithmetic, filters, and policy cross no public signature carrying a residual; the exact result is the `Sign` the predicate returns.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`), TYoshimura.DoubleDouble (`ddouble` 106-bit refine, `AdjustScale`/`Sign`/`Abs`), ExtendedNumerics.BigRational (`Fraction`/`Fraction.Sign`/`Add`/`Subtract`/`Multiply`, `(Fraction)double` lossless decomposition — the PRIMARY exact-rational top tier over `System.Numerics.BigInteger`), PeterO.Numbers (`EFloat.FromDouble`/`Add`/`Subtract`/`Multiply` + `EContext.ForPrecisionAndRounding(53, ERounding.Floor|Ceiling).WithPrecisionInBits(true)` — the directed-rounding interval tier; `EContext.Unlimited`/`WithBlankFlags`/`FlagInexact` + `ERational.Sign`/`FromDouble`/`CompareToBinary` — the INDEPENDENT binary + rational exact adjudicators whose self-contained `EInteger` `uint[]` bignum shares NO representation with `Fraction`'s `BigInteger`), BCL inbox (`System.Math.FusedMultiplyAdd`, `System.Runtime.Intrinsics.X86.Fma`/`Arm.AdvSimd.Arm64` capability statics, `double.Epsilon`); no external geometry dependency.
- Growth: a new exact carrier (a hardware `Float128` bracket) is one `IExact` conformance — every construction and fold instantiates it with zero polynomial edits; a new predicate's filter is one `ErrorBound` row; a longer exact computation grows the `Expansion` component buffer, never a parallel arbitrary-precision type; the interior-double scope widens to a new kernel only by naming it in `NumericsPolicy`.
- Boundary: `Expansion` is ONE owner for sign-exact arithmetic and a separate `TwoSum`/`TwoProduct` free-function set or a parallel `BigFloat`/`MPFR`-style type is the deleted form; `Interval` is ONE owner for the directed-rounding bracket and a per-predicate hand-rolled epsilon-inflation filter is the deleted form — the bracket is sound by construction where an epsilon guess is a tuned lie; the two `TwoProduct` rows share one member gated once on `NumericsPolicy.HardwareFma` and a per-call-site FMA probe or a second product type is the deleted form; `ErrorBound` is the single permanence-threshold table and an inlined magic-number literal is the named defect; `NumericsPolicy` states the strict-IEEE-754/RID invariant as fact — `net10.0` evaluates `double` at binary64 with round-to-nearest-even, no FTZ/DAZ, no extended-precision spill on every supported RID, which is the floor the forward-error coefficients are derived against, and a runtime violating it is outside the support matrix, not a tolerated mode; the predicates never under-refine to pass a degenerate case; `double` is exposed only at the seam as the canonical geometric coordinate carried by `Point3d`.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
// One polynomial, two carriers: Expansion (sign-exact, Verdict always determined) and Interval
// (directed-rounding bracket, Verdict null on a zero-straddle). Struct-constrained static-abstract
// dispatch — JIT-specialized per carrier, zero boxing.
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
// Strict-IEEE/RID invariant: net10.0 evaluates double strictly at binary64 — round-to-nearest-even,
// no FTZ/DAZ, no x87 extended spill — on every supported RID; the error-bound coefficients below are
// derived against exactly that model. HardwareFma selects the TwoProduct row once per process.
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

    // The 106-bit refine bounds: the same permanence coefficients re-derived against the double-double
    // unit roundoff, so the middle tier resolves a sign exactly where the double filter brackets it but
    // the determinant stays well above the 2^-107 noise floor.
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
    // Two rows, one member: the FMA error recovery on FMA-capable RIDs, the Dekker split on FMA-free
    // RIDs — both exact; HardwareFma is a JIT-constant, so the untaken row folds away after tiering.
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
    // Fast-expansion-sum: merge ascending by magnitude, thread the TwoSum high word as the carry, emit
    // each nonzero low word. The carry/low split reads TwoSumCore directly — routing through the
    // Pair-compressed TwoSum would double-count the high word on every exact step.
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
    // Shewchuk's linear scale-expansion: one TwoProduct plus two TwoSum chains per component with the
    // carry threading forward — never a quadratic Sum-per-component accumulate.
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
    // The IExact conformance: leaf lifts are the error-free transforms, so the generic polynomial
    // instantiated at Expansion is bit-for-bit the sign-exact branch — never a rounded leaf.
    static Expansion IExact<Expansion>.Of(double value) => Single(value);
    static Expansion IExact<Expansion>.Diff(double a, double b) => TwoSum(a, -b);
    static Expansion IExact<Expansion>.Prod(double a, double b) => TwoProduct(a, b);
    Expansion IExact<Expansion>.Add(Expansion other) => Sum(this, other);
    Expansion IExact<Expansion>.Sub(Expansion other) => Difference(this, other);
    Expansion IExact<Expansion>.Mul(Expansion other) => Multiply(this, other);
    Expansion IExact<Expansion>.Scale(double exact) => Scale(this, exact);
    Sign? IExact<Expansion>.Verdict => SignOf(this);

    // --- [RATIONAL_LIFT]
    // Exact bridge into the rational oracle: every nonoverlapping component is a lossless
    // `(Fraction)double`, summed in infinite-precision `Fraction` so the expansion's exact real value
    // crosses to the BigInteger tier without a rounding.
    public Fraction ToFraction() {
        Fraction acc = Fraction.Zero;
        for (int i = 0; i < length; i++) acc = Fraction.Add(acc, (Fraction)components[i]);
        return acc;
    }

    // The nonoverlapping components in magnitude order — the exact bridge the INDEPENDENT
    // `RationalOracle.BinaryOf` `EFloat`/`ERational` second-source adjudicator lifts losslessly.
    public ReadOnlySpan<double> Components => components.AsSpan(0, length);

    static Expansion Pair(double small, double large) =>
        small == 0.0 ? Single(large) : new Expansion(new[] { small, large }, 2);
}

// The directed-rounding bracket of the implicit filter tier: 53-bit EFloat endpoints under frozen
// Floor/Ceiling contexts. Every operation widens the bracket conservatively, so a resolved Verdict
// is a PROOF of the exact sign; the cost is fixed per operation — no BigInteger growth on the filter.
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

    // One verdict protocol across every tier — a determinate Sign, or null to escalate: the double
    // filter, the 106-bit refine, and Interval.Verdict all speak it, so every ladder is a `??`-chain.
    public Sign? Of(double det, double permanent) =>
        Math.Abs(det) > Coefficient * permanent ? Sign.Of(det) : null;

    public Sign? Refine(ddouble det, ddouble permanent) =>
        ddouble.Abs(det) > RefineCoefficient * permanent ? Sign.Of(ddouble.Sign(det)) : null;
}
```

## [04]-[DENSITY_BAR]

One owner per axis; capability is a member, case, or row, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes — pure verdicts because every predicate result is total and exact; no `GeometryFault` routes here (a sign is always defined, even for coincident or degenerate-construction input where the verdict is `Zero`).

| [INDEX] | [AXIS/CONCERN]              | [OWNER]           | [KIND]                                                                                                                             | [RAIL]                                            | [CASES] |
| :-----: | :-------------------------- | :---------------- | :---------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------- | :-----: |
|  [01]   | Sign verdict                | `Sign`            | `[SmartEnum<int>]` ternary + `Of`/`Flip`/`Times` verdict algebra                                                                    | `Sign.Of → Sign` (pure, total)                     |    3    |
|  [02]   | Coordinate vocabulary       | `Axis`            | `[SmartEnum<int>]` — Key = coordinate ordinal, `U`/`V` = projected-plane ordinals (the one generator over the plane family)         | data columns read by every axis-projected member   |    3    |
|  [03]   | Constructed-point carrier   | `Implicit`        | `[Union<Point3d,Ssi,Lpi,Tpi>]` ad-hoc — defining points only + `Homogeneous<T>` derivation + the ONE `Round()` emission projection | `Homogeneous<T>` (pure, exact); `Round` (emission) |    4    |
|  [04]   | Exact predicates            | `Predicate`       | ONE static family — 4 direct ladders (Interval-skipping) + axis-projected implicit `Orient2D` + `Compare` order key + implicit in-circum pair (spanning the whole explicit/implicit × axis lattice) | `Predicate.<member> → Sign` (pure, total, exact)   |    8    |
|  [05]   | Precision ladder            | `PrecisionTier`   | `[SmartEnum<int>]` ordered tiers (`Double`/`DoubleDouble`/`Interval`/`Expansion`/`Rational`)                                        | escalation order (pure)                            |    5    |
|  [3a]   | Implicit constructions      | `Ssi`/`Lpi`/`Tpi` | `readonly record struct` defining-point carriers (4/5/9 points) with one generic `Homogeneous<T>` polynomial each                   | `Homogeneous<T>` (pure, exact, no rounding)        |    3    |
|  [3b]   | Exact-carrier algebra       | `IExact<TSelf>`   | static-abstract interface — one polynomial serves both carriers                                                                    | constraint (compile-time)                          |    8    |
|  [3c]   | Expansion arithmetic        | `Expansion`       | `readonly struct` + `TwoSum`/`TwoProduct`(2 rows)/`Sum`/`Difference`/`Scale`/`Multiply`/`SignOf`/`ToFraction` fold                  | `Expansion.SignOf → Sign` (pure, exact)            |    7    |
|  [3d]   | Interval bracket            | `Interval`        | `readonly struct` — directed-rounding 53-bit `EFloat` endpoints, sound `Verdict`                                                   | `Interval.Verdict → Sign?` (pure, sound)           |    4    |
|  [3e]   | Tier filter/refine          | `ErrorBound`      | static-permanence `record` rows + `Of`/`Refine → Sign?` filter and 106-bit verdicts (one protocol)                                            | `ErrorBound.Of`/`Refine` (pure)                    |    4    |
|  [3f]   | Rational oracle             | `RationalOracle`  | static exact adjudicators — `Fraction` primary (`Orient2D`/`Orient3D`/`InCircle`/`InSphere`/`InCircum`) + `EFloat` independent (`BinaryOf → Sign?`, self-certifying) | `RationalOracle.InCircum → Sign` (pure, exact)     |    6    |
|  [3g]   | Interior-double policy      | `NumericsPolicy`  | static owner — strict-IEEE/RID invariant, interior-double scope, error-bound coefficients, `HardwareFma` gate + `Splitter`          | constants + one capability static                  |    —    |

## [05]-[RESEARCH]

- [NUMERIC_DETERMINISM] — the sign-exactness law-matrix the spec rail asserts. The harness is `PredicateLaws` (a CsCheck property suite): it generates near-degenerate input by perturbing collinear/cocircular/cospherical configurations at the `Epsilon` scale and asserts (1) every `Predicate` member — direct AND implicit — returns the EXACT sign of the rational-arithmetic determinant the `RationalOracle` `Fraction.Sign` computes over the lossless `(Fraction)double` decomposition of the same ordinates (for implicit members, over the exact rational value of the constructed point); (1b) the FOUR-WAY differential agreement — the `Expansion.SignOf` exact branch, the `RationalOracle.BinaryOf` `PeterO.Numbers` `EFloat` binary adjudicator, the `Fraction.Sign` `ExtendedNumerics` rational oracle, and the `ERational.Sign` `PeterO.Numbers` second rational oracle all return the SAME determinant sign across FOUR unrelated arbitrary-precision implementations (no two share a representation), so a shared-implementation blind spot cannot pass; (1c) `BinaryOf` is self-certifying — `EContext.WithBlankFlags()` records any `FlagInexact` and a raised flag returns null (a failed proof, never a fabricated verdict), so the matrix asserts `BinaryOf` resolved AND agreed; (2) the antisymmetry law `Orient2D(a,b,c) == Orient2D(b,a,c).Flip` and its 3D/incircle/insphere analogues hold under coordinate permutation parity, extended to the implicit family under defining-point permutation within each construction's symmetry group; (3) every ladder tier agrees with the oracle on the set it resolves — the `double` filter where `Of` resolves, the `ddouble` `Refine` where `Some`, the `Interval` bracket where `Verdict` resolves, and the `Expansion` branch elsewhere — so both ladders are monotone (a tier never reverses a coarser tier's determinate sign) and FILTER SOUNDNESS holds by construction: a resolved `Interval.Verdict` always equals the oracle sign because the directed bracket contains the real value; (4) the predicates are translation-invariant (a uniform `Point3d` offset never flips a direct verdict; an implicit verdict is invariant under uniform offset of all defining points); (5) `Compare` is a strict weak order per axis — antisymmetric, transitive, and equal to the sign of the exact rational coordinate difference computed from `Homogeneous` (wherever `lambda != 0`); `Round()`-materialized doubles preserve that order only outside the rounding band, which is exactly why the exact key exists. The harness needs NO live-host probe — `Fraction`/`ddouble`/`EFloat`/`ERational`/`Math.FusedMultiplyAdd`/`Point3d` are stable; the kernel is total by construction. The Dekker-split `TwoProduct` row is LANDED behind `NumericsPolicy.HardwareFma`; the one deferred numeric probe is confirming that row's bit-exactness against the FMA row on a genuinely FMA-free target — it gates nothing on osx-arm64/x64-FMA, where the FMA row is the taken branch.
- [INDIRECT_PREDICATES] — the implicit family extends the floor from explicit-coordinate predicates to constructed intersection points (Attene/Cherchi indirect predicates) whose coordinates are NEVER materialized as rounded doubles: `Ssi` (two coplanar segments — FOUR defining points + the construction plane), `Lpi` (line × three-point plane — FIVE defining points, three numerators + `lambda`), `Tpi` (three planes EACH BY THREE POINTS — NINE defining points; a pre-rounded `Plane` normal input is the dead derived form) each carry defining points only and derive exact homogeneous coordinates on demand through ONE generic polynomial per construction, instantiated at `Interval` for the filter and `Expansion` for the exact branch — the filter provably tests the same polynomial the exact branch decides. Every leaf difference rides the error-free `IExact.Diff` transform; the dead rounded-`Denominator`/`Estimate()`-inside-the-exact-carrier shape is deleted. The orientation and order-key folds flip the verdict by `Sign.Times` over the exact sign of every odd-power `lambda` (a zero `lambda` — parallel operands — yields `Zero`, the degeneracy witness); the in-circum folds clear the query denominator by `lambda^2` on every explicit-row lift and adjudicate through `RationalOracle.InCircum` — the `Expansion.ToFraction` exact lift composed with the exact `lambda.ToFraction().Sign` at the polynomial's structural parity (`lambdaDegree` 4 even for in-circle, 5 odd for in-sphere) — an unconditional flip is wrong for exactly one of the two parities, so the parity is data, never a shared constant. This is the realized indirect-predicate family the exact-arrangement `Constraint` carriage, the constrained-Delaunay Steiner recovery, the self-intersect re-mesh crossings, the slice-stack contour ordering, and the intersection chain-merge keys compose — each carries defining-entity ids on its own page and feeds this kernel the defining points, materializing a rounded coordinate ONLY at its emission seam through `Implicit.Round()`.
- [PRECISION_LADDER] — the `PrecisionTier` five-tier contract: `Double` (IEEE forward-error filter, direct members), `DoubleDouble` (`ddouble` 106-bit EFT refine, direct members), `Interval` (directed-rounding 53-bit `EFloat` bracket, implicit members — the tier that kills both the false no-filter-possible rationale and the BigInteger-per-test throughput gap on the implicit family), `Expansion` (sign-exact nonoverlapping expansion, both ladders), `Rational` (`Fraction` `BigInteger` oracle — the implicit in-circum runtime terminal and the law-matrix ground truth, with `PeterO` `EFloat`/`ERational` the INDEPENDENT second-source adjudicators at the same altitude in unrelated representations). Direct members walk filter → refine → exact inline and allocation-free; implicit members walk bracket → exact (→ rational for in-circum). The ladder is MONOTONE: `ddouble` shares the `TwoProduct`/`TwoSum` rounding model with `Expansion` so the two never disagree on a sign both resolve, the `Interval` bracket is sound by directed rounding so it can only accept true signs, and the `Fraction`/`EFloat`/`ERational` tiers are the infinite-precision ceiling every faster tier is differentially fuzzed against. The cost profile is the design: arbitrary-precision adjudication is paid only on the measure-zero degenerate set; the `ddouble` refine is a fixed two-`double` recompute; the `Interval` bracket is a fixed small-mantissa cost per implicit test. This is the `PRECISION_LADDER` anchor the `api-doubledouble`, `api-bigrational`, and `api-peteronumbers` catalogs reference as the home of their middle, primary-top, interval-filter, and independent-adjudicator tiers.
