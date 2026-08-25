# [TS_IAC_API_PULUMI_PULUMI]

`@pulumi/pulumi` is the deploy-plane engine: the `Output<T>`/`Input<T>` dependency algebra, the `Resource`/`ComponentResource` model every stack tier extends, `Config`/`StackReference` state access, and the Automation API's operation-specific lifecycle results. `iac` lifts inline `LocalWorkspace` programs onto one Effect rail.

## [01]-[OUTPUT_ALGEBRA]

Every resource arg and output flows through `Output<T>`, the async-dependency monad: `OutputInstance<T> & Lifted<T>`, so property access on a struct output lifts to `Output<field>` without `.apply`. Lifting, joining, string shaping, and JSON transit each own one combinator.

[PUBLIC_TYPE_SCOPE]: output types

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

## [02]-[RESOURCE_MODEL]

`stack` ComponentResource tiers subclass this hierarchy and the `kube`/`secret`/`observe` rows instantiate it. `ComponentResource` is the grouping owner (`registerOutputs` runs last), the resource-option interfaces build the ownership DAG, and `mergeOptions` folds one inherited `opts` bag down a tier's children.

[PUBLIC_TYPE_SCOPE]: resource classes + options

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

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [NOTE]                                                       |
| :-----: | :------------------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Config`                                                 | class         | read matrix `{get,require}{,Secret}{,Boolean,Number,Object}` |
|  [02]   | `StringConfigOptions` / `NumberConfigOptions`            | interface     | `allowedValues`/`pattern`/`min`/`max` validation             |
|  [03]   | `Asset` → `FileAsset`/`StringAsset`/`RemoteAsset`        | class family  | file/inline/URL asset for chart + configmap inputs           |
|  [04]   | `Archive` → `AssetArchive`/`FileArchive`/`RemoteArchive` | class family  | multi-file archive inputs                                    |
|  [05]   | `RunError` / `ResourceError`                             | error class   | clean abort / resource-associated abort                      |
|  [06]   | `InputPropertyError` / `InputPropertiesError`            | error class   | single / multi input-property validation failure             |
|  [07]   | `log.{debug,info,warn,error,hasErrors}`                  | function set  | structured deployment-engine log (per-resource)              |

## [03]-[AUTOMATION_API]

`@pulumi/pulumi/automation` is the programmatic lifecycle. `LocalWorkspace.createOrSelectStack` is idempotent, `program: PulumiFn` returns the record that becomes stack outputs, and `LocalWorkspaceOptions` carries the deploy-host facts. Each lifecycle method returns its own native result type.

[PUBLIC_TYPE_SCOPE]: workspace + program

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [NOTE]                                                                   |
| :-----: | :---------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `LocalWorkspace`        | class         | `implements Workspace`; local CLI backend                                |
|  [02]   | `Workspace`             | interface     | the workspace method contract (members in [02] below)                    |
|  [03]   | `PulumiFn`              | alias         | `() => Promise<Record<string,any> \| void>` — returned keys are outputs  |
|  [04]   | `InlineProgramArgs`     | args          | `{stackName, projectName, program}`                                      |
|  [05]   | `LocalProgramArgs`      | args          | `{stackName, workDir}`                                                   |
|  [06]   | `LocalWorkspaceOptions` | options       | deploy-host facts bag (fields in [06] below)                             |
|  [07]   | `PulumiCommand`         | class         | resolved CLI binary; install owner                                       |
|  [08]   | `ProjectSettings`       | settings      | project-level settings block                                             |
|  [09]   | `ProjectBackend`        | settings      | `backend.url` selects the self-managed state store (`file://`/`s3://`/…) |
|  [10]   | `ConfigMap`             | config        | `{ [key]: ConfigValue }` config bag                                      |
|  [11]   | `ConfigValue`           | config        | `{value, secret}` config entry                                           |
|  [12]   | `StackSummary`          | info          | stack roster row                                                         |
|  [13]   | `WhoAmIResult`          | info          | caller identity                                                          |
|  [14]   | `PluginInfo`            | info          | plugin inventory                                                         |

