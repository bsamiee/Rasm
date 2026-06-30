# [MATERIALS_MASONRY]

THE MASONRY COMPONENT-FAMILY VOCABULARY and THE GENERATIVE BOND ALGEBRA. The masonry-unit cross-section vocabulary — the frog/perforation void geometry, the EN 771-1 work-vs-actual tolerance, the bond/orientation/cut/closure/special-shape algebra, the ASTM C270 mortar-joint specification, and the regional `ComponentCatalogue` rows — is the cross-section one `component#COMPONENT_OWNER` `Component` carries in the `ComponentSection.Masonry(MasonryUnit)` arm. A masonry unit is a `Component` row in the `ComponentFamily.Masonry` MINOR family (`ComponentClass.Minor`, an `IfcElementComponent` brick — one standardized type, many laid pieces), never a `Brick` type: the frog/perforation void geometry, the special-shape silhouette, the EN 771-1 tolerance, and the regional standard are masonry-`Component` columns, and the bond/orientation algebra is the data the `Construction/layout#ASSEMBLY_FOLD` course fold reads. The `Bond` axis is a GENERATIVE ALGEBRA (template-OR-computed): a `BondKind.Template` bond reads its course set by wrapped index, and a `BondKind.Generated` bond carries a parametric `BondGeometry` descriptor whose OWN `Course(index)` delegate DERIVES the FULL per-unit packing transforms (the basketweave/pinwheel/diaper offset + lateral + rotation), so flemish/herringbone/basketweave/pinwheel/diaper bonds COMPUTE their course rather than railing `ComponentFault.Bond`, and a new decorative bond is one `BondGeometry` row binding its course delegate, never a transcribed course set and never an arm added to a central interpreter switch. The descriptor IS the behavior: the per-pattern course derivation rides each `BondGeometry` row as a `[UseDelegateFromConstructor]` column. The page is HAND-ROLLED in-fence — a masonry unit is a laid course, not a profiled structural member, so it carries NO `ComputedSection` (it is absent from `ComponentCatalogue.Sections`, and the `[M7]` `ResolvedComponent.Section` resolves `None`) and composes no `VividOrange` section solver; the masonry compressive utilisation rides the `MortarType` flexural-bond strength and the `capacity#SECTION_CAPACITY` `MasonryCompression` rail off the assemblage, never a per-unit stiffness receipt. It composes `component#COMPONENT_OWNER` for the `Component`/`ComponentUnit`/`ComponentStandard`/`ComponentAuthority`/`Coring` shapes and `Appearance/graph#MATERIAL_LIBRARY` for the per-row `MaterialId`; a cmu/steel/timber/glazing family lands its own sibling vocabulary on its own page.

## [01]-[INDEX]

