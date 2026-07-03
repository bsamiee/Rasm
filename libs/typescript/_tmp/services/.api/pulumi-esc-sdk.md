# [API_CATALOGUE] @pulumi/esc-sdk

`@pulumi/esc-sdk` is the TypeScript client for Pulumi ESC (Environments, Secrets, Configuration): a high-level `EscApi` facade over the OpenAPI-generated `EscRawApi` (reachable as `EscApi.rawApi` for un-surfaced operations). It owns environment lifecycle (CRUD + YAML), session-based value resolution (open a session, read the resolved `EnvironmentDefinitionValues` with `Value.secret` marks), and revision/environment tagging. The load-bearing surface for this branch is the session-resolution flow — `new EscApi(new Configuration({ accessToken }))` then `openAndReadEnvironment(org, project, env)` reading `EnvironmentResponse.values` — driven from both the deploy-time `provisioning/drift#PROVISIONING` sweep and the runtime `security/secret#SECRET_STORE` `EscEnv` arm.

- package: `@pulumi/esc-sdk`
- version: `0.14.0`
- license: `Apache-2.0`
- tier: `node` — server-side Bearer-token REST client; the browser owns no ESC token and never reaches this surface.
- rail: secrets
- fidelity: declared — a prepared `iac/secret/inject` row (`@dopplerhq/node-sdk` is the canonical secret provider), NOT an installed dependency. The surface below is verified against the published `0.14.0` `esc/index.d.ts` declarations, not local reflection; re-verify on admission.

## [01]-[PACKAGE_SURFACE]

`EscApi` is the one client; every method returns `Promise<X | undefined>`. `Configuration` is the axios-generated HTTP config (Bearer `accessToken` + `basePath`); the `DefaultConfiguration`/`DefaultClient` factories fold `PULUMI_ACCESS_TOKEN` / `PULUMI_BACKEND_URL` env defaults.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [DESCRIPTION]                                             |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `new EscApi(config: Configuration)`             | client ctor    | high-level client over `EscRawApi`                       |
|  [02]   | `EscApi.rawApi: EscRawApi`                       | field          | the low-level axios client for un-surfaced operations    |
|  [03]   | `DefaultConfiguration(config?): Configuration`  | factory        | merge provided params with env-var defaults              |
|  [04]   | `DefaultClient(config?): EscApi`                 | factory        | `new EscApi(DefaultConfiguration(config))`               |
|  [05]   | `new Configuration(params?)`                    | config ctor    | `{ accessToken?, basePath? }`; `accessToken` is `string \| Promise<string> \| (name?, scopes?) => string \| Promise<string>` |

Re-exported models: `Environment`, `EnvironmentDefinition`, `EnvironmentDefinitionValues`, `OpenEnvironment`, `Value`, `OrgEnvironments`/`OrgEnvironment`, `EnvironmentDiagnostics`, `CheckEnvironment`, `EnvironmentRevision`, `EnvironmentRevisionTag`(`s`), `EnvironmentTag`/`ListEnvironmentTags`, `Pos`/`Range`/`Trace`, `Configuration`, `EscRawApi`.

## [02]-[SESSION_RESOLUTION]

The load-bearing rail: open a session, resolve its values. `OpenEnvironment` carries the session `id`; `EnvironmentResponse` carries `{ environment?, values? }`; a resolved property is a `Value` (`{ value: any; secret?: boolean; unknown?: boolean; trace: Trace }`).

```ts
openEnvironment(orgName, projectName, envName): Promise<OpenEnvironment | undefined>                              // → { id, diagnostics? }
openEnvironmentAtVersion(orgName, projectName, envName, version): Promise<OpenEnvironment | undefined>
readOpenEnvironment(orgName, projectName, envName, openSessionID): Promise<EnvironmentResponse | undefined>       // { environment?, values? }
openAndReadEnvironment(orgName, projectName, envName): Promise<EnvironmentResponse | undefined>                   // open+read convenience
openAndReadEnvironmentAtVersion(orgName, projectName, envName, version): Promise<EnvironmentResponse | undefined>
readOpenEnvironmentProperty(orgName, projectName, envName, openSessionID, property): Promise<EnvironmentPropertyResponse | undefined>  // { property: Value, value }

interface EnvironmentResponse         { environment?: Environment; values?: EnvironmentDefinitionValues }
interface EnvironmentPropertyResponse { property: Value; value: any }
```

- `openAndReadEnvironment` is the whole-environment path; `openEnvironment` → capture `OpenEnvironment.id` → `readOpenEnvironmentProperty(…, id, property)` is the single-key path (session reuse).
- Every read is `| undefined` at the boundary — the `Effect.tryPromise` wrapper must treat an absent response and an absent `values?.[key]` as a fault, never a silent `undefined`.

## [03]-[LIFECYCLE_AND_TAGS]

[LIFECYCLE]: definition CRUD, YAML round-trip, validation, clone, decrypt — the deploy-authoring surface:

