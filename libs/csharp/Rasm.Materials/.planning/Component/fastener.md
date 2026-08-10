# [MATERIALS_FASTENER]

THE FASTENER SEED PAGE owns the `ComponentFamily.Fastener` fold, the thread-form algebra, and the EN 1993-1-8 single-fastener design values. `StockRow.Threaded` carries `ThreadRow` and `GradeRow`; `StockRow.Plain` carries published nail, dowel, and rivet data — including its own PUBLISHED tensile strength — without a fake thread or bolt grade. Both cases project through ONE `StockFacts` read, so geometry, IFC binding, realization detail, the EC5 dowel check, and `Component.Of` share one fold while case-specific admission remains total. Every design value this page emits is a DESIGN resistance already divided by its own partial factor, so a consumer folds demand against it directly and no arm re-divides.

## [01]-[INDEX]

- [02]-[FASTENER_FAMILY]: the `FastenerKind`/`ThreadSeries`/`BoltCategory`/`FayingSurface`/`HeadForm` policy vocabularies, the `HexHardware` head-nut-washer dimension set, the `ThreadRow`/`GradeRow` standards tables with their `Threads`/`Grades` owners and the ISO 68-1 form algebra every geometric column derives from, the `Fastening` EN 1993-1-8 shear/tension/punching design values and EC5 §8.5 dowel-type timber-connection algebra, the `FastenerSelection` inverse sizing scan over both roster arrays, the `FastenerDetail` realization bag, and the `FastenerSeed.Rows` typed-selection generator with its `FastenerPlacement`-driven capacity producer.
- [03]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — bolt + grip-plies (`Count`) + shear-planes + nut + washer over one `(ThreadRow, GradeRow, BoltCategory, FayingSurface, HeadForm)` — the `BoltPosition`/`HoleShape`/`BearingDesign` EN 1993-1-8 Table 3.4 bearing geometry, the `PreloadKn` `Fp,C = 0.7·fub·As` projection under its yield ceiling, the `FastenerInstallation` admitted slip-and-torque factor set, the `SlipResistanceKn` EN 1993-1-8 §3.9 design value, and the ISO 7089/7090 washer-hardness selection.

## [02]-[FASTENER_FAMILY]

- Owner: `FastenerSeed` owns the `ComponentFamily.Fastener` row fold and the capacity producer; `Threads` and `Grades` own the standards tables; `FastenerKind` owns the complete IFC entity/token binding and the realization token every kind reads distinctly; `BoltCategory`, `FayingSurface`, `ThreadSeries`, and `HeadForm` own policy; `Fastening` owns the design values; `FastenerSelection` owns the inverse sizing scan; `FastenerAssembly` owns installed-bolt state; `FastenerDetail` owns the realization bag.
- Cases: kind {`bolt` · `nut` · `nail` · `screw` · `anchor` · `dowel` · `rivet` · `coupler`} × stock form {threaded hardware over a `ThreadRow`/`GradeRow` pair · plain shank over its published designation, diameter, length, tensile strength, standard, and material pair}; the joint category is a `FastenerAssembly` decision, never a type-row column.
- Entry: `FastenerSeed.Rows(context)` traverses the typed `Stocked` selection, dispatches `StockRow.Admit`, and feeds the one `StockFacts` projection into `Component.Of`; `FastenerSeed.Capacity` dispatches the `FastenerPlacement` the connection carries into the matching `CapacityReceipt`; `Fastening` owns the EN 1993-1-8 §3.6 resistances, the ISO 4014 length algebra, and the EC5 §8.5 dowel-type check.
- Packages: Rasm.Numerics (`Dimension` aliased `Count` — the `[03]` discrete grip-ply/shear-plane columns), Rasm.Domain (`Op`/`Context`/`AcceptValidated`), Rasm.Element (`MaterialId`, `DetailSchema`, `PropertyBag`, `PropertyName`, `PropertyValue`, the SI `Dimension` axis the bag mints over), Rasm.Materials.Component (the parent owner: `Component`/`ComponentRow`/`ComponentFamily`/`SectionProfile.Circle.Of` the railed profile admission/`IfcBinding`/`Coring`/`ComponentStandard`/`ComponentAuthority`/`ComponentFault`/`ComponentDetail`, and the sibling `TimberGrade`/`TimberPartialFactor`/`ServiceClass`/`LoadDuration` the EC5 join reads), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the policy vocabularies, `[UseDelegateFromConstructor]` for the shear-plane and bolt-position columns, `[ComplexValueObject]` for the admitted design sets), LanguageExt.Core (`Fin`/`Seq`/`Traverse`/`.As()`/`guard`/`Option`/`ToFin`), BCL (`ImmutableArray`, `FrozenDictionary`). No bolt-grade producer exists among admitted packages (`VividOrange.Materials` `EnSteelGrade` is EN member-grade data, no ISO 898-1/SAE/ASTM bolt classes), so the rows are PUBLISHED here under per-column provenance.
- Growth: a new threaded combination is one `StockRow.Threaded`; a new plain-shank product one `StockRow.Plain`; a new kind one `FastenerKind` row plus its appropriate stock case; a new thread one `Threads` entry carrying its own diameter and pitch; a new property class one `Grades` factory call; a new connection category one `BoltCategory` row; a new head geometry one `HeadForm` row; a new bolt-group position one `BoltPosition` row.
- Boundary: every fastener uses `SectionProfile.Circle` and the seed-built realization bag. Thread semantics and `GradeRow` material data exist only on `StockRow.Threaded`; `StockRow.Plain` carries its own published diameter, length, tensile strength, standard, and independent substance/appearance pair. `Fastening.TimberDowelShearKn` takes the SCALARS EC5 §8.5 consumes — shank diameter, fastener ultimate strength, and the load-to-grain angle — so the plain dowel, nail, and rivet rows the clause is written for reach it through `StockFacts.UltimateMpa`, never a threaded currency a plain product does not carry. IFC tokens remain portable egress hints validated by `Rasm.Bim`.
- Boundary: this page emits EN 1993-1-8 design resistances and EN 1995-1-1 design resistances and NOTHING ELSE. `GradeRow.EurocodeAlphaV` is `Some` only for the seven property classes EN 1993-1-8 Table 3.1 tabulates, so a SAE, ASTM, 9.8, or 12.9 grade RAILS out of the Eurocode resistances rather than borrowing an α_v the code never published for it — the AISC 360 §J3 resistances those grades design under are a `capacity#SECTION_CAPACITY` `DesignBasis` row and its own arm, never a silent reuse of this one. The published mechanical band, the preload, and the stock identity stay total over every grade, because those are each body's own specification data.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;        // FrozenDictionary (the designation-keyed stock join the capacity producer reads)
using System.Collections.Immutable;
using LanguageExt;
using Rasm.Numerics;
using Rasm.Domain;                      // Op, Context, AcceptValidated
using Rasm.Element.Composition;                     // MaterialId, DetailSchema, PropertyBag, PropertyName, PropertyValue
using Rasm.Element.Properties;
using Thinktecture;
using Count = Rasm.Numerics.Dimension;                 // the kernel discrete-count atom — spelled apart so the seam Dimension keeps the bare name every family page gives it
using Dimension = Rasm.Element.Properties.Dimension;   // the SI-dimension axis the detail-bag mints ride — disambiguated from the Rasm.Numerics discrete count
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The kind axis of the seed generator: the member-type vocabulary owning the COMPLETE entity-token binding
// (POLICY_VALUES — entity selection is a row read, never reconstructed at the seed). The verified GeometryGym
// IfcMechanicalFastenerTypeEnum carries BOLT/SCREW/NAIL/ANCHORBOLT/DOWEL/RIVET/COUPLER and NO NUT member, so the nut
// ROW binds IfcDiscreteAccessory/USERDEFINED — the entity split is vocabulary data, not a seed special case. Because
// USERDEFINED is the schema's own catch-all, the wire token alone leaves a nut indistinguishable from any other
// owner-labelled accessory, so DetailToken is the row's SEPARATE realization identity and the bag stamps THAT: a nut
// reads "NUT" on a shop document while the IFC enumeration still receives the only member it admits. The form flags
// drive the length split; anchor/dowel/rivet/coupler are arms HERE — ComponentFamily stays closed at ten.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerKind {
    public static readonly FastenerKind Bolt    = new("bolt",    ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "BOLT",        detailToken: "BOLT",    threaded: true,  headed: true);
    public static readonly FastenerKind Nut     = new("nut",     ifcEntity: "IfcDiscreteAccessory",  ifcPredefinedType: "USERDEFINED", detailToken: "NUT",     threaded: true,  headed: false);
    public static readonly FastenerKind Nail    = new("nail",    ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "NAIL",        detailToken: "NAIL",    threaded: false, headed: true);
    public static readonly FastenerKind Screw   = new("screw",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "SCREW",       detailToken: "SCREW",   threaded: true,  headed: true);
    public static readonly FastenerKind Anchor  = new("anchor",  ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "ANCHORBOLT",  detailToken: "ANCHOR",  threaded: true,  headed: true);
    public static readonly FastenerKind Dowel   = new("dowel",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "DOWEL",       detailToken: "DOWEL",   threaded: false, headed: false);
    public static readonly FastenerKind Rivet   = new("rivet",   ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "RIVET",       detailToken: "RIVET",   threaded: false, headed: true);
    public static readonly FastenerKind Coupler = new("coupler", ifcEntity: "IfcMechanicalFastener", ifcPredefinedType: "COUPLER",     detailToken: "COUPLER", threaded: true,  headed: false);
    public string IfcEntity { get; }
    public string IfcPredefinedType { get; }
    public string DetailToken { get; }   // the realization identity — distinct per kind where the IFC enumeration collapses to USERDEFINED
    public bool Threaded { get; }        // a dowel/rivet has no thread — ThreadLengthMm resolves 0, the body is all shank
    public bool Headed { get; }          // a headless threaded part (nut/coupler) threads through its whole length
}