- [01]-[MASONRY_FAMILY]: the masonry unit vocabulary (`BondKind`, `BondGeometry`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`, `MortarProfile`, `MortarType`), the `BondName` template/generated course catalogue, the delegate-bearing `BondGeometry` per-unit packing-transform descriptor, the `FrogGeometry`/`Perforation` generative void geometry, the EN 771-1 `SizeTolerance`/`SizeRange` work-vs-actual categories, the ASTM C270 `MortarJoint`/`CourseTemplate`/`UnitPlacement` joint-and-course shapes, the `MasonryUnit` `ComponentSection.Masonry` payload, and the `ComponentCatalogue.BuildMasonryRows` regional row table the `component#COMPONENT_OWNER` `ComponentCatalogue.Build` folds.

## [02]-[MASONRY_FAMILY]

- Owner: the masonry unit vocabulary (`BondKind`, `BondGeometry`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`, `MortarProfile`, `MortarType`, `SizeTolerance`, `SizeRange`); `BondName` the template/generated bond catalogue; `UnitPlacement`/`CourseTemplate` the per-unit course transform; `MortarJoint` the head/bed-width + ASTM C270 profile + mortar-strength joint specification; `FrogGeometry`/`Perforation` the generative bed-face-indentation and through-perforation void geometry; `MasonryUnit` the `ComponentSection.Masonry` cross-section payload; `MasonryRow` the regional raw row; `ComponentCatalogue.BuildMasonryRows` the registered-row seed `component#COMPONENT_OWNER` composes.
- Cases: bond {template + generated kinds, the generated kind a `BondGeometry` descriptor whose own course delegate computes the full per-unit transform} · orientation {stretcher/header/soldier/sailor/rowlock/shiner, each carrying its run/rise course footprint} · cut {whole/three-quarter/half-bat/quarter-bat/queen-closer/king-closer/bevel, each carrying its cut-plane length/width remainder + plane-normal orientation} · closure {none/queen-closer/king-closer/half-bat} · special-shape {none/bullnose/cownose/plinth/coping/cant/squint/birdsmouth/voussoir, each carrying its profile-modifier + the voussoir radial wedge taper} · mortar-profile {concave/v/weathered/struck/raked/flush/beaded/squeezed ASTM C270 tooled joints, each carrying its 3D recess cross-section} · mortar-type {M/S/N/O/K ASTM C270 compressive classes mapped to their EN 998-2 `M`-class and cement:lime:sand proportion} · tolerance {T1/T2/Tm EN 771-1 mean-deviation categories} · range {R1/R2/Rm EN 771-1 batch-range categories}.
- Entry: `public Fin<CourseTemplate> Course(int index, Op key)` on `BondName` and `public bool Fits(MasonryUnit unit)` the `[BoundaryAdapter]` bond-fit check — a `BondKind.Template` bond reads its course template by wrapped index, a `BondKind.Generated` bond resolves its course through its `BondGeometry.Course(index)` delegate over the descriptor's unit-cell columns (the repeat width, the per-course offset, the diagonal/herringbone rotation, the basketweave/pinwheel lateral packing), so every generated bond COMPUTES its course rather than railing `ComponentFault.Bond`; `Fits` admits the bond only when the unit's length-over-height lies inside the descriptor's `[AspectLo, AspectHi]` band (`BondGeometry.Admits`) the diagonal/woven cells require (a herringbone needs a near-2:1 unit, a stack admits any band), so a unit that cannot tile the cell is rejected at the edge rather than producing a degenerate course. `MasonryUnit.ToCoring()` buckets the frog/perforation-derived void fraction onto the `component#COMPONENT_OWNER` `Coring` void class the `Component.Of` coring arg carries; the owner's `ComponentSection.CrossNominalMm` reads `MasonryUnit.WidthMm`, and `MasonryUnit.ToUnit()` re-wraps the dimensions into the shared `ComponentUnit` the layout fold reads.
- Packages: Rasm (project — `PositiveMagnitude`/`UnitInterval` the kernel value-object atoms from `Rasm.Vectors`, `Op`/`Context`/`AcceptValidated` the boundary admission from `Rasm.Domain`), Rasm.Element (project — `MaterialId` the per-row appearance + capacity handle), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`/`[UseDelegateFromConstructor]`/`[KeyMemberEqualityComparer]`, `ComparerAccessors`), LanguageExt.Core (`Fin`/`Option`/`Seq` admission + course projection), BCL inbox (`FrozenDictionary`); no `VividOrange` — the masonry generative data is hand-rolled in-fence (`[4]` CATALOG_LAW).
- Growth: the masonry vocabulary grows by DATA — a new template bond is one `BondName` row carrying its `CourseTemplate` set, a new generated bond is one `BondGeometry` row binding its `Course` delegate plus a `BondName` row referencing it (never an arm added to a central interpreter — the row owns its derivation), a new orientation one `Orientation` case, a new special shape one `SpecialShape` row carrying its profile-modifier, a new mortar class one `MortarType` row, a new tolerance one `SizeTolerance`/`SizeRange` row, a new region one `MasonryRow` — never a per-bond layout method, never a parallel generated-layout owner. A sibling family (cmu/steel/timber/glazing/reinforcement/fastener/connector/joint) lands its own vocabulary on its own page the way masonry carries `BondName`/`Orientation`/`MortarType`.
- Boundary: the masonry vocabulary is the realized masonry `ComponentFamily` arm — a per-material masonry class is the deleted form; `BondName.Course` is the OOP capsule reading the template course set or invoking the generated descriptor's course delegate, `BondName.Fits` the `[BoundaryAdapter]` aspect-ratio gate, both at the edge; the course-placement projection (the station/elevation fold) is `Construction/layout#ASSEMBLY_FOLD`, composed not re-shown here; a `BondKind.Generated` bond carries no template course set, so its `BondGeometry` descriptor's `Course` delegate DERIVES the full per-unit `CourseTemplate` (the `Seq<UnitPlacement>` of orientation + along-fraction + lateral-fraction + rotation, plus the course-level offset) from the unit-cell columns rather than railing `ComponentFault.Bond`, the derivation living ON THE ROW (`[UseDelegateFromConstructor]`) so there is no central switch to grow; the per-unit rotation/offset a herringbone/diaper/basketweave bond emits rides the `UnitPlacement` columns the `Construction/layout#ASSEMBLY_FOLD` `StationStep` consumes, never a widening of the host-neutral scalar-`Placement` model; the frog/perforation void geometry (`FrogGeometry` the bed-face indentation depth/taper/single-vs-double, `Perforation` the through-hole grid count/diameter) is the REAL generative geometry the unit solid extrudes, and `MasonryUnit.VoidFraction` derives the net void the `ToCoring` bucket maps onto the `component#COMPONENT_OWNER` `Coring` void class (replacing a per-row scalar void band); the special-shape silhouette and the EN 771-1 `SizeTolerance`/`SizeRange` are per-unit `MasonryUnit` columns the unit solid and the actual-vs-work tolerance read; the mortar 3D recess (`MortarProfile.RecessShape`/`SlopeDegrees`/`DepthFactor`) is the cross-section the joint solid extrudes and the `Appearance/weathering#WEATHERING` raked-joint ambient-occlusion mask reads off `ShadowLine`/`RecessDepthMm`; a non-positive head/bed width rails `ComponentFault.Mortar` at `MortarJoint.Of` (the joint-SPEC fault disjoint from `ComponentFault.Bond` the course-pattern fault), distinct from the layout-tier `Construction/assembly#PLACEMENT_MODEL` `ConstructionFault.Joint`; the masonry unit is hand-rolled (no `VividOrange`, `ComputedSection`-free) so it never contributes to `ComponentCatalogue.Sections` and the `[M7]` `ResolvedComponent.Section` resolves `None`; `ComponentCatalogue.BuildMasonryRows(context)` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue` table with the regional masonry `Component` rows (us/uk/din/au/is-modular/is-conventional), the `ComponentUnit` width/height/length/coursing columns admitting through `ComponentUnit.Of` into the kernel `PositiveMagnitude` value-object so the masonry unit never re-mints a length primitive, each row carrying its real fired-clay/calcium-silicate `MaterialId` (never one flat glazed appearance for every region — glaze is an `Appearance/graph` finish, not the base material) and its published frog/perforation void geometry, and a malformed row drops through `Choose`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Foundation.CSharp.Analyzers.Contracts;   // [BoundaryAdapter] (the boundary-capsule marker on BondName.Fits)
using LanguageExt;                              // Fin, Option, Seq
using Rasm.Vectors;                             // PositiveMagnitude, UnitInterval — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                              // Context, Op, AcceptValidated (the boundary-admission key + the validated-accept extension)
using Rasm.Element;                             // MaterialId (the seam appearance/capacity handle each masonry row carries)
using Thinktecture;                             // [SmartEnum]/[UseDelegateFromConstructor]/[KeyMemberEqualityComparer], ComparerAccessors
using Rasm.Materials.Component;                 // Component/ComponentUnit/ComponentStandard/ComponentAuthority/ComponentId/ComponentFault/ComponentFamily/ComponentSection/Coring (the parent COMPONENT_OWNER)
using static LanguageExt.Prelude;               // Seq, Seq1, toSeq, Some, None

// This page DEFINES the masonry vocabulary (BondName/Orientation/MortarType/MortarJoint/CourseTemplate/SpecialShape) the
// cmu#CMU_FAMILY + Construction siblings import as Rasm.Materials.Component.Masonry; the parent owner types (Component,
// ComponentUnit, ComponentFault, Coring) are members of the enclosing Rasm.Materials.Component namespace, composed via the
// using above. Coring/CoringClass are RELOCATED to component#COMPONENT_OWNER — masonry composes the void class, owning only
// the masonry-specific frog/perforation void GEOMETRY the ToCoring bucket maps onto it.
namespace Rasm.Materials.Component.Masonry;

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondKind {
    public static readonly BondKind Template = new("template");
    public static readonly BondKind Generated = new("generated");
}

// The brick laying-orientation — which of the unit's three faces shows and how the unit consumes the course run/rise.
// Each orientation carries its course FOOTPRINT as DATA so the Construction/layout#ASSEMBLY_FOLD reads RunFraction/
// RiseFraction off the row rather than a relocated footprint Switch. RunFraction is the per-unit course advance and
// RiseFraction the course height, both as multiples of the base stretcher slot (a stretcher 1.0/1.0; a header turns the
// unit 90 degrees in plan so it advances a HALF slot 0.5 at the same rise 1.0; a soldier/sailor stand the unit vertical
// so the rise is the tripled-height 3.0; a rowlock/shiner lay the unit on edge so the rise doubles 2.0). FaceShown names
// the presented face the Appearance/weathering engine reads. A new orientation is one row carrying its footprint.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Orientation {
    public static readonly Orientation Stretcher = new("stretcher", runFraction: 1.0, riseFraction: 1.0, faceShown: "length");   // length face, full course slot
    public static readonly Orientation Header    = new("header",    runFraction: 0.5, riseFraction: 1.0, faceShown: "width");    // width face out, half-slot advance
    public static readonly Orientation Soldier   = new("soldier",   runFraction: 1.0, riseFraction: 3.0, faceShown: "width");    // stood on end, vertical — full-slot run, tripled rise
    public static readonly Orientation Sailor    = new("sailor",    runFraction: 1.0, riseFraction: 3.0, faceShown: "length");   // stood on end length-up — full-slot run, tripled rise
    public static readonly Orientation Rowlock   = new("rowlock",   runFraction: 0.5, riseFraction: 2.0, faceShown: "end");      // header on edge — half-slot run, doubled rise
    public static readonly Orientation Shiner    = new("shiner",    runFraction: 1.0, riseFraction: 2.0, faceShown: "bed");      // stretcher on edge — full-slot run, doubled rise
    public double RunFraction { get; }     // per-unit course advance as a multiple of the base stretcher slot (layout FootprintRun)
    public double RiseFraction { get; }    // course rise as a multiple of the unit height (layout FootprintRise)
    public string FaceShown { get; }       // the presented face the Appearance/weathering engine reads
}

// The field cut applied to a unit at a placement — its remaining footprint AND the cut-plane orientation, never a bare
// length scalar. LengthFraction is the remaining length after a TRANSVERSE cut (the bats), WidthFraction the remaining
// width after a LONGITUDINAL cut (a queen-closer halves the width along the length), and PlaneNormalDegrees the cut-plane
// normal off the transverse axis: 0 a straight transverse bat, 90 a longitudinal queen-closer split, an intermediate
// value a diagonal king-closer / splayed bevel. The cut-plane position (the two remainder fractions) AND orientation
// (the plane normal) close the geometry the Construction/layout#ASSEMBLY_FOLD ArchOver head detail places.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Cut {
    public static readonly Cut Whole        = new("whole",         lengthFraction: 1.000, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut ThreeQuarter = new("three-quarter", lengthFraction: 0.750, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut Half         = new("half-bat",      lengthFraction: 0.500, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut Quarter      = new("quarter-bat",   lengthFraction: 0.250, widthFraction: 1.00, planeNormalDegrees: 0.0);
    public static readonly Cut QueenCloser  = new("queen-closer",  lengthFraction: 1.000, widthFraction: 0.50, planeNormalDegrees: 90.0);   // split along the length, half width
    public static readonly Cut KingCloser   = new("king-closer",   lengthFraction: 0.750, widthFraction: 1.00, planeNormalDegrees: 45.0);   // diagonal corner cut
    public static readonly Cut Bevel        = new("bevel",         lengthFraction: 1.000, widthFraction: 1.00, planeNormalDegrees: 30.0);   // full footprint, splayed face
    public double LengthFraction { get; }
    public double WidthFraction { get; }
    public double PlaneNormalDegrees { get; }
    public bool Angled => PlaneNormalDegrees is > 0.0 and < 90.0;   // a diagonal/splay cut (king-closer, bevel) vs an axis-aligned bat or longitudinal split
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClosureRule {
    public static readonly ClosureRule None        = new("none",         closer: Cut.Whole);
    public static readonly ClosureRule QueenCloser = new("queen-closer", closer: Cut.QueenCloser);
    public static readonly ClosureRule KingCloser  = new("king-closer",  closer: Cut.KingCloser);
    public static readonly ClosureRule HalfBat     = new("half-bat",     closer: Cut.Half);
    public Cut Closer { get; }
}

// The BS 4729 / architectural special-shape vocabulary — each row carries its profile-modifier geometry, never a bare
// identity tag. RadiusFraction rounds an arris by that fraction of the unit height (bullnose single, cownose double),
// ChamferDegrees cuts a splay (cant/squint/birdsmouth), SetbackFraction steps the face in (plinth), WashFraction slopes
// the top (coping throating), and TaperDegrees is the VOUSSOIR radial wedge taper between the two bed faces (the
// extrados-to-intrados convergence the unit is manufactured with). The Construction/layout#ASSEMBLY_FOLD reads
// SpecialShape.Voussoir by identity for the arch wedge AND reads TaperDegrees for the per-unit wedge; the arch SWEEP
// (radius/springing/voussoir count) is the layout concern. The Appearance/weathering engine reads the modifier columns
// for the rounded/splayed/washed silhouette, so a special unit is one row carrying its modifier, never a subtype.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpecialShape {
    public static readonly SpecialShape None       = new("none",       radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);
    public static readonly SpecialShape Bullnose   = new("bullnose",   radiusFraction: 0.50, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);   // single rounded arris
    public static readonly SpecialShape Cownose    = new("cownose",    radiusFraction: 1.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);   // double-bullnose half-round end
    public static readonly SpecialShape Plinth     = new("plinth",     radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.25, washFraction: 0.20, taperDegrees: 0.0);   // chamfered set-back base course
    public static readonly SpecialShape Coping     = new("coping",     radiusFraction: 0.10, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.30, taperDegrees: 0.0);   // weathered wall-cap with wash
    public static readonly SpecialShape Cant       = new("cant",       radiusFraction: 0.00, chamferDegrees: 45.0, setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);   // single splay
    public static readonly SpecialShape Squint     = new("squint",     radiusFraction: 0.00, chamferDegrees: 30.0, setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);   // oblique-angle corner unit
    public static readonly SpecialShape Birdsmouth = new("birdsmouth", radiusFraction: 0.00, chamferDegrees: 60.0, setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 0.0);   // internal-angle notch
    public static readonly SpecialShape Voussoir   = new("voussoir",   radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00, taperDegrees: 5.0);   // tapered arch wedge — the per-unit radial bed-face convergence
    public double RadiusFraction { get; }    // arris round as a fraction of unit height
    public double ChamferDegrees { get; }    // splay angle off the face
    public double SetbackFraction { get; }   // face set-back as a fraction of unit width
    public double WashFraction { get; }      // top-slope rise as a fraction of unit height
    public double TaperDegrees { get; }      // voussoir radial wedge taper (the two bed faces converge toward the arch centre)
    public bool ModifiesProfile => RadiusFraction > 0.0 || ChamferDegrees > 0.0 || SetbackFraction > 0.0 || WashFraction > 0.0 || TaperDegrees > 0.0;
}

// The ASTM C270 tooled-joint profile carrying its 3D RECESS cross-section, never a bare depth scalar. DepthFactor scales
// the bed-joint width to the recess depth (signed: positive recessed in, negative projecting out — a flush joint 0);
// RecessShape names the cross-section the joint solid extrudes (concave-arc / vee-groove / sloped / rectangular / flat /
// convex-bead / extruded); SlopeDegrees is the sloped-recess face angle (positive out-down sheds water for weathered,
// negative in-down ledges water for struck); ShadowLine is the Appearance/weathering#WEATHERING ambient-occlusion weight the
// raked-joint cavity mask reads. WeatherTight grades water-shedding: the concave/V/weathered/flush profiles compact and
// shed, the struck/raked recessed-ledge profiles and the projecting beaded/squeezed arrises do NOT. The squeezed case is
// the single extruded/weeping/overhung joint (mortar squeezed out untooled); there is no ASTM C1314 prism case (that is a
// compression-test specimen, never a finished joint).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarProfile {
    public static readonly MortarProfile Concave   = new("concave",   depthFactor: 0.10,  shadowLine: 0.3, recessShape: "concave-arc", slopeDegrees: 0.0,   weatherTight: true);
    public static readonly MortarProfile Vee       = new("v",         depthFactor: 0.15,  shadowLine: 0.5, recessShape: "vee-groove",  slopeDegrees: 0.0,   weatherTight: true);
    public static readonly MortarProfile Weathered = new("weathered", depthFactor: 0.20,  shadowLine: 0.4, recessShape: "sloped",      slopeDegrees: 15.0,  weatherTight: true);    // slopes outward-down, sheds water
    public static readonly MortarProfile Struck    = new("struck",    depthFactor: 0.20,  shadowLine: 0.6, recessShape: "sloped",      slopeDegrees: -15.0, weatherTight: false);   // slopes inward-down, ledges water
    public static readonly MortarProfile Raked     = new("raked",     depthFactor: 0.50,  shadowLine: 1.0, recessShape: "rectangular", slopeDegrees: 0.0,   weatherTight: false);   // deep square recess, water-trapping
    public static readonly MortarProfile Flush     = new("flush",     depthFactor: 0.00,  shadowLine: 0.0, recessShape: "flat",        slopeDegrees: 0.0,   weatherTight: true);
    public static readonly MortarProfile Beaded    = new("beaded",    depthFactor: -0.10, shadowLine: 0.7, recessShape: "convex-bead", slopeDegrees: 0.0,   weatherTight: false);   // convex bead projects a water-ledging arris — NOT weather-tight
    public static readonly MortarProfile Squeezed  = new("squeezed",  depthFactor: -0.20, shadowLine: 0.8, recessShape: "extruded",    slopeDegrees: 0.0,   weatherTight: false);   // weeping/overhung — mortar squeezed out untooled
    public double DepthFactor { get; }    // recess depth as a signed fraction of joint width (+ recessed in, - projecting out)
    public double ShadowLine { get; }     // the Appearance/weathering#WEATHERING ambient-occlusion weight the cavity mask reads
    public string RecessShape { get; }    // the 3D recess cross-section the joint solid extrudes
    public double SlopeDegrees { get; }   // the sloped-recess face angle (+ out-down weathered, - in-down struck), 0 for the symmetric profiles
    public bool WeatherTight { get; }
}

// The ASTM C270 mortar classification, full row not a bare compressive scalar: the 28-day minimum compressive strength
// (M=17.2, S=12.4, N=5.2, O=2.4, K=0.5 MPa), the EN 998-2 M-class the strength maps to, the ASTM C270 Table 2
// cement:lime:sand proportion-spec volumes, and the minimum flexural tensile bond the masonry shear/flexure capacity
// reads. EnClassMpa lets the thermal/structural seam cite either standard, and FlexuralBondMpa feeds the
// capacity#SECTION_CAPACITY MasonryCompression rail (a masonry unit has no per-unit ComputedSection, so its
// design strength is the assemblage f'm + the mortar bond, never a section receipt). A new class is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarType {
    public static readonly MortarType M = new("M", compressiveMpa: 17.2, enClassMpa: 15.0, cement: 1.0, lime: 0.25, sand: 3.50, flexuralBondMpa: 0.40);
    public static readonly MortarType S = new("S", compressiveMpa: 12.4, enClassMpa: 10.0, cement: 1.0, lime: 0.50, sand: 4.50, flexuralBondMpa: 0.30);
    public static readonly MortarType N = new("N", compressiveMpa: 5.2,  enClassMpa: 5.0,  cement: 1.0, lime: 1.00, sand: 6.00, flexuralBondMpa: 0.20);
    public static readonly MortarType O = new("O", compressiveMpa: 2.4,  enClassMpa: 2.5,  cement: 1.0, lime: 2.00, sand: 9.00, flexuralBondMpa: 0.10);
    public static readonly MortarType K = new("K", compressiveMpa: 0.5,  enClassMpa: 1.0,  cement: 1.0, lime: 3.00, sand: 12.0, flexuralBondMpa: 0.05);
    public double CompressiveMpa { get; }     // ASTM C270 Table 2 minimum 28-day compressive strength
    public double EnClassMpa { get; }         // mapped EN 998-2 M-class (compressive class designation)
    public double Cement { get; }             // proportion-spec parts portland cement
    public double Lime { get; }               // proportion-spec parts hydrated lime
    public double Sand { get; }               // proportion-spec parts damp loose sand
    public double FlexuralBondMpa { get; }    // minimum flexural tensile bond the masonry capacity seam reads
}

// The EN 771-1 mean-dimension tolerance category: the permissible deviation of the MEAN actual size from the work size,
// the greater of a floor and a square-root-scaled term (a larger unit admits a wider deviation), verified against work
// size by EN 772-16. T1 the looser common-brick category, T2 the tighter facing-brick category, Tm the per-batch
// manufacturer-declared deviation (no standard formula — the declared value rides the unit data, modelled as a zero
// floor here so a Tm unit reads its declared deviation rather than a fabricated standard bound).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SizeTolerance {
    public static readonly SizeTolerance T1 = new("T1", floorMm: 3.0, sqrtCoefficient: 0.40);
    public static readonly SizeTolerance T2 = new("T2", floorMm: 2.0, sqrtCoefficient: 0.25);
    public static readonly SizeTolerance Tm = new("Tm", floorMm: 0.0, sqrtCoefficient: 0.00);   // manufacturer-declared
    public double FloorMm { get; }
    public double SqrtCoefficient { get; }
    public double MeanDeviationMm(double workMm) => Math.Max(FloorMm, SqrtCoefficient * Math.Sqrt(Math.Max(0.0, workMm)));
}

