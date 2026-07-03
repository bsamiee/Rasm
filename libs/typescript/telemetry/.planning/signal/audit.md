# [TELEMETRY_AUDIT]

The audit stream is a durable fact family, not a log level: `AuditFact` is one `Schema.Class` owning the actor/action/target vocabulary, typed diff evidence as a closed change family, and retention as policy rows carrying their keep-window — and it drains through the fact-journal port law this page declares for the whole folder: a generic `Audit.Journal<F>` port shape with one `JournalFault`, a Tag per stream, satisfied at the app root by `store` journal Layers because the edge ledger keeps `telemetry -> store` absent. The `Audit` service stamps identity and time, buffers through a bounded queue, and drains in latency-bounded batches — the journal append is the fact's system of record, and a structured log emission rides beside it for observability, never instead of it.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                    |
| :-----: | :------------- | :-------------------------------------------------------------------------- |
|  [01]   | [FACT]         | the `AuditFact` owner: actor, action, target, change family, retention rows  |
|  [02]   | [JOURNAL_PORT] | the folder-wide `FactJournal` port shape, `JournalFault`, the audit Tag      |
|  [03]   | [RAIL]         | the `Audit` service: stamped recording, buffered drain, beside-log emission  |

## [2]-[FACT]

[FACT]:
- Owner: `AuditFact` — one rich `Schema.Class`: the actor block (`kind` row over `user`/`service`/`system` plus the principal key), the dotted-verb `action` brand, the target block (`kind`, `key`, optional parent for hierarchy), the `change` array over the closed `Change` union, the `retention` row key, the identity stamp (`app`, `tenant`), the `at` instant, and an `Option`-carried trace correlation.
- Law: diff evidence is the closed `Change` family — `Assigned { path, next }`, `Shifted { path, prior, next }`, `Cleared { path, prior }` — with `path` a JSON-pointer-shaped brand and values carried as JSON-encodable strings at the evidence altitude; a free-form `details` bag is the rejected shape because policy cannot fold what it cannot type.
- Law: retention is a policy table — `operational` (90 days), `compliance` (7 years), `forensic` (10 years) — whose row the fact references by key and projects through the `keep` getter; the store-side sweep enforces the window, this owner declares it, and a new class is one row every consumer inherits.
- Law: the action brand is the dotted verb path (`entity.amend`, `session.revoke`) — pattern-refined at the field so the vocabulary stays greppable and dashboard-groupable without a central verb registry; the actor/action/target attribute NAMES live on `Convention.rasm`, the fact SHAPE lives here.
- Receipt: the encoded twin derives (`typeof AuditFact.Encoded`) — the shape the store journal Layer persists and the C#-side auditor reads by name-level parity; no hand wire twin exists.
- Growth: a new evidence kind is one `Change` case plus its arm in consumers' folds; a new retention class is one `_retention` row.

```typescript
import { Duration, Schema } from "effect"

const _ACTORS = ["user", "service", "system"] as const

const _retention = {
  compliance: { keep: Duration.decode("2557 days") },
  forensic: { keep: Duration.decode("3653 days") },
  operational: { keep: Duration.decode("90 days") },
} as const

const _Path = Schema.String.pipe(Schema.pattern(/^(\/[^/~]*(~[01][^/~]*)*)*$/), Schema.brand("ChangePath"))
const _Action = Schema.String.pipe(Schema.pattern(/^[a-z][a-z0-9]*(\.[a-z][a-z0-9]*)+$/), Schema.brand("AuditAction"))

const Assigned = Schema.TaggedStruct("Assigned", { next: Schema.String, path: _Path })
const Cleared = Schema.TaggedStruct("Cleared", { path: _Path, prior: Schema.String })
const Shifted = Schema.TaggedStruct("Shifted", { next: Schema.String, path: _Path, prior: Schema.String })

const Change: Schema.Union<[typeof Assigned, typeof Cleared, typeof Shifted]> = Schema.Union(Assigned, Cleared, Shifted)
type Change = typeof Change.Type

class AuditFact extends Schema.Class<AuditFact>("AuditFact")({
  action: _Action,
  actor: Schema.Struct({ key: Schema.NonEmptyString, kind: Schema.Literal(..._ACTORS) }),
  app: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  change: Schema.Array(Change),
  retention: Schema.Literal("compliance", "forensic", "operational"),
  target: Schema.Struct({
    key: Schema.NonEmptyString,
    kind: Schema.NonEmptyString,
    parent: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  }),
  tenant: Schema.NonEmptyString,
  trace: Schema.optionalWith(Schema.String, { as: "Option" }),
}) {
  get keep(): Duration.Duration {
    return _retention[this.retention].keep
  }
}

declare namespace AuditFact {
  type Retention = keyof typeof _retention
  type Wire = typeof AuditFact.Encoded
  type _Rows<T extends Record<Retention, { readonly keep: Duration.Duration }> = typeof _retention> = T
}
```

## [3]-[JOURNAL_PORT]

