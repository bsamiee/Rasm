# [TS_IAC_API_PULUMI_PULUMI]

`@pulumi/pulumi` is the deploy-plane engine: the `Output<T>`/`Input<T>` async-dependency algebra, the `Resource`/`ComponentResource` model the `stack` tiers extend, `Config`/`StackReference` state access, and the full Automation API (`LocalWorkspace` inline programs, `Stack` up|preview|refresh|destroy) whose `EngineEvent` stream and `OpType`/`OpMap` deltas feed the typed run-receipt ledger. `iac` composes it as ONE Effect rail — a `Match.exhaustive` ledger dispatch over `Effect.async`-wrapped `Stack` methods whose `AbortSignal` is Effect interruption, whose `onEvent` callback is an Effect `Stream`, and whose `OutputMap` decodes through `Schema` into StackOutputs. No `Pulumi.yaml`, no CLI shell-out — the pulumi CLI-binary is one `PulumiCommand` deploy-host fact and the self-managed state backend is one `ProjectSettings.backend.url`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumi`
- package: `@pulumi/pulumi`
- module: `@pulumi/pulumi` (core), `@pulumi/pulumi/automation` (programmatic lifecycle), `@pulumi/pulumi/{asset,log,dynamic,provider,runtime,iterable}` (submodules)
- installed: `3.250.0`
- license: `Apache-2.0`
- asset: output algebra, resource model, config, stack references, error rails, Automation API, engine-event stream
- runtime: `node` — the `pulumi` CLI binary is a deploy-host fact wrapped once via `PulumiCommand`/`LocalWorkspaceOptions.pulumiCommand`; provider plugins auto-download on first resource registration
- rail: deployment

## [02]-[OUTPUT_ALGEBRA]

The async-dependency monad every resource arg and output flows through. `Output<T>` is `OutputInstance<T> & Lifted<T>` — property access on `Output<{a:string}>` yields `Output<string>` without `.apply`. All lifting is `output`/`secret`; all joining is `all` (pick the tuple overload to preserve element types); all string shaping is `interpolate`/`concat`; serialization is `jsonStringify`/`jsonParse`.

[PUBLIC_TYPE_SCOPE]: output types
- rail: deployment

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [NOTE]                                     |
| :-----: | :------------------ | :------------ | :----------------------------------------- |
|  [01]   | `Output<T>`         | async value   | `OutputInstance<T> & Lifted<T>`            |
|  [02]   | `OutputInstance<T>` | interface     | `.apply`/`.get` contract                   |
|  [03]   | `Input<T>`          | alias         | `T \| Promise<T> \| OutputInstance<T>`     |
|  [04]   | `Inputs`            | alias         | `Record<string, Input<any>>`               |
|  [05]   | `Unwrap<T>`         | utility       | deeply strip `Promise`/`Output` nesting    |
|  [06]   | `Lifted<T>`         | utility       | property-lifted `Output` projection        |
|  [07]   | `Unknown`           | sentinel      | preview-time unresolved marker (`unknown`) |

[ENTRYPOINT_SCOPE]: output combinators
- rail: deployment

| [INDEX] | [SURFACE]                                            | [FAMILY]         | [NOTE]                                       |
| :-----: | :--------------------------------------------------- | :--------------- | :------------------------------------------- |
|  [01]   | `output<T>(val: Input<T>): Output<Unwrap<T>>`        | lift             | plain/promised → tracked output              |
|  [02]   | `secret<T>(val: Input<T>): Output<Unwrap<T>>`        | lift             | mark output sensitive (state-encrypted)      |
|  [03]   | `unsecret<T>(val: Output<T>): Output<T>`             | strip            | remove secret marking                        |
|  [04]   | `isSecret<T>(val: Output<T>): Promise<boolean>`      | predicate        | test secret flag                             |
|  [05]   | `all(vals): Output<...>`                             | combinator       | join record / tuple(2..8) / array of inputs  |
|  [06]   | `concat(...params: Input<any>[]): Output<string>`    | string join      | stringify + concatenate                      |
|  [07]   | `interpolate(literals, ...ph): Output<string>`       | tagged template  | template-literal output interpolation        |
|  [08]   | `jsonStringify(obj, replacer?, space?): Output<str>` | serializer       | `JSON.stringify` over `Input<any>`           |
|  [09]   | `jsonParse(text, reviver?): Output<any>`             | deserializer     | `JSON.parse` over `Input<string>`            |
|  [10]   | `recover<T>(o, (err)=>Input<T>): Output<T>`          | fallback         | recover a rejected output to a value         |
|  [11]   | `deferredOutput<T>(): [Output<T>, resolver]`         | deferred pair    | forward-reference an output before its source |
|  [12]   | `getAllResources<T>(op): Promise<Set<Resource>>`     | dependency query | resources an output depends on               |
|  [13]   | `isUnknown(v)` / `containsUnknowns(v)`               | preview guard    | detect preview-time unresolved values        |
|  [14]   | `Output.create` / `Output.isInstance` / `Output.all` | static           | factory / type guard / static join           |

## [03]-[RESOURCE_MODEL]

The class hierarchy the `stack` ComponentResource tiers extend and the `kube`/`secret`/`observe` rows instantiate. `ComponentResource` is the grouping owner every tier subclasses (call `registerOutputs` last); `parent`/`dependsOn`/`protect`/`ignoreChanges` on the option interfaces build the ownership DAG; `mergeOptions` folds a base option bag into per-resource overrides so a tier passes ONE inherited `opts` down its children.

[PUBLIC_TYPE_SCOPE]: resource classes + options
- rail: deployment

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]  | [NOTE]                                            |
| :-----: | :------------------------------------ | :------------- | :----------------------------------------------- |
|  [01]   | `Resource`                            | abstract class | base; `urn: Output<URN>`                          |
|  [02]   | `CustomResource`                      | abstract class | extends `Resource`; `id: Output<ID>`             |
|  [03]   | `ComponentResource<TData>`            | class          | grouping owner; `registerOutputs(outputs?)`      |
|  [04]   | `ProviderResource`                    | abstract class | explicit provider instance (multi-region/multi-cluster) |
|  [05]   | `StackReference`                      | class          | cross-stack reads; `getOutput`/`requireOutput`/`getOutputDetails` |
|  [06]   | `ResourceHook` / `ErrorHook`          | class          | lifecycle + error transform binding              |
|  [07]   | `ResourceOptions`                     | interface      | `parent`/`dependsOn`/`protect`/`ignoreChanges`/`provider`/`transforms` |
|  [08]   | `CustomResourceOptions`               | interface      | adds `import`/`id`/`deleteBeforeReplace`/`replaceOnChanges` |
|  [09]   | `ComponentResourceOptions`            | interface      | adds `providers` map                             |
|  [10]   | `CustomTimeouts` / `Alias`            | interface      | per-op timeout strings / rename descriptor       |
|  [11]   | `mergeOptions(a, b)`                  | fold           | overload trio; merge base opts into overrides    |
|  [12]   | `createUrn` / `resourceType` / `resourceName` | utility | compute URN / read type / read name              |

[PUBLIC_TYPE_SCOPE]: config, assets, errors, log
- rail: deployment

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]  | [NOTE]                                            |
| :-----: | :--------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `Config`                                       | class          | read matrix: `{get,require}{,Secret}{,Boolean,Number,Object}` — mode × type × secrecy |
|  [02]   | `StringConfigOptions` / `NumberConfigOptions`  | interface      | `allowedValues`/`pattern`/`min`/`max` validation |
|  [03]   | `Asset` → `FileAsset`/`StringAsset`/`RemoteAsset` | class family | file/inline/URL asset for chart + configmap inputs |
|  [04]   | `Archive` → `AssetArchive`/`FileArchive`/`RemoteArchive` | class family | multi-file archive inputs                   |
|  [05]   | `RunError` / `ResourceError`                   | error class    | clean abort / resource-associated abort          |
|  [06]   | `InputPropertyError` / `InputPropertiesError`  | error class    | single / multi input-property validation failure |
|  [07]   | `log.{debug,info,warn,error,hasErrors}`        | function set   | structured deployment-engine log (per-resource)  |

## [04]-[AUTOMATION_API]

The programmatic lifecycle (`@pulumi/pulumi/automation`) — the ONLY entry `iac` uses. `LocalWorkspace.createOrSelectStack({stackName, projectName, program})` is idempotent; `program: PulumiFn` is the inline typed program whose returned record becomes stack outputs. `LocalWorkspaceOptions` carries every deploy-host fact — the CLI wrap, self-managed backend, state-secrets provider, env — so nothing leaks to disk. Every lifecycle method returns a typed result folded into the run receipt.

[PUBLIC_TYPE_SCOPE]: workspace + program
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [NOTE]                                            |
| :-----: | :------------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `LocalWorkspace`                             | class         | `implements Workspace`; local CLI backend        |
|  [02]   | `Workspace`                                  | interface     | `program`/`projectSettings`/`stackSettings`/`whoAmI`/`listStacks`/`installPlugin`/`stackOutputs` |
|  [03]   | `PulumiFn`                                   | alias         | `() => Promise<Record<string,any> \| void>` — returned keys are outputs |
|  [04]   | `InlineProgramArgs` / `LocalProgramArgs`     | args          | `{stackName,projectName,program}` / `{stackName,workDir}` |
|  [05]   | `LocalWorkspaceOptions`                       | options       | `pulumiCommand`/`projectSettings`/`stackSettings`/`secretsProvider`/`envVars`/`pulumiHome`/`workDir` |
|  [06]   | `PulumiCommand`                              | class         | resolved CLI binary; version-pin + install owner |
|  [07]   | `ProjectSettings` / `ProjectBackend`         | settings      | `backend.url` selects the self-managed state store (`file://`/`s3://`/…) |
|  [08]   | `ConfigMap` / `ConfigValue`                  | config        | `{value, secret}` config entries                 |
|  [09]   | `StackSummary` / `WhoAmIResult` / `PluginInfo` | info        | roster / identity / plugin inventory             |

