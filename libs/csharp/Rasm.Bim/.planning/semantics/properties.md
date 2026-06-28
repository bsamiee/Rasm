# [BIM_PROPERTY_TEMPLATES]

The IFC Pset/Qto TEMPLATE authority over the `Rasm.Element` seam graph: the `PropertyKey` `[SmartEnum<string>]` standard `Pset_*`/`Qto_*` vocabulary, the bSDD-resolved template that supplies each property's IFC `DataType` and Pset placement, the `PropertyInheritance` classifier that stamps each seam `PropertySet`/`QuantitySet` node with its `InheritanceMode` at ingest so the seam `Bake` applies the correct type→occurrence precedence [H1], and the `QuantityDerivation` base-quantity fold deriving the standard `Qto_*BaseQuantities` from the kernel geometry the seam node references by content key. The typed VALUE half is RETIRED to the seam: `PropertyValue` (`Text`/`Measure`/`Boolean`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`), `MeasureValue` (`quantityType`/`Si`/`canonicalUnit` over the `Dimension` value-object [H2]), and the old `QuantityKind` enum now live on `Rasm.Element/Properties` — this page no longer owns the value, it owns the TEMPLATE the projector threads into the seam value and the precedence policy the seam `Bake` reads. The stringly `PropertyBinding`/`QuantityBinding` records the retired `BimElement` carried are GONE: a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, never a `(SetName, Name, string Value)` triple. The page is the typed promotion the `Projection/semantic#SEMANTIC_PROJECTOR` projector composes at ingress and the `Review/validation#IDS_FACETS` Property facet reads; base-quantity derivation runs from the kernel geometry measures the node references by content key, never re-tessellating, and the bSDD template resolves from the live dictionary, never a frozen `Pset_*` table that drifts.

## [01]-[INDEX]

- [01]-[PROPERTY_TEMPLATES]: `PropertyKey` `[SmartEnum<string>]` standard Pset/Qto vocabulary, `TemplateKind` axis, the bSDD `Resolve` template supplying each property's IFC `DataType`/Pset placement, and the `PropertyInheritance` `InheritanceMode` classifier stamped on each seam bag node at ingest [H1].
- [02]-[BASE_QUANTITIES]: `BaseQuantityTable` the per-`IfcClass` `Qto_*BaseQuantities` derivation roster and `QuantityDerivation.Derive` the base-quantity fold over the kernel geometry measures the seam node references by content key, producing the seam `QuantitySet` node values under derived-wins precedence.

## [02]-[PROPERTY_TEMPLATES]

