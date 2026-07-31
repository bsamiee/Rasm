# [RUNTIME_TOOL]

The tool vocabulary and both MCP lanes in one owner: tools are `Schema`-typed data (`Tool.make` declarations collected by `Toolkit.make`, merged by `Toolkit.merge`, handled by `toLayer` — a god-toolkit is structurally impossible because assembly happens at the consumer), the `Safety` partition is the ONE blast-radius admission every generation gate and agent turn consumes — with one hint-admission constructor both the local annotation band and the remote MCP hint band fold through, so the fail-closed default is a single spelling — the `Arsenal` ledger types the four provider packages that expose provider-defined tools as rows of one name-keyed table, and MCP is a duality with a hard boundary: hosting is NATIVE (`McpServer`/`McpSchema` from `@effect/ai` — toolkits, resources, prompts, and elicitation served over stdio or HTTP transports), consumption is the reference SDK's client lane ONLY (`@modelcontextprotocol/sdk` `Client` transcribed at the seam — Promise lifted, every result re-parsed through the native `McpSchema` classes, hints graded onto `Safety` — the `./server` subpath has no import site). That lane is bidirectional and bounded: the session enumerates resources before reading one, watches a subscribed resource as a filtered channel feed, answers a server that elicits, and prices every request against one `Budget` row whose attempt deadline, total deadline, and interrupt-wired signal reach the SDK's own `RequestOptions`. A remote server's tools enter an app toolkit as ordinary declared rows, so the language model treats local, provider-defined, and remote tools identically and no raw wire value escapes the client seam. The module is `runtime/src/ai/tool.ts`.

## [01]-[INDEX]

- [02]-[TOOL_LAW]: the declaration and assembly law — schemas, failure routing, annotations as hints; —.
- [03]-[SAFETY]: blast-radius classes, posture modes, hint admission, the fail-closed partition; `Safety`.
- [04]-[ARSENAL]: the provider-defined tool ledger — four package rosters as one graded table; `Arsenal`.
- [05]-[HOST]: native MCP hosting — toolkit projection, resources, prompts, elicitation, transports; `Host`.
- [06]-[REMOTE]: the SDK client lane — one scoped session owner, budgeted calls, declared rows, both inbound halves; `Remote`.

## [02]-[TOOL_LAW]

[TOOL_LAW]:
- Law: a tool is declared once by `Tool.make(name, { description, parameters, success, failure, failureMode, dependencies })` — parameters, success, and failure are `Schema`s, `dependencies` threads `Context.Tag`s into the handler's `R`, and `failureMode` is the routing policy: `"return"` keeps a handler failure inside the tool result so the model self-corrects in-band, `"error"` lifts it onto the effect rail; the mode is chosen per tool by whether the model or the caller owns recovery.
- Law: `Tool.fromTaggedRequest(schema)` lifts a `Schema.TaggedRequest` — the same triple that serves an entity message and a workflow payload — so one request class is simultaneously actor protocol, workflow definition, and tool row; agents-as-tools is this lift applied to an agent's `Act` class, never a wrapper.
- Law: annotations are the hint band — `Tool.Readonly`/`Destructive`/`Idempotent`/`OpenWorld`/`Title` project one-to-one onto MCP tool hints at the host seam and seed the `Safety` grade through the one admission constructor; a tool declared without annotations grades through the same constructor's absent arm and lands fail-closed.
- Law: `ToolFault` is `Schema.TaggedError` because it crosses in-band — under `failureMode: "return"` the failure serializes into the tool result the model reads, and the remote rows declare it as their failure schema — so the fault is encodable by construction. Its rows close through the core `FaultClass.family` seam: `reason` derives its literal schema from the mint, `class` projects the mint's row, and severity, retryability, blame, and quarantine come from the core row table, so no local guard pair and no rank or retry column ride beside `class`. Five reasons route recovery — `dial` (transport or handshake) and `call` (a rejected invocation) classify `unavailable` and re-dial or re-issue, `lapsed` (a deadline the budget row set) classifies `expired` so a blown call is priced as budget exhaustion rather than as an unhealthy server, `shape` (a re-parse miss) classifies `malformed` and quarantines, and `declined` (a refused credential or an `isError` result) classifies `denied` and never re-drives.
- Law: assembly is data — a folder exports `Toolkit.make(...tools)` values; the composition root merges selected toolkits with `Toolkit.merge` and binds handlers with `toolkit.toLayer(handlers)` where the handler record is compiler-checked exhaustive; `Toolkit.empty` seeds gated calls that admit no tools.
- Law: `Tool.make` and `Toolkit.make` are used directly — a local wrapper renaming the declaration surface is the one-hop defect; this page adds vocabulary beside the package surface, never in front of it.

