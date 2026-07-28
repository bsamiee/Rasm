# [IAC_CONVERGE]

`Converge` realizes one `Backend.Projection` as an unpublished target whichever branch minted it, orders native materialization, hydration, and proof runners, and publishes only after Kubernetes reports the proof Job complete. Generated contract files remain immutable inputs; provider runners retain EF, Marten, SQL, replay, copy, and replication execution. Stable pointer names retain generation evidence; one Automation stack serializes its atomic pointer writes.

## [01]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                       | [PUBLIC]   |
| :-----: | :------------ | :----------------------------------------------------------- | :--------- |
|  [01]   | `PROJECTION`  | generated files, expected identity, runner and target inputs | `Converge` |
|  [02]   | `RUNNER_FOLD` | ordered materialize, hydrate, and observed-proof Jobs        | `Converge` |
|  [03]   | `PUBLICATION` | retained evidence and the generation pointer after proof     | `Converge` |

## [02]-[PROJECTION]

- Owner: `Converge.admit` is the one entry — it proves the served topology, folds the backend input to one projection, and constructs the tier; IaC never re-decodes or re-hashes generated artifacts.
- Cases: the input discriminates on shape — one `Backend.Projection` realizes directly, an array of branch contributions folds through `Backend.merge` first, so a single-language and a three-language application enter identically.
- Law: `service` and `edge` are the topology values this tier serves out of the closed roster `program/spec.md` owns; an in-host, sidecar, companion, or cli composition carries no cluster to converge against and refuses at admission with `ConvergeRefused` naming the axis and the rejected value.
- Law: the topology arrives as one caller-supplied row, so a `StackSpec.Profile` satisfies it structurally and a composition root outside this estate supplies its own — the tier reads deployment shape and infers none.
- Law: one immutable ConfigMap carries `contract.json`, `contract.schema.json`, and `contract.conformance.json`.
- Law: caller-owned provider resources arrive as one readiness dependency and environment coordinates.
- Law: one image and command own every phase; the phase table carries only argument vectors.
- Boundary: generated files and the deployment fence schedule work; only a completed proof Job admits publication.

```typescript signature
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Backend, BackendFault } from "@rasm/ts/data"
import { Array, Data, Effect, Encoding, Record } from "effect"
import { Tier, type StackSpec } from "../program/spec.ts"

// --- [TYPES] ----------------------------------------------------------------------------

const _PHASES = ["materialize", "hydrate", "prove"] as const
// `_PROOF` names the gating phase, so appending a phase never re-points publication, and a rename
// breaks at this binding because the literal must inhabit the tuple.
const _PROOF = "prove" satisfies Converge.Phase
const _GENERATION = "RASM_BACKEND_GENERATION"
const _CONTRACT_ROOT = "RASM_BACKEND_CONTRACT_ROOT"
const _FENCE = "RASM_BACKEND_DEPLOYMENT_FENCE"
// Convergence realizes onto a cluster, so the deploy plane serves the two topology values that
// carry one; the roster stays whole in the type so a rejected value is representable in the fault.
const _SERVED: ReadonlySet<Converge.Topology> = new Set(["service", "edge"])

declare namespace Converge {
  type Phase = (typeof _PHASES)[number]
  type Topology = StackSpec.Topology
  type Profile = { readonly topology: Topology }
  type Runner = {
    readonly image: pulumi.Input<string>
    readonly command: ReadonlyArray<string>
    readonly args: Readonly<Record<Phase, ReadonlyArray<string>>>
    readonly env: ReadonlyArray<k8s.types.input.core.v1.EnvVar>
    readonly serviceAccountName: pulumi.Input<string>
    readonly contractRoot: string
    readonly deadlineSeconds: number
  }
  type Target = {
    readonly ready: pulumi.Resource
    readonly env: ReadonlyArray<k8s.types.input.core.v1.EnvVar>
  }
  type Publication = {
    readonly name: string
    readonly fence: pulumi.Input<string>
  }
  type Args = {
    readonly namespace: pulumi.Input<string>
    readonly profile: Profile
    readonly backend: Backend.Projection | ReadonlyArray<Backend.Projection>
    readonly runner: Runner
    readonly target: Target
    readonly publication: Publication
  }
}

// --- [ERRORS] ---------------------------------------------------------------------------

class ConvergeRefused extends Data.TaggedError("ConvergeRefused")<{
  readonly axis: "topology"
  readonly value: Converge.Topology
}> {}

// --- [OPERATIONS] -----------------------------------------------------------------------

// `binaryData` takes standard-alphabet base64; the codec rides the substrate rail, so the deploy
// plane composes on no host global and stays admissible in every runtime an operator drives it from.
const _contractFiles = (files: Backend.Files): Record.ReadonlyRecord<string, string> => ({
  "contract.json": Encoding.encodeBase64(files.contract),
  "contract.schema.json": Encoding.encodeBase64(files.schema),
  "contract.conformance.json": Encoding.encodeBase64(files.conformance),
})

// Server-assigned `ObjectMeta` fields are optional in the input schema and always present on a
// created resource; one projection states that once, so no evidence row defaults to an empty
// string and records a proof that never ran.
const _meta = <K extends "name" | "uid">(
  resource: { readonly metadata: pulumi.Output<k8s.types.output.meta.v1.ObjectMeta> },
  field: K,
): pulumi.Output<string> => resource.metadata.apply((meta) => meta[field] as string)

const _served = (profile: Converge.Profile): Effect.Effect<void, ConvergeRefused> =>
  _SERVED.has(profile.topology)
    ? Effect.void
    : Effect.fail(new ConvergeRefused({ axis: "topology", value: profile.topology }))

// One backend input, two shapes: a single contribution realizes as it stands, a branch set folds
// through the deterministic merge — so no per-composition entry and no boolean knob exists.
const _projected = (
  backend: Converge.Args["backend"],
  contract: string,
): Effect.Effect<Backend.Projection, BackendFault> =>
  Array.isArray(backend) ? Backend.merge(backend, contract) : Effect.succeed(backend)
```

