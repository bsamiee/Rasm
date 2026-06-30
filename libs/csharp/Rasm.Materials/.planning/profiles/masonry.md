# [MATERIALS_MASONRY]

THE FIRST REALIZED PROFILEFAMILY and THE GENERATIVE BOND ALGEBRA. The masonry family vocabulary — the void-class / bond / orientation / cut / closure / special-shape algebra and the regional `ProfileCatalogue` rows — is the realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Masonry` case. A masonry unit is a `Profile` row, never a `Brick` type: the coring void class, the bond course template, the unit orientation, and the regional standard are masonry-`Profile` columns, and the bond/orientation algebra is the data the `Construction/layout#ASSEMBLY_FOLD` course fold reads. The masonry vocabulary grows by data — a new bond is one `BondName` row, a new orientation one `Orientation` case, a new special shape one `SpecialShape` row — never a per-bond layout method. The `Bond` axis is a GENERATIVE ALGEBRA (template-OR-computed): a `BondKind.Template` bond reads its course set by wrapped index, and a `BondKind.Generated` bond carries a parametric `BondGeometry` descriptor whose OWN `Course(index)` delegate DERIVES the course placement — so flemish/herringbone/basketweave/pinwheel/diaper bonds COMPUTE their course (and per-unit rotation into `Construction/assembly#PLACEMENT_MODEL` `Placement.PathAngleDegrees`) rather than railing `ProfileFault.Bond`, and a new decorative bond is one `BondGeometry` row binding its course delegate, never a transcribed course set and never an arm added to a central interpreter switch. The descriptor IS the behavior: the per-pattern course derivation rides each `BondGeometry` row as a `[UseDelegateFromConstructor]` column, so the once-parallel `BondPattern` discriminant and the `GeneratedBond.Interpret` full-coverage switch are collapsed into the data the row carries. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape and `Appearance/graph#MATERIAL_LIBRARY` for the per-row `MaterialId`; a cmu/steel/timber/glazing family lands its own sibling vocabulary on its own page.

## [01]-[INDEX]

- [01]-[PROFILE_FAMILY]: the masonry unit/coring/bond/orientation/cut/closure/special-shape vocabulary, the `BondName` template/generated catalogue, the delegate-bearing `BondGeometry` parametric course descriptor, the `MortarProfile`/`MortarType`/`MortarJoint` ASTM C270 / EN 998-2 joint specification, and the `ProfileCatalogue.BuildMasonryRows` regional row table.

## [02]-[PROFILE_FAMILY]

