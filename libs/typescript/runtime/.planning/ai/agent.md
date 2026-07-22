# [RUNTIME_AGENT]

The agent altitude, ruled and sealed: an agent session's interaction state is a `Transition` machine from the core state page — a closed phase spine driven by a transition table, booted as the in-process serializable actor with snapshot-grade durability — and its conversational memory is the persisted `Chat` substrate the intelligence package ships, so neither state nor history is hand-assembled anywhere. The turn is one fold: recall the session through the persistence Tag's restore-or-create, weave app-passed retrieval into the measured prompt, screen once at the gate, iterate gated tool-calling generation under a bounded step budget, compact when the meter demands, persist. The `Act`/`Turn`/`AgentFault` schema triple is declared once as tagged requests and serves three surfaces without re-declaration — the cluster entity's message protocol (a durable multi-process session is `entity#ACTOR_MINT` applied to the same triple, single-writer turn order arriving as an entity fact), the agents-as-tools row (`Tool.fromTaggedRequest` lifts `Act` so an agent is callable by another agent's toolkit), and the serving plane's proxied drive. Held-tool approval is evidence, not ceremony: the gate's held partition emits unresolved calls, the emitted tool-call parts persist inside the chat history itself — the substrate is the evidence store — the machine holds the `awaiting` phase, and release demands a complete id-correlated, structurally equal disposition of every held call before the actor resumes. The phase vocabulary has ONE anchor: the `_PHASES` tuple feeds the `Turn.phase` literal and closes the machine's node roster through its guard, so the receipt and the statechart cannot drift. The module is `runtime/src/ai/agent.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]  | [OWNS]                                                                | [PUBLIC]  |
| :-----: | :--------- | :-------------------------------------------------------------------- | :-------- |
|  [01]   | `SESSION`  | persisted chat memory, the session row, the two compaction lanes      | `Session` |
|  [02]   | `TURN`     | the phase anchor, the turn fold, the `Act`/`Turn`/`AgentFault` triple | `Agent`   |
|  [03]   | `ACTOR`    | the phase machine, the in-process boot, the entity escalation row     | `Agent`   |
|  [04]   | `APPROVAL` | the held-call evidence fold and the two release paths                 | `Agent`   |

## [02]-[SESSION]

