# [MATERIALS_CONNECTOR]

THE FRAMING-CONNECTOR COMPONENTFAMILY. The connector vocabulary — the `ConnectorType` framing-connector discriminant (joist hanger · framing angle · strap · hold-down) carrying its verified IFC accessory/fastener tokens, its resisted-direction band, and its host-materialization plate builder, the `SteelGauge` cold-formed sheet-steel gauge axis (the AISI S100 base-metal/design thickness, the gauge-implied yield, and the per-width axial section capacity), the `ConnectorInstall` attachment discriminant (the nailed/screwed/bolted `FastenerSpec` schedule the connector demands), the `LoadDurationFactor` NDS duration-of-load axis, and the `ConnectorSection` cold-formed allowable-load receipt (the carried-member nominal fit, the typed download/uplift/lateral `LoadResistance` direction-set, the base gauge and thickness) — is the realized framing-connector vocabulary one `component#COMPONENT_OWNER` `Component` carries in the `ComponentFamily.Connector` case. A connector is `ComponentClass.Minor` (an `IfcElementComponent`: one standardized type, MANY fabricated pieces), never a primary space-bounding member: a joist hanger is a `Component` row, never a `Connector` type, and its cross-section IS a `Component` FIELD — the `ConnectorSection` composes the `component#COMPONENT_OWNER` `ComponentSection.Connector` arm exactly as a steel `SteelSection` composes the `ComponentSection.Steel` arm, the connector type, the gauge, the carried-member fit, the resisted-direction allowable receipt, and the fastener schedule its columns, and the `ConnectorSection` projection feeds the same `component#COMPONENT_OWNER` `Component.Of` admission and the same `ComponentCatalogue.Build` fold the reinforcement, fastener, and timber families drive. The connector vocabulary grows by data — a new connector is one `ComponentCatalogue` `ConnectorRow` entry, a new gauge one `SteelGauge` row, a new connector class one `ConnectorType` row carrying its plate-builder column, a new attachment one `ConnectorInstall` row — never a per-connector type. A cold-formed framing connector is a host-neutral capacity receipt the host materializes into a stamped-and-formed steel-plate solid, NEVER a host brep here (the host-neutral scalar-`Placement` discipline `Construction/layout#ASSEMBLY_FOLD` keeps — the resolved layout is a `Seq<Placement>` of scalar tuples the host boundary materializes, and the connector body is the `ConnectorPlate` `[Union]` receipt the host folds saddle/angle/strap/anchor-plate into the stamped solid exactly as `Component/reinforcement#REINFORCEMENT_FAMILY` `RebarBend` is the bent-bar receipt the host lofts). VividOrange owns the structural-MEMBER section catalogues and EN grade data, NOT the manufacturer-proprietary framing-connector range (the joist-hanger/angle/strap/hold-down hardware ICC-ES evaluation reports certify), so the connector catalogue is HAND-ROLLED in-fence exactly as `Component/fastener#FASTENER_FAMILY` hand-keys the ISO 898-1 bolt property classes — the published allowables, the gauge bands, and the generative plate geometry are the realized in-fence vocabulary, never a re-tabulated VividOrange surface. The page composes `component#COMPONENT_OWNER` for the `Component`/`ComponentId`/`ComponentSection`/`ComponentFault` shape, the `Rasm.Vectors` kernel `PositiveMagnitude` value-object for every member-size/capacity/thickness column and `Dimension` for the discrete gauge designation and the discrete fastener count, the `Appearance/graph#MATERIAL_LIBRARY` `MaterialId` (`metal.steel` — galvanized cold-formed steel) appearance column each row carries, and the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` capacity receipt the connection-design seam reads by `MaterialId`, never re-derived here; the reinforcement, fastener, and joint families land their own sibling vocabularies on `Component/reinforcement#REINFORCEMENT_FAMILY`, `Component/fastener#FASTENER_FAMILY`, and `Component/joint#JOINT_FAMILY`, and the `anchor` connector folds as a `FastenerKind` arm on `Component/fastener#FASTENER_FAMILY`, never a separate family. The framing connector carries the IFC 4.3 `IfcDiscreteAccessoryTypeEnum` accessory token (`SHOE`/`BRACKET`/`ANCHORPLATE` — the schema-spelled members the `Rasm.Bim` egress IfcClass valid-set carries) the connector physically IS, and the GeometryGym-verified `IfcMechanicalFastenerTypeEnum` attachment token (`NAILPLATE`/`NAIL`/`SCREW`/`BOLT`) of the SEPARATE attaching fastener the `Rasm.Bim` egress gate reads to decide the IFC entity class, never a free designation string the federation cannot round-trip.

## [01]-[INDEX]

- [01]-[CONNECTOR_FAMILY]: the `ConnectorType` framing-connector discriminant carrying its verified IFC accessory/fastener tokens, resisted-direction band, and `[UseDelegateFromConstructor]` `ConnectorPlate` builder column, the `SteelGauge` cold-formed sheet-steel gauge axis (base-metal/design thickness + AISI S100 yield + per-width axial capacity), the `ConnectorInstall` attachment discriminant carrying its `FastenerSpec` schedule, the `LoadDurationFactor` NDS duration axis, the `LoadResistance` typed download/uplift/lateral allowable-and-direction receipt, the `ConnectorPlate` `[Union]` host-materialization body (saddle/angle/strap/anchor-plate), the `ConnectorSection` cold-formed capacity receipt with its `GovernedCapacity`/`DemandCapacityRatio` design projections, and the `ComponentCatalogue.BuildConnectorRows` framing-connector row table.

## [02]-[CONNECTOR_FAMILY]

