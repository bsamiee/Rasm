# [MATERIALS_MASONRY]

THE MASONRY SEED PAGE and THE GENERATIVE BOND ALGEBRA. Masonry is one `ComponentFamily` policy row (`ComponentFamily.Masonry`: `ComponentClass.Minor`, `DetailLane.Realization`, admits `SectionProfile.Rectangle` or `SectionProfile.CellularRectangle`, cross-nominal `GrossRectangleMm.WidthMm`, rows `MasonrySeed.Rows`) — a masonry unit is a `Component` row, never a `Brick` type. This page owns the FORM-law vocabularies (`BondKind`/`BondGeometry`/`BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape`/`MortarProfile`/`MortarType`/`MortarSystem`/`RuptureModulus`/`SizeTolerance`/`SizeRange`), the `MortarJoint` joint specification, the `FrogGeometry`/`Perforation` generative void geometry, the `AUTHORED` regional row table, and the ONE `MasonrySeed.Rows : Context -> Fin<Seq<ComponentRow>>` fold `component#CATALOGUE` `ComponentCatalogue.Of` traverses. The bespoke `MasonryUnit` payload record is DELETED: its geometry is the BED-PLANE `SectionProfile.Rectangle`/`CellularRectangle` (W through-wall × L along-wall — the cmu-congruent orientation whose net area IS the TMS 402 bed section), its frog/perforation voids are TRUE-GRID `VoidCell` rows the `MasonryVoids` generator stations in that plane (every declared hole at its declared position), its vocabulary columns are the kept SmartEnums, and its `HeightMm`/`CourseHeightMm`/tolerance/range/shape realization columns are seed-row values landing in the seed-built `MasonryDetail` bag (`Masonry` widens to `DetailLane.Realization` — the EN 771-1 work-vs-actual envelope, the unit height, and the coursing module have no other landing surface). Every row seeds `Sectioned: false`: a masonry unit is a laid course, not a profiled member, so it is absent from `ComponentCatalogue.Sections`, `graph.SectionOf` resolves `None`, and `SectionSolver` never sees a masonry profile at build; the URM flexural-tension screen rides the `RuptureModulus` TMS 402 Table `9.1.9.2` row keyed by `MortarSystem`/`MortarType` into the `capacity#SECTION_CAPACITY` `MasonryCompression.FrMpa` column through the `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Masonry` case — the mortar feed off the assemblage, never `MortarType.FlexuralBondMpa` (ASTM C1072 bond data, firewalled from `fr`). The `Bond` axis is a GENERATIVE ALGEBRA over a THEOREM-CLOSED census: a `BondKind.Template` bond reads its course set by wrapped index, a `BondKind.Generated` bond IS a plane-symmetry tiling — its `BondGeometry` row names a `Rasm/Parametric/patternmap#PATTERNING` `WallpaperGroup` (the complete 17-row plane census) plus its motif seats in cell coordinates, and the FULL per-unit packing transform (offset + lateral + rotation + mirror parity) derives from the kernel `Patterning.Apply(PatternOp.Orbit)` fold read band by band, the unit's own coordinating module the cell basis — so a new decorative bond is DATA over a closed theorem, never a hand-authored per-index delegate and never an interpreter arm. The course fold, joint policy, and station projection consuming `CourseTemplate`/`UnitPlacement`/`MortarJoint` are `Rasm.Generation`; the shared `Coring` void class, `VoidCell`, `ComponentUnit`, and `ComponentDetail` bag constructors are `component#COMPONENT_OWNER`; the cmu sibling buckets onto the same `Coring` vocabulary and shares the `CellularRectangle` profile arm.

## [01]-[INDEX]

- [02]-[MASONRY_FAMILY]: the retained bond/orientation/cut/closure/special-shape/mortar/tolerance vocabularies with the wallpaper-group `BondGeometry` symmetry descriptor and the `BondName` template/generated catalogue, the `[ComplexValueObject]` `MortarJoint` ASTM C270 joint specification, the `MortarSystem` cementitious-system rows and the TMS 402 Table `9.1.9.2` `RuptureModulus` modulus-of-rupture table (the `capacity#SECTION_CAPACITY` URM flexural-tension feed), the `FrogGeometry`/`Perforation` void geometry with the ONE `MasonryVoids` owner (the bed-plane `VoidCell` grid generator + the railed ASTM C652/C216 `Coring` bucket), the EN 771-1 `SizeTolerance`/`SizeRange` work-vs-actual envelope, the `MasonryBody` substance axis with the `MasonryPhysics` receipt and the `WallAcoustics` single-leaf mass-law fold, the `AUTHORED` `MasonryRow` regional table, the seed-built `MasonryDetail` realization bag with its `Properties` seam lowering, and the `MasonrySeed.Rows` fail-loud `Traverse` fold.

## [02]-[MASONRY_FAMILY]

