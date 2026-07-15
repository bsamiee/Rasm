# [STRUCTURAL_PROJECTION]

The structural-analysis-domain reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: `StructuralProjection` lowers the GeometryGym structural-analysis entity surface onto NEUTRAL seam payloads — the `Map<PropertyName, PropertyValue>` attribute bag a `Relations/relation#EDGE_ALGEBRA` `Relationship.Generic` edge or a structural `Properties/property#PROPERTY_BAG` `PropertySet` node carries — so a `Rasm.Compute` frame solve reads the idealization off the ONE `Graph/element#ELEMENT_GRAPH` `ElementGraph` it already holds, never a second store. The idealized analytical line is NOT a payload this reader produces: it is content-keyed into the member `Object` node's `Graph/element#NODE_MODEL` `RepresentationContentHash` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time (the ONE `IfcRepresentation.Keys` representation content-keyer, every geometry — display `Body` and analytical `Axis`/`FootPrint` — hashed alike), and `Rasm.Compute` resolves the coordinate line one-hop BY CONTENT KEY from the blob store; an inline `AxisCurve`/`Vector3` coordinate field on the seam `Object` node is the named §4-RT-M2 seam violation, the deleted form. This RETIRES the migration source's parallel `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint` record family keyed by `BimModel`/`GlobalId` (the very "second stored record off the element" the rebuild forbids): an idealized curve member is the seam `Object` node it already is (an `IfcStructuralItem` is an `IfcProduct`, so the general projector mints it), its analytical line the content-keyed `Axis` representation, the member↔connection 6-DOF restraint and member↔activity applied load the `Generic` edge payloads `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.Structural` carries, and the analysis-model / load-group grouping the `Assign.Group` edge the general `IfcRelAssignsToGroup` fold authors — never a re-modeled analysis mesh and never a parallel selection surface.

The owner is the deep STRUCTURAL half of the projection `SemanticProjector` keeps out of the general fold: the complete `IfcBoundaryCondition` restraint algebra, relationship-level member-end release, applied-load family, grouping definitions, analytical topology discriminants, inverse authoring residue, and SAF workbook exchange. The reader is HOST-NEUTRAL: absent optional source detail remains `Option`/empty enrichment, while every present SI magnitude crosses `MeasureValue.OfSi` on the enclosing `Fin` rail and re-keys rejection through `ElementFault.ValueRejected`; no host geometry type, non-finite measure, or partial inverse crosses the boundary.

## [01]-[INDEX]

