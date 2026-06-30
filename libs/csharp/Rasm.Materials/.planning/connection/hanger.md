# [MATERIALS_HANGER]

THE FRAMING-CONNECTOR CONNECTIONFAMILY. The hanger vocabulary — the `HangerType` framing-connector discriminant (joist hanger · framing angle · strap · hold-down), the `SteelGauge` cold-formed sheet-steel gauge axis (the AISI S100 base-metal/design thickness and gauge-implied yield), the `HangerInstall` attachment discriminant (the nailed/screwed/bolted fastener schedule the connector demands), and the `HangerSection` cold-formed capacity receipt (the carried-member nominal fit, the typed download/uplift/lateral `LoadResistance` direction-set, the base-steel gauge and thickness) — is the realized framing-connector vocabulary one `connection#CONNECTION_OWNER` `ConnectionItem` carries in the `ConnectionFamily.Hanger` case. A joist hanger is a `ConnectionItem` row, never a `Hanger` type: the connector type, the gauge, the carried-member fit, the resisted-direction allowable receipt, and the fastener schedule are hanger-`ConnectionItem` columns, and the `HangerSection` projection feeds the same `connection#CONNECTION_OWNER` `ConnectionItem.Of` admission and the same `ConnectionCatalogue.Build` fold the reinforcement and fastener families drive — a connector schedule places through the construction layout fold over one `ConnectionItem`, never a per-family schedule owner. The hanger vocabulary grows by data — a new connector is one `ConnectionCatalogue` `HangerRow` entry, a new gauge one `SteelGauge` row, a new connector class one `HangerType` row, a new attachment one `HangerInstall` row — never a per-connector type. A cold-formed framing connector is a host-neutral capacity receipt the host materializes into a stamped-and-formed steel-plate solid, NEVER a host brep here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps — the resolved layout is a `Seq<Placement>` of scalar tuples the host boundary materializes, and the connector plate is the scalar `HangerPlate` receipt the host folds into the saddle-seat/flange/nail-grid solid exactly as `Connection/reinforcement#REINFORCEMENT_FAMILY` `RebarBend` is the bent-bar receipt the host lofts). The page composes `connection#CONNECTION_OWNER` for the `ConnectionItem`/`ConnectionId`/`ConnectionSection`/`ConnectionFault` shape, the `Rasm` kernel `PositiveMagnitude` value-object for every member-size/capacity/thickness column and `Dimension` for the discrete gauge designation and the discrete fastener count, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel` — galvanized cold-formed steel) appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here; the reinforcement and fastener families land their own sibling vocabularies on `Connection/reinforcement#REINFORCEMENT_FAMILY` and `Connection/fastener#FASTENER_FAMILY`, and the `anchor` connector folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, never a separate family — the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint). The framing connector carries the IFC4.3 `IfcDiscreteAccessoryTypeEnum` accessory token (`SHOE`/`BRACKET`/`ANCHORPLATE` — the schema-spelled members the `Rasm.Bim` egress IfcClass valid-set carries) and the GeometryGym-verified `IfcMechanicalFastenerTypeEnum` attachment token (`NAILPLATE`/`NAIL`/`SCREW`/`BOLT`) the `Rasm.Bim` egress gate reads to decide the IFC entity class, never a free designation string the federation cannot round-trip.

## [01]-[INDEX]

- [01]-[HANGER_FAMILY]: the `HangerType` framing-connector discriminant carrying its verified IFC accessory/fastener tokens and resisted-direction band, the `SteelGauge` cold-formed sheet-steel gauge axis (base-metal/design thickness + AISI S100 yield + per-width axial capacity), the `HangerInstall` attachment discriminant carrying its `FastenerSpec` schedule, the `LoadResistance` typed download/uplift/lateral allowable-and-direction receipt, the `HangerSection` cold-formed capacity receipt, the `HangerPlate` host-materialization scalar tuple, and the `ConnectionCatalogue.BuildHangerRows` framing-connector row table.

## [02]-[HANGER_FAMILY]

