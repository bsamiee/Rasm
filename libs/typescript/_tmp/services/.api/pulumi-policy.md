# [API_CATALOGUE] @pulumi/policy

`@pulumi/policy` is the CrossGuard policy-as-code SDK: a `PolicyPack` registers a set of `ResourceValidationPolicy` (per-resource) and `StackValidationPolicy` (cross-resource) rules that the Pulumi CLI runs in a separate process during preview and update. Each rule carries an `EnforcementLevel` (`advisory` reports, `mandatory` blocks, `remediate` auto-fixes before blocking, `disabled` suppresses), rich metadata (`severity`, `framework`, `configSchema`, `remediationSteps`, `tags`, `url`), and one of three callbacks: `validateResource` (flag via `reportViolation`), `remediateResource` (return replacement props applied before provisioning), or `validateStack` (fold over the whole resolved resource graph). The `validateResourceOfType`/`remediateResourceOfType`/`validateRemediateResourceOfType`/`validateStackResourcesOfType` helpers turn a `@pulumi/pulumi` resource class into a strongly-typed, class-filtered callback so a rule reads the provider's own args type rather than an untyped bag. `Secret` marks a remediated value for engine-side encryption. The load-bearing services use is the `provisioning/contract#PROVISIONING` `PolicyGuard`: a per-`DeployMode` rule set (no public object-store bucket, encryption-at-rest, tag presence) that fails the deploy at engine time rather than a runtime audit.

- package: `@pulumi/policy`
- version: `1.21.0`
- license: `Apache-2.0`
- tier: `node` — deploy-time only, an out-of-band policy-pack process the CLI invokes; lives in its own `PulumiPolicy.yaml` project, never bundled with the infrastructure program, never on the durable runtime hot path, never browser-reachable.
- rail: policy

## [01]-[PACKAGE_SURFACE]

`PolicyPack` is the container; the two policy interfaces are the rule kinds; the callback + args families are the evaluation contract. `index.d.ts` re-exports the `policy` module and the (under-declared, type-invisible) `./proxy` unknown-value helpers — the typed surface is the `policy` module below.

| [INDEX] | [SYMBOL]                                        | [KIND]           | [ROLE]                                                              |
| :-----: | :---------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `new PolicyPack(name, args: PolicyPackArgs, initialConfig?: PolicyPackConfig)` | class ctor       | registers all `policies` under `name`; the pack entry             |
|  [02]   | `PolicyPackArgs`                                 | args interface   | `policies`, `enforcementLevel?`, `description?`, `displayName?`, `readme?`, `provider?`, `tags?`, `repository?` |
|  [03]   | `EnforcementLevel`                              | string union     | `"advisory" \| "mandatory" \| "remediate" \| "disabled"` — pack default, per-policy override |
|  [04]   | `Severity`                                       | string union     | `"low" \| "medium" \| "high" \| "critical"`                         |
|  [05]   | `Policies`                                        | type alias       | `(ResourceValidationPolicy \| StackValidationPolicy)[]`             |
|  [06]   | `PolicyPackConfig`                                | type alias       | `{ [policy]: EnforcementLevel \| ({ enforcementLevel? } & Record<string, any>) }` — per-policy config bag |
|  [07]   | `Secret`                                          | class            | `new Secret(value)`; `.value` is the plaintext the engine encrypts when returned from a remediation |
|  [08]   | `Policy`                                           | base interface   | shared rule metadata (below); both policy kinds extend it          |
|  [09]   | `ResourceValidationPolicy` / `StackValidationPolicy` | policy interfaces | extend `Policy`; carry the validate/remediate callbacks           |

## [02]-[POLICY_MODEL]

A rule is `Policy` metadata plus a callback. `ResourceValidationPolicy` runs per-resource and may carry BOTH `validateResource` (flag) and `remediateResource` (fix); remediations run before validations. `validateResource` accepts one callback or an ordered array. `StackValidationPolicy` runs once over the fully-resolved resource graph.

