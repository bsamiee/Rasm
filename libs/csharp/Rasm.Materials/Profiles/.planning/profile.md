# [MATERIALS_PROFILE]

| [OWNER]   | [AXES]                                                                                                          | [STATE] | [DEPTH]                                                                 |
| --------- | ------------------------------------------------------------------------------------------------------------- | :-----: | ---------------------------------------------------------------------- |
| `profile` | `Profile` · `ProfileFamily` · `ProfileId` · `ProfileUnit` · `Coring` · `BondName` · `Orientation` · `ProfileFault` | SPIKE   | 5 family axis cases (masonry realized; CMU/steel/timber/glazing queued); 1 polymorphic owner; 2 fences |

[STATE] is SPIKE: the `Profile` polymorphic owner and the `ProfileFamily` growth axis are transcription-shaped — the masonry family (brick) carries its full unit/coring/bond/orientation vocabulary and the dimensional value-objects from the archived masonry source, the `ProfileFault` band is closed, and the OOP boundary capsule / FP-ROP internal split is fixed. The residual probe is the typed generated-bond interpreter algebra `BondName` owns (`BondKind.Generated` rows fail explicitly until it lands) and the arch/pier scalar-placement rules the masonry assembly fold consumes from `Construction/construction#ASSEMBLY_FOLD`; the four un-realized families (CMU/steel/timber/glazing) are the growth axis named on the card Growth line and queued in `TASKLOG.md`, not net-new fences here.

THE POLYMORPHIC PROFILE OWNER and THE FAMILY GROWTH AXIS. One `Profile` is the canonical cross-section concept every architectural material parameterizes — a unit shape (dimensions, coring, regional standard, source receipt) plus the bond/orientation vocabulary its layout assembly reads; one `ProfileFamily` `[SmartEnum<string>]` closes the family-kind axis (masonry · cmu · steel · timber · glazing), masonry realized first as the brick catalogue and the four siblings the named growth cost. A profile is NEVER a per-material class: a brick is a `Profile` in the `masonry` family, a steel section is a `Profile` in the `steel` family, differing only in unit columns and the family discriminant — never a `BrickProfile`/`SteelProfile` type. The OOP capsule lives at the boundary (the `[SmartEnum]` catalogue rows, the `[ValueObject]` dimensional columns, the `[BoundaryAdapter]` bond-fit check), the FP-ROP rail owns the internals (the `Fin<T>` layout admission, the `Seq`/`Fold` course projection, the `[Union]` orientation/closure algebra). The page composes the `Rasm` kernel for the unit/dimension value-objects and the `Construction/construction#ASSEMBLY_FOLD` for the layout that turns a `Profile` into a placement stream; the appearance assignment is the `Appearance/appearance-graph#MATERIAL_LIBRARY` `MaterialId` row a `Profile` maps to, never a profile-specific appearance type.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                                       |
| :-----: | --------------- | ------------------------------------------------------------------------------------------- |
|   [1]   | PROFILE_OWNER   | `Profile` canonical unit shape; `ProfileId` key; `ProfileFault` band; dimensional value-objects |
|   [2]   | PROFILE_FAMILY  | `ProfileFamily` family-kind axis; masonry unit/coring/bond/orientation vocabulary; the growth-axis seam |

## [2]-[PROFILE_OWNER]

