# [MATERIALS_MASONRY]

THE MASONRY SEED PAGE and THE GENERATIVE BOND ALGEBRA. Masonry is one `ComponentFamily` policy row (`ComponentFamily.Masonry`: `ComponentClass.Minor`, `DetailLane.Realization`, admits `SectionProfile.Rectangle` or `SectionProfile.CellularRectangle`, cross-nominal `GrossRectangleMm.WidthMm`) — a masonry unit is a `Component` row, never a `Brick` type. This page owns the FORM-law vocabularies (`Orientation`/`FaceShown`/`Cut`/`ClosureRule`/`SpecialShape`/`RecessShape`/`MortarProfile`/`MortarType`/`MortarSystem`/`EnMortarClass`/`EnMortarKind`/`RuptureModulus`/`FlexuralStrengthEn`/`FlexuralStrengthNa`/`NaMortarBand`/`LateralAction`/`SizeTolerance`/`SizeRange`/`RatingPeriod`/`BondGeometry`/`BondName`), the `MortarJoint` joint specification, the `FrogGeometry`/`Perforation` generative void geometry, the `AUTHORED` regional row table, and the `MasonrySeed.Roster`/`Law` pair the ONE `component#COMPONENT_SEED` generator folds. A masonry unit carries no payload record of its own: its geometry is the BED-PLANE `SectionProfile.Rectangle`/`CellularRectangle` (W through-wall × L along-wall), its frog/perforation voids are TRUE-GRID `VoidCell` rows the `MasonryVoids` generator stations in that plane, its vocabulary columns are the kept SmartEnums, and its `HeightMm`/`CourseHeightMm`/tolerance/range/shape realization columns are seed-row values landing in the seed-built `MasonryDetail` bag (`Masonry` carries `DetailLane.Realization` — the EN 771-1 work-vs-actual tolerance band, the unit height, and the coursing module have no other landing surface).

The unreinforced tension screen rides TWO published table families into the `capacity#SECTION_CAPACITY` `MasonryUnreinforced` columns through the `SectionCapacity.Lift(lift, key)` `CapacityLift.Masonry` case, the lift's `DesignBasis` selecting the read: `RuptureModulus` the TMS 402 Table `9.1.9.2` modulus of rupture `fr` keyed by `MortarSystem`/`MortarType`; `FlexuralStrengthEn` the EN 1996-1-1 clause `3.6.3(3)` NOTE-2 flexural pair `f_xk1`/`f_xk2` keyed by unit group, carrying beside it the Table `3.4` initial shear strength `f_vko` the same unit group keys; `FlexuralStrengthNa` the UK National Annex Table `NA.6` supersession keyed by water absorption, unit format, and wall thickness. One direction column (`RuptureModulus.SpanParallelToBed`) serves every read, the mortar feed off the assemblage, never `MortarType.FlexuralBondMpa` (ASTM C1072 bond data, firewalled from `fr`). EN 1996-1-1 `8.1.4.1(2)P` requires unreinforced units to overlap on alternate courses, so an un-bonded pattern is not a weaker table row but an inadmissible unreinforced wall: `MasonrySeed.Capacity` REFUSES it typed, and `BondName.Overlap` derives the `8.1.4.1(3)` overlap from the bond's own course stagger.

The `Bond` axis is a GENERATIVE ALGEBRA over a THEOREM-CLOSED census: a template bond reads its course set by wrapped index, a generated bond IS a plane-symmetry tiling — its `BondGeometry` row names a `Rasm/Parametric/patternmap#PATTERNING` `WallpaperGroup` (the complete 17-row plane census) with its motif seats in cell coordinates, and the FULL per-unit packing transform (offset + lateral + rotation + mirror parity) derives from the kernel `Patterning.Apply(PatternOp.Orbit)` fold read band by band, the unit's own coordinating module the cell basis — so a new decorative bond is DATA over a closed theorem, never a per-index delegate and never an interpreter arm. The course fold, joint policy, and station projection consuming `CourseTemplate`/`UnitPlacement`/`MortarJoint` are the app root's; the shared `Coring` void class, `VoidCell`, `ComponentUnit`, `SeedJoin`, and `ComponentDetail` bag constructors are `component#COMPONENT_OWNER`; the cmu sibling buckets onto the same `Coring` vocabulary and shares the `CellularRectangle` profile arm.

## [01]-[INDEX]

- [02]-[MASONRY_FAMILY]: the retained bond/orientation/cut/closure/special-shape/mortar/tolerance vocabularies with the wallpaper-group `BondGeometry` symmetry descriptor, the `BondName` template/generated catalogue with its EC6 overlap derivation, the `[ComplexValueObject]` `MortarJoint` ASTM C270 joint specification, the EN 998-2 `EnMortarClass`/`EnMortarKind` product vocabulary the ASTM `MortarType` derives its declared class against, the `MortarSystem` cementitious-system rows with the TMS 402 Table `9.1.9.2` `RuptureModulus` table, the EN 1996-1-1 clause `3.6.3(3)`/Table `3.4` `FlexuralStrengthEn` sibling and its `FlexuralStrengthNa` UK-annex supersession, the `FrogGeometry`/`Perforation` void geometry with the ONE `MasonryVoids` owner, the EN 771-1 `SizeTolerance`/`SizeRange` work-vs-actual bands admitted on the kernel `Grade` tolerance lane, the `RatingPeriod` published fire-period vocabulary both coursing families floor through, the `MasonryBody` substance axis with the `MasonryPhysics` row and the `WallAcoustics` single-leaf mass-law fold, the `AUTHORED` `MasonryRow` regional table, the seed-built `MasonryDetail` realization bag with its `Properties` contract lowering, the `MasonrySeed.Roster`/`Law` pair, and the `MasonrySeed.Capacity` basis-admitting producer.

## [02]-[MASONRY_FAMILY]

