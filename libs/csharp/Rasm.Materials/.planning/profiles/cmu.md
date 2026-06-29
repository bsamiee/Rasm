# [MATERIALS_CMU]

THE CMU PROFILEFAMILY. The concrete-masonry-unit cross-section vocabulary — the ASTM C90 cell / face-shell / web dimensional columns (face-shell thickness, web thickness, cell count, the nominal-to-actual width/height/length module) and the hollow/solid grade discriminant — is a realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Cmu` case. A concrete block is a `Profile` row, never a `ConcreteBlock` type: the cell geometry, the face-shell/web thickness columns, the void class, and the regional standard are cmu-`Profile` columns, and the `CmuSection` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the masonry family drives — a CMU run is the same station-stepped course fold over one `Profile`, never a per-family layout. The cmu vocabulary grows by data — a new unit is one `CmuRow` catalogue row, a new grade one `CmuGrade` case — never a per-block type. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape, `masonry#PROFILE_FAMILY` `Coring`/`BondName`/`Orientation` for the course algebra, and the `Rasm` kernel `PositiveMagnitude` for every length column; timber/glazing land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[CMU_FAMILY]: the `CmuGrade` solid/hollow discriminant, the `CmuSection` cell/face-shell record, the `CmuSection.ToUnit` projection that flows a CMU through the masonry-shaped `Resolve` fold, and the `ProfileCatalogue.BuildCmuRows` ASTM C90 row table.

## [02]-[CMU_FAMILY]