- Owner: the connector vocabulary (`ConnectorType` the joist-hanger/framing-angle/strap/hold-down framing-connector discriminant carrying its `IfcDiscreteAccessoryTypeEnum`/`IfcMechanicalFastenerTypeEnum` tokens, the carried-member predicate, and the `ConnectorPlate` builder delegate; `SteelGauge` the 18/16/14/12/10 ga sheet-steel base-metal/design-thickness axis with its AISI S100 gauge yield; `ConnectorInstall` the nailed/screwed/bolted attachment discriminant carrying its `FastenerSpec` schedule; `LoadDurationFactor` the NDS Table 2.3.2 duration-of-load axis; `LoadResistance` the typed three-direction allowable receipt; `ConnectorPlate` the host-materialization body `[Union]`; `ConnectorSection` the cold-formed allowable-load receipt); `ComponentCatalogue.BuildConnectorRows` the registered-row seed `component#COMPONENT_OWNER` `ComponentCatalogue.Build` folds; the `ConnectorSection.GovernedCapacity` projection emitting the duration-adjusted allowable the design seam reads and the `ConnectorSection.DemandCapacityRatio` design-check the structural-connection seam reads.
- Cases: type {joist-hanger (face-mount/top-flange carried-member saddle, an `IfcDiscreteAccessory` `SHOE`, nailplate-fastened, resists download + uplift, materializes a `ConnectorPlate.Saddle`) · framing-angle (the L-bend tension/shear clip, a `BRACKET`, nail/screw-fastened, resists uplift + lateral, materializes a `ConnectorPlate.Angle`) · strap (the flat tension tie / coiled strap, a nailplate `BRACKET`, nail-fastened, resists uplift only, materializes a `ConnectorPlate.Strap`) · hold-down (the shear-wall tension anchor, an `ANCHORPLATE`, bolt-fastened, resists uplift only, materializes a `ConnectorPlate.AnchorPlate`)} · gauge {18 ga (1.214 mm) · 16 ga (1.519 mm) · 14 ga (1.897 mm) · 12 ga (2.657 mm) · 10 ga (3.416 mm) base-metal thickness, the manufacturer-published cold-formed designation} · install {nailed (10d common — `NAIL`) · screwed (the SD9/SD10 structural-connector screw — `SCREW`) · bolted (the cast-in/through-bolt — `BOLT`)} — a connector is a `Component` row over one `ConnectorType`, one `SteelGauge`, and one `ConnectorInstall`, never a connector subtype.
- Entry: `public Fin<ConnectorCapacity> GovernedCapacity(LoadDurationFactor duration, Op key)` on `ConnectorSection` — the allowable-load projection resolving the governing allowable over ONLY the directions `ConnectorType.Resists` flags (a strap reads uplift alone, a joist hanger download capped by uplift), capped by the LESSER of the fastener-group and gauge bounds and scaled by the NDS wood-connector duration factor (`Cd`) for a wood-fastened connector and passed unscaled for a bolted hold-down, into the scalar `ConnectorCapacity` receipt the structural-connection-design seam reads, `Fin<T>` railing a connector whose every physical bound has collapsed (a transcription gap) through `ComponentFault.Capacity`; `public double DemandCapacityRatio(ConnectorCapacity capacity, LoadDemand demand)` the unit-check the connector schedule reads (the max of the per-direction demand/allowable ratios, > 1 an overstressed connector the design seam flags); `public ConnectorPlate Plate => Type.BuildPlate(this)` the host-materialization receipt the body-form delegate column emits; `ComponentCatalogue.BuildConnectorRows(context)` folds the framing-connector `ConnectorRow` table through `ConnectorOf` into the registered `Component` rows `ComponentCatalogue.Build` concatenates — one polymorphic catalogue fold, never a `GetConnectorByType`/`GetByGauge` family.
- Packages: Rasm.Vectors (project — `PositiveMagnitude` for the carried-member-size/allowable-load/base-thickness columns, never an int-backed `Dimension` that truncates a fractional capacity; `Dimension` for the discrete sheet-steel gauge designation and the discrete fastener count), Rasm.Domain (`Op` the boundary-admission key, the `AcceptValidated` admission extension, `Context`), Rasm.Element (`MaterialId` the seam-carried appearance/capacity identity), Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` for the type/gauge/install/duration axes with the generated total dispatch, `[UseDelegateFromConstructor]` for the `ConnectorType.BuildPlate` body-form column, `[ComplexValueObject]` for the `LoadResistance` three-direction receipt, `[Union]` for the `ConnectorPlate` body, `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` for the catalogue key), LanguageExt.Core (`Fin`/`Seq`/`Choose`/`Fold`/`guard` for the admission rail and the catalogue fold), BCL inbox (`FrozenDictionary`). No structural-connector external package — VividOrange owns the structural-MEMBER section catalogue and EN grade data, NOT the manufacturer-proprietary framing-connector range, so the connector property bands and the generative plate geometry are the realized in-fence vocabulary, hand-rolled exactly as `Component/fastener#FASTENER_FAMILY` hand-keys the ISO 898-1 bolt property classes; the IFC accessory/fastener enum members are GeometryGym-verified at the `Rasm.Bim` egress (the `Rasm.Bim` folder `.api` `api-geometrygym-ifc`), the token a plain `string` here.
- Growth: the connector vocabulary grows by data — a new connector is one `ConnectorRow` catalogue entry carrying its type/gauge/install/carried-member/resisted-allowable columns, a new gauge one `SteelGauge` row carrying its base-metal/design thickness and gauge yield, a new connector class one `ConnectorType` row carrying its accessory/fastener tokens, resisted-direction set, and `ConnectorPlate` builder, a new attachment one `ConnectorInstall` row carrying its `FastenerSpec`, a new duration case one `LoadDurationFactor` row — never a per-connector type, never a per-type `Component` variant. A twist-strap or a coiled-strap connector is a new `ConnectorType` row reusing the `BuildStrap` body-form delegate, so multiple connector types share one plate body without a parallel geometry axis; a new resisted-load direction is one `LoadResistance` column shared by every connector (a value-object growth), never a per-direction connector type; a new host-materialization body form is one `ConnectorPlate` `[Union]` case plus one builder, never a per-feature plate type. The reinforcement/fastener/joint families carry their own vocabularies on their own pages; `anchor` folds as a `FastenerKind` arm on `Component/fastener#FASTENER_FAMILY`, so the framing connector never grows a fifth sibling family.
- Boundary: the connector vocabulary is the framing-connector `ComponentFamily` arm — a per-connector `Connector` class is the deleted form; `ConnectorSection` composes the `Rasm.Vectors` kernel `PositiveMagnitude` (the double-backed `> 0` finite magnitude) for every length/load column so the section never re-mints a primitive and a fractional cold-formed base thickness (`18 ga = 1.214 mm`, `12 ga = 2.657 mm`) and a fractional published allowable (`download = 7.83 kN`) admit without the truncation an int-backed `Dimension` count forces, the discrete sheet-steel gauge number admitting as the `Dimension` count carrier so a gauge is a designation never a length, the discrete fastener count (the `FastenerSpec.Quantity` the connector demands) admitting as a `Dimension` so a count is never a fractional magnitude; the connector resists a TYPED set of directions — the `LoadResistance` `[ComplexValueObject]` carries each direction's allowable AND a resisted-flag so `GoverningKn` reads ONLY the directions the connector actually resists rather than a degenerate min over a placeholder column, and `ConnectorType.Resists` is the type-level direction predicate the section validates the row against (a strap row declaring a download capacity rails `ComponentFault.Dimension`); the framing connector is a host-neutral capacity receipt — `ConnectorSection` carries the carried-member nominal fit, the resisted download/uplift/lateral loads, and the base gauge/thickness as `PositiveMagnitude` columns the `IfcDiscreteAccessory`/`IfcMechanicalFastener` wire and the structural-design seam read, NEVER a host brep, the host boundary materializes the stamped-plate solid from the `ConnectorPlate` `[Union]` receipt the `ConnectorType.BuildPlate` body-form delegate emits (the saddle U-seat/side-and-back-flange/nail-grid, the angle L-leg/bend/hole-grid, the strap length/width/hole-pattern, the anchor-plate seat/anchor-bolt-hole/post-grid), each derived from the section columns plus the cold-forming bend-radius and the fastener shank diameter, so this owner stays host-neutral exactly as the construction layout materializes a `Placement` tuple; the allowable-load receipt is the published manufacturer-evaluated capacity scaled by the wood-connector duration factor at `GovernedCapacity`, NOT a re-derived structural calculation — the design seam reads the `ConnectorCapacity` allowable the manufacturer evaluation report certifies, and the `SteelGauge` carries the AISI S100 gauge yield and the per-width axial section capacity the connector's net-section and bearing checks compose so the gauge columns are load-bearing (a connector's allowable is bounded by the gauge section, never a free number), never a connection re-analysis the seam re-runs; the appearance assignment crosses to `Appearance/graph#MATERIAL_LIBRARY` as the `MaterialId` (`metal.steel` for galvanized cold-formed steel) the row's `Component.AppearanceId` column carries, never a connector-specific shade; the mechanical capacity crosses to `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` (`YieldStrengthMpa`/`YoungsModulusMpa`) read by `MaterialId` through the `MaterialPropertySet` key, never re-derived here, so the `SteelGauge` base-metal band is the cold-formed designation, the gauge yield the AISI S100 spec-nominal cold-formed strength, and the measured grade the property-library receipt — the `AppearanceId` and the `CapacityKey` are INDEPENDENT `MaterialId` slots (the `component#COMPONENT_OWNER` two-slot law), a galvanized connector keeping its steel `CapacityKey` while its `AppearanceId` names a zinc-coat finish; `ComponentCatalogue.BuildConnectorRows` seeds the `component#COMPONENT_OWNER` `ComponentCatalogue.Rows` table with the framing-connector rows keyed `connector.<designation>` (`connector.jh-2x8-18ga`, `connector.holdown-hd5b-12ga`) under the `ComponentId` `family.designation` format, the dimensional columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)` and the gauge/fastener-count through `key.AcceptValidated<Dimension>(candidate:)` so a malformed row drops through `Choose` rather than seeding a degenerate `Component`; the placement of a connector schedule reads `Construction/layout#ASSEMBLY_FOLD` `StationStep` over the SAME realized fold, never a parallel connector-layout owner; the IFC entity class is the `Rasm.Bim` egress gate's choice (`ConnectorType.IfcAccessoryType` selecting `IfcDiscreteAccessory` for the fabricated saddle/bracket/anchorplate, `ConnectorType.IfcFastenerType` the attaching-fastener `IfcMechanicalFastener` token), this page emitting only the verified token columns, never an IFC entity.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;     // FrozenDictionary (the registered-row table)
using System.Globalization;          // CultureInfo.InvariantCulture (the LoadResistance.Validate format provider)
using LanguageExt;
using Rasm.Vectors;                  // PositiveMagnitude (>0 finite magnitude), Dimension (>=1 discrete count) — the kernel value-object atoms live in Rasm.Vectors, NOT Rasm.Domain
using Rasm.Domain;                   // Op (the boundary-admission key), the AcceptValidated admission extension, Context
using Rasm.Element;                  // MaterialId (the seam-carried material identity the Component AppearanceId/CapacityKey reference)
using Rasm.Materials.Component;      // Component/ComponentId/ComponentFamily/ComponentSection/ComponentFault (the parent COMPONENT_OWNER)
using Thinktecture;
using static LanguageExt.Prelude;

