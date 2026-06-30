# [MATERIALS_PROFILE]

THE POLYMORPHIC PROFILE OWNER, THE CANONICAL `ComputedSection` RECEIPT, and THE FAMILY GROWTH AXIS. One `Profile` is the canonical cross-section concept every architectural material parameterizes — a unit shape (dimensions, coring, regional standard, source receipt) plus the family vocabulary its layout assembly reads; one `ProfileFamily` `[SmartEnum<string>]` closes the family-kind axis (masonry · cmu · steel · timber · glazing), all five realized as sibling cross-section vocabularies the one `ProfileCatalogue.Build` folds. A profile is NEVER a per-material class: a brick is a `Profile` in the `masonry` family, a steel section is a `Profile` in the `steel` family, a CMU in `cmu`, a glulam in `timber`, an IGU in `glazing`, differing only in unit columns and the family discriminant — never a `BrickProfile`/`SteelProfile` type. The owner is `IfcProfileDef`-aligned (the canonical parameterized-cross-section model the BIM wire serializes). The OOP capsule lives at the boundary (the `[ValueObject]` dimensional columns, the `[ValidationError]` dimensional validators), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` course projection).

This owner also defines the ONE canonical `ComputedSection` — the seventeen-field strong-AND-weak-axis stiffness receipt the `Rasm.Compute` design-code checks read off the seam — that EVERY realized family fills through the SAME `VividOrange.Sections.SectionProperties` Green's-theorem polygon integral, never a per-family closed-form literal: the parametric families (`cmu` hollow net section, `timber` rectangle, a built-up composite) integrate through `ParametricSection.Rectangle`/`Hollow` over a built `Perimeter`, and the `steel` family integrates the catalogued `IProfile` and FILLS the warping / shear-area / plastic columns the elastic integral cannot yield. The page composes the `Rasm` kernel for the unit/dimension value-objects and `Construction/layout#ASSEMBLY_FOLD` for the layout that turns a `Profile` into a placement stream; the appearance assignment is the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` row a `Profile` maps to, never a profile-specific appearance type. Each family vocabulary lives on its own sibling page (`masonry`/`steel`/`cmu`/`timber`/`glazing`); this page owns the one `Profile` shape, the closed family axis, the canonical `ComputedSection` receipt + the shared `ParametricSection` solver, and the `[M7]` one-hop `ProfileResolution` that dereferences the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.ProfileSet` `ProfileRef` to a `(Profile, Option<ComputedSection>)` receipt (`Some` for a profiled structural member, `None` for a non-profiled masonry/glazing one) so a structural consumer never re-runs the section integral per call.

## [01]-[INDEX]

- [01]-[PROFILE_OWNER]: the `Profile` canonical unit shape, the `ProfileFamily` family-kind axis, the `ProfileId` key, the `ProfileFault` `[Union]` band, the `ProfileAuthority`/`ProfileStandard` standards vocabulary, the `ProfileUnit` dimensional value-object, the canonical `ComputedSection` seventeen-field receipt + the shared `ParametricSection` polygon-integral solver, and the `ProfileCatalogue` registered-row + section table.
- [02]-[PROFILE_RESOLUTION]: the seam-`ProfileRef` one-hop `ProfileResolution` — the `ResolvedProfile` `(Profile, Option<ComputedSection>)` receipt and the frozen `Build`/`Resolve` cache the seam `MaterialComposition.ProfileSet` dereferences ([M7]).

## [02]-[PROFILE_OWNER]