- Owner: `Profile` over the closed `ProfileFamily` axis; `ProfileId` key; `ProfileFault` `[Union]` band 2300; `ProfileKeyPolicy` ordinal accessor.
- Cases: one `Profile` shape across all families — `Family` (the discriminant), `Unit` (the dimensional cross-section), `Coring` (the void-class row), `Standard` (the regional source receipt); a family is a `ProfileFamily` ROW, never a profile subtype.
- Entry: `public static Fin<Profile> Of(ProfileFamily family, ProfileUnit unit, Coring coring, ProfileStandard standard, Op key)` — `Fin<T>` aborts on a non-positive dimension (`ProfileFault.Dimension`, key-correlated), a void-fraction outside `[0,1)` (`ProfileFault.Coring`), or a family/unit mismatch (`ProfileFault.Family`); `Lookup` resolves a registered `ProfileId` to its catalogue `Profile`, and the same `Of` admits an ad-hoc unit through the row validation a registered row passes — one polymorphic entry, never a `GetById`/`GetByFamily` family.
- Packages: Rasm (project — `Dimension`/`UnitInterval`/`PositiveMagnitude` value-objects), Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Collections.Frozen.
- Growth: a new architectural cross-section is one `Profile` row in the matching `ProfileFamily`; a new family is one `ProfileFamily` case carrying its unit vocabulary (masonry realized, cmu/steel/timber/glazing the named cost); a new void class is one `Coring` row; a new fault is one `ProfileFault` case — never a `BrickProfile`/`SteelSection` type, never a per-family `Profile` variant. The masonry family deletes the archived `Rasm.Materials.Bricks` `BrickDesignation`/`Brick` split: the brick IS the masonry `Profile`, the `BrickDesignation` catalogue rows are masonry `Profile` rows, and the `Dim3`/`VerticalCoursing`/`AspectRatio` value-objects re-express as the kernel `Dimension`-composed `ProfileUnit` columns.
- Boundary: `Profile` is the ONE cross-section concept — a per-material class is the deleted form; `ProfileUnit` composes the `Rasm` kernel `Dimension` value-object for every length column (width/height/length and the coursing module) so the profile never re-mints a dimension primitive; `ProfileFault` (band 2300) is the one fault every `Fin.Fail` reads (dimension/coring/family/bond slots) so a layout never throws and never returns a sentinel; the `[BoundaryAdapter]` bond-fit check and the `[ValidationError]` dimensional validators are the OOP capsule at the edge, the `Fin`/`Seq`/`Fold` course projection is the FP-ROP internal; the appearance assignment crosses to `Appearance/appearance-graph#MATERIAL_LIBRARY` as a `MaterialId` row a `Profile.AppearanceId` column carries, never a profile-specific `SurfaceShade`; the layout that turns a `Profile` into a placement stream is `Construction/construction#ASSEMBLY_FOLD`, composed not re-derived here.

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
    public static readonly ProfileFamily Masonry = new("masonry");   // realized — the archived brick catalogue, the first family
    public static readonly ProfileFamily Cmu = new("cmu");           // growth — concrete masonry unit cells/face-shells
    public static readonly ProfileFamily Steel = new("steel");       // growth — hot-rolled/HSS section library
    public static readonly ProfileFamily Timber = new("timber");     // growth — sawn/glulam/CLT lamella sections
    public static readonly ProfileFamily Glazing = new("glazing");   // growth — IGU pane/spacer/frame profiles
}

// --- [ERRORS] ------------------------------------------------------------------------------
// ProfileFault is the band-2300 fault [Union] (one band below MaterialFault's 2400). The owner pins these exact
// smart-constructors so every call site threads the Op key:
//   public static Error Dimension(Op key, string detail);  // band-2300 non-positive / non-finite cross-section dimension
//   public static Error Coring(Op key, string detail);     // band-2300 void-fraction outside [0,1)
//   public static Error Family(Op key, string detail);     // band-2300 family/unit discriminant mismatch
//   public static Error Bond(Op key, string detail);       // band-2300 incompatible bond / generated-bond not yet interpretable
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
// ProfileUnit: the dimensional cross-section, every length column the kernel Dimension value-object so a unit never
// re-mints a length primitive. The archived Dim3/VerticalCoursing/AspectRatio collapse here: width/height/length are
// Dimension columns, the coursing module is a Dimension pair, the aspect ratio derives from the length/width columns.
public readonly record struct ProfileUnit(Dimension WidthMm, Dimension HeightMm, Dimension LengthMm, Dimension CourseHeightMm) {
    public double LengthOverWidth => LengthMm.Value / WidthMm.Value;
    public static Fin<ProfileUnit> Of(double widthMm, double heightMm, double lengthMm, double courseHeightMm, Context context, Op key) =>
        from w in Dimension.Of(widthMm, context, key)
        from h in Dimension.Of(heightMm, context, key)
        from l in Dimension.Of(lengthMm, context, key)
        from c in Dimension.Of(courseHeightMm, context, key)
        select new ProfileUnit(w, h, l, c);
}

