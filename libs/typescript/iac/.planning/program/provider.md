# [IAC_PROVIDER]

Provider dispatch and the service surface as ONE owner keyed by one union: the `_map` equivalence table and the `_ARMS` handler record both key on `StackSpec.Arm`, so the capability-by-arm data and the arm program that realizes it live on one page and cannot drift — the census-flagged duplication where two files re-spelled the arm axis is dead. Rows are capabilities, columns are arms, cells name the exact resource-family spelling, and a hole is honest absence the reader gets as `Option`. Each arm is a total function from the spec (plus the Effect-resolved host material and deploy-time pins) to a `PulumiFn`: it proves its own preconditions on the rail as typed `DeployFault`s, constructs exactly one provider seam, composes the tier roster its column names, and returns the outputs record `StackOutputs` decodes. `Bootstrap` is the column's own realized row: `@pulumi/command` `remote.Command` over owned metal/VPS installs the control plane, its stdout is the kubeconfig, staged assets ride `CopyToRemote`, and ordering is graph-derived — `command` provisions the metal, `@pulumi/kubernetes` owns every workload thereafter. Adding a cloud is one record row plus one map column; finalizing one is a `StackSpec` value, never a lib edit. The module is `iac/src/program/provider.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                               | [PUBLIC]    |
| :-----: | :------------------ | :-------------------------------------------------------------------- | :---------- |
|  [01]   | `EQUIVALENCE_MAP`   | the capability-by-arm table and its `Option`-lifted projections       | `Dispatch`  |
|  [02]   | `ARM_CONTRACT`      | the material read, pins, the arm signature, the exhaustive record law | `Dispatch`  |
|  [03]   | `CLUSTER_BOOTSTRAP` | the metal/VPS bootstrap tier: staging, install, kubeconfig egress     | `Bootstrap` |
|  [04]   | `ARM_PROGRAMS`      | the five arm bodies at their realized depth                           | `Dispatch`  |

## [2]-[EQUIVALENCE_MAP]

[EQUIVALENCE_MAP]:
- Owner: the interior `_capabilities` key tuple anchoring row order, the `_map` table carrying per-arm cells as exact-optional keys (a hole is an omitted key, never a sentinel), and the two projections riding the exported owner: `cell(capability, arm)` lifts the unproven cell read to `Option`, `column(arm)` folds an arm's realized subset in row order. The reads ride `_cells`, the table widened to `Dispatch.Cell` rows — a declared-key access on the literal union demands the key on every row, so the bracket read is index trust lifted at the seam while `_map` keeps its literals.
- Law: cells are family spellings, not mechanics — a cell names the resource classes (`gcp.container.Cluster + NodePool`) or the owning row (`helm minio-continuation | ceph-rgw`), and the page that constructs them is the mechanics owner; the map is what arm programs, drift reports, and capability audits read.
- Law: the object row admits only conditional-put-conforming engines — the self-host cells name the maintained MinIO continuation and Ceph RGW, the managed cells name S3, R2, and Tigris-class endpoints; the CRDT-metadata engine that cannot honor `If-None-Match: *` has no cell anywhere, because the data plane's write-once identity algebra is non-negotiable and the refusal is `data`'s engine table read as deployment law.
- Law: the canonical secret owner spans every column — the `secret` row is Doppler on all five arms, and a cloud secret manager is reachable only as a mirror (`secretssync.<Target>` where the Doppler provider ships the destination, an in-graph write fed by the fan-in read where it does not), so no arm ever grows a second secret source of truth.
- Law: a prepared column's filled cells are its finalization contract — the `gcp` column names GKE, Cloud SQL, GCS, Cloud DNS, and the Secret Manager mirror against the same capability rows the primary arm realizes, so finalizing means instantiating the named subset with the `StackSpec` value; the dormant remainder of a provider SDK is unreachable by construction.
- Entry: `Dispatch.column(spec.target)` inside an arm; `Dispatch.cell("data", "aws")` for a point read.
- Growth: a new capability is one `_capabilities` entry plus one `_map` row; a new arm is one cell per realized row under the new column key.
- Boundary: kube-row mechanics are `kube/*`; the object/data engine choices are `StackSpec.profile` values; cross-stack output reads ride `StackReference` inside a consuming program when a multi-stack estate earns one.
- Packages: `effect` (`Array`, `Option`, `Types`); `./spec.ts` (`StackSpec`).

