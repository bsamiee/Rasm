# [API_CATALOGUE] @pulumi/pulumi

`@pulumi/pulumi` supplies the core Pulumi SDK: the `Output<T>` / `Input<T>` algebra, `Resource`, `CustomResource`, `ComponentResource`, `StackReference`, `Config`, typed error classes, module namespaces for `asset`, `automation`, `log`, `runtime`, and `provider`, the full Automation API (`LocalWorkspace`, `Stack`) for programmatic up/preview/destroy/refresh lifecycle drives, and the automation engine-event stream (`EngineEvent`, `StepEventMetadata`, `OpType`, `OpMap`, `DiffKind`, `PropertyDiff`, `previewRefresh`) the deploy and drift drivers fold in the services deploy tier.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumi`
- package: `@pulumi/pulumi`
- module: `@pulumi/pulumi` (main), `@pulumi/pulumi/automation` (programmatic API)
- asset: output algebra, resource model, config, stack references, automation SDK, error types
- rail: deployment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: output algebra family
- rail: deployment

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                    |
| :-----: | :------------------ | :--------------- | :---------------------------------------- |
|  [01]   | `Output<T>`         | async value type | `OutputInstance<T> & Lifted<T>`           |
|  [02]   | `OutputInstance<T>` | interface        | `apply`, `get` contract                   |
|  [03]   | `Input<T>`          | type alias       | `T \| Promise<T> \| OutputInstance<T>`    |
|  [04]   | `Inputs`            | type alias       | `Record<string, Input<any>>`              |
|  [05]   | `Unwrap<T>`         | utility type     | deeply unwrap `Promise` and `Output` nest |
|  [06]   | `Lifted<T>`         | utility type     | property-lifted `Output` projection       |

[PUBLIC_TYPE_SCOPE]: resource class family
- rail: deployment

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [RAIL]                                      |
| :-----: | :------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Resource`                 | abstract class | base; holds `urn: Output<URN>`              |
|  [02]   | `CustomResource`           | abstract class | extends `Resource`; holds `id: Output<ID>`  |
|  [03]   | `ComponentResource<TData>` | class          | extends `Resource`; logical grouping owner  |
|  [04]   | `ProviderResource`         | abstract class | extends `CustomResource`; CRUD provider     |
|  [05]   | `StackReference`           | class          | extends `CustomResource`; cross-stack reads |
|  [06]   | `ResourceHook`             | class          | lifecycle hook binding                      |
|  [07]   | `ErrorHook`                | class          | error lifecycle hook                        |

[PUBLIC_TYPE_SCOPE]: options family
- rail: deployment

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `ResourceOptions`          | interface     | `parent`, `dependsOn`, `protect`, `ignoreChanges`    |
|  [02]   | `CustomResourceOptions`    | interface     | extends `ResourceOptions`; adds `import`, `id`       |
|  [03]   | `ComponentResourceOptions` | interface     | extends `ResourceOptions`; adds `providers`          |
|  [04]   | `CustomTimeouts`           | interface     | `create`, `update`, `delete`, `read` timeout strings |
|  [05]   | `Alias`                    | interface     | prior name descriptor for resource rename            |

[PUBLIC_TYPE_SCOPE]: config class
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `Config`              | class         | typed config and secret resolution                   |
|  [02]   | `StringConfigOptions` | interface     | `allowedValues`, `minLength`, `maxLength`, `pattern` |
|  [03]   | `NumberConfigOptions` | interface     | `min`, `max` bounds                                  |

[PUBLIC_TYPE_SCOPE]: error class family
- rail: deployment

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :--------------------- | :------------ | :--------------------------------------- |
|  [01]   | `RunError`             | error class   | clean abort without stack trace          |
|  [02]   | `ResourceError`        | error class   | resource-associated abort                |
|  [03]   | `InputPropertyError`   | error class   | single input property validation failure |
|  [04]   | `InputPropertiesError` | error class   | multi-property validation failure        |

