# [IAC_CONVERGE]

`Converge` realizes one `Backend.Projection` as an unpublished target whichever branch minted it, orders native materialization, hydration, and proof runners, and publishes only after Kubernetes reports the proof Job complete. Generated contract files remain immutable inputs; provider runners retain EF, Marten, SQL, replay, copy, and replication execution. Stable pointer names retain generation evidence; one Automation stack serializes its atomic pointer writes.

Rollout strategy governs cutover as data: `_STAGED` holds one row per member of the closed `immediate | canary | bluegreen` vocabulary the publication row reads off the spec coordinate, each row carrying the candidate's staged Gateway weight and the gate whose verdict the pointer waits on. Staging precedes the cutover whole — a candidate serves behind a weighted route while the pointer keeps naming the incumbent — so the pointer write stays the one atomic cutover and the entire rollback story, and abort retires the staged set without moving the pointer.

## [01]-[INDEX]

- [02]-[PROJECTION]: generated files, expected identity, runner and target inputs; `Converge`.
- [03]-[RUNNER_FOLD]: ordered materialize, hydrate, and observed-proof Jobs; `Converge`.
- [04]-[PUBLICATION]: rollout strategy rows, retained evidence, and the generation pointer; `Converge`.

## [02]-[PROJECTION]

- Owner: `Converge.admit` is the one entry — it proves the served topology, folds the backend input to one projection, and constructs the tier; IaC never re-decodes or re-hashes generated artifacts.
- Cases: the input discriminates on shape — one `Backend.Projection` realizes directly, an array of branch contributions folds through `Backend.merge` first, so a single-language and a three-language application enter identically.
- Law: `service` and `edge` are the topology values this tier serves out of the closed roster `program/spec.md` owns; an in-host, sidecar, companion, or cli composition carries no cluster to converge against and refuses at admission with `ConvergeRefused` naming the axis and the rejected value.
- Law: deployment shape arrives as one caller-supplied row carrying `topology` beside the `objective` it declares, which `StackSpec.Profile` satisfies structurally off `program/spec`'s derived getter, so a composition root outside this estate supplies its own pair — this tier reads deployment shape and infers none.
- Law: cutover shape rides that same profile row as `rollout` — the closed `immediate | canary | bluegreen` coordinate `program/spec#SPEC_OWNER` owns and defaults to `immediate` — so topology, objective, and cutover posture reach this tier as one value and no second argument carries deployment shape; the publication row carries `staged` beside it, because the candidate services a strategy fronts are per-deployment coordinates the spec never holds.
- Law: one immutable ConfigMap carries `contract.json`, `contract.schema.json`, and `contract.conformance.json`.
- Law: caller-owned provider resources arrive as one readiness dependency and environment coordinates.
- Law: one image and command own every phase; the phase table carries only argument vectors.
- Boundary: generated files and the deployment fence schedule work; only a completed proof Job admits publication.