- [02]-[WORKSPACE]: `program` `projectSettings` `stackSettings` `whoAmI` `listStacks` `installPlugin` `stackOutputs`.
- [06]-[LOCAL_WORKSPACE_OPTIONS]: `pulumiCommand` `secretsProvider` `projectSettings` `stackSettings` `envVars` `pulumiHome` `workDir`.

[ENTRYPOINT_SCOPE]: stack lifecycle

| [INDEX] | [SURFACE]                                                             | [FAMILY]  | [NOTE]                                          |
| :-----: | :-------------------------------------------------------------------- | :-------- | :---------------------------------------------- |
|  [01]   | `LocalWorkspace.createOrSelectStack(args, opts?): Promise<Stack>`     | factory   | idempotent; `create`/`select` variants          |
|  [02]   | `Stack.up(opts?): Promise<UpResult>`                                  | lifecycle | deploy/update; `outputs` + `summary`            |
|  [03]   | `Stack.preview(opts?): Promise<PreviewResult>`                        | lifecycle | dry-run; `changeSummary: OpMap`                 |
|  [04]   | `Stack.refresh(opts?): Promise<RefreshResult>`                        | lifecycle | reconcile state with provider (mutating)        |
|  [05]   | `Stack.destroy(opts?): Promise<DestroyResult>`                        | lifecycle | delete all resources                            |
|  [06]   | `Stack.previewRefresh(opts?): Promise<PreviewResult>`                 | drift     | read-only refresh-preview; no mutation          |
|  [07]   | `Stack.outputs(): Promise<OutputMap>`                                 | query     | `{[k]: {value, secret}}`                        |
|  [08]   | `Stack.{getConfig,setConfig,getAllConfig,setAllConfig,refreshConfig}` | config    | per-key + bulk config                           |
|  [09]   | `Stack.addEnvironments(...envs): Promise<void>`                       | ESC       | attach Pulumi ESC environments to the stack     |
|  [10]   | `Stack.{getTag,setTag,listTags}`                                      | tag       | stack tag CRUD                                  |
|  [11]   | `Stack.history(pageSize?,page?,secrets?): Promise<UpdateSummary[]>`   | audit     | update history; `info()` = latest               |
|  [12]   | `Stack.cancel(): Promise<void>`                                       | control   | cancel an in-flight update                      |
|  [13]   | `Stack.exportStack()` / `importStack(state)`                          | state     | `Deployment` snapshot export/import (backup/DR) |
|  [14]   | `Stack.rename(opts): Promise<RenameResult>`                           | mutate    | rename stack in state                           |
|  [15]   | `Stack.import(opts): Promise<ImportResult>`                           | adopt     | batch-adopt existing cloud resources into state |
|  [16]   | `fullyQualifiedStackName(org, project, stack): string`                | helper    | canonical stack name                            |

[PREPARED_ROW]: `RemoteWorkspace.{create,select,createOrSelect}Stack(RemoteGitProgramArgs, RemoteWorkspaceOptions) -> Promise<RemoteStack>` runs Pulumi-Deployments Git-sourced stacks; `iac` drives inline `LocalWorkspace` against a self-managed backend, so this is a prepared row, not the entry.

## [04]-[ENGINE_EVENT_STREAM]

Every lifecycle `opts.onEvent` delivers a discriminated `EngineEvent` directly to the caller. `previewRefresh` re-reads provider state without mutation and returns `PreviewResult`, whose `changeSummary` is the native drift fact.

[PUBLIC_TYPE_SCOPE]: engine events

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

