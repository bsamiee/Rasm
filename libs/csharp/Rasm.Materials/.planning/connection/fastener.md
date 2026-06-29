# [MATERIALS_FASTENER]

THE FASTENER CONNECTIONFAMILY. The fastener vocabulary — the `FastenerKind` member-type discriminant (bolt · nut · screw · anchor), the `FastenerGrade` proof/tensile strength axis (ISO 898-1 property classes and SAE J429 grades), the `FastenerSection` thread receipt (nominal thread diameter / thread pitch / shank length / across-flats head dimension), and the `FastenerStandard` spec discriminant — is the realized mechanical-fastener vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Fastener` case. A 3/8in hex bolt is a `ConnectionItem` row, never a `Bolt` type: the kind, the grade, the thread receipt, and the spec are fastener-`ConnectionItem` columns, and the `FastenerSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the reinforcement (`Connection/reinforcement#REINFORCEMENT_FAMILY`) and hanger (`Connection/hanger#HANGER_FAMILY`) families drive — a bolt pattern places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. The `anchor` member type is a `FastenerKind` arm folded INSIDE this vocabulary, NEVER a separate `ConnectionFamily` sibling: the `ConnectionFamily` axis is CLOSED at four (reinforcement/fastener/hanger/joint), so a cast-in anchor bolt and a post-installed expansion anchor are `FastenerKind.Anchor` rows that key and serialize the same way a structural bolt does. The fastener vocabulary grows by data — a new property class is one `FastenerGrade` row, a new thread size one `ThreadSize` row, a new designation one `FastenerRow` catalogue entry — never a per-bolt type. The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every thread/length/head column, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel`/`metal.iron`) appearance column each row carries (the `Profile.AppearanceId` mirror, `graph.md` line 212), and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here (`properties.md` line 12) — the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa` axis is the spec-nominal grade band, the measured capacity the property-library receipt; the reinforcement and hanger families land their own sibling vocabularies on `Connection/reinforcement#REINFORCEMENT_FAMILY` and `Connection/hanger#HANGER_FAMILY`.

## [01]-[INDEX]

- [01]-[FASTENER_FAMILY]: the `FastenerKind` member-type discriminant, the `FastenerStandard` spec discriminant, the `FastenerGrade` proof/tensile axis, the `ThreadSize` nominal-thread axis, the `FastenerSection` thread receipt, and the `ConnectionCatalogue.BuildFastenerRows` ISO 898-1 / SAE J429 row table.

## [02]-[FASTENER_FAMILY]

