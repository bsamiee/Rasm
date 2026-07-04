# [MATERIALS_FASTENER]

THE FASTENER SEED PAGE and THE BOLT-CAPACITY-AND-ASSEMBLY OWNER. `FastenerSeed.Rows : Context -> Fin<Seq<ComponentRow>>` is the `ComponentFamily.Fastener` row fold — the ONE generator `Threads × Grades × FastenerKind` cross-product filtered by the `GradeRow.Admits(ThreadRow)` system predicate (an ISO 898-1 property class pairs metric threads only, an SAE J429 / ASTM F3125 grade inch threads only) and the `Stocked` membership map, `Traverse`d through `Component.Of` so a rejected row ABORTS the build. A 3/8in Grade-5 hex bolt is one generated `Component` row — `SectionProfile.Circle.Of(thread.MajorMm, key)` the railed geometry, `IfcBinding.Of("IfcMechanicalFastener", kind.IfcPredefinedType)` the IFC stamp, `FastenerDetail.Of(kind, thread, lengthMm)` the `DetailLane.Realization` bag — never a `Bolt` type and never a flat designation roster: widening the range is one `Stocked` entry (a stocked combination) or one `ThreadRow`/`GradeRow` (a new size or property class), and the generated set is guarded row-for-row against the `Stocked` count so an unadmitted stocked pair faults loud. The standards data is two frozen row tables under per-column provenance: `ThreadRow` carries the PUBLISHED ISO 261/724 metric-coarse and ASME B1.1 UNC basic profile (`MajorMm`/`PitchMm`, the ISO 724 basic minor `MinorMm`, the printed ISO 898-1 / ASME B1.1 tensile stress area `StressAreaMm2` stored verbatim and never re-derived) plus the ISO 4014/4032/7089 `HexHardware` envelope, with the ISO 68-1 thread form (`d2`/`d3`/`H`/`e`, the 60° flank, the 6g/6H fits) as DEFINED expression-bodied derivations; `GradeRow` carries the PUBLISHED ISO 898-1 / SAE J429 / ASTM F3125 proof/tensile/yield minima with the `GradeStep` >M16 size band behind the ONE `At(thread)` read, the EN 1993-1-8 Table 3.4 threaded-plane `αv`, the preloadable flag, and the per-row `ComponentStandard` citation. The `FastenerKind` `[SmartEnum]` (bolt · nut · screw · anchor · dowel · rivet · coupler) is the kind axis of the generator and the IFC-token owner — `anchor`/`dowel`/`rivet`/`coupler` are `FastenerKind` arms folded INSIDE this vocabulary, never sibling families, so `ComponentFamily` stays closed at ten. `BoltCategory` (the EN 1993-1-8 Table 3.2 joint category, citing the typed `En1993Part.Part1_8` joints code), `FayingSurface` (the §3.9 slip class), and `ThreadSeries` stay policy `[SmartEnum]`s feeding the two capacity owners: the `Fastening` algebra projects the single-bolt shear/bearing/proof/tension design values and the ISO 4014 shank-vs-thread split over `(ThreadRow, GradeRow)`, and the `[03]` `FastenerAssembly` owner assembles the complete connection (bolt + grip plies + shear planes + nut + washer + `Fp,C = 0.7·fub·As` preload + `Fs,Rd` slip resistance). No admitted package owns this data — `VividOrange.Materials` is EN structural-MEMBER grade data only — so the rows are AUTHORED/PUBLISHED in-fence; the joints design code is CITED typed (`En1993`), never re-transcribed as a clause string. The multi-bolt group resistance, the long-joint `β`, and the combined shear+tension interaction are `Rasm.Compute` reads over these single-bolt receipts, never re-derived here.

## [01]-[INDEX]

- [02]-[FASTENER_FAMILY]: the `FastenerKind`/`ThreadSeries`/`BoltCategory`/`FayingSurface` policy vocabularies, the `HexHardware` envelope, the `ThreadRow`/`GradeRow` frozen standards tables with `Threads`/`Grades` owners, the `Fastening` single-bolt capacity and thread-split algebra, the `FastenerDetail` realization bag, and the `FastenerSeed.Rows` cross-product generator with the `Stocked` membership oracle.
- [03]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — bolt + grip-plies (`Dimension`) + shear-planes + nut + washer over one `(ThreadRow, GradeRow, BoltCategory, FayingSurface)`, the `PreloadKn` `Fp,C = 0.7·fub·As` projection, the `SlipResistanceKn` EN 1993-1-8 preloaded design value, and the ISO 7089/7090 washer-hardness selection.

## [02]-[FASTENER_FAMILY]

