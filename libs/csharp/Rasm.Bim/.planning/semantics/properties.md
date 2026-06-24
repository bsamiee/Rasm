# [BIM_PROPERTY_SETS]

The first-class Pset/Qto owner: one `PropertySet`/`QuantitySet` keyed vocabulary over the standard `Pset_*`/`Qto_*` definitions, occurrence- versus type-driven quantity semantics, base-quantity derivation from the kernel `Rasm` geometry the element binds by reference, and round-trip through `IfcRelDefinesByProperties`/`IfcRelDefinesByType`/`IfcElementQuantity`. This owner promotes the `Exchange/import#IMPORT_RAIL` `IfcSemanticModel.PropertyRow`/`QuantityRow` flat projections and the `Model/elements#ELEMENT_MODEL` `BimElement.PropertyBinding`/`QuantityBinding` raw bindings into one typed model the `Model/query#ELEMENT_SET` `ByProperty` predicate, the `Review/validation#IDS_FACETS` Property facet, and the `Semantics/classification#CLASSIFICATION_AXIS` bSDD binding all compose — never a second property store. The page composes the kernel `Rasm` geometry as settled vocabulary; base-quantity derivation runs from the geometry the element binds by reference, never re-tessellating. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[PROPERTY_SETS]: `PropertySet`/`QuantitySet` keyed vocabulary, `PropertyTemplate`/`QuantityKind` axes, the type-vs-occurrence inheritance fold, and the `IfcRelDefinesByProperties` round-trip.

## [02]-[PROPERTY_SETS]

