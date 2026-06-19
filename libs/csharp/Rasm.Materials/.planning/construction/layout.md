# [MATERIALS_LAYOUT]

THE RESOLVED PLACEMENT STREAM and THE ONE LAYOUT FOLD. One `LayoutRun` is a parameterized run — a `profile#PROFILE_OWNER` `Profile`, a `masonry#PROFILE_FAMILY` `BondName`, an `assembly#ELEMENT_MODEL` `RunPath`, a height, a `JointPolicy`, and the element's `assembly#MATERIAL_ASSIGNMENT` — and one `Layout` is the resolved placement stream the run folds to through the single `ConstructionLayout.Resolve` fold. A layout is NEVER per-family code: the station/elevation/joint course fold generalizes to a `Profile` over any `ProfileFamily`, so a brick wall, a CMU wall, and a glulam frame are the SAME `Resolve` fold differing only in the `Profile` unit columns and the bond/course vocabulary. The fold is host-neutral — each course projects a station-stepped `Seq<Element>` of scalar `Placement` tuples through `StationStep` (the realized cursor/sequence/station projection: a per-unit pitch of `unitLength + headJoint`, a per-course `OffsetFraction` shift normalized into the run, the closing-cut bat at the run tail, and the `FootprintRun`/`FootprintRise` per-`Orientation` footprint) and the layout is the immutable `Fold` concatenation, never a mutable placement-list accumulation; the host boundary materializes the placement stream at the app root and the appearance engine shades through `appearance/graph#MATERIAL_LIBRARY`. The page composes `masonry#PROFILE_FAMILY` `BondName.Course` for the course template (a generated bond rails through that owner's `Fin`, never re-interpreted here), `assembly#ELEMENT_MODEL` `RunPathAlgebra` for the path length/angle, and `assembly#MATERIAL_ASSIGNMENT` for the layer-set/profile-set the run resolves; opening subtraction, corner closure, arch placement, pier solving, and the `LayerSet` cumulative-thickness `NormalOffsetMm` buildup are the realized fold stages, each a `ConstructionFault`-railed extension of the one `Resolve`.

## [1]-[INDEX]

One cluster: `[2]-[ASSEMBLY_FOLD]` owns the `LayoutRun` parameterized run, the `Layout` resolved placement stream, the station/elevation course fold, the `JointPolicy`, and the realized opening/corner/arch/pier/layer-buildup stages.

## [2]-[ASSEMBLY_FOLD]