| [INDEX] | [SYMBOL]                    | [KIND]           | [SHAPE]                                                                                                       |
| :-----: | :-------------------------- | :--------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `Policy`                    | base interface   | `name`, `description`, `enforcementLevel?`, `severity?`, `configSchema?`, `displayName?`, `framework?`, `tags?`, `remediationSteps?`, `url?` |
|  [02]   | `ResourceValidationPolicy`  | policy interface | extends `Policy`; `validateResource?: ResourceValidation \| ResourceValidation[]`, `remediateResource?: ResourceRemediation` |
|  [03]   | `StackValidationPolicy`     | policy interface | extends `Policy`; `validateStack: StackValidation`                                                            |
|  [04]   | `PolicyComplianceFramework` | interface        | `name`, `version`, `reference`, `specification` — the `framework?` value                                     |
|  [05]   | `ResourceValidation`        | callback         | `(args: ResourceValidationArgs, reportViolation: ReportViolation) => Promise<void> \| void`                   |
|  [06]   | `ResourceRemediation`       | callback         | `(args: ResourceValidationArgs) => Record<string, any> \| void \| Promise<Record<string, any>> \| Promise<void> \| undefined` |
|  [07]   | `StackValidation`           | callback         | `(args: StackValidationArgs, reportViolation: ReportViolation) => Promise<void> \| void`                      |
|  [08]   | `ReportViolation`           | callback         | `(message: string, urn?: string) => void` — call repeatedly for multiple violations                          |
|  [09]   | `ResourceValidationArgs`    | args interface   | `type`, `props`, `urn`, `name`, `opts: PolicyResourceOptions`, `provider?: PolicyProviderResource`, `stackTags: ReadonlyMap<string,string>`, `isType<T>()`, `asType<T,A>()`, `getConfig<T>()`, `notApplicable(reason?)` |
|  [10]   | `StackValidationArgs`       | args interface   | `resources: PolicyResource[]`, `stackTags`, `getConfig<T>()`, `notApplicable(reason?)`                        |
|  [11]   | `PolicyResource`            | interface        | `type`, `props`, `urn`, `name`, `opts`, `provider?`, `parent?: PolicyResource`, `dependencies: PolicyResource[]`, `propertyDependencies: Record<string, PolicyResource[]>`, `isType<T>()`, `asType<T>()` |

```ts contract
// @pulumi/policy — the per-resource arg bag; `isType`/`asType` narrow by @pulumi/pulumi resource class
interface ResourceValidationArgs {
  type: string
  props: Record<string, any>                                              // the resource inputs
  urn: string; name: string
  opts: PolicyResourceOptions                                             // resolved options bag ([04])
  provider?: PolicyProviderResource
  stackTags: ReadonlyMap<string, string>
  isType<TResource extends Resource>(cls: { new (...a: any[]): TResource }): boolean
  asType<TResource extends Resource, TArgs>(cls: { new (name: string, args: TArgs, ...a: any[]): TResource }): Unwrap<NonNullable<TArgs>> | undefined
  getConfig<T extends object>(): T                                        // typed config declared in configSchema
  notApplicable(reason?: string): never                                  // skip evaluation cleanly
}
```

## [03]-[CONFIG_SCHEMA]