- Owner: the masonry vocabulary (`BondKind`, `BondGeometry`, `BondName`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`, `MortarProfile`, `MortarType`, `MortarSystem`, `RuptureModulus`, `SizeTolerance`, `SizeRange` — all FORM-law `[SmartEnum<string>]`, each stacking `[KeyMemberComparer]` beside `[KeyMemberEqualityComparer]` so ordered key lookup matches the `ComponentFamily` row convention); `UnitPlacement`/`CourseTemplate` the per-unit course transform; `MortarJoint` the generated-admission joint specification; `FrogGeometry`/`Perforation` the generative void geometry; `MasonryVoids` the `VoidCell` derivation + railed void-class bucket; `MasonryBody` the substance axis keyed on its own `MaterialId`; `MasonryDetail` the seed-time realization bag AND the seam `Properties` lowering; `MasonryRow` the `AUTHORED` raw row; `MasonrySeed` the ONE fold the `component#CATALOGUE` composes with its `ComponentId`-keyed `Table` body join.
- Cases: bond {template + generated kinds; a generated bond's `BondGeometry` descriptor names its wallpaper group and motif seats, the kernel orbit computing the full per-unit transform} · orientation {stretcher/header/soldier/sailor/rowlock/shiner, each carrying its run/rise course footprint} · cut {whole/three-quarter/half-bat/quarter-bat/queen-closer/king-closer/bevel, each carrying its cut-plane remainder + plane-normal orientation} · closure {none/queen-closer/king-closer/half-bat} · special-shape {none/bullnose/cownose/plinth/coping/cant/squint/birdsmouth/voussoir, each carrying its profile modifier + the voussoir radial taper} · mortar-profile {concave/v/weathered/struck/raked/flush/beaded/squeezed, each carrying its 3D recess cross-section} · mortar-type {M/S/N/O/K with EN 998-2 class, proportion spec, and C1072 flexural bond} · mortar-system {portland-lime/mortar-cement/masonry-cement/air-entrained-portland-lime, the `ReducedBond` Table `9.1.9.2` column-group discriminant} · rupture-modulus {8 direction×form rows × 4 published MPa columns — the TMS 402 Table `9.1.9.2` `fr` the mortar key dispatches, with the `PartialGrout` footnote interpolation} · tolerance {T1/T2/Tm} · range {R1/R2/Rm} · profile {`Rectangle` a solid unit, `CellularRectangle` a frogged/cored unit whose `VoidCell` rows the `MasonryVoids` generator derives}.
- Entry: `MasonrySeed.Rows(Context) : Fin<Seq<ComponentRow>>` — the ONE generator fold: each `MasonryRow` proves its coordinating module (`CourseMm = HMm + JointMm` within the published-rounding band), admits its four dimensional columns once through `ComponentUnit.Of`, derives its BED-PLANE profile through the railed `SectionProfile.Rectangle.Of`/`CellularRectangle.Of` factory (`MasonryVoids.Cells` supplying the true Columns × Rows hole grid and the frog pocket cell), buckets its `Coring` through `MasonryVoids.Bucket`, binds `IfcBinding.Supertype(ComponentFamily.Masonry.Class)` (masonry is a supertype family — the concrete `IfcWall`/`IfcMember` leaf is an occurrence refinement), builds its `MasonryDetail` bag at seed time, and constructs through `Component.Of` INSIDE the `Traverse` — a failed row ABORTS the build, never drops. `BondName.Course(unit, jointMm, index, key)` resolves a course template or orbits the generated descriptor's wallpaper group over the unit's coordinating module and reads the band, `BondGeometry.Courses(unit, jointMm, courses, key)` the whole-stack primary the single-index read projects; `BondName.Fits(ComponentUnit)` is the aspect-ratio tiling gate over the owner-provided `LengthOverHeight` projection; `MasonryDetail.Properties(profile, body, key)` lowers the `MasonryPhysics` receipt onto the seam `Thermal`/`Acoustic`/`Fire` cases; `SizeTolerance.WorkEnvelopeMm(workMm, declaredMm)` is the EN 771-1 actual-size envelope the coursing tolerance and GLB tessellation read off the bag inputs (`declaredMm` the manufacturer's Tm deviation, zero for the categorial classes).
- Packages: Rasm (project — `PositiveMagnitude` from `Rasm.Numerics`; `Op`/`Context`/`AcceptValidated` from `Rasm.Domain`), Rasm.Element (project — `MaterialId`, `PropertyBag`, the seam `DetailSchema`/`PropertyName`/`Dimension` currencies `MasonryDetail` composes, and the `MaterialPropertySet` its `Properties` lowering mints; every `DetailSchema.Realization` row a unit stamps is Element-declared at `property#DETAIL_SCHEMA`, never minted here), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[ComplexValueObject]` + generated `ValidateFactoryArguments`/`Validate`, `[UseDelegateFromConstructor]`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, `ComparerAccessors`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Traverse` — the fail-loud RAIL law), BCL inbox; NO `VividOrange` — masonry is `AUTHORED` under `SEED_ROW_LAW` (no admitted producer owns EN 771/ASTM C216 masonry-unit tables; every value restates verbatim with per-column provenance).
- Growth: a new template bond is one `BondName` row carrying its course set; a new generated bond is one `BondGeometry` row naming its wallpaper group and motif seats plus one `BondName` row — the symmetry census is closed by theorem, so the row is DATA and never a new derivation; a new orientation/cut/shape/mortar/tolerance row is one SmartEnum row; a new `fr` direction/form row or cementitious system is one `RuptureModulus`/`MortarSystem` row; a new regional unit is one `MasonryRow` — ZERO type edits, per `[DIFF_OF_NEXT_THING]`. A sibling family lands its own vocabulary on its own seed page.
- Boundary: this page emits profiles, vocabulary rows, bags, seam property sets, and the seed fold; it authors no host curve or IFC entity — a `BondGeometry` seat is a kernel-owned cell COORDINATE, never a transform, and the course fold that materializes one is `Rasm.Generation`'s. `MasonryVoids.Cells` stations every perforation at its declared bed-plane grid position and preserves void area through an area-equivalent square cell, and `MasonryVoids.Bucket` rails a derived fraction outside `[0,1)` on `ComponentFault.Coring`. `Coring` therefore matches the published volume fraction; circular-hole local inertia remains bounded by the explicit square-equivalent section convention rather than being described as exact circular geometry. The kernel orbit is composed at ONE site (`BondGeometry.Courses`) and its `2449` plane-pattern fault maps ONCE onto `ComponentFault.Bond` with the kernel message preserved, so the page keeps its single band.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                // FrozenDictionary — the MasonrySeed.Table ComponentId-keyed body join
using LanguageExt;                              // Fin, Option, Seq, Traverse
using Rasm.Numerics;                             // PositiveMagnitude — the kernel value-object atoms live in Rasm.Numerics, NOT Rasm.Domain
using Rasm.Domain;                              // Context, Op, AcceptValidated
using Rasm.Element.Composition;                             // MaterialId, PropertyBag, DetailSchema, Dimension (the seam bag currencies MasonryDetail composes)
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis — disambiguated from the Rasm.Numerics discrete count
using Rasm.Parametric;                          // WallpaperGroup, PatternPlan, PatternOp, Patterning, InstanceStream, TangentLogMapAlgorithm — the theorem-closed plane-symmetry orbit the generated bonds compose
using Rhino.Geometry;                           // Point2d, Vector2d — the kernel plan's own cell-coordinate and basis currency (a bond carries no host curve or transform)
using Thinktecture;                             // [SmartEnum]/[ComplexValueObject]/[UseDelegateFromConstructor]/[KeyMemberEqualityComparer]/[KeyMemberComparer], ComparerAccessors
using static LanguageExt.Prelude;               // Seq, toSeq, Some, None

// This page DEFINES the masonry vocabulary the cmu sibling and the generation spec import as
// Rasm.Materials.Component; owner types resolve by bare name in the shared namespace. Coring/CoringClass and
// VoidCell are component#COMPONENT_OWNER's — masonry derives cells and buckets the class, owning only the
// frog/perforation GEOMETRY.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondKind {
    public static readonly BondKind Template = new("template");
    public static readonly BondKind Generated = new("generated");
}

// The brick laying-orientation — which face shows and how the unit consumes the course run/rise. RunFraction is
// the per-unit along-wall advance and RiseFraction the course height, both DEFINED multiples of the base stretcher
// slot under the ideal L=2W=3H coordination module: a header shows W along (half slot); a soldier stands L vertical
// with H along (third-slot advance, tripled rise); a sailor stands L vertical with W along; a rowlock/shiner lie on
// edge with W vertical (1.5 rise). FaceShown names the exposed brick face {stretcher, header, bed} the weathering
// engine reads — a soldier presents the stretcher face, a sailor/shiner the bed, a header/rowlock the header face.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Orientation {
    public static readonly Orientation Stretcher = new("stretcher", runFraction: 1.0,       riseFraction: 1.0, faceShown: "stretcher");
    public static readonly Orientation Header    = new("header",    runFraction: 0.5,       riseFraction: 1.0, faceShown: "header");
    public static readonly Orientation Soldier   = new("soldier",   runFraction: 1.0 / 3.0, riseFraction: 3.0, faceShown: "stretcher");
    public static readonly Orientation Sailor    = new("sailor",    runFraction: 0.5,       riseFraction: 3.0, faceShown: "bed");
    public static readonly Orientation Rowlock   = new("rowlock",   runFraction: 1.0 / 3.0, riseFraction: 1.5, faceShown: "header");
    public static readonly Orientation Shiner    = new("shiner",    runFraction: 1.0,       riseFraction: 1.5, faceShown: "bed");
    public double RunFraction { get; }
    public double RiseFraction { get; }
    public string FaceShown { get; }
}