```typescript
import { Array, Option, Record } from "effect"
import type { StackSpec } from "./spec.ts"

const _capabilities = [
  "bootstrap", "workload", "data", "object", "fanout", "cert", "dns",
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
    aws: "awsx.ecs.FargateService",
    gcp: "gcp.container.Cluster + gcp.container.NodePool",
    cloudflare: "cloudflare.WorkersScript + cloudflare.WorkersRoute",
  },
  data: {
    "selfhosted-k8s": "cnpg Cluster CR (managed roles) + Database CR + ensure Job",
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
    "selfhosted-docker": "collector container + grafana apply",
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
- Law: arms prove, never assume — a selfhosted arm lifts `spec.connection` and the SSH key together and an absence mints an `input` fault naming the coordinate; a prepared arm demands `region` the same way; no arm body ever meets an unproven `Option`.
- Law: pins are a parameter, never a module read — `Dispatch.Pins` carries the deploy-time facts the spec does not (chart and operator versions, the extension-image ref, the install script, the build context, the ensure-DDL roster the data plane publishes, the encoded boards and alert specs from the core observe suite); the app root resolves them from its own config and suite call, so ingress is parameterized end to end and the lib hardcodes no version anywhere.
- Law: one provider seam per arm — the arm constructs its provider (kubeconfig-bound `k8s.Provider`, `ssh://` `docker.Provider`, credentialed cloud provider) exactly once and threads it through tier options; per-resource providers are the named defect, and the credential arrives from `Secrets.read` in-graph or the ambient `doppler run` env, never a literal.
- Law: the `PulumiFn` body is the deploy plane's program seam — a promise-returning composition of tier constructors bound to consts and one returned outputs record; the platform owns that shape, and everything the arm computes before entering it stays on the rail.
- Entry: `Effect.flatMap(Dispatch.material, (material) => Dispatch.program(spec, material, pins))` then `Automation.stack(spec, program)`.
- Growth: one record row plus one map column per cloud; a new shared deploy-time fact is one `Pins` field, a new shared secret fact is one `material` field.
- Boundary: the run and receipt are `automation.md`'s; outputs keys are `spec.md`'s contract.
- Packages: `effect` (`Config`, `Effect`, `Option`, `Redacted`); `./spec.ts` (`StackSpec`); `./automation.ts` (`DeployFault`).

```typescript
import type { PulumiFn } from "@pulumi/pulumi/automation"
import { Config, Effect, Option, Redacted } from "effect"
import type { Alert, DashboardModel } from "@rasm/ts/core"
import { DeployFault } from "./automation.ts"
import { StackSpec } from "./spec.ts"

declare namespace Dispatch {
  type Material = { readonly sshKey: Option.Option<Redacted.Redacted<string>> }
  type Arm = (spec: StackSpec, material: Material, pins: Pins) => Effect.Effect<PulumiFn, DeployFault>
  type Pins = {
    readonly install: string
    readonly pgImage: string
    readonly operator: string
    readonly object: string
    readonly nats: string
    readonly lgtm: string
    readonly collector: string
    readonly port: number
    readonly context: string
    readonly ensures: ReadonlyArray<string>
    readonly boards: ReadonlyArray<typeof DashboardModel.Encoded>
    readonly alerts: ReadonlyArray<Alert.Spec>
  }
}

const _material = Config.unwrap({
  sshKey: Config.option(Config.redacted("IAC_SSH_KEY")),
})

const _input = (spec: StackSpec, detail: string): DeployFault =>
  new DeployFault({ reason: "input", stack: spec.name, detail })

const _proven = (spec: StackSpec, material: Dispatch.Material): Effect.Effect<{
  readonly connection: StackSpec.Connection
  readonly key: Redacted.Redacted<string>
}, DeployFault> =>
  Option.zipWith(spec.connection, material.sshKey, (connection, key) => ({ connection, key })).pipe(
    Effect.mapError(() => _input(spec, "<missing-connection-or-key>")),
  )
```

