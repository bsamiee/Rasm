# [MATERIALS_FASTENER]

THE FASTENER COMPONENTFAMILY and THE BOLT-CAPACITY-AND-ASSEMBLY OWNER. The fastener vocabulary — the `FastenerKind` member-type discriminant (bolt · nut · screw · anchor · dowel · rivet · coupler), the `FastenerStandard` spec discriminant (ISO 898-1 · SAE J429 · ASTM F3125), the `FastenerGrade` proof/tensile/shear-factor strength axis, the `BoltCategory` EN 1993-1-8 bearing-vs-preloaded axis, the `FayingSurface` slip-coefficient class, the `ThreadSeries`/`ThreadSize` nominal-thread axis carrying the ISO 261/724 basic thread form AND the validated `HexHardware` head/nut/washer envelope, and the `FastenerSection` thread-and-capacity receipt — is the realized mechanical-fastener vocabulary one `component#COMPONENT_OWNER` `Component` carries in the `ComponentFamily.Fastener` case, a `ComponentClass.Minor` standardized part (`IfcElementComponent`: one type, MANY placed pieces). A 3/8in hex bolt is a `Component` row, never a `Bolt` type: the kind, the grade, the thread receipt, the category, and the head/nut/washer geometry are fastener-`Component` columns, and the `FastenerSection` projection feeds the same `component#COMPONENT_OWNER` `Component.Of` admission and the same `ComponentCatalogue.Build` fold the reinforcement (`reinforcement#REINFORCEMENT_FAMILY`) and connector (`connector#CONNECTOR_FAMILY`) families drive — a bolt pattern places through the construction layout fold over one `Component`, never a per-family schedule owner. This owner is ALSO the host-neutral BOLT-CAPACITY assembler: the `FastenerSection` capacity family projects the single-bolt shear/bearing/proof design values (the AISC 360 J3 / EN 1993-1-8 Table 3.4 resistances) and the `FastenerAssembly` owner composes the bolt + grip-plies + nut + washer + preload of a complete connection (`Fp,C = 0.7·fub·As`), mirroring how `reinforcement#RC_SECTION` is the second owner block on the reinforcement page. The `anchor` member type is a `FastenerKind` arm folded INSIDE this vocabulary, NEVER a separate `ComponentFamily` sibling: a cast-in anchor bolt and a post-installed expansion anchor are `FastenerKind.Anchor` rows that key and serialize the way a structural bolt does, and an unthreaded shear `dowel`, a `rivet`, and a bar `coupler` are the same one `FastenerKind` axis grown by data, never per-kind types. The fastener vocabulary grows by data — a new property class is one `FastenerGrade` row, a new thread size one `ThreadSize` row, a new designation one `FastenerRow` catalogue entry — never a per-bolt type. The page captures the COMPLETE generative data a host materializes a bolt+nut+washer solid from: the ISO 68-1/261/724 thread form (`FlankAngleDeg` 60, pitch `P`, pitch diameter `d2`, rounded-root minor `d3`, nut basic minor `D1`, the 6g/6H tolerance classes), the ISO 4014 hex head (`s` across-flats, `e` across-corners, `k` height, `dw` washer-face, `da` fillet, `b` thread length, `u` run-out, `ds` shank), the ISO 4032 hex nut (`s`/`m`), the ISO 7089/7090 plain washer (`d1`/`d2`/`t` + 200/300 HV), and the shank-vs-thread split (`b = 2d+6 / 2d+12 / 2d+25`, `UnthreadedShank = L − b`) — hand-rolled in-fence because no admitted .NET package owns the ISO 898-1 bolt property classes (`VividOrange.Materials` `EnSteelGrade` is EN structural-MEMBER grade DATA only, `.api/api-vividorange-materials.md` `[FLOOR_SCOPE_GATE]`), the EN 1993-1-8 joint code (`En1993Part.Part1_8`, `.api/api-vividorange-standards.md`) CITED on the `BoltCategory` axis. The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentId`/`ComponentSection`/`ComponentFault` shape, the `Rasm` kernel `PositiveMagnitude` for every thread/length/head column and `Dimension` for the discrete grip-ply/shear-plane count, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel`/`metal.iron`) appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here — the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa` axis is the spec-nominal grade band, the measured base-metal capacity the property-library receipt; the reinforcement, connector, and joint families land their own sibling vocabularies on `reinforcement#REINFORCEMENT_FAMILY`, `connector#CONNECTOR_FAMILY`, and `joint#JOINT_FAMILY`.

## [01]-[INDEX]

- [02]-[FASTENER_FAMILY]: the `FastenerKind` member-type discriminant, the `FastenerStandard` spec discriminant, the `FastenerGrade` proof/tensile/shear-factor axis, the `BoltCategory` bearing-vs-preloaded axis, the `FayingSurface` slip-coefficient class, the `ThreadSeries`/`ThreadSize` nominal-thread axis carrying the ISO 261/724 thread form + the `HexHardware` ISO 4014/4032/7089 head/nut/washer envelope + the ISO 898-1 metric / ASME B1.1 inch tensile stress area, the `FastenerSection` thread-and-capacity receipt with the single-bolt shear/bearing/proof family and the shank-vs-thread split, and the `ComponentCatalogue.BuildFastenerRows` ISO 898-1 / SAE J429 / ASTM F3125 row table.
- [03]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — the bolt + grip-plies (`Dimension`) + nut + washer admission over one `FastenerSection`, the `PreloadKn` `Fp,C = 0.7·fub·As` projection, the `SlipResistanceKn` EN 1993-1-8 preloaded design value the category-B/C/E slip-critical seam reads, and the ISO 7089/7090 washer-hardness selection the high-strength joint reads.

## [02]-[FASTENER_FAMILY]

