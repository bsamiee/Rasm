# Bricks — Roadmap to Generation/Assembly

## Status

The `Bricks/` catalogue is **complete for the materials layer**. All downstream brick-aware geometry work happens OUTSIDE this folder, in `Rasm/Analysis/` (universal projection primitives), `Rasm.Rhino/` (universal block instancing), and a future `Rasm.Masonry/` (assembly composition). Materials never depends on Rhino, never references geometry, never owns layout.

Final state (post critical review):

- **Single-file footprint**: `Brick.cs` only. `BrickCatalog.cs` eliminated — `SpecialShape.Catalog` is a static field on the `SpecialShape` Union; `BrickRegion.Designations()` is an extension method.
- **Brick** is attached to `BrickDesignation` SmartEnum via `Spec` plus delegate properties — consumers access `BrickDesignation.UsModular.Region`, `.Specified`, `.Coring`, `.Type`, etc. directly without `.Spec.` indirection.
- **Bond course templates** attached to `BondName` SmartEnum (**34 bonds** — Gothic removed as geometrically identical to Monk; Dearnes removed as interpretive without authoritative diagram; RunningRotated reclassified Parametric).
- **Regional masonry policy** (movement / expansion / weep / tie) attached to `BrickRegion` SmartEnum.
- **Arch rules** attached to `ArchProfile` SmartEnum (BIA TN 31 universal).
- **`Dim3`, `AspectRatio`, `VerticalCoursing`** are Thinktecture validated value objects (`[ComplexValueObject]` / `[ValueObject<double>]`) decorated with `[ValidationError<BrickFault>]` — validation failures emit a typed `BrickFault` (an `Error`-derived `[Union]` implementing `IValidationError<BrickFault>`) that flows directly into the LanguageExt `Fin<T>` rail at consumer call sites. `Dim3.TryCreate(...)` returns `BrickFault?` directly; no `MapFail` bridge needed.
- **`AspectRatio.Accepts(actual)`** with `MatchTolerance = 0.15` constant — bond-brick compatibility check is one call.
- **`BrickSource`** typed `[Union]` (BiaTn10Table1/Table2/BsEn771Part1/Din105/AsNzs4455Part1/Is1077, each `Note`).
- **Special shapes** as `[Union] SpecialShape` cases (Bullnose/Cownose/Plinth/Coping/Cant/Squint/Voussoir/AirVent/Soap/Half/ThreeQuarter/Quoin/BrickOnEdgeSill) — 32 catalogued shapes in `SpecialShape.Catalog` `FrozenSet`.
- **Ergonomic projections**: `BondName.AcceptsAspect(Brick)`, `BondName.CourseAt(int courseIndex)`, `ExpansionJointSpacing.SpacingFor(bool hasOpenings)`, `BrickRegion.Designations()`.
- **No `BrickCatalog` class, no `BrickError` Union, no `WeepLocation`/`TieType`, no `WaterResistance`** — eliminated as speculative/dead.

## Bond Coverage (34 entries, authoritative-only)

**Anglo-American (BIA TN 30 / BDA UK TIS-A4 / Lynch v.1)** — Running, ThirdRunning, Common, English, Flemish, Stack, EnglishCross, FlemishCross, Sussex, Scotch, Header, Monk, BasketWeave, Soldier, DoubleStretcherGardenWall, MixedGardenWall, FlemishStretcher, FlemishDiagonal, FlemishSpiral, TudorCrossHatch, Pinwheel, DellaRobbiaWeave, Diaper

**Indian (IS 2212)** — RatTrap, Quetta

**Variant UK Flemish (Lynch v.1)** — SingleFlemish, DoubleFlemish

**Continental European** — DutchMulti (Holländischer Verband), Wild (Wilder Verband, DIN 1053), Staffel (Dutch staircase), AppareilALaFrancaise (Wiktionnaire)

**Roman** — OpusReticulatum

**Parametric** (require runtime composition — rotation, randomness, or per-design diaper mask): RunningRotated, Herringbone45, OpusReticulatum, Diaper, FlemishDiagonal, FlemishSpiral, TudorCrossHatch, Pinwheel, Wild — **9 parametric** of 34 total.

