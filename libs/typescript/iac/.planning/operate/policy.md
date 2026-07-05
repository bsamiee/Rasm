# [IAC_POLICY]

The policy plane in one owner with three verdict directions: `Guard` judges desired state before apply — one `PolicyPackArgs` value of policies-as-data rows, pack-level `mandatory` enforcement with per-policy overrides, compliance frames riding the rows they cover, attached to every run through `Automation.Options.policyPacks` so no `up` or `preview` executes ungated — `Drift` judges live state after it, projecting `Automation.reconcile` receipts into `DriftReport` rows plus the docker-cell store-conformance read-back, and `Reconcile` closes the loop in-cluster: the Pulumi Kubernetes Operator as a chart row with typed `Stack` CRs, so desired state re-asserts continuously between deploy-host sweeps and a tenant-submitted CR can trigger provisioning without a deploy-host actor. Guard policies narrow against the exact resource classes the tier pages construct through the typed helper family, and the rows encode this folder's own laws as machine pressure ON THE ARM THAT SHIPS: digest-pinned images, no superuser roles on BOTH the bridged `postgresql.Role` class and the CNPG `managed.roles` rows the primary arm actually creates, TLS at the Gateway and legacy-Ingress edges, protected data planes with their scheduled backups present, namespace network fences, managed-by stamps through the one combined validate-remediate callback. The previewRefresh mechanics live on the automation driver — drift here is pure projection over the shared receipt vocabulary, so deploy evidence and drift evidence cannot fork. The module is `iac/src/operate/policy.ts`; a new invariant is one policy row, a new drift dimension is one report field folded from rows already carried, a new reconcile subject is one `Stack` CR row, and no validator ever branches — growth is rows, never arms.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             | [PUBLIC]    |
| :-----: | :--------------- | :------------------------------------------------------------------ | :---------- |
|  [01]   | `PACK_ASSEMBLY`  | the pack value, enforcement vocabulary, compliance frames           | `Guard`     |
|  [02]   | `POLICY_ROWS`    | the typed validation, stack-invariant, and remediation rows         | `Guard`     |
|  [03]   | `DRIFT_REPORT`   | the report owner: drifted rows, rotation watch, skew evidence       | `Drift`     |
|  [04]   | `DRIFT_SWEEP`    | the reconcile projection, the fleet sweep, the conformance read-back | `Drift`     |
|  [05]   | `RECONCILE_LOOP` | the in-cluster PKO operator and its typed Stack CR rows             | `Reconcile` |

## [2]-[PACK_ASSEMBLY]

[PACK_ASSEMBLY]:
- Owner: `Guard`, the `PolicyPackArgs` value — the `policies` array is the whole pack, `enforcementLevel: "mandatory"` is the pack default each row may override (`"advisory"` for stamps, `"remediate"` for fix-forward rows), and metadata (`description`, `severity`, `framework`, `remediationSteps`) rides each row as data the engine surfaces with the violation.
- Law: compliance is a frame on the row — `_CIS` is the one `PolicyComplianceFramework` vocabulary value, stamped on the rows whose invariant realizes a benchmark control, so an auditor reads coverage off the pack value and a compliance mapping is a `framework` field, never a parallel document.
- Law: the pack module is pure and the analyzer entry is a boot edge — this module exports the args value; a one-line entry module (`new PolicyPack("rasm-guard", Guard)`) is the analyzer process's own top level, executed by the engine's policy plugin, and `Automation` attaches the entry's path via `policyPacks`, so the lib stays side-effect-free and the pack still gates every run.
- Law: violations are receipt material — the engine folds `ReportViolation` calls into the run's policy events; a `mandatory` violation fails the run before apply, and the receipt's diagnostics carry the evidence, so gating and reporting are one stream.
- Law: configuration is typed at the row — a policy with knobs declares `configSchema` and reads `args.getConfig<T>()`; a config-less policy declares none, knob defaults live in the schema, never in validator bodies, and a per-app enforcement override is `PolicyPackConfig` data at the entry, never a pack edit.
- Growth: one policy row per invariant; a new benchmark is one frame value stamped on the rows it maps.
- Boundary: attachment plumbing is `program/automation.md`'s options row; the narrowed classes are the tier pages' constructions; enforcement semantics (`remediate` apply order, `mandatory` abort) are the engine's contract.
- Packages: `@pulumi/policy` (`PolicyPackArgs`, `ResourceValidationPolicy`, `StackValidationPolicy`, the typed helper family, `PolicyComplianceFramework`, `Secret`); `@pulumi/kubernetes`, `@pulumi/postgresql` (the narrowed classes).

