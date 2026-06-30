# [MATERIALS_FASTENER]

THE FASTENER CONNECTIONFAMILY and THE BOLT-CAPACITY OWNER. The fastener vocabulary — the `FastenerKind` member-type discriminant (bolt · nut · screw · anchor · dowel · rivet · coupler), the `FastenerGrade` proof/tensile/shear-factor axis (ISO 898-1 property classes, SAE J429 grades, ASTM F3125 structural-bolt grades), the `ThreadSize` nominal-thread axis carrying the ISO 898-1 tensile stress area, the `BoltCategory` bearing-vs-preloaded axis (EN 1993-1-8 categories A/B/C/D/E), the `FayingSurface` slip-coefficient class, and the `FastenerSection` thread receipt — is the realized mechanical-fastener vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Fastener` case. A 3/8in hex bolt is a `ConnectionItem` row, never a `Bolt` type: the kind, the grade, the thread receipt, and the category are fastener-`ConnectionItem` columns, and the `FastenerSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the reinforcement (`Connection/reinforcement#REINFORCEMENT_FAMILY`) and hanger (`Connection/hanger#HANGER_FAMILY`) families drive — a bolt pattern places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. This owner is ALSO the host-neutral BOLT-CAPACITY assembler: the `FastenerSection` capacity family projects the bolt shear/tension/bearing design values (the AISC 360 J3 / EN 1993-1-8 Table 3.4 single-bolt resistances) and the `FastenerAssembly` owner composes the bolt + grip-plies + nut + washer + preload of a complete connection (`Fp,C = 0.7·fub·As`), mirroring how `reinforcement#RC_SECTION` is the second owner block on the reinforcement page. The `anchor` member type is a `FastenerKind` arm folded INSIDE this vocabulary, NEVER a separate `ConnectionFamily` sibling: the `ConnectionFamily` axis is CLOSED at four (reinforcement/fastener/hanger/joint), so a cast-in anchor bolt and a post-installed expansion anchor are `FastenerKind.Anchor` rows that key and serialize the same way a structural bolt does, and an unthreaded shear `dowel`, a `rivet`, and a bar `coupler` are the same one `FastenerKind` axis grown by data, never per-kind types. The fastener vocabulary grows by data — a new property class is one `FastenerGrade` row, a new thread size one `ThreadSize` row, a new designation one `FastenerRow` catalogue entry — never a per-bolt type. The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every thread/length/head column and `Dimension` for the discrete grip-ply count, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel`/`metal.iron`) appearance column each row carries (the `Profile.AppearanceId` mirror), and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here — the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa` axis is the spec-nominal grade band, the measured base-metal capacity the property-library receipt; the reinforcement, hanger, and joint families land their own sibling vocabularies on `Connection/reinforcement#REINFORCEMENT_FAMILY`, `Connection/hanger#HANGER_FAMILY`, and `Connection/joint#JOINT_FAMILY`.

## [01]-[INDEX]

- [01]-[FASTENER_FAMILY]: the `FastenerKind` member-type discriminant, the `FastenerStandard` spec discriminant, the `FastenerGrade` proof/tensile/shear-factor axis, the `BoltCategory` bearing-vs-preloaded axis, the `FayingSurface` slip-coefficient class, the `ThreadSeries`/`ThreadSize` nominal-thread axis carrying the ISO 898-1 stress area, the `FastenerSection` thread receipt with the bolt shear/tension/bearing/preload capacity family, and the `ConnectionCatalogue.BuildFastenerRows` ISO 898-1 / SAE J429 / ASTM F3125 row table.
- [02]-[BOLT_ASSEMBLY]: the `FastenerAssembly` complete-connection owner — the bolt + grip-plies (`Dimension`) + nut + washer admission over one `FastenerSection`, the `PreloadKn` `Fp,C = 0.7·fub·As` projection, and the `SlipResistanceKn` EN 1993-1-8 preloaded design value the category-D/E slip-critical seam reads.

## [02]-[FASTENER_FAMILY]

