# [STRUCTURAL_PROJECTION]

The structural-analysis-domain reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: `StructuralProjection` lowers the GeometryGym structural-analysis entity surface onto NEUTRAL seam payloads — the `Map<PropertyName, PropertyValue>` attribute bag a `Relations/relation#EDGE_ALGEBRA` `Relationship.Generic` edge or a structural `Properties/property#PROPERTY_BAG` `PropertySet` node carries — so a `Rasm.Compute` frame solve reads the idealization off the ONE `Graph/element#ELEMENT_GRAPH` `ElementGraph` it already holds, never a second store. The idealized analytical line is NOT a payload this reader produces: it is content-keyed into the member `Object` node's `Graph/element#NODE_MODEL` `RepresentationContentHash` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time (the ONE `IfcRepresentation.Keys` representation content-keyer, every geometry — display `Body` and analytical `Axis`/`FootPrint` — hashed alike), and `Rasm.Compute` resolves the coordinate line one-hop BY CONTENT KEY from the blob store; an inline `AxisCurve`/`Vector3` coordinate field on the seam `Object` node is the named §4-RT-M2 seam violation, the deleted form. This RETIRES the migration source's parallel `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint` record family keyed by `BimModel`/`GlobalId` (the very "second stored record off the element" the rebuild forbids): an idealized curve member is the seam `Object` node it already is (an `IfcStructuralItem` is an `IfcProduct`, so the general projector mints it), its analytical line the content-keyed `Axis` representation, the member↔connection 6-DOF restraint and member↔activity applied load the `Generic` edge payloads `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.Structural` carries, and the analysis-model / load-group grouping the `Assign.Group` edge the general `IfcRelAssignsToGroup` fold authors — never a re-modeled analysis mesh and never a parallel selection surface.

The owner is the deep STRUCTURAL half of the projection `SemanticProjector` keeps out of the general fold: the complete `IfcBoundaryCondition` restraint algebra, relationship-level member-end release, applied-load family, grouping definitions, analytical topology discriminants, inverse authoring residue, and SAF workbook exchange. The reader is HOST-NEUTRAL: absent optional source detail remains `Option`/empty enrichment, while every present SI magnitude crosses `MeasureValue.OfSi` on the enclosing `Fin` rail and re-keys rejection through `ElementFault.ValueRejected`; no host geometry type, non-finite measure, or partial inverse crosses the boundary.

## [01]-[INDEX]

