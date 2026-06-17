# [MATERIALS_LAYOUT]

THE RESOLVED PLACEMENT STREAM and THE ONE LAYOUT FOLD. One `LayoutRun` is a parameterized run — a `profile#PROFILE_OWNER` `Profile`, a `masonry#PROFILE_FAMILY` `BondName`, an `assembly#ELEMENT_MODEL` `RunPath`, a height, a `JointPolicy`, and the element's `assembly#MATERIAL_ASSIGNMENT` — and one `Layout` is the resolved placement stream the run folds to through the single `ConstructionLayout.Resolve` fold. A layout is NEVER per-family code: the station/elevation/joint course fold generalizes to a `Profile` over any `ProfileFamily`, so a brick wall, a CMU wall, and a glulam frame are the SAME `Resolve` fold differing only in the `Profile` unit columns and the bond/course vocabulary. The fold is host-neutral — each course projects a station-stepped `Seq<Element>` of scalar `Placement` tuples and the layout is the immutable `Fold` concatenation, never a mutable placement-list accumulation; the host boundary materializes the placement stream at the app root and the appearance engine shades through `appearance/graph#MATERIAL_LIBRARY`. The page composes `masonry#PROFILE_FAMILY` `BondName.Course` for the course template (a generated bond rails through that owner's `Fin`, never re-interpreted here), `assembly#ELEMENT_MODEL` `RunPathAlgebra` for the path length/angle, and `assembly#MATERIAL_ASSIGNMENT` for the layer-set/profile-set the run resolves; opening subtraction, corner closure, arch placement, and pier solving are the queued fold stages, each a `ConstructionFault`-railed extension.

## [1]-[INDEX]

One cluster: `[2]-[ASSEMBLY_FOLD]` owns the `LayoutRun` parameterized run, the `Layout` resolved placement stream, the station/elevation course fold, the `JointPolicy`, and the queued opening/corner/arch/pier stages.

## [2]-[ASSEMBLY_FOLD]

