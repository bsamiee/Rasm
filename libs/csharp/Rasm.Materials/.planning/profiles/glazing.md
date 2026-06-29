# [MATERIALS_GLAZING]

THE GLAZING PROFILEFAMILY. The glazing cross-section vocabulary — the insulating-glass-unit pane / spacer / cavity columns (the per-pane glass thickness, the spacer width and warm/cold-edge type, the cavity gas fill, the overall unit thickness) and the unit-build discriminant — is a realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Glazing` case. A double-glazed unit is a `Profile` row, never a `DoubleGlazedUnit` type: the pane stack, the spacer columns, the cavity fill, and the regional standard are glazing-`Profile` columns, and the `GlazingSection` projection feeds the same `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` the IGU stack resolves to — a glazing unit is the IFC `IfcMaterialLayerSet` the layer-set assignment owner already models (pane / cavity / pane), never a per-family layout. The glazing vocabulary grows by data — a new unit is one `GlazingRow` catalogue row, a new build one `GlazingBuild` case — never a per-unit type. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard` shape, `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet`/`MaterialLayer` for the pane-cavity-pane buildup, and the `Rasm` kernel `PositiveMagnitude`/`Dimension` for every length column; cmu/timber land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[GLAZING_FAMILY]: the `GlazingBuild` double/triple discriminant, the `SpacerType` warm/cold-edge axis, the `CavityGas` EN 673 fill axis, the `GlazingSection` pane/spacer/cavity record, the `GlazingSection.ToUnit` projection and the `GlazingSection.ToLayerSet` IGU layer-set bridge, and the `ProfileCatalogue.BuildGlazingRows` row table.

## [02]-[GLAZING_FAMILY]

