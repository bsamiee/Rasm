# [RASM_ELEMENT_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[STRATA_AMENDMENT]-[QUEUED]: register `Rasm.Element` as the new lowest-AEC sub-stratum across the three architecture levels.
- Capability: `libs/.planning/architecture.md` adds `Rasm.Element` to the strata + package roster, `libs/.planning/planning-targets.md` registers the new package, and `libs/csharp/.planning/ARCHITECTURE.md` records the seam in the C# stack map — AEC peers now depend up on `{Rasm, Rasm.Element}`.
- Anchors: the strata amendment (a shared LOWER stratum aligning the peers without coupling), the acyclic-verified one-kernel-only insertion.
- Ripple: `libs` `[ARCHITECTURE_STRATA]`.

[PEER_SEAM_REFERENCES]-[QUEUED]: add the upward `ProjectReference ../Rasm.Element` to every AEC peer and consumer csproj.
- Capability: `Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`, `Rasm.Compute`, and `Rasm.Persistence` gain one upward project reference to the seam; `Rasm.AppHost` stays reference-light (an ArchUnitNET fitness rule forbids GeometryGym below the seam).
- Anchors: the central `Directory.Packages.props` (no new pin), the one-reference-per-peer DAG preservation.
- Ripple: `csharp:Rasm.Materials`/`Rasm.Bim`/`Rasm.Fabrication`/`Rasm.Compute`/`Rasm.Persistence` `[SEAM_PROJECT_REFERENCE]`.
- Atomic: one `<ProjectReference>` row per peer csproj.

[BIM_PROJECTOR_REBUILD]-[QUEUED]: rebuild `Rasm.Bim` as the seam's IFC projector, retiring the parallel element owner.
- Capability: `Semantics`/`Model` author `SemanticProjector:IElementProjection` (Project lowers GeometryGym → `GraphDelta`; Emit Bim-internal) and a Bim `IGraphConstraint` (IFC-semantic legality); RETIRE `BimElement`/`BimModel` + the stringly `PropertyBinding`/`QuantityBinding` + the seam-owned `PropertyValue`/`MeasureValue` half; KEEP `IfcClass`/`PredefinedType` + `PropertyKey`/bSDD/QTO-override/base-qty; emit the generic `Classification` onto `Object` nodes; map every `IfcRel*` onto the neutral `Relationship` algebra or the `Generic` passthrough; map `IfcMapConversion`→seam `GeoReference` twelve-tuple.
- Anchors: the `Projection/projection#PROJECTION_CONTRACT` floor, the `Relations/relation#EDGE_ALGEBRA` neutral algebra, the `Geospatial/reference#GEO_REFERENCE` tuple, GeometryGym (sole IFC owner).
- Ripple: `csharp:Rasm.Bim` `[ELEMENT_SEAM_PROJECTOR]`.

[MATERIALS_PROJECTOR_REBUILD]-[QUEUED]: rebuild `Rasm.Materials` as the seam's material projector.
- Capability: `Construction`/`Properties` author `MaterialProjector:IElementProjection` (Project-only; authoring the `Associate` material edge when given a non-empty element-id set); RETIRE `Materials.Element`/`MaterialAssignment`; route the `MaterialProperty` unions into the seam `Material`/`MaterialPropertySet` + `Assessment` inputs; `ProfileSet`→`ProfileRef` (one-hop VividOrange resolution); the intrinsic acoustic folds now seam-owned (`Composition/acoustic`); hand the multi-ply `AssemblyAggregator` to `Rasm.Compute`; the `Appearance` node content-keyed.
- Anchors: the `Composition/material#MATERIAL_NODE` family, the `Composition/acoustic#ACOUSTIC_FOLDS` shared `StcContourFit`, the `Relations/relation#EDGE_ALGEBRA` `Associate` usage.
- Ripple: `csharp:Rasm.Materials` `[ELEMENT_SEAM_PROJECTOR]`.

[PERSISTENCE_GRAPH_REBUILD]-[QUEUED]: full `Rasm.Persistence` rebuild over the `ElementGraph` + `GraphDelta` event body.
- Capability: `Element/` graph store-load roundtrip; `Version/` the Marten event SoR (the `GraphDelta` is the event body; inline `SingleStreamProjection` folds `GraphDelta`→`ElementGraph`; `AggregateSnapshot` bounds replay) beneath the bespoke op-log/CRDT/time-travel/`StructuralMerge`/causal-DAG engine; `Query/` the in-process `QuikGraph` topology + DuckDB columnar (AGE optional); the content-keyed object store for geometry/coverage blobs (write-blob-first).
- Anchors: the `Graph/delta#GRAPH_DELTA` `ReplayOnto` fold, the `Projection/address#CONTENT_ADDRESS` content key, the `Generator.Equals` `Inequalities` 3-way merge feed, Marten 9.11.
- Ripple: `csharp:Rasm.Persistence` `[ELEMENT_GRAPH_PERSISTENCE]`.

