# [BIM_PROPERTY_TEMPLATES]

The IFC Pset/Qto TEMPLATE authority over the `Rasm.Element` seam graph: the offline `Xbim.Properties` `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` buildingSMART catalogue is the canonical, schema-versioned, scope-selected, network-free template floor — every standard `Pset_*` and `Qto_*`, its `ApplicableClasses` (entity AND `PredefinedType` scope), each property's IFC `DataType`/value-type kind PLUS its full value constraint (the enumerated kind's `EnumList`/`ConstantList` allowed values, the bounded kind's `ValueRangeDef` range, the declared `UnitType`, the localized `NameAliases`), and each base quantity's `QtoTypeEnum` + `MethodOfMeasurement` — and the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass` dictionary enriches it (dictionary-wins on a name collision), its rows carrying the class-scoped `allowedValues`/`pattern`/bounds/`units`/SI base-dimension constraint surface. `PropertyKey.Resolve` unions the two into one FULL-CONSTRAINT `PropertyTemplate` map scoped by the node's `PredefinedType` token, so the `Review/validation#IDS_FACETS` Property facet validates VALUE constraints, never type alone; `PropertyInheritance.ModeOf` reads the authoritative IFC `templatetype` the catalogue declares to stamp each seam `PropertySet`/`QuantitySet` node with its `InheritanceMode` at ingest so the seam `Bake` applies the correct type→occurrence precedence over the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge a `Component` Type `Object` binds — the `typeBound` bag the classifier reads is the set the projector resolved from an `IfcTypeProduct`/`IfcElementType` (the seam `ObjectKind.Type` node the `Rasm.Materials` `ComponentProjector` mints), so a wall-type's shared `Pset_WallCommon` rides the type bag and the occurrence's overriding values the occurrence bag, the two merged once in `Bake`; `QuantityDerivation.Derive` sources the class's base-quantity set from the catalogue and folds the geometry-true takeoff from the kernel geometry the node references by content key. The typed VALUE half is seam-owned: the closed `PropertyValue` family (`Text`/`Measure`/`Boolean`/`Logical`/`Integer`/`Number`/`Binary`/`Temporal`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex` — `Logical` the three-valued `IfcLogical`/`IfcLogicalEnum` `UNKNOWN`, `Complex` the named nested `IfcComplexProperty` bag) and the `MeasureValue` (`QuantityType`/`Dimension`/`Si`/`CanonicalUnit` over the `Dimension` `[ComplexValueObject]` + the `QuantityType` `[ValueObject<string>]` discriminator) live on `Rasm.Element/Properties` — this page owns the TEMPLATE (which properties a class carries and their declared types) and the PRECEDENCE policy, never the value. A property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, never a `(SetName, Name, string Value)` triple. The page is the template oracle the `Projection/semantic#SEMANTIC_PROJECTOR` projector and the `Review/validation#IDS_FACETS` Property facet read, and `TemplateAudit.Run` turns the same resolved templates into the zero-configuration graph-wide model-QA fold — presence, declared type, allowed values, bounds, pattern, and dimension audited per element with no authored IDS — the BASELINE tier of the `Review/validation#MODEL_HEALTH` two-tier verdict surface, this page producing the stream and that owner composing it beneath the authored-IDS lane; a hand-coded `Pset_*` property table beside the `Xbim.Properties` catalogue is the deleted form.

## [01]-[INDEX]

- [02]-[PROPERTY_TEMPLATES]: `PropertyCatalog` the `Xbim.Properties` offline standard-template catalogue (loaded once per `(schema, scope)` pair), `TemplateScope` the `[SmartEnum<string>]` definition-set policy carrying each scope's two `Definitions<T>` loaders and its pinned schema, `PropertyTemplate` the unified resolved full-constraint template both sources lower into, `PropertyKey` the curated well-known `Pset_*` recognition anchors, `PropertyKey.Resolve` the predefined-scoped catalogue-floor ∪ bSDD-live template union, and `PropertyInheritance.ModeOf` the `templatetype`-driven `InheritanceMode` classifier stamped on each seam bag node at ingest.
- [03]-[BASE_QUANTITIES]: `PropertyCatalog.BaseQuantitySet` the per-`IfcClass` `Qto_*BaseQuantities` set + its `MethodOfMeasurement` basis + each geometry-relevant `QtoDef`'s declared name + `Dimension` (from the catalogue, never a hand-listed slice), `QuantityDerivation.Derive` the base-quantity fold deriving the geometry-true takeoff (incl. `NetWeight` from volume × material density) from the kernel geometry measures the seam node references by content key, keyed by declared set members only, producing the seam `QuantitySet` node values under derived-wins precedence, and `QuantityDerivation.Decompose` the material-true takeoff — the element volume split per `MaterialId` over the seam `MaterialComposition` (layer thickness share, constituent `Fraction`, per-compound-row section-area × length), the per-material join key every 5D/6D consumer reads.
- [04]-[TEMPLATE_AUDIT]: `TemplateAudit.Run` the zero-configuration model-QA fold auditing a whole seam `ElementGraph` against the resolved buildingSMART templates themselves — per-`(class, predefined)` template resolution through `PropertyKey.Resolve`, per-element presence/kind/allowed-value/bounds/pattern/dimension verdicts as typed `TemplateFinding` rows over the `TemplateVerdict` vocabulary — the baseline tier the `Review/validation#MODEL_HEALTH` owner composes; the authored-requirement lane stays `Review/validation#IDS_FACETS`.

## [02]-[PROPERTY_TEMPLATES]