- Owner: the glazing unit vocabulary (`GlazingBuild` the double/triple discriminant, `SpacerType` the warm/cold-edge spacer axis, `GlazingSection` the pane/spacer/cavity record); `ProfileCatalogue.BuildGlazingRows` the registered-row seed `profile#PROFILE_OWNER` composes; the `GlazingSection.ToUnit` projection and the `GlazingSection.ToLayerSet` IGU layer-set bridge.
- Cases: build {double (pane-cavity-pane), triple (pane-cavity-pane-cavity-pane)} — the IGU unit-build set; spacer {warm-edge, cold-edge-aluminum} — the edge-seal type; gas {air, argon, krypton, xenon} — the closed `CavityGas` fill carrying its EN 673 conductivity; a section is a `GlazingSection` row over one `GlazingBuild`/`SpacerType`/`CavityGas`, never a section subtype.
- Entry: `public Fin<ProfileUnit> ToUnit(Context context, Op key)` on `GlazingSection` — the section→`ProfileUnit` projection (`WidthMm` = the overall unit thickness, `HeightMm`/`LengthMm`/`CourseHeightMm` = the standard frame-rebate module the host overrides) so a glazing unit flows through the same catalogue; `public Fin<MaterialComposition> ToLayerSet(Op key)` the IGU pane-cavity-pane seam `LayerSet` composition bridge (built through the `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor`), and `public double OverallThicknessMm()` the unit-build depth the frame seam reads.
- Packages: Rasm (project — `PositiveMagnitude` for the pane/spacer columns, `Dimension` for the layer thicknesses), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the glazing vocabulary grows by data — a new IGU is one `GlazingRow` catalogue row keyed by its build designation, a new unit build one `GlazingBuild` case, a new spacer one `SpacerType` row — never a per-unit type, never a per-family `Profile` variant. A cmu/timber family lands its own vocabulary on its own page the way glazing carries `GlazingBuild`/`GlazingSection`.
- Boundary: the glazing vocabulary is a realized `ProfileFamily` — a per-unit class is the deleted form; `GlazingSection` composes the `Rasm` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive, the standard 4 mm/6 mm pane glass and 12/16/20 mm spacer cavity admitting as fractional millimeters; `GlazingSection.ToLayerSet` is the ONE bridge from the pane/spacer vocabulary to the `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` — a double-glazed unit resolves to the three-layer `LayerSet` (glass pane, cavity gas fill, glass pane), a triple-glazed unit the five-layer set, each `MaterialLayer` carrying its `Dimension` thickness and a `MaterialId` (`glass.crown` for the panes, `gas.cavity` the transparent sealed-cavity appearance row the `Appearance/graph#MATERIAL_LIBRARY` owner carries for the argon/krypton/air fills, the gas species held in the layer label and the typed `CavityGas` thermal identity, never re-distinguished in appearance) so the IGU shares the IFC `IfcMaterialLayerSet` the layer-set assignment owner already models, never a glazing-special-case; the overall unit crosses the IFC wire as the `IfcMaterialLayerSet` the `assembly#MATERIAL_COMPOSITION` `LayerSet` serializes, the pane/cavity stack the layer rows; the `SpacerType` carries the edge-seal thermal-bridge discriminant the energy seam reads; `ProfileCatalogue.BuildGlazingRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the rows keyed `glazing.<designation>`, the realized cross-section grounded in the standard IGU build values.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GlazingBuild {
    public static readonly GlazingBuild Double = new("double", panes: 2, cavities: 1);
    public static readonly GlazingBuild Triple = new("triple", panes: 3, cavities: 2);
    public int Panes { get; }
    public int Cavities { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpacerType {
    public static readonly SpacerType WarmEdge        = new("warm-edge", psiWmK: 0.04);
    public static readonly SpacerType ColdEdgeAluminum = new("cold-edge-aluminum", psiWmK: 0.11);
    public double PsiWmK { get; }
}

// The cavity fill is a closed gas vocabulary, never a free string — each row carries its EN 673 thermal
// conductivity (W/m·K) so the IGU center-of-glass U-value reads a typed cavity receipt beside SpacerType.PsiWmK.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CavityGas {
    public static readonly CavityGas Air     = new("air",     conductivityWMK: 0.0241);
    public static readonly CavityGas Argon   = new("argon",   conductivityWMK: 0.0162);
    public static readonly CavityGas Krypton = new("krypton", conductivityWMK: 0.0090);
    public static readonly CavityGas Xenon   = new("xenon",   conductivityWMK: 0.0054);
    public double ConductivityWMK { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct GlazingSection(
    GlazingBuild Build,
    SpacerType Spacer,
    PositiveMagnitude PaneThicknessMm,
    PositiveMagnitude CavityWidthMm,
    CavityGas Gas) {

    public double OverallThicknessMm() => Build.Panes * PaneThicknessMm.Value + Build.Cavities * CavityWidthMm.Value;

    // EN 673 cavity conductance per metre of gas gap — the typed thermal receipt the energy seam reads, replacing
    // the implicit string fill; a wider cavity or a heavier gas lowers the conductance and the IGU U-value.
    public double CavityConductanceWM2K => Gas.ConductivityWMK / (CavityWidthMm.Value / 1000.0);

    public Fin<ProfileUnit> ToUnit(Context context, Op key) {
        double overall = OverallThicknessMm();
        return ProfileUnit.Of(overall, overall, overall, overall, context, key);
    }

    // The IGU pane-cavity-pane stack lowers to a seam MaterialComposition.LayerSet via the Construction/assembly
    // CompositionAuthor (the seam owns the composition + the MeasureValue thickness coercion); the alternating panes
    // and cavities tag glass.crown / gas.cavity material ids the appearance and Rasm.Bim sides read off the seam node.
    public Fin<MaterialComposition> ToLayerSet(Op key) =>
        CompositionAuthor.LayerSet(
            toSeq(Enumerable.Range(0, Build.Panes + Build.Cavities))
                .Map(i => (i & 1) == 0
                    ? (MaterialId.Of("glass.crown"), PaneThicknessMm.Value, $"pane-{i / 2}")
                    : (MaterialId.Of("gas.cavity"), CavityWidthMm.Value, $"cavity-{Gas.Key}-{i / 2}")),
            key);
}

public readonly record struct GlazingRow(string Designation, string Build, string Spacer, double PaneMm, double CavityMm, string Gas);

public sealed record GlazingShape(ProfileId Id, GlazingSection Section, ProfileStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    static readonly ProfileStandard IguStandard = new("eu", StandardJointThicknessMm: 0.0, Authority: "EN 1279");

    static readonly Seq<GlazingRow> GlazingRows = Seq(
        new GlazingRow("glazing.double-4-16-4",       "double", "warm-edge",          4.0, 16.0, "argon"),
        new GlazingRow("glazing.double-6-12-6",       "double", "warm-edge",          6.0, 12.0, "argon"),
        new GlazingRow("glazing.double-4-20-4",       "double", "warm-edge",          4.0, 20.0, "argon"),
        new GlazingRow("glazing.double-6-16-6-lowe",  "double", "warm-edge",          6.0, 16.0, "argon"),
        new GlazingRow("glazing.double-4-12-4-alu",   "double", "cold-edge-aluminum", 4.0, 12.0, "air"),
        new GlazingRow("glazing.double-66.2-16-4-lam","double", "warm-edge",          6.4, 16.0, "argon"),
        new GlazingRow("glazing.triple-4-16-4-16-4",  "triple", "warm-edge",          4.0, 16.0, "krypton"),
        new GlazingRow("glazing.triple-4-12-4-12-4",  "triple", "warm-edge",          4.0, 12.0, "argon"));

    static Fin<GlazingShape> GlazingOf(GlazingRow r, Context context, Op key) =>
        from pane in key.AcceptValidated<PositiveMagnitude>(candidate: r.PaneMm)
        from cavity in key.AcceptValidated<PositiveMagnitude>(candidate: r.CavityMm)
        from build in GlazingBuild.TryGet(r.Build, out GlazingBuild? b) ? Fin.Succ(b!) : Fin.Fail<GlazingBuild>(ProfileFault.Family(key, $"<unknown-glazing-build:{r.Build}>"))
        from spacer in SpacerType.TryGet(r.Spacer, out SpacerType? s) ? Fin.Succ(s!) : Fin.Fail<SpacerType>(ProfileFault.Family(key, $"<unknown-spacer:{r.Spacer}>"))
        from gas in CavityGas.TryGet(r.Gas, out CavityGas? g) ? Fin.Succ(g!) : Fin.Fail<CavityGas>(ProfileFault.Family(key, $"<unknown-cavity-gas:{r.Gas}>"))
        select new GlazingShape(ProfileId.Of(r.Designation), new GlazingSection(build, spacer, pane, cavity, gas), IguStandard);

    public static FrozenDictionary<ProfileId, Profile> BuildGlazingRows(Context context) =>
        GlazingRows
            .Choose(row => GlazingOf(row, context, default).ToOption())
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Glazing, unit, Coring.None, shape.Standard, MaterialId.Of("glass.crown")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [GLAZING_ROW_TRANSCRIPTION]: REALIZED — the standard EN 1279 IGU builds carry the 4 mm/6 mm pane glass, the 6.4 mm laminated (66.2) safety pane, the 12/16/20 mm cavity, and the double `4-16-4` (24 mm overall) / triple `4-16-4-16-4` configurations with argon/krypton/air gas fill plus the low-E coated double; the catalogue carries the double argon/air/low-E/laminated and triple argon/krypton builds keyed `glazing.<designation>`, the remaining pane thicknesses, low-E coating tiers, and electrochromic/vacuum (VIG) variants one further `GlazingRow` data addition, each one row, never a new type. The overall unit thickness is the pane-count times pane plus cavity-count times cavity, the `IfcMaterialLayerSet` pane-cavity-pane stack the wire shape.
- [IFCPROFILEDEF_GLAZING_ALIGNMENT]: glazing is the one family that is an `IfcMaterialLayerSet` rather than an `IfcProfileDef` profile — the `GlazingSection.ToLayerSet` bridge resolves the IGU to the `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` (pane / cavity / pane), so a glazing unit round-trips to IFC 4.3 as the `IfcMaterialLayerSet` the layer-set assignment owner serializes (`IfcMaterialLayer.LayerThickness` per pane/cavity, `IfcMaterialLayer.IsVentilated` false for the sealed cavity), the `SpacerType` edge-seal a `Pset_` thermal-bridge property on the element rather than a profile column, the laminated pane a two-`MaterialLayer` sub-stack within the pane thickness. The `ToUnit` rectangle projection serves only the catalogue keying and the frame-rebate module the host overrides; the wire shape is the layer set, not a rectangle profile. The probe is the per-coating `IfcSurfaceStyle`/low-E emissivity mapping at the `Rasm.Bim` boundary.
- [GLAZING_THERMAL_RECEIPT]: REALIZED — the cavity fill is the typed `CavityGas` `[SmartEnum<string>]` (air/argon/krypton/xenon) carrying its EN 673 `ConductivityWMK`, never a free string, so the IGU center-of-glass U-value reads a typed cavity receipt: `GlazingSection.CavityConductanceWM2K` is the per-gap gas conductance `λ_gas / gap` the energy seam composes with the `SpacerType.PsiWmK` edge-bridge and the pane resistances, the heavier gas (krypton/xenon) lowering the conductance the way a wider cavity does. The remaining low-E coating emissivity is `GlazingSection` column growth, never a parallel section owner. The cavity gas crosses the appearance wire as the `gas.cavity` `MaterialId` transparent sealed-cavity row the `Appearance/graph#MATERIAL_LIBRARY` owner carries (transmission 1.0, IOR 1.0, no Fresnel interface — the gas species never re-distinguished in appearance), its thermal identity the typed `CavityGas`; the layer thickness crosses the IFC wire as the `IfcMaterialLayer.LayerThickness`.
