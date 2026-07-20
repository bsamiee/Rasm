# [TS_IAC_API_PULUMI_PULUMI]

`@pulumi/pulumi` is the deploy-plane engine: the `Output<T>`/`Input<T>` async-dependency algebra, the `Resource`/`ComponentResource` model the `stack` tiers extend, `Config`/`StackReference` state access, and the full Automation API (`LocalWorkspace` inline programs, `Stack` up|preview|refresh|destroy) whose `EngineEvent` stream and `OpType`/`OpMap` deltas feed the typed run-receipt ledger. `iac` composes it as ONE Effect rail — a mapped-record ledger dispatch over the `Stack` methods, the `onEvent` callback bridged through `Stream.asyncPush` whose acquire/release owns an `AbortController` so Effect interruption aborts the run, one `Stream.runFold` pass folding summary, steps, diagnostics, violations, stdout, and the timestamp band, and the `OutputMap` decoding through `Schema` into StackOutputs. No `Pulumi.yaml`, no CLI shell-out — the pulumi CLI-binary is one `PulumiCommand` deploy-host fact and the self-managed state backend is one `ProjectSettings.backend.url`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumi`
- package: `@pulumi/pulumi`
- module: `@pulumi/pulumi` (core — including the `InvokeOptions`/`InvokeOutputOptions` data-source-invoke seam), `@pulumi/pulumi/automation` (programmatic lifecycle), `@pulumi/pulumi/{asset,log,dynamic,provider,runtime,iterable,queryable}` (submodules — `queryable.ResolvedResource<T>` backs `@pulumi/policy` stack narrowing)
- license: `Apache-2.0`
- asset: output algebra, resource model, config, stack references, error rails, Automation API, engine-event stream
- runtime: `node` — the `pulumi` CLI binary is a deploy-host fact wrapped once via `PulumiCommand`/`LocalWorkspaceOptions.pulumiCommand`; provider plugins auto-download on first resource registration
- rail: deployment

## [02]-[OUTPUT_ALGEBRA]

Every resource arg and output flows through `Output<T>`, the async-dependency monad. `Output<T>` is `OutputInstance<T> & Lifted<T>` — property access on `Output<{a:string}>` yields `Output<string>` without `.apply`. All lifting is `output`/`secret`; all joining is `all` (pick the tuple overload to preserve element types); all string shaping is `interpolate`/`concat`; serialization is `jsonStringify`/`jsonParse`.

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

| [INDEX] | [SURFACE]                                            | [FAMILY]         | [NOTE]                                        |
| :-----: | :--------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `output<T>(val: Input<T>): Output<Unwrap<T>>`        | lift             | plain/promised → tracked output               |
|  [02]   | `secret<T>(val: Input<T>): Output<Unwrap<T>>`        | lift             | mark output sensitive (state-encrypted)       |
|  [03]   | `unsecret<T>(val: Output<T>): Output<T>`             | strip            | remove secret marking                         |
|  [04]   | `isSecret<T>(val: Output<T>): Promise<boolean>`      | predicate        | test secret flag                              |
|  [05]   | `all(vals): Output<...>`                             | combinator       | join record / tuple(2..8) / array of inputs   |
|  [06]   | `concat(...params: Input<any>[]): Output<string>`    | string join      | stringify + concatenate                       |
|  [07]   | `interpolate(literals, ...ph): Output<string>`       | tagged template  | template-literal output interpolation         |
|  [08]   | `jsonStringify(obj, replacer?, space?): Output<str>` | serializer       | `JSON.stringify` over `Input<any>`            |
|  [09]   | `jsonParse(text, reviver?): Output<any>`             | deserializer     | `JSON.parse` over `Input<string>`             |
|  [10]   | `recover<T>(o, (err)=>Input<T>): Output<T>`          | recovery         | recover a rejected output to a value          |
|  [11]   | `deferredOutput<T>(): [Output<T>, resolver]`         | deferred pair    | forward-reference an output before its source |
|  [12]   | `getAllResources<T>(op): Promise<Set<Resource>>`     | dependency query | resources an output depends on                |
|  [13]   | `isUnknown(v)` / `containsUnknowns(v)`               | preview guard    | detect preview-time unresolved values         |
|  [14]   | `Output.create` / `Output.isInstance` / `Output.all` | static           | factory / type guard / static join            |

## [03]-[RESOURCE_MODEL]

`stack` ComponentResource tiers extend this class hierarchy, and the `kube`/`secret`/`observe` rows instantiate it. `ComponentResource` is the grouping owner every tier subclasses (call `registerOutputs` last); `parent`/`dependsOn`/`protect`/`ignoreChanges` on the option interfaces build the ownership DAG; `mergeOptions` folds a base option bag into per-resource overrides so a tier passes ONE inherited `opts` down its children.

[PUBLIC_TYPE_SCOPE]: resource classes + options
- rail: deployment

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]  | [NOTE]                                                                             |
| :-----: | :------------------------------ | :------------- | :--------------------------------------------------------------------------------- |
|  [01]   | `Resource`                      | abstract class | base; `urn: Output<URN>`                                                           |
|  [02]   | `CustomResource`                | abstract class | extends `Resource`; `id: Output<ID>`                                               |
|  [03]   | `ComponentResource<TData>`      | class          | grouping owner; `registerOutputs(outputs?)`                                        |
|  [04]   | `ProviderResource`              | abstract class | explicit provider instance (multi-region/multi-cluster)                            |
|  [05]   | `StackReference`                | class          | cross-stack reads; `getOutput`/`requireOutput`/`getOutputDetails`                  |
|  [06]   | `ResourceHook` / `ErrorHook`    | class          | lifecycle + error transform binding                                                |
|  [07]   | `ResourceOptions`               | interface      | `parent`/`dependsOn`/`protect`/`ignoreChanges`/`provider`/`transforms`             |
|  [08]   | `CustomResourceOptions`         | interface      | adds `import`/`id`/`deleteBeforeReplace`/`replaceOnChanges`                        |
|  [09]   | `ComponentResourceOptions`      | interface      | adds `providers` map                                                               |
|  [10]   | `CustomTimeouts` / `Alias`      | interface      | per-op timeout strings / rename descriptor                                         |
|  [11]   | `mergeOptions(a, b)`            | fold           | overload trio; merge base opts into overrides                                      |
|  [12]   | `createUrn`                     | utility        | compute a resource URN                                                             |
|  [13]   | `resourceType`                  | utility        | read a resource's type token                                                       |
|  [14]   | `resourceName`                  | utility        | read a resource's name                                                             |
|  [15]   | `InvokeOptions`                 | interface      | data-source invoke opts: `parent`/`provider`/`version`/`pluginDownloadURL`/`async` |
|  [16]   | `InvokeOutputOptions`           | interface      | extends `InvokeOptions`; adds graph `dependsOn` for the `getXOutput` seam          |
|  [17]   | `URN` / `ID`                    | string brand   | `URN`/`ID` = `string`; identity `static get(name, id: Input<ID>, …)` adopts        |
|  [18]   | `queryable.ResolvedResource<T>` | utility        | `Omit<Resolved<T>, "urn"\|"getProvider">` — `@pulumi/policy` stack-narrowing view  |

[PUBLIC_TYPE_SCOPE]: config, assets, errors, log
- rail: deployment

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [NOTE]                                                       |
| :-----: | :------------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Config`                                                 | class         | read matrix `{get,require}{,Secret}{,Boolean,Number,Object}` |
|  [02]   | `StringConfigOptions` / `NumberConfigOptions`            | interface     | `allowedValues`/`pattern`/`min`/`max` validation             |
|  [03]   | `Asset` → `FileAsset`/`StringAsset`/`RemoteAsset`        | class family  | file/inline/URL asset for chart + configmap inputs           |
|  [04]   | `Archive` → `AssetArchive`/`FileArchive`/`RemoteArchive` | class family  | multi-file archive inputs                                    |
|  [05]   | `RunError` / `ResourceError`                             | error class   | clean abort / resource-associated abort                      |
|  [06]   | `InputPropertyError` / `InputPropertiesError`            | error class   | single / multi input-property validation failure             |
|  [07]   | `log.{debug,info,warn,error,hasErrors}`                  | function set  | structured deployment-engine log (per-resource)              |