```typescript
import { McpSchema, McpServer, Tool, Toolkit } from "@effect/ai"
import { Client } from "@modelcontextprotocol/sdk/client/index.js"
import { StdioClientTransport, getDefaultEnvironment } from "@modelcontextprotocol/sdk/client/stdio.js"
import { StreamableHTTPClientTransport } from "@modelcontextprotocol/sdk/client/streamableHttp.js"
import { UnauthorizedError, type OAuthClientProvider } from "@modelcontextprotocol/sdk/client/auth.js"
import type { RequestOptions } from "@modelcontextprotocol/sdk/shared/protocol.js"
import { ElicitRequestSchema, ErrorCode, McpError, ResourceUpdatedNotificationSchema } from "@modelcontextprotocol/sdk/types.js"
import { AnthropicTool } from "@effect/ai-anthropic"
import { AmazonBedrockTool } from "@effect/ai-amazon-bedrock"
import { GoogleTool } from "@effect/ai-google"
import { OpenAiTool } from "@effect/ai-openai"
import { Array, Context, Duration, Effect, FiberSet, Layer, Match, Option, PubSub, Queue, Record, Schema, type Scope, Sink, Stream } from "effect"
import { Budget, FaultClass } from "@rasm/ts/core"

const _faults = FaultClass.family(["dial", "call", "lapsed", "shape", "declined"] as const, {
  dial: { class: "unavailable" },
  call: { class: "unavailable" },
  lapsed: { class: "expired" },
  shape: { class: "malformed" },
  declined: { class: "denied" },
})

declare namespace ToolFault {
  type Reason = (typeof _faults.reasons)[number]
}

class ToolFault extends Schema.TaggedError<ToolFault>()("ToolFault", {
  reason: _faults.schema,
  server: Schema.String,
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _faults.classOf(this.reason)
  }
  override get message(): string {
    return `<tool:${this.reason}> ${this.server}: ${this.detail}`
  }
}
```

## [03]-[SAFETY]

[SAFETY]:
- Owner: `Safety` — the assembled admission vocabulary: four blast-radius classes in severity order (`read` — pure observation; `write` — reversible mutation; `spend` — external cost or egress; `destroy` — irreversible loss), three postures (`auto` — the model runs it; `held` — the call emits but a supervisor releases it; `deny` — the tool is invisible to the model), and the mode table crossing them — `autonomous` (read/write auto, spend held, destroy denied), `supervised` (read auto, write/spend held, destroy denied), `locked` (read auto, everything else denied).
- Law: hint admission is ONE constructor with MCP-default semantics — `Safety.hints(band)` folds an `Option`-carried annotation band into the total `Hints` record, and an absent field takes the protocol's own fail-closed default (`readOnly: false`, `destructive: true`, `idempotent: false`, `openWorld: true`), so an entirely unannotated tool grades `destroy` by the same fold that grades a fully annotated one; the local annotation band and the remote `readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint` band both admit through this constructor, and a second admission entry with divergent absent-field defaults is the cross-seam split-brain this owner exists to kill.
- Law: `Safety.grade(hints)` derives the class in severity order — `destructive` grades `destroy`, `openWorld` lifts to `spend`, `readOnly` grades `read`, the remainder is `write` — so under the admission defaults an ungraded tool is `destroy`: fail-closed is structural, never a posture.
- Law: `Safety.admit(graded, mode)` is the ONE partition — it splits graded names into `{ allowed, held }` by the mode row, and the generation gate compiles `allowed` into its `toolChoice` restriction while the agent loop holds `held` calls as approval evidence; a second admission read anywhere (a gate-local list, an agent-local check) is the named split-brain.
- Growth: a new blast class is one tuple entry plus one column on each mode row; a new mode is one row; a per-tenant mode override is a mode value carried on the caller's context, resolved before `admit`.
- Packages: `@effect/ai` (`Tool`); `effect` (`Array`, `Context`, `Option`, `Record`).

