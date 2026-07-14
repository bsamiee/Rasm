# [RASM_FABRICATION_WORKHOLDING]

`Workholding` is the single fixture-admission, keep-out, conditioning, and restraint owner. `Clamp` closes pad, jaws, chuck, vacuum, magnet, and bed modalities with case-specific geometry, contact direction, friction, pull-off, pressure, and capacity values. `WorkholdingKind` maps those cases onto `HoldingClass`; no nullable payload record or parallel force table exists.

`Workholding.Of` is the sole `Fixture` admission boundary. The factory-only carrier accumulates clamp-zone failures, rejects overlapping motion windows, preserves the actual initial cursor, and gates optional stock snapshots through `MachinedHit`. `ExclusionZone` retains all inflated loops and evaluates even-odd coverage at its carried height. `Condition` subdivides arcs at `HoldingPhysics.ArcChordErrorMm` and collision-checks rapid and feed moves identically.

`Restrains` balances directional force and moment demand against per-contact reactions. Opposed vise jaws contribute opposite normals; friction contributes only along each contact tangent; vacuum contributes one pull-off reaction per sealed component of the leak-subtracted bed, its footprint and net area coupled; magnetic attraction contributes pull-off capacity; bed pressure acts over the carried contact region. `HoldingReceipt` exposes axis-specific and coupled directional margins instead of one scalar capacity ratio.

Wire posture: HOST-LOCAL. `Fixture`, `ExclusionZone`, `ToolCorridor`, and restraint receipts cross only in-process fabrication seams.

## [01]-[INDEX]

- [01]-[WORKHOLDING]: owns clamp modality, fixture admission, exact keep-outs, height-aware motion and tool-corridor clearance, machined-stock witnesses, and directional restraint evidence.

## [02]-[WORKHOLDING]

