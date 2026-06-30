# [STRUCTURAL_PROJECTION]

The structural-analysis-domain reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: `StructuralProjection` lowers the GeometryGym structural-analysis entity surface onto NEUTRAL seam payloads — the `Map<PropertyName, PropertyValue>` attribute bag a `Relations/relation#EDGE_ALGEBRA` `Relationship.Generic` edge or a structural `Properties/property#PROPERTY_BAG` `PropertySet` node carries — so a `Rasm.Compute` frame solve reads the idealization off the ONE `Graph/element#ELEMENT_GRAPH` `ElementGraph` it already holds, never a second store. The idealized analytical line is NOT a payload this reader produces: it is content-keyed into the member `Object` node's `Graph/element#NODE_MODEL` `RepresentationContentHash` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time (the ONE `IfcRepresentation.Keys` representation content-keyer, every geometry — display `Body` and analytical `Axis`/`FootPrint` — hashed alike), and `Rasm.Compute` resolves the coordinate line one-hop BY CONTENT KEY from the blob store; an inline `AxisCurve`/`Vector3` coordinate field on the seam `Object` node is the named §4-RT-M2 seam violation, the deleted form. This RETIRES the migration source's parallel `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint` record family keyed by `BimModel`/`GlobalId` (the very "second stored record off the element" the rebuild forbids): an idealized curve member is the seam `Object` node it already is (an `IfcStructuralItem` is an `IfcProduct`, so the general projector mints it), its analytical line the content-keyed `Axis` representation, the member↔connection 6-DOF restraint and member↔activity applied load the `Generic` edge payloads `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.Structural` carries, and the analysis-model / load-group grouping the `Assign.Group` edge the general `IfcRelAssignsToGroup` fold authors — never a re-modeled analysis mesh and never a parallel selection surface.

The owner is the deep STRUCTURAL half of the projection `SemanticProjector` keeps OUT of the general fold: the full `IfcBoundaryCondition` six-degree-of-freedom restraint algebra (preserving the SI spring-stiffness magnitude the prior boolean-only reader dropped [H2]), the full `IfcStructuralLoad` family (single force, line force, planar force, temperature, single displacement — not single-force-only), and the load-group / analysis-model / surface-member definition scalars a load combination and an analysis theory carry — the structural-analysis-domain vocabulary one focused reader owns so the general projector stays generic, a peer of `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection` and `Semantics/georeference#GEO_REFERENCE` `GeoReferenceProjector`. The reader is HOST-NEUTRAL and TOTAL: it never returns a host geometry type (no RhinoCommon type, no in-process BRep evaluation — the analytical line rides the content-keyed representation, never a coordinate field on the node), an unreadable structural detail yields a `None`/empty enrichment, and the element identity, entity class, and dangling-reference rails are the general projector's `Fin<GraphDelta>` concern — the structural reader never re-cases `Model/faults#FAULT_BAND` `BimFault`.

## [01]-[INDEX]

- [01]-[STRUCTURAL_PROJECTION]: `StructuralProjection` the GeometryGym structural-analysis-domain reader — `AtStart` the start/end endpoint discriminant the restraint edge carries, and `Attrs` the ONE polymorphic attribute-bag reader discriminating the structural entity onto its neutral `Map<PropertyName, PropertyValue>` payload (an `IfcStructuralConnection` onto the six-DOF restraint — fixity Boolean + SI stiffness `MeasureValue` per DOF [H2] over the node/edge condition selects; an `IfcStructuralActivity` onto the `IfcStructuralLoad` family force/moment/line/planar/temperature components (the `SingleDisplacement` settlement carried as frame attrs only — its components internal-field-only) PLUS the neutral FE `LoadKind` token and the load-group-`ActionSource`-derived `Case` token `Rasm.Compute` reads; an `IfcStructuralLoadGroup`/`IfcStructuralAnalysisModel`/`IfcStructuralSurfaceMember` onto its load-combination / analysis-theory / thickness definition) — each the seam payload the `Projection/relations#RELATION_ALGEBRA` `EdgeProjection.Structural` edge or a structural `PropertySet` node carries; the idealized analytical line is NOT read here — it is content-keyed into the member `Object`'s `Representations` map under the `Axis` key at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` time and `Rasm.Compute` resolves it one-hop by content key.

## [02]-[STRUCTURAL_PROJECTION]

