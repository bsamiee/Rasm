# [IAC_WORKLOAD]

Typed application workloads on the `selfhosted-k8s` arm: one `Workload` tier turns one spec row — a digest-pinned image, a port, the assembled env rows, the profile's scale — into the complete typed resource set: `ServiceAccount`, `Deployment`, and `Service`, with derived labels threading selector, template, and service as one correspondence. The page owns the whole runtime-injection seam the old corpus split into its own file: the channel-to-variable key map that is the folder's single env-spelling authority, the one Kubernetes `Secret` holding `DOPPLER_TOKEN`, the container `EnvVar` assembly, and the `doppler run --` entrypoint wrap — a deployed process reads secrets from its environment and nothing else. The runtime lifecycle contract mirrors here structurally: the `_LIFE` anchor carries the drain budget and the probe-route trio the runtime's `Life` owner anchors (`/startupz`, `/readyz`, `/livez`), `terminationGracePeriodSeconds` derives from the drain budget plus a fixed margin, and the same anchor stamps the `RUNTIME_LIFE_DRAIN` env row — one anchor, two projections, so the pod's grace period and the process's drain budget cannot drift. Sizing is a vocabulary table keyed by `StackSpec.profile.scale`; capacity retunes by editing a row, never a manifest. The module is `iac/src/kube/workload.ts`; a new injected fact is one key-map row, a new workload axis is one args field, a batch verb is one `CronJob` member on the same tier.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                  | [PUBLIC]   |
| :-----: | :-------------- | :----------------------------------------------------------------------- | :--------- |
|  [01]   | `SIZING_ROWS`   | the scale vocabulary: replicas, requests, limits per profile row         | `Workload` |
|  [02]   | `LIFE_MIRROR`   | the drain-budget and probe-route anchor mirrored from the runtime plane  | `Workload` |
|  [03]   | `ENV_ASSEMBLY`  | the key map, the token secret, the env rows, the entrypoint wrap         | `Workload` |
|  [04]   | `WORKLOAD_TIER` | the typed deployment/service/account set and the cron verb               | `Workload` |

## [2]-[SIZING_ROWS]

[SIZING_ROWS]:
- Owner: the interior `_scale` table keyed by the profile's `dev | standard | fleet` literal — each row carries `replicas`, `requests`, and `limits` as the `core/v1` resource-quantity strings the generated shapes consume, plus the probe cadence column; the row is read once at construction and stamps the container's `resources` block.
- Law: the scale key is `StackSpec`'s vocabulary — this table interprets it for the k8s arm and no second interpretation exists; the guard pair anchors on the spec's own scale union, so a spec tier with no row and an excess row both fail at the declaration, and a per-arm sizing divergence is a second table in that arm's owner, never a widened key.
- Growth: a new tier is one row; a new sizing axis (a GPU request, an ephemeral-storage bound) is one column every row states.
- Boundary: what the quantities mean to the scheduler is cluster fact; `StackSpec.profile.scale` selection is the app's.

## [3]-[LIFE_MIRROR]

