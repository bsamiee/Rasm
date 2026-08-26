# [IAC_AUTOMATION]

`Automation` drives inline programs through `LocalWorkspace.createOrSelectStack`, returns each Pulumi lifecycle method's native result, and keeps deploy-host configuration, interruption, retry, timeout, and tracing on one Effect boundary. Fleet configuration, state, history, and hosted execution remain direct projections of the workspace and stack APIs. `DeployFault` owns the engine's thrown failure family.

## [01]-[INDEX]

- [02]-[DEPLOY_FAULT]: `DeployFault` reason family, policy table, and triage.
- [03]-[AUTOMATION_RUN]: host facts, native lifecycle results, internalized resilience, the fleet verbs; `Automation`.

## [02]-[DEPLOY_FAULT]

[DEPLOY_FAULT]:
- Law: `triaged(stack)` is the one foreign-value conversion — a `Match.instanceOf` ladder over the engine's thrown classes (`ConcurrentUpdateError`, `StackNotFoundError`, `StackAlreadyExistsError`, the `CommandError` base, `InputPropertiesError`/`InputPropertyError`, `RunError`) with `Match.orElse` minting the `alien` row, subclasses matched before their base; every `Effect.tryPromise` catch slot in the folder names this triage and no second conversion exists.
- Law: a run that outlives its ceiling is its own reason — `budget` names the timeout after the scope-close abort, so the discriminant rides the closed vocabulary instead of a sentinel string a reader must match inside a free detail field, and the ceiling crosses as the measured `Duration` the run was given rather than as text no reader can compare.
- Law: every reason declares its own subject and renderer, so engine failures carry the stack identity and foreign message while `budget` carries the ceiling it outlived.
- Law: legs partition by the surface that DECIDED — `run` for the drive this owner brackets end to end, `workspace` for the stack identity and configuration it selects against, `engine` for what the pulumi engine itself reported — so a census reads which surface refused without re-deriving it from the reason.
- Boundary: `ParseError` from output decodes maps into this family at the decode boundary; the severity fold over accumulated faults rides `DeployFault.bySeverity`.

```typescript
import { InputPropertiesError, InputPropertyError, RunError } from "@pulumi/pulumi"
import { CommandError, ConcurrentUpdateError, StackAlreadyExistsError, StackNotFoundError } from "@pulumi/pulumi/automation"
import { Fault } from "@rasm/core"
import { Duration, Match, Order, Schema, pipe } from "effect"

const _Triaged = Schema.Struct({ stack: Schema.String, detail: Schema.String })
const _Overran = Schema.Struct({ stack: Schema.String, ceiling: Schema.DurationFromSelf })

const _family = Fault.Class.family(
  ["concurrent", "absent", "duplicate", "input", "command", "budget", "diagnostic", "alien"] as const,
  {
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

## [03]-[AUTOMATION_RUN]

[AUTOMATION_RUN]:
- Owner: `Automation` — `stack` acquires the idempotent workspace, `run` returns the selected lifecycle method's native result, `reconcile` returns `PreviewResult` from the read-only `previewRefresh` leg, and the fleet members project the corresponding workspace or stack operation directly.
- Law: `_operations` maps each operation to its `Stack` method and derives the selected method's native result through indexed `ReturnType`; `reconcile` calls `previewRefresh`, while mutating `refresh` remains an explicit operation.
- Law: `Effect.tryPromise` supplies the fiber-bound `AbortSignal`, and the caller's `onEvent` callback remains the direct engine observation point.
- Law: recurrence is one operation row under one local bound — `_CONTENDED` composes `Fault.Budget.schedule("bulk")` and intersects the deploy's own re-drive ceiling, so jitter, quiet reset, and the elapsed window are the branch's single decision and a tuning change lands at the operation map; the class gate is that member's own default, so `conflicted` stays the one reason a run re-drives and no local predicate restates it; `Effect.timeoutFail` bounds the orthogonal axis — the schedule rations how often a wedged lock is re-driven, the timeout caps how long one attempt lives.
- Law: the deploy host exports through the runtime plane's `Export.live`; the run span carries the qualified stack name and operation while Pulumi's callback carries engine events directly.
- Law: the run span, `history`, and `series` use `fullyQualifiedStackName("organization", host.project, name)`; fault triage retains the caller's slug because it can fire before host facts resolve.
- Law: `config` discriminates by input shape — absent reads the whole map (`getAllConfig`), a string reads one key (`getConfig`), a `[key, value]` tuple writes one (`setConfig`), a `ConfigMap` writes bulk (`setAllConfig`), and `{ refresh: true }` re-pulls from the backend (`refreshConfig`) — five modalities on one member whose ladder reads evidence the value carries, so no `getConfig`/`setConfig` sibling family exists on this owner.
- Law: `series` projects `UpdateSummary` rows into version, operation, result, elapsed seconds, and resource changes.
- Law: `_host` resolves from the doppler-injected process env with the `.env` fallback provided as `PlatformConfigProvider.layerDotEnv` at the composition root, and `NodeRuntime.runMain(program.pipe(Effect.provide(root)))` is the automation process's imperative edge.
- Law: `Automation.Options` derives from the run-option interface with `signal` reserved for `Effect.tryPromise`; the option arrays keep the engine's mutable-array spelling at the boundary.
- Law: the deploy host obeys the injection law — the automation process itself runs under `doppler run`, which is how `PULUMI_CONFIG_PASSPHRASE`, the bootstrap `DOPPLER_TOKEN`, and the provider material reads resolve; one injection mechanism spans the deploy host and every deployed process.
- Entry: `Automation.stack(spec, program)` then `Automation.run(stack, spec.name, "up", { policyPacks })`; `Automation.reconcile(stack, spec.name)` for the standing drift read; `Automation.series(stack, spec.name)` for the regression read; `Automation.ephemeral(spec, program)` under a `Scope` for review stacks; `Automation.adopt(stack, spec.name, resources)` to absorb a pre-existing deployment; `Automation.attach(stack, spec.name, envs)` after `operate/cloud.md` authors the environment.
- Packages: `@pulumi/pulumi/automation` (the workspace, stack, lifecycle-result, and engine-event surface); `effect` (`Config`, `Duration`, `Effect`, `Schedule`, `Schema`); `@rasm/core` (`Fault.Budget`).
- Growth: a new host fact is one `_host` row; a new call-local option is one `_RunOpts` field inherited by `Options` mechanically; a new fleet verb is one member over the engine method that carries it; a new config modality is one `_config` overload line plus one ladder arm.
- Boundary: the `PulumiFn` the stack runs is `provider.md`'s product; `operate/policy.md` consumes `PreviewResult` from `reconcile`; hosted schedule and webhook resources remain `operate/cloud.md`'s.

```typescript
import {
  fullyQualifiedStackName, LocalWorkspace, PulumiCommand, RemoteWorkspace,
  type ConfigMap, type ConfigValue, type Deployment, type DestroyResult, type EngineEvent, type ImportResource, type PreviewResult,
  type PulumiFn, type RefreshResult, type RemoteGitProgramArgs, type RemoteStack, type Stack, type StackSummary, type UpdateSummary,
  type UpResult, type WhoAmIResult,
} from "@pulumi/pulumi/automation"
import { Fault } from "@rasm/core"
import { Array, Config, Duration, Effect, Option, Predicate, Record, Redacted, Schedule, Schema, type Scope } from "effect"
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
  readonly onEvent?: (event: EngineEvent) => void
  readonly budget?: Duration.DurationInput
  readonly parallel?: number
  readonly expectNoChanges?: boolean
  readonly refresh?: boolean
  readonly policyPacks?: Array<string>
  readonly policyPackConfigs?: Array<string>
}

