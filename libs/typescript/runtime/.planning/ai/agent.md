# [RUNTIME_AGENT]

The agent altitude, ruled and sealed: an agent session's interaction state is a `Transition` machine from the core state page — a closed phase spine driven by a transition table, booted as the in-process serializable actor with snapshot-grade durability — and its conversational memory is the persisted `Chat` substrate the intelligence package ships, handed to the model page's gate AS its carrier, so neither state nor history is hand-assembled anywhere. The turn is one fold: recall the session through the persistence Tag's restore-or-create, compact when the meter demands, weave app-passed retrieval into the measured prompt, screen once at the gate, then iterate chat-carried gated generation under a bounded step budget until a toolless reply, a held call, or the ceiling settles it — persistence needs no closing write because a persisted chat stores prompt and response on every generation. The `Act`/`Turn`/`AgentFault` schema triple is declared once as tagged requests and serves three surfaces without re-declaration — the cluster entity's message protocol (a durable multi-process session is `work/entity#ACTOR_MINT` applied to the same triple, single-writer turn order arriving as an entity fact), the agents-as-tools row (`Tool.fromTaggedRequest` lifts `Act` so an agent is callable by another agent's toolkit), and the serving plane's proxied drive. Held-tool approval is evidence, not ceremony: the gate's held partition emits unresolved calls, the emitted tool-call parts persist inside the chat history itself — the substrate is the evidence store — the machine holds the `awaiting` phase, and release demands a complete id-correlated, structurally equal disposition of every held call before the actor resumes. The phase vocabulary has ONE anchor: the `_PHASES` tuple feeds the `Turn.phase` literal and closes the machine's node roster through its guard, so the receipt and the statechart cannot drift. The module is `runtime/src/ai/agent.ts`.

## [01]-[INDEX]

- [02]-[SESSION]: persisted chat memory, the session row, the two compaction lanes; `Session`.
- [03]-[TURN]: the phase anchor, the chat-carried turn fold, the `Act`/`Turn`/`AgentFault` triple; `Agent`.
- [04]-[ACTOR]: the phase machine, the in-process boot, the entity escalation row; `Agent`.
- [05]-[APPROVAL]: the held-call evidence fold and the two release paths; `Agent`.

## [02]-[SESSION]

[SESSION]:
- Owner: `Session`, one `Schema.Class` — the session row is itself an admitted value: `key` (branded, tenant-scoped — one session per `(tenant, conversation)` identity, the same key the entity id carries when the session escalates to the durable row, so in-process and sharded sessions share identity by construction), `budget` (`window`, `reply`, and `steps`, with a reply reserve strictly below the window), `compaction` (the lane literal), `mode` (the safety mode — its guard alias proves the field covers `Safety.Mode` so a new mode breaks this row at compile time), and `idle` (the retirement window, threaded as the persisted chat's `timeToLive`). `Session.open(row)` yields the persisted chat through the substrate's own restore-or-create — `Chat.Persistence` then `getOrCreate(row.key, { timeToLive: row.idle })` — so a reopened session recovers its history after process loss and a hand-assembled history record beside `Chat` is the killed lane; `Session.persisted` re-exports `Chat.layerPersisted({ storeId })`, the Layer the app root backs with a data-wave key-value scope through `Persistence.BackingPersistence`.
- Law: the persisted chat IS the durable surface, and the gate's carrier parameter is what makes it so — `getOrCreate` answers a `Persisted` chat carrying `id` and `save`, and handing that chat to `Guardrail.generate` as the carrier appends both turns AND writes them to the backing store on every generation, so a turn needs no closing save and `save` is reserved for the one place a lane writes history by hand. The snapshot rides the substrate's own twins (`export`/`exportJson`) only at the wire hop; a persistence failure folds to the `session`-reasoned `AgentFault`, never a raw backing error.
- Law: compaction is two lanes and ONE write — both lanes are pure prompt producers under one `Match` (`trim` truncates through the model page's fit enforcement, the tokenizer owning the cut; `digest` folds the history into one summary block), a single `Ref.set` seeds the result, and one `save` persists it; a per-lane write forks the seam that has to stay singular for the save to mean anything. The lane is a policy value on the session row.
- Law: the digest generation runs on the gate's DEFAULT free carrier, never on the chat — a summarization routed through the chat would append itself to the very history it compacts — and it runs with `Toolkit.empty` because no tool is reachable from a summarization.
- Law: retirement is a lifecycle fact — the `idle` window is the persisted chat's `timeToLive`, so an untouched session ages out of the backing store by declaration; the final digest records as evidence before release, and an unbounded session set is the named leak.
- Growth: a memory concern (pinned facts, user preferences) is a system block the digest lane preserves, never a second store.
- Packages: `@effect/ai` (`Chat`, `AiError`, `Prompt`, `Toolkit`); `@effect/experimental` (`Persistence` — the backing requirement behind `Chat.layerPersisted`); `effect` (`Effect`, `Ref`, `Match`, `Schema`); `./model.ts` (`Tokens`, `Guardrail`, `GuardrailFault`).

```typescript
import { type AiError, Chat, type LanguageModel, Prompt, type Response, Tool, Toolkit } from "@effect/ai"
import { Array, BigDecimal, Effect, Either, HashSet, Match, Option, Ref, Schema } from "effect"
import { Budget, FaultClass, type Spent, Transition } from "@rasm/ts/core"
import { Guardrail, GuardrailFault, type Ladder, Spend, Tokens } from "./model.ts"
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

const _backed = (fault: { readonly _tag: string }): AgentFault => new AgentFault({ reason: "session", detail: fault._tag })

const _folded = (fault: GuardrailFault | AiError.AiError): AgentFault =>
  Match.value(fault).pipe(
    Match.tag("GuardrailFault", (refused) => new AgentFault({ reason: "refused", detail: refused.reason })),
    Match.orElse(_backed), // a meter or transport fault inside session maintenance is a session fault, never a refusal
  )

const _open = (row: Session) =>
  Effect.gen(function* () {
    const persistence = yield* Chat.Persistence
    const chat = yield* Effect.mapError(persistence.getOrCreate(row.key, { timeToLive: row.idle }), _backed)
    const seeded = (policy: Guardrail.Policy, history: Prompt.Prompt) =>
      Match.value(row.compaction).pipe(
        Match.when("trim", () => Tokens.fit(history, row.budget)),
        // the free carrier, deliberately: a digest routed through `chat` would append itself to the history it compacts
        Match.when("digest", () =>
          Effect.map(
            Guardrail.generate(policy, { _tag: "Text", options: { prompt: history, toolkit: Toolkit.empty } }),
            (summary) => Prompt.make(`memory: ${summary.text}`),
          )),
        Match.exhaustive,
      )
    const compact = (policy: Guardrail.Policy) =>
      Ref.get(chat.history).pipe(
        Effect.flatMap((history) => Effect.mapError(seeded(policy, history), _folded)),
        Effect.flatMap((folded) => Ref.set(chat.history, folded)), // the ONE history write: both lanes are pure prompt producers
        Effect.zipRight(Effect.mapError(chat.save, _backed)), // the one save, earned by the hand write above
      )
    return { chat, compact } as const
  })
```

## [03]-[TURN]

[TURN]:
- Owner: `Agent` and the request triple. `_PHASES` is the one phase anchor — the `Turn.phase` literal spreads it and the machine's node guard closes against it, so the receipt vocabulary and the statechart roster are one declaration. `Act` is the inbound tagged request — the admitted `Session` carrier, utterance, and app-passed retrieval passages — with `Turn` as its success (the reply text, held-call evidence, citation band, spend receipt, settled phase) and `AgentFault` as its failure. The carrier owns identity, budget, compaction, safety mode, and lifetime together; repeating `session.key` and `mode` on the request creates independent knobs with contradictory turn values. `Schema.TaggedRequest` declares payload, success, and failure in one class, and that single declaration is the entity Rpc, the `Tool.fromTaggedRequest` row, and the wire contract.
- Law: the turn is one fold, budget-bounded — recall (`Session.open`), gauge-and-compact when the history outgrows its budget share, weave (`Tokens.weave` over the passages — retrieval arrives as values, the data wave is never imported), screen once (the gate screens the woven prompt, not each iteration), then the tool-loop: at most `steps` chat-carried gated generations where each iteration's `toolChoice` and `disableToolCallResolution` compile from the safety partition, and the loop exits on a toolless reply, a held call, or the step ceiling — the ceiling folding to an `exhausted`-classed fault, never a silent truncation.
- Law: the loop feeds ONLY new material because the carrier is the accumulator — the opening iteration carries the woven prompt plus the utterance and every later iteration carries `Prompt.empty`, since the chat has already merged the assistant message and its resolved tool results into history through the substrate's own `Prompt.fromResponseParts` append. A loop that re-feeds the prior response duplicates every tool result it meant to forward, and the reconstruction is the substrate's, never this page's.
- Law: the loop's carrier is `Either` on the rail, not a flag — `Effect.iterate` advances on `Either.right` and settles on `Either.left`, with the `while` gate reading both the carrier and the remaining steps, so the ceiling is the ONE place a still-advancing cursor survives the loop and that survival IS the `budget`-reasoned fault; a boolean `done` cell beside the state cannot state the difference between settled and spent.
- Law: every turn settles with evidence — spend accumulated across iterations from the gate's own accounting, phase read from the machine's macro receipt (never re-derived from what the fold just decided), held calls as data (each provider-minted `{ id, tool, params }` re-admitted through the `Json` schema so the parameter value is structurally comparable at release), and provenance as data: the response's `DocumentSourcePart`/`UrlSourcePart` citation parts project into the `Turn.sources` band with the address and the display title both, so a grounded reply carries citations a reader can follow; a turn's receipt is what supervision, billing, and the approval surface read, so the loop returns `Turn`, never bare text.
- Law: the turn drives the machine at exactly three points — `act` on entry, then `settle` or `hold` on the way out, and the compaction pair around the fold when the meter demands — so the phase on the receipt is the machine's own `entered` node, and `_isPhase` (the derived guard over the one phase anchor) narrows the node roster to the receipt vocabulary with no cast.
- Law: replay parity rides a deterministic `IdGenerator` Layer at the durable root so re-driven turns mint identical tool-call ids and the workflow journal stays byte-stable.
- Growth: a loop concern (reflection pass, plan-then-act) is a phase row plus a fold arm, never a second loop.
- Packages: `@effect/ai` (`Prompt`, `Tool`, `Toolkit`, `Response`, `LanguageModel`); `effect` (`Effect`, `Either`, `BigDecimal`, `Option`, `Schema`); `./model.ts` (`Guardrail`, `Spend`, `Tokens`, `Ladder`); `./tool.ts` (`Safety`).

```typescript
const _PHASES = ["idle", "thinking", "awaiting", "compacting"] as const
const _Phase = Schema.Literal(..._PHASES) // the one anchor spread: the receipt literal, the machine's node guard, and the phase refinement all read it
const _isPhase = Schema.is(_Phase)

const _reasons = FaultClass.family(["budget", "refused", "tool", "session"] as const, {
  budget: { class: "exhausted" },
  refused: { class: "denied" },
  // a disposition that contradicts held evidence is caller-malformed and quarantinable: re-driving the identical verdict set can never settle
  tool: { class: "invalid" },
  session: { class: "unavailable" },
})

class AgentFault extends Schema.TaggedError<AgentFault>()("AgentFault", {
  reason: _reasons.schema,
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _reasons.classOf(this.reason)
  }
  override get message(): string {
    return `<agent:${this.reason}> ${this.detail}`
  }
}

declare namespace Agent {
  type Json = null | boolean | number | string | ReadonlyArray<Json> | { readonly [key: string]: Json }
  type Session = Effect.Effect.Success<ReturnType<typeof _open>>
  type Drive<Tools extends Record<string, Tool.Any>> = {
    readonly actor: Actor
    readonly charter: string // the agent's identity block: a drive-level fact the weave seeds, never a per-request field on Act
    readonly policy: Guardrail.Policy
    readonly toolkit: Toolkit.WithHandler<Tools>
    readonly tier: Ladder.Tier
  }
  type Turning = {
    readonly prompt: Prompt.Prompt
    readonly left: number
    readonly spend: BigDecimal.BigDecimal
    readonly reply: string
    readonly sources: Turn["sources"]
    readonly held: ReadonlyArray<Held>
  }
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
  sources: Schema.Array(Schema.Struct({ kind: Schema.Literal("document", "url"), ref: Schema.String, title: Schema.String })),
  spend: Schema.BigDecimal,
  phase: _Phase,
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

const _spent = (fault: Spent): AgentFault => new AgentFault({ reason: "budget", detail: fault._tag })

const _landed = (entered: ReadonlyArray<string>): Turn["phase"] =>
  Option.getOrElse(Array.findFirst(entered, _isPhase), () => "idle" as const) // the machine's own entered node, narrowed by the anchor's derived guard

const _cited = (content: ReadonlyArray<Response.AnyPart>): Turn["sources"] =>
  Array.filterMap(content, (part) =>
    part.type !== "source" ? Option.none() : Option.some(
      part.sourceType === "document"
        ? { kind: "document" as const, ref: part.id, title: part.title }
        : { kind: "url" as const, ref: part.url.href, title: part.title },
    ))

const _evidence = Schema.decodeUnknown(Schema.Array(_Held))

const _kept = <Tools extends Record<string, Tool.Any>>(
  roster: ReadonlyArray<string>,
  response: LanguageModel.GenerateTextResponse<Tools>,
): Effect.Effect<ReadonlyArray<Agent.Held>, AgentFault> =>
  Effect.mapError(
    _evidence(
      Array.filterMap(response.toolCalls, (call) =>
        Array.contains(roster, call.name) ? Option.some({ id: call.id, tool: call.name, params: call.params }) : Option.none()),
    ),
    (fault) => new AgentFault({ reason: "tool", detail: fault._tag }),
  )

const _stepped = <Tools extends Record<string, Tool.Any>>(drive: Agent.Drive<Tools>, chat: Chat.Persisted, roster: ReadonlyArray<string>) =>
(state: Agent.Turning): Effect.Effect<Either.Either<Agent.Turning, Agent.Turning>, AgentFault, LanguageModel.LanguageModel> =>
  Guardrail.generate(drive.policy, { _tag: "Text", options: { prompt: state.prompt, toolkit: drive.toolkit } }, chat).pipe(
    Effect.mapError(_folded),
    Effect.flatMap((response) =>
      Effect.all({ spent: Spend.accounted(drive.tier, response), held: _kept(roster, response) }).pipe(
        Effect.map(({ held, spent }) => {
          const advanced: Agent.Turning = {
            prompt: Prompt.empty, // the chat already merged the assistant message and every resolved tool result into history
            left: state.left - 1,
            spend: BigDecimal.sum(state.spend, spent),
            reply: response.text,
            sources: Array.appendAll(state.sources, _cited(response.content)),
            held,
          }
          return held.length > 0 || response.toolCalls.length === 0 ? Either.left(advanced) : Either.right(advanced)
        }),
      )),
  )

const _measured = <Tools extends Record<string, Tool.Any>>(opened: Agent.Session, row: Session, drive: Agent.Drive<Tools>) =>
  Ref.get(opened.chat.history).pipe(
    Effect.flatMap((history) => Effect.mapError(Tokens.gauge(history), _folded)),
    Effect.flatMap((gauged) =>
      Effect.when(
        Effect.mapError(drive.actor.feed("compact"), _spent).pipe(
          Effect.zipRight(opened.compact(drive.policy)),
          Effect.zipRight(Effect.mapError(drive.actor.feed("done"), _spent)),
        ),
        () => gauged > row.budget.window - row.budget.reply, // the same retrieval share Tokens.fit cuts against: one threshold, two readers
      )),
    Effect.asVoid,
  )

const _act = <Tools extends Record<string, Tool.Any>>(act: Act, drive: Agent.Drive<Tools>) =>
  Effect.gen(function* () {
    const opened = yield* Session.open(act.session)
    const gate = yield* Effect.mapError(Guardrail.admitted(drive.policy), _folded)
    yield* _measured(opened, act.session, drive)
    const woven = yield* Effect.mapError(Tokens.weave(drive.charter, act.passages, act.session.budget), _folded)
    yield* Effect.mapError(drive.actor.feed("act"), _spent)
    const cursor = yield* Effect.iterate(
      Either.right<Agent.Turning, Agent.Turning>({
        prompt: Prompt.merge(woven, Prompt.make(act.utterance)),
        left: act.session.budget.steps,
        spend: BigDecimal.make(0n, 0),
        reply: "",
        sources: [],
        held: [],
      }),
      {
        while: (cursor) => Either.isRight(cursor) && Either.merge(cursor).left > 0,
        body: _stepped(drive, opened.chat, gate.held),
      },
    )
    return yield* Either.match(cursor, {
      // a Right surviving the gate means the ceiling stopped the loop while it was still advancing: spent, never silently truncated
      onRight: (spentOut) => Effect.fail(new AgentFault({ reason: "budget", detail: `steps:${spentOut.left}` })),
      onLeft: (settled) =>
        Effect.map(
          Effect.mapError(drive.actor.feed(settled.held.length > 0 ? "hold" : "settle"), _spent),
          (macro) =>
            new Turn({
              reply: settled.reply,
              held: settled.held,
              sources: settled.sources,
              spend: settled.spend,
              phase: _landed(macro.entered),
            }),
        ),
    })
  })
```

## [04]-[ACTOR]

[ACTOR]:
- Owner: the phase machine — one `Transition.spec`: a depth-one statechart (`session` compound over the four `_PHASES` atomics, the node roster closed against the anchor by the `_Nodes` guard), signals `act | settle | hold | release | compact | done`, verdict programs naming what the driver does next; the rows are the whole interaction protocol — an unmatched signal is an empty program, never a hand branch — the `awaiting` node's watch row arms the approval deadline as a delayed self-signal, and `recover` re-initializes a defecting actor under a `pulse`-budget schedule. The compiled spec's `boot` runs the machine scoped beside the session so phase and history live and die together; `freeze`/`restore` carry an interactive session across a page or process hop.
- Law: the signal plane is literal-only by the core machine's own law, so held-call evidence never rides a signal and the machine's `extended` stays `Schema.Null` — the durable evidence home is the persisted chat itself, and the mechanism is the carrier: because the turn generates THROUGH the chat, the emitted tool-call parts land in history and in the backing store as part of the same call that produced them, and `_kept` projects the `Turn.held` band off that very response. A second evidence store beside the substrate is the named split, and a gate that bypassed the chat would leave this law with nothing behind it.
- Law: recovery is priced by the branch budget — `recover` is `Budget.schedule("pulse")`, the interactive point-op row, so a defecting actor re-initializes under the same jittered, attempt-bounded, window-capped geometry every other interactive rail uses; a curve composed at this site would carry no jitter, no reset, and no elapsed bound the row already states.
- Law: the altitude ruling is enforced by construction — the in-process actor serves the interactive lane (live phase, request-serialized turns, snapshot durability); the durable multi-process lane is `Actor.make({ name: "agent", protocol, clazz: "interactive", tenant })` over the SAME `Act` protocol, where per-session single-writer ordering, mailbox fencing, and message durability are entity facts — the machine table travels unchanged, and no third session runtime exists between them.
- Law: the turn drives the machine, never the reverse — `act` enters `thinking`, a toolless settle emits `settle` back to `idle`, a held call emits `hold` into `awaiting`, the compaction trigger emits `compact`; a phase mutated outside a signal is unspellable because the table is the only transition author.
- Growth: a new interaction posture (streaming turn, background reflection) is a node row plus its transition rows; the entity escalation inherits it by sharing the spec.
- Packages: `@rasm/ts/core` (`Transition`, `Budget`); `../work/entity.ts` (`Actor` — the escalation mint).

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
  recover: Budget.schedule("pulse"), // the interactive point-op geometry, not a per-site curve
})

const _boot = _spec.boot
```

## [05]-[APPROVAL]

[APPROVAL]:
- Owner: the held-call fold — when the gate's admission returns held names, the generation runs with tool resolution disabled, `_kept` intersects the response's emitted tool calls with that held roster and re-admits each `{ id, tool, params }` through the `Json` schema onto the `Turn.held` band (the durable copy riding the same chat-carried write), and the machine enters `awaiting` on the `hold` signal the turn fold feeds. Release is a COMPLETE disposition: `Agent.release(spec)` demands exactly one id-correlated verdict per held call, rejects duplicate ids and structural parameter drift, refuses an empty held band, settles every verdict through the supplied tool-result continuation in source order, and feeds `release` only after every settlement succeeds; the returned `Release` receipt carries both partitions as the audit evidence. The two release paths split by longevity: the in-process path supplies the live continuation, and the durable path — an approval that outlives the process — declares a `work/flow#SIGNAL_GATE` deferred whose token travels to the approval surface and whose settlement re-drives the entity turn with the same fold; the `awaiting` watch row expires unanswered holds into `done`, so an abandoned approval degrades to a bounded, evidenced no-op.
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
        Effect.zipRight(Effect.mapError(spec.actor.feed("release"), _spent)), // the machine's fuel rail folds at its own seam; the settle channel is already typed
        Effect.as(receipt),
      )
    : Effect.fail(new AgentFault({ reason: "tool", detail: "disposition incomplete or differs from held evidence" }))
}

const _pending = (turn: Turn) => turn.held.length > 0

const Agent = {
  tool: _asTool,
  spec: _spec,
  boot: _boot,
  act: _act,
  release: _release,
  held: _pending,
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