// The field cut at a placement — the remaining footprint AND the cut-plane orientation, never a bare length
// scalar. LengthFraction is the transverse-bat remainder, WidthFraction the longitudinal-split remainder, and
// PlaneNormalDegrees the cut-plane normal off the transverse axis (0 a straight bat, 90 a queen-closer split,
// intermediate a diagonal king-closer/bevel). The spec course fold places closures and arch-head cuts from these.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Cut {
    public static readonly Cut Whole        = new("whole",         lengthFraction: 1.000, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut ThreeQuarter = new("three-quarter", lengthFraction: 0.750, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut Half         = new("half-bat",      lengthFraction: 0.500, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut Quarter      = new("quarter-bat",   lengthFraction: 0.250, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut QueenCloser  = new("queen-closer",  lengthFraction: 1.000, widthFraction: 0.50, planeNormalDegrees: 90.0);
    public static readonly Cut KingCloser   = new("king-closer",   lengthFraction: 0.750, widthFraction: 1.00, planeNormalDegrees: 45.0);
    public static readonly Cut Bevel        = new("bevel",         lengthFraction: 1.000, widthFraction: 1.00, planeNormalDegrees: 30.0);
    public double LengthFraction { get; }
    public double WidthFraction { get; }
    public double PlaneNormalDegrees { get; }
    public bool Angled => PlaneNormalDegrees is > 0.0 and < 90.0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClosureRule {
    public static readonly ClosureRule None        = new("none",         closer: Cut.Whole);
    public static readonly ClosureRule QueenCloser = new("queen-closer", closer: Cut.QueenCloser);
    public static readonly ClosureRule KingCloser  = new("king-closer",  closer: Cut.KingCloser);
    public static readonly ClosureRule HalfBat     = new("half-bat",     closer: Cut.Half);
    public Cut Closer { get; }
}

// The BS 4729 special-shape vocabulary — each row carries its profile-modifier geometry, never a bare identity
// tag: RadiusFraction rounds an arris (bullnose single, cownose double), ChamferDegrees cuts a splay
// (cant/squint/birdsmouth), SetbackFraction steps the face (plinth), WashFraction slopes the top (coping),
// TaperDegrees the VOUSSOIR radial bed-face convergence. The spec arch fold reads Voussoir by identity AND
// TaperDegrees for the manufactured wedge; the arch sweep (radius/springing/count) is the spec's concern.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpecialShape {
    public static readonly SpecialShape None       = new("none",       radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Bullnose   = new("bullnose",   radiusFraction: 0.50, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Cownose    = new("cownose",    radiusFraction: 1.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Plinth     = new("plinth",     radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.25, washFraction: 0.20, taperDegrees: 0.0);
    public static readonly SpecialShape Coping     = new("coping",     radiusFraction: 0.10, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.30, taperDegrees: 0.0);
    public static readonly SpecialShape Cant       = new("cant",       radiusFraction: 0.00, chamferDegrees: 45.0, setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Squint     = new("squint",     radiusFraction: 0.00, chamferDegrees: 30.0, setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Birdsmouth = new("birdsmouth", radiusFraction: 0.00, chamferDegrees: 60.0, setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Voussoir   = new("voussoir",   radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 5.0);
    public double RadiusFraction { get; }
    public double ChamferDegrees { get; }
    public double SetbackFraction { get; }
    public double WashFraction { get; }
    public double TaperDegrees { get; }
    public bool ModifiesProfile => RadiusFraction > 0.0 || ChamferDegrees > 0.0 || SetbackFraction > 0.0 || WashFraction > 0.0 || TaperDegrees > 0.0;
}

// The ASTM C270 tooled-joint profile carrying its 3D RECESS cross-section: DepthFactor scales bed width to the
// signed recess depth (+ recessed, - projecting, flush 0); RecessShape names the extruded cross-section;
// SlopeDegrees the sloped face angle (+ out-down weathered sheds, - in-down struck ledges); ShadowLine the
// weathering ambient-occlusion weight. Beaded/squeezed project water-ledging arrises — NOT weather-tight; the
// squeezed case is the single extruded/weeping/overhung untooled joint. No ASTM C1314 prism case exists (a
// compression-test specimen, never a finished joint).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarProfile {
    public static readonly MortarProfile Concave   = new("concave",   depthFactor: 0.10,  shadowLine: 0.3, recessShape: "concave-arc", slopeDegrees: 0.0,   weatherTight: true);
    public static readonly MortarProfile Vee       = new("v",         depthFactor: 0.15,  shadowLine: 0.5, recessShape: "vee-groove",  slopeDegrees: 0.0,   weatherTight: true);
    public static readonly MortarProfile Weathered = new("weathered", depthFactor: 0.20,  shadowLine: 0.4, recessShape: "sloped",      slopeDegrees: 15.0,  weatherTight: true);
    public static readonly MortarProfile Struck    = new("struck",    depthFactor: 0.20,  shadowLine: 0.6, recessShape: "sloped",      slopeDegrees: -15.0, weatherTight: false);
    public static readonly MortarProfile Raked     = new("raked",     depthFactor: 0.50,  shadowLine: 1.0, recessShape: "rectangular", slopeDegrees: 0.0,   weatherTight: false);
    public static readonly MortarProfile Flush     = new("flush",     depthFactor: 0.00,  shadowLine: 0.0, recessShape: "flat",        slopeDegrees: 0.0,   weatherTight: true);
    public static readonly MortarProfile Beaded    = new("beaded",    depthFactor: -0.10, shadowLine: 0.7, recessShape: "convex-bead", slopeDegrees: 0.0,   weatherTight: false);
    public static readonly MortarProfile Squeezed  = new("squeezed",  depthFactor: -0.20, shadowLine: 0.8, recessShape: "extruded",    slopeDegrees: 0.0,   weatherTight: false);
    public double DepthFactor { get; }
    public double ShadowLine { get; }
    public string RecessShape { get; }
    public double SlopeDegrees { get; }
    public bool WeatherTight { get; }
}

// The ASTM C270 mortar classification, full row not a bare compressive scalar — Table 2 28-day minimum
// compressive strength, the mapped EN 998-2 M-class, the cement:lime:sand proportion-spec volumes, and the ASTM
// C1072 minimum flexural tensile BOND. FlexuralBondMpa is spec/submittal-lane data (the CmuDensity
// MaxAbsorptionKgPerM3 posture) — NEVER the TMS 402 modulus of rupture fr; the capacity#SECTION_CAPACITY feed is
// the RuptureModulus table below, this row only the mortar-type KEY it dispatches on. PUBLISHED.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarType {
    public static readonly MortarType M = new("M", compressiveMpa: 17.2, enClassMpa: 15.0, cement: 1.0, lime: 0.25, sand: 3.50, flexuralBondMpa: 0.40);
    public static readonly MortarType S = new("S", compressiveMpa: 12.4, enClassMpa: 10.0, cement: 1.0, lime: 0.50, sand: 4.50, flexuralBondMpa: 0.30);
    public static readonly MortarType N = new("N", compressiveMpa: 5.2,  enClassMpa: 5.0,  cement: 1.0, lime: 1.00, sand: 6.00, flexuralBondMpa: 0.20);
    public static readonly MortarType O = new("O", compressiveMpa: 2.4,  enClassMpa: 2.5,  cement: 1.0, lime: 2.00, sand: 9.00, flexuralBondMpa: 0.10);
    public static readonly MortarType K = new("K", compressiveMpa: 0.5,  enClassMpa: 1.0,  cement: 1.0, lime: 3.00, sand: 12.0, flexuralBondMpa: 0.05);
    public double CompressiveMpa { get; }
    public double EnClassMpa { get; }
    public double Cement { get; }
    public double Lime { get; }
    public double Sand { get; }
    public double FlexuralBondMpa { get; }
}

// The ASTM C270 cementitious SYSTEM — the TMS 402 Table 9.1.9.2 column-group discriminant, orthogonal to the M/S/N
// strength type (a Type S exists in all four systems). ReducedBond maps the four systems onto the table's two column
// groups: masonry-cement and air-entrained portland-lime carry the reduced-bond columns.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarSystem {
    public static readonly MortarSystem PortlandLime             = new("portland-lime",              reducedBond: false);
    public static readonly MortarSystem MortarCement             = new("mortar-cement",              reducedBond: false);
    public static readonly MortarSystem MasonryCement            = new("masonry-cement",             reducedBond: true);
    public static readonly MortarSystem AirEntrainedPortlandLime = new("air-entrained-portland-lime", reducedBond: true);
    public bool ReducedBond { get; }
}

// TMS 402 Table 9.1.9.2 modulus of rupture fr, PUBLISHED psi→MPa: span direction × unit/grout form ROWS × mortar
// system-group COLUMNS (portland-lime/mortar-cement group M-or-S · N | masonry-cement/air-entrained group M-or-S · N).
// Type O/K sit outside the structural tables — FrMpa 0.0, so any net tension governs outright; stack-other prints 0
// (tension normal to a continuous head-joint plane has no bond path). MortarType.FlexuralBondMpa (ASTM C1072
// unit-mortar bond, 0.05-0.40 MPa) is NEVER this fr. A new fr row or mortar system is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuptureModulus {
    public static readonly RuptureModulus NormalSolid              = new("normal-solid",               pclMsMpa: 0.917, pclNMpa: 0.689, mcMsMpa: 0.552, mcNMpa: 0.352);
    public static readonly RuptureModulus NormalHollowUngrouted    = new("normal-hollow-ungrouted",    pclMsMpa: 0.579, pclNMpa: 0.441, mcMsMpa: 0.352, mcNMpa: 0.214);
    public static readonly RuptureModulus NormalHollowGrouted      = new("normal-hollow-grouted",      pclMsMpa: 1.124, pclNMpa: 1.089, mcMsMpa: 1.055, mcNMpa: 1.000);
    public static readonly RuptureModulus ParallelRunningSolid     = new("parallel-running-solid",     pclMsMpa: 1.841, pclNMpa: 1.379, mcMsMpa: 1.103, mcNMpa: 0.689);
    public static readonly RuptureModulus ParallelRunningUngrouted = new("parallel-running-ungrouted", pclMsMpa: 1.151, pclNMpa: 0.876, mcMsMpa: 0.689, mcNMpa: 0.441);   // the printed row also covers partially grouted
    public static readonly RuptureModulus ParallelRunningGrouted   = new("parallel-running-grouted",   pclMsMpa: 1.841, pclNMpa: 1.379, mcMsMpa: 1.103, mcNMpa: 0.689);
    public static readonly RuptureModulus StackContinuousGrout     = new("stack-continuous-grout",     pclMsMpa: 2.310, pclNMpa: 2.310, mcMsMpa: 2.310, mcNMpa: 2.310);
    public static readonly RuptureModulus StackOther               = new("stack-other",                pclMsMpa: 0.0,   pclNMpa: 0.0,   mcMsMpa: 0.0,   mcNMpa: 0.0);
    public double PclMsMpa { get; }
    public double PclNMpa { get; }
    public double McMsMpa { get; }
    public double McNMpa { get; }

    // The mortar-keyed fr read through the generated exhaustive Switch (the cmu#CMU_FAMILY CmuStrength.RequiredUnitMpa
    // pattern — a new MortarType row breaks HERE at compile time, never an ==-chain a row falls past).
    public double FrMpa(MortarSystem system, MortarType mortar) => mortar.Switch(
        state: (Owner: this, System: system),
        m: static x => x.System.ReducedBond ? x.Owner.McMsMpa : x.Owner.PclMsMpa,
        s: static x => x.System.ReducedBond ? x.Owner.McMsMpa : x.Owner.PclMsMpa,
        n: static x => x.System.ReducedBond ? x.Owner.McNMpa : x.Owner.PclNMpa,
        o: static _ => 0.0,
        k: static _ => 0.0);

    // The TMS footnote's partial-grout linear interpolation between the two normal-hollow rows; the fraction is the
    // lattice-honest cmu#CMU_FAMILY CmuPhysics.GroutedCellFraction.
    public static double PartialGrout(double groutedCellFraction, MortarSystem system, MortarType mortar) =>
        NormalHollowUngrouted.FrMpa(system, mortar)
            + (NormalHollowGrouted.FrMpa(system, mortar) - NormalHollowUngrouted.FrMpa(system, mortar)) * Math.Clamp(groutedCellFraction, 0.0, 1.0);
}

// EN 771-1 mean-dimension tolerance: the permissible deviation of the MEAN actual size from the work size, the
// greater of a floor and a square-root-scaled term (DEFINED — the standard's own formula), verified by EN 772-16.
// Tm is manufacturer-declared (zero floor/coefficient): the declared deviation enters as declaredMm — spec/seed DATA
// for a Tm unit, zero for the categorial classes, so ONE formula owns all three rows. WorkEnvelopeMm is the
// as-manufactured envelope the coursing tolerance and the GLB tessellation read off the bag inputs.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SizeTolerance {
    public static readonly SizeTolerance T1 = new("T1", floorMm: 3.0, sqrtCoefficient: 0.40);
    public static readonly SizeTolerance T2 = new("T2", floorMm: 2.0, sqrtCoefficient: 0.25);
    public static readonly SizeTolerance Tm = new("Tm", floorMm: 0.0, sqrtCoefficient: 0.00);
    public double FloorMm { get; }
    public double SqrtCoefficient { get; }
    public double MeanDeviationMm(double workMm, double declaredMm = 0.0) =>
        Math.Max(Math.Max(FloorMm, declaredMm), SqrtCoefficient * Math.Sqrt(Math.Max(0.0, workMm)));
    public (double LowMm, double HighMm) WorkEnvelopeMm(double workMm, double declaredMm = 0.0) =>
        (workMm - MeanDeviationMm(workMm, declaredMm), workMm + MeanDeviationMm(workMm, declaredMm));
}

// EN 771-1 range category: the permissible RANGE (largest minus smallest) of a delivery, the batch-uniformity
// bound governing coursing consistency (a high range steps the bed joints over a long elevation). DEFINED formula.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SizeRange {
    public static readonly SizeRange R1 = new("R1", sqrtCoefficient: 0.60);
    public static readonly SizeRange R2 = new("R2", sqrtCoefficient: 0.30);
    public static readonly SizeRange Rm = new("Rm", sqrtCoefficient: 0.00);
    public double SqrtCoefficient { get; }
    public double PermittedRangeMm(double workMm) => SqrtCoefficient * Math.Sqrt(Math.Max(0.0, workMm));
}

// The unit-body substance axis — the physics columns the MasonryRow keys by body, never per-row literals: EN 1745 tabulated
// gross dry density, design conductivity, specific heat, and EN ISO 13788 vapour-resistance factor for the two seeded
// bodies, the IBC equivalent thickness for a 1-hour rating (fired clay the Table 722.4.1(1) solid-brick 68.6 mm; calcium
// silicate rated under the siliceous-aggregate column 71.1 mm — the same (te/cn)^1.7 power law the cmu sibling runs), and
// the MaterialId both component slots bind (a glazed unit splits them at its own row). The KEY IS the substance id: it
// resolves the Properties/properties#MATERIAL_PROPERTY_CATALOGUE and Properties/sustainability#SUSTAINABILITY_PROPERTY
// rows verbatim, so every physics column here reads byte-identically to the catalogue row it keys — a divergent design λ
// between the two owners would lower two conflicting Thermal cases onto one MaterialId. A new body — AAC, adobe,
// concrete brick — is one row plus its catalogue pair.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MasonryBody {
    public static readonly MasonryBody FiredClay       = new("masonry.clay",             densityKgM3: 1800.0, conductivityWmK: 0.77, specificHeatJKgK: 1000.0, vapourMu: 16.0, eqThick1HrMm: 68.6);
    public static readonly MasonryBody CalciumSilicate = new("masonry.calciumsilicate",  densityKgM3: 1800.0, conductivityWmK: 1.00, specificHeatJKgK: 1000.0, vapourMu: 15.0, eqThick1HrMm: 71.1);
    public double DensityKgM3 { get; }
    public double ConductivityWmK { get; }
    public double SpecificHeatJKgK { get; }
    public double VapourMu { get; }
    public double EqThick1HrMm { get; }
    public MaterialId Material => MaterialId.Of(Key);
}

// The generative bond descriptor: a decorative bond IS a plane-symmetry tiling, so the row is its WALLPAPER GROUP
// (the kernel's theorem-closed 17-row census — the complete plane census admits no 18th), its motif SEATS in cell
// coordinates [0,1)² with each seat's own spin and laid face, and the unit aspect band the cell tiles. The course
// derivation is the kernel Patterning.Apply(PatternOp.Orbit) fold read band by band — Orbit takes NO surface (pure
// plane algebra) and already emits position, spin, seat ordinal, and mirror parity — so the six hand-authored
// per-index offset/lateral/rotation delegates are the DELETED form and a new decorative bond is DATA over a closed
// theorem. The group's own PatternLattice row proves the basis pair, so a unit whose coordinating module cannot
// carry the group's lattice faults at the plan rather than tiling wrong.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondGeometry {
    public static readonly BondGeometry Stack       = new("stack",       WallpaperGroup.Pmm, aspectLo: 0.10, aspectHi: 8.0,  seats: Motif(Seat(0.50, 0.50, 0.0, Orientation.Stretcher)));
    public static readonly BondGeometry Flemish     = new("flemish",     WallpaperGroup.Cmm, aspectLo: 1.50, aspectHi: 5.0,  seats: Motif(Seat(0.25, 0.50, 0.0, Orientation.Stretcher), Seat(0.75, 0.50, 0.0, Orientation.Header)));
    public static readonly BondGeometry Herringbone = new("herringbone", WallpaperGroup.Pgg, aspectLo: 1.60, aspectHi: 2.40, seats: Motif(Seat(0.25, 0.25, QuarterPi, Orientation.Stretcher)));
    public static readonly BondGeometry Basketweave = new("basketweave", WallpaperGroup.P4g, aspectLo: 1.60, aspectHi: 2.40, seats: Motif(Seat(0.25, 0.50, 0.0, Orientation.Stretcher), Seat(0.75, 0.50, HalfPi, Orientation.Stretcher)));
    public static readonly BondGeometry Pinwheel    = new("pinwheel",    WallpaperGroup.P4,  aspectLo: 1.40, aspectHi: 3.0,  seats: Motif(Seat(0.30, 0.50, 0.0, Orientation.Stretcher), Seat(0.50, 0.30, HalfPi, Orientation.Header)));
    public static readonly BondGeometry Diaper      = new("diaper",      WallpaperGroup.Cmm, aspectLo: 1.60, aspectHi: 2.40, seats: Motif(Seat(0.25, 0.25, QuarterPi, Orientation.Stretcher)));

    // The kernel's spin column is RADIANS (the seat spin is the atan2 of the Seitz linear part), so the two turned
    // seats spell their angles in radians and the placement projection converts once at the CourseTemplate edge.
    const double QuarterPi = Math.PI / 4.0;
    const double HalfPi = Math.PI / 2.0;
    // The orbit disc must reach the far corner of the requested course band, so the fill radius is the band diagonal
    // plus one cell of margin — an edge seat clipped by the disc would drop a unit the course genuinely lays.
    const double ExtentMarginCells = 1.0;

    public WallpaperGroup Group { get; }
    public Arr<(Point2d Site, double Spin, Orientation Face)> Seats { get; }
    public double AspectLo { get; }
    public double AspectHi { get; }

    public bool Admits(double lengthOverHeight) => lengthOverHeight >= AspectLo && lengthOverHeight <= AspectHi;

    // The ONE course derivation: the coordinating module IS the cell basis in the kernel plan's root-tangent METERS
    // (A the along-course advance length + head joint, B the course rise height + bed joint, the group's own lattice
    // row proving the orthogonal pair), the motif seats ride as the plan Anchors, and the planar orbit is read band
    // by band — band c is the half-open rise interval [c·B, (c+1)·B). Every course of a wall comes from ONE orbit, so
    // the whole-stack entry is the primary and the single-index read is its projection (MODAL_ARITY). The Algorithm
    // column is unread on the Orbit arm (no surface crosses), so it carries the interactive grade.
    public Fin<Seq<CourseTemplate>> Courses(ComponentUnit unit, double jointMm, int courses, Op key) =>
        from plan in Fin.Succ(Plan(unit, jointMm, courses))
        from stream in Patterning.Apply(new PatternOp.Orbit(plan), key)
            .MapFail(fault => (Error)ComponentFault.Bond(key, $"<bond-orbit:{Key}:{Group.Key}:{fault.Message}>"))
        from bands in stream.Switch(
            planar: p => Fin.Succ(Bands(p, plan, courses)),
            mapped: _ => Fin.Fail<Seq<CourseTemplate>>(ComponentFault.Bond(key, $"<bond-orbit-mapped-stream:{Key}>")))
        select bands;

    public Fin<CourseTemplate> Course(ComponentUnit unit, double jointMm, int index, Op key) =>
        Courses(unit, jointMm, Math.Abs(index) + 1, key).Bind(stack => stack.IsEmpty
            ? Fin.Fail<CourseTemplate>(ComponentFault.Bond(key, $"<bond-course-stack-empty:{Key}:{index}>"))
            : Fin.Succ(stack[((index % stack.Count) + stack.Count) % stack.Count]));

    // The coordinating module IS the cell basis in the kernel plan's root-tangent METRES: A the along-course advance
    // (length + head joint), B the course rise (height + bed joint). The disc extent reaches the far corner of the
    // requested band plus one cell of margin, so an edge seat is never clipped out of a course that genuinely lays it.
    PatternPlan Plan(ComponentUnit unit, double jointMm, int courses) =>
        new(Group,
            new Vector2d((unit.LengthMm.Value + jointMm) * 1e-3, 0.0),
            new Vector2d(0.0, (unit.HeightMm.Value + jointMm) * 1e-3),
            Seats.Map(static s => (s.Site, s.Spin)),
            Math.Sqrt(Math.Pow((unit.LengthMm.Value + jointMm) * 1e-3, 2.0) + Math.Pow((unit.HeightMm.Value + jointMm) * 1e-3 * courses, 2.0))
                + (ExtentMarginCells * Math.Max(unit.LengthMm.Value, unit.HeightMm.Value) * 1e-3),
            new Point2d(0.0, 0.0), TangentLogMapAlgorithm.VectorHeatApproximate);

    // The band projection: sites in a course's rise interval order along the advance axis, each site's residual
    // beyond its whole-unit step the along offset, its residual within the band the lateral offset, its radian spin
    // the placement rotation, its seat parity the mirror the glide-bearing groups (pgg/p4g/cmm) emit and a
    // hand-authored delegate could never express. The course offset is the leading site's own along residual, so the
    // running-bond half-unit shift is a DERIVED read of the orbit rather than a stored per-index fraction, and an
    // empty band (a group whose seats miss the interval) reads as an empty course rather than a fabricated unit.
    Seq<CourseTemplate> Bands(InstanceStream.Planar planar, PatternPlan plan, int courses) =>
        toSeq(Enumerable.Range(0, courses))
            .Map(band => Placed(planar, plan, band))
            .Map(static units => new CourseTemplate(units, units.Head.Map(static u => u.AlongFraction).IfNone(0.0)));

    Seq<UnitPlacement> Placed(InstanceStream.Planar planar, PatternPlan plan, int band) =>
        toSeq(Enumerable.Range(0, planar.Site.Count)
                .Where(i => planar.Site[i].Y >= band * plan.BasisB.Y && planar.Site[i].Y < (band + 1) * plan.BasisB.Y)
                .OrderBy(i => planar.Site[i].X))
            .Map(i => new UnitPlacement(
                Seats[planar.Anchor[i]].Face,
                Residual(planar.Site[i].X, plan.BasisA.X),
                (planar.Site[i].Y - (band * plan.BasisB.Y)) / plan.BasisB.Y,
                planar.Spin[i] * (180.0 / Math.PI),
                planar.Mirrored[i]));

    static double Residual(double along, double advance) => (along / advance) - Math.Floor(along / advance);

    static Arr<(Point2d Site, double Spin, Orientation Face)> Motif(params ReadOnlySpan<(Point2d Site, double Spin, Orientation Face)> seats) => new([.. seats]);

    static (Point2d Site, double Spin, Orientation Face) Seat(double u, double v, double spin, Orientation face) => (new Point2d(u, v), spin, face);
}

// The bond catalogue: template bonds carry an explicit course set (single-unit OR multi-unit cells — monk proves
// the mixed cell); generated bonds reference a BondGeometry whose own delegate computes the full per-unit course.
// The header-family classics (header, English garden wall 3S+1H, monk 2S+1H) are template DATA rows.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondName {
    public static readonly BondName Running       = new("running",             template: Seq(StretcherCourse(0.0), StretcherCourse(0.5)));
    public static readonly BondName English       = new("english",             template: Seq(StretcherCourse(0.0), HeaderCourse(0.5)));
    public static readonly BondName Header        = new("header",              template: Seq(HeaderCourse(0.0), HeaderCourse(0.25)));
    public static readonly BondName EnglishGarden = new("english-garden-wall", template: Seq(StretcherCourse(0.0), StretcherCourse(0.5), StretcherCourse(0.0), HeaderCourse(0.25)));
    public static readonly BondName Monk          = new("monk",                template: Seq(MonkCourse(0.0), MonkCourse(0.25)));
    public static readonly BondName Stack         = new("stack",          generated: BondGeometry.Stack);
    public static readonly BondName Flemish       = new("flemish",        generated: BondGeometry.Flemish);
    public static readonly BondName Herringbone45 = new("herringbone-45", generated: BondGeometry.Herringbone);
    public static readonly BondName Basketweave   = new("basketweave",    generated: BondGeometry.Basketweave);
    public static readonly BondName Pinwheel      = new("pinwheel",       generated: BondGeometry.Pinwheel);
    public static readonly BondName Diaper        = new("diaper",         generated: BondGeometry.Diaper);

    public BondKind Kind { get; }
    public Seq<CourseTemplate> Courses { get; }
    public Option<BondGeometry> Geometry { get; }

    private BondName(string key, Seq<CourseTemplate> template) : this(key) => (Kind, Courses, Geometry) = (BondKind.Template, template, None);
    private BondName(string key, BondGeometry generated) : this(key) => (Kind, Courses, Geometry) = (BondKind.Generated, Seq<CourseTemplate>(), Some(generated));

    // The real aspect-ratio tiling gate: a template bond admits any positive-height unit; a generated bond admits
    // only a unit whose length-over-HEIGHT lies in the descriptor's band (a herringbone rejects a near-square unit
    // too square to tile its diagonal cell). Reads the owner-provided ComponentUnit.LengthOverHeight projection.
    public bool Fits(ComponentUnit unit) =>
        Geometry.Match(Some: g => g.Admits(unit.LengthOverHeight), None: () => true);

    // A template bond reads its course by wrapped index; a generated bond orbits its wallpaper group over the unit's
    // own coordinating module and reads the band. Both arms take the unit and joint because a generated course is a
    // FUNCTION of the module — the prior index-only entry could only answer a module-free per-index offset.
    public Fin<CourseTemplate> Course(ComponentUnit unit, double jointMm, int index, Op key) => Kind.Switch(
        state: (Owner: this, Unit: unit, JointMm: jointMm, Index: index, Key: key),
        template: static x => x.Owner.Courses.IsEmpty
            ? Fin.Fail<CourseTemplate>(ComponentFault.Bond(x.Key, $"<template-bond-empty:{x.Owner.Key}>"))
            : Fin.Succ(x.Owner.Courses[((x.Index % x.Owner.Courses.Count) + x.Owner.Courses.Count) % x.Owner.Courses.Count]),
        generated: static x => x.Owner.Geometry
            .ToFin(ComponentFault.Bond(x.Key, $"<generated-bond-missing-geometry:{x.Owner.Key}>"))
            .Bind(geometry => geometry.Course(x.Unit, x.JointMm, x.Index, x.Key)));

    static CourseTemplate StretcherCourse(double courseOffset) => new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0, false)), courseOffset);
    static CourseTemplate HeaderCourse(double courseOffset) => new(Seq(new UnitPlacement(Orientation.Header, 0.0, 0.0, 0.0, false)), courseOffset);
    static CourseTemplate MonkCourse(double courseOffset) => new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0, false), new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0, false), new UnitPlacement(Orientation.Header, 0.0, 0.0, 0.0, false)), courseOffset);
}