- Owner: the fastener vocabulary (`FastenerKind` the bolt/nut/screw/anchor/dowel/rivet/coupler member-type discriminant, `FastenerStandard` the ISO 898-1 / SAE J429 / ASTM F3125 spec discriminant, `FastenerGrade` the proof/tensile/shear-factor strength axis, `BoltCategory` the EN 1993-1-8 bearing-vs-preloaded category axis, `FayingSurface` the slip-coefficient class, `ThreadSeries`/`ThreadSize` the M6..M36 / 1/4in..1-1/2in nominal-thread axis carrying the ISO 261/724 basic profile + the `HexHardware` head/nut/washer envelope, `FastenerSection` the thread-and-capacity receipt); `ComponentCatalogue.BuildFastenerRows` the registered-row seed `component#COMPONENT_OWNER` `ComponentCatalogue.Build` folds; the `ThreadSize.TensileStressAreaMm2` projection emitting the standard-correct stress area the bolt-capacity family reads.
- Cases: kind {bolt (externally threaded, headed) · nut (internally threaded mate, an `IfcDiscreteAccessory` companion at the wire) · screw (self-driven, no nut) · anchor (cast-in or post-installed) · dowel (unthreaded shear pin) · rivet (driven permanent) · coupler (bar/rebar mechanical splice)} · standard {ISO 898-1 (metric property class) · SAE J429 (inch grade) · ASTM F3125 (structural-bolt, A325/A490 lineage)} · grade {ISO 898-1 cls 4.6 / 4.8 / 5.6 / 5.8 / 6.8 / 8.8 / 10.9 / 12.9 · SAE J429 Gr2 / Gr5 / Gr8 · ASTM F3125 A325 / A490} · category {A (bearing, non-preloaded) · B (slip-critical at SLS) · C (slip-critical at ULS) · D (tension, non-preloaded) · E (tension, preloaded)} · size {metric coarse M6..M36 · imperial UNC 1/4in..1-1/2in} — a fastener is a `Component` row over one `FastenerKind`, one `FastenerGrade`, one `ThreadSize`, and one `BoltCategory`, never a fastener subtype.
- Entry: `public double TensileStressAreaMm2 { get; }` on `ThreadSize` — the standard-correct nominal tensile stress area (the ISO 898-1 `As = (π/4)·((d2+d3)/2)²` over the pitch/minor diameters for a metric thread, the ASME B1.1 Unified `As = 0.7854·(d − 0.9743·P)²` for an inch thread — a single derived `double` projection dispatched on `Series.Metric`, the area both the `FastenerSection` capacity family and the `FastenerAssembly` preload read, never re-computed at each call site); `FastenerSection.ShearCapacityKn(bool threadsInShearPlane)` / `BearingCapacityKn(double plyThicknessMm, double plyUltimateMpa, double edgeFactor)` / `ProofLoadKn` / `TensileLoadKn` project the AISC 360 J3 / EN 1993-1-8 single-bolt resistances over the stored thread columns and the grade band (a derived `double` projection over the stored `PositiveMagnitude` columns, exactly as `reinforcement#REINFORCEMENT_FAMILY` `RebarSection.YieldForceKn` projects); `ComponentCatalogue.BuildFastenerRows(context)` folds the ISO/SAE/ASTM `FastenerRow` table through `FastenerOf` into the registered `Component` rows `ComponentCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBoltBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` for the thread-diameter/pitch/length/head length columns and `Dimension` for the discrete grip-ply/shear-plane count, never an int-backed `Dimension` that truncates a fractional millimetre pitch), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the kind/standard/grade/category/faying/series/size axes with the generated total `Switch`, `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold`/`Option` for the admission rail, the catalogue fold, and the `HexHardware` envelope), BCL inbox (`FrozenDictionary`). No bolt-grade external package — `VividOrange.Materials` is EN-only structural-MEMBER grade DATA (`EnSteelGrade` S235..S460), with NO ISO 898-1 / SAE / ASTM F3125 bolt-property-class producer (`.api/api-vividorange-materials.md` `[FLOOR_SCOPE_GATE]`), so the bolt property classes AND the ISO 4014/4032/7089 fastener geometry are the realized catalogue vocabulary here exactly as `reinforcement#REINFORCEMENT_FAMILY` hand-keys the ASTM bar bands; the structural design code is CITED (`VividOrange.Standards` `En1993Part.Part1_8` joints, `.api/api-vividorange-standards.md`) on the `BoltCategory` axis, never re-minted.
- Growth: the fastener vocabulary grows by data — a new property class is one `FastenerGrade` row carrying its proof/tensile/yield/shear-factor, a new thread size one `ThreadSize` row carrying its major-diameter/pitch/minor-diameter + `HexHardware` envelope, a new member type one `FastenerKind` row, a new bolt category one `BoltCategory` row, a new designation one `FastenerRow` catalogue entry — never a per-bolt type, never a per-grade `Component` variant. `anchor`/`dowel`/`rivet`/`coupler` are `FastenerKind` arms folded INSIDE this vocabulary, so the `ComponentFamily` axis stays closed at ten (masonry/cmu/steel/timber/glazing/reinforcement/fastener/connector/joint/panel), never an eleventh sibling family or a per-bolt type. The single-bolt resistances the `FastenerSection`/`FastenerAssembly` project are the `[02]`/`[03]` owner concern; a multi-bolt connection-group resistance (the `ΣFv,Rd` group reduction, the long-joint β factor, the combined shear+tension interaction) is the `Rasm.Compute` design-check concern reading these single-bolt receipts off the seam, never re-derived here. The ASME B18.2.1/B18.2.2/B18.22.1 INCH head/nut/washer envelope rides the `ThreadSize.Hardware` `Option<HexHardware>` `None` arm today; the inch UNC `HexHardware` rows seed when an inch-detailed generative target lands (the `reinforcement#REINFORCEMENT_FAMILY` `BarSize.Catalogue` `Option<BarDiameter>` None-for-ASTM mirror).
- Boundary: the fastener vocabulary is a realized `ComponentFamily` — a per-bolt `Bolt`/`Nut`/`Screw`/`Anchor`/`Dowel`/`Rivet`/`Coupler` class is the deleted form, collapsed into the one `FastenerKind` `[SmartEnum]` arm (the 3+-parallel-item-shape collapse trigger), and the cast-in/post-installed anchor is the `FastenerKind.Anchor` row, NEVER a separate `ComponentFamily` case; `FastenerSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so a fractional ISO thread (`M12` major `12.000 mm`, pitch `1.75 mm`; `3/8in` major `9.525 mm`, 16-TPI pitch `1.5875 mm`) admits without the truncation an int-backed `Dimension` count forces, the `Dimension` carrier reserved for discrete counts (the `FastenerAssembly` grip-ply / shear-plane counts); the COMPLETE generative geometry is captured so a host materializes a real bolt+nut+washer solid — the ISO 261/724 thread form is derived once on `ThreadSize` (`PitchDiameterMm` `d2 = d − 0.649519·P`, `NutMinorDiameterMm` `D1 = d − 1.082532·P`, `FundamentalHeightMm` `H = 0.866025·P`, the stored `MinorDiameterMm` the rounded-root `d3 = d − 1.226869·P`, `FlankAngleDeg` the const 60° ISO 68-1 included angle, the 6g external / 6H internal ISO 965 tolerance classes), the ISO 4014 hex head + ISO 4032 nut + ISO 7089 washer in the per-size `HexHardware` envelope (`k`/`dw`/`da` head, `m` nut, `d1`/`d2`/`t` washer), and the shank-vs-thread split on `FastenerSection` (`LengthMm` the overall length `L`, `ThreadLengthMm` `b = 2d+6 / 2d+12 / 2d+25`, `UnthreadedShankMm = L − b`, `ThreadRunoutMm` the ISO 3508 `x ≤ 2.5·P` incomplete-thread allowance, `ds` the major-diameter shank); the tensile stress area is the `ThreadSize.TensileStressAreaMm2` derived projection — the ISO 898-1 mean-of-`d2`-and-`d3` area for a metric thread, the ASME B1.1 Unified `0.7854·(d − 0.9743·P)²` for an inch thread (the metric formula OVER-predicts an inch thread by ~3% and is the deleted form), NOT a host computation; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for alloy-steel cls 8.8/10.9/12.9 and SAE Gr5/Gr8 and structural A325/A490, `metal.iron` for low-carbon cls 4.6/4.8/5.6/SAE Gr2) the row's `Component.AppearanceId` column carries, never a fastener-specific shade; the mechanical base-metal capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa` axis is the spec-nominal class band and the measured capacity is the property-library receipt the bearing projection's `plyUltimateMpa` supplies; `ComponentCatalogue.BuildFastenerRows` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue.Rows` table with the ISO 898-1 / SAE J429 / ASTM F3125 rows keyed `fastener.<designation>` (`fastener.bolt-m12-88`, `fastener.bolt-0375-gr5`, `fastener.anchor-m16-88`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`; the placement of a bolt pattern reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel fastener-layout owner; the fastener reaches `Rasm.Bim` through the `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` (the one Component projection, the owner-mints-its-identity law — Materials owns Types, so the projection mints the deterministic-rooted Type `Object` node stamping the neutral `IfcMechanicalFastener` class + the `FastenerKind.IfcPredefinedType` token, occurrences binding via `Assign.TypeDefinition`) plus the seam-declared NEUTRAL detail bag the `Semantics/connection#CONNECTION_DETAIL` reader round-trips (the `Pset_*`/`Rasm_ConnectionRealization` IFC name a `Rasm.Bim` egress mapping, never authored here), the `IfcMechanicalFastenerTypeEnum` `PredefinedType` resolved + validated at the `Rasm.Bim` `Emit` gate ([C6] — the predefined type is a Bim-owned egress concern), so this owner carries only the portable `FastenerKind.IfcPredefinedType` string the projector lands and the `FastenerType` detail row the reader recovers one-hop, NEVER an interior `IfcOpenShell` evaluation.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude — thread/length/head columns), Dimension (>=1 discrete count — the FastenerAssembly grip-ply/shear-plane carrier) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                  // MaterialId (the seam-carried material identity the Component AppearanceId/SubstanceId reference)
using Rasm.Materials.Component;      // ComponentFamily/ComponentId/Component/ComponentSection/ComponentFault/Coring/ComponentStandard/ComponentAuthority (the parent COMPONENT_OWNER)
using Thinktecture;
using static LanguageExt.Prelude;

// Each Component family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the nine sibling
// `ComponentCatalogue` static classes are distinct types (a shared Rasm.Materials.Component collides with
// component#COMPONENT_OWNER's own `ComponentCatalogue`); component#COMPONENT_OWNER stays the parent Rasm.Materials.Component
// and folds Fastener.ComponentCatalogue.BuildFastenerRows by the sub-namespace-qualified name; parent types via the using above.
namespace Rasm.Materials.Component.Fastener;

