# [MATERIALS_ASSEMBLY]

THE HOST-NEUTRAL RUN GEOMETRY and THE SEAM MATERIAL-COMPOSITION AUTHOR. The canonical element and its material assignment are SEAM-owned: the `Rasm.Element` `ElementGraph` owns the element (the `Object` node + the `Bake` fold), and the seam `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet`) owns the IFC 4.3 material-assignment vocabulary — a placed-unit element record and a parallel `MaterialAssignment` trichotomy are the seam's concern, never re-declared here. This page owns two host-neutral construction concerns the layout fold and the `Projection/component#COMPONENT_PROJECTOR` consume.

`RunPath`/`Placement`/`RunPathAlgebra`/`ConstructionFault` are the host-neutral RUN GEOMETRY. A `Placement` carries a station/elevation/run/rise/path-angle scalar tuple plus the orientation, cut, ply material, and layer-buildup normal offset — never a `Rhino.Geometry` curve or transform — so the `Construction/layout#ASSEMBLY_FOLD` produces a portable `Seq<Placement>` the host materializes at the app root and the appearance engine shades. `RunPath` is the closed coursing-spine geometry (a 1-D arc-length domain) and `LengthOf` the one arc-length algebra; the in-plan turns of a faceted run live on the `Construction/layout#ASSEMBLY_FOLD` `Corner` annotation, never as a parallel path subtype, so the path stays the pure coursing spine.

`CompositionAuthor` is the seam-`MaterialComposition` AUTHOR — it validates NOTHING the seam already owns. The seam `Rasm.Element/Composition` owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent` specs, the `MeasureValue` SI coercion, AND the empty/thickness/fraction admission rail (`ElementFault.ValueRejected`); this author coerces Materials' raw layer/profile/constituent rows into the seam shapes and DELEGATES every invariant to the seam smart-constructor, composing the seam `MeasureValue` for each thickness and a `Component/component#COMPONENT_OWNER` `ComponentId` wrapped into a seam `ProfileRef` for a profiled member. It also derives the `[C7]` occurrence usage the seam `Associate` edge carries (`LayerSetUsage` direction/sense/offset/extent, `ProfileSetUsage` cardinal-point/extent) so the type-level composition and the per-occurrence binding stay disjoint. The page composes the seam (`MaterialComposition`/`MaterialUsage`/`MeasureValue`/`MaterialId`/`ProfileRef`/`CardinalPoint`), the `Component/component#COMPONENT_OWNER` `ComponentId` catalogue key, the `Component/masonry#MASONRY_FAMILY` `Orientation`/`Cut` vocabulary, and the `ConstructionFault` band-2350 rail for the run-geometry faults alone; it re-mints NO seam type and serializes NOTHING to IFC directly — the `Projection/component#COMPONENT_PROJECTOR` lowers the composition into the seam `Material` node and `Rasm.Bim` reads the IFC projection from the seam graph.

## [01]-[INDEX]

- [01]-[PLACEMENT_MODEL]: the host-neutral `RunPath` `[Union]` coursing-spine geometry, the `Placement` scalar tuple (carrying the ply `MaterialId` and the layer-buildup `NormalOffsetMm`), the `RunPathAlgebra` line/arc/dome arc-length algebra, and the `ConstructionFault` band-2350 run-geometry/layout union the construction sub-domain shares.
- [02]-[MATERIAL_COMPOSITION]: the `CompositionAuthor` coercing Materials' layer/profile/constituent rows into the seam `MaterialComposition` (single/layer-set/profile-set(`ProfileRef`)/constituent-set) and DELEGATING every invariant to the seam, plus the `[C7]` `MaterialUsage` occurrence payload (`LayerSetUsage`/`ProfileSetUsage`) the seam `Associate` edge carries.

## [02]-[PLACEMENT_MODEL]