// --- [MODELS] ------------------------------------------------------------------------------
// One unit's placement within a course cell: orientation, additional along-course offset beyond the natural
// consecutive step, across-course lateral offset, in-plane rotation, and the handedness parity a glide or mirror seat
// carries (a frogged or perforated unit is not mirror-symmetric, so the parity is load-bearing geometry the course
// fold applies — a template bond lays every unit unmirrored). The spec course fold consumes these.
public readonly record struct UnitPlacement(Orientation Orientation, double AlongFraction, double LateralFraction, double RotationDegrees, bool Mirrored);

// One course's full per-unit transform plus the course-level horizontal offset (the running-bond half-unit shift).
public sealed record CourseTemplate(Seq<UnitPlacement> Units, double CourseOffsetFraction);

// The frog: a bed-face indentation pressed into a solid unit (the mortar key). VoidFraction is DEFINED geometry —
// pocket depth over height times bed-face footprint, doubled for a double frog.
public readonly record struct FrogGeometry(double DepthMm, double LengthFraction, double WidthFraction, double TaperDegrees, bool Double) {
    public static readonly FrogGeometry None = new(0.0, 0.0, 0.0, 0.0, false);
    public bool Present => DepthMm > 0.0 && LengthFraction > 0.0 && WidthFraction > 0.0;
    public double VoidFraction(double heightMm) =>
        Present && heightMm > 0.0 ? DepthMm / heightMm * LengthFraction * WidthFraction * (Double ? 2.0 : 1.0) : 0.0;
}