- [02]-[STRUCTURAL_PROJECTION]: `StructuralProjection` the GeometryGym structural-analysis-domain reader — `Attrs` the ONE polymorphic attribute-bag reader discriminating the structural entity onto its neutral `Map<PropertyName, PropertyValue>` payload (an `IfcRelConnectsStructuralMember` onto the member-end restraint edge bag — the rel-level `AppliedCondition` falling back to the connection's, the `ConditionCoordinateSystem` frame, the `SupportedLength`, and the folded-in `AtStart` endpoint discriminant; an `IfcRelConnectsStructuralActivity` onto the applied-load edge bag plus the point-action `Station` position; an `IfcStructuralConnection`/`IfcStructuralActivity` onto its own restraint/load bag; an `IfcStructuralLoadCase`/`IfcStructuralLoadGroup`/`IfcStructuralResultGroup`/`IfcStructuralAnalysisModel`/`IfcStructuralCurveMember`/`IfcStructuralSurfaceMember` onto its self-weight / load-combination / theory-and-linearity / model-type-plus-group-joins-plus-2D-plane / local-axis / thickness definition bag), the load components riding the consumer-neutral `StructuralRows.Force`/`Moment` axis families with the per-family SI `Dimension`, plus the two-tier `ActionRow` derivation (specific `IfcActionSourceTypeEnum` rows, then the group `IfcActionTypeEnum` nature) carrying the neutral case token, the EN 1990 `ActionClass`, the `ImposedLoadCategory`, and the Eurocode `Psi0`/`Psi1`/`Psi2`/`GammaSup`/`GammaInf` factor rows the `EurocodePolicy` resolves, and the FE `LoadKind` token `Rasm.Compute` reads; `AtStart` and `Station` the transient-topology endpoint/position discriminants the restraint and load edges carry; the idealized analytical line is NOT read here — it is content-keyed into the member `Object`'s `Representations` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time and `Rasm.Compute` resolves it one-hop by content key.

## [02]-[STRUCTURAL_PROJECTION]

- Owner: `StructuralProjection` the static structural-analysis-domain reader `SemanticProjector` composes, lowering the GeometryGym structural-analysis surface onto neutral seam payloads — never a stored record. It owns the polymorphic `Attrs` attribute-bag reader (one entry discriminating the structural entity — relationship, connection, activity, load case/group, result group, analysis model, curve/surface member — onto its restraint / load / definition bag), the `AtStart`/`Station` transient-topology discriminants, the `ActionRow` two-tier load-action derivation, and the `EurocodePolicy`/`EurocodeAction` pair under which the EN 1990 combination and partial factors resolve; every row name it stamps is a `Rasm.Element` `StructuralRows` static or a `PropertyCategory.Seam.Row` mint, never a call-site spelling; the typed analysis structures the migration source minted (`AnalysisModel`, the `AnalysisMember` `[Union]`, `LoadGroup`, `Support`, `MemberConnection`, `SupportRestraint`, `StructuralLoadKind`, `StructuralCurveMemberKind`) are all GONE — the member is the seam `Object` node, the joint kind its `PredefinedType` token, the topology its neutral `Connect`/`Generic` edges, the restraint/load the typed `PropertyValue` edge payloads, and the analytical line the `Axis`-keyed content hash in the member's `Representations` map (content-keyed at `ObjectNode`, resolved one-hop by `Rasm.Compute`, never read or baked here).
- Entry: `Attrs(BaseClassIfc? entity, Op key, Option<EurocodePolicy> eurocode = default)` lowers every supported structural entity through one `Fin<Map<PropertyName, PropertyValue>>` dispatch; `eurocode` is the annex-plus-Table-A1.2 policy VALUE under which the load arm resolves the EN 1990 combination and partial factors, absent which the arm emits the IFC-declared attributes alone and never a `RecommendedValues` set nobody selected. All SI measures traverse `MeasureValue.OfSi` and re-key `ElementFault.ValueRejected`; non-finite GeometryGym sentinels remain absent. `AtStart` and `Station` return `Option` discriminants, so unresolved topology emits no assertion. `Author(DatabaseIfc, IfcObjectDefinition, Map<PropertyName, PropertyValue>)` re-stamps verified restraint and single-force constructors and returns every unconsumed row as fidelity residue. `Saf(SafOp operation, IExcelImportService imports, IExcelExportService exports, IExcelValidator validator, Op key)` validates and executes both XLSX directions over `ExcelModel.Objects`; the source version derives from `ExcelModel.OriginalVersion`, while the operation carries only the caller-selected target version.
- Auto: the analytical line is not produced here — at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` the member's inherited `IfcProduct.Representation` `IfcProductDefinitionShape` is content-keyed through `IfcRepresentation.Keys` (every `RepresentationIdentifier` — `Axis`/`Body`/`Box`/`FootPrint` — onto its content hash), so the `Axis` line and the heavy display body alike ride `RepresentationContentHash` and `Rasm.Compute` resolves the line's coordinates one-hop by content key from the blob store; the restraint arms discriminate the boundary condition over `IfcBoundaryNodeCondition` (the `TranslationalStiffnessX`/`Y`/`Z` `IfcTranslationalStiffnessSelect` + `RotationalStiffnessX`/`Y`/`Z` `IfcRotationalStiffnessSelect`) and `IfcBoundaryEdgeCondition` (the `LinearStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfTranslationalSubgradeReactionSelect` + `RotationalStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfRotationalSubgradeReactionSelect`), reducing each DOF through one four-arm type switch over GeometryGym's split select hierarchy (`IfcTranslationalStiffnessSelect` + the two subgrade-reaction selects derive `StiffnessSelect<TMeasure>`, `IfcRotationalStiffnessSelect` standalone — no common base unifies them, but each independently exposes a `Rigid` Boolean + a `Stiffness` measure whose `.Measure` rides `IfcDerivedMeasureValue`) onto ONE row per degree of freedom whose `PropertyValue` CASE carries the reading — a rigid or free DOF a `Boolean`, a DOF carrying a finite positive stiffness the SI spring `Measure` [H2] — the seam-declared `StructuralRows.Translation`/`Rotation` families keying them so the retired parallel `<dof>Stiffness` roster that split one fact across two rows is gone; the `Frame` reader stamping the `ConditionCoordinateSystem` `Axis`/`RefDirection` direction ratios as ONE `StructuralRows.Frame` positional list so a skewed support's restraint axes survive; the load arm discriminates the `AppliedLoad` over the `IfcStructuralLoadSingleForce`/`LinearForce`/`PlanarForce`/`Temperature`/`SingleDisplacement` family onto typed force/moment/pressure/temperature `MeasureValue` components — the 1D families sharing the consumer-neutral `ForceX..Z`/`MomentX..Z` names the `Rasm.Compute` `Vec(g, "Force")`/`Vec(g, "Moment")` reads take for point AND uniform actions, the family discriminated by the `LoadType` token and the per-component `Dimension` (N vs N/m), the `SingleDisplacement` settlement carried as frame attrs only (its components internal-field-only) — plus the neutral FE `LoadKind` token and the two-tier `ActionRow` (the specific `CaseSources` row over `IfcActionSourceTypeEnum`, else the group `ActionType` nature — `PERMANENT_G` to the dead permanent action, every other nature to the imposed variable action — so a prestress, shrinkage, or settlement group factors as a permanent action rather than silently mis-casing variable) carrying the `Case` token, the EN 1990 `ActionClass`, the project `ImposedLoadCategory`, and the `EurocodeAction` mint whose `IVariableCase` supplies `Psi0`/`Psi1`/`Psi2` beside the elected `EN.ITableA1_2` set's `GammaSup`/`GammaInf`, the `Attrs` egress `Filter` NaN-guarding the raw-sentinel surfaces (a `Temperature` `DeltaT_*`, a `SupportedLength`, a `Coefficient`, a 2D `DirectionRatioZ` unset read NaN and drop; a `Single`/`Linear`/`PlanarForce` component the public getter coerced to 0.0 emits a deliberate 0 the Filter cannot suppress); the group/case/result/model arms read the `IfcStructuralLoadGroup` `PredefinedType`/`ActionType`/`ActionSource`/`Coefficient`/`Purpose`, the `IfcStructuralLoadCase.SelfWeightCoefficients` gravity vector, the `IfcStructuralResultGroup` `TheoryType`/`IsLinear`/`ResultForLoadGroup`, and the `IfcStructuralAnalysisModel` `PredefinedType` (the `IfcAnalysisModelTypeEnum` loading-model type, stamped `ModelType` — the analysis THEORY lives on the result group) plus the `LoadedBy`/`HasResults` model→group JOINS as GlobalId `PropertyValue.List` payloads (direct set attributes no `IfcRel*` edge carries — a count would erase the wiring a multi-model file needs) and the `OrientationOf2DPlane` 2D loading plane through the same prefix-parameterized `Frame` reader (`PlaneAxisX..PlaneRefZ`) onto the structural definition bags, the member arms the `IfcStructuralCurveMember.Axis` local-axis direction ratios and the `IfcStructuralSurfaceMember.Thickness`.
- Receipt: the readers' payloads land on the ONE seam `ElementGraph` — the six-DOF restraint, frame, supported length, and `AtStart` on the `IfcRelConnectsStructuralMember` `Generic` edge, the applied load and `Station` on the `IfcRelConnectsStructuralActivity` `Generic` edge, and the load-group / load-case / result-group / analysis-model / member definitions on structural `PropertySet` nodes, the idealized analytical line riding the member `Object`'s `Axis`-keyed content hash in `Representations` — so the `Rasm.Compute` structural runner resolves the analytical line one-hop by content key, reads the support fixity-or-stiffness and the load components off the member's incident edges through the SAME `Rasm.Element` `StructuralRows` statics this reader stamps (`AtStart`, `Station`, `SupportedLength`, `Frame`, `LoadKind`, `Case`, and the `Translation`/`Rotation`/`Force`/`Moment` axis families) rather than a duplicated literal at either end, one DOF row carrying either the boolean restraint or the spring measure, and joins the section properties the `Graph/element#ELEMENT_GRAPH` `SectionOf` accessor bakes off the member's `ProfileSet` composition — resolved through the member's `Component` Type by the seam's one-hop type-resolved fallback (an occurrence with no own `ProfileSet` reads its `Element.Type` `Component`'s `SectionProperties`, the `Assign.TypeDefinition` inheritance the `Bake` fold applies), so an analytical member sharing a standardized cross-section reads it once off the deduped Type rather than per occurrence, the frozen Op-free `SectionOf(member)` signature untouched — the analysis owner producing the idealized graph, the solve and the typed `FrameModel` living wholly in `Rasm.Compute`, never re-projected here. A beam's analytical line, a slab's idealized thickness, a column-base node's six-DOF skewed support, a quarter-span point load, and a self-weight-vectored gravity case each ride the one graph the consumer already holds.
- Packages: GeometryGymIFC_Core, StructuralAnalysisFormat, VividOrange.Cases, VividOrange.Loads, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new boundary-condition kind is one arm on the `RestraintOf` switch reading the next `IfcBoundaryCondition` subtype's stiffness selects through the SAME `object`-pattern reducer; a new applied-load family is one arm on the `Vectors` switch reading the next `IfcStructuralLoad` subtype's components; a new structural entity or relationship with a definition bag is one arm on the polymorphic `Attrs` switch; a new action-source classification is one `CaseSources` row carrying its token, `ActionClass`, category, and mint (the `ActionType` nature tier already totalizing the residue); a new Eurocode action is one `EurocodeAction` row carrying its `ENLoadCaseFactory` mint; a new national deviation is a `NationalAnnex` value on the policy, never a per-country branch; a new degree of freedom or load axis is one `StructuralRows.Axes` entry at the seam owner every family absorbs; a new analytical-geometry kind is one more `RepresentationIdentifier` the `Projection/semantic#SEMANTIC_PROJECTOR` `IfcRepresentation.Keys` content-keyer maps into the member's `Representations`, resolved by content key downstream; never a per-member-type analysis record, never a `RestraintAttrs`/`LoadAttrs` sibling family, never a second analysis store, never a re-modeled analytical mesh, and never an inline analytical coordinate field on the node.
- Boundary: `StructuralProjection` produces ONLY neutral seam payloads — the migration `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint`/`StructuralLoadKind`/`StructuralCurveMemberKind` typed store is the deleted form, the idealized member being the seam `Object` node (an `IfcStructuralItem` IS an `IfcProduct` the general fold mints), its joint kind the `Object.PredefinedType` token, its topology the neutral `Connect`/`Generic` edges, and its restraint/load the typed `PropertyValue` edge payloads; the entity→bag reading is ONE polymorphic `Attrs` discriminating by input value and a `RestraintAttrs`/`LoadAttrs`/`GroupAttrs`/`ModelAttrs` sibling-method family is the deleted form; the `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` edge bag builds from ONE `Attrs(rel)` read and a caller-side bag-plus-manual-`Add` assembly is the deleted two-step; the 1D load components ride the consumer-neutral `ForceX..Z`/`MomentX..Z` wire names the `Rasm.Compute` `StructuralReads` accessors probe, and a per-family `LinearForceX`-style namespace that forks the uniform-load read onto silent zeros is the deleted form (the family discriminant is the `LoadType` token + the component `Dimension`, never the name); the structural reader is the DEEP half `SemanticProjector` composes and re-introducing it as a standalone `IElementProjection` (a second projector minting the member nodes the general fold already mints) is the deleted form; the analytical line rides the member `Object`'s `Axis`-keyed content hash in `RepresentationContentHash` (content-keyed at `ObjectNode` by `IfcRepresentation.Keys`, resolved one-hop by content key in `Rasm.Compute`), and an inline `AxisCurve`/`Vector3` analytical-coordinate field on the seam node — like a RhinoCommon `Curve`/`Brep` field or an in-process BRep tessellation — is the named §4-RT-M2 seam violation, the deleted form (the `AtStart`/`Station`/`Frame` topology reads are TRANSIENT, emitting only Boolean/scalar attributes); every row name this reader stamps resolves to an OWNER-declared static — the cross-package structural vocabulary through `Rasm.Element` `StructuralRows` and every remaining name through the owner-blessed empty-prefix `PropertyCategory.Seam.Row` — so a call-site `PropertyName.Create` anywhere in this reader is the deleted form that forks the key space between the Bim writer and its non-referencing `Rasm.Compute` reader, and a name a second package begins keying on is PROMOTED to `StructuralRows` at the Element owner rather than re-declared here; the restraint preserves the SI spring stiffness as a `MeasureValue` on the DOF's OWN row [H2], the `PropertyValue` case carrying restraint-versus-spring, and a parallel `<dof>Stiffness` roster beside the fixity row — the shape that strands the magnitude on every reader keying only the boolean — is the deleted form, as is a boolean-only fixity that drops the magnitude outright; every magnitude admits SI-NATIVE through `MeasureValue.OfSi(QuantityType, Dimension, double)` under the IFC MEASURE-TYPE name the `Projection/semantic#SEMANTIC_PROJECTOR` `MeasureDimensions` table keys (`IfcMomentOfInertiaMeasure`, `IfcMassPerLengthMeasure`; that table also carries the `IfcThermalResistanceMeasure` absent-from-GeometryGym negative, so a thermal-resistance read takes the `MeasureUnmapped` Text drop), and BINDING a UnitsNet quantity struct at this boundary is the deleted form on two independent counts — the registry ingress coerces through `ToUnit(UnitSystem.SI)`, which throws `No units were found for the given UnitSystem` for every quantity whose SI unit-info walk is empty (`LinearDensity`, `ThermalResistance`, `Mass`, `Density`, `Torque`, `HeatTransferCoefficient` among the majority of the registry), so the admission rails `ValueRejected` rather than landing a measure; and the `QuantityTypeConverter` wire is a culture-formatted abbreviation string (`1 kg/m`, `1 m²K/kW`) while the seam wire is the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter.Measure` byte run — the length-prefixed type token, the IEEE-754 SI magnitude, and the seven ordinal exponents — so the two currencies are incommensurable and neither reproduces the other. The measure-type NAME is the round-trip identity the `Projection/egress#IFC_EGRESS` `RaiseMeasure` mint derives from, so re-tokening it to a registry quantity name would fork every content key AND strand the raise table; the orientation frame is ONE `StructuralRows.Frame` positional list and a prefix-built `RestraintAxisX`/`PlaneRefZ` name family is the deleted form; the Eurocode regime is the `EurocodePolicy` VALUE (annex, elected Table A1.2 set, imposed category, snow altitude) and a per-country branch, a bare-`double` partial factor, a hand-tabulated psi set beside `ENLoadCaseFactory`, or a `MissingNationalAnnexException`/`NotImplementedException` propagating past the one `BimFault.CapabilityMiss` seam is the deleted form; an absent policy emits no factor row at all rather than a fabricated `NationalAnnex.RecommendedValues` factor; the load family is read over the full `IfcStructuralLoad` subtype set and a single-force-only reader is the deleted form; the `Case` derivation is total over `IfcActionSourceTypeEnum` through the two-tier source-row-then-`ActionType`-nature fold and a five-row map folding every permanent action to `live` is the deleted mis-casing; the member↔connection topology is the seam `Generic` edge (wire-name `IfcRelConnectsStructuralMember`) the `EdgeProjection.Structural` fold authors and a typed `MemberConnection` record is the deleted form; the content-key identity is the seam `ElementGraph` content address (the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`) the consumer reads the graph by, and minting a second `(GeometryKey, PropertyKey)` scheme or reaching the up-stratum `Rasm.Compute` `InterchangeIdentity` is the named cross-folder drift defect [H7]; the GeometryGym structural-analysis surface is consumed as settled vocabulary (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) and a hand-rolled structural-member reader is the deleted form; the reader is TOTAL and routing a structural enrichment onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the class/reference rails are the general fold's `Fin<GraphDelta>`).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.IO;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Projection;   // Attrs composes the ONE per-projection UnitScale the SEMANTIC_PROJECTOR owns
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using SAF.DataAccess.Contracts;
using SAF.DataAccess.Models;
using SAF.DataAccess.Models.Enums;
using Thinktecture;
using VividOrange.Loads;                  // ILoad — the empty action list the psi-factor mints take
using VividOrange.Loads.Cases;            // ActionClass, ImposedLoadCategory, IVariableCase, ENLoadCaseFactory, EN.ITableA1_2
using VividOrange.Standards.Eurocode;     // NationalAnnex, MissingNationalAnnexException
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record SafOp {
    private SafOp() { }

    public sealed record Import(Stream Workbook, Version TargetVersion) : SafOp;
    public sealed record Export(Stream Workbook, ExcelModel Model, Version TargetVersion) : SafOp;
}

// Which EN 1990 Annex A1.1 action a source classifies as, and the ONE ENLoadCaseFactory mint that answers its psi
// factors. The factory is the mint the package documents as pre-loading each action's psi set, so the row carries the
// call rather than a switch reading four factory names at a call site; an action with no EN factory carries no row and
// therefore no psi, which is the honest reading for a source the code does not tabulate. The
// imposed mint is category-keyed and yields None when the project declares no category, so a psi row is absent
// rather than defaulted onto whichever Category A-H the reader picked.
[SmartEnum<string>]
public sealed partial class EurocodeAction {
    public static readonly EurocodeAction Imposed = new("imposed", static policy =>
        policy.Imposed.Map(category => (IVariableCase)ENLoadCaseFactory.CreateImposed([], category, policy.Annex)));
    public static readonly EurocodeAction Snow = new("snow", static policy =>
        Some((IVariableCase)ENLoadCaseFactory.CreateSnow(policy.Annex, policy.AltitudeAbove1000m)));
    public static readonly EurocodeAction Thermal = new("thermal", static policy =>
        Some((IVariableCase)ENLoadCaseFactory.CreateThermal(policy.Annex)));
    public static readonly EurocodeAction Wind = new("wind", static policy =>
        Some((IVariableCase)ENLoadCaseFactory.CreateWind(policy.Annex)));

    [UseDelegateFromConstructor]
    public partial Option<IVariableCase> Mint(EurocodePolicy policy);
}

// --- [MODELS] ----------------------------------------------------------------------------- The
// Eurocode regime as ONE policy value: the national annex every psi/gamma lookup keys on, the EN 1990 Table A1.2
// partial-factor table the composition elected (Set A equilibrium, Set B member design, Set C geotechnical — the
// package ships one singleton per set and the choice is a design-situation decision, so the table arrives as a value
// this reader reads rather than a set the reader picks), the project's imposed-load category, and the snow-altitude
// discriminant its own factory takes. Absent policy means absent factors: the reader emits the IFC-declared attributes
// alone rather than stamping RecommendedValues nobody selected.
public readonly record struct EurocodePolicy(
    NationalAnnex Annex, EN.ITableA1_2 Partials, Option<ImposedLoadCategory> Imposed, bool AltitudeAbove1000m);

// One resolved action row: the consumer-neutral case token, the EN 1990 action nature the combination algebra factors
// under, the imposed category a Category A-H action carries, and the psi-factor mint. It replaces the bare token map —
// the token alone stranded every consumer re-deriving the nature it already knew and left the code factors unreachable.
internal readonly record struct ActionRow(
    string Case, ActionClass Class, Option<ImposedLoadCategory> Imposed, Option<EurocodeAction> Action);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class StructuralProjection {
    public static Fin<ExcelModel> Saf(
        SafOp operation,
        IExcelImportService imports,
        IExcelExportService exports,
        IExcelValidator validator,
        Op key) =>
        operation.Switch<Fin<ExcelModel>>(
            import: request => Try.lift(() => imports.Import(request.Workbook, request.TargetVersion)).Run()
                .MapFail(error => new BimFault.CodecReject(key, $"saf-import:{error.Message}"))
                .Bind(model => AdmitSaf(validator.ValidateForImport(model, request.TargetVersion, model.OriginalVersion), key)),
            export: request => AdmitSaf(validator.ValidateForExport(request.Model, request.TargetVersion, request.Model.OriginalVersion), key)
                .Bind(model => Try.lift(() => exports.Export(request.Workbook, model, request.TargetVersion, model.OriginalVersion)).Run()
                    .MapFail(error => new BimFault.CodecReject(key, $"saf-export:{error.Message}")))
                .Bind(result => result.IsSuccess
                    ? Fin.Succ(result.Model)
                    : Fin.Fail<ExcelModel>(new BimFault.ModelRejected(key, $"saf-export:{ExcelValidationResult.Format(result.ValidationResults)}"))));

    private static Fin<ExcelModel> AdmitSaf(ExcelModel model, Op key) =>
        model.ValidationErrors.Any(static error => error.Severity == ExcelValidationMessageSeverity.Error)
            ? Fin.Fail<ExcelModel>(new BimFault.ModelRejected(key, $"saf-validation:{ExcelValidationResult.Format(model.ValidationErrors)}"))
            : Fin.Succ(model);

    // The SI dimensions the structural measures stamp: the force-derived dimensions COMPOSE from the seam
    // Dimension.ForceDim/LengthDim/AreaDim algebra so a hand-coded exponent table never drifts from the quantity
    // registry — N.m (moment / node rotational stiffness), N/m (line force / node translational stiffness),
    // N/m^2 (planar force / edge line stiffness) — and the temperature delta is the SI base temperature dimension (K).
    private static readonly Dimension Moment = Dimension.ForceDim.Multiply(Dimension.LengthDim);
    private static readonly Dimension ForcePerLength = Dimension.ForceDim.Divide(Dimension.LengthDim);
    private static readonly Dimension ForcePerArea = Dimension.ForceDim.Divide(Dimension.AreaDim);
    private static readonly Dimension TemperatureDelta = Dimension.Create(0, 0, 0, 0, 1, 0, 0);

    private static readonly Seq<string> LoadKinds = Seq(
        "IfcStructuralLoadSingleForce", "IfcStructuralLoadLinearForce", "IfcStructuralLoadPlanarForce",
        "IfcStructuralLoadTemperature", "IfcStructuralLoadSingleDisplacement", "IfcStructuralLoadConfiguration");

    // The Enumerated allowed-sets are DERIVED from the GeometryGym enums (IfcLoadGroupTypeEnum carries
    // LOAD_COMBINATION_GROUP beyond the obvious four; IfcAnalysisTheoryTypeEnum the first/second/third-order +
    // full-nonlinear ladder; IfcAnalysisModelTypeEnum the in-plane/out-plane/3D loading split) so no roster
    // comment or hand-listed subset ever drifts from the schema.
    private static readonly Seq<string> LoadGroupKinds = Enum.GetNames<IfcLoadGroupTypeEnum>().ToSeq();
    private static readonly Seq<string> TheoryKinds = Enum.GetNames<IfcAnalysisTheoryTypeEnum>().ToSeq();
    private static readonly Seq<string> ModelKinds = Enum.GetNames<IfcAnalysisModelTypeEnum>().ToSeq();

    // The SPECIFIC tier of the two-tier load-CASE derivation Rasm.Compute factors under its LoadCombinationSpec:
    // the named permanent/climatic/seismic IfcActionSourceTypeEnum sources resolve to a ROW carrying the consumer's
    // closed dead/live/snow/wind/seismic token, the EN 1990 ActionClass nature the combination algebra factors under,
    // the imposed category where the action is category-keyed, and the psi-factor mint. The permanent-nature sources
    // (completion, prestress, settlement, shrinkage, creep, imperfection, lack-of-fit) land dead + Permanent so they
    // factor as permanent actions; the token alone was the thin slice that made every consumer re-derive the nature.
    // A source with no row falls to the NATURE tier through Nature, so the residue (temperature, fire, impact, wave,
    // brakes, ...) is the imposed variable action rather than a silently mis-cased permanent, and USERDEFINED/
    // NOTDEFINED sources classify identically.
    private static readonly Map<IfcActionSourceTypeEnum, ActionRow> CaseSources = toMap(Seq(
        (IfcActionSourceTypeEnum.DEAD_LOAD_G,        new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.COMPLETION_G1,      new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.PRESTRESSING_P,     new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SETTLEMENT_U,       new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SHRINKAGE,          new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.CREEP,              new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SYSTEM_IMPERFECTION, new ActionRow("dead",   ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.LACK_OF_FIT,        new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SNOW_S,             new ActionRow("snow",    ActionClass.Variable,   None, Some(EurocodeAction.Snow))),
        (IfcActionSourceTypeEnum.WIND_W,             new ActionRow("wind",    ActionClass.Variable,   None, Some(EurocodeAction.Wind))),
        (IfcActionSourceTypeEnum.EARTHQUAKE_E,       new ActionRow("seismic", ActionClass.Accidental, None, None))));

    // --- [ROW_NAMES] ---------------------------------------------------------------------------
    // Every row name the reader stamps resolves to an OWNER-declared static: the cross-package structural vocabulary
    // is the Rasm.Element StructuralRows roster (the Bim writer and the Rasm.Compute analysis reader are
    // non-referencing peers, so a literal at either end forks on the first rename), and every remaining row mints
    // through PropertyCategory.Seam.Row — the owner-blessed EMPTY-prefix category, which is what keeps a round-tripped
    // name bare while still routing custody through the seam declarer. A call-site PropertyName.Create anywhere in
    // this reader is the deleted form. A name a second package begins keying on is PROMOTED to StructuralRows at the
    // Element owner rather than re-declared here.
    private static readonly PropertyName LoadType = PropertyCategory.Seam.Row("LoadType");
    private static readonly PropertyName GlobalOrLocal = PropertyCategory.Seam.Row("GlobalOrLocal");
    private static readonly PropertyName Source = PropertyCategory.Seam.Row("Source");
    private static readonly PropertyName ActionClassRow = PropertyCategory.Seam.Row("ActionClass");
    private static readonly PropertyName ImposedCategory = PropertyCategory.Seam.Row("ImposedCategory");
    private static readonly PropertyName LoadGroupType = PropertyCategory.Seam.Row("LoadGroupType");
    private static readonly PropertyName ActionType = PropertyCategory.Seam.Row("ActionType");
    private static readonly PropertyName ActionSource = PropertyCategory.Seam.Row("ActionSource");
    private static readonly PropertyName Coefficient = PropertyCategory.Seam.Row("Coefficient");
    private static readonly PropertyName Purpose = PropertyCategory.Seam.Row("Purpose");
    private static readonly PropertyName AnalysisTheory = PropertyCategory.Seam.Row("AnalysisTheory");
    private static readonly PropertyName IsLinear = PropertyCategory.Seam.Row("IsLinear");
    private static readonly PropertyName ResultFor = PropertyCategory.Seam.Row("ResultFor");
    private static readonly PropertyName ModelType = PropertyCategory.Seam.Row("ModelType");
    private static readonly PropertyName LoadedBy = PropertyCategory.Seam.Row("LoadedBy");
    private static readonly PropertyName HasResults = PropertyCategory.Seam.Row("HasResults");
    private static readonly PropertyName Thickness = PropertyCategory.Seam.Row("Thickness");

    // The axis-indexed families the Element roster does not yet declare: the planar/temperature components and the
    // trapezoid ramp ends Rasm.Compute probes as Vec(g, "Start")/Vec(g, "End"). They generate off StructuralRows.Axes
    // so a seventh axis is unreachable by typo, and they are the PROMOTION candidates the moment the Element roster
    // widens — until then the empty-prefix category keeps their spellings byte-identical at both ends.
    private static readonly Map<string, PropertyName> PlanarForce = Family("PlanarForce");
    private static readonly Map<string, PropertyName> Start = Family("Start");
    private static readonly Map<string, PropertyName> End = Family("End");
    private static readonly Map<string, PropertyName> SelfWeight = Family("SelfWeight");
    private static readonly Map<string, PropertyName> LocalAxis = Family("LocalAxis");
    private static readonly Map<string, PropertyName> DeltaT = toMap(Seq(
        ("Constant", PropertyCategory.Seam.Row("DeltaTConstant")),
        ("Y", PropertyCategory.Seam.Row("DeltaTY")),
        ("Z", PropertyCategory.Seam.Row("DeltaTZ"))));

    // The EN 1990 factor rows: the three combination factors off the action's own minted case and the two permanent
    // partial factors off the elected Table A1.2 set, each a dimensionless Measure beside Coefficient so one consumer
    // read covers every factor on the bag.
    private static readonly Seq<PropertyName> PsiRows = Seq(
        PropertyCategory.Seam.Row("Psi0"), PropertyCategory.Seam.Row("Psi1"), PropertyCategory.Seam.Row("Psi2"));
    private static readonly PropertyName GammaSup = PropertyCategory.Seam.Row("GammaSup");
    private static readonly PropertyName GammaInf = PropertyCategory.Seam.Row("GammaInf");

    private static Map<string, PropertyName> Family(string stem) =>
        StructuralRows.Axes.Fold(Map<string, PropertyName>(), (map, axis) => map.Add(axis, PropertyCategory.Seam.Row($"{stem}{axis}")));

    // --- [ATTRIBUTES] -------------------------------------------------------------------------
    // ONE polymorphic structural attribute-bag reader discriminating on the entity shape — never a RestraintAttrs/
    // LoadAttrs/GroupAttrs sibling family. The two IfcRelConnects* arms build the WHOLE Generic edge payload in one
    // call (restraint + frame + supported length + AtStart; load + Station) so EdgeProjection.Structural reads
    // Attrs(rel) once; a connection's/activity's own bag serves the entity-level enrichment; a load-group /
    // load-case / result-group / analysis-model / member definition rides a structural PropertySet node. A
    // non-structural or null entity yields the empty bag (a graceful skip). The egress Filter drops every
    // non-finite Measure so the surfaces whose public getter exposes the unset NaN sentinel never emit: DeltaT_*
    // (raw auto-property), Coefficient, Thickness, SupportedLength, and a 2D direction's DirectionRatioZ all read
    // NaN unset and drop here. The IfcStructuralLoad force families (Single/Linear/Planar) are NOT in that set —
    // GeometryGym 25.7.30 backs each force/moment component with a NaN field but the public getter COERCES unset
    // NaN -> 0.0, so an unset force component reads a deliberate 0 (a zero force, harmless to the FE consumer)
    // the Filter cannot distinguish from a real 0; the drop is a NaN guard over the raw-sentinel surfaces, not a
    // universal unset-component eliminator.
    public static Fin<Map<PropertyName, PropertyValue>> Attrs(BaseClassIfc? entity, Op key, Option<EurocodePolicy> eurocode = default) {
        UnitScale scale = entity?.Database is { } database ? UnitScale.Of(database) : UnitScale.Si;
        return entity switch {
            IfcRelConnectsStructuralMember relation =>
                from restraint in RestraintOf(relation.AppliedCondition ?? relation.RelatedStructuralConnection?.AppliedCondition, scale, key)
                from frame in Frame(relation.ConditionCoordinateSystem, key)
                from length in Measures(Seq((StructuralRows.SupportedLength, Dimension.LengthDim, relation.SupportedLength * scale.Factor(Dimension.LengthDim))), key)
                select restraint.AddRange(frame).AddRange(length).AddRange(
                    AtStart(relation.RelatingStructuralMember as IfcStructuralCurveMember, relation.RelatedStructuralConnection)
                        .Map(static atStart => (StructuralRows.AtStart, (PropertyValue)new PropertyValue.Boolean(atStart))).ToSeq()),
            IfcRelConnectsStructuralActivity relation =>
                from load in LoadOf(relation.RelatedStructuralActivity, scale, eurocode, key)
                from station in Measures(Station(relation.RelatingElement as IfcStructuralCurveMember, relation.RelatedStructuralActivity)
                    .Map(static value => (StructuralRows.Station, Dimension.Dimensionless, value)).ToSeq(), key)
                select load.AddRange(station),
            IfcStructuralConnection connection =>
                from restraint in RestraintOf(connection.AppliedCondition, scale, key)
                from frame in Frame((connection as IfcStructuralPointConnection)?.ConditionCoordinateSystem, key)
                select restraint.AddRange(frame),
            IfcStructuralActivity activity => LoadOf(activity, scale, eurocode, key),
            IfcStructuralLoadCase loadCase =>
                from group in GroupOf(loadCase, key)
                from weight in Measures(Optional(loadCase.SelfWeightCoefficients).ToSeq().Bind(static vector => Seq(
                    (SelfWeight["X"], Dimension.Dimensionless, vector.Item1),
                    (SelfWeight["Y"], Dimension.Dimensionless, vector.Item2),
                    (SelfWeight["Z"], Dimension.Dimensionless, vector.Item3))), key)
                select group.AddRange(weight),
            IfcStructuralLoadGroup group => GroupOf(group, key),
            IfcStructuralResultGroup result => Fin.Succ(Map(
                (AnalysisTheory, Enumerated(result.TheoryType.ToString(), TheoryKinds)),
                (IsLinear, (PropertyValue)new PropertyValue.Boolean(result.IsLinear)))
                .AddRange(Optional(result.ResultForLoadGroup)
                    .Map(static loadGroup => (ResultFor, (PropertyValue)new PropertyValue.Text(loadGroup.GlobalId))).ToSeq())),
            IfcStructuralAnalysisModel model =>
                from frame in Frame(model.OrientationOf2DPlane, key)
                select Seq(
                        (LoadedBy, toSeq(model.LoadedBy).Map(static group => group.GlobalId)),
                        (HasResults, toSeq(model.HasResults).Map(static result => result.GlobalId)))
                    .Filter(static join => !join.Item2.IsEmpty)
                    .Fold(Map((ModelType, Enumerated(model.PredefinedType.ToString(), ModelKinds))),
                        static (map, join) => map.Add(join.Item1,
                            new PropertyValue.List(join.Item2.Map(static id => (PropertyValue)new PropertyValue.Text(id)))))
                    .AddRange(frame),
            IfcStructuralCurveMember member => Measures(Optional(member.Axis).ToSeq().Bind(static axis => Seq(
                (LocalAxis["X"], Dimension.Dimensionless, axis.DirectionRatioX),
                (LocalAxis["Y"], Dimension.Dimensionless, axis.DirectionRatioY),
                (LocalAxis["Z"], Dimension.Dimensionless, axis.DirectionRatioZ))), key),
            IfcStructuralSurfaceMember surface => Measures(
                Seq((Thickness, Dimension.LengthDim, surface.Thickness * scale.Factor(Dimension.LengthDim))), key),
            _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
        };
    }

    // --- [RESTRAINT] -------------------------------------------------------------------------- The
    // six-DOF support condition: a fixity Boolean PLUS the SI spring-stiffness magnitude per DOF [H2], so
    // Rasm.Compute reads BOTH a pinned/fixed support and a finite spring off the edge (the prior boolean-only
    // reader dropped the stiffness). A node condition reads its 6 stiffness selects, an edge condition its 6
    // by-length selects; the four select types each expose a Rigid/Stiffness shape (no shared base — see Dof) so
    // the per-DOF type switch reduces every DOF. Takes the CONDITION (not the connection) so the rel-level
    // member-end release and the connection's own support reduce through one reader. An absent condition — or a
    // face condition, whose area-stiffness GeometryGym 25.7.30 exposes ONLY as internal fields (no public
    // properties) — yields the empty (free) bag.
    private static Fin<Map<PropertyName, PropertyValue>> RestraintOf(IfcBoundaryCondition? condition, UnitScale scale, Op key) => condition switch {
        IfcBoundaryNodeCondition n => SixDof(
            (n.TranslationalStiffnessX, n.TranslationalStiffnessY, n.TranslationalStiffnessZ),
            (n.RotationalStiffnessX, n.RotationalStiffnessY, n.RotationalStiffnessZ),
            ForcePerLength, Moment, scale, key),
        IfcBoundaryEdgeCondition e => SixDof(
            (e.LinearStiffnessByLengthX, e.LinearStiffnessByLengthY, e.LinearStiffnessByLengthZ),
            (e.RotationalStiffnessByLengthX, e.RotationalStiffnessByLengthY, e.RotationalStiffnessByLengthZ),
            ForcePerArea, Dimension.ForceDim, scale, key),
        _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
    };

    // The local orientation frame as ONE StructuralRows.Frame row carrying the six Axis/RefDirection direction ratios
    // in declared order (AxisX, AxisY, AxisZ, RefX, RefY, RefZ) — the owner-declared frame row both the connection's
    // skewed ConditionCoordinateSystem (an inclined roller's DOF axes) and the analysis model's OrientationOf2DPlane
    // land on, so the retired prefix-parameterized RestraintAxisX/PlaneRefZ name family is gone with the string-built
    // spellings it forced. The two frames never co-occupy one bag (a restraint frame rides a connection or rel bag, a
    // plane frame the analysis-model bag), so one row carries either. A global-axes placement (absent, or partial)
    // emits nothing rather than a fabricated frame, a non-finite ratio drops the WHOLE row rather than poisoning a
    // positional list, and the ratios are attribute data, never the content-keyed analytical geometry.
    private static Fin<Map<PropertyName, PropertyValue>> Frame(IfcAxis2Placement3D? system, Op key) =>
        system is { Axis: { } axis, RefDirection: { } reference }
        && Seq(axis.DirectionRatioX, axis.DirectionRatioY, axis.DirectionRatioZ,
               reference.DirectionRatioX, reference.DirectionRatioY, reference.DirectionRatioZ) is var ratios
        && ratios.ForAll(double.IsFinite)
            ? ratios.TraverseM(ratio => PropertyValue.Of(new PropertyValue.Number(ratio), key)).As()
                .Map(values => Map((StructuralRows.Frame, (PropertyValue)new PropertyValue.List(values))))
            : Fin.Succ(Map<PropertyName, PropertyValue>());

    // ONE row per degree of freedom, its PropertyValue CASE carrying whether the support is a rigid restraint or a
    // finite spring — the seam's own custody law, so a reader keying the DOF never has to know a parallel Kx roster
    // exists and can never read the boolean while stranding the magnitude. A rigid or free DOF stamps Boolean, a DOF
    // carrying a finite positive stiffness stamps the SI Measure [H2]; the retired TranslationX + TranslationKx pair
    // was the twin that split one fact across two rows.
    private static Fin<Map<PropertyName, PropertyValue>> SixDof(
        (object? X, object? Y, object? Z) translation, (object? X, object? Y, object? Z) rotation,
        Dimension translationDim, Dimension rotationDim, UnitScale scale, Op key) =>
        Seq((StructuralRows.Translation["X"], translation.X, translationDim),
            (StructuralRows.Translation["Y"], translation.Y, translationDim),
            (StructuralRows.Translation["Z"], translation.Z, translationDim),
            (StructuralRows.Rotation["X"],    rotation.X,    rotationDim),
            (StructuralRows.Rotation["Y"],    rotation.Y,    rotationDim),
            (StructuralRows.Rotation["Z"],    rotation.Z,    rotationDim))
            .TraverseM(degree => Dof(degree.Item2, degree.Item3, scale) switch {
                (_, var spring) when double.IsFinite(spring) && spring > 0d =>
                    MeasureValue.OfSi(degree.Item3, spring)
                        .MapFail(_ => ElementFault.ValueRejected(key, $"<structural-measure:{degree.Item1}:{spring:R}>"))
                        .Map(value => (Name: degree.Item1, Value: (PropertyValue)new PropertyValue.Measure(value))),
                (var fixity, _) => FinSucc((Name: degree.Item1, Value: (PropertyValue)new PropertyValue.Boolean(fixity))),
            })
            .As()
            .Map(static rows => rows.Fold(Map<PropertyName, PropertyValue>(), static (map, row) => map.Add(row.Name, row.Value)));

    // ONE reading per DOF select: the fixity Boolean AND the SI spring magnitude from one four-arm type switch over
    // GeometryGym's SPLIT select hierarchy (IfcTranslationalStiffnessSelect + the two subgrade-reaction selects derive
    // StiffnessSelect<TMeasure>; IfcRotationalStiffnessSelect is standalone) — no common base unifies them, so a single
    // property pattern is impossible, but all four independently expose a Rigid Boolean + a Stiffness measure whose
    // .Measure rides IfcDerivedMeasureValue, so the prior Fixity/SpringOf split that pattern-matched every DOF twice
    // collapses to one reader. A DOF is fixed when rigid OR carrying a finite positive spring; the magnitude is 0 for a
    // rigid or free DOF and the model-NATIVE spring coerced to SI by the UnitScale dimensional factor otherwise [H2]
    // (a NaN stiffness reads free, dropped at the Attrs egress).
    private static (bool Fixity, double Spring) Dof(object? select, Dimension dimension, UnitScale scale) {
        (bool Rigid, double Native) reading = select switch {
            IfcTranslationalStiffnessSelect s                 => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcRotationalStiffnessSelect s                    => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcModulusOfTranslationalSubgradeReactionSelect s => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcModulusOfRotationalSubgradeReactionSelect s    => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            _                                                 => (false, 0d),
        };
        return (reading.Rigid || reading.Native > 0d, reading.Rigid ? 0d : reading.Native * scale.Factor(dimension));
    }

    private static Fin<Map<PropertyName, PropertyValue>> Measures(
        Seq<(PropertyName Name, Dimension Dimension, double Si)> rows,
        Op key) =>
        rows.Filter(static row => double.IsFinite(row.Si))
            .TraverseM(row => MeasureValue.OfSi(row.Dimension, row.Si)
                .MapFail(_ => ElementFault.ValueRejected(key, $"<structural-measure:{row.Name}:{row.Si:R}>"))
                .Map(value => (Name: row.Name, Value: (PropertyValue)new PropertyValue.Measure(value))))
            .As()
            .Map(static admitted => admitted.Fold(
                Map<PropertyName, PropertyValue>(),
                static (map, row) => map.Add(row.Name, row.Value)));

    private static PropertyValue Enumerated(string selected, Seq<string> allowed) =>
        new PropertyValue.Enumerated(
            Seq<PropertyValue>(new PropertyValue.Text(selected)),
            allowed.Map(static value => (PropertyValue)new PropertyValue.Text(value)));

    // The reader's inverse the Projection/egress#IFC_EGRESS Emit composes over the authored structural entities:
    // the node-level AppliedCondition (6-DOF fixity + SI springs) and the AppliedLoad single-force components
    // re-stamp off the StructuralDefinition bag the ingest Attrs lowered — the [RELATIONSHIP_REEMIT] named drop
    // this closes. The family discriminant is the ingest's own LoadType token — the ForceX..Z wire names are
    // family-SHARED across the 1D loads, so a token-blind ForceX gate would re-author every uniform line action
    // as a fabricated point force. The egress target database is SI by construction, so the bag's SI magnitudes
    // land verbatim (no inverse UnitScale fold exists to get wrong; ctors + settable columns decompile-verified).
    // TOTAL and residue-HONEST: the return is the bag rows the re-stamp did NOT consume, so a payload with no
    // verified re-author ctor (a line/planar/temperature action, a trapezoid configuration, a displacement)
    // stays VISIBLE at the owning boundary as the typed fidelity residue Emit accumulates into the exchange
    // evidence — never a silently partial inverse behind a total void surface.
    public static Map<PropertyName, PropertyValue> Author(DatabaseIfc db, IfcObjectDefinition entity, Map<PropertyName, PropertyValue> attrs) =>
        entity switch {
            IfcStructuralConnection connection when attrs.ContainsKey(StructuralRows.Translation["X"]) =>
                Consume(attrs, StructuralRows.Dofs, () => connection.AppliedCondition = new IfcBoundaryNodeCondition(db, "",
                    Translational(attrs, StructuralRows.Translation["X"]),
                    Translational(attrs, StructuralRows.Translation["Y"]),
                    Translational(attrs, StructuralRows.Translation["Z"]),
                    Rotational(attrs, StructuralRows.Rotation["X"]),
                    Rotational(attrs, StructuralRows.Rotation["Y"]),
                    Rotational(attrs, StructuralRows.Rotation["Z"]))),
            IfcStructuralActivity activity when LoadTypeOf(attrs) == nameof(IfcStructuralLoadSingleForce) =>
                Consume(attrs, ForceNames, () => activity.AppliedLoad = new IfcStructuralLoadSingleForce(db,
                        Si(attrs, StructuralRows.Force["X"]), Si(attrs, StructuralRows.Force["Y"]), Si(attrs, StructuralRows.Force["Z"])) {
                    MomentX = Si(attrs, StructuralRows.Moment["X"]),
                    MomentY = Si(attrs, StructuralRows.Moment["Y"]),
                    MomentZ = Si(attrs, StructuralRows.Moment["Z"]),
                }),
            _ => attrs,
        };

    // Consumed names = the stamped components plus the family discriminant; the frame tokens (LoadKind/Case/
    // ActionClass/GlobalOrLocal/Source) re-derive at the next ingest and never count as drops, and the Eurocode factors
    // re-resolve from the annex policy rather than round-tripping. StructuralRows.Dofs IS the six restraint rows, so
    // the retired twelve-name fixity+stiffness roster has no counterpart to re-mint. The stamp Action is the
    // GG-authoring mutation seam, confined here.
    private static readonly Seq<PropertyName> ForceNames =
        StructuralRows.Force.Values.ToSeq() + StructuralRows.Moment.Values.ToSeq() + Seq(LoadType);

    private static Map<PropertyName, PropertyValue> Consume(Map<PropertyName, PropertyValue> attrs, Seq<PropertyName> names, Action stamp) {
        stamp();
        return names.Fold(attrs, static (residue, name) => residue.Remove(name));
    }

    private static string LoadTypeOf(Map<PropertyName, PropertyValue> attrs) =>
        attrs.Find(LoadType)
            .Bind(static value => value is PropertyValue.Enumerated enumerated ? enumerated.Selected.Head : None)
            .Bind(static selected => selected is PropertyValue.Text text ? Some(text.Value) : None)
            .IfNone("");

    // A DOF select off the ONE bag row — the Dof reading's inverse over the collapsed shape: a Measure row re-stamps
    // its SI stiffness through the double ctor, a true Boolean row re-stamps rigid, and an absent or false row is free.
    private static IfcTranslationalStiffnessSelect Translational(Map<PropertyName, PropertyValue> attrs, PropertyName dof) =>
        Si(attrs, dof) is > 0d and var k ? new IfcTranslationalStiffnessSelect(k) : new IfcTranslationalStiffnessSelect(Fixity(attrs, dof));

    private static IfcRotationalStiffnessSelect Rotational(Map<PropertyName, PropertyValue> attrs, PropertyName dof) =>
        Si(attrs, dof) is > 0d and var k ? new IfcRotationalStiffnessSelect(k) : new IfcRotationalStiffnessSelect(Fixity(attrs, dof));

    private static double Si(Map<PropertyName, PropertyValue> attrs, PropertyName name) =>
        attrs.Find(name).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None).IfNone(0d);

    private static bool Fixity(Map<PropertyName, PropertyValue> attrs, PropertyName name) =>
        attrs.Find(name).Exists(static v => v is PropertyValue.Boolean { Value: true });

    // --- [LOAD] ------------------------------------------------------------------------------- The
    // applied load the IfcRelConnectsStructuralActivity Generic edge carries: typed force/moment/pressure/
    // temperature MeasureValue components over the IfcStructuralLoad family PLUS the load-type token, the
    // global/local frame, and the source name (the prior single-force-only reader dropped the line/planar/
    // temperature families). IfcStructuralLoadSingleDisplacement, whose components are internal-field-only, yields
    // the frame attrs only, a graceful passthrough; a Temperature DeltaT_* unset = NaN drops at the Attrs egress,
    // while a force/moment component the public getter coerced to 0.0 emits a 0 (the getter masks the unset
    // sentinel, so the Filter NaN-guards the raw-sentinel surfaces and never suppresses a coerced 0). Every
    // component is model-NATIVE and coerces to SI through the UnitScale dimensional factor at the fold.
    private static Fin<Map<PropertyName, PropertyValue>> LoadOf(IfcStructuralActivity? activity, UnitScale scale, Option<EurocodePolicy> eurocode, Op key) =>
        Optional(activity).Bind(static candidate => Optional(candidate.AppliedLoad).Map(load => (Activity: candidate, Load: load))).Match(
            Some: pair => {
                ActionRow row = RowOf(pair.Activity, eurocode);
                return from measures in Measures(Vectors(pair.Load).Map(vector =>
                           (vector.Name, vector.Dim, vector.Native * scale.Factor(vector.Dim))), key)
                       from factors in Factors(row, eurocode, key)
                       select Map(
                           (LoadType, Enumerated(pair.Load.GetType().Name, LoadKinds)),
                           (StructuralRows.LoadKind, (PropertyValue)new PropertyValue.Text(KindOf(pair.Load))),
                           (StructuralRows.Case, new PropertyValue.Text(row.Case)),
                           (ActionClassRow, new PropertyValue.Text(row.Class.ToString())),
                           (GlobalOrLocal, new PropertyValue.Text(pair.Activity.GlobalOrLocal.ToString())),
                           (Source, new PropertyValue.Text(pair.Activity.Name ?? "")))
                           .AddRange(row.Imposed.Map(static category => (ImposedCategory, (PropertyValue)new PropertyValue.Text(category.ToString()))).ToSeq())
                           .AddRange(measures)
                           .AddRange(factors);
            },
            None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()));

    // The Rasm.Compute FE idealization kind (point/uniform/trapezoid) the IfcStructuralLoad class lowers onto, stamped
    // ALONGSIDE the faithful IFC LoadType: a single force is a point action, a linear force a uniform line action, and
    // the IFC varying line action — IfcStructuralLoadConfiguration, public `Values`/`Locations` decompile-verified —
    // is the trapezoid the Analysis/structural ToLoad Vec(g, "Start")/Vec(g, "End") probes read.
    private static string KindOf(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadConfiguration => "trapezoid",
        IfcStructuralLoadLinearForce   => "uniform",
        _                              => "point",
    };

    // The two-tier neutral load-ACTION row walked off the activity's IfcStructuralLoadGroup assignment
    // (HasAssignments -> IfcRelAssignsToGroup.RelatingGroup -> IfcStructuralLoadGroup): the SPECIFIC CaseSources row
    // over ActionSource first, else the NATURE tier off the group's ActionType — so a prestress or shrinkage group
    // factors permanent and a temperature or impact group factors variable under the consumer's LoadCombinationSpec.
    // An ungrouped activity takes the same variable nature row, the unfactored case the consumer defaults.
    private static ActionRow RowOf(IfcStructuralActivity activity, Option<EurocodePolicy> eurocode) =>
        activity.HasAssignments.AsIterable()
            .Choose(static a => a is IfcRelAssignsToGroup { RelatingGroup: IfcStructuralLoadGroup g } ? Some(g) : None)
            .ToSeq().Head
            .Map(g => CaseSources.Find(g.ActionSource).IfNone(() => Nature(g.ActionType, eurocode)))
            .IfNone(() => Nature(IfcActionTypeEnum.VARIABLE_Q, eurocode));

    // The NATURE tier: a permanent group is the dead permanent action carrying no psi, and every other nature is the
    // IMPOSED variable action — which is where the project's own ImposedLoadCategory lands, because EN 1990 keys the
    // imposed psi set by category and the IFC source vocabulary carries no category of its own. Absent a policy the
    // row keeps its token and nature and carries no category and no mint, so the reader never invents one.
    private static ActionRow Nature(IfcActionTypeEnum nature, Option<EurocodePolicy> eurocode) =>
        nature == IfcActionTypeEnum.PERMANENT_G
            ? new ActionRow("dead", ActionClass.Permanent, None, None)
            : new ActionRow("live", ActionClass.Variable, eurocode.Bind(static policy => policy.Imposed),
                eurocode.Bind(static policy => policy.Imposed).Map(static _ => EurocodeAction.Imposed));

    // The EN 1990 Annex A1 factor rows: psi0/psi1/psi2 read off the case the action's own ENLoadCaseFactory mint
    // pre-loads (never a hand-tabulated psi set beside the package that owns the tables) and gammaG,sup/gammaG,inf off
    // the composition's elected Table A1.2 set, both as dimensionless Measures beside Coefficient so one consumer read
    // covers every factor on the bag. The EN singletons throw for an uncovered annex and the standards kernel throws
    // MissingNationalAnnexException, so both cross into the Fin rail as BimFault.CapabilityMiss at this one seam and
    // never propagate into the fold; an absent policy yields no rows at all.
    private static Fin<Map<PropertyName, PropertyValue>> Factors(ActionRow row, Option<EurocodePolicy> eurocode, Op key) =>
        eurocode.Match(
            None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()),
            Some: policy => Try.lift(() => row.Action.Bind(action => action.Mint(policy)).Match(
                    Some: variable => PsiRows.Zip(Seq(
                        variable.CombinationFactor.DecimalFractions,
                        variable.FrequentFactor.DecimalFractions,
                        variable.QuasiPermanentFactor.DecimalFractions)),
                    None: static () => Seq<(PropertyName, double)>())
                + Partials(policy)).Run()
                .MapFail(error => new BimFault.CapabilityMiss(key, $"eurocode-factors:{policy.Annex}:{error.Message}"))
                .Bind(rows => Measures(rows.Map(static factor => (factor.Item1, Dimension.Dimensionless, factor.Item2)), key)));

    private static Seq<(PropertyName, double)> Partials(EurocodePolicy policy) {
        EN.TableA1_2Properties gamma = policy.Partials.GetProperties(policy.Annex);
        return Seq((GammaSup, gamma.Gamma_Gsup.DecimalFractions), (GammaInf, gamma.Gamma_Ginf.DecimalFractions));
    }

    // The load-group definition bag the LoadCase arm extends: the combination/case/group discriminant, the action
    // nature and source, the partial-safety Coefficient (NaN unset, dropped at egress), and the purpose label.
    private static Fin<Map<PropertyName, PropertyValue>> GroupOf(IfcStructuralLoadGroup group, Op key) =>
        Measures(Seq((Coefficient, Dimension.Dimensionless, group.Coefficient)), key)
            .Map(measures => Map(
                (LoadGroupType, Enumerated(group.PredefinedType.ToString(), LoadGroupKinds)),
                (ActionType, (PropertyValue)new PropertyValue.Text(group.ActionType.ToString())),
                (ActionSource, new PropertyValue.Text(group.ActionSource.ToString())),
                (Purpose, new PropertyValue.Text(group.Purpose ?? "")))
                .AddRange(measures));

    // The 1D families share the consumer-neutral ForceX..Z / MomentX..Z names — the exact wire the Rasm.Compute
    // StructuralReads Vec(g, "Force")/Vec(g, "Moment") probes take for point AND uniform actions — the family
    // discriminated by the LoadType token and the per-component Dimension (a point force N, a line force N/m, a
    // line moment N·m/m = N); a per-family LinearForceX-style namespace forked the uniform read onto silent zeros.
    private static Seq<(PropertyName Name, double Native, Dimension Dim)> Vectors(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadSingleForce f => Seq(
            (StructuralRows.Force["X"], f.ForceX, Dimension.ForceDim), (StructuralRows.Force["Y"], f.ForceY, Dimension.ForceDim), (StructuralRows.Force["Z"], f.ForceZ, Dimension.ForceDim),
            (StructuralRows.Moment["X"], f.MomentX, Moment), (StructuralRows.Moment["Y"], f.MomentY, Moment), (StructuralRows.Moment["Z"], f.MomentZ, Moment)),
        IfcStructuralLoadLinearForce l => Seq(
            (StructuralRows.Force["X"], l.LinearForceX, ForcePerLength), (StructuralRows.Force["Y"], l.LinearForceY, ForcePerLength), (StructuralRows.Force["Z"], l.LinearForceZ, ForcePerLength),
            (StructuralRows.Moment["X"], l.LinearMomentX, Dimension.ForceDim), (StructuralRows.Moment["Y"], l.LinearMomentY, Dimension.ForceDim), (StructuralRows.Moment["Z"], l.LinearMomentZ, Dimension.ForceDim)),
        IfcStructuralLoadPlanarForce p => Seq(
            (PlanarForce["X"], p.PlanarForceX, ForcePerArea), (PlanarForce["Y"], p.PlanarForceY, ForcePerArea), (PlanarForce["Z"], p.PlanarForceZ, ForcePerArea)),
        IfcStructuralLoadTemperature t => Seq(
            (DeltaT["Constant"], t.DeltaT_Constant, TemperatureDelta), (DeltaT["Y"], t.DeltaT_Y, TemperatureDelta), (DeltaT["Z"], t.DeltaT_Z, TemperatureDelta)),
        // The IFC varying line action: IfcStructuralLoadConfiguration (public Values/Locations, decompile-verified)
        // carrying positioned linear forces — the first/last rows lower onto the trapezoid wire (StartX..Z/EndX..Z)
        // the Rasm.Compute Vec(g, "Start")/Vec(g, "End") probes read; a single-row or non-linear-force configuration
        // falls through to the graceful passthrough, never a fabricated ramp.
        IfcStructuralLoadConfiguration cfg when cfg.Values.OfType<IfcStructuralLoadLinearForce>().ToSeq() is { Count: >= 2 } ramp => Seq(
            (Start["X"], ramp[0].LinearForceX, ForcePerLength), (Start["Y"], ramp[0].LinearForceY, ForcePerLength), (Start["Z"], ramp[0].LinearForceZ, ForcePerLength),
            (End["X"], ramp[ramp.Count - 1].LinearForceX, ForcePerLength), (End["Y"], ramp[ramp.Count - 1].LinearForceY, ForcePerLength), (End["Z"], ramp[ramp.Count - 1].LinearForceZ, ForcePerLength)),
        // IfcStructuralLoadSingleDisplacement holds its DisplacementX/Y/Z + RotationalDisplacementRX/RY/RZ as INTERNAL
        // fields in GeometryGym 25.7.30 — NO public accessor crosses the assembly boundary — so a prescribed-displacement
        // (support-settlement) load reads the frame attrs only (LoadType/LoadKind/Case/GlobalOrLocal/Source via LoadOf),
        // the documented surface boundary rather than a phantom `d.DisplacementX` read or a silently-invented 0-settlement;
        // the `_` graceful passthrough owns it alongside any unenumerated load family, never a fabricated component.
        _ => Seq<(PropertyName, double, Dimension)>(),
    };

    // --- [TOPOLOGY_DISCRIMINANTS] ---------------------------------------------------------------
    // The start/end discriminant the IfcRelConnectsStructuralMember Generic edge carries so Rasm.Compute resolves a
    // support to the correct member joint: the point connection's vertex compared to the member's analytical-edge
    // endpoints (nearer Start -> true). The endpoint coordinates are read TRANSIENTLY off GeometryGym topology to
    // compute the boolean — never stored on the seam node (the analytical line itself rides the Axis-keyed content
    // hash in Representations). A member with no analytical edge, a connection with no vertex, or a malformed
    // vertex point folds to the end joint (false — the consumer's WireAtStart default), so an unresolved endpoint
    // never silently claims the start and never compares against a fabricated origin.
    public static Option<bool> AtStart(IfcStructuralCurveMember? member, IfcStructuralConnection? connection) =>
        from m in Optional(member)
        from edge in EdgeOf(m.Representation)
        from vertex in PointOf((connection as IfcStructuralPointConnection)?.Vertex)
        from s in PointOf(edge.EdgeStart)
        from e in PointOf(edge.EdgeEnd)
        select Vector3.Distance(vertex, s) <= Vector3.Distance(vertex, e);

    // The normalized point-action position along the member's analytical edge (0 start .. 1 end): the activity's
    // topology vertex projected onto the start-end chord, the SAME transient read discipline AtStart holds. None
    // when any topology is absent/malformed or the chord degenerate — a surface action or an unpositioned load
    // never fabricates a station, and the consumer's absent-default (midspan 0.5) stays the honest fallback.
    public static Option<double> Station(IfcStructuralCurveMember? member, IfcStructuralActivity? activity) =>
        from m in Optional(member)
        from edge in EdgeOf(m.Representation)
        from v in VertexOf(activity?.Representation)
        from s in PointOf(edge.EdgeStart)
        from e in PointOf(edge.EdgeEnd)
        let chord = Vector3.Dot(e - s, e - s)
        where chord > 0d
        select Math.Clamp(Vector3.Dot(v - s, e - s) / chord, 0d, 1d);

    // The member's analytical topology edge / the activity's position vertex, read transiently off the inherited
    // IfcProduct.Representation for the AtStart/Station compares ONLY — the coordinates produce the Boolean/scalar
    // discriminant and are never carried onto a seam node.
    private static Option<IfcEdge> EdgeOf(IfcProductDefinitionShape? shape) =>
        Optional(shape).Bind(static s => s.Representations.AsIterable()
            .SelectMany(static rep => rep.Items.AsIterable())
            .Choose(static item => item is IfcEdge e ? Some(e) : None)
            .ToSeq().Head);

    private static Option<Vector3> VertexOf(IfcProductDefinitionShape? shape) =>
        Optional(shape).Bind(static s => s.Representations.AsIterable()
            .SelectMany(static rep => rep.Items.AsIterable())
            .Choose(static item => item is IfcVertexPoint vp ? PointOf(vp) : None)
            .ToSeq().Head);

    // 2D-honest: an IN_PLANE analytical model's IfcCartesianPoint legally carries TWO coordinates, so a 2-coordinate
    // vertex reads (x, y, 0) rather than collapsing every plane-frame joint onto a fabricated origin; a point with
    // fewer coordinates is malformed and yields None — the callers' honest fallbacks own it.
    private static Option<Vector3> PointOf(IfcVertex? vertex) =>
        vertex is IfcVertexPoint { VertexGeometry: IfcCartesianPoint { Coordinates: { Count: >= 2 } c } }
            ? Some(new Vector3(c[0], c[1], c.Count >= 3 ? c[2] : 0d))
            : None;
}
```