[ENTRYPOINT_SCOPE]: stack lifecycle
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SURFACE]                                                        | [FAMILY]      | [NOTE]                                   |
| :-----: | :--------------------------------------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `LocalWorkspace.createOrSelectStack(args, opts?): Promise<Stack>` | factory       | idempotent; `create`/`select` variants   |
|  [02]   | `Stack.up(opts?): Promise<UpResult>`                            | ledger        | deploy/update; `outputs` + `summary`     |
|  [03]   | `Stack.preview(opts?): Promise<PreviewResult>`                 | ledger        | dry-run; `changeSummary: OpMap`          |
|  [04]   | `Stack.refresh(opts?): Promise<RefreshResult>`                | ledger        | reconcile state with provider (mutating) |
|  [05]   | `Stack.destroy(opts?): Promise<DestroyResult>`                | ledger        | delete all resources                     |
|  [06]   | `Stack.previewRefresh(opts?): Promise<PreviewResult>`         | drift         | read-only refresh-preview; no mutation   |
|  [07]   | `Stack.outputs(): Promise<OutputMap>`                          | query         | `{[k]: {value, secret}}`                 |
|  [08]   | `Stack.{getConfig,setConfig,getAllConfig,setAllConfig,refreshConfig}` | config   | per-key + bulk config                    |
|  [09]   | `Stack.addEnvironments(...envs): Promise<void>`               | ESC           | attach Pulumi ESC environments to the stack |
|  [10]   | `Stack.{getTag,setTag,listTags}`                              | tag           | stack tag CRUD                           |
|  [11]   | `Stack.history(pageSize?,page?,secrets?): Promise<UpdateSummary[]>` | audit  | update history; `info()` = latest        |
|  [12]   | `Stack.cancel(): Promise<void>`                               | control       | cancel an in-flight update               |
|  [13]   | `Stack.exportStack()` / `importStack(state)`                 | state         | `Deployment` snapshot export/import (backup/DR) |
|  [14]   | `Stack.rename(opts): Promise<RenameResult>`                  | mutate        | rename stack in state                    |
|  [15]   | `Stack.import(opts): Promise<ImportResult>`                  | adopt         | batch-adopt existing cloud resources into state |
|  [16]   | `fullyQualifiedStackName(org, project, stack): string`      | helper        | canonical stack name                     |

