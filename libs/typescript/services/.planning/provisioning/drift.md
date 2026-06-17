# [SERVICES_STACK_DRIFT]

The desired-vs-actual reconciliation fold over the Pulumi engine event stream — `StackDrift`, the `previewRefresh` drift fold that re-reads provider state read-only and classifies each per-URN `StepEventMetadata` `OpType` into a typed `DriftResource`, then folds the per-URN `HashMap` into a self-validating `StackDriftSummary` receipt. A mandatory divergence fails CI the same engine-time way a `PolicyGuard` rule does. `StackDrift` is a ROW on the provisioning surface (`provisioning/contract#PROVISIONING`), not a parallel owner: it reuses the `AutomationDriver`-resolved `Stack`. The `@pulumi/*` types it folds stay inside the `./provisioning` subpath closure; the summary it returns carries only primitives, so no `@pulumi/*` type escapes onto the durable hot path.

## [1]-[INDEX]

One cluster: `[2]-[PROVISIONING]` owns the `previewRefresh` drift fold, the typed `StackDriftSummary` receipt, and the CI drift gate.

## [2]-[PROVISIONING]

- Owner: `StackDrift`, the desired-vs-actual reconciliation fold over the engine event stream — `diff` returns the typed `StackDriftSummary` receipt, `gate` is the mandatory arm that fails `divergent` when the summary is not clean.
- Cases: `StackDrift` computes the desired-vs-actual reconciliation diff by driving the Automation API `previewRefresh` (a read-only refresh-preview that re-reads provider state without mutating it) with an `onEvent` callback bridged into an Effect via `Effect.async`, folding each `resourcePreEvent.metadata` `StepEventMetadata` `OpType` into a per-URN `DriftResource` `Data.TaggedEnum` (`Added` for an `op:"create"`/`"import"` the program declares but the provider lacks, `Removed` for a `delete`/`delete-replaced`/`remove-pending-replace`/`discard`/`discard-replaced` the provider holds but the program no longer declares, `Changed` for an `update`/`replace`/`create-replacement`/`import-replacement`/`read-replacement` carrying the `detailedDiff` `Record<string, PropertyDiff>` property-path delta, `Unchanged` for a `same`/`refresh`/`read`), then `$match`-folding the per-URN `HashMap` into a typed `StackDriftSummary` (added/removed/changed resource arrays + a `clean` predicate), so drift is a typed receipt rather than a stdout scrape and a `mandatory` divergence fails CI the same engine-time way a `PolicyGuard` rule does. `PreviewResult.changeSummary` (the engine's own per-OpType `OpMap`) is captured alongside so the folded buckets reconcile against the engine's count, making the receipt self-validating.
- Entry: the fold reuses the `AutomationDriver`-resolved `Stack` (`provisioning/contract#PROVISIONING`); `AutomationDriver` grows one `drift` verb on the SAME command tree so a divergent stack exits non-zero engine-time, and a scheduled drift sweep registers as a `runtime-backplane/backplane#RUNNER_AND_SCHEDULING` cluster singleton or shard-pinned cron emitting a typed drift receipt to the `ObservabilityStack`.
- Packages: `@pulumi/pulumi` (the `automation` module: `EngineEvent`/`OpType`/`PreviewResult`/`PropertyDiff`/`StepEventMetadata`/`Stack`/`DiffKind`), `@effect/cli` for the `drift` verb on the existing tree, and `effect` for the `Data.TaggedEnum` classification, the `HashMap` accumulator, the `Match.exhaustive` fold over the 15-member `OpType` union, and the `Schema.Class` receipt.
- Growth: a new drift classification lands as one `DriftResource` `Data.TaggedEnum` variant breaking the `$match` fold at compile time, never a parallel diff surface; the scheduled sweep lands as one singleton/cron layer.
- Boundary: `StackDrift` is no exception to the subpath rule — the `EngineEvent`/`StepEventMetadata`/`PropertyDiff` types it folds are `@pulumi/pulumi/automation` types consumed inside the subpath closure, the `StackDriftSummary` it returns is a domain receipt carrying only primitive URN/type/property-path strings, so no `@pulumi/*` type escapes onto the durable hot path; this is a node-only deploy-time surface, never browser-reachable.

```ts drift
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import type { EngineEvent, OpType, PreviewResult, PropertyDiff, StepEventMetadata, Stack } from "@pulumi/pulumi/automation";
import type { AutomationDriver } from "./contract.js";
import { DiffKind } from "@pulumi/pulumi/automation";
import { Command, Options } from "@effect/cli";
import { Array as Arr, Data, Effect, HashMap, Match, Option, pipe, Schema } from "effect";

// --- [TYPES] ---------------------------------------------------------------------------
// One closed family for the per-resource verdict; the engine OpType axis re-closes into the
// domain on fold. Added/Removed/Changed/Unchanged are the four drift verdicts; `$match` is the
// exhaustive fold and a fifth verdict is a compile-time break across every fold site.
type DriftResource = Data.TaggedEnum<{
  readonly Added: { readonly urn: string; readonly type: string };
  readonly Removed: { readonly urn: string; readonly type: string };
  readonly Changed: { readonly urn: string; readonly type: string; readonly diff: ReadonlyArray<PropertyChange> };
  readonly Unchanged: { readonly urn: string; readonly type: string };
}>;

// The detailedDiff Record<string, PropertyDiff> re-closes into a flat property-path row carrying
// only primitives so no @pulumi/* type escapes the subpath onto the durable hot path.
type PropertyChange = {
  readonly path: string;
  readonly kind: DiffKind;
  readonly inputDiff: boolean;
};

const DriftResource = Data.taggedEnum<DriftResource>();

// --- [MODELS] --------------------------------------------------------------------------
// The drift summary IS the receipt: three resource arrays plus a clean predicate, decoded from
// primitives only. One Model.Class, projections derive — no parallel added/removed/changed schema.
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

// --- [ERRORS] --------------------------------------------------------------------------
// One boundary fault for the deploy-time refresh-preview; the engine-stage cause is the payload,
// not a parallel error rail. A mandatory divergence is signalled by the dedicated `divergent` stage
// so the @effect/cli verb fails CI engine-time the way a PolicyGuard rule does.
class DriftFault extends Schema.TaggedError<DriftFault>()("DriftFault", {
  stack: Schema.String,
  stage: Schema.Literal("refresh", "decode", "divergent"),
  detail: Schema.String,
}) {}

// --- [SERVICES] ------------------------------------------------------------------------
// StackDrift is a ROW on the provisioning surface, not a parallel owner: it reuses the
// AutomationDriver-resolved Stack and returns the typed summary receipt. `gate` is the mandatory
// arm — it re-runs the diff and fails `divergent` when the summary is not clean.
interface StackDrift {
  readonly diff: (stack: Stack, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault>;
  readonly gate: (stack: Stack, stackName: string) => Effect.Effect<StackDriftSummary, DriftFault>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
// classifyStep: the engine OpType axis -> the domain DriftResource family. `previewRefresh` emits
// at most one resourcePreEvent per URN; `op:"same"`/`"refresh"` is no drift, the mutating ops are the
// three drift verdicts. Match.exhaustive guards the full 15-member OpType union at compile time.
const classifyStep = (m: StepEventMetadata): DriftResource =>
  Match.value(m.op).pipe(
    Match.when("create", () => DriftResource.Added({ urn: m.urn, type: m.type })),
    Match.when("import", () => DriftResource.Added({ urn: m.urn, type: m.type })),
    Match.whenOr("delete", "delete-replaced", "remove-pending-replace", "discard", "discard-replaced", () =>
      DriftResource.Removed({ urn: m.urn, type: m.type }),
    ),
    Match.whenOr("update", "replace", "create-replacement", "import-replacement", "read-replacement", () =>
      DriftResource.Changed({ urn: m.urn, type: m.type, diff: detailedDiff(m.detailedDiff) }),
    ),
    Match.whenOr("same", "refresh", "read", () => DriftResource.Unchanged({ urn: m.urn, type: m.type })),
    Match.exhaustive,
  );

// detailedDiff: Record<string, PropertyDiff> -> flat PropertyChange rows; the optional record folds to
// an empty array (a `replace` with no detailedDiff is still a Changed verdict carrying zero paths).
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

// foldVerdicts: the per-URN HashMap accumulator -> the StackDriftSummary via one `$match` fold over
// the closed family. `Unchanged` contributes to no bucket; the three drift buckets project to the
// three Model arrays. HashMap.values is the immutable accumulator drained once, no .set mutation.
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

// mutatedCount: the authoritative drift cardinality from PreviewResult.changeSummary (the engine's own
// per-OpType OpMap), summing every mutating OpType and excluding the three no-drift ops. The Match over
// the closed OpType union decides drift membership; `same`/`refresh`/`read` contribute zero.
const NO_DRIFT_OPS: ReadonlyArray<OpType> = ["same", "refresh", "read"];
const mutatedCount = (summary: PreviewResult["changeSummary"]): number =>
  pipe(
    Object.entries(summary) as ReadonlyArray<readonly [OpType, number]>,
    Arr.reduce(0, (total, [op, count]) => (Arr.contains(NO_DRIFT_OPS, op) ? total : total + count)),
  );

// computeStackDrift: the transcription-complete drift-diff body. `previewRefresh` re-reads provider
// state read-only; the structured `onEvent` callback is bridged into an Effect via `Effect.async`, the
// per-URN verdict accumulated into a HashMap closed over the callback (boundary-local mutable handle,
// drained into the immutable summary on resolve). The last verdict per URN wins (refresh emits one).
// `PreviewResult.changeSummary` is captured alongside so the folded buckets reconcile against the
// engine's own count, making the receipt self-validating and catching any onEvent/promise race.
const computeStackDrift = (stack: Stack, stackName: string): Effect.Effect<StackDriftSummary, DriftFault> =>
  Effect.async<readonly [HashMap.HashMap<string, DriftResource>, PreviewResult], DriftFault>((resume) => {
    let verdicts = HashMap.empty<string, DriftResource>();
    const onEvent = (event: EngineEvent): void => {
      const meta = Option.fromNullable(event.resourcePreEvent?.metadata);
      Option.match(meta, {
        onNone: () => undefined,
        onSome: (m) => {
          verdicts = HashMap.set(verdicts, m.urn, classifyStep(m));
        },
      });
    };
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

// driftGate: the mandatory arm — a clean summary passes through, a divergent summary fails `divergent`
// so the @effect/cli `drift` verb exits non-zero in CI exactly as a mandatory PolicyGuard rule does.
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

// --- [COMPOSITION] ---------------------------------------------------------------------
const stackDrift: StackDrift = { diff: computeStackDrift, gate: driftGate };

// AutomationDriver grows one `drift` verb on the SAME command tree (one command on the existing tree,
// not a parallel CLI): it resolves the Stack via `driver.stack`, runs `gate`, and the DriftFault is the
// verb's typed failure channel so a divergent stack exits non-zero engine-time.
const driftVerb = (driver: AutomationDriver): Command.Command<"drift", never, DriftFault, { readonly stack: string }> =>
  Command.make("drift", { stack: Options.text("stack") }, ({ stack }) =>
    driver.stack(stack).pipe(
      Effect.flatMap((s) => stackDrift.gate(s, stack)),
      Effect.scoped,
      Effect.asVoid,
    ),
  );
```
