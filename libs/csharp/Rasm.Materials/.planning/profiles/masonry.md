# [MATERIALS_MASONRY]

THE FIRST REALIZED PROFILEFAMILY and THE GENERATIVE BOND ALGEBRA. The masonry family vocabulary — the void-class / bond / orientation / cut / closure / special-shape algebra and the regional `ProfileCatalogue` rows — is the realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Masonry` case. A masonry unit is a `Profile` row, never a `Brick` type: the coring void class, the bond course template, the unit orientation, and the regional standard are masonry-`Profile` columns, and the bond/orientation algebra is the data the `Construction/layout#ASSEMBLY_FOLD` course fold reads. The masonry vocabulary grows by data — a new bond is one `BondName` row, a new orientation one `Orientation` case, a new special shape one `SpecialShape` entry — never a per-bond layout method. The `Bond` axis is a GENERATIVE ALGEBRA (template-OR-computed): a `BondKind.Template` bond reads its course set by wrapped index, and a `BondKind.Generated` bond DERIVES its course placement from a parametric `BondGeometry` descriptor through the `GeneratedBond.Interpret` fold — so flemish/herringbone/basketweave/pinwheel/diaper bonds COMPUTE their course (and per-unit rotation into `Construction/assembly#ELEMENT_MODEL` `Placement.PathAngleDegrees`) rather than railing `ProfileFault.Bond`, and a new decorative bond is a `BondGeometry` row not a transcribed course set. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape and the `Rasm` kernel `Dimension` value-object; a cmu/steel/timber/glazing family lands its own sibling vocabulary on its own page.

## [01]-[INDEX]

- [01]-[PROFILE_FAMILY]: the masonry unit/coring/bond/orientation/cut/closure/special-shape vocabulary, the `BondName` template/generated catalogue, the `BondGeometry` parametric descriptor and the `GeneratedBond.Interpret` course-deriving fold, the `MortarProfile`/`MortarType`/`MortarJoint` ASTM C270 joint specification, and the `ProfileCatalogue.BuildMasonryRows` regional row table.

## [02]-[PROFILE_FAMILY]