- Owner: the masonry unit vocabulary (`Coring`, `CoringClass`, `BondName`, `BondKind`, `BondGeometry`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`, `MortarProfile`, `MortarType`); `CourseTemplate` the bond course shape, `MortarJoint` the head/bed-width + profile + mortar-strength joint specification; `MasonryRow` the regional raw row; `ProfileCatalogue.BuildMasonryRows` the registered-row seed `profile#PROFILE_OWNER` composes.
- Cases: coring {solid/frogged/cored/cellular/perforated/hollow void classes, each a `CoringClass` carrying its ASTM C652 net-area band} · bond {template + generated kinds, the generated kind a `BondGeometry` descriptor whose own course delegate computes the placement} · orientation {stretcher/header/soldier/sailor/rowlock/shiner, each carrying its run/rise face footprint} · cut {whole/three-quarter/half-bat/quarter-bat/queen-closer/king-closer/bevel} · closure {none/queen-closer/king-closer/half-bat} · special-shape {none/bullnose/cownose/plinth/coping/cant/squint/birdsmouth/voussoir, each carrying its profile-modifier geometry} · mortar-profile {concave/v/weathered/struck/raked/flush/beaded ASTM C270 tooled joints} · mortar-type {M/S/N/O/K ASTM C270 compressive classes mapped to their EN 998-2 `M`-class and cement:lime:sand proportion}.
- Entry: `public Fin<CourseTemplate> Course(int index, Op key)` on `BondName` and `public bool Fits(Profile profile)` the `[BoundaryAdapter]` bond-fit check — a `BondKind.Template` bond reads its course template by wrapped index, a `BondKind.Generated` bond resolves its course through its `BondGeometry.Course(index)` delegate over the row's descriptor (the unit-cell repeat, the per-course orientation pattern, the diagonal/herringbone rotation, the flemish alternating sequence), so every generated bond computes its course rather than railing `ProfileFault.Bond`; `Fits` admits the bond only when the unit's length-over-height lies inside the descriptor's `[AspectLo, AspectHi]` band (`BondGeometry.Admits`) the diagonal/woven cells require (a herringbone needs a near-2:1 unit, a stack admits any band), so a unit that cannot tile the cell is rejected at the edge rather than producing a degenerate course.
- Packages: Rasm.Element (project — `MaterialId` the per-row appearance carries), Rasm (project — the `Profile`/`ProfileUnit` shapes composed from `profile#PROFILE_OWNER`, kernel-`PositiveMagnitude`-backed), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the masonry vocabulary grows by data: a new template bond is one `BondName` row carrying its `CourseTemplate` set, a new generated bond is one `BondGeometry` row binding its `Course` delegate plus a `BondName` row referencing it (never an arm added to a central interpreter — the row owns its derivation), a new orientation one `Orientation` case, a new special shape one `SpecialShape` row carrying its profile-modifier, a new mortar class one `MortarType` row, a new region one `MasonryRow` — never a per-bond layout method, never a parallel generated-layout owner. A sibling `ProfileFamily` (cmu/steel/timber/glazing) lands its own vocabulary on its own page the way masonry carries `Coring`/`BondName`/`Orientation`.
- Boundary: the masonry vocabulary is the realized first `ProfileFamily` — a per-material masonry class is the deleted form; `BondName.Course` is the OOP capsule reading the template course set or invoking the generated descriptor's course delegate, `BondName.Fits` the `[BoundaryAdapter]` aspect-ratio gate, both at the edge; the course-placement projection (the station/elevation fold) is `Construction/layout#ASSEMBLY_FOLD`, composed not re-shown here; a `BondKind.Generated` bond is structurally distinct — it carries no template course set, so its `BondGeometry` descriptor's `Course` delegate DERIVES the placement from the unit-cell columns (the repeat width, the per-course orientation sequence, the diagonal rotation a herringbone/diaper emits into `Construction/assembly#PLACEMENT_MODEL` `Placement.PathAngleDegrees`, the flemish alternating stretcher/header pattern) rather than railing `ProfileFault.Bond`, the derivation living ON THE ROW (`[UseDelegateFromConstructor]`) so there is no central switch to grow and the once-parallel `BondPattern` enum is collapsed into the geometry data; the per-unit rotation a herringbone/diaper bond emits rides the existing `CourseTemplate`/`Placement` columns the `layout#ASSEMBLY_FOLD` `StepCourse` already consumes, never a widening of the `Placement` model, keeping the host-neutral scalar-`Placement` discipline; `ProfileCatalogue.BuildMasonryRows(context)` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the regional masonry `Profile` rows (us/uk/din/au/is), the `ProfileUnit` width/height/length/coursing columns admitting through `ProfileUnit.Of` into the kernel `PositiveMagnitude` value-object so the masonry unit never re-mints a length primitive, each row carrying its real fired-clay/calcium-silicate `MaterialId` (never one flat glazed appearance for every region — glaze is an `Appearance/graph` finish, not the base material) and its published `Coring` void class, and a malformed row drops through `Choose`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Foundation.CSharp.Analyzers.Contracts;   // [BoundaryAdapter] (the boundary-capsule marker on BondName.Fits)
using LanguageExt;                   // Fin, Option, Seq
using Rasm.Domain;                   // Context, Op
using Rasm.Element;                  // MaterialId (the seam appearance handle each masonry row carries)
using Thinktecture;                  // [SmartEnum]/[Union]/[UseDelegateFromConstructor]/KeyMemberEqualityComparer, ComparerAccessors
using Op = Rasm.Domain.Op;
using Rasm.Materials.Profiles;       // Profile/ProfileUnit/ProfileStandard/ProfileId/ProfileFault/ProfileFamily (the parent PROFILE_OWNER)
using static LanguageExt.Prelude;    // Seq, Seq1, toSeq, Some, None

// Each family page is its OWN Rasm.Materials.Profiles.<Family> sub-namespace so the six sibling `ProfileCatalogue` static
// classes are distinct types (one shared namespace would be a CS0101 collision); profile#PROFILE_OWNER stays the parent
// Rasm.Materials.Profiles and folds Masonry.ProfileCatalogue.BuildMasonryRows by the sub-namespace-qualified name. This
// page DEFINES the masonry vocabulary (Coring/Orientation/MortarType/MortarJoint/BondName) the cmu#CMU_FAMILY +
// Construction siblings import as Rasm.Materials.Profiles.Masonry; the parent owner types are composed via the using above.
namespace Rasm.Materials.Profiles.Masonry;

// --- [TYPES] -------------------------------------------------------------------------------
// The ASTM C652 / C90 net-area void class — the bounded threshold a SolidFraction falls into. ASTM C652 fixes the
// hollow-brick classes (H40V <=25% void at <=40% net, H60V) and C62/C216 the solid (>=75% net); NetAreaFloor is the
// minimum net-area ratio the class admits, so cmu#CMU_FAMILY ToCoring buckets a net-area fraction onto the matching row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CoringClass {
    public static readonly CoringClass Solid      = new("solid",      netAreaFloor: 0.75);   // ASTM C62/C216 solid clay
    public static readonly CoringClass Frogged    = new("frogged",    netAreaFloor: 0.80);   // single-face indentation, solid for net-area
    public static readonly CoringClass Cored      = new("cored",      netAreaFloor: 0.75);   // <=25% coring, structurally solid
    public static readonly CoringClass Cellular   = new("cellular",   netAreaFloor: 0.60);   // closed cavity one bed face
    public static readonly CoringClass Perforated = new("perforated", netAreaFloor: 0.50);   // through-perforations, net 50-75%
    public static readonly CoringClass Hollow     = new("hollow",     netAreaFloor: 0.00);   // ASTM C652 H40V/H60V hollow brick / C90 CMU
    public double NetAreaFloor { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Coring {
    public static readonly Coring None             = new("none",                voidFraction: 0.00, classification: CoringClass.Solid);
    public static readonly Coring Frog             = new("frog",                voidFraction: 0.10, classification: CoringClass.Frogged);
    public static readonly Coring Cored3Hole       = new("cored-3-hole",        voidFraction: 0.20, classification: CoringClass.Cored);
    public static readonly Coring Cellular         = new("cellular",            voidFraction: 0.35, classification: CoringClass.Cellular);
    public static readonly Coring Perforated10Cell = new("perforated-10-cell",  voidFraction: 0.42, classification: CoringClass.Perforated);
    public static readonly Coring Hollow3Cell      = new("hollow-3-cell",       voidFraction: 0.47, classification: CoringClass.Hollow);   // the 12-in 3-cell CMU void class — more web material than the 2-cell unit (cmu#CMU_FAMILY ToCoring)
    public static readonly Coring Hollow2Cell      = new("hollow-2-cell",       voidFraction: 0.50, classification: CoringClass.Hollow);
    // The shared void-class vocabulary spans clay-brick (cored/perforated/cellular) AND concrete-masonry (hollow 2-cell /
    // 3-cell) geometry, so cmu#CMU_FAMILY ToCoring buckets BOTH the 8-in 2-cell and the 12-in 3-cell unit onto a faithful
    // row rather than forcing the 3-cell unit onto the 2-cell label; the exact net section is cmu's ParametricSection.Hollow.
    // VoidFraction stays a double in [0,1): profile#PROFILE_OWNER Profile.Of guards `coring.VoidFraction is >= 0.0 and < 1.0`,
    // a relational pattern over the raw key — the bound is the guard's, not a re-minted UnitInterval the pattern cannot read.
    public double VoidFraction { get; }
    public CoringClass Classification { get; }
    public double NetAreaFraction => 1.0 - VoidFraction;   // the C90 net-area ratio the structural capacity scales by
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondKind {
    public static readonly BondKind Template = new("template");
    public static readonly BondKind Generated = new("generated");
}

// The generative bond descriptor: the unit-cell columns (repeat width, rotation, block size) PLUS the row's OWN
// course-deriving delegate, so the per-pattern derivation is DATA on the row — not an arm in a central switch. A new
// decorative bond is one BondGeometry row binding its Course delegate; the once-parallel BondPattern discriminant and
// the GeneratedBond.Interpret full-coverage switch are collapsed into this column (POLICY_VALUES + DERIVED_LOGIC).
// AspectLo/AspectHi bound the unit length/height ratio the cell tiles cleanly inside — Fits gates on it via Admits:
// a 45-degree diagonal cell (herringbone/diaper) needs a near-2:1 unit, a stack/running cell admits any positive ratio.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondGeometry {
    public static readonly BondGeometry Stack       = new("stack",       repeatUnits: 1, rotationDegrees: 0.0,  blockUnits: 1, aspectLo: 0.10, aspectHi: 8.0, course: Running);
    public static readonly BondGeometry Flemish     = new("flemish",     repeatUnits: 2, rotationDegrees: 0.0,  blockUnits: 1, aspectLo: 1.50, aspectHi: 5.0, course: Flemished);
    public static readonly BondGeometry Herringbone = new("herringbone", repeatUnits: 2, rotationDegrees: 45.0, blockUnits: 1, aspectLo: 1.60, aspectHi: 2.40, course: Diagonal);
    public static readonly BondGeometry Basketweave = new("basketweave", repeatUnits: 2, rotationDegrees: 90.0, blockUnits: 2, aspectLo: 1.60, aspectHi: 2.40, course: Woven);
    public static readonly BondGeometry Pinwheel    = new("pinwheel",    repeatUnits: 4, rotationDegrees: 90.0, blockUnits: 1, aspectLo: 1.40, aspectHi: 3.0, course: Rotary);
    public static readonly BondGeometry Diaper      = new("diaper",      repeatUnits: 4, rotationDegrees: 45.0, blockUnits: 1, aspectLo: 1.60, aspectHi: 2.40, course: Lattice);

    public int RepeatUnits { get; }          // unit-cell repeat length in units before the course offset cycles
    public double RotationDegrees { get; }   // per-unit off-horizontal rotation for diagonal/rotary patterns
    public int BlockUnits { get; }           // basketweave/diaper block size (units per woven block)
    public double AspectLo { get; }          // closed length/height band the cell tiles — see Fits (a ratio, not [0,1])
    public double AspectHi { get; }

    // The course derivation IS the row: every BondGeometry answers its own course by index. No central interpreter.
    [UseDelegateFromConstructor]
    public partial CourseTemplate Course(int courseIndex);

    public double OffsetFraction(int course) => RepeatUnits <= 1 ? 0.0 : (course % RepeatUnits) / (double)RepeatUnits;
    public bool Admits(double lengthOverHeight) => lengthOverHeight >= AspectLo && lengthOverHeight <= AspectHi;

    // --- [COURSE_DELEGATES]
    // Running/stack: a single stretcher course, every other course offset by the cell fraction (a half-unit for the
    // 2-cell, none for stack); the offset is the OffsetFraction the StepCursor reads — the base running cycle.
    static CourseTemplate Running(int course) =>
        new(Seq1(Orientation.Stretcher), Stack.OffsetFraction(course), 0.0);

    // Flemish: each course alternates stretcher-header per unit, every other course shifted a quarter unit so a header
    // centres over the stretcher below; the two-unit cell is the OffsetFraction the StepCursor reads.
    static CourseTemplate Flemished(int course) =>
        new(Seq(Orientation.Stretcher, Orientation.Header), (course & 1) == 0 ? 0.0 : 0.25, 0.0);

    // Herringbone: pairs of units laid at +/-45 degrees, the per-unit rotation alternating by course parity.
    static CourseTemplate Diagonal(int course) =>
        new(Seq(Orientation.Stretcher, Orientation.Stretcher), 0.0, (course & 1) == 0 ? Herringbone.RotationDegrees : -Herringbone.RotationDegrees);

    // Basketweave: N-unit blocks alternating horizontal/vertical (0/90 degrees) every BlockUnits courses.
    static CourseTemplate Woven(int course) =>
        new(toSeq(Enumerable.Range(0, Basketweave.BlockUnits)).Map(static _ => Orientation.Stretcher), 0.0, ((course / Basketweave.BlockUnits) & 1) == 0 ? 0.0 : Basketweave.RotationDegrees);

    // Pinwheel: four units rotated 0/90/180/270 about a square centre, the rotation stepping with the unit index.
    static CourseTemplate Rotary(int course) =>
        new(Seq(Orientation.Stretcher, Orientation.Header), Pinwheel.OffsetFraction(course), (course % 4) * Pinwheel.RotationDegrees);

    // Diaper: diamond lattice — units rotated +/-45 degrees on a four-course diagonal repeat, the offset stepping each course.
    static CourseTemplate Lattice(int course) =>
        new(Seq1(Orientation.Stretcher), Diaper.OffsetFraction(course), (course % 2 == 0 ? 1 : -1) * Diaper.RotationDegrees);
}

// The brick laying-orientation — which of the unit's three faces shows and how the unit consumes the course run/rise.
// A flat 6-case tag is the deleted form (the prior empty [Union]): each orientation carries its course FOOTPRINT as
// DATA so the Construction/layout#ASSEMBLY_FOLD reads RunFraction/RiseFraction off the row rather than the relocated
// FootprintRun/FootprintRise Switch. RunFraction is the per-unit course advance and RiseFraction the course height,
// both as multiples of the base stretcher slot (a stretcher 1.0/1.0; a header turns the unit 90° in plan so it
// advances a HALF slot 0.5 at the same rise 1.0; a soldier/sailor stand the unit vertical so the rise is the
// tripled-height 3.0 — a soldier on its end width-out, a sailor length-up; a rowlock/shiner lay the unit on edge so
// the rise doubles 2.0 — a rowlock a header-on-edge half-advance 0.5, a shiner a stretcher-on-edge full-advance 1.0).
// FaceShown names the presented face the Appearance/weathering engine reads. These columns MATCH the layout fold's
// footprint exactly (the relocation is behaviour-preserving); a new orientation (a bull-header, a diagonal) is one
// row carrying its footprint, never a per-orientation type and never a footprint Switch the next orientation grows.
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Cut {
    public static readonly Cut Whole = new("whole", lengthFraction: 1.000, bevelDegrees: 0.0);
    public static readonly Cut ThreeQuarter = new("three-quarter", lengthFraction: 0.750, bevelDegrees: 0.0);
    public static readonly Cut Half = new("half-bat", lengthFraction: 0.500, bevelDegrees: 0.0);
    public static readonly Cut Quarter = new("quarter-bat", lengthFraction: 0.250, bevelDegrees: 0.0);
    public static readonly Cut QueenCloser = new("queen-closer", lengthFraction: 0.250, bevelDegrees: 0.0);
    public static readonly Cut KingCloser = new("king-closer", lengthFraction: 0.750, bevelDegrees: 45.0);
    public static readonly Cut Bevel = new("bevel", lengthFraction: 1.000, bevelDegrees: 30.0);
    public double LengthFraction { get; }
    public double BevelDegrees { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ClosureRule {
    public static readonly ClosureRule None = new("none", closer: Cut.Whole);
    public static readonly ClosureRule QueenCloser = new("queen-closer", closer: Cut.QueenCloser);
    public static readonly ClosureRule KingCloser = new("king-closer", closer: Cut.KingCloser);
    public static readonly ClosureRule HalfBat = new("half-bat", closer: Cut.Half);
    public Cut Closer { get; }
}

// The BS 4729 / architectural special-shape vocabulary — each row carries its profile-modifier geometry, never a bare
// identity tag. RadiusFraction rounds an edge by that fraction of the unit height (bullnose single, cownose double),
// ChamferDegrees cuts a splay (cant/squint), SetbackFraction steps the face in (plinth), WashFraction slopes the top
// (coping throating). The layout reads SpecialShape.Voussoir by identity for the arch wedge; the appearance/weathering
// engine reads the modifier columns for the rounded/splayed silhouette so a special unit is one row, never a subtype.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpecialShape {
    public static readonly SpecialShape None       = new("none",       radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00);
    public static readonly SpecialShape Bullnose   = new("bullnose",   radiusFraction: 0.50, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00);   // single rounded arris
    public static readonly SpecialShape Cownose    = new("cownose",    radiusFraction: 1.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00);   // double-bullnose half-round end
    public static readonly SpecialShape Plinth      = new("plinth",     radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.25, washFraction: 0.20);   // chamfered set-back base course
    public static readonly SpecialShape Coping     = new("coping",     radiusFraction: 0.10, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.30);   // weathered wall-cap with wash
    public static readonly SpecialShape Cant       = new("cant",       radiusFraction: 0.00, chamferDegrees: 45.0, setbackFraction: 0.00, washFraction: 0.00);   // single splay
    public static readonly SpecialShape Squint     = new("squint",     radiusFraction: 0.00, chamferDegrees: 30.0, setbackFraction: 0.00, washFraction: 0.00);   // oblique-angle corner unit
    public static readonly SpecialShape Birdsmouth = new("birdsmouth", radiusFraction: 0.00, chamferDegrees: 60.0, setbackFraction: 0.00, washFraction: 0.00);   // internal-angle notch
    public static readonly SpecialShape Voussoir   = new("voussoir",   radiusFraction: 0.00, chamferDegrees: 0.0,  setbackFraction: 0.00, washFraction: 0.00);   // wedge — taper is the arch sweep, not a unit modifier
    public double RadiusFraction { get; }    // arris round as a fraction of unit height
    public double ChamferDegrees { get; }    // splay angle off the face
    public double SetbackFraction { get; }   // face set-back as a fraction of unit width
    public double WashFraction { get; }      // top-slope rise as a fraction of unit height
    public bool ModifiesProfile => RadiusFraction > 0.0 || ChamferDegrees > 0.0 || SetbackFraction > 0.0 || WashFraction > 0.0;
}

// The ASTM C270 tooled-joint profile: DepthFactor scales the bed-joint width to the recess depth a raked/struck
// joint carries (0 = flush), ShadowLine the relative shadow weight the weathering#WEATHERING raked-joint AO reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarProfile {
    public static readonly MortarProfile Concave   = new("concave",   depthFactor: 0.10, shadowLine: 0.3, weatherTight: true);
    public static readonly MortarProfile Vee       = new("v",         depthFactor: 0.15, shadowLine: 0.5, weatherTight: true);
    public static readonly MortarProfile Weathered = new("weathered", depthFactor: 0.20, shadowLine: 0.4, weatherTight: true);
    public static readonly MortarProfile Struck    = new("struck",    depthFactor: 0.20, shadowLine: 0.6, weatherTight: false);
    public static readonly MortarProfile Raked     = new("raked",     depthFactor: 0.50, shadowLine: 1.0, weatherTight: false);
    public static readonly MortarProfile Flush     = new("flush",     depthFactor: 0.00, shadowLine: 0.0, weatherTight: true);
    public static readonly MortarProfile Beaded    = new("beaded",    depthFactor: -0.10, shadowLine: 0.7, weatherTight: true);
    public double DepthFactor { get; }
    public double ShadowLine { get; }
    public bool WeatherTight { get; }
}

// The ASTM C270 mortar classification, full row not a bare compressive scalar: the 28-day minimum compressive strength
// (M=17.2, S=12.4, N=5.2, O=2.4, K=0.5 MPa), the EN 998-2 M-class the strength maps to (M >= EN M15, S >= M10, N >= M5,
// O >= M2.5, K below class), the ASTM C270 Table 2 cement:lime:sand proportion-spec volumes, and the minimum flexural
// tensile bond the masonry shear/flexure capacity reads. The EnClassMpa lets the thermal/structural seam cite either
// standard. A new class (Type-K lime, an EN M20 high-strength) is one row carrying its full spec, never a parallel type.
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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BondName {
    // Template bonds carry an explicit course set; generated bonds reference a BondGeometry whose own delegate computes the course.
    public static readonly BondName Running       = new("running",        template: Seq(StretcherAt(0.0), StretcherAt(0.5)));
    public static readonly BondName English        = new("english",        template: Seq(StretcherAt(0.0), HeaderAt(0.5)));
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

    // The real aspect-ratio gate (not a length>0 tautology): a template bond admits any positive unit; a generated bond
    // admits only when the unit's length/height lies in the descriptor's [AspectLo, AspectHi] band the cell tiles cleanly
    // — a herringbone or basketweave rejects a near-square unit that cannot lay a 45-degree diagonal cell.
    [BoundaryAdapter]
    public bool Fits(Profile profile) =>
        profile.Unit.HeightMm.Value > 0.0 &&
        Geometry.Match(Some: g => g.Admits(profile.Unit.LengthOverHeight), None: () => true);

    // A template bond reads its course by wrapped index; a generated bond invokes its descriptor's own course delegate.
    public Fin<CourseTemplate> Course(int index, Op key) => Kind.Switch(
        template:  _ => Courses.IsEmpty
            ? Fin.Fail<CourseTemplate>(ProfileFault.Bond(key, $"<template-bond-empty:{Key}>"))
            : Fin.Succ(Courses[((index % Courses.Count) + Courses.Count) % Courses.Count]),
        generated: _ => Geometry
            .ToFin(ProfileFault.Bond(key, $"<generated-bond-missing-geometry:{Key}>"))
            .Map(geometry => geometry.Course(index)));

    static CourseTemplate StretcherAt(double offset) => new(Seq1(Orientation.Stretcher), offset, 0.0);
    static CourseTemplate HeaderAt(double offset) => new(Seq1(Orientation.Header), offset, 0.0);
}

// --- [MODELS] ------------------------------------------------------------------------------
// PerUnitRotationDegrees carries the off-horizontal rotation a herringbone/diaper unit needs; the
// Construction/layout#ASSEMBLY_FOLD StepCourse folds it into Placement.PathAngleDegrees, never a host transform.
public sealed record CourseTemplate(Seq<Orientation> Sequence, double OffsetFraction, double PerUnitRotationDegrees = 0.0);

// The full mortar-joint specification the layout#ASSEMBLY_FOLD JointPolicy resolves head/bed width AND profile
// AND mortar strength from, replacing the single scalar ProfileStandard.StandardJointThicknessMm: a masonry run
// carries its buildable joint detail, the weathering raked-joint shadow line, and the ASTM C270 strength receipt.
// Of rails the profile#PROFILE_OWNER ProfileFault.Mortar slot on a non-positive head/bed width — the joint-SPEC fault
// disjoint from ProfileFault.Bond (the course-pattern fault), not the layout-tier ConstructionFault.Joint.
public readonly record struct MortarJoint(double HeadWidthMm, double BedWidthMm, MortarProfile Profile, MortarType Mortar) {
    public static Fin<MortarJoint> Of(double headMm, double bedMm, MortarProfile profile, MortarType mortar, Op key) =>
        double.IsFinite(headMm) && headMm > 0.0 && double.IsFinite(bedMm) && bedMm > 0.0
            ? Fin.Succ(new MortarJoint(headMm, bedMm, profile, mortar))
            : Fin.Fail<MortarJoint>(ProfileFault.Mortar(key, $"<mortar-joint-nonpositive:head={headMm}:bed={bedMm}>"));

    // Standard coordinating joint from a single thickness — the scalar StandardJointThicknessMm an unspecified run uses.
    public static MortarJoint Standard(double thicknessMm) => new(thicknessMm, thicknessMm, MortarProfile.Concave, MortarType.N);
    public double RecessDepthMm => BedWidthMm * Profile.DepthFactor;
}

// The regional raw row: the published coordinating dimensions PLUS the unit's real void class and base appearance,
// so a DIN solid clay (Coring.None, fired clay) and a UK perforated calcium-silicate carry their OWN material, never
// one flat ceramic.glazed for every region. Appearance is the Appearance/graph#MATERIAL_LIBRARY MaterialId key.
// Authority is the bounded profile#PROFILE_OWNER ProfileAuthority [SmartEnum] row the ProfileStandard carries (NOT a
// free authority string), Region an explicit token because a din/au/is regional row carries a region the body does not name.
public readonly record struct MasonryRow(string Designation, double WMm, double HMm, double LMm, double CourseMm, double JointMm, string Region, ProfileAuthority Authority, Coring Coring, string Appearance);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    // Each regional unit carries its published void class and its real base material — a US modular cored fired-clay
    // brick, a UK perforated calcium-silicate, a DIN solid Vollziegel fired clay, an AU cored clay, an IS solid clay.
    static readonly Seq<MasonryRow> RegionalRows = Seq(
        new MasonryRow("masonry.us-modular",   92.0,  57.0,  194.0, 67.0,  9.5,  "us",  ProfileAuthority.Astm, Coring.Cored3Hole,       "ceramic.fired-clay"),       // ASTM C216
        new MasonryRow("masonry.uk-standard",  102.5, 65.0,  215.0, 75.0,  10.0, "uk",  ProfileAuthority.Bs,   Coring.Perforated10Cell, "ceramic.calcium-silicate"), // BS EN 771-1
        new MasonryRow("masonry.din-nf",       115.0, 71.0,  240.0, 83.5,  12.5, "din", ProfileAuthority.Din,  Coring.None,             "ceramic.fired-clay"),       // DIN 105
        new MasonryRow("masonry.au-standard",  110.0, 76.0,  230.0, 86.0,  10.0, "au",  ProfileAuthority.As,   Coring.Cored3Hole,       "ceramic.fired-clay"),       // AS 4773
        new MasonryRow("masonry.is-standard",  100.0, 70.0,  200.0, 80.0,  10.0, "is",  ProfileAuthority.Is,   Coring.None,             "ceramic.fired-clay"));      // IS 1077

    static Fin<(ProfileId Id, Profile Profile)> MasonryOf(MasonryRow r, Context context, Op key) =>
        from unit in ProfileUnit.Of(r.WMm, r.HMm, r.LMm, r.CourseMm, context, key)
        let standard = new ProfileStandard(r.Region, r.JointMm, r.Authority)
        select (ProfileId.Of(r.Designation), new Profile(ProfileFamily.Masonry, unit, r.Coring, standard, MaterialId.Of(r.Appearance)));

    public static FrozenDictionary<ProfileId, Profile> BuildMasonryRows(Context context) =>
        RegionalRows
            .Choose(row => MasonryOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [GENERATED_BOND_INTERPRETER]: REALIZED — the generated-bond derivation is DATA on each `BondGeometry` row: a `[UseDelegateFromConstructor]` `Course(int courseIndex)` delegate column derives the `CourseTemplate` (orientation sequence + `OffsetFraction` + `PerUnitRotationDegrees`) from the descriptor's unit-cell columns (repeat-units, rotation, block-units), so the stack/flemish/herringbone/basketweave/pinwheel/diaper generated bonds COMPUTE their course rather than railing `ProfileFault.Bond`. This collapses the prior two-surface shape — the parallel `BondPattern` discriminant AND the `GeneratedBond.Interpret` full-coverage `Switch` — into one row-owned delegate per `POLICY_VALUES` + `DERIVED_LOGIC` (the vocabulary item IS the behavior, never a central interpreter the next bond grows an arm in). A new decorative bond (quetta, monk, sussex) is one `BondGeometry` row binding its `Course` delegate plus one `BondName` row, never a `BondPattern` case, an `Interpret` arm, or a parallel generated-layout owner. The per-unit rotation a herringbone/diaper/pinwheel emits rides the `CourseTemplate.PerUnitRotationDegrees` column the `Construction/layout#ASSEMBLY_FOLD` `StationStep` folds into `Construction/assembly#PLACEMENT_MODEL` `Placement.PathAngleDegrees`, never a widening of the host-neutral `Placement` model; the course delegates reference sibling `BondGeometry` rows (`Stack.OffsetFraction`, `Herringbone.RotationDegrees`) deferred-behind-delegate per the smart-enum init-order law, materialized only at call time.
- [BOND_FIT_GATE]: REALIZED — `BondName.Fits` is the real `[BoundaryAdapter]` aspect-ratio gate, not a `LengthOverHeight > 0.0` tautology a `PositiveMagnitude`-backed unit always satisfies: a template bond admits any positive-height unit, and a generated bond admits only when the unit's length-over-HEIGHT lies inside the descriptor's aspect band (`BondGeometry.AspectLo`/`AspectHi`) the cell tiles cleanly — a herringbone or basketweave 45-degree diagonal cell needs a near-2:1 unit (`[1.6, 2.4]`) and rejects a near-square unit that would lay a degenerate diagonal course, while a stack/running cell admits the broad `[0.1, 8.0]` band. The gate reads the unit's aspect through the `profile#PROFILE_OWNER` owner-provided `ProfileUnit.LengthOverHeight` projection (ONE_HOP_RESOLUTION — never the `LengthMm.Value / HeightMm.Value` inline re-spelling), and the `Admits(lengthOverHeight)` predicate reads the band columns, so a new bond's tiling constraint is one `AspectLo`/`AspectHi` pair on its `BondGeometry` row, never a hardcoded ratio check in the fit method. Ripple counterpart: `profile#PROFILE_OWNER` `[PROFILE_OWNER]` (the `ProfileUnit.LengthOverHeight` aspect projection this gate composes).
- [MORTAR_JOINT_PROFILE]: REALIZED — the `MortarProfile` `[SmartEnum]` (concave/V/weathered/struck/raked/flush/beaded ASTM C270 tooled-joint profiles, each carrying a `DepthFactor` recess scale and a `ShadowLine` weight) and the `MortarType` `[SmartEnum]` (M/S/N/O/K) close the mortar vocabulary, and the `MortarJoint` record (head-width, bed-width, profile, mortar type) is the full buildable joint specification the `Construction/layout#ASSEMBLY_FOLD` `JointPolicy.Resolve` reads beside the coordinating thickness — replacing the single scalar `ProfileStandard.StandardJointThicknessMm` with head AND bed AND profile AND strength. `MortarType` is the FULL ASTM C270 classification row, not a bare compressive scalar: the 28-day minimum compressive strength (17.2/12.4/5.2/2.4/0.5 MPa), the mapped EN 998-2 `M`-class (`EnClassMpa`), the ASTM C270 Table 2 cement:lime:sand proportion-spec volumes, and the minimum flexural tensile bond (`FlexuralBondMpa`) the masonry shear/flexure capacity seam reads — so the thermal/structural seam cites either standard and the masonry-capacity check reads the bond strength directly. A non-positive joint width rails `ProfileFault.Mortar` at `MortarJoint.Of` — the profile-tier mortar-joint-SPEC slot the `profile#PROFILE_OWNER` `ProfileFault` `[Union]` carries beside `Bond` (a course-pattern defect is `Bond`, a joint-width/spec defect is `Mortar`, the two disjoint exactly as `Section` is disjoint from `Family`), and distinct from the layout-tier `Construction/assembly#PLACEMENT_MODEL` `ConstructionFault.Joint` the run fold rails on the resolved coordinating joint; an unspecified run reads `MortarJoint.Standard(thicknessMm)` (the concave/Type-N coordinating default) so the scalar route survives as the default. The `MortarProfile.ShadowLine`/`RecessDepthMm` is the source the `weathering#WEATHERING` raked-joint cavity-mask AO reads, and the `StationStep` pitch reads the `HeadWidthMm` as the per-unit head joint unchanged.
- [SPECIAL_SHAPE_GEOMETRY]: REALIZED — `SpecialShape` carries its profile-modifier geometry as columns, never a bare identity tag: `RadiusFraction` rounds an arris (bullnose single, cownose half-round end), `ChamferDegrees` splays a face (cant/squint/birdsmouth), `SetbackFraction` steps the face in (plinth), `WashFraction` slopes the top (coping throating), and `ModifiesProfile` is the derived "this shape alters the unit silhouette" predicate. The `Construction/layout#ASSEMBLY_FOLD` reads `SpecialShape.Voussoir` by identity for the arch wedge (the taper is the arch sweep, not a unit modifier, so its modifier columns are zero); the `Appearance/graph` / `weathering#WEATHERING` engine reads the modifier columns for the rounded/splayed/washed silhouette. A new special shape (a plinth-stretcher, a saddleback coping) is one `SpecialShape` row carrying its modifier, the prior `[01]-[INDEX]` `SpecialShape.Catalog` phantom retired — the vocabulary IS the `Items` set.
- [CORING_NET_AREA]: REALIZED — `Coring` spans the real void-class range (solid/frog/cored/cellular/perforated/hollow) and each row carries a `UnitInterval VoidFraction` plus a `CoringClass` whose `NetAreaFloor` is the ASTM C652 / C90 minimum net-area ratio the class admits (C62/C216 solid >=75% net, C652 H40V/H60V hollow), with `Coring.NetAreaFraction` the `1 − VoidFraction` ratio the masonry compressive capacity scales by. The `cmu#CMU_FAMILY` `CmuSection.ToCoring` buckets a computed net-area solid fraction onto the matching row, so the masonry void vocabulary and the cmu net-section share one classification; a new void class (an A-series cellular, a C652 H80V) is one `Coring` row binding its `CoringClass`, never a per-unit type.
- [MASONRY_ROW_TRANSCRIPTION]: REALIZED — the regional masonry cross-section catalogue (the us/uk/din/au/is regional rows) seeds through `ProfileCatalogue.BuildMasonryRows(context)` over the `MasonryRow` raw-double table, the dimension/coursing/joint columns admitting once through `ProfileUnit.Of` into the kernel-`PositiveMagnitude`-composed `ProfileUnit` and the `BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape` algebra realized as the masonry vocabulary; each row carries its published `Coring` void class AND its real base `MaterialId` (a US modular cored fired-clay, a UK perforated calcium-silicate, a DIN solid Vollziegel fired clay), so the catalogue never assigns one flat `ceramic.glazed` appearance to every region — glaze is an `Appearance/graph` finish, not the base material. The five regional rows are the seed the `Construction/layout#ASSEMBLY_FOLD` course fold consumes and a new region is one `MasonryRow` data addition; the standard module/joint values transcribe the ASTM C216 / BS EN 771-1 / DIN 105 / AS 4773 / IS 1077 published dimensions, and a non-positive column rails the `ProfileUnit.Of` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `Profile`.
