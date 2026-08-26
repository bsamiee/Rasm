# [TS_IAC_API_GATEWAY_API]

`gateway-api` is the routing contract the deploy plane's network edge is written against, and it is the one CRD set no chart in this folder installs: the cluster's Gateway controller owns the definitions, the cluster authors only the custom resources. That split is the contract — no field here configures a controller, and no controller value changes what a `Gateway` or `HTTPRoute` may hold.

Catalogue placement follows the folder rule that a CRD set arriving with no chart of its own earns a folder-tier catalogue: no chart surface exists to hang a stacking bullet on, leaving the fences that assert these members verifying against nothing.

## [01]-[GATEWAY_CONTRACT]

[CLASS_CONTRACT]: `GatewayClass` is CLUSTER-scoped and this cluster REFERENCES one by name without ever authoring it — the controller installs its own class, so `spec.controllerName` (`string` 1..253, REQUIRED, domain-slash-path pattern) is read-only cluster context. `description` is `string` maxLength 64 and `parametersRef` is the implementation-specific handle.

| [INDEX] | [FIELD]                              | [CAPABILITY]                                                             |
| :-----: | :----------------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `spec.gatewayClassName`              | `string` 1..253 REQUIRED — a name no class answers leaves it unaccepted  |
|  [02]   | `spec.listeners[]`                   | `array` minItems 1, maxItems 64, REQUIRED — the whole listen surface     |
|  [03]   | `…listeners[].name`                  | `string` 1..253 REQUIRED — the section key a route's `sectionName` binds |
|  [04]   | `…listeners[].port`                  | `integer` 1..65535 REQUIRED                                              |
|  [05]   | `…listeners[].protocol`              | `string` 1..255 REQUIRED — PATTERN-bound, never an enum                  |
|  [06]   | `…listeners[].hostname`              | `string` 1..253 — one leading `*.` admits; absent matches every hostname |
|  [07]   | `…listeners[].tls.mode`              | `string` enum `Terminate \| Passthrough`, DEFAULT `Terminate`            |
|  [08]   | `…listeners[].tls.certificateRefs[]` | `array` maxItems 64 — the sink material; `Passthrough` admits none       |
|  [09]   | `…certificateRefs[].name`            | `string` 1..253 REQUIRED — the ref's only required member                |
|  [10]   | `…certificateRefs[].{group,kind}`    | `kind` DEFAULT `Secret`, `group` DEFAULT `""`                            |
|  [11]   | `spec.*` remainder                   | request addresses, per-Gateway metadata, backend TLS, listener admission |

[GATEWAY_REMAINDER]: `addresses` (maxItems 16), `infrastructure`, `tls`, `allowedListeners` — the surface beyond the rows above.
[LISTENER_KEY]: listeners key on `name`, and `name`/`port`/`protocol` is the only required set, so a listener is spellable with no TLS block at all — an HTTPS listener carrying no `certificateRefs` is schema-valid and fails at the controller instead. That gap is what `operate/policy.md`'s `gateway-tls-required` row closes, walking listener sets for an `HTTPS` member whose `certificateRefs` is non-empty.
[REF_NAMESPACE]: `certificateRefs[].namespace` (`string` 1..63) crosses namespaces only under a `ReferenceGrant` in the target namespace; a cross-namespace ref without one resolves to a refused listener, never a silent fallback.

## [02]-[ROUTE_CONTRACT]

[ROUTE_CONTRACT]: `HTTPRoute` is NAMESPACED and its `spec` requires nothing — every field defaults, so an empty spec is admissible and inert. That is the trap the rows below exist against: absence is a decision the schema makes, not one the author made.