[PUBLIC_TYPE_SCOPE]: automation API family
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------- | :------------ | :------------------------------------- |
|  [01]   | `LocalWorkspace`        | class         | `Workspace` implementation; local CLI  |
|  [02]   | `Stack`                 | class         | up / preview / refresh / destroy owner |
|  [03]   | `LocalWorkspaceOptions` | options type  | `workDir`, `envVars`, `program`        |
|  [04]   | `UpResult`              | result type   | summary + outputs map                  |
|  [05]   | `PreviewResult`         | result type   | change summary                         |
|  [06]   | `DestroyResult`         | result type   | resource removal summary               |
|  [07]   | `RefreshResult`         | result type   | state reconciliation summary           |
|  [08]   | `OutputMap`             | type alias    | `Record<string, OutputValue>`          |
|  [09]   | `OutputValue`           | value type    | `{ value: any; secret: boolean }`      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: output algebra functions
- rail: deployment

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]    | [RAIL]                                |
| :-----: | :-------------------------------------------- | :---------------- | :------------------------------------ |
|  [01]   | `output<T>(val: Input<T>): Output<Unwrap<T>>` | lift              | wraps plain or promised value         |
|  [02]   | `secret<T>(val: Input<T>): Output<Unwrap<T>>` | lift              | wraps value as secret output          |
|  [03]   | `unsecret<T>(val: Output<T>): Output<T>`      | strip             | removes secret marking                |
|  [04]   | `isSecret<T>(val: Output<T>): Promise<bool>`  | predicate         | tests secret flag                     |
|  [05]   | `all(vals): Output<...>`                      | combinator        | joins heterogeneous input tuple       |
|  [06]   | `concat(...params): Output<string>`           | string combinator | stringifies and concatenates inputs   |
|  [07]   | `interpolate(literals, ...): Output<string>`  | tagged template   | template-literal output interpolation |
|  [08]   | `jsonStringify(obj, ...): Output<string>`     | serializer        | `JSON.stringify` over `Input<any>`    |
|  [09]   | `jsonParse(text, ...): Output<any>`           | deserializer      | `JSON.parse` over `Input<string>`     |
|  [10]   | `deferredOutput<T>()`                         | deferred factory  | `[Output<T>, resolver]` pair          |
|  [11]   | `Output.apply<U>(fn): Output<U>`              | instance method   | transform with dependency tracking    |
|  [12]   | `Output.get(): T`                             | instance method   | unwrap at cloud runtime only          |
|  [13]   | `Output.create<T>(val): Output<Unwrap<T>>`    | static factory    | construct output from input           |
|  [14]   | `Output.isInstance(obj): obj is Output<T>`    | static predicate  | runtime type guard                    |

[ENTRYPOINT_SCOPE]: resource construction
- rail: deployment

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                           |
| :-----: | :----------------------------------------------------------------- | :-------------- | :------------------------------- |
|  [01]   | `new CustomResource(t, name, props?, opts?)`                       | constructor     | leaf managed resource            |
|  [02]   | `new ComponentResource(type, name, args?, opts?)`                  | constructor     | logical grouping owner           |
|  [03]   | `ComponentResource.registerOutputs(outputs?)`                      | instance method | declare output map for component |
|  [04]   | `new StackReference(name, args?, opts?)`                           | constructor     | cross-stack output reader        |
|  [05]   | `StackReference.getOutput(name): Output<any>`                      | instance method | fetch output or `undefined`      |
|  [06]   | `StackReference.requireOutput(name): Output<any>`                  | instance method | fetch output or throw            |
|  [07]   | `StackReference.getOutputDetails(name): Promise<OutputDetails>`    | instance method | value + secret flag              |
|  [08]   | `createUrn(name, type, parent?, project?, stack?): Output<string>` | utility         | compute resource URN             |