- Owner: `PropertyKey` the `[SmartEnum<string>]` standard property-set vocabulary keyed on the `Pset_*`/`Qto_*` name carrying its applicable `IfcDomain` and its `TemplateKind`; `TemplateKind` the property-vs-quantity template axis; `PropertyInheritance` the classifier mapping a Pset/Qto name (and whether it is type-bound) onto the seam `InheritanceMode` (`OccurrenceWins`/`TypeDrivenOverride`/`TypeDrivenOnly`) the projector stamps on each seam `PropertySet`/`QuantitySet` node at ingest [H1]. The typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned (`Rasm.Element/Properties`); this page supplies the TEMPLATE (the `DataType`/placement) the seam value is constructed from, never the value itself.
- Entry: `PropertyKey.Resolve(string classCode, BsddClass dictionary)` resolves the template for an IFC class — it unions the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass.Properties` dictionary rows over the static `PropertyKey` anchors (dictionary-wins on a `PropertySet.Code` collision) so each property carries its dictionary-declared `DataType`/Pset placement and a bSDD-declared property with no static anchor still resolves; `PropertyInheritance.ModeOf(string setName, bool typeDriven)` returns the seam `InheritanceMode` for a bag the projector is stamping; `Fin<T>` is not the rail here — template resolution degrades to the static anchors when the dictionary is unreachable (`Semantics/classification#BSDD_RESOLUTION` `LocalShape`), never faulting ingest.
- Auto: `Resolve` reads the `BsddClass.Properties` rows the dictionary returns (each `BsddProperty` carrying `Code`/`DataType`/`PropertySet`/`PredefinedValue`/`IsRequired`), appends the static `TemplatesFor(domain)` anchors as `IfcText` defaults, and de-duplicates by `{PropertySet}.{Code}` so the projector threads each property's `DataType` into the seam `PropertyValue.Of(value, dataType)` — an `IfcMeasure` datatype routing through the seam `MeasureValue` UnitsNet SI coercion, every other type carrying the verbatim typed value; `ModeOf` reads the `InheritanceTable` frozen rows — a `Qto_*BaseQuantities` set is `TypeDrivenOverride` (a type-bound quantity overrides an occurrence quantity of the same key), a `Pset_*Common` set is `OccurrenceWins` (an occurrence property overrides a type property), a type-only template (`Pset_*TypeCommon`) is `TypeDrivenOnly` — so the seam `Bake` applies the correct precedence wholly within the seam, never a per-call-site merge [H1].
- Receipt: the resolved template is the typing evidence the projector threads into the seam `PropertyValue`; the stamped `InheritanceMode` is the precedence evidence the seam `Bake` reads when folding a `DefinesByType` edge into the occurrence; the `Review/validation#IDS_FACETS` Property facet reads the seam `PropertyValue` the template typed.
- Packages: GeometryGymIFC_Core, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new standard Pset/Qto is one `PropertyKey` row carrying its name, domain, and template kind; a new property data type is one seam `PropertyValue` union arm (not this page); a new inheritance policy is one `InheritanceTable` row; the bSDD class-to-property mapping is one dictionary-URI row shared with `classification` and `validation`; never a per-Pset type and never a second property store.
- Boundary: there is ONE property model and it is the seam graph — the retired `BimElement.PropertyBinding`/`QuantityBinding(string, string, string)` stringly triples are GONE, a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, and a per-Pset `WallProperties`/`SlabProperties` class family is the deleted form; the typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned [H2] and re-declaring it here is the named drift defect — this page owns the TEMPLATE (`DataType`/placement) and the PRECEDENCE policy, the seam owns the value; the type-vs-occurrence precedence is the IFC `QTO_TYPEDRIVENOVERRIDE` inheritance stamped as the seam `InheritanceMode` at ingest [H1] and applied once in the seam `Bake`, never a per-call-site merge and never a stored-twice type→occurrence fold; the template resolves from the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass.Properties` dictionary mapping, never a frozen `Pset_*` table that drifts — the static `PropertyKey` rows are the well-known anchors the bSDD resolution enriches, not the authoritative source; the bag round-trip rides the GeometryGym `IfcRelDefinesByProperties`/`IfcRelDefinesByType`/`IfcElementQuantity` surface consumed as settled vocabulary (`Projection/semantic#RELATION_ALGEBRA` neutral `Associate` edges), and a hand-rolled Pset reader is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
public enum TemplateKind : byte { Property = 0, Quantity = 1 }

// --- [MODELS] -----------------------------------------------------------------------------
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

    public static Seq<PropertyKey> TemplatesFor(IfcDomain domain) =>
        Items.Filter(row => row.Kind == TemplateKind.Property && row.Domain == domain).ToSeq();

    // Dictionary-driven template resolution: the bSDD ClassProperty rows supply each property's DataType/Pset
    // placement, unioned over the static anchors (dictionary-wins), so the projector threads each DataType into
    // the seam PropertyValue.Of(value, dataType). The static rows seed the well-known set; the dictionary enriches.
    public static Seq<BsddProperty> Resolve(IfcDomain domain, BsddClass dictionary) =>
        dictionary.Properties
            .Filter(static p => p.PropertySet.Length > 0)
            .Append(TemplatesFor(domain)
                .Map(static key => new BsddProperty(key.Key, key.Key, "IfcText", key.Key, "", IsRequired: false)))
            .DistinctBy(static p => $"{p.PropertySet}.{p.Code}");
}