// The through-perforation grid: Columns x Rows circular holes pierced full-height through the bed faces —
// Columns station along the unit LENGTH, Rows across the through-wall WIDTH. VoidFraction is DEFINED geometry —
// the hole-grid area over the bed face (full-height holes make the area fraction the volume fraction, exactly the
// fraction the MasonryVoids bed-plane cells net).
public readonly record struct Perforation(int Columns, int Rows, double HoleDiameterMm, double EdgeMarginMm) {
    public static readonly Perforation None = new(0, 0, 0.0, 0.0);
    public int HoleCount => Math.Max(0, Columns) * Math.Max(0, Rows);
    public bool Present => HoleCount > 0 && HoleDiameterMm > 0.0;
    public double VoidFraction(double lengthMm, double widthMm) =>
        Present && lengthMm > 0.0 && widthMm > 0.0 ? HoleCount * Math.PI * HoleDiameterMm * HoleDiameterMm / 4.0 / (lengthMm * widthMm) : 0.0;
}

// The full mortar-joint specification the spec [05] joint policy resolves head/bed width AND 3D recess AND
// mortar strength from — never a single scalar thickness. GENERATED admission ([ComplexValueObject]): the
// validation partial owns the positive-finite head/bed guard, the ONE railed Of lifts the generated outcome onto
// ComponentFault.Mortar (the joint-SPEC fault, disjoint from ComponentFault.Bond the course-pattern fault).
[ComplexValueObject]
public readonly partial struct MortarJoint {
    public double HeadWidthMm { get; }
    public double BedWidthMm { get; }
    public MortarProfile Profile { get; }
    public MortarType Mortar { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double headWidthMm, ref double bedWidthMm, ref MortarProfile profile, ref MortarType mortar) =>
        validationError = double.IsFinite(headWidthMm) && headWidthMm > 0.0 && double.IsFinite(bedWidthMm) && bedWidthMm > 0.0
            ? null
            : new ValidationError($"<mortar-joint-nonpositive:head={headWidthMm}:bed={bedWidthMm}>");

    public static Fin<MortarJoint> Of(double headMm, double bedMm, MortarProfile profile, MortarType mortar, Op key) =>
        Validate(headMm, bedMm, profile, mortar, out MortarJoint joint) is { } error
            ? Fin.Fail<MortarJoint>(ComponentFault.Mortar(key, error.Message))
            : Fin.Succ(joint);

    // The coordinating joint from a single thickness — the default an unspecified run resolves (concave / Type-N).
    // RAILED through the ONE Of: the fallback thickness is caller DATA (ComponentStandard.StandardJointThicknessMm
    // is 0.0 for every non-coursing family), so the generated throwing Create never sees an unproven value.
    public static Fin<MortarJoint> Standard(double thicknessMm, Op key) => Of(thicknessMm, thicknessMm, MortarProfile.Concave, MortarType.N, key);

    // The signed 3D recess depth the joint solid extrudes over Profile.RecessShape/SlopeDegrees.
    public double RecessDepthMm => BedWidthMm * Profile.DepthFactor;
}