// Each Component family page is its OWN Rasm.Materials.Component.<Family> sub-namespace so the sibling
// `ComponentCatalogue` static classes are distinct types (a shared Rasm.Materials.Component is a CS0101 collision
// with component.md's own `ComponentCatalogue`); component#COMPONENT_OWNER stays the parent Rasm.Materials.Component and
// folds Connector.ComponentCatalogue.BuildConnectorRows by the sub-namespace-qualified name; parent types via the using
// above. LoadDurationFactor is the connector NDS Cd axis, disjoint from the EC5 timber LoadDuration (kmod): two codes
// over two factors under Rasm.Materials.Component, never conflated.
namespace Rasm.Materials.Component.Connector;

// --- [TYPES] -------------------------------------------------------------------------------
// ConnectorType carries the portable IFC tokens the Rasm.Bim egress gate reads to choose the IFC entity class: a fabricated
// cold-formed connector IS an IfcDiscreteAccessory (IfcDiscreteAccessoryTypeEnum SHOE for a joist-hanger saddle, BRACKET
// for an angle/strap clip, ANCHORPLATE for a shear-wall hold-down — the IFC4.3 member spellings the Bim egress IfcClass
// valid-set carries), itself FASTENED by mechanical fasteners (the GeometryGym-verified IfcMechanicalFastenerTypeEnum
// NAILPLATE for the nailplate-formed body, NAIL/SCREW/BOLT for the attaching fastener). Resists is the type-level direction
// predicate (which of download/uplift/lateral the connector develops), so a strap (uplift-only) never carries a fake
// download column. CarriesMember is true only for the saddle-seat hanger. BuildPlate is the host-materialization body-form
// delegate (the vocabulary item owns its plate geometry, so a new connector type names its body form by reusing an
// existing builder rather than growing a parallel form axis); the IfcAccessoryType BRACKET splitting into the Angle and
// Strap bodies is why the plate form is a per-type delegate, never derivable from the accessory token alone.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConnectorType {
    public static readonly ConnectorType JoistHanger  = new("joist-hanger",  ifcDesignation: "joist-hanger", ifcAccessoryType: "SHOE",        ifcFastenerType: "NAILPLATE", carriesMember: true,  resists: ResistanceFlags.Download | ResistanceFlags.Uplift,  BuildSaddle);
    public static readonly ConnectorType FramingAngle = new("framing-angle", ifcDesignation: "framing-angle", ifcAccessoryType: "BRACKET",     ifcFastenerType: "NAIL",      carriesMember: false, resists: ResistanceFlags.Uplift   | ResistanceFlags.Lateral, BuildAngle);
    public static readonly ConnectorType Strap        = new("strap",         ifcDesignation: "strap-tie",     ifcAccessoryType: "BRACKET",     ifcFastenerType: "NAILPLATE", carriesMember: false, resists: ResistanceFlags.Uplift,                            BuildStrap);
    public static readonly ConnectorType HoldDown     = new("hold-down",     ifcDesignation: "hold-down",     ifcAccessoryType: "ANCHORPLATE", ifcFastenerType: "BOLT",      carriesMember: false, resists: ResistanceFlags.Uplift,                            BuildAnchorPlate);
    public string IfcDesignation { get; }
    public string IfcAccessoryType { get; }   // IFC4.3 IfcDiscreteAccessoryTypeEnum member spelling (the Bim egress IfcClass valid-set reads — the connector physically IS this)
    public string IfcFastenerType { get; }     // GeometryGym-verified IfcMechanicalFastenerTypeEnum member spelling (the SEPARATE attaching fastener)
    public bool CarriesMember { get; }
    public ResistanceFlags Resists { get; }

    // The host-materialization body form: the vocabulary item owns its ConnectorPlate construction (FORM_CHOOSER row [04])
    // so the plate geometry is co-located with the connector class, derived from the section columns the delegate reads.
    [UseDelegateFromConstructor]
    public partial ConnectorPlate BuildPlate(ConnectorSection section);

    const double NailPitchMm = 19.05;     // 3/4in standard cold-formed connector hole-grid pitch
    const double BendFactor = 1.5;        // cold-form inside bend radius ≈ 1.5·t (AISI S100 minimum forming radius)
    const double HoleClearanceMm = 0.8;   // nail/screw hole over the fastener shank

    // The face-mount joist-hanger U-channel saddle: a seat cradling the joist width, two side flanges rising the joist
    // depth (nailed into the joist), and a back face-flange face-nailed to the supporting member; the host distributes the
    // HoleCount nailable holes at HolePitchMm over the side and back faces.
    static ConnectorPlate BuildSaddle(ConnectorSection s) => new ConnectorPlate.Saddle(
        SeatWidthMm: s.CarriedMemberWidthMm.Value,
        SeatDepthMm: Math.Max(38.1, s.CarriedMemberWidthMm.Value),
        SideFlangeHeightMm: s.CarriedMemberDepthMm.Value,
        BackFlangeWidthMm: 25.4 + s.BaseThicknessMm.Value * 6.0,
        SheetThicknessMm: s.BaseThicknessMm.Value,
        BendRadiusMm: s.BaseThicknessMm.Value * BendFactor,
        HoleDiameterMm: s.Install.Fastener.ShankDiameterMm + HoleClearanceMm,
        HoleCount: s.Install.Fastener.Quantity,
        HolePitchMm: NailPitchMm);

    // The framing-angle L-bend: two formed legs (the carried-member fit columns) at one bend, each leg a nailed/screwed face.
    static ConnectorPlate BuildAngle(ConnectorSection s) => new ConnectorPlate.Angle(
        LegAMm: s.CarriedMemberWidthMm.Value,
        LegBMm: s.CarriedMemberDepthMm.Value,
        WidthMm: Math.Max(38.1, s.CarriedMemberWidthMm.Value),
        SheetThicknessMm: s.BaseThicknessMm.Value,
        BendRadiusMm: s.BaseThicknessMm.Value * BendFactor,
        HoleDiameterMm: s.Install.Fastener.ShankDiameterMm + HoleClearanceMm,
        HoleCount: s.Install.Fastener.Quantity,
        HolePitchMm: NailPitchMm);

    // The flat tension strap: a long flat plate (length the member-depth column, width the member-width column) with a
    // nail grid; GaugeLines is the nail-column count the width admits (a wide strap stacks two gauge lines).
    static ConnectorPlate BuildStrap(ConnectorSection s) => new ConnectorPlate.Strap(
        LengthMm: s.CarriedMemberDepthMm.Value,
        WidthMm: s.CarriedMemberWidthMm.Value,
        SheetThicknessMm: s.BaseThicknessMm.Value,
        HoleDiameterMm: s.Install.Fastener.ShankDiameterMm + HoleClearanceMm,
        HoleCount: s.Install.Fastener.Quantity,
        HolePitchMm: NailPitchMm,
        GaugeLines: s.CarriedMemberWidthMm.Value >= 50.0 ? 2 : 1);

    // The shear-wall hold-down anchor body: a post seat with a cast-in anchor-rod hole (the larger clearance) and a
    // post-attachment bolt/screw hole grid (the FastenerSpec the connector demands).
    static ConnectorPlate BuildAnchorPlate(ConnectorSection s) => new ConnectorPlate.AnchorPlate(
        SeatWidthMm: s.CarriedMemberWidthMm.Value,
        SeatHeightMm: s.CarriedMemberDepthMm.Value,
        StandoffMm: s.BaseThicknessMm.Value * 2.0,
        SheetThicknessMm: s.BaseThicknessMm.Value,
        AnchorBoltHoleDiameterMm: s.Install.Fastener.ShankDiameterMm + 2.0,
        PostHoleDiameterMm: s.Install.Fastener.ShankDiameterMm + HoleClearanceMm,
        PostHoleCount: s.Install.Fastener.Quantity,
        HolePitchMm: NailPitchMm);
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