// The EN 771-1 range category: the permissible RANGE (largest minus smallest measured dimension) of a delivery, the
// square-root-scaled batch-uniformity bound that governs coursing consistency. R1 the looser category, R2 the tighter
// (a high range steps the bed joints over a long elevation), Rm the manufacturer-declared range.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SizeRange {
    public static readonly SizeRange R1 = new("R1", sqrtCoefficient: 0.60);
    public static readonly SizeRange R2 = new("R2", sqrtCoefficient: 0.30);
    public static readonly SizeRange Rm = new("Rm", sqrtCoefficient: 0.00);   // manufacturer-declared
    public double SqrtCoefficient { get; }
    public double PermittedRangeMm(double workMm) => SqrtCoefficient * Math.Sqrt(Math.Max(0.0, workMm));
}

// The generative bond descriptor: the unit-cell columns (repeat width, per-unit rotation, woven block size, the aspect
// band the cell tiles cleanly) PLUS the row's OWN course-deriving delegate, so the per-pattern derivation is DATA on the
// row — not an arm in a central switch. A new decorative bond is one BondGeometry row binding its Course delegate. The
// Course delegate emits the FULL per-unit CourseTemplate (the Seq<UnitPlacement> of orientation + along-offset + lateral-
// offset + rotation, plus the course-level offset) so a basketweave/pinwheel/diaper computes its complete packing
// transform. AspectLo/AspectHi bound the unit length/height ratio Fits gates on via Admits: a 45-degree diagonal cell
// (herringbone/diaper) needs a near-2:1 unit, a stack/running cell admits any positive ratio. The course delegates
// reference sibling rows (Stack.OffsetFraction, Herringbone.RotationDegrees) deferred behind the delegate per the
// smart-enum init-order law, materialized only at call time.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondGeometry {
    public static readonly BondGeometry Stack       = new("stack",       repeatUnits: 1, rotationDegrees: 0.0,  blockUnits: 1, aspectLo: 0.10, aspectHi: 8.0,  course: Stacked);
    public static readonly BondGeometry Flemish     = new("flemish",     repeatUnits: 2, rotationDegrees: 0.0,  blockUnits: 1, aspectLo: 1.50, aspectHi: 5.0,  course: Flemished);
    public static readonly BondGeometry Herringbone = new("herringbone", repeatUnits: 2, rotationDegrees: 45.0, blockUnits: 1, aspectLo: 1.60, aspectHi: 2.40, course: Diagonal);
    public static readonly BondGeometry Basketweave = new("basketweave", repeatUnits: 2, rotationDegrees: 90.0, blockUnits: 2, aspectLo: 1.60, aspectHi: 2.40, course: Woven);
    public static readonly BondGeometry Pinwheel    = new("pinwheel",    repeatUnits: 4, rotationDegrees: 90.0, blockUnits: 1, aspectLo: 1.40, aspectHi: 3.0,  course: Rotary);
    public static readonly BondGeometry Diaper      = new("diaper",      repeatUnits: 4, rotationDegrees: 45.0, blockUnits: 1, aspectLo: 1.60, aspectHi: 2.40, course: Lattice);

    public int RepeatUnits { get; }          // unit-cell repeat length in units before the course offset cycles
    public double RotationDegrees { get; }   // per-unit off-horizontal rotation for diagonal/rotary patterns
    public int BlockUnits { get; }           // basketweave/diaper block size (units per woven block)
    public double AspectLo { get; }          // closed length/height band the cell tiles — see Fits (a ratio, not [0,1])
    public double AspectHi { get; }

    // The course derivation IS the row: every BondGeometry answers its own per-unit course by index. No central interpreter.
    [UseDelegateFromConstructor]
    public partial CourseTemplate Course(int courseIndex);

    public double OffsetFraction(int course) => RepeatUnits <= 1 ? 0.0 : (course % RepeatUnits) / (double)RepeatUnits;
    public bool Admits(double lengthOverHeight) => lengthOverHeight >= AspectLo && lengthOverHeight <= AspectHi;

    // --- [COURSE_DELEGATES]
    // Stack: a single stretcher per course, vertically aligned (RepeatUnits 1, so OffsetFraction stays 0) — the base
    // stacked cell with no offset and no per-unit rotation.
    static CourseTemplate Stacked(int course) =>
        new(Seq1(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0)), Stack.OffsetFraction(course));

    // Flemish: each course alternates stretcher-header per unit, every other course shifted a quarter unit so a header
    // centres over the stretcher below; the two units pack consecutively (along/lateral 0), the course offset the cell key.
    static CourseTemplate Flemished(int course) =>
        new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0), new UnitPlacement(Orientation.Header, 0.0, 0.0, 0.0)), (course & 1) == 0 ? 0.0 : 0.25);

    // Herringbone: interlocking pairs at plus/minus 45 degrees — the first unit rotated up, the second rotated down and
    // laterally offset a half cell, the lead rotation flipping by course parity so adjacent rows zigzag.
    static CourseTemplate Diagonal(int course) {
        double lead = (course & 1) == 0 ? Herringbone.RotationDegrees : -Herringbone.RotationDegrees;
        return new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, lead), new UnitPlacement(Orientation.Stretcher, 0.0, 0.5, -lead)), 0.0);
    }

    // Basketweave: BlockUnits-wide blocks alternating horizontal/vertical (0/90 degrees) every block of courses — each
    // unit in the block laterally stepped by its index fraction, the whole block rotated by the course-block parity.
    static CourseTemplate Woven(int course) {
        double turn = ((course / Basketweave.BlockUnits) & 1) == 0 ? 0.0 : Basketweave.RotationDegrees;
        return new(toSeq(Enumerable.Range(0, Basketweave.BlockUnits))
            .Map(i => new UnitPlacement(Orientation.Stretcher, 0.0, i / (double)Basketweave.BlockUnits, turn)), 0.0);
    }

    // Pinwheel: a stretcher-header pair rotating about a square centre, the rotation stepping 90 degrees with the course
    // index modulo four (the four-unit motif leaving the central square void), the header laterally offset a half cell.
    static CourseTemplate Rotary(int course) {
        double spin = (course % 4) * Pinwheel.RotationDegrees;
        return new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, spin), new UnitPlacement(Orientation.Header, 0.0, 0.5, spin + Pinwheel.RotationDegrees)), Pinwheel.OffsetFraction(course));
    }

    // Diaper: a diamond lattice — one stretcher per course rotated plus/minus 45 degrees by course parity on a four-course
    // diagonal repeat, the offset stepping each course so the diamonds interlock.
    static CourseTemplate Lattice(int course) =>
        new(Seq1(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, (course % 2 == 0 ? 1 : -1) * Diaper.RotationDegrees)), Diaper.OffsetFraction(course));
}