- Owner: `PropertyCatalog` the offline `Xbim.Properties` template catalogue — `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` loaded once per `(IFC Version, TemplateScope)` pair and cached, the always-available buildingSMART template floor declaring what every `Pset_*`/`Qto_*` IS (its `ApplicableClasses` with entity + `PredefinedType` scope, its `PropertyDef`s with their `DataType`/value-type kind + allowed values + range + unit + aliases, its `QtoDef`s with their `QtoTypeEnum` under the set's `MethodOfMeasurement`); `PropertyTemplate` the unified resolved FULL-CONSTRAINT template (`Set`/`Code`/`DataType`/`Kind`/`Required` + `AllowedValues`/`Bounds`/`Pattern`/`Units`/`SiDimension`/`Predefined`/`Aliases`, its `SiDimension` the seam `Dimension` itself rather than a second exponent carrier every consumer re-projects) both the catalogue `PropertyDef` and the bSDD `BsddProperty` lower into; `PropertyKey` the curated well-known `Pset_*` recognition anchors (the opinionated common set name + its `IfcDomain`) authoring surfaces first; `TemplateScope` the closed definition-set policy value (`Standard` the bundled buildingSMART sets, `Cobie` the COBie handover superset, `Handover` both) each row carrying its `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` loader pair and the schema its dataset pins; `PropertyInheritance` the classifier reading the catalogue's authoritative IFC `templatetype` onto the seam `InheritanceMode`. The typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned (`Rasm.Element/Properties`); this page supplies the TEMPLATE (which properties, their declared `DataType`) the seam value is constructed against, never the value.
- Entry: `PropertyKey.Resolve(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, Option<BsddClass> dictionary)` resolves a class's property templates — `PropertyCatalog.Templates(cls, predefined, schema, scope)` (the offline `Xbim.Properties` floor under the scope's own definition set, every `PropertySetDef` whose `ApplicableClasses` names the class AND whose `PredefinedType` scope, when declared, matches the node's token — a `ClassName`-only match over-applies a predefined-scoped Pset to its whole class) unioned UNDER the live `BsddClass.Properties` dictionary rows (dictionary-wins on a `{Set}.{Code}` collision), so a measured property carries its declared IFC `DataType` AND its value constraint, and the offline catalogue resolves when bSDD is unreachable; `PropertyCatalog.TemplateTypeOf(string setName, TemplateScope scope)` is the `internal` catalogue query returning the raw IFC `templatetype` a set declares (the `Xbim.Properties` enum kept Bim-internal, never a public seam return); `PropertyInheritance.ModeOf(string setName, bool typeBound, TemplateScope scope)` is the public canonical surface returning the seam `InheritanceMode` the projector stamps on a bag at ingest; `Fin<T>` is not the rail here — resolution degrades to the offline catalogue (and to the structural inference) when the dictionary is unreachable, never faulting ingest.
- Auto: `Resolve` folds the bSDD `BsddProperty` rows (lowered to the full-constraint `PropertyTemplate` — `DataType`/`ValueKind`/`IsRequired` plus `AllowedValues` value strings, `Bounds`, `Pattern`, `Units`, the seam `SiDimension`, and the class-fixed `PredefinedValue`) OVER the `PropertyCatalog.Templates` floor with the two-arm `AddOrUpdate` so a dictionary-declared property overrides the offline default and a bSDD-only property still resolves — dictionary-wins-when-SPEAKING: a declared narrowing (`AllowedValues`/`Bounds`/`Pattern`/`Units`) overrides its floor axis, a SILENT axis keeps the floor's constraint, and the localized `Aliases` only the catalogue carries always survive, so a terse dictionary row never erases a floor constraint it merely failed to restate; `PropertyCatalog.Templates` reads `definitions.DefinitionSets`, keeps the sets whose `ApplicableClasses` match entity + `PredefinedType` scope, and lowers each `PropertyDef` through its `PropertyType.PropertyValueType` value-type kind in ONE pass (`LowerValue`): the scalar IFC data-type token off `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyReferenceValue` `DataType.Type` (a `DataTypeEnum`) and `TypeSimpleProperty.DataType.Type`, the same kind selecting the `BsddValueKind`, the enumerated kind yielding its `EnumList.Items`+`ConstantList` allowed values, the bounded kind its inclusive `ValueRangeDef` range, the single/bounded/simple/list kinds their declared `UnitType` token (the composite kinds carry no scalar token), and `NameAliases` folding onto the per-language display map — so the projector and the IDS facet know each property's expected type AND legal values without re-deriving either; `ModeOf` reads `PropertyCatalog.TemplateTypeOf` (the `templatetype` enum the `PropertySetDef` declares) — `PSET_TYPEDRIVENONLY`/`QTO_TYPEDRIVENONLY` is `TypeDrivenOnly`, `PSET_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENOVERRIDE` is `TypeDrivenOverride`, every other declared kind (`PSET_OCCURRENCEDRIVEN`/`PSET_PERFORMANCEDRIVEN`/`PSET_PROFILEDRIVEN`/`PSET_MATERIALDRIVEN`) is `OccurrenceWins`, and `NOTDEFINED` resolves None — falling back to the structural inference (a `Qto_*` quantity set, whose `QtoSetDef` carries no `templatetype`, and a type-bound property set are `TypeDrivenOverride`, an occurrence-only set `OccurrenceWins`) when no catalogue template type is declared, so the seam `Bake` applies the IFC inheritance once per bag rather than a per-call-site merge.
- Receipt: the resolved `PropertyTemplate` map is the EXPECTED-type AND VALUE-CONSTRAINT evidence the `Review/validation#IDS_FACETS` Property facet validates the seam `PropertyValue` against (`AllowedValues`/`Bounds`/`Pattern` narrow into the facet's `ValueConstraint`; `SiDimension` corroborates a measured value's `Dimension`) and the from-scratch authoring path constructs a typed value from; at IFC import the typing is the `Projection/semantic#SEMANTIC_PROJECTOR` `PropertyLowering.Lower` narrowing the live `IfcValue` runtime type onto the seam `PropertyValue` case directly (the catalogue/bSDD `DataType` is the expected type, never a `PropertyValue.Of(value, dataType)` the seam does not own); the stamped `InheritanceMode` is the precedence evidence the seam `Bake` reads when folding the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge (the neutral seam lowering of the IFC `IfcRelDefinesByType` the projector authored) into the occurrence — the `Component` Type bag's values merging into the occurrence by the stamped mode.
- Packages: Xbim.Properties, ids-lib, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new standard Pset is already in the `Xbim.Properties` catalogue (no edit) or one live bSDD dictionary row; a new curated recognition anchor is one `PropertyKey` row; a new bundled definition set is one `TemplateScope` row carrying its loader pair and schema pin, with no memo-key or call-site edit; a new IFC value-type kind is one `BsddValueKind`-mapping arm; a new constraint axis is one `PropertyTemplate` column BOTH lowerings fill; a new inheritance policy is the catalogue `templatetype` the dictionary declares; never a hand-coded `Pset_*` property table, never a per-Pset type, and never a second property store.
- Boundary: there is ONE property model and it is the seam graph — a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, and a per-Pset `WallProperties`/`SlabProperties` class family is the deleted form; the typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned and re-declaring it here is the deleted form — this page owns the TEMPLATE (`DataType`/value-type kind) and the PRECEDENCE policy, the seam owns the value; the offline standard template is the `Xbim.Properties` `Definitions<T>` catalogue read as the canonical floor and a hand-coded `Pset_*` property table beside it is the deleted form; the definition-set choice is the `TemplateScope` policy value the memo key carries, so a `LoadAllDefault`/`LoadIFC4COBie`/`LoadIFC4AndCOBie` selector at a call site or a second cache keyed by `Version` alone (whose second load evicts the first scope's dataset) is the deleted form, and a COBie-scoped resolution reads its row's pinned `IFC4` dataset whatever the model's `ReleaseVersion` rather than claiming a schema its definitions do not ship; applicability matches BOTH `ApplicableClass.ClassName` and its `PredefinedType` scope; the constraint surface lowers from the CURRENT value-type kinds alone (`TypePropertyEnumeratedValue.EnumList`/`ConstantList`, `TypePropertyBoundedValue.ValueRangeDef`, the `UnitType` axis), so a set whose only min/max/default lived on the retired slot resolves an ABSENT constraint and a suppression-scoped read of a retired member is the deleted form; the live bSDD dictionary unions OVER the catalogue with dictionary-wins, never the SOLE source and never a fault on a service miss; the type-vs-occurrence precedence is the IFC `IfcPropertySetTemplate.templatetype` the catalogue declares, lowered to the seam `InheritanceMode` at ingest and applied once in the seam `Bake`, never a per-call-site merge, never a stored-twice type→occurrence fold, and never a fragile set-name suffix heuristic; `Xbim.Properties` is a TEMPLATE source only (no IFC entity graph, no property values, no IDS engine) and consuming it as a model reader or value store is the rejected form; every bag key this page writes or reads mints through the owner-blessed `PropertyCategory.Seam.Row` EMPTY-prefix category (a round-tripped IFC/bSDD code stays bare) and a call-site `PropertyName.Create` in the derivation writer or the audit reader is the key-space fork the branch row-name custody ruling deletes; requiredness is a THREE-state axis the offline dataset never states, so the floor answers `None`, only a dictionary declares, and a `Missing` verdict traces to a stated requirement rather than to a `false` this page supplied on the catalogue's behalf; `SiDimension` merges under the same dictionary-wins-when-SPEAKING law as every other narrowing, an unconditional dictionary take stripping the quantity floor's own `QtoTypeEnum`-derived dimension and darkening `TemplateVerdict.WrongDimension`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using IdsLib.IfcSchema;                        // SchemaInfo — the ids-lib measure/datatype authority every declared IFC data type resolves its SI exponents through
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

// --- [TYPES] ------------------------------------------------------------------------------
// The definition-set SCOPE a catalogue loads: the buildingSMART standard sets alone, the COBie handover superset, or
// both. It is a policy VALUE, not a knob — each row carries the two Definitions<T> loaders it drives and the schema it
// pins, so a new bundled definition set is one row and the memo key widens by nothing. The two loader columns are the
// C#-forced shape: Definitions<PropertySetDef> and Definitions<QtoSetDef> are distinct closed generics over one
// instance method and no delegate spans both, so the pair is one row's data rather than a switch at For.
// Schema pins the version a scope's dataset ships — LoadIFC4COBie and LoadIFC4AndCOBie carry IFC4 definitions
// alone — so a COBie-scoped resolution reads IFC4 templates whatever the model's own ReleaseVersion, and Standard
// leaves the pin absent so the seam release lowers.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TemplateScope {
    public static readonly TemplateScope Standard = new("standard", None,
        static psets => psets.LoadAllDefault(), static qtos => qtos.LoadAllDefault());
    public static readonly TemplateScope Cobie = new("cobie", Some(Version.IFC4),
        static psets => psets.LoadIFC4COBie(), static qtos => qtos.LoadIFC4COBie());
    public static readonly TemplateScope Handover = new("handover", Some(Version.IFC4),
        static psets => psets.LoadIFC4AndCOBie(), static qtos => qtos.LoadIFC4AndCOBie());

    public Option<Version> Schema { get; }
    public Action<Definitions<PropertySetDef>> LoadPsets { get; }
    public Action<Definitions<QtoSetDef>> LoadQtos { get; }

    private TemplateScope(string key, Option<Version> schema,
        Action<Definitions<PropertySetDef>> psets, Action<Definitions<QtoSetDef>> qtos) : this(key) =>
        (Schema, LoadPsets, LoadQtos) = (schema, psets, qtos);
}

// --- [MODELS] -----------------------------------------------------------------------------
// One resolved property template — the unified FULL-CONSTRAINT shape BOTH the offline Xbim.Properties PropertyDef
// and the live bSDD BsddProperty lower into, so a consumer reads ONE template type regardless of source. DataType is
// the IFC data-type token (IfcThermalTransmittanceMeasure/IfcLabel/...), Kind the value-type kind the seam
// PropertyValue arm is chosen from (BsddValueKind, Semantics/classification), Required the THREE-state requirement
// axis — the offline buildingSMART dataset carries no requiredness column at all (PropertyDef and its
// QuantityPropertyDef base publish Name/Definition/aliases/PropertyType and no requiredness at all), so a floor row answers None
// and only a dictionary row declares; a bare `false` there asserts "optional" on the floor's behalf and a presence
// audit then reads every unstated property as satisfied;
// AllowedValues/Bounds/Pattern are the VALUE constraint the IDS Property facet narrows into a ValueConstraint (type
// alone was the retired thin slice), Units the declared unit vocabulary the seam MeasureValue coercion corroborates,
// SiDimension the seam Dimension itself, Predefined the class-fixed value, Aliases the per-language display names
// an authoring surface renders. Bounds reuses the classification-owned BsddBounds carrier and SiDimension IS the seam
// Dimension both sources build directly — one concept, one type, no per-consumer re-projection of an exponent vector.
public readonly record struct PropertyTemplate(
    string Set, string Code, string DataType, BsddValueKind Kind, Option<bool> Required,
    Seq<string> AllowedValues, Option<BsddBounds> Bounds, Option<string> Pattern,
    Seq<string> Units, Option<Dimension> SiDimension, Option<string> Predefined,
    Map<string, string> Aliases) {
    // Units is the declared unit VOCABULARY (a dataset states at most one token, a bSDD row may state several
    // spellings of one unit); Unit is the single declared token a renderer or a handover column reads — the first the
    // source stated, None when neither source declared one, where the seam Dimension.SiSymbol is the canonical emit
    // unit. Code is the property NAME: the IFC/bSDD property code IS the bag key, so no second name column exists.
    public Option<string> Unit => Units.Head;
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
    public static Seq<PropertyKey> TemplatesFor(IfcDomain domain) => toSeq(Items).Filter(row => row.Domain == domain);

    // Per-class property templates: the bSDD live dictionary rows (dictionary-wins) unioned OVER the offline
    // Xbim.Properties standard catalogue floor (.api/api-xbim-properties + .api/api-bsdd), keyed {Set}.{Code}. The
    // catalogue is the always-available buildingSMART floor (deterministic, schema-versioned, network-free); bSDD
    // enriches/overrides when live; absent a dictionary the catalogue alone resolves — never a fabricated anchor.
    // predefined is the node's PredefinedType token: an ApplicableClass.PredefinedType-scoped set resolves ONLY for
    // a matching token, so a predefined-scoped Pset never over-matches its whole class. The bSDD row lowers its FULL
    // class-scoped constraint (allowed values, bounds, pattern, units, SI exponents, predefined value) — the
    // class-level narrowing wins over the property master by contract. scope is the definition-set policy value the
    // catalogue floor loads under, so a COBie handover resolution and a standard one read one entry.
    public static Map<string, PropertyTemplate> Resolve(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, Option<BsddClass> dictionary) =>
        dictionary.Map(static d => d.Properties).IfNone(Seq<BsddProperty>())
            .Filter(static p => p.PropertySet.Length > 0)
            .Fold(PropertyCatalog.Templates(cls, predefined, schema, scope),
                  static (template, p) => template.AddOrUpdate($"{p.PropertySet}.{p.Code}",
                      Some: existing => Lower(p, Some(existing)),
                      None: () => Lower(p, None)));

    // The bSDD row lowering under dictionary-wins-when-SPEAKING: a dictionary row overrides ONLY the axes it states — a
    // declared narrowing (allowed values, bounds, pattern, units, SI dimension) wins, a SILENT axis keeps the catalogue
    // floor's, and the localized Aliases only the offline catalogue carries always survive — so a terse dictionary row
    // never ERASES a floor constraint it merely failed to restate (the silent-axis wipe was the masked-constraint-loss
    // form). SiDimension follows that law like every other narrowing: the floor DOES carry one on every quantity row
    // (QuantityTemplate derives it from the QtoTypeEnum), so an unconditional dictionary take strips a Qto_* row's
    // dimension and darkens TemplateVerdict.WrongDimension for that property. Requiredness is the ONE axis with no
    // floor to preserve, so a dictionary row's flag is always the whole answer.
    static PropertyTemplate Lower(BsddProperty p, Option<PropertyTemplate> floor) =>
        new(p.PropertySet, p.Code, p.DataType, p.ValueKind, Some(p.IsRequired),
            p.AllowedValues.IsEmpty ? floor.Map(static f => f.AllowedValues).IfNone(Seq<string>()) : p.AllowedValues.Map(static v => v.Value),
            p.Bounds.IsSome ? p.Bounds : floor.Bind(static f => f.Bounds),
            p.Pattern.IsSome ? p.Pattern : floor.Bind(static f => f.Pattern),
            p.Units.IsEmpty ? floor.Map(static f => f.Units).IfNone(Seq<string>()) : p.Units,
            p.SiDimension.IsSome ? p.SiDimension : floor.Bind(static f => f.SiDimension),
            Optional(p.PredefinedValue).Filter(static s => s.Length > 0),
            floor.Map(static f => f.Aliases).IfNone(Map<string, string>()));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The offline standard-Pset/Qto template catalogue: Xbim.Properties Definitions<T> loaded once per IFC schema and
// cached (the CDDL-1.0 binary referenced, never vendored). It declares what a Pset_*/Qto_* IS (its applicable classes
// with PredefinedType scope, its properties' DataType/value-type kind + constraint + unit + aliases, its base
// quantities' QtoTypeEnum + MethodOfMeasurement), never an IFC entity graph and never a property value. The live bSDD dictionary (Semantics/classification#BSDD_RESOLUTION) unions OVER it with dictionary-wins.
public static class PropertyCatalog {
    // The memo key is (schema, scope): one dataset per schema PER definition-set scope, so a standard and a COBie
    // handover catalogue coexist rather than the second load evicting the first behind one Version key. The mint body
    // is ONE fold reading the scope row's two loader columns — the per-scope load bodies are row data, never arms here.
    static readonly ConcurrentDictionary<(Version Schema, TemplateScope Scope), (Definitions<PropertySetDef> Psets, Definitions<QtoSetDef> Qtos)> Catalogues = new();

    // The scope's OWN pin outranks the caller's release — a COBie dataset ships IFC4 definitions and nothing else — so
    // the resolution order is stated ONCE here and every entry composes it rather than re-deciding at its own call.
    static (Definitions<PropertySetDef> Psets, Definitions<QtoSetDef> Qtos) For(ReleaseVersion schema, TemplateScope scope) =>
        For(scope.Schema.IfNone(() => Lower(schema)), scope);

    static (Definitions<PropertySetDef> Psets, Definitions<QtoSetDef> Qtos) For(Version schema, TemplateScope scope) =>
        Catalogues.GetOrAdd((schema, scope), static key => {
            Definitions<PropertySetDef> psets = new(key.Schema); key.Scope.LoadPsets(psets);
            Definitions<QtoSetDef> qtos = new(key.Schema); key.Scope.LoadQtos(qtos);
            return (psets, qtos);
        });

    // The seam ReleaseVersion (the model Header currency) -> the Xbim.Properties Version: templates exist for the three
    // published buildingSMART schemas, so a finer seam release folds onto its base schema rather than missing the
    // catalogue. The fold is ORDERED, not an equality ladder, and it names only the two live boundary members
    // (GeometryGym retires IFC4/IFC4A1/IFC4X1/IFC4X2 and every IFC4X3 release candidate): everything at or
    // below IFC2x3 reads the 2x3 dataset, everything below IFC4X3 — the whole IFC4 family and the withdrawn 4.x drafts
    // — reads IFC4, and IFC4X3 onward (including IFC4X3_ADD2 and the IFC4X4 draft) reads IFC4x3. A new enum member
    // therefore lands on the right side by its own ordinal, where an equality ladder silently fell through to the tail.
    static Version Lower(ReleaseVersion schema) =>
        schema <= ReleaseVersion.IFC2x3   ? Version.IFC2x3
        : schema < ReleaseVersion.IFC4X3  ? Version.IFC4
        : Version.IFC4x3;

    // The standard property templates applicable to a class: every offline PropertySetDef whose ApplicableClasses
    // names the IFC entity AND whose PredefinedType scope (when declared) matches the node's token, each PropertyDef
    // lowered to the unified full-constraint PropertyTemplate in ONE pass, keyed {Set}.{Code}. PropertyDefinitions
    // is a nullable backing list (the safe Definitions getter guards it), so an empty/absent set folds to no rows.
    public static Map<string, PropertyTemplate> Templates(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope) {
        var catalogues = For(schema, scope);
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
    // lowering, and NameAliases fold onto the per-language display map. The CURRENT value-type kinds are the whole
    // constraint surface — a set whose only constraint lived on the retired min/max/default slot resolves an
    // ABSENT constraint, which is the honest reading of a source that no longer states one. Requiredness stays
    // dictionary-sourced because the offline dataset declares none at all, landing None so the audit grades what a
    // source stated rather than what a literal implied.
    // The SI exponent vector is NO LONGER one of them: the dataset states the property's IFC DATA TYPE, and a data type
    // determines its dimension, so DimensionOf resolves it through the ids-lib measure authority and every measured
    // Pset template carries the dimension the WrongDimension verdict grades against.
    static PropertyTemplate TemplateOf(string setName, PropertyDef p) {
        var (dataType, kind, allowed, bounds, units) = LowerValue(p.PropertyType?.PropertyValueType);
        return new PropertyTemplate(
            setName, p.Name, dataType, kind, None,
            allowed, bounds, None,
            units, DimensionOf(dataType), None,
            (p.NameAliases ?? []).Aggregate(Map<string, string>(), static (acc, alias) => acc.AddOrUpdate(alias.Lang ?? "", alias.Value ?? "")));
    }

    // A quantity row DOES carry a dimension — its QtoTypeEnum names an IFC measure type and that type publishes the
    // exponents — which is why the bSDD merge preserves the floor's SiDimension rather than taking a silent dictionary
    // None over it. A Count/Time quantity resolves no geometry-derivable dimension and drops.
    static Option<PropertyTemplate> QuantityTemplate(string setName, QtoDef quantity) =>
        QuantityDataType(quantity.QuantityType) is var dataType && DimensionOf(dataType).Case is Dimension dimension
            ? Some(new PropertyTemplate(
                setName, quantity.Name ?? "", dataType, BsddValueKind.Single, None,
                Seq<string>(), None, None, Seq<string>(), Some(dimension), None, Map<string, string>()))
            : None;

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
    // A SCOPE-PINNED dataset answers from its own schema (a COBie handover reads the IFC4 templatetype declarations it
    // actually ships); an unpinned Standard scope reads the IFC4x3 catalogue, which carries every template type the
    // earlier schemas declare and the ones they do not. The precedence is the SAME one For(ReleaseVersion, ...) applies
    // — scope pin first — so this entry needs no caller release to stay consistent with every other resolution.
    internal static Option<templatetype> TemplateTypeOf(string setName, TemplateScope scope) =>
        For(scope.Schema.IfNone(Version.IFC4x3), scope).Psets[setName] is { } set && set.templatetype is var t and not templatetype.NOTDEFINED
            ? Some(t) : None;

    // The class's base-quantity set name + its MethodOfMeasurement (the measurement basis the 5D estimate reads
    // beside the values) + each geometry-relevant QtoDef as its DECLARED NAME paired with its Dimension (mapped
    // from the QtoTypeEnum) — sourced from the catalogue rather than a hand-listed per-class table that slices it;
    // the predefined token scopes applicability the same way the Pset leg does. The MOST-SPECIFIC applicable set is
    // elected — a set whose ApplicableClass row declares the node's PredefinedType wins over a blank-scope row (the
    // declaration-order FirstOrDefault picked whichever the dataset listed first, so a predefined-scoped set never
    // reliably beat its general sibling). The names ride along so a derived quantity always keys by a member the
    // standard set declares, never a fabricated suffix.
    public static Option<(string Set, string Method, Seq<(string Name, Dimension Dimension)> Quantities)> BaseQuantitySet(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope) =>
        For(schema, scope).Qtos.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .OrderByDescending(set => ScopedMatch(set, cls, predefined))
            .FirstOrDefault() is { } qto
            ? Some((qto.Name, qto.MethodOfMeasurement ?? "", toSeq((qto.QuantityDefinitions ?? [])
                .Select(static q => DimensionOf(QuantityDataType(q.QuantityType)).Map(dimension => (Name: q.Name ?? "", Dimension: dimension))).Somes()
                .Where(static row => row.Name.Length > 0))))
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
        (toSeq(enumerated.EnumList?.Items ?? [])
            + toSeq(enumerated.ConstantList ?? []).Map(static c => c.Name ?? ""))
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

    // The ONE dimension authority for a DECLARED IFC data type: ids-lib publishes every measure's SI base-dimension
    // exponent vector, so a token resolves its seam Dimension by NAME through SchemaInfo.TryGetMeasureInformation
    // (which upper-cases its argument, so the match is case-insensitive by construction) and the seven integers build
    // the seam Dimension through its own generated factory — the SAME concept the bSDD wire columns build, so no
    // consumer projects an exponent vector twice. It REPLACES the five-row QtoTypeEnum table this owner hand-carried:
    // that table answered the four geometry quantities alone, so every Pset measure template resolved dimensionless
    // and TemplateVerdict.WrongDimension could never fire on a property — the audit's entire dimensional axis was dark
    // outside the Qto leg. A pure-number datatype (IfcLabel, IfcBoolean, IfcCountMeasure) is dimensionless and lowers
    // None, so the seam MeasureValue coercion never fires on a count or a label. The memo is keyed by token because
    // the resolution is a linear scan over the published measure set and a graph-wide audit resolves the same handful
    // of tokens once per element otherwise.
    static readonly ConcurrentDictionary<string, Option<Dimension>> Dimensions = new(StringComparer.OrdinalIgnoreCase);

    internal static Option<Dimension> DimensionOf(string dataType) =>
        dataType is { Length: > 0 }
            ? Dimensions.GetOrAdd(dataType, static token =>
                SchemaInfo.TryGetMeasureInformation(token, out IfcMeasureInformation? measure) && measure is { Exponents: { } e }
                && Dimension.Create(e.Length, e.Mass, e.Time, e.ElectricCurrent, e.Temperature, e.AmountOfSubstance, e.LuminousIntensity) is var dimension
                && dimension != Dimension.Dimensionless
                    ? Some(dimension)
                    : Option<Dimension>.None)
            : None;

    // The declared datatype AGREEMENT probe the Semantics/properties#TEMPLATE_AUDIT DataTypeMismatch verdict reads:
    // ids-lib resolves the template's token to its schema definition (an unparseable token is a template the audit
    // cannot grade, so it agrees), and a DIMENSIONED token demands the seam Measure case while an undimensioned one
    // refuses it. The value-type Kind cannot decide this — BsddValueKind.Single spans a label, a boolean, and a
    // thermal transmittance alike — so a Pset declaring IfcThermalTransmittanceMeasure and carrying the U-value as a
    // Text row passed every landed axis while being unreadable to every downstream measure consumer.
    internal static bool DataTypeAgrees(string dataType, PropertyValue value) =>
        !SchemaInfo.TryParseIfcDataType(dataType, out _)
        || DimensionOf(dataType).IsSome == (value is PropertyValue.Measure);
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

    public static InheritanceMode ModeOf(string setName, bool typeBound, TemplateScope scope) =>
        PropertyCatalog.TemplateTypeOf(setName, scope).Match(
            Some: FromTemplate,
            None: () => setName.StartsWith("Qto_", StringComparison.Ordinal) || typeBound
                ? InheritanceMode.TypeDrivenOverride
                : InheritanceMode.OccurrenceWins);
}
```

## [03]-[BASE_QUANTITIES]

- Owner: `QuantityDerivation` the base-quantity fold deriving the standard `Qto_*BaseQuantities` from `GeometryMeasures` — the kernel `Rasm` `Analysis/measure` aggregate metrology bundle `GeometryMeasures` — `Kind` plus `Option`-valued `Length`/`Area`/`Volume`/`Centroid`/`Radii`/`Inertia`/`InertiaProducts`/`PrincipalFrame`, minted by `GeometryMeasures.Of` off one leased mass handle — the kernel/Compute resolve from the geometry the seam `Object` node references by content key (`Model/elements#REPRESENTATION_KEYS` `RepresentationContentHash`) and supply to `Derive` (Bim consumes the measure, never tessellates it) — producing the seam `QuantitySet` node values as seam `MeasureValue` under derived-wins precedence. The class's base-quantity SET, its `MethodOfMeasurement` basis (the measurement-rule string the 5D estimate reads beside the values), and each declared quantity's NAME + `Dimension` come from `PropertyCatalog.BaseQuantitySet` (the offline `Xbim.Properties` `QtoSetDef` catalogue, predefined-scoped like the Pset leg), so the roster covers every class the standard defines, not a hand-listed slice — and every derived key names a quantity the standard set declares.
- Entry: `QuantityDerivation.Derive(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, GeometryMeasures measures, Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence, Op key)` derives the geometry-true base quantities for a class and merges them over the occurrence-stored quantities under derived-wins precedence (the geometry takeoff supersedes an authoring tool's stored quantity), returning the seam `QuantitySet` node value map; a class with no `Qto_*BaseQuantities` set in the catalogue returns the occurrence quantities unchanged so a non-takeoff class never blocks. `QuantityDerivation.Decompose(GeometryMeasures measures, Seq<BakedMaterial> materials, Func<ProfileRef, Option<SectionProperties>> sections, Op key)` is the MATERIAL-true takeoff the element-level fold cannot answer ("how much concrete is in this model") — the element volume split per `MaterialId` over the seam `MaterialComposition` the baked element's `Associate` edges bind (`element.Materials` + the `SectionOf` baked section are the caller's `Bake` reads): a `LayerSet` splits by thickness share, a `ConstituentSet` by declared `Fraction`, a `ProfileSet` folds PER COMPOUND ROW — each seam `MaterialProfile`'s own one-hop-resolved `SectionProperties.Area × Length` under its OWN `MaterialId`, re-stamped `QuantityType.Volume` through the band-preserving `WithType` (`Multiply` is dimension-anonymous by seam law), a row whose section does not resolve contributing no share, a `Single` carries the element volume whole; a colliding `MaterialId` sums through the seam `MeasureValue.Sum`, an absent element measure yields no row (never a fabricated zero), and the multi-ply WEIGHT decomposition stays the `Rasm.Compute` `AssemblyAggregator`'s — the frozen boundary: volume splits are composition-derivable in full, mass is not.
- Auto: `Derive` reads `PropertyCatalog.BaseQuantitySet(cls, predefined, schema, scope)` (the `Qto_*` set name + its `MethodOfMeasurement` + each geometry-relevant `QtoDef`'s declared NAME paired with its `Dimension`, the MOST-SPECIFIC applicable set elected — a `PredefinedType`-scoped row beats a blank-scope sibling, never dataset declaration order) and asks the ONE `Derivations` frozen table for each DECLARED MEMBER by `(Dimension, name)` — a member the table answers derives, a member it does not answer derives nothing and leaves the occurrence value standing, so the emitted key is a standard-set member BY CONSTRUCTION and an oriented takeoff the scalar bundle cannot separate (`GrossArea`, `NetSideArea`, `GrossVolume`) is never stamped from the one it can; the kernel scalar is already SI-base, so each derived value admits through the seam `MeasureValue.OfSi(QuantityType, Dimension, double)` carrying its QTO identity (a dimension-only admit stamps the dimension-anonymous type and strips the QTO read off every derived-wins row), the set name riding the `QuantitySet` bag node so a `{Set}.{name}`-prefixed non-member key whose derived-wins merge silently never collides is the deleted form, merged over the occurrence map with derived-wins so the 5D `Planning/cost#ESTIMATE` join reads the geometry-true measure (`Volume ≻ Area ≻ Length ≻ Mass`); `NetWeight = NetVolume × massDensity` through the seam `MeasureValue.Multiply` re-stamped by the band-preserving `WithType(QuantityType.Mass)` (`VolumeDim × DensityDim IS MassDim`, so the algebra proves the product and carries the density's `MeasureBand` forward), a non-density carrier or an absent density skipping the weight rows; an element-set aggregate of the same `Dimension` reduces through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum` reducer, never a manual `double` fold.
- Packages: Xbim.Properties, ids-lib, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new class's base-quantity set is already in the `Xbim.Properties` `QtoSetDef` catalogue (no edit); a newly derivable standard quantity is one `Derivations` row keyed `(Dimension, declared member name)` carrying its admitting projector, landing only where the kernel bundle honestly answers that member; a new decomposition modality is the seam `MaterialComposition` case arm the `Shares` generated total `Switch` breaks on at compile; the derived quantities merge over the occurrence map under one precedence rule; never a per-class `Derive` method, never a hand-listed per-class set table, and never a re-tessellation in this owner.
- Boundary: base-quantity derivation runs from the kernel `Analysis/measure` `GeometryMeasures` bundle (`Kind`-stamped `Option` scalars — this fold reads `Length`/`Area`/`Volume`; the moment fields serve the structural consumers) the kernel/Compute resolve from the `RepresentationContentHash` geometry and inject into `Derive`, so a Bim-local `GeometryMeasures` re-declaration or an in-owner geometry-measure computation is the deleted form (Bim depends UP on the kernel and never owns geometry measurement); a re-tessellation in this owner is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the derived value is a seam `MeasureValue` admitted through `MeasureValue.OfSi` under its QTO `QuantityType` (the seam owns the typed quantity over `Dimension` + UnitsNet), so a Bim-local `MeasureValue` re-declaration, a dimension-anonymous derived takeoff, a hand-stamped unit string drifting from the seam canonical `Dimension.SiSymbol`, and a bare-`double` product standing in for the seam `Multiply`/`WithType` algebra are the deleted forms; `NetWeight` is the homogeneous-element takeoff (`NetVolume × Mechanical.Density`, the material's `Composition/material#MATERIAL_PROPERTY` `Mechanical.Density` resolved One-Hop from the `Associate` material edge) and the multi-ply/layered weight is the `Rasm.Compute` `AssemblyAggregator`'s richer fold, never re-modeled here; the base-quantity SET and its declared quantity names/dimensions come from the `Xbim.Properties` `QtoSetDef` catalogue and a hand-listed per-class `BaseQuantityTable` that slices the standard is the deleted form; a derived value keys ONLY by a quantity name the standard set declares (the fold walks the declared members and looks each up) and a fabricated `{Set}.{suffix}` non-member key — one the `Review/validation#IDS_FACETS` Property facet and a downstream `Qto` reader never match — is the deleted form; the ONE `(Dimension, member)`-keyed `Derivations` frozen table owns the per-member election, and a dimension-keyed row that lands one measure on whichever member a set declared first is the deleted form that fabricated a gross takeoff from a net solid and a plan area from a surface area; the derived-wins precedence is applied once in `Derive`, never a per-call-site merge; the per-material `Decompose` reads the seam composition rows ONLY (thickness share, declared fraction, baked section area) — re-deriving a composition here, re-tessellating per material, or fabricating a mass split the composition cannot carry is the deleted form, the weight decomposition staying `Rasm.Compute`'s.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class QuantityDerivation {
    // ONE Dimension-keyed derivation table — the former Measure + NameOf parallel switches collapse here: each row the
    // canonical geometry-takeoff suffix + the projector from the kernel measures (+ the material mass density for the
    // weight derivation). The kernel GeometryMeasures scalars are already SI-base, so each row admits through
    // MeasureValue.OfSi(QuantityType, Dimension, double) carrying its QTO IDENTITY — the dimension-only OfSi overload
    // stamps the dimension-anonymous QuantityType, which made every derived takeoff untyped: a derived-wins merge then
    // REPLACED an occurrence-stored QuantityType.Volume with an anonymous one, voiding the As(QuantityType) read and
    // failing the Type-equality gate on any Sum against a stored quantity. NetWeight is the DIMENSIONED product —
    // MeasureValue.Multiply over the density carrier (VolumeDim x DensityDim IS MassDim) re-stamped through the
    // band-preserving WithType — so the seam algebra proves the product and propagates the density's MeasureBand,
    // where the bare `volume * density.Si` multiply it replaces discarded every declared uncertainty, the band-dropping
    // re-mint the seam owner names as its deleted form. The density admission guard stays on the INPUT (a non-density
    // carrier skips the weight rows) so the product's dimension is right by construction.
    static readonly FrozenDictionary<(Dimension Dimension, string Member), Func<GeometryMeasures, Option<MeasureValue>, Option<Fin<MeasureValue>>>> Derivations =
        new Dictionary<(Dimension, string), Func<GeometryMeasures, Option<MeasureValue>, Option<Fin<MeasureValue>>>> {
            [(Dimension.LengthDim, "Length")] = static (m, _) => m.Length.Map(static v => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, v)),
            // The bundle publishes ONE area — the realized solid's total surface area — so it answers the two standard
            // members that ASK for exactly that (Qto_BodyGeometryValidation SurfaceArea, and the TotalSurfaceArea the
            // element sets declare) and NO oriented member. GrossArea, NetArea, GrossSideArea, NetFootprintArea, and
            // CrossSectionArea are PROJECTIONS along an axis the bundle does not carry, and the retired
            // dimension-keyed table stamped the total surface area onto whichever of them a class happened to declare
            // first — a slab's Qto_SlabBaseQuantities GrossArea came back at roughly twice its plan area, an error no
            // consumer could distinguish from a correct takeoff.
            [(Dimension.AreaDim, "SurfaceArea")] = static (m, _) => m.Area.Map(static v => MeasureValue.OfSi(QuantityType.Area, Dimension.AreaDim, v)),
            [(Dimension.AreaDim, "TotalSurfaceArea")] = static (m, _) => m.Area.Map(static v => MeasureValue.OfSi(QuantityType.Area, Dimension.AreaDim, v)),
            // The realized body IS the net solid — its openings and voids are already resolved by the representation
            // the measures were taken from — so NetVolume and the validation set's bare Volume are the two members it
            // answers. GrossVolume is the pre-void magnitude the same geometry cannot separate, so it carries no row
            // and the occurrence's own stored GrossVolume survives the derived-wins merge intact.
            [(Dimension.VolumeDim, "Volume")] = static (m, _) => m.Volume.Map(static v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v)),
            [(Dimension.VolumeDim, "NetVolume")] = static (m, _) => m.Volume.Map(static v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v)),
            [(Dimension.MassDim, "NetWeight")] = static (m, density) =>
                from volume in m.Volume.Map(static v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v))
                from carrier in density.Filter(static d => d.Dimension == Dimension.DensityDim)
                select volume.Bind(admitted => admitted.Multiply(carrier)).Bind(static mass => mass.WithType(QuantityType.Mass)),
        }.ToFrozenDictionary();

    // Geometry-true base quantities (derived-wins) merged over the occurrence quantities. PropertyCatalog.BaseQuantitySet
    // (the offline Xbim.Properties Qto catalogue under the caller's TemplateScope) supplies the class's Qto set name, its
    // MethodOfMeasurement basis, and each geometry-relevant QtoDef's DECLARED NAME + Dimension — predefined-scoped
    // exactly like the Pset leg.
    // The fold walks the set's DECLARED MEMBERS and asks the table for each by NAME: a member the table answers derives
    // its geometry-true value under derived-wins, and a member it does not answer derives NOTHING, leaving whatever the
    // occurrence stored. That per-member keying is the whole election — the retired dimension-keyed table plus its
    // NameFor "first-declared quantity of this dimension" fallback stamped ONE takeoff onto whichever member a set
    // happened to list first, so a class declaring only GrossArea received the total surface area under that name and a
    // class declaring only GrossVolume received the net solid volume under that one. Both read as measured takeoffs and
    // neither was. The emitted key is a member of the standard set BY CONSTRUCTION, because the key IS the member the
    // catalogue declared; the QuantitySet bag node carries the set name, so a `{Set}.{name}`-prefixed PropertyName is a
    // NON-MEMBER key that never collides with the occurrence rows and never matches the IDS facet — the deleted form.
    // GeometryMeasures is the kernel Rasm value-object the kernel/Compute resolve from the Object
    // RepresentationContentHash geometry and inject (Bim never tessellates); massDensity is the element material's
    // Mechanical.Density (Composition/material#MATERIAL_PROPERTY) resolved One-Hop from the Associate material edge,
    // absent which the weight rows skip rather than fabricate.
    public static Fin<Map<PropertyName, MeasureValue>> Derive(
        IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, GeometryMeasures measures,
        Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence, Op key) =>
        PropertyCatalog.BaseQuantitySet(cls, predefined, schema, scope).Match(
            None: () => Fin.Succ(occurrence),
            Some: set => set.Quantities.Distinct().Fold(Fin.Succ(occurrence), (rail, member) =>
                rail.Bind(acc => Derivations.TryGetValue((member.Dimension, member.Name), out var project)
                    ? project(measures, massDensity).Match(
                        Some: derived => derived.Map(value => acc.AddOrUpdate(PropertyCategory.Seam.Row(member.Name), value)),
                        None: () => Fin.Succ(acc))
                    : Fin.Succ(acc))));

    // The material-true takeoff: the element vector split per MaterialId over the seam MaterialComposition the baked
    // element's Associate edges bind (element.Materials + element.Section are the caller's Bake reads) — the 5D/6D
    // per-material join key ("how much concrete is in this model"), which the element-level Derive cannot answer.
    // A colliding MaterialId (occurrence + type-inherited bindings naming one substance) sums through the seam
    // MeasureValue.Sum; an absent element measure yields no row, never a fabricated zero. Volume only: the multi-ply
    // WEIGHT fold stays the Rasm.Compute AssemblyAggregator's (the frozen boundary — volume splits are composition-
    // derivable in full, mass is not).
    public static Fin<Map<MaterialId, MeasureValue>> Decompose(
        GeometryMeasures measures, Seq<BakedMaterial> materials, Func<ProfileRef, Option<SectionProperties>> sections, Op key) =>
        materials.TraverseM(baked => Shares(measures, baked.Material.Composition, sections)).As()
            .Bind(rows => rows.Flatten().Fold(Fin.Succ(Map<MaterialId, MeasureValue>()), (rail, row) =>
                rail.Bind(acc => acc.Find(row.Material).Match(
                    Some: existing => MeasureValue.Sum(Seq(existing, row.Share), key).Map(sum => acc.SetItem(row.Material, sum)),
                    None: () => Fin.Succ(acc.Add(row.Material, row.Share))))));

    // One share fold per seam composition case — the generated TOTAL Switch, so a new composition modality breaks
    // this at compile time: Single carries the element volume whole, LayerSet splits by thickness share (total > 0
    // by the seam OfLayerSet admission), ConstituentSet by declared Fraction, and ProfileSet folds PER COMPOUND ROW —
    // each seam MaterialProfile's own section Area x the member Length under its OWN MaterialId, re-stamped
    // QuantityType.Volume through the band-preserving WithType (Multiply is dimension-anonymous by seam law — the
    // identity re-stamp is the consumer's).
    // The compound row fold is what makes the profile leg answer the question it exists for: a
    // Semantics/composition#MATERIAL_COMPOSITION ProfileSet carries EVERY IfcMaterialProfile row with its own material
    // and its own content-keyed section, so a steel-concrete composite beam or a plate girder splits its volume across
    // its plates. The retired single-section read multiplied ONE section area by the member length and attributed the
    // whole swept volume to the set's head material, which is exactly the takeoff a composite member is asked for and
    // exactly the one it got wrong.
    // `sections` is the caller's one-hop ProfileRef -> baked SectionProperties resolution (the Rasm.Materials catalog
    // lookup the seam deliberately does not store), so a row whose section does not resolve contributes NO share
    // rather than borrowing a sibling plate's area.
    static Fin<Seq<(MaterialId Material, MeasureValue Share)>> Shares(
        GeometryMeasures measures, MaterialComposition composition, Func<ProfileRef, Option<SectionProperties>> sections) =>
        composition.Switch(
            single: s => ElementVolume(measures).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Map(volume => Seq((s.Material, volume)))),
            layerSet: s => ElementVolume(measures).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Bind(volume => {
                    double total = s.Layers.Fold(0.0, static (acc, layer) => acc + layer.Thickness.Si);
                    return s.Layers.TraverseM(layer =>
                        volume.Scale(layer.Thickness.Si / total).Map(share => (layer.Material, share))).As();
                })),
            profileSet: s => measures.Length.Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: length => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, length)
                    .Bind(span => s.Profiles
                        .TraverseM(row => sections(row.Profile).Match(
                            None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                            Some: section =>
                                from swept in section.Area.Multiply(span)
                                from volume in swept.WithType(QuantityType.Volume)
                                select Seq((row.Material, volume))))
                        .As()
                        .Map(static rows => rows.Flatten()))),
            constituentSet: s => ElementVolume(measures).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Bind(volume => s.Constituents.TraverseM(constituent =>
                    volume.Scale(constituent.Fraction).Map(share => (constituent.Material, share))).As())));

    static Option<Fin<MeasureValue>> ElementVolume(GeometryMeasures measures) =>
        measures.Volume.Map(static v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v));
}
```

## [04]-[TEMPLATE_AUDIT]

- Owner: `TemplateAudit` the graph-wide standard-template conformance fold — the first model-quality question every project asks spec-free ("does each element carry its standard Pset with correctly-typed, in-range values") answered directly against the buildingSMART ground truth this page already resolves, with no authored IDS document; `TemplateVerdict` the `[SmartEnum<string>]` closed verdict vocabulary (`Missing`/`KindMismatch`/`DataTypeMismatch`/`NotAllowed`/`OutOfBounds`/`PatternReject`/`WrongDimension` — one row per constraint axis the `PropertyTemplate` carries, so a new template constraint axis is one verdict row plus one `Verdict` arm); `TemplateFinding` the typed per-element finding row a report renders and a fix pass keys on.
- Entry: `TemplateAudit.Run(ElementGraph graph, TemplateScope scope, Func<IfcClass, Option<BsddClass>> dictionary, Op key)` audits every entity-type-classified occurrence `Object` node (the `ClassificationSystem.IfcSystem` row key compared in the roster's own `OrdinalIgnoreCase` space, never a bare token literal) against its resolved templates — templates resolve ONCE per distinct `(Classification.Code, PredefinedType.Token)` pair through `PropertyKey.Resolve` (the catalogue floor ∪ live dictionary union, `graph.Header.Schema` the schema, the caller's `scope` the definition set — a `Handover` audit grades COBie completeness on the same fold that grades the standard sets, the injected `dictionary` the per-class live evidence a caller supplies or leaves `None` for the offline-only audit) and every element of that pair checks against the SAME resolved map, never a per-element re-resolution; `Fin<T>` carries only the seam `Bake` rail (an absent root or cyclic compose is the graph's fault, never this fold's) and the audit itself is total — a clean model returns the empty finding set.
- Auto: per element the merged `Bake`-derived `element.Properties`/`element.Quantities` bags (type→occurrence precedence already applied by the stamped `InheritanceMode`) probe each template row — an absent value on a `Required` template lands `Missing`; a present value decides per axis: a `Text`/`Enumerated` value outside a non-empty `AllowedValues` lands `NotAllowed`, a `Text` failing the whole-value-anchored `Pattern` lands `PatternReject`, a `Measure` whose `Dimension` disagrees with `SiDimension` lands `WrongDimension`, a `Measure`, `Integer`, or `Number` outside `Bounds` lands `OutOfBounds` (the bSDD `ClassPropertyContract.v1` min/max carry for Integer and Real properties, not only dimensioned measures), and a seam case irreconcilable with the template `Kind` (a `Complex` where the kind is `Single`) lands `KindMismatch` — the verdict axes are the SAME constraint family the `Review/validation#IDS_FACETS` facet narrows into its `ValueConstraint`, decided here with the failing AXIS named because a QA report acts per axis where a facet needs only pass/fail.
- Receipt: the `Seq<TemplateFinding>` is the baseline-tier evidence — composed WHOLE as the `Review/validation#MODEL_HEALTH` `ModelFinding.Baseline` case beneath the authored IDS audits, so `Rasm.AppUi` and the review pipeline read the ONE `ModelHealth` verdict surface, never this stream directly; each row carries the element `NodeId`, the `{Set}.{Code}` template coordinate, the verdict, and the actual value so a fix pass addresses the exact property.
- Packages: Xbim.Properties, ids-lib, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new constraint axis is one `TemplateVerdict` row plus one `Verdict` arm reading the new `PropertyTemplate` column, ordered most-specific first; a richer dictionary is the same injected `dictionary` resolver (zero fold edits); never a per-class audit method, never a second checker beside the IDS lane, and never a finding type per verdict.
- Boundary: the audit READS the resolved `PropertyTemplate` map and the baked element — it re-derives neither the template union (that is `PropertyKey.Resolve`'s) nor the type→occurrence merge (the seam `Bake`'s under the stamped mode [H1]); the verdict vocabulary mirrors the `Model/query#ELEMENT_SET` `ValueMatch` restriction family but stays a SEPARATE closed vocabulary because the finding names the failing axis where `ValueMatch` answers only membership — the IDS lane keeps `ValueMatch`, this lane keeps the axis-named verdict, and collapsing the two erases the axis evidence; the audit is spec-FREE (the buildingSMART templates ARE the spec) and a user-authored requirement routes the `Review/validation#IDS_FACETS` owner, never a template-audit extension; the finding stream surfaces only through the `Review/validation#MODEL_HEALTH` composition — a second report consumer forked off this stream is the deleted form; the `Pattern` compiles once per template row (`RegexOptions.NonBacktracking`, whole-value anchored — the untrusted-grammar law), never per element; the fold is SPAN-grade under [MODEL_SLOT_RULING] — one span over the whole graph pass carrying the package namespace slot, never a per-element or per-template instrument, because occurrences and resolved template rows are both unbounded in model size and a metric keyed on either multiplies every series by that count; the span itself is `Observability`'s to open around this entry and a telemetry mint inside this fold is the deleted form, the audit returning findings alone.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// One verdict row per PropertyTemplate constraint axis: the finding names WHICH axis failed (a QA report acts per
// axis), where the IDS ValueMatch family answers only membership — two consumers, two shapes, one template source.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TemplateVerdict {
    public static readonly TemplateVerdict Missing          = new("missing");
    public static readonly TemplateVerdict KindMismatch     = new("kind-mismatch");
    public static readonly TemplateVerdict DataTypeMismatch = new("data-type-mismatch");
    public static readonly TemplateVerdict NotAllowed       = new("not-allowed");
    public static readonly TemplateVerdict OutOfBounds      = new("out-of-bounds");
    public static readonly TemplateVerdict PatternReject    = new("pattern-reject");
    public static readonly TemplateVerdict WrongDimension   = new("wrong-dimension");
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TemplateFinding(NodeId Element, string Set, string Code, TemplateVerdict Verdict, Option<PropertyValue> Actual);

