# [RASM_FABRICATION_WORKHOLDING]

The workholding owner closes fixture safety over real keep-out geometry. It admits fixture footprints from `HoldingClass`, expands them through `PolygonAlgebra.Offset`, rejects current `StockSnapshot` machined-face occupation, and conditions CAM motion by exact segment-vs-polygon predicates before guard consumes the same `ExclusionZone` shape.

## [01]-[INDEX]

- [01]-[WORKHOLDING]: owns fixture kind dispatch, footprint geometry rows, current-stock admission, loop-ordered motion conditioning, and guard-facing exclusion zones.

## [02]-[WORKHOLDING]

- Owner: `Workholding` owns the only fixture keep-out rail; `WorkholdingKind` owns the total `HoldingClass` mapping and the per-kind footprint functions; `Fixture` owns operation scope, per-loop motion runs, and optional `StockSnapshot` admission.
- Cases: `clamp` one inflated clamp loop; `vise` two rectangular jaw loops; `chuck` three or more revolved jaw loops; `vacuum-table` the full bed loop; `magnet` one magnetic pad loop; `sacrificial-bed` an empty keep-out row.
- Entry: `public static Fin<Seq<Move>> Condition(Seq<Move> moves, Fixture fixture)`; `public static ExclusionZone Zone(Clamp clamp)`; `public static bool Clears(Edge3 segment, Fixture fixture)`.
- Auto: `ForHolding` returns every legal row for a `HoldingClass`, including both `Clamp` and `Vise` for mechanical holding; `Fixture.Ordered` composes `PolygonAlgebra.NestingOrder(Arr<Loop> loops)` with loop run windows before conditioning.
- Receipt: `Collision(ExclusionZone)` emits from motion crossing; `ClampOnMachinedFace(int, Point3d)` emits from snapshot admission carrying the ACTUAL keepout-overlap witness vertex; setup feasibility stays outside this owner. Safety posture is FAIL-CLOSED end to end: admission proves every clamp inflation on the `Fin` rail (an offset failure rejects the fixture, never downgrades the margin), a snapshot clip failure propagates as the kernel fault (never reads as no-hit), and the `Zone` projection's only fallback OVER-blocks via margin-expanded dilation.
- Packages: LanguageExt `Fin<T>`/`Seq<T>`/`Option<T>` for rails and folds; Thinktecture `[SmartEnum<string>]` for behavior rows; RhinoCommon `Point3d`/`BoundingBox`/`Vector3d` for geometry carriers.
- Growth: a new holding mode is a `WorkholdingKind` row with one footprint function and one `HoldingClass` switch arm; a new fixture policy is a `Clamp` field consumed by existing geometry rows.
- Boundary: scalar keep-out tuning, probe-only clearance, and downstream ordering all collapse into fixture-owned loops, exact segment predicates, and CAM-side conditioning.

