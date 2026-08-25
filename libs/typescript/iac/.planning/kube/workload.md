# [IAC_WORKLOAD]

`Workload` lowers one service or worker row into shared identity, pod, sizing, lifecycle, hardening, and optional network cells. Service rows own rolling replacement, CPU elasticity, and a `Service`; worker rows own claim-safe `Recreate` replacement and no network surface. `StackOutputs` supplies channel, custody, and backend environment spellings, one Kubernetes `Secret` carries the custody roster, and `doppler run --` is the injection edge. `_LIFE` mirrors runtime drain and probe facts into pod grace and health gates, `Tier.harden` is the privilege posture every pod and container on the tier stamps, and `_scale` interprets `StackSpec.profile.scale`.

## [01]-[INDEX]

- [02]-[SIZING_ROWS]: the scale vocabulary: replicas, requests, limits per profile row; `Workload`.
- [03]-[LIFE_MIRROR]: the drain-budget and probe-route anchor mirrored from the runtime plane; `Workload`.
- [04]-[ENV_ASSEMBLY]: the key map, the token secret, the env rows, the entrypoint wrap; `Workload`.
- [05]-[WORKLOAD_TIER]: service/worker lowering, identity, hardening, deployment, optional service, cron; `Workload`.

## [02]-[SIZING_ROWS]

[SIZING_ROWS]:
- Owner: the interior `_scale` table keyed by the profile's `dev | standard | fleet` literal — each row carries `replicas`, `requests`, and `limits` as the `core/v1` resource-quantity strings the generated shapes consume, with the resilience columns the row's posture earns: `disruptionBudget` realizes a `policy/v1.PodDisruptionBudget` and `autoscale` realizes an `autoscaling/v2.HorizontalPodAutoscaler` at construction, so capacity, availability floor, and elasticity retune by editing one row, never a manifest.
- Law: the scale key is `StackSpec`'s vocabulary — this table interprets it for the k8s arm and no second interpretation exists; the guard pair anchors on the spec's own scale union, so a spec tier with no row and an excess row both fail at the declaration, and a per-arm sizing divergence is a second table in that arm's owner, never a widened key.
- Law: an autoscale row owns the replica count — the deployment omits `replicas` on any row carrying `autoscale` so the autoscaler's live verdict survives every `up` instead of resetting to the row's floor; `minReplicas` is the floor's one spelling on such a row.
- Law: an autoscale row owns its elasticity in both directions — `behavior` carries a stabilization window, a per-period ceiling, and a selection posture per direction, because the API's own defaults are a 300-second scale-down window against no scale-up window at all, so a row stating `replicas` and a `disruptionBudget` explicitly while leaving flap behaviour implicit is the one uncontrolled knob on a table built to make capacity editable in one place; the readonly policy tuples cross into the generated input shape at the one `_behavior` fold, so neither direction re-spreads its own list at the construction site.
- Law: the direction keys derive and the two policy vocabularies close locally — `Direction` reads `keyof` the generated behavior shape so a row cannot name a direction the API omits, while the generated `selectPolicy` and policy `type` are bare `Input<string>` and this table closes each against its own literal union; the rules shape also carries a `tolerance` field gated behind the cluster's `HPAConfigurableTolerance` feature, so no row states it and the cluster-wide default stands.
- Law: a disruption row owns its eviction criterion — `unhealthyPodEvictionPolicy` decides whether a drain completes, since the API default (`IfHealthyBudget`) refuses to evict a running-but-unhealthy pod while that pod is exactly what keeps `currentHealthy` below `desiredHealthy`, so a two-replica `standard` row at `minAvailable: 1` deadlocks its own node drain; `AlwaysAllow` is the estate's answer because a pod failing its readiness gate serves no traffic and its eviction costs nothing the budget was protecting.
- Growth: a new tier is one row; a new sizing axis (a GPU request, an ephemeral-storage bound) is one column every row states.
- Boundary: what the quantities mean to the scheduler is cluster fact; `StackSpec.profile.scale` selection is the app's.