## [04]-[AUTOMATION_API]

`@pulumi/pulumi/automation` is the programmatic lifecycle — the ONLY entry `iac` uses. `LocalWorkspace.createOrSelectStack({stackName, projectName, program})` is idempotent; `program: PulumiFn` is the inline typed program whose returned record becomes stack outputs. `LocalWorkspaceOptions` carries every deploy-host fact — the CLI wrap, self-managed backend, state-secrets provider, env — so nothing leaks to disk. Every lifecycle method returns a typed result folded into the run receipt.

[PUBLIC_TYPE_SCOPE]: workspace + program
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [NOTE]                                                                   |
| :-----: | :---------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `LocalWorkspace`        | class         | `implements Workspace`; local CLI backend                                |
|  [02]   | `Workspace`             | interface     | the workspace method contract (members in [02] below)                    |
|  [03]   | `PulumiFn`              | alias         | `() => Promise<Record<string,any> \| void>` — returned keys are outputs  |
|  [04]   | `InlineProgramArgs`     | args          | `{stackName, projectName, program}`                                      |
|  [05]   | `LocalProgramArgs`      | args          | `{stackName, workDir}`                                                   |
|  [06]   | `LocalWorkspaceOptions` | options       | deploy-host facts bag (fields in [06] below)                             |
|  [07]   | `PulumiCommand`         | class         | resolved CLI binary; version-pin + install owner                         |
|  [08]   | `ProjectSettings`       | settings      | project-level settings block                                             |
|  [09]   | `ProjectBackend`        | settings      | `backend.url` selects the self-managed state store (`file://`/`s3://`/…) |
|  [10]   | `ConfigMap`             | config        | `{ [key]: ConfigValue }` config bag                                      |
|  [11]   | `ConfigValue`           | config        | `{value, secret}` config entry                                           |
|  [12]   | `StackSummary`          | info          | stack roster row                                                         |
|  [13]   | `WhoAmIResult`          | info          | caller identity                                                          |
|  [14]   | `PluginInfo`            | info          | plugin inventory                                                         |

