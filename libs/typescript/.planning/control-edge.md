# [TYPESCRIPT_CONTROL_EDGE]

One page owns the browser write edge — the three control verbs addressed by stable string keys, the command-intent deep links, and the fold of each command receipt back into availability. It is the sole owner of the write-verb law and stands apart from the composition root so the read law and the write law never share a page. The page consumes clusters 3 and 10 at the write edge and reads the command-availability fold the state-stores page owns so a disabled command never fires. It holds no domain state and dials no read stream.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]       | [OWNS]                                                |
| :-----: | :-------------- | :---------------------------------------------------- |
|   [1]   | COMMAND_GATEWAY | the single write gateway over the three control verbs |
|   [2]   | INTENT_REGISTRY | the deep-link key vocabulary the gateway resolves     |

## [2]-[COMMAND_GATEWAY]

- Owner: `CommandGateway`, the single gateway over the three control verbs.
- Cases: the gateway turns a UI intent into a unary call and folds the receipt back into the availability fold; the three verbs — capture a support bundle, set a degradation level, reload options — dial the control-service verbs on `remote-lane.md#TS_PROJECTION` and the support capture verb against `support-bundles.md#TS_PROJECTION`.
- Entry: payloads and outcomes are carried by the command wire shapes verbatim against `commands-availability.md#TS_PROJECTION`; availability is driven by the command-availability fold so disabled commands never fire; `CommandGateway` is the closed-tier gateway owner.
- Packages: the connect web transport for unary calls and the effect core for the gateway composition.
- Growth: a new control verb lands as one gateway method, never a sibling gateway.
- Boundary: a second gateway beside this one is the named defect; the gateway is the only write edge and holds no domain state — availability lives on the state-stores page and is read here as a gate.

## [3]-[INTENT_REGISTRY]

- Owner: `IntentRegistry`, the deep-link key vocabulary the gateway resolves.
- Cases: `IntentRegistry` addresses command intents by stable string keys against `commands-availability.md#TS_PROJECTION` so deep links survive a reload; a resolved intent dispatches through `CommandGateway` carrying its payload.
- Entry: intents resolve from the query-string deep-link binding the view-surfaces page owns; the key is the stable identifier, never a re-derived display string.
- Packages: the effect core for the keyed-registry composition.
- Growth: a new addressable intent lands as one registry key bound to one gateway verb.
- Boundary: the registry holds keys, never receipts or availability; receipt folding is the gateway's concern and availability is the state-stores page's.