- [01]-[STRUCTURAL_PROJECTION]: `StructuralProjection` the GeometryGym structural-analysis-domain reader — `Attrs` the ONE polymorphic attribute-bag reader discriminating the structural entity onto its neutral `Map<PropertyName, PropertyValue>` payload (an `IfcRelConnectsStructuralMember` onto the member-end restraint edge bag — the rel-level `AppliedCondition` falling back to the connection's, the `ConditionCoordinateSystem` frame, the `SupportedLength`, and the folded-in `AtStart` endpoint discriminant; an `IfcRelConnectsStructuralActivity` onto the applied-load edge bag plus the point-action `Station` position; an `IfcStructuralConnection`/`IfcStructuralActivity` onto its own restraint/load bag; an `IfcStructuralLoadCase`/`IfcStructuralLoadGroup`/`IfcStructuralResultGroup`/`IfcStructuralAnalysisModel`/`IfcStructuralCurveMember`/`IfcStructuralSurfaceMember` onto its self-weight / load-combination / theory-and-linearity / model-type-plus-group-joins-plus-2D-plane / local-axis / thickness definition bag), the load components riding the consumer-neutral `ForceX..Z`/`MomentX..Z` wire names with the per-family SI `Dimension`, plus the two-tier `Case` derivation (specific `IfcActionSourceTypeEnum` rows, then the group `IfcActionTypeEnum` nature) and the FE `LoadKind` token `Rasm.Compute` reads; `AtStart` and `Station` the transient-topology endpoint/position discriminants the restraint and load edges carry; the idealized analytical line is NOT read here — it is content-keyed into the member `Object`'s `Representations` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time and `Rasm.Compute` resolves it one-hop by content key.

## [02]-[STRUCTURAL_PROJECTION]

- Owner: `StructuralProjection` the static structural-analysis-domain reader `SemanticProjector` composes, lowering the GeometryGym structural-analysis surface onto neutral seam payloads — never a stored record. It owns the polymorphic `Attrs` attribute-bag reader (one entry discriminating the structural entity — relationship, connection, activity, load case/group, result group, analysis model, curve/surface member — onto its restraint / load / definition bag) and the `AtStart`/`Station` transient-topology discriminants; the typed analysis structures the migration source minted (`AnalysisModel`, the `AnalysisMember` `[Union]`, `LoadGroup`, `Support`, `MemberConnection`, `SupportRestraint`, `StructuralLoadKind`, `StructuralCurveMemberKind`) are all GONE — the member is the seam `Object` node, the joint kind its `PredefinedType` token, the topology its neutral `Connect`/`Generic` edges, the restraint/load the typed `PropertyValue` edge payloads, and the analytical line the `Axis`-keyed content hash in the member's `Representations` map (content-keyed at `ObjectNode`, resolved one-hop by `Rasm.Compute`, never read or baked here).
- Entry: `Attrs(BaseClassIfc? entity, Op key)` lowers every supported structural entity through one `Fin<Map<PropertyName, PropertyValue>>` dispatch. All SI measures traverse `MeasureValue.OfSi` and re-key `ElementFault.ValueRejected`; non-finite GeometryGym sentinels remain absent. `AtStart` and `Station` return `Option` discriminants, so unresolved topology emits no assertion. `Author(DatabaseIfc, IfcObjectDefinition, Map<PropertyName, PropertyValue>)` re-stamps verified restraint and single-force constructors and returns every unconsumed row as fidelity residue. `Saf(SafOp operation, IExcelImportService imports, IExcelExportService exports, IExcelValidator validator, Op key)` validates and executes both XLSX directions over `ExcelModel.Objects`; the source version derives from `ExcelModel.OriginalVersion`, while the operation carries only the caller-selected target version.
- Auto: the analytical line is not produced here — at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` the member's inherited `IfcProduct.Representation` `IfcProductDefinitionShape` is content-keyed through `IfcRepresentation.Keys` (every `RepresentationIdentifier` — `Axis`/`Body`/`Box`/`FootPrint` — onto its content hash), so the `Axis` line and the heavy display body alike ride `RepresentationContentHash` and `Rasm.Compute` resolves the line's coordinates one-hop by content key from the blob store; the restraint arms discriminate the boundary condition over `IfcBoundaryNodeCondition` (the `TranslationalStiffnessX`/`Y`/`Z` `IfcTranslationalStiffnessSelect` + `RotationalStiffnessX`/`Y`/`Z` `IfcRotationalStiffnessSelect`) and `IfcBoundaryEdgeCondition` (the `LinearStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfTranslationalSubgradeReactionSelect` + `RotationalStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfRotationalSubgradeReactionSelect`), reducing each DOF through one four-arm type switch over GeometryGym's split select hierarchy (`IfcTranslationalStiffnessSelect` + the two subgrade-reaction selects derive `StiffnessSelect<TMeasure>`, `IfcRotationalStiffnessSelect` standalone — no common base unifies them, but each independently exposes a `Rigid` Boolean + a `Stiffness` measure whose `.Measure` rides `IfcDerivedMeasureValue`) onto a fixity `PropertyValue.Boolean` PLUS the SI spring stiffness `PropertyValue.Measure` [H2], the `Frame` reader stamping the `ConditionCoordinateSystem` `Axis`/`RefDirection` direction ratios so a skewed support's restraint axes survive; the load arm discriminates the `AppliedLoad` over the `IfcStructuralLoadSingleForce`/`LinearForce`/`PlanarForce`/`Temperature`/`SingleDisplacement` family onto typed force/moment/pressure/temperature `MeasureValue` components — the 1D families sharing the consumer-neutral `ForceX..Z`/`MomentX..Z` names the `Rasm.Compute` `Vec(g, "Force")`/`Vec(g, "Moment")` reads take for point AND uniform actions, the family discriminated by the `LoadType` token and the per-component `Dimension` (N vs N/m), the `SingleDisplacement` settlement carried as frame attrs only (its components internal-field-only) — plus the neutral FE `LoadKind` token and the two-tier `Case` token (the specific `CaseSources` row over `IfcActionSourceTypeEnum`, else the group `ActionType` nature — `PERMANENT_G` to `dead`, else `live` — so a prestress, shrinkage, or settlement group factors as a permanent action rather than silently mis-casing variable), the `Attrs` egress `Filter` NaN-guarding the raw-sentinel surfaces (a `Temperature` `DeltaT_*`, a `SupportedLength`, a `Coefficient`, a 2D `DirectionRatioZ` unset read NaN and drop; a `Single`/`Linear`/`PlanarForce` component the public getter coerced to 0.0 emits a deliberate 0 the Filter cannot suppress); the group/case/result/model arms read the `IfcStructuralLoadGroup` `PredefinedType`/`ActionType`/`ActionSource`/`Coefficient`/`Purpose`, the `IfcStructuralLoadCase.SelfWeightCoefficients` gravity vector, the `IfcStructuralResultGroup` `TheoryType`/`IsLinear`/`ResultForLoadGroup`, and the `IfcStructuralAnalysisModel` `PredefinedType` (the `IfcAnalysisModelTypeEnum` loading-model type, stamped `ModelType` — the analysis THEORY lives on the result group) plus the `LoadedBy`/`HasResults` model→group JOINS as GlobalId `PropertyValue.List` payloads (direct set attributes no `IfcRel*` edge carries — a count would erase the wiring a multi-model file needs) and the `OrientationOf2DPlane` 2D loading plane through the same prefix-parameterized `Frame` reader (`PlaneAxisX..PlaneRefZ`) onto the structural definition bags, the member arms the `IfcStructuralCurveMember.Axis` local-axis direction ratios and the `IfcStructuralSurfaceMember.Thickness`.
- Receipt: the readers' payloads land on the ONE seam `ElementGraph` — the six-DOF restraint, frame, supported length, and `AtStart` on the `IfcRelConnectsStructuralMember` `Generic` edge, the applied load and `Station` on the `IfcRelConnectsStructuralActivity` `Generic` edge, and the load-group / load-case / result-group / analysis-model / member definitions on structural `PropertySet` nodes, the idealized analytical line riding the member `Object`'s `Axis`-keyed content hash in `Representations` — so the `Rasm.Compute` structural runner resolves the analytical line one-hop by content key, reads the support fixity/stiffness and the load components off the member's incident edges by the exact wire names its `SupportsOf`/`LoadsOf` accessors probe (`AtStart`, `TranslationX..RotationZ`, `LoadKind`, `Case`, `ForceX..Z`, `MomentX..Z`, `Station`), and joins the section properties the `Graph/element#ELEMENT_GRAPH` `SectionOf` accessor bakes off the member's `ProfileSet` composition — resolved through the member's `Component` Type by the seam's one-hop type-resolved fallback (an occurrence with no own `ProfileSet` reads its `Element.Type` `Component`'s `SectionProperties`, the `Assign.TypeDefinition` inheritance the `Bake` fold applies), so an analytical member sharing a standardized cross-section reads it once off the deduped Type rather than per occurrence, the frozen Op-free `SectionOf(member)` signature untouched — the analysis owner producing the idealized graph, the solve and the typed `FrameModel` living wholly in `Rasm.Compute`, never re-projected here. A beam's analytical line, a slab's idealized thickness, a column-base node's six-DOF skewed support, a quarter-span point load, and a self-weight-vectored gravity case each ride the one graph the consumer already holds.
- Packages: GeometryGymIFC_Core, StructuralAnalysisFormat, Rasm.Element, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new boundary-condition kind is one arm on the `RestraintOf` switch reading the next `IfcBoundaryCondition` subtype's stiffness selects through the SAME `object`-pattern reducer; a new applied-load family is one arm on the `Vectors` switch reading the next `IfcStructuralLoad` subtype's components; a new structural entity or relationship with a definition bag is one arm on the polymorphic `Attrs` switch; a new action-source classification is one `CaseSources` row (the `ActionType` nature tier already totalizing the residue); a new analytical-geometry kind is one more `RepresentationIdentifier` the `Projection/semantic#SEMANTIC_PROJECTOR` `IfcRepresentation.Keys` content-keyer maps into the member's `Representations`, resolved by content key downstream; never a per-member-type analysis record, never a `RestraintAttrs`/`LoadAttrs` sibling family, never a second analysis store, never a re-modeled analytical mesh, and never an inline analytical coordinate field on the node.
- Boundary: `StructuralProjection` produces ONLY neutral seam payloads — the migration `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint`/`StructuralLoadKind`/`StructuralCurveMemberKind` typed store is the deleted form, the idealized member being the seam `Object` node (an `IfcStructuralItem` IS an `IfcProduct` the general fold mints), its joint kind the `Object.PredefinedType` token, its topology the neutral `Connect`/`Generic` edges, and its restraint/load the typed `PropertyValue` edge payloads; the entity→bag reading is ONE polymorphic `Attrs` discriminating by input value and a `RestraintAttrs`/`LoadAttrs`/`GroupAttrs`/`ModelAttrs` sibling-method family is the deleted form; the `IfcRelConnectsStructuralMember`/`IfcRelConnectsStructuralActivity` edge bag builds from ONE `Attrs(rel)` read and a caller-side bag-plus-manual-`Add` assembly is the deleted two-step; the 1D load components ride the consumer-neutral `ForceX..Z`/`MomentX..Z` wire names the `Rasm.Compute` `StructuralReads` accessors probe, and a per-family `LinearForceX`-style namespace that forks the uniform-load read onto silent zeros is the deleted form (the family discriminant is the `LoadType` token + the component `Dimension`, never the name); the structural reader is the DEEP half `SemanticProjector` composes and re-introducing it as a standalone `IElementProjection` (a second projector minting the member nodes the general fold already mints) is the deleted form; the analytical line rides the member `Object`'s `Axis`-keyed content hash in `RepresentationContentHash` (content-keyed at `ObjectNode` by `IfcRepresentation.Keys`, resolved one-hop by content key in `Rasm.Compute`), and an inline `AxisCurve`/`Vector3` analytical-coordinate field on the seam node — like a RhinoCommon `Curve`/`Brep` field or an in-process BRep tessellation — is the named §4-RT-M2 seam violation, the deleted form (the `AtStart`/`Station`/`Frame` topology reads are TRANSIENT, emitting only Boolean/scalar attributes); the restraint preserves the SI spring stiffness as a `MeasureValue` per DOF [H2] and a boolean-only fixity that drops the spring magnitude is the deleted form; the load family is read over the full `IfcStructuralLoad` subtype set and a single-force-only reader is the deleted form; the `Case` derivation is total over `IfcActionSourceTypeEnum` through the two-tier source-row-then-`ActionType`-nature fold and a five-row map folding every permanent action to `live` is the deleted mis-casing; the member↔connection topology is the seam `Generic` edge (wire-name `IfcRelConnectsStructuralMember`) the `EdgeProjection.Structural` fold authors and a typed `MemberConnection` record is the deleted form; the content-key identity is the seam `ElementGraph` content address (the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`) the consumer reads the graph by, and minting a second `(GeometryKey, PropertyKey)` scheme or reaching the up-stratum `Rasm.Compute` `InterchangeIdentity` is the named cross-folder drift defect [H7]; the GeometryGym structural-analysis surface is consumed as settled vocabulary (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) and a hand-rolled structural-member reader is the deleted form; the reader is TOTAL and routing a structural enrichment onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the class/reference rails are the general fold's `Fin<GraphDelta>`).

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
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record SafOp {
    private SafOp() { }

    public sealed record Import(Stream Workbook, Version TargetVersion) : SafOp;
    public sealed record Export(Stream Workbook, ExcelModel Model, Version TargetVersion) : SafOp;
}

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
    // the named permanent/climatic/seismic IfcActionSourceTypeEnum rows map onto the consumer's closed
    // dead/live/snow/wind/seismic StructuralCase tokens — the permanent-nature sources (completion, prestress,
    // settlement, shrinkage, creep, imperfection, lack-of-fit) land dead so they factor as permanent actions.
    // A source with no row falls to the NATURE tier: the group's IfcActionTypeEnum (PERMANENT_G -> dead, else
    // live), so the residue (temperature, fire, impact, wave, brakes, ...) factors as the variable case rather
    // than a silently mis-cased permanent, and USERDEFINED/NOTDEFINED sources still classify by nature.
    private static readonly Map<IfcActionSourceTypeEnum, string> CaseSources = toMap(Seq(
        (IfcActionSourceTypeEnum.DEAD_LOAD_G, "dead"), (IfcActionSourceTypeEnum.COMPLETION_G1, "dead"),
        (IfcActionSourceTypeEnum.PRESTRESSING_P, "dead"), (IfcActionSourceTypeEnum.SETTLEMENT_U, "dead"),
        (IfcActionSourceTypeEnum.SHRINKAGE, "dead"), (IfcActionSourceTypeEnum.CREEP, "dead"),
        (IfcActionSourceTypeEnum.SYSTEM_IMPERFECTION, "dead"), (IfcActionSourceTypeEnum.LACK_OF_FIT, "dead"),
        (IfcActionSourceTypeEnum.SNOW_S, "snow"), (IfcActionSourceTypeEnum.WIND_W, "wind"),
        (IfcActionSourceTypeEnum.EARTHQUAKE_E, "seismic")));

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
    public static Fin<Map<PropertyName, PropertyValue>> Attrs(BaseClassIfc? entity, Op key) {
        UnitScale scale = entity?.Database is { } database ? UnitScale.Of(database) : UnitScale.Si;
        return entity switch {
            IfcRelConnectsStructuralMember relation =>
                from restraint in RestraintOf(relation.AppliedCondition ?? relation.RelatedStructuralConnection?.AppliedCondition, scale, key)
                from frame in Frame(relation.ConditionCoordinateSystem, "Restraint", key)
                from length in Measures(Seq(("SupportedLength", Dimension.LengthDim, relation.SupportedLength * scale.Factor(Dimension.LengthDim))), key)
                select restraint.AddRange(frame).AddRange(length).AddRange(
                    AtStart(relation.RelatingStructuralMember as IfcStructuralCurveMember, relation.RelatedStructuralConnection)
                        .Map(static atStart => (PropertyName.Create("AtStart"), (PropertyValue)new PropertyValue.Boolean(atStart))).ToSeq()),
            IfcRelConnectsStructuralActivity relation =>
                from load in LoadOf(relation.RelatedStructuralActivity, scale, key)
                from station in Measures(Station(relation.RelatingElement as IfcStructuralCurveMember, relation.RelatedStructuralActivity)
                    .Map(static value => ("Station", Dimension.Dimensionless, value)).ToSeq(), key)
                select load.AddRange(station),
            IfcStructuralConnection connection =>
                from restraint in RestraintOf(connection.AppliedCondition, scale, key)
                from frame in Frame((connection as IfcStructuralPointConnection)?.ConditionCoordinateSystem, "Restraint", key)
                select restraint.AddRange(frame),
            IfcStructuralActivity activity => LoadOf(activity, scale, key),
            IfcStructuralLoadCase loadCase =>
                from group in GroupOf(loadCase, key)
                from weight in Measures(Optional(loadCase.SelfWeightCoefficients).ToSeq().Bind(static vector => Seq(
                    ("SelfWeightX", Dimension.Dimensionless, vector.Item1),
                    ("SelfWeightY", Dimension.Dimensionless, vector.Item2),
                    ("SelfWeightZ", Dimension.Dimensionless, vector.Item3))), key)
                select group.AddRange(weight),
            IfcStructuralLoadGroup group => GroupOf(group, key),
            IfcStructuralResultGroup result => Fin.Succ(Map(
                (PropertyName.Create("AnalysisTheory"), Enumerated(result.TheoryType.ToString(), TheoryKinds)),
                (PropertyName.Create("IsLinear"), (PropertyValue)new PropertyValue.Boolean(result.IsLinear)))
                .AddRange(Optional(result.ResultForLoadGroup)
                    .Map(static loadGroup => (PropertyName.Create("ResultFor"), (PropertyValue)new PropertyValue.Text(loadGroup.GlobalId))).ToSeq())),
            IfcStructuralAnalysisModel model =>
                from frame in Frame(model.OrientationOf2DPlane, "Plane", key)
                select Seq(
                        ("LoadedBy", toSeq(model.LoadedBy).Map(static group => group.GlobalId)),
                        ("HasResults", toSeq(model.HasResults).Map(static result => result.GlobalId)))
                    .Filter(static join => !join.Item2.IsEmpty)
                    .Fold(Map((PropertyName.Create("ModelType"), Enumerated(model.PredefinedType.ToString(), ModelKinds))),
                        static (map, join) => map.Add(PropertyName.Create(join.Item1),
                            new PropertyValue.List(join.Item2.Map(static id => (PropertyValue)new PropertyValue.Text(id)))))
                    .AddRange(frame),
            IfcStructuralCurveMember member => Measures(Optional(member.Axis).ToSeq().Bind(static axis => Seq(
                ("LocalAxisX", Dimension.Dimensionless, axis.DirectionRatioX),
                ("LocalAxisY", Dimension.Dimensionless, axis.DirectionRatioY),
                ("LocalAxisZ", Dimension.Dimensionless, axis.DirectionRatioZ))), key),
            IfcStructuralSurfaceMember surface => Measures(
                Seq(("Thickness", Dimension.LengthDim, surface.Thickness * scale.Factor(Dimension.LengthDim))), key),
            _ => Fin.Succ(Map<PropertyName, PropertyValue>()),
        };
    }

    // --- [RESTRAINT] --------------------------------------------------------------------------
    // The six-DOF support condition: a fixity Boolean PLUS the SI spring-stiffness magnitude per DOF [H2], so
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

    // The local orientation frame: the Axis/RefDirection direction ratios (dimensionless) under a caller-named
    // prefix — "Restraint" for a skewed support's ConditionCoordinateSystem (an inclined roller's DOF axes),
    // "Plane" for the analysis model's OrientationOf2DPlane — one reader, the prefix the only variation; a
    // global-axes placement (absent, or partial) emits nothing rather than a fabricated frame, and the ratios
    // are attribute data, never the content-keyed analytical geometry.
    private static Fin<Map<PropertyName, PropertyValue>> Frame(IfcAxis2Placement3D? system, string prefix, Op key) =>
        system is { Axis: { } axis, RefDirection: { } reference }
            ? Measures(Seq(("AxisX", axis.DirectionRatioX), ("AxisY", axis.DirectionRatioY), ("AxisZ", axis.DirectionRatioZ),
                  ("RefX", reference.DirectionRatioX), ("RefY", reference.DirectionRatioY), ("RefZ", reference.DirectionRatioZ))
                .Map(ratio => ($"{prefix}{ratio.Item1}", Dimension.Dimensionless, ratio.Item2)), key)
            : Fin.Succ(Map<PropertyName, PropertyValue>());

    private static Fin<Map<PropertyName, PropertyValue>> SixDof(
        (object? X, object? Y, object? Z) translation, (object? X, object? Y, object? Z) rotation,
        Dimension translationDim, Dimension rotationDim, UnitScale scale, Op key) {
        Seq<(string Fixity, string Spring, object? Select, Dimension Dim)> degrees = Seq(
            ("TranslationX", "TranslationKx", translation.X, translationDim),
            ("TranslationY", "TranslationKy", translation.Y, translationDim),
            ("TranslationZ", "TranslationKz", translation.Z, translationDim),
            ("RotationX",    "RotationKx",    rotation.X,    rotationDim),
            ("RotationY",    "RotationKy",    rotation.Y,    rotationDim),
            ("RotationZ",    "RotationKz",    rotation.Z,    rotationDim));
        Map<PropertyName, PropertyValue> fixities = degrees.Fold(Map<PropertyName, PropertyValue>(), (map, degree) => {
            (bool fixedAxis, double _) = Dof(degree.Select, degree.Dim, scale);
            return map.Add(PropertyName.Create(degree.Fixity), new PropertyValue.Boolean(fixedAxis));
        });
        return Measures(degrees.Map(degree => {
            (bool _, double spring) = Dof(degree.Select, degree.Dim, scale);
            return (degree.Spring, degree.Dim, spring);
        }), key).Map(springs => fixities.AddRange(springs));
    }

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
        Seq<(string Name, Dimension Dimension, double Si)> rows,
        Op key) =>
        rows.Filter(static row => double.IsFinite(row.Si))
            .TraverseM(row => MeasureValue.OfSi(row.Dimension, row.Si)
                .MapFail(_ => ElementFault.ValueRejected(key, $"<structural-measure:{row.Name}:{row.Si:R}>"))
                .Map(value => (Name: PropertyName.Create(row.Name), Value: (PropertyValue)new PropertyValue.Measure(value))))
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
            IfcStructuralConnection connection when attrs.ContainsKey(PropertyName.Create("TranslationX")) =>
                Consume(attrs, RestraintNames, () => connection.AppliedCondition = new IfcBoundaryNodeCondition(db, "",
                    Translational(attrs, "TranslationX", "TranslationKx"),
                    Translational(attrs, "TranslationY", "TranslationKy"),
                    Translational(attrs, "TranslationZ", "TranslationKz"),
                    Rotational(attrs, "RotationX", "RotationKx"),
                    Rotational(attrs, "RotationY", "RotationKy"),
                    Rotational(attrs, "RotationZ", "RotationKz"))),
            IfcStructuralActivity activity when LoadType(attrs) == nameof(IfcStructuralLoadSingleForce) =>
                Consume(attrs, ForceNames, () => activity.AppliedLoad = new IfcStructuralLoadSingleForce(db, Si(attrs, "ForceX"), Si(attrs, "ForceY"), Si(attrs, "ForceZ")) {
                    MomentX = Si(attrs, "MomentX"), MomentY = Si(attrs, "MomentY"), MomentZ = Si(attrs, "MomentZ"),
                }),
            _ => attrs,
        };

    // Consumed names = the stamped components plus the family discriminant; the frame tokens (LoadKind/Case/
    // GlobalOrLocal/Source) re-derive at the next ingest and never count as drops. The stamp Action is the
    // GG-authoring mutation seam, confined here.
    private static readonly Seq<string> RestraintNames = Seq(
        "TranslationX", "TranslationY", "TranslationZ", "RotationX", "RotationY", "RotationZ",
        "TranslationKx", "TranslationKy", "TranslationKz", "RotationKx", "RotationKy", "RotationKz");

    private static readonly Seq<string> ForceNames = Seq("ForceX", "ForceY", "ForceZ", "MomentX", "MomentY", "MomentZ", "LoadType");

    private static Map<PropertyName, PropertyValue> Consume(Map<PropertyName, PropertyValue> attrs, Seq<string> names, Action stamp) {
        stamp();
        return names.Fold(attrs, static (residue, name) => residue.Remove(PropertyName.Create(name)));
    }

    private static string LoadType(Map<PropertyName, PropertyValue> attrs) =>
        attrs.Find(PropertyName.Create("LoadType"))
            .Bind(static value => value is PropertyValue.Enumerated enumerated ? enumerated.Selected.Head : None)
            .Bind(static selected => selected is PropertyValue.Text text ? Some(text.Value) : None)
            .IfNone("");

    // A DOF select off the bag pair — the Dof reading's inverse: a finite positive spring wins (the SI stiffness
    // re-stamps through the double ctor), a bare true fixity re-stamps rigid, an absent/false pair is free.
    private static IfcTranslationalStiffnessSelect Translational(Map<PropertyName, PropertyValue> attrs, string fixity, string spring) =>
        Si(attrs, spring) is > 0d and var k ? new IfcTranslationalStiffnessSelect(k) : new IfcTranslationalStiffnessSelect(Fixity(attrs, fixity));

    private static IfcRotationalStiffnessSelect Rotational(Map<PropertyName, PropertyValue> attrs, string fixity, string spring) =>
        Si(attrs, spring) is > 0d and var k ? new IfcRotationalStiffnessSelect(k) : new IfcRotationalStiffnessSelect(Fixity(attrs, fixity));

    private static double Si(Map<PropertyName, PropertyValue> attrs, string name) =>
        attrs.Find(PropertyName.Create(name)).Bind(static v => v is PropertyValue.Measure m ? Some(m.Value.Si) : None).IfNone(0d);

    private static bool Fixity(Map<PropertyName, PropertyValue> attrs, string name) =>
        attrs.Find(PropertyName.Create(name)).Exists(static v => v is PropertyValue.Boolean { Value: true });

    // --- [LOAD] -------------------------------------------------------------------------------
    // The applied load the IfcRelConnectsStructuralActivity Generic edge carries: typed force/moment/pressure/
    // temperature MeasureValue components over the IfcStructuralLoad family PLUS the load-type token, the
    // global/local frame, and the source name (the prior single-force-only reader dropped the line/planar/
    // temperature families). IfcStructuralLoadSingleDisplacement, whose components are internal-field-only, yields
    // the frame attrs only, a graceful passthrough; a Temperature DeltaT_* unset = NaN drops at the Attrs egress,
    // while a force/moment component the public getter coerced to 0.0 emits a 0 (the getter masks the unset
    // sentinel, so the Filter NaN-guards the raw-sentinel surfaces and never suppresses a coerced 0). Every
    // component is model-NATIVE and coerces to SI through the UnitScale dimensional factor at the fold.
    private static Fin<Map<PropertyName, PropertyValue>> LoadOf(IfcStructuralActivity? activity, UnitScale scale, Op key) =>
        Optional(activity).Bind(static candidate => Optional(candidate.AppliedLoad).Map(load => (Activity: candidate, Load: load))).Match(
            Some: pair => Measures(Vectors(pair.Load).Map(vector =>
                    (vector.Name, vector.Dim, vector.Native * scale.Factor(vector.Dim))), key)
                .Map(measures => Map(
                    (PropertyName.Create("LoadType"), Enumerated(pair.Load.GetType().Name, LoadKinds)),
                    (PropertyName.Create("LoadKind"), (PropertyValue)new PropertyValue.Text(KindOf(pair.Load))),
                    (PropertyName.Create("Case"), new PropertyValue.Text(CaseOf(pair.Activity))),
                    (PropertyName.Create("GlobalOrLocal"), new PropertyValue.Text(pair.Activity.GlobalOrLocal.ToString())),
                    (PropertyName.Create("Source"), new PropertyValue.Text(pair.Activity.Name ?? "")))
                    .AddRange(measures)),
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

    // The two-tier neutral load CASE walked off the activity's IfcStructuralLoadGroup assignment (HasAssignments ->
    // IfcRelAssignsToGroup.RelatingGroup -> IfcStructuralLoadGroup): the SPECIFIC CaseSources row over ActionSource
    // first, else the NATURE tier off the group's ActionType (PERMANENT_G -> dead, else live) — so a prestress or
    // shrinkage group factors permanent and a temperature or impact group factors variable under the consumer's
    // LoadCombinationSpec. An ungrouped activity folds to "live", the unfactored variable case the consumer defaults.
    private static string CaseOf(IfcStructuralActivity activity) =>
        activity.HasAssignments.AsIterable()
            .Choose(static a => a is IfcRelAssignsToGroup { RelatingGroup: IfcStructuralLoadGroup g } ? Some(g) : None)
            .ToSeq().Head
            .Map(static g => CaseSources.Find(g.ActionSource)
                .IfNone(() => g.ActionType == IfcActionTypeEnum.PERMANENT_G ? "dead" : "live"))
            .IfNone("live");

    // The load-group definition bag the LoadCase arm extends: the combination/case/group discriminant, the action
    // nature and source, the partial-safety Coefficient (NaN unset, dropped at egress), and the purpose label.
    private static Fin<Map<PropertyName, PropertyValue>> GroupOf(IfcStructuralLoadGroup group, Op key) =>
        Measures(Seq(("Coefficient", Dimension.Dimensionless, group.Coefficient)), key)
            .Map(measures => Map(
                (PropertyName.Create("LoadGroupType"), Enumerated(group.PredefinedType.ToString(), LoadGroupKinds)),
                (PropertyName.Create("ActionType"), (PropertyValue)new PropertyValue.Text(group.ActionType.ToString())),
                (PropertyName.Create("ActionSource"), new PropertyValue.Text(group.ActionSource.ToString())),
                (PropertyName.Create("Purpose"), new PropertyValue.Text(group.Purpose ?? "")))
                .AddRange(measures));

    // The 1D families share the consumer-neutral ForceX..Z / MomentX..Z names — the exact wire the Rasm.Compute
    // StructuralReads Vec(g, "Force")/Vec(g, "Moment") probes take for point AND uniform actions — the family
    // discriminated by the LoadType token and the per-component Dimension (a point force N, a line force N/m, a
    // line moment N·m/m = N); a per-family LinearForceX-style namespace forked the uniform read onto silent zeros.
    private static Seq<(string Name, double Native, Dimension Dim)> Vectors(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadSingleForce f => Seq(
            ("ForceX", f.ForceX, Dimension.ForceDim), ("ForceY", f.ForceY, Dimension.ForceDim), ("ForceZ", f.ForceZ, Dimension.ForceDim),
            ("MomentX", f.MomentX, Moment), ("MomentY", f.MomentY, Moment), ("MomentZ", f.MomentZ, Moment)),
        IfcStructuralLoadLinearForce l => Seq(
            ("ForceX", l.LinearForceX, ForcePerLength), ("ForceY", l.LinearForceY, ForcePerLength), ("ForceZ", l.LinearForceZ, ForcePerLength),
            ("MomentX", l.LinearMomentX, Dimension.ForceDim), ("MomentY", l.LinearMomentY, Dimension.ForceDim), ("MomentZ", l.LinearMomentZ, Dimension.ForceDim)),
        IfcStructuralLoadPlanarForce p => Seq(
            ("PlanarForceX", p.PlanarForceX, ForcePerArea), ("PlanarForceY", p.PlanarForceY, ForcePerArea), ("PlanarForceZ", p.PlanarForceZ, ForcePerArea)),
        IfcStructuralLoadTemperature t => Seq(
            ("DeltaTConstant", t.DeltaT_Constant, TemperatureDelta), ("DeltaTY", t.DeltaT_Y, TemperatureDelta), ("DeltaTZ", t.DeltaT_Z, TemperatureDelta)),
        // The IFC varying line action: IfcStructuralLoadConfiguration (public Values/Locations, decompile-verified)
        // carrying positioned linear forces — the first/last rows lower onto the trapezoid wire (StartX..Z/EndX..Z)
        // the Rasm.Compute Vec(g, "Start")/Vec(g, "End") probes read; a single-row or non-linear-force configuration
        // falls through to the graceful passthrough, never a fabricated ramp.
        IfcStructuralLoadConfiguration cfg when cfg.Values.OfType<IfcStructuralLoadLinearForce>().ToSeq() is { Count: >= 2 } ramp => Seq(
            ("StartX", ramp[0].LinearForceX, ForcePerLength), ("StartY", ramp[0].LinearForceY, ForcePerLength), ("StartZ", ramp[0].LinearForceZ, ForcePerLength),
            ("EndX", ramp[ramp.Count - 1].LinearForceX, ForcePerLength), ("EndY", ramp[ramp.Count - 1].LinearForceY, ForcePerLength), ("EndZ", ramp[ramp.Count - 1].LinearForceZ, ForcePerLength)),
        // IfcStructuralLoadSingleDisplacement holds its DisplacementX/Y/Z + RotationalDisplacementRX/RY/RZ as INTERNAL
        // fields in GeometryGym 25.7.30 — NO public accessor crosses the assembly boundary — so a prescribed-displacement
        // (support-settlement) load reads the frame attrs only (LoadType/LoadKind/Case/GlobalOrLocal/Source via LoadOf),
        // the documented surface boundary rather than a phantom `d.DisplacementX` read or a silently-invented 0-settlement;
        // the `_` graceful passthrough owns it alongside any unenumerated load family, never a fabricated component.
        _ => Seq<(string, double, Dimension)>(),
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

## [03]-[RESEARCH]

- [STRUCTURAL_SURFACE_DISPATCH]: the structural-analysis entity surface `StructuralProjection` reads grounds against the live GeometryGym 25.7.30 decompile (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) — `IfcStructuralCurveMember : IfcStructuralMember` carries `PredefinedType` (`IfcStructuralCurveMemberTypeEnum`) and `Axis` (`IfcDirection`, the `DirectionRatioX/Y/Z` local-axis orientation the member arm lowers), `IfcStructuralSurfaceMember` carries `Thickness` (double) and `PredefinedType`, `IfcStructuralConnection` (abstract) carries `AppliedCondition` (`IfcBoundaryCondition`) and the point subtype its `ConditionCoordinateSystem` (`IfcAxis2Placement3D`) + `Vertex`, `IfcStructuralActivity : IfcProduct` (abstract) carries `AppliedLoad` (`IfcStructuralLoad`) and `GlobalOrLocal` (`IfcGlobalOrLocalEnum`), `IfcRelConnectsStructuralMember : IfcRelConnects` carries `RelatingStructuralMember`/`RelatedStructuralConnection` PLUS the member-end `AppliedCondition` (`IfcBoundaryCondition`), `SupportedLength` (double, NaN unset), and `ConditionCoordinateSystem` (`IfcAxis2Placement3D`) the rel arm lowers, `IfcRelConnectsStructuralActivity` carries `RelatingElement` (`IfcStructuralActivityAssignmentSelect`)/`RelatedStructuralActivity`, `IfcStructuralAnalysisModel : IfcSystem` carries `PredefinedType` (`IfcAnalysisModelTypeEnum` — the IN_PLANE/OUT_PLANE/3D LOADING model type, stamped `ModelType`, never the analysis theory)/`OrientationOf2DPlane`/`LoadedBy` (`SET<IfcStructuralLoadGroup>`)/`HasResults` (`SET<IfcStructuralResultGroup>`), `IfcStructuralLoadGroup : IfcGroup` carries `PredefinedType` (`IfcLoadGroupTypeEnum` — six members including `LOAD_COMBINATION_GROUP`)/`ActionType`/`ActionSource`/`Coefficient`/`Purpose`, its `IfcStructuralLoadCase` subtype the `SelfWeightCoefficients` (`Tuple<double,double,double>`, null unset) gravity vector, and `IfcStructuralResultGroup : IfcGroup` carries `TheoryType` (`IfcAnalysisTheoryTypeEnum`)/`ResultForLoadGroup`/`IsLinear` — so the reader discriminates the real analysis graph including its relationship-level and grouping-level scalars, the idealized members landing as seam `Object` nodes through the general `Extract<IfcProduct>` fold (an `IfcStructuralItem` IS an `IfcProduct`) and the member↔connection / member↔activity / group-membership topology landing as neutral `Connect`/`Generic`/`Assign` edges through `Projection/relations#RELATION_ALGEBRA`, never a hand-rolled structural-member reader and never a parallel analysis store.
- [BOUNDARY_CONDITION_RESTRAINT]: the six-DOF restraint the restraint arms read grounds against the live decompile — `IfcBoundaryNodeCondition : IfcBoundaryCondition` carries `TranslationalStiffnessX`/`Y`/`Z` (`IfcTranslationalStiffnessSelect`) and `RotationalStiffnessX`/`Y`/`Z` (`IfcRotationalStiffnessSelect`); `IfcBoundaryEdgeCondition` carries `LinearStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfTranslationalSubgradeReactionSelect`) and `RotationalStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfRotationalSubgradeReactionSelect`). `IfcTranslationalStiffnessSelect`/`IfcModulusOfTranslationalSubgradeReactionSelect`/`IfcModulusOfRotationalSubgradeReactionSelect` derive the generic `StiffnessSelect<TMeasure>` (over `IfcLinearStiffnessMeasure`/`IfcModulusOfLinearSubgradeReactionMeasure`/`IfcModulusOfRotationalSubgradeReactionMeasure`) and `IfcRotationalStiffnessSelect` is standalone, but ALL FOUR expose the identical public shape — `bool Rigid` and `Stiffness` returning a measure whose `.Measure` double rides `IfcDerivedMeasureValue` — so one four-arm type switch (no common base permits a single property pattern across the split hierarchy) reduces each DOF: a rigid support reads `Rigid == true`, a finite spring reads `Stiffness.Measure > 0`, an absent select reads free. The reader preserves the SI spring stiffness as a `MeasureValue` per DOF [H2] — `IfcLinearStiffnessMeasure` (N/m) onto the node translational, `IfcModulusOfLinearSubgradeReactionMeasure` (N/m²) onto the edge line stiffness — where the prior boolean-only restraint dropped the magnitude the FE constraint needs. `RestraintOf` takes the `IfcBoundaryCondition?` itself so the REL-level member-end release (`IfcRelConnectsStructuralMember.AppliedCondition`, the pinned-beam-end-into-a-rigid-node semantics) and the connection's own support condition reduce through ONE reader with the rel's condition taking precedence, and the `Frame` reader lowers the `ConditionCoordinateSystem` `Axis`/`RefDirection` `IfcDirection` ratios so a skewed support's restraint axes survive (both directions demanded — a partial placement emits nothing). `IfcBoundaryFaceCondition` (a surface connection) holds its `TranslationalStiffnessByArea*` only as INTERNAL fields in 25.7.30 — no public accessor — so a face restraint reads free, the documented boundary limit rather than a silently invented stiffness.
- [LOAD_FAMILY]: the applied-load the load arm reads grounds against the live decompile — `IfcStructuralLoadSingleForce : IfcStructuralLoadStatic` carries `ForceX`/`Y`/`Z` and `MomentX`/`Y`/`Z` (double), `IfcStructuralLoadLinearForce` carries `LinearForceX`/`Y`/`Z` and `LinearMomentX`/`Y`/`Z`, `IfcStructuralLoadPlanarForce` carries `PlanarForceX`/`Y`/`Z`, and `IfcStructuralLoadTemperature` carries `DeltaT_Constant`/`DeltaT_Y`/`DeltaT_Z` as PUBLIC properties — the 1D families lowered onto the consumer-neutral `ForceX..Z`/`MomentX..Z` wire names the `Rasm.Compute` `StructuralReads.Vec(g, "Force")`/`Vec(g, "Moment")` probes read for point AND uniform actions (each an SI `MeasureValue` whose `Dimension` discriminates the family — `Dimension.ForceDim` point force, `ForceDim·LengthDim` point moment, `ForceDim/LengthDim` line force, `ForceDim` line moment (N·m/m), `ForceDim/AreaDim` planar force, the seven-base temperature delta for `DeltaT_*` — composed from the seam quantity registry, never a hand-coded exponent table); `IfcStructuralLoadSingleDisplacement` is in the family but exposes its `DisplacementX`/`Y`/`Z` + `RotationalDisplacementRX`/`RY`/`RZ` ONLY as internal fields (no public accessor in 25.7.30), so a prescribed-displacement load lands the frame attrs alone — the documented surface boundary, never a phantom component read; a configuration load the switch does not enumerate rides the frame attrs (`LoadType`/`GlobalOrLocal`/`Source`) as a graceful passthrough, a new load family folding onto one switch arm; the IFC varying line action is `IfcStructuralLoadConfiguration`, catalog-confirmed at `.api/api-geometrygym-ifc` (`Values` as `LIST<IfcStructuralLoadOrResult>`, `Locations` as `List<List<double>>`, the `(val, length)`/`(vals, locations)`/`(val1, loc1, val2, loc2)` trapezoid ctors), so the `Vectors` configuration arm reads the first/last `IfcStructuralLoadLinearForce` rows onto the `Rasm.Compute` `MemberLoad.Trapezoid` wire (`LoadKind` `"trapezoid"`, the `Vec(g, "Start")`/`Vec(g, "End")` `StartX..Z`/`EndX..Z` probes) and a single-row or non-linear-force configuration rides the graceful passthrough — never a fabricated ramp; the GeometryGym getter convention splits the unset behavior — `DeltaT_*` is a raw auto-property returning its NaN default (so an unset temperature drops at the `Attrs` egress `Filter`) while the force/moment/planar getters coerce an unset NaN field to 0.0 (so an unset force component emits a deliberate 0 the Filter cannot distinguish), the Filter therefore a NaN guard over the raw-sentinel surfaces rather than a universal unset-component eliminator. The neutral consumer tokens ride the same arm: the FE `LoadKind` derives from the load class (`IfcStructuralLoadSingleForce`->point, `IfcStructuralLoadLinearForce`->uniform), and the `Case` derivation is TWO-TIER and total over the 27-member `IfcActionSourceTypeEnum` — the specific `CaseSources` rows map the permanent-nature sources (`DEAD_LOAD_G`/`COMPLETION_G1`/`PRESTRESSING_P`/`SETTLEMENT_U`/`SHRINKAGE`/`CREEP`/`SYSTEM_IMPERFECTION`/`LACK_OF_FIT`) onto `dead` and the climatic/seismic (`SNOW_S`/`WIND_W`/`EARTHQUAKE_E`) onto their named cases, and the residue falls to the group's `IfcActionTypeEnum` nature (`PERMANENT_G` -> `dead`, `VARIABLE_Q`/`EXTRAORDINARY_A`/unset -> `live`) — walked off `IfcObjectDefinition.HasAssignments` (`SET<IfcRelAssigns>`) filtered to `IfcRelAssignsToGroup` whose `RelatingGroup` is an `IfcStructuralLoadGroup`, so a prestress group never factors as a variable action, all grounded against the live decompile.
- [ANALYTICAL_GEOMETRY]: the idealized analytical line is content-keyed, never inline-baked — at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` the inherited `IfcProduct.Representation` (`IfcProductDefinitionShape`) folds through the ONE `IfcRepresentation.Keys` content-keyer, the analytical `IfcTopologyRepresentation` (`IfcShapeModel`) landing under the `Axis` `RepresentationIdentifier` as a content hash in the member `Object`'s `Graph/element#NODE_MODEL` `RepresentationContentHash` map alongside the heavy display `Body` [M2], so `Rasm.Compute` resolves the coordinate line one-hop BY CONTENT KEY from the blob store — an inline `AxisCurve`/`Vector3` coordinate field on the seam node is the named §4-RT-M2 violation, the deleted form, and re-minting an analysis solver or a second content-key identity in the Bim folder is the named cross-folder drift defect, the solve and the typed frame model living wholly in `Rasm.Compute` (`Rasm.Compute/Analysis/structural`). The `AtStart`/`Station` discriminants ground against the live decompile — the member's analytical edge rides the `IfcTopologyRepresentation` `Items` as an `IfcEdge` (`EdgeStart`/`EdgeEnd` `IfcVertex`) whose endpoint coordinates ride `IfcVertexPoint.VertexGeometry` (`IfcCartesianPoint.Coordinates` `LIST<double>` — legally TWO coordinates in an IN_PLANE 2D analytical model, so `PointOf` reads a 2-coordinate point as `(x, y, 0)` and yields `None` below two, never a fabricated origin a plane-frame model's every joint would otherwise collapse onto), the point connection's vertex the `IfcStructuralPointConnection.Vertex` convenience accessor, and a point action's position vertex the activity's own topology representation — so both read the coordinates TRANSIENTLY (the seam `Rasm.Element` `Vector3` distance/projection compares producing only a Boolean edge attribute and a clamped dimensionless station), and the `EdgeProjection.Structural` fold resolves a support to the nearer member joint and a point load to its real span position (the `Rasm.Compute` `Si(g, "Station")` read, defaulting midspan 0.5 when absent) rather than the prior always-the-end / always-midspan defaults.
