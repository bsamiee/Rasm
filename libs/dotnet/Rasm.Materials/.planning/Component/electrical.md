# [MATERIALS_ELECTRICAL]

THE ELECTRICAL SEED PAGE owns the `ComponentFamily.Electrical` row facts (`ComponentClass.Minor`, `DetailLane.Product`): the buildable conductor PRODUCT rows — AWG/kcmil and metric sizes, alloy, insulation class, and the NEC 310.16 / IEC 60364-5-52 ampacity cells as component RATING rows — and the containment vocabularies the conduit and cable-tray trades stand on. The conductor rosters store only the columns the standards print independently (circular-mil area, metric cross-section) and derive every geometric sibling; the two ampacity estates transcribe cell-for-cell with the one cross-source conflict landing ABSENT and every single-posted cell refused. The conduit trade-size ladder and the NEMA tray-class vocabulary carry NO dimension or load cell — no pack proves the NEC Chapter 9 Table 4 ladder or the NEMA VE 1 working loads — so neither containment family mints a component until its columns prove. Electrical SIZING — load calculation, ambient and grouping derating, voltage drop, protection coordination, conduit fill — is `Rasm.Compute`'s; this page owns the buildable product rows alone, and ampacity is lawful here exactly because it is a COMPONENT rating: the substance catalogue admits conductor constants alone and no current rating ever seeds a roster row (`Properties/properties#MATERIAL_PROPERTY_CATALOGUE`).

`ConductorAlloy` is this page's engineering-substance axis and the `Appearance/surface#CONDUCTOR_IOR` `ConductorMetal` its OPTICAL peer: two S0-peer sub-domains that share zero columns and zero consumers, so the name survives at the optical owner and the join between them is PROVEN rather than asserted — `ElectricalSeed.RosterCensus` folds a type-init parity census over the `AppearanceId` join at every catalogue build, so an alloy row whose library id no `ConductorMetal` key resolves refuses the build instead of shading through the dielectric fallback. The page composes settled law without re-derivation: `SectionProfile.Circle.Of` is the railed geometry admission over the area-true solid-equivalent diameter and `component#COMPONENT_SEED` the ONE generator fold; the product bag builds through `ComponentDetail.ProductRows` over the `Rasm.Element/Properties/property#DETAIL_SCHEMA` rows `ConductorSize`/`InsulationClass`/`AmpacityBasis` (containment trade-size and tray-load rows re-land at the seam the moment their NEC/NEMA columns prove); `Attestation` and the `SegmentRows` trade mints compose from `pipework#PIPE_SYSTEMS` and `pipework#PIPEWORK_SEED`; the ampere dimension is the seam roster's own `Dimension.CurrentDim`; `ThreadRow.InchToMm` is the one inch basis (`fastener#FASTENER_FAMILY`); the IFC stamps are `IfcCableSegment`/`CONDUCTORSEGMENT` for conductors, `IfcCableCarrierSegment`/`CONDUITSEGMENT` for conduit, and `IfcCableCarrierSegment`/`CABLELADDERSEGMENT`+`CABLETRAYSEGMENT` for the tray kinds, all Gate-0 valid at Ifc2X3; substances bind `copper.c12200`/`aluminium.1350`/`steel.galvanized`/`pipe.pvc` and appearances `metal.copper`/`metal.aluminum`/`metal.steel`/`plastic.pvc`.

## [01]-[INDEX]

- [02]-[CONDUCTOR_TABLES]: the `ConductorAlloy` axis with its appearance-join census, `InsulationClass`, `IecMethod`, and `ConductorLoading`; the `Awg` and `Metric` published size rosters with their derivation algebra; the `Table310` NEC estate and the ONE `TableB52` `AmpacityRow` estate.
- [03]-[CONTAINMENT]: the `ConduitSystem` and `TrayKind` policy rows, the trade-size and NEMA class vocabularies — typed-absent admission domains that mint nothing until their dimension packs prove.
- [04]-[ELECTRICAL_SEED]: the `AmpacityBasis` installation-basis axis with its stocking delegate, the `WireSystem` stocked selections, the `WireRow` seed currency, and `ElectricalSeed` — the flattened roster, the two-census coherence, the product bag, and the typed sizing refusal.

## [02]-[CONDUCTOR_TABLES]

