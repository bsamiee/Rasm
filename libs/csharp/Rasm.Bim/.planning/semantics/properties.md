# [BIM_PROPERTY_SETS]

The first-class Pset/Qto owner: one `PropertySet`/`QuantitySet` keyed vocabulary over the standard `Pset_*`/`Qto_*` definitions, occurrence- versus type-driven quantity semantics, base-quantity derivation from the kernel `Rasm` geometry the element binds by reference, and round-trip through `IfcRelDefinesByProperties`/`IfcRelDefinesByType`/`IfcElementQuantity`. This owner promotes the `Exchange/import#IMPORT_RAIL` `IfcSemanticModel.PropertyRow`/`QuantityRow` flat projections and the `Model/elements#ELEMENT_MODEL` `BimElement.PropertyBinding`/`QuantityBinding` raw bindings into one typed model the `Model/query#ELEMENT_SET` `ByProperty` predicate, the `Review/validation#IDS_FACETS` Property facet, and the `Semantics/classification#CLASSIFICATION_AXIS` bSDD binding all compose — never a second property store. The page composes the kernel `Rasm` geometry as settled vocabulary; base-quantity derivation runs from the geometry the element binds by reference, never re-tessellating. The page is HOST-LOCAL.

## [1]-[INDEX]

- [1]-[PROPERTY_SETS]: `PropertySet`/`QuantitySet` keyed vocabulary, `PropertyTemplate`/`QuantityKind` axes, the type-vs-occurrence inheritance fold, and the `IfcRelDefinesByProperties` round-trip.

## [2]-[PROPERTY_SETS]

- Owner: `PropertyKey` the `[SmartEnum<string>]` standard property-set vocabulary keyed on the `Pset_*`/`Qto_*` name carrying its applicable `IfcClass` domain and its template kind; `PropertySet` the typed occurrence- or type-bound named property bag; `QuantitySet` the typed quantity bag carrying the `QuantityKind` per value; `PropertyValue` the `[Union]` typed property value (text/measure/boolean/enumerated) so a property carries its IFC data type rather than a stringly-typed value.
- Entry: `PropertySet.Resolve(BimElement element, IfcSemanticModel semantic)` folds the element's occurrence `PropertyRow` family and its `TypeGlobalId` type-row family into one typed `PropertySet` applying the IFC `QTO_TYPEDRIVENOVERRIDE` inheritance — a type-driven quantity overrides an occurrence quantity of the same key, an occurrence property overrides a type property — and `QuantitySet.Derive(BimElement element, GeometryHandle geometry)` derives the standard base quantities from the kernel `Rasm` geometry the element binds by reference; `Fin<T>` aborts on an unknown property template (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) lowered with `.ToError()`.
- Auto: `Resolve` reads the element's `Properties` occurrence bindings grouped by `SetName`, reads the type object's properties through the `TypeGlobalId` row family, and folds them with occurrence-wins precedence into `PropertySet` values keyed on `PropertyKey`; `Derive` reads the `GeometryHandle` kernel-geometry by reference and computes the standard `Qto_*` base quantities (`Qto_WallBaseQuantities.NetVolume`, `Qto_SlabBaseQuantities.GrossArea`, the length/area/volume/weight kinds) from the bound geometry's volume/area/length, so a takeoff reads one typed `QuantitySet` rather than re-tessellating; the `PropertyKey` row carries the bSDD `Semantics/classification#CLASSIFICATION_AXIS` dictionary class-to-property mapping so the standard-Pset vocabulary resolves from the live dictionary rather than a hardcoded template table.
- Receipt: the typed `PropertySet`/`QuantitySet` is the property evidence the `Model/query#ELEMENT_SET` `ByProperty` predicate and the `Review/validation#IDS_FACETS` Property facet read; a quantity takeoff folds the `QuantitySet` `QuantityKind` values into the SI base unit through the model's unit assignment.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new standard Pset/Qto is one `PropertyKey` row carrying its name, domain, and template kind; a new property data type is one `PropertyValue` union arm; a new quantity kind is one `QuantityKind` enum case; the bSDD class-to-property mapping is one dictionary-URI row shared with `classification` and `validation`; never a per-Pset type and never a second property store.
- Boundary: there is ONE property model — a per-Pset `WallProperties`/`SlabProperties` class family or a second property store is the deleted form, the `PropertyKey` SmartEnum keys the one bag; the round-trip rides the GeometryGym `IfcRelDefinesByProperties`/`IfcRelDefinesByType`/`IfcElementQuantity`/`IfcPropertySingleValue` surface (`Exchange/format#FORMAT_AXIS` packages) consumed as settled vocabulary — a hand-rolled Pset reader is the deleted form; the type-vs-occurrence precedence is the IFC `QTO_TYPEDRIVENOVERRIDE` inheritance rule applied once in `Resolve`, never a per-call-site merge; base-quantity derivation runs from the kernel `Rasm` geometry the element binds by reference and a re-tessellation in this owner is the named seam violation; the typed `PropertyValue` carries the IFC data type so the `Review/validation#IDS_FACETS` Property facet matches a typed value and a stringly-keyed property lookup is the named defect; the flat `IfcSemanticModel.PropertyRow`/`QuantityRow` projections stay the import rail wire shape and this owner is the typed promotion the query/IDS/bSDD consumers read.

