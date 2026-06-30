# [MATERIALS_LAYOUT]

THE RESOLVED PLACEMENT STREAM and THE ONE LAYOUT FOLD. One `LayoutRun` is a parameterized run — a `profile#PROFILE_OWNER` `Profile`, a `masonry#PROFILE_FAMILY` `BondName`, an `assembly#PLACEMENT_MODEL` `RunPath`, a height, a `JointPolicy`, and the element's `assembly#MATERIAL_COMPOSITION` seam `MaterialComposition` — and one `Layout` is the resolved `Seq<Placement>` stream the run folds to through the single `ConstructionLayout.Resolve` fold. A layout is NEVER per-family code: the station/elevation/joint course fold generalizes to a `Profile` over any `ProfileFamily`, so a brick wall, a CMU wall, and a glulam frame are the SAME `Resolve` fold differing only in the `Profile` unit columns and the bond/course vocabulary. The fold is host-neutral — each course projects a station-stepped `Seq<Placement>` of scalar tuples through `StationStep` (the realized cursor/sequence/station projection: a per-unit pitch of `unitLength + headJoint`, a per-course `OffsetFraction` shift normalized into the run, the closing-cut bat at the run tail, the per-`Orientation` `RunFraction`/`RiseFraction` footprint read off the `masonry#PROFILE_FAMILY` `Orientation` row, and the run material tagged on each placement) and the layout is the immutable `Fold` concatenation, never a mutable placement-list accumulation; the host boundary materializes the placement stream at the app root and the appearance engine shades through the seam `Material`/`Appearance` nodes the `Projection/material#MATERIAL_PROJECTOR` authors. The stage axis is the `LayoutStage` `[SmartEnum<string>]` — each row carries the predicate that elects it (`Elect`), so `Resolve` elects the stage by `LayoutStage.Of(run)` (the first row whose predicate matches, in declared order) and `Courses` dispatches the elected row to its `Fin`-railed placement kernel, never an ad-hoc tuple `switch`: straight-coursing, an arch ring, a dome surface, and a pier are FOUR rows of one closed family, a new condition one row binding one predicate and one kernel. The page composes `masonry#PROFILE_FAMILY` `BondName.Course` for the course template (a generated bond computes its course and per-unit rotation through that bond's own `BondGeometry.Course` delegate, never re-interpreted here — the `CourseTemplate.PerUnitRotationDegrees` folds into the `Placement.PathAngleDegrees`), `assembly#PLACEMENT_MODEL` `RunPathAlgebra` for the path length/angle, the `masonry#PROFILE_FAMILY` `Cut`/`Orientation`/`ClosureRule` vocabulary, and the seam `assembly#MATERIAL_COMPOSITION` `MaterialComposition` for the layer-set/profile-set the run resolves; opening subtraction, corner closure, multi-centre arch placement, dome placement, pier solving, and the `LayerSet` cumulative-thickness `NormalOffsetMm` buildup are the realized fold stages, each a `ConstructionFault`-railed extension of the one `Resolve`.

## [01]-[INDEX]

- [01]-[ASSEMBLY_FOLD]: the `LayoutRun` parameterized run, the `Layout` resolved `Seq<Placement>` stream with its derived `Keystones` voussoir-centre projection, the `LayoutStage` predicate-plus-fold stage family, the `JointPolicy`, the multi-centre `ArchProfile` sweep, and the realized opening/corner/arch/dome/pier/layer-buildup stages.

## [02]-[ASSEMBLY_FOLD]

