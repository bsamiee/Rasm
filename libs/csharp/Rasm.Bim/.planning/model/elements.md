# [BIM_ELEMENTS]

The host-neutral BIM element vocabulary on three folded axes of one product-type concept: the `IfcClass` `[SmartEnum<string>]` entity-class breadth discriminated by a seven-case `IfcDomain` partition, the `PredefinedType` `[ValueObject<string>]` sub-class discriminant validated against each row's frozen valid-predefined set, and the `BimType` type-occurrence factoring an `IfcMappedItem` references — all carried on the one `BimElement` record, the `BimModel` element-collection owner, and the `BimModel.Project` fold from the `IfcSemanticModel` graph. The element model is HOST-NEUTRAL — the geometry handle binds the kernel `Rasm` geometry by reference and never carries a RhinoCommon type — and projects from the `Exchange/import#IMPORT_RAIL` `IfcSemanticModel` graph consumed as settled vocabulary, never re-minting the semantic-graph shape. The vocabulary is ONE owner widened on all three axes, never three parallel surfaces: a new entity class is one `IfcClass` row, a new sub-class kind is one frozen valid-predefined entry, and a 4000-window model carries one `BimType` plus 4000 occurrence references rather than 4000 inlined geometries.

## [1]-[INDEX]

- [1]-[ELEMENT_MODEL]: `BimElement` element record, `IfcClass`/`IfcDomain` entity-class vocabulary, `PredefinedType` discriminant, `BimModel` collection, and the `Project` fold.
- [2]-[BIM_TYPE]: `BimType` type-object record carrying the `IfcElementType`/`IfcTypeProduct` discriminant, the type-bound property/material/predefined bindings, and the `IfcRepresentationMap` instanced-geometry library.

## [2]-[ELEMENT_MODEL]

