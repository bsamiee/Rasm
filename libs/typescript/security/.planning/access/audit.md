# [SECURITY_AUDIT]

Security fact rail: breach evidence is receipt-truth on one typed `SecurityFact` plane, and metrics stay the lossy dashboard channel beside it. Every loud arm the folder fires — refresh reuse, webauthn clone, ceremony refusal, secret rotation, policy deny, key-admission quarantine, shred-open reject — publishes one tagged fact through `Witness`, a `Context.Reference` whose default is silent: an arm composes one total publish line beside its `Reject.mark`/log call, verdict paths stay byte-identical, and a library composition without the rail pays zero requirement pressure. Registry state is app-scoped data — the closed `_points` table brands every fact with its `rasm.security.<domain>.<point>` key and its observe/veto modality, `Audit.live(identity)` binds the live rail per app root so two apps in one process never share a mutable registry, and subscribers consume their own scoped `PubSub` streams on their own fibers, so a subscriber fault degrades one observer and can never reach a publisher or a verdict. Durability is class-routed lane policy: `breached`-class facts (reuse, clone, shred-open) ride the bounded suspend lane — backpressure is the deliberate coupling, because dropped breach evidence costs more than publish latency — while `notice`-class facts ride the sliding lane, and both drain into the append-only `AuditJournal` port under a jittered retry gated on `FaultClass.retryable`; the data wave satisfies the port on its journal spine, so audit facts age, export, and crypto-shred under the same law as every durable fact. Analytics egress is pseudonymized by construction: `Audit.egress` folds each record through the `Pseudonym` port — the app root satisfies it with `Crypto.sign` under a dedicated egress pepper, a KEYED mac, because an unkeyed digest over low-entropy subject identifiers is dictionary-reversible and therefore never a pseudonymizer — and the `AuditTrace` wire carries masked subjects and bounded facets only, so no PII leaves to an analytics store. Board and alert planes stay with their one owners, and this page only feeds them: `Audit.board` composes the core `DashboardModel` security pack, `Audit.objectives` declares the folder's latency objectives over the `Convention.instrument.security*` histogram rows, `Alert.of` alone derives their burn specs, tenant grouping rides the `$tenant` variable under the `Convention.rasm.tenant` governor, and zero-tolerance paging on breached facts is a fact-rail subscriber at the app root — never a hand alert rule and never a parallel dashboard family. `Audit.snapshot` is the exporter-free support bundle: the core `Metric.snapshot` read twin filtered to the folder instrument set and sealed into a typed receipt.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                      | [PUBLIC]                                 |
| :-----: | :------------ | :-------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `FACT_FAMILY` | the tagged fact vocabulary, the point/class/modality table, the fault shape | `SecurityFact`, `AuditFault`             |
|  [02]   | `FACT_RAIL`   | the journal record, the append-only port, the publish seam, lane policy     | `AuditRecord`, `AuditJournal`, `Witness` |
|  [03]   | `EGRESS`      | the keyed-mac pseudonymizer port and the PII-free analytics projection      | `Pseudonym`, `AuditTrace`                |
|  [04]   | `BOARD`       | the snapshot receipt, latency objectives, alert derivation, assembled owner | `Audit`, `Snapshot`                      |

## [02]-[FACT_FAMILY]

[FACT_FAMILY]:
- Owner: `SecurityFact` — one closed `Schema.Union` of tagged cases mirroring the folder's loud arms: `Reuse` (a replayed rotated refresh), `Clone` (a webauthn counter regression), `Ceremony` (a challenge replay, intent mismatch, or stale phase), `Rotation` (an observed secret-set change), `Deny` (a policy denial), `Admission` (a quarantined JWKS entry), `ShredOpen` (an open refused on a shredded or tampered key). Its `_points` table is the registry brand: one row per case carrying the full `rasm.security.<domain>.<point>` key under the dotted-brand guard, the `breached`/`notice` class that routes lane policy and paging, and the observe/veto modality — every current row is observe, so publication is evidence and no fact can alter a verdict. `AuditFault` instantiates the folder fault shape at the rail's two failure sites.
- Law: facts are wire-carried by construction — they cross to the journal and the analytics egress, so every case is a `Schema.TaggedStruct` and the union decodes; a process-local `Data.taggedEnum` here forbids the port crossing the cards exist for.
- Law: fields carry evidence as data at the owning page's grain — `Deny.action`/`Deny.reason` hold the claim page's closed permission and denial vocabularies, `Ceremony.intent` the ceremony intent pair, `Rotation.coordinate` the `(project, config)` custody coordinate — and an identifier-grade field (`sid`, `passkey`, `subject`) is journal material the egress projection masks or drops, never a metric facet.
- Law: the guard pair closes the plane in both directions — `_Points` proves every union tag has a registry row with a branded key, `_Keys` proves every row has a case — so a fact without a point or a point without a fact fails at this declaration.
- Law: a veto row runs pre-commit inside the owning arm's rail and joins its `E` channel; the security plane's current rows are all observe, and the modality axis exists so an admission-gating point lands as one row flip with its arm's typed fault, never a second registry.
- Growth: a new loud arm is one case, one `_points` row, and one publish line at its arm; a new fact field is a case field the journal inherits.
- Packages: `effect` (`Schema`, `Option`); `@rasm/ts/core` (`FaultClass`, `TenantContext`).