## [03]-[LIFE_MIRROR]

[LIFE_MIRROR]:
- Owner: the `_LIFE` anchor — `drainSeconds` (the runtime `Setting.life.drain` default read as a deploy fact), `margin` (the finalizer headroom the pod grants past the process's own budget), and the `probes` record mirroring the runtime `Life` owner's kind/route anchor: `started → /startupz`, `ready → /readyz`, `live → /livez`; the tier derives `terminationGracePeriodSeconds = drainSeconds + margin` and the three probe blocks from this one anchor, and the `RUNTIME_LIFE_DRAIN` env row stamps the same `drainSeconds` so the process and its pod read one number.
- Law: probe semantics follow the runtime contract — `startupProbe` gates on `/startupz` with a generous failure budget (slow warm-up is legal once per boot), `readinessProbe` polls `/readyz` at the scale row's cadence (the phase-gated report flips to 503 the instant the drain starts, so the load balancer stops routing before any finalizer runs), and `livenessProbe` polls `/livez` on a slower cadence with restart as its only verb; the serving edge encodes pass/warn as 200 and fail as 503, so the probe blocks read HTTP status alone.
- Law: every probe row states its own response budget — the API defaults `timeoutSeconds` to ONE, so a report the process answers correctly but slowly counts as a failure, and the row that then restarts a healthy pod under load is the liveness one; each kind carries the budget its report earns, and a row silent on the field runs the whole health contract against a one-second ceiling nothing on either side of the seam chose.
- Law: the paths exist once — the runtime anchors the routes and mounts them, this anchor mirrors the spellings into manifests, and a route rename is one edit on each side of the process boundary with the seam recorded in the folder's architecture; no third spelling exists.
- Growth: a new probe kind on the runtime side is one `probes` row here; a drain-budget change is one `drainSeconds` edit propagating to both projections.
- Boundary: the drain fold, the report grades, and the phase spine are the runtime plane's; this anchor is the deploy-side mirror of a settled contract, never a re-derivation.

```typescript
const _LIFE = {
  drainSeconds: 25,
  margin: 5,
  probes: {
    started: { path: "/startupz", periodSeconds: 5, failureThreshold: 24, timeoutSeconds: 3 },
    ready: { path: "/readyz", periodSeconds: 10, failureThreshold: 3, timeoutSeconds: 3 },
    live: { path: "/livez", periodSeconds: 20, failureThreshold: 3, timeoutSeconds: 5 },
  },
} as const

declare namespace _LIFE {
  type Kind = keyof typeof _LIFE.probes
  type Row = { readonly path: string; readonly periodSeconds: number; readonly failureThreshold: number; readonly timeoutSeconds: number }
  type _Rows<T extends { readonly [K in Kind]: Row } = typeof _LIFE.probes> = T
}
```

## [04]-[ENV_ASSEMBLY]

[ENV_ASSEMBLY]:
- Owner: the env seam — `StackOutputs.channels` is the typed channel-to-variable catalog and `StackOutputs.custody` its custody-cell half (both owned at `program/spec.md`, the channel map derived total over the output planes), `Workload.token` provisions the unleased namespace cell carrying the token variable and yields the same `StackOutputs.Cell` shape `operate/secret.md`'s leased mint yields, `Workload.rows` assembles the container's `EnvVar` list, and `Workload.entrypoint` is the `doppler run --` wrap; pair values are `Input`-typed, so live tier `Output`s (the in-program assembly) and decoded `StackOutputs.Pair` strings (the post-run projection) ride one signature.
- Law: the variable a channel lands on is `StackOutputs.channels`' row, not a map here — this tier renders the `EnvVar` and the catalog owns the spelling, so a channel with no catalog row is dropped by `filterMap` as the deliberate absence it is (`served` is the one such plane, its key vocabulary being its caller's), and publishing a new channel to processes is one catalog row at the plane that mints it.
- Law: policy rows stamp beside output rows — `_POLICY` carries the deploy-owned runtime Setting rows no output plane emits: `RUNTIME_LIFE_DRAIN` from the `_LIFE` anchor and `RUNTIME_CLUSTER_LOCK_REFRESH`/`RUNTIME_CLUSTER_LOCK_EXPIRY` (the leaderless grid's advisory-lock cadence, a topology posture the deploy plane owns); the `rows` signature's `policy` parameter is the merge seam an arm widens with the `RUNTIME_MAIL_*` coordinate set (SMTP host, port, user, DKIM domain and selector, rate ceiling) when the app's mail coordinates exist — widening rows merge over the `_POLICY` base, so an arm never re-spells the standing rows to add one. Every value is a coordinate or a duration literal — the SMTP password rides Doppler like every credential.
- Law: coordinates ride plain rows, material rides references — output pairs are non-secret by `StackOutputs.read`'s gate, so they inject as `value`; the custody cell's own variables are the settled `secretKeyRef` set, and any other secret row is evidence a value bypassed Doppler.
- Law: the custody cell is one secret and its contents are data — `StackOutputs.custody` maps each variable a namespace cell may carry to its spelling, and `rows` stamps exactly the roster the cell states, so `operate/secret.md`'s plain token cell and its leased cell (the token beside the encoded lease boundary the security custodian decodes) reach this tier through one shape with no flag branching the fold; the roster seats at the spec owner because the minting tier sits on a lower stratum than this one and a literal at either end forks one variable name into two, so a second custody variable is one row there reaching the mint and this stamp together.
- Law: the entrypoint wrap is the injection moment — a container carrying a `command` stamps `Workload.entrypoint(cmd)`, and a container without one runs an image whose baked `ENTRYPOINT` is the same `doppler run --` wrap (the app image's build contract), so `doppler run` resolves the scoped config into the process environment at start and the runtime's provider chain reads validated values; the deploy plane never writes a decrypted payload to any surface a process reads before injection.
- Law: each custody coordinate names its injection source at the security owner — `crypt/secret#LEASED_CUSTODY`'s `Coordinate` table marks `DOPPLER_TOKEN` mounted and `DOPPLER_PROJECT`/`DOPPLER_CONFIG` injected, so this tier stamps the token alone as the custody cell's `secretKeyRef` and the `doppler run --` wrap resolves the remaining pair off the token's own config scope inside the process it wraps; a `_POLICY` row or a second secret row carrying a Doppler coordinate is the fork that table forecloses.
- Entry: `Workload.token(name, { namespace, token }, opts)` once per arm, or `Secrets.lease(...)` where the scope is leased; `Workload.rows(custody, pairs, policy?)` into the container env; `Workload.entrypoint(cmd)` as the container command.
- Growth: a new output channel, custody variable, or backend coordinate is one row on its `StackOutputs` catalog; one `_POLICY` row per new deploy-owned runtime setting.
- Boundary: pair emission and both variable catalogs are `program/spec.md`'s; token minting is `operate/secret.md`'s; the runtime `Setting` owner reads the catalog's spellings as its writing counterpart.
- Packages: `@pulumi/kubernetes` (`core.v1.Secret`, `types.input.core.v1.EnvVar`); `@pulumi/pulumi` (`Input`, `Output`); `effect` (`Array`, `Option`, `Record`); `../program/spec.ts` (`StackOutputs`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Array, Option, Record } from "effect"
import { StackOutputs } from "../program/spec.ts"

const _POLICY = {
  RUNTIME_LIFE_DRAIN: `${_LIFE.drainSeconds} seconds`,
  RUNTIME_CLUSTER_LOCK_REFRESH: "20 seconds",
  RUNTIME_CLUSTER_LOCK_EXPIRY: "1 minute",
} as const

const _keyed: Record.ReadonlyRecord<string, string> = StackOutputs.channels
const _CUSTODY = StackOutputs.custody

declare namespace Workload {
  type Channel = StackOutputs.Channel
  type EnvRow = k8s.types.input.core.v1.EnvVar
  type Pair = readonly [channel: string, value: pulumi.Input<string>]
}

const _token = (
  name: string,
  args: { readonly namespace: pulumi.Input<string>; readonly token: pulumi.Input<string> },
  opts?: pulumi.CustomResourceOptions,
): StackOutputs.Cell => ({
  secret: new k8s.core.v1.Secret(name, {
    metadata: { namespace: args.namespace },
    stringData: { [_CUSTODY.token]: args.token },
  }, opts).metadata.name,
  carries: ["token"],
})

const _rows = (
  custody: StackOutputs.Cell,
  outputPairs: ReadonlyArray<Workload.Pair>,
  policy?: Record.ReadonlyRecord<string, string>,
): ReadonlyArray<Workload.EnvRow> => [
  ...Array.map(custody.carries, (held) => ({
    name: _CUSTODY[held],
    valueFrom: { secretKeyRef: { name: custody.secret, key: _CUSTODY[held] } },
  })),
  ...Array.map(Record.toEntries({ ..._POLICY, ...policy }), ([name, value]) => ({ name, value })),
  ...Array.filterMap(outputPairs, ([channel, value]) =>
    Option.map(Option.fromNullable(_keyed[channel]), (key) => ({ name: key, value }))),
]

const _entrypoint = (command: ReadonlyArray<string>): ReadonlyArray<string> => ["doppler", "run", "--", ...command]
```

## [05]-[WORKLOAD_TIER]

[WORKLOAD_TIER]:
- Owner: `Workload` lowers the `role` discriminant through one constructor; identity, pod, sizing, probes, placement, and drain remain shared.
- Law: rolling replacement, CPU HPA where the scale row admits it, and one label-derived `Service`.
- Law: `Recreate` prevents claim overlap, replica count remains explicit, and the network surface is absent.
- Law: the image is a digest ref — `Workload.Args.image` receives a `docker-build.Image` `ref`/`digest` value or an app-supplied `...@sha256:...` string; a mutable tag is admitted nowhere on this tier, and the compile-time gate is `operate/policy.md`'s digest policy over exactly this resource class.
- Law: labels are one derivation — `_labels(name)` stamps `app.kubernetes.io/name` and `app.kubernetes.io/managed-by`, and selector, template, and service all read the same value; a hand-written selector beside the derived labels is the drift this collapse deletes.
- Law: privilege posture is `Tier.harden` and this tier stamps it, never declares it — the Deployment's pod spec, its container, `Workload.cron`'s job pod spec, and that job's container each read the base anchor, which the traffic tier's connector and the converge tier's runners read too; a copy here would be a second estate posture on a plane whose own mandatory gate asserts exactly one.
- Law: the API token mounts on evidence, never by default — a projected service-account token is an ambient cluster credential sitting in every container's filesystem, so `automountServiceAccountToken` reads the one fact that decides whether the workload reaches the API at all: the `rbac` rows this tier granted; a workload stating none carries no token, and `Workload.cron` states none by construction.
- Law: labels, hardening, and the backend projection stack in one pod slot each — `volumes` and `volumeMounts` merge the anchor's scratch pair with `_backed`'s projection rather than choosing between them, so a workload composing no backend generation still mounts its writable path.
- Law: the selector rides the tier — `Workload.selector` publishes the derived label set the Service and template already share, so the traffic tier's fence selects this workload's own pods instead of every pod in a namespace the estate also fills with its data, fanout, object, and collector planes; a fence keyed on the namespace closes the app's own dependencies out of it.
- Law: `Workload.rows` owns application settings; the backend projection adds only generated-file paths beside those rows.
- Law: the backend generation is caller-supplied, never assumed — `_backed` folds its absence to three empty pod slots, so a workload composing no schema contract deploys with no projected volume and no pointer path; a required generation narrows this tier to the one deployment shape that already carries a merged, deployed, admitted contract.
- Law: namespace is a parameter — the arm constructs one `core/v1.Namespace` and threads `metadata.name` here; the tier never mints its own namespace, so every arm resource shares one blast-radius scope.
- Law: the cron verb is the host-schedule surface — `Workload.cron(name, args)` is one `batch/v1.CronJob` member reading the same labels, env assembly, entrypoint wrap, and hardening anchor; it exists for schedules a database grant refusal pushes out of `pg_cron` and for deploy-plane maintenance verbs, and its schedule string is the caller's cron dialect fact.
- Law: a maintenance schedule carries its execution policy, never the API's defaults — `_CRON` is the one policy row and `_cronPolicy` merges the caller's deltas over it exactly as `_rows` merges `_POLICY`, so `concurrency` defaults to `Forbid` (the API defaults to `Allow`, and an overlapping run of a verb that mutates the state it reads corrupts it), `history` retains a bounded success and failure pair as the operator's evidence, `deadlineSeconds` bounds how late a missed window may still fire, `timeZone` fixes the wall clock the schedule reads because an unset field runs it in the kube-controller-manager's own zone and drifts on a control-plane move, and `suspend` is the pause an operator holds through a migration; a caller stating no policy gets the maintenance-correct row rather than the permissive one, and `_cronPolicy` projects onto the generated spec's own field set so a renamed member breaks at the fold rather than at apply.
- Law: the schedule's policy reaches the JOB it spawns, not the CronJob alone — `Forbid` suppresses every later run while an earlier one is alive, so a verb that hangs suspends its own schedule indefinitely unless the Job states a wall-clock ceiling: `runSeconds` bounds a single execution through `activeDeadlineSeconds`, `attempts` replaces the API's six silent retries of a mutating verb with a stated budget, and `ttlSeconds` retires the finished Job's pod so an operator's evidence window is the history pair rather than an unbounded pod residue. The two halves ride one `_CRON` row and `_jobPolicy` projects the job-level half exactly as `_cronPolicy` projects the schedule-level one.
- Entry: `new Workload("app", { spec, namespace, image, role, env }, opts)` inside the k8s arm; `Workload.cron(name, { namespace, schedule, image, command, env, policy? }, opts)` for a maintenance verb.
- Packages: `@pulumi/kubernetes` (`core.v1`, `apps.v1`, `batch.v1`, `policy.v1`, `autoscaling.v2`, `types.input.batch.v1.{CronJobSpec,JobSpec}`, `types.input.autoscaling.v2.{HorizontalPodAutoscalerBehavior,HPAScalingRules}`); `@pulumi/pulumi` (`Input`, `Output`); `../program/spec.ts` (`StackSpec`, `Tier`).
- Growth: a new elasticity or availability posture is one `_scale` row column; a new privilege refusal is one `Tier.harden` field; a new schedule policy axis is one `_CRON` row field; an API grant is one `rbac` rule row; a second exposed port is one field consumed at the one construction site.
- Boundary: runtime owns claims, leases, backlog evidence, handlers, and generation admission; this tier only lowers carrier facts.

```typescript
import * as random from "@pulumi/random"
import { Match } from "effect"
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
    disruptionBudget: { minAvailable: 1, unhealthyPodEvictionPolicy: "AlwaysAllow" },
  },
  fleet: {
    replicas: 4,
    requests: { cpu: "500m", memory: "1Gi" },
    limits: { cpu: "2", memory: "2Gi" },
    disruptionBudget: { minAvailable: 2, unhealthyPodEvictionPolicy: "AlwaysAllow" },
    autoscale: {
      min: 4,
      max: 12,
      cpuPercent: 70,
      behavior: {
        scaleDown: { stabilizationWindowSeconds: 300, selectPolicy: "Min", policies: [{ type: "Pods", value: 1, periodSeconds: 60 }] },
        scaleUp: { stabilizationWindowSeconds: 30, selectPolicy: "Max", policies: [{ type: "Percent", value: 100, periodSeconds: 30 }] },
      },
    },
  },
} as const

