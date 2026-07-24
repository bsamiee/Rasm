# [TS_IAC_API_PULUMI_ESC_SDK]

`@pulumi/esc-sdk` drives ESC environments imperatively: one `EscApi` over a `Configuration` owns environment CRUD, the open-read session model, diagnostic-gated writes, decrypt, revisions, and tags at `org/project/env` arity, for any Node process outside a running Pulumi program. `@pulumi/pulumiservice` authors environment existence in the graph while this SDK reads and writes out-of-graph; Doppler stays the canonical store and an ESC environment projects over it, never a second store.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/esc-sdk`
- package: `@pulumi/esc-sdk` (Apache-2.0)
- module: `@pulumi/esc-sdk` CJS root; generated raw client at `escApi.rawApi`
- runtime: `node` — standalone HTTP client, no Pulumi-runtime dependency
- rail: operate / environment

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the ESC wire models and the client configuration

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :------------------------------ | :------------ | :---------------------------- |
|  [01]   | `Configuration`                 | class         | client access-token holder    |
|  [02]   | `EnvironmentDefinition`         | interface     | the environment document      |
|  [03]   | `EnvironmentDefinitionValues`   | interface     | three projection channels     |
|  [04]   | `Value`                         | interface     | resolved property with secret |
|  [05]   | `OpenEnvironment`               | interface     | open read-session identity    |
|  [06]   | `EnvironmentResponse`           | interface     | opened environment payload    |
|  [07]   | `EnvironmentDefinitionResponse` | interface     | definition with yaml          |
|  [08]   | `EnvironmentPropertyResponse`   | interface     | single-property payload       |
|  [09]   | `CheckEnvironment`              | interface     | validation verdict            |
|  [10]   | `EnvironmentDiagnostics`        | interface     | typed write verdict           |
|  [11]   | `OrgEnvironments`               | interface     | paged roster payload          |
|  [12]   | `EnvironmentRevision`           | interface     | revision history entry        |
|  [13]   | `EnvironmentRevisionTag`        | interface     | named revision pin            |
|  [14]   | `EnvironmentTag`                | interface     | key-value environment tag     |
|  [15]   | `CloneEnvironmentOptions`       | interface     | clone preserve flags          |

- `EnvironmentDefinition`: `imports` is the merge DAG, `values` the projection payload over `pulumiConfig`/`environmentVariables`/`files` with free string keys.
- `Value`: a set `secret` flag lifts the property to `Redacted` at the boundary; `trace` carries source spans.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and the `EscApi` methods

Every method resolves a `Promise` that may be `undefined` and takes `(org, project, env, …)` arity; rows show only the trailing args that differ and the resolved shape. `checkEnvironment`, `checkEnvironmentYaml`, `listEnvironments`, and `cloneEnvironment` break the `project`/`env` arity.

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :--------------------------- |
|  [01]   | `new EscApi(Configuration)`                                                           | ctor     | client over one config       |
|  [02]   | `new Configuration({ accessToken, basePath? })`                                       | ctor     | access-token config          |
|  [03]   | `DefaultConfiguration(config?) -> Configuration`                                      | factory  | ambient-token config         |
|  [04]   | `DefaultClient(config?) -> EscApi`                                                    | factory  | ambient-token client         |
|  [05]   | `escApi.rawApi`                                                                       | property | raw generated operations     |
|  [06]   | `listEnvironments(continuationToken?) -> OrgEnvironments`                             | instance | paged environment roster     |
|  [07]   | `getEnvironment -> EnvironmentDefinitionResponse`                                     | instance | full definition read         |
|  [08]   | `getEnvironmentAtVersion(version)`                                                    | instance | pinned definition read       |
|  [09]   | `createEnvironment -> void`                                                           | instance | mint an empty environment    |
|  [10]   | `cloneEnvironment(srcProject, srcEnv, destProject, destEnv, opts?)`                   | instance | copy with preserve flags     |
|  [11]   | `updateEnvironment(EnvironmentDefinition) -> EnvironmentDiagnostics`                  | instance | definition write             |
|  [12]   | `updateEnvironmentYaml(yaml) -> EnvironmentDiagnostics`                               | instance | yaml write                   |
|  [13]   | `deleteEnvironment -> void`                                                           | instance | remove the environment       |
|  [14]   | `openEnvironment -> OpenEnvironment`                                                  | instance | mint a read session          |
|  [15]   | `openEnvironmentAtVersion(version)`                                                   | instance | pinned read session          |
|  [16]   | `readOpenEnvironment(openSessionID) -> EnvironmentResponse`                           | instance | whole-session read           |
|  [17]   | `readOpenEnvironmentProperty(openSessionID, property) -> EnvironmentPropertyResponse` | instance | single-property read         |
|  [18]   | `openAndReadEnvironment -> EnvironmentResponse`                                       | instance | one-shot open-read           |
|  [19]   | `openAndReadEnvironmentAtVersion(version)`                                            | instance | pinned one-shot read         |
|  [20]   | `checkEnvironment(env) -> CheckEnvironment`                                           | instance | pre-write validation         |
|  [21]   | `checkEnvironmentYaml(yaml) -> CheckEnvironment`                                      | instance | pre-write yaml validation    |
|  [22]   | `decryptEnvironment -> EnvironmentDefinitionResponse`                                 | instance | operator-only plaintext read |
|  [23]   | `listEnvironmentRevisions(before?, count?) -> EnvironmentRevision[]`                  | instance | revision history             |

[revision tag]: `listEnvironmentRevisionTags` `getEnvironmentRevisionTag` `createEnvironmentRevisionTag(tag, revision)` `updateEnvironmentRevisionTag` `deleteEnvironmentRevisionTag`
[environment tag]: `listEnvironmentTags` `getEnvironmentTag` `createEnvironmentTag(tag, value)` `updateEnvironmentTag` `deleteEnvironmentTag`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- session law: `openEnvironment` resolves every dynamic provider once and mints the session `id` that `readOpenEnvironment`/`readOpenEnvironmentProperty` consume; `openAndReadEnvironment` folds open and read for whole reads, the per-property read serves the narrow one.
- projection law: `imports` is the environment DAG and `values` fans three channels — `pulumiConfig` to stack config, `environmentVariables` to process env, `files` to mounted paths; a secret literal authored into a definition rather than resolved through a dynamic-provider open is the second-source defect.
- write law: `checkEnvironment`/`checkEnvironmentYaml` gate every `updateEnvironment`/`updateEnvironmentYaml`, and the returned `EnvironmentDiagnostics` are the sole pre-flight verdict; `decryptEnvironment` yields plaintext an automation path never reads.
- attachment law: a stack consumes environments through `Stack.addEnvironments(...envs)`/`listEnvironments()`/`removeEnvironment(env)` on the Automation-API stack; no workspace-settings field carries environments, so attachment is imperative run data.

[STACKING]:
- `@pulumi/pulumiservice`(`.api/pulumi-pulumiservice.md`): `Environment` authors graph-owned environment existence, this SDK's writes own out-of-graph ones, and `EnvironmentVersionTag` ↔ `openEnvironmentAtVersion` against a moved tag pins consumers — both writing one environment is drift.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): Doppler is the canonical store an ESC definition composes through dynamic-provider opens; `accessToken` binds from the same `Config.redacted` under `doppler run` the deploy host obeys.
- within-lib: every `Promise` method folds through `Effect.tryPromise` with the folder fault triage, `undefined` lifts to `Option`, and a `secret`-flagged `Value` lifts to `Redacted` — the gate `StackOutputs.read` enforces on the engine `OutputMap`.

[LOCAL_ADMISSION]:
- Admit only the wrapped client: an `EscApi` call enters through `Effect.tryPromise`, `undefined` lifts to `Option`, and `accessToken` sources from `Config.redacted`; a raw `Promise` escaping the boundary is refused.

[RAIL_LAW]:
- Package: `@pulumi/esc-sdk`
- Owns: imperative ESC environment access for out-of-graph processes — CRUD, open/read sessions, diagnostic-gated writes, decrypt, revisions, tags
- Accept: `openAndReadEnvironment` for whole reads, per-property reads for narrow ones, check-gated writes, revision-tag pinning, `Redacted`-lifted secrets, `Config.redacted` tokens
- Reject: secret literals in definitions, unchecked writes, dual writers over one environment, attachment through workspace settings, raw calls escaping `Effect.tryPromise`, `decryptEnvironment` in automation paths