Rejected as rebrands, folkloric, or insufficient authority (do NOT add): Silverlock (= RatTrap), Cross (= EnglishCross), Gothic (= Monk in 3-period mirror), Dearnes (interpretive — no published authoritative diagram for the brick-on-edge variant), Yorkshire (no authority), HeaderGardenWall (no authority), opus latericium/testaceum (= Running), MultiRow (= Common in Russian SP 15.13330), Mughal/Persian khishti/banna'i (ornamental, not structural bonds), Sint-Andriesverband (folkloric).

## Remaining Work — Categorised by Future Folder

### `libs/csharp/Rasm.Rhino/Blocks/` — Universal block geometry (NOT brick-only)

**Purpose**: stamp any `GeometryBase` at N `Transform` placements with efficient `InstanceDefinition` reuse. Used by bricks today, by future steel sections / timber members / stone units tomorrow.

**Surface to provide (one polymorphic capsule)**:
- `Blocks.Place(GeometryBase definition, Seq<Transform> placements, BlockTarget target, ObjectAttributes? attrs) → Fin<Seq<Guid>>`
- `BlockTarget [Union] { Live(RhinoDoc) | Archive(File3dm) }` — discriminates archive-vs-live (`UpdateType`, `ArchiveFileStatus`, `UseCount` are live-doc-only per memory `reference_rhino_idef_archive_vs_live.md`)
- Internal: `InstanceDefinitionTable.Add(name, description, basePoint, geometry, attributes)` once per definition + `ObjectTable.AddInstanceObject(int idefIndex, Transform xform, ObjectAttributes attrs)` per placement
- Custom GH2 pear `InstanceReferencePear(Guid idefId, Transform xform)` + `BakeObject` override that routes through `AddInstanceObject` — **without this**, GH2 generic bake silently flattens N instances into N standalone Breps, losing the 55× memory advantage. **Verification gate before declaring `Blocks/` done**: bake 20K placements through the pear, open the resulting `.3dm`, assert `doc.InstanceDefinitions.Count == 1 && doc.Objects.OfType<InstanceObject>().Count() == 20000`.

**Memory math** (plausibility check): per-`InstanceReferenceGeometry` is `Guid + Transform` ≈ 152 bytes (Guid 16 B + Transform 128 B + ref overhead); 20,000 instances ≈ 3.0 MB + 1 × shared Brep (~10 kB) ≈ ~3.0 MB resident. Compared to 20,000 standalone Breps (~10 kB each) ≈ 200 MB — order-of-magnitude reduction.

**Verified API surfaces** (Rhino 9 WIP, RhinoCommon 2026-05-12 build):
- `Box.ToBrep()` — canonical unit construction; `MakeValid` is `[Obsolete]` 9.0
- `Brep.CreateBooleanDifferenceWithIndexMap(IEnumerable<Brep>, IEnumerable<Brep>, double tolerance, bool manifoldOnly, out int[] indexMap) → Brep[]` — `<since>8.13`, only overload with input→result map (per-brick attribute carryover)
- `InstanceReferenceGeometry(Guid instanceDefinitionId, Transform transform)` — ctor only; `Xform` and `ParentIdefId` are **read-only**; retransform = construct new instance
- `InstanceDefinitionTable.Add(name, description, basePoint, geometry, attributes)` returns `int` index (-1 on failure); `File3dmInstanceDefinitionTable.Add` mirrors for headless `File3dm` authoring
- `ObjectTable.AddInstanceObject(int idefIndex, Transform xform, ObjectAttributes attrs, HistoryRecord, bool isReference)` — `<since>6.24`, `isReference=true` for transient previews that don't persist in archives
- `RhinoObject.RenderMeshes(MeshType, ViewportInfo, List<InstanceObject>, ...)` — share one mesh across the instance ancestry for display performance
- `InstanceDefinitionTable.ModifyGeometry(int, IEnumerable<GeometryBase>, ...)` — live re-keying of a definition; every existing `InstanceObject` updates in place (useful for material-swap UX)

### `libs/csharp/Rasm/Analysis/` — Universal 2D/3D projection primitives

**Belongs in the existing geometry kernel, not `Rasm.Rhino/`**. `Rasm/` already imports `Rhino.Geometry` as a global using and `Rasm/Analysis/` already owns geometry analysis (`Faces.cs`, `Curves.cs`, `Intersect.cs`, `Spatial.cs`, `Topology.cs`). Projection is analysis algebra, not boundary/UI concern. Add a new `Rasm/Analysis/Projection.cs`.

