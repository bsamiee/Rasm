# [AI_TOOLKIT]

A tool is one `Schema`-typed declaration and a toolkit is data: `Tool.make` carries parameters, success, failure, failure routing, and dependency Tags in one value; `Toolkit.make`/`Toolkit.merge` assemble collections the way `edge` assembles `HttpApiGroup` families — domain code contributes toolkit values, the app root merges and binds handlers, and the god-toolkit is structurally impossible because the merged value exists only at composition. This page owns the folder's whole tool vocabulary: the declaration law every consumer authors against, the `Arsenal` ledger that types the five providers' provider-defined tool families as one name-keyed table, and the `Safety` vocabulary that classifies every tool into a gate posture — the admission fold `model/provider` and `agent/actor` both read, fail-closed by construction. Tool execution never bypasses safety: the gate restricts what a model may call before any call is made, so admission is structural, not interception.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                              |
| :-----: | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | [DECLARATION] | the tool authoring law — `Tool.make`, `fromTaggedRequest`, annotations, failure routing |
|  [02]   | [ASSEMBLY]    | toolkit collection, handler binding, and the inference channels                      |
|  [03]   | [ARSENAL]     | the provider-defined tool ledger — one name-keyed table over five rosters            |
|  [04]   | [SAFETY]      | the safety-class vocabulary + mode-posture policy the gate and actor consume         |

## [2]-[DECLARATION]

[DECLARATION]:
- Owner: `Tool.make(name, { description?, parameters?, success?, failure?, failureMode?, dependencies? })` — one declaration is the tool's name, its `Schema.Struct.Fields` parameters, its `success`/`failure` Schemas, its failure-routing policy, and the `Context.Tag` list its handler consumes; `Tool.getJsonSchema(tool)` derives the `JsonSchema7` provider wire from the same declaration, so the model-facing contract can never skew from the handler-facing one.
- Law: `failureMode` is the routing decision, fixed at declaration — `"error"` (the default) routes a handler failure onto the typed error channel where `catchTag` recovers; `"return"` keeps the failure in-band as a tool result the model reads, the self-correcting-loop posture `agent/actor` selects for tools whose failure is conversational evidence rather than a program fault.
- Law: a tool that already exists as a `Schema.TaggedRequest` lifts through `Tool.fromTaggedRequest(schema)` — payload, success, and failure arrive from the one request declaration, so a `work` job family or worker protocol row doubles as a tool with zero re-statement; `agent/actor`'s `Act` protocol is the standing consumer.
- Law: `dependencies: [Tag, ...]` threads capability into `Tool.HandlersFor` — the handler's requirements surface on the toolkit's `toLayer` result and travel the requirement channel to the root; a handler reaching for ambient capability instead is the module-level-singleton defect.
- Law: the annotation vocabulary — `Tool.Readonly`, `Tool.Destructive`, `Tool.Idempotent`, `Tool.OpenWorld`, `Tool.Title` — projects one-to-one onto MCP tool hints at the `tool/mcp` host seam; annotations describe, they never enforce — enforcement is `[04]`'s posture table keyed by tool name, so a missing annotation degrades a hint, never an admission.
- Law: `Tool.isUserDefined`/`Tool.isProviderDefined` discriminate the two tool kinds and `Tool.Any` is the erased bound every `Record<string, Tool.Any>` signature keys; `Tool.ProviderDefined<Name, Config, RequiresHandler>` is the brand every provider roster returns — a per-provider tool type is the named defect.
- Reject: a tool declared as a bare function beside a hand-written JSON schema; a `failureMode` decision buried in handler bodies; a tool family split across parallel declarations one `parameters` union carries.
- Packages: `@effect/ai` (`Tool`).

## [3]-[ASSEMBLY]

