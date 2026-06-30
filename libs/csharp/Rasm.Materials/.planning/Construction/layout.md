# [MATERIALS_LAYOUT]

THE RESOLVED PLACEMENT STREAM and THE ONE LAYOUT FOLD. One `LayoutRun` is a parameterized run — a `Component/component#COMPONENT_OWNER` `Component` and its resolved `ComponentUnit` dimensional substrate, a `Component/masonry#MASONRY_FAMILY` `BondName`, an `assembly#PLACEMENT_MODEL` `RunPath`, a height, a `JointPolicy`, and the element's `assembly#MATERIAL_COMPOSITION` seam `MaterialComposition` — and one `Layout` is the resolved `Seq<Placement>` stream the run folds to through the single `ConstructionLayout.Resolve` fold. A layout is NEVER per-family code: the station/elevation/joint course fold generalizes to a `Component` over any coursed `ComponentFamily`, so a brick wall and a CMU wall are the SAME `Resolve` fold differing only in the `ComponentUnit` dimensional columns and the bond/course vocabulary — the caller resolves the canonical `ComponentUnit` from the component's family section ONCE (`Component/masonry#MASONRY_FAMILY` `MasonryUnit.ToUnit()` total, `Component/cmu#CMU_FAMILY` `CmuSection.ToUnit` `Fin`-railed) and hands the run the dimensional substrate, so the fold reads the unit dims off `run.Unit` and the family/coring/standard off `run.Component` without re-projecting a section. The fold is host-neutral — each course projects a station-stepped `Seq<Placement>` of scalar tuples through `StationStep` (the realized cursor/sequence/station projection: a per-unit pitch of `unitLength + headJoint`, a per-course `CourseOffsetFraction` shift normalized into the run, the closing-cut bat at the run tail, the per-`Orientation` `RunFraction`/`RiseFraction` footprint read off the `Component/masonry#MASONRY_FAMILY` `Orientation` row, the per-unit `UnitPlacement` rotation/lateral/along transforms a generated bond emits, and the run material tagged on each placement) and the layout is the immutable `Fold` concatenation, never a mutable placement-list accumulation; the host boundary materializes the placement stream at the app root and the appearance engine shades through the seam `Material`/`Appearance` nodes the `Projection/component#COMPONENT_PROJECTOR` authors. The stage axis is the `LayoutStage` `[SmartEnum<string>]` — each row carries the predicate that elects it (`Elect`), so `Resolve` elects the stage by `LayoutStage.Of(run)` (the first row whose predicate matches, in declared order) and `Courses` dispatches the elected row to its `Fin`-railed placement kernel, never an ad-hoc tuple `switch`: straight-coursing, an arch ring, a dome surface, and a pier are FOUR rows of one closed family, a new condition one row binding one predicate and one kernel. The page composes `Component/masonry#MASONRY_FAMILY` `BondName.Course` for the course template (a generated bond computes its full per-unit course through that bond's own `BondGeometry.Course` delegate, never re-interpreted here — the `UnitPlacement.RotationDegrees` folds into the `Placement.PathAngleDegrees`, the `LateralFraction` into the `Placement.LateralOffsetMm`, the `AlongFraction` into the placed station), `assembly#PLACEMENT_MODEL` `RunPathAlgebra` for the path length/angle, the `Component/masonry#MASONRY_FAMILY` `Cut`/`Orientation`/`ClosureRule` vocabulary, and the seam `assembly#MATERIAL_COMPOSITION` `MaterialComposition` for the layer-set/profile-set the run resolves; opening subtraction, corner closure, multi-centre arch placement, dome placement, pier solving, and the `LayerSet` cumulative-thickness `NormalOffsetMm` buildup are the realized fold stages, each a `ConstructionFault`-railed extension of the one `Resolve`.

## [01]-[INDEX]

- [01]-[ASSEMBLY_FOLD]: the `LayoutRun` parameterized run, the `Layout` resolved `Seq<Placement>` stream with its derived `Keystones` voussoir-centre projection, the `LayoutStage` predicate-plus-fold stage family, the `JointPolicy`, the multi-centre `ArchProfile` sweep, and the realized opening/corner/arch/dome/pier/layer-buildup stages.

## [02]-[ASSEMBLY_FOLD]

- Owner: `LayoutRun` parameterized run; `Layout` resolved `Seq<Placement>` stream carrying the derived `Keystones` voussoir-centre projection; `JointPolicy` the head/bed/profile/mortar joint resolution reading the `Component/masonry#MASONRY_FAMILY` `MortarJoint` specification; `LayoutStage` the closed predicate-plus-fold stage family the `Courses` dispatch elects; `ArchProfile`/`OpeningHead`/`EdgeCut` the per-condition vocabulary rows; the station/elevation course fold.
- Cases: one `LayoutRun` shape — `Component` + resolved `ComponentUnit` + `BondName` + `RunPath` + height + `JointPolicy` + seam `MaterialComposition` + `Seq<Opening>` + `Seq<Corner>` + `SpecialShape` + `ArchProfile` + `Option<double>` pier width; the `LayoutStage` `[SmartEnum<string>]` (coursing · arch · dome · pier) carries the realized condition stages as ROWS, each row owning the predicate that elects it (`Elect`), the elected row dispatched in `Courses` to its `Fin`-railed placement kernel; one `Layout` fold producing the `Seq<Placement>` stream over a line/arc/dome path.
- Entry: `public static Fin<Layout> Resolve(LayoutRun run, Op key)` — the host-neutral layout fold: validate the run, resolve joints, compute the course count, elect the `LayoutStage` and fold its own `Resolve` arm into a station-stepped `Seq<Placement>` (the coursing row skips opening-interrupted placements and closes corners; the arch/dome/pier rows run their bounded projection), then stack the seam `MaterialComposition.LayerSet` plies by cumulative `NormalOffsetMm`; `Fin<T>` aborts on a generated bond (`ComponentFault.Bond` via `BondName.Course`), a degenerate path (`ConstructionFault.Path`), an unsupported arch/dome on a cored non-masonry profile (`ConstructionFault.Course`), or a non-positive joint (`ConstructionFault.Joint`).
- Packages: Rasm (project — scalar geometry, the `Op` op-key), Rasm.Element (project — `MaterialComposition`/`MaterialId` the run resolves and each placement tags), Rasm.Materials.Component (`Component`/`ComponentUnit`/`ComponentFamily`/`Coring` the parent `COMPONENT_OWNER` + the `Component/masonry#MASONRY_FAMILY` `BondName`/`Cut`/`Orientation`/`ClosureRule`/`MortarJoint`/`CourseTemplate`/`UnitPlacement`/`SpecialShape` vocabulary sub-namespace), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: opening subtraction, corner/closure conditions, arch placement (`Voussoirs` over a multi-centre `ArchProfile`), dome placement (`DomeRings` lifting `Voussoirs` over a `RunPath.Dome`), and pier solving (`Pier`) are each ONE `LayoutStage` row binding one `Fin`-railed fold the dispatch elects; a new arch profile is one `ArchProfile` row, a new condition is one `LayoutStage` row, a new joint rule one `JointPolicy`/`MortarJoint` column, a new opening-jamb detailing one `EdgeCut` row, a new opening head one `OpeningHead` row, a new corner multi-leaf reconciliation one `Corner.Leaves` value — row/column growth on the existing condition records, never a re-architecture; the family axis grows at `Component/component#COMPONENT_OWNER`, so a CMU run is the SAME `Resolve` fold over a different `Component`/`ComponentUnit` — never a per-family layout method. A `LayerSet` buildup resolves through the same fold reading the seam `assembly#MATERIAL_COMPOSITION` `MaterialComposition.LayerSet` and the per-ply `NormalOffsetMm` cumulative offset, never a second layout owner.
- Law: `StepCursor`/`StationStep`/`Voussoirs`/`DomeRings`/`Pier` are the page's `[EXPRESSION_SPINE]` kernel exemptions — the `StepCursor` `yield` enumerator advances the station cursor by the fixed coursing pitch across the bounded once-per-course pass, `StationStep` projects one course's cursor stream into placements (locals + one projecting `Map`), and the arch/dome/pier folds run their bounded `Enumerable.Range` projection (the dome a nested ring×unit projection, lifting `Voussoirs` per ring), these carrying the statements on the page; the per-course `Map`/`Filter` projection, the `Courses`/`StepCourse`/`StackLayers` `Fin` fold, the `LayerOffset` cumulative-thickness `Fold`, the `LayoutStage`/`JointPolicy`/`Opening`/`Corner` dispatch, and every other surface are expression-bodied, and the course/ply concatenation is the immutable `Seq` `Fold`/`Bind`, never a mutable placement-list accumulation.
- Boundary: `Resolve` is the ONE layout fold, never a per-family `Layout`; it composes the `Component/masonry#MASONRY_FAMILY` `BondName.Course` template (a generated bond computes its course through that bond's own `BondGeometry.Course` delegate, the per-unit `UnitPlacement.RotationDegrees` summed into the `Placement.PathAngleDegrees` and the `LateralFraction` lifted to `Placement.LateralOffsetMm` by `StationStep`, never re-interpreted here) and the `assembly#PLACEMENT_MODEL` `RunPath` length/angle algebra; the course fold is immutable — each course projects a station-stepped `Seq<Placement>` and the layout is the `Fold` concatenation, never a mutable placement-list accumulation; the stage selection is the `LayoutStage` `[SmartEnum]` carrying each condition's `Elect` predicate as a ROW, `Courses` dispatching the elected row to its `Fin`-railed placement kernel, so a pier on any path, an arch on an arc, and a dome on a dome path each elect their row and a new stage is one row binding one predicate and one kernel; `JointPolicy` resolves the full `Component/masonry#MASONRY_FAMILY` `MortarJoint` once — the head/bed width, the ASTM C270 tooled `MortarProfile`, and the `MortarType` strength — from a specified joint or the `Component.Standard.StandardJointThicknessMm` coordinating-thickness fallback (`MortarJoint.Standard`) with an explicit head/bed override, so a joint literal never scatters and the resolved `Layout.Joint` carries the buildable profile the `weathering#WEATHERING` raked-joint shadow line and the thermal/structural seam read, the `StationStep` pitch reading the `HeadWidthMm` head joint unchanged; opening subtraction is the `Opening.Interrupts` station/elevation predicate the `StationStep` `Filter` skips so a window/door void drops the interrupted units, the `Opening.JambDetail` `EdgeCut` resolving the per-course jamb detailing (`Toothed` alternating whole/three-quarter for a bonded return, `Straight` a flush half-bat, `Quoined` an alternating whole/half quoin) through `JambDetailCut` so the reveal courses read the requested edge detail, and the `Opening.Head` `OpeningHead` ROW resolving the over-opening condition (a `Lintel` flush-cut course, a `SoldierCourse` rotated soldier band, an `ArchOver` springing voussoir set) so a door/window head reads its real detail rather than a flush coursing run straight over the void; corner closure reads the `Corner.At` station match and `Corner.Reconcile(course)` substitutes the `ClosureRule.Closer` cut for a single-leaf return or the multi-leaf course-alternating closer (reading the course parity against the corner's own `Leaves` count) for a `Leaves > 1` bonded corner so a return wall closes with a queen/king closer or a reconciled multi-leaf bond rather than a degenerate overlap, the `CornerOrClosingCut` resolving corner-reconcile before opening-jamb before the closing bat in one order; arch placement is the `Voussoirs` station-normalized fold over the `ArchProfile` MULTI-CENTRE sweep (segmental/semicircular each one circular arc; three-centre/equilateral/lancet a piecewise multi-radius sweep over `CentreCount` arc segments; ogee a reversed-curvature S-sweep over the `Ogee` flag), placing each wedge as a `Bevel`-cut `Soldier`-oriented unit radially tilted at its arc station with the centre voussoir the keystone (the central wedge of the radial taper, realized by the `Layout.Keystones` derived projection — the `Bevel`-`Soldier` wedges grouped by `Course`, the `Sequence == n/2` centre per ring — never a narrower `Cut`), gated to masonry/solid profiles; dome placement is the `DomeRings` fold LIFTING `Voussoirs` to a surface of revolution over a `RunPath.Dome` — a stack of horizontal ring courses springing→crown, each ring a full revolution resolved through the SAME `Voussoirs` projection at its latitude radius `R·cos(latitude)`, every ring radially tilted to its meridian angle and the crown ring the compression keystone, an under-counted dome railing `ConstructionFault.Course`; pier solving is the `Pier` alternating stretcher/header course fold over a pier width; the `LayerSet` buildup folds each genuine ply at its cumulative `NormalOffsetMm` through `StackLayers`, offsetting the resolved coursing stream once per ply and re-tagging that ply's `Placement.Material` so a wall buildup is a stacked placement stream the host materializes ply-by-ply (the primary structural ply at offset zero carries the coursing, the secondary plies offset along the normal), never re-multiplying a stream already tagged with one material; the resolved `Layout` is portable data (a `Seq<Placement>` of scalar tuples) the host boundary materializes and the appearance engine shades — every stage is a `ConstructionFault`-railed extension of the one fold, never a placeholder.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                       // Op, guard
using Rasm.Element;                      // MaterialId, MaterialComposition (Single|LayerSet|…) + MaterialLayer (the run's seam composition the buildup reads)
using Rasm.Materials.Component;          // Component, ComponentFamily, ComponentUnit, Coring (the parent COMPONENT_OWNER + the relocated void class)
using Rasm.Materials.Component.Masonry;  // BondName, Cut, Orientation, ClosureRule, MortarJoint, CourseTemplate, UnitPlacement, SpecialShape (the Component/masonry#MASONRY_FAMILY vocabulary sub-namespace)
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
// The joint resolution reads a full Component/masonry#MASONRY_FAMILY MortarJoint (head/bed width + ASTM C270 profile +
// mortar strength) when one is specified, falling back to the coordinating ComponentStandard.StandardJointThicknessMm scalar.
public readonly record struct JointPolicy(Option<MortarJoint> Mortar, Option<double> HeadJointMm, Option<double> BedJointMm) {
    public static readonly JointPolicy Standard = new(Option<MortarJoint>.None, Option<double>.None, Option<double>.None);
    public static JointPolicy Of(MortarJoint mortar) => new(Some(mortar), Some(mortar.HeadWidthMm), Some(mortar.BedWidthMm));

    // The resolved joint: the specified MortarJoint or the coordinating Concave/Type-N default from the component's
    // standard thickness, then the explicit head/bed override (each falling back to the resolved joint's own width).
    // No `is var` fake-ternary — the override is a direct `with` on the base joint.
    public MortarJoint Resolve(Component component) {
        MortarJoint joint = Mortar.IfNone(() => MortarJoint.Standard(component.Standard.StandardJointThicknessMm));
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
// pier-width run elects Pier; a dome path elects Dome; a voussoir special-shape on an arc path elects Arch. The row IS
// the discriminant (POLICY_VALUES). Election order is the static Items order: Pier, Dome, Arch, Coursing.
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

// The run carries the Component (its family/coring/standard) AND the resolved ComponentUnit (its width/height/length/
// course-height dimensional substrate the caller projected ONCE from the component's family section — masonry
// MasonryUnit.ToUnit() total, cmu CmuSection.ToUnit Fin), so the fold reads dims off Unit and family/coring/standard
// off Component without re-projecting a section. Composition is the seam MaterialComposition the run resolves; Special
// elects the arch stage; PierWidthMm elects the pier stage; Arch drives the voussoir sweep.
public sealed record LayoutRun(
    Component Component,
    ComponentUnit Unit,
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
    public static LayoutRun Of(Component component, ComponentUnit unit, BondName bond, RunPath path, double heightMm, JointPolicy joints, MaterialComposition composition) =>
        new(component, unit, bond, path, heightMm, joints, composition, Seq<Opening>(), Seq<Corner>(), SpecialShape.None, ArchProfile.Semicircular, Option<double>.None);
}

public sealed record Layout(LayoutRun Run, double LengthMm, int CourseCount, MortarJoint Joint, Seq<Placement> Placements, double TotalThicknessMm) {
    // The keystone of each voussoir course/ring — the centre wedge a host materializes as the locking unit and a
    // structural readout treats as the crown. It is a DERIVED projection over the resolved stream (DERIVED_LOGIC,
    // GoverningRadiusMm/PrimaryMaterial shape), NOT a stored flag on the host-neutral Placement: every voussoir is a
    // Cut.Bevel Soldier wedge (ArchCourses/DomeRings emit no other), so the bevel-soldier wedges grouped by (Course,
    // NormalOffsetMm) are exactly one arch ring (Course 0) or one dome ring (Course k) on one buildup ply, and the
    // keystone is the wedge at Sequence == n/2 within its group (the odd voussoir count ArchCourses' `| 1` lift
    // guarantees one exact centre; a dome ring's even count rounds toward the lower-indexed centre). The NormalOffsetMm
    // co-key is LOAD-BEARING: StackLayers replicates the wedge stream once per LayerSet ply (re-tagging NormalOffsetMm +
    // Material, Cut/Orientation intact), so a multi-ply masonry arch carries one keystone PER PLY — grouping by Course
    // alone would fold every ply's wedges into one count*plies group and Skip(count/2) would miss the geometric centre.
    // For the single-ply (offset-zero) run every wedge shares NormalOffsetMm 0, so the group collapses to one ring and
    // the one-keystone-per-ring behaviour is exact. The per-group Sequence-ascending order survives StackLayers' in-order
    // per-ply Map, so Skip lands on the centre. A non-voussoir coursing run has no Cut.Bevel placements, so the projection
    // is empty — the keystone is realized HERE, not asserted as an undeclared downstream grouping.
    public Seq<Placement> Keystones =>
        Placements.Filter(static p => p.Cut == Cut.Bevel && p.Orientation == Orientation.Soldier)
            .GroupBy(static p => (p.Course, p.NormalOffsetMm))
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
        from archGuard in guard(run.Path is not (RunPath.Arc or RunPath.Dome) || run.Component.Coring == Coring.None || run.Component.Family == ComponentFamily.Masonry,
            ConstructionFault.Course(key, $"<arch-or-dome-unsupported-on-cored:{run.Component.Coring.Key}>"))
        let joint = run.Joints.Resolve(run.Component)
        from validHead in guard(double.IsFinite(joint.HeadWidthMm) && joint.HeadWidthMm > 0.0, ConstructionFault.Joint(key, $"<head-joint:{joint.HeadWidthMm}>"))
        from validBed in guard(double.IsFinite(joint.BedWidthMm) && joint.BedWidthMm > 0.0, ConstructionFault.Joint(key, $"<bed-joint:{joint.BedWidthMm}>"))
        let courseCount = Math.Max(1, (int)Math.Ceiling(run.HeightMm / run.Unit.CourseHeightMm.Value))
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
            pier:     _ => Fin.Succ(Pier(run, courseCount, run.PierWidthMm.IfNone(run.Unit.LengthMm.Value))),
            dome:     _ => run.Path is RunPath.Dome dome
                ? DomeRings(run, dome, key)
                : Fin.Fail<Seq<Placement>>(ConstructionFault.Course(key, $"<dome-stage-off-dome-path:{run.Path.GetType().Name}>")),
            // The keystone is the centre voussoir (Voussoirs takes count / 2), so an even count shifts the crown
            // off-centre; `| 1` lifts the rounded count to the next odd before the >= 3 floor and the arch fold.
            arch:     _ => run.Path is RunPath.Arc arc
                ? ArchCourses(run, arc, Math.Max(3, (int)Math.Round(lengthMm / Math.Max(JoinToleranceMm, run.Unit.LengthMm.Value + headJointMm)) | 1), key)
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
        guard(!template.Units.IsEmpty, ConstructionFault.Course(key, $"<empty-course:{course}>"))
            .Map(_ => StationStep(run, template, course, lengthMm, headJointMm));

    // One course's cursor projection: the StepCursor advances a fixed coursing pitch across the run; each step reads its
    // per-unit UnitPlacement cell (the new CourseTemplate.Units shape — orientation + along-fraction + lateral-fraction +
    // rotation) by the wrapped sequence index. The Orientation drives the run/rise footprint, the RotationDegrees folds
    // into Placement.PathAngleDegrees, the LateralFraction into Placement.LateralOffsetMm (the across-course in-plan offset
    // a woven/basketweave/pinwheel bond emits), and the AlongFraction shifts the placed station beyond the consecutive
    // slot — a template bond's cell carries one orientation with zero offsets/rotation so a running/english course is
    // unperturbed. The opening/corner/closing detection stays on the regular slot station (step.StationMm); the cell's
    // along-offset shifts only the rendered placement.
    static Seq<Placement> StationStep(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm) {
        double unitLengthMm = run.Unit.LengthMm.Value;
        double courseHeightMm = run.Unit.CourseHeightMm.Value;
        double pitchMm = Math.Max(JoinToleranceMm, unitLengthMm + headJointMm);
        double elevationMm = course * courseHeightMm;
        double offsetStationMm = template.CourseOffsetFraction * pitchMm;
        int span = Math.Max(1, template.Units.Count);
        MaterialId material = run.Composition.PrimaryMaterial;
        return toSeq(StepCursor(offsetStationMm, pitchMm, lengthMm))
            .Filter(step => !Interrupted(run, step.StationMm + unitLengthMm * 0.5, elevationMm, courseHeightMm))
            .Map(step => {
                UnitPlacement cell = template.Units[step.Sequence % span];
                double centreStationMm = step.StationMm + unitLengthMm * 0.5;
                Orientation orientation = cell.Orientation;
                Cut cut = CornerOrClosingCut(run, step.StationMm, course, unitLengthMm, headJointMm, lengthMm, pitchMm);
                // The over-opening head condition (lintel/soldier/arch-over) overrides the coursing cut+orientation
                // for a unit on a head course spanning the void; elsewhere the coursing cut+cell orientation hold.
                (cut, orientation) = HeadDetail(run, centreStationMm, elevationMm, courseHeightMm, course, cut, orientation);
                double runMm = unitLengthMm * cut.LengthFraction * orientation.RunFraction;
                double riseMm = courseHeightMm * orientation.RiseFraction;
                // A generated bond (herringbone/diaper/pinwheel) emits per-unit rotation/lateral/along transforms through
                // the UnitPlacement columns; a template bond's cell carries 0 rotation and 0 offsets, so the path-following
                // angle and the slot station survive unchanged.
                double angleDeg = RunPathAlgebra.AngleAt(run.Path, step.StationMm) + cell.RotationDegrees;
                double stationMm = step.StationMm + cell.AlongFraction * pitchMm;
                double lateralMm = cell.LateralFraction * unitLengthMm;
                return new Placement(course, step.Sequence, stationMm, elevationMm, runMm, riseMm, angleDeg, orientation, cut, material, LateralOffsetMm: lateralMm);
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
        double unitLengthMm = run.Unit.LengthMm.Value;
        double unitHeightMm = run.Unit.HeightMm.Value;
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
    // meridian angle and the crown ring the compression-ring keystone — the SAME Voussoirs fold lifted per ring, the
    // ring index threading Placement.Course.
    static Fin<Seq<Placement>> DomeRings(LayoutRun run, RunPath.Dome dome, Op key) =>
        guard(dome.RingCourses >= 2 && (run.Component.Coring == Coring.None || run.Component.Family == ComponentFamily.Masonry),
            ConstructionFault.Course(key, $"<dome-ring-courses-or-coring:{dome.RingCourses}>"))
            .Map(_ => toSeq(Enumerable.Range(0, dome.RingCourses)).Bind(ring => {
                double latitude = (ring + 0.5) / dome.RingCourses * (Math.PI * 0.5);   // springing(0) → crown(π/2)
                double ringRadiusMm = dome.RadiusMm * Math.Cos(latitude);
                double elevationMm = dome.RadiusMm * Math.Sin(latitude);
                double unitLengthMm = run.Unit.LengthMm.Value;
                int unitsThisRing = Math.Max(1, (int)Math.Round(2.0 * Math.PI * ringRadiusMm / Math.Max(JoinToleranceMm, unitLengthMm)));
                return Voussoirs(run, ringRadiusMm, elevationMm, unitsThisRing, ring);
            }));

    // --- [PIER_STAGE]
    static Seq<Placement> Pier(LayoutRun run, int courseCount, double pierWidthMm) {
        double courseHeightMm = run.Unit.CourseHeightMm.Value;
        double unitLengthMm = run.Unit.LengthMm.Value;
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
    // (the seam SI base is metres, assembly#MATERIAL_COMPOSITION boundary) and lifted to this page's mm coordinate by
    // *MetreToMm, cumulatively offset along the normal. A non-LayerSet composition has no buildup (single/profile/
    // constituent resolve to one stream at offset zero), so the Plies are empty and StackLayers passes through.
    static LayerBuildup LayerOffset(MaterialComposition composition) =>
        composition is MaterialComposition.LayerSet set
            ? set.Layers.Fold(new LayerBuildup(0.0, Seq<(MaterialId, double, double)>()),
                static (acc, layer) => {
                    // The ply offset is the running total BEFORE this ply's thickness (ply 0 at 0, ply 1 at t0, …); a
                    // block lambda names the lifted-to-mm thickness ONCE — never an `is var ? … : acc` dead-armed
                    // fake-ternary (an `is var` pattern always matches, so the alternate arm is unreachable).
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

- [LAYOUT_STAGE_DISPATCH]: the stage selection is the `LayoutStage` `[SmartEnum<string>]` carrying each condition as a ROW with its own `Elect(run)` predicate (`[UseDelegateFromConstructor]`); `LayoutStage.Of(run)` elects the first row whose predicate matches in declared order (Pier, Dome, Arch, then the always-true Coursing default) and the `Courses` dispatch routes the elected row through the SmartEnum's own generated `Switch(pier:, dome:, arch:, coursing:)` to its `Fin`-railed placement kernel (`StraightCoursing`/`ArchCourses`/`DomeRings`/`Pier`, the statement-kernels the `[EXPRESSION_SPINE]` exemption names, so they stay named methods rather than row-delegate fields). The row IS the discriminant (POLICY_VALUES + SYMBOLIC_REFERENCE: the generated `Switch` arm-name is the case symbol, never a restated `.Key` string literal), a new stage one row binding one `Elect` predicate and one dispatched kernel, never a decorative enum. The dispatch stays total over the closed family (the `dome`/`arch` arms narrow `run.Path` to the case election already proved present, the election-impossible path railing `ConstructionFault.Course`; the Coursing predicate is `static _ => true`), so a pier on an arc path resolves as a pier rather than mis-staging, the `Resolve` arch/dome guard having already gated a cored non-masonry profile.
- [MULTI_CENTRE_ARCH]: the `ArchProfile` `[SmartEnum]` rows (segmental/semicircular/three-centre/ogee/equilateral/lancet) are SIX real geometries the `Voussoirs` fold consumes: `CentreCount` divides the sweep over equal-angle segments (a three-centre/equilateral/lancet pointed arch a piecewise multi-radius union approximating the gothic curve) and `Ogee` reverses curvature at the inflection (the S-sweep `TiltDegreesAt` winds one way then the other), so the per-station meridian tilt `ArchProfile.TiltDegreesAt(sweepFraction)` reads the profile's real turning rather than a bare single-arc angle. Each voussoir is a `Cut.Bevel` radial wedge (the `Component/masonry#MASONRY_FAMILY` cut for a tapered unit), the keystone the CENTRE wedge realized by the `Layout.Keystones` derived projection (the `Bevel`-`Soldier` wedges grouped by `Course`, the `Sequence == n/2` centre per ring, the odd voussoir count the arch `| 1` lift guarantees) — a keystone is geometrically the widest crown wedge, never a narrower `Cut`; the radial taper rides the existing `Placement.PathAngleDegrees`/`Orientation.Soldier` columns the host materializes.
- [DOME_LIFTS_VOUSSOIRS]: `DomeRings` LIFTS the SAME `Voussoirs` projection per ring, so a dome is a stack of horizontal ring courses springing→crown each resolved through `Voussoirs(run, R·cos(latitude), R·sin(latitude), unitsThisRing, ring)`, the ring index threading `Placement.Course` so the stacked rings stay course-distinct, every ring radially tilted to its meridian and the crown ring the compression keystone, an under-counted dome (`RingCourses < 2`) or a cored non-masonry profile railing `ConstructionFault.Course`. The arch and the dome share ONE voussoir owner — a segmental arch and a hemispherical dome place through the SAME fold, the dome adding only the per-ring latitude/radius reduction, never a parallel dome placement owner.
- [OPENING_HEAD_AND_JAMB]: an `Opening` carries BOTH the jamb detailing (`EdgeCut` toothed/straight/quoined through `JambDetailCut`) AND the over-opening head condition (`OpeningHead` `[SmartEnum]` lintel/soldier-course/arch-over through `Resolve(course, incomingCut)`), so a door/window head reads its real masonry detail rather than a flush coursing run straight over the void — a `Lintel` keeps the coursing cut on the head course, a `SoldierCourse` rotates the head band to soldiers, an `ArchOver` wedges the head springing. The `Interrupted` predicate drops a unit inside the void BELOW the head course (the sill-to-(head-1) band) but KEEPS the head course (it carries the head detail), so the head condition is a stage on the existing `StationStep` projection — one `OpeningHead` row and one `OnHead` predicate, never a parallel opening-head method.
- [MORTAR_JOINT_RESOLUTION]: `JointPolicy` resolves the `Component/masonry#MASONRY_FAMILY` `MortarJoint` (head/bed width + ASTM C270 `MortarProfile` + `MortarType` strength) rather than a single scalar joint thickness, the `Layout.Joint` carrying the full buildable specification. A specified `JointPolicy.Of(mortarJoint)` reads the joint's head/bed/profile/mortar; an unspecified run falls back to `MortarJoint.Standard(component.Standard.StandardJointThicknessMm)` (concave/Type-N coordinating default) so the scalar route survives; an explicit `HeadJointMm`/`BedJointMm` overrides the width while keeping the profile through a direct `with`. The `StationStep` pitch reads the resolved `HeadWidthMm` exactly as a scalar head joint, so the placement fold is unchanged; the `MortarProfile` recess and `MortarJoint.RecessDepthMm` are the source the `weathering#WEATHERING` raked-joint cavity AO reads and the `MortarType` strength the thermal/structural seam reads, both riding the resolved `Layout.Joint` not a parallel joint surface.
- [PER_UNIT_COURSE_TRANSFORM]: the `StationStep` reads the `Component/masonry#MASONRY_FAMILY` `CourseTemplate.Units` per-unit `UnitPlacement` cells (`Orientation` + `AlongFraction` + `LateralFraction` + `RotationDegrees`) and the `CourseOffsetFraction` course shift, folding each cell into a host-neutral `Placement`: the `Orientation` drives the `RunFraction`/`RiseFraction` footprint, the `RotationDegrees` folds into `Placement.PathAngleDegrees`, the `LateralFraction` into `Placement.LateralOffsetMm` (the across-course in-plan offset a woven/basketweave/pinwheel bond emits, the `assembly#PLACEMENT_MODEL` `Placement` column the buildup carries), and the `AlongFraction` shifts the placed station beyond the consecutive slot. A template bond (running/english/stack/flemish) carries one orientation per cell with zero offsets/rotation so a structural course is unperturbed and the opening/corner/closing detection on the regular slot station is exact; a generated bond (herringbone/diaper/pinwheel/basketweave) fills the per-unit transform through its `BondGeometry.Course` delegate, so the host-neutral `Placement` carries the full packing transform without a host matrix and the layout never re-interprets a bond's geometry.
- [LAYER_BUILDUP_SI]: the `LayerSet` buildup reads each `MaterialLayer.Thickness` (a seam `MeasureValue`) SI-native through `.Si` (metres, the seam SI base per `assembly#MATERIAL_COMPOSITION`) and lifts it to this page's mm coordinate by `* MetreToMm` — `MeasureValue` exposes no `.As(LengthUnit)` member (that is the UnitsNet `Length` struct surface, never the seam wrapper). `StackLayers` then offsets the resolved coursing stream once per genuine ply along the normal and re-tags that ply's material (the primary structural ply at offset zero carrying the coursing), guarding `Plies.Count <= 1` so a single-ply or non-`LayerSet` run passes through untouched rather than re-multiplying a stream already carrying one material. The per-ply `NormalOffsetMm` and `Placement.Material` are the realized buildup columns the host materializes ply-by-ply, the seam `MaterialComposition.LayerSet` the source.
- [HOST_MATERIALIZATION_SEAM]: the resolved `Layout` is portable scalar data; the host boundary at the app root turns each `Placement` station/elevation/path-angle/lateral-offset/normal-offset tuple into a `Rhino.Geometry` transform — this owner never holds a host geometry type, the materialization the consumer's concern at the host edge, keeping the construction model wire-portable. The per-unit `Placement.Material` and the `MaterialComposition` the run resolved are what the `Projection/component#COMPONENT_PROJECTOR` reads when lowering the layout into the seam `Material` subgraph and the element→material `Associate` edge, so the layout never authors a seam node itself — it produces the portable placement stream and the composition the projector lowers, the `[C7]` `MaterialUsage` occurrence binding (which the `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.UsageOf` derives) riding the `Associate` edge, never conflated onto the placement.