// ConnectorInstall carries the FASTENER SCHEDULE the connector demands — the nail/screw/bolt designation, the per-connector
// quantity, the single-fastener allowable, the fastener shank diameter (the ConnectorPlate hole-diameter source), and the
// verified IfcMechanicalFastenerTypeEnum token. DurationSensitive flags a wood-driven fastener (the Cd factor applies); a
// bolted hold-down passes unscaled.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConnectorInstall {
    public static readonly ConnectorInstall Nailed  = new("nailed",  fastener: FastenerSpec.Of("10d-common",   quantity: 10, perFastenerKn: 0.62, shankDiameterMm: 3.76,   ifcFastenerType: "NAIL"),  durationSensitive: true);
    public static readonly ConnectorInstall Screwed = new("screwed", fastener: FastenerSpec.Of("sd9-screw",    quantity: 8,  perFastenerKn: 1.05, shankDiameterMm: 4.50,   ifcFastenerType: "SCREW"), durationSensitive: true);
    public static readonly ConnectorInstall Bolted  = new("bolted",  fastener: FastenerSpec.Of("through-bolt", quantity: 2,  perFastenerKn: 18.0, shankDiameterMm: 15.875, ifcFastenerType: "BOLT"),  durationSensitive: false);
    public FastenerSpec Fastener { get; }
    public bool DurationSensitive { get; }
}