- Owner: the hanger vocabulary (`HangerType` the joist-hanger/framing-angle/strap/hold-down framing-connector discriminant carrying its `IfcDiscreteAccessoryTypeEnum`/`IfcMechanicalFastenerTypeEnum` tokens and the carried-member predicate, `SteelGauge` the 18/16/14/12/10 ga sheet-steel base-metal/design-thickness axis with its AISI S100 gauge yield, `HangerInstall` the nailed/screwed/bolted attachment discriminant carrying its `FastenerSpec` schedule, `LoadResistance` the typed three-direction allowable receipt, `HangerSection` the cold-formed allowable-load receipt, `HangerPlate` the host-materialization scalar tuple); `ConnectionCatalogue.BuildHangerRows` the registered-row seed `connection#CONNECTION_OWNER` `ConnectionCatalogue.Build` folds; the `HangerSection.GovernedCapacity` projection emitting the load-duration-adjusted allowable the design seam reads and the `HangerSection.DemandCapacityRatio` design-check the structural-connection seam reads.
- Cases: type {joist-hanger (face-mount/top-flange carried-member saddle, an `IfcDiscreteAccessory` `SHOE`, nailplate-fastened, resists download + uplift) · framing-angle (the L-bend tension/shear clip, a `BRACKET`, nail/screw-fastened, resists uplift + lateral) · strap (the flat tension tie / coiled strap, a nailplate `BRACKET`, nail-fastened, resists uplift only) · hold-down (the shear-wall tension anchor, an `ANCHORPLATE`, bolt-fastened, resists uplift only)} · gauge {18 ga (1.214 mm) · 16 ga (1.519 mm) · 14 ga (1.897 mm) · 12 ga (2.657 mm) · 10 ga (3.416 mm) base-metal thickness, the manufacturer-published cold-formed designation} · install {nailed (10d/16d common, or the SDS/SD structural-connector screw — `NAIL`) · screwed (the SD9/SD10 structural-connector screw — `SCREW`) · bolted (the cast-in/through-bolt — `BOLT`)} — a connector is a `ConnectionItem` row over one `HangerType`, one `SteelGauge`, and one `HangerInstall`, never a connector subtype.
- Entry: `public Fin<HangerCapacity> GovernedCapacity(LoadDuration duration, Op key)` on `HangerSection` — the allowable-load projection resolving the governing allowable over ONLY the directions `HangerType.Resists` flags (a strap reads uplift alone, a joist hanger download capped by uplift), scaled by the wood-connector load-duration factor (`Cd`) for a wood-fastened connector and passed unscaled for a bolted hold-down, into the scalar `HangerCapacity` receipt the structural-connection-design seam reads, `Fin<T>` railing a connector whose every resisted direction is degenerate (a transcription gap) through `ConnectionFault.Capacity`; `public double DemandCapacityRatio(HangerCapacity capacity, LoadDemand demand)` the unit-check the connector schedule reads (the max of the per-direction demand/allowable ratios, > 1 an overstressed connector the design seam flags); `ConnectionCatalogue.BuildHangerRows(context)` folds the framing-connector `HangerRow` table through `HangerOf` into the registered `ConnectionItem` rows `ConnectionCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetHangerByType`/`GetByGauge` family.
- Packages: Rasm (project — `PositiveMagnitude` for the carried-member-size/allowable-load/base-thickness columns, never an int-backed `Dimension` that truncates a fractional capacity; `Dimension` for the discrete sheet-steel gauge designation and the discrete fastener count), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the type/gauge/install axes with the generated total `Switch`, `[ComplexValueObject]` for the `LoadResistance` three-direction receipt, `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold` for the admission rail and the catalogue fold), GeometryGymIFC_Core (the `IfcMechanicalFastenerTypeEnum` member spellings GeometryGym-verified in `api-geometrygym-ifc.md` and the IFC4.3 `IfcDiscreteAccessoryTypeEnum` member spellings the Bim egress validates against its IfcClass valid-set — read at the `Rasm.Bim` egress, the IFC enum the Bim folder `.api`; the token is a plain `string` here), BCL inbox (`FrozenDictionary`).
- Growth: the hanger vocabulary grows by data — a new connector is one `HangerRow` catalogue entry carrying its type/gauge/install/carried-member/resisted-allowable columns, a new gauge one `SteelGauge` row carrying its base-metal/design thickness and gauge yield, a new connector class one `HangerType` row carrying its accessory/fastener tokens and resisted-direction set, a new attachment one `HangerInstall` row carrying its `FastenerSpec` — never a per-connector type, never a per-type `ConnectionItem` variant. The reinforcement/fastener/joint families carry their own vocabularies on their own pages the way hanger carries `HangerType`/`SteelGauge`/`HangerSection`; `anchor` folds as a `FastenerKind` arm on `Connection/fastener#FASTENER_FAMILY`, so the `ConnectionFamily` axis stays CLOSED at four (reinforcement/fastener/hanger/joint), never a fifth sibling family or a per-connector type. A new resisted-load direction is one `LoadResistance` column shared by every connector (a value-object growth), never a per-direction connector type; a new host-materialization feature (a back-flange, a sloped seat) is one `HangerPlate` column, never a per-feature plate type.
- Boundary: the hanger vocabulary is the framing-connector `ConnectionFamily` — a per-connector `Hanger` class is the deleted form; `HangerSection` composes the `Rasm` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every length/load column so the section never re-mints a primitive and a fractional cold-formed base thickness (`18 ga = 1.214 mm`, `12 ga = 2.657 mm`) and a fractional published allowable (`download = 7.83 kN`) admit without the truncation an int-backed `Dimension` count would force, the discrete sheet-steel gauge number admitting as the `Dimension` count carrier so a gauge is a designation never a length, the discrete fastener count (the `FastenerSpec.Quantity` nails the connector demands) admitting as a `Dimension` so a count is never a fractional magnitude; the connector resists a TYPED set of directions — the prior floor-positive `0.10` kN placeholder columns (a strap seeding a fake `0.10` download a connector never resists) are the deleted form, replaced by the `LoadResistance` `[ComplexValueObject]` carrying each direction's allowable AND a resisted-flag so `GoverningAllowableKn` reads ONLY the directions the connector actually resists rather than a degenerate min over a placeholder, and `HangerType.Resists` is the type-level direction predicate the section validates the row against (a strap row declaring a download capacity rails `ConnectionFault.Capacity`); the framing connector is a host-neutral capacity receipt — `HangerSection` carries the carried-member nominal fit, the resisted download/uplift/lateral loads, and the base gauge/thickness as `PositiveMagnitude` columns the `IfcDiscreteAccessory`/`IfcMechanicalFastener` wire and the structural-design seam read, NEVER a host brep, the host boundary materializes the stamped-plate solid from the scalar `HangerPlate` receipt (the saddle seat-width/seat-depth/back-height, the formed-flange width, the nail-hole grid pitch and count) exactly as the construction layout materializes a `Placement` tuple, so this owner stays host-neutral; the allowable-load receipt is the published manufacturer-evaluated capacity scaled by the wood-connector load-duration factor at `GovernedCapacity`, NOT a re-derived structural calculation — the design seam reads the `HangerCapacity` allowable the manufacturer evaluation report (ICC-ES / manufacturer catalogue) certifies, and the `SteelGauge` carries the AISI S100 gauge yield and the per-width axial section capacity the connector's NET-section and bearing checks compose so the gauge columns are load-bearing (a connector's allowable is bounded by the gauge section, never a free number), never a connection re-analysis the seam re-runs; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for galvanized cold-formed steel) the row's `ConnectionItem.AppearanceId` column carries, never a hanger-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `SteelGauge` base-metal band is the cold-formed designation, the gauge yield the AISI S100 spec-nominal cold-formed strength, and the measured grade the property-library receipt; `ConnectionCatalogue.BuildHangerRows` seeds the `connection#CONNECTION_OWNER` `ConnectionCatalogue.Rows` table with the framing-connector rows keyed `connection.<designation>` (`connection.hanger-jh-2x8-18ga`, `connection.holdown-hd-12ga`), the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` and the gauge/fastener-count through `key.AcceptValidated<Dimension>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `ConnectionItem`; the placement of a connector schedule reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel hanger-layout owner; the IFC entity class is the `Rasm.Bim` egress gate's choice (`HangerType.IfcAccessoryType` selecting `IfcDiscreteAccessory` for a fabricated saddle/bracket, `HangerType.IfcFastenerType` the attaching-fastener `IfcMechanicalFastener` token), this page emitting only the verified token columns, never an IFC entity.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;     // FrozenDictionary (the registered-row table)
using System.Globalization;          // CultureInfo.InvariantCulture (the LoadResistance.Validate format provider)
using LanguageExt;
using Rasm.Domain;                   // Op (the boundary-admission key)
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude), Dimension (>=1 discrete count)
using Rasm.Element;                  // MaterialId (the seam-carried material identity the ConnectionItem AppearanceId/CapacityKey reference)
using Rasm.Materials.Connection;     // ConnectionFamily/ConnectionId/ConnectionItem/ConnectionSection/ConnectionFault (the parent CONNECTION_OWNER)
using Thinktecture;
using static LanguageExt.Prelude;