// The bond catalogue: template bonds carry an explicit per-unit course set; generated bonds reference a BondGeometry
// whose own delegate computes the full per-unit course. A herringbone/basketweave/pinwheel/diaper is a generated row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondName {
    public static readonly BondName Running       = new("running",        template: Seq(StretcherCourse(0.0), StretcherCourse(0.5)));
    public static readonly BondName English       = new("english",        template: Seq(StretcherCourse(0.0), HeaderCourse(0.5)));
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

    // The real aspect-ratio gate (not a length>0 tautology a PositiveMagnitude-backed unit always satisfies): a template
    // bond admits any positive-height unit; a generated bond admits only when the unit's length-over-HEIGHT lies in the
    // descriptor's [AspectLo, AspectHi] band the cell tiles cleanly — a herringbone/basketweave rejects a near-square
    // unit too square to tile its 45-degree diagonal cell. The aspect reads the owner-provided
    // ComponentUnit.LengthOverHeight projection, never a LengthMm/HeightMm inline re-spelling.
    [BoundaryAdapter]
    public bool Fits(MasonryUnit unit) =>
        unit.HeightMm.Value > 0.0 &&
        Geometry.Match(Some: g => g.Admits(unit.LengthOverHeight), None: () => true);

    // A template bond reads its course by wrapped index; a generated bond invokes its descriptor's own course delegate.
    public Fin<CourseTemplate> Course(int index, Op key) => Kind.Switch(
        template:  _ => Courses.IsEmpty
            ? Fin.Fail<CourseTemplate>(ComponentFault.Bond(key, $"<template-bond-empty:{Key}>"))
            : Fin.Succ(Courses[((index % Courses.Count) + Courses.Count) % Courses.Count]),
        generated: _ => Geometry
            .ToFin(ComponentFault.Bond(key, $"<generated-bond-missing-geometry:{Key}>"))
            .Map(geometry => geometry.Course(index)));

    static CourseTemplate StretcherCourse(double courseOffset) => new(Seq1(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0)), courseOffset);
    static CourseTemplate HeaderCourse(double courseOffset) => new(Seq1(new UnitPlacement(Orientation.Header, 0.0, 0.0, 0.0)), courseOffset);
}

