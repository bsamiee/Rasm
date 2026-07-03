# [IAC_DISPATCH]

The one closed provider dispatch: `_ARMS` is an exhaustive handler record keyed by `StackSpec.Arm` — the record form of exhaustive dispatch over the spec vocabulary, where the mapped contract makes a missing arm a compile error at the record and `Dispatch.program` is one generic indexed call. Each arm is a total function from the spec (plus the Effect-resolved host material) to a `PulumiFn`: it proves its own preconditions on the rail as typed `DeployFault`s, constructs exactly one provider seam, composes the tier roster its surface column names, and returns the outputs record `StackOutputs` decodes. The `selfhosted-k8s` arm is the fully realized composition — bootstrap through boards in one dependency-ordered body; `selfhosted-docker` builds through the canonical `docker-build.Image` and runs over an `ssh://` daemon; the prepared arms construct their live provider seams from the spec plus the Doppler fan-in and hold their capability realizers at declared-signature depth until their argument surfaces are catalogued to operator depth. Adding a cloud is one record row plus one surface column; finalizing one is a `StackSpec` value. The module is `iac/src/provider/dispatch.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]      | [OWNS]                                                            | [PUBLIC]   |
| :-----: | :------------- | :------------------------------------------------------------------ | :--------- |
|  [01]   | `ARM_CONTRACT` | the material read, the arm signature, and the exhaustive record law | `Dispatch` |
|  [02]   | `ARM_PROGRAMS` | the five arm bodies and the tier composition order                  | `Dispatch` |

## [2]-[ARM_CONTRACT]

[ARM_CONTRACT]:
- Owner: `Dispatch` — `material` is the one deploy-host Config read the arms share (`IAC_SSH_KEY` as an optional `Redacted`, resolved under `doppler run`), `program(spec, material, pins)` is the generic indexed call over `_ARMS`, and the record's mapped annotation `{ readonly [K in StackSpec.Arm]: Dispatch.Arm }` is the exhaustiveness proof — a `StackSpec.arms` entry with no row fails compilation at the record.
- Law: arms prove, never assume — a selfhosted arm lifts `spec.connection` and the SSH key together and an absence mints an `input` fault naming the coordinate; a prepared arm demands `region` the same way; no arm body ever meets an unproven `Option`.
- Law: pins are a parameter, never a module read — `Dispatch.Pins` carries the deploy-time facts the spec does not (chart/operator versions, the extension-image ref, the install script, the telemetry suite's encoded boards and alert specs); the app root resolves them from its own config and suite call, so ingress is parameterized end to end and the lib hardcodes no version anywhere.
- Law: one provider seam per arm — the arm constructs its provider (kubeconfig-bound `k8s.Provider`, `ssh://` `docker.Provider`, credentialed cloud provider) exactly once and threads it through tier options; per-resource providers are the named defect, and the credential arrives from `Secrets.read` in-graph or the ambient `doppler run` env, never a literal.
- Law: the `PulumiFn` body is the deploy plane's program seam — a promise-returning composition of tier constructors bound to consts and one returned outputs record; the platform owns that shape, and everything the arm computes before entering it stays on the rail.
- Entry: `Effect.flatMap(Dispatch.material, (material) => Dispatch.program(spec, material, pins))` then `Automation.stack(spec, program)`.
- Growth: one record row plus one surface column per cloud; a new shared deploy-time fact is one `Pins` field, a new shared secret fact is one `material` field.
- Boundary: the equivalence cells an arm realizes are `provider/surface.md`'s; the run and receipt are `program/automation.md`'s; outputs keys are `stack/output.md`'s contract.
- Packages: `effect` (`Config`, `Effect`, `Option`, `Redacted`); `../program/spec.ts` (`StackSpec`); `../program/automation.ts` (`DeployFault`).

