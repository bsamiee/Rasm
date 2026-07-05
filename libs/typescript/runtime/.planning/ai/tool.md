# [RUNTIME_TOOL]

The tool vocabulary and both MCP lanes in one owner: tools are `Schema`-typed data (`Tool.make` declarations collected by `Toolkit.make`, merged by `Toolkit.merge`, handled by `toLayer` — a god-toolkit is structurally impossible because assembly happens at the consumer), the `Safety` partition is the ONE blast-radius admission every generation gate and agent turn consumes, the `Arsenal` ledger types five providers' provider-defined tools as rows of one name-keyed table, and MCP is a duality with a hard boundary: hosting is NATIVE (`McpServer`/`McpSchema` from `@effect/ai` — toolkits, resources, prompts, and elicitation served over stdio or HTTP transports), consumption is the reference SDK's client lane ONLY (`@modelcontextprotocol/sdk` `Client` transcribed at the seam — Promise lifted, Zod results re-parsed through `effect/Schema`, hints graded onto `Safety` — the `./server` subpath has no import site). A remote server's tools enter an app toolkit as ordinary rows, so the language model treats local, provider-defined, and remote tools identically. The module is `runtime/src/ai/tool.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                             | [PUBLIC]  |
| :-----: | :----------- | :------------------------------------------------------------------------------------ | :-------- |
|  [01]   | `TOOL_LAW`   | the declaration and assembly law — schemas, failure routing, annotations as hints      | —         |
|  [02]   | `SAFETY`     | blast-radius classes, posture modes, the fail-closed admission partition               | `Safety`  |
|  [03]   | `ARSENAL`    | the provider-defined tool ledger — five rosters as one graded table                    | `Arsenal` |
|  [04]   | `HOST`       | native MCP hosting — toolkit projection, resources, prompts, elicitation, transports   | `Host`    |
|  [05]   | `REMOTE`     | the SDK client lane — scoped dial, roster projection, capability primitives            | `Remote`  |

## [2]-[TOOL_LAW]

[TOOL_LAW]:
- Law: a tool is declared once by `Tool.make(name, { description, parameters, success, failure, failureMode, dependencies })` — parameters, success, and failure are `Schema`s, `dependencies` threads `Context.Tag`s into the handler's `R`, and `failureMode` is the routing policy: `"return"` keeps a handler failure inside the tool result so the model self-corrects in-band, `"error"` lifts it onto the effect rail; the mode is chosen per tool by whether the model or the caller owns recovery.
- Law: `Tool.fromTaggedRequest(schema)` lifts a `Schema.TaggedRequest` — the same triple that serves an entity message and a workflow payload — so one request class is simultaneously actor protocol, workflow definition, and tool row; agents-as-tools is this lift applied to an agent's `Act` class, never a wrapper.
- Law: annotations are the hint band — `Tool.Readonly`/`Destructive`/`Idempotent`/`OpenWorld`/`Title` project one-to-one onto MCP tool hints at the host seam and seed the `Safety` grade; a tool declared without its annotations grades to the fail-closed default.
- Law: assembly is data — a folder exports `Toolkit.make(...tools)` values; the composition root merges selected toolkits with `Toolkit.merge` and binds handlers with `toolkit.toLayer(handlers)` where the handler record is compiler-checked exhaustive; `Toolkit.empty` seeds gated calls that admit no tools.
- Law: `Tool.make` and `Toolkit.make` are used directly — a local wrapper renaming the declaration surface is the one-hop defect; this page adds vocabulary beside the package surface, never in front of it.