- Owner: `Profile` over the closed `ProfileFamily` axis; `ProfileId` key; `ProfileFault` `[Union]` band 2300; `ComparerAccessors.StringOrdinal` accessor; `ComputedSection` the canonical seventeen-field section receipt; `ParametricSection` the shared polygon-integral solver; `ProfileCatalogue` the registered `ProfileId`→`Profile` row table AND the `ProfileId`→`ComputedSection` section table.
- Cases: one `Profile` shape across all families — `Family` (the discriminant), `Unit` (the dimensional cross-section), `Coring` (the void-class row from `masonry#PROFILE_FAMILY`), `Standard` (the regional source receipt over a bounded `ProfileAuthority`), `AppearanceId` (the `MaterialId` row); family {masonry · cmu · steel · timber · glazing}; a family is a `ProfileFamily` ROW, never a profile subtype. The section receipt is one `ComputedSection` shape across all families — the same seventeen columns whether sourced from a polygon integral (parametric) or a catalogued `IProfile` (steel).
- Entry: `public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key)` — `Fin<T>` aborts on a void-fraction outside `[0,1)` (`ProfileFault.Coring`, key-correlated); the dimensional admission (`ProfileFault.Dimension` on a non-positive length) already happened in `ProfileUnit.Of` whose admitted value the caller threads, so `Of` re-guards only the invariant `Profile` itself owns (the family/coring relation) — a `Profile` cannot re-validate a `PositiveMagnitude` the kernel already proved. `ProfileCatalogue.Build(context)` folds every realized family's `ProfileId`→`Profile` row builder into the one frozen registry and `ProfileCatalogue.Sections(context)` folds every realized family's `ProfileId`→`ComputedSection` section map into the one frozen section table, `ProfileCatalogue.Lookup(rows, id, key)` resolves a registered `ProfileId` to its catalogue `Profile`, and the same `Of` admits an ad-hoc unit through the row validation a registered row passes — one polymorphic entry, never a `GetById`/`GetByFamily` family.
- Packages: Rasm (project — `PositiveMagnitude` the double-backed `> 0` finite magnitude for every length column, `Op`/`Context`/`Expected`), Rasm.Element (project — `MaterialId` the per-row appearance carries, `ProfileRef` the `[02]` resolves), VividOrange.Sections.SectionProperties + VividOrange.Profiles.Perimeter + VividOrange.Geometry (the shared parametric section bridge — `new Perimeter(outerEdge, voidEdges)` over `LocalPolyline2d`/`LocalPoint2d` fed to `new SectionProperties(IProfile)`; `.api/api-vividorange-sections-sectionproperties.md`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new architectural cross-section is one `Profile` row in the matching `ProfileFamily`; a new family is one `ProfileFamily` case carrying its unit vocabulary AND its `ProfileId`→`ComputedSection` section map on its own sibling page folded into `ProfileCatalogue.Build`/`ProfileCatalogue.Sections`; a new fault is one `ProfileFault` case; a new structural-design column is one `ComputedSection` field every family fills — never a `BrickProfile`/`SteelSection` parallel receipt, never a per-family `Profile` variant. All five families are realized, each its own `ProfileFamily` case with its own `ProfileUnit` projection and `BuildXRows` catalogue builder: `masonry` carries the void-class/bond/orientation algebra and the regional rows (no section — a unit course, not a structural member), `steel` the AISC/EN catalogued sections filling the warping/shear columns, `cmu` the ASTM C90 hollow net section, `timber` the EN 14080/APA PRG 320 rectangle, `glazing` the IGU layer set (no section — an `IfcMaterialLayerSet`, not a profile) — a new section in any family is a row data addition, never a new surface.
- Boundary: `Profile` is the ONE cross-section concept — a per-material class is the deleted form; `ProfileUnit` composes the `Rasm` kernel `PositiveMagnitude` for every length column (width/height/length and the coursing module) so the profile never re-mints a dimension primitive and a fractional millimeter — an AISC web thickness, a metric joint module — admits without the truncation an int-backed dimension count would force; `ProfileFault` is the one fault every `Fin.Fail` reads (dimension/coring/family/bond/mortar/section/capacity slots), an `Expected`-derived `Error` (`IValidationError<ProfileFault>`) whose 2300 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail, so a layout never throws and never returns a sentinel; the `Section` slot is DISTINCT from `Family` because a section-computation failure (a degenerate net rectangle, a non-finite polygon-integral output) is a different fault than an unregistered/mis-family ref — `ParametricSection.Admit` rails `ProfileFault.Section`, the `ProfileResolution` cache rails `ProfileFault.Family` only for a genuinely unregistered `ProfileRef`; the `Capacity` slot is likewise DISTINCT from `Section` because a section-CAPACITY-SOLVE failure (the `Profiles/capacity#SECTION_CAPACITY` `SectionCapacityResolver.Resolve` `InteractionDiagram` eager solve / a degenerate RC-timber-masonry design solve) is a different fault than a section-PROPERTY-integral failure — `SectionCapacityResolver.Resolve` rails `ProfileFault.Capacity`, so a capacity telemetry reader banding by `Expected.Code`+`Category` separates a design-solve fault from a section-property fault and from a registration fault; the `Mortar` slot is likewise DISTINCT from `Bond` because a mortar-joint width/spec defect (`masonry#PROFILE_FAMILY` `MortarJoint.Of` on a non-positive head/bed width) is a different fault than a course-pattern defect (an empty bond template, a generated bond missing its geometry) — `MortarJoint.Of` rails `ProfileFault.Mortar`, `BondName.Course` rails `ProfileFault.Bond`, and this profile-tier spec slot stays distinct from the layout-tier `Construction/assembly#PLACEMENT_MODEL` `ConstructionFault.Joint` (band 2350) the run fold rails on the RESOLVED coordinating joint; the `[ValidationError]` dimensional validators are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` course projection is the FP-ROP internal; the `ProfileStandard` upgrades its `Authority` to the bounded `ProfileAuthority` `[SmartEnum]` (the published standards-body row) so a consumer dispatches on `standard.Authority` rather than a free authority string, keeping `Region` as an explicit token (a masonry `din`/`au`/`is` row's region is not the authority's home region) and `StandardJointThicknessMm` by name (the `Construction/layout#ASSEMBLY_FOLD` coursing reads it as masonry/cmu provenance, the steel/timber/glazing families passing `0.0` because a rolled/sawn/IGU profile has no mortar joint — the buildable joint detail is the `masonry#PROFILE_FAMILY` `MortarJoint` the masonry layout reads, never a structural section input); the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `Profile.AppearanceId` column carries, never a profile-specific `SurfaceShade`; the layout that turns a `Profile` into a placement stream is `Construction/layout#ASSEMBLY_FOLD`, composed not re-derived here.

The canonical `ComputedSection` is the ONE structural-section receipt the whole family axis shares — a per-family rectangular `W·D²/6` / cell-area-subtraction literal is the deleted form. The elastic columns (`Area`, both-axis second moment `IxMm4`/`IyMm4`, both-axis elastic modulus `SxMm3`/`SyMm3`, both-axis radius of gyration `RxMm`/`RyMm`) and the fire-exposed `HeatedPerimeterMm` come from the ONE `VividOrange.Sections.SectionProperties` polygon integral (`Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter`), never a per-family closed-form constant; the plastic moduli (`ZxMm3`/`ZyMm3`), the St-Venant torsion constant (`JMm4`), the warping constant (`IwMm6`, the EN 1993-1-1 §6.3.2 lateral-torsional-buckling input the bare `J` cannot supply — engineering-zero for the solid/closed parametric families, positive only for an OPEN thin-walled steel shape), and the both-axis shear areas (`AvyMm2`/`AvzMm2`) the polygon solver does NOT expose are COMPUTED from the section geometry — the rectangle/hollow closed forms here for the parametric families, the `steel#STEEL_FAMILY` `SteelStiffness` per-topology algebra over the catalogued shape — so a steel W-shape and a glulam rectangle resolve the SAME seventeen-field receipt and `Projection/material#MATERIAL_PROJECTOR` `SeamSection` lifts the whole set onto the seam `Composition/material#MATERIAL_COMPOSITION` `SectionProperties` (mm→SI, each a typed `MeasureValue` in the seam's declared order with `Iw` 5th) without re-resolving or admitting VividOrange downstream. `DepthMm`/`WidthMm` are the bounding cross-section dimensions; `AxisDistanceMm` is the EN 1992-1-2 cover-to-reinforcement and is engineering-zero for every non-RC parametric/catalogued section here — the RC reinforcement cover rides the `Connection/reinforcement#RC_SECTION` `ConcreteSectionProperties` path, the `ComputedSection` owner declaring the column the RC resolver fills, never a phantom this owner sources.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                                   // PositiveMagnitude, Op, Context
using Rasm.Element;                                  // MaterialId, ProfileRef (the seam handle the [02] resolves)
using Thinktecture;
using VividOrange.Geometry;                          // LocalPoint2d, LocalPolyline2d, ILocalPoint2d, ILocalPolyline2d (the Y-Z section-plane geometry)
using VividOrange.Profiles;                          // Perimeter, IProfile (the parametric section input)
using VividOrange.Sections.SectionProperties;        // SectionProperties polygon-integral solver over IProfile
using UnitsNet;                                      // Length (the section-plane coordinate quantity)
using Expected = Rasm.Domain.Expected;               // the kernel Expected (parameterless ctor + virtual Category), NOT LanguageExt.Common.Expected
using Rasm.Materials.Profiles.Masonry;               // Coring (the masonry#PROFILE_FAMILY void-class the Profile record + Of carry)
using static LanguageExt.Prelude;

// profile#PROFILE_OWNER is the PARENT Rasm.Materials.Profiles — it owns Profile/ProfileFamily/ComputedSection/
// ParametricSection/ProfileCatalogue.Build/Sections and folds each family's own catalogue by the sub-namespace-qualified
// name (Masonry./Steel./Cmu./Timber./Glazing.ProfileCatalogue, each its own Rasm.Materials.Profiles.<Family>), the child
// leaf visible from the parent without a using. Coring lives in the masonry child, imported above for the Profile field.
namespace Rasm.Materials.Profiles;

// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public readonly partial struct ProfileId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.Contains('.') ? new ValidationError("<profile-id requires 'family.name'>") : null;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileFamily {
    public static readonly ProfileFamily Masonry = new("masonry");
    public static readonly ProfileFamily Cmu = new("cmu");
    public static readonly ProfileFamily Steel = new("steel");
    public static readonly ProfileFamily Timber = new("timber");
    public static readonly ProfileFamily Glazing = new("glazing");
}
// Which families contribute a profiled ComputedSection (steel/cmu/timber) vs the unit-course masonry / layer-set glazing
// is NOT a family-flag here — it is the per-profile ResolvedProfile.Section Option a consumer reads (Some => profiled,
// None => non-profiled): the Option is the consumed answer, so a redundant ProfileFamily.Sectioned bool would be a
// decorative second source. ProfileCatalogue.Sections folds exactly the steel/cmu/timber section maps the families expose.

// The published standards body a regional ProfileStandard cites — a bounded vocabulary, never a free Authority string
// (SEMANTIC_NAMING/POLICY_VALUES): a row per standards organization carrying its scope so a consumer dispatches on the
// authority rather than string-matching "ASTM C216"; a new authority is one row, never a parallel "standard" enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProfileAuthority {
    public static readonly ProfileAuthority Astm = new("ASTM", region: "us");          // ASTM C90/C216/C652 masonry & CMU
    public static readonly ProfileAuthority Aisc = new("AISC", region: "us");          // AISC Shapes Database v16.0
    public static readonly ProfileAuthority Aisi = new("AISI", region: "us");          // AISI S100 cold-formed
    public static readonly ProfileAuthority En = new("EN", region: "eu");              // EN 10365/338/14080/14374/1279
    public static readonly ProfileAuthority Bs = new("BS", region: "uk");              // BS EN 771 masonry
    public static readonly ProfileAuthority Din = new("DIN", region: "din");           // DIN 105 masonry
    public static readonly ProfileAuthority As = new("AS", region: "au");              // AS 4773 masonry
    public static readonly ProfileAuthority Is = new("IS", region: "is");              // IS 1077 masonry
    public static readonly ProfileAuthority Apa = new("APA", region: "us");            // APA PRG 320 CLT
    public string Region { get; }

    public static Fin<ProfileAuthority> Parse(string token, Op key) =>
        TryGet(token, out ProfileAuthority? a) && a is { } v ? Fin.Succ(v) : ProfileFault.Family(key, $"<profile-authority-unknown:{token}>");
}