```typescript
import type { PulumiFn } from "@pulumi/pulumi/automation"
import { Config, Effect, Option, Redacted } from "effect"
import { DeployFault } from "../program/automation.ts"
import { StackSpec } from "../program/spec.ts"

declare namespace Dispatch {
  type Material = { readonly sshKey: Option.Option<Redacted.Redacted<string>> }
  type Arm = (spec: StackSpec, material: Material, pins: Pins) => Effect.Effect<PulumiFn, DeployFault>
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

## [3]-[ARM_PROGRAMS]

[ARM_PROGRAMS]:
- Law: the k8s arm composes in dependency order — `Bootstrap` (kubeconfig) → `k8s.Provider` + one `Namespace` → `Secrets` (generated entries: `DB_ADMIN_PASSWORD`, `DB_PASSWORD`, `OBJECT_USER`, `OBJECT_PASSWORD`, `GRAFANA_PASSWORD`; `CLOUDFLARE_API_TOKEN` pre-exists on the app's config) → `ObjectStore` → `Postgres` (the admin and app credentials as two distinct reads) → `Lgtm` → `Boards` → `Inject.token` → optional `Workload` with its live-`Output` env pairs (when `spec.image` is present) → `Traffic` over the workload service — and returns every realized `StackOutputs` plane.
- Law: the docker arm is build-plus-runtime — the canonical `docker-build.Image` builds the pins-supplied context and pushes to the spec's target ref (`push: true`; the immutable `ref` egress pins the runtime), the `ssh://` `docker.Provider` binds the proven connection, and the runtime trio is held at declared-signature depth until the nested arg shapes (`ContainerPort`, `envs`, `networksAdvanced`) are catalogued to operator depth.
- Law: prepared arms are provider-seam-complete — `aws` builds its provider from the proven region with credentials ambient under `doppler run`; `gcp` binds `credentials` from the `GCP_CREDENTIALS` fan-in read; `cloudflare` binds `apiToken` from `CLOUDFLARE_API_TOKEN`; each returns its declared realizer's outputs, and the realizer signatures are the settled law the finalization fills.
- Law: every arm funds the boards — the telemetry suite's encoded models and alert specs enter as arm arguments where the arm realizes an observe cell; an arm without the observe cell returns no `grafana` plane and drops nothing silently.
- Growth: promoting a prepared arm is implementing its declared realizer against the catalogued args — the record row, signature, and outputs contract never move.
- Boundary: tier mechanics live on the tier pages; the declared realizers' argument catalogues are the standing research items on the four provider `.api` files.
- Packages: `@pulumi/kubernetes`, `@pulumi/docker`, `@pulumi/docker-build`, `@pulumi/aws`, `@pulumi/gcp`, `@pulumi/cloudflare` (providers + composed classes); every folder tier.