// The pitch family. Metric and unified threads share ONE 60° form, so the series carries only what genuinely differs:
// the SYSTEM bit a grade admits against, and the ISO 898-1 versus ASME B1.1 tensile-stress-area coefficient — the two
// standards subtract 0.9382·P and 0.9743·P from the major diameter respectively, and that single coefficient IS the
// ~3% disagreement between their printed area tables. A fine row is one Threads entry naming this series with its own
// finer pitch: both columns are read the moment such a row exists, so the fine rows are reachable by admission.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSeries {
    public static readonly ThreadSeries MetricCoarse  = new("metric-coarse", metric: true,  stressAreaCoefficient: 0.9382);
    public static readonly ThreadSeries MetricFine    = new("metric-fine",   metric: true,  stressAreaCoefficient: 0.9382);
    public static readonly ThreadSeries UnifiedCoarse = new("unc",           metric: false, stressAreaCoefficient: 0.9743);
    public static readonly ThreadSeries UnifiedFine   = new("unf",           metric: false, stressAreaCoefficient: 0.9743);
    public bool Metric { get; }
    public double StressAreaCoefficient { get; }
}

// The EN 1993-1-8 Table 3.2 joint category — the bearing-vs-preloaded axis a CONNECTION selects, never a type-row
// column. The EN 1993-1-8 citation rides the capacity#SECTION_CAPACITY SectionCapacity.Code column on the lifted
// verdict, so no inert static and no re-transcribed clause string sits beside the rows. Preloaded gates the [03] slip
// projection; Shear selects WHICH resistance triple the capacity fold reports as governing, so a category-D tension
// connection never reports a shear verdict. A non-preloadable grade in a B/C/E joint rails at FastenerAssembly.Of.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltCategory {
    public static readonly BoltCategory A = new("A", shear: true,  preloaded: false);
    public static readonly BoltCategory B = new("B", shear: true,  preloaded: true);
    public static readonly BoltCategory C = new("C", shear: true,  preloaded: true);
    public static readonly BoltCategory D = new("D", shear: false, preloaded: false);
    public static readonly BoltCategory E = new("E", shear: false, preloaded: true);
    public bool Shear { get; }       // a shear category reports the shear/bearing pair; a tension category the tension resistance
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

// The head geometry as the TWO published EN 1993-1-8 Table 3.4 corrections it drives: k2, the tension-resistance
// coefficient (0.9 for every head the table names, 0.63 countersunk — a 30% reduction), and the bearing-thickness
// deduction, because a countersink removes half its own depth from the ply and the code sizes that depth at half the
// bolt diameter. One row carries both, so a countersunk connection cannot pick up one correction and miss the other.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HeadForm {
    public static readonly HeadForm Hexagon     = new("hexagon",     tensionFactor: 0.90, thicknessDeductionRatio: 0.00);
    public static readonly HeadForm Countersunk = new("countersunk", tensionFactor: 0.63, thicknessDeductionRatio: 0.25);
    public double TensionFactor { get; }             // k2
    public double ThicknessDeductionRatio { get; }   // × d, subtracted from the bearing ply depth
}

// The shear plane as its TWO independent published columns rather than one fused scalar: the AREA the plane cuts (the
// ISO/ASME tensile stress area through the thread, the gross shank area through the plain body) and the α_v the code
// tabulates for that plane. Table 3.4 gives the shank plane 0.6 for EVERY class while the threaded plane splits per
// class, so the shank arm answers a constant and the threaded arm reads the grade's own column — and a grade the
// Eurocode does not tabulate answers None on both, which is what makes the refusal reachable instead of implicit.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ShearPlane {
    const double ShankAlphaV = 0.60;
    public static readonly ShearPlane Threaded = new("threaded", static thread => thread.StressAreaMm2,  static grade => grade.EurocodeAlphaV);
    public static readonly ShearPlane Shank    = new("shank",    static thread => thread.NominalAreaMm2, static grade => grade.EurocodeAlphaV.Map(static _ => ShankAlphaV));
    [UseDelegateFromConstructor] public partial double ResistanceAreaMm2(ThreadRow thread);
    [UseDelegateFromConstructor] public partial Option<double> ShearFactor(GradeRow grade);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-size hex hardware envelope, carried for BOTH thread systems: head height, nut height, and the plain-washer
// bore/outside/thickness triple are dimensioned by ISO 4014/4032/7089 and by ASME B18.2.1/B18.2.2/B18.22.1 alike, so
// they are plain columns. The BEARING-FACE and UNDER-HEAD FILLET diameters are optional because only the ISO product
// declares them: an inch head is dimensioned across flats and corners, and a dw/da column on a UNC row could only be
// an ISO shape transplanted onto a product that never published one. Every column crosses into the realization bag as
// the fabrication envelope a shop reads.
public readonly record struct HexHardware(
    double HeadHeightMm, Option<double> BearingDiameterMm, Option<double> FilletDiameterMm,
    double NutHeightMm, double WasherInnerMm, double WasherOuterMm, double WasherThicknessMm);

// ISO 261/724 + ASME B1.1 thread row. Only the three columns the standards genuinely PRINT independently are stored —
// major diameter, pitch, and across-flats — because every remaining thread dimension is the ISO 68-1 60° form's own
// algebra over those three, and the previous table hand-typed four derived columns seventeen times. The basic minor
// d1 = d − 1.25·H, the pitch diameter d2 = d − 0.75·H, the rounded root d3 = d − (17/12)·H, and the fundamental
// triangle height H = P/(2·tan(α/2)) all descend from the ONE flank angle, so the form constant is read rather than
// pre-multiplied into four magic decimals. StressAreaMm2 is the printed tensile stress area reproduced EXACTLY by its
// own standard's formula over the series coefficient — the metric and unified tables disagree only in that
// coefficient, so one derivation serves both and a transcription slip in either is unrepresentable. Tag is the
// designation token for inch rows ("3/8" -> "0375"); Key doubles as the token for metric rows.
public readonly record struct ThreadRow(
    string Key, ThreadSeries Series, double MajorMm, double PitchMm, double AcrossFlatsMm,
    Option<HexHardware> Hardware = default, Option<string> Tag = default) {

    public const double InchToMm = 25.4;        // the ONE inch basis both the thread table and the grade bands convert on
    public const double FlankAngleDeg = 60.0;   // ISO 68-1 / ASME B1.1 included angle — the ONE form constant every geometric column below descends from

    public string Designation => Tag.IfNone(Key);
    public double FundamentalHeightMm => PitchMm / (2.0 * Math.Tan(FlankAngleDeg * Math.PI / 360.0));   // H
    public double MinorMm => MajorMm - 1.25 * FundamentalHeightMm;                                      // ISO 724 / ASME basic minor d1
    public double PitchDiameterMm => MajorMm - 0.75 * FundamentalHeightMm;                              // ISO 724 d2
    public double RootMinorMm => MajorMm - 17.0 / 12.0 * FundamentalHeightMm;                           // ISO 898-1 rounded-root d3 — the solid-model root
    public double AcrossCornersMm => AcrossFlatsMm * 2.0 / Math.Sqrt(3.0);                              // e = s·2/√3
    public double NominalAreaMm2 => Math.PI / 4.0 * MajorMm * MajorMm;                                  // gross shank area (shank-in-shear-plane)
    public double StressAreaMm2 => Math.PI / 4.0 * Math.Pow(MajorMm - Series.StressAreaCoefficient * PitchMm, 2.0);
    public double RunoutMm => 2.5 * PitchMm;                                                            // ISO 3508 incomplete-thread allowance x
    // EN 1993-1-8 §3.6.1(4) d_m: the mean of the across-points and across-flats dimensions of the head or nut,
    // whichever is smaller — the punching-shear diameter, distinct from the ISO 4014 washer-face dw the shop reads.
    public double PunchingDiameterMm => 0.5 * (AcrossFlatsMm + AcrossCornersMm);
}