```ts
listEnvironments(orgName, continuationToken?): Promise<OrgEnvironments | undefined>
getEnvironment(orgName, projectName, envName): Promise<EnvironmentDefinitionResponse | undefined>                 // { definition, yaml }
getEnvironmentAtVersion(orgName, projectName, envName, version): Promise<EnvironmentDefinitionResponse | undefined>
createEnvironment(orgName, projectName, envName): Promise<void>
updateEnvironment(orgName, projectName, envName, values: EnvironmentDefinition): Promise<EnvironmentDiagnostics | undefined>
updateEnvironmentYaml(orgName, projectName, envName, yaml: string): Promise<EnvironmentDiagnostics | undefined>
deleteEnvironment(orgName, projectName, envName): Promise<void>
cloneEnvironment(orgName, srcProjectName, srcEnvName, destProjectName, destEnvName, cloneOptions?: CloneEnvironmentOptions): Promise<…>
checkEnvironment(orgName, env: EnvironmentDefinition): Promise<CheckEnvironment | undefined>
checkEnvironmentYaml(orgName, yaml: string): Promise<CheckEnvironment | undefined>
decryptEnvironment(orgName, projectName, envName): Promise<EnvironmentDefinitionResponse | undefined>            // secrets in plaintext
// CloneEnvironmentOptions = { preserveHistory?, preserveAccess?, preserveEnvironmentTags?, preserveRevisionTags? }
```

[TAGS]: two parameterized tag families — revision tags (stable pointers to a revision number) and environment tags (key-value metadata). Each is the uniform `list`/`get`/`create`/`update`/`delete` shape:

```ts
listEnvironmentRevisions(orgName, projectName, envName, before?: number, count?: number): Promise<Array<EnvironmentRevision> | undefined>
listEnvironmentRevisionTags(orgName, projectName, envName, after?, count?): Promise<EnvironmentRevisionTags | undefined>
getEnvironmentRevisionTag / createEnvironmentRevisionTag / updateEnvironmentRevisionTag(… tag, revision: number) / deleteEnvironmentRevisionTag(… tag): Promise<…>
listEnvironmentTags(orgName, projectName, envName, after?, count?): Promise<ListEnvironmentTags | undefined>
getEnvironmentTag / createEnvironmentTag(… tag, value) / updateEnvironmentTag(… tag, current_value, new_tag, new_value) / deleteEnvironmentTag(… tag): Promise<…>
```

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `EscApi` wraps `EscRawApi`; drop to `EscApi.rawApi` only for operations the high-level surface does not expose.
- `Configuration.accessToken` accepts a static string, a `Promise<string>`, or a `(name?, scopes?) => string | Promise<string>` — static and dynamic (rotating) token resolution both fit.
- `Configuration.basePath` defaults to the Pulumi Cloud API; set it for self-hosted backends.
- Revision tags are stable pointers to revision numbers; environment tags are key-value metadata not tied to a revision.

[RUNTIME_RESOLUTION]: how the `EscEnv` arm and the drift sweep stack this into one Effect rail.
- Construction: the client is built once at the `security/secret#SECRET_STORE` layer from `new EscApi(new Configuration({ accessToken: Redacted.value(escToken) }))`, `escToken` itself resolved through the ambient `ConfigProvider` (`Config.redacted("PULUMI_ACCESS_TOKEN")`, the `Static` arm) — never a bare `process.env`.
- Resolution: the `EscEnv` arm calls `esc.openAndReadEnvironment(org, project, env)` inside `Effect.tryPromise({ try, catch: (cause) => new SecretFault({ reason: "esc_resolve", … }) })`, reads `resolved?.values?.[property]`, and lifts it into `Redacted.make` so the value never widens to a loggable string; a `Value.secret` mark confirms the redaction. The `provisioning/drift#PROVISIONING` `DriftSweep` calls the same `openAndReadEnvironment` for the policy-pack environment, reading `response.values`.
- Rotation: a new ESC revision is a rotation — the `LeasedSecret` `SubscriptionRef` cache invalidates on the `changes` edge and re-runs `openAndReadEnvironment`, pushing the new value to dependents without a process restart. The deploy-time `provisioning/contract#PROVISIONING` `SecretResolver` and this runtime arm MEET at the one `ConfigProvider` boundary.

[SIBLING_STACK]:
- `@dopplerhq/node-sdk` (`dopplerhq-node-sdk.md`) is the canonical peer arm of the same `SecretRef` `Match.tagsExhaustive` fold; both resolve into one `Redacted.Redacted`, never two parallel secret schemes. On admission this row becomes the second settled provider, not a replacement.
- `effect` owns the `Config.redacted`/`Redacted`/`ConfigProvider` carrier, the `SubscriptionRef` lease cell + `changes` rotation `Stream`, and the `Effect.tryPromise` boundary these `Promise<X | undefined>` methods cross.
- `@pulumi/pulumi` (`pulumi-pulumi.md`) is the deploy-time neighbor whose `automation` `Stack` the drift sweep folds alongside the ESC resolution.

[RAIL_LAW]:
- Package: `@pulumi/esc-sdk`
- Owns: ESC environment lifecycle, session-based value/secret resolution, revision and environment tagging.
- Accept: `EscApi` from an explicit `Configuration` whose `accessToken` is `Redacted`-resolved; `openAndReadEnvironment` (whole environment) or `openEnvironment`+`readOpenEnvironmentProperty` (single key), each wrapped in `Effect.tryPromise` → `SecretFault` and lifted into `Redacted`; treat every `| undefined` boundary as a fault.
- Reject: `EscRawApi` calls the high-level surface already covers; reading a resolved value as a plain `string` instead of `Redacted.Redacted`; settling this as an active row before it is admitted (it is a prepared `iac/secret/inject` identity).
