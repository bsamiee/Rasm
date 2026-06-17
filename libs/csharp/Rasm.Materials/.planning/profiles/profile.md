# [MATERIALS_PROFILE]

THE POLYMORPHIC PROFILE OWNER and THE FAMILY GROWTH AXIS. One `Profile` is the canonical cross-section concept every architectural material parameterizes — a unit shape (dimensions, coring, regional standard, source receipt) plus the family vocabulary its layout assembly reads; one `ProfileFamily` `[SmartEnum<string>]` closes the family-kind axis (masonry · cmu · steel · timber · glazing), masonry realized first as the cross-section catalogue and the four siblings the named growth cost. A profile is NEVER a per-material class: a brick is a `Profile` in the `masonry` family, a steel section is a `Profile` in the `steel` family, differing only in unit columns and the family discriminant — never a `BrickProfile`/`SteelProfile` type. The owner is `IfcProfileDef`-aligned (the canonical parameterized-cross-section model the BIM wire serializes). The OOP capsule lives at the boundary (the `[ValueObject]` dimensional columns, the `[ValidationError]` dimensional validators), the FP-ROP rail owns the internals (the `Fin<T>` admission, the `Seq`/`Fold` course projection). The page composes the `Rasm` kernel for the unit/dimension value-objects and `construction/layout#ASSEMBLY_FOLD` for the layout that turns a `Profile` into a placement stream; the appearance assignment is the `appearance/graph#MATERIAL_LIBRARY` `MaterialId` row a `Profile` maps to, never a profile-specific appearance type. The realized masonry family vocabulary lives on `masonry`; this page owns the one `Profile` shape and the closed family axis.

## [1]-[INDEX]

One cluster: `[2]-[PROFILE_OWNER]` owns the `Profile` canonical unit shape, the `ProfileFamily` family-kind axis, the `ProfileId` key, the `ProfileFault` band, the dimensional value-objects, and the `ProfileCatalogue` registered-row table.

## [2]-[PROFILE_OWNER]

