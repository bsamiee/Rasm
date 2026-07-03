# [API_CATALOGUE] @pulumi/pulumi

`@pulumi/pulumi` is the core Pulumi SDK and the anchor every `@pulumi/*` sibling composes against: the `Output<T>`/`Input<T>` async-value algebra with its combinators (`output`/`secret`/`all`/`concat`/`interpolate`/`jsonStringify`/`jsonParse`/`deferredOutput`), the resource model (`Resource` → `CustomResource`/`ComponentResource`/`ProviderResource`, `StackReference`, lifecycle `ResourceHook`/`ErrorHook`), the resource-options family, the typed `Config` reader (a `{get,require}×{string,Boolean,Number,Object}` matrix with a secret arm), the typed error classes, the top-level `asset`/`log`/`runtime` namespaces, and the subpath modules `@pulumi/pulumi/automation`, `/provider`, `/dynamic`, `/queryable`. The Automation API (`LocalWorkspace`/`RemoteWorkspace`/`Stack`) drives up/preview/refresh/destroy programmatically, and its engine-event stream (`EngineEvent`, `StepEventMetadata`, `OpType`, `OpMap`, `DiffKind`, `PropertyDiff`, `previewRefresh`) is the source the deploy and drift drivers fold in the services deploy tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumi`
- package: `@pulumi/pulumi` (3.250.0, Apache-2.0, © Pulumi Corporation)
- module format: CommonJS with generated `.d.ts`; the main entry re-exports the output algebra, resource model, `Config`, error classes, and the `asset`/`log`/`runtime` namespaces; `@pulumi/pulumi/automation` (programmatic lifecycle) + `/provider`/`/dynamic`/`/queryable` are subpath modules
- runtime target: node deploy-time only — reachable through the `./provisioning` (`iac`) subpath so the `@pulumi/*` closure never enters the durable hot path; the deploy driver and the drift fold consume it, never browser-reachable
- surface: the `Output<T>`/`Input<T>` algebra + combinators, the `Resource` → `CustomResource`/`ComponentResource`/`ProviderResource` model, `Config`, `StackReference`, the typed error rails, and the `@pulumi/pulumi/automation` `LocalWorkspace`/`Stack` + `EngineEvent` stream
- consumer: `provisioning/contract#PROVISIONING` deploy driver + `provisioning/drift#PROVISIONING` drift fold; every `@pulumi/*` sibling (`.api/pulumi-{policy,random,command,docker,kubernetes,aws,awsx}.md`) composes its `Output`/`Input` algebra and `CustomResourceOptions`
- rail: deployment

The main entry carries the output algebra, resource model, config, and errors; `asset`/`log`/`runtime` are re-exported namespaces; `automation` (`@pulumi/pulumi/automation`) is the programmatic lifecycle module; `provider`/`dynamic`/`queryable` are the provider-authoring and resolved-resource subpaths.

| [INDEX] | [MODULE]                     | [SURFACE]                                                                                | [CAPABILITY]                                  |
| :-----: | :--------------------------- | :-------------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `@pulumi/pulumi`             | `Output`/`Input`, resource classes, `Config`, error classes, output combinators          | core deployment SDK                           |
|  [02]   | `@pulumi/pulumi/automation`  | `LocalWorkspace`, `RemoteWorkspace`, `Stack`, result types, `EngineEvent` stream          | programmatic up/preview/refresh/destroy       |
|  [03]   | `asset` / `log` / `runtime`  | `asset.Asset`/`Archive`, structured logging, `runtime` invoke/settings                    | re-exported namespaces                         |
|  [04]   | `provider` / `dynamic` / `queryable` | provider host, dynamic-resource authoring, `ResolvedResource<T>` snapshot         | provider-authoring + resolved-resource subpaths |

## [02]-[OUTPUT_ALGEBRA]

`Output<T>` is `OutputInstance<T> & Lifted<T>` — property access on `Output<{a: string}>` yields `Output<string>` directly, no `.apply`. `Input<T>` is `T | Promise<T> | OutputInstance<T>`; every resource arg field accepts it. The combinators are the join/serialize surface; `Output` (the value export) carries the statics.

| [INDEX] | [SYMBOL]                                                        | [KIND]           | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `Output<T> = OutputInstance<T> & Lifted<T>`                     | type alias       | async value; property-lifted                                      |
|  [02]   | `Input<T> = T \| Promise<T> \| OutputInstance<T>`               | type alias       | all resource arg fields accept it                                 |
|  [03]   | `Inputs` / `Unwrap<T>` / `Lifted<T>`                           | utility types    | `Record<string, Input<any>>` / deep-unwrap / property projection  |
|  [04]   | `output<T>(val: Input<T>): Output<Unwrap<T>>`                   | lift             | wrap plain/promised value (undefined-tolerant overload)           |
|  [05]   | `secret<T>(val): Output<Unwrap<T>>` · `unsecret<T>(o): Output<T>` · `isSecret<T>(o): Promise<boolean>` | secret ops | mark / strip / test the secret flag                               |
|  [06]   | `all(vals): Output<...>`                                        | combinator       | join heterogeneous inputs; record + tuple(2–8) + array overloads — pick the narrowest to keep tuple element types |
|  [07]   | `concat(...params: Input<any>[]): Output<string>`              | string combinator| stringify-and-concatenate                                         |
|  [08]   | `interpolate(literals, ...placeholders): Output<string>`       | tagged template  | template-literal output interpolation                             |
|  [09]   | `jsonStringify(obj: Input<any>, replacer?, space?): Output<string>` · `jsonParse(text: Input<string>, reviver?): Output<any>` | serde | `JSON.stringify`/`parse` over inputs                              |
|  [10]   | `deferredOutput<T>(): [Output<T>, (source: Output<T>) => void]` | deferred factory | output + resolver pair for cyclic/late-bound wiring               |
|  [11]   | `Output.create<T>(val): Output<Unwrap<T>>` · `Output.isInstance(obj): obj is Output<T>` | statics (via `OutputConstructor`) | construct / runtime type-guard                                    |
|  [12]   | `output.apply<U>(fn): Output<U>` · `output.get(): T`           | instance methods | dependency-tracked transform (3 overloads) / cloud-runtime unwrap |
|  [13]   | `getAllResources<T>(o: OutputInstance<T>): Promise<Set<Resource>>` | introspection   | the resource dependency set of an output                          |

## [03]-[RESOURCE_MODEL]

`Resource` is the base (holds `urn: Output<URN>`); `CustomResource` adds `id: Output<ID>`; `ComponentResource` is the logical-grouping owner; `ProviderResource` is the explicit provider. `StackReference` reads another stack's outputs. `ResourceHook`/`ErrorHook` bind lifecycle callbacks. The options family threads parent/ordering/protection/aliasing.

| [INDEX] | [SYMBOL]                                                        | [KIND]           | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `Resource` / `CustomResource` / `ProviderResource`             | (abstract) classes | base `urn` / leaf `id` / CRUD provider (`ProviderResource extends CustomResource`) |
|  [02]   | `new ComponentResource(type, name, args?, opts?)` · `registerOutputs(outputs?)` | class + method | logical grouping owner; declare its output map (`protected registerOutputs`) |
|  [03]   | `createUrn(name, type, parent?, project?, stack?): Output<URN>` | utility          | compute a resource URN                                            |
|  [04]   | `ResourceHook` / `ErrorHook`                                    | classes          | lifecycle-hook / error-hook bindings (`isInstance` guarded)       |
|  [05]   | `new StackReference(name, args?, opts?)`                        | class ctor       | cross-stack reader (`extends CustomResource`; a tracked resource) |
|  [06]   | `sr.getOutput(name: Input<string>): Output<any>` · `sr.requireOutput(name): Output<any>` | methods | fetch output-or-`undefined` / fetch-or-throw                     |
|  [07]   | `sr.getOutputDetails(name: string): Promise<StackReferenceOutputDetails>` · `sr.requireOutputValue(name): Promise<any>` | methods | `{ value?, secretValue? }` (value `null` when secret/absent) / plain value-or-throw |
|  [08]   | `ResourceOptions` / `CustomResourceOptions` / `ComponentResourceOptions` | interfaces | `parent`/`dependsOn`/`protect`/`ignoreChanges`/`aliases`/`customTimeouts` (+ `import`/`id`, + `providers`) |
|  [09]   | `CustomTimeouts` / `Alias`                                      | interfaces       | `create`/`update`/`delete`/`read` timeout strings / prior-name descriptor |

## [04]-[CONFIG]

`Config` scopes by project name by default; pass a custom name to read a separate namespace. The read surface is a `{get, require} × {string, Boolean, Number, Object<T>}` matrix, each with a `Secret` variant (except `Object`) that returns an `Output` carrying the secret mark. `get*` returns `| undefined`; `require*` throws when absent.

| [INDEX] | [SURFACE]                                                                 | [FAMILY]      | [CAPABILITY]                                                |
| :-----: | :------------------------------------------------------------------------ | :------------ | :--------------------------------------------------------- |
|  [01]   | `new Config(name?)`                                                        | ctor          | project-scoped config bag                                  |
|  [02]   | `get(key, opts?: StringConfigOptions<K>): K \| undefined` · `require(key, opts?): K` | string        | plain string read                                          |
|  [03]   | `getSecret(key, opts?): Output<K> \| undefined` · `requireSecret(key, opts?): Output<K>` | string secret | secret-marked `Output`                                     |
|  [04]   | `getBoolean(key): boolean \| undefined` · `requireBoolean(key): boolean`   | boolean       | typed boolean                                              |
|  [05]   | `getSecretBoolean(key): Output<boolean> \| undefined` · `requireSecretBoolean(key): Output<boolean>` | boolean secret | secret-marked                                             |
|  [06]   | `getNumber(key, opts?: NumberConfigOptions): number \| undefined` · `requireNumber(key, opts?): number` | number        | typed number                                              |
|  [07]   | `getSecretNumber(key, opts?): Output<number> \| undefined` · `requireSecretNumber(key, opts?): Output<number>` | number secret | secret-marked                                             |
|  [08]   | `getObject<T>(key): T \| undefined` · `requireObject<T>(key): T`           | object        | JSON-deserialized object (no secret-object variant)        |
|  [09]   | `StringConfigOptions<K>` / `NumberConfigOptions`                           | option types  | `allowedValues`/`minLength`/`maxLength`/`pattern` · `min`/`max` bounds |

## [05]-[ERRORS]

Typed error classes for clean deployment aborts. `RunError` aborts without a stack trace; the property errors carry structured validation detail for a boundary decode.

| [INDEX] | [SYMBOL]               | [CAPABILITY]                                              |
| :-----: | :--------------------- | :------------------------------------------------------- |
|  [01]   | `RunError`             | clean abort, no stack trace printed                      |
|  [02]   | `ResourceError`        | resource-associated abort (carries the resource)         |
|  [03]   | `InputPropertyError`   | single input-property validation failure                 |
|  [04]   | `InputPropertiesError` | multi-property validation failure                        |

## [06]-[AUTOMATION_API]

`@pulumi/pulumi/automation` drives the lifecycle in-process. `LocalWorkspace` backs inline (`InlineProgramArgs`: `{ stackName, projectName, program: PulumiFn }`) or on-disk (`LocalProgramArgs`: `{ stackName, workDir }`) programs; `RemoteWorkspace` drives Pulumi Cloud deployments. `Stack` owns the four lifecycle verbs plus `previewRefresh`, config, and outputs.

| [INDEX] | [SURFACE]                                                                       | [FAMILY]         | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------------------------ | :--------------- | :------------------------------------------------- |
|  [01]   | `LocalWorkspace.create(opts: LocalWorkspaceOptions): Promise<LocalWorkspace>`    | factory          | workspace from options                             |
|  [02]   | `LocalWorkspace.{create,select,createOrSelect}Stack(args, opts?): Promise<Stack>` | factories        | `LocalProgramArgs`/`InlineProgramArgs` overloads   |
|  [03]   | `Stack.{create,select,createOrSelect}(name, workspace): Promise<Stack>`          | static factories | construct from an existing workspace               |
|  [04]   | `Stack.up(opts?: UpOptions): Promise<UpResult>`                                  | lifecycle        | deploy/update; `UpResult` = summary + `outputs: OutputMap` |
|  [05]   | `Stack.preview(opts?: PreviewOptions): Promise<PreviewResult>`                   | lifecycle        | dry-run; `changeSummary: OpMap`                     |
|  [06]   | `Stack.refresh(opts?: RefreshOptions): Promise<RefreshResult>`                   | lifecycle        | reconcile state (mutating)                          |
|  [07]   | `Stack.destroy(opts?: DestroyOptions): Promise<DestroyResult>`                   | lifecycle        | delete all resources                               |
|  [08]   | `Stack.previewRefresh(opts?: RefreshOptions): Promise<PreviewResult>`            | lifecycle        | read-only refresh-preview (the drift source, [07])  |
|  [09]   | `Stack.outputs(): Promise<OutputMap>` · `Stack.getAllConfig(): Promise<ConfigMap>` · `Stack.setConfig(key, value: ConfigValue, path?): Promise<void>` | query/config | current outputs / full config / set one key         |
|  [10]   | `PulumiFn = () => Promise<Record<string, any> \| void>`                          | program type     | return-object keys become stack outputs            |
|  [11]   | `OutputMap = Record<string, OutputValue>` · `OutputValue = { value: any; secret: boolean }` | result types | the stack output map                               |

## [07]-[ENGINE_EVENT_STREAM]

Every lifecycle `opts.onEvent` (`UpOptions`/`PreviewOptions`/`RefreshOptions`/`DestroyOptions`) delivers a discriminated `EngineEvent` per engine step — exactly one event field is non-nil. `previewRefresh({ onEvent })` re-reads provider state read-only and streams `resourcePreEvent` steps without mutating the stack, where `StepEventMetadata.op` (`OpType`) classifies the divergence and `detailedDiff: Record<string, PropertyDiff>` carries the per-property delta. `PreviewResult.changeSummary` (`OpMap`) is the engine's own per-`OpType` count to reconcile a folded bucket count against.

| [INDEX] | [SYMBOL]                                              | [KIND]        | [CAPABILITY]                                                    |
| :-----: | :--------------------------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `EngineEvent`                                        | interface     | event sum; `sequence`/`timestamp` + exactly one event field    |
|  [02]   | `ResourcePreEvent` / `ResOutputsEvent`               | interfaces    | `metadata: StepEventMetadata` before / after a step            |
|  [03]   | `ResOpFailedEvent`                                    | interface     | `metadata` + `status`/`steps`; step failure                    |
|  [04]   | `StepEventMetadata`                                   | interface     | `op`/`urn`/`type`/`detailedDiff`/`old`/`new`/`keys`/`diffs`/`provider` |
|  [05]   | `StepEventStateMetadata`                              | interface     | per-resource old/new state (`inputs`/`outputs`/`protect`/`id`…)  |
|  [06]   | `OpType`                                              | string union  | 15-member CRUD-op vocabulary (drift classifier key)            |
|  [07]   | `OpMap = { [op in OpType]?: number }`                | mapped type   | per-op count; `SummaryEvent.resourceChanges` / `PreviewResult.changeSummary` |
|  [08]   | `DiffKind`                                            | enum          | 6-member property-diff kind (runtime enum → `Schema.Enums`)     |
|  [09]   | `PropertyDiff`                                        | interface     | `diffKind: DiffKind`; `inputDiff: boolean`                     |
|  [10]   | `SummaryEvent` / `DiagnosticEvent` / `PolicyEvent`   | interfaces    | end-of-update summary / provider diagnostic / CrossGuard violation |
|  [11]   | `StdoutEngineEvent` / `PreludeEvent` / `CancelEvent` / `StartDebuggingEvent` | interfaces/types | stdout message / config prelude / cancellation / DAP-attach event |

```ts contract
// @pulumi/pulumi/automation — events.d.ts + stack.d.ts (drift-fold surface)
export declare type OpType =
  | "same" | "create" | "update" | "delete" | "replace"
  | "create-replacement" | "delete-replaced" | "read" | "read-replacement"
  | "refresh" | "discard" | "discard-replaced" | "remove-pending-replace"
  | "import" | "import-replacement"
export declare type OpMap = { [key in OpType]?: number }

export declare enum DiffKind {
  add = "add", addReplace = "add-replace", delete = "delete",
  deleteReplace = "delete-replace", update = "update", updateReplace = "update-replace"
}
export interface PropertyDiff { diffKind: DiffKind; inputDiff: boolean }

export interface StepEventMetadata {
  op: OpType; urn: string; type: string
  old?: StepEventStateMetadata; new?: StepEventStateMetadata
  keys?: string[]; diffs?: string[]
  detailedDiff?: Record<string, PropertyDiff>
  logical?: boolean; provider: string
}
export interface SummaryEvent {
  maybeCorrupt: boolean; durationSeconds: number
  resourceChanges: OpMap; policyPacks: Record<string, string>   // PolicyPackName -> version
}
export interface DiagnosticEvent {
  urn?: string; prefix?: string; message: string; color: string
  severity: "info" | "info#err" | "warning" | "error"; streamID?: number; ephemeral?: boolean
}
export interface PolicyEvent {
  resourceUrn?: string; message: string; color: string
  policyName: string; policyPackName: string; policyPackVersion: string
  policyPackVersionTag: string; enforcementLevel: "warning" | "mandatory"
}
export declare type CancelEvent = {}
export interface StdoutEngineEvent { message: string; color: string }
export interface PreludeEvent { config: Record<string, string> }
export interface StartDebuggingEvent { config: Record<string, any> }
export interface ResourcePreEvent { metadata: StepEventMetadata; planning?: boolean }

export interface EngineEvent {
  sequence: number; timestamp: number
  cancelEvent?: CancelEvent; stdoutEvent?: StdoutEngineEvent
  diagnosticEvent?: DiagnosticEvent; preludeEvent?: PreludeEvent
  summaryEvent?: SummaryEvent; resourcePreEvent?: ResourcePreEvent
  resOutputsEvent?: ResOutputsEvent; resOpFailedEvent?: ResOpFailedEvent
  policyEvent?: PolicyEvent; startDebuggingEvent?: StartDebuggingEvent
}

// PreviewResult (up/preview/previewRefresh) — RefreshOptions.onEvent is the per-step stream
export interface PreviewResult { stdout: string; stderr: string; changeSummary: OpMap }
interface RefreshOptions {
  onEvent?: (event: EngineEvent) => void; onOutput?: (out: string) => void
  // + parallel/message/target/expectNoChanges/userAgent/color (shared option fields)
}
```

`previewRefresh` is the non-mutating refresh-preview: it re-reads provider state and reports each resource's drift as a stream of `resourcePreEvent` steps — `StepEventMetadata.op` classifies the divergence, `detailedDiff` carries the per-property delta — without writing the refreshed state back. Drive it with an `onEvent` accumulator and reconcile the folded bucket count against `PreviewResult.changeSummary`.

## [08]-[IMPLEMENTATION_LAW]

[OUTPUT_TOPOLOGY]:
- `Output<T>` is `OutputInstance<T> & Lifted<T>`; property access on `Output<{a: string}>` returns `Output<string>` directly without `.apply`.
- `Input<T>` is `T | Promise<T> | OutputInstance<T>`; every resource arg field accepts `Input<T>`.
- `all()` overloads cover record, tuple (2–8), and array shapes; pick the narrowest to preserve tuple element types.
- `apply()` receives the unwrapped `T` only during `up`/`preview`, and may not be called at all during preview when the value is unknown; `get()` is valid only inside cloud-runtime code — calling it during deployment planning throws.
- `deferredOutput()` returns `[Output<T>, resolver]` for late-bound/cyclic wiring; resolve exactly once.

[AUTOMATION_TOPOLOGY]:
- `LocalWorkspace` backs inline or local-dir programs; `RemoteWorkspace` drives Pulumi Cloud. `PulumiFn` return-object keys become stack outputs.
- `UpOptions.onOutput`/`onEvent` deliver streaming callbacks for CI progress; `StackReference` is a tracked resource (consumes a destroy op).

[DEPLOY_STACK]: how `provisioning/contract#PROVISIONING` composes this core onto the Effect rails.
- `TierStack.tier(kind, mode): pulumi.ComponentResource` builds each tier as a `ComponentResource`, instantiating children with `{ parent: this }` (`ComponentResourceOptions`) so they join its URN tree, and surfacing values through `registerOutputs`; the bootstrap orders steps with `ResourceOptions.dependsOn`, never shell chaining.
- `AutomationDriver` wraps the Automation API in Effect: `workspace: Effect<LocalWorkspace, never, Scope>` and `stack(name): Effect<Stack, never, Scope>` acquire the workspace/stack as scoped resources, and `run(verb, stackName): Effect<OutputMap, AutomationFault>` drives one lifecycle verb through `Effect.tryPromise` (each `Stack.up/preview/refresh/destroy` is a `Promise`), folding a `Stack.outputs()` `OutputMap` — bound as one typed `@effect/cli` command tree, never a bare CLI.
- `StackOutputs.reference: StackReference` reads this tier's OWN provisioned topology; `output<K>(key): Effect<StackOutputShape[K], ConfigError>` projects one typed reference row (a DSN as `Redacted`, the collector OTLP endpoint) via `requireOutput`/`getOutputDetails`.
- `SecretResolver` composes `Config.requireSecret`/`pulumi.secret()` `Output`s so deploy-time secrets stay redacted in state; a machine-generated secret arrives from `@pulumi/random` `RandomPassword.result` (`pulumi-random.md`) already secret-marked.
- `PolicyGuard.pack: policy.PolicyPack` (`pulumi-policy.md`) rides the same engine: a `mandatory` violation surfaces as a `PolicyEvent` on the `EngineEvent` stream and folds into `SummaryEvent.policyPacks`.

[DRIFT_STACK]: how `provisioning/drift#PROVISIONING` folds the event stream (`pulumi-pulumi.md` automation module only).
- `computeStackDrift` drives `stack.previewRefresh({ onEvent })` bridged into `Effect.async`; the `onEvent` accumulator reads `event.resourcePreEvent?.metadata` and folds each `StepEventMetadata` by `op` (`OpType`) into a per-URN `DriftResource` `Data.TaggedEnum` (create/import → Added; delete/delete-replaced/remove-pending-replace/discard/discard-replaced → Removed; update/replace/create-replacement/import-replacement/read-replacement → Changed with the `detailedDiff` `PropertyDiff` path/`diffKind`/`inputDiff` delta; same/refresh/read → Unchanged), the 15-member `OpType` union making the `Match.exhaustive` fold total.
- `DiffKind` is imported as a runtime enum value (`Schema.Enums(DiffKind)` in the `StackDriftSummary` receipt), not a type-only import.
- The folded bucket count reconciles against `PreviewResult.changeSummary` (`OpMap`) — `Object.entries` over it as `[OpType, number]`, minus the no-drift ops (`same`/`refresh`/`read`) — so the receipt is self-validating; only primitive URN/type/property-path strings cross the subpath boundary, no `@pulumi/*` type escapes onto the hot path.

[SIBLING_STACK]:
- `@pulumi/policy` (`pulumi-policy.md`) resolves its `isType`/`asType`/`*OfType` guards against this core's `Resource`/`Unwrap`/`queryable.ResolvedResource<T>`, and its enforcement rides the automation `PolicyEvent`/`SummaryEvent.policyPacks`.
- `@pulumi/random` (`pulumi-random.md`), `@pulumi/command` (`pulumi-command.md`), `@pulumi/docker`, `@pulumi/kubernetes`, `@pulumi/aws`/`@pulumi/awsx` are all `CustomResource`/`ComponentResource` families over this `Output`/`Input` algebra and `CustomResourceOptions`; every inter-resource value flows as `Output<T>`.
- `effect` (`libs/typescript/.api/effect.md`) owns the `Match.exhaustive` `DeployMode` dispatch, the `Effect.tryPromise`/`Effect.async` bridges over the Automation API `Promise`s, and the `Config`/`Redacted`/`Schema.Class` receipts the driver and drift fold return.

[RAIL_LAW]:
- Package: `@pulumi/pulumi`
- Owns: the output/input algebra, resource model, config, stack references, the Automation API, the engine-event stream, and the typed error rails.
- Accept: `Output<T>` for all inter-resource value flow; `Config.requireSecret` / `secret()` for sensitive values; the Automation API under `Effect.tryPromise`/`Effect.async`; `previewRefresh` for read-only drift; `all`/`apply`/`interpolate` for output composition.
- Reject: direct promise chaining across resource boundaries without `Output.apply`; `Config.get` for secrets; `Output.get()` during deployment planning; a stdout scrape where the typed `EngineEvent`/`PreviewResult.changeSummary` stream carries the fact.