- Owner: `BimElement` the single host-neutral element record carrying the `IfcClass` discriminant, the `PredefinedType` sub-class discriminant, the stable `GlobalId`, the kernel-geometry handle, and the property/quantity/material/classification/type bindings; `IfcClass` `[SmartEnum<string>]` the closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string, each row carrying its `IfcDomain` and its frozen valid-predefined set; `IfcDomain` the seven-case buildingSMART discipline partition; `PredefinedType` `[ValueObject<string>]` the sub-class discriminant admitted once against the class row's valid set; `BimModel` the element-collection owner wrapping the projected element graph with the schema/model-view header.
- Entry: `BimModel.Project(IfcSemanticModel semantic, ClockPolicy clocks)` projects the in-process IFC semantic graph into the host-neutral `BimModel` element collection — `Fin<T>` aborts on an unmapped entity class (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or a missing spatial host (`BimFault.DanglingReference`), each lowered with `.ToError()`; the `ProductRow.PredefinedType` string admits through `IfcClass.AdmitPredefined` against the row's frozen valid set with the `USERDEFINED`/`OBJECTTYPE` fallback, the geometry handle binds the kernel `Rasm` geometry by reference, never re-tessellating, and the `BimType` resolves lazily from `TypeGlobalId` against the model's `Types` index.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new element class is one `IfcClass` row carrying its domain and valid-predefined set; a new sub-class kind is one frozen entry in the row's valid-predefined set; a new element binding is one column on `BimElement` projected from the existing `IfcSemanticModel` row family; a new IFC4.3 infrastructure class rides the same projection on one new row; never a per-element-class type and never a `Get<Domain>` family.
- Boundary: `BimElement` is ONE record discriminated by `IfcClass` row data on three folded axes (entity-class, predefined-type, type-occurrence) — a `WallElement`/`SlabElement`/`ColumnElement` class family, a `LoadBearingWall`/`ExteriorDoor` predefined-type subclass, and a per-window inlined geometry are the deleted forms; the entity-class breadth widens the one `IfcClass` SmartEnum row table across the architectural/MEP/structural/infrastructure/geotechnical domains rather than minting parallel surfaces, and the `IfcDomain` enum widens to the frozen seven-case buildingSMART partition so `Model/query#ELEMENT_SET` discriminates by domain without a per-element type; the `PredefinedType` is admitted once at projection against the `IfcClass` row's frozen valid set, the `USERDEFINED`/`OBJECTTYPE` fallback resolving the `ObjectType` string, and a per-call predefined regex is the named defect; the geometry handle is the kernel `Rasm` geometry consumed by reference and a RhinoCommon `Brep`/`Mesh` field on `BimElement` is the named seam violation; the `IfcSemanticModel` graph is owned at `Exchange/import#IMPORT_RAIL` and consumed as settled vocabulary; a raw entity-type string crossing a public signature is the named defect; property/quantity/material/classification/type bindings project from the existing `IfcSemanticModel.PropertyRow`/`QuantityRow`/`MaterialRow`/`ClassificationRow`/`TypeRow` families, never a second property model; the `Materials` binding carries the typed `Semantics/composition#MATERIAL_COMPOSITION` `BimMaterial` composition (layered/profiled/constituent) rather than a bare name string; the `Classifications` binding carries the typed `ClassificationRef` owned at `Semantics/classification#CLASSIFICATION_AXIS` so the `Model/query#ELEMENT_SET` `ByClassification` arm matches a typed system-and-code pair rather than a stringly-keyed property lookup; the `TypeGlobalId` resolves the `[3]-[BIM_TYPE]` `BimType` by reference so an occurrence inherits type-bound properties/material/predefined and instances the type representation map, never inlining the type geometry.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class IfcClass {
    public static readonly IfcClass Wall          = new("IfcWall", IfcDomain.Architecture, Seq("STANDARD", "POLYGONAL", "SHEAR", "ELEMENTEDWALL", "PLUMBINGWALL", "MOVABLE", "PARAPET", "PARTITIONING", "SOLIDWALL", "RETAININGWALL"));
    public static readonly IfcClass Slab          = new("IfcSlab", IfcDomain.Architecture, Seq("FLOOR", "ROOF", "LANDING", "BASESLAB", "APPROACH_SLAB", "PAVING", "WEARING", "SIDEWALK", "TRACKSLAB"));
    public static readonly IfcClass Column        = new("IfcColumn", IfcDomain.Architecture, Seq("COLUMN", "PILASTER", "PIERSTEM", "PIERSTEM_SEGMENT", "STANDCOLUMN"));
    public static readonly IfcClass Beam          = new("IfcBeam", IfcDomain.Architecture, Seq("BEAM", "JOIST", "HOLLOWCORE", "LINTEL", "SPANDREL", "T_BEAM", "GIRDER_SEGMENT", "DIAPHRAGM", "PIERCAP", "HATSTONE", "CORNICE", "EDGEBEAM"));
    public static readonly IfcClass Door          = new("IfcDoor", IfcDomain.Architecture, Seq("DOOR", "GATE", "TRAPDOOR", "BOOM_BARRIER", "TURNSTILE"));
    public static readonly IfcClass Window        = new("IfcWindow", IfcDomain.Architecture, Seq("WINDOW", "SKYLIGHT", "LIGHTDOME"));
    public static readonly IfcClass Covering      = new("IfcCovering", IfcDomain.Architecture, Seq("CEILING", "FLOORING", "CLADDING", "ROOFING", "INSULATION", "MEMBRANE", "SLEEVING", "WRAPPING", "MOLDING", "SKIRTINGBOARD", "TOPPING"));
    public static readonly IfcClass CurtainWall   = new("IfcCurtainWall", IfcDomain.Architecture, Seq<string>());
    public static readonly IfcClass Railing       = new("IfcRailing", IfcDomain.Architecture, Seq("HANDRAIL", "GUARDRAIL", "BALUSTRADE", "FENCE"));
    public static readonly IfcClass Ramp          = new("IfcRamp", IfcDomain.Architecture, Seq("STRAIGHT_RUN_RAMP", "TWO_STRAIGHT_RUN_RAMP", "QUARTER_TURN_RAMP", "TWO_QUARTER_TURN_RAMP", "HALF_TURN_RAMP", "SPIRAL_RAMP"));
    public static readonly IfcClass Roof          = new("IfcRoof", IfcDomain.Architecture, Seq("FLAT_ROOF", "SHED_ROOF", "GABLE_ROOF", "HIP_ROOF", "HIPPED_GABLE_ROOF", "GAMBREL_ROOF", "MANSARD_ROOF", "BARREL_ROOF", "RAINBOW_ROOF", "BUTTERFLY_ROOF", "PAVILION_ROOF", "DOME_ROOF", "FREEFORM"));
    public static readonly IfcClass Stair         = new("IfcStair", IfcDomain.Architecture, Seq("STRAIGHT_RUN_STAIR", "TWO_STRAIGHT_RUN_STAIR", "QUARTER_WINDING_STAIR", "QUARTER_TURN_STAIR", "HALF_WINDING_STAIR", "HALF_TURN_STAIR", "TWO_QUARTER_WINDING_STAIR", "TWO_QUARTER_TURN_STAIR", "THREE_QUARTER_WINDING_STAIR", "THREE_QUARTER_TURN_STAIR", "SPIRAL_STAIR", "DOUBLE_RETURN_STAIR", "CURVED_RUN_STAIR", "TWO_CURVED_RUN_STAIR", "LADDER"));
    public static readonly IfcClass Plate         = new("IfcPlate", IfcDomain.Architecture, Seq("CURTAIN_PANEL", "SHEET", "FLANGE_PLATE", "WEB_PLATE", "STIFFENER_PLATE", "GUSSET_PLATE", "COVER_PLATE", "BASE_PLATE", "SPLICE_PLATE"));
    public static readonly IfcClass Member        = new("IfcMember", IfcDomain.Architecture, Seq("BRACE", "CHORD", "COLLAR", "MEMBER", "MULLION", "PLATE", "POST", "PURLIN", "RAFTER", "STRINGER", "STRUT", "STUD", "STIFFENING_RIB", "ARCH_SEGMENT", "SUSPENSION_CABLE", "SUSPENDER", "STAY_CABLE", "TIEBAR"));
    public static readonly IfcClass Footing       = new("IfcFooting", IfcDomain.Structural, Seq("CAISSON_FOUNDATION", "FOOTING_BEAM", "PAD_FOOTING", "PILE_CAP", "STRIP_FOOTING"));
    public static readonly IfcClass Pile          = new("IfcPile", IfcDomain.Structural, Seq("BORED", "DRIVEN", "JETGROUTING", "COHESION", "FRICTION", "SUPPORT"));
    public static readonly IfcClass FlowSegment   = new("IfcFlowSegment", IfcDomain.HvacFire, Seq<string>());
    public static readonly IfcClass FlowFitting   = new("IfcFlowFitting", IfcDomain.HvacFire, Seq<string>());
    public static readonly IfcClass FlowTerminal  = new("IfcFlowTerminal", IfcDomain.HvacFire, Seq<string>());
    public static readonly IfcClass FlowController= new("IfcFlowController", IfcDomain.HvacFire, Seq<string>());
    public static readonly IfcClass DistributionPort = new("IfcDistributionPort", IfcDomain.Electrical, Seq("CABLE", "CABLECARRIER", "DUCT", "PIPE", "WIRELESS"));
    public static readonly IfcClass StructuralCurveMember   = new("IfcStructuralCurveMember", IfcDomain.Structural, Seq("RIGID_JOINED_MEMBER", "PIN_JOINED_MEMBER", "CABLE", "TENSION_MEMBER", "COMPRESSION_MEMBER"));
    public static readonly IfcClass StructuralSurfaceMember = new("IfcStructuralSurfaceMember", IfcDomain.Structural, Seq("BENDING_ELEMENT", "MEMBRANE_ELEMENT", "SHELL"));
    public static readonly IfcClass StructuralPointConnection = new("IfcStructuralPointConnection", IfcDomain.Structural, Seq<string>());
    public static readonly IfcClass Space         = new("IfcSpace", IfcDomain.Architecture, Seq("SPACE", "PARKING", "GFA", "INTERNAL", "EXTERNAL", "BERTH"));
    public static readonly IfcClass Proxy         = new("IfcBuildingElementProxy", IfcDomain.Architecture, Seq("COMPLEX", "ELEMENT", "PARTIAL", "PROVISIONFORVOID", "PROVISIONFORSPACE"));
    public static readonly IfcClass Bridge        = new("IfcBridge", IfcDomain.Infrastructure, Seq("ARCHED", "CABLE_STAYED", "CANTILEVER", "CULVERT", "FRAMEWORK", "GIRDER", "SUSPENSION", "TRUSS"));
    public static readonly IfcClass Road          = new("IfcRoad", IfcDomain.Infrastructure, Seq<string>());
    public static readonly IfcClass Railway       = new("IfcRailway", IfcDomain.Infrastructure, Seq<string>());
    public static readonly IfcClass MarineFacility= new("IfcMarineFacility", IfcDomain.Infrastructure, Seq("CANAL", "WATERWAYSHIPLIFT", "REVETMENT", "LAUNCHRECOVERY", "MARINEDEFENCE", "HYDROLIFT", "SHIPYARD", "SHIPLIFT", "PORT", "QUAY", "FLOATINGDOCK", "NAVIGATIONALCHANNEL", "BREAKWATER", "DRYDOCK", "JETTY", "SLIPWAY"));
    public static readonly IfcClass Course        = new("IfcCourse", IfcDomain.Infrastructure, Seq("ARMOUR", "FILTER", "PAVEMENT", "PROTECTION"));
    public static readonly IfcClass Pavement      = new("IfcPavement", IfcDomain.Infrastructure, Seq("FLEXIBLE", "RIGID"));
    public static readonly IfcClass Rail          = new("IfcRail", IfcDomain.Infrastructure, Seq("RACKRAIL", "BLADE", "GUARDRAIL", "STOCKRAIL", "CHECKRAIL", "RAIL"));
    public static readonly IfcClass Alignment     = new("IfcAlignment", IfcDomain.Infrastructure, Seq<string>());
    public static readonly IfcClass EarthworksFill= new("IfcEarthworksFill", IfcDomain.Geotechnical, Seq("BACKFILL", "COUNTERWEIGHT", "EMBANKMENT", "SLOPEFILL", "SUBGRADE", "SUBGRADEBED", "TRANSITIONSECTION"));
    public static readonly IfcClass EarthworksCut = new("IfcEarthworksCut", IfcDomain.Geotechnical, Seq("BASE_EXCAVATION", "CUT", "DREDGING", "OVEREXCAVATION", "PAVEMENTMILLING", "STEPEXCAVATION", "TOPSOILREMOVAL", "TRENCH"));
    public static readonly IfcClass GeotechnicalStratum = new("IfcGeotechnicalStratum", IfcDomain.Geotechnical, Seq("SOLID", "VOID", "WATER"));
    public static readonly IfcClass Borehole      = new("IfcBorehole", IfcDomain.Geotechnical, Seq<string>());

    public IfcDomain Domain { get; }
    public Seq<string> ValidPredefined { get; }

    public Fin<PredefinedType> AdmitPredefined(string token, string objectType) =>
        token.Trim().ToUpperInvariant() switch {
            "" or "NOTDEFINED"            => Fin.Succ(PredefinedType.NotDefined),
            "USERDEFINED" or "OBJECTTYPE" => PredefinedType.TryCreate(objectType.Trim()).ToFin(new BimFault.UnmappedClass($"predefined-objecttype-miss:{Key}").ToError()),
            var value when ValidPredefined.IsEmpty || ValidPredefined.Contains(value) => Fin.Succ(PredefinedType.Create(value)),
            var value                     => Fin.Fail<PredefinedType>(new BimFault.UnmappedClass($"predefined-reject:{Key}:{value}").ToError()),
        };
}