- [02]-[WORKSPACE]: `program`/`projectSettings`/`stackSettings`/`whoAmI`/`listStacks`/`installPlugin`/`stackOutputs`.
- [06]-[LOCAL_WORKSPACE_OPTIONS]: `pulumiCommand`/`secretsProvider`/`projectSettings`/`stackSettings`/`envVars`/`pulumiHome`/`workDir`.

[ENTRYPOINT_SCOPE]: stack lifecycle
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SURFACE]                                                             | [FAMILY] | [NOTE]                                          |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `LocalWorkspace.createOrSelectStack(args, opts?): Promise<Stack>`     | factory  | idempotent; `create`/`select` variants          |
|  [02]   | `Stack.up(opts?): Promise<UpResult>`                                  | ledger   | deploy/update; `outputs` + `summary`            |
|  [03]   | `Stack.preview(opts?): Promise<PreviewResult>`                        | ledger   | dry-run; `changeSummary: OpMap`                 |
|  [04]   | `Stack.refresh(opts?): Promise<RefreshResult>`                        | ledger   | reconcile state with provider (mutating)        |
|  [05]   | `Stack.destroy(opts?): Promise<DestroyResult>`                        | ledger   | delete all resources                            |
|  [06]   | `Stack.previewRefresh(opts?): Promise<PreviewResult>`                 | drift    | read-only refresh-preview; no mutation          |
|  [07]   | `Stack.outputs(): Promise<OutputMap>`                                 | query    | `{[k]: {value, secret}}`                        |
|  [08]   | `Stack.{getConfig,setConfig,getAllConfig,setAllConfig,refreshConfig}` | config   | per-key + bulk config                           |
|  [09]   | `Stack.addEnvironments(...envs): Promise<void>`                       | ESC      | attach Pulumi ESC environments to the stack     |
|  [10]   | `Stack.{getTag,setTag,listTags}`                                      | tag      | stack tag CRUD                                  |
|  [11]   | `Stack.history(pageSize?,page?,secrets?): Promise<UpdateSummary[]>`   | audit    | update history; `info()` = latest               |
|  [12]   | `Stack.cancel(): Promise<void>`                                       | control  | cancel an in-flight update                      |
|  [13]   | `Stack.exportStack()` / `importStack(state)`                          | state    | `Deployment` snapshot export/import (backup/DR) |
|  [14]   | `Stack.rename(opts): Promise<RenameResult>`                           | mutate   | rename stack in state                           |
|  [15]   | `Stack.import(opts): Promise<ImportResult>`                           | adopt    | batch-adopt existing cloud resources into state |
|  [16]   | `fullyQualifiedStackName(org, project, stack): string`                | helper   | canonical stack name                            |