- Owner: `LayoutRun` parameterized run; `Layout` resolved placement stream; `JointPolicy` the head/bed joint resolution; the station/elevation course fold.
- Cases: one `LayoutRun` shape — `Profile` + `BondName` + `RunPath` + height + `JointPolicy` + `MaterialAssignment`; one `Layout` fold producing the `Seq<Element>` placement stream.
- Entry: `public static Fin<Layout> Resolve(LayoutRun run, Op key)` — the host-neutral layout fold: validate the run, resolve joints, compute the course count, fold each course's template into a station-stepped `Seq<Element>`; `Fin<T>` aborts on a generated bond (`ProfileFault.Bond` via `BondName.Course`), a degenerate path (`ConstructionFault.Path`), or a non-positive joint (`ConstructionFault.Joint`).
- Packages: Rasm (project — scalar geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: opening subtraction, corner/closure conditions, arch placement, and pier solving are each one fold stage the resolve composes (the queued probes); a new joint rule is one `JointPolicy` column; the family axis grows at `profile#PROFILE_OWNER`, so a CMU/timber/glazing run is the SAME `Resolve` fold over a different `Profile` — never a per-family layout method. A `LayerSet` buildup resolves through the same fold reading `assembly#MATERIAL_ASSIGNMENT` `LayerSet.TotalThickness` and the per-layer offset, never a second layout owner.
- Boundary: `Resolve` is the ONE layout fold — a per-family `Layout` is the deleted form; it composes the `masonry#PROFILE_FAMILY` `BondName.Course` template (a generated bond rails through that owner's `Fin`, never re-interpreted here) and the `assembly#ELEMENT_MODEL` `RunPath` length/angle algebra; the course fold is immutable — each course projects a station-stepped `Seq<Element>` and the layout is the `Fold` concatenation, never a mutable placement-list accumulation; `JointPolicy` resolves head/bed joints once from the `Profile.Standard` coordinating thickness with an explicit override, so a joint literal never scatters; the resolved `Layout` is portable data (a `Seq<Element>` of scalar `Placement` tuples) the host boundary materializes and the appearance engine shades — opening subtraction, corner closure, arch placement, and pier solving are the queued fold stages, each a `ConstructionFault`-railed extension, never a placeholder.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct JointPolicy(Option<double> HeadJointMm, Option<double> BedJointMm) {
    public static readonly JointPolicy Standard = new(Option<double>.None, Option<double>.None);
    public (double Head, double Bed) Resolve(Profile profile) =>
        (HeadJointMm.IfNone(profile.Standard.StandardJointThicknessMm), BedJointMm.IfNone(profile.Standard.StandardJointThicknessMm));
}

public sealed record LayoutRun(Profile Profile, BondName Bond, RunPath Path, double HeightMm, JointPolicy Joints, MaterialAssignment Assignment);

public sealed record Layout(LayoutRun Run, double LengthMm, int CourseCount, double HeadJointMm, double BedJointMm, Seq<Element> Elements);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConstructionLayout {
    public static Fin<Layout> Resolve(LayoutRun run, Op key) =>
        from length in RunPathAlgebra.LengthOf(run.Path, key)
        let joints = run.Joints.Resolve(run.Profile)
        from validHead in guard(double.IsFinite(joints.Head) && joints.Head > 0.0, ConstructionFault.Joint(key, $"<head-joint:{joints.Head}>"))
        from validBed in guard(double.IsFinite(joints.Bed) && joints.Bed > 0.0, ConstructionFault.Joint(key, $"<bed-joint:{joints.Bed}>"))
        let courseCount = Math.Max(1, (int)Math.Ceiling(run.HeightMm / run.Profile.Unit.CourseHeightMm.Value))
        from elements in Courses(run, length, courseCount, joints.Head, key)
        select new Layout(run, length, courseCount, joints.Head, joints.Bed, elements);

    static Fin<Seq<Element>> Courses(LayoutRun run, double lengthMm, int courseCount, double headJointMm, Op key) =>
        toSeq(Enumerable.Range(0, courseCount)).Fold(
            Fin.Succ(Seq<Element>()),
            (acc, course) => acc.Bind(elements =>
                run.Bond.Course(course, key)
                    .Bind(template => StepCourse(run, template, course, lengthMm, headJointMm, key))
                    .Map(courseElements => elements + courseElements)));

    static Fin<Seq<Element>> StepCourse(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm, Op key) =>
        guard(!template.Sequence.IsEmpty, ConstructionFault.Course(key, $"<empty-course:{course}>"))
            .Map(_ => StationStep(run, template, course, lengthMm, headJointMm));

    static Seq<Element> StationStep(LayoutRun run, CourseTemplate template, int course, double lengthMm, double headJointMm) => Seq<Element>();
}
```

## [3]-[RESEARCH]

- [LAYOUT_STAGE_DEPTH_FILL]: the assembly fold owns straight-run course placement today; opening subtraction (station/elevation interruptions + edge-cut requests), corner endpoint/turn/closure conditions, arch placement (source-backed profile constraints + a station-normalized path rule), and pier layout/closure solving are the queued fold stages, each a `ConstructionFault`-railed extension of `Resolve`, queued in `TASKLOG.md`.
- [STATION_PLACEMENT_PROJECTION]: the `StepCourse` station-stepped placement projection is the cursor/sequence/station stepping with the offset-fraction course shift and the footprint run/rise per orientation, generalized from the masonry footprint to the `Profile.Unit` columns; the projection is the immutable course-state fold, never a mutable accumulation.
- [ARCH_PLACEMENT_CONSTRAINT]: arch placement carries source-backed profile constraints (BS/TN detailing constraints give the voussoir geometry, not a complete scalar placement algorithm) plus a station-normalized path rule; the probe is the scalar placement algorithm that turns the arch `RunPath.Arc` and the voussoir `Profile` into a station-stepped `Seq<Element>` without a host curve. Until it lands, an arch run rails `ConstructionFault.Course` rather than producing a degenerate course.
- [HOST_MATERIALIZATION_SEAM]: the resolved `Layout` is portable scalar data; the host boundary at the app root turns each `Placement` station/elevation/path-angle tuple into a `Rhino.Geometry` transform — this owner never holds a host geometry type, the materialization is the consumer's concern at the host edge, keeping the construction model wire-portable.
