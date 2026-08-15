# [MATERIALS_ELECTRICAL]

THE ELECTRICAL SEED PAGE owns the `ComponentFamily.Electrical` fold (`ComponentClass.Minor`, `DetailLane.Product`): the buildable conductor PRODUCT rows — AWG/kcmil and metric sizes, metal, insulation class, and the NEC 310.16 / IEC 60364-5-52 ampacity cells as component RATING rows — and the containment vocabularies the conduit and cable-tray trades stand on. The conductor rosters store only the columns the standards print independently (circular-mil area, metric cross-section) and derive every geometric sibling; the two ampacity estates transcribe cell-for-cell with the one cross-source conflict landing ABSENT and every single-posted cell refused. The conduit trade-size ladder and the NEMA tray-class vocabulary carry NO dimension or load cell — no pack proves the NEC Chapter 9 Table 4 ladder or the NEMA VE 1 working loads — so neither containment family mints a component until its columns prove. Electrical SIZING — load calculation, ambient and grouping derating, voltage drop, protection coordination, conduit fill — is `Rasm.Compute`'s; this page owns the buildable product rows alone, and ampacity is lawful here exactly because it is a COMPONENT rating: the substance catalogue admits conductor constants alone and no current rating ever seeds a roster row (`Properties/properties#MATERIAL_PROPERTY_CATALOGUE`).

The page composes settled law without re-derivation: `SectionProfile.Circle.Of` is the railed geometry admission over the area-true solid-equivalent diameter and `Component.Of` the one construction rail (`component#COMPONENT_OWNER`); the product bag builds through `ComponentDetail.ProductRows` over the `Rasm.Element/Properties/property#DETAIL_SCHEMA` rows `ConductorSize`/`InsulationClass`/`AmpacityBasis`, with `TradeSize`/`TrayLoadClass` reserved for the containment mints; `Attestation` and the `SegmentRows` trade mints compose from `pipework#PIPE_SYSTEMS` and `pipework#PIPEWORK_SEED`; `ThreadRow.InchToMm` is the one inch basis (`fastener#FASTENER_FAMILY`); the IFC stamps are `IfcCableSegment`/`CONDUCTORSEGMENT` for conductors, `IfcCableCarrierSegment`/`CONDUITSEGMENT` for conduit, and `IfcCableCarrierSegment`/`CABLELADDERSEGMENT`+`CABLETRAYSEGMENT` for the tray kinds, all Gate-0 valid at Ifc2X3; substances bind `copper.c12200`/`aluminium.1350`/`steel.galvanized`/`pipe.pvc` and appearances `metal.copper`/`metal.aluminum`/`metal.steel`/`plastic.pvc` at `Properties/properties#MATERIAL_PROPERTY_CATALOGUE`.

## [01]-[INDEX]

- [02]-[CONDUCTOR_TABLES]: the `ConductorMetal`, `InsulationClass`, and `IecMethod` axes; the `Awg` and `Metric` published size rosters with their derivation algebra; the `Table310` NEC and `TableB52` IEC ampacity estates.
- [03]-[CONTAINMENT]: the `ConduitSystem` and `TrayKind` policy rows, the trade-size and NEMA class vocabularies — typed-absent admission domains that mint nothing until their dimension packs prove.
- [04]-[ELECTRICAL_SEED]: the `AmpacityBasis` installation-basis axis, the `WireSystem` stocked selections, the `ElectricalDetail` product bag, the `ElectricalSeed.Rows` generation fold, and the typed sizing refusal.

## [02]-[CONDUCTOR_TABLES]