## [4]-[CLUSTER_BOOTSTRAP]

[CLUSTER_BOOTSTRAP]:
- Owner: `Bootstrap`, the tier that turns owned metal into a cluster — staged assets ride `remote.CopyToRemote` (rendered install artifacts as `Asset`/`Archive` values, never checked-in paths), the control plane installs through one `remote.Command` whose CRUD slots own install (`create`) and teardown (`delete`), and `kubeconfig` egresses as the secret-tracked stdout the `@pulumi/kubernetes` `Provider` binds.
- Law: the connection is coordinates plus injected material — `StackSpec.Connection` supplies host/user/port, the PEM key arrives as a `pulumi.Input<string>` already secret-tracked from `Dispatch.material`, and `logging: "none"` gates every credential-bearing step so captured output never echoes key material; a bastion hop is one `proxy: ProxyConnectionArgs` row on the same connection when the estate earns one.
- Law: re-run is trigger-driven — the `triggers` list carries the spec `epoch` and the staged-asset references, so a bootstrap re-runs exactly when its inputs change and never by manual replacement; ordering is graph-derived, the plane command `dependsOn` its staged copies.
- Law: the takeover boundary is absolute — after the kubeconfig exists, every workload is a typed `@pulumi/kubernetes` resource; a shell command that duplicates a typed provider resource is the named defect, and `command` survives only for bare-metal mutation no typed provider owns; an unconditional host fact is `local.runOutput` when it threads the graph, `local.run` for an eager read inside the program body.
- Entry: `new Bootstrap("plane", { connection, key, epoch, install, assets }, opts)` inside the selfhosted arms; `bootstrap.kubeconfig` feeds `new k8s.Provider(...)`.
- Growth: a new host mutation is one `remote.Command` row inside this tier with its own triggers; a new staged artifact is one `assets` entry.
- Boundary: the install script content is app data handed in as `install`; the k8s provider construction is the arm body's.
- Packages: `@pulumi/command` (`remote.Command`, `remote.CopyToRemote`, `local.run`, `local.runOutput`); `@pulumi/pulumi` (`Output`, `secret`, `asset`); `./spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as command from "@pulumi/command"
import * as pulumi from "@pulumi/pulumi"
import { Tier } from "./spec.ts"

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
```

## [5]-[ARM_PROGRAMS]