- Owner: the cmu unit vocabulary (`CmuGrade` the solid/hollow discriminant, `CmuSection` the ASTM C90 cell/face-shell record); `ProfileCatalogue.BuildCmuRows` the registered-row seed `profile#PROFILE_OWNER` composes; the `CmuSection.ToUnit` projection bridging a section to the canonical `ProfileUnit`.
- Cases: grade {hollow-load-bearing, hollow-non-load-bearing, solid-load-bearing} — the ASTM C90 unit-grade set; a section is a `CmuSection` row over one `CmuGrade`, never a section subtype.
- Entry: `public Fin<ProfileUnit> ToUnit(Context context, Op key)` on `CmuSection` — the section→`ProfileUnit` projection (`WidthMm` = actual unit width, `HeightMm`/`CourseHeightMm` = actual unit height plus the standard bed joint, `LengthMm` = actual unit length) so a CMU member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public Fin<ComputedSection> NetSection(Op key)` the EXACT net cross-section — the `profile#PROFILE_OWNER` `ParametricSection.Hollow` solver over the cell voids returning net Area AND net inertia, the design seam's true net section; `public double SolidFraction()` the quick net-area ratio the `Coring` bucket reads, and `public Coring ToCoring()` the masonry void-class bridge.
- Packages: Rasm (project — `PositiveMagnitude` for every fractional-millimeter section column), VividOrange.Sections.SectionProperties (via the shared `profile#PROFILE_OWNER` `ParametricSection` bridge — the hollow `Perimeter` net-section integral; `.api/api-vividorange-sections-sectionproperties.md`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the cmu vocabulary grows by data — a new ASTM C90 unit is one `CmuRow` catalogue row keyed by its nominal designation, a new grade one `CmuGrade` case — never a per-block type, never a per-family `Profile` variant. A timber/glazing family lands its own vocabulary on its own page the way cmu carries `CmuGrade`/`CmuSection`.
- Boundary: the cmu vocabulary is a realized `ProfileFamily` — a per-block class is the deleted form; `CmuSection` composes the `Rasm` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive, the ASTM C90 minimum face-shell (19/25/32 mm by nominal width) and 19 mm web thickness admitting as fractional millimeters; `CmuSection.ToUnit` is the ONE bridge from the cell/face-shell vocabulary to the canonical `ProfileUnit` the `Resolve` fold consumes, and `CmuSection.ToCoring` maps the net-area solid fraction to the `masonry#PROFILE_FAMILY` `Coring` void class (a hollow two-cell unit reads `Coring.Hollow2Cell`) so the cmu unit shares the masonry course algebra; a CMU run is a `LayerSet`/`ProfileSet` assignment along the `RunPath` extruded through one `Profile`, never a masonry-special-case; the `CmuGrade` carries the load-bearing/non-load-bearing discriminant the design seam reads and the `IfcRectangleProfileDef` rectangle subtype the cmu maps to on the wire (a hollow unit's cell voids cross as the `Coring` void fraction, the wire profile the outer rectangle); `ProfileCatalogue.BuildCmuRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the ASTM C90 rows keyed `cmu.<designation>`, the realized cross-section grounded in the published ASTM C90 dimensional values.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CmuGrade {
    public static readonly CmuGrade HollowLoadBearing    = new("hollow-load-bearing", loadBearing: true,  hollow: true);
    public static readonly CmuGrade HollowNonLoadBearing = new("hollow-non-load-bearing", loadBearing: false, hollow: true);
    public static readonly CmuGrade SolidLoadBearing     = new("solid-load-bearing", loadBearing: true,  hollow: false);
    public bool LoadBearing { get; }
    public bool Hollow { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct CmuSection(
    CmuGrade Grade,
    PositiveMagnitude ActualWidthMm,
    PositiveMagnitude ActualHeightMm,
    PositiveMagnitude ActualLengthMm,
    PositiveMagnitude FaceShellMm,
    PositiveMagnitude WebThicknessMm,
    int CellCount,
    double BedJointMm) {

    public double GrossAreaMm2 => ActualWidthMm.Value * ActualLengthMm.Value;
    public double NetAreaMm2 => Grade.Hollow
        ? GrossAreaMm2 - Math.Max(0, CellCount) * CellAreaMm2()
        : GrossAreaMm2;
    public double SolidFraction() => GrossAreaMm2 > 0.0 ? Math.Clamp(NetAreaMm2 / GrossAreaMm2, 0.0, 1.0) : 1.0;

    private double CellAreaMm2() {
        double cellWidth = ActualWidthMm.Value - 2.0 * FaceShellMm.Value;
        double webSpan = Math.Max(0, CellCount + 1) * WebThicknessMm.Value;
        double cellLength = (ActualLengthMm.Value - webSpan) / Math.Max(1, CellCount);
        return Math.Max(0.0, cellWidth) * Math.Max(0.0, cellLength);
    }

    // The cell voids in the horizontal L×W cross-section: CellCount equal cells separated by webs, each cell inset by
    // the face shell across the width and centred along the length, the void list profile#PROFILE_OWNER ParametricSection
    // subtracts from the outer rectangle for the EXACT net section.
    private Seq<(double X, double Y, double W, double H)> CellVoids() {
        if (!Grade.Hollow || CellCount <= 0) { return Seq<(double, double, double, double)>(); }
        double cellWid = Math.Max(0.0, ActualWidthMm.Value - 2.0 * FaceShellMm.Value);
        double cellLen = Math.Max(0.0, (ActualLengthMm.Value - (CellCount + 1) * WebThicknessMm.Value) / CellCount);
        double pitch = cellLen + WebThicknessMm.Value;
        return toSeq(Enumerable.Range(0, CellCount))
            .Map(i => (X: -ActualLengthMm.Value / 2 + WebThicknessMm.Value + cellLen / 2 + i * pitch, Y: 0.0, W: cellLen, H: cellWid));
    }

    public Coring ToCoring() => SolidFraction() switch {
        >= 0.95 => Coring.None,
        >= 0.70 => Coring.Cored3Hole,
        >= 0.55 => Coring.Perforated10Cell,
        _       => Coring.Hollow2Cell,
    };

    // The EXACT net section the structural design seam reads — net Area AND net moment-of-inertia with the cells
    // subtracted from the second moment, not the gross-minus-cell-area scalar SolidFraction approximates; the ONE
    // profile#PROFILE_OWNER ParametricSection solver runs over the built hollow Perimeter, never a per-cell literal.
    public Fin<ComputedSection> NetSection(Op key) =>
        ParametricSection.Hollow(ActualLengthMm.Value, ActualWidthMm.Value, CellVoids(), key);

    public Fin<ProfileUnit> ToUnit(Context context, Op key) =>
        ProfileUnit.Of(ActualWidthMm.Value, ActualHeightMm.Value, ActualLengthMm.Value, ActualHeightMm.Value + BedJointMm, context, key);
}

public readonly record struct CmuRow(string Designation, double WMm, double HMm, double LMm, double FaceShellMm, double WebMm, int Cells, bool Hollow);

public sealed record CmuShape(ProfileId Id, CmuSection Section, ProfileStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    static readonly ProfileStandard AstmC90 = new("us", StandardJointThicknessMm: 9.5, Authority: "ASTM C90");

    static readonly Seq<CmuRow> AstmRows = Seq(
        new CmuRow("cmu.4in-hollow",   90.0, 190.0, 390.0, 19.0, 19.0, 2, true),
        new CmuRow("cmu.6in-hollow",  140.0, 190.0, 390.0, 25.0, 19.0, 2, true),
        new CmuRow("cmu.8in-hollow",  190.0, 190.0, 390.0, 32.0, 19.0, 2, true),
        new CmuRow("cmu.10in-hollow", 240.0, 190.0, 390.0, 32.0, 19.0, 2, true),
        new CmuRow("cmu.12in-hollow", 290.0, 190.0, 390.0, 32.0, 19.0, 3, true),
        new CmuRow("cmu.4in-solid",    90.0, 190.0, 390.0, 45.0, 19.0, 0, false),
        new CmuRow("cmu.8in-solid",   190.0, 190.0, 390.0, 95.0, 19.0, 0, false),
        new CmuRow("cmu.8in-splitface", 190.0, 190.0, 390.0, 32.0, 19.0, 2, true),
        new CmuRow("cmu.8in-halfhigh", 190.0, 90.0, 390.0, 32.0, 19.0, 2, true));

    static Fin<CmuShape> CmuOf(CmuRow r, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WMm)
        from h in key.AcceptValidated<PositiveMagnitude>(candidate: r.HMm)
        from l in key.AcceptValidated<PositiveMagnitude>(candidate: r.LMm)
        from fs in key.AcceptValidated<PositiveMagnitude>(candidate: r.FaceShellMm)
        from web in key.AcceptValidated<PositiveMagnitude>(candidate: r.WebMm)
        let grade = r.Hollow ? CmuGrade.HollowLoadBearing : CmuGrade.SolidLoadBearing
        select new CmuShape(ProfileId.Of(r.Designation), new CmuSection(grade, w, h, l, fs, web, r.Cells, AstmC90.StandardJointThicknessMm), AstmC90);

    public static FrozenDictionary<ProfileId, Profile> BuildCmuRows(Context context) =>
        AstmRows
            .Choose(row => CmuOf(row, context, default).ToOption())
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Cmu, unit, shape.Section.ToCoring(), shape.Standard, MaterialId.Of("ceramic.porcelain")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [CMU_ROW_TRANSCRIPTION]: REALIZED — the ASTM C90 standard carries the hollow/solid load-bearing concrete-masonry-unit grades with the actual 190×190×390 mm 8-inch module, the minimum face-shell thickness by nominal width (19 mm at 4 in, 25 mm at 6 in, 32 mm at 8 in and above), and the 19 mm minimum web thickness; the catalogue carries the 4/6/8/10/12-inch hollow nominal widths, the 4/8-inch solid grades, the 190-mm splitface architectural unit, and the 90-mm half-high unit, the full standard nominal-width set keyed `cmu.<designation>`, a new architectural finish (ground-face, scored, ribbed) or metric A-series unit one further `CmuRow` data addition, never a new type. The raw `CmuRow` carries plain doubles and admits once through `CmuOf` into the kernel value-objects so the catalogue seed validates every column, a non-positive dimension dropping the row through `Choose` rather than seeding a degenerate `Profile`.
- [IFCPROFILEDEF_CMU_ALIGNMENT]: the cmu family is the `IfcRectangleProfileDef` rectangle subtype on the `IfcProfileDef` wire — the outer 190×390 mm face is the `XDim`/`YDim` rectangle and the hollow-unit cell voids cross as the `Coring` void fraction the `ToCoring` net-area bridge derives, never a per-cell `IfcArbitraryProfileDefWithVoids` here; a CMU member round-trips to IFC 4.3 as an `IfcMaterialProfileSet` carrying the rectangle profile plus the `Coring` receipt, the splitface/ground-face finish a surface-style on the element rather than a profile variant. The probe is the per-finish surface-style mapping at the `Rasm.Bim` boundary, the rectangle profile the realized base case.
- [CMU_CELL_GEOMETRY]: REALIZED — the exact net section is the `CmuSection.NetSection` projection composing the shared `profile#PROFILE_OWNER` `ParametricSection.Hollow` solver over the `CellVoids` cell rectangles (the `VividOrange.Sections.SectionProperties` Green's-theorem integral subtracting the cells from BOTH the area AND the second moment), so the structural design seam reads the true net `Area`/`MomentOfInertiaYy`/`Zz` rather than the gross-minus-cell-area scalar the `SolidFraction` approximation yields — the `NetAreaMm2`/`CellAreaMm2` fast path survives only as the coarse `Coring`-bucket ratio the total `ToCoring` reads. The cell voids are computed from the face-shell/web/cell-count columns (`CellVoids`: `CellCount` equal cells inset by the face shell, separated by webs, centred along the length), never a per-cell profile literal; a precise per-cell draft taper is a `CellVoids` column growth, never a parallel section owner. The hollow-unit cell voids cross the IFC wire as the rectangle outer profile plus the `Coring` void fraction, the exact net properties the design seam's own concern.