```typescript
const _classes = ["read", "write", "spend", "destroy"] as const
const _modes = {
  autonomous: { read: "auto", write: "auto", spend: "held", destroy: "deny" },
  supervised: { read: "auto", write: "held", spend: "held", destroy: "deny" },
  locked: { read: "auto", write: "deny", spend: "deny", destroy: "deny" },
} as const

declare namespace Safety {
  type Class = (typeof _classes)[number]
  type Posture = "auto" | "held" | "deny"
  type Mode = keyof typeof _modes
  type Band = {
    readonly readOnlyHint?: boolean
    readonly destructiveHint?: boolean
    readonly idempotentHint?: boolean
    readonly openWorldHint?: boolean
  }
  type Hints = {
    readonly readOnly: boolean
    readonly destructive: boolean
    readonly idempotent: boolean
    readonly openWorld: boolean
  }
  type Admission = { readonly allowed: ReadonlyArray<string>; readonly held: ReadonlyArray<string> }
  type _Rows<T extends { readonly [M in Mode]: { readonly [C in Class]: Posture } } = typeof _modes> = T
}

const _hinted = (band: Option.Option<Safety.Band>): Safety.Hints =>
  Option.match(band, {
    // the absent band and every absent field take the MCP defaults: destructive true, openWorld true — fail-closed falls out of admission
    onNone: (): Safety.Hints => ({ readOnly: false, destructive: true, idempotent: false, openWorld: true }),
    onSome: (hints): Safety.Hints => ({
      readOnly: hints.readOnlyHint === true,
      destructive: hints.destructiveHint !== false,
      idempotent: hints.idempotentHint === true,
      openWorld: hints.openWorldHint !== false,
    }),
  })

const _grade = (hints: Safety.Hints): Safety.Class =>
  hints.destructive ? "destroy" : hints.openWorld ? "spend" : hints.readOnly ? "read" : "write"

const _local = (tool: Tool.Any): { readonly name: string; readonly clazz: Safety.Class } => ({
  name: tool.name,
  clazz: _grade(_hinted(Option.some({
    readOnlyHint: Option.getOrUndefined(Context.getOption(tool.annotations, Tool.Readonly)),
    destructiveHint: Option.getOrUndefined(Context.getOption(tool.annotations, Tool.Destructive)),
    idempotentHint: Option.getOrUndefined(Context.getOption(tool.annotations, Tool.Idempotent)),
    openWorldHint: Option.getOrUndefined(Context.getOption(tool.annotations, Tool.OpenWorld)),
  }))),
})

const _admit = (graded: ReadonlyArray<{ readonly name: string; readonly clazz: Safety.Class }>, mode: Safety.Mode): Safety.Admission => ({
  allowed: graded.filter((tool) => _modes[mode][tool.clazz] === "auto").map((tool) => tool.name),
  held: graded.filter((tool) => _modes[mode][tool.clazz] === "held").map((tool) => tool.name),
})

const Safety = {
  classes: _classes,
  modes: _modes,
  hints: _hinted,
  grade: _grade,
  local: _local,
  admit: _admit,
}
```

## [04]-[ARSENAL]

[ARSENAL]:
- Owner: `Arsenal` — the provider-defined tool ledger: one name-keyed table whose rows carry the constructor, the executing family, and the pre-assigned `Safety` class, so a provider-executed capability (web search, code execution, computer use, file search) is admitted by naming a row and the gate grades it without a hint band — the provider runs it, the ledger prices it. The verified rosters: `OpenAiTool` ships `CodeInterpreter`/`FileSearch`/`WebSearch`/`WebSearchPreview`; `AnthropicTool` ships Bash, computer-use, text-editor, code-execution, and search families; `GoogleTool` ships code execution, current search, legacy dynamic search, and URL context; `AmazonBedrockTool` ships the Anthropic-on-Bedrock Bash, computer-use, and text-editor generations. `@effect/ai-openrouter` exposes no provider-defined tool module, so it contributes no phantom roster. Every materialized row is typed `Tool.ProviderDefined` by its own package, never a local type.
- Law: web-search rows grade `spend` (external egress, per-call cost), code-execution rows grade `spend`, computer-use and bash rows grade `destroy` — held or denied under every shipped mode — and a ledger row's class is data a deployment tightens, never loosens.
- Law: the ledger is the only place a provider tool name appears — a page or app composing `OpenAiTool.WebSearch(...)` inline bypasses pricing and is unspellable; `Arsenal.row(name)` answers the exact value-derived row so its constructor signature survives selection. `Arsenal.Display`, `Arsenal.Threshold`, and `Arsenal.StoreIds` admit the package's loose geometry, confidence, and vector-store inputs before a provider row can form.
- Growth: a provider's new built-in is one ledger row; a package gaining its first provider-defined tool contributes one row band with its own constructors.
- Packages: `@effect/ai` (`Tool.ProviderDefined`); `@effect/ai-amazon-bedrock` (`AmazonBedrockTool`); `@effect/ai-anthropic` (`AnthropicTool`); `@effect/ai-google` (`GoogleTool`); `@effect/ai-openai` (`OpenAiTool`).

