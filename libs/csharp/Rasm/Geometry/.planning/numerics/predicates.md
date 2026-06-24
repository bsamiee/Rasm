# [RASM_NUMERICS_PREDICATES]

The adaptive-precision exact-predicate floor every higher geometry owner rides, authored from first principles because no admitted external geometry library carries a robustness guarantee. The page owns the closed `Predicate` family — `Orient2D`/`Orient3D`/`InCircle`/`InSphere`, each a two-stage adaptive fold: a fast IEEE-754 `double` filter gated by a forward error bound, falling back to exact expansion arithmetic only where the filtered interval straddles zero, returning an exact `Sign` verdict in every case. Under it sit `Expansion`, the sign-exact nonoverlapping floating-point expansion kernel, and `ErrorBound`, the static-permanence error-bound row table; `NumericsPolicy` names the filter-then-exact contract and the sanctioned interior-double scope. The kernel composes `Vectors` `Point3d` coordinates at the seam, emits only `Sign` verdicts, computes no hash, and mints no identity.

`Expansion` and `ErrorBound` operate on raw `double` because robust arithmetic is mathematically defined over IEEE-754 doubles, never unit-bearing quantities. This kernel and the healing weld inner loop are the only sanctioned interior-double owners; `double` crosses a public signature only as a `Point3d` coordinate (the domain's native scalar) at the seam. An interior `double` escaping a public predicate signature elsewhere is the named seam violation.

## [01]-[INDEX]

- [01]-[ROBUST_PREDICATES]: Closed `Predicate` family — `Orient2D`/`Orient3D`/`InCircle`/`InSphere` direct plus `OrientLPI`/`OrientTPI`/`InCircleLPI`/`InSphereTPI` implicit-point — filter-then-exact two-stage adaptive evaluation returning an exact `Sign`.
- [02]-[INTERIOR_NUMERICS]: `Expansion` sign-exact expansion arithmetic; `ErrorBound` static-permanence filter rows; `Lpi`/`Tpi` exact implicit-point homogeneous constructions; `NumericsPolicy` the interior-double scope owner.

## [02]-[ROBUST_PREDICATES]

- Owner: `Sign` `[SmartEnum<int>]` the closed ternary verdict (`Negative`/`Zero`/`Positive`, key `-1`/`0`/`+1`) every predicate returns; `Predicate` the static surface whose four members `Orient2D`/`Orient3D`/`InCircle`/`InSphere` each run the two-stage `ErrorBound`-gated fold — a fast `double` determinant filter, then an `Expansion` exact fallback only where the filtered interval straddles zero; coordinates arrive as `Vectors` `Point3d` and the predicate reads `.X`/`.Y`/`.Z` once at the seam.
- Cases: `Sign` rows `Negative` · `Zero` · `Positive` (3); `Predicate` members `Orient2D` (2D left-turn) · `Orient3D` (3D above-plane) · `InCircle` (2D Delaunay in-circle) · `InSphere` (3D in-sphere) · `OrientLPI` (2D orientation against a line-line implicit intersection point) · `OrientTPI` (2D orientation against a three-plane implicit intersection point) · `InCircleLPI` (2D in-circle against a line-line implicit point) · `InSphereTPI` (3D in-sphere against a three-plane implicit point) (8); the four direct members are each one filter-then-exact fold, never a sibling fast/exact pair; the four implicit-point members (two orientation, two in-circum) are exact-only — each evaluates the sign over a CONSTRUCTED intersection point (its homogeneous numerator/denominator carried in `Lpi`/`Tpi` as `Expansion`s) without ever materializing a rounded coordinate, so no `double` filter applies (a filter would require the very rounded coordinate the construction forbids) and the member runs the `Expansion` fold directly — the in-circum pair lifts the homogeneous point into the in-circle/in-sphere determinant by carrying the `lambda` denominator through the lift (the lifted column is `(numerator·lambda^{-1})` so the lift squares the denominator, folded back into the explicit-vertex lift terms as `lambda^2`) and flips the verdict on a negative denominator so the exact sign tracks the un-normalized homogeneous point.
- Entry: `public static Sign Orient2D(Point3d a, Point3d b, Point3d c)` returns the exact sign of the `(b-a)×(c-a)` 2D cross-product determinant — positive for a counter-clockwise turn; `public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d)` the exact sign of the `3×3` determinant placing `d` relative to the oriented plane `abc`; `public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d)` the exact sign of the `4×4` lifted determinant testing `d` inside the circumcircle of `abc`; `public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e)` the exact sign of the `5×5` lifted determinant testing `e` inside the circumsphere of `abcd`; `public static Sign InCircleLPI(Point3d a, Point3d b, Point3d c, Point3d p, Point3d q, Point3d r, Point3d s)` the exact in-circle sign of the line-line implicit point `Lpi.Of(p,q,r,s)` against the circle through `abc`; `public static Sign InSphereTPI(Point3d a, Point3d b, Point3d c, Point3d d, Plane p, Plane q, Plane r)` the exact in-sphere sign of the three-plane implicit point `Tpi.Of(p,q,r)` against the sphere through `abcd` — every member total, pure, exact, no rail.
- Auto: each predicate computes the approximate determinant and its permanence magnitude in one `double` expression each, then dispatches on `ErrorBound.Of(det, permanent)` — a total `switch` over the `Stage` projection; `Stage.Filtered` proves `|det|` clears the permanence-scaled threshold and the sign is the `double` sign by `Sign.Of(det)` with zero exact work (the common fast path), `Stage.Exact` falls to the `Expansion` fold via `TwoProduct`/`TwoSum`/`ExpansionSum`/`ScaleExpansion` read through `Expansion.SignOf`, so the exact branch runs solely on near-degenerate input and the verdict is always the true sign of the real-arithmetic determinant.
- Receipt: none — a predicate returns a `Sign` verdict, the most refined receipt a total exact test admits; the prior notion of a per-predicate evidence record is the deleted form because a sign carries no residual.
- Packages: Thinktecture.Runtime.Extensions, Rasm.Vectors (project, `Point3d`/`Direction` vocabulary), BCL inbox (`System.Math` FMA/IEEE)
- Growth: a new predicate (`ParallelOrder`, `CompareDistance`, segment-segment intersection sign, Delaunay-flip sign) is one `Predicate` member riding the same `ErrorBound` stage table and `Expansion` fold — the `InCircleLPI`/`InSphereTPI` implicit-point in-circum members the constrained-Delaunay Steiner recovery and the `arrangement` cell-classification compose are LANDED as exactly that case-row growth, two members on this owner riding the same `Expansion` homogeneous fold and `Sign` verdict the `OrientLPI`/`OrientTPI` orientation members use, never a sibling in-circum type; a new implicit-point construction is one `Lpi`/`Tpi`-style homogeneous-coordinate struct carrying its numerator/denominator `Expansion`s (exact-only, no filter row, because its coordinate is never rounded); a new direct-predicate error band is one `ErrorBound` row; zero new surface, no sibling fast/exact predicate types.
- Boundary: the four predicates are members on ONE `Predicate` static owner and a per-predicate class or a separate `FastOrient2D`/`ExactOrient2D` pair is the deleted form — the two stages are branches inside one member, gated by `ErrorBound.Stage`, never two surfaces; the verdict union is the closed `Sign` SmartEnum and a raw `int`/`double` sign leaking across a public signature is the named defect, callers match on `Sign.Negative`/`Sign.Zero`/`Sign.Positive`; coordinates are `Vectors` `Point3d` read at the seam and a domain-local point struct is the deleted form; the fast filter is the IEEE-754 `double` determinant gated by `ErrorBound`, the exact fallback is the `Expansion` fold, and loosening a predicate to pass a near-degenerate case by widening the filter band instead of taking the exact branch is the named correctness defect — a sign verdict is exact or it is a defect; the interior `double` arithmetic inside the filter and the `Expansion` fold is the sanctioned scope owned by `NumericsPolicy`, and an interior `double` escaping a public predicate signature is the named seam violation.

```csharp
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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Predicate {
    // --- [ORIENT_2D]
    public static Sign Orient2D(Point3d a, Point3d b, Point3d c) {
        double acx = a.X - c.X, bcx = b.X - c.X, acy = a.Y - c.Y, bcy = b.Y - c.Y;
        double detLeft = acx * bcy, detRight = acy * bcx;
        double det = detLeft - detRight;
        double detsum = Math.Abs(detLeft) + Math.Abs(detRight);
        return ErrorBound.Orient2D.Of(det, detsum) switch {
            ErrorBound.Stage.Filtered => Sign.Of(det),
            _ => Orient2DExact(a, b, c),
        };
    }

    // --- [ORIENT_3D]
    public static Sign Orient3D(Point3d a, Point3d b, Point3d c, Point3d d) {
        double adx = a.X - d.X, bdx = b.X - d.X, cdx = c.X - d.X;
        double ady = a.Y - d.Y, bdy = b.Y - d.Y, cdy = c.Y - d.Y;
        double adz = a.Z - d.Z, bdz = b.Z - d.Z, cdz = c.Z - d.Z;
        double bdxcdy = bdx * cdy, cdxbdy = cdx * bdy;
        double cdxady = cdx * ady, adxcdy = adx * cdy;
        double adxbdy = adx * bdy, bdxady = bdx * ady;
        double det = adz * (bdxcdy - cdxbdy) + bdz * (cdxady - adxcdy) + cdz * (adxbdy - bdxady);
        double permanent =
            (Math.Abs(bdxcdy) + Math.Abs(cdxbdy)) * Math.Abs(adz)
            + (Math.Abs(cdxady) + Math.Abs(adxcdy)) * Math.Abs(bdz)
            + (Math.Abs(adxbdy) + Math.Abs(bdxady)) * Math.Abs(cdz);
        return ErrorBound.Orient3D.Of(det, permanent) switch {
            ErrorBound.Stage.Filtered => Sign.Of(det),
            _ => Orient3DExact(a, b, c, d),
        };
    }

    // --- [IN_CIRCLE]
    public static Sign InCircle(Point3d a, Point3d b, Point3d c, Point3d d) {
        double adx = a.X - d.X, bdx = b.X - d.X, cdx = c.X - d.X;
        double ady = a.Y - d.Y, bdy = b.Y - d.Y, cdy = c.Y - d.Y;
        double bdxcdy = bdx * cdy, cdxbdy = cdx * bdy, alift = adx * adx + ady * ady;
        double cdxady = cdx * ady, adxcdy = adx * cdy, blift = bdx * bdx + bdy * bdy;
        double adxbdy = adx * bdy, bdxady = bdx * ady, clift = cdx * cdx + cdy * cdy;
        double det = alift * (bdxcdy - cdxbdy) + blift * (cdxady - adxcdy) + clift * (adxbdy - bdxady);
        double permanent =
            (Math.Abs(bdxcdy) + Math.Abs(cdxbdy)) * alift
            + (Math.Abs(cdxady) + Math.Abs(adxcdy)) * blift
            + (Math.Abs(adxbdy) + Math.Abs(bdxady)) * clift;
        return ErrorBound.InCircle.Of(det, permanent) switch {
            ErrorBound.Stage.Filtered => Sign.Of(det),
            _ => InCircleExact(a, b, c, d),
        };
    }

    // --- [IN_SPHERE]
    public static Sign InSphere(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) {
        double aex = a.X - e.X, bex = b.X - e.X, cex = c.X - e.X, dex = d.X - e.X;
        double aey = a.Y - e.Y, bey = b.Y - e.Y, cey = c.Y - e.Y, dey = d.Y - e.Y;
        double aez = a.Z - e.Z, bez = b.Z - e.Z, cez = c.Z - e.Z, dez = d.Z - e.Z;
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
        return ErrorBound.InSphere.Of(det, permanent) switch {
            ErrorBound.Stage.Filtered => Sign.Of(det),
            _ => InSphereExact(a, b, c, d, e),
        };
    }

    // --- [EXACT_FALLBACKS]
    static Sign Orient2DExact(Point3d a, Point3d b, Point3d c) {
        Expansion left = Expansion.TwoProduct(a.X - c.X, b.Y - c.Y);
        Expansion right = Expansion.TwoProduct(a.Y - c.Y, b.X - c.X);
        return Expansion.SignOf(Expansion.Difference(left, right));
    }

    static Sign Orient3DExact(Point3d a, Point3d b, Point3d c, Point3d d) {
        Expansion bc = Expansion.Difference(Expansion.TwoProduct(b.X - d.X, c.Y - d.Y), Expansion.TwoProduct(c.X - d.X, b.Y - d.Y));
        Expansion ca = Expansion.Difference(Expansion.TwoProduct(c.X - d.X, a.Y - d.Y), Expansion.TwoProduct(a.X - d.X, c.Y - d.Y));
        Expansion ab = Expansion.Difference(Expansion.TwoProduct(a.X - d.X, b.Y - d.Y), Expansion.TwoProduct(b.X - d.X, a.Y - d.Y));
        Expansion det = Expansion.Sum(
            Expansion.Scale(bc, a.Z - d.Z),
            Expansion.Sum(Expansion.Scale(ca, b.Z - d.Z), Expansion.Scale(ab, c.Z - d.Z)));
        return Expansion.SignOf(det);
    }

    static Sign InCircleExact(Point3d a, Point3d b, Point3d c, Point3d d) {
        Expansion bc = Expansion.Difference(Expansion.TwoProduct(b.X - d.X, c.Y - d.Y), Expansion.TwoProduct(c.X - d.X, b.Y - d.Y));
        Expansion ca = Expansion.Difference(Expansion.TwoProduct(c.X - d.X, a.Y - d.Y), Expansion.TwoProduct(a.X - d.X, c.Y - d.Y));
        Expansion ab = Expansion.Difference(Expansion.TwoProduct(a.X - d.X, b.Y - d.Y), Expansion.TwoProduct(b.X - d.X, a.Y - d.Y));
        Expansion alift = Lift2(a.X - d.X, a.Y - d.Y), blift = Lift2(b.X - d.X, b.Y - d.Y), clift = Lift2(c.X - d.X, c.Y - d.Y);
        Expansion det = Expansion.Sum(
            Expansion.Multiply(alift, bc),
            Expansion.Sum(Expansion.Multiply(blift, ca), Expansion.Multiply(clift, ab)));
        return Expansion.SignOf(det);
    }

    static Sign InSphereExact(Point3d a, Point3d b, Point3d c, Point3d d, Point3d e) {
        Expansion abc = Minor3(b, c, d_: a, e), bcd = Minor3(c, d, d_: b, e), cda = Minor3(d, a, d_: c, e), dab = Minor3(a, b, d_: d, e);
        Expansion det = Expansion.Sum(
            Expansion.Difference(Expansion.Multiply(Lift3(d.X - e.X, d.Y - e.Y, d.Z - e.Z), abc), Expansion.Multiply(Lift3(c.X - e.X, c.Y - e.Y, c.Z - e.Z), dab)),
            Expansion.Difference(Expansion.Multiply(Lift3(b.X - e.X, b.Y - e.Y, b.Z - e.Z), cda), Expansion.Multiply(Lift3(a.X - e.X, a.Y - e.Y, a.Z - e.Z), bcd)));
        return Expansion.SignOf(det);
    }

    // --- [IMPLICIT_POINTS]
    public static Sign OrientLPI(Point3d a, Point3d b, Point3d p, Point3d q, Point3d r, Point3d s) {
        Lpi point = Lpi.Of(p, q, r, s);
        Expansion det = Expansion.Difference(
            Expansion.Multiply(point.Lambda, OrientHomogeneous(a, b, point.Numerator)),
            Expansion.Scale(OrientExplicit(a, b), point.Denominator));
        return Expansion.SignOf(det);
    }

    public static Sign OrientTPI(Point3d a, Point3d b, Plane p, Plane q, Plane r) {
        Tpi point = Tpi.Of(p, q, r);
        Expansion det = Expansion.Difference(
            Expansion.Multiply(point.Lambda, OrientHomogeneous(a, b, point.Numerator)),
            Expansion.Scale(OrientExplicit(a, b), point.Denominator));
        return Expansion.SignOf(det);
    }

    // --- [IMPLICIT_IN_CIRCUM]
    public static Sign InCircleLPI(Point3d a, Point3d b, Point3d c, Point3d p, Point3d q, Point3d r, Point3d s) {
        Lpi point = Lpi.Of(p, q, r, s);
        Expansion det = InCircleHomogeneous(a, b, c, point.Numerator, point.Lambda);
        return point.Denominator < 0.0 ? Expansion.SignOf(det).Flip : Expansion.SignOf(det);
    }

    public static Sign InSphereTPI(Point3d a, Point3d b, Point3d c, Point3d d, Plane p, Plane q, Plane r) {
        Tpi point = Tpi.Of(p, q, r);
        Expansion det = InSphereHomogeneous(a, b, c, d, point.Numerator, point.Lambda);
        return point.Denominator < 0.0 ? Expansion.SignOf(det).Flip : Expansion.SignOf(det);
    }

    static Expansion InCircleHomogeneous(Point3d a, Point3d b, Point3d c, (Expansion X, Expansion Y) e, Expansion lambda) {
        Expansion ex = Expansion.Difference(e.X, Expansion.Scale(lambda, a.X)), ey = Expansion.Difference(e.Y, Expansion.Scale(lambda, a.Y));
        Expansion bx = Expansion.Scale(lambda, b.X - a.X), by = Expansion.Scale(lambda, b.Y - a.Y);
        Expansion cx = Expansion.Scale(lambda, c.X - a.X), cy = Expansion.Scale(lambda, c.Y - a.Y);
        Expansion bc = Expansion.Difference(Expansion.Multiply(bx, cy), Expansion.Multiply(cx, by));
        Expansion ce = Expansion.Difference(Expansion.Multiply(cx, ey), Expansion.Multiply(ex, cy));
        Expansion eb = Expansion.Difference(Expansion.Multiply(ex, by), Expansion.Multiply(bx, ey));
        Expansion eLift = Expansion.Sum(Expansion.Multiply(ex, ex), Expansion.Multiply(ey, ey));
        Expansion bLift = Expansion.Multiply(Lift2(b.X - a.X, b.Y - a.Y), Expansion.Multiply(lambda, lambda));
        Expansion cLift = Expansion.Multiply(Lift2(c.X - a.X, c.Y - a.Y), Expansion.Multiply(lambda, lambda));
        return Expansion.Sum(
            Expansion.Multiply(eLift, bc),
            Expansion.Sum(Expansion.Multiply(bLift, ce), Expansion.Multiply(cLift, eb)));
    }

    static Expansion InSphereHomogeneous(Point3d a, Point3d b, Point3d c, Point3d d, (Expansion X, Expansion Y) e, Expansion lambda) {
        Expansion ex = Expansion.Difference(e.X, Expansion.Scale(lambda, a.X)), ey = Expansion.Difference(e.Y, Expansion.Scale(lambda, a.Y));
        Expansion ez = Expansion.Scale(lambda, -a.Z);
        Expansion bm = Minor3Homogeneous(a, c, d, ex, ey, ez, lambda), cm = Minor3Homogeneous(a, d, b, ex, ey, ez, lambda), dm = Minor3Homogeneous(a, b, c, ex, ey, ez, lambda);
        Expansion eLift = Expansion.Sum(Expansion.Multiply(ex, ex), Expansion.Sum(Expansion.Multiply(ey, ey), Expansion.Multiply(ez, ez)));
        Expansion lam2 = Expansion.Multiply(lambda, lambda);
        Expansion bLift = Expansion.Multiply(Lift3(b.X - a.X, b.Y - a.Y, b.Z - a.Z), lam2);
        Expansion cLift = Expansion.Multiply(Lift3(c.X - a.X, c.Y - a.Y, c.Z - a.Z), lam2);
        Expansion dLift = Expansion.Multiply(Lift3(d.X - a.X, d.Y - a.Y, d.Z - a.Z), lam2);
        return Expansion.Sum(
            Expansion.Difference(Expansion.Multiply(eLift, dm), Expansion.Multiply(dLift, ExactMinor3(a, b, c, ex, ey, ez))),
            Expansion.Difference(Expansion.Multiply(bLift, cm), Expansion.Multiply(cLift, bm)));
    }

    static Expansion Minor3Homogeneous(Point3d a, Point3d p, Point3d q, Expansion ex, Expansion ey, Expansion ez, Expansion lambda) {
        Expansion px = Expansion.Scale(lambda, p.X - a.X), py = Expansion.Scale(lambda, p.Y - a.Y), pz = Expansion.Scale(lambda, p.Z - a.Z);
        Expansion qx = Expansion.Scale(lambda, q.X - a.X), qy = Expansion.Scale(lambda, q.Y - a.Y), qz = Expansion.Scale(lambda, q.Z - a.Z);
        Expansion pq = Expansion.Difference(Expansion.Multiply(px, qy), Expansion.Multiply(qx, py));
        Expansion qe = Expansion.Difference(Expansion.Multiply(qx, ey), Expansion.Multiply(ex, qy));
        Expansion ep = Expansion.Difference(Expansion.Multiply(ex, py), Expansion.Multiply(px, ey));
        return Expansion.Sum(Expansion.Multiply(ez, pq), Expansion.Sum(Expansion.Multiply(pz, qe), Expansion.Multiply(qz, ep)));
    }

    static Expansion ExactMinor3(Point3d a, Point3d b, Point3d c, Expansion ex, Expansion ey, Expansion ez) {
        Expansion bx = Expansion.Single(b.X - a.X), by = Expansion.Single(b.Y - a.Y), bz = Expansion.Single(b.Z - a.Z);
        Expansion cx = Expansion.Single(c.X - a.X), cy = Expansion.Single(c.Y - a.Y), cz = Expansion.Single(c.Z - a.Z);
        Expansion bc = Expansion.Difference(Expansion.Multiply(bx, cy), Expansion.Multiply(cx, by));
        Expansion ce = Expansion.Difference(Expansion.Multiply(cx, ey), Expansion.Multiply(ex, cy));
        Expansion eb = Expansion.Difference(Expansion.Multiply(ex, by), Expansion.Multiply(bx, ey));
        return Expansion.Sum(Expansion.Multiply(ez, bc), Expansion.Sum(Expansion.Multiply(bz, ce), Expansion.Multiply(cz, eb)));
    }

    static Expansion OrientExplicit(Point3d a, Point3d b) =>
        Expansion.Difference(Expansion.TwoProduct(b.X, a.Y), Expansion.TwoProduct(a.X, b.Y));

    static Expansion OrientHomogeneous(Point3d a, Point3d b, (Expansion X, Expansion Y) numerator) =>
        Expansion.Sum(
            Expansion.Difference(Expansion.Scale(numerator.Y, b.X - a.X), Expansion.Scale(numerator.X, b.Y - a.Y)),
            Expansion.Difference(Expansion.TwoProduct(a.X, b.Y), Expansion.TwoProduct(b.X, a.Y)));

    // --- [LIFTS_AND_MINORS]
    static Expansion Lift2(double x, double y) => Expansion.Sum(Expansion.TwoProduct(x, x), Expansion.TwoProduct(y, y));
    static Expansion Lift3(double x, double y, double z) => Expansion.Sum(Expansion.TwoProduct(x, x), Expansion.Sum(Expansion.TwoProduct(y, y), Expansion.TwoProduct(z, z)));

    static Expansion Minor3(Point3d p, Point3d q, Point3d d_, Point3d e) {
        Expansion pq = Expansion.Difference(Expansion.TwoProduct(p.X - e.X, q.Y - e.Y), Expansion.TwoProduct(q.X - e.X, p.Y - e.Y));
        Expansion qd = Expansion.Difference(Expansion.TwoProduct(q.X - e.X, d_.Y - e.Y), Expansion.TwoProduct(d_.X - e.X, q.Y - e.Y));
        Expansion dp = Expansion.Difference(Expansion.TwoProduct(d_.X - e.X, p.Y - e.Y), Expansion.TwoProduct(p.X - e.X, d_.Y - e.Y));
        return Expansion.Sum(Expansion.Scale(pq, d_.Z - e.Z), Expansion.Sum(Expansion.Scale(qd, p.Z - e.Z), Expansion.Scale(dp, q.Z - e.Z)));
    }
}
```

## [03]-[INTERIOR_NUMERICS]

- Owner: `Expansion` the `readonly struct` nonoverlapping floating-point expansion (a small per-fold-sized component buffer, most significant component last) with the `TwoSum`/`TwoProduct`/`Grow`/`ScaleExpansion`/`ExpansionSum` construction folds and the `Estimate`/`SignOf` projections — the sign-exact arithmetic the predicates' exact branch rides; `ErrorBound` the `record` static-permanence filter-row table (`Orient2D`/`Orient3D`/`InCircle`/`InSphere` rows, each carrying its forward error coefficient derived once from `Epsilon`) with the `Of` → `Stage` projection; `NumericsPolicy` the static owner documenting the filter-then-exact contract and the error-bound constants — the named, scoped sanction for raw interior `double` arithmetic.
- Cases: `Expansion` construction folds `TwoSum` · `TwoProduct` · `Grow` · `ScaleExpansion` · `ExpansionSum` (5) plus the `Estimate`/`SignOf` projections; `ErrorBound` rows `Orient2D` · `Orient3D` · `InCircle` · `InSphere` (4) — the permanence axis is one row per predicate, never a parallel threshold owner.
- Entry: `public static Expansion TwoProduct(double a, double b)` returns the exact two-component product `a*b` via `Math.FusedMultiplyAdd` (the error component is `fma(a,b,-(a*b))`, no Dekker split needed on FMA hardware); `public static Expansion TwoSum(double a, double b)` the exact two-component sum with Knuth's branch-free rounding-error recovery; `public static Expansion Sum(Expansion left, Expansion right)` and `public static Expansion Scale(Expansion e, double scalar)` the expansion-sum and scale-expansion folds growing the nonoverlapping sequence; `public static Sign SignOf(Expansion e)` reads the sign of the most-significant nonzero component (exact because the sequence is nonoverlapping and sorted by magnitude); `public ErrorBound.Stage Of(double det, double permanent)` projects `Stage.Filtered`/`Stage.Exact` from whether a `double` determinant clears the permanence-scaled threshold.
- Auto: `TwoProduct` and `TwoSum` are error-free transforms — they recover the rounding error each IEEE-754 operation discards, so the two returned components sum to the exact real result with no precision loss; `Sum`/`Scale` thread those transforms through Shewchuk's `fast-expansion-sum` and `scale-expansion` algorithms, each maintaining the nonoverlapping invariant by `TwoSum`-merging components in magnitude order so `SignOf` reads the true sign from the top nonzero term; `ErrorBound.Of` compares `|det|` against `coefficient * permanent` where the per-row coefficient is the static-permanence forward error bound computed once from `Epsilon` (the binary64 unit roundoff `2^-53`) at type construction — `Stage.Filtered` proves the `double` sign is the exact sign and the predicate skips the `Expansion` fold entirely.
- Receipt: none — `Expansion` is interior arithmetic, `ErrorBound` is a filter, `NumericsPolicy` is a policy owner; none crosses a public signature carrying a residual. The exact result is the `Sign` the predicate returns and the only public artifact.
- Packages: Thinktecture.Runtime.Extensions, BCL inbox (`System.Math.FusedMultiplyAdd`, `double.Epsilon`/`BitIncrement`), no external geometry or arbitrary-precision dependency
- Growth: a new predicate's filter is one `ErrorBound` row carrying its forward error coefficient; a longer exact computation grows the `Expansion` component capacity, never a parallel arbitrary-precision type; the interior-double scope widens to a new kernel only by naming it in `NumericsPolicy`, never by leaking interior `double` past a public signature elsewhere; zero new surface.
- Boundary: `Expansion` is ONE owner for sign-exact arithmetic and a separate `TwoSum`/`TwoProduct`/`GrowExpansion` free-function set or a parallel `BigFloat`/`MPFR`-style arbitrary-precision type is the deleted form — the construction folds are static members on the struct, the nonoverlapping component buffer is the single representation; `TwoProduct` rides `Math.FusedMultiplyAdd` for the exact product error and a Dekker-split branch is the deleted form on FMA-capable targets (the split survives only as the named fallback if an FMA-free RID enters — a RESEARCH probe, never a second product type); `ErrorBound` is the single permanence-threshold table and a per-predicate magic-number literal inlined at the call site is the named defect, the coefficient is a row column derived once from `Epsilon`; `NumericsPolicy` is the interior-double scope owner and the interior `double` arithmetic of `Expansion`/`ErrorBound`/the filter stage is the ONLY sanctioned interior-double scope — an interior `double` anywhere outside this kernel and the healing weld inner loop is the named seam violation; the predicates never under-refine to pass a degenerate case, the exact branch is mandatory where the filter band is straddled, and the `Expansion.SignOf` verdict is the true sign of the real-arithmetic determinant or it is a defect; `double` is exposed only at the seam as the canonical geometric coordinate carried by `Point3d`, never as a unit-bearing quantity and never as an interior signature parameter outside this kernel.

```csharp
// --- [CONSTANTS] --------------------------------------------------------------------------
public static class NumericsPolicy {
    public const double Epsilon = 1.1102230246251565e-16;
    public const double Splitter = 134_217_729.0;

    public const double Orient2DBound = (3.0 + 16.0 * Epsilon) * Epsilon;
    public const double Orient3DBound = (7.0 + 56.0 * Epsilon) * Epsilon;
    public const double InCircleBound = (10.0 + 96.0 * Epsilon) * Epsilon;
    public const double InSphereBound = (16.0 + 224.0 * Epsilon) * Epsilon;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly struct Expansion {
    private readonly double[] components;
    private readonly int length;

    private Expansion(double[] components, int length) { this.components = components; this.length = length; }

    public static Expansion Single(double value) => new(new[] { value }, value == 0.0 ? 0 : 1);

    // --- [TWO_SUM]
    public static Expansion TwoSum(double a, double b) {
        double x = a + b;
        double bv = x - a;
        double y = (a - (x - bv)) + (b - bv);
        return Pair(y, x);
    }

    // --- [TWO_PRODUCT]
    public static Expansion TwoProduct(double a, double b) {
        double x = a * b;
        double y = Math.FusedMultiplyAdd(a, b, -x);
        return Pair(y, x);
    }

    // --- [GROW]
    public static Expansion Grow(Expansion e, double scalar) => Sum(e, Single(scalar));

    // --- [EXPANSION_SUM]
    public static Expansion Sum(Expansion left, Expansion right) {
        var merged = new double[left.length + right.length];
        int li = 0, ri = 0, written = 0;
        double carry = 0.0;
        while (li < left.length || ri < right.length) {
            double next =
                li >= left.length ? right.components[ri++]
                : ri >= right.length ? left.components[li++]
                : Math.Abs(left.components[li]) < Math.Abs(right.components[ri]) ? left.components[li++]
                : right.components[ri++];
            Expansion sum = TwoSum(carry, next);
            if (sum.components[0] != 0.0) merged[written++] = sum.components[0];
            carry = sum.length == 0 ? 0.0 : sum.components[sum.length - 1];
        }
        if (carry != 0.0 || written == 0) merged[written++] = carry;
        return new Expansion(merged, written);
    }

    public static Expansion Difference(Expansion left, Expansion right) => Sum(left, Negate(right));

    // --- [SCALE_EXPANSION]
    public static Expansion Scale(Expansion e, double scalar) {
        if (e.length == 0 || scalar == 0.0) return Single(0.0);
        Expansion acc = TwoProduct(e.components[0], scalar);
        for (int i = 1; i < e.length; i++) acc = Sum(acc, TwoProduct(e.components[i], scalar));
        return acc;
    }

    // --- [MULTIPLY]
    public static Expansion Multiply(Expansion left, Expansion right) {
        if (left.length == 0 || right.length == 0) return Single(0.0);
        Expansion acc = Scale(left, right.components[0]);
        for (int i = 1; i < right.length; i++) acc = Sum(acc, Scale(left, right.components[i]));
        return acc;
    }

    public static Expansion Negate(Expansion e) {
        var flipped = new double[e.length];
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

    static Expansion Pair(double small, double large) =>
        small == 0.0 ? Single(large) : new Expansion(new[] { small, large }, 2);
}

public sealed record ErrorBound(double Coefficient) {
    public static readonly ErrorBound Orient2D = new(NumericsPolicy.Orient2DBound);
    public static readonly ErrorBound Orient3D = new(NumericsPolicy.Orient3DBound);
    public static readonly ErrorBound InCircle = new(NumericsPolicy.InCircleBound);
    public static readonly ErrorBound InSphere = new(NumericsPolicy.InSphereBound);

    public enum Stage { Filtered, Exact }

    public Stage Of(double det, double permanent) =>
        Math.Abs(det) > Coefficient * permanent ? Stage.Filtered : Stage.Exact;
}

public readonly record struct Lpi((Expansion X, Expansion Y) Numerator, Expansion Lambda, double Denominator) {
    public static Lpi Of(Point3d p, Point3d q, Point3d r, Point3d s) {
        Expansion d1 = Expansion.Difference(Expansion.TwoProduct(s.X - r.X, q.Y - p.Y), Expansion.TwoProduct(s.Y - r.Y, q.X - p.X));
        Expansion n = Expansion.Difference(Expansion.TwoProduct(s.X - p.X, q.Y - p.Y), Expansion.TwoProduct(s.Y - p.Y, q.X - p.X));
        return new Lpi(
            (Expansion.Sum(Expansion.Scale(d1, p.X), Expansion.Multiply(n, Expansion.Single(q.X - p.X))),
             Expansion.Sum(Expansion.Scale(d1, p.Y), Expansion.Multiply(n, Expansion.Single(q.Y - p.Y)))),
            d1, d1.Estimate());
    }
}

public readonly record struct Tpi((Expansion X, Expansion Y) Numerator, Expansion Lambda, double Denominator) {
    public static Tpi Of(Plane p, Plane q, Plane r) {
        Vector3d np = p.Normal, nq = q.Normal, nr = r.Normal;
        double dp = (np.X * p.Origin.X) + (np.Y * p.Origin.Y) + (np.Z * p.Origin.Z);
        double dq = (nq.X * q.Origin.X) + (nq.Y * q.Origin.Y) + (nq.Z * q.Origin.Z);
        double dr = (nr.X * r.Origin.X) + (nr.Y * r.Origin.Y) + (nr.Z * r.Origin.Z);
        Vector3d qr = Vector3d.CrossProduct(nq, nr), rp = Vector3d.CrossProduct(nr, np), pq = Vector3d.CrossProduct(np, nq);
        Expansion lambda = Expansion.Sum(
            Expansion.TwoProduct(np.X, qr.X),
            Expansion.Sum(Expansion.TwoProduct(np.Y, qr.Y), Expansion.TwoProduct(np.Z, qr.Z)));
        Expansion numX = Expansion.Sum(
            Expansion.TwoProduct(dp, qr.X),
            Expansion.Sum(Expansion.TwoProduct(dq, rp.X), Expansion.TwoProduct(dr, pq.X)));
        Expansion numY = Expansion.Sum(
            Expansion.TwoProduct(dp, qr.Y),
            Expansion.Sum(Expansion.TwoProduct(dq, rp.Y), Expansion.TwoProduct(dr, pq.Y)));
        return new Tpi((numX, numY), lambda, lambda.Estimate());
    }
}
```

## [04]-[DENSITY_BAR]

One owner per axis; capability is a member, case, or row, never a sibling surface. The `[RAIL]` cell names the one return rail each owner exposes — pure verdicts because every predicate result is total and exact; no `GeometryFault` routes here (a sign is always defined, even for coincident input where the verdict is `Zero`).

| [INDEX] | [AXIS/CONCERN]              | [OWNER]          | [KIND]                                                                                                                                                      | [RAIL]                                           | [CASES] |
| :-----: | :-------------------------- | :--------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------- | :-----: |
|  [01]   | Sign verdict                | `Sign`           | `[SmartEnum<int>]` ternary (`Negative`/`Zero`/`Positive`) + `Of`/`Flip`/`Times`                                                                             | `Sign.Of → Sign` (pure, total)                   |    3    |
|  [02]   | Exact predicates            | `Predicate`      | static surface + `Orient2D`/`Orient3D`/`InCircle`/`InSphere` filter-then-exact + `OrientLPI`/`OrientTPI`/`InCircleLPI`/`InSphereTPI` implicit-point members | `Predicate.Orient2D → Sign` (pure, total, exact) |    8    |
|  [2a]   | Expansion arithmetic        | `Expansion`      | `readonly struct` + `TwoSum`/`TwoProduct`/`Grow`/`Sum`/`Scale`/`Multiply`/`SignOf` fold                                                                     | `Expansion.SignOf → Sign` (pure, exact)          |    7    |
|  [2b]   | Filter stage                | `ErrorBound`     | static-permanence `record` rows (the four direct predicates) + `Of → Stage` projection                                                                      | `ErrorBound.Of → Stage` (pure)                   |    4    |
|  [2c]   | Implicit-point construction | `Lpi`/`Tpi`      | `readonly record struct` homogeneous numerator/denominator `Expansion`s for line-line / three-plane intersection points                                     | `Lpi.Of → Lpi` (pure, exact, no rounding)        |    —    |
|  [2d]   | Interior-double policy      | `NumericsPolicy` | static const owner — the interior-double scope + error-bound coefficients                                                                                   | constants (read by `Predicate`/`Expansion`)      |    —    |

## [05]-[RESEARCH]

- [NUMERIC_DETERMINISM] — the sign-exactness law-matrix the spec rail asserts. The harness is `PredicateLaws` (a CsCheck property suite under `testing-cs`): it generates near-degenerate input by perturbing collinear/cocircular/cospherical configurations at the `Epsilon` scale and asserts (1) every `Predicate` member returns the EXACT sign of the rational-arithmetic determinant computed independently in `System.Numerics.BigInteger` over scaled-integer coordinates — the exact oracle the kernel is proved against; (2) the antisymmetry law `Orient2D(a,b,c) == Orient2D(b,a,c).Flip` and its 3D/incircle/insphere analogues hold under coordinate permutation parity; (3) the filter path and the exact path agree on every input where the filter passes (the filter is conservative, never wrong); (4) the predicates are translation-invariant (a uniform `Point3d` offset never flips a verdict). The harness needs NO live-host probe — `BigInteger`, `Math.FusedMultiplyAdd`, and `Point3d` are stable; the kernel is total by construction. The one residual the harness watches is the FMA-availability assumption: `Math.FusedMultiplyAdd` lowers to a hardware FMA on osx-arm64; a future FMA-free RID routes `TwoProduct` through the named Dekker-split fallback (`NumericsPolicy.Splitter`, the `2^27+1` constant already seated) — confirming that fallback's bit-exactness against the FMA path on a non-FMA target is the only deferred numeric probe, and it gates a fallback row, never the FMA-path predicates.
- [INDIRECT_PREDICATES] — `OrientLPI`/`OrientTPI`/`InCircleLPI`/`InSphereTPI` extend the floor from explicit-coordinate predicates to the implicit-point family (Attene/Cherchi indirect predicates): each evaluates an exact orientation or in-circum sign against a CONSTRUCTED intersection point (a line-line `Lpi` or three-plane `Tpi` crossing) whose coordinates are NEVER materialized as a rounded `double` — `Lpi.Of`/`Tpi.Of` carry the intersection point's homogeneous numerator and denominator as `Expansion`s (the rational point `(Numerator.X/Denominator, Numerator.Y/Denominator)`), and the determinant is evaluated by `Expansion.Multiply`/`Scale` over those homogeneous coordinates so the sign is exact without a division or a rounding. The orientation pair lifts the homogeneous point into the `2×2`/`3×3` orientation determinant; the in-circum pair (`InCircleHomogeneous`/`InSphereHomogeneous`) lifts it into the in-circle/in-sphere LIFTED determinant — the lifted column for the implicit point is its squared homogeneous distance `(Numerator·lambda^{-1})·(Numerator·lambda^{-1})`, so every explicit-vertex lift term is scaled by `lambda^2` to clear the denominator without a division, and the final verdict is flipped when `lambda < 0` so the sign tracks the un-normalized homogeneous point. The four members ride the SAME `Expansion` fold and `Sign` verdict the direct predicates use, but they carry NO `double` filter row — a filter would have to materialize the rounded constructed coordinate the homogeneous form exists to avoid, so the implicit-point members are exact-only by construction. This is the realized `INDIRECT_PREDICATES` idea (orientation + in-circum): it unlocks the exact-arithmetic constructed-point pipelines the `Processing/repair#HEALING` `SelfIntersectResolve` split, the `Meshing/delaunay#CONSTRAINT_RECOVERY` Steiner-point flip, and the `Meshing/arrangement#ARRANGEMENT` cell-classification in-circum test compose — the self-intersect crossing point, the Delaunay Steiner vertex, and the arrangement implicit-point cell vertex feed the next predicate exactly without rounding, exactly where a rounded materialized coordinate loses the robustness guarantee. The law-matrix extends to assert exact-sign agreement of all four implicit-point members against the `BigInteger` rational oracle over the constructed point, no host probe.
