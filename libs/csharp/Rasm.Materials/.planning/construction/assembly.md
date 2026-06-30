# [MATERIALS_ASSEMBLY]

THE HOST-NEUTRAL RUN GEOMETRY and THE SEAM MATERIAL-COMPOSITION AUTHOR. The canonical element and its material assignment are SEAM-owned: the `Rasm.Element` `ElementGraph` owns the element (the `Object` node + the `Bake` fold), and the seam `MaterialComposition` `[Union]` (`Single`/`LayerSet`/`ProfileSet(ProfileRef)`/`ConstituentSet`) owns the IFC 4.3 material-assignment vocabulary — so the prior `Materials.Element` placed-unit record and the prior `MaterialAssignment` trichotomy are RETIRED, never re-declared here. This owner is now two host-neutral construction concerns the projector and the layout fold consume. `RunPath`/`Placement`/`RunPathAlgebra`/`ConstructionFault` are the host-neutral RUN GEOMETRY — a `Placement` carries a station/elevation/run/rise/path-angle scalar tuple plus the orientation, cut, ply material, and layer-buildup normal offset, never a `Rhino.Geometry` curve or transform, so the `Construction/layout#ASSEMBLY_FOLD` produces a portable `Seq<Placement>` the host materializes at the app root and the appearance engine shades. `CompositionAuthor` is the seam-`MaterialComposition` AUTHOR — Materials validates its own layer/profile/constituent rows and BUILDS the seam composition value (the seam owns the `[Union]` type; this author constructs it, composing the seam `MeasureValue` for each layer thickness and the `Profiles/profile#PROFILE_RESOLUTION` `ProfileRef` for a profiled member), AND it produces the `[C7]` occurrence usage the seam `Associate` edge carries (`LayerSetUsage` direction/sense/offset, `ProfileSetUsage` cardinal-point/extent) so the type-level composition and the per-occurrence binding stay disjoint. The page composes the seam (`MaterialComposition`/`MaterialUsage`/`MeasureValue`/`MaterialId`/`ProfileRef`), the `Rasm` kernel for the scalar length algebra and the `Dimension` thickness primitive, the `masonry#PROFILE_FAMILY` `Orientation`/`Cut` vocabulary, and the `ConstructionFault` band-2350 rail; it re-mints NO seam type and serializes NOTHING to IFC directly — the `MaterialProjector` lowers the composition into the seam `Material` node and `Rasm.Bim` reads the IFC projection from the seam graph.

## [01]-[INDEX]

- [01]-[PLACEMENT_MODEL]: the host-neutral `RunPath` `[Union]` path geometry, the `Placement` scalar tuple (carrying the ply `MaterialId` and the layer-buildup `NormalOffsetMm`), the `RunPathAlgebra` line/arc/dome arc-length algebra, and the `ConstructionFault` band-2350 union.
- [02]-[MATERIAL_COMPOSITION]: the `CompositionAuthor` building the seam `MaterialComposition` (single/layer-set/profile-set(`ProfileRef`)/constituent-set) from Materials' layer/profile/constituent rows, and the `[C7]` `MaterialUsage` occurrence payload (`LayerSetUsage`/`ProfileSetUsage`) the seam `Associate` edge carries.

## [02]-[PLACEMENT_MODEL]