// --- [ERRORS] ------------------------------------------------------------------------------
// The profile-sub-domain fault band (2300): Expected-derived over the kernel Rasm.Domain.Expected so band 2300 IS the
// Expected Code and a typed case lifts BARE onto Fin<T>/Validation<Error,T> (no .ToError() hop). The kernel base ctor
// is PARAMETERLESS (Code a virtual Error member, Message abstract, Category virtual), so band 2300 is a `Code => 2300`
// override and `Message => Detail`, and the per-case Category override drives FaultExtensions.Category(error). The seven
// slots are disjoint: Dimension a non-positive/non-finite length, Coring a void fraction outside [0,1), Family a
// family/standard/registration mismatch, Bond a masonry COURSE-PATTERN fault (an empty template course set, a generated
// bond missing its geometry — the laying pattern itself), Mortar a masonry MORTAR-JOINT-SPEC fault (a non-positive head/
// bed joint width at MortarJoint.Of — the joint specification, NOT the course pattern), Section a section-INTEGRAL
// failure (a degenerate net rectangle, a non-finite polygon-solver output), Capacity a section-CAPACITY-SOLVE failure
// (the Profiles/capacity#SECTION_CAPACITY InteractionDiagram eager solve / a degenerate RC-timber-masonry design solve —
// distinct from Section's elastic-integral failure, so a capacity telemetry reader bands a design-solve fault apart from
// a section-property fault under Expected.Code). Mortar is disjoint from Bond exactly as
// Section is disjoint from Family: a joint-width/spec defect is not a course-pattern defect, so the masonry layout reads
// the true cause off Category rather than flattening every masonry fault to "Bond"; this Mortar SPEC slot is also
// distinct from the layout-tier Construction/assembly#PLACEMENT_MODEL ConstructionFault.Joint (band 2350) the run fold
// rails on a non-positive RESOLVED coordinating joint — the profile vocabulary owns the spec admission, the layout owns
// the run-geometry check. The Section slot likewise frees Family to mean ONLY a mis-keyed/unregistered ref, so a
// downstream Resolve fault names the true cause. [SkipUnionOps] skips the generated
// implicit-conversion ops (every case carries an explicit Op) and emits NO per-case factory, so the band declares its
// own (the production UiFault / seam ElementFault shape): a nested `…Case` record carries the data and a same-name-less
// static factory ProfileFault.Section(key, detail) returns the Expected-derived base so the case lifts BARE onto Fin<T>
// with no `new` and no .ToError() hop — the `…Case` suffix frees the unsuffixed factory name (a same-named nested type
// + method is CS0102). Create routes the unspecific case under a boundary-admission Op.
[SkipUnionOps]
[Union]
public abstract partial record ProfileFault : Expected, IValidationError<ProfileFault> {
    private ProfileFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => 2300;
    public override string Message => Detail;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record DimensionCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Dimension"; }
    public sealed record CoringCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Coring"; }
    public sealed record FamilyCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Family"; }
    public sealed record BondCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Bond"; }
    public sealed record MortarCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Mortar"; }
    public sealed record SectionCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Section"; }
    public sealed record CapacityCase(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Capacity"; }

    public static ProfileFault Dimension(Op key, string detail) => new DimensionCase(key, detail);
    public static ProfileFault Coring(Op key, string detail) => new CoringCase(key, detail);
    public static ProfileFault Family(Op key, string detail) => new FamilyCase(key, detail);
    public static ProfileFault Bond(Op key, string detail) => new BondCase(key, detail);
    public static ProfileFault Mortar(Op key, string detail) => new MortarCase(key, detail);
    public static ProfileFault Section(Op key, string detail) => new SectionCase(key, detail);
    public static ProfileFault Capacity(Op key, string detail) => new CapacityCase(key, detail);
    public static ProfileFault Create(string message) => Family(Admission, message);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The dimensional cross-section every family projects to — width/height/length plus the coursing module — each column
// a kernel PositiveMagnitude admitted ONCE through ProfileUnit.Of, so the interior carries proven-positive magnitudes
// and no length re-validates. A non-positive/non-finite column rails ProfileFault.Dimension (key-correlated) at the
// ONE admission, never a sentinel that seeds a degenerate Profile.
public readonly record struct ProfileUnit(PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, PositiveMagnitude LengthMm, PositiveMagnitude CourseHeightMm) {
    // The unit aspect ratio the masonry#PROFILE_FAMILY BondGeometry.Admits tiling gate reads — length-over-HEIGHT (the
    // course-laying ratio a herringbone/diaper cell bounds), the ONE owner-provided derived projection the bond-fit gate
    // composes (profile.Unit.LengthOverHeight) rather than re-spelling LengthMm.Value / HeightMm.Value inline. NOT
    // length-over-width: the course tiles along the bed, so height — not width — is the load-bearing aspect denominator.
    public double LengthOverHeight => LengthMm.Value / HeightMm.Value;
    public static Fin<ProfileUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: widthMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: heightMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: lengthMm)
        from c in key.AcceptValidated<PositiveMagnitude>(candidate: courseHeightMm)
        select new ProfileUnit(w, h, l, c);
}

// The regional source receipt: the region token + the bounded standards Authority + the as-published coordinating joint
// thickness a masonry/cmu coursing reads (the steel/timber/glazing families pass StandardJointThicknessMm = 0 because a
// rolled/sawn/IGU profile has no mortar joint — the column is masonry/cmu provenance the Construction/layout#ASSEMBLY_FOLD
// JointPolicy reads, NOT a structural section input). The Authority is the bounded ProfileAuthority [SmartEnum] (the
// published standards-body row) REPLACING the prior free Authority string, so a consumer dispatches on standard.Authority
// rather than string-matching "ASTM C216"; Region stays an explicit token because a masonry din/au/is regional row carries
// a region the single authority does not name. The field name StandardJointThicknessMm is preserved (the layout fold and
// the cmu builder read it), so this reshape upgrades ONLY the Authority type — the minimal cross-family break.
public readonly record struct ProfileStandard(string Region, double StandardJointThicknessMm, ProfileAuthority Authority);

public sealed record Profile(
    ProfileFamily Family,
    ProfileUnit Unit,
    Coring Coring,
    ProfileStandard Standard,
    MaterialId AppearanceId) {

    // The ONLY invariant Profile owns beyond its already-admitted columns: the coring void fraction is a physical
    // fraction in [0,1). The dimensional admission lives in ProfileUnit.Of (the kernel PositiveMagnitude rail), so Of
    // re-guards ONLY the family/coring relation — a Profile cannot re-prove a PositiveMagnitude the kernel admitted, and
    // a family/unit "mismatch" is unrepresentable because each family's ToUnit projection produces a valid ProfileUnit.
    public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key) =>
        coring.VoidFraction is >= 0.0 and < 1.0
            ? Fin.Succ(new Profile(family, unit, coring, standard, appearanceId))
            : ProfileFault.Coring(key, $"<void-fraction-out-of-range:{coring.VoidFraction:R}>");
}

