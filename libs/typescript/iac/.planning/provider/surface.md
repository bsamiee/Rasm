# [IAC_SURFACE]

The per-arm service surface: one vocabulary table maps every deployment capability to the resource family that realizes it on each arm — the service-equivalence map that makes a prepared cloud row worth carrying, because retargeting a stack is re-reading one column, never rewriting topology. Rows are capabilities, columns are `StackSpec.Arm`, cells name the exact resource-family spelling, and a hole is honest absence the reader gets as `Option`. The `selfhosted-k8s` column additionally owns the cluster-bootstrap row realized here as the `Bootstrap` tier: `@pulumi/command` `remote.Command` over owned metal/VPS installs the control plane, its stdout is the kubeconfig, staged assets ride `CopyToRemote`, and ordering is graph-derived — `command` provisions the metal, `@pulumi/kubernetes` owns every workload thereafter. The module is `iac/src/provider/surface.ts`; a new capability is one map row, a new cloud is one column filled cell by cell, and no cell is ever a recipe — the realizing page owns the mechanics.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                           | [PUBLIC]    |
| :-----: | :------------------ | :------------------------------------------------------------------ | :---------- |
|  [01]   | `EQUIVALENCE_MAP`   | the capability-by-arm table and its `Option`-lifted projections     | `Surface`   |
|  [02]   | `CLUSTER_BOOTSTRAP` | the metal/VPS bootstrap tier: install, staging, kubeconfig egress   | `Bootstrap` |

## [2]-[EQUIVALENCE_MAP]

[EQUIVALENCE_MAP]:
- Owner: `Surface` — the interior `_capabilities` key tuple anchors the row order, the `_map` table carries per-arm cells as exact-optional keys (a hole is an omitted key, never a sentinel), and the exported owner assembles rows, `capabilities`, and the two projections: `cell(capability, arm)` lifts the unproven cell read to `Option`, `column(arm)` folds an arm's realized subset in row order. The reads ride `_cells`, the table widened to `Partial<Record<Arm, string>>` rows — a declared-key access on the literal union would demand the key on every row, so the bracket read is index trust lifted at the seam while `_map` keeps its literals for the assembled owner.
- Law: cells are family spellings, not mechanics — a cell names the resource classes (`container.Cluster + container.NodePool`) or the owning row (`helm minio | garage`), and the page that constructs them is the mechanics owner; the map is what dispatch arms, drift reports, and capability audits read.
- Law: the canonical secret owner spans every column — the `secret` row is Doppler on all five arms, and a cloud secret manager is reachable only as a mirror: a `secretssync.<Target>` pair where the Doppler provider ships the destination (`AwsSecretsManager`), an in-graph provider write fed by the fan-in read where it does not (`gcp.secretmanager`), so no arm ever grows a second secret source of truth.
- Law: a prepared column's filled cells are its finalization contract — the `gcp` column names GKE, Cloud SQL, GCS, Cloud DNS, and Secret Manager mirrors against the same capability rows the primary arm realizes, so finalizing means instantiating the named subset with the `StackSpec` value; the dormant remainder of a provider SDK is unreachable by construction.
- Entry: `Surface.column(spec.target)` inside a dispatch arm; `Surface.cell("data", "aws")` for a point read.
- Growth: a new capability is one `_capabilities` entry plus one `_map` row; a new arm is one cell per realized row under the new column key.
- Boundary: the arm programs composing these cells are `provider/dispatch.md`'s; the kube rows' mechanics are `kube/*`; the object/data engine choices are `StackSpec.profile` values.
- Packages: `effect` (`Array`, `Option`, `Types`); `../program/spec.ts` (`StackSpec`).