[JOURNAL_PORT]:
- Owner: the folder's fact-journal port law, declared once here — the generic `Audit.Journal<F>` shape (`append` over a proven-non-empty batch), the one `JournalFault` (a `Schema.TaggedError` carrying the stream discriminant, the refused count, and retryability as evidence), and the `Audit.Journal` Tag instantiating the shape at `AuditFact`; `signal/meter` mints its own Tag against the same shape and fault, so the two streams are rows on one port law, never two port designs.
- Law: the port exists because the ledger forbids the edge — `telemetry` declares the Tag against its own fact model, `store` journal Layers satisfy it at the app root, and a Tag minted where an import is legal would be the named defect; the shape is sized for the family (batch-only append, typed fault, no read surface) because telemetry writes facts and never queries them — reads are `store` projections.
- Law: `append` takes `NonEmptyReadonlyArray<F>` — plurality is proven at the type, so a drained empty window is unspellable and the implementor never guards emptiness.
- Boundary: which table, upcasting, and the retention sweep are the store journal Layer's law; this page owns the contract's shape and fault only.
- Growth: a new durable fact stream elsewhere in the folder is one Tag row against `Audit.Journal<F>` — the shape and fault are closed.

```typescript
import type { Array, Effect } from "effect"
import { Context, Schema } from "effect"

class JournalFault extends Schema.TaggedError<JournalFault>()("JournalFault", {
  count: Schema.Int,
  retryable: Schema.Boolean,
  stream: Schema.Literal("audit", "meter"),
}) {}

type _FactJournal<F> = {
  readonly append: (facts: Array.NonEmptyReadonlyArray<F>) => Effect.Effect<void, JournalFault>
}

class _AuditJournal extends Context.Tag("telemetry/AuditJournal")<_AuditJournal, _FactJournal<AuditFact>>() {}
```

## [4]-[RAIL]

[RAIL]:
- Owner: the `Audit` service — a Layer factory over the app's `AppIdentity` (`Audit.Default(identity)`), holding the bounded intake queue and the scoped drain fiber; `record` is the one accessor, the Tag and fault ride the class as `Audit.Journal`/`Audit.JournalFault` statics with their types on the merged namespace, so the whole audit surface travels one import.
- Law: `record` takes a `Draft` — actor, action, target, change, retention, and the optional trace — and the service stamps what the caller must not control: `app`/`tenant` from the identity, `at` from the clock on the rail; construction runs the schema filters, so a malformed draft fails the writer, never the drain.
- Law: the drain is one pipeline — `Stream.fromQueue` over the intake, `Stream.groupedWithin(width, patience)` so a quiet surface still flushes on latency, the batch appended through the port under a jittered bounded retry gated on `JournalFault.retryable`, the drained-batch count tracked by the `Convention.metric.auditDrained` counter — and backpressure is the queue's `bounded` contract: a saturated intake suspends writers rather than dropping evidence.
- Law: every recorded fact also emits one structured log annotated with the `Convention.rasm` audit rows — observability beside durability; the journal append is the system of record, and a consumer wanting audit search reads the store projection, never the log stream.
- Entry: `Audit.record(draft)`; wiring is `Audit.Default(identity)` provided the store-owned journal Layer at the root.
- Growth: a new stamped dimension is one line in the stamp; a new drain policy axis is one `_FLOW` field.

```typescript
import { Array, Chunk, DateTime, Effect, Metric, Queue, Schedule, Stream } from "effect"
import type { AppIdentity } from "@rasm/ts/kernel"
import { Convention } from "@rasm/ts/telemetry"

const _FLOW = { intake: 256, patience: "2 seconds", width: 64 } as const

const _drained = Metric.counter(Convention.metric.auditDrained, { incremental: true })

const _RETRY = Schedule.exponential("100 millis").pipe(
  Schedule.jittered,
  Schedule.intersect(Schedule.recurs(5)),
  Schedule.whileInput((fault: JournalFault) => fault.retryable),
)

class Audit extends Effect.Service<Audit>()("telemetry/Audit", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const journal = yield* _AuditJournal
      const intake = yield* Queue.bounded<AuditFact>(_FLOW.intake)
      yield* Effect.forkScoped(
        Stream.fromQueue(intake).pipe(
          Stream.groupedWithin(_FLOW.width, _FLOW.patience),
          Stream.runForEach((batch) => {
            const rows = Chunk.toReadonlyArray(batch)
            return Array.isNonEmptyReadonlyArray(rows)
              ? journal.append(rows).pipe(
                  Effect.retry(_RETRY),
                  Effect.andThen(Metric.increment(_drained)),
                  Effect.catchAll((fault) =>
                    Effect.logError("audit drain refused").pipe(Effect.annotateLogs({ count: fault.count }))),
                )
              : Effect.void
          }),
        ),
      )
      return {
        record: (draft: Audit.Draft): Effect.Effect<void> =>
          Effect.gen(function* () {
            const at = yield* DateTime.now
            const fact = new AuditFact({ ...draft, app: identity.app, at, tenant: identity.tenant })
            yield* Queue.offer(intake, fact)
            yield* Effect.logInfo("audit").pipe(
              Effect.annotateLogs({
                [Convention.rasm.auditAction]: fact.action,
                [Convention.rasm.auditActorKey]: fact.actor.key,
                [Convention.rasm.auditActorKind]: fact.actor.kind,
                [Convention.rasm.auditRetention]: fact.retention,
                [Convention.rasm.auditTargetKey]: fact.target.key,
                [Convention.rasm.auditTargetKind]: fact.target.kind,
              }),
            )
          }),
      }
    }),
  accessors: true,
}) {
  static readonly Journal = _AuditJournal
  static readonly JournalFault = JournalFault
}

declare namespace Audit {
  type Draft = Omit<typeof AuditFact.Type, "app" | "at" | "tenant">
  type Journal<F> = _FactJournal<F>
  type JournalFault = InstanceType<typeof JournalFault>
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Audit, AuditFact }
```