public enum IfcDomain : byte {
    Architecture = 0, Structural = 1, HvacFire = 2, Electrical = 3, Plumbing = 4, Infrastructure = 5, Geotechnical = 6,
}

[ValueObject<string>]
public sealed partial class PredefinedType {
    static partial void NormalizeValidate(ref string value) => value = value.Trim().ToUpperInvariant();

    public static readonly PredefinedType NotDefined = PredefinedType.Create("NOTDEFINED");

    public bool IsUserDefined => Value is "USERDEFINED" or "OBJECTTYPE";
}

public sealed record BimElement(
    string GlobalId,
    IfcClass Class,
    PredefinedType Predefined,
    string Name,
    string Tag,
    GeometryHandle Geometry,
    Seq<PropertyBinding> Properties,
    Seq<QuantityBinding> Quantities,
    Seq<BimMaterial> Materials,
    Seq<ClassificationRef> Classifications,
    Option<string> TypeGlobalId,
    Option<string> SpatialContainerId) {
    public sealed record PropertyBinding(string SetName, string Name, string Value);
    public sealed record QuantityBinding(string SetName, string Name, double Value, string Unit);
}

public sealed record BimModel(
    ReleaseVersion Schema,
    ModelView View,
    Seq<BimElement> Elements,
    Seq<BimType> Types,
    Map<string, Seq<string>> Zones,
    double Tolerance,
    Instant At) {
    public static readonly BimModel Empty = new(
        ReleaseVersion.IFC4X3_ADD2, ModelView.Ifc4Reference,
        Seq<BimElement>(), Seq<BimType>(), Map<string, Seq<string>>(), 1e-6, Instant.MinValue);

    public Option<BimType> TypeOf(BimElement element) =>
        element.TypeGlobalId.Bind(id => Types.Find(t => t.GlobalId == id));

    public static Fin<BimModel> Project(IfcSemanticModel semantic, ClockPolicy clocks) =>
        semantic.Types.TraverseM(BimType.Project).As()
            .Bind(types => semantic.Products
                .TraverseM(row => IfcClass.TryGet(row.EntityType)
                    .ToFin(new BimFault.UnmappedClass($"element-class-miss:{row.EntityType}").ToError())
                    .Bind(cls => cls.AdmitPredefined(row.PredefinedType, row.ObjectType)
                        .Map(predefined => new BimElement(
                            row.GlobalId, cls, predefined, row.Name, row.Tag, GeometryHandle.Pending(row.GlobalId),
                            semantic.Properties.Filter(p => p.OwnerGlobalId == row.GlobalId)
                                .Map(p => new BimElement.PropertyBinding(p.SetName, p.PropertyName, p.Value)),
                            semantic.Quantities.Filter(q => q.OwnerGlobalId == row.GlobalId)
                                .Map(q => new BimElement.QuantityBinding(q.SetName, q.QuantityName, q.Value, q.Unit)),
                            semantic.Materials.Filter(m => m.OwnerGlobalId == row.GlobalId).Map(BimMaterial.Of),
                            semantic.Classifications.Filter(c => c.OwnerGlobalId == row.GlobalId)
                                .Choose(c => from system in Classification.TryGet(c.System)
                                             from code in ClassificationCode.TryCreate(c.Code).ToOption()
                                             select new ClassificationRef(row.GlobalId, system, code, c.DictionaryClassUri)),
                            row.TypeGlobalId,
                            semantic.Spatial.Find(node => node.ContainedGlobalIds.Contains(row.GlobalId)).Map(static node => node.GlobalId)))))
                .As()
                .Map(elements => new BimModel(semantic.Schema, semantic.View, elements, types,
                    BimZone.IndexOf(semantic.Zones), semantic.Tolerance, clocks.Now)));
}
```

## [3]-[BIM_TYPE]

- Owner: `BimType` the host-neutral type-object record promoting the thin `Exchange/import#IMPORT_RAIL` `TypeRow` into a first-class owner carrying the `IfcElementType`/`IfcTypeProduct` discriminant, the type-bound `PropertySet`/material/`PredefinedType` bindings the occurrence inherits, and the `IfcRepresentationMap` instanced-geometry key an `IfcMappedItem` occurrence references; `BimTypeKind` the `[SmartEnum<string>]` over the IFC type-object class string so a 4000-identical-window model carries one `BimType` plus 4000 occurrence references.
- Entry: `BimType.Project(IfcSemanticModel.TypeRow row)` projects one type row into the typed `BimType`, admitting the type's own predefined token through the resolved `IfcClass.AdmitPredefined` and threading the representation-map content key — `Fin<T>` aborts on an unmapped type class (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) lowered with `.ToError()`; `BimModel.TypeOf(element)` resolves an occurrence's `TypeGlobalId` against the model's `Types` index so the `Semantics/properties#PROPERTY_SETS` `QTO_TYPEDRIVENOVERRIDE` inheritance fold reads a typed source rather than a re-filtered flat row set.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new type-object class is one `BimTypeKind` row; a new type-bound inheritance is one column on `BimType` projected from the widened `TypeRow`; the representation-map instancing is one content key the occurrence reads by reference; never a per-type-class record and never an inlined occurrence geometry.
- Boundary: the `BimType` is the type half of the type-occurrence factoring — the occurrence reads the type by `TypeGlobalId` reference and inlining the type geometry per occurrence is the deleted form; the `IfcRepresentationMap` instanced-geometry library is keyed by a content key the `Exchange/reconstruct#RECONSTRUCTION` instanced-geometry reuse and the `Exchange/tessellation#TESSELLATION_BRIDGE` re-import both join, never a re-tessellation per occurrence; the type-bound property/material/predefined bindings flow to the occurrence through the `Semantics/properties#PROPERTY_SETS` `QTO_TYPEDRIVENOVERRIDE` fold, the occurrence overriding the type, never a second property model; the `BimType` projects from the widened `Exchange/import#IMPORT_RAIL` `Extract<IfcTypeObject>` `TypeRow` consumed as settled vocabulary.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class BimTypeKind {
    public static readonly BimTypeKind WallType   = new("IfcWallType");
    public static readonly BimTypeKind SlabType    = new("IfcSlabType");
    public static readonly BimTypeKind ColumnType  = new("IfcColumnType");
    public static readonly BimTypeKind BeamType    = new("IfcBeamType");
    public static readonly BimTypeKind DoorType    = new("IfcDoorType");
    public static readonly BimTypeKind WindowType  = new("IfcWindowType");
    public static readonly BimTypeKind CoveringType= new("IfcCoveringType");
    public static readonly BimTypeKind FlowSegmentType = new("IfcFlowSegmentType");
    public static readonly BimTypeKind FlowFittingType = new("IfcFlowFittingType");
    public static readonly BimTypeKind FlowTerminalType= new("IfcFlowTerminalType");
    public static readonly BimTypeKind ElementType = new("IfcElementType");
}