[ASSEMBLY]:
- Owner: `Toolkit.make(...tools)` collects declarations into one data value keyed by tool name; `Toolkit.merge(...toolkits)` composes contributions with later-wins collision semantics; `Toolkit.empty` seeds the fold — so an app's toolkit is `Toolkit.merge(domainKit, remoteKit, arsenalKit)` at the root, one value, never a registry mutated at import time.
- Law: handler binding is `kit.toLayer(build)` — `build` is the `HandlersFrom<Tools>` record or an `Effect` that constructs it, checked key-by-key against the tool set so a missing or misspelled handler is a compile error at the record; the result is `Layer.Layer<Tool.HandlersFor<Tools>, EX, Exclude<RX, Scope>>`, wired at the root like every capability; `kit.toContext` is the same binding as a `Context` value, and `kit.of(handlers)` is the identity-typed record helper that keeps a staged handler record checked before `toLayer` consumes it.
- Law: the toolkit value is itself an `Effect` — `Toolkit<Tools>` extends `Effect<Toolkit.WithHandler<Tools>, never, Tool.HandlersFor<Tools>>` — so passing the raw toolkit to a generation call threads the handler requirement into `R` automatically; `LanguageModel.ExtractError` and `LanguageModel.ExtractContext` compute the added error and context channels from the options shape, which is why a tool-augmented call's full fault surface is readable from its annotation with zero hand-stated unions.
- Law: growth is a row — a new capability is one `Tool.make` declaration plus one handler record entry; every generation call over the merged toolkit picks it up with no signature edit, and the compile error at the handler record is the completeness proof.
- Boundary: which tools a given call may actually invoke is `[04]`'s posture fold applied by `model/provider`'s gate through `toolChoice` — assembly owns collection, never admission.
- Packages: `@effect/ai` (`Toolkit`, `Tool`).

## [4]-[ARSENAL]

[ARSENAL]:
- Owner: the interior `_arsenal` table — every provider-defined tool tag across the five rosters as one name-keyed row set, columns `family` (the owning provider lens), `executor` (`"provider"` runs it upstream, `"app"` demands a bound handler — the `requiresHandler` asymmetry as data), and `grade` (the `[04]` safety class). `Arsenal.grade(name)` is the one lookup the safety fold consults for provider-defined tools.
- Law: the ledger keys by tool TAG, not by package — the Bedrock roster's eight constructors mint the same `Anthropic*` tags as the Anthropic package (Claude-on-Bedrock reuses the family), so one row serves both providers and the table cannot fork per transport.
- Law: `executor: "app"` rows (`AnthropicBash`, `AnthropicComputerUse`, `AnthropicTextEditor`) are local-execution tools whose handlers bind through `toLayer` exactly like user tools; `executor: "provider"` rows run upstream and their results arrive as response parts — the column is what tells `agent/actor` which rows need a handler record entry before the toolkit compiles.
- Law: grades are assigned from the catalogued semantics and fail conservative — shell and computer control are `destroy`, file editing and code execution are `write`, retrieval-only search families are `search`, vector-store file search is `read`; an app overrides a grade only through its own `[04]` table, never by editing the ledger reflexively.
- Growth: a provider ships a new tool — one row; a sixth provider roster — its rows join the same table under the same three columns.
- Packages: `@effect/ai-openai` (`OpenAiTool`), `@effect/ai-anthropic` (`AnthropicTool`), `@effect/ai-google` (`GoogleTool`), `@effect/ai-amazon-bedrock` (`AmazonBedrockTool`) — named here as roster sources; the table carries their tags as data and imports none of them.

```typescript
import { Option } from "effect"

const _arsenal = {
  AnthropicBash: { family: "anthropic", executor: "app", grade: "destroy" },
  AnthropicCodeExecution: { family: "anthropic", executor: "provider", grade: "write" },
  AnthropicComputerUse: { family: "anthropic", executor: "app", grade: "destroy" },
  AnthropicTextEditor: { family: "anthropic", executor: "app", grade: "write" },
  AnthropicWebSearch: { family: "anthropic", executor: "provider", grade: "search" },
  GoogleCodeExecution: { family: "google", executor: "provider", grade: "write" },
  GoogleSearch: { family: "google", executor: "provider", grade: "search" },
  GoogleSearchRetrieval: { family: "google", executor: "provider", grade: "search" },
  GoogleUrlContext: { family: "google", executor: "provider", grade: "search" },
  OpenAiCodeInterpreter: { family: "openai", executor: "provider", grade: "write" },
  OpenAiFileSearch: { family: "openai", executor: "provider", grade: "read" },
  OpenAiWebSearch: { family: "openai", executor: "provider", grade: "search" },
  OpenAiWebSearchPreview: { family: "openai", executor: "provider", grade: "search" },
} as const

declare namespace Arsenal {
  type Name = keyof typeof _arsenal
  type Family = (typeof _arsenal)[Name]["family"]
  type Executor = (typeof _arsenal)[Name]["executor"]
  type Row = { readonly family: Family; readonly executor: "app" | "provider"; readonly grade: Safety.Class }
  type Shape = typeof _arsenal & { readonly grade: (name: string) => Option.Option<Safety.Class> }
  type _Rows<T extends Record<Name, Row> = typeof _arsenal> = T
  type _Keys<K extends Name = keyof typeof _arsenal> = K
}

const _graded: Readonly<Record<string, Arsenal.Row>> = _arsenal

const Arsenal: Arsenal.Shape = {
  ..._arsenal,
  grade: (name) => Option.map(Option.fromNullable(_graded[name]), (row) => row.grade),
}
```

