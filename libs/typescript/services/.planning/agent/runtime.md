# [SERVICES_RUNTIME]

The durable multi-step AI agent composed from primitives, not a stock owner — `DurableAgent`, a tool-using agent that maintains conversation state across workflow steps, calls tools through an `@effect/ai` `Toolkit`, and survives interrupt/resume on the cluster engine, journaling every step to `AgentJournal`. The `DurableAgent` primitive DOES NOT EXIST in `@effect/ai` or `@effect/workflow`: the agent is COMPOSED from `Activity` (the run-once durable unit), `@effect/ai` `Toolkit`/`Tool`/`Chat.layerPersisted` (the tool-calling chat session), and `ClusterEngine` (the durable execution kernel), over the SAME one `AiProvider` literal axis `execution/ai#AI_ACTIVITY` declares. The agent is one durable unit on `execution/engine#ENGINE`, not a parallel agent per provider; provider selection is a runtime layer swap. This page is the first member of the one `./agent` subpath closure (agent-runtime + mcp-transport + session-actors). It is node-only, never browser-reachable, and crosses no .NET wire except through the BLOCKED `mcp-transport` sibling.

## [01]-[INDEX]

- [01]-[AGENT_RUNTIME]: owns the composed `DurableAgent`, the per-step tool-calling loop, the persisted `Chat` session, and the `AgentJournal` checkpoint replay.

## [02]-[AGENT_RUNTIME]

- Owner: `DurableAgent`, the multi-step tool-using agent COMPOSED from `Activity` + `@effect/ai` `Toolkit`/`Chat.layerPersisted` + `ClusterEngine`, over the one `execution/ai#AI_ACTIVITY` `AiProvider` literal axis; the agent's checkpoints ARE `AgentJournal` rows, not a side log.
- Cases: the `DurableAgent` primitive is absent from `@effect/ai`/`@effect/workflow`, so the agent is one composition — `AgentToolkit` is the `@effect/ai` `Toolkit.make(...)` of `Tool.make(name, { parameters, success, failure })` tools with handlers bound through `toolkit.toLayer({ ToolName: (params) => Effect... })`, the `failureMode: "error"` default routing tool failures into the effect error channel; the agent step is one `Activity.make` durable unit whose body is a `Chat` `generateText` (or `generateObject`) call passing the bound toolkit as `GenerateTextOptions.toolkit`, the selected provider `LanguageModel.LanguageModel` provided over the activity through the unified model layer, so the five providers stay one row on the agent and not five agents; the conversation persists through `Chat.layerPersisted({ storeId })` so the chat history survives across activity attempts and process restarts, the chat backplane keyed by the agent session; each step the agent emits — `session_start` on hydrate, `tool_call` per resolved tool, `checkpoint` per model turn, `session_complete` on terminal — writes one `AgentJournal` row through the one `persistence/store#STORE_BOUNDARY` `SqlClient`, so the journal IS the checkpoint ledger; interrupt/resume replays from the journal — `Workflow.SuspendOnFailure` suspends the workflow on a tool or provider error, `ClusterEngine` resumes it by execution id, and the agent re-hydrates the persisted `Chat` and the journal rows rather than restarting the conversation.
- Entry: the agent runs as a durable unit on `execution/engine#ENGINE` `ClusterEngine` — the agent workflow's `execute` body composes the per-step activities over the cluster-backed `WorkflowEngine`, the activity interrupt-retry schedule keying to the effect's FAILURE value (`Schema.Schema.Type<E>`) not a `Cause`, one convention shared with `ai-activity`; the persisted `Chat` and the `AgentJournal` rows ride the one `persistence/store#STORE_BOUNDARY` `PgClient`, never a second store; the `AiProvider` axis stays the sole declaration site at `execution/ai#AI_ACTIVITY` and the agent references it as settled vocabulary, selecting the provider model layer at the composition root so provider-agnostic agent logic chooses the provider at runtime without a parallel agent per provider; the agent's tool catalog is the `AgentToolkit` here, distinct from the host MCP tool catalog the BLOCKED `mcp#MCP_TRANSPORT` sibling consumes.
- Packages: `@effect/ai` for the `Toolkit`/`Tool`/`Chat`/`LanguageModel` surface (`Toolkit.make`, `Tool.make`, `toolkit.toLayer`, `Chat.layerPersisted`/`Chat.makePersisted`, `LanguageModel.generateText`/`generateObject`), the `@effect/ai-*` provider set for the five provider layers, `@effect/workflow` for `Activity.make` and the workflow `SuspendOnFailure` resume annotation, `@effect/cluster` for the engine the agent resumes on, `@effect/sql`/`@effect/sql-pg` for the journal rows, `@effect/platform-node` for the driver host.
- Growth: a new agent tool lands as one `Tool.make` row on `AgentToolkit` with one `toLayer` handler, never a parallel toolkit; a new agent event lands as one `AgentJournal` row kind; a new provider lands as one `AiProvider` literal at the `ai-activity` declaration site, never a new agent; a new agent capability lands as one activity on the agent workflow, never a second engine.
- Boundary: the named defects — a hand-rolled state machine instead of the `Chat.layerPersisted` + `AgentJournal` checkpoint composition; a parallel agent per provider instead of the one runtime-selected `AiProvider` row; a side log beside the `AgentJournal` checkpoint ledger; a second chat/journal store beside the one `PgClient`; a re-minted `DurableAgent` type asserting the absent primitive exists. The agent is the only multi-step AI surface in the branch, node-only, never browser-reachable; the SQL boundary is reached only through the one `persistence/store#STORE_BOUNDARY`, never a second client.