```typescript
import { McpSchema, McpServer, Tool, Toolkit } from "@effect/ai"
import { Client } from "@modelcontextprotocol/sdk/client/index.js"
import { StdioClientTransport } from "@modelcontextprotocol/sdk/client/stdio.js"
import { StreamableHTTPClientTransport } from "@modelcontextprotocol/sdk/client/streamableHttp.js"
import type { OAuthClientProvider } from "@modelcontextprotocol/sdk/client/auth.js"
import { AnthropicTool } from "@effect/ai-anthropic"
import { OpenAiTool } from "@effect/ai-openai"
import { Data, Effect, Layer, Option, Record, Schema, Stream } from "effect"
import { FaultClass } from "@rasm/ts/core"

class ToolFault extends Data.TaggedError("ToolFault")<{
  readonly reason: "dial" | "call" | "shape" | "declined"
  readonly server: string
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return this.reason === "shape" ? "malformed" : this.reason === "declined" ? "denied" : "unavailable"
  }
}
```

## [3]-[SAFETY]

[SAFETY]:
- Owner: `Safety` — the assembled admission vocabulary: four blast-radius classes in severity order (`read` — pure observation; `write` — reversible mutation; `spend` — external cost or egress; `destroy` — irreversible loss), three postures (`auto` — the model runs it; `held` — the call emits but a supervisor releases it; `deny` — the tool is invisible to the model), and the mode table crossing them — `autonomous` (read/write auto, spend held, destroy denied), `supervised` (read auto, write/spend held, destroy denied), `locked` (read auto, everything else denied). `Safety.grade(tool)` derives a class from the annotation band — `Destructive` grades `destroy`, absent-`Readonly` grades `write`, `OpenWorld` lifts to `spend` — and an ungraded tool is `destroy`: fail-closed is the default, never a posture.
- Law: `Safety.admit(toolkit, mode)` is the ONE partition — it splits a toolkit's names into `{ allowed, held }` by the mode row, and the generation gate compiles `allowed` into its `toolChoice` restriction while the agent loop holds `held` calls as approval evidence; a second admission read anywhere (a gate-local list, an agent-local check) is the named split-brain this owner exists to kill.
- Law: remote hints grade through the same fold — the SDK's `readOnlyHint`/`destructiveHint`/`idempotentHint`/`openWorldHint` map onto the annotation band at `[5]`'s seam, so an external tool and a local tool are indistinguishable to admission.
- Growth: a new blast class is one tuple entry plus one column on each mode row; a new mode is one row; a per-tenant mode override is a mode value carried on the caller's context, resolved before `admit`.
- Packages: `@effect/ai` (`Tool`); `effect` (`Array`, `Record`).

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
  type Hints = {
    readonly readOnly: boolean
    readonly destructive: boolean
    readonly idempotent: boolean
    readonly openWorld: boolean
  }
  type Admission = { readonly allowed: ReadonlyArray<string>; readonly held: ReadonlyArray<string> }
  type _Rows<T extends { readonly [M in Mode]: { readonly [C in Class]: Posture } } = typeof _modes> = T
}

const _grade = (hints: Safety.Hints): Safety.Class =>
  hints.destructive ? "destroy" : hints.openWorld ? "spend" : hints.readOnly ? "read" : "write"

const _admit = (graded: ReadonlyArray<{ readonly name: string; readonly clazz: Safety.Class }>, mode: Safety.Mode): Safety.Admission => ({
  allowed: graded.filter((tool) => _modes[mode][tool.clazz] === "auto").map((tool) => tool.name),
  held: graded.filter((tool) => _modes[mode][tool.clazz] === "held").map((tool) => tool.name),
})