// --- [TYPES] -------------------------------------------------------------------------------
// FastenerKind is the member-type axis grown by data, NOT a per-kind class. IfcPredefinedType is the VERIFIED
// GeometryGym.Ifc.IfcMechanicalFastenerTypeEnum member spelling the Projection/component#COMPONENT_PROJECTOR ComponentProjector
// lands on the seam Type Object node's PredefinedType and in the neutral detail bag's FastenerType row (the Rasm.Bim
// Semantics/connection#CONNECTION_DETAIL reader recovers it one-hop). BOLT / SCREW / ANCHORBOLT / DOWEL / RIVET / COUPLER are
// real members (api-geometrygym-ifc.md: COUPLER is the IFC4X2 mechanical-splice member — a bar coupler maps to it directly);
// the enum has NO `NUT` member, so a nut rides USERDEFINED as an IfcDiscreteAccessory companion on the bolt with a NUT project
// property (the [IFC_FASTENER_WIRE] law). The token is the egress HINT the Rasm.Bim Emit gate resolves + validates against the
// frozen IfcClass valid-set ([C6]), never the authority — this owner carries only the portable string.
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
    public bool Threaded { get; }   // a dowel/rivet has no thread; ThreadLengthMm resolves to 0 and the body is all shank
    public bool Headed { get; }     // a bolt/screw/anchor/rivet carries a head; a nut/coupler/dowel does not
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerStandard {
    public static readonly FastenerStandard Iso898    = new("iso-898-1",  metric: true,  authority: "ISO 898-1");
    public static readonly FastenerStandard SaeJ429   = new("sae-j429",   metric: false, authority: "SAE J429");
    public static readonly FastenerStandard AstmF3125 = new("astm-f3125", metric: false, authority: "ASTM F3125/F3125M");
    public bool Metric { get; }
    public string Authority { get; }
}

// FastenerGrade carries the proof/tensile/yield band PLUS the EN 1993-1-8 Table 3.4 THREADED-plane shear-strength factor
// αv (0.6 for the 4.6/5.6/8.8 classes, 0.5 for the higher-strength 4.8/5.8/6.8/10.9 classes whose threaded shear plane
// reduces) — the threads-in-shear-plane coefficient; the unthreaded-shank-plane αv is 0.6 for EVERY grade (the reduction
// does NOT apply to the shank), applied directly in FastenerSection.ShearCapacityKn — and the Preloadable flag (only 8.8 /
// 10.9 / A325 / A490 admit a slip-critical joint, EN 1993-1-8 §3.9 / RCSC) the FastenerAssembly slip-resistance and the
// ISO 7090 300 HV washer selection read. The band is the spec-nominal grade the schedule reads; the measured base-metal
// strength is the properties#MATERIAL_PROPERTY_CATALOGUE Mechanical receipt read by SubstanceId.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerGrade {
    public static readonly FastenerGrade Cls46  = new("4.6",  proofStressMpa: 225.0,  tensileStrengthMpa: 400.0,  minimumYieldMpa: 240.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-4_6",  appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls48  = new("4.8",  proofStressMpa: 310.0,  tensileStrengthMpa: 420.0,  minimumYieldMpa: 340.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-4_8",  appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls56  = new("5.6",  proofStressMpa: 280.0,  tensileStrengthMpa: 500.0,  minimumYieldMpa: 300.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-5_6",  appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls58  = new("5.8",  proofStressMpa: 380.0,  tensileStrengthMpa: 500.0,  minimumYieldMpa: 400.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-5_8",  appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls68  = new("6.8",  proofStressMpa: 440.0,  tensileStrengthMpa: 600.0,  minimumYieldMpa: 480.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-6_8",  appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls88  = new("8.8",  proofStressMpa: 600.0,  tensileStrengthMpa: 800.0,  minimumYieldMpa: 640.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-8_8",  appearanceId: "metal.steel", preloadable: true);
    public static readonly FastenerGrade Cls109 = new("10.9", proofStressMpa: 830.0,  tensileStrengthMpa: 1040.0, minimumYieldMpa: 940.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-10_9", appearanceId: "metal.steel", preloadable: true);
    public static readonly FastenerGrade Cls129 = new("12.9", proofStressMpa: 970.0,  tensileStrengthMpa: 1220.0, minimumYieldMpa: 1100.0, shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    substanceId: "steel.fastener-12_9", appearanceId: "metal.steel", preloadable: false);
    public static readonly FastenerGrade Gr2    = new("gr2",  proofStressMpa: 379.0,  tensileStrengthMpa: 510.0,  minimumYieldMpa: 393.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.SaeJ429,   substanceId: "steel.fastener-gr2",  appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Gr5    = new("gr5",  proofStressMpa: 585.0,  tensileStrengthMpa: 827.0,  minimumYieldMpa: 634.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.SaeJ429,   substanceId: "steel.fastener-gr5",  appearanceId: "metal.steel", preloadable: false);
    public static readonly FastenerGrade Gr8    = new("gr8",  proofStressMpa: 830.0,  tensileStrengthMpa: 1034.0, minimumYieldMpa: 896.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.SaeJ429,   substanceId: "steel.fastener-gr8",  appearanceId: "metal.steel", preloadable: false);
    public static readonly FastenerGrade A325   = new("a325", proofStressMpa: 585.0,  tensileStrengthMpa: 827.0,  minimumYieldMpa: 634.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.AstmF3125, substanceId: "steel.fastener-a325", appearanceId: "metal.steel", preloadable: true);
    public static readonly FastenerGrade A490   = new("a490", proofStressMpa: 830.0,  tensileStrengthMpa: 1040.0, minimumYieldMpa: 940.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.AstmF3125, substanceId: "steel.fastener-a490", appearanceId: "metal.steel", preloadable: true);
    public double ProofStressMpa { get; }
    public double TensileStrengthMpa { get; }
    public double MinimumYieldMpa { get; }
    public double ShearStrengthFactor { get; }   // EN 1993-1-8 Table 3.4 αv — the threaded-plane bolt-tensile-to-shear coefficient
    public FastenerStandard Standard { get; }
    public string SubstanceId { get; }
    public MaterialId Substance => MaterialId.Of(SubstanceId);
    public string AppearanceId { get; }
    public bool Preloadable { get; }              // only these classes admit a slip-critical (category B/C/E) joint + an ISO 7090 300 HV washer
    public bool Metric => Standard.Metric;
}

// BoltCategory is the EN 1993-1-8 Table 3.2 joint category — the bearing-vs-preloaded axis that selects WHICH design
// resistance governs and WHETHER a preload + faying-surface slip coefficient is required. It cites En1993Part.Part1_8
// (the joints code, VividOrange.Standards) rather than inlining the clause numbers. The Preloaded flag gates the
// FastenerAssembly slip-resistance projection; a non-preloadable grade in a B/C/E joint rails ComponentFault.Grade.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltCategory {
    public static readonly BoltCategory A = new("A", shear: true,  preloaded: false, clause: "EN 1993-1-8 Cat A bearing");
    public static readonly BoltCategory B = new("B", shear: true,  preloaded: true,  clause: "EN 1993-1-8 Cat B slip-resistant SLS");
    public static readonly BoltCategory C = new("C", shear: true,  preloaded: true,  clause: "EN 1993-1-8 Cat C slip-resistant ULS");
    public static readonly BoltCategory D = new("D", shear: false, preloaded: false, clause: "EN 1993-1-8 Cat D tension non-preloaded");
    public static readonly BoltCategory E = new("E", shear: false, preloaded: true,  clause: "EN 1993-1-8 Cat E tension preloaded");
    public bool Shear { get; }       // a shear category reads ShearCapacityKn/BearingCapacityKn; a tension category TensileLoadKn
    public bool Preloaded { get; }   // a preloaded category requires a Preloadable grade + a FayingSurface slip class
    public string Clause { get; }
}

// FayingSurface is the EN 1993-1-8 §3.9 / RCSC Table 3.7 slip-factor class μ of the contact surfaces a preloaded
// (category B/C/E) joint relies on; a non-preloaded joint carries the inert None row (μ = 0, slip not relied upon).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FayingSurface {
    public static readonly FayingSurface None    = new("none",     slipFactor: 0.00);
    public static readonly FayingSurface ClassA  = new("class-a",  slipFactor: 0.50);   // blasted, loose rust removed
    public static readonly FayingSurface ClassB  = new("class-b",  slipFactor: 0.40);   // blasted + alkali-zinc-silicate coat
    public static readonly FayingSurface ClassC  = new("class-c",  slipFactor: 0.30);   // wire-brushed / hot-dip galvanized + roughened
    public static readonly FayingSurface ClassD  = new("class-d",  slipFactor: 0.20);   // untreated
    public double SlipFactor { get; }
}

// ThreadSeries is the coarse-vs-fine thread pitch family; the catalogue M/UNC rows are coarse, a fine UNF/UNEF or
// metric-fine row a future data addition keying its own pitch/minor diameter, never a parallel size axis.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSeries {
    public static readonly ThreadSeries MetricCoarse  = new("metric-coarse", metric: true);
    public static readonly ThreadSeries MetricFine    = new("metric-fine",   metric: true);
    public static readonly ThreadSeries UnifiedCoarse = new("unc",           metric: false);
    public static readonly ThreadSeries UnifiedFine   = new("unf",           metric: false);
    public bool Metric { get; }
}