- Owner: `Clamp` owns modality-specific geometry and reaction capacity; `ExclusionZone` owns exact keep-out topology, its once-densified wall polygons, and height; `ToolCorridor` owns variable-radius access; `Fixture` owns admitted zones and motion context; `CutLoad` owns force, moment, application point, and contact region; `HoldingReceipt` owns directional equilibrium evidence.
- Cases: `Clamp` carries six closed cases. Each case projects exact footprints and `ContactReaction` rows; vacuum leak zones, magnetic coupling, jaw opening, throat, step, chuck radial/axial capacity, bed contact, and reaction normals remain live inputs.
- Entry: `Of(...) -> Fin<Fixture>` admits the fixture; `Condition(Seq<Move>, Fixture) -> Fin<Seq<Move>>` validates tool motion; `Zone(Clamp, double) -> Fin<ExclusionZone>` constructs exact keep-outs; both `Clears(ToolCorridor, Fixture)` and `Clears(Edge3, Fixture)` return `Fin<bool>`; `MachinedHit` and `Restrains` publish typed evidence.
- Auto: `Of` traverses `Zone` on the accumulating `Validation` rail, constructs one immutable fixture, and checks machined-stock overlap. `Clears(ToolCorridor, Fixture)` offsets complete keep-out regions by the maximum corridor radius. `Restrains` folds `ContactReaction` vectors and moments into per-axis demand, capacity, and margins.
- Receipt: `Collision(ExclusionZone)` identifies blocked motion, `ClampOnMachinedFace(int, Point3d)` carries the actual overlap witness, and `HoldingReceipt` carries translational and rotational `AxisMargin` rows plus the governing margin.
- Packages: `Rasm`, `RhinoCommon`, `Clipper2`, `Thinktecture.Runtime.Extensions`, and `LanguageExt.Core`.
- Growth: a new holding mode is one `Clamp` case (its footprint and capacity arms) plus one `WorkholdingKind` row and one `ForHolding` arm; a new restraint law (deflection-under-cut, contact-pressure limit) is one `HoldingReceipt` field plus one fold term in `Restrains`; a new keep-out dimension policy is an `ExclusionZone` column its predicates read; zero new entrypoints.
- Boundary: fixture inflation, corridor clearance, machined overlap, and restraint have one owner. No geometry error becomes a fallback zone or no-hit verdict. Height remains live in every segment test, rapid motion receives the same collision law as feed motion, directional reactions never collapse into one scalar holding force, and a vacuum reaction never proxies the unperforated bed for its sealed contact footprint.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
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

    public sealed record Pad(Loop Footprint, Vector3d ReactionNormal, double Margin, double Height, double ClampForceN) : Clamp;
    public sealed record Jaws(Loop Footprint, Vector3d ReactionNormal, double OpeningMinMm, double OpeningMaxMm, double StepHeightMm,
        double ThroatDepthMm, double JawDepth, double Margin, double Height, double ClampForceN) : Clamp;
    public sealed record Chuck(Point3d Center, Context Tolerance, double Radius, double JawDepth, double JawWidth, double JawHeight, int JawCount,
        double Margin, double Height, double RadialGripForceN, double AxialCapacityN) : Clamp;
    public sealed record Vacuum(Loop Bed, Seq<Loop> LeakZones, double Margin, double Height, double PadKpa) : Clamp;
    // Coupling is the flux-coupling derate in (0, 1] — air gap, stack thickness, and material response folded into
    // one admitted factor; a raw permeability number is not normalized and is the rejected column.
    public sealed record Magnet(Loop Pad, double Coupling, double Margin, double Height, double HoldKpa) : Clamp;
    public sealed record Bed(Loop Contact, Vector3d ReactionNormal, double Height, double HoldKpa) : Clamp;

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
            pad: static p => p.Height, jaws: static j => j.Height + j.StepHeightMm, chuck: static c => Math.Max(c.Height, c.JawHeight),
            vacuum: static v => v.Height, magnet: static m => m.Height, bed: static b => b.Height);

    public Fin<Seq<Loop>> Footprints() =>
        Switch(
            pad: static p => Fin.Succ(Seq(p.Footprint.AsCcw())),
            jaws: static j => JawPair(j),
            chuck: static c => toSeq(Enumerable.Range(0, c.JawCount))
                .Traverse(index => Jaw(c.Center, c.Radius, c.JawDepth, c.JawWidth, index * Math.Tau / c.JawCount, c.Tolerance).ToValidation())
                .As().ToFin(),
            vacuum: static v => Fin.Succ(Seq(v.Bed.AsCcw())),
            magnet: static m => Fin.Succ(Seq(m.Pad.AsCcw())),
            bed: static b => Fin.Succ(Seq(b.Contact.AsCcw())));

    public Fin<Seq<ContactReaction>> Reactions(HoldingPhysics physics) =>
        Switch(
            state: physics,
            pad: static (p, value) => Fin.Succ(Seq1(Frictional(value.Footprint, value.ReactionNormal, value.ClampForceN, p.FrictionMu))),
            jaws: static (p, value) => JawPair(value).Map(loops => loops.Map((loop, index) =>
                Frictional(loop, index == 0 ? value.ReactionNormal : -value.ReactionNormal, 0.5 * value.ClampForceN, p.FrictionMu))),
            chuck: static (p, value) => toSeq(Enumerable.Range(0, value.JawCount)).Traverse(index => {
                double angle = index * Math.Tau / value.JawCount;
                Vector3d radial = new(-Math.Cos(angle), -Math.Sin(angle), 0.0);
                return Jaw(value.Center, value.Radius, value.JawDepth, value.JawWidth, angle, value.Tolerance)
                    .Map(footprint => Contact(footprint, radial, value.RadialGripForceN / value.JawCount,
                        value.AxialCapacityN / value.JawCount)).ToValidation();
            }).As().ToFin(),
            // The sealed vacuum region is the REAL Boolean bed-minus-leaks, and the REACTION GEOMETRY is that
            // sealed topology: one reaction per sealed component, its outer the contact footprint and its signed
            // net area the capacity, so asymmetric or splitting leaks move reaction centers, lever arms, and
            // directional moment margins — the unperforated bed as footprint was the deleted proxy. A fully
            // leaked pad yields zero reactions and the margin math reads it as no capacity.
            vacuum: static (p, value) => ArcAlgebra.ArcBoolean(Seq1(value.Bed), value.LeakZones, BoolKind.Not)
                .Bind(sealedRegion => SealedComponents(sealedRegion).Map(components => components
                    .Filter(static component => component.NetArea > 0.0)
                    .Map(component => Frictional(component.Outer, -Vector3d.ZAxis,
                        value.PadKpa * component.NetArea / 1000.0, p.FrictionMu)))),
            magnet: static (p, value) => Fin.Succ(Seq1(Frictional(value.Pad, -Vector3d.ZAxis,
                value.HoldKpa * Math.Abs(value.Pad.Area()) * value.Coupling / 1000.0, p.FrictionMu))),
            bed: static (p, value) => Fin.Succ(Seq1(Frictional(value.Contact, value.ReactionNormal,
                value.HoldKpa * Math.Abs(value.Contact.Area()) / 1000.0, p.FrictionMu))));

    static ContactReaction Contact(Loop footprint, Vector3d normal, double normalCapacityN, double tangentialCapacityN) {
        Vector3d admitted = normal;
        admitted.Unitize();
        return new ContactReaction(footprint, footprint.Bound().Center, admitted, normalCapacityN, tangentialCapacityN);
    }

    // Every friction-tangential contact is one derivation: tangential capacity = normal capacity x mu; only the
    // chuck carries a datasheet axial rating instead.
    static ContactReaction Frictional(Loop footprint, Vector3d normal, double capacityN, double mu) =>
        Contact(footprint, normal, capacityN, capacityN * mu);

    // Sealed-component census over the Boolean result's winding topology: each negative hole charges its smallest
    // containing positive outer, so every component's footprint and net area stay coupled for the reaction
    // projection and summed capacity always matches sealed net area.
    static Fin<Seq<(Loop Outer, double NetArea)>> SealedComponents(Seq<Loop> region) {
        Seq<(Loop Loop, int Index)> outers = region.Filter(static loop => loop.Winding() == Sign.Positive)
            .Map((loop, index) => (loop, index)).ToSeq();
        Seq<Loop> holes = region.Filter(static loop => loop.Winding() == Sign.Negative);
        return holes.Traverse(hole => outers.Traverse(outer => ArcAlgebra.ArcContains(outer.Loop, hole)
                    .Map(relation => (outer.Index, Inside: relation == ArcRelation.SecondInsideFirst)).ToValidation())
                .As().ToFin()
                .Map(census => (Hole: hole, Owner: census.Filter(static row => row.Inside)
                    .OrderBy(row => Math.Abs(outers[row.Index].Loop.Area()))
                    .Map(static row => row.Index).HeadOrNone()))
                .ToValidation())
            .As().ToFin()
            .Map(assignments => outers.Map(outer => (Outer: outer.Loop,
                NetArea: outer.Loop.Area() + assignments.Filter(row => row.Owner == Some(outer.Index))
                    .Sum(static row => row.Hole.Area()))));
    }

    // Jaw bodies straddle the gripped footprint ALONG the normalized reaction axis — a global-Y pair under an
    // arbitrary ReactionNormal was the promise/body split; the tangent extent spans the footprint projection.
    static Fin<Seq<Loop>> JawPair(Jaws j) {
        Vector3d normal = j.ReactionNormal;
        normal.Unitize();
        Vector3d tangent = new(-normal.Y, normal.X, 0.0);
        Seq<(double N, double T)> spans = toSeq(j.Footprint.Vertices).Map(vertex =>
            ((vertex - Point3d.Origin) * normal, (vertex - Point3d.Origin) * tangent));
        (double nLo, double nHi) = (spans.Map(static span => span.N).Min(), spans.Map(static span => span.N).Max());
        (double tLo, double tHi) = (spans.Map(static span => span.T).Min(), spans.Map(static span => span.T).Max());
        double depth = Math.Max(Math.Max(j.JawDepth, j.ThroatDepthMm), j.Margin);
        return Seq((Lo: nLo - depth, Hi: nLo), (Lo: nHi, Hi: nHi + depth))
            .Traverse(band => Band(normal, tangent, band.Lo, band.Hi, tLo, tHi, j.Footprint.Tolerance).ToValidation())
            .As().ToFin();
    }

    static Fin<Loop> Band(Vector3d normal, Vector3d tangent, double nLo, double nHi, double tLo, double tHi, Context tolerance) =>
        Loop.Admit(
            Arr(
                Point3d.Origin + (normal * nLo) + (tangent * tLo),
                Point3d.Origin + (normal * nHi) + (tangent * tLo),
                Point3d.Origin + (normal * nHi) + (tangent * tHi),
                Point3d.Origin + (normal * nLo) + (tangent * tHi)),
            closed: true, Arr<double>(), tolerance);

    static Fin<Loop> Jaw(Point3d center, double radius, double depth, double width, double angle, Context tolerance) {
        Vector3d radial = new(Math.Cos(angle), Math.Sin(angle), 0.0);
        Vector3d tangent = new(-radial.Y, radial.X, 0.0);
        Point3d inner = center + (radial * Math.Max(0.0, radius - depth));
        Point3d outer = center + (radial * radius);
        double halfWidth = 0.5 * width;
        return Loop.Admit(
            Arr(
                inner - (tangent * halfWidth),
                outer - (tangent * halfWidth),
                outer + (tangent * halfWidth),
                inner + (tangent * halfWidth)),
            closed: true, Arr<double>(), tolerance);
    }
}

