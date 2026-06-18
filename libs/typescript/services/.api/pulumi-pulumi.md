# [API_CATALOGUE] @pulumi/pulumi

`@pulumi/pulumi` supplies the core Pulumi SDK: the `Output<T>` / `Input<T>` algebra, `Resource`, `CustomResource`, `ComponentResource`, `StackReference`, `Config`, typed error classes, module namespaces for `asset`, `automation`, `log`, `runtime`, and `provider`, and the full Automation API (`LocalWorkspace`, `Stack`) for programmatic up/preview/destroy/refresh lifecycle drives in the services deploy tier.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumi`
- package: `@pulumi/pulumi`
- module: `@pulumi/pulumi` (main), `@pulumi/pulumi/automation` (programmatic API)
- asset: output algebra, resource model, config, stack references, automation SDK, error types
- rail: deployment

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: output algebra family
- rail: deployment

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]    | [RAIL]                                    |
| :-----: | :------------------ | :--------------- | :---------------------------------------- |
|   [1]   | `Output<T>`         | async value type | `OutputInstance<T> & Lifted<T>`           |
|   [2]   | `OutputInstance<T>` | interface        | `apply`, `get` contract                   |
|   [3]   | `Input<T>`          | type alias       | `T \| Promise<T> \| OutputInstance<T>`    |
|   [4]   | `Inputs`            | type alias       | `Record<string, Input<any>>`              |
|   [5]   | `Unwrap<T>`         | utility type     | deeply unwrap `Promise` and `Output` nest |
|   [6]   | `Lifted<T>`         | utility type     | property-lifted `Output` projection       |

[PUBLIC_TYPE_SCOPE]: resource class family
- rail: deployment

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [RAIL]                                      |
| :-----: | :------------------------- | :------------- | :------------------------------------------ |
|   [1]   | `Resource`                 | abstract class | base; holds `urn: Output<URN>`              |
|   [2]   | `CustomResource`           | abstract class | extends `Resource`; holds `id: Output<ID>`  |
|   [3]   | `ComponentResource<TData>` | class          | extends `Resource`; logical grouping owner  |
|   [4]   | `ProviderResource`         | abstract class | extends `CustomResource`; CRUD provider     |
|   [5]   | `StackReference`           | class          | extends `CustomResource`; cross-stack reads |
|   [6]   | `ResourceHook`             | class          | lifecycle hook binding                      |
|   [7]   | `ErrorHook`                | class          | error lifecycle hook                        |

[PUBLIC_TYPE_SCOPE]: options family
- rail: deployment

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :------------------------- | :------------ | :--------------------------------------------------- |
|   [1]   | `ResourceOptions`          | interface     | `parent`, `dependsOn`, `protect`, `ignoreChanges`    |
|   [2]   | `CustomResourceOptions`    | interface     | extends `ResourceOptions`; adds `import`, `id`       |
|   [3]   | `ComponentResourceOptions` | interface     | extends `ResourceOptions`; adds `providers`          |
|   [4]   | `CustomTimeouts`           | interface     | `create`, `update`, `delete`, `read` timeout strings |
|   [5]   | `Alias`                    | interface     | prior name descriptor for resource rename            |

[PUBLIC_TYPE_SCOPE]: config class
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                               |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------- |
|   [1]   | `Config`              | class         | typed config and secret resolution                   |
|   [2]   | `StringConfigOptions` | interface     | `allowedValues`, `minLength`, `maxLength`, `pattern` |
|   [3]   | `NumberConfigOptions` | interface     | `min`, `max` bounds                                  |

[PUBLIC_TYPE_SCOPE]: error class family
- rail: deployment

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                                   |
| :-----: | :--------------------- | :------------ | :--------------------------------------- |
|   [1]   | `RunError`             | error class   | clean abort without stack trace          |
|   [2]   | `ResourceError`        | error class   | resource-associated abort                |
|   [3]   | `InputPropertyError`   | error class   | single input property validation failure |
|   [4]   | `InputPropertiesError` | error class   | multi-property validation failure        |

[PUBLIC_TYPE_SCOPE]: automation API family
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------- | :------------ | :------------------------------------- |
|   [1]   | `LocalWorkspace`        | class         | `Workspace` implementation; local CLI  |
|   [2]   | `Stack`                 | class         | up / preview / refresh / destroy owner |
|   [3]   | `LocalWorkspaceOptions` | options type  | `workDir`, `envVars`, `program`        |
|   [4]   | `UpResult`              | result type   | summary + outputs map                  |
|   [5]   | `PreviewResult`         | result type   | change summary                         |
|   [6]   | `DestroyResult`         | result type   | resource removal summary               |
|   [7]   | `RefreshResult`         | result type   | state reconciliation summary           |
|   [8]   | `OutputMap`             | type alias    | `Record<string, OutputValue>`          |
|   [9]   | `OutputValue`           | value type    | `{ value: any; secret: boolean }`      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: output algebra functions
- rail: deployment

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY]    | [RAIL]                                |
| :-----: | :-------------------------------------------- | :---------------- | :------------------------------------ |
|   [1]   | `output<T>(val: Input<T>): Output<Unwrap<T>>` | lift              | wraps plain or promised value         |
|   [2]   | `secret<T>(val: Input<T>): Output<Unwrap<T>>` | lift              | wraps value as secret output          |
|   [3]   | `unsecret<T>(val: Output<T>): Output<T>`      | strip             | removes secret marking                |
|   [4]   | `isSecret<T>(val: Output<T>): Promise<bool>`  | predicate         | tests secret flag                     |
|   [5]   | `all(vals): Output<...>`                      | combinator        | joins heterogeneous input tuple       |
|   [6]   | `concat(...params): Output<string>`           | string combinator | stringifies and concatenates inputs   |
|   [7]   | `interpolate(literals, ...): Output<string>`  | tagged template   | template-literal output interpolation |
|   [8]   | `jsonStringify(obj, ...): Output<string>`     | serializer        | `JSON.stringify` over `Input<any>`    |
|   [9]   | `jsonParse(text, ...): Output<any>`           | deserializer      | `JSON.parse` over `Input<string>`     |
|  [10]   | `deferredOutput<T>()`                         | deferred factory  | `[Output<T>, resolver]` pair          |
|  [11]   | `Output.apply<U>(fn): Output<U>`              | instance method   | transform with dependency tracking    |
|  [12]   | `Output.get(): T`                             | instance method   | unwrap at cloud runtime only          |
|  [13]   | `Output.create<T>(val): Output<Unwrap<T>>`    | static factory    | construct output from input           |
|  [14]   | `Output.isInstance(obj): obj is Output<T>`    | static predicate  | runtime type guard                    |

[ENTRYPOINT_SCOPE]: resource construction
- rail: deployment

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY]  | [RAIL]                           |
| :-----: | :----------------------------------------------------------------- | :-------------- | :------------------------------- |
|   [1]   | `new CustomResource(t, name, props?, opts?)`                       | constructor     | leaf managed resource            |
|   [2]   | `new ComponentResource(type, name, args?, opts?)`                  | constructor     | logical grouping owner           |
|   [3]   | `ComponentResource.registerOutputs(outputs?)`                      | instance method | declare output map for component |
|   [4]   | `new StackReference(name, args?, opts?)`                           | constructor     | cross-stack output reader        |
|   [5]   | `StackReference.getOutput(name): Output<any>`                      | instance method | fetch output or `undefined`      |
|   [6]   | `StackReference.requireOutput(name): Output<any>`                  | instance method | fetch output or throw            |
|   [7]   | `StackReference.getOutputDetails(name): Promise<OutputDetails>`    | instance method | value + secret flag              |
|   [8]   | `createUrn(name, type, parent?, project?, stack?): Output<string>` | utility         | compute resource URN             |

[ENTRYPOINT_SCOPE]: Config resolution
- rail: deployment

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :----------------------------------------------------- | :------------- | :-------------------------------- |
|   [1]   | `new Config(name?)`                                    | constructor    | scoped config bag by project name |
|   [2]   | `Config.get(key, opts?): K \| undefined`               | optional read  | string value or `undefined`       |
|   [3]   | `Config.require(key, opts?): K`                        | required read  | throws if absent                  |
|   [4]   | `Config.getSecret(key, opts?): Output<K> \| undefined` | secret read    | secret output or `undefined`      |
|   [5]   | `Config.requireSecret(key, opts?): Output<K>`          | secret read    | required secret output            |
|   [6]   | `Config.getBoolean / requireBoolean`                   | typed read     | boolean resolution                |
|   [7]   | `Config.getNumber / requireNumber`                     | typed read     | number resolution                 |
|   [8]   | `Config.getObject<T> / requireObject<T>`               | typed read     | JSON-deserialized object          |

[ENTRYPOINT_SCOPE]: Automation API lifecycle
- rail: deployment
- module: `@pulumi/pulumi/automation`

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]   | [RAIL]                             |
| :-----: | :---------------------------------------------------------------- | :--------------- | :--------------------------------- |
|   [1]   | `LocalWorkspace.create(opts): Promise<LocalWorkspace>`            | factory          | workspace from options             |
|   [2]   | `LocalWorkspace.createStack(args, opts?): Promise<Stack>`         | factory          | new stack; fails if already exists |
|   [3]   | `LocalWorkspace.selectStack(args, opts?): Promise<Stack>`         | factory          | existing stack; fails if absent    |
|   [4]   | `LocalWorkspace.createOrSelectStack(args, opts?): Promise<Stack>` | factory          | idempotent stack selection         |
|   [5]   | `Stack.create(name, workspace): Promise<Stack>`                   | factory          | construct from existing workspace  |
|   [6]   | `Stack.select(name, workspace): Promise<Stack>`                   | factory          | select from existing workspace     |
|   [7]   | `Stack.createOrSelect(name, workspace): Promise<Stack>`           | factory          | idempotent construction            |
|   [8]   | `Stack.up(opts?): Promise<UpResult>`                              | lifecycle method | deploy or update                   |
|   [9]   | `Stack.preview(opts?): Promise<PreviewResult>`                    | lifecycle method | dry-run diff                       |
|  [10]   | `Stack.destroy(opts?): Promise<DestroyResult>`                    | lifecycle method | delete all resources               |
|  [11]   | `Stack.refresh(opts?): Promise<RefreshResult>`                    | lifecycle method | reconcile state with provider      |
|  [12]   | `Stack.outputs(): Promise<OutputMap>`                             | query method     | current stack output values        |
|  [13]   | `Stack.setConfig(key, value, path?): Promise<void>`               | config method    | set config key-value               |
|  [14]   | `Stack.getAllConfig(): Promise<ConfigMap>`                        | config method    | full config map                    |

## [4]-[IMPLEMENTATION_LAW]

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