// --- [MODELS] ------------------------------------------------------------------------------
// One unit's placement transform within a course cell: which orientation, the additional along-course offset beyond the
// natural consecutive step (0 = packed consecutively), the across-course lateral offset (woven/pinwheel stacking), and
// the in-plane rotation (the diagonal/rotary patterns). The Construction/layout#ASSEMBLY_FOLD StationStep folds these
// into Construction/assembly#PLACEMENT_MODEL Placement (the rotation into Placement.PathAngleDegrees), never a host transform.
public readonly record struct UnitPlacement(Orientation Orientation, double AlongFraction, double LateralFraction, double RotationDegrees);

// One course's full per-unit transform: the Seq<UnitPlacement> the StationStep lays in order, plus the course-level
// horizontal offset (the running-bond half-unit shift between courses). The generated BondGeometry.Course delegate emits
// the complete cell; the layout StepCourse rejects an empty Units course.
public sealed record CourseTemplate(Seq<UnitPlacement> Units, double CourseOffsetFraction);

// The frog: a bed-face indentation pressed into a solid unit (the mortar key). DepthMm the pocket depth, LengthFraction/
// WidthFraction the pocket footprint over the bed face, TaperDegrees the de-mould draft on the pocket walls, Double a
// frog on BOTH bed faces. VoidFraction is the unit void the frog removes (the pocket volume over the unit volume — the
// depth over the height times the bed-face footprint, doubled for a double frog) the MasonryUnit.ToCoring bucket reads.
public readonly record struct FrogGeometry(double DepthMm, double LengthFraction, double WidthFraction, double TaperDegrees, bool Double) {
    public static readonly FrogGeometry None = new(0.0, 0.0, 0.0, 0.0, false);
    public bool Present => DepthMm > 0.0 && LengthFraction > 0.0 && WidthFraction > 0.0;
    public double VoidFraction(double heightMm) =>
        Present && heightMm > 0.0 ? DepthMm / heightMm * LengthFraction * WidthFraction * (Double ? 2.0 : 1.0) : 0.0;
}