- Owner: `FastenerSeed` the `ComponentFamily.Fastener` row fold; `Threads`/`Grades` the frozen standards-row tables (`ThreadRow` 17 rows, `GradeRow` 13 rows, the standards' Table-3 min-column values under per-column `PUBLISHED`/`DEFINED` provenance — typed row references carry every read, so no key index exists); `FastenerKind` the kind axis carrying the verified `IfcMechanicalFastenerTypeEnum` token plus the `Threaded`/`Headed` form flags the thread-split algebra dispatches on; `BoltCategory`/`FayingSurface`/`ThreadSeries` the retained policy vocabularies; `Fastening` the single-bolt design-value algebra over `(ThreadRow, GradeRow)`; `FastenerDetail` the seed-time `DetailLane.Realization` bag builder composing `ComponentDetail`.
- Cases: kind {`bolt` (externally threaded, headed) · `nut` (internally threaded mate — `USERDEFINED`, an `IfcDiscreteAccessory` companion at egress) · `screw` (self-driven) · `anchor` (cast-in or post-installed — `ANCHORBOLT`) · `dowel` (unthreaded shear pin) · `rivet` (driven permanent) · `coupler` (bar mechanical splice — `COUPLER`)} × thread {M6..M36 metric coarse · 1/4in..1-1/2in UNC} × grade {ISO 898-1 cls 4.6/4.8/5.6/5.8/6.8/8.8/10.9/12.9 · SAE J429 Gr2/Gr5/Gr8 · ASTM F3125 A325/A490} — a fastener is one generated row over one kind, one `ThreadRow`, and one `GradeRow`, never a fastener subtype; the joint category (EN 1993-1-8 A..E) is a `FastenerAssembly` connection decision, never a type-row column, so one bolt type serves bearing and preloaded joints without duplicate rows.
- Entry: `FastenerSeed.Rows(context)` — the ONE catalogue fold: the kind × thread × grade cross-product, `GradeRow.Admits(ThreadRow)` the system exclusion (`Metric == thread.Series.Metric`) and the `Stocked` map the membership policy, both explicit `Filter`s BEFORE `Component.Of`, then the pre-`Traverse` oracle `guard` that the admitted candidate set covers every `Stocked` key (each miss NAMED in the fault detail, so a mistyped membership key faults instead of silently dropping a row), then `Traverse` into `Fin<Seq<ComponentRow>>` (every row `Sectioned: false` — a fastener crosses to IFC as a component, never a profile solve); `Fastening.ShearCapacityKn(thread, grade, threadsInShearPlane)` / `BearingCapacityKn` / `ProofLoadKn` / `TensileLoadKn` the single-bolt design values and `Fastening.ThreadLengthMm(kind, thread, lengthMm)` / `UnthreadedShankMm` / `ThreadRunoutMm` the ISO 4014 length split — pure projections over the row tables, never re-computed per call site and never a `GetBoltBySize`/`GetByGrade` family.
- Packages: Rasm.Numerics (`Dimension` — the `[03]` discrete grip-ply/shear-plane counts), Rasm.Domain (`Op`/`Context`/`AcceptValidated`), Rasm.Element (`MaterialId`, `DetailSchema`, `PropertyBag`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile.Circle.Of` the railed profile admission/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the four retained policy vocabularies), LanguageExt.Core (`Fin`/`Seq`/`Bind`/`Filter`/`Traverse`/`.As()`/`guard`/`Option`), VividOrange.Standards (`En1993`/`En1993Part.Part1_8`/`NationalAnnex` — the typed joints-code citation on `BoltCategory`), BCL (`FrozenDictionary`, `ImmutableArray`). No bolt-grade producer exists among admitted packages (`VividOrange.Materials` `EnSteelGrade` is EN member-grade data, no ISO 898-1/SAE/ASTM bolt classes), so the rows are AUTHORED/PUBLISHED here.
- Growth: a new stocked combination is one `Stocked` entry; a new thread size one `ThreadRow`; a new property class one `GradeRow`; a size-banded class one `GradeStep` column fill; a new member type one `FastenerKind` row; a new joint category one `BoltCategory` row — zero type edits, zero fold edits, the count guard re-proving membership on every build. A metric-fine or UNF pitch family is `ThreadRow`s under the existing `ThreadSeries` rows, never a parallel size axis; the inch ASME B18.2 hex envelope seeds by filling the UNC rows' `Hardware` column from `None` to `Some`.
- Boundary: the bespoke `FastenerSection` payload and its `ComponentSection` arm are DELETED — geometry is the closed `SectionProfile.Circle` arm over the nominal major diameter, the thread form and hex envelope ride the row tables, and the realization identity rides the seed-built bag (`FastenerType` token + `NominalDiameter` + `NominalLength`, dimension-only SI mints, rows byte-identical to the retired projector switch so `Node.PropertySet` content keys are unchanged); the stress area is the printed standard value (`PUBLISHED` — the ISO 898-1 metric table and the ASME B1.1 Unified table disagree with each other's formulas by ~3%, so neither is re-derived); the per-row stock length is `Stocked` seed data feeding the bag's `NominalLength`, while the grip-driven placed length stays an instance concern above this page; the substance/appearance split rides `GradeRow` (`steel.fastener-*` substance read by the properties seam, `metal.iron`/`metal.steel` appearance) — two independent `MaterialId` slots; the `IfcMechanicalFastenerTypeEnum` token is the portable egress HINT the `Rasm.Bim` gate validates against the generated roster, never authority here, and the nut's `USERDEFINED` rides as an `IfcDiscreteAccessory` companion at egress; a bolt pattern places through the layout fold over these rows, never a per-family schedule owner.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;                     // Dimension (the [03] discrete grip-ply/shear-plane carrier)
using Rasm.Domain;                      // Op, Context, AcceptValidated
using Rasm.Element;                     // MaterialId, DetailSchema, PropertyBag
using Thinktecture;
using VividOrange.Standards.Eurocode;   // En1993, En1993Part, NationalAnnex — the typed EN 1993-1-8 joints citation
using SiDim = Rasm.Element.Dimension;   // the SI-dimension axis the detail-bag mints ride — aliased clear of the Rasm.Numerics discrete-count Dimension
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The kind axis of the seed generator: the member-type vocabulary carrying the VERIFIED GeometryGym
// IfcMechanicalFastenerTypeEnum token (BOLT/SCREW/ANCHORBOLT/DOWEL/RIVET/COUPLER real members; NO NUT member, so a nut
// rides USERDEFINED as an IfcDiscreteAccessory companion at egress) and the form flags the Fastening thread-split
// dispatches on. anchor/dowel/rivet/coupler are arms HERE — ComponentFamily stays closed at ten.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerKind {
    public static readonly FastenerKind Bolt    = new("bolt",    ifcPredefinedType: "BOLT",        threaded: true,  headed: true);
    public static readonly FastenerKind Nut     = new("nut",     ifcPredefinedType: "USERDEFINED", threaded: true,  headed: false);
    public static readonly FastenerKind Screw   = new("screw",   ifcPredefinedType: "SCREW",       threaded: true,  headed: true);
    public static readonly FastenerKind Anchor  = new("anchor",  ifcPredefinedType: "ANCHORBOLT",  threaded: true,  headed: true);
    public static readonly FastenerKind Dowel   = new("dowel",   ifcPredefinedType: "DOWEL",       threaded: false, headed: false);
    public static readonly FastenerKind Rivet   = new("rivet",   ifcPredefinedType: "RIVET",       threaded: false, headed: true);
    public static readonly FastenerKind Coupler = new("coupler", ifcPredefinedType: "COUPLER",     threaded: true,  headed: false);
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
public static class Threads {
    public static readonly ImmutableArray<ThreadRow> Rows = [
        new("m6",    ThreadSeries.MetricCoarse,  6.000, 1.000,  4.917,  20.1, 10.000, Some(new HexHardware(4.0,  8.74,  6.8,  5.2,  6.4,  12.0, 1.6))),
        new("m8",    ThreadSeries.MetricCoarse,  8.000, 1.250,  6.647,  36.6, 13.000, Some(new HexHardware(5.3,  11.47, 9.2,  6.8,  8.4,  16.0, 1.6))),
        new("m10",   ThreadSeries.MetricCoarse, 10.000, 1.500,  8.376,  58.0, 16.000, Some(new HexHardware(6.4,  14.47, 11.2, 8.4,  10.5, 20.0, 2.0))),
        new("m12",   ThreadSeries.MetricCoarse, 12.000, 1.750, 10.106,  84.3, 18.000, Some(new HexHardware(7.5,  16.47, 13.7, 10.8, 13.0, 24.0, 2.5))),
        new("m16",   ThreadSeries.MetricCoarse, 16.000, 2.000, 13.835, 157.0, 24.000, Some(new HexHardware(10.0, 22.00, 17.7, 14.8, 17.0, 30.0, 3.0))),
        new("m20",   ThreadSeries.MetricCoarse, 20.000, 2.500, 17.294, 245.0, 30.000, Some(new HexHardware(12.5, 27.70, 22.4, 18.0, 21.0, 37.0, 3.0))),
        new("m24",   ThreadSeries.MetricCoarse, 24.000, 3.000, 20.752, 353.0, 36.000, Some(new HexHardware(15.0, 33.25, 26.4, 21.5, 25.0, 44.0, 4.0))),
        new("m30",   ThreadSeries.MetricCoarse, 30.000, 3.500, 26.211, 561.0, 46.000, Some(new HexHardware(18.7, 42.75, 33.4, 25.6, 31.0, 56.0, 4.0))),
        new("m36",   ThreadSeries.MetricCoarse, 36.000, 4.000, 31.670, 817.0, 55.000, Some(new HexHardware(22.5, 51.11, 39.4, 31.0, 37.0, 66.0, 5.0))),
        new("1/4",   ThreadSeries.UnifiedCoarse,  6.350, 1.2700,  4.976,  20.5, 11.113, None, "0250"),
        new("3/8",   ThreadSeries.UnifiedCoarse,  9.525, 1.5875,  7.805,  50.0, 14.288, None, "0375"),
        new("1/2",   ThreadSeries.UnifiedCoarse, 12.700, 1.9538, 10.585,  91.5, 19.050, None, "0500"),
        new("5/8",   ThreadSeries.UnifiedCoarse, 15.875, 2.3091, 13.375, 145.8, 23.813, None, "0625"),
        new("3/4",   ThreadSeries.UnifiedCoarse, 19.050, 2.5400, 16.300, 215.5, 28.575, None, "0750"),
        new("7/8",   ThreadSeries.UnifiedCoarse, 22.225, 2.8222, 19.170, 298.1, 33.338, None, "0875"),
        new("1",     ThreadSeries.UnifiedCoarse, 25.400, 3.1750, 21.963, 391.0, 38.100, None, "1000"),
        new("1-1/2", ThreadSeries.UnifiedCoarse, 38.100, 4.2333, 33.517, 906.5, 57.150, None, "1500")];
}

// 13 grade rows: 8 ISO 898-1 property classes (EN ISO citation) + 3 SAE J429 + 2 ASTM F3125 (US citation). Base
// columns are the Table 3 MIN values for the ≤threshold band; 8.8 is the one rostered class that steps (>M16).
public static class Grades {
    static readonly ComponentStandard EnIso = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    static readonly ComponentStandard Us    = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    public static readonly ImmutableArray<GradeRow> Rows = [
        new("4.6",  "ISO 898-1",       225.0,  400.0,  240.0, 0.60, false, true,  EnIso, "steel.fastener-4_6",  "metal.iron"),
        new("4.8",  "ISO 898-1",       310.0,  420.0,  340.0, 0.50, false, true,  EnIso, "steel.fastener-4_8",  "metal.iron"),
        new("5.6",  "ISO 898-1",       280.0,  500.0,  300.0, 0.60, false, true,  EnIso, "steel.fastener-5_6",  "metal.iron"),
        new("5.8",  "ISO 898-1",       380.0,  520.0,  420.0, 0.50, false, true,  EnIso, "steel.fastener-5_8",  "metal.iron"),
        new("6.8",  "ISO 898-1",       440.0,  600.0,  480.0, 0.50, false, true,  EnIso, "steel.fastener-6_8",  "metal.iron"),
        new("8.8",  "ISO 898-1",       580.0,  800.0,  640.0, 0.60, true,  true,  EnIso, "steel.fastener-8_8",  "metal.steel", Some(new GradeStep(16.0, 600.0, 830.0, 660.0))),
        new("10.9", "ISO 898-1",       830.0, 1040.0,  940.0, 0.50, true,  true,  EnIso, "steel.fastener-10_9", "metal.steel"),
        new("12.9", "ISO 898-1",       970.0, 1220.0, 1100.0, 0.50, false, true,  EnIso, "steel.fastener-12_9", "metal.steel"),
        new("gr2",  "SAE J429",        379.0,  510.0,  393.0, 0.60, false, false, Us,    "steel.fastener-gr2",  "metal.iron"),
        new("gr5",  "SAE J429",        585.0,  827.0,  634.0, 0.60, false, false, Us,    "steel.fastener-gr5",  "metal.steel"),
        new("gr8",  "SAE J429",        830.0, 1034.0,  896.0, 0.50, false, false, Us,    "steel.fastener-gr8",  "metal.steel"),
        new("a325", "ASTM F3125",      585.0,  827.0,  634.0, 0.60, true,  false, Us,    "steel.fastener-a325", "metal.steel"),
        new("a490", "ASTM F3125",      830.0, 1040.0,  940.0, 0.50, true,  false, Us,    "steel.fastener-a490", "metal.steel")];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The single-bolt design-value and length-split algebra over (ThreadRow, GradeRow) — the receipts Rasm.Compute reads
// off the seam for group resistance and combined shear+tension interaction, never re-derived there.
public static class Fastening {
    const double ShankAlphaV = 0.60;   // EN 1993-1-8 Table 3.4: the shank-plane αv is 0.6 for EVERY grade — the threaded-plane reduction never applies to the shank

    public static double ProofLoadKn(ThreadRow thread, GradeRow grade) => grade.At(thread).ProofStressMpa * thread.StressAreaMm2 * 1e-3;
    public static double TensileLoadKn(ThreadRow thread, GradeRow grade) => grade.At(thread).TensileStrengthMpa * thread.StressAreaMm2 * 1e-3;

    // Fv = αv·fub·A — αv AND A are plane-dependent: the stored threaded-plane αv with the stress area through the
    // thread, the un-reduced 0.6 with the gross shank area through the shank (the stored 0.5 on a 10.9 shank plane
    // under-predicts ~17% and is the deleted form); fub the size-banded At read.
    public static double ShearCapacityKn(ThreadRow thread, GradeRow grade, bool threadsInShearPlane) =>
        (threadsInShearPlane ? grade.ShearStrengthFactor : ShankAlphaV) * grade.At(thread).TensileStrengthMpa
            * (threadsInShearPlane ? thread.StressAreaMm2 : thread.NominalAreaMm2) * 1e-3;

    // Fb = k1·αb·fu,ply·d·t — edge/end/pitch geometry collapsed into the caller-supplied edgeFactor, the ply ultimate
    // strength supplied by the properties Mechanical receipt, never re-keyed here.
    public static double BearingCapacityKn(ThreadRow thread, double plyThicknessMm, double plyUltimateMpa, double edgeFactor) =>
        edgeFactor * plyUltimateMpa * thread.MajorMm * plyThicknessMm * 1e-3;

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
    public static PropertyBag Of(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        ComponentDetail.RealizationRows(
            ComponentDetail.Token(DetailSchema.FastenerType, kind.IfcPredefinedType),
            ComponentDetail.Measured(DetailSchema.NominalDiameter, SiDim.LengthDim, thread.MajorMm * 1e-3),
            ComponentDetail.Measured(DetailSchema.NominalLength, SiDim.LengthDim, lengthMm * 1e-3));
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The ONE generator: kind × thread × grade, Admits the system exclusion and Stocked the membership policy — both
// explicit Filters BEFORE construction — then Traverse so a Component.Of rejection ABORTS the build. Widening the
// range is one Stocked entry; the pre-Traverse oracle NAMES every stocked key the cross-product never admitted
// (a mistyped key or a system-mismatched pair), so membership drift faults loud before any row constructs.
public static class FastenerSeed {
    // (kind, thread, grade) -> stock length L in mm: the bolt/screw/anchor overall length, the ISO 4032 nut height m,
    // the coupler body length. Membership + length are ONE seed policy row; the mechanism is the fold below.
    static readonly FrozenDictionary<(string Kind, string Thread, string Grade), double> Stocked =
        new Dictionary<(string, string, string), double> {
            [("bolt", "m12", "8.8")]    = 60.0,
            [("bolt", "m16", "8.8")]    = 80.0,
            [("bolt", "m16", "10.9")]   = 80.0,
            [("bolt", "m20", "8.8")]    = 90.0,
            [("bolt", "m20", "10.9")]   = 90.0,
            [("bolt", "m24", "10.9")]   = 110.0,
            [("bolt", "m30", "12.9")]   = 140.0,
            [("bolt", "3/8", "gr5")]    = 63.5,
            [("bolt", "1/2", "gr5")]    = 76.2,
            [("bolt", "3/4", "gr8")]    = 101.6,
            [("bolt", "7/8", "a325")]   = 114.3,
            [("bolt", "7/8", "a490")]   = 114.3,
            [("nut", "m16", "8.8")]     = 14.8,
            [("nut", "m20", "10.9")]    = 18.0,
            [("screw", "m8", "8.8")]    = 40.0,
            [("screw", "1/4", "gr2")]   = 31.8,
            [("dowel", "m20", "8.8")]   = 100.0,
            [("coupler", "m20", "8.8")] = 60.0,
            [("anchor", "m16", "8.8")]  = 200.0,
            [("anchor", "m20", "8.8")]  = 250.0,
            [("anchor", "3/4", "a325")] = 304.8,
        }.ToFrozenDictionary();

    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        from stocked in Fin.Succ(FastenerKind.Items.ToSeq()
            .Bind(kind => Threads.Rows.ToSeq()
                .Bind(thread => Grades.Rows.ToSeq()
                    .Filter(grade => grade.Admits(thread))
                    .Map(grade => (Kind: kind, Thread: thread, Grade: grade, StockKey: (kind.Key, thread.Key, grade.Key)))))
            .Filter(static row => Stocked.ContainsKey(row.StockKey)))
        from oracle in guard(stocked.Count == Stocked.Count,
            ComponentFault.Family(context.Key,
                $"<stocked-pair-unadmitted:{string.Join(',', Stocked.Keys.Except(stocked.Map(static r => r.StockKey)))}>"))
        from rows in stocked
            .Traverse(row =>
                from profile in SectionProfile.Circle.Of(row.Thread.MajorMm, context.Key)
                from item in Component.Of(
                    ComponentFamily.Fastener, $"fastener.{row.Kind.Key}-{row.Thread.Designation}-{row.Grade.Tag}",
                    profile,
                    IfcBinding.Of("IfcMechanicalFastener", row.Kind.IfcPredefinedType),
                    Coring.None, row.Grade.Standard, substanceId: row.Grade.Substance, appearanceId: row.Grade.Appearance,
                    detail: Some(FastenerDetail.Of(row.Kind, row.Thread, Stocked[row.StockKey])), context.Key)
                select new ComponentRow(item, Sectioned: false)).As()
        select rows;
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` the complete-connection owner over one `(ThreadRow, GradeRow, BoltCategory, FayingSurface)` — the bolt plus the discrete `Dimension` grip-ply and shear-plane counts, the washer presence, and the preloaded design state; `PreloadKn` the `Fp,C = 0.7·fub·As` EN 1993-1-8 §3.9 / RCSC projection (fub the size-banded `GradeRow.At` read), `SlipResistanceKn(ks, γM3)` the per-bolt `Fs,Rd = ks·n·μ·Fp,C/γM3` slip design value the category-B/C/E seam reads, `TighteningTorqueNm(km)` the EN 1090-2 §8.5 torque-method installation moment `Mr = km·d·Fp,C` over the manufacturer-declared EN 14399-2 k-class factor, `WasherHardnessHv` the ISO 7089 200 HV / ISO 7090 300 HV grade-driven split, and the nut/washer geometry reads off the thread's `Hardware` envelope.
- Cases: one assembly shape for every modality — a non-preloaded (A/D) assembly resolves `FayingSurface.None` and reads zero preload and slip; a preloaded (B/C/E) assembly carries its named slip class and the full projections — never a `PreloadedBolt`/`BearingBolt` pair; the joint category lives HERE, on the connection, so one generated bolt type serves every category without duplicate seed rows.
- Entry: `FastenerAssembly.Of(thread, grade, category, faying, gripPlies, shearPlanes, withWasher, key)` — the ONE assembly admission: it rails a system-mismatched thread/grade pair (`grade.Admits(thread)`) and a preloaded category over a non-`Preloadable` grade through `ComponentFault.Grade`, admits the discrete counts through `key.AcceptValidated<Dimension>(candidate:)` (a non-positive count rails the kernel `Dimension` admission), and resolves `category.Preloaded ? faying : FayingSurface.None` so a bearing joint never relies on a slip coefficient — one polymorphic boundary, never a per-modality factory family.
- Growth: a new connection modality is a `BoltCategory`/`FayingSurface` row the assembly reads; the multi-bolt group `ΣFs,Rd`, the long-joint `β`, and the `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` interaction are `Rasm.Compute` consumers over these single-bolt receipts.
- Boundary: the grip-ply and shear-plane counts are the kernel `Dimension` discrete-count carrier, never a `PositiveMagnitude` that admits a fractional layer; the `ks` hole-tolerance factor, the `γM3` partial factor, and the `km` k-class torque factor are caller-supplied design-code parameters `Rasm.Compute` owns; the preloaded-over-non-preloadable state is unconstructible (railed at `Of`), so every slip read is over a valid preloaded bolt; the assembly is host-neutral scalar data — NOT a `Component` (a `Component` is one discrete schedule type; the assembly is the populated connection it completes), the two meeting at the row tables this page owns.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
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
        from plies in key.AcceptValidated<Dimension>(candidate: gripPlies)
        from planes in key.AcceptValidated<Dimension>(candidate: shearPlanes)
        select new FastenerAssembly(thread, grade, category, category.Preloaded ? faying : FayingSurface.None, plies, planes, withWasher);

    // Fp,C = 0.7·fub·As over the size-banded At read — the design pretension a preloaded high-strength bolt installs
    // to; zero for a bearing joint. Of already railed preloaded-over-non-preloadable, so no interior re-check exists.
    public double PreloadKn => Category.Preloaded
        ? 0.7 * Grade.At(Thread).TensileStrengthMpa * Thread.StressAreaMm2 * 1e-3
        : 0.0;

    // Fs,Rd = ks·n·μ·Fp,C/γM3 — n the admitted friction-interface count, μ the resolved faying class.
    public double SlipResistanceKn(double ks, double gammaM3) =>
        gammaM3 > 0.0 ? ks * ShearPlanes.Value * Faying.SlipFactor * PreloadKn / gammaM3 : 0.0;

    // EN 1090-2 §8.5 torque-method installation moment Mr = km·d·Fp,C — km the manufacturer-declared EN 14399-2
    // k-class factor (caller-supplied like ks/γM3); zero for a bearing joint through the PreloadKn gate.
    public double TighteningTorqueNm(double km) => km * (Thread.MajorMm * 1e-3) * (PreloadKn * 1e3);

    // ISO 7090 300 HV (chamfered, preloaded high-strength) vs ISO 7089 200 HV (plain) washer hardness.
    public double WasherHardnessHv => Grade.Preloadable ? 300.0 : 200.0;

    public Option<double> NutHeightMm => Thread.Hardware.Map(static h => h.NutHeightMm);
    public Option<double> WasherOuterMm => WithWasher ? Thread.Hardware.Map(static h => h.WasherOuterMm) : None;
    public Option<double> WasherThicknessMm => WithWasher ? Thread.Hardware.Map(static h => h.WasherThicknessMm) : None;
}
```

## [04]-[RESEARCH]

- [SEED_GENERATOR]: REALIZED — the flat designation roster is retired; the catalogue is the kind × thread × grade cross-product filtered by the `GradeRow.Admits(ThreadRow)` system predicate and the `Stocked` membership map, `Traverse`d through `Component.Of` into `Fin<Seq<ComponentRow>>` (fail-loud, no `Choose`). The kind is the fold's own axis over `FastenerKind.Items` — a grade-carried kind cannot reproduce the nut/screw/dowel/coupler/anchor rows that share a property class with the bolt rows, so the 13 `GradeRow`s stay 1:1 with the property classes and the kind multiplicity rides the generator. The generated set equals the prior 21 designations byte-for-byte (`fastener.bolt-m12-88` .. `fastener.anchor-0750-a325` via the `ThreadRow.Designation`/`GradeRow.Tag` tokens), and the pre-`Traverse` oracle guard proves membership on every build, NAMING each unadmitted stocked key in the fault detail; widening is one `Stocked` entry, with today's set the admitted oracle.
- [ROW_TABLE_CONVERSION]: REALIZED — `ThreadSize`(17) and `FastenerGrade`(13) convert 1:1 to the frozen `ThreadRow`/`GradeRow` tables, values verbatim, per-column provenance: `MajorMm`/`PitchMm` and the ISO 724 / ASME B1.1 basic minor `MinorMm` PUBLISHED; `StressAreaMm2` PUBLISHED — the printed ISO 898-1 metric areas (`M12` = `84.3 mm²`) and ASME B1.1 Unified areas (`3/8-16` = `50.0 mm²`) stored verbatim, replacing the prior in-fence formulas (a printed normative value is never re-derived; the metric mean-of-diameters formula over-predicts an inch thread ~3%); the thread form DEFINED (`d2 = d − 0.649519·P`, rounded-root `d3 = d − 1.226869·P`, `H = 0.866025·P`, `e = s·2/√3`, gross shank `πd²/4`); the grade bands PUBLISHED as the Table 3 MIN column (cls 8.8 proof `580`/tensile `800 MPa` ≤M16 with the `GradeStep` `600`/`830`/`660` above, Gr5 `585`/`827`, A490 `830`/`1040`) with the Table 3.4 threaded-plane `αv` and the §3.9 preloadable flag — the prior 5.8 row's nominal-column transcription (`500`/`400`) is corrected to the min column (`520`/`420`) the sibling rows already carry. The generated `Switch`/`Map` surfaces of the prior vocabularies were unused (no per-case behavior), and no key ingress survives the typed fold — the speculative `Lazy<FrozenDictionary>` `ByKey` indexes are deleted with it (the connector page's own no-consumer verdict, applied here). Each `GradeRow` cites its own `ComponentStandard` (EN ISO 898-1 rows under `ComponentAuthority.En`, SAE/ASTM rows under `ComponentAuthority.Astm`), retiring the prior single mislabeled static.
- [TYPE_ROW_SHAPE]: REALIZED — the bespoke `FastenerSection` payload and its section arm are deleted: geometry is `SectionProfile.Circle` over the nominal major (Sectioned `false`, no `ComputedSection`); the realization identity is the seed-built bag whose rows are byte-identical to the retired projector switch (`FastenerType` token + `NominalDiameter` + `NominalLength`, dimension-only SI mints), the per-row stock length riding the `Stocked` policy into `NominalLength`; the joint category and faying class leave the type rows for the `[03]` connection owner, so one bolt type serves bearing and preloaded joints (the prior duplicate bearing/slip-critical rows for one physical bolt collapse); the placed grip length stays an instance concern above this page.
- [CAPACITY_ALGEBRA]: REALIZED — the single-bolt design values relocate to the `Fastening` algebra over `(ThreadRow, GradeRow)`: `ShearCapacityKn` with the plane-dependent `αv` (stored threaded-plane factor; un-reduced `0.6` shank plane for every grade), `BearingCapacityKn` over the caller-supplied `k1·αb` and the properties-seam ply ultimate, `ProofLoadKn`/`TensileLoadKn`, and the ISO 4014 length split (`b = 2d+6/2d+12/2d+25` clamped, headless-threaded full-length, unthreaded all-shank, ISO 3508 runout `2.5·P`) dispatched on `FastenerKind.Threaded`/`Headed`. `FastenerAssembly` gains the `grade.Admits(thread)` system guard (a mismatched inch grade on a metric thread was previously constructible) and keeps `PreloadKn`/`SlipResistanceKn`/`WasherHardnessHv` — every fub/proof read routed through the banded `GradeRow.At` — widened by `TighteningTorqueNm(km)` (EN 1090-2 §8.5 `Mr = km·d·Fp,C`, the site installation-torque projection off the preload the owner already carries) with `ks`/`γM3`/`km` caller-supplied. `BoltCategory` carries the typed `En1993(En1993Part.Part1_8, NationalAnnex.RecommendedValues)` joints citation; the re-transcribed clause strings are deleted.
- [IFC_FASTENER_WIRE]: REALIZED — every generated row binds `IfcBinding.Of("IfcMechanicalFastener", kind.IfcPredefinedType)` over the verified `IfcMechanicalFastenerTypeEnum` members {`BOLT`, `SCREW`, `ANCHORBOLT`, `DOWEL`, `RIVET`, `COUPLER`}; the enum has no `NUT` member, so a nut rides `USERDEFINED` and crosses as an `IfcDiscreteAccessory` companion at the `Rasm.Bim` egress, where the per-token gate validates every predefined string against the generated roster — this page carries only the portable token and the bag's `FastenerType` row the connection reader recovers one-hop. A non-mechanical realizing element (adhesive/mortar/weld) crosses as `IfcFastener` over the joint family's vocabulary, never here.
- [INCH_HARDWARE_ENVELOPE]: RESEARCH — the UNC rows' `Hardware` column is `None`; the ASME B18.2.1 head, B18.2.2 nut, and B18.22.1 washer rows seed the inch `HexHardware` when an inch-detailed generative target lands (the across-flats wrench sizes are already carried). Until then an inch bolt's thread form, stress area, and capacities are complete while its hex solid waits on the ASME envelope rows.
- [GRADE_SIZE_BANDS]: REALIZED — ISO 898-1 bands the 8.8 mechanical values by size (proof `580`/tensile `800`/yield `640 MPa` at `≤ M16`; `600`/`830`/`660` above): the `GradeStep` option column carries the >threshold triple, `GradeRow.At(thread)` is the ONE band read every capacity projection (`ProofLoadKn`/`TensileLoadKn`/`ShearCapacityKn`/`PreloadKn`) routes through, and the prior single hybrid row (>M16 proof beside ≤M16 tensile — a 3.4% proof overstatement on the stocked `m12`/`m16` bolts) is retired; ASTM F3125 unified the old A325 over-1in reduction, so no inch row steps and the designations are unchanged.
- [COMPUTE_CONSUMER_RIPPLE]: NEXT-CAMPAIGN — `Rasm.Compute`'s bolt-check seam moves from the deleted `FastenerSection` instance projections to the static `Fastening` algebra (`Fastening.ShearCapacityKn(thread, grade, threadsInShearPlane)`/`TensileLoadKn`/`FastenerAssembly.SlipResistanceKn`); a consumer re-point per the brief `[CONSUMER_RIPPLE_RULES]`, recorded here for the R6 ripple census — no in-campaign Compute edit, the Materials receipts are the frozen read surface.