// The ONE canonical section-property receipt every Profiles family shares — the FULL structural-design + fire column
// set the Rasm.Compute design-code checks read off the seam, in SI millimetres. The elastic columns (Area / strong-AND-
// weak-axis inertia IxMm4=Iyy,IyMm4=Izz / elastic modulus SxMm3=Wely,SyMm3=Welz / radius of gyration) and the fire-
// exposed HeatedPerimeterMm come from the ONE VividOrange polygon integral; the plastic moduli (ZxMm3=Wply, ZyMm3=Wplz),
// the St-Venant torsion constant (JMm4), the warping constant (IwMm6, the EN 1993-1-1 §6.3.2 lateral-torsional-buckling
// input the bare J cannot supply), and the both-axis shear areas (AvyMm2 the MAJOR-axis/web shear, AvzMm2 the MINOR-axis/
// flange shear — the SAME X=major(Yy)/Y=minor(Zz) convention the inertia/modulus columns carry, so SeamSection maps
// AvyMm2->AvY(major)/AvzMm2->AvZ(minor) and the §G design web shear reads AvyMm2) the solver does NOT expose are COMPUTED
// from the section geometry (ParametricSection's rectangle/hollow closed forms for the parametric families, which fill
// BOTH shear columns with the symmetric gross net area and so never observe the major/minor split; steel#STEEL_FAMILY
// SteelStiffness over the catalogued shape for the OPEN topologies where the split is load-bearing). DepthMm/WidthMm are the bounding dimensions; AxisDistanceMm is the EN 1992-1-2
// cover-to-reinforcement (0 for a non-RC section, the RC value from the Connection/reinforcement#RC_SECTION ConcreteSectionProperties
// path). Projection/material#MATERIAL_PROJECTOR SeamSection lifts this whole set onto the SEVENTEEN-field neutral seam
// SectionProperties (mm -> SI, each a typed MeasureValue in the seam's declared order with Iw 5th), so a Rasm.Compute
// structural/fire runner reads graph.SectionOf(member) without re-resolving or admitting VividOrange.
public readonly record struct ComputedSection(
    PositiveMagnitude AreaMm2,
    PositiveMagnitude IxMm4,
    PositiveMagnitude IyMm4,
    PositiveMagnitude SxMm3,
    PositiveMagnitude SyMm3,
    PositiveMagnitude RxMm,
    PositiveMagnitude RyMm,
    PositiveMagnitude ZxMm3,
    PositiveMagnitude ZyMm3,
    PositiveMagnitude JMm4,
    double IwMm6,
    PositiveMagnitude AvyMm2,   // MAJOR-axis shear area (the web of an OPEN steel shape, the §G design web shear φVn reads it); the seam AvY
    PositiveMagnitude AvzMm2,   // MINOR-axis shear area (the two flanges); the seam AvZ. A symmetric parametric family (cmu/timber) fills BOTH with the gross net area, so it never observes the major/minor split — the convention is load-bearing only for the steel topologies, pinned HERE at the owner so a future family fills the pair consistently
    PositiveMagnitude DepthMm,
    PositiveMagnitude WidthMm,
    PositiveMagnitude HeatedPerimeterMm,
    double AxisDistanceMm) {

    // The WEAK-axis radius the column-buckling check governs about — a derived read over the two solver radii, never a
    // stored column that could drift from the pair the polygon integral supplies (the steel#STEEL_FAMILY Capacity reads it).
    public double GoverningRadiusMm => Math.Min(RxMm.Value, RyMm.Value);
}
// IwMm6 is the EN 1993-1-1 §6.3.2 / AISC 360 Ch.F warping constant (mm^6) the seam SectionProperties carries 5th (after J)
// — the lateral-torsional-buckling input the bare J cannot supply. It is a plain double NOT a PositiveMagnitude (the SAME
// zero-admitting modeling as AxisDistanceMm) because a SOLID or CLOSED section's warping resistance is engineering-zero —
// ParametricSection yields 0.0 for every rectangle/hollow parametric family, and an OPEN thin-walled steel shape carries
// its own positive Iw from the steel-family SteelStiffness; the seam Warping map lifts it to
// MeasureValue.OfSi(QuantityType.Create("WarpingConstant"), Dimension.Create(6,0,0,0,0,0,0), mm6 * 1e-18).

// --- [OPERATIONS] --------------------------------------------------------------------------
// The shared parametric section-property bridge: one VividOrange.Sections.SectionProperties Green's-theorem integral
// over a built Perimeter (outer polyline + void edges) computes the elastic columns for a non-catalogued section, so a
// cmu hollow net section, a timber rectangle, and a built-up composite all integrate EXACTLY through the SAME solver the
// steel#STEEL_FAMILY runs over a catalogued IProfile — no per-family literal. The plastic/torsion/shear columns the
// elastic integral cannot yield arrive from the rectangle/hollow closed forms below, so the whole seventeen-field
// ComputedSection is filled for a parametric family the way SteelStiffness fills it for a catalogued one.
public static class ParametricSection {
    public static Fin<ComputedSection> Rectangle(double widthMm, double depthMm, Op key) =>
        Solve(RectanglePerimeter(widthMm, depthMm, Seq<(double, double, double, double)>()), depthMm, widthMm,
            RectanglePlastics(widthMm, depthMm), key);