A policy declares typed config through `configSchema`; `args.getConfig<T>()` returns it validated against the JSON-Schema draft below. `PolicyConfigJSONSchema` is the full per-property vocabulary — the config surface a `PolicyPackConfig` entry (or a stack's policy config) is validated against. This is the mechanism behind `getConfig`, not an opaque bag.

| [INDEX] | [SYMBOL]                          | [ROLE]                                                                                          |
| :-----: | :-------------------------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `PolicyConfigSchema`              | `properties: Record<string, PolicyConfigJSONSchema>`, `required?: string[]`                     |
|  [02]   | `PolicyConfigJSONSchema`          | per-property JSON-Schema draft (below)                                                          |
|  [03]   | `PolicyConfigJSONSchemaDefinition`| `PolicyConfigJSONSchema \| boolean`                                                             |
|  [04]   | `PolicyConfigJSONSchemaTypeName`  | `"string" \| "number" \| "integer" \| "boolean" \| "object" \| "array" \| "null"`               |
|  [05]   | `PolicyConfigJSONSchemaType`      | the value union (array / boolean / number / null / object / string)                             |

```ts contract
// @pulumi/policy — the config-property draft (validation keywords the pack enforces on getConfig<T>)
interface PolicyConfigJSONSchema {
  type?: PolicyConfigJSONSchemaTypeName | PolicyConfigJSONSchemaTypeName[]
  enum?: PolicyConfigJSONSchemaType[]; const?: PolicyConfigJSONSchemaType; default?: PolicyConfigJSONSchemaType
  multipleOf?: number; maximum?: number; exclusiveMaximum?: number; minimum?: number; exclusiveMinimum?: number
  maxLength?: number; minLength?: number; pattern?: string; format?: string
  items?: PolicyConfigJSONSchemaDefinition | PolicyConfigJSONSchemaDefinition[]; additionalItems?: PolicyConfigJSONSchemaDefinition
  maxItems?: number; minItems?: number; uniqueItems?: boolean; contains?: PolicyConfigJSONSchema
  maxProperties?: number; minProperties?: number; required?: string[]
  properties?: Record<string, PolicyConfigJSONSchemaDefinition>; patternProperties?: Record<string, PolicyConfigJSONSchemaDefinition>
  additionalProperties?: PolicyConfigJSONSchemaDefinition; propertyNames?: PolicyConfigJSONSchemaDefinition
  dependencies?: Record<string, PolicyConfigJSONSchemaDefinition | string[]>
  description?: string
}
```

## [04]-[RESOLVED_RESOURCE_BAG]

`ResourceValidationArgs.opts` and `PolicyResource.opts` are a fully-resolved (non-`Output`) options snapshot — the plain values a validation reads directly. `provider?` is the resolved provider reference. This is the resolved shape validation actually inspects; `PolicyResource.asType<T>()` returns the whole resource as a `q.ResolvedResource<T>` (from `@pulumi/pulumi/queryable`), the non-`Output` snapshot for stack-level folds.

```ts contract
// @pulumi/policy — resolved (non-Output) option + provider snapshots read during validation
interface PolicyResourceOptions {
  protect: boolean
  ignoreChanges: string[]
  deleteBeforeReplace?: boolean
  aliases: string[]
  customTimeouts: PolicyCustomTimeouts
  additionalSecretOutputs: string[]
  parent?: string                                                        // parent URN
}
interface PolicyCustomTimeouts { createSeconds: number; updateSeconds: number; deleteSeconds: number }
interface PolicyProviderResource { type: string; props: Record<string, any>; urn: string; name: string }
```

## [05]-[TYPED_HELPERS]

The `*OfType` helpers filter a rule to one resource class and hand the callback the provider's own strongly-typed args (`Unwrap<NonNullable<TArgs>>`), collapsing the manual `isType`/`asType` guard. The `Typed*` aliases are the callback signatures they wrap.

| [INDEX] | [SURFACE]                                                                       | [RETURNS]                                                         | [ROLE]                                                        |
| :-----: | :------------------------------------------------------------------------------ | :--------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `validateResourceOfType(resourceClass, validate: TypedResourceValidation<TProps>)` | `ResourceValidation`                                             | class-filtered typed validation                              |
|  [02]   | `remediateResourceOfType(resourceClass, remediate: TypedResourceRemediation<TProps>)` | `ResourceRemediation`                                            | class-filtered typed remediation                             |
|  [03]   | `validateRemediateResourceOfType(resourceClass, validateRemediate: TypedResourceValidationRemediation<TProps>)` | `{ validateResource, remediateResource }`                        | one callback that both flags and fixes; spread onto the policy |
|  [04]   | `validateStackResourcesOfType(resourceClass, validate)`                          | `StackValidation`                                                | stack fold over only resources of the class (`q.ResolvedResource<T>[]`) |
|  [05]   | `TypedResourceValidation<TProps>`                                                | `(props, args, reportViolation) => Promise<void> \| void`         | typed validate callback                                      |
|  [06]   | `TypedResourceRemediation<TProps>`                                               | `(props, args) => Record<string, any> \| void \| Promise<…> \| undefined` | typed remediate callback                                    |
|  [07]   | `TypedResourceValidationRemediation<TProps>`                                     | `(props, args, reportViolation) => Record<string, any> \| void \| Promise<…>` | combined validate+remediate callback                         |

## [06]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A `PolicyPack` runs as a separate process the CLI invokes during preview and update; it lives in its own `PulumiPolicy.yaml` project, never bundled with the infra program.
- `enforcementLevel`: `advisory` reports without blocking; `mandatory` blocks; `remediate` runs `remediateResource` to fix before blocking; `disabled` suppresses. Set the pack default on `PolicyPackArgs.enforcementLevel`, override per policy.
- `remediateResource` runs BEFORE validations, receives `ResourceValidationArgs`, and returns a partial/full replacement props object the engine applies; return `undefined` to leave the resource unchanged. Wrap a sensitive replacement value in `new Secret(value)` so the engine encrypts it.
- `validateResource` accepts one callback or an ordered array run in sequence; `reportViolation(message, urn?)` may fire multiple times. `getConfig<T>()` returns the `configSchema`-validated config; `notApplicable(reason?)` short-circuits a rule cleanly.
- `PolicyResource.asType<T>()` / `ResourceValidationArgs.asType<T,A>()` return the resolved non-`Output` snapshot — validate against plain values, never `Output`s.

[DEPLOY_STACK]: how `provisioning/contract#PROVISIONING` `PolicyGuard` composes this onto `@pulumi/pulumi` (`pulumi-pulumi.md`) and the Effect rails.
- `PolicyGuard.pack: policy.PolicyPack` is the single registered pack; `resourceRule(name, level, validate)` builds a `policy.ResourceValidationPolicy` and `enforce(mode: DeployMode)` selects the active `ReadonlyArray<policy.ResourceValidationPolicy>` per deploy mode through a `Match.exhaustive` table, so the cloud and self-hosted rule sets are one parameterized selection, not a parallel pack.
- The design's `PolicyLevel = "advisory" | "mandatory" | "disabled"` is a three-member subset of the four-member `EnforcementLevel`; the `remediate` arm and its `remediateResource`/`Secret` axis are available for a future auto-fix guard (encryption-at-rest applied rather than merely flagged) without a new surface — one more level value on the existing rule.
- Engine-event seam: a `mandatory` violation surfaces at engine time as the `@pulumi/pulumi/automation` `PolicyEvent` (`policyName`/`enforcementLevel`/`resourceUrn`) on the `EngineEvent` stream and folds into `SummaryEvent.policyPacks` — the `AutomationDriver`'s `onEvent` accumulator (`pulumi-pulumi.md`) reads it the same way `drift#PROVISIONING` reads `resourcePreEvent`, so a policy failure fails the deploy on the same rail as a drift divergence.

[SIBLING_STACK]:
- `@pulumi/pulumi` core owns the `Resource`/`Unwrap` types the `isType`/`asType`/`*OfType` guards resolve against and the `queryable.ResolvedResource<T>` snapshot the stack fold reads; it also owns the automation `PolicyEvent`/`SummaryEvent.policyPacks` the guard's outcome rides.
- Any typed provider (`@pulumi/aws`, `@pulumi/kubernetes`, `@pulumi/docker`) supplies the resource class a `validateResourceOfType` rule filters on, so a rule reads that provider's own args type.
- `effect` (`libs/typescript/.api/effect.md`) owns the `DeployMode` `Match.exhaustive` dispatch selecting the active rule set and the `Effect` program the `AutomationDriver` runs the pack under.

[RAIL_LAW]:
- Package: `@pulumi/policy`
- Owns: CrossGuard policy-pack authoring, enforcement, remediation, and typed config.
- Accept: `ResourceValidationPolicy` for per-resource gates, `StackValidationPolicy` for cross-resource folds, `validateResourceOfType`/`validateRemediateResourceOfType` for class-typed rules, `configSchema` + `getConfig<T>()` for parameterized rules, `new Secret(...)` for encrypted remediated values.
- Reject: inline ad-hoc validation inside a resource constructor where a `mandatory` `ResourceValidationPolicy` achieves the invariant portably; a parallel rule set per deploy mode where one `enforce(mode)` selection suffices; validating against `Output` values where the resolved `opts`/`asType` snapshot is the correct read.