[SESSION]:
- Owner: `Session`, one `Schema.Class` — the session row is itself an admitted value: `key` (branded, tenant-scoped — one session per `(tenant, conversation)` identity, the same key the entity id carries when the session escalates to the durable row, so in-process and sharded sessions share identity by construction), `budget` (`window`, `reply`, and `steps`, with a reply reserve strictly below the window), `compaction` (the lane literal), `mode` (the safety mode — its guard alias proves the field covers `Safety.Mode` so a new mode breaks this row at compile time), and `idle` (the retirement window, threaded as the persisted chat's `timeToLive`). `Session.open(row)` yields the persisted chat through the substrate's own restore-or-create — `Chat.Persistence` then `getOrCreate(row.key, { timeToLive: row.idle })` — so a reopened session recovers its history after process loss and a hand-assembled history record beside `Chat` is the killed lane; `Session.persisted` re-exports `Chat.layerPersisted({ storeId })`, the Layer the app root backs with a data-wave key-value scope through `Persistence.BackingPersistence`.
- Law: the persisted chat IS the durable surface — `getOrCreate` answers a `Persisted` chat carrying `id` and `save`, every generation through it appends both turns to the backing store, and the snapshot rides the substrate's own twins (`export`/`exportJson`) only at the wire hop; a persistence failure folds to the `session`-reasoned `AgentFault`, never a raw backing error.
- Law: compaction is two lanes under one trigger — when the gauged history exceeds its budget share, `trim` truncates through the model page's fit enforcement (the tokenizer owns the cut), and `digest` folds the trimmed prefix into one summary block through a gated call with `Toolkit.empty` (no tools reachable from a summarization) re-seeding the history; the lane is a policy value on the session row.
- Law: retirement is a lifecycle fact — the `idle` window is the persisted chat's `timeToLive`, so an untouched session ages out of the backing store by declaration; the final digest records as evidence before release, and an unbounded session set is the named leak.
- Growth: a memory concern (pinned facts, user preferences) is a system block the digest lane preserves, never a second store.
- Packages: `@effect/ai` (`Chat`); `@effect/experimental` (`Persistence` — the backing requirement behind `Chat.layerPersisted`); `effect` (`Effect`, `Ref`, `Match`, `Schema`); `./model.ts` (`Tokens`, `Guardrail`).

```typescript
import { Chat, Prompt, Tool, Toolkit } from "@effect/ai"
import { Array, Effect, HashSet, Match, Ref, Schedule, Schema } from "effect"
import { type FaultClass, Transition } from "@rasm/ts/core"
import { Guardrail, Tokens } from "./model.ts"
import type { Safety } from "./tool.ts"

class Session extends Schema.Class<Session>("Session")({
  key: Schema.NonEmptyString.pipe(Schema.brand("SessionKey")),
  budget: Schema.Struct({
    window: Schema.Int.pipe(Schema.positive()),
    reply: Schema.Int.pipe(Schema.positive()),
    steps: Schema.Int.pipe(Schema.positive()),
  }).pipe(Schema.filter((budget) => budget.reply < budget.window, { identifier: "ReplyWithinWindow" })),
  compaction: Schema.Literal("trim", "digest"),
  mode: Schema.Literal("autonomous", "supervised", "locked"),
  idle: Schema.Duration,
}) {
  static readonly open = (row: Session) => _open(row) // the restore-or-create recall; the persisted chat is the one durable surface
  static readonly persisted = Chat.layerPersisted
}

declare namespace Session {
  type Key = Session["key"]
  type _Modes<K extends Session["mode"] = Safety.Mode> = K // guard: every Safety.Mode row is representable on the session row
}

const _open = (row: Session) =>
  Effect.gen(function* () {
    const persistence = yield* Chat.Persistence
    const chat = yield* persistence.getOrCreate(row.key, { timeToLive: row.idle }).pipe(
      Effect.mapError((fault) => new AgentFault({ reason: "session", detail: fault._tag })),
    )
    const compact = (policy: Guardrail.Policy) =>
      Effect.gen(function* () {
        const history = yield* Ref.get(chat.history)
        yield* Match.value(row.compaction).pipe(
          Match.when("trim", () => Tokens.fit(history, row.budget).pipe(Effect.flatMap((fitted) => Ref.set(chat.history, fitted)))),
          Match.when("digest", () =>
            Guardrail.generate(policy, { _tag: "Text", options: { prompt: history, toolkit: Toolkit.empty } }).pipe(
              Effect.flatMap((summary) => Ref.set(chat.history, Prompt.make(`memory: ${summary.text}`))),
            )),
          Match.exhaustive,
        )
        yield* Effect.mapError(chat.save, (fault) => new AgentFault({ reason: "session", detail: fault._tag }))
      })
    return { chat, compact } as const
  })
```

## [03]-[TURN]

[TURN]:
- Owner: `Agent` and the request triple. `_PHASES` is the one phase anchor — the `Turn.phase` literal spreads it and the machine's node guard closes against it, so the receipt vocabulary and the statechart roster are one declaration. `Act` is the inbound tagged request — the admitted `Session` carrier, utterance, and app-passed retrieval passages — with `Turn` as its success (the reply text, held-call evidence, citation band, spend receipt, settled phase) and `AgentFault` as its failure. The carrier owns identity, budget, compaction, safety mode, and lifetime together; repeating `session.key` and `mode` on the request creates independent knobs with contradictory turn values. `Schema.TaggedRequest` declares payload, success, and failure in one class, and that single declaration is the entity Rpc, the `Tool.fromTaggedRequest` row, and the wire contract.
- Law: the turn is one fold, budget-bounded — recall (`Session.open`), weave (`Tokens.weave` over the passages — retrieval arrives as values, the data wave is never imported), screen once (the gate screens the woven prompt, not each iteration), then the tool-loop: at most `steps` gated generations where each iteration's `toolChoice` and `disableToolCallResolution` compile from the safety partition, resolved tool results append through the chat so the model sees its own evidence, and the loop exits on a toolless reply, a held call, or the step ceiling — the ceiling folding to an `exhausted`-classed fault, never a silent truncation.
- Law: every turn settles with evidence — spend from the gate's receipt, phase from the machine, held calls as data (each provider-minted `{ id, tool, params }`, preserving the decoded parameter value for structural comparison), and provenance as data: the response's `DocumentSourcePart`/`UrlSourcePart` citation parts project into the `Turn.sources` band, so a grounded reply carries its own citations; a turn's receipt is what supervision, billing, and the approval surface read, so the loop returns `Turn`, never bare text.
- Law: the tool loop reconstructs prompts through the package — `Prompt.fromResponseParts` rebuilds the next iteration's prompt from the prior response's parts, so multi-turn tool evidence is the package's own reconstruction, never a hand-spliced history; replay parity rides a deterministic `IdGenerator` Layer at the durable root so re-driven turns mint identical tool-call ids.
- Growth: a loop concern (reflection pass, plan-then-act) is a phase row plus a fold arm, never a second loop.
- Packages: `@effect/ai` (`Prompt`, `Tool`, `Toolkit`); `effect` (`Effect`, `Schema`); `./model.ts` (`Guardrail`, `Tokens`); `./tool.ts` (`Safety`).

```typescript
const _PHASES = ["idle", "thinking", "awaiting", "compacting"] as const

class AgentFault extends Schema.TaggedError<AgentFault>()("AgentFault", {
  reason: Schema.Literal("budget", "refused", "tool", "session"),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return this.reason === "budget" ? "exhausted" : this.reason === "refused" ? "denied" : "unavailable"
  }
}

declare namespace Agent {
  type Json = null | boolean | number | string | ReadonlyArray<Json> | { readonly [key: string]: Json }
}

const _Json: Schema.Schema<Agent.Json> = Schema.suspend(() =>
  Schema.Union(
    Schema.Null,
    Schema.Boolean,
    Schema.Number,
    Schema.String,
    Schema.Array(_Json),
    Schema.Record({ key: Schema.String, value: _Json }),
  ),
)
const _sameParams = Schema.equivalence(_Json)
const _Held = Schema.Struct({ id: Schema.NonEmptyString, tool: Schema.NonEmptyString, params: _Json })

class Turn extends Schema.Class<Turn>("Turn")({
  reply: Schema.String,
  held: Schema.Array(_Held),
  sources: Schema.Array(Schema.Struct({ kind: Schema.Literal("document", "url"), ref: Schema.String })),
  spend: Schema.BigDecimal,
  phase: Schema.Literal(..._PHASES),
}) {}

class Act extends Schema.TaggedRequest<Act>()("Act", {
  failure: AgentFault,
  success: Turn,
  payload: {
    session: Session,
    utterance: Schema.NonEmptyString,
    passages: Schema.Array(Schema.Struct({ origin: Schema.String, rank: Schema.Int, body: Schema.String })),
  },
}) {}

const _asTool = Tool.fromTaggedRequest(Act)
```

## [04]-[ACTOR]

[ACTOR]:
- Owner: the phase machine — one `Transition.spec`: a depth-one statechart (`session` compound over the four `_PHASES` atomics, the node roster closed against the anchor by the `_Nodes` guard), signals `act | settle | hold | release | compact | done`, verdict programs naming what the driver does next; the rows are the whole interaction protocol — an unmatched signal is an empty program, never a hand branch — the `awaiting` node's watch row arms the approval deadline as a delayed self-signal, and `recover` re-initializes a defecting actor under a `pulse`-budget schedule. The compiled spec's `boot` runs the machine scoped beside the session so phase and history live and die together; `freeze`/`restore` carry an interactive session across a page or process hop.
- Law: the signal plane is literal-only by the core machine's own law, so held-call evidence never rides a signal and the machine's `extended` stays `Schema.Null` — the durable evidence home is the persisted chat itself: the emitted tool-call parts persist with the assistant message, and the release fold reads its held band from the `Turn` receipt whose source is that history; a second evidence store beside the substrate is the named split.
- Law: the altitude ruling is enforced by construction — the in-process actor serves the interactive lane (live phase, request-serialized turns, snapshot durability); the durable multi-process lane is `Actor.make({ name: "agent", protocol, clazz: "interactive", tenant })` over the SAME `Act` protocol, where per-session single-writer ordering, mailbox fencing, and message durability are entity facts — the machine table travels unchanged, and no third session runtime exists between them.
- Law: the turn drives the machine, never the reverse — `act` enters `thinking`, a toolless settle emits `settle` back to `idle`, a held call emits `hold` into `awaiting`, the compaction trigger emits `compact`; a phase mutated outside a signal is unspellable because the table is the only transition author.
- Growth: a new interaction posture (streaming turn, background reflection) is a node row plus its transition rows; the entity escalation inherits it by sharing the spec.
- Packages: `@rasm/ts/core` (`Transition`); `../work/entity.ts` (`Actor` — the escalation mint); `effect` (`Schedule`).

```typescript
const _nodes = {
  session: { kind: "compound", initial: "idle" },
  idle: { kind: "atomic", parent: "session" },
  thinking: { kind: "atomic", parent: "session" },
  awaiting: { kind: "atomic", parent: "session", watch: { after: "15 minutes", signal: "done" } },
  compacting: { kind: "atomic", parent: "session" },
} as const

declare namespace _nodes {
  type _Nodes<K extends (typeof _PHASES)[number] = Exclude<keyof typeof _nodes, "session">> = K // guard: the node roster IS the phase anchor plus the compound root
}

const _spec = Transition.spec({
  name: "agent",
  nodes: _nodes,
  rows: [
    { source: "idle", on: "act", to: ["thinking"], emit: ["generate"] },
    { source: "idle", on: "compact", to: ["compacting"], emit: ["fold"] },
    { source: "thinking", on: "settle", to: ["idle"], emit: ["reply"] },
    { source: "thinking", on: "hold", to: ["awaiting"], emit: ["escalate"] },
    { source: "thinking", on: "compact", to: ["compacting"], emit: ["fold"] },
    { source: "thinking", on: "done", to: ["idle"], emit: ["reply"] },
    { source: "awaiting", on: "release", to: ["thinking"], emit: ["resume"] },
    { source: "awaiting", on: "done", to: ["idle"] },
    { source: "compacting", on: "done", to: ["idle"] },
  ],
  signal: Schema.Literal("act", "settle", "hold", "release", "compact", "done"),
  verdict: Schema.Literal("generate", "reply", "escalate", "resume", "fold"),
  extended: Schema.Null,
  seed: null,
  fuel: 4,
  lag: 32,
  traced: true,
  recover: Schedule.exponential("250 millis"),
})

const _boot = _spec.boot
```

## [05]-[APPROVAL]

[APPROVAL]:
- Owner: the held-call fold — when the gate's admission returns held names, the generation runs with tool resolution disabled, the emitted tool-call parts persist on the `Turn.held` band as `{ id, tool, params }` evidence (and inside the chat history, the durable copy), and the machine enters `awaiting`. Release is a COMPLETE disposition: `Agent.release(spec)` demands exactly one id-correlated verdict per held call, rejects duplicate ids and structural parameter drift, refuses an empty held band, settles every verdict through the supplied tool-result continuation in source order, and feeds `release` only after every settlement succeeds; the returned `Release` receipt carries both partitions as the audit evidence. The two release paths split by longevity: the in-process path supplies the live continuation, and the durable path — an approval that outlives the process — declares a `flow#SIGNAL_GATE` deferred whose token travels to the approval surface and whose settlement re-drives the entity turn with the same fold; the `awaiting` watch row expires unanswered holds into `done`, so an abandoned approval degrades to a bounded, evidenced no-op.
- Law: a held call never executes speculatively — the tool continuation receives an approval value only after the complete disposition validates, with parameters structurally equal to the held evidence; an "execute then ask" ordering is unspellable because resolution was disabled at the gate, and a partial, duplicate, or superset disposition fails before any continuation or signal fires.
- Law: approval is an audited action — release and expiry each append a fact row (who, which tool, which session, which verdict) through the data wave's fact rail at the approving surface; this page holds evidence and phases, the serving plane owns the approval endpoint.
- Growth: an approval policy axis (auto-release below a spend ceiling, four-eyes for `destroy`) is a predicate over the held band composed at release, never a second hold mechanism.
- Packages: `./model.ts` (`Guardrail`); `../work/flow.ts` (`Signal` — the durable deferred); `@rasm/ts/core` (`Transition`).

```typescript
declare namespace Agent {
  type Held = typeof _Held.Type
  type Verdict = Held & { readonly approve: boolean }
  type Release = {
    readonly approved: ReadonlyArray<Held>
    readonly declined: ReadonlyArray<Held>
  }
  type Actor = Effect.Effect.Success<typeof _boot>
  type ReleaseSpec<R> = {
    readonly actor: Actor
    readonly held: ReadonlyArray<Held>
    readonly verdicts: ReadonlyArray<Verdict>
    readonly settle: (held: Held, approved: boolean) => Effect.Effect<void, AgentFault, R>
  }
}

const _matched = (held: ReadonlyArray<Agent.Held>, verdict: Agent.Verdict): boolean =>
  Array.some(held, (kept) => kept.id === verdict.id && kept.tool === verdict.tool && _sameParams(kept.params, verdict.params))

const _release = <R>(spec: Agent.ReleaseSpec<R>): Effect.Effect<Agent.Release, AgentFault, R> => {
  const unique = (rows: ReadonlyArray<{ readonly id: string }>): boolean =>
    HashSet.size(HashSet.fromIterable(Array.map(rows, (row) => row.id))) === rows.length
  const valid = Array.isNonEmptyReadonlyArray(spec.held) &&
    spec.verdicts.length === spec.held.length &&
    unique(spec.held) &&
    unique(spec.verdicts) &&
    Array.every(spec.verdicts, (verdict) => _matched(spec.held, verdict))
  const receipt: Agent.Release = {
    approved: Array.map(Array.filter(spec.verdicts, (verdict) => verdict.approve), ({ id, tool, params }) => ({ id, tool, params })),
    declined: Array.map(Array.filter(spec.verdicts, (verdict) => !verdict.approve), ({ id, tool, params }) => ({ id, tool, params })),
  }
  return valid
    ? Effect.forEach(
        spec.verdicts,
        ({ approve, ...held }) => spec.settle(held, approve),
        { concurrency: 1, discard: true },
      ).pipe(
        Effect.zipRight(spec.actor.feed("release")),
        Effect.mapError((fault) => fault instanceof AgentFault ? fault : new AgentFault({ reason: "budget", detail: fault._tag })),
        Effect.as(receipt),
      )
    : Effect.fail(new AgentFault({ reason: "tool", detail: "disposition incomplete or differs from held evidence" }))
}

const _held = (turn: Turn) => turn.held.length > 0

const Agent = {
  tool: _asTool,
  spec: _spec,
  boot: _boot,
  release: _release,
  held: _held,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Act, Agent, AgentFault, Session, Turn }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
