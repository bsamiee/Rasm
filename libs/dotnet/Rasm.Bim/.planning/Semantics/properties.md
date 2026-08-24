# [BIM_PROPERTY_TEMPLATES]

The IFC Pset/Qto TEMPLATE authority over the `Rasm.Element` seam graph: the offline `Xbim.Properties` `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` buildingSMART catalogue is the canonical, schema-versioned, scope-selected, network-free template floor — every standard `Pset_*` and `Qto_*`, its `ApplicableClasses` (entity AND `PredefinedType` scope), each property's IFC `DataType`/value-type kind AND its full value constraint (the enumerated kind's `EnumList`/`ConstantList` allowed values, the bounded kind's `ValueRangeDef` range, the declared `UnitType`, the localized `NameAliases`), and each base quantity's `QtoTypeEnum` + `MethodOfMeasurement` — and the live `Semantics/classification#BSDD_RESOLUTION` `BsddClass` dictionary enriches it (dictionary-wins on a name collision), its rows carrying the class-scoped `allowedValues`/`pattern`/bounds/`units`/SI base-dimension constraint surface. `PropertyKey.Resolve` unions the two into one FULL-CONSTRAINT `PropertyTemplate` map scoped by the node's `PredefinedType` token, so the `Review/validation#IDS_FACETS` Property facet validates VALUE constraints, never type alone; `PropertyInheritance.ModeOf` reads the authoritative IFC `templatetype` the catalogue declares to stamp each seam `PropertySet`/`QuantitySet` node with its `InheritanceMode` at ingest so the seam `Bake` applies the correct type→occurrence precedence over the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge a `Component` Type `Object` binds — the `typeBound` bag the classifier reads is the set the projector resolved from an `IfcTypeProduct`/`IfcElementType` (the seam `ObjectKind.Type` node the `Rasm.Materials` `ComponentProjector` mints), so a wall-type's shared `Pset_WallCommon` rides the type bag and the occurrence's overriding values the occurrence bag, the two merged once in `Bake`; `QuantityDerivation.Derive` sources the class's base-quantity set from the catalogue and folds the geometry-true takeoff from the kernel geometry the node references by content key. The typed VALUE half is seam-owned: the closed `PropertyValue` family (`Text`/`Measure`/`Boolean`/`Logical`/`Integer`/`Number`/`Binary`/`Temporal`/`Enumerated`/`Reference`/`Bounded`/`List`/`Table`/`Complex` — `Logical` the three-valued `IfcLogical`/`IfcLogicalEnum` `UNKNOWN`, `Complex` the named nested `IfcComplexProperty` bag) and the `MeasureValue` (`QuantityType`/`Dimension`/`Si`/`CanonicalUnit` over the `Dimension` `[ComplexValueObject]` + the `QuantityType` `[ValueObject<string>]` discriminator) live on `Rasm.Element/Properties` — this page owns the TEMPLATE (which properties a class carries and their declared types) and the PRECEDENCE policy, never the value. A property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, never a `(SetName, Name, string Value)` triple. The page is the template oracle the `Projection/semantic#SEMANTIC_PROJECTOR` projector and the `Review/validation#IDS_FACETS` Property facet read, and `TemplateAudit.Run` turns the same resolved templates into the zero-configuration graph-wide model-QA fold — presence, declared type, allowed values, bounds, pattern, and dimension audited per element with no authored IDS — the BASELINE tier of the `Review/validation#MODEL_HEALTH` two-tier verdict surface, this page producing the stream and that owner composing it beneath the authored-IDS lane; a hand-coded `Pset_*` property table beside the `Xbim.Properties` catalogue is the deleted form.

## [01]-[INDEX]

- [02]-[PROPERTY_TEMPLATES]: `PropertyCatalog` the `Xbim.Properties` offline standard-template catalogue (loaded once per `(schema, scope)` pair), `TemplateScope` the `[SmartEnum<string>]` definition-set policy carrying each scope's two `Definitions<T>` loaders and its pinned schema, `PropertyTemplate` the unified resolved full-constraint template both sources lower into, `PropertyKey` the domain-partitioned, schema-windowed curated `Pset_*` recognition roster over two stem mints, `PropertyKey.Resolve` the predefined-scoped catalogue-floor ∪ bSDD-live template union, and `PropertyInheritance.ModeOf` the `templatetype`-driven `InheritanceMode` classifier over the closed `TypeBinding` origin row, stamped on each seam bag node at ingest.
- [03]-[BASE_QUANTITIES]: `PropertyCatalog.BaseQuantitySet` the per-`IfcClass` `Qto_*BaseQuantities` set + its `MethodOfMeasurement` basis (empty on every bundled set — advisory, never a keyed read) + each geometry-relevant `QtoDef`'s declared name + `Dimension` (from the catalogue, never a hand-listed slice), `QuantityDerivation.Derive` the base-quantity fold deriving the geometry-true takeoff (incl. `NetWeight` from volume × material density) from the kernel `MeasureBundle` minted under `QuantityDerivation.Demand`, keyed by declared set members only, producing the seam `QuantitySet` node values under derived-wins precedence, and `QuantityDerivation.Decompose` the material-true takeoff — the element volume split per `MaterialId` over the seam `MaterialComposition` (layer thickness share, constituent `Fraction`, per-compound-row section-area × length), the per-material join key every 5D/6D consumer reads.
- [04]-[TEMPLATE_AUDIT]: `TemplateAudit.Run` the zero-configuration model-QA fold auditing a whole seam `ElementGraph` against the resolved buildingSMART templates themselves — per-`(class, predefined)` template resolution through `PropertyKey.Resolve`, per-element presence/kind/allowed-value/bounds/pattern/dimension verdicts as typed `TemplateFinding` rows over the `TemplateVerdict` vocabulary — the baseline tier the `Review/validation#MODEL_HEALTH` owner composes; the authored-requirement lane stays `Review/validation#IDS_FACETS`.

## [02]-[PROPERTY_TEMPLATES]

