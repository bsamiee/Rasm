# [STACK_IAC] — ultra-stacking research for libs/typescript/iac

Findings-only dossier for the improver. Spellings are source-verified against the 22 folder `.api` catalogs on disk and the branch substrate roster (`libs/typescript/.api/`), or registry-verified via the RULING prefetch dossier (`prefetch-ui-iac.md` §[B]) — prefetch rows carry `[RULED]`. The folder is Pulumi-over-Automation-API driven as ONE Effect rail; nine pages, no `src/` yet, `IDEAS.md`/`TASKLOG.md` empty. The `selfhosted-k8s` arm is realized end-to-end; `selfhosted-docker` is realized; `aws` is partially realized; `gcp`/`cloudflare` arms are single-resource stubs; six admitted packages (`eks`, `cloudinit`, `synced-folder`, `acme`, `github`, `esc-sdk`, `pulumiservice`) have ZERO constructing fence.

The single largest finding: the folder charter (IaC as a world-class product, thousands-of-apps/multi-tenant, single+multi-tenant, K8s+non-K8s) is met at ~40% of admitted capability. The gaps close almost entirely with capability ALREADY ADMITTED (unused packages + unused members) plus typed-CRD targets via `crd2pulumi` — very little new npm is required. Two RULED corrections are load-bearing dead-ends the current pages carry.

---

## [0]-[RULED CORRECTIONS THE IMPROVER MUST HONOR FIRST]

These are closed ground from `prefetch-ui-iac.md` §[B4]/[C] — do not re-research; apply as law.

1. **CNPG in-tree barman is a DEAD END.** `kube/data.md` §[4] authors `spec.backup.barmanObjectStore` + `s3Credentials` directly on the `Cluster` CR. `[RULED]`: in-tree `barmanObjectStore` is DEPRECATED, removal slated CNPG **1.31**. The only forward path is the **Barman Cloud CNPG-I plugin** (`plugin-barman-cloud` v0.13.0) — author `spec.plugins[].name: "barman-cloud.cloudnative-pg.io"` on the `Cluster` plus a separate `ObjectStore` CRD (`barmancloud.cnpg.io/v1`) carrying `configuration.destinationPath`/`endpointURL`/`s3Credentials`. This is a structural rewrite of `Postgres`, not a field rename.
2. **CNPG CR immutability.** `[RULED]`: `cluster` references on dependent CRDs (`Database`, `ScheduledBackup`, `Pooler`) are CEL-validated immutable (1.29.2+/1.30) — generators must treat them as create-only (they already are on `data.md`, keep it).
3. **Gateway API is GA and admitted as a typed target.** `kube/traffic.md` uses legacy `networking/v1.Ingress` + hardcoded nginx `_EDGE`. `[RULED]` §[B4]: Gateway API v1.6.0 ships ALL core resources as `v1` (`GatewayClass`/`Gateway`/`HTTPRoute`/`GRPCRoute`/`TLSRoute`/`TCPRoute`/`UDPRoute`/`ReferenceGrant`/`BackendTLSPolicy`/`ListenerSet`). This is the modern edge; Ingress is the legacy fallback. See §[D1].
4. **crd2pulumi, not raw `CustomResource<any>`.** `data.md` declares the CNPG `Cluster`/`Database`/`ScheduledBackup` through `apiextensions.CustomResource` with an untyped `[field: string]: Input<any>` catch-all. `[RULED]` §[B4]: raw `CustomResource<any>` forfeits compile-time shape checking exactly where the estate is PG-heavy; generate typed TS SDKs with `crd2pulumi` (Go CLI, dev-time tool, NOT an npm dep — commit generated code, regenerate on operator bumps). Same for cert-manager / external-dns / Gateway API / Capsule CRDs.
5. **Deprecated-alias spellings already correct.** `cloudflare.DnsRecord` (not `Record`), `docker-build.Image` (not `docker.Image`), `ManagedNodeGroup`/`NodeGroupV2` (not `NodeGroup`) — the pages use current spellings. Keep.

---

## [A]-[UNDERUTILIZED MEMBERS PER CATALOG]

Exact spellings; each with the owning page it should land in. `[named-growth]` = the page's own prose names it as future growth but no fence realizes it; `[silent]` = catalogued capability with no page mention.