[ENTRYPOINT_SCOPE]: Config resolution
- rail: deployment

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :----------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `new Config(name?)`                                    | constructor    | scoped config bag by project name |
|  [02]   | `Config.get(key, opts?): K \| undefined`               | optional read  | string value or `undefined`       |
|  [03]   | `Config.require(key, opts?): K`                        | required read  | throws if absent                  |
|  [04]   | `Config.getSecret(key, opts?): Output<K> \| undefined` | secret read    | secret output or `undefined`      |
|  [05]   | `Config.requireSecret(key, opts?): Output<K>`          | secret read    | required secret output            |
|  [06]   | `Config.getBoolean / requireBoolean`                   | typed read     | boolean resolution                |
|  [07]   | `Config.getNumber / requireNumber`                     | typed read     | number resolution                 |
|  [08]   | `Config.getObject<T> / requireObject<T>`               | typed read     | JSON-deserialized object          |

[ENTRYPOINT_SCOPE]: Automation API lifecycle
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]   | [RAIL]                             |
| :-----: | :---------------------------------------------------------------- | :--------------- | :--------------------------------- |
|  [01]   | `LocalWorkspace.create(opts): Promise<LocalWorkspace>`            | factory          | workspace from options             |
|  [02]   | `LocalWorkspace.createStack(args, opts?): Promise<Stack>`         | factory          | new stack; fails if already exists |
|  [03]   | `LocalWorkspace.selectStack(args, opts?): Promise<Stack>`         | factory          | existing stack; fails if absent    |
|  [04]   | `LocalWorkspace.createOrSelectStack(args, opts?): Promise<Stack>` | factory          | idempotent stack selection         |
|  [05]   | `Stack.create(name, workspace): Promise<Stack>`                   | factory          | construct from existing workspace  |
|  [06]   | `Stack.select(name, workspace): Promise<Stack>`                   | factory          | select from existing workspace     |
|  [07]   | `Stack.createOrSelect(name, workspace): Promise<Stack>`           | factory          | idempotent construction            |
|  [08]   | `Stack.up(opts?): Promise<UpResult>`                              | lifecycle method | deploy or update                   |
|  [09]   | `Stack.preview(opts?): Promise<PreviewResult>`                    | lifecycle method | dry-run diff                       |
|  [10]   | `Stack.destroy(opts?): Promise<DestroyResult>`                    | lifecycle method | delete all resources               |
|  [11]   | `Stack.refresh(opts?): Promise<RefreshResult>`                    | lifecycle method | reconcile state with provider      |
|  [12]   | `Stack.outputs(): Promise<OutputMap>`                             | query method     | current stack output values        |
|  [13]   | `Stack.setConfig(key, value, path?): Promise<void>`               | config method    | set config key-value               |
|  [14]   | `Stack.getAllConfig(): Promise<ConfigMap>`                        | config method    | full config map                    |

## [04]-[AUTOMATION_EVENT_STREAM]

The `@pulumi/pulumi/automation` engine-event surface the deploy and drift drivers fold. A lifecycle method's `onEvent` callback (on `UpOptions`, `PreviewOptions`, `RefreshOptions`, `DestroyOptions`) delivers a discriminated `EngineEvent` per engine step; `previewRefresh({ onEvent })` re-reads provider state read-only and streams `resourcePreEvent` steps without mutating the stack, the source the desired-vs-actual drift fold consumes. `EngineEvent` is a sum where exactly one event field is non-nil; the per-step `StepEventMetadata` carries the `OpType` operation and the optional `detailedDiff` property delta. The `PreviewResult.changeSummary` `OpMap` is the engine's own per-`OpType` count, reconciled against a folded bucket count.

