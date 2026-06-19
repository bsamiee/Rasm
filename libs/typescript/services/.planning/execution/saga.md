# [SERVICES_SAGA]

The durable saga compensation owner: a multi-step durable transaction folded from a closed `SagaStep` chain over the cluster-backed `WorkflowEngine`, never a parallel saga type family. Each step is a forward/compensating pair; rollback rides `Workflow.withCompensation`, the engine-owned reverse-order finalizer, on `Cause` of a later step. The saga's success/error/payload schemas are the workflow's OWN schemas (success encodes `SagaTerminal`), so the start/`Discard`/`Resume` procedures derive over the SAME wire `Schema` through the one `WorkflowProxy`. This page crosses no .NET wire.

## [1]-[INDEX]

- [1]-[SAGA]: owns the `SagaStep` chain, the `StepOutcome`/`SagaTerminal` fold, and the engine-compensated saga workflow.

## [2]-[SAGA]

- Owner: `SagaOwner` and `SagaStep`, the saga step chain and the workflow that folds it; `sagaFold`, the total reduction over the closed `StepOutcome` stream to one `SagaTerminal`.
- Cases: each `SagaStep` is a forward/compensating pair — the forward effect is one `Activity` carrying its own `retryPolicy`, timeout, and `Activity.idempotencyKey` so a re-attempt after a restart never re-applies a side effect, and the compensating effect is registered with that same activity through `Workflow.withCompensation` so it runs in reverse declaration order ONLY for steps that committed, automatically, on any failure or interruption of a later step. The fold is `sagaFold` over the closed `StepOutcome` union (`Forward`/`Compensating`/`Skipped`), reduced by `StepOutcome.$match`/`SagaTerminal.$match` (the generated total `Data.TaggedEnum` matchers) so the saga's terminal outcome — `committed` when every forward arm succeeds, `rolled-back` when a forward arm fails and the committed prefix compensates, `aborted` when a compensation finalizer itself faults — is total and adding an outcome or terminal without a fold arm is a typecheck failure. The `StepOutcome` stream is a REAL mixed stream assembled across two seams: `runStep` emits one `Forward({ name, attempt })` per committed step (the `Activity.CurrentAttempt`-tagged success), and `sagaBody`'s `Effect.tapErrorCause` synthesizes the failure tail — the committed prefix, one `Compensating({ name, cause })` for the failing step, and `Skipped({ name })` for every step the fault skipped; `sagaFold` then reduces that assembled stream to the `SagaTerminal` the workflow's success `Schema` carries.
- Entry: the saga's success/error/payload schemas are the workflow's OWN schemas (success encodes `SagaTerminal`), so the saga workflow becomes callable only by feeding it into `InternalRpc.fromWorkflows` (messaging/rpc#INTERNAL_RPC), which derives the start/`Discard`/`Resume` procedures over the SAME wire `Schema` the saga workflow defines — a saga-local proxy member, a second `RpcGroup`, or a hand-built saga DTO is the named defect. Every step boundary writes one `AgentJournal` row through a `Model.makeRepository` `insertVoid` over the one `SqlClient` `persistence/store#STORE_BOUNDARY` owns (`checkpoint` on forward commit, `failed` on compensation) so the saga's progress is durably replayable, never a parallel ledger.
- Packages: `@effect/workflow` for `Workflow.withCompensation`, `Activity`, and the workflow algebra; `@effect/cluster` for the engine the workflow body resolves; `@effect/sql` and `@effect/sql-pg` for the journal rows through the persistence boundary.
- Growth: a new saga step lands as one `SagaStep` row appended to the chain (forward `Activity` + compensating finalizer), never a new saga workflow type; a new saga terminal lands as one case on the `StepOutcome` union that `sagaFold` must then handle.
- Boundary: the saga is one workflow whose compensation rides `Workflow.withCompensation` (the engine-owned reverse-order finalizer), never a hand-rolled try/rollback or a parallel `SagaCoordinator` service; rollback is driven by the engine on `Cause` of a later step, so the fold NEVER catches a defect to ROLL BACK — actual compensation runs as the engine's exit finalizer. `sagaBody`'s `Effect.tapErrorCause` only OBSERVES the failing step's `Cause` into the `Compensating` `StepOutcome` and the journal `failed` row (for the `sagaFold` terminal); it re-emits no recovery effect and triggers no compensation, so the engine's exit-finalizer model remains the sole rollback driver. `runStep` is invoked only at the workflow body's TOP LEVEL (`sagaBody`'s `Effect.forEach`), because `Workflow.withCompensation` registers finalizers for top-level effects only — nesting a step inside an outer activity silently voids its rollback guarantee. The saga workflow is callable only through the ONE `WorkflowProxy`-derived `RpcGroup` over its own `Schema`.

