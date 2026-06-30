# [BIM_IFC_TAXONOMY]

The IFC entity-class vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner: the `IfcClass` `[SmartEnum<string>]` buildingSMART entity-class breadth keyed on the IFC entity-type string, each row carrying its `IfcDomain` discipline partition, its frozen valid-predefined set, and its `IntroducedIn`/`RemovedIn` schema span; the `AdmitPredefined` egress gate validating a predefined token against the row's valid set AND the target schema; and the `IfcRepresentation` content-keyer projecting an IFC product's representations onto the seam `RepresentationContentHash` map. This page is the IFC-schema authority the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes at BOTH legs — at ingress it resolves the entity-type string to the row that supplies the generic `Classification("ifc", classKey)` stamped on the seam `Object` node and admits the predefined token, and at egress it runs the `PredefinedType` validity gate against the frozen valid set and the schema span before the IFC entity is authored [C6][H8]. The retired `BimElement` element record and `BimModel` collection are GONE: the consumer-facing element is the seam `Bake(objectNode)` fold over the reachable `ElementGraph` subgraph, never a second stored record keyed by `GlobalId`, so "has it all" is one flat read on the seam graph and the typed data is never stranded off the element. The vocabulary is ONE owner widened on three folded axes — a new entity class is one `IfcClass` row, a new sub-class kind is one valid-predefined entry, a new schema availability is one span column — never three parallel surfaces and never a per-element-class type.

## [01]-[INDEX]

- [01]-[IFC_CLASS]: `IfcClass` `[SmartEnum<string>]` entity-class vocabulary, the `IfcDomain` discipline partition, the frozen valid-predefined set, the `IntroducedIn`/`RemovedIn` schema span [H8], the `Resolve` ingress lookup, and the `AdmitPredefined` egress gate over the seam-owned `PredefinedType` value-object [C6].
- [02]-[REPRESENTATION_KEYS]: `IfcRepresentation` the geometry-reference content-keyer projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map (axis/body/box/footprint → kernel `XxHash128` content hash) [M2], composing the kernel seed-zero `ContentHash.Of` identity, never a second hasher.

## [02]-[IFC_CLASS]

- Owner: `IfcClass` the `[SmartEnum<string>]` closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string, each row carrying its `IfcDomain` discipline, its frozen `ValidPredefined` set, and its seam-owned `SchemaSpan` (`IntroducedIn`/`RemovedIn` `ReleaseVersion`) so a class authored against a schema that does not carry it faults at egress; `IfcDomain` the seven-case buildingSMART discipline partition; the `SchemaSpan` (`Graph/element#NODE_MODEL`) the per-class availability window [H8] over the seam `ReleaseVersion` the model `Header` carries — the SAME window record the projector stamps onto a node at ingress, never a parallel Bim copy, with `IfcSchema` owning only the IFC release chronology and the `Covers` gate the class window and the stamped node span share. The `PredefinedType` sub-class discriminant is the seam-carried `PredefinedType` `[ValueObject<string>]` on the `Object` node (`Rasm.Element/Graph/element#NODE_MODEL`), of which `IfcClass` is the IFC validation authority — the predefined-type semantics live HERE (the frozen valid sets + `AdmitPredefined`), the seam node carrying only the typed token value-object [C6].
- Entry: `IfcClass.Resolve(string entityType, Op key)` is the strict ingress lookup the projector composes — it `Canonical`-folds the deprecated `*StandardCase`/`*ElementedCase` subtypes onto their base row, then resolves the IFC entity-type string (the `ParserIfc.IdentifyIfcClass` class half) to the row that supplies the generic `Classification("ifc", row.Key)` stamped on the seam `Object` node, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` on a class the vocabulary does not carry — the typed case (band 2600, `Expected`-derived) lifting BARE onto the `Fin` rail with no `.ToError()` hop; a projector that prefers a permissive ingress instead reads `IfcClass.TryGet(entityType).IfNone(IfcClass.Proxy)` so an unrostered IFC4 leaf lands the `Proxy` row rather than aborting the import (the `Semantics/geospatial#GEOSPATIAL_SEAM` resilience idiom), the two paths sharing the ONE generated `TryGet` and never a parallel resolver. `IfcClass.AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key)` is the egress gate — it admits the predefined token against the row's frozen valid set with the `USERDEFINED` fallback AND validates the class is available in the target `Header` schema span, returning the validated IFC predefined token the projector authors (the seam `Object` node carrying the `PredefinedType` value-object) or faulting `BimFault.UnmappedClass`.
- Auto: `Resolve` reads the `Items` SmartEnum table by key through `TryGet`; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair (`"ifc"`, `"IfcWall"`) rather than the `IfcClass` type itself, keeping the seam IFC-schema-free; `AdmitPredefined` trims and upper-cases the token, routes `""`/`"NOTDEFINED"` to the canonical `PredefinedType.NotDefined.Token`, routes `"USERDEFINED"` to the validated `USERDEFINED` marker requiring a non-empty `objectType` label (the projector authors the IFC `ObjectType` from it — there is no `OBJECTTYPE` token), accepts a token in the frozen `ValidPredefined` set (or any token when the set is empty, the schema not constraining it), and otherwise faults; the schema-span check rejects a class whose `IntroducedIn` is later than (or `RemovedIn` not later than) the target `schema` so an IFC4.3 `IfcAlignment` authored against an IFC2x3 emit faults rather than writing an entity the schema forbids [H8], comparing a frozen chronological rank because `ReleaseVersion` is a `[SmartEnum]` whose declaration order is not chronological; the admitted predefined token folds into the seam node content hash through the seam `Node.ToCanonicalBytes` [C6].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new entity class is one `IfcClass` row carrying its domain, valid-predefined set, and schema span; a new sub-class kind is one frozen entry in the row's valid set; a new schema availability is one `IfcSchemaSpan` column; a new IFC4.4 infrastructure class rides the same ingress/egress on one new row; never a per-element-class type, never a `Get<Domain>` family, and never the retired `BimElement` record.
- Boundary: `BimElement` and `BimModel` are RETIRED — the consumer element is the seam `Bake(objectNode)` fold over the `ElementGraph`, and any owner that re-stores a `BimElement(GlobalId, IfcClass, …)` record off the seam graph is the deleted form; `IfcClass` is the IFC-schema vocabulary the projector composes, NOT a field on a seam node — the seam `Object` node carries the generic `Classification("ifc", code)` value-object and a typed `IfcClass` on the node is the named seam violation [C6]; the `PredefinedType` is a seam `[ValueObject<string>]` on the `Object` node (its `.Token` the string) and `IfcClass.AdmitPredefined` is its IFC validation authority — a Bim-owned `PredefinedType` type is the deleted form (the seam owns the value-object; Bim owns only the valid-set + `AdmitPredefined`), and a per-call predefined regex instead of the frozen valid set is the named defect; the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the valid set + the schema span [C6][H8]) and silent acceptance of an out-of-schema predefined is the named defect; the schema span compares the seam `ReleaseVersion` the `Header` carries (the `Graph/element#NODE_MODEL` model currency) through a frozen chronological rank and a bare `>=` over the SmartEnum or a leak of the GeometryGym `ReleaseVersion` enum into the gate signature is the named defect; the entity-type strings ground against the GeometryGym entity vocabulary consumed as settled (`.api/api-geometrygym-ifc`), and a raw entity-type string crossing a seam signature is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                            // the kernel operation key each typed BimFault case carries
using ReleaseVersion = Rasm.Element.ReleaseVersion;   // the seam schema currency the Header carries — disambiguated
                                                      // from GeometryGym.Ifc.ReleaseVersion (the IFC-text codec leg
                                                      // semantic.Sniff/Emit owns), which this page never touches.

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
public enum IfcDomain : byte {
    Architecture = 0, Structural = 1, HvacFire = 2, Electrical = 3, Plumbing = 4, Infrastructure = 5, Geotechnical = 6,
}