```typescript
const _Display = Schema.Struct({
  width: Schema.Int.pipe(Schema.positive()),
  height: Schema.Int.pipe(Schema.positive()),
  number: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
})
const _Threshold = Schema.Number.pipe(Schema.between(0, 1))
const _StoreIds = Schema.NonEmptyArray(Schema.NonEmptyString)

const _arsenal = {
  webSearch: { build: () => OpenAiTool.WebSearch({}), family: "openai", clazz: "spend" },
  webSearchPreview: { build: () => OpenAiTool.WebSearchPreview({}), family: "openai", clazz: "spend" },
  fileSearch: { build: (ids: Arsenal.StoreIds) => OpenAiTool.FileSearch({ vector_store_ids: ids }), family: "openai", clazz: "read" },
  codeInterpreter: { build: () => OpenAiTool.CodeInterpreter({ container: { type: "auto" } }), family: "openai", clazz: "spend" },
  bash: { build: () => AnthropicTool.Bash_20250124({}), family: "anthropic", clazz: "destroy" },
  textEditor: { build: () => AnthropicTool.TextEditor_20250728({}), family: "anthropic", clazz: "destroy" },
  computer: {
    build: (display: Arsenal.Display) => AnthropicTool.ComputerUse_20250124({
      display_width_px: display.width,
      display_height_px: display.height,
      display_number: Option.getOrNull(display.number),
    }),
    family: "anthropic",
    clazz: "destroy",
  },
  codeExecution: { build: () => AnthropicTool.CodeExecution_20250825({}), family: "anthropic", clazz: "spend" },
  anthropicSearch: { build: () => AnthropicTool.WebSearch_20250305({}), family: "anthropic", clazz: "spend" },
  googleSearch: { build: () => GoogleTool.GoogleSearch({}), family: "google", clazz: "spend" },
  googleDynamicSearch: {
    build: (threshold: Arsenal.Threshold) => GoogleTool.GoogleSearchRetrieval({ mode: "MODE_DYNAMIC", dynamicThreshold: threshold }),
    family: "google",
    clazz: "spend",
  },
  googleUrl: { build: () => GoogleTool.UrlContext({}), family: "google", clazz: "spend" },
  googleCode: { build: () => GoogleTool.CodeExecution({}), family: "google", clazz: "spend" },
  bedrockBash: { build: () => AmazonBedrockTool.AnthropicBash_20250124({}), family: "bedrock", clazz: "destroy" },
  bedrockComputer: {
    build: (display: Arsenal.Display) => AmazonBedrockTool.AnthropicComputerUse_20250124({
      display_width_px: display.width,
      display_height_px: display.height,
      display_number: Option.getOrNull(display.number),
    }),
    family: "bedrock",
    clazz: "destroy",
  },
  bedrockTextEditor: { build: () => AmazonBedrockTool.AnthropicTextEditor_20250728({}), family: "bedrock", clazz: "destroy" },
} as const

declare namespace Arsenal {
  type Display = typeof _Display.Type
  type Threshold = typeof _Threshold.Type
  type StoreIds = typeof _StoreIds.Type
  type Name = keyof typeof _arsenal
  type Row = (typeof _arsenal)[Name]
}

const Arsenal = {
  ..._arsenal,
  Display: _Display,
  Threshold: _Threshold,
  StoreIds: _StoreIds,
  names: Record.keys(_arsenal),
  row: <Name extends Arsenal.Name>(name: Name): (typeof _arsenal)[Name] => _arsenal[name],
  clazz: (name: Arsenal.Name): Safety.Class => _arsenal[name].clazz,
}
```

## [05]-[HOST]

[HOST]:
- Owner: `Host` — native MCP hosting: `Host.serve(spec)` merges the capability Layers — `McpServer.toolkit(toolkit)` projecting an app toolkit as MCP tools with the annotation band as hints, `McpServer.resource` rows with typed `McpSchema.param` templates and completion functions, `McpServer.prompt` rows with `Schema`-typed parameters — onto one transport arm: `McpServer.layerStdio({ name, version, stdin, stdout })` for a spawned-server deployment, `McpServer.layerHttp({ name, version, path })` mounted beside the serving plane's routes. Elicitation is first-class: a handler that needs structured operator input mid-call runs `McpServer.elicit({ message, schema })` and the declined arm is the typed `ElicitationDeclined` folded to the tool's failure.
- Law: hosting never re-implements — the SDK's `./server` subpath has no import site; the native server IS the host, and the toolkit projected is the same admitted, safety-graded toolkit the in-process model consumes, so an MCP client sees exactly what the local model sees.
- Law: a resource row serves evidence, not computation — resource templates answer stored artifacts, reports, and projections by key; a resource whose read triggers work is a tool wearing the wrong capability.
- Law: local tools emit standard schemas — `Tool.getJsonSchema(tool)` is the `JsonSchema7` projection of any admitted tool, so the MCP projection and every wire-tool export publish the declaration's own schema and a hand-authored JSON-Schema beside a `Tool.make` row is unspellable.
- Growth: a new hosted capability is one row on the merged Layer set; a second transport deployment is an arm selection at the root.
- Packages: `@effect/ai` (`McpServer`, `McpSchema`); `effect` (`Layer`, `Schema`).

