# [IAC_POLICY]

The policy plane in one owner with two verdict directions: `Guard` judges desired state before apply — one `PolicyPackArgs` value of policies-as-data rows, pack-level `mandatory` enforcement with per-policy overrides, attached to every run through `Automation.Options.policyPacks` so no `up` or `preview` executes ungated — and `Drift` judges live state after it, projecting `Automation.reconcile` receipts into `DriftReport` rows: the non-`same` steps, the certificate-rotation watch, and the skew record when the event-folded buckets disagree with the engine's own change summary. Guard policies narrow against the exact resource classes the tier pages construct through the typed helper family — `validateResourceOfType` hands each validator the resource's unwrapped input props at compile-checked depth, `validateStackResourcesOfType` narrows the whole stack to one class array for cross-resource invariants, `remediateResourceOfType` fixes forward before validation judges — and the rows encode this folder's own laws as machine pressure: digest-pinned images, no superuser roles, TLS-only ingress, protected data planes, namespace network fences, managed-by stamps. The previewRefresh mechanics live on the automation driver — drift here is pure projection over the shared receipt vocabulary, so deploy evidence and drift evidence cannot fork. The module is `iac/src/operate/policy.ts`; a new invariant is one policy row, a new drift dimension is one report field folded from rows already carried, and no validator ever branches — growth is rows, never arms.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                             | [PUBLIC] |
| :-----: | :-------------- | :------------------------------------------------------------------ | :------- |
|  [01]   | `PACK_ASSEMBLY` | the pack value, enforcement vocabulary, the analyzer-entry law      | `Guard`  |
|  [02]   | `POLICY_ROWS`   | the typed validation, stack-invariant, and remediation rows         | `Guard`  |
|  [03]   | `DRIFT_REPORT`  | the report owner: drifted rows, rotation watch, skew evidence       | `Drift`  |
|  [04]   | `DRIFT_SWEEP`   | the reconcile projection and the scheduled fleet sweep              | `Drift`  |

## [2]-[PACK_ASSEMBLY]

[PACK_ASSEMBLY]:
- Owner: `Guard`, the `PolicyPackArgs` value — the `policies` array is the whole pack, `enforcementLevel: "mandatory"` is the pack default each row may override (`"advisory"` for stamps, `"remediate"` for fix-forward rows), and metadata (`description`, `severity`, `framework`, `remediationSteps`) rides each row as data the engine surfaces with the violation.
- Law: the pack module is pure and the analyzer entry is a boot edge — this module exports the args value; a one-line entry module (`new PolicyPack("rasm-guard", Guard)`) is the analyzer process's own top level, executed by the engine's policy plugin, and `Automation` attaches the entry's path via `policyPacks`, so the lib stays side-effect-free and the pack still gates every run.
- Law: violations are receipt material — the engine folds `ReportViolation` calls into the run's policy events; a `mandatory` violation fails the run before apply, and the receipt's diagnostics carry the evidence, so gating and reporting are one stream.
- Law: configuration is typed at the row — a policy with knobs declares `configSchema` and reads `args.getConfig<T>()`; a config-less policy declares none, knob defaults live in the schema, never in validator bodies, and a per-app enforcement override is `PolicyPackConfig` data at the entry, never a pack edit.
- Growth: one policy row per invariant; a compliance frame is one `framework` field on the rows it covers.
- Boundary: attachment plumbing is `program/automation.md`'s options row; the narrowed classes are the tier pages' constructions; enforcement semantics (`remediate` apply order, `mandatory` abort) are the engine's contract.
- Packages: `@pulumi/policy` (`PolicyPackArgs`, `ResourceValidationPolicy`, `StackValidationPolicy`, the typed helper family, `Secret`); `@pulumi/kubernetes`, `@pulumi/postgresql` (the narrowed classes).

## [3]-[POLICY_ROWS]