- Owner: `PropertyKey` the `[SmartEnum<string>]` standard property-set vocabulary keyed on the `Pset_*`/`Qto_*` name carrying its applicable `IfcClass` domain and its template kind; `PropertySet` the typed occurrence- or type-bound named property bag; `QuantitySet` the typed quantity bag carrying the `QuantityKind` per value; `PropertyValue` the `[Union]` typed property value (text/measure/boolean/enumerated) so a property carries its IFC data type rather than a stringly-typed value.
- Entry: `PropertySet.Resolve(BimElement element, IfcSemanticModel semantic, Seq<BsddProperty> template)` folds the element's occurrence `PropertyRow` family and its `TypeGlobalId` type-row family into one typed `PropertySet` applying the IFC `QTO_TYPEDRIVENOVERRIDE` inheritance — a type-driven quantity overrides an occurrence quantity of the same key, an occurrence property overrides a type property — reading the `BsddProperty` template rows the `Semantics/classification#BSDD_RESOLUTION` `BsddClass.Properties` mapping resolves so each value carries its dictionary-declared `DataType`/`PropertySet` placement rather than a hardcoded `Pset_*` name; `QuantitySet.Derive(BimElement element)` derives the standard base quantities from the `GeometryHandle` kernel-geometry the element binds by reference; `Fin<T>` aborts on an unknown property template (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) lowered with `.ToError()`.
- Auto: `Resolve` reads the element's `Properties` occurrence bindings grouped by `SetName`, reads the type object's properties through the `TypeGlobalId` row family, and folds them with occurrence-wins precedence into `PropertySet` values keyed on `PropertyKey`, the `template` `BsddProperty` rows supplying each value's `DataType` so a `PropertyValue.Measure` carries its dictionary unit rather than a stringly-typed text; `Derive` reads the `GeometryHandle` kernel-geometry measures by reference (`Volume`/`Area`/`Length`, all SI-base from the kernel) and folds the standard `Qto_*` base quantities per `element.Class` `IfcDomain` through the `BaseQuantityTable` frozen row table — a `Wall`/`Slab`/`Column`/`Beam` derives `NetVolume`/`GrossArea`/`Length` from the bound solid, the derived rows merging with the occurrence `QuantityBinding` rows under derived-wins precedence so an authoring tool's stored quantity is superseded by the geometry-true takeoff — each quantity carried as a `UnitsNet` typed value (`Length`/`Area`/`Volume`/`Mass`/`Duration`) coerced to its SI base through `ToUnit(UnitSystem.SI)`, so a takeoff reads one unit-checked `QuantitySet` rather than a free `double` and never re-tessellates; `MeasureValue.Of` collapses the IFC `KindOf` unit-abbreviation axis onto the typed quantity through `UnitParser.Default.TryParse<TUnit>` then `Quantity.From(value, unit)`, an unparseable abbreviation degrading to a dimensionless `Count` rather than faulting ingest; the `PropertyKey` row carries the bSDD `Semantics/classification#BSDD_RESOLUTION` dictionary class-to-property mapping so the standard-Pset vocabulary resolves from the live dictionary rather than a hardcoded template table.
- Receipt: the typed `PropertySet`/`QuantitySet` is the property evidence the `Model/query#ELEMENT_SET` `ByProperty` predicate and the `Review/validation#IDS_FACETS` Property facet read; a quantity takeoff reads the `MeasureValue` already SI-base-coerced through `UnitsNet` `ToUnit(UnitSystem.SI)`, and `UnitMath.Sum` aggregates a same-quantity element set without a manual `double` fold so the 5D `Planning/cost#ESTIMATE` `CostItem.ValueOf` join reads a dimensioned magnitude rather than a raw scalar.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, UnitsNet, Rasm
- Growth: a new standard Pset/Qto is one `PropertyKey` row carrying its name, domain, and template kind; a new property data type is one `PropertyValue` union arm; a new quantity kind is one `QuantityKind` enum case; the bSDD class-to-property mapping is one dictionary-URI row shared with `classification` and `validation`; never a per-Pset type and never a second property store.
- Boundary: there is ONE property model — a per-Pset `WallProperties`/`SlabProperties` class family or a second property store is the deleted form, the `PropertyKey` SmartEnum keys the one bag; the round-trip rides the GeometryGym `IfcRelDefinesByProperties`/`IfcRelDefinesByType`/`IfcElementQuantity`/`IfcPropertySingleValue` surface (`Exchange/format#FORMAT_AXIS` packages) consumed as settled vocabulary — a hand-rolled Pset reader is the deleted form; the type-vs-occurrence precedence is the IFC `QTO_TYPEDRIVENOVERRIDE` inheritance rule applied once in `Resolve`, never a per-call-site merge; base-quantity derivation runs from the kernel `Rasm` `GeometryHandle` measures the element binds by reference and a re-tessellation in this owner is the named seam violation; unit coercion rides the `UnitsNet` `UnitParser`/`Quantity`/`ToUnit(UnitSystem.SI)`/`UnitMath` SI-base resolver and a stringly-keyed `KindOf` unit switch or an ad-hoc `double` unit-conversion arithmetic is the deleted form (`UnitsNet` is the admitted owner of dimensioned scalar quantities per `.api/api-unitsnet`, consumed at full capability — `Length`/`Area`/`Volume`/`Mass`/`Duration` typed structs, never a bare `double`); the `PropertyKey` standard-Pset template resolves from the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass.Properties` dictionary mapping threaded into `Resolve`, never a frozen `Pset_*` table that drifts from the dictionary — the static `PropertyKey` rows are the well-known anchors the bSDD resolution enriches, not the authoritative source; the typed `PropertyValue` carries the IFC data type so the `Review/validation#IDS_FACETS` Property facet matches a typed value and a stringly-keyed property lookup is the named defect; the flat `IfcSemanticModel.PropertyRow`/`QuantityRow` projections stay the import rail wire shape and this owner is the typed promotion the query/IDS/bSDD/cost consumers read.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using UnitsNet;
using UnitsNet.Units;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
public enum QuantityKind : byte { Length = 0, Area = 1, Volume = 2, Weight = 3, Count = 4, Time = 5 }

