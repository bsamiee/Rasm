# [BIM_PROPERTY_TEMPLATES]

The IFC Pset/Qto TEMPLATE authority over the `Rasm.Element` seam graph: the offline `Xbim.Properties` `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` buildingSMART catalogue is the canonical, schema-versioned, network-free template floor — every standard `Pset_*` and `Qto_*`, its `ApplicableClasses` (entity AND `PredefinedType` scope), each property's IFC `DataType`/value-type kind PLUS its full value constraint (the enumerated kind's `EnumList`/`ConstantList` allowed values, the bounded kind's `ValueRangeDef` range, the declared `UnitType`, the localized `NameAliases`), and each base quantity's `QtoTypeEnum` + `MethodOfMeasurement` — and the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass` dictionary enriches it (dictionary-wins on a name collision), its rows carrying the class-scoped `allowedValues`/`pattern`/bounds/`units`/seven-int SI exponent constraint surface. `PropertyKey.Resolve` unions the two into one FULL-CONSTRAINT `PropertyTemplate` map scoped by the node's `PredefinedType` token, so the `Review/validation#IDS_FACETS` Property facet validates VALUE constraints, never type alone; `PropertyInheritance.ModeOf` reads the authoritative IFC `templatetype` the catalogue declares to stamp each seam `PropertySet`/`QuantitySet` node with its `InheritanceMode` at ingest so the seam `Bake` applies the correct type→occurrence precedence over the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge a `Component` Type `Object` binds — the `typeBound` bag the classifier reads is the set the projector resolved from an `IfcTypeProduct`/`IfcElementType` (the seam `ObjectKind.Type` node the `Rasm.Materials` `ComponentProjector` mints), so a wall-type's shared `Pset_WallCommon` rides the type bag and the occurrence's overriding values the occurrence bag, the two merged once in `Bake`; `QuantityDerivation.Derive` sources the class's base-quantity set from the catalogue and folds the geometry-true takeoff from the kernel geometry the node references by content key. The typed VALUE half is seam-owned: the closed `PropertyValue` family (`Text`/`Measure`/`Boolean`/`Logical`/`Integer`/`Number`/`Binary`/`Temporal`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex` — `Logical` the three-valued `IfcLogical`/`IfcLogicalEnum` `UNKNOWN`, `Complex` the named nested `IfcComplexProperty` bag) and the `MeasureValue` (`QuantityType`/`Dimension`/`Si`/`CanonicalUnit` over the `Dimension` `[ComplexValueObject]` + the `QuantityType` `[ValueObject<string>]` discriminator) live on `Rasm.Element/Properties` — this page owns the TEMPLATE (which properties a class carries and their declared types) and the PRECEDENCE policy, never the value. A property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, never a `(SetName, Name, string Value)` triple. The page is the template oracle the `Projection/semantic#SEMANTIC_PROJECTOR` projector and the `Review/validation#IDS_FACETS` Property facet read, and `TemplateAudit.Run` turns the same resolved templates into the zero-configuration graph-wide model-QA fold — presence, declared type, allowed values, bounds, pattern, and dimension audited per element with no authored IDS, this page's own spec-free model-health surface beside the `Review/validation#IDS_FACETS` authored-IDS lane; a hand-coded `Pset_*` property table beside the `Xbim.Properties` catalogue is the deleted form.

## [01]-[INDEX]

- [01]-[PROPERTY_TEMPLATES]: `PropertyCatalog` the `Xbim.Properties` offline standard-template catalogue (loaded once per IFC schema), `PropertyTemplate` the unified resolved full-constraint template both sources lower into, `PropertyKey` the curated well-known `Pset_*` recognition anchors, `PropertyKey.Resolve` the predefined-scoped catalogue-floor ∪ bSDD-live template union, and `PropertyInheritance.ModeOf` the `templatetype`-driven `InheritanceMode` classifier stamped on each seam bag node at ingest.
- [02]-[BASE_QUANTITIES]: `PropertyCatalog.BaseQuantitySet` the per-`IfcClass` `Qto_*BaseQuantities` set + its `MethodOfMeasurement` basis + each geometry-relevant `QtoDef`'s declared name + `Dimension` (from the catalogue, never a hand-listed slice), `QuantityDerivation.Derive` the base-quantity fold deriving the geometry-true takeoff (incl. `NetWeight` from volume × material density) from the kernel geometry measures the seam node references by content key, keyed by declared set members only, producing the seam `QuantitySet` node values under derived-wins precedence, and `QuantityDerivation.Decompose` the material-true takeoff — the element volume split per `MaterialId` over the seam `MaterialComposition` (layer thickness share, constituent `Fraction`, baked section-area × length), the per-material join key every 5D/6D consumer reads.
- [03]-[TEMPLATE_AUDIT]: `TemplateAudit.Run` the zero-configuration model-QA fold auditing a whole seam `ElementGraph` against the resolved buildingSMART templates themselves — per-`(class, predefined)` template resolution through `PropertyKey.Resolve`, per-element presence/kind/allowed-value/bounds/pattern/dimension verdicts as typed `TemplateFinding` rows over the `TemplateVerdict` vocabulary — this page's own spec-free model-health surface; the authored-requirement lane stays `Review/validation#IDS_FACETS`.

## [02]-[PROPERTY_TEMPLATES]