- Owner: `StructuralProjection` the static structural-analysis-domain reader `SemanticProjector` composes, lowering the GeometryGym structural-analysis surface onto neutral seam payloads — never a stored record. It owns the polymorphic `Attrs` attribute-bag reader (one entry discriminating the structural entity onto its restraint / load / load-group / analysis-model / surface-member bag) and the `AtStart` endpoint discriminant; the typed analysis structures the migration source minted (`AnalysisModel`, the `AnalysisMember` `[Union]`, `LoadGroup`, `Support`, `MemberConnection`, `SupportRestraint`, `StructuralLoadKind`, `StructuralCurveMemberKind`) are all GONE — the member is the seam `Object` node, the joint kind its `PredefinedType` token, the topology its neutral `Connect`/`Generic` edges, the restraint/load the typed `PropertyValue` edge payloads, and the analytical line the `Axis`-keyed content hash in the member's `Representations` map (content-keyed at `ObjectNode`, resolved one-hop by `Rasm.Compute`, never read or baked here).
- Entry: the readers are TOTAL pure projections the projector composes at ingest — `Attrs(BaseClassIfc? entity)` is the ONE polymorphic attribute-bag reader discriminating on the entity shape — an `IfcStructuralConnection` onto the six-DOF restraint bag the `IfcRelConnectsStructuralMember` `Generic` edge carries, an `IfcStructuralActivity` onto the applied-load bag the `IfcRelConnectsStructuralActivity` `Generic` edge carries (the faithful IFC `LoadType` + full force/moment family PLUS the neutral FE `LoadKind` token and the load-group-`ActionSource`-derived `Case` token `Rasm.Compute` reads), an `IfcStructuralLoadGroup`/`IfcStructuralAnalysisModel`/`IfcStructuralSurfaceMember` onto the structural `PropertySet` definition bag, and any other (or null) onto the empty bag — no `Fin` rail, because a structural enrichment never fails the whole import over one unreadable detail (the class miss is the general fold's `BimFault.UnmappedClass`, the dangling endpoint its `BimFault.DanglingReference`), and no `Get`/`RestraintAttrs`/`LoadAttrs` sibling family — one polymorphic `Attrs` discriminating by input value; `AtStart(IfcStructuralCurveMember?, IfcStructuralConnection?)` is the start/end endpoint discriminant the `EdgeProjection.Structural` fold stamps on the restraint edge (the connection vertex nearer the member's analytical-edge start, read from the member's topology representation), so `Rasm.Compute` resolves a support to the correct member joint rather than always the end. The idealized analytical line is content-keyed into the member `Object`'s `Representations` map under the `Axis` key at `ObjectNode` time, never read or baked here.
- Auto: the analytical line is not produced here — at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` the member's inherited `IfcProduct.Representation` `IfcProductDefinitionShape` is content-keyed through `IfcRepresentation.Keys` (every `RepresentationIdentifier` — `Axis`/`Body`/`Box`/`FootPrint` — onto its content hash), so the `Axis` line and the heavy display body alike ride `RepresentationContentHash` and `Rasm.Compute` resolves the line's coordinates one-hop by content key from the blob store; `Attrs`' restraint arm discriminates the `AppliedCondition` over `IfcBoundaryNodeCondition` (the `TranslationalStiffnessX`/`Y`/`Z` `IfcTranslationalStiffnessSelect` + `RotationalStiffnessX`/`Y`/`Z` `IfcRotationalStiffnessSelect`) and `IfcBoundaryEdgeCondition` (the `LinearStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfTranslationalSubgradeReactionSelect` + `RotationalStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfRotationalSubgradeReactionSelect`), reducing each DOF through one four-arm type switch over GeometryGym's split select hierarchy (`IfcTranslationalStiffnessSelect` + the two subgrade-reaction selects derive `StiffnessSelect<TMeasure>`, `IfcRotationalStiffnessSelect` standalone — no common base unifies them, but each independently exposes a `Rigid` Boolean + a `Stiffness` measure whose `.Measure` rides `IfcDerivedMeasureValue`) onto a fixity `PropertyValue.Boolean` PLUS the SI spring stiffness `PropertyValue.Measure` [H2]; the load arm discriminates the `AppliedLoad` over the `IfcStructuralLoadSingleForce`/`LinearForce`/`PlanarForce`/`Temperature`/`SingleDisplacement` family onto typed force/moment/pressure/temperature `MeasureValue` components (the `SingleDisplacement` settlement components are GeometryGym internal-field-only, so a displacement load lands the frame attrs alone) plus the load-type, frame, and source attrs AND the neutral FE `LoadKind` token (single force -> point, linear force -> uniform) and the `Case` token walked off the activity's `IfcStructuralLoadGroup.ActionSource`, the `Attrs` egress `Filter` NaN-guarding the raw-sentinel surfaces (a `Temperature` `DeltaT_*` unset reads NaN and drops; a `Single`/`Linear`/`PlanarForce` component the public getter coerced to 0.0 emits a deliberate 0 the Filter cannot suppress); the group/model arms read the `IfcStructuralLoadGroup` `PredefinedType`/`ActionType`/`ActionSource`/`Coefficient`/`Purpose` and the `IfcStructuralAnalysisModel` `PredefinedType`/`LoadedBy`/`HasResults` onto the structural definition bags, and the surface-member arm its idealized `Thickness`.
- Receipt: the readers' payloads land on the ONE seam `ElementGraph` — the six-DOF restraint on the `IfcRelConnectsStructuralMember` `Generic` edge, the applied load on the `IfcRelConnectsStructuralActivity` `Generic` edge, and the load-group / analysis-model / surface-member definitions on structural `PropertySet` nodes, the idealized analytical line riding the member `Object`'s `Axis`-keyed content hash in `Representations` — so the `Rasm.Compute` structural runner resolves the analytical line one-hop by content key, reads the support fixity/stiffness off the member's `Connect`/`Generic` incidence edges and the load components off the activity edges to build its typed analysis view and FE solve, joining the section properties the `Graph/element#ELEMENT_GRAPH` `SectionOf` accessor bakes off the member's `ProfileSet` composition — resolved through the member's `Component` Type by the seam's one-hop type-resolved fallback (an occurrence with no own `ProfileSet` reads its `Element.Type` `Component`'s `SectionProperties`, the `Assign.TypeDefinition` inheritance the `Bake` fold applies), so an analytical member sharing a standardized cross-section reads it once off the deduped Type rather than per occurrence, the frozen Op-free `SectionOf(member)` signature untouched — the analysis owner producing the idealized graph, the solve and the typed `AnalysisModel` living wholly in `Rasm.Compute`, never re-projected here. A beam's analytical line, a slab's idealized thickness, a column-base node's six-DOF support, and a gravity load case each ride the one graph the consumer already holds.
- Packages: GeometryGymIFC_Core (the structural-analysis entity surface consumed as settled vocabulary), Rasm.Element (the seam `PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension` payloads AND the seam-owned host-neutral `Vector3` coordinate read transiently by the `AtStart` endpoint compare, never stored on a node — the seam owns the analytical coordinate the way it owns `Dimension`, so `using Rasm.Element` provides it and no kernel `Vector3` exists), LanguageExt.Core (`Option`/`Seq`/`Map`).
- Growth: a new boundary-condition kind is one arm on the `Attrs` restraint switch reading the next `IfcBoundaryCondition` subtype's stiffness selects through the SAME `object`-pattern reducer; a new applied-load family is one arm on the `Vectors` switch reading the next `IfcStructuralLoad` subtype's components; a new structural entity with a definition bag is one arm on the polymorphic `Attrs` switch; a new analytical-geometry kind is one more `RepresentationIdentifier` the `Projection/semantic#SEMANTIC_PROJECTOR` `IfcRepresentation.Keys` content-keyer maps into the member's `Representations`, resolved by content key downstream; never a per-member-type analysis record, never a `RestraintAttrs`/`LoadAttrs` sibling family, never a second analysis store, never a re-modeled analytical mesh, and never an inline analytical coordinate field on the node.
- Boundary: `StructuralProjection` produces ONLY neutral seam payloads — the migration `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint`/`StructuralLoadKind`/`StructuralCurveMemberKind` typed store is the deleted form, the idealized member being the seam `Object` node (an `IfcStructuralItem` IS an `IfcProduct` the general fold mints), its joint kind the `Object.PredefinedType` token, its topology the neutral `Connect`/`Generic` edges, and its restraint/load the typed `PropertyValue` edge payloads; the entity→bag reading is ONE polymorphic `Attrs` discriminating by input value and a `RestraintAttrs`/`LoadAttrs`/`GroupAttrs`/`ModelAttrs` sibling-method family is the deleted form; the structural reader is the DEEP half `SemanticProjector` composes and re-introducing it as a standalone `IElementProjection` (a second projector minting the member nodes the general fold already mints) is the deleted form; the analytical line rides the member `Object`'s `Axis`-keyed content hash in `RepresentationContentHash` (content-keyed at `ObjectNode` by `IfcRepresentation.Keys`, resolved one-hop by content key in `Rasm.Compute`), and an inline `AxisCurve`/`Vector3` analytical-coordinate field on the seam node — like a RhinoCommon `Curve`/`Brep` field or an in-process BRep tessellation — is the named §4-RT-M2 seam violation, the deleted form (every geometry, display body and analytical line alike, routes the `RepresentationContentHash` content key); the restraint preserves the SI spring stiffness as a `MeasureValue` per DOF [H2] and a boolean-only fixity that drops the spring magnitude is the deleted form; the load family is read over the full `IfcStructuralLoad` subtype set and a single-force-only reader is the deleted form; the member↔connection topology is the seam `Generic` edge (wire-name `IfcRelConnectsStructuralMember`) the `EdgeProjection.Structural` fold authors and a typed `MemberConnection` record is the deleted form; the content-key identity is the seam `ElementGraph` content address (the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`) the consumer reads the graph by, and minting a second `(GeometryKey, PropertyKey)` scheme or reaching the up-stratum `Rasm.Compute` `InterchangeIdentity` is the named cross-folder drift defect [H7]; the GeometryGym structural-analysis surface is consumed as settled vocabulary (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) and a hand-rolled structural-member reader is the deleted form; the reader is TOTAL and routing a structural enrichment onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the class/reference rails are the general fold's `Fin<GraphDelta>`).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm;
using Rasm.Element;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [OPERATIONS] -------------------------------------------------------------------------
// The structural-analysis-domain reader Projection/semantic#SEMANTIC_PROJECTOR composes: it lowers the
// GeometryGym structural-analysis entities onto NEUTRAL seam payloads (a Map<PropertyName,PropertyValue>
// attribute bag a Generic edge or a structural PropertySet carries, plus the AtStart endpoint discriminant),
// never a parallel AnalysisModel store. The analytical line is NOT a payload here — it is content-keyed into
// the member Object's Representations under "Axis" at ObjectNode and resolved one-hop by content key downstream.
// The readers are TOTAL — an unreadable structural detail yields a None/empty enrichment, never a fault; the
// element identity, entity class, and dangling-endpoint rails are the general projector's Fin<GraphDelta>
// concern, so the structural reader never re-cases BimFault.
public static class StructuralProjection {
    // The SI dimensions the structural measures stamp: the force-derived dimensions COMPOSE from the seam
    // Dimension.ForceDim/LengthDim/AreaDim algebra so a hand-coded exponent table never drifts from the quantity
    // registry — N.m (moment / node rotational stiffness), N/m (line force / node translational stiffness),
    // N/m^2 (planar force / edge line stiffness) — and the temperature delta is the SI base temperature dimension (K).
    static readonly Dimension Moment        = Dimension.ForceDim.Multiply(Dimension.LengthDim);
    static readonly Dimension ForcePerLength = Dimension.ForceDim.Divide(Dimension.LengthDim);
    static readonly Dimension ForcePerArea   = Dimension.ForceDim.Divide(Dimension.AreaDim);
    static readonly Dimension TemperatureDelta = Dimension.Create(0, 0, 0, 0, 1, 0, 0);