[POLICY_ROWS]:
- Law: image provenance is structural — `image-digest-pinned` narrows `k8s.apps.v1.Deployment` and reports every container whose image lacks an `@sha256:` digest, with a typed `configSchema` allowlisting registries a proof stack may pull mutable (`args.getConfig` decodes it, empty by default); the `kube/workload` digest law compiles to this gate, so a mutable tag cannot reach a cluster even from an app-authored program.
- Law: the data plane cannot escalate or vanish — `role-no-superuser` narrows `postgresql.Role` and rejects `superuser: true`; `data-plane-protected` walks the stack graph for CNPG `Cluster` CRs (matched on the CR's own `apiVersion`/`kind` props, the carrier's stable discriminant) and demands `opts.protect`, so an unprotected database is unshippable.
- Law: traffic is TLS-only — `ingress-tls-required` narrows `k8s.networking.v1.Ingress` and rejects a spec whose `tls` block is empty; the `kube/traffic` sink law becomes machine pressure here.
- Law: workloads carry fences — `namespace-network-fence` narrows the stack to the `Deployment` class through `validateStackResourcesOfType` and demands a `k8s.networking.v1.NetworkPolicy` beside any member, judged over the dependency-aware `PolicyResource` graph because presence-beside is a cross-resource fact no single-resource validator can see; `kube/traffic`'s constructed fence satisfies it.
- Law: remediation stamps, never invents — `managed-by-stamp` remediates `Deployment` metadata with the `app.kubernetes.io/managed-by` label before validation, returning the corrected prop bag; a remediation that mints credential material wraps it in `new policy.Secret(...)` so the engine encrypts it in state.
- Law: validators read unwrapped props totally — optional chains over the generated arg shapes, `report(message, urn?)` once per finding, `args.notApplicable(reason)` where a policy cannot judge a resource; a validator that throws is a defect, not a verdict.
- Growth: one row per invariant appended to `_policies`; a prepared-arm invariant (bucket retention, IAM floor) lands as a row narrowing that provider's class when the arm's realizer settles.

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as policy from "@pulumi/policy"
import * as postgresql from "@pulumi/postgresql"

type _DigestConfig = { readonly allowRegistries?: ReadonlyArray<string> }

const _digestPinned: policy.ResourceValidationPolicy = {
  name: "image-digest-pinned",
  description: "workload images pin an immutable digest",
  severity: "high",
  configSchema: { properties: { allowRegistries: { type: "array", items: { type: "string" } } } },
  validateResource: policy.validateResourceOfType(k8s.apps.v1.Deployment, (deployment, args, report) => {
    const allowed = args.getConfig<_DigestConfig>().allowRegistries ?? []
    return (deployment.spec?.template.spec?.containers ?? [])
      .filter((container) => !(container.image ?? "").includes("@sha256:"))
      .filter((container) => !allowed.some((registry) => (container.image ?? "").startsWith(registry)))
      .forEach((container) => report(`<mutable-image:${container.name}>`))
  }),
}

const _noSuperuser: policy.ResourceValidationPolicy = {
  name: "role-no-superuser",
  description: "app roles never hold superuser",
  severity: "critical",
  validateResource: policy.validateResourceOfType(postgresql.Role, (role, _args, report) =>
    void (role.superuser === true && report("<superuser-role>"))),
}

const _tlsIngress: policy.ResourceValidationPolicy = {
  name: "ingress-tls-required",
  description: "every ingress carries a tls block",
  severity: "high",
  validateResource: policy.validateResourceOfType(k8s.networking.v1.Ingress, (ingress, _args, report) =>
    void ((ingress.spec?.tls ?? []).length === 0 && report("<ingress-without-tls>"))),
}

const _protectedData: policy.StackValidationPolicy = {
  name: "data-plane-protected",
  description: "database clusters carry protect",
  severity: "critical",
  validateStack: (args, report) =>
    args.resources
      .filter((resource) => resource.props.apiVersion === "postgresql.cnpg.io/v1" && resource.props.kind === "Cluster")
      .filter((resource) => resource.opts.protect !== true)
      .forEach((resource) => report("<unprotected-cluster>", resource.urn)),
}