// The NDS load-duration factor (Cd) the wood-fastened connector's published allowable scales by — the NDS Table 2.3.2
// duration-of-load set keyed by the governing load case, disjoint from the EC5 timber LoadDuration (kmod): two distinct
// codes over distinct factors (NDS Cd vs EC5 kmod) under Rasm.Materials.Component, never conflated. GovernedCapacity
// scales a duration-sensitive (nailed/screwed-into-wood) connector by this Cd and passes a bolted hold-down unscaled;
// a new load case is one row, never a per-case scale literal.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LoadDurationFactor {
    public static readonly LoadDurationFactor Permanent   = new("permanent",    cd: 0.90);
    public static readonly LoadDurationFactor TenYear     = new("ten-year",     cd: 1.00);
    public static readonly LoadDurationFactor TwoMonth    = new("two-month",    cd: 1.15);
    public static readonly LoadDurationFactor SevenDay    = new("seven-day",    cd: 1.25);
    public static readonly LoadDurationFactor WindSeismic = new("wind-seismic", cd: 1.60);
    public static readonly LoadDurationFactor Impact      = new("impact",       cd: 2.00);
    public double Cd { get; }
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-connector fastener schedule: the discrete fastener designation/count, the single-fastener allowable, and the
// shank diameter (the ConnectorPlate hole-diameter derivation reads it), so the connector's fastener-governed capacity
// (Quantity * PerFastenerKn) is a derived bound GovernedCapacity caps the published allowable against — a connector is
// governed by the LESSER of its steel-section allowable and its fastener group. The verified IfcMechanicalFastenerTypeEnum
// token rides to the Rasm.Bim egress for the attaching-fastener entity.
public readonly record struct FastenerSpec(string Designation, int Quantity, double PerFastenerKn, double ShankDiameterMm, string IfcFastenerType) {
    public static FastenerSpec Of(string designation, int quantity, double perFastenerKn, double shankDiameterMm, string ifcFastenerType) =>
        new(designation, quantity, perFastenerKn, shankDiameterMm, ifcFastenerType);
    public double GroupAllowableKn => Quantity * PerFastenerKn;
}

// The typed three-direction allowable receipt — each direction's published manufacturer allowable AND whether the
// connector resists it. A strap carries Uplift alone (Download/Lateral Resisted=false, allowable irrelevant), so Governing
// reads ONLY the resisted directions rather than a degenerate min over a fake column. The [ComplexValueObject] guards each
// resisted allowable > 0 at Create, so an unrepresentable connector (a resisted direction with a non-positive allowable, or
// a connector resisting nothing) can never construct.
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

// The host-materialization body the host boundary folds into the stamped-and-formed steel-plate solid — one [Union] over
// the four cold-formed body forms (saddle/angle/strap/anchor-plate), each carrying the GENERATIVE geometry a generator
// builds the real solid from (no scalar smear, no flat receipt forcing a strap into a saddle's seat columns). NEVER a host
// brep here: a connector is one ConnectorPlate the Rasm.Bim wire serializes and the host plug-in materializes face-by-face,
// exactly as RebarBend is the bent-bar receipt the host lofts. Every field derives from the ConnectorSection columns plus
// the cold-forming bend-radius and the FastenerSpec shank diameter, so the geometry traces to the section, never a literal.
[Union]
public abstract partial record ConnectorPlate {
    public sealed record Saddle(double SeatWidthMm, double SeatDepthMm, double SideFlangeHeightMm, double BackFlangeWidthMm, double SheetThicknessMm, double BendRadiusMm, double HoleDiameterMm, int HoleCount, double HolePitchMm) : ConnectorPlate;
    public sealed record Angle(double LegAMm, double LegBMm, double WidthMm, double SheetThicknessMm, double BendRadiusMm, double HoleDiameterMm, int HoleCount, double HolePitchMm) : ConnectorPlate;
    public sealed record Strap(double LengthMm, double WidthMm, double SheetThicknessMm, double HoleDiameterMm, int HoleCount, double HolePitchMm, int GaugeLines) : ConnectorPlate;
    public sealed record AnchorPlate(double SeatWidthMm, double SeatHeightMm, double StandoffMm, double SheetThicknessMm, double AnchorBoltHoleDiameterMm, double PostHoleDiameterMm, int PostHoleCount, double HolePitchMm) : ConnectorPlate;
}

// The duration-adjusted allowable receipt the structural-connection-design seam reads — each resisted direction scaled by
// Cd (a wood-fastened connector) and capped by the fastener-group/gauge bounds, plus the governing capacity and the applied
// Cd so the seam reads the connector's adjusted strength in one shape.
public readonly record struct ConnectorCapacity(double DownloadKn, double UpliftKn, double LateralKn, double GoverningKn, double Cd);

// The applied connector demand (a load combination's factored download/uplift/lateral) the DemandCapacityRatio unit-check reads.
public readonly record struct LoadDemand(double DownloadKn, double UpliftKn, double LateralKn);