// The through-perforation grid: Columns x Rows circular holes of HoleDiameterMm pierced full-height through the bed
// faces, EdgeMarginMm the margin from the unit edge. The hole grid is the generative geometry the unit solid bores
// rather than a per-row scalar void band; VoidFraction is the hole-grid area over the bed face (the holes run the full
// height, so the area fraction is the volume fraction) the MasonryUnit.ToCoring bucket reads.
public readonly record struct Perforation(int Columns, int Rows, double HoleDiameterMm, double EdgeMarginMm) {
    public static readonly Perforation None = new(0, 0, 0.0, 0.0);
    public int HoleCount => Math.Max(0, Columns) * Math.Max(0, Rows);
    public bool Present => HoleCount > 0 && HoleDiameterMm > 0.0;
    public double VoidFraction(double lengthMm, double widthMm) =>
        Present && lengthMm > 0.0 && widthMm > 0.0 ? HoleCount * Math.PI * HoleDiameterMm * HoleDiameterMm / 4.0 / (lengthMm * widthMm) : 0.0;
}

// The full mortar-joint specification the Construction/layout#ASSEMBLY_FOLD JointPolicy resolves head/bed width AND 3D
// recess profile AND mortar strength from, never a single scalar joint thickness: a masonry run carries its
// buildable joint detail, the weathering recess line, and the ASTM C270 strength receipt. Of rails the
// component#COMPONENT_OWNER ComponentFault.Mortar slot on a non-positive head/bed width — the joint-SPEC fault disjoint
// from ComponentFault.Bond (the course-pattern fault), distinct from the layout-tier ConstructionFault.Joint.
public readonly record struct MortarJoint(double HeadWidthMm, double BedWidthMm, MortarProfile Profile, MortarType Mortar) {
    public static Fin<MortarJoint> Of(double headMm, double bedMm, MortarProfile profile, MortarType mortar, Op key) =>
        double.IsFinite(headMm) && headMm > 0.0 && double.IsFinite(bedMm) && bedMm > 0.0
            ? Fin.Succ(new MortarJoint(headMm, bedMm, profile, mortar))
            : ComponentFault.Mortar(key, $"<mortar-joint-nonpositive:head={headMm}:bed={bedMm}>");

    // The coordinating joint from a single thickness — the scalar default an unspecified run uses (concave / Type-N).
    public static MortarJoint Standard(double thicknessMm) => new(thicknessMm, thicknessMm, MortarProfile.Concave, MortarType.N);

    // The signed 3D recess depth the joint solid extrudes over the Profile.RecessShape / SlopeDegrees cross-section.
    public double RecessDepthMm => BedWidthMm * Profile.DepthFactor;
}

// The masonry cross-section — the ComponentSection.Masonry(MasonryUnit Unit) payload. WidthMm/HeightMm/LengthMm/
// CourseHeightMm the dimensional columns admitted ONCE through ComponentUnit.Of (the kernel PositiveMagnitude rail) then
// carried DIRECT so the owner's ComponentSection.CrossNominalMm reads m.Unit.WidthMm; Frog/Perforation the generative
// void geometry, Shape the manufactured special silhouette, Tolerance/Range the EN 771-1 work-vs-actual categories. The
// unit derives its void fraction from the real geometry and buckets it onto the component#COMPONENT_OWNER Coring void
// class the Component.Of coring arg carries, never a forged scalar band. A masonry unit is a laid course, not a profiled
// member, so it carries no ComputedSection. ToUnit re-wraps the proven magnitudes into the shared ComponentUnit the
// Construction/layout#ASSEMBLY_FOLD Resolve fold reads (no re-admission — the columns are already kernel-proven positive).
public readonly record struct MasonryUnit(
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeightMm,
    PositiveMagnitude LengthMm,
    PositiveMagnitude CourseHeightMm,
    FrogGeometry Frog,
    Perforation Perforation,
    SpecialShape Shape,
    SizeTolerance Tolerance,
    SizeRange Range) {

    public double LengthOverHeight => LengthMm.Value / HeightMm.Value;
    public ComponentUnit ToUnit() => new(WidthMm, HeightMm, LengthMm, CourseHeightMm);
    public double VoidFraction => Math.Clamp(
        Frog.VoidFraction(HeightMm.Value) + Perforation.VoidFraction(LengthMm.Value, WidthMm.Value), 0.0, 0.999);

    // The void-class bucket onto the component#COMPONENT_OWNER Coring vocabulary the Component.Of coring arg carries: a
    // through-perforated unit buckets by void fraction (ASTM C652 H60V >=40% to Hollow2Cell, H40V >=25% to
    // Perforated10Cell, C216 <25% to Cored3Hole), a single/double frog to Frog/Cellular, a plain unit to None — the
    // shared clay/concrete-masonry void vocabulary cmu#CMU_FAMILY ToCoring also buckets onto. The exact net void is the
    // geometry above; this is the coarse class the capacity scaling and the Appearance/bsdf#SHADING_FRAME band-disjointness read.
    public Coring ToCoring() =>
        Perforation.Present
            ? VoidFraction switch { >= 0.40 => Coring.Hollow2Cell, >= 0.25 => Coring.Perforated10Cell, _ => Coring.Cored3Hole }
            : Frog.Present ? (Frog.Double ? Coring.Cellular : Coring.Frog) : Coring.None;

    // The EN 771-1 actual-size bounds about a work-size dimension: the work size plus/minus the tolerance-category mean
    // deviation, the as-manufactured envelope the coursing tolerance and the GLB tessellation read.
    public double ActualLowMm(double workMm) => workMm - Tolerance.MeanDeviationMm(workMm);
    public double ActualHighMm(double workMm) => workMm + Tolerance.MeanDeviationMm(workMm);
}