// Each Connection family page is its OWN Rasm.Materials.Connection.<Family> sub-namespace so the four sibling
// `ConnectionCatalogue` static classes are distinct types (a shared Rasm.Materials.Connection would be a CS0101 collision
// with connection.md's own `ConnectionCatalogue`); connection#CONNECTION_OWNER stays the parent Rasm.Materials.Connection
// and folds Hanger.ConnectionCatalogue.BuildHangerRows by the sub-namespace-qualified name. The page-local LoadDuration is
// the hanger Cd axis (distinct from Profiles timber's EC5 LoadDuration — separate namespaces); parent types via the using above.
namespace Rasm.Materials.Connection.Hanger;

// --- [TYPES] -------------------------------------------------------------------------------
// HangerType carries the portable IFC tokens the Rasm.Bim egress gate reads to choose the IFC entity class: a fabricated
// cold-formed connector IS an IfcDiscreteAccessory (IfcDiscreteAccessoryTypeEnum SHOE for a joist-hanger saddle, BRACKET
// for an angle/strap clip, ANCHORPLATE for a shear-wall hold-down — the IFC4.3 IfcDiscreteAccessoryTypeEnum member
// spellings the Bim egress's IfcClass valid-set carries), itself FASTENED by mechanical fasteners (the GeometryGym-verified
// IfcMechanicalFastenerTypeEnum NAILPLATE for the nailplate-formed body, NAIL/SCREW/BOLT for the attaching fastener).
// Resists is the type-level direction predicate (which of download/uplift/lateral the connector develops), so a strap
// (uplift-only) never carries a fake download column. CarriesMember is true only for the saddle-seat hanger.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HangerType {
    public static readonly HangerType JoistHanger  = new("joist-hanger",  ifcDesignation: "joist-hanger", ifcAccessoryType: "SHOE",        ifcFastenerType: "NAILPLATE", carriesMember: true,  resists: ResistanceFlags.Download | ResistanceFlags.Uplift);
    public static readonly HangerType FramingAngle = new("framing-angle", ifcDesignation: "framing-angle", ifcAccessoryType: "BRACKET",     ifcFastenerType: "NAIL",      carriesMember: false, resists: ResistanceFlags.Uplift   | ResistanceFlags.Lateral);
    public static readonly HangerType Strap        = new("strap",         ifcDesignation: "strap-tie",     ifcAccessoryType: "BRACKET",     ifcFastenerType: "NAILPLATE", carriesMember: false, resists: ResistanceFlags.Uplift);
    public static readonly HangerType HoldDown     = new("hold-down",     ifcDesignation: "hold-down",     ifcAccessoryType: "ANCHORPLATE", ifcFastenerType: "BOLT",      carriesMember: false, resists: ResistanceFlags.Uplift);
    public string IfcDesignation { get; }
    public string IfcAccessoryType { get; }   // IFC4.3 IfcDiscreteAccessoryTypeEnum member spelling (the Bim egress IfcClass valid-set reads)
    public string IfcFastenerType { get; }     // GeometryGym-verified IfcMechanicalFastenerTypeEnum member spelling (the attaching fastener)
    public bool CarriesMember { get; }
    public ResistanceFlags Resists { get; }
}