```typescript
import { Alert, type AppIdentity, Convention, DashboardModel, FaultClass, Objective, Sli, TenantContext } from "@rasm/ts/core"
import {
  Array, Context, DateTime, Duration, Effect, Layer, Match, Number, Option, PubSub, Queue, Record, Schedule, Schema, Stream, pipe,
} from "effect"

const _kinds = ["Admission", "Ceremony", "Clone", "Deny", "Reuse", "Rotation", "ShredOpen"] as const

const _points = {
  Admission: { class: "notice", key: "rasm.security.crypt.admission", modality: "observe" },
  Ceremony: { class: "notice", key: "rasm.security.webauthn.ceremony", modality: "observe" },
  Clone: { class: "breached", key: "rasm.security.webauthn.clone", modality: "observe" },
  Deny: { class: "notice", key: "rasm.security.access.deny", modality: "observe" },
  Reuse: { class: "breached", key: "rasm.security.session.reuse", modality: "observe" },
  Rotation: { class: "notice", key: "rasm.security.secret.rotation", modality: "observe" },
  ShredOpen: { class: "breached", key: "rasm.security.crypt.shred", modality: "observe" },
} as const

const _Admission = Schema.TaggedStruct("Admission", {
  kid: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  detail: Schema.String,
})
const _Ceremony = Schema.TaggedStruct("Ceremony", {
  subject: Schema.NonEmptyString,
  intent: Schema.Literal("enroll", "assert"),
})
const _Clone = Schema.TaggedStruct("Clone", {
  subject: Schema.NonEmptyString,
  passkey: Schema.NonEmptyString,
})
const _Deny = Schema.TaggedStruct("Deny", {
  subject: Schema.NonEmptyString,
  action: Schema.NonEmptyString,
  reason: Schema.NonEmptyString,
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
})
const _Reuse = Schema.TaggedStruct("Reuse", {
  subject: Schema.NonEmptyString,
  sid: Schema.NonEmptyString,
  tenant: Schema.optionalWith(TenantContext.fields.tenant, { as: "Option" }),
})
const _Rotation = Schema.TaggedStruct("Rotation", {
  coordinate: Schema.NonEmptyString,
})
const _ShredOpen = Schema.TaggedStruct("ShredOpen", {
  detail: Schema.String,
})

const _Fact = Schema.Union(_Admission, _Ceremony, _Clone, _Deny, _Reuse, _Rotation, _ShredOpen)
type SecurityFact = typeof _Fact.Type

declare namespace SecurityFact {
  type Kind = SecurityFact["_tag"]
  type Class = (typeof _points)[Kind]["class"]
  type Modality = "observe" | "veto"
  type Point = (typeof _points)[Kind]["key"]
  type _Points<T extends Record<Kind, { readonly class: "breached" | "notice"; readonly key: `rasm.security.${string}.${string}`; readonly modality: Modality }> = typeof _points> = T
  type _Keys<K extends Kind = keyof typeof _points> = K
  type _Kinds<K extends (typeof _kinds)[number] = Kind> = K
}

const SecurityFact: {
  readonly Admission: typeof _Admission.make
  readonly Ceremony: typeof _Ceremony.make
  readonly Clone: typeof _Clone.make
  readonly Deny: typeof _Deny.make
  readonly Reuse: typeof _Reuse.make
  readonly Rotation: typeof _Rotation.make
  readonly ShredOpen: typeof _ShredOpen.make
  readonly classOf: (fact: SecurityFact) => SecurityFact.Class
  readonly pointOf: (fact: SecurityFact) => SecurityFact.Point
  readonly points: typeof _points
  readonly wire: typeof _Fact
} = {
  Admission: _Admission.make,
  Ceremony: _Ceremony.make,
  Clone: _Clone.make,
  Deny: _Deny.make,
  Reuse: _Reuse.make,
  Rotation: _Rotation.make,
  ShredOpen: _ShredOpen.make,
  classOf: (fact) => _points[fact._tag].class,
  pointOf: (fact) => _points[fact._tag].key,
  points: _points,
  wire: _Fact,
}

const _reasons = ["append", "mask"] as const

const _faults = {
  append: { class: "unavailable" },
  mask: { class: "defect" },
} as const

declare namespace AuditFault {
  type Reason = (typeof _reasons)[number]
  type _Rows<T extends Record<Reason, { readonly class: FaultClass.Kind }> = typeof _faults> = T
  type _Closed<K extends Reason = keyof typeof _faults> = K
}

class AuditFault extends Schema.TaggedError<AuditFault>()("AuditFault", {
  reason: Schema.Literal(..._reasons),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults[this.reason].class
  }
  override get message(): string {
    return `<audit:${this.reason}> ${this.detail}`
  }
}
```

