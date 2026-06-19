# [API_CATALOGUE] @pulumi/esc-sdk

`@pulumi/esc-sdk` supplies `EscApi`, a high-level client for the Pulumi ESC REST API, along with `DefaultConfiguration` and `DefaultClient` factory functions and the model types `Environment`, `OpenEnvironment`, `Value`, `EnvironmentDefinition`, `EnvironmentRevision`, `EnvironmentRevisionTag`, `EnvironmentTag`, `OrgEnvironments`, and `CheckEnvironment` — covering environment lifecycle, session management, YAML-based definition updates, revision tagging, and environment tagging.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/esc-sdk`
- package: `@pulumi/esc-sdk`
- module: `@pulumi/esc-sdk` (root re-exports `esc/index`)
- asset: ESC API client, environment lifecycle, session and value resolution, revision and tag management
- rail: secrets

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and configuration family
- rail: secrets

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :--------------------------- | :------------ | :--------------------------------------------- |
|   [1]   | `EscApi`                     | class         | high-level ESC API client wrapping `EscRawApi` |
|   [2]   | `Configuration`              | class         | HTTP client config: `accessToken`, `basePath`  |
|   [3]   | `ConfigurationParameters`    | interface     | `accessToken`, `basePath` constructor params   |
|   [4]   | `EscRawApi` (as `EscRawApi`) | class         | low-level axios-based API client               |

[PUBLIC_TYPE_SCOPE]: environment model family
- rail: secrets

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                                                |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------------------- |
|   [1]   | `Environment`                 | interface     | `exprs?`, `properties?`, `schema?`, `executionContext?`               |
|   [2]   | `OpenEnvironment`             | interface     | `id: string` (session ID), `diagnostics?`                             |
|   [3]   | `EnvironmentDefinition`       | interface     | structured definition object for CRUD operations                      |
|   [4]   | `EnvironmentDefinitionValues` | interface     | resolved values map from an open session                              |
|   [5]   | `EnvironmentDiagnostics`      | interface     | validation diagnostics returned by check/update operations            |
|   [6]   | `CheckEnvironment`            | interface     | result of a check operation                                           |
|   [7]   | `Value`                       | interface     | `value: any`, `secret?: boolean`, `unknown?: boolean`, `trace: Trace` |
|   [8]   | `OrgEnvironments`             | interface     | paginated list of org environments                                    |
|   [9]   | `OrgEnvironment`              | interface     | single org environment descriptor                                     |
|  [10]   | `EnvironmentRevision`         | interface     | revision metadata                                                     |
|  [11]   | `EnvironmentRevisionTag`      | interface     | tag pointing to a revision number                                     |
|  [12]   | `EnvironmentRevisionTags`     | interface     | paginated list of revision tags                                       |
|  [13]   | `EnvironmentTag`              | interface     | key-value environment tag                                             |

[PUBLIC_TYPE_SCOPE]: response and option family
- rail: secrets

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                                                                                     |
| :-----: | :------------------------------ | :------------ | :----------------------------------------------------------------------------------------- |
|   [1]   | `EnvironmentDefinitionResponse` | interface     | `definition: EnvironmentDefinition`, `yaml: string`                                        |
|   [2]   | `EnvironmentResponse`           | interface     | `environment?: Environment`, `values?: EnvironmentDefinitionValues`                        |
|   [3]   | `EnvironmentPropertyResponse`   | interface     | `property: Value`, `value: any`                                                            |
|   [4]   | `CloneEnvironmentOptions`       | interface     | `preserveHistory?`, `preserveAccess?`, `preserveEnvironmentTags?`, `preserveRevisionTags?` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction
- rail: secrets

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------- | :------------- | :-------------------------------------------- |
|   [1]   | `new EscApi(config: Configuration)` | client init    | high-level client from explicit configuration |
|   [2]   | `DefaultConfiguration(config?)`     | factory        | merges provided params with env-var defaults  |
|   [3]   | `DefaultClient(config?)`            | factory        | `new EscApi(DefaultConfiguration(config))`    |

[ENTRYPOINT_SCOPE]: environment lifecycle operations
- rail: secrets

| [INDEX] | [SURFACE]                                                                    | [ENTRY_FAMILY]    | [RAIL]                                        |
| :-----: | :--------------------------------------------------------------------------- | :---------------- | :-------------------------------------------- |
|   [1]   | `listEnvironments(orgName, continuationToken?)`                              | list              | paginated org environment list                |
|   [2]   | `getEnvironment(orgName, projectName, envName)`                              | read              | definition + YAML string                      |
|   [3]   | `getEnvironmentAtVersion(orgName, projectName, envName, version)`            | read at version   | definition + YAML at revision                 |
|   [4]   | `createEnvironment(orgName, projectName, envName)`                           | create            | create empty environment                      |
|   [5]   | `updateEnvironment(orgName, projectName, envName, values)`                   | update            | replace definition, returns diagnostics       |
|   [6]   | `updateEnvironmentYaml(orgName, projectName, envName, yaml)`                 | update via YAML   | replace definition from YAML string           |
|   [7]   | `deleteEnvironment(orgName, projectName, envName)`                           | delete            | remove environment                            |
|   [8]   | `cloneEnvironment(orgName, srcProject, srcEnv, destProject, destEnv, opts?)` | clone             | copy environment with optional history/access |
|   [9]   | `checkEnvironment(orgName, env)`                                             | validate          | check definition for errors                   |
|  [10]   | `checkEnvironmentYaml(orgName, yaml)`                                        | validate via YAML | check YAML string for errors                  |
|  [11]   | `decryptEnvironment(orgName, projectName, envName)`                          | decrypt           | definition with secrets in plaintext          |

[ENTRYPOINT_SCOPE]: session and value resolution operations
- rail: secrets

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY]         | [RAIL]                                                 |
| :-----: | :------------------------------------------------------------------------------------ | :--------------------- | :----------------------------------------------------- |
|   [1]   | `openEnvironment(orgName, projectName, envName)`                                      | open session           | returns `OpenEnvironment` with session `id`            |
|   [2]   | `openEnvironmentAtVersion(orgName, projectName, envName, version)`                    | open at version        | session at a specific revision                         |
|   [3]   | `readOpenEnvironment(orgName, projectName, envName, openSessionID)`                   | read session           | resolved `Environment` + `EnvironmentDefinitionValues` |
|   [4]   | `openAndReadEnvironment(orgName, projectName, envName)`                               | open + read            | convenience: open then read in one call                |
|   [5]   | `openAndReadEnvironmentAtVersion(orgName, projectName, envName, version)`             | open + read at version | versioned convenience open-and-read                    |
|   [6]   | `readOpenEnvironmentProperty(orgName, projectName, envName, openSessionID, property)` | read property          | single resolved `Value`                                |

[ENTRYPOINT_SCOPE]: revision and tag management
- rail: secrets

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY]      | [RAIL]                          |
| :-----: | :----------------------------------------------------------------------------------------- | :------------------ | :------------------------------ |
|   [1]   | `listEnvironmentRevisions(orgName, projectName, envName, before?, count?)`                 | list revisions      | paginated revision history      |
|   [2]   | `listEnvironmentRevisionTags(orgName, projectName, envName, after?, count?)`               | list revision tags  | paginated revision tag list     |
|   [3]   | `getEnvironmentRevisionTag(orgName, projectName, envName, tag)`                            | get revision tag    | single `EnvironmentRevisionTag` |
|   [4]   | `createEnvironmentRevisionTag(orgName, projectName, envName, tag, revision)`               | create revision tag | pin tag to revision number      |
|   [5]   | `updateEnvironmentRevisionTag(orgName, projectName, envName, tag, revision)`               | update revision tag | move tag to new revision        |
|   [6]   | `deleteEnvironmentRevisionTag(orgName, projectName, envName, tag)`                         | delete revision tag | remove revision tag             |
|   [7]   | `listEnvironmentTags(orgName, projectName, envName, after?, count?)`                       | list env tags       | paginated environment tag list  |
|   [8]   | `getEnvironmentTag(orgName, projectName, envName, tag)`                                    | get env tag         | single `EnvironmentTag`         |
|   [9]   | `createEnvironmentTag(orgName, projectName, envName, tag, value)`                          | create env tag      | add key-value tag               |
|  [10]   | `updateEnvironmentTag(orgName, projectName, envName, tag, currentValue, newTag, newValue)` | update env tag      | rename or revalue tag           |
|  [11]   | `deleteEnvironmentTag(orgName, projectName, envName, tag)`                                 | delete env tag      | remove key-value tag            |

## [4]-[IMPLEMENTATION_LAW]

[ESC_TOPOLOGY]:
- `EscApi` wraps `EscRawApi`; the raw client is accessible via `EscApi.rawApi` for operations not yet exposed on the high-level surface
- `Configuration.accessToken` accepts a string, a `Promise<string>`, or a function returning a string/promise — supports both static and dynamic token resolution
- `Configuration.basePath` defaults to the Pulumi Cloud API base URL; set it explicitly for self-hosted Pulumi deployments
- session flow: `openEnvironment` → capture `OpenEnvironment.id` → `readOpenEnvironment(…, sessionID)` to resolve secrets and config values; or use the `openAndReadEnvironment` convenience method
- `Value.secret` marks a resolved property as secret; `Value.unknown` indicates a value that could not be resolved at check time
- revision tags are stable pointers to revision numbers; environment tags are key-value metadata not tied to a specific revision

[LOCAL_ADMISSION]:
- Use `DefaultClient()` when `PULUMI_ACCESS_TOKEN` and the default base URL apply; pass an explicit `Configuration` for token rotation or non-default endpoints.
- `openAndReadEnvironment` is the standard path for runtime config/secret injection; `readOpenEnvironmentProperty` is for targeted single-key reads.

[RUNTIME_RESOLUTION]:
- Consumed deploy-time by `provisioning/contract#PROVISIONING` and RUNTIME-side by the `secrets/secret-store#SECRET_STORE` `SecretStore` `EscEnv` arm — the same `EscApi` session flow resolves secrets at runtime as at deploy-time, so the two MEET at the one `ConfigProvider` boundary.
- The runtime read is the `openAndReadEnvironment(orgName, projectName, envName)` convenience (open + read in one call) when the whole environment resolves at once, or `openEnvironment` → capture `OpenEnvironment.id` → `readOpenEnvironmentProperty(…, sessionID, property)` for a single targeted key; the resolved `Value.secret`-marked properties carry into `Config.redacted` so the secret never enters a span or log.
- A rotation is a new ESC revision; the `SecretStore` `LeasedSecret` cache invalidates its lease on the `SubscriptionRef.changes` edge and re-runs `openAndReadEnvironment` to re-resolve, never a process restart.

[RAIL_LAW]:
- Package: `@pulumi/esc-sdk`
- Owns: ESC environment lifecycle, session-based secret resolution, revision and tag management
- Accept: `EscApi` with `DefaultConfiguration` for standard token auth; `Configuration` with explicit `accessToken` for rotation or testing
- Reject: direct calls to `EscRawApi` methods when the high-level `EscApi` surface covers the operation