// The regional raw row: the published coordinating dimensions PLUS the unit's real void geometry, special silhouette,
// EN 771-1 tolerance categories, and base material, so a DIN frogged solid clay and a UK perforated calcium-silicate
// carry their OWN geometry and material, never one flat ceramic.glazed for every region. Material is the
// Appearance/graph#MATERIAL_LIBRARY MaterialId the row binds to BOTH the appearance and the capacity slot (the base
// clay's render and intrinsic mechanical material coincide; a glazed/coated unit splits the two slots). Region is an
// explicit token because a din/au/is regional row carries a region the bounded ComponentAuthority does not name.
public readonly record struct MasonryRow(
    string Designation, double WMm, double HMm, double LMm, double CourseMm, double JointMm,
    string Region, ComponentAuthority Authority,
    FrogGeometry Frog, Perforation Perforation, SpecialShape Shape,
    SizeTolerance Tolerance, SizeRange Range, string Material);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // Each regional unit carries its published void geometry and its real base material — a US modular cored fired-clay
    // brick (ASTM C216, 3 cores), a UK perforated calcium-silicate (BS EN 771-1, 10-hole grid), a DIN solid frogged
    // Vollziegel (DIN 105, single frog), an AU cored clay (AS 4773, 3 cores), the IS 1077 modular 190x90x90 (frogged)
    // and conventional 230x110x70 (solid) units. T1/T2 mean-tolerance and R1/R2 range follow the regional precision.
    static readonly Seq<MasonryRow> RegionalRows = Seq(
        new MasonryRow("masonry.us-modular",     92.0,  57.0, 194.0,  67.0,  9.5, "us",  ComponentAuthority.Astm, FrogGeometry.None,                       new Perforation(3, 1, 38.0, 25.0), SpecialShape.None, SizeTolerance.T1, SizeRange.R1, "ceramic.fired-clay"),         // ASTM C216 cored
        new MasonryRow("masonry.uk-standard",   102.5,  65.0, 215.0,  75.0, 10.0, "uk",  ComponentAuthority.Bs,   FrogGeometry.None,                       new Perforation(5, 2, 29.0, 15.0), SpecialShape.None, SizeTolerance.T2, SizeRange.R1, "ceramic.calcium-silicate"),   // BS EN 771-1 perforated
        new MasonryRow("masonry.din-nf",        115.0,  71.0, 240.0,  83.5, 12.5, "din", ComponentAuthority.Din,  new FrogGeometry(12.0, 0.55, 0.40, 8.0, false), Perforation.None,           SpecialShape.None, SizeTolerance.T2, SizeRange.R2, "ceramic.fired-clay"),         // DIN 105 frogged solid
        new MasonryRow("masonry.au-standard",   110.0,  76.0, 230.0,  86.0, 10.0, "au",  ComponentAuthority.As,   FrogGeometry.None,                       new Perforation(3, 1, 40.0, 25.0), SpecialShape.None, SizeTolerance.T2, SizeRange.R1, "ceramic.fired-clay"),         // AS 4773 cored
        new MasonryRow("masonry.is-modular",     90.0,  90.0, 190.0, 100.0, 10.0, "is",  ComponentAuthority.Is,   new FrogGeometry(10.0, 0.50, 0.40, 6.0, false), Perforation.None,           SpecialShape.None, SizeTolerance.T1, SizeRange.R1, "ceramic.fired-clay"),         // IS 1077 modular 190x90x90 frogged
        new MasonryRow("masonry.is-conventional", 110.0, 70.0, 230.0,  80.0, 10.0, "is",  ComponentAuthority.Is,  FrogGeometry.None,                       Perforation.None,                  SpecialShape.None, SizeTolerance.T1, SizeRange.R1, "ceramic.fired-clay"));        // IS 1077 conventional 230x110x70 solid

    // The masonry row admission: the dimensions admit ONCE through ComponentUnit.Of into the kernel PositiveMagnitude
    // rail (a non-positive/non-finite column rails ComponentFault.Dimension), the MasonryUnit composes the void geometry
    // + special silhouette + EN 771-1 tolerance, and the Component carries the masonry cross-section as the
    // ComponentSection.Masonry arm with its regional standard and base material (the appearance and capacity slot coincide
    // on the base clay). A malformed row drops through Choose rather than seeding a degenerate Component.
    static Fin<(ComponentId Id, Component Component)> MasonryOf(MasonryRow r, Context context, Op key) =>
        from unit in ComponentUnit.Of(r.WMm, r.HMm, r.LMm, r.CourseMm, context, key)
        let masonry = new MasonryUnit(unit.WidthMm, unit.HeightMm, unit.LengthMm, unit.CourseHeightMm, r.Frog, r.Perforation, r.Shape, r.Tolerance, r.Range)
        let standard = new ComponentStandard(r.Region, r.JointMm, r.Authority)
        from component in Component.Of(ComponentFamily.Masonry, r.Designation, ComponentSection.Masonry(masonry), masonry.ToCoring(), standard, MaterialId.Of(r.Material), MaterialId.Of(r.Material), key)
        select (Id: component.Designation, Component: component);

    // The masonry contribution the component#COMPONENT_OWNER ComponentCatalogue.Build folds — the registered
    // ComponentId -> Component rows. Masonry contributes NO section map (it is a unit course, absent from
    // ComponentCatalogue.Sections), so the [M7] ComponentResolution.Build joins a masonry ProfileRef to Option.None
    // (ProfileRef/ComputedSection stay seam-canonical — the semantic rename stops at the Materials folder boundary).
    // ComponentId's generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so NO explicit
    // comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>, a type mismatch
    // on the ComponentId key (the component#COMPONENT_OWNER ComponentCatalogue.Build convention the master fold follows).
    public static FrozenDictionary<ComponentId, Component> BuildMasonryRows(Context context) =>
        RegionalRows
            .Choose(row => MasonryOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Component);
}
```

## [03]-[RESEARCH]

- [GENERATIVE_VOID_GEOMETRY]: REALIZED — the masonry void is REAL geometry, not a scalar band: `FrogGeometry` carries the bed-face indentation depth/footprint/draft-taper and the single-vs-double frog (a standard pressed-brick frog at roughly 100x40x10 mm), `Perforation` the through-hole grid count/diameter/edge-margin (a 3-core US modular, a 10-hole UK perforated unit), and `MasonryUnit.VoidFraction` derives the net void the `ToCoring` bucket maps onto the `component#COMPONENT_OWNER` `Coring` void class (ASTM C652 H60V `Hollow` at >=40% void, H40V `Perforated` at >=25%, ASTM C216 `Cored` below 25%, the single/double frog the `Frog`/`Cellular` class). The unit solid extrudes the real frog pocket and hole grid; the coarse `Coring` class survives only as the capacity-scaling and band-disjointness read. Ripple counterpart: `component#COMPONENT_OWNER` (the relocated `Coring`/`CoringClass` cross-family void-class owner the `ToCoring` bucket targets).
- [GENERATIVE_BOND_TRANSFORMS]: REALIZED — `BondGeometry` emits the FULL per-unit packing transform: the `[UseDelegateFromConstructor]` `Course(int courseIndex)` delegate derives a `CourseTemplate(Seq<UnitPlacement>, CourseOffsetFraction)` whose every `UnitPlacement` carries an orientation + along-fraction + lateral-fraction + rotation, so the herringbone (interlocking plus/minus 45 pairs), basketweave (block-rotated lateral stacking), pinwheel (four-unit 90-degree rotary motif), and diaper (diamond lattice) bonds compute their complete cell with each unit carrying its own offset and rotation. A new decorative bond is one `BondGeometry` row binding its delegate plus one `BondName` row, never a central interpreter arm; the per-unit rotation/offset rides the `UnitPlacement` columns the layout consumes, never a widened host-neutral `Placement`. Ripple counterpart: `Construction/layout#ASSEMBLY_FOLD` (the `StationStep` reads `CourseTemplate.Units` per-unit `Orientation`/`AlongFraction`/`LateralFraction`/`RotationDegrees` + `CourseOffsetFraction`; `BondName.Fits` takes the `MasonryUnit`).
- [EN771_WORK_VS_ACTUAL]: REALIZED — `SizeTolerance` (T1/T2/Tm) and `SizeRange` (R1/R2/Rm) realize the EN 771-1 work-vs-actual capture: `MeanDeviationMm` is the permissible deviation of the mean actual size from the work size (the greater of a floor and a square-root-scaled term — T1 the looser common-brick category, T2 the tighter facing category, Tm the manufacturer-declared), `PermittedRangeMm` the batch range (R1 looser, R2 tighter for coursing consistency), and `MasonryUnit.ActualLowMm`/`ActualHighMm` the as-manufactured envelope the GLB tessellation and coursing tolerance read. The work size is the `ComponentUnit` dimension, the actual size the work size within the category deviation.
- [MORTAR_RECESS_PROFILE]: REALIZED — `MortarProfile` carries its 3D recess cross-section (`RecessShape` concave-arc/vee-groove/sloped/rectangular/flat/convex-bead/extruded, `SlopeDegrees` the weathered out-down vs struck in-down face angle, `DepthFactor` the signed recess depth), so the joint solid extrudes the real recess and `Appearance/weathering#WEATHERING` reads `ShadowLine`/`MortarJoint.RecessDepthMm`. The phantom corrections land: the beaded joint is re-graded NOT weather-tight (a convex bead projects a water-ledging arris), the extruded and weeping joints collapse into the one `Squeezed` case (mortar squeezed out untooled), and no ASTM C1314 prism case exists (a compression-test specimen, never a finished joint). A non-positive head/bed width rails `ComponentFault.Mortar` at `MortarJoint.Of`. Ripple counterpart: `Appearance/weathering#WEATHERING` (the raked-joint cavity-mask ambient occlusion reads `ShadowLine`/`RecessDepthMm`).
- [SPECIAL_SHAPE_VOUSSOIR]: REALIZED — `SpecialShape.Voussoir` fills its radial wedge `TaperDegrees` (the two bed faces converge toward the arch centre), so the voussoir is no longer an all-zero-modifier identity tag; the `Construction/layout#ASSEMBLY_FOLD` reads `Voussoir` by identity for the arch wedge AND `TaperDegrees` for the per-unit manufactured wedge, the arch sweep (radius/springing/voussoir count) the layout concern. The `Cut` cut-plane carries both position (the length/width remainder fractions) and orientation (`PlaneNormalDegrees` — transverse bat, longitudinal queen-closer, diagonal king-closer/bevel). Ripple counterpart: `Construction/layout#ASSEMBLY_FOLD` (the `Voussoirs` keystone-centre projection reads the `Voussoir` taper, the `ArchOver`/closure head detail reads the `Cut` cut-plane).
- [MASONRY_SECTION_ARM]: REALIZED — `MasonryUnit` is the `ComponentSection.Masonry(MasonryUnit Unit)` payload, the masonry arm of the `component#COMPONENT_OWNER` `ComponentSection` `[Union]`; the owner's `ComponentSection.CrossNominalMm` reads `MasonryUnit.WidthMm` (the dimensions carried direct, the cmu-sibling pattern), `ComponentSection.Family` the masonry arm maps to `ComponentFamily.Masonry`. The catalogue composes the owner `Component.Of(ComponentFamily.Masonry, designation, ComponentSection.Masonry(unit), unit.ToCoring(), standard, capacityKey, appearanceId, key)` — the `Coring` an explicit `Component` field the `ToCoring` bucket supplies, the two `MaterialId` slots the base clay coincides on. A masonry unit is hand-rolled (no `VividOrange`, `ComputedSection`-free), so it contributes NO section map and the `[M7]` `ResolvedComponent.Section` resolves `None` — the seam-honest absence of a section for a laid course, never a forged all-zero receipt. Ripple counterpart: `component#COMPONENT_OWNER` (the `ComponentSection` `[Union]` `Masonry(MasonryUnit)` arm + the `CrossNominalMm`/`Family` polymorphic members read `MasonryUnit.WidthMm`, the merged `Component`/`Component.Of`/`Coring` shapes the catalogue composes).
- [REGIONAL_CATALOGUE]: REALIZED — the six regional masonry rows (us-modular/uk-standard/din-nf/au-standard/is-modular/is-conventional) seed through `ComponentCatalogue.BuildMasonryRows` over the `MasonryRow` raw table, each carrying its real void geometry, special silhouette, EN 771-1 tolerance, and base `MaterialId`; the IS 1077 seed carries the modular 190x90x90 and conventional 230x110x70 units. The dimension/coursing columns admit once through `ComponentUnit.Of` into the kernel `PositiveMagnitude`, a non-positive column dropping the row through `Choose`. A new region is one `MasonryRow` data addition. Ripple counterpart: `component#COMPONENT_OWNER` (`ComponentCatalogue.Build` folds `BuildMasonryRows` into the one frozen registry).