    // The hollow net section: the outer rectangle minus the inset cell voids — the exact net Area AND net inertia (cells
    // subtracted from the second moment), not an approximate gross-minus-cell-area scalar.
    public static Fin<ComputedSection> Hollow(double widthMm, double depthMm, Seq<(double X, double Y, double W, double H)> voids, Op key) =>
        Solve(RectanglePerimeter(widthMm, depthMm, voids), depthMm, widthMm, HollowPlastics(widthMm, depthMm, voids), key);

    // The IProfile gross-cross-section outline for a Profile — the rectangle the Profile.Unit width/height bounds — the
    // Connection/reinforcement#RC_SECTION RcSection.Of feeds to a VividOrange.Sections ConcreteSection as the concrete
    // outline, the SAME perimeter the section-property integral runs over so the RC elastic + N-M-M solvers and the gross
    // section properties share one IProfile. A catalogued steel section passes its own ICatalogue IProfile. This is a
    // cross-file public seam the RC section consumes, NOT single-call internal sprawl — it stays Fin<IProfile> railing
    // ProfileFault.Dimension on a degenerate unit (the RcSection.Of consumer threads the rail) and keeps the name ProfileOf.
    public static Fin<IProfile> ProfileOf(Profile profile, Op key) =>
        profile.Unit.WidthMm.Value > 0.0 && profile.Unit.HeightMm.Value > 0.0
            ? Fin.Succ((IProfile)RectanglePerimeter(profile.Unit.WidthMm.Value, profile.Unit.HeightMm.Value, Seq<(double, double, double, double)>()))
            : ProfileFault.Dimension(key, $"<profile-perimeter-degenerate:{profile.Family.Key}>");

    // The plastic/torsion/shear columns the VividOrange ELASTIC polygon integral does not expose, COMPUTED from the
    // rectangle geometry (closed-form, EXACT for a solid rectangle): the plastic moduli Z = b·h²/4 (shape factor 1.5 over
    // the elastic W = b·h²/6), the St-Venant rectangle torsion constant J = a·b³·(1/3 − 0.21·(b/a)·(1 − (b/a)⁴/12)) with
    // a/b the long/short side (Roark Table 10.1, a/b ≥ 1), and the plastic shear area Av = A (EN 1993-1-1 §6.2.6(3): a
    // solid section's shear area is its gross area). The parametric families (timber/masonry rectangle, cmu hollow) are
    // rectangles, so these are the section's true mechanics; a catalogued thin-walled steel shape carries its OWN
    // steel#STEEL_FAMILY SteelStiffness plastic/web-flange shear areas, not this rectangle bound.
    static (double Zx, double Zy, double J, double Avy, double Avz) RectanglePlastics(double w, double d) {
        double area = w * d;
        return (w * d * d / 4.0, d * w * w / 4.0, Torsion(w, d), area, area);
    }

    // The hollow net columns via gross-minus-void superposition (the doubly-symmetric centred cells the cmu#CMU_FAMILY
    // generates straddle both centroidal bending axes, so the net plastic modulus is the gross minus each void's own
    // plastic modulus and the net torsion the gross minus each void's St-Venant constant; the net shear area is the gross
    // minus the void material) — a documented engineering net-section approximation for the perforated rectangle.
    static (double Zx, double Zy, double J, double Avy, double Avz) HollowPlastics(double w, double d, Seq<(double X, double Y, double W, double H)> voids) {
        double zx = w * d * d / 4.0 - voids.Sum(static v => v.W * v.H * v.H / 4.0);
        double zy = d * w * w / 4.0 - voids.Sum(static v => v.H * v.W * v.W / 4.0);
        double j = Torsion(w, d) - voids.Sum(static v => Torsion(v.W, v.H));
        double netArea = w * d - voids.Sum(static v => v.W * v.H);
        return (zx, zy, j, netArea, netArea);
    }

    // The Roark Table 10.1 St-Venant torsion constant for a solid rectangle, a/b the long/short side — the one closed
    // form RectanglePlastics and HollowPlastics share, so the rectangle torsion is computed ONCE not transcribed twice.
    static double Torsion(double a, double b) {
        double lng = Math.Max(a, b), sht = Math.Min(a, b);
        return lng * sht * sht * sht * (1.0 / 3.0 - 0.21 * (sht / lng) * (1.0 - Math.Pow(sht / lng, 4) / 12.0));
    }

    static Fin<ComputedSection> Solve(Perimeter perimeter, double depthMm, double widthMm, (double Zx, double Zy, double J, double Avy, double Avz) plastics, Op key) =>
        Admit(new SectionProperties((IProfile)perimeter), depthMm, widthMm, plastics, key);

    // The elastic columns + the fire-exposed Perimeter come from the VividOrange polygon integral; the plastic moduli /
    // torsion / shear areas the solver cannot yield arrive precomputed from the rectangle/hollow geometry; Depth/Width are
    // the bounding dimensions and AxisDistance is zero (a non-RC parametric section — the RC cover rides the
    // Connection/reinforcement#RC_SECTION path). Every column admits once through the kernel PositiveMagnitude rail, so a
    // degenerate (non-positive net) section drops to ProfileFault.SECTION rather than seeding a corrupt stiffness on the
    // seam — the Section fault slot, never Dimension (a length input) or Family (a registration mismatch).
    static Fin<ComputedSection> Admit(SectionProperties p, double depthMm, double widthMm, (double Zx, double Zy, double J, double Avy, double Avz) plastics, Op key) =>
        (from area in Section(p.Area.SquareMillimeters, key)
         from ix in Section(p.MomentOfInertiaYy.MillimetersToTheFourth, key)
         from iy in Section(p.MomentOfInertiaZz.MillimetersToTheFourth, key)
         from sx in Section(p.ElasticSectionModulusYy.CubicMillimeters, key)
         from sy in Section(p.ElasticSectionModulusZz.CubicMillimeters, key)
         from rx in Section(p.RadiusOfGyrationYy.Millimeters, key)
         from ry in Section(p.RadiusOfGyrationZz.Millimeters, key)
         from zx in Section(plastics.Zx, key)
         from zy in Section(plastics.Zy, key)
         from jj in Section(plastics.J, key)
         from avy in Section(plastics.Avy, key)
         from avz in Section(plastics.Avz, key)
         from depth in Section(depthMm, key)
         from width in Section(widthMm, key)
         from perim in Section(p.Perimeter.Millimeters, key)
         // IwMm6: 0.0 — a solid/closed RECTANGLE (every parametric family) has engineering-zero warping resistance
         // (the EN 1993-1-1 §6.3.2 warping constant is nonzero only for OPEN thin-walled shapes); AxisDistanceMm: 0.0
         // yields a zero cover for a non-RC section. Both lift to a zero MeasureValue at the seam SeamSection map.
         select new ComputedSection(area, ix, iy, sx, sy, rx, ry, zx, zy, jj, IwMm6: 0.0, avy, avz, depth, width, perim, AxisDistanceMm: 0.0));