// The >threshold mechanical step a size-banded class carries, in the units its own body prints. ISO 898-1 bands class
// 8.8 above M16; SAE J429 bands grade 2 above 3/4 in and grade 5 above 1 in; ASTM F3125 unified the legacy A325
// over-1-in reduction AWAY, so no F3125 row steps and a step transcribed onto one contradicts the specification that
// removed it.
public readonly record struct GradeStep(double AboveMm, double ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa);

// The diameter range a grade's own specification covers. It is REQUIRED rather than optional because every body scopes
// its classes, and the scope is what makes an unplaced roster row honest: class 9.8 exists only to M16, SAE 5.2 and
// 8.2 only to 1 in, the twist-off F3125 grades only to 1-1/4 in, and Admits refuses outside the band instead of
// pricing a size the specification never covered.
public readonly record struct SizeBand(double MinMm, double MaxMm) {
    public bool Covers(double diameterMm) => diameterMm >= MinMm && diameterMm <= MaxMm;
}

// ISO 898-1 / SAE J429 / ASTM F3125 grade row. ProofStressMpa is OPTIONAL because F3125 acceptance is tensile and
// yield alone — the proof-load stresses circulate in one secondary reproduction and typed absence is the honest
// carrier for a cell no second source confirms. SpecifiedUltimateMpa is the ultimate each body designates for DESIGN
// and preload, distinct from the acceptance minimum: an ISO class designates Rm,nom (the leading number × 100 — the
// EN 1993-1-8 Table 3.1 f_ub exactly), while the Table 3 acceptance minimum sits above it for the classes that round
// up. EurocodeAlphaV is the Table 3.4 THREADED-plane α_v and is Some ONLY for the seven classes Table 3.1 tabulates —
// 9.8, 12.9, and every SAE and ASTM grade carry None, because the Eurocode publishes no α_v for them and inventing
// one is the failure this column exists to make impossible. Preloadable is the §3.9 / RCSC slip-critical admission.
public readonly record struct GradeRow(
    string Key, ComponentStandard Standard, SizeBand Sizes,
    Option<double> ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa, double SpecifiedUltimateMpa,
    Option<double> EurocodeAlphaV, bool Preloadable, bool Metric,
    string SubstanceId, string AppearanceId, Option<GradeStep> Step = default) {

    public string Tag => Key.Replace(".", "");
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public MaterialId Appearance => MaterialId.Of(AppearanceId);
    // The SYSTEM exclusion and the SIZE SCOPE in one law: an ISO class pairs metric threads and an SAE/ASTM grade inch
    // threads, and either only within the diameter range its own body publishes.
    public bool Admits(ThreadRow thread) => Metric == thread.Series.Metric && Sizes.Covers(thread.MajorMm);

    // The effective mechanical band at a thread size — the ONE band read every projection routes through, so an M20
    // 8.8 reads 600/830/660 and an M12 8.8 580/800/640, never a hybrid row.
    public (Option<double> ProofStressMpa, double TensileStrengthMpa, double MinimumYieldMpa) At(ThreadRow thread) =>
        Step.Filter(s => thread.MajorMm > s.AboveMm)
            .Map(static s => (Some(s.ProofStressMpa), s.TensileStrengthMpa, s.MinimumYieldMpa))
            .IfNone((ProofStressMpa, TensileStrengthMpa, MinimumYieldMpa));
}

// --- [TABLES] ------------------------------------------------------------------------------
// 17 thread rows: 9 ISO 261 metric coarse + 8 ASME B1.1 UNC (Tag the decimal token), each carrying its own system's
// published hex envelope. NAMED statics (the connector Gauges form) so the Stocked selection references rows
// SYMBOLICALLY — a typo'd size is a compile miss, never a runtime key.
public static class Threads {
    // The ISO envelope in the millimetres ISO 4014/4032/7089 print, bearing face and fillet included.
    static Option<HexHardware> Iso(double headHeight, double bearing, double fillet, double nutHeight, double washerInner, double washerOuter, double washerThickness) =>
        Some(new HexHardware(headHeight, Some(bearing), Some(fillet), nutHeight, washerInner, washerOuter, washerThickness));

    // The ASME envelope in the INCHES B18.2.1 (head height), B18.2.2 (nut thickness), and B18.22.1 (Type A narrow
    // washer) print, converted once — a pre-rounded millimetre column would hide which digits the standard published
    // behind a conversion the reader has to invert. The heavy-hex and wide-washer series are DIFFERENT products with
    // their own tables, so a row here never blends them.
    static Option<HexHardware> Asme(double headHeightIn, double nutHeightIn, double washerInnerIn, double washerOuterIn, double washerThicknessIn) =>
        Some(new HexHardware(headHeightIn * ThreadRow.InchToMm, None, None, nutHeightIn * ThreadRow.InchToMm,
            washerInnerIn * ThreadRow.InchToMm, washerOuterIn * ThreadRow.InchToMm, washerThicknessIn * ThreadRow.InchToMm));

    // An inch row states the FRACTION and the threads per inch its own designation IS, plus the across-flats fraction
    // B18.2.1 prints, and the mint derives every millimetre column — so the 25.4-multiples and 25.4/n reciprocals the
    // table previously hand-carried cannot drift from the size they name.
    static ThreadRow Unc(string key, string tag, double inches, double threadsPerInch, double acrossFlatsIn, Option<HexHardware> hardware) =>
        new(key, ThreadSeries.UnifiedCoarse, inches * ThreadRow.InchToMm, ThreadRow.InchToMm / threadsPerInch, acrossFlatsIn * ThreadRow.InchToMm, hardware, tag);

    public static readonly ThreadRow M6     = new("m6",  ThreadSeries.MetricCoarse,  6.0, 1.00, 10.0, Iso(4.0,  8.74,  6.8,  5.2,  6.4,  12.0, 1.6));
    public static readonly ThreadRow M8     = new("m8",  ThreadSeries.MetricCoarse,  8.0, 1.25, 13.0, Iso(5.3,  11.47, 9.2,  6.8,  8.4,  16.0, 1.6));
    public static readonly ThreadRow M10    = new("m10", ThreadSeries.MetricCoarse, 10.0, 1.50, 16.0, Iso(6.4,  14.47, 11.2, 8.4,  10.5, 20.0, 2.0));
    public static readonly ThreadRow M12    = new("m12", ThreadSeries.MetricCoarse, 12.0, 1.75, 18.0, Iso(7.5,  16.47, 13.7, 10.8, 13.0, 24.0, 2.5));
    public static readonly ThreadRow M16    = new("m16", ThreadSeries.MetricCoarse, 16.0, 2.00, 24.0, Iso(10.0, 22.00, 17.7, 14.8, 17.0, 30.0, 3.0));
    public static readonly ThreadRow M20    = new("m20", ThreadSeries.MetricCoarse, 20.0, 2.50, 30.0, Iso(12.5, 27.70, 22.4, 18.0, 21.0, 37.0, 3.0));
    public static readonly ThreadRow M24    = new("m24", ThreadSeries.MetricCoarse, 24.0, 3.00, 36.0, Iso(15.0, 33.25, 26.4, 21.5, 25.0, 44.0, 4.0));
    public static readonly ThreadRow M30    = new("m30", ThreadSeries.MetricCoarse, 30.0, 3.50, 46.0, Iso(18.7, 42.75, 33.4, 25.6, 31.0, 56.0, 4.0));
    public static readonly ThreadRow M36    = new("m36", ThreadSeries.MetricCoarse, 36.0, 4.00, 55.0, Iso(22.5, 51.11, 39.4, 31.0, 37.0, 66.0, 5.0));
    public static readonly ThreadRow In0250 = Unc("1/4",   "0250", 0.250,  20.0, 0.4375, Asme(0.1719, 0.2188, 0.281, 0.625, 0.065));
    public static readonly ThreadRow In0375 = Unc("3/8",   "0375", 0.375,  16.0, 0.5625, Asme(0.2500, 0.3281, 0.406, 0.812, 0.065));
    public static readonly ThreadRow In0500 = Unc("1/2",   "0500", 0.500,  13.0, 0.7500, Asme(0.3438, 0.4375, 0.531, 1.062, 0.095));
    public static readonly ThreadRow In0625 = Unc("5/8",   "0625", 0.625,  11.0, 0.9375, Asme(0.4219, 0.5469, 0.656, 1.312, 0.095));
    public static readonly ThreadRow In0750 = Unc("3/4",   "0750", 0.750,  10.0, 1.1250, Asme(0.5000, 0.6406, 0.812, 1.469, 0.134));
    public static readonly ThreadRow In0875 = Unc("7/8",   "0875", 0.875,   9.0, 1.3125, Asme(0.5781, 0.7500, 0.938, 1.750, 0.134));
    public static readonly ThreadRow In1000 = Unc("1",     "1000", 1.000,   8.0, 1.5000, Asme(0.6719, 0.8594, 1.062, 2.000, 0.134));
    public static readonly ThreadRow In1500 = Unc("1-1/2", "1500", 1.500,   6.0, 2.2500, Asme(1.0000, 1.2813, 1.625, 3.000, 0.165));
    public static readonly ImmutableArray<ThreadRow> Rows = [M6, M8, M10, M12, M16, M20, M24, M30, M36, In0250, In0375, In0500, In0625, In0750, In0875, In1000, In1500];
}

