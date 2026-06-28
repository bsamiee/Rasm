# [BIM_IFC_TAXONOMY]

The IFC entity-class vocabulary `Rasm.Bim` owns as the SOLE GeometryGym/IFC owner: the `IfcClass` `[SmartEnum<string>]` buildingSMART entity-class breadth keyed on the IFC entity-type string, each row carrying its `IfcDomain` discipline partition, its frozen valid-predefined set, and its `IntroducedIn`/`RemovedIn` schema span; the `AdmitPredefined` egress gate validating a predefined token against the row's valid set AND the target schema; and the `IfcRepresentation` content-keyer projecting an IFC product's representations onto the seam `RepresentationContentHash` map. This page is the IFC-schema authority the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes at BOTH legs — at ingress it resolves the entity-type string to the row that supplies the generic `Classification("ifc", classKey)` stamped on the seam `Object` node and admits the predefined token, and at egress it runs the `PredefinedType` validity gate against the frozen valid set and the schema span before the IFC entity is authored [C6][H8]. The retired `BimElement` element record and `BimModel` collection are GONE: the consumer-facing element is the seam `Bake(objectNode)` fold over the reachable `ElementGraph` subgraph, never a second stored record keyed by `GlobalId`, so "has it all" is one flat read on the seam graph and the typed data is never stranded off the element. The vocabulary is ONE owner widened on three folded axes — a new entity class is one `IfcClass` row, a new sub-class kind is one valid-predefined entry, a new schema availability is one span column — never three parallel surfaces and never a per-element-class type.

## [01]-[INDEX]

- [01]-[IFC_CLASS]: `IfcClass` `[SmartEnum<string>]` entity-class vocabulary, the `IfcDomain` discipline partition, the frozen valid-predefined set, the `IntroducedIn`/`RemovedIn` schema span [H8], the `Resolve` ingress lookup, and the `AdmitPredefined` egress gate over the seam-owned `PredefinedType` token [C6].
- [02]-[REPRESENTATION_KEYS]: `IfcRepresentation` the geometry-reference content-keyer projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map (axis/body/box/footprint → kernel `XxHash128` content hash) [M2], composing the kernel seed-zero identity, never a second hasher.

## [02]-[IFC_CLASS]

- Owner: `IfcClass` the `[SmartEnum<string>]` closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string, each row carrying its `IfcDomain` discipline, its frozen `ValidPredefined` set, and its `IfcSchemaSpan` (`IntroducedIn`/`RemovedIn` `ReleaseVersion`) so a class authored against a schema that does not carry it faults at egress; `IfcDomain` the seven-case buildingSMART discipline partition; `IfcSchemaSpan` the per-class availability window [H8]. The `PredefinedType` sub-class discriminant is the seam-carried neutral token (a `string`) on the `Object` node (`Rasm.Element/Graph/element#ELEMENT`), of which `IfcClass` is the IFC validation authority — the predefined-type semantics live HERE (the frozen valid sets + `AdmitPredefined`), the seam node carrying only the string token [C6].
- Entry: `IfcClass.Resolve(string entityType)` is the ingress lookup the projector composes — it resolves the IFC entity-type string (the `ParserIfc.IdentifyIfcClass` class half) to the row that supplies the generic `Classification("ifc", row.Key)` stamped on the seam `Object` node, faulting `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` on a class the vocabulary does not carry, lowered with `.ToError()`; `IfcClass.AdmitPredefined(string token, string objectType, ReleaseVersion schema)` is the egress gate — it admits the predefined token against the row's frozen valid set with the `USERDEFINED`/`OBJECTTYPE` fallback AND validates the class is available in the target schema span, returning the validated predefined token (the seam `Object` node's `string PredefinedType`) or faulting `BimFault.UnmappedClass`.
- Auto: `Resolve` reads the `Items` SmartEnum table by key; the projector folds its result into the generic `Classification` value-object so the seam node carries a `(system, code)` pair (`"ifc"`, `"IfcWall"`) rather than the `IfcClass` type itself, keeping the seam IFC-schema-free; `AdmitPredefined` trims and upper-cases the token, routes `""`/`"NOTDEFINED"` to the canonical `"NOTDEFINED"` token, routes `"USERDEFINED"`/`"OBJECTTYPE"` to the object-type fallback string, accepts a token in the frozen `ValidPredefined` set (or any token when the set is empty, the schema not constraining it), and otherwise faults; the schema-span check rejects a class whose `IntroducedIn` is later than (or `RemovedIn` earlier than) the target `schema` so an IFC4.3 `IfcAlignment` authored against an IFC2x3 emit faults rather than writing an entity the schema forbids [H8]; the admitted predefined token folds into the seam node content hash [C6].
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new entity class is one `IfcClass` row carrying its domain, valid-predefined set, and schema span; a new sub-class kind is one frozen entry in the row's valid set; a new schema availability is one `IfcSchemaSpan` column; a new IFC4.4 infrastructure class rides the same ingress/egress on one new row; never a per-element-class type, never a `Get<Domain>` family, and never the retired `BimElement` record.
- Boundary: `BimElement` and `BimModel` are RETIRED — the consumer element is the seam `Bake(objectNode)` fold over the `ElementGraph`, and any owner that re-stores a `BimElement(GlobalId, IfcClass, …)` record off the seam graph is the deleted form; `IfcClass` is the IFC-schema vocabulary the projector composes, NOT a field on a seam node — the seam `Object` node carries the generic `Classification("ifc", code)` value-object and a typed `IfcClass` on the node is the named seam violation [C6]; the `PredefinedType` token is a `string` on the seam `Object` node and `IfcClass.AdmitPredefined` is its IFC validation authority — a Bim `PredefinedType` value-object type is the deleted form (the seam carries the token as a `string`, Bim owns only the valid-set + `AdmitPredefined`), and a per-call predefined regex instead of the frozen valid set is the named defect; the predefined validity is an EGRESS gate (validated when the IFC entity is authored, against the valid set + the schema span [C6][H8]) and silent acceptance of an out-of-schema predefined is the named defect; the entity-type strings ground against the GeometryGym entity vocabulary consumed as settled (`.api/api-geometrygym-ifc`), and a raw entity-type string crossing a seam signature is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
public enum IfcDomain : byte {
    Architecture = 0, Structural = 1, HvacFire = 2, Electrical = 3, Plumbing = 4, Infrastructure = 5, Geotechnical = 6,
}