**Surface to provide**:
- `Projection.Elevation(GeometryBase source, Plane viewPlane) → Fin<Seq<Curve>>` — silhouette + projection
- `Projection.Plan(GeometryBase source, double elevation) → Fin<Seq<Curve>>` — horizontal section
- `Projection.Section(GeometryBase source, Plane sectionPlane) → Fin<Seq<Curve>>` — vertical cut

**Verified API surfaces**:
- `Silhouette.Compute(GeometryBase, SilhouetteType, Vector3d parallelCameraDirection, double tolerance, double angleToleranceRadians, IEnumerable<Plane> clippingPlanes, CancellationToken)` — canonical elevation outline
- `Brep.CreateContourCurves(Brep, Plane sectionPlane)` — canonical section
- `Curve.ProjectToPlane(Curve, Plane)` — wrap silhouettes into the elevation/plan plane
- `BrepFace.DuplicateFace(false).ToNurbsCurve()` — per-face outlines without isocurve noise

### `libs/csharp/Rasm.Masonry/` — Wall / arch / pier assembly

**Purpose**: turn `BrickDesignation × BondName × WallGeometry` into a `Seq<PlacedBrick>` then a buildable Brep wall. Depends on `Rasm.Materials.Bricks` (read-only) + `Rasm.Rhino.Blocks` + `Rasm.Analysis.Projection` + `Rasm` (geometry kernel).

**Types it owns** (not Materials):
- `sealed record PlacedBrick(BrickDesignation Unit, Plane Frame, Orientation Orientation, Cut Cut, int SequenceId)`
- `[Union] WallSpec` — `Planar | Curved | Arch | Pier`
- `sealed record OpeningSpec(Plane Frame, double WidthMm, double HeightMm, LintelKind Lintel)`
- `[Union] LayoutError : Error` — `BondIncompatibleWithBrick(BondName, BrickDesignation)`, `WallTooShortForPatternRepeat`, `OpeningConflictsWithExpansionJoint`, `RadiusBelowVoussoirThreshold`, `UnsupportedSpecialShape`

**Operations**:
- Course transducer: walk `(course_index, bond.CourseAt(course_index).Sequence)` along wall length, emit `PlacedBrick` records
- Closure derivation: pattern-match on `bond.Closure` and wall corner geometry — `ClosureRule` `[Union]` dispatch
- Arch voussoir generation per BIA TN 31: voussoir count `N` is odd (keystone-centred); per-voussoir wedge angle = `arch_subtended_angle / N` (e.g., `π / N` radians for a semicircular arch, `arc_radians / N` generally); voussoir depth ≥ `ArchProfile.<kind>.Rule.MinDepthPerFootMm × span_ft`; joint thickness ∈ `[Rule.JointThicknessMinMm, Rule.JointThicknessMaxMm]`; keystone height ≤ `Rule.KeystoneMaxFraction × arch_depth`
- Curved-wall validation: per-brick chord-arc sagitta `s ≈ L² / (8R)` ; this drives the *head-joint taper* at the brick ends — taper magnitude grows with wall curvature; switch from rectangular bricks (uniform L) to voussoir-style tapered bricks when the resulting joint thickness exceeds `ArchRule.JointThicknessMaxMm` (typically below `R ≈ 10 × L`)

**Single polymorphic entry**: `Masonry.Generate(WallSpec spec) → Fin<MasonryAssembly>`

### `libs/csharp/Rasm.Grasshopper/Components/` — GH2 components (flat naming)

Existing `Rasm.Grasshopper/Components/` is flat — no per-domain subfolders. Future masonry components follow the same convention with name-prefixed identification:
- `MasonryWallComponent.cs`, `MasonryArchComponent.cs`, etc.

Do NOT introduce a `Components/Masonry/` subfolder — it would be the first per-domain folder in the existing flat layout and inconsistent with infrastructure files (`Bridge.cs`, `Component.cs`, `Output.cs`, `Port.cs`).

## Consumer-side Example (truthful, post-refactor)

