# [MATERIALS_PROFILE]

THE POLYMORPHIC PROFILE OWNER and THE FAMILY GROWTH AXIS. One `Profile` is the canonical cross-section concept every architectural material parameterizes — a unit shape (dimensions, coring, regional standard, source receipt) plus the family vocabulary its layout assembly reads; one `ProfileFamily` `[SmartEnum<string>]` closes the family-kind axis (masonry · cmu · steel · timber · glazing), all five realized as sibling cross-section vocabularies the one `ProfileCatalogue.Build` folds. A profile is NEVER a per-material class: a brick is a `Profile` in the `masonry` family, a steel section is a `Profile` in the `steel` family, a CMU in `cmu`, a glulam in `timber`, an IGU in `glazing`, differing only in unit columns and the family discriminant — never a `BrickProfile`/`SteelProfile` type. The owner is `IfcProfileDef`-aligned (the canonical parameterized-cross-section model the BIM wire serializes). The OOP capsule lives at the boundary (the `[ValueObject]` dimensional columns, the `[ValidationError]` dimensional validators), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` course projection). The page composes the `Rasm` kernel for the unit/dimension value-objects and `construction/layout#ASSEMBLY_FOLD` for the layout that turns a `Profile` into a placement stream; the appearance assignment is the `appearance/graph#MATERIAL_LIBRARY` `MaterialId` row a `Profile` maps to, never a profile-specific appearance type. Each family vocabulary lives on its own sibling page (`masonry`/`steel`/`cmu`/`timber`/`glazing`); this page owns the one `Profile` shape and the closed family axis.

## [1]-[INDEX]

One cluster: `[2]-[PROFILE_OWNER]` owns the `Profile` canonical unit shape, the `ProfileFamily` family-kind axis, the `ProfileId` key, the `ProfileFault` band, the dimensional value-objects, and the `ProfileCatalogue` registered-row table.

## [2]-[PROFILE_OWNER]