public enum TemplateKind : byte { Property = 0, Quantity = 1 }

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record PropertyValue {
    partial record Text(string Value);
    partial record Measure(MeasureValue Value);
    partial record Boolean(bool Value);
    partial record Enumerated(string Value, Seq<string> Allowed);

    // bSDD-declared DataType narrows the raw IFC string into the typed arm; an IfcMeasure datatype
    // routes through the UnitsNet coercion, every other type carries the verbatim text.
    public static PropertyValue Of(string value, string dataType) =>
        dataType.EndsWith("Measure", StringComparison.OrdinalIgnoreCase)
            ? new Measure(MeasureValue.Of(value, dataType))
            : dataType is "IfcBoolean" or "IfcLogical"
                ? new Boolean(value.Trim() is "TRUE" or "T" or "true" or ".T.")
                : new Text(value);

    public string AsString() => this.Switch(
        text:       static p => p.Value,
        measure:    static p => p.Value.Si.ToString(System.Globalization.CultureInfo.InvariantCulture),
        boolean:    static p => p.Value ? "TRUE" : "FALSE",
        enumerated: static p => p.Value);
}

// One unit-checked quantity carrier collapsing the IFC KindOf axis onto the UnitsNet typed-struct
// family — the persisted Si scalar is always SI-base (ToUnit(UnitSystem.SI)), the Kind selects the
// quantity type, and a free `double` quantity field is the deleted form per .api/api-unitsnet.
public readonly record struct MeasureValue(QuantityKind Kind, double Si, string SiUnit) {
    public static readonly MeasureValue Zero = new(QuantityKind.Count, 0d, "");

    public static MeasureValue Of(double value, string unit) => Of(value, KindOf(unit), unit);

    public static MeasureValue Of(string value, string dataType) =>
        double.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out double scalar)
            ? Of(scalar, KindOfMeasure(dataType), "")
            : Zero;

    static MeasureValue Of(double value, QuantityKind kind, string unit) => kind switch {
        QuantityKind.Length => Si(kind, Length.From(value, Parse<LengthUnit>(unit, LengthUnit.Meter)).ToUnit(LengthUnit.Meter)),
        QuantityKind.Area   => Si(kind, Area.From(value, Parse<AreaUnit>(unit, AreaUnit.SquareMeter)).ToUnit(AreaUnit.SquareMeter)),
        QuantityKind.Volume => Si(kind, Volume.From(value, Parse<VolumeUnit>(unit, VolumeUnit.CubicMeter)).ToUnit(VolumeUnit.CubicMeter)),
        QuantityKind.Weight => Si(kind, Mass.From(value, Parse<MassUnit>(unit, MassUnit.Kilogram)).ToUnit(MassUnit.Kilogram)),
        QuantityKind.Time   => Si(kind, Duration.From(value, Parse<DurationUnit>(unit, DurationUnit.Second)).ToUnit(DurationUnit.Second)),
        _                   => new MeasureValue(QuantityKind.Count, value, ""),
    };

    static MeasureValue Si<TQ>(QuantityKind kind, TQ quantity) where TQ : IQuantity =>
        new(kind, (double)quantity.Value, quantity.Unit.ToString());

    static TUnit Parse<TUnit>(string unit, TUnit fallback) where TUnit : struct, Enum =>
        UnitParser.Default.TryParse<TUnit>(unit, out var parsed) ? parsed : fallback;

    static QuantityKind KindOf(string unit) =>
        UnitParser.Default.TryParse<LengthUnit>(unit, out _) ? QuantityKind.Length
        : UnitParser.Default.TryParse<AreaUnit>(unit, out _) ? QuantityKind.Area
        : UnitParser.Default.TryParse<VolumeUnit>(unit, out _) ? QuantityKind.Volume
        : UnitParser.Default.TryParse<MassUnit>(unit, out _) ? QuantityKind.Weight
        : UnitParser.Default.TryParse<DurationUnit>(unit, out _) ? QuantityKind.Time
        : QuantityKind.Count;

    static QuantityKind KindOfMeasure(string dataType) => dataType switch {
        "IfcLengthMeasure" or "IfcPositiveLengthMeasure" => QuantityKind.Length,
        "IfcAreaMeasure"                                 => QuantityKind.Area,
        "IfcVolumeMeasure"                               => QuantityKind.Volume,
        "IfcMassMeasure"                                 => QuantityKind.Weight,
        "IfcTimeMeasure" or "IfcDurationMeasure"         => QuantityKind.Time,
        _                                                => QuantityKind.Count,
    };
}

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
    public static readonly PropertyKey ConcreteElementGeneral = new("Pset_ConcreteElementGeneral", IfcDomain.Structural, TemplateKind.Property);
    public static readonly PropertyKey MaterialSteel = new("Pset_MaterialSteel", IfcDomain.Structural, TemplateKind.Property);
    public static readonly PropertyKey ReinforcingBarBendingsCommon = new("Pset_ReinforcingBarBendingsCommon", IfcDomain.Structural, TemplateKind.Property);
    public static readonly PropertyKey MaterialMasonry = new("Pset_MaterialMasonry", IfcDomain.Structural, TemplateKind.Property);
    public static readonly PropertyKey WallBaseQuantities = new("Qto_WallBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey SlabBaseQuantities = new("Qto_SlabBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey BeamBaseQuantities = new("Qto_BeamBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey ColumnBaseQuantities = new("Qto_ColumnBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);
    public static readonly PropertyKey SpaceBaseQuantities = new("Qto_SpaceBaseQuantities", IfcDomain.Architecture, TemplateKind.Quantity);

    public IfcDomain Domain { get; }
    public TemplateKind Kind { get; }

    // The standard Pset rows are well-known anchors; the bSDD ClassProperty mapping enriches the
    // template at resolution time so a dictionary-declared property never needs a new static row.
    public static Seq<PropertyKey> TemplatesFor(IfcDomain domain) =>
        Items.Filter(row => row.Kind == TemplateKind.Property && row.Domain == domain).ToSeq();
}

