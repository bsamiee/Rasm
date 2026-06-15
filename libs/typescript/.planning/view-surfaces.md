# [TYPESCRIPT_VIEW_SURFACES]

One page owns leaf rendering and read-only observation — the view edge that subscribes to stores through the one sanctioned atom binding, and the dashboards specialization that consumes the heavier receipt and evidence clusters and reads the telemetry collector. The view holds no domain state of its own; every domain read flows through a store and the atom binding, the only sanctioned React state binding under the collapse-scan law. The page collapses view and dashboards because both are thin subscribers over the state-stores page sharing one state-binding axis; their retained clusters own disjoint decision sets — interaction primitives versus observation routes. It owns no wire cluster directly and consumes clusters 9 and 11 only through stores.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                   | [OWNS]                                                       |
| :-----: | :-------------------------- | :----------------------------------------------------------- |
|   [1]   | ATOM_BINDING_AND_PRIMITIVES | the sanctioned state binding and the interaction primitives  |
|   [2]   | OBSERVATION_ROUTES          | the read-only evidence, benchmark, geo, and collector panels |

## [2]-[ATOM_BINDING_AND_PRIMITIVES]

- Owner: `AtomBinding`, the single sanctioned React state binding, plus the accessible interaction primitives, the headless table machinery, and the virtualization machinery composed under it; `DeepLinkBinding` owns URL-resident state in the query string as a sibling binding.
- Cases: components subscribe at the leaf, not the root; all domain state lives in the owning store and reaches the component only through `AtomBinding`; local component state holding domain data is the named defect the collapse-scan law deletes; `DeepLinkBinding` resolves route-resident state through the query string and command intents resolve from stable string keys so deep links survive, never duplicated into component state.
- Entry: the view renders document, health, progress, conflict, presence, and command surfaces by reading stores through the atom hooks; accessible primitives, virtualized tables, command-menu and drawer surfaces compose under that binding.
- Packages: the React line and its DOM renderer, the React atom binding, the accessible-component and primitive sets, the headless table and virtualization libraries, the icon and styling-merge utilities, and the query-string state library.
- Growth: a new leaf surface lands as one subscriber component; a new interaction primitive composes under the existing binding; a new deep-link surface lands as one query-string-bound key.
- Boundary: a second state binding beside the atom layer is the named defect; the view emits intents only through the command gateway the control-edge page owns and never dials a transport directly; URL-resident state is never duplicated into local component state.

## [3]-[OBSERVATION_ROUTES]

- Owner: the read-only observation routes — `EvidenceTimelineRoute`, `BenchmarkRoute`, `GeoSeriesSurface`, and `CollectorPanel`.
- Cases: `EvidenceTimelineRoute` carries receipt envelopes whole and renders them in HLC order with skew bands from the evidence fold; `BenchmarkRoute` renders only when fingerprint-gated by the host-fingerprint shape on `receipts-and-benchmarks.md#TS_PROJECTION`, so an unverifiable claim never displays as verified; `GeoSeriesSurface` decodes embedded geometry through the geometry rail the wire-contracts page owns; `CollectorPanel` reads the telemetry collector.
- Entry: dashboards sit on the evidence fold and the receipt store and read, never emit; the read-versus-emit split is explicit — dashboards read the collector while instrumentation belongs to the host.
- Packages: the React line, the virtualization library, and the opentelemetry surface used strictly as a collector reader.
- Growth: a new observation route lands as one route module over an existing store; a new telemetry panel reads the same collector.
- Boundary: a benchmark claim displayed without the fingerprint gate is the named defect; `CollectorPanel` crosses no wire contract of its own and references no telemetry wire type.