// 19 grade rows: 9 ISO 898-1:2013 property classes, 6 SAE J429 grades, 4 ASTM F3125 grades. Each body mints through
// its own factory, because each PRINTS its data differently and the difference is the derivation: an ISO class
// DESIGNATES its nominal strengths (the leading number is Rm,nom/100 and the trailing number the yield ratio in
// tenths), so the class key IS the nominal pair and only Table 3's separately printed minimums are arguments; a US
// grade prints in ksi, so the ksi values are the arguments and the conversion happens once. Class 3.6 is ABSENT: it
// belongs to a withdrawn edition and no current-standard source carries it. SAE grades 4, 5.1, 7, and 8.1 are absent:
// grade 4's proof load, grade 5.1's size range, and grade 7's currency each fail the two-source bar.
public static class Grades {
    const double KsiToMpa = 6.894757;
    static readonly ComponentStandard EnIso = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);
    static readonly ComponentStandard Sae   = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Sae);
    static readonly ComponentStandard Astm  = new("us", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);
    static readonly SizeBand IsoRange = new(1.6, 39.0);        // ISO 898-1 scope M1.6–M39
    static readonly SizeBand SaeRange = new(0.25 * ThreadRow.InchToMm, 1.5 * ThreadRow.InchToMm);
    static readonly SizeBand SaeCapRange = new(0.25 * ThreadRow.InchToMm, 1.0 * ThreadRow.InchToMm);
    static readonly SizeBand F3125Range = new(0.5 * ThreadRow.InchToMm, 1.5 * ThreadRow.InchToMm);
    static readonly SizeBand TwistOffRange = new(0.5 * ThreadRow.InchToMm, 1.25 * ThreadRow.InchToMm);

    // An ISO class row: the designation carries the nominals, Table 3 carries the minimums, EN 1993-1-8 Table 3.1
    // carries the α_v where it tabulates the class at all.
    static GradeRow Iso(string designation, SizeBand sizes, double proofMpa, double tensileMinMpa, double yieldMinMpa,
        Option<double> alphaV, bool preloadable, Option<GradeStep> step = default) =>
        new(designation, EnIso, sizes, Some(proofMpa), tensileMinMpa, yieldMinMpa,
            SpecifiedUltimateMpa: double.Parse(designation.Split('.')[0]) * 100.0,
            alphaV, preloadable, Metric: true,
            $"steel.fastener-{designation.Replace('.', '_')}", tensileMinMpa >= 800.0 ? "metal.steel" : "metal.iron", step);

    // A US grade row: every strength arrives in the ksi its own table prints and converts once here, so no MPa literal
    // in this table is a hand-run conversion nothing can re-check.
    static GradeRow Us(string key, ComponentStandard standard, SizeBand sizes, Option<double> proofKsi,
        double tensileKsi, double yieldKsi, bool preloadable, Option<(double AboveIn, double ProofKsi, double TensileKsi, double YieldKsi)> step = default) =>
        new(key, standard, sizes, proofKsi.Map(static ksi => ksi * KsiToMpa), tensileKsi * KsiToMpa, yieldKsi * KsiToMpa,
            SpecifiedUltimateMpa: tensileKsi * KsiToMpa, EurocodeAlphaV: None, preloadable, Metric: false,
            $"steel.fastener-{key}", tensileKsi >= 120.0 ? "metal.steel" : "metal.iron",
            step.Map(static s => new GradeStep(s.AboveIn * ThreadRow.InchToMm, s.ProofKsi * KsiToMpa, s.TensileKsi * KsiToMpa, s.YieldKsi * KsiToMpa)));

    public static readonly GradeRow G46   = Iso("4.6",  IsoRange, 225.0,  400.0,  240.0, Some(0.60), false);
    public static readonly GradeRow G48   = Iso("4.8",  IsoRange, 310.0,  420.0,  340.0, Some(0.50), false);
    public static readonly GradeRow G56   = Iso("5.6",  IsoRange, 280.0,  500.0,  300.0, Some(0.60), false);
    public static readonly GradeRow G58   = Iso("5.8",  IsoRange, 380.0,  520.0,  420.0, Some(0.50), false);
    public static readonly GradeRow G68   = Iso("6.8",  IsoRange, 440.0,  600.0,  480.0, Some(0.50), false);
    public static readonly GradeRow G88   = Iso("8.8",  IsoRange, 580.0,  800.0,  640.0, Some(0.60), true, Some(new GradeStep(16.0, 600.0, 830.0, 660.0)));
    public static readonly GradeRow G98   = Iso("9.8",  new SizeBand(1.6, 16.0), 650.0, 900.0, 720.0, None, false);   // the class exists only to M16, and EN 1993-1-8 Table 3.1 does not tabulate it
    public static readonly GradeRow G109  = Iso("10.9", IsoRange, 830.0, 1040.0,  940.0, Some(0.50), true);
    public static readonly GradeRow G129  = Iso("12.9", IsoRange, 970.0, 1220.0, 1100.0, None, false);                // outside the Eurocode structural-bolt set
    public static readonly GradeRow Gr1   = Us("gr1",   Sae,  SaeRange,    Some(33.0),  60.0,  36.0, false);
    public static readonly GradeRow Gr2   = Us("gr2",   Sae,  SaeRange,    Some(55.0),  74.0,  57.0, false, Some((0.75, 33.0, 60.0, 36.0)));
    public static readonly GradeRow Gr5   = Us("gr5",   Sae,  SaeRange,    Some(85.0), 120.0,  92.0, false, Some((1.00, 74.0, 105.0, 81.0)));
    public static readonly GradeRow Gr52  = Us("gr5.2", Sae,  SaeCapRange, Some(85.0), 120.0,  92.0, false);
    public static readonly GradeRow Gr8   = Us("gr8",   Sae,  SaeRange,   Some(120.0), 150.0, 130.0, false);
    public static readonly GradeRow Gr82  = Us("gr8.2", Sae,  SaeCapRange, Some(120.0), 150.0, 130.0, false);
    public static readonly GradeRow A325  = Us("a325",  Astm, F3125Range,   None, 120.0,  92.0, true);
    public static readonly GradeRow F1852 = Us("f1852", Astm, TwistOffRange, None, 120.0,  92.0, true);
    public static readonly GradeRow A490  = Us("a490",  Astm, F3125Range,   None, 150.0, 130.0, true);
    public static readonly GradeRow F2280 = Us("f2280", Astm, TwistOffRange, None, 150.0, 130.0, true);
    public static readonly ImmutableArray<GradeRow> Rows = [
        G46, G48, G56, G58, G68, G88, G98, G109, G129,
        Gr1, Gr2, Gr5, Gr52, Gr8, Gr82, A325, F1852, A490, F2280];
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The single-fastener DESIGN values over (ThreadRow, GradeRow) — every projection here is already divided by its own
// partial factor, so the receipts Rasm.Compute reads off the seam fold demand directly and the group resistance and
// combined-action interaction compose them without re-dividing.
public static class Fastening {
    public const double GammaM2 = 1.25;   // EN 1993-1-8 §2.2 Table 2.1 recommended value for the resistance of bolts