- Owner: `PropertyCatalog` the offline `Xbim.Properties` template catalogue — `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` loaded once per IFC `Version` and cached, the always-available buildingSMART template floor declaring what every `Pset_*`/`Qto_*` IS (its `ApplicableClasses` with entity + `PredefinedType` scope, its `PropertyDef`s with their `DataType`/value-type kind + allowed values + range + unit + aliases, its `QtoDef`s with their `QtoTypeEnum` under the set's `MethodOfMeasurement`); `PropertyTemplate` the unified resolved FULL-CONSTRAINT template (`Set`/`Code`/`DataType`/`Kind`/`Required` + `AllowedValues`/`Bounds`/`Pattern`/`Units`/`Exponents`/`Predefined`/`Aliases`, with `ToDimension()` seeding the seam `Dimension` off the bSDD exponents through the generated `Dimension.Create`) both the catalogue `PropertyDef` and the bSDD `BsddProperty` lower into; `PropertyKey` the curated well-known `Pset_*` recognition anchors (the opinionated common set name + its `IfcDomain`) authoring surfaces first; `PropertyInheritance` the classifier reading the catalogue's authoritative IFC `templatetype` onto the seam `InheritanceMode`. The typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned (`Rasm.Element/Properties`); this page supplies the TEMPLATE (which properties, their declared `DataType`) the seam value is constructed against, never the value.
- Entry: `PropertyKey.Resolve(IfcClass cls, Option<string> predefined, ReleaseVersion schema, Option<BsddClass> dictionary)` resolves a class's property templates — `PropertyCatalog.Templates(cls, predefined, schema)` (the offline `Xbim.Properties` floor, every `PropertySetDef` whose `ApplicableClasses` names the class AND whose `PredefinedType` scope, when declared, matches the node's token — a `ClassName`-only match over-applies a predefined-scoped Pset to its whole class) unioned UNDER the live `BsddClass.Properties` dictionary rows (dictionary-wins on a `{Set}.{Code}` collision), so a measured property carries its declared IFC `DataType` AND its value constraint, and the offline catalogue resolves when bSDD is unreachable; `PropertyCatalog.TemplateTypeOf(string setName)` is the `internal` catalogue query returning the raw IFC `templatetype` a set declares (the `Xbim.Properties` enum kept Bim-internal, never a public seam return); `PropertyInheritance.ModeOf(string setName, bool typeBound)` is the public canonical surface returning the seam `InheritanceMode` the projector stamps on a bag at ingest; `Fin<T>` is not the rail here — resolution degrades to the offline catalogue (and to the structural inference) when the dictionary is unreachable, never faulting ingest.
- Auto: `Resolve` folds the bSDD `BsddProperty` rows (lowered to the full-constraint `PropertyTemplate` — `DataType`/`ValueKind`/`IsRequired` plus `AllowedValues` value strings, `Bounds`, `Pattern`, `Units`, the `SiExponents`, and the class-fixed `PredefinedValue`) OVER the `PropertyCatalog.Templates` floor with the two-arm `AddOrUpdate` so a dictionary-declared property overrides the offline default and a bSDD-only property still resolves — dictionary-wins-when-SPEAKING: a declared narrowing (`AllowedValues`/`Bounds`/`Pattern`/`Units`) overrides its floor axis, a SILENT axis keeps the floor's constraint, and the localized `Aliases` only the catalogue carries always survive, so a terse dictionary row never erases a floor constraint it merely failed to restate; `PropertyCatalog.Templates` reads `definitions.DefinitionSets`, keeps the sets whose `ApplicableClasses` match entity + `PredefinedType` scope, and lowers each `PropertyDef` through its `PropertyType.PropertyValueType` value-type kind in ONE pass (`LowerValue`): the scalar IFC data-type token off `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyReferenceValue` `DataType.Type` (a `DataTypeEnum`) and `TypeSimpleProperty.DataType.Type`, the same kind selecting the `BsddValueKind`, the enumerated kind yielding its `EnumList.Items`+`ConstantList` allowed values, the bounded kind its inclusive `ValueRangeDef` range, the single/bounded/simple/list kinds their declared `UnitType` token (`PropertyDef.ValueDef` is the `[Obsolete]` legacy min/max/default slot read ONLY as the fallback constraint carrier under a scoped pragma — never a data type; the composite kinds carry no scalar token), and `NameAliases` folding onto the per-language display map — so the projector and the IDS facet know each property's expected type AND legal values without re-deriving either; `ModeOf` reads `PropertyCatalog.TemplateTypeOf` (the `templatetype` enum the `PropertySetDef` declares) — `PSET_TYPEDRIVENONLY`/`QTO_TYPEDRIVENONLY` is `TypeDrivenOnly`, `PSET_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENOVERRIDE` is `TypeDrivenOverride`, every other declared kind (`PSET_OCCURRENCEDRIVEN`/`PSET_PERFORMANCEDRIVEN`/`PSET_PROFILEDRIVEN`/`PSET_MATERIALDRIVEN`) is `OccurrenceWins`, and `NOTDEFINED` resolves None — falling back to the structural inference (a `Qto_*` quantity set, whose `QtoSetDef` carries no `templatetype`, and a type-bound property set are `TypeDrivenOverride`, an occurrence-only set `OccurrenceWins`) when no catalogue template type is declared, so the seam `Bake` applies the IFC inheritance once per bag rather than a per-call-site merge.
- Receipt: the resolved `PropertyTemplate` map is the EXPECTED-type AND VALUE-CONSTRAINT evidence the `Review/validation#IDS_FACETS` Property facet validates the seam `PropertyValue` against (`AllowedValues`/`Bounds`/`Pattern` narrow into the facet's `ValueConstraint`; `ToDimension()` corroborates a measured value's `Dimension`) and the from-scratch authoring path constructs a typed value from; at IFC import the typing is the `Projection/semantic#SEMANTIC_PROJECTOR` `PropertyLowering.Lower` narrowing the live `IfcValue` runtime type onto the seam `PropertyValue` case directly (the catalogue/bSDD `DataType` is the expected type, never a `PropertyValue.Of(value, dataType)` the seam does not own); the stamped `InheritanceMode` is the precedence evidence the seam `Bake` reads when folding the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge (the neutral seam lowering of the IFC `IfcRelDefinesByType` the projector authored) into the occurrence — the `Component` Type bag's values merging into the occurrence by the stamped mode.
- Packages: Xbim.Properties, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new standard Pset is already in the `Xbim.Properties` catalogue (no edit) or one live bSDD dictionary row; a new curated recognition anchor is one `PropertyKey` row; a new IFC value-type kind is one `BsddValueKind`-mapping arm; a new constraint axis is one `PropertyTemplate` column BOTH lowerings fill; a new inheritance policy is the catalogue `templatetype` the dictionary declares; never a hand-coded `Pset_*` property table, never a per-Pset type, and never a second property store.
- Boundary: there is ONE property model and it is the seam graph — a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, and a per-Pset `WallProperties`/`SlabProperties` class family is the deleted form; the typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned and re-declaring it here is the deleted form — this page owns the TEMPLATE (`DataType`/value-type kind) and the PRECEDENCE policy, the seam owns the value; the offline standard template is the `Xbim.Properties` `Definitions<T>` catalogue read as the canonical floor and a hand-coded `Pset_*` property table beside it is the deleted form; applicability matches BOTH `ApplicableClass.ClassName` and its `PredefinedType` scope; the constraint surface lowers from the value-type kinds themselves (`TypePropertyEnumeratedValue.EnumList`/`ConstantList`, `TypePropertyBoundedValue.ValueRangeDef`, the `UnitType` axis) and the `[Obsolete]` `ValueDef` min/max/default slot is the guarded FALLBACK read only (one pragma-scoped accessor — constraint source, never a data type); the live bSDD dictionary unions OVER the catalogue with dictionary-wins, never the SOLE source and never a fault on a service miss; the type-vs-occurrence precedence is the IFC `IfcPropertySetTemplate.templatetype` the catalogue declares, lowered to the seam `InheritanceMode` at ingest and applied once in the seam `Bake`, never a per-call-site merge, never a stored-twice type→occurrence fold, and never a fragile set-name suffix heuristic; `Xbim.Properties` is a TEMPLATE source only (no IFC entity graph, no property values, no IDS engine) and consuming it as a model reader or value store is the rejected form; the `PropertyName` admits through the seam `PropertyName.Create` factory and a `new PropertyName(...)` over the value-object's private constructor is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LanguageExt;
using Rasm;
using Rasm.Bim;
using Rasm.Element.Classification;
using Rasm.Element.Composition;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Thinktecture;
using Xbim.Properties;
using static LanguageExt.Prelude;
using Op = Rasm.Domain.Op;                     // the kernel operation key the audit Bake rail threads
using Version = Xbim.Properties.Version;       // the Xbim schema enum (IFC2x3/IFC4/IFC4x3); aliased off System.Version, which ImplicitUsings imports

namespace Rasm.Bim.Semantics;