[PREPARED_ROW]: `RemoteWorkspace.{create,select,createOrSelect}Stack(RemoteGitProgramArgs, RemoteWorkspaceOptions): Promise<RemoteStack>` — Pulumi-Deployments Git-sourced runs, demoted; `iac` runs inline `LocalWorkspace` programs against a self-managed backend, so `RemoteWorkspace` is a prepared row, not the entry.

## [05]-[ENGINE_EVENT_STREAM]

Native drift + progress pipeline the receipt ledger folds. Every lifecycle `opts.onEvent` delivers a discriminated `EngineEvent` per engine step; `previewRefresh` re-reads provider state read-only and streams `resourcePreEvent` steps where `StepEventMetadata.op` classifies the divergence and `detailedDiff` carries the per-property delta — the desired-vs-actual drift source, reconciled against `PreviewResult.changeSummary`. `OpType` is the 15-member operation vocabulary the drift fold buckets over.

[PUBLIC_TYPE_SCOPE]: engine events
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY] | [NOTE]                                                                  |
| :-----: | :------------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `EngineEvent`                          | event sum     | exactly one event field non-nil                                         |
|  [02]   | `ResourcePreEvent` / `ResOutputsEvent` | interface     | `metadata: StepEventMetadata` before/after                              |
|  [03]   | `ResOpFailedEvent`                     | interface     | `metadata` + `status`/`steps`                                           |
|  [04]   | `StepEventMetadata`                    | interface     | `op`/`urn`/`type`/`detailedDiff`/`old`/`new`/`provider`                 |
|  [05]   | `StepEventStateMetadata`               | interface     | per-resource inputs/outputs/protect/id state                            |
|  [06]   | `OpType`                               | string union  | 15-member CRUD op vocabulary                                            |
|  [07]   | `OpMap`                                | mapped type   | `{ [op in OpType]?: number }` per-op count                              |
|  [08]   | `DiffKind` / `PropertyDiff`            | enum + iface  | 6-kind property diff; `inputDiff` flag                                  |
|  [09]   | `SummaryEvent`                         | interface     | `resourceChanges: OpMap`; end-of-update                                 |
|  [10]   | `DiagnosticEvent`                      | interface     | provider diagnostic; `severity` is the bridged-provider error match key |
|  [11]   | `PolicyEvent`                          | interface     | CrossGuard violation                                                    |
|  [12]   | `StdoutEngineEvent`                    | interface     | stdout line; a non-resource `EngineEvent` arm                           |
|  [13]   | `PreludeEvent`                         | interface     | op-start config prelude                                                 |
|  [14]   | `CancelEvent`                          | type          | cancellation (`{}`)                                                     |
|  [15]   | `StartDebuggingEvent`                  | interface     | DAP-attach                                                              |
|  [16]   | `CommandError`                         | error base    | `ConcurrentUpdateError`/`StackNotFoundError`/`StackAlreadyExistsError`  |

