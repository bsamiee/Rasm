# [SERVICES_STACK_DRIFT]

The desired-vs-actual reconciliation fold over the Pulumi engine event stream — `StackDrift`, the `previewRefresh` drift fold that re-reads provider state read-only and classifies each per-URN `StepEventMetadata` `OpType` into a typed `DriftResource`, then folds the per-URN `HashMap` into a self-validating `StackDriftSummary` receipt. A mandatory divergence fails CI the same engine-time way a `PolicyGuard` rule does. `StackDrift` is a ROW on the provisioning surface (`provisioning/contract#PROVISIONING`), not a parallel owner: it reuses the `AutomationDriver`-resolved `Stack`. The `@pulumi/*` types it folds stay inside the `./provisioning` subpath closure; the summary it returns carries only primitives, so no `@pulumi/*` type escapes onto the durable hot path.

## [1]-[INDEX]

One cluster: `[2]-[PROVISIONING]` owns the `previewRefresh` drift fold, the typed `StackDriftSummary` receipt, the continuous drift sweep, and the CI drift gate.

## [2]-[PROVISIONING]

- Owner: `StackDrift`, the desired-vs-actual reconciliation fold over the engine event stream — `diff` returns the typed `StackDriftSummary` receipt, `gate` is the mandatory arm that fails `divergent` when the summary is not clean; and `DriftSweep`, the continuous drift owner registering the fold as a `runtime-backplane` shard-pinned cron that emits the primitive receipt to the `ObservabilityStack`.
- Cases: `StackDrift` computes the desired-vs-actual reconciliation diff by driving the Automation API `previewRefresh` (a read-only refresh-preview that re-reads provider state without mutating it) with an `onEvent` callback bridged into an Effect via `Effect.async`, folding each `resourcePreEvent.metadata` `StepEventMetadata` `OpType` into a per-URN `DriftResource` `Data.TaggedEnum` (`Added` for an `op:"create"`/`"import"` the program declares but the provider lacks, `Removed` for a `delete`/`delete-replaced`/`remove-pending-replace`/`discard`/`discard-replaced` the provider holds but the program no longer declares, `Changed` for an `update`/`replace`/`create-replacement`/`import-replacement`/`read-replacement` carrying the `detailedDiff` `Record<string, PropertyDiff>` property-path delta, `Unchanged` for a `same`/`refresh`/`read`), then `$match`-folding the per-URN `HashMap` into a typed `StackDriftSummary` (added/removed/changed resource arrays + a `clean` predicate), so drift is a typed receipt rather than a stdout scrape and a `mandatory` divergence fails CI the same engine-time way a `PolicyGuard` rule does. `PreviewResult.changeSummary` (the engine's own per-OpType `OpMap`) is captured alongside so the folded buckets reconcile against the engine's count, making the receipt self-validating. `DriftSweep` is the continuous arm: a `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` `ScheduledWork.cron` registers a shard-pinned scheduled execution that resolves the active ESC policy-pack environment through `@pulumi/esc-sdk` `EscApi.openAndReadEnvironment`, runs the `computeStackDrift` fold off the deploy CLI, and emits ONLY the `StackDriftSummary` primitive receipt into the `ObservabilityStack` collector — exactly-once per the cron contract, never double-running because the shard pin places it on one runner.
- Entry: the fold reuses the `AutomationDriver`-resolved `Stack` (`provisioning/contract#PROVISIONING`); `AutomationDriver` grows one `drift` verb on the SAME command tree so a divergent stack exits non-zero engine-time, and the `DriftSweep` cron registers as a `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` shard-pinned scheduled execution emitting the typed drift receipt to the `provisioning/contract#PROVISIONING` `ObservabilityStack`. The sweep stays deploy-time-only inside the `./provisioning` subpath closure: the `@pulumi/*` types it folds never load on the durable hot path, and only the primitive-carrying receipt crosses the subpath boundary to observability.
- Packages: `@pulumi/pulumi` (the `automation` module: `EngineEvent`/`OpType`/`PreviewResult`/`PropertyDiff`/`StepEventMetadata`/`Stack`/`DiffKind`/`previewRefresh`/`RefreshOptions`), `@pulumi/esc-sdk` (`EscApi`/`DefaultClient`/`openAndReadEnvironment` for the policy-pack environment resolution at sweep time), `@effect/cli` for the `drift` verb on the existing tree, `@effect/cluster` for the shard-pinned `ScheduledWork.cron` the sweep registers on, and `effect` for the `Data.TaggedEnum` classification, the `HashMap` accumulator, the `Match.exhaustive` fold over the 15-member `OpType` union, and the `Schema.Class` receipt.
- Growth: a new drift classification lands as one `DriftResource` `Data.TaggedEnum` variant breaking the `$match` fold at compile time, never a parallel diff surface; the scheduled sweep lands as one `ScheduledWork.cron` layer row.
- Boundary: `StackDrift` and `DriftSweep` are no exception to the subpath rule — the `EngineEvent`/`StepEventMetadata`/`PropertyDiff` types they fold and the `EscApi` resolution are `@pulumi/*` deploy-time concerns consumed inside the subpath closure, the `StackDriftSummary` they return is a domain receipt carrying only primitive URN/type/property-path strings, so no `@pulumi/*` type escapes onto the durable hot path; the sweep emits ONLY the primitive receipt to the collector, never a `@pulumi/*` value; this is a node-only deploy-time surface, never browser-reachable.

```ts drift
import type { EngineEvent, OpType, PreviewResult, PropertyDiff, StepEventMetadata, Stack } from "@pulumi/pulumi/automation";
import type { Cron, Layer, Scope } from "effect";
import type { Sharding } from "@effect/cluster";
import type { EscApi } from "@pulumi/esc-sdk";
import type { AutomationDriver, ObservabilityStack } from "./contract.js";
import type { ScheduledWork } from "../runtime-backplane/backplane.js";
import { DiffKind } from "@pulumi/pulumi/automation";
import { Command, Options } from "@effect/cli";
import { Array as Arr, Data, Effect, HashMap, Match, Option, pipe, Schema } from "effect";

type DriftResource = Data.TaggedEnum<{
  readonly Added: { readonly urn: string; readonly type: string };
  readonly Removed: { readonly urn: string; readonly type: string };
  readonly Changed: { readonly urn: string; readonly type: string; readonly diff: ReadonlyArray<PropertyChange> };
  readonly Unchanged: { readonly urn: string; readonly type: string };
}>;

type PropertyChange = {
  readonly path: string;
  readonly kind: DiffKind;
  readonly inputDiff: boolean;
};

const DriftResource = Data.taggedEnum<DriftResource>();

class StackDriftSummary extends Schema.Class<StackDriftSummary>("StackDriftSummary")({
  stack: Schema.String,
  added: Schema.Array(Schema.Struct({ urn: Schema.String, type: Schema.String })),
  removed: Schema.Array(Schema.Struct({ urn: Schema.String, type: Schema.String })),
  changed: Schema.Array(
    Schema.Struct({
      urn: Schema.String,
      type: Schema.String,
      diff: Schema.Array(
        Schema.Struct({
          path: Schema.String,
          kind: Schema.Enums(DiffKind),
          inputDiff: Schema.Boolean,
        }),
      ),
    }),
  ),
}) {
  get clean(): boolean {
    return this.added.length === 0 && this.removed.length === 0 && this.changed.length === 0;
  }
}

class DriftFault extends Schema.TaggedError<DriftFault>()("DriftFault", {
  stack: Schema.String,
  stage: Schema.Literal("refresh", "decode", "divergent"),
  detail: Schema.String,
}) {}

interface StackDrift {
  readonly diff: (stack: Stack, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault>;
  readonly gate: (stack: Stack, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault>;
}

const classifyStep = (m: StepEventMetadata): DriftResource =>
  Match.value(m.op).pipe(
    Match.whenOr("create", "import", () => DriftResource.Added({ urn: m.urn, type: m.type })),
    Match.whenOr("delete", "delete-replaced", "remove-pending-replace", "discard", "discard-replaced", () =>
      DriftResource.Removed({ urn: m.urn, type: m.type }),
    ),
    Match.whenOr("update", "replace", "create-replacement", "import-replacement", "read-replacement", () =>
      DriftResource.Changed({ urn: m.urn, type: m.type, diff: detailedDiff(m.detailedDiff) }),
    ),
    Match.whenOr("same", "refresh", "read", () => DriftResource.Unchanged({ urn: m.urn, type: m.type })),
    Match.exhaustive,
  );

const detailedDiff = (record: Record<string, PropertyDiff> | undefined): ReadonlyArray<PropertyChange> =>
  pipe(
    Option.fromNullable(record),
    Option.map((r) =>
      pipe(
        Object.entries(r),
        Arr.map(([path, d]) => ({ path, kind: d.diffKind, inputDiff: d.inputDiff }) satisfies PropertyChange),
      ),
    ),
    Option.getOrElse(() => Arr.empty<PropertyChange>()),
  );

const foldVerdicts = (stack: string, verdicts: HashMap.HashMap<string, DriftResource>): StackDriftSummary =>
  pipe(
    HashMap.values(verdicts),
    Arr.fromIterable,
    Arr.reduce(
      { added: Arr.empty<{ urn: string; type: string }>(), removed: Arr.empty<{ urn: string; type: string }>(), changed: Arr.empty<{ urn: string; type: string; diff: ReadonlyArray<PropertyChange> }>() },
      (acc, v) =>
        DriftResource.$match(v, {
          Added: ({ urn, type }) => ({ ...acc, added: Arr.append(acc.added, { urn, type }) }),
          Removed: ({ urn, type }) => ({ ...acc, removed: Arr.append(acc.removed, { urn, type }) }),
          Changed: ({ urn, type, diff }) => ({ ...acc, changed: Arr.append(acc.changed, { urn, type, diff }) }),
          Unchanged: () => acc,
        }),
    ),
    (buckets) => new StackDriftSummary({ stack, ...buckets }),
  );

const NO_DRIFT_OPS: ReadonlyArray<OpType> = ["same", "refresh", "read"];
const mutatedCount = (summary: PreviewResult["changeSummary"]): number =>
  pipe(
    Object.entries(summary) as ReadonlyArray<readonly [OpType, number]>,
    Arr.reduce(0, (total, [op, count]) => (Arr.contains(NO_DRIFT_OPS, op) ? total : total + count)),
  );

const computeStackDrift = (stack: Stack, stackName: string): Effect.Effect<StackDriftSummary, DriftFault> =>
  Effect.async<readonly [HashMap.HashMap<string, DriftResource>, PreviewResult], DriftFault>((resume) => {
    let verdicts = HashMap.empty<string, DriftResource>();
    const onEvent = (event: EngineEvent): void =>
      Option.match(Option.fromNullable(event.resourcePreEvent?.metadata), {
        onNone: () => undefined,
        onSome: (m) => {
          verdicts = HashMap.set(verdicts, m.urn, classifyStep(m));
        },
      });
    stack.previewRefresh({ onEvent }).then(
      (result: PreviewResult) => resume(Effect.succeed([verdicts, result] as const)),
      (cause: unknown) =>
        resume(Effect.fail(new DriftFault({ stack: stackName, stage: "refresh", detail: String(cause) }))),
    );
  }).pipe(
    Effect.map(([verdicts, result]) => [foldVerdicts(stackName, verdicts), result] as const),
    Effect.filterOrFail(
      ([summary, result]) =>
        summary.added.length + summary.removed.length + summary.changed.length === mutatedCount(result.changeSummary),
      ([summary, result]) =>
        new DriftFault({
          stack: stackName,
          stage: "decode",
          detail: `folded ${String(summary.added.length + summary.removed.length + summary.changed.length)} != changeSummary ${String(mutatedCount(result.changeSummary))}`,
        }),
    ),
    Effect.map(([summary]) => summary),
  );

const driftGate = (stack: Stack, stackName: string): Effect.Effect<StackDriftSummary, DriftFault> =>
  computeStackDrift(stack, stackName).pipe(
    Effect.filterOrFail(
      (summary) => summary.clean,
      (summary) =>
        new DriftFault({
          stack: stackName,
          stage: "divergent",
          detail: `+${summary.added.length} -${summary.removed.length} ~${summary.changed.length}`,
        }),
    ),
  );

const stackDrift: StackDrift = { diff: computeStackDrift, gate: driftGate };

const driftVerb = (driver: AutomationDriver): Command.Command<"drift", never, DriftFault, { readonly stack: string }> =>
  Command.make("drift", { stack: Options.text("stack") }, ({ stack }) =>
    driver.stack(stack).pipe(
      Effect.flatMap((s) => stackDrift.gate(s, stack)),
      Effect.scoped,
      Effect.asVoid,
    ),
  );

type DriftEnvironment = {
  readonly org: string;
  readonly project: string;
  readonly environment: string;
};

interface DriftSweep {
  readonly resolveEnvironment: (api: EscApi, env: DriftEnvironment) => Effect.Effect<Record<string, unknown>, DriftFault>;
  readonly sweep: (driver: AutomationDriver, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault, Scope.Scope>;
  readonly layer: (schedule: ScheduledWork, options: {
    readonly driver: AutomationDriver;
    readonly observe: ObservabilityStack;
    readonly api: EscApi;
    readonly environment: DriftEnvironment;
    readonly stackName: string;
    readonly cron: Cron.Cron;
    readonly shardGroup?: string;
  }) => Layer.Layer<never, never, Sharding>;
}

const resolveEnvironment = (api: EscApi, env: DriftEnvironment): Effect.Effect<Record<string, unknown>, DriftFault> =>
  Effect.tryPromise({
    try: () => api.openAndReadEnvironment(env.org, env.project, env.environment),
    catch: (cause) => new DriftFault({ stack: env.environment, stage: "refresh", detail: String(cause) }),
  }).pipe(Effect.map((response) => response.values ?? {}));

const sweepStack = (driver: AutomationDriver, stackName: string): Effect.Effect<StackDriftSummary, DriftFault, Scope.Scope> =>
  driver.stack(stackName).pipe(Effect.flatMap((s) => computeStackDrift(s, stackName)));

const driftSweepLayer = (
  schedule: ScheduledWork,
  options: {
    readonly driver: AutomationDriver;
    readonly observe: ObservabilityStack;
    readonly api: EscApi;
    readonly environment: DriftEnvironment;
    readonly stackName: string;
    readonly cron: Cron.Cron;
    readonly shardGroup?: string;
  },
): Layer.Layer<never, never, Sharding> =>
  schedule.cron({
    name: `drift-sweep/${options.stackName}`,
    cron: options.cron,
    shardGroup: options.shardGroup,
    execute: resolveEnvironment(options.api, options.environment).pipe(
      Effect.zipRight(Effect.scoped(sweepStack(options.driver, options.stackName))),
      Effect.flatMap((summary) => options.observe.emit(summary)),
      Effect.catchAll((fault) => options.observe.emit(new StackDriftSummary({ stack: fault.stack, added: [], removed: [], changed: [] }))),
    ),
  });

const driftSweep: DriftSweep = { resolveEnvironment, sweep: sweepStack, layer: driftSweepLayer };
```