### `@pulumi/pulumi` (`pulumi-pulumi.md`) → `program/automation.md`, new fleet owner
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| cross-stack read | `StackReference` + `.getOutput`/`.requireOutput`/`.getOutputDetails` | `automation.md` or new `program/fleet.ts` | `[named-growth]` provider.md names it "when a multi-stack estate earns one" — the multi-tenant platform-stack↔tenant-stack read seam (§[B1]) |
| batch adoption | `Stack.import(opts: ImportOptions)` with `ImportOptions.resources: ImportResource[]` (+`converter`/`generateCode`/`protect`) | `automation.md` | `[named-growth]` demoted "prepared member"; the operator disaster/onboarding verb |
| ESC attach | `Stack.addEnvironments(...envs)` / `listEnvironments()` / `removeEnvironment(env)` | `automation.md` | `[named-growth]` demoted; the ONLY stack↔ESC path (no typed `StackSettings` field) — pairs with §[D5] |
| stack tags | `Stack.getTag`/`setTag`/`listTags` | `automation.md` or fleet owner | `[silent]` fleet organization/labelling for multi-tenant sweeps |
| audit | `Stack.history(pageSize?, page?, secrets?)` / `Stack.info()` | `automation.md` | `[silent]` update-history evidence beside the `RunReceipt` |
| lifecycle hooks | `ResourceHook` / `ErrorHook` | `spec.md` `Tier` base | `[silent]` pre/post-op + error-transform binding a tier could carry internally |
| forward ref | `deferredOutput<T>(): [Output<T>, resolver]` | any tier with a cyclic ref | `[silent]` breaks tier construction-order cycles without restructuring |
| output recovery | `recover<T>(o, (err)=>Input<T>)` | output/tier reads | `[silent]` resilience on a rejected `Output` — resistance-as-concept (§[D8]) |
| structured log | `log.{debug,info,warn,error,hasErrors}` | tiers | `[silent]` per-resource engine log vs ad-hoc |
| remote exec | `RemoteWorkspace.{create,select,createOrSelect}Stack(RemoteGitProgramArgs, RemoteWorkspaceOptions)` → `RemoteStack` | `automation.md` | `[named-growth]` prepared row — Deployments-compute execution backend (Git-source only) |
| release lifecycle | `helm.v3.Release` (`atomic`/`waitForJobs`/`timeout`/`recreatePods`) | `observe.md`/`data.md` | correctly unused; reach only for true release lifecycle — keep as escape hatch |

### `@pulumi/kubernetes` (`pulumi-kubernetes.md`) → `observe.md`, `data.md`, `workload.md`, `provider.md`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| chart provenance | `helm.v4.Chart` args `verify: true` + `keyring: Asset` | `data.md`/`observe.md` | `[named-growth]` data.md says "provenance verify rides the pins when a keyring asset accompanies them" — no fence wires `keyring` |
| rendered set | `chart.resources: Output<any[]>` | `observe.md` Boards, `policy.md` | `[silent]` feed CrossGuard `validateStack` or Grafana dashboard discovery (§[C7]) |
| post-render | `helm.v4.Chart` arg `postRenderer: { command, args }` | `data.md`/`observe.md` | `[silent]` kustomize post-render hook for values a chart can't express |
| SSA patch | `apiextensions.CustomResourcePatch` | `data.md` | `[silent]` patch operator-owned objects (CNPG defaults) via SSA field-manager |
| typed CRD install | `apiextensions.v1.CustomResourceDefinition` | `data.md` | `[silent]` explicit CRD install vs chart `skipCrds:false` (needed for crd2pulumi flow §[0.4]) |
| RBAC realizers | `rbac.authorization.k8s.io/v1.Role` + `RoleBinding` (+`ClusterRole`/`ClusterRoleBinding`) | `workload.md` | `[silent]` identity `_map` cell is `ServiceAccount + Role + RoleBinding` — workload builds ONLY the ServiceAccount; 2/3 of the identity capability is unrealized |
| autoscaler | `autoscaling` group (HPA) | `workload.md` | `[named-growth]` "an HPA is one autoscaling row when a profile earns it" |
| disruption budget | `policy` group `PodDisruptionBudget` | `workload.md` | `[silent]` fleet resilience — a `fleet` scale row should carry a PDB |
| storage class | `storage.k8s.io/v1.StorageClass` | `data.md`/`workload.md` | `[silent]` profile-selectable storage class vs chart default |
| provider GC | `Provider.deleteUnreachable` / `renderYamlToDirectory` / `helmReleaseSettings` | `provider.md` arm | `[silent]` unreachable-cluster GC + render-only preview |

### `@pulumi/policy` (`pulumi-policy.md`) → `operate/policy.md`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| combined verdict | `validateRemediateResourceOfType(Class, cb)` → `{ validateResource, remediateResource }` | `policy.md` `_managedBy` | `[silent]` one callback yields both halves — collapses a validate+remediate pair |
| compliance frame | `PolicyComplianceFramework { name, version, reference, specification }` on the `framework` field | `policy.md` rows | `[named-growth]` "a compliance frame is one framework field on the rows it covers" — SOC2/PCI/CIS mapping absent |
| property deps | `PolicyResource.propertyDependencies: Record<string, PolicyResource[]>` | `policy.md` `_networkFence`/new rows | `[silent]` per-property dep edges for deeper cross-resource invariants (every `Cluster`→`ScheduledBackup`, every `HTTPRoute`→`BackendTLSPolicy`) |
| multi-validation | `validateResource?: ResourceValidation | ResourceValidation[]` | `policy.md` | `[silent]` ordered multiple checks per policy row |
| metadata | `remediationSteps` / `url` on `Policy` base | `policy.md` rows | `[silent]` operator remediation guidance surfaced with the violation |
| stack tags | `args.stackTags: ReadonlyMap<string,string>` | `policy.md` | `[silent]` tenant-scoped policy differentiation |