- Owner: `RunPath` `[Union]` the host-neutral coursing-spine geometry; `Placement` the scalar tuple; `RunPathAlgebra` the arc-length/path-angle algebra; `ConstructionFault` `[Union]` band 2350 the construction sub-domain shares.
- Cases: path {line (length), arc (radius, sweep), dome (radius, ring-courses)} — the closed `RunPath` set. A dome is the surface-of-revolution spine the `Construction/layout#ASSEMBLY_FOLD` ring-course fold resolves to a stack of horizontal courses, its meridian a quarter great circle springing→crown. A placed unit is a `Placement` (course/sequence/station/elevation/run/rise/path-angle + orientation/cut + ply `MaterialId` + normal/lateral offset), never a path subtype and never an element-wrapper record (the element identity is the seam's). The `ConstructionFault` cases (`Path`/`Joint`/`Course`/`Opening`/`Nest`) are the run-geometry, joint, coursing, opening, and cutting-stock faults the `RunPathAlgebra`, `Construction/layout#ASSEMBLY_FOLD`, and `Construction/nesting#STOCK_NEST` folds rail on — never a composition-admission fault (the seam owns those).
- Entry: `public static Fin<double> LengthOf(RunPath path, Op key)` is the line/arc/dome arc-length algebra (`Fin<T>` aborts on a non-positive length/radius/sweep/ring-count, `ConstructionFault.Path`); the dome meridian is `radius·π/2`. `public static double AngleAt(RunPath path, double stationMm)` projects a station onto a path angle so a curved coursing run reads its local rotation without a host curve. It is the TOTAL `RunPath` Switch the coursing `StationStep` consumes for `Line`/`Arc` runs; the `Dome` arm completes the union but the live dome path never reaches it (the `Construction/layout#ASSEMBLY_FOLD` `Dome` row elects `DomeRings`, whose per-ring meridian tilt is the `ArchProfile.TiltDegreesAt` sweep), so `AngleAt`'s dome value is a direct-caller meridian projection, never the dome-coursing rotation source.
- Packages: Rasm (project — scalar geometry + `Op`/`Expected`/`FaultExtensions.Category`), Rasm.Element (project — `MaterialId` the ply placement carries), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new coursing spine is one `RunPath` case carrying its arc-length arm (a spline run, the realized dome surface-of-revolution); a new fault is one `ConstructionFault` case; a placed-unit attribute shared by all families is one `Placement` column — never a per-path placement method, never a per-family placed-unit type, never re-introducing the element identity (the SEAM's). The faceted-run turn is NOT a new path case: it is the `Construction/layout#ASSEMBLY_FOLD` `Corner` annotation on a `Line` spine, so the path stays the pure 1-D coursing domain.
- Boundary: `Placement` is HOST-NEUTRAL — it carries station/elevation/run/rise/path-angle plus the orientation, cut, ply `MaterialId`, and the `NormalOffsetMm` layer-buildup offset along the path normal as raw scalars, NEVER a `Rhino.Geometry.Plane`/`Transform`/curve. The ply `MaterialId` tags each placed unit with its layer/profile material so a `LayerSet` buildup carries the per-ply material on the placement itself (the run-level composition is the layout input, the per-unit material the placement column), and `NormalOffsetMm` defaults to zero for a single-ply run and carries the cumulative layer offset for a buildup so the host materializes each ply at its depth without a second placement owner. A unit's cut-plane splay rides the `Cut.PlaneNormalDegrees` the `Component/masonry#MASONRY_FAMILY` `Cut` row carries (a king-closer/cant/bevel cut, `Cut.Angled` flagging the diagonal splay), read off `Placement.Cut` at the host, never a parallel placement column. `RunPath` is the closed coursing-spine geometry and `LengthOf` the one arc-length algebra (a line is its length, an arc is `radius · sweep · π/180`, a dome meridian `radius·π/2`), so a curved run never re-derives arc length per call site. `ConstructionFault` is the one fault every run-geometry/layout/nesting `Fin.Fail` reads across the construction sub-domain (path/joint/course/opening/nest slots — the `Nest` case the `Construction/nesting#STOCK_NEST` cutting-stock fold rails an empty cut-list or an oversized part on, sharing the one 2350 band rather than minting a parallel nesting fault), an `Expected`-derived `Error` (`IValidationError<ConstructionFault>`) whose 2350 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail. It is DISJOINT from the seam `ElementFault.ValueRejected` that owns composition admission: a layout never throws and never returns a sentinel placement, and the composition author never mints a `ConstructionFault`. The orientation/cut vocabulary is the `Component/masonry#MASONRY_FAMILY` `Orientation`/`Cut` algebra composed, never re-minted here.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Element;                  // MaterialId (the seam-carried material identity the ply placement tags)
using Rasm.Materials.Component.Masonry;  // Orientation, Cut (the Component/masonry#MASONRY_FAMILY unit-face + cut vocabulary the Placement carries, in its own sub-namespace)
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using Op = Rasm.Domain.Op;

namespace Rasm.Materials.Construction;

// --- [TYPES] -------------------------------------------------------------------------------
[Union]
public abstract partial record RunPath {
    private RunPath() { }
    public sealed record Line(double LengthMm) : RunPath;
    public sealed record Arc(double RadiusMm, double SweepDegrees) : RunPath;
    public sealed record Dome(double RadiusMm, int RingCourses) : RunPath;
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The construction-sub-domain fault band (2350): Expected-derived over the kernel Rasm.Domain.Expected so band
// 2350 IS the Expected Code and a typed case lifts BARE onto Fin<T>/Validation<Error,T> (no .ToError() hop). The
// kernel base ctor is PARAMETERLESS (Code a virtual Error member, Message abstract, Category virtual) — so band
// 2350 is a `Code => 2350` override and `Message => Detail`, and the per-case Category override drives
// FaultExtensions.Category(error); the legacy `base(detail, 2350, None)` form targets the OTHER
// LanguageExt.Common.Expected (which carries no Category to override) and was the defect. [SkipUnionOps] skips the
// generated implicit-conversion ops (every case carries an explicit Op) while the generated Switch survives, and it
// emits NO per-case factory, so the band declares its own (the production UiFault / seam ElementFault shape): a nested
// `…Case` record carries the data and a same-name-less static factory ConstructionFault.Path(key, detail) returns the
// Expected-derived base so the case lifts BARE onto Fin<T>/Validation<Error,T> with no `new` and no .ToError() hop —
// the `…Case` suffix frees the unsuffixed factory name (a same-named nested type + method is CS0102). This band carries
// the RUN-GEOMETRY/LAYOUT/NESTING faults ONLY (path/joint/course/opening/nest) — composition admission is the seam
// ElementFault.ValueRejected, never re-cased here. Create routes the unspecific case under a boundary-admission Op.
[SkipUnionOps]
[Union]
public abstract partial record ConstructionFault : Expected, IValidationError<ConstructionFault> {
    private ConstructionFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => 2350;
    public override string Message => Detail;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record PathCase(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Path"; }
    public sealed record JointCase(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Joint"; }
    public sealed record CourseCase(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Course"; }
    public sealed record OpeningCase(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Opening"; }
    public sealed record NestCase(Op Key, string Detail) : ConstructionFault(Key, Detail) { public override string Category => "Nest"; }

    public static ConstructionFault Path(Op key, string detail) => new PathCase(key, detail);
    public static ConstructionFault Joint(Op key, string detail) => new JointCase(key, detail);
    public static ConstructionFault Course(Op key, string detail) => new CourseCase(key, detail);
    public static ConstructionFault Opening(Op key, string detail) => new OpeningCase(key, detail);
    public static ConstructionFault Nest(Op key, string detail) => new NestCase(key, detail);
    public static ConstructionFault Create(string message) => Path(Admission, message);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The host-neutral placed unit. The ply Material tags the unit with its layer/profile material (the run-level
// composition is the layout input, the per-unit material this column) so a buildup carries no second placement owner;
// NormalOffsetMm is the cumulative layer offset along the path NORMAL a LayerSet buildup folds, and LateralOffsetMm the
// across-course in-plan offset perpendicular to the path tangent a woven/pinwheel/herringbone bond emits (the
// Component/masonry#MASONRY_FAMILY UnitPlacement.LateralFraction the Construction/layout#ASSEMBLY_FOLD StationStep lifts
// to mm, zero for a stacked/running course). A unit's cut-plane splay is read off the Placement.Cut PlaneNormalDegrees/
// Angled at the host (a king-closer/cant/bevel cut), never a parallel placement column.
public readonly record struct Placement(
    int Course,
    int Sequence,
    double StationMm,
    double ElevationMm,
    double RunMm,
    double RiseMm,
    double PathAngleDegrees,
    Orientation Orientation,
    Cut Cut,
    MaterialId Material,
    double NormalOffsetMm = 0.0,
    double LateralOffsetMm = 0.0);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class RunPathAlgebra {
    public static Fin<double> LengthOf(RunPath path, Op key) =>
        path.Switch(
            line: line => double.IsFinite(line.LengthMm) && line.LengthMm > 0.0
                ? Fin.Succ(line.LengthMm)
                : Fin.Fail<double>(ConstructionFault.Path(key, $"<run-path-line-degenerate:{line.LengthMm:R}>")),
            arc: arc => double.IsFinite(arc.RadiusMm) && arc.RadiusMm > 0.0 && double.IsFinite(arc.SweepDegrees) && Math.Abs(arc.SweepDegrees) > 0.0
                ? Fin.Succ(arc.RadiusMm * (Math.PI / 180.0) * Math.Abs(arc.SweepDegrees))
                : Fin.Fail<double>(ConstructionFault.Path(key, $"<run-path-arc-degenerate:r={arc.RadiusMm:R}:sweep={arc.SweepDegrees:R}>")),
            // A hemispherical dome's meridian is a quarter great circle springing→crown; length is radius·π/2.
            dome: dome => double.IsFinite(dome.RadiusMm) && dome.RadiusMm > 0.0 && dome.RingCourses > 0
                ? Fin.Succ(dome.RadiusMm * Math.PI * 0.5)
                : Fin.Fail<double>(ConstructionFault.Path(key, $"<run-path-dome-degenerate:r={dome.RadiusMm:R}:rings={dome.RingCourses}>")));

    public static double AngleAt(RunPath path, double stationMm) =>
        path.Switch(
            line: _ => 0.0,
            arc: arc => Math.Sign(arc.SweepDegrees) * stationMm / arc.RadiusMm * 180.0 / Math.PI,
            // The dome arm completes the total RunPath Switch and projects a station onto the meridian latitude
            // (station/radius radians) for any direct caller; the live dome COURSING never reaches it — the
            // Construction/layout#ASSEMBLY_FOLD Dome row always elects DomeRings, whose per-ring meridian tilt is the
            // ArchProfile.TiltDegreesAt sweep, not this single-arc projection. A dome path therefore never enters the
            // coursing StationStep that calls AngleAt; the arm is here so a future direct meridian read stays total.
            dome: dome => stationMm / dome.RadiusMm * 180.0 / Math.PI);
}
```

## [03]-[MATERIAL_COMPOSITION]

- Owner: `CompositionAuthor` the seam-`MaterialComposition` BUILDER (coerce-and-delegate, owning NO invariant the seam owns); the `[C7]` `MaterialUsage` occurrence-payload DERIVER.
- Cases: one `CompositionAuthor` family over the seam trichotomy-plus-single — `Single` (one `MaterialId`, a homogeneous element — `IfcMaterial`), `LayerSet` (Materials' material-plus-thickness rows coerced into a `Seq<MaterialLayer>`, walls/slabs/IGUs — `IfcMaterialLayerSet`), `ProfileSet` (one `MaterialId` per extruded member resolved through a `Component/component#COMPONENT_RESOLUTION` `ProfileRef`, members — `IfcMaterialProfileSet`), `ConstituentSet` (Materials' keyword-tagged fraction-weighted rows coerced into a `Seq<MaterialConstituent>`, composites — `IfcMaterialConstituentSet`); the author coerces the rows and DELEGATES to the seam smart-constructor, never a fourth case (the trichotomy + single is the seam's closed set; `IfcMaterialList` deprecated, never admitted).
- Entry: `public static Fin<MaterialComposition> LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key)` · `ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction)> constituents, Op key)` are the coerce-and-delegate `Fin<T>` builders — `LayerSet` coerces each thickness through `MeasureValue.Of(value, UnitsNet.Units.LengthUnit.Millimeter, key)` (which rails `ElementFault.ValueRejected` on a non-finite magnitude) then hands the `Seq<MaterialLayer>` to the seam `MaterialComposition.OfLayerSet(specs, key)` which OWNS the empty-set and non-positive-thickness admission; `ConstituentSet` lifts each row into a `MaterialConstituent` then hands it to the seam `MaterialComposition.OfConstituentSet(constituents, key)` which OWNS the empty-set, fraction-range, and fraction-normalization admission. `ProfileSet(MaterialId material, ComponentId component)` · `Single(MaterialId material)` are TOTAL (the seam `OfProfileSet`/`OfSingle` carry no admission invariant), `ProfileSet` wrapping the catalogue key into a seam `ProfileRef.Of(component.Value)` for the `[M7]` one-hop section handle. `public static Fin<MaterialUsage> UsageOf(MaterialComposition composition, Op key)` derives the `[C7]` occurrence usage the seam `Associate` edge carries from the composition shape — `Fin<T>` because the `ProfileSet` usage admits a `CardinalPoint` through the seam's `MaterialUsage.ProfileSet.Of` admission, the `Single`/`LayerSet`/`ConstituentSet` arms total and lifted into `Fin.Succ`.
- Packages: Rasm.Element (project — `MaterialComposition`/`MaterialUsage`(`None`/`LayerSet`/`ProfileSet`)/`MaterialLayer`/`MaterialConstituent`/`MeasureValue`/`MaterialId`/`ProfileRef`/`LayerSetDirection`/`DirectionSense`/`CardinalPoint`, the seam this author builds), Rasm.Materials.Component (`ComponentId` the catalogue key), Rasm (project — `Op` the op-key), Thinktecture.Runtime.Extensions, LanguageExt.Core (`Seq`/`Fin`/`Traverse`/`Bind`/`Map`).
- Growth: a new composition shape is a seam `MaterialComposition` case the author gains one coerce-and-delegate arm for (a seam growth, never a parallel Materials assignment); a new layer/constituent attribute is one column on the seam layer/constituent spec the coercion fills; a new occurrence-usage axis is one seam `MaterialUsage` case the `UsageOf` derivation maps — never a per-element-type assignment, never a fourth composition family. An element selects its composition by domain: a wall/slab a `LayerSet`, a beam/column a `ProfileSet`, a curtain wall a `ConstituentSet`, a homogeneous element a `Single`.
- Boundary: `CompositionAuthor` BUILDS the seam value and NEVER re-declares OR re-validates it — the seam `Rasm.Element/Composition` owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent` specs, the `MeasureValue` SI coercion, AND the empty/thickness/fraction admission, so this author coerces Materials' raw rows and DELEGATES every invariant to the seam smart-constructor (a duplicated empty/thickness pre-guard here, or a `ConstructionFault` minted for a composition miss, is the named defect — composition admission rails the seam `ElementFault.ValueRejected`, the run-geometry band carrying only path/joint/course/opening/nest). The layer thickness coerces once through the seam `MeasureValue.Of(_, UnitsNet.Units.LengthUnit.Millimeter, key)` ([H2] — the seam owns the SI coercion) so a buildup never re-mints a measure. A `ProfileSet` references a `Component/component#COMPONENT_OWNER` component by its catalogue key as a seam `ProfileRef` (`ProfileRef.Of(component.Value)`) the `Component/component#COMPONENT_RESOLUTION` resolver dereferences ONE-HOP to the `ComputedSection` ([M7]) so a structural consumer never re-resolves a section per call. The `[C7]` occurrence usage is DISJOINT from the type-level composition — `UsageOf` derives a `LayerSetUsage` (`LayerSetDirection.Axis2`/`DirectionSense.Positive`/zero `OffsetFromReferenceLine`/NaN `ReferenceExtent` unset, the IFC-typical vertical-wall default) for a layered element and a `ProfileSetUsage` (`CardinalPoint.Mid` reference, NaN `ReferenceExtent` unset) for a profiled member, the default IFC-canonical binding an app overrides through the seam `MaterialUsage` before projection where an occurrence differs (the seam owns the usage type — Materials needs no override surface); the `Projection/component#COMPONENT_PROJECTOR` rides this usage on the seam `Associate` edge so the per-occurrence binding (which face the layer set references, which cardinal point the profile aligns to) lives on the edge and the buildup lives on the type, never conflated. The composition lowers into the seam `Material` node the `COMPONENT_PROJECTOR` authors and `Rasm.Bim` reads the IFC 4.3 trichotomy (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet` + the `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence) from the seam graph — no `Rhino.Geometry` type, no `IfcOpenShell` evaluation, and no Materials→IFC carrier crosses this owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op
using Rasm.Element;                  // MaterialId, MaterialComposition (Single|LayerSet|ProfileSet|ConstituentSet), MaterialLayer,
                                     // MaterialConstituent, ProfileRef, MeasureValue, MaterialUsage (None|LayerSet|ProfileSet),
                                     // LayerSetDirection, DirectionSense, CardinalPoint
using Rasm.Materials.Component;      // ComponentId — the Component-catalogue key a ProfileSet wraps into a seam ProfileRef
using static LanguageExt.Prelude;

namespace Rasm.Materials.Construction;

// --- [OPERATIONS] --------------------------------------------------------------------------
// Builds the SEAM MaterialComposition (Rasm.Element/Composition owns the [Union] type, the layer/constituent specs,
// the MeasureValue SI coercion, AND the empty/thickness/fraction admission). This author COERCES Materials' raw rows
// into the seam shapes and DELEGATES every invariant to the seam smart-constructor — it declares NO type, owns NO
// invariant the seam owns, and mints NO ConstructionFault for a composition miss (composition admission rails the seam
// ElementFault.ValueRejected; the 2350 band is run-geometry/layout/nesting only).
public static class CompositionAuthor {
    public static MaterialComposition Single(MaterialId material) => MaterialComposition.OfSingle(material);

    // Coerce each row's thickness through the seam MeasureValue (which rails ElementFault.ValueRejected on a non-finite
    // magnitude), then DELEGATE to the seam OfLayerSet which OWNS the empty-set and non-positive-thickness admission —
    // no duplicated pre-guard, no wrong-band fault. The Traverse short-circuits on the first non-finite thickness.
    public static Fin<MaterialComposition> LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key) =>
        layers.Traverse(l => MeasureValue.Of(l.ThicknessMm, UnitsNet.Units.LengthUnit.Millimeter, key).Map(t => new MaterialLayer(l.Material, t, l.Name)))
              .Bind(specs => MaterialComposition.OfLayerSet(specs, key));

    // The component is referenced by its seam ProfileRef ONE-HOP (M7) — the catalogue key (ComponentId.Value) wraps to a
    // ProfileRef the seam ProfileSet case carries. The composition holds the HANDLE here; the Projection/component#COMPONENT_PROJECTOR
    // resolves it through Component/component#COMPONENT_RESOLUTION ONCE and BAKES the neutral SectionProperties onto the case (WithSection).
    // TOTAL — the seam OfProfileSet carries no admission invariant (the ProfileRef one-hop handle is built unconditionally),
    // mirroring the total Single builder rather than wrapping a total op in a fake Fin rail.
    public static MaterialComposition ProfileSet(MaterialId material, ComponentId component) =>
        MaterialComposition.OfProfileSet(material, ProfileRef.Of(component.Value));

    // Lift each row into a seam MaterialConstituent, then DELEGATE to the seam OfConstituentSet which OWNS the empty-set,
    // the per-fraction unit-range, AND the fraction-sum normalization — this author keeps NO constituent invariant.
    public static Fin<MaterialComposition> ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction)> constituents, Op key) =>
        MaterialComposition.OfConstituentSet(constituents.Map(static c => new MaterialConstituent(c.Material, c.Category, c.Fraction)), key);

    // C7: the per-occurrence usage the seam Associate edge carries (seam MaterialUsage), derived from the composition
    // shape — a LayerSet binds with a direction/sense/offset/extent (the IFC-typical vertical-wall Axis2/positive/0
    // default, the 4th referenceExtent an unset NaN because a Materials-authored layer usage knows no occurrence extent —
    // the seam treats NaN as the unset-extent sentinel exactly as the ProfileSet usage's extent does, so the canonical
    // bytes stabilize), a ProfileSet with an IFC cardinal point (Mid, the IFC default reference) + an unset NaN extent, a
    // Single/ConstituentSet with None. Fin<T> because the ProfileSet usage admits its CardinalPoint through the seam
    // MaterialUsage.ProfileSet.Of grid admission (CardinalPoint.Mid is in-grid, so this never faults at the default —
    // the rail is the seam's contract, not a fabricated failure); the other three arms are total and lifted into Fin.Succ.
    // An app needing a non-default occurrence binding composes a seam MaterialUsage directly (the seam owns the type).
    public static Fin<MaterialUsage> UsageOf(MaterialComposition composition, Op key) =>
        composition.Switch(
            single:         _ => Fin.Succ<MaterialUsage>(new MaterialUsage.None()),
            layerSet:       _ => Fin.Succ<MaterialUsage>(new MaterialUsage.LayerSet(LayerSetDirection.Axis2, DirectionSense.Positive, 0.0, double.NaN)),
            profileSet:     _ => MaterialUsage.ProfileSet.Of(CardinalPoint.Mid.Key, double.NaN, key),
            constituentSet: _ => Fin.Succ<MaterialUsage>(new MaterialUsage.None()));
}
```

## [04]-[RESEARCH]

- [SEAM_OWNS_ELEMENT_AND_COMPOSITION]: the canonical element is the `Rasm.Element` `ElementGraph` (the `Object` node + the `Bake` derived fold) and the material-assignment vocabulary is the seam `MaterialComposition` `[Union]`; Materials authors the composition VALUE (`CompositionAuthor`) and the host-neutral run geometry (`RunPath`/`Placement`), and the `Projection/component#COMPONENT_PROJECTOR` lowers the composition into the seam `Material` node. Materials owns no element identity and no second IFC stack — the element identity and the `Bake` fold are the seam's.
- [DELEGATE_NEVER_DUPLICATE]: `CompositionAuthor` is a COERCE-AND-DELEGATE author, not a re-validator — the seam `MaterialComposition.OfLayerSet`/`OfConstituentSet` own the empty-set, non-positive-thickness, fraction-range, and fraction-normalization admission and rail `ElementFault.ValueRejected`, so the author coerces Materials' raw rows into the seam `MaterialLayer`/`MaterialConstituent` shapes (the thickness once through the seam `MeasureValue.Of`, which itself rails `ElementFault.ValueRejected` on a non-finite magnitude) and DELEGATES every invariant to the seam smart-constructor. The 2350 `ConstructionFault` band is the run-geometry/layout/nesting rail (path/joint/course/opening/nest) the `RunPathAlgebra`/`Construction/layout`/`Construction/nesting` folds read, DISJOINT from the seam composition-admission rail: the author mints no `ConstructionFault` for a composition miss and the band carries no composition admission.
- [IFC_MATERIAL_ASSIGNMENT]: the IFC 4.3 material-assignment trichotomy is the seam `MaterialComposition` host-neutral model — `IfcMaterialLayerSet` (walls and slabs), `IfcMaterialProfileSet` (extruded members), `IfcMaterialConstituentSet` (components), plus `Single` for a homogeneous element; `IfcMaterialList` is deprecated and never admitted. `Rasm.Bim` reads the per-case IFC member-shape mapping (`IfcMaterialLayer.LayerThickness`/`IfcMaterialProfile.Profile`/`IfcMaterialConstituent.Category`/`Fraction`) from the projected seam `Material` node, never a Materials wire carrier. The `MaterialConstituent.Fraction` column rides the seam constituent spec — the fraction rides the composition, and the relocated `Rasm.Compute` rule-of-mixtures fold reads the seam-normalized fractions directly without re-guarding them.
- [C7_OCCURRENCE_USAGE]: the type-level `MaterialComposition` Set is the material buildup; the per-occurrence binding is the seam `Associate` edge's `MaterialUsage` — `MaterialUsage.LayerSet` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`/`ReferenceExtent`, mapped to the IFC `IfcMaterialLayerSetUsage`) and `MaterialUsage.ProfileSet` (an IFC `CardinalPoint` reference + `ReferenceExtent`, mapped to the IFC `IfcMaterialProfileSetUsage`, admitted through the seam `MaterialUsage.ProfileSet.Of` grid admission so an out-of-grid reference rails `ElementFault.ValueRejected`). `CompositionAuthor.UsageOf` derives the DEFAULT usage shape from the composition (the IFC-typical `Axis2`/positive/0 layer binding with an unset NaN `ReferenceExtent`, the `Mid` profile reference); the `Projection/component#COMPONENT_PROJECTOR` rides it on the `Associate` edge and `Rasm.Bim`'s emitter resolves it onto the IFC usage entity, so the occurrence binding (which element face the layer set offsets from, which cardinal point the profile aligns) is the edge's and the buildup the type's, never conflated. An app composes a non-default `MaterialUsage` directly against the seam where an occurrence differs (the seam owns the usage type; Materials needs no override surface).
- [LAYER_BUILDUP_GEOMETRY]: a `LayerSet` buildup resolves to a stacked `Seq<Placement>` where each ply offsets by its cumulative thickness along the path normal and tags its own `MaterialId`; the `Construction/layout#ASSEMBLY_FOLD` reads the seam `MaterialComposition.LayerSet` layer thicknesses and folds the wall plies into a placement stream the host materializes — the per-ply `NormalOffsetMm` and `Placement.Material` the buildup columns. The cumulative-thickness offset projection is the `Construction/layout#ASSEMBLY_FOLD` `LayerOffset`/`StackLayers` fold reading the seam composition.
- [COURSING_SPINE_DISCIPLINE]: `RunPath` is the pure 1-D coursing-spine domain — `Line`/`Arc`/`Dome` — and the in-plan turns of a faceted run (an L/U/T-plan wall, a bay) are the `Construction/layout#ASSEMBLY_FOLD` `Corner` annotation on a `Line` spine (a station-match + `TurnDegrees`), never a parallel `Polyline` path case, so the turn concept has ONE owner (`Corner`) and the path stays the arc-length spine the `RunPathAlgebra` integrates. A `Dome` is the surface-of-revolution spine the `Construction/layout#ASSEMBLY_FOLD` `DomeRings` fold resolves to a stack of horizontal ring courses (each ring `R·cos(latitude)` in radius, the meridian a quarter great circle), so the rotational-run family is the dome's discrete-ring case; a continuous spiral run (a helical ramp/stair) is a genuine future `RunPath` case carrying its own pitch-bearing arc-length arm, deferred until a layout consumer requires it rather than smuggled onto the masonry-construction baseline as an unconsumed surface.