[STEP_EVENT_METADATA]: `op: OpType` `urn: string` `type: string` `provider: string` `old: StepEventStateMetadata` `new: StepEventStateMetadata` `keys: string[]` `diffs: string[]` `detailedDiff: Record<string,PropertyDiff>` `logical: boolean`
[DIAGNOSTIC_EVENT]: `urn: string` `prefix: string` `message: string` `color: string` `severity: "info"|"info#err"|"warning"|"error"` `streamID: number` `ephemeral: boolean`
[ENGINE_EVENT]: `sequence: number` `timestamp: number` `resourcePreEvent` `resOutputsEvent` `resOpFailedEvent` `summaryEvent` `diagnosticEvent` `policyEvent` `stdoutEvent` `preludeEvent` `cancelEvent` `startDebuggingEvent`
[PREVIEW_RESULT]: `stdout: string` `stderr: string` `changeSummary: OpMap`
[GLOBAL_OPTS]: `color: "always"|"never"|"raw"|"auto"` `tracing: string` `debug: boolean` `suppressOutputs: boolean` `suppressProgress: boolean`
[UP_OPTIONS]: `parallel: number` `message: string` `expectNoChanges: boolean` `refresh: boolean` `diff: boolean` `target: string[]` `replace: string[]` `exclude: string[]` `excludeDependents: boolean` `targetDependents: boolean` `policyPacks: string[]` `policyPackConfigs: string[]` `plan: string` `continueOnError: boolean` `attachDebugger: boolean` `onOutput: (out:string)=>void` `onEvent: (event:EngineEvent)=>void` `onError: (err:string)=>void` `signal: AbortSignal`

## [05]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Output<T>` property access is direct through `Lifted`; `.apply` transforms, `all` joins, `interpolate` templates, never raw promise chaining across a resource boundary.
- `.get()` is cloud-runtime-only and throws during planning; `.apply` may skip at preview when the value is `unknown`, guarded by `isUnknown`.
- `secret()`/`Config.requireSecret` mark a value state-encrypted, so a generated `random`/`tls` credential or a `Doppler` value crosses a resource boundary only as a secret-tracked `Output`.
- `LocalWorkspace` is the sole workspace for inline programs; the CLI wrap, `backend.url`, and `secretsProvider` bind through `LocalWorkspaceOptions`, never an authored `Pulumi.yaml`.
- `_operations` maps the operation vocabulary to `Stack` methods while preserving each method's native result type; `previewRefresh` is the non-mutating drift leg, `policyPacks` attach CrossGuard, and stack methods retain their own state and ESC results.

[STACKING]:
- `effect`(`libs/typescript/.api/effect.md`): every `Stack` operation lifts through `Effect.tryPromise`, which threads fiber interruption through its `AbortSignal`; retry, timeout, and tracing compose on that rail while `onEvent` remains the caller's direct observation callback.

| [INDEX] | [PULUMI_SEAM]                         | [EFFECT_MEMBER]                                           |
| :-----: | :------------------------------------ | :-------------------------------------------------------- |
|  [01]   | `Stack.up/preview/refresh/destroy` op | `_operations` mapped record with correlated native result |
|  [02]   | lifecycle `signal: AbortSignal`       | `Effect.tryPromise` fiber interruption                    |
|  [03]   | `opts.onEvent: (EngineEvent)=>void`   | caller-supplied callback                                  |
|  [04]   | `LocalWorkspaceOptions` host facts    | `Config.unwrap` + `Config.redacted`                       |
|  [05]   | `OutputMap` `{value,secret}`          | `Schema.decodeUnknown` + secret-refusal gate              |
|  [06]   | `CommandError` family + `RunError`    | `Match.instanceOf` triage → `Data.TaggedError`            |
|  [07]   | ephemeral stack lifecycle             | `Effect.acquireRelease`                                   |

[_OPERATIONS]: `_operations.up` `_operations.preview` `_operations.refresh` `_operations.destroy` `_operations.reconcile`
[SURFACES]: `Automation.run` `Automation.reconcile`

- `_operations` maps every operation to its `Stack` method, and indexed `ReturnType` derives the selected method's native success type.
- `Effect.tryPromise` supplies the interrupt-bound signal to each lifecycle method; an ephemeral stack still brackets acquisition and destroy with `Effect.acquireRelease`.
- `Automation.reconcile` returns `PreviewResult` directly, and `Drift.sweep` publishes its `changeSummary` without a second result model.
- Secret-flagged outputs refuse at the decode gate and typed StackOutputs decode into `ShardingConfig`; the `CommandError` family and `RunError` triage through one `Match.instanceOf` ladder into the reason-discriminated fault family.
