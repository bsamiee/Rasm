# [TS_IAC_API_PULUMI_CLOUDFLARE]

`@pulumi/cloudflare` is the Terraform-bridged Cloudflare provider — the prepared `cloudflare` dispatch row and the cert/dns/ingress source for the `selfhosted-k8s` arm. One Pulumi resource ABI governs every resource class, so a Cloudflare resource is a row on that shape and a new capability extends the row-space, never a bespoke type.

## [01]-[RESOURCE_ABI]

[ABI_SCOPE]: the parameterized bridged-resource shape every resource class instantiates

`opts` on every constructor and `.get` is the universal `pulumi.CustomResourceOptions` parameter (`provider`/`dependsOn`/`parent`/`protect`/`ignoreChanges`/`import`, `.api/pulumi-pulumi.md`); nested arg shapes live under `types.input.*`, every output prop is the `Output<T>` mirror of its `XArgs` field, and `XState` — the `.get` shape — is the all-optional `Output` mirror of `XArgs`.

| [INDEX] | [MEMBER]                                                                | [SHAPE_BOUNDARY]                                         |
| :-----: | :---------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `new X(name, XArgs, opts?)`                                             | construct any resource; `XArgs` fields are `Input<T>`    |
|  [02]   | `X.get(name, id, XState?, opts?)`                                       | adopt an existing Cloudflare object by id                |
|  [03]   | `X.isInstance(obj)`                                                     | multi-SDK-safe guard `obj is X`                          |
|  [04]   | `getX(args, InvokeOptions?)` / `getXOutput(args, InvokeOutputOptions?)` | eager `Promise<GetXResult>` / graph `Output<GetXResult>` |

## [02]-[RESOURCE_FAMILIES]

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
- [03]-[ACCESS]: app args `{ accountId?, domain?, type?, name?, sessionDuration?, policies?: [{ id?, decision?, … }] }`; policy args `{ accountId (required), name (required), decision (required), includes?/excludes?/requires?: [{ everyone?, email?, group?, anyValidServiceToken?, … }] }` — a policy names its account while an app infers one, so the pair cannot share a coordinate record.
- [03]-[ACCESS_REUSE]: `ZeroTrustAccessPolicy.reusable: Output<boolean>` and `.appCount: Output<number>` resolve the sharing posture the `id` binding creates — an app admits a policy by `policies[].id`, so one policy realized once fronts every app referencing it, and the two outputs read back whether the account granted that reuse and how many apps took it; both are resolved-only, never `ZeroTrustAccessPolicyArgs` fields, so a fence reads the posture and never requests it.
- [05]-[TOKEN]: `({ accountId, tunnelId }) → { token }` — the `TUNNEL_TOKEN` the in-cluster cloudflared Deployment runs with.

[TRAFFIC_SCOPE]: load balancing, rulesets, certificates

`Ruleset` is the unified rules engine — WAF, transform, and redirect ride one `phase`/`rules` resource.

| [INDEX] | [SYMBOL]                                                    | [ROLE]                                        |
| :-----: | :---------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `LoadBalancer` / `LoadBalancerPool` / `LoadBalancerMonitor` | steering + origin pool + health monitor       |
|  [02]   | `Ruleset`                                                   | unified rules engine (`phase`, `rules`)       |
|  [03]   | `OriginCaCertificate` / `CertificatePack` / `TotalTls`      | origin CA cert, edge cert pack, automatic TLS |
|  [04]   | `SpectrumApplication` / `PageRule`                          | L4 app proxy / page rule                      |

- `Ruleset` args: `{ zoneId | accountId, name, kind ("zone" | "custom" | "managed" | "root"), phase, rules }`; the response-header phase is `"http_response_headers_transform"`, each rule `{ expression, action, actionParameters }` with `action: "rewrite"` and `actionParameters.headers: { [name]: { operation ("add" | "set" | "remove"), value } }`; expressions read `http.request.uri.path` through `starts_with`/`ends_with`, and EVERY matching header-transform rule in the phase applies — first-match narrowing belongs to the CDN dialects, never here.

## [03]-[PROVIDER]

[PROVIDER_SCOPE]: the API credential — the cloudflare arm boundary

One `Provider` per arm carries the credential and threads through `opts.provider`: `apiToken` is the canonical scoped credential, with `apiKey`+`email` and `apiUserServiceKey` as mutually-exclusive alternates.

| [INDEX] | [FIELD]                         | [TYPE]          | [MEANING]                                                          |
| :-----: | :------------------------------ | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `apiToken`                      | `Input<string>` | scoped API token (canonical); bind a `doppler` secret Output       |
|  [02]   | `apiKey` / `email`              | `Input<string>` | global API key + account email; mutually exclusive with `apiToken` |
|  [03]   | `apiUserServiceKey` / `baseUrl` | `Input<string>` | restricted-endpoint service key, HTTP base override                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every Cloudflare resource constructs as `new X(name, XArgs, opts?)` on the one ABI, so a new capability lands as a row on a seeded family.
- `Match.exhaustive` dispatch (`provider/dispatch`, `libs/typescript/.api/effect.md`) constructs the `cloudflare` arm's one `Provider` from a `Schema`-decoded `StackSpec` `apiToken` ref and threads it via `opts.provider`; per-resource providers are rejected.
- `ZeroTrustTunnelCloudflared` + `…Config` route the selfhosted-k8s ingress to the in-cluster service, fronted by `ZeroTrustAccessApplication`/`…Policy`; a `DnsRecord` CNAMEs the hostname to `<id>.cfargotunnel.com`, so the cluster needs no public IP.
- `getXOutput` reads an existing zone, record, or bucket into an `Input` and `getX` serves the eager `async` read, so Cloudflare state never re-derives out of band.

[STACKING]:
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `Provider.apiToken` binds a single-key `getSecretsOutput({ project, config }).apply(r => r.map["CLOUDFLARE_TOKEN"])` `Output<string>` — the in-graph credential bind the Doppler provider-credential fan-in names for this row; `WorkersScript` secret bindings draw from the same canonical store.
- `@pulumi/tls`(`.api/pulumi-tls.md`): `CertRequest.certRequestPem` feeds `OriginCaCertificate.csr` and `PrivateKey.privateKeyPem` supplies the origin key where Cloudflare does not mint it — the cert material the `kube/traffic` rows consume.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): every `XArgs` field is `Input<T>` and every output an `Output<T>` mirror threaded through `opts.provider` via `CustomResourceOptions`; resource failures reject the lifecycle operation and map to `DeployFault`, and a `TUNNEL_TOKEN` crossing a manifest travels only `pulumi.secret`-wrapped.
- within-lib: `effect` owns the `Match` arm dispatch and the `StackSpec`/`StackOutputs` `Schema`; typed `StackOutputs` (zone id, tunnel id, record fqdn) project through `stack/output`.

[LOCAL_ADMISSION]:
- `Provider.apiToken` binds the Doppler secret `Output`, never a literal.
- `DnsRecord`/`CustomHostname` + `OriginCaCertificate` pair the cert/dns rows with the in-cluster ingress at `kube/traffic`.
- `R2Bucket` is the object-store equivalent and `WorkersScript`/`PagesProject` the compute/hosting equivalents in the service-equivalence map (`provider/surface`); an app finalizes the row with a `StackSpec` value — zone, token ref — never a lib edit.