    // The section-column admission: a positive finite SI-millimetre magnitude into the kernel PositiveMagnitude, the
    // value-object's own Fin RE-LABELLED to the Section fault slot (a degenerate net column is a SECTION failure, not a
    // raw dimension input) so a structural consumer reads the true cause; AcceptValidated already enforces > 0 finite.
    static Fin<PositiveMagnitude> Section(double mm, Op key) =>
        key.AcceptValidated<PositiveMagnitude>(candidate: mm).MapFail(_ => ProfileFault.Section(key, $"<section-column-nonpositive:{mm:R}>"));

    // The section plane is VividOrange.Geometry's Y-Z: a centred rectangle is four LocalPoint2d corners, each cell a void
    // polyline; Perimeter(outerEdge, voidEdges) closes the polygons the integral iterates (the (ILocalPolyline2d, IList<ILocalPolyline2d>) ctor).
    static Perimeter RectanglePerimeter(double w, double d, Seq<(double X, double Y, double W, double H)> voids) =>
        new(Loop(-w / 2, -d / 2, w, d),
            voids.Map(v => Loop(v.X - v.W / 2, v.Y - v.H / 2, v.W, v.H)).ToList<ILocalPolyline2d>());

    static ILocalPolyline2d Loop(double y0, double z0, double w, double h) =>
        new LocalPolyline2d(new List<ILocalPoint2d> {
            new LocalPoint2d(Length.FromMillimeters(y0), Length.FromMillimeters(z0)),
            new LocalPoint2d(Length.FromMillimeters(y0 + w), Length.FromMillimeters(z0)),
            new LocalPoint2d(Length.FromMillimeters(y0 + w), Length.FromMillimeters(z0 + h)),
            new LocalPoint2d(Length.FromMillimeters(y0), Length.FromMillimeters(z0 + h)),
        });
}

// --- [TABLES] ------------------------------------------------------------------------------
// The ONE catalogue: the registered ProfileId -> Profile rows AND the registered ProfileId -> ComputedSection sections,
// each family contributing BOTH at build time (where its family section record — SteelShape/CmuShape/TimberShape — is
// in scope). The section map is the M7 substrate: ProfileResolution.Build joins the two by ProfileId, so a section is
// captured ONCE at the site that owns the family geometry, never reconstructed from a bare Profile (which has discarded
// the catalogue identity, the cell voids, the topology) — the deleted Func<Profile, Op, Fin<ComputedSection>> phantom.
public static class ProfileCatalogue {
    public static FrozenDictionary<ProfileId, Profile> Build(Context context) =>
        Masonry.ProfileCatalogue.BuildMasonryRows(context)
            .Concat(Steel.ProfileCatalogue.BuildSteelRows(context))
            .Concat(Cmu.ProfileCatalogue.BuildCmuRows(context))
            .Concat(Timber.ProfileCatalogue.BuildTimberRows(context))
            .Concat(Glazing.ProfileCatalogue.BuildGlazingRows(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value, ComparerAccessors.StringOrdinal.EqualityComparer);

    // The ProfileId -> ComputedSection table the M7 resolution caches off — folded from each STRUCTURAL family's own
    // section map (steel#STEEL_FAMILY SteelSections, cmu#CMU_FAMILY CmuSections, timber#TIMBER_FAMILY TimberSections), each
    // computed ONCE at family build over its family section record. Masonry (a unit course) and glazing (an IfcMaterialLayerSet)
    // contribute none — they are absent from this map, so ProfileResolution.Build joins their rows to Option.None, never a forged section.
    public static FrozenDictionary<ProfileId, ComputedSection> Sections(Context context) =>
        Steel.ProfileCatalogue.SteelSections
            .Concat(Cmu.ProfileCatalogue.CmuSections(context))
            .Concat(Timber.ProfileCatalogue.TimberSections(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value, ComparerAccessors.StringOrdinal.EqualityComparer);

    public static Fin<Profile> Lookup(FrozenDictionary<ProfileId, Profile> rows, ProfileId id, Op key) =>
        rows.TryGetValue(id, out Profile? row) && row is { } r ? Fin.Succ(r) : ProfileFault.Family(key, $"<unregistered-profile:{id.Value}>");
}
```

## [03]-[PROFILE_RESOLUTION]

- Owner: `ResolvedProfile` the one-hop `(Profile, Option<ComputedSection>)` receipt; `ProfileResolution` the seam-`ProfileRef` resolver and the frozen resolution cache the `[M7]` consumers read.
- Cases: one `ResolvedProfile` shape across all families — a `Profile` (the unit/family/standard) plus an `Option<ComputedSection>` (the seventeen-field stiffness receipt computed once through the family's section path, `Some` for a profiled structural member, `None` for a non-profiled one); a `ProfileRef` keys exactly one `ResolvedProfile`, never a per-family resolution receipt. A profile registered in the catalogue but ABSENT from the section map (a masonry unit, a glazing IGU — neither a profiled structural member) resolves to its `Profile` with `Option<ComputedSection>.None` — the seam-honest absence, NOT a fabricated all-zero section (a `PositiveMagnitude` rejects zero, so an all-zero `ComputedSection` is unrepresentable AND a forged-zero is the named anti-pattern), so the cache is total over every registered `ProfileRef` and a non-structural ref resolves without a fault. The `Option` aligns with the seam `MaterialComposition.ProfileSet(Material, Profile, Option<SectionProperties>)` the projector bakes: a `None` resolution bakes no section, a `Some` resolution bakes the lifted seam `SectionProperties`.
- Entry: `public static FrozenDictionary<ProfileRef, ResolvedProfile> Build(FrozenDictionary<ProfileId, Profile> rows, FrozenDictionary<ProfileId, ComputedSection> sections)` pre-resolves every catalogued profile to its `(Profile, Option<ComputedSection>)` once — keyed by the seam `ProfileRef.Of(profileId.Value)`, joining the two frozen maps the `ProfileCatalogue` already computed (a profiled member joins `Some(section)`, a non-profiled one `None`) — so resolution is an O(1) total join with no fault rail and no `Op` key (every row resolves; the integral ran once at catalogue build, in the family page that owns the geometry); `public static Fin<ResolvedProfile> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedProfile> table, Op key)` is the one-hop dereference the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.ProfileSet` consumer (`Profiles/capacity`, the `Rasm.Compute` structural route) calls, `Fin<T>` aborting on an unregistered ref (`ProfileFault.Family`, the registration slot — distinct from the `ProfileFault.Section` integral failure that already fell out at catalogue build).
- Packages: Rasm.Element (project — `ProfileRef` the seam handle the composition carries), Rasm (project — `Context`/`Op`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new resolvable attribute is one column on `ComputedSection` (a shear-centre offset, a monosymmetry parameter) the family section path fills; a new family's section participation is one entry in `ProfileCatalogue.Sections` (the family's own `ProfileId`→`ComputedSection` map), so a new family's section path is a catalogue contribution, never a resolver edit — the resolver owns the one-hop cache, the family pages own the section computation, and the `ProfileCatalogue` owns the join. There is NO `sectionOf` delegate: the section is data captured at build, not a function re-invoked at resolution.
- Boundary: `ProfileResolution` is the `[M7]` ONE-HOP — the seam `MaterialComposition.ProfileSet` carries a `ProfileRef` (the catalogue key the `Construction/assembly#MATERIAL_COMPOSITION` author wrapped from a `ProfileId`), and a structural consumer dereferences it ONCE through `Resolve` to the `(Profile, Option<ComputedSection>)` receipt rather than re-running the `ParametricSection` Green's-theorem integral or the steel `SectionReader` catalogue lookup per design-check call; the `Build` cache is the frozen pre-resolution every catalogued profile passes through, the section captured at the SAME catalogue-build site the family geometry lives (steel through its `SteelSections` map keyed by the `American`/`European` identity, the parametric families through their `CmuSections`/`TimberSections` maps over the built perimeter), so a steel W-shape and a glulam rectangle both carry their section into the one cache WITHOUT the resolver ever seeing a bare `Profile` it cannot re-derive a section from — the deleted `Func<Profile, Op, Fin<ComputedSection>>` phantom (a `Profile` retains only its `ProfileUnit`, having discarded the catalogue identity / cell voids / topology a section integral needs, so a `Profile`-keyed `sectionOf` was unrealizable for steel and lossy for the parametric rectangle); the resolver owns NO section math (the family pages + `ParametricSection` own it) and NO seam type (`ProfileRef` is the seam's) — it owns the one-hop dereference and the frozen cache. `Build` joins by membership in the section map: a `ProfileId` present in BOTH maps carries `Some(section)`; a `ProfileId` present only in the row map (a masonry/glazing non-structural profile) carries `None` so the cache is total and a non-structural `ProfileRef` resolves without a fault — the named defects being a structural consumer re-resolving a `ProfileRef` per call, a forged all-zero `ComputedSection` for a non-profiled ref (unrepresentable — a `PositiveMagnitude` rejects zero), and the silent-drop a `Choose`-over-`sectionOf` that vanished a structural profile whose integral failed at build (now a build-time `ParametricSection.Admit` `ProfileFault.Section` the family `BuildXRows` `Choose` surfaces in its own page, never a swallowed gap the resolver mis-reports as "unregistered").

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                   // Op, Context
using Rasm.Element;                  // ProfileRef, SectionProperties (the seam handle + neutral receipt the MaterialComposition.ProfileSet case carries)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Profiles;

