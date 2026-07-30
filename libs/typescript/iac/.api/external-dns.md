# [TS_IAC_API_EXTERNAL_DNS]

`external-dns` is the edge tier's record reconciler: one controller reads a declared SOURCE set out of the cluster and writes the matching records at the DNS provider. Every governing knob is a top-level values key — the source list, the provider, the ownership registry, and the domain filter — and the ownership registry is what makes the controller's writes reversible rather than merely additive.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `external-dns`
- chart: `external-dns` from `https://kubernetes-sigs.github.io/external-dns/` (Apache-2.0), chart and `appVersion` versioned independently
- asset: the controller Deployment, a Service, a ServiceAccount, its RBAC cell, and an off-by-default ServiceMonitor
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / edge DNS
- crds: NONE

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                                          | [CAPABILITY]                                                                      |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `sources`                                      | `string[]` — which cluster objects mint records                                   |
|  [02]   | `provider`                                     | the DNS backend — `{ name, webhook }` or a bare `string`                          |
|  [03]   | `domainFilters` `excludeDomains`               | `string[]` — the zones the controller may and may not touch                       |
|  [04]   | `policy`                                       | DEFAULT `upsert-only` — the controller never DELETES a record                     |
|  [05]   | `registry`                                     | the ownership store; `txt` writes a companion TXT record per managed name         |
|  [06]   | `txtOwnerId` `txtPrefix` `txtSuffix`           | `string` — the ownership identity two controllers on one zone must not share      |
|  [07]   | `env`                                          | provider credentials as `secretKeyRef` environment                                |
|  [08]   | `interval` `triggerLoopOnEvent`                | `1m` / `boolean` — the reconcile cadence and its event trigger                    |
|  [09]   | `sourceNamespace` `namespaced`                 | `string` / `boolean` — narrow the watched namespace and drop to a namespaced Role |
|  [10]   | `gatewayNamespace` `enableGatewayListenerSets` | Gateway API scoping                                                               |
|  [11]   | `secretConfiguration` `extraArgs`              | file-mounted provider config and raw flags                                        |
|  [12]   | `logLevel` `logFormat`                         | enums — `panic \| debug \| info \| warning \| error \| fatal`, `text \| json`     |
|  [13]   | `{name,fullname,namespace}Override`            | nullable; standard collapse scaffold                                              |

[ADMISSION_FILTERS]: `managedRecordTypes` `labelFilter` `annotationFilter` `annotationPrefix`
[POLICY_LADDER]: `upsert-only` creates and updates and never deletes — the safe default, and the reason a removed HTTPRoute leaves a stale record. `sync` deletes what the sources no longer declare, which is the only policy under which the cluster is the record's source of truth. `create-only` writes once and never touches an existing record.
[REGISTRY_LAW]: the `txt` registry writes a companion TXT record carrying `txtOwnerId`, and the controller edits ONLY records that TXT claims. Two controllers over one zone with the same owner id fight; with different ids they ignore each other's records — so the owner id is the isolation boundary, not the domain filter.
[CREDENTIAL_FORM]: provider credentials ride `env` as a `secretKeyRef`, never as a values literal, because chart values render into cluster state and every stack export.

[FULLNAME]: the standard collapse scaffold with flat `nameOverride`/`fullnameOverride`, both nullable — the pin reaches the Deployment, the Service, the ServiceAccount, and the RBAC objects.
[SERVICE_NAME]: `<fullname>` UNSUFFIXED, serving the controller's metrics door. Nothing in this estate dials it; the Service exists for the ServiceMonitor.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The record set is DERIVED, never declared twice — the edge tier states hostnames on its Gateway API objects and this controller reflects them into the zone, so a hostname lives at one site and the zone converges to it.
- The provider credential is the same in-graph token the Cloudflare provider itself holds: one read, two consumers, and the cluster half reaches it as environment sourced from a Secret this tier mints.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the controller; `core.v1.Secret` carries the provider token the `env` row binds by reference.
- `@pulumi/cloudflare`(`.api/pulumi-cloudflare.md`): the same API token drives the in-graph provider for tunnel, ruleset, and zone resources, so the deploy-time and reconcile-time halves of the zone speak one credential.
- `kube/traffic#EDGE_TIER`: the owner installing this chart with `sources: ["gateway-httproute"]`, `provider.name: "cloudflare"`, the token as a `secretKeyRef` env row, and `domainFilters` pinned to the stack's own domain.
- Gateway API objects (`kube/traffic`): the HTTPRoute hostnames ARE the source set, which is why the source list carries `gateway-httproute` alone and no Ingress or Service source is armed.

[LOCAL_ADMISSION]:
- Declare `sources` explicitly and narrowly; every armed source is another object family that can mint a record.
- Bound the blast radius with `domainFilters` on the stack's own domain, and set a distinct `txtOwnerId` per controller sharing a zone — the filter narrows what is considered, the owner id decides what may be edited.
- State the `policy` deliberately: the `upsert-only` default leaves stale records behind on every removal, and `sync` is what makes the cluster authoritative.
- Bind the provider credential through `env` with `valueFrom.secretKeyRef`; a token in a values literal is plaintext in the rendered ConfigMap and in every stack export.
- Leave the metrics Service unpublished; it serves the ServiceMonitor and no consumer.

[RAIL_LAW]:
- Contract: `external-dns` chart values
- Owns: DNS record reconciliation — the source set, the provider binding, the ownership registry, the domain filters, and the reconcile cadence
- Accept: `sources: ["gateway-httproute"]` as the one source; `provider.name` naming the backend; `domainFilters` on the stack domain; a `secretKeyRef` credential in `env`; a distinct `txtOwnerId` per controller over a shared zone; a deliberately stated `policy`
- Reject: a provider token as a values literal; an unfiltered domain scope; a shared `txtOwnerId` across controllers; `upsert-only` where removals must converge; a second source family armed without a record owner for it