// HexHardware is the per-nominal-size standardized hex hardware envelope — the ISO 4014 hex head (k height / dw washer-face
// bearing diameter / da fillet transition), the ISO 4032 style-1 hex nut height m (its across-flats is the thread's s), and
// the ISO 7089 plain washer (d1 inner / d2 outer / t thickness, the 200 vs 300 HV hardness a grade-driven FastenerAssembly
// projection over the ISO 7089 / ISO 7090 split). ONE flat value-object the ThreadSize carries (Some for the ISO metric
// coarse sizes, None for the inch UNC sizes whose ASME B18.2 envelope is a future data addition); the across-corners e is
// the derived hex geometry (e = s·2/√3) on ThreadSize, never a stored tolerance-min column.
public readonly record struct HexHardware(
    double HeadHeightMm,        // ISO 4014 k (nominal head height)
    double BearingDiameterMm,   // ISO 4014 dw (washer-face / bearing circle, product class B)
    double FilletDiameterMm,    // ISO 4014 da (max transition diameter under the head)
    double NutHeightMm,         // ISO 4032 m (style-1 hex nut height, max)
    double WasherInnerMm,       // ISO 7089 d1 (washer hole diameter)
    double WasherOuterMm,       // ISO 7089 d2 (washer outer diameter)
    double WasherThicknessMm);  // ISO 7089 h (washer thickness, nominal)

// ThreadSize is the nominal-thread axis carrying the ISO 261/724 basic profile AND the ISO 4014/4032/7089 HexHardware
// envelope. MajorDiameterMm (d, also the shank diameter ds), PitchMm (P), and the rounded-root MinorDiameterMm (d3 =
// d − 1.226869·P, the ISO 898-1 stress-area minor) are stored; the pitch diameter d2, the nut basic minor D1, the
// fundamental-triangle height H, and the tensile stress area are derived. The metric stress area is the ISO 898-1
// As = (π/4)·((d2+d3)/2)²; the inch stress area is the ASME B1.1 Unified As = 0.7854·(d − 0.9743·P)² (the metric mean-of-
// diameters formula over-predicts an inch thread by ~3%). The FlankAngleDeg + tolerance classes are the ISO 68-1/965 form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSize {
    public static readonly ThreadSize M6   = new("m6",     series: ThreadSeries.MetricCoarse,  majorDiameterMm: 6.000,  pitchMm: 1.000,  minorDiameterMm: 4.773,  acrossFlatsMm: 10.000, hardware: Some(new HexHardware(4.0,  8.74,  6.8,  5.2,  6.4,  12.0, 1.6)));
    public static readonly ThreadSize M8   = new("m8",     series: ThreadSeries.MetricCoarse,  majorDiameterMm: 8.000,  pitchMm: 1.250,  minorDiameterMm: 6.466,  acrossFlatsMm: 13.000, hardware: Some(new HexHardware(5.3,  11.47, 9.2,  6.8,  8.4,  16.0, 1.6)));
    public static readonly ThreadSize M10  = new("m10",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 10.000, pitchMm: 1.500,  minorDiameterMm: 8.160,  acrossFlatsMm: 16.000, hardware: Some(new HexHardware(6.4,  14.47, 11.2, 8.4,  10.5, 20.0, 2.0)));
    public static readonly ThreadSize M12  = new("m12",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 12.000, pitchMm: 1.750,  minorDiameterMm: 9.853,  acrossFlatsMm: 18.000, hardware: Some(new HexHardware(7.5,  16.47, 13.7, 10.8, 13.0, 24.0, 2.5)));
    public static readonly ThreadSize M16  = new("m16",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 16.000, pitchMm: 2.000,  minorDiameterMm: 13.546, acrossFlatsMm: 24.000, hardware: Some(new HexHardware(10.0, 22.00, 17.7, 14.8, 17.0, 30.0, 3.0)));
    public static readonly ThreadSize M20  = new("m20",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 20.000, pitchMm: 2.500,  minorDiameterMm: 16.933, acrossFlatsMm: 30.000, hardware: Some(new HexHardware(12.5, 27.70, 22.4, 18.0, 21.0, 37.0, 3.0)));
    public static readonly ThreadSize M24  = new("m24",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 24.000, pitchMm: 3.000,  minorDiameterMm: 20.319, acrossFlatsMm: 36.000, hardware: Some(new HexHardware(15.0, 33.25, 26.4, 21.5, 25.0, 44.0, 4.0)));
    public static readonly ThreadSize M30  = new("m30",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 30.000, pitchMm: 3.500,  minorDiameterMm: 25.706, acrossFlatsMm: 46.000, hardware: Some(new HexHardware(18.7, 42.75, 33.4, 25.6, 31.0, 56.0, 4.0)));
    public static readonly ThreadSize M36  = new("m36",    series: ThreadSeries.MetricCoarse,  majorDiameterMm: 36.000, pitchMm: 4.000,  minorDiameterMm: 31.093, acrossFlatsMm: 55.000, hardware: Some(new HexHardware(22.5, 51.11, 39.4, 31.0, 37.0, 66.0, 5.0)));
    public static readonly ThreadSize In025 = new("1/4",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 6.350,  pitchMm: 1.2700, minorDiameterMm: 4.976,  acrossFlatsMm: 11.113, hardware: None);
    public static readonly ThreadSize In037 = new("3/8",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 9.525,  pitchMm: 1.5875, minorDiameterMm: 7.722,  acrossFlatsMm: 14.288, hardware: None);
    public static readonly ThreadSize In050 = new("1/2",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 12.700, pitchMm: 1.9538, minorDiameterMm: 10.450, acrossFlatsMm: 19.050, hardware: None);
    public static readonly ThreadSize In062 = new("5/8",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 15.875, pitchMm: 2.3091, minorDiameterMm: 13.157, acrossFlatsMm: 23.813, hardware: None);
    public static readonly ThreadSize In075 = new("3/4",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 19.050, pitchMm: 2.5400, minorDiameterMm: 16.090, acrossFlatsMm: 28.575, hardware: None);
    public static readonly ThreadSize In087 = new("7/8",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 22.225, pitchMm: 2.8222, minorDiameterMm: 18.928, acrossFlatsMm: 33.338, hardware: None);
    public static readonly ThreadSize In100 = new("1",     series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 25.400, pitchMm: 3.1750, minorDiameterMm: 21.557, acrossFlatsMm: 38.100, hardware: None);
    public static readonly ThreadSize In150 = new("1-1/2", series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 38.100, pitchMm: 4.2333, minorDiameterMm: 32.539, acrossFlatsMm: 57.150, hardware: None);
    public ThreadSeries Series { get; }
    public double MajorDiameterMm { get; }
    public double PitchMm { get; }
    public double MinorDiameterMm { get; }            // d3 — the rounded-root minor diameter the ISO 898-1 stress area reads
    public double AcrossFlatsMm { get; }              // s — the wrench size the hex head AND the hex nut share
    public Option<HexHardware> Hardware { get; }      // Some for ISO metric coarse, None for inch UNC (ASME B18.2 envelope deferred)
    public const double FlankAngleDeg = 60.0;         // ISO 68-1 / ASME B1.1 basic-profile included thread angle
    public const string ExternalToleranceClass = "6g";   // ISO 965 medium external (bolt) thread fit
    public const string InternalToleranceClass = "6H";   // ISO 965 medium internal (nut) thread fit
    public double PitchDiameterMm => MajorDiameterMm - 0.649519 * PitchMm;       // d2 (ISO 724 basic pitch diameter)
    public double NutMinorDiameterMm => MajorDiameterMm - 1.082532 * PitchMm;    // D1 (internal-thread basic minor diameter)
    public double FundamentalHeightMm => 0.866025 * PitchMm;                     // H (ISO 68-1 fundamental-triangle height)
    public double AcrossCornersMm => AcrossFlatsMm * 2.0 / 1.7320508;            // e = s·2/√3 (regular-hex across-corners)
    public double NominalAreaMm2 => Math.PI / 4.0 * MajorDiameterMm * MajorDiameterMm;   // gross shank area (shank-in-shear plane)
    public double TensileStressAreaMm2 => Series.Metric
        ? Math.PI / 16.0 * Math.Pow(PitchDiameterMm + MinorDiameterMm, 2.0)      // ISO 898-1 As = (π/4)·((d2+d3)/2)²
        : 0.7854 * Math.Pow(MajorDiameterMm - 0.9743 * PitchMm, 2.0);           // ASME B1.1 Unified As = 0.7854·(d − 0.9743·P)² (mm², d/P in mm)
}

