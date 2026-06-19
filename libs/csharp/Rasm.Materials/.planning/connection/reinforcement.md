# [MATERIALS_REINFORCEMENT]

THE FIRST REALIZED CONNECTIONFAMILY. The reinforcement vocabulary — the `RebarGrade` yield-strength axis, the `BarSize` nominal-bar axis, the `RebarSection` cross-section receipt (nominal bar diameter / cross-sectional area / unit weight), and the ACI 318 standard bend/hook geometry carried as a scalar bend-angle/radius/hook-extension tuple — is the realized reinforcing-bar vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Reinforcement` case. A #5 rebar is a `ConnectionItem` row, never a `Rebar` type: the bar size, the grade, the section receipt, and the bend template are reinforcement-`ConnectionItem` columns, and the `RebarSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the fastener and hanger families drive — a reinforcing schedule places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. The reinforcement vocabulary grows by data — a new bar size is one `BarSize` row, a new grade one `RebarGrade` row, a new designation one `RebarRow` catalogue entry — never a per-bar type. The bend geometry is a scalar bend-angle/radius/hook-extension tuple the host materializes into a curve, NEVER a host curve here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps — the resolved layout is a `Seq<Element>` of scalar `Placement` tuples the host boundary materializes). The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every length/area/weight column, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.iron`/`metal.steel`) appearance column each row carries, and the `physical-properties#MATERIAL_PROPERTY` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here; the fastener and hanger families land their own sibling vocabularies on `Connection/fastener#FASTENER_FAMILY` and `Connection/hanger#HANGER_FAMILY`.

## [1]-[INDEX]

- [1]-[REINFORCEMENT_FAMILY]: the `RebarGrade` yield-strength axis, the `BarSize` nominal-bar axis, the `RebarBend` ACI 318 bend/hook scalar tuple, the `RebarSection` cross-section receipt, and the `ConnectionCatalogue.BuildRebarRows` ASTM A615/A706 row table.

## [2]-[REINFORCEMENT_FAMILY]

