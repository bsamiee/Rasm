# [STRUCTURAL_PROJECTION]

The structural-analysis-domain reader the `Projection/semantic#SEMANTIC_PROJECTOR` `SemanticProjector` composes: `StructuralProjection` lowers the GeometryGym structural-analysis entity surface onto NEUTRAL seam payloads — the idealized `Graph/element#NODE_MODEL` `AxisCurve` baked onto a structural member's `Object` node, and the `Map<PropertyName, PropertyValue>` attribute bag a `Relations/relation#EDGE_ALGEBRA` `Relationship.Generic` edge or a structural `Properties/property#PROPERTY_BAG` `PropertySet` node carries — so a `Rasm.Compute` frame solve reads the idealization off the ONE `Graph/element#ELEMENT_GRAPH` `ElementGraph` it already holds, never a second store. This RETIRES the migration source's parallel `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint` record family keyed by `BimModel`/`GlobalId` (the very "second stored record off the element" the rebuild forbids): an idealized curve member is the seam `Object` node it already is (an `IfcStructuralItem` is an `IfcProduct`, so the general projector mints it), its analytical line is the baked `Object.Axis`, the member↔connection 6-DOF restraint and member↔activity applied load are the `Generic` edge payloads `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection.Structural` carries, and the analysis-model / load-group grouping is the `Assign.Group` edge the general `IfcRelAssignsToGroup` fold authors — never a re-modeled analysis mesh and never a parallel selection surface.

The owner is the deep STRUCTURAL half of the projection `SemanticProjector` keeps OUT of the general fold: the full `IfcBoundaryCondition` six-degree-of-freedom restraint algebra (preserving the SI spring-stiffness magnitude the prior boolean-only reader dropped [H2]), the full `IfcStructuralLoad` family (single force, line force, planar force, temperature, single displacement — not single-force-only), the analytical-line geometry walk (the topology-edge endpoints the seam `AxisCurve` bake needs), and the load-group / analysis-model / surface-member definition scalars a load combination and an analysis theory carry — the structural-analysis-domain vocabulary one focused reader owns so the general projector stays generic, a peer of `Semantics/composition#MATERIAL_COMPOSITION` `MaterialProjection` and `Semantics/georeference#GEO_REFERENCE` `GeoReferenceProjector`. The reader is HOST-NEUTRAL and TOTAL: it binds the kernel `Rasm` geometry through `Vector3` coordinate reads (never a RhinoCommon type, never an in-process BRep evaluation), an unreadable analytical detail yields a `None`/empty enrichment, and the element identity, entity class, and dangling-reference rails are the general projector's `Fin<GraphDelta>` concern — the structural reader never re-cases `Model/faults#FAULT_BAND` `BimFault`.

## [01]-[INDEX]

- [01]-[STRUCTURAL_PROJECTION]: `StructuralProjection` the GeometryGym structural-analysis-domain reader — `AxisOf` the idealized-line topology-edge walk feeding the seam `Object.Axis` bake, `AtStart` the start/end endpoint discriminant the restraint edge carries, and `Attrs` the ONE polymorphic attribute-bag reader discriminating the structural entity onto its neutral `Map<PropertyName, PropertyValue>` payload (an `IfcStructuralConnection` onto the six-DOF restraint — fixity Boolean + SI stiffness `MeasureValue` per DOF [H2] over the node/edge condition selects; an `IfcStructuralActivity` onto the `IfcStructuralLoad` family force/moment/line/planar/temperature/displacement components PLUS the neutral FE `LoadKind` token and the load-group-`ActionSource`-derived `Case` token `Rasm.Compute` reads; an `IfcStructuralLoadGroup`/`IfcStructuralAnalysisModel`/`IfcStructuralSurfaceMember` onto its load-combination / analysis-theory / thickness definition) — each the seam payload the `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection.Structural` edge or a structural `PropertySet` node and the `Projection/semantic#SEMANTIC_PROJECTOR` `Enrich` bake compose.