[PUBLIC_TYPE_SCOPE]: engine-event union and step metadata
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `EngineEvent`            | interface     | event sum; exactly one event field non-nil              |
|  [02]   | `ResourcePreEvent`       | interface     | `metadata: StepEventMetadata`; emitted before a step    |
|  [03]   | `ResOutputsEvent`        | interface     | `metadata: StepEventMetadata`; emitted after a step     |
|  [04]   | `ResOpFailedEvent`       | interface     | `metadata` + `status`/`steps`; step failure             |
|  [05]   | `StepEventMetadata`      | interface     | `op`/`urn`/`type`/`detailedDiff`/`old`/`new`/`provider` |
|  [06]   | `StepEventStateMetadata` | interface     | per-resource old/new state detail                       |
|  [07]   | `OpType`                 | string union  | 15-member CRUD operation vocabulary                     |
|  [08]   | `OpMap`                  | mapped type   | `{ [op in OpType]?: number }` per-op count              |
|  [09]   | `DiffKind`               | enum          | property-diff kind (6 members)                          |
|  [10]   | `PropertyDiff`           | interface     | `diffKind: DiffKind`; `inputDiff: boolean`              |
|  [11]   | `SummaryEvent`           | interface     | `resourceChanges: OpMap`; end-of-update summary         |
|  [12]   | `DiagnosticEvent`        | interface     | provider diagnostic message + severity                  |
|  [13]   | `PolicyEvent`            | interface     | CrossGuard policy-violation event                       |

```ts contract
// @pulumi/pulumi/automation — events.d.ts
export declare type OpType =
  | "same" | "create" | "update" | "delete" | "replace"
  | "create-replacement" | "delete-replaced" | "read" | "read-replacement"
  | "refresh" | "discard" | "discard-replaced" | "remove-pending-replace"
  | "import" | "import-replacement"

export declare type OpMap = { [key in OpType]?: number }

export declare enum DiffKind {
  add = "add",
  addReplace = "add-replace",
  delete = "delete",
  deleteReplace = "delete-replace",
  update = "update",
  updateReplace = "update-replace"
}

export interface PropertyDiff {
  diffKind: DiffKind
  inputDiff: boolean
}

export interface StepEventMetadata {
  op: OpType
  urn: string
  type: string
  old?: StepEventStateMetadata
  new?: StepEventStateMetadata
  keys?: string[]
  diffs?: string[]
  detailedDiff?: Record<string, PropertyDiff>
  logical?: boolean
  provider: string
}

export interface StepEventStateMetadata {
  type: string
  urn: string
  custom?: boolean
  delete?: boolean
  id: string
  parent: string
  protect?: boolean
  taint?: boolean
  retainOnDelete?: boolean
  inputs: Record<string, any>
  outputs: Record<string, any>
  provider: string
  initErrors?: string[]
}

export interface ResourcePreEvent {
  metadata: StepEventMetadata
  planning?: boolean
}
export interface ResOutputsEvent {
  metadata: StepEventMetadata
  planning?: boolean
}
export interface ResOpFailedEvent {
  metadata: StepEventMetadata
  status: number
  steps: number
}

export interface SummaryEvent {
  maybeCorrupt: boolean
  durationSeconds: number
  resourceChanges: OpMap
  policyPacks: Record<string, string>
}

export interface DiagnosticEvent {
  urn?: string
  prefix?: string
  message: string
  color: string
  severity: "info" | "info#err" | "warning" | "error"
  streamID?: number
  ephemeral?: boolean
}

export interface PolicyEvent {
  resourceUrn?: string
  message: string
  color: string
  policyName: string
  policyPackName: string
  policyPackVersion: string
  policyPackVersionTag: string
  enforcementLevel: "warning" | "mandatory"
}

export interface EngineEvent {
  sequence: number
  timestamp: number
  cancelEvent?: {}
  stdoutEvent?: { message: string; color: string }
  diagnosticEvent?: DiagnosticEvent
  preludeEvent?: { config: Record<string, string> }
  summaryEvent?: SummaryEvent
  resourcePreEvent?: ResourcePreEvent
  resOutputsEvent?: ResOutputsEvent
  resOpFailedEvent?: ResOpFailedEvent
  policyEvent?: PolicyEvent
  startDebuggingEvent?: { config: Record<string, any> }
}
```