```typescript
import { Array, Option, type Types } from "effect"
import type { StackSpec } from "../program/spec.ts"

const _capabilities = [
  "bootstrap", "workload", "data", "object", "cert", "dns",
  "ingress", "secret", "registry", "network", "identity", "observe",
] as const

const _map = {
  bootstrap: {
    "selfhosted-k8s": "command.remote.Command + CopyToRemote",
    "selfhosted-docker": "command.remote.Command",
  },
  workload: {
    "selfhosted-k8s": "kubernetes.apps/v1.Deployment + core/v1.Service",
    "selfhosted-docker": "docker.Container",
    aws: "awsx.ecs.FargateService | aws.eks.Cluster",
    gcp: "gcp.container.Cluster + gcp.container.NodePool",
    cloudflare: "cloudflare.WorkersScript + cloudflare.WorkersRoute",
  },
  data: {
    "selfhosted-k8s": "cnpg Cluster CR + postgresql.Database/Role/Grant/Extension",
    "selfhosted-docker": "docker.Container(postgres) + postgresql.Database/Role/Grant/Extension",
    aws: "aws.rds.Cluster",
    gcp: "gcp.sql.DatabaseInstance + gcp.sql.Database + gcp.sql.User",
    cloudflare: "cloudflare.D1Database",
  },
  object: {
    "selfhosted-k8s": "helm minio | garage",
    "selfhosted-docker": "docker.Container(minio | garage)",
    aws: "aws.s3.BucketV2",
    gcp: "gcp.storage.Bucket + gcp.storage.BucketIAMMember",
    cloudflare: "cloudflare.R2Bucket",
  },
  cert: {
    "selfhosted-k8s": "tls chain -> core/v1.Secret(kubernetes.io/tls)",
    aws: "aws.acm.Certificate",
    gcp: "gcp.certificatemanager.Certificate",
    cloudflare: "cloudflare.OriginCaCertificate + cloudflare.TotalTls",
  },
  dns: {
    "selfhosted-k8s": "cloudflare.DnsRecord",
    "selfhosted-docker": "cloudflare.DnsRecord",
    aws: "aws.route53.Zone + aws.route53.Record",
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
    aws: "aws.iam.Role + aws.iam.Policy + aws.iam.RolePolicyAttachment",
    gcp: "gcp.serviceaccount.Account + gcp.projects.IAMMember",
  },
  observe: {
    "selfhosted-k8s": "helm lgtm + otel collector + grafana apply",
    "selfhosted-docker": "helm-free collector container + grafana apply",
  },
} as const

declare namespace Surface {
  type Capability = (typeof _capabilities)[number]
  type Column = ReadonlyArray<readonly [Capability, string]>
  type Shape = Types.Simplify<typeof _map & {
    readonly capabilities: typeof _capabilities
    readonly cell: (capability: Capability, arm: StackSpec.Arm) => Option.Option<string>
    readonly column: (arm: StackSpec.Arm) => Column
  }>
  type _Rows<T extends Record<Capability, Partial<Record<StackSpec.Arm, string>>> = typeof _map> = T
  type _Keys<K extends keyof typeof _map = Capability> = K
}

const _cells: Record<Surface.Capability, Partial<Record<StackSpec.Arm, string>>> = _map

const Surface: Surface.Shape = {
  ..._map,
  capabilities: _capabilities,
  cell: (capability, arm) => Option.fromNullable(_cells[capability][arm]),
  column: (arm) =>
    Array.filterMap(_capabilities, (capability) =>
      Option.map(Option.fromNullable(_cells[capability][arm]), (family) => [capability, family] as const)),
}
```

## [3]-[CLUSTER_BOOTSTRAP]

[CLUSTER_BOOTSTRAP]:
- Owner: `Bootstrap`, the tier that turns owned metal into a cluster — staged assets ride `remote.CopyToRemote` (rendered install artifacts as `Asset`/`Archive` values, never checked-in paths), the control plane installs through one `remote.Command` whose CRUD slots own install (`create`) and teardown (`remove`), and `kubeconfig` egresses as the secret-tracked stdout the `@pulumi/kubernetes` `Provider` binds.
- Law: the connection is coordinates plus injected material — `StackSpec.Connection` supplies host/user/port, the PEM key arrives as a `pulumi.Input<string>` already secret-tracked from `Dispatch.material`, and `logging: "none"` gates every credential-bearing step so captured output never echoes key material.
- Law: re-run is trigger-driven — the `triggers` list carries the spec `epoch` and the staged-asset references, so a bootstrap re-runs exactly when its inputs change and never by manual replacement; ordering is graph-derived, the plane command `dependsOn` its staged copies.
- Law: the takeover boundary is absolute — after the kubeconfig exists, every workload is a typed `@pulumi/kubernetes` resource; a shell command that duplicates a typed provider resource is the named defect, and `command` survives only for bare-metal mutation no typed provider owns.
- Entry: `new Bootstrap("plane", { connection, key, epoch, install, assets }, opts)` inside the selfhosted arms; `bootstrap.kubeconfig` feeds `new k8s.Provider(...)`.
- Growth: a new host mutation is one `remote.Command` row inside this tier with its own triggers; a new staged artifact is one `assets` entry.
- Boundary: the install script content is app/config data handed in as `install`; the k8s provider construction is `provider/dispatch.md`'s arm body; unconditional shell reads (`local.runOutput`) belong to the arm that needs the fact.
- Packages: `@pulumi/command` (`remote.Command`, `remote.CopyToRemote`); `@pulumi/pulumi` (`Output`, `secret`, `asset`); `../program/spec.ts` (`StackSpec`); `../stack/component.ts` (`Tier`).

```typescript
import * as command from "@pulumi/command"
import * as pulumi from "@pulumi/pulumi"
import type { StackSpec } from "../program/spec.ts"
import { Tier } from "../stack/component.ts"

declare namespace Bootstrap {
  type Args = {
    readonly connection: StackSpec.Connection
    readonly key: pulumi.Input<string>
    readonly epoch: string
    readonly install: pulumi.Input<string>
    readonly remove?: pulumi.Input<string>
    readonly assets?: ReadonlyArray<{
      readonly source: pulumi.asset.Asset | pulumi.asset.Archive
      readonly remotePath: string
    }>
  }
}

class Bootstrap extends Tier {
  readonly kubeconfig: pulumi.Output<string>
  constructor(name: string, args: Bootstrap.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Bootstrap", name, opts)
    const connection = {
      host: args.connection.host,
      user: args.connection.user,
      port: args.connection.port,
      privateKey: args.key,
    }
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
      triggers: [args.epoch, ...staged.map((copy) => copy.remotePath)],
      logging: "none",
    }, this.child({ dependsOn: staged }))
    this.kubeconfig = pulumi.secret(plane.stdout)
    this.seal({ kubeconfig: this.kubeconfig })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Bootstrap, Surface }
```