## [02]-[STRUCTURAL_PROJECTION]

- Owner: `StructuralProjection` the static structural-analysis-domain reader `SemanticProjector` composes, lowering the GeometryGym structural-analysis surface onto neutral seam payloads — never a stored record. It owns the analytical-line `AxisOf` geometry walk and the polymorphic `Attrs` attribute-bag reader (one entry discriminating the structural entity onto its restraint / load / load-group / analysis-model / surface-member bag); the typed analysis structures the migration source minted (`AnalysisModel`, the `AnalysisMember` `[Union]`, `LoadGroup`, `Support`, `MemberConnection`, `SupportRestraint`, `StructuralLoadKind`, `StructuralCurveMemberKind`) are all GONE — the member is the seam `Object` node, the joint kind its `PredefinedType` token, the topology its neutral `Connect`/`Generic` edges, and the restraint/load the typed `PropertyValue` edge payloads.
- Entry: the readers are TOTAL pure projections the projector composes at ingest — `AxisOf(IfcStructuralCurveMember member)` returns the idealized `AxisCurve` (start/end/up) `Option<T>` (`None` when the member carries no analytical edge), the lightweight line the `Enrich` bake stamps on `Object.Axis`; `Attrs(BaseClassIfc? entity)` is the ONE polymorphic attribute-bag reader discriminating on the entity shape — an `IfcStructuralConnection` onto the six-DOF restraint bag the `IfcRelConnectsStructuralMember` `Generic` edge carries, an `IfcStructuralActivity` onto the applied-load bag the `IfcRelConnectsStructuralActivity` `Generic` edge carries (the faithful IFC `LoadType` + full force/moment family PLUS the neutral FE `LoadKind` token and the load-group-`ActionSource`-derived `Case` token `Rasm.Compute` reads), an `IfcStructuralLoadGroup`/`IfcStructuralAnalysisModel`/`IfcStructuralSurfaceMember` onto the structural `PropertySet` definition bag, and any other (or null) onto the empty bag — no `Fin` rail, because a structural enrichment never fails the whole import over one unreadable detail (the class miss is the general fold's `BimFault.UnmappedClass`, the dangling endpoint its `BimFault.DanglingReference`), and no `Get`/`RestraintAttrs`/`LoadAttrs` sibling family — one polymorphic `Attrs` discriminating by input value; `AtStart(IfcStructuralCurveMember?, IfcStructuralConnection?)` is the start/end endpoint discriminant the `EdgeProjection.Structural` fold stamps on the restraint edge (the connection vertex nearer the member's `AxisOf` start), so `Rasm.Compute` resolves a support to the correct member joint rather than always the end.
- Auto: `AxisOf` reads the member's inherited `IfcProduct.Representation` `IfcProductDefinitionShape`, folds its `Representations` `IfcShapeModel` set onto the first `IfcEdge`/`IfcEdgeCurve` topology item, and reads the `EdgeStart`/`EdgeEnd` `IfcVertexPoint.VertexGeometry` `IfcCartesianPoint.Coordinates` onto the kernel `Vector3` endpoints with the member's `Axis` `IfcDirection.DirectionRatios` as the local up (default `+Z`) — the lightweight analytical line, the heavy display geometry staying content-hashed on `RepresentationContentHash`; `Attrs`' restraint arm discriminates the `AppliedCondition` over `IfcBoundaryNodeCondition` (the `TranslationalStiffnessX`/`Y`/`Z` `IfcTranslationalStiffnessSelect` + `RotationalStiffnessX`/`Y`/`Z` `IfcRotationalStiffnessSelect`) and `IfcBoundaryEdgeCondition` (the `LinearStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfTranslationalSubgradeReactionSelect` + `RotationalStiffnessByLengthX`/`Y`/`Z` `IfcModulusOfRotationalSubgradeReactionSelect`), reducing each DOF through one four-arm type switch over GeometryGym's split select hierarchy (`IfcTranslationalStiffnessSelect` + the two subgrade-reaction selects derive `StiffnessSelect<TMeasure>`, `IfcRotationalStiffnessSelect` standalone — no common base unifies them, but each independently exposes a `Rigid` Boolean + a `Stiffness` measure whose `.Measure` rides `IfcDerivedMeasureValue`) onto a fixity `PropertyValue.Boolean` PLUS the SI spring stiffness `PropertyValue.Measure` [H2]; the load arm discriminates the `AppliedLoad` over the `IfcStructuralLoadSingleForce`/`LinearForce`/`PlanarForce`/`Temperature`/`SingleDisplacement` family onto typed force/moment/pressure/temperature/displacement `MeasureValue` components plus the load-type, frame, and source attrs AND the neutral FE `LoadKind` token (single force -> point, linear force -> uniform) and the `Case` token walked off the activity's `IfcStructuralLoadGroup.ActionSource`, every unset OPTIONAL component (NaN) dropped at the `Attrs` egress `Filter`; the group/model arms read the `IfcStructuralLoadGroup` `PredefinedType`/`ActionType`/`ActionSource`/`Coefficient`/`Purpose` and the `IfcStructuralAnalysisModel` `PredefinedType`/`LoadedBy`/`HasResults` onto the structural definition bags, and the surface-member arm its idealized `Thickness`.
- Receipt: the readers' payloads land on the ONE seam `ElementGraph` — the `AxisCurve` baked on the structural member's `Object.Axis`, the six-DOF restraint on the `IfcRelConnectsStructuralMember` `Generic` edge, the applied load on the `IfcRelConnectsStructuralActivity` `Generic` edge, and the load-group / analysis-model / surface-member definitions on structural `PropertySet` nodes — so the `Rasm.Compute` structural runner reads `graph.AxisOf(member)`, the support fixity/stiffness off the member's `Connect`/`Generic` incidence edges, and the load components off the activity edges to build its typed analysis view and FE solve, joining the section properties the `Graph/element#ELEMENT_GRAPH` `SectionOf` accessor bakes off the member's `ProfileSet` composition — the analysis owner producing the idealized graph, the solve and the typed `AnalysisModel` living wholly in `Rasm.Compute`, never re-projected here. A beam's analytical line, a slab's idealized thickness, a column-base node's six-DOF support, and a gravity load case each ride the one graph the consumer already holds.
- Packages: GeometryGymIFC_Core (the structural-analysis entity surface consumed as settled vocabulary), Rasm.Element (the seam `AxisCurve`/`PropertyName`/`PropertyValue`/`MeasureValue`/`Dimension` payloads), Rasm (the kernel `Vector3` analytical coordinate), LanguageExt.Core (`Option`/`Seq`/`Map`).
- Growth: a new boundary-condition kind is one arm on the `Attrs` restraint switch reading the next `IfcBoundaryCondition` subtype's stiffness selects through the SAME `object`-pattern reducer; a new applied-load family is one arm on the `Vectors` switch reading the next `IfcStructuralLoad` subtype's components; a new structural entity with a definition bag is one arm on the polymorphic `Attrs` switch; a new analytical geometry is one branch on the `AxisOf` topology walk; never a per-member-type analysis record, never a `RestraintAttrs`/`LoadAttrs` sibling family, never a second analysis store, never a re-modeled analytical mesh.
- Boundary: `StructuralProjection` produces ONLY neutral seam payloads — the migration `AnalysisModel`/`AnalysisMember`/`LoadGroup`/`Support`/`MemberConnection`/`SupportRestraint`/`StructuralLoadKind`/`StructuralCurveMemberKind` typed store is the deleted form, the idealized member being the seam `Object` node (an `IfcStructuralItem` IS an `IfcProduct` the general fold mints), its joint kind the `Object.PredefinedType` token, its topology the neutral `Connect`/`Generic` edges, and its restraint/load the typed `PropertyValue` edge payloads; the entity→bag reading is ONE polymorphic `Attrs` discriminating by input value and a `RestraintAttrs`/`LoadAttrs`/`GroupAttrs`/`ModelAttrs` sibling-method family is the deleted form; the structural reader is the DEEP half `SemanticProjector` composes and re-introducing it as a standalone `IElementProjection` (a second projector minting the member nodes the general fold already mints) is the deleted form; the analytical line/coordinate is kernel `Vector3` only and a RhinoCommon `Curve`/`Brep` field or an in-process BRep tessellation is the named seam violation (the heavy display geometry routes the `RepresentationContentHash` content key); the restraint preserves the SI spring stiffness as a `MeasureValue` per DOF [H2] and a boolean-only fixity that drops the spring magnitude is the deleted form; the load family is read over the full `IfcStructuralLoad` subtype set and a single-force-only reader is the deleted form; the member↔connection topology is the seam `Generic` edge (wire-name `IfcRelConnectsStructuralMember`) the `EdgeProjection.Structural` fold authors and a typed `MemberConnection` record is the deleted form; the content-key identity is the seam `ElementGraph` content address (the kernel seed-zero `XxHash128` over `Node.ToCanonicalBytes`) the consumer reads the graph by, and minting a second `(GeometryKey, PropertyKey)` scheme or reaching the up-stratum `Rasm.Compute` `InterchangeIdentity` is the named cross-folder drift defect [H7]; the GeometryGym structural-analysis surface is consumed as settled vocabulary (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) and a hand-rolled structural-member reader is the deleted form; the reader is TOTAL and routing a structural enrichment onto `Model/faults#FAULT_BAND` `BimFault` is the deleted form (the class/reference rails are the general fold's `Fin<GraphDelta>`).

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
// GeometryGym structural-analysis entities onto NEUTRAL seam payloads (an AxisCurve baked on the Object node;
// a Map<PropertyName,PropertyValue> attribute bag a Generic edge or a structural PropertySet carries), never a
// parallel AnalysisModel store. Both readers are TOTAL — an unreadable analytical detail yields a None/empty
// enrichment, never a fault; the element identity, entity class, and dangling-endpoint rails are the general
// projector's Fin<GraphDelta> concern, so the structural reader never re-cases BimFault.
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

    // --- [ANALYTICAL_GEOMETRY] ----------------------------------------------------------------
    // The idealized structural line Projection/semantic#SEMANTIC_PROJECTOR Enrich bakes onto Object.Axis so a
    // Rasm.Compute frame solve reads graph.AxisOf off the node: the topology-edge endpoints + the member's local
    // Axis up, kernel Vector3 only. None when the member carries no analytical edge — a graceful skip, never a fault.
    public static Option<AxisCurve> AxisOf(IfcStructuralCurveMember member) =>
        EdgeOf(member.Representation).Map(edge => new AxisCurve(
            PointOf(edge.EdgeStart), PointOf(edge.EdgeEnd), UpOf(member.Axis)));

    static Option<IfcEdge> EdgeOf(IfcProductDefinitionShape? shape) =>
        Optional(shape).Bind(static s => s.Representations.AsIterable()
            .SelectMany(static rep => rep.Items.AsIterable())
            .Choose(static item => item is IfcEdge e ? Some(e) : None)
            .HeadOrNone());

    static Vector3 PointOf(IfcVertex? vertex) =>
        vertex is IfcVertexPoint { VertexGeometry: IfcCartesianPoint { Coordinates: { Count: >= 3 } c } }
            ? new Vector3(c[0], c[1], c[2])
            : new Vector3(0d, 0d, 0d);

    static Vector3 UpOf(IfcDirection? axis) =>
        axis is { DirectionRatios: { Count: >= 3 } r } ? new Vector3(r[0], r[1], r[2]) : new Vector3(0d, 0d, 1d);

    // --- [ATTRIBUTES] -------------------------------------------------------------------------
    // ONE polymorphic structural attribute-bag reader discriminating on the entity shape — never a RestraintAttrs/
    // LoadAttrs/GroupAttrs sibling family. A connection's restraint and an activity's load ride a Generic edge; a
    // load-group / analysis-model / surface-member definition rides a structural PropertySet node; the caller (the
    // SemanticProjector Enrich bake + EdgeProjection.Structural fold) decides attachment. A non-structural or null entity
    // yields the empty bag (a graceful skip). The egress Filter drops every non-finite Measure so an unset OPTIONAL IFC
    // value (Coefficient/Thickness/a partially-specified force default to NaN) never emits a NaN or misleading-0 measure.
    public static Map<PropertyName, PropertyValue> Attrs(BaseClassIfc? entity) => (entity switch {
        IfcStructuralConnection c    => RestraintOf(c),
        IfcStructuralActivity a      => LoadOf(a),
        IfcStructuralLoadGroup g     => Map(
            (PropertyName.Create("LoadGroupType"), (PropertyValue)new PropertyValue.Enumerated(g.PredefinedType.ToString(), LoadGroupKinds)),
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
    // The applied load the IfcRelConnectsStructuralActivity Generic edge carries: typed force/moment/pressure/
    // temperature/displacement MeasureValue components over the IfcStructuralLoad family PLUS the load-type token, the
    // global/local frame, and the source name (the prior single-force-only reader dropped the line/planar/temperature/
    // displacement families). A configuration load the switch does not enumerate yields the frame attrs only — a graceful
    // passthrough; an unset OPTIONAL component (NaN) is dropped at the Attrs egress, never an emitted NaN force.
    static Map<PropertyName, PropertyValue> LoadOf(IfcStructuralActivity activity) =>
        Optional(activity.AppliedLoad).Match(
            Some: load => Vectors(load).Fold(
                Map(
                    (PropertyName.Create("LoadType"),      (PropertyValue)new PropertyValue.Enumerated(load.GetType().Name, LoadKinds)),
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
            .HeadOrNone()
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
        IfcStructuralLoadSingleDisplacement d => Seq(
            ("DisplacementX", d.DisplacementX, Dimension.LengthDim), ("DisplacementY", d.DisplacementY, Dimension.LengthDim), ("DisplacementZ", d.DisplacementZ, Dimension.LengthDim),
            ("RotationalDisplacementX", d.RotationalDisplacementRX, Dimension.Dimensionless), ("RotationalDisplacementY", d.RotationalDisplacementRY, Dimension.Dimensionless), ("RotationalDisplacementZ", d.RotationalDisplacementRZ, Dimension.Dimensionless)),
        _ => Seq<(string, double, Dimension)>(),
    };

    // --- [ENDPOINT] ---------------------------------------------------------------------------
    // The start/end discriminant the IfcRelConnectsStructuralMember Generic edge carries so Rasm.Compute resolves a support
    // to the correct member joint: the point connection's vertex compared to the member's analytical endpoints (nearer Start
    // -> true), the Projection/semantic#SEMANTIC_PROJECTOR EdgeProjection.Structural fold stamping it per edge off the SAME
    // AxisOf the Enrich bake reads. A member with no analytical axis or a connection with no vertex folds to the end joint
    // (false — the consumer's WireAtStart default), so an unresolved endpoint never silently claims the start joint.
    public static bool AtStart(IfcStructuralCurveMember? member, IfcStructuralConnection? connection) =>
        (from m in Optional(member)
         from axis in AxisOf(m)
         from c in Optional(connection as IfcStructuralPointConnection)
         from v in Optional(c.Vertex)
         select Vector3.Distance(PointOf(v), axis.Start) <= Vector3.Distance(PointOf(v), axis.End)).IfNone(false);
}
```

## [03]-[RESEARCH]

- [STRUCTURAL_SURFACE_DISPATCH]: the structural-analysis entity surface `StructuralProjection` reads grounds against the live GeometryGym 25.7.30 decompile (`.api/api-geometrygym-ifc` structural-analysis-domain rows `[01]`-`[16]`) — `IfcStructuralCurveMember : IfcStructuralMember` carries `PredefinedType` (`IfcStructuralCurveMemberTypeEnum`) and `Axis` (`IfcDirection`), `IfcStructuralSurfaceMember` carries `Thickness` (double) and `PredefinedType`, `IfcStructuralConnection` (abstract) carries `AppliedCondition` (`IfcBoundaryCondition`), `IfcStructuralActivity : IfcProduct` (abstract) carries `AppliedLoad` (`IfcStructuralLoad`) and `GlobalOrLocal` (`IfcGlobalOrLocalEnum`), `IfcStructuralAnalysisModel : IfcSystem` carries `PredefinedType` (`IfcAnalysisModelTypeEnum`)/`OrientationOf2DPlane`/`LoadedBy` (`SET<IfcStructuralLoadGroup>`)/`HasResults` (`SET<IfcStructuralResultGroup>`), and `IfcStructuralLoadGroup : IfcGroup` carries `PredefinedType` (`IfcLoadGroupTypeEnum`)/`ActionType`/`ActionSource`/`Coefficient`/`Purpose` — so the reader discriminates the real analysis graph, the idealized members landing as seam `Object` nodes through the general `Extract<IfcProduct>` fold (an `IfcStructuralItem` IS an `IfcProduct`) and the member↔connection / member↔activity / group-membership topology landing as neutral `Connect`/`Generic`/`Assign` edges through `Projection/semantic#RELATION_ALGEBRA`, never a hand-rolled structural-member reader and never a parallel analysis store.
- [BOUNDARY_CONDITION_RESTRAINT]: the six-DOF restraint the `Attrs` restraint arm reads grounds against the live decompile — `IfcBoundaryNodeCondition : IfcBoundaryCondition` carries `TranslationalStiffnessX`/`Y`/`Z` (`IfcTranslationalStiffnessSelect`) and `RotationalStiffnessX`/`Y`/`Z` (`IfcRotationalStiffnessSelect`); `IfcBoundaryEdgeCondition` carries `LinearStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfTranslationalSubgradeReactionSelect`) and `RotationalStiffnessByLengthX`/`Y`/`Z` (`IfcModulusOfRotationalSubgradeReactionSelect`). `IfcTranslationalStiffnessSelect`/`IfcModulusOfTranslationalSubgradeReactionSelect`/`IfcModulusOfRotationalSubgradeReactionSelect` derive the generic `StiffnessSelect<TMeasure>` (over `IfcLinearStiffnessMeasure`/`IfcModulusOfLinearSubgradeReactionMeasure`/`IfcModulusOfRotationalSubgradeReactionMeasure`) and `IfcRotationalStiffnessSelect` is standalone, but ALL FOUR expose the identical public shape — `bool Rigid` and `Stiffness` returning a measure whose `.Measure` double rides `IfcDerivedMeasureValue` — so one four-arm type switch (no common base permits a single property pattern across the split hierarchy) reduces each DOF, each arm reading the shared `Rigid` Boolean + `Stiffness.Measure` double: a rigid support reads `Rigid == true`, a finite spring reads `Stiffness.Measure > 0`, an absent select reads free. The reader preserves the SI spring stiffness as a `MeasureValue` per DOF [H2] — `IfcLinearStiffnessMeasure` (N/m) onto the node translational, `IfcModulusOfLinearSubgradeReactionMeasure` (N/m²) onto the edge line stiffness — where the prior boolean-only restraint dropped the magnitude the FE constraint needs. `IfcBoundaryFaceCondition` (a surface connection) holds its `TranslationalStiffnessByArea*` only as INTERNAL fields in 25.7.30 — no public accessor — so a face restraint reads free, the documented boundary limit rather than a silently invented stiffness.
- [LOAD_FAMILY]: the applied-load the `Attrs` load arm reads grounds against the live decompile — `IfcStructuralLoadSingleForce : IfcStructuralLoadStatic` carries `ForceX`/`Y`/`Z` and `MomentX`/`Y`/`Z` (double), `IfcStructuralLoadLinearForce` carries `LinearForceX`/`Y`/`Z` and `LinearMomentX`/`Y`/`Z`, `IfcStructuralLoadPlanarForce` carries `PlanarForceX`/`Y`/`Z`, and `IfcStructuralLoadTemperature` carries `DeltaT_Constant`/`DeltaT_Y`/`DeltaT_Z` — so the reader covers the full force/moment/pressure/temperature/displacement family rather than single-force only, each component an SI `MeasureValue` on its composed `Dimension` (`Dimension.ForceDim` for force, `Dimension.ForceDim.Multiply(Dimension.LengthDim)` for moment, `Dimension.ForceDim.Divide(Dimension.LengthDim)` for line force, `Dimension.ForceDim.Divide(Dimension.AreaDim)` for planar force, the seven-base temperature delta for `DeltaT_*`, `Dimension.LengthDim`/`Dimension.Dimensionless` for the `IfcStructuralLoadSingleDisplacement` translation/rotation), the `Dimension` composing from the seam quantity registry rather than a hand-coded exponent table; a configuration load the switch does not enumerate rides the frame attrs (`LoadType`/`GlobalOrLocal`/`Source`) as a graceful passthrough, a new load family folding onto one switch arm; an unset OPTIONAL component (NaN, the GeometryGym double default) is dropped at the `Attrs` egress `Filter` so a partially-specified force never emits a NaN measure. The neutral consumer tokens ride the same arm: the FE `LoadKind` derives from the load class (`IfcStructuralLoadSingleForce`->point, `IfcStructuralLoadLinearForce`->uniform), and the `Case` walks the activity's group assignment — `IfcObjectDefinition.HasAssignments` (`SET<IfcRelAssigns>`) filtered to `IfcRelAssignsToGroup` whose `RelatingGroup` is an `IfcStructuralLoadGroup`, its `ActionSource` (`IfcActionSourceTypeEnum`: `DEAD_LOAD_G`/`LIVE_LOAD_Q`/`SNOW_S`/`WIND_W`/`EARTHQUAKE_E`) mapped onto the neutral dead/live/snow/wind/seismic token, all grounded against the live decompile.
- [ANALYTICAL_GEOMETRY]: the idealized-line `AxisOf` walk grounds against the live decompile — the inherited `IfcProduct.Representation` (`IfcProductDefinitionShape`) carries the analytical line as an `IfcTopologyRepresentation` (`IfcShapeModel`) whose `Items` hold an `IfcEdge`/`IfcEdgeCurve` (`EdgeStart`/`EdgeEnd` `IfcVertex`), the endpoint coordinates riding `IfcVertexPoint.VertexGeometry` (`IfcCartesianPoint.Coordinates` `LIST<double>`) and the local up `IfcStructuralCurveMember.Axis` (`IfcDirection.DirectionRatios` `LIST<double>`) — so the seam `Graph/element#NODE_MODEL` `AxisCurve` (`Vector3 Start`/`End`/`Up`) the `Projection/semantic#SEMANTIC_PROJECTOR` `Enrich` bakes onto `Object.Axis` reads the real topology rather than a guessed shape, the lightweight analytical coordinate baked while the heavy display geometry stays content-hashed on `RepresentationContentHash` [M2]; the `Rasm.Compute` structural runner reads `graph.AxisOf` baked off the node, never re-reading IFC geometry, the solve and the typed analysis model living wholly in `Rasm.Compute` (`Rasm.Compute/Analysis/structural`) and re-minting an analysis solver or a second content-key identity in the Bim folder being the named cross-folder drift defect. The `AtStart` endpoint discriminant reads the `IfcStructuralPointConnection.Vertex` (an `IfcVertexPoint` the convenience accessor walks off the connection's `IfcTopologyRepresentation` items) and compares its `PointOf` coordinate to the member `AxisOf` endpoints, so the `EdgeProjection.Structural` fold resolves a support to the nearer member joint rather than the prior always-the-end default.