// The per-class schema availability window [H8]: the schema that introduced the class and the schema (if any)
// that withdrew it, so AdmitPredefined rejects a class authored against a schema that cannot carry it. The
// IFC4.3 infrastructure classes (IfcAlignment/IfcRoad/IfcBridge/IfcEarthworks*) are unavailable in IFC2x3/IFC4.
public readonly record struct IfcSchemaSpan(ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn) {
    public static readonly IfcSchemaSpan Ifc2x3 = new(ReleaseVersion.IFC2x3, None);
    public static readonly IfcSchemaSpan Ifc4 = new(ReleaseVersion.IFC4A2, None);
    public static readonly IfcSchemaSpan Ifc4x3 = new(ReleaseVersion.IFC4X3_ADD2, None);

    public bool Covers(ReleaseVersion schema) =>
        schema >= IntroducedIn && RemovedIn.Match(Some: removed => schema < removed, None: () => true);
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class IfcClass {
    public static readonly IfcClass Wall          = new("IfcWall", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("STANDARD", "POLYGONAL", "SHEAR", "ELEMENTEDWALL", "PLUMBINGWALL", "MOVABLE", "PARAPET", "PARTITIONING", "SOLIDWALL", "RETAININGWALL"));
    public static readonly IfcClass Slab          = new("IfcSlab", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("FLOOR", "ROOF", "LANDING", "BASESLAB", "APPROACH_SLAB", "PAVING", "WEARING", "SIDEWALK", "TRACKSLAB"));
    public static readonly IfcClass Column        = new("IfcColumn", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("COLUMN", "PILASTER", "PIERSTEM", "PIERSTEM_SEGMENT", "STANDCOLUMN"));
    public static readonly IfcClass Beam          = new("IfcBeam", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("BEAM", "JOIST", "HOLLOWCORE", "LINTEL", "SPANDREL", "T_BEAM", "GIRDER_SEGMENT", "DIAPHRAGM", "PIERCAP", "HATSTONE", "CORNICE", "EDGEBEAM"));
    public static readonly IfcClass Door          = new("IfcDoor", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("DOOR", "GATE", "TRAPDOOR", "BOOM_BARRIER", "TURNSTILE"));
    public static readonly IfcClass Window        = new("IfcWindow", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("WINDOW", "SKYLIGHT", "LIGHTDOME"));
    public static readonly IfcClass Covering      = new("IfcCovering", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("CEILING", "FLOORING", "CLADDING", "ROOFING", "INSULATION", "MEMBRANE", "SLEEVING", "WRAPPING", "MOLDING", "SKIRTINGBOARD", "TOPPING"));
    public static readonly IfcClass CurtainWall   = new("IfcCurtainWall", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq<string>());
    public static readonly IfcClass Railing       = new("IfcRailing", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("HANDRAIL", "GUARDRAIL", "BALUSTRADE", "FENCE"));
    public static readonly IfcClass Ramp          = new("IfcRamp", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("STRAIGHT_RUN_RAMP", "TWO_STRAIGHT_RUN_RAMP", "QUARTER_TURN_RAMP", "TWO_QUARTER_TURN_RAMP", "HALF_TURN_RAMP", "SPIRAL_RAMP"));
    public static readonly IfcClass Roof          = new("IfcRoof", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("FLAT_ROOF", "SHED_ROOF", "GABLE_ROOF", "HIP_ROOF", "HIPPED_GABLE_ROOF", "GAMBREL_ROOF", "MANSARD_ROOF", "BARREL_ROOF", "RAINBOW_ROOF", "BUTTERFLY_ROOF", "PAVILION_ROOF", "DOME_ROOF", "FREEFORM"));
    public static readonly IfcClass Stair         = new("IfcStair", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("STRAIGHT_RUN_STAIR", "TWO_STRAIGHT_RUN_STAIR", "QUARTER_WINDING_STAIR", "QUARTER_TURN_STAIR", "HALF_WINDING_STAIR", "HALF_TURN_STAIR", "TWO_QUARTER_WINDING_STAIR", "TWO_QUARTER_TURN_STAIR", "THREE_QUARTER_WINDING_STAIR", "THREE_QUARTER_TURN_STAIR", "SPIRAL_STAIR", "DOUBLE_RETURN_STAIR", "CURVED_RUN_STAIR", "TWO_CURVED_RUN_STAIR", "LADDER"));
    public static readonly IfcClass Plate         = new("IfcPlate", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("CURTAIN_PANEL", "SHEET", "FLANGE_PLATE", "WEB_PLATE", "STIFFENER_PLATE", "GUSSET_PLATE", "COVER_PLATE", "BASE_PLATE", "SPLICE_PLATE"));
    public static readonly IfcClass Member        = new("IfcMember", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("BRACE", "CHORD", "COLLAR", "MEMBER", "MULLION", "PLATE", "POST", "PURLIN", "RAFTER", "STRINGER", "STRUT", "STUD", "STIFFENING_RIB", "ARCH_SEGMENT", "SUSPENSION_CABLE", "SUSPENDER", "STAY_CABLE", "TIEBAR"));
    public static readonly IfcClass Footing       = new("IfcFooting", IfcDomain.Structural, IfcSchemaSpan.Ifc2x3, Seq("CAISSON_FOUNDATION", "FOOTING_BEAM", "PAD_FOOTING", "PILE_CAP", "STRIP_FOOTING"));
    public static readonly IfcClass Pile          = new("IfcPile", IfcDomain.Structural, IfcSchemaSpan.Ifc2x3, Seq("BORED", "DRIVEN", "JETGROUTING", "COHESION", "FRICTION", "SUPPORT"));
    public static readonly IfcClass FlowSegment   = new("IfcFlowSegment", IfcDomain.HvacFire, IfcSchemaSpan.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowFitting   = new("IfcFlowFitting", IfcDomain.HvacFire, IfcSchemaSpan.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowTerminal  = new("IfcFlowTerminal", IfcDomain.HvacFire, IfcSchemaSpan.Ifc2x3, Seq<string>());
    public static readonly IfcClass FlowController= new("IfcFlowController", IfcDomain.HvacFire, IfcSchemaSpan.Ifc2x3, Seq<string>());
    public static readonly IfcClass DistributionPort = new("IfcDistributionPort", IfcDomain.Electrical, IfcSchemaSpan.Ifc2x3, Seq("CABLE", "CABLECARRIER", "DUCT", "PIPE", "WIRELESS"));
    public static readonly IfcClass StructuralCurveMember   = new("IfcStructuralCurveMember", IfcDomain.Structural, IfcSchemaSpan.Ifc2x3, Seq("RIGID_JOINED_MEMBER", "PIN_JOINED_MEMBER", "CABLE", "TENSION_MEMBER", "COMPRESSION_MEMBER"));
    public static readonly IfcClass StructuralSurfaceMember = new("IfcStructuralSurfaceMember", IfcDomain.Structural, IfcSchemaSpan.Ifc2x3, Seq("BENDING_ELEMENT", "MEMBRANE_ELEMENT", "SHELL"));
    public static readonly IfcClass StructuralPointConnection = new("IfcStructuralPointConnection", IfcDomain.Structural, IfcSchemaSpan.Ifc2x3, Seq<string>());
    public static readonly IfcClass Space         = new("IfcSpace", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("SPACE", "PARKING", "GFA", "INTERNAL", "EXTERNAL", "BERTH"));
    public static readonly IfcClass Proxy         = new("IfcBuildingElementProxy", IfcDomain.Architecture, IfcSchemaSpan.Ifc2x3, Seq("COMPLEX", "ELEMENT", "PARTIAL", "PROVISIONFORVOID", "PROVISIONFORSPACE"));
    public static readonly IfcClass Bridge        = new("IfcBridge", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq("ARCHED", "CABLE_STAYED", "CANTILEVER", "CULVERT", "FRAMEWORK", "GIRDER", "SUSPENSION", "TRUSS"));
    public static readonly IfcClass Road          = new("IfcRoad", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq<string>());
    public static readonly IfcClass Railway       = new("IfcRailway", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq<string>());
    public static readonly IfcClass MarineFacility= new("IfcMarineFacility", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq("CANAL", "WATERWAYSHIPLIFT", "REVETMENT", "LAUNCHRECOVERY", "MARINEDEFENCE", "HYDROLIFT", "SHIPYARD", "SHIPLIFT", "PORT", "QUAY", "FLOATINGDOCK", "NAVIGATIONALCHANNEL", "BREAKWATER", "DRYDOCK", "JETTY", "SLIPWAY"));
    public static readonly IfcClass Course        = new("IfcCourse", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq("ARMOUR", "FILTER", "PAVEMENT", "PROTECTION"));
    public static readonly IfcClass Pavement      = new("IfcPavement", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq("FLEXIBLE", "RIGID"));
    public static readonly IfcClass Rail          = new("IfcRail", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq("RACKRAIL", "BLADE", "GUARDRAIL", "STOCKRAIL", "CHECKRAIL", "RAIL"));
    public static readonly IfcClass Alignment     = new("IfcAlignment", IfcDomain.Infrastructure, IfcSchemaSpan.Ifc4x3, Seq<string>());
    public static readonly IfcClass EarthworksFill= new("IfcEarthworksFill", IfcDomain.Geotechnical, IfcSchemaSpan.Ifc4x3, Seq("BACKFILL", "COUNTERWEIGHT", "EMBANKMENT", "SLOPEFILL", "SUBGRADE", "SUBGRADEBED", "TRANSITIONSECTION"));
    public static readonly IfcClass EarthworksCut = new("IfcEarthworksCut", IfcDomain.Geotechnical, IfcSchemaSpan.Ifc4x3, Seq("BASE_EXCAVATION", "CUT", "DREDGING", "OVEREXCAVATION", "PAVEMENTMILLING", "STEPEXCAVATION", "TOPSOILREMOVAL", "TRENCH"));
    public static readonly IfcClass GeotechnicalStratum = new("IfcGeotechnicalStratum", IfcDomain.Geotechnical, IfcSchemaSpan.Ifc4x3, Seq("SOLID", "VOID", "WATER"));
    public static readonly IfcClass Borehole      = new("IfcBorehole", IfcDomain.Geotechnical, IfcSchemaSpan.Ifc4x3, Seq<string>());

    public IfcDomain Domain { get; }
    public IfcSchemaSpan Span { get; }
    public Seq<string> ValidPredefined { get; }

    // The ingress lookup the projector composes: entity-type string -> the row supplying the generic
    // Classification("ifc", Key) the seam Object node carries. UnmappedClass on a class the vocabulary omits.
    public static Fin<IfcClass> Resolve(string entityType) =>
        TryGet(entityType).ToFin(new BimFault.UnmappedClass($"element-class-miss:{entityType}").ToError());

    // The egress gate [C6][H8]: admit the predefined token against the frozen valid set (USERDEFINED/OBJECTTYPE
    // fallback) AND validate the class is available in the target schema span, returning the validated predefined
    // token (a string, the seam Object node's PredefinedType field) or faulting UnmappedClass. The admitted token
    // folds into the seam node content hash; the seam carries the token as a string, Bim owns the valid-set.
    public Fin<string> AdmitPredefined(string token, string objectType, ReleaseVersion schema) =>
        !Span.Covers(schema)
            ? Fin.Fail<string>(new BimFault.UnmappedClass($"class-out-of-schema:{Key}:{schema}").ToError())
            : token.Trim().ToUpperInvariant() switch {
                "" or "NOTDEFINED"            => Fin.Succ("NOTDEFINED"),
                "USERDEFINED" or "OBJECTTYPE" => objectType.Trim() is { Length: > 0 } userDefined
                                                     ? Fin.Succ(userDefined)
                                                     : Fin.Fail<string>(new BimFault.UnmappedClass($"predefined-objecttype-miss:{Key}").ToError()),
                var value when ValidPredefined.IsEmpty || ValidPredefined.Contains(value) => Fin.Succ(value),
                var value                     => Fin.Fail<string>(new BimFault.UnmappedClass($"predefined-reject:{Key}:{value}").ToError()),
            };
}
```

## [03]-[REPRESENTATION_KEYS]

- Owner: `IfcRepresentation` the geometry-reference content-keyer [M2] projecting an `IfcProduct`/`IfcTypeProduct` representation set onto the seam `RepresentationContentHash` keyed map — `RepresentationIdentifier` (`Axis`/`Body`/`Box`/`FootPrint`) → the kernel seed-zero `XxHash128` content hash of the representation STEP — so the seam `Object` node references its geometry by content key per representation, never an IFC name leak and never an in-process BRep evaluation. Bim owns the IFC representation mapping and the `IfcRepresentationMap`/`IfcMappedItem` instancing per representation; the seam holds the neutral keyed map.
- Entry: `IfcRepresentation.KeysOf(IfcProduct product, double tolerance, double angularTolerance)` folds the product's `IfcProductDefinitionShape.Representations` into the `RepresentationContentHash` map keyed by `RepresentationIdentifier`, content-keying each representation's STEP through the kernel `InterchangeIdentity.Key`; `IfcRepresentation.MapKeys(IfcTypeProduct? type, double tolerance, double angularTolerance)` folds the type's `RepresentationMaps` `IfcMappedItem` instancing onto the same map so an occurrence instancing a type representation shares the content key rather than re-keying.
- Auto: `KeysOf` reads each `IfcShapeRepresentation.RepresentationIdentifier` (the IFC axis/body/box/footprint discriminant), serializes the representation to its STEP string, and content-keys it through `InterchangeIdentity.Key("ifc-rep", bytes, tolerance, tolerance, angularTolerance)` (the kernel seed-zero `XxHash128`, tolerance-folded) so two structurally-identical representations key identically and the geometry deduplicates; `MapKeys` keys the `IfcRepresentationMap.MappedRepresentation` once so every `IfcMappedItem` occurrence referencing it shares the content key, mirroring the `Exchange/reconstruct#RECONSTRUCTION` instanced-geometry reuse, never a per-occurrence re-key.
- Packages: GeometryGymIFC_Core, Rasm.Element, Rasm, LanguageExt.Core
- Growth: a new representation identifier is one `RepresentationIdentifier` key the map carries; the content-key seed is the kernel's single seed-zero `XxHash128`; never a second hasher and never a geometry blob on the seam node — only the content key.
- Boundary: the geometry reference is the content-keyed map [M2] and an inlined geometry blob, a stored `GeometryHandle`, or an IFC representation name on the seam node is the deleted form; the content key composes the kernel seed-zero `XxHash128` through `InterchangeIdentity.Key` and a second hasher is the named defect [H7]; the representation STEP is keyed, NOT evaluated — an in-process BRep tessellation here is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the type representation-map instancing shares one content key across occurrences and a per-occurrence re-key is the deleted form.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class IfcRepresentation {
    // The keyed geometry map [M2]: each representation -> its content key (kernel seed-zero XxHash128 over the
    // representation STEP, tolerance-folded). The seam Object node references geometry by this key only.
    public static RepresentationContentHash KeysOf(IfcProduct product, double tolerance, double angularTolerance) =>
        Optional(product.Representation).Match(
            None: () => RepresentationContentHash.Empty,
            Some: shape => shape.Representations.AsIterable()
                .Choose(rep => Optional(rep.RepresentationIdentifier)
                    .Map(id => (Key: id, Hash: InterchangeIdentity.Key("ifc-rep", System.Text.Encoding.UTF8.GetBytes(rep.StringSTEP()), tolerance, tolerance, angularTolerance))))
                .Fold(RepresentationContentHash.Empty, static (map, pair) => map.With(pair.Key, pair.Hash)));

    // The type representation-map instancing: the IfcRepresentationMap.MappedRepresentation is keyed once so
    // every IfcMappedItem occurrence referencing it shares the content key, never a per-occurrence re-key.
    public static RepresentationContentHash MapKeys(IfcTypeProduct? type, double tolerance, double angularTolerance) =>
        Optional(type).Match(
            None: () => RepresentationContentHash.Empty,
            Some: product => product.RepresentationMaps.AsIterable()
                .Choose(map => Optional(map.MappedRepresentation)
                    .Bind(rep => Optional(rep.RepresentationIdentifier)
                        .Map(id => (Key: id, Hash: InterchangeIdentity.Key("ifc-repmap", System.Text.Encoding.UTF8.GetBytes(map.StringSTEP()), tolerance, tolerance, angularTolerance)))))
                .Fold(RepresentationContentHash.Empty, static (acc, pair) => acc.With(pair.Key, pair.Hash)));
}
```

## [04]-[RESEARCH]

- [CLASS_TAXONOMY]: the `IfcClass` closed-vocabulary case list and the per-class `ValidPredefined` value sets ground against the GeometryGym entity-class surface (`.api/api-geometrygym-ifc` architectural built-element / MEP distribution / structural-analysis / infrastructure-geotechnics families) and the per-class `IfcXxxTypeEnum` predefined members; the `IfcSchemaSpan` `IntroducedIn`/`RemovedIn` per class [H8] ground against the GeometryGym `VersionAddedAttribute` schema-availability reflection surface and the IFC2x3→IFC4→IFC4.3 release history (the infrastructure classes `IfcAlignment`/`IfcRoad`/`IfcRailway`/`IfcBridge`/`IfcMarineFacility`/`IfcEarthworks*`/`IfcGeotechnicalStratum`/`IfcBorehole`/`IfcCourse`/`IfcPavement`/`IfcRail` introduced in IFC4X3, the architectural/structural/MEP core available from IFC2x3) so the egress gate rejects a class the target schema cannot carry.
- [PREDEFINED_GATE]: the `AdmitPredefined` egress gate [C6] grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT C6 (carry `PredefinedType` as a first-class typed value on the seam `Object` node; validity is a Bim-owned EGRESS gate resolving the `IfcClass` row from `code` and running `AdmitPredefined` against the frozen valid set → `BimFault.UnmappedClass`; fold the predefined token into the content hash) and the seam `Rasm.Element/Graph/element#ELEMENT` `string PredefinedType` token field — Bim is the IFC validation authority for the seam-carried token (the seam carries the `string`, Bim owns the valid-set), the frozen valid sets and the `USERDEFINED`/`OBJECTTYPE` fallback the load-bearing logic the `KEEP` preserves.
- [REPRESENTATION_KEY]: the `IfcRepresentation` content-keyer [M2] grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT M2 (`GeometryRef` becomes a keyed map `RepresentationIdentifier → contentHash`, renamed neutral `RepresentationContentHash`; Bim owns the IFC representation mapping + `IfcRepresentationMap`/`IfcMappedItem` instancing per representation) and the kernel `InterchangeIdentity.Key` seed-zero `XxHash128` content-identity (`.api/api-hashing` + the kernel identity owner), composed once, never a second hasher [H7]; the `IfcShapeRepresentation.RepresentationIdentifier`/`IfcProductDefinitionShape.Representations`/`IfcRepresentationMap.MappedRepresentation`/`IfcMappedItem.MappingSource` member spellings confirm against `.api/api-geometrygym-ifc` geometry-representation + type-occurrence families.
- [ELEMENT_RETIREMENT]: the `BimElement`/`BimModel` retirement grounds against `ELEMENT-REBUILD-PLAN.md` §2 (two parallel unaligned element owners collapsed into one property-graph IFC-native element) and §4B (the consumer-facing `Element` is a derived fold `Bake(objectNode)` over the reachable subgraph, never a second stored record) — the typed data formerly stranded on `BimElement.Properties`/`Quantities`/`Materials` now rides the seam `PropertySet`/`QuantitySet`/`Material` nodes and the `Bake` fold reads one flat Option-typed field at the wire.