```ts contract
import type { LanguageModel } from "@effect/ai"
import type { SqlError } from "@effect/sql"
import type { Workflow } from "@effect/workflow"
import { Chat, Tool, Toolkit } from "@effect/ai"
import { Activity } from "@effect/workflow"
import { Effect, Layer, Schema } from "effect"
import { AiProvider } from "../execution/ai.js"
import { AgentJournal } from "../execution/ai.js"

const AgentEvent = Schema.Literal("session_start", "tool_call", "checkpoint", "session_complete")
type AgentEvent = Schema.Schema.Type<typeof AgentEvent>

class AgentFault extends Schema.TaggedError<AgentFault>()("AgentFault", {
  sessionId: Schema.String,
  stage: Schema.Literal("hydrate", "tool", "model", "checkpoint"),
  cause: Schema.Unknown,
}) {}

class AgentTurn extends Schema.Class<AgentTurn>("AgentTurn")({
  sessionId: Schema.String,
  text: Schema.String,
  toolCalls: Schema.Number,
  finishReason: Schema.String,
}) {}

interface AgentTool<Name extends string, P extends Schema.Struct.Fields, S extends Schema.Schema.Any, E extends Schema.Schema.All> {
  readonly name: Name
  readonly tool: Tool.Tool<Name, { parameters: Schema.Struct<P>; success: S; failure: E; failureMode: "error" }, never>
}

interface DurableAgent {
  readonly toolkit: <Tools extends ReadonlyArray<Tool.Any>>(...tools: Tools) => Toolkit.Toolkit<Toolkit.ToolsByName<Tools>>
  readonly chat: (storeId: string) => Layer.Layer<Chat.Persistence>
  readonly journal: (sessionId: string, kind: AgentEvent, payload: unknown) => Effect.Effect<void, SqlError.SqlError>
  readonly step: <Tools extends Record<string, Tool.Any>>(options: {
    readonly name: string
    readonly provider: AiProvider
    readonly model: string
    readonly sessionId: string
    readonly prompt: string
    readonly toolkit: Toolkit.WithHandler<Tools>
  }) => Activity.Activity<typeof AgentTurn, typeof AgentFault, LanguageModel.LanguageModel | Chat.Chat>
  readonly run: (options: {
    readonly provider: AiProvider
    readonly model: string
    readonly sessionId: string
    readonly storeId: string
    readonly prompt: string
  }) => Effect.Effect<AgentJournal, AgentFault, Workflow.WorkflowEngine | LanguageModel.LanguageModel>
}

const agentStep = <Tools extends Record<string, Tool.Any>>(options: {
  readonly name: string
  readonly sessionId: string
  readonly prompt: string
  readonly toolkit: Toolkit.WithHandler<Tools>
  readonly journal: (kind: AgentEvent, payload: unknown) => Effect.Effect<void, SqlError.SqlError>
}): Activity.Activity<typeof AgentTurn, typeof AgentFault, LanguageModel.LanguageModel | Chat.Chat> =>
  Activity.make({
    name: options.name,
    success: AgentTurn,
    error: AgentFault,
    execute: Effect.gen(function* () {
      const chat = yield* Chat.Chat
      yield* options.journal("checkpoint", { prompt: options.prompt }).pipe(Effect.ignore)
      const response = yield* chat.generateText({ prompt: options.prompt, toolkit: options.toolkit }).pipe(
        Effect.mapError((cause) => new AgentFault({ sessionId: options.sessionId, stage: "model", cause })),
      )
      yield* Effect.forEach(response.toolResults, (result) => options.journal("tool_call", result), { discard: true }).pipe(Effect.ignore)
      return new AgentTurn({ sessionId: options.sessionId, text: response.text, toolCalls: response.toolCalls.length, finishReason: response.finishReason })
    }),
  })
```
