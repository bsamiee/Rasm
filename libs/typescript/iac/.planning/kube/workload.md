# [IAC_WORKLOAD]

Typed application workloads on the `selfhosted-k8s` arm: one `Workload` tier turns one spec row — a digest-pinned image, a port, the injected env rows, the profile's scale — into the complete typed resource set: `ServiceAccount`, `Deployment`, and `Service`, with derived labels threading selector, template, and service as one correspondence. Sizing is a vocabulary table keyed by `StackSpec.profile.scale` — replicas, requests, and limits are rows read at construction, so capacity retunes by editing a row, never a manifest. There is no YAML and no manifest adoption on this path: every field is a typed `Input` on the generated `apps/v1`/`core/v1` shapes, the image is an immutable `docker-build` digest ref (`policy/guard` compiles the pin into a gate), and provider binding arrives down the tier's parent chain from the arm's `k8s.Provider`. The module is `iac/src/kube/workload.ts`; a new workload axis is one args field consumed at the one construction site, a batch verb is one `batch/v1.CronJob` row on the same tier.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                        | [PUBLIC]   |
| :-----: | :-------------- | :--------------------------------------------------------------- | :--------- |
|  [01]   | `SIZING_ROWS`   | the scale vocabulary: replicas, requests, limits per profile row | `Workload` |
|  [02]   | `WORKLOAD_TIER` | the typed deployment/service/account set under one spec row      | `Workload` |

## [2]-[SIZING_ROWS]

[SIZING_ROWS]:
- Owner: the interior `_scale` table keyed by the profile's `dev | standard | fleet` literal — each row carries `replicas`, `requests`, and `limits` as the `core/v1` resource-quantity strings the generated shapes consume; the row is read once at construction and stamps the container's `resources` block.
- Law: the scale key is `StackSpec`'s vocabulary — this table interprets it for the k8s arm and no second interpretation exists; the guard pair anchors on the spec's own scale union, so a spec tier with no row and an excess row both fail at the declaration, and a per-arm sizing divergence is a second table in that arm's owner, never a widened key.
- Law: probes are policy columns — `readinessPath` and the probe cadence ride the row so a scale tier tunes health posture with capacity; a workload overrides neither at a call site.
- Growth: a new tier is one row; a new sizing axis (a GPU request, an ephemeral-storage bound) is one column every row states.
- Boundary: what the quantities mean to the scheduler is cluster fact; `StackSpec.profile.scale` selection is the app's.

## [3]-[WORKLOAD_TIER]

[WORKLOAD_TIER]:
- Owner: `Workload` — one constructor builds the `ServiceAccount` (the identity row RBAC binds against), the `Deployment` (selector and template labels derived from one `_labels` projection, the container carrying image, port, env rows, probe, and the scale row's resources), and the `Service` (the same label selector, port-to-port); the service rides the tier as a readonly field so consumers wire `workload.service.metadata.name` onward.
- Law: the image is a digest ref — `Workload.Args.image` receives a `docker-build.Image` `ref`/`digest` value or an app-supplied `...@sha256:...` string; a mutable tag is admitted nowhere on this tier, and the compile-time gate is `policy/guard.md`'s digest policy over exactly this resource class.
- Law: labels are one derivation — `_labels(name)` stamps `app.kubernetes.io/name` and `app.kubernetes.io/managed-by`, and selector, template, and service all read the same value; a hand-written selector beside the derived labels is the drift this collapse deletes.
- Law: env is assembled upstream — the rows arrive from `Inject.rows` (token reference plus output pairs) and the tier appends nothing; a literal env row at this tier is a value that bypassed Doppler or the outputs seam.
- Law: namespace is a parameter — the arm constructs one `core/v1.Namespace` and threads `metadata.name` here; the tier never mints its own namespace, so every arm resource shares one blast-radius scope.
- Entry: `new Workload("app", { spec, namespace, image, port, env }, opts)` inside the k8s arm, `opts` carrying the arm provider.
- Growth: a `CronJob` verb is one `batch/v1.CronJob` row on this tier reading the same labels and scale row; an HPA is one `autoscaling` row when a profile earns it.
- Boundary: ingress to the service is `kube/traffic.md`'s; RBAC rows beyond the account are the arm's identity cell; the env-row shape is `secret/inject.md`'s.
- Packages: `@pulumi/kubernetes` (`core.v1`, `apps.v1`); `@pulumi/pulumi` (`Input`, `Output`); `../program/spec.ts` (`StackSpec`); `../secret/inject.ts` (`Inject`); `../stack/component.ts` (`Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import type { StackSpec } from "../program/spec.ts"
import type { Inject } from "../secret/inject.ts"
import { Tier } from "../stack/component.ts"

const _scale = {
  dev: {
    replicas: 1,
    requests: { cpu: "100m", memory: "256Mi" },
    limits: { cpu: "500m", memory: "512Mi" },
    readinessPath: "/healthz",
    probeSeconds: 10,
  },
  standard: {
    replicas: 2,
    requests: { cpu: "250m", memory: "512Mi" },
    limits: { cpu: "1", memory: "1Gi" },
    readinessPath: "/healthz",
    probeSeconds: 10,
  },
  fleet: {
    replicas: 4,
    requests: { cpu: "500m", memory: "1Gi" },
    limits: { cpu: "2", memory: "2Gi" },
    readinessPath: "/healthz",
    probeSeconds: 5,
  },
} as const

const _labels = (name: string): Record<string, string> => ({
  "app.kubernetes.io/name": name,
  "app.kubernetes.io/managed-by": "rasm-iac",
})

declare namespace Workload {
  type Scale = StackSpec.Profile["scale"]
  type Row = (typeof _scale)[Scale]
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly image: pulumi.Input<string>
    readonly port: number
    readonly env: ReadonlyArray<Inject.EnvRow>
  }
  type _Rows<T extends Record<Scale, {
    readonly replicas: number
    readonly requests: { readonly cpu: string; readonly memory: string }
    readonly limits: { readonly cpu: string; readonly memory: string }
    readonly readinessPath: string
    readonly probeSeconds: number
  }> = typeof _scale> = T
  type _Keys<K extends Scale = keyof typeof _scale> = K
}

class Workload extends Tier {
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
            containers: [{
              name,
              image: args.image,
              ports: [{ containerPort: args.port }],
              env: [...args.env],
              readinessProbe: {
                httpGet: { path: row.readinessPath, port: args.port },
                periodSeconds: row.probeSeconds,
              },
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