// --- [MODELS] ------------------------------------------------------------------------------
// FastenerSection is the thread-and-capacity receipt PLUS the per-instance shank-vs-thread split — the AISC 360 J3 /
// EN 1993-1-8 Table 3.4 shear/tension/bearing resistances over the stored thread columns and the grade band (the derived
// `double` projections the Rasm.Compute design-code checks read off the seam SectionProperties consumer contract and the
// IfcMechanicalFastener wire reads), and the ISO 4014 thread-length b + unthreaded-shank L−b a host materializes the bolt
// solid from. LengthMm is the OVERALL bolt length L (NOT a shank length — the b/shank split below derives b + the shank); the head/nut/washer
// geometry rides Size.Hardware, the thread form Size's derivations.
public readonly record struct FastenerSection(
    FastenerKind Kind,
    FastenerGrade Grade,
    ThreadSize Size,
    BoltCategory Category,
    PositiveMagnitude ThreadDiameterMm,
    PositiveMagnitude PitchMm,
    PositiveMagnitude LengthMm,
    PositiveMagnitude AcrossFlatsMm) {

    // The FINISH the appearance projection reads (metal.iron for low-carbon cls 4.6/4.8/5.6/SAE Gr2, metal.steel for alloy
    // cls 8.8/10.9/12.9 and A325/A490), INDEPENDENT from SubstanceId: a galvanized bolt keeps the steel SubstanceId while
    // its AppearanceId stays the base-metal appearance row unless a finish system overrides it above the component.
    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);

    // The substance whose properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row the design seam reads — the bolt's
    // grade-specific structural steel, sourced independently from the AppearanceId finish.
    public MaterialId SubstanceId => Grade.Substance;

    // The generative head/nut/washer envelope (Some for ISO metric coarse, None for inch UNC) the host reads the hex head
    // height + washer-face + nut + washer solids from; the thread form rides Size's derivations, the per-instance length
    // split the projections below.
    public Option<HexHardware> Hardware => Size.Hardware;

    public double TensileStressAreaMm2 => Size.TensileStressAreaMm2;
    public double ProofLoadKn => Grade.ProofStressMpa * TensileStressAreaMm2 * 1e-3;
    public double TensileLoadKn => Grade.TensileStrengthMpa * TensileStressAreaMm2 * 1e-3;

    // The ISO 4014 reference thread length b and the unthreaded grip shank L−b a host materializes the bolt solid from. A
    // threaded headed fastener (bolt/screw/anchor) threads b = 2d+6 (L ≤ 125), 2d+12 (125 < L ≤ 200), 2d+25 (L > 200) from
    // the tip, clamped to L for a short fully-threaded bolt; a threaded headless part (nut/coupler) is threaded through its
    // whole length (b = L); an unthreaded dowel/rivet is all shank (b = 0). The run-out is the ISO 3508 incomplete-thread
    // allowance x ≤ 2.5·P at the shank/thread boundary; ds (the unthreaded shank diameter) is the nominal major diameter.
    public double ThreadLengthMm =>
        !Kind.Threaded ? 0.0
        : !Kind.Headed ? LengthMm.Value
        : Math.Min(LengthMm.Value, LengthMm.Value <= 125.0 ? 2.0 * ThreadDiameterMm.Value + 6.0
            : LengthMm.Value <= 200.0 ? 2.0 * ThreadDiameterMm.Value + 12.0 : 2.0 * ThreadDiameterMm.Value + 25.0);
    public double UnthreadedShankMm => LengthMm.Value - ThreadLengthMm;
    public double ThreadRunoutMm => 2.5 * PitchMm.Value;
    public double ShankDiameterMm => ThreadDiameterMm.Value;

    // EN 1993-1-8 Table 3.4 / AISC 360 J3.6 single-bolt shear: Fv = αv · fub · A, with A = the tensile stress area when the
    // shear plane passes through the THREAD and the gross shank area when it passes through the unthreaded SHANK. The αv
    // coefficient is plane-DEPENDENT: the stored Grade.ShearStrengthFactor (0.5 for the reducing 4.8/5.8/6.8/10.9 classes,
    // 0.6 otherwise) is the THREADED-plane factor; the unthreaded-shank plane is αv = 0.6 for EVERY grade (the EN Table 3.4
    // reduction does NOT apply to the shank — applying the stored 0.5 to the shank case is the deleted form, under-predicting
    // a 10.9 shank-shear bolt by ~17%). The 0.6 here is the EN shank-plane αv, not a base-metal read.
    public double ShearCapacityKn(bool threadsInShearPlane) =>
        (threadsInShearPlane ? Grade.ShearStrengthFactor : 0.60) * Grade.TensileStrengthMpa
            * (threadsInShearPlane ? TensileStressAreaMm2 : Size.NominalAreaMm2) * 1e-3;

    // EN 1993-1-8 Table 3.4 / AISC 360 J3.10 single-bolt bearing on the connected ply: Fb = k1·αb · fu,ply · d · t, the
    // edge/end/pitch geometry collapsed into one caller-supplied edgeFactor (k1·αb, capped at the spec ceiling by the caller)
    // and the ply ultimate strength supplied by the properties#MATERIAL_PROPERTY_CATALOGUE Mechanical receipt the Rasm.Compute
    // design check reads — never a base-metal strength re-keyed here.
    public double BearingCapacityKn(double plyThicknessMm, double plyUltimateMpa, double edgeFactor) =>
        edgeFactor * plyUltimateMpa * ThreadDiameterMm.Value * plyThicknessMm * 1e-3;
}