public sealed record BimType(
    string GlobalId,
    BimTypeKind Kind,
    string Name,
    PredefinedType Predefined,
    Seq<BimElement.PropertyBinding> Properties,
    Seq<BimMaterial> Materials,
    Option<UInt128> RepresentationMapKey) {
    public static Fin<BimType> Project(IfcSemanticModel.TypeRow row) =>
        BimTypeKind.TryGet(row.EntityType)
            .ToFin(new BimFault.UnmappedClass($"type-class-miss:{row.EntityType}").ToError())
            .Map(kind => new BimType(
                row.GlobalId, kind, row.Name,
                row.PredefinedType.Trim() is "" or "NOTDEFINED" or "USERDEFINED" ? PredefinedType.NotDefined : PredefinedType.Create(row.PredefinedType.Trim().ToUpperInvariant()),
                row.Properties.Map(p => new BimElement.PropertyBinding(p.SetName, p.PropertyName, p.Value)),
                row.Materials.Map(BimMaterial.Of),
                row.RepresentationMapKey));
}
```

## [4]-[RESEARCH]

- [ELEMENT_PROJECTION]: the `BimModel.Project` fold over the `IfcSemanticModel` product/property/quantity/material/type rows and the `IfcClass` closed-vocabulary case list ground against the GeometryGym entity-class surface (`.api/api-geometrygym-ifc` architectural built-element / MEP distribution / structural-analysis / infrastructure-geotechnics families) and the `GeometryHandle` shape at cross-folder alignment with the kernel `Rasm` geometry owner; the per-class `IfcXxxTypeEnum` valid-predefined value sets the `IfcClass` rows freeze confirm against the GeometryGym per-class predefined enum members so the `AdmitPredefined` valid-set check matches the schema before the row table is final.
- [GEOMETRY_HANDLE]: the `GeometryHandle` the `BimElement` carries — the by-reference binding to the kernel `Rasm` geometry the tessellation bridge re-imports — confirms its shape against the kernel geometry owner so the element model never re-tessellates and never carries a host-bound geometry type.
- [TYPE_OCCURRENCE_MAP]: the `BimType` `IfcRepresentationMap`/`IfcMappedItem` instanced-geometry projection grounds against the GeometryGym `IfcTypeProduct.RepresentationMaps`/`IfcMappedItem.MappingSource`/`MappingTarget` traversal (`.api/api-geometrygym-ifc` type-occurrence entrypoint scope) so the representation-map content key the occurrence references matches the real type library; the type-bound `IfcTypeObject.HasPropertySets` inheritance the `QTO_TYPEDRIVENOVERRIDE` fold reads confirms the type-vs-occurrence property override before the `BimType` property binding is final.
