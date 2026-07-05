# [IAC_AUTOMATION]

The Automation-API driver: inline typed programs over `LocalWorkspace.createOrSelectStack` with no `Pulumi.yaml` anywhere — every workspace fact (self-managed `backend.url`, `secretsProvider`, passphrase, CLI root) is an Effect `Config` read resolved once, and the pulumi CLI-binary-on-PATH is a deploy-host fact wrapped exactly here. One `Effect.async` wrap owns the whole lifecycle: the up | preview | refresh | destroy ledger is one exhaustive handler record, `Effect` interruption binds the engine's `AbortSignal` so a cancelled fiber aborts the run with no orphan update, the `onEvent` callback accumulates the engine stream, and every run folds into a `RunReceipt` — op, summary buckets, step rows, diagnostics — decoded through one Schema owner. `previewRefresh` rides the same owner as the read-only fifth leg: `reconcile` re-reads live provider state against the desired graph without mutating, and its receipt is the drift material `operate/policy.md` projects, so drift evidence and deploy evidence share one vocabulary by construction. The engine has no in-band typed error class, so this page also owns the deploy plane's one fault family: `DeployFault`, reason-discriminated with a policy table, minted by one foreign-value triage over the `CommandError` classes. The module is `iac/src/program/automation.ts`; a new engine event arm is one fold row, a new failure cause is one reason row plus one triage arm, and the mutating ledger itself is closed.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                                   | [PUBLIC]      |
| :-----: | :------------------ | :----------------------------------------------------------------------- | :------------ |
|  [01]   | `ENGINE_VOCABULARY` | the ledger tuple, the 15-member `OpType` union, the receipt owner        | `RunReceipt`  |
|  [02]   | `DEPLOY_FAULT`      | the reason-discriminated fault family and the foreign-error triage       | `DeployFault` |
|  [03]   | `AUTOMATION_RUN`    | host facts, workspace acquisition, the async ledger wrap, the drift leg  | `Automation`  |

## [2]-[ENGINE_VOCABULARY]