// Height-aware keep-out: Keepouts carry exact arc topology for point coverage; Walls carry the once-densified
// polygons every segment test reads. Crosses clips the motion to its z < Height parametric window FIRST, so a
// segment entering above the walls, dipping into the keep-out volume mid-flight, and exiting above still fails
// closed. Guard's Height: double.MaxValue zones block at every Z through the same predicate.
public sealed record ExclusionZone(int Operation, WorkholdingKind Kind, Seq<Loop> Keepouts, Seq<Loop> Walls, double Height, double ArcChordErrorMm) {
    public bool Covers(Point3d point) =>
        point.Z < Height && Keepouts.Count(loop => loop.Covers(point)) % 2 == 1;

    // The clipped sub-segment is straight and fully below Height, so it blocks iff an endpoint or the midpoint
    // sits inside a keep-out or it crosses a wall — total, zero per-move re-densification.
    public bool Crosses(Edge3 segment) =>
        Below(segment).Exists(sub =>
            Covers(Lerp(sub, 0.5)) || Covers(sub.A) || Covers(sub.B) ||
            Walls.Exists(wall => Workholding.SegmentCrosses(sub, wall)));

    Option<Edge3> Below(Edge3 segment) {
        double dz = segment.B.Z - segment.A.Z;
        (double lo, double hi) = Math.Abs(dz) < 1e-12
            ? (0.0, 1.0)
            : dz > 0.0
                ? (0.0, Math.Min(1.0, (Height - segment.A.Z) / dz))
                : (Math.Max(0.0, (Height - segment.A.Z) / dz), 1.0);
        return Math.Min(segment.A.Z, segment.B.Z) < Height && lo < hi
            ? Some(new Edge3(Lerp(segment, lo), Lerp(segment, hi)))
            : None;
    }