```csharp
using static LanguageExt.Prelude;
using Rasm.Materials.Bricks;
// future:
// using Rasm.Masonry;

// Direct catalogue access — delegate properties on BrickDesignation; no .Spec. indirection
BrickRegion region = BrickDesignation.UsModular.Region;          // BrickRegion.Us
Dim3 specified = BrickDesignation.UsModular.Specified;            // 92.075 × 57.15 × 193.675
double widthMm = specified.WidthMm;
double mortarMm = BrickDesignation.UsModular.Region.StandardJointThicknessMm;  // 9.525
double courseHeightMm = BrickDesignation.UsModular.Coursing.CourseHeightMm;    // 67.7333 (already includes joint via nominal coursing)
Coring coring = BrickDesignation.UsModular.Coring;                // Coring.Cored3Hole
double voidFraction = coring.VoidFraction;                        // 0.20

// Bond course walking — direct attached properties + projection helpers
CourseTemplate row5 = BondName.English.CourseAt(courseIndex: 5);  // modulo handled inside
ClosureRule corner = BondName.English.Closure;                    // QueenCloser(InsetCourses: 1)

// Bond → brick compatibility (single call; tolerance internalized)
bool fits = BondName.Common.AcceptsAspect(brick: BrickDesignation.UsModular.Spec);

// Regional masonry policy (movement, expansion, weep, tie)
RegionalMasonryPolicy policy = BrickDesignation.UsModular.Region.Policy;
double expansionSpacingMm = policy.Expansion.SpacingFor(hasOpenings: true);  // 6096 if openings, 7620 if not

// Arch rule attached to ArchProfile
ArchRule jackRule = ArchProfile.Jack.Rule;
double maxArchJointMm = jackRule.JointThicknessMaxMm;             // 19.05 mm

// Enumerate by facet — no BrickCatalog needed
Seq<Coring> hollowCorings = toSeq(Coring.Items.Where(c => c.Classification == CoringClass.Hollow));
Seq<BrickDesignation> usBricks = BrickRegion.Us.Designations();   // extension on BrickRegion
FrozenSet<SpecialShape> specialShapes = SpecialShape.Catalog;     // static on the Union itself

// Consumer-input value construction (future GH2 component reading user parameters)
// — Dim3.TryCreate returns typed BrickFault on failure, flowing directly into Fin<T>
Fin<Dim3> userDim =
    Dim3.TryCreate(widthMm: userWidth, heightMm: userHeight, lengthMm: userLength, out Dim3 dim, out BrickFault? fault)
        ? Fin.Succ(value: dim)
        : Fin.Fail<Dim3>(error: fault!);

// Future masonry layout (Rasm.Masonry, not yet implemented)
Fin<MasonryAssembly> wall =
    from spec    in Fin.Succ(new WallSpec.Planar(Geometry: planar, Designation: BrickDesignation.UsModular, BondName: BondName.English, Joint: JointProfile.Concave))
    from result  in Masonry.Generate(spec)
    select result;
```

## Out of Scope for `Rasm.Materials.Bricks`

Do NOT add to this folder:
- Geometry generation (`Box.ToBrep`, `Brep.CreateBooleanDifference`, …) — belongs to `Rasm.Rhino.Blocks` / `Rasm.Analysis.Projection`
- Layout transducers (course walking, closure derivation, voussoir geometry) — belongs to `Rasm.Masonry`
- Grasshopper2 components / pears / goos — belongs to `Rasm.Grasshopper/Components/`
- `PlacedBrick`, `WallSpec`, `OpeningSpec`, `MasonryAssembly`, `LayoutError` — belong to `Rasm.Masonry`
- Wall/arch/pier construction logic — belongs to `Rasm.Masonry`
- Lintel structural design (load-case dependent) — out of scope for the entire material/masonry chain; consumer-supplied
- Per-manufacturer SKU / pricing / sourcing — never part of standards-based catalogue
- Mortar mix chemistry (Type N/S/M proportions) — future `Rasm.Materials.Mortars/` if needed
- Thermal / acoustic / fire-rated assembly classification — assembly-level, belongs in future `Rasm.Masonry` or building-physics modules

## Verification

`bash scripts/check-cs.sh check` — 0 warnings, 0 errors across Rasm.Materials.

## Block / Drawing Universalisation Note

The `Rasm.Rhino/Blocks/` module is NOT brick-only — it's a universal primitive for any material that ships repetitive geometry (steel I-sections, timber studs, concrete blocks, glass panels, stone units). Brick is the first consumer; the API surface should encode that intent through generic `GeometryBase` parameters, not brick-specific signatures.

The `Rasm/Analysis/Projection.cs` module is similarly universal — any 3D geometry, any view plane. When the next material lands (Steel/Concrete/Timber), the block/projection modules expand only if a NEW capability is needed (e.g., curved geometry stamping for tapered concrete) — never with material-specific overloads.
