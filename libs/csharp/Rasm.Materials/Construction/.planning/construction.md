# [MATERIALS_CONSTRUCTION]

| [OWNER]        | [AXES]                                                                                                       | [STATE] | [DEPTH]                                                              |
| -------------- | ----------------------------------------------------------------------------------------------------------- | :-----: | ------------------------------------------------------------------ |
| `construction` | `Element` · `Assembly` · `Layout` · `Placement` · `RunPath` · `Course` · `JointPolicy` · `ConstructionFault` | SPIKE   | 3-layer owner (element→assembly→layout); 1 placement fold; 2 fences |

[STATE] is SPIKE: the `Element → Assembly → Layout` layering and the `Placement` host-neutral data shape are transcription-shaped from the archived masonry layout algebra (`BrickRun → BrickAssembly.Layout → BrickPlacement`), generalized to a `Profiles/profile#PROFILE_OWNER` `Profile` over any family; the `RunPath` line/arc length algebra and the station/elevation course fold are settled. The residual probe is the opening-subtraction / corner-closure / arch-placement / pier-solving scalar rules the assembly fold consumes (the archived layout owns straight-run course placement; openings, corners, arches, and piers are queued), and the cross-domain seam where a placement carries a `Profiles/profile#PROFILE_OWNER` appearance assignment. The owner card and the layering axis are decision-complete now; depth-fill is queued in `TASKLOG.md`.

THE HOST-NEUTRAL CONSTRUCTION DATA MODEL. One `Element` is a single placed unit (a `Profiles/profile#PROFILE_OWNER` `Profile` at a station/elevation with an orientation and a cut), one `Assembly` is a parameterized run of elements (a profile + a bond + a path + joint policy), and one `Layout` is the resolved placement stream an assembly folds to — three layers, element ⊂ assembly ⊂ layout, as PORTABLE DATA, never host geometry. The model is host-neutral: a `Placement` carries a station/elevation/run/rise/path-angle scalar tuple plus the orientation and cut, never a `Rhino.Geometry` curve or a host transform — the host boundary turns the placement stream into geometry at the app root, this owner produces only the portable layout data the wire and the appearance engine consume. A layout is NEVER per-family code: the archived masonry `BrickAssembly.Layout` station/elevation/joint fold generalizes to a `Profile` over any `ProfileFamily`, so a brick wall and a CMU wall and a glulam frame are the SAME `Layout` fold differing only in the `Profile` unit columns and the bond/course vocabulary. The page composes the `Profiles/profile#PROFILE_FAMILY` bond/orientation vocabulary for the course template, the `Rasm` kernel for the scalar length algebra, and the `Appearance/appearance-graph#MATERIAL_LIBRARY` `MaterialId` a placed element's `Profile` maps to for appearance — never re-deriving a profile, a bond, or an appearance here.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                                |
| :-----: | -------------- | ------------------------------------------------------------------------------------ |
|   [1]   | ELEMENT_MODEL  | `Element` placed-unit shape; `Placement` scalar tuple; `RunPath` line/arc length algebra; `ConstructionFault` band |
|   [2]   | ASSEMBLY_FOLD  | `Assembly` parameterized run; `Layout` resolved placement stream; the station/elevation course fold; `JointPolicy` |

## [2]-[ELEMENT_MODEL]