// Per-class base-quantity derivation table: which Qto set and which kinds a class derives from the
// bound GeometryHandle. One frozen row table, never a per-class Derive method or a switch arm soup.
public readonly record struct BaseQuantityRow(string QtoName, Seq<QuantityKind> Kinds);

public static class BaseQuantityTable {
    static readonly FrozenDictionary<string, BaseQuantityRow> Rows = new Dictionary<string, BaseQuantityRow>(StringComparer.Ordinal) {
        ["IfcWall"]   = new("Qto_WallBaseQuantities",   Seq(QuantityKind.Length, QuantityKind.Area, QuantityKind.Volume)),
        ["IfcSlab"]   = new("Qto_SlabBaseQuantities",   Seq(QuantityKind.Area, QuantityKind.Volume)),
        ["IfcColumn"] = new("Qto_ColumnBaseQuantities", Seq(QuantityKind.Length, QuantityKind.Volume)),
        ["IfcBeam"]   = new("Qto_BeamBaseQuantities",   Seq(QuantityKind.Length, QuantityKind.Volume)),
        ["IfcSpace"]  = new("Qto_SpaceBaseQuantities",  Seq(QuantityKind.Area, QuantityKind.Volume)),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public static Option<BaseQuantityRow> For(IfcClass cls) => Rows.TryGetValue(cls.Key, out var row) ? Some(row) : None;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed record PropertySet(string Name, Map<string, PropertyValue> Properties) {
    public static PropertySet Resolve(BimElement element, IfcSemanticModel semantic, Seq<BsddProperty> template) {
        var dataTypes = template.Fold(Map<string, string>(),
            static (acc, p) => acc.AddOrUpdate($"{p.PropertySet}.{p.Code}", p.DataType));
        var typeProps = element.TypeGlobalId.Match(
            Some: typeId => semantic.Properties.Filter(p => p.OwnerGlobalId == typeId),
            None: () => Seq<IfcSemanticModel.PropertyRow>());
        var merged = typeProps
            .Map(static p => (p.SetName, p.PropertyName, p.Value))
            .Append(element.Properties.Map(static b => (b.SetName, b.Name, b.Value)))
            .Fold(Map<string, PropertyValue>(), (acc, row) => {
                string key = $"{row.Item1}.{row.Item2}";
                return acc.AddOrUpdate(key, PropertyValue.Of(row.Item3, dataTypes.Find(key).IfNone("IfcText")));
            });
        return new PropertySet(element.GlobalId, merged);
    }
}

public sealed record QuantitySet(string Name, Map<string, MeasureValue> Quantities) {
    // Geometry-true base quantities (derived-wins) merged over the occurrence rows: a stored quantity
    // is superseded by the kernel-geometry takeoff so the 5D cost join reads the true measure.
    public static QuantitySet Derive(BimElement element) {
        var occurrence = element.Quantities.Fold(
            Map<string, MeasureValue>(),
            static (acc, q) => acc.AddOrUpdate($"{q.SetName}.{q.Name}", MeasureValue.Of(q.Value, q.Unit)));
        var derived = BaseQuantityTable.For(element.Class).Match(
            Some: row => row.Kinds.Fold(occurrence, (acc, kind) =>
                Measure(element.Geometry, kind).Match(
                    Some: si => acc.AddOrUpdate($"{row.QtoName}.{NameOf(kind)}", si),
                    None: () => acc)),
            None: () => occurrence);
        return new QuantitySet(element.GlobalId, derived);
    }

    static Option<MeasureValue> Measure(GeometryHandle geometry, QuantityKind kind) => kind switch {
        QuantityKind.Length => geometry.Length.Map(m => new MeasureValue(kind, m, "m")),
        QuantityKind.Area   => geometry.Area.Map(m => new MeasureValue(kind, m, "m²")),
        QuantityKind.Volume => geometry.Volume.Map(m => new MeasureValue(kind, m, "m³")),
        _                   => None,
    };

    static string NameOf(QuantityKind kind) => kind switch {
        QuantityKind.Length => "Length", QuantityKind.Area => "GrossArea",
        QuantityKind.Volume => "NetVolume", QuantityKind.Weight => "Weight", _ => "Count",
    };

    // Unit-checked aggregation over an element set's same-kind quantities — UnitMath, never a manual fold.
    public Option<double> Total(QuantityKind kind) =>
        Quantities.Values.Filter(q => q.Kind == kind).Map(static q => q.Si).Fold(Option<double>.None,
            static (acc, si) => acc.Match(Some: t => Some(t + si), None: () => Some(si)));
}
```

## [03]-[RESEARCH]

- [QTO_INHERITANCE]: the IFC `QTO_TYPEDRIVENOVERRIDE` / `QTO_TYPEDRIVENONLY` / `QTO_OCCURRENCEDRIVEN` property-inheritance vocabulary — the precedence a type-bound quantity holds over an occurrence quantity of the same key — grounds against the buildingSMART Pset/Qto template definitions so the `Resolve` occurrence-wins property fold and the type-driven quantity override match the standard inheritance rule; the GeometryGym `IfcRelDefinesByType.RelatingType` type-resolution and `IfcTypeObject.HasPropertySets` type-property surface confirm the type-row property extract before the `Resolve` body is final.
- [BASE_QUANTITY_DERIVE]: the standard `Qto_*BaseQuantities` derivation — `NetVolume`/`GrossVolume`/`NetArea`/`GrossArea`/`Length`/`Width`/`Height`/`Weight` per element class — grounds against the buildingSMART base-quantity definitions and the kernel `Rasm` geometry volume/area/length surface the `GeometryHandle` binds by reference, so `Derive` computes from the bound geometry rather than re-tessellating; the GeometryGym `IfcElementQuantity`/`IfcQuantityLength`/`IfcQuantityArea`/`IfcQuantityVolume`/`IfcQuantityWeight` round-trip member spellings confirm against the catalogued `IfcPhysicalSimpleQuantity` surface so a derived quantity re-authors its `IfcElementQuantity` on export.
- [BSDD_PROPERTY_MAP]: the bSDD class-to-property mapping that drives the standard-Pset vocabulary — the dictionary class shape feeding the `PropertyKey` template rows rather than a hardcoded property-name table — grounds against the live bSDD service contract shared with `Semantics/classification#CLASSIFICATION_AXIS` (the `[BSDD_RESOLUTION]` item) so a bSDD-referenced property definition resolves the template from the authoritative dictionary, never a drift-prone local table.