const _CRON: Workload.CronPolicy = {
  concurrency: "Forbid",
  history: { successful: 3, failed: 1 },
  deadlineSeconds: 120,
  suspend: false,
  timeZone: "UTC",
  runSeconds: 3600,
  attempts: 1,
  ttlSeconds: 86400,
}

const _behavior = (rows: Workload.Behavior): k8s.types.input.autoscaling.v2.HorizontalPodAutoscalerBehavior =>
  Record.map(rows, (rules) => ({
    stabilizationWindowSeconds: rules.stabilizationWindowSeconds,
    selectPolicy: rules.selectPolicy,
    policies: Array.map(rules.policies, (policy) => ({ type: policy.type, value: policy.value, periodSeconds: policy.periodSeconds })),
  }))

const _cronPolicy = (
  policy?: Partial<Workload.CronPolicy>,
): Pick<
  k8s.types.input.batch.v1.CronJobSpec,
  "concurrencyPolicy" | "successfulJobsHistoryLimit" | "failedJobsHistoryLimit" | "startingDeadlineSeconds" | "suspend" | "timeZone"
> => {
  const row = { ..._CRON, ...policy }
  return {
    concurrencyPolicy: row.concurrency,
    successfulJobsHistoryLimit: row.history.successful,
    failedJobsHistoryLimit: row.history.failed,
    startingDeadlineSeconds: row.deadlineSeconds,
    suspend: row.suspend,
    timeZone: row.timeZone,
  }
}