// The InheritanceMode classifier [H1]: the projector stamps each seam PropertySet/QuantitySet node with its
// precedence policy at ingest so the seam Bake applies type->occurrence precedence wholly within the seam. A
// Qto_*BaseQuantities is TypeDrivenOverride, a Pset_*Common is OccurrenceWins, a *TypeCommon is TypeDrivenOnly.
public static class PropertyInheritance {
    static readonly FrozenDictionary<string, InheritanceMode> InheritanceTable = new Dictionary<string, InheritanceMode>(StringComparer.Ordinal) {
        ["Qto_WallBaseQuantities"]   = InheritanceMode.TypeDrivenOverride,
        ["Qto_SlabBaseQuantities"]   = InheritanceMode.TypeDrivenOverride,
        ["Qto_BeamBaseQuantities"]   = InheritanceMode.TypeDrivenOverride,
        ["Qto_ColumnBaseQuantities"] = InheritanceMode.TypeDrivenOverride,
        ["Qto_SpaceBaseQuantities"]  = InheritanceMode.TypeDrivenOverride,
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public static InheritanceMode ModeOf(string setName, bool typeDriven) =>
        InheritanceTable.TryGetValue(setName, out var mode) ? mode
        : setName.EndsWith("TypeCommon", StringComparison.Ordinal) ? InheritanceMode.TypeDrivenOnly
        : typeDriven ? InheritanceMode.TypeDrivenOverride
        : InheritanceMode.OccurrenceWins;
}
```

## [03]-[BASE_QUANTITIES]

- Owner: `BaseQuantityTable` the per-`IfcClass` derivation roster (which `Qto_*BaseQuantities` set and which dimensions a class derives from the geometry); `QuantityDerivation` the base-quantity fold deriving the standard base quantities from `GeometryMeasures` — the kernel `Rasm` value-object `GeometryMeasures(Option<double> Length, Option<double> Area, Option<double> Volume)` the kernel/Compute resolves from the geometry the seam `Object` node references by content key (`Model/elements#REPRESENTATION_KEYS` `RepresentationContentHash`) and supplies to `Derive` (Bim consumes the measure, never tessellates it) — producing the seam `QuantitySet` node values as seam `MeasureValue` under derived-wins precedence.
- Entry: `QuantityDerivation.Derive(IfcClass cls, GeometryMeasures measures, Map<PropertyName, MeasureValue> occurrence)` derives the standard base quantities for a class from the kernel geometry measures and merges them over the occurrence-stored quantities under derived-wins precedence (the geometry-true takeoff supersedes an authoring tool's stored quantity), returning the seam `QuantitySet` node value map; a class with no `BaseQuantityTable` row returns the occurrence quantities unchanged so a non-takeoff class never blocks.
- Auto: `Derive` reads the `BaseQuantityTable` row for the class (the `Qto_*` set name and the `Dimension` set — `LengthDim`/`AreaDim`/`VolumeDim`/`MassDim`) and folds each dimension from the kernel `GeometryMeasures` (the already-SI-base `Length`/`Area`/`Volume` the kernel computes from the geometry resolved by content key, never re-tessellated here) into a seam `MeasureValue` over its canonical `Dimension` row [H2] (the kernel scalar already SI-base, wrapped directly, never re-coerced), keyed `{QtoName}.{dimensionName}`, merged over the occurrence map with derived-wins so the 5D `Planning/cost#ESTIMATE` join reads the geometry-true measure; an element-set aggregate of the same `Dimension` reduces through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum` reducer, never a manual `double` fold.
- Packages: Rasm.Element, GeometryGymIFC_Core, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new base-quantity derivation is one `BaseQuantityTable` row keyed on the `IfcClass`; a new dimension is one seam `Dimension` the row lists; the derived quantities merge over the occurrence map under one precedence rule; never a per-class `Derive` method and never a re-tessellation in this owner.
- Boundary: base-quantity derivation runs from the kernel `GeometryMeasures` value-object (`Option<double>` `Length`/`Area`/`Volume`, the same kernel `Rasm` owner the `Dimension` value-object rides) the kernel/Compute resolves from the `RepresentationContentHash` geometry and injects into `Derive`, so a Bim-local `GeometryMeasures` re-declaration or an in-owner geometry-measure computation is the deleted form (Bim depends UP on the kernel and never owns geometry measurement); a re-tessellation in this owner is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the derived value is a seam `MeasureValue` (the seam owns the typed quantity over `Dimension` + UnitsNet [H2]) and a Bim-local `MeasureValue` re-declaration is the deleted form; the derived-wins precedence is applied once in `Derive`, never a per-call-site merge; the `BaseQuantityTable` is a frozen data table keyed on the `IfcClass`, never enumerated `switch` arms or a per-class derive method.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Per-class base-quantity derivation: which Qto set and which seam Dimensions a class derives from the kernel
// geometry. One frozen row table [H2 replaces the 6-value QuantityKind with the seam Dimension value-object].
public readonly record struct BaseQuantityRow(string QtoName, Seq<Dimension> Dimensions);

public static class BaseQuantityTable {
    static readonly FrozenDictionary<string, BaseQuantityRow> Rows = new Dictionary<string, BaseQuantityRow>(StringComparer.Ordinal) {
        ["IfcWall"]   = new("Qto_WallBaseQuantities",   Seq(Dimension.LengthDim, Dimension.AreaDim, Dimension.VolumeDim)),
        ["IfcSlab"]   = new("Qto_SlabBaseQuantities",   Seq(Dimension.AreaDim, Dimension.VolumeDim)),
        ["IfcColumn"] = new("Qto_ColumnBaseQuantities", Seq(Dimension.LengthDim, Dimension.VolumeDim)),
        ["IfcBeam"]   = new("Qto_BeamBaseQuantities",   Seq(Dimension.LengthDim, Dimension.VolumeDim)),
        ["IfcSpace"]  = new("Qto_SpaceBaseQuantities",  Seq(Dimension.AreaDim, Dimension.VolumeDim)),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    public static Option<BaseQuantityRow> For(IfcClass cls) => Rows.TryGetValue(cls.Key, out var row) ? Some(row) : None;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class QuantityDerivation {
    // Geometry-true base quantities (derived-wins) merged over the occurrence quantities: the kernel geometry
    // takeoff supersedes an authoring tool's stored quantity, each derived value a seam MeasureValue over the
    // seam Dimension row [H2], the kernel measure already SI-base. GeometryMeasures(Option<double> Length, Area,
    // Volume) is the kernel Rasm value-object the kernel/Compute resolves from the Object RepresentationContentHash
    // geometry and injects here — Bim consumes the measure, never computes or re-tessellates it.
    public static Map<PropertyName, MeasureValue> Derive(IfcClass cls, GeometryMeasures measures, Map<PropertyName, MeasureValue> occurrence) =>
        BaseQuantityTable.For(cls).Match(
            None: () => occurrence,
            Some: row => row.Dimensions.Fold(occurrence, (acc, dimension) =>
                Measure(measures, dimension).Match(
                    Some: si => acc.AddOrUpdate(new PropertyName($"{row.QtoName}.{NameOf(dimension)}"), si),
                    None: () => acc)));

    // The kernel GeometryMeasures are already SI-base, so each wraps directly into the seam MeasureValue over its
    // canonical Dimension row — never a re-coercion through the UnitsNet registry (the seam Of path is for raw
    // unit-bearing values, not an already-SI kernel scalar); an absent measure yields None and skips the row.
    static Option<MeasureValue> Measure(GeometryMeasures measures, Dimension dimension) =>
        dimension == Dimension.LengthDim ? measures.Length.Map(static m => new MeasureValue(Dimension.LengthDim, m, "m"))
        : dimension == Dimension.AreaDim ? measures.Area.Map(static m => new MeasureValue(Dimension.AreaDim, m, "m²"))
        : dimension == Dimension.VolumeDim ? measures.Volume.Map(static m => new MeasureValue(Dimension.VolumeDim, m, "m³"))
        : None;

    static string NameOf(Dimension dimension) =>
        dimension == Dimension.LengthDim ? "Length"
        : dimension == Dimension.AreaDim ? "GrossArea"
        : dimension == Dimension.VolumeDim ? "NetVolume"
        : "Weight";
}
```

## [04]-[RESEARCH]

- [TEMPLATE_RESOLUTION]: the `PropertyKey.Resolve` bSDD-over-static template fold grounds against the `Semantics/classification#BSDD_RESOLUTION` `BsddClass`/`BsddProperty` evidence (`.api/api-bsdd` `ClassPropertyContract.v1` `propertyCode`/`dataType`/`propertySet`/`predefinedValue`/`isRequired`) so the projector threads each property's `DataType` into the seam `Rasm.Element/Properties` `PropertyValue.Of(value, dataType)`; the static `PropertyKey` rows are the well-known anchors the dictionary enriches, never the authoritative source, so a new standard becomes a dictionary-URI row not a hardcoded table.
- [INHERITANCE_STAMP]: the `PropertyInheritance.ModeOf` `InheritanceMode` classifier grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT H1 (stamp each PropertySet/QuantitySet node with a structural `InheritanceMode` — `OccurrenceWins`/`TypeDrivenOverride`/`TypeDrivenOnly` — at Bim ingest so `Bake` applies correct type→occurrence precedence wholly within the seam) and the IFC `QTO_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENONLY`/`QTO_OCCURRENCEDRIVEN` inheritance vocabulary; the projector reads `ModeOf` and stamps the seam node, the seam `Bake` reading the stamp when folding a `DefinesByType` edge so the precedence is never re-derived per call site.
- [BASE_QUANTITY_DERIVE]: the `QuantityDerivation.Derive` base-quantity fold grounds against the buildingSMART `Qto_*BaseQuantities` definitions (`NetVolume`/`GrossArea`/`Length`/`Weight` per element class) and the kernel `Rasm` geometry measures the seam node references by content key, deriving from the resolved geometry rather than re-tessellating; the value-half retirement grounds against `ELEMENT-REBUILD-PLAN.md` §4B (the typed VALUE family `PropertyValue`/`MeasureValue` is seam-owned, library-neutral) and §4-RT H2 (the 6-value `QuantityKind` replaced by the seam `Dimension` value-object over the 7 SI base dimensions + a UnitsNet `QuantityType` discriminator), so this page owns the TEMPLATE and the PRECEDENCE, the seam owns the value.