[PREPARED_ROW]: `RemoteWorkspace.{create,select,createOrSelect}Stack(RemoteGitProgramArgs, RemoteWorkspaceOptions): Promise<RemoteStack>` — Pulumi-Deployments Git-sourced runs, demoted; `iac` runs inline `LocalWorkspace` programs against a self-managed backend, so `RemoteWorkspace` is a prepared row, not the entry.

## [05]-[ENGINE_EVENT_STREAM]

The native drift + progress pipeline the receipt ledger folds. Every lifecycle `opts.onEvent` delivers a discriminated `EngineEvent` per engine step; `previewRefresh` re-reads provider state read-only and streams `resourcePreEvent` steps where `StepEventMetadata.op` classifies the divergence and `detailedDiff` carries the per-property delta — the desired-vs-actual drift source, reconciled against `PreviewResult.changeSummary`. `OpType` is the 15-member operation vocabulary the drift fold buckets over.

[PUBLIC_TYPE_SCOPE]: engine events
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [NOTE]                                       |
| :-----: | :----------------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `EngineEvent`                              | event sum     | exactly one event field non-nil              |
|  [02]   | `ResourcePreEvent` / `ResOutputsEvent`     | interface     | `metadata: StepEventMetadata` before/after   |
|  [03]   | `ResOpFailedEvent`                         | interface     | `metadata` + `status`/`steps`                |
|  [04]   | `StepEventMetadata`                        | interface     | `op`/`urn`/`type`/`detailedDiff`/`old`/`new`/`provider` |
|  [05]   | `StepEventStateMetadata`                   | interface     | per-resource inputs/outputs/protect/id state |
|  [06]   | `OpType`                                   | string union  | 15-member CRUD op vocabulary                 |
|  [07]   | `OpMap`                                    | mapped type   | `{ [op in OpType]?: number }` per-op count   |
|  [08]   | `DiffKind` / `PropertyDiff`                | enum + iface  | 6-kind property diff; `inputDiff` flag       |
|  [09]   | `SummaryEvent`                             | interface     | `resourceChanges: OpMap`; end-of-update      |
|  [10]   | `DiagnosticEvent` / `PolicyEvent`          | interface     | provider diagnostic / CrossGuard violation   |
|  [11]   | `CommandError`                             | error base    | `ConcurrentUpdateError`/`StackNotFoundError`/`StackAlreadyExistsError` |