- Owner: `ConductorMetal` the one metal axis — the NEC cell-triple read, the IEC roster read, and both `MaterialId` slots per metal; `InsulationClass` the NEC temperature-column pick and the IEC PVC-table admission; `IecMethod` the Table B.52.1 reference-method axis with its three-loaded and two-loaded column reads; `Awg`/`Metric` the published size rosters; `Table310`/`TableB52` the transcribed ampacity estates.
- Cases: 18 AWG/kcmil rows (14 AWG–500 kcmil, cmil the ONE stored column) and 16 metric rows (1.5–300 mm²); NEC copper 18×3 and aluminium 17×3 cells (no 14 AWG aluminium row — `Option` absence, never a zero); IEC B.52.4 copper 16×7 and aluminium 15×7 with the four 2.5–10 mm² D2 cells typed-absent and no aluminium 1.5 mm² row; B.52.2 two-loaded copper 15×5.
- Law: stored columns are only what the standards print independently — the AWG solid diameter and mm² area derive from the circular-mil definition's own algebra (d = √cmil mil, 1 cmil = π/4 mil²) and the metric diameter from the cross-section — so a transcription slip in a derivable cell is unrepresentable and the derivation reproduces every published mm² and diameter cell exactly.
- Law: the B.52.2 240 mm² method-A1 cell lands ABSENT — the two sources print 320 and 321 and a conflicted cell never crosses the seam wearing either value; the single-posted 300 mm² two-loaded row is not transcribed at all, joining when a second posting proves.
- Law: the NEC 8 AWG aluminium 60 °C cell stands at 35 A — two independent sources agree and the 30 A third posting is the recorded outlier, so the cell is `Corroborated`, not conflicted.
- Law: ampacity is a COMPONENT rating, never a substance column — a cell keys on insulation temperature rating, installation method, and conductor grouping, none of which a substance carries; the catalogue's `Electrical` case admits resistivity and permittivity alone and this page owns every current cell.
- Packages: Rasm.Domain (`Op`/`Context`), Rasm.Element (the seam bag currencies), Rasm.Materials.Component parent owner (`Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile`/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`/`Provenance`, the sibling `Attestation`/`SegmentRows`, the `ThreadRow.InchToMm` inch basis), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[UseDelegateFromConstructor]`), LanguageExt.Core, BCL (`ImmutableArray`). NO conductor-table producer exists among admitted packages, so the rosters and ampacity estates are PUBLISHED here under SEED_ROW_LAW with per-column provenance.
- Growth: a new insulation temperature class is one `InsulationClass` row; a new reference method one `IecMethod` row; the XLPE-90 IEC tables one roster pair beside `TableB52` read by the same method delegates; a size beyond 500 kcmil or 300 mm² one roster row; a second conductor metal (copper-clad aluminium as its own product) one `ConductorMetal` row.
- Boundary: the ambient and ground-temperature correction ladders and the conductor-count grouping factors are `Rasm.Compute` derating inputs — captured two-sourced in evidence, they land on the sizing route and never stamp a component row, because a corrected ampacity is a DESIGN verdict over an occurrence, not a product fact.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;
using System.Globalization;             // the invariant designation-tag format
using LanguageExt;
using Rasm.Domain;                      // Op, Context
using Rasm.Element.Composition;         // MaterialId, DetailSchema, PropertyBag, PropertyName, PropertyValue
using Rasm.Element.Properties;          // MeasureValue, PropertyCategory
using Thinktecture;
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis the detail-bag mints ride
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The conductor-metal axis: the NEC cell-triple read (aluminium answers None at 14 AWG — the table prints no row),
// the IEC roster read (deferred behind a delegate so the axis never races the table statics), and both MaterialId
// slots. aluminium.1350 is the AA-1350 conductor grade — the property-catalogue row lands with this page per
// SUBSTANCE-ID CLOSURE; copper.c12200 is the one catalogue copper carrying the conductor resistivity column.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConductorMetal {
    public static readonly ConductorMetal Copper    = new("copper",    substanceId: "copper.c12200",  appearanceId: "metal.copper",   nec: static row => Some((row.Cu60, row.Cu75, row.Cu90)), iec: static () => toSeq(TableB52.Cu));
    public static readonly ConductorMetal Aluminium = new("aluminium", substanceId: "aluminium.1350", appearanceId: "metal.aluminum", nec: static row => row.Al,                               iec: static () => toSeq(TableB52.Al));

    [UseDelegateFromConstructor] public partial Option<(double C60, double C75, double C90)> NecCells(NecRow row);
    [UseDelegateFromConstructor] public partial Seq<IecRow> IecRows();
    public string SubstanceId { get; }
    public string AppearanceId { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
}