[ENTRYPOINT_SCOPE]: refresh-preview drift driver
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]   | [RAIL]                                         |
| :-----: | :---------------------------------------------------- | :--------------- | :--------------------------------------------- |
|  [01]   | `Stack.previewRefresh(opts?): Promise<PreviewResult>` | lifecycle method | read-only refresh-preview; no state mutation   |
|  [02]   | `Stack.refresh(opts?): Promise<RefreshResult>`        | lifecycle method | reconcile state with provider (mutating)       |
|  [03]   | `RefreshOptions.onEvent`                              | callback         | `(event: EngineEvent) => void` per-step stream |
|  [04]   | `PreviewResult.changeSummary: OpMap`                  | result field     | engine's own per-`OpType` change count         |

```ts contract
// @pulumi/pulumi/automation — Stack (refresh-preview leg)
export interface PreviewResult {
  stdout: string
  stderr: string
  changeSummary: OpMap
}

interface RefreshOptions {
  onEvent?: (event: EngineEvent) => void
  onOutput?: (out: string) => void
  // plus parallel/message/target/expectNoChanges/userAgent/color (shared option fields)
}

declare class Stack {
  preview(opts?: PreviewOptions): Promise<PreviewResult>
  previewRefresh(opts?: RefreshOptions): Promise<PreviewResult>
  refresh(opts?: RefreshOptions): Promise<RefreshResult>
}
```

`previewRefresh` is the non-mutating refresh-preview: it re-reads provider state and reports the drift each resource carries as a stream of `resourcePreEvent` steps, where `StepEventMetadata.op` classifies the divergence and `detailedDiff` carries the per-property delta, without writing the refreshed state back. Drive it with an `onEvent` accumulator and reconcile the folded bucket count against `PreviewResult.changeSummary`.

## [05]-[IMPLEMENTATION_LAW]

[OUTPUT_TOPOLOGY]:
- `Output<T>` is `OutputInstance<T> & Lifted<T>`; property access on an `Output<{a: string}>` returns `Output<string>` directly without `.apply`
- `Input<T>` is `T | Promise<T> | OutputInstance<T>`; all resource arg types accept `Input<T>` fields
- `all()` overloads cover tuple, array, and record shapes; pick the narrowest overload to preserve tuple element types
- `apply()` receives the unwrapped `T` only during `pulumi up` / `preview`; during preview it may not be called if the value is unknown
- `get()` is valid only inside cloud runtime code; calling it during deployment planning throws

[AUTOMATION_TOPOLOGY]:
- `LocalWorkspace` is the only `Workspace` implementation for inline or local-dir programs; remote workspace (`RemoteWorkspace`) drives Pulumi Cloud deployments
- `InlineProgramArgs` carries `{ stackName, projectName, program: PulumiFn }` for in-process programs; `LocalProgramArgs` carries `{ stackName, workDir }` for disk-based programs
- `PulumiFn` is `() => Promise<Record<string, any> | void>`; return object keys become stack outputs
- `UpOptions.onOutput` and `UpOptions.onEvent` deliver streaming event callbacks; useful for progress reporting in CI drivers

[LOCAL_ADMISSION]:
- All resource constructors receive `CustomResourceOptions` or `ComponentResourceOptions`; `parent` sets the ownership hierarchy, `dependsOn` adds explicit ordering, `protect` prevents deletion
- `Config` scopes by project name by default; pass a custom name to read from a separate config namespace
- `StackReference` extends `CustomResource`; it is a tracked resource in the state file and consumes a destroy op

[RAIL_LAW]:
- Package: `@pulumi/pulumi`
- Owns: output algebra, resource model, config, stack references, automation API, error types
- Accept: `Output<T>` for all inter-resource value flow; `Config.requireSecret` for sensitive values
- Reject: direct promise chaining across resource boundaries without `Output.apply`; `Config.get` for secrets
