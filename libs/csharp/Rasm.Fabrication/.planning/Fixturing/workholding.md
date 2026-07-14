# [RASM_FABRICATION_WORKHOLDING]

The ONE fixture keep-out and holding-adequacy substrate for the `Fixturing` folder: `Clamp` the closed `[Union]` of per-kind fixture instances (each case carrying exactly its own geometry and capacity columns — no nullable payload bag), `WorkholdingKind` the `[SmartEnum<string>]` behavior vocabulary binding each kind to its `HoldingClass` row, `ExclusionZone` the height-aware keep-out carrier every consumer reads, `Fixture` the per-operation holding context, and the `Workholding` static surface owning motion conditioning (`Condition`), the exact segment-vs-polygon clearance predicates (`Clears`/`Zone`/`SegmentCrosses`), the published machined-face admission (`MachinedHit` — the one clamp-on-machined-face verdict `Fixturing/setups` composes instead of re-deriving), and the holding-adequacy verdict (`Restrains` — slip and tip margins against the cutting load, the force half of the workholding domain the keep-out half alone never answers). Footprint expansion routes the one `Geometry2D/algebra#POLYGON_ALGEBRA` `Offset` owner; the exact side tests read the kernel `Rasm/Numerics/predicates#ROBUST_PREDICATES` `Predicate.Orient2D` sign through `Sign.Times`; the cutting-force demand the adequacy verdict consumes is the `Tooling/cuttingdata#CUTTING_DATA` `CuttingData.Force(b, h)` Kienzle law crossing as a raw `CutLoad` scalar — the force LAW stays Tooling-owned, this page owns only the restraint balance over it.

Safety posture is FAIL-CLOSED end to end: admission proves every clamp inflation on the accumulating `Validation` rail (an offset failure rejects the fixture with every failing clamp named, never a downgraded margin), a machined-face clip failure propagates as the kernel fault (never reads as no-hit), the `Zone` projection's only fallback OVER-blocks via margin-expanded dilation, and the keep-out is height-aware fail-closed — a segment whose Z window dips below the zone `Height` is tested at its full planar extent (over-blocks the partial crossing), while a segment riding entirely at or above `Height` clears, so a retract plane above every clamp is expressible and a rapid dragged through a clamp at cutting height is caught. Every move conditions: the old rapid bypass is deleted — rapid versus feed changes the feed word, never the collision physics.

Wire posture: HOST-LOCAL. `ExclusionZone`/`Fixture` cross only in-process seams — `Toolpath/motion` and `Posting/program` compose `Condition`, `Toolpath/guard` constructs zones and concats `Fixture.Zones` into its obstacle set, `Fixturing/setups` composes `Clears`/`MachinedHit`, `Fixturing/assembly` composes `Clears` — no type on this page sits between wire and rail.

## [01]-[INDEX]

- [01]-[WORKHOLDING]: owns the `Clamp` per-kind instance union with its footprint and capacity folds, the `WorkholdingKind` vocabulary and `HoldingClass` mapping, the height-aware `ExclusionZone`, the `Fixture`/`MoveRun` context, and the `Workholding` surface — conditioning, clearance, machined-face admission, and the holding-adequacy receipt.

## [02]-[WORKHOLDING]