// The insulation axis: the NEC pick delegate names which temperature column a class reads (TW 60 °C, THW 75 °C,
// THHN/XHHW 90 °C — the rating is the class designation's own letter algebra), and Pvc answers the IEC lane — the
// B.52 tables published here are PVC-insulation 70 °C tables, so only the PVC row admits them and an XLPE class
// gains its own admission bit when its tables land. Pvc70 reads no NEC column; a NEC-basis system over it mints
// nothing rather than a fabricated cell.
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

// The Table B.52.1 reference-method axis. Three reads the B.52.4 three-loaded row, Two the B.52.2 two-loaded row —
// the pick delegates keep every column choice a row read, and the methods the two-loaded table does not publish
// (buried D1/D2) answer None instead of borrowing a three-loaded cell.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IecMethod {
    public static readonly IecMethod A1 = new("a1", three: static row => Some(row.A1), two: static row => row.A1);
    public static readonly IecMethod A2 = new("a2", three: static row => Some(row.A2), two: static row => Some(row.A2));
    public static readonly IecMethod B1 = new("b1", three: static row => Some(row.B1), two: static row => Some(row.B1));
    public static readonly IecMethod B2 = new("b2", three: static row => Some(row.B2), two: static row => Some(row.B2));
    public static readonly IecMethod C  = new("c",  three: static row => Some(row.C),  two: static row => Some(row.C));
    public static readonly IecMethod D1 = new("d1", three: static row => Some(row.D1), two: static row => None);
    public static readonly IecMethod D2 = new("d2", three: static row => row.D2,       two: static row => None);

    [UseDelegateFromConstructor] public partial Option<double> Three(IecRow row);
    [UseDelegateFromConstructor] public partial Option<double> Two(TwoRow row);
}

// --- [MODELS] ------------------------------------------------------------------------------
// AWG/kcmil size row: the circular-mil area is the ONE published column and the solid diameter its definition's own
// algebra — d = √cmil in mils — so the diameter and mm² cells the sources print (1.63 mm / 2.08 mm² at 14 AWG,
// 12.70 mm / 127 mm² at 250 kcmil) reproduce exactly off the solved circle rather than transcribe. The stored circle
// is the AREA-TRUE solid-equivalent section; a stranded build-out OD is unproved and absent.
public readonly record struct AwgRow(string Key, double Cmil) {
    public const double MilToMm = ThreadRow.InchToMm / 1000.0;
    public string Tag => Key.Replace("/", "-");
    public double DiameterMm => Math.Sqrt(Cmil) * MilToMm;
}

// IEC metric size row: the mm² cross-section is the published identity and the solid-equivalent diameter derives.
// Tag carries tenths ("1.5" -> 0015) so the designation grammar's single-dot law holds at every size.
public readonly record struct MetricRow(double Mm2) {
    public string Key => Mm2.ToString("0.#", CultureInfo.InvariantCulture);
    public string Tag => (Mm2 * 10.0).ToString("0000", CultureInfo.InvariantCulture);
    public double DiameterMm => 2.0 * Math.Sqrt(Mm2 / Math.PI);
}

// NEC 310.16 row: the copper 60/75/90 °C triple plus the aluminium triple as ONE Option — the standard prints no
// aluminium 14 AWG row, and three parallel Options would let a row be half-absent.
public readonly record struct NecRow(AwgRow Size, double Cu60, double Cu75, double Cu90, Option<(double C60, double C75, double C90)> Al);

// IEC B.52.4 row (PVC, three loaded, 30 °C air / 20 °C ground): the seven reference-method cells, D2 optional
// because the aluminium table prints no D2 cell below 16 mm².
public readonly record struct IecRow(MetricRow Size, double A1, double A2, double B1, double B2, double C, double D1, Option<double> D2);

// IEC B.52.2 row (PVC, two loaded, copper, methods A1–C): A1 optional because the 240 mm² cell is the one
// cross-source CONFLICT (320 vs 321) and lands absent.
public readonly record struct TwoRow(MetricRow Size, Option<double> A1, double A2, double B1, double B2, double C);

// --- [TABLES] ------------------------------------------------------------------------------
// The AWG/kcmil ladder, cmil verbatim (three agreeing sources). NAMED statics (the fastener Threads form) so the
// ampacity rows reference sizes SYMBOLICALLY — a typo'd size is a compile miss, never a runtime key.
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

// The IEC 60364 cross-section ladder 1.5–300 mm².
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