- Owner: `LayoutRun` parameterized run; `Layout` resolved placement stream; `JointPolicy` the head/bed joint resolution; the station/elevation course fold.
- Cases: one `LayoutRun` shape — `Profile` + `BondName` + `RunPath` + height + `JointPolicy` + `MaterialAssignment` + `Seq<Opening>` + `Seq<Corner>` + `SpecialShape` + `Option<double>` pier width; the `LayoutStage` axis (straight-run · opening · corner · arch · pier) names the realized condition stages the `Courses` dispatch selects; one `Layout` fold producing the `Seq<Element>` placement stream.
- Entry: `public static Fin<Layout> Resolve(LayoutRun run, Op key)` — the host-neutral layout fold: validate the run, resolve joints, compute the course count, fold each course's template into a station-stepped `Seq<Element>` skipping opening-interrupted placements and closing corners, then stack the `LayerSet` plies by cumulative `NormalOffsetMm`; `Fin<T>` aborts on a generated bond (`ProfileFault.Bond` via `BondName.Course`), a degenerate path (`ConstructionFault.Path`), an unsupported arch voussoir (`ConstructionFault.Course`), or a non-positive joint (`ConstructionFault.Joint`).
- Packages: Rasm (project — scalar geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: opening subtraction, corner/closure conditions, arch placement (`ArchCourses`/`Voussoirs`), and pier solving (`PierClosure`) are each one realized fold stage the resolve composes; a new condition is one `LayoutStage` row binding one stage arm, a new joint rule one `JointPolicy` column; the family axis grows at `profile#PROFILE_OWNER`, so a CMU/timber/glazing run is the SAME `Resolve` fold over a different `Profile` — never a per-family layout method. A `LayerSet` buildup resolves through the same fold reading `assembly#MATERIAL_ASSIGNMENT` `LayerSet.TotalThickness` and the per-layer `NormalOffsetMm` cumulative offset, never a second layout owner.
- Law: `StationStep`/`StepCursor`/`Voussoirs`/`PierClosure` are the page's `[EXPRESSION_SPINE]` kernel exemptions — the `StepCursor` `yield` enumerator advances the station cursor by the fixed coursing pitch across the bounded once-per-course pass and the arch/pier stages run their bounded `Enumerable.Range` projection, these carrying the only statements on the page; the per-course `Map`/`Filter` projection, the `Courses`/`StepCourse`/`StackLayers` `Fin` fold, the `LayerOffset` cumulative-thickness `Fold`, and every other surface are expression-bodied, and the course/ply concatenation is the immutable `Seq` `Fold`/`Bind`, never a mutable placement-list accumulation.
- Boundary: `Resolve` is the ONE layout fold — a per-family `Layout` is the deleted form; it composes the `masonry#PROFILE_FAMILY` `BondName.Course` template (a generated bond rails through that owner's `Fin`, never re-interpreted here) and the `assembly#ELEMENT_MODEL` `RunPath` length/angle algebra; the course fold is immutable — each course projects a station-stepped `Seq<Element>` and the layout is the `Fold` concatenation, never a mutable placement-list accumulation; `JointPolicy` resolves head/bed joints once from the `Profile.Standard` coordinating thickness with an explicit override, so a joint literal never scatters; opening subtraction is the `Opening.Interrupts` station/elevation predicate the `StationStep` `Filter` skips so a window/door void drops the interrupted units; corner closure reads the `Corner.At` station match and substitutes the `ClosureRule.Closer` cut at the turn so a return wall closes with a queen/king closer rather than a degenerate overlap; arch placement is the `Voussoirs` station-normalized fold over a `RunPath.Arc` sweep placing each wedge as a `Soldier`-oriented unit at its arc station (gated to masonry/solid profiles, an unsupported voussoir railing `ConstructionFault.Course`); pier solving is the `PierClosure` alternating stretcher/header course fold over a pier width; the `LayerSet` buildup folds each ply at its cumulative `NormalOffsetMm` through `StackLayers`, re-tagging each ply's `MaterialId` so a wall buildup is a stacked placement stream the host materializes ply-by-ply; the resolved `Layout` is portable data (a `Seq<Element>` of scalar `Placement` tuples) the host boundary materializes and the appearance engine shades — every stage is a `ConstructionFault`-railed extension of the one fold, never a placeholder.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct JointPolicy(Option<double> HeadJointMm, Option<double> BedJointMm) {
    public static readonly JointPolicy Standard = new(Option<double>.None, Option<double>.None);
    public (double Head, double Bed) Resolve(Profile profile) =>
        (HeadJointMm.IfNone(profile.Standard.StandardJointThicknessMm), BedJointMm.IfNone(profile.Standard.StandardJointThicknessMm));
}

public readonly record struct Opening(double StationMm, double WidthMm, double SillElevationMm, double HeadElevationMm) {
    public bool Interrupts(double stationMm, double elevationMm) =>
        stationMm >= StationMm && stationMm < StationMm + WidthMm && elevationMm >= SillElevationMm && elevationMm < HeadElevationMm;
}

public readonly record struct Corner(double StationMm, double TurnDegrees, ClosureRule Closure) {
    public bool At(double stationMm, double pitchMm) => Math.Abs(stationMm - StationMm) < pitchMm * 0.5;
}

[SmartEnum<string>]
public sealed partial class LayoutStage {
    public static readonly LayoutStage StraightRun = new("straight-run");
    public static readonly LayoutStage Opening     = new("opening");
    public static readonly LayoutStage Corner      = new("corner");
    public static readonly LayoutStage Arch        = new("arch");
    public static readonly LayoutStage Pier        = new("pier");
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
    Option<double> PierWidthMm) {
    public static LayoutRun Of(Profile profile, BondName bond, RunPath path, double heightMm, JointPolicy joints, MaterialAssignment assignment) =>
        new(profile, bond, path, heightMm, joints, assignment, Seq<Opening>(), Seq<Corner>(), SpecialShape.None, Option<double>.None);
}

public sealed record Layout(LayoutRun Run, double LengthMm, int CourseCount, double HeadJointMm, double BedJointMm, Seq<Element> Elements, double TotalThicknessMm);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConstructionLayout {
    private const double JoinToleranceMm = 1e-6;

    public static Fin<Layout> Resolve(LayoutRun run, Op key) =>
        from length in RunPathAlgebra.LengthOf(run.Path, key)
        from archGuard in guard(run.Path is not RunPath.Arc || run.Profile.Coring == Coring.None || run.Profile.Family == ProfileFamily.Masonry, ConstructionFault.Course(key, "<arch-voussoir-unsupported>"))
        let joints = run.Joints.Resolve(run.Profile)
        from validHead in guard(double.IsFinite(joints.Head) && joints.Head > 0.0, ConstructionFault.Joint(key, $"<head-joint:{joints.Head}>"))
        from validBed in guard(double.IsFinite(joints.Bed) && joints.Bed > 0.0, ConstructionFault.Joint(key, $"<bed-joint:{joints.Bed}>"))
        let courseCount = Math.Max(1, (int)Math.Ceiling(run.HeightMm / run.Profile.Unit.CourseHeightMm.Value))
        from elements in Courses(run, length, courseCount, joints.Head, key)
        let buildup = LayerOffset(run.Assignment)
        from layered in StackLayers(run, elements, buildup, key)
        select new Layout(run, length, courseCount, joints.Head, joints.Bed, layered, buildup.TotalMm);

    static Fin<Seq<Element>> Courses(LayoutRun run, double lengthMm, int courseCount, double headJointMm, Op key) =>
        (run.Special, run.PierWidthMm) switch {
            (_, { IsSome: true } pier) => Fin.Succ(PierClosure(run, courseCount, pier.IfNone(run.Profile.Unit.LengthMm.Value))),
            ({ } s, _) when s == SpecialShape.Voussoir && run.Path is RunPath.Arc =>
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
                Cut cut = CornerOrClosingCut(run, step.StationMm, unitLengthMm, headJointMm, lengthMm, pitchMm);
                double runMm = unitLengthMm * cut.LengthFraction * FootprintRun(orientation);
                double riseMm = courseHeightMm * FootprintRise(orientation);
                return new Element(
                    run.Profile,
                    new Placement(
                        course,
                        step.Sequence,
                        step.StationMm,
                        elevationMm,
                        runMm,
                        riseMm,
                        RunPathAlgebra.AngleAt(run.Path, step.StationMm),
                        orientation,
                        cut),
                    run.Assignment);
            });
    }

    // --- [OPENING_CORNER_STAGE]
    static Cut CornerOrClosingCut(LayoutRun run, double stationMm, double unitLengthMm, double headJointMm, double lengthMm, double pitchMm) =>
        run.Corners.Find(c => c.At(stationMm, pitchMm)).Match(
            Some: corner => corner.Closure.Closer,
            None: () => ClosingCut(stationMm, unitLengthMm, headJointMm, lengthMm));

    // --- [ARCH_STAGE]
    static Fin<Seq<Element>> ArchCourses(LayoutRun run, int voussoirCount, Op key) =>
        run.Path is RunPath.Arc arc
            ? guard(voussoirCount >= 3, ConstructionFault.Course(key, $"<arch-voussoir-count:{voussoirCount}>"))
                .Map(_ => Voussoirs(run, arc, voussoirCount))
            : Fin.Fail<Seq<Element>>(ConstructionFault.Path(key, "<arch-requires-arc-path>"));

    static Seq<Element> Voussoirs(LayoutRun run, RunPath.Arc arc, int count) {
        double unitLengthMm = run.Profile.Unit.LengthMm.Value;
        double sweepStep = arc.SweepDegrees / count;
        double stationStep = arc.RadiusMm * (Math.PI / 180.0) * Math.Abs(sweepStep);
        return toSeq(Enumerable.Range(0, count)).Map(i => {
            double stationMm = (i + 0.5) * stationStep;
            return new Element(
                run.Profile,
                new Placement(0, i, stationMm, 0.0, unitLengthMm, run.Profile.Unit.HeightMm.Value, RunPathAlgebra.AngleAt(run.Path, stationMm), new Orientation.Soldier(), Cut.Whole),
                run.Assignment);
        });
    }

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
                    Assignment = MaterialAssignment.ProfileSet(ply.Material, e.Profile).IfFail(run.Assignment) })));

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