[Flags]
public enum ResistanceFlags { None = 0, Download = 1, Uplift = 2, Lateral = 4 }

// SteelGauge carries the AISI S100 cold-formed sheet-steel design model: the base-metal (uncoated, manufacturer-published)
// and design (the as-formed) thickness, plus the gauge-band cold-formed yield (Fy, the SS Grade 33/50 minimum the gauge
// implies). AxialSectionCapacityKnPerMm is the per-unit-width nominal axial section strength (Fy * design thickness * 1e-3)
// the connector's NET-section tension and bearing checks bound the published allowable against, so the gauge columns are
// load-bearing — a connector's allowable can never exceed its gauge section, never a free catalogue number.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SteelGauge {
    public static readonly SteelGauge Ga18 = new("18ga", gaugeNumber: 18, baseThicknessMm: 1.214, designThicknessMm: 1.092, yieldMpa: 230.0);
    public static readonly SteelGauge Ga16 = new("16ga", gaugeNumber: 16, baseThicknessMm: 1.519, designThicknessMm: 1.367, yieldMpa: 230.0);
    public static readonly SteelGauge Ga14 = new("14ga", gaugeNumber: 14, baseThicknessMm: 1.897, designThicknessMm: 1.707, yieldMpa: 340.0);
    public static readonly SteelGauge Ga12 = new("12ga", gaugeNumber: 12, baseThicknessMm: 2.657, designThicknessMm: 2.391, yieldMpa: 340.0);
    public static readonly SteelGauge Ga10 = new("10ga", gaugeNumber: 10, baseThicknessMm: 3.416, designThicknessMm: 3.074, yieldMpa: 340.0);
    public int GaugeNumber { get; }
    public double BaseThicknessMm { get; }
    public double DesignThicknessMm { get; }
    public double YieldMpa { get; }
    public double AxialSectionCapacityKnPerMm => YieldMpa * DesignThicknessMm * 1e-3;
}

// HangerInstall carries the FASTENER SCHEDULE the connector demands — the nail/screw/bolt designation, the per-connector
// quantity, the single-fastener allowable, and the verified IfcMechanicalFastenerTypeEnum token. DurationSensitive flags
// a wood-driven fastener (the Cd load-duration factor applies); a bolted hold-down passes unscaled.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HangerInstall {
    public static readonly HangerInstall Nailed  = new("nailed",  fastener: FastenerSpec.Of("10d-common", quantity: 10, perFastenerKn: 0.62, ifcFastenerType: "NAIL"),  durationSensitive: true);
    public static readonly HangerInstall Screwed = new("screwed", fastener: FastenerSpec.Of("sd9-screw",  quantity: 8,  perFastenerKn: 1.05, ifcFastenerType: "SCREW"), durationSensitive: true);
    public static readonly HangerInstall Bolted  = new("bolted",  fastener: FastenerSpec.Of("through-bolt", quantity: 2, perFastenerKn: 18.0, ifcFastenerType: "BOLT"), durationSensitive: false);
    public FastenerSpec Fastener { get; }
    public bool DurationSensitive { get; }
}