[ENGINE_VOCABULARY]:
- Owner: `RunReceipt`, one `Schema.Class` — `op` (the ledger-or-reconcile literal), `stack` (the fully qualified name), `summary` (the per-`OpType` count record the terminal `SummaryEvent.resourceChanges` carries), `steps` (one inline row per `resourcePreEvent`: op, urn, type token, provider, changed property paths), and `diagnostics` (severity-tagged provider messages) — step and diagnostic shapes are inline `Schema.Struct` blocks embedded in the one owner, reachable as `RunReceipt["steps"][number]`, never sibling classes.
- Law: the interior anchors are tuples — `_ops` (the four-member mutating ledger plus the `reconcile` read leg) and `_opTypes` (the engine's 15-member operation union) — spread into `Schema.Literal` so the schema arm holds the non-empty overload; `RunReceipt.ops`/`RunReceipt.opTypes` ride the class, and `operate/policy.md` buckets over the same anchor, so the operation vocabulary has one spelling folder-wide.
- Law: the summary decodes as `Schema.Record` keyed by the `OpType` literal under `Schema.partialWith({ exact: true })` — the engine's `OpMap` is partial, so only present buckets decode with no `undefined` cell, an unknown operation fails the decode loudly, and `mutated` projects whether any non-`same` bucket is inhabited so callers gate on evidence, never on stdout text.
- Law: diagnostics keep the engine's own severity union (`info | info#err | warning | error`) verbatim — bridged providers report failures only through this stream, so severity is the one match key and message text is never parsed.
- Growth: a new receipt dimension is one field decoded from the event arm that carries it; a new engine event arm is one fold row in `[4]`'s `_folded`.
- Boundary: outputs are not receipt material — `spec.md` owns the `OutputMap` decode; the drift projection over these same step rows is `operate/policy.md`'s.
- Packages: `effect` (`Schema`, `Record`).

```typescript
import { Record, Schema } from "effect"

const _ops = ["up", "preview", "refresh", "destroy", "reconcile"] as const
const _opTypes = [
  "same", "create", "update", "delete", "replace",
  "create-replacement", "delete-replaced", "read", "read-replacement",
  "refresh", "discard", "discard-replaced", "remove-pending-replace",
  "import", "import-replacement",
] as const
const _severities = ["info", "info#err", "warning", "error"] as const

const _Op = Schema.Literal(..._ops)
const _OpType = Schema.Literal(..._opTypes)

class RunReceipt extends Schema.Class<RunReceipt>("RunReceipt")({
  op: _Op,
  stack: Schema.NonEmptyString,
  summary: Schema.Record({ key: _OpType, value: Schema.Int }).pipe(Schema.partialWith({ exact: true })),
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
}) {
  static readonly ops: RunReceipt.Ops = _ops
  static readonly opTypes: RunReceipt.OpTypes = _opTypes
  get mutated(): boolean {
    return Record.reduce(this.summary, 0, (acc, count, op) => (op === "same" ? acc : acc + count)) > 0
  }
}

declare namespace RunReceipt {
  type Ops = typeof _ops
  type OpTypes = typeof _opTypes
  type Op = (typeof _ops)[number]
  type OpType = (typeof _opTypes)[number]
  type Step = RunReceipt["steps"][number]
  type Severity = (typeof _severities)[number]
}
```

## [3]-[DEPLOY_FAULT]

[DEPLOY_FAULT]:
- Owner: `DeployFault`, the deploy plane's one fault family — a `Data.TaggedError` whose `reason` row discriminates every failure route, with the exported `FaultPolicy` anchor carrying rank, retryability, and halting posture per reason; the class projects its row as `policy`, so recovery reads data, never a reason `switch`.
- Law: the family is in-process by construction — a deploy fault never crosses a wire (the receipt does), so the zero-codec `Data.TaggedError` form is correct and promotion to `Schema.TaggedError` rewrites only the declaration.
- Law: `triaged(stack)` is the one foreign-value conversion — a `Match.instanceOf` ladder over the engine's thrown classes (`ConcurrentUpdateError`, `StackNotFoundError`, `StackAlreadyExistsError`, the `CommandError` base, `InputPropertiesError`/`InputPropertyError`, `RunError`) with `Match.orElse` minting the `alien` quarantine row, subclasses matched before their base; every `Effect.tryPromise` catch slot in the folder names this triage and no second conversion exists.
- Law: only `concurrent` retries — a state-lock collision is the one transient cause, its recurrence riding a caller-composed `Schedule` gated `Schedule.whileInput` on `fault.policy.retry`; `cancelled` is not a failure posture (rank 1, non-halting) because interruption is the caller's own verdict.
- Growth: a new cause is one `FaultPolicy` row plus one triage arm; a new policy axis is one row column every consumer reads through `policy`.
- Boundary: `ParseError` from receipt and output decodes re-spells into this family at the decode seam (`alien` with the parse message as detail); the rank fold over accumulated faults rides `DeployFault.byRank`.
- Packages: `effect` (`Data`, `Match`, `Order`, `pipe`); `@pulumi/pulumi` (`InputPropertiesError`, `InputPropertyError`, `RunError`); `@pulumi/pulumi/automation` (`CommandError` family).

```typescript
import { InputPropertiesError, InputPropertyError, RunError } from "@pulumi/pulumi"
import { CommandError, ConcurrentUpdateError, StackAlreadyExistsError, StackNotFoundError } from "@pulumi/pulumi/automation"
import { Data, Match, Order, pipe } from "effect"

const FaultPolicy = {
  cancelled: { rank: 1, retry: false, halting: false },
  concurrent: { rank: 2, retry: true, halting: false },
  absent: { rank: 3, retry: false, halting: true },
  duplicate: { rank: 3, retry: false, halting: true },
  command: { rank: 4, retry: false, halting: true },
  input: { rank: 4, retry: false, halting: true },
  diagnostic: { rank: 5, retry: false, halting: true },
  alien: { rank: 6, retry: false, halting: true },
} as const

declare namespace FaultPolicy {
  type Reason = keyof typeof FaultPolicy
  type Row = { readonly rank: number; readonly retry: boolean; readonly halting: boolean }
  type _Rows<T extends { readonly [K in Reason]: Row } = typeof FaultPolicy> = T
}

class DeployFault extends Data.TaggedError("DeployFault")<{
  readonly reason: FaultPolicy.Reason
  readonly stack: string
  readonly detail: string
}> {
  static readonly byRank: Order.Order<DeployFault> = Order.mapInput(Order.number, (fault: DeployFault) => fault.policy.rank)
  static readonly triaged = (stack: string): ((caught: unknown) => DeployFault) =>
    pipe(
      Match.type<unknown>(),
      Match.when(Match.instanceOf(ConcurrentUpdateError), (e) => new DeployFault({ reason: "concurrent", stack, detail: e.message })),
      Match.when(Match.instanceOf(StackNotFoundError), (e) => new DeployFault({ reason: "absent", stack, detail: e.message })),
      Match.when(Match.instanceOf(StackAlreadyExistsError), (e) => new DeployFault({ reason: "duplicate", stack, detail: e.message })),
      Match.when(Match.instanceOf(CommandError), (e) => new DeployFault({ reason: "command", stack, detail: e.message })),
      Match.when(Match.instanceOf(InputPropertiesError), (e) => new DeployFault({ reason: "input", stack, detail: e.message })),
      Match.when(Match.instanceOf(InputPropertyError), (e) => new DeployFault({ reason: "input", stack, detail: e.message })),
      Match.when(Match.instanceOf(RunError), (e) => new DeployFault({ reason: "diagnostic", stack, detail: e.message })),
      Match.orElse((residue) => new DeployFault({ reason: "alien", stack, detail: String(residue) })),
    )
  get policy(): (typeof FaultPolicy)[FaultPolicy.Reason] {
    return FaultPolicy[this.reason]
  }
}
```

## [4]-[AUTOMATION_RUN]

[AUTOMATION_RUN]:
- Owner: `Automation` — `stack` acquires the idempotent workspace, `run` drives one mutating ledger op to a receipt, `reconcile` runs the read-only `previewRefresh` leg to the same receipt shape, `receipt` folds an event batch, `ephemeral` brackets a stack whose release destroys it, and `snapshot`/`restore` are the state-lifecycle pair over `exportStack`/`importStack` — backup before a risky ledger op, restore as the operator's disaster verb. The `_host` Config surface is the one deploy-host read: `PULUMI_BACKEND_URL` selects the self-managed state store, `PULUMI_CONFIG_PASSPHRASE` rides `Config.redacted` and unwraps exactly once into `envVars`, `PULUMI_PROJECT` defaults, `PULUMI_HOME` is optional — no `Pulumi.yaml` exists because `projectSettings` carries the same facts programmatically.
- Law: the driver is one exhaustive record — `_LEDGER` maps each op, `reconcile` included, to its `Stack` method under a mapped contract, so a sixth op is a compile error at the record; `up`/`preview` receive the policy-pack and gate options, `refresh`/`destroy`/`reconcile` the minimal projection, and every arm receives `signal` and `onEvent`. The `reconcile` arm calls `previewRefresh` — the engine's non-mutating reconcile — so a mutating `refresh` remains a deliberate ledger choice a human or workflow makes after reading drift evidence.
- Law: interruption is structural — `Effect.async` hands its `AbortSignal` into the op's `signal` slot, so fiber interruption or scope close aborts the engine run and an orphaned update is unspellable; the settle callback is the platform-forced statement seam (the engine is promise-and-callback shaped), and nothing above it leaves expression shape.
- Law: the receipt folds from the event stream only — `summary` from the last `SummaryEvent`, steps from every `resourcePreEvent`, diagnostics from every `DiagnosticEvent` — never from per-op result shapes, so all five arms share one fold; the assembled record decodes through `RunReceipt` and a decode failure re-spells as an `alien` fault.
- Law: `Automation.Options` carries call-local intent only — `parallel`, `expectNoChanges`, `policyPacks`/`policyPackConfigs` (the `operate/policy.md` guard attachment), `refresh` — and the option arrays keep the engine's mutable-array spelling because the record is a boundary mirror consumed once.
- Law: the deploy host obeys the injection law — the automation process itself runs under `doppler run`, which is how `PULUMI_CONFIG_PASSPHRASE`, the bootstrap `DOPPLER_TOKEN`, and the provider material reads resolve; one injection mechanism spans the deploy host and every deployed process.
- Entry: `Automation.stack(spec, program)` then `Automation.run(stack, spec.name, "up", { policyPacks })`; `Automation.reconcile(stack, spec.name)` for the standing drift read; `Automation.ephemeral(spec, program)` under a `Scope` for review stacks.
- Growth: a new host fact is one `_host` row; a new call-local option is one `Options` field threaded to the arms that accept it; ESC attachment (`Stack.addEnvironments`) and batch adoption (`Stack.import`) are the demoted prepared members an app's own spec data revives.
- Boundary: the `PulumiFn` the stack runs is `provider.md`'s product; the drift projection over `reconcile` receipts is `operate/policy.md`'s.
- Packages: `effect` (`Config`, `Effect`, `Option`, `Redacted`, `Schema`, `Array`, `pipe`); `@pulumi/pulumi/automation` (`LocalWorkspace`, `Stack`, `EngineEvent`, `PulumiFn`, `Deployment`).

```typescript
import { LocalWorkspace, type Deployment, type EngineEvent, type PulumiFn, type Stack } from "@pulumi/pulumi/automation"
import { Array, Config, Effect, Option, pipe, Redacted, Schema, type Scope } from "effect"
import { StackSpec } from "./spec.ts"

const _host = Config.unwrap({
  backend: Config.string("PULUMI_BACKEND_URL"),
  passphrase: Config.redacted("PULUMI_CONFIG_PASSPHRASE"),
  project: Config.string("PULUMI_PROJECT").pipe(Config.withDefault("rasm")),
  home: Config.option(Config.string("PULUMI_HOME")),
})

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

const _folded = (op: RunReceipt.Op, stack: string, events: ReadonlyArray<EngineEvent>): unknown => ({
  op,
  stack,
  summary: pipe(
    Array.filterMap(events, (event) => Option.fromNullable(event.summaryEvent)),
    Array.last,
    Option.map((summary) => summary.resourceChanges),
    Option.getOrElse(() => ({})),
  ),
  steps: Array.filterMap(events, (event) =>
    Option.map(Option.fromNullable(event.resourcePreEvent), (pre) => ({
      op: pre.metadata.op,
      urn: pre.metadata.urn,
      type: pre.metadata.type,
      provider: pre.metadata.provider,
      ...(pre.metadata.diffs !== undefined && { changed: pre.metadata.diffs }),
    }))),
  diagnostics: Array.filterMap(events, (event) =>
    Option.map(Option.fromNullable(event.diagnosticEvent), (diag) => ({
      severity: diag.severity,
      message: diag.message,
      ...(diag.urn !== undefined && { urn: diag.urn }),
    }))),
})

declare namespace Automation {
  type Options = {
    readonly parallel?: number
    readonly expectNoChanges?: boolean
    readonly refresh?: boolean
    readonly policyPacks?: Array<string>
    readonly policyPackConfigs?: Array<string>
  }
}

const _driven = (stack: Stack, name: string, op: RunReceipt.Op, options?: Automation.Options): Effect.Effect<RunReceipt, DeployFault> =>
  Effect.flatMap(
    Effect.async<ReadonlyArray<EngineEvent>, DeployFault>((resume, signal) => {
      const events: Array<EngineEvent> = []
      const onEvent = (event: EngineEvent): void => void events.push(event)
      _LEDGER[op](stack, { ...options, signal, onEvent }).then(
        () => resume(Effect.succeed(events)),
        (caught) => resume(Effect.fail(DeployFault.triaged(name)(caught))),
      )
    }),
    (events) => Automation.receipt(op, name, events),
  )

const Automation = {
  stack: (spec: StackSpec, program: PulumiFn): Effect.Effect<Stack, DeployFault> =>
    Effect.flatMap(
      Effect.mapError(_host, (issue) => new DeployFault({ reason: "input", stack: spec.name, detail: String(issue) })),
      (host) =>
        Effect.tryPromise({
          try: () =>
            LocalWorkspace.createOrSelectStack(
              { stackName: spec.name, projectName: host.project, program },
              {
                projectSettings: { name: host.project, runtime: "nodejs", backend: { url: host.backend } },
                secretsProvider: "passphrase",
                envVars: { PULUMI_CONFIG_PASSPHRASE: Redacted.value(host.passphrase) },
                ...(Option.isSome(host.home) && { pulumiHome: host.home.value }),
              },
            ),
          catch: DeployFault.triaged(spec.name),
        }),
    ),
  run: (stack: Stack, name: string, op: Exclude<RunReceipt.Op, "reconcile">, options?: Automation.Options): Effect.Effect<RunReceipt, DeployFault> =>
    _driven(stack, name, op, options),
  reconcile: (stack: Stack, name: string): Effect.Effect<RunReceipt, DeployFault> =>
    _driven(stack, name, "reconcile"),
  receipt: (op: RunReceipt.Op, name: string, events: ReadonlyArray<EngineEvent>): Effect.Effect<RunReceipt, DeployFault> =>
    Effect.mapError(
      Schema.decodeUnknown(RunReceipt)(_folded(op, name, events)),
      (parse) => new DeployFault({ reason: "alien", stack: name, detail: parse.message }),
    ),
  snapshot: (stack: Stack, name: string): Effect.Effect<Deployment, DeployFault> =>
    Effect.tryPromise({ try: () => stack.exportStack(), catch: DeployFault.triaged(name) }),
  restore: (stack: Stack, name: string, state: Deployment): Effect.Effect<void, DeployFault> =>
    Effect.tryPromise({ try: () => stack.importStack(state), catch: DeployFault.triaged(name) }),
  ephemeral: (spec: StackSpec, program: PulumiFn): Effect.Effect<Stack, DeployFault, Scope.Scope> =>
    Effect.acquireRelease(Automation.stack(spec, program), (stack) =>
      Effect.orDie(_driven(stack, spec.name, "destroy"))),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Automation, DeployFault, FaultPolicy, RunReceipt }
```