// --- [OPERATIONS] -------------------------------------------------------------------------
// The zero-configuration model-QA fold: templates resolve ONCE per distinct (class, predefined) pair and every
// element of the pair checks the SAME map; the per-template Pattern compiles once (NonBacktracking, whole-value
// anchored). The Bake rail is the only fault channel — the audit itself is total.
public static class TemplateAudit {
    public static Fin<Seq<TemplateFinding>> Run(ElementGraph graph, TemplateScope scope, Func<IfcClass, Option<BsddClass>> dictionary, Op key) {
        // The entity-type classification is the roster row's OWN key compared in the roster's OWN key space — a bare
        // token literal forks the system vocabulary and an ordinal compare reads a re-cased export as a foreign system.
        Seq<Node.Object> occurrences = graph.ObjectNodes
            .Filter(static o => o.Kind == ObjectKind.Occurrence
                && string.Equals(o.Classification.System, ClassificationSystem.IfcSystem.Key, StringComparison.OrdinalIgnoreCase))
            .ToSeq();
        Map<(string Code, string Token), Map<string, (PropertyTemplate Template, Option<Regex> Pattern)>> resolved =
            occurrences.Map(static o => (o.Classification.Code, o.PredefinedType.Token)).Distinct()
                .Fold(Map<(string, string), Map<string, (PropertyTemplate, Option<Regex>)>>(), (acc, pair) =>
                    IfcClass.TryGet(pair.Code).Match(
                        None: () => acc,
                        Some: cls => acc.Add(pair, PropertyKey.Resolve(cls, Token(pair.Token), graph.Header.Schema, scope, dictionary(cls))
                            .Map(static t => (t, t.Pattern.Map(static p => new Regex($"^(?:{p})$", RegexOptions.NonBacktracking | RegexOptions.CultureInvariant)))))));
        return occurrences
            .TraverseM(node => graph.Bake(node.Id, key).Map(element =>
                resolved.Find((node.Classification.Code, node.PredefinedType.Token))
                    .Map(templates => Check(node.Id, templates, element))
                    .IfNone(Seq<TemplateFinding>())))
            .As()
            .Map(static findings => findings.Flatten().ToSeq());
    }