const _jobPolicy = (
  policy?: Partial<Workload.CronPolicy>,
): Pick<k8s.types.input.batch.v1.JobSpec, "activeDeadlineSeconds" | "backoffLimit" | "ttlSecondsAfterFinished"> => {
  const row = { ..._CRON, ...policy }
  return {
    activeDeadlineSeconds: row.runSeconds,
    backoffLimit: row.attempts - 1,
    ttlSecondsAfterFinished: row.ttlSeconds,
  }
}

const _labels = (name: string): Record.ReadonlyRecord<string, string> => ({
  "app.kubernetes.io/name": name,
  "app.kubernetes.io/managed-by": "rasm-iac",
})

const _probe = (kind: _LIFE.Kind, port: number): k8s.types.input.core.v1.Probe => ({
  httpGet: { path: _LIFE.probes[kind].path, port },
  periodSeconds: _LIFE.probes[kind].periodSeconds,
  failureThreshold: _LIFE.probes[kind].failureThreshold,
  timeoutSeconds: _LIFE.probes[kind].timeoutSeconds,
})

const _backed = (backend: Workload.Backend | undefined): {
  readonly env: ReadonlyArray<Workload.EnvRow>
  readonly mounts: ReadonlyArray<k8s.types.input.core.v1.VolumeMount>
  readonly volumes: ReadonlyArray<k8s.types.input.core.v1.Volume>
} =>
  backend === undefined
    ? { env: [], mounts: [], volumes: [] }
    : {
        env: [
          { name: StackOutputs.backend.root, value: backend.root },
          { name: StackOutputs.backend.pointer, value: `${backend.root}/generation` },
        ],
        mounts: [{ name: "backend", mountPath: backend.root, readOnly: true }],
        volumes: [{
          name: "backend",
          projected: {
            sources: [
              { configMap: { name: backend.contract.metadata.name } },
              { configMap: { name: backend.pointer.metadata.name } },
            ],
          },
        }],
      }