// --- [MODELS] -----------------------------------------------------------------------------
// One resolved property template — the unified FULL-CONSTRAINT shape BOTH the offline Xbim.Properties PropertyDef
// and the live bSDD BsddProperty lower into, so a consumer reads ONE template type regardless of source. DataType is
// the IFC data-type token (IfcThermalTransmittanceMeasure/IfcLabel/...), Kind the value-type kind the seam
// PropertyValue arm is chosen from (BsddValueKind, Semantics/classification), Required the IDS requirement flag;
// AllowedValues/Bounds/Pattern are the VALUE constraint the IDS Property facet narrows into a ValueConstraint (type
// alone was the retired thin slice), Units the declared unit vocabulary the seam MeasureValue coercion corroborates,
// Exponents the bSDD seven-int SI vector, Predefined the class-fixed value, Aliases the per-language display names
// an authoring surface renders. Bounds/Exponents reuse the classification-owned BsddBounds/SiExponents carriers —
// one concept, one type, both sources lowering into it.
public readonly record struct PropertyTemplate(
    string Set, string Code, string DataType, BsddValueKind Kind, bool Required,
    Seq<string> AllowedValues, Option<BsddBounds> Bounds, Option<string> Pattern,
    Seq<string> Units, Option<SiExponents> Exponents, Option<string> Predefined,
    Map<string, string> Aliases) {

    // The seam Dimension seeded directly off the bSDD exponents (Dimension.Create is the generated seven-int
    // factory) — a dimensioned template hands MeasureValue.OfSi its Dimension with no re-derivation.
    public Option<Dimension> ToDimension() =>
        Exponents.Map(static e => Dimension.Create(e.Length, e.Mass, e.Time, e.Current, e.Temperature, e.Amount, e.LuminousIntensity));
}

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

    // The key-chaining ctor the [SmartEnum<string>] generator's this(key) overload completes (the corpus
    // SmartEnum-with-fields shape).
    private PropertyKey(string key, IfcDomain domain) : this(key) => Domain = domain;

    // The curated common Psets for a discipline — the opinionated recognition set authoring offers; the FULL applicable
    // template set is PropertyCatalog.Templates(cls, schema) over the Xbim.Properties ApplicableClasses.
    public static Seq<PropertyKey> TemplatesFor(IfcDomain domain) => Items.ToSeq().Filter(row => row.Domain == domain);

    // Per-class property templates: the bSDD live dictionary rows (dictionary-wins) unioned OVER the offline
    // Xbim.Properties standard catalogue floor (.api/api-xbim-properties + .api/api-bsdd), keyed {Set}.{Code}. The
    // catalogue is the always-available buildingSMART floor (deterministic, schema-versioned, network-free); bSDD
    // enriches/overrides when live; absent a dictionary the catalogue alone resolves — never a fabricated anchor.
    // predefined is the node's PredefinedType token: an ApplicableClass.PredefinedType-scoped set resolves ONLY for
    // a matching token, so a predefined-scoped Pset never over-matches its whole class. The bSDD row lowers its FULL
    // class-scoped constraint (allowed values, bounds, pattern, units, SI exponents, predefined value) — the
    // class-level narrowing wins over the property master by contract.
    public static Map<string, PropertyTemplate> Resolve(IfcClass cls, Option<string> predefined, ReleaseVersion schema, Option<BsddClass> dictionary) =>
        dictionary.Map(static d => d.Properties).IfNone(Seq<BsddProperty>())
            .Filter(static p => p.PropertySet.Length > 0)
            .Fold(PropertyCatalog.Templates(cls, predefined, schema),
                  static (template, p) => template.AddOrUpdate($"{p.PropertySet}.{p.Code}",
                      Some: existing => Lower(p, Some(existing)),
                      None: () => Lower(p, None)));

    // The bSDD row lowering under dictionary-wins-when-SPEAKING: a dictionary row overrides ONLY the axes it states — a
    // declared narrowing (allowed values, bounds, pattern, units) wins, a SILENT axis keeps the catalogue floor's, and
    // the localized Aliases only the offline catalogue carries always survive — so a terse dictionary row never ERASES
    // a floor constraint it merely failed to restate (the silent-axis wipe was the masked-constraint-loss form).
    static PropertyTemplate Lower(BsddProperty p, Option<PropertyTemplate> floor) =>
        new(p.PropertySet, p.Code, p.DataType, p.ValueKind, p.IsRequired,
            p.AllowedValues.IsEmpty ? floor.Map(static f => f.AllowedValues).IfNone(Seq<string>()) : p.AllowedValues.Map(static v => v.Value),
            p.Bounds.IsSome ? p.Bounds : floor.Bind(static f => f.Bounds),
            p.Pattern.IsSome ? p.Pattern : floor.Bind(static f => f.Pattern),
            p.Units.IsEmpty ? floor.Map(static f => f.Units).IfNone(Seq<string>()) : p.Units,
            p.Exponents,
            Optional(p.PredefinedValue).Filter(static s => s.Length > 0),
            floor.Map(static f => f.Aliases).IfNone(Map<string, string>()));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The offline standard-Pset/Qto template catalogue: Xbim.Properties Definitions<T> loaded once per IFC schema and
