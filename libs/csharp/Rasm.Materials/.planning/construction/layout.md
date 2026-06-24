# [MATERIALS_LAYOUT]

THE RESOLVED PLACEMENT STREAM and THE ONE LAYOUT FOLD. One `LayoutRun` is a parameterized run — a `profile#PROFILE_OWNER` `Profile`, a `masonry#PROFILE_FAMILY` `BondName`, an `assembly#ELEMENT_MODEL` `RunPath`, a height, a `JointPolicy`, and the element's `assembly#MATERIAL_ASSIGNMENT` — and one `Layout` is the resolved placement stream the run folds to through the single `ConstructionLayout.Resolve` fold. A layout is NEVER per-family code: the station/elevation/joint course fold generalizes to a `Profile` over any `ProfileFamily`, so a brick wall, a CMU wall, and a glulam frame are the SAME `Resolve` fold differing only in the `Profile` unit columns and the bond/course vocabulary. The fold is host-neutral — each course projects a station-stepped `Seq<Element>` of scalar `Placement` tuples through `StationStep` (the realized cursor/sequence/station projection: a per-unit pitch of `unitLength + headJoint`, a per-course `OffsetFraction` shift normalized into the run, the closing-cut bat at the run tail, and the `FootprintRun`/`FootprintRise` per-`Orientation` footprint) and the layout is the immutable `Fold` concatenation, never a mutable placement-list accumulation; the host boundary materializes the placement stream at the app root and the appearance engine shades through `Appearance/graph#MATERIAL_LIBRARY`. The page composes `masonry#PROFILE_FAMILY` `BondName.Course` for the course template (a generated bond computes its course and per-unit rotation through that owner's `GeneratedBond.Interpret`, never re-interpreted here — the `CourseTemplate.PerUnitRotationDegrees` folds into the `Placement.PathAngleDegrees`), `assembly#ELEMENT_MODEL` `RunPathAlgebra` for the path length/angle, and `assembly#MATERIAL_ASSIGNMENT` for the layer-set/profile-set the run resolves; opening subtraction, corner closure, arch placement, pier solving, and the `LayerSet` cumulative-thickness `NormalOffsetMm` buildup are the realized fold stages, each a `ConstructionFault`-railed extension of the one `Resolve`.

## [01]-[INDEX]

- [01]-[ASSEMBLY_FOLD]: the `LayoutRun` parameterized run, the `Layout` resolved placement stream, the station/elevation course fold, the `JointPolicy`, and the realized opening/corner/arch/pier/layer-buildup stages.

## [02]-[ASSEMBLY_FOLD]