// The AUTHORED regional raw row (SEED_ROW_LAW: no admitted producer owns EN 771/ASTM C216 masonry tables; every
// column PUBLISHED verbatim from the named standard, the void geometry the standard's printed core/frog pattern).
// Body is the typed substance axis carrying density/conductivity/fire and the MaterialId both slots bind (base
// render and intrinsic mechanical material coincide; a glazed unit splits them). Region is explicit because
// din/au/is rows carry a region the bounded ComponentAuthority does not name.
public readonly record struct MasonryRow(
    string Designation, double WMm, double HMm, double LMm, double CourseMm, double JointMm,
    string Region, ComponentAuthority Authority,
    FrogGeometry Frog, Perforation Perforation, SpecialShape Shape,
    SizeTolerance Tolerance, SizeRange Range, MasonryBody Body);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The ONE void owner over the frog/perforation geometry pair — the cell derivation AND the coring bucket read the
// SAME inputs, so both live on one surface. Cells lay the BED-PLANE (W × L) lattice the cmu sibling shares:
// Perforation stations the FULL Columns × Rows hole grid at its true positions (Columns along the length, Rows
// across the width — the US modular's 3 cores run along the 194 mm length), each circular hole an AREA-EQUIVALENT
// square cell of side d·√π/2, so the net bed section, its moments, and the void fraction all follow the declared
// grid exactly (a through-hole's bed-area fraction IS its volume fraction — the equivalence is identity, never a
// smeared full-height slot). Frog: one centered pocket-footprint cell whose along-length span scales by
// DepthMm/H so bed-area fraction equals pocket volume fraction; a double frog doubles the span. Cells stay
// ungrouted/unreinforced — clay masonry has no grout path; the grouted/reinforced VoidCell flags are the cmu
// sibling's columns.
public static class MasonryVoids {
    public static Seq<VoidCell> Cells(FrogGeometry frog, Perforation perforation, double wMm, double hMm, double lMm) {
        double side = perforation.HoleDiameterMm * Math.Sqrt(Math.PI) / 2.0;
        Seq<VoidCell> holes = perforation.Present
            ? toSeq(Enumerable.Range(0, perforation.Columns)).Bind(col =>
                toSeq(Enumerable.Range(0, Math.Max(1, perforation.Rows))).Map(row => new VoidCell(
                    XMm: perforation.EdgeMarginMm + (wMm - 2.0 * perforation.EdgeMarginMm) * (row + 0.5) / Math.Max(1, perforation.Rows) - side * 0.5,
                    YMm: perforation.EdgeMarginMm + (lMm - 2.0 * perforation.EdgeMarginMm) * (col + 0.5) / perforation.Columns - side * 0.5,
                    WidthMm: side, HeightMm: side)))
            : Seq<VoidCell>();
        double frogW = frog.WidthFraction * wMm;
        double frogL = frog.LengthFraction * lMm * (hMm > 0.0 ? frog.DepthMm / hMm : 0.0) * (frog.Double ? 2.0 : 1.0);
        Seq<VoidCell> pockets = frog.Present
            ? Seq(new VoidCell((wMm - frogW) * 0.5, (lMm - frogL) * 0.5, frogW, frogL))
            : Seq<VoidCell>();
        return holes + pockets;
    }