- Owner: `ConductorAlloy` the one engineering-metal axis — the NEC cell-triple read, both `MaterialId` slots, the `AppearanceMetal` join, and the parity census over it; `InsulationClass` the NEC temperature-column pick and the IEC PVC-table admission; `IecMethod` the Table B.52.1 reference-method identity; `ConductorLoading` the loaded-conductor axis the IEC tables are cut by; `Awg`/`Metric` the published size rosters; `Table310`/`TableB52` the transcribed ampacity estates.
- Cases: 18 AWG/kcmil rows (14 AWG–500 kcmil, cmil the ONE stored column) and 16 metric rows (1.5–300 mm²); NEC copper 18×3 and aluminium 17×3 cells (no 14 AWG aluminium row — `Option` absence, never a zero); IEC B.52.4 three-loaded copper 16 and aluminium 15 rows with the four 2.5–10 mm² D2 cells typed-absent, and B.52.2 two-loaded copper 15 rows — all three estates ONE `AmpacityRow` roster keyed by (alloy, loading, size), each row carrying the FULL seven-method cell map so an unpublished method is a stated absence rather than a missing key.
- Law: stored columns are only what the standards print independently — the AWG solid diameter and mm² area derive from the circular-mil definition's own algebra (d = √cmil mil, 1 cmil = π/4 mil²) and the metric diameter from the cross-section — so a transcription slip in a derivable cell is unrepresentable and the derivation reproduces every published mm² and diameter cell exactly.
- Law: the B.52.2 240 mm² method-A1 cell lands ABSENT — the two sources print 320 and 321 and a conflicted cell never crosses the seam wearing either value; the single-posted 300 mm² two-loaded row is not transcribed at all, joining when a second posting proves.
- Law: the NEC 8 AWG aluminium 60 °C cell stands at 35 A — two independent sources agree and the 30 A third posting is the recorded outlier, so the cell is `Corroborated`, not conflicted.
- Law: ampacity is a COMPONENT rating, never a substance column — a cell keys on insulation temperature rating, installation method, and conductor grouping, none of which a substance carries; the catalogue's `Electrical` case admits resistivity and permittivity alone and this page owns every current cell.
- Law: `ConductorAlloy.AppearanceId` is a JOIN, not a label — every row's `metal.<name>` id must resolve a `Appearance/surface#CONDUCTOR_IOR` `ConductorMetal` key, which the deferred parity census proves at catalogue build. The optical roster spells the metal `aluminum` and this page's engineering row spells the alloy `aluminium` after its EN designation; the two spellings are lawful because they name different axes, and the census is what keeps the JOIN VALUE on the optical roster's spelling instead of drifting to the alloy's.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (`MaterialId`, `EvidenceGrade`, `Dimension.CurrentDim`, the seam bag currencies), Rasm.Materials.Appearance (`ConductorMetal.Resolve` — the ONE optical-roster read the parity census composes), the parent `component#COMPONENT_OWNER`/`#COMPONENT_DETAIL`/`#COMPONENT_SEED` owners, the sibling `Attestation`/`SegmentRows`, the `ThreadRow.InchToMm` inch basis, Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]`), LanguageExt.Core, BCL (`ImmutableArray`, `FrozenDictionary`). NO conductor-table producer exists among admitted packages, so the rosters and ampacity estates are PUBLISHED here under SEED_ROW_LAW with per-column provenance.
- Growth: a new insulation temperature class is one `InsulationClass` row; a new reference method one `IecMethod` row and one cell per `AmpacityRow`; the XLPE-90 IEC tables are `AmpacityRow` rows at a new loading or a new basis, read by the same filter; a size beyond 500 kcmil or 300 mm² one roster row; a second conductor alloy (copper-clad aluminium as its own product) one `ConductorAlloy` row whose appearance join the census then proves.
- Boundary: the ambient and ground-temperature correction ladders and the conductor-count grouping factors are `Rasm.Compute` derating inputs — captured two-sourced in evidence, they land on the sizing route and never stamp a component row, because a corrected ampacity is a DESIGN verdict over an occurrence, not a product fact.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Globalization;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Rasm.Materials.Appearance;
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConductorAlloy {
    public static readonly ConductorAlloy Copper    = new("copper",    substanceId: "copper.c12200",  appearanceId: "metal.copper",   nec: static row => Some((row.Cu60, row.Cu75, row.Cu90)));
    public static readonly ConductorAlloy Aluminium = new("aluminium", substanceId: "aluminium.1350", appearanceId: "metal.aluminum", nec: static row => row.Al);

    [UseDelegateFromConstructor] public partial Option<(double C60, double C75, double C90)> NecCells(NecRow row);
    public string SubstanceId { get; }
    public string AppearanceId { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);

    public Option<ConductorMetal> AppearanceMetal =>
        AppearanceId.Split('.') is [string family, string name] ? ConductorMetal.Resolve(family, name) : None;

    public static readonly Lazy<Validation<Error, Unit>> AppearanceParity = new(static () =>
        toSeq(Items)
            .Map(static alloy => guard(alloy.AppearanceMetal.IsSome,
                new KernelFault.InvalidValue(nameof(alloy.AppearanceMetal), "a resolved conductor appearance", Some(Join))).ToValidation())
            .Sequence().As().Map(static _ => unit));

    static readonly Op Join = Op.Of(name: "conductor-appearance-parity");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class InsulationClass {
    public static readonly InsulationClass Tw    = new("tw",     pvc: false, nec: static (c60, c75, c90) => Some(c60));
    public static readonly InsulationClass Thw   = new("thw",    pvc: false, nec: static (c60, c75, c90) => Some(c75));
    public static readonly InsulationClass Thhn  = new("thhn",   pvc: false, nec: static (c60, c75, c90) => Some(c90));
    public static readonly InsulationClass Xhhw  = new("xhhw",   pvc: false, nec: static (c60, c75, c90) => Some(c90));
    public static readonly InsulationClass Pvc70 = new("pvc-70", pvc: true,  nec: static (c60, c75, c90) => None);

    [UseDelegateFromConstructor] public partial Option<double> Nec(double c60, double c75, double c90);
    public bool Pvc { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IecMethod {
    public static readonly IecMethod A1 = new("a1");
    public static readonly IecMethod A2 = new("a2");
    public static readonly IecMethod B1 = new("b1");
    public static readonly IecMethod B2 = new("b2");
    public static readonly IecMethod C  = new("c");
    public static readonly IecMethod D1 = new("d1");
    public static readonly IecMethod D2 = new("d2");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConductorLoading {
    public static readonly ConductorLoading Two   = new("two-loaded");
    public static readonly ConductorLoading Three = new("three-loaded");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct AwgRow(string Key, double Cmil) {
    public const double MilToMm = ThreadRow.InchToMm / 1000.0;
    public string Tag => Key.Replace("/", "-");
    public double DiameterMm => Math.Sqrt(Cmil) * MilToMm;
}

public readonly record struct MetricRow(double Mm2) {
    public string Key => Mm2.ToString("0.#", CultureInfo.InvariantCulture);
    public string Tag => (Mm2 * 10.0).ToString("0000", CultureInfo.InvariantCulture);
    public double DiameterMm => 2.0 * Math.Sqrt(Mm2 / Math.PI);
}

public readonly record struct NecRow(AwgRow Size, double Cu60, double Cu75, double Cu90, Option<(double C60, double C75, double C90)> Al);

public readonly record struct AmpacityRow(
    ConductorAlloy Alloy, ConductorLoading Loading, MetricRow Size, FrozenDictionary<IecMethod, Option<double>> Cells);

// --- [TABLES] --------------------------------------------------------------------------
public static class Awg {
    public static readonly AwgRow S14   = new("14",       4_110.0);
    public static readonly AwgRow S12   = new("12",       6_530.0);
    public static readonly AwgRow S10   = new("10",      10_380.0);
    public static readonly AwgRow S8    = new("8",       16_510.0);
    public static readonly AwgRow S6    = new("6",       26_240.0);
    public static readonly AwgRow S4    = new("4",       41_740.0);
    public static readonly AwgRow S3    = new("3",       52_620.0);
    public static readonly AwgRow S2    = new("2",       66_360.0);
    public static readonly AwgRow S1    = new("1",       83_690.0);
    public static readonly AwgRow S1_0  = new("1/0",    105_600.0);
    public static readonly AwgRow S2_0  = new("2/0",    133_100.0);
    public static readonly AwgRow S3_0  = new("3/0",    167_800.0);
    public static readonly AwgRow S4_0  = new("4/0",    211_600.0);
    public static readonly AwgRow K250  = new("250kcmil", 250_000.0);
    public static readonly AwgRow K300  = new("300kcmil", 300_000.0);
    public static readonly AwgRow K350  = new("350kcmil", 350_000.0);
    public static readonly AwgRow K400  = new("400kcmil", 400_000.0);
    public static readonly AwgRow K500  = new("500kcmil", 500_000.0);
    public static readonly ImmutableArray<AwgRow> Rows = [S14, S12, S10, S8, S6, S4, S3, S2, S1, S1_0, S2_0, S3_0, S4_0, K250, K300, K350, K400, K500];
}

public static class Metric {
    public static readonly MetricRow M1p5 = new(1.5);
    public static readonly MetricRow M2p5 = new(2.5);
    public static readonly MetricRow M4   = new(4.0);
    public static readonly MetricRow M6   = new(6.0);
    public static readonly MetricRow M10  = new(10.0);
    public static readonly MetricRow M16  = new(16.0);
    public static readonly MetricRow M25  = new(25.0);
    public static readonly MetricRow M35  = new(35.0);
    public static readonly MetricRow M50  = new(50.0);
    public static readonly MetricRow M70  = new(70.0);
    public static readonly MetricRow M95  = new(95.0);
    public static readonly MetricRow M120 = new(120.0);
    public static readonly MetricRow M150 = new(150.0);
    public static readonly MetricRow M185 = new(185.0);
    public static readonly MetricRow M240 = new(240.0);
    public static readonly MetricRow M300 = new(300.0);
    public static readonly ImmutableArray<MetricRow> Rows = [M1p5, M2p5, M4, M6, M10, M16, M25, M35, M50, M70, M95, M120, M150, M185, M240, M300];
}

public static class Table310 {
    public static readonly ImmutableArray<NecRow> Rows = [
        new(Awg.S14,   15.0,  20.0,  25.0, None),
        new(Awg.S12,   20.0,  25.0,  30.0, Some(( 15.0,  20.0,  25.0))),
        new(Awg.S10,   30.0,  35.0,  40.0, Some(( 25.0,  30.0,  35.0))),
        new(Awg.S8,    40.0,  50.0,  55.0, Some(( 35.0,  40.0,  45.0))),
        new(Awg.S6,    55.0,  65.0,  75.0, Some(( 40.0,  50.0,  55.0))),
        new(Awg.S4,    70.0,  85.0,  95.0, Some(( 55.0,  65.0,  75.0))),
        new(Awg.S3,    85.0, 100.0, 115.0, Some(( 65.0,  75.0,  85.0))),
        new(Awg.S2,    95.0, 115.0, 130.0, Some(( 75.0,  90.0, 100.0))),
        new(Awg.S1,   110.0, 130.0, 145.0, Some(( 85.0, 100.0, 115.0))),
        new(Awg.S1_0, 125.0, 150.0, 170.0, Some((100.0, 120.0, 135.0))),
        new(Awg.S2_0, 145.0, 175.0, 195.0, Some((115.0, 135.0, 150.0))),
        new(Awg.S3_0, 165.0, 200.0, 225.0, Some((130.0, 155.0, 175.0))),
        new(Awg.S4_0, 195.0, 230.0, 260.0, Some((150.0, 180.0, 205.0))),
        new(Awg.K250, 215.0, 255.0, 290.0, Some((170.0, 205.0, 230.0))),
        new(Awg.K300, 240.0, 285.0, 320.0, Some((195.0, 230.0, 260.0))),
        new(Awg.K350, 260.0, 310.0, 350.0, Some((210.0, 250.0, 280.0))),
        new(Awg.K400, 280.0, 335.0, 380.0, Some((225.0, 270.0, 305.0))),
        new(Awg.K500, 320.0, 380.0, 430.0, Some((260.0, 310.0, 350.0)))];
}

public static class TableB52 {
    public static readonly ImmutableArray<AmpacityRow> Rows = [
        Three(ConductorAlloy.Copper, Metric.M1p5,  13.5,  13.0,  15.5,  15.0,  17.5,  18.0, Some( 19.0)),
        Three(ConductorAlloy.Copper, Metric.M2p5,  18.0,  17.5,  21.0,  20.0,  24.0,  24.0, Some( 24.0)),
        Three(ConductorAlloy.Copper, Metric.M4,    24.0,  23.0,  28.0,  27.0,  32.0,  30.0, Some( 33.0)),
        Three(ConductorAlloy.Copper, Metric.M6,    31.0,  29.0,  36.0,  34.0,  41.0,  38.0, Some( 41.0)),
        Three(ConductorAlloy.Copper, Metric.M10,   42.0,  39.0,  50.0,  46.0,  57.0,  50.0, Some( 54.0)),
        Three(ConductorAlloy.Copper, Metric.M16,   56.0,  52.0,  68.0,  62.0,  76.0,  64.0, Some( 70.0)),
        Three(ConductorAlloy.Copper, Metric.M25,   73.0,  68.0,  89.0,  80.0,  96.0,  82.0, Some( 92.0)),
        Three(ConductorAlloy.Copper, Metric.M35,   89.0,  83.0, 110.0,  99.0, 119.0,  98.0, Some(110.0)),
        Three(ConductorAlloy.Copper, Metric.M50,  108.0,  99.0, 134.0, 118.0, 144.0, 116.0, Some(130.0)),
        Three(ConductorAlloy.Copper, Metric.M70,  136.0, 125.0, 171.0, 149.0, 184.0, 143.0, Some(162.0)),
        Three(ConductorAlloy.Copper, Metric.M95,  164.0, 150.0, 207.0, 179.0, 223.0, 169.0, Some(193.0)),
        Three(ConductorAlloy.Copper, Metric.M120, 188.0, 172.0, 239.0, 206.0, 259.0, 192.0, Some(220.0)),
        Three(ConductorAlloy.Copper, Metric.M150, 216.0, 196.0, 262.0, 225.0, 299.0, 217.0, Some(246.0)),
        Three(ConductorAlloy.Copper, Metric.M185, 245.0, 223.0, 296.0, 255.0, 341.0, 243.0, Some(278.0)),
        Three(ConductorAlloy.Copper, Metric.M240, 286.0, 261.0, 346.0, 297.0, 403.0, 280.0, Some(320.0)),
        Three(ConductorAlloy.Copper, Metric.M300, 328.0, 298.0, 394.0, 339.0, 464.0, 316.0, Some(359.0)),

        Three(ConductorAlloy.Aluminium, Metric.M2p5,  14.0,  13.5,  16.5,  15.5,  18.5,  18.5, None),
        Three(ConductorAlloy.Aluminium, Metric.M4,    18.5,  17.5,  22.0,  21.0,  25.0,  24.0, None),
        Three(ConductorAlloy.Aluminium, Metric.M6,    24.0,  23.0,  28.0,  27.0,  32.0,  30.0, None),
        Three(ConductorAlloy.Aluminium, Metric.M10,   32.0,  31.0,  39.0,  36.0,  44.0,  39.0, None),
        Three(ConductorAlloy.Aluminium, Metric.M16,   43.0,  41.0,  53.0,  48.0,  59.0,  50.0, Some( 53.0)),
        Three(ConductorAlloy.Aluminium, Metric.M25,   57.0,  53.0,  70.0,  62.0,  73.0,  64.0, Some( 69.0)),
        Three(ConductorAlloy.Aluminium, Metric.M35,   70.0,  65.0,  86.0,  77.0,  90.0,  77.0, Some( 83.0)),
        Three(ConductorAlloy.Aluminium, Metric.M50,   84.0,  78.0, 104.0,  92.0, 110.0,  91.0, Some( 99.0)),
        Three(ConductorAlloy.Aluminium, Metric.M70,  107.0,  98.0, 133.0, 116.0, 140.0, 112.0, Some(122.0)),
        Three(ConductorAlloy.Aluminium, Metric.M95,  129.0, 118.0, 161.0, 139.0, 170.0, 132.0, Some(148.0)),
        Three(ConductorAlloy.Aluminium, Metric.M120, 149.0, 135.0, 186.0, 160.0, 197.0, 150.0, Some(169.0)),
        Three(ConductorAlloy.Aluminium, Metric.M150, 170.0, 155.0, 204.0, 176.0, 227.0, 169.0, Some(189.0)),
        Three(ConductorAlloy.Aluminium, Metric.M185, 194.0, 176.0, 230.0, 199.0, 259.0, 190.0, Some(214.0)),
        Three(ConductorAlloy.Aluminium, Metric.M240, 227.0, 207.0, 269.0, 232.0, 305.0, 218.0, Some(250.0)),
        Three(ConductorAlloy.Aluminium, Metric.M300, 261.0, 237.0, 306.0, 265.0, 351.0, 247.0, Some(282.0)),

        Two(Metric.M1p5, Some( 14.5),  14.0,  17.5,  16.5,  19.5),
        Two(Metric.M2p5, Some( 19.5),  18.5,  24.0,  23.0,  27.0),
        Two(Metric.M4,   Some( 26.0),  25.0,  32.0,  30.0,  36.0),
        Two(Metric.M6,   Some( 34.0),  32.0,  41.0,  38.0,  46.0),
        Two(Metric.M10,  Some( 46.0),  43.0,  57.0,  52.0,  63.0),
        Two(Metric.M16,  Some( 61.0),  57.0,  76.0,  69.0,  85.0),
        Two(Metric.M25,  Some( 80.0),  75.0, 101.0,  90.0, 112.0),
        Two(Metric.M35,  Some( 99.0),  92.0, 125.0, 111.0, 138.0),
        Two(Metric.M50,  Some(119.0), 110.0, 151.0, 133.0, 168.0),
        Two(Metric.M70,  Some(151.0), 139.0, 192.0, 168.0, 213.0),
        Two(Metric.M95,  Some(182.0), 167.0, 232.0, 201.0, 258.0),
        Two(Metric.M120, Some(210.0), 192.0, 269.0, 232.0, 299.0),
        Two(Metric.M150, Some(240.0), 219.0, 300.0, 258.0, 344.0),
        Two(Metric.M185, Some(273.0), 248.0, 341.0, 294.0, 392.0),
        Two(Metric.M240, None,        291.0, 400.0, 344.0, 461.0)];

    static AmpacityRow Three(ConductorAlloy alloy, MetricRow size, double a1, double a2, double b1, double b2, double c, double d1, Option<double> d2) =>
        new(alloy, ConductorLoading.Three, size, Cells(Some(a1), Some(a2), Some(b1), Some(b2), Some(c), Some(d1), d2));

    static AmpacityRow Two(MetricRow size, Option<double> a1, double a2, double b1, double b2, double c) =>
        new(ConductorAlloy.Copper, ConductorLoading.Two, size, Cells(a1, Some(a2), Some(b1), Some(b2), Some(c), None, None));

    static FrozenDictionary<IecMethod, Option<double>> Cells(
        Option<double> a1, Option<double> a2, Option<double> b1, Option<double> b2, Option<double> c, Option<double> d1, Option<double> d2) =>
        new Dictionary<IecMethod, Option<double>> {
            [IecMethod.A1] = a1, [IecMethod.A2] = a2, [IecMethod.B1] = b1, [IecMethod.B2] = b2,
            [IecMethod.C] = c, [IecMethod.D1] = d1, [IecMethod.D2] = d2,
        }.ToFrozenDictionary();
}
```

