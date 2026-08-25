# [IAC_CONVERGE]

`Converge` realizes one `Backend.Projection` as an unpublished target whichever branch minted it, orders native materialization, hydration, and proof runners, and publishes only after Kubernetes reports the proof Job complete. The generated contract document remains an immutable input; provider runners retain EF, Marten, SQL, replay, copy, and replication execution. Stable pointer names retain generation evidence; one Automation stack serializes its atomic pointer writes.

## [01]-[INDEX]

- [02]-[PROJECTION]: generated document, expected identity, runner and target inputs; `Converge`.
- [03]-[RUNNER_FOLD]: ordered materialize, hydrate, and observed-proof Jobs; `Converge`.
- [04]-[PUBLICATION]: retained evidence and the generation pointer; `Converge`.

## [02]-[PROJECTION]

- Owner: `Converge.admit` is the one entry — it proves the served topology and constructs the tier from the application composition owner's final projection; IaC never merges, re-decodes, or re-hashes generated artifacts.
- Cases: one already-merged `Backend.Projection` realizes directly; single- and multi-language applications differ only at the upstream composition owner and enter this deployment tier identically.
- Law: `service` and `edge` are the topology values this tier serves out of the closed roster `program/spec.md` owns; an in-host, sidecar, companion, or cli composition carries no cluster to converge against and refuses at admission with `ConvergeRefused` naming the axis and the rejected value.
- Law: deployment shape arrives as one caller-supplied row carrying `topology` beside the `objective` it declares, which `StackSpec.Profile` satisfies structurally off `program/spec`'s derived getter, so a composition root outside this estate supplies its own pair — this tier reads deployment shape and infers none.
- Law: the generated document owns its contract coordinate; the publication name remains a Kubernetes pointer coordinate and never enters contract composition.
- Law: one immutable ConfigMap carries `contract.json`, the generated backend message's official ProtoJSON document; validation remains descriptor-owned and no schema or conformance sidecar exists.
- Law: caller-owned provider resources arrive as one readiness dependency and environment coordinates.
- Law: one image and command own every phase; the phase table carries only argument vectors.
- Boundary: the generated document and deployment fence schedule work; only a completed proof Job admits publication.

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Backend } from "@rasm/data"
import { Array, Data, Duration, Effect, Encoding, Record } from "effect"
import { StackOutputs, Tier, type StackSpec } from "../program/spec.ts"

// --- [TYPES] ---------------------------------------------------------------------------

const _PHASES = ["materialize", "hydrate", "prove"] as const
const _PROOF = "prove" satisfies Converge.Phase
const _GENERATION = "RASM_BACKEND_GENERATION"
const _FENCE = "RASM_BACKEND_DEPLOYMENT_FENCE"
const _RPO = "RASM_BACKEND_RPO_MILLIS"
const _RTO = "RASM_BACKEND_RTO_MILLIS"
const _TOPOLOGY = {
  "in-host": "refuse",
  sidecar: "refuse",
  companion: "refuse",
  service: "serve",
  edge: "serve",
  cli: "refuse",
} as const satisfies Record.ReadonlyRecord<Converge.Topology, "serve" | "refuse">

declare namespace Converge {
  type Phase = (typeof _PHASES)[number]
  type Step = Phase
  type Topology = StackSpec.Topology
  type Profile = {
    readonly topology: Topology
    readonly objective: Backend.Objective
  }
  type Runner = {
    readonly image: pulumi.Input<string>
    readonly command: ReadonlyArray<string>
    readonly args: Readonly<Record<Step, ReadonlyArray<string>>>
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
    readonly backend: Backend.Projection
    readonly runner: Runner
    readonly target: Target
    readonly publication: Publication
  }
}

// --- [ERRORS] --------------------------------------------------------------------------

