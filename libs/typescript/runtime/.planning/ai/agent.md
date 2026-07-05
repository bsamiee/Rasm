# [RUNTIME_AGENT]

The agent altitude, ruled and sealed: an agent session's interaction state is a `Transition` machine from the core state page — a closed phase spine driven by a transition table, booted as the in-process serializable actor with snapshot-grade durability — and its conversational memory is the persisted `Chat` substrate the intelligence package ships, so neither state nor history is hand-assembled anywhere. The turn is one fold: recall the session, weave app-passed retrieval into the measured prompt, screen once at the gate, iterate gated tool-calling generation under a bounded step budget, compact when the meter demands, persist. The `Act`/`Turn`/`AgentFault` schema triple is declared once as tagged requests and serves three surfaces without re-declaration — the cluster entity's message protocol (a durable multi-process session is `entity#ACTOR_MINT` applied to the same triple, single-writer turn order arriving as an entity fact), the agents-as-tools row (`Tool.fromTaggedRequest` lifts `Act` so an agent is callable by another agent's toolkit), and the serving plane's proxied drive. Held-tool approval is evidence, not ceremony: the gate's held partition emits unresolved calls, the machine holds the `awaiting` phase, and release arrives as a signal in-process or a durable token for approvals that outlive the process. The module is `runtime/src/ai/agent.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]   | [OWNS]                                                                            | [PUBLIC]   |
| :-----: | :---------- | :------------------------------------------------------------------------------------ | :--------- |
|  [01]   | `SESSION`   | persisted chat memory, session identity, the two compaction lanes                       | `Session`  |
|  [02]   | `TURN`      | the one turn fold and the `Act`/`Turn`/`AgentFault` request triple                      | `Agent`    |
|  [03]   | `ACTOR`     | the phase machine, the in-process boot, the entity escalation row                       | `Agent`    |
|  [04]   | `APPROVAL`  | the held-call evidence fold and the two release paths                                   | `Agent`    |

## [2]-[SESSION]

[SESSION]:
- Owner: `Session` — durable conversational memory on the shipped substrate: `Chat.layerPersisted` provides the persistence Tag over `@effect/experimental` `Persistence.BackingPersistence` (satisfied at the app root from a data-wave key-value scope), `Session.open(key)` restores-or-creates the chat whose `history` is the substrate's own `Ref<Prompt>`, and the snapshot rides the substrate's own twins — `export`/`fromExport` for structured hops, `exportJson`/`fromJson` for the string form the KV and wire persistence paths store — a hand-assembled history record beside `Chat` is the killed lane, and no key-value session schema exists on this page.
- Law: the session key is branded and tenant-scoped — one session per `(tenant, conversation)` identity, the same key the entity id carries when the session escalates to the durable row, so in-process and sharded sessions share identity by construction.
- Law: compaction is two lanes under one trigger — when the gauged history exceeds its budget share, `trim` truncates through the model page's fit enforcement (the tokenizer owns the cut), and `digest` folds the trimmed prefix into one summary block through a gated call with `Toolkit.empty` (no tools reachable from a summarization) prepended as system context; the lane is a policy value on the session row.
- Law: retirement is a lifecycle fact — a session past its idle window exports, records its final digest as evidence, and releases; an unbounded session set is the named leak.
- Growth: a memory concern (pinned facts, user preferences) is a system block the digest lane preserves, never a second store.
- Packages: `@effect/ai` (`Chat`); `@effect/experimental` (`Persistence`); `effect` (`Layer`, `Effect`, `Schema`); `./model.ts` (`Tokens`, `Guardrail`).

```typescript
import { Chat, Prompt, Tool, Toolkit } from "@effect/ai"
import { Array, Effect, Match, Ref, Schedule, Schema } from "effect"
import { type FaultClass, Transition } from "@rasm/ts/core"
import { Guardrail, Tokens } from "./model.ts"
import type { Safety } from "./tool.ts"

const SessionKey = Schema.NonEmptyString.pipe(Schema.brand("SessionKey"))

declare namespace Session {
  type Key = typeof SessionKey.Type
  type Row = {
    readonly key: Key
    readonly budget: Tokens.Pair
    readonly compaction: "trim" | "digest"
    readonly mode: Safety.Mode
  }
}

const _open = (row: Session.Row) =>
  Effect.gen(function* () {
    const chat = yield* Chat.empty
    const compact = (policy: Parameters<typeof Guardrail.text>[0]) =>
      Effect.gen(function* () {
        const history = yield* Ref.get(chat.history)
        yield* Match.value(row.compaction).pipe(
          Match.when("trim", () => Tokens.fit(history, row.budget).pipe(Effect.flatMap((fitted) => Ref.set(chat.history, fitted)))),
          Match.when("digest", () =>
            Guardrail.text(policy)({ prompt: history, toolkit: Toolkit.empty }).pipe(
              Effect.flatMap((summary) => Ref.set(chat.history, Prompt.make(`memory: ${summary.text}`))),
            )),
          Match.exhaustive,
        )
      })
    return { chat, compact } as const
  })

const Session = { key: SessionKey, open: _open, persisted: Chat.layerPersisted }
```

## [3]-[TURN]

[TURN]:
- Owner: `Agent` and the request triple. `Act` is the inbound tagged request — session key, utterance, app-passed retrieval passages, the safety mode — with `Turn` as its success (the reply text, the held-call evidence band, the spend receipt, the settled phase) and `AgentFault` as its failure (reason-discriminated, class-carrying); `Schema.TaggedRequest` declares payload, success, and failure in one class, and that single declaration is the entity Rpc, the `Tool.fromTaggedRequest` row, and the wire contract.
- Law: the turn is one fold, budget-bounded — recall (`Session.open`), weave (`Tokens.weave` over the passages — retrieval arrives as values, the data wave is never imported), screen once (the gate screens the woven prompt, not each iteration), then the tool-loop: at most `steps` gated generations where each iteration's `toolChoice` and `disableToolCallResolution` compile from the safety partition, resolved tool results append through the chat so the model sees its own evidence, and the loop exits on a toolless reply, a held call, or the step ceiling — the ceiling folding to an `exhausted`-classed fault, never a silent truncation.
- Law: every turn settles with evidence — spend from the gate's receipt, phase from the machine, held calls as data, and provenance as data: the response's `DocumentSourcePart`/`UrlSourcePart` citation parts project into the `Turn.sources` band, so a grounded reply carries its own citations; a turn's receipt is what supervision, billing, and the approval surface read, so the loop returns `Turn`, never bare text.
- Law: the tool loop reconstructs prompts through the package — `Prompt.fromResponseParts` rebuilds the next iteration's prompt from the prior response's parts, so multi-turn tool evidence is the package's own reconstruction, never a hand-spliced history; replay parity rides a deterministic `IdGenerator` Layer at the durable root so re-driven turns mint identical tool-call ids.
- Growth: a loop concern (reflection pass, plan-then-act) is a phase row plus a fold arm, never a second loop.
- Packages: `@effect/ai` (`Prompt`, `Tool`, `Toolkit`); `effect` (`Effect`, `Schema`); `./model.ts` (`Guardrail`, `Tokens`); `./tool.ts` (`Safety`).

```typescript
class AgentFault extends Schema.TaggedError<AgentFault>()("AgentFault", {
  reason: Schema.Literal("budget", "refused", "tool", "session"),
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return this.reason === "budget" ? "exhausted" : this.reason === "refused" ? "denied" : "unavailable"
  }
}

class Turn extends Schema.Class<Turn>("Turn")({
  reply: Schema.String,
  held: Schema.Array(Schema.Struct({ tool: Schema.String, params: Schema.String })),
  sources: Schema.Array(Schema.Struct({ kind: Schema.Literal("document", "url"), ref: Schema.String })),
  spend: Schema.BigDecimal,
  phase: Schema.Literal("idle", "thinking", "awaiting", "compacting"),
}) {}

class Act extends Schema.TaggedRequest<Act>()("Act", {
  failure: AgentFault,
  success: Turn,
  payload: {
    session: SessionKey,
    utterance: Schema.NonEmptyString,
    passages: Schema.Array(Schema.Struct({ origin: Schema.String, rank: Schema.Int, body: Schema.String })),
    mode: Schema.Literal("autonomous", "supervised", "locked"),
  },
}) {}

const _asTool = Tool.fromTaggedRequest(Act)
```

## [4]-[ACTOR]

[ACTOR]:
- Owner: the phase machine — one `Transition.spec`: a depth-one statechart (`session` compound over four atomic phases), signals `act | settle | hold | release | compact | done`, verdict programs naming what the driver does next; the rows are the whole interaction protocol — an unmatched signal is an empty program, never a hand branch — the `awaiting` node's watch row arms the approval deadline as a delayed self-signal, and `recover` re-initializes a defecting actor under a `pulse`-budget schedule. The compiled spec's `boot` runs the machine scoped beside the session so phase and history live and die together; `freeze`/`restore` carry an interactive session across a page or process hop.
- Law: the altitude ruling is enforced by construction — the in-process actor serves the interactive lane (live phase, request-serialized turns, snapshot durability); the durable multi-process lane is `Actor.make({ name: "agent", protocol, clazz: "interactive", tenant })` over the SAME `Act` protocol, where per-session single-writer ordering, mailbox fencing, and message durability are entity facts — the machine table travels unchanged, and no third session runtime exists between them.
- Law: the turn drives the machine, never the reverse — `act` enters `thinking`, a toolless settle emits `settle` back to `idle`, a held call emits `hold` into `awaiting`, the compaction trigger emits `compact`; a phase mutated outside a signal is unspellable because the table is the only transition author.
- Growth: a new interaction posture (streaming turn, background reflection) is a node row plus its transition rows; the entity escalation inherits it by sharing the spec.
- Packages: `@rasm/ts/core` (`Transition`); `../work/entity.ts` (`Actor` — the escalation mint); `effect` (`Schedule`).

```typescript
const _spec = Transition.spec({
  name: "agent",
  nodes: {
    session: { kind: "compound", initial: "idle" },
    idle: { kind: "atomic", parent: "session" },
    thinking: { kind: "atomic", parent: "session" },
    awaiting: { kind: "atomic", parent: "session", watch: { after: "15 minutes", signal: "done" } },
    compacting: { kind: "atomic", parent: "session" },
  },
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
  traced: true,
  recover: Schedule.exponential("250 millis"),
})

const _boot = _spec.boot
```

## [5]-[APPROVAL]

[APPROVAL]:
- Owner: the held-call fold — when the gate's admission returns held names, the generation runs with tool resolution disabled, the emitted tool-call parts persist on the `Turn.held` band as `{ tool, params }` evidence, and the machine enters `awaiting`. Release is two paths by longevity: the in-process path feeds `release` to the machine when a supervisor approves within the session's live window (the approved calls re-enter the loop as ordinary resolved results), and the durable path — an approval that outlives the process — declares a `flow#SIGNAL_GATE` deferred whose token travels to the approval surface and whose settlement re-drives the entity turn; the `awaiting` watch row expires unanswered holds into `done`, so an abandoned approval degrades to a bounded, evidenced no-op.
- Law: a held call never executes speculatively — the handler runs only after release, with the released parameters byte-equal to the held evidence; an "execute then ask" ordering is unspellable because resolution was disabled at the gate.
- Law: approval is an audited action — release and expiry each append a fact row (who, which tool, which session) through the data wave's fact rail at the approving surface; this page holds evidence and phases, the serving plane owns the approval endpoint.
- Growth: an approval policy axis (auto-release below a spend ceiling, four-eyes for `destroy`) is a predicate over the held band composed at release, never a second hold mechanism.
- Packages: `./model.ts` (`Guardrail`); `../work/flow.ts` (`Signal` — the durable deferred); `@rasm/ts/core` (`Transition`).

```typescript
const _held = (turn: Turn) => turn.held.length > 0

const _release = (
  actor: Transition.Actor<
    "session" | "idle" | "thinking" | "awaiting" | "compacting",
    "act" | "settle" | "hold" | "release" | "compact" | "done",
    "generate" | "reply" | "escalate" | "resume" | "fold",
    null
  >,
  approved: ReadonlyArray<{ readonly tool: string; readonly params: string }>,
  held: ReadonlyArray<{ readonly tool: string; readonly params: string }>,
) =>
  Array.every(approved, (call) => Array.some(held, (kept) => kept.tool === call.tool && kept.params === call.params))
    ? actor.feed("release")
    : Effect.fail(new AgentFault({ reason: "tool", detail: "release differs from held evidence" }))

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