    static Point3d Lerp(Edge3 segment, double t) => segment.A + (t * (segment.B - segment.A));
}

public readonly record struct MoveRun(int Loop, int Start, int Count);

public readonly record struct ToolCorridor(Edge3 Axis, double StartRadiusMm, double EndRadiusMm);

// Restraint policy + demand rows: FrictionMu the part-fixture pair's coefficient, SafetyFactor the shop margin;
// CutLoad.ForceN arrives from the committed operation's CuttingData.Force(b, h) — the Kienzle law stays Tooling's.
public sealed record HoldingPhysics(double FrictionMu, double SafetyFactor, double ArcChordErrorMm) {
    public static readonly HoldingPhysics SteelOnSteel = new(FrictionMu: 0.15, SafetyFactor: 2.0, ArcChordErrorMm: 0.01);
    public static readonly HoldingPhysics AluminiumOnSteel = new(FrictionMu: 0.3, SafetyFactor: 2.0, ArcChordErrorMm: 0.01);

    public bool Valid => double.IsFinite(FrictionMu) && FrictionMu >= 0.0 && double.IsFinite(SafetyFactor) && SafetyFactor >= 1.0 &&
        double.IsFinite(ArcChordErrorMm) && ArcChordErrorMm > 0.0;
}

public readonly record struct CutLoad(Vector3d ForceN, Vector3d MomentNmm, Point3d At, Loop ContactRegion);

public readonly record struct ContactReaction(Loop Footprint, Point3d Center, Vector3d Normal, double NormalCapacityN, double TangentialCapacityN);

public readonly record struct AxisMargin(Vector3d Capacity, Vector3d Demand) {
    public double Minimum => new[] {
        Demand.X > 0.0 ? Capacity.X / Demand.X : double.PositiveInfinity,
        Demand.Y > 0.0 ? Capacity.Y / Demand.Y : double.PositiveInfinity,
        Demand.Z > 0.0 ? Capacity.Z / Demand.Z : double.PositiveInfinity,
    }.Min();
}

public readonly record struct HoldingReceipt(
    AxisMargin Force,
    AxisMargin Moment,
    double DirectionalForceMargin,
    double DirectionalMomentMargin,
    Seq<ContactReaction> Contacts) {
    public double MinimumMargin => new[] { Force.Minimum, Moment.Minimum, DirectionalForceMargin, DirectionalMomentMargin }.Min();
    public bool Holds => MinimumMargin >= 1.0;
}

public sealed record Fixture {
    private Fixture(int operation, Seq<Clamp> clamps, Arr<Loop> profiles, Seq<MoveRun> runs, Seq<ExclusionZone> zones,
        Point3d initialCursor, Option<StockSnapshot> current) =>
        (Operation, Clamps, Profiles, Runs, Zones, InitialCursor, Current) =
        (operation, clamps, profiles, runs, zones, initialCursor, current);

    public int Operation { get; }
    public Seq<Clamp> Clamps { get; }
    public Arr<Loop> Profiles { get; }
    public Seq<MoveRun> Runs { get; }
    public Seq<ExclusionZone> Zones { get; }
    public Point3d InitialCursor { get; }
    public Option<StockSnapshot> Current { get; }

    public static readonly Fixture Free = new(0, Seq<Clamp>(), Arr<Loop>(), Seq<MoveRun>(), Seq<ExclusionZone>(), Point3d.Origin, None);