[COMPUTE_ASSESSMENT_RUNNERS]-[QUEUED]: `Rasm.Compute` owns the discipline-specific assessment I/O, runners, and the relocated `AssemblyAggregator`.
- Capability: Compute reads the concrete `ElementGraph` directly (above the seam, no interface), owns the structural/thermal/energy/acoustic/fire/LCA solver input-result shapes and runners (VividOrange/BriefFiniteElement/FEALiTE2D + NREL.OpenStudio + closed-form + EC3 REST), the multi-ply `AssemblyAggregator` (series-resistance U-value, rule-of-mixtures density, ISO 12354 layered STC through the seam `StcContourFit`), and writes `Assessment` result nodes back content-keyed on `(InputKey, Route)`.
- Anchors: the `Assessment/assessment#ASSESSMENT_NODE` generic receipt, the `Composition/acoustic#ACOUSTIC_FOLDS` shared kernel, the `Composition/material#MATERIAL_COMPOSITION` plies.
- Ripple: `csharp:Rasm.Compute` `[ELEMENT_ASSESSMENT_RUNNERS]`.

[PY_WIRE_ALIGNMENT]-[QUEUED]: align the Python companion to the seam content-key and typed wire vocabulary.
- Capability: the `python:geometry/ifc-companion` IFC→IfcOpenShell→GLB decoder and the interchange decoders align to the seam `RepresentationContentHash` content-key (one `XxHash128` seed) and the typed `Material`/`Property`/`Assessment`/`Classification` wire vocabulary — decode by content key, never re-mint.
- Anchors: the `Projection/address#CONTENT_ADDRESS` seed-zero rail, the wire-alignment-only architecture clause (no Materials/Bim/Persistence reference).
- Ripple: `python:geometry/ifc` `[SEAM_WIRE_ALIGNMENT]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[SEAM_CORPUS_BUILD]-[COMPLETE]: authored the full `Rasm.Element` design corpus — `Graph/element` (NodeId/Node `[Union]`/Header/frozen `ElementGraph`/incidence index/memoized `Bake`), `Graph/delta` (`GraphMutation`/HAMT `WorkingGraph`/structural edge law/`GraphDelta` event body/`ReplayOnto`), `Relations/relation` (neutral `Relationship` algebra + `MaterialUsage`), `Classification/classification` (`Classification`/`Discipline`), `Properties/property` (`PropertyValue`/`PropertyName`/`PropertyBag`/`QuantityBag`/`InheritanceMode`), `Properties/quantity` (`Dimension`/`MeasureValue`), `Composition/material` (`MaterialId`/`MaterialComposition`/`MaterialPropertySet`/`ProfileRef`), `Composition/acoustic` (`AcousticBand`/`Acoustic`/`StcContourFit`), `Assessment/assessment` (`AssessmentPayload`), `Geospatial/coverage` (`CoverageGrid`), `Geospatial/reference` (`GeoReference` twelve-tuple), `Projection/projection` (`IElementProjection`/`IGraphConstraint`/`Assemble`), `Projection/address` (`CanonicalWriter`/`ContentAddress`), `Projection/fault` (`ElementFault` band 2500) — plus `README.md`, `ARCHITECTURE.md`, and `Rasm.Element.csproj` (ProjectReference `../Rasm`; PackageReference UnitsNet/QuikGraph/Riok.Mapperly/Generator.Equals/Thinktecture.Runtime.Extensions.Json/System.IO.Hashing/NodaTime); all design overrides applied.

[QUIKGRAPH_API_TIER]-[COMPLETE]: promoted `api-quikgraph.md` to the shared `libs/csharp/.api/` tier as cross-cutting substrate — the package-centric catalog folds the seam `ElementGraph` topology view, the Persistence synchronous `Query/topology` lane, and the Bim CPM/`SystemTrace`/version-DAG walks; registered the `[GRAPH_ALGORITHM]` group in `libs/csharp/.planning/README.md`, re-tiered the Bim README entry from `[02]-[DOMAIN_PACKAGES]` to `[03]-[SUBSTRATE_PACKAGES]` and removed the Bim-local copy; the `Rasm.Element/.api/` directory stays intentionally empty (every seam substrate routes to the shared tier or the core Thinktecture/LanguageExt rails).