const _networkFence: policy.StackValidationPolicy = {
  name: "namespace-network-fence",
  description: "deployment-bearing stacks carry a network policy",
  severity: "medium",
  validateStack: policy.validateStackResourcesOfType(k8s.apps.v1.Deployment, (deployments, args, report) =>
    void (
      deployments.length > 0
      && !args.resources.some((resource) => resource.isType(k8s.networking.v1.NetworkPolicy))
      && report("<deployments-without-network-policy>")
    )),
}

const _managedBy: policy.ResourceValidationPolicy = {
  name: "managed-by-stamp",
  description: "workloads carry the managed-by label",
  enforcementLevel: "remediate",
  remediateResource: policy.remediateResourceOfType(k8s.apps.v1.Deployment, (deployment) => ({
    ...deployment,
    metadata: {
      ...deployment.metadata,
      labels: { ...deployment.metadata?.labels, "app.kubernetes.io/managed-by": "rasm-iac" },
    },
  })),
}

const _policies: policy.Policies = [_digestPinned, _noSuperuser, _tlsIngress, _protectedData, _networkFence, _managedBy]

const Guard: policy.PolicyPackArgs = {
  policies: _policies,
  enforcementLevel: "mandatory",
  description: "rasm deploy-plane invariants",
}
```

## [4]-[DRIFT_REPORT]

[DRIFT_REPORT]:
- Owner: `DriftReport`, one `Schema.Class` reusing the automation owner's field schemas — `summary` and `drifted` are `RunReceipt.fields.summary` and `RunReceipt.fields.steps` composed directly, so the drift vocabulary cannot fork from the receipt vocabulary — plus `rotations` (the urns of certificate resources whose reissue window is open) and the `Option`-carried `skew` pair.
- Law: `clean` is a projection — no drifted row and no open rotation; a report is evidence, and acting on it (re-running `up`, bumping an epoch) is the caller's decision over data.
- Law: rotation is type-token matched — a `tls:`-prefixed step whose op is not `same` is a certificate moving through its renewal window, the deploy-plane read of the `Certs` `earlyRenewalHours`/`readyForRenewal` law; the match is a prefix on the step's own `type` field, no second watch channel exists.
- Law: skew is fold-audit evidence — the engine's change summary and the event-folded buckets must agree; a disagreement ships as the `skew` pair rather than a silent preference, because a fold that quietly trusts one source cannot detect its own decode drift.
- Growth: a new watch family is one prefix row in the rotation filter or one projection field.
- Boundary: step and summary shapes are `program/automation.md`'s; what a drifted board means is `operate/observe.md`'s content-hash law; database-DDL conformance is not drift material here — the runtime's fail-closed capability probe owns out-of-band DDL divergence, and a provider read-back of schema state duplicates a verify that already gates every boot.
- Packages: `effect` (`Schema`, `Option`); `../program/automation.ts` (`RunReceipt`).

```typescript
import type { Stack } from "@pulumi/pulumi/automation"
import { Array, Effect, Option, pipe, Record, Schema, type Schedule } from "effect"
import { Automation, DeployFault, RunReceipt } from "../program/automation.ts"
import type { StackSpec } from "../program/spec.ts"