## [3]-[POLICY_ROWS]

[POLICY_ROWS]:
- Law: image provenance is structural — `image-digest-pinned` narrows `k8s.apps.v1.Deployment` and reports every container whose image lacks an `@sha256:` digest, with a typed `configSchema` allowlisting registries a proof stack may pull mutable; the `kube/workload` digest law compiles to this gate, so a mutable tag cannot reach a cluster even from an app-authored program.
- Law: the data plane cannot escalate on ANY arm — `role-no-superuser` narrows the bridged `postgresql.Role` class (the docker cell's spelling), and `managed-role-no-superuser` walks the stack for CNPG `Cluster` CRs (matched on the CR's own `apiVersion`/`kind` props, the carrier's stable discriminant) and reports any `spec.managed.roles[]` row carrying `superuser: true` — the primary arm's managed roles are guarded by the same law the bridged class row enforces, one invariant, two carrier spellings.
- Law: the data plane cannot vanish or run unarchived — `data-plane-protected` demands `opts.protect` on every CNPG `Cluster`, and `backup-beside-cluster` is the dependency-aware cross-resource row: every `Cluster` CR must have a `ScheduledBackup` CR whose `spec.cluster.name` references it, so an unprotected or unarchived database is unshippable.
- Law: traffic is TLS-only at both edges — `ingress-tls-required` narrows the legacy `k8s.networking.v1.Ingress` and rejects a spec whose `tls` block is empty; `gateway-tls-required` walks Gateway API `Gateway` CRs and rejects a listener set with no `HTTPS`-terminating member carrying `certificateRefs`; the `kube/traffic` sink law becomes machine pressure on whichever edge row the estate selects.
- Law: workloads carry fences — `namespace-network-fence` narrows the stack to the `Deployment` class through `validateStackResourcesOfType` and demands a `k8s.networking.v1.NetworkPolicy` beside any member, judged over the dependency-aware `PolicyResource` graph because presence-beside is a cross-resource fact no single-resource validator can see.
- Law: remediation and validation are one callback — `managed-by-stamp` rides `validateRemediateResourceOfType`, whose single callback yields both halves spread into the row: the returned prop bag fixes forward under `"remediate"`, the same callback judges under stricter levels, and a remediation that mints credential material wraps it in `new policy.Secret(...)` so the engine encrypts it in state.
- Law: validators read unwrapped props totally — optional chains over the generated arg shapes, `report(message, urn?)` once per finding, `args.notApplicable(reason)` where a policy cannot judge a resource; a validator that throws is a defect, not a verdict; the validator bodies are boundary-framework kernels over foreign prop bags, the one place native iteration is ruled.
- Growth: one row per invariant appended to `_policies`; a prepared-arm invariant (bucket retention, IAM floor) lands as a row narrowing that provider's class when the arm's realizer settles; a tenant-isolation row (every tenant namespace carries its fence) lands beside `namespace-network-fence` when the tenancy mode escalates.

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as policy from "@pulumi/policy"
import * as postgresql from "@pulumi/postgresql"

type _DigestConfig = { readonly allowRegistries?: ReadonlyArray<string> }

const _CIS: policy.PolicyComplianceFramework = {
  name: "CIS-Kubernetes",
  version: "1.9",
  reference: "https://www.cisecurity.org/benchmark/kubernetes",
  specification: "workload and data-plane hardening controls",
}

const _digestPinned: policy.ResourceValidationPolicy = {
  name: "image-digest-pinned",
  description: "workload images pin an immutable digest",
  severity: "high",
  framework: _CIS,
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
  framework: _CIS,
  validateResource: policy.validateResourceOfType(postgresql.Role, (role, _args, report) =>
    void (role.superuser === true && report("<superuser-role>"))),
}

const _cnpg = (resource: policy.PolicyResource): boolean =>
  resource.props.apiVersion === "postgresql.cnpg.io/v1" && resource.props.kind === "Cluster"

const _noManagedSuperuser: policy.StackValidationPolicy = {
  name: "managed-role-no-superuser",
  description: "cnpg managed roles never hold superuser",
  severity: "critical",
  framework: _CIS,
  validateStack: (args, report) =>
    args.resources
      .filter(_cnpg)
      .filter((resource) => (resource.props.spec?.managed?.roles ?? []).some((role: { superuser?: boolean }) => role.superuser === true))
      .forEach((resource) => report("<superuser-managed-role>", resource.urn)),
}

const _protectedData: policy.StackValidationPolicy = {
  name: "data-plane-protected",
  description: "database clusters carry protect",
  severity: "critical",
  validateStack: (args, report) =>
    args.resources
      .filter(_cnpg)
      .filter((resource) => resource.opts.protect !== true)
      .forEach((resource) => report("<unprotected-cluster>", resource.urn)),
}

const _backupBeside: policy.StackValidationPolicy = {
  name: "backup-beside-cluster",
  description: "every database cluster has a scheduled backup referencing it",
  severity: "high",
  validateStack: (args, report) => {
    const archived = new Set(
      args.resources
        .filter((resource) => resource.props.apiVersion === "postgresql.cnpg.io/v1" && resource.props.kind === "ScheduledBackup")
        .map((resource) => resource.props.spec?.cluster?.name),
    )
    return args.resources
      .filter(_cnpg)
      .filter((resource) => !archived.has(resource.props.metadata?.name ?? resource.name))
      .forEach((resource) => report("<cluster-without-scheduled-backup>", resource.urn))
  },
}

const _tlsIngress: policy.ResourceValidationPolicy = {
  name: "ingress-tls-required",
  description: "every legacy ingress carries a tls block",
  severity: "high",
  validateResource: policy.validateResourceOfType(k8s.networking.v1.Ingress, (ingress, _args, report) =>
    void ((ingress.spec?.tls ?? []).length === 0 && report("<ingress-without-tls>"))),
}

const _tlsGateway: policy.StackValidationPolicy = {
  name: "gateway-tls-required",
  description: "every gateway terminates tls on a certificate",
  severity: "high",
  validateStack: (args, report) =>
    args.resources
      .filter((resource) => resource.props.kind === "Gateway" && String(resource.props.apiVersion ?? "").startsWith("gateway.networking.k8s.io"))
      .filter((resource) =>
        !(resource.props.spec?.listeners ?? []).some((listener: { protocol?: string; tls?: { certificateRefs?: ReadonlyArray<unknown> } }) =>
          listener.protocol === "HTTPS" && (listener.tls?.certificateRefs ?? []).length > 0))
      .forEach((resource) => report("<gateway-without-tls>", resource.urn)),
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
  ...policy.validateRemediateResourceOfType(k8s.apps.v1.Deployment, (deployment) => ({
    ...deployment,
    metadata: {
      ...deployment.metadata,
      labels: { ...deployment.metadata?.labels, "app.kubernetes.io/managed-by": "rasm-iac" },
    },
  })),
}

const _policies: policy.Policies = [
  _digestPinned, _noSuperuser, _noManagedSuperuser, _protectedData,
  _backupBeside, _tlsIngress, _tlsGateway, _networkFence, _managedBy,
]

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
- Law: rotation is type-token matched — a `tls:`-prefixed step whose op is not `same` is a certificate moving through its renewal window, the deploy-plane read of the `Certs` `earlyRenewalHours`/`readyForRenewal` law; the ACME lane's ARI-window reissue surfaces through the same prefix watch on its own type token, so one channel covers both lanes.
- Law: skew is fold-audit evidence — the engine's change summary and the event-folded buckets must agree; a disagreement ships as the `skew` pair rather than a silent preference, because a fold that quietly trusts one source cannot detect its own decode drift.
- Growth: a new watch family is one prefix row in the rotation filter or one projection field.
- Boundary: step and summary shapes are `program/automation.md`'s; what a drifted board means is `operate/observe.md`'s content-hash law; in-cluster DDL divergence on the k8s arm is the runtime's fail-closed capability probe, ruled there — the `conform` read-back below exists ONLY for the docker cell whose daemon host the deploy plane can reach.
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
- Owner: `Drift` — `check(stack, name)` composes `Automation.reconcile` (the driver's read-only leg) and projects the receipt through `_report`; `sweep(fleet, cadence, sink)` repeats the fleet check under the caller's `Schedule` at the fiber's inherited concurrency budget, and each stack's failure is isolated through `Effect.either` so one faulted stack never starves the rest of the fleet cycle — the sweep delivers every cycle's verdicts, faults included, to the sink; `conform(database, expected)` is the docker-cell store read-back over `postgresql.getTables`, returning the relations the expected roster names that the live store does not carry.
- Law: the leg never mutates — `reconcile` is the engine's non-mutating previewRefresh; the mutating `refresh` stays a ledger op a human or workflow chooses after reading a report; the event-shaped triggers between sweep cycles are the two webhooks of one evidence-delivery law — the Doppler secret-change delivery (`operate/secret.md`) and the Pulumi Cloud `DriftDetected` filter (`operate/cloud.md`, when the backend is hosted) — both landing on a sink that runs `check`.
- Law: observed buckets fold from steps — group by op, count, compare per `OpType` against the receipt summary with absent buckets read as zero; the comparison is total over the anchored vocabulary, so a new engine op is a compile-time event here, never a silent bucket.
- Law: the projection is expression-shaped end to end — the callback seam lives inside the driver's one stream bridge; this page folds decoded values only.
- Entry: `Drift.check(stack, spec.name)` ad hoc or webhook-triggered; `Drift.sweep(fleet, Schedule.cron("0 4 * * *"), sink)` as the standing watch; `Drift.conform(database, roster)` on the docker cell beside the sweep.
- Growth: a per-arm drift posture (ignore rows an operator owns) is one filter parameter over the drifted rows, defaulted permissive.
- Boundary: the cadence value and its composition are the rails law consumed as a parameter; reports persist wherever the caller's sink writes them; `conform` is valid only where the daemon host is deploy-reachable — the k8s arm's conformance is the runtime probe, never this read.
- Packages: `effect` (`Effect`, `Array`, `Either`, `Option`, `pipe`, `Record`, `Schedule`); `@pulumi/pulumi/automation` (`Stack`); `@pulumi/postgresql` (`getTables`); `../program/automation.ts` (`Automation`, `DeployFault`, `RunReceipt`).

```typescript
import { Either } from "effect"
import * as postgresql from "@pulumi/postgresql"

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
  conform: (database: string, expected: ReadonlyArray<string>): Effect.Effect<ReadonlyArray<string>, DeployFault> =>
    Effect.map(
      Effect.tryPromise({ try: () => postgresql.getTables({ database }), catch: DeployFault.triaged(database) }),
      (result) => Array.difference(expected, Array.map(result.tables, (table) => table.objectName)),
    ),
  sweep: <R>(
    fleet: ReadonlyArray<readonly [StackSpec, Stack]>,
    cadence: Schedule.Schedule<unknown>,
    sink: (verdicts: ReadonlyArray<Either.Either<DriftReport, DeployFault>>) => Effect.Effect<void, never, R>,
  ): Effect.Effect<void, never, R> =>
    Effect.repeat(
      Effect.flatMap(
        Effect.forEach(fleet, ([spec, stack]) => Effect.either(Drift.check(stack, spec.name)), { concurrency: "inherit" }),
        sink,
      ),
      { schedule: cadence },
    ).pipe(Effect.asVoid),
} as const
```

## [6]-[RECONCILE_LOOP]

[RECONCILE_LOOP]:
- Owner: `Reconcile`, the in-cluster continuous-reconciliation tier — the Pulumi Kubernetes Operator installs as one `helm.v4.Chart` row, and each reconciled estate is one typed `Stack` CR (committed `crd2pulumi` classes from `../crds/pko`): `spec.stack` names the target, `spec.projectRepo`/`branch` bind the Git source of the desired-state program, `spec.refresh: true` re-reads provider state each cycle, `spec.continueResyncOnCommitMatch` + `spec.resyncFrequencySeconds` make the loop continuous rather than commit-edge-triggered, and `spec.envRefs` bind the workspace facts from the ONE workspace `Secret` this tier mints from its `workspace` args — the same facts `_host` reads on the deploy host, one vocabulary, two execution planes, and a CR referencing a secret nothing minted is the phantom this owner closes.
- Law: two clocks never watch one stack — the deploy-host `Drift.sweep` and an in-cluster `Stack` CR are alternative reconcilers; an estate under PKO drops out of the local fleet roster, so evidence has one producer per stack and remediation posture stays deliberate on both paths.
- Law: tenant-triggered provisioning rides the operator — a tenant-submitted CR (the `Program` CR carrying an inline desired-state program, or a `Stack` CR referencing a tenant repo) is reconciled by PKO inside the tenant's own RBAC envelope, so multi-tenant self-service provisioning needs no deploy-host actor and the Capsule/vcluster boundary from `kube/tenant.md` scopes what the tenant's CR may reach.
- Law: the operator is scoped, not cluster-wide by default — the chart installs into the estate namespace with its workload identity bound through the same `ServiceAccount`/`Role` cell `kube/workload.md` realizes; widening to cluster scope is a deliberate values row.
- Entry: `new Reconcile("reconcile", { spec, namespace, version, source, frequencySeconds, workspace }, opts)` inside the k8s arm when the estate earns the in-cluster loop, `workspace` carrying the backend URL and the passphrase read the composing arm resolves.
- Growth: a second reconciled estate is one more `Stack` CR row; an inline-program subject is one `Program` CR row.
- Boundary: the operator chart's values drift with its pin; the generated `crds/pko` module regenerates on operator bumps; hosted drift schedules are `operate/cloud.md`'s twin, subject to the same one-clock law.
- Packages: `@pulumi/kubernetes` (`helm.v4.Chart`); `../crds/pko` (`v1.Stack`, `v1.Program` — crd2pulumi); `../program/spec.ts` (`StackSpec`, `Tier`).

```typescript
import * as k8s from "@pulumi/kubernetes"
import * as pko from "../crds/pko"
import { Tier, type StackSpec } from "../program/spec.ts"

declare namespace Reconcile {
  type Args = {
    readonly spec: StackSpec
    readonly namespace: pulumi.Input<string>
    readonly version: pulumi.Input<string>
    readonly source: { readonly repo: string; readonly branch: string; readonly dir?: string }
    readonly frequencySeconds: number
    readonly workspace: { readonly backend: pulumi.Input<string>; readonly passphrase: pulumi.Input<string> }
  }
}

class Reconcile extends Tier {
  constructor(name: string, args: Reconcile.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Reconcile", name, opts)
    const operator = new k8s.helm.v4.Chart(name, {
      chart: "pulumi-kubernetes-operator",
      repositoryOpts: { repo: "https://pulumi.github.io/pulumi-kubernetes-operator" },
      version: args.version,
      namespace: args.namespace,
      skipCrds: false,
    }, this.child())
    const workspace = new k8s.core.v1.Secret(`${name}-workspace`, {
      metadata: { namespace: args.namespace },
      stringData: {
        PULUMI_BACKEND_URL: args.workspace.backend,
        PULUMI_CONFIG_PASSPHRASE: args.workspace.passphrase,
      },
    }, this.child())
    new pko.v1.Stack(args.spec.name, {
      metadata: { namespace: args.namespace },
      spec: {
        stack: args.spec.name,
        projectRepo: args.source.repo,
        branch: args.source.branch,
        ...(args.source.dir !== undefined && { repoDir: args.source.dir }),
        refresh: true,
        continueResyncOnCommitMatch: true,
        resyncFrequencySeconds: args.frequencySeconds,
        envRefs: {
          PULUMI_BACKEND_URL: { type: "Secret", secret: { name: workspace.metadata.name, key: "PULUMI_BACKEND_URL" } },
          PULUMI_CONFIG_PASSPHRASE: { type: "Secret", secret: { name: workspace.metadata.name, key: "PULUMI_CONFIG_PASSPHRASE" } },
        },
      },
    }, this.child({ dependsOn: [operator] }))
    this.seal({})
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Drift, DriftReport, Guard, Reconcile }
```
