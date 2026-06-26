# [MATERIALS_HANGER]

THE FRAMING-CONNECTOR CONNECTIONFAMILY. The hanger vocabulary — the `HangerType` framing-connector discriminant (joist hanger · framing angle · strap · hold-down), the `SteelGauge` sheet-steel gauge axis, and the `HangerSection` cold-formed receipt (the carried-member nominal size, the allowable download/uplift/lateral capacity, the base-steel gauge and thickness) — is the realized framing-connector vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Hanger` case. A joist hanger is a `ConnectionItem` row, never a `Hanger` type: the connector type, the gauge, the carried-member fit, and the allowable-load receipt are hanger-`ConnectionItem` columns, and the `HangerSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the reinforcement and fastener families drive — a connector schedule places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. The hanger vocabulary grows by data — a new connector is one `ConnectionCatalogue` `HangerRow` entry, a new gauge one `SteelGauge` row, a new connector class one `HangerType` row — never a per-connector type. A cold-formed framing connector is a host-neutral capacity receipt the host materializes into a stamped-plate solid, NEVER a host brep here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps — the resolved layout is a `Seq<Element>` of scalar `Placement` tuples the host boundary materializes). The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every member-size/capacity/thickness column and `Dimension` for the discrete gauge designation, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel` — galvanized cold-formed steel) appearance column each row carries, and the `properties#MATERIAL_PROPERTY` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here; the reinforcement and fastener families land their own sibling vocabularies on `Connection/reinforcement#REINFORCEMENT_FAMILY` and `Connection/fastener#FASTENER_FAMILY`, and the `anchor` connector folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, never a separate family — the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint).

## [01]-[INDEX]

- [01]-[HANGER_FAMILY]: the `HangerType` framing-connector discriminant, the `SteelGauge` sheet-steel gauge axis, the `HangerSection` cold-formed capacity receipt (carried-member size / allowable download-uplift-lateral / gauge and base thickness), and the `ConnectionCatalogue.BuildHangerRows` framing-connector row table.

## [02]-[HANGER_FAMILY]