    static readonly Seq<string> LoadKinds = Seq(
        "IfcStructuralLoadSingleForce", "IfcStructuralLoadLinearForce", "IfcStructuralLoadPlanarForce",
        "IfcStructuralLoadTemperature", "IfcStructuralLoadSingleDisplacement");

    // The LoadGroupType Enumerated allowed-set is DERIVED from the GeometryGym enum (LOAD_GROUP / LOAD_CASE /
    // LOAD_COMBINATION / USERDEFINED / NOTDEFINED) so it never drifts from the schema roster a hand-listed subset did.
    static readonly Seq<string> LoadGroupKinds = Enum.GetNames<IfcLoadGroupTypeEnum>().ToSeq();

    // The neutral load-CASE token Rasm.Compute reads off the activity edge, DERIVED from the schema's load-source taxonomy
    // (IfcActionSourceTypeEnum) — the load group's ActionSource mapped onto the dead/live/snow/wind/seismic case the
    // Analysis/structural LoadCombinationSpec factors; an unmapped source folds to the unfactored case the consumer defaults.
    static readonly Map<IfcActionSourceTypeEnum, string> CaseSources = toMap(Seq(
        (IfcActionSourceTypeEnum.DEAD_LOAD_G, "dead"), (IfcActionSourceTypeEnum.LIVE_LOAD_Q, "live"),
        (IfcActionSourceTypeEnum.SNOW_S, "snow"), (IfcActionSourceTypeEnum.WIND_W, "wind"),
        (IfcActionSourceTypeEnum.EARTHQUAKE_E, "seismic")));