    // Inner-before-outer cutting: profiles order by containment depth DESCENDING over the atoms Covers member —
    // an inner contour cuts before the loop that surrounds it, the same depth law linking's chains apply.
    public Fin<Seq<Move>> Ordered(Seq<Move> moves) {
        if (Runs.IsEmpty) return Fin.Succ(moves);
        Seq<int> indices = Runs.Bind(run => toSeq(Enumerable.Range(run.Start, run.Count)));
        bool partition = Runs.ForAll(run => run.Loop >= 0 && run.Loop < Profiles.Count && run.Start >= 0 && run.Count >= 0 && run.Start + run.Count <= moves.Count) &&
            indices.Count == moves.Count && indices.Distinct().Count() == moves.Count;
        return partition
            ? Fin.Succ(toSeq(Enumerable.Range(0, Profiles.Count))
                .OrderByDescending(index => toSeq(Enumerable.Range(0, Profiles.Count))
                    .Count(other => other != index && Profiles[other].Covers(Profiles[index].At(0))))
                .ToSeq()
                .Bind(index => Runs.Filter(run => run.Loop == index))
                .Bind(run => moves.Skip(run.Start).Take(run.Count)))
            : Fin.Fail<Seq<Move>>(GeometryFault.DegenerateInput("fixture:move-run-partition").ToError());
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Workholding {
    // Per-clamp admission lives INSIDE the accumulating Zone traversal, so every invalid clamp reports beside its
    // siblings; only the fixture-global facts abort ahead of it.
    public static Fin<Fixture> Of(int operation, Seq<Clamp> clamps, Arr<Loop> profiles, Seq<MoveRun> runs, Point3d initialCursor,
        HoldingPhysics physics, Option<StockSnapshot> current = default) =>
        operation < 0 || !physics.Valid || !Finite(initialCursor) || profiles.Exists(static profile => !Profile(profile)) || !ValidRuns(profiles, runs)
            ? Fin.Fail<Fixture>(GeometryFault.DegenerateInput("fixture:admission").ToError())
            : clamps.Traverse(clamp => Zone(clamp, physics.ArcChordErrorMm).ToValidation())
                .As().ToFin()
                .Map(zones => new Fixture(operation, clamps, profiles, runs,
                    zones.Map(zone => zone with { Operation = operation }), initialCursor, current))
                .Bind(fixture => fixture.Current.Match(
                    Some: snapshot => MachinedHit(fixture, snapshot).Bind(hit => hit.Match(
                        Some: point => Fin.Fail<Fixture>(FabricationFault.ClampOnMachinedFace(operation, point).ToError()),
                        None: () => Fin.Succ(fixture))),
                    None: () => Fin.Succ(fixture)));

    public static Fin<ExclusionZone> Zone(Clamp clamp, double arcChordErrorMm) =>
        !Valid(clamp) || !double.IsFinite(arcChordErrorMm) || arcChordErrorMm <= 0.0
            ? Fin.Fail<ExclusionZone>(GeometryFault.DegenerateInput("fixture:zone").ToError())
            : clamp.Footprints().Bind(footprints => footprints
                .Traverse(shape => ArcAlgebra.ShapeOffset(Seq1(shape.AsCcw()), Math.Abs(clamp.MarginOf)).ToValidation())
                .As()
                .ToFin())
                .Map(static inflated => inflated.Bind(static loops => loops))
                .Bind(keepouts => keepouts
                    .Traverse(loop => ArcAlgebra.Densify(loop, arcChordErrorMm).Map(static receipt => receipt.Result).ToValidation())
                    .As().ToFin()
                    .Map(walls => new ExclusionZone(Operation: 0, clamp.Kind, keepouts, walls, clamp.HeightOf, arcChordErrorMm)));

    // `Dilated` is an explicit conservative projection; no exact-zone failure selects it implicitly. Rectangular
    // keep-outs are their own walls.
    public static Fin<ExclusionZone> Dilated(Clamp clamp, double arcChordErrorMm) =>
        !Valid(clamp) || !double.IsFinite(arcChordErrorMm) || arcChordErrorMm <= 0.0
            ? Fin.Fail<ExclusionZone>(GeometryFault.DegenerateInput("fixture:dilated-zone").ToError())
            : clamp.Footprints().Bind(footprints => footprints
                .Traverse(shape => Quad(shape.Bound(), Math.Abs(clamp.MarginOf), shape.Tolerance).ToValidation())
                .As().ToFin()
                .Map(keepouts => new ExclusionZone(Operation: 0, clamp.Kind, keepouts, keepouts, clamp.HeightOf, arcChordErrorMm)));

    public static Fin<bool> Clears(Edge3 segment, Fixture fixture) =>
        !Finite(segment.A) || !Finite(segment.B)
            ? Fin.Fail<bool>(GeometryFault.DegenerateInput("fixture:segment").ToError())
            : Fin.Succ(Blocks(segment, fixture).IsNone);

    public static Fin<bool> Clears(ToolCorridor corridor, Fixture fixture) {
        Vector3d axis = corridor.Axis.B - corridor.Axis.A;
        return !double.IsFinite(corridor.StartRadiusMm) || !double.IsFinite(corridor.EndRadiusMm) ||
               corridor.StartRadiusMm < 0.0 || corridor.EndRadiusMm < corridor.StartRadiusMm || !axis.Unitize()
            ? Fin.Fail<bool>(GeometryFault.DegenerateInput("fixture:tool-corridor").ToError())
            : fixture.Zones.Traverse(zone => Inflated(zone, corridor.EndRadiusMm)
                    .Map(grown => !grown.Crosses(corridor.Axis)).ToValidation())
                .As().ToFin().Map(static verdicts => verdicts.ForAll(identity));
    }

    // Conservative maximum-radius projection: keep-outs grow once by the corridor's END radius, which bounds the
    // linearly varying swept radius from above; the grown zone reuses the height-aware crossing predicate.
    static Fin<ExclusionZone> Inflated(ExclusionZone zone, double radiusMm) =>
        ArcAlgebra.ShapeOffset(zone.Keepouts, radiusMm).Bind(grown => grown
            .Traverse(loop => ArcAlgebra.Densify(loop, zone.ArcChordErrorMm).Map(static receipt => receipt.Result).ToValidation())
            .As().ToFin()
            .Map(walls => zone with { Keepouts = grown, Walls = walls }));

    // EVERY move conditions — rapid and feed alike ride the height-aware predicate; a retract plane above every
    // clamp Height clears structurally, a rapid dragged through a clamp at cutting height routes Collision 2707.
    public static Fin<Seq<Move>> Condition(Seq<Move> moves, Fixture fixture) =>
        moves.Traverse(move => Move.Admit(move).ToValidation()).As().ToFin().Bind(admitted => fixture.Ordered(admitted)).Bind(ordered =>
            fixture.Zones.IsEmpty
                ? Fin.Succ(ordered)
                : ordered
                .Fold(
                    Fin.Succ((Cursor: fixture.InitialCursor, Accepted: Seq<Move>())),
                    (state, move) => state.Bind(cursor => Step(fixture.Zones.HeadOrNone().Map(static zone => zone.ArcChordErrorMm)
                        .ToFin(GeometryFault.DegenerateInput("fixture:zone-axis").ToError()), fixture, cursor, move)))
                .Map(state => state.Accepted));

    // The published clamp-on-machined-face verdict: the hit WITNESS is the actual keepout∩machined-face overlap
    // vertex — never a keep-out or face corner — and a clip failure rides the rail out, never reads as no-hit.
    public static Fin<Option<Point3d>> MachinedHit(Fixture fixture, StockSnapshot snapshot) =>
        fixture.Zones
            .Bind(zone => snapshot.Machined.Map(face => (Region: zone.Keepouts, Face: face)))
            .Fold(Fin.Succ(Option<Point3d>.None), (acc, row) => acc.Bind(hit => hit.IsSome
                ? Fin.Succ(hit)
                : ArcAlgebra.ArcBoolean(row.Region, Seq1(row.Face), BoolKind.And)
                    .Map(overlap => overlap.Find(static loop => loop.Winding() == Sign.Positive && loop.Count > 0).Map(static loop => loop.At(0)))));

    // Slip: Σ per-clamp friction capacity against SafetyFactor·ForceN. Tip: the restraining moment sum about the
    // load's XY point against the overturning moment ForceN·At.Z. Receipt-only — the consumer routes its own fault.
    public static Fin<HoldingReceipt> Restrains(Fixture fixture, CutLoad load, HoldingPhysics physics) =>
        !physics.Valid || fixture.Clamps.Exists(static clamp => !Valid(clamp)) || !Finite(load.ForceN) || !Finite(load.MomentNmm) ||
        !Finite(load.At) || !Profile(load.ContactRegion)
            ? Fin.Fail<HoldingReceipt>(GeometryFault.DegenerateInput("fixture:holding-demand").ToError())
            : fixture.Clamps.Traverse(clamp => clamp.Reactions(physics).ToValidation())
                .As().ToFin().Map(reactions => Receipt(reactions.Bind(identity), load, physics));

    // Callers hand a pre-clipped below-height segment and a pre-densified wall polygon, so the test is pure planar
    // segment-against-segment intersection.
    public static bool SegmentCrosses(Edge3 segment, Loop wall) =>
        toSeq(Enumerable.Range(0, wall.Count)).Exists(index =>
            IntersectionAt(segment, new Edge3(wall.At(index), wall.At(index + 1))).IsSome);

    static Fin<(Point3d Cursor, Seq<Move> Accepted)> Step(
        Fin<double> error,
        Fixture fixture,
        (Point3d Cursor, Seq<Move> Accepted) state,
        Move move) =>
        error.Bind(tolerance => MoveSegments(state.Cursor, move, tolerance).Bind(path =>
            Blocks(path, fixture).Match(
                Some: zone => Fin.Fail<(Point3d Cursor, Seq<Move> Accepted)>(FabricationFault.Collision(zone, CollisionContact.Cutter).ToError()),
                None: () => Fin.Succ((Target(move), state.Accepted.Add(move))))));

    static Option<ExclusionZone> Blocks(Edge3 segment, Fixture fixture) =>
        fixture.Zones.Find(zone => zone.Crosses(segment));

    static Option<ExclusionZone> Blocks(Seq<Edge3> path, Fixture fixture) =>
        fixture.Zones.Find(zone => path.Exists(zone.Crosses));

    static Fin<Seq<Edge3>> MoveSegments(Point3d from, Move move, double errorMm) =>
        move.Switch(
            state:      (from, errorMm),
            rapid:      static (state, row) => Fin.Succ(Seq1(new Edge3(state.from, row.Target))),
            linear:     static (state, row) => Fin.Succ(Seq1(new Edge3(state.from, row.Target))),
            circular:   static (state, row) => ArcSegments(state.from, row.Target, row.Arc, state.errorMm));

    static Fin<Seq<Edge3>> ArcSegments(Point3d from, Point3d to, ArcCenter arc, double errorMm) {
        Vector3d a = from - arc.Center, b = to - arc.Center;
        double radius = a.Length;
        if (!double.IsFinite(radius) || radius <= 0.0 || Math.Abs(radius - b.Length) > errorMm)
            return Fin.Fail<Seq<Edge3>>(GeometryFault.DegenerateInput("fixture:arc-motion").ToError());
        double start = Math.Atan2(a.Y, a.X), end = Math.Atan2(b.Y, b.X);
        bool clockwise = arc.Sense == RotationSense.Clockwise;
        double sweep = clockwise ? -Normalize(start - end) : Normalize(end - start);
        if (from.DistanceTo(to) <= errorMm) sweep = clockwise ? -Math.Tau : Math.Tau;
        double maxStep = 2.0 * Math.Acos(Math.Clamp(1.0 - (errorMm / radius), -1.0, 1.0));
        int count = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / Math.Max(1e-6, maxStep)));
        Seq<Point3d> points = toSeq(Enumerable.Range(0, count + 1)).Map(index => {
            double angle = start + (sweep * index / count);
            double z = from.Z + ((to.Z - from.Z) * index / count);
            return new Point3d(arc.Center.X + (radius * Math.Cos(angle)), arc.Center.Y + (radius * Math.Sin(angle)), z);
        });
        return Fin.Succ(toSeq(Enumerable.Range(0, count)).Map(index => new Edge3(points[index], points[index + 1])));
    }