class DriftReport extends Schema.Class<DriftReport>("DriftReport")({
  stack: Schema.NonEmptyString,
  summary: RunReceipt.fields.summary,
  drifted: RunReceipt.fields.steps,
  rotations: Schema.Array(Schema.String),
  skew: Schema.optionalWith(
    Schema.Struct({
      expected: RunReceipt.fields.summary,
      observed: RunReceipt.fields.summary,
    }),
    { as: "Option" },
  ),
}) {
  get clean(): boolean {
    return this.drifted.length === 0 && this.rotations.length === 0
  }
}
```

## [5]-[DRIFT_SWEEP]

[DRIFT_SWEEP]:
- Owner: `Drift` — `check(stack, name)` composes `Automation.reconcile` (the driver's read-only leg) and projects the receipt through `_report`: drifted rows, rotations, observed buckets, and the skew comparison against the decoded summary; `sweep(fleet, cadence, sink)` repeats the fleet check under the caller's `Schedule` at the fiber's inherited concurrency budget — the boundary that launches the sweep owns the degree through `Effect.withConcurrency`, never a literal here — delivering each cycle's reports to the sink.
- Law: the leg never mutates — `reconcile` is the engine's non-mutating previewRefresh; the mutating `refresh` stays a ledger op a human or workflow chooses after reading a report, and a secret-change webhook delivery is the event-shaped trigger that runs `check` between sweep cycles.
- Law: observed buckets fold from steps — group by op, count, compare per `OpType` against the receipt summary with absent buckets read as zero; the comparison is total over the anchored vocabulary, so a new engine op is a compile-time event here, never a silent bucket.
- Law: the projection is expression-shaped end to end — the callback seam lives inside the driver's one `Effect.async` wrap; this page folds decoded values only.
- Entry: `Drift.check(stack, spec.name)` ad hoc or webhook-triggered; `Drift.sweep(fleet, Schedule.cron("0 4 * * *"), sink)` as the standing watch.
- Growth: a per-arm drift posture (ignore rows an operator owns) is one filter parameter over the drifted rows, defaulted permissive.
- Boundary: the cadence value and its composition are the rails law consumed as a parameter; reports persist wherever the caller's sink writes them.
- Packages: `effect` (`Effect`, `Array`, `Option`, `pipe`, `Record`, `Schedule`); `@pulumi/pulumi/automation` (`Stack`); `../program/automation.ts` (`Automation`, `DeployFault`, `RunReceipt`).

```typescript
const _observed = (steps: RunReceipt["steps"]): Record.ReadonlyRecord<string, number> =>
  pipe(
    Array.groupBy(steps, (step) => step.op),
    Record.map((rows) => rows.length),
  )

const _skewed = (
  expected: RunReceipt["summary"],
  observed: Record.ReadonlyRecord<string, number>,
): Option.Option<{ readonly expected: RunReceipt["summary"]; readonly observed: Record.ReadonlyRecord<string, number> }> =>
  Array.every(RunReceipt.opTypes, (op) => (expected[op] ?? 0) === (observed[op] ?? 0))
    ? Option.none()
    : Option.some({ expected, observed })

const _report = (receipt: RunReceipt): unknown => {
  const drifted = Array.filter(receipt.steps, (step) => step.op !== "same")
  const observed = _observed(receipt.steps)
  return {
    stack: receipt.stack,
    summary: receipt.summary,
    drifted,
    rotations: Array.map(
      Array.filter(drifted, (step) => step.type.startsWith("tls:")),
      (step) => step.urn,
    ),
    ...Option.match(_skewed(receipt.summary, observed), {
      onNone: () => ({}),
      onSome: (skew) => ({ skew }),
    }),
  }
}

const Drift = {
  check: (stack: Stack, name: string): Effect.Effect<DriftReport, DeployFault> =>
    Automation.reconcile(stack, name).pipe(
      Effect.flatMap((receipt) =>
        Effect.mapError(
          Schema.decodeUnknown(DriftReport)(_report(receipt)),
          (parse) => new DeployFault({ reason: "alien", stack: name, detail: parse.message }),
        )),
    ),
  sweep: <R>(
    fleet: ReadonlyArray<readonly [StackSpec, Stack]>,
    cadence: Schedule.Schedule<unknown>,
    sink: (reports: ReadonlyArray<DriftReport>) => Effect.Effect<void, never, R>,
  ): Effect.Effect<void, DeployFault, R> =>
    Effect.repeat(
      Effect.flatMap(
        Effect.forEach(fleet, ([spec, stack]) => Drift.check(stack, spec.name), { concurrency: "inherit" }),
        sink,
      ),
      { schedule: cadence },
    ).pipe(Effect.asVoid),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Drift, DriftReport, Guard }
```