```typescript
declare namespace Host {
  type Spec<Tools extends Record<string, Tool.Any>> = {
    readonly name: string
    readonly version: string
    readonly toolkit: Toolkit.Toolkit<Tools>
    readonly resources: ReadonlyArray<Layer.Layer<never>>
    readonly prompts: ReadonlyArray<Layer.Layer<never>>
    readonly transport: { readonly kind: "stdio"; readonly stdin: Stream.Stream<Uint8Array>; readonly stdout: Sink.Sink<unknown, Uint8Array> } | { readonly kind: "http"; readonly path: `/${string}` }
  }
}

const _artifactResource = McpServer.resource`artifact://${McpSchema.param("key", Schema.String)}`({
  name: "artifact",
  mimeType: "application/octet-stream",
  completion: { key: (prefix: string) => Effect.succeed([prefix]) },
  content: (key) => Effect.succeed(`artifact:${key}`),
})

const _confirm = (message: string) =>
  McpServer.elicit({ message, schema: Schema.Struct({ approve: Schema.Boolean, note: Schema.String }) }).pipe(
    Effect.mapError((declined) => new ToolFault({ reason: "declined", server: "host", detail: String(declined) })),
  )

const _serve = <Tools extends Record<string, Tool.Any>>(spec: Host.Spec<Tools>) =>
  Layer.mergeAll(McpServer.toolkit(spec.toolkit), ...spec.resources, ...spec.prompts).pipe(
    Layer.provide(
      spec.transport.kind === "http"
        ? McpServer.layerHttp({ name: spec.name, version: spec.version, path: spec.transport.path })
        : McpServer.layerStdio({ name: spec.name, version: spec.version, stdin: spec.transport.stdin, stdout: spec.transport.stdout }),
    ),
  )