## [03]-[FACT_RAIL]

[FACT_RAIL]:
- Owner: `AuditRecord` — the journal row sealing a fact with its occurrence instant, its owning app key, and its registry point, stamped once at publish so an arm never constructs a timestamp; `AuditJournal` — the append-only `Context.Tag` port the data wave satisfies on its journal spine, `append` on the `E`-channel fault law with no update or delete verb, because erasure is subject-key destruction on the data plane, never row mutation; `Witness` — the publish seam every arm composes: a `Context.Reference` whose default swallows, so a publish line is total, requirement-free, and inert until `Audit.live` binds the rail at an app root. `_LANES` is the class-routed capacity policy and `_APPEND_RETRY` the drain's recurrence value.
- Law: publication is evidence, never control flow — every current point is observe modality, `Witness.publish` returns `Effect<void>` with an empty error channel, and it composes through `Effect.tap*`/`Effect.zipRight` beside the arm's existing `Reject.mark`/log call, so a rail outage can never alter a security verdict and the verdict paths of every publishing page stay byte-identical.
- Law: durability is class-routed — a `breached` record offers into the bounded suspend lane, where a saturated journal deliberately backpressures the publisher because dropping breach evidence is the worse failure; a `notice` record offers into the sliding lane and sheds oldest under pressure, because notice-grade facts also project onto metrics; both lanes drain through `AuditJournal.append` under the jittered `_APPEND_RETRY` gated on `FaultClass.retryable`, and an exhausted append logs at error — the dashboard gap law, inverted: a journal gap is loud, a metric gap is not.
- Law: the observe fan is decoupled by construction — `Audit.live` publishes each record to a sliding `PubSub` beside the lanes, a subscriber consumes its own subscription stream on its own fiber, and unsubscription is the subscriber's scope closing, so a slow or faulted subscriber costs its own lag, never publish latency and never a sibling's delivery.
- Law: the registry is app-scoped — `Audit.live(identity)` stamps `identity.app` on every record and each app root binds its own `Witness`, so two apps composing this library in one process hold disjoint rails and disjoint subscriber sets with no shared mutable registry.
- Growth: a new lane class is one `_LANES` row with its `_points` class value; a new delivery guarantee is a drain policy edit, never a second rail.
- Boundary: the data wave maps `append` onto its one atomic journal write with the audit retention class and per-subject crypto-shred wiring; sealing subject-bearing fields under `SealedEnvelope`/`WrappedKey` is the data plane's, and this page never composes a store.
- Packages: `effect` (`Context`, `PubSub`, `Queue`, `Schedule`, `Stream`, `DateTime`); `@rasm/ts/core` (`FaultClass`).

```typescript
class AuditRecord extends Schema.Class<AuditRecord>("AuditRecord")({
  app: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  point: Schema.NonEmptyString,
  fact: _Fact,
}) {}

class AuditJournal extends Context.Tag("security/access/AuditJournal")<AuditJournal, {
  readonly append: (record: AuditRecord) => Effect.Effect<void, AuditFault>
}>() {}

class Witness extends Context.Reference<Witness>()("security/access/Witness", {
  defaultValue: (): { readonly publish: (fact: SecurityFact) => Effect.Effect<void> } => ({ publish: () => Effect.void }),
}) {
  static readonly publish = (fact: SecurityFact): Effect.Effect<void> =>
    Effect.flatMap(Witness, (witness) => witness.publish(fact))
}

const _LANES = { breached: 512, fan: 1024, notice: 2048 } as const

const _APPEND_RETRY = Schedule.exponential(Duration.millis(50)).pipe(Schedule.jittered, Schedule.intersect(Schedule.recurs(5)))
```

