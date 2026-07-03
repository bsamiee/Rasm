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

[PERSISTENCE_GRAPH_REBUILD]-[QUEUED]: full `Rasm.Persistence` rebuild over the `ElementGraph` + `GraphDelta` event body.
- Capability: `Element/` graph store-load roundtrip; `Version/` the Marten event SoR (the `GraphDelta` is the event body; inline `SingleStreamProjection` folds `GraphDelta`→`ElementGraph`; the periodic Marten snapshot `Snapshot<T>(SnapshotLifecycle.Inline)` bounds replay) beneath the bespoke op-log/CRDT/time-travel/`StructuralMerge`/causal-DAG engine; `Query/` the in-process `QuikGraph` topology + DuckDB columnar (AGE optional); the content-keyed object store for geometry/coverage blobs (write-blob-first).
- Anchors: the `Graph/delta#GRAPH_DELTA` `ReplayOnto` fold, the `Projection/address#CONTENT_ADDRESS` content key, the `Generator.Equals` `Inequalities` 3-way merge feed, Marten 9.11.
- Ripple: `csharp:Rasm.Persistence` `[ELEMENT_GRAPH_PERSISTENCE]`.

[COMPUTE_ASSESSMENT_RUNNERS]-[QUEUED]: `Rasm.Compute` owns the discipline-specific assessment I/O, runners, and the relocated `AssemblyAggregator`.
- Capability: Compute reads the concrete `ElementGraph` directly (above the seam, no interface), owns the structural/thermal/energy/acoustic/fire/LCA solver input-result shapes and runners (VividOrange/BriefFiniteElement/FEALiTE2D + NREL.OpenStudio + closed-form + EC3 REST), the multi-ply `AssemblyAggregator` (series-resistance U-value, rule-of-mixtures density, ISO 12354 layered STC through the seam `StcContourFit`), and writes `Assessment` result nodes back content-keyed on `(InputKey, Route)`.
- Anchors: the `Assessment/assessment#ASSESSMENT_NODE` generic receipt, the `Composition/acoustic#ACOUSTIC_FOLDS` shared kernel, the `Composition/material#MATERIAL_COMPOSITION` plies.
- Ripple: `csharp:Rasm.Compute` `[ELEMENT_ASSESSMENT_RUNNERS]`.