```ts contract
// @pulumi/pulumi/automation — the ledger + drift surface iac folds
export declare type OpType =
  | "same" | "create" | "update" | "delete" | "replace"
  | "create-replacement" | "delete-replaced" | "read" | "read-replacement"
  | "refresh" | "discard" | "discard-replaced" | "remove-pending-replace"
  | "import" | "import-replacement"
export declare type OpMap = { [key in OpType]?: number }

export interface StepEventMetadata {
  op: OpType; urn: string; type: string; provider: string
  old?: StepEventStateMetadata; new?: StepEventStateMetadata
  keys?: string[]; diffs?: string[]; detailedDiff?: Record<string, PropertyDiff>
}
export interface EngineEvent {
  sequence: number; timestamp: number
  resourcePreEvent?: ResourcePreEvent; resOutputsEvent?: ResOutputsEvent
  resOpFailedEvent?: ResOpFailedEvent; summaryEvent?: SummaryEvent
  diagnosticEvent?: DiagnosticEvent; policyEvent?: PolicyEvent
}
export interface PreviewResult { stdout: string; stderr: string; changeSummary: OpMap }

// GlobalOpts ∩ UpOptions — the option surface the Effect rail parameterizes
interface GlobalOpts {
  color?: "always" | "never" | "raw" | "auto"; message?: string
  debug?: boolean; tracing?: string; suppressOutputs?: boolean; suppressProgress?: boolean
}
interface UpOptions extends GlobalOpts {
  parallel?: number; expectNoChanges?: boolean; refresh?: boolean; diff?: boolean
  target?: string[]; replace?: string[]; exclude?: string[]; targetDependents?: boolean
  policyPacks?: string[]; policyPackConfigs?: string[]; plan?: string; continueOnError?: boolean
  onOutput?: (out: string) => void; onEvent?: (event: EngineEvent) => void; onError?: (err: string) => void
  signal?: AbortSignal        // ← Effect.async's AbortSignal binds here; Effect.interrupt aborts the run
}
```

## [06]-[INTEGRATION]

The receipt-ledger rail — how `@pulumi/pulumi` stacks onto the `effect` substrate into ONE typed program. The Automation API is Promise-shaped and callback-driven; the rail wraps it once so every downstream row composes typed Effects, never raw promises.

[RAIL]: `automation → effect` — the four stacking seams (all `effect` members verified real)