```csharp signature
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;

using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

[SmartEnum<string>]
public sealed partial class WorkholdingKind {
    public static readonly WorkholdingKind Clamp = new("clamp", HoldingClass.Mechanical, ClampFootprint);
    public static readonly WorkholdingKind Vise = new("vise", HoldingClass.Mechanical, ViseFootprint);
    public static readonly WorkholdingKind Chuck = new("chuck", HoldingClass.Revolved, ChuckFootprint);
    public static readonly WorkholdingKind VacuumTable = new("vacuum-table", HoldingClass.Vacuum, VacuumFootprint);
    public static readonly WorkholdingKind Magnet = new("magnet", HoldingClass.Magnetic, MagnetFootprint);
    public static readonly WorkholdingKind SacrificialBed = new("sacrificial-bed", HoldingClass.Bed, BedFootprint);

    public HoldingClass Holding { get; }

    [UseDelegateFromConstructor]
    public partial Seq<Loop> Footprints(Clamp clamp);

    public static Arr<WorkholdingKind> ForHolding(HoldingClass holding) =>
        holding.Switch(
            mechanical: static _ => Arr(Clamp, Vise),
            revolved: static _ => Arr(Chuck),
            vacuum: static _ => Arr(VacuumTable),
            magnetic: static _ => Arr(Magnet),
            bed: static _ => Arr(SacrificialBed));

    static Seq<Loop> ClampFootprint(Clamp clamp) =>
        Seq(clamp.Footprint.AsCcw());

    static Seq<Loop> ViseFootprint(Clamp clamp) {
        BoundingBox box = clamp.Footprint.Bound();
        double depth = Math.Max(clamp.JawDepth, clamp.Margin);

        return Seq(
            Box(box.Min.X, box.Min.Y - depth, box.Max.X, box.Min.Y),
            Box(box.Min.X, box.Max.Y, box.Max.X, box.Max.Y + depth));
    }

    static Seq<Loop> ChuckFootprint(Clamp clamp) {
        Point3d center = clamp.Center.IfNone(clamp.Footprint.Bound().Center);
        double radius = Math.Max(clamp.ChuckRadius, clamp.Footprint.Bound().Diagonal.Length * 0.5);
        int jaws = Math.Max(3, clamp.JawCount);

        return Range(0, jaws).Map(i => Jaw(center, radius, clamp.JawDepth, i * Math.Tau / jaws));
    }

    static Seq<Loop> VacuumFootprint(Clamp clamp) =>
        Seq(clamp.Bed.IfNone(clamp.Footprint).AsCcw());

    static Seq<Loop> MagnetFootprint(Clamp clamp) =>
        Seq(clamp.Footprint.AsCcw());

    static Seq<Loop> BedFootprint(Clamp _) =>
        Seq<Loop>();

    static Loop Box(double minX, double minY, double maxX, double maxY) =>
        new(
            Arr(
                new Point3d(minX, minY, 0.0),
                new Point3d(maxX, minY, 0.0),
                new Point3d(maxX, maxY, 0.0),
                new Point3d(minX, maxY, 0.0)),
            Closed: true);

    static Loop Jaw(Point3d center, double radius, double depth, double angle) {
        Vector3d radial = new(Math.Cos(angle), Math.Sin(angle), 0.0);
        Vector3d tangent = new(-radial.Y, radial.X, 0.0);
        Point3d inner = center + radial * Math.Max(0.0, radius - depth);
        Point3d outer = center + radial * radius;
        double halfWidth = Math.Max(depth * 0.5, 1.0);

        return new(
            Arr(
                inner - tangent * halfWidth,
                outer - tangent * halfWidth,
                outer + tangent * halfWidth,
                inner + tangent * halfWidth),
            Closed: true);
    }
}

public sealed record Clamp(
    Loop Footprint,
    WorkholdingKind Kind,
    double Margin,
    double Height,
    Option<Loop> Bed = default,
    Option<Point3d> Center = default,
    double JawDepth = 8.0,
    double ChuckRadius = 0.0,
    int JawCount = 3);

public sealed record ExclusionZone(int Operation, WorkholdingKind Kind, Seq<Loop> Keepouts, double Height) {
    public bool Covers(Point3d point) =>
        Keepouts.Exists(loop => loop.Covers(point));

    public bool Crosses(Edge3 segment) =>
        Keepouts.Exists(loop => Workholding.SegmentCrosses(segment, loop));
}

public readonly record struct MoveRun(int Loop, int Start, int Count);

public sealed record Fixture(
    int Operation,
    Seq<Clamp> Clamps,
    Arr<Loop> Profiles,
    Seq<MoveRun> Runs,
    Option<StockSnapshot> Current = default) {
    // The clamp-free floor: Condition over Free is identity and Zones is empty — the context-free post posture.
    public static readonly Fixture Free = new(Operation: 0, Seq<Clamp>(), Arr<Loop>(), Seq<MoveRun>());

    public Seq<ExclusionZone> Zones =>
        Clamps.Map(clamp => Workholding.Zone(clamp) with { Operation = Operation });

    public Seq<Move> Ordered(Seq<Move> moves) =>
        Runs.IsEmpty
            ? moves
            : PolygonAlgebra.NestingOrder(Profiles)
                .Bind(index => Runs.Filter(run => run.Loop == index))
                .Bind(run => moves.Skip(run.Start).Take(run.Count));
}

public static class Workholding {
    public static ExclusionZone Zone(Clamp clamp) =>
        new(
            Operation: 0,
            Kind: clamp.Kind,
            Keepouts: clamp.Kind.Footprints(clamp).Bind(shape =>
                PolygonAlgebra.Offset(Seq(shape.AsCcw()), Math.Abs(clamp.Margin), OffsetEnds.Polygon)
                    .IfFail(Dilate(shape.AsCcw(), Math.Abs(clamp.Margin)))),
            Height: clamp.Height);

    // Margin-preserving FAIL-CLOSED fallback: when the exact inflation cannot run the keepout dilates to the
    // margin-expanded bounding quad — it OVER-blocks, never under-blocks, so the commanded clamp margin survives
    // every failure path; Admit re-proves the exact inflation on the Fin rail before any conditioning.
    static Seq<Loop> Dilate(Loop shape, double margin) {
        BoundingBox bound = shape.Bound();
        return Seq1(new Loop(Arr(
            new Point3d(bound.Min.X - margin, bound.Min.Y - margin, 0.0),
            new Point3d(bound.Max.X + margin, bound.Min.Y - margin, 0.0),
            new Point3d(bound.Max.X + margin, bound.Max.Y + margin, 0.0),
            new Point3d(bound.Min.X - margin, bound.Max.Y + margin, 0.0)), Closed: true));
    }

    public static bool Clears(Edge3 segment, Fixture fixture) =>
        Blocks(segment, fixture).IsNone;

    public static Fin<Seq<Move>> Condition(Seq<Move> moves, Fixture fixture) =>
        Admit(fixture).Bind(admitted =>
            admitted.Ordered(moves)
                .Fold(
                    Fin.Succ((Cursor: Point3d.Origin, Accepted: Seq<Move>())),
                    (state, move) => state.Bind(cursor => Step(admitted, cursor, move)))
                .Map(state => state.Accepted));

    public static bool SegmentCrosses(Edge3 segment, Loop loop) =>
        loop.Covers(segment.A)
        || loop.Covers(segment.B)
        || Range(0, loop.Vertices.Count).Exists(index =>
            SegmentsIntersect(
                segment,
                new Edge3(loop.At(index), loop.At(index + 1))));

    // Admission proves the exact clamp inflation FIRST (an offset failure rejects the fixture on the rail, never a
    // downgraded keepout), then gates the snapshot: a clip failure PROPAGATES as the kernel fault — an unknown
    // fixture-stock overlap is never admitted as clear.
    static Fin<Fixture> Admit(Fixture fixture) =>
        fixture.Clamps
            .Fold(Fin.Succ(fixture), (acc, clamp) => acc.Bind(f => Inflate(clamp).Map(_ => f)))
            .Bind(static f => f.Current.Match(
                Some: snapshot => SnapshotHit(f, snapshot).Bind(hit => hit.Match(
                    Some: point => Fin.Fail<Fixture>(
                        FabricationFault.ClampOnMachinedFace(f.Operation, point).ToError()),
                    None: () => Fin.Succ(f))),
                None: () => Fin.Succ(f)));

    static Fin<Seq<Loop>> Inflate(Clamp clamp) =>
        clamp.Kind.Footprints(clamp).Fold(Fin.Succ(Seq<Loop>()), (acc, shape) => acc.Bind(seq =>
            PolygonAlgebra.Offset(Seq(shape.AsCcw()), Math.Abs(clamp.Margin), OffsetEnds.Polygon).Map(inflated => seq + inflated)));

    // The hit WITNESS is the actual keepout∩machined-face overlap vertex — never a keepout corner; the Fin rail
    // carries a clip failure out of admission instead of collapsing it to no-hit.
    static Fin<Option<Point3d>> SnapshotHit(Fixture fixture, StockSnapshot snapshot) =>
        fixture.Zones
            .Bind(zone => zone.Keepouts)
            .Bind(keepout => snapshot.Machined.Map(face => (Keepout: keepout, Face: face)))
            .Fold(Fin.Succ(Option<Point3d>.None), (acc, row) => acc.Bind(hit => hit.IsSome
                ? Fin.Succ(hit)
                : PolygonAlgebra.Clip(Seq(row.Keepout), Seq(row.Face), ClipOp.Intersect)
                    .Map(overlap => overlap.HeadOrNone().Filter(static loop => loop.Count > 0).Map(static loop => loop.At(0)))));

    static Fin<(Point3d Cursor, Seq<Move> Accepted)> Step(
        Fixture fixture,
        (Point3d Cursor, Seq<Move> Accepted) state,
        Move move) =>
        move.Rapid
            ? Fin.Succ((move.To, state.Accepted.Add(move)))
            : Blocks(new Edge3(state.Cursor, move.To), fixture).Match(
                Some: zone => Fin.Fail<(Point3d Cursor, Seq<Move> Accepted)>(
                    FabricationFault.Collision(zone).ToError()),
                None: () => Fin.Succ((move.To, state.Accepted.Add(move))));

    static Option<ExclusionZone> Blocks(Edge3 segment, Fixture fixture) =>
        fixture.Zones.Find(zone => zone.Crosses(segment));

    static bool SegmentsIntersect(Edge3 a, Edge3 b) {
        Sign aToB0 = Predicate.Orient2D(a.A, a.B, b.A);
        Sign aToB1 = Predicate.Orient2D(a.A, a.B, b.B);
        Sign bToA0 = Predicate.Orient2D(b.A, b.B, a.A);
        Sign bToA1 = Predicate.Orient2D(b.A, b.B, a.B);

        return aToB0.Times(aToB1) == Sign.Negative
            && bToA0.Times(bToA1) == Sign.Negative
            || (aToB0 == Sign.Zero && OnSegment(b.A, a))
            || (aToB1 == Sign.Zero && OnSegment(b.B, a))
            || (bToA0 == Sign.Zero && OnSegment(a.A, b))
            || (bToA1 == Sign.Zero && OnSegment(a.B, b));
    }

    static bool OnSegment(Point3d point, Edge3 segment) =>
        point.X >= Math.Min(segment.A.X, segment.B.X)
        && point.X <= Math.Max(segment.A.X, segment.B.X)
        && point.Y >= Math.Min(segment.A.Y, segment.B.Y)
        && point.Y <= Math.Max(segment.A.Y, segment.B.Y);
}
```