// ProfileStandard: the regional source receipt — the standard authority and the coordinating joint thickness, carried
// as the typed evidence the archived BrickRegion/BrickSource pair held. One receipt shape across families.
public readonly record struct ProfileStandard(string Region, double StandardJointThicknessMm, string Authority);

// Profile: THE polymorphic cross-section. A brick is a masonry Profile, a steel section a steel Profile — one shape,
// the Family column the discriminant, the AppearanceId the Appearance/appearance-graph#MATERIAL_LIBRARY MaterialId
// the profile maps to (never a profile-specific appearance). A second Profile shape is the named defect.
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
```

## [3]-[PROFILE_FAMILY]

- Owner: `ProfileFamily` family-kind axis; the masonry unit vocabulary (`Coring`, `BondName`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`); `ProfileCatalogue` the registered-row table.
- Cases: family {masonry (realized), cmu, steel, timber, glazing (growth)}; masonry vocabulary — coring {solid/cored/perforated/hollow void classes} · bond {template + generated kinds} · orientation {stretcher/header/soldier/sailor/rowlock/shiner} · cut/closure {bat/closer/bevel algebra} · special-shape {bullnose/cownose/plinth/coping/cant/squint/voussoir/...}.
- Entry: `public Fin<CourseTemplate> Course(int index, Op key)` on `BondName` and `public bool Fits(Profile profile)` the `[BoundaryAdapter]` bond-fit check — the masonry bond reads its course template by wrapped index; a `BondKind.Generated` bond rails `ProfileFault.Bond` until the generated-bond interpreter algebra lands.
- Packages: Rasm (project — `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core, System.Collections.Frozen.
- Growth: the family axis grows by one `ProfileFamily` case per architectural material domain — `cmu` adds the cell/face-shell unit columns, `steel` the section-property columns (depth/flange/web/fillet), `timber` the lamella/grade columns, `glazing` the pane/spacer/frame columns; each lands its own unit vocabulary the way masonry carries `Coring`/`BondName`/`Orientation`, the named cost stated here and queued in `TASKLOG.md`. The masonry vocabulary itself grows by data: a new bond is one `BondName` row, a new orientation one `Orientation` case, a new special shape one `SpecialShape.Catalog` entry — never a per-bond layout method.
- Boundary: `ProfileFamily` is the ONE growth axis — a per-family `Profile` subtype is the deleted form; the masonry vocabulary is the archived `Rasm.Materials.Bricks` catalogue re-homed as masonry-family `Profile` rows and their bond/orientation algebra (the `Brick.cs` `[SmartEnum]`/`[Union]` owners), so the brick concept is preserved as the first family with zero capability loss; `BondName.Course` is the OOP capsule reading the template course set, `BondName.Fits` the `[BoundaryAdapter]` aspect-ratio gate, both at the edge; the course-placement projection (the archived `Layout.cs` station/elevation fold) is `Construction/construction#ASSEMBLY_FOLD`, composed not re-shown here; a `BondKind.Generated` bond is structurally distinct from a template bond — the generated interpreter is the SPIKE probe, and until it lands a generated bond rails `ProfileFault.Bond` rather than silently producing an empty course; the cmu/steel/timber/glazing families carry NO fence here — they are the named axis cost, depth-filled when each lands.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The masonry family vocabulary — the archived Brick.cs [SmartEnum]/[Union] catalogue re-homed as the first family's
// unit algebra. Coring is the void-class row, BondName the template/generated bond catalogue, Orientation/Cut/ClosureRule
// the placement algebra. These are masonry-Profile columns; a steel/timber/glazing family lands its own sibling vocabulary.
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
    public static readonly BondKind Template = new("template");     // course set drives placement directly
    public static readonly BondKind Generated = new("generated");   // requires the interpreter algebra (the SPIKE probe)
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

// BondName: the template/generated bond catalogue keyed by name; Course reads the wrapped template course, Fits gates by
// aspect ratio. A generated bond rails ProfileFault.Bond until the interpreter algebra lands. One bond catalogue, rows
// not types; the full archived row set (running/english/flemish/herringbone/...) re-homes here verbatim as masonry rows.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class BondName {
    public static readonly BondName Running = new("running", kind: BondKind.Template, courses: Seq(StretcherAt(0.0), StretcherAt(0.5)));
    public static readonly BondName English = new("english", kind: BondKind.Template, courses: Seq(StretcherAt(0.0), HeaderAt(0.5)));
    public static readonly BondName Herringbone45 = new("herringbone-45", kind: BondKind.Generated, courses: Seq<CourseTemplate>());
    public BondKind Kind { get; }
    public Seq<CourseTemplate> Courses { get; }

    [BoundaryAdapter]
    public bool Fits(Profile profile) => profile.Unit.LengthOverWidth > 0.0;

    public Fin<CourseTemplate> Course(int index, Op key) =>
        Kind == BondKind.Generated
            ? Fin.Fail<CourseTemplate>(ProfileFault.Bond(key, $"<generated-bond-not-interpretable:{Key}>"))
            : Fin.Succ(Courses[((index % Courses.Count) + Courses.Count) % Courses.Count]);

    static CourseTemplate StretcherAt(double offset) => new(Seq<Orientation>(new Orientation.Stretcher()), offset);
    static CourseTemplate HeaderAt(double offset) => new(Seq<Orientation>(new Orientation.Header()), offset);
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record CourseTemplate(Seq<Orientation> Sequence, double OffsetFraction);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ProfileCatalogue {
    // The registered-row table — masonry Profile rows keyed by ProfileId. The archived BrickDesignation catalogue
    // (us.modular/uk.standard/din.nf/...) re-homes as masonry-family Profile rows; a steel/timber/glazing row keys
    // the same way once its family lands. One table, rows not types; a per-family table is the deleted form.
    public static readonly FrozenDictionary<ProfileId, Profile> Rows = BuildMasonryRows();

    public static Fin<Profile> Lookup(ProfileId id, Op key) =>
        Rows.TryGetValue(id, out Profile? row) ? Fin.Succ(row!) : Fin.Fail<Profile>(ProfileFault.Family(key, $"<unregistered-profile:{id.Value}>"));

    static FrozenDictionary<ProfileId, Profile> BuildMasonryRows() => /* masonry catalogue rows, TASKLOG transcription */ FrozenDictionary<ProfileId, Profile>.Empty;
}
```

## [4]-[RESEARCH]

- [GENERATED_BOND_INTERPRETER]: the typed generated-bond interpreter algebra `BondName` owns — `BondKind.Generated` rows (herringbone/basket-weave/pinwheel/diaper/quetta) carry no template course set and require a geometric interpreter that derives course placement from the bond geometry rather than a wrapped template index. Until it lands, a generated bond rails `ProfileFault.Bond`; the probe is the interpreter, not a re-architecture of the catalogue.
- [FAMILY_DEPTH_FILL]: the cmu/steel/timber/glazing families are the named growth axis — each lands its own unit vocabulary (cmu cell/face-shell columns, steel section-property columns, timber lamella/grade columns, glazing pane/spacer/frame columns) the way masonry carries `Coring`/`BondName`/`Orientation`. The probe is the per-family vocabulary depth-fill, queued in `TASKLOG.md`; the `ProfileFamily` axis and the one `Profile` owner are settled.
- [MASONRY_ROW_TRANSCRIPTION]: the archived `Rasm.Materials.Bricks` `BrickDesignation` catalogue (the us/uk/din/au/is regional rows) re-homes as masonry-family `Profile` rows through `ProfileCatalogue.BuildMasonryRows`, the `Dim3`/`VerticalCoursing`/`AspectRatio` value-objects re-expressed as kernel-`Dimension`-composed `ProfileUnit` columns and the `BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape` algebra re-homed verbatim; the transcription seeds the table the layout assembly consumes.
