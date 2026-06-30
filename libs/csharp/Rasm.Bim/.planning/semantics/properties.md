# [BIM_PROPERTY_TEMPLATES]

The IFC Pset/Qto TEMPLATE authority over the `Rasm.Element` seam graph: the offline `Xbim.Properties` `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` buildingSMART catalogue is the canonical, schema-versioned, network-free template floor — every standard `Pset_*` and `Qto_*`, its `ApplicableClasses`, each property's IFC `DataType`/value-type kind, and each base quantity's `QtoTypeEnum` — and the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass` dictionary enriches it (dictionary-wins on a name collision). `PropertyKey.Resolve` unions the two into one `PropertyTemplate` map; `PropertyInheritance.ModeOf` reads the authoritative IFC `templatetype` the catalogue declares to stamp each seam `PropertySet`/`QuantitySet` node with its `InheritanceMode` at ingest so the seam `Bake` applies the correct type→occurrence precedence [H1]; `QuantityDerivation.Derive` sources the class's base-quantity set from the catalogue and folds the geometry-true takeoff from the kernel geometry the node references by content key [H2]. The typed VALUE half is RETIRED to the seam: the `PropertyValue` ten-case family (`Text`/`Measure`/`Boolean`/`Logical`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex` — `Logical` the three-valued `IfcLogical`/`IfcLogicalEnum` `UNKNOWN`, `Complex` the named nested `IfcComplexProperty` bag) and the `MeasureValue` (`QuantityType`/`Dimension`/`Si`/`CanonicalUnit` over the `Dimension` `[ComplexValueObject]` + the `QuantityType` `[ValueObject<string>]` discriminator [H2]) live on `Rasm.Element/Properties` — this page owns the TEMPLATE (which properties a class carries and their declared types) and the PRECEDENCE policy, never the value. The stringly `PropertyBinding`/`QuantityBinding` triples the retired `BimElement` carried are GONE: a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, never a `(SetName, Name, string Value)` triple. The page is the template oracle the `Projection/semantic#SEMANTIC_PROJECTOR` projector and the `Review/validation#IDS_FACETS` Property facet read; a hand-coded `Pset_*` property table beside the `Xbim.Properties` catalogue is the named defect this owner closes.

## [01]-[INDEX]

- [01]-[PROPERTY_TEMPLATES]: `PropertyCatalog` the `Xbim.Properties` offline standard-template catalogue (loaded once per IFC schema), `PropertyTemplate` the unified resolved-template shape both sources lower into, `PropertyKey` the curated well-known `Pset_*` recognition anchors, `PropertyKey.Resolve` the catalogue-floor ∪ bSDD-live template union, and `PropertyInheritance.ModeOf` the `templatetype`-driven `InheritanceMode` classifier stamped on each seam bag node at ingest [H1].
- [02]-[BASE_QUANTITIES]: `PropertyCatalog.BaseQuantitySet` the per-`IfcClass` `Qto_*BaseQuantities` set + the geometry-relevant `Dimension`s its `QtoDef`s declare (from the catalogue, never a hand-listed slice), and `QuantityDerivation.Derive` the base-quantity fold deriving the geometry-true takeoff (incl. `NetWeight` from volume × material density) from the kernel geometry measures the seam node references by content key, producing the seam `QuantitySet` node values under derived-wins precedence.

## [02]-[PROPERTY_TEMPLATES]

