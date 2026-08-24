# [PY_DATA_IDEAS]

Forward pool of higher-order concepts for `data`, grounded in the host-free interchange role. Each idea is a card — slug leader with the capability, what it unlocks, and the gap or technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — concept grain only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

(none)

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[ASSESSMENT_RECORD_WIRE]-[COMPLETE]: the shape decision resolved SECOND RECORD CLASS — `impact/declaration.md` mints `DeclarationRecord` (material identity, issuer + registration, declared unit, issue/expiry, presence-censused indicator cells) as the `libs/contracts/manifest.json` `declaration-record/declaration-record` domain case with `python:data/impact/declaration#DECLARATION` its producer actor; the impact frame stays impact-only by charter and `dotnet:Rasm.Materials/Properties/assessment#ASSESSMENT_RECORD` is the committed consumer actor.
[EPD_RECORD_WIRE]-[COMPLETE]: registry sourcing landed on `impact/declaration.md`'s payload-shape axis, never as widened `MaterialImpact` rows — the Ökobaudat/ILCD arm whole through `epdx.convert_ilcd`, the EC3 arm on the axis with its validity-date member pin riding the page fence, EPD-Norge and offline bundles frozen `Registry` tokens whose arms land by the page's own growth grammar.
[DURABLE_EVIDENCE_JOURNAL]-[COMPLETE]: realized as `tabular/journal.md`, which composes the commit and scan owners rather than widening either.
[ENGINE_PROFILE_PARITY]-[COMPLETE]: every engine feeds `EngineProfile.of` its own payload and none widened the band.
[RESIDENCE_PROVISIONING]-[COMPLETE]: arming is a `ResidenceRow.layout` row the ingest plan leads with, so no composition remembers an out-of-band create.
[SUBSTRAIT_PLAN_GATE]-[COMPLETE]: both inbound plan legs admit at `_reach` through `_plan_refusal`, and the original bytes reach the executor untouched.
[ADBC_DRIVER_SET]-[COMPLETE]: the three native driver rows landed on `_DRIVER`, from which the floor roster and the instrumentation seam both derive.
[EMBEDDED_ENGINE_OBSERVABILITY]-[COMPLETE]: embedded engines carry no scrape surface, so the profiled session bracket is their whole observability.
[QUERY_BENCH_LANE]-[COMPLETE]: `QueryEngine.bench` drives the runtime bench lane per spec tag and refuses the mutation INGEST spec.
[DATA_HOOK_POINTS]-[COMPLETE]: one composition registration consumes every data hook point under one scope.
[DATASET_COST_LEDGER]-[COMPLETE]: `tabular/cost.md` harvests every receipt family into one content-keyed priced frame.
[DBAPI_SPAN_THREADING]-[COMPLETE]: `dbapi_seams()` derives its rows from `_DRIVER` under the same floor gate the dispatch reads.
[LAYER_TOPOLOGY_GRAPH_FACTS]-[COMPLETE]: data-side half landed whole as `graph/graph.md` `[03]-[TOPOLOGY]` — `LayerTopology`/`OverrideFact` decode rows and the `layer_graph` containment fold onto the one rustworkx kernel; the wire schema pin stays the .NET-owner card at `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`, and the decode model re-proves field-for-field when it lands.
[GEOARROW_NATIVE_SET]-[COMPLETE]: landed as `spatial/geospatial.md` `[04]-[NATIVE]` (`NativeIngress`/`query_postgis`), the `_NATIVE_WRITER` egress arms, and the ellipsoidal kernel rebuild replacing the pyproj.Geod scalar loop; SHAPEFILE refuted native — the rust surface publishes no shapefile IO — so it stays the pyogrio long tail, the split predicate now a folder ruling.
[ARRO3_SLIM_SET]-[COMPLETE]: the IO half landed as `tabular/interop#CARRIER` `WireCodec`/`wire_bytes`/`wire_table`; the compute half is REFUTED for the CDF hop — `arro3.compute` spells no `sort_by` and no `isin`, so the lakehouse re-import stays pyarrow — recorded at the ruling and the changefeed arm.
[VECTOR_DATA_CUBES]-[COMPLETE]: landed as `spatial/cube.md` `ZoneCube` — xvec geometry-indexed dimensions under the claim CRS prelude, the `frame` bridge onto vector claims, `FieldReceipt` egress under the `cube` tag.
[COMPRESSED_CARRIER_BAND]-[COMPLETE]: landed as the `WireCodec` transport band on `tabular/interop#CARRIER` over `arro3.io` body compression (pyarrow readers open it, probe-proven); every identity fold stays the uncompressed `arrow_bytes` by law, and the C# decode arm rides the standing handoff.
[APP_NEUTRAL_STORE_SCOPES]-[COMPLETE]: `ResourceGuard` single-writer guards landed on the lakehouse committing envelope and the tensor staged write; the planetary-computer subscription re-scoped through the `_bind_subscription` compare-and-refuse latch beside the composition-bound credential provider.
[IMPACT_PLANE_BUILDOUT]-[COMPLETE]: realized as `impact/inventory.md` + `impact/solve.md` + `impact/scenario.md` beside the landed `declaration.md`; the carrier keeps the one normalization fold, and `premise` stays FLOOR-GATED.
[GEOMETRY_FRAME_ADMISSION]-[COMPLETE]: landed as `tabular/columnar#SCAN` `admit_evidence` — subject and producer-key lead join columns on the admitted table under the `domain="query"` projection; the subject stays an opaque wire literal, so a new frame family admits with zero data edits.
[IFC_CRS_SOURCE]-[COMPLETE]: landed as `spatial/geospatial.md` `GeoreferenceFact` and the `reproject(frame, source=)` helmert prelude; the geometry-end producer stays geometry's `[IFC_GEOREFERENCE]` card, and the `[SHAPE]: GeoreferenceFact` seam edge is declared on this side.
[SCENARIO_FIELD_TREES]-[COMPLETE]: landed as `gridded/ensemble.md` `ScenarioTree`/`TreeOp`/`TreeResult` over `DataTree`, the minted `scenario` concat dimension, and `FieldReceipt` egress under the `tree` tag.
[GRAPH_NETWORK_ANALYSIS]-[COMPLETE]: landed as `graph/network.md` `FlowNetwork`/`FlowAlgorithm` over the networkx flow family, lowering through the new `GraphResult.flows` edge-keyed case onto the columnar join.