// cached (the CDDL-1.0 binary referenced, never vendored). It declares what a Pset_*/Qto_* IS (its applicable classes
// with PredefinedType scope, its properties' DataType/value-type kind + constraint + unit + aliases, its base
// quantities' QtoTypeEnum + MethodOfMeasurement), never an IFC entity graph and never a property value. The live bSDD dictionary (Semantics/classification#BSDD_RESOLUTION) unions OVER it with dictionary-wins.
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

    // The standard property templates applicable to a class: every offline PropertySetDef whose ApplicableClasses
    // names the IFC entity AND whose PredefinedType scope (when declared) matches the node's token, each PropertyDef
    // lowered to the unified full-constraint PropertyTemplate in ONE pass, keyed {Set}.{Code}. PropertyDefinitions
    // is a nullable backing list (the safe Definitions getter guards it), so an empty/absent set folds to no rows.
    public static Map<string, PropertyTemplate> Templates(IfcClass cls, Option<string> predefined, ReleaseVersion schema) {
        var catalogues = For(schema);
        IEnumerable<PropertyTemplate> properties = catalogues.Psets.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .SelectMany(set => (set.PropertyDefinitions ?? []).Select(p => TemplateOf(set.Name, p)));
        IEnumerable<PropertyTemplate> quantities = catalogues.Qtos.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .SelectMany(set => (set.QuantityDefinitions ?? []).Select(q => QuantityTemplate(set.Name, q)).Somes());
        return properties.Concat(quantities)
            .Aggregate(Map<string, PropertyTemplate>(), static (template, p) => template.AddOrUpdate($"{p.Set}.{p.Code}", p));
    }

    // One PropertyDef -> the unified template: the ONE PropertyType.PropertyValueType value-type kind drives the
    // data-type token, the kind, the allowed-value enumeration, the numeric range, and the declared unit in a single
    // lowering; the [Obsolete] ValueDef min/max/default slot is the legacy FALLBACK constraint carrier only (never a
    // data-type source); NameAliases fold onto the per-language display map. The SI exponent vector stays
    // dictionary-sourced — the catalogue carries no dimension vector.
    static PropertyTemplate TemplateOf(string setName, PropertyDef p) {
        var (dataType, kind, allowed, bounds, units) = LowerValue(p.PropertyType?.PropertyValueType);
        var (legacyBounds, legacyDefault) = Legacy(p);
        return new PropertyTemplate(
            setName, p.Name, dataType, kind, false,
            allowed, bounds.IsSome ? bounds : legacyBounds, None,
            units, None, legacyDefault,
            (p.NameAliases ?? []).Aggregate(Map<string, string>(), static (acc, alias) => acc.AddOrUpdate(alias.Lang ?? "", alias.Value ?? "")));
    }

    static Option<PropertyTemplate> QuantityTemplate(string setName, QtoDef quantity) =>
        DimensionOf(quantity.QuantityType).Map(dimension => new PropertyTemplate(
            setName, quantity.Name ?? "", QuantityDataType(quantity.QuantityType), BsddValueKind.Single, false,
            Seq<string>(), None, None, Seq<string>(),
            Some(new SiExponents(dimension.Length, dimension.Mass, dimension.Time, dimension.Current,
                dimension.Temperature, dimension.Amount, dimension.LuminousIntensity)),
            None, Map<string, string>()));

    static string QuantityDataType(QtoTypeEnum quantity) => quantity switch {
        QtoTypeEnum.Q_LENGTH => "IfcLengthMeasure",
        QtoTypeEnum.Q_AREA   => "IfcAreaMeasure",
        QtoTypeEnum.Q_VOLUME => "IfcVolumeMeasure",
        QtoTypeEnum.Q_WEIGHT => "IfcMassMeasure",
        QtoTypeEnum.Q_TIME   => "IfcTimeMeasure",
        _                    => "IfcCountMeasure",
    };

    // The IFC template type the offline PropertySetDef declares (the Xbim.Properties templatetype enum — PSET_TYPEDRIVEN-
    // OVERRIDE/...) — the authoritative source for the seam InheritanceMode [H1]; templates are an IFC4+ concept, so the
    // IFC4.3 catalogue carries them. NOTDEFINED (the enum default a set without a declared template type carries) and an
    // unknown set both resolve None so the projector's structural inference applies; QtoSetDef carries no templatetype, so
    // a Qto set name resolves None here and ModeOf's Qto_* structural branch decides it.
    internal static Option<templatetype> TemplateTypeOf(string setName) =>
        For(ReleaseVersion.Ifc4X3).Psets[setName] is { } set && set.templatetype is var t and not templatetype.NOTDEFINED
            ? Some(t) : None;

    // The class's base-quantity set name + its MethodOfMeasurement (the measurement basis the 5D estimate reads
    // beside the values) + each geometry-relevant QtoDef as its DECLARED NAME paired with its Dimension (mapped
    // from the QtoTypeEnum) — sourced from the catalogue rather than a hand-listed per-class table that slices it;
    // the predefined token scopes applicability the same way the Pset leg does. The MOST-SPECIFIC applicable set is
    // elected — a set whose ApplicableClass row declares the node's PredefinedType wins over a blank-scope row (the
    // declaration-order FirstOrDefault picked whichever the dataset listed first, so a predefined-scoped set never
    // reliably beat its general sibling). The names ride along so a derived quantity always keys by a member the
    // standard set declares, never a fabricated suffix.
    public static Option<(string Set, string Method, Seq<(string Name, Dimension Dimension)> Quantities)> BaseQuantitySet(IfcClass cls, Option<string> predefined, ReleaseVersion schema) =>
        For(schema).Qtos.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .OrderByDescending(set => ScopedMatch(set, cls, predefined))
            .FirstOrDefault() is { } qto
            ? Some((qto.Name, qto.MethodOfMeasurement ?? "", (qto.QuantityDefinitions ?? [])
                .Select(static q => DimensionOf(q.QuantityType).Map(dimension => (Name: q.Name ?? "", Dimension: dimension))).Somes()
                .Where(static row => row.Name.Length > 0).ToSeq()))
            : None;

    // The specificity probe the election orders on: an ApplicableClass row that names BOTH the entity and the node's
    // PredefinedType token is the narrower declaration.
    static bool ScopedMatch(QuantityPropertySetDef set, IfcClass cls, Option<string> predefined) =>
        set.ApplicableClasses.Any(c =>
            string.Equals(c.ClassName, cls.Key, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(c.PredefinedType)
            && predefined.Exists(token => string.Equals(c.PredefinedType, token, StringComparison.OrdinalIgnoreCase)));

    // ApplicableClass carries BOTH the entity name AND an optional PredefinedType scope — a scoped row matches only
    // its token (the retired ClassName-only match over-applied a predefined-scoped set to its whole class), a blank
    // scope matches every token.
    static bool Applies(QuantityPropertySetDef set, IfcClass cls, Option<string> predefined) =>
        set.ApplicableClasses.Any(c =>
            string.Equals(c.ClassName, cls.Key, StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(c.PredefinedType)
                || predefined.Exists(token => string.Equals(c.PredefinedType, token, StringComparison.OrdinalIgnoreCase))));

    // ONE lowering of the Xbim.Properties value-type kind over the FULL constraint surface: the IFC data-type token
    // (off the single/bounded/reference DataType.Type DataTypeEnum, the simple-property DataType.Type), the bSDD
    // ValueKind axis, the allowed-value enumeration (the enumerated kind's EnumList.Items + ConstantList names), the
    // inclusive IFC range (the bounded kind's ValueRangeDef), and the declared unit (the single/bounded UnitType,
    // the simple-property UnitType.Type, the list kind's ListValue.UnitType). The former parallel switches over the
    // same IPropertyValueType stay collapsed; the composite kinds carry no scalar token, so the token is empty and
    // the IDS facet reads the kind.
    static (string DataType, BsddValueKind Kind, Seq<string> Allowed, Option<BsddBounds> Bounds, Seq<string> Units) LowerValue(IPropertyValueType? valueType) => valueType switch {
        TypePropertySingleValue single   => (single.DataType?.Type?.ToString() ?? "", BsddValueKind.Single, Seq<string>(), None, UnitOf(single.UnitType)),
        TypePropertyBoundedValue bounded => (bounded.DataType?.Type?.ToString() ?? "", BsddValueKind.Range, Seq<string>(), RangeOf(bounded.ValueRangeDef), UnitOf(bounded.UnitType)),
        TypePropertyReferenceValue refer => (refer.DataType?.Type?.ToString() ?? "", BsddValueKind.Single, Seq<string>(), None, Seq<string>()),
        TypeSimpleProperty simple        => (simple.DataType?.Type ?? "", BsddValueKind.Single, Seq<string>(), None, Optional(simple.UnitType?.Type).Filter(static u => u.Length > 0).ToSeq()),
        TypePropertyEnumeratedValue e    => ("", BsddValueKind.List, Allowed(e), None, Seq<string>()),
        TypePropertyListValue list       => ("", BsddValueKind.List, Seq<string>(), None, UnitOf(list.ListValue?.UnitType)),
        TypePropertyTableValue           => ("", BsddValueKind.ComplexList, Seq<string>(), None, Seq<string>()),
        TypeComplexProperty              => ("", BsddValueKind.Complex, Seq<string>(), None, Seq<string>()),
        _                                => ("", BsddValueKind.Single, Seq<string>(), None, Seq<string>()),
    };

    // The enumerated kind's allowed-value catalogue: EnumList.Items plus the richer ConstantDef names, blank-pruned.
    static Seq<string> Allowed(TypePropertyEnumeratedValue enumerated) =>
        ((enumerated.EnumList?.Items ?? []).ToSeq()
            + (enumerated.ConstantList ?? []).ToSeq().Map(static c => c.Name ?? ""))
            .Filter(static v => v.Length > 0);

    // IfcPropertyBoundedValue bounds are INCLUSIVE by schema; the dataset stores them as strings, parsed invariant.
    static Option<BsddBounds> RangeOf(ValueRangeDef? range) =>
        BoundsOf(Parse(range?.LowerBoundValue?.Value), Parse(range?.UpperBoundValue?.Value));

    static Option<BsddBounds> BoundsOf(Option<double> lower, Option<double> upper) =>
        lower.IsNone && upper.IsNone ? None : Some(new BsddBounds(lower, upper, None, None));

    static Option<double> Parse(string? value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ? Some(parsed) : None;

    // UnitType carries the IFC unit token (Type when the enum member parsed, _Value the raw dataset text); a
    // unit-less template defers to the seam Dimension.SiSymbol canonical emit unit.
    static Seq<string> UnitOf(UnitType? unit) =>
        Optional(unit?.Type?.ToString() ?? unit?._Value).Filter(static u => u.Length > 0).ToSeq();

    // The [Obsolete] ValueDef legacy slot reads ONLY as the fallback constraint carrier — min/max/default strings on
    // sets predating the typed value kinds; never a data-type source. The pragma scopes the one sanctioned read.
#pragma warning disable CS0618
    static (Option<BsddBounds> Bounds, Option<string> Default) Legacy(PropertyDef p) =>
        p.ValueDef is { } legacy
            ? (BoundsOf(Parse(legacy.MinValue), Parse(legacy.MaxValue)), Optional(legacy.DefaultValue).Filter(static d => d.Length > 0))
            : (None, None);
#pragma warning restore CS0618

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

- Owner: `QuantityDerivation` the base-quantity fold deriving the standard `Qto_*BaseQuantities` from `GeometryMeasures` — the kernel `Rasm` value-object `GeometryMeasures(Option<double> Length, Option<double> Area, Option<double> Volume)` the kernel/Compute resolve from the geometry the seam `Object` node references by content key (`Model/elements#REPRESENTATION_KEYS` `RepresentationContentHash`) and supply to `Derive` (Bim consumes the measure, never tessellates it) — producing the seam `QuantitySet` node values as seam `MeasureValue` under derived-wins precedence. The class's base-quantity SET, its `MethodOfMeasurement` basis (the measurement-rule string the 5D estimate reads beside the values), and each declared quantity's NAME + `Dimension` come from `PropertyCatalog.BaseQuantitySet` (the offline `Xbim.Properties` `QtoSetDef` catalogue, predefined-scoped like the Pset leg), so the roster covers every class the standard defines, not a hand-listed slice — and every derived key names a quantity the standard set actually declares.
- Entry: `QuantityDerivation.Derive(IfcClass cls, Option<string> predefined, ReleaseVersion schema, GeometryMeasures measures, Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence)` derives the geometry-true base quantities for a class and merges them over the occurrence-stored quantities under derived-wins precedence (the geometry takeoff supersedes an authoring tool's stored quantity), returning the seam `QuantitySet` node value map; a class with no `Qto_*BaseQuantities` set in the catalogue returns the occurrence quantities unchanged so a non-takeoff class never blocks. `QuantityDerivation.Decompose(GeometryMeasures measures, Seq<BakedMaterial> materials, Option<SectionProperties> section, Op key)` is the MATERIAL-true takeoff the element-level fold cannot answer ("how much concrete is in this model") — the element volume split per `MaterialId` over the seam `MaterialComposition` the baked element's `Associate` edges bind (`element.Materials` + the `SectionOf` baked section are the caller's `Bake` reads): a `LayerSet` splits by thickness share, a `ConstituentSet` by declared `Fraction`, a `ProfileSet` takes `SectionProperties.Area × Length` re-stamped `QuantityType.Volume` through the band-preserving `WithType` (`Multiply` is dimension-anonymous by seam law), a `Single` carries the element volume whole; a colliding `MaterialId` sums through the seam `MeasureValue.Sum`, an absent element measure yields no row (never a fabricated zero), and the multi-ply WEIGHT decomposition stays the `Rasm.Compute` `AssemblyAggregator`'s — the frozen boundary: volume splits are composition-derivable in full, mass is not.
- Auto: `Derive` reads `PropertyCatalog.BaseQuantitySet(cls, predefined, schema)` (the `Qto_*` set name + its `MethodOfMeasurement` + each geometry-relevant `QtoDef`'s declared NAME paired with its `Dimension`, the MOST-SPECIFIC applicable set elected — a `PredefinedType`-scoped row beats a blank-scope sibling, never dataset declaration order) and folds each distinct dimension through the ONE `Derivations` frozen table — keyed by `Dimension`, each row the canonical takeoff suffix plus the projector from the kernel `GeometryMeasures` (and the material mass density for the weight derivation); the kernel scalar is already SI-base, so each derived value wraps through the seam `MeasureValue.OfSi` (its canonical `Dimension.SiSymbol` unit), keyed by the BARE declared member name under the `NameFor` election (the canonical suffix when the set carries it, else the set's first-declared quantity of that dimension — the emitted key is ALWAYS a member of the standard set, the set name riding the `QuantitySet` bag node, never a `{Set}.{name}`-prefixed non-member key whose derived-wins merge silently never collides), merged over the occurrence map with derived-wins so the 5D `Planning/cost#ESTIMATE` join reads the geometry-true measure (`Volume ≻ Area ≻ Length ≻ Mass`); `NetWeight = NetVolume × massDensity` (`VolumeDim × DensityDim IS MassDim`, so the SI product is the SI mass), absent a density skipping the weight rows; an element-set aggregate of the same `Dimension` reduces through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum` reducer, never a manual `double` fold.
- Packages: Xbim.Properties, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new class's base-quantity set is already in the `Xbim.Properties` `QtoSetDef` catalogue (no edit); a new geometry-derivable dimension is one `Derivations` row carrying its canonical suffix and projector; a new decomposition modality is the seam `MaterialComposition` case arm the `Shares` generated total `Switch` breaks on at compile; the derived quantities merge over the occurrence map under one precedence rule; never a per-class `Derive` method, never a hand-listed per-class set table, and never a re-tessellation in this owner.
- Boundary: base-quantity derivation runs from the kernel `GeometryMeasures` value-object (`Option<double>` `Length`/`Area`/`Volume`) the kernel/Compute resolve from the `RepresentationContentHash` geometry and inject into `Derive`, so a Bim-local `GeometryMeasures` re-declaration or an in-owner geometry-measure computation is the deleted form (Bim depends UP on the kernel and never owns geometry measurement); a re-tessellation in this owner is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the derived value is a seam `MeasureValue` wrapped through `MeasureValue.OfSi` (the seam owns the typed quantity over `Dimension` + UnitsNet), so a Bim-local `MeasureValue` re-declaration or a hand-stamped unit string that drifts from the seam canonical `Dimension.SiSymbol` is the deleted form; `NetWeight` is the homogeneous-element takeoff (`NetVolume × Mechanical.Density`, the material's `Composition/material#MATERIAL_PROPERTY` `Mechanical.Density` resolved One-Hop from the `Associate` material edge) and the multi-ply/layered weight is the `Rasm.Compute` `AssemblyAggregator`'s richer fold, never re-modeled here; the base-quantity SET and its declared quantity names/dimensions come from the `Xbim.Properties` `QtoSetDef` catalogue and a hand-listed per-class `BaseQuantityTable` that slices the standard is the deleted form; a derived value keys ONLY by a quantity name the standard set declares (the `NameFor` election) and a fabricated `{Set}.{suffix}` non-member key — one the `Review/validation#IDS_FACETS` Property facet and a downstream `Qto` reader never match — is the deleted form; the ONE `Dimension`-keyed `Derivations` frozen table owns the measure-and-name election; the derived-wins precedence is applied once in `Derive`, never a per-call-site merge; the per-material `Decompose` reads the seam composition rows ONLY (thickness share, declared fraction, baked section area) — re-deriving a composition here, re-tessellating per material, or fabricating a mass split the composition cannot carry is the deleted form, the weight decomposition staying `Rasm.Compute`'s.

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
    // (the offline Xbim.Properties Qto catalogue) supplies the class's Qto set name, its MethodOfMeasurement basis, and
    // each geometry-relevant QtoDef's DECLARED NAME + Dimension — predefined-scoped exactly like the Pset leg; each
    // derivable dimension folds to a seam MeasureValue under derived-wins so the geometry takeoff supersedes a stored
    // quantity, KEYED by the BARE declared member name (NameFor: the canonical suffix when the set carries it —
    // GrossArea on Qto_SlabBaseQuantities — else the set's first-declared quantity of that dimension — Qto_Wall-
    // BaseQuantities has no GrossArea, so the area takeoff lands on its declared area row). The QuantitySet bag node
    // carries the set name, so a `{Set}.{name}`-prefixed PropertyName is a NON-MEMBER key that never collides with the
    // occurrence rows (derived-wins silently dead) and never matches the IDS facet or a Qto reader — the deleted form.
    // GeometryMeasures
    // is the kernel Rasm value-object the kernel/Compute resolve from the Object RepresentationContentHash geometry and
    // inject (Bim never tessellates); massDensity is the element material's Mechanical.Density
    // (Composition/material#MATERIAL_PROPERTY) resolved One-Hop from the Associate material edge, absent which the
    // weight rows skip rather than fabricate.
    public static Fin<Map<PropertyName, MeasureValue>> Derive(
        IfcClass cls, Option<string> predefined, ReleaseVersion schema, GeometryMeasures measures,
        Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence, Op key) =>
        PropertyCatalog.BaseQuantitySet(cls, predefined, schema).Match(
            None: () => FinSucc(occurrence),
            Some: set => set.Quantities.Map(static q => q.Dimension).Distinct().Fold(FinSucc(occurrence), (rail, dimension) =>
                rail.Bind(acc => Derivations.TryGetValue(dimension, out var d)
                    ? d.Project(measures, massDensity).Match(
                        Some: si => MeasureValue.OfSi(dimension, si)
                            .Map(value => acc.AddOrUpdate(PropertyName.Create(NameFor(set.Quantities, dimension, d.Suffix)), value)),
                        None: () => FinSucc(acc))
                    : FinSucc(acc))));

    // The standard-name election: the canonical takeoff suffix when the set declares it, else the set's
    // first-declared quantity of the dimension — the emitted key is ALWAYS a member of the standard set.
    static string NameFor(Seq<(string Name, Dimension Dimension)> quantities, Dimension dimension, string canonical) {
        Seq<string> names = quantities.Filter(q => q.Dimension == dimension).Map(static q => q.Name);
        return names.Find(name => name == canonical).IfNone(() => names.Head.IfNone(canonical));
    }

    // The material-true takeoff: the element vector split per MaterialId over the seam MaterialComposition the baked
    // element's Associate edges bind (element.Materials + element.Section are the caller's Bake reads) — the 5D/6D
    // per-material join key ("how much concrete is in this model"), which the element-level Derive cannot answer.
    // A colliding MaterialId (occurrence + type-inherited bindings naming one substance) sums through the seam
    // MeasureValue.Sum; an absent element measure yields no row, never a fabricated zero. Volume only: the multi-ply
    // WEIGHT fold stays the Rasm.Compute AssemblyAggregator's (the frozen boundary — volume splits are composition-
    // derivable in full, mass is not).
    public static Fin<Map<MaterialId, MeasureValue>> Decompose(
        GeometryMeasures measures, Seq<BakedMaterial> materials, Option<SectionProperties> section, Op key) =>
        materials.TraverseM(baked => Shares(measures, baked.Material.Composition, section)).As()
            .Bind(rows => rows.Flatten().Fold(FinSucc(Map<MaterialId, MeasureValue>()), (rail, row) =>
                rail.Bind(acc => acc.Find(row.Material).Match(
                    Some: existing => MeasureValue.Sum(Seq(existing, row.Share), key).Map(sum => acc.SetItem(row.Material, sum)),
                    None: () => FinSucc(acc.Add(row.Material, row.Share))))));

    // One share fold per seam composition case — the generated TOTAL Switch, so a new composition modality breaks
    // this at compile time: Single carries the element volume whole, LayerSet splits by thickness share (total > 0
    // by the seam OfLayerSet admission), ConstituentSet by declared Fraction, ProfileSet takes the baked
    // SectionProperties.Area x member Length re-stamped QuantityType.Volume through the band-preserving WithType
    // (Multiply is dimension-anonymous by seam law — the identity re-stamp is the consumer's).
    static Fin<Seq<(MaterialId Material, MeasureValue Share)>> Shares(
        GeometryMeasures measures, MaterialComposition composition, Option<SectionProperties> section) =>
        composition.Switch(
            single: s => ElementVolume(measures).Match(
                None: static () => FinSucc(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Map(volume => Seq((s.Material, volume)))),
            layerSet: s => ElementVolume(measures).Match(
                None: static () => FinSucc(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Bind(volume => {
                    double total = s.Layers.Fold(0.0, static (acc, layer) => acc + layer.Thickness.Si);
                    return s.Layers.TraverseM(layer =>
                        volume.Scale(layer.Thickness.Si / total).Map(share => (layer.Material, share))).As();
                })),
            profileSet: s => section.Bind(sec => measures.Length.Map(length => (Section: sec, Length: length))).Match(
                None: static () => FinSucc(Seq<(MaterialId, MeasureValue)>()),
                Some: pair =>
                    from length in MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, pair.Length)
                    from swept in pair.Section.Area.Multiply(length)
                    from volume in swept.WithType(QuantityType.Volume)
                    select Seq((s.Material, volume))),
            constituentSet: s => ElementVolume(measures).Match(
                None: static () => FinSucc(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Bind(volume => s.Constituents.TraverseM(constituent =>
                    volume.Scale(constituent.Fraction).Map(share => (constituent.Material, share))).As())));

    static Option<Fin<MeasureValue>> ElementVolume(GeometryMeasures measures) =>
        measures.Volume.Map(static v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v));
}
```

## [04]-[TEMPLATE_AUDIT]

- Owner: `TemplateAudit` the graph-wide standard-template conformance fold — the first model-quality question every project asks spec-free ("does each element carry its standard Pset with correctly-typed, in-range values") answered directly against the buildingSMART ground truth this page already resolves, with no authored IDS document; `TemplateVerdict` the `[SmartEnum<string>]` closed verdict vocabulary (`Missing`/`KindMismatch`/`NotAllowed`/`OutOfBounds`/`PatternReject`/`WrongDimension` — one row per constraint axis the `PropertyTemplate` carries, so a new template constraint axis is one verdict row plus one `Verdict` arm); `TemplateFinding` the typed per-element finding row a report renders and a fix pass keys on.
- Entry: `TemplateAudit.Run(ElementGraph graph, Func<IfcClass, Option<BsddClass>> dictionary, Op key)` audits every `"ifc"`-classified occurrence `Object` node against its resolved templates — templates resolve ONCE per distinct `(Classification.Code, PredefinedType.Token)` pair through `PropertyKey.Resolve` (the catalogue floor ∪ live dictionary union, `graph.Header.Schema` the schema, the injected `dictionary` the per-class live evidence a caller supplies or leaves `None` for the offline-only audit) and every element of that pair checks against the SAME resolved map, never a per-element re-resolution; `Fin<T>` carries only the seam `Bake` rail (an absent root or cyclic compose is the graph's fault, never this fold's) and the audit itself is total — a clean model returns the empty finding set.
- Auto: per element the merged `Bake`-derived `element.Properties`/`element.Quantities` bags (type→occurrence precedence already applied by the stamped `InheritanceMode`) probe each template row — an absent value on a `Required` template lands `Missing`; a present value decides per axis: a `Text`/`Enumerated` value outside a non-empty `AllowedValues` lands `NotAllowed`, a `Text` failing the whole-value-anchored `Pattern` lands `PatternReject`, a `Measure` whose `Dimension` disagrees with `ToDimension()` lands `WrongDimension`, a `Measure`, `Integer`, or `Number` outside `Bounds` lands `OutOfBounds` (the bSDD `ClassPropertyContract.v1` min/max carry for Integer and Real properties, not only dimensioned measures), and a seam case irreconcilable with the template `Kind` (a `Complex` where the kind is `Single`) lands `KindMismatch` — the verdict axes are the SAME constraint family the `Review/validation#IDS_FACETS` facet narrows into its `ValueConstraint`, decided here with the failing AXIS named because a QA report acts per axis where a facet needs only pass/fail.
- Receipt: the `Seq<TemplateFinding>` is the model-health evidence — the spec-free baseline tier beneath any authored IDS, rendered by `Rasm.AppUi` as the per-element model-health report; each row carries the element `NodeId`, the `{Set}.{Code}` template coordinate, the verdict, and the actual value so a fix pass addresses the exact property.
- Packages: Xbim.Properties, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new constraint axis is one `TemplateVerdict` row plus one `Verdict` arm reading the new `PropertyTemplate` column; a richer dictionary is the same injected `dictionary` resolver (zero fold edits); never a per-class audit method, never a second checker beside the IDS lane, and never a finding type per verdict.
- Boundary: the audit READS the resolved `PropertyTemplate` map and the baked element — it re-derives neither the template union (that is `PropertyKey.Resolve`'s) nor the type→occurrence merge (the seam `Bake`'s under the stamped mode [H1]); the verdict vocabulary mirrors the `Model/query#ELEMENT_SET` `ValueMatch` restriction family but stays a SEPARATE closed vocabulary because the finding names the failing axis where `ValueMatch` answers only membership — the IDS lane keeps `ValueMatch`, this lane keeps the axis-named verdict, and collapsing the two erases the axis evidence; the audit is spec-FREE (the buildingSMART templates ARE the spec) and a user-authored requirement routes the `Review/validation#IDS_FACETS` owner, never a template-audit extension; the `Pattern` compiles once per template row (`RegexOptions.NonBacktracking`, whole-value anchored — the untrusted-grammar law), never per element.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// One verdict row per PropertyTemplate constraint axis: the finding names WHICH axis failed (a QA report acts per
// axis), where the IDS ValueMatch family answers only membership — two consumers, two shapes, one template source.
[SmartEnum<string>]
public sealed partial class TemplateVerdict {
    public static readonly TemplateVerdict Missing        = new("missing");
    public static readonly TemplateVerdict KindMismatch   = new("kind-mismatch");
    public static readonly TemplateVerdict NotAllowed     = new("not-allowed");
    public static readonly TemplateVerdict OutOfBounds    = new("out-of-bounds");
    public static readonly TemplateVerdict PatternReject  = new("pattern-reject");
    public static readonly TemplateVerdict WrongDimension = new("wrong-dimension");
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TemplateFinding(NodeId Element, string Set, string Code, TemplateVerdict Verdict, Option<PropertyValue> Actual);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The zero-configuration model-QA fold: templates resolve ONCE per distinct (class, predefined) pair and every
// element of the pair checks the SAME map; the per-template Pattern compiles once (NonBacktracking, whole-value
// anchored). The Bake rail is the only fault channel — the audit itself is total.
public static class TemplateAudit {
    public static Fin<Seq<TemplateFinding>> Run(ElementGraph graph, Func<IfcClass, Option<BsddClass>> dictionary, Op key) {
        Seq<Node.Object> occurrences = graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Occurrence && o.Classification.System == "ifc").ToSeq();
        Map<(string Code, string Token), Map<string, (PropertyTemplate Template, Option<Regex> Pattern)>> resolved =
            occurrences.Map(static o => (o.Classification.Code, o.PredefinedType.Token)).Distinct()
                .Fold(Map<(string, string), Map<string, (PropertyTemplate, Option<Regex>)>>(), (acc, pair) =>
                    IfcClass.TryGet(pair.Code).Match(
                        None: () => acc,
                        Some: cls => acc.Add(pair, PropertyKey.Resolve(cls, Token(pair.Token), graph.Header.Schema, dictionary(cls))
                            .Map(static t => (t, t.Pattern.Map(static p => new Regex($"^(?:{p})$", RegexOptions.NonBacktracking | RegexOptions.CultureInvariant)))))));
        return occurrences
            .TraverseM(node => graph.Bake(node.Id, key).Map(element =>
                resolved.Find((node.Classification.Code, node.PredefinedType.Token))
                    .Map(templates => Check(node.Id, templates, element))
                    .IfNone(Seq<TemplateFinding>())))
            .As()
            .Map(static findings => findings.Flatten().ToSeq());
    }

    static Option<string> Token(string token) => Optional(token).Filter(static t => t.Length > 0 && t != "NOTDEFINED");

    static Seq<TemplateFinding> Check(NodeId element, Map<string, (PropertyTemplate Template, Option<Regex> Pattern)> templates, Element baked) =>
        templates.Values.ToSeq().Bind(row => {
            Option<PropertyValue> actual = baked.Properties.Find(b => b.SetName == row.Template.Set)
                .Bind(bag => bag.Find(PropertyName.Create(row.Template.Code)))
                | baked.Quantities.Find(b => b.SetName == row.Template.Set)
                    .Bind(bag => bag.Find(PropertyName.Create(row.Template.Code)))
                    .Map(static measure => (PropertyValue)new PropertyValue.Measure(measure));
            return Verdict(row.Template, row.Pattern, actual)
                .Map(verdict => new TemplateFinding(element, row.Template.Set, row.Template.Code, verdict, actual)).ToSeq();
        });

    // One arm per constraint axis, most-specific first; a value passing every declared axis yields None — no finding.
    static Option<TemplateVerdict> Verdict(PropertyTemplate template, Option<Regex> pattern, Option<PropertyValue> actual) =>
        actual.Match(
            None: () => template.Required ? Some(TemplateVerdict.Missing) : None,
            Some: value => !Compatible(template.Kind, value) ? Some(TemplateVerdict.KindMismatch) : value switch {
                PropertyValue.Text t when !template.AllowedValues.IsEmpty && !template.AllowedValues.Contains(t.Value) => Some(TemplateVerdict.NotAllowed),
                PropertyValue.Text t when pattern.Exists(p => !p.IsMatch(t.Value)) => Some(TemplateVerdict.PatternReject),
                PropertyValue.Enumerated e when !template.AllowedValues.IsEmpty && e.Selected.Exists(s => !template.AllowedValues.Contains(s.Render())) => Some(TemplateVerdict.NotAllowed),
                PropertyValue.Measure m when template.ToDimension().Exists(d => d != m.Value.Dimension) => Some(TemplateVerdict.WrongDimension),
                PropertyValue.Measure m when template.Bounds.Exists(b => !Within(b, m.Value.Si)) => Some(TemplateVerdict.OutOfBounds),
                PropertyValue.Integer i when template.Bounds.Exists(b => !Within(b, (double)i.Value)) => Some(TemplateVerdict.OutOfBounds),
                PropertyValue.Number n when template.Bounds.Exists(b => !Within(b, n.Value)) => Some(TemplateVerdict.OutOfBounds),
                _ => None,
            });

    static bool Compatible(BsddValueKind kind, PropertyValue value) =>
        kind == BsddValueKind.Range ? value is PropertyValue.Bounded
        : kind == BsddValueKind.List ? value is PropertyValue.List or PropertyValue.Enumerated
        : kind == BsddValueKind.Complex ? value is PropertyValue.Complex
        : kind == BsddValueKind.ComplexList ? value is PropertyValue.Table or PropertyValue.List
        : value is not (PropertyValue.Bounded or PropertyValue.List or PropertyValue.Table or PropertyValue.Complex);

    static bool Within(BsddBounds bounds, double si) =>
        bounds.MinInclusive.ForAll(min => si >= min) && bounds.MaxInclusive.ForAll(max => si <= max)
        && bounds.MinExclusive.ForAll(min => si > min) && bounds.MaxExclusive.ForAll(max => si < max);
}
```

## [05]-[RESEARCH]

- [TEMPLATE_RESOLUTION]: the `PropertyKey.Resolve` catalogue-floor ∪ bSDD-live union grounds against `.api/api-xbim-properties` — `DefinitionSets`, `ApplicableClass{ClassName, PredefinedType}` (BOTH matched by `Applies`), `PropertyDef.PropertyType.PropertyValueType` lowered through `TypePropertySingleValue{DataType, UnitType}`/`TypePropertyBoundedValue{DataType, UnitType, ValueRangeDef{LowerBoundValue.Value, UpperBoundValue.Value}}`/`TypePropertyReferenceValue{DataType}`/`TypeSimpleProperty{DataType.Type, UnitType.Type}`/`TypePropertyEnumeratedValue{EnumList.Items, ConstantList}`/`TypePropertyListValue{ListValue.UnitType}` (`DataType.Type` a `DataTypeEnum?`, `UnitType.Type` else `_Value` the raw dataset token), `QuantityPropertyDef.NameAliases` (`NameAlias{Lang, Value}`), `ValueDef{MinValue, MaxValue, DefaultValue}` the `[Obsolete]` legacy slot read only as the pragma-scoped fallback constraint carrier, and the `templatetype` enum — and against `.api/api-bsdd` `ClassPropertyContract.v1` (`propertyCode`/`dataType`/`propertySet`/`isRequired`/`propertyValueKind` PLUS the class-scoped `allowedValues[]`/`pattern`/`minInclusive`..`maxExclusive`/`units[]`/`predefinedValue` and the seven `dimension*` int exponents the classification-owned `BsddProperty`/`BsddBounds`/`SiExponents` carriers already lower) — `Xbim.Properties` is the always-available canonical floor (deterministic, schema-versioned, network-free) and bSDD the live dictionary that enriches/overrides it with dictionary-wins, so a property carries its declared IFC `DataType` AND its legal-value constraint whether or not the service is reachable; the `SiExponents`→`Dimension.Create(length, mass, time, current, temperature, amount, luminousIntensity)` seeding grounds against the seam `Properties/quantity#MEASURE_VALUE` generated factory; the typing flow is the `Projection/semantic#SEMANTIC_PROJECTOR` `PropertyLowering.Lower` narrowing the live `IfcValue` runtime type at import (the template `DataType` is the EXPECTED type the IDS facet validates against and the authoring path constructs from, never a `PropertyValue.Of(value, dataType)` the seam does not own). The hand-coded `Pset_*` table is the rejected form (the package owns the template dataset).
- [INHERITANCE_STAMP]: the `PropertyInheritance.ModeOf` `InheritanceMode` classifier grounds against the seam inheritance law (each `PropertySet`/`QuantitySet` node carries a structural `InheritanceMode` — `OccurrenceWins`/`TypeDrivenOverride`/`TypeDrivenOnly` — stamped at Bim ingest so `Bake` applies the type→occurrence precedence wholly within the seam) and the IFC `IfcPropertySetTemplate.TemplateType` vocabulary (`PSET_TYPEDRIVENOVERRIDE`/`PSET_TYPEDRIVENONLY`/`PSET_OCCURRENCEDRIVEN`/…) the `Xbim.Properties` `PropertySetDef.templatetype` declares — the authoritative source the catalogue carries, with the structural inference (a `Qto_*` quantity set and a type-bound property set are type-driven-override) the fallback when no template type is declared; the projector reads `ModeOf` and stamps the seam node, the seam `Bake` reading the stamp when folding the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge a `Component` Type `Object` binds (the neutral seam lowering of the IFC `IfcRelDefinesByType`) so the precedence is never re-derived per call site and never a fragile `*TypeCommon` set-name suffix heuristic.
- [BASE_QUANTITY_DERIVE]: the `QuantityDerivation.Derive` base-quantity fold grounds against the buildingSMART `Qto_*BaseQuantities` definitions the `Xbim.Properties` `QtoSetDef{QuantityDefinitions, MethodOfMeasurement}`/`QtoDef.Name`/`QtoDef.QuantityType` (`Q_LENGTH`/`Q_AREA`/`Q_VOLUME`/`Q_WEIGHT`/…) catalogue declares per class (predefined-scoped through the same `Applies`) and the kernel `Rasm` `GeometryMeasures` the seam node references by content key, deriving the geometry-true takeoff (incl. `NetWeight = NetVolume × Mechanical.Density`, the `Composition/material#MATERIAL_PROPERTY` density) rather than re-tessellating and keying each derived value by a declared set member (the `NameFor` canonical-suffix-else-first-declared election — `Qto_WallBaseQuantities` declares no `GrossArea`, so a fabricated non-member key never matches the IDS facet or a `Qto` reader), the `Planning/cost#ESTIMATE` join reading by the `Dimension` rank; the typed VALUE family `PropertyValue`/`MeasureValue` is seam-owned and library-neutral — the seam `Dimension` `[ComplexValueObject]` over the 7 SI base dimensions PLUS the `QuantityType` `[ValueObject<string>]` name discriminator (the exponent vector is not injective over quantity types) — so this page owns the TEMPLATE and the PRECEDENCE, the seam owns the value, and the one `Dimension`-keyed `Derivations` table wraps every derived value through `MeasureValue.OfSi`. The per-material `Decompose` grounds against the seam accessors and algebra verbatim: `BakedMaterial(Node.Material Material, MaterialUsage Usage)` off `element.Materials`, the `MaterialComposition` generated total `Switch` over `MaterialLayer.Thickness` (`MeasureValue`, `.Si` SI-native)/`MaterialConstituent.Fraction`/`SectionProperties.Area`, and the `MeasureValue.Scale`/`Multiply`/`WithType`/`Sum` fold surface — the LayerSet total is positive by the seam `OfLayerSet` admission, so the thickness-share factor is finite by construction.
