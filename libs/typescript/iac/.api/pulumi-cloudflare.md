# [TS_IAC_API_PULUMI_CLOUDFLARE]

`@pulumi/cloudflare` is the Terraform-bridged Cloudflare provider — the prepared `cloudflare` dispatch row and the cert/dns/ingress source for the `selfhosted-k8s` arm. One Pulumi resource ABI governs every resource class, so a Cloudflare resource is a row on that shape and a new capability extends the row-space, never a bespoke type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/cloudflare`
- package: `@pulumi/cloudflare` (Apache-2.0)
- module: `@pulumi/cloudflare` (flat resource + `getX`/`getXOutput` barrel; `config`/`types.input`/`types.output` namespaces)
- asset: the bridged Cloudflare resource surface — DNS/zones, the Workers/KV/R2/Pages/Queues/D1/Hyperdrive edge platform, Zero-Trust tunnels + Access, load balancing/rulesets/certificates
- runtime: `node` — the Terraform-bridged provider plugin auto-downloads on first registration; no local daemon or CLI
- depends: `@pulumi/pulumi` — provider-schema-shaped `Input<T>` args and `Output<T>` outputs
- rail: cloud-row / cloudflare + the selfhosted-k8s cert/dns/ingress rows

## [02]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized bridged-resource shape every resource class instantiates

`opts` on every constructor and `.get` is the universal `pulumi.CustomResourceOptions` seam (`provider`/`dependsOn`/`parent`/`protect`/`ignoreChanges`/`import`, `.api/pulumi-pulumi.md`); nested arg shapes live under `types.input.*`, every output prop is the `Output<T>` mirror of its `XArgs` field, and `XState` — the `.get` shape — is the all-optional `Output` mirror of `XArgs`.

| [INDEX] | [MEMBER]                                                                | [SHAPE_BOUNDARY]                                         |
| :-----: | :---------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `new X(name, XArgs, opts?)`                                             | construct any resource; `XArgs` fields are `Input<T>`    |
|  [02]   | `X.get(name, id, XState?, opts?)`                                       | adopt an existing Cloudflare object by id                |
|  [03]   | `X.isInstance(obj)`                                                     | multi-SDK-safe guard `obj is X`                          |
|  [04]   | `getX(args, InvokeOptions?)` / `getXOutput(args, InvokeOutputOptions?)` | eager `Promise<GetXResult>` / graph `Output<GetXResult>` |

## [03]-[RESOURCE_FAMILIES]

[DNS_SCOPE]: zones and records — the cert/dns rows every cloud arm binds

| [INDEX] | [SYMBOL]                                          | [ROLE]                                                               |
| :-----: | :------------------------------------------------ | :------------------------------------------------------------------- |
|  [01]   | `Zone`                                            | the domain zone (`account`, `name`, `type`)                          |
|  [02]   | `DnsRecord`                                       | a DNS record (`zoneId`, `type`, `name`, `content`, `proxied`, `ttl`) |
|  [03]   | `ZoneDnssec` / `ZoneDnsSettings` / `ZoneSetting`  | DNSSEC, zone DNS settings, per-setting overrides                     |
|  [04]   | `CustomHostname` / `CustomHostnameFallbackOrigin` | SaaS custom-hostname + recovery                                      |

[EDGE_SCOPE]: Workers, KV, R2, Pages, Queues, D1, Hyperdrive — the edge compute and object-store rows

| [INDEX] | [SYMBOL]                                                                                       | [ROLE]                               |
| :-----: | :--------------------------------------------------------------------------------------------- | :----------------------------------- |
|  [01]   | `R2Bucket` (+ `R2BucketLifecycle`/`R2BucketCors`/`R2BucketEventNotification`/`R2CustomDomain`) | object store + policies              |
|  [02]   | `WorkersScript` / `WorkersRoute` / `WorkersCustomDomain`                                       | edge compute + routing               |
|  [03]   | `WorkersKvNamespace` / `WorkersKv`                                                             | edge KV namespace + entries          |
|  [04]   | `PagesProject` / `PagesDomain`                                                                 | static hosting + domain              |
|  [05]   | `Queue` / `D1Database` / `HyperdriveConfig`                                                    | edge queue / SQLite / DB-accelerator |

[INGRESS_SCOPE]: Zero-Trust tunnel and Access — the selfhosted-k8s ingress rows

`ZeroTrustTunnelCloudflared` opens a cloudflared tunnel into a cluster with no public IP; `…Config`/`…Route` carry the ingress rules and routes, and `ZeroTrustAccess*` fronts it with authentication.

| [INDEX] | [SYMBOL]                                                          | [ROLE]                                                |
| :-----: | :---------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `ZeroTrustTunnelCloudflared`                                      | the named cloudflared tunnel into the cluster         |
|  [02]   | `ZeroTrustTunnelCloudflaredConfig` / `…Route` / `…VirtualNetwork` | tunnel ingress rules, network routes, virtual network |
|  [03]   | `ZeroTrustAccessApplication` / `…Policy` / `…Group`               | Access app + authorization policy + identity group    |
|  [04]   | `ZeroTrustAccessServiceToken` / `…IdentityProvider`               | service-to-service token, IdP binding                 |
|  [05]   | `getZeroTrustTunnelCloudflaredToken` / `…Output`                  | connector credential read → `TUNNEL_TOKEN`            |

[SHAPES]: intent arg objects an implementer fills, keyed to the rows above.
- [01]-[TUNNEL]: args `{ accountId, name (required), tunnelSecret?, configSrc? }`; `id` is the CNAME target base (`<id>.cfargotunnel.com`).
- [02]-[ROUTING]: config args `{ accountId, tunnelId, config: { ingresses: [{ hostname?, service (required), path?, originRequest? }], originRequest? } }`; the last ingress row is the catch-all (`service: "http_status:404"`).
- [03]-[ACCESS]: app args `{ accountId?, domain?, type?, name?, sessionDuration?, policies: [{ id?, decision?, … }] }`; policy args `{ accountId?, name (required), decision (required), includes: [{ everyone?, email?, group?, anyValidServiceToken?, … }] }`.
- [05]-[TOKEN]: `({ accountId, tunnelId }) → { token }` — the `TUNNEL_TOKEN` the in-cluster cloudflared Deployment runs with.

[TRAFFIC_SCOPE]: load balancing, rulesets, certificates

`Ruleset` is the unified rules engine — WAF, transform, and redirect ride one `phase`/`rules` resource.

| [INDEX] | [SYMBOL]                                                    | [ROLE]                                        |
| :-----: | :---------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `LoadBalancer` / `LoadBalancerPool` / `LoadBalancerMonitor` | steering + origin pool + health monitor       |
|  [02]   | `Ruleset`                                                   | unified rules engine (`phase`, `rules`)       |
|  [03]   | `OriginCaCertificate` / `CertificatePack` / `TotalTls`      | origin CA cert, edge cert pack, automatic TLS |
|  [04]   | `SpectrumApplication` / `PageRule`                          | L4 app proxy / page rule                      |

## [04]-[PROVIDER]

[PROVIDER_SCOPE]: the API credential — the cloudflare arm seam

One `Provider` per arm carries the credential and threads through `opts.provider`: `apiToken` is the canonical scoped credential, with `apiKey`+`email` and `apiUserServiceKey` as mutually-exclusive alternates.

| [INDEX] | [FIELD]                         | [TYPE]          | [MEANING]                                                          |
| :-----: | :------------------------------ | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `apiToken`                      | `Input<string>` | scoped API token (canonical); bind a `doppler` secret Output       |
|  [02]   | `apiKey` / `email`              | `Input<string>` | global API key + account email; mutually exclusive with `apiToken` |
|  [03]   | `apiUserServiceKey` / `baseUrl` | `Input<string>` | restricted-endpoint service key, HTTP base override                |

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A Cloudflare resource is `new X(name, XArgs, opts?)` on the one ABI; a new capability lands as a row on a seeded family, never a bespoke shape.
- `Match.exhaustive` dispatch (`provider/dispatch`, `libs/typescript/.api/effect.md`) constructs the `cloudflare` arm's one `Provider` from a `Schema`-decoded `StackSpec` `apiToken` ref and threads it via `opts.provider`; per-resource providers are rejected.
- `ZeroTrustTunnelCloudflared` + `…Config` route the selfhosted-k8s ingress to the in-cluster service, fronted by `ZeroTrustAccessApplication`/`…Policy`; a `DnsRecord` CNAMEs the hostname to `<id>.cfargotunnel.com`, so the cluster needs no public IP.
- An existing zone, record, or bucket reads through `getXOutput` when it feeds an `Input` and `getX` for an eager `async` read, never an out-of-band Cloudflare-state re-derivation.

[STACKING]:
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `Provider.apiToken` binds a single-key `getSecretsOutput({ project, config }).apply(r => r.map["CLOUDFLARE_TOKEN"])` `Output<string>` — the in-graph credential bind the Doppler provider-credential fan-in names for this row; `WorkersScript` secret bindings draw from the same canonical store.
- `@pulumi/tls`(`.api/pulumi-tls.md`): `CertRequest.certRequestPem` feeds `OriginCaCertificate.csr` and `PrivateKey.privateKeyPem` supplies the origin key where Cloudflare does not mint it — the cert material the `kube/traffic` rows consume.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): every `XArgs` field is `Input<T>` and every output an `Output<T>` mirror threaded through `opts.provider` via `CustomResourceOptions`; resource failures fold into the `automation.UpResult` run receipt, and a `TUNNEL_TOKEN` crossing a manifest travels only `pulumi.secret`-wrapped.
- within-lib: `effect` owns the `Match` arm dispatch and the `StackSpec`/`StackOutputs` `Schema`; typed `StackOutputs` (zone id, tunnel id, record fqdn) project through `stack/output`.

[LOCAL_ADMISSION]:
- `Provider.apiToken` binds the Doppler secret `Output`, never a literal.
- `DnsRecord`/`CustomHostname` + `OriginCaCertificate` pair the cert/dns rows with the in-cluster ingress at `kube/traffic`.
- `R2Bucket` is the object-store equivalent and `WorkersScript`/`PagesProject` the compute/hosting equivalents in the service-equivalence map (`provider/surface`); an app finalizes the row with a `StackSpec` value — zone, token ref — never a lib edit.

[RAIL_LAW]:
- Package: `@pulumi/cloudflare`
- Owns: DNS/zones, the Workers/KV/R2/Pages/Queues/D1/Hyperdrive edge platform, Zero-Trust tunnels + Access, load balancing/rulesets/certificates — the prepared `cloudflare` row and the selfhosted-k8s cert/dns/ingress rows
- Accept: current-spelling resources on the one ABI under an arm-scoped `Provider`, `ZeroTrustTunnelCloudflared` ingress, `R2Bucket`/`WorkersScript` service-equivalence rows, `getXOutput` reads, a Doppler-bound `apiToken`
- Reject: per-resource providers, a literal API token, roster enumeration over the ABI pattern, out-of-band state reads
