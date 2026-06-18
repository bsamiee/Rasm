# [MATERIALS_MASONRY]

THE FIRST REALIZED PROFILEFAMILY. The masonry family vocabulary — the void-class / bond / orientation / cut / closure / special-shape algebra and the regional `ProfileCatalogue` rows — is the realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Masonry` case. A masonry unit is a `Profile` row, never a `Brick` type: the coring void class, the bond course template, the unit orientation, and the regional standard are masonry-`Profile` columns, and the bond/orientation algebra is the data the `construction/layout#ASSEMBLY_FOLD` course fold reads. The masonry vocabulary grows by data — a new bond is one `BondName` row, a new orientation one `Orientation` case, a new special shape one `SpecialShape` entry — never a per-bond layout method. A `BondKind.Generated` bond is structurally distinct from a template bond: the generated interpreter is the named probe, and until it lands a generated bond rails `ProfileFault.Bond` rather than silently producing an empty course. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape and the `Rasm` kernel `Dimension` value-object; a cmu/steel/timber/glazing family lands its own sibling vocabulary on its own page.

## [1]-[INDEX]

One cluster: `[2]-[PROFILE_FAMILY]` owns the masonry unit/coring/bond/orientation/cut/closure/special-shape vocabulary, the `BondName` template/generated catalogue, and the `ProfileCatalogue.BuildMasonryRows` regional row table.

## [2]-[PROFILE_FAMILY]

- Owner: the masonry unit vocabulary (`Coring`, `CoringClass`, `BondName`, `BondKind`, `Orientation`, `Cut`, `ClosureRule`, `SpecialShape`); `CourseTemplate` the bond course shape; `ProfileCatalogue.BuildMasonryRows` the registered-row seed `profile#PROFILE_OWNER` composes.
- Cases: coring {solid/cored/perforated/hollow void classes} · bond {template + generated kinds} · orientation {stretcher/header/soldier/sailor/rowlock/shiner} · cut {whole/three-quarter/half-bat/quarter-bat/queen-closer/king-closer/bevel} · closure {none/queen-closer/king-closer/half-bat} · special-shape {none/bullnose/cownose/plinth/coping/cant/squint/voussoir}.
- Entry: `public Fin<CourseTemplate> Course(int index, Op key)` on `BondName` and `public bool Fits(Profile profile)` the `[BoundaryAdapter]` bond-fit check — the masonry bond reads its course template by wrapped index; a `BondKind.Generated` bond rails `ProfileFault.Bond` until the generated-bond interpreter algebra lands.
- Packages: Rasm (project — `Dimension`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the masonry vocabulary grows by data: a new bond is one `BondName` row, a new orientation one `Orientation` case, a new special shape one `SpecialShape.Catalog` entry — never a per-bond layout method. A sibling `ProfileFamily` (cmu/steel/timber/glazing) lands its own vocabulary on its own page the way masonry carries `Coring`/`BondName`/`Orientation`; the named cost is stated at `profile#STRUCTURAL_FAMILY_VOCABULARY` and queued in `TASKLOG.md`.
- Boundary: the masonry vocabulary is the realized first `ProfileFamily` — a per-material masonry class is the deleted form; `BondName.Course` is the OOP capsule reading the template course set, `BondName.Fits` the `[BoundaryAdapter]` aspect-ratio gate, both at the edge; the course-placement projection (the station/elevation fold) is `construction/layout#ASSEMBLY_FOLD`, composed not re-shown here; a `BondKind.Generated` bond is structurally distinct — the generated interpreter is the named probe, and until it lands a generated bond rails `ProfileFault.Bond`; `ProfileCatalogue.BuildMasonryRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the regional masonry `Profile` rows (us/uk/din/au/is), the `ProfileUnit` width/height/length/coursing columns composing the kernel `Dimension` value-object so the masonry unit never re-mints a length primitive.

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

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    public static FrozenDictionary<ProfileId, Profile> BuildMasonryRows() => FrozenDictionary<ProfileId, Profile>.Empty;
}
```

## [3]-[RESEARCH]

- [GENERATED_BOND_INTERPRETER]: the typed generated-bond interpreter algebra `BondName` owns — `BondKind.Generated` rows (herringbone/basket-weave/pinwheel/diaper/quetta) carry no template course set and require a geometric interpreter that derives course placement from the bond geometry rather than a wrapped template index. Until it lands, a generated bond rails `ProfileFault.Bond`; the probe is the interpreter, not a re-architecture of the catalogue.
- [MASONRY_ROW_TRANSCRIPTION]: the regional masonry cross-section catalogue (the us/uk/din/au/is regional rows) seeds through `ProfileCatalogue.BuildMasonryRows`, the dimension/coursing/aspect-ratio value-objects re-expressed as kernel-`Dimension`-composed `ProfileUnit` columns and the `BondName`/`Orientation`/`Cut`/`ClosureRule`/`SpecialShape` algebra realized as masonry rows; the transcription seeds the table the `construction/layout#ASSEMBLY_FOLD` course fold consumes.