- Owner: `PropertyCatalog` the offline `Xbim.Properties` template catalogue — `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` loaded once per `(IFC Version, TemplateScope)` pair and cached, the always-available buildingSMART template floor declaring what every `Pset_*`/`Qto_*` IS (its `ApplicableClasses` with entity + `PredefinedType` scope, its `PropertyDef`s with their `DataType`/value-type kind + allowed values + range + unit + aliases, its `QtoDef`s with their `QtoTypeEnum` under the set's `MethodOfMeasurement`); `PropertyTemplate` the unified resolved FULL-CONSTRAINT template (`Set`/`Code`/`DataType`/`Kind`/`Required` + `AllowedValues`/`Bounds`/`Pattern`/`Units`/`SiDimension`/`Predefined`/`Aliases`, its `SiDimension` the seam `Dimension` itself rather than a second exponent carrier every consumer re-projects) both the catalogue `PropertyDef` and the bSDD `BsddProperty` lower into; `PropertyKey` the curated well-known `Pset_*` recognition roster (the opinionated common set name + its `IfcDomain` discipline + its `SchemaSpan` recognition window, stem-minted `Common`/`TypeCommon` spellings spanning the architectural, structural/foundation, complete bundled MEP flow-device, electrical, plumbing/fire, controls, circulation, envelope, and spatial families) authoring surfaces first; `TemplateScope` the closed definition-set policy value (`Standard` the bundled buildingSMART sets, `Cobie` the COBie handover superset, `Handover` both) each row carrying its `Definitions<PropertySetDef>`/`Definitions<QtoSetDef>` loader pair and the schema its dataset pins; `PropertyInheritance` the classifier reading the catalogue's authoritative IFC `templatetype` onto the seam `InheritanceMode`, its fallback the `TypeBinding` row's own declared inference; every nullable column the `Xbim.Properties` surface publishes admits ONCE at `Token`/`Rows`, the string half riding the folder's ONE `Projection/value#PROPERTY_LOWERING` `PropertyLowering.Stated` entry. The typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned (`Rasm.Element/Properties`); this page supplies the TEMPLATE (which properties, their declared `DataType`) the seam value is constructed against, never the value.
- Entry: `PropertyKey.Resolve(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, Option<BsddClass> dictionary)` resolves a class's property templates — `PropertyCatalog.Templates(cls, predefined, schema, scope)` (the offline `Xbim.Properties` floor under the scope's own definition set, every `PropertySetDef` whose `ApplicableClasses` names the class AND whose `PredefinedType` scope, when declared, matches the node's token — a `ClassName`-only match over-applies a predefined-scoped Pset to its whole class) unioned UNDER the live `BsddClass.Properties` dictionary rows (dictionary-wins on a `{Set}.{Code}` collision), so a measured property carries its declared IFC `DataType` AND its value constraint, and the offline catalogue resolves when bSDD is unreachable; `PropertyCatalog.TemplateTypeOf(string setName, TemplateScope scope)` is the `internal` catalogue query returning the raw IFC `templatetype` a set declares (the `Xbim.Properties` enum kept Bim-internal, never a public seam return); `PropertyInheritance.ModeOf(string setName, TypeBinding binding, TemplateScope scope)` is the public canonical surface returning the seam `InheritanceMode` the projector stamps on a bag at ingest — `TypeBinding` the closed row family naming WHICH seam node the projector resolved the bag from and carrying the structural inference as its own column, never a boolean the classifier re-interprets per call; `PropertyKey.Unbacked(schema, scope)` is the roster-mirror invariant naming every curated anchor the scope's dataset does not declare at a schema the row's own `Span` admits; `Fin<T>` is not the rail here — resolution degrades to the offline catalogue (and to the structural inference) when the dictionary is unreachable, never faulting ingest.
- Auto: `Resolve` folds the bSDD `BsddProperty` rows (lowered to the full-constraint `PropertyTemplate` — `DataType`/`ValueKind`/`Traits` with `AllowedValues` value strings, `Bounds`, `Pattern`, `Units`, the seam `SiDimension`, and the class-fixed `PredefinedValue`) OVER the `PropertyCatalog.Templates` floor with the two-arm `AddOrUpdate` so a dictionary-declared property overrides the offline default and a bSDD-only property still resolves — dictionary-wins-when-SPEAKING: a declared narrowing (`AllowedValues`/`Bounds`/`Pattern`/`Units`) overrides its floor axis, a SILENT axis keeps the floor's constraint, and the localized `Aliases` only the catalogue carries always survive, so a terse dictionary row never erases a floor constraint it merely failed to restate; `PropertyCatalog.Templates` reads `definitions.DefinitionSets`, keeps the sets whose `ApplicableClasses` match entity + `PredefinedType` scope, and lowers each `PropertyDef` through its `PropertyType.PropertyValueType` value-type kind in ONE pass (`LowerValue`): the scalar IFC data-type token off `TypePropertySingleValue`/`TypePropertyBoundedValue`/`TypePropertyReferenceValue` `DataType.Type` (a `DataTypeEnum`) and `TypeSimpleProperty.DataType.Type`, the same kind selecting the `BsddValueKind`, the enumerated kind yielding its `EnumList.Items`+`ConstantList` allowed values, the bounded kind its inclusive `ValueRangeDef` range, the single/bounded/simple/list kinds their declared `UnitType` token (the composite kinds carry no scalar token), and `NameAliases` folding onto the per-language display map — so the projector and the IDS facet know each property's expected type AND legal values without re-deriving either; `ModeOf` reads `PropertyCatalog.TemplateTypeOf` (the `templatetype` enum the `PropertySetDef` declares) — `PSET_TYPEDRIVENONLY`/`QTO_TYPEDRIVENONLY` is `TypeDrivenOnly`, `PSET_TYPEDRIVENOVERRIDE`/`QTO_TYPEDRIVENOVERRIDE` is `TypeDrivenOverride`, every other declared kind (`PSET_OCCURRENCEDRIVEN`/`PSET_PERFORMANCEDRIVEN`/`PSET_PROFILEDRIVEN`/`PSET_MATERIALDRIVEN`) is `OccurrenceWins`, and `NOTDEFINED` resolves None — falling back to the structural inference (a `Qto_*` quantity set, whose `QtoSetDef` carries no `templatetype`, is `TypeDrivenOverride` by the set's own property; every other undeclared set reads its `TypeBinding` row's own `Inferred` column) when no catalogue template type is declared, so the seam `Bake` applies the IFC inheritance once per bag rather than a per-call-site merge — and the bundled datasets split hard on that axis: the IFC4 dataset declares `NOTDEFINED` on every set it ships and ONLY the IFC4x3 dataset declares real template types (`TYPEDRIVENOVERRIDE` the dominant kind beside `OCCURRENCEDRIVEN`/`PERFORMANCEDRIVEN`/`MATERIALDRIVEN`/`PROFILEDRIVEN`), so an IFC4-pinned `Cobie`/`Handover` resolution always answers `None` and rides the structural inference, the unpinned `Standard` scope's IFC4x3 read the only declared-mode path.
- Receipt: the resolved `PropertyTemplate` map is the EXPECTED-type AND VALUE-CONSTRAINT evidence the `Review/validation#IDS_FACETS` Property facet validates the seam `PropertyValue` against (`AllowedValues`/`Bounds`/`Pattern` narrow into the facet's `ValueConstraint`; `SiDimension` corroborates a measured value's `Dimension`) and the from-scratch authoring path constructs a typed value from; at IFC import the typing is the `Projection/value#PROPERTY_LOWERING` `PropertyLowering.Lower` narrowing the live `IfcValue` runtime type onto the seam `PropertyValue` case directly (the catalogue/bSDD `DataType` is the expected type, never a `PropertyValue.Of(value, dataType)` the seam does not own); the stamped `InheritanceMode` is the precedence evidence the seam `Bake` reads when folding the `Graph/element#ELEMENT_GRAPH` `Assign.TypeDefinition` edge (the neutral seam lowering of the IFC `IfcRelDefinesByType` the projector authored) into the occurrence — the `Component` Type bag's values merging into the occurrence by the stamped mode.
- Packages: Xbim.Properties, ids-lib, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new standard Pset is already in the `Xbim.Properties` catalogue (no edit) or one live bSDD dictionary row; a new bag-binding origin is one `TypeBinding` row carrying its own inference, with no `ModeOf` body edit; a new curated recognition anchor is one `PropertyKey` mint row, its window narrowed only where the dataset constrains, and a renamed standard set is a closed window and its successor row; a new bundled definition set is one `TemplateScope` row carrying its loader pair and schema pin, with no memo-key or call-site edit; a new IFC value-type kind is one `BsddValueKind`-mapping arm; a new constraint axis is one `PropertyTemplate` column BOTH lowerings fill; a new inheritance policy is the catalogue `templatetype` the dictionary declares; never a hand-coded `Pset_*` property table, never a per-Pset type, and never a second property store.
- Boundary: there is ONE property model and it is the seam graph — a property is a seam `PropertyValue` keyed by `PropertyName` in a `PropertySet` bag node, and a per-Pset `WallProperties`/`SlabProperties` class family is the deleted form; the typed `PropertyValue`/`MeasureValue`/`Dimension` value family is seam-owned and re-declaring it here is the deleted form — this page owns the TEMPLATE (`DataType`/value-type kind) and the PRECEDENCE policy, the seam owns the value; the offline standard template is the `Xbim.Properties` `Definitions<T>` catalogue read as the canonical floor and a hand-coded `Pset_*` property table beside it is the deleted form; the `PropertyKey` roster names sets and windows recognition only — a renamed standard set carries TWO windowed rows (`Pset_ElectricDistributionBoardTypeCommon` closing at `Ifc4X3`, its successor `Pset_DistributionBoardTypeCommon` opening there — the `IfcClass` retirement idiom), and an unwindowed anchor offering a dead name to a schema-scoped authoring surface is the deleted form; the definition-set choice is the `TemplateScope` policy value the memo key carries, so a `LoadAllDefault`/`LoadIFC4COBie`/`LoadIFC4AndCOBie` selector at a call site or a second cache keyed by `Version` alone (whose second load evicts the first scope's dataset) is the deleted form, and a COBie-scoped resolution reads its row's pinned `IFC4` dataset whatever the model's `ReleaseVersion` rather than claiming a schema its definitions do not ship; applicability matches BOTH `ApplicableClass.ClassName` and its `PredefinedType` scope; the constraint surface lowers from the CURRENT value-type kinds alone (`TypePropertyEnumeratedValue.EnumList`/`ConstantList`, `TypePropertyBoundedValue.ValueRangeDef`, the `UnitType` axis), so a set whose only min/max/default lived on the retired slot resolves an ABSENT constraint and a suppression-scoped read of a retired member is the deleted form; the live bSDD dictionary unions OVER the catalogue with dictionary-wins, never the SOLE source and never a fault on a service miss; the type-vs-occurrence precedence is the IFC `IfcPropertySetTemplate.templatetype` the catalogue declares, lowered to the seam `InheritanceMode` at ingest and applied once in the seam `Bake`, never a per-call-site merge, never a stored-twice type→occurrence fold, and never a fragile set-name suffix heuristic; the classifier's binding input is the closed `TypeBinding` row family — a `bool typeBound` parameter is the deleted form, because a boolean forced every call site to re-decide what its two states meant and left the structural inference spelled beside the flag instead of on the row; the `PropertyKey` roster carries no per-row provenance column, every row sharing ONE upstream (the bundled definition sets under the caller's scope) and a live bSDD anchor never entering the roster at all — the mirror proves itself through `Unbacked` instead, and an asserted per-row dictionary name nothing reads is the decorative form; every nullable column the `Xbim.Properties` surface publishes admits ONCE at `Token`/`Rows` and a `?? ""`/`?? []` beside them is the deleted duplicate, a second string-admission owner in this folder the named twin; `Xbim.Properties` is a TEMPLATE source only (no IFC entity graph, no property values, no IDS engine) and consuming it as a model reader or value store is the rejected form; every bag key this page writes or reads mints through the owner-blessed `PropertyCategory.Seam.Row` EMPTY-prefix category (a round-tripped IFC/bSDD code stays bare) and a call-site `PropertyName.Create` in the derivation writer or the audit reader is the key-space fork the branch row-name custody ruling deletes; requiredness rides the classification-owned `CapabilitySet<TemplateTrait>` and a `bool`/`Option<bool>` column beside it is the deleted form — the offline dataset never states the axis, so a floor row lands the EMPTY set, only a dictionary that answered holds `Declared`, and a `Missing` verdict traces to a stated requirement rather than to a `false` this page supplied on the catalogue's behalf; `SiDimension` merges under the same dictionary-wins-when-SPEAKING law as every other narrowing, an unconditional dictionary take stripping the quantity floor's own `QtoTypeEnum`-derived dimension and darkening `TemplateVerdict.WrongDimension`.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using IdsLib.IfcSchema;                        // SchemaInfo — the ids-lib measure/datatype authority every declared IFC data type resolves its SI exponents through
using LanguageExt;
using Rasm.Analysis;                           // MassKind, MeasureBundle — the kernel kind-keyed multi-domain takeoff carrier this fold reads
using Rasm.Bim;
using Rasm.Bim.Projection;
using Rasm.Domain;
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
// The two loader columns are the C#-forced shape: Definitions<PropertySetDef> and Definitions<QtoSetDef> are distinct
// closed generics over one instance method and no delegate spans both, so the pair is one row's data rather than a
// switch at For. LoadIFC4COBie and LoadIFC4AndCOBie carry IFC4 definitions alone, which is what Schema pins; Standard
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
// Traits carries the requiredness axis as the classification-owned CapabilitySet<TemplateTrait>, so the offline
// buildingSMART floor — whose PropertyDef and QuantityPropertyDef base publish Name/Definition/aliases/PropertyType
// and nothing more — lands the EMPTY set and only a dictionary that answered holds Declared. A bare `false` asserted
// "optional" on the floor's behalf and a presence audit then read every unstated property as satisfied. Bounds reuses
// the classification-owned BsddBounds carrier and SiDimension IS the seam Dimension both sources build directly, so
// no consumer re-projects an exponent vector.
public readonly record struct PropertyTemplate(
    string Set, string Code, string DataType, BsddValueKind Kind, CapabilitySet<TemplateTrait> Traits,
    Seq<string> AllowedValues, Option<BsddBounds> Bounds, Option<string> Pattern,
    Seq<string> Units, Option<Dimension> SiDimension, Option<string> Predefined,
    Map<string, string> Aliases) {
    // A dataset states at most one unit token where a bSDD row may state several spellings of one unit, so Units is
    // the vocabulary and Unit the single declared token a renderer reads; the seam Dimension.SiSymbol is the canonical
    // emit unit when neither source declared one.
    public Option<string> Unit => Units.Head;
}

// A RECOGNITION roster, never the authoritative catalogue: it names the common sets and fabricates no property. Two
// mints own the standard spellings, so the applicable entity stays recoverable as Ifc{stem} on every entity-scoped
// row; the material-scoped and non-conforming spellings state their own literals, and ReinforcingBarBendings mints by
// name alone with no Ifc{stem} entity behind it. Domain is the curated authoring discipline, not a re-derivation of
// the IfcClass census — the spatial rows read Architecture where the census seats IfcSite/IfcSpace on the General
// backbone, and the vent StackTerminal reads Plumbing where the census leaves it on the HvacFire root. Span narrows
// ONLY where the bundled dataset proves a constraint, because an unwindowed roster offers a dead name to every
// schema-scoped authoring surface.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PropertyKey {
    // ONE mint per spelling family: the stem is the whole literal, the window defaulting to the open floor.
    private static PropertyKey Common(string stem, IfcDomain domain, Option<SchemaSpan> span = default) =>
        new($"Pset_{stem}Common", domain, span.IfNone(IfcSchema.Ifc2X3));
    private static PropertyKey TypeCommon(string stem, IfcDomain domain, Option<SchemaSpan> span = default) =>
        new($"Pset_{stem}TypeCommon", domain, span.IfNone(IfcSchema.Ifc2X3));

    // [ARCHITECTURE] — built cores, envelope, circulation, and the spatial anchors.
    public static readonly PropertyKey WallCommon = Common("Wall", IfcDomain.Architecture);
    public static readonly PropertyKey SlabCommon = Common("Slab", IfcDomain.Architecture);
    public static readonly PropertyKey BeamCommon = Common("Beam", IfcDomain.Architecture);
    public static readonly PropertyKey ColumnCommon = Common("Column", IfcDomain.Architecture);
    public static readonly PropertyKey DoorCommon = Common("Door", IfcDomain.Architecture);
    public static readonly PropertyKey WindowCommon = Common("Window", IfcDomain.Architecture);
    public static readonly PropertyKey RoofCommon = Common("Roof", IfcDomain.Architecture);
    public static readonly PropertyKey CurtainWallCommon = Common("CurtainWall", IfcDomain.Architecture);
    public static readonly PropertyKey CoveringCommon = Common("Covering", IfcDomain.Architecture);
    public static readonly PropertyKey PlateCommon = Common("Plate", IfcDomain.Architecture);
    public static readonly PropertyKey MemberCommon = Common("Member", IfcDomain.Architecture);
    public static readonly PropertyKey ShadingDeviceCommon = Common("ShadingDevice", IfcDomain.Architecture, IfcSchema.Ifc4);
    public static readonly PropertyKey StairCommon = Common("Stair", IfcDomain.Architecture);
    public static readonly PropertyKey StairFlightCommon = Common("StairFlight", IfcDomain.Architecture);
    public static readonly PropertyKey RampCommon = Common("Ramp", IfcDomain.Architecture);
    public static readonly PropertyKey RailingCommon = Common("Railing", IfcDomain.Architecture);
    public static readonly PropertyKey SpaceCommon = Common("Space", IfcDomain.Architecture);
    public static readonly PropertyKey BuildingCommon = Common("Building", IfcDomain.Architecture);
    public static readonly PropertyKey SiteCommon = Common("Site", IfcDomain.Architecture);
    public static readonly PropertyKey BuildingStoreyCommon = Common("BuildingStorey", IfcDomain.Architecture);

    // [STRUCTURAL] — concrete/steel/masonry material sets, reinforcing, and the foundation pair.
    public static readonly PropertyKey ConcreteElementGeneral = new("Pset_ConcreteElementGeneral", IfcDomain.Structural, IfcSchema.Ifc2X3);
    public static readonly PropertyKey MaterialSteel = new("Pset_MaterialSteel", IfcDomain.Structural, IfcSchema.Ifc2X3);
    public static readonly PropertyKey MaterialMasonry = new("Pset_MaterialMasonry", IfcDomain.Structural, IfcSchema.Ifc2X3);
    public static readonly PropertyKey ReinforcingBarBendingsCommon = Common("ReinforcingBarBendings", IfcDomain.Structural);
    public static readonly PropertyKey FootingCommon = Common("Footing", IfcDomain.Structural, IfcSchema.Ifc4);
    public static readonly PropertyKey PileCommon = Common("Pile", IfcDomain.Structural, IfcSchema.Ifc4);

    // [HVAC_FIRE] — the complete bundled flow-device census on the distribution root, fire suppression included.
    public static readonly PropertyKey AirTerminalTypeCommon = TypeCommon("AirTerminal", IfcDomain.HvacFire);
    public static readonly PropertyKey AirTerminalBoxTypeCommon = TypeCommon("AirTerminalBox", IfcDomain.HvacFire);
    public static readonly PropertyKey AirToAirHeatRecoveryTypeCommon = TypeCommon("AirToAirHeatRecovery", IfcDomain.HvacFire);
    public static readonly PropertyKey BoilerTypeCommon = TypeCommon("Boiler", IfcDomain.HvacFire);
    public static readonly PropertyKey BurnerTypeCommon = TypeCommon("Burner", IfcDomain.HvacFire, IfcSchema.Ifc4);
    public static readonly PropertyKey ChillerTypeCommon = TypeCommon("Chiller", IfcDomain.HvacFire);
    public static readonly PropertyKey CoilTypeCommon = TypeCommon("Coil", IfcDomain.HvacFire);
    public static readonly PropertyKey CompressorTypeCommon = TypeCommon("Compressor", IfcDomain.HvacFire);
    public static readonly PropertyKey CondenserTypeCommon = TypeCommon("Condenser", IfcDomain.HvacFire);
    public static readonly PropertyKey CooledBeamTypeCommon = TypeCommon("CooledBeam", IfcDomain.HvacFire);
    public static readonly PropertyKey CoolingTowerTypeCommon = TypeCommon("CoolingTower", IfcDomain.HvacFire);
    public static readonly PropertyKey DamperTypeCommon = TypeCommon("Damper", IfcDomain.HvacFire);
    public static readonly PropertyKey DuctFittingTypeCommon = TypeCommon("DuctFitting", IfcDomain.HvacFire);
    public static readonly PropertyKey DuctSegmentTypeCommon = TypeCommon("DuctSegment", IfcDomain.HvacFire);
    public static readonly PropertyKey DuctSilencerTypeCommon = TypeCommon("DuctSilencer", IfcDomain.HvacFire);
    public static readonly PropertyKey EngineTypeCommon = TypeCommon("Engine", IfcDomain.HvacFire, IfcSchema.Ifc4);
    public static readonly PropertyKey EvaporativeCoolerTypeCommon = TypeCommon("EvaporativeCooler", IfcDomain.HvacFire);
    public static readonly PropertyKey EvaporatorTypeCommon = TypeCommon("Evaporator", IfcDomain.HvacFire);
    public static readonly PropertyKey FanTypeCommon = TypeCommon("Fan", IfcDomain.HvacFire);
    public static readonly PropertyKey FilterTypeCommon = TypeCommon("Filter", IfcDomain.HvacFire);
    public static readonly PropertyKey FireSuppressionTerminalTypeCommon = TypeCommon("FireSuppressionTerminal", IfcDomain.HvacFire, IfcSchema.Ifc4);
    public static readonly PropertyKey HeatExchangerTypeCommon = TypeCommon("HeatExchanger", IfcDomain.HvacFire);
    public static readonly PropertyKey HumidifierTypeCommon = TypeCommon("Humidifier", IfcDomain.HvacFire);
    public static readonly PropertyKey MedicalDeviceTypeCommon = TypeCommon("MedicalDevice", IfcDomain.HvacFire, IfcSchema.Ifc4);
    public static readonly PropertyKey SpaceHeaterTypeCommon = TypeCommon("SpaceHeater", IfcDomain.HvacFire);
    public static readonly PropertyKey TubeBundleTypeCommon = TypeCommon("TubeBundle", IfcDomain.HvacFire);
    public static readonly PropertyKey UnitaryEquipmentTypeCommon = TypeCommon("UnitaryEquipment", IfcDomain.HvacFire, IfcSchema.Ifc4);

    // [PLUMBING] — piped distribution, storage, metering, and the terminal family.
    public static readonly PropertyKey PipeFittingTypeCommon = TypeCommon("PipeFitting", IfcDomain.Plumbing);
    public static readonly PropertyKey PipeSegmentTypeCommon = TypeCommon("PipeSegment", IfcDomain.Plumbing);
    public static readonly PropertyKey PumpTypeCommon = TypeCommon("Pump", IfcDomain.Plumbing);
    public static readonly PropertyKey ValveTypeCommon = TypeCommon("Valve", IfcDomain.Plumbing);
    public static readonly PropertyKey TankTypeCommon = TypeCommon("Tank", IfcDomain.Plumbing);
    public static readonly PropertyKey FlowMeterTypeCommon = TypeCommon("FlowMeter", IfcDomain.Plumbing);
    public static readonly PropertyKey InterceptorTypeCommon = TypeCommon("Interceptor", IfcDomain.Plumbing, IfcSchema.Ifc4);
    public static readonly PropertyKey SanitaryTerminalTypeCommon = TypeCommon("SanitaryTerminal", IfcDomain.Plumbing, IfcSchema.Ifc4);
    public static readonly PropertyKey StackTerminalTypeCommon = TypeCommon("StackTerminal", IfcDomain.Plumbing, IfcSchema.Ifc4);
    public static readonly PropertyKey WasteTerminalTypeCommon = TypeCommon("WasteTerminal", IfcDomain.Plumbing, IfcSchema.Ifc4);

    // [ELECTRICAL] — fixtures, machines, protection, containment, and the one renamed standard set: the IFC4 name
    // closes at Ifc4X3, where the renamed entity's successor set opens (the IfcClass retirement idiom).
    public static readonly PropertyKey LightFixtureTypeCommon = TypeCommon("LightFixture", IfcDomain.Electrical);
    public static readonly PropertyKey LampTypeCommon = TypeCommon("Lamp", IfcDomain.Electrical);
    public static readonly PropertyKey OutletTypeCommon = TypeCommon("Outlet", IfcDomain.Electrical);
    public static readonly PropertyKey SwitchingDeviceTypeCommon = TypeCommon("SwitchingDevice", IfcDomain.Electrical);
    public static readonly PropertyKey TransformerTypeCommon = TypeCommon("Transformer", IfcDomain.Electrical);
    public static readonly PropertyKey ElectricGeneratorTypeCommon = TypeCommon("ElectricGenerator", IfcDomain.Electrical);
    public static readonly PropertyKey ElectricMotorTypeCommon = TypeCommon("ElectricMotor", IfcDomain.Electrical);
    public static readonly PropertyKey ProtectiveDeviceTypeCommon = TypeCommon("ProtectiveDevice", IfcDomain.Electrical);
    public static readonly PropertyKey ElectricApplianceTypeCommon = TypeCommon("ElectricAppliance", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey ElectricFlowStorageDeviceTypeCommon = TypeCommon("ElectricFlowStorageDevice", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey ElectricTimeControlTypeCommon = TypeCommon("ElectricTimeControl", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey MotorConnectionTypeCommon = TypeCommon("MotorConnection", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey ProtectiveDeviceTrippingUnitTypeCommon = TypeCommon("ProtectiveDeviceTrippingUnit", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey SolarDeviceTypeCommon = TypeCommon("SolarDevice", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey AudioVisualApplianceTypeCommon = TypeCommon("AudioVisualAppliance", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey CommunicationsApplianceTypeCommon = TypeCommon("CommunicationsAppliance", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey JunctionBoxTypeCommon = TypeCommon("JunctionBox", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey CableSegmentTypeCommon = TypeCommon("CableSegment", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey CableCarrierSegmentTypeCommon = TypeCommon("CableCarrierSegment", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey CableFittingTypeCommon = TypeCommon("CableFitting", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey CableCarrierFittingTypeCommon = TypeCommon("CableCarrierFitting", IfcDomain.Electrical, IfcSchema.Ifc4);
    public static readonly PropertyKey ElectricDistributionBoardTypeCommon = TypeCommon("ElectricDistributionBoard", IfcDomain.Electrical, new SchemaSpan(ReleaseVersion.Ifc4, Some(ReleaseVersion.Ifc4X3)));
    public static readonly PropertyKey DistributionBoardTypeCommon = TypeCommon("DistributionBoard", IfcDomain.Electrical, IfcSchema.Ifc4X3);

    // [CONTROLS] — the building-automation branch on the distribution-control root.
    public static readonly PropertyKey ActuatorTypeCommon = TypeCommon("Actuator", IfcDomain.Controls);
    public static readonly PropertyKey ControllerTypeCommon = TypeCommon("Controller", IfcDomain.Controls);
    public static readonly PropertyKey SensorTypeCommon = TypeCommon("Sensor", IfcDomain.Controls, IfcSchema.Ifc4);
    public static readonly PropertyKey AlarmTypeCommon = TypeCommon("Alarm", IfcDomain.Controls, IfcSchema.Ifc4);
    public static readonly PropertyKey FlowInstrumentTypeCommon = TypeCommon("FlowInstrument", IfcDomain.Controls, IfcSchema.Ifc4);
    public static readonly PropertyKey UnitaryControlElementTypeCommon = TypeCommon("UnitaryControlElement", IfcDomain.Controls, IfcSchema.Ifc4);

    // [INFRASTRUCTURE] — the transportation-device branch.
    public static readonly PropertyKey TransportElementCommon = Common("TransportElement", IfcDomain.Infrastructure);

    public IfcDomain Domain { get; }
    public SchemaSpan Span { get; }

    // The key-chaining ctor the [SmartEnum<string>] generator's this(key) overload completes (the corpus
    // SmartEnum-with-fields shape).
    private PropertyKey(string key, IfcDomain domain, SchemaSpan span) : this(key) => (Domain, Span) = (domain, span);

    // Span gates recognition, so an IFC2x3 palette never offers a set its dataset does not carry and an IFC4x3 palette
    // never offers the retired distribution-board name.
    public static Seq<PropertyKey> TemplatesFor(IfcDomain domain, ReleaseVersion schema) =>
        toSeq(Items).Filter(row => row.Domain == domain && row.Span.Covers(schema));

    // Every row's upstream is the SAME one — the bundled definition sets under the caller's scope — so a per-row
    // provenance column would be one constant repeated across the roster, and a bSDD anchor never enters here at all
    // (the live dictionary resolves per class in Resolve, named by its own URI there). The mirror proves itself
    // instead: a dataset revision that renames or retires a set surfaces as rows rather than as an anchor silently
    // offering a dead name.
    public static Seq<PropertyKey> Unbacked(ReleaseVersion schema, TemplateScope scope) =>
        toSeq(Items).Filter(row => row.Span.Covers(schema) && !PropertyCatalog.Declares(row.Key, schema, scope));

    // The catalogue floor is deterministic, schema-versioned and network-free, so an unreachable dictionary still
    // resolves — never a fabricated anchor. A bSDD row lowers its FULL class-scoped constraint and the class-level
    // narrowing wins over the property master by contract (.api/api-xbim-properties + .api/api-bsdd).
    public static Map<string, PropertyTemplate> Resolve(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, Option<BsddClass> dictionary) =>
        dictionary.Map(static d => d.Properties).IfNone(Seq<BsddProperty>())
            .Filter(static p => p.PropertySet.Length > 0)
            .Fold(PropertyCatalog.Templates(cls, predefined, schema, scope),
                  static (template, p) => template.AddOrUpdate($"{p.PropertySet}.{p.Code}",
                      Some: existing => Lower(p, Some(existing)),
                      None: () => Lower(p, None)));

    // Dictionary-wins-when-SPEAKING: a declared narrowing wins, a SILENT axis keeps the floor's, and the localized
    // Aliases only the catalogue carries always survive — so a terse dictionary row never ERASES a floor constraint it
    // merely failed to restate. SiDimension follows that law because the floor DOES carry one on every quantity row,
    // and an unconditional take strips a Qto_* dimension and darkens TemplateVerdict.WrongDimension. Traits is the ONE
    // axis with no floor to preserve, the floor's own set being empty.
    static PropertyTemplate Lower(BsddProperty p, Option<PropertyTemplate> floor) =>
        new(p.PropertySet, p.Code, p.DataType, p.ValueKind, p.Traits,
            p.AllowedValues.IsEmpty ? floor.Map(static f => f.AllowedValues).IfNone(Seq<string>()) : p.AllowedValues.Map(static v => v.Value),
            p.Bounds.IsSome ? p.Bounds : floor.Bind(static f => f.Bounds),
            p.Pattern.IsSome ? p.Pattern : floor.Bind(static f => f.Pattern),
            p.Units.IsEmpty ? floor.Map(static f => f.Units).IfNone(Seq<string>()) : p.Units,
            p.SiDimension.IsSome ? p.SiDimension : floor.Bind(static f => f.SiDimension),
            Optional(p.PredefinedValue).Filter(static s => s.Length > 0),
            floor.Map(static f => f.Aliases).IfNone(Map<string, string>()));
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The CDDL-1.0 dataset binary is REFERENCED, never vendored, and it declares templates alone — no IFC entity graph
// and no property value.
public static class PropertyCatalog {
    // The memo key is (schema, scope) so a standard and a COBie handover catalogue coexist, where one Version key let
    // the second load evict the first.
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

    // Templates exist for three published schemas, so a finer seam release folds onto its base rather than missing the
    // catalogue. The fold is ORDERED through the roster-derived IfcSchema.Rank (a bare comparison over the SmartEnum is
    // the elements-page named defect) and names only the two live boundary members, so a new enum member lands on the
    // right side by its own rank where an equality ladder silently fell through to the tail.
    static Version Lower(ReleaseVersion schema) =>
        IfcSchema.Rank(schema) <= IfcSchema.Rank(ReleaseVersion.Ifc2X3)  ? Version.IFC2x3
        : IfcSchema.Rank(schema) < IfcSchema.Rank(ReleaseVersion.Ifc4X3) ? Version.IFC4
        : Version.IFC4x3;

    public static Map<string, PropertyTemplate> Templates(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope) {
        var catalogues = For(schema, scope);
        IEnumerable<PropertyTemplate> properties = catalogues.Psets.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .SelectMany(set => Rows(set.PropertyDefinitions).Map(p => TemplateOf(set.Name, p)));
        IEnumerable<PropertyTemplate> quantities = catalogues.Qtos.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .SelectMany(set => Rows(set.QuantityDefinitions).Map(q => QuantityTemplate(set.Name, q)).Somes());
        return properties.Concat(quantities)
            .Aggregate(Map<string, PropertyTemplate>(), static (template, p) => template.AddOrUpdate($"{p.Set}.{p.Code}", p));
    }

    // The CURRENT value-type kinds are the whole constraint surface, so a set whose only constraint lived on the
    // retired min/max/default slot resolves an ABSENT constraint — the honest reading of a source that no longer
    // states one. The dataset states the property's IFC DATA TYPE and a data type determines its dimension, so
    // DimensionOf supplies the dimension the WrongDimension verdict grades against.
    static PropertyTemplate TemplateOf(string setName, PropertyDef p) {
        var (dataType, kind, allowed, bounds, units) = LowerValue(p.PropertyType?.PropertyValueType);
        return new PropertyTemplate(
            setName, p.Name, dataType, kind, CapabilitySet<TemplateTrait>.None,
            allowed, bounds, None,
            units, DimensionOf(dataType), None,
            Rows(p.NameAliases).Fold(Map<string, string>(), static (acc, alias) => acc.AddOrUpdate(Token(alias.Lang), Token(alias.Value))));
    }

    // A quantity row DOES carry a dimension — its QtoTypeEnum names an IFC measure type and that type publishes the
    // exponents — which is why the bSDD merge preserves the floor's SiDimension. A Count/Time quantity resolves no
    // geometry-derivable dimension and drops.
    static Option<PropertyTemplate> QuantityTemplate(string setName, QtoDef quantity) =>
        QuantityDataType(quantity.QuantityType) is var dataType && DimensionOf(dataType).Case is Dimension dimension
            ? Some(new PropertyTemplate(
                setName, Token(quantity.Name), dataType, BsddValueKind.Single, CapabilitySet<TemplateTrait>.None,
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

    // The authoritative source for the seam InheritanceMode [H1]. The bundled datasets split hard on this axis: the
    // IFC4 dataset declares NOTDEFINED on every set it ships and ONLY the IFC4x3 dataset declares real template types,
    // so a scope PINNED to IFC4 — Cobie and Handover both — always resolves None and rides the structural inference.
    // QtoSetDef carries no templatetype at all, so a Qto set name resolves None here too. A scope-pinned dataset
    // answers from its own schema, the same precedence For(ReleaseVersion, ...) applies.
    internal static Option<templatetype> TemplateTypeOf(string setName, TemplateScope scope) =>
        For(scope.Schema.IfNone(Version.IFC4x3), scope).Psets[setName] is { } set && set.templatetype is var t and not templatetype.NOTDEFINED
            ? Some(t) : None;

    // MethodOfMeasurement is EMPTY on every set the bundled dataset ships, so it surfaces as advisory display evidence
    // and no 5D read keys on it. The MOST-SPECIFIC applicable set is elected — a declaration-order FirstOrDefault let
    // a predefined-scoped set lose to whichever sibling the dataset listed first. The declared names ride along so a
    // derived quantity always keys by a member the standard set declares, never a fabricated suffix.
    public static Option<(string Set, string Method, Seq<(string Name, Dimension Dimension)> Quantities)> BaseQuantitySet(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope) =>
        For(schema, scope).Qtos.DefinitionSets
            .Where(set => Applies(set, cls, predefined))
            .OrderByDescending(set => ScopedMatch(set, cls, predefined))
            .FirstOrDefault() is { } qto
            ? Some((qto.Name, Token(qto.MethodOfMeasurement), Rows(qto.QuantityDefinitions)
                .Map(static q => DimensionOf(QuantityDataType(q.QuantityType)).Map(dimension => (Name: Token(q.Name), Dimension: dimension))).Somes()
                .Filter(static row => row.Name.Length > 0)))
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

    // ONE lowering over the FULL constraint surface — the former parallel switches over the same IPropertyValueType
    // stay collapsed. The composite kinds carry no scalar token, so the token is empty and the IDS facet reads the kind.
    static (string DataType, BsddValueKind Kind, Seq<string> Allowed, Option<BsddBounds> Bounds, Seq<string> Units) LowerValue(IPropertyValueType? valueType) => valueType switch {
        TypePropertySingleValue single   => (Token(single.DataType?.Type), BsddValueKind.Single, Seq<string>(), None, UnitOf(single.UnitType)),
        TypePropertyBoundedValue bounded => (Token(bounded.DataType?.Type), BsddValueKind.Range, Seq<string>(), RangeOf(bounded.ValueRangeDef), UnitOf(bounded.UnitType)),
        TypePropertyReferenceValue refer => (Token(refer.DataType?.Type), BsddValueKind.Single, Seq<string>(), None, Seq<string>()),
        TypeSimpleProperty simple        => (Token(simple.DataType?.Type), BsddValueKind.Single, Seq<string>(), None, PropertyLowering.Stated(simple.UnitType?.Type).ToSeq()),
        TypePropertyEnumeratedValue e    => ("", BsddValueKind.List, Allowed(e), None, Seq<string>()),
        TypePropertyListValue list       => ("", BsddValueKind.List, Seq<string>(), None, UnitOf(list.ListValue?.UnitType)),
        TypePropertyTableValue           => ("", BsddValueKind.ComplexList, Seq<string>(), None, Seq<string>()),
        TypeComplexProperty              => ("", BsddValueKind.Complex, Seq<string>(), None, Seq<string>()),
        _                                => ("", BsddValueKind.Single, Seq<string>(), None, Seq<string>()),
    };

    // The enumerated kind's allowed-value catalogue: EnumList.Items plus the richer ConstantDef names, blank-pruned.
    static Seq<string> Allowed(TypePropertyEnumeratedValue enumerated) =>
        (Rows(enumerated.EnumList?.Items) + Rows(enumerated.ConstantList).Map(static c => Token(c.Name)))
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
        (PropertyLowering.Stated(unit?.Type?.ToString()) | PropertyLowering.Stated(unit?._Value)).ToSeq();

    // The Xbim.Properties dataset surface publishes nullable string columns and nullable backing lists alike, and BOTH
    // admit exactly once here: a string through the folder's ONE blank-or-absent entry
    // (Projection/value#PROPERTY_LOWERING Stated), a list through Rows. No member below re-tests for absence, and no
    // second string-admission owner exists in this folder to drift from that one.
    static string Token(object? declared) => PropertyLowering.Stated(declared?.ToString()).IfNone("");

    static Seq<T> Rows<T>(IEnumerable<T>? source) => source is null ? Seq<T>() : toSeq(source);

    // The roster-mirror probe PropertyKey.Unbacked reads: whether the scope's dataset declares a set name at all.
    internal static bool Declares(string setName, ReleaseVersion schema, TemplateScope scope) =>
        For(schema, scope).Psets[setName] is not null;

    // ids-lib publishes every measure's SI base-dimension exponent vector and SchemaInfo.TryGetMeasureInformation
    // upper-cases its argument, so the match is case-insensitive by construction and the seven integers build the seam
    // Dimension through its own generated factory — the SAME concept the bSDD wire columns build. A pure-number
    // datatype (IfcLabel, IfcBoolean, IfcCountMeasure) is dimensionless and lowers None, so the seam MeasureValue
    // coercion never fires on a count or a label. The memo is keyed by token because the resolution is a linear scan
    // over the published measure set and a graph-wide audit resolves the same handful of tokens per element otherwise.
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

    // An unparseable token is a template the audit cannot grade, so it agrees. The value-type Kind cannot decide this
    // — BsddValueKind.Single spans a label, a boolean, and a thermal transmittance alike — so a Pset declaring
    // IfcThermalTransmittanceMeasure and carrying the U-value as a Text row passed every other axis while being
    // unreadable to every downstream measure consumer.
    internal static bool DataTypeAgrees(string dataType, PropertyValue value) =>
        !SchemaInfo.TryParseIfcDataType(dataType, out _)
        || DimensionOf(dataType).IsSome == (value is PropertyValue.Measure);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The structural inference rides as a ROW COLUMN, so the classifier's fallback reads a declared policy rather than
// re-deciding what a boolean meant at each call site.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TypeBinding {
    public static readonly TypeBinding Occurrence = new("occurrence", InheritanceMode.OccurrenceWins);
    public static readonly TypeBinding TypeBound = new("type-bound", InheritanceMode.TypeDrivenOverride);

    public InheritanceMode Inferred { get; }

    private TypeBinding(string key, InheritanceMode inferred) : this(key) => Inferred = inferred;
}

public static class PropertyInheritance {
    // NOTDEFINED never reaches here — TemplateTypeOf maps it to None.
    static InheritanceMode FromTemplate(templatetype t) => t switch {
        templatetype.PSET_TYPEDRIVENONLY or templatetype.QTO_TYPEDRIVENONLY         => InheritanceMode.TypeDrivenOnly,
        templatetype.PSET_TYPEDRIVENOVERRIDE or templatetype.QTO_TYPEDRIVENOVERRIDE => InheritanceMode.TypeDrivenOverride,
        _                                                                           => InheritanceMode.OccurrenceWins,
    };

    // A Qto set is type-driven-override by STRUCTURE whatever its bag (QtoSetDef declares no templatetype at all), so
    // that arm is the set's own property; every other undeclared set reads the binding row's own inference.
    public static InheritanceMode ModeOf(string setName, TypeBinding binding, TemplateScope scope) =>
        PropertyCatalog.TemplateTypeOf(setName, scope).Match(
            Some: FromTemplate,
            None: () => setName.StartsWith("Qto_", StringComparison.Ordinal)
                ? InheritanceMode.TypeDrivenOverride
                : binding.Inferred);
}
```

## [03]-[BASE_QUANTITIES]

- Owner: `QuantityDerivation` the base-quantity fold deriving the standard `Qto_*BaseQuantities` from `MeasureBundle` — the kernel `Rasm` `Analysis/measure` KIND-KEYED multi-domain takeoff carrier whose `Seq<(MassKind Kind, double Magnitude)> Measures` holds one magnitude per answered domain, whose `Magnitude(MassKind)` answers `Option<double>` so an unheld domain reads honest absence and never a fabricated zero, and whose `Coverage` DERIVES the held `CapabilitySet<MassKind>` off those rows rather than a hand-kept mirror, minted by `MeasureBundle.Of(GeometryBase, CapabilitySet<MassKind>, Context, Op?)` one leased mass handle per demanded kind — the sibling single-domain `GeometryMeasures` moment bundle answers ONE domain and its `Centroid`/`Radii`/`Inertia`/`InertiaProducts`/`PrincipalFrame` moments serve the structural consumers, so it is the wrong carrier at this owner: one `Qto_*BaseQuantities` set declares Length, Area, and Volume members TOGETHER, and a one-kind bundle derived a single dimension, left every member of another dimension standing on its occurrence value, and re-paid the mass computation once per domain the caller asked for separately — the kernel/Compute resolve from the geometry the seam `Object` node references by content key (`Model/elements#REPRESENTATION_KEYS` `RepresentationContentHash`) and supply to `Derive` (Bim consumes the measure, never tessellates it) — producing the seam `QuantitySet` node values as seam `MeasureValue` under derived-wins precedence. The class's base-quantity SET, its `MethodOfMeasurement` basis (the measurement-rule string the 5D estimate displays beside the values WHEN a dataset states one — the bundled definitions state none, every set resolving empty, so no 5D read depends on the basis), and each declared quantity's NAME + `Dimension` come from `PropertyCatalog.BaseQuantitySet` (the offline `Xbim.Properties` `QtoSetDef` catalogue, predefined-scoped like the Pset leg), so the roster covers every class the standard defines, not a hand-listed slice — and every derived key names a quantity the standard set declares.
- Entry: `QuantityDerivation.Demand(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope)` and `QuantityDerivation.Demand(Seq<BakedMaterial> materials)` are the ONE polymorphic demand entrypoint each fold's caller mints its bundle under, discriminating on input shape — the declared-member ceiling for the class takeoff, the composition-implied ceiling for the material takeoff — so no caller re-reads the catalogue or the composition rows to guess which kinds to ask for. `QuantityDerivation.Derive(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, MeasureBundle measures, Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence, Op key)` derives the geometry-true base quantities for a class and merges them over the occurrence-stored quantities under derived-wins precedence (the geometry takeoff supersedes an authoring tool's stored quantity), returning the seam `QuantitySet` node value map; a class with no `Qto_*BaseQuantities` set in the catalogue returns the occurrence quantities unchanged so a non-takeoff class never blocks. `QuantityDerivation.Decompose(MeasureBundle measures, Seq<BakedMaterial> materials, Func<ProfileRef, Option<SectionProperties>> sections, Op key)` is the MATERIAL-true takeoff the element-level fold cannot answer ("how much concrete is in this model") — the element volume split per `MaterialId` over the seam `MaterialComposition` the baked element's `Associate` edges bind (`element.Materials` + the `SectionOf` baked section are the caller's `Bake` reads): a `LayerSet` splits by thickness share, a `ConstituentSet` by declared `Fraction`, a `ProfileSet` folds PER COMPOUND ROW — each seam `MaterialProfile`'s own one-hop-resolved `SectionProperties.Area × Length` under its OWN `MaterialId`, the swept length and the sibling arms' volume read off ONE bundle because a single element's material rows mix the two modalities and a one-kind carrier answered at most one of them, re-stamped `QuantityType.Volume` through the band-preserving `WithType` (`Multiply` is dimension-anonymous by seam law), a row whose section does not resolve contributing no share, a `Single` carries the element volume whole; a colliding `MaterialId` sums through the seam `MeasureValue.Sum`, an absent element measure yields no row (never a fabricated zero), and the multi-ply WEIGHT decomposition stays the `Rasm.Compute` `AssemblyAggregator`'s — the frozen boundary: volume splits are composition-derivable in full, mass is not.
- Auto: `Derive` reads `PropertyCatalog.BaseQuantitySet(cls, predefined, schema, scope)` (the `Qto_*` set name + its `MethodOfMeasurement` + each geometry-relevant `QtoDef`'s declared NAME paired with its `Dimension`, the MOST-SPECIFIC applicable set elected — a `PredefinedType`-scoped row beats a blank-scope sibling, never dataset declaration order) and asks the ONE `Derivations` frozen table for each DECLARED MEMBER by `(Dimension, name)` — a member the table answers derives, a member it does not answer derives nothing and leaves the occurrence value standing, and a member whose row demands a domain the bundle never held reads `Magnitude`'s `None` and leaves it standing likewise, so the emitted key is a standard-set member BY CONSTRUCTION and an oriented takeoff the scalar bundle cannot separate (`GrossArea`, `NetSideArea`, `GrossVolume`) is never stamped from the one it can; each row's `Demands` column is the same table entry's second half, so `Demand` unions the declared members' domains and the projector reads exactly what that union minted — one correspondence, never a demand list beside a projector list that drift apart; the kernel scalar is already SI-base, so each derived value admits through the seam `MeasureValue.OfSi(QuantityType, Dimension, double)` carrying its QTO identity (a dimension-only admit stamps the dimension-anonymous type and strips the QTO read off every derived-wins row), the set name riding the `QuantitySet` bag node so a `{Set}.{name}`-prefixed non-member key whose derived-wins merge silently never collides is the deleted form, merged over the occurrence map with derived-wins so the 5D `Planning/cost#ESTIMATE` join reads the geometry-true measure (`Volume ≻ Area ≻ Length ≻ Mass`); the derived keys are the SAME bare spellings the seam `Properties/property#DETAIL_SCHEMA` `QuantityRows` statics freeze for its non-referencing readers, whose net-before-gross chains (`SurfaceArea`/`FloorArea`/`FootprintArea`/`CrossSection`/`Volume`/`Weight`) fold these rows back first-hit-wins — writer key and reader chain meeting on one declared spelling; `NetWeight = NetVolume × massDensity` through the seam `MeasureValue.Multiply` re-stamped by the band-preserving `WithType(QuantityType.Mass)` (`VolumeDim × DensityDim IS MassDim`, so the algebra proves the product and carries the density's `MeasureBand` forward), a non-density carrier or an absent density skipping the weight rows; an element-set aggregate of the same `Dimension` reduces through the seam `Properties/quantity#MEASURE_VALUE` `MeasureValue.Sum` reducer, never a manual `double` fold.
- Packages: Xbim.Properties, ids-lib, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new class's base-quantity set is already in the `Xbim.Properties` `QtoSetDef` catalogue (no edit); a newly derivable standard quantity is one `Derivations` row keyed `(Dimension, declared member name)` carrying its demanded `CapabilitySet<MassKind>` beside its admitting projector, so `Demand` widens with the row and no caller edits, landing only where the kernel bundle honestly answers that member; a new decomposition modality is the seam `MaterialComposition` case arm the `Shares` and `Demand` generated total `Switch`es both break on at compile; a new kernel mass domain is demandable the moment the kernel lands its row, the bundle keying by `MassKind` and widening by data alone; the derived quantities merge over the occurrence map under one precedence rule; never a per-class `Derive` method, never a hand-listed per-class set table, and never a re-tessellation in this owner.
- Boundary: base-quantity derivation runs from the kernel `Analysis/measure` `MeasureBundle` the kernel/Compute resolve from the `RepresentationContentHash` geometry and inject into `Derive`, so a Bim-local `MeasureBundle` re-declaration or an in-owner geometry-measure computation is the deleted form (Bim depends UP on the kernel and never owns geometry measurement); every read is `Magnitude(MassKind)` answering `Option<double>`, so a re-spelled `measures.Length`/`.Area`/`.Volume` column read and a `?? 0` collapse at the unheld-domain edge are the deleted forms — one bundle derives EVERY declared dimension the mint held, and the single-domain `GeometryMeasures` carrier this owner retired is the deleted form because a `Qto_*` set declaring Length, Area and Volume derived one of the three, left the other two on their occurrence values indistinguishably from a member the table cannot answer, and re-paid the mass computation for each domain the caller then asked for separately; the kernel refuses a bundle WHOLE on a demanded kind the geometry cannot measure, so `Demand` publishes the declared ceiling and the geometry-side resolver lowers it — a caller demanding `CapabilitySet<MassKind>.All` refuses every solid on the curve-length solve, and a caller hand-listing kinds beside the catalogue is the second roster `Demand` deletes; Bim reads no moment slot, so a `Centroid`/`Radii`/`Inertia`/`InertiaProducts`/`PrincipalFrame` read in this folder is the seam violation naming `GeometryMeasures`' structural consumers' business; a re-tessellation in this owner is the named seam violation (geometry realization routes the `Exchange/tessellation#TESSELLATION_BRIDGE` companion rail); the derived value is a seam `MeasureValue` admitted through `MeasureValue.OfSi` under its QTO `QuantityType` (the seam owns the typed quantity over `Dimension` + UnitsNet), so a Bim-local `MeasureValue` re-declaration, a dimension-anonymous derived takeoff, a hand-stamped unit string drifting from the seam canonical `Dimension.SiSymbol`, and a bare-`double` product standing in for the seam `Multiply`/`WithType` algebra are the deleted forms; `NetWeight` is the homogeneous-element takeoff (`NetVolume × Mechanical.Density`, the material's `Composition/material#MATERIAL_PROPERTY` `Mechanical.Density` resolved One-Hop from the `Associate` material edge) and the multi-ply/layered weight is the `Rasm.Compute` `AssemblyAggregator`'s richer fold, never re-modeled here; the base-quantity SET and its declared quantity names/dimensions come from the `Xbim.Properties` `QtoSetDef` catalogue and a hand-listed per-class `BaseQuantityTable` that slices the standard is the deleted form; a derived value keys ONLY by a quantity name the standard set declares (the fold walks the declared members and looks each up — the dataset's own `Qto_BuildingStoreyBaseQuantities` `NetHeigtht` misspelling included, the proof that a hand-spelled mirror forks on the first divergence) and a fabricated `{Set}.{suffix}` non-member key — one the `Review/validation#IDS_FACETS` Property facet and a downstream `Qto` reader never match — is the deleted form; the ONE `(Dimension, member)`-keyed `Derivations` frozen table owns the per-member election, and a dimension-keyed row that lands one measure on whichever member a set declared first is the deleted form that fabricated a gross takeoff from a net solid and a plan area from a surface area; the derived-wins precedence is applied once in `Derive`, never a per-call-site merge; the per-material `Decompose` reads the seam composition rows ONLY (thickness share, declared fraction, baked section area) — re-deriving a composition here, re-tessellating per material, or fabricating a mass split the composition cannot carry is the deleted form, the weight decomposition staying `Rasm.Compute`'s.

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
public static class QuantityDerivation {
    // ONE domain row per kernel MassKind carrying the seam Dimension and QuantityType its magnitude admits under plus
    // the standard member NAMES that magnitude honestly answers, so a newly answerable member is one string on its
    // row rather than a copied body. Area publishes ONE magnitude — the realized solid's total surface area — so it
    // answers only the members asking for exactly the realized body's whole surface, the realized body BEING the net
    // solid with its processings resolved; GrossArea, NetArea, GrossSideArea, NetFootprintArea and CrossSectionArea
    // are PROJECTIONS along an axis the bundle does not carry, GrossSurfaceArea the pre-void magnitude the net body
    // cannot separate, OuterSurfaceArea the lateral envelope the total cannot isolate — none is named and each
    // occurrence value survives derived-wins. Volume answers NetVolume and the validation set's bare Volume for the
    // same reason, GrossVolume being the pre-void magnitude the same geometry cannot separate.
    static readonly Seq<(MassKind Domain, QuantityType Type, Dimension Dimension, Seq<string> Members)> Takeoffs = Seq(
        (MassKind.Length, QuantityType.Length, Dimension.LengthDim, Seq("Length")),
        (MassKind.Area, QuantityType.Area, Dimension.AreaDim, Seq("SurfaceArea", "TotalSurfaceArea", "NetSurfaceArea")),
        (MassKind.Volume, QuantityType.Volume, Dimension.VolumeDim, Seq("Volume", "NetVolume")));

    // Every row admits through MeasureValue.OfSi(QuantityType, Dimension, double) carrying its QTO IDENTITY: the
    // dimension-only overload stamps the dimension-anonymous QuantityType, and a derived-wins merge then REPLACED an
    // occurrence-stored QuantityType.Volume with an anonymous one, voiding the As(QuantityType) read and failing the
    // Type-equality gate on any Sum against a stored quantity. NetWeight is the DIMENSIONED product — Multiply over
    // the density carrier (VolumeDim x DensityDim IS MassDim) re-stamped through the band-preserving WithType — where
    // a bare `volume * density.Si` discards every declared uncertainty; its guard stays on the INPUT, so the product's
    // dimension is right by construction.
    // Demands rides BESIDE its projector as the same row's second column, so the demand a caller mints the bundle
    // under and the magnitude the projector reads resolve from ONE correspondence — a member demanding a domain the
    // projector never reads, and a projector reading a domain nothing demanded, are both unspellable.
    static readonly FrozenDictionary<(Dimension Dimension, string Member),
        (CapabilitySet<MassKind> Demands, Func<MeasureBundle, Option<MeasureValue>, Op, Option<Fin<MeasureValue>>> Project)> Derivations =
        Takeoffs
            .Bind(row => row.Members.Map(member => KeyValuePair.Create(
                (row.Dimension, member),
                (Demands: CapabilitySet<MassKind>.Of(row.Domain),
                 Project: (Func<MeasureBundle, Option<MeasureValue>, Op, Option<Fin<MeasureValue>>>)((measures, _, key) =>
                     measures.Magnitude(row.Domain).Map(v => MeasureValue.OfSi(row.Type, row.Dimension, v, key: key)))))))
            .Add(KeyValuePair.Create(
                (Dimension.MassDim, "NetWeight"),
                (Demands: CapabilitySet<MassKind>.Of(MassKind.Volume),
                 Project: (Func<MeasureBundle, Option<MeasureValue>, Op, Option<Fin<MeasureValue>>>)(static (measures, density, key) =>
                     from volume in measures.Magnitude(MassKind.Volume).Map(v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v, key: key))
                     from carrier in density.Filter(static d => d.Dimension == Dimension.DensityDim)
                     select volume.Bind(admitted => admitted.Multiply(carrier, key)).Bind(mass => mass.WithType(QuantityType.Mass, key))))))
            .ToFrozenDictionary();

    // Demand names the mass domains a class's DECLARED members can answer, so the geometry-side resolver mints the
    // bundle over exactly those kinds. Kernel law refuses a bundle WHOLE on a demanded kind the geometry cannot
    // measure, so this set is the declared ceiling that resolver lowers against the geometry it holds: demanding
    // every kind refused a solid's whole bundle on the curve-length solve, and demanding none re-paid the mass
    // computation once per domain the fold asked for separately.
    public static CapabilitySet<MassKind> Demand(IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope) =>
        PropertyCatalog.BaseQuantitySet(cls, predefined, schema, scope).Match(
            None: static () => CapabilitySet<MassKind>.None,
            Some: set => set.Quantities.Distinct().Fold(CapabilitySet<MassKind>.None, static (demand, member) =>
                Derivations.TryGetValue((member.Dimension, member.Name), out var row)
                    ? row.Demands.Held.Aggregate(demand, static (held, kind) => held.With(kind))
                    : demand));

    // Decompose demands off the SAME discriminant its Shares fold reads — the seam MaterialComposition case — so the
    // two never disagree about which domain a composition needs, and a new modality breaks BOTH total Switches at
    // compile time rather than silently under-demanding a magnitude the split then reads as absent.
    public static CapabilitySet<MassKind> Demand(Seq<BakedMaterial> materials) =>
        materials.Fold(CapabilitySet<MassKind>.None, static (demand, baked) => demand.With(baked.Material.Composition.Switch(
            single: static _ => MassKind.Volume,
            layerSet: static _ => MassKind.Volume,
            profileSet: static _ => MassKind.Length,
            constituentSet: static _ => MassKind.Volume)));

    // The fold walks the set's DECLARED MEMBERS and asks the table for each by NAME, so a member the table does not
    // answer derives NOTHING and leaves whatever the occurrence stored. That per-member keying is the whole election:
    // the retired dimension-keyed table plus its "first-declared quantity of this dimension" fallback stamped ONE
    // takeoff onto whichever member a set happened to list first, so a class declaring only GrossArea received the
    // total surface area under that name and a class declaring only GrossVolume received the net solid volume under
    // that one — both reading as measured takeoffs and neither being one. The emitted key is a standard-set member BY
    // CONSTRUCTION because the key IS the member the catalogue declared; the QuantitySet bag node carries the set
    // name, so a `{Set}.{name}`-prefixed PropertyName is a NON-MEMBER key that never collides with the occurrence rows
    // and never matches the IDS facet. massDensity is the material's Mechanical.Density resolved One-Hop from the
    // Associate edge, absent which the weight rows skip rather than fabricate. One bundle answers EVERY declared
    // dimension at once: a Qto_*BaseQuantities set naming Length, SurfaceArea and NetVolume together derives all
    // three off the kind-keyed rows, where the retired single-domain bundle derived whichever one domain it carried
    // and left the rest standing on their occurrence values while the caller re-measured for each.
    public static Fin<Map<PropertyName, MeasureValue>> Derive(
        IfcClass cls, Option<string> predefined, ReleaseVersion schema, TemplateScope scope, MeasureBundle measures,
        Option<MeasureValue> massDensity, Map<PropertyName, MeasureValue> occurrence, Op key) =>
        PropertyCatalog.BaseQuantitySet(cls, predefined, schema, scope).Match(
            None: () => Fin.Succ(occurrence),
            Some: set => set.Quantities.Distinct().Fold(Fin.Succ(occurrence), (rail, member) =>
                rail.Bind(acc => Derivations.TryGetValue((member.Dimension, member.Name), out var row)
                    ? row.Project(measures, massDensity, key).Match(
                        Some: derived => derived.Map(value => acc.AddOrUpdate(PropertyCategory.Seam.Row(member.Name), value)),
                        None: () => Fin.Succ(acc))
                    : Fin.Succ(acc))));

    // A colliding MaterialId (occurrence + type-inherited bindings naming one substance) sums through the seam
    // MeasureValue.Sum; an absent element measure yields no row, never a fabricated zero. Volume only: the multi-ply
    // WEIGHT fold stays the Rasm.Compute AssemblyAggregator's, the frozen boundary being that volume splits are
    // composition-derivable in full and mass is not.
    public static Fin<Map<MaterialId, MeasureValue>> Decompose(
        MeasureBundle measures, Seq<BakedMaterial> materials, Func<ProfileRef, Option<SectionProperties>> sections, Op key) =>
        materials.TraverseM(baked => Shares(measures, baked.Material.Composition, sections, key)).As()
            .Bind(rows => rows.Flatten().Fold(Fin.Succ(Map<MaterialId, MeasureValue>()), (rail, row) =>
                rail.Bind(acc => acc.Find(row.Material).Match(
                    Some: existing => MeasureValue.Sum(Seq(existing, row.Share), key).Map(sum => acc.SetItem(row.Material, sum)),
                    None: () => Fin.Succ(acc.Add(row.Material, row.Share))))));

    // The generated TOTAL Switch, so a new composition modality breaks this at compile time. LayerSet's thickness
    // total is > 0 by the seam OfLayerSet admission; Multiply is dimension-anonymous by seam law, so the
    // QuantityType.Volume re-stamp through the band-preserving WithType is the consumer's. ProfileSet folds PER
    // COMPOUND ROW because a ProfileSet carries EVERY IfcMaterialProfile row with its own material and its own
    // content-keyed section — the retired single-section read multiplied ONE section area by the member length and
    // attributed the whole swept volume to the set's head material, which is exactly the takeoff a composite member is
    // asked for and exactly the one it got wrong. `sections` is the caller's one-hop resolution (the Rasm.Materials
    // catalog lookup the seam deliberately does not store), so an unresolved row contributes NO share rather than
    // borrowing a sibling plate's area.
    static Fin<Seq<(MaterialId Material, MeasureValue Share)>> Shares(
        MeasureBundle measures, MaterialComposition composition, Func<ProfileRef, Option<SectionProperties>> sections, Op key) =>
        composition.Switch(
            single: s => ElementVolume(measures, key).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Map(volume => Seq((s.Material, volume)))),
            layerSet: s => ElementVolume(measures, key).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Bind(volume => {
                    double total = s.Layers.Fold(0.0, static (acc, layer) => acc + layer.Thickness.Si);
                    return s.Layers.TraverseM(layer =>
                        volume.Scale(layer.Thickness.Si / total, key).Map(share => (layer.Material, share))).As();
                })),
            profileSet: s => measures.Magnitude(MassKind.Length).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: length => MeasureValue.OfSi(QuantityType.Length, Dimension.LengthDim, length, key: key)
                    .Bind(span => s.Profiles
                        .TraverseM(row => sections(row.Profile).Match(
                            None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                            Some: section =>
                                from swept in section.Area.Multiply(span, key)
                                from volume in swept.WithType(QuantityType.Volume, key)
                                select Seq((row.Material, volume))))
                        .As()
                        .Map(static rows => rows.Flatten()))),
            constituentSet: s => ElementVolume(measures, key).Match(
                None: static () => Fin.Succ(Seq<(MaterialId, MeasureValue)>()),
                Some: fin => fin.Bind(volume => s.Constituents.TraverseM(constituent =>
                    volume.Scale(constituent.Fraction, key).Map(share => (constituent.Material, share))).As())));

    static Option<Fin<MeasureValue>> ElementVolume(MeasureBundle measures, Op key) =>
        measures.Magnitude(MassKind.Volume).Map(v => MeasureValue.OfSi(QuantityType.Volume, Dimension.VolumeDim, v, key: key));
}
```

## [04]-[TEMPLATE_AUDIT]

- Owner: `TemplateAudit` the graph-wide standard-template conformance fold — the first model-quality question every project asks spec-free ("does each element carry its standard Pset with correctly-typed, in-range values") answered directly against the buildingSMART ground truth this page already resolves, with no authored IDS document; `TemplateVerdict` the `[SmartEnum<string>]` closed verdict vocabulary (`Missing`/`KindMismatch`/`DataTypeMismatch`/`NotAllowed`/`OutOfBounds`/`PatternReject`/`WrongDimension` — one row per constraint axis the `PropertyTemplate` carries, so a new template constraint axis is one verdict row and one `Verdict` arm); `TemplateFinding` the typed per-element finding row a report renders and a fix pass keys on.
- Entry: `TemplateAudit.Run(ElementGraph graph, TemplateScope scope, Func<IfcClass, Option<BsddClass>> dictionary, Op key)` audits every entity-type-classified occurrence `Object` node (the `ClassificationSystem.IfcSystem` row key compared in the roster's own `OrdinalIgnoreCase` space, never a bare token literal) against its resolved templates — templates resolve ONCE per distinct `(Classification.Code, PredefinedType.Token)` pair through `PropertyKey.Resolve` (the catalogue floor ∪ live dictionary union, `graph.Header.Schema` the schema, the caller's `scope` the definition set — a `Handover` audit grades COBie completeness on the same fold that grades the standard sets, the injected `dictionary` the per-class live evidence a caller supplies or leaves `None` for the offline-only audit) and every element of that pair checks against the SAME resolved map, never a per-element re-resolution; `Fin<T>` carries only the seam `Bake` rail (an absent root or cyclic compose is the graph's fault, never this fold's) and the audit itself is total — a clean model returns the empty finding set.
- Auto: per element the merged `Bake`-derived `element.Properties`/`element.Quantities` bags (type→occurrence precedence already applied by the stamped `InheritanceMode`) probe each template row — an absent value on a template whose `Traits` admit `Required` lands `Missing`; a present value decides per axis: a `Text`/`Enumerated` value outside a non-empty `AllowedValues` lands `NotAllowed`, a `Text` failing the whole-value-anchored `Pattern` lands `PatternReject`, a `Measure` whose `Dimension` disagrees with `SiDimension` lands `WrongDimension`, a `Measure`, `Integer`, or `Number` outside `Bounds` lands `OutOfBounds` (the bSDD `ClassPropertyContract.v1` min/max carry for Integer and Real properties, not only dimensioned measures), and a seam case irreconcilable with the template `Kind` (a `Complex` where the kind is `Single`) lands `KindMismatch` — the verdict axes are the SAME constraint family the `Review/validation#IDS_FACETS` facet narrows into its `ValueConstraint`, decided here with the failing AXIS named because a QA report acts per axis where a facet needs only pass/fail.
- Receipt: the `Seq<TemplateFinding>` is the baseline-tier evidence — composed WHOLE as the `Review/validation#MODEL_HEALTH` `ModelFinding.Baseline` case beneath the authored IDS audits, so `Rasm.AppUi` and the review pipeline read the ONE `ModelHealth` verdict surface, never this stream directly; each row carries the element `NodeId`, the `{Set}.{Code}` template coordinate, the verdict, and the actual value so a fix pass addresses the exact property.
- Packages: Xbim.Properties, ids-lib, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new constraint axis is one `TemplateVerdict` row and one `Verdict` arm reading the new `PropertyTemplate` column, ordered most-specific first; a richer dictionary is the same injected `dictionary` resolver (zero fold edits); never a per-class audit method, never a second checker beside the IDS lane, and never a finding type per verdict.
- Boundary: the audit READS the resolved `PropertyTemplate` map and the baked element — it re-derives neither the template union (that is `PropertyKey.Resolve`'s) nor the type→occurrence merge (the seam `Bake`'s under the stamped mode [H1]); the verdict vocabulary mirrors the seam `Rasm.Element/Query/predicate#ELEMENT_PREDICATE` `ValueMatch` restriction family but stays a SEPARATE closed vocabulary because the finding names the failing axis where `ValueMatch` answers only membership — the IDS lane keeps `ValueMatch`, this lane keeps the axis-named verdict, and collapsing the two erases the axis evidence; the audit is spec-FREE (the buildingSMART templates ARE the spec) and a user-authored requirement routes the `Review/validation#IDS_FACETS` owner, never a template-audit extension; the finding stream surfaces only through the `Review/validation#MODEL_HEALTH` composition — a second report consumer forked off this stream is the deleted form; the `Pattern` compiles once per template row (`RegexOptions.NonBacktracking`, whole-value anchored — the untrusted-grammar law), never per element; the fold is SPAN-grade under [MODEL_SLOT_RULING] — one span over the whole graph pass carrying the package namespace slot, never a per-element or per-template instrument, because occurrences and resolved template rows are both unbounded in model size and a metric keyed on either multiplies every series by that count; the span itself is `Observability`'s to open around this entry and a telemetry mint inside this fold is the deleted form, the audit returning findings alone.

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

    // PropertyCategory.Seam.Row is the owner-blessed EMPTY-prefix category the round-tripped
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
            None: () => template.Traits.Admits(TemplateTrait.Required) ? Some(TemplateVerdict.Missing) : None,
            Some: value => !Compatible(template.Kind, value) ? Some(TemplateVerdict.KindMismatch)
                // A second, finer axis than Kind, which spans IfcLabel, IfcBoolean and IfcThermalTransmittanceMeasure
                // alike; an unparseable or unstated token grades nothing.
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