class ConvergeRefused extends Data.TaggedError("ConvergeRefused")<{
  readonly axis: "topology"
  readonly value: Converge.Topology
}> {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _contractFiles = (files: Backend.Files): Record.ReadonlyRecord<string, string> => ({
  "contract.json": Encoding.encodeBase64(files.contract),
})

const _meta = <K extends "name" | "uid">(
  resource: { readonly metadata: pulumi.Output<k8s.types.output.meta.v1.ObjectMeta> },
  field: K,
): pulumi.Output<string> => resource.metadata.apply((meta) => meta[field] as string)

const _served = (profile: Converge.Profile): Effect.Effect<void, ConvergeRefused> =>
  _TOPOLOGY[profile.topology] === "serve"
    ? Effect.void
    : Effect.fail(new ConvergeRefused({ axis: "topology", value: profile.topology }))
```

## [03]-[RUNNER_FOLD]

- Owner: `_runner` lowers one ordered `_PHASES` member into a single-attempt Job mounted against the immutable contract document, with the phase's own env rows appended to the shared set.
- Law: provider-native owners create an empty target and apply their framework artifacts.
- Law: canonical journals, event stores, objects, or typed copy projections populate the target.
- Law: runner reads realized catalogs and data frontiers, builds `Backend.Observation`, and exits only after `Backend.admit` grades it against the objective this fold supplies — the deploy plane owns the recovery target, so the runner measures a window and never sets the bar it is judged against.
- Law: `backoffLimit: 0` and `activeDeadlineSeconds` preserve one terminal observation per deployment attempt.
- Law: a runner pod carries `Tier.harden` like every other pod this estate declares — the phases hold the target's own libpq credentials and author every relation the application then binds, so the one pod family with DDL authority is the last that may run root-capable on a writable root filesystem; the anchor's scratch pair is what keeps the read-only root survivable for the runner's own temp writes.
- Auto: `Array.mapAccum` threads target readiness through `materialize → hydrate → prove`, mapping each phase to its `[phase, job]` pair, so the fold yields a phase-keyed record and no callback graph or parallel plan exists.
- Packages: `@pulumi/kubernetes` typed `ConfigMap` and `Job`; `@pulumi/pulumi` resource dependency algebra.

```typescript
const _runner = (
  name: string,
  step: Converge.Step,
  projection: Backend.Projection,
  args: Converge.Args,
  contract: k8s.core.v1.ConfigMap,
  after: pulumi.Resource,
  carried: ReadonlyArray<k8s.types.input.core.v1.EnvVar>,
  opts: pulumi.CustomResourceOptions,
): k8s.batch.v1.Job =>
  new k8s.batch.v1.Job(`${name}-${step}-${projection.contract.id.slice(0, 8)}`, {
    metadata: {
      namespace: args.namespace,
      labels: {
        "app.kubernetes.io/name": name,
        "app.kubernetes.io/component": "backend-convergence",
      },
      annotations: {
        "rasm.dev/backend-generation": projection.contract.id,
        "rasm.dev/backend-step": step,
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
          securityContext: Tier.harden.pod,
          containers: [{
            name: step,
            image: args.runner.image,
            command: [...args.runner.command],
            args: [...args.runner.args[step]],
            env: [
              ...args.runner.env,
              ...args.target.env,
              { name: _GENERATION, value: projection.contract.id },
              { name: StackOutputs.backend.root, value: args.runner.contractRoot },
              { name: _FENCE, value: args.publication.fence },
              { name: _RPO, value: `${Duration.toMillis(args.profile.objective.rpo)}` },
              { name: _RTO, value: `${Duration.toMillis(args.profile.objective.rto)}` },
              ...carried,
            ],
            securityContext: Tier.harden.container,
            volumeMounts: [
              { name: "contract", mountPath: args.runner.contractRoot, readOnly: true },
              ...Tier.harden.mounts,
            ],
          }],
          volumes: [
            { name: "contract", configMap: { name: contract.metadata.name } },
            ...Tier.harden.volumes,
          ],
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
- Boundary: this tier ends at the retained pointer. Workload mounting is deploy-owned; local decode, generation comparison, and readiness admission belong to the application runtime and are not claimed by this page.

```typescript
// --- [COMPOSITION] ---------------------------------------------------------------------

class Converge extends Tier {
  readonly contract: k8s.core.v1.ConfigMap
  readonly pointer: k8s.core.v1.ConfigMap
  readonly evidence: k8s.core.v1.ConfigMap

  static admit(
    name: string,
    args: Converge.Args,
    opts?: pulumi.ComponentResourceOptions,
  ): Effect.Effect<Converge, ConvergeRefused> {
    return Effect.gen(function* () {
      yield* _served(args.profile)
      return new Converge(name, args.backend, args, opts)
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
    const [, ordered] = Array.mapAccum(
      _PHASES,
      args.target.ready,
      (after, phase) => {
        const job = _runner(name, phase, projection, args, this.contract, after, [], this.child())
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Converge, ConvergeRefused }
```