// --- [TABLES] -----------------------------------------------------------------------------
// The per-class schema availability window is the SEAM-owned `SchemaSpan` (`Graph/element#NODE_MODEL`):
// the seam owns the [IntroducedIn, RemovedIn) window record the projector stamps onto a node at ingress
// [H8], so the IfcClass row's class window and the stamped node span are ONE type — never a parallel Bim
// copy. Bim owns only the IFC release chronology the seam [SmartEnum] declaration order does not encode
// and the `Covers` gate the class window and the node span SHARE. The IFC4.3 infrastructure classes
// (IfcAlignment/IfcRoad/IfcBridge/IfcEarthworks*) are unavailable in IFC2x3/IFC4, so the gate ranks
// chronologically (the SmartEnum has no ordinal and is not declared in release order), never bare `>=`.
internal static class IfcSchema {
    public static readonly SchemaSpan Ifc2x3 = SchemaSpan.From(ReleaseVersion.Ifc2X3);
    public static readonly SchemaSpan Ifc4   = SchemaSpan.From(ReleaseVersion.Ifc4);
    public static readonly SchemaSpan Ifc4x3 = SchemaSpan.From(ReleaseVersion.Ifc4X3);

    static readonly FrozenDictionary<string, int> Chronology = new Dictionary<string, int> {
        [ReleaseVersion.Ifc2X3.Key] = 0, [ReleaseVersion.Ifc4.Key] = 1, [ReleaseVersion.Ifc4X1.Key] = 2,
        [ReleaseVersion.Ifc4X3.Key] = 3, [ReleaseVersion.Ifc4X3Add2.Key] = 4, [ReleaseVersion.Ifc5.Key] = 5,
    }.ToFrozenDictionary();

    static int Rank(ReleaseVersion v) => Chronology.GetValueOrDefault(v.Key, int.MaxValue);