| [INDEX] | [FIELD]                            | [CAPABILITY]                                                              |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------ |
|  [01]   | `spec.parentRefs[]`                | `array` maxItems 32 — the Gateways this route attaches to                 |
|  [02]   | `…parentRefs[].name`               | `string` 1..253 REQUIRED; `kind` DEFAULT `Gateway`, `group` the API group |
|  [03]   | `…parentRefs[].{sectionName,port}` | narrows to one listener or port; absent attaches to all compatible        |
|  [04]   | `spec.hostnames[]`                 | `array` maxItems 16 of `string` 1..253 — intersected with the listener's  |
|  [05]   | `spec.rules[]`                     | `array` minItems 1, maxItems 16 — DEFAULT one PathPrefix `/` rule         |
|  [06]   | `…rules[].matches[]`               | `array` maxItems 64, DEFAULT PathPrefix `/` — omitted matches every path  |
|  [07]   | `…rules[].backendRefs[]`           | `array` maxItems 16 — the weighted backend set                            |
|  [08]   | `…backendRefs[].name`              | `string` 1..253 REQUIRED — the entry's only required member               |
|  [09]   | `…backendRefs[].port`              | `integer` 1..65535 — required in practice for a `Service` backend         |
|  [10]   | `…backendRefs[].weight`            | `integer` 0..1000000, DEFAULT 1 — see `[WEIGHT_ALGEBRA]`                  |
|  [11]   | `…backendRefs[].{group,kind}`      | `kind` DEFAULT `Service`, `group` DEFAULT `""`                            |
|  [12]   | `…rules[].{name,filters,timeouts}` | rule identity, per-rule filter chain, request and backend deadlines       |

[WEIGHT_ALGEBRA]: `weight` is a PROPORTION, never a percentage — each backend receives `weight / (sum of weights in this backendRefs list)`, and the sum need not equal 100. One backend alone carrying weight above 0 takes everything regardless of the number. Weight `0` forwards nothing to that entry while leaving it resolved and reported. Omitting `weight` is weight `1`, so a two-backend rule written without weights is an even split — which makes the field's absence a routing decision rather than a default worth resting on. Implementations may deviate by an epsilon from the exact proportion.
[INVALID_BACKEND]: an invalid backendRef sheds nothing onto its siblings — the proportion routed to it answers `500` instead, and when every entry is invalid with no filters on the rule, all matching traffic answers `500`. Weighted splits become safe only once the referenced Service exists: writing a candidate weight ahead of the candidate Service turns that share of live traffic into errors rather than routing it to the incumbent.

## [03]-[IMPLEMENTATION_LAW]

- Controller installs the CRDs; this cluster authors the CRs. Every Gateway API object the branch declares is a committed `crd2pulumi` class, so the routing vocabulary is compile-checked at the one boundary the public edge runs through, and a raw untyped `CustomResource` has no spelling.
- Generated CRD module and the channel pin move together: a Gateway API bump regenerates `../crds/gateway` rather than shifting an npm dependency, so the cluster schema and the typed classes never disagree.
- Standard channel bounds the pin — a member outside it carries no generated class and no catalogue row, so reaching for one is a channel decision made at the pin, never at a call site.

[STACKING]:
- `../crds/gateway` (crd2pulumi): `v1.Gateway` and `v1.HTTPRoute` are the branch's compile-checked spelling of this CRD set, regenerated against the pinned channel rather than pinned as an npm dependency.
- `kube/traffic#EDGE_FAMILY`: `_EDGES.gateway` carries the class name and controller namespace beside the legacy `Ingress` row.
- `kube/traffic#EDGE_REALIZE`: authors the `Gateway` HTTPS listener over a `kubernetes.io/tls` sink and the `HTTPRoute` binding hostname to one backend.
- `operate/policy#POLICY_ROWS`: `gateway-tls-required` walks `Gateway` CRs by `kind` and an `apiVersion` prefixed `gateway.networking.k8s.io`, judging listener sets for an `HTTPS` member with non-empty `certificateRefs` — the CR's own props are the carrier's stable discriminant.
- `external-dns` (`.api/external-dns.md`): `gateway-httproute` reads `HTTPRoute` hostnames as its record set, which is why that source stands alone with no Ingress or Service source armed.

[LOCAL_ADMISSION]:
- Order the candidate Service ahead of any weighted route naming it — an invalid backendRef answers `500` across its whole share instead of falling back, so a weight written early is an outage proportional to the number.
- State `weight` on EVERY entry of a multi-backend rule; an omitted weight is `1`, so one stated weight beside one omitted one is a split nobody intended.
- Collapse the route to a single backendRef once a cutover lands: a lone backend takes all traffic whatever its weight, so a stale weight row is dead state the next diff still carries.
- State `tls.mode` even though `Terminate` is the default, and pair it with a non-empty `certificateRefs` — the schema admits an HTTPS listener with neither and defers the failure to the controller.
- Bind `parentRefs` by `name` alone where one listener serves, and reach for `sectionName` the moment a Gateway carries more than one; attachment to every compatible listener is the default and it widens as listeners are added.
- Let `matches` default only where matching every path is the intent — that default is a PathPrefix `/` rule, so an omitted match block is a catch-all rather than an inert route.