    // --- [ATTRIBUTES] -------------------------------------------------------------------------
    // ONE polymorphic structural attribute-bag reader discriminating on the entity shape — never a RestraintAttrs/
    // LoadAttrs/GroupAttrs sibling family. A connection's restraint and an activity's load ride a Generic edge; a
    // load-group / analysis-model / surface-member definition rides a structural PropertySet node; the caller (the
    // SemanticProjector Enrich bake + EdgeProjection.Structural fold) decides attachment. A non-structural or null entity
    // yields the empty bag (a graceful skip). The egress Filter drops every non-finite Measure so the families whose
    // public getter surfaces the unset sentinel never emit NaN: IfcStructuralLoadTemperature (DeltaT_* raw auto-property,
    // unset = NaN), IfcStructuralLoadGroup.Coefficient, and IfcStructuralSurfaceMember.Thickness all read NaN unset and
    // drop here. The IfcStructuralLoad force families (Single/Linear/Planar) are NOT in that set — GeometryGym 25.7.30
    // backs each force/moment component with a NaN field but the public getter COERCES unset NaN -> 0.0, so an unset force
    // component reads a deliberate 0 (a zero force, harmless to the FE consumer) the Filter cannot distinguish from a real
    // 0; the drop is a NaN guard over the raw-sentinel surfaces, not a universal unset-component eliminator.
    public static Map<PropertyName, PropertyValue> Attrs(BaseClassIfc? entity) => (entity switch {
        IfcStructuralConnection c    => RestraintOf(c),
        IfcStructuralActivity a      => LoadOf(a),
        IfcStructuralLoadGroup g     => Map(
            (PropertyName.Create("LoadGroupType"), (PropertyValue)new PropertyValue.Enumerated(Seq(g.PredefinedType.ToString()), LoadGroupKinds)),
            (PropertyName.Create("ActionType"),    new PropertyValue.Text(g.ActionType.ToString())),
            (PropertyName.Create("ActionSource"),  new PropertyValue.Text(g.ActionSource.ToString())),
            (PropertyName.Create("Coefficient"),   new PropertyValue.Measure(MeasureValue.OfSi(Dimension.Dimensionless, g.Coefficient))),
            (PropertyName.Create("Purpose"),       new PropertyValue.Text(g.Purpose ?? ""))),
        IfcStructuralAnalysisModel m => Map(
            (PropertyName.Create("AnalysisTheory"),   (PropertyValue)new PropertyValue.Text(m.PredefinedType.ToString())),
            (PropertyName.Create("LoadGroupCount"),   new PropertyValue.Measure(MeasureValue.OfSi(Dimension.Dimensionless, m.LoadedBy.Count))),
            (PropertyName.Create("ResultGroupCount"), new PropertyValue.Measure(MeasureValue.OfSi(Dimension.Dimensionless, m.HasResults.Count)))),
        IfcStructuralSurfaceMember s => Map(
            (PropertyName.Create("Thickness"), (PropertyValue)new PropertyValue.Measure(MeasureValue.OfSi(Dimension.LengthDim, s.Thickness)))),
        _                            => Map<PropertyName, PropertyValue>(),
    }).Filter(static v => v is not PropertyValue.Measure m || double.IsFinite(m.Value.Si));