// The NDS load-duration factor (Cd) the wood-fastened connector's published allowable scales by — the NDS Table 2.3.2
// duration-of-load set keyed by the governing load case, DISTINCT from the EC5 timber k_mod (a separate code over a
// separate factor — the two are never conflated). GovernedCapacity scales a duration-sensitive (nailed/screwed-into-wood)
// connector by this Cd and passes a bolted hold-down unscaled; a new load case is one row, never a per-case scale literal.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LoadDuration {
    public static readonly LoadDuration Permanent   = new("permanent",   cd: 0.90);
    public static readonly LoadDuration TenYear     = new("ten-year",    cd: 1.00);
    public static readonly LoadDuration TwoMonth    = new("two-month",   cd: 1.15);
    public static readonly LoadDuration SevenDay    = new("seven-day",   cd: 1.25);
    public static readonly LoadDuration WindSeismic = new("wind-seismic", cd: 1.60);
    public static readonly LoadDuration Impact      = new("impact",      cd: 2.00);
    public double Cd { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-connector fastener schedule: the discrete fastener designation/count and the single-fastener allowable, so
// the connector's fastener-governed capacity (Quantity * PerFastenerKn) is a derived bound the GovernedCapacity caps the
// published allowable against — a connector is governed by the LESSER of its steel-section allowable and its fastener
// group. The verified IfcMechanicalFastenerTypeEnum token rides to the Rasm.Bim egress for the attaching-fastener entity.
public readonly record struct FastenerSpec(string Designation, int Quantity, double PerFastenerKn, string IfcFastenerType) {
    public static FastenerSpec Of(string designation, int quantity, double perFastenerKn, string ifcFastenerType) =>
        new(designation, quantity, perFastenerKn, ifcFastenerType);
    public double GroupAllowableKn => Quantity * PerFastenerKn;
}

// The typed three-direction allowable receipt — each direction's published manufacturer allowable AND whether the
// connector resists it. Replaces the deleted floor-positive 0.10 kN placeholder columns: a strap carries Uplift alone
// (Download/Lateral Resisted=false, allowable irrelevant), so Governing reads ONLY the resisted directions rather than a
// degenerate min over a fake 0.10. The [ComplexValueObject] guards each resisted allowable > 0 at Create, so an
// unrepresentable connector (a resisted direction with a non-positive allowable) can never construct.
[ComplexValueObject]
public readonly partial struct LoadResistance {
    public double DownloadKn { get; }
    public bool ResistsDownload { get; }
    public double UpliftKn { get; }
    public bool ResistsUplift { get; }
    public double LateralKn { get; }
    public bool ResistsLateral { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double downloadKn, ref bool resistsDownload,
        ref double upliftKn, ref bool resistsUplift,
        ref double lateralKn, ref bool resistsLateral) {
        bool degenerate =
            (resistsDownload && !(downloadKn > 0.0)) ||
            (resistsUplift && !(upliftKn > 0.0)) ||
            (resistsLateral && !(lateralKn > 0.0)) ||
            !(resistsDownload || resistsUplift || resistsLateral);
        if (degenerate)
            validationError = new ValidationError($"<load-resistance-degenerate:d={downloadKn:R}/{resistsDownload},u={upliftKn:R}/{resistsUplift},l={lateralKn:R}/{resistsLateral}>");
    }

    // The governing allowable is the LEAST resisted-direction allowable (the connector fails in its weakest resisted
    // direction); the per-direction reads return 0 for an unresisted direction so a demand in that direction over-stresses.
    public double GoverningKn => Math.Min(
        ResistsDownload ? DownloadKn : double.MaxValue,
        Math.Min(ResistsUplift ? UpliftKn : double.MaxValue, ResistsLateral ? LateralKn : double.MaxValue));
    public double Download => ResistsDownload ? DownloadKn : 0.0;
    public double Uplift => ResistsUplift ? UpliftKn : 0.0;
    public double Lateral => ResistsLateral ? LateralKn : 0.0;
}

// The load-duration-adjusted allowable receipt the structural-connection-design seam reads — each resisted direction
// scaled by Cd (a wood-fastened connector) and the fastener-group bound applied, plus the governing capacity and the
// applied Cd so the seam reads the connector's adjusted strength in one shape.
public readonly record struct HangerCapacity(double DownloadKn, double UpliftKn, double LateralKn, double GoverningKn, double Cd);

// The applied connector demand (a load combination's factored download/uplift/lateral) the DemandCapacityRatio unit-check reads.
public readonly record struct LoadDemand(double DownloadKn, double UpliftKn, double LateralKn);

// The host-materialization scalar receipt — the saddle-seat/back-flange/nail-grid geometry the host boundary folds into
// the stamped-and-formed steel-plate solid, exactly as RebarBend is the bent-bar receipt the host lofts. NEVER a host
// brep here: a connector is one HangerPlate the Rasm.Bim wire serializes and the host plug-in materializes ply-by-ply.
public readonly record struct HangerPlate(
    double SeatWidthMm,
    double SeatDepthMm,
    double BackHeightMm,
    double FlangeWidthMm,
    double NailHolePitchMm,
    int NailHoleCount);

public readonly record struct HangerSection(
    HangerType Type,
    SteelGauge Gauge,
    HangerInstall Install,
    Dimension GaugeNumber,
    Dimension FastenerCount,
    PositiveMagnitude CarriedMemberWidthMm,
    PositiveMagnitude CarriedMemberDepthMm,
    PositiveMagnitude BaseThicknessMm,
    LoadResistance Allowable) {

    // The FINISH the appearance projection reads (galvanized cold-formed steel), INDEPENDENT from CapacityKey by the
    // connection#CONNECTION_OWNER two-slot law — a connector with a distinct mill finish keeps its steel CapacityKey.
    public MaterialId AppearanceId => MaterialId.Of("metal.steel");

    // The CAPACITY material whose properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row the design seam reads — the
    // connector's cold-formed structural STEEL (metal.steel), sourced INDEPENDENTLY from the AppearanceId finish.
    public MaterialId CapacityKey => MaterialId.Of("metal.steel");

    // The per-fastener group bound and the gauge net-section axial bound the published allowable is capped against, so
    // the gauge and fastener columns are load-bearing: a connector can never develop more than its weakest physical bound.
    public double FastenerGroupAllowableKn => Install.Fastener.GroupAllowableKn;
    public double GaugeAxialBoundKn => Gauge.AxialSectionCapacityKnPerMm * CarriedMemberWidthMm.Value;

    // The host-materialization receipt: the saddle seat sizes the carried member, the back rises with the member depth,
    // the formed flange a base-thickness multiple, the nail grid the fastener schedule's count at a standard pitch.
    public HangerPlate Plate => new(
        SeatWidthMm: CarriedMemberWidthMm.Value,
        SeatDepthMm: Math.Max(38.0, CarriedMemberWidthMm.Value),
        BackHeightMm: CarriedMemberDepthMm.Value,
        FlangeWidthMm: 25.0 + BaseThicknessMm.Value * 6.0,
        NailHolePitchMm: 19.05,
        NailHoleCount: Install.Fastener.Quantity);

    // The governing allowable over ONLY the resisted directions, capped by the fastener-group and gauge bounds, scaled
    // by Cd for a duration-sensitive (wood-fastened) connector. Fin rails a connector whose every physical bound has
    // collapsed (a transcription gap) — NOT the dead `> 0` guard a PositiveMagnitude column already guarantees.
    public Fin<HangerCapacity> GovernedCapacity(LoadDuration duration, Op key) =>
        from bounded in guard(
            Allowable.GoverningKn > 0.0 && FastenerGroupAllowableKn > 0.0 && GaugeAxialBoundKn > 0.0,
            ConnectionFault.Capacity(key, $"<degenerate-connector-bound:{Type.Key}:{Gauge.Key}:gov={Allowable.GoverningKn:R}/grp={FastenerGroupAllowableKn:R}/sec={GaugeAxialBoundKn:R}>"))
        let cd = Install.DurationSensitive ? duration.Cd : 1.0
        let bound = Math.Min(FastenerGroupAllowableKn, GaugeAxialBoundKn)
        let download = Math.Min(Allowable.Download, bound) * cd
        let uplift = Math.Min(Allowable.Uplift, bound) * cd
        let lateral = Math.Min(Allowable.Lateral, bound) * cd
        let governing = Math.Min(Allowable.GoverningKn, bound) * cd
        select new HangerCapacity(download, uplift, lateral, governing, cd);

    // The connector unit-check: the max of the per-direction demand/allowable ratios (a zero allowable in a direction
    // the connector does not resist drives the ratio to infinity, flagging a misapplied connector). > 1 is overstressed.
    public double DemandCapacityRatio(HangerCapacity capacity, LoadDemand demand) => Math.Max(
        Ratio(demand.DownloadKn, capacity.DownloadKn),
        Math.Max(Ratio(demand.UpliftKn, capacity.UpliftKn), Ratio(demand.LateralKn, capacity.LateralKn)));

    static double Ratio(double demand, double allowable) =>
        demand <= 0.0 ? 0.0 : allowable <= 0.0 ? double.PositiveInfinity : demand / allowable;
}

public readonly record struct HangerRow(
    string Designation,
    string Type,
    string Gauge,
    string Install,
    double CarriedMemberWidthMm,
    double CarriedMemberDepthMm,
    double DownloadKn,
    double UpliftKn,
    double LateralKn);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConnectionCatalogue {
    // ICC-ES / manufacturer-catalogue ten-year-duration allowables; a load direction the connector does not resist
    // carries a 0.0 column (HangerType.Resists is the truth, the column ignored when unresisted) rather than a fake 0.10.
    static readonly Seq<HangerRow> HangerRows = Seq(
        new HangerRow("connection.hanger-jh-2x6-18ga",   "joist-hanger",  "18ga", "nailed",  38.1,  139.7, 4.85,  0.78, 0.0),
        new HangerRow("connection.hanger-jh-2x8-18ga",   "joist-hanger",  "18ga", "nailed",  38.1,  184.2, 7.83,  0.78, 0.0),
        new HangerRow("connection.hanger-jh-2x10-16ga",  "joist-hanger",  "16ga", "nailed",  38.1,  235.0, 10.45, 1.00, 0.0),
        new HangerRow("connection.hanger-jh-2x12-16ga",  "joist-hanger",  "16ga", "nailed",  38.1,  285.8, 12.46, 1.00, 0.0),
        new HangerRow("connection.hanger-jh-4x8-14ga",   "joist-hanger",  "14ga", "screwed", 88.9,  184.2, 14.23, 1.33, 0.0),
        new HangerRow("connection.hanger-jh-4x10-14ga",  "joist-hanger",  "14ga", "screwed", 88.9,  235.0, 17.79, 1.33, 0.0),
        new HangerRow("connection.angle-a35-18ga",       "framing-angle", "18ga", "nailed",  38.1,  38.1,  0.0,   3.96, 2.18),
        new HangerRow("connection.angle-l70-16ga",       "framing-angle", "16ga", "screwed", 63.5,  63.5,  0.0,   5.74, 3.11),
        new HangerRow("connection.angle-l90-14ga",       "framing-angle", "14ga", "screwed", 88.9,  88.9,  0.0,   8.45, 4.60),
        new HangerRow("connection.strap-cs16-16ga",      "strap",         "16ga", "nailed",  38.1,  600.0, 0.0,   9.34, 0.0),
        new HangerRow("connection.strap-cs14-14ga",      "strap",         "14ga", "nailed",  50.8,  900.0, 0.0,  14.50, 0.0),
        new HangerRow("connection.holdown-hd5b-12ga",    "hold-down",     "12ga", "bolted",  63.5,  254.0, 0.0,  23.13, 0.0),
        new HangerRow("connection.holdown-hd12-10ga",    "hold-down",     "10ga", "bolted",  76.2,  330.0, 0.0,  53.38, 0.0));

    static Fin<(ConnectionId Id, ConnectionItem Item)> HangerOf(HangerRow r, Context context, Op key) =>
        from type in HangerType.TryGet(r.Type, out HangerType? t) ? Fin.Succ(t!) : Fin.Fail<HangerType>(ConnectionFault.Designation(key, $"<unknown-hanger-type:{r.Type}>"))
        from gauge in SteelGauge.TryGet(r.Gauge, out SteelGauge? g) ? Fin.Succ(g!) : Fin.Fail<SteelGauge>(ConnectionFault.Grade(key, $"<unknown-gauge:{r.Gauge}>"))
        from install in HangerInstall.TryGet(r.Install, out HangerInstall? i) ? Fin.Succ(i!) : Fin.Fail<HangerInstall>(ConnectionFault.Designation(key, $"<unknown-install:{r.Install}>"))
        from resistance in ResistanceOf(type, r, key)
        from gaugeNo in key.AcceptValidated<Dimension>(candidate: gauge.GaugeNumber)
        from fastenerCount in key.AcceptValidated<Dimension>(candidate: install.Fastener.Quantity)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.CarriedMemberWidthMm)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: r.CarriedMemberDepthMm)
        from thickness in key.AcceptValidated<PositiveMagnitude>(candidate: gauge.BaseThicknessMm)
        let section = new HangerSection(type, gauge, install, gaugeNo, fastenerCount, width, depth, thickness, resistance)
        from item in ConnectionItem.Of(ConnectionFamily.Hanger, r.Designation, new ConnectionSection.Hanger(section), section.CapacityKey, section.AppearanceId, key)
        select (ConnectionId.Of(r.Designation), item);

    // The row's per-direction allowables admit through the LoadResistance [ComplexValueObject] gated by the connector's
    // type-level Resists predicate, so a strap row carrying a download column (or a joist-hanger row missing its download)
    // rails ConnectionFault.Capacity rather than seeding a connector that resists a direction it has no published value for.
    static Fin<LoadResistance> ResistanceOf(HangerType type, HangerRow r, Op key) =>
        LoadResistance.Validate(
            downloadKn: r.DownloadKn, resistsDownload: type.Resists.HasFlag(ResistanceFlags.Download),
            upliftKn: r.UpliftKn, resistsUplift: type.Resists.HasFlag(ResistanceFlags.Uplift),
            lateralKn: r.LateralKn, resistsLateral: type.Resists.HasFlag(ResistanceFlags.Lateral),
            provider: CultureInfo.InvariantCulture, item: out LoadResistance? built) is { } error
            ? Fin.Fail<LoadResistance>(ConnectionFault.Capacity(key, $"<resistance-mismatch:{type.Key}:{r.Designation}:{error.Message}>"))
            : Fin.Succ(built!.Value);

    public static FrozenDictionary<ConnectionId, ConnectionItem> BuildHangerRows(Context context) =>
        HangerRows
            .Choose(row => HangerOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [HANGER_ROW_TRANSCRIPTION]: REALIZED — the cold-formed framing-connector catalogue (the joist-hanger face-mount saddles over the 2x6..4x10 carried-member sizes, the A35/L-series framing angles, the CS-series flat straps, and the HD-series shear-wall hold-downs) seeds through `ConnectionCatalogue.BuildHangerRows(context)` over the `HangerRow` designation/type/gauge/install table, the carried-member columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)`, the discrete sheet-steel gauge and fastener count through `key.AcceptValidated<Dimension>(candidate:)`, and the per-direction allowables through the `LoadResistance` `[ComplexValueObject]` gated by the `HangerType.Resists` type-level direction predicate, into the kernel value-objects; a new connector is one `HangerRow` data addition plus, if novel, one `HangerType`/`SteelGauge`/`HangerInstall` row. The `SteelGauge.BaseThicknessMm` columns transcribe the manufacturer-uncoated cold-formed sheet-steel base-metal thicknesses (18 ga = 1.214 mm, 16 ga = 1.519 mm, 14 ga = 1.897 mm, 12 ga = 2.657 mm, 10 ga = 3.416 mm) and `DesignThicknessMm` the AISI S100 design (as-formed) thickness (≈0.90·base for galvanized stock); the `SteelGauge.YieldMpa` is the AISI S100 cold-formed minimum yield the gauge band implies (SS Grade 33 ≈ 230 MPa for the light 18/16 ga stock, SS Grade 50 ≈ 340 MPa for the structural 14/12/10 ga stock), so `AxialSectionCapacityKnPerMm = YieldMpa · DesignThicknessMm · 1e-3` is the per-unit-width nominal axial section strength the connector net-section/bearing bound composes; the `DownloadKn`/`UpliftKn`/`LateralKn` columns transcribe the published manufacturer-evaluation-report (ICC-ES) ten-year-duration allowable loads, a direction the connector does not resist carrying a `0.0` column the `Resists` predicate ignores (the deleted `0.10` kN placeholder, a fake column dressed as a resisted load, is gone); the `GovernedCapacity` fold reads ONLY the resisted directions, caps each by the LESSER of the fastener-group allowable (`FastenerSpec.Quantity · PerFastenerKn`) and the gauge axial bound (`GaugeAxialBoundKn`), and scales by the NDS wood-connector load-duration factor (`Cd` ∈ {0.90 permanent, 1.00 ten-year, 1.15 two-month, 1.25 seven-day, 1.60 ten-minute/wind-seismic, 2.00 impact}) for a nailed/screwed connector into wood, passing the bolted hold-down allowable through unscaled (`DurationSensitive == false`).
- [HANGER_DESIGN_CHECK]: REALIZED — the connector capacity is the LEAST of three physical bounds, so the `SteelGauge` and `HangerInstall` columns are load-bearing rather than decorative: the published manufacturer allowable (`LoadResistance`), the fastener-group allowable (`HangerInstall.Fastener.GroupAllowableKn = Quantity · PerFastenerKn`, the nail/screw/bolt group the connector demands), and the gauge net-section axial bound (`Gauge.AxialSectionCapacityKnPerMm · CarriedMemberWidthMm`, the AISI S100 section strength the connector body develops). `HangerSection.GovernedCapacity` caps the published allowable against the lesser of the fastener and gauge bounds so a connector under-fastened or under-gauged for its catalogue allowable governs at the physical bound, never a free catalogue number; `HangerSection.DemandCapacityRatio(capacity, demand)` is the unit-check the connector schedule reads (the max of the per-direction demand/allowable ratios, a demand in an unresisted direction driving the ratio to infinity so a misapplied connector — a download on a strap — is flagged, > 1 an overstressed connector the design seam reports). This is the gauge/fastener/published-allowable join the prior page carried as three stored-but-unused columns; the design check is the `HangerSection` projection, never a `Rasm.Compute` re-analysis (the seam reads the bound, never re-derives the manufacturer evaluation).
- [HANGER_HOST_MATERIALIZATION]: REALIZED — the `HangerPlate(SeatWidthMm, SeatDepthMm, BackHeightMm, FlangeWidthMm, NailHolePitchMm, NailHoleCount)` scalar receipt is the host-neutral connector geometry the `HangerSection.Plate` projection emits — the host boundary materializes the stamped-and-formed steel-plate solid (the saddle seat sized to the carried member, the back rising with the member depth, the formed flanges a base-thickness multiple wide, the nail-hole grid at a standard 3/4in pitch over the fastener-schedule count) from the scalar columns, exactly as `Connection/reinforcement#REINFORCEMENT_FAMILY` `RebarBend` is the bent-bar receipt the host lofts and `Construction/layout#ASSEMBLY_FOLD` materializes a `Seq<Placement>` of scalar tuples into host geometry. This owner NEVER constructs a host brep: the `HangerPlate` is the portable schedule data the `Rasm.Bim` wire serializes and the host plug-in materializes, so the hanger catalogue stays a leaf below the host boundary. A connector schedule (a joist-to-beam saddle run, a shear-wall hold-down stack) is a `HangerSection` per connector (its `Plate` the host solid) plus a `Construction/layout` station-stepped placement, never a per-connector host solid type — the prior page's prose-promised "saddle seat, nail-hole grid, formed flanges" materialization is now the typed `HangerPlate` receipt the host reads, not an unbacked claim.
- [IFC_DISCRETE_ACCESSORY_WIRE]: a framing connector round-trips to IFC 4.3 as an `IfcDiscreteAccessory` — the fabricated cold-formed saddle/bracket/anchorplate carrying `PredefinedType` ∈ `IfcDiscreteAccessoryTypeEnum` {SHOE (the joist-hanger saddle), BRACKET (the framing-angle/strap clip), ANCHORPLATE (the shear-wall hold-down), ANCHORPLATE/SHOE/BRACKET the connector classes the `HangerType.IfcAccessoryType` token names} — itself FASTENED by mechanical fasteners that round-trip as `IfcMechanicalFastener` carrying `PredefinedType` ∈ `IfcMechanicalFastenerTypeEnum` {NAILPLATE (the nailplate-formed connector body), NAIL/SCREW/BOLT (the attaching fastener the `HangerInstall.Fastener.IfcFastenerType` token names)}. The `HangerType.IfcAccessoryType` token is the IFC4.3 `IfcDiscreteAccessoryTypeEnum` member spelling and the `IfcFastenerType` the GeometryGym-verified `IfcMechanicalFastenerTypeEnum` spelling the `Rasm.Bim` egress gate reads to choose the IFC entity class (a fabricated connector IS an `IfcDiscreteAccessory`, the attaching nailplate/nails an `IfcMechanicalFastener` related through `IfcRelConnectsWithRealizingElements`); the `HangerType.IfcDesignation` is the `ObjectType` discriminant the federation reads when the predefined enum lacks the precise connector class. The connector is keyed to the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrengthMpa` of its base-steel `MaterialId` (`metal.steel`) the cold-formed grade asserts, the `SteelGauge.YieldMpa` the spec-nominal cold-formed band, and the allowable-load receipt rides as the `Pset_MechanicalFastenerType` design property the structural-analysis federation reads. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the verified token columns and the scalar capacity/`HangerPlate` columns the `connection#CONNECTION_PROJECTOR` `ConnectionProjector` lowers into the `Rasm_ConnectionRealization` detail bag, never an IFC entity and never a second carrier. Ripple counterpart: `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` (the `ConnectionProjection.Detail` reader — its hanger arm reads `HangerType.IfcFastenerType` for the attaching-fastener token, never a hardcoded `"BOLT"`, so a nailed joist hanger round-trips `NAILPLATE`/`NAIL` and a bolted hold-down `ANCHORPLATE`/`BOLT`).