- Owner: `Profile` over the closed `ProfileFamily` axis; `ProfileId` key; `ProfileFault` `[Union]` band 2300; `ProfileKeyPolicy` ordinal accessor; `ProfileCatalogue` the registered-row table.
- Cases: one `Profile` shape across all families — `Family` (the discriminant), `Unit` (the dimensional cross-section), `Coring` (the void-class row), `Standard` (the regional source receipt), `AppearanceId` (the `MaterialId` row); family {masonry (realized), cmu, steel, timber, glazing (growth)}; a family is a `ProfileFamily` ROW, never a profile subtype.
- Entry: `public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key)` — `Fin<T>` aborts on a non-positive dimension (`ProfileFault.Dimension`, key-correlated), a void-fraction outside `[0,1)` (`ProfileFault.Coring`), or a family/unit mismatch (`ProfileFault.Family`); `ProfileCatalogue.Build(context)` folds every realized family's row builder (masonry seed plus `steel#STEEL_FAMILY` `BuildSteelRows`) into the one frozen registry, `ProfileCatalogue.Lookup(rows, id, key)` resolves a registered `ProfileId` to its catalogue `Profile`, and the same `Of` admits an ad-hoc unit through the row validation a registered row passes — one polymorphic entry, never a `GetById`/`GetByFamily` family.
- Packages: Rasm (project — `Dimension`/`UnitInterval`/`PositiveMagnitude` value-objects), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new architectural cross-section is one `Profile` row in the matching `ProfileFamily`; a new family is one `ProfileFamily` case carrying its unit vocabulary on its own sibling page folded into `ProfileCatalogue.Build`; a new fault is one `ProfileFault` case — never a `BrickProfile`/`SteelSection` type, never a per-family `Profile` variant. All five families are realized, each its own `ProfileFamily` case with its own `ProfileUnit` projection and `BuildXRows` catalogue builder: `masonry` carries the void-class/bond/orientation algebra and the regional rows, `steel` the section-property columns from the AISC Shapes Database v16.0, `cmu` the ASTM C90 cell/face-shell columns, `timber` the EN 14080/APA PRG 320 sawn/glulam/CLT lamella/grade columns, `glazing` the IGU pane/spacer/cavity columns — a new section in any family is a row data addition, never a new surface.
- Boundary: `Profile` is the ONE cross-section concept — a per-material class is the deleted form; `ProfileUnit` composes the `Rasm` kernel `PositiveMagnitude` value-object (the double-backed `> 0` finite magnitude) for every length column (width/height/length and the coursing module) so the profile never re-mints a dimension primitive and a fractional millimeter — an AISC web thickness, a metric joint module — admits without the truncation an int-backed `Dimension` count would force; `ProfileFault` is the one fault every `Fin.Fail` reads (dimension/coring/family/bond slots), an `Expected`-derived `Error` (`IValidationError<ProfileFault>`) whose 2300 band IS the `Expected` `Code` so a bare typed case lifts directly into the `Fin<T>` rail, so a layout never throws and never returns a sentinel; the `[ValidationError]` dimensional validators are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` course projection is the FP-ROP internal; the appearance assignment crosses to `appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `Profile.AppearanceId` column carries, never a profile-specific `SurfaceShade`; the layout that turns a `Profile` into a placement stream is `construction/layout#ASSEMBLY_FOLD`, composed not re-derived here; the masonry family vocabulary (`Coring`/`BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape`) lives on `masonry#PROFILE_FAMILY`, and the `ProfileCatalogue.Build` registered-row table folds every realized family's `BuildXRows` (masonry seed plus `steel#STEEL_FAMILY` `BuildSteelRows`) into one frozen registry so a registered row keys the same way as each cmu/timber/glazing family lands its own builder.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
[KeyMemberComparer<ProfileKeyPolicy, string>]
public readonly partial struct ProfileId {
    static partial void NormalizeAndValidate(ref string value, ref ValidationError? validationError) =>
        validationError = string.IsNullOrWhiteSpace(value) || !value.Contains('.') ? new ValidationError("<profile-id requires 'family.name'>") : null;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
[KeyMemberComparer<ProfileKeyPolicy, string>]
public sealed partial class ProfileFamily {
    public static readonly ProfileFamily Masonry = new("masonry");
    public static readonly ProfileFamily Cmu = new("cmu");
    public static readonly ProfileFamily Steel = new("steel");
    public static readonly ProfileFamily Timber = new("timber");
    public static readonly ProfileFamily Glazing = new("glazing");
}

// --- [ERRORS] ------------------------------------------------------------------------------
[Union]
public abstract partial record ProfileFault : Expected, IValidationError<ProfileFault> {
    private ProfileFault(Op key, string detail) : base(detail, 2300, None) => Key = key;
    public Op Key { get; }
    public static ProfileFault Create(string message) => new Family(default, message);
    public sealed record Dimension(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Dimension"; }
    public sealed record Coring(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Coring"; }
    public sealed record Family(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Family"; }
    public sealed record Bond(Op Key, string Detail) : ProfileFault(Key, Detail) { public override string Category => "Bond"; }
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class ProfileKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ProfileUnit(PositiveMagnitude WidthMm, PositiveMagnitude HeightMm, PositiveMagnitude LengthMm, PositiveMagnitude CourseHeightMm) {
    public double LengthOverWidth => LengthMm.Value / WidthMm.Value;
    public static Fin<ProfileUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: widthMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: heightMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: lengthMm)
        from c in key.AcceptValidated<PositiveMagnitude>(candidate: courseHeightMm)
        select new ProfileUnit(w, h, l, c);
}

public readonly record struct ProfileStandard(string Region, double StandardJointThicknessMm, string Authority);

public sealed record Profile(
    ProfileFamily Family,
    ProfileUnit Unit,
    Coring Coring,
    ProfileStandard Standard,
    MaterialId AppearanceId) {

    public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key) =>
        from valid in guard(coring.VoidFraction is >= 0.0 and < 1.0, ProfileFault.Coring(key, $"<void-fraction-out-of-range:{coring.VoidFraction}>"))
        select new Profile(family, unit, coring, standard, appearanceId);
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    public static FrozenDictionary<ProfileId, Profile> Build(Context context) =>
        Masonry.ProfileCatalogue.BuildMasonryRows(context)
            .Concat(Steel.ProfileCatalogue.BuildSteelRows(context))
            .Concat(Cmu.ProfileCatalogue.BuildCmuRows(context))
            .Concat(Timber.ProfileCatalogue.BuildTimberRows(context))
            .Concat(Glazing.ProfileCatalogue.BuildGlazingRows(context))
            .ToFrozenDictionary(static r => r.Key, static r => r.Value, ProfileKeyPolicy.EqualityComparer);

    public static Fin<Profile> Lookup(FrozenDictionary<ProfileId, Profile> rows, ProfileId id, Op key) =>
        rows.TryGetValue(id, out Profile? row) ? Fin.Succ(row!) : Fin.Fail<Profile>(ProfileFault.Family(key, $"<unregistered-profile:{id.Value}>"));
}
```

## [3]-[RESEARCH]

- [STRUCTURAL_FAMILY_VOCABULARY]: REALIZED — all five families carry their own sibling page and `BuildXRows` builder: `masonry#PROFILE_FAMILY` (void-class/bond/orientation, the us/uk/din/au/is regional rows), `steel#STEEL_FAMILY` (the AISC Shapes Database v16.0 W-shape seed), `cmu#CMU_FAMILY` (the ASTM C90 cell/face-shell rows), `timber#TIMBER_FAMILY` (the EN 14080/APA PRG 320 sawn/glulam/CLT rows), `glazing#GLAZING_FAMILY` (the IGU pane/spacer/cavity rows). Each is one `ProfileFamily` case with its own `ProfileUnit` projection, folded into the one `ProfileCatalogue.Build`. The remaining per-family depth is pure row data additions (the remaining AISC sections, ASTM nominal widths, EN strength classes, IGU builds), each one row never a new type; the `ProfileFamily` axis and the one `Profile` owner are settled.
- [IFCPROFILEDEF_ALIGNMENT]: `IfcProfileDef` is the canonical parameterized-cross-section model the BIM wire serializes; the `Profile` owner aligns its column set to the `IfcProfileDef` parameterized-profile subtypes per family, the column-to-subtype mapping landed on each family page. Masonry and cmu are the `IfcRectangleProfileDef` rectangle (the outer face the `XDim`/`YDim`, the cmu cell voids the `Coring` void fraction rather than a per-cell void profile — VERIFIED `cmu#IFCPROFILEDEF_CMU_ALIGNMENT`); steel carries the full subtype axis on its `SteelClass` (`IfcIShapeProfileDef`/`IfcUShapeProfileDef`/`IfcLShapeProfileDef`/`IfcRectangleHollowProfileDef`/`IfcCircleHollowProfileDef`/`IfcTShapeProfileDef` — VERIFIED `steel#STEEL_FAMILY`); timber is the `IfcRectangleProfileDef` rectangle with the CLT cross-ply carried in the lamella columns and the material-profile-set layers (VERIFIED `timber#IFCPROFILEDEF_TIMBER_ALIGNMENT`); glazing is the one family that crosses as an `IfcMaterialLayerSet` rather than a profile, the IGU pane/cavity stack the `GlazingSection.ToLayerSet` bridge resolves (VERIFIED `glazing#IFCPROFILEDEF_GLAZING_ALIGNMENT`). A steel/timber member's cross-section round-trips to IFC 4.3 as an `IfcMaterialProfileSet` (the `construction/assembly#MATERIAL_ASSIGNMENT` `ProfileSet` shape), a wall/IGU as an `IfcMaterialLayerSet`; the remaining probe is the per-family `IfcMaterialProfileSetUsage` cardinal-point/orientation mapping at the `Rasm.Bim` boundary.