[PY_WIRE_ALIGNMENT]-[QUEUED]: align the Python companion to the seam content-key and typed wire vocabulary.
- Capability: the `python:geometry/ifc-companion` IFC→IfcOpenShell→GLB decoder and the interchange decoders align to the seam `RepresentationContentHash` content-key (one `XxHash128` seed) and the typed `Material`/`Property`/`Assessment`/`Classification` wire vocabulary — decode by content key, never re-mint.
- Anchors: the `Projection/address#CONTENT_ADDRESS` seed-zero rail, the `Graph/wire#WIRE_CODEC` `rasm.element.v1` contract (the concrete proto + counted-bag canonical layout the mirror builds against), the wire-alignment-only architecture clause (no Materials/Bim/Persistence reference).
- Ripple: `python:geometry/ifc` `[SEAM_WIRE_ALIGNMENT]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[WIRE_CODEC_PAGE]-[COMPLETE]: authored `.planning/Graph/wire.md` (the `rasm.element.v1` proto contract + `WireCodec` Mapperly family + `ElementWire` railed boundary) and landed the routing — README router row `[04]-[WIRE]` + package cards, ARCHITECTURE codemap `Wire.cs`/`element.proto` + the precise `Graph/wire` peer seam rows replacing the `*` wildcard, csproj `Google.Protobuf`/`Grpc.Tools`/`NodaTime.Serialization.Protobuf` admissions, and the shared-registry folder attributions; the `<Protobuf>` codegen item stays signature-locked in the page until the `.proto` lands as source.
[SEAM_CORPUS_BUILD]-[COMPLETE]: authored the full `Rasm.Element` design corpus — `Graph/element` (NodeId/Node `[Union]`/Header/frozen `ElementGraph`/incidence index/memoized `Bake`), `Graph/delta` (`GraphMutation`/HAMT `WorkingGraph`/structural edge law/`GraphDelta` event body/`ReplayOnto`), `Relations/relation` (neutral `Relationship` algebra + `MaterialUsage`), `Classification/classification` (`Classification`/`Discipline`), `Properties/property` (`PropertyValue`/`PropertyName`/`PropertyBag`/`QuantityBag`/`InheritanceMode`), `Properties/quantity` (`Dimension`/`MeasureValue`), `Composition/material` (`MaterialId`/`MaterialComposition`/`MaterialPropertySet`/`ProfileRef`), `Composition/acoustic` (`AcousticBand`/`Acoustic`/`StcContourFit`), `Assessment/assessment` (`AssessmentPayload`), `Geospatial/coverage` (`CoverageGrid`), `Geospatial/reference` (`GeoReference` twelve-tuple), `Projection/projection` (`IElementProjection`/`IGraphConstraint`/`Assemble`), `Projection/address` (`CanonicalWriter`/`ContentAddress`), `Projection/fault` (`ElementFault` band 2500) — plus `README.md`, `ARCHITECTURE.md`, and `Rasm.Element.csproj` (ProjectReference `../Rasm`; PackageReference UnitsNet/QuikGraph/Riok.Mapperly/Generator.Equals/Thinktecture.Runtime.Extensions.Json/System.IO.Hashing/NodaTime); all design overrides applied.
[QUIKGRAPH_API_TIER]-[COMPLETE]: promoted `api-quikgraph.md` to the shared `libs/csharp/.api/` tier as cross-cutting substrate — the package-centric catalog folds the seam `ElementGraph` topology view, the Persistence synchronous `Query/topology` lane, and the Bim CPM/`SystemTrace`/version-DAG walks; registered the `[GRAPH_ALGORITHM]` group in `libs/csharp/.planning/README.md`, re-tiered the Bim README entry from `[02]-[DOMAIN_PACKAGES]` to `[03]-[SUBSTRATE_PACKAGES]` and removed the Bim-local copy; the `Rasm.Element/.api/` directory stays intentionally empty (every seam substrate routes to the shared tier or the core Thinktecture/LanguageExt rails).
[STRATA_AMENDMENT]-[COMPLETE]: `Rasm.Element` registered as the lowest-AEC sub-stratum — `libs/.planning/architecture.md` `[01]`/`[02]` carry the strata slot plus the `{Rasm, Rasm.Element}` upward-peer rule, `libs/.planning/planning-targets.md` lists the package, and `libs/csharp/.planning/ARCHITECTURE.md` records the seam node, the peer/consumer edges, and the python/typescript wire rows.
[BIM_PROJECTOR_REBUILD]-[COMPLETE]: `Rasm.Bim/.planning/Projection/semantic.md` realizes `SemanticProjector : IElementProjection` (`Project` lowers `DatabaseIfc`→`GraphDelta`, `Emit` Bim-internal) plus `IfcLegality : IGraphConstraint`; `Model/elements.md` retires `BimElement`/`BimModel` onto the seam `Bake(objectNode)` fold, `Rasm.Bim` now the seam's sole IFC projector.
[MATERIALS_PROJECTOR_REBUILD]-[COMPLETE]: `Rasm.Materials/.planning/Projection/component.md` realizes `ComponentProjector : IElementProjection` — the `MaterialProjector` + `ConnectionProjector` merge SUPERSEDING this card's narrower `MaterialProjector` charter — routing the material/property unions into the seam `Material`/`MaterialPropertySet` and resolving `ProfileSet`→`ProfileRef` one-hop.
[PEER_SEAM_REFERENCES]-[COMPLETE]: every AEC-peer and consumer csproj carries `<ProjectReference Include="../Rasm.Element/Rasm.Element.csproj"/>` — `Rasm.Materials`, `Rasm.Bim`, `Rasm.Fabrication`, `Rasm.Compute`, `Rasm.Persistence` all verified present; `Rasm.AppHost` stays reference-light.
