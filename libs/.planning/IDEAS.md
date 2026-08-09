# [CROSS_LIBS_IDEAS]

Cross-language ideas span two or more branch estates through a language-neutral contract. Single-branch concepts live in that branch's register.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis — the capability outcome, never the landing motion>.
- Capability: <the higher-order invariant, owner capability, or concept established — altitude only, never a page path, row list, or member spelling>.
- Shape: <where the work lands and at what grain — repo-relative page with section/row, or a new-page path; the concrete surface, so Capability never names it>.
- Unlocks: <the downstream capability at the consumer grain — a task narrows its parent idea's Unlocks to THIS slice as `IDEAS.md [SLUG] — consequence`; a set-completion card states the completeness bar that is its acceptance contract>.
- Anchors: <owners, seams, packages, catalogs, doctrines, and techniques making the work plausible — anchors, never procedures>.
- Arms: <BLOCKED or gated cards only; the exact observable flipping it actionable — catalog row landing, member query evidence, package admitted>.
- Route: <present only on a probe, research, or member-pin card; the ordered verification path run before any fence lands>.
- Tension: <only when an unresolved constraint, boundary, or bet shapes the work — the genuine bet, never the arming condition Arms carries>.
- Ripple: <counterpart — cross-folder `pkg` `[SLUG]`, same-folder prerequisite `[SLUG]`; load-bearing build order prefixes follows/precedes/mirrors>.
Capability, Shape, Unlocks, and Anchors are required on every open card; statuses closed — `ACTIVE|QUEUED|BLOCKED` open, `COMPLETE|DROPPED` closed; IDs are SEMANTIC UPPERCASE_SNAKE slugs carrying meaning — never numeric (`[0007]`-class NNNN IDs are a defect), for cards AND research tokens alike; a hyphenated slug anywhere is a defect; repo-relative paths only. Design pages carry the terminal `[RESEARCH]` section always — `(none)` marks empty, absence is an error. Ideas state higher-order concepts, never landing-grain tasks.
-->

[PROFILE_SIGNAL_OTLP]-[BLOCKED]: Continuous profiles migrate from vendor push onto the OTLP profiles signal the moment it stabilizes.
- Capability: Profiles, the fourth signal, ride the same gateway, resource identity, and scope law as traces/metrics/logs — Pyroscope push SDKs in all three runtimes retire into OTLP exporters, and profile-to-span correlation becomes wire-native.
- Shape: One exporter-row swap per runtime composition root and one collector pipeline row per the `libs/.planning/ARCHITECTURE.md` `[PROFILE_SWAP]` table; the span-profile correlation processors and dashboards survive unchanged.
- Unlocks: Vendor-neutral profiling, one ingress for all four signals, and profile exemplar links alongside the metric-trace jumps.
- Anchors: `csharp` Pyroscope span-profile correlation; `python:runtime/observability/profiles.md`; `typescript:runtime/otel/profile.md`; `typescript:iac/operate/observe.md` Pyroscope row; the collector gateway.
- Arms: an SDK train ships a profiles provider and an OTLP profiles exporter. Python's train proves the whole signal's stage today — `opentelemetry.proto.profiles.v1development.profiles_pb2` and its collector service resolve, while `opentelemetry.sdk` carries `trace`, `metrics`, and `_logs` and no profiles module at all, and neither the HTTP nor the gRPC exporter package publishes a profile exporter beside its trace, metric, and log ones. That `v1development` package segment turning `v1` beside a landed SDK module IS the observable; the swap then executes as row replacement per the `[PROFILE_SWAP]` table.

## [02]-[CLOSED]

<!-- source-only: closed idea card template:
[GENERATION_RECOVERY_CONTRACT]-[COMPLETE]: all three branch mints grade one verdict on two proofs with no second identity axis, recovery evidence staying observation-side so canonical bytes and every peer decode are untouched. Skew closed a real divergence — a frontier stamped after its own observation computed a negative lag that passed the C# and Python gauges trivially, and a same-instant frontier refused in TypeScript alone; both rules ride `libs/.planning/RULINGS.md`.
[HOST_OPLOG_CRDT_PRODUCER]-[COMPLETE]: merge policy settled per mutation kind on one `OpLaw` triple three runtimes spell, surfacing three live defects — replay dedup was content-proven and discarded a second edit of identical bytes, the crdt lane counted genuine `set` conflicts as convergence, and `maintain` admitted a horizon its minter never observed, now `SyncFault.Unobserved`. Registration is the `OPLOG_ENTRY` entry, never a descriptor family.
[DAYLIGHTING_SCENE_DESCRIPTOR]-[COMPLETE]: both ends built against the pinned schema — `Render/settings#SUN_ASTRONOMY` mints `SceneSun` off the kernel almanac, `Objects/lights#SEED_AND_EDIT` mints `ScenePhotometry`, and `python:geometry/energy/simulate#SIMULATE` decodes into shade meshes, a point-in-time sky, and an authority-ranked roster. Probing settled that no engine on the rail reads an IES web, so it crosses by content key alone.
[LAYER_TOPOLOGY_GRAPH_FACTS]-[COMPLETE]: landed whole as the `ORGANIZATION_WIRE` entry over `rasm.organization.v1` — producer `csharp:Rasm.Rhino/Document/layers#ORGANIZATION_PROJECTION`, decoders `python:data/graph/graph#TOPOLOGY` folding onto the one rustworkx kernel and `typescript:data/read/query#ORGANIZATION_ROWS` beside `read/fold#LANE_SPEC`. `LayerTopologyFact` does not re-enter under any spelling.
[ID]-[COMPLETE|DROPPED]: <one-line disposition — a DROPPED row carries the rejection reason at ruling grain>; keep closed cards collapsed unless a second retained fact changes future routing.
-->

[TIER0_STRATA_DELAMINATION]-[COMPLETE]: Tier-0 `[01]-[STRATA]` now states the branch-agnostic stratification law and `[02]-[DEPENDENCY_DIRECTION]` the cross-branch no-direction law; the C# roster, its charters, and the S4 app-shell rank seat at `libs/csharp/.planning/ARCHITECTURE.md` `[02]-[STRATA]`, and the reviewer configs, workflow lanes, and routers resolve the branch table live.
[UNIFIED_SIGNAL_FABRIC]-[COMPLETE]: reopened when the disk audit refuted the fabric — the collector endpoint every workload bound resolved to nothing, fifteen python producer measures hit a producer-killing census lookup, and six C# pages failed compilation against their kernel owner; re-closed against the landed campaign: canonical rows at `[08]-[OBSERVABILITY_CONFORMANCE]` with the hook and evidence-residence planes, receipt-projected instruments (`InstrumentFan`/`Metrics.record`/`Pulse`), the census-gated `INSTRUMENTS` fill, and the iac backend whose endpoint derives from `_urls`.
[FLEET_TELEMETRY_SCALE_ROWS]-[COMPLETE]: escalation family named with arming coordinates at `ARCHITECTURE.md` `[FLEET_ESCALATION]`; every row stays OFF at estate scale by ruling, so fleet pressure flips a coordinate instead of re-deriving the design.
[COST_ATTRIBUTION_BAGGAGE]-[COMPLETE]: tenant join realized at `ARCHITECTURE.md` `[TENANT_COST_JOIN]` — the `rasm.tenant` dimension row-identical across runtimes after the python attribute-key repair, spend vectors projected through the instrument fan, cost boards joined at the OpenCost row and tenant organizations.