- Owner: the fastener vocabulary (`FastenerKind` the bolt/nut/screw/anchor/dowel/rivet/coupler member-type discriminant, `FastenerStandard` the ISO 898-1 / SAE J429 / ASTM F3125 spec discriminant, `FastenerGrade` the proof/tensile/shear-factor strength axis, `BoltCategory` the EN 1993-1-8 bearing-vs-preloaded category axis, `FayingSurface` the slip-coefficient class, `ThreadSeries`/`ThreadSize` the M6..M36 / 1/4in..1-1/2in nominal-thread axis, `FastenerSection` the thread receipt); `ConnectionCatalogue.BuildFastenerRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `ThreadSize.TensileStressAreaMm2` projection emitting the ISO 898-1 stress area the bolt-capacity family reads.
- Cases: kind {bolt (externally threaded, headed) · nut (internally threaded mate, an `IfcDiscreteAccessory` companion at the wire) · screw (self-driven, no nut) · anchor (cast-in or post-installed) · dowel (unthreaded shear pin) · rivet (driven permanent) · coupler (bar/rebar mechanical splice)} · standard {ISO 898-1 (metric property class) · SAE J429 (inch grade) · ASTM F3125 (structural-bolt, A325/A490 lineage)} · grade {ISO 898-1 cls 4.6 / 4.8 / 5.6 / 5.8 / 6.8 / 8.8 / 10.9 / 12.9 · SAE J429 Gr2 / Gr5 / Gr8 · ASTM F3125 A325 / A490} · category {A (bearing, non-preloaded) · B (slip-critical at SLS) · C (slip-critical at ULS) · D (tension, non-preloaded) · E (tension, preloaded)} · size {metric coarse M6..M36 · imperial UNC 1/4in..1-1/2in} — a fastener is a `ConnectionItem` row over one `FastenerKind`, one `FastenerGrade`, one `ThreadSize`, and one `BoltCategory`, never a fastener subtype.
- Entry: `public double TensileStressAreaMm2 { get; }` on `ThreadSize` — the ISO 898-1 nominal tensile stress area `As = (π/4)·((d2+d3)/2)²` over the pitch/minor diameter the size carries (a pure thread property, the area both the `FastenerSection` capacity family and the `FastenerAssembly` preload read, never re-computed at each call site); `FastenerSection.ShearCapacityKn(bool threadsInShearPlane)` / `BearingCapacityKn(double plyThicknessMm, double plyUltimateMpa, double edgeFactor)` / `ProofLoadKn` / `TensileLoadKn` project the AISC 360 J3 / EN 1993-1-8 single-bolt resistances over the stored thread columns and the grade band (a derived `double` projection over the stored `PositiveMagnitude` columns, never a re-minted dimension primitive, exactly as `RebarSection.YieldForceKn` projects); `ConnectionCatalogue.BuildFastenerRows(context)` folds the ISO/SAE/ASTM `FastenerRow` table through `FastenerOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBoltBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` for the thread-diameter/pitch/shank/head/stress-area length columns and `Dimension` for the discrete grip-ply count, never an int-backed `Dimension` that truncates a fractional millimeter pitch), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the kind/standard/grade/category/faying/series/size axes with the generated total `Switch`, `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`). No bolt-grade external package — `VividOrange.Materials` is EN-only structural-MEMBER grade DATA (`EnSteelGrade` S235..S460), with NO ISO 898-1 / SAE / ASTM F3125 bolt-property-class producer (`.api/api-vividorange-materials.md` `[FLOOR_SCOPE_GATE]`), so the bolt property classes are the realized catalogue vocabulary here exactly as `reinforcement#REINFORCEMENT_FAMILY` hand-keys the ASTM bar bands; the structural design code is CITED (`VividOrange.Standards` `En1993Part.Part1_8` joints, `.api/api-vividorange-standards.md`) on the `BoltCategory` axis, never re-minted.
- Growth: the fastener vocabulary grows by data — a new property class is one `FastenerGrade` row carrying its proof/tensile/yield/shear-factor, a new thread size one `ThreadSize` row carrying its major-diameter/pitch/minor-diameter, a new member type one `FastenerKind` row, a new bolt category one `BoltCategory` row, a new designation one `FastenerRow` catalogue entry — never a per-bolt type, never a per-grade `ConnectionItem` variant. `anchor`/`dowel`/`rivet`/`coupler` are `FastenerKind` arms folded INSIDE this vocabulary, so the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint), never a fifth sibling family or a per-bolt type; a reinforcement/hanger/joint family lands its own vocabulary on its own page the way fastener carries `FastenerKind`/`FastenerGrade`/`FastenerSection`. The bolt-capacity arithmetic the `FastenerSection`/`FastenerAssembly` project is the `[02]`/`[02]-[BOLT_ASSEMBLY]` owner concern; a global multi-bolt connection-group resistance (the `ΣFv,Rd` group reduction, the long-joint β factor) is the `Rasm.Compute` design-check concern reading these single-bolt receipts off the seam, never re-derived here.
- Boundary: the fastener vocabulary is a realized `ConnectionFamily` — a per-bolt `Bolt`/`Nut`/`Screw`/`Anchor`/`Dowel`/`Rivet`/`Coupler` class is the deleted form, collapsed into the one `FastenerKind` `[SmartEnum]` arm (the 3+-parallel-item-shape collapse trigger), and the cast-in/post-installed anchor is the `FastenerKind.Anchor` row, NEVER a separate `ConnectionFamily` case; `FastenerSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional ISO thread (`M12` major `12.000 mm`, pitch `1.75 mm`; `3/8in` major `9.525 mm`, 16-TPI pitch `1.5875 mm`) admits without the truncation an int-backed `Dimension` count would force, the `Dimension` carrier reserved for discrete counts (thread starts, grip plies — the `FastenerAssembly` grip count); the ISO 898-1 tensile stress area is the `ThreadSize.TensileStressAreaMm2` derived `double` projection (over the stored `PositiveMagnitude` thread columns) the bolt-capacity family and the `IfcMechanicalFastener` wire read, NOT a host computation; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for alloy-steel cls 8.8/10.9/12.9 and SAE Gr5/Gr8 and structural A325/A490, `metal.iron` for low-carbon cls 4.6/4.8/5.6/SAE Gr2) the row's `ConnectionItem.AppearanceId` column carries, never a fastener-specific shade; the mechanical base-metal capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa` axis is the spec-nominal class band and the measured capacity is the property-library receipt the bearing projection's `plyUltimateMpa` supplies; `ConnectionCatalogue.BuildFastenerRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the ISO 898-1 / SAE J429 / ASTM F3125 rows keyed `connection.<designation>` (`connection.bolt-m12-88`, `connection.bolt-0375-gr5`, `connection.anchor-m16-88`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a bolt pattern reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel fastener-layout owner; the fastener reaches `Rasm.Bim` ONLY as the `connection#CONNECTION_PROJECTOR` `ConnectionProjector`-authored seam `Object` node plus its `Rasm_ConnectionRealization` `PropertySet` detail bag (NEVER a second `ConnectionItemWire`/`ConnectionWire` carrier — the deleted form `connection#CONNECTION_PROJECTOR` retires), the `Projection/semantic#IFC_EGRESS` `Emit` serializing it to the IFC 4.3 `IfcMechanicalFastener`/`IfcFastener` element — and the `IfcMechanicalFastenerTypeEnum` `PredefinedType` token is a `Rasm.Bim` EGRESS gate (§4-RT C6: the predefined type is resolved + validated at `Emit`, not authored here), so this owner carries only the `FastenerKind.IfcPredefinedType` portable string the projector lands on the seam `Object` node's `PredefinedType` and in the detail bag's `FastenerType` row, the `Semantics/connection#CONNECTION_DETAIL` `ConnectionProjection.Detail` reader recovering it one-hop, NEVER an interior `IfcOpenShell` evaluation.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude — thread/length/head columns), Dimension (>=1 discrete count — the FastenerAssembly grip-ply/shear-plane carrier) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                  // MaterialId (the seam-carried material identity the ConnectionItem AppearanceId/CapacityKey reference)
using Rasm.Materials.Connection;     // ConnectionFamily/ConnectionId/ConnectionItem/ConnectionSection/ConnectionFault (the parent CONNECTION_OWNER)
using Thinktecture;
using static LanguageExt.Prelude;

// Each Connection family page is its OWN Rasm.Materials.Connection.<Family> sub-namespace so the four sibling
// `ConnectionCatalogue` static classes are distinct types (a shared Rasm.Materials.Connection would be a CS0101 collision
// with connection.md's own `ConnectionCatalogue`); connection#CONNECTION_OWNER stays the parent Rasm.Materials.Connection
// and folds Fastener.ConnectionCatalogue.BuildFastenerRows by the sub-namespace-qualified name; parent types via the using above.
namespace Rasm.Materials.Connection.Fastener;