```ts signature
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
  keys?: string[]; diffs?: string[]; detailedDiff?: Record<string, PropertyDiff>; logical?: boolean
}
export interface DiagnosticEvent {                              // grafana matches bridged-provider errors on severity
  urn?: string; prefix?: string; message: string; color: string
  severity: "info" | "info#err" | "warning" | "error"; streamID?: number; ephemeral?: boolean
}
export interface EngineEvent {                                  // exactly one event arm non-nil
  sequence: number; timestamp: number
  resourcePreEvent?: ResourcePreEvent; resOutputsEvent?: ResOutputsEvent; resOpFailedEvent?: ResOpFailedEvent
  summaryEvent?: SummaryEvent; diagnosticEvent?: DiagnosticEvent; policyEvent?: PolicyEvent
  stdoutEvent?: StdoutEngineEvent; preludeEvent?: PreludeEvent; cancelEvent?: CancelEvent; startDebuggingEvent?: StartDebuggingEvent
}
export interface PreviewResult { stdout: string; stderr: string; changeSummary: OpMap }

// GlobalOpts ∩ UpOptions — the option surface the Effect rail parameterizes
interface GlobalOpts {
  color?: "always" | "never" | "raw" | "auto"; tracing?: string
  debug?: boolean; suppressOutputs?: boolean; suppressProgress?: boolean
}
interface UpOptions extends GlobalOpts {
  parallel?: number; message?: string; expectNoChanges?: boolean; refresh?: boolean; diff?: boolean
  target?: string[]; replace?: string[]; exclude?: string[]; excludeDependents?: boolean; targetDependents?: boolean
  policyPacks?: string[]; policyPackConfigs?: string[]; plan?: string; continueOnError?: boolean; attachDebugger?: boolean
  onOutput?: (out: string) => void; onEvent?: (event: EngineEvent) => void; onError?: (err: string) => void
  signal?: AbortSignal        // Effect.async's AbortSignal binds here; Effect.interrupt aborts the run
}
```

## [06]-[INTEGRATION]

Receipt-ledger rail — how `@pulumi/pulumi` stacks onto the `effect` substrate into ONE typed program. Automation API is Promise-shaped and callback-driven; the rail wraps it once so every downstream row composes typed Effects, never raw promises.

[RAIL]: `automation → effect` — the stacking seams (all `effect` members verified real)

| [INDEX] | [PULUMI_SEAM]                         | [EFFECT_MEMBER]                                       |
| :-----: | :------------------------------------ | :---------------------------------------------------- |
|  [01]   | `Stack.up/preview/refresh/destroy` op | `_LEDGER` mapped record, one generic indexed call     |
|  [02]   | `UpOptions.signal: AbortSignal`       | `Effect.acquireRelease` over an `AbortController`     |
|  [03]   | `opts.onEvent: (EngineEvent)=>void`   | `Stream.asyncPush` + `Stream.runFold` over `_folded`  |
|  [04]   | `EngineEvent` fold product            | `Schema.decodeUnknown(RunReceipt)`                    |
|  [05]   | `LocalWorkspaceOptions` host facts    | `Config.unwrap` + `Config.redacted`                   |
|  [06]   | `OutputMap` `{value,secret}`          | `Schema.decodeUnknown` + secret-refusal gate          |
|  [07]   | `CommandError` family + `RunError`    | `Match.instanceOf` triage → `Data.TaggedError`        |
|  [08]   | ephemeral stack lifecycle             | `Effect.acquireRelease`                               |

