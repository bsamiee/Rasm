# [RUNTIME_AGENT]

The agent altitude, ruled and sealed: an agent session's interaction state is a `Transition` machine from the core state page — a closed phase spine driven by a transition table, booted as the in-process serializable actor with snapshot-grade durability — and its conversational memory is the persisted `Chat` substrate the intelligence package ships, handed to the model page's gate AS its carrier, so neither state nor history is hand-assembled anywhere. The turn is one fold: recall the session through the persistence Tag's restore-or-create, compact when the meter demands, weave app-passed retrieval into the measured prompt, screen once at the gate, then iterate chat-carried gated generation under a bounded step budget until a toolless reply, a held call, or the ceiling settles it — persistence needs no closing write because a persisted chat stores prompt and response on every generation. The `Act`/`Turn`/`AgentFault` schema triple is declared once as tagged requests and serves three surfaces without re-declaration — the cluster entity's message protocol (a durable multi-process session is `work/entity#ACTOR_MINT` applied to the same triple, single-writer turn order arriving as an entity fact), the agents-as-tools row (`Tool.fromTaggedRequest` lifts `Act` so an agent is callable by another agent's toolkit), and the serving plane's proxied drive. Held-tool approval is evidence, not ceremony: the gate's held partition emits unresolved calls, the emitted tool-call parts persist inside the chat history itself — the substrate is the evidence store — the machine holds the `awaiting` phase, and release demands a complete id-correlated, structurally equal disposition of every held call before the actor resumes. The phase vocabulary has ONE anchor: the `_PHASES` tuple feeds the `Turn.phase` literal and closes the machine's node roster through its guard, so the receipt and the statechart cannot drift. The module is `runtime/src/ai/agent.ts`.

## [01]-[INDEX]

- [02]-[SESSION]: persisted chat memory, the session row, the two compaction lanes; `Session`.
- [03]-[TURN]: the phase anchor, the chat-carried turn fold, the `Act`/`Turn`/`AgentFault` triple; `Agent`.
- [04]-[ACTOR]: the phase machine, the in-process boot, the entity escalation row; `Agent`.
- [05]-[APPROVAL]: the held-call evidence fold and the two release paths; `Agent`.

## [02]-[SESSION]

[SESSION]:
- Growth: a memory concern (pinned facts, user preferences) is a system block the digest lane preserves, never a second store.

```typescript
import { type AiError, Chat, type LanguageModel, Prompt, type Response, Tool, Toolkit } from "@effect/ai"
import { Array, BigDecimal, Effect, Either, Function, HashSet, Match, Option, Record, Ref, Schema } from "effect"
import { Fault, Transition } from "@rasm/ts/core"
import { Guardrail, GuardrailFault, Ladder, Spend, Tokens } from "./model.ts"
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

// Retryability must SURVIVE the fold. Every provider failure landing on one reason hands the class lattice a single
// verdict for a throttle, a revoked key, and a malformed answer, so whatever retries above this fold either replays a
// call that can never succeed or abandons one that would. The model page already grades the union on its real axis,
// so this fold reads that grade and lands each band on the reason carrying the SAME retryability.
const _bands = {
  exhausted: "budget",
  denied: "refused",
  unavailable: "session",
  expired: "session",
} as const satisfies Partial<Record<Fault.Class.Kind, AgentFault["reason"]>>

const _folded = (fault: GuardrailFault | AiError.AiError): AgentFault =>
  Match.value(fault).pipe(
    Match.tag("GuardrailFault", (refused) => new AgentFault({ reason: "refused", detail: refused.reason })),
    // an unbanded grade is terminal by construction: replaying identical octets against the same peer settles nothing
    Match.orElse((held) =>
      new AgentFault({
        reason: Record.get(_bands, Ladder.grade(held)).pipe(Option.getOrElse(() => "provider" as const)),
        detail: held._tag,
      })),
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
- Growth: a loop concern (reflection pass, plan-then-act) is a phase row plus a fold arm, never a second loop.

```typescript
const _PHASES = ["idle", "thinking", "awaiting", "compacting"] as const
const _Phase = Schema.Literal(..._PHASES) // the one anchor spread: the receipt literal, the machine's node guard, and the phase refinement all read it
const _isPhase = Schema.is(_Phase)

const _reasons = Fault.Class.family(["budget", "refused", "tool", "session", "provider"] as const, {
  budget: { class: "exhausted" },
  refused: { class: "denied" },
  // a disposition that contradicts held evidence is caller-malformed and quarantinable: re-driving the identical verdict set can never settle
  tool: { class: "invalid" },
  session: { class: "unavailable" },
  // a peer answering wrongly is not a session outage: `session` is retryable and would replay a call that cannot settle
  provider: { class: "malformed" },
})

class AgentFault extends Schema.TaggedError<AgentFault>()("AgentFault", {
  reason: _reasons.schema,
  detail: Schema.String,
}) {
  get class(): Fault.Class.Kind {
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

const _spent = (fault: Transition.Spent): AgentFault => new AgentFault({ reason: "budget", detail: fault._tag })

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
          (fact) =>
            new Turn({
              reply: settled.reply,
              held: settled.held,
              sources: settled.sources,
              spend: settled.spend,
              phase: _landed(fact.macro.entered),
            }),
        ),
    })
  })
```

## [04]-[ACTOR]

[ACTOR]:
- Packages: `@rasm/ts/core` (`Transition`, `Fault.Budget`); `effect` (`Function`); `../work/entity.ts` (`Actor` — the escalation mint).

```typescript
const _phaseNodes = {
  idle: { kind: "atomic", parent: "session" },
  thinking: { kind: "atomic", parent: "session" },
  awaiting: {
    kind: "atomic",
    parent: "session",
    watches: [{ key: "approval", after: "15 minutes", signal: "done" }],
  },
  compacting: { kind: "atomic", parent: "session" },
} as const satisfies Record<(typeof _PHASES)[number], object>

const _nodes = [
  { id: "session", kind: "compound", initial: "idle" },
  ...Array.map(_PHASES, (id) => ({ id, ..._phaseNodes[id] })),
] as const

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
  // `MachineDefect` is this schedule's sole inhabitant — a serializable machine boots without failure, so `InitError`
  // resolves `never` and the remaining defect carries `_tag`/`cause` alone, which the default gate refuses
  recover: () => Fault.Budget.schedule("pulse", Function.constTrue),
})

const _compiled = Effect.fromEither(_spec)
const _boot = Effect.flatMap(_compiled, (compiled) => compiled.boot)
const _restore = (frozen: Transition.Frozen) => Effect.flatMap(_compiled, (compiled) => compiled.restore(frozen))
```

## [05]-[APPROVAL]

[APPROVAL]:
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
  restore: _restore,
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

- [COMPACT_DIGEST]-[OPEN]: which member performs the `digest` compaction arm, given the `trim` arm binds `Tokens.fit` while `digest` names a summarizing rewrite no `Chat` or `Prompt` member performs; verify against `@effect/ai/Chat` and `@effect/ai/Prompt` on the member rail.
- [REASONING_CONTINUITY]-[OPEN]: whether a chat-carried multi-turn loop round-trips each provider's reasoning-continuity carrier (`AnthropicReasoningInfo`, `AmazonBedrockReasoningInfo`, `OpenRouterReasoningInfo`, the Google `thoughtSignature`, the OpenAI reasoning `itemId`/`encryptedContent`) through `Prompt.fromResponseParts`, since a dropped signature makes the next turn refuse; verify against `@effect/ai/Prompt` `fromResponseParts` and each provider's `Prompt` augmentation on the member rail.
