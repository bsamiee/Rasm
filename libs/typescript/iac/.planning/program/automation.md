# [IAC_AUTOMATION]

The Automation-API driver: inline typed programs over `LocalWorkspace.createOrSelectStack` with no `Pulumi.yaml` anywhere — every workspace fact (self-managed `backend.url`, `secretsProvider`, passphrase, CLI root) is an Effect `Config` read resolved once, and the pulumi CLI-binary-on-PATH is a deploy-host fact wrapped exactly here. The engine's `onEvent` callback bridges into an Effect `Stream` through `Stream.asyncPush` inside the run's own `Scope` — release aborts the engine run, so fiber interruption, scope close, and budget exhaustion all cancel with no orphan update — and one fold function buckets summary, steps, and diagnostics in a single pass whether the events arrive as a stream or a batch. The run owner internalizes its own resilience: the core retry ledger's `bulk` row intersected with this plane's re-drive ceiling self-heals state-lock collisions, a per-run budget rides `Effect.timeoutFail`, and a span wraps every drive, so a consumer composes capability, never recurrence plumbing. `previewRefresh` rides the same owner as the read-only fifth leg: `reconcile` re-reads live provider state against the desired graph without mutating, and its receipt is the drift material `operate/policy.md` projects, so drift evidence and deploy evidence share one vocabulary by construction. The fleet verbs close the engine's workspace surface on the same owner — ESC attachment, batch adoption, update history with its duration series, stack tags, cancel, rename, the polymorphic config verb, and the workspace roster reads are typed members over the engine's own methods, receipt identity carries the one `fullyQualifiedStackName` spelling, and `remote` is the Deployments execution row gated on the spec's `cloud` backend. The engine has no in-band typed error class, so this page also owns the deploy plane's one fault family: `DeployFault`, reason-discriminated over one core `Fault.Class.family` mint and minted by one foreign-value triage over the `CommandError` classes. The module is `iac/src/program/automation.ts`; a new engine event arm is one fold row, a new failure cause is one family row plus one triage arm, and the mutating ledger itself is closed.

## [01]-[INDEX]

- [02]-[ENGINE_VOCABULARY]: the ledger tuple, the `OpType` union, the receipt owner; `RunReceipt`.
- [03]-[DEPLOY_FAULT]: the reason-discriminated fault family, its policy table, the triage; `DeployFault`.
- [04]-[AUTOMATION_RUN]: host facts, the stream bridge, internalized resilience, the fleet verbs; `Automation`.

## [02]-[ENGINE_VOCABULARY]