- Owner: `Clamp` `[Union]` the fixture instance — `Pad` (one inflated clamp loop + clamp force) · `Jaws` (two rectangular vise-jaw loops derived from the gripped footprint + jaw depth) · `Chuck` (N revolved jaw loops about a center + radial grip force) · `Vacuum` (the full bed loop + pad vacuum) · `Magnet` (the magnetic pad loop + holding pressure) · `Bed` (sacrificial bed: empty keep-out, adhesion pressure over the part contact area) — each case carrying exactly its own geometry and capacity columns, the common `Kind`/`Margin`/`Height`/`Footprints`/`CapacityN` projections one total `Switch` each; `WorkholdingKind` `[SmartEnum<string>]` the kind vocabulary carrying the `HoldingClass` column and the `ForHolding` reverse map (`mechanical` admits `Clamp` and `Vise` rows, `revolved` the chuck, `vacuum`/`magnetic`/`bed` their one row each); `ExclusionZone` the keep-out carrier (operation scope, kind, inflated keep-out loops, `Height`) with height-aware `Covers`/`Crosses`; `MoveRun` the per-loop motion window; `Fixture` the per-operation context (clamps, profiles, loop windows, optional current `StockSnapshot`) with the `Free` clamp-free floor; `HoldingPhysics` the restraint policy row (`FrictionMu`, `SafetyFactor`); `CutLoad` the demand row (force, application point, part contact area); `HoldingReceipt` the typed adequacy evidence; `Workholding` the static surface.
- Cases: `Clamp` cases 6, each footprint arm one `Switch` row — `Pad`/`Magnet` their own loop, `Jaws` two jaw rectangles beyond the gripped footprint's Y extents, `Chuck` `JawCount ≥ 3` trapezoid jaws on the radius, `Vacuum` its bed loop, `Bed` the empty row; capacity arms — mechanical cases `ClampForceN·μ` friction restraint, `Chuck` `GripForceN·μ`, `Vacuum`/`Magnet` `kPa · loopAreaMm2/1000 · μ`, `Bed` `HoldKpa · load.PartContactAreaMm2/1000` (adhesion needs no friction product).
- Entry: `public static Fin<Seq<Move>> Condition(Seq<Move> moves, Fixture fixture)` — EVERY move conditions against the height-aware zones (a blocked segment routes `FabricationFault.Collision(zone)` 2707); `public static Fin<ExclusionZone> Zone(Clamp clamp)`; `public static bool Clears(Edge3 segment, Fixture fixture)`; `public static Fin<Option<Point3d>> MachinedHit(Fixture fixture, StockSnapshot snapshot)` — the published clamp-on-machined-face witness (`ClipOp.Intersect` overlap vertex, never a keep-out corner) `setups` composes; `public static HoldingReceipt Restrains(Fixture fixture, CutLoad load, HoldingPhysics physics)` — the slip/tip adequacy verdict, receipt-only (the consumer routes its own fault off a failing margin).
- Auto: `ForHolding` returns every legal kind row for a `HoldingClass`; `Fixture.Ordered` composes `PolygonAlgebra.NestingOrder(Arr<Loop>)` with the loop run windows before conditioning; `Admit` traverses every clamp's exact inflation on the accumulating `Validation` rail (all failing clamps report together) then gates the snapshot through `MachinedHit`, routing `ClampOnMachinedFace(operation, witness)` 2727 with the ACTUAL overlap vertex; `Restrains` folds per-clamp capacity through the `CapacityN` arm — the slip margin is `Σcapacity / (SafetyFactor·ForceN)`, the tip margin the clamp-moment sum `Σ(capacityᵢ · ‖centroidᵢ − loadXY‖)` against the overturning `ForceN · load.At.Z` — and the cutting-force demand arrives pre-computed from the committed operation's `CuttingData.Force(b, h)` at the caller.
- Receipt: `Collision(ExclusionZone)` emits from motion crossing; `ClampOnMachinedFace(int, Point3d)` emits from snapshot admission carrying the overlap witness vertex; `HoldingReceipt(double CapacityN, double DemandN, double SlipMargin, double TipMargin, bool Holds)` is the adequacy evidence — setup feasibility stays `setups`', which reads these verdicts and routes its own faults.
- Packages: LanguageExt (`Fin`/`Validation`/`Seq`/`Option` — the accumulating admission rail); Thinktecture (`[Union]` instance family, `[SmartEnum<string>]` vocabulary rows); `Geometry2D/algebra#POLYGON_ALGEBRA` (`Offset`/`Clip`/`NestingOrder`/`Area`); `Rasm/Numerics/predicates#ROBUST_PREDICATES` (`Predicate.Orient2D`/`Sign`); `Process/owner#FABRICATION_OWNER` (`Loop`/`Edge3`/`Move`/`StockSnapshot`); `Process/family#PROCESS_FAMILY` (`HoldingClass`); `Process/faults#FAULT_BAND` (2707/2727); RhinoCommon (`Point3d`/`Vector3d`/`BoundingBox`).
- Growth: a new holding mode is one `Clamp` case (its footprint and capacity arms) plus one `WorkholdingKind` row and one `ForHolding` arm; a new restraint law (deflection-under-cut, contact-pressure limit) is one `HoldingReceipt` field plus one fold term in `Restrains`; a new keep-out dimension policy is an `ExclusionZone` column its predicates read; zero new entrypoints.
- Boundary: this page is the ONE keep-out and adequacy owner — a setups-local clamp-hit test, an assembly-local reach test, or a second fixture solver anywhere is the deleted form (`MachinedHit` and `Clears` are the published verdicts); the keep-out is height-aware and a planar predicate ignoring the carried `Height` is the named Z-blind defect — `Toolpath/guard`'s `Height: double.MaxValue` zones block at every Z by the same predicate, no special case; the rapid bypass is deleted — every emitted segment carries the same clearance evidence; the per-kind payload lives on the union case and a flat clamp record carrying chuck-only, vise-only, and vacuum-only columns behind defaults is the deleted nullable-payload-bag form; the capacity columns live on the case and a parallel `HoldingForce` sibling table is the deleted form; the cutting-force LAW stays `Tooling/cuttingdata`'s (`CuttingData.Force`) and a local Kienzle re-derivation or a magic force constant here is the deleted form — `CutLoad` carries the resolved scalar; footprint inflation routes the one `PolygonAlgebra.Offset` owner and the only fallback is the margin-expanded `Dilate` over-block; the machined-face witness is the clip-overlap vertex and a face-corner or keep-out-corner witness is the named weaker-witness defect.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Fixturing;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// The kind VOCABULARY: HoldingClass mapping + reverse map. Per-instance geometry lives on the Clamp union case,
// never as delegate columns here — one discriminant, two faces (row = behavior, case = payload).
[SmartEnum<string>]
public sealed partial class WorkholdingKind {
    public static readonly WorkholdingKind Clamp = new("clamp", HoldingClass.Mechanical);
    public static readonly WorkholdingKind Vise = new("vise", HoldingClass.Mechanical);
    public static readonly WorkholdingKind Chuck = new("chuck", HoldingClass.Revolved);
    public static readonly WorkholdingKind VacuumTable = new("vacuum-table", HoldingClass.Vacuum);
    public static readonly WorkholdingKind Magnet = new("magnet", HoldingClass.Magnetic);
    public static readonly WorkholdingKind SacrificialBed = new("sacrificial-bed", HoldingClass.Bed);