- Owner: `PropertyCatalog` the offline `Xbim.Properties` template catalogue — `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` loaded once per IFC `Version` and cached, the always-available buildingSMART template floor declaring what every `Pset_*`/`Qto_*` IS (its `ApplicableClasses`, its `PropertyDef`s with their `DataType`/value-type kind, its `QtoDef`s with their `QtoTypeEnum`); `PropertyTemplate` the unified resolved template (`Set`/`Code`/`DataType`/`Kind`/`Required`) both the catalogue `PropertyDef` and the bSDD `BsddProperty` lower into; `PropertyKey` the curated well-known `Pset_*` recognition anchors (the opinionated common set name + its `IfcDomain`) authoring surfaces first; `PropertyInheritance` the classifier reading the catalogue's authoritative IFC `templatetype` onto the seam `InheritanceMode` [H1]. The typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned (`Rasm.Element/Properties`); this page supplies the TEMPLATE (which properties, their declared `DataType`) the seam value is constructed against, never the value.
- Entry: `PropertyKey.Resolve(IfcClass cls, ReleaseVersion schema, Option<BsddClass> dictionary)` resolves a class's property templates — `PropertyCatalog.Templates(cls, schema)` (the offline `Xbim.Properties` floor, every `PropertySetDef` whose `ApplicableClasses` names the class) unioned UNDER the live `BsddClass.Properties` dictionary rows (dictionary-wins on a `{Set}.{Code}` collision), so a measured property carries its declared IFC `DataType` and the offline catalogue resolves when bSDD is unreachable; `PropertyCatalog.TemplateTypeOf(string setName)` is the `internal` catalogue query returning the raw IFC `templatetype` a set declares (the `Xbim.Properties` enum kept Bim-internal, never a public seam return); `PropertyInheritance.ModeOf(string setName, bool typeBound)` is the public canonical surface returning the seam `InheritanceMode` the projector stamps on a bag at ingest; `Fin<T>` is not the rail here — resolution degrades to the offline catalogue (and to the structural inference) when the dictionary is unreachable, never faulting ingest.
- Auto: `Resolve` folds the bSDD `BsddProperty` rows (lowered to `PropertyTemplate` carrying each property's `DataType`/`ValueKind`/`IsRequired`) OVER the `PropertyCatalog.Templates` floor with `AddOrUpdate` so a dictionary-declared property overrides the offline default and a bSDD-only property still resolves; `PropertyCatalog.Templates` reads `definitions.DefinitionSets`, keeps the sets whose `ApplicableClasses` name the `IfcClass`, and lowers each `PropertyDef` through its `PropertyType.PropertyValueType` value-type kind in ONE pass (`LowerValue`): the scalar IFC data-type token off `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyReferenceValue` `DataType.Type` (a `DataTypeEnum`) and `TypeSimpleProperty.DataType.Type`, the same kind selecting the `BsddValueKind` the seam `PropertyValue` arm is chosen from (`PropertyDef.ValueDef` is the `[Obsolete]` legacy min/max/default slot and carries NO data type, the composite enumerated/list/table/complex kinds no scalar token), so the projector and the IDS facet know each property's expected type without re-deriving it; `ModeOf` reads `PropertyCatalog.TemplateTypeOf` (the `templatetype` enum the `PropertySetDef` declares) — `PSET_TYPEDRIVENONLY`/`QTO_TYPEDRIVENONLY` is `TypeDrivenOnly`, `PSET_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENOVERRIDE` is `TypeDrivenOverride`, every other declared kind (`PSET_OCCURRENCEDRIVEN`/`PSET_PERFORMANCEDRIVEN`/`PSET_PROFILEDRIVEN`/`PSET_MATERIALDRIVEN`) is `OccurrenceWins`, and `NOTDEFINED` resolves None — falling back to the structural inference (a `Qto_*` quantity set, whose `QtoSetDef` carries no `templatetype`, and a type-bound property set are `TypeDrivenOverride`, an occurrence-only set `OccurrenceWins`) when no catalogue template type is declared, so the seam `Bake` applies the IFC inheritance once per bag rather than a per-call-site merge [H1].
- Receipt: the resolved `PropertyTemplate` map is the EXPECTED-type evidence the `Review/validation#IDS_FACETS` Property facet validates the seam `PropertyValue` against and the from-scratch authoring path constructs a typed value from; at IFC import the typing is the `Projection/semantic#SEMANTIC_PROJECTOR` `PropertyLowering.Lower` narrowing the live `IfcValue` runtime type onto the seam `PropertyValue` case directly (the catalogue/bSDD `DataType` is the expected type, never a `PropertyValue.Of(value, dataType)` the seam does not own); the stamped `InheritanceMode` is the precedence evidence the seam `Bake` reads when folding a `DefinesByType` edge into the occurrence.
- Packages: Xbim.Properties, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new standard Pset is already in the `Xbim.Properties` catalogue (no edit) or one live bSDD dictionary row; a new curated recognition anchor is one `PropertyKey` row; a new IFC value-type kind is one `BsddValueKind`-mapping arm; a new inheritance policy is the catalogue `templatetype` the dictionary declares; never a hand-coded `Pset_*` property table, never a per-Pset type, and never a second property store.
- Boundary: there is ONE property model and it is the seam graph — the retired `BimElement.PropertyBinding`/`QuantityBinding` stringly triples are GONE, a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, and a per-Pset `WallProperties`/`SlabProperties` class family is the deleted form; the typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned [H2] and re-declaring it here is the named drift defect — this page owns the TEMPLATE (`DataType`/value-type kind) and the PRECEDENCE policy, the seam owns the value; the offline standard template is the `Xbim.Properties` `Definitions<T>` catalogue read as the canonical floor and a hand-coded `Pset_*` property table beside it (the retired fabricated `IfcText`-per-Pset anchor that matched no real property) is the deleted form; the live bSDD dictionary unions OVER the catalogue with dictionary-wins, never the SOLE source and never a fault on a service miss; the type-vs-occurrence precedence is the IFC `IfcPropertySetTemplate.templatetype` the catalogue declares, lowered to the seam `InheritanceMode` at ingest [H1] and applied once in the seam `Bake`, never a per-call-site merge, never a stored-twice type→occurrence fold, and never a fragile set-name suffix heuristic; `Xbim.Properties` is a TEMPLATE source only (no IFC entity graph, no property values, no IDS engine) and consuming it as a model reader or value store is the rejected form; the `PropertyName` admits through the seam `PropertyName.Create` factory and a `new PropertyName(...)` over the value-object's private constructor is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Linq;
using LanguageExt;
using Rasm;
using Rasm.Element;
using Thinktecture;
using Xbim.Properties;
using static LanguageExt.Prelude;
using Version = Xbim.Properties.Version;       // the Xbim schema enum (IFC2x3/IFC4/IFC4x3); aliased off System.Version, which ImplicitUsings imports

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
// One resolved property template — the unified shape BOTH the offline Xbim.Properties PropertyDef and the live bSDD
// BsddProperty lower into, so a consumer reads ONE template type regardless of source. DataType is the IFC data-type
// token (IfcThermalTransmittanceMeasure/IfcLabel/...), Kind the value-type kind the seam PropertyValue arm is chosen
// from (BsddValueKind, Semantics/classification), Required the IDS requirement flag.
public readonly record struct PropertyTemplate(string Set, string Code, string DataType, BsddValueKind Kind, bool Required);

// The curated well-known Pset recognition anchors — the opinionated common set names authoring surfaces first and a
// TryGet recognizes, each carrying its IFC discipline. NOT the authoritative catalogue: the FULL buildingSMART template
// set is the offline Xbim.Properties PropertyCatalog (Resolve composes it), so this roster never fabricates a property,
// it names the common sets, the typed properties arriving from the catalogue and the live dictionary.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PropertyKey {
    public static readonly PropertyKey WallCommon = new("Pset_WallCommon", IfcDomain.Architecture);
    public static readonly PropertyKey SlabCommon = new("Pset_SlabCommon", IfcDomain.Architecture);
    public static readonly PropertyKey BeamCommon = new("Pset_BeamCommon", IfcDomain.Architecture);
    public static readonly PropertyKey ColumnCommon = new("Pset_ColumnCommon", IfcDomain.Architecture);
    public static readonly PropertyKey DoorCommon = new("Pset_DoorCommon", IfcDomain.Architecture);
    public static readonly PropertyKey WindowCommon = new("Pset_WindowCommon", IfcDomain.Architecture);
    public static readonly PropertyKey SpaceCommon = new("Pset_SpaceCommon", IfcDomain.Architecture);
    public static readonly PropertyKey ConcreteElementGeneral = new("Pset_ConcreteElementGeneral", IfcDomain.Structural);
    public static readonly PropertyKey MaterialSteel = new("Pset_MaterialSteel", IfcDomain.Structural);
    public static readonly PropertyKey MaterialMasonry = new("Pset_MaterialMasonry", IfcDomain.Structural);
    public static readonly PropertyKey ReinforcingBarBendingsCommon = new("Pset_ReinforcingBarBendingsCommon", IfcDomain.Structural);

    public IfcDomain Domain { get; }

    // The curated common Psets for a discipline — the opinionated recognition set authoring offers; the FULL applicable
    // template set is PropertyCatalog.Templates(cls, schema) over the Xbim.Properties ApplicableClasses.
    public static Seq<PropertyKey> TemplatesFor(IfcDomain domain) => Items.Filter(row => row.Domain == domain).ToSeq();

    // Per-class property templates: the bSDD live dictionary rows (dictionary-wins) unioned OVER the offline
    // Xbim.Properties standard catalogue floor (.api/api-xbim-properties + .api/api-bsdd), keyed {Set}.{Code}. The
    // catalogue is the always-available buildingSMART floor (deterministic, schema-versioned, network-free); bSDD
    // enriches/overrides when live; absent a dictionary the catalogue alone resolves — never a fabricated anchor.
    public static Map<string, PropertyTemplate> Resolve(IfcClass cls, ReleaseVersion schema, Option<BsddClass> dictionary) =>
        dictionary.Map(static d => d.Properties).IfNone(Seq<BsddProperty>())
            .Filter(static p => p.PropertySet.Length > 0)
            .Fold(PropertyCatalog.Templates(cls, schema),
                  static (template, p) => template.AddOrUpdate($"{p.PropertySet}.{p.Code}",
                      new PropertyTemplate(p.PropertySet, p.Code, p.DataType, p.ValueKind, p.IsRequired)));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The offline standard-Pset/Qto template catalogue: Xbim.Properties Definitions<T> loaded once per IFC schema and
// cached (the CDDL-1.0 binary referenced, never vendored). It declares what a Pset_*/Qto_* IS (its applicable classes,
// its properties' DataType/value-type kind, its base quantities' QtoTypeEnum), never an IFC entity graph and never a
// property value. The live bSDD dictionary (Semantics/classification#BSDD_RESOLUTION) unions OVER it with dictionary-wins.
public static class PropertyCatalog {
    static readonly ConcurrentDictionary<Version, (Definitions<PropertySetDef> Psets, Definitions<QtoSetDef> Qtos)> Catalogues = new();

    static (Definitions<PropertySetDef> Psets, Definitions<QtoSetDef> Qtos) For(ReleaseVersion schema) =>
        Catalogues.GetOrAdd(Lower(schema), static version => {
            Definitions<PropertySetDef> psets = new(version); psets.LoadAllDefault();
            Definitions<QtoSetDef> qtos = new(version); qtos.LoadAllDefault();
            return (psets, qtos);
        });

    // The seam ReleaseVersion (the model Header currency) -> the Xbim.Properties Version: templates exist for the three
    // published buildingSMART schemas, so a finer seam release folds onto its base schema (Ifc4X1->IFC4, the IFC4.3
    // variants/Ifc5->IFC4x3) rather than missing the catalogue.
    static Version Lower(ReleaseVersion schema) =>
        schema == ReleaseVersion.Ifc2X3                                  ? Version.IFC2x3
        : schema == ReleaseVersion.Ifc4 || schema == ReleaseVersion.Ifc4X1 ? Version.IFC4
        : Version.IFC4x3;

    // The standard property templates applicable to a class: every offline PropertySetDef whose ApplicableClasses names
    // the IFC entity, each PropertyDef lowered to the unified PropertyTemplate carrying its IFC DataType (the data-type
    // token) and the value-type kind the seam PropertyValue arm is chosen from, keyed {Set}.{Code}. PropertyDefinitions
    // is a nullable backing list (the safe Definitions getter guards it), so an empty/absent set folds to no rows.
    public static Map<string, PropertyTemplate> Templates(IfcClass cls, ReleaseVersion schema) =>
        For(schema).Psets.DefinitionSets
            .Where(set => Applies(set, cls))
            .SelectMany(set => (set.PropertyDefinitions ?? []).Select(p => TemplateOf(set.Name, p)))
            .Aggregate(Map<string, PropertyTemplate>(), static (template, p) => template.AddOrUpdate($"{p.Set}.{p.Code}", p));

    // One PropertyDef -> the unified PropertyTemplate: the IFC data-type token AND the bSDD ValueKind both derive from the
    // ONE PropertyType.PropertyValueType value-type kind (LowerValue), keyed {Set}.{Code}. PropertyDef.ValueDef is the
    // [Obsolete] legacy min/max/default slot and carries NO data type — the scalar IFC type lives on the value-type kind.
    static PropertyTemplate TemplateOf(string setName, PropertyDef p) =>
        LowerValue(p.PropertyType?.PropertyValueType) switch {
            var (dataType, kind) => new PropertyTemplate(setName, p.Name, dataType, kind, false),
        };

    // The IFC template type the offline PropertySetDef declares (the Xbim.Properties templatetype enum — PSET_TYPEDRIVEN-
    // OVERRIDE/...) — the authoritative source for the seam InheritanceMode [H1]; templates are an IFC4+ concept, so the
    // IFC4.3 catalogue carries them. NOTDEFINED (the enum default a set without a declared template type carries) and an
    // unknown set both resolve None so the projector's structural inference applies; QtoSetDef carries no templatetype, so
    // a Qto set name resolves None here and ModeOf's Qto_* structural branch decides it.
    internal static Option<templatetype> TemplateTypeOf(string setName) =>
        For(ReleaseVersion.Ifc4X3).Psets[setName] is { } set && set.templatetype is var t and not templatetype.NOTDEFINED
            ? Some(t) : None;

    // The class's base-quantity set name + the distinct geometry-relevant Dimensions its standard QtoDefs declare (mapped
    // from the QtoTypeEnum) — so QuantityDerivation derives the geometry-true takeoff for exactly the dimensions the
    // standard set carries, sourced from the catalogue rather than a hand-listed per-class table that slices it.
    public static Option<(string Set, Seq<Dimension> Dimensions)> BaseQuantitySet(IfcClass cls, ReleaseVersion schema) =>
        For(schema).Qtos.DefinitionSets.FirstOrDefault(set => Applies(set, cls)) is { } qto
            ? Some((qto.Name, (qto.QuantityDefinitions ?? []).Select(static q => DimensionOf(q.QuantityType)).Somes().Distinct().ToSeq()))
            : None;

    static bool Applies(QuantityPropertySetDef set, IfcClass cls) =>
        set.ApplicableClasses.Any(c => string.Equals(c.ClassName, cls.Key, StringComparison.OrdinalIgnoreCase));

    // ONE lowering of the Xbim.Properties value-type kind: the IFC data-type token (off the single/bounded/reference
    // DataType.Type DataTypeEnum, the simple-property DataType.Type) + the bSDD ValueKind axis the seam PropertyValue arm
    // is chosen from. The two former parallel switches over the same IPropertyValueType collapse here; the composite kinds
    // (enumerated/list/table/complex) carry no scalar data type, so the token is empty and the IDS facet reads the kind.
    static (string DataType, BsddValueKind Kind) LowerValue(IPropertyValueType? valueType) => valueType switch {
        TypePropertySingleValue single   => (single.DataType?.Type?.ToString() ?? "", BsddValueKind.Single),
        TypePropertyBoundedValue bounded => (bounded.DataType?.Type?.ToString() ?? "", BsddValueKind.Range),
        TypePropertyReferenceValue refer => (refer.DataType?.Type?.ToString() ?? "", BsddValueKind.Single),
        TypeSimpleProperty simple        => (simple.DataType?.Type ?? "", BsddValueKind.Single),
        TypePropertyEnumeratedValue      => ("", BsddValueKind.List),
        TypePropertyListValue            => ("", BsddValueKind.List),
        TypePropertyTableValue           => ("", BsddValueKind.ComplexList),
        TypeComplexProperty              => ("", BsddValueKind.Complex),
        _                                => ("", BsddValueKind.Single),
    };

    // QtoTypeEnum -> the seam Dimension; Count/Time are not geometry-derivable, so they yield None and the derivation skips.
    static Option<Dimension> DimensionOf(QtoTypeEnum kind) => kind switch {
        QtoTypeEnum.Q_LENGTH => Dimension.LengthDim,
        QtoTypeEnum.Q_AREA   => Dimension.AreaDim,
        QtoTypeEnum.Q_VOLUME => Dimension.VolumeDim,
        QtoTypeEnum.Q_WEIGHT => Dimension.MassDim,
        _                    => Option<Dimension>.None,
    };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The InheritanceMode classifier [H1]: the projector stamps each seam PropertySet/QuantitySet node with its precedence
// policy at ingest so the seam Bake applies type->occurrence precedence wholly within the seam. The authoritative source
// is the catalogue's IFC templatetype; the structural inference is the fallback when no template type is declared.
public static class PropertyInheritance {
    // The Xbim.Properties templatetype enum -> the seam InheritanceMode: PSET_/QTO_TYPEDRIVENONLY take the type bag only,
    // PSET_/QTO_TYPEDRIVENOVERRIDE let the type bag override; every other declared kind (occurrence/performance/profile/
    // material-driven) is occurrence-wins. NOTDEFINED never reaches here — TemplateTypeOf maps it to None.
    static InheritanceMode FromTemplate(templatetype t) => t switch {
        templatetype.PSET_TYPEDRIVENONLY or templatetype.QTO_TYPEDRIVENONLY         => InheritanceMode.TypeDrivenOnly,
        templatetype.PSET_TYPEDRIVENOVERRIDE or templatetype.QTO_TYPEDRIVENOVERRIDE => InheritanceMode.TypeDrivenOverride,
        _                                                                           => InheritanceMode.OccurrenceWins,
    };

    public static InheritanceMode ModeOf(string setName, bool typeBound) =>
        PropertyCatalog.TemplateTypeOf(setName).Match(
            Some: FromTemplate,
            None: () => setName.StartsWith("Qto_", StringComparison.Ordinal) || typeBound
                ? InheritanceMode.TypeDrivenOverride
                : InheritanceMode.OccurrenceWins);
}
```

## [03]-[BASE_QUANTITIES]

- Owner: `QuantityDerivation` the base-quantity fold deriving the standard `Qto_*BaseQuantities` from `GeometryMeasures` — the kernel `Rasm` value-object `GeometryMeasures(Option<double> Length, Option<double> Area, Option<double> Volume)` the kernel/Compute resolve from the geometry the seam `Object` node references by content key (`Model/elements#REPRESENTATION_KEYS` `RepresentationContentHash`) and supply to `Derive` (Bim consumes the measure, never tessellates it) — producing the seam `QuantitySet` node values as seam `MeasureValue` under derived-wins precedence. The class's base-quantity SET and the dimensions it derives come from `PropertyCatalog.BaseQuantitySet` (the offline `Xbim.Properties` `QtoSetDef` catalogue), so the roster covers every class the standard defines, not a hand-listed slice.
- Entry: `QuantityDerivation.Derive(IfcClass cls, ReleaseVersion schema, GeometryMeasures measures, Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence)` derives the geometry-true base quantities for a class and merges them over the occurrence-stored quantities under derived-wins precedence (the geometry takeoff supersedes an authoring tool's stored quantity), returning the seam `QuantitySet` node value map; a class with no `Qto_*BaseQuantities` set in the catalogue returns the occurrence quantities unchanged so a non-takeoff class never blocks.
- Auto: `Derive` reads `PropertyCatalog.BaseQuantitySet(cls, schema)` (the `Qto_*` set name + the distinct geometry-relevant `Dimension`s its standard `QtoDef`s declare) and folds each dimension through the ONE `Derivations` frozen table — keyed by `Dimension`, each row the canonical base-quantity suffix plus the projector from the kernel `GeometryMeasures` (and the material mass density for the weight derivation) — collapsing the former parallel measure-vs-name switches; the kernel scalar is already SI-base, so each derived value wraps through the seam `MeasureValue.OfSi` (its canonical `Dimension.SiSymbol` unit), keyed `{SetName}.{suffix}`, merged over the occurrence map with derived-wins so the 5D `Planning/cost#ESTIMATE` join reads the geometry-true measure (`Volume ≻ Area ≻ Length ≻ Mass`); `NetWeight = NetVolume × massDensity` (`VolumeDim × DensityDim IS MassDim`, so the SI product is the SI mass), absent a density skipping the weight rows; an element-set aggregate of the same `Dimension` reduces through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum` reducer, never a manual `double` fold.
- Packages: Xbim.Properties, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new class's base-quantity set is already in the `Xbim.Properties` `QtoSetDef` catalogue (no edit); a new geometry-derivable dimension is one `Derivations` row carrying its canonical suffix and projector; the derived quantities merge over the occurrence map under one precedence rule; never a per-class `Derive` method, never a hand-listed per-class set table, and never a re-tessellation in this owner.
- Boundary: base-quantity derivation runs from the kernel `GeometryMeasures` value-object (`Option<double>` `Length`/`Area`/`Volume`) the kernel/Compute resolve from the `RepresentationContentHash` geometry and inject into `Derive`, so a Bim-local `GeometryMeasures` re-declaration or an in-owner geometry-measure computation is the deleted form (Bim depends UP on the kernel and never owns geometry measurement); a re-tessellation in this owner is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the derived value is a seam `MeasureValue` wrapped through `MeasureValue.OfSi` (the seam owns the typed quantity over `Dimension` + UnitsNet [H2]), so a Bim-local `MeasureValue` re-declaration or a hand-stamped unit string that drifts from the seam canonical `Dimension.SiSymbol` is the deleted form; `NetWeight` is the homogeneous-element takeoff (`NetVolume × Mechanical.Density`, the material's `Composition/material#MATERIAL_PROPERTY` `Mechanical.Density` resolved One-Hop from the `Associate` material edge) and the multi-ply/layered weight is the `Rasm.Compute` `AssemblyAggregator`'s richer fold, never re-modeled here; the base-quantity SET and its dimensions come from the `Xbim.Properties` `QtoSetDef` catalogue and a hand-listed per-class `BaseQuantityTable` that slices the standard is the deleted form; the `Measure`/`NameOf` parallel switches collapse into the ONE `Dimension`-keyed `Derivations` frozen table; the derived-wins precedence is applied once in `Derive`, never a per-call-site merge.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class QuantityDerivation {
    // ONE Dimension-keyed derivation table — the former Measure + NameOf parallel switches collapse here: each row the
    // canonical geometry-takeoff suffix + the projector from the kernel measures (+ the material mass density for the
    // weight derivation). The kernel GeometryMeasures scalars are already SI-base, so each derived value wraps through
    // MeasureValue.OfSi (its canonical Dimension.SiSymbol unit), never re-coerced through UnitsNet and never a hand-stamped
    // unit string. NetWeight = NetVolume x density: VolumeDim(3,0,..) x DensityDim(-3,1,..) IS MassDim, the SI product the SI mass.
    static readonly FrozenDictionary<Dimension, (string Suffix, Func<GeometryMeasures, Option<MeasureValue>, Option<double>> Project)> Derivations =
        new Dictionary<Dimension, (string, Func<GeometryMeasures, Option<MeasureValue>, Option<double>>)> {
            [Dimension.LengthDim] = ("Length",    static (m, _)       => m.Length),
            [Dimension.AreaDim]   = ("GrossArea", static (m, _)       => m.Area),
            [Dimension.VolumeDim] = ("NetVolume", static (m, _)       => m.Volume),
            [Dimension.MassDim]   = ("NetWeight", static (m, density) => m.Volume.Bind(v => density.Filter(static d => d.Dimension == Dimension.DensityDim).Map(d => v * d.Si))),
        }.ToFrozenDictionary();

    // Geometry-true base quantities (derived-wins) merged over the occurrence quantities. PropertyCatalog.BaseQuantitySet
    // (the offline Xbim.Properties Qto catalogue) supplies the class's Qto set name and the geometry-relevant dimensions
    // its standard QtoDefs declare; each derivable dimension folds to a seam MeasureValue under derived-wins so the
    // geometry takeoff supersedes a stored quantity. GeometryMeasures is the kernel Rasm value-object the kernel/Compute
    // resolve from the Object RepresentationContentHash geometry and inject (Bim never tessellates); massDensity is the
    // element material's Mechanical.Density (Composition/material#MATERIAL_PROPERTY) resolved One-Hop from the Associate
    // material edge, absent which the weight rows skip rather than fabricate.
    public static Map<PropertyName, MeasureValue> Derive(
        IfcClass cls, ReleaseVersion schema, GeometryMeasures measures, Option<MeasureValue> massDensity,
        Map<PropertyName, MeasureValue> occurrence) =>
        PropertyCatalog.BaseQuantitySet(cls, schema).Match(
            None: () => occurrence,
            Some: set => set.Dimensions.Fold(occurrence, (acc, dimension) =>
                Derivations.TryGetValue(dimension, out var d)
                    ? d.Project(measures, massDensity).Match(
                        Some: si => acc.AddOrUpdate(PropertyName.Create($"{set.Set}.{d.Suffix}"), MeasureValue.OfSi(dimension, si)),
                        None: () => acc)
                    : acc));
}
```

## [04]-[RESEARCH]

- [TEMPLATE_RESOLUTION]: the `PropertyKey.Resolve` catalogue-floor ∪ bSDD-live union grounds against `.api/api-xbim-properties` (the `Definitions<PropertySetDef>` offline catalogue — `DefinitionSets`, `PropertySetDef.ApplicableClasses`/`PropertyDefinitions`, `PropertyDef.PropertyType.PropertyValueType` lowered through `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyReferenceValue` `DataType.Type` (`DataTypeEnum`) + `TypeSimpleProperty.DataType.Type` (`PropertyDef.ValueDef` is `[Obsolete]`, carrying no data type), and the `templatetype` enum) and `.api/api-bsdd` (`ClassPropertyContract.v1` `propertyCode`/`dataType`/`propertySet`/`isRequired`/`propertyValueKind`) — `Xbim.Properties` is the always-available canonical floor (deterministic, schema-versioned, network-free) and bSDD the live dictionary that enriches/overrides it with dictionary-wins, so a property carries its declared IFC `DataType` whether or not the service is reachable; the typing flow is the `Projection/semantic#SEMANTIC_PROJECTOR` `PropertyLowering.Lower` narrowing the live `IfcValue` runtime type at import (the template `DataType` is the EXPECTED type the IDS facet validates against and the authoring path constructs from, never a `PropertyValue.Of(value, dataType)` the seam does not own). The fabricated `IfcText`-per-Pset anchor that matched no real property is the deleted form; the hand-coded `Pset_*` table is the rejected form per `.api/api-xbim-properties` (the package owns the template dataset).
- [INHERITANCE_STAMP]: the `PropertyInheritance.ModeOf` `InheritanceMode` classifier grounds against `ELEMENT-REBUILD-PLAN.md` §4-RT H1 (stamp each PropertySet/QuantitySet node with a structural `InheritanceMode` — `OccurrenceWins`/`TypeDrivenOverride`/`TypeDrivenOnly` — at Bim ingest so `Bake` applies correct type→occurrence precedence wholly within the seam) and the IFC `IfcPropertySetTemplate.TemplateType` vocabulary (`PSET_TYPEDRIVENOVERRIDE`/`PSET_TYPEDRIVENONLY`/`PSET_OCCURRENCEDRIVEN`/…) the `Xbim.Properties` `PropertySetDef.templatetype` declares — the authoritative source the catalogue carries, with the structural inference (a `Qto_*` quantity set and a type-bound property set are type-driven-override) the fallback when no template type is declared; the projector reads `ModeOf` and stamps the seam node, the seam `Bake` reading the stamp when folding a `DefinesByType` edge so the precedence is never re-derived per call site and never a fragile `*TypeCommon` set-name suffix heuristic.
- [BASE_QUANTITY_DERIVE]: the `QuantityDerivation.Derive` base-quantity fold grounds against the buildingSMART `Qto_*BaseQuantities` definitions the `Xbim.Properties` `QtoSetDef`/`QtoDef.QuantityType` (`Q_LENGTH`/`Q_AREA`/`Q_VOLUME`/`Q_WEIGHT`/…) catalogue declares per class and the kernel `Rasm` `GeometryMeasures` the seam node references by content key, deriving the geometry-true takeoff (incl. `NetWeight = NetVolume × Mechanical.Density`, the `Composition/material#MATERIAL_PROPERTY` density) rather than re-tessellating, the `Planning/cost#ESTIMATE` join reading by the `Dimension` rank; the value-half retirement grounds against `ELEMENT-REBUILD-PLAN.md` §4B (the typed VALUE family `PropertyValue`/`MeasureValue` is seam-owned, library-neutral) and §4-RT H2 (the 6-value `QuantityKind` replaced by the seam `Dimension` `[ComplexValueObject]` over the 7 SI base dimensions PLUS the `QuantityType` `[ValueObject<string>]` name discriminator, since the exponent vector is not injective over quantity types), so this page owns the TEMPLATE and the PRECEDENCE, the seam owns the value, and the `Measure`/`NameOf` parallel switches collapse into the one `Dimension`-keyed `Derivations` table wrapped through `MeasureValue.OfSi`.