- Owner: `RunPath` `[Union]` the host-neutral path geometry; `Placement` the scalar tuple; `RunPathAlgebra` the arc-length/path-angle algebra; `ConstructionFault` `[Union]` band 2350.
- Cases: path {line (length), arc (radius, sweep), dome (radius, ring-courses)} — the closed `RunPath` set; a dome is the surface-of-revolution path the `Construction/layout#ASSEMBLY_FOLD` ring-course fold resolves to a stack of horizontal courses, the meridian a quarter great circle springing→crown; a placed unit is a `Placement` (course/sequence/station/elevation/run/rise/path-angle + orientation/cut + ply `MaterialId` + normal offset), never a path subtype and never the retired `Element` wrapper.
- Entry: `public static Fin<double> LengthOf(RunPath path, Op key)` — the line/arc/dome arc-length algebra (`Fin<T>` aborts on a non-positive length/radius/sweep/ring-count, `ConstructionFault.Path`); the dome meridian is `radius·π/2`; `RunPathAlgebra.AngleAt` projects a station onto a path angle so a curved run (or a dome ring course at its latitude) reads its local rotation without a host curve.
- Packages: Rasm (project — scalar geometry), Rasm.Element (project — `MaterialId` the ply placement carries), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new path geometry is one `RunPath` case (spline/polyline/the realized dome surface-of-revolution) carrying its arc-length arm; a new fault is one `ConstructionFault` case; a placed-unit attribute shared by all families is one `Placement` column — never a per-path placement method, never a per-family placed-unit type (the element identity is the SEAM's, never re-introduced here).
- Boundary: `Placement` is HOST-NEUTRAL — it carries station/elevation/run/rise/path-angle plus the orientation, cut, ply `MaterialId`, and the `NormalOffsetMm` layer-buildup offset along the path normal as raw scalars, NEVER a `Rhino.Geometry.Plane`/`Transform`/curve; the ply `MaterialId` tags each placed unit with its layer/profile material so a `Construction/layout#ASSEMBLY_FOLD` `LayerSet` buildup carries the per-ply material on the placement itself (the retired `Element.Assignment` is gone — the run-level composition is the layout input, the per-unit material the placement column), and the `NormalOffsetMm` defaults to zero for a single-ply run and carries the cumulative layer offset for a buildup so the host materializes each ply at its depth without a second placement owner; `RunPath` is the closed path geometry and `LengthOf` the one arc-length algebra (a line is its length, an arc is `radius · sweep · π/180`), so a curved run never re-derives arc length per call site; `ConstructionFault` is the one fault every `Fin.Fail` reads across the construction sub-domain (path/joint/course/opening/nest slots — the `Nest` case the `Construction/nesting#STOCK_NEST` cutting-stock fold rails an empty cut-list or an oversized part on, sharing the one 2350 band rather than minting a parallel nesting fault), an `Expected`-derived `Error` (`IValidationError<ConstructionFault>`) whose 2350 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail, so a layout never throws and never returns a sentinel placement; the orientation/cut vocabulary is the `masonry#PROFILE_FAMILY` `Orientation`/`Cut` algebra composed, never re-minted here.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Element;                  // MaterialId (the seam-carried material identity the ply placement tags)
using Thinktecture;
using Expected = Rasm.Domain.Expected;   // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using Op = Rasm.Domain.Op;

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
// the `…Case` suffix frees the unsuffixed factory name (a same-named nested type + method is CS0102). The Create bridge
// routes the unspecific case under a boundary-admission Op, never a default Op.
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
// composition is the layout input, the per-unit material this column) so a buildup carries no retired Element
// wrapper; NormalOffsetMm is the cumulative layer offset along the path normal a LayerSet buildup folds.
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
    double NormalOffsetMm = 0.0);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class RunPathAlgebra {
    public static Fin<double> LengthOf(RunPath path, Op key) =>
        path.Switch(
            state: key,
            line: static (k, line) => double.IsFinite(line.LengthMm) && line.LengthMm > 0.0
                ? Fin.Succ(line.LengthMm)
                : Fin.Fail<double>(ConstructionFault.Path(k, "<run-path-degenerate>")),
            arc: static (k, arc) => double.IsFinite(arc.RadiusMm) && arc.RadiusMm > 0.0 && Math.Abs(arc.SweepDegrees) > 0.0
                ? Fin.Succ(arc.RadiusMm * (Math.PI / 180.0) * Math.Abs(arc.SweepDegrees))
                : Fin.Fail<double>(ConstructionFault.Path(k, "<run-path-degenerate>")),
            // A hemispherical dome's meridian is a quarter great circle springing→crown; length is radius·π/2.
            dome: static (k, dome) => double.IsFinite(dome.RadiusMm) && dome.RadiusMm > 0.0 && dome.RingCourses > 0
                ? Fin.Succ(dome.RadiusMm * Math.PI * 0.5)
                : Fin.Fail<double>(ConstructionFault.Path(k, "<run-path-degenerate>")));

    public static double AngleAt(RunPath path, double stationMm) =>
        path.Switch(
            state: stationMm,
            line: static (_, _) => 0.0,
            arc: static (station, arc) => Math.Sign(arc.SweepDegrees) * station / arc.RadiusMm * 180.0 / Math.PI,
            // The meridian tilt at a station: the latitude angle from springing (station/radius radians) in degrees.
            dome: static (station, dome) => station / dome.RadiusMm * 180.0 / Math.PI);
}
```

## [03]-[MATERIAL_COMPOSITION]

- Owner: `CompositionAuthor` the seam-`MaterialComposition` builder; the `[C7]` `MaterialUsage` occurrence-payload producer; the layer/profile/constituent input validation.
- Cases: one `CompositionAuthor` family over the seam trichotomy-plus-single — `Single` (one `MaterialId`, a homogeneous element), `LayerSet` (a `Seq<(MaterialId, ThicknessMm, Name)>` of material-plus-thickness layers, walls/slabs/IGUs — `IfcMaterialLayerSet`), `ProfileSet` (one `MaterialId` per extruded member resolved through a `Profiles/profile#PROFILE_RESOLUTION` `ProfileRef`, members — `IfcMaterialProfileSet`), `ConstituentSet` (a `Seq<(MaterialId, Category, Fraction)>` of keyword-tagged components, curtain walls/composites — `IfcMaterialConstituentSet`); the author builds the seam value and derives the matching `MaterialUsage` shape, never a fourth case (the trichotomy + single is the seam's closed set; `IfcMaterialList` deprecated, never admitted).
- Entry: `public static Fin<MaterialComposition> LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key)` · `ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction)> constituents, Op key)` are the invariant-bearing `Fin<T>` builders (aborting on an empty set, a non-positive layer thickness, or a non-normalizing constituent-fraction set — `ConstructionFault`), while `ProfileSet(MaterialId material, ProfileId profile)` · `Single(MaterialId material)` are TOTAL (the seam `OfProfileSet`/`OfSingle` carry no admission invariant); each composes the seam `MeasureValue.Of(value, UnitsNet.Units.LengthUnit.Millimeter, key)` for a thickness and `ProfileRef.Of(profileId.Value)` for the `[M7]` one-hop section handle (the seam `OfLayerSet`/`OfConstituentSet` take `Op` and return `Fin` owning the empty/thickness/fraction admission, the total `OfSingle`/`OfProfileSet` lifted into `Fin` only where a caller's fold demands it); `public static MaterialUsage UsageOf(MaterialComposition composition)` derives the `[C7]` occurrence usage the seam `Associate` edge carries from the composition shape.
- Packages: Rasm.Element (project — `MaterialComposition`/`MaterialUsage`(`LayerSetUsage`/`ProfileSetUsage`)/`MeasureValue`/`QuantityType`/`MaterialId`/`ProfileRef`, the seam this author builds), Rasm (project — `Dimension` the raw thickness primitive), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new composition shape is a seam `MaterialComposition` case the author gains one builder arm for (a seam growth, never a parallel Materials assignment); a new layer/constituent attribute is one column on the seam layer/constituent spec the builder fills; a new occurrence-usage axis is one seam `MaterialUsage` case the `UsageOf` derivation maps — never a per-element-type assignment, never a fourth composition family. An element selects its composition by domain: a wall/slab a `LayerSet`, a beam/column a `ProfileSet`, a curtain wall a `ConstituentSet`, a homogeneous element a `Single`.
- Boundary: `CompositionAuthor` BUILDS the seam value and NEVER re-declares it — the seam `Rasm.Element/Composition` owns the `MaterialComposition` `[Union]`, the `MaterialLayer`/`MaterialConstituent` specs it carries, and the `MeasureValue` thickness; this author validates Materials' raw rows and constructs the seam composition, the layer thickness coerced once through the seam `MeasureValue.Of(QuantityType.Length, mm, "mm", key)` (the seam owns the SI coercion, [H2]) so a buildup never re-mints a measure; a `ProfileSet` references a `Profiles/profile#PROFILE_OWNER` profile by its catalogue key as a seam `ProfileRef` (`ProfileRef.Of(profileId.Value)`) the `Profiles/profile#PROFILE_RESOLUTION` resolver dereferences ONE-HOP to the `ComputedSection` ([M7]) so a structural consumer never re-resolves a section per call; the `[C7]` occurrence usage is DISJOINT from the type-level composition — `UsageOf` derives a `LayerSetUsage` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`) for a layered element and a `ProfileSetUsage` (`CardinalPoint`/`ReferenceExtent`) for a profiled member, the `MaterialProjector` rides this usage on the seam `Associate` edge so the per-occurrence binding (which face the layer set references, which cardinal point the profile aligns to) lives on the edge and the buildup lives on the type, never conflated; the composition lowers into the seam `Material` node the `MaterialProjector` authors and `Rasm.Bim` reads the IFC 4.3 trichotomy (`IfcMaterialLayerSet`/`IfcMaterialProfileSet`/`IfcMaterialConstituentSet` + the `IfcMaterialLayerSetUsage`/`IfcMaterialProfileSetUsage` occurrence) from the seam graph — no `Rhino.Geometry` type, no `IfcOpenShell` evaluation, and no Materials→IFC carrier crosses this owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op
using Rasm.Element;                  // MaterialId, MaterialComposition (Single|LayerSet|ProfileSet|ConstituentSet), MaterialLayer,
                                     // MaterialConstituent, ProfileRef, MeasureValue, MaterialUsage (None|LayerSet|ProfileSet), LayerSetDirection, DirectionSense
using static LanguageExt.Prelude;

// --- [OPERATIONS] --------------------------------------------------------------------------
// Builds the SEAM MaterialComposition (Rasm.Element/Composition owns the [Union] type, the layer/constituent specs,
// the MeasureValue SI coercion, and the empty/thickness/fraction admission). This author validates Materials' raw rows
// and constructs the seam value through the seam smart-constructors (which take Op and return Fin); it declares NO type.
public static class CompositionAuthor {
    public static MaterialComposition Single(MaterialId material) => MaterialComposition.OfSingle(material);

    public static Fin<MaterialComposition> LayerSet(Seq<(MaterialId Material, double ThicknessMm, string Name)> layers, Op key) =>
        layers.IsEmpty || layers.Exists(static l => !(double.IsFinite(l.ThicknessMm) && l.ThicknessMm > 0.0))
            ? Fin.Fail<MaterialComposition>(ConstructionFault.Path(key, "<layer-set-empty-or-nonpositive-thickness>"))
            : layers.Traverse(l => MeasureValue.Of(l.ThicknessMm, UnitsNet.Units.LengthUnit.Millimeter, key).Map(t => new MaterialLayer(l.Material, t, l.Name)))
                .Bind(specs => MaterialComposition.OfLayerSet(specs, key));

    // The profile is referenced by its seam ProfileRef ONE-HOP (M7) — the catalogue key (ProfileId.Value) wraps to a
    // ProfileRef the seam ProfileSet case carries. The composition holds the HANDLE here; the Projection/material#MATERIAL_PROJECTOR
    // resolves it through Profiles/profile#PROFILE_RESOLUTION ONCE and BAKES the neutral SectionProperties onto the case (WithSection).
    // TOTAL — the seam OfProfileSet carries no admission invariant (the ProfileRef one-hop handle is built unconditionally),
    // so this mirrors the total Single builder rather than wrapping a total op in a fake Fin rail.
    public static MaterialComposition ProfileSet(MaterialId material, ProfileId profile) =>
        MaterialComposition.OfProfileSet(material, ProfileRef.Of(profile.Value));

    // The seam OfConstituentSet owns the fraction-sum normalization; this author keeps only the empty-set ConstructionFault.
    public static Fin<MaterialComposition> ConstituentSet(Seq<(MaterialId Material, string Category, double Fraction)> constituents, Op key) =>
        constituents.IsEmpty
            ? Fin.Fail<MaterialComposition>(ConstructionFault.Course(key, "<constituent-set-empty>"))
            : MaterialComposition.OfConstituentSet(constituents.Map(static c => new MaterialConstituent(c.Material, c.Category, c.Fraction)), key);

    // C7: the per-occurrence usage the seam Associate edge carries (seam MaterialUsage), derived from the composition
    // shape — a LayerSet binds with a direction/sense/offset, a ProfileSet with an IFC cardinal point (10 = centroid) +
    // extent, a Single/ConstituentSet with None. The default is the IFC-typical axis/centroid; an app overrides it before
    // projection where an occurrence differs.
    public static MaterialUsage UsageOf(MaterialComposition composition) =>
        composition.Switch<MaterialUsage>(
            single:         static _ => new MaterialUsage.None(),
            layerSet:       static _ => new MaterialUsage.LayerSet(LayerSetDirection.Axis2, DirectionSense.Positive, 0.0),
            profileSet:     static _ => new MaterialUsage.ProfileSet(10, 0.0),
            constituentSet: static _ => new MaterialUsage.None());
}
```

## [04]-[RESEARCH]

- [SEAM_OWNS_ELEMENT_AND_COMPOSITION]: the canonical element is the `Rasm.Element` `ElementGraph` (the `Object` node + the `Bake` derived fold), and the material-assignment vocabulary is the seam `MaterialComposition` `[Union]` — so the prior parallel `Materials.Element(Profile, Placement, MaterialAssignment)` and `MaterialAssignment(LayerSet/ProfileSet/ConstituentSet)` are RETIRED, the two-parallel-element-owner naivety the rebuild cures. Materials authors the composition VALUE (`CompositionAuthor`) and the host-neutral run geometry (`RunPath`/`Placement`), then the `Projection/material#MATERIAL_PROJECTOR` lowers the composition into the seam `Material` node — Materials owns no element identity and no second IFC stack.
- [IFC_MATERIAL_ASSIGNMENT]: the IFC 4.3 material-assignment trichotomy is the seam `MaterialComposition` host-neutral model — `IfcMaterialLayerSet` (walls and slabs), `IfcMaterialProfileSet` (extruded members), `IfcMaterialConstituentSet` (components), plus `Single` for a homogeneous element; `IfcMaterialList` is deprecated and never admitted. `Rasm.Bim` reads the per-case IFC member-shape mapping (`IfcMaterialLayer.LayerThickness`/`IfcMaterialProfile.Profile`/`IfcMaterialConstituent.Category`/`Fraction`) from the projected seam `Material` node, never a Materials wire carrier. The `MaterialConstituent.Fraction` column landed on the seam constituent spec (the prior `ConstituentWeight` fold-input residual is closed — the fraction rides the composition, and the relocated `Rasm.Compute` rule-of-mixtures fold reads it directly).
- [C7_OCCURRENCE_USAGE]: the type-level `MaterialComposition` Set is the material buildup; the per-occurrence binding is the seam `Associate` edge's `MaterialUsage` — `MaterialUsage.LayerSet` (`LayerSetDirection`/`DirectionSense`/`OffsetFromReferenceLine`, mapped to the IFC `IfcMaterialLayerSetUsage`) and `MaterialUsage.ProfileSet` (an IFC cardinal-point int / `ReferenceExtent`, mapped to the IFC `IfcMaterialProfileSetUsage`). `CompositionAuthor.UsageOf` derives the usage shape from the composition; the `MaterialProjector` rides it on the `Associate` edge and `Rasm.Bim`'s emitter resolves it onto the IFC usage entity, so the occurrence binding (which element face the layer set offsets from, which cardinal point the profile aligns) is the edge's, the buildup the type's — never conflated, the layer-set-usage stranding the prior model carried as a probe now realized as the edge payload.
- [LAYER_BUILDUP_GEOMETRY]: a `LayerSet` buildup resolves to a stacked `Seq<Placement>` where each ply offsets by its cumulative thickness along the path normal and tags its own `MaterialId`; the `Construction/layout#ASSEMBLY_FOLD` reads the seam `MaterialComposition.LayerSet` layer thicknesses and folds the wall plies into a placement stream the host materializes — the per-ply `NormalOffsetMm` and `Placement.Material` are the realized buildup columns, the retired `Element.Assignment` re-tagging gone. The cumulative-thickness offset projection is the `Construction/layout#ASSEMBLY_FOLD` `LayerOffset`/`StackLayers` fold reading the seam composition.