    // F_v,Rd = α_v·f_ub·A/γM2. The α_v and the area both ride the ShearPlane row, and a grade EN 1993-1-8 does not
    // tabulate refuses HERE rather than borrowing a neighbouring class's factor.
    public static Fin<double> ShearResistanceKn(ThreadRow thread, GradeRow grade, ShearPlane plane, Op key) =>
        plane.ShearFactor(grade)
            .Map(alphaV => alphaV * grade.SpecifiedUltimateMpa * plane.ResistanceAreaMm2(thread) / GammaM2 * 1e-3)
            .ToFin(ComponentFault.Grade(key, $"<grade-outside-eurocode-bolt-set:{grade.Key}:{plane.Key}>"));

    // F_t,Rd = k2·f_ub·A_s/γM2 over the head's own k2.
    public static Fin<double> TensionResistanceKn(ThreadRow thread, GradeRow grade, HeadForm head, Op key) =>
        grade.EurocodeAlphaV
            .Map(_ => head.TensionFactor * grade.SpecifiedUltimateMpa * thread.StressAreaMm2 / GammaM2 * 1e-3)
            .ToFin(ComponentFault.Grade(key, $"<grade-outside-eurocode-bolt-set:{grade.Key}>"));

    // B_p,Rd = 0.6·π·d_m·t_p·f_u/γM2 — the EN 1993-1-8 §3.6.1(4) punching shear of the ply under the head or nut,
    // read off the head envelope's own across-flats/across-corners mean. It is a PLY resistance, so it takes the ply
    // scalars and no grade column at all.
    public static double PunchingResistanceKn(ThreadRow thread, double plyThicknessMm, double plyUltimateMpa) =>
        0.6 * Math.PI * thread.PunchingDiameterMm * plyThicknessMm * plyUltimateMpa / GammaM2 * 1e-3;

