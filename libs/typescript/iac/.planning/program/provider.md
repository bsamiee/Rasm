# [IAC_PROVIDER]

Provider dispatch and the service surface as ONE owner keyed by one union: the `_map` equivalence table and the `_ARMS` handler record both key on `StackSpec.Arm`, so the capability-by-arm data and the arm program that realizes it live on one page and review pressure holds them adjacent — the map is the audit surface capability reads, the record is the construction it describes. Rows are capabilities, columns are arms, cells name the exact resource-family spelling, and a hole is honest absence the reader gets as `Option`. Each arm is a total function from the spec (plus the Effect-resolved host material and deploy-time pins) to a `PulumiFn`: it proves EVERY spec-derived coordinate its tiers require on the rail as typed `DeployFault`s before the `PulumiFn` is entered — `_proven` for connection-plus-key, `_coord` for each coordinate, `_staged` assembling the traffic edge as a tagged case — so no tier ever throws for a value the spec could have proven. The `_estate` composition is the single k8s-estate builder: the selfhosted arm feeds it a `Bootstrap` kubeconfig provider, the aws arm's `cluster` compute row feeds it an `eks.Cluster.kubeconfigJson` provider, and the whole workload/data/traffic/observe/tenant roster rides either plane unchanged — promoting a cloud to a k8s estate is a provider-seam swap, never a tier rewrite. `Bootstrap` is the metal row hardened end to end: `cloudinit`-rendered first boot lays the SSH surface before the connection is reachable, host-key pinning and bastion proxy and dial budgets ride the connection, `local.runOutput` host facts join the trigger set, and the control plane installs through one `remote.Command` whose stdout is the kubeconfig. Adding a cloud is one record row plus one map column; finalizing one is a `StackSpec` value, never a lib edit. The module is `iac/src/program/provider.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                               | [PUBLIC]    |
| :-----: | :------------------ | :-------------------------------------------------------------------- | :---------- |
|  [01]   | `EQUIVALENCE_MAP`   | the capability-by-arm table and its `Option`-lifted projections       | `Dispatch`  |
|  [02]   | `ARM_CONTRACT`      | the material read, pins, the coordinate proofs, the exhaustive record | `Dispatch`  |
|  [03]   | `CLUSTER_BOOTSTRAP` | first boot, staging, install, connection hardening, kubeconfig egress | `Bootstrap` |
|  [04]   | `ARM_PROGRAMS`      | the shared k8s estate builder and the five arm bodies                 | `Dispatch`  |

## [2]-[EQUIVALENCE_MAP]

[EQUIVALENCE_MAP]:
- Owner: the interior `_capabilities` key tuple anchoring row order, the `_map` table carrying per-arm cells as exact-optional keys (a hole is an omitted key, never a sentinel), and the two projections riding the exported owner: `cell(capability, arm)` lifts the unproven cell read to `Option`, `column(arm)` folds an arm's realized subset in row order. The reads ride `_cells`, the table widened to `Dispatch.Cell` rows — a declared-key access on the literal union demands the key on every row, so the bracket read is index trust lifted at the seam while `_map` keeps its literals.
- Law: cells are family spellings, not mechanics — a cell names the resource classes (`gcp.container.Cluster + NodePool`) or the owning row (`helm minio-continuation | ceph-rgw`), and the page that constructs them is the mechanics owner; the map is what arm programs, drift reports, and capability audits read.
- Law: the object row admits only conditional-put-conforming engines — the self-host cells name the maintained MinIO continuation and Ceph RGW, the managed cells name S3, R2, and GCS; the CRDT-metadata engine that cannot honor `If-None-Match: *` has no cell anywhere, because the data plane's write-once identity algebra is non-negotiable and the refusal is `data`'s engine table read as deployment law.
- Law: the canonical secret owner spans every column — the `secret` row is Doppler on all five arms, and a cloud secret manager is reachable only as a mirror (`secretssync.<Target>` where the Doppler provider ships the destination, an in-graph write fed by the fan-in read where it does not), so no arm ever grows a second secret source of truth.
- Law: a prepared column's filled cells are its finalization contract — the `gcp` column names GKE, Cloud SQL, GCS, Cloud DNS, and the Secret Manager mirror against the same capability rows the primary arm realizes, so finalizing means instantiating the named subset with the `StackSpec` value; the dormant remainder of a provider SDK is unreachable by construction.
- Entry: `Dispatch.column(spec.target)` inside an arm; `Dispatch.cell("data", "aws")` for a point read.
- Growth: a new capability is one `_capabilities` entry plus one `_map` row; a new arm is one cell per realized row under the new column key.
- Boundary: kube-row mechanics are `kube/*`; the object/data engine choices are `StackSpec.profile` values; the tenant row's mechanics are `kube/tenant.md`; the in-cluster reconcile row's mechanics are `operate/policy.md`; cross-stack output reads ride `StackReference` inside the tenant seam `kube/tenant.md` owns.
- Packages: `effect` (`Array`, `Option`, `Record`); `./spec.ts` (`StackSpec`).