// --- [MODELS] ------------------------------------------------------------------------------
// The one-hop resolution receipt: a ProfileRef dereferences to its Profile AND its OPTIONAL computed section in one
// lookup, so a structural consumer reads the section without re-running the section integral per call (M7). Section is
// Some(canonical ComputedSection) for a profiled structural family (steel/cmu/timber), None for a non-profiled one
// (masonry course / glazing IfcMaterialLayerSet) — the seam-honest absence, never a forged all-zero section (a
// PositiveMagnitude rejects zero, so an all-zero ComputedSection is UNREPRESENTABLE). The Option mirrors the seam
// MaterialComposition.ProfileSet(Material, Profile, Option<SectionProperties>) the Projection/material projector bakes.
public readonly record struct ResolvedProfile(Profile Profile, Option<ComputedSection> Section);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileResolution {
    // Pre-resolves every catalogued profile to its (Profile, Option<ComputedSection>) once, keyed by the seam ProfileRef
    // the composition carries; the frozen table is the M7 one-hop — resolution is an O(1) lookup, the section a build-time
    // JOIN of the two frozen catalogue maps (rows + sections), NOT a Func re-invoked here. A profile with a section entry
    // (steel/cmu/timber) joins Some(section); one with none (masonry/glazing) joins None so the cache is total over every
    // registered ProfileRef. No section integral runs at resolution — it ran once at catalogue build in the owning family.
    public static FrozenDictionary<ProfileRef, ResolvedProfile> Build(FrozenDictionary<ProfileId, Profile> rows, FrozenDictionary<ProfileId, ComputedSection> sections) =>
        rows.ToFrozenDictionary(
            static kv => ProfileRef.Of(kv.Key.Value),
            kv => new ResolvedProfile(kv.Value, sections.TryGetValue(kv.Key, out ComputedSection s) ? Some(s) : Option<ComputedSection>.None));

    public static Fin<ResolvedProfile> Resolve(ProfileRef reference, FrozenDictionary<ProfileRef, ResolvedProfile> table, Op key) =>
        table.TryGetValue(reference, out ResolvedProfile resolved)
            ? Fin.Succ(resolved)
            : ProfileFault.Family(key, $"<unresolved-profile-ref:{reference.Value}>");
}
```

## [04]-[RESEARCH]

- [STRUCTURAL_FAMILY_VOCABULARY]: REALIZED — all five families carry their own sibling page and `BuildXRows` builder: `masonry#PROFILE_FAMILY` (void-class/bond/orientation, the us/uk/din/au/is regional rows), `steel#STEEL_FAMILY` (the AISC Shapes Database v16.0 + EN 10365 catalogued sections), `cmu#CMU_FAMILY` (the ASTM C90 cell/face-shell rows), `timber#TIMBER_FAMILY` (the EN 14080/APA PRG 320 sawn/glulam/CLT rows), `glazing#GLAZING_FAMILY` (the IGU pane/spacer/cavity rows). Each is one `ProfileFamily` case with its own `ProfileUnit` projection, folded into the one `ProfileCatalogue.Build`. Which families contribute a profiled section (steel/cmu/timber) versus the unit-course masonry and the layer-set glazing is the per-profile `ResolvedProfile.Section` `Option` a consumer reads (`Some`/`None`), not a redundant family flag, so `ProfileCatalogue.Sections` folds exactly the structural maps the three families expose. The remaining per-family depth is pure row data additions (the remaining AISC sections, ASTM nominal widths, EN strength classes, IGU builds), each one row never a new type; the `ProfileFamily` axis and the one `Profile` owner are settled.
- [CANONICAL_COMPUTEDSECTION]: REALIZED — `ComputedSection` is the ONE seventeen-field section-property receipt the whole family axis shares (`Area`/`IxMm4`/`IyMm4`/`SxMm3`/`SyMm3`/`RxMm`/`RyMm`/`ZxMm3`/`ZyMm3`/`JMm4`/`IwMm6`/`AvyMm2`/`AvzMm2`/`DepthMm`/`WidthMm`/`HeatedPerimeterMm`/`AxisDistanceMm`), filled IDENTICALLY whether the section is a parametric rectangle/hollow (`ParametricSection` over a built `VividOrange.Profiles.Perimeter`) or a catalogued steel shape (`steel#STEEL_FAMILY` `SectionReader` over the catalogued `IProfile` + `SteelStiffness` for the warping/shear/plastic columns) — a per-family `W·D²/6` literal is the deleted form, and a parallel narrow per-family section receipt (the deleted `SteelSection`) is doubly deleted. The elastic columns + `HeatedPerimeter` come from the ONE `VividOrange.Sections.SectionProperties` polygon integral (`Area`/`MomentOfInertiaYy,Zz`/`ElasticSectionModulusYy,Zz`/`RadiusOfGyrationYy,Zz`/`Perimeter`); the plastic moduli (`ZxMm3`/`ZyMm3` = `b·h²/4` solid, gross-minus-void hollow), the St-Venant torsion (`JMm4`, the Roark Table 10.1 rectangle constant), and the shear areas (`AvyMm2`/`AvzMm2` = net area) the solver does not expose are the closed forms here for the rectangle families; the `IwMm6` warping column the parametric families leave engineering-zero is the column the `steel#STEEL_FAMILY` OPEN-topology shape FILLS. Every column admits once through the kernel `PositiveMagnitude` rail, a degenerate net section railing `ProfileFault.Section` (the section slot, distinct from a dimension input or a registration mismatch) so a corrupt stiffness never reaches the seam. Ripple counterpart: `steel#STEEL_FAMILY` (the catalogued shape filling the warping/shear columns through `SteelStiffness`), `cmu#CMU_FAMILY` + `timber#TIMBER_FAMILY` (the parametric families seeding `ParametricSection.Hollow`/`Rectangle`).
- [PROFILE_RESOLUTION_AS_JOIN]: REALIZED — the `[03]-[PROFILE_RESOLUTION]` `ProfileResolution.Build` resolves a `ProfileRef` to its `(Profile, Option<ComputedSection>)` by JOINING the two frozen catalogue maps the `ProfileCatalogue` already computed (`ProfileId`→`Profile` and `ProfileId`→`ComputedSection`), so the section is DATA captured once at the catalogue-build site where the family geometry lives — NOT a `Func<Profile, Op, Fin<ComputedSection>>` delegate re-invoked at resolution. The delegate form was the named defect: a `Profile` carries only its `ProfileUnit` (it has discarded the `American`/`European` catalogue identity a steel section integral needs, the cell-void geometry a cmu net section needs, the topology a steel warping column needs), so a `Profile`-keyed `sectionOf` was UNREALIZABLE for steel and LOSSY for the parametric rectangle (the catalogue's filleted/exact-corner geometry cannot be reconstructed from a `ProfileUnit` width/depth). Each structural family instead exposes its own build-time `ProfileId`→`ComputedSection` map (`steel#STEEL_FAMILY` `SteelSections`, `cmu#CMU_FAMILY` `CmuSections`, `timber#TIMBER_FAMILY` `TimberSections`), `ProfileCatalogue.Sections` folds them, and `Build` joins by `ProfileId` — an O(1) frozen lookup, zero re-integration, zero swallowed failure (a structural section that fails its integral falls out at the family `BuildXRows` `Choose` with a `ProfileFault.Section`, surfaced in its own page, never silently vanished from the resolution cache and mis-reported as "unregistered"). A non-structural profile (masonry/glazing) is absent from the section map and joins `Option<ComputedSection>.None` — the seam-honest absence, never a forged all-zero `ComputedSection` (a `PositiveMagnitude` rejects zero) — so the cache is total over every registered `ProfileRef`. Ripple counterpart: `steel#STEEL_FAMILY` (the `SteelSections` map keyed by `ProfileId` ALREADY realized, the realized `SteelSectionOf(ProfileId, Op)` resolver the prior `profile.md` mis-typed as a `Func<Profile, …>`), `cmu#CMU_FAMILY` + `timber#TIMBER_FAMILY` (each must expose a `CmuSections`/`TimberSections` `FrozenDictionary<ProfileId, ComputedSection>` over its build sequence, the sibling to steel's `SteelSections`).
- [IFCPROFILEDEF_ALIGNMENT]: `IfcProfileDef` is the canonical parameterized-cross-section model the BIM wire serializes; the `Profile` owner aligns its column set to the `IfcProfileDef` parameterized-profile subtypes per family, the column-to-subtype mapping landed on each family page. Masonry and cmu are the `IfcRectangleProfileDef` rectangle (the outer face the `XDim`/`YDim`, the cmu cell voids the `Coring` void fraction rather than a per-cell void profile — `cmu#IFCPROFILEDEF_CMU_ALIGNMENT`); steel carries the full subtype axis on its `SteelClass` (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef` — `steel#STEEL_FAMILY`); timber is the `IfcRectangleProfileDef` rectangle with the CLT cross-ply carried in the layer set (`timber#IFCPROFILEDEF_TIMBER_ALIGNMENT`); glazing is the one family that crosses as an `IfcMaterialLayerSet` rather than a profile, the IGU pane/cavity stack the `glazing#GLAZING_FAMILY` `GlazingSection.ToLayerSet` bridge resolves. A steel/timber member's cross-section round-trips to IFC 4.3 as an `IfcMaterialProfileSet` (the seam `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition.ProfileSet(ProfileRef)` shape, the `ProfileRef` the `[03]-[PROFILE_RESOLUTION]` resolves), a wall/IGU as an `IfcMaterialLayerSet`; the per-family `IfcMaterialProfileSetUsage` cardinal-point/orientation is the `[C7]` `ProfileSetUsage` the seam `Relations/relation#EDGE_ALGEBRA` `Associate` edge carries (the `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.UsageOf` derivation), authored onto IFC at the `Rasm.Bim` boundary.
- [STANDARDS_VOCABULARY]: REALIZED — `ProfileStandard` upgrades its free `Authority` string to the bounded `ProfileAuthority` `[SmartEnum]` (the published standards-body row — ASTM/AISC/AISI/EN/BS/DIN/AS/IS/APA, each carrying its `Region` token and `Parse` admission), so a consumer dispatches on `standard.Authority` (and reads `standard.Authority.Region` for the body's home region) rather than string-matching `"ASTM C216"` — a standards body is a row, never a free string parsed at a call site, and a new body is one `ProfileAuthority` row never a parallel "standard" enum. The reshape is DELIBERATELY MINIMAL: `Region` stays an explicit `ProfileStandard` token (a masonry `din`/`au`/`is` regional row carries a region the single authority does not name) and `StandardJointThicknessMm` keeps its name (the `Construction/layout#ASSEMBLY_FOLD` `JointPolicy` and the `cmu#CMU_FAMILY` builder read it), so ONLY the `Authority` field type breaks across the five sibling builders — the coordinating joint thickness survives as masonry/cmu coursing provenance (the steel/timber/glazing families pass `0.0` because a rolled/sawn/IGU profile has no mortar joint), the buildable joint detail being the `masonry#PROFILE_FAMILY` `MortarJoint` (head/bed width + ASTM C270 profile + strength) the masonry layout owns. Ripple counterpart: the five family builders (`masonry`/`steel`/`cmu`/`timber`/`glazing` `BuildXRows`) each construct `ProfileStandard` with a `ProfileAuthority` row in place of the prior free authority string.