    // ISO 4014 reference thread length b = 2d+6 (L ≤ 125) / 2d+12 (125 < L ≤ 200) / 2d+25 (L > 200), clamped to L for
    // a short fully-threaded bolt; a headless threaded part (nut/coupler) threads its whole length; a dowel/rivet is
    // all shank.
    public static double ThreadLengthMm(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        !kind.Threaded ? 0.0
        : !kind.Headed ? lengthMm
        : Math.Min(lengthMm, lengthMm <= 125.0 ? 2.0 * thread.MajorMm + 6.0
            : lengthMm <= 200.0 ? 2.0 * thread.MajorMm + 12.0 : 2.0 * thread.MajorMm + 25.0);
    public static double UnthreadedShankMm(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        lengthMm - ThreadLengthMm(kind, thread, lengthMm);

    // EC5 §8.5 dowel-type TIMBER connection — the cross-material composition of the fastener and timber vocabularies.
    // Embedment enters ANGLED: f_h,0,k = 0.082·(1 − 0.01·d)·ρk is the parallel-to-grain value, and the §8.5.1.1
    // reduction f_h,α,k = f_h,0,k/(k90·sin²α + cos²α) carries it to the actual load-to-grain angle, k90 riding each
    // side's own material class (softwood 1.35 + 0.015d, LVL 1.30 + 0.015d, hardwood 0.90 + 0.015d). A bolt loaded
    // across the grain embeds at roughly two thirds of its parallel value on a softwood side, so an angle-free
    // embedment over-states every non-parallel connection the clause is written for. The fastener yield moment is
    // My,Rk = 0.3·fu,b·d^2.6, the per-shear-plane timber-to-timber single-shear characteristic Fv,Rk is the MINIMUM
    // over the six Johansen modes (the rope-effect Fax/4 term taken 0 — the withdrawal capacity is hardware-specific
    // data), and the design value is kmod·Fv,Rk/γM at the timber owner's own CONNECTION partial factor, which the EN
    // 1995-1-1 Table 2.3 connections row sets independently of any member form.
    public static Fin<double> TimberDowelShearKn(
        double diameterMm, double fastenerUltimateMpa, double loadToGrainDeg,
        TimberGrade side1, double t1Mm, TimberGrade side2, double t2Mm,
        ServiceClass service, LoadDuration duration, Op key) =>
        from admitted in guard(double.IsFinite(t1Mm) && t1Mm > 0.0 && double.IsFinite(t2Mm) && t2Mm > 0.0
                && double.IsFinite(diameterMm) && diameterMm > 0.0 && double.IsFinite(fastenerUltimateMpa) && fastenerUltimateMpa > 0.0
                && double.IsFinite(loadToGrainDeg),
            ComponentFault.Dimension(key, $"<dowel-inputs-rejected:d={diameterMm:R}:fu={fastenerUltimateMpa:R}:t1={t1Mm:R}:t2={t2Mm:R}:alpha={loadToGrainDeg:R}>"))
        let d = diameterMm
        let alpha = loadToGrainDeg * Math.PI / 180.0
        let sin2 = Math.Pow(Math.Sin(alpha), 2.0)
        let cos2 = Math.Pow(Math.Cos(alpha), 2.0)
        let fh1 = 0.082 * (1.0 - 0.01 * d) * side1.DensityK / ((side1.K90Base + 0.015 * d) * sin2 + cos2)
        let fh2 = 0.082 * (1.0 - 0.01 * d) * side2.DensityK / ((side2.K90Base + 0.015 * d) * sin2 + cos2)
        let beta = fh2 / fh1
        let my = 0.3 * fastenerUltimateMpa * Math.Pow(d, 2.6)
        let ratio = t2Mm / t1Mm
        let modeC = fh1 * t1Mm * d / (1.0 + beta)
            * (Math.Sqrt(beta + 2.0 * beta * beta * (1.0 + ratio + ratio * ratio) + beta * beta * beta * ratio * ratio) - beta * (1.0 + ratio))
        let modeD = 1.05 * fh1 * t1Mm * d / (2.0 + beta)
            * (Math.Sqrt(2.0 * beta * (1.0 + beta) + 4.0 * beta * (2.0 + beta) * my / (fh1 * d * t1Mm * t1Mm)) - beta)
        let modeE = 1.05 * fh1 * t2Mm * d / (1.0 + 2.0 * beta)
            * (Math.Sqrt(2.0 * beta * beta * (1.0 + beta) + 4.0 * beta * (1.0 + 2.0 * beta) * my / (fh1 * d * t2Mm * t2Mm)) - beta)
        let modeF = 1.15 * Math.Sqrt(2.0 * beta / (1.0 + beta)) * Math.Sqrt(2.0 * my * fh1 * d)
        let fvk = Seq(fh1 * t1Mm * d, fh2 * t2Mm * d, modeC, modeD, modeE, modeF).Min(double.PositiveInfinity)
        select duration.KmodFor(service) * fvk / TimberPartialFactor.Connection * 1e-3;
}

// The INVERSE query over both roster arrays: given a design shear demand and a thread system, the least-diameter
// admissible (thread, grade) pair whose EN 1993-1-8 resistance covers it. This is the capacity#SECTION_CAPACITY
// SectionSelection.Lightest law at fastener grain — a sizing scan, not a second capacity surface — and it is what
// makes every catalogued thread and grade REACHABLE: a row no Stocked selection names is still selected here the
// moment its size and system fit the demand, so the tables are the admission domain rather than decoration.
public static class FastenerSelection {
    public static Option<(ThreadRow Thread, GradeRow Grade, double ResistanceKn)> LeastShear(
        double demandKn, ThreadSeries series, ShearPlane plane, Op key) =>
        toSeq(Threads.Rows)
            .Filter(thread => thread.Series == series)
            .Bind(thread => toSeq(Grades.Rows)
                .Filter(grade => grade.Admits(thread))
                .Bind(grade => Fastening.ShearResistanceKn(thread, grade, plane, key)
                    .ToSeq()
                    .Filter(resistance => resistance >= demandKn)
                    .Map(resistance => (Thread: thread, Grade: grade, ResistanceKn: resistance))))
            .OrderBy(static candidate => candidate.Thread.MajorMm)
            .ThenBy(static candidate => candidate.ResistanceKn)
            .AsIterable()
            .Head();
}

// The seed-time DetailLane.Realization bag. The FastenerForm complex row carries the ISO 68-1 thread algebra and the
// ISO 4014/4032/7089 hex envelope the shop cuts and turns from — the derived geometry has exactly one consumer and it
// is the fabrication document, so the form columns are published rather than computed and discarded.
public static class FastenerDetail {
    public static Fin<PropertyBag> Of(FastenerKind kind, StockFacts facts, Option<ThreadRow> thread, Provenance source) =>
        from diameter in ComponentDetail.Measured(DetailSchema.NominalDiameter, Dimension.LengthDim, facts.DiameterMm * 1e-3)
        from length in ComponentDetail.Measured(DetailSchema.NominalLength, Dimension.LengthDim, facts.LengthMm * 1e-3)
        from form in thread.Match(Some: t => FormRow(kind, t, facts.LengthMm).Map(Some), None: static () => Fin.Succ(Option<(PropertyName, PropertyValue)>.None))
        select ComponentDetail.RealizationRows([
            ComponentDetail.Token(DetailSchema.FastenerType, kind.DetailToken),
            ComponentDetail.Sourced(source),
            diameter,
            length,
            .. form.ToSeq(),
        ]);

    static Fin<(PropertyName, PropertyValue)> FormRow(FastenerKind kind, ThreadRow thread, double lengthMm) =>
        from pitch in Si(thread.PitchMm)
        from minor in Si(thread.MinorMm)
        from pitchDiameter in Si(thread.PitchDiameterMm)
        from root in Si(thread.RootMinorMm)
        from corners in Si(thread.AcrossCornersMm)
        from runout in Si(thread.RunoutMm)
        from threaded in Si(Fastening.ThreadLengthMm(kind, thread, lengthMm))
        from shank in Si(Fastening.UnthreadedShankMm(kind, thread, lengthMm))
        select (DetailSchema.FastenerForm, (PropertyValue)new PropertyValue.Complex("fastener-form", Map(
            (PropertyName.Create("FlankAngle"), (PropertyValue)new PropertyValue.Text($"{ThreadRow.FlankAngleDeg:R}")),
            (PropertyName.Create("Pitch"), pitch),
            (PropertyName.Create("MinorDiameter"), minor),
            (PropertyName.Create("PitchDiameter"), pitchDiameter),
            (PropertyName.Create("RootDiameter"), root),
            (PropertyName.Create("AcrossCorners"), corners),
            (PropertyName.Create("ThreadRunout"), runout),
            (PropertyName.Create("ThreadLength"), threaded),
            (PropertyName.Create("UnthreadedShank"), shank))
            + thread.Hardware.Map(HexEnvelope).IfNone(Map<PropertyName, PropertyValue>())));

    // The columns every hex product declares ride the map unconditionally; the two only the ISO product dimensions are
    // OMITTED where absent rather than written as a zero, so a UNC bag content-keys on the envelope its standards
    // actually publish and a reader never mistakes a missing dimension for a measured one.
    static Map<PropertyName, PropertyValue> HexEnvelope(HexHardware hex) => Map(
        (PropertyName.Create("HeadHeight"), (PropertyValue)new PropertyValue.Text($"{hex.HeadHeightMm:R}")),
        (PropertyName.Create("NutHeight"), new PropertyValue.Text($"{hex.NutHeightMm:R}")),
        (PropertyName.Create("WasherInner"), new PropertyValue.Text($"{hex.WasherInnerMm:R}")),
        (PropertyName.Create("WasherOuter"), new PropertyValue.Text($"{hex.WasherOuterMm:R}")),
        (PropertyName.Create("WasherThickness"), new PropertyValue.Text($"{hex.WasherThicknessMm:R}")))
        + Declared("BearingDiameter", hex.BearingDiameterMm)
        + Declared("FilletDiameter", hex.FilletDiameterMm);

    static Map<PropertyName, PropertyValue> Declared(string name, Option<double> mm) =>
        mm.Match(
            Some: value => Map((PropertyName.Create(name), (PropertyValue)new PropertyValue.Text($"{value:R}"))),
            None: static () => Map<PropertyName, PropertyValue>());

    static Fin<PropertyValue> Si(double mm) =>
        MeasureValue.OfSi(Dimension.LengthDim, mm * 1e-3).Map(static value => (PropertyValue)new PropertyValue.Measure(value));
}

// --- [COMPOSITION] -------------------------------------------------------------------------
// The face BOTH stock cases answer, minted by ONE dispatch. Eight separate two-arm Switches over the same union were
// eight copies of one correspondence, so a ninth shared column meant a ninth dispatch; here it is one more field.
public readonly record struct StockFacts(
    FastenerKind Kind, string Designation, double DiameterMm, double LengthMm, double UltimateMpa,
    ComponentStandard Standard, MaterialId Substance, MaterialId Appearance);

// Threaded rows reference thread and grade currencies symbolically; plain rows carry only shank facts.
[Union]
public abstract partial record StockRow {
    private StockRow() { }
    public sealed record Threaded(FastenerKind Kind, ThreadRow Thread, GradeRow Grade, double LengthMm) : StockRow;
    // UltimateMpaColumn is the PUBLISHED tensile strength of the plain shank — ASTM F1667 common nail 690, EN 10025
    // dowel bar 400, ASTM A502 rivet 415 — the one datum the EC5 §8.5 yield-moment relation needs and no thread/grade
    // pair carries for a plain product.
    public sealed record Plain(
        FastenerKind Kind, string Designation, double DiameterMm, double LengthMm, double UltimateMpaColumn,
        ComponentStandard Standard, MaterialId Substance, MaterialId Appearance) : StockRow;

    // The ONE projection: a threaded row reads its grade's tensile strength at its own thread band, a plain row its
    // published column, and every downstream consumer — geometry, IFC binding, the detail bag, the EC5 dowel check,
    // Component.Of — reads the same record.
    public StockFacts Facts => Switch(
        threaded: static row => new StockFacts(
            row.Kind, $"{row.Thread.Designation}-{row.Grade.Tag}", row.Thread.MajorMm, row.LengthMm,
            row.Grade.At(row.Thread).TensileStrengthMpa, row.Grade.Standard, row.Grade.Substance, row.Grade.Appearance),
        plain: static row => new StockFacts(
            row.Kind, row.Designation, row.DiameterMm, row.LengthMm, row.UltimateMpaColumn,
            row.Standard, row.Substance, row.Appearance));

    public Option<ThreadRow> Thread => Switch(threaded: static row => Some(row.Thread), plain: static _ => Option<ThreadRow>.None);
    public Option<GradeRow> Grade => Switch(threaded: static row => Some(row.Grade), plain: static _ => Option<GradeRow>.None);

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

// The CONNECTION a catalogued fastener sits in — the state no stock row can carry, as one closed family whose cases
// each hold EXACTLY the evidence their receipt arm consumes. It rides ONE Option column on the capacity placement, so
// a bolted verdict costs the placement one decision rather than a per-family argument tail, and the modality is
// recoverable from the value alone rather than from a category flag beside it.
[Union]
public abstract partial record FastenerPlacement {
    private FastenerPlacement() { }
    public sealed record Bearing(BoltCategory Category, HeadForm Head, ShearPlane Plane, int GripPlies, int ShearPlanes, bool WithWasher, BearingDesign Ply) : FastenerPlacement;
    public sealed record SlipCritical(BoltCategory Category, FayingSurface Faying, HeadForm Head, int GripPlies, int ShearPlanes, bool WithWasher, FastenerInstallation Install) : FastenerPlacement;
    public sealed record TimberDowel(TimberGrade Side1, double Thickness1Mm, TimberGrade Side2, double Thickness2Mm, double LoadToGrainDeg, int ShearPlanes, ServiceClass Service, LoadDuration Duration) : FastenerPlacement;
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
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0625, Grades.F1852, 88.9),
        new StockRow.Threaded(FastenerKind.Bolt,    Threads.In0750, Grades.F2280, 101.6),
        new StockRow.Threaded(FastenerKind.Nut,     Threads.M16,    Grades.G88,  14.8),
        new StockRow.Threaded(FastenerKind.Nut,     Threads.M20,    Grades.G109, 18.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.M8,     Grades.G88,  40.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.M6,     Grades.G98,  30.0),
        new StockRow.Threaded(FastenerKind.Screw,   Threads.In0250, Grades.Gr2,  31.8),
        new StockRow.Threaded(FastenerKind.Coupler, Threads.M20,    Grades.G88,  60.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.M16,    Grades.G88,  200.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.M20,    Grades.G88,  250.0),
        new StockRow.Threaded(FastenerKind.Anchor,  Threads.In0750, Grades.A325, 304.8),
        new StockRow.Plain(FastenerKind.Nail,  "8d-common",   3.33, 63.5,  690.0, new ComponentStandard("us", 0.0, ComponentAuthority.Astm), MaterialId.Of("steel.fastener-nail"),  MaterialId.Of("metal.iron")),
        new StockRow.Plain(FastenerKind.Nail,  "10d-common",  3.76, 76.2,  690.0, new ComponentStandard("us", 0.0, ComponentAuthority.Astm), MaterialId.Of("steel.fastener-nail"),  MaterialId.Of("metal.iron")),
        new StockRow.Plain(FastenerKind.Dowel, "dowel-20",   20.0, 100.0, 400.0, new ComponentStandard("eu", 0.0, ComponentAuthority.En),   MaterialId.Of("steel.fastener-dowel"), MaterialId.Of("metal.steel")),
        new StockRow.Plain(FastenerKind.Rivet, "rivet-0500", 12.7, 38.1,  415.0, new ComponentStandard("us", 0.0, ComponentAuthority.Astm), MaterialId.Of("steel.fastener-rivet"), MaterialId.Of("metal.iron"))];

    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        toSeq(Stocked).Traverse(row =>
            from admitted in row.Admit(context.Key)
            let facts = row.Facts
            from profile in SectionProfile.Circle.Of(facts.DiameterMm, context.Key)
            from detail in FastenerDetail.Of(facts.Kind, facts, row.Thread, Stock)
            from item in Component.Of(
                ComponentFamily.Fastener, $"fastener.{facts.Kind.Key}-{facts.Designation}",
                profile,
                IfcBinding.Of(facts.Kind.IfcEntity, facts.Kind.IfcPredefinedType),
                Coring.None, facts.Standard, substanceId: facts.Substance, appearanceId: facts.Appearance,
                detail: Some(detail), context.Key)
            select new ComponentRow(item, Stock)).As();

    // Every stocked row transcribes a product standard whole — the thread geometry off ISO 68-1/261, the strength band
    // off the grade table, and the length off the product standard's own length series — so one provenance covers the
    // roster rather than a per-case selector whose arms would agree.
    static readonly Provenance Stock = Provenance.Published;

    static readonly FrozenDictionary<ComponentId, StockRow> Table =
        Stocked.ToFrozenDictionary(static row => ComponentId.Create($"fastener.{row.Facts.Kind.Key}-{row.Facts.Designation}"), static row => row);

    public static Fin<StockRow> Resolve(Component component, Op key) =>
        Table.TryGetValue(component.Designation, out StockRow row)
            ? Fin.Succ(row)
            : ComponentFault.Family(key, $"<fastener-row-unregistered:{component.Designation.Value}>");

    // The ComponentFamily.Fastener CAPACITY producer. A single fastener's design values are meaningless without the
    // CONNECTION it sits in, so the placement's FastenerPlacement column is the input and its case selects the
    // receipt: a bearing connection lifts the assembly its own Of already admitted, a preloaded connection the slip
    // state of that same assembly, and a dowel-type timber connection the EC5 per-plane value this page computes.
    // The refusal survives ONLY where the placement declares no fastener connection at all — that is the one state a
    // catalogue row genuinely cannot price, and it names the column that fixes it.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in Resolve(component, key)
        from connection in placement.Fastener.ToFin(
            ComponentFault.Capacity(key, $"<fastener-capacity-needs-placement-connection:{component.Designation.Value}>"))
        from receipt in connection.Switch(
            bearing: state =>
                from thread in row.Thread.ToFin(ComponentFault.Family(key, $"<bearing-connection-over-plain-stock:{row.Facts.Designation}>"))
                from grade in row.Grade.ToFin(ComponentFault.Family(key, $"<bearing-connection-over-plain-stock:{row.Facts.Designation}>"))
                from assembly in FastenerAssembly.Of(thread, grade, state.Category, FayingSurface.None, state.Head, state.GripPlies, state.ShearPlanes, state.WithWasher, key)
                select (CapacityReceipt)new CapacityReceipt.Bolt(component.Designation, assembly, state.Ply, state.Plane),
            slipCritical: state =>
                from thread in row.Thread.ToFin(ComponentFault.Family(key, $"<preloaded-connection-over-plain-stock:{row.Facts.Designation}>"))
                from grade in row.Grade.ToFin(ComponentFault.Family(key, $"<preloaded-connection-over-plain-stock:{row.Facts.Designation}>"))
                from assembly in FastenerAssembly.Of(thread, grade, state.Category, state.Faying, state.Head, state.GripPlies, state.ShearPlanes, state.WithWasher, key)
                select (CapacityReceipt)new CapacityReceipt.SlipCritical(component.Designation, assembly, state.Install),
            timberDowel: state =>
                from perPlane in Fastening.TimberDowelShearKn(
                    row.Facts.DiameterMm, row.Facts.UltimateMpa, state.LoadToGrainDeg,
                    state.Side1, state.Thickness1Mm, state.Side2, state.Thickness2Mm, state.Service, state.Duration, key)
                select (CapacityReceipt)new CapacityReceipt.TimberDowel(component.Designation, perPlane, state.ShearPlanes))
        select SectionCapacity.Lift(receipt);
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` owns the installed bolt state and its own resistance projections; `BearingDesign` owns the ply the shank bears against and derives its EN 1993-1-8 Table 3.4 factors from the bolt-group geometry; `BoltPosition` and `HoleShape` own the published position and hole-form policy; `FastenerInstallation` admits the shared `(ks, γM3, km)` slip-and-torque policy.
- Cases: one assembly shape for every modality — a non-preloaded (A/D) assembly resolves `FayingSurface.None` and returns `None` for preload, slip, and tightening torque; a preloaded (B/C/E) assembly requires a named slip class and returns `Some` design values — never a numeric absence sentinel and never a `PreloadedBolt`/`BearingBolt` pair. `BoltPosition` closes the four-cell product of the two independent Table 3.4 discriminants: end-versus-inner along the load path selects α_d, edge-versus-inner across it selects k1.
- Entry: `FastenerAssembly.Of(thread, grade, category, faying, head, gripPlies, shearPlanes, withWasher, key)` rails a system- or size-mismatched thread/grade pair, a preloaded category over a non-`Preloadable` grade, a preloaded category with `FayingSurface.None`, and non-positive discrete counts before constructing the one assembly owner. `BearingDesign.Of` admits the ply and its bolt-group distances once.
- Growth: a new connection modality is a `BoltCategory`/`FayingSurface` row the assembly reads; a new hole form one `HoleShape` row; a new bolt-group position one `BoltPosition` row; the multi-bolt group `ΣFs,Rd`, the long-joint `β`, and the `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` interaction are `Rasm.Compute` consumers over these single-bolt design values.
- Boundary: `Count` admits the discrete grip and shear-plane columns. `BearingDesign` takes the DISTANCES the code's own formulas consume and derives `k1` and `α_b` from them, so a caller cannot hand the resistance one opaque scalar in which a transposed edge and end distance is invisible; the hole-shape reduction and the countersink thickness deduction are rows the same derivation reads. The preload is bounded by the grade's own yield load, because a pretension above the elastic limit is a tightening method the assembly cannot represent.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The EN 1993-1-8 Table 3.4 bolt position as the FLATTENED PRODUCT of its two independent discriminants: the
// load-path position selects α_d (an end bolt reads its end distance e1, an inner bolt its pitch p1 less a quarter),
// and the transverse position selects k1 (an edge bolt reads 2.8·e2/d0 − 1.7, an inner bolt 1.4·p2/d0 − 1.7). One row
// per cell keeps both reads at one level and makes a mis-paired rule unrepresentable.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltPosition {
    public static readonly BoltPosition EndEdge     = new("end-edge",     static (l, d0) => l / (3.0 * d0),        static (t, d0) => 2.8 * t / d0 - 1.7);
    public static readonly BoltPosition EndInner    = new("end-inner",    static (l, d0) => l / (3.0 * d0),        static (t, d0) => 1.4 * t / d0 - 1.7);
    public static readonly BoltPosition InnerEdge   = new("inner-edge",   static (l, d0) => l / (3.0 * d0) - 0.25, static (t, d0) => 2.8 * t / d0 - 1.7);
    public static readonly BoltPosition InnerInner  = new("inner-inner",  static (l, d0) => l / (3.0 * d0) - 0.25, static (t, d0) => 1.4 * t / d0 - 1.7);
    [UseDelegateFromConstructor] public partial double AlphaD(double loadwiseMm, double holeMm);
    [UseDelegateFromConstructor] public partial double K1Raw(double transverseMm, double holeMm);
}

// The EN 1993-1-8 Table 3.4 hole-form reduction on the bearing resistance.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HoleShape {
    public static readonly HoleShape Normal                = new("normal",                 bearingFactor: 1.0);
    public static readonly HoleShape Oversize              = new("oversize",               bearingFactor: 0.8);
    public static readonly HoleShape SlottedPerpendicular  = new("slotted-perpendicular",  bearingFactor: 0.6);
    public double BearingFactor { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The ply the shank bears against, carrying the GEOMETRY EN 1993-1-8 Table 3.4 consumes rather than a pre-collapsed
// scalar: the loadwise distance (e1 for an end bolt, p1 for an inner one), the transverse distance (e2 or p2), the
// hole diameter, its form, and the bolt-group position. k1 and α_b are DERIVED here, so the two published factors
// stay separable on the receipt and an edge distance transposed into the end slot changes the answer visibly instead
// of disappearing into one number. The generated validation owns the positive-finite guard.
[ComplexValueObject]
public readonly partial struct BearingDesign {
    public double PlyThicknessMm { get; }
    public double PlyUltimateMpa { get; }
    public double LoadwiseDistanceMm { get; }
    public double TransverseDistanceMm { get; }
    public double HoleDiameterMm { get; }
    public HoleShape Hole { get; }
    public BoltPosition Position { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref double plyThicknessMm, ref double plyUltimateMpa,
        ref double loadwiseDistanceMm, ref double transverseDistanceMm, ref double holeDiameterMm,
        ref HoleShape hole, ref BoltPosition position) =>
        validationError = hole is not null && position is not null
            && double.IsFinite(plyThicknessMm) && plyThicknessMm > 0.0
            && double.IsFinite(plyUltimateMpa) && plyUltimateMpa > 0.0
            && double.IsFinite(loadwiseDistanceMm) && loadwiseDistanceMm > 0.0
            && double.IsFinite(transverseDistanceMm) && transverseDistanceMm > 0.0
            && double.IsFinite(holeDiameterMm) && holeDiameterMm > 0.0
            ? null
            : new ValidationError($"<bearing-design-invalid:t={plyThicknessMm:R}:fu={plyUltimateMpa:R}:e1={loadwiseDistanceMm:R}:e2={transverseDistanceMm:R}:d0={holeDiameterMm:R}>");

    public static Fin<BearingDesign> Of(
        double plyThicknessMm, double plyUltimateMpa, double loadwiseDistanceMm, double transverseDistanceMm,
        double holeDiameterMm, HoleShape hole, BoltPosition position, Op key) =>
        Validate(plyThicknessMm, plyUltimateMpa, loadwiseDistanceMm, transverseDistanceMm, holeDiameterMm, hole, position, out BearingDesign design) is { } error
            ? Fin.Fail<BearingDesign>(ComponentFault.Dimension(key, error.Message))
            : Fin.Succ(design);

    public double K1 => Math.Min(Position.K1Raw(TransverseDistanceMm, HoleDiameterMm), 2.5);
    public double AlphaB(GradeRow grade) =>
        Math.Min(Math.Min(Position.AlphaD(LoadwiseDistanceMm, HoleDiameterMm), grade.SpecifiedUltimateMpa / PlyUltimateMpa), 1.0);

    // F_b,Rd = k1·α_b·f_u·d·t/γM2 over the BOLT's nominal diameter and the thinnest connected ply, the countersink
    // removing its own half-depth — sized at a quarter of the bolt diameter — from that thickness.
    public double ResistanceKn(ThreadRow thread, GradeRow grade, HeadForm head) =>
        Hole.BearingFactor * K1 * AlphaB(grade) * PlyUltimateMpa * thread.MajorMm
            * (PlyThicknessMm - head.ThicknessDeductionRatio * thread.MajorMm) / Fastening.GammaM2 * 1e-3;
}

// The EN 1993-1-8 §3.9 / EN 1090-2 §8.5 installation design set admitted ONCE: ks the hole-tolerance factor, γM3 the
// slip partial factor, km the manufacturer-declared EN 14399-2 k-class torque factor. The generated validation owns
// the positive-finite guard, so no raw factor reaches a slip or torque projection.
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

// The complete bolt-connection receipt over the standards rows: preload, slip, and washer projections are the
// EN 1993-1-8 §3.9 single-bolt design values the Rasm.Compute slip-critical and combined-action checks read.
public readonly record struct FastenerAssembly(
    ThreadRow Thread, GradeRow Grade, BoltCategory Category, FayingSurface Faying, HeadForm Head,
    Count GripPlies, Count ShearPlanes, bool WithWasher) {

    public static Fin<FastenerAssembly> Of(
        ThreadRow thread, GradeRow grade, BoltCategory category, FayingSurface faying, HeadForm head,
        int gripPlies, int shearPlanes, bool withWasher, Op key) =>
        from system in guard(grade.Admits(thread), ComponentFault.Grade(key, $"<grade-refuses-thread:{thread.Key}:{grade.Key}>"))
        from preload in guard(!category.Preloaded || grade.Preloadable, ComponentFault.Grade(key, $"<non-preloadable-grade-in-preloaded-joint:{grade.Key}:{category.Key}>"))
        from surface in guard(!category.Preloaded || faying != FayingSurface.None, ComponentFault.Dimension(key, $"<preloaded-joint-without-faying-class:{category.Key}>"))
        from plies in key.AcceptValidated<Count>(candidate: gripPlies)
        from planes in key.AcceptValidated<Count>(candidate: shearPlanes)
        select new FastenerAssembly(thread, grade, category, category.Preloaded ? faying : FayingSurface.None, head, plies, planes, withWasher);

    // The two published loads a tightening must stay under, and the governing one. The PROOF load is the stress the
    // specification requires the bolt to sustain with no permanent set, and where a body prints it, it binds tighter
    // than yield — an ISO 8.8 proofs at 580 MPa against a 640 MPa yield — so the ceiling is the lesser of the two and
    // a grade whose body prints no proof load falls back to nothing: it is bounded by its yield alone.
    public double YieldLoadKn => Grade.At(Thread).MinimumYieldMpa * Thread.StressAreaMm2 * 1e-3;
    public Option<double> ProofLoadKn => Grade.At(Thread).ProofStressMpa.Map(stress => stress * Thread.StressAreaMm2 * 1e-3);
    public double PreloadCeilingKn => ProofLoadKn.Map(proof => Math.Min(proof, YieldLoadKn)).IfNone(YieldLoadKn);

    // Fp,C = 0.7·fub·As over the size-banded read. None IS a snug-tight non-preloaded connection, the absence the
    // Rasm.Compute consumer reads through the Option — never numeric zero, which would price a preload the joint has
    // not. A pretension above the ceiling answers None as well: that is not a weaker preload, it is a tightening the
    // assembly does not represent.
    public Option<double> PreloadKn =>
        Category.Preloaded
            ? Some(0.7 * Grade.SpecifiedUltimateMpa * Thread.StressAreaMm2 * 1e-3).Filter(preload => preload <= PreloadCeilingKn)
            : None;

    public Option<double> SlipResistanceKn(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Ks * ShearPlanes.Value * Faying.SlipFactor * preload / design.GammaM3);

    public Option<double> TighteningTorqueNm(FastenerInstallation design) =>
        PreloadKn.Map(preload => design.Km * (Thread.MajorMm * 1e-3) * (preload * 1e3));

    // The group shear over every plane, the tension under the head's own k2, and the bearing against the ply — the
    // three columns the Connection verdict folds, each already a design resistance.
    public Fin<double> ShearResistanceKn(ShearPlane plane, Op key) =>
        Fastening.ShearResistanceKn(Thread, Grade, plane, key).Map(perPlane => perPlane * ShearPlanes.Value);
    public Fin<double> TensionResistanceKn(Op key) => Fastening.TensionResistanceKn(Thread, Grade, Head, key);
    public double BearingResistanceKn(BearingDesign ply) => ply.ResistanceKn(Thread, Grade, Head);
    public double PunchingResistanceKn(BearingDesign ply) =>
        Fastening.PunchingResistanceKn(Thread, ply.PlyThicknessMm, ply.PlyUltimateMpa);

    // ISO 7090 300 HV (chamfered, preloaded high-strength) vs ISO 7089 200 HV (plain). A connection with no washer
    // has no washer hardness — absence, not the hardness of a part that is not there.
    public Option<double> WasherHardnessHv => WithWasher ? Some(Grade.Preloadable ? 300.0 : 200.0) : None;

    public Option<double> NutHeightMm => Thread.Hardware.Map(static h => h.NutHeightMm);
    public Option<double> WasherOuterMm => WithWasher ? Thread.Hardware.Map(static h => h.WasherOuterMm) : None;
    public Option<double> WasherThicknessMm => WithWasher ? Thread.Hardware.Map(static h => h.WasherThicknessMm) : None;
}
```

## [04]-[RESEARCH]

(none)