| [INDEX] | [PULUMI SEAM]                          | [EFFECT MEMBER]                    | [COMPOSED RAIL]                                                   |
| :-----: | :------------------------------------- | :-------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `Stack.up/preview/refresh/destroy` op  | `Match.value(op).pipe(Match.exhaustive)` | ONE ledger dispatch over the op tag → the method; not four drivers |
|  [02]   | `UpOptions.signal: AbortSignal`        | `Effect.async((resume, signal)=>…)` | `Effect.interrupt`/scope-close aborts the pulumi run — no orphan updates |
|  [03]   | `opts.onEvent: (EngineEvent)=>void`    | `Stream.asyncPush` / `Queue.unbounded` + `Stream.fromQueue` | engine steps become an Effect `Stream`; `Stream.runFold` buckets the `OpMap` |
|  [04]   | `EngineEvent` / `StepEventMetadata`    | `Schema.decodeUnknown(StepReceipt)` | each step decodes to a typed receipt row; drift fold reconciles vs `changeSummary` |
|  [05]   | `LocalWorkspaceOptions` host facts     | `Config.redacted` + `Layer.effect`  | `pulumiCommand`/`backend.url`/`secretsProvider`/`envVars` from Effect `Config`, not literals |
|  [06]   | `OutputMap` `{value,secret}`           | `Schema.decodeUnknown` + `Redacted.make` | secret-flagged outputs → `Redacted`; typed StackOutputs → `ShardingConfig` |
|  [07]   | `CommandError` family + `RunError`     | `Data.TaggedError` in `catch`       | `ConcurrentUpdateError`/`StackNotFoundError`/input errors → tagged domain faults |
|  [08]   | ephemeral stack lifecycle              | `Effect.acquireRelease`             | `createOrSelectStack` acquired, `destroy` released — scoped teardown |

```ts contract
// iac/program/automation.ts — the one wrap; every consumer sees a typed Effect
const run = (stack: Stack, op: LedgerOp, spec: StackSpec) =>
  Effect.async<RunReceipt, DeployFault>((resume, signal) => {
    const steps: StepReceipt[] = []
    const onEvent = (e: EngineEvent) =>
      e.resourcePreEvent && steps.push(decodeStep(e.resourcePreEvent.metadata))
    const opts = { signal, onEvent, onOutput: sink }            // signal ← Effect interruption
    Match.value(op).pipe(                                       // the up|preview|refresh|destroy ledger
      Match.tag("up", () => stack.up(opts)),
      Match.tag("preview", () => stack.preview(opts)),
      Match.tag("refresh", () => stack.refresh(opts)),
      Match.tag("destroy", () => stack.destroy(opts)),
      Match.exhaustive,
    )().then(
      (r) => resume(Effect.succeed(foldReceipt(op, r, steps))),
      (e) => resume(Effect.fail(toFault(e))),                   // CommandError → Data.TaggedError
    )
  })
```

## [07]-[IMPLEMENTATION_LAW]

[OUTPUT_TOPOLOGY]:
- `Output<T>` property access is direct (`Lifted`); reach for `.apply` only to transform, `all` only to join, `interpolate` only to template — never chain raw promises across a resource boundary.
- `.get()` is cloud-runtime-only; calling it during planning throws. `.apply` may be skipped at preview when the value is `unknown` — guard with `isUnknown`.
- `secret()`/`Config.requireSecret` mark values state-encrypted; a generated credential (`random`/`tls`) or a `Doppler` value crosses resource boundaries only as a secret-tracked `Output`.

[AUTOMATION_TOPOLOGY]:
- `LocalWorkspace` is the sole workspace for inline programs; `PulumiFn` returns the outputs record. Configure the CLI wrap, `backend.url`, and `secretsProvider` through `LocalWorkspaceOptions` — never author `Pulumi.yaml`.
- The ledger is ONE `Match.exhaustive` over the op tag; `previewRefresh` is the non-mutating drift leg; `expectNoChanges`/`plan` gate CI runs; `policyPacks` attach CrossGuard.
- Adopt existing cloud resources with `Stack.import`; back up state with `exportStack`; attach ESC with `addEnvironments`.

[RAIL_LAW]:
- Package: `@pulumi/pulumi`
- Owns: output algebra, resource model, config, stack references, Automation API, engine-event stream, error rails
- Accept: `Output<T>` for all inter-resource value flow; `Effect.async` signal for cancellation; `Config.redacted` for host secrets; `Schema` for `OutputMap` decode
- Reject: authored `Pulumi.yaml`; raw `pulumi` CLI shell-out; promise chaining across resource boundaries; `Config.get` for secrets; four parallel op drivers where one `Match.exhaustive` ledger owns them
