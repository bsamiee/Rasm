# [TS_IAC_API_PULUMI_CLOUDFLARE]

`@pulumi/cloudflare` is the Terraform-bridged Cloudflare provider — the prepared `cloudflare` dispatch row and the cert/dns/ingress source for the `selfhosted-k8s` arm. It is a wide surface (266 resource classes, ~197 `getX`/`getXOutput` data-source pairs) governed by ONE Pulumi resource ABI: every class is `new X(name, XArgs, opts?)` with `static get`/`isInstance` and `XArgs`/`XState` interfaces, so the catalog documents the ABI as the mechanism and the 266 classes are its row-space — a resource is a row on the pattern, never a bespoke shape. The design seeds four families onto that pattern: DNS (`DnsRecord`/`Zone`/`ZoneDnssec`/`CustomHostname`), edge compute + object store (`WorkersScript`/`WorkersRoute`/`WorkersKvNamespace`/`R2Bucket`/`PagesProject`/`Queue`/`D1Database`), Zero-Trust ingress (`ZeroTrustTunnelCloudflared`/`…Config`/`…Route` + `ZeroTrustAccessApplication`/`…Policy`/`…Group`), and traffic + cert (`LoadBalancer`/`Ruleset`/`OriginCaCertificate`/`CertificatePack`/`TotalTls`). The provider takes an `apiToken`; the bridge carries deprecated pre-v6 aliases (`Record`, `WorkerScript`, `Access*`, `Tunnel*`) whose current spelling is load-bearing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/cloudflare`
- package: `@pulumi/cloudflare`
- version: `6.17.0`
- license: Apache-2.0
- import: `@pulumi/cloudflare` (flat resource + `getX`/`getXOutput` barrel; `{ config, types }` namespaces)
- owner: `iac`
- rail: cloud-row / cloudflare + selfhosted-k8s cert/dns/ingress
- runtime: a Cloudflare API credential context; no local daemon or CLI
- build-floor: `@pulumi/pulumi` `^3.142.0` (catalog pins `3.250.0`)
- depends-on: `@pulumi/pulumi` (`CustomResource`/`Input`/`Output`); Terraform-bridged, so args/outputs are provider-schema-shaped
- namespaces: root barrel (266 resources + ~197 data-source pairs), `cloudflare.config` (ambient provider vars mirroring `ProviderArgs`), `cloudflare.types.input`/`cloudflare.types.output` (per-resource nested arg/state shapes)
- capability: DNS/zones, Workers + KV + R2 + Pages + Queues + D1 + Hyperdrive edge platform, Zero-Trust tunnels + Access, load balancing + rulesets + certificates, and the full bridged Cloudflare resource surface
- abi-note: every resource output prop mirrors its `Args` field resolved through `Output<T>`; `XState` (the `.get` shape) is the all-optional Output mirror of `XArgs`; deprecated aliases carry an `@deprecated` JSDoc pointing at the current class

## [02]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized bridged-resource shape all 266 classes instantiate
- rail: cloudflare
- One shape owns the entire surface; the roster below is SEED DATA of the pattern, never the mechanism. Construction is `new X(name, XArgs, opts?)`; `opts` is the universal `pulumi.CustomResourceOptions` seam (`provider`/`dependsOn`/`parent`/`protect`/`ignoreChanges`/`import`, `.api/pulumi-pulumi.md`). Adoption of an API-existing object is `static get(name, id, XState?, opts?)`. Nested arg shapes live under `types.input.*`; every output is an `Output<T>` mirror.

| [INDEX] | [MEMBER] | [SHAPE / BOUNDARY] |
| :-----: | :------- | :----------------- |
|  [01]   | `new X(name, XArgs, opts?)` | construct any resource; `XArgs` fields are `Input<T>` |
|  [02]   | `X.get(name, id, XState?, opts?)` | adopt an existing Cloudflare object by id |
|  [03]   | `X.isInstance(obj)` | multi-SDK-safe guard `obj is X` |
|  [04]   | `getX(args, InvokeOptions?)` / `getXOutput(args, InvokeOutputOptions?)` | data-source read: eager `Promise<GetXResult>` / graph-threaded `Output<GetXResult>` |

## [03]-[RESOURCE_FAMILIES]

[DNS_SCOPE]: zones + records — the cert/dns rows
- rail: cloudflare
- The DNS surface the `kube/traffic` cert/dns rows and every cloud row need. `Zone` roots a domain; `DnsRecord` is the record (A/AAAA/CNAME/TXT via its `type`/`content` args); `CustomHostname` fronts a SaaS domain.

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `Zone` | the domain zone (`account`, `name`, `type`) |
|  [02]   | `DnsRecord` | a DNS record (`zoneId`, `type`, `name`, `content`, `proxied`, `ttl`) |
|  [03]   | `ZoneDnssec` / `ZoneDnsSettings` / `ZoneSetting` | DNSSEC, zone DNS settings, per-setting overrides |
|  [04]   | `CustomHostname` / `CustomHostnameFallbackOrigin` | SaaS custom-hostname + fallback |

[EDGE_SCOPE]: Workers, KV, R2, Pages — edge compute + object store
- rail: cloudflare
- The service-equivalence compute/object rows: `R2Bucket` is the object-store equivalent (the MinIO/Garage/S3 counterpart in the equivalence map); `WorkersScript` + `WorkersRoute` are edge compute; `PagesProject` is static hosting.

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `R2Bucket` (+ `R2BucketLifecycle`/`R2BucketCors`/`R2BucketEventNotification`/`R2CustomDomain`) | object store + policies |
|  [02]   | `WorkersScript` / `WorkersRoute` / `WorkersCustomDomain` | edge compute + routing |
|  [03]   | `WorkersKvNamespace` / `WorkersKv` | edge KV namespace + entries |
|  [04]   | `PagesProject` / `PagesDomain` | static hosting + domain |
|  [05]   | `Queue` / `D1Database` / `HyperdriveConfig` | edge queue / SQLite / DB-accelerator |

[INGRESS_SCOPE]: Zero-Trust tunnel + Access — the selfhosted-k8s ingress rows
- rail: selfhosted-k8s
- The ingress path for the selfhosted cluster with no public IP: a `ZeroTrustTunnelCloudflared` (cloudflared tunnel) with `…Config`/`…Route`, fronted by `ZeroTrustAccess*` authentication. All current-spelling (v6); the pre-v6 `Tunnel`/`Access*` aliases are deprecated.

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `ZeroTrustTunnelCloudflared` | the named tunnel (cloudflared) into the cluster |
|  [02]   | `ZeroTrustTunnelCloudflaredConfig` / `…Route` / `…VirtualNetwork` | tunnel ingress rules, network routes, virtual network |
|  [03]   | `ZeroTrustAccessApplication` / `…Policy` / `…Group` | Access app + authorization policy + identity group |
|  [04]   | `ZeroTrustAccessServiceToken` / `…IdentityProvider` | service-to-service token, IdP binding |

[TRAFFIC_SCOPE]: load balancing, rulesets, certificates
- rail: cloudflare
- Traffic steering, WAF/transform rules, and cert material. `Ruleset` is the unified rules engine (WAF, transform, redirect via its `phase`/`rules`); `OriginCaCertificate`/`CertificatePack`/`TotalTls` are the cert rows.

| [INDEX] | [SYMBOL] | [ROLE] |
| :-----: | :------- | :----- |
|  [01]   | `LoadBalancer` / `LoadBalancerPool` / `LoadBalancerMonitor` | steering + origin pool + health monitor |
|  [02]   | `Ruleset` | unified rules engine (`phase`, `rules`) — WAF/transform/redirect |
|  [03]   | `OriginCaCertificate` / `CertificatePack` / `TotalTls` | origin CA cert, edge cert pack, automatic TLS |
|  [04]   | `SpectrumApplication` / `PageRule` | L4 app proxy / legacy page rule |

## [04]-[PROVIDER]

[PROVIDER_SCOPE]: the API credential — the cloudflare arm seam
- rail: cloudflare
- The dispatch arm constructs ONE `Provider` and threads it via `opts.provider`. `apiToken` (scoped) is the canonical credential; the `apiKey` + `email` pair is the legacy account-key auth. `config.*` mirrors these as ambient vars.

| [INDEX] | [FIELD] | [TYPE] | [MEANING] |
| :-----: | :------ | :----- | :-------- |
|  [01]   | `apiToken` | `Input<string>` | scoped API token (canonical); bind a `doppler` secret Output |
|  [02]   | `apiKey` / `email` | `Input<string>` | legacy global-key auth pair |
|  [03]   | `apiUserServiceKey` / `baseUrl` | `Input<string>` | user service key, API base override |

## [05]-[IMPLEMENTATION_LAW]

[CLOUDFLARE_TOPOLOGY]:
- pattern law: a resource is `new X(name, XArgs, opts?)` on the one ABI; the four seeded families are the design's row-space, and any of the 266 classes is reachable by the same shape — never enumerate the roster, extend it as rows.
- spelling law: author the v6 current class, never the deprecated alias — `DnsRecord` not `Record`, `WorkersScript` not `WorkerScript`, `ZeroTrustAccessApplication` not `AccessApplication`, `ZeroTrustTunnelCloudflared`/`…Config`/`…Route` not `Tunnel`/`TunnelConfig`/`TunnelRoute`; the `@deprecated` JSDoc names the replacement.
- arm law: the `cloudflare` `Match.exhaustive` arm (`provider/dispatch`, `libs/typescript/.api/effect.md`) constructs one `Provider` from a `Schema`-decoded `StackSpec` `apiToken` ref and threads it via `opts.provider`; per-resource providers are rejected.
- ingress law: the selfhosted-k8s ingress is `ZeroTrustTunnelCloudflared` + `…Config` (routing to the in-cluster service) fronted by `ZeroTrustAccessApplication`/`…Policy`; a `DnsRecord` CNAMEs the hostname to the tunnel — the cluster needs no public IP.
- read law: an existing zone/record/bucket is a `getXOutput` when it feeds an `Input`, `getX` for an eager `async` read; never re-derive Cloudflare state out-of-band.

[LOCAL_ADMISSION]:
- `Provider.apiToken` binds a `@pulumiverse/doppler` secret Output (`.api/pulumiverse-doppler.md`), never a literal; `WorkersScript` secret bindings and `R2Bucket` access keys route through the same secret owner.
- the cert/dns rows meet `kube/traffic`: a `DnsRecord`/`CustomHostname` + `OriginCaCertificate` pairs with the in-cluster ingress; `@pulumi/tls` (`.api/pulumi-tls.md`) supplies origin key material where Cloudflare does not mint it.
- as a prepared cloud row, `R2Bucket` is the object-store equivalent and `WorkersScript`/`PagesProject` the compute/hosting equivalents in the service-equivalence map (`provider/surface`); an app finalizes the row with a `StackSpec` VALUE (zone, token ref), never a lib edit.
- the arm folds resource failures into the `program/automation` typed run receipt (`@pulumi/pulumi` `automation.UpResult`, `.api/pulumi-pulumi.md`); `effect` owns arm dispatch (`Match`) and the StackSpec/StackOutputs `Schema`; typed StackOutputs (zone id, tunnel id, record fqdn) project through `stack/output`.
- canonical law: `getXOutput` for graph-threaded reads; the v6 current class over every deprecated alias; one shared `Provider` per arm.

[RAIL_LAW]:
- Package: `@pulumi/cloudflare`
- Owns: DNS/zones, Workers/KV/R2/Pages/Queues/D1/Hyperdrive edge platform, Zero-Trust tunnels + Access, load balancing/rulesets/certificates — the prepared `cloudflare` row and the selfhosted-k8s cert/dns/ingress rows
- Accept: current-spelling resources on the one ABI under an arm-scoped `Provider`, `ZeroTrustTunnelCloudflared` ingress, `R2Bucket`/`WorkersScript` service-equivalence rows, `getXOutput` reads, Doppler-bound `apiToken`
- Reject: deprecated aliases (`Record`/`WorkerScript`/`Access*`/`Tunnel*`), per-resource providers, literal API tokens, roster enumeration over the ABI pattern, out-of-band state reads