## [04]-[EGRESS]

[EGRESS]:
- Owner: `Pseudonym` — the keyed-mac port the analytics projection folds subjects through — and `AuditTrace`, the PII-free egress wire: app, instant, point, kind, the masked subject, the plain tenant scope, and a bounded facet record projected per case by one `Match.valueTags` dispatch, so what leaves to an analytics store is recoverable from this declaration alone.
- Law: the pseudonymizer is KEYED — the app root satisfies `Pseudonym` from `crypt/sign`'s `Crypto.sign` under a dedicated `Config.redacted` egress pepper, so masking is deterministic per key and irreversible without it; an unkeyed digest over subject identifiers is rejected outright, because subjects are low-entropy values a dictionary walk reverses, and the unkeyed `Crypto.fingerprint` projection stays admitted for high-entropy token lookup only.
- Law: the projection sheds by default — identifier-grade fields (`sid`, `passkey`, free `detail` prose) never reach the trace, bounded vocabulary fields (`intent`, `action`, `reason`, `coordinate`) ride the facet record, and the tenant crosses as its scope string because the partition key is an operational coordinate, not a person; a new case ships with its shed arm or the dispatch record fails to compile.
- Law: egress is a pull projection, never a lane — a compliance or analytics consumer folds `Audit.egress` over its own journal read or subscription, so the rail carries full evidence exactly once and every downstream trust grade derives its own view.
- Growth: a new analytics dimension is one facet key inside an existing shed arm; a new masking policy is a `Pseudonym` binding swap at the root.
- Boundary: which store receives `AuditTrace` values is the composing app's lake seam; this page owns only the projection and its masking law.
- Packages: `effect` (`Match`, `Option`, `Record`); `crypt/sign` (`Crypto.sign` as the root-side `Pseudonym` satisfaction, never an import here).

```typescript
class Pseudonym extends Context.Tag("security/access/Pseudonym")<Pseudonym, {
  readonly mask: (value: string) => Effect.Effect<string, AuditFault>
}>() {}

class AuditTrace extends Schema.Class<AuditTrace>("AuditTrace")({
  app: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  point: Schema.NonEmptyString,
  kind: Schema.Literal(..._kinds),
  subject: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  tenant: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  facet: Schema.Record({ key: Schema.String, value: Schema.String }),
}) {}

const _shed = (fact: SecurityFact): {
  readonly facet: Record.ReadonlyRecord<string, string>
  readonly subject: Option.Option<string>
  readonly tenant: Option.Option<string>
} =>
  Match.valueTags(fact, {
    Admission: () => ({ facet: {}, subject: Option.none(), tenant: Option.none() }),
    Ceremony: ({ intent, subject }) => ({ facet: { intent }, subject: Option.some(subject), tenant: Option.none() }),
    Clone: ({ subject }) => ({ facet: {}, subject: Option.some(subject), tenant: Option.none() }),
    Deny: ({ action, reason, subject, tenant }) => ({ facet: { action, reason }, subject: Option.some(subject), tenant: Option.map(tenant, String) }),
    Reuse: ({ subject, tenant }) => ({ facet: {}, subject: Option.some(subject), tenant: Option.map(tenant, String) }),
    Rotation: ({ coordinate }) => ({ facet: { coordinate }, subject: Option.none(), tenant: Option.none() }),
    ShredOpen: () => ({ facet: {}, subject: Option.none(), tenant: Option.none() }),
  })

const _egress = (record: AuditRecord): Effect.Effect<AuditTrace, AuditFault, Pseudonym> =>
  Effect.gen(function* () {
    const pseudonym = yield* Pseudonym
    const shed = _shed(record.fact)
    const subject = yield* Option.match(shed.subject, {
      onNone: () => Effect.succeedNone,
      onSome: (raw) => Effect.map(pseudonym.mask(raw), Option.some),
    })
    return new AuditTrace({
      app: record.app,
      at: record.at,
      point: record.point,
      kind: record.fact._tag,
      subject,
      tenant: shed.tenant,
      facet: shed.facet,
    })
  })
```

## [05]-[BOARD]