    static double Normalize(double radians) => radians < 0.0 ? radians + Math.Tau : radians;

    static Point3d Target(Move move) =>
        move.Switch(rapid: static row => row.Target, linear: static row => row.Target, circular: static row => row.Target);

    static HoldingReceipt Receipt(Seq<ContactReaction> contacts, CutLoad load, HoldingPhysics physics) {
        Point3d reference = load.ContactRegion.Bound().Center;
        Vector3d forceDemand = load.ForceN * physics.SafetyFactor;
        Vector3d momentDemand = (load.MomentNmm + Vector3d.CrossProduct(load.At - reference, load.ForceN)) * physics.SafetyFactor;
        Vector3d forceCapacity = contacts.Fold(Vector3d.Zero, (capacity, contact) => capacity + OpposingForce(contact, forceDemand));
        Vector3d momentCapacity = contacts.Fold(Vector3d.Zero, (capacity, contact) => {
            Vector3d generated = Vector3d.CrossProduct(contact.Center - reference, contact.Normal * contact.NormalCapacityN);
            return capacity + new Vector3d(
                generated.X * momentDemand.X < 0.0 ? Math.Abs(generated.X) : 0.0,
                generated.Y * momentDemand.Y < 0.0 ? Math.Abs(generated.Y) : 0.0,
                generated.Z * momentDemand.Z < 0.0 ? Math.Abs(generated.Z) : 0.0);
        });
        return new HoldingReceipt(
            new AxisMargin(forceCapacity, Abs(forceDemand)),
            new AxisMargin(momentCapacity, Abs(momentDemand)),
            DirectionalForceMargin(contacts, forceDemand),
            DirectionalMomentMargin(contacts, reference, momentDemand),
            contacts);
    }