## [03]-[CONTAINMENT]

- Owner: `ConduitSystem` the raceway policy axis — substance and appearance bindings and the `CONDUITSEGMENT` stamp, standing ready per system; `TrayKind` the cable-tray form axis carrying its own Gate-0 predefined token per kind; `TrayClass` the NEMA VE 1 span-load vocabulary row; `Containment` the trade-size ladder and the generated class roster.
- Cases: five conduit systems {emt · imc · rmc · pvc-40 · pvc-80} over the ten-rung trade-size ladder ½–4 in; three tray kinds {ladder · trough · channel}; twelve NEMA classes as the {8 · 12 · 16 · 20 ft} × {A · B · C} cross.
- Law: every dimension and load column is typed-absent — no pack proves the NEC Chapter 9 Table 4 OD/ID ladder, the per-system size spans above 4 in, or the NEMA VE 1 working-load cells — so no `CircleHollow` exists to admit, neither containment family mints a component, and no containment trade-size or tray-load seam row exists until the columns prove; a vocabulary minting a guessed dimension certifies an unbuilt product.
- Law: the tray-class key IS the NEMA designation's own span-and-load algebra (`12b` = 12 ft support span, load class B) — identity, never a measured cell; the working load joins as the `Option` column flip when its table proves.
- Growth: the conduit dimension pack landing is one column pair per system row and one `SeedLaw` beside the conductor law; a proven tray section is one width-and-rail roster read by the same kinds; liquid-tight and ENT systems are one `ConduitSystem` row each.
- Boundary: conduit FILL (NEC Chapter 9 Table 1 percentages against conductor build-out areas) is a `Rasm.Compute` design verdict over an occurrence's conductor set — never a product row here.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConduitSystem {
    public static readonly ConduitSystem Emt   = new("emt",    substanceId: "steel.galvanized", appearanceId: "metal.steel");
    public static readonly ConduitSystem Imc   = new("imc",    substanceId: "steel.galvanized", appearanceId: "metal.steel");
    public static readonly ConduitSystem Rmc   = new("rmc",    substanceId: "steel.galvanized", appearanceId: "metal.steel");
    public static readonly ConduitSystem Pvc40 = new("pvc-40", substanceId: "pipe.pvc",         appearanceId: "plastic.pvc");
    public static readonly ConduitSystem Pvc80 = new("pvc-80", substanceId: "pipe.pvc",         appearanceId: "plastic.pvc");

    public string SubstanceId { get; }
    public string AppearanceId { get; }
    public IfcBinding Ifc => IfcBinding.Of("IfcCableCarrierSegment", "CONDUITSEGMENT");
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TrayKind {
    public static readonly TrayKind Ladder  = new("ladder",  predefined: "CABLELADDERSEGMENT");
    public static readonly TrayKind Trough  = new("trough",  predefined: "CABLETRAYSEGMENT");
    public static readonly TrayKind Channel = new("channel", predefined: "CABLETRAYSEGMENT");

    public string Predefined { get; }
    public IfcBinding Ifc => IfcBinding.Of("IfcCableCarrierSegment", Predefined);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TrayClass(string Key, Option<double> WorkingLoadKgPerM);

// --- [TABLES] --------------------------------------------------------------------------
public static class Containment {
    public static readonly ImmutableArray<string> TradeSizes = ["1/2", "3/4", "1", "1-1/4", "1-1/2", "2", "2-1/2", "3", "3-1/2", "4"];
    static readonly Seq<string> Spans = Seq("8", "12", "16", "20");
    static readonly Seq<string> Loads = Seq("a", "b", "c");
    public static readonly Seq<TrayClass> Classes = Spans.Bind(span => Loads.Map(load => new TrayClass($"{span}{load}", None)));
}
```

## [04]-[ELECTRICAL_SEED]

- Owner: `AmpacityBasis` the installation-basis axis every stamped rating names, carrying the STOCKING delegate that resolves its own estate; `WireSystem` the stocked conductor selections; `WireRow` the seed currency; `ElectricalSeed` the flattened roster, the seed law, and the capacity refusal.
- Cases: six stocked systems — three NEC lanes {cu-thhn · cu-xhhw · al-xhhw} folding `Table310` through the alloy and insulation delegates, two three-loaded IEC lanes {cu-pvc · al-pvc} and the two-loaded copper lane {cu-pvc-two} filtering `TableB52` by (alloy, loading) and reading the basis's own method cell — 99 conductors from 49 published roster rows, each rating RESOLVED from its table, never asserted per stocked row; a size whose cell the basis does not publish mints nothing rather than a fabricated ampere.
- Entry: `ComponentSeed.Rows(context, ElectricalSeed.Roster, ElectricalSeed.Law)` — the roster is `WireSystem.Items` flattened through each system's own basis delegate, so the `Rows` fold that hand-wired six systems and the three per-fold guards are gone.
- Law: a bag stamps ONE rating under ONE named basis — the `AmpacityBasis` token beside the ampere row is what keeps an NEC raceway cell from reading as an IEC reference-method cell; the full method axis stays a table read for any consumer needing another basis, and a second stocked basis is ONE `AmpacityBasis` row plus one `WireSystem` row.
- Law: the TW and THW rows close the NEC column set as resolution arms — no TW/THW system is stocked, and stocking the legacy 60/75 °C lanes is one `WireSystem` row each with zero type edits.
- Law: a declared `WireSystem` that stocks NOTHING is a defect, not an empty product line — the roster census names it. It subsumes the three hand guards the folds carried (a basis with no method, an IEC lane over a non-PVC class, a two-loaded lane over aluminium) because each of those conditions produced exactly zero rungs, and it also convicts the class they missed: a system whose estate simply publishes no row for its alloy.
- Output: the projector derives the takeoff rows from the solved `Circle` section through `QuantityRow.VolumePerLength`/`SurfaceAreaPerLength`/`LinearDensity` (`component#QUANTITY_ROW`) — the area-true solid-equivalent section is what makes the copper and aluminium tonnage honest; no takeoff cell is stamped here.
- Boundary: every stamped bag rides `Attestation.Corroborated` because every cell that mints is two-sourced — the conflicted and single-posted cells are typed-absent in the tables and can never reach a bag, so the flag states transcription truth rather than table-wide optimism, per the `pipework#PIPEWORK_SEED` weakest-contributor law. `ElectricalSeed.Capacity` is the typed refusal — a conductor's governing verdict is circuit-side (load, derating, voltage drop, protection), owned by `Rasm.Compute`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AmpacityBasis {
    public static readonly AmpacityBasis Nec310 = new("nec-310-16",  standard: Us,  source: EvidenceGrade.Catalogue, ratings: NecRatings);
    public static readonly AmpacityBasis IecC   = new("iec-b52-4-c", standard: Eu,  source: EvidenceGrade.Catalogue, ratings: static system => IecRatings(system, ConductorLoading.Three, IecMethod.C));
    public static readonly AmpacityBasis IecC2  = new("iec-b52-2-c", standard: Eu,  source: EvidenceGrade.Catalogue, ratings: static system => IecRatings(system, ConductorLoading.Two, IecMethod.C));

    [UseDelegateFromConstructor] public partial Seq<WireRow> Ratings(WireSystem system);
    public ComponentStandard Standard { get; }
    public EvidenceGrade Source { get; }

    static readonly ComponentStandard Us =
        new(ComponentAuthority.Nfpa.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.Nfpa);
    static readonly ComponentStandard Eu =
        new(ComponentAuthority.Iec.Region, StandardJointThicknessMm: 0.0, ComponentAuthority.Iec);

    // --- [ESTATES]
    static Seq<WireRow> NecRatings(WireSystem system) =>
        toSeq(Table310.Rows).Bind(row => system.Alloy.NecCells(row)
            .Bind(cells => system.Insulation.Nec(cells.C60, cells.C75, cells.C90))
            .Map(amps => new WireRow(system, row.Size.Key, row.Size.Tag, row.Size.DiameterMm, amps))
            .ToSeq());

    static Seq<WireRow> IecRatings(WireSystem system, ConductorLoading loading, IecMethod method) =>
        toSeq(TableB52.Rows)
            .Filter(row => row.Alloy == system.Alloy && row.Loading == loading && system.Insulation.Pvc)
            .Bind(row => row.Cells[method]
                .Map(amps => new WireRow(system, row.Size.Key, row.Size.Tag, row.Size.DiameterMm, amps))
                .ToSeq());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireSystem {
    public static readonly WireSystem CuThhn = new("cu-thhn",    alloy: ConductorAlloy.Copper,    insulation: InsulationClass.Thhn,  basis: AmpacityBasis.Nec310, rated: Attestation.Corroborated);
    public static readonly WireSystem CuXhhw = new("cu-xhhw",    alloy: ConductorAlloy.Copper,    insulation: InsulationClass.Xhhw,  basis: AmpacityBasis.Nec310, rated: Attestation.Corroborated);
    public static readonly WireSystem AlXhhw = new("al-xhhw",    alloy: ConductorAlloy.Aluminium, insulation: InsulationClass.Xhhw,  basis: AmpacityBasis.Nec310, rated: Attestation.Corroborated);
    public static readonly WireSystem CuPvc  = new("cu-pvc",     alloy: ConductorAlloy.Copper,    insulation: InsulationClass.Pvc70, basis: AmpacityBasis.IecC,   rated: Attestation.Corroborated);
    public static readonly WireSystem AlPvc  = new("al-pvc",     alloy: ConductorAlloy.Aluminium, insulation: InsulationClass.Pvc70, basis: AmpacityBasis.IecC,   rated: Attestation.Corroborated);
    public static readonly WireSystem CuPvc2 = new("cu-pvc-two", alloy: ConductorAlloy.Copper,    insulation: InsulationClass.Pvc70, basis: AmpacityBasis.IecC2,  rated: Attestation.Corroborated);

    public ConductorAlloy Alloy { get; }
    public InsulationClass Insulation { get; }
    public AmpacityBasis Basis { get; }
    public Attestation Rated { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct WireRow(WireSystem System, string Size, string Tag, double DiameterMm, double Amps) {
    public string Designation => $"electrical.{System.Key}-{Tag}";
}

// --- [TABLES] --------------------------------------------------------------------------
public static class ElectricalSeed {
    static readonly IfcBinding Conductor = IfcBinding.Of("IfcCableSegment", "CONDUCTORSEGMENT");
    static readonly PropertyName Ampacity = PropertyCategory.Materials.Row("Ampacity");
    static readonly Op Proof = Op.Of(name: "electrical-roster-census");

    public static readonly Seq<WireRow> Roster =
        toSeq(WireSystem.Items).Bind(static system => system.Basis.Ratings(system));

    public static readonly Lazy<Validation<Error, Unit>> RosterCensus = new(static () =>
        (ConductorAlloy.AppearanceParity.Value,
         toSeq(WireSystem.Items)
             .Map(static system => guard(Roster.Exists(row => row.System == system),
                 new KernelFault.InvalidValue(nameof(WireSystem), "at least one stocked conductor", Some(Proof))).ToValidation())
             .Sequence().As().Map(static _ => unit))
        .Apply(static (_, _) => unit).As());

    public static readonly SeedLaw<WireRow> Law = SeedLaw<WireRow>.Of(
        family: ComponentFamily.Electrical,
        designation: static r => r.Designation,
        coherence: Coherence,
        profile: static (r, key) => SectionProfile.Circle.Of(r.DiameterMm, key),
        substance: static r => r.System.Alloy.Substance,
        source: static r => r.System.Basis.Source,
        standard: static r => r.System.Basis.Standard,
        detail: Some<Func<WireRow, SectionProfile, Op, Fin<PropertyBag>>>(Detail),
        appearance: static r => r.System.Alloy.Appearance,
        ifc: static _ => Conductor);

    static Validation<Error, Unit> Coherence(WireRow r, Op key) =>
        (RosterCensus.Value,
         guard(double.IsFinite(r.Amps) && r.Amps > 0.0,
             new KernelFault.OutOfRange(nameof(r.Amps), r.Amps, "finite and positive", Some(key))).ToValidation(),
         guard(double.IsFinite(r.DiameterMm) && r.DiameterMm > 0.0,
             new KernelFault.OutOfRange(nameof(r.DiameterMm), r.DiameterMm, "finite and positive", Some(key))).ToValidation())
            .Apply(static (_, _, _) => unit).As();

    static Fin<PropertyBag> Detail(WireRow r, SectionProfile profile, Op key) =>
        from rating in ComponentDetail.Measured(Ampacity, Dimension.CurrentDim, r.Amps)
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.ConductorSize, r.Size),
            ComponentDetail.Token(DetailSchema.InsulationClass, r.System.Insulation.Key),
            ComponentDetail.Token(DetailSchema.AmpacityBasis, r.System.Basis.Key),
            ComponentDetail.Sourced(r.System.Basis.Source),
            SegmentRows.Attested(r.System.Rated),
            rating,
        ]);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        new ComponentFault.CapacityUnavailable(key, component.Designation);
}
```

## [05]-[RESEARCH]

(none)