    public HoldingClass Holding { get; }

    public static Arr<WorkholdingKind> ForHolding(HoldingClass holding) =>
        holding.Switch(
            mechanical: static _ => Arr(Clamp, Vise),
            revolved: static _ => Arr(Chuck),
            vacuum: static _ => Arr(VacuumTable),
            magnetic: static _ => Arr(Magnet),
            bed: static _ => Arr(SacrificialBed));
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
// Per-kind instance family: each case carries exactly its own geometry + capacity columns. The old flat record
// with Option<Loop> Bed, Option<Point3d> Center, and chuck-only defaults was the nullable-payload-bag defect.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Clamp {
    private Clamp() { }

    public sealed record Pad(Loop Footprint, double Margin, double Height, double ClampForceN) : Clamp;
    public sealed record Jaws(Loop Footprint, double JawDepth, double Margin, double Height, double ClampForceN) : Clamp;
    public sealed record Chuck(Point3d Center, double Radius, double JawDepth, int JawCount, double Margin, double Height, double GripForceN) : Clamp;
    public sealed record Vacuum(Loop Bed, double Margin, double Height, double PadKpa) : Clamp;
    public sealed record Magnet(Loop Pad, double Margin, double Height, double HoldKpa) : Clamp;
    public sealed record Bed(double Height, double HoldKpa) : Clamp;