    // The void-class bucket onto the component#COMPONENT_OWNER Coring vocabulary: a through-perforated unit
    // buckets by net void, a single/double frog -> Frog/Cellular, a plain unit -> None — the shared clay/concrete
    // void vocabulary the cmu sibling also buckets onto. The ASTM C652/C216 band thresholds live HERE because this
    // is one of the two sites that compare against them (the class itself stores no floor); the DERIVED fraction is
    // the sum of two independent published columns, so a frog-plus-perforation geometry outside [0,1) RAILS
    // ComponentFault.Coring rather than clamping to a fabricated 0.999 — the exact net void stays the cell geometry,
    // and this coarse class is the IFC-profile-lane and Appearance/bsdf#SHADING_FRAME band-disjointness read.
    const double HollowVoidFloor = 0.40;       // ASTM C652 H60V
    const double PerforatedVoidFloor = 0.25;   // ASTM C652 H40V; below it ASTM C216 cored

    public static Fin<Coring> Bucket(FrogGeometry frog, Perforation perforation, double wMm, double hMm, double lMm, Op key) =>
        frog.VoidFraction(hMm) + perforation.VoidFraction(lMm, wMm) is var voids && double.IsFinite(voids) && voids is >= 0.0 and < 1.0
            ? Fin.Succ(perforation.Present
                ? voids switch { >= HollowVoidFloor => Coring.Hollow2Cell, >= PerforatedVoidFloor => Coring.Perforated10Cell, _ => Coring.Cored3Hole }
                : frog.Present ? (frog.Double ? Coring.Cellular : Coring.Frog) : Coring.None)
            : ComponentFault.Coring(key, $"<void-fraction-out-of-range:{voids:R}>");
}

// The clay/calcium-silicate physics receipt — the CmuPhysics parity surface over (bed-plane profile, MasonryBody): ACI/IBC
// equivalent thickness on the same solid-fraction basis (a clay unit has no grout path, so every cell voids), the IBC 722.4
// power-law fire rating over the body's cn, oven-dry self-weight per wall-face m², the material-only thermal resistance
// (homogeneous slab for a solid unit; two face shells in series with the web/cell parallel core for a cored/perforated
// lattice — the cell path the trapped-air resistance), and the areal mass the WallAcoustics mass law reads. Bag-free: seed
// time and any consumer holding the M7-resolved profile plus the row Body compute the identical receipt.
public readonly record struct MasonryPhysics(
    double EquivalentThicknessMm,
    double FireRatingHours,
    double SelfWeightKnPerM2,
    double ThermalResistanceM2KPerW,
    double SolidFraction) {

    const double GravityMPerS2 = 9.80665;

    public double ArealMassKgPerM2 => SelfWeightKnPerM2 * 1000.0 / GravityMPerS2;

    public static MasonryPhysics Of(SectionProfile profile, MasonryBody body) {
        double w = profile.GrossRectangleMm.WidthMm.Value, len = profile.GrossRectangleMm.DepthMm.Value, gross = w * len;
        Seq<VoidCell> cells = profile is SectionProfile.CellularRectangle c ? c.Cells : Seq<VoidCell>();
        double voids = cells.Sum(static v => v.WidthMm * v.HeightMm);
        double net = gross - voids, te = net / len;
        return new(
            EquivalentThicknessMm: te,
            FireRatingHours: Math.Clamp(Math.Pow(te / body.EqThick1HrMm, 1.7), 0.0, 4.0),
            SelfWeightKnPerM2: net * body.DensityKgM3 * GravityMPerS2 / (len * 1e6),
            ThermalResistanceM2KPerW: Resistance(cells, body, w, len),
            SolidFraction: gross > 0.0 ? Math.Clamp(net / gross, 0.0, 1.0) : 1.0);
    }

    // The isothermal-planes core for the clay lattice: the cell path is always trapped air (no grout arm), the face shells
    // and core width derive from the widest cell, a cell-free solid is one homogeneous slab.
    static double Resistance(Seq<VoidCell> cells, MasonryBody body, double wMm, double lenMm) {
        double k = body.ConductivityWmK, widthM = wMm / 1000.0;
        if (cells.IsEmpty) { return widthM / k; }
        double coreWidthM = cells.Max(static c => c.WidthMm) / 1000.0;
        double webConductance = (lenMm - cells.Sum(static c => c.HeightMm)) / lenMm * (k / coreWidthM);
        double cellConductance = cells.Sum(c => c.HeightMm / lenMm * (1.0 / CmuPhysics.CellAirResistanceM2KPerW));
        return (widthM - coreWidthM) / k + 1.0 / (webConductance + cellConductance);
    }
}

// The single-leaf field-incidence mass law over the seam acoustic bands — the heavy-wall spectrum ONE fold serves for the
// clay AND concrete coursing families (MasonryPhysics/CmuPhysics supply the areal mass; the IGU spectrum stays glazing's,
// whose cavity resonances a single leaf does not carry): R(f) = 20·log₁₀(m'·f) − 47 dB, absorption the hard-masonry 0.02
// flat, Rw the seam RatingContour fit read off the receipt.
public static class WallAcoustics {
    const double MassLawOffsetDb = 47.0;

    public static Fin<Acoustic> Of(double arealMassKgPerM2, Op key) {
        double[] sri = new double[AcousticBand.Count];
        double[] absorption = new double[AcousticBand.Count];
        foreach (AcousticBand band in AcousticBand.Items) {
            sri[band.Key] = Math.Max(0.0, 20.0 * Math.Log10(Math.Max(arealMassKgPerM2, 1e-9) * band.CenterHz) - MassLawOffsetDb);
            absorption[band.Key] = 0.02;
        }
        return Acoustic.Of(absorption, sri, key);
    }
}

// The seed-built realization bag (Masonry WIDENS to DetailLane.Realization — the dissolved payload's realization
// columns have no other landing): the EN 771-1 work-vs-actual envelope inputs (SizeTolerance/SizeRange tokens the
// coursing tolerance and GLB tessellation read), the special-shape token, and the unit HEIGHT + coursing module
// the spec course fold reads (the bed-plane W × L dims ride the Profile). Token/Measured/RealizationRows are the
// relocated component#COMPONENT_DETAIL constructors; the SI mints are the dimension-only MeasureValue.OfSi so an
// authored and an imported bag content-key identically.
public static class MasonryDetail {
    public static Fin<PropertyBag> Of(PositiveMagnitude heightMm, PositiveMagnitude courseHeightMm, SizeTolerance tolerance, SizeRange range, SpecialShape shape) =>
        from unitHeight in ComponentDetail.Measured(DetailSchema.UnitHeight, Dimension.LengthDim, heightMm.Value * 1e-3)
        from courseHeight in ComponentDetail.Measured(DetailSchema.CourseHeight, Dimension.LengthDim, courseHeightMm.Value * 1e-3)
        select ComponentDetail.RealizationRows(
            ComponentDetail.Token(DetailSchema.SizeTolerance, tolerance.Key),
            ComponentDetail.Token(DetailSchema.SizeRange, range.Key),
            ComponentDetail.Token(DetailSchema.SpecialShape, shape.Key),
            unitHeight,
            courseHeight);