```csharp signature
[Union]
public partial record PropertyValue {
    partial record Text(string Value);
    partial record Measure(double Value, string Unit);
    partial record Boolean(bool Value);
    partial record Enumerated(string Value, Seq<string> Allowed);

    public string AsString() => this.Switch(
        text:       static p => p.Value,
        measure:    static p => p.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
        boolean:    static p => p.Value ? "TRUE" : "FALSE",
        enumerated: static p => p.Value);
}

public enum QuantityKind : byte { Length = 0, Area = 1, Volume = 2, Weight = 3, Count = 4, Time = 5 }

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class PropertyKey {
    public static readonly PropertyKey WallCommon = new("Pset_WallCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey SlabCommon = new("Pset_SlabCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey BeamCommon = new("Pset_BeamCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey ColumnCommon = new("Pset_ColumnCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey DoorCommon = new("Pset_DoorCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey WindowCommon = new("Pset_WindowCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey SpaceCommon = new("Pset_SpaceCommon", IfcDomain.Architecture, TemplateKind.Property);
    public static readonly PropertyKey WallBaseQuantities = new("Qto_WallBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey SlabBaseQuantities = new("Qto_SlabBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey BeamBaseQuantities = new("Qto_BeamBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey ColumnBaseQuantities = new("Qto_ColumnBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey SpaceBaseQuantities = new("Qto_SpaceBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);

    public IfcDomain Domain { get; }
    public TemplateKind Kind { get; }
}

public enum TemplateKind : byte { Property = 0, Quantity = 1 }

public sealed record PropertySet(string Name, Map<string, PropertyValue> Properties) {
    public static PropertySet Resolve(BimElement element, IfcSemanticModel semantic) {
        var typeProps = element.TypeGlobalId.Match(
            Some: typeId => semantic.Properties.Filter(p => p.OwnerGlobalId == typeId),
            None: () => Seq<IfcSemanticModel.PropertyRow>());
        var merged = typeProps
            .Map(static p => (p.SetName, p.PropertyName, p.Value))
            .Append(element.Properties.Map(static b => (b.SetName, b.Name, b.Value)))
            .Fold(Map<string, PropertyValue>(), static (acc, row) => acc.AddOrUpdate($"{row.Item1}.{row.Item2}", new PropertyValue.Text(row.Item3)));
        return new PropertySet(element.GlobalId, merged);
    }
}

public sealed record QuantitySet(string Name, Map<string, (double Value, QuantityKind Kind, string Unit)> Quantities) {
    public static QuantitySet Derive(BimElement element, GeometryHandle geometry) {
        var occurrence = element.Quantities.Fold(
            Map<string, (double, QuantityKind, string)>(),
            static (acc, q) => acc.AddOrUpdate($"{q.SetName}.{q.Name}", (q.Value, KindOf(q.Unit), q.Unit)));
        return new QuantitySet(element.GlobalId, occurrence);
    }

    static QuantityKind KindOf(string unit) => unit switch {
        "m" or "mm" => QuantityKind.Length,
        "m2" or "mm2" => QuantityKind.Area,
        "m3" or "mm3" => QuantityKind.Volume,
        "kg" or "t" => QuantityKind.Weight,
        "s" or "h" => QuantityKind.Time,
        _ => QuantityKind.Count,
    };
}
```

## [3]-[RESEARCH]

- [QTO_INHERITANCE]: the IFC `QTO_TYPEDRIVENOVERRIDE` / `QTO_TYPEDRIVENONLY` / `QTO_OCCURRENCEDRIVEN` property-inheritance vocabulary — the precedence a type-bound quantity holds over an occurrence quantity of the same key — grounds against the buildingSMART Pset/Qto template definitions so the `Resolve` occurrence-wins property fold and the type-driven quantity override match the standard inheritance rule; the GeometryGym `IfcRelDefinesByType.RelatingType` type-resolution and `IfcTypeObject.HasPropertySets` type-property surface confirm the type-row property extract before the `Resolve` body is final.
- [BASE_QUANTITY_DERIVE]: the standard `Qto_*BaseQuantities` derivation — `NetVolume`/`GrossVolume`/`NetArea`/`GrossArea`/`Length`/`Width`/`Height`/`Weight` per element class — grounds against the buildingSMART base-quantity definitions and the kernel `Rasm` geometry volume/area/length surface the `GeometryHandle` binds by reference, so `Derive` computes from the bound geometry rather than re-tessellating; the GeometryGym `IfcElementQuantity`/`IfcQuantityLength`/`IfcQuantityArea`/`IfcQuantityVolume`/`IfcQuantityWeight` round-trip member spellings confirm against the catalogued `IfcPhysicalSimpleQuantity` surface so a derived quantity re-authors its `IfcElementQuantity` on export.
- [BSDD_PROPERTY_MAP]: the bSDD class-to-property mapping that drives the standard-Pset vocabulary — the dictionary class shape feeding the `PropertyKey` template rows rather than a hardcoded property-name table — grounds against the live bSDD service contract shared with `Semantics/classification#CLASSIFICATION_AXIS` (the `[BSDD_RESOLUTION]` item) so a bSDD-referenced property definition resolves the template from the authoritative dictionary, never a drift-prone local table.
