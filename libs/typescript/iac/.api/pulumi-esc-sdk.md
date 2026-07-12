# [TS_IAC_API_PULUMI_ESC_SDK]

`@pulumi/esc-sdk` is the imperative ESC client — a plain typed API surface, not a provider: `EscApi` over one `Configuration` drives environment CRUD, the open/read session model, YAML and definition-shaped writes with diagnostics, decryption, revisions, and tags, all at `org/project/env` arity. It is the runtime half of the ESC lane: `@pulumi/pulumiservice`'s `Environment` resource owns environment EXISTENCE declaratively, this SDK owns reads and imperative writes from processes that are not a Pulumi program, and stack attachment is the Automation API's own imperative trio (`Stack.addEnvironments`/`listEnvironments`/`removeEnvironment` — no typed `StackSettings` environment field exists). Canonical secret ownership does not move: Doppler is the source of truth on every arm, and an ESC environment composes canonical material through its dynamic-provider opens — ESC is a projection DAG over stores, never a second store.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/esc-sdk`
- package: `@pulumi/esc-sdk`
- license: Apache-2.0
- import: `@pulumi/esc-sdk` → `{ EscApi, EscRawApi, Configuration, DefaultConfiguration, DefaultClient }` + the model types (`EnvironmentDefinition`, `EnvironmentDefinitionValues`, `OpenEnvironment`, `Environment`, `Value`, `EnvironmentDiagnostics`, `CheckEnvironment`, `OrgEnvironments`, `EnvironmentRevision`, `EnvironmentRevisionTag`, `EnvironmentTag`)
- owner: `iac`
- rail: operate / environment (the Pulumi-Cloud lane beside the Doppler canonical store)
- depends-on: nothing Pulumi-runtime — a standalone HTTP client any Node process composes
- capability: environment CRUD and clone, open/read sessions with per-property reads, versioned opens, YAML and definition writes gated by diagnostics, check-before-write validation, decrypt, revision history, revision tags, key/value environment tags
- abi-note: every method is `Promise`-shaped and may resolve `undefined` — the boundary wrap lifts to `Effect.tryPromise` + `Option`; resolved property values arrive as `Value { value, secret?, unknown?, trace }`, the same `{ value, secret }` posture the engine's `OutputMap` carries

## [02]-[CLIENT_SURFACE]

[CONFIG_SCOPE]: one client, one configuration
- rail: environment
- `new EscApi(new Configuration({ accessToken, basePath? }))`; `DefaultConfiguration`/`DefaultClient` are the ambient-token factories. `accessToken` accepts a value, promise, or resolver function — bind it from an Effect `Config.redacted` read, never a literal; `basePath` is the self-hosted API row. `EscRawApi` (`escApi.rawApi`) exposes the generated raw operations when a response header or status matters.

[METHOD_SCOPE]: `EscApi` — the families at `org/project/env` arity
- rail: environment

| [INDEX] | [FAMILY]        | [MEMBERS]                                                                                                                                                                                                                                                                                                                                    |
| :-----: | :-------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | roster          | `listEnvironments(org, continuationToken?)` → `OrgEnvironments`                                                                                                                                                                                                                                                                              |
|  [02]   | definition CRUD | `createEnvironment`, `getEnvironment` → `{ environment: EnvironmentDefinition, yaml }`, `getEnvironmentAtVersion`, `updateEnvironment(org, project, env, values: EnvironmentDefinition)`, `updateEnvironmentYaml(org, project, env, yaml)`, `deleteEnvironment`, `cloneEnvironment(org, srcProject, srcEnv, destProject, destEnv, options?)` |
|  [03]   | open/read       | `openEnvironment` → `OpenEnvironment { id }`, `readOpenEnvironment(org, project, env, openSessionID)`, `openAndReadEnvironment` (the composed spelling), `readOpenEnvironmentProperty(…, openSessionID, property)`, `openEnvironmentAtVersion` / `openAndReadEnvironmentAtVersion`                                                           |
|  [04]   | validation      | `checkEnvironment(org, env: EnvironmentDefinition)` / `checkEnvironmentYaml(org, yaml)` → `CheckEnvironment` with `diagnostics`                                                                                                                                                                                                              |
|  [05]   | operator        | `decryptEnvironment(org, project, env)` — plaintext definition read, operator-only                                                                                                                                                                                                                                                           |
|  [06]   | history         | `listEnvironmentRevisions(…, before?, count?)`; revision tags: `listEnvironmentRevisionTags`, `getEnvironmentRevisionTag`, `createEnvironmentRevisionTag(…, tag, revision)`, `updateEnvironmentRevisionTag`, `deleteEnvironmentRevisionTag`                                                                                                  |
|  [07]   | labels          | `listEnvironmentTags`, `getEnvironmentTag`, `createEnvironmentTag(…, tag, value)`, `updateEnvironmentTag`, `deleteEnvironmentTag`                                                                                                                                                                                                            |

```ts contract
// the definition shape writes travel as — imports is the composition DAG
interface EnvironmentDefinition { imports?: Array<string>; values?: EnvironmentDefinitionValues }
interface EnvironmentDefinitionValues {
  [key: string]: any
  pulumiConfig?: { [key: string]: any }          // → stack config projection
  environmentVariables?: { [key: string]: string } // → process-env projection
  files?: { [key: string]: string }              // → file-mount projection
}
interface OpenEnvironment { id: string; diagnostics?: EnvironmentDiagnostics }
interface Value { value: any; secret?: boolean; unknown?: boolean; trace: Trace }
```

## [03]-[IMPLEMENTATION_LAW]

[ENVIRONMENT_TOPOLOGY]:
- session law: an open is a lease — `openEnvironment` resolves every dynamic provider once and mints the session `id` that `readOpenEnvironment`/`readOpenEnvironmentProperty` consume; `openAndReadEnvironment` is the one-shot spelling for whole-environment reads, the per-property read the narrow one. A `secret`-flagged `Value` lifts to `Redacted` at the boundary — the same secret gate `StackOutputs.read` enforces on the engine's map.
- composition law: `imports` is the environment DAG and the three `values` channels are the projection contract — `pulumiConfig` lands as stack config, `environmentVariables` as process env, `files` as mounted paths; an environment is composition data, and a secret literal authored into a definition instead of resolved through a dynamic-provider open is the second-source defect.
- write law: `checkEnvironment` gates every `updateEnvironment`/`updateEnvironmentYaml` — the returned `diagnostics` are the typed verdict, and a write that skips the check discards the only pre-flight evidence the API offers; `decryptEnvironment` never feeds automation.
- attachment law: a stack consumes environments through `Stack.addEnvironments(...envs)` / `listEnvironments()` / `removeEnvironment(env)` on the Automation-API stack — imperative only; no `StackSettings` field carries environments, so attachment is run data beside `Automation.Options`, never workspace settings.
- writer law: one writer per environment — `@pulumi/pulumiservice`'s `Environment` resource authors an environment that lives in the graph (`.api/pulumi-pulumiservice.md`); this SDK authors environments owned by out-of-graph processes; both writing one environment is drift by construction. Consumers pin through revision tags (`openEnvironmentAtVersion` against a moved tag), so rotation is a tag move, not a consumer edit.
- boundary law: the client is `Promise`-shaped foreign material — wrap in `Effect.tryPromise` with the folder's fault triage, lift `undefined` to `Option`, and source `accessToken` from `Config.redacted` under the same `doppler run` injection the deploy host already obeys.

[RAIL_LAW]:
- Package: `@pulumi/esc-sdk`
- Owns: imperative ESC environment access — CRUD, open/read sessions, validated writes, decrypt, revisions, tags — for processes outside a Pulumi program
- Accept: `openAndReadEnvironment` for whole reads, per-property reads for narrow ones, check-gated writes, revision-tag pinning, `Redacted`-lifted secret values, `Config.redacted`-sourced tokens
- Reject: secret literals in definitions, unchecked writes, dual writers over one environment, environment attachment through workspace settings, raw client calls escaping the `Effect.tryPromise` boundary, `decryptEnvironment` in automation paths