- Owner: `Profile` over the closed `ProfileFamily` axis; `ProfileId` key; `ProfileFault` `[Union]` band 2300; `ProfileKeyPolicy` ordinal accessor; `ProfileCatalogue` the registered-row table.
- Cases: one `Profile` shape across all families — `Family` (the discriminant), `Unit` (the dimensional cross-section), `Coring` (the void-class row), `Standard` (the regional source receipt), `AppearanceId` (the `MaterialId` row); family {masonry (realized), cmu, steel, timber, glazing (growth)}; a family is a `ProfileFamily` ROW, never a profile subtype.
- Entry: `public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, MaterialId appearanceId, Op key)` — `Fin<T>` aborts on a non-positive dimension (`ProfileFault.Dimension`, key-correlated), a void-fraction outside `[0,1)` (`ProfileFault.Coring`), or a family/unit mismatch (`ProfileFault.Family`); `ProfileCatalogue.Lookup` resolves a registered `ProfileId` to its catalogue `Profile`, and the same `Of` admits an ad-hoc unit through the row validation a registered row passes — one polymorphic entry, never a `GetById`/`GetByFamily` family.
- Packages: Rasm (project — `Dimension`/`UnitInterval`/`PositiveMagnitude` value-objects), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: a new architectural cross-section is one `Profile` row in the matching `ProfileFamily`; a new family is one `ProfileFamily` case carrying its unit vocabulary (masonry realized, cmu/steel/timber/glazing the named cost depth-filled on a sibling page); a new fault is one `ProfileFault` case — never a `BrickProfile`/`SteelSection` type, never a per-family `Profile` variant. The four un-realized families are the growth axis named on the card Growth line and queued in `TASKLOG.md`: `cmu` adds the cell/face-shell unit columns, `steel` the section-property columns (depth/flange/web/fillet) from the AISC Shapes Database v16.0, `timber` the sawn/glulam/CLT lamella/grade columns, `glazing` the IGU pane/spacer/frame columns — each its own `ProfileFamily` case with its own `ProfileUnit` projection the way masonry carries its vocabulary.
- Boundary: `Profile` is the ONE cross-section concept — a per-material class is the deleted form; `ProfileUnit` composes the `Rasm` kernel `Dimension` value-object for every length column (width/height/length and the coursing module) so the profile never re-mints a dimension primitive; `ProfileFault` (band 2300) is the one fault every `Fin.Fail` reads (dimension/coring/family/bond slots) so a layout never throws and never returns a sentinel; the `[ValidationError]` dimensional validators are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` course projection is the FP-ROP internal; the appearance assignment crosses to `appearance/graph#MATERIAL_LIBRARY` as a `MaterialId` row a `Profile.AppearanceId` column carries, never a profile-specific `SurfaceShade`; the layout that turns a `Profile` into a placement stream is `construction/layout#ASSEMBLY_FOLD`, composed not re-derived here; the masonry family vocabulary (`Coring`/`BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape`) lives on `masonry#PROFILE_FAMILY`, and the `ProfileCatalogue` registered-row table composes the realized family's `BuildRows` so a registered row keys the same way once a steel/timber/glazing family lands.

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
public abstract partial record ProfileFault {
    private ProfileFault() { }
    public sealed record Dimension(string Detail) : ProfileFault;
    public sealed record Coring(string Detail) : ProfileFault;
    public sealed record Family(string Detail) : ProfileFault;
    public sealed record Bond(string Detail) : ProfileFault;
    public int Band => 2300;
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class ProfileKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct ProfileUnit(Dimension WidthMm, Dimension HeightMm, Dimension LengthMm, Dimension CourseHeightMm) {
    public double LengthOverWidth => LengthMm.Value / WidthMm.Value;
    public static Fin<ProfileUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Context context, Op key) =>
        from w in Dimension.Of(widthMm, context, key)
        from h in Dimension.Of(heightMm, context, key)
        from l in Dimension.Of(lengthMm, context, key)
        from c in Dimension.Of(courseHeightMm, context, key)
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
    public static readonly FrozenDictionary<ProfileId, Profile> Rows = Masonry.ProfileCatalogue.BuildMasonryRows();

    public static Fin<Profile> Lookup(ProfileId id, Op key) =>
        Rows.TryGetValue(id, out Profile? row) ? Fin.Succ(row!) : Fin.Fail<Profile>(ProfileFault.Family(key, $"<unregistered-profile:{id.Value}>"));
}
```

## [3]-[RESEARCH]

- [STRUCTURAL_FAMILY_VOCABULARY]: the cmu/steel/timber/glazing families are the named growth axis — each lands its own unit vocabulary as source-backed data. Steel rows source from the AISC Shapes Database v16.0 (free, all standard steel shapes 1873-2016) with depth/flange/web/fillet/Ix/Sx columns; timber rows are standard sawn/glulam/CLT property tables; cmu rows are cell/face-shell tables; glazing rows are IGU pane/spacer/frame tables. Each is one `ProfileFamily` case with its own `ProfileUnit` projection the way masonry carries `Coring`/`BondName`/`Orientation`. The probe is the per-family vocabulary depth-fill (one sibling page per family), queued in `TASKLOG.md`; the `ProfileFamily` axis and the one `Profile` owner are settled.
- [IFCPROFILEDEF_ALIGNMENT]: `IfcProfileDef` is the canonical parameterized-cross-section model the BIM wire serializes; the `Profile` owner aligns its column set to the `IfcProfileDef` parameterized-profile subtypes (I-shape/U-shape/L-shape/rectangle/circle/arbitrary-closed) so a steel/timber member's cross-section round-trips to IFC 4.3 as an `IfcMaterialProfileSet` (the `construction/assembly#MATERIAL_ASSIGNMENT` ProfileSet shape). The probe is the column-to-subtype mapping per family; the masonry family is a rectangle profile, the realized base case.