```ts contract
interface SagaStep<S extends Schema.Schema.Any, E extends Schema.Schema.All, R> {
  readonly name: string;
  readonly forward: Activity.Activity<S, E, R>;
  readonly compensate: (value: Schema.Schema.Type<S>, cause: Cause.Cause<Schema.Schema.Type<E>>) => Effect.Effect<void, never, R>;
  readonly retryPolicy: Schedule.Schedule<unknown, Schema.Schema.Type<E>>;
  readonly timeout: Duration.Duration;
}

type StepOutcome = Data.TaggedEnum<{
  readonly Forward: { readonly name: string; readonly attempt: number };
  readonly Compensating: { readonly name: string; readonly cause: Cause.Cause<unknown> };
  readonly Skipped: { readonly name: string };
}>;
const StepOutcome = Data.taggedEnum<StepOutcome>();

type SagaTerminal = Data.TaggedEnum<{
  readonly Committed: { readonly steps: ReadonlyArray<string> };
  readonly RolledBack: { readonly failedAt: string; readonly compensated: ReadonlyArray<string> };
  readonly Aborted: { readonly failedAt: string; readonly compensationFault: DurableFault };
}>;
const SagaTerminal = Data.taggedEnum<SagaTerminal>();

const sagaFold = (outcomes: ReadonlyArray<StepOutcome>): SagaTerminal =>
  Array.reduce(outcomes, SagaTerminal.Committed({ steps: [] }) as SagaTerminal, (acc, outcome) =>
    StepOutcome.$match(outcome, {
      Forward: ({ name }) =>
        SagaTerminal.$match(acc, {
          Committed: ({ steps }) => SagaTerminal.Committed({ steps: [...steps, name] }),
          RolledBack: (t) => SagaTerminal.RolledBack(t),
          Aborted: (t) => SagaTerminal.Aborted(t),
        }),
      Compensating: ({ name, cause }) =>
        SagaTerminal.$match(acc, {
          Committed: () =>
            Cause.isDie(cause)
              ? SagaTerminal.Aborted({ failedAt: name, compensationFault: DurableFault.Compensated({ name }) })
              : SagaTerminal.RolledBack({ failedAt: name, compensated: [name] }),
          RolledBack: ({ failedAt, compensated }) => SagaTerminal.RolledBack({ failedAt, compensated: [...compensated, name] }),
          Aborted: (t) => SagaTerminal.Aborted(t),
        }),
      Skipped: () => acc,
    }),
  );

const runStep = <S extends Schema.Schema.Any, E extends Schema.Schema.All, R>(
  step: SagaStep<S, E, R>,
): Effect.Effect<StepOutcome, Schema.Schema.Type<E>, R | WorkflowEngine | WorkflowInstance | Scope.Scope> =>
  Activity.idempotencyKey(step.name, { includeAttempt: false }).pipe(
    Effect.zipRight(step.forward),
    Effect.timeoutTo({ duration: step.timeout, onTimeout: () => Effect.interrupt, onSuccess: Effect.succeed }),
    Effect.flatten,
    Effect.retry({ schedule: step.retryPolicy }),
    Workflow.withCompensation((value, cause) =>
      step.compensate(value, cause).pipe(
        Effect.catchAllCause((compCause) => Effect.logError("saga.compensate.fault", compCause)),
      ),
    ),
    Effect.flatMap(() => Activity.CurrentAttempt.pipe(Effect.map((attempt) => StepOutcome.Forward({ name: step.name, attempt })))),
  );

interface SagaOwner {
  readonly make: <const Name extends string, P extends Schema.Struct.Fields, S extends Schema.Schema.Any, E extends Schema.Schema.All>(options: {
    readonly name: Name;
    readonly payload: P;
    readonly success: S;
    readonly error: E;
    readonly idempotencyKey: (payload: Schema.Struct.Type<P>) => string;
    readonly steps: (payload: Schema.Struct.Type<P>) => NonEmptyReadonlyArray<SagaStep<Schema.Schema.Any, Schema.Schema.All, never>>;
  }) => {
    readonly workflow: Workflow.Workflow<Name, Schema.Struct<P>, S, E>;
    readonly layer: Layer.Layer<never, never, WorkflowEngine | SqlClient.SqlClient>;
  };
}

const sagaBody = <E extends Schema.Schema.All>(sessionId: string) =>
  (steps: NonEmptyReadonlyArray<SagaStep<Schema.Schema.Any, E, never>>): Effect.Effect<SagaTerminal, Schema.Schema.Type<E>, WorkflowEngine | WorkflowInstance | Scope.Scope | SqlClient.SqlClient> =>
    Effect.all([Ref.make<ReadonlyArray<StepOutcome>>([]), Model.makeRepository(AgentJournal, { tableName: "agent_journal", spanPrefix: "saga", idColumn: "id" })]).pipe(
      Effect.flatMap(([committed, journal]) =>
        Effect.forEach(
          steps,
          (step) =>
            runStep(step).pipe(
              Effect.zipLeft(Effect.annotateCurrentSpan("saga.step", step.name)),
              Effect.tap((outcome) => Ref.update(committed, (xs) => [...xs, outcome])),
              Effect.tap(() => journal.insertVoid({ sessionId, kind: "checkpoint", status: "completed", payload: { step: step.name } })),
            ),
          { concurrency: 1 },
        ).pipe(
          Effect.map((forwards) => sagaFold(forwards)),
          Effect.tapErrorCause((cause) =>
            Ref.get(committed).pipe(
              Effect.flatMap((prefix) => {
                const failedAt = steps[prefix.length]?.name ?? steps[steps.length - 1].name;
                const stream: ReadonlyArray<StepOutcome> = [
                  ...prefix,
                  StepOutcome.Compensating({ name: failedAt, cause }),
                  ...Array.drop(steps, prefix.length + 1).map((s) => StepOutcome.Skipped({ name: s.name })),
                ];
                return journal.insertVoid({ sessionId, kind: "tool_call", status: "failed", payload: { step: failedAt, cause: Cause.pretty(cause) } }).pipe(
                  Effect.zipRight(Effect.logWarning("saga.rolled-back", sagaFold(stream))),
                );
              }),
            ),
          ),
        ),
      ),
    );

const SagaOwnerLive: SagaOwner = {
  make: (options) => {
    const workflow = Workflow.make({
      name: options.name,
      payload: options.payload,
      success: options.success,
      error: options.error,
      idempotencyKey: options.idempotencyKey,
    });
    const layer = workflow.toLayer((payload, _executionId) =>
      sagaBody(_executionId)(options.steps(payload)) as Effect.Effect<Schema.Schema.Type<S>, Schema.Schema.Type<E>, never>,
    );
    return { workflow, layer } as ReturnType<SagaOwner["make"]>;
  },
};
```
