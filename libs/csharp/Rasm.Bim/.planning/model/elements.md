# [BIM_ELEMENTS]

The host-neutral BIM element vocabulary: one `BimElement` record discriminated by an `IfcClass` row, the `BimModel` element-collection owner, and the `BimModel.Project` fold from the `IfcSemanticModel` graph. The element model is HOST-NEUTRAL — the geometry handle binds the kernel `Rasm` geometry by reference and never carries a RhinoCommon type — and projects from the `exchange/import-rail#IMPORT_RAIL` `IfcSemanticModel` graph consumed as settled vocabulary, never re-minting the semantic-graph shape.

## [1]-[INDEX]

- [2]-[ELEMENT_MODEL]: `BimElement` element record, `IfcClass` discriminant, `BimModel` collection, and the `Project` fold.

## [2]-[ELEMENT_MODEL]

- Owner: `BimElement` the single host-neutral element record carrying the `IfcClass` discriminant, the stable `GlobalId`, the kernel-geometry handle, and the property/quantity/material/type bindings; `IfcClass` `[SmartEnum<string>]` the closed buildingSMART entity-class vocabulary keyed on the IFC entity-type string; `BimModel` the element-collection owner wrapping the projected element graph with the schema/model-view header.
- Entry: `BimModel.Project(IfcSemanticModel semantic, ClockPolicy clocks)` projects the in-process IFC semantic graph into the host-neutral `BimModel` element collection — `Fin<T>` aborts on an unmapped entity class or a missing spatial host; the geometry handle binds the kernel `Rasm` geometry by reference, never re-tessellating.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm
- Growth: a new element class is one `IfcClass` row; a new element binding is one column on `BimElement` projected from the existing `IfcSemanticModel` row family; a new IFC4.3 infrastructure class rides the same projection on one new row; never a per-element-class type.
- Boundary: `BimElement` is ONE record discriminated by `IfcClass` row data — a `WallElement`/`SlabElement`/`ColumnElement` class family is the deleted form mirroring the no-per-material-type law; the geometry handle is the kernel `Rasm` geometry consumed by reference and a RhinoCommon `Brep`/`Mesh` field on `BimElement` is the named seam violation; the `IfcSemanticModel` graph is owned at `exchange/import-rail#IMPORT_RAIL` and consumed as settled vocabulary; the element-class vocabulary is the closed `IfcClass` SmartEnum and a raw entity-type string crossing a public signature is the named defect; property/quantity/material/classification/type bindings project from the existing `IfcSemanticModel.PropertyRow`/`QuantityRow`/`MaterialRow`/`ClassificationRow`/`TypeRow` families, never a second property model; the `Classifications` binding carries the typed `ClassificationRef` owned at `classification/systems#CLASSIFICATION_AXIS` so the `query/element-set#ELEMENT_SET` `ByClassification` arm matches a typed system-and-code pair rather than a stringly-keyed property lookup.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class IfcClass {
    public static readonly IfcClass Wall = new("IfcWall", IfcDomain.Building);
    public static readonly IfcClass Slab = new("IfcSlab", IfcDomain.Building);
    public static readonly IfcClass Column = new("IfcColumn", IfcDomain.Building);
    public static readonly IfcClass Beam = new("IfcBeam", IfcDomain.Building);
    public static readonly IfcClass Door = new("IfcDoor", IfcDomain.Building);
    public static readonly IfcClass Window = new("IfcWindow", IfcDomain.Building);
    public static readonly IfcClass Space = new("IfcSpace", IfcDomain.Spatial);
    public static readonly IfcClass Proxy = new("IfcBuildingElementProxy", IfcDomain.Building);
    public static readonly IfcClass Bridge = new("IfcBridge", IfcDomain.Infrastructure);
    public static readonly IfcClass Road = new("IfcRoad", IfcDomain.Infrastructure);
    public static readonly IfcClass Railway = new("IfcRailway", IfcDomain.Infrastructure);
    public static readonly IfcClass Pavement = new("IfcPavement", IfcDomain.Infrastructure);

    public IfcDomain Domain { get; }
}

public enum IfcDomain : byte { Building = 0, Spatial = 1, Infrastructure = 2 }

public sealed record BimElement(
    string GlobalId,
    IfcClass Class,
    string Name,
    string Tag,
    GeometryHandle Geometry,
    Seq<PropertyBinding> Properties,
    Seq<QuantityBinding> Quantities,
    Seq<string> Materials,
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
    double Tolerance,
    Instant At) {
    public static Fin<BimModel> Project(IfcSemanticModel semantic, ClockPolicy clocks) =>
        semantic.Products
            .TraverseM(row => IfcClass.TryGet(row.EntityType)
                .ToFin(new BimFault.ModelRejected($"<element-class-miss:{row.EntityType}>"))
                .Map(cls => new BimElement(
                    row.GlobalId, cls, row.Name, row.Tag, GeometryHandle.Pending(row.GlobalId),
                    semantic.Properties.Filter(p => p.OwnerGlobalId == row.GlobalId)
                        .Map(p => new BimElement.PropertyBinding(p.SetName, p.PropertyName, p.Value)),
                    semantic.Quantities.Filter(q => q.OwnerGlobalId == row.GlobalId)
                        .Map(q => new BimElement.QuantityBinding(q.SetName, q.QuantityName, q.Value, q.Unit)),
                    semantic.Materials.Filter(m => m.OwnerGlobalId == row.GlobalId).Map(static m => m.MaterialName),
                    semantic.Classifications.Filter(c => c.OwnerGlobalId == row.GlobalId)
                        .Choose(c => from system in Classification.TryGet(c.System)
                                     from code in ClassificationCode.TryCreate(c.Code).ToOption()
                                     select new ClassificationRef(row.GlobalId, system, code, c.DictionaryClassUri)),
                    row.TypeGlobalId,
                    semantic.Spatial.Find(node => node.ContainedGlobalIds.Contains(row.GlobalId)).Map(static node => node.GlobalId))))
            .As()
            .Map(elements => new BimModel(semantic.Schema, semantic.View, elements, semantic.Tolerance, clocks.Now));
}
```

## [3]-[RESEARCH]

- [ELEMENT_PROJECTION]: the `BimModel.Project` fold over the `IfcSemanticModel` product/property/quantity/material/type rows and the `IfcClass` closed-vocabulary case list ground against the GeometryGym entity-class surface and the `GeometryHandle` shape at cross-folder alignment with the kernel `Rasm` geometry owner.
- [GEOMETRY_HANDLE]: the `GeometryHandle` the `BimElement` carries — the by-reference binding to the kernel `Rasm` geometry the tessellation bridge re-imports — confirms its shape against the kernel geometry owner so the element model never re-tessellates and never carries a host-bound geometry type.