    // --- [RESTRAINT] --------------------------------------------------------------------------
    // The six-DOF support condition the IfcRelConnectsStructuralMember Generic edge carries: a fixity Boolean PLUS
    // the SI spring-stiffness magnitude per DOF [H2], so Rasm.Compute reads BOTH a pinned/fixed support and a finite
    // spring off the edge (the prior boolean-only reader dropped the stiffness). A node condition reads its 6 stiffness
    // selects, an edge condition its 6 by-length selects; the four select types each expose a Rigid/Stiffness shape (no
    // shared base — see Dof) so the per-DOF type switch reduces every DOF. An absent condition — or a face condition,
    // whose area-stiffness GeometryGym 25.7.30 exposes ONLY as internal fields (no public properties) — yields the empty (free) bag.
    static Map<PropertyName, PropertyValue> RestraintOf(IfcStructuralConnection connection) =>
        Optional(connection.AppliedCondition).Match(
            Some: condition => condition switch {
                IfcBoundaryNodeCondition n => SixDof(
                    (n.TranslationalStiffnessX, n.TranslationalStiffnessY, n.TranslationalStiffnessZ),
                    (n.RotationalStiffnessX, n.RotationalStiffnessY, n.RotationalStiffnessZ),
                    ForcePerLength, Moment),
                IfcBoundaryEdgeCondition e => SixDof(
                    (e.LinearStiffnessByLengthX, e.LinearStiffnessByLengthY, e.LinearStiffnessByLengthZ),
                    (e.RotationalStiffnessByLengthX, e.RotationalStiffnessByLengthY, e.RotationalStiffnessByLengthZ),
                    ForcePerArea, Dimension.ForceDim),
                _ => Map<PropertyName, PropertyValue>(),
            },
            None: () => Map<PropertyName, PropertyValue>());

    static Map<PropertyName, PropertyValue> SixDof(
        (object? X, object? Y, object? Z) translation, (object? X, object? Y, object? Z) rotation,
        Dimension translationDim, Dimension rotationDim) =>
        Seq<(string Fixity, string Spring, object? Select, Dimension Dim)>(
            ("TranslationX", "TranslationKx", translation.X, translationDim),
            ("TranslationY", "TranslationKy", translation.Y, translationDim),
            ("TranslationZ", "TranslationKz", translation.Z, translationDim),
            ("RotationX",    "RotationKx",    rotation.X,    rotationDim),
            ("RotationY",    "RotationKy",    rotation.Y,    rotationDim),
            ("RotationZ",    "RotationKz",    rotation.Z,    rotationDim))
        .Fold(Map<PropertyName, PropertyValue>(), static (acc, dof) => {
            var (fixity, spring) = Dof(dof.Select, dof.Dim);
            return acc.Add(PropertyName.Create(dof.Fixity), fixity).Add(PropertyName.Create(dof.Spring), spring);
        });

    // ONE reading per DOF select: the fixity Boolean AND the SI spring magnitude from one four-arm type switch over
    // GeometryGym's SPLIT select hierarchy (IfcTranslationalStiffnessSelect + the two subgrade-reaction selects derive
    // StiffnessSelect<TMeasure>; IfcRotationalStiffnessSelect is standalone) — no common base unifies them, so a single
    // property pattern is impossible, but all four independently expose a Rigid Boolean + a Stiffness measure whose
    // .Measure rides IfcDerivedMeasureValue, so the prior Fixity/SpringOf split that pattern-matched every DOF twice
    // collapses to one reader. A DOF is fixed when rigid OR carrying a finite positive spring; the magnitude is 0 for a
    // rigid or free DOF and the finite spring otherwise [H2] (a NaN stiffness reads free, dropped at the Attrs egress).
    static (PropertyValue Fixity, PropertyValue Spring) Dof(object? select, Dimension dimension) {
        (bool Rigid, double Si) reading = select switch {
            IfcTranslationalStiffnessSelect s                 => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcRotationalStiffnessSelect s                    => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcModulusOfTranslationalSubgradeReactionSelect s => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            IfcModulusOfRotationalSubgradeReactionSelect s    => (s.Rigid, s.Stiffness?.Measure ?? 0d),
            _                                                 => (false, 0d),
        };
        return (new PropertyValue.Boolean(reading.Rigid || reading.Si > 0d),
                new PropertyValue.Measure(MeasureValue.OfSi(dimension, reading.Rigid ? 0d : reading.Si)));
    }