[ARM_PROGRAMS]:
- Law: the k8s arm composes in dependency order — `Bootstrap` (kubeconfig) → `k8s.Provider` + one `Namespace` → `Secrets` (generated entries: `DB_ADMIN_PASSWORD`, `DB_PASSWORD`, `OBJECT_USER`, `OBJECT_PASSWORD`, `GRAFANA_PASSWORD`; `CLOUDFLARE_API_TOKEN` pre-exists on the app's config) → `ObjectStore` → `Nats` → `Postgres` (the admin and app credentials as two distinct reads) → `Lgtm` → `Boards` → `Workload.token` → optional `Workload` whose live-`Output` env pairs derive from the same outputs record through the one `_pairs` fold (when `spec.image` is present) → one `Certs.root` CA → `Traffic` over the workload service with the issuance capability injected — and returns every realized `StackOutputs` plane, `fanout` included.
- Law: the app image is one buildx product — the docker arm and any registry cell build through `docker-build.Image` with `push: true`, the immutable `ref`/`digest` pinning every runtime; `platforms` rows make the build multi-arch, `cacheFrom`/`cacheTo` registry rows reuse layers across runs, and by-value `secrets` bind Doppler outputs so no build credential touches disk. The fastcdc wasm artifact is a build-stage product of this same image — a rust stage runs `wasm-pack build` over the pinned `fastcdc` crate and the runtime stage copies the pkg — so the chunking artifact ships inside the image digest and no second artifact pipeline exists.
- Law: the docker arm is build-plus-runtime — the `ssh://` `docker.Provider` binds the proven connection (the daemon `Bootstrap`'s docker install left behind), `docker.Network` and `docker.Volume` fence and persist, and the app `docker.Container` pins the built digest at its flat verified members (`image`, `restart`, `envs`, `command`); the nested `ports`/`mounts`/`networksAdvanced` arg records are a RESEARCH row until the docker catalog carries their field spellings, so the container constructs without them, publishes no phantom output plane, and the arm returns only the planes it realizes.
- Law: the aws arm realizes its column through the composition tier — one `aws.Provider` from the proven region (credentials ambient under `doppler run`), `awsx.ec2.Vpc` expands the AZ intent, `awsx.ecr.Repository` + `awsx.ecr.Image` build-and-push through the same bundled buildx builder, `aws.ecs.Cluster` + `awsx.ecs.FargateService` run the digest behind `awsx.lb.ApplicationLoadBalancer`, and `aws.s3.BucketV2` is the object cell; raw `aws.*` resources compose only where the component does not expose the attribute.
- Law: the gcp and cloudflare arms are provider-seam-complete — `gcp` binds `credentials` from the `GCP_CREDENTIALS` fan-in read and realizes the GKE anchor (`container.Cluster` at its catalogued fields); `cloudflare` binds `apiToken` from `CLOUDFLARE_API_TOKEN` and realizes the dns cell (`DnsRecord` at its catalogued fields); the Cloud SQL, GCS, R2, and Workers realizers stay declared rows whose argument records are the standing RESEARCH items, and promoting either arm is implementing its declared realizer against the catalogued args — the record row, signature, and outputs contract never move.
- Law: every arm funds the boards — the encoded models and alert specs enter as pins where the arm realizes an observe cell; an arm without the observe cell returns no `grafana` plane and drops nothing silently.
- Growth: promoting a prepared arm is one realizer body; a new cloud is one record row plus one map column.
- Boundary: tier mechanics live on the tier pages; the declared realizers' argument catalogues are the standing research items on the provider `.api` files.
- Packages: `@pulumi/kubernetes`, `@pulumi/docker`, `@pulumi/docker-build`, `@pulumi/aws`, `@pulumi/awsx`, `@pulumi/gcp`, `@pulumi/cloudflare` (providers + composed classes); every folder tier.

```typescript
import * as aws from "@pulumi/aws"
import * as awsx from "@pulumi/awsx"
import * as cloudflare from "@pulumi/cloudflare"
import * as docker from "@pulumi/docker"
import * as dockerBuild from "@pulumi/docker-build"
import * as gcp from "@pulumi/gcp"
import * as k8s from "@pulumi/kubernetes"
import { Nats, ObjectStore, Postgres } from "../kube/data.ts"
import { Traffic } from "../kube/traffic.ts"
import { Workload } from "../kube/workload.ts"
import { Boards, Lgtm } from "../operate/observe.ts"
import { Certs, Secrets } from "../operate/secret.ts"

declare const _gcpRows: (spec: StackSpec, provider: gcp.Provider, plane: gcp.container.Cluster) => Record.ReadonlyRecord<string, unknown>
declare const _cloudflareRows: (spec: StackSpec, provider: cloudflare.Provider) => Record.ReadonlyRecord<string, unknown>

const _pairs = (
  planes: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, pulumi.Input<string | number>>>,
): ReadonlyArray<Workload.Pair> =>
  Array.flatMap(Record.toEntries(planes), ([plane, held]) =>
    Array.map(Record.toEntries(held), ([field, value]) =>
      [`${plane}.${field}`, pulumi.output(value).apply(String)] as const))

const _ARMS: { readonly [K in StackSpec.Arm]: Dispatch.Arm } = {
  "selfhosted-k8s": (spec, material, pins) =>
    Effect.map(_proven(spec, material), (proven) => async () => {
      const bootstrap = new Bootstrap("plane", {
        connection: proven.connection,
        key: pulumi.secret(Redacted.value(proven.key)),
        epoch: spec.epoch,
        install: pins.install,
      })
      const provider = new k8s.Provider("k8s", { kubeconfig: bootstrap.kubeconfig, enableServerSideApply: true })
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
      const fanout = new Nats("fanout", {
        spec,
        namespace: ns.metadata.name,
        version: pins.nats,
      }, bound)
      const data = new Postgres("data", {
        spec,
        namespace: ns.metadata.name,
        image: pins.pgImage,
        operatorVersion: pins.operator,
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
      new Boards("boards", { spec, lgtm, auth: secrets.read("GRAFANA_PASSWORD"), boards: pins.boards, alerts: pins.alerts })
      const token = Workload.token("doppler-token", { namespace: ns.metadata.name, token: secrets.token }, { provider })
      const outputs = {
        data: { host: data.host, port: data.port, database: data.database, role: data.role },
        object: { endpoint: objects.endpoint, bucket: objects.bucket },
        fanout: { origin: fanout.origin },
        otlp: { endpoint: lgtm.collectorEndpoint },
        grafana: { url: lgtm.urls.grafana },
      }
      return Option.match(spec.image, {
        onNone: () => outputs,
        onSome: (image) => {
          const workload = new Workload("app", {
            spec,
            namespace: ns.metadata.name,
            image,
            port: pins.port,
            env: Workload.rows(token.metadata.name, _pairs(outputs)),
          }, bound)
          const ca = Certs.root("mesh-ca")
          const traffic = new Traffic("traffic", {
            spec,
            namespace: ns.metadata.name,
            service: workload.service.metadata.name,
            port: pins.port,
            issue: (hostname) => Certs.issue("edge", { ca, hostname }),
            apiToken: secrets.read("CLOUDFLARE_API_TOKEN"),
          }, bound)
          return { ...outputs, ingress: { hostname: traffic.hostname } }
        },
      })
    }),
  "selfhosted-docker": (spec, material, pins) =>
    Effect.map(
      Effect.zipWith(
        _proven(spec, material),
        Effect.mapError(spec.image, () => _input(spec, "<missing-image>")),
        (proven, ref) => ({ proven, ref }),
      ),
      ({ proven, ref }) => async () => {
        const provider = new docker.Provider("engine", {
          host: `ssh://${proven.connection.user}@${proven.connection.host}:${proven.connection.port}`,
        })
        const image = new dockerBuild.Image("app", {
          push: true,
          tags: [ref],
          context: { location: pins.context },
          platforms: ["linux/amd64", "linux/arm64"],
          cacheFrom: [{ registry: { ref: `${ref}-cache` } }],
          cacheTo: [{ registry: { ref: `${ref}-cache` } }],
        })
        new docker.Network("fence", { driver: "bridge", internal: false }, { provider })
        new docker.Volume("state", { driver: "local" }, { provider })
        new docker.Container("app", {
          image: image.ref,
          restart: "unless-stopped",
        }, { provider })
        return {}
      },
    ),
  aws: (spec, _material, pins) =>
    Effect.map(
      Effect.mapError(spec.region, () => _input(spec, "<missing-region>")),
      (region) => async () => {
        const provider = new aws.Provider("aws", { region })
        const opts = { provider }
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
    ),
  gcp: (spec, _material, _pins) =>
    Effect.map(
      Effect.mapError(spec.region, () => _input(spec, "<missing-region>")),
      (region) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: {} })
        const provider = new gcp.Provider("gcp", {
          project: spec.doppler.project,
          region,
          credentials: secrets.read("GCP_CREDENTIALS"),
        })
        const plane = new gcp.container.Cluster("plane", {
          location: region,
          initialNodeCount: 2,
        }, { provider })
        return _gcpRows(spec, provider, plane)
      },
    ),
  cloudflare: (spec, _material, _pins) =>
    Effect.map(
      Effect.all([
        Effect.mapError(spec.domain, () => _input(spec, "<missing-domain>")),
        Effect.mapError(spec.zone, () => _input(spec, "<missing-zone>")),
      ]),
      ([domain, zone]) => async () => {
        const secrets = new Secrets("secrets", { spec, entries: {} })
        const provider = new cloudflare.Provider("cf", { apiToken: secrets.read("CLOUDFLARE_API_TOKEN") })
        new cloudflare.DnsRecord("apex", {
          zoneId: zone,
          type: "CNAME",
          name: `${spec.app}.${domain}`,
          content: `${spec.app}.pages.dev`,
          proxied: true,
          ttl: 1,
        }, { provider })
        return { ..._cloudflareRows(spec, provider), ingress: { hostname: `${spec.app}.${domain}` } }
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