const _operations = {
  up: (stack: Stack, { signal, onEvent, parallel, expectNoChanges, refresh, policyPacks, policyPackConfigs }: _RunOpts): Promise<UpResult> =>
    stack.up({ signal, onEvent, parallel, expectNoChanges, refresh, policyPacks, policyPackConfigs }),
  preview: (stack: Stack, { signal, onEvent, parallel, expectNoChanges, refresh, policyPacks, policyPackConfigs }: _RunOpts): Promise<PreviewResult> =>
    stack.preview({ signal, onEvent, parallel, expectNoChanges, refresh, policyPacks, policyPackConfigs }),
  refresh: (stack: Stack, { signal, onEvent, parallel }: _RunOpts): Promise<RefreshResult> => stack.refresh({ signal, onEvent, parallel }),
  destroy: (stack: Stack, { signal, onEvent, parallel }: _RunOpts): Promise<DestroyResult> => stack.destroy({ signal, onEvent, parallel }),
  reconcile: (stack: Stack, { signal, onEvent, parallel }: _RunOpts): Promise<PreviewResult> => stack.previewRefresh({ signal, onEvent, parallel }),
} as const satisfies Record<string, (stack: Stack, opts: _RunOpts) => Promise<unknown>>

const _driven = <K extends Automation.Op>(
  stack: Stack,
  name: string,
  op: K,
  options?: Automation.Options,
): Effect.Effect<Awaited<ReturnType<(typeof _operations)[K]>>, DeployFault> =>
  Effect.flatMap(_facts(name), (host) =>
    ((qualified, ceiling) =>
      Effect.tryPromise({
        try: (signal): ReturnType<(typeof _operations)[K]> => _operations[op](stack, { ...options, signal }) as ReturnType<(typeof _operations)[K]>,
        catch: DeployFault.triaged(name),
      }).pipe(
        Effect.timeoutFail({
          duration: ceiling,
          onTimeout: () => new DeployFault({ case: { reason: "budget", stack: name, ceiling } }),
        }),
        Effect.retry(_CONTENDED),
        Effect.withSpan("iac.automation.run", { attributes: { stack: qualified, op } }),
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
  type Op = keyof typeof _operations
  type Options = Omit<_RunOpts, "signal">
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
  run: <K extends Exclude<Automation.Op, "reconcile">>(stack: Stack, name: string, op: K, options?: Automation.Options): Effect.Effect<Awaited<ReturnType<(typeof _operations)[K]>>, DeployFault> =>
    _driven(stack, name, op, options),
  reconcile: (stack: Stack, name: string, options?: Automation.Options): Effect.Effect<PreviewResult, DeployFault> =>
    _driven(stack, name, "reconcile", options),
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

export { Automation, DeployFault }
```