- Owner: the masonry vocabulary (all FORM-law `[SmartEnum]`, each stacking `[KeyMemberComparer]` beside `[KeyMemberEqualityComparer]` so ordered key lookup matches the `ComponentFamily` row convention); `MasonrySeed` the roster, the seed law, and the basis-admitting capacity producer; `RatingPeriod` the published fire-period ladder; `UnitPlacement`/`CourseTemplate` the per-unit course transform; `BondOverlap` the EC6 overlap derivation; `MortarJoint` the generated-admission joint specification; `FrogGeometry`/`Perforation` the generative void geometry; `MasonryVoids` the `VoidCell` derivation + fallible void-class bucket; `MasonryBody` the substance axis keyed on its own `MaterialId`; `MasonryDetail` the seed-time realization bag AND the contract `Properties` lowering; `MasonryRow` the `AUTHORED` raw row; `MasonrySeed` the ONE fold the `component#CATALOGUE` composes with its `SeedJoin` body join.
- Cases: orientation {stretcher/header/soldier/sailor/rowlock/shiner, each carrying its run/rise course footprint and its exposed `FaceShown`} · cut {whole/three-quarter/half-bat/quarter-bat/queen-closer/king-closer/bevel, each carrying its cut-plane remainder + plane-normal orientation} · closure {none/queen-closer/king-closer/half-bat} · special-shape {none/bullnose/cownose/plinth/coping/cant/squint/birdsmouth/voussoir, each carrying its profile modifier + the voussoir radial taper} · mortar-profile {concave/v/weathered/struck/raked/flush/beaded/squeezed, each carrying its `RecessShape` cross-section} · mortar-type {M/S/N/O/K, the ASTM C270 proportion and strength row whose EN class DERIVES} · en-mortar-class {M1/M2,5/M5/M10/M15/M20 categorial + `Md` open} · en-mortar-kind {G/T/L} · mortar-system {portland-lime/mortar-cement/masonry-cement/air-entrained-portland-lime} · rupture-modulus {8 direction×form rows × 4 published MPa columns} · en flexural {7 unit-group rows × the four-column `FlexuralBand` pair × the Table 3.4 shear band} · na flexural {13 UK-annex rows × the three-column `NaBand` pair} · lateral-action {transient/permanent} · tolerance {T1/T2/Tm} · range {R1/R2/Rm} · authority {ASTM/BS/DIN/AS/IS, each carrying the region its own standards body publishes into} · profile {`Rectangle` a solid unit, `CellularRectangle` a frogged/cored unit}.
- Entry: `ComponentSeed.Rows(context, MasonrySeed.Roster, MasonrySeed.Law)` — this page states the roster and the policy, never the fold, and the law's coherence proves the coordinating module, the whole frog declaration, the four dimensional columns through `ComponentUnit.Of`, the derived void fraction, and the EN 771-1 deviation and range bands TOGETHER, so a malformed row names every column it broke in one verdict. `BondName.Course(unit, jointMm, index, key)` resolves a course template or orbits the generated descriptor's wallpaper group and reads the band, `BondGeometry.Courses(unit, jointMm, courses, key)` the whole-stack primary the single-index read projects; `BondName.Fits(ComponentUnit)` is the aspect-ratio tiling gate and `BondName.Overlap(unit, jointMm, key)` the EN 1996-1-1 `8.1.4.1(3)` overlap derivation; `MasonryDetail.Properties(profile, body, key)` lowers the `MasonryPhysics` row onto the contract `Thermal`/`Acoustic`/`Fire` cases; `SizeTolerance.WorkEnvelope(workMm, declaredMm, key)` is the EN 771-1 permitted actual-size range, ADMITTED on the kernel `Grade` tolerance lane, that the coursing tolerance and the GLB tessellation read.
- Packages: Rasm (project — `PositiveMagnitude` from `Rasm.Numerics`; `Op`/`Context`/`AcceptValidated` from `Rasm.Domain`), Rasm.Element (project — `MaterialId`, `PropertyBag`, the contract `DetailSchema`/`PropertyCategory`/`PropertyName`/`Dimension` currencies `MasonryDetail` composes, and the `MaterialPropertySet` its `Properties` lowering mints; every `DetailSchema.Realization` row a unit stamps is Element-declared at `Rasm.Element/Properties/property#DETAIL_SCHEMA`, never minted here), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]`, `[ComplexValueObject]` + generated `ValidateFactoryArguments`/`Validate`, `[UseDelegateFromConstructor]`, `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]`, `ComparerAccessors`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`Traverse` — the fail-loud RESULT law), BCL inbox; NO `VividOrange` — masonry is `AUTHORED` under `SEED_ROW_LAW` (no admitted producer owns EN 771/ASTM C216 masonry-unit tables; every value restates verbatim with per-column provenance).
- Growth: a new template bond is one `BondName` row carrying its course set; a new generated bond is one `BondGeometry` row naming its wallpaper group and motif seats beside one `BondName` row — the symmetry census is closed by theorem, so the row is DATA and never a new derivation; a new orientation/cut/shape/mortar/tolerance row is one SmartEnum row; a new `fr` direction/form row, EN unit group, UK-annex row, or cementitious system is one `RuptureModulus`/`FlexuralStrengthEn`/`FlexuralStrengthNa`/`MortarSystem` row; a thin-layer or lightweight mortar is one `MortarType` row declaring that `EnMortarKind`, which arms the EN table's T and L columns with zero type edits; a new regional unit is one `MasonryRow` — per `[DIFF_OF_NEXT_THING]`. A sibling family lands its own vocabulary on its own seed page.
- Boundary: this page emits profiles, vocabulary rows, bags, contract property sets, and the seed fold. `MasonryVoids.Bucket` refuses an invalid derived fraction on `CoringRejected`; bond-orbit refusals lower once to `BondRejected`, and section-map membership derives from `ProfileTopology.Solvable`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Dimension = Rasm.Element.Properties.Dimension;
using Rasm.Parametric;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FaceShown {
    public static readonly FaceShown Stretcher = new("stretcher");
    public static readonly FaceShown Header    = new("header");
    public static readonly FaceShown Bed       = new("bed");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Orientation {
    public static readonly Orientation Stretcher = new("stretcher", runFraction: 1.0,       riseFraction: 1.0, faceShown: FaceShown.Stretcher);
    public static readonly Orientation Header    = new("header",    runFraction: 0.5,       riseFraction: 1.0, faceShown: FaceShown.Header);
    public static readonly Orientation Soldier   = new("soldier",   runFraction: 1.0 / 3.0, riseFraction: 3.0, faceShown: FaceShown.Stretcher);
    public static readonly Orientation Sailor    = new("sailor",    runFraction: 0.5,       riseFraction: 3.0, faceShown: FaceShown.Bed);
    public static readonly Orientation Rowlock   = new("rowlock",   runFraction: 1.0 / 3.0, riseFraction: 1.5, faceShown: FaceShown.Header);
    public static readonly Orientation Shiner    = new("shiner",    runFraction: 1.0,       riseFraction: 1.5, faceShown: FaceShown.Bed);
    public double RunFraction { get; }
    public double RiseFraction { get; }
    public FaceShown FaceShown { get; }
}

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
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecessShape {
    public static readonly RecessShape ConcaveArc  = new("concave-arc");
    public static readonly RecessShape VeeGroove   = new("vee-groove");
    public static readonly RecessShape Sloped      = new("sloped");
    public static readonly RecessShape Rectangular = new("rectangular");
    public static readonly RecessShape Flat        = new("flat");
    public static readonly RecessShape ConvexBead  = new("convex-bead");
    public static readonly RecessShape Extruded    = new("extruded");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarProfile {
    public static readonly MortarProfile Concave   = new("concave",   depthFactor: 0.10,  shadowLine: 0.3, recessShape: RecessShape.ConcaveArc,  slopeDegrees: 0.0);
    public static readonly MortarProfile Vee       = new("v",         depthFactor: 0.15,  shadowLine: 0.5, recessShape: RecessShape.VeeGroove,   slopeDegrees: 0.0);
    public static readonly MortarProfile Weathered = new("weathered", depthFactor: 0.20,  shadowLine: 0.4, recessShape: RecessShape.Sloped,      slopeDegrees: 15.0);
    public static readonly MortarProfile Struck    = new("struck",    depthFactor: 0.20,  shadowLine: 0.6, recessShape: RecessShape.Sloped,      slopeDegrees: -15.0);
    public static readonly MortarProfile Raked     = new("raked",     depthFactor: 0.50,  shadowLine: 1.0, recessShape: RecessShape.Rectangular, slopeDegrees: 0.0);
    public static readonly MortarProfile Flush     = new("flush",     depthFactor: 0.00,  shadowLine: 0.0, recessShape: RecessShape.Flat,        slopeDegrees: 0.0);
    public static readonly MortarProfile Beaded    = new("beaded",    depthFactor: -0.10, shadowLine: 0.7, recessShape: RecessShape.ConvexBead,  slopeDegrees: 0.0);
    public static readonly MortarProfile Squeezed  = new("squeezed",  depthFactor: -0.20, shadowLine: 0.8, recessShape: RecessShape.Extruded,    slopeDegrees: 0.0);
    public double DepthFactor { get; }
    public double ShadowLine { get; }
    public RecessShape RecessShape { get; }
    public double SlopeDegrees { get; }

    public bool WeatherTight => SlopeDegrees >= 0.0 && DepthFactor >= 0.0 && DepthFactor < RakedRetentionDepth;
    const double RakedRetentionDepth = 0.50;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnMortarClass {
    public static readonly EnMortarClass M1   = new("M1",   Some(1.0));
    public static readonly EnMortarClass M2_5 = new("M2,5", Some(2.5));
    public static readonly EnMortarClass M5   = new("M5",   Some(5.0));
    public static readonly EnMortarClass M10  = new("M10",  Some(10.0));
    public static readonly EnMortarClass M15  = new("M15",  Some(15.0));
    public static readonly EnMortarClass M20  = new("M20",  Some(20.0));
    public static readonly EnMortarClass Md   = new("Md",   Option<double>.None);
    public Option<double> FloorMpa { get; }

    const double OpenClassFloorMpa = 20.0;
    const double OpenClassStepMpa = 5.0;

    public static Option<EnMortarClass> Of(double compressiveMpa) =>
        !double.IsFinite(compressiveMpa) || compressiveMpa < 1.0
            ? Option<EnMortarClass>.None
            : compressiveMpa > OpenClassCeilingMpa ? Some(Md)
            : toSeq(Items.OrderByDescending(static c => c.FloorMpa.IfNone(double.MaxValue)))
                .Filter(c => c.FloorMpa.Exists(floor => compressiveMpa >= floor))
                .Head;

    static double OpenClassCeilingMpa => M20.FloorMpa.IfNone(OpenClassFloorMpa);

    public Fin<double> DeclaredMpa(double declaredMpa, Op key) =>
        this != Md
            ? FloorMpa.ToFin(new ComponentFault.MortarUnavailable(key, declaredMpa))
            : double.IsFinite(declaredMpa) && declaredMpa > OpenClassFloorMpa
                && Math.Abs(declaredMpa % OpenClassStepMpa) < EpsilonPolicy.ZeroTolerance
                ? Fin.Succ(declaredMpa)
                : new ComponentFault.MortarUnavailable(key, declaredMpa);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnMortarKind {
    public static readonly EnMortarKind General     = new("G", maxAggregateMm: Option<double>.None, maxDryDensityKgM3: Option<double>.None);
    public static readonly EnMortarKind ThinLayer   = new("T", maxAggregateMm: Option<double>.None, maxDryDensityKgM3: Option<double>.None);
    public static readonly EnMortarKind Lightweight = new("L", maxAggregateMm: Option<double>.None, maxDryDensityKgM3: Some(1300.0));
    public Option<double> MaxAggregateMm { get; }
    public Option<double> MaxDryDensityKgM3 { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MortarType {
    public static readonly MortarType M = new("M", compressiveMpa: 17.2, cement: 1.0, lime: 0.25, sand: 3.50, flexuralBondMpa: 0.40, kind: EnMortarKind.General);
    public static readonly MortarType S = new("S", compressiveMpa: 12.4, cement: 1.0, lime: 0.50, sand: 4.50, flexuralBondMpa: 0.30, kind: EnMortarKind.General);
    public static readonly MortarType N = new("N", compressiveMpa: 5.2,  cement: 1.0, lime: 1.00, sand: 6.00, flexuralBondMpa: 0.20, kind: EnMortarKind.General);
    public static readonly MortarType O = new("O", compressiveMpa: 2.4,  cement: 1.0, lime: 2.00, sand: 9.00, flexuralBondMpa: 0.10, kind: EnMortarKind.General);
    public static readonly MortarType K = new("K", compressiveMpa: 0.5,  cement: 1.0, lime: 3.00, sand: 12.0, flexuralBondMpa: 0.05, kind: EnMortarKind.General);
    public double CompressiveMpa { get; }
    public double Cement { get; }
    public double Lime { get; }
    public double Sand { get; }
    public double FlexuralBondMpa { get; }
    public EnMortarKind Kind { get; }

    public Option<EnMortarClass> EnClass => EnMortarClass.Of(CompressiveMpa);
}

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RuptureModulus {
    public static readonly RuptureModulus NormalSolid              = new("normal-solid",               pclMsMpa: 0.917, pclNMpa: 0.689, mcMsMpa: 0.552, mcNMpa: 0.352, spanParallelToBed: false, stackBond: false);
    public static readonly RuptureModulus NormalHollowUngrouted    = new("normal-hollow-ungrouted",    pclMsMpa: 0.579, pclNMpa: 0.441, mcMsMpa: 0.352, mcNMpa: 0.214, spanParallelToBed: false, stackBond: false);
    public static readonly RuptureModulus NormalHollowGrouted      = new("normal-hollow-grouted",      pclMsMpa: 1.124, pclNMpa: 1.089, mcMsMpa: 1.055, mcNMpa: 1.000, spanParallelToBed: false, stackBond: false);
    public static readonly RuptureModulus ParallelRunningSolid     = new("parallel-running-solid",     pclMsMpa: 1.841, pclNMpa: 1.379, mcMsMpa: 1.103, mcNMpa: 0.689, spanParallelToBed: true,  stackBond: false);
    public static readonly RuptureModulus ParallelRunningUngrouted = new("parallel-running-ungrouted", pclMsMpa: 1.151, pclNMpa: 0.876, mcMsMpa: 0.689, mcNMpa: 0.441, spanParallelToBed: true,  stackBond: false);
    public static readonly RuptureModulus ParallelRunningGrouted   = new("parallel-running-grouted",   pclMsMpa: 1.841, pclNMpa: 1.379, mcMsMpa: 1.103, mcNMpa: 0.689, spanParallelToBed: true,  stackBond: false);
    public static readonly RuptureModulus StackContinuousGrout     = new("stack-continuous-grout",     pclMsMpa: 2.310, pclNMpa: 2.310, mcMsMpa: 2.310, mcNMpa: 2.310, spanParallelToBed: true,  stackBond: true);
    public static readonly RuptureModulus StackOther               = new("stack-other",                pclMsMpa: 0.0,   pclNMpa: 0.0,   mcMsMpa: 0.0,   mcNMpa: 0.0,   spanParallelToBed: true,  stackBond: true);
    public double PclMsMpa { get; }
    public double PclNMpa { get; }
    public double McMsMpa { get; }
    public double McNMpa { get; }
    public bool SpanParallelToBed { get; }
    public bool StackBond { get; }

    public double FrMpa(MortarSystem system, MortarType mortar) => mortar.Switch(
        state: (Owner: this, System: system),
        m: static x => x.System.ReducedBond ? x.Owner.McMsMpa : x.Owner.PclMsMpa,
        s: static x => x.System.ReducedBond ? x.Owner.McMsMpa : x.Owner.PclMsMpa,
        n: static x => x.System.ReducedBond ? x.Owner.McNMpa : x.Owner.PclNMpa,
        o: static _ => 0.0,
        k: static _ => 0.0);

    public static double PartialGrout(double groutedCellFraction, MortarSystem system, MortarType mortar) =>
        NormalHollowUngrouted.FrMpa(system, mortar)
            + (NormalHollowGrouted.FrMpa(system, mortar) - NormalHollowUngrouted.FrMpa(system, mortar)) * Math.Clamp(groutedCellFraction, 0.0, 1.0);

    public static RuptureModulus For(SectionProfile profile, RuptureModulus declared, double groutedCellFraction) =>
        (declared.SpanParallelToBed, declared.StackBond, Solid(profile), groutedCellFraction) switch {
            (_, true, _, >= 1.0)      => StackContinuousGrout,
            (_, true, _, _)           => StackOther,
            (false, _, true, _)       => NormalSolid,
            (false, _, _, >= 1.0)     => NormalHollowGrouted,
            (false, _, _, _)          => NormalHollowUngrouted,
            (true, _, true, _)        => ParallelRunningSolid,
            (true, _, _, >= 1.0)      => ParallelRunningGrouted,
            _                         => ParallelRunningUngrouted,
        };

    public const double SolidNetFloor = 0.75;

    static bool Solid(SectionProfile profile) =>
        profile is not SectionProfile.CellularRectangle cell
        || 1.0 - cell.Cells.Sum(static c => c.WidthMm * c.HeightMm)
            / (cell.WidthMm.Value * cell.DepthMm.Value) >= SolidNetFloor;
}

public readonly record struct FlexuralBand(double GpWeakMpa, double GpStrongMpa, Option<double> ThinLayerMpa, Option<double> LightweightMpa);

public readonly record struct ShearBand(double LowMpa, double MidMpa, double HighMpa, Option<double> ThinLayerMpa, Option<double> LightweightMpa);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LateralAction {
    public static readonly LateralAction Transient = new("transient");
    public static readonly LateralAction Permanent = new("permanent");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlexuralStrengthEn {
    public static readonly FlexuralStrengthEn Clay = new("clay",
        fxk1: new FlexuralBand(0.10, 0.10, Some(0.15), Some(0.10)),
        fxk2: new FlexuralBand(0.20, 0.40, Some(0.15), Some(0.10)),
        fvk0: new ShearBand(0.10, 0.20, 0.30, Option<double>.None, Option<double>.None));
    public static readonly FlexuralStrengthEn CalciumSilicate = new("calcium-silicate",
        fxk1: new FlexuralBand(0.05, 0.10, Some(0.20), Option<double>.None),
        fxk2: new FlexuralBand(0.20, 0.40, Some(0.30), Option<double>.None),
        fvk0: new ShearBand(0.10, 0.15, 0.20, Option<double>.None, Option<double>.None));
    public static readonly FlexuralStrengthEn AggregateConcrete = new("aggregate-concrete",
        fxk1: new FlexuralBand(0.05, 0.10, Some(0.20), Option<double>.None),
        fxk2: new FlexuralBand(0.20, 0.40, Some(0.30), Option<double>.None),
        fvk0: new ShearBand(0.10, 0.15, 0.20, Option<double>.None, Option<double>.None));
    public static readonly FlexuralStrengthEn AutoclavedAeratedLight = new("autoclaved-aerated-light",
        fxk1: new FlexuralBand(0.05, 0.10, Some(0.15), Some(0.10)),
        fxk2: new FlexuralBand(0.20, 0.20, Some(0.20), Some(0.15)),
        fvk0: new ShearBand(0.10, 0.15, 0.20, Option<double>.None, Option<double>.None),
        thinLayerFbCoefficient: Some(0.035));
    public static readonly FlexuralStrengthEn AutoclavedAeratedDense = new("autoclaved-aerated-dense",
        fxk1: new FlexuralBand(0.05, 0.10, Some(0.15), Some(0.10)),
        fxk2: new FlexuralBand(0.20, 0.40, Some(0.30), Some(0.15)),
        fvk0: new ShearBand(0.10, 0.15, 0.20, Option<double>.None, Option<double>.None),
        thinLayerFbCoefficient: Some(0.035));
    public static readonly FlexuralStrengthEn ManufacturedStone = new("manufactured-stone",
        fxk1: new FlexuralBand(0.05, 0.10, Option<double>.None, Option<double>.None),
        fxk2: new FlexuralBand(0.20, 0.40, Option<double>.None, Option<double>.None),
        fvk0: new ShearBand(0.10, 0.15, 0.20, Option<double>.None, Option<double>.None));
    public static readonly FlexuralStrengthEn DimensionedNaturalStone = new("dimensioned-natural-stone",
        fxk1: new FlexuralBand(0.05, 0.10, Some(0.15), Option<double>.None),
        fxk2: new FlexuralBand(0.20, 0.40, Some(0.15), Option<double>.None),
        fvk0: new ShearBand(0.10, 0.15, 0.20, Option<double>.None, Option<double>.None));

    public FlexuralBand Fxk1 { get; }
    public FlexuralBand Fxk2 { get; }
    public ShearBand Fvk0 { get; }
    public Option<double> ThinLayerFbCoefficient { get; }

    const double SpecialMortarFloorMpa = 5.0;
    const double GeneralBandSplitMpa = 5.0;

    public double FxkMpa(MortarType mortar, RuptureModulus rupture, Option<LateralAction> action = default, Option<double> normalisedStrengthMpa = default) =>
        !rupture.SpanParallelToBed && action.Exists(static a => a == LateralAction.Permanent)
            ? 0.0
            : (from coefficient in ThinLayerFbCoefficient
               from fb in normalisedStrengthMpa
               where !rupture.SpanParallelToBed && mortar.Kind == EnMortarKind.ThinLayer
               select coefficient * fb)
              .IfNone(Column(rupture.SpanParallelToBed ? Fxk2 : Fxk1, mortar).IfNone(0.0));

    const double ShearHighBandFloorMpa = 10.0;
    const double ShearMidBandFloorMpa = 2.5;

    public double Fvk0Mpa(MortarType mortar) => mortar.Kind.Switch(
        state: (Owner: this, Class: mortar.EnClass.Bind(static c => c.FloorMpa).IfNone(0.0)),
        general: static x => x.Class >= ShearHighBandFloorMpa ? x.Owner.Fvk0.HighMpa
            : x.Class >= ShearMidBandFloorMpa ? x.Owner.Fvk0.MidMpa
            : x.Owner.Fvk0.LowMpa,
        thinLayer: static x => x.Owner.Fvk0.ThinLayerMpa.IfNone(0.0),
        lightweight: static x => x.Owner.Fvk0.LightweightMpa.IfNone(0.0));

    static Option<double> Column(FlexuralBand band, MortarType mortar) => mortar.Kind.Switch(
        state: (Band: band, Mortar: mortar),
        general: static x => Some(x.Mortar.CompressiveMpa >= GeneralBandSplitMpa ? x.Band.GpStrongMpa : x.Band.GpWeakMpa),
        thinLayer: static x => x.Mortar.CompressiveMpa >= SpecialMortarFloorMpa ? x.Band.ThinLayerMpa : Option<double>.None,
        lightweight: static x => x.Mortar.CompressiveMpa >= SpecialMortarFloorMpa ? x.Band.LightweightMpa : Option<double>.None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class NaMortarBand {
    public static readonly NaMortarBand M12  = new("M12",  floorMpa: 12.0);
    public static readonly NaMortarBand M6M4 = new("M6M4", floorMpa: 4.0);
    public static readonly NaMortarBand M2   = new("M2",   floorMpa: 2.0);
    public double FloorMpa { get; }

    public static Option<NaMortarBand> Of(MortarType mortar) =>
        mortar.Kind != EnMortarKind.General
            ? Option<NaMortarBand>.None
            : Some(mortar.CompressiveMpa >= M12.FloorMpa ? M12 : mortar.CompressiveMpa >= M6M4.FloorMpa ? M6M4 : M2);
}

public readonly record struct NaBand(double M12Mpa, double M6M4Mpa, double M2Mpa) {
    public static NaBand Merged(double m12AndM6M4Mpa, double m2Mpa) => new(m12AndM6M4Mpa, m12AndM6M4Mpa, m2Mpa);
    public double At(NaMortarBand band) => band.Switch(
        state: this,
        m12: static x => x.M12Mpa,
        m6M4: static x => x.M6M4Mpa,
        m2: static x => x.M2Mpa);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FlexuralStrengthNa {
    public static readonly FlexuralStrengthNa ClayLowAbsorption = new("clay-lt-7",
        fxk1: new NaBand(0.7, 0.5, 0.4), fxk2: new NaBand(2.0, 1.5, 1.2));
    public static readonly FlexuralStrengthNa ClayMidAbsorption = new("clay-7-to-12",
        fxk1: new NaBand(0.5, 0.4, 0.35), fxk2: new NaBand(1.5, 1.1, 1.0));
    public static readonly FlexuralStrengthNa ClayHighAbsorption = new("clay-gt-12",
        fxk1: new NaBand(0.4, 0.3, 0.25), fxk2: new NaBand(1.1, 0.9, 0.8));

    public static readonly FlexuralStrengthNa CalciumSilicateBrick = new("calcium-silicate-brick",
        fxk1: NaBand.Merged(0.3, 0.2), fxk2: NaBand.Merged(0.9, 0.6));
    public static readonly FlexuralStrengthNa AggregateConcreteBrick = new("aggregate-concrete-brick",
        fxk1: NaBand.Merged(0.3, 0.2), fxk2: NaBand.Merged(0.9, 0.6));

    public static readonly FlexuralStrengthNa BlockThin29 = new("block-100-2.9",
        fxk1: NaBand.Merged(0.25, 0.2), fxk2: NaBand.Merged(0.40, 0.4), thicknessMm: 100.0, declaredStrengthMpa: 2.9);
    public static readonly FlexuralStrengthNa BlockThin36 = new("block-100-3.6",
        fxk1: NaBand.Merged(0.25, 0.2), fxk2: NaBand.Merged(0.45, 0.4), thicknessMm: 100.0, declaredStrengthMpa: 3.6);
    public static readonly FlexuralStrengthNa BlockThin73 = new("block-100-7.3",
        fxk1: NaBand.Merged(0.25, 0.2), fxk2: NaBand.Merged(0.60, 0.5), thicknessMm: 100.0, declaredStrengthMpa: 7.3);
    public static readonly FlexuralStrengthNa BlockThick29 = new("block-250-2.9",
        fxk1: NaBand.Merged(0.15, 0.1), fxk2: NaBand.Merged(0.25, 0.2), thicknessMm: 250.0, declaredStrengthMpa: 2.9);
    public static readonly FlexuralStrengthNa BlockThick36 = new("block-250-3.6",
        fxk1: NaBand.Merged(0.15, 0.1), fxk2: NaBand.Merged(0.25, 0.2), thicknessMm: 250.0, declaredStrengthMpa: 3.6);
    public static readonly FlexuralStrengthNa BlockThick73 = new("block-250-7.3",
        fxk1: NaBand.Merged(0.15, 0.1), fxk2: NaBand.Merged(0.35, 0.3), thicknessMm: 250.0, declaredStrengthMpa: 7.3);
    public static readonly FlexuralStrengthNa BlockStrong104 = new("block-any-10.4",
        fxk1: NaBand.Merged(0.25, 0.2), fxk2: NaBand.Merged(0.75, 0.6), declaredStrengthMpa: 10.4);
    public static readonly FlexuralStrengthNa BlockStrong175 = new("block-any-17.5",
        fxk1: NaBand.Merged(0.25, 0.2), fxk2: NaBand.Merged(0.90, 0.7), declaredStrengthMpa: 17.5, orthogonalRatio: Some(0.3));

    public NaBand Fxk1 { get; }
    public NaBand Fxk2 { get; }
    public Option<double> ThicknessMm { get; }
    public Option<double> DeclaredStrengthMpa { get; }
    public Option<double> OrthogonalRatio { get; }

    const double LowAbsorptionPercent = 7.0;
    const double HighAbsorptionPercent = 12.0;
    const double InterpolationFloorMm = 100.0;
    const double InterpolationCeilingMm = 250.0;

    public static FlexuralStrengthNa ForClay(double waterAbsorptionPercent) =>
        waterAbsorptionPercent < LowAbsorptionPercent ? ClayLowAbsorption
        : waterAbsorptionPercent <= HighAbsorptionPercent ? ClayMidAbsorption
        : ClayHighAbsorption;

    public static Fin<double> Block(double thicknessMm, double declaredStrengthMpa, NaMortarBand band, RuptureModulus rupture, Op key) =>
        !double.IsFinite(thicknessMm) || !double.IsFinite(declaredStrengthMpa) || thicknessMm <= 0.0 || declaredStrengthMpa <= 0.0
            ? new KernelFault.InvalidValue(nameof(Block), "finite positive thickness and declared strength", Some(key))
        : declaredStrengthMpa >= BlockStrong175.DeclaredStrengthMpa.IfNone(17.5) ? Fin.Succ(Plane(BlockStrong175, band, rupture))
        : declaredStrengthMpa >= BlockStrong104.DeclaredStrengthMpa.IfNone(10.4) ? Fin.Succ(Plane(BlockStrong104, band, rupture))
        : Fin.Succ(Lerp(
            Strength(BlockThin29, BlockThin36, BlockThin73, declaredStrengthMpa, band, rupture),
            Strength(BlockThick29, BlockThick36, BlockThick73, declaredStrengthMpa, band, rupture),
            Fraction(Math.Clamp(thicknessMm, InterpolationFloorMm, InterpolationCeilingMm), InterpolationFloorMm, InterpolationCeilingMm)));

    static double Plane(FlexuralStrengthNa row, NaMortarBand band, RuptureModulus rupture) =>
        (rupture.SpanParallelToBed ? row.Fxk2 : row.Fxk1).At(band);

    static double Strength(FlexuralStrengthNa low, FlexuralStrengthNa mid, FlexuralStrengthNa high, double declaredStrengthMpa, NaMortarBand band, RuptureModulus rupture) =>
        declaredStrengthMpa <= low.DeclaredStrengthMpa.IfNone(2.9) ? Plane(low, band, rupture)
        : declaredStrengthMpa <= mid.DeclaredStrengthMpa.IfNone(3.6)
            ? Lerp(Plane(low, band, rupture), Plane(mid, band, rupture),
                Fraction(declaredStrengthMpa, low.DeclaredStrengthMpa.IfNone(2.9), mid.DeclaredStrengthMpa.IfNone(3.6)))
        : declaredStrengthMpa <= high.DeclaredStrengthMpa.IfNone(7.3)
            ? Lerp(Plane(mid, band, rupture), Plane(high, band, rupture),
                Fraction(declaredStrengthMpa, mid.DeclaredStrengthMpa.IfNone(3.6), high.DeclaredStrengthMpa.IfNone(7.3)))
        : Plane(high, band, rupture);

    static double Fraction(double value, double lo, double hi) => hi > lo ? (value - lo) / (hi - lo) : 0.0;
    static double Lerp(double lo, double hi, double t) => lo + (hi - lo) * Math.Clamp(t, 0.0, 1.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SizeTolerance {
    public static readonly SizeTolerance T1 = new("T1", floorMm: 3.0, sqrtCoefficient: 0.40);
    public static readonly SizeTolerance T2 = new("T2", floorMm: 2.0, sqrtCoefficient: 0.25);
    public static readonly SizeTolerance Tm = new("Tm", floorMm: 0.0, sqrtCoefficient: 0.00);
    public double FloorMm { get; }
    public double SqrtCoefficient { get; }

    public Fin<Tolerance> MeanDeviation(double workMm, double declaredMm, Op key) =>
        Tolerance.Of(ToleranceLane.Grade,
            Math.Max(Math.Max(FloorMm, declaredMm), SqrtCoefficient * Math.Sqrt(Math.Max(0.0, workMm))), key);

    public Fin<(double LowMm, double HighMm)> WorkEnvelope(double workMm, double declaredMm, Op key) =>
        MeanDeviation(workMm, declaredMm, key).Map(band => (workMm - band.Value, workMm + band.Value));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SizeRange {
    public static readonly SizeRange R1 = new("R1", sqrtCoefficient: 0.60);
    public static readonly SizeRange R2 = new("R2", sqrtCoefficient: 0.30);
    public static readonly SizeRange Rm = new("Rm", sqrtCoefficient: 0.00);
    public double SqrtCoefficient { get; }

    public Fin<Tolerance> PermittedRange(double workMm, Op key) =>
        Tolerance.Of(ToleranceLane.Grade, SqrtCoefficient * Math.Sqrt(Math.Max(0.0, workMm)), key);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MasonryBody {
    public static readonly MasonryBody FiredClay = new("masonry.clay",
        densityKgM3: 1800.0, conductivityWmK: 0.77, specificHeatJKgK: 1000.0, vapourMu: 16.0,
        eqThick1HrMm: Some(68.6), enGroup: FlexuralStrengthEn.Clay);
    public static readonly MasonryBody CalciumSilicate = new("masonry.calciumsilicate",
        densityKgM3: 1800.0, conductivityWmK: 1.00, specificHeatJKgK: 1000.0, vapourMu: 15.0,
        eqThick1HrMm: Option<double>.None, enGroup: FlexuralStrengthEn.CalciumSilicate);
    public double DensityKgM3 { get; }
    public double ConductivityWmK { get; }
    public double SpecificHeatJKgK { get; }
    public double VapourMu { get; }
    public Option<double> EqThick1HrMm { get; }
    public FlexuralStrengthEn EnGroup { get; }
    public MaterialId Material => MaterialId.Create(Key);
}

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

    const double QuarterPi = Math.PI / 4.0;
    const double HalfPi = Math.PI / 2.0;
    const double ExtentMarginCells = 1.0;

    public WallpaperGroup Group { get; }
    public Arr<(double U, double V, double Spin, Orientation Face)> Seats { get; }
    public double AspectLo { get; }
    public double AspectHi { get; }

    public bool Admits(double lengthOverHeight) => lengthOverHeight >= AspectLo && lengthOverHeight <= AspectHi;

    public Fin<Seq<CourseTemplate>> Courses(ComponentUnit unit, double jointMm, int courses, Op key) =>
        from plan in Fin.Succ(Plan(unit, jointMm, courses))
        from stream in Patterning.Apply(new PatternOp.Orbit(plan), key)
        from bands in stream.Switch(
            planar: p => Fin.Succ(Bands(p, plan, courses)),
            mapped: _ => Fin.Fail<Seq<CourseTemplate>>(new ComponentFault.BondRejected(key, None)))
        select bands;

    public Fin<CourseTemplate> Course(ComponentUnit unit, double jointMm, int index, Op key) =>
        Courses(unit, jointMm, Math.Abs(index) + 1, key).Bind(stack => stack.IsEmpty
            ? Fin.Fail<CourseTemplate>(new ComponentFault.BondRejected(key, Some(index)))
            : Fin.Succ(stack[((index % stack.Count) + stack.Count) % stack.Count]));

    PatternPlan Plan(ComponentUnit unit, double jointMm, int courses) =>
        new(Group,
            ((unit.LengthMm.Value + jointMm) * 1e-3, 0.0),
            (0.0, (unit.HeightMm.Value + jointMm) * 1e-3),
            Seats.Map(static s => (s.U, s.V, s.Spin)),
            Math.Sqrt(Math.Pow((unit.LengthMm.Value + jointMm) * 1e-3, 2.0) + Math.Pow((unit.HeightMm.Value + jointMm) * 1e-3 * courses, 2.0))
                + (ExtentMarginCells * Math.Max(unit.LengthMm.Value, unit.HeightMm.Value) * 1e-3),
            (0.0, 0.0), TangentLogMapAlgorithm.VectorHeatApproximate);

    Seq<CourseTemplate> Bands(InstanceStream.Planar planar, PatternPlan plan, int courses) =>
        toSeq(Enumerable.Range(0, courses))
            .Map(band => Placed(planar, plan, band))
            .Map(static units => new CourseTemplate(units, units.Head.Map(static u => u.AlongFraction).IfNone(0.0)));

    Seq<UnitPlacement> Placed(InstanceStream.Planar planar, PatternPlan plan, int band) =>
        toSeq(Enumerable.Range(0, planar.Site.Count)
                .Where(i => planar.Site[i].V >= band * plan.BasisB.V && planar.Site[i].V < (band + 1) * plan.BasisB.V)
                .OrderBy(i => planar.Site[i].U))
            .Map(i => new UnitPlacement(
                Seats[planar.Anchor[i]].Face,
                Residual(planar.Site[i].U, plan.BasisA.U),
                (planar.Site[i].V - (band * plan.BasisB.V)) / plan.BasisB.V,
                planar.Spin[i] * (180.0 / Math.PI),
                planar.Mirrored[i]));

    static double Residual(double along, double advance) => (along / advance) - Math.Floor(along / advance);

    static Arr<(double U, double V, double Spin, Orientation Face)> Motif(params ReadOnlySpan<(double U, double V, double Spin, Orientation Face)> seats) => new([.. seats]);

    static (double U, double V, double Spin, Orientation Face) Seat(double u, double v, double spin, Orientation face) => (u, v, spin, face);
}

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

    public Seq<CourseTemplate> Courses { get; }
    public Option<BondGeometry> Geometry { get; }

    const int OverlapSampleCourses = 4;

    private BondName(string key, Seq<CourseTemplate> template) : this(key) => (Courses, Geometry) = (template, None);
    private BondName(string key, BondGeometry generated) : this(key) => (Courses, Geometry) = (Seq<CourseTemplate>(), Some(generated));

    public bool Fits(ComponentUnit unit) =>
        Geometry.Match(Some: g => g.Admits(unit.LengthOverHeight), None: () => true);

    public Fin<CourseTemplate> Course(ComponentUnit unit, double jointMm, int index, Op key) =>
        Stack(unit, jointMm, Math.Abs(index) + 1, key).Bind(stack => stack.IsEmpty
            ? Fin.Fail<CourseTemplate>(new ComponentFault.BondRejected(key, Some(index)))
            : Fin.Succ(stack[((index % stack.Count) + stack.Count) % stack.Count]));

    public Fin<Seq<CourseTemplate>> Stack(ComponentUnit unit, double jointMm, int courses, Op key) =>
        Geometry.Match(
            Some: geometry => geometry.Courses(unit, jointMm, courses, key),
            None: () => Courses.IsEmpty
                ? Fin.Fail<Seq<CourseTemplate>>(new ComponentFault.BondRejected(key, None))
                : Fin.Succ(Courses));

    public Fin<BondOverlap> Overlap(ComponentUnit unit, double jointMm, Op key) =>
        Stack(unit, jointMm, OverlapSampleCourses, key).Map(stack => new BondOverlap(
            OverlapMm: Stagger(stack) * (unit.LengthMm.Value + jointMm) is var lapped && lapped > unit.LengthMm.Value
                ? unit.LengthMm.Value
                : Stagger(stack) * (unit.LengthMm.Value + jointMm),
            RequiredMm: BondOverlap.Required(unit.HeightMm.Value)));

    static double Stagger(Seq<CourseTemplate> stack) =>
        stack.Count < 2
            ? 0.0
            : toSeq(Enumerable.Range(0, stack.Count))
                .Map(i => Folded(Math.Abs(stack[(i + 1) % stack.Count].CourseOffsetFraction - stack[i].CourseOffsetFraction)))
                .Fold(1.0, static (least, lap) => Math.Min(least, lap));

    static double Folded(double shift) => shift - Math.Floor(shift) is var f && f > 0.5 ? 1.0 - f : f;

    static CourseTemplate StretcherCourse(double courseOffset) => new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0, false)), courseOffset);
    static CourseTemplate HeaderCourse(double courseOffset) => new(Seq(new UnitPlacement(Orientation.Header, 0.0, 0.0, 0.0, false)), courseOffset);
    static CourseTemplate MonkCourse(double courseOffset) => new(Seq(new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0, false), new UnitPlacement(Orientation.Stretcher, 0.0, 0.0, 0.0, false), new UnitPlacement(Orientation.Header, 0.0, 0.0, 0.0, false)), courseOffset);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class UnitClass {
    public static readonly UnitClass EnM4   = new("en-m4",   Some(4.0));
    public static readonly UnitClass EnM10  = new("en-m10",  Some(10.0));
    public static readonly UnitClass EnM15  = new("en-m15",  Some(15.0));
    public static readonly UnitClass EnM20  = new("en-m20",  Some(20.0));
    public static readonly UnitClass EnM30  = new("en-m30",  Some(30.0));
    public static readonly UnitClass AstmSw = new("astm-sw", Option<double>.None);
    public static readonly UnitClass AstmMw = new("astm-mw", Option<double>.None);
    public static readonly UnitClass AstmNw = new("astm-nw", Option<double>.None);
    public Option<double> NormalisedStrengthMpa { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct UnitPlacement(Orientation Orientation, double AlongFraction, double LateralFraction, double RotationDegrees, bool Mirrored);

public sealed record CourseTemplate(Seq<UnitPlacement> Units, double CourseOffsetFraction);

public readonly record struct BondOverlap(double OverlapMm, double RequiredMm) {
    const double ShortUnitHeightMm = 250.0;
    const double ShortUnitFraction = 0.4;
    const double ShortUnitFloorMm = 40.0;
    const double TallUnitFraction = 0.2;
    const double TallUnitFloorMm = 100.0;

    public bool Satisfies => OverlapMm >= RequiredMm;
    public double ShortfallMm => Math.Max(0.0, RequiredMm - OverlapMm);

    public static double Required(double unitHeightMm) =>
        unitHeightMm <= ShortUnitHeightMm
            ? Math.Max(ShortUnitFraction * unitHeightMm, ShortUnitFloorMm)
            : Math.Max(TallUnitFraction * unitHeightMm, TallUnitFloorMm);
}

public readonly record struct FrogGeometry(double DepthMm, double LengthFraction, double WidthFraction, double TaperDegrees, bool Double) {
    public static readonly FrogGeometry None = new(0.0, 0.0, 0.0, 0.0, false);
    public bool Present => DepthMm > 0.0 && LengthFraction > 0.0 && WidthFraction > 0.0;
    public bool Absent => DepthMm <= 0.0 && LengthFraction <= 0.0 && WidthFraction <= 0.0;
    public bool Declared => Present || Absent;

    public double VoidFraction(double heightMm) =>
        Present && heightMm > 0.0 ? DepthMm / heightMm * LengthFraction * WidthFraction * (Double ? 2.0 : 1.0) : 0.0;

    public double NetBedAreaMm2(double lengthMm, double widthMm) =>
        Present ? lengthMm * widthMm * (1.0 - LengthFraction * WidthFraction) : lengthMm * widthMm;
}

public readonly record struct Perforation(int Columns, int Rows, double HoleDiameterMm, double EdgeMarginMm) {
    public static readonly Perforation None = new(0, 0, 0.0, 0.0);
    public int HoleCount => Math.Max(0, Columns) * Math.Max(0, Rows);
    public bool Present => HoleCount > 0 && HoleDiameterMm > 0.0;
    public double VoidFraction(double lengthMm, double widthMm) =>
        Present && lengthMm > 0.0 && widthMm > 0.0 ? HoleCount * Math.PI * HoleDiameterMm * HoleDiameterMm / 4.0 / (lengthMm * widthMm) : 0.0;
}

[ComplexValueObject]
public readonly partial struct MortarJoint {
    public double HeadWidthMm { get; }
    public double BedWidthMm { get; }
    public MortarProfile Profile { get; }
    public MortarType Mortar { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double headWidthMm, ref double bedWidthMm, ref MortarProfile profile, ref MortarType mortar) =>
        validationError = double.IsFinite(headWidthMm) && headWidthMm > 0.0 && double.IsFinite(bedWidthMm) && bedWidthMm > 0.0
            ? null
            : new ValidationError($"Mortar-joint widths must be finite and positive; received {headWidthMm:R} and {bedWidthMm:R}.");

    public static Fin<MortarJoint> Of(double headMm, double bedMm, MortarProfile profile, MortarType mortar, Op key) =>
        key.AcceptValidated<MortarJoint>(Validate(headMm, bedMm, profile, mortar, out MortarJoint joint), joint);

    public static Fin<MortarJoint> Standard(double thicknessMm, Op key) => Of(thicknessMm, thicknessMm, MortarProfile.Concave, MortarType.N, key);

    public double RecessDepthMm => BedWidthMm * Profile.DepthFactor;
}

public readonly record struct MasonryRow(
    string Designation, double WMm, double HMm, double LMm, double CourseMm, double JointMm,
    ComponentAuthority Authority,
    FrogGeometry Frog, Perforation Perforation, SpecialShape Shape,
    SizeTolerance Tolerance, SizeRange Range, MasonryBody Body,
    Option<UnitClass> Class = default, Option<double> WaterAbsorptionPercent = default) {
    public EvidenceGrade Source { get; init; } = EvidenceGrade.User;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
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

    const double HollowVoidFloor = 0.40;
    const double PerforatedVoidFloor = 0.25;
    const int MultiCellHoleCount = 3;

    public static Fin<Coring> Bucket(FrogGeometry frog, Perforation perforation, double wMm, double hMm, double lMm, Op key) =>
        frog.VoidFraction(hMm) + perforation.VoidFraction(lMm, wMm) is var voids && double.IsFinite(voids) && voids is >= 0.0 and < 1.0
            ? Fin.Succ(Class(frog, perforation, wMm, hMm, lMm))
            : new ComponentFault.CoringRejected(key, voids);

    public static Coring Class(FrogGeometry frog, Perforation perforation, double wMm, double hMm, double lMm) =>
        (perforation.Present, perforation.HoleCount, frog.VoidFraction(hMm) + perforation.VoidFraction(lMm, wMm), frog.Present, frog.Double) switch {
            (true, > MultiCellHoleCount, >= HollowVoidFloor, _, _)      => Coring.Hollow3Cell,
            (true, _, >= HollowVoidFloor, _, _)                         => Coring.Hollow2Cell,
            (true, > MultiCellHoleCount, >= PerforatedVoidFloor, _, _)  => Coring.Perforated10Cell,
            (true, _, _, _, _)                                          => Coring.Cored3Hole,
            (_, _, _, true, true)                                       => Coring.Cellular,
            (_, _, _, true, _)                                          => Coring.Frog,
            _                                                           => Coring.None,
        };
}

public readonly record struct MasonryPhysics(
    double EquivalentThicknessMm,
    Option<RatingPeriod> FireRating,
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
            FireRating: body.EqThick1HrMm.Bind(cn => RatingPeriod.Floor(Math.Pow(te / cn, 1.7))),
            SelfWeightKnPerM2: net * body.DensityKgM3 * GravityMPerS2 / (len * 1e6),
            ThermalResistanceM2KPerW: Resistance(cells, body, w, len),
            SolidFraction: gross > 0.0 ? Math.Clamp(net / gross, 0.0, 1.0) : 1.0);
    }

    static double Resistance(Seq<VoidCell> cells, MasonryBody body, double wMm, double lenMm) {
        double k = body.ConductivityWmK, widthM = wMm / 1000.0;
        if (cells.IsEmpty) { return widthM / k; }
        double coreWidthM = cells.Max(static c => c.WidthMm) / 1000.0;
        double webConductance = (lenMm - cells.Sum(static c => c.HeightMm)) / lenMm * (k / coreWidthM);
        double cellConductance = cells.Sum(c => c.HeightMm / lenMm
            / (CmuPhysics.CellAirResistanceM2KPerW + (coreWidthM - c.WidthMm / 1000.0) / k));
        return (widthM - coreWidthM) / k + 1.0 / (webConductance + cellConductance);
    }
}

[SmartEnum<int>]
public sealed partial class RatingPeriod {
    public static readonly RatingPeriod OneHour       = new(60);
    public static readonly RatingPeriod NinetyMinute  = new(90);
    public static readonly RatingPeriod TwoHour       = new(120);
    public static readonly RatingPeriod ThreeHour     = new(180);
    public static readonly RatingPeriod FourHour      = new(240);
    public double Hours => Key / 60.0;

    static readonly Seq<RatingPeriod> Ladder = toSeq(Items.OrderBy(static row => row.Key));

    public static Option<RatingPeriod> Floor(double computedHours) =>
        double.IsFinite(computedHours)
            ? Ladder.Filter(period => computedHours >= period.Hours).Last
            : Option<RatingPeriod>.None;
}

public static class WallAcoustics {
    const double MassLawOffsetDb = 47.0;
    const double HardMasonryAbsorption = 0.02;

    public static Fin<Acoustic> Of(double arealMassKgPerM2, Op key) =>
        Acoustic.Of(
            AcousticBand.Items.Select(static _ => HardMasonryAbsorption).ToArray(),
            AcousticBand.Items
                .OrderBy(static band => band.Key)
                .Select(band => Math.Max(0.0, 20.0 * Math.Log10(Math.Max(arealMassKgPerM2, 1e-9) * band.CenterHz) - MassLawOffsetDb))
                .ToArray(),
            key);
}

public static class MasonryDetail {
    public static Fin<PropertyBag> Of(MasonryRow row, PositiveMagnitude heightMm, PositiveMagnitude courseHeightMm) =>
        from unitHeight in ComponentDetail.Measured(DetailSchema.UnitHeight, Dimension.LengthDim, heightMm.Value * 1e-3)
        from courseHeight in ComponentDetail.Measured(DetailSchema.CourseHeight, Dimension.LengthDim, courseHeightMm.Value * 1e-3)
        select ComponentDetail.RealizationRows(
            ComponentDetail.Token(DetailSchema.SizeTolerance, row.Tolerance.Key),
            ComponentDetail.Token(DetailSchema.SizeRange, row.Range.Key),
            ComponentDetail.Token(DetailSchema.SpecialShape, row.Shape.Key),
            unitHeight,
            courseHeight,
            ComponentDetail.Sourced(row.Source));

    public static Fin<Seq<MaterialPropertySet>> Properties(SectionProfile profile, MasonryBody body, Op key) =>
        from physics in Fin.Succ(MasonryPhysics.Of(profile, body))
        from thermal in MaterialPropertySet.OfThermal(
            conductivity: body.ConductivityWmK,
            specificHeat: body.SpecificHeatJKgK,
            uValue: 1.0 / physics.ThermalResistanceM2KPerW,
            vapourResistanceFactor: body.VapourMu, key)
        from spectrum in WallAcoustics.Of(physics.ArealMassKgPerM2, key)
        from fire in physics.FireRating
            .TraverseM(period => FireResistance.Of(FireCoverage.I, period.Key, key).Map(static r => Seq(MaterialPropertySet.OfFire(FireRating.A1, r)))).As()
            .Map(static rows => rows.IfNone(Seq<MaterialPropertySet>()))
        select Seq(thermal, MaterialPropertySet.OfAcoustic(spectrum)) + fire;
}

// --- [TABLES] --------------------------------------------------------------------------
public static class MasonrySeed {
    public static readonly Seq<MasonryRow> Roster = Seq(
        new MasonryRow("masonry.us-modular",      92.0,  57.0, 194.0,  67.0,  9.5, ComponentAuthority.Astm, FrogGeometry.None,                              new Perforation(3, 1, 38.0, 25.0), SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.us-norman",       92.0,  57.0, 295.0,  67.0,  9.5, ComponentAuthority.Astm, FrogGeometry.None,                              new Perforation(3, 1, 38.0, 25.0), SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.uk-standard",    102.5,  65.0, 215.0,  75.0, 10.0, ComponentAuthority.Bs,   FrogGeometry.None,                              new Perforation(5, 2, 29.0, 15.0), SpecialShape.None,     SizeTolerance.T2, SizeRange.R1, MasonryBody.CalciumSilicate),
        new MasonryRow("masonry.uk-bullnose",    102.5,  65.0, 215.0,  75.0, 10.0, ComponentAuthority.Bs,   FrogGeometry.None,                              Perforation.None,                  SpecialShape.Bullnose, SizeTolerance.T2, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.din-nf",         115.0,  71.0, 240.0,  83.5, 12.5, ComponentAuthority.Din,  new FrogGeometry(12.0, 0.55, 0.40, 8.0, false), Perforation.None,                  SpecialShape.None,     SizeTolerance.T2, SizeRange.R2, MasonryBody.FiredClay),
        new MasonryRow("masonry.au-standard",    110.0,  76.0, 230.0,  86.0, 10.0, ComponentAuthority.As,   FrogGeometry.None,                              new Perforation(3, 1, 40.0, 25.0), SpecialShape.None,     SizeTolerance.T2, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.is-modular",      90.0,  90.0, 190.0, 100.0, 10.0, ComponentAuthority.Is,   new FrogGeometry(10.0, 0.50, 0.40, 6.0, false), Perforation.None,                  SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay),
        new MasonryRow("masonry.is-conventional", 110.0, 70.0, 230.0,  80.0, 10.0, ComponentAuthority.Is,   FrogGeometry.None,                              Perforation.None,                  SpecialShape.None,     SizeTolerance.T1, SizeRange.R1, MasonryBody.FiredClay));

    static readonly Lazy<Fin<FrozenDictionary<ComponentId, MasonryRow>>> Table =
        SeedJoin.Of(Roster, static r => r.Designation);

    public static Fin<MasonryRow> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Table, component.Designation, key);

    const double CoursingClosureTolMm = 1.0;

    public static readonly SeedLaw<MasonryRow> Law = SeedLaw<MasonryRow>.Of(
        family: ComponentFamily.Masonry,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: static (r, key) => r.Frog.Present || r.Perforation.Present
            ? SectionProfile.CellularRectangle.Of(r.WMm, r.LMm, MasonryVoids.Cells(r.Frog, r.Perforation, r.WMm, r.HMm, r.LMm), key)
            : SectionProfile.Rectangle.Of(r.WMm, r.LMm, key),
        substance: static r => r.Body.Material,
        source: static r => r.Source,
        standard: static r => new ComponentStandard(r.Authority.Region, r.JointMm, r.Authority),
        detail: Some<Func<MasonryRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        voids: static r => MasonryVoids.Class(r.Frog, r.Perforation, r.WMm, r.HMm, r.LMm));

    static Validation<Error, Unit> Coherence(MasonryRow r, Op key) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(Math.Abs(r.CourseMm - (r.HMm + r.JointMm)) <= CoursingClosureTolMm,
                new KernelFault.InvalidValue(nameof(r.CourseMm), "unit height plus joint thickness", Some(key))),
            AdmissionSlots.Gate(r.Frog.Declared,
                new KernelFault.InvalidValue(nameof(r.Frog), "all frog axes declared together or all absent", Some(key))),
            ComponentUnit.Of(r.WMm, r.HMm, r.LMm, r.CourseMm, key).ToValidation().Map(static _ => unit),
            MasonryVoids.Bucket(r.Frog, r.Perforation, r.WMm, r.HMm, r.LMm, key).ToValidation().Map(static _ => unit),
            r.Tolerance.MeanDeviation(r.LMm, declaredMm: 0.0, key).ToValidation().Map(static _ => unit),
            r.Range.PermittedRange(r.LMm, key).ToValidation().Map(static _ => unit)));

    static Fin<PropertyBag> Detail(MasonryRow r, SectionProfile profile, Op key) =>
        from unit in ComponentUnit.Of(r.WMm, r.HMm, r.LMm, r.CourseMm, key)
        from bag in MasonryDetail.Of(r, unit.HeightMm, unit.CourseHeightMm)
        select bag;

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in SeedJoin.Resolve(Table, component.Designation, key)
        from solved in section.ToFin(new ComponentFault.SectionUnavailable(key, component.Designation))
        from pocketed in guard(
            row.Frog.NetBedAreaMm2(row.LMm, row.WMm) is var netBed && netBed > 0.0 && netBed <= row.LMm * row.WMm,
            new ComponentFault.SectionIncoherent(key, component.Profile.GetType()))
        from bonded in guard(placement.Basis != DesignBasis.En1996 || !placement.Rupture.StackBond,
            new ComponentFault.BondRejected(key, None))
        from annexed in placement.Basis == DesignBasis.En1996 && placement.Annex == NationalAnnex.UnitedKingdom
            ? from absorption in row.WaterAbsorptionPercent.ToFin(new ComponentFault.WaterAbsorptionMissing(key, component.Designation))
              from band in NaMortarBand.Of(placement.Mortar).ToFin(new ComponentFault.MortarBandMissing(key, placement.Mortar))
              from published in guard(
                  FlexuralStrengthNa.ForClay(absorption) is var annex
                  && (placement.Rupture.SpanParallelToBed ? annex.Fxk2 : annex.Fxk1).At(band) > 0.0,
                  new ComponentFault.FlexuralCellMissing(key, component.Designation))
              select row
            : Fin.Succ(row)
        from strength in row.Class
            .Bind(static declared => declared.NormalisedStrengthMpa)
            .Bind(fb => MaterialGrade.CmuOf(fb, placement.Mortar))
            .Bind(static grade => grade.CmuArm)
            .ToFin(new ComponentFault.AssemblageStrengthMissing(key, component.Designation))
        from capacity in SectionCapacity.Lift(
            new CapacityLift.Masonry(
                component.Designation, strength, solved, placement.HeightMm, placement.Basis,
                RuptureModulus.For(component.Profile, placement.Rupture, groutedCellFraction: 0.0),
                row.Body.EnGroup, placement.System, placement.Mortar),
            key)
        select capacity;
}
```

## [03]-[RESEARCH]

(none)