    // `Covers` extends the seam SchemaSpan so the SAME availability gate validates an IfcClass row's class
    // window AND a stamped node span at Emit, ranked chronologically because the SmartEnum carries no ordinal.
    extension(SchemaSpan span) {
        public bool Covers(ReleaseVersion schema) =>
            Rank(schema) >= Rank(span.IntroducedIn) && span.RemovedIn.Match(Some: removed => Rank(schema) < Rank(removed), None: () => true);
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class IfcClass {
    public static readonly IfcClass Wall          = new("IfcWall", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("STANDARD", "POLYGONAL", "SHEAR", "ELEMENTEDWALL", "PLUMBINGWALL", "MOVABLE", "PARAPET", "PARTITIONING", "SOLIDWALL", "RETAININGWALL", "WAVEWALL"));
    public static readonly IfcClass Slab          = new("IfcSlab", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("FLOOR", "ROOF", "LANDING", "BASESLAB", "APPROACH_SLAB", "PAVING", "WEARING", "SIDEWALK", "TRACKSLAB"));
    public static readonly IfcClass Column        = new("IfcColumn", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("COLUMN", "PILASTER", "PIERSTEM", "PIERSTEM_SEGMENT", "STANDCOLUMN"));
    public static readonly IfcClass Beam          = new("IfcBeam", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("BEAM", "JOIST", "HOLLOWCORE", "LINTEL", "SPANDREL", "T_BEAM", "GIRDER_SEGMENT", "DIAPHRAGM", "PIERCAP", "HATSTONE", "CORNICE", "EDGEBEAM"));
    public static readonly IfcClass Door          = new("IfcDoor", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("DOOR", "GATE", "TRAPDOOR", "BOOM_BARRIER", "TURNSTILE"));
    public static readonly IfcClass Window        = new("IfcWindow", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("WINDOW", "SKYLIGHT", "LIGHTDOME"));
    public static readonly IfcClass Covering      = new("IfcCovering", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("CEILING", "FLOORING", "CLADDING", "ROOFING", "INSULATION", "MEMBRANE", "SLEEVING", "WRAPPING", "MOLDING", "SKIRTINGBOARD", "TOPPING"));
    public static readonly IfcClass CurtainWall   = new("IfcCurtainWall", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass Railing       = new("IfcRailing", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("HANDRAIL", "GUARDRAIL", "BALUSTRADE", "FENCE"));
    public static readonly IfcClass Ramp          = new("IfcRamp", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("STRAIGHT_RUN_RAMP", "TWO_STRAIGHT_RUN_RAMP", "QUARTER_TURN_RAMP", "TWO_QUARTER_TURN_RAMP", "HALF_TURN_RAMP", "SPIRAL_RAMP"));
    public static readonly IfcClass Roof          = new("IfcRoof", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("FLAT_ROOF", "SHED_ROOF", "GABLE_ROOF", "HIP_ROOF", "HIPPED_GABLE_ROOF", "GAMBREL_ROOF", "MANSARD_ROOF", "BARREL_ROOF", "RAINBOW_ROOF", "BUTTERFLY_ROOF", "PAVILION_ROOF", "DOME_ROOF", "FREEFORM"));
    public static readonly IfcClass Stair         = new("IfcStair", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("STRAIGHT_RUN_STAIR", "TWO_STRAIGHT_RUN_STAIR", "QUARTER_WINDING_STAIR", "QUARTER_TURN_STAIR", "HALF_WINDING_STAIR", "HALF_TURN_STAIR", "TWO_QUARTER_WINDING_STAIR", "TWO_QUARTER_TURN_STAIR", "THREE_QUARTER_WINDING_STAIR", "THREE_QUARTER_TURN_STAIR", "SPIRAL_STAIR", "DOUBLE_RETURN_STAIR", "CURVED_RUN_STAIR", "TWO_CURVED_RUN_STAIR", "LADDER"));
    public static readonly IfcClass Plate         = new("IfcPlate", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("CURTAIN_PANEL", "SHEET", "FLANGE_PLATE", "WEB_PLATE", "STIFFENER_PLATE", "GUSSET_PLATE", "COVER_PLATE", "BASE_PLATE", "SPLICE_PLATE"));
    public static readonly IfcClass Member        = new("IfcMember", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("BRACE", "CHORD", "COLLAR", "MEMBER", "MULLION", "PLATE", "POST", "PURLIN", "RAFTER", "STRINGER", "STRUT", "STUD", "STIFFENING_RIB", "ARCH_SEGMENT", "SUSPENSION_CABLE", "SUSPENDER", "STAY_CABLE", "TIEBAR"));
    public static readonly IfcClass Footing       = new("IfcFooting", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("CAISSON_FOUNDATION", "FOOTING_BEAM", "PAD_FOOTING", "PILE_CAP", "STRIP_FOOTING"));
    public static readonly IfcClass Pile          = new("IfcPile", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("BORED", "DRIVEN", "JETGROUTING", "COHESION", "FRICTION", "SUPPORT"));
    // The structural-connection realizing-element family the Projection/semantic#IFC_EGRESS Emit re-authors and the
    // Semantics/connection#CONNECTION_DETAIL reader resolves at ingress (the .api/api-geometrygym-ifc realizing surface):
    // the discrete IfcMechanicalFastener bolt/screw/anchor/dowel/rivet AND the welded-stud STUDSHEARCONNECTOR/SHEARCONNECTOR
    // (a Rasm.Materials joint#JOINT_FAMILY welded stud projects this row), the non-mechanical IfcFastener weld/glue/mortar
    // bead, the fabricated IfcDiscreteAccessory framing connector (SHOE/BRACKET/ANCHORPLATE — the hanger#HANGER_FAMILY
    // HangerType.IfcAccessoryType token, fastened BY a related IfcMechanicalFastener), the cast-in reinforcing bar/mesh and
    // the post-tensioning tendon/anchor, and the IFC4.3 IfcBearing support. The valid sets are the IFC4-base members the
    // schema span carries; the IFC4.3 rail-domain accessory/fastener tail rides USERDEFINED, the gate never enumerates it.
    public static readonly IfcClass MechanicalFastener  = new("IfcMechanicalFastener", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("ANCHORBOLT", "BOLT", "DOWEL", "NAIL", "NAILPLATE", "RIVET", "SCREW", "SHEARCONNECTOR", "STAPLE", "STUDSHEARCONNECTOR", "COUPLER"));
    public static readonly IfcClass Fastener            = new("IfcFastener", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("GLUE", "MORTAR", "WELD"));
    public static readonly IfcClass DiscreteAccessory   = new("IfcDiscreteAccessory", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("ANCHORPLATE", "BRACKET", "SHOE"));
    public static readonly IfcClass ReinforcingBar      = new("IfcReinforcingBar", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("MAIN", "SHEAR", "LIGATURE", "STUD", "PUNCHING", "EDGE", "RING", "ANCHORING", "SPACEBAR"));
    public static readonly IfcClass ReinforcingMesh     = new("IfcReinforcingMesh", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass Tendon              = new("IfcTendon", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("STRAND", "WIRE", "BAR", "COATED"));
    public static readonly IfcClass TendonAnchor        = new("IfcTendonAnchor", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("COUPLER", "FIXED_END", "TENSIONING_END"));
    public static readonly IfcClass Bearing             = new("IfcBearing", IfcDomain.Structural, IfcSchema.Ifc4x3, Seq("CYLINDRICAL", "SPHERICAL", "ELASTOMERIC", "POT", "GUIDE", "ROCKER", "ROLLER", "DISK"));
    public static readonly IfcClass FlowSegment   = new("IfcFlowSegment", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowFitting   = new("IfcFlowFitting", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowTerminal  = new("IfcFlowTerminal", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowController= new("IfcFlowController", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowMovingDevice    = new("IfcFlowMovingDevice", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowStorageDevice   = new("IfcFlowStorageDevice", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass EnergyConversionDevice = new("IfcEnergyConversionDevice", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass DistributionPort = new("IfcDistributionPort", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("CABLE", "CABLECARRIER", "DUCT", "PIPE", "WIRELESS"));
    // Concrete MEP distribution leaves — the entity types real models instantiate (the abstract IfcFlow* supertypes
    // above never appear as occurrences). The projector reads `p.GetType().Name`, so the leaf is the resolved row;
    // an unrostered long-tail/IFC4-new leaf still rides the permissive TryGet(...).IfNone(Proxy) ingress.
    public static readonly IfcClass DuctSegment             = new("IfcDuctSegment", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("RIGIDSEGMENT", "FLEXIBLESEGMENT"));
    public static readonly IfcClass PipeSegment             = new("IfcPipeSegment", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("FLEXIBLESEGMENT", "RIGIDSEGMENT", "GUTTER", "SPOOL", "CULVERT"));
    public static readonly IfcClass CableSegment            = new("IfcCableSegment", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("CABLESEGMENT", "CONDUCTORSEGMENT", "BUSBARSEGMENT", "CORESEGMENT", "CONTACTWIRESEGMENT", "FIBERSEGMENT", "FIBERTUBE", "OPTICALCABLESEGMENT", "STITCHWIRE", "WIREPAIRSEGMENT"));
    public static readonly IfcClass CableCarrierSegment     = new("IfcCableCarrierSegment", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("CABLELADDERSEGMENT", "CABLETRAYSEGMENT", "CABLETRUNKINGSEGMENT", "CONDUITSEGMENT", "CABLEBRACKET", "CATENARYWIRE", "DROPPER"));
    public static readonly IfcClass DuctFitting             = new("IfcDuctFitting", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("BEND", "CONNECTOR", "ENTRY", "EXIT", "JUNCTION", "OBSTRUCTION", "TRANSITION"));
    public static readonly IfcClass PipeFitting             = new("IfcPipeFitting", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("BEND", "CONNECTOR", "ENTRY", "EXIT", "JUNCTION", "OBSTRUCTION", "TRANSITION"));
    public static readonly IfcClass CableCarrierFitting     = new("IfcCableCarrierFitting", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("BEND", "CONNECTOR", "CROSS", "JUNCTION", "REDUCER", "TEE", "TRANSITION"));
    public static readonly IfcClass JunctionBox             = new("IfcJunctionBox", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("DATA", "POWER"));
    public static readonly IfcClass AirTerminal             = new("IfcAirTerminal", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("GRILLE", "REGISTER", "DIFFUSER", "LOUVRE", "EYEBALL", "IRIS", "LINEARGRILLE", "LINEARDIFFUSER"));
    public static readonly IfcClass SanitaryTerminal        = new("IfcSanitaryTerminal", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("BATH", "BIDET", "CISTERN", "SHOWER", "SINK", "SANITARYFOUNTAIN", "TOILETPAN", "URINAL", "WASHHANDBASIN", "WCSEAT"));
    public static readonly IfcClass WasteTerminal           = new("IfcWasteTerminal", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("FLOORTRAP", "FLOORWASTE", "GULLYSUMP", "GULLYTRAP", "GREASEINTERCEPTOR", "OILINTERCEPTOR", "PETROLINTERCEPTOR", "ROOFDRAIN", "WASTEDISPOSALUNIT", "WASTETRAP"));
    public static readonly IfcClass FireSuppressionTerminal = new("IfcFireSuppressionTerminal", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("BREECHINGINLET", "FIREHYDRANT", "HOSEREEL", "SPRINKLER", "SPRINKLERDEFLECTOR", "FIREMONITOR"));
    public static readonly IfcClass LightFixture            = new("IfcLightFixture", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("POINTSOURCE", "DIRECTIONSOURCE", "SECURITYLIGHTING"));
    public static readonly IfcClass Outlet                  = new("IfcOutlet", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("AUDIOVISUALOUTLET", "COMMUNICATIONSOUTLET", "POWEROUTLET", "DATAOUTLET", "TELEPHONEOUTLET"));
    public static readonly IfcClass Valve                   = new("IfcValve", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("AIRRELEASE", "ANTIVACUUM", "CHANGEOVER", "CHECK", "COMMISSIONING", "DIVERTING", "DRAWOFFCOCK", "DOUBLECHECK", "DOUBLEREGULATING", "FAUCET", "FLUSHING", "GASCOCK", "GASTAP", "ISOLATING", "MIXING", "PRESSUREREDUCING", "PRESSURERELIEF", "REGULATING", "SAFETYCUTOFF", "STEAMTRAP", "STOPCOCK"));
    public static readonly IfcClass Damper                  = new("IfcDamper", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("CONTROLDAMPER", "FIREDAMPER", "SMOKEDAMPER", "FIRESMOKEDAMPER", "BACKDRAFTDAMPER", "RELIEFDAMPER", "BLASTDAMPER", "GRAVITYDAMPER", "GRAVITYRELIEFDAMPER", "BALANCINGDAMPER", "FUMEHOODEXHAUST"));
    public static readonly IfcClass SwitchingDevice         = new("IfcSwitchingDevice", IfcDomain.Electrical, IfcSchema.Ifc2x3, Seq("CONTACTOR", "EMERGENCYSTOP", "STARTER", "SWITCHDISCONNECTOR", "TOGGLESWITCH", "DIMMERSWITCH", "KEYPAD", "MOMENTARYSWITCH", "SELECTORSWITCH", "RELAY", "START_AND_STOP_EQUIPMENT"));
    public static readonly IfcClass FlowMeter               = new("IfcFlowMeter", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("ENERGYMETER", "GASMETER", "OILMETER", "WATERMETER", "ELECTRICMETER", "FLOWMETER"));
    public static readonly IfcClass Pump                    = new("IfcPump", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("CIRCULATOR", "ENDSUCTION", "SPLITCASE", "VERTICALINLINE", "VERTICALTURBINE", "SUBMERSIBLEPUMP", "SUMPPUMP"));
    public static readonly IfcClass Fan                     = new("IfcFan", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("CENTRIFUGALFORWARDCURVED", "CENTRIFUGALRADIAL", "CENTRIFUGALBACKWARDINCLINEDCURVED", "CENTRIFUGALAIRFOIL", "TUBEAXIAL", "VANEAXIAL", "PROPELLORAXIAL", "JET"));
    public static readonly IfcClass Tank                    = new("IfcTank", IfcDomain.Plumbing, IfcSchema.Ifc2x3, Seq("EXPANSION", "PRESSUREVESSEL", "PREFORMED", "SECTIONAL", "BASIN", "BREAKPRESSURE", "FEEDANDEXPANSION", "STORAGE", "VESSEL", "OILRETENTIONTRAY"));
    public static readonly IfcClass Boiler                  = new("IfcBoiler", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("WATER", "STEAM"));
    public static readonly IfcClass Chiller                 = new("IfcChiller", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("AIRCOOLED", "WATERCOOLED", "HEATRECOVERY"));
    public static readonly IfcClass Coil                    = new("IfcCoil", IfcDomain.HvacFire, IfcSchema.Ifc2x3, Seq("DXCOOLINGCOIL", "WATERCOOLINGCOIL", "STEAMHEATINGCOIL", "WATERHEATINGCOIL", "ELECTRICHEATINGCOIL", "GASHEATINGCOIL", "HYDRONICCOIL"));
    public static readonly IfcClass StructuralCurveMember   = new("IfcStructuralCurveMember", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("RIGID_JOINED_MEMBER", "PIN_JOINED_MEMBER", "CABLE", "TENSION_MEMBER", "COMPRESSION_MEMBER"));
    public static readonly IfcClass StructuralSurfaceMember = new("IfcStructuralSurfaceMember", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq("BENDING_ELEMENT", "MEMBRANE_ELEMENT", "SHELL"));
    public static readonly IfcClass StructuralPointConnection = new("IfcStructuralPointConnection", IfcDomain.Structural, IfcSchema.Ifc2x3, Seq<string>());
    public static readonly IfcClass Space         = new("IfcSpace", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("SPACE", "PARKING", "GFA", "INTERNAL", "EXTERNAL", "BERTH"));
    public static readonly IfcClass Proxy         = new("IfcBuildingElementProxy", IfcDomain.Architecture, IfcSchema.Ifc2x3, Seq("COMPLEX", "ELEMENT", "PARTIAL", "PROVISIONFORVOID", "PROVISIONFORSPACE"));
    public static readonly IfcClass Bridge        = new("IfcBridge", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq("ARCHED", "CABLE_STAYED", "CANTILEVER", "CULVERT", "FRAMEWORK", "GIRDER", "SUSPENSION", "TRUSS"));
    public static readonly IfcClass Road          = new("IfcRoad", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq<string>());
    public static readonly IfcClass Railway       = new("IfcRailway", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq<string>());
    public static readonly IfcClass MarineFacility= new("IfcMarineFacility", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq("CANAL", "WATERWAYSHIPLIFT", "REVETMENT", "LAUNCHRECOVERY", "MARINEDEFENCE", "HYDROLIFT", "SHIPYARD", "SHIPLIFT", "PORT", "QUAY", "FLOATINGDOCK", "NAVIGATIONALCHANNEL", "BREAKWATER", "DRYDOCK", "JETTY", "SLIPWAY"));
    public static readonly IfcClass Course        = new("IfcCourse", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq("ARMOUR", "FILTER", "PAVEMENT", "PROTECTION"));
    public static readonly IfcClass Pavement      = new("IfcPavement", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq("FLEXIBLE", "RIGID"));
    public static readonly IfcClass Rail          = new("IfcRail", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq("RACKRAIL", "BLADE", "GUARDRAIL", "STOCKRAIL", "CHECKRAIL", "RAIL"));
    public static readonly IfcClass Alignment     = new("IfcAlignment", IfcDomain.Infrastructure, IfcSchema.Ifc4x3, Seq<string>());
    public static readonly IfcClass EarthworksFill= new("IfcEarthworksFill", IfcDomain.Geotechnical, IfcSchema.Ifc4x3, Seq("BACKFILL", "COUNTERWEIGHT", "EMBANKMENT", "SLOPEFILL", "SUBGRADE", "SUBGRADEBED", "TRANSITIONSECTION"));
    public static readonly IfcClass EarthworksCut = new("IfcEarthworksCut", IfcDomain.Geotechnical, IfcSchema.Ifc4x3, Seq("BASE_EXCAVATION", "CUT", "DREDGING", "OVEREXCAVATION", "PAVEMENTMILLING", "STEPEXCAVATION", "TOPSOILREMOVAL", "TRENCH"));
    public static readonly IfcClass GeotechnicalStratum = new("IfcGeotechnicalStratum", IfcDomain.Geotechnical, IfcSchema.Ifc4x3, Seq("SOLID", "VOID", "WATER"));
    public static readonly IfcClass Borehole      = new("IfcBorehole", IfcDomain.Geotechnical, IfcSchema.Ifc4x3, Seq<string>());

    public IfcDomain Domain { get; }
    public SchemaSpan Span { get; }
    public Seq<string> ValidPredefined { get; }

    // The strict ingress lookup the projector composes: entity-type string -> the row supplying the generic
    // Classification("ifc", Key) the seam Object node carries, Canonical folding the deprecated *StandardCase/
    // *ElementedCase subtypes onto their base row first. The typed UnmappedClass case lifts BARE onto the Fin rail
    // (band 2600 IS the Expected Code; no .ToError() hop) on a class the vocabulary omits; a permissive ingress
    // instead reads TryGet(entityType).IfNone(Proxy) over the SAME generated TryGet (no 2nd resolver).
    public static Fin<IfcClass> Resolve(string entityType, Op key) =>
        TryGet(Canonical(entityType)).ToFin(new BimFault.UnmappedClass(key, $"element-class-miss:{entityType}"));

    // IFC4 collapsed the deprecated *StandardCase/*ElementedCase implementation subtypes (IfcWallStandardCase,
    // IfcSlabElementedCase, ...) into the base class + PredefinedType; ParserIfc.IdentifyIfcClass does NOT fold them,
    // so a 2x3 IfcWallStandardCase the projector reads off p.GetType().Name resolves the IfcWall row here (its
    // predefined token rides AdmitPredefined separately) rather than aborting the import on the most common real entity.
    static string Canonical(string entityType) =>
        entityType.EndsWith("StandardCase", StringComparison.Ordinal)    ? entityType[..^"StandardCase".Length]
        : entityType.EndsWith("ElementedCase", StringComparison.Ordinal) ? entityType[..^"ElementedCase".Length]
        : entityType;

    // The egress gate [C6][H8]: admit the seam Object node's PredefinedType.Token against the row's frozen valid set
    // (USERDEFINED requires a non-empty ObjectType label) AND validate the class is available in the target Header
    // schema span, returning the validated IFC predefined token the projector authors (it sets the entity enum from
    // the token and authors ObjectType=objectType when USERDEFINED) or faulting UnmappedClass. The schema currency is
    // the SEAM ReleaseVersion (the Header's), ranked chronologically because the SmartEnum has no ordinal.
    public Fin<string> AdmitPredefined(string token, string objectType, ReleaseVersion schema, Op key) =>
        !Span.Covers(schema)
            ? Fin.Fail<string>(new BimFault.UnmappedClass(key, $"class-out-of-schema:{Key}:{schema.Key}"))
            : token.Trim().ToUpperInvariant() switch {
                "" or "NOTDEFINED" => Fin.Succ(PredefinedType.NotDefined.Token),
                "USERDEFINED"      => objectType.Trim() is { Length: > 0 }
                                          ? Fin.Succ("USERDEFINED")
                                          : Fin.Fail<string>(new BimFault.UnmappedClass(key, $"predefined-objecttype-miss:{Key}")),
                var value when ValidPredefined.IsEmpty || ValidPredefined.Contains(value) => Fin.Succ(value),
                var value          => Fin.Fail<string>(new BimFault.UnmappedClass(key, $"predefined-reject:{Key}:{value}")),
            };
}
```

## [03]-[REPRESENTATION_KEYS]

- Owner: `IfcRepresentation` the geometry-reference content-keyer [M2] projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map — `RepresentationIdentifier` (`Axis`/`Body`/`Box`/`FootPrint`) → the kernel seed-zero `XxHash128` content hash of the representation STEP — so the seam `Object` node references its geometry by content key per representation, never an IFC name leak and never an in-process BRep evaluation. Bim owns the IFC representation mapping and the `IfcRepresentationMap`/`IfcMappedItem` instancing per representation; the seam holds the neutral keyed map.
- Entry: `IfcRepresentation.Keys(IfcObjectDefinition? definition)` is ONE polymorphic content-keyer discriminating on the input shape — an occurrence `IfcProduct` folds its `IfcProductDefinitionShape.Representations` into the `RepresentationContentHash` map keyed by `RepresentationIdentifier`, content-keying each representation's STEP through the kernel `ContentHash.Of`; a type `IfcTypeProduct` folds its `RepresentationMaps` `IfcMappedItem` instancing onto the same map so an occurrence instancing a type representation shares the content key rather than re-keying; any other definition (or a null) yields `RepresentationContentHash.Empty`. There is no `KeysOf`/`MapKeys` operation family — the occurrence-versus-type distinction is the input case, never a name suffix.
- Auto: the occurrence arm reads each `IfcShapeRepresentation.RepresentationIdentifier` (the IFC axis/body/box/footprint discriminant), serializes the representation to its STEP record line, and content-keys it through the kernel seed-zero `ContentHash.Of` (`Rasm.Domain.ContentHash`, the single `XxHash128` seed) tag-namespaced so a direct shape and a mapped library shape never collide; the type arm keys the `IfcRepresentationMap.MappedRepresentation` once so every `IfcMappedItem` occurrence referencing that SAME map entity shares the content key — the instanced-geometry reuse the `Exchange/reconstruct#RECONSTRUCTION` lane mirrors, never a per-occurrence re-key. The content key is over the entity STEP serialization, so the SAME entity (a shared `IfcShapeRepresentation`, or one `IfcRepresentationMap` instanced by every occurrence) keys identically; the content-stable realized-geometry identity across distinct entities is the kernel `GeometryHash` over the tessellated geometry at the `Exchange/tessellation#TESSELLATION_BRIDGE` GLB wire, a separate content key this serialization key never duplicates.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new representation identifier is one `RepresentationIdentifier` key the map carries; the content-key seed is the kernel's single seed-zero `XxHash128` `ContentHash.Of`; never a second hasher and never a geometry blob on the seam node — only the content key.
- Boundary: the geometry reference is the content-keyed map [M2] and an inlined geometry blob, a stored `GeometryHandle`, or an IFC representation name on the seam node is the deleted form; the content key composes the kernel seed-zero `XxHash128` through `Rasm.Domain.ContentHash.Of` and a second hasher (or the strata-violating `Rasm.Compute` `InterchangeIdentity` consumed up-stratum) is the named defect [H7]; the representation STEP is keyed, NOT evaluated — an in-process BRep tessellation here is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the type representation-map instancing shares one content key across occurrences and a per-occurrence re-key is the deleted form; the occurrence-versus-type modality is the input case of one `Keys` entry and a `KeysOf`/`MapKeys` name family is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class IfcRepresentation {
    // The keyed geometry map [M2]: ONE polymorphic entry over IfcObjectDefinition discriminates the occurrence
    // product (its direct representations) from the type product (the IfcRepresentationMap instanced-geometry
    // library) on the input case, each representation keyed by its content key. The seam Object node references
    // geometry by this content key only — never an inlined geometry blob, never a second hasher.
    public static RepresentationContentHash Keys(IfcObjectDefinition? definition) =>
        definition switch {
            IfcProduct product => Optional(product.Representation).Match(
                None: () => RepresentationContentHash.Empty,
                Some: shape => shape.Representations.AsIterable()
                    .Choose(rep => Optional(rep.RepresentationIdentifier).Map(id => (Key: id, Hash: RepKey("ifc-rep", rep))))
                    .Fold(RepresentationContentHash.Empty, static (map, pair) => map.With(pair.Key, pair.Hash))),
            IfcTypeProduct type => type.RepresentationMaps.AsIterable()
                .Choose(map => Optional(map.MappedRepresentation)
                    .Bind(rep => Optional(rep.RepresentationIdentifier).Map(id => (Key: id, Hash: RepKey("ifc-repmap", rep)))))
                .Fold(RepresentationContentHash.Empty, static (acc, pair) => acc.With(pair.Key, pair.Hash)),
            _ => RepresentationContentHash.Empty,
        };

    // The content key composes the kernel seed-zero XxHash128 (Rasm.Domain.ContentHash.Of) over the entity STEP
    // record line, tag-namespaced so a direct shape and a mapped library shape never collide. ONE hasher, no second
    // hasher and no up-stratum InterchangeIdentity; the realized-geometry GeometryHash at the GLB wire is separate.
    static UInt128 RepKey(string tag, BaseClassIfc entity) =>
        ContentHash.Of(Encoding.UTF8.GetBytes(string.Concat(tag, " ", entity.StringSTEP())));
}
```

## [04]-[RESEARCH]

- [CLASS_TAXONOMY]: the `IfcClass` closed-vocabulary case list and the per-class `ValidPredefined` value sets ground against the GeometryGym entity-class surface (`.api/api-geometrygym-ifc` architectural built-element rows `IfcWall`…`IfcBuildingElementProxy`, the MEP distribution-element family `IfcFlowSegment`/`IfcFlowFitting`/`IfcFlowTerminal`/`IfcFlowController`/`IfcFlowMovingDevice`/`IfcFlowStorageDevice`/`IfcEnergyConversionDevice`/`IfcDistributionPort`, the structural-analysis member family, and the infrastructure-geotechnics rows) and the per-class `IfcXxxTypeEnum` predefined members; a documented family whose `IfcXxxTypeEnum` members the catalog does not enumerate carries an empty `ValidPredefined` set (the schema not constraining it) rather than a fabricated token list; the high-frequency concrete MEP distribution leaves (`IfcDuctSegment`/`IfcPipeSegment`/`IfcCableSegment`/`IfcDuctFitting`/`IfcPipeFitting`/`IfcAirTerminal`/`IfcSanitaryTerminal`/`IfcValve`/`IfcDamper`/`IfcPump`/`IfcFan`/`IfcBoiler`/`IfcChiller`/`IfcCoil`/…) are first-class rows carrying their `assay`-verified `IfcXxxTypeEnum` token sets, and the long-tail / IFC4-introduced leaves (`IfcBurner`/`IfcSolarDevice`/`IfcMedicalDevice`/`IfcCableFitting`/`IfcAudioVisualAppliance`/…) resolve through the permissive `TryGet(...).IfNone(Proxy)` ingress until rostered. The seam `SchemaSpan` `IntroducedIn`/`RemovedIn` per class [H8] grounds against the GeometryGym `VersionAddedAttribute` schema-availability reflection surface and the IFC2x3→IFC4→IFC4.3 release history (the infrastructure classes `IfcAlignment`/`IfcRoad`/`IfcRailway`/`IfcBridge`/`IfcMarineFacility`/`IfcEarthworks*`/`IfcGeotechnicalStratum`/`IfcBorehole`/`IfcCourse`/`IfcPavement`/`IfcRail` introduced in IFC4X3, the architectural/structural/MEP core available from IFC2x3) so the egress gate rejects a class the target schema cannot carry.
- [PREDEFINED_GATE]: the `AdmitPredefined` egress gate [C6] grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C6 (carry `PredefinedType` as a first-class typed value on the seam `Object` node; validity is a Bim-owned EGRESS gate resolving the `IfcClass` row from `code` and running `AdmitPredefined` against the frozen valid set → `BimFault.UnmappedClass`; fold the predefined token into the content hash) and the seam `Rasm.Element/Graph/element#NODE_MODEL` `PredefinedType` `[ValueObject<string>]` field — Bim is the IFC validation authority for the seam-carried value-object (the seam owns the `PredefinedType`, Bim owns the valid-set + `AdmitPredefined`), the frozen valid sets and the `USERDEFINED` fallback the load-bearing logic the `KEEP` preserves; the gate's schema argument is the SEAM `ReleaseVersion` the projector passes as `graph.Header` schema (`Projection/semantic#SEMANTIC_PROJECTOR` `Emit`), ranked chronologically because the `[SmartEnum]` carries no ordinal and the seam declaration order is not chronological — the GeometryGym `ReleaseVersion` enum stays on the IFC-text codec leg `Sniff`/`new DatabaseIfc` owns, never in this gate signature.
- [REPRESENTATION_KEY]: the `IfcRepresentation` content-keyer [M2] grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M2 (`GeometryRef` becomes a keyed map `RepresentationIdentifier → contentHash`, renamed neutral `RepresentationContentHash`; Bim owns the IFC representation mapping + `IfcRepresentationMap`/`IfcMappedItem` instancing per representation) and the kernel `Rasm.Domain.ContentHash.Of(ReadOnlySpan<byte>) → UInt128` seed-zero `XxHash128` content-identity (`libs/csharp/.api/api-hashing` + the kernel `Rasm/Domain/ContentHash` owner the seam `NodeId`/`ContentAddress` also compose), composed once, never a second hasher and never the up-stratum `Rasm.Compute` `InterchangeIdentity` [H7]; the `IfcShapeRepresentation.RepresentationIdentifier`/`IfcProductDefinitionShape.Representations`/`IfcRepresentationMap.MappedRepresentation`/`IfcMappedItem.MappingSource` member spellings and `BaseClassIfc.StringSTEP` confirm against `.api/api-geometrygym-ifc` geometry-representation + type-occurrence families; the `StringSTEP` content key keys the SAME entity identically (instanced reuse), the content-stable realized-geometry dedup across distinct entities being the kernel `GeometryHash` at the `Exchange/tessellation#TESSELLATION_BRIDGE` GLB wire, not this serialization key.
- [ELEMENT_RETIREMENT]: the `BimElement`/`BimModel` retirement grounds against `ELEMENT-REBUILD-PLAN.md` §2 (two parallel unaligned element owners collapsed into one property-graph IFC-native element) and §4B (the consumer-facing `Element` is a derived fold `Bake(objectNode)` over the reachable subgraph, never a second stored record) — the typed data formerly stranded on `BimElement.Properties`/`Quantities`/`Materials` now rides the seam `PropertySet`/`QuantitySet`/`Material` nodes and the `Bake` fold reads one flat Option-typed field at the wire.