    // --- [LOAD] -------------------------------------------------------------------------------
    // The applied load the IfcRelConnectsStructuralActivity Generic edge carries: typed force/moment/pressure/temperature
    // MeasureValue components over the IfcStructuralLoad family PLUS the load-type token, the global/local frame, and the
    // source name (the prior single-force-only reader dropped the line/planar/temperature families). A configuration load
    // the switch does not enumerate — and IfcStructuralLoadSingleDisplacement, whose components are internal-field-only —
    // yields the frame attrs only, a graceful passthrough; a Temperature DeltaT_* unset = NaN drops at the Attrs egress,
    // while a force/moment component the public getter coerced to 0.0 emits a 0 (the getter masks the unset sentinel, so
    // the Filter NaN-guards the raw-sentinel surfaces and never suppresses a coerced 0).
    static Map<PropertyName, PropertyValue> LoadOf(IfcStructuralActivity activity) =>
        Optional(activity.AppliedLoad).Match(
            Some: load => Vectors(load).Fold(
                Map(
                    (PropertyName.Create("LoadType"),      (PropertyValue)new PropertyValue.Enumerated(Seq(load.GetType().Name), LoadKinds)),
                    (PropertyName.Create("LoadKind"),      new PropertyValue.Text(KindOf(load))),
                    (PropertyName.Create("Case"),          new PropertyValue.Text(CaseOf(activity))),
                    (PropertyName.Create("GlobalOrLocal"), new PropertyValue.Text(activity.GlobalOrLocal.ToString())),
                    (PropertyName.Create("Source"),        new PropertyValue.Text(activity.Name ?? ""))),
                static (acc, e) => acc.Add(PropertyName.Create(e.Name), new PropertyValue.Measure(MeasureValue.OfSi(e.Dim, e.Si)))),
            None: () => Map<PropertyName, PropertyValue>());

    // The Rasm.Compute FE idealization kind (point/uniform/trapezoid) the IfcStructuralLoad class lowers onto, stamped
    // ALONGSIDE the faithful IFC LoadType: a single force is a point action, a linear force a uniform line action — the
    // neutral token Analysis/structural's ToLoad reads. IFC carries no trapezoid primitive (a varying line action is a
    // Compute synthesis), so the lowering yields point/uniform only, never a fabricated trapezoid.
    static string KindOf(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadLinearForce => "uniform",
        _                            => "point",
    };

    // The neutral load CASE walked off the activity's IfcStructuralLoadGroup assignment ActionSource (HasAssignments ->
    // IfcRelAssignsToGroup.RelatingGroup -> IfcStructuralLoadGroup.ActionSource) through CaseSources, so Analysis/structural
    // factors the action under the correct LoadCombinationSpec case. An ungrouped or NOTDEFINED-source activity folds to
    // "live", the unfactored variable case the consumer defaults to, never a silently-miscased action.
    static string CaseOf(IfcStructuralActivity activity) =>
        activity.HasAssignments.AsIterable()
            .Choose(static a => a is IfcRelAssignsToGroup { RelatingGroup: IfcStructuralLoadGroup g } ? Some(g.ActionSource) : None)
            .ToSeq().Head
            .Bind(CaseSources.Find)
            .IfNone("live");

    static Seq<(string Name, double Si, Dimension Dim)> Vectors(IfcStructuralLoad load) => load switch {
        IfcStructuralLoadSingleForce f => Seq(
            ("ForceX", f.ForceX, Dimension.ForceDim), ("ForceY", f.ForceY, Dimension.ForceDim), ("ForceZ", f.ForceZ, Dimension.ForceDim),
            ("MomentX", f.MomentX, Moment), ("MomentY", f.MomentY, Moment), ("MomentZ", f.MomentZ, Moment)),
        IfcStructuralLoadLinearForce l => Seq(
            ("LinearForceX", l.LinearForceX, ForcePerLength), ("LinearForceY", l.LinearForceY, ForcePerLength), ("LinearForceZ", l.LinearForceZ, ForcePerLength),
            ("LinearMomentX", l.LinearMomentX, Dimension.ForceDim), ("LinearMomentY", l.LinearMomentY, Dimension.ForceDim), ("LinearMomentZ", l.LinearMomentZ, Dimension.ForceDim)),
        IfcStructuralLoadPlanarForce p => Seq(
            ("PlanarForceX", p.PlanarForceX, ForcePerArea), ("PlanarForceY", p.PlanarForceY, ForcePerArea), ("PlanarForceZ", p.PlanarForceZ, ForcePerArea)),
        IfcStructuralLoadTemperature t => Seq(
            ("DeltaTConstant", t.DeltaT_Constant, TemperatureDelta), ("DeltaTY", t.DeltaT_Y, TemperatureDelta), ("DeltaTZ", t.DeltaT_Z, TemperatureDelta)),
        // IfcStructuralLoadSingleDisplacement holds its DisplacementX/Y/Z + RotationalDisplacementRX/RY/RZ as INTERNAL
        // fields in GeometryGym 25.7.30 — NO public accessor crosses the assembly boundary — so a prescribed-displacement
        // (support-settlement) load reads the frame attrs only (LoadType/LoadKind/Case/GlobalOrLocal/Source via LoadOf),
        // the documented surface boundary rather than a phantom `d.DisplacementX` read or a silently-invented 0-settlement;
        // the `_` graceful passthrough owns it alongside any unenumerated load family, never a fabricated component.
        _ => Seq<(string, double, Dimension)>(),
    };