const Host = { serve: _serve, confirm: _confirm, artifact: _artifactResource }
```

## [06]-[REMOTE]

[REMOTE]:
- Owner: `Remote` — the outbound client lane as one scoped session owner: `Remote.dial(spec)` acquires the SDK `Client` under `Effect.acquireRelease` with `close` as its release, arms the inbound halves the protocol is bidirectional about, and only THEN connects — so a handshake that fails still tears its transport down, where a bracket whose acquire spans the connect would leak a half-started process — and yields the `Session` whose members are the only crossings: `call(row, params)` (encode through the row's parameter schema, `callTool`, re-parse the promise result through `McpSchema.CallToolResult`, then re-parse `structuredContent` through the row's success schema — an `isError` result folds to `declined` carrying the content text), `roster(mode)` (the graded census — `listTools` decoded through `McpSchema.ListToolsResult`, each hint band admitted through `Safety.hints` and partitioned by `Safety.admit`; the census carries evidence, never a call capability), `resources()` (the resource census — `listResources` and `listResourceTemplates` settled applicatively as one pair, each decoded through its `McpSchema` result class), `watch(uri)` (a live update feed for one resource), `read(uri)`/`prompt(name, args)`/`complete(ref)` (each decoded through its `McpSchema` result class), and `capabilities` (the post-init server facts). The Zod wire never escapes: every promise result crosses one native-Schema admission before any consumer sees it.
- Law: enumeration precedes reading — `resources()` is what makes `read(uri)` reachable by a caller that does not already know the URI, so `[05]-[HOST]`'s "a resource row serves evidence" has a consuming half; the pair settles under `Effect.all` with an explicit degree because the two lists are independent, and templates travel beside concrete rows so a parameterized address is admitted from the server's own template rather than assembled by hand.
- Law: resource subscription has ONE handler and N watchers — the notification method admits exactly one handler per client, so `_dial` registers it once feeding a sliding `PubSub` and `watch(uri)` is a scoped subscription filtered by URI prefix (the protocol states an update may name a sub-resource of the subscribed one). The subscribe RPC and its `unsubscribeResource` release bracket the stream's own scope through `Stream.unwrapScoped`, so the server-side subscription dies with the feed; a per-watcher handler registration would silently overwrite its predecessor, and that collision is the reason the fan-out is a channel rather than a callback.
- Law: elicitation is answered, not ignored — a spec carrying an `answer` responder registers the `elicitation` client capability BEFORE `connect` (a capability declared after the handshake is invisible to the server) and installs one request handler that decodes the ask through `McpSchema.Elicit.payloadSchema`, runs the responder on the rail, and encodes the verdict through `McpSchema.ElicitResult`. `Host.confirm` owns the hosting half of the same capability, so both directions of one protocol feature live in this file; a spec with no responder declares no capability, so a server never elicits into a lane that cannot answer.
- Law: the platform callback seams cross exactly twice, each by its own sanctioned spelling — a notification is a synchronous `Queue.unsafeOffer` onto the channel (no fiber per event), and a request handler that must answer with a promise runs through a `FiberSet.makeRuntimePromise` runner minted in the dial scope, so both handlers' work stays owned and dies when the session's scope closes.
- Law: remote tools enter a toolkit as DECLARED rows — `Remote.tool(session, row)` lifts a `Remote.Row` (name, description, parameter fields, success schema, safety hints) into an ordinary `Tool.make` declaration whose handler is `session.call` under `failureMode: "return"`; the four hints become native `Tool` annotations, so hosting, local grading, and remote grading consume one metadata band. The census names what a server ships, the declared row is what the model may call, and an undeclared remote tool is uncallable by construction.
- Law: transport is locality — `StdioClientTransport({ command, args, env: getDefaultEnvironment() })` for a local server process (the safe-env allowlist), `StreamableHTTPClientTransport(url, { authProvider })` for remote with the OAuth provider arriving as an app-passed value composing the security wave's ceremony — no `security` import here.
- Law: every call is bounded, cancellable, and priced by one row — `_bounded` builds the SDK's own `RequestOptions` from the spec's `Budget.Kind`: the row's per-attempt deadline becomes `timeout`, its whole-call deadline becomes `maxTotalTimeout`, and `resetTimeoutOnProgress` makes the server's progress notification the heartbeat that re-arms the per-attempt clock while the total stays absolute, so a long handler with liveness survives and a silent one does not. The `signal` is the one `Effect.tryPromise` hands its evaluator, so fiber interruption cancels the in-flight request instead of abandoning it — every `dialed` call takes `(signal) => Promise<T>` for exactly that reason. A per-call deadline literal is unspellable because the geometry is the branch budget row's.
- Law: every SDK rejection folds to `ToolFault` through one triage — `UnauthorizedError` to `declined`, an `McpError` carrying the request-timeout code to `lapsed`, the caller's stated reason otherwise — and the fault's class column routes retry through the caller's budget exactly like every other rail. The triage is `Match.instanceOf` arms over an open `unknown` residue, so `Match.orElse` is its lawful terminal.
- Growth: a new server is a dial spec value; a new SDK capability (tasks, sampling, roots) is one more member on the `Session` owner beside one handler in the dial scope.
- Packages: `@modelcontextprotocol/sdk` (`Client`, `StdioClientTransport`, `getDefaultEnvironment`, `StreamableHTTPClientTransport`, `OAuthClientProvider`, `RequestOptions`, `ElicitRequestSchema`, `ResourceUpdatedNotificationSchema`, `McpError`, `ErrorCode`); `@effect/ai` (`McpSchema`, `Tool`); `effect` (`Scope`, `Effect`, `Schema`, `PubSub`, `Queue`, `FiberSet`, `Match`, `Duration`, `Stream`); `@rasm/ts/core` (`Budget`).

```typescript
const _UPDATES = 256 // the resource-update channel sheds oldest: a stalled watcher never backpressures the transport's read loop