- Owner: the hanger vocabulary (`HangerType` the joist-hanger/framing-angle/strap/hold-down framing-connector discriminant, `SteelGauge` the 18/16/14/12/10 ga sheet-steel base-metal axis, `HangerInstall` the nailed/screwed/bolted attachment discriminant, `HangerSection` the cold-formed allowable-load receipt); `ConnectionCatalogue.BuildHangerRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `HangerSection.GovernedCapacity` projection emitting the load-duration-adjusted allowable the design seam reads.
- Cases: type {joist-hanger (face-mount/top-flange carried-member saddle) · framing-angle (the L-bend tension/shear clip) · strap (the flat tension tie / coiled strap) · hold-down (the shear-wall tension anchor)} · gauge {18 ga (1.214 mm) · 16 ga (1.519 mm) · 14 ga (1.897 mm) · 12 ga (2.657 mm) · 10 ga (3.416 mm) base-metal thickness, the manufacturer-published cold-formed designation} · install {nailed (10d/16d common, the SDS/SD structural-connector screw) · bolted} — a connector is a `ConnectionItem` row over one `HangerType` and one `SteelGauge`, never a connector subtype.
- Entry: `public Fin<HangerCapacity> GovernedCapacity(LoadDuration duration, Op key)` on `HangerSection` — the allowable-load projection resolving the governing download/uplift/lateral allowable scaled by the wood-connector load-duration factor (`Cd`) into the scalar `HangerCapacity` receipt the structural-connection-design seam reads, `Fin<T>` railing a degenerate (non-positive) allowable through `ConnectionFault.Capacity`; `ConnectionCatalogue.BuildHangerRows(context)` folds the framing-connector `HangerRow` table through `HangerOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetHangerByType`/`GetByGauge` family.
- Packages: Rasm (project — `PositiveMagnitude` for the carried-member-size/allowable-load/base-thickness columns, never an int-backed `Dimension` that truncates a fractional capacity; `Dimension` for the discrete sheet-steel gauge designation), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the type/gauge/install axes with the generated total `Switch`, `[KeyMemberEqualityComparer]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`).
- Growth: the hanger vocabulary grows by data — a new connector is one `HangerRow` catalogue entry carrying its type/gauge/carried-member/allowable columns, a new gauge one `SteelGauge` row carrying its base-metal thickness, a new connector class one `HangerType` row, a new attachment one `HangerInstall` row — never a per-connector type, never a per-type `ConnectionItem` variant. The reinforcement/fastener/joint families carry their own vocabularies on their own pages the way hanger carries `HangerType`/`SteelGauge`/`HangerSection`; `anchor` folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, so the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint), never a fifth sibling family or a per-connector type.
- Boundary: the hanger vocabulary is the framing-connector `ConnectionFamily` — a per-connector `Hanger` class is the deleted form; `HangerSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every length/load column so the section never re-mints a primitive and a fractional cold-formed base thickness (`18 ga = 1.214 mm`, `12 ga = 2.657 mm`) and a fractional published allowable (`download = 7.83 kN`) admit without the truncation an int-backed `Dimension` count would force, the discrete sheet-steel gauge number admitting as the `Dimension` count carrier so a gauge is a designation never a length; the framing connector is a host-neutral capacity receipt — `HangerSection` carries the carried-member nominal fit, the allowable download/uplift/lateral loads, and the base gauge/thickness as `PositiveMagnitude` columns the `IfcMechanicalFastener` wire and the structural-design seam read, NEVER a host brep, the host boundary materializes the stamped-plate solid from the scalar columns exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the allowable-load receipt is the published manufacturer-evaluated capacity scaled by the wood-connector load-duration factor at `GovernedCapacity`, NOT a re-derived structural calculation — the design seam reads the `HangerCapacity` allowable the manufacturer evaluation report (ICC-ES / manufacturer catalogue) certifies, never a connection re-analysis; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for galvanized cold-formed steel) the row's `ConnectionItem.AppearanceId` column carries, never a hanger-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `SteelGauge` base-metal band is the cold-formed designation and the measured grade is the property-library receipt; `ConnectionCatalogue.BuildHangerRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the framing-connector rows keyed `connection.<designation>` (`connection.hanger-jh-2x8-18ga`, `connection.holdown-hd-12ga`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` and the gauge through `key.AcceptValidated<Dimension>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a connector schedule reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel hanger-layout owner.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class HangerType {
    public static readonly HangerType JoistHanger  = new("joist-hanger",  ifcDesignation: "joist-hanger", carriesMember: true);
    public static readonly HangerType FramingAngle = new("framing-angle", ifcDesignation: "framing-angle", carriesMember: false);
    public static readonly HangerType Strap        = new("strap",         ifcDesignation: "strap-tie",     carriesMember: false);
    public static readonly HangerType HoldDown     = new("hold-down",     ifcDesignation: "hold-down",     carriesMember: false);
    public string IfcDesignation { get; }
    public bool CarriesMember { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class SteelGauge {
    public static readonly SteelGauge Ga18 = new("18ga", gaugeNumber: 18, baseThicknessMm: 1.214);
    public static readonly SteelGauge Ga16 = new("16ga", gaugeNumber: 16, baseThicknessMm: 1.519);
    public static readonly SteelGauge Ga14 = new("14ga", gaugeNumber: 14, baseThicknessMm: 1.897);
    public static readonly SteelGauge Ga12 = new("12ga", gaugeNumber: 12, baseThicknessMm: 2.657);
    public static readonly SteelGauge Ga10 = new("10ga", gaugeNumber: 10, baseThicknessMm: 3.416);
    public int GaugeNumber { get; }
    public double BaseThicknessMm { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class HangerInstall {
    public static readonly HangerInstall Nailed  = new("nailed",  durationSensitive: true);
    public static readonly HangerInstall Screwed = new("screwed", durationSensitive: true);
    public static readonly HangerInstall Bolted  = new("bolted",  durationSensitive: false);
    public bool DurationSensitive { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ConnectionKeyPolicy, string>]
public sealed partial class LoadDuration {
    public static readonly LoadDuration Permanent = new("permanent", cd: 0.90);
    public static readonly LoadDuration TenYear   = new("ten-year",  cd: 1.00);
    public static readonly LoadDuration TwoMonth  = new("two-month", cd: 1.15);
    public static readonly LoadDuration SevenDay  = new("seven-day", cd: 1.25);
    public static readonly LoadDuration TenMinute = new("ten-minute", cd: 1.60);
    public static readonly LoadDuration Impact    = new("impact",    cd: 2.00);
    public double Cd { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct HangerCapacity(double DownloadKn, double UpliftKn, double LateralKn, double Cd);

public readonly record struct HangerSection(
    HangerType Type,
    SteelGauge Gauge,
    HangerInstall Install,
    Dimension GaugeNumber,
    PositiveMagnitude CarriedMemberWidthMm,
    PositiveMagnitude CarriedMemberDepthMm,
    PositiveMagnitude BaseThicknessMm,
    PositiveMagnitude AllowableDownloadKn,
    PositiveMagnitude AllowableUpliftKn,
    PositiveMagnitude AllowableLateralKn) {

    public MaterialId AppearanceId => MaterialId.Of("metal.steel");
    public double GoverningAllowableKn => Math.Min(AllowableDownloadKn.Value, Math.Min(AllowableUpliftKn.Value, AllowableLateralKn.Value));

    public Fin<HangerCapacity> GovernedCapacity(LoadDuration duration, Op key) =>
        from valid in guard(AllowableDownloadKn.Value > 0.0 && AllowableUpliftKn.Value > 0.0 && AllowableLateralKn.Value > 0.0, ConnectionFault.Capacity(key, $"<degenerate-allowable:{Type.Key}:{Gauge.Key}>"))
        let cd = Install.DurationSensitive ? duration.Cd : 1.0
        select new HangerCapacity(AllowableDownloadKn.Value * cd, AllowableUpliftKn.Value * cd, AllowableLateralKn.Value * cd, cd);
}

public readonly record struct HangerRow(
    string Designation,
    string Type,
    string Gauge,
    string Install,
    double CarriedMemberWidthMm,
    double CarriedMemberDepthMm,
    double AllowableDownloadKn,
    double AllowableUpliftKn,
    double AllowableLateralKn);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    static readonly Seq<HangerRow> HangerRows = Seq(
        new HangerRow("connection.hanger-jh-2x6-18ga",  "joist-hanger",  "18ga", "nailed",  38.1,  139.7, 4.85,  0.78, 1.33),
        new HangerRow("connection.hanger-jh-2x8-18ga",  "joist-hanger",  "18ga", "nailed",  38.1,  184.2, 7.83,  0.78, 1.33),
        new HangerRow("connection.hanger-jh-2x10-16ga", "joist-hanger",  "16ga", "nailed",  38.1,  235.0, 10.45, 1.00, 1.78),
        new HangerRow("connection.hanger-jh-2x12-16ga", "joist-hanger",  "16ga", "nailed",  38.1,  285.8, 12.46, 1.00, 1.78),
        new HangerRow("connection.hanger-jh-4x8-14ga",  "joist-hanger",  "14ga", "nailed",  88.9,  184.2, 14.23, 1.33, 2.45),
        new HangerRow("connection.angle-a35-18ga",      "framing-angle", "18ga", "nailed",  0.10,  0.10,  3.96,  3.96, 2.18),
        new HangerRow("connection.angle-l70-16ga",      "framing-angle", "16ga", "nailed",  0.10,  0.10,  5.74,  5.74, 3.11),
        new HangerRow("connection.strap-cs16-16ga",     "strap",         "16ga", "nailed",  0.10,  0.10,  0.10,  9.34, 0.10),
        new HangerRow("connection.strap-cs14-14ga",     "strap",         "14ga", "nailed",  0.10,  0.10,  0.10, 14.50, 0.10),
        new HangerRow("connection.holdown-hd5b-12ga",   "hold-down",     "12ga", "bolted",  0.10,  0.10,  0.10, 23.13, 0.10),
        new HangerRow("connection.holdown-hd12-10ga",   "hold-down",     "10ga", "bolted",  0.10,  0.10,  0.10, 53.38, 0.10));

    static Fin<(ConnectionId Id, ConnectionItem Item)> HangerOf(HangerRow r, Context context, Op key) =>
        from type in HangerType.TryGet(r.Type, out HangerType? t) ? Fin.Succ(t!) : Fin.Fail<HangerType>(ConnectionFault.Designation(key, $"<unknown-hanger-type:{r.Type}>"))
        from gauge in SteelGauge.TryGet(r.Gauge, out SteelGauge? g) ? Fin.Succ(g!) : Fin.Fail<SteelGauge>(ConnectionFault.Grade(key, $"<unknown-gauge:{r.Gauge}>"))
        from install in HangerInstall.TryGet(r.Install, out HangerInstall? i) ? Fin.Succ(i!) : Fin.Fail<HangerInstall>(ConnectionFault.Designation(key, $"<unknown-install:{r.Install}>"))
        from gaugeNo in key.AcceptValidated<Dimension>(candidate: gauge.GaugeNumber)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.CarriedMemberWidthMm)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: r.CarriedMemberDepthMm)
        from thickness in key.AcceptValidated<PositiveMagnitude>(candidate: gauge.BaseThicknessMm)
        from download in key.AcceptValidated<PositiveMagnitude>(candidate: r.AllowableDownloadKn)
        from uplift in key.AcceptValidated<PositiveMagnitude>(candidate: r.AllowableUpliftKn)
        from lateral in key.AcceptValidated<PositiveMagnitude>(candidate: r.AllowableLateralKn)
        let section = new HangerSection(type, gauge, install, gaugeNo, width, depth, thickness, download, uplift, lateral)
        from item in ConnectionItem.Of(ConnectionFamily.Hanger, r.Designation, new ConnectionSection.Hanger(section), section.AppearanceId, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildHangerRows(Context context) =>
        HangerRows
            .Choose(row => HangerOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ConnectionKeyPolicy.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [HANGER_ROW_TRANSCRIPTION]: REALIZED — the cold-formed framing-connector catalogue (the joist-hanger face-mount saddles over the 2x6..4x8 carried-member sizes, the A35/L-series framing angles, the CS-series flat straps, and the HD-series shear-wall hold-downs) seeds through `ConnectionCatalogue.BuildHangerRows(context)` over the `HangerRow` designation/type/gauge/install table, the carried-member/allowable-load columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` and the discrete sheet-steel gauge through `key.AcceptValidated<Dimension>(candidate:)` into the kernel value-objects, and the type/gauge/install algebra realized as the hanger vocabulary; the connector rows are the seed the connection-layout fold consumes and a new connector is one `HangerRow` data addition plus, if novel, one `HangerType`/`SteelGauge` row. The `SteelGauge.BaseThicknessMm` columns transcribe the manufacturer-uncoated cold-formed sheet-steel base-metal thicknesses (18 ga = 1.214 mm, 16 ga = 1.519 mm, 14 ga = 1.897 mm, 12 ga = 2.657 mm, 10 ga = 3.416 mm); the `AllowableDownloadKn`/`AllowableUpliftKn`/`AllowableLateralKn` columns transcribe the published manufacturer-evaluation-report (ICC-ES) ten-year-duration allowable loads (a single-family framing-connector evaluation), the load-direction columns a connector does not resist seeded at the floor-positive `0.10` kN placeholder so the `PositiveMagnitude` rail admits the row and `GoverningAllowableKn` reads the connector's true governing direction rather than a degenerate zero; the `GovernedCapacity` fold scales the published allowable by the NDS wood-connector load-duration factor (`Cd` ∈ {0.90 permanent, 1.00 ten-year, 1.15 two-month, 1.25 seven-day, 1.60 ten-minute/wind-seismic, 2.00 impact}) for a nailed/screwed connector into wood and passes the bolted hold-down allowable through unscaled (`DurationSensitive == false`).
- [HANGER_HOST_MATERIALIZATION]: the `HangerSection` carried-member/allowable/gauge scalar columns are the host-neutral connector receipt — the host boundary materializes the stamped-and-formed steel plate solid (the saddle seat, the nail-hole grid, the formed flanges) from the scalar columns, exactly as `Construction/layout#ASSEMBLY_FOLD` materializes a `Seq<Element>` of scalar `Placement` tuples into host geometry. This owner NEVER constructs a host brep: the scalar columns are the portable schedule data the `Rasm.Bim` wire serializes and the host plug-in materializes, so the hanger catalogue stays a leaf below the host boundary. A connector schedule (a joist-to-beam saddle run, a shear-wall hold-down stack) is a `HangerSection` per connector plus a `Construction/layout` station-stepped placement, never a per-connector host solid type.
- [IFC_MECHANICAL_FASTENER_WIRE]: a framing connector round-trips to IFC 4.3 as an `IfcMechanicalFastener` carrying `NominalDiameter`/`NominalLength` (the connector envelope the carried-member fit and base gauge bound) and `PredefinedType` ∈ {ANCHORBOLT, BOLT, DOWEL, NAIL, NAILPLATE, RIVET, SCREW, SHEARCONNECTOR, STAPLE, STUDSHEARCONNECTOR, USERDEFINED, NOTDEFINED} — a nailplate-formed joist hanger and a strap tie map to `NAILPLATE`, a bolted hold-down to `SHEARCONNECTOR`, the `HangerType.IfcDesignation` the `ObjectType` discriminant the federation reads when the predefined enum lacks the precise connector class. The connector is keyed to the `properties#MATERIAL_PROPERTY` `Mechanical` `YieldStrengthMpa` of its base-steel `MaterialId` (`metal.steel`) the cold-formed grade asserts, and the allowable-load receipt rides as the `Pset_MechanicalFastenerType` design property the structural-analysis federation reads. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the scalar columns the wire reads, never an IFC entity.