## [3]-[RESEARCH]

- [LAYOUT_STAGE_DEPTH_FILL]: REALIZED — opening subtraction (the `Opening.Interrupts` station/elevation predicate the `StationStep` `Filter` skips), corner closure (the `Corner.At` station match substituting the `ClosureRule.Closer` cut), arch placement (the `Voussoirs` station-normalized fold over `RunPath.Arc`), pier solving (the `PierClosure` alternating-course fold), and the `LayerSet` cumulative-thickness `NormalOffsetMm` per-ply offset (`LayerOffset`/`StackLayers`) are realized fold stages of the one `Resolve`, each a `ConstructionFault`-railed extension over the immutable projected `Seq<Element>`, never a parallel placement method. The remaining calibration is the per-region edge-cut-request detailing depth and the multi-leaf corner bond reconciliation, column growth on `Opening`/`Corner`, not a re-architecture.
- [ARCH_PLACEMENT_CONSTRAINT]: REALIZED for the masonry/solid voussoir case — the `Voussoirs` fold turns a `RunPath.Arc` sweep into a station-stepped `Seq<Element>` of `Soldier`-oriented wedge units at each arc station via `RunPathAlgebra.AngleAt`, gated to masonry/solid profiles (a cored/hollow or non-masonry arch rails `ConstructionFault.Course`). The remaining probe is the source-backed voussoir taper geometry (the BS/TN wedge-angle detailing the host materializes from the per-unit `PathAngleDegrees`) and the keystone special-shape, a `SpecialShape.Voussoir` column the placement carries, not a parallel placement owner.
- [HOST_MATERIALIZATION_SEAM]: the resolved `Layout` is portable scalar data; the host boundary at the app root turns each `Placement` station/elevation/path-angle tuple into a `Rhino.Geometry` transform — this owner never holds a host geometry type, the materialization is the consumer's concern at the host edge, keeping the construction model wire-portable.