    // --- [ENDPOINT] ---------------------------------------------------------------------------
    // The start/end discriminant the IfcRelConnectsStructuralMember Generic edge carries so Rasm.Compute resolves a support
    // to the correct member joint: the point connection's vertex compared to the member's analytical-edge endpoints (nearer
    // Start -> true), the Projection/semantic#SEMANTIC_PROJECTOR EdgeProjection.Structural fold stamping it per edge. The
    // endpoint coordinates are read TRANSIENTLY off GeometryGym topology to compute the boolean — never stored on the seam
    // node (the analytical line itself rides the Axis-keyed content hash in Representations). A member with no analytical
    // edge or a connection with no vertex folds to the end joint (false — the consumer's WireAtStart default), so an
    // unresolved endpoint never silently claims the start joint.
    public static bool AtStart(IfcStructuralCurveMember? member, IfcStructuralConnection? connection) =>
        (from m in Optional(member)
         from edge in EdgeOf(m.Representation)
         from c in Optional(connection as IfcStructuralPointConnection)
         from v in Optional(c.Vertex)
         let vertex = PointOf(v)
         select Vector3.Distance(vertex, PointOf(edge.EdgeStart)) <= Vector3.Distance(vertex, PointOf(edge.EdgeEnd))).IfNone(false);

    // The member's analytical topology edge, read transiently off the inherited IfcProduct.Representation for the AtStart
    // distance compare ONLY — the coordinates produce the boolean discriminant and are never carried onto a seam node.
    static Option<IfcEdge> EdgeOf(IfcProductDefinitionShape? shape) =>
        Optional(shape).Bind(static s => s.Representations.AsIterable()
            .SelectMany(static rep => rep.Items.AsIterable())
            .Choose(static item => item is IfcEdge e ? Some(e) : None)
            .ToSeq().Head);