declare namespace Remote {
  type Spec = {
    readonly name: string
    readonly version: string
    readonly budget: Budget.Kind // the row whose attempt deadline bounds one request and whose total deadline bounds the whole call
    readonly answer: Option.Option<(ask: typeof McpSchema.Elicit.payloadSchema.Type) => Effect.Effect<typeof McpSchema.ElicitResult.Type, ToolFault>>
    readonly transport:
      | { readonly kind: "stdio"; readonly command: string; readonly args: ReadonlyArray<string> }
      | { readonly kind: "http"; readonly url: URL; readonly auth: Option.Option<OAuthClientProvider> }
  }
  type Row<Fields extends Schema.Struct.Fields, A, AI> = {
    readonly name: string
    readonly description: string
    readonly parameters: Fields
    readonly success: Schema.Schema<A, AI>
    readonly hints: Safety.Band
  }
  type Session = {
    readonly call: <Fields extends Schema.Struct.Fields, A, AI>(
      row: Row<Fields, A, AI>,
      params: Schema.Struct.Type<Fields>,
    ) => Effect.Effect<A, ToolFault>
    readonly roster: (mode: Safety.Mode) => Effect.Effect<{
      readonly graded: ReadonlyArray<{ readonly name: string; readonly clazz: Safety.Class }>
      readonly admission: Safety.Admission
    }, ToolFault>
    readonly resources: () => Effect.Effect<{
      readonly listed: ReadonlyArray<McpSchema.Resource>
      readonly templates: ReadonlyArray<McpSchema.ResourceTemplate>
    }, ToolFault>
    readonly watch: (uri: string) => Stream.Stream<string, ToolFault>
    readonly read: (uri: string) => Effect.Effect<McpSchema.ReadResourceResult, ToolFault>
    readonly prompt: (name: string, args: Record<string, string>) => Effect.Effect<McpSchema.GetPromptResult, ToolFault>
    readonly complete: (params: Parameters<Client["complete"]>[0]) => Effect.Effect<McpSchema.CompleteResult, ToolFault>
    readonly capabilities: Effect.Effect<Option.Option<McpSchema.ServerCapabilities>, ToolFault>
  }
}

const _bounded = (spec: Remote.Spec, signal: AbortSignal): RequestOptions => ({
  signal, // the fiber's own interrupt wiring: cancellation crosses the seam instead of abandoning an in-flight request
  timeout: Duration.toMillis(Budget[spec.budget].attempt),
  resetTimeoutOnProgress: true, // a progress notification IS the heartbeat re-arming the per-attempt clock
  maxTotalTimeout: Duration.toMillis(Budget[spec.budget].total),
})

const _lifted = (server: string, reason: ToolFault.Reason) => (cause: unknown): ToolFault =>
  Match.value(cause).pipe(
    Match.when(Match.instanceOf(UnauthorizedError), (fault) => new ToolFault({ reason: "declined", server, detail: fault.message })),
    Match.when(Match.instanceOf(McpError), (fault) =>
      new ToolFault({ reason: fault.code === ErrorCode.RequestTimeout ? "lapsed" : reason, server, detail: fault.message })),
    Match.orElse((residue) => new ToolFault({ reason, server, detail: String(residue) })), // instanceOf subtracts nothing, so the open residue earns orElse
  )

const _shaped = (server: string) => (fault: unknown): ToolFault =>
  new ToolFault({ reason: "shape", server, detail: String(fault) })

const _session = (spec: Remote.Spec, client: Client, updates: PubSub.PubSub<string>): Remote.Session => {
  const dialed = <T>(run: (signal: AbortSignal) => Promise<T>): Effect.Effect<T, ToolFault> =>
    Effect.tryPromise({ try: run, catch: _lifted(spec.name, "call") })
  const admitted = <A, I>(shape: Schema.Schema<A, I>) => (raw: unknown): Effect.Effect<A, ToolFault> =>
    Effect.mapError(Schema.decodeUnknown(shape)(raw), _shaped(spec.name))
  return {
    call: (row, params) =>
      Schema.encode(Schema.Struct(row.parameters))(params).pipe(
        Effect.flatMap(Schema.decodeUnknown(Schema.Record({ key: Schema.String, value: Schema.Unknown }))),
        Effect.mapError(_shaped(spec.name)),
        Effect.flatMap((args) => dialed((signal) => client.callTool({ name: row.name, arguments: args }, undefined, _bounded(spec, signal)))),
        Effect.flatMap(admitted(McpSchema.CallToolResult)),
        Effect.filterOrFail(
          (result) => result.isError !== true,
          (result) => new ToolFault({ reason: "declined", server: spec.name, detail: JSON.stringify(result.content) }),
        ),
        Effect.flatMap((result) => admitted(row.success)(result.structuredContent)),
      ),
    roster: (mode) =>
      dialed((signal) => client.listTools(undefined, _bounded(spec, signal))).pipe(
        Effect.flatMap(admitted(McpSchema.ListToolsResult)),
        Effect.map((result) =>
          Array.map(result.tools, (tool) => ({
            name: tool.name,
            clazz: _grade(_hinted(Option.fromNullable(tool.annotations))),
          }))
        ),
        Effect.map((graded) => ({ graded, admission: _admit(graded, mode) })),
      ),
    resources: () =>
      Effect.all({
        listed: dialed((signal) => client.listResources(undefined, _bounded(spec, signal))).pipe(
          Effect.flatMap(admitted(McpSchema.ListResourcesResult)),
          Effect.map((result) => result.resources),
        ),
        templates: dialed((signal) => client.listResourceTemplates(undefined, _bounded(spec, signal))).pipe(
          Effect.flatMap(admitted(McpSchema.ListResourceTemplatesResult)),
          Effect.map((result) => result.resourceTemplates),
        ),
      }, { concurrency: 2 }), // two independent censuses: the product, never a bind chain that serializes them
    watch: (uri) =>
      Stream.unwrapScoped(
        // the local subscription lands BEFORE the server-side one: an update arriving on the subscribe ack has a reader waiting
        PubSub.subscribe(updates).pipe(
          Effect.zipLeft(Effect.acquireRelease(
            dialed((signal) => client.subscribeResource({ uri }, _bounded(spec, signal))),
            () => Effect.ignore(dialed((signal) => client.unsubscribeResource({ uri }, _bounded(spec, signal)))),
          )),
          Effect.map((feed) => Stream.filter(Stream.fromQueue(feed), (updated) => updated.startsWith(uri))), // an update may name a sub-resource of the subscribed URI
        ),
      ),
    read: (uri) =>
      dialed((signal) => client.readResource({ uri }, _bounded(spec, signal))).pipe(Effect.flatMap(admitted(McpSchema.ReadResourceResult))),
    prompt: (name, args) =>
      dialed((signal) => client.getPrompt({ name, arguments: args }, _bounded(spec, signal))).pipe(
        Effect.flatMap(admitted(McpSchema.GetPromptResult)),
      ),
    complete: (params) =>
      dialed((signal) => client.complete(params, _bounded(spec, signal))).pipe(Effect.flatMap(admitted(McpSchema.CompleteResult))),
    capabilities: Effect.flatMap(
      Effect.sync(() => Option.fromNullable(client.getServerCapabilities())),
      Option.match({
        onNone: () => Effect.succeedNone,
        onSome: (raw) => Effect.asSome(admitted(McpSchema.ServerCapabilities)(raw)),
      }),
    ),
  }
}