    public WorkholdingKind Kind =>
        Switch(
            pad: static _ => WorkholdingKind.Clamp,
            jaws: static _ => WorkholdingKind.Vise,
            chuck: static _ => WorkholdingKind.Chuck,
            vacuum: static _ => WorkholdingKind.VacuumTable,
            magnet: static _ => WorkholdingKind.Magnet,
            bed: static _ => WorkholdingKind.SacrificialBed);

    public double MarginOf =>
        Switch(
            pad: static p => p.Margin, jaws: static j => j.Margin, chuck: static c => c.Margin,
            vacuum: static v => v.Margin, magnet: static m => m.Margin, bed: static _ => 0.0);

    public double HeightOf =>
        Switch(
            pad: static p => p.Height, jaws: static j => j.Height, chuck: static c => c.Height,
            vacuum: static v => v.Height, magnet: static m => m.Height, bed: static b => b.Height);

    public Seq<Loop> Footprints() =>
        Switch(
            pad: static p => Seq(p.Footprint.AsCcw()),
            jaws: static j => JawPair(j),
            chuck: static c => toSeq(Enumerable.Range(0, Math.Max(3, c.JawCount)))
                .Map(i => Jaw(c.Center, Math.Max(c.Radius, 1.0), c.JawDepth, i * Math.Tau / Math.Max(3, c.JawCount))),
            vacuum: static v => Seq(v.Bed.AsCcw()),
            magnet: static m => Seq(m.Pad.AsCcw()),
            bed: static _ => Seq<Loop>());

    // Restraint capacity in newtons: friction restraint for the force-clamped cases, pressure × pad area for the
    // suction cases, adhesion × part contact area for the sacrificial bed. kPa × mm² / 1000 = N.
    public double CapacityN(HoldingPhysics physics, CutLoad load) =>
        Switch(
            state: (physics, load),
            pad: static (s, p) => p.ClampForceN * s.physics.FrictionMu,
            jaws: static (s, j) => j.ClampForceN * s.physics.FrictionMu,
            chuck: static (s, c) => c.GripForceN * s.physics.FrictionMu,
            vacuum: static (s, v) => v.PadKpa * Math.Abs(PolygonAlgebra.Area(v.Bed)) / 1000.0 * s.physics.FrictionMu,
            magnet: static (s, m) => m.HoldKpa * Math.Abs(PolygonAlgebra.Area(m.Pad)) / 1000.0 * s.physics.FrictionMu,
            bed: static (s, b) => b.HoldKpa * s.load.PartContactAreaMm2 / 1000.0);

    static Seq<Loop> JawPair(Jaws j) {
        BoundingBox box = j.Footprint.Bound();
        double depth = Math.Max(j.JawDepth, j.Margin);
        return Seq(
            Box(box.Min.X, box.Min.Y - depth, box.Max.X, box.Min.Y),
            Box(box.Min.X, box.Max.Y, box.Max.X, box.Max.Y + depth));
    }

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
        Point3d inner = center + (radial * Math.Max(0.0, radius - depth));
        Point3d outer = center + (radial * radius);
        double halfWidth = Math.Max(depth * 0.5, 1.0);
        return new(
            Arr(
                inner - (tangent * halfWidth),
                outer - (tangent * halfWidth),
                outer + (tangent * halfWidth),
                inner + (tangent * halfWidth)),
            Closed: true);
    }
}

// Height-aware keep-out: a point below Height inside a keep-out loop is covered; a segment riding entirely at or
// above Height clears; a segment whose Z window dips below Height tests at its full planar extent (fail-closed on
// the partial crossing). Guard's Height: double.MaxValue zones block at every Z through the same predicate.
public sealed record ExclusionZone(int Operation, WorkholdingKind Kind, Seq<Loop> Keepouts, double Height) {
    public bool Covers(Point3d point) =>
        point.Z < Height && Keepouts.Exists(loop => loop.Covers(point));

    public bool Crosses(Edge3 segment) =>
        Math.Min(segment.A.Z, segment.B.Z) < Height
        && Keepouts.Exists(loop => Workholding.SegmentCrosses(segment, loop));
}