// NEC 2023 Table 310.16 — allowable ampacity, ≤3 current-carrying conductors in raceway/cable/earth, 30 °C ambient;
// amperes, two-sourced with a concordant third, unchanged 2020→2023 on every row. The aluminium 8 AWG 60 °C cell
// stands at 35 A (two sources agree; the 30 A third posting is the recorded outlier).
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

// IEC 60364-5-52 Tables B.52.4 (PVC, three loaded) and B.52.2 (PVC, two loaded, copper) — amperes, both two-sourced
// cell-for-cell. The aluminium table starts at 2.5 mm² and prints no D2 cell below 16 mm² (typed-absent, never a
// borrowed copper value). Two carries the 240 mm² method-A1 cell ABSENT under the 320-vs-321 cross-source conflict,
// and its single-posted 300 mm² row is not transcribed.
public static class TableB52 {
    public static readonly ImmutableArray<IecRow> Cu = [
        new(Metric.M1p5,  13.5,  13.0,  15.5,  15.0,  17.5,  18.0, Some( 19.0)),
        new(Metric.M2p5,  18.0,  17.5,  21.0,  20.0,  24.0,  24.0, Some( 24.0)),
        new(Metric.M4,    24.0,  23.0,  28.0,  27.0,  32.0,  30.0, Some( 33.0)),
        new(Metric.M6,    31.0,  29.0,  36.0,  34.0,  41.0,  38.0, Some( 41.0)),
        new(Metric.M10,   42.0,  39.0,  50.0,  46.0,  57.0,  50.0, Some( 54.0)),
        new(Metric.M16,   56.0,  52.0,  68.0,  62.0,  76.0,  64.0, Some( 70.0)),
        new(Metric.M25,   73.0,  68.0,  89.0,  80.0,  96.0,  82.0, Some( 92.0)),
        new(Metric.M35,   89.0,  83.0, 110.0,  99.0, 119.0,  98.0, Some(110.0)),
        new(Metric.M50,  108.0,  99.0, 134.0, 118.0, 144.0, 116.0, Some(130.0)),
        new(Metric.M70,  136.0, 125.0, 171.0, 149.0, 184.0, 143.0, Some(162.0)),
        new(Metric.M95,  164.0, 150.0, 207.0, 179.0, 223.0, 169.0, Some(193.0)),
        new(Metric.M120, 188.0, 172.0, 239.0, 206.0, 259.0, 192.0, Some(220.0)),
        new(Metric.M150, 216.0, 196.0, 262.0, 225.0, 299.0, 217.0, Some(246.0)),
        new(Metric.M185, 245.0, 223.0, 296.0, 255.0, 341.0, 243.0, Some(278.0)),
        new(Metric.M240, 286.0, 261.0, 346.0, 297.0, 403.0, 280.0, Some(320.0)),
        new(Metric.M300, 328.0, 298.0, 394.0, 339.0, 464.0, 316.0, Some(359.0))];

    public static readonly ImmutableArray<IecRow> Al = [
        new(Metric.M2p5,  14.0,  13.5,  16.5,  15.5,  18.5,  18.5, None),
        new(Metric.M4,    18.5,  17.5,  22.0,  21.0,  25.0,  24.0, None),
        new(Metric.M6,    24.0,  23.0,  28.0,  27.0,  32.0,  30.0, None),
        new(Metric.M10,   32.0,  31.0,  39.0,  36.0,  44.0,  39.0, None),
        new(Metric.M16,   43.0,  41.0,  53.0,  48.0,  59.0,  50.0, Some( 53.0)),
        new(Metric.M25,   57.0,  53.0,  70.0,  62.0,  73.0,  64.0, Some( 69.0)),
        new(Metric.M35,   70.0,  65.0,  86.0,  77.0,  90.0,  77.0, Some( 83.0)),
        new(Metric.M50,   84.0,  78.0, 104.0,  92.0, 110.0,  91.0, Some( 99.0)),
        new(Metric.M70,  107.0,  98.0, 133.0, 116.0, 140.0, 112.0, Some(122.0)),
        new(Metric.M95,  129.0, 118.0, 161.0, 139.0, 170.0, 132.0, Some(148.0)),
        new(Metric.M120, 149.0, 135.0, 186.0, 160.0, 197.0, 150.0, Some(169.0)),
        new(Metric.M150, 170.0, 155.0, 204.0, 176.0, 227.0, 169.0, Some(189.0)),
        new(Metric.M185, 194.0, 176.0, 230.0, 199.0, 259.0, 190.0, Some(214.0)),
        new(Metric.M240, 227.0, 207.0, 269.0, 232.0, 305.0, 218.0, Some(250.0)),
        new(Metric.M300, 261.0, 237.0, 306.0, 265.0, 351.0, 247.0, Some(282.0))];