- Owner: `Element` placed-unit shape; `Placement` the host-neutral scalar tuple; `RunPath` `[Union]` the path geometry; `ConstructionFault` `[Union]` band 2350; `ConstructionKeyPolicy` ordinal accessor.
- Cases: path {line (length), arc (radius, sweep)} — the closed `RunPath` set; an element is a `Profile` placed at a `Placement`, never a path subtype.
- Entry: `public static Fin<double> LengthOf(RunPath path, Op key)` — the line/arc arc-length algebra (`Fin<T>` aborts on a non-positive length/radius/sweep, `ConstructionFault.Path`); `Placement.At` projects a station onto a path angle so a curved run reads its local rotation without a host curve.
- Packages: Rasm (project — scalar geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new path geometry is one `RunPath` case (spline/polyline) carrying its arc-length arm; a new fault is one `ConstructionFault` case; a placed-unit attribute shared by all families is one `Placement`/`Element` column — never a per-path placement method, never a per-family element type. The archived `BrickPath`/`BrickPlacement` collapse here: `RunPath` is the path union and `Placement` the scalar tuple, generalized from brick to any `Profile`.
- Boundary: `Placement` is HOST-NEUTRAL — it carries station/elevation/run/rise/path-angle as raw scalars plus the orientation and cut, NEVER a `Rhino.Geometry.Plane`/`Transform`/curve; the host boundary at the app root materializes the placement stream into geometry, this owner produces only portable data the wire and the appearance engine read; `RunPath` is the closed path geometry and `LengthOf` the one arc-length algebra (a line is its length, an arc is `radius · sweep · π/180`), so a curved run never re-derives arc length per call site; `ConstructionFault` (band 2350) is the one fault every `Fin.Fail` reads (path/joint/course/opening slots) so a layout never throws and never returns a sentinel placement; the orientation/cut vocabulary is the `Profiles/profile#PROFILE_FAMILY` `Orientation`/`Cut` algebra composed, never re-minted here.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// RunPath: the closed path geometry an assembly runs along — host-neutral, the archived BrickPath generalized. Line
// carries its length, Arc its radius and sweep; LengthOf is the one arc-length algebra. A spline path is one more case.
[Union]
public abstract partial record RunPath {
    private RunPath() { }
    public sealed record Line(double LengthMm) : RunPath;
    public sealed record Arc(double RadiusMm, double SweepDegrees) : RunPath;
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ConstructionFault {
    private ConstructionFault() { }
    public sealed record Path(string Detail) : ConstructionFault;      // non-positive / non-finite path dimension
    public sealed record Joint(string Detail) : ConstructionFault;     // non-positive head/bed joint
    public sealed record Course(string Detail) : ConstructionFault;    // empty / undersized course template
    public sealed record Opening(string Detail) : ConstructionFault;   // opening-subtraction degeneracy (the queued probe)
    public int Band => 2350;
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class ConstructionKeyPolicy : IEqualityComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
}

// --- [MODELS] ------------------------------------------------------------------------------
// Placement: THE host-neutral placed-unit datum — scalar station/elevation/run/rise/path-angle plus orientation and cut.
// NEVER a host Plane/Transform/curve; the app-root host boundary materializes geometry from this, the model stays portable.
// The archived BrickPlacement generalized: it placed a brick, this places any Profiles/profile#PROFILE_OWNER Profile.
public readonly record struct Placement(
    int Course,
    int Sequence,
    double StationMm,
    double ElevationMm,
    double RunMm,
    double RiseMm,
    double PathAngleDegrees,
    Orientation Orientation,
    Cut Cut);

// Element: a single placed unit — a Profile at a Placement. One element shape across families; a per-family element is
// the deleted form. The AppearanceId rides the Profile (Profiles/profile#PROFILE_OWNER) so a placement shades through
// the Appearance/appearance-graph#MATERIAL_LIBRARY MaterialId row, never a placement-specific appearance.
public sealed record Element(Profile Profile, Placement Placement);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class RunPathAlgebra {
    // LengthOf: the one arc-length algebra. A line is its length; an arc is radius * sweep * pi/180. One owner for the
    // path length — a per-call-site arc-length copy is the deleted form. Generalized from the archived BrickAssembly.LengthOf.
    public static Fin<double> LengthOf(RunPath path, Op key) =>
        path switch {
            RunPath.Line line when double.IsFinite(line.LengthMm) && line.LengthMm > 0.0 => Fin.Succ(line.LengthMm),
            RunPath.Arc arc when double.IsFinite(arc.RadiusMm) && arc.RadiusMm > 0.0 && Math.Abs(arc.SweepDegrees) > 0.0 =>
                Fin.Succ(arc.RadiusMm * (Math.PI / 180.0) * Math.Abs(arc.SweepDegrees)),
            _ => Fin.Fail<double>(ConstructionFault.Path("<run-path-degenerate>")),
        };

    // AngleAt: project a station onto the local path angle so a curved run reads its rotation without a host curve.
    public static double AngleAt(RunPath path, double stationMm) =>
        path switch {
            RunPath.Arc arc => Math.Sign(arc.SweepDegrees) * stationMm / arc.RadiusMm * 180.0 / Math.PI,
            _ => 0.0,
        };
}
```

## [3]-[ASSEMBLY_FOLD]

- Owner: `Assembly` parameterized run; `Layout` resolved placement stream; `JointPolicy` the head/bed joint resolution; the station/elevation course fold.
- Cases: one `Assembly` shape — `Profile` + `BondName` + `RunPath` + height + `JointPolicy`; one `Layout` fold producing the `Seq<Element>` placement stream.
- Entry: `public static Fin<Layout> Resolve(Assembly assembly, Op key)` — the host-neutral layout fold: validate the run, resolve joints, compute the course count, fold each course's template into a station-stepped `Seq<Element>`; `Fin<T>` aborts on a generated bond (`ProfileFault.Bond` via `BondName.Course`), a degenerate path (`ConstructionFault.Path`), or a non-positive joint (`ConstructionFault.Joint`).
- Packages: Rasm (project — scalar geometry), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: opening subtraction, corner/closure conditions, arch placement, and pier solving are each one fold stage the resolve composes (the queued probes); a new joint rule is one `JointPolicy` column; the family axis grows at `Profiles/profile#PROFILE_FAMILY`, so a CMU/timber/glazing assembly is the SAME `Resolve` fold over a different `Profile` — never a per-family layout method. The archived `BrickAssembly.Layout` straight-run fold is the seed; the queued stages extend it without a second layout owner.
- Boundary: `Resolve` is the ONE layout fold — a per-family `Layout` is the deleted form; it composes the `Profiles/profile#PROFILE_FAMILY` `BondName.Course` template (a generated bond rails through that owner's `Fin`, never re-interpreted here) and the element-2 `RunPath` length/angle algebra; the course fold is immutable — each course projects a station-stepped `Seq<Element>` and the layout is the `Fold` concatenation, never a mutable placement-list accumulation; `JointPolicy` resolves head/bed joints once from the `Profile.Standard` coordinating thickness with an explicit override, so a joint literal never scatters; the resolved `Layout` is portable data (a `Seq<Element>` of scalar `Placement` tuples) the host boundary materializes and the appearance engine shades — opening subtraction, corner closure, arch placement, and pier solving are the queued fold stages, each a `ConstructionFault`-railed extension, never a placeholder.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// JointPolicy: the head/bed joint resolution — coordinating thickness from the Profile.Standard with an explicit override.
// One joint owner; a scattered joint literal is the deleted form. The archived JointsOf generalized off the Profile.
public readonly record struct JointPolicy(Option<double> HeadJointMm, Option<double> BedJointMm) {
    public static readonly JointPolicy Standard = new(Option<double>.None, Option<double>.None);
    public (double Head, double Bed) Resolve(Profile profile) =>
        (HeadJointMm.IfNone(profile.Standard.StandardJointThicknessMm), BedJointMm.IfNone(profile.Standard.StandardJointThicknessMm));
}

// Assembly: the parameterized run — a Profile, a bond, a path, a height, and a joint policy. One assembly shape across
// families; the same Resolve fold turns a brick wall and a CMU wall and a glulam frame into a Layout by Profile data alone.
public sealed record Assembly(Profile Profile, BondName Bond, RunPath Path, double HeightMm, JointPolicy Joints);

// Layout: the resolved placement stream — the portable host-neutral output. A Seq<Element> of scalar Placement tuples
// the host boundary materializes into geometry at the app root and the appearance engine shades through MaterialId.
public sealed record Layout(Assembly Assembly, double LengthMm, int CourseCount, double HeadJointMm, double BedJointMm, Seq<Element> Elements);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ConstructionLayout {
    // Resolve: THE host-neutral layout fold, generalized from the archived BrickAssembly.Layout. Validate the run,
    // resolve joints off the Profile, compute the course count from the Profile coursing, fold each course's template
    // (BondName.Course — a generated bond rails through Profiles) into a station-stepped Seq<Element>, concatenate. One
    // fold over any Profile family; the immutable Fold concatenation is the placement stream, never a mutable list.
    public static Fin<Layout> Resolve(Assembly assembly, Op key) =>
        from length in RunPathAlgebra.LengthOf(assembly.Path, key)
        let joints = assembly.Joints.Resolve(assembly.Profile)
        from validHead in guard(double.IsFinite(joints.Head) && joints.Head > 0.0, ConstructionFault.Joint(key, $"<head-joint:{joints.Head}>"))
        from validBed in guard(double.IsFinite(joints.Bed) && joints.Bed > 0.0, ConstructionFault.Joint(key, $"<bed-joint:{joints.Bed}>"))
        let courseCount = Math.Max(1, (int)Math.Ceiling(assembly.HeightMm / assembly.Profile.Unit.CourseHeightMm.Value))
        from elements in Courses(assembly, length, courseCount, joints.Head, key)
        select new Layout(assembly, length, courseCount, joints.Head, joints.Bed, elements);

    // Courses: fold each course index into its template's station-stepped Seq<Element>, concatenating immutably. A
    // generated bond rails ProfileFault.Bond through BondName.Course; an empty template rails ConstructionFault.Course.
    static Fin<Seq<Element>> Courses(Assembly assembly, double lengthMm, int courseCount, double headJointMm, Op key) =>
        toSeq(Enumerable.Range(0, courseCount)).Fold(
            Fin.Succ(Seq<Element>()),
            (acc, course) => acc.Bind(elements =>
                assembly.Bond.Course(course, key)
                    .Bind(template => StepCourse(assembly, template, course, lengthMm, headJointMm, key))
                    .Map(courseElements => elements + courseElements)));

    static Fin<Seq<Element>> StepCourse(Assembly assembly, CourseTemplate template, int course, double lengthMm, double headJointMm, Op key) =>
        guard(!template.Sequence.IsEmpty, ConstructionFault.Course(key, $"<empty-course:{course}>"))
            .Map(_ => /* station-stepped placement projection — the archived Layout.cs Repeat/Place fold over the Profile unit */ Seq<Element>());
}
```

## [4]-[RESEARCH]

- [LAYOUT_STAGE_DEPTH_FILL]: the assembly fold owns straight-run course placement today (the archived `BrickAssembly.Layout` generalized); opening subtraction (station/elevation interruptions + edge-cut requests), corner endpoint/turn/closure conditions, arch placement (source-backed profile constraints + a station-normalized path rule), and pier layout/closure solving are the queued fold stages, each a `ConstructionFault`-railed extension of `Resolve`, queued in `TASKLOG.md`.
- [STATION_PLACEMENT_PROJECTION]: the `StepCourse` station-stepped placement projection transcribes the archived `Layout.cs` `Repeat`/`Place` fold (the cursor/sequence/station stepping with the offset-fraction course shift and the footprint run/rise per orientation), generalized from the brick footprint to the `Profile.Unit` columns; the projection is the immutable course-state fold, never a mutable accumulation.
- [HOST_MATERIALIZATION_SEAM]: the resolved `Layout` is portable scalar data; the host boundary at the app root turns each `Placement` station/elevation/path-angle tuple into a `Rhino.Geometry` transform — this owner never holds a host geometry type, the materialization is the consumer's concern at the host edge, keeping the construction model wire-portable.