public readonly record struct MoveRun(int Loop, int Start, int Count);

// Restraint policy + demand rows: FrictionMu the part-fixture pair's coefficient, SafetyFactor the shop margin;
// CutLoad.ForceN arrives from the committed operation's CuttingData.Force(b, h) — the Kienzle law stays Tooling's.
public sealed record HoldingPhysics(double FrictionMu, double SafetyFactor) {
    public static readonly HoldingPhysics SteelOnSteel = new(FrictionMu: 0.15, SafetyFactor: 2.0);
    public static readonly HoldingPhysics AluminiumOnSteel = new(FrictionMu: 0.3, SafetyFactor: 2.0);
}

public readonly record struct CutLoad(double ForceN, Point3d At, double PartContactAreaMm2);

public readonly record struct HoldingReceipt(double CapacityN, double DemandN, double SlipMargin, double TipMargin) {
    public bool Holds => SlipMargin >= 1.0 && TipMargin >= 1.0;
}

public sealed record Fixture(
    int Operation,
    Seq<Clamp> Clamps,
    Arr<Loop> Profiles,
    Seq<MoveRun> Runs,
    Option<StockSnapshot> Current = default) {
    // The clamp-free floor: Condition over Free is identity and Zones is empty — the context-free post posture.
    public static readonly Fixture Free = new(Operation: 0, Seq<Clamp>(), Arr<Loop>(), Seq<MoveRun>());

    public Seq<ExclusionZone> Zones =>
        Clamps.Map(clamp => Workholding.Zone(clamp).IfFail(Workholding.Dilated(clamp)) with { Operation = Operation });

    public Seq<Move> Ordered(Seq<Move> moves) =>
        Runs.IsEmpty
            ? moves
            : PolygonAlgebra.NestingOrder(Profiles)
                .Bind(index => Runs.Filter(run => run.Loop == index))
                .Bind(run => moves.Skip(run.Start).Take(run.Count));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Workholding {
    public static Fin<ExclusionZone> Zone(Clamp clamp) =>
        clamp.Footprints()
            .Traverse(shape => PolygonAlgebra.Offset(Seq(shape.AsCcw()), Math.Abs(clamp.MarginOf), OffsetEnds.Polygon).ToValidation())
            .As()
            .ToFin()
            .Map(inflated => new ExclusionZone(Operation: 0, clamp.Kind, inflated.Bind(static loops => loops), clamp.HeightOf));

    // Margin-preserving FAIL-CLOSED fallback: when the exact inflation cannot run, the keep-out dilates to the
    // margin-expanded bounding quad per footprint — it OVER-blocks, never under-blocks, so the commanded clamp
    // margin survives every failure path; Condition's Admit re-proves the exact inflation on the rail first.
    public static ExclusionZone Dilated(Clamp clamp) =>
        new(Operation: 0, clamp.Kind, clamp.Footprints().Map(shape => Quad(shape.Bound(), Math.Abs(clamp.MarginOf))), clamp.HeightOf);

    public static bool Clears(Edge3 segment, Fixture fixture) =>
        Blocks(segment, fixture).IsNone;

    // EVERY move conditions — rapid and feed alike ride the height-aware predicate; a retract plane above every
    // clamp Height clears structurally, a rapid dragged through a clamp at cutting height routes Collision 2707.
    public static Fin<Seq<Move>> Condition(Seq<Move> moves, Fixture fixture) =>
        Admit(fixture).Bind(admitted =>
            admitted.Ordered(moves)
                .Fold(
                    Fin.Succ((Cursor: Point3d.Origin, Accepted: Seq<Move>())),
                    (state, move) => state.Bind(cursor => Step(admitted, cursor, move)))
                .Map(state => state.Accepted));

    // The published clamp-on-machined-face verdict: the hit WITNESS is the actual keepout∩machined-face overlap
    // vertex — never a keep-out or face corner — and a clip failure rides the rail out, never reads as no-hit.
    public static Fin<Option<Point3d>> MachinedHit(Fixture fixture, StockSnapshot snapshot) =>
        fixture.Zones
            .Bind(zone => zone.Keepouts)
            .Bind(keepout => snapshot.Machined.Map(face => (Keepout: keepout, Face: face)))
            .Fold(Fin.Succ(Option<Point3d>.None), (acc, row) => acc.Bind(hit => hit.IsSome
                ? Fin.Succ(hit)
                : PolygonAlgebra.Clip(Seq(row.Keepout), Seq(row.Face), ClipOp.Intersect)
                    .Map(overlap => overlap.HeadOrNone().Filter(static loop => loop.Count > 0).Map(static loop => loop.At(0)))));

    // Slip: Σ per-clamp friction capacity against SafetyFactor·ForceN. Tip: the restraining moment sum about the
    // load's XY point against the overturning moment ForceN·At.Z. Receipt-only — the consumer routes its own fault.
    public static HoldingReceipt Restrains(Fixture fixture, CutLoad load, HoldingPhysics physics) {
        double capacity = fixture.Clamps.Sum(clamp => clamp.CapacityN(physics, load));
        double demand = physics.SafetyFactor * load.ForceN;
        double restoring = fixture.Clamps.Sum(clamp =>
            clamp.CapacityN(physics, load) * clamp.Footprints().HeadOrNone()
                .Map(loop => Planar(loop.Bound().Center).DistanceTo(Planar(load.At)))
                .IfNone(0.0));
        double overturning = physics.SafetyFactor * load.ForceN * Math.Max(0.0, load.At.Z);
        return new HoldingReceipt(
            CapacityN: capacity,
            DemandN: demand,
            SlipMargin: demand > 0.0 ? capacity / demand : double.PositiveInfinity,
            TipMargin: overturning > 0.0 ? restoring / overturning : double.PositiveInfinity);
    }

    public static bool SegmentCrosses(Edge3 segment, Loop loop) =>
        loop.Covers(segment.A)
        || loop.Covers(segment.B)
        || Range(0, loop.Vertices.Count).Exists(index =>
            SegmentsIntersect(
                segment,
                new Edge3(loop.At(index), loop.At(index + 1))));

    // Admission proves EVERY clamp's exact inflation on the accumulating Validation rail (all failing clamps
    // report together, never first-fault), then gates the snapshot through the published MachinedHit verdict.
    static Fin<Fixture> Admit(Fixture fixture) =>
        fixture.Clamps
            .Traverse(clamp => Zone(clamp).ToValidation())
            .As()
            .ToFin()
            .Bind(_ => fixture.Current.Match(
                Some: snapshot => MachinedHit(fixture, snapshot).Bind(hit => hit.Match(
                    Some: point => Fin.Fail<Fixture>(
                        FabricationFault.ClampOnMachinedFace(fixture.Operation, point).ToError()),
                    None: () => Fin.Succ(fixture))),
                None: () => Fin.Succ(fixture)));

    static Fin<(Point3d Cursor, Seq<Move> Accepted)> Step(
        Fixture fixture,
        (Point3d Cursor, Seq<Move> Accepted) state,
        Move move) =>
        Blocks(new Edge3(state.Cursor, move.To), fixture).Match(
            Some: zone => Fin.Fail<(Point3d Cursor, Seq<Move> Accepted)>(
                FabricationFault.Collision(zone).ToError()),
            None: () => Fin.Succ((move.To, state.Accepted.Add(move))));

    static Option<ExclusionZone> Blocks(Edge3 segment, Fixture fixture) =>
        fixture.Zones.Find(zone => zone.Crosses(segment));

    static Loop Quad(BoundingBox bound, double margin) =>
        new(
            Arr(
                new Point3d(bound.Min.X - margin, bound.Min.Y - margin, 0.0),
                new Point3d(bound.Max.X + margin, bound.Min.Y - margin, 0.0),
                new Point3d(bound.Max.X + margin, bound.Max.Y + margin, 0.0),
                new Point3d(bound.Min.X - margin, bound.Max.Y + margin, 0.0)),
            Closed: true);

    static Point3d Planar(Point3d p) => new(p.X, p.Y, 0.0);

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