### `@pulumiverse/doppler` (`pulumiverse-doppler.md`) → `operate/secret.md`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| machine identity | `ServiceAccount` + `ServiceAccountToken` (`workplaceRole`/`workplacePermissions`) | `secret.md` | `[named-growth]` "the identity upgrade over the config-scoped token when an estate earns workplace RBAC" |
| RBAC | `Group`/`GroupMember`/`GroupMembers`, `ProjectRole`, `projectmember.{Group,ServiceAccount}` | `secret.md` | `[silent]` multi-tenant/team secret access control (§[B1]) |
| sync targets | `secretssync.{AwsParameterStore,TerraformCloud,Circleci,Flyio}` + matching `integration.<T>` | `secret.md` `_MIRRORS` | `[named-growth]` growth names "parameter store, workspace variables, CI contexts, app platforms" — the exact rows; `_MIRRORS` carries only 2 of 6 |
| composed secrets | `Secret.computed` (`${ref}` interpolation) + `Secret.visibility` | `secret.md` | `[silent]` server-side referenced/composed secrets |
| webhook auth | `Webhook.authentication` / `Webhook.payload` | `secret.md` `Secrets.webhook` | `[silent]` webhook sets only `url`/`enabledConfigs` — no auth/payload |

### `@pulumi/postgresql` (`pulumi-postgresql.md`) → `kube/data.md` (selfhosted-docker cell + drift)
Realized only on the `selfhosted-docker` cell (deploy host can't reach `.svc`). Underused within that boundary + drift:
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| logical replication | `Publication` / `Subscription` | `data.md` | `[named-growth]` "a logical-replication Publication seam" — multi-region/tenant replication |
| replication slots | `ReplicationSlot` / `PhysicalReplicationSlot` | `data.md` | `[silent]` WAL slot management |
| drift read-back | `getSchemas`/`getSchemasOutput`, `getTables`, `getSequences` | `policy.md` `Drift`/`data.md` | `[named-growth]` catalog: "feed the policy/drift read-back and store-side conformance check" — never wired |
| default ACL | `DefaultPrivileges` | `data.md` finalize | `[silent]` default privileges for future objects vs per-object grant |
| security label | `SecurityLabel` | `data.md` | `[silent]` RLS/anon labels |
| cloud IAM auth | `Provider.awsRdsIamAuth`/`azureIdentityAuth`/`gcpIamImpersonateServiceAccount` | `provider.md` prepared arms | `[silent]` platform-IAM auth flip for the prepared cloud DB cells |

### `@pulumi/cloudflare` (`pulumi-cloudflare.md`) → `kube/traffic.md`, `provider.md` cloudflare arm
266 classes; ~10 used. Named-growth/stub cells:
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| WAF/rules | `Ruleset` (`phase`, `rules`) | `traffic.md` | `[named-growth]` "a WAF posture is one Ruleset member on the same provider" |
| identity group | `ZeroTrustAccessGroup` | `traffic.md` | `[named-growth]` "when a group earns it" |
| object/compute cells | `R2Bucket`(+`R2BucketLifecycle`/`R2BucketCors`), `WorkersScript`/`WorkersRoute`, `PagesProject`, `D1Database`, `Queue`, `HyperdriveConfig`, `WorkersKvNamespace` | `provider.md` cloudflare arm | `[named-growth]` `_map` cloudflare cells — arm is a `DnsRecord`-only stub |
| cert cells | `OriginCaCertificate`, `CertificatePack`, `TotalTls`, `ZoneDnssec` | `provider.md`/`traffic.md` | `[named-growth]` cert `_map` cells unrealized |
| multi-tenant hostnames | `CustomHostname` (+`CustomHostnameFallbackOrigin`) | `traffic.md` | `[silent]` SaaS per-tenant custom hostnames (§[B1]) |
| load balancing | `LoadBalancer`/`LoadBalancerPool`/`LoadBalancerMonitor` | `provider.md` | `[silent]` multi-origin steering |

### `@pulumiverse/grafana` (`pulumiverse-grafana.md`) → `operate/observe.md`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| alert compile | `alerting.RuleGroup` (`folderUid`/`intervalSeconds`/`rules` verified), `alerting.ContactPoint`, `alerting.NotificationPolicy` | `observe.md` `_alertRows` | **DECLARED-STUB** — the `Alert.Spec`→rule-model compile is the page's standing design item; the improver MUST realize `_alertRows` |
| SLO | `slo.Slo` (+`slo.getSlos`) | `observe.md` `_sloRows` | **DECLARED-STUB** — realize alongside alerts |
| paging | `alerting.MuteTiming` / `alerting.MessageTemplate` / `alerting.AlertEnrichment` | `observe.md` | `[named-growth]` "when a paging policy earns them" |
| machine identity | `oss.ServiceAccount` + `oss.ServiceAccountToken` | `observe.md` Boards provider | `[named-growth]` "the machine-identity upgrade over the admin:password binding" |
| board extras | `oss.LibraryPanel`, `oss.Playlist`, `oss.Annotation`, `oss.Organization`/`Team`/`User`, `oss.SsoSettings` | `observe.md` | `[silent]` multi-tenant org isolation (`oss.Organization` per tenant §[B1]) |
| provider resilience | `Provider.retryStatusCodes`/`retryWait`/`httpHeaders` | `observe.md` Boards | `[silent]` only `retries:3` set — tune the transient-fault posture |

### `@pulumi/tls` (`pulumi-tls.md`) → `operate/secret.md`, `kube/traffic.md`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| foreign chain pin | `getCertificateOutput({url|content, verifyChain?})` | `secret.md` `Certs` | `[named-growth]` "a foreign endpoint's chain pins through getCertificateOutput when an arm must trust material it does not mint" |
| pubkey derive | `getPublicKey`/`getPublicKeyOutput` | `secret.md` | `[silent]` derive pubkey+fingerprints |
| openssh pubkey | `PrivateKey.publicKeyOpenssh` | github source-control leg (§[B4]) | `[silent]` deploy-key material |
| CA-chain semantics | `SelfSignedCert.maxPathLength`/`setAuthorityKeyId`/`setSubjectKeyId` | `secret.md` `Certs.root` | `[silent]` proper CA path-length + key-id chaining |

### `@pulumi/command` (`pulumi-command.md`) → `program/provider.md` `Bootstrap`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| host reads | `local.run(RunArgs)` / `local.runOutput(RunOutputArgs)` | `provider.md` `Bootstrap` | `[named-growth]` "an unconditional host fact is local.runOutput when it threads the graph, local.run for an eager read" |
| bastion hop | `ConnectionArgs.proxy: ProxyConnectionArgs` | `provider.md` `Bootstrap` | `[named-growth]` "a bastion hop is one proxy row on the same connection when the estate earns one" |
| SSH hardening | `ConnectionArgs.hostKey` / `agentSocketPath` / `perDialTimeout` / `dialErrorLimit` | `provider.md` `Bootstrap` | `[silent]` `hostKey` pinning is a real security gap (no host-key verification today) |
| host mutation | `local.Command` | `provider.md`/new maintenance | `[silent]` deploy-host-side one-shot mutation |

### `@pulumi/random` (`pulumi-random.md`) → `spec.md`, `workload.md`, `secret.md`
| member | spelling | landing | note |
| :-- | :-- | :-- | :-- |
| sortable ids | `RandomUuid7` (`result`, v7 time-ordered) | `spec.md` `StackOutputs` | `[silent]` catalog names it StackOutputs; sortable deployment ids |
| seeded spread | `RandomShuffle` (`inputs`→`results`, `seed`-stable) | `workload.md` | `[silent]` seed-stable AZ/replica spreading across `up` (§[C1]) |
| digest store | `RandomString.bcryptHash` | `secret.md` | `[named-growth]` "the bcryptHash projection serves a consumer that stores a digest instead of the value" |
| stable names | `RandomPet` / `RandomInteger` / `RandomId.{hex,b64Url}` | tiers | `[silent]` DNS-safe suffixes, human-friendly names |

---

## [B]-[NEVER-USED ADMITTED CAPABILITY THE FOLDER CONCEPT DEMANDS]

Whole packages/concerns admitted (pinned in `pnpm-workspace.yaml`, catalogs on disk) with ZERO constructing fence, that the charter directly demands.

### [B1] Multi-tenancy — ENTIRELY ABSENT (the single largest gap)
Charter law #3 "THOUSANDS OF APPS, NOT A MEGA-APP" and #10 "single-tenant, multi-tenant, K8s and non-K8s." The folder has NO tenancy dimension: `StackSpec` is single-app, no `Tenant`, no isolation tier. `[RULED]` §[B5] is a full multi-tenant provisioning ladder the folder never touches:
- **Capsule** `Tenant` CRD (v0.13.6, typed target via crd2pulumi) — policy-based soft tenancy over vanilla namespaces (RBAC/NetworkPolicy/ResourceQuota propagation). One `Tenant` CR per logical tenant.
- **vcluster** (v0.35, typed target) — virtual control-plane-per-tenant for hard multi-tenancy (untrusted/external tenant, own CRDs/API versions).
- **PG three-tier tenant escalation** `[RULED]`: (1) shared-schema + RLS in one `Cluster` (highest density — `Tenancy.rls` already crosses from `@rasm/ts/data`); (2) database-per-tenant logical — one `Cluster`, N `Database` CRDs (declarative `owner`/`extensions`/`schemas` — "this is what the CRD exists for, never hand-rolled CREATE DATABASE"); (3) dedicated `Cluster` per tenant. `data.md` builds exactly ONE `Database` for `spec.app` — the tenancy escalation is absent.
- **Doppler tenant isolation**: `ServiceAccount`/`Group`/`ProjectRole`/`projectmember.*` (§[A]) scope secret access per tenant.
- **Grafana tenant isolation**: `oss.Organization` per tenant.
- **Cloudflare**: `CustomHostname` per tenant.
Encode tenancy as POLICY ROWS/spec data, never bespoke code paths (`[RULED]` pattern-selection law). This is a new `StackSpec.tenancy` axis + a `kube/tenant.ts` owner (new page earned) + policy escalation rows.

### [B2] Pulumi Kubernetes Operator (PKO) — in-cluster reconcile loop absent
`[RULED]` §[B6]: PKO v2.7.0 is `admit-substrate` — in-cluster continuous reconciliation (`spec.refresh`, `spec.continueResyncOnCommitMatch`, `spec.resyncFrequencySeconds`), desired state as a real program via `Stack`/`Program` CRs. The folder's `Drift.sweep` is a LOCAL `previewRefresh` poll; it has no in-cluster reconcile loop. PKO is the Pulumi-native equivalent of Crossplane's reconcile (Crossplane REJECTED — two desired-state owners is split-brain). Also the tenant-submitted-CR-triggers-provisioning path routes through PKO `Program`/`Stack` CRs. Lands as a new `operate/reconcile.ts` owner or a `Drift` sibling.

### [B3] `@pulumi/pulumiservice` — Cloud control plane, ZERO consumption
Entire package unused; no owning page. `[RULED]` §[B3]: triggers/schedules/review-stacks are Cloud-only and MUST configure via this package, never hand-rolled REST. Members: `DeploymentSettings` (`vcs.previewPullRequests` + `pullRequestTemplate` = per-PR review stacks), `DriftSchedule`(`autoRemediate:false`), `TtlSchedule` (hosted ephemeral twin), `DeploymentSchedule`(`pulumiOperation`), `Webhook` (drift/deployment filters — `WebhookFilters` spans `DriftDetected`/`UpdateFailed`/etc), `Environment`/`EnvironmentVersionTag`, RBAC (`Team`/`TeamStackPermission`/`OrganizationRole`/`ApprovalRule`), tokens (`AccessToken` + `buildStackScopedPermissions`/`buildEnvironmentScopedPermissions`), `AgentPool`, `OidcIssuer`, `TemplateSource`. These are the hosted TWINS of local owners (`DriftSchedule`↔`Drift.sweep`, `TtlSchedule`↔`Automation.ephemeral`, `Webhook`↔`doppler.Webhook`). Lands as a new `operate/cloud.ts` owner, gated on a Cloud backend `StackSpec` value — the "review stacks / deployment settings" leg of charter #10.

### [B4] `@pulumi/esc-sdk` — imperative config/environment plane, ZERO consumption
`EscApi` over `Configuration`: environment CRUD, open/read sessions, check-gated writes (`checkEnvironment`→`diagnostics`), `openAndReadEnvironment`, revisions/tags, `decryptEnvironment`. `[RULED]`: ESC is a projection DAG over stores (Doppler stays canonical), stack attachment is imperative-only (`Stack.addEnvironments`). The `EnvironmentDefinitionValues` projection (`pulumiConfig`/`environmentVariables`/`files`) is the config-composition contract. Lands as `operate/environment.ts` beside `secret.ts`, boundary-wrapped in `Effect.tryPromise` + `Option` + `DeployFault` triage.

### [B5] `@pulumiverse/acme` — CA-trusted cert lane, ZERO consumption
`Registration` + `Certificate` (CSR mode over the `tls` chain, `dnsChallenges:[{provider:"cloudflare", config}]` DNS-01, `minDaysRemaining`/`useRenewalInfo` ARI rotation). `secret.md` `Certs` is self-signed ONLY. README lists acme under IDENTITY_MATERIAL. This is the third cert lane — browser-trusted certs OUTSIDE a cluster (docker arm edge, bare-metal), complementing self-signed (mesh) and cert-manager (in-cluster). Lands as a third `Certs` lane in `secret.md` (CSR posture keeps keys in the `tls` owner).

### [B6] `@pulumi/eks` — aws-arm k8s escalation, ZERO consumption
`Cluster.kubeconfigJson → k8s.Provider` reuses the ENTIRE `kube/*` tier roster unchanged — promotes the `aws` arm from `awsx.ecs.FargateService` to a full k8s estate (CNPG/traffic/observe). `createOidcProvider:true` mints IRSA anchors; `ManagedNodeGroup` capacity; `authenticationMode:"API"` + `accessEntries`. The managed twin of `Bootstrap.kubeconfig`. Lands as an `aws`-arm variant cell in `provider.md` (a `workload` sub-choice: Fargate vs EKS).

### [B7] `@pulumi/cloudinit` — pre-SSH first-boot, ZERO consumption
`getConfigOutput` renders multi-part MIME user-data (`parts[]` with `contentType`/`mergeType`). Owns what happens BEFORE `StackSpec.Connection` is reachable (package install, user/key layout, daemon enablement) — the `Bootstrap` tier's SSH surface should be a cloud-init product, not a manual prerequisite. Lands in `provider.md` `Bootstrap` as a `userData` arg feeding the host-provisioning resource; `command.remote.Command` takes over post-boot.

### [B8] `@pulumi/synced-folder` + `@pulumi/github` — distribution + source-control legs, ZERO consumption
- `synced-folder.{S3BucketFolder,GoogleCloudFolder,AzureBlobFolder}` — static-frontend distribution (the UI folder's built web app → arm object cell → DNS/CDN). Charter #8 "FILESYSTEM + CLOUD ESCALATED."
- `github.{Repository,RepositoryEnvironment,RepositoryDeployKey,RepositoryWebhook,RepositoryRuleset,ActionsVariable}` — the bootstrap-axis source-control leg. `RepositoryEnvironment` is the shell `doppler.secretssync.GithubActions` (already in `_MIRRORS`) populates — secret.md has the sync but not the shell it writes into. Lands as a new `program/source.ts` or `operate/` owner.

---

## [C]-[CROSS-STACKING PLAYS THE CORPUS NEVER ATTEMPTS]

Package × package compositions with verified seams.

1. **`random.RandomShuffle` × workload/vpc placement** — `RandomShuffle({ inputs: azs, seed })` → `results` gives seed-stable AZ/replica spreading stable across `up`; feed `workload.md` replica placement and `awsx.ec2.Vpc` subnet spread. (`pulumi-random.md` [05] row.)
2. **`tls.getCertificateOutput` × cloudflare/foreign endpoint** — pin a foreign endpoint's chain at deploy time for mTLS trust the arm doesn't mint (`secret.md` `Certs` growth). Composes with the mesh CA for cross-boundary trust.
3. **`docker-build.Image.digest` cross-arm × `k8s` workload ref** — the k8s arm currently takes `spec.image` as a pre-built ref; the docker arm builds via `docker-build.Image`. A mixed stack builds once (`docker-build.Image` with `platforms:[amd64,arm64]` + `cacheFrom/cacheTo` registry) and pins the immutable `digest` into `Workload.Args.image` — one build, any runtime arm. `awsx.ecr.Image` bundles the same builder for the aws arm.
4. **`pulumiservice.Webhook` (drift filter) × `doppler.Webhook` (secret-change) → ONE evidence-delivery plane** — `[RULED]` §[B3] "a drift-filter webhook lands beside the Doppler secret-change webhook as one evidence-delivery law with two sources." Both deliver to a sink that runs `Drift.check` between sweep cycles — `secret.md` `Secrets.webhook` exists but is not correlated to a `pulumiservice.Webhook` `DriftDetected` filter.
5. **`cloudinit.getConfigOutput` × `command.remote.Command`** — first-boot user-data lays the SSH surface (`parts[]`), then `Bootstrap.remote.Command` installs the control plane; `[RULED]` split law "cloud-init owns first boot, command owns steady state."
6. **`eks.Cluster.kubeconfigJson` × `k8s.Provider` × whole `kube/*` roster** — the deepest play: one provider-seam swap promotes the aws arm to reuse `Workload`/`Traffic`/`Postgres`/`Lgtm`/`Boards` unchanged (§[B6]).
7. **`helm.v4 chart.resources` × `policy.validateStack` / `grafana` Boards** — feed the rendered child set to CrossGuard cross-resource validation, or to Boards to discover a chart-emitted Grafana. Currently `chart.resources` is never read.
8. **`policy.validateStackResourcesOfType` + `PolicyResource.propertyDependencies` × CNPG/Gateway invariants** — dependency-aware cross-resource rows: every CNPG `Cluster` has a `ScheduledBackup`, every Gateway `HTTPRoute` a `BackendTLSPolicy`, every `Database` an owning `managed.role`. `_networkFence` is the only cross-resource row today.
9. **`esc-sdk` × `Stack.addEnvironments` × Doppler dynamic-provider open** — ESC composes canonical Doppler material through dynamic-provider opens; `EnvironmentDefinition.imports` is the composition DAG, attached to a stack via `addEnvironments` (§[B3]/[B4]).
10. **`RandomUuid7.result` × `StackOutputs`** — time-ordered sortable deployment/tenant ids surfaced as outputs.
11. **`github.RepositoryEnvironment` × `doppler.secretssync.GithubActions`** — the mirror (already in `_MIRRORS`) writes INTO the `RepositoryEnvironment` shell whose name aligns with `StackSpec.doppler.config` — mirror + gate + stack speak one environment vocabulary (§[B8]).
12. **`command.local.runOutput` × `Bootstrap` triggers** — an unconditional host fact (kernel version, existing k3s token) threaded into the bootstrap `triggers` set instead of blind epoch-only re-runs.

---

## [D]-[GAP CAPABILITIES + PACKAGE/INTEGRATION SHAPE]

Candidates for the improver to weigh. Most close with ALREADY-ADMITTED packages or typed-CRD targets (crd2pulumi, no npm) — new npm is rare.

### [D1] Gateway API edge (typed target, crd2pulumi — no npm)
`[RULED]` §[B4]: replace/complement `networking/v1.Ingress` with Gateway API v1.6 `Gateway`/`HTTPRoute`/`GRPCRoute`/`TLSRoute` + `BackendTLSPolicy` + `ReferenceGrant`. Integration: `traffic.md` `_EDGE` becomes a `GatewayClass`+`Gateway` anchor; the tunnel/direct exposure routes become `HTTPRoute` rows. cert-manager ACME via the `gatewayHTTPRoute` solver (`config.enableGatewayAPI=true`), external-dns via native Gateway sources (`gateway-httproute`). Ingress stays the legacy fallback row.

### [D2] cert-manager in-cluster ACME (typed target, crd2pulumi + helm.v4)
`[RULED]` §[B4]: cert-manager v1.20.3 `Certificate`/`Issuer`/`ClusterIssuer` (`cert-manager.io/v1`), `Order`/`Challenge` (`acme.cert-manager.io/v1`). The in-cluster CA-trusted lane between self-signed (`Certs`) and cluster-external (`acme` §[B5]). Install via `helm.v4.Chart`, CRDs via crd2pulumi.

### [D3] external-dns (typed target, helm.v4 + crd2pulumi)
`[RULED]` §[B4]: external-dns v0.21.0 with native Gateway sources + `DNSEndpoint` CRD (`--source=crd`). Automates DNS from Gateway/HTTPRoute state — removes the manual `cloudflare.DnsRecord` per-hostname authoring in `traffic.md`.

### [D4] CNPG plugin-barman-cloud + ObjectStore CRD (typed target — MANDATORY per §[0.1])
Not optional: the in-tree path is a dead end. `plugin-barman-cloud` v0.13.0 + `barmancloud.cnpg.io/v1` `ObjectStore` CRD. Also CNPG `Pooler` (PgBouncer), `Publication`/`Subscription`, `DatabaseRole` (1.30 new), `ImageCatalog` — all typed targets the data plane's tenancy escalation (§[B1]) needs.

### [D5] ESC dynamic OIDC credentials (admitted `esc-sdk`/`pulumiservice.Environment`)
`[RULED]` §[B2]: ESC dynamic providers (`fn::open::aws-login` OIDC, `gcp-login`, Vault, 1Password) + Rotated Secrets (AWS IAM, Postgres/MySQL). Replaces static provider-credential fan-in with short-lived OIDC — the prepared cloud arms' credential path. `PULUMI_DISABLE_ESC_SSRF_PROTECTION` escape hatch for same-host providers (Apr 2026 SSRF hardening).

### [D6] Dynamic secrets / PKI engines — `@pulumi/vault` (NEW npm, mine-design-only)
`[RULED]` §[B7]: `@pulumi/vault` 7.10.0 is `mine-design-only` — admit ONLY on a concrete dynamic-DB-creds / PKI / transit feature. Distinct from Doppler/ESC static distribution. Weigh against ESC Rotated Secrets (§[D5]) which may subsume it.

### [D7] Identity provisioning — `@pulumi/keycloak` XOR `@pulumi/auth0` (NEW npm, mine-design-only)
`[RULED]` §[B7]: admit exactly ONE after the identity-architecture decision, never both. Only if the multi-tenant IdP path (§[B1]) earns it.

### [D8] Resilience-as-a-concept on the Automation driver (existing `effect` primitives)
Charter #6: `automation.md` `DeployFault` retries ONLY `concurrent` (state-lock). No circuit-breaker/hedge/bulkhead/adaptive-retry on the engine run or the `Drift.sweep` fleet loop. The `Automation.run`/`reconcile`/`Drift.sweep` owners should carry internal `Schedule` algebra (exponential backoff + jitter on `concurrent`, a breaker on repeated `alien`/`command` faults per stack, bounded concurrency already via `withConcurrency`) so a future consumer composes capability, not plumbing. `pulumi.recover` (§[A]) on `Output` reads. This is realizing the charter's "resilience rides EVERY egress owner internally" on the deploy plane's egress.

---

## [E]-[PER-PAGE INTEGRATION MAP]

What the improver executes, page by page (dependency order).

### `program/spec.md`
- Add `tenancy` axis to `_Profile`/`StackSpec` (§[B1]): `{ mode: "single" | "namespace-capsule" | "vcluster", pgTier: "shared-rls" | "db-per-tenant" | "cluster-per-tenant" }` — defaults-total, drives the tenant escalation as data.
- Add a `backend` coordinate (`self-managed` | `cloud`) so `pulumiservice` (§[B3]) and `RemoteWorkspace` gate on spec data.
- `StackOutputs`: add `RandomUuid7`-sourced deployment id plane; add tenant-scoped output planes if tenancy present.
- `Tier` base: consider `ResourceHook`/`ErrorHook` binding (§[A]) as an internal capability all tiers inherit.

### `program/provider.md`
- `Bootstrap`: wire `cloudinit.getConfigOutput` `userData` (§[B7]); add `ConnectionArgs.proxy` bastion + `hostKey` pinning + `perDialTimeout`/`dialErrorLimit` (§[A]); add `local.runOutput` host-fact triggers (§[C12]).
- aws arm: realize past the awsx-Fargate stub — add the EKS variant cell (`eks.Cluster.kubeconfigJson → k8s.Provider`, §[B6]) so the kube roster reuses; realize the `_map` cells (rds/acm/route53/iam) the census flags catalogued-only.
- gcp/cloudflare arms: realize the stub `_map` columns (Cloud SQL/GCS/Secret-Manager-mirror/Cloud-DNS for gcp; R2/Workers/Access/TotalTls for cloudflare, §[A] cloudflare rows).
- Add ESC dynamic-OIDC credential path (§[D5]) as the prepared-arm credential option.

### `program/automation.md`
- `StackReference` cross-stack reads (§[A]) for the multi-tenant platform↔tenant seam.
- Revive `Stack.import` (batch adoption) and `Stack.addEnvironments`/`listEnvironments` (ESC attach) from demoted to realized.
- Resilience: internal `Schedule` algebra + breaker on the `run`/`reconcile` owners (§[D8]).
- `Stack.history`/`getTag`/`setTag` for fleet audit/labelling.
- `RemoteWorkspace` prepared row realized when `backend:cloud`.

### `operate/secret.md`
- `_MIRRORS`: add `AwsParameterStore`/`TerraformCloud`/`Circleci`/`Flyio` rows (§[A]).
- `ServiceAccount`/`ServiceAccountToken` + `Group`/`ProjectRole`/`projectmember.*` for multi-tenant RBAC (§[B1]).
- `Secrets.webhook`: add `authentication`/`payload`; correlate with `pulumiservice.Webhook` drift filter (§[C4]).
- `Certs`: add the third CA-trusted lane via `@pulumiverse/acme` (§[B5]); wire `getCertificateOutput` foreign-chain pin (§[C2]); `bcryptHash` digest projection; `SelfSignedCert.maxPathLength`/key-id CA semantics.
- New sibling `operate/environment.ts`: `esc-sdk` `EscApi` config plane (§[B4]).

### `operate/observe.md`
- **Realize `_alertRows`/`_sloRows`** (declared stubs) — the `Alert.Spec`→`alerting.RuleGroup`/`ContactPoint`/`NotificationPolicy`/`slo.Slo` compile (§[A]).
- `oss.ServiceAccount`/`ServiceAccountToken` machine identity over `admin:password`.
- `oss.Organization` per tenant (§[B1]); `alerting.MuteTiming`/`MessageTemplate` paging.
- `helm.v4` `verify`/`keyring` provenance; read `chart.resources` for dashboard discovery (§[C7]).
- `Provider.retryStatusCodes`/`retryWait` resilience tuning.

### `operate/policy.md`
- `validateRemediateResourceOfType` collapse (§[A]); `PolicyComplianceFramework` on rows (SOC2/CIS); `propertyDependencies` cross-resource rows (§[C8]).
- New rows: prepared-arm invariants (bucket retention, IAM floor), Gateway `BackendTLSPolicy` presence, CNPG `ScheduledBackup` presence, tenant-isolation NetworkPolicy.
- `Drift`: wire the `postgresql.getSchemas`/`getTables` drift read-back (§[A]); correlate the `pulumiservice.Webhook` drift filter as an event-shaped `check` trigger (§[C4]).
- New `operate/reconcile.ts` or `Drift` sibling: PKO in-cluster reconcile loop (§[B2]).
- New `operate/cloud.ts`: `pulumiservice` control plane — deployment settings, review stacks, schedules, RBAC (§[B3]), gated on `backend:cloud`.

### `kube/workload.md`
- Realize `rbac/v1.Role`+`RoleBinding` (identity cell is 2/3 unrealized, §[A]).
- HPA (`autoscaling`) + `PodDisruptionBudget` on the `fleet` scale row; `StorageClass` selection.
- `RandomShuffle` seed-stable replica/AZ placement (§[C1]).

### `kube/traffic.md`
- **Gateway API migration** (§[D1]): `Gateway`/`HTTPRoute`/`BackendTLSPolicy` typed CRDs (crd2pulumi), Ingress as legacy fallback.
- external-dns (§[D3]) replaces manual per-hostname `DnsRecord` authoring.
- `cloudflare.Ruleset` WAF + `ZeroTrustAccessGroup` (§[A]); `CustomHostname` per tenant (§[B1]).
- cert-manager gatewayHTTPRoute solver (§[D2]).

### `kube/data.md`
- **MANDATORY**: rewrite backup to `plugin-barman-cloud` + `ObjectStore` CRD; kill the in-tree `barmanObjectStore` (§[0.1]/[D4]).
- crd2pulumi typed CNPG SDK replaces raw `CustomResource<any>` (§[0.4]).
- **Tenant escalation** (§[B1]): the three PG tiers — shared-RLS / N-`Database`-CRs / N-`Cluster`s — driven by `StackSpec.tenancy.pgTier`.
- CNPG `Pooler` (PgBouncer), `Publication`/`Subscription` logical replication, `DatabaseRole` (1.30); `postgresql.getSchemas` drift feed on the docker cell.
- New `kube/tenant.ts` (new page earned): Capsule `Tenant` / vcluster typed-CRD owner (§[B1]).

---

## [F]-[NOTES / FLAGS]

- **Doppler version**: `pulumiverse-doppler.md` header says `installed: 0.9.0`, but the census reports it as a 132-line deep catalog and `admissions.md` does not pin it (pre-existing). `ServiceAccount`/sync-target members are catalogued, so treat them as available; verify the pin during application if the improver adds `ServiceAccount` rows.
- **`crd2pulumi` is NOT an npm dependency** (`admissions.md` rejected row) — it is a dev-time Go CLI generating committed TS SDKs; regenerate on operator bumps. Every typed-CRD gap (§[D1]-[D4], §[B1]) uses it, not a manifest pin.
- **New npm the improver may weigh**: `@pulumi/vault` (§[D6], on a dynamic-secrets feature), `@pulumi/keycloak` XOR `@pulumi/auth0` (§[D7], on the IdP decision) — both `mine-design-only`, admit only on a concrete earned feature, never speculatively. Everything else the gaps demand is already admitted.
- **Charter-scale reminder**: the folder is currently a single-app, self-signed, Ingress-based, self-managed-backend deploy plane. The charter demands a multi-tenant, CA-trusted, Gateway-API, PKO-reconciled, Cloud-control-plane-capable product at 2-3x app depth. The capability to reach that is ~90% already admitted and unrealized — this is a REALIZATION campaign, not a package-hunting one.
