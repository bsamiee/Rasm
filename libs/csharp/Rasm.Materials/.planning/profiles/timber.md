# [MATERIALS_TIMBER]

THE TIMBER PROFILEFAMILY. The timber cross-section vocabulary — the sawn/glulam/CLT product columns (the rectangular section width/depth, the lamella thickness and count, the EN 14080 / APA PRG 320 strength-grade designation, the grain orientation) and the product-form discriminant — is a realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Timber` case. A glulam beam is a `Profile` row, never a `GlulamBeam` type: the section dimensions, the lamella columns, the grade receipt, and the product form are timber-`Profile` columns, and the `TimberSection` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the masonry and steel families drive — a timber member extrudes through one `Profile` over the `RunPath`, never a per-family layout. The timber vocabulary grows by data — a new section is one `TimberRow` catalogue row, a new product form one `TimberForm` case — never a per-member type. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape and the `Rasm` kernel `PositiveMagnitude` for every length column; cmu/glazing land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[TIMBER_FAMILY]: the `TimberForm` sawn/glulam/CLT discriminant, the `TimberGrade` strength-class axis, the `TimberSection` lamella/section record, the `TimberSection.ToUnit` projection, and the `ProfileCatalogue.BuildTimberRows` row table.

## [02]-[TIMBER_FAMILY]

- Owner: the timber unit vocabulary (`TimberForm` the product-form discriminant, `TimberGrade` the EN 14080/APA PRG 320 strength class, `TimberSection` the lamella/section record); `ProfileCatalogue.BuildTimberRows` the registered-row seed `profile#PROFILE_OWNER` composes; the `TimberSection.ToUnit` projection bridging a section to the canonical `ProfileUnit`.
- Cases: form {sawn (solid-dimension lumber), glulam (glue-laminated lamellae), clt (cross-laminated plies)} — the timber product-form set; grade {gl24h, gl28h, gl32h, c24, c30} — the EN strength classes; a section is a `TimberSection` row over one `TimberForm`/`TimberGrade`, never a section subtype.
- Entry: `public Fin<ProfileUnit> ToUnit(Context context, Op key)` on `TimberSection` — the section→`ProfileUnit` projection (`WidthMm` = section width, `HeightMm`/`CourseHeightMm` = section depth, `LengthMm` = the lamella module a glulam/CLT layup repeats, or the section depth for sawn) so a timber member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public int LamellaCount()` the `[BoundaryAdapter]` ply/lamella count the layup seam reads, and `public double SectionModulusMm3()` the rectangular elastic section modulus the design seam reads.
- Packages: Rasm (project — `PositiveMagnitude` for every fractional-millimeter section column), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the timber vocabulary grows by data — a new section is one `TimberRow` catalogue row keyed by its product designation, a new product form one `TimberForm` case carrying its `IfcProfileDef` subtype mapping, a new strength class one `TimberGrade` row — never a per-member type, never a per-family `Profile` variant. A cmu/glazing family lands its own vocabulary on its own page the way timber carries `TimberForm`/`TimberSection`; the named cost is stated at `profile#STRUCTURAL_FAMILY_VOCABULARY`.
- Boundary: the timber vocabulary is a realized `ProfileFamily` — a per-member class is the deleted form; `TimberSection` composes the `Rasm` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive, the EN/APA standard lamella thickness (38–45 mm glulam, 20–40 mm CLT ply) and the sawn nominal-to-actual (38×89 mm) admitting as fractional millimeters; `TimberSection.ToUnit` is the ONE bridge from the lamella/section vocabulary to the canonical `ProfileUnit` the `Resolve` fold consumes — a timber run is the single-orientation station-stepped fold so a beam/column/panel is a `ProfileSet` extrusion (`assembly#MATERIAL_ASSIGNMENT` `ProfileSet`) along the `RunPath`, never a masonry-style multi-unit course; `TimberForm` maps each product to its `IfcProfileDef` rectangle subtype (`IfcRectangleProfileDef` for a sawn/glulam section, the CLT panel a rectangle with the cross-ply layup carried in the lamella columns) so a timber member round-trips to IFC 4.3 as an `IfcMaterialProfileSet`; the `TimberGrade` carries the characteristic strength/stiffness receipt (the `Fmk` bending strength, the `E0Mean` mean modulus) the structural-design seam reads, never raw doubles; `ProfileCatalogue.BuildTimberRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the rows keyed `timber.<designation>`, the realized cross-section grounded in the published EN 14080 (glulam) and APA PRG 320 (CLT) section tables.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class TimberForm {
    public static readonly TimberForm Sawn   = new("sawn", ifcSubtype: "IfcRectangleProfileDef", crossPly: false);
    public static readonly TimberForm Glulam = new("glulam", ifcSubtype: "IfcRectangleProfileDef", crossPly: false);
    public static readonly TimberForm Clt    = new("clt", ifcSubtype: "IfcRectangleProfileDef", crossPly: true);
    public string IfcSubtype { get; }
    public bool CrossPly { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ProfileKeyPolicy, string>]
public sealed partial class TimberGrade {
    public static readonly TimberGrade Gl24h = new("gl24h", fmkMpa: 24.0, e0MeanMpa: 11_500.0);
    public static readonly TimberGrade Gl28h = new("gl28h", fmkMpa: 28.0, e0MeanMpa: 12_500.0);
    public static readonly TimberGrade Gl32h = new("gl32h", fmkMpa: 32.0, e0MeanMpa: 14_200.0);
    public static readonly TimberGrade C24   = new("c24", fmkMpa: 24.0, e0MeanMpa: 11_000.0);
    public static readonly TimberGrade C30   = new("c30", fmkMpa: 30.0, e0MeanMpa: 12_000.0);
    public double FmkMpa { get; }
    public double E0MeanMpa { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct TimberSection(
    TimberForm Form,
    TimberGrade Grade,
    PositiveMagnitude WidthMm,
    PositiveMagnitude DepthMm,
    PositiveMagnitude LamellaThicknessMm,
    int Lamellae) {

    public int LamellaCount() => Math.Max(1, Lamellae);
    public double SectionModulusMm3() => WidthMm.Value * DepthMm.Value * DepthMm.Value / 6.0;
    public double MomentOfInertiaMm4() => WidthMm.Value * DepthMm.Value * DepthMm.Value * DepthMm.Value / 12.0;

    public Fin<ProfileUnit> ToUnit(Context context, Op key) =>
        ProfileUnit.Of(WidthMm.Value, DepthMm.Value, Form.CrossPly ? LamellaThicknessMm.Value : DepthMm.Value, DepthMm.Value, context, key);
}

public readonly record struct TimberRow(string Designation, string Form, string Grade, double WMm, double DMm, double LamellaMm, int Lamellae);

public sealed record TimberShape(ProfileId Id, TimberSection Section, ProfileStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    static readonly ProfileStandard En14080 = new("eu", StandardJointThicknessMm: 0.0, Authority: "EN 14080 / APA PRG 320");

    static readonly Seq<TimberRow> TimberRows = Seq(
        new TimberRow("timber.sawn-c24-38x89",      "sawn",   "c24",   38.0,  89.0,  89.0,  1),
        new TimberRow("timber.sawn-c24-38x140",     "sawn",   "c24",   38.0,  140.0, 140.0, 1),
        new TimberRow("timber.sawn-c24-38x184",     "sawn",   "c24",   38.0,  184.0, 184.0, 1),
        new TimberRow("timber.sawn-c30-63x175",     "sawn",   "c30",   63.0,  175.0, 175.0, 1),
        new TimberRow("timber.glulam-gl24h-90x225", "glulam", "gl24h", 90.0,  225.0, 45.0,  5),
        new TimberRow("timber.glulam-gl28h-90x270", "glulam", "gl28h", 90.0,  270.0, 45.0,  6),
        new TimberRow("timber.glulam-gl32h-115x405","glulam", "gl32h", 115.0, 405.0, 45.0,  9),
        new TimberRow("timber.clt-c24-3ply-90",     "clt",    "c24",   1250.0,  90.0, 30.0, 3),
        new TimberRow("timber.clt-c24-5ply-150",    "clt",    "c24",   1250.0, 150.0, 30.0, 5),
        new TimberRow("timber.clt-c24-7ply-230",    "clt",    "c24",   1250.0, 230.0, 33.0, 7));

    static Fin<TimberShape> TimberOf(TimberRow r, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WMm)
        from d in key.AcceptValidated<PositiveMagnitude>(candidate: r.DMm)
        from lam in key.AcceptValidated<PositiveMagnitude>(candidate: r.LamellaMm)
        from form in TimberForm.TryGet(r.Form, out TimberForm? f) ? Fin.Succ(f!) : Fin.Fail<TimberForm>(ProfileFault.Family(key, $"<unknown-timber-form:{r.Form}>"))
        from grade in TimberGrade.TryGet(r.Grade, out TimberGrade? g) ? Fin.Succ(g!) : Fin.Fail<TimberGrade>(ProfileFault.Family(key, $"<unknown-timber-grade:{r.Grade}>"))
        select new TimberShape(ProfileId.Of(r.Designation), new TimberSection(form, grade, w, d, lam, r.Lamellae), En14080);

    public static FrozenDictionary<ProfileId, Profile> BuildTimberRows(Context context) =>
        TimberRows
            .Choose(row => TimberOf(row, context, default).ToOption())
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Timber, unit, Coring.None, shape.Standard, MaterialId.Of("wood.oak")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ProfileKeyPolicy.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [TIMBER_ROW_TRANSCRIPTION]: REALIZED — EN 14080 carries the glulam strength classes (GL24h/GL28h/GL32h homogeneous, the combined `c` variants) with the standard 40–45 mm lamella thickness, EN 338 the C-class structural-sawn grades, and APA PRG 320 the CLT layup grades with the 3/5/7-ply cross-laminated lamellae at 20–40 mm; the catalogue carries the C24/C30 sawn sizes (38×89 through 63×175), the GL24h/GL28h/GL32h glulam sections, and the 3/5/7-ply CLT panels, the standard structural set keyed `timber.<designation>`, the remaining EN strength classes and the LVL/PSL/glulam-curved engineered products one further `TimberRow` data addition, each one row, never a new type. The CLT panel width is the full panel module the layup repeats, the sawn/glulam width the rectangular section breadth, the lamella thickness/count carrying the layup the gamma-method composite stiffness reads.
- [IFCPROFILEDEF_TIMBER_ALIGNMENT]: every timber form maps to the `IfcRectangleProfileDef` rectangle subtype (`TimberForm.IfcSubtype` carries the spelling) — a sawn/glulam member is the `WidthMm`/`DepthMm` rectangle and a CLT panel the `WidthMm` panel module by `DepthMm` total thickness, the cross-ply layup carried in the `Lamellae`/`LamellaThicknessMm` columns and the material-profile-set layers rather than a profile variant; a timber member round-trips to IFC 4.3 as an `IfcMaterialProfileSet` whose profile is the rectangle and whose `IfcMaterialProfileSetUsage` carries the grade/lamella receipt, the curved/tapered glulam a `SweptArea` extrusion the host materializes from the rectangle profile along the `RunPath`, never a per-product profile subtype. The probe is the `IfcMaterialProfileSetUsage` cardinal-point/orientation mapping at the `Rasm.Bim` boundary.
- [TIMBER_LAYUP_GEOMETRY]: a CLT panel's cross-ply layup (alternating longitudinal/transverse plies) drives the effective-section composite stiffness the design seam computes from the lamella columns; the per-ply orientation and the rolling-shear modulus are `TimberSection` column growth, never a parallel section owner — the lamella thickness/count and the `TimberForm.CrossPly` discriminant already carry the layup receipt the `IfcRectangleProfileDef` rectangle wire and the gamma-method composite stiffness read. The CLT panel crosses the IFC wire as the rectangle outer profile, the layup carried in the material-profile-set layers.