- [01]-[LEDGER_DISPATCH]: `_LEDGER` maps each op (`reconcile` included) to its `Stack` method under a mapped contract — a sixth op is a compile error at the record, not four drivers.
- [02]-[INTERRUPT]: the bridge acquires an `AbortController` and releases by aborting — interruption, scope close, and budget exhaustion all cancel the run; no orphan updates.
- [03]-[EVENT_STREAM]: engine events become one Effect `Stream`; `Stream.runFold` drives the single `_folded` pass on the live path and `Array.reduce` drives the same fold on the batch path.
- [04]-[RECEIPT_DECODE]: the fold product decodes once into `RunReceipt` — summary, steps, diagnostics, violations, stdout lines, and the first/last-timestamp band; the drift fold reconciles vs `changeSummary`.
- [05]-[HOST_CONFIG]: `pulumiCommand`/`backend.url`/`secretsProvider`/`envVars` from Effect `Config`, not literals.
- [06]-[SECRET_OUTPUTS]: secret-flagged outputs refuse at the gate; typed StackOutputs → `ShardingConfig`.
- [07]-[TAGGED_FAULTS]: `ConcurrentUpdateError`/`StackNotFoundError`/input errors triage through one `Match.instanceOf` ladder into the reason-discriminated fault family.
- [08]-[SCOPED_LIFECYCLE]: `createOrSelectStack` acquired, `destroy` released — scoped teardown.

```ts signature
// iac/program/automation.ts — the one wrap; every consumer sees a typed Effect
const _LEDGER: { readonly [K in RunReceipt.Op]: (stack: Stack, opts: _RunOpts) => Promise<unknown> } = {
  up: (stack, opts) => stack.up(opts),
  preview: (stack, opts) => stack.preview(opts),
  refresh: (stack, { signal, onEvent, parallel }) => stack.refresh({ signal, onEvent, parallel }),
  destroy: (stack, { signal, onEvent, parallel }) => stack.destroy({ signal, onEvent, parallel }),
  reconcile: (stack, { signal, onEvent, parallel }) => stack.previewRefresh({ signal, onEvent, parallel }),
}

const _streamed = (stack: Stack, name: string, op: RunReceipt.Op, options?: Automation.Options) =>
  Stream.asyncPush<EngineEvent, DeployFault>((emit) =>
    Effect.acquireRelease(
      Effect.sync(() => {
        const abort = new AbortController()
        _LEDGER[op](stack, { ...options, signal: abort.signal, onEvent: (event) => void emit.single(event) }).then(
          () => emit.end(),
          (caught) => emit.fail(DeployFault.triaged(name)(caught)),
        )
        return abort
      }),
      (abort) => Effect.sync(() => abort.abort()),              // release aborts the engine run: no orphan update
    ))

// one pass, both paths: Stream.runFold(_streamed(…), _SEED, _folded) live; Array.reduce(events, _SEED, _folded) batch
```

## [07]-[IMPLEMENTATION_LAW]

[OUTPUT_TOPOLOGY]:
- `Output<T>` property access is direct (`Lifted`); reach for `.apply` only to transform, `all` only to join, `interpolate` only to template — never chain raw promises across a resource boundary.
- `.get()` is cloud-runtime-only; calling it during planning throws. `.apply` may be skipped at preview when the value is `unknown` — guard with `isUnknown`.
- `secret()`/`Config.requireSecret` mark values state-encrypted; a generated credential (`random`/`tls`) or a `Doppler` value crosses resource boundaries only as a secret-tracked `Output`.

[AUTOMATION_TOPOLOGY]:
- `LocalWorkspace` is the sole workspace for inline programs; `PulumiFn` returns the outputs record. Configure the CLI wrap, `backend.url`, and `secretsProvider` through `LocalWorkspaceOptions` — never author `Pulumi.yaml`.
- The `_LEDGER` mapped record over the op vocabulary is the whole ledger; `previewRefresh` is the non-mutating drift leg; `expectNoChanges`/`plan` gate CI runs; `policyPacks` attach CrossGuard.
- Adopt existing cloud resources with `Stack.import`; back up state with `exportStack`; attach ESC with `addEnvironments`.

[RAIL_LAW]:
- Package: `@pulumi/pulumi`
- Owns: output algebra, resource model, config, stack references, Automation API, engine-event stream, error rails
- Accept: `Output<T>` for all inter-resource value flow; the acquire/release `AbortController` for cancellation; `Config.redacted` for host secrets; `Schema` for `OutputMap` decode
- Reject: authored `Pulumi.yaml`; raw `pulumi` CLI shell-out; promise chaining across resource boundaries; `Config.get` for secrets; four parallel op drivers where the one `_LEDGER` mapped record owns them