    static double DirectionalForceMargin(Seq<ContactReaction> contacts, Vector3d demand) {
        double magnitude = demand.Length;
        if (magnitude <= 1e-9) return double.PositiveInfinity;
        Vector3d direction = demand / magnitude;
        double capacity = contacts.Sum(contact => Support(contact, -direction));
        return capacity / magnitude;
    }

    static double DirectionalMomentMargin(Seq<ContactReaction> contacts, Point3d reference, Vector3d demand) {
        double magnitude = demand.Length;
        if (magnitude <= 1e-9) return double.PositiveInfinity;
        Vector3d direction = demand / magnitude;
        double capacity = contacts.Sum(contact => Support(contact, Vector3d.CrossProduct(contact.Center - reference, direction)));
        return capacity / magnitude;
    }

    static double Support(ContactReaction contact, Vector3d direction) {
        double axial = direction * contact.Normal;
        double tangential = (direction - (axial * contact.Normal)).Length;
        return (Math.Max(0.0, axial) * contact.NormalCapacityN) + (tangential * contact.TangentialCapacityN);
    }

    static Vector3d OpposingForce(ContactReaction contact, Vector3d demand) =>
        new(
            Tangential(contact, contact.Normal.X) + (contact.Normal.X * demand.X < 0.0 ? Math.Abs(contact.Normal.X) * contact.NormalCapacityN : 0.0),
            Tangential(contact, contact.Normal.Y) + (contact.Normal.Y * demand.Y < 0.0 ? Math.Abs(contact.Normal.Y) * contact.NormalCapacityN : 0.0),
            Tangential(contact, contact.Normal.Z) + (contact.Normal.Z * demand.Z < 0.0 ? Math.Abs(contact.Normal.Z) * contact.NormalCapacityN : 0.0));

    static double Tangential(ContactReaction contact, double normalComponent) =>
        Math.Sqrt(Math.Max(0.0, 1.0 - (normalComponent * normalComponent))) * contact.TangentialCapacityN;