[BOARD]:
- Owner: `Audit` — the assembled rail owner: the scoped service holds the app-stamped publish fold, the class-routed lanes, the drain fibers, and the polymorphic `subscribe`; the statics carry every projection — `live` (the rail with its `Witness` binding), `egress`, `snapshot`, `objectives`, `alerts`, and `board` — so one import serves arms, subscribers, the support bundle, and the deploy plane. `Snapshot` is the typed support-bundle receipt over `SnapshotRow` rows.
- Law: `subscribe` is one entrypoint over every consumption modality — no selection streams the whole rail, a class literal (`"breached"`) streams a severity band, a point key streams one arm — discriminated on the input value, never a `subscribeByPoint` sibling; zero-tolerance paging is exactly `subscribe("breached")` routed to the app's notifier at the root, so breach alerting reads receipt-truth, not a lossy rate.
- Law: the board and alert owners are composed, never re-minted — `board` calls the core `DashboardModel` security pack, so a panel-set change is a core pack row; `objectives` declares the folder's two latency objectives over the `securityKdf` and `securityJwksResolve` histogram rows with ceilings that land on declared bucket bounds; `alerts` is `Alert.of` folded over them, so burn thresholds, windows, and severities stay the core burn table's and the iac observe leg compiles the same specs the board annotates — a hand-authored security alert rule or a security-local dashboard family is the forked-discipline defect.
- Law: tenant grouping rides governed — board series group by the `$tenant` template variable under the core `Convention.rasm.tenant` key, the one dimension the runtime tenant metric-view row admits beneath its cardinality ceiling, so a per-tenant security view exists with no new series fan.
- Law: `snapshot` is exporter-free — it reads the core `Metric.snapshot` twin (`DashboardModel.snapshot`), keeps exactly the `Convention.instrument.security*` rows, and seals them with the app key and instant into one encodable receipt, so a doctor probe, a test, or a support bundle dumps the folder's live counters with no telemetry backend and no second registry walk.
- Receipt: `Snapshot` — app, instant, and one `SnapshotRow` per live security series with its kind, labels, reading, and the frequency occurrence map when the instrument carries one.
- Growth: a new objective is one `_OBJECTIVES` row over an instrument-qualified Convention metric; a new subscriber posture is a selection value, never a new member.
- Boundary: pack dispatch and panel vocabulary are the core board owner's; burn rows and severity routing are the core slo owner's; rule and dashboard compilation is the iac observe leg's over the encoded specs; the runtime export lane owns the OTLP path the snapshot bypasses.
- Packages: `effect` (`Effect`, `Layer`, `Queue`, `PubSub`, `Stream`, `Number`); `@rasm/ts/core` (`DashboardModel`, `Alert`, `Objective`, `Sli`, `Convention`).