```typescript
import * as aws from "@pulumi/aws"
import * as cloudflare from "@pulumi/cloudflare"
import * as docker from "@pulumi/docker"
import * as dockerBuild from "@pulumi/docker-build"
import * as gcp from "@pulumi/gcp"
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import type { Alert, DashboardModel } from "@rasm/ts/telemetry"
import { Postgres, ObjectStore } from "../kube/data.ts"
import { Traffic } from "../kube/traffic.ts"
import { Workload } from "../kube/workload.ts"
import { Boards } from "../observe/apply.ts"
import { Lgtm } from "../observe/stack.ts"
import { Inject } from "../secret/inject.ts"
import { Secrets } from "../secret/doppler.ts"
import { Bootstrap } from "./surface.ts"

declare namespace Dispatch {
  type Pins = {
    readonly install: string
    readonly pgImage: string
    readonly operator: string
    readonly object: string
    readonly lgtm: string
    readonly collector: string
    readonly port: number
    readonly context: string
    readonly boards: ReadonlyArray<typeof DashboardModel.Encoded>
    readonly alerts: ReadonlyArray<Alert.Spec>
  }
}

declare const _dockerRows: (spec: StackSpec, provider: docker.Provider, image: Option.Option<pulumi.Output<string>>) => Record<string, unknown>
declare const _awsRows: (spec: StackSpec, provider: aws.Provider) => Record<string, unknown>
declare const _gcpRows: (spec: StackSpec, provider: gcp.Provider) => Record<string, unknown>
declare const _cloudflareRows: (spec: StackSpec, provider: cloudflare.Provider) => Record<string, unknown>

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
      const data = new Postgres("data", {
        spec,
        namespace: ns.metadata.name,
        image: pins.pgImage,
        operatorVersion: pins.operator,
        objects,
        auth: { admin: secrets.read("DB_ADMIN_PASSWORD"), app: secrets.read("DB_PASSWORD") },
      }, bound)
      const lgtm = new Lgtm("observe", {
        spec,
        namespace: ns.metadata.name,
        versions: { lgtm: pins.lgtm, collector: pins.collector },
        auth: secrets.read("GRAFANA_PASSWORD"),
      }, bound)
      new Boards("boards", { spec, lgtm, auth: secrets.read("GRAFANA_PASSWORD"), boards: pins.boards, alerts: pins.alerts })
      const token = Inject.token("doppler-token", { namespace: ns.metadata.name, token: secrets.token }, { provider })
      const outputs = {
        data: { host: data.host, port: 5432, database: data.database, role: data.role },
        object: { endpoint: objects.endpoint, bucket: objects.bucket },
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
            env: Inject.rows(token.metadata.name, [
              ["data.host", data.host],
              ["data.port", "5432"],
              ["data.database", data.database],
              ["data.role", data.role],
              ["object.endpoint", objects.endpoint],
              ["object.bucket", objects.bucket],
              ["otlp.endpoint", lgtm.collectorEndpoint],
            ]),
          }, bound)
          const traffic = new Traffic("traffic", {
            spec,
            namespace: ns.metadata.name,
            service: workload.service.metadata.name,
            port: pins.port,
            apiToken: secrets.read("CLOUDFLARE_API_TOKEN"),
          }, bound)
          return { ...outputs, ingress: { hostname: traffic.hostname } }
        },
      })
    }),
  "selfhosted-docker": (spec, material, pins) =>
    Effect.map(_proven(spec, material), (proven) => async () => {
      const provider = new docker.Provider("engine", {
        host: `ssh://${proven.connection.user}@${proven.connection.host}:${proven.connection.port}`,
      })
      const image = Option.map(spec.image, (ref) =>
        new dockerBuild.Image("app", {
          push: true,
          tags: [ref],
          context: { location: pins.context },
        }).ref)
      return _dockerRows(spec, provider, image)
    }),
  aws: (spec, _material, _pins) =>
    Option.match(spec.region, {
      onNone: () => Effect.fail(_input(spec, "<missing-region>")),
      onSome: (region) => Effect.succeed(async () => _awsRows(spec, new aws.Provider("aws", { region }))),
    }),
  gcp: (spec, _material, _pins) =>
    Option.match(spec.region, {
      onNone: () => Effect.fail(_input(spec, "<missing-region>")),
      onSome: (region) =>
        Effect.succeed(async () => {
          const secrets = new Secrets("secrets", { spec, entries: {} })
          const provider = new gcp.Provider("gcp", {
            project: spec.doppler.project,
            region,
            credentials: secrets.read("GCP_CREDENTIALS"),
          })
          return _gcpRows(spec, provider)
        }),
    }),
  cloudflare: (spec, _material, _pins) =>
    Effect.succeed(async () => {
      const secrets = new Secrets("secrets", { spec, entries: {} })
      const provider = new cloudflare.Provider("cf", { apiToken: secrets.read("CLOUDFLARE_API_TOKEN") })
      return _cloudflareRows(spec, provider)
    }),
}

const Dispatch = {
  material: _material,
  program: (spec: StackSpec, material: Dispatch.Material, pins: Dispatch.Pins): Effect.Effect<PulumiFn, DeployFault> =>
    _ARMS[spec.target](spec, material, pins),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Dispatch }
```
