# [IAC_INJECT]

The runtime-injection law: a deployed process reads secrets from its environment under `doppler run`, and this page owns the entire env assembly that makes that true — the one Kubernetes `Secret` holding `DOPPLER_TOKEN`, the container env rows pairing that token reference with the `StackOutputs` channel pairs, the `doppler run --` entrypoint wrap, and the channel-to-variable key map that is the folder's single env-spelling authority. The `security/secret` read path meets the deploy plane here and only here: the runtime's Doppler client reads the same config this token scopes, the boundary is the process environment, and no import exists in either direction. ESC is the demoted prepared row — `Stack.addEnvironments` attaches Pulumi ESC environments to a stack when an app supplies them, one member, dormant by default. The module is `iac/src/secret/inject.ts`; a new injected fact is one key-map row, and a consumer key rename is one cell edit, never a sweep.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                             | [PUBLIC] |
| :-----: | :-------------- | :------------------------------------------------------------------- | :------- |
|  [01]   | `ENV_ASSEMBLY`  | the key map, the token secret, and the container env rows           | `Inject` |
|  [02]   | `BOUNDARY_LAW`  | the entrypoint wrap, the security seam, and the ESC prepared row    | `Inject` |

## [2]-[ENV_ASSEMBLY]

[ENV_ASSEMBLY]:
- Owner: `Inject` — `_KEYS` is the channel-to-variable map (the one place a channel becomes an environment spelling), `token` provisions the namespace-scoped `core/v1.Secret` carrying `DOPPLER_TOKEN`, and `rows` assembles the container's `EnvVar` list: the token as a `secretKeyRef` (the value never rides the pod spec), every channel pair as a plain `value` row keyed through the map — pair values are `Input`-typed, so live tier `Output`s (the in-program assembly) and decoded `StackOutputs.Pair` strings (the post-run projection) ride one signature.
- Law: the map is total over emitted channels — a `pairs` channel with no `_KEYS` row is dropped by `filterMap` and that drop is deliberate absence, so publishing a new channel to processes is exactly one map row; the sharding channels' final spellings are pinned here once the `work` seam's consumed names are pinned, and the map is where that edit lands.
- Law: coordinates ride plain rows, material rides references — output pairs are non-secret by `StackOutputs.read`'s gate, so they inject as `value`; the only secret-bearing row is the token `secretKeyRef`, and a second secret row is evidence a value bypassed Doppler.
- Entry: `Inject.token(name, { namespace, token: secrets.token }, opts)` once per arm; `Inject.rows(tokenRef, pairs(outputs))` into `kube/workload`'s container env.
- Growth: one `_KEYS` row per new fact; nothing else moves.
- Boundary: the pair emission is `stack/output.md`'s; the container that consumes the rows is `kube/workload.md`'s; the token's mint is `secret/doppler.md`'s.
- Packages: `@pulumi/kubernetes` (`core.v1.Secret`, `types.input.core.v1.EnvVar`); `@pulumi/pulumi` (`Input`, `Output`); `effect` (`Array`, `Option`); `../stack/output.ts` (`StackOutputs`, `pairs`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Array, Option } from "effect"

const _TOKEN = "DOPPLER_TOKEN"
const _KEYS = {
  "ingress.hostname": "IAC_INGRESS_HOSTNAME",
  "data.host": "IAC_DATA_HOST",
  "data.port": "IAC_DATA_PORT",
  "data.database": "IAC_DATA_DATABASE",
  "data.role": "IAC_DATA_ROLE",
  "object.endpoint": "IAC_OBJECT_ENDPOINT",
  "object.bucket": "IAC_OBJECT_BUCKET",
  "otlp.endpoint": "OTEL_EXPORTER_OTLP_ENDPOINT",
  "grafana.url": "IAC_GRAFANA_URL",
  "sharding.host": "IAC_SHARDING_HOST",
  "sharding.port": "IAC_SHARDING_PORT",
} as const

const _keyed: Record<string, string> = _KEYS

declare namespace Inject {
  type Channel = keyof typeof _KEYS
  type EnvRow = k8s.types.input.core.v1.EnvVar
  type Pair = readonly [channel: string, value: pulumi.Input<string>]
  type _Keys<T extends Record<Channel, string> = typeof _KEYS> = T
}

const Inject = {
  keys: _KEYS,
  token: (
    name: string,
    args: { readonly namespace: pulumi.Input<string>; readonly token: pulumi.Input<string> },
    opts?: pulumi.CustomResourceOptions,
  ): k8s.core.v1.Secret =>
    new k8s.core.v1.Secret(name, {
      metadata: { namespace: args.namespace },
      stringData: { [_TOKEN]: args.token },
    }, opts),
  rows: (
    tokenSecret: pulumi.Output<string>,
    outputPairs: ReadonlyArray<Inject.Pair>,
  ): ReadonlyArray<Inject.EnvRow> => [
    { name: _TOKEN, valueFrom: { secretKeyRef: { name: tokenSecret, key: _TOKEN } } },
    ...Array.filterMap(outputPairs, ([channel, value]) =>
      Option.map(Option.fromNullable(_keyed[channel]), (key) => ({ name: key, value }))),
  ],
  entrypoint: (command: ReadonlyArray<string>): ReadonlyArray<string> => ["doppler", "run", "--", ...command],
} as const
```

## [3]-[BOUNDARY_LAW]

[BOUNDARY_LAW]:
- Law: the entrypoint wrap is the injection moment — a container's command is `Inject.entrypoint(cmd)`, so `doppler run` resolves the scoped config into the process environment at start, the app's `host/config` provider chain reads validated values, and the `security/secret` runtime owner leases against the same config; the deploy plane never writes a decrypted payload anywhere a process could read it early.
- Law: the deploy host obeys its own law — the automation process itself runs under `doppler run`, which is how `PULUMI_CONFIG_PASSPHRASE`, the bootstrap `DOPPLER_TOKEN`, and `Dispatch.material` reads resolve; one injection mechanism spans both altitudes.
- Law: ESC is prepared, not default — `esc` attaches Pulumi ESC environments through `Stack.addEnvironments(...names)` when an app supplies them as data; the member exists so adopting ESC is zero structure, and Doppler remains canonical until an app's spec says otherwise.
- Growth: a second injection dialect (a file mount, a cloud secret ref) is one new `Inject` member consuming the same `_KEYS` map.
- Boundary: `Stack` and its triage arrive from `program/automation.md`; the runtime lease/rotation semantics belong to `security/secret` and are invisible here.
- Packages: `@pulumi/pulumi/automation` (`Stack`); `effect` (`Effect`); `../program/automation.ts` (`DeployFault`).

```typescript
import type { Stack } from "@pulumi/pulumi/automation"
import { Effect } from "effect"
import { DeployFault } from "../program/automation.ts"

const esc = (stack: Stack, name: string, environments: ReadonlyArray<string>): Effect.Effect<void, DeployFault> =>
  Effect.tryPromise({
    try: () => stack.addEnvironments(...environments),
    catch: DeployFault.triaged(name),
  })

// --- [EXPORTS] --------------------------------------------------------------------------

export { esc, Inject }
```