- Owner: the fastener vocabulary (`FastenerKind` the bolt/nut/screw/anchor member-type discriminant, `FastenerStandard` the ISO 898-1 / SAE J429 spec discriminant, `FastenerGrade` the proof/tensile strength axis, `ThreadSize` the M6..M36 / 1/4in..1-1/2in nominal-thread axis, `FastenerSection` the thread receipt); `ConnectionCatalogue.BuildFastenerRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `FastenerSection.TensileStressAreaMm2` projection emitting the ISO 898-1 stress area the bolt-capacity seam reads.
- Cases: kind {bolt (externally threaded, headed) · nut (internally threaded mate) · screw (self-driven, no nut) · anchor (cast-in or post-installed, `PredefinedType=ANCHORBOLT` at the wire)} · standard {ISO 898-1 (metric property class) · SAE J429 (inch grade) · ASTM F3125 (structural-bolt, A325/A490 lineage)} · grade {ISO 898-1 cls 4.6 / 5.8 / 8.8 / 10.9 / 12.9 · SAE J429 Gr2 / Gr5 / Gr8 · ASTM F3125 A325 / A490} · size {metric coarse M6..M36 · imperial UNC 1/4in..1-1/2in} — a fastener is a `ConnectionItem` row over one `FastenerKind`, one `FastenerGrade`, and one `ThreadSize`, never a fastener subtype.
- Entry: `public double TensileStressAreaMm2 { get; }` on `FastenerSection` — the ISO 898-1 nominal tensile stress area `As = (π/4)·((d2+d3)/2)²` projected from the pitch/minor diameter the `ThreadSize` carries, the area the `FastenerGrade.ProofStressMpa` multiplies into `ProofLoadKn`/`TensileLoadKn` the structural-design seam reads (a derived `double` projection over the stored `PositiveMagnitude` columns, never a re-minted dimension primitive, exactly as `RebarSection.YieldForceKn` projects, `reinforcement.md` line 96); `ConnectionCatalogue.BuildFastenerRows(context)` folds the ISO/SAE `FastenerRow` table through `FastenerOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBoltBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` for the thread-diameter/pitch/shank/head/stress-area length columns, never an int-backed `Dimension` that truncates a fractional millimeter pitch), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the kind/standard/grade/size axes with the generated total `Switch`, `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the fastener vocabulary grows by data — a new property class is one `FastenerGrade` row carrying its proof/tensile/yield, a new thread size one `ThreadSize` row carrying its major-diameter/pitch/minor-diameter, a new member type one `FastenerKind` row, a new designation one `FastenerRow` catalogue entry — never a per-bolt type, never a per-grade `ConnectionItem` variant. `anchor` is a `FastenerKind` arm folded INSIDE this vocabulary, so the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint), never a fifth sibling family or a per-bolt type; a reinforcement/hanger/joint family lands its own vocabulary on its own page the way fastener carries `FastenerKind`/`FastenerGrade`/`FastenerSection`.
- Boundary: the fastener vocabulary is a realized `ConnectionFamily` — a per-bolt `Bolt`/`Nut`/`Screw`/`Anchor` class is the deleted form, collapsed into the one `FastenerKind` `[SmartEnum]` arm (the 3+-parallel-item-shape collapse trigger), and the cast-in/post-installed anchor is the `FastenerKind.Anchor` row, NEVER a separate `ConnectionFamily` case; `FastenerSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional ISO thread (`M12` major `12.000 mm`, pitch `1.75 mm`; `3/8in` major `9.525 mm`, 16-TPI pitch `1.5875 mm`) admits without the truncation an int-backed `Dimension` count would force, the `Dimension` carrier reserved for discrete counts (thread starts, grip plies); the ISO 898-1 tensile stress area is the `FastenerSection.TensileStressAreaMm2` derived `double` projection (over the stored `PositiveMagnitude` thread columns) the `IfcMechanicalFastener` wire and the structural-bolt seam read, NOT a host computation; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for alloy-steel cls 8.8/10.9/12.9 and SAE Gr5/Gr8, `metal.iron` for low-carbon cls 4.6/SAE Gr2) the row's `ConnectionItem.AppearanceId` column carries, never a fastener-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa` axis is the spec-nominal class band and the measured capacity is the property-library receipt; `ConnectionCatalogue.BuildFastenerRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the ISO 898-1 / SAE J429 rows keyed `connection.<designation>` (`connection.bolt-m12-88`, `connection.bolt-0375-gr5`, `connection.anchor-m16-88`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a bolt pattern reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel fastener-layout owner; the fastener serializes to the IFC 4.3 `IfcMechanicalFastener`/`IfcFastener` element at the `Rasm.Bim` boundary (portable scalar data here, never an interior `IfcOpenShell` evaluation).

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerKind {
    public static readonly FastenerKind Bolt   = new("bolt",   ifcPredefinedType: "BOLT");
    public static readonly FastenerKind Nut     = new("nut",     ifcPredefinedType: "NUT");
    public static readonly FastenerKind Screw   = new("screw",   ifcPredefinedType: "SCREW");
    public static readonly FastenerKind Anchor  = new("anchor",  ifcPredefinedType: "ANCHORBOLT");
    public string IfcPredefinedType { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerStandard {
    public static readonly FastenerStandard Iso898   = new("iso-898-1", metric: true,  authority: "ISO 898-1");
    public static readonly FastenerStandard SaeJ429  = new("sae-j429",  metric: false, authority: "SAE J429");
    public static readonly FastenerStandard AstmF3125 = new("astm-f3125", metric: false, authority: "ASTM F3125/F3125M");
    public bool Metric { get; }
    public string Authority { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FastenerGrade {
    public static readonly FastenerGrade Cls46  = new("4.6",  proofStressMpa: 225.0,  tensileStrengthMpa: 400.0,  minimumYieldMpa: 240.0,  standard: FastenerStandard.Iso898,    appearanceId: "metal.iron");
    public static readonly FastenerGrade Cls58  = new("5.8",  proofStressMpa: 380.0,  tensileStrengthMpa: 500.0,  minimumYieldMpa: 400.0,  standard: FastenerStandard.Iso898,    appearanceId: "metal.iron");
    public static readonly FastenerGrade Cls88  = new("8.8",  proofStressMpa: 600.0,  tensileStrengthMpa: 800.0,  minimumYieldMpa: 640.0,  standard: FastenerStandard.Iso898,    appearanceId: "metal.steel");
    public static readonly FastenerGrade Cls109 = new("10.9", proofStressMpa: 830.0,  tensileStrengthMpa: 1040.0, minimumYieldMpa: 940.0,  standard: FastenerStandard.Iso898,    appearanceId: "metal.steel");
    public static readonly FastenerGrade Cls129 = new("12.9", proofStressMpa: 970.0,  tensileStrengthMpa: 1220.0, minimumYieldMpa: 1100.0, standard: FastenerStandard.Iso898,    appearanceId: "metal.steel");
    public static readonly FastenerGrade Gr2    = new("gr2",  proofStressMpa: 379.0,  tensileStrengthMpa: 510.0,  minimumYieldMpa: 393.0,  standard: FastenerStandard.SaeJ429,   appearanceId: "metal.iron");
    public static readonly FastenerGrade Gr5    = new("gr5",  proofStressMpa: 585.0,  tensileStrengthMpa: 827.0,  minimumYieldMpa: 634.0,  standard: FastenerStandard.SaeJ429,   appearanceId: "metal.steel");
    public static readonly FastenerGrade Gr8    = new("gr8",  proofStressMpa: 830.0,  tensileStrengthMpa: 1034.0, minimumYieldMpa: 896.0,  standard: FastenerStandard.SaeJ429,   appearanceId: "metal.steel");
    public static readonly FastenerGrade A325   = new("a325", proofStressMpa: 585.0,  tensileStrengthMpa: 827.0,  minimumYieldMpa: 634.0,  standard: FastenerStandard.AstmF3125, appearanceId: "metal.steel");
    public static readonly FastenerGrade A490   = new("a490", proofStressMpa: 830.0,  tensileStrengthMpa: 1040.0, minimumYieldMpa: 940.0,  standard: FastenerStandard.AstmF3125, appearanceId: "metal.steel");
    public double ProofStressMpa { get; }
    public double TensileStrengthMpa { get; }
    public double MinimumYieldMpa { get; }
    public FastenerStandard Standard { get; }
    public string AppearanceId { get; }
    public bool Metric => Standard.Metric;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ThreadSize {
    public static readonly ThreadSize M6   = new("m6",    majorDiameterMm: 6.000,  pitchMm: 1.000,   minorDiameterMm: 4.773,  acrossFlatsMm: 10.000);
    public static readonly ThreadSize M8   = new("m8",    majorDiameterMm: 8.000,  pitchMm: 1.250,   minorDiameterMm: 6.466,  acrossFlatsMm: 13.000);
    public static readonly ThreadSize M10  = new("m10",   majorDiameterMm: 10.000, pitchMm: 1.500,   minorDiameterMm: 8.160,  acrossFlatsMm: 16.000);
    public static readonly ThreadSize M12  = new("m12",   majorDiameterMm: 12.000, pitchMm: 1.750,   minorDiameterMm: 9.853,  acrossFlatsMm: 18.000);
    public static readonly ThreadSize M16  = new("m16",   majorDiameterMm: 16.000, pitchMm: 2.000,   minorDiameterMm: 13.546, acrossFlatsMm: 24.000);
    public static readonly ThreadSize M20  = new("m20",   majorDiameterMm: 20.000, pitchMm: 2.500,   minorDiameterMm: 16.933, acrossFlatsMm: 30.000);
    public static readonly ThreadSize M24  = new("m24",   majorDiameterMm: 24.000, pitchMm: 3.000,   minorDiameterMm: 20.319, acrossFlatsMm: 36.000);
    public static readonly ThreadSize M30  = new("m30",   majorDiameterMm: 30.000, pitchMm: 3.500,   minorDiameterMm: 25.706, acrossFlatsMm: 46.000);
    public static readonly ThreadSize M36  = new("m36",   majorDiameterMm: 36.000, pitchMm: 4.000,   minorDiameterMm: 31.093, acrossFlatsMm: 55.000);
    public static readonly ThreadSize In025 = new("1/4",  majorDiameterMm: 6.350,  pitchMm: 1.2700,  minorDiameterMm: 4.976,  acrossFlatsMm: 11.113);
    public static readonly ThreadSize In037 = new("3/8",  majorDiameterMm: 9.525,  pitchMm: 1.5875,  minorDiameterMm: 7.722,  acrossFlatsMm: 14.288);
    public static readonly ThreadSize In050 = new("1/2",  majorDiameterMm: 12.700, pitchMm: 1.9538,  minorDiameterMm: 10.450, acrossFlatsMm: 19.050);
    public static readonly ThreadSize In062 = new("5/8",  majorDiameterMm: 15.875, pitchMm: 2.3091,  minorDiameterMm: 13.157, acrossFlatsMm: 23.813);
    public static readonly ThreadSize In075 = new("3/4",  majorDiameterMm: 19.050, pitchMm: 2.5400,  minorDiameterMm: 16.090, acrossFlatsMm: 28.575);
    public static readonly ThreadSize In100 = new("1",    majorDiameterMm: 25.400, pitchMm: 3.1750,  minorDiameterMm: 21.557, acrossFlatsMm: 38.100);
    public static readonly ThreadSize In150 = new("1-1/2", majorDiameterMm: 38.100, pitchMm: 4.2333, minorDiameterMm: 32.539, acrossFlatsMm: 57.150);
    public double MajorDiameterMm { get; }
    public double PitchMm { get; }
    public double MinorDiameterMm { get; }
    public double AcrossFlatsMm { get; }
    public double PitchDiameterMm => MajorDiameterMm - 0.649519 * PitchMm;
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct FastenerSection(
    FastenerKind Kind,
    FastenerGrade Grade,
    ThreadSize Size,
    PositiveMagnitude ThreadDiameterMm,
    PositiveMagnitude PitchMm,
    PositiveMagnitude ShankLengthMm,
    PositiveMagnitude AcrossFlatsMm) {

    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);

    public double TensileStressAreaMm2 => Math.PI / 16.0 * Math.Pow(Size.PitchDiameterMm + Size.MinorDiameterMm, 2.0);
    public double ProofLoadKn => Grade.ProofStressMpa * TensileStressAreaMm2 * 1e-3;
    public double TensileLoadKn => Grade.TensileStrengthMpa * TensileStressAreaMm2 * 1e-3;
}

public readonly record struct FastenerRow(string Designation, string Kind, string Size, string Grade, double ShankLengthMm);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    static readonly Seq<FastenerRow> FastenerRows = Seq(
        new FastenerRow("connection.bolt-m12-88",   "bolt",   "m12", "8.8",  60.0),
        new FastenerRow("connection.bolt-m16-88",   "bolt",   "m16", "8.8",  80.0),
        new FastenerRow("connection.bolt-m16-109",  "bolt",   "m16", "10.9", 80.0),
        new FastenerRow("connection.bolt-m20-88",   "bolt",   "m20", "8.8",  90.0),
        new FastenerRow("connection.bolt-m20-109",  "bolt",   "m20", "10.9", 90.0),
        new FastenerRow("connection.bolt-m24-109",  "bolt",   "m24", "10.9", 110.0),
        new FastenerRow("connection.bolt-m30-129",  "bolt",   "m30", "12.9", 140.0),
        new FastenerRow("connection.bolt-0375-gr5", "bolt",   "3/8", "gr5",  63.5),
        new FastenerRow("connection.bolt-0500-gr5", "bolt",   "1/2", "gr5",  76.2),
        new FastenerRow("connection.bolt-0750-gr8", "bolt",   "3/4", "gr8",  101.6),
        new FastenerRow("connection.bolt-0875-a325","bolt",   "7/8", "a325", 114.3),
        new FastenerRow("connection.nut-m16-88",    "nut",    "m16", "8.8",  13.0),
        new FastenerRow("connection.nut-m20-109",   "nut",    "m20", "10.9", 16.0),
        new FastenerRow("connection.screw-m8-88",   "screw",  "m8",  "8.8",  40.0),
        new FastenerRow("connection.screw-0250-gr2","screw",  "1/4", "gr2",  31.8),
        new FastenerRow("connection.anchor-m16-88", "anchor", "m16", "8.8",  200.0),
        new FastenerRow("connection.anchor-m20-88", "anchor", "m20", "8.8",  250.0),
        new FastenerRow("connection.anchor-0750-a325","anchor","3/4", "a325", 304.8));

    static Fin<(ConnectionId Id, ConnectionItem Item)> FastenerOf(FastenerRow r, Context context, Op key) =>
        from kind in FastenerKind.TryGet(r.Kind, out FastenerKind? k) ? Fin.Succ(k!) : Fin.Fail<FastenerKind>(ConnectionFault.Designation(key, $"<unknown-fastener-kind:{r.Kind}>"))
        from size in ThreadSize.TryGet(r.Size, out ThreadSize? t) ? Fin.Succ(t!) : Fin.Fail<ThreadSize>(ConnectionFault.Designation(key, $"<unknown-thread-size:{r.Size}>"))
        from grade in FastenerGrade.TryGet(r.Grade, out FastenerGrade? g) ? Fin.Succ(g!) : Fin.Fail<FastenerGrade>(ConnectionFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from metricMatch in guard(size.MajorDiameterMm > 0.0, ConnectionFault.Grade(key, $"<degenerate-thread:{r.Size}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.MajorDiameterMm)
        from pitch in key.AcceptValidated<PositiveMagnitude>(candidate: size.PitchMm)
        from shank in key.AcceptValidated<PositiveMagnitude>(candidate: r.ShankLengthMm)
        from flats in key.AcceptValidated<PositiveMagnitude>(candidate: size.AcrossFlatsMm)
        let section = new FastenerSection(kind, grade, size, diameter, pitch, shank, flats)
        from item in ConnectionItem.Of(ConnectionFamily.Fastener, r.Designation, new ConnectionSection.Fastener(section), section.AppearanceId, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildFastenerRows(Context context) =>
        FastenerRows
            .Choose(row => FastenerOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [FASTENER_ROW_TRANSCRIPTION]: REALIZED — the ISO 898-1 metric property-class and SAE J429 / ASTM F3125 inch-grade mechanical-fastener catalogue (the M6..M36 ISO metric coarse and 1/4in..1-1/2in UNC nominal threads, the ISO 898-1 cls 4.6/5.8/8.8/10.9/12.9 proof/tensile/yield bands, the SAE J429 Gr2/Gr5/Gr8 grades, and the ASTM F3125 A325/A490 structural-bolt lineage) seeds through `ConnectionCatalogue.BuildFastenerRows(context)` over the `FastenerRow` designation/kind/size/grade/shank table, the major-diameter/pitch/shank/across-flats columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the kind/grade/size algebra realized as the fastener vocabulary; the rows are the seed the connection-layout fold consumes and a new fastener is one `FastenerRow` data addition plus, if novel, one `ThreadSize`/`FastenerGrade` row. The major-diameter/pitch/minor-diameter/across-flats values transcribe the published ISO 261 / ISO 724 metric coarse thread series (`M12` major `12.000 mm`, pitch `1.75 mm`, minor `9.853 mm`) and the ASME B1.1 Unified inch coarse series (`3/8-16` major `9.525 mm`, pitch `1.5875 mm`); the `FastenerGrade.ProofStressMpa`/`TensileStrengthMpa`/`MinimumYieldMpa` bands transcribe the ISO 898-1 Table 3 property-class minima and the SAE J429 grade minima (cls 8.8 proof `600 MPa` / tensile `800 MPa`, Gr5 proof `85 ksi ≈ 585 MPa` / tensile `120 ksi ≈ 827 MPa`); a non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`.
