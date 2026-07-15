# [MATERIALS_FASTENER]

THE FASTENER SEED PAGE owns the `ComponentFamily.Fastener` fold and the single-fastener capacity and assembly algebra. `StockRow.Threaded` carries `ThreadRow` and `GradeRow`; `StockRow.Plain` carries published nail, dowel, and rivet data without a fake thread or bolt grade. Both cases project through `SelectedKind`, `StockDesignation`, `NominalDiameterMm`, `StockLengthMm`, `StockStandard`, `StockSubstance`, and `StockAppearance`, so geometry, IFC binding, realization detail, and `Component.Of` share one fold while case-specific admission remains total.

## [01]-[INDEX]

- [02]-[FASTENER_FAMILY]: the `FastenerKind`/`ThreadSeries`/`BoltCategory`/`FayingSurface` policy vocabularies, the `HexHardware` envelope, the `ThreadRow`/`GradeRow` frozen standards tables with `Threads`/`Grades` owners, the `Fastening` single-bolt capacity and thread-split algebra, the `FastenerDetail` realization bag, and the `FastenerSeed.Rows` typed-selection generator over the `StockRow` symbolic rows.
- [03]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — bolt + grip-plies (`Dimension`) + shear-planes + nut + washer over one `(ThreadRow, GradeRow, BoltCategory, FayingSurface)`, the `PreloadKn` `Fp,C = 0.7·fub·As` projection, the `FastenerInstallation` admitted slip-and-torque factor set, the `SlipResistanceKn` EN 1993-1-8 preloaded design value, and the ISO 7089/7090 washer-hardness selection.

## [02]-[FASTENER_FAMILY]