- Owner: `LayoutRun` parameterized run; `Layout` resolved placement stream; `JointPolicy` the head/bed/profile/mortar joint resolution reading the `masonry#PROFILE_FAMILY` `MortarJoint` specification; the station/elevation course fold.
- Cases: one `LayoutRun` shape — `Profile` + `BondName` + `RunPath` + height + `JointPolicy` + `MaterialAssignment` + `Seq<Opening>` + `Seq<Corner>` + `SpecialShape` + `ArchProfile` + `Option<double>` pier width; the `LayoutStage` axis (straight-run · opening · corner · arch · dome · pier) names the realized condition stages the `Courses` dispatch selects; one `Layout` fold producing the `Seq<Element>` placement stream over a line/arc/dome path.
- Entry: `public static Fin<Layout> Resolve(LayoutRun run, Op key)` — the host-neutral layout fold: validate the run, resolve joints, compute the course count, fold each course's template into a station-stepped `Seq<Element>` skipping opening-interrupted placements and closing corners, then stack the `LayerSet` plies by cumulative `NormalOffsetMm`; `Fin<T>` aborts on a generated bond (`ProfileFault.Bond` via `BondName.Course`), a degenerate path (`ConstructionFault.Path`), an unsupported arch voussoir (`ConstructionFault.Course`), or a non-positive joint (`ConstructionFault.Joint`).
- Packages: Rasm (project — scalar geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: opening subtraction, corner/closure conditions, arch placement (`ArchCourses`/`Voussoirs` over an `ArchProfile`), dome placement (`DomeRings` over a `RunPath.Dome`), and pier solving (`PierClosure`) are each one realized fold stage the resolve composes; a new arch profile is one `ArchProfile` row, a new condition is one `LayoutStage` row binding one stage arm, a new joint rule one `JointPolicy`/`MortarJoint` column, a new opening-jamb detailing one `EdgeCut` row, a new corner multi-leaf reconciliation one `Corner.Leaves` value — column/row growth on the existing condition records, never a re-architecture; the family axis grows at `profile#PROFILE_OWNER`, so a CMU/timber/glazing run is the SAME `Resolve` fold over a different `Profile` — never a per-family layout method. A `LayerSet` buildup resolves through the same fold reading `assembly#MATERIAL_ASSIGNMENT` `LayerSet.TotalThickness` and the per-layer `NormalOffsetMm` cumulative offset, never a second layout owner.
- Law: `StationStep`/`StepCursor`/`Voussoirs`/`DomeRings`/`PierClosure` are the page's `[EXPRESSION_SPINE]` kernel exemptions — the `StepCursor` `yield` enumerator advances the station cursor by the fixed coursing pitch across the bounded once-per-course pass and the arch/dome/pier stages run their bounded `Enumerable.Range` projection (the dome a nested ring×unit projection), these carrying the only statements on the page; the per-course `Map`/`Filter` projection, the `Courses`/`StepCourse`/`StackLayers` `Fin` fold, the `LayerOffset` cumulative-thickness `Fold`, and every other surface are expression-bodied, and the course/ply concatenation is the immutable `Seq` `Fold`/`Bind`, never a mutable placement-list accumulation.
- Boundary: `Resolve` is the ONE layout fold — a per-family `Layout` is the deleted form; it composes the `masonry#PROFILE_FAMILY` `BondName.Course` template (a generated bond computes its course through that owner's `GeneratedBond.Interpret`, the `CourseTemplate.PerUnitRotationDegrees` summed into the `Placement.PathAngleDegrees` by `StationStep`, never re-interpreted here) and the `assembly#ELEMENT_MODEL` `RunPath` length/angle algebra; the course fold is immutable — each course projects a station-stepped `Seq<Element>` and the layout is the `Fold` concatenation, never a mutable placement-list accumulation; `JointPolicy` resolves the full `masonry#PROFILE_FAMILY` `MortarJoint` once — the head/bed width, the ASTM C270 tooled `MortarProfile`, and the `MortarType` strength — from a specified joint or the `Profile.Standard` coordinating-thickness fallback (`MortarJoint.Standard`) with an explicit head/bed override, so a joint literal never scatters and the resolved `Layout.Joint` carries the buildable profile the `weathering#WEATHERING` raked-joint shadow line and the thermal/structural seam read, the `StationStep` pitch reading the `HeadWidthMm` head joint unchanged; opening subtraction is the `Opening.Interrupts` station/elevation predicate the `StationStep` `Filter` skips so a window/door void drops the interrupted units, and the `Opening.JambCut` `EdgeCut` resolves the per-course jamb detailing (`Toothed` alternating whole/three-quarter for a bonded return, `Straight` a flush half-bat, `Quoined` an alternating whole/half quoin) through `JambDetailCut` so the reveal courses read the requested edge detail rather than a flush cut; corner closure reads the `Corner.At` station match and `Corner.Reconcile(course)` substitutes the `ClosureRule.Closer` cut for a single-leaf return or the multi-leaf course-alternating closer for a `Leaves > 1` bonded corner so a return wall closes with a queen/king closer or a reconciled multi-leaf bond rather than a degenerate overlap, the `CornerOrClosingCut` resolving corner-reconcile before opening-jamb before the closing bat in one order; arch placement is the `Voussoirs` station-normalized fold over a `RunPath.Arc` sweep driven by the `ArchProfile` (segmental/semicircular/three-centre/ogee/equilateral/lancet springing-to-crown sweep), placing each wedge as a `Soldier`-oriented unit radially tilted at its arc station with the centre voussoir the keystone (a `Cut.KingCloser` wider-taper special-cut at the crown), gated to masonry/solid profiles; dome placement is the `DomeRings` fold lifting `Voussoirs` to a surface of revolution over a `RunPath.Dome` — a stack of horizontal ring courses springing→crown, each ring a full revolution of units whose count falls with the ring radius `R·cos(latitude)`, every ring radially tilted to its meridian angle and the crown ring the compression keystone, an under-counted dome railing `ConstructionFault.Course`; pier solving is the `PierClosure` alternating stretcher/header course fold over a pier width; the `LayerSet` buildup folds each ply at its cumulative `NormalOffsetMm` through `StackLayers`, re-tagging each ply's `MaterialId` so a wall buildup is a stacked placement stream the host materializes ply-by-ply; the resolved `Layout` is portable data (a `Seq<Element>` of scalar `Placement` tuples) the host boundary materializes and the appearance engine shades — every stage is a `ConstructionFault`-railed extension of the one fold, never a placeholder.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The joint resolution reads a full masonry#PROFILE_FAMILY MortarJoint (head/bed width + ASTM C270 profile + mortar
// strength) when one is specified, falling back to the coordinating ProfileStandard.StandardJointThicknessMm scalar.
public readonly record struct JointPolicy(Option<MortarJoint> Mortar, Option<double> HeadJointMm, Option<double> BedJointMm) {
    public static readonly JointPolicy Standard = new(Option<MortarJoint>.None, Option<double>.None, Option<double>.None);
    public static JointPolicy Of(MortarJoint mortar) => new(Some(mortar), Some(mortar.HeadWidthMm), Some(mortar.BedWidthMm));

    public MortarJoint Resolve(Profile profile) =>
        Mortar.IfNone(() => MortarJoint.Standard(profile.Standard.StandardJointThicknessMm)) is var joint
            ? joint with { HeadWidthMm = HeadJointMm.IfNone(joint.HeadWidthMm), BedWidthMm = BedJointMm.IfNone(joint.BedWidthMm) }
            : joint;
}

public readonly record struct Opening(double StationMm, double WidthMm, double SillElevationMm, double HeadElevationMm, EdgeCut JambCut) {
    public static Opening Of(double stationMm, double widthMm, double sillMm, double headMm) =>
        new(stationMm, widthMm, sillMm, headMm, EdgeCut.Toothed);

    public bool Interrupts(double stationMm, double elevationMm) =>
        stationMm >= StationMm && stationMm < StationMm + WidthMm && elevationMm >= SillElevationMm && elevationMm < HeadElevationMm;

    public bool OnJamb(double stationMm, double pitchMm) =>
        Math.Abs(stationMm - StationMm) < pitchMm * 0.5 || Math.Abs(stationMm - (StationMm + WidthMm)) < pitchMm * 0.5;

    public Cut JambDetailCut(int course) => JambCut.Resolve(course);
}

[SmartEnum<string>]
public sealed partial class EdgeCut {
    public static readonly EdgeCut Toothed = new("toothed", static course => (course & 1) == 0 ? Cut.Whole : Cut.ThreeQuarter);
    public static readonly EdgeCut Straight = new("straight", static _ => Cut.Half);
    public static readonly EdgeCut Quoined  = new("quoined", static course => (course & 1) == 0 ? Cut.Whole : Cut.Half);

    [UseDelegateFromConstructor]
    public partial Cut Resolve(int course);
}

public readonly record struct Corner(double StationMm, double TurnDegrees, ClosureRule Closure, int Leaves = 1) {
    public bool At(double stationMm, double pitchMm) => Math.Abs(stationMm - StationMm) < pitchMm * 0.5;

    public Cut Reconcile(int course) =>
        Leaves <= 1
            ? Closure.Closer
            : (course % Math.Max(2, Leaves)) == 0 ? Closure.Closer : Cut.Half;
}

[SmartEnum<string>]
public sealed partial class LayoutStage {
    public static readonly LayoutStage StraightRun = new("straight-run");
    public static readonly LayoutStage Opening     = new("opening");
    public static readonly LayoutStage Corner      = new("corner");
    public static readonly LayoutStage Arch        = new("arch");
    public static readonly LayoutStage Dome        = new("dome");
    public static readonly LayoutStage Pier        = new("pier");
}

// The professional masonry-arch profile driving the voussoir radial pitch and springing/crown geometry over a
// RunPath.Arc sweep: RiseRatio is the rise/span the springing angle derives from, CentreCount the curve-centre count.
[SmartEnum<string>]
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
    // The springing-to-crown sweep the voussoir pitch divides; a semicircle is 180°, a segmental arch less.
    public double SweepDegrees => 2.0 * Math.Atan(2.0 * RiseRatio) * 180.0 / Math.PI;
}

public sealed record LayoutRun(
    Profile Profile,
    BondName Bond,
    RunPath Path,
    double HeightMm,
    JointPolicy Joints,
    MaterialAssignment Assignment,
    Seq<Opening> Openings,
    Seq<Corner> Corners,
    SpecialShape Special,
    ArchProfile Arch,
    Option<double> PierWidthMm) {
    public static LayoutRun Of(Profile profile, BondName bond, RunPath path, double heightMm, JointPolicy joints, MaterialAssignment assignment) =>
        new(profile, bond, path, heightMm, joints, assignment, Seq<Opening>(), Seq<Corner>(), SpecialShape.None, ArchProfile.Semicircular, Option<double>.None);
}

public sealed record Layout(LayoutRun Run, double LengthMm, int CourseCount, MortarJoint Joint, Seq<Element> Elements, double TotalThicknessMm) {
    public double HeadJointMm => Joint.HeadWidthMm;
    public double BedJointMm => Joint.BedWidthMm;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConstructionLayout {
    private const double JoinToleranceMm = 1e-6;

    public static Fin<Layout> Resolve(LayoutRun run, Op key) =>
        from length in RunPathAlgebra.LengthOf(run.Path, key)
        from archGuard in guard(run.Path is not RunPath.Arc || run.Profile.Coring == Coring.None || run.Profile.Family == ProfileFamily.Masonry, ConstructionFault.Course(key, "<arch-voussoir-unsupported>"))
        let joint = run.Joints.Resolve(run.Profile)
        from validHead in guard(double.IsFinite(joint.HeadWidthMm) && joint.HeadWidthMm > 0.0, ConstructionFault.Joint(key, $"<head-joint:{joint.HeadWidthMm}>"))
        from validBed in guard(double.IsFinite(joint.BedWidthMm) && joint.BedWidthMm > 0.0, ConstructionFault.Joint(key, $"<bed-joint:{joint.BedWidthMm}>"))
        let courseCount = Math.Max(1, (int)Math.Ceiling(run.HeightMm / run.Profile.Unit.CourseHeightMm.Value))
        from elements in Courses(run, length, courseCount, joint.HeadWidthMm, key)
        let buildup = LayerOffset(run.Assignment)
        from layered in StackLayers(run, elements, buildup, key)
        select new Layout(run, length, courseCount, joint, layered, buildup.TotalMm);

    static Fin<Seq<Element>> Courses(LayoutRun run, double lengthMm, int courseCount, double headJointMm, Op key) =>
        (run.Special, run.Path, run.PierWidthMm) switch {
            (_, _, { IsSome: true } pier) => Fin.Succ(PierClosure(run, courseCount, pier.IfNone(run.Profile.Unit.LengthMm.Value))),
            (_, RunPath.Dome dome, _) => DomeRings(run, dome, key),
            ({ } s, RunPath.Arc, _) when s == SpecialShape.Voussoir =>
                ArchCourses(run, Math.Max(3, (int)Math.Round(lengthMm / Math.Max(JoinToleranceMm, run.Profile.Unit.LengthMm.Value + headJointMm))), key),
            _ => toSeq(Enumerable.Range(0, courseCount)).Fold(
                Fin.Succ(Seq<Element>()),
                (acc, course) => acc.Bind(elements =>
                    run.Bond.Course(course, key)
                        .Bind(template => StepCourse(run, template, course, lengthMm, headJointMm, key))
                        .Map(courseElements => elements + courseElements))),
        };

    static Fin<Seq<Element>> StepCourse(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm, Op key) =>
        guard(!template.Sequence.IsEmpty, ConstructionFault.Course(key, $"<empty-course:{course}>"))
            .Map(_ => StationStep(run, template, course, lengthMm, headJointMm));

    static Seq<Element> StationStep(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm) {
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        double courseHeightMm = run.Profile.Unit.CourseHeightMm.Value;
        double pitchMm = Math.Max(JoinToleranceMm, unitLengthMm + headJointMm);
        double elevationMm = course * courseHeightMm;
        double offsetStationMm = template.OffsetFraction * pitchMm;
        int span = Math.Max(1, template.Sequence.Count);
        return toSeq(StepCursor(offsetStationMm, pitchMm, lengthMm))
            .Filter(step => !run.Openings.Exists(o => o.Interrupts(step.StationMm + unitLengthMm * 0.5, elevationMm + courseHeightMm * 0.5)))
            .Map(step => {
                Orientation orientation = template.Sequence[step.Sequence % span];
                Cut cut = CornerOrClosingCut(run, step.StationMm, course, unitLengthMm, headJointMm, lengthMm, pitchMm);
                double runMm = unitLengthMm * cut.LengthFraction * FootprintRun(orientation);
                double riseMm = courseHeightMm * FootprintRise(orientation);
                // A generated bond (herringbone/diaper/pinwheel) emits per-unit rotation into the path angle; a
                // template bond's PerUnitRotationDegrees is 0, so the path-following angle survives unchanged.
                double angleDeg = RunPathAlgebra.AngleAt(run.Path, step.StationMm) + template.PerUnitRotationDegrees;
                return new Element(
                    run.Profile,
                    new Placement(
                        course,
                        step.Sequence,
                        step.StationMm,
                        elevationMm,
                        runMm,
                        riseMm,
                        angleDeg,
                        orientation,
                        cut),
                    run.Assignment);
            });
    }

    // --- [OPENING_CORNER_STAGE]
    static Cut CornerOrClosingCut(LayoutRun run, double stationMm, int course, double unitLengthMm, double headJointMm, double lengthMm, double pitchMm) =>
        run.Corners.Find(c => c.At(stationMm, pitchMm)).Match(
            Some: corner => corner.Reconcile(course),
            None: () => run.Openings.Find(o => o.OnJamb(stationMm + unitLengthMm * 0.5, pitchMm)).Match(
                Some: opening => opening.JambDetailCut(course),
                None: () => ClosingCut(stationMm, unitLengthMm, headJointMm, lengthMm)));

    // --- [ARCH_STAGE]
    static Fin<Seq<Element>> ArchCourses(LayoutRun run, int voussoirCount, Op key) =>
        run.Path is RunPath.Arc arc
            ? guard(voussoirCount >= 3, ConstructionFault.Course(key, $"<arch-voussoir-count:{voussoirCount}>"))
                .Map(_ => Voussoirs(run, arc, voussoirCount))
            : Fin.Fail<Seq<Element>>(ConstructionFault.Path(key, "<arch-requires-arc-path>"));

    // Voussoirs over an ArchProfile: the springing→crown sweep is the profile's, each wedge a Soldier-oriented unit
    // at its arc station radially tilted, the centre voussoir the keystone (a Bevel special-cut wider taper at the crown).
    static Seq<Element> Voussoirs(LayoutRun run, RunPath.Arc arc, int count) {
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        double sweep = run.Arch.SweepDegrees > 0.0 ? run.Arch.SweepDegrees : arc.SweepDegrees;
        double sweepStep = sweep / count;
        double stationStep = arc.RadiusMm * (Math.PI / 180.0) * Math.Abs(sweepStep);
        int keystone = count / 2;
        return toSeq(Enumerable.Range(0, count)).Map(i => {
            double stationMm = (i + 0.5) * stationStep;
            bool isKeystone = i == keystone;
            return new Element(
                run.Profile,
                new Placement(0, i, stationMm, 0.0, isKeystone ? unitLengthMm * Cut.KingCloser.LengthFraction : unitLengthMm, run.Profile.Unit.HeightMm.Value,
                    RunPathAlgebra.AngleAt(run.Path, stationMm) - sweep * 0.5, new Orientation.Soldier(), isKeystone ? Cut.KingCloser : Cut.Whole),
                run.Assignment);
        });
    }

    // --- [DOME_STAGE]
    // A dome resolves as a stack of horizontal ring courses springing→crown, each ring a full revolution of
    // voussoir units whose count falls with the cosine of the latitude (the ring radius), every ring radially tilted
    // to its meridian angle and the crown ring the compression-ring keystone — the Voussoirs fold lifted to a surface.
    static Fin<Seq<Element>> DomeRings(LayoutRun run, RunPath.Dome dome, Op key) =>
        guard(dome.RingCourses >= 2 && (run.Profile.Coring == Coring.None || run.Profile.Family == ProfileFamily.Masonry), ConstructionFault.Course(key, $"<dome-ring-courses-or-coring:{dome.RingCourses}>"))
            .Map(_ => {
                double unitLengthMm = run.Profile.Unit.LengthMm.Value;
                double courseHeightMm = run.Profile.Unit.CourseHeightMm.Value;
                return toSeq(Enumerable.Range(0, dome.RingCourses)).Bind(ring => {
                    double latitude = (ring + 0.5) / dome.RingCourses * (Math.PI * 0.5);   // springing(0) → crown(π/2)
                    double ringRadiusMm = dome.RadiusMm * Math.Cos(latitude);
                    double elevationMm = dome.RadiusMm * Math.Sin(latitude);
                    int unitsThisRing = Math.Max(1, (int)Math.Round(2.0 * Math.PI * ringRadiusMm / Math.Max(JoinToleranceMm, unitLengthMm)));
                    bool crown = ring == dome.RingCourses - 1;
                    return toSeq(Enumerable.Range(0, unitsThisRing)).Map(u => new Element(
                        run.Profile,
                        new Placement(ring, u, u * (ringRadiusMm > 0.0 ? 2.0 * Math.PI * ringRadiusMm / unitsThisRing : unitLengthMm), elevationMm,
                            unitLengthMm, courseHeightMm, latitude * 180.0 / Math.PI, new Orientation.Soldier(), crown ? Cut.KingCloser : Cut.Whole),
                        run.Assignment));
                });
            });

    // --- [PIER_STAGE]
    static Seq<Element> PierClosure(LayoutRun run, int courseCount, double pierWidthMm) {
        double courseHeightMm = run.Profile.Unit.CourseHeightMm.Value;
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        int unitsPerCourse = Math.Max(1, (int)Math.Round(pierWidthMm / unitLengthMm));
        return toSeq(Enumerable.Range(0, courseCount)).Bind(course =>
            toSeq(Enumerable.Range(0, unitsPerCourse)).Map(u => {
                Orientation orientation = (course & 1) == 0 ? new Orientation.Stretcher() : new Orientation.Header();
                return new Element(
                    run.Profile,
                    new Placement(course, u, u * unitLengthMm, course * courseHeightMm, unitLengthMm * FootprintRun(orientation), courseHeightMm, 0.0, orientation, Cut.Whole),
                    run.Assignment);
            }));
    }

    // --- [LAYER_BUILDUP_STAGE]
    readonly record struct LayerBuildup(double TotalMm, Seq<(MaterialId Material, double OffsetMm, double ThicknessMm)> Plies);

    static LayerBuildup LayerOffset(MaterialAssignment assignment) =>
        assignment is MaterialAssignment.LayerSet set
            ? set.Layers.Fold(new LayerBuildup(0.0, Seq<(MaterialId, double, double)>()),
                static (acc, layer) => acc with {
                    TotalMm = acc.TotalMm + layer.ThicknessMm.Value,
                    Plies = acc.Plies.Add((layer.Material, acc.TotalMm, layer.ThicknessMm.Value)) })
            : new LayerBuildup(0.0, Seq<(MaterialId, double, double)>());

    static Fin<Seq<Element>> StackLayers(LayoutRun run, Seq<Element> elements, LayerBuildup buildup, Op key) =>
        buildup.Plies.IsEmpty
            ? Fin.Succ(elements)
            : Fin.Succ(buildup.Plies.Bind(ply =>
                elements.Map(e => e with {
                    Placement = e.Placement with { NormalOffsetMm = ply.OffsetMm },
                    Assignment = MaterialAssignment.ProfileSet(ply.Material, e.Profile, key).IfFail(run.Assignment) })));

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

    static double FootprintRun(Orientation orientation) => orientation.Switch(
        stretcher: static _ => 1.0,
        header:    static _ => 0.5,
        soldier:   static _ => 1.0,
        sailor:    static _ => 1.0,
        rowlock:   static _ => 0.5,
        shiner:    static _ => 1.0);

    static double FootprintRise(Orientation orientation) => orientation.Switch(
        stretcher: static _ => 1.0,
        header:    static _ => 1.0,
        soldier:   static _ => 3.0,
        sailor:    static _ => 3.0,
        rowlock:   static _ => 2.0,
        shiner:    static _ => 2.0);
}
```

## [03]-[RESEARCH]

- [LAYOUT_STAGE_DEPTH_FILL]: REALIZED — opening subtraction (the `Opening.Interrupts` station/elevation predicate the `StationStep` `Filter` skips), corner closure (the `Corner.At` station match), arch placement (the `Voussoirs` station-normalized fold over `RunPath.Arc`), pier solving (the `PierClosure` alternating-course fold), and the `LayerSet` cumulative-thickness `NormalOffsetMm` per-ply offset (`LayerOffset`/`StackLayers`) are realized fold stages of the one `Resolve`, each a `ConstructionFault`-railed extension over the immutable projected `Seq<Element>`, never a parallel placement method. The per-region edge-cut detailing and the multi-leaf corner-bond reconciliation are REALIZED as column growth on the condition records, not a re-architecture: the `Opening.JambCut` `EdgeCut` `[SmartEnum]` (toothed/straight/quoined) drives the per-course jamb cut through `JambDetailCut` so an opening reveal reads a toothed-and-quoined return rather than a flush straight cut, and the `Corner.Leaves` count drives the multi-leaf bond reconciliation through `Reconcile` so a two-leaf corner alternates the closer course by course, both threading the SAME immutable `StepCursor` and re-folding the projected `Seq<Element>` through the one `CornerOrClosingCut` resolution order (corner reconcile, then opening jamb, then closing bat) — no mutable accumulation, no second fold.
- [ARCH_PLACEMENT_CONSTRAINT]: REALIZED — the arch stage is the full professional masonry-arch vocabulary as a parametric `ArchProfile` axis (segmental/semicircular/three-centre/ogee/equilateral/lancet, each carrying a `RiseRatio` the springing-to-crown `SweepDegrees` derives from and a `CentreCount`), the `Voussoirs` fold turning a `RunPath.Arc` sweep into a station-stepped `Seq<Element>` of `Soldier`-oriented wedge units radially tilted at each arc station via `RunPathAlgebra.AngleAt` with the centre voussoir the keystone (a `Cut.KingCloser` wider-taper special-cut), gated to masonry/solid profiles. The dome is realized as the `RunPath.Dome` case and the `DomeRings` fold lifting `Voussoirs` to a surface of revolution — a stack of horizontal ring courses springing→crown, each ring `R·cos(latitude)` in radius with its unit count falling toward the crown, every ring tilted to its meridian angle and the crown ring the compression keystone, the per-ring voussoir taper riding the existing `Placement` columns the host materializes from the per-unit `PathAngleDegrees`. A segmental/ogee/three-centre arch and a masonry dome place through the one `Resolve` fold as a parametric `ArchProfile`/`RunPath` axis, the keystone and per-voussoir taper reading the existing `Placement` columns, never a parallel arch/dome placement owner.
- [MORTAR_JOINT_RESOLUTION]: REALIZED — `JointPolicy` resolves the `masonry#PROFILE_FAMILY` `MortarJoint` (head/bed width + ASTM C270 `MortarProfile` + `MortarType` strength) rather than a single scalar joint thickness, the `Layout.Joint` carrying the full buildable specification. A specified `JointPolicy.Of(mortarJoint)` reads the joint's head/bed/profile/mortar; an unspecified run falls back to `MortarJoint.Standard(profile.Standard.StandardJointThicknessMm)` (concave/Type-N coordinating default) so the scalar route survives; an explicit `HeadJointMm`/`BedJointMm` overrides the width while keeping the profile. The `StationStep` pitch reads the resolved `HeadWidthMm` exactly as it read the scalar head joint, so the placement fold is unchanged; the `MortarProfile.ShadowLine`/`RecessDepthMm` is the source the `weathering#WEATHERING` raked-joint cavity AO reads and the `MortarType.CompressiveMpa` the strength the thermal/structural seam reads, both riding the resolved `Layout.Joint` not a parallel joint surface.
- [HOST_MATERIALIZATION_SEAM]: the resolved `Layout` is portable scalar data; the host boundary at the app root turns each `Placement` station/elevation/path-angle tuple into a `Rhino.Geometry` transform — this owner never holds a host geometry type, the materialization is the consumer's concern at the host edge, keeping the construction model wire-portable.