```typescript signature
import * as k8s from "@pulumi/kubernetes"
import * as pulumi from "@pulumi/pulumi"
import { Backend, BackendFault } from "@rasm/ts/data"
import { Array, Data, Duration, Effect, Encoding, Option, Record } from "effect"
import { Tier, type StackSpec } from "../program/spec.ts"

// --- [TYPES] ----------------------------------------------------------------------------

const _PHASES = ["materialize", "hydrate", "prove"] as const
// `_PROOF` names the gating phase, so appending a phase never re-points publication, and a rename
// breaks at this binding because the literal must inhabit the tuple.
const _PROOF = "prove" satisfies Converge.Phase
// `_ADMIT` names a STEP, never a phase: `_PHASES` is the ordered chain converging the TARGET, while `admit` runs
// after that chain against a candidate the cluster already fronts, so seating it in the tuple threads target
// readiness through a step measuring live traffic.
const _ADMIT = "admit" as const
const _GENERATION = "RASM_BACKEND_GENERATION"
const _CONTRACT_ROOT = "RASM_BACKEND_CONTRACT_ROOT"
const _FENCE = "RASM_BACKEND_DEPLOYMENT_FENCE"
// Recovery OBJECTIVE crosses as data the deploy plane supplies, never a runner default: `Backend.admit`
// grades the measured window against it, so a runner inventing one grades against a target nobody set.
const _RPO = "RASM_BACKEND_RPO_MILLIS"
const _RTO = "RASM_BACKEND_RTO_MILLIS"
// Rollout PREDICATE crosses exactly as the recovery objective does — as data the strategy row supplies to a step
// evaluating it in-cluster and exiting on the verdict — so no Pulumi process ever reads live traffic, and one
// gate exit is the only signal the pointer waits on.
const _ROLLOUT = "RASM_BACKEND_ROLLOUT"
const _ROSTER = "RASM_BACKEND_ROLLOUT_ROSTER"
const _SHARE = "RASM_BACKEND_ROLLOUT_SHARE"
const _WINDOW = "RASM_BACKEND_ROLLOUT_WINDOW_MILLIS"
const _CEILING = "RASM_BACKEND_ROLLOUT_ERROR_PPM"
const _FLOOR = "RASM_BACKEND_ROLLOUT_SAMPLE_FLOOR"
// Convergence realizes onto a cluster, so the deploy plane serves the two topology values that
// carry one; the roster stays whole in the type so a rejected value is representable in the fault.
const _SERVED: ReadonlySet<Converge.Topology> = new Set(["service", "edge"])

declare namespace Converge {
  type Phase = (typeof _PHASES)[number]
  type Step = Phase | typeof _ADMIT
  type Topology = StackSpec.Topology
  type Rollout = StackSpec.Rollout
  // Candidate verdicts differ in KIND across the strategies, not merely in threshold, so the gate is a case
  // family rather than one widened row: `Share` grades traffic the candidate already carries, `Parity` grades a
  // whole candidate roster carrying none, and `Direct` grades nothing because no candidate ever stages.
  type Gate = Data.TaggedEnum<{
    Direct: {}
    Share: {
      readonly share: number
      readonly window: Duration.Duration
      readonly ceiling: number
      readonly floor: number
    }
    Parity: { readonly share: number; readonly window: Duration.Duration }
  }>
  // Three members, structurally satisfied rather than imported: `StackSpec.Profile` carries `topology` and `rollout`
  // as fields and `objective` as the getter resolving `runtime/proc/config`'s topology table, so a `StackSpec`
  // deployment passes its profile row straight in while a foreign composition root states the same trio. Naming
  // `StackSpec.Profile`
  // here instead would bind this tier to one spec shape, and `Backend.Objective` keeps the grader's own spelling
  // on the member `Backend.admit` reads.
  type Profile = {
    readonly topology: Topology
    readonly objective: Backend.Objective
    readonly rollout: Rollout
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
    // Candidate services this generation fronts while it stages: one member under `canary`, the incumbent's
    // whole parallel set under `bluegreen`, empty under `immediate`, which stages nothing at all.
    readonly staged: ReadonlyArray<pulumi.Input<string>>
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

- Owner: `_runner` lowers one step — an ordered `_PHASES` member or the rollout gate — into a single-attempt Job mounted against the immutable generated files, with the step's own env rows appended to the shared set, so the gate is a Job of the same family the phases are and no second runner form exists.
- Law: provider-native owners create an empty target and apply their framework artifacts.
- Law: canonical journals, event stores, objects, or typed copy projections populate the target.
- Law: runner reads realized catalogs and data frontiers, builds `Backend.Observation`, and exits only after `Backend.admit` grades it against the objective this fold supplies — the deploy plane owns the recovery target, so the runner measures a window and never sets the bar it is judged against.
- Law: `backoffLimit: 0` and `activeDeadlineSeconds` preserve one terminal observation per deployment attempt.
- Law: a runner pod carries `Tier.harden` like every other pod this estate declares — the phases hold the target's own libpq credentials and author every relation the application then binds, so the one pod family with DDL authority is the last that may run root-capable on a writable root filesystem; the anchor's scratch pair is what keeps the read-only root survivable for the runner's own temp writes.
- Auto: `Array.mapAccum` threads target readiness through `materialize → hydrate → prove`, mapping each phase to its `[phase, job]` pair, so the fold yields a phase-keyed record and no callback graph or parallel plan exists.
- Packages: `@pulumi/kubernetes` typed `ConfigMap` and `Job`; `@pulumi/pulumi` resource dependency algebra.

```typescript signature
const _runner = (
  name: string,
  step: Converge.Step,
  projection: Backend.Projection,
  args: Converge.Args,
  contract: k8s.core.v1.ConfigMap,
  after: pulumi.Resource,
  // Step-local coordinates: empty for every ordered phase, the strategy row's own predicate for the gate — so the
  // env set stays one list and a predicate never leaks into a phase that has no verdict to reach.
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
          // The one pod family holding DDL authority over the target, so it carries the estate's own posture
          // rather than the API's defaults; the anchor's scratch pair keeps the read-only root survivable.
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
              { name: _CONTRACT_ROOT, value: args.runner.contractRoot },
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
- Law: the rollout vocabulary is `_STAGED`, one gate case per strategy under a mapped type keyed by the spec coordinate, so a fourth strategy is a compile error until it carries a case; the fold is the only reader of the literal, and `Converge.share` is its one projection outward — `Option`-carried, so `immediate` hands the edge no weight row rather than a zero the edge must interpret.
- Law: staging never writes the pointer — a candidate admits against the stable `-staged` ConfigMap this tier mints while the pointer proper keeps naming the incumbent for the whole window, so admission evidence compares the candidate under live traffic against a published incumbent, and the pointer's single write stays the whole cutover.
- Law: abort is structural, never a verb — the staged pair carries no `retainOnDelete` while the contract, evidence, and pointer trio does, so a refused generation retires when the next apply drops its weight row and staging pointer, the incumbent is untouched because nothing ever moved it, and no second entry, manual runbook, or rollback path exists to keep in agreement.
- Law: the pointer's `dependsOn` folds the strategy's own gate in where one stages and reads the evidence alone where none does, so every strategy converges on one atomic write and a refused gate fails the apply before that write is ever reached.
- Law: the sample floor is what keeps a canary honest against its own route — a window observing fewer than `floor` candidate requests cannot admit, so a gate racing the weighted route's propagation holds on an empty measurement instead of passing on one.
- Law: bluegreen holds what canary does not — the candidate's WHOLE parallel roster stages at weight `0`, so parity is graded dark and no fraction of live traffic ever reaches it, the instant swap IS the pointer flip rather than a second weight step, and its abort predicate is a roster member failing parity rather than an error share no request produced.
- Receipt: one immutable retained ConfigMap records generation, contract ConfigMap name, proof Job UID, and deployment fence; gate evidence rides that same vocabulary because the gate is a `_runner` Job like every phase, so its steps, generation annotation, and terminal state reach `RunReceipt` and the `operate/policy#EVIDENCE_SPINE` rows through the one receipt stream with no rollout-local form.
- Boundary: service and worker readiness compare the pointer with their locally admitted `Backend.Generation`, and a staged candidate compares the staging pointer for the same reason; the weighted route carrying `share` is `kube/traffic#EDGE_REALIZE`'s split row, and the serialized pointer write is `program/automation#AUTOMATION_RUN`'s ledger, unchanged by any strategy.

```typescript signature
// --- [POLICIES] -------------------------------------------------------------------------

const Gate = Data.taggedEnum<Converge.Gate>()

// One strategy fold owns every variation: each row carries the candidate's staged weight and the verdict the
// pointer waits on, and the mapped key proves the table total against the spec's own vocabulary.
const _STAGED: { readonly [K in Converge.Rollout]: Converge.Gate } = {
  // Nothing stages, no weight row is written, and the proof Job is the whole admission — the default strategy
  // publishes on exactly the evidence this tier published on before any strategy existed.
  immediate: Gate.Direct(),
  // Candidate carries a tenth of live traffic. Admission takes the conjunction — the window stayed under the
  // error ceiling AND observed the sample floor — so abort is either arm failing: an error share above the
  // ceiling, or a window too thin to have measured anything a verdict rests on.
  canary: Gate.Share({ share: 10, window: Duration.minutes(15), ceiling: 2_000, floor: 500 }),
  // Weight `0` resolves the candidate's entry and forwards it no request, so the whole roster proves itself dark
  // and the swap is the pointer flip alone; abort is any staged member failing parity across the window.
  bluegreen: Gate.Parity({ share: 0, window: Duration.minutes(5) }),
}

// Staged pair is the candidate's entire existence before the cutover: the admission coordinate it becomes ready
// against, and the gate whose exit is the strategy's verdict. Neither takes `retainOnDelete`, which is abort in
// full — the next apply dropping this pair retires the generation with no line ever naming rollback.
const _gated = (
  name: string,
  projection: Backend.Projection,
  args: Converge.Args,
  contract: k8s.core.v1.ConfigMap,
  evidence: k8s.core.v1.ConfigMap,
  after: pulumi.Resource,
  predicate: ReadonlyArray<k8s.types.input.core.v1.EnvVar>,
  child: pulumi.CustomResourceOptions,
): k8s.batch.v1.Job => {
  const staging = new k8s.core.v1.ConfigMap(`${args.publication.name}-staged`, {
    metadata: { name: `${args.publication.name}-staged`, namespace: args.namespace },
    data: {
      generation: projection.contract.id,
      evidence: _meta(evidence, "name"),
      fence: args.publication.fence,
    },
  }, child)
  return _runner(name, _ADMIT, projection, args, contract, after, [
    { name: _ROLLOUT, value: args.profile.rollout },
    { name: _ROSTER, value: pulumi.all(args.publication.staged).apply((rows) => rows.join(",")) },
    ...predicate,
  ], pulumi.mergeOptions(child, { dependsOn: [staging] }))
}

const _staging = (
  name: string,
  projection: Backend.Projection,
  args: Converge.Args,
  contract: k8s.core.v1.ConfigMap,
  evidence: k8s.core.v1.ConfigMap,
  after: pulumi.Resource,
  child: pulumi.CustomResourceOptions,
): Option.Option<k8s.batch.v1.Job> =>
  Gate.$match(_STAGED[args.profile.rollout], {
    Direct: () => Option.none(),
    Share: (row) =>
      Option.some(_gated(name, projection, args, contract, evidence, after, [
        { name: _SHARE, value: `${row.share}` },
        { name: _WINDOW, value: `${Duration.toMillis(row.window)}` },
        { name: _CEILING, value: `${row.ceiling}` },
        { name: _FLOOR, value: `${row.floor}` },
      ], child)),
    Parity: (row) =>
      Option.some(_gated(name, projection, args, contract, evidence, after, [
        { name: _SHARE, value: `${row.share}` },
        { name: _WINDOW, value: `${Duration.toMillis(row.window)}` },
      ], child)),
  })

// --- [COMPOSITION] ----------------------------------------------------------------------

class Converge extends Tier {
  readonly contract: k8s.core.v1.ConfigMap
  readonly pointer: k8s.core.v1.ConfigMap
  readonly evidence: k8s.core.v1.ConfigMap
  readonly gate: Option.Option<k8s.batch.v1.Job>

  // Fold projects outward exactly once: the edge tier writes the weight, so it reads the candidate's share here
  // rather than the strategy literal. `none` writes no second backendRef at all, which is why `immediate` leaves
  // that route the single-backend shape it carried before a strategy vocabulary existed.
  static readonly share = (rollout: Converge.Rollout): Option.Option<number> =>
    Gate.$match(_STAGED[rollout], {
      Direct: () => Option.none(),
      Share: ({ share }) => Option.some(share),
      Parity: ({ share }) => Option.some(share),
    })

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
    this.gate = _staging(name, projection, args, this.contract, this.evidence, proof, this.child())
    this.pointer = new k8s.core.v1.ConfigMap(args.publication.name, {
      metadata: { name: args.publication.name, namespace: args.namespace },
      data: {
        generation: projection.contract.id,
        evidence: _meta(this.evidence, "name"),
        fence: args.publication.fence,
      },
      // Pointer holds the ONE cutover, and this is the one line a strategy reaches: a staged generation seats
      // its gate here, so the write lands on a passed verdict alone and a refusal fails the apply with the
      // incumbent still named.
    }, this.child({ dependsOn: [this.evidence, ...Option.toArray(this.gate)] }))
    this.seal({
      generation: projection.contract.id,
      rollout: args.profile.rollout,
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