    // The seam-lowering door, the glazing#GLAZING_FAMILY GlazingDetail.Properties parity owner: the MasonryPhysics
    // receipt the unit already computes finally REACHES the seam. Thermal carries the body's EN 1745 design λ and c
    // with the U-value the isothermal-planes lattice resistance inverts (the unit-level reference the EN ISO 6946
    // assembly fold at Rasm.Compute supersedes) and the EN ISO 13788 vapour factor; Acoustic the single-leaf mass-law
    // spectrum over the receipt's own areal mass; Fire the IBC 722.4 power-law hours as EN 13501-2 minutes on all
    // three criteria (a solid fired-clay leaf is load-bearing, integrity-sound, and insulating for the same duration
    // — the equivalent-thickness law rates the whole leaf) under the A1 non-combustible reaction every fired or
    // calcium-silicate unit carries. Bag-free and receipt-driven: the projector composes it beside the glazing door,
    // so the coursing families lower the identical set at seed time or at projection.
    public static Fin<Seq<MaterialPropertySet>> Properties(SectionProfile profile, MasonryBody body, Op key) =>
        from physics in Fin.Succ(MasonryPhysics.Of(profile, body))
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: body.ConductivityWmK,
            specificHeat: body.SpecificHeatJKgK,
            uValue: 1.0 / physics.ThermalResistanceM2KPerW,
            vapourResistanceFactor: body.VapourMu, key)
        from spectrum in WallAcoustics.Of(physics.ArealMassKgPerM2, key)
        from resistance in FireResistance.Of(FireMinutes(physics), FireMinutes(physics), FireMinutes(physics), key)
        select Seq(thermal, MaterialPropertySet.OfAcoustic(spectrum), MaterialPropertySet.OfFire(FireRating.A1, resistance));

    static int FireMinutes(MasonryPhysics physics) => (int)(physics.FireRatingHours * 60.0);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The AUTHORED regional table: each unit carries its published coordinating dimensions, real void geometry,
// silhouette, EN 771-1 categories, and base material — a US modular cored clay (ASTM C216, 3 cores), a UK
// perforated calcium-silicate (BS EN 771-1, 10-hole), a DIN frogged Vollziegel (DIN 105), an AU cored clay
// (AS 4773), the IS 1077 modular/conventional pair, the ASTM C216 norman long-format, and the BS 4729 bullnose
// special (the SpecialShape vocabulary instantiated, not dead data). All dimensional columns PUBLISHED.
public static class MasonrySeed {
    static readonly Seq<MasonryRow> Regional = Seq(
        new MasonryRow("masonry.us-modular",      92.0,  57.0, 194.0,  67.0,  9.5, "us",  ComponentAuthority.Astm, FrogGeometry.None,                              new Perforation(3, 1, 38.0, 25.0), SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.us-norman",       92.0,  57.0, 295.0,  67.0,  9.5, "us",  ComponentAuthority.Astm, FrogGeometry.None,                              new Perforation(3, 1, 38.0, 25.0), SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.uk-standard",    102.5,  65.0, 215.0,  75.0, 10.0, "uk",  ComponentAuthority.Bs,   FrogGeometry.None,                              new Perforation(5, 2, 29.0, 15.0), SpecialShape.None,     SizeTolerance.T2, SizeRange.R1, MasonryBody.CalciumSilicate),
        new MasonryRow("masonry.uk-bullnose",    102.5,  65.0, 215.0,  75.0, 10.0, "uk",  ComponentAuthority.Bs,   FrogGeometry.None,                              Perforation.None,                  SpecialShape.Bullnose, SizeTolerance.T2, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.din-nf",         115.0,  71.0, 240.0,  83.5, 12.5, "din", ComponentAuthority.Din,  new FrogGeometry(12.0, 0.55, 0.40, 8.0, false), Perforation.None,                  SpecialShape.None,     SizeTolerance.T2, SizeRange.R2, MasonryBody.FiredClay),
        new MasonryRow("masonry.au-standard",    110.0,  76.0, 230.0,  86.0, 10.0, "au",  ComponentAuthority.As,   FrogGeometry.None,                              new Perforation(3, 1, 40.0, 25.0), SpecialShape.None,     SizeTolerance.T2, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.is-modular",      90.0,  90.0, 190.0, 100.0, 10.0, "is",  ComponentAuthority.Is,   new FrogGeometry(10.0, 0.50, 0.40, 6.0, false), Perforation.None,                  SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.is-conventional", 110.0, 70.0, 230.0,  80.0, 10.0, "is",  ComponentAuthority.Is,   FrogGeometry.None,                              Perforation.None,                  SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay));

    // The TYPED axis join, the cmu#CMU_FAMILY CmuSeed.Table form: an M7-resolved Component keys ONE lookup for the
    // MasonryBody the Properties lowering and the coursing physics read, both key spaces minting from the identical
    // designation column of the identical rows so seed and canonical correspondence cannot drift.
    public static readonly FrozenDictionary<ComponentId, MasonryRow> Table =
        Regional.ToFrozenDictionary(static r => ComponentId.Create(r.Designation), static r => r);

    // Coordinating-module closure: Course = H + bed joint within the published-rounding band (the US modular print
    // carries 67.0 over 57.0 + 9.5) — the same standard authored twice must not diverge; a transposed column faults.
    const double CoursingClosureTolMm = 1.0;

    // The ONE generator fold (RAIL law): `Fin` traversal aborts on the first failing row and never drops it.
    // The coursing-closure gate proves the module column; dimensions admit ONCE
    // through ComponentUnit.Of; the profile constructs through the railed SectionProfile Of factories on the BED
    // PLANE (Rectangle W × L solid, CellularRectangle frogged/cored with the true-grid cells — the cmu-congruent
    // orientation whose net area IS the TMS 402 bed section); the Coring bucket, the supertype IfcBinding, and the
    // seed-built Realization bag land per row; Sectioned: false pins masonry out of the section map
    // (graph.SectionOf None preserved).
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Regional.Traverse(r =>
            from coursed in guard(Math.Abs(r.CourseMm - (r.HMm + r.JointMm)) <= CoursingClosureTolMm,
                ComponentFault.Dimension(context.Key, $"<coursing-module-mismatch:{r.Designation}>"))
            from unit in ComponentUnit.Of(r.WMm, r.HMm, r.LMm, r.CourseMm, context.Key)
            from profile in r.Frog.Present || r.Perforation.Present
                ? SectionProfile.CellularRectangle.Of(r.WMm, r.LMm, MasonryVoids.Cells(r.Frog, r.Perforation, r.WMm, r.HMm, r.LMm), context.Key)
                : SectionProfile.Rectangle.Of(r.WMm, r.LMm, context.Key)
            from detail in MasonryDetail.Of(unit.HeightMm, unit.CourseHeightMm, r.Tolerance, r.Range, r.Shape)
            from coring in MasonryVoids.Bucket(r.Frog, r.Perforation, r.WMm, r.HMm, r.LMm, context.Key)
            from item in Component.Of(
                ComponentFamily.Masonry, r.Designation, profile,
                IfcBinding.Supertype(ComponentFamily.Masonry.Class),
                coring,
                new ComponentStandard(r.Region, r.JointMm, r.Authority),
                substanceId: r.Body.Material, appearanceId: r.Body.Material,
                detail: Some(detail),
                context.Key)
            select new ComponentRow(item, Sectioned: false)).As();
}
```

## [03]-[RESEARCH]

- [BOND_GROUP_CENSUS]-[OPEN]: do the DERIVED stack `pmm` and flemish `cmm` wallpaper-group assignments hold (the herringbone `pgg`, pinwheel `p4`, basketweave `p4g`, and diaper `cmm` rows are adjudicated); route: a tiling-theory or architectural-geometry source enumerating brick-bond plane groups — a refutation is one `BondGeometry` `Group` column edit, the motif seats unchanged.