const Safety = {
  classes: _classes,
  modes: _modes,
  grade: _grade,
  admit: _admit,
}
```

## [4]-[ARSENAL]

[ARSENAL]:
- Owner: `Arsenal` — the provider-defined tool ledger: one name-keyed table whose rows carry the constructor, the executing family, and the pre-assigned `Safety` class, so a provider-executed capability (web search, code execution, computer use, file search) is admitted by naming a row and the gate grades it without a hint band — the provider runs it, the ledger prices it. The verified rosters: `OpenAiTool` ships `CodeInterpreter`/`FileSearch`/`WebSearch`/`WebSearchPreview`; `AnthropicTool` ships the Bash/Computer/TextEditor families plus its search rows; Bedrock re-uses the Anthropic tags through its Converse tool config; Google's roster rides its generateContent tool config — every row typed `Tool.ProviderDefined` by its own package, never a local type.
- Law: web-search rows grade `spend` (external egress, per-call cost), code-execution rows grade `spend`, computer-use and bash rows grade `destroy` — held or denied under every shipped mode — and a ledger row's class is data a deployment tightens, never loosens.
- Law: the ledger is the only place a provider tool name appears — a page or app composing `OpenAiTool.WebSearch(...)` inline bypasses pricing and is unspellable; `Arsenal.pick(names)` answers the constructed rows for a toolkit.
- Growth: a provider's new built-in is one ledger row; a sixth provider's roster is a row band with its package's constructors.
- Packages: `@effect/ai-openai` (`OpenAiTool`); `@effect/ai-anthropic` (`AnthropicTool`); `@effect/ai` (`Tool.ProviderDefined`).

```typescript
const _arsenal = {
  webSearch: { build: () => OpenAiTool.WebSearch({}), family: "openai", clazz: "spend" },
  fileSearch: { build: (ids: ReadonlyArray<string>) => OpenAiTool.FileSearch({ vector_store_ids: ids }), family: "openai", clazz: "read" },
  codeInterpreter: { build: () => OpenAiTool.CodeInterpreter({ container: { type: "auto" } }), family: "openai", clazz: "spend" },
  bash: { build: () => AnthropicTool.Bash_20250124({}), family: "anthropic", clazz: "destroy" },
  textEditor: { build: () => AnthropicTool.TextEditor_20250728({}), family: "anthropic", clazz: "destroy" },
} as const

declare namespace Arsenal {
  type Name = keyof typeof _arsenal
  type Row = (typeof _arsenal)[Name]
}

const Arsenal = {
  ..._arsenal,
  names: Record.keys(_arsenal),
  clazz: (name: Arsenal.Name): Safety.Class => _arsenal[name].clazz,
}
```

## [5]-[HOST]

[HOST]:
- Owner: `Host` — native MCP hosting: `Host.serve(spec)` merges the capability Layers — `McpServer.toolkit(toolkit)` projecting an app toolkit as MCP tools with the annotation band as hints, `McpServer.resource` rows with typed `McpSchema.param` templates and completion functions, `McpServer.prompt` rows with `Schema`-typed parameters — onto one transport arm: `McpServer.layerStdio({ name, version, stdin, stdout })` for a spawned-server deployment, `McpServer.layerHttp({ name, version, path })` mounted beside the serving plane's routes. Elicitation is first-class: a handler that needs structured operator input mid-call runs `McpServer.elicit({ message, schema })` and the declined arm is the typed `ElicitationDeclined` folded to the tool's failure.
- Law: hosting never re-implements — the SDK's `./server` subpath has no import site; the native server IS the host, and the toolkit projected is the same admitted, safety-graded toolkit the in-process model consumes, so an MCP client sees exactly what the local model sees.
- Law: a resource row serves evidence, not computation — resource templates answer stored artifacts, reports, and projections by key; a resource whose read triggers work is a tool wearing the wrong capability.
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
    readonly transport: { readonly kind: "stdio"; readonly stdin: Stream.Stream<Uint8Array>; readonly stdout: unknown } | { readonly kind: "http"; readonly path: `/${string}` }
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
        : McpServer.layerStdio({ name: spec.name, version: spec.version, stdin: spec.transport.stdin, stdout: spec.transport.stdout as never }),
    ),
  )

const Host = { serve: _serve, confirm: _confirm, artifact: _artifactResource }
```

## [6]-[REMOTE]