- [TENSILE_STRESS_AREA]: REALIZED — the `FastenerSection.TensileStressAreaMm2` derived `double` projection is the ISO 898-1 nominal stress area `As = (π/4)·((d2+d3)/2)²` where `d2` is the pitch diameter (`ThreadSize.PitchDiameterMm = d − 0.649519·P`, the ISO 724 basic profile) and `d3` the minor diameter at the root, so a `FastenerGrade` proof/tensile band multiplies into `ProofLoadKn`/`TensileLoadKn` directly (cls 8.8 `M12`: `As ≈ 84.3 mm²`, proof load `≈ 50.6 kN`). The stress area is the connection capacity column the `IfcMechanicalFastener` wire and the structural-bolt seam read; it is NOT re-derived at the `properties#MATERIAL_PROPERTY_CATALOGUE` boundary — the `MaterialId`-keyed `Mechanical` row carries the measured `YieldStrengthMpa`/`YoungsModulusMpa`, the spec-nominal grade band carries the catalogue proof/tensile, and the two meet at the design seam.
- [IFC_FASTENER_WIRE]: a mechanical fastener round-trips to IFC 4.3 as an `IfcMechanicalFastener` (`PredefinedType` ∈ {ANCHORBOLT, BOLT, DOWEL, NAIL, NAILPLATE, RIVET, SCREW, SHEARCONNECTOR, STAPLE, STUDSHEARCONNECTOR, USERDEFINED, NOTDEFINED}) carrying `NominalDiameter` (the `FastenerSection.ThreadDiameterMm`) and `NominalLength` (the `ShankLengthMm`); the `FastenerKind.IfcPredefinedType` column maps each member type to its `PredefinedType` arm — `bolt→BOLT`, `nut→USERDEFINED` (a nut is an `IfcDiscreteAccessory` companion on the bolt, the `NUT` literal a project property), `screw→SCREW`, `anchor→ANCHORBOLT` (the cast-in / post-installed anchor the fold flags). A non-mechanical or fully proprietary fastener crosses as an `IfcFastener` (`PredefinedType` ∈ {GLUE, MORTAR, WELD, USERDEFINED, NOTDEFINED}) over the same `FastenerSection` columns; the structural property set `Pset_FastenerMechanical` (the proof/tensile load the `TensileStressAreaMm2`×`FastenerGrade` projection asserts) is the `MaterialId`-keyed `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` read at the federation. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the scalar columns the wire reads, never an IFC entity.