    static Option<string> Token(string token) => Optional(token).Filter(static t => t.Length > 0 && t != PredefinedType.NotDefined.Token); // the seam roster row, never a literal twin of it

    // The bag probe keys through PropertyCategory.Seam.Row — the owner-blessed EMPTY-prefix category the round-tripped
    // IFC/bSDD template code lands under, so this reader shares one key space with every writer of the same rows
    // rather than spelling PropertyName.Create at a call site the seam declarer never sees.
    static Seq<TemplateFinding> Check(NodeId element, Map<string, (PropertyTemplate Template, Option<Regex> Pattern)> templates, Element baked) =>
        templates.Values.ToSeq().Bind(row => {
            PropertyName code = PropertyCategory.Seam.Row(row.Template.Code);
            Option<PropertyValue> actual = baked.Properties.Find(b => b.SetName == row.Template.Set)
                .Bind(bag => bag.Find(code))
                | baked.Quantities.Find(b => b.SetName == row.Template.Set)
                    .Bind(bag => bag.Find(code))
                    .Map(static measure => (PropertyValue)new PropertyValue.Measure(measure));
            return Verdict(row.Template, row.Pattern, actual)
                .Map(verdict => new TemplateFinding(element, row.Template.Set, row.Template.Code, verdict, actual)).ToSeq();
        });