## [5]-[SAFETY]

[SAFETY]:
- Owner: the `Safety` vocabulary — four closed classes ordered by blast radius (`read` observes closed material, `search` reaches the open world, `write` mutates reversibly, `destroy` mutates irreversibly or executes arbitrarily) and three postures (`auto` runs unattended, `confirm` demands an approval turn, `deny` never runs); `_modes` maps each agent mode to a full class-to-posture row, so an operating posture is one policy value.
- Law: classification is declared, never inferred — an app assigns classes to its own tools through a `Safety.Table` it authors beside its toolkit; provider-defined tools resolve through `Arsenal.grade`; a name absent from both resolves `destroy` — the fail-closed default that makes an unregistered tool inert rather than trusted.
- Law: `Safety.admit(mode, table, names)` partitions a toolkit's name set into the allowed and held lists under the mode's posture row — `auto` admits, `confirm` and `deny` hold — and the gate turns the split into `toolChoice`: a non-empty allowed list becomes `{ oneOf: allowed }`, an empty one becomes `"none"`, so a model can never name a tool the mode withholds; `confirm`-class rows re-enter only through `agent/actor`'s supervised pause, never through a widened admission.
- Law: mode rows are total over classes — the `_Rows` guard proves every mode maps every class, so adding a class breaks every mode row loudly at the declaration.
- Boundary: the posture fold's application site is `model/provider`'s gate; the supervised approval turn is `agent/actor`'s; this page owns the vocabulary and the partition.
- Growth: a new operating mode is one `_modes` row; a new blast-radius class is one `_classes` entry plus the compile-forced column in every mode row.
- Packages: `effect` (`Array`, `Option`, `Record`).

```typescript
import { Array, Option } from "effect"

const _classes = ["read", "search", "write", "destroy"] as const
const _postures = ["auto", "confirm", "deny"] as const

const _modes = {
  autonomous: { read: "auto", search: "auto", write: "confirm", destroy: "deny" },
  supervised: { read: "auto", search: "confirm", write: "confirm", destroy: "confirm" },
  sealed: { read: "auto", search: "deny", write: "deny", destroy: "deny" },
} as const

declare namespace Safety {
  type Class = (typeof _classes)[number]
  type Posture = (typeof _postures)[number]
  type Mode = keyof typeof _modes
  type Table = Readonly<Record<string, Class>>
  type Split<N extends string = string> = { readonly allowed: ReadonlyArray<N>; readonly held: ReadonlyArray<N> }
  type Shape = typeof _modes & {
    readonly classes: typeof _classes
    readonly classed: (table: Table, name: string) => Class
    readonly posture: (mode: Mode, grade: Class) => Posture
    readonly admit: <N extends string>(mode: Mode, table: Table, names: ReadonlyArray<N>) => Split<N>
  }
  type _Rows<T extends Record<Mode, Record<Class, Posture>> = typeof _modes> = T
  type _Keys<K extends Mode = keyof typeof _modes> = K
}

const Safety: Safety.Shape = {
  ..._modes,
  classes: _classes,
  classed: (table, name) =>
    Option.getOrElse(
      Option.orElse(Option.fromNullable(table[name]), () => Arsenal.grade(name)),
      () => "destroy" as const,
    ),
  posture: (mode, grade) => _modes[mode][grade],
  admit: (mode, table, names) => {
    const [held, allowed] = Array.partition(names, (name) => Safety.posture(mode, Safety.classed(table, name)) === "auto")
    return { allowed, held }
  },
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Arsenal, Safety }
```