declare namespace Workload {
  type Scale = StackSpec.Profile["scale"]
  type Row = (typeof _scale)[Scale]
  type Rule = { readonly apiGroups: ReadonlyArray<string>; readonly resources: ReadonlyArray<string>; readonly verbs: ReadonlyArray<string> }
  type Role =
    | { readonly _tag: "service"; readonly port: number }
    | { readonly _tag: "worker"; readonly probePort: number }
  type Backend = {
    readonly contract: k8s.core.v1.ConfigMap
    readonly pointer: k8s.core.v1.ConfigMap
    readonly root: string
  }
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly image: pulumi.Input<string>
    readonly role: Role
    readonly env: ReadonlyArray<Workload.EnvRow>
    readonly backend?: Backend
    readonly command?: ReadonlyArray<string>
    readonly rbac?: ReadonlyArray<Rule>
    readonly zones?: ReadonlyArray<string>
  }
  type Concurrency = "Allow" | "Forbid" | "Replace"
  type CronPolicy = {
    readonly concurrency: Concurrency
    readonly history: { readonly successful: number; readonly failed: number }
    readonly deadlineSeconds: number
    readonly suspend: boolean
    readonly timeZone: string
    readonly runSeconds: number
    readonly attempts: number
    readonly ttlSeconds: number
  }
  type CronArgs = {
    readonly namespace: pulumi.Input<string>
    readonly schedule: string
    readonly image: pulumi.Input<string>
    readonly command: ReadonlyArray<string>
    readonly env: ReadonlyArray<Workload.EnvRow>
    readonly policy?: Partial<CronPolicy>
  }
  type Direction = keyof k8s.types.input.autoscaling.v2.HorizontalPodAutoscalerBehavior
  type Select = "Max" | "Min" | "Disabled"
  type Step = "Pods" | "Percent"
  type Rules = {
    readonly stabilizationWindowSeconds: number
    readonly selectPolicy: Select
    readonly policies: ReadonlyArray<{ readonly type: Step; readonly value: number; readonly periodSeconds: number }>
  }
  type Behavior = { readonly [D in Direction]-?: Rules }
  type _Rows<T extends Record.ReadonlyRecord<Scale, {
    readonly replicas: number
    readonly requests: { readonly cpu: string; readonly memory: string }
    readonly limits: { readonly cpu: string; readonly memory: string }
    readonly disruptionBudget?: { readonly minAvailable: number; readonly unhealthyPodEvictionPolicy: "AlwaysAllow" | "IfHealthyBudget" }
    readonly autoscale?: {
      readonly min: number
      readonly max: number
      readonly cpuPercent: number
      readonly behavior: Behavior
    }
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
        ..._cronPolicy(args.policy),
        jobTemplate: {
          spec: {
            ..._jobPolicy(args.policy),
            template: {
              spec: {
                restartPolicy: "Never",
                securityContext: Tier.harden.pod,
                automountServiceAccountToken: false,
                containers: [{
                  name,
                  image: args.image,
                  command: [..._entrypoint(args.command)],
                  env: [...args.env],
                  securityContext: Tier.harden.container,
                  volumeMounts: [...Tier.harden.mounts],
                }],
                volumes: [...Tier.harden.volumes],
              },
            },
          },
        },
      },
    }, opts)
  readonly service: Option.Option<k8s.core.v1.Service>
  readonly selector: Record.ReadonlyRecord<string, string>
  constructor(name: string, args: Workload.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Workload", name, opts)
    const row = _scale[args.spec.profile.scale]
    const backed = _backed(args.backend)
    const labels = _labels(name)
    this.selector = labels
    const port = Match.value(args.role).pipe(Match.tagsExhaustive({
      service: (role) => role.port,
      worker: (role) => role.probePort,
    }))
    const account = new k8s.core.v1.ServiceAccount(name, {
      metadata: { namespace: args.namespace, labels },
    }, this.child())
    const rules = args.rbac ?? []
    if (rules.length > 0) {
      const role = new k8s.rbac.v1.Role(name, {
        metadata: { namespace: args.namespace, labels },
        rules: rules.map((rule) => ({ apiGroups: [...rule.apiGroups], resources: [...rule.resources], verbs: [...rule.verbs] })),
      }, this.child())
      new k8s.rbac.v1.RoleBinding(name, {
        metadata: { namespace: args.namespace, labels },
        roleRef: { apiGroup: "rbac.authorization.k8s.io", kind: "Role", name: role.metadata.name },
        subjects: [{ kind: "ServiceAccount", name: account.metadata.name, namespace: args.namespace }],
      }, this.child())
    }
    const spread = args.zones === undefined
      ? undefined
      : new random.RandomShuffle(`${name}-zones`, { inputs: [...args.zones], seed: name }, this.child())
    new k8s.apps.v1.Deployment(name, {
      metadata: { name, namespace: args.namespace, labels },
      spec: {
        ...(args.role._tag === "service" && "autoscale" in row ? {} : { replicas: row.replicas }),
        strategy: args.role._tag === "worker"
          ? { type: "Recreate" }
          : { type: "RollingUpdate", rollingUpdate: { maxSurge: 1, maxUnavailable: 0 } },
        selector: { matchLabels: labels },
        template: {
          metadata: { labels },
          spec: {
            serviceAccountName: account.metadata.name,
            securityContext: Tier.harden.pod,
            automountServiceAccountToken: rules.length > 0,
            terminationGracePeriodSeconds: _LIFE.drainSeconds + _LIFE.margin,
            topologySpreadConstraints: [{
              maxSkew: 1,
              topologyKey: "kubernetes.io/hostname",
              whenUnsatisfiable: "ScheduleAnyway",
              labelSelector: { matchLabels: labels },
            }],
            ...(spread !== undefined && {
              affinity: {
                nodeAffinity: {
                  preferredDuringSchedulingIgnoredDuringExecution: spread.results.apply((zones) =>
                    zones.map((zone, rank) => ({
                      weight: 100 - rank * 10,
                      preference: { matchExpressions: [{ key: "topology.kubernetes.io/zone", operator: "In", values: [zone] }] },
                    }))),
                },
              },
            }),
            containers: [{
              name,
              image: args.image,
              ...(args.command !== undefined && { command: [..._entrypoint(args.command)] }),
              ports: [{ containerPort: port }],
              env: [...args.env, ...backed.env],
              securityContext: Tier.harden.container,
              volumeMounts: [...Tier.harden.mounts, ...backed.mounts],
              startupProbe: _probe("started", port),
              readinessProbe: _probe("ready", port),
              livenessProbe: _probe("live", port),
              resources: { requests: row.requests, limits: row.limits },
            }],
            volumes: [...Tier.harden.volumes, ...backed.volumes],
          },
        },
      },
    }, this.child())
    if ("disruptionBudget" in row) {
      new k8s.policy.v1.PodDisruptionBudget(name, {
        metadata: { namespace: args.namespace, labels },
        spec: {
          minAvailable: row.disruptionBudget.minAvailable,
          unhealthyPodEvictionPolicy: row.disruptionBudget.unhealthyPodEvictionPolicy,
          selector: { matchLabels: labels },
        },
      }, this.child())
    }
    if (args.role._tag === "service" && "autoscale" in row) {
      new k8s.autoscaling.v2.HorizontalPodAutoscaler(name, {
        metadata: { namespace: args.namespace, labels },
        spec: {
          scaleTargetRef: { apiVersion: "apps/v1", kind: "Deployment", name },
          minReplicas: row.autoscale.min,
          maxReplicas: row.autoscale.max,
          behavior: _behavior(row.autoscale.behavior),
          metrics: [{
            type: "Resource",
            resource: { name: "cpu", target: { type: "Utilization", averageUtilization: row.autoscale.cpuPercent } },
          }],
        },
      }, this.child())
    }
    this.service = args.role._tag === "service"
      ? Option.some(new k8s.core.v1.Service(name, {
          metadata: { namespace: args.namespace, labels },
          spec: { selector: labels, ports: [{ port, targetPort: port }] },
        }, this.child()))
      : Option.none()
    this.seal({
      role: args.role._tag,
      service: Option.match(this.service, {
        onNone: () => undefined,
        onSome: (service) => service.metadata.name,
      }),
    })
  }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Workload }
```

## [06]-[RESEARCH]

(none)