    public static readonly ImmutableArray<TwoRow> Two = [
        new(Metric.M1p5, Some( 14.5),  14.0,  17.5,  16.5,  19.5),
        new(Metric.M2p5, Some( 19.5),  18.5,  24.0,  23.0,  27.0),
        new(Metric.M4,   Some( 26.0),  25.0,  32.0,  30.0,  36.0),
        new(Metric.M6,   Some( 34.0),  32.0,  41.0,  38.0,  46.0),
        new(Metric.M10,  Some( 46.0),  43.0,  57.0,  52.0,  63.0),
        new(Metric.M16,  Some( 61.0),  57.0,  76.0,  69.0,  85.0),
        new(Metric.M25,  Some( 80.0),  75.0, 101.0,  90.0, 112.0),
        new(Metric.M35,  Some( 99.0),  92.0, 125.0, 111.0, 138.0),
        new(Metric.M50,  Some(119.0), 110.0, 151.0, 133.0, 168.0),
        new(Metric.M70,  Some(151.0), 139.0, 192.0, 168.0, 213.0),
        new(Metric.M95,  Some(182.0), 167.0, 232.0, 201.0, 258.0),
        new(Metric.M120, Some(210.0), 192.0, 269.0, 232.0, 299.0),
        new(Metric.M150, Some(240.0), 219.0, 300.0, 258.0, 344.0),
        new(Metric.M185, Some(273.0), 248.0, 341.0, 294.0, 392.0),
        new(Metric.M240, None,        291.0, 400.0, 344.0, 461.0)];
}
```

## [03]-[CONTAINMENT]

- Owner: `ConduitSystem` the raceway policy axis — substance and appearance bindings and the `CONDUITSEGMENT` stamp, standing ready per system; `TrayKind` the cable-tray form axis carrying its own Gate-0 predefined token per kind; `TrayClass` the NEMA VE 1 span-load vocabulary row; `Containment` the trade-size ladder and the generated class roster.
- Cases: five conduit systems {emt · imc · rmc · pvc-40 · pvc-80} over the ten-rung trade-size ladder ½–4 in; three tray kinds {ladder · trough · channel}; twelve NEMA classes as the {8 · 12 · 16 · 20 ft} × {A · B · C} cross.
- Law: every dimension and load column is typed-absent — no pack proves the NEC Chapter 9 Table 4 OD/ID ladder, the per-system size spans above 4 in, or the NEMA VE 1 working-load cells — so no `CircleHollow` exists to admit, neither containment family mints a component, and the `TradeSize`/`TrayLoadClass` stamps stay reserved until the columns prove; a vocabulary minting a guessed dimension certifies an unbuilt product.
- Law: the tray-class key IS the NEMA designation's own span-and-load algebra (`12b` = 12 ft support span, load class B) — identity, never a measured cell; the working load joins as the `Option` column flip when its table proves.
- Growth: the conduit dimension pack landing is one column pair per system row and one mint fold beside the conductor folds; a proven tray section is one width-and-rail roster read by the same kinds; liquid-tight and ENT systems are one `ConduitSystem` row each.
- Boundary: conduit FILL (NEC Chapter 9 Table 1 percentages against conductor build-out areas) is a `Rasm.Compute` design verdict over an occurrence's conductor set — never a product row here.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The raceway policy axis: bindings stand ready, dimensions typed-absent — the roster is the admission domain the
// NEC Chapter 9 Table 4 pack mints from the moment its OD/ID ladder proves. Steel systems ride the one galvanized
// carbon-steel catalogue row; PVC conduit shares the rigid-PVC pipe substance.
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

// The tray form axis: each kind carries its own Gate-0 predefined leaf — a ladder tray is IFC's own
// CABLELADDERSEGMENT, never a mislabelled tray segment.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TrayKind {
    public static readonly TrayKind Ladder  = new("ladder",  predefined: "CABLELADDERSEGMENT");
    public static readonly TrayKind Trough  = new("trough",  predefined: "CABLETRAYSEGMENT");
    public static readonly TrayKind Channel = new("channel", predefined: "CABLETRAYSEGMENT");

    public string Predefined { get; }
    public IfcBinding Ifc => IfcBinding.Of("IfcCableCarrierSegment", Predefined);
}

// --- [MODELS] ------------------------------------------------------------------------------
// One NEMA VE 1 class: the span-load designation as identity, the working load typed-absent until its cell proves.
public readonly record struct TrayClass(string Key, Option<double> WorkingLoadKgPerM);

// --- [TABLES] ------------------------------------------------------------------------------
// The trade-size ladder the five conduit systems share and the generated NEMA class cross — vocabulary only; every
// numeric column on both estates is typed-absent.
public static class Containment {
    public static readonly ImmutableArray<string> TradeSizes = ["1/2", "3/4", "1", "1-1/4", "1-1/2", "2", "2-1/2", "3", "3-1/2", "4"];
    static readonly Seq<string> Spans = Seq("8", "12", "16", "20");
    static readonly Seq<string> Loads = Seq("a", "b", "c");
    public static readonly Seq<TrayClass> Classes = Spans.Bind(span => Loads.Map(load => new TrayClass($"{span}{load}", None)));
}
```