## [03]-[RUNNER_FOLD]

- Owner: `_runner` lowers one phase row into a single-attempt Job mounted against the immutable generated files.
- Law: provider-native owners create an empty target and apply their framework artifacts.
- Law: canonical journals, event stores, objects, or typed copy projections populate the target.
- Law: runner reads realized catalogs and data frontiers, builds `Backend.Observation`, and exits only after `Backend.admit`.
- Law: `backoffLimit: 0` and `activeDeadlineSeconds` preserve one terminal observation per deployment attempt.
- Auto: `Array.mapAccum` threads target readiness through `materialize → hydrate → prove`, mapping each phase to its `[phase, job]` pair, so the fold yields a phase-keyed record and no callback graph or parallel plan exists.
- Packages: `@pulumi/kubernetes` typed `ConfigMap` and `Job`; `@pulumi/pulumi` resource dependency algebra.

```typescript signature
const _runner = (
  name: string,
  phase: Converge.Phase,
  projection: Backend.Projection,
  args: Converge.Args,
  contract: k8s.core.v1.ConfigMap,
  after: pulumi.Resource,
  opts: pulumi.CustomResourceOptions,
): k8s.batch.v1.Job =>
  new k8s.batch.v1.Job(`${name}-${phase}-${projection.contract.id.slice(0, 8)}`, {
    metadata: {
      namespace: args.namespace,
      labels: {
        "app.kubernetes.io/name": name,
        "app.kubernetes.io/component": "backend-convergence",
      },
      annotations: {
        "rasm.dev/backend-generation": projection.contract.id,
        "rasm.dev/backend-phase": phase,
        "rasm.dev/backend-fence": args.publication.fence,
      },
    },
    spec: {
      activeDeadlineSeconds: args.runner.deadlineSeconds,
      backoffLimit: 0,
      completions: 1,
      parallelism: 1,
      template: {
        metadata: {
          labels: {
            "app.kubernetes.io/name": name,
            "app.kubernetes.io/component": "backend-convergence",
          },
        },
        spec: {
          restartPolicy: "Never",
          serviceAccountName: args.runner.serviceAccountName,
          containers: [{
            name: phase,
            image: args.runner.image,
            command: [...args.runner.command],
            args: [...args.runner.args[phase]],
            env: [
              ...args.runner.env,
              ...args.target.env,
              { name: _GENERATION, value: projection.contract.id },
              { name: _CONTRACT_ROOT, value: args.runner.contractRoot },
              { name: _FENCE, value: args.publication.fence },
            ],
            volumeMounts: [{
              name: "contract",
              mountPath: args.runner.contractRoot,
              readOnly: true,
            }],
          }],
          volumes: [{
            name: "contract",
            configMap: { name: contract.metadata.name },
          }],
        },
      },
    },
  }, pulumi.mergeOptions(opts, { dependsOn: [after] }))
```