const _dial = (spec: Remote.Spec): Effect.Effect<Remote.Session, ToolFault, Scope.Scope> =>
  Effect.gen(function* () {
    const updates = yield* Effect.acquireRelease(PubSub.sliding<string>(_UPDATES), PubSub.shutdown)
    const answered = yield* FiberSet.makeRuntimePromise<never>() // the one owned runner for the handler that must reply with a promise
    const client = yield* Effect.acquireRelease(
      Effect.sync(() => new Client({ name: spec.name, version: spec.version })),
      (held) => Effect.promise(() => held.close()),
    )
    yield* Effect.sync(() =>
      client.setNotificationHandler(ResourceUpdatedNotificationSchema, (note) => void Queue.unsafeOffer(updates, note.params.uri))
    )
    yield* Option.match(spec.answer, {
      onNone: () => Effect.void,
      onSome: (respond) =>
        Effect.sync(() => {
          client.registerCapabilities({ elicitation: {} }) // declared BEFORE connect or the server never sees the lane
          client.setRequestHandler(ElicitRequestSchema, (ask) =>
            answered(
              Schema.decodeUnknown(McpSchema.Elicit.payloadSchema)(ask.params).pipe(
                Effect.flatMap(respond),
                Effect.flatMap(Schema.encode(McpSchema.ElicitResult)),
              ),
            ))
        }),
    })
    const transport = spec.transport.kind === "stdio"
      ? new StdioClientTransport({ command: spec.transport.command, args: [...spec.transport.args], env: getDefaultEnvironment() })
      : new StreamableHTTPClientTransport(spec.transport.url, { authProvider: Option.getOrUndefined(spec.transport.auth) })
    yield* Effect.tryPromise({
      try: (signal) => client.connect(transport, _bounded(spec, signal)),
      catch: _lifted(spec.name, "dial"),
    })
    return _session(spec, client, updates)
  })

const _tool = <const Name extends string, Fields extends Schema.Struct.Fields, A, AI>(
  session: Remote.Session,
  row: Remote.Row<Fields, A, AI> & { readonly name: Name },
) => {
  const hints = _hinted(Option.some(row.hints))
  return [
    Tool.make(row.name, {
      description: row.description,
      parameters: row.parameters,
      success: row.success,
      failure: ToolFault,
      failureMode: "return",
    }).annotate(Tool.Readonly, hints.readOnly)
      .annotate(Tool.Destructive, hints.destructive)
      .annotate(Tool.Idempotent, hints.idempotent)
      .annotate(Tool.OpenWorld, hints.openWorld),
    (params: Schema.Struct.Type<Fields>) => session.call(row, params),
  ] as const
}

const Remote = { dial: _dial, tool: _tool }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Arsenal, Host, Remote, Safety, ToolFault }
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