public readonly record struct ConnectorSection(
    ConnectorType Type,
    SteelGauge Gauge,
    ConnectorInstall Install,
    Dimension GaugeNumber,
    Dimension FastenerCount,
    PositiveMagnitude CarriedMemberWidthMm,
    PositiveMagnitude CarriedMemberDepthMm,
    PositiveMagnitude BaseThicknessMm,
    LoadResistance Allowable) {

    // The FINISH the appearance projection reads (galvanized cold-formed steel), INDEPENDENT from CapacityKey by the
    // component#COMPONENT_OWNER two-slot law — a connector with a distinct mill finish keeps its steel CapacityKey.
    public MaterialId AppearanceId => MaterialId.Of("metal.steel");

    // The CAPACITY material whose properties#MATERIAL_PROPERTY_CATALOGUE Mechanical row the design seam reads — the
    // connector's cold-formed structural STEEL (metal.steel), sourced INDEPENDENTLY from the AppearanceId finish.
    public MaterialId CapacityKey => MaterialId.Of("metal.steel");

    // The per-fastener group bound and the gauge net-section axial bound the published allowable is capped against, so the
    // gauge and fastener columns are load-bearing: a connector can never develop more than its weakest physical bound.
    public double FastenerGroupAllowableKn => Install.Fastener.GroupAllowableKn;
    public double GaugeAxialBoundKn => Gauge.AxialSectionCapacityKnPerMm * CarriedMemberWidthMm.Value;

    // The host-materialization receipt: the connector type's body-form delegate builds its ConnectorPlate arm from the
    // section columns, so the saddle/angle/strap/anchor-plate geometry is co-located with the connector class, never a smear.
    public ConnectorPlate Plate => Type.BuildPlate(this);

    // The governing allowable over ONLY the resisted directions, capped by the fastener-group and gauge bounds, scaled by
    // Cd for a duration-sensitive (wood-fastened) connector. Fin rails a connector whose every physical bound has collapsed
    // (a transcription gap) — NOT the dead `> 0` guard a PositiveMagnitude column already guarantees, so the fault is a
    // capacity-solve degenerate (ComponentFault.Capacity), distinct from the row-admission Dimension fault ResistanceOf rails.
    public Fin<ConnectorCapacity> GovernedCapacity(LoadDurationFactor duration, Op key) =>
        from bounded in guard(
            Allowable.GoverningKn > 0.0 && FastenerGroupAllowableKn > 0.0 && GaugeAxialBoundKn > 0.0,
            ComponentFault.Capacity(key, $"<degenerate-connector-bound:{Type.Key}:{Gauge.Key}:gov={Allowable.GoverningKn:R}/grp={FastenerGroupAllowableKn:R}/sec={GaugeAxialBoundKn:R}>"))
        let cd = Install.DurationSensitive ? duration.Cd : 1.0
        let bound = Math.Min(FastenerGroupAllowableKn, GaugeAxialBoundKn)
        let download = Math.Min(Allowable.Download, bound) * cd
        let uplift = Math.Min(Allowable.Uplift, bound) * cd
        let lateral = Math.Min(Allowable.Lateral, bound) * cd
        let governing = Math.Min(Allowable.GoverningKn, bound) * cd
        select new ConnectorCapacity(download, uplift, lateral, governing, cd);

    // The connector unit-check: the max of the per-direction demand/allowable ratios (a zero allowable in a direction the
    // connector does not resist drives the ratio to infinity, flagging a misapplied connector). > 1 is overstressed.
    public double DemandCapacityRatio(ConnectorCapacity capacity, LoadDemand demand) => Math.Max(
        Ratio(demand.DownloadKn, capacity.DownloadKn),
        Math.Max(Ratio(demand.UpliftKn, capacity.UpliftKn), Ratio(demand.LateralKn, capacity.LateralKn)));

    static double Ratio(double demand, double allowable) =>
        demand <= 0.0 ? 0.0 : allowable <= 0.0 ? double.PositiveInfinity : demand / allowable;
}

