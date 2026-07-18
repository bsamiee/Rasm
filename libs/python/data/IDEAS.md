# [PY_DATA_IDEAS]

The forward pool of higher-order concepts for `data`, grounded in the host-free interchange role. Each idea is a card — slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[LAYER_TOPOLOGY_GRAPH_FACTS]-[QUEUED]: Decoded `LayerTopologyFact` rows fold into a containment graph the graph plane analyzes for host organization.
- Capability: Wire-carried layer and relation keys decode into a `GraphPayload` whose nodes are layer identities and whose edges are layer-path nesting and membership, so topology analysis — containment ancestry, nesting depth, membership closure — answers layer organization over the decoded graph with no host handle; per-viewport overrides ride the decoded rows as detached facts.
- Shape: A boundary decoder folds the detached fact rows into the `graph/graph` plane's node-link source; `GraphPayload.analyze` runs the containment and nesting queries on the one `rustworkx` kernel keyed by the stable `NodeId` index, and the node-keyed `GraphResult.frame` left-joins layer organization onto the `tabular/columnar` scan plane by `node`.
- Unlocks: Host-organized element queries over the graph plane, layer-scoped dataset slicing, and one decoded organizational axis every interchange consumer reads by name.
- Anchors: `graph/graph.md` `GraphPayload`/`analyze`/`NodeId`/`GraphResult.frame`; `rasm.runtime.identity` `ContentIdentity`/`ContentKey` for the stable wire identity; `README.md` host-free interchange role meeting C# only at the content-identity wire.
- Tension: Wire schema and codec mint in C#; this plane decodes and never re-mints, and the containment graph carries only detached fact rows, never a host layer handle.
- Ripple: `libs/.planning` `[LAYER_TOPOLOGY_GRAPH_FACTS]`.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