[ENGINE_VOCABULARY]:
- Owner: `RunReceipt`, one `Schema.Class` — `op` (the ledger-or-reconcile literal), `stack` (the fully qualified name), `summary` (the per-`OpType` count record the terminal `SummaryEvent.resourceChanges` carries), `steps` (one inline row per `resourcePreEvent`: op, urn, type token, provider, changed property paths), `diagnostics` (severity-tagged provider messages), `violations` (one row per `policyEvent`: policy, pack, enforcement level, message, `Option`-carried urn), `output` (the ordered `stdoutEvent` lines), and `timing` (the `Option`-carried first/last-timestamp band) — step, diagnostic, and violation shapes are inline `Schema.Struct` blocks embedded in the one owner, reachable as `RunReceipt["steps"][number]`, never sibling classes.
- Law: the interior anchors are tuples — `_ops` (the mutating ledger plus the `reconcile` read leg) and `_opTypes` (the engine's operation union) — spread into `Schema.Literal` so the schema arm holds the non-empty overload; `RunReceipt.ops`/`RunReceipt.opTypes` ride the class, and `operate/policy.md` buckets over the same anchor, so the operation vocabulary has one spelling folder-wide.
- Law: the summary decodes as `Schema.Record` keyed by the `OpType` literal under `Schema.partialWith({ exact: true })` — the engine's `OpMap` is partial, so only present buckets decode with no `undefined` cell, an unknown operation fails the decode loudly, and `mutated` projects whether any non-`same` bucket is inhabited so callers gate on evidence, never on stdout text.
- Law: an engine vocabulary this page restates is BOUND to its declaration in both directions — the engine ships `OpType` and the inline `DiagnosticEvent["severity"]`/`PolicyEvent["enforcementLevel"]` unions as types with no runtime const, so a tuple is the only roster a schema can spread; `satisfies` refuses a member the engine dropped and the `_Ops`/`_Severities`/`_Levels` probes refuse a member a provider bump added, so the one case a hand roster cannot avoid still cannot drift. `_ops` binds to nothing on purpose: it carries this page's own `reconcile` leg and the engine's `UpdateKind` names settled updates including rows this driver never drives.
- Law: diagnostics keep the engine's own severity union (`info | info#err | warning | error`) verbatim — bridged providers report failures only through this stream, so severity is the one match key and message text is never parsed.
- Law: violations are decoded gate evidence — one row per `policyEvent` keeps the engine's `enforcementLevel` union (`warning | mandatory`) verbatim beside policy name, pack, message, and the `Option`-carried resource urn, and `gated` projects whether any `mandatory` row is inhabited, so the `operate/policy.md` violations-are-receipt-material law is true by construction and a gate verdict never parses stdout.
- Law: `timing` is the run's own band — `started` and `settled` are the first and last `EngineEvent.timestamp` epoch seconds the one-pass fold witnesses, `elapsed` derives at the decode seam, and the band is `Option`-carried because a receipt folded from an empty batch has no span; `output` keeps the `stdoutEvent` lines ordered for the one consumer that must read engine text, and no verdict reads them.
- Growth: a new receipt dimension is one field decoded from the event arm that carries it; a new engine event arm is one `_folded` row in `[4]`.
- Boundary: outputs are not receipt material — `spec.md` owns the `OutputMap` decode; the drift projection over these same step rows is `operate/policy.md`'s.
- Packages: `effect` (`Schema`, `Record`).

```typescript
import type { DiagnosticEvent, OpType as EngineOpType, PolicyEvent } from "@pulumi/pulumi/automation"
import { Array, Record, Schema } from "effect"

const _ops = ["up", "preview", "refresh", "destroy", "reconcile"] as const
const _opTypes = [
  "same", "create", "update", "delete", "replace",
  "create-replacement", "delete-replaced", "read", "read-replacement",
  "refresh", "discard", "discard-replaced", "remove-pending-replace",
  "import", "import-replacement",
] as const satisfies ReadonlyArray<EngineOpType>
const _severities = ["info", "info#err", "warning", "error"] as const satisfies ReadonlyArray<DiagnosticEvent["severity"]>
const _levels = ["warning", "mandatory"] as const satisfies ReadonlyArray<PolicyEvent["enforcementLevel"]>

const _Op = Schema.Literal(..._ops)
const _OpType = Schema.Literal(..._opTypes)
const _Timing = Schema.Struct({ started: Schema.Int, settled: Schema.Int, elapsed: Schema.Int })

class RunReceipt extends Schema.Class<RunReceipt>("RunReceipt")({
  op: _Op,
  stack: Schema.NonEmptyString,
  summary: Schema.Record({ key: _OpType, value: Schema.Int }).pipe(Schema.partialWith({ exact: true }))
    .annotations({ parseOptions: { onExcessProperty: "error" } }),
  steps: Schema.Array(Schema.Struct({
    op: _OpType,
    urn: Schema.String,
    type: Schema.String,
    provider: Schema.String,
    changed: Schema.optionalWith(Schema.Array(Schema.String), { as: "Option" }),
  })),
  diagnostics: Schema.Array(Schema.Struct({
    severity: Schema.Literal(..._severities),
    message: Schema.String,
    urn: Schema.optionalWith(Schema.String, { as: "Option" }),
  })),
  violations: Schema.Array(Schema.Struct({
    policy: Schema.String,
    pack: Schema.String,
    level: Schema.Literal(..._levels),
    message: Schema.String,
    urn: Schema.optionalWith(Schema.String, { as: "Option" }),
  })),
  output: Schema.Array(Schema.String),
  timing: Schema.optionalWith(_Timing, { as: "Option" }),
}) {
  static readonly ops: RunReceipt.Ops = _ops
  static readonly opTypes: RunReceipt.OpTypes = _opTypes
  get mutated(): boolean {
    return Record.reduce(this.summary, 0, (acc, count, op) => (op === "same" ? acc : acc + count)) > 0
  }
  get gated(): boolean {
    return Array.some(this.violations, (violation) => violation.level === "mandatory")
  }
}

declare namespace RunReceipt {
  type Ops = typeof _ops
  type OpTypes = typeof _opTypes
  type Op = (typeof _ops)[number]
  type OpType = (typeof _opTypes)[number]
  type Step = RunReceipt["steps"][number]
  type Severity = (typeof _severities)[number]
  type Violation = RunReceipt["violations"][number]
  type Level = (typeof _levels)[number]
  type Timing = typeof _Timing.Type
  type _Ops<K extends OpType = EngineOpType> = K
  type _Severities<K extends Severity = DiagnosticEvent["severity"]> = K
  type _Levels<K extends Level = PolicyEvent["enforcementLevel"]> = K
}
```

## [03]-[DEPLOY_FAULT]

[DEPLOY_FAULT]:
- Law: `triaged(stack)` is the one foreign-value conversion — a `Match.instanceOf` ladder over the engine's thrown classes (`ConcurrentUpdateError`, `StackNotFoundError`, `StackAlreadyExistsError`, the `CommandError` base, `InputPropertiesError`/`InputPropertyError`, `RunError`) with `Match.orElse` minting the `alien` row, subclasses matched before their base; every `Effect.tryPromise` catch slot in the folder names this triage and no second conversion exists.
- Law: a run that outlives its ceiling is its own reason — `budget` names the timeout after the scope-close abort, so the discriminant rides the closed vocabulary instead of a sentinel string a reader must match inside a free detail field, and the ceiling crosses as the measured `Duration` the run was given rather than as text no reader can compare.
- Law: every reason declares its OWN subject and its own renderer, so the raise carries one `case` field and no free-string `detail` column survives — the engine-thrown reasons share the run identity and the foreign message they all hold, `cancelled` carries identity alone because a withdrawal has no message, and `budget` carries the ceiling it outlived.
- Law: legs partition by the surface that DECIDED — `run` for the drive this owner brackets end to end, `workspace` for the stack identity and configuration it selects against, `engine` for what the pulumi engine itself reported — so a census reads which seam refused without re-deriving it from the reason.
- Boundary: `ParseError` from receipt and output decodes re-spells into this family at the decode seam (`alien` with the parse message as detail); the severity fold over accumulated faults rides `DeployFault.bySeverity`.

```typescript
import { InputPropertiesError, InputPropertyError, RunError } from "@pulumi/pulumi"
import { CommandError, ConcurrentUpdateError, StackAlreadyExistsError, StackNotFoundError } from "@pulumi/pulumi/automation"
import { Fault } from "@rasm/core"
import { Duration, Match, Order, Schema, pipe } from "effect"

const _Triaged = Schema.Struct({ stack: Schema.String, detail: Schema.String })
const _Withdrawn = Schema.Struct({ stack: Schema.String })
const _Overran = Schema.Struct({ stack: Schema.String, ceiling: Schema.DurationFromSelf })

const _family = Fault.Class.family(
  ["cancelled", "concurrent", "absent", "duplicate", "input", "command", "budget", "diagnostic", "alien"] as const,
  {
    cancelled: Fault.Class.row({
      class: "denied",
      leg: "run",
      detail: _Withdrawn,
      render: ({ stack }) => `${stack} withdrawn by its own caller`,
    }),
    concurrent: Fault.Class.row({
      class: "conflicted",
      leg: "run",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} holds a state lock another update took — ${detail}`,
    }),
    absent: Fault.Class.row({
      class: "absent",
      leg: "workspace",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} names no stack this backend holds — ${detail}`,
    }),
    duplicate: Fault.Class.row({
      class: "invalid",
      leg: "workspace",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} names an identity already taken — ${detail}`,
    }),
    input: Fault.Class.row({
      class: "invalid",
      leg: "workspace",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} offered a coordinate this plane refuses — ${detail}`,
    }),
    command: Fault.Class.row({
      class: "breached",
      leg: "engine",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} failed inside the pulumi CLI — ${detail}`,
    }),
    budget: Fault.Class.row({
      class: "breached",
      leg: "run",
      detail: _Overran,
      render: ({ stack, ceiling }) => `${stack} outlived its ${Duration.format(ceiling)} ceiling`,
    }),
    diagnostic: Fault.Class.row({
      class: "breached",
      leg: "engine",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} refused inside the program body — ${detail}`,
    }),
    alien: Fault.Class.row({
      class: "defect",
      leg: "engine",
      detail: _Triaged,
      render: ({ stack, detail }) => `${stack} raised a value no engine class grades — ${detail}`,
    }),
  },
)

class DeployFault extends Schema.TaggedError<DeployFault>()("DeployFault", {
  case: _family.payload,
}) {
  static readonly bySeverity: Order.Order<DeployFault> = Order.mapInput(Fault.Class.order, (fault: DeployFault) => fault.class)
  static readonly triaged = (stack: string): ((caught: unknown) => DeployFault) =>
    pipe(
      Match.type<unknown>(),
      Match.when(Match.instanceOf(ConcurrentUpdateError), (e) => new DeployFault({ case: { reason: "concurrent", stack, detail: e.message } })),
      Match.when(Match.instanceOf(StackNotFoundError), (e) => new DeployFault({ case: { reason: "absent", stack, detail: e.message } })),
      Match.when(Match.instanceOf(StackAlreadyExistsError), (e) => new DeployFault({ case: { reason: "duplicate", stack, detail: e.message } })),
      Match.when(Match.instanceOf(CommandError), (e) => new DeployFault({ case: { reason: "command", stack, detail: e.message } })),
      Match.when(Match.instanceOf(InputPropertiesError), (e) => new DeployFault({ case: { reason: "input", stack, detail: e.message } })),
      Match.when(Match.instanceOf(InputPropertyError), (e) => new DeployFault({ case: { reason: "input", stack, detail: e.message } })),
      Match.when(Match.instanceOf(RunError), (e) => new DeployFault({ case: { reason: "diagnostic", stack, detail: e.message } })),
      Match.orElse((residue) => new DeployFault({ case: { reason: "alien", stack, detail: String(residue) } })),
    )
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

declare namespace DeployFault {
  type Reason = (typeof _family.kinds)[number]
  type Case = typeof _family.payload.Type
}
```

## [04]-[AUTOMATION_RUN]

[AUTOMATION_RUN]:
- Owner: `Automation` — `stack` acquires the idempotent workspace, `run` drives one mutating ledger op to a receipt, `reconcile` runs the read-only `previewRefresh` leg to the same receipt shape, `receipt` folds an event batch through the same one-pass fold the stream rides, `ephemeral` brackets a stack whose release destroys it, `snapshot`/`restore` are the state-lifecycle pair over `exportStack`/`importStack`, `adopt` is the batch-adoption verb over `Stack.import` (`ImportResource` rows, `protect` by default — the operator disaster/onboarding entry), `attach`/`environments` are the imperative ESC pair over `Stack.addEnvironments`/`listEnvironments` (no typed `StackSettings` field exists, so attachment is run data), `history` reads the engine's update audit beside the receipt and `series` projects it into the per-op duration benchmark, `label`/`tags` write and read stack tags for fleet organization, `cancel` aborts a wedged in-flight update, `rename` moves stack identity in state, `config` is the one polymorphic configuration verb, `whoAmI`/`listStacks`/`installPlugin` read and provision the workspace roster, and `remote` is the Deployments execution row over `RemoteWorkspace.createOrSelectStack`, admitted only when `spec.hosted`. The `_host` Config surface is the one deploy-host read: `PULUMI_BACKEND_URL` selects the self-managed state store, `PULUMI_CONFIG_PASSPHRASE` rides `Config.redacted` and unwraps exactly once into `envVars`, `PULUMI_PROJECT` defaults, `PULUMI_HOME` and `PULUMI_CLI_ROOT` are optional — the CLI binary resolves through `PulumiCommand.get` against the optional root and rides `LocalWorkspaceOptions.pulumiCommand`, and no `Pulumi.yaml` exists because `projectSettings` carries the same facts programmatically.
- Law: the driver is one exhaustive record — `_LEDGER` maps each op, `reconcile` included, to its `Stack` method under a mapped contract, so a sixth op is a compile error at the record; `up`/`preview` receive the policy-pack and gate options, `refresh`/`destroy`/`reconcile` the minimal projection, and every arm receives `signal` and `onEvent`. The `reconcile` arm calls `previewRefresh` — the engine's non-mutating reconcile — so a mutating `refresh` remains a deliberate ledger choice a human or workflow makes after reading drift evidence.
- Law: the event bridge is a Stream, not an accumulator — `_streamed` registers `onEvent` through `Stream.asyncPush` inside the run's `Scope`, `emit.single` carries each engine event, settle maps to `emit.end`/`emit.fail`, and the `AbortController` acquired with the run is released by aborting it, so interruption, scope close, and timeout all cancel the engine run structurally and an orphaned update is unspellable; `_folded` is the single one-pass fold — summary from the last `SummaryEvent`, a step row per `resourcePreEvent`, a diagnostic row per `DiagnosticEvent` — consumed by `Stream.runFold` on the live path and `Array.reduce` on the batch path, so the receipt derivation exists once and never re-scans a buffer.
- Law: recurrence is one ledger row under one local bound — `_CONTENDED` composes `Fault.Budget.schedule("bulk")` and intersects the deploy's own re-drive ceiling, so jitter, quiet reset, and the elapsed window are the branch's single decision and a tuning change lands at the ledger; the class gate is that member's own default, so `conflicted` stays the one reason a run re-drives and no local predicate restates it; `Effect.timeoutFail` bounds the orthogonal axis — the schedule rations how often a wedged lock is re-driven, the timeout caps how long one attempt lives.
- Law: the deploy host exports its own telemetry — the automation process's composition root merges the runtime plane's `Export.live` beneath this owner, so the `iac.automation.run` span, its retry annotations, and the process logs reach the same collector every deployed app feeds; this page names spans and receipts only, and the deploy-visible annotation the boards carry is `operate/observe.md`'s apply-time resource.
- Law: identity has one spelling — the receipt `stack` field and the run span carry the canonical `fullyQualifiedStackName("organization", host.project, name)` the engine itself resolves for a self-managed backend, while the fault family carries the caller's slug because triage fires before host facts resolve; a receipt keyed on the raw slug is the drift this law forecloses, and `history`, `series`, and the drift projection correlate on the same qualified spelling.
- Law: `config` discriminates by input shape — absent reads the whole map (`getAllConfig`), a string reads one key (`getConfig`), a `[key, value]` tuple writes one (`setConfig`), a `ConfigMap` writes bulk (`setAllConfig`), and `{ refresh: true }` re-pulls from the backend (`refreshConfig`) — five modalities on one member whose ladder reads evidence the value carries, so no `getConfig`/`setConfig` sibling family exists on this owner.
- Law: `series` is the deploy benchmark — one projection over `history` `UpdateSummary` rows into `{ version, op, result, seconds, changes }`, elapsed derived from the engine's own `startTime`/`endTime` at the boundary mirror, so deploy regression reads as data over the ledger vocabulary; receipt `timing` measures the live run, `series` measures the settled audit, and both speak seconds.
- Law: the deploy host rides the branch platform contracts — `_host` resolves from the doppler-injected process env with the `.env` fallback provided as `PlatformConfigProvider.layerDotEnv` at the composition root, `NodeContext.layer` satisfies every platform Tag the `operate/policy.md` evidence sinks and sweep cursor read, and `NodeRuntime.runMain(program.pipe(Effect.provide(root)))` is the automation process's one imperative edge — Tags in domain code, Layers at the root, so an unwired capability fails at the boot line, never mid-sweep.
- Law: `Automation.Options` derives from the run-option seam — `Omit<_RunOpts, "signal" | "onEvent">` plus the `budget` duration, so the caller surface cannot drift from what the arms accept; the option arrays keep the engine's mutable-array spelling because the record is a boundary mirror consumed once.
- Law: the deploy host obeys the injection law — the automation process itself runs under `doppler run`, which is how `PULUMI_CONFIG_PASSPHRASE`, the bootstrap `DOPPLER_TOKEN`, and the provider material reads resolve; one injection mechanism spans the deploy host and every deployed process.
- Entry: `Automation.stack(spec, program)` then `Automation.run(stack, spec.name, "up", { policyPacks })`; `Automation.reconcile(stack, spec.name)` for the standing drift read; `Automation.series(stack, spec.name)` for the regression read; `Automation.ephemeral(spec, program)` under a `Scope` for review stacks; `Automation.adopt(stack, spec.name, resources)` to absorb a pre-existing estate; `Automation.attach(stack, spec.name, envs)` after `operate/cloud.md` authors the environment.
- Packages: `@pulumi/pulumi/automation` (the workspace, stack, and engine-event surface); `effect` (`Config`, `Duration`, `Effect`, `Schedule`, `Schema`, `Stream`); `@rasm/core` (`Fault.Budget`).
- Growth: a new host fact is one `_host` row; a new call-local option is one `_RunOpts` field inherited by `Options` mechanically; a new fleet verb is one member over the engine method that carries it; a new config modality is one `_config` overload line plus one ladder arm.
- Boundary: the `PulumiFn` the stack runs is `provider.md`'s product; the drift projection over `reconcile` receipts and the `Evidence` sink vocabulary run settle delivers into are `operate/policy.md`'s; the hosted schedule/webhook twins of these verbs are `operate/cloud.md`'s rows.

```typescript
import {
  fullyQualifiedStackName, LocalWorkspace, PulumiCommand, RemoteWorkspace,
  type ConfigMap, type ConfigValue, type Deployment, type EngineEvent, type ImportResource, type PulumiFn,
  type RemoteGitProgramArgs, type RemoteStack, type Stack, type StackSummary, type UpdateSummary, type WhoAmIResult,
} from "@pulumi/pulumi/automation"
import { Fault } from "@rasm/core"
import { Array, Config, Duration, Effect, Option, Predicate, Record, Redacted, Schedule, Schema, Stream, type Scope } from "effect"
import { StackSpec } from "./spec.ts"

const _host = Config.unwrap({
  backend: Config.string("PULUMI_BACKEND_URL"),
  passphrase: Config.redacted("PULUMI_CONFIG_PASSPHRASE"),
  project: Config.string("PULUMI_PROJECT").pipe(Config.withDefault("rasm")),
  home: Config.option(Config.string("PULUMI_HOME")),
  root: Config.option(Config.string("PULUMI_CLI_ROOT")),
})

const _facts = (name: string) =>
  Effect.mapError(_host, (issue) => new DeployFault({ case: { reason: "input", stack: name, detail: String(issue) } }))

const _qualified = (project: string, name: string): string => fullyQualifiedStackName("organization", project, name)

const _CONTENDED = Schedule.intersect(Fault.Budget.schedule("bulk"), Schedule.recurs(4))

type _RunOpts = {
  readonly signal: AbortSignal
  readonly onEvent: (event: EngineEvent) => void
  readonly parallel?: number
  readonly expectNoChanges?: boolean
  readonly refresh?: boolean
  readonly policyPacks?: Array<string>
  readonly policyPackConfigs?: Array<string>
}

const _LEDGER: { readonly [K in RunReceipt.Op]: (stack: Stack, opts: _RunOpts) => Promise<unknown> } = {
  up: (stack, opts) => stack.up(opts),
  preview: (stack, opts) => stack.preview(opts),
  refresh: (stack, { signal, onEvent, parallel }) => stack.refresh({ signal, onEvent, parallel }),
  destroy: (stack, { signal, onEvent, parallel }) => stack.destroy({ signal, onEvent, parallel }),
  reconcile: (stack, { signal, onEvent, parallel }) => stack.previewRefresh({ signal, onEvent, parallel }),
}

type _Fold = {
  readonly summary: Record.ReadonlyRecord<string, number>
  readonly steps: ReadonlyArray<Record.ReadonlyRecord<string, unknown>>
  readonly diagnostics: ReadonlyArray<Record.ReadonlyRecord<string, unknown>>
  readonly violations: ReadonlyArray<Record.ReadonlyRecord<string, unknown>>
  readonly output: ReadonlyArray<string>
  readonly span: Option.Option<readonly [started: number, settled: number]>
}

const _SEED: _Fold = { summary: {}, steps: [], diagnostics: [], violations: [], output: [], span: Option.none() }

const _folded = (acc: _Fold, event: EngineEvent): _Fold => ({
  summary: event.summaryEvent?.resourceChanges ?? acc.summary,
  steps: event.resourcePreEvent === undefined ? acc.steps : Array.append(acc.steps, {
    op: event.resourcePreEvent.metadata.op,
    urn: event.resourcePreEvent.metadata.urn,
    type: event.resourcePreEvent.metadata.type,
    provider: event.resourcePreEvent.metadata.provider,
    ...(event.resourcePreEvent.metadata.diffs !== undefined && { changed: event.resourcePreEvent.metadata.diffs }),
  }),
  diagnostics: event.diagnosticEvent === undefined ? acc.diagnostics : Array.append(acc.diagnostics, {
    severity: event.diagnosticEvent.severity,
    message: event.diagnosticEvent.message,
    ...(event.diagnosticEvent.urn !== undefined && { urn: event.diagnosticEvent.urn }),
  }),
  violations: event.policyEvent === undefined ? acc.violations : Array.append(acc.violations, {
    policy: event.policyEvent.policyName,
    pack: event.policyEvent.policyPackName,
    level: event.policyEvent.enforcementLevel,
    message: event.policyEvent.message,
    ...(event.policyEvent.resourceUrn !== undefined && { urn: event.policyEvent.resourceUrn }),
  }),
  output: event.stdoutEvent === undefined ? acc.output : Array.append(acc.output, event.stdoutEvent.message),
  span: Option.match(acc.span, {
    onNone: () => Option.some([event.timestamp, event.timestamp] as const),
    onSome: ([started]) => Option.some([started, event.timestamp] as const),
  }),
})

const _streamed = (stack: Stack, name: string, op: RunReceipt.Op, options?: Automation.Options): Stream.Stream<EngineEvent, DeployFault> =>
  Stream.asyncPush((emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        const abort = new AbortController()
        _LEDGER[op](stack, { ...options, signal: abort.signal, onEvent: (event) => void emit.single(event) }).then(
          () => emit.end(),
          (caught) => emit.fail(DeployFault.triaged(name)(caught)),
        )
        return abort
      }),
      (abort) => Effect.sync(() => abort.abort()),
    ))

const _decoded = (op: RunReceipt.Op, name: string, fold: _Fold): Effect.Effect<RunReceipt, DeployFault> =>
  Effect.mapError(
    Schema.decodeUnknown(RunReceipt)({
      op, stack: name, summary: fold.summary, steps: fold.steps, diagnostics: fold.diagnostics,
      violations: fold.violations, output: fold.output,
      ...Option.match(fold.span, {
        onNone: () => ({}),
        onSome: ([started, settled]) => ({ timing: { started, settled, elapsed: settled - started } }),
      }),
    }),
    (parse) => new DeployFault({ case: { reason: "alien", stack: name, detail: parse.message } }),
  )

const _driven = (stack: Stack, name: string, op: RunReceipt.Op, options?: Automation.Options): Effect.Effect<RunReceipt, DeployFault> =>
  Effect.flatMap(_facts(name), (host) =>
    ((qualified, ceiling) =>
      Stream.runFold(_streamed(stack, name, op, options), _SEED, _folded).pipe(
        Effect.flatMap((fold) => _decoded(op, qualified, fold)),
        Effect.timeoutFail({
          duration: ceiling,
          onTimeout: () => new DeployFault({ case: { reason: "budget", stack: name, ceiling } }),
        }),
        Effect.retry(_CONTENDED),
        Effect.withSpan("iac.automation.run", { attributes: { stack: qualified, op } }),
        Effect.scoped,
      ))(_qualified(host.project, name), Duration.decode(options?.budget ?? Duration.minutes(45))))

function _config(stack: Stack, name: string): Effect.Effect<ConfigMap, DeployFault>
function _config(stack: Stack, name: string, input: string): Effect.Effect<ConfigValue, DeployFault>
function _config(stack: Stack, name: string, input: readonly [key: string, value: ConfigValue]): Effect.Effect<void, DeployFault>
function _config(stack: Stack, name: string, input: { readonly refresh: true }): Effect.Effect<ConfigMap, DeployFault>
function _config(stack: Stack, name: string, input: ConfigMap): Effect.Effect<void, DeployFault>
function _config(
  stack: Stack,
  name: string,
  input?: string | readonly [key: string, value: ConfigValue] | { readonly refresh: true } | ConfigMap,
): Effect.Effect<ConfigMap | ConfigValue | void, DeployFault> {
  return Effect.tryPromise({
    try: () =>
      input === undefined ? stack.getAllConfig()
        : Predicate.isString(input) ? stack.getConfig(input)
          : Array.isArray(input) ? stack.setConfig(input[0], input[1])
            : Predicate.hasProperty(input, "refresh") && input.refresh === true ? stack.refreshConfig()
              : stack.setAllConfig(input as ConfigMap),
    catch: DeployFault.triaged(name),
  })
}

declare namespace Automation {
  type Options = Omit<_RunOpts, "signal" | "onEvent"> & { readonly budget?: Duration.DurationInput }
  type Series = ReadonlyArray<{
    readonly version: number
    readonly op: UpdateSummary["kind"]
    readonly result: UpdateSummary["result"]
    readonly seconds: number
    readonly changes: Record.ReadonlyRecord<string, number>
  }>
}

const Automation = {
  stack: (spec: StackSpec, program: PulumiFn): Effect.Effect<Stack, DeployFault> =>
    Effect.flatMap(_facts(spec.name), (host) =>
      Effect.tryPromise({
        try: () =>
          PulumiCommand.get(Option.match(host.root, { onNone: () => ({}), onSome: (root) => ({ root }) })).then((cli) =>
            LocalWorkspace.createOrSelectStack(
              { stackName: spec.name, projectName: host.project, program },
              {
                pulumiCommand: cli,
                projectSettings: { name: host.project, runtime: "nodejs", backend: { url: host.backend } },
                secretsProvider: "passphrase",
                envVars: { PULUMI_CONFIG_PASSPHRASE: Redacted.value(host.passphrase) },
                ...(Option.isSome(host.home) && { pulumiHome: host.home.value }),
              },
            )),
        catch: DeployFault.triaged(spec.name),
      })),
  run: (stack: Stack, name: string, op: Exclude<RunReceipt.Op, "reconcile">, options?: Automation.Options): Effect.Effect<RunReceipt, DeployFault> =>
    _driven(stack, name, op, options),
  reconcile: (stack: Stack, name: string): Effect.Effect<RunReceipt, DeployFault> =>
    _driven(stack, name, "reconcile"),
  receipt: (op: RunReceipt.Op, name: string, events: ReadonlyArray<EngineEvent>): Effect.Effect<RunReceipt, DeployFault> =>
    Effect.flatMap(_facts(name), (host) => _decoded(op, _qualified(host.project, name), Array.reduce(events, _SEED, _folded))),
  adopt: (stack: Stack, name: string, resources: ReadonlyArray<ImportResource>): Effect.Effect<void, DeployFault> =>
    Effect.asVoid(Effect.tryPromise({
      try: () => stack.import({ resources: [...resources], protect: true }),
      catch: DeployFault.triaged(name),
    })),
  attach: (stack: Stack, name: string, environments: ReadonlyArray<string>): Effect.Effect<void, DeployFault> =>
    Effect.tryPromise({ try: () => stack.addEnvironments(...environments), catch: DeployFault.triaged(name) }),
  environments: (stack: Stack, name: string): Effect.Effect<ReadonlyArray<string>, DeployFault> =>
    Effect.tryPromise({ try: () => stack.listEnvironments(), catch: DeployFault.triaged(name) }),
  history: (stack: Stack, name: string, pageSize?: number): Effect.Effect<ReadonlyArray<UpdateSummary>, DeployFault> =>
    Effect.tryPromise({ try: () => stack.history(pageSize), catch: DeployFault.triaged(name) }),
  series: (stack: Stack, name: string, pageSize?: number): Effect.Effect<Automation.Series, DeployFault> =>
    Effect.map(
      Automation.history(stack, name, pageSize),
      Array.map((row) => ({
        version: row.version,
        op: row.kind,
        result: row.result,
        seconds: (row.endTime.getTime() - row.startTime.getTime()) / 1000,
        changes: row.resourceChanges ?? {},
      })),
    ),
  cancel: (stack: Stack, name: string): Effect.Effect<void, DeployFault> =>
    Effect.tryPromise({ try: () => stack.cancel(), catch: DeployFault.triaged(name) }),
  rename: (stack: Stack, name: string, to: string): Effect.Effect<void, DeployFault> =>
    Effect.asVoid(Effect.tryPromise({ try: () => stack.rename({ stackName: to }), catch: DeployFault.triaged(name) })),
  config: _config,
  whoAmI: (stack: Stack, name: string): Effect.Effect<WhoAmIResult, DeployFault> =>
    Effect.tryPromise({ try: () => stack.workspace.whoAmI(), catch: DeployFault.triaged(name) }),
  listStacks: (stack: Stack, name: string): Effect.Effect<ReadonlyArray<StackSummary>, DeployFault> =>
    Effect.tryPromise({ try: () => stack.workspace.listStacks(), catch: DeployFault.triaged(name) }),
  installPlugin: (stack: Stack, name: string, plugin: { readonly name: string; readonly version: string; readonly kind?: string }): Effect.Effect<void, DeployFault> =>
    Effect.tryPromise({ try: () => stack.workspace.installPlugin(plugin.name, plugin.version, plugin.kind), catch: DeployFault.triaged(name) }),
  label: (stack: Stack, name: string, tags: Record.ReadonlyRecord<string, string>): Effect.Effect<void, DeployFault> =>
    Effect.asVoid(Effect.forEach(Record.toEntries(tags), ([key, value]) =>
      Effect.tryPromise({ try: () => stack.setTag(key, value), catch: DeployFault.triaged(name) }))),
  tags: (stack: Stack, name: string): Effect.Effect<Record.ReadonlyRecord<string, string>, DeployFault> =>
    Effect.tryPromise({ try: () => stack.listTags(), catch: DeployFault.triaged(name) }),
  snapshot: (stack: Stack, name: string): Effect.Effect<Deployment, DeployFault> =>
    Effect.tryPromise({ try: () => stack.exportStack(), catch: DeployFault.triaged(name) }),
  restore: (stack: Stack, name: string, state: Deployment): Effect.Effect<void, DeployFault> =>
    Effect.tryPromise({ try: () => stack.importStack(state), catch: DeployFault.triaged(name) }),
  ephemeral: (spec: StackSpec, program: PulumiFn): Effect.Effect<Stack, DeployFault, Scope.Scope> =>
    Effect.acquireRelease(Automation.stack(spec, program), (stack) =>
      Effect.orDie(_driven(stack, spec.name, "destroy"))),
  remote: (spec: StackSpec, git: RemoteGitProgramArgs): Effect.Effect<RemoteStack, DeployFault> =>
    spec.hosted
      ? Effect.tryPromise({ try: () => RemoteWorkspace.createOrSelectStack(git), catch: DeployFault.triaged(spec.name) })
      : Effect.fail(new DeployFault({ case: { reason: "input", stack: spec.name, detail: "<remote-requires-cloud-backend>" } })),
} as const

// --- [EXPORTS] -------------------------------------------------------------------------

export { Automation, DeployFault, RunReceipt }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