// --- [TYPES] -------------------------------------------------------------------------------
// FastenerKind is the member-type axis grown by data, NOT a per-kind class. IfcPredefinedType is the VERIFIED
// GeometryGym.Ifc.IfcMechanicalFastenerTypeEnum member name the connection#CONNECTION_PROJECTOR ConnectionProjector lands
// on the seam Object node's PredefinedType and in the Rasm_ConnectionRealization detail bag's FastenerType row (the
// Rasm.Bim Semantics/connection#CONNECTION_DETAIL reader recovers it one-hop). BOLT / SCREW /
// ANCHORBOLT / DOWEL / RIVET are real members; the enum has NO `NUT` member, so the nut rides USERDEFINED as an
// IfcDiscreteAccessory companion on the bolt with a NUT project property, and a bar coupler USERDEFINED with a COUPLER
// project property (the [IFC_FASTENER_WIRE] law). The token is the egress HINT the Rasm.Bim Emit gate resolves +
// validates (§4-RT C6), never the authority — Bim resolves the IfcClass row and runs AdmitPredefined; this owner carries
// only the portable string.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerKind {
    public static readonly FastenerKind Bolt    = new("bolt",    ifcPredefinedType: "BOLT",        threaded: true,  headed: true);
    public static readonly FastenerKind Nut     = new("nut",     ifcPredefinedType: "USERDEFINED", threaded: true,  headed: false);
    public static readonly FastenerKind Screw   = new("screw",   ifcPredefinedType: "SCREW",       threaded: true,  headed: true);
    public static readonly FastenerKind Anchor  = new("anchor",  ifcPredefinedType: "ANCHORBOLT",  threaded: true,  headed: true);
    public static readonly FastenerKind Dowel   = new("dowel",   ifcPredefinedType: "DOWEL",       threaded: false, headed: false);
    public static readonly FastenerKind Rivet   = new("rivet",   ifcPredefinedType: "RIVET",       threaded: false, headed: true);
    public static readonly FastenerKind Coupler = new("coupler", ifcPredefinedType: "USERDEFINED", threaded: true,  headed: false);
    public string IfcPredefinedType { get; }
    public bool Threaded { get; }   // a dowel/rivet has no thread; ThreadSize is the unthreaded shank for those rows
    public bool Headed { get; }
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
// reduces) — this is the threads-in-shear-plane coefficient; the unthreaded-shank-plane αv is 0.6 for EVERY grade (the
// reduction does NOT apply to the shank), applied directly in FastenerSection.ShearCapacityKn — and the Preloadable flag
// (only 8.8 and 10.9 / A325 / A490 are preloadable for a slip-critical joint, EN 1993-1-8 §3.9 / RCSC). The band is the
// spec-nominal grade the connection schedule reads; the measured base-metal strength is the
// properties#MATERIAL_PROPERTY_CATALOGUE Mechanical receipt read by AppearanceId.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerGrade {
    public static readonly FastenerGrade Cls46  = new("4.6",  proofStressMpa: 225.0,  tensileStrengthMpa: 400.0,  minimumYieldMpa: 240.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.Iso898,    appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls48  = new("4.8",  proofStressMpa: 310.0,  tensileStrengthMpa: 420.0,  minimumYieldMpa: 340.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls56  = new("5.6",  proofStressMpa: 280.0,  tensileStrengthMpa: 500.0,  minimumYieldMpa: 300.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.Iso898,    appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls58  = new("5.8",  proofStressMpa: 380.0,  tensileStrengthMpa: 500.0,  minimumYieldMpa: 400.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls68  = new("6.8",  proofStressMpa: 440.0,  tensileStrengthMpa: 600.0,  minimumYieldMpa: 480.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Cls88  = new("8.8",  proofStressMpa: 600.0,  tensileStrengthMpa: 800.0,  minimumYieldMpa: 640.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.Iso898,    appearanceId: "metal.steel", preloadable: true);
    public static readonly FastenerGrade Cls109 = new("10.9", proofStressMpa: 830.0,  tensileStrengthMpa: 1040.0, minimumYieldMpa: 940.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    appearanceId: "metal.steel", preloadable: true);
    public static readonly FastenerGrade Cls129 = new("12.9", proofStressMpa: 970.0,  tensileStrengthMpa: 1220.0, minimumYieldMpa: 1100.0, shearStrengthFactor: 0.50, standard: FastenerStandard.Iso898,    appearanceId: "metal.steel", preloadable: false);
    public static readonly FastenerGrade Gr2    = new("gr2",  proofStressMpa: 379.0,  tensileStrengthMpa: 510.0,  minimumYieldMpa: 393.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.SaeJ429,   appearanceId: "metal.iron",  preloadable: false);
    public static readonly FastenerGrade Gr5    = new("gr5",  proofStressMpa: 585.0,  tensileStrengthMpa: 827.0,  minimumYieldMpa: 634.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.SaeJ429,   appearanceId: "metal.steel", preloadable: false);
    public static readonly FastenerGrade Gr8    = new("gr8",  proofStressMpa: 830.0,  tensileStrengthMpa: 1034.0, minimumYieldMpa: 896.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.SaeJ429,   appearanceId: "metal.steel", preloadable: false);
    public static readonly FastenerGrade A325   = new("a325", proofStressMpa: 585.0,  tensileStrengthMpa: 827.0,  minimumYieldMpa: 634.0,  shearStrengthFactor: 0.60, standard: FastenerStandard.AstmF3125, appearanceId: "metal.steel", preloadable: true);
    public static readonly FastenerGrade A490   = new("a490", proofStressMpa: 830.0,  tensileStrengthMpa: 1040.0, minimumYieldMpa: 940.0,  shearStrengthFactor: 0.50, standard: FastenerStandard.AstmF3125, appearanceId: "metal.steel", preloadable: true);
    public double ProofStressMpa { get; }
    public double TensileStrengthMpa { get; }
    public double MinimumYieldMpa { get; }
    public double ShearStrengthFactor { get; }   // EN 1993-1-8 Table 3.4 αv — the bolt-tensile-to-shear coefficient
    public FastenerStandard Standard { get; }
    public string AppearanceId { get; }
    public bool Preloadable { get; }              // only these classes admit a slip-critical (category B/C/E) joint
    public bool Metric => Standard.Metric;
}

// BoltCategory is the EN 1993-1-8 Table 3.2 joint category — the bearing-vs-preloaded axis that selects WHICH design
// resistance governs and WHETHER a preload + faying-surface slip coefficient is required. It cites En1993Part.Part1_8
// (the joints code, VividOrange.Standards) rather than inlining the clause numbers. The Preloaded flag gates the
// FastenerAssembly slip-resistance projection; a non-preloadable grade in a B/C/E joint rails ConnectionFault.Grade.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BoltCategory {
    public static readonly BoltCategory A = new("A", shear: true,  preloaded: false, clause: "EN 1993-1-8 Cat A bearing");
    public static readonly BoltCategory B = new("B", shear: true,  preloaded: true,  clause: "EN 1993-1-8 Cat B slip-resistant SLS");
    public static readonly BoltCategory C = new("C", shear: true,  preloaded: true,  clause: "EN 1993-1-8 Cat C slip-resistant ULS");
    public static readonly BoltCategory D = new("D", shear: false, preloaded: false, clause: "EN 1993-1-8 Cat D tension non-preloaded");
    public static readonly BoltCategory E = new("E", shear: false, preloaded: true,  clause: "EN 1993-1-8 Cat E tension preloaded");
    public bool Shear { get; }       // a shear category reads ShearCapacityKn/BearingCapacityKn; a tension category TensileLoadKn
    public bool Preloaded { get; }   // a preloaded category requires Preloadable grade + a FayingSurface slip class
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
    public static readonly ThreadSeries MetricCoarse = new("metric-coarse", metric: true);
    public static readonly ThreadSeries MetricFine   = new("metric-fine",   metric: true);
    public static readonly ThreadSeries UnifiedCoarse = new("unc",          metric: false);
    public static readonly ThreadSeries UnifiedFine   = new("unf",          metric: false);
    public bool Metric { get; }
}

// ThreadSize is the nominal-thread axis carrying the basic-profile diameters AND the ISO 898-1 tensile stress area
// (a pure thread property, derived once here, never re-computed at each FastenerSection call site). PitchDiameterMm is
// the ISO 724 basic-profile pitch diameter d2 = d − 0.649519·P; TensileStressAreaMm2 the ISO 898-1 As = (π/4)·((d2+d3)/2)²
// over the pitch (d2) and rounded-root minor (d3 = MinorDiameterMm) diameters (cls 8.8 M12: As ≈ 84.3 mm²).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSize {
    public static readonly ThreadSize M6   = new("m6",     series: ThreadSeries.MetricCoarse, majorDiameterMm: 6.000,  pitchMm: 1.000,  minorDiameterMm: 4.773,  acrossFlatsMm: 10.000);
    public static readonly ThreadSize M8   = new("m8",     series: ThreadSeries.MetricCoarse, majorDiameterMm: 8.000,  pitchMm: 1.250,  minorDiameterMm: 6.466,  acrossFlatsMm: 13.000);
    public static readonly ThreadSize M10  = new("m10",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 10.000, pitchMm: 1.500,  minorDiameterMm: 8.160,  acrossFlatsMm: 16.000);
    public static readonly ThreadSize M12  = new("m12",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 12.000, pitchMm: 1.750,  minorDiameterMm: 9.853,  acrossFlatsMm: 18.000);
    public static readonly ThreadSize M16  = new("m16",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 16.000, pitchMm: 2.000,  minorDiameterMm: 13.546, acrossFlatsMm: 24.000);
    public static readonly ThreadSize M20  = new("m20",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 20.000, pitchMm: 2.500,  minorDiameterMm: 16.933, acrossFlatsMm: 30.000);
    public static readonly ThreadSize M24  = new("m24",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 24.000, pitchMm: 3.000,  minorDiameterMm: 20.319, acrossFlatsMm: 36.000);
    public static readonly ThreadSize M30  = new("m30",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 30.000, pitchMm: 3.500,  minorDiameterMm: 25.706, acrossFlatsMm: 46.000);
    public static readonly ThreadSize M36  = new("m36",    series: ThreadSeries.MetricCoarse, majorDiameterMm: 36.000, pitchMm: 4.000,  minorDiameterMm: 31.093, acrossFlatsMm: 55.000);
    public static readonly ThreadSize In025 = new("1/4",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 6.350,  pitchMm: 1.2700, minorDiameterMm: 4.976,  acrossFlatsMm: 11.113);
    public static readonly ThreadSize In037 = new("3/8",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 9.525,  pitchMm: 1.5875, minorDiameterMm: 7.722,  acrossFlatsMm: 14.288);
    public static readonly ThreadSize In050 = new("1/2",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 12.700, pitchMm: 1.9538, minorDiameterMm: 10.450, acrossFlatsMm: 19.050);
    public static readonly ThreadSize In062 = new("5/8",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 15.875, pitchMm: 2.3091, minorDiameterMm: 13.157, acrossFlatsMm: 23.813);
    public static readonly ThreadSize In075 = new("3/4",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 19.050, pitchMm: 2.5400, minorDiameterMm: 16.090, acrossFlatsMm: 28.575);
    public static readonly ThreadSize In087 = new("7/8",   series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 22.225, pitchMm: 2.8222, minorDiameterMm: 18.928, acrossFlatsMm: 33.338);
    public static readonly ThreadSize In100 = new("1",     series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 25.400, pitchMm: 3.1750, minorDiameterMm: 21.557, acrossFlatsMm: 38.100);
    public static readonly ThreadSize In150 = new("1-1/2", series: ThreadSeries.UnifiedCoarse, majorDiameterMm: 38.100, pitchMm: 4.2333, minorDiameterMm: 32.539, acrossFlatsMm: 57.150);
    public ThreadSeries Series { get; }
    public double MajorDiameterMm { get; }
    public double PitchMm { get; }
    public double MinorDiameterMm { get; }
    public double AcrossFlatsMm { get; }
    public double PitchDiameterMm => MajorDiameterMm - 0.649519 * PitchMm;
    public double NominalAreaMm2 => Math.PI / 4.0 * MajorDiameterMm * MajorDiameterMm;          // gross shank area (shank-in-shear plane)
    public double TensileStressAreaMm2 => Math.PI / 16.0 * Math.Pow(PitchDiameterMm + MinorDiameterMm, 2.0);   // ISO 898-1 As
}

// --- [MODELS] ------------------------------------------------------------------------------
// FastenerSection is the thread receipt PLUS the host-neutral single-bolt design-capacity family — the AISC 360 J3 /
// EN 1993-1-8 Table 3.4 shear/tension/bearing resistances over the stored thread columns and the grade band, the
// derived `double` projections the Rasm.Compute design-code checks read off the seam (the CONSUMER contract the seam
// SectionProperties names) and the IfcMechanicalFastener wire reads, never a re-minted dimension or a host computation.
// The detail-bag columns (Kind/Size/ThreadDiameterMm/ShankLengthMm) are KEPT exactly as connection#CONNECTION_PROJECTOR's
// ConnectionProjector Detail fastener arm and ConnectionSection.NominalMm read them; the capacity family is additive.
public readonly record struct FastenerSection(
    FastenerKind Kind,
    FastenerGrade Grade,
    ThreadSize Size,
    BoltCategory Category,
    PositiveMagnitude ThreadDiameterMm,
    PositiveMagnitude PitchMm,
    PositiveMagnitude ShankLengthMm,
    PositiveMagnitude AcrossFlatsMm) {

    // The FINISH the appearance projection reads (metal.iron for low-carbon cls 4.6/4.8/5.6/SAE Gr2, metal.steel for alloy
    // cls 8.8/10.9/12.9 and A325/A490), INDEPENDENT from CapacityKey: a galvanized bolt keeps the steel CapacityKey while
    // its AppearanceId names a zinc-coat finish row, the connection#CONNECTION_OWNER two-slot independence.
    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);

    // The CAPACITY material whose properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row the design seam reads — the bolt's
    // structural STEEL (metal.steel), sourced INDEPENDENTLY from the AppearanceId finish (a galvanized/coated bolt's
    // capacity steel and its finish are distinct MaterialIds, neither derived from the other).
    public MaterialId CapacityKey => MaterialId.Of("metal.steel");

    public double TensileStressAreaMm2 => Size.TensileStressAreaMm2;
    public double ProofLoadKn => Grade.ProofStressMpa * TensileStressAreaMm2 * 1e-3;
    public double TensileLoadKn => Grade.TensileStrengthMpa * TensileStressAreaMm2 * 1e-3;

    // EN 1993-1-8 Table 3.4 / AISC 360 J3.6 single-bolt shear: Fv = αv · fub · A, with A = the tensile stress area when
    // the shear plane passes through the THREAD and the gross shank area when it passes through the unthreaded SHANK. The
    // αv coefficient is plane-DEPENDENT: the stored Grade.ShearStrengthFactor (0.5 for the reducing 4.8/5.8/6.8/10.9
    // classes, 0.6 otherwise) is the THREADED-plane factor; when the shear plane passes through the unthreaded SHANK the
    // EN 1993-1-8 Table 3.4 reduction does NOT apply and αv is 0.6 for EVERY grade — applying the stored 0.5 to the shank
    // case is the deleted form (it under-predicts a 10.9 shank-shear bolt by ~17%). The 0.6 here is the EN shank-plane αv,
    // not a base-metal read.
    public double ShearCapacityKn(bool threadsInShearPlane) =>
        (threadsInShearPlane ? Grade.ShearStrengthFactor : 0.60) * Grade.TensileStrengthMpa
            * (threadsInShearPlane ? TensileStressAreaMm2 : Size.NominalAreaMm2) * 1e-3;

    // EN 1993-1-8 Table 3.4 / AISC 360 J3.10 single-bolt bearing on the connected ply: Fb = k1·αb · fu,ply · d · t, the
    // edge/end/pitch geometry collapsed into one caller-supplied edgeFactor (k1·αb, capped at the spec ceiling by the
    // caller) and the ply ultimate strength supplied by the properties#MATERIAL_PROPERTY_CATALOGUE Mechanical receipt the
    // Rasm.Compute design check reads — never a base-metal strength re-keyed here.
    public double BearingCapacityKn(double plyThicknessMm, double plyUltimateMpa, double edgeFactor) =>
        edgeFactor * plyUltimateMpa * ThreadDiameterMm.Value * plyThicknessMm * 1e-3;
}

// FastenerRow is the flat catalogue seed; Category defaults the schedule to bearing-type A unless a structural-bolt row
// names a preloaded category (the slip-critical A325/A490 rows), the faying class None for a bearing joint.
public readonly record struct FastenerRow(string Designation, string Kind, string Size, string Grade, string Category, string Faying, double ShankLengthMm);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    static readonly Seq<FastenerRow> FastenerRows = Seq(
        new FastenerRow("connection.bolt-m12-88",    "bolt",   "m12", "8.8",  "A", "none",    60.0),
        new FastenerRow("connection.bolt-m16-88",    "bolt",   "m16", "8.8",  "A", "none",    80.0),
        new FastenerRow("connection.bolt-m16-109",   "bolt",   "m16", "10.9", "C", "class-a", 80.0),
        new FastenerRow("connection.bolt-m20-88",    "bolt",   "m20", "8.8",  "A", "none",    90.0),
        new FastenerRow("connection.bolt-m20-109",   "bolt",   "m20", "10.9", "C", "class-a", 90.0),
        new FastenerRow("connection.bolt-m24-109",   "bolt",   "m24", "10.9", "C", "class-a", 110.0),
        new FastenerRow("connection.bolt-m30-129",   "bolt",   "m30", "12.9", "A", "none",    140.0),
        new FastenerRow("connection.bolt-0375-gr5",  "bolt",   "3/8", "gr5",  "A", "none",    63.5),
        new FastenerRow("connection.bolt-0500-gr5",  "bolt",   "1/2", "gr5",  "A", "none",    76.2),
        new FastenerRow("connection.bolt-0750-gr8",  "bolt",   "3/4", "gr8",  "A", "none",    101.6),
        new FastenerRow("connection.bolt-0875-a325", "bolt",   "7/8", "a325", "C", "class-a", 114.3),
        new FastenerRow("connection.bolt-0875-a490", "bolt",   "7/8", "a490", "C", "class-b", 114.3),
        new FastenerRow("connection.nut-m16-88",     "nut",    "m16", "8.8",  "A", "none",    13.0),
        new FastenerRow("connection.nut-m20-109",    "nut",    "m20", "10.9", "A", "none",    16.0),
        new FastenerRow("connection.screw-m8-88",    "screw",  "m8",  "8.8",  "A", "none",    40.0),
        new FastenerRow("connection.screw-0250-gr2", "screw",  "1/4", "gr2",  "A", "none",    31.8),
        new FastenerRow("connection.dowel-m20-88",   "dowel",  "m20", "8.8",  "A", "none",    100.0),
        new FastenerRow("connection.coupler-m20-88", "coupler","m20", "8.8",  "A", "none",    60.0),
        new FastenerRow("connection.anchor-m16-88",  "anchor", "m16", "8.8",  "D", "none",    200.0),
        new FastenerRow("connection.anchor-m20-88",  "anchor", "m20", "8.8",  "D", "none",    250.0),
        new FastenerRow("connection.anchor-0750-a325","anchor","3/4", "a325", "E", "class-a", 304.8));

    static Fin<(ConnectionId Id, ConnectionItem Item)> FastenerOf(FastenerRow r, Context context, Op key) =>
        from kind in FastenerKind.TryGet(r.Kind, out FastenerKind? k) ? Fin.Succ(k!) : Fin.Fail<FastenerKind>(ConnectionFault.Designation(key, $"<unknown-fastener-kind:{r.Kind}>"))
        from size in ThreadSize.TryGet(r.Size, out ThreadSize? t) ? Fin.Succ(t!) : Fin.Fail<ThreadSize>(ConnectionFault.Designation(key, $"<unknown-thread-size:{r.Size}>"))
        from grade in FastenerGrade.TryGet(r.Grade, out FastenerGrade? g) ? Fin.Succ(g!) : Fin.Fail<FastenerGrade>(ConnectionFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from category in BoltCategory.TryGet(r.Category, out BoltCategory? c) ? Fin.Succ(c!) : Fin.Fail<BoltCategory>(ConnectionFault.Grade(key, $"<unknown-category:{r.Category}>"))
        from faying in FayingSurface.TryGet(r.Faying, out FayingSurface? f) ? Fin.Succ(f!) : Fin.Fail<FayingSurface>(ConnectionFault.Grade(key, $"<unknown-faying:{r.Faying}>"))
        from preloadValid in guard(!category.Preloaded || grade.Preloadable, ConnectionFault.Grade(key, $"<non-preloadable-grade-in-preloaded-joint:{grade.Key}:{category.Key}>"))
        from metricMatch in guard(size.MajorDiameterMm > 0.0, ConnectionFault.Grade(key, $"<degenerate-thread:{r.Size}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.MajorDiameterMm)
        from pitch in key.AcceptValidated<PositiveMagnitude>(candidate: size.PitchMm)
        from shank in key.AcceptValidated<PositiveMagnitude>(candidate: r.ShankLengthMm)
        from flats in key.AcceptValidated<PositiveMagnitude>(candidate: size.AcrossFlatsMm)
        let section = new FastenerSection(kind, grade, size, category, diameter, pitch, shank, flats)
        from item in ConnectionItem.Of(ConnectionFamily.Fastener, r.Designation, new ConnectionSection.Fastener(section), section.CapacityKey, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildFastenerRows(Context context) =>
        FastenerRows
            .Choose(row => FastenerOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[BOLT_ASSEMBLY]

- Owner: `FastenerAssembly` the complete-connection owner over one `FastenerSection` — the bolt PLUS the discrete `Dimension` grip-ply count, the nut+washer presence, and the preloaded design state (the `BoltCategory.Preloaded` + `FayingSurface` slip class); the `PreloadKn` `Fp,C = 0.7·fub·As` projection (EN 1993-1-8 §3.9 / RCSC) and the `SlipResistanceKn` `Fs,Rd = ks·n·μ·Fp,C/γM3` projection the category-B/C/E slip-critical seam reads. `FastenerAssembly.Of` is the ONE assembly boundary railing a degenerate grip / a non-preloadable grade in a preloaded joint.
- Cases: a fastener assembly is one `FastenerSection` (the bolt) + a `Dimension` grip-ply count (the connected layers the shank passes — the `Dimension` discrete-count carrier `connection#CONNECTION_OWNER` reserves for grip plies) + a nut/washer presence flag + the section's `Category`/`FayingSurface` — no per-assembly subtype, the modality riding the `FastenerSection` it composes; a non-preloaded (category A/D) assembly carries `FayingSurface.None` and reads zero slip resistance, a preloaded (B/C/E) assembly the named slip class and the full slip resistance.
- Entry: `public static Fin<FastenerAssembly> Of(FastenerSection section, FayingSurface faying, int gripPlies, int shearPlanes, bool withWasher, Op key)` — the ONE assembly admission: it admits the `gripPlies`/`shearPlanes` discrete counts through `key.AcceptValidated<Dimension>(candidate:)` (a non-positive count rails `ConnectionFault.Capacity`), rails a preloaded `Category` over a non-`Preloadable` grade through `ConnectionFault.Grade`, and reads `section.Category.Preloaded ? faying : FayingSurface.None` so a bearing joint never relies on a slip coefficient; `public double PreloadKn` projects `0.7·fub·As`, `public double SlipResistanceKn(double ks, double gammaM3)` the EN 1993-1-8 slip resistance over the admitted shear-plane count and the faying μ — one polymorphic boundary, never a `BuildPreloadedAssembly`/`BuildBearingAssembly` family.
- Packages: Rasm (project — `PositiveMagnitude` through the composed `FastenerSection`, `Dimension` for the grip-ply/shear-plane discrete counts), Thinktecture.Runtime.Extensions (the `FastenerGrade`/`BoltCategory`/`FayingSurface` axes the assembly reads through their generated keys), LanguageExt.Core (`Fin`/`Seq`/`Fold` for the admission rail), BCL inbox. No new external package — the assembly composes the realized `[02]` vocabulary and the kernel value-objects; the structural design code is CITED (`En1993Part.Part1_8`), the multi-bolt group resistance the `Rasm.Compute` concern over these single-assembly receipts.
- Growth: a new connection modality is a `FastenerSection`/`BoltCategory` row the assembly reads, never a per-assembly type; the slip-resistance arithmetic the `SlipResistanceKn` projects is the single-bolt design value, a global connection-group `ΣFs,Rd` the `Rasm.Compute` design-check concern; a tension+shear interaction (the EN 1993-1-8 `Fv,Ed/Fv,Rd + Ft,Ed/(1.4·Ft,Rd) ≤ 1` combined-action check) is the `Rasm.Compute` consumer reading `ShearCapacityKn`/`TensileLoadKn`/`SlipResistanceKn` off the seam, never re-derived here.
- Boundary: `FastenerAssembly` is the BOLT-CONNECTION assembler — a per-modality `PreloadedBolt`/`BearingBolt` class is the deleted form, the modality riding the composed `FastenerSection.Category`/`FayingSurface`; the grip-ply and shear-plane counts are the `Rasm` kernel `Dimension` discrete-count carrier (`connection#CONNECTION_OWNER` reserves `Dimension` for grip plies, never a `PositiveMagnitude` that admits a fractional layer), admitted once through `key.AcceptValidated<Dimension>(candidate:)`; the preload is `Fp,C = 0.7·fub·As` over the `FastenerGrade.TensileStrengthMpa` band and the `ThreadSize.TensileStressAreaMm2`, NOT a host computation; the slip resistance `Fs,Rd = ks·n·μ·Fp,C/γM3` reads the admitted shear-plane count `n` and the `FayingSurface.SlipFactor` μ, the `ks` hole-tolerance factor and the `γM3` partial factor caller-supplied (the design-code parameters the `Rasm.Compute` consumer owns, never inlined here); a preloaded category over a non-preloadable grade is impossible (railed at `Of`), so a slip-resistance read is always over a valid preloaded bolt; the assembly is host-neutral scalar DATA the `IfcMechanicalFastener` wire reads (the nut a separate `IfcDiscreteAccessory` companion at the `Rasm.Bim` boundary, the washer a project property), never an interior `IfcOpenShell` evaluation; the assembly is NOT a `ConnectionItem` — a `ConnectionItem` is one discrete bolt in the schedule, the `FastenerAssembly` the populated connection the bolt completes, the two meeting at the `FastenerSection` vocabulary this page already owns.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The complete bolt-connection receipt: the FastenerSection bolt plus the discrete grip-ply / shear-plane counts (the
// Dimension carrier), the resolved faying-surface slip class (None for a bearing joint), and the washer presence the
// IfcDiscreteAccessory wire reads. The preload/slip-resistance projections are the EN 1993-1-8 §3.9 single-bolt design
// values the Rasm.Compute slip-critical and combined-action checks read off the seam, never re-derived per consumer.
public readonly record struct FastenerAssembly(
    FastenerSection Section,
    FayingSurface Faying,
    Dimension GripPlies,
    Dimension ShearPlanes,
    bool WithWasher) {

    public static Fin<FastenerAssembly> Of(FastenerSection section, FayingSurface faying, int gripPlies, int shearPlanes, bool withWasher, Op key) =>
        from preloadValid in guard(!section.Category.Preloaded || section.Grade.Preloadable, ConnectionFault.Grade(key, $"<non-preloadable-grade-in-preloaded-joint:{section.Grade.Key}:{section.Category.Key}>"))
        from plies in key.AcceptValidated<Dimension>(candidate: gripPlies)
        from planes in key.AcceptValidated<Dimension>(candidate: shearPlanes)
        let resolvedFaying = section.Category.Preloaded ? faying : FayingSurface.None
        select new FastenerAssembly(section, resolvedFaying, plies, planes, withWasher);

    // EN 1993-1-8 §3.9.1 / RCSC preload: Fp,C = 0.7 · fub · As — the design pretension a preloaded high-strength bolt is
    // installed to (zero for a non-preloadable / bearing-category bolt, so a bearing joint reads no preload).
    public double PreloadKn => Section.Category.Preloaded && Section.Grade.Preloadable
        ? 0.7 * Section.Grade.TensileStrengthMpa * Section.TensileStressAreaMm2 * 1e-3
        : 0.0;

    // EN 1993-1-8 §3.9.1 slip resistance per bolt: Fs,Rd = ks · n · μ · Fp,C / γM3, with n the friction-interface count
    // (the admitted ShearPlanes) and μ the faying-surface slip factor; the ks hole-tolerance factor and the γM3 partial
    // factor are the design-code parameters the Rasm.Compute consumer supplies, never inlined here.
    public double SlipResistanceKn(double ks, double gammaM3) =>
        gammaM3 > 0.0 ? ks * ShearPlanes.Value * Faying.SlipFactor * PreloadKn / gammaM3 : 0.0;
}
```

## [04]-[RESEARCH]

- [FASTENER_ROW_TRANSCRIPTION]: REALIZED — the ISO 898-1 metric property-class and SAE J429 / ASTM F3125 inch-grade mechanical-fastener catalogue (the M6..M36 ISO metric coarse and 1/4in..1-1/2in UNC nominal threads, the ISO 898-1 cls 4.6/4.8/5.6/5.8/6.8/8.8/10.9/12.9 proof/tensile/yield bands, the SAE J429 Gr2/Gr5/Gr8 grades, and the ASTM F3125 A325/A490 structural-bolt lineage) seeds through `ConnectionCatalogue.BuildFastenerRows(context)` over the `FastenerRow` designation/kind/size/grade/category/faying/shank table, the major-diameter/pitch/shank/across-flats columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the kind/grade/size/category algebra realized as the fastener vocabulary; the rows are the seed the connection-layout fold consumes and a new fastener is one `FastenerRow` data addition plus, if novel, one `ThreadSize`/`FastenerGrade` row. The major-diameter/pitch/minor-diameter/across-flats values transcribe the published ISO 261 / ISO 724 metric coarse thread series (`M12` major `12.000 mm`, pitch `1.75 mm`, rounded-root minor `9.853 mm`) and the ASME B1.1 Unified inch coarse series (`3/8-16` major `9.525 mm`, pitch `1.5875 mm`); the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa`/`MinimumYieldMpa` bands transcribe the ISO 898-1 Table 3 property-class minima and the SAE J429 grade minima (cls 8.8 proof `600 MPa` / tensile `800 MPa`, Gr5 proof `85 ksi ≈ 585 MPa` / tensile `120 ksi ≈ 827 MPa`), the `ShearStrengthFactor` αv the EN 1993-1-8 Table 3.4 coefficient (0.6 for the classes whose αv applies to the full bolt, 0.5 for the higher-strength 4.8/5.8/6.8/10.9 classes whose threaded shear plane reduces); a non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`, and a preloaded `BoltCategory` over a non-`Preloadable` grade rails `ConnectionFault.Grade` so a category-B/C/E joint never seeds a non-preloadable bolt.
- [TENSILE_STRESS_AREA]: REALIZED — the `ThreadSize.TensileStressAreaMm2` derived `double` projection is the ISO 898-1 nominal stress area `As = (π/4)·((d2+d3)/2)²` where `d2` is the pitch diameter (`ThreadSize.PitchDiameterMm = d − 0.649519·P`, the ISO 724 basic profile) and `d3` the rounded-root minor diameter (`ThreadSize.MinorDiameterMm`), so a `FastenerGrade` proof/tensile band multiplies into `ProofLoadKn`/`TensileLoadKn` directly (cls 8.8 `M12`: `As ≈ 84.3 mm²`, proof load `≈ 50.6 kN`). The stress area lives ONCE on `ThreadSize` (a pure thread property) rather than re-computed at each `FastenerSection` call site; `FastenerSection.NominalAreaMm2` is the gross shank area `(π/4)·d²` the shank-in-shear-plane case reads. The stress area is the connection capacity column the `IfcMechanicalFastener` wire and the structural-bolt seam read; it is NOT re-derived at the `properties#MATERIAL_PROPERTY_CATALOGUE` boundary — the `MaterialId`-keyed `Mechanical` row carries the measured `YieldStrengthMpa`/`YoungsModulusMpa`, the spec-nominal grade band carries the catalogue proof/tensile, and the two meet at the design seam.
- [BOLT_CAPACITY_PROJECTIONS]: REALIZED — the `FastenerSection` capacity family is the host-neutral single-bolt design-value set the `Rasm.Compute` design-code checks read off the seam (the CONSUMER contract the `Composition/material#MATERIAL_OWNER` `SectionProperties` prose names): `ShearCapacityKn(threadsInShearPlane)` is the EN 1993-1-8 Table 3.4 / AISC 360 J3.6 `Fv = αv·fub·A` with `A` the tensile stress area (thread in plane) or the gross shank area (shank in plane) AND the plane-dependent αv — the stored `Grade.ShearStrengthFactor` (the reduced 0.5 for the 4.8/5.8/6.8/10.9 classes) only for the threaded plane, the un-reduced 0.6 for the unthreaded-shank plane for every grade (the EN Table 3.4 reduction not applying to the shank); `BearingCapacityKn(plyThicknessMm, plyUltimateMpa, edgeFactor)` the EN 1993-1-8 / AISC 360 J3.10 `Fb = k1·αb·fu,ply·d·t` over the connected-ply ultimate strength the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` receipt supplies (never re-keyed here) and the caller-supplied edge/end/pitch geometry factor; `ProofLoadKn`/`TensileLoadKn` the single-bolt axial values. The single-bolt resistances are the receipts; the multi-bolt group resistance (the `ΣFv,Rd` reduction, the long-joint β factor, the combined shear+tension interaction) is the `Rasm.Compute` design-check concern over these receipts, never re-derived here.
- [BOLT_ASSEMBLY_COMPOSITION]: REALIZED — the `[02]-[BOLT_ASSEMBLY]` `FastenerAssembly` owner is the host-neutral complete-connection assembler: `FastenerAssembly.Of` admits the discrete grip-ply and shear-plane counts through the `Rasm` kernel `Dimension` carrier (the discrete-count carrier `connection#CONNECTION_OWNER` reserves for grip plies, never a fractional `PositiveMagnitude`), rails a preloaded `BoltCategory` over a non-`Preloadable` `FastenerGrade`, and resolves the `FayingSurface` to `None` for a bearing joint so a non-preloaded connection never relies on a slip coefficient. `PreloadKn` projects the EN 1993-1-8 §3.9 `Fp,C = 0.7·fub·As` design pretension, `SlipResistanceKn(ks, γM3)` the per-bolt slip resistance `Fs,Rd = ks·n·μ·Fp,C/γM3` over the admitted friction-interface count and the faying μ — the slip-critical (category B/C/E) design value the `Rasm.Compute` connection check reads off the seam. The assembly is NOT a `ConnectionItem` (a discrete schedule bolt) — it is the populated connection the bolt completes, the two meeting at the `FastenerSection` vocabulary. Ripple counterpart: `Rasm.Compute` (the multi-bolt connection-group resistance + combined shear+tension interaction over these single-bolt/assembly receipts).
- [IFC_FASTENER_WIRE]: a mechanical fastener round-trips to IFC 4.3 as an `IfcMechanicalFastener` carrying `NominalDiameter` (the `FastenerSection.ThreadDiameterMm`) and `NominalLength` (the `ShankLengthMm`), both GeometryGym-internal scalars the `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reader recovers through the associated `IfcMaterialProfileSetUsage` → `IfcCircleProfileDef.Radius` cross-section. The VERIFIED `GeometryGym.Ifc.IfcMechanicalFastenerTypeEnum` members are {ANCHORBOLT, BOLT, COUPLER, DOWEL, NAIL, NAILPLATE, RIVET, SCREW, SHEARCONNECTOR, STAPLE, STUDSHEARCONNECTOR, USERDEFINED, NOTDEFINED} — there is NO `NUT` member, so the `FastenerKind.IfcPredefinedType` maps `bolt→BOLT`, `screw→SCREW`, `anchor→ANCHORBOLT`, `dowel→DOWEL`, `rivet→RIVET`, and `nut`/`coupler→USERDEFINED` (a nut is an `IfcDiscreteAccessory` companion on the bolt with a `NUT` project property, a coupler an `IfcMechanicalFastener` with a `COUPLER`-described project property since the precise mechanical-splice member is not in the predefined enum). The `IfcMechanicalFastenerTypeEnum` `PredefinedType` is resolved + validated at the `Rasm.Bim` `Projection/semantic#IFC_EGRESS` `Emit` gate (§4-RT C6 — the predefined type is a Bim-owned egress concern, not authored here), this owner carrying only the portable `IfcPredefinedType` string the `connection#CONNECTION_PROJECTOR` `ConnectionProjector` lands on the seam `Object` node's `PredefinedType` and in the `Rasm_ConnectionRealization` detail bag's `FastenerType` row (the `Semantics/connection#CONNECTION_DETAIL` `ConnectionProjection.Detail` reader recovers it). A non-mechanical or fully proprietary fastener (an adhesive/mortar/weld realizing element) crosses as an `IfcFastener` (`IfcFastenerTypeEnum` ∈ {GLUE, MORTAR, WELD, USERDEFINED, NOTDEFINED}) over the `Connection/joint#JOINT_FAMILY` vocabulary; the structural property set `Pset_FastenerMechanical` (the proof/tensile/shear/preload the `ThreadSize.TensileStressAreaMm2`×`FastenerGrade` projections and the `FastenerAssembly.PreloadKn` assert) is the `MaterialId`-keyed `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` read at the federation. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the scalar columns + the portable token the wire reads, never an IFC entity.