```typescript
class SnapshotRow extends Schema.Class<SnapshotRow>("SnapshotRow")({
  name: Schema.NonEmptyString,
  kind: Schema.Literal("counter", "frequency", "gauge", "histogram", "summary", "unknown"),
  labels: Schema.Record({ key: Schema.String, value: Schema.String }),
  value: Schema.Number,
  count: Schema.optionalWith(Schema.Number, { as: "Option" }),
  occurrences: Schema.optionalWith(Schema.Record({ key: Schema.String, value: Schema.Number }), { as: "Option" }),
}) {}

class Snapshot extends Schema.Class<Snapshot>("Snapshot")({
  app: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  rows: Schema.Array(SnapshotRow),
}) {}

const _instruments: ReadonlyArray<string> = pipe(
  Record.toEntries(Convention.instrument),
  Array.filterMap(([key, row]) => (key.startsWith("security") ? Option.some(row.name) : Option.none())),
)

const _labels = (bag: Convention.Bag): Record.ReadonlyRecord<string, string> =>
  Record.map(bag, (value) => String(value))

const _rowOf = (signal: DashboardModel.Signal): SnapshotRow =>
  Match.valueTags(signal, {
    Counter: ({ labels, name, value }) =>
      new SnapshotRow({ name, kind: "counter", labels: _labels(labels), value: globalThis.Number(value), count: Option.none(), occurrences: Option.none() }),
    Frequency: ({ labels, name, values }) =>
      new SnapshotRow({ name, kind: "frequency", labels: _labels(labels), value: Number.sumAll(values.values()), count: Option.none(), occurrences: Option.some(Record.fromEntries(values)) }),
    Gauge: ({ labels, name, value }) =>
      new SnapshotRow({ name, kind: "gauge", labels: _labels(labels), value: globalThis.Number(value), count: Option.none(), occurrences: Option.none() }),
    Histogram: ({ count, labels, name, sum }) =>
      new SnapshotRow({ name, kind: "histogram", labels: _labels(labels), value: sum, count: Option.some(count), occurrences: Option.none() }),
    Summary: ({ count, labels, name, sum }) =>
      new SnapshotRow({ name, kind: "summary", labels: _labels(labels), value: sum, count: Option.some(count), occurrences: Option.none() }),
    Unknown: ({ labels, name }) =>
      new SnapshotRow({ name, kind: "unknown", labels: _labels(labels), value: 0, count: Option.none(), occurrences: Option.none() }),
  })

const _OBJECTIVES = {
  jwks: new Objective({
    name: "security-jwks",
    sli: Sli.Latency({ ceiling: Duration.millis(1000), metric: Convention.metric.securityJwksResolve, quantile: 0.99 }),
    target: 0.99,
  }),
  kdf: new Objective({
    name: "security-kdf",
    sli: Sli.Latency({ ceiling: Duration.millis(250), metric: Convention.metric.securityKdf, quantile: 0.99 }),
    target: 0.99,
  }),
} as const

class Audit extends Effect.Service<Audit>()("security/access/Audit", {
  scoped: (identity: AppIdentity) =>
    Effect.gen(function* () {
      const journal = yield* AuditJournal
      const fan = yield* PubSub.sliding<AuditRecord>(_LANES.fan)
      const evidence = yield* Queue.bounded<AuditRecord>(_LANES.breached)
      const notice = yield* Queue.sliding<AuditRecord>(_LANES.notice)
      const _drained = (intake: Queue.Dequeue<AuditRecord>): Effect.Effect<void> =>
        Stream.fromQueue(intake).pipe(
          Stream.runForEach((record) =>
            journal.append(record).pipe(
              Effect.retry({ schedule: _APPEND_RETRY, while: (fault) => FaultClass.retryable(fault.class) }),
              Effect.catchAll((fault) => Effect.logError("audit append exhausted", fault)),
            )),
        )
      yield* Effect.forkScoped(_drained(evidence))
      yield* Effect.forkScoped(_drained(notice))
      const publish = (fact: SecurityFact): Effect.Effect<void> =>
        Effect.gen(function* () {
          const record = new AuditRecord({ app: identity.app, at: yield* DateTime.now, point: SecurityFact.pointOf(fact), fact })
          yield* PubSub.publish(fan, record)
          yield* Queue.offer(SecurityFact.classOf(fact) === "breached" ? evidence : notice, record)
        })
      const subscribe = (selection?: SecurityFact.Class | SecurityFact.Point): Stream.Stream<AuditRecord> =>
        Stream.fromPubSub(fan).pipe(
          Stream.filter((record) =>
            selection === undefined
              ? true
              : selection === "breached" || selection === "notice"
                ? SecurityFact.classOf(record.fact) === selection
                : record.point === selection),
        )
      return { publish, subscribe } as const
    }),
  accessors: true,
}) {
  static readonly alerts: ReadonlyArray<Alert.Spec> = Array.flatMap(Record.values(_OBJECTIVES), Alert.of)
  static readonly board = (identity: AppIdentity): DashboardModel => DashboardModel.pack("security", identity, {})
  static readonly egress = (record: AuditRecord): Effect.Effect<AuditTrace, AuditFault, Pseudonym> => _egress(record)
  static readonly live = (identity: AppIdentity): Layer.Layer<Audit | Witness, never, AuditJournal> =>
    Layer.provideMerge(
      Layer.effect(Witness, Effect.map(Audit, (audit) => ({ publish: audit.publish }))),
      Audit.Default(identity),
    )
  static readonly objectives: ReadonlyArray<Objective> = Record.values(_OBJECTIVES)
  static readonly snapshot = (identity: AppIdentity): Effect.Effect<Snapshot> =>
    Effect.flatMap(DateTime.now, (at) =>
      Effect.map(DashboardModel.snapshot, (signals) =>
        new Snapshot({
          app: identity.app,
          at,
          rows: Array.filterMap(signals, (signal) =>
            Array.contains(_instruments, signal.name) ? Option.some(_rowOf(signal)) : Option.none()),
        })))
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Audit, AuditFault, AuditJournal, AuditRecord, AuditTrace, Pseudonym, SecurityFact, Snapshot, SnapshotRow, Witness }
```