## [04]-[ELECTRICAL_SEED]

- Owner: `AmpacityBasis` the installation-basis axis every stamped rating names; `WireSystem` the stocked conductor selections; `ElectricalDetail` the product-bag constructor; `ElectricalSeed` the generation fold and the capacity refusal.
- Cases: six stocked systems — three NEC lanes {cu-thhn · cu-xhhw · al-xhhw} folding `Table310` through the metal and insulation delegates, two three-loaded IEC lanes {cu-pvc · al-pvc} folding `TableB52` through the basis method, and the two-loaded copper lane {cu-pvc-two} folding the B.52.2 estate — 99 conductors from 34 roster rows (18+18+17 NEC, 16+15 three-loaded, 15 two-loaded), each rating RESOLVED from its table by metal, size, and column pick, never asserted per stocked row; a size whose cell the basis does not publish mints nothing rather than a fabricated ampere.
- Law: a bag stamps ONE rating under ONE named basis — the `AmpacityBasis` token beside the ampere row is what keeps an NEC raceway cell from reading as an IEC reference-method cell; the full method axis stays a table read for any consumer needing another basis, and a second stocked basis is one `AmpacityBasis` row and one `WireSystem` row — the two-loaded B.52.2 lane landed exactly this way, with the LOADING riding the fold each system wires in `Rows`.
- Law: the TW and THW rows close the NEC column set as resolution arms — no TW/THW system is stocked, and stocking the legacy 60/75 °C lanes is one `WireSystem` row each with zero type edits.
- Law: the ampere dimension mints page-locally — the seam's `Dimension` roster names no current dimension yet, and the mint is the `Rasm.Element/Properties/quantity#DIMENSION` consumer-mint arm; the `CurrentDim` static homes on the seam roster (with its `A` symbol row) the moment a second consumer proves.
- Entry: `ElectricalSeed.Rows(context)` the `ComponentFamily.Electrical` row fold; `ElectricalSeed.Capacity` the typed refusal — a conductor's governing verdict is circuit-side (load, derating, voltage drop, protection), owned by `Rasm.Compute`, and the refusal names that route.
- Output: the projector derives the takeoff rows from the solved `Circle` section through `QuantityRow.VolumePerLength`/`SurfaceAreaPerLength`/`LinearDensity` (`component#QUANTITY_ROW`) — the area-true solid-equivalent section is what makes the copper and aluminium tonnage honest; no takeoff cell is stamped here.
- Boundary: every stamped bag rides `Attestation.Corroborated` because every cell that mints is two-sourced — the conflicted and single-posted cells are typed-absent in the tables and can never reach a bag, so the flag states transcription truth rather than table-wide optimism, per the `pipework#PIPEWORK_SEED` weakest-contributor law.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The installation basis a stamped rating names: the regional standard receipt (the NEC lane cites NFPA 70, the IEC
// lane IEC 60364-5-52 via its CENELEC HD adoption) and the reference-method read for the IEC lanes. A NEC basis
// carries no method — its table keys on the insulation temperature column instead — and the two-vs-three LOADING
// distinction between the two IEC rows rides the fold each system wires in Rows, the same way the pipework rosters
// select their folds.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AmpacityBasis {
    public static readonly AmpacityBasis Nec310 = new("nec-310-16",  standard: new ComponentStandard("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Nfpa), method: None);
    public static readonly AmpacityBasis IecC   = new("iec-b52-4-c", standard: new ComponentStandard("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Iec),  method: Some(IecMethod.C));
    public static readonly AmpacityBasis IecC2  = new("iec-b52-2-c", standard: new ComponentStandard("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Iec),  method: Some(IecMethod.C));

    public ComponentStandard Standard { get; }
    public Option<IecMethod> Method { get; }
}

// The stocked conductor systems: metal, insulation, and basis — three columns the folds resolve everything else
// from. Rated is the rating column's attestation (both ampacity estates are two-sourced, so every stocked row
// carries Corroborated; a future single-posted table stocks flagged, never dressed).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireSystem {
    public static readonly WireSystem CuThhn = new("cu-thhn",    metal: ConductorMetal.Copper,    insulation: InsulationClass.Thhn,  basis: AmpacityBasis.Nec310, rated: Attestation.Corroborated);
    public static readonly WireSystem CuXhhw = new("cu-xhhw",    metal: ConductorMetal.Copper,    insulation: InsulationClass.Xhhw,  basis: AmpacityBasis.Nec310, rated: Attestation.Corroborated);
    public static readonly WireSystem AlXhhw = new("al-xhhw",    metal: ConductorMetal.Aluminium, insulation: InsulationClass.Xhhw,  basis: AmpacityBasis.Nec310, rated: Attestation.Corroborated);
    public static readonly WireSystem CuPvc  = new("cu-pvc",     metal: ConductorMetal.Copper,    insulation: InsulationClass.Pvc70, basis: AmpacityBasis.IecC,   rated: Attestation.Corroborated);
    public static readonly WireSystem AlPvc  = new("al-pvc",     metal: ConductorMetal.Aluminium, insulation: InsulationClass.Pvc70, basis: AmpacityBasis.IecC,   rated: Attestation.Corroborated);
    public static readonly WireSystem CuPvc2 = new("cu-pvc-two", metal: ConductorMetal.Copper,    insulation: InsulationClass.Pvc70, basis: AmpacityBasis.IecC2,  rated: Attestation.Corroborated);

    public ConductorMetal Metal { get; }
    public InsulationClass Insulation { get; }
    public AmpacityBasis Basis { get; }
    public Attestation Rated { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The DetailLane.Product bag: size, insulation, and basis tokens, provenance and attestation, and the ONE measured
// ampacity under the page-local ampere dimension — the rating row the mandate's law makes a COMPONENT fact.
public static class ElectricalDetail {
    static readonly Dimension CurrentDim = Dimension.Create(0, 0, 0, 1, 0, 0, 0);   // ampere — the consumer-mint arm; homes on the seam roster when a second consumer proves
    static readonly PropertyName Ampacity = PropertyCategory.Materials.Row("Ampacity");

    public static Fin<PropertyBag> Of(WireSystem system, string size, double amps, Op key) =>
        from rating in ComponentDetail.Measured(Ampacity, CurrentDim, amps)
        select ComponentDetail.ProductRows([
            ComponentDetail.Token(DetailSchema.ConductorSize, size),
            ComponentDetail.Token(DetailSchema.InsulationClass, system.Insulation.Key),
            ComponentDetail.Token(DetailSchema.AmpacityBasis, system.Basis.Key),
            ComponentDetail.Sourced(Provenance.Published),
            SegmentRows.Attested(system.Rated),
            rating,
        ]);
}

// The ComponentFamily.Electrical generator: one Mint rail, three table folds, the system→table wiring in ONE Rows
// expression. Designation tags derive from the size identity ("1/0" -> 1-0, 1.5 mm² -> 0015) so the tag can never
// name a size the row does not carry, and the single-dot ComponentId grammar holds at every rung.
public static class ElectricalSeed {
    static readonly IfcBinding Conductor = IfcBinding.Of("IfcCableSegment", "CONDUCTORSEGMENT");

    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Seq(Nec(WireSystem.CuThhn, context),
            Nec(WireSystem.CuXhhw, context),
            Nec(WireSystem.AlXhhw, context),
            Iec(WireSystem.CuPvc, context),
            Iec(WireSystem.AlPvc, context),
            Two(WireSystem.CuPvc2, context))
        .Traverse(static fold => fold).As()
        .Map(static folds => folds.Bind(static rows => rows));

    // The ComponentFamily.Electrical CAPACITY producer: an explicit typed refusal — a conductor's governing verdict
    // is circuit-side (connected load, derating, voltage drop, protective-device coordination), owned by the
    // Rasm.Compute electrical route, and a section integral prices none of it.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        ComponentFault.Capacity(key, $"<electrical-sizing-rides-compute-circuit:{component.Designation.Value}>");

    static Fin<Seq<ComponentRow>> Nec(WireSystem system, Context context) =>
        toSeq(Table310.Rows)
            .Bind(row => system.Metal.NecCells(row)
                .Bind(cells => system.Insulation.Nec(cells.C60, cells.C75, cells.C90))
                .Map(amps => (row.Size, Amps: amps)).ToSeq())
            .Traverse(sized => Mint(system, sized.Size.Key, sized.Size.Tag, sized.Size.DiameterMm, sized.Amps, context)).As();

    static Fin<Seq<ComponentRow>> Iec(WireSystem system, Context context) =>
        from method in system.Basis.Method
            .ToFin(ComponentFault.Family(context.Key, $"<iec-fold-requires-method-basis:{system.Key}>"))
        from admitted in guard(system.Insulation.Pvc,
            ComponentFault.Family(context.Key, $"<iec-tables-are-pvc-70:{system.Insulation.Key}>"))
        from rows in system.Metal.IecRows()
            .Bind(row => method.Three(row).Map(amps => (row.Size, Amps: amps)).ToSeq())
            .Traverse(sized => Mint(system, sized.Size.Key, sized.Size.Tag, sized.Size.DiameterMm, sized.Amps, context)).As()
        select rows;

    // The two-loaded B.52.2 fold: copper-only by the table's own scope, and the 240 mm² method-A1 absence means an
    // A1-basis stocking would mint 14 rows rather than a fabricated cell — the conflict stays unstampable.
    static Fin<Seq<ComponentRow>> Two(WireSystem system, Context context) =>
        from method in system.Basis.Method
            .ToFin(ComponentFault.Family(context.Key, $"<iec-fold-requires-method-basis:{system.Key}>"))
        from admitted in guard(system.Insulation.Pvc,
            ComponentFault.Family(context.Key, $"<iec-tables-are-pvc-70:{system.Insulation.Key}>"))
        from copper in guard(system.Metal == ConductorMetal.Copper,
            ComponentFault.Family(context.Key, $"<b52-2-publishes-copper-only:{system.Key}>"))
        from rows in toSeq(TableB52.Two)
            .Bind(row => method.Two(row).Map(amps => (row.Size, Amps: amps)).ToSeq())
            .Traverse(sized => Mint(system, sized.Size.Key, sized.Size.Tag, sized.Size.DiameterMm, sized.Amps, context)).As()
        select rows;

    static Fin<ComponentRow> Mint(WireSystem system, string size, string tag, double diameterMm, double amps, Context context) =>
        from profile in SectionProfile.Circle.Of(diameterMm, context.Key)
        from detail in ElectricalDetail.Of(system, size, amps, context.Key)
        from item in Component.Of(
            ComponentFamily.Electrical, $"electrical.{system.Key}-{tag}",
            profile, Conductor, Coring.None, system.Basis.Standard,
            substanceId: system.Metal.Substance, appearanceId: system.Metal.Appearance,
            detail: Some(detail), context.Key)
        select new ComponentRow(item, Provenance.Published);
}
```

## [05]-[RESEARCH]

(none)