    static Vector3d Abs(Vector3d value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));

    static bool Valid(Clamp clamp) =>
        clamp.Switch(
            pad: static value => Profile(value.Footprint) && Normal(value.ReactionNormal) && Nonnegative(value.Margin, value.Height) && StrictlyPositive(value.ClampForceN),
            jaws: static value => Profile(value.Footprint) && Normal(value.ReactionNormal) && Nonnegative(value.OpeningMinMm, value.StepHeightMm, value.Margin, value.Height) &&
                StrictlyPositive(value.OpeningMaxMm, value.ThroatDepthMm, value.JawDepth, value.ClampForceN) && value.OpeningMinMm <= value.OpeningMaxMm &&
                Span(value.Footprint, value.ReactionNormal) is var grip && grip >= value.OpeningMinMm && grip <= value.OpeningMaxMm,
            chuck: static value => Finite(value.Center) && Nonnegative(value.Margin, value.Height) &&
                StrictlyPositive(value.Radius, value.JawDepth, value.JawWidth, value.JawHeight, value.RadialGripForceN, value.AxialCapacityN) &&
                value.JawDepth <= value.Radius && value.JawCount >= 3,
            vacuum: static value => Profile(value.Bed) && value.LeakZones.ForAll(loop => Profile(loop) && loop.Vertices.ForAll(value.Bed.Covers)) &&
                Nonnegative(value.Margin, value.Height) && StrictlyPositive(value.PadKpa),
            magnet: static value => Profile(value.Pad) && value.Coupling is > 0.0 and <= 1.0 &&
                Nonnegative(value.Margin, value.Height) && StrictlyPositive(value.HoldKpa),
            bed: static value => Profile(value.Contact) && Normal(value.ReactionNormal) && Nonnegative(value.Height) && StrictlyPositive(value.HoldKpa));

    // Grip opening measured along the normalized reaction axis, matching the JawPair body orientation.
    static double Span(Loop footprint, Vector3d normal) {
        Vector3d axis = normal;
        axis.Unitize();
        Seq<double> along = toSeq(footprint.Vertices).Map(vertex => (vertex - Point3d.Origin) * axis);
        return along.Max() - along.Min();
    }

    static bool Profile(Loop loop) => loop.Closed && loop.Count >= 3 && loop.Vertices.ForAll(Finite) && loop.Bulges.ForAll(double.IsFinite);
    static bool Nonnegative(params double[] values) => values.All(static value => double.IsFinite(value) && value >= 0.0);
    static bool StrictlyPositive(params double[] values) => values.All(static value => double.IsFinite(value) && value > 0.0);
    static bool Normal(Vector3d value) => Finite(value) && value.Length > 1e-9;
    static bool Finite(Point3d value) => double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
    static bool Finite(Vector3d value) => double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);

    static bool ValidRuns(Arr<Loop> profiles, Seq<MoveRun> runs) {
        if (!runs.ForAll(run => run.Loop >= 0 && run.Loop < profiles.Count && run.Start >= 0 && run.Count >= 0 &&
            run.Start <= int.MaxValue - run.Count)) return false;
        return runs.OrderBy(static run => run.Start).ToSeq()
            .Fold((End: 0, First: true, Valid: true), static (state, run) =>
                (run.Start + run.Count, false, state.Valid && (state.First || run.Start >= state.End))).Valid;
    }

    static Fin<Loop> Quad(BoundingBox bound, double margin, Context tolerance) =>
        Loop.Admit(
            Arr(
                new Point3d(bound.Min.X - margin, bound.Min.Y - margin, 0.0),
                new Point3d(bound.Max.X + margin, bound.Min.Y - margin, 0.0),
                new Point3d(bound.Max.X + margin, bound.Max.Y + margin, 0.0),
                new Point3d(bound.Min.X - margin, bound.Max.Y + margin, 0.0)),
            closed: true, Arr<double>(), tolerance);

    static Option<double> IntersectionAt(Edge3 motion, Edge3 wall) {
        Vector3d r = motion.B - motion.A, s = wall.B - wall.A, q = wall.A - motion.A;
        double cross = (r.X * s.Y) - (r.Y * s.X);
        if (Math.Abs(cross) < 1e-12) {
            if (Math.Abs((q.X * r.Y) - (q.Y * r.X)) >= 1e-12) return None;
            double left = Project(wall.A, motion), right = Project(wall.B, motion);
            double start = Math.Max(0.0, Math.Min(left, right)), end = Math.Min(1.0, Math.Max(left, right));
            return start <= end ? Some(start) : None;
        }
        double t = ((q.X * s.Y) - (q.Y * s.X)) / cross;
        double u = ((q.X * r.Y) - (q.Y * r.X)) / cross;
        return t is >= 0.0 and <= 1.0 && u is >= 0.0 and <= 1.0 ? Some(t) : None;
    }

    static double Project(Point3d point, Edge3 segment) {
        Vector3d direction = segment.B - segment.A;
        double length2 = direction * direction;
        return length2 <= 1e-18 ? 0.0 : ((point - segment.A) * direction) / length2;
    }
}
```