```typescript
import { Array, Option, Record } from "effect"
import type { StackSpec } from "./spec.ts"

const _capabilities = [
  "bootstrap", "workload", "data", "object", "fanout", "cert", "dns",
  "ingress", "secret", "registry", "network", "identity", "observe",
  "tenant", "distribution", "reconcile",
] as const

const _map = {
  bootstrap: {
    "selfhosted-k8s": "cloudinit.getConfigOutput first boot + command.remote.Command + CopyToRemote",
    "selfhosted-docker": "cloudinit.getConfigOutput first boot + command.remote.Command",
  },
  workload: {
    "selfhosted-k8s": "kubernetes.apps/v1.Deployment + core/v1.Service",
    "selfhosted-docker": "docker.Container",
    aws: "awsx.ecs.FargateService | eks.Cluster + eks.ManagedNodeGroup (compute: cluster)",
    gcp: "gcp.container.Cluster + gcp.container.NodePool",
    cloudflare: "cloudflare.WorkersScript + cloudflare.WorkersRoute",
  },
  data: {
    "selfhosted-k8s": "cnpg Cluster CR (managed roles, plugin-barman-cloud) + Database CR + ensure Job",
    "selfhosted-docker": "docker.Container(postgres) + postgresql.Database/Role/Grant/Extension",
    aws: "aws.rds.Cluster",
    gcp: "gcp.sql.DatabaseInstance + gcp.sql.Database + gcp.sql.User",
    cloudflare: "cloudflare.D1Database",
  },
  object: {
    "selfhosted-k8s": "helm minio-continuation | ceph-rgw",
    "selfhosted-docker": "docker.Container(minio-continuation)",
    aws: "aws.s3.BucketV2",
    gcp: "gcp.storage.Bucket + gcp.storage.BucketIAMMember",
    cloudflare: "cloudflare.R2Bucket",
  },
  fanout: {
    "selfhosted-k8s": "helm nats (jetstream + websocket)",
    "selfhosted-docker": "docker.Container(nats)",
  },
  cert: {
    "selfhosted-k8s": "tls chain -> core/v1.Secret(kubernetes.io/tls) | acme.Certificate (cluster-external)",
    "selfhosted-docker": "acme.Registration + acme.Certificate (CSR over tls chain)",
    aws: "aws.acm.Certificate",
    gcp: "gcp.certificatemanager.Certificate",
    cloudflare: "cloudflare.OriginCaCertificate + cloudflare.TotalTls",
  },
  dns: {
    "selfhosted-k8s": "cloudflare.DnsRecord",
    "selfhosted-docker": "cloudflare.DnsRecord",
    aws: "aws.route53.Zone + aws.route53.Record | cloudflare.DnsRecord (cluster estate)",
    gcp: "gcp.dns.ManagedZone + gcp.dns.RecordSet",
    cloudflare: "cloudflare.Zone + cloudflare.DnsRecord",
  },
  ingress: {
    "selfhosted-k8s": "kubernetes.networking/v1.Ingress | cloudflare.ZeroTrustTunnelCloudflared",
    "selfhosted-docker": "docker.Container ports",
    aws: "awsx.lb.ApplicationLoadBalancer",
    gcp: "gcp.compute.GlobalAddress + gcp.compute.URLMap",
    cloudflare: "cloudflare.ZeroTrustTunnelCloudflared + cloudflare.ZeroTrustAccessApplication",
  },
  secret: {
    "selfhosted-k8s": "doppler canonical",
    "selfhosted-docker": "doppler canonical",
    aws: "doppler canonical + secretssync.AwsSecretsManager mirror",
    gcp: "doppler canonical + gcp.secretmanager mirror",
    cloudflare: "doppler canonical",
  },
  registry: {
    "selfhosted-k8s": "docker-build.Image registry export",
    "selfhosted-docker": "docker-build.Image + docker.RegistryImage",
    aws: "awsx.ecr.Repository + awsx.ecr.Image",
    gcp: "gcp.artifactregistry.Repository",
  },
  network: {
    "selfhosted-k8s": "kubernetes.networking/v1.NetworkPolicy",
    "selfhosted-docker": "docker.Network + docker.Volume",
    aws: "awsx.ec2.Vpc",
    gcp: "gcp.compute.Network + gcp.compute.Subnetwork + gcp.compute.Firewall",
  },
  identity: {
    "selfhosted-k8s": "core/v1.ServiceAccount + rbac/v1.Role + rbac/v1.RoleBinding",
    aws: "aws.iam.Role + aws.iam.Policy + aws.iam.RolePolicyAttachment | eks IRSA (createOidcProvider)",
    gcp: "gcp.serviceaccount.Account + gcp.projects.IAMMember",
  },
  observe: {
    "selfhosted-k8s": "helm lgtm + otel collector + grafana apply",
    "selfhosted-docker": "collector container + grafana apply",
  },
  tenant: {
    "selfhosted-k8s": "capsule Tenant CR | vcluster chart (crd2pulumi typed)",
    aws: "kube tenant tier over the EKS estate (compute: cluster)",
  },
  distribution: {
    aws: "synced-folder.S3BucketFolder over aws.s3.BucketV2",
    gcp: "synced-folder.GoogleCloudFolder over gcp.storage.Bucket",
    cloudflare: "cloudflare.PagesProject | R2Bucket static origin",
  },
  reconcile: {
    "selfhosted-k8s": "PKO Stack/Program CR (in-cluster reconcile loop)",
  },
} as const

declare namespace Dispatch {
  type Capability = (typeof _capabilities)[number]
  type Column = ReadonlyArray<readonly [Dispatch.Capability, string]>
  type Cell = Partial<Record.ReadonlyRecord<StackSpec.Arm, string>>
  type _Rows<T extends Record.ReadonlyRecord<Capability, Cell> = typeof _map> = T
  type _Keys<K extends keyof typeof _map = Capability> = K
}

const _cells: Record.ReadonlyRecord<Dispatch.Capability, Dispatch.Cell> = _map
```