## [04]-[PUBLICATION]

- Law: one stable ConfigMap publishes the evidence name only after proof completion; its update is the atomic cutover.
- Law: `_PROOF` names the gating phase, so the publication reads `jobs[_PROOF]` by identity and a phase appended to `_PHASES` extends the fold without re-pointing what publication waits on.
- Law: one Automation stack owns the pointer; its serialized update and one ConfigMap write close the publication edge.
- Law: a retained predecessor re-enters the same proof fold before pointer selection; no bypassing rollback verb exists.
- Law: contract and evidence ConfigMaps use `retainOnDelete`; policy-driven collection owns expiry outside this tier.
- Receipt: one immutable retained ConfigMap records generation, contract ConfigMap name, proof Job UID, and deployment fence.
- Boundary: service and worker readiness compare the pointer with their locally admitted `Backend.Generation`.

```typescript signature
// --- [COMPOSITION] ----------------------------------------------------------------------

class Converge extends Tier {
  readonly contract: k8s.core.v1.ConfigMap
  readonly pointer: k8s.core.v1.ConfigMap
  readonly evidence: k8s.core.v1.ConfigMap

  // `admit` is the one entry: the axis proof and the backend fold run on the typed rail before a
  // single resource is declared, so an unserved topology never half-constructs a tier.
  static admit(
    name: string,
    args: Converge.Args,
    opts?: pulumi.ComponentResourceOptions,
  ): Effect.Effect<Converge, ConvergeRefused | BackendFault> {
    return Effect.gen(function* () {
      yield* _served(args.profile)
      const projection = yield* _projected(args.backend, args.publication.name)
      return new Converge(name, projection, args, opts)
    })
  }

  private constructor(
    name: string,
    projection: Backend.Projection,
    args: Converge.Args,
    opts?: pulumi.ComponentResourceOptions,
  ) {
    super("Converge", name, opts)
    const retained = this.child({ retainOnDelete: true })
    this.contract = new k8s.core.v1.ConfigMap(`${name}-contract`, {
      metadata: {
        namespace: args.namespace,
        annotations: { "rasm.dev/backend-generation": projection.contract.id },
      },
      immutable: true,
      binaryData: _contractFiles(projection.files),
    }, retained)
    // mapAccum threads readiness and returns [threadedState, mappedArray]; the accumulator is the
    // readiness resource, which carries no ObjectMeta, so the mapped pairs are the whole product.
    const [, ordered] = Array.mapAccum(
      _PHASES,
      args.target.ready,
      (after, phase) => {
        const job = _runner(name, phase, projection, args, this.contract, after, this.child())
        return [job, [phase, job] as const] as const
      },
    )
    const jobs: Readonly<Record<Converge.Phase, k8s.batch.v1.Job>> = Record.fromEntries(ordered)
    const proof = jobs[_PROOF]
    this.evidence = new k8s.core.v1.ConfigMap(`${name}-evidence`, {
      metadata: {
        namespace: args.namespace,
        annotations: { "rasm.dev/backend-generation": projection.contract.id },
      },
      immutable: true,
      data: {
        generation: projection.contract.id,
        contract: _meta(this.contract, "name"),
        proof: _meta(proof, "uid"),
        fence: args.publication.fence,
      },
    }, pulumi.mergeOptions(retained, { dependsOn: [proof] }))
    this.pointer = new k8s.core.v1.ConfigMap(args.publication.name, {
      metadata: { name: args.publication.name, namespace: args.namespace },
      data: {
        generation: projection.contract.id,
        evidence: _meta(this.evidence, "name"),
        fence: args.publication.fence,
      },
    }, this.child({ dependsOn: [this.evidence] }))
    this.seal({
      generation: projection.contract.id,
      evidence: _meta(this.evidence, "name"),
      pointer: _meta(this.pointer, "name"),
    })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Converge, ConvergeRefused }
```

## [05]-[RESEARCH]

(none)