// FastenerRow is the flat catalogue seed; Category defaults the schedule to bearing-type A unless a structural-bolt row
// names a preloaded category (the slip-critical A325/A490 rows), the faying class None for a bearing joint. LengthMm is the
// overall length L (the bolt/screw/anchor/dowel/coupler length; for a nut row, the ISO 4032 nut height m).
public readonly record struct FastenerRow(string Designation, string Kind, string Size, string Grade, string Category, string Faying, double LengthMm);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ComponentCatalogue {
    // The non-regional fastener standard the registered rows cite (ISO 898-1 / SAE J429 / ASTM F3125 are bolt-property
    // specifications, not a masonry-style regional joint thickness) — StandardJointThicknessMm 0.0 (a fastener lays no
    // mortar joint), ComponentAuthority.Astm the F3125 structural-bolt authority, mirroring steel.md's Aisc/En statics.
    static readonly ComponentStandard Standard = new("iso", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.Astm);

    static readonly Seq<FastenerRow> FastenerRows = Seq(
        new FastenerRow("fastener.bolt-m12-88",     "bolt",    "m12", "8.8",  "A", "none",    60.0),
        new FastenerRow("fastener.bolt-m16-88",     "bolt",    "m16", "8.8",  "A", "none",    80.0),
        new FastenerRow("fastener.bolt-m16-109",    "bolt",    "m16", "10.9", "C", "class-a", 80.0),
        new FastenerRow("fastener.bolt-m20-88",     "bolt",    "m20", "8.8",  "A", "none",    90.0),
        new FastenerRow("fastener.bolt-m20-109",    "bolt",    "m20", "10.9", "C", "class-a", 90.0),
        new FastenerRow("fastener.bolt-m24-109",    "bolt",    "m24", "10.9", "C", "class-a", 110.0),
        new FastenerRow("fastener.bolt-m30-129",    "bolt",    "m30", "12.9", "A", "none",    140.0),
        new FastenerRow("fastener.bolt-0375-gr5",   "bolt",    "3/8", "gr5",  "A", "none",    63.5),
        new FastenerRow("fastener.bolt-0500-gr5",   "bolt",    "1/2", "gr5",  "A", "none",    76.2),
        new FastenerRow("fastener.bolt-0750-gr8",   "bolt",    "3/4", "gr8",  "A", "none",    101.6),
        new FastenerRow("fastener.bolt-0875-a325",  "bolt",    "7/8", "a325", "C", "class-a", 114.3),
        new FastenerRow("fastener.bolt-0875-a490",  "bolt",    "7/8", "a490", "C", "class-b", 114.3),
        new FastenerRow("fastener.nut-m16-88",      "nut",     "m16", "8.8",  "A", "none",    14.8),
        new FastenerRow("fastener.nut-m20-109",     "nut",     "m20", "10.9", "A", "none",    18.0),
        new FastenerRow("fastener.screw-m8-88",     "screw",   "m8",  "8.8",  "A", "none",    40.0),
        new FastenerRow("fastener.screw-0250-gr2",  "screw",   "1/4", "gr2",  "A", "none",    31.8),
        new FastenerRow("fastener.dowel-m20-88",    "dowel",   "m20", "8.8",  "A", "none",    100.0),
        new FastenerRow("fastener.coupler-m20-88",  "coupler", "m20", "8.8",  "A", "none",    60.0),
        new FastenerRow("fastener.anchor-m16-88",   "anchor",  "m16", "8.8",  "D", "none",    200.0),
        new FastenerRow("fastener.anchor-m20-88",   "anchor",  "m20", "8.8",  "D", "none",    250.0),
        new FastenerRow("fastener.anchor-0750-a325","anchor",  "3/4", "a325", "E", "class-a", 304.8));

    static Fin<(ComponentId Id, Component Item)> FastenerOf(FastenerRow r, Context context, Op key) =>
        from kind in FastenerKind.TryGet(r.Kind, out FastenerKind? k) ? Fin.Succ(k!) : Fin.Fail<FastenerKind>(ComponentFault.Designation(key, $"<unknown-fastener-kind:{r.Kind}>"))
        from size in ThreadSize.TryGet(r.Size, out ThreadSize? t) ? Fin.Succ(t!) : Fin.Fail<ThreadSize>(ComponentFault.Designation(key, $"<unknown-thread-size:{r.Size}>"))
        from grade in FastenerGrade.TryGet(r.Grade, out FastenerGrade? g) ? Fin.Succ(g!) : Fin.Fail<FastenerGrade>(ComponentFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from category in BoltCategory.TryGet(r.Category, out BoltCategory? c) ? Fin.Succ(c!) : Fin.Fail<BoltCategory>(ComponentFault.Grade(key, $"<unknown-category:{r.Category}>"))
        from faying in FayingSurface.TryGet(r.Faying, out FayingSurface? f) ? Fin.Succ(f!) : Fin.Fail<FayingSurface>(ComponentFault.Grade(key, $"<unknown-faying:{r.Faying}>"))
        from preloadValid in guard(!category.Preloaded || grade.Preloadable, ComponentFault.Grade(key, $"<non-preloadable-grade-in-preloaded-joint:{grade.Key}:{category.Key}>"))
        from positiveThread in guard(size.MajorDiameterMm > 0.0, ComponentFault.Dimension(key, $"<degenerate-thread:{r.Size}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.MajorDiameterMm)
        from pitch in key.AcceptValidated<PositiveMagnitude>(candidate: size.PitchMm)
        from length in key.AcceptValidated<PositiveMagnitude>(candidate: r.LengthMm)
        from flats in key.AcceptValidated<PositiveMagnitude>(candidate: size.AcrossFlatsMm)
        let section = new FastenerSection(kind, grade, size, category, diameter, pitch, length, flats)
        from item in Component.Of(ComponentFamily.Fastener, r.Designation, new ComponentSection.Fastener(section), Coring.None, Standard, section.SubstanceId, section.AppearanceId, key)
        select (ComponentId.Of(r.Designation), item);

    // ComponentId's generated [KeyMemberEqualityComparer] ordinal value-equality keys the frozen dictionary, so NO explicit
    // comparer is threaded — ComparerAccessors.StringOrdinal.EqualityComparer is an IEqualityComparer<string>, a type mismatch
    // on a ComponentId key (the component#COMPONENT_OWNER ComponentCatalogue.Build convention the master fold follows).
    public static FrozenDictionary<ComponentId, Component> BuildFastenerRows(Context context) =>
        FastenerRows
            .Choose(row => FastenerOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item);
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` the complete-connection owner over one `FastenerSection` — the bolt PLUS the discrete `Dimension` grip-ply count, the nut+washer presence, and the preloaded design state (the `BoltCategory.Preloaded` + `FayingSurface` slip class); the `PreloadKn` `Fp,C = 0.7·fub·As` projection (EN 1993-1-8 §3.9 / RCSC), the `SlipResistanceKn` `Fs,Rd = ks·n·μ·Fp,C/γM3` projection the category-B/C/E slip-critical seam reads, and the ISO 7089/7090 `WasherHardnessHv` the high-strength joint selects. `FastenerAssembly.Of` is the ONE assembly boundary railing a degenerate grip / a non-preloadable grade in a preloaded joint.
- Cases: a fastener assembly is one `FastenerSection` (the bolt) + a `Dimension` grip-ply count (the connected layers the shank passes — the `Dimension` discrete-count carrier `component#COMPONENT_OWNER` reserves for grip plies) + a `Dimension` shear-plane count + a washer-presence flag + the section's `Category`/`FayingSurface` — no per-assembly subtype, the modality riding the `FastenerSection` it composes; a non-preloaded (category A/D) assembly carries `FayingSurface.None` and reads zero slip resistance, a preloaded (B/C/E) assembly the named slip class and the full slip resistance; the nut height + washer geometry the host reads ride the composed `Section.Hardware` (ISO 4032 m / ISO 7089 d1/d2/t), the washer hardness the grade-driven 200/300 HV split.
- Entry: `public static Fin<FastenerAssembly> Of(FastenerSection section, FayingSurface faying, int gripPlies, int shearPlanes, bool withWasher, Op key)` — the ONE assembly admission: it admits the `gripPlies`/`shearPlanes` discrete counts through `key.AcceptValidated<Dimension>(candidate:)` (a non-positive count rails the kernel `Dimension` admission, not a component fault — a count is a discrete `Dimension`, never a `Capacity`), rails a preloaded `Category` over a non-`Preloadable` grade through `ComponentFault.Grade`, and reads `section.Category.Preloaded ? faying : FayingSurface.None` so a bearing joint never relies on a slip coefficient; `public double PreloadKn` projects `0.7·fub·As`, `public double SlipResistanceKn(double ks, double gammaM3)` the EN 1993-1-8 slip resistance over the admitted shear-plane count and the faying μ, `public double WasherHardnessHv` the ISO 7090 300 HV (preloadable high-strength) vs ISO 7089 200 HV split — one polymorphic boundary, never a `BuildPreloadedAssembly`/`BuildBearingAssembly` family.
- Packages: Rasm (project — `PositiveMagnitude` through the composed `FastenerSection`, `Dimension` for the grip-ply/shear-plane discrete counts), Thinktecture.Runtime.Extensions (the `FastenerGrade`/`BoltCategory`/`FayingSurface` axes the assembly reads through their generated keys), LanguageExt.Core (`Fin`/`Seq`/`Fold`/`Option` for the admission rail and the `Section.Hardware` nut/washer reads), BCL inbox. No new external package — the assembly composes the realized `[02]` vocabulary and the kernel value-objects; the structural design code is CITED (`En1993Part.Part1_8`), the multi-bolt group resistance the `Rasm.Compute` concern over these single-assembly receipts.
- Growth: a new connection modality is a `FastenerSection`/`BoltCategory` row the assembly reads, never a per-assembly type; the slip-resistance arithmetic the `SlipResistanceKn` projects is the single-bolt design value, a global connection-group `ΣFs,Rd` the `Rasm.Compute` design-check concern; a tension+shear interaction (the EN 1993-1-8 `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` combined-action check) is the `Rasm.Compute` consumer reading `ShearCapacityKn`/`TensileLoadKn`/`SlipResistanceKn` off the seam, never re-derived here.
- Boundary: `FastenerAssembly` is the BOLT-CONNECTION assembler — a per-modality `PreloadedBolt`/`BearingBolt` class is the deleted form, the modality riding the composed `FastenerSection.Category`/`FayingSurface`; the grip-ply and shear-plane counts are the `Rasm` kernel `Dimension` discrete-count carrier (`component#COMPONENT_OWNER` reserves `Dimension` for grip plies, never a `PositiveMagnitude` that admits a fractional layer), admitted once through `key.AcceptValidated<Dimension>(candidate:)`; the preload is `Fp,C = 0.7·fub·As` over the `FastenerGrade.TensileStrengthMpa` band and the `ThreadSize.TensileStressAreaMm2`, NOT a host computation; the slip resistance `Fs,Rd = ks·n·μ·Fp,C/γM3` reads the admitted shear-plane count `n` and the `FayingSurface.SlipFactor` μ, the `ks` hole-tolerance factor and the `γM3` partial factor caller-supplied (the design-code parameters the `Rasm.Compute` consumer owns, never inlined here); the washer hardness is the ISO 7089 200 HV / ISO 7090 300 HV split keyed by `FastenerGrade.Preloadable` (a high-strength preloaded bolt seats a 300 HV washer to spread the pretension, a low-strength bolt the 200 HV plain washer), the nut height + washer geometry the host reads off the composed `Section.Hardware` (`None` for an inch UNC bolt whose ASME B18.22 envelope is deferred); a preloaded category over a non-preloadable grade is impossible (railed at `Of`), so a slip-resistance read is always over a valid preloaded bolt; the assembly is host-neutral scalar DATA the `IfcMechanicalFastener` wire reads (the nut a separate `IfcDiscreteAccessory` companion at the `Rasm.Bim` boundary, the washer a project property), never an interior `IfcOpenShell` evaluation; the assembly is NOT a `Component` — a `Component` is one discrete bolt in the schedule, the `FastenerAssembly` the populated connection the bolt completes, the two meeting at the `FastenerSection` vocabulary this page already owns.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The complete bolt-connection receipt: the FastenerSection bolt plus the discrete grip-ply / shear-plane counts (the
// Dimension carrier), the resolved faying-surface slip class (None for a bearing joint), and the washer presence the
// IfcDiscreteAccessory wire reads. The preload/slip-resistance/washer-hardness projections are the EN 1993-1-8 §3.9 single-
// bolt design values + the ISO 7089/7090 hardware the Rasm.Compute slip-critical and combined-action checks and the host
// solid read off the seam, never re-derived per consumer.
public readonly record struct FastenerAssembly(
    FastenerSection Section,
    FayingSurface Faying,
    Dimension GripPlies,
    Dimension ShearPlanes,
    bool WithWasher) {

    public static Fin<FastenerAssembly> Of(FastenerSection section, FayingSurface faying, int gripPlies, int shearPlanes, bool withWasher, Op key) =>
        from preloadValid in guard(!section.Category.Preloaded || section.Grade.Preloadable, ComponentFault.Grade(key, $"<non-preloadable-grade-in-preloaded-joint:{section.Grade.Key}:{section.Category.Key}>"))
        from plies in key.AcceptValidated<Dimension>(candidate: gripPlies)
        from planes in key.AcceptValidated<Dimension>(candidate: shearPlanes)
        let resolvedFaying = section.Category.Preloaded ? faying : FayingSurface.None
        select new FastenerAssembly(section, resolvedFaying, plies, planes, withWasher);

    // EN 1993-1-8 §3.9.1 / RCSC preload: Fp,C = 0.7 · fub · As — the design pretension a preloaded high-strength bolt is
    // installed to (zero for a non-preloadable / bearing-category bolt, so a bearing joint reads no preload).
    public double PreloadKn => Section.Category.Preloaded && Section.Grade.Preloadable
        ? 0.7 * Section.Grade.TensileStrengthMpa * Section.TensileStressAreaMm2 * 1e-3
        : 0.0;

    // EN 1993-1-8 §3.9.1 slip resistance per bolt: Fs,Rd = ks · n · μ · Fp,C / γM3, with n the friction-interface count (the
    // admitted ShearPlanes) and μ the faying-surface slip factor; the ks hole-tolerance factor and the γM3 partial factor are
    // the design-code parameters the Rasm.Compute consumer supplies, never inlined here.
    public double SlipResistanceKn(double ks, double gammaM3) =>
        gammaM3 > 0.0 ? ks * ShearPlanes.Value * Faying.SlipFactor * PreloadKn / gammaM3 : 0.0;

    // ISO 7089 (200 HV plain) vs ISO 7090 (300 HV chamfered, the high-strength preloaded washer) hardness split keyed by the
    // grade's Preloadable flag — a high-strength preloaded bolt seats the 300 HV washer to spread its pretension.
    public double WasherHardnessHv => Section.Grade.Preloadable ? 300.0 : 200.0;

    // The nut height + washer geometry the host reads for the assembly solid — Some for an ISO metric bolt (ISO 4032 m, ISO
    // 7089 d1/d2/t), None for an inch UNC bolt whose ASME B18.2 envelope is deferred; the washer reads gate on WithWasher.
    public Option<double> NutHeightMm => Section.Hardware.Map(static h => h.NutHeightMm);
    public Option<double> WasherOuterMm => WithWasher ? Section.Hardware.Map(static h => h.WasherOuterMm) : None;
    public Option<double> WasherThicknessMm => WithWasher ? Section.Hardware.Map(static h => h.WasherThicknessMm) : None;
}
```

## [04]-[RESEARCH]

- [FASTENER_ROW_TRANSCRIPTION]: REALIZED — the ISO 898-1 metric property-class and SAE J429 / ASTM F3125 inch-grade mechanical-fastener catalogue (the M6..M36 ISO metric coarse and 1/4in..1-1/2in UNC nominal threads, the ISO 898-1 cls 4.6/4.8/5.6/5.8/6.8/8.8/10.9/12.9 proof/tensile/yield bands, the SAE J429 Gr2/Gr5/Gr8 grades, and the ASTM F3125 A325/A490 structural-bolt lineage) seeds through `ComponentCatalogue.BuildFastenerRows(context)` over the `FastenerRow` designation/kind/size/grade/category/faying/length table keyed `fastener.<designation>`, the major-diameter/pitch/length/across-flats columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the kind/grade/size/category algebra realized as the fastener vocabulary; a new fastener is one `FastenerRow` data addition plus, if novel, one `ThreadSize`/`FastenerGrade` row. The major-diameter/pitch/minor-diameter values transcribe the published ISO 261 / ISO 724 metric coarse thread series (`M12` major `12.000 mm`, pitch `1.75 mm`, rounded-root minor `9.853 mm`) and the ASME B1.1 Unified inch coarse series (`3/8-16` major `9.525 mm`, pitch `1.5875 mm`); the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa`/`MinimumYieldMpa` bands transcribe the ISO 898-1 Table property-class minima and the SAE J429 grade minima (cls 8.8 proof `600 MPa` / tensile `800 MPa`, Gr5 proof `85 ksi ≈ 585 MPa` / tensile `120 ksi ≈ 827 MPa`), the `ShearStrengthFactor` αv the EN 1993-1-8 Table 3.4 coefficient (0.6 for the classes whose αv applies to the full bolt, 0.5 for the higher-strength 4.8/5.8/6.8/10.9 classes whose threaded shear plane reduces); a non-positive column rails the `AcceptValidated` `Fin` and a degenerate thread `ComponentFault.Dimension` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`, and a preloaded `BoltCategory` over a non-`Preloadable` grade rails `ComponentFault.Grade` so a category-B/C/E joint never seeds a non-preloadable bolt.
- [THREAD_FORM_AND_STRESS_AREA]: REALIZED — the `ThreadSize` thread form is the ISO 68-1 / ISO 724 basic profile derived once on the size: the pitch diameter `PitchDiameterMm` (`d2 = d − 0.649519·P`), the internal-thread basic minor `NutMinorDiameterMm` (`D1 = d − 1.082532·P`), the fundamental-triangle height `FundamentalHeightMm` (`H = 0.866025·P`), and the stored rounded-root minor `MinorDiameterMm` (`d3 = d − 1.226869·P`) the stress area reads, with the `FlankAngleDeg` const 60° included angle and the ISO 965 `6g` external / `6H` internal tolerance classes. The `TensileStressAreaMm2` projection is standard-CORRECT by series: a metric thread reads the ISO 898-1 `As = (π/4)·((d2+d3)/2)²` (cls 8.8 `M12`: `As ≈ 84.3 mm²`, proof load `≈ 50.6 kN`), an inch thread the ASME B1.1 Unified `As = 0.7854·(d − 0.9743·P)²` (the formula is dimensionless in the unit of `d`/`P`, so the mm-valued columns yield mm² directly; `3/8-16`: `As ≈ 49.99 mm²`) — the metric mean-of-diameters formula over-predicts an inch thread by ~3% and is the deleted form. `FastenerSection.NominalAreaMm2` is the gross shank area `(π/4)·d²` the shank-in-shear-plane case reads. The stress area lives ONCE on `ThreadSize` (a pure thread property) rather than re-computed at each call site, and is NOT re-derived at the `properties#MATERIAL_PROPERTY_CATALOGUE` boundary — the `MaterialId`-keyed `Mechanical` row carries the measured `YieldStrengthMpa`/`YoungsModulusMpa`, the spec-nominal grade band the catalogue proof/tensile, the two meeting at the design seam.
- [FASTENER_GEOMETRY_CAPTURE]: REALIZED — the validated bolt+nut+washer generative geometry a host materializes a real solid from: the per-size `HexHardware` envelope carries the ISO 4014 hex head (`k` head height, `dw` washer-face bearing diameter product-class B, `da` fillet transition), the ISO 4032 style-1 hex nut height `m` (its across-flats the thread's `s`), and the ISO 7089 plain washer (`d1` inner, `d2` outer, `t` thickness) — `Some` for the ISO metric coarse M6..M36 sizes, `None` for the inch UNC sizes; the across-corners `e = s·2/√3` is the derived regular-hex geometry, never a stored tolerance-min column. `FastenerSection` carries the per-instance shank-vs-thread split — `LengthMm` the overall length `L` (the whole bolt length, not a shank length), `ThreadLengthMm` the ISO 4014 reference thread length `b = 2d+6` (`L ≤ 125`) / `2d+12` (`125 < L ≤ 200`) / `2d+25` (`L > 200`) clamped to `L`, `UnthreadedShankMm = L − b`, `ThreadRunoutMm` the ISO 3508 incomplete-thread allowance `x ≤ 2.5·P`, `ShankDiameterMm` the nominal `ds = d` — discriminated by `FastenerKind.Threaded`/`Headed` so a nut/coupler threads through its whole length (`b = L`), a dowel/rivet is all shank (`b = 0`), a bolt/screw/anchor the partial-thread split. The washer hardness is the `FastenerAssembly.WasherHardnessHv` ISO 7089 200 HV / ISO 7090 300 HV grade-driven split.
- [BOLT_CAPACITY_PROJECTIONS]: REALIZED — the `FastenerSection` capacity family is the host-neutral single-bolt design-value set the `Rasm.Compute` design-code checks read off the seam (the CONSUMER contract the seam `SectionProperties` names): `ShearCapacityKn(threadsInShearPlane)` is the EN 1993-1-8 Table 3.4 / AISC 360 J3.6 `Fv = αv·fub·A` with `A` the tensile stress area (thread in plane) or the gross shank area (shank in plane) AND the plane-dependent αv — the stored `Grade.ShearStrengthFactor` (the reduced 0.5 for the 4.8/5.8/6.8/10.9 classes) only for the threaded plane, the un-reduced 0.6 for the unthreaded-shank plane for every grade (the EN Table 3.4 reduction not applying to the shank); `BearingCapacityKn(plyThicknessMm, plyUltimateMpa, edgeFactor)` the EN 1993-1-8 / AISC 360 J3.10 `Fb = k1·αb·fu,ply·d·t` over the connected-ply ultimate strength the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt supplies (never re-keyed here) and the caller-supplied edge/end/pitch factor; `ProofLoadKn`/`TensileLoadKn` the single-bolt axial values. The single-bolt resistances are the receipts; the multi-bolt group resistance (the `ΣFv,Rd` reduction, the long-joint β factor, the combined shear+tension interaction) is the `Rasm.Compute` design-check concern over these receipts, never re-derived here. Ripple counterpart: `Rasm.Compute` (the connection-group resistance + combined-action interaction over these single-bolt/assembly receipts).
- [BOLT_ASSEMBLY_COMPOSITION]: REALIZED — the `[03]-[BOLT_ASSEMBLY]` `FastenerAssembly` owner is the host-neutral complete-connection assembler: `FastenerAssembly.Of` admits the discrete grip-ply and shear-plane counts through the `Rasm` kernel `Dimension` carrier (never a fractional `PositiveMagnitude`), rails a preloaded `BoltCategory` over a non-`Preloadable` `FastenerGrade` through `ComponentFault.Grade`, and resolves the `FayingSurface` to `None` for a bearing joint so a non-preloaded connection never relies on a slip coefficient. `PreloadKn` projects the EN 1993-1-8 §3.9 `Fp,C = 0.7·fub·As` design pretension, `SlipResistanceKn(ks, γM3)` the per-bolt slip resistance `Fs,Rd = ks·n·μ·Fp,C/γM3` over the admitted friction-interface count and the faying μ — the slip-critical (category B/C/E) design value the `Rasm.Compute` connection check reads off the seam — and `WasherHardnessHv` the ISO 7089/7090 200/300 HV grade-driven washer selection; the nut height + washer geometry the host reads ride the composed `Section.Hardware`. The assembly is NOT a `Component` (a discrete schedule bolt) — it is the populated connection the bolt completes, the two meeting at the `FastenerSection` vocabulary. Ripple counterpart: `Rasm.Compute` (the multi-bolt connection-group resistance + combined shear+tension interaction over these single-bolt/assembly receipts).
- [IFC_FASTENER_WIRE]: REALIZED — a mechanical fastener round-trips to IFC 4.3 as an `IfcMechanicalFastener` carrying `NominalDiameter` (the `FastenerSection.ThreadDiameterMm`) and `NominalLength` (the `LengthMm`), GeometryGym-internal scalars the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader recovers through the associated `IfcMaterialProfileSetUsage` → `IfcCircleProfileDef.Radius` cross-section. The VERIFIED `GeometryGym.Ifc.IfcMechanicalFastenerTypeEnum` members are {NOTDEFINED, USERDEFINED, ANCHORBOLT, BOLT, DOWEL, NAIL, NAILPLATE, RIVET, SCREW, SHEARCONNECTOR, STAPLE, STUDSHEARCONNECTOR, COUPLER (IFC4X2), RAILJOINT / RAILFASTENING / CHAIN / ROPE (IFC4X3)} per `api-geometrygym-ifc.md` — so `FastenerKind.IfcPredefinedType` maps `bolt→BOLT`, `screw→SCREW`, `anchor→ANCHORBOLT`, `dowel→DOWEL`, `rivet→RIVET`, and `coupler→COUPLER` (the IFC4X2 mechanical-splice member — a bar coupler maps to it directly), and ONLY `nut→USERDEFINED` (the enum has NO `NUT` member, so a nut rides USERDEFINED as an `IfcDiscreteAccessory` companion on the bolt with a `NUT` project property). The `IfcMechanicalFastenerTypeEnum` `PredefinedType` is resolved + validated at the `Rasm.Bim` `Emit` gate ([C6] — the predefined type is a Bim-owned egress concern, not authored here), this owner carrying only the portable `FastenerKind.IfcPredefinedType` string the `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lands on the seam Type `Object` node's `PredefinedType` and in the neutral detail bag's `FastenerType` row (the `Semantics/connection#CONNECTION_DETAIL` reader recovers it one-hop). A non-mechanical or proprietary fastener (an adhesive/mortar/weld realizing element) crosses as an `IfcFastener` (`IfcFastenerTypeEnum` ∈ {NOTDEFINED, USERDEFINED, GLUE, MORTAR, WELD}) over the `joint#JOINT_FAMILY` vocabulary; the structural property set (the proof/tensile/shear/preload the `ThreadSize.TensileStressAreaMm2`×`FastenerGrade` projections and the `FastenerAssembly.PreloadKn` assert) is the `MaterialId`-keyed `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` read at the federation. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the scalar columns + the portable token the wire reads, never an IFC entity.
- [INCH_HARDWARE_ENVELOPE]: RESEARCH — the inch UNC `ThreadSize.Hardware` `Option<HexHardware>` `None` arm carries no head/nut/washer envelope today; the ASME B18.2.1 (hex bolt head height + bearing), ASME B18.2.2 (hex nut height), and ASME B18.22.1 / SAE flat-washer rows seed the inch `HexHardware` when an inch-detailed generative target lands, the across-flats `s` already carried (the deterministic fractional-inch wrench sizes). The `Option<HexHardware>` `None`-for-inch shape mirrors the `reinforcement#REINFORCEMENT_FAMILY` `BarSize.Catalogue` `Option<BarDiameter>` `None`-for-ASTM pattern, so an inch bolt's thread form + capacity + As are complete while its hex hardware solid waits on the ASME envelope rows.