    static Vector3 PointOf(IfcVertex? vertex) =>
        vertex is IfcVertexPoint { VertexGeometry: IfcCartesianPoint { Coordinates: { Count: >= 3 } c } }
            ? new Vector3(c[0], c[1], c[2])
            : new Vector3(0d, 0d, 0d);
}
```

## [03]-[RESEARCH]

- [STRUCTURAL_SURFACE_DISPATCH]: the structural-analysis entity surface `StructuralProjection` reads grounds against the live GeometryGym 25.7.30 decompile (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) — `IfcStructuralCurveMember : IfcStructuralMember` carries `PredefinedType` (`IfcStructuralCurveMemberTypeEnum`) and `Axis` (`IfcDirection`), `IfcStructuralSurfaceMember` carries `Thickness` (double) and `PredefinedType`, `IfcStructuralConnection` (abstract) carries `AppliedCondition` (`IfcBoundaryCondition`), `IfcStructuralActivity : IfcProduct` (abstract) carries `AppliedLoad` (`IfcStructuralLoad`) and `GlobalOrLocal` (`IfcGlobalOrLocalEnum`), `IfcStructuralAnalysisModel : IfcSystem` carries `PredefinedType` (`IfcAnalysisModelTypeEnum`)/`OrientationOf2DPlane`/`LoadedBy` (`SET<IfcStructuralLoadGroup>`)/`HasResults` (`SET<IfcStructuralResultGroup>`), and `IfcStructuralLoadGroup : IfcGroup` carries `PredefinedType` (`IfcLoadGroupTypeEnum`)/`ActionType`/`ActionSource`/`Coefficient`/`Purpose` — so the reader discriminates the real analysis graph, the idealized members landing as seam `Object` nodes through the general `Extract<IfcProduct>` fold (an `IfcStructuralItem` IS an `IfcProduct`) and the member↔connection / member↔activity / group-membership topology landing as neutral `Connect`/`Generic`/`Assign` edges through `Projection/relations#RELATION_ALGEBRA`, never a hand-rolled structural-member reader and never a parallel analysis store.
- [BOUNDARY_CONDITION_RESTRAINT]: the six-DOF restraint the `Attrs` restraint arm reads grounds against the live decompile — `IfcBoundaryNodeCondition : IfcBoundaryCondition` carries `TranslationalStiffnessX`/`Y`/`Z` (`IfcTranslationalStiffnessSelect`) and `RotationalStiffnessX`/`Y`/`Z` (`IfcRotationalStiffnessSelect`); `IfcBoundaryEdgeCondition` carries `LinearStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfTranslationalSubgradeReactionSelect`) and `RotationalStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfRotationalSubgradeReactionSelect`). `IfcTranslationalStiffnessSelect`/`IfcModulusOfTranslationalSubgradeReactionSelect`/`IfcModulusOfRotationalSubgradeReactionSelect` derive the generic `StiffnessSelect<TMeasure>` (over `IfcLinearStiffnessMeasure`/`IfcModulusOfLinearSubgradeReactionMeasure`/`IfcModulusOfRotationalSubgradeReactionMeasure`) and `IfcRotationalStiffnessSelect` is standalone, but ALL FOUR expose the identical public shape — `bool Rigid` and `Stiffness` returning a measure whose `.Measure` double rides `IfcDerivedMeasureValue` — so one four-arm type switch (no common base permits a single property pattern across the split hierarchy) reduces each DOF, each arm reading the shared `Rigid` Boolean + `Stiffness.Measure` double: a rigid support reads `Rigid == true`, a finite spring reads `Stiffness.Measure > 0`, an absent select reads free. The reader preserves the SI spring stiffness as a `MeasureValue` per DOF [H2] — `IfcLinearStiffnessMeasure` (N/m) onto the node translational, `IfcModulusOfLinearSubgradeReactionMeasure` (N/m²) onto the edge line stiffness — where the prior boolean-only restraint dropped the magnitude the FE constraint needs. `IfcBoundaryFaceCondition` (a surface connection) holds its `TranslationalStiffnessByArea*` only as INTERNAL fields in 25.7.30 — no public accessor — so a face restraint reads free, the documented boundary limit rather than a silently invented stiffness.
- [LOAD_FAMILY]: the applied-load the `Attrs` load arm reads grounds against the live decompile — `IfcStructuralLoadSingleForce : IfcStructuralLoadStatic` carries `ForceX`/`Y`/`Z` and `MomentX`/`Y`/`Z` (double), `IfcStructuralLoadLinearForce` carries `LinearForceX`/`Y`/`Z` and `LinearMomentX`/`Y`/`Z`, `IfcStructuralLoadPlanarForce` carries `PlanarForceX`/`Y`/`Z`, and `IfcStructuralLoadTemperature` carries `DeltaT_Constant`/`DeltaT_Y`/`DeltaT_Z` as PUBLIC properties — so the reader covers the force/moment/pressure/temperature components rather than single-force only, each an SI `MeasureValue` on its composed `Dimension` (`Dimension.ForceDim` for force, `Dimension.ForceDim.Multiply(Dimension.LengthDim)` for moment, `Dimension.ForceDim.Divide(Dimension.LengthDim)` for line force, `Dimension.ForceDim.Divide(Dimension.AreaDim)` for planar force, the seven-base temperature delta for `DeltaT_*`), the `Dimension` composing from the seam quantity registry rather than a hand-coded exponent table; `IfcStructuralLoadSingleDisplacement` is in the family but exposes its `DisplacementX`/`Y`/`Z` + `RotationalDisplacementRX`/`RY`/`RZ` ONLY as internal fields (no public accessor in 25.7.30), so a prescribed-displacement load lands the frame attrs alone — the documented surface boundary, never a phantom component read; a configuration load the switch does not enumerate rides the frame attrs (`LoadType`/`GlobalOrLocal`/`Source`) as a graceful passthrough, a new load family folding onto one switch arm; the GeometryGym getter convention splits the unset behavior — `DeltaT_*` is a raw auto-property returning its NaN default (so an unset temperature drops at the `Attrs` egress `Filter`) while the force/moment/planar getters coerce an unset NaN field to 0.0 (so an unset force component emits a deliberate 0 the Filter cannot distinguish), the Filter therefore a NaN guard over the raw-sentinel surfaces rather than a universal unset-component eliminator. The neutral consumer tokens ride the same arm: the FE `LoadKind` derives from the load class (`IfcStructuralLoadSingleForce`->point, `IfcStructuralLoadLinearForce`->uniform), and the `Case` walks the activity's group assignment — `IfcObjectDefinition.HasAssignments` (`SET<IfcRelAssigns>`) filtered to `IfcRelAssignsToGroup` whose `RelatingGroup` is an `IfcStructuralLoadGroup`, its `ActionSource` (`IfcActionSourceTypeEnum`: `DEAD_LOAD_G`/`LIVE_LOAD_Q`/`SNOW_S`/`WIND_W`/`EARTHQUAKE_E`) mapped onto the neutral dead/live/snow/wind/seismic token, all grounded against the live decompile.
- [ANALYTICAL_GEOMETRY]: the idealized analytical line is content-keyed, never inline-baked — at `Projection/semantic#SEMANTIC_PROJECTOR` `ObjectNode` the inherited `IfcProduct.Representation` (`IfcProductDefinitionShape`) folds through the ONE `IfcRepresentation.Keys` content-keyer, the analytical `IfcTopologyRepresentation` (`IfcShapeModel`) landing under the `Axis` `RepresentationIdentifier` as a content hash in the member `Object`'s `Graph/element#NODE_MODEL` `RepresentationContentHash` map alongside the heavy display `Body` [M2], so `Rasm.Compute` resolves the coordinate line one-hop BY CONTENT KEY from the blob store — an inline `AxisCurve`/`Vector3` coordinate field on the seam node is the named §4-RT-M2 violation, the deleted form, and re-minting an analysis solver or a second content-key identity in the Bim folder is the named cross-folder drift defect, the solve and the typed analysis model living wholly in `Rasm.Compute` (`Rasm.Compute/Analysis/structural`). The `AtStart` endpoint discriminant grounds against the live decompile — the member's analytical edge rides the `IfcTopologyRepresentation` `Items` as an `IfcEdge`/`IfcEdgeCurve` (`EdgeStart`/`EdgeEnd` `IfcVertex`) whose endpoint coordinates ride `IfcVertexPoint.VertexGeometry` (`IfcCartesianPoint.Coordinates` `LIST<double>`), and the `IfcStructuralPointConnection.Vertex` (an `IfcVertexPoint` off the connection's topology) — so `AtStart` reads those endpoints TRANSIENTLY (the seam `Rasm.Element` `Vector3` distance compare producing only the boolean edge attribute, never a stored coordinate) and the `EdgeProjection.Structural` fold resolves a support to the nearer member joint rather than the prior always-the-end default.