- Owner: the masonry unit vocabulary (`Coring`, `CoringClass`, `BondName`, `BondKind`, `BondGeometry`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`, `MortarProfile`, `MortarType`); `CourseTemplate` the bond course shape, `MortarJoint` the head/bed-width + profile + mortar-strength joint specification; `GeneratedBond` the parametric course-deriving fold; `ProfileCatalogue.BuildMasonryRows` the registered-row seed `profile#PROFILE_OWNER` composes.
- Cases: coring {solid/cored/perforated/hollow void classes} · bond {template + generated kinds, the generated kind a `BondGeometry` descriptor the `GeneratedBond.Interpret` fold computes a course from} · orientation {stretcher/header/soldier/sailor/rowlock/shiner} · cut {whole/three-quarter/half-bat/quarter-bat/queen-closer/king-closer/bevel} · closure {none/queen-closer/king-closer/half-bat} · special-shape {none/bullnose/cownose/plinth/coping/cant/squint/voussoir} · mortar-profile {concave/v/weathered/struck/raked/flush/beaded ASTM C270 tooled joints} · mortar-type {M/S/N/O/K ASTM C270 compressive classes}.
- Entry: `public Fin<CourseTemplate> Course(int index, Op key)` on `BondName` and `public bool Fits(Profile profile)` the `[BoundaryAdapter]` bond-fit check — a `BondKind.Template` bond reads its course template by wrapped index, a `BondKind.Generated` bond resolves its course through `GeneratedBond.Interpret(geometry, index)` over the row's `BondGeometry` descriptor (the unit-cell repeat, the per-course orientation pattern, the diagonal/herringbone rotation, the flemish alternating sequence), so every generated bond computes its course rather than railing `ProfileFault.Bond`.
- Packages: Rasm (project — `PositiveMagnitude` for the `ProfileUnit` length columns), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the masonry vocabulary grows by data: a new template bond is one `BondName` row carrying its `CourseTemplate` set, a new generated bond is one `BondName` row carrying its `BondGeometry` descriptor (never a per-bond `Interpret` method — the one fold reads the descriptor), a new orientation one `Orientation` case, a new special shape one `SpecialShape.Catalog` entry — never a per-bond layout method. A sibling `ProfileFamily` (cmu/steel/timber/glazing) lands its own vocabulary on its own page the way masonry carries `Coring`/`BondName`/`Orientation`; the named cost is stated at `profile#STRUCTURAL_FAMILY_VOCABULARY` and queued in `TASKLOG.md`.
- Boundary: the masonry vocabulary is the realized first `ProfileFamily` — a per-material masonry class is the deleted form; `BondName.Course` is the OOP capsule reading the template course set or computing the generated course, `BondName.Fits` the `[BoundaryAdapter]` aspect-ratio gate, both at the edge; the course-placement projection (the station/elevation fold) is `Construction/layout#ASSEMBLY_FOLD`, composed not re-shown here; a `BondKind.Generated` bond is structurally distinct — it carries no template course set, so `GeneratedBond.Interpret` DERIVES the course from the `BondGeometry` descriptor (the unit-cell repeat width, the per-course orientation sequence, the diagonal rotation a herringbone/diaper emits into `Construction/assembly#ELEMENT_MODEL` `Placement.PathAngleDegrees`, the flemish alternating stretcher/header pattern) rather than railing `ProfileFault.Bond`, the interpreter ONE fold over a bounded bond-geometry vocabulary never a parallel generated-layout owner; the per-unit rotation a herringbone/diaper bond emits rides the existing `CourseTemplate`/`Placement` columns the `layout#ASSEMBLY_FOLD` `StepCourse` already consumes, never a widening of the `Element` model, keeping the host-neutral scalar-`Placement` discipline; `ProfileCatalogue.BuildMasonryRows(context)` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the regional masonry `Profile` rows (us/uk/din/au/is), the `ProfileUnit` width/height/length/coursing columns admitting through `ProfileUnit.Of` into the kernel `PositiveMagnitude` value-object so the masonry unit never re-mints a length primitive and a malformed row drops through `Choose`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class CoringClass {
    public static readonly CoringClass Solid = new("solid");
    public static readonly CoringClass Cored = new("cored");
    public static readonly CoringClass Perforated = new("perforated");
    public static readonly CoringClass Hollow = new("hollow");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class Coring {
    public static readonly Coring None = new("none", voidFraction: 0.00, classification: CoringClass.Solid);
    public static readonly Coring Cored3Hole = new("cored-3-hole", voidFraction: 0.20, classification: CoringClass.Cored);
    public static readonly Coring Perforated10Cell = new("perforated-10-cell", voidFraction: 0.42, classification: CoringClass.Perforated);
    public static readonly Coring Hollow2Cell = new("hollow-2-cell", voidFraction: 0.50, classification: CoringClass.Hollow);
    public double VoidFraction { get; }
    public CoringClass Classification { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class BondKind {
    public static readonly BondKind Template = new("template");
    public static readonly BondKind Generated = new("generated");
}

// The unit-cell pattern a generated bond derives its course from: the repeat width in units, the
// per-course orientation cycle, the diagonal rotation, and the flemish alternating stretcher/header pattern.
// A new decorative bond is one BondGeometry row, never a transcribed course set or a per-bond method.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class BondPattern {
    public static readonly BondPattern AllStretcher  = new("all-stretcher");   // stack/running base cycle
    public static readonly BondPattern Alternating   = new("alternating");     // flemish stretcher-header per-unit alternation
    public static readonly BondPattern Diagonal      = new("diagonal");        // herringbone 45° two-unit cell
    public static readonly BondPattern Woven         = new("woven");           // basketweave N×N alternating block
    public static readonly BondPattern Rotary        = new("rotary");          // pinwheel four-unit rotation about a centre
    public static readonly BondPattern Lattice       = new("lattice");         // diaper diamond diagonal repeat
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class BondGeometry {
    public static readonly BondGeometry Stack       = new("stack",       pattern: BondPattern.AllStretcher, repeatUnits: 1, rotationDegrees: 0.0,  blockUnits: 1);
    public static readonly BondGeometry Flemish     = new("flemish",     pattern: BondPattern.Alternating,  repeatUnits: 2, rotationDegrees: 0.0,  blockUnits: 1);
    public static readonly BondGeometry Herringbone = new("herringbone", pattern: BondPattern.Diagonal,     repeatUnits: 2, rotationDegrees: 45.0, blockUnits: 1);
    public static readonly BondGeometry Basketweave = new("basketweave", pattern: BondPattern.Woven,        repeatUnits: 2, rotationDegrees: 90.0, blockUnits: 2);
    public static readonly BondGeometry Pinwheel    = new("pinwheel",    pattern: BondPattern.Rotary,       repeatUnits: 4, rotationDegrees: 90.0, blockUnits: 1);
    public static readonly BondGeometry Diaper      = new("diaper",      pattern: BondPattern.Lattice,      repeatUnits: 4, rotationDegrees: 45.0, blockUnits: 1);

    public BondPattern Pattern { get; }
    public int RepeatUnits { get; }          // unit-cell repeat length in units before the course offset cycles
    public double RotationDegrees { get; }   // per-unit off-horizontal rotation for diagonal/rotary patterns
    public int BlockUnits { get; }           // basketweave/diaper block size (units per woven block)
    public double OffsetFraction(int course) => RepeatUnits <= 1 ? 0.0 : (course % RepeatUnits) / (double)RepeatUnits;
}

[Union]
public abstract partial record Orientation {
    private Orientation() { }
    public sealed record Stretcher : Orientation;
    public sealed record Header : Orientation;
    public sealed record Soldier : Orientation;
    public sealed record Sailor : Orientation;
    public sealed record Rowlock : Orientation;
    public sealed record Shiner : Orientation;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
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
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class ClosureRule {
    public static readonly ClosureRule None = new("none", closer: Cut.Whole);
    public static readonly ClosureRule QueenCloser = new("queen-closer", closer: Cut.QueenCloser);
    public static readonly ClosureRule KingCloser = new("king-closer", closer: Cut.KingCloser);
    public static readonly ClosureRule HalfBat = new("half-bat", closer: Cut.Half);
    public Cut Closer { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class SpecialShape {
    public static readonly SpecialShape None = new("none");
    public static readonly SpecialShape Bullnose = new("bullnose");
    public static readonly SpecialShape Cownose = new("cownose");
    public static readonly SpecialShape Plinth = new("plinth");
    public static readonly SpecialShape Coping = new("coping");
    public static readonly SpecialShape Cant = new("cant");
    public static readonly SpecialShape Squint = new("squint");
    public static readonly SpecialShape Voussoir = new("voussoir");
}

// The ASTM C270 tooled-joint profile: DepthFactor scales the bed-joint width to the recess depth a raked/struck
// joint carries (0 = flush), ShadowLine the relative shadow weight the weathering#WEATHERING raked-joint AO reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
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

// ASTM C270 mortar types by compressive strength (M=17.2, S=12.4, N=5.2, O=2.4, K=0.5 MPa); EN 998-2 classes map by Mpa.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class MortarType {
    public static readonly MortarType M = new("M", compressiveMpa: 17.2);
    public static readonly MortarType S = new("S", compressiveMpa: 12.4);
    public static readonly MortarType N = new("N", compressiveMpa: 5.2);
    public static readonly MortarType O = new("O", compressiveMpa: 2.4);
    public static readonly MortarType K = new("K", compressiveMpa: 0.5);
    public double CompressiveMpa { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class BondName {
    // Template bonds carry an explicit course set; generated bonds carry a BondGeometry the interpreter computes from.
    public static readonly BondName Running      = new("running",      template: Seq(StretcherAt(0.0), StretcherAt(0.5)));
    public static readonly BondName English      = new("english",      template: Seq(StretcherAt(0.0), HeaderAt(0.5)));
    public static readonly BondName Stack        = new("stack",        generated: BondGeometry.Stack);
    public static readonly BondName Flemish      = new("flemish",      generated: BondGeometry.Flemish);
    public static readonly BondName Herringbone45 = new("herringbone-45", generated: BondGeometry.Herringbone);
    public static readonly BondName Basketweave  = new("basketweave",  generated: BondGeometry.Basketweave);
    public static readonly BondName Pinwheel     = new("pinwheel",     generated: BondGeometry.Pinwheel);
    public static readonly BondName Diaper       = new("diaper",       generated: BondGeometry.Diaper);

    public BondKind Kind { get; }
    public Seq<CourseTemplate> Courses { get; }
    public Option<BondGeometry> Geometry { get; }

    private BondName(string key, Seq<CourseTemplate> template) : this(key) => (Kind, Courses, Geometry) = (BondKind.Template, template, None);
    private BondName(string key, BondGeometry generated) : this(key) => (Kind, Courses, Geometry) = (BondKind.Generated, Seq<CourseTemplate>(), Some(generated));

    [BoundaryAdapter]
    public bool Fits(Profile profile) => profile.Unit.LengthOverWidth > 0.0;

    // A template bond reads its course by wrapped index; a generated bond computes it from its BondGeometry descriptor.
    public Fin<CourseTemplate> Course(int index, Op key) => Kind.Switch(
        template:  _ => Courses.IsEmpty
            ? Fin.Fail<CourseTemplate>(ProfileFault.Bond(key, $"<template-bond-empty:{Key}>"))
            : Fin.Succ(Courses[((index % Courses.Count) + Courses.Count) % Courses.Count]),
        generated: _ => Geometry.ToFin(ProfileFault.Bond(key, $"<generated-bond-missing-geometry:{Key}>"))
            .Map(geometry => GeneratedBond.Interpret(geometry, index)));

    static CourseTemplate StretcherAt(double offset) => new(Seq<Orientation>(new Orientation.Stretcher()), offset, 0.0);
    static CourseTemplate HeaderAt(double offset) => new(Seq<Orientation>(new Orientation.Header()), offset, 0.0);
}

// --- [MODELS] ------------------------------------------------------------------------------
// PerUnitRotationDegrees carries the off-horizontal rotation a herringbone/diaper unit needs; the
// Construction/layout#ASSEMBLY_FOLD StepCourse folds it into Placement.PathAngleDegrees, never a host transform.
public sealed record CourseTemplate(Seq<Orientation> Sequence, double OffsetFraction, double PerUnitRotationDegrees = 0.0);

// The full mortar-joint specification the layout#ASSEMBLY_FOLD JointPolicy resolves head/bed width AND profile
// AND mortar strength from, replacing the single scalar ProfileStandard.StandardJointThicknessMm: a masonry run
// carries its buildable joint detail, the weathering raked-joint shadow line, and the ASTM C270 strength receipt.
public readonly record struct MortarJoint(double HeadWidthMm, double BedWidthMm, MortarProfile Profile, MortarType Mortar) {
    public static Fin<MortarJoint> Of(double headMm, double bedMm, MortarProfile profile, MortarType mortar, Op key) =>
        double.IsFinite(headMm) && headMm > 0.0 && double.IsFinite(bedMm) && bedMm > 0.0
            ? Fin.Succ(new MortarJoint(headMm, bedMm, profile, mortar))
            : Fin.Fail<MortarJoint>(ProfileFault.Bond(key, $"<mortar-joint-nonpositive:head={headMm}:bed={bedMm}>"));

    // Standard coordinating joint from a single thickness — the scalar StandardJointThicknessMm an unspecified run uses.
    public static MortarJoint Standard(double thicknessMm) => new(thicknessMm, thicknessMm, MortarProfile.Concave, MortarType.N);
    public double RecessDepthMm => BedWidthMm * Profile.DepthFactor;
}

public readonly record struct MasonryRow(string Designation, double WMm, double HMm, double LMm, double CourseMm, double JointMm, string Region, string Authority, Coring Coring);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The generative bond algebra: ONE fold over the bounded BondPattern vocabulary deriving a CourseTemplate
// (orientation sequence + offset + per-unit rotation) from a BondGeometry descriptor for a generated bond's
// course index — flemish/herringbone/basketweave/pinwheel/diaper compute their course, never a per-bond method.
public static class GeneratedBond {
    public static CourseTemplate Interpret(BondGeometry geometry, int courseIndex) =>
        geometry.Pattern.Switch(
            allStretcher: _ => new CourseTemplate(Seq<Orientation>(new Orientation.Stretcher()), geometry.OffsetFraction(courseIndex), 0.0),
            alternating:  _ => Flemish(geometry, courseIndex),
            diagonal:     _ => Herringbone(geometry, courseIndex),
            woven:        _ => Basketweave(geometry, courseIndex),
            rotary:       _ => Pinwheel(geometry, courseIndex),
            lattice:      _ => Diaper(geometry, courseIndex));

    // Flemish: each course alternates stretcher-header-stretcher-header, every other course shifted half a unit
    // so a header centres over the stretcher below; the two-unit cell is the OffsetFraction the StepCursor reads.
    static CourseTemplate Flemish(BondGeometry g, int course) =>
        new(Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Header()), (course & 1) == 0 ? 0.0 : 0.25, 0.0);

    // Herringbone: pairs of units laid at ±45°, the per-unit rotation alternating by course parity.
    static CourseTemplate Herringbone(BondGeometry g, int course) =>
        new(Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Stretcher()), 0.0, (course & 1) == 0 ? g.RotationDegrees : -g.RotationDegrees);

    // Basketweave: N×N blocks alternating horizontal/vertical (0°/90°) every BlockUnits courses.
    static CourseTemplate Basketweave(BondGeometry g, int course) =>
        new(toSeq(Enumerable.Range(0, g.BlockUnits)).Map(_ => (Orientation)new Orientation.Stretcher()), 0.0, ((course / g.BlockUnits) & 1) == 0 ? 0.0 : g.RotationDegrees);

    // Pinwheel: four units rotated 0/90/180/270 about a square centre, the rotation stepping with the unit index.
    static CourseTemplate Pinwheel(BondGeometry g, int course) =>
        new(Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Header()), (course % g.RepeatUnits) / (double)g.RepeatUnits, (course % 4) * g.RotationDegrees);

    // Diaper: diamond lattice — units rotated ±45° on a four-course diagonal repeat, the offset stepping each course.
    static CourseTemplate Diaper(BondGeometry g, int course) =>
        new(Seq<Orientation>(new Orientation.Stretcher()), g.OffsetFraction(course), (course % 2 == 0 ? 1 : -1) * g.RotationDegrees);
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    static readonly Seq<MasonryRow> RegionalRows = Seq(
        new MasonryRow("masonry.us-modular",   92.0,  57.0,  194.0, 67.0,  9.5,  "us",  "ASTM C216",   Coring.Cored3Hole),
        new MasonryRow("masonry.uk-standard",  102.5, 65.0,  215.0, 75.0,  10.0, "uk",  "BS EN 771-1", Coring.Perforated10Cell),
        new MasonryRow("masonry.din-nf",       115.0, 71.0,  240.0, 83.5,  12.5, "din", "DIN 105",     Coring.None),
        new MasonryRow("masonry.au-standard",  110.0, 76.0,  230.0, 86.0,  10.0, "au",  "AS 4773",     Coring.Cored3Hole),
        new MasonryRow("masonry.is-standard",  100.0, 70.0,  200.0, 80.0,  10.0, "is",  "IS 1077",     Coring.None));

    static Fin<(ProfileId Id, Profile Profile)> MasonryOf(MasonryRow r, Context context, Op key) =>
        from unit in ProfileUnit.Of(r.WMm, r.HMm, r.LMm, r.CourseMm, context, key)
        let standard = new ProfileStandard(r.Region, r.JointMm, r.Authority)
        select (ProfileId.Of(r.Designation), new Profile(ProfileFamily.Masonry, unit, r.Coring, standard, MaterialId.Of("ceramic.glazed")));

    public static FrozenDictionary<ProfileId, Profile> BuildMasonryRows(Context context) =>
        RegionalRows
            .Choose(row => MasonryOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ProfileKeyPolicy.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [GENERATED_BOND_INTERPRETER]: REALIZED — the typed generated-bond interpreter algebra `BondName` owns is the `GeneratedBond.Interpret` fold over the bounded `BondPattern` vocabulary deriving a `CourseTemplate` (orientation sequence + `OffsetFraction` + `PerUnitRotationDegrees`) from a row's `BondGeometry` descriptor (repeat-units, per-course pattern, rotation, block-units), so the stack/flemish/herringbone/basketweave/pinwheel/diaper generated bonds COMPUTE their course rather than railing `ProfileFault.Bond`. The descriptor is the data — a new decorative bond (quetta, monk, sussex) is one `BondGeometry` row binding one `BondPattern`, never a per-bond `Interpret` method or a parallel generated-layout owner. The per-unit rotation a herringbone/diaper/pinwheel emits rides the `CourseTemplate.PerUnitRotationDegrees` column the `Construction/layout#ASSEMBLY_FOLD` `StationStep` folds into `Construction/assembly#ELEMENT_MODEL` `Placement.PathAngleDegrees`, never a widening of the host-neutral `Element` model. The flemish two-unit cell (stretcher-header per unit, half-unit course shift) and the basketweave N×N block alternation are the realized cell rules; a generated bond's `Fits` aspect-ratio gate stays the `[BoundaryAdapter]` check, the catalogue unchanged.
- [MORTAR_JOINT_PROFILE]: REALIZED — the `MortarProfile` `[SmartEnum]` (concave/V/weathered/struck/raked/flush/beaded ASTM C270 tooled-joint profiles, each carrying a `DepthFactor` recess scale and a `ShadowLine` weight) and the `MortarType` `[SmartEnum]` (M/S/N/O/K ASTM C270 compressive classes 17.2/12.4/5.2/2.4/0.5 MPa, EN 998-2 mapping by Mpa) close the mortar vocabulary, and the `MortarJoint` record (head-width, bed-width, profile, mortar type) is the full buildable joint specification the `Construction/layout#ASSEMBLY_FOLD` `JointPolicy.Resolve` reads beside the coordinating thickness — replacing the single scalar `ProfileStandard.StandardJointThicknessMm` with head AND bed AND profile AND strength. A non-positive joint width rails `ProfileFault.Bond` at `MortarJoint.Of`; an unspecified run reads `MortarJoint.Standard(thicknessMm)` (the concave/Type-N coordinating default) so the scalar route survives as the default. The `MortarProfile.ShadowLine`/`RecessDepthMm` is the source the `weathering#WEATHERING` raked-joint cavity-mask AO reads, the `MortarType.CompressiveMpa` the strength receipt the thermal/structural seam reads, and the `StationStep` pitch reads the `HeadWidthMm` as the per-unit head joint unchanged.
- [MASONRY_ROW_TRANSCRIPTION]: REALIZED — the regional masonry cross-section catalogue (the us/uk/din/au/is regional rows) seeds through `ProfileCatalogue.BuildMasonryRows(context)` over the `MasonryRow` raw-double table, the dimension/coursing/joint columns admitting once through `ProfileUnit.Of` into the kernel-`PositiveMagnitude`-composed `ProfileUnit` and the `BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape` algebra realized as the masonry vocabulary; the five regional rows are the seed the `Construction/layout#ASSEMBLY_FOLD` course fold consumes and a new region is one `MasonryRow` data addition. The standard module/joint values transcribe the ASTM C216 / BS EN 771-1 / DIN 105 / AS 4773 / IS 1077 published dimensions; a non-positive column rails the `ProfileUnit.Of` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `Profile`.