[REMOTE]:
- Owner: `Remote` — the outbound client lane: `Remote.dial(spec)` acquires the SDK `Client` under `Effect.acquireRelease` (`connect` on acquire, `close` on release — a spawned stdio server or an HTTP session dies with the scope), and every capability is one lifted primitive: `tools` (`listTools` → each remote tool projected into an app-toolkit row — its JSON-Schema `inputSchema` admitted, its hints graded through `Safety.grade`, its handler a `callTool` whose `structuredContent` re-parses through the row's own `effect/Schema`), `read` (`readResource`), `prompt` (`getPrompt`), `complete` (`complete`) — so external servers contribute tools, evidence, and prompt templates through four primitives and the Zod wire never escapes the seam.
- Law: transport is locality — `StdioClientTransport({ command, args, env })` for a local server process (the safe-env allowlist from `getDefaultEnvironment`), `StreamableHTTPClientTransport(url, { authProvider })` for remote with the OAuth provider arriving as an app-passed value composing the security wave's ceremony — no `security` import here.
- Law: every SDK rejection folds to `ToolFault` — `UnauthorizedError` to `declined`, transport failure to `dial`, a re-parse miss to `shape` — and the fault's class column routes retry through the caller's budget exactly like every other rail.
- Growth: a new server is a dial spec value; a new SDK capability (tasks, subscriptions) is one more lifted primitive on this owner.
- Packages: `@modelcontextprotocol/sdk` (`Client`, `StdioClientTransport`, `StreamableHTTPClientTransport`, `OAuthClientProvider`); `effect` (`Scope`, `Effect`, `Schema`).

```typescript
declare namespace Remote {
  type Spec = {
    readonly name: string
    readonly transport:
      | { readonly kind: "stdio"; readonly command: string; readonly args: ReadonlyArray<string> }
      | { readonly kind: "http"; readonly url: URL; readonly auth: Option.Option<OAuthClientProvider> }
  }
}

const _dial = (spec: Remote.Spec) =>
  Effect.acquireRelease(
    Effect.gen(function* () {
      const client = new Client({ name: spec.name, version: "1" })
      const transport = spec.transport.kind === "stdio"
        ? new StdioClientTransport({ command: spec.transport.command, args: [...spec.transport.args] })
        : new StreamableHTTPClientTransport(spec.transport.url, { authProvider: Option.getOrUndefined(spec.transport.auth) })
      yield* Effect.tryPromise({
        try: () => client.connect(transport),
        catch: (cause) => new ToolFault({ reason: "dial", server: spec.name, detail: String(cause) }),
      })
      return client
    }),
    (client) => Effect.promise(() => client.close()),
  )

const _tools = (server: string, client: Client, mode: Safety.Mode) =>
  Effect.tryPromise({
    try: () => client.listTools(),
    catch: (cause) => new ToolFault({ reason: "call", server, detail: String(cause) }),
  }).pipe(
    Effect.map((roster) =>
      roster.tools.map((tool) => ({
        name: tool.name,
        clazz: _grade({
          readOnly: tool.annotations?.readOnlyHint === true,
          destructive: tool.annotations?.destructiveHint !== false,
          idempotent: tool.annotations?.idempotentHint === true,
          openWorld: tool.annotations?.openWorldHint === true,
        }),
        call: (args: Record<string, unknown>) =>
          Effect.tryPromise({
            try: () => client.callTool({ name: tool.name, arguments: args }),
            catch: (cause) => new ToolFault({ reason: "call", server, detail: String(cause) }),
          }),
      }))
    ),
    Effect.map((graded) => ({ graded, admission: _admit(graded, mode) })),
  )

const Remote = {
  dial: _dial,
  tools: _tools,
  read: (server: string, client: Client, uri: string) =>
    Effect.tryPromise({
      try: () => client.readResource({ uri }),
      catch: (cause) => new ToolFault({ reason: "call", server, detail: String(cause) }),
    }),
  prompt: (server: string, client: Client, name: string, args: Record<string, string>) =>
    Effect.tryPromise({
      try: () => client.getPrompt({ name, arguments: args }),
      catch: (cause) => new ToolFault({ reason: "call", server, detail: String(cause) }),
    }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Arsenal, Host, Remote, Safety, ToolFault }
```