    // One arm per constraint axis, most-specific first; a value passing every declared axis yields None — no finding.
    // Absence grades against a DECLARED requiredness alone: an undeclared axis (every offline-floor row, since the
    // buildingSMART dataset carries no requiredness column) yields no verdict, so a presence finding always traces to
    // a dictionary that actually stated the requirement rather than to a literal this fold supplied on its behalf.
    static Option<TemplateVerdict> Verdict(PropertyTemplate template, Option<Regex> pattern, Option<PropertyValue> actual) =>
        actual.Match(
            None: () => template.Required == Some(true) ? Some(TemplateVerdict.Missing) : None,
            Some: value => !Compatible(template.Kind, value) ? Some(TemplateVerdict.KindMismatch)
                // The declared IFC DATA TYPE is a second, finer axis than the value-type Kind: BsddValueKind.Single
                // spans IfcLabel, IfcBoolean, and IfcThermalTransmittanceMeasure alike, so a Pset that declares a
                // measure and carries a Text row passed every landed axis while being unreadable to every downstream
                // MeasureValue consumer. PropertyCatalog.DataTypeAgrees resolves the token through the ids-lib schema
                // and grades the seam case against it; an unparseable or unstated token grades nothing.
                : !PropertyCatalog.DataTypeAgrees(template.DataType, value) ? Some(TemplateVerdict.DataTypeMismatch)
                : value switch {
                PropertyValue.Text t when !template.AllowedValues.IsEmpty && !template.AllowedValues.Contains(t.Value) => Some(TemplateVerdict.NotAllowed),
                PropertyValue.Text t when pattern.Exists(p => !p.IsMatch(t.Value)) => Some(TemplateVerdict.PatternReject),
                PropertyValue.Enumerated e when !template.AllowedValues.IsEmpty && e.Selected.Exists(s => !template.AllowedValues.Contains(s.Render())) => Some(TemplateVerdict.NotAllowed),
                PropertyValue.Measure m when template.SiDimension.Exists(d => d != m.Value.Dimension) => Some(TemplateVerdict.WrongDimension),
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

(none)