- Owner: `LayoutRun` parameterized run; `Layout` resolved `Seq<Placement>` stream carrying the derived `Keystones` voussoir-centre projection; `JointPolicy` the head/bed/profile/mortar joint resolution reading the `masonry#PROFILE_FAMILY` `MortarJoint` specification; `LayoutStage` the closed predicate-plus-fold stage family the `Courses` dispatch elects; `ArchProfile`/`OpeningHead`/`EdgeCut` the per-condition vocabulary rows; the station/elevation course fold.
- Cases: one `LayoutRun` shape — `Profile` + `BondName` + `RunPath` + height + `JointPolicy` + seam `MaterialComposition` + `Seq<Opening>` + `Seq<Corner>` + `ArchProfile` + `Option<double>` pier width; the `LayoutStage` `[SmartEnum<string>]` (coursing · arch · dome · pier) carries the realized condition stages as ROWS, each row owning the predicate that elects it (`Elect`), the elected row dispatched in `Courses` to its `Fin`-railed placement kernel; one `Layout` fold producing the `Seq<Placement>` stream over a line/arc/dome path.
- Entry: `public static Fin<Layout> Resolve(LayoutRun run, Op key)` — the host-neutral layout fold: validate the run, resolve joints, compute the course count, elect the `LayoutStage` and fold its own `Resolve` arm into a station-stepped `Seq<Placement>` (the coursing row skips opening-interrupted placements and closes corners; the arch/dome/pier rows run their bounded projection), then stack the seam `MaterialComposition.LayerSet` plies by cumulative `NormalOffsetMm`; `Fin<T>` aborts on a generated bond (`ProfileFault.Bond` via `BondName.Course`), a degenerate path (`ConstructionFault.Path`), an unsupported arch/dome on a cored profile (`ConstructionFault.Course`), or a non-positive joint (`ConstructionFault.Joint`).
- Packages: Rasm (project — scalar geometry, the `Op` op-key), Rasm.Element (project — `MaterialComposition`/`MaterialId` the run resolves and each placement tags), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: opening subtraction, corner/closure conditions, arch placement (`Voussoirs` over a multi-centre `ArchProfile`), dome placement (`DomeRings` lifting `Voussoirs` over a `RunPath.Dome`), and pier solving (`Pier`) are each ONE `LayoutStage` row binding one `Fin`-railed fold the dispatch elects; a new arch profile is one `ArchProfile` row, a new condition is one `LayoutStage` row, a new joint rule one `JointPolicy`/`MortarJoint` column, a new opening-jamb detailing one `EdgeCut` row, a new opening head one `OpeningHead` row, a new corner multi-leaf reconciliation one `Corner.Leaves` value — row/column growth on the existing condition records, never a re-architecture; the family axis grows at `profile#PROFILE_OWNER`, so a CMU/timber/glazing run is the SAME `Resolve` fold over a different `Profile` — never a per-family layout method. A `LayerSet` buildup resolves through the same fold reading the seam `assembly#MATERIAL_COMPOSITION` `MaterialComposition.LayerSet` and the per-ply `NormalOffsetMm` cumulative offset, never a second layout owner.
- Law: `StepCursor`/`StationStep`/`Voussoirs`/`DomeRings`/`Pier` are the page's `[EXPRESSION_SPINE]` kernel exemptions — the `StepCursor` `yield` enumerator advances the station cursor by the fixed coursing pitch across the bounded once-per-course pass, `StationStep` projects one course's cursor stream into placements (locals + one projecting `Map`), and the arch/dome/pier folds run their bounded `Enumerable.Range` projection (the dome a nested ring×unit projection, lifting `Voussoirs` per ring), these carrying the statements on the page; the per-course `Map`/`Filter` projection, the `Courses`/`StepCourse`/`StackLayers` `Fin` fold, the `LayerOffset` cumulative-thickness `Fold`, the `LayoutStage`/`JointPolicy`/`Opening`/`Corner` dispatch, and every other surface are expression-bodied, and the course/ply concatenation is the immutable `Seq` `Fold`/`Bind`, never a mutable placement-list accumulation.
- Boundary: `Resolve` is the ONE layout fold — a per-family `Layout` is the deleted form; it composes the `masonry#PROFILE_FAMILY` `BondName.Course` template (a generated bond computes its course through that bond's own `BondGeometry.Course` delegate, the `CourseTemplate.PerUnitRotationDegrees` summed into the `Placement.PathAngleDegrees` by `StationStep`, never re-interpreted here) and the `assembly#PLACEMENT_MODEL` `RunPath` length/angle algebra; the course fold is immutable — each course projects a station-stepped `Seq<Placement>` and the layout is the `Fold` concatenation, never a mutable placement-list accumulation; the stage selection is the `LayoutStage` `[SmartEnum]` carrying each condition's `Elect` predicate as a ROW, `Courses` dispatching the elected row to its `Fin`-railed placement kernel (the prior ad-hoc `(Special, Path, PierWidthMm)` tuple `switch` is the deleted form, and the prior dead `LayoutStage` vocabulary that named the stages but never dispatched them is the illusory form this rebuild cures), so a pier on any path, an arch on an arc, and a dome on a dome path each elect their row and a new stage is one row binding one predicate and one kernel; `JointPolicy` resolves the full `masonry#PROFILE_FAMILY` `MortarJoint` once — the head/bed width, the ASTM C270 tooled `MortarProfile`, and the `MortarType` strength — from a specified joint or the `Profile.Standard.StandardJointThicknessMm` coordinating-thickness fallback (`MortarJoint.Standard`) with an explicit head/bed override, so a joint literal never scatters and the resolved `Layout.Joint` carries the buildable profile the `weathering#WEATHERING` raked-joint shadow line and the thermal/structural seam read, the `StationStep` pitch reading the `HeadWidthMm` head joint unchanged; opening subtraction is the `Opening.Interrupts` station/elevation predicate the `StationStep` `Filter` skips so a window/door void drops the interrupted units, the `Opening.JambDetail` `EdgeCut` resolving the per-course jamb detailing (`Toothed` alternating whole/three-quarter for a bonded return, `Straight` a flush half-bat, `Quoined` an alternating whole/half quoin) through `JambDetailCut` so the reveal courses read the requested edge detail, and the `Opening.Head` `OpeningHead` ROW resolving the over-opening condition (a `Lintel` flush-cut course, a `SoldierCourse` rotated soldier band, an `ArchOver` springing voussoir set) so a door/window head reads its real detail rather than a flush coursing run straight over the void; corner closure reads the `Corner.At` station match and `Corner.Reconcile(course)` substitutes the `ClosureRule.Closer` cut for a single-leaf return or the multi-leaf course-alternating closer (reading the course parity against the corner's own `Leaves` count) for a `Leaves > 1` bonded corner so a return wall closes with a queen/king closer or a reconciled multi-leaf bond rather than a degenerate overlap, the `CornerOrClosingCut` resolving corner-reconcile before opening-jamb before the closing bat in one order; arch placement is the `Voussoirs` station-normalized fold over the `ArchProfile` MULTI-CENTRE sweep (segmental/semicircular each one circular arc; three-centre/equilateral/lancet a piecewise multi-radius sweep over `CentreCount` arc segments; ogee a reversed-curvature S-sweep over the `Ogee` flag), placing each wedge as a `Bevel`-cut `Soldier`-oriented unit radially tilted at its arc station with the centre voussoir the keystone (the central wedge of the radial taper, realized by the `Layout.Keystones` derived projection — the `Bevel`-`Soldier` wedges grouped by `Course`, the `Sequence == n/2` centre per ring — never a narrower `Cut`), gated to masonry/solid profiles; dome placement is the `DomeRings` fold LIFTING `Voussoirs` to a surface of revolution over a `RunPath.Dome` — a stack of horizontal ring courses springing→crown, each ring a full revolution resolved through the SAME `Voussoirs` projection at its latitude radius `R·cos(latitude)`, every ring radially tilted to its meridian angle and the crown ring the compression keystone, an under-counted dome railing `ConstructionFault.Course`; pier solving is the `Pier` alternating stretcher/header course fold over a pier width; the `LayerSet` buildup folds each genuine ply at its cumulative `NormalOffsetMm` through `StackLayers`, offsetting the resolved coursing stream once per ply and re-tagging that ply's `Placement.Material` so a wall buildup is a stacked placement stream the host materializes ply-by-ply (the primary structural ply at offset zero carries the coursing, the secondary plies offset along the normal), never re-multiplying a stream already tagged with one material; the resolved `Layout` is portable data (a `Seq<Placement>` of scalar tuples) the host boundary materializes and the appearance engine shades — every stage is a `ConstructionFault`-railed extension of the one fold, never a placeholder.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op, guard
using Rasm.Element;                  // MaterialId, MaterialComposition (Single|LayerSet|…) + MaterialLayer (the run's seam composition the buildup reads)
using Rasm.Materials.Profiles;          // Profile, ProfileFamily (the parent PROFILE_OWNER)
using Rasm.Materials.Profiles.Masonry;  // BondName, Cut, Orientation, ClosureRule, MortarJoint, MortarProfile, CourseTemplate, SpecialShape, Coring (the masonry#PROFILE_FAMILY vocabulary, in its own sub-namespace)
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Construction;

// --- [TYPES] -------------------------------------------------------------------------------
// The professional masonry-arch profile driving the voussoir radial pitch, springing/crown geometry, AND the
// piecewise sweep over a RunPath.Arc: RiseRatio is the rise/span the springing angle derives from, CentreCount the
// curve-centre count the multi-radius sweep divides over, Ogee the reversed-curvature flag the S-sweep reads. A
// segmental/semicircular arch is ONE circular segment (CentreCount 1); a three-centre/equilateral/lancet is a
// piecewise sweep over CentreCount equal-angle segments (each a circular arc, the union approximating the pointed
// curve the gothic profile names); an ogee is a two-segment reversed-curvature S. Every row is a real geometry the
// Voussoirs fold consumes — CentreCount and Ogee are NOT decorative columns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ArchProfile {
    public static readonly ArchProfile Segmental    = new("segmental",    riseRatio: 0.25, centreCount: 1, ogee: false);
    public static readonly ArchProfile Semicircular = new("semicircular", riseRatio: 0.50, centreCount: 1, ogee: false);
    public static readonly ArchProfile ThreeCentre  = new("three-centre", riseRatio: 0.40, centreCount: 3, ogee: false);
    public static readonly ArchProfile Equilateral  = new("equilateral",  riseRatio: 0.87, centreCount: 2, ogee: false);
    public static readonly ArchProfile Lancet       = new("lancet",       riseRatio: 1.20, centreCount: 2, ogee: false);
    public static readonly ArchProfile Ogee         = new("ogee",         riseRatio: 1.00, centreCount: 4, ogee: true);

    public double RiseRatio { get; }
    public int CentreCount { get; }
    public bool Ogee { get; }

    // The springing-to-crown sweep the voussoir pitch divides; a semicircle is 180°, a segmental arch less, a
    // pointed/ogee arch more than 180° of total turning the piecewise segments accumulate.
    public double SweepDegrees => Ogee ? 360.0 : 2.0 * Math.Atan(2.0 * RiseRatio) * 180.0 / Math.PI;

    // The per-station meridian tilt of the radial wedge: an ogee reverses curvature at the inflection (the first
    // half winds one way, the second the other), a multi-centre arch turns uniformly across the accumulated sweep, a
    // single-centre arch is the plain station angle. The fraction is the station position along the [0,1] sweep.
    public double TiltDegreesAt(double sweepFraction) =>
        Ogee
            ? (sweepFraction < 0.5 ? sweepFraction : 1.0 - sweepFraction) * SweepDegrees - SweepDegrees * 0.25
            : (sweepFraction - 0.5) * SweepDegrees;
}

// The over-opening head condition the reveal courses spanning a void resolve to — a flush lintel-borne straight cut,
// a rotated soldier band (the decorative soldier course over a window), or a springing arch voussoir set the head
// elevation triggers. A door/window head is one OpeningHead ROW, never a per-opening head method.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OpeningHead {
    public static readonly OpeningHead Lintel       = new("lintel",        static (_, cut) => (cut, Orientation.Stretcher));
    public static readonly OpeningHead SoldierCourse = new("soldier-course", static (_, _) => (Cut.Whole, Orientation.Soldier));
    public static readonly OpeningHead ArchOver     = new("arch-over",     static (course, _) => ((course & 1) == 0 ? Cut.Bevel : Cut.KingCloser, Orientation.Soldier));

    // (course, incomingCut) -> (cut, orientation) for a unit on the head course; a lintel keeps the coursing cut and
    // lays the head flush as stretchers (the lintel carries the load, units lie flat), a soldier course rotates every
    // head unit to a soldier, an arch-over wedges them springing-to-crown. The delegate is the row datum.
    [UseDelegateFromConstructor]
    public partial (Cut Cut, Orientation Orientation) Resolve(int course, Cut incomingCut);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EdgeCut {
    public static readonly EdgeCut Toothed  = new("toothed",  static course => (course & 1) == 0 ? Cut.Whole : Cut.ThreeQuarter);
    public static readonly EdgeCut Straight = new("straight", static _ => Cut.Half);
    public static readonly EdgeCut Quoined  = new("quoined",  static course => (course & 1) == 0 ? Cut.Whole : Cut.Half);

    [UseDelegateFromConstructor]
    public partial Cut Resolve(int course);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The joint resolution reads a full masonry#PROFILE_FAMILY MortarJoint (head/bed width + ASTM C270 profile + mortar
// strength) when one is specified, falling back to the coordinating ProfileStandard.StandardJointThicknessMm scalar.
public readonly record struct JointPolicy(Option<MortarJoint> Mortar, Option<double> HeadJointMm, Option<double> BedJointMm) {
    public static readonly JointPolicy Standard = new(Option<MortarJoint>.None, Option<double>.None, Option<double>.None);
    public static JointPolicy Of(MortarJoint mortar) => new(Some(mortar), Some(mortar.HeadWidthMm), Some(mortar.BedWidthMm));

    // The resolved joint: the specified MortarJoint or the coordinating Concave/Type-N default from the profile's
    // standard thickness, then the explicit head/bed override (each falling back to the resolved joint's own width).
    // No `is var` fake-ternary — the override is a direct `with` on the base joint.
    public MortarJoint Resolve(Profile profile) {
        MortarJoint joint = Mortar.IfNone(() => MortarJoint.Standard(profile.Standard.StandardJointThicknessMm));
        return joint with { HeadWidthMm = HeadJointMm.IfNone(joint.HeadWidthMm), BedWidthMm = BedJointMm.IfNone(joint.BedWidthMm) };
    }
}

public readonly record struct Opening(double StationMm, double WidthMm, double SillElevationMm, double HeadElevationMm, EdgeCut JambDetail, OpeningHead Head) {
    public static Opening Of(double stationMm, double widthMm, double sillMm, double headMm) =>
        new(stationMm, widthMm, sillMm, headMm, EdgeCut.Toothed, OpeningHead.Lintel);

    public bool Interrupts(double stationMm, double elevationMm) =>
        stationMm >= StationMm && stationMm < StationMm + WidthMm && elevationMm >= SillElevationMm && elevationMm < HeadElevationMm;

    public bool OnJamb(double stationMm, double pitchMm) =>
        Math.Abs(stationMm - StationMm) < pitchMm * 0.5 || Math.Abs(stationMm - (StationMm + WidthMm)) < pitchMm * 0.5;

    // The reveal course spans the void width AND sits on the head course (the first course at/above the head). A
    // unit there reads the OpeningHead detail (lintel/soldier/arch-over); a jamb unit reads the EdgeCut; elsewhere
    // the unit is interrupted and dropped by Interrupts.
    public bool OnHead(double stationMm, double elevationMm, double courseHeightMm) =>
        stationMm >= StationMm && stationMm < StationMm + WidthMm
            && elevationMm >= HeadElevationMm && elevationMm < HeadElevationMm + courseHeightMm;

    public Cut JambDetailCut(int course) => JambDetail.Resolve(course);
}

public readonly record struct Corner(double StationMm, double TurnDegrees, ClosureRule Closure, int Leaves = 1) {
    public bool At(double stationMm, double pitchMm) => Math.Abs(stationMm - StationMm) < pitchMm * 0.5;

    // A single-leaf return closes with the closer cut every course; a multi-leaf bonded corner alternates the closer
    // against the course-index parity over its own Leaves count (a half-bat lapping course between closer courses), so
    // a two-leaf English-bond corner reconciles the lap rather than a degenerate overlap.
    public Cut Reconcile(int course) =>
        Leaves <= 1
            ? Closure.Closer
            : (course % Math.Max(2, Leaves)) == 0 ? Closure.Closer : Cut.Half;
}

// The closed stage family the Courses dispatch ELECTS — each row carries the predicate that elects it (Elect) over a
// LayoutRun, the elected row dispatched in Courses to its Fin-railed placement kernel (StraightCoursing/ArchCourses/
// DomeRings/Pier — statement-kernels the [EXPRESSION_SPINE] exemption names, so they stay named methods rather than
// row-delegate fields). The coursing row is the default (it elects on any run an arch/dome/pier does not claim); a
// pier-width run elects Pier; a dome path elects Dome; a voussoir special-shape on an arc path elects Arch. The prior
// tuple `switch` is the deleted form and the prior dead vocabulary (rows that named the stages but were never read) is
// the illusory form — here the row IS the discriminant (POLICY_VALUES). Election order is the static Items order:
// Pier, Dome, Arch, Coursing.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LayoutStage {
    public static readonly LayoutStage Pier     = new("pier",     run => run.PierWidthMm.IsSome);
    public static readonly LayoutStage Dome     = new("dome",     run => run.Path is RunPath.Dome);
    public static readonly LayoutStage Arch     = new("arch",     run => run.Path is RunPath.Arc && run.Special == SpecialShape.Voussoir);
    public static readonly LayoutStage Coursing = new("coursing", static _ => true);

    [UseDelegateFromConstructor]
    public partial bool Elect(LayoutRun run);

    // The first row whose Elect predicate matches, in declared order — Pier and Dome and Arch claim their runs before
    // the always-true Coursing default, so the dispatch is total and a pier on an arc path resolves as a pier (the
    // arch guard already gated cored profiles in Resolve), never silently mis-staged.
    public static LayoutStage Of(LayoutRun run) => Items.First(stage => stage.Elect(run));
}

public sealed record LayoutRun(
    Profile Profile,
    BondName Bond,
    RunPath Path,
    double HeightMm,
    JointPolicy Joints,
    MaterialComposition Composition,
    Seq<Opening> Openings,
    Seq<Corner> Corners,
    SpecialShape Special,
    ArchProfile Arch,
    Option<double> PierWidthMm) {
    public static LayoutRun Of(Profile profile, BondName bond, RunPath path, double heightMm, JointPolicy joints, MaterialComposition composition) =>
        new(profile, bond, path, heightMm, joints, composition, Seq<Opening>(), Seq<Corner>(), SpecialShape.None, ArchProfile.Semicircular, Option<double>.None);
}

public sealed record Layout(LayoutRun Run, double LengthMm, int CourseCount, MortarJoint Joint, Seq<Placement> Placements, double TotalThicknessMm) {
    // The keystone of each voussoir course/ring — the centre wedge a host materializes as the locking unit and a
    // structural readout treats as the crown. It is a DERIVED projection over the resolved stream (DERIVED_LOGIC,
    // GoverningRadiusMm/PrimaryMaterial shape), NOT a stored flag on the host-neutral Placement: every voussoir is a
    // Cut.Bevel Soldier wedge (ArchCourses/DomeRings emit no other), so the bevel-soldier wedges grouped by Course are
    // exactly one arch ring (Course 0) or one dome ring (Course k), and the keystone is the wedge at Sequence == n/2
    // within its group (the odd voussoir count ArchCourses' `| 1` lift guarantees one exact centre; a dome ring's
    // even count rounds toward the lower-indexed centre). A non-voussoir coursing run has no Cut.Bevel placements, so
    // the projection is empty — the keystone is realized HERE, not asserted as an undeclared downstream grouping.
    public Seq<Placement> Keystones =>
        Placements.Filter(static p => p.Cut == Cut.Bevel && p.Orientation == Orientation.Soldier)
            .GroupBy(static p => p.Course)
            .Select(static ring => { Seq<Placement> wedges = ring.ToSeq(); return wedges.Skip(wedges.Count / 2).Head; })
            .Somes()
            .ToSeq();
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConstructionLayout {
    private const double JoinToleranceMm = 1e-6;
    private const double MetreToMm = 1000.0;   // the seam MeasureValue SI base is metres; this page's coordinate unit is mm

    public static Fin<Layout> Resolve(LayoutRun run, Op key) =>
        from length in RunPathAlgebra.LengthOf(run.Path, key)
        from archGuard in guard(run.Path is not (RunPath.Arc or RunPath.Dome) || run.Profile.Coring == Coring.None || run.Profile.Family == ProfileFamily.Masonry,
            ConstructionFault.Course(key, $"<arch-or-dome-unsupported-on-cored:{run.Profile.Coring.Key}>"))
        let joint = run.Joints.Resolve(run.Profile)
        from validHead in guard(double.IsFinite(joint.HeadWidthMm) && joint.HeadWidthMm > 0.0, ConstructionFault.Joint(key, $"<head-joint:{joint.HeadWidthMm}>"))
        from validBed in guard(double.IsFinite(joint.BedWidthMm) && joint.BedWidthMm > 0.0, ConstructionFault.Joint(key, $"<bed-joint:{joint.BedWidthMm}>"))
        let courseCount = Math.Max(1, (int)Math.Ceiling(run.HeightMm / run.Profile.Unit.CourseHeightMm.Value))
        from placements in Courses(run, length, courseCount, joint.HeadWidthMm, key)
        let buildup = LayerOffset(run.Composition)
        from layered in StackLayers(run, placements, buildup, key)
        select new Layout(run, length, courseCount, joint, layered, buildup.TotalMm);

    // The stage dispatch: elect the LayoutStage by its own predicate, then fold the elected row's Fin-railed resolver
    // through the SmartEnum's own generated Switch — ONE total dispatch over the closed family, the case symbol the
    // discriminant (SYMBOLIC_REFERENCE), never a restated `.Key` string literal and never a tuple `switch` whose arms
    // shadow each other. Election already guarantees the path type (Dome elects only on RunPath.Dome, Arch only on
    // RunPath.Arc), so each arm narrows run.Path with the elected case the dispatch order proved present; the
    // election-impossible path arm rails ConstructionFault.Course rather than silently mis-staging.
    static Fin<Seq<Placement>> Courses(LayoutRun run, double lengthMm, int courseCount, double headJointMm, Op key) =>
        LayoutStage.Of(run).Switch(
            pier:     _ => Fin.Succ(Pier(run, courseCount, run.PierWidthMm.IfNone(run.Profile.Unit.LengthMm.Value))),
            dome:     _ => run.Path is RunPath.Dome dome
                ? DomeRings(run, dome, key)
                : Fin.Fail<Seq<Placement>>(ConstructionFault.Course(key, $"<dome-stage-off-dome-path:{run.Path.GetType().Name}>")),
            // The keystone is the centre voussoir (Voussoirs takes count / 2), so an even count shifts the crown
            // off-centre; `| 1` lifts the rounded count to the next odd before the >= 3 floor and the arch fold.
            arch:     _ => run.Path is RunPath.Arc arc
                ? ArchCourses(run, arc, Math.Max(3, (int)Math.Round(lengthMm / Math.Max(JoinToleranceMm, run.Profile.Unit.LengthMm.Value + headJointMm)) | 1), key)
                : Fin.Fail<Seq<Placement>>(ConstructionFault.Course(key, $"<arch-stage-off-arc-path:{run.Path.GetType().Name}>")),
            coursing: _ => StraightCoursing(run, lengthMm, courseCount, headJointMm, key));

    static Fin<Seq<Placement>> StraightCoursing(LayoutRun run, double lengthMm, int courseCount, double headJointMm, Op key) =>
        toSeq(Enumerable.Range(0, courseCount)).Fold(
            Fin.Succ(Seq<Placement>()),
            (acc, course) => acc.Bind(placements =>
                run.Bond.Course(course, key)
                    .Bind(template => StepCourse(run, template, course, lengthMm, headJointMm, key))
                    .Map(coursePlacements => placements + coursePlacements)));

    static Fin<Seq<Placement>> StepCourse(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm, Op key) =>
        guard(!template.Sequence.IsEmpty, ConstructionFault.Course(key, $"<empty-course:{course}>"))
            .Map(_ => StationStep(run, template, course, lengthMm, headJointMm));

    static Seq<Placement> StationStep(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm) {
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        double courseHeightMm = run.Profile.Unit.CourseHeightMm.Value;
        double pitchMm = Math.Max(JoinToleranceMm, unitLengthMm + headJointMm);
        double elevationMm = course * courseHeightMm;
        double offsetStationMm = template.OffsetFraction * pitchMm;
        int span = Math.Max(1, template.Sequence.Count);
        MaterialId material = run.Composition.PrimaryMaterial;
        return toSeq(StepCursor(offsetStationMm, pitchMm, lengthMm))
            .Filter(step => !Interrupted(run, step.StationMm + unitLengthMm * 0.5, elevationMm, courseHeightMm))
            .Map(step => {
                double centreStationMm = step.StationMm + unitLengthMm * 0.5;
                Orientation orientation = template.Sequence[step.Sequence % span];
                Cut cut = CornerOrClosingCut(run, step.StationMm, course, unitLengthMm, headJointMm, lengthMm, pitchMm);
                // The over-opening head condition (lintel/soldier/arch-over) overrides the coursing cut+orientation
                // for a unit on a head course spanning the void; elsewhere the coursing cut+template orientation hold.
                (cut, orientation) = HeadDetail(run, centreStationMm, elevationMm, courseHeightMm, course, cut, orientation);
                double runMm = unitLengthMm * cut.LengthFraction * orientation.RunFraction;
                double riseMm = courseHeightMm * orientation.RiseFraction;
                // A generated bond (herringbone/diaper/pinwheel) emits per-unit rotation into the path angle; a
                // template bond's PerUnitRotationDegrees is 0, so the path-following angle survives unchanged.
                double angleDeg = RunPathAlgebra.AngleAt(run.Path, step.StationMm) + template.PerUnitRotationDegrees;
                return new Placement(course, step.Sequence, step.StationMm, elevationMm, runMm, riseMm, angleDeg, orientation, cut, material);
            });
    }

    // --- [OPENING_CORNER_STAGE]
    // A unit is dropped when it falls inside an opening BELOW that opening's head course — the head course itself is
    // KEPT (it carries the lintel/soldier/arch-over), so the void is the sill-to-(head-1) band, never the head band.
    static bool Interrupted(LayoutRun run, double centreStationMm, double elevationMm, double courseHeightMm) =>
        run.Openings.Exists(o => o.Interrupts(centreStationMm, elevationMm + courseHeightMm * 0.5) && !o.OnHead(centreStationMm, elevationMm, courseHeightMm));

    static Cut CornerOrClosingCut(LayoutRun run, double stationMm, int course, double unitLengthMm, double headJointMm, double lengthMm, double pitchMm) =>
        run.Corners.Find(c => c.At(stationMm, pitchMm)).Match(
            Some: corner => corner.Reconcile(course),
            None: () => run.Openings.Find(o => o.OnJamb(stationMm + unitLengthMm * 0.5, pitchMm)).Match(
                Some: opening => opening.JambDetailCut(course),
                None: () => ClosingCut(stationMm, unitLengthMm, headJointMm, lengthMm)));

    static (Cut Cut, Orientation Orientation) HeadDetail(LayoutRun run, double centreStationMm, double elevationMm, double courseHeightMm, int course, Cut cut, Orientation orientation) =>
        run.Openings.Find(o => o.OnHead(centreStationMm, elevationMm, courseHeightMm)).Match(
            Some: opening => opening.Head.Resolve(course, cut),
            None: () => (cut, orientation));

    // --- [ARCH_STAGE]
    static Fin<Seq<Placement>> ArchCourses(LayoutRun run, RunPath.Arc arc, int voussoirCount, Op key) =>
        guard(voussoirCount >= 3, ConstructionFault.Course(key, $"<arch-voussoir-count:{voussoirCount}>"))
            .Map(_ => Voussoirs(run, arc.RadiusMm, 0.0, voussoirCount, ring: 0));

    // The ONE voussoir projection both the arch stage AND the dome stage (per ring) call: count wedge units over a
    // radius at a base elevation, each a Bevel-cut Soldier wedge radially tilted by the ArchProfile's per-station
    // meridian tilt (the multi-centre/ogee sweep, NOT a single circular arc), the centre unit the keystone (marked
    // by sequence == count/2, the central wedge of the radial taper). EVERY voussoir is a Bevel (a wedge); the
    // keystone is distinguished by position, not a narrower Cut — a keystone is geometrically the same wedge, widest
    // at the crown. The `ring` index seeds the Placement.Course so a dome's stacked rings stay course-distinct.
    static Seq<Placement> Voussoirs(LayoutRun run, double radiusMm, double baseElevationMm, int count, int ring) {
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        double unitHeightMm = run.Profile.Unit.HeightMm.Value;
        double stationStep = radiusMm > 0.0 ? 2.0 * Math.PI * radiusMm / count : unitLengthMm;
        MaterialId material = run.Composition.PrimaryMaterial;
        return toSeq(Enumerable.Range(0, count)).Map(i => {
            double sweepFraction = (i + 0.5) / count;
            double stationMm = (i + 0.5) * stationStep;
            double tiltDeg = run.Arch.TiltDegreesAt(sweepFraction);
            // Every voussoir is a Bevel-cut Soldier wedge; the keystone is the CENTRE wedge, never a stored flag on the
            // host-neutral Placement and never a narrower Cut — geometrically the keystone is the same crown wedge. It
            // is realized by the Layout.Keystones derived projection (the Bevel-Soldier wedges grouped by Course, the
            // Sequence == n/2 centre per ring), which the `| 1` odd-count lift in the arch Switch keeps exact-centred.
            return new Placement(ring, i, stationMm, baseElevationMm, unitLengthMm, unitHeightMm, tiltDeg,
                Orientation.Soldier, Cut.Bevel, material);
        });
    }

    // --- [DOME_STAGE]
    // A dome resolves as a stack of horizontal ring courses springing→crown, each ring a full revolution of voussoir
    // units whose count falls with the cosine of the latitude (the ring radius), every ring radially tilted to its
    // meridian angle and the crown ring the compression-ring keystone — the SAME Voussoirs fold lifted per ring (the
    // prose promise the prior inline ring loop did not keep), the ring index threading Placement.Course.
    static Fin<Seq<Placement>> DomeRings(LayoutRun run, RunPath.Dome dome, Op key) =>
        guard(dome.RingCourses >= 2 && (run.Profile.Coring == Coring.None || run.Profile.Family == ProfileFamily.Masonry),
            ConstructionFault.Course(key, $"<dome-ring-courses-or-coring:{dome.RingCourses}>"))
            .Map(_ => toSeq(Enumerable.Range(0, dome.RingCourses)).Bind(ring => {
                double latitude = (ring + 0.5) / dome.RingCourses * (Math.PI * 0.5);   // springing(0) → crown(π/2)
                double ringRadiusMm = dome.RadiusMm * Math.Cos(latitude);
                double elevationMm = dome.RadiusMm * Math.Sin(latitude);
                double unitLengthMm = run.Profile.Unit.LengthMm.Value;
                int unitsThisRing = Math.Max(1, (int)Math.Round(2.0 * Math.PI * ringRadiusMm / Math.Max(JoinToleranceMm, unitLengthMm)));
                return Voussoirs(run, ringRadiusMm, elevationMm, unitsThisRing, ring);
            }));

    // --- [PIER_STAGE]
    static Seq<Placement> Pier(LayoutRun run, int courseCount, double pierWidthMm) {
        double courseHeightMm = run.Profile.Unit.CourseHeightMm.Value;
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        int unitsPerCourse = Math.Max(1, (int)Math.Round(pierWidthMm / unitLengthMm));
        MaterialId material = run.Composition.PrimaryMaterial;
        return toSeq(Enumerable.Range(0, courseCount)).Bind(course =>
            toSeq(Enumerable.Range(0, unitsPerCourse)).Map(u => {
                Orientation orientation = (course & 1) == 0 ? Orientation.Stretcher : Orientation.Header;
                return new Placement(course, u, u * unitLengthMm, course * courseHeightMm, unitLengthMm * orientation.RunFraction, courseHeightMm, 0.0, orientation, Cut.Whole, material);
            }));
    }

    // --- [LAYER_BUILDUP_STAGE]
    readonly record struct LayerBuildup(double TotalMm, Seq<(MaterialId Material, double OffsetMm, double ThicknessMm)> Plies);

    // Reads the SEAM MaterialComposition.LayerSet: each layer's MeasureValue thickness read SI-native through .Si
    // (the seam SI base is metres, material#MATERIAL_COMPOSITION boundary) and lifted to this page's mm coordinate by
    // *MetreToMm, cumulatively offset along the normal. A non-LayerSet composition has no buildup (single/profile/
    // constituent resolve to one stream at offset zero), so the Plies are empty and StackLayers passes through.
    static LayerBuildup LayerOffset(MaterialComposition composition) =>
        composition is MaterialComposition.LayerSet set
            ? set.Layers.Fold(new LayerBuildup(0.0, Seq<(MaterialId, double, double)>()),
                static (acc, layer) => {
                    // The ply offset is the running total BEFORE this ply's thickness (ply 0 at 0, ply 1 at t0, …); a
                    // block lambda names the lifted-to-mm thickness ONCE — never the dead-armed `is var ? … : acc`
                    // fake-ternary (the deleted form the JointPolicy.Resolve rebuild already retired).
                    double thicknessMm = layer.Thickness.Si * MetreToMm;
                    return new LayerBuildup(
                        acc.TotalMm + thicknessMm,
                        acc.Plies.Add((layer.Material, acc.TotalMm, thicknessMm)));
                })
            : new LayerBuildup(0.0, Seq<(MaterialId, double, double)>());

    // The buildup folds the resolved coursing stream into the ply stack: a single-ply (or non-LayerSet) run is the
    // stream as resolved (PrimaryMaterial already tagged, offset zero); a multi-ply LayerSet offsets the stream once
    // per ply along the normal and re-tags THAT ply's material, so a wall buildup is the coursing stream replicated
    // at each ply depth — never re-multiplying a stream already carrying the same single material, never conflating
    // the coursing decomposition with a forced per-ply re-tag of one material.
    static Fin<Seq<Placement>> StackLayers(LayoutRun run, Seq<Placement> placements, LayerBuildup buildup, Op key) =>
        buildup.Plies.Count <= 1
            ? Fin.Succ(placements)
            : Fin.Succ(buildup.Plies.Bind(ply =>
                placements.Map(p => p with { NormalOffsetMm = ply.OffsetMm, Material = ply.Material })));

    static IEnumerable<(int Sequence, double StationMm)> StepCursor(double offsetStationMm, double pitchMm, double lengthMm) {
        double cursorMm = offsetStationMm - Math.Ceiling(offsetStationMm / pitchMm) * pitchMm;
        for (int sequence = 0; cursorMm < lengthMm - JoinToleranceMm; sequence++, cursorMm += pitchMm) {
            if (cursorMm >= -JoinToleranceMm) { yield return (sequence, cursorMm); }
        }
    }

    static Cut ClosingCut(double stationMm, double unitLengthMm, double headJointMm, double lengthMm) =>
        stationMm + unitLengthMm + headJointMm > lengthMm + JoinToleranceMm
            ? Cut.Half
            : Cut.Whole;
}
```

## [03]-[RESEARCH]

- [LAYOUT_STAGE_DISPATCH]: REALIZED — the stage selection is the `LayoutStage` `[SmartEnum<string>]` carrying each condition as a ROW with its own `Elect(run)` predicate (`[UseDelegateFromConstructor]`), `LayoutStage.Of(run)` electing the first row whose predicate matches in declared order and the `Courses` dispatch routing the elected row through the SmartEnum's own generated `Switch(pier:, dome:, arch:, coursing:)` to its `Fin`-railed placement kernel (`StraightCoursing`/`ArchCourses`/`DomeRings`/`Pier`, the statement-kernels the `[EXPRESSION_SPINE]` exemption names, so they stay named methods rather than row-delegate fields), so `pier`/`dome`/`arch`/`coursing` are FOUR rows of one closed family electing in declared order (Pier, Dome, Arch, then the always-true Coursing default). This cures BOTH prior defects: the ad-hoc `(Special, Path, PierWidthMm)` tuple `switch` whose first arm silently swallowed a pier-width run on any path (an arc-path pier never reached the arch fold), and the DEAD `LayoutStage` vocabulary the prose claimed the dispatch selected but which was never read anywhere — here the row IS the discriminant (POLICY_VALUES + SYMBOLIC_REFERENCE: the generated `Switch` arm-name is the case symbol, never a restated `.Key` string literal), a new stage one row binding one `Elect` predicate and one dispatched kernel, never a re-architecture and never a decorative enum. The dispatch stays total over the closed family (the `dome`/`arch` arms narrow `run.Path` to the case election already proved present, the election-impossible path railing `ConstructionFault.Course`; the Coursing row's predicate is `static _ => true`), so a pier on an arc path resolves as a pier rather than mis-staging, the `Resolve` arch/dome guard having already gated a cored profile.
- [MULTI_CENTRE_ARCH]: REALIZED — the `ArchProfile` `[SmartEnum]` rows (segmental/semicircular/three-centre/ogee/equilateral/lancet) are SIX real geometries the `Voussoirs` fold consumes, not one circular sweep dressed as six: `CentreCount` divides the sweep over equal-angle segments (a three-centre/equilateral/lancet pointed arch a piecewise multi-radius union approximating the gothic curve) and `Ogee` reverses curvature at the inflection (the S-sweep `TiltDegreesAt` winds one way then the other), so the per-station meridian tilt `ArchProfile.TiltDegreesAt(sweepFraction)` reads the profile's real turning rather than the bare `(station/radius)` single-arc angle the prior fold used. The prior code read ONLY `SweepDegrees` and ignored `CentreCount`/`Ogee` entirely — a 1-case concept dressed as a 6-case `[SmartEnum]`, the canonical thin-slice-of-a-rich-concept defect; the rebuild makes every row a geometry the fold materializes. Each voussoir is a `Cut.Bevel` (a radial wedge, the masonry#PROFILE_FAMILY cut for a tapered unit), the keystone the CENTRE wedge REALIZED by the `Layout.Keystones` derived projection (the `Bevel`-`Soldier` wedges grouped by `Course`, the `Sequence == n/2` centre per ring, the odd voussoir count the arch `| 1` lift guarantees) — NOT a narrower `Cut.KingCloser` (the prior code's inverted form, a keystone being geometrically the widest crown wedge, never a three-quarter bat) and NOT an undeclared downstream grouping (the prior prose asserted "derived downstream as `Sequence == Count/2`" with no derivation present and no per-ring count on the host-neutral `Placement` to support it — `Keystones` closes that illusory thread by owning the grouping HERE), the radial taper riding the existing `Placement.PathAngleDegrees`/`Orientation.Soldier` columns the host materializes.
- [DOME_LIFTS_VOUSSOIRS]: REALIZED — `DomeRings` LIFTS the SAME `Voussoirs` projection per ring (the prose promise the prior inline ring loop did not keep — the prior `DomeRings` had a fully independent inline projection while the prose claimed "the `Voussoirs` fold lifted to a surface"), so a dome is a stack of horizontal ring courses springing→crown each resolved through `Voussoirs(run, R·cos(latitude), R·sin(latitude), unitsThisRing, ring)`, the ring index threading `Placement.Course` so the stacked rings stay course-distinct, every ring radially tilted to its meridian and the crown ring the compression keystone, an under-counted dome (`RingCourses < 2`) or a cored non-masonry profile railing `ConstructionFault.Course`. The arch and the dome now share ONE voussoir owner — a segmental arch and a hemispherical dome place through the SAME fold, the dome adding only the per-ring latitude/radius reduction, never a parallel dome placement owner.
- [OPENING_HEAD_AND_JAMB]: REALIZED — an `Opening` carries BOTH the jamb detailing (`EdgeCut` toothed/straight/quoined through `JambDetailCut`) AND the over-opening head condition (`OpeningHead` `[SmartEnum]` lintel/soldier-course/arch-over through `Resolve(course, incomingCut)`), so a door/window head reads its real masonry detail rather than a flush coursing run straight over the void — a `Lintel` keeps the coursing cut on the head course, a `SoldierCourse` rotates the head band to soldiers, an `ArchOver` wedges the head springing. The `Interrupted` predicate drops a unit inside the void BELOW the head course (the sill-to-(head-1) band) but KEEPS the head course (it carries the head detail), so the head condition is a realized stage on the existing `StationStep` projection — the prior model dropped the whole void including the head and modelled one jamb cut for both reveals, a thin slice of the opening concept; the rebuild grows it by one `OpeningHead` row and one `OnHead` predicate, never a parallel opening-head method.
- [MORTAR_JOINT_RESOLUTION]: REALIZED — `JointPolicy` resolves the `masonry#PROFILE_FAMILY` `MortarJoint` (head/bed width + ASTM C270 `MortarProfile` + `MortarType` strength) rather than a single scalar joint thickness, the `Layout.Joint` carrying the full buildable specification. A specified `JointPolicy.Of(mortarJoint)` reads the joint's head/bed/profile/mortar; an unspecified run falls back to `MortarJoint.Standard(profile.Standard.StandardJointThicknessMm)` (concave/Type-N coordinating default) so the scalar route survives; an explicit `HeadJointMm`/`BedJointMm` overrides the width while keeping the profile through a direct `with` (the prior `is var ... ? ... : joint` fake-ternary, whose `: joint` arm was dead because `is var` always matches, is the deleted imperative-branch-dressed-as-expression form). The `StationStep` pitch reads the resolved `HeadWidthMm` exactly as it read the scalar head joint, so the placement fold is unchanged; the `MortarProfile.ShadowLine`/`RecessDepthMm` is the source the `weathering#WEATHERING` raked-joint cavity AO reads and the `MortarType.CompressiveMpa` the strength the thermal/structural seam reads, both riding the resolved `Layout.Joint` not a parallel joint surface.
- [LAYER_BUILDUP_SI]: REALIZED — the `LayerSet` buildup reads each `MaterialLayer.Thickness` (a seam `MeasureValue`) SI-native through `.Si` (metres, the seam SI base per `material#MATERIAL_COMPOSITION`) and lifts it to this page's mm coordinate by `* MetreToMm` — the prior `layer.Thickness.As("mm")` was a PHANTOM: `MeasureValue` exposes no `.As` method (that is the UnitsNet `Length` struct surface `.As(LengthUnit)`/`.As(UnitSystem)`, never the seam wrapper), so the cited member never existed and the prose comment doubled down on it. `StackLayers` then offsets the resolved coursing stream once per genuine ply along the normal and re-tags that ply's material (the primary structural ply at offset zero carrying the coursing), guarding `Plies.Count <= 1` so a single-ply or non-`LayerSet` run passes through untouched rather than re-multiplying a stream already carrying one material — the prior code re-tagged every coursing placement (already stamped `PrimaryMaterial`) for EVERY ply including the single-ply case, conflating the coursing decomposition with a forced per-ply re-tag. The per-ply `NormalOffsetMm` and `Placement.Material` are the realized buildup columns the host materializes ply-by-ply, the seam `MaterialComposition.LayerSet` the source.
- [HOST_MATERIALIZATION_SEAM]: the resolved `Layout` is portable scalar data; the host boundary at the app root turns each `Placement` station/elevation/path-angle tuple into a `Rhino.Geometry` transform — this owner never holds a host geometry type, the materialization is the consumer's concern at the host edge, keeping the construction model wire-portable. The per-unit `Placement.Material` and the `MaterialComposition` the run resolved are what the `Projection/material#MATERIAL_PROJECTOR` reads when lowering the layout into the seam `Material` subgraph and the element→material `Associate` edge, so the layout never authors a seam node itself — it produces the portable placement stream and the composition the projector lowers, the `[C7]` `MaterialUsage` occurrence binding (which the `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.UsageOf` derives) riding the `Associate` edge, never conflated onto the placement.