## [3]-[ARM_CONTRACT]

[ARM_CONTRACT]:
- Owner: the arm signature and the record law — `material` is the one deploy-host Config read the arms share (`IAC_SSH_KEY` as an optional `Redacted`, resolved under `doppler run`), `program(spec, material, pins)` is the generic indexed call over `_ARMS`, and the record's mapped annotation `{ readonly [K in StackSpec.Arm]: Dispatch.Arm }` is the exhaustiveness proof — a `StackSpec.arms` entry with no row fails compilation at the record.
- Law: arms prove, never assume — `_coord` lifts any spec `Option` onto the rail minting an `input` fault naming the coordinate, `_proven` zips connection and key, and `_staged` proves the entire traffic-edge coordinate set (domain, zone, and the exposure row's own demand: the connection host under `direct`, the account under `tunnel`) into one `Traffic.Edge` tagged case; no arm body or tier constructor ever meets an unproven `Option`, and a construction-time `RunError` for a spec-derivable value is the named defect this proof family deletes.
- Law: pins are a parameter, never a module read — `Dispatch.Pins` carries the deploy-time facts the spec does not (chart and operator versions, the extension-image ref, the cloudflared connector image, the install script and its first-boot parts, the host-fact probe commands, the managed-capacity and managed-data rows for the prepared arms, the build context, the ensure-DDL roster the data plane publishes, the encoded boards and alert specs from the core observe suite); the app root resolves them from its own config and suite call, so ingress is parameterized end to end and the lib hardcodes no version anywhere.
- Law: one provider seam per arm — the arm constructs its provider (kubeconfig-bound `k8s.Provider`, `ssh://` `docker.Provider`, credentialed cloud provider) exactly once and threads it through tier options; per-resource providers are the named defect, and the credential arrives from `Secrets.read` in-graph or the ambient `doppler run` env, never a literal.
- Law: the `PulumiFn` body is the deploy plane's program seam — a promise-returning composition of tier constructors bound to consts and one returned outputs record; the platform owns that shape, and everything the arm computes before entering it stays on the rail.
- Entry: `Effect.flatMap(Dispatch.material, (material) => Dispatch.program(spec, material, pins))` then `Automation.stack(spec, program)`.
- Growth: one record row plus one map column per cloud; a new shared deploy-time fact is one `Pins` field, a new shared secret fact is one `material` field; a new spec coordinate a tier requires is one `_coord` call in its arm's proof.
- Boundary: the run and receipt are `automation.md`'s; outputs keys are `spec.md`'s contract.
- Packages: `effect` (`Config`, `Effect`, `Option`, `Redacted`); `./spec.ts` (`StackSpec`); `./automation.ts` (`DeployFault`); `../kube/traffic.ts` (`Traffic.Edge`).

```typescript
import type { PulumiFn } from "@pulumi/pulumi/automation"
import { Config, Effect, Option, Redacted } from "effect"
import type { Alert, DashboardModel, Slo } from "@rasm/ts/core"
import { DeployFault } from "./automation.ts"
import type { StackSpec } from "./spec.ts"

declare namespace Dispatch {
  type Material = { readonly sshKey: Option.Option<Redacted.Redacted<string>> }
  type Arm = (spec: StackSpec, material: Material, pins: Pins) => Effect.Effect<PulumiFn, DeployFault>
  type App = { readonly image: string; readonly edge: Traffic.Edge }
  type Pins = {
    readonly install: string
    readonly firstBoot: ReadonlyArray<{ readonly content: string; readonly contentType?: string; readonly filename?: string; readonly mergeType?: string }>
    readonly facts: ReadonlyArray<string>
    readonly pgImage: string
    readonly operator: string
    readonly barman: string
    readonly object: string
    readonly nats: string
    readonly lgtm: string
    readonly collector: string
    readonly cloudflared: string
    readonly capsule: string
    readonly vcluster: string
    readonly port: number
    readonly context: string
    readonly nodes: { readonly instanceType: string; readonly min: number; readonly max: number }
    readonly managedData: { readonly engine: string; readonly tier: string }
    readonly ensures: ReadonlyArray<string>
    readonly boards: ReadonlyArray<typeof DashboardModel.Encoded>
    readonly alerts: ReadonlyArray<Alert.Spec>
    readonly objectives: ReadonlyArray<Slo.Objective>
    readonly contacts: Partial<Record<"page" | "ticket", { readonly webhook: string }>>
  }
}

const _material = Config.unwrap({
  sshKey: Config.option(Config.redacted("IAC_SSH_KEY")),
})

const _input = (spec: StackSpec, detail: string): DeployFault =>
  new DeployFault({ reason: "input", stack: spec.name, detail })

const _coord = <A>(spec: StackSpec, held: Option.Option<A>, name: string): Effect.Effect<A, DeployFault> =>
  Effect.mapError(held, () => _input(spec, `<missing-${name}>`))

const _proven = (spec: StackSpec, material: Dispatch.Material): Effect.Effect<{
  readonly connection: StackSpec.Connection
  readonly key: Redacted.Redacted<string>
}, DeployFault> =>
  Option.zipWith(spec.connection, material.sshKey, (connection, key) => ({ connection, key })).pipe(
    Effect.mapError(() => _input(spec, "<missing-connection-or-key>")),
  )

const _staged = (spec: StackSpec): Effect.Effect<Option.Option<Dispatch.App>, DeployFault> =>
  Option.match(spec.image, {
    onNone: () => Effect.succeedNone,
    onSome: (image) =>
      Effect.all([_coord(spec, spec.domain, "domain"), _coord(spec, spec.zone, "zone")]).pipe(
        Effect.flatMap(([domain, zone]) =>
          spec.profile.exposure === "direct"
            ? Effect.map(_coord(spec, spec.connection, "connection"), (connection) =>
                Traffic.Edge.Direct({ domain, zone, address: connection.host }))
            : Effect.map(_coord(spec, spec.account, "account"), (account) =>
                Traffic.Edge.Tunnel({ domain, zone, account }))),
        Effect.map((edge) => Option.some({ image, edge })),
      ),
  })
```

## [4]-[CLUSTER_BOOTSTRAP]

[CLUSTER_BOOTSTRAP]:
- Owner: `Bootstrap`, the tier that turns owned metal into a cluster — `Bootstrap.firstBoot(parts, encoding?)` renders the multi-part MIME user-data a host-provisioning resource consumes as its pre-SSH product, staged assets ride `remote.CopyToRemote` (rendered install artifacts as `Asset`/`Archive` values, never checked-in paths), the control plane installs through one `remote.Command` whose CRUD slots own install (`create`) and teardown (`delete`), and `kubeconfig` egresses as the secret-tracked stdout the `@pulumi/kubernetes` `Provider` binds.
- Law: cloud-init owns first boot, `command` owns steady state — `firstBoot` composes `cloudinit.getConfigOutput` over ordered typed parts (one `text/cloud-config` declarative part plus `text/x-shellscript` steps, `mergeType` on composed cloud-config parts), the rendered body lays the SSH surface (users, keys, packages, daemon) the `Connection` coordinates then reach, and part content carries coordinates and installers only — user-data is metadata-endpoint-readable, so credential material inside a part is the named defect; a first-boot step re-run over SSH, or an SSH step folded into user-data, is the same defect in two directions.
- Law: the connection is coordinates plus injected material, hardened — `StackSpec.Connection` supplies host/user/port, the PEM key arrives as a `pulumi.Input<string>` already secret-tracked from `Dispatch.material`, `hostKey` pins the host's public key so a MITM re-key fails the dial instead of silently trusting, `proxy` is the bastion hop as one `ProxyConnectionArgs` row on the same connection, `perDialTimeout`/`dialErrorLimit` bound the dial budget as data, and `logging: "none"` gates every credential-bearing step so captured output never echoes key material.
- Law: re-run is trigger-driven and fact-aware — the `triggers` list carries the spec `epoch`, the staged-asset references, and the `local.runOutput` host facts (`facts` rows: kernel version, an existing k3s token, a daemon fingerprint — each an unconditional deploy-host read threading the graph), so a bootstrap re-runs exactly when its real inputs change and never by blind epoch-only replacement.
- Law: the takeover boundary is absolute — after the kubeconfig exists, every workload is a typed `@pulumi/kubernetes` resource; a shell command that duplicates a typed provider resource is the named defect, and `command` survives only for bare-metal mutation no typed provider owns; an unconditional host fact is `local.runOutput` when it threads the graph, `local.run` for an eager read inside the program body.
- Entry: `new Bootstrap("plane", { connection, key, epoch, install, facts, hostKey, proxy }, opts)` inside the selfhosted arms; `bootstrap.kubeconfig` feeds `new k8s.Provider(...)`; `Bootstrap.firstBoot(pins.firstBoot)` wherever a host-provisioning resource takes user-data.
- Growth: a new host mutation is one `remote.Command` row inside this tier with its own triggers; a new staged artifact is one `assets` entry; a new re-run discriminant is one `facts` row.
- Boundary: the install script and first-boot part content are app data handed in as pins; the k8s provider construction is the arm body's; VPS provisioning itself (the resource consuming `firstBoot`) is the owning cloud arm's when an estate provisions rather than adopts its metal.
- Packages: `@pulumi/command` (`remote.Command`, `remote.CopyToRemote`, `local.run`, `local.runOutput`, `types.input.remote.ProxyConnectionArgs`); `@pulumi/cloudinit` (`getConfigOutput`); `@pulumi/pulumi` (`Output`, `secret`, `asset`); `./spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as cloudinit from "@pulumi/cloudinit"
import * as command from "@pulumi/command"
import * as pulumi from "@pulumi/pulumi"
import { Tier } from "./spec.ts"

declare namespace Bootstrap {
  type Part = {
    readonly content: pulumi.Input<string>
    readonly contentType?: string
    readonly filename?: string
    readonly mergeType?: string
  }
  type Args = {
    readonly connection: StackSpec.Connection
    readonly key: pulumi.Input<string>
    readonly epoch: string
    readonly install: pulumi.Input<string>
    readonly remove?: pulumi.Input<string>
    readonly facts?: ReadonlyArray<string>
    readonly hostKey?: pulumi.Input<string>
    readonly proxy?: command.types.input.remote.ProxyConnectionArgs
    readonly dial?: { readonly perDialTimeout: number; readonly dialErrorLimit: number }
    readonly assets?: ReadonlyArray<{
      readonly source: pulumi.asset.Asset | pulumi.asset.Archive
      readonly remotePath: string
    }>
  }
}

class Bootstrap extends Tier {
  static readonly firstBoot = (
    parts: ReadonlyArray<Bootstrap.Part>,
    encoding?: { readonly gzip: boolean; readonly base64: boolean },
  ): pulumi.Output<string> =>
    cloudinit.getConfigOutput({
      gzip: encoding?.gzip ?? false,
      base64Encode: encoding?.base64 ?? false,
      parts: [...parts],
    }).rendered
  readonly kubeconfig: pulumi.Output<string>
  constructor(name: string, args: Bootstrap.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Bootstrap", name, opts)
    const connection = {
      host: args.connection.host,
      user: args.connection.user,
      port: args.connection.port,
      privateKey: args.key,
      perDialTimeout: args.dial?.perDialTimeout ?? 15,
      dialErrorLimit: args.dial?.dialErrorLimit ?? 10,
      ...(args.hostKey !== undefined && { hostKey: args.hostKey }),
      ...(args.proxy !== undefined && { proxy: args.proxy }),
    }
    const facts = (args.facts ?? []).map((probe, rank) =>
      command.local.runOutput({ command: probe }).stdout.apply((fact) => `${rank}:${fact}`))
    const staged = (args.assets ?? []).map((asset, rank) =>
      new command.remote.CopyToRemote(`${name}-asset-${rank}`, {
        connection,
        source: asset.source,
        remotePath: asset.remotePath,
        triggers: [args.epoch],
      }, this.child()))
    const plane = new command.remote.Command(`${name}-plane`, {
      connection,
      create: args.install,
      ...(args.remove !== undefined && { delete: args.remove }),
      triggers: [args.epoch, ...facts, ...staged.map((copy) => copy.remotePath)],
      logging: "none",
    }, this.child({ dependsOn: staged }))
    this.kubeconfig = pulumi.secret(plane.stdout)
    this.seal({ kubeconfig: this.kubeconfig })
  }
}
```

## [5]-[ARM_PROGRAMS]

[ARM_PROGRAMS]:
- Law: `_estate` is the one k8s-estate composition — namespace → `Secrets` (generated entries: `DB_ADMIN_PASSWORD`, `DB_PASSWORD`, `OBJECT_USER`, `OBJECT_PASSWORD`, `GRAFANA_PASSWORD`; `CLOUDFLARE_API_TOKEN` pre-exists on the app's config) → `ObjectStore` → `Nats` → `Postgres` (the admin and app credentials as two distinct reads) → `Lgtm` → `Boards` → `Tenants` when the tenancy mode escalates past `single` → the `RandomUuid7` deployment identity → `Workload.token` → optional `Workload` whose live-`Output` env pairs ride `StackOutputs.pairsOf` with the `pulumi.output(value).apply(String)` renderer — the same flatten the decoded getter rides — → one `Certs.root` CA → `Traffic` over the workload service with the issuance capability and the proven `Edge` case injected; it returns every realized `StackOutputs` plane, `deploy` included. Both k8s-plane sources feed it: the selfhosted arm's `Bootstrap.kubeconfig` and the aws arm's `eks.Cluster.kubeconfigJson`, so the entire tier roster is plane-agnostic by construction.
- Law: the app image is one buildx product — the docker arm and any registry cell build through `docker-build.Image` with `push: true`, the immutable `ref`/`digest` pinning every runtime; `platforms` rows make the build multi-arch, `cacheFrom`/`cacheTo` registry rows reuse layers across runs, and by-value `secrets` bind Doppler outputs so no build credential touches disk. The fastcdc wasm artifact is a build-stage product of this same image — a rust stage runs `wasm-pack build` over the pinned `fastcdc` crate and the runtime stage copies the pkg — so the chunking artifact ships inside the image digest and no second artifact pipeline exists.
- Law: the docker arm is bootstrap-plus-build-plus-runtime — the daemon `Bootstrap` realizes the column's bootstrap cell, the `ssh://` `docker.Provider` binds the proven connection's own `ssh` projection (the `Connection` class getter — no hand-built URL) with `dependsOn` the daemon so the first `up` cannot race the install, and the app `docker.Container` pins the built digest with the runtime fully wired at the catalogued nested spellings: `ports` rows, `networksAdvanced` binding the `docker.Network` by its `name` output, `volumes` binding the `docker.Volume` by `volumeName`/`containerPath`; the arm publishes no output plane it does not realize.
- Law: the aws arm dispatches its compute posture as data — `_AWS` is a handler record keyed by `StackSpec.Profile["compute"]`: the `serverless` row realizes VPC → ECR build → Fargate behind an ALB with the S3 object cell; the `cluster` row escalates to `eks.Cluster` (`authenticationMode: "API"`, `createOidcProvider: true` for IRSA, `skipDefaultNodeGroup: true`) with one `ManagedNodeGroup` sized from `pins.nodes`, binds `kubeconfigJson` into the arm's one `k8s.Provider` seam, and reuses `_estate` whole — the managed twin of `Bootstrap.kubeconfig`, one seam swap and zero tier edits.
- Law: the gcp arm realizes its finalization contract — the provider binds `credentials` from the `GCP_CREDENTIALS` fan-in read, the GKE anchor stands, the object cell is `gcp.storage.Bucket` (uniform bucket-level access), and the data cell is `gcp.sql.DatabaseInstance` + `Database` + `User` with the engine tag and machine tier arriving as `pins.managedData`; the cloudflare arm binds `apiToken` from the fan-in, realizes the `R2Bucket` object cell against the proven `account` and the dns cell against the proven `zone`; each returns exactly the planes it realizes.
- Law: every arm funds the boards — the encoded models and alert specs enter as pins where the arm realizes an observe cell; an arm without the observe cell returns no `grafana` plane and drops nothing silently.
- Growth: promoting a prepared arm is one realizer body or one `_AWS`-style posture row; a new cloud is one record row plus one map column.
- Boundary: tier mechanics live on the tier pages; the declared realizers' argument catalogues are the standing research items on the provider `.api` files.
- Packages: `@pulumi/kubernetes`, `@pulumi/eks`, `@pulumi/docker`, `@pulumi/docker-build`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/gcp`, `@pulumi/cloudflare`, `@pulumi/random` (providers + composed classes); every folder tier.

```typescript
import * as aws from "@pulumi/aws"
import * as awsx from "@pulumi/awsx"
import * as cloudflare from "@pulumi/cloudflare"
import * as docker from "@pulumi/docker"
import * as dockerBuild from "@pulumi/docker-build"
import * as eks from "@pulumi/eks"
import * as gcp from "@pulumi/gcp"
import * as k8s from "@pulumi/kubernetes"
import * as random from "@pulumi/random"
import { Nats, ObjectStore, Postgres } from "../kube/data.ts"
import { Tenants } from "../kube/tenant.ts"
import { Traffic } from "../kube/traffic.ts"
import { Workload } from "../kube/workload.ts"
import { Boards, Lgtm } from "../operate/observe.ts"
import { Certs, Secrets } from "../operate/secret.ts"
import { StackOutputs } from "./spec.ts"

const _estate = (
  spec: StackSpec,
  pins: Dispatch.Pins,
  provider: k8s.Provider,
  app: Option.Option<Dispatch.App>,
): Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, pulumi.Input<string | number>>> => {
  const bound = { providers: [provider] }
  const ns = new k8s.core.v1.Namespace(spec.name, { metadata: { name: spec.name } }, { provider })
  const secrets = new Secrets("secrets", {
    spec,
    entries: {
      DB_ADMIN_PASSWORD: { generate: {} },
      DB_PASSWORD: { generate: {} },
      OBJECT_USER: { generate: { special: false, length: 20 } },
      OBJECT_PASSWORD: { generate: {} },
      GRAFANA_PASSWORD: { generate: {} },
    },
  })
  const objects = new ObjectStore("objects", {
    spec,
    namespace: ns.metadata.name,
    version: pins.object,
    auth: { user: secrets.read("OBJECT_USER"), password: secrets.read("OBJECT_PASSWORD") },
  }, bound)
  const fanout = new Nats("fanout", { spec, namespace: ns.metadata.name, version: pins.nats }, bound)
  const data = new Postgres("data", {
    spec,
    namespace: ns.metadata.name,
    image: pins.pgImage,
    operatorVersion: pins.operator,
    barmanVersion: pins.barman,
    objects,
    auth: { admin: secrets.read("DB_ADMIN_PASSWORD"), app: secrets.read("DB_PASSWORD") },
    ensures: pins.ensures,
  }, bound)
  const lgtm = new Lgtm("observe", {
    spec,
    namespace: ns.metadata.name,
    versions: { lgtm: pins.lgtm, collector: pins.collector },
    auth: secrets.read("GRAFANA_PASSWORD"),
  }, bound)
  new Boards("boards", {
    spec,
    lgtm,
    auth: secrets.read("GRAFANA_PASSWORD"),
    boards: pins.boards,
    alerts: pins.alerts,
    objectives: pins.objectives,
    contacts: pins.contacts,
  })
  if (spec.profile.tenancy.mode !== "single") {
    new Tenants("tenants", { spec, versions: { capsule: pins.capsule, vcluster: pins.vcluster } }, bound)
  }
  const identity = new random.RandomUuid7("deploy-id", { keepers: { epoch: spec.epoch } })
  const token = Workload.token("doppler-token", { namespace: ns.metadata.name, token: secrets.token }, { provider })
  const outputs = {
    data: { host: data.host, port: data.port, database: data.database, role: data.role },
    object: { endpoint: objects.endpoint, bucket: objects.bucket },
    fanout: { origin: fanout.origin },
    otlp: { endpoint: lgtm.collectorEndpoint },
    grafana: { url: lgtm.urls.grafana },
    deploy: { id: identity.result },
  }
  return Option.match(app, {
    onNone: () => outputs,
    onSome: ({ image, edge }) => {
      const workload = new Workload("app", {
        spec,
        namespace: ns.metadata.name,
        image,
        port: pins.port,
        env: Workload.rows(token.metadata.name, StackOutputs.pairsOf(outputs, (value) => pulumi.output(value).apply(String))),
      }, bound)
      const ca = Certs.root("mesh-ca")
      const traffic = new Traffic("traffic", {
        spec,
        namespace: ns.metadata.name,
        service: workload.service.metadata.name,
        port: pins.port,
        connector: pins.cloudflared,
        issue: (hostname) => Certs.issue("edge", { ca, hostname }),
        apiToken: secrets.read("CLOUDFLARE_API_TOKEN"),
        edge,
      }, bound)
      return { ...outputs, ingress: { hostname: traffic.hostname } }
    },
  })
}

const _AWS: {
  readonly [K in StackSpec.Profile["compute"]]: (
    spec: StackSpec,
    pins: Dispatch.Pins,
    app: Option.Option<Dispatch.App>,
    opts: { readonly provider: aws.Provider },
  ) => Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, pulumi.Input<string | number>>>
} = {
  serverless: (spec, pins, _app, opts) => {
    const vpc = new awsx.ec2.Vpc("net", { numberOfAvailabilityZones: 2, natGateways: { strategy: "Single" } }, opts)
    const repo = new awsx.ecr.Repository("registry", { forceDelete: false }, opts)
    const image = new awsx.ecr.Image("app", { repositoryUrl: repo.url, context: pins.context }, opts)
    const alb = new awsx.lb.ApplicationLoadBalancer("edge", {}, opts)
    const cluster = new aws.ecs.Cluster("compute", {}, opts)
    new awsx.ecs.FargateService("app", {
      cluster: cluster.arn,
      desiredCount: 2,
      networkConfiguration: { subnets: vpc.privateSubnetIds },
      loadBalancers: [{ targetGroupArn: alb.defaultTargetGroup.arn, containerName: "app", containerPort: pins.port }],
      taskDefinitionArgs: { container: { name: "app", image: image.imageUri, portMappings: [{ containerPort: pins.port }] } },
    }, opts)
    const bucket = new aws.s3.BucketV2("objects", {}, opts)
    return {
      object: { endpoint: bucket.bucketRegionalDomainName, bucket: bucket.bucket },
      ingress: { hostname: alb.loadBalancer.dnsName },
    }
  },
  cluster: (spec, pins, app, opts) => {
    const vpc = new awsx.ec2.Vpc("net", { numberOfAvailabilityZones: 2, natGateways: { strategy: "Single" } }, opts)
    const plane = new eks.Cluster("plane", {
      vpcId: vpc.vpcId,
      publicSubnetIds: vpc.publicSubnetIds,
      privateSubnetIds: vpc.privateSubnetIds,
      authenticationMode: eks.AuthenticationMode.Api,
      createOidcProvider: true,
      skipDefaultNodeGroup: true,
    }, opts)
    new eks.ManagedNodeGroup("capacity", {
      cluster: plane,
      instanceTypes: [pins.nodes.instanceType],
      scalingConfig: { desiredSize: pins.nodes.min, minSize: pins.nodes.min, maxSize: pins.nodes.max },
    }, opts)
    const provider = new k8s.Provider("k8s", { kubeconfig: plane.kubeconfigJson, enableServerSideApply: true })
    return _estate(spec, pins, provider, app)
  },
}

const _ARMS: { readonly [K in StackSpec.Arm]: Dispatch.Arm } = {
  "selfhosted-k8s": (spec, material, pins) =>
    Effect.map(
      Effect.all({ proven: _proven(spec, material), app: _staged(spec) }),
      ({ proven, app }) => async () => {
        const bootstrap = new Bootstrap("plane", {
          connection: proven.connection,
          key: pulumi.secret(Redacted.value(proven.key)),
          epoch: spec.epoch,
          install: pins.install,
          facts: pins.facts,
        })
        const provider = new k8s.Provider("k8s", { kubeconfig: bootstrap.kubeconfig, enableServerSideApply: true })
        return _estate(spec, pins, provider, app)
      },
    ),
  "selfhosted-docker": (spec, material, pins) =>
    Effect.map(
      Effect.zipWith(
        _proven(spec, material),
        Effect.mapError(spec.image, () => _input(spec, "<missing-image>")),
        (proven, ref) => ({ proven, ref }),
      ),
      ({ proven, ref }) => async () => {
        const daemon = new Bootstrap("daemon", {
          connection: proven.connection,
          key: pulumi.secret(Redacted.value(proven.key)),
          epoch: spec.epoch,
          install: pins.install,
          facts: pins.facts,
        })
        const provider = new docker.Provider("engine", { host: proven.connection.ssh }, { dependsOn: [daemon] })
        const image = new dockerBuild.Image("app", {
          push: true,
          tags: [ref],
          context: { location: pins.context },
          platforms: ["linux/amd64", "linux/arm64"],
          cacheFrom: [{ registry: { ref: `${ref}-cache` } }],
          cacheTo: [{ registry: { ref: `${ref}-cache` } }],
        })
        const fence = new docker.Network("fence", { driver: "bridge", internal: false }, { provider })
        const state = new docker.Volume("state", { driver: "local" }, { provider })
        new docker.Container("app", {
          image: image.ref,
          restart: "unless-stopped",
          ports: [{ internal: pins.port, external: pins.port }],
          networksAdvanced: [{ name: fence.name }],
          volumes: [{ volumeName: state.name, containerPath: "/var/lib/rasm" }],
        }, { provider })
        return {}
      },
    ),
  aws: (spec, _material, pins) =>
    Effect.map(
      Effect.all({ region: _coord(spec, spec.region, "region"), app: _staged(spec) }),
      ({ region, app }) => async () => {
        const provider = new aws.Provider("aws", { region })
        return _AWS[spec.profile.compute](spec, pins, app, { provider })
      },
    ),
  gcp: (spec, _material, pins) =>
    Effect.map(
      Effect.all([
        Effect.mapError(spec.region, () => _input(spec, "<missing-region>")),
        Effect.mapError(spec.project, () => _input(spec, "<missing-project>")),
      ]),
      ([region, project]) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: { DB_PASSWORD: { generate: {} } } })
        const provider = new gcp.Provider("gcp", { project, region, credentials: secrets.read("GCP_CREDENTIALS") })
        const opts = { provider }
        new gcp.container.Cluster("plane", { location: region, initialNodeCount: 2 }, opts)
        const bucket = new gcp.storage.Bucket("objects", { location: region, uniformBucketLevelAccess: true }, opts)
        const sql = new gcp.sql.DatabaseInstance("data", {
          databaseVersion: pins.managedData.engine,
          region,
          settings: { tier: pins.managedData.tier },
        }, opts)
        new gcp.sql.Database("app", { instance: sql.name, name: spec.app }, opts)
        new gcp.sql.User("app-role", { instance: sql.name, name: `${spec.app}_app`, password: secrets.read("DB_PASSWORD") }, opts)
        return {
          object: { endpoint: bucket.url, bucket: bucket.name },
          data: { host: sql.publicIpAddress, port: 5432, database: spec.app, role: `${spec.app}_app` },
        }
      },
    ),
  cloudflare: (spec, _material, _pins) =>
    Effect.map(
      Effect.all([
        Effect.mapError(spec.domain, () => _input(spec, "<missing-domain>")),
        Effect.mapError(spec.zone, () => _input(spec, "<missing-zone>")),
        Effect.mapError(spec.account, () => _input(spec, "<missing-account>")),
      ]),
      ([domain, zone, account]) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: {} })
        const provider = new cloudflare.Provider("cf", { apiToken: secrets.read("CLOUDFLARE_API_TOKEN") })
        const store = new cloudflare.R2Bucket("objects", { accountId: account, name: `${spec.app}-artifacts` }, { provider })
        new cloudflare.DnsRecord("apex", {
          zoneId: zone,
          type: "CNAME",
          name: `${spec.app}.${domain}`,
          content: `${spec.app}.pages.dev`,
          proxied: true,
          ttl: 1,
        }, { provider })
        return {
          object: { endpoint: pulumi.interpolate`https://${account}.r2.cloudflarestorage.com/${store.name}`, bucket: store.name },
          ingress: { hostname: `${spec.app}.${domain}` },
        }
      },
    ),
}

const Dispatch = {
  ..._map,
  capabilities: _capabilities,
  cell: (capability: Dispatch.Capability, arm: StackSpec.Arm): Option.Option<string> =>
    Option.fromNullable(_cells[capability][arm]),
  column: (arm: StackSpec.Arm): Dispatch.Column =>
    Array.filterMap(_capabilities, (capability) =>
      Option.map(Option.fromNullable(_cells[capability][arm]), (family) => [capability, family] as const)),
  material: _material,
  program: (spec: StackSpec, material: Dispatch.Material, pins: Dispatch.Pins): Effect.Effect<PulumiFn, DeployFault> =>
    _ARMS[spec.target](spec, material, pins),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bootstrap, Dispatch }
```