- Owner: `FastenerSeed` owns the `ComponentFamily.Fastener` row fold; `Threads` and `Grades` own frozen standards data; `FastenerKind` owns the complete IFC entity/token binding for nut and every mechanical kind, including nail; `BoltCategory`, `FayingSurface`, and `ThreadSeries` own policy; `Fastening` owns single-fastener design values; `FastenerAssembly` owns installed-bolt state; and `FastenerDetail` owns the realization bag.
- Cases: kind {`bolt` · `nut` · `nail` · `screw` · `anchor` · `dowel` · `rivet` · `coupler`} × stock form {threaded hardware over a `ThreadRow`/`GradeRow` pair · plain shank over its published designation, diameter, length, standard, and material pair}; the joint category is a `FastenerAssembly` decision, never a type-row column.
- Entry: `FastenerSeed.Rows(context)` traverses the typed `Stocked` selection, dispatches `StockRow.Admit`, and feeds the common `StockDesignation`/`NominalDiameterMm`/`StockLengthMm`/material projections into `Component.Of`; `Fastening` owns the threaded-hardware capacity and ISO 4014 length algebra.
- Packages: Rasm.Numerics (`Dimension` — the `[03]` discrete grip-ply/shear-plane counts), Rasm.Domain (`Op`/`Context`/`AcceptValidated`), Rasm.Element (`MaterialId`, `DetailSchema`, `PropertyBag`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile.Circle.Of` the railed profile admission/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the four retained policy vocabularies), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`.As()`/`guard`/`Option`), VividOrange.Standards (`En1993`/`En1993Part.Part1_8`/`NationalAnnex` — the typed joints-code citation on `BoltCategory`), BCL (`ImmutableArray`). No bolt-grade producer exists among admitted packages (`VividOrange.Materials` `EnSteelGrade` is EN member-grade data, no ISO 898-1/SAE/ASTM bolt classes), so the rows are AUTHORED/PUBLISHED here.
- Growth: a new threaded combination is one `StockRow.Threaded`; a new plain-shank product one `StockRow.Plain`; a new kind one `FastenerKind` row plus its appropriate stock case; a new thread or property class one named table row; and a new connection category one `BoltCategory` row.
- Boundary: every fastener uses `SectionProfile.Circle` and the seed-built realization bag. Thread semantics and `GradeRow` material data exist only on `StockRow.Threaded`; `StockRow.Plain` carries its own published diameter, length, standard, and independent substance/appearance pair. IFC tokens remain portable egress hints validated by `Rasm.Bim`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                     // Dimension (the [03] discrete grip-ply/shear-plane carrier)
using Rasm.Domain;                      // Op, Context, AcceptValidated
using Rasm.Element.Composition;                     // MaterialId, DetailSchema, PropertyBag
using Rasm.Element.Properties;
using Thinktecture;
using VividOrange.Standards.Eurocode;   // En1993, En1993Part, NationalAnnex — the typed EN 1993-1-8 joints citation
using SiDim = Rasm.Element.Properties.Dimension;   // the SI-dimension axis the detail-bag mints ride — aliased clear of the Rasm.Numerics discrete-count Dimension
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The kind axis of the seed generator: the member-type vocabulary owning the COMPLETE entity-token binding
// (POLICY_VALUES — entity selection is a row read, never reconstructed at the seed). The verified GeometryGym
// IfcMechanicalFastenerTypeEnum carries BOLT/SCREW/NAIL/ANCHORBOLT/DOWEL/RIVET/COUPLER and NO NUT member, so the nut
// ROW binds IfcDiscreteAccessory/USERDEFINED — the entity split is vocabulary data, not a seed special case. The form
// flags drive the Fastening thread-split; anchor/dowel/rivet/coupler are arms HERE — ComponentFamily stays closed at ten.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerKind {
    public static readonly FastenerKind Bolt    = new("bolt",    ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "BOLT",        threaded: true,  headed: true);
    public static readonly FastenerKind Nut     = new("nut",     ifcEntity: "IfcDiscreteAccessory",  ifcPredefinedType: "USERDEFINED", threaded: true,  headed: false);
    public static readonly FastenerKind Nail    = new("nail",    ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "NAIL",        threaded: false, headed: true);
    public static readonly FastenerKind Screw   = new("screw",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "SCREW",       threaded: true,  headed: true);
    public static readonly FastenerKind Anchor  = new("anchor",  ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "ANCHORBOLT",  threaded: true,  headed: true);
    public static readonly FastenerKind Dowel   = new("dowel",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "DOWEL",       threaded: false, headed: false);
    public static readonly FastenerKind Rivet   = new("rivet",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "RIVET",       threaded: false, headed: true);
    public static readonly FastenerKind Coupler = new("coupler", ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "COUPLER",     threaded: true,  headed: false);
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public bool Threaded { get; }   // a dowel/rivet has no thread — ThreadLengthMm resolves 0, the body is all shank
    public bool Headed { get; }     // a headless threaded part (nut/coupler) threads through its whole length
}

// The coarse-vs-fine pitch family; a fine row keys its own pitch/minor under the existing series, never a parallel axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSeries {
    public static readonly ThreadSeries MetricCoarse  = new("metric-coarse", metric: true);
    public static readonly ThreadSeries MetricFine    = new("metric-fine",   metric: true);
    public static readonly ThreadSeries UnifiedCoarse = new("unc",           metric: false);
    public static readonly ThreadSeries UnifiedFine   = new("unf",           metric: false);
    public bool Metric { get; }
}

// The EN 1993-1-8 Table 3.2 joint category — the bearing-vs-preloaded axis a CONNECTION selects, never a type-row
// column. Joints is the ONE typed design-code citation (Part 1-8, recommended values) shared by every category row —
// the re-transcribed clause string is the deleted form. Preloaded gates the [03] slip projection; a non-preloadable
// grade in a B/C/E joint rails ComponentFault.Grade at FastenerAssembly.Of.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltCategory {
    public static readonly En1993 Joints = new(En1993Part.Part1_8, NationalAnnex.RecommendedValues);

    public static readonly BoltCategory A = new("A", shear: true,  preloaded: false);
    public static readonly BoltCategory B = new("B", shear: true,  preloaded: true);
    public static readonly BoltCategory C = new("C", shear: true,  preloaded: true);
    public static readonly BoltCategory D = new("D", shear: false, preloaded: false);
    public static readonly BoltCategory E = new("E", shear: false, preloaded: true);
    public bool Shear { get; }       // a shear category reads ShearCapacityKn/BearingCapacityKn; a tension category TensileLoadKn
    public bool Preloaded { get; }   // a preloaded category requires a Preloadable grade + a FayingSurface slip class
}

// The EN 1993-1-8 §3.9 / RCSC slip-factor class μ a preloaded joint relies on; None (μ = 0) is the bearing-joint row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FayingSurface {
    public static readonly FayingSurface None   = new("none",    slipFactor: 0.00);
    public static readonly FayingSurface ClassA = new("class-a", slipFactor: 0.50);   // blasted, loose rust removed
    public static readonly FayingSurface ClassB = new("class-b", slipFactor: 0.40);   // blasted + alkali-zinc-silicate coat
    public static readonly FayingSurface ClassC = new("class-c", slipFactor: 0.30);   // wire-brushed / galvanized + roughened
    public static readonly FayingSurface ClassD = new("class-d", slipFactor: 0.20);   // untreated
    public double SlipFactor { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShearPlane {
    const double ShankAlphaV = 0.60;
    public static readonly ShearPlane Threaded = new("threaded", static (thread, grade) => grade.ShearStrengthFactor * thread.StressAreaMm2);
    public static readonly ShearPlane Shank = new("shank", static (thread, _) => ShankAlphaV * thread.NominalAreaMm2);
    [UseDelegateFromConstructor] public partial double ResistanceAreaFactor(ThreadRow thread, GradeRow grade);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-size ISO hex hardware envelope: ISO 4014 head (k / dw product-class-B washer face / da fillet), ISO 4032
// style-1 nut height m (across-flats is the thread's s), ISO 7089 plain washer (d1/d2/t). Some for metric coarse rows,
// None for inch UNC rows whose ASME B18.2 envelope is the open research row.
public readonly record struct HexHardware(
    double HeadHeightMm, double BearingDiameterMm, double FilletDiameterMm,
    double NutHeightMm, double WasherInnerMm, double WasherOuterMm, double WasherThicknessMm);

// ISO 261/724 + ASME B1.1 thread row — VALUES verbatim, per-column provenance: MajorMm/PitchMm/MinorMm (the ISO 724 /
// ASME basic minor d1) and StressAreaMm2 (the PRINTED ISO 898-1 / ASME B1.1 tensile stress area, stored verbatim and
// NEVER re-derived — the two standards' formulas disagree ~3%) are PUBLISHED; the thread form (d2/d3/H/e, gross shank
// area) is DEFINED — the standard's own formula as an expression-bodied derivation. Tag is the designation token for
// inch rows ("3/8" -> "0375"); Key doubles as the token for metric rows.
public readonly record struct ThreadRow(
    string Key, ThreadSeries Series, double MajorMm, double PitchMm, double MinorMm, double StressAreaMm2,
    double AcrossFlatsMm, Option<HexHardware> Hardware = default, Option<string> Tag = default) {

    public const double FlankAngleDeg = 60.0;              // ISO 68-1 / ASME B1.1 included angle
    public const string ExternalToleranceClass = "6g";     // ISO 965 medium external (bolt) fit
    public const string InternalToleranceClass = "6H";     // ISO 965 medium internal (nut) fit

    public string Designation => Tag.IfNone(Key);
    public double PitchDiameterMm => MajorMm - 0.649519 * PitchMm;       // DEFINED: ISO 724 d2
    public double RootMinorMm => MajorMm - 1.226869 * PitchMm;           // DEFINED: ISO 898-1 rounded-root d3 (the solid-model root)
    public double FundamentalHeightMm => 0.866025 * PitchMm;             // DEFINED: ISO 68-1 H
    public double AcrossCornersMm => AcrossFlatsMm * 2.0 / 1.7320508;    // DEFINED: e = s·2/√3
    public double NominalAreaMm2 => Math.PI / 4.0 * MajorMm * MajorMm;   // DEFINED: gross shank area (shank-in-shear-plane)
}

// The >threshold mechanical step a size-banded class carries (ISO 898-1 bands 8.8 above M16; ASTM F3125 unified the
// old A325 over-1in reduction away, so no inch row steps) — None for the single-band classes.
public readonly record struct GradeStep(double AboveMm, double ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa);

// ISO 898-1 / SAE J429 / ASTM F3125 grade row — the Table 3 MIN-column proof/tensile/yield PUBLISHED verbatim (base
// columns the ≤threshold band; Step the >threshold band, selected by At over the thread's major diameter so an m20
// 8.8 reads 600/830/660 and an m12 8.8 580/800/640 — never a hybrid row); ShearStrengthFactor the EN 1993-1-8
// Table 3.4 THREADED-plane αv (0.5 for the reducing 4.8/5.8/6.8/10.9 classes, 0.6 otherwise — the shank plane is 0.6
// for EVERY grade, owned by Fastening); Preloadable the §3.9 / RCSC slip-critical admission (8.8/10.9/A325/A490
// only); Standard the ComponentStandard citation; Admits the SYSTEM exclusion — an ISO class pairs metric threads,
// an SAE/ASTM grade inch threads. Tag is the dotless designation token ("8.8" -> "88").
public readonly record struct GradeRow(
    string Key, string Spec, double ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa,
    double ShearStrengthFactor, bool Preloadable, bool Metric, ComponentStandard Standard,
    string SubstanceId, string AppearanceId, Option<GradeStep> Step = default) {

    public string Tag => Key.Replace(".", "");
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
    public bool Admits(ThreadRow thread) => Metric == thread.Series.Metric;

    // The effective mechanical triple at a thread size — the ONE band read every capacity projection routes through.
    public (double ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa) At(ThreadRow thread) =>
        Step.Filter(s => thread.MajorMm > s.AboveMm)
            .Map(static s => (s.ProofStressMpa, s.TensileStrengthMpa, s.MinimumYieldMpa))
            .IfNone((ProofStressMpa, TensileStrengthMpa, MinimumYieldMpa));
}

// --- [TABLES] ------------------------------------------------------------------------------
// 17 thread rows: 9 ISO 261 metric coarse (hardware Some) + 8 ASME B1.1 UNC (hardware None, Tag the decimal token).
// NAMED statics (the connector Gauges form) so the Stocked selection references rows SYMBOLICALLY — a typo'd size
// is a compile miss, never a runtime key.
public static class Threads {
    public static readonly ThreadRow M6     = new("m6",    ThreadSeries.MetricCoarse,  6.000, 1.000,  4.917,  20.1, 10.000, Some(new HexHardware(4.0,  8.74,  6.8,  5.2,  6.4,  12.0, 1.6)));
    public static readonly ThreadRow M8     = new("m8",    ThreadSeries.MetricCoarse,  8.000, 1.250,  6.647,  36.6, 13.000, Some(new HexHardware(5.3,  11.47, 9.2,  6.8,  8.4,  16.0, 1.6)));
    public static readonly ThreadRow M10    = new("m10",   ThreadSeries.MetricCoarse, 10.000, 1.500,  8.376,  58.0, 16.000, Some(new HexHardware(6.4,  14.47, 11.2, 8.4,  10.5, 20.0, 2.0)));
    public static readonly ThreadRow M12    = new("m12",   ThreadSeries.MetricCoarse, 12.000, 1.750, 10.106,  84.3, 18.000, Some(new HexHardware(7.5,  16.47, 13.7, 10.8, 13.0, 24.0, 2.5)));
    public static readonly ThreadRow M16    = new("m16",   ThreadSeries.MetricCoarse, 16.000, 2.000, 13.835, 157.0, 24.000, Some(new HexHardware(10.0, 22.00, 17.7, 14.8, 17.0, 30.0, 3.0)));
    public static readonly ThreadRow M20    = new("m20",   ThreadSeries.MetricCoarse, 20.000, 2.500, 17.294, 245.0, 30.000, Some(new HexHardware(12.5, 27.70, 22.4, 18.0, 21.0, 37.0, 3.0)));
    public static readonly ThreadRow M24    = new("m24",   ThreadSeries.MetricCoarse, 24.000, 3.000, 20.752, 353.0, 36.000, Some(new HexHardware(15.0, 33.25, 26.4, 21.5, 25.0, 44.0, 4.0)));
    public static readonly ThreadRow M30    = new("m30",   ThreadSeries.MetricCoarse, 30.000, 3.500, 26.211, 561.0, 46.000, Some(new HexHardware(18.7, 42.75, 33.4, 25.6, 31.0, 56.0, 4.0)));
    public static readonly ThreadRow M36    = new("m36",   ThreadSeries.MetricCoarse, 36.000, 4.000, 31.670, 817.0, 55.000, Some(new HexHardware(22.5, 51.11, 39.4, 31.0, 37.0, 66.0, 5.0)));
    public static readonly ThreadRow In0250 = new("1/4",   ThreadSeries.UnifiedCoarse,  6.350, 1.2700,  4.976,  20.5, 11.113, None, "0250");
    public static readonly ThreadRow In0375 = new("3/8",   ThreadSeries.UnifiedCoarse,  9.525, 1.5875,  7.805,  50.0, 14.288, None, "0375");
    public static readonly ThreadRow In0500 = new("1/2",   ThreadSeries.UnifiedCoarse, 12.700, 1.9538, 10.585,  91.5, 19.050, None, "0500");
    public static readonly ThreadRow In0625 = new("5/8",   ThreadSeries.UnifiedCoarse, 15.875, 2.3091, 13.375, 145.8, 23.813, None, "0625");
    public static readonly ThreadRow In0750 = new("3/4",   ThreadSeries.UnifiedCoarse, 19.050, 2.5400, 16.300, 215.5, 28.575, None, "0750");
    public static readonly ThreadRow In0875 = new("7/8",   ThreadSeries.UnifiedCoarse, 22.225, 2.8222, 19.170, 298.1, 33.338, None, "0875");
    public static readonly ThreadRow In1000 = new("1",     ThreadSeries.UnifiedCoarse, 25.400, 3.1750, 21.963, 391.0, 38.100, None, "1000");
    public static readonly ThreadRow In1500 = new("1-1/2", ThreadSeries.UnifiedCoarse, 38.100, 4.2333, 33.517, 906.5, 57.150, None, "1500");
    public static readonly ImmutableArray<ThreadRow> Rows = [M6, M8, M10, M12, M16, M20, M24, M30, M36, In0250, In0375, In0500, In0625, In0750, In0875, In1000, In1500];
}

// 13 grade rows: 8 ISO 898-1 property classes (EN ISO citation) + 3 SAE J429 + 2 ASTM F3125 (US citation). Base
// columns are the Table 3 MIN values for the ≤threshold band; 8.8 is the one rostered class that steps (>M16).
// Named statics for the same symbolic-selection law the thread table carries.
public static class Grades {
    static readonly ComponentStandard EnIso = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    static readonly ComponentStandard Us    = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    public static readonly GradeRow G46  = new("4.6",  "ISO 898-1",  225.0,  400.0,  240.0, 0.60, false, true,  EnIso, "steel.fastener-4_6",  "metal.iron");
    public static readonly GradeRow G48  = new("4.8",  "ISO 898-1",  310.0,  420.0,  340.0, 0.50, false, true,  EnIso, "steel.fastener-4_8",  "metal.iron");
    public static readonly GradeRow G56  = new("5.6",  "ISO 898-1",  280.0,  500.0,  300.0, 0.60, false, true,  EnIso, "steel.fastener-5_6",  "metal.iron");
    public static readonly GradeRow G58  = new("5.8",  "ISO 898-1",  380.0,  520.0,  420.0, 0.50, false, true,  EnIso, "steel.fastener-5_8",  "metal.iron");
    public static readonly GradeRow G68  = new("6.8",  "ISO 898-1",  440.0,  600.0,  480.0, 0.50, false, true,  EnIso, "steel.fastener-6_8",  "metal.iron");
    public static readonly GradeRow G88  = new("8.8",  "ISO 898-1",  580.0,  800.0,  640.0, 0.60, true,  true,  EnIso, "steel.fastener-8_8",  "metal.steel", Some(new GradeStep(16.0, 600.0, 830.0, 660.0)));
    public static readonly GradeRow G109 = new("10.9", "ISO 898-1",  830.0, 1040.0,  940.0, 0.50, true,  true,  EnIso, "steel.fastener-10_9", "metal.steel");
    public static readonly GradeRow G129 = new("12.9", "ISO 898-1",  970.0, 1220.0, 1100.0, 0.50, false, true,  EnIso, "steel.fastener-12_9", "metal.steel");
    public static readonly GradeRow Gr2  = new("gr2",  "SAE J429",   379.0,  510.0,  393.0, 0.60, false, false, Us,    "steel.fastener-gr2",  "metal.iron");
    public static readonly GradeRow Gr5  = new("gr5",  "SAE J429",   585.0,  827.0,  634.0, 0.60, false, false, Us,    "steel.fastener-gr5",  "metal.steel");
    public static readonly GradeRow Gr8  = new("gr8",  "SAE J429",   830.0, 1034.0,  896.0, 0.50, false, false, Us,    "steel.fastener-gr8",  "metal.steel");
    public static readonly GradeRow A325 = new("a325", "ASTM F3125", 585.0,  827.0,  634.0, 0.60, true,  false, Us,    "steel.fastener-a325", "metal.steel");
    public static readonly GradeRow A490 = new("a490", "ASTM F3125", 830.0, 1040.0,  940.0, 0.50, true,  false, Us,    "steel.fastener-a490", "metal.steel");
    public static readonly ImmutableArray<GradeRow> Rows = [G46, G48, G56, G58, G68, G88, G109, G129, Gr2, Gr5, Gr8, A325, A490];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The single-bolt design-value and length-split algebra over (ThreadRow, GradeRow) — the receipts Rasm.Compute reads
// off the seam for group resistance and combined shear+tension interaction, never re-derived there.
public static class Fastening {
    public static double ProofLoadKn(ThreadRow thread, GradeRow grade) => grade.At(thread).ProofStressMpa * thread.StressAreaMm2 * 1e-3;
    public static double TensileLoadKn(ThreadRow thread, GradeRow grade) => grade.At(thread).TensileStrengthMpa * thread.StressAreaMm2 * 1e-3;

    public static double ShearCapacityKn(ThreadRow thread, GradeRow grade, ShearPlane plane) =>
        grade.At(thread).TensileStrengthMpa * plane.ResistanceAreaFactor(thread, grade) * 1e-3;

    // Fb = k1·αb·fu,ply·d·t — edge/end/pitch geometry collapsed into the caller-supplied edgeFactor, the ply ultimate
    // strength supplied by the properties Mechanical receipt, never re-keyed here.
    public static double BearingCapacityKn(ThreadRow thread, BearingDesign design) =>
        design.EdgeFactor * design.PlyUltimateMpa * thread.MajorMm * design.PlyThicknessMm * 1e-3;

    // ISO 4014 reference thread length b = 2d+6 (L ≤ 125) / 2d+12 (125 < L ≤ 200) / 2d+25 (L > 200), clamped to L for
    // a short fully-threaded bolt; a headless threaded part (nut/coupler) threads its whole length; a dowel/rivet is
    // all shank. The runout is the ISO 3508 incomplete-thread allowance x ≤ 2.5·P; the shank diameter is the nominal d.
    public static double ThreadLengthMm(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        !kind.Threaded ? 0.0
        : !kind.Headed ? lengthMm
        : Math.Min(lengthMm, lengthMm <= 125.0 ? 2.0 * thread.MajorMm + 6.0
            : lengthMm <= 200.0 ? 2.0 * thread.MajorMm + 12.0 : 2.0 * thread.MajorMm + 25.0);
    public static double UnthreadedShankMm(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        lengthMm - ThreadLengthMm(kind, thread, lengthMm);
    public static double ThreadRunoutMm(ThreadRow thread) => 2.5 * thread.PitchMm;
}

// The seed-time DetailLane.Realization bag — rows byte-identical to the retired projector switch (FastenerType token,
// dimension-only SI mints for NominalDiameter/NominalLength) so projected Node.PropertySet content keys are unchanged.
public static class FastenerDetail {
    public static Fin<PropertyBag> Of(FastenerKind kind, double diameterMm, double lengthMm) =>
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, SiDim.LengthDim, diameterMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.NominalLength, SiDim.LengthDim, lengthMm * 1e-3)
        select ComponentDetail.RealizationRows(
            ComponentDetail.Token(DetailSchema.FastenerType, kind.IfcPredefinedType),
            diameter,
            length);
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// Threaded rows reference thread and grade currencies symbolically; plain rows carry only shank facts.
[Union]
public abstract partial record StockRow {
    private StockRow() { }
    public sealed record Threaded(FastenerKind Kind, ThreadRow Thread, GradeRow Grade, double LengthMm) : StockRow;
    public sealed record Plain(
        FastenerKind Kind, string Designation, double DiameterMm, double LengthMm,
        ComponentStandard Standard, MaterialId Substance, MaterialId Appearance) : StockRow;

    public FastenerKind SelectedKind => Switch(threaded: static row => row.Kind, plain: static row => row.Kind);
    public string StockDesignation => Switch(
        threaded: static row => $"{row.Thread.Designation}-{row.Grade.Tag}",
        plain: static row => row.Designation);
    public double NominalDiameterMm => Switch(threaded: static row => row.Thread.MajorMm, plain: static row => row.DiameterMm);
    public double StockLengthMm => Switch(threaded: static row => row.LengthMm, plain: static row => row.LengthMm);
    public ComponentStandard StockStandard => Switch(threaded: static row => row.Grade.Standard, plain: static row => row.Standard);
    public MaterialId StockSubstance => Switch(threaded: static row => row.Grade.Substance, plain: static row => row.Substance);
    public MaterialId StockAppearance => Switch(threaded: static row => row.Grade.Appearance, plain: static row => row.Appearance);
    public Fin<Unit> Admit(Op key) => Switch(
        threaded: row => !(row.Kind.Threaded && row.Grade.Admits(row.Thread))
            ? Fin.Fail<Unit>(ComponentFault.Grade(key, $"<threaded-stock-mismatch:{row.Kind.Key}:{row.Thread.Key}:{row.Grade.Key}>"))
            : !(double.IsFinite(row.LengthMm) && row.LengthMm > 0.0)
                ? Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<threaded-stock-length-invalid:{row.Kind.Key}:{row.Thread.Key}:{row.LengthMm:R}>"))
                : Fin.Succ(unit),
        plain: row => !row.Kind.Threaded && double.IsFinite(row.DiameterMm) && row.DiameterMm > 0.0
                && double.IsFinite(row.LengthMm) && row.LengthMm > 0.0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(ComponentFault.Dimension(key, $"<plain-stock-invalid:{row.Kind.Key}:{row.Designation}>")));
}

// The ONE generator traverses both stock cases through the same admission and construction rail.
public static class FastenerSeed {
    static readonly ImmutableArray<StockRow> Stocked = [
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M12,    Grades.G88,  60.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M16,    Grades.G88,  80.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M16,    Grades.G109, 80.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M20,    Grades.G88,  90.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M20,    Grades.G109, 90.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M24,    Grades.G109, 110.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.M30,    Grades.G129, 140.0),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0375, Grades.Gr5,  63.5),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0500, Grades.Gr5,  76.2),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0750, Grades.Gr8,  101.6),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0875, Grades.A325, 114.3),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0875, Grades.A490, 114.3),
        new StockRow.Threaded(FastenerKind.Nut,     Threads.M16,    Grades.G88,  14.8),
        new StockRow.Threaded(FastenerKind.Nut,     Threads.M20,    Grades.G109, 18.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.M8,     Grades.G88,  40.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.In0250, Grades.Gr2,  31.8),
        new StockRow.Threaded(FastenerKind.Coupler, Threads.M20,    Grades.G88,  60.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.M16,    Grades.G88,  200.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.M20,    Grades.G88,  250.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.In0750, Grades.A325, 304.8),
        new StockRow.Plain(FastenerKind.Nail,  "8d-common",   3.33, 63.5,  new ComponentStandard("us", 0.0, ComponentAuthority.Astm), MaterialId.Of("steel.fastener-nail"),  MaterialId.Of("metal.iron")),
        new StockRow.Plain(FastenerKind.Nail,  "10d-common",  3.76, 76.2,  new ComponentStandard("us", 0.0, ComponentAuthority.Astm), MaterialId.Of("steel.fastener-nail"),  MaterialId.Of("metal.iron")),
        new StockRow.Plain(FastenerKind.Dowel, "dowel-20",   20.0, 100.0, new ComponentStandard("eu", 0.0, ComponentAuthority.En),   MaterialId.Of("steel.fastener-dowel"), MaterialId.Of("metal.steel")),
        new StockRow.Plain(FastenerKind.Rivet, "rivet-0500", 12.7, 38.1,  new ComponentStandard("us", 0.0, ComponentAuthority.Astm), MaterialId.Of("steel.fastener-rivet"), MaterialId.Of("metal.iron"))];

    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        Stocked.ToSeq().Traverse(row =>
            from admitted in row.Admit(context.Key)
            from profile in SectionProfile.Circle.Of(row.NominalDiameterMm, context.Key)
            from detail in FastenerDetail.Of(row.SelectedKind, row.NominalDiameterMm, row.StockLengthMm)
            from item in Component.Of(
                ComponentFamily.Fastener, $"fastener.{row.SelectedKind.Key}-{row.StockDesignation}",
                profile,
                IfcBinding.Of(row.SelectedKind.IfcEntity, row.SelectedKind.IfcPredefinedType),
                Coring.None, row.StockStandard, substanceId: row.StockSubstance, appearanceId: row.StockAppearance,
                detail: Some(detail), context.Key)
            select new ComponentRow(item, Sectioned: false)).As();
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` owns the installed bolt state, and `FastenerInstallation` admits the shared `(ks, γM3, km)` slip-and-torque policy. `ShearPlane` owns the threaded-versus-shank resistance projection; no boolean policy knob reaches `Fastening.ShearCapacityKn`.
- Cases: one assembly shape for every modality — a non-preloaded (A/D) assembly resolves `FayingSurface.None` and returns `None` for preload, slip, and tightening torque; a preloaded (B/C/E) assembly requires a named slip class and returns `Some` design values — never a numeric absence sentinel and never a `PreloadedBolt`/`BearingBolt` pair.
- Entry: `FastenerAssembly.Of(thread, grade, category, faying, gripPlies, shearPlanes, withWasher, key)` rails a system-mismatched thread/grade pair, a preloaded category over a non-`Preloadable` grade, a preloaded category with `FayingSurface.None`, and non-positive discrete counts before constructing the one assembly owner.
- Growth: a new connection modality is a `BoltCategory`/`FayingSurface` row the assembly reads; the multi-bolt group `ΣFs,Rd`, the long-joint `β`, and the `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` interaction are `Rasm.Compute` consumers over these single-bolt receipts.
- Boundary: `Dimension` admits the discrete grip and shear-plane counts. `FastenerInstallation.Of` admits the hole, partial, and torque factors once, and `ShearPlane` makes the resistance-area policy explicit in the input shape.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The EN 1993-1-8 §3.9 / EN 1090-2 §8.5 installation design set admitted ONCE: ks the hole-tolerance factor, γM3 the
// slip partial factor, km the manufacturer-declared EN 14399-2 k-class torque factor. The generated validation owns
// the positive-finite guard, so no raw factor reaches a slip or torque projection and a bearing-joint zero stays
// distinguishable from invalid design input — the silent 0.0-on-bad-γM3 sentinel is the deleted form.
[ComplexValueObject]
public readonly partial struct BearingDesign {
    public double PlyThicknessMm { get; }
    public double PlyUltimateMpa { get; }
    public double EdgeFactor { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double plyThicknessMm, ref double plyUltimateMpa, ref double edgeFactor) =>
        validationError = double.IsFinite(plyThicknessMm) && plyThicknessMm > 0.0
            && double.IsFinite(plyUltimateMpa) && plyUltimateMpa > 0.0
            && double.IsFinite(edgeFactor) && edgeFactor > 0.0
            ? null
            : new ValidationError($"<bearing-design-invalid:t={plyThicknessMm:R}:fu={plyUltimateMpa:R}:edge={edgeFactor:R}>");

    public static Fin<BearingDesign> Of(double plyThicknessMm, double plyUltimateMpa, double edgeFactor, Op key) =>
        Validate(plyThicknessMm, plyUltimateMpa, edgeFactor, out BearingDesign design) is { } error
            ? Fin.Fail<BearingDesign>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(design);
}

[ComplexValueObject]
public readonly partial struct FastenerInstallation {
    public double Ks { get; }
    public double GammaM3 { get; }
    public double Km { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double ks, ref double gammaM3, ref double km) =>
        validationError = double.IsFinite(ks) && ks > 0.0 && double.IsFinite(gammaM3) && gammaM3 > 0.0 && double.IsFinite(km) && km > 0.0
            ? null
            : new ValidationError($"<fastener-installation-invalid:ks={ks:R}:gammaM3={gammaM3:R}:km={km:R}>");

    public static Fin<FastenerInstallation> Of(double ks, double gammaM3, double km, Op key) =>
        Validate(ks, gammaM3, km, out FastenerInstallation design) is { } error
            ? Fin.Fail<FastenerInstallation>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(design);
}

// The complete bolt-connection receipt over the standards rows: preload/slip/washer projections are the EN 1993-1-8
// §3.9 single-bolt design values the Rasm.Compute slip-critical and combined-action checks read off the seam.
public readonly record struct FastenerAssembly(
    ThreadRow Thread, GradeRow Grade, BoltCategory Category, FayingSurface Faying,
    Dimension GripPlies, Dimension ShearPlanes, bool WithWasher) {

    public static Fin<FastenerAssembly> Of(
        ThreadRow thread, GradeRow grade, BoltCategory category, FayingSurface faying,
        int gripPlies, int shearPlanes, bool withWasher, Op key) =>
        from system in guard(grade.Admits(thread), ComponentFault.Grade(key, $"<system-mismatched-assembly:{thread.Key}:{grade.Key}>"))
        from preload in guard(!category.Preloaded || grade.Preloadable, ComponentFault.Grade(key, $"<non-preloadable-grade-in-preloaded-joint:{grade.Key}:{category.Key}>"))
        from surface in guard(!category.Preloaded || faying != FayingSurface.None, ComponentFault.Dimension(key, $"<preloaded-joint-without-faying-class:{category.Key}>"))
        from plies in key.AcceptValidated<Dimension>(candidate: gripPlies)
        from planes in key.AcceptValidated<Dimension>(candidate: shearPlanes)
        select new FastenerAssembly(thread, grade, category, category.Preloaded ? faying : FayingSurface.None, plies, planes, withWasher);

    // Fp,C = 0.7·fub·As over the size-banded At read.
    public Option<double> PreloadKn => Category.Preloaded
        ? Some(0.7 * Grade.At(Thread).TensileStrengthMpa * Thread.StressAreaMm2 * 1e-3)
        : None;

    public Option<double> SlipResistanceKn(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Ks * ShearPlanes.Value * Faying.SlipFactor * preload / design.GammaM3);

    public Option<double> TighteningTorqueNm(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Km * (Thread.MajorMm * 1e-3) * (preload * 1e3));

    // ISO 7090 300 HV (chamfered, preloaded high-strength) vs ISO 7089 200 HV (plain) washer hardness.
    public double WasherHardnessHv => Grade.Preloadable ? 300.0 : 200.0;

    public Option<double> NutHeightMm => Thread.Hardware.Map(static h => h.NutHeightMm);
    public Option<double> WasherOuterMm => WithWasher ? Thread.Hardware.Map(static h => h.WasherOuterMm) : None;
    public Option<double> WasherThicknessMm => WithWasher ? Thread.Hardware.Map(static h => h.WasherThicknessMm) : None;
}
```

## [04]-[RESEARCH]

- [SEED_GENERATOR]: `StockRow.Threaded` preserves symbolic thread/grade selection and its system guard; `StockRow.Plain` admits nails, dowels, and rivets without fake thread or bolt-grade semantics. Both cases traverse the same `Component.Of` rail and a malformed row aborts the catalogue.
- [ROW_TABLE_CONVERSION]: REALIZED — `ThreadSize`(17) and `FastenerGrade`(13) convert 1:1 to the frozen `ThreadRow`/`GradeRow` tables, values verbatim, per-column provenance: `MajorMm`/`PitchMm` and the ISO 724 / ASME B1.1 basic minor `MinorMm` PUBLISHED; `StressAreaMm2` PUBLISHED — the printed ISO 898-1 metric areas (`M12` = `84.3 mm²`) and ASME B1.1 Unified areas (`3/8-16` = `50.0 mm²`) stored verbatim, replacing the prior in-fence formulas (a printed normative value is never re-derived; the metric mean-of-diameters formula over-predicts an inch thread ~3%); the thread form DEFINED (`d2 = d − 0.649519·P`, rounded-root `d3 = d − 1.226869·P`, `H = 0.866025·P`, `e = s·2/√3`, gross shank `πd²/4`); the grade bands PUBLISHED as the Table 3 MIN column (cls 8.8 proof `580`/tensile `800 MPa` ≤M16 with the `GradeStep` `600`/`830`/`660` above, Gr5 `585`/`827`, A490 `830`/`1040`) with the Table 3.4 threaded-plane `αv` and the §3.9 preloadable flag — the prior 5.8 row's nominal-column transcription (`500`/`400`) is corrected to the min column (`520`/`420`) the sibling rows already carry. The generated `Switch`/`Map` surfaces of the prior vocabularies were unused (no per-case behavior), and no key ingress survives the typed fold — the speculative `Lazy<FrozenDictionary>` `ByKey` indexes are deleted with it (the connector page's own no-consumer verdict, applied here). Each `GradeRow` cites its own `ComponentStandard` (EN ISO 898-1 rows under `ComponentAuthority.En`, SAE/ASTM rows under `ComponentAuthority.Astm`), retiring the prior single mislabeled static.
- [TYPE_ROW_SHAPE]: REALIZED — the bespoke `FastenerSection` payload and its section arm are deleted: geometry is `SectionProfile.Circle` over the nominal major (Sectioned `false`, no `ComputedSection`); the realization identity is the seed-built bag whose rows are byte-identical to the retired projector switch (`FastenerType` token + `NominalDiameter` + `NominalLength`, dimension-only SI mints), the per-row stock length riding the `StockRow` selection into `NominalLength`; the joint category and faying class leave the type rows for the `[03]` connection owner, so one bolt type serves bearing and preloaded joints (the prior duplicate bearing/slip-critical rows for one physical bolt collapse); the placed grip length stays an instance concern above this page.
- [CAPACITY_ALGEBRA]: `Fastening` owns shear, bearing, proof, tensile, and ISO length projections over typed thread and grade rows. `ShearPlane` selects the threaded stress-area/grade factor or gross-shank/`0.60` policy without a boolean. `BearingDesign` admits ply thickness, ultimate stress, and edge factor. `FastenerAssembly` returns optional preload, slip resistance, and tightening torque for the preloaded modality, and `FastenerInstallation` admits their shared factors.
- [IFC_FASTENER_WIRE]: `FastenerKind` owns both IFC entity and predefined token. Bolt, screw, nail, anchor bolt, dowel, rivet, and coupler stamp `IfcMechanicalFastener`; nut stamps `IfcDiscreteAccessory`/`USERDEFINED`. `ConnectorInstall` composes the canonical `FastenerKind.Nail`, `Screw`, and `Bolt` rows instead of duplicating token strings.
- [INCH_HARDWARE_ENVELOPE]: RESEARCH — the UNC rows' `Hardware` column is `None`; the ASME B18.2.1 head, B18.2.2 nut, and B18.22.1 washer rows seed the inch `HexHardware` when an inch-detailed generative target lands (the across-flats wrench sizes are already carried). Until then an inch bolt's thread form, stress area, and capacities are complete while its hex solid waits on the ASME envelope rows.
- [GRADE_SIZE_BANDS]: REALIZED — ISO 898-1 bands the 8.8 mechanical values by size (proof `580`/tensile `800`/yield `640 MPa` at `≤ M16`; `600`/`830`/`660` above): the `GradeStep` option column carries the >threshold triple, `GradeRow.At(thread)` is the ONE band read every capacity projection (`ProofLoadKn`/`TensileLoadKn`/`ShearCapacityKn`/`PreloadKn`) routes through, and the prior single hybrid row (>M16 proof beside ≤M16 tensile — a 3.4% proof overstatement on the stocked `m12`/`m16` bolts) is retired; ASTM F3125 unified the old A325 over-1in reduction, so no inch row steps and the designations are unchanged.
- [COMPUTE_CONSUMER]: `Rasm.Compute` consumes `Fastening.ShearCapacityKn`, `Fastening.TensileLoadKn`, and the optional `FastenerAssembly.SlipResistanceKn`; the absence branch is a snug-tight connection, never numeric zero.