- Owner: the reinforcement vocabulary (`RebarGrade` the ASTM A615/A706 yield-strength axis, `BarSize` the #3..#18 / 10M..55M nominal-bar axis, `RebarStandard` the spec discriminant, `RebarBend` the ACI 318 bend/hook scalar tuple, `RebarSection` the cross-section receipt); `ConnectionCatalogue.BuildRebarRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `RebarSection.StandardHook` projection emitting the ACI 318 hook geometry the host materializes.
- Cases: grade {A615 Gr40 / Gr60 / Gr75 / Gr80 (carbon-steel, non-weldable above Gr60 without supplemental S1) · A706 Gr60 / Gr80 (low-alloy, weldable) · CSA G30.18 400W / 500W (metric, weldable)} · size {imperial #3..#11, #14, #18 · metric 10M..55M} · hook {ninety / one-thirty-five / one-eighty standard bend angles, the ACI 318 Table 25.3.1/25.3.2 minimum-bend-diameter and hook-extension rule} — a bar is a `ConnectionItem` row over one `RebarGrade` and one `BarSize`, never a bar subtype.
- Entry: `public Fin<RebarBend> StandardHook(RebarHook hook, Op key)` on `RebarSection` — the ACI 318 standard-hook projection resolving the bend angle, the minimum inside bend diameter (a `BarSize`-dependent multiple of the bar diameter), and the straight hook extension into the scalar `RebarBend` tuple the host curve-materializes; `ConnectionCatalogue.BuildRebarRows(context)` folds the A615/A706 `RebarRow` table through `RebarOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetBarBySize`/`GetByGrade` family.
- Packages: Rasm (project — `PositiveMagnitude` for the bar-diameter/area/weight/bend-geometry length columns, never an int-backed `Dimension` that truncates a fractional millimeter), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the grade/size/hook axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the reinforcement vocabulary grows by data — a new bar size is one `BarSize` row carrying its nominal diameter/area/weight, a new grade one `RebarGrade` row carrying its minimum yield, a new designation one `RebarRow` catalogue entry, a new hook one `RebarHook` row — never a per-bar type, never a per-grade `ConnectionItem` variant. A fastener/hanger family lands its own vocabulary on its own page the way reinforcement carries `RebarGrade`/`BarSize`/`RebarSection`; `anchor` folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, so the `ConnectionFamily` axis stays CLOSED at three (reinforcement/fastener/hanger), never a fourth sibling family or a per-bar type.
- Boundary: the reinforcement vocabulary is the first realized `ConnectionFamily` — a per-bar `Rebar` class is the deleted form; `RebarSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every column so the section never re-mints a length primitive and a fractional ASTM nominal diameter (`#5 = 15.875 mm`, `10M = 11.3 mm`) admits without the truncation an int-backed `Dimension` count would force, the `NominalAreaMm2` and `UnitWeightKgM` columns carried as `PositiveMagnitude` receipts the `IfcReinforcingBar` wire and the structural-design seam read; the ACI 318 bend geometry is the scalar `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm)` tuple `StandardHook` emits — NEVER a host `Curve`, the host boundary lofts the bar centerline and the bend arc from the scalar tuple exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.iron` for plain carbon A615, `metal.steel` for low-alloy A706) the row's `ConnectionItem.AppearanceId` column carries, never a reinforcement-specific shade; the mechanical capacity crosses to `physical-properties#MATERIAL_PROPERTY` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `RebarGrade.MinimumYieldMpa` axis is the spec-nominal grade band and the measured capacity is the property-library receipt; `ConnectionCatalogue.BuildRebarRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the A615/A706/G30.18 rows keyed `connection.<designation>` (`connection.rebar-no5-gr60`, `connection.rebar-10m-400w`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a rebar schedule reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel reinforcement-layout owner.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class RebarStandard {
    public static readonly RebarStandard A615 = new("astm-a615", weldable: false, authority: "ASTM A615/A615M");
    public static readonly RebarStandard A706 = new("astm-a706", weldable: true, authority: "ASTM A706/A706M");
    public static readonly RebarStandard G30 = new("csa-g30.18", weldable: true, authority: "CSA G30.18");
    public bool Weldable { get; }
    public string Authority { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class RebarGrade {
    public static readonly RebarGrade Gr40  = new("gr40",  minimumYieldMpa: 280.0, standard: RebarStandard.A615, appearanceId: "metal.iron");
    public static readonly RebarGrade Gr60  = new("gr60",  minimumYieldMpa: 420.0, standard: RebarStandard.A615, appearanceId: "metal.iron");
    public static readonly RebarGrade Gr75  = new("gr75",  minimumYieldMpa: 520.0, standard: RebarStandard.A615, appearanceId: "metal.iron");
    public static readonly RebarGrade Gr80  = new("gr80",  minimumYieldMpa: 550.0, standard: RebarStandard.A615, appearanceId: "metal.iron");
    public static readonly RebarGrade Gr60W = new("gr60w", minimumYieldMpa: 420.0, standard: RebarStandard.A706, appearanceId: "metal.steel");
    public static readonly RebarGrade Gr80W = new("gr80w", minimumYieldMpa: 550.0, standard: RebarStandard.A706, appearanceId: "metal.steel");
    public static readonly RebarGrade G400W = new("400w",  minimumYieldMpa: 400.0, standard: RebarStandard.G30,  appearanceId: "metal.steel");
    public static readonly RebarGrade G500W = new("500w",  minimumYieldMpa: 500.0, standard: RebarStandard.G30,  appearanceId: "metal.steel");
    public double MinimumYieldMpa { get; }
    public RebarStandard Standard { get; }
    public string AppearanceId { get; }
    public bool Weldable => Standard.Weldable;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class BarSize {
    public static readonly BarSize No3  = new("#3",  nominalDiameterMm: 9.525,  nominalAreaMm2: 71.0,   unitWeightKgM: 0.560,  bendFactor: 6.0);
    public static readonly BarSize No4  = new("#4",  nominalDiameterMm: 12.700, nominalAreaMm2: 129.0,  unitWeightKgM: 0.994,  bendFactor: 6.0);
    public static readonly BarSize No5  = new("#5",  nominalDiameterMm: 15.875, nominalAreaMm2: 199.0,  unitWeightKgM: 1.552,  bendFactor: 6.0);
    public static readonly BarSize No6  = new("#6",  nominalDiameterMm: 19.050, nominalAreaMm2: 284.0,  unitWeightKgM: 2.235,  bendFactor: 6.0);
    public static readonly BarSize No7  = new("#7",  nominalDiameterMm: 22.225, nominalAreaMm2: 387.0,  unitWeightKgM: 3.042,  bendFactor: 6.0);
    public static readonly BarSize No8  = new("#8",  nominalDiameterMm: 25.400, nominalAreaMm2: 510.0,  unitWeightKgM: 3.973,  bendFactor: 6.0);
    public static readonly BarSize No9  = new("#9",  nominalDiameterMm: 28.651, nominalAreaMm2: 645.0,  unitWeightKgM: 5.060,  bendFactor: 8.0);
    public static readonly BarSize No10 = new("#10", nominalDiameterMm: 32.258, nominalAreaMm2: 819.0,  unitWeightKgM: 6.404,  bendFactor: 8.0);
    public static readonly BarSize No11 = new("#11", nominalDiameterMm: 35.814, nominalAreaMm2: 1006.0, unitWeightKgM: 7.907,  bendFactor: 8.0);
    public static readonly BarSize No14 = new("#14", nominalDiameterMm: 43.002, nominalAreaMm2: 1452.0, unitWeightKgM: 11.380, bendFactor: 10.0);
    public static readonly BarSize No18 = new("#18", nominalDiameterMm: 57.328, nominalAreaMm2: 2581.0, unitWeightKgM: 20.240, bendFactor: 10.0);
    public static readonly BarSize M10  = new("10M", nominalDiameterMm: 11.300, nominalAreaMm2: 100.0,  unitWeightKgM: 0.785,  bendFactor: 6.0);
    public static readonly BarSize M15  = new("15M", nominalDiameterMm: 16.000, nominalAreaMm2: 200.0,  unitWeightKgM: 1.570,  bendFactor: 6.0);
    public static readonly BarSize M20  = new("20M", nominalDiameterMm: 19.500, nominalAreaMm2: 300.0,  unitWeightKgM: 2.355,  bendFactor: 6.0);
    public static readonly BarSize M25  = new("25M", nominalDiameterMm: 25.200, nominalAreaMm2: 500.0,  unitWeightKgM: 3.925,  bendFactor: 6.0);
    public static readonly BarSize M30  = new("30M", nominalDiameterMm: 29.900, nominalAreaMm2: 700.0,  unitWeightKgM: 5.495,  bendFactor: 8.0);
    public static readonly BarSize M35  = new("35M", nominalDiameterMm: 35.700, nominalAreaMm2: 1000.0, unitWeightKgM: 7.850,  bendFactor: 8.0);
    public static readonly BarSize M45  = new("45M", nominalDiameterMm: 43.700, nominalAreaMm2: 1500.0, unitWeightKgM: 11.775, bendFactor: 10.0);
    public static readonly BarSize M55  = new("55M", nominalDiameterMm: 56.400, nominalAreaMm2: 2500.0, unitWeightKgM: 19.625, bendFactor: 10.0);
    public double NominalDiameterMm { get; }
    public double NominalAreaMm2 { get; }
    public double UnitWeightKgM { get; }
    public double BendFactor { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class RebarHook {
    public static readonly RebarHook Ninety       = new("90", bendDegrees: 90.0,  extensionFactor: 12.0);
    public static readonly RebarHook OneThirtyFive = new("135", bendDegrees: 135.0, extensionFactor: 6.0);
    public static readonly RebarHook OneEighty     = new("180", bendDegrees: 180.0, extensionFactor: 4.0);
    public double BendDegrees { get; }
    public double ExtensionFactor { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct RebarBend(double BendDegrees, double InsideBendDiameterMm, double HookExtensionMm);

public readonly record struct RebarSection(
    BarSize Size,
    RebarGrade Grade,
    PositiveMagnitude DiameterMm,
    PositiveMagnitude NominalAreaMm2,
    PositiveMagnitude UnitWeightKgM) {

    public MaterialId AppearanceId => MaterialId.Of(Grade.AppearanceId);
    public double YieldForceKn => Grade.MinimumYieldMpa * NominalAreaMm2.Value * 1e-3;

    public Fin<RebarBend> StandardHook(RebarHook hook, Op key) =>
        from valid in guard(hook.ExtensionFactor > 0.0 && Size.BendFactor > 0.0, ConnectionFault.Capacity(key, $"<degenerate-hook:{Grade.Key}:{Size.Key}>"))
        let diameter = DiameterMm.Value
        let insideBendDiameterMm = Size.BendFactor * diameter
        let hookExtensionMm = hook.ExtensionFactor * diameter
        select new RebarBend(hook.BendDegrees, insideBendDiameterMm, hookExtensionMm);
}

public readonly record struct RebarRow(string Designation, string Size, string Grade);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    static readonly Seq<RebarRow> RebarRows = Seq(
        new RebarRow("connection.rebar-no3-gr60",   "#3",  "gr60"),
        new RebarRow("connection.rebar-no4-gr60",   "#4",  "gr60"),
        new RebarRow("connection.rebar-no5-gr60",   "#5",  "gr60"),
        new RebarRow("connection.rebar-no6-gr60",   "#6",  "gr60"),
        new RebarRow("connection.rebar-no7-gr75",   "#7",  "gr75"),
        new RebarRow("connection.rebar-no8-gr75",   "#8",  "gr75"),
        new RebarRow("connection.rebar-no9-gr80",   "#9",  "gr80"),
        new RebarRow("connection.rebar-no11-gr80",  "#11", "gr80"),
        new RebarRow("connection.rebar-no5-gr60w",  "#5",  "gr60w"),
        new RebarRow("connection.rebar-no8-gr80w",  "#8",  "gr80w"),
        new RebarRow("connection.rebar-10m-400w", "10M", "400w"),
        new RebarRow("connection.rebar-15m-400w", "15M", "400w"),
        new RebarRow("connection.rebar-25m-500w", "25M", "500w"),
        new RebarRow("connection.rebar-35m-500w", "35M", "500w"));

    static Fin<(ConnectionId Id, ConnectionItem Item)> RebarOf(RebarRow r, Context context, Op key) =>
        from size in BarSize.TryGet(r.Size, out BarSize? s) ? Fin.Succ(s!) : Fin.Fail<BarSize>(ConnectionFault.Designation(key, $"<unknown-bar-size:{r.Size}>"))
        from grade in RebarGrade.TryGet(r.Grade, out RebarGrade? g) ? Fin.Succ(g!) : Fin.Fail<RebarGrade>(ConnectionFault.Grade(key, $"<unknown-grade:{r.Grade}>"))
        from diameter in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalDiameterMm)
        from area in key.AcceptValidated<PositiveMagnitude>(candidate: size.NominalAreaMm2)
        from weight in key.AcceptValidated<PositiveMagnitude>(candidate: size.UnitWeightKgM)
        let section = new RebarSection(size, grade, diameter, area, weight)
        from item in ConnectionItem.Of(ConnectionFamily.Reinforcement, r.Designation, new ConnectionSection.Reinforcement(section), section.AppearanceId, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildRebarRows(Context context) =>
        RebarRows
            .Choose(row => RebarOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ConnectionKeyPolicy.EqualityComparer);
}
```

## [3]-[RESEARCH]

- [REBAR_ROW_TRANSCRIPTION]: REALIZED — the ASTM A615/A706 + CSA G30.18 reinforcing-bar catalogue (the #3..#18 imperial and 10M..55M metric nominal columns, the A615 Gr40/60/75/80, A706 Gr60/80 weldable, and CSA 400W/500W weldable grades) seeds through `ConnectionCatalogue.BuildRebarRows(context)` over the `RebarRow` designation/size/grade table, the nominal-diameter/area/weight columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` into the kernel value-object and the grade/size/hook algebra realized as the reinforcement vocabulary; the bar rows are the seed the connection-layout fold consumes and a new bar is one `RebarRow` data addition plus, if novel, one `BarSize`/`RebarGrade` row. The nominal diameter/area/unit-weight values transcribe the published ASTM A615/A706 (US-customary soft-metric) and CSA G30.18 (metric 10M..55M) dimensions; a non-positive column rails the `AcceptValidated` `Fin` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`. The `BarSize.BendFactor` is the ACI 318-19 Table 25.3.1 minimum-inside-bend-diameter multiple (6dᵦ for #3..#8, 8dᵦ for #9..#11, 10dᵦ for #14/#18 and the matching metric bands); the `RebarHook.ExtensionFactor` is the ACI 318-19 Table 25.3.2 standard-hook straight extension (12dᵦ for a 90° hook, 6dᵦ for 135°, 4dᵦ for 180°, the latter floored at 65 mm at the host materialization, never here).
- [REBAR_BEND_HOST_MATERIALIZATION]: the `RebarBend(BendDegrees, InsideBendDiameterMm, HookExtensionMm)` scalar tuple is the host-neutral bend receipt — the host boundary lofts the bar centerline polyline and fillets each bend by the inside-bend-diameter radius into the host `Curve`, exactly as `Construction/layout#ASSEMBLY_FOLD` materializes a `Seq<Element>` of scalar `Placement` tuples into host geometry. This owner NEVER constructs a host curve: the scalar tuple is the portable schedule data the `Rasm.Bim` wire serializes and the host plug-in materializes, so the reinforcement catalogue stays a leaf below the host boundary. A bent-bar schedule (a stirrup, a column tie, a development hook) is a `RebarBend` per bend plus a `Construction/layout` station-stepped centerline, never a per-shape rebar curve type.
- [IFC_REINFORCING_WIRE]: a reinforcing bar round-trips to IFC 4.3 as an `IfcReinforcingBar` (`PredefinedType` ∈ {MAIN, LIGATURE, STUD, PUNCHING, RING, SHEAR, EDGE, ANCHORING, SPACEBAR}) carrying `NominalDiameter` (the `RebarSection.DiameterMm`), `CrossSectionArea` (`NominalAreaMm2`), `BarLength`, `BarSurface` (PLAIN | TEXTURED), and `SteelGrade` (the `RebarGrade.Key`); a reinforcement mesh round-trips as an `IfcReinforcingMesh` (`MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarSpacing`/`TransverseBarSpacing`) over the same `RebarSection` columns. The wire mapping is the `Rasm.Bim` boundary projection (`MaterialId`-keyed to the `physical-properties#MATERIAL_PROPERTY` `Mechanical` `YieldStrengthMpa` the `SteelGrade` asserts), host-neutral here — this page emits the scalar columns the wire reads, never an IFC entity. The `RebarBend` ACI 318 hook geometry maps to the `IfcReinforcingBar` `BendingShapeCode`/`BendingParameters` (the ISO 3766 / BS 8666 schedule shape) the federation reads, the angle and bend-diameter the scalar tuple already carries.