public readonly record struct ConnectorRow(
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
public static class ComponentCatalogue {
    // ICC-ES / manufacturer-evaluation-report ten-year-duration allowables; a load direction the connector does not resist
    // carries a 0.0 column (ConnectorType.Resists is the truth, the column ignored when unresisted). Joist hangers resist
    // download + uplift; framing angles uplift + lateral; straps and hold-downs uplift only.
    static readonly Seq<ConnectorRow> ConnectorRows = Seq(
        new ConnectorRow("connector.jh-2x6-18ga",        "joist-hanger",  "18ga", "nailed",   38.1,  139.7,  4.85,  0.78,  0.0),
        new ConnectorRow("connector.jh-2x8-18ga",        "joist-hanger",  "18ga", "nailed",   38.1,  184.2,  7.83,  0.78,  0.0),
        new ConnectorRow("connector.jh-2x10-16ga",       "joist-hanger",  "16ga", "nailed",   38.1,  235.0, 10.45,  1.00,  0.0),
        new ConnectorRow("connector.jh-2x12-16ga",       "joist-hanger",  "16ga", "nailed",   38.1,  285.8, 12.46,  1.00,  0.0),
        new ConnectorRow("connector.jh-2x14-16ga",       "joist-hanger",  "16ga", "nailed",   38.1,  336.6, 13.50,  1.00,  0.0),
        new ConnectorRow("connector.jh-4x8-14ga",        "joist-hanger",  "14ga", "screwed",  88.9,  184.2, 14.23,  1.33,  0.0),
        new ConnectorRow("connector.jh-4x10-14ga",       "joist-hanger",  "14ga", "screwed",  88.9,  235.0, 17.79,  1.33,  0.0),
        new ConnectorRow("connector.jh-4x12-14ga",       "joist-hanger",  "14ga", "screwed",  88.9,  285.8, 20.50,  1.33,  0.0),
        new ConnectorRow("connector.jh-6x10-12ga",       "joist-hanger",  "12ga", "screwed", 139.7,  235.0, 26.70,  1.78,  0.0),
        new ConnectorRow("connector.angle-a23-18ga",     "framing-angle", "18ga", "nailed",   38.1,   38.1,  0.0,   2.45,  1.45),
        new ConnectorRow("connector.angle-a35-18ga",     "framing-angle", "18ga", "nailed",   38.1,   38.1,  0.0,   3.96,  2.18),
        new ConnectorRow("connector.angle-l50-16ga",     "framing-angle", "16ga", "nailed",   50.8,   50.8,  0.0,   4.60,  2.55),
        new ConnectorRow("connector.angle-l70-16ga",     "framing-angle", "16ga", "screwed",  63.5,   63.5,  0.0,   5.74,  3.11),
        new ConnectorRow("connector.angle-l90-14ga",     "framing-angle", "14ga", "screwed",  88.9,   88.9,  0.0,   8.45,  4.60),
        new ConnectorRow("connector.strap-pa18-18ga",    "strap",         "18ga", "nailed",   38.1,  228.6,  0.0,   4.20,  0.0),
        new ConnectorRow("connector.strap-cs16-16ga",    "strap",         "16ga", "nailed",   38.1,  600.0,  0.0,   9.34,  0.0),
        new ConnectorRow("connector.strap-cs14-14ga",    "strap",         "14ga", "nailed",   50.8,  900.0,  0.0,  14.50,  0.0),
        new ConnectorRow("connector.strap-mst27-14ga",   "strap",         "14ga", "nailed",   50.8,  685.8,  0.0,  18.30,  0.0),
        new ConnectorRow("connector.strap-cmst16-16ga",  "strap",         "16ga", "nailed",   63.5, 1219.2,  0.0,  24.50,  0.0),
        new ConnectorRow("connector.holdown-hd5b-12ga",  "hold-down",     "12ga", "bolted",   63.5,  254.0,  0.0,  23.13,  0.0),
        new ConnectorRow("connector.holdown-hdu8-12ga",  "hold-down",     "12ga", "bolted",   63.5,  298.5,  0.0,  29.30,  0.0),
        new ConnectorRow("connector.holdown-hd12-10ga",  "hold-down",     "10ga", "bolted",   76.2,  330.0,  0.0,  53.38,  0.0),
        new ConnectorRow("connector.holdown-hdu14-10ga", "hold-down",     "10ga", "bolted",   76.2,  384.2,  0.0,  65.40,  0.0));

    static Fin<(ComponentId Id, Component Item)> ConnectorOf(ConnectorRow r, Context context, Op key) =>
        from type in ConnectorType.TryGet(r.Type, out ConnectorType? t) ? Fin.Succ(t!) : Fin.Fail<ConnectorType>(ComponentFault.Designation(key, $"<unknown-connector-type:{r.Type}>"))
        from gauge in SteelGauge.TryGet(r.Gauge, out SteelGauge? g) ? Fin.Succ(g!) : Fin.Fail<SteelGauge>(ComponentFault.Grade(key, $"<unknown-gauge:{r.Gauge}>"))
        from install in ConnectorInstall.TryGet(r.Install, out ConnectorInstall? i) ? Fin.Succ(i!) : Fin.Fail<ConnectorInstall>(ComponentFault.Designation(key, $"<unknown-install:{r.Install}>"))
        from resistance in ResistanceOf(type, r, key)
        from gaugeNo in key.AcceptValidated<Dimension>(candidate: gauge.GaugeNumber)
        from fastenerCount in key.AcceptValidated<Dimension>(candidate: install.Fastener.Quantity)
        from width in key.AcceptValidated<PositiveMagnitude>(candidate: r.CarriedMemberWidthMm)
        from depth in key.AcceptValidated<PositiveMagnitude>(candidate: r.CarriedMemberDepthMm)
        from thickness in key.AcceptValidated<PositiveMagnitude>(candidate: gauge.BaseThicknessMm)
        let section = new ConnectorSection(type, gauge, install, gaugeNo, fastenerCount, width, depth, thickness, resistance)
        from item in Component.Of(ComponentFamily.Connector, r.Designation, new ComponentSection.Connector(section), section.CapacityKey, section.AppearanceId, key)
        select (ComponentId.Of(r.Designation), item);

    // The row's per-direction allowables admit through the LoadResistance [ComplexValueObject] gated by the connector's
    // type-level Resists predicate, so a strap row carrying a download column (or a joist-hanger row missing its download)
    // rails ComponentFault.Dimension rather than seeding a connector that resists a direction it has no published value for —
    // a malformed/non-positive section load column is a Dimension fault, the Capacity case reserved for the GovernedCapacity solve.
    static Fin<LoadResistance> ResistanceOf(ConnectorType type, ConnectorRow r, Op key) =>
        LoadResistance.Validate(
            downloadKn: r.DownloadKn, resistsDownload: type.Resists.HasFlag(ResistanceFlags.Download),
            upliftKn: r.UpliftKn, resistsUplift: type.Resists.HasFlag(ResistanceFlags.Uplift),
            lateralKn: r.LateralKn, resistsLateral: type.Resists.HasFlag(ResistanceFlags.Lateral),
            provider: CultureInfo.InvariantCulture, item: out LoadResistance? built) is { } error
            ? Fin.Fail<LoadResistance>(ComponentFault.Dimension(key, $"<resistance-mismatch:{type.Key}:{r.Designation}:{error.Message}>"))
            : Fin.Succ(built!.Value);

    public static FrozenDictionary<ComponentId, Component> BuildConnectorRows(Context context) =>
        ConnectorRows
            .Choose(row => ConnectorOf(row, context, default).ToOption())
            .ToFrozenDictionary(static r => r.Id, static r => r.Item, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[RESEARCH]

- [CONNECTOR_ROW_TRANSCRIPTION]: REALIZED — the cold-formed framing-connector catalogue (the joist-hanger face-mount saddles over the 2x6..6x10 carried-member sizes, the A23/A35/L-series framing angles, the PA/CS/MST/CMST-series flat straps, and the HD/HDU-series shear-wall hold-downs) seeds through `ComponentCatalogue.BuildConnectorRows(context)` over the `ConnectorRow` designation/type/gauge/install table, the carried-member columns admitting once through `key.AcceptValidated<PositiveMagnitude>(candidate:)`, the discrete sheet-steel gauge and fastener count through `key.AcceptValidated<Dimension>(candidate:)`, and the per-direction allowables through the `LoadResistance` `[ComplexValueObject]` gated by the `ConnectorType.Resists` type-level direction predicate, into the kernel value-objects; a new connector is one `ConnectorRow` data addition plus, if novel, one `ConnectorType`/`SteelGauge`/`ConnectorInstall` row. The catalogue is HAND-ROLLED in-fence because VividOrange owns the structural-MEMBER section catalogues (AISC v16.0 + EN 10365) and EN grade factories, NOT the manufacturer-proprietary framing-connector range (the joist-hanger/angle/strap/hold-down hardware ICC-ES evaluation reports certify), exactly as `Component/fastener#FASTENER_FAMILY` hand-keys the ISO 898-1 bolt property classes VividOrange's EN-only member-grade data does not own. The `SteelGauge.BaseThicknessMm` columns transcribe the manufacturer-uncoated cold-formed sheet-steel base-metal thicknesses (18 ga = 1.214 mm, 16 ga = 1.519 mm, 14 ga = 1.897 mm, 12 ga = 2.657 mm, 10 ga = 3.416 mm) and `DesignThicknessMm` the AISI S100 design (as-formed) thickness (≈0.90·base for galvanized stock); the `SteelGauge.YieldMpa` is the AISI S100 cold-formed minimum yield the gauge band implies (SS Grade 33 ≈ 230 MPa for the light 18/16 ga stock, SS Grade 50 ≈ 340 MPa for the structural 14/12/10 ga stock), so `AxialSectionCapacityKnPerMm = YieldMpa · DesignThicknessMm · 1e-3` is the per-unit-width nominal axial section strength the connector net-section/bearing bound composes; the `DownloadKn`/`UpliftKn`/`LateralKn` columns transcribe the published manufacturer-evaluation-report (ICC-ES) ten-year-duration allowable loads, a direction the connector does not resist carrying a `0.0` column the `Resists` predicate ignores; the `GovernedCapacity` fold reads ONLY the resisted directions, caps each by the LESSER of the fastener-group allowable (`FastenerSpec.Quantity · PerFastenerKn`) and the gauge axial bound (`GaugeAxialBoundKn`), and scales by the NDS wood-connector duration factor (`Cd` ∈ {0.90 permanent, 1.00 ten-year, 1.15 two-month, 1.25 seven-day, 1.60 wind-seismic, 2.00 impact}) for a nailed/screwed connector into wood, passing the bolted hold-down allowable through unscaled (`DurationSensitive == false`).
- [CONNECTOR_DESIGN_CHECK]: REALIZED — the connector capacity is the LEAST of three physical bounds, so the `SteelGauge` and `ConnectorInstall` columns are load-bearing rather than decorative: the published manufacturer allowable (`LoadResistance`), the fastener-group allowable (`ConnectorInstall.Fastener.GroupAllowableKn = Quantity · PerFastenerKn`, the nail/screw/bolt group the connector demands), and the gauge net-section axial bound (`Gauge.AxialSectionCapacityKnPerMm · CarriedMemberWidthMm`, the AISI S100 section strength the connector body develops). `ConnectorSection.GovernedCapacity` caps the published allowable against the lesser of the fastener and gauge bounds so a connector under-fastened or under-gauged for its catalogue allowable governs at the physical bound, never a free catalogue number; `ConnectorSection.DemandCapacityRatio(capacity, demand)` is the unit-check the connector schedule reads (the max of the per-direction demand/allowable ratios, a demand in an unresisted direction driving the ratio to infinity so a misapplied connector — a download on a strap — is flagged, > 1 an overstressed connector the design seam reports). The design check is the `ConnectorSection` projection, never a `Rasm.Compute` re-analysis (the seam reads the bound, never re-derives the manufacturer evaluation); the row-admission resistance fault rails `ComponentFault.Dimension` (a malformed section load column) and the capacity-solve degenerate rails `ComponentFault.Capacity`, the two fault cases disjoint by construction so a transcription gap and a solve collapse are distinguishable on the rail.
- [CONNECTOR_HOST_MATERIALIZATION]: REALIZED — the `ConnectorPlate` `[Union]` over the four cold-formed body forms (`Saddle`/`Angle`/`Strap`/`AnchorPlate`) is the host-neutral connector geometry the `ConnectorSection.Plate => Type.BuildPlate(this)` body-form delegate emits — one `[Union]` arm per body form so a strap and a hold-down never collapse into a saddle's seat/back/flange columns, the scalar smear the complete-generative-data law forbids. Each body form carries the GENERATIVE geometry a generator builds the real stamped-and-formed solid from: the `Saddle` U-seat (seat width/depth, side-flange height = carried-member depth, back face-flange width, sheet thickness, cold-form bend radius, nail-hole diameter/count/pitch); the `Angle` L-bend (two formed legs the carried-member fit columns, bend radius, hole grid); the `Strap` flat plate (length, width, gauge-line count the width admits, hole pattern); the `AnchorPlate` shear-wall body (post seat, cast-in anchor-rod hole, standoff, post-attachment hole grid). Every field derives from the `ConnectorSection` columns plus the cold-forming inside-bend radius (`≈ 1.5·t`) and the `FastenerSpec.ShankDiameterMm` (the hole diameter source), so the geometry traces to the section, never an unbacked literal; the `ConnectorType` carries its body-form builder as a `[UseDelegateFromConstructor]` column, so a new connector type names its body form by reusing an existing builder (a twist-strap reusing `BuildStrap`) and the `IfcDiscreteAccessoryTypeEnum` BRACKET splitting into the distinct Angle and Strap bodies stays expressible. This owner NEVER constructs a host brep: the `ConnectorPlate` is the portable schedule data the `Rasm.Bim` wire serializes and the host plug-in materializes, so the connector catalogue stays a leaf below the host boundary; a connector schedule (a joist-to-beam saddle run, a shear-wall hold-down stack) is a `ConnectorSection` per connector (its `Plate` the host solid) plus a `Construction/layout#ASSEMBLY_FOLD` station-stepped placement, never a per-connector host solid type.
- [IFC_DISCRETE_ACCESSORY_WIRE]: a framing connector round-trips to IFC 4.3 as an `IfcDiscreteAccessory` — the fabricated cold-formed saddle/bracket/anchorplate carrying `PredefinedType` ∈ `IfcDiscreteAccessoryTypeEnum` {SHOE (the joist-hanger saddle), BRACKET (the framing-angle/strap clip), ANCHORPLATE (the shear-wall hold-down), the connector classes the `ConnectorType.IfcAccessoryType` token names} — itself FASTENED by mechanical fasteners that round-trip as `IfcMechanicalFastener` carrying `PredefinedType` ∈ `IfcMechanicalFastenerTypeEnum` {NAILPLATE (the nailplate-formed connector body), NAIL/SCREW/BOLT (the attaching fastener the `ConnectorInstall.Fastener.IfcFastenerType` token names)}. The `ConnectorType.IfcAccessoryType` token is the IFC4.3 `IfcDiscreteAccessoryTypeEnum` member spelling and the `IfcFastenerType` the GeometryGym-verified `IfcMechanicalFastenerTypeEnum` spelling the `Rasm.Bim` egress gate reads to choose the IFC entity class (a fabricated connector IS an `IfcDiscreteAccessory`, the attaching nailplate/nails an `IfcMechanicalFastener` related through `IfcRelConnectsWithRealizingElements`); the `ConnectorType.IfcDesignation` is the `ObjectType` discriminant the federation reads when the predefined enum lacks the precise connector class. The connector is keyed to the `properties#MATERIAL_PROPERTY_CATALOGUE` `Mechanical` `YieldStrengthMpa` of its base-steel `MaterialId` (`metal.steel`) the cold-formed grade asserts, the `SteelGauge.YieldMpa` the spec-nominal cold-formed band, and the allowable-load receipt rides as the structural design property the analysis federation reads. The wire mapping is the `Rasm.Bim` boundary projection, host-neutral here — this page emits the verified token columns and the scalar capacity/`ConnectorPlate` columns the `Projection/component#COMPONENT_PROJECTOR` `ComponentProjector` lowers into the neutral realizing-detail bag (the Element-declared neutral `DetailSchema` over the canonical `PropertyName` vocabulary; the IFC `Rasm_ConnectionRealization` Pset name stays a `Rasm.Bim` egress concern), never an IFC entity and never a second carrier. Ripple counterpart: `Projection/component#COMPONENT_PROJECTOR`'s connector arm lowers the `AccessoryType` row (= the `IfcDiscreteAccessory` `PredefinedToken`), the SEPARATE `FastenerType` row (`ConnectorType.IfcFastenerType` — the attaching `IfcMechanicalFastener` token, never a hardcoded `"BOLT"`), and the `CarriedMemberWidth`/`CarriedMemberDepth` measured columns, and `Rasm.Bim` `Semantics/connection#CONNECTION_DETAIL` reads/round-trips the IDENTICAL bag at IFC ingress/`Emit` so a nailed joist hanger round-trips `NAILPLATE`/`NAIL` and a bolted hold-down `ANCHORPLATE`/`BOLT`.