[LIFE_MIRROR]:
- Owner: the `_LIFE` anchor — `drainSeconds` (the runtime `Setting.life.drain` default read as a deploy fact), `margin` (the finalizer headroom the pod grants past the process's own budget), and the `probes` record mirroring the runtime `Life` owner's kind/route anchor: `started → /startupz`, `ready → /readyz`, `live → /livez`; the tier derives `terminationGracePeriodSeconds = drainSeconds + margin` and the three probe blocks from this one anchor, and the `RUNTIME_LIFE_DRAIN` env row stamps the same `drainSeconds` so the process and its pod read one number.
- Law: probe semantics follow the runtime contract — `startupProbe` gates on `/startupz` with a generous failure budget (slow warm-up is legal once per boot), `readinessProbe` polls `/readyz` at the scale row's cadence (the phase-gated report flips to 503 the instant the drain starts, so the load balancer stops routing before any finalizer runs), and `livenessProbe` polls `/livez` on a slower cadence with restart as its only verb; the serving edge encodes pass/warn as 200 and fail as 503, so the probe blocks read HTTP status alone.
- Law: the paths exist once — the runtime anchors the routes and mounts them, this anchor mirrors the spellings into manifests, and a route rename is one edit on each side of the process boundary with the seam recorded in the folder's architecture; no third spelling exists.
- Growth: a new probe kind on the runtime side is one `probes` row here; a drain-budget change is one `drainSeconds` edit propagating to both projections.
- Boundary: the drain fold, the report grades, and the phase spine are the runtime plane's; this anchor is the deploy-side mirror of a settled contract, never a re-derivation.

```typescript
const _LIFE = {
  drainSeconds: 25,
  margin: 5,
  probes: {
    started: { path: "/startupz", periodSeconds: 5, failureThreshold: 24 },
    ready: { path: "/readyz", periodSeconds: 10, failureThreshold: 3 },
    live: { path: "/livez", periodSeconds: 20, failureThreshold: 3 },
  },
} as const

declare namespace _LIFE {
  type Kind = keyof typeof _LIFE.probes
  type _Rows<T extends { readonly [K in Kind]: { readonly path: string; readonly periodSeconds: number; readonly failureThreshold: number } } = typeof _LIFE.probes> = T
}
```

## [4]-[ENV_ASSEMBLY]

[ENV_ASSEMBLY]:
- Owner: the env seam — `_KEYS` is the channel-to-variable map (the one place a `StackOutputs` channel becomes an environment spelling), `Workload.token` provisions the namespace-scoped `core/v1.Secret` carrying `DOPPLER_TOKEN`, `Workload.rows` assembles the container's `EnvVar` list, and `Workload.entrypoint` is the `doppler run --` wrap; pair values are `Input`-typed, so live tier `Output`s (the in-program assembly) and decoded `StackOutputs.Pair` strings (the post-run projection) ride one signature.
- Law: the map is total over emitted channels — a channel with no `_KEYS` row is dropped by `filterMap` and that drop is deliberate absence, so publishing a new channel to processes is exactly one map row; the runtime-consumed spellings are pinned to their owners: `otlp.endpoint → OTEL_EXPORTER_OTLP_ENDPOINT` (the OTel exporter contract), `fanout.origin → RUNTIME_FANOUT_ORIGIN` (the runtime `Setting` fanout group), `object.* → OBJECT_*` (the data object plane's own Config rows), `sharding.* → IAC_SHARDING_*` (the rows `ShardingConfig.layerFromEnv` reads at the work seam).
- Law: policy rows stamp beside output rows — `_POLICY` carries the deploy-owned runtime Setting rows no output plane emits: `RUNTIME_LIFE_DRAIN` from the `_LIFE` anchor and `RUNTIME_CLUSTER_LOCK_REFRESH`/`RUNTIME_CLUSTER_LOCK_EXPIRY` (the leaderless grid's advisory-lock cadence, a topology posture the deploy plane owns); the `rows` signature's `policy` parameter is the merge seam an arm widens with the `RUNTIME_MAIL_*` coordinate set (SMTP host, port, user, DKIM domain and selector, rate ceiling) when the app's mail coordinates exist. Every value is a coordinate or a duration literal — the SMTP password rides Doppler like every credential.
- Law: coordinates ride plain rows, material rides references — output pairs are non-secret by `StackOutputs.read`'s gate, so they inject as `value`; the only secret-bearing row is the token `secretKeyRef`, and a second secret row is evidence a value bypassed Doppler.
- Law: the entrypoint wrap is the injection moment — a container carrying a `command` stamps `Workload.entrypoint(cmd)`, and a container without one runs an image whose baked `ENTRYPOINT` is the same `doppler run --` wrap (the app image's build contract), so `doppler run` resolves the scoped config into the process environment at start, the runtime's provider chain reads validated values, and the security plane's leased-secret owner reads the same config; the deploy plane never writes a decrypted payload to any surface a process reads before injection.
- Entry: `Workload.token(name, { namespace, token }, opts)` once per arm; `Workload.rows(tokenRef, pairs, policy?)` into the container env; `Workload.entrypoint(cmd)` as the container command.
- Growth: one `_KEYS` row per new output fact; one `_POLICY` row per new deploy-owned runtime setting.
- Boundary: the pair emission is `program/spec.md`'s; the token's mint is `operate/secret.md`'s; the runtime variable spellings are the runtime `Setting` owner's contract, mirrored here and recorded as a seam.
- Packages: `@pulumi/kubernetes` (`core.v1.Secret`, `types.input.core.v1.EnvVar`); `@pulumi/pulumi` (`Input`, `Output`); `effect` (`Array`, `Option`, `Record`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Array, Option, Record } from "effect"

const _TOKEN = "DOPPLER_TOKEN"
const _KEYS = {
  "ingress.hostname": "IAC_INGRESS_HOSTNAME",
  "data.host": "DATA_PG_HOST",
  "data.port": "DATA_PG_PORT",
  "data.database": "DATA_PG_DATABASE",
  "data.role": "DATA_PG_ROLE",
  "object.endpoint": "OBJECT_ENDPOINT",
  "object.bucket": "OBJECT_BUCKET",
  "fanout.origin": "RUNTIME_FANOUT_ORIGIN",
  "otlp.endpoint": "OTEL_EXPORTER_OTLP_ENDPOINT",
  "grafana.url": "IAC_GRAFANA_URL",
  "sharding.host": "IAC_SHARDING_HOST",
  "sharding.port": "IAC_SHARDING_PORT",
} as const

const _POLICY = {
  RUNTIME_LIFE_DRAIN: `${_LIFE.drainSeconds} seconds`,
  RUNTIME_CLUSTER_LOCK_REFRESH: "20 seconds",
  RUNTIME_CLUSTER_LOCK_EXPIRY: "1 minute",
} as const

const _keyed: Record.ReadonlyRecord<string, string> = _KEYS

declare namespace Workload {
  type Channel = keyof typeof _KEYS
  type EnvRow = k8s.types.input.core.v1.EnvVar
  type Pair = readonly [channel: string, value: pulumi.Input<string>]
  type _Keys<T extends Record.ReadonlyRecord<Channel, string> = typeof _KEYS> = T
}

const _token = (
  name: string,
  args: { readonly namespace: pulumi.Input<string>; readonly token: pulumi.Input<string> },
  opts?: pulumi.CustomResourceOptions,
): k8s.core.v1.Secret =>
  new k8s.core.v1.Secret(name, {
    metadata: { namespace: args.namespace },
    stringData: { [_TOKEN]: args.token },
  }, opts)

const _rows = (
  tokenSecret: pulumi.Output<string>,
  outputPairs: ReadonlyArray<Workload.Pair>,
  policy: Record.ReadonlyRecord<string, string> = _POLICY,
): ReadonlyArray<Workload.EnvRow> => [
  { name: _TOKEN, valueFrom: { secretKeyRef: { name: tokenSecret, key: _TOKEN } } },
  ...Array.map(Record.toEntries(policy), ([name, value]) => ({ name, value })),
  ...Array.filterMap(outputPairs, ([channel, value]) =>
    Option.map(Option.fromNullable(_keyed[channel]), (key) => ({ name: key, value }))),
]

const _entrypoint = (command: ReadonlyArray<string>): ReadonlyArray<string> => ["doppler", "run", "--", ...command]
```

## [5]-[WORKLOAD_TIER]

[WORKLOAD_TIER]:
- Owner: `Workload` — one constructor builds the `ServiceAccount` (the identity row RBAC binds against), the `Deployment` (selector and template labels derived from one `_labels` projection; the container carrying image, port, env rows, the three `_LIFE` probe blocks, and the scale row's resources; the pod carrying the derived `terminationGracePeriodSeconds`), and the `Service` (the same label selector, port-to-port); the service rides the tier as a readonly field so consumers wire `workload.service.metadata.name` onward, and the assembly members (`token`, `rows`, `entrypoint`, `cron`) ride the class as statics so one import carries the tier and its env seam.
- Law: the image is a digest ref — `Workload.Args.image` receives a `docker-build.Image` `ref`/`digest` value or an app-supplied `...@sha256:...` string; a mutable tag is admitted nowhere on this tier, and the compile-time gate is `operate/policy.md`'s digest policy over exactly this resource class.
- Law: labels are one derivation — `_labels(name)` stamps `app.kubernetes.io/name` and `app.kubernetes.io/managed-by`, and selector, template, and service all read the same value; a hand-written selector beside the derived labels is the drift this collapse deletes.
- Law: env is assembled on this page and appended nowhere else — the rows arrive from `Workload.rows` (token reference, policy rows, output pairs); a literal env row at a call site is a value that bypassed Doppler or the outputs seam.
- Law: namespace is a parameter — the arm constructs one `core/v1.Namespace` and threads `metadata.name` here; the tier never mints its own namespace, so every arm resource shares one blast-radius scope.
- Law: the cron verb is the host-schedule surface — `Workload.cron(name, args)` is one `batch/v1.CronJob` member reading the same labels, env assembly, and entrypoint wrap; it exists for schedules a database grant refusal pushes out of `pg_cron` and for deploy-plane maintenance verbs, and its schedule string is the caller's cron dialect fact.
- Entry: `new Workload("app", { spec, namespace, image, port, env }, opts)` inside the k8s arm, `opts` carrying the arm provider.
- Growth: an HPA is one `autoscaling` row when a profile earns it; a second exposed port is one field consumed at the one construction site.
- Boundary: ingress to the service is `kube/traffic.md`'s; RBAC rows beyond the account are the arm's identity cell; probe grading and drain choreography are the runtime plane's.
- Packages: `@pulumi/kubernetes` (`core.v1`, `apps.v1`, `batch.v1`); `@pulumi/pulumi` (`Input`, `Output`); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import { Tier, type StackSpec } from "../program/spec.ts"

const _scale = {
  dev: {
    replicas: 1,
    requests: { cpu: "100m", memory: "256Mi" },
    limits: { cpu: "500m", memory: "512Mi" },
  },
  standard: {
    replicas: 2,
    requests: { cpu: "250m", memory: "512Mi" },
    limits: { cpu: "1", memory: "1Gi" },
  },
  fleet: {
    replicas: 4,
    requests: { cpu: "500m", memory: "1Gi" },
    limits: { cpu: "2", memory: "2Gi" },
  },
} as const

const _labels = (name: string): Record.ReadonlyRecord<string, string> => ({
  "app.kubernetes.io/name": name,
  "app.kubernetes.io/managed-by": "rasm-iac",
})

const _probe = (kind: _LIFE.Kind, port: number): k8s.types.input.core.v1.Probe => ({
  httpGet: { path: _LIFE.probes[kind].path, port },
  periodSeconds: _LIFE.probes[kind].periodSeconds,
  failureThreshold: _LIFE.probes[kind].failureThreshold,
})

declare namespace Workload {
  type Scale = StackSpec.Profile["scale"]
  type Row = (typeof _scale)[Scale]
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly image: pulumi.Input<string>
    readonly port: number
    readonly env: ReadonlyArray<Workload.EnvRow>
    readonly command?: ReadonlyArray<string>
  }
  type CronArgs = {
    readonly namespace: pulumi.Input<string>
    readonly schedule: string
    readonly image: pulumi.Input<string>
    readonly command: ReadonlyArray<string>
    readonly env: ReadonlyArray<Workload.EnvRow>
  }
  type _Rows<T extends Record.ReadonlyRecord<Scale, {
    readonly replicas: number
    readonly requests: { readonly cpu: string; readonly memory: string }
    readonly limits: { readonly cpu: string; readonly memory: string }
  }> = typeof _scale> = T
  type _Keys<K extends Scale = keyof typeof _scale> = K
}

class Workload extends Tier {
  static readonly token = _token
  static readonly rows = _rows
  static readonly entrypoint = _entrypoint
  static readonly cron = (name: string, args: Workload.CronArgs, opts?: pulumi.CustomResourceOptions): k8s.batch.v1.CronJob =>
    new k8s.batch.v1.CronJob(name, {
      metadata: { namespace: args.namespace, labels: _labels(name) },
      spec: {
        schedule: args.schedule,
        jobTemplate: {
          spec: {
            template: {
              spec: {
                restartPolicy: "Never",
                containers: [{ name, image: args.image, command: [..._entrypoint(args.command)], env: [...args.env] }],
              },
            },
          },
        },
      },
    }, opts)
  readonly service: k8s.core.v1.Service
  constructor(name: string, args: Workload.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Workload", name, opts)
    const row = _scale[args.spec.profile.scale]
    const labels = _labels(name)
    const account = new k8s.core.v1.ServiceAccount(name, {
      metadata: { namespace: args.namespace, labels },
    }, this.child())
    new k8s.apps.v1.Deployment(name, {
      metadata: { namespace: args.namespace, labels },
      spec: {
        replicas: row.replicas,
        selector: { matchLabels: labels },
        template: {
          metadata: { labels },
          spec: {
            serviceAccountName: account.metadata.name,
            terminationGracePeriodSeconds: _LIFE.drainSeconds + _LIFE.margin,
            containers: [{
              name,
              image: args.image,
              ...(args.command !== undefined && { command: [..._entrypoint(args.command)] }),
              ports: [{ containerPort: args.port }],
              env: [...args.env],
              startupProbe: _probe("started", args.port),
              readinessProbe: _probe("ready", args.port),
              livenessProbe: _probe("live", args.port),
              resources: { requests: row.requests, limits: row.limits },
            }],
          },
        },
      },
    }, this.child())
    this.service = new k8s.core.v1.Service(name, {
      metadata: { namespace: args.namespace, labels },
      spec: {
        selector: labels,
        ports: [{ port: args.port, targetPort: args.port }],
      },
    }, this.child())
    this.seal({ service: this.service.metadata.name })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Workload }
```
