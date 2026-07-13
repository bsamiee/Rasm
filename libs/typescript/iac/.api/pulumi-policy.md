# [TS_IAC_API_PULUMI_POLICY]

`@pulumi/policy` is the CrossGuard policy-as-code framework: a `PolicyPack` owns an array of `ResourceValidationPolicy` (per-resource, with optional pre-validation remediation) and `StackValidationPolicy` (whole-stack, dependency-aware) checks, each carrying an `EnforcementLevel`/`Severity`/compliance-framework metadata and an optional JSON-schema-typed config. Unlike the provider SDKs this is a bespoke authoring API, not codegen — its power is the strongly-typed helper family (`validateResourceOfType`/`remediateResourceOfType`/`validateRemediateResourceOfType`/`validateStackResourcesOfType`) that narrows any Pulumi `Resource` subclass to its typed input props, so ONE parameterized helper owns validation across every resource class the other `iac` catalogs export. In the deploy plane it is `policy/guard`: the CrossGuard packs that gate every `program/automation` run, narrowing against the exact `@pulumi/kubernetes`, `@pulumi/gcp`, and `@pulumi/postgresql` classes; the `policy/drift` `previewRefresh` fold over `OpType` sits beside it on the engine's Automation API.

```ts
// @pulumi/policy
export * from "./policy"                                          // PolicyPack + policy/validation/remediation surface
export { unknownCheckingProxy, UnknownValueError } from "./proxy" // preview-time unknown-value guard
```

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/policy`
- package: `@pulumi/policy`
- license: `Apache-2.0`
- build-floor: peer `@pulumi/pulumi ^catalog` (higher floor than the provider SDKs — needs the current analyzer plugin protocol); bundles `@grpc/grpc-js` + `google-protobuf` (the policy plugin speaks the analyzer gRPC service)
- target: `node` (runs as the `pulumi-analyzer-policy` plugin process the engine invokes during `preview`/`up`; not a resource in the graph)
- entry: `@pulumi/policy` (re-exports `./policy` + the `./proxy` unknown guard)
- asset: `PolicyPack`, the `ResourceValidationPolicy`/`StackValidationPolicy` shapes, the typed `*ResourceOfType` helper family, the `ResourceValidationArgs`/`StackValidationArgs`/`PolicyResource` inspection surface, the `EnforcementLevel`/`Severity`/compliance-framework vocabularies, `ReportViolation`, the remediation `Secret`, and the `unknownCheckingProxy`
- rail: iac / policy

## [02]-[PUBLIC_TYPES]

### [02.1]-[POLICYPACK_THE_PACK_ENFORCEMENT_VOCABULARY]

[PUBLIC_TYPE_SCOPE]: pack + vocabularies
- rail: iac / policy
- entry: `@pulumi/policy`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `PolicyPack`       | class         | `new PolicyPack(name, args, initialConfig?)` — registers the pack                              |
|  [02]   | `PolicyPackArgs`   | interface     | `{ policies, enforcementLevel, description, displayName, readme, provider, tags, repository }` |
|  [03]   | `Policies`         | union array   | `(ResourceValidationPolicy \| StackValidationPolicy)[]`                                        |
|  [04]   | `EnforcementLevel` | string union  | `"advisory" \| "mandatory" \| "remediate" \| "disabled"`                                       |
|  [05]   | `Severity`         | string union  | `"low" \| "medium" \| "high" \| "critical"`                                                    |
|  [06]   | `PolicyPackConfig` | record        | `{ [policy: string]: PolicyConfig }` — per-policy config bag                                   |
|  [07]   | `PolicyConfig`     | union         | `EnforcementLevel \| ({ enforcementLevel?; [key]: any })`                                      |

```ts signature
import { Resource, Unwrap } from "@pulumi/pulumi"

declare class PolicyPack {
  constructor(name: string, args: PolicyPackArgs, initialConfig?: PolicyPackConfig)
}
interface PolicyPackArgs {
  policies: Policies
  enforcementLevel?: EnforcementLevel        // pack default; each policy may override
  description?: string; displayName?: string; readme?: string
  provider?: string; tags?: string[]; repository?: string
}
type EnforcementLevel = "advisory" | "mandatory" | "remediate" | "disabled"
type Severity = "low" | "medium" | "high" | "critical"
type Policies = (ResourceValidationPolicy | StackValidationPolicy)[]
type PolicyPackConfig = { [policy: string]: PolicyConfig }
```

### [02.2]-[POLICY_BASE_SHARED_METADATA_CONFIG_SCHEMA]

[PUBLIC_TYPE_SCOPE]: policy base
- rail: iac / policy
- entry: `@pulumi/policy`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Policy`                    | interface     | base: `name`/`description` required; optional `severity`/`framework`/`configSchema` |
|  [02]   | `PolicyComplianceFramework` | interface     | `{ name; version; reference; specification }` — SOC2/PCI/etc. tag                   |
|  [03]   | `PolicyConfigSchema`        | interface     | `{ properties: { [k]: PolicyConfigJSONSchema }; required? }`                        |
|  [04]   | `PolicyConfigJSONSchema`    | type (schema) | JSON-schema node backing `configSchema` (from `./schema`)                           |

```ts signature
import { PolicyConfigJSONSchema } from "./schema"

interface Policy {
  name: string                                   // unique within the pack
  description: string
  enforcementLevel?: EnforcementLevel
  configSchema?: PolicyConfigSchema              // typed config read via args.getConfig<T>()
  displayName?: string
  severity?: Severity
  framework?: PolicyComplianceFramework
  tags?: string[]
  remediationSteps?: string
  url?: string
}
interface PolicyConfigSchema { properties: { [key: string]: PolicyConfigJSONSchema }; required?: string[] }
interface PolicyComplianceFramework { name: string; version: string; reference: string; specification: string }
```

### [02.3]-[RESOURCEVALIDATIONPOLICY_PER_RESOURCE_CHECK_REMEDIATION]

[PUBLIC_TYPE_SCOPE]: resource validation
- rail: iac / policy
- entry: `@pulumi/policy`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `ResourceValidationPolicy` | interface     | `Policy` + `validateResource?` (one or list) + `remediateResource?`                     |
|  [02]   | `ResourceValidation`       | callback      | `(args: ResourceValidationArgs, report: ReportViolation) => Promise<void> \| void`      |
|  [03]   | `ResourceRemediation`      | callback      | `(args) => Record<string, any> \| void \| Promise<…>` — runs BEFORE validation, may fix |
|  [04]   | `ResourceValidationArgs`   | interface     | inspection bag: `opts`, `stackTags`, `isType`/`asType`, `getConfig`, `notApplicable`    |
|  [05]   | `PolicyResourceOptions`    | interface     | resource-option snapshot the analyzer reads (roster in [05] below)                      |
|  [06]   | `PolicyCustomTimeouts`     | interface     | `{ createSeconds; updateSeconds; deleteSeconds }`                                       |
|  [07]   | `PolicyProviderResource`   | interface     | `{ type; props; urn; name }` — the resource's provider                                  |

- [05]-[POLICY_RESOURCE_OPTIONS]: `protect`, `ignoreChanges`, `deleteBeforeReplace?`, `aliases`, `customTimeouts`, `additionalSecretOutputs`, `parent?`.

```ts signature
import { Resource, Unwrap } from "@pulumi/pulumi"

interface ResourceValidationPolicy extends Policy {
  validateResource?: ResourceValidation | ResourceValidation[]   // multiple run in order
  remediateResource?: ResourceRemediation                        // pre-validation fix pass
}
type ResourceValidation = (args: ResourceValidationArgs, reportViolation: ReportViolation) => Promise<void> | void
type ResourceRemediation = (args: ResourceValidationArgs) => Promise<Record<string, any>> | Record<string, any> | Promise<void> | void | undefined

interface ResourceValidationArgs {
  type: string; props: Record<string, any>; urn: string; name: string
  opts: PolicyResourceOptions; provider?: PolicyProviderResource
  stackTags: ReadonlyMap<string, string>
  isType<T extends Resource>(resourceClass: { new (...rest: any[]): T }): boolean
  asType<T extends Resource, TArgs>(resourceClass: { new (name: string, args: TArgs, ...rest: any[]): T }): Unwrap<NonNullable<TArgs>> | undefined
  getConfig<T extends object>(): T                               // decodes configSchema
  notApplicable(reason?: string): never                          // skip this policy for this resource
}
```

### [02.4]-[THE_TYPED_RESOURCEOFTYPE_HELPER_FAMILY_THE_PARAMETERIZED_NARROWING_PATTERN]

[PUBLIC_TYPE_SCOPE]: typed helpers
- rail: iac / policy
- entry: `@pulumi/policy`

One helper family narrows any Pulumi `Resource` subclass to its `Unwrap`ped typed props, so a policy authored against `kubernetes.apps.v1.Deployment` or `gcp.storage.Bucket` gets full field typing with no manual `isType`/`asType` plumbing. This IS the mechanism — the specific resource classes are data supplied at the call site.

| [INDEX] | [SYMBOL]                           | [SIGNATURE]                                                                             |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `validateResourceOfType`           | `(resourceClass, (props, args, report) => …) => ResourceValidation`                     |
|  [02]   | `remediateResourceOfType`          | `(resourceClass, (props, args) => Record<string,any> \| void) => ResourceRemediation`   |
|  [03]   | `validateRemediateResourceOfType`  | `(resourceClass, cb) => { validateResource; remediateResource }` — spread into a policy |
|  [04]   | `validateStackResourcesOfType`     | `(resourceClass, (resources[], args, report) => …) => StackValidation`                  |
|  [05]   | `TypedResourceValidation<TProps>`  | typed twin of `ResourceValidation`                                                      |
|  [06]   | `TypedResourceRemediation<TProps>` | typed twin of `ResourceRemediation`                                                     |

```ts signature
import { Resource, Unwrap } from "@pulumi/pulumi"

function validateResourceOfType<TResource extends Resource, TArgs>(
  resourceClass: { new (name: string, args: TArgs, ...rest: any[]): TResource },
  validate: (props: Unwrap<NonNullable<TArgs>>, args: ResourceValidationArgs, reportViolation: ReportViolation) => Promise<void> | void
): ResourceValidation

function remediateResourceOfType<TResource extends Resource, TArgs>(
  resourceClass: { new (name: string, args: TArgs, ...rest: any[]): TResource },
  remediate: (props: Unwrap<NonNullable<TArgs>>, args: ResourceValidationArgs) => Record<string, any> | void | Promise<Record<string, any> | void> | undefined
): ResourceRemediation

function validateRemediateResourceOfType<TResource extends Resource, TArgs>(
  resourceClass: { new (name: string, args: TArgs, ...rest: any[]): TResource },
  validateRemediate: (props: Unwrap<NonNullable<TArgs>>, args: ResourceValidationArgs, reportViolation: ReportViolation) => Record<string, any> | void | Promise<Record<string, any> | void> | undefined
): { validateResource: ResourceValidation; remediateResource: ResourceRemediation }

function validateStackResourcesOfType<TResource extends Resource>(
  resourceClass: { new (...rest: any[]): TResource },
  validate: (resources: q.ResolvedResource<TResource>[], args: StackValidationArgs, reportViolation: ReportViolation) => Promise<void> | void
): StackValidation
```

### [02.5]-[STACKVALIDATIONPOLICY_WHOLE_STACK_DEPENDENCY_AWARE]

[PUBLIC_TYPE_SCOPE]: stack validation
- rail: iac / policy
- entry: `@pulumi/policy`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :---------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `StackValidationPolicy` | interface     | `Policy` + `validateStack: StackValidation`                                             |
|  [02]   | `StackValidation`       | callback      | `(args: StackValidationArgs, report: ReportViolation) => Promise<void> \| void`         |
|  [03]   | `StackValidationArgs`   | interface     | `{ resources: PolicyResource[]; stackTags; getConfig; notApplicable }`                  |
|  [04]   | `PolicyResource`        | interface     | resource-graph node with `dependencies`/`propertyDependencies` edges, `isType`/`asType` |

```ts signature
import { Resource } from "@pulumi/pulumi"
import * as q from "@pulumi/pulumi/queryable"

interface StackValidationPolicy extends Policy { validateStack: StackValidation }
type StackValidation = (args: StackValidationArgs, reportViolation: ReportViolation) => Promise<void> | void
interface StackValidationArgs {
  resources: PolicyResource[]
  stackTags: ReadonlyMap<string, string>
  getConfig<T extends object>(): T
  notApplicable(reason?: string): never
}
interface PolicyResource {
  type: string; props: Record<string, any>; urn: string; name: string
  opts: PolicyResourceOptions; provider?: PolicyProviderResource; parent?: PolicyResource
  dependencies: PolicyResource[]                              // full dependency graph
  propertyDependencies: Record<string, PolicyResource[]>     // per-property dep edges
  isType<T extends Resource>(resourceClass: { new (...rest: any[]): T }): boolean
  asType<T extends Resource>(resourceClass: { new (...rest: any[]): T }): q.ResolvedResource<T> | undefined
}
```

### [02.6]-[REPORTING_REMEDIATION_SECRETS_PREVIEW_GUARD]

[PUBLIC_TYPE_SCOPE]: reporting / secret / proxy
- rail: iac / policy
- entry: `@pulumi/policy`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `ReportViolation`      | callback      | `(message: string, urn?: string) => void` — call N times for N violations           |
|  [02]   | `Secret`               | class         | `new Secret(value)` — mark a remediated value for engine encryption                 |
|  [03]   | `unknownCheckingProxy` | re-export     | preview-unknown props guard re-exported from `./proxy`; empty `.d.ts`, runtime-only |
|  [04]   | `UnknownValueError`    | re-export     | paired guard export; same empty-`.d.ts` caveat, no declared shape                   |

```ts signature
type ReportViolation = (message: string, urn?: string) => void
declare class Secret { value: any; constructor(value: any) }
// unknownCheckingProxy / UnknownValueError: re-exported by index.d.ts from "./proxy",
// but proxy.d.ts ships as `export {}` — no typed declaration. Runtime-only; do not
// compose against a typed signature. Reach for them only in raw preview-unknown guarding.
```

## [03]-[IMPLEMENTATION_LAW]

[PACK_TOPOLOGY]:
- A `PolicyPack` is instantiated once per plugin process; its `policies` array mixes `ResourceValidationPolicy` and `StackValidationPolicy`. Pack-level `enforcementLevel` is the default; each policy overrides it, and `PolicyPackConfig`/`initialConfig` sets per-policy config and level at registration. `"remediate"` runs `remediateResource` and applies the returned props; `"mandatory"` blocks the run; `"advisory"` warns; `"disabled"` skips.
- Author `Policies` as pure data — a policy is `{ name, description, enforcementLevel, validateResource }`; grow the pack by adding a row, never by branching a validator.

[VALIDATION_TOPOLOGY]:
- Prefer the typed `validateResourceOfType(Class, (props, args, report) => …)` over hand-written `isType`/`asType` — it narrows `props` to the resource's `Unwrap`ped input type at compile time. `report(message, urn?)` accumulates violations; `args.notApplicable(reason?)` short-circuits a policy for a resource; `args.getConfig<T>()` decodes the `configSchema`.
- Remediation runs BEFORE validation: `remediateResource` returns a corrected prop bag (or `void` to skip), giving `"remediate"` policies a fix-forward path. `validateRemediateResourceOfType` yields both halves from one callback for spread into a policy literal.
- `StackValidationPolicy` sees the whole `PolicyResource[]` with `dependencies`/`propertyDependencies` — use it for cross-resource invariants (every `Deployment` has a matching `NetworkPolicy`, every `Bucket` a retention `Grant`). `validateStackResourcesOfType` narrows the stack to one class array.

[STACK_LAW]:
- GUARD SEAM (`policy/guard`): the CrossGuard pack narrows against the exact classes the sibling catalogs export — `validateResourceOfType(kubernetes.apps.v1.Deployment, …)`, `validateResourceOfType(gcp.storage.Bucket, …)`, `validateResourceOfType(postgresql.Role, (role, _, report) => role.superuser && report("app roles must not be superuser"))`. The pack is passed to `program/automation` so every `LocalWorkspace` `preview`/`up` runs it; violations fold into the typed run receipt over the up | preview | refresh | destroy ledger.
- DRIFT SEAM (`policy/drift`): the `previewRefresh` drift fold over `OpType` reads the engine's `Stack.previewRefresh(): Promise<PreviewResult>` and the `OpType` op union (`"same" | "create" | "update" | "delete" | "replace" | …`) / `OpMap` change summary — those symbols live in `@pulumi/pulumi/automation` (the `pulumi-pulumi` catalog), NOT here. This catalog owns the guard packs; the drift fold composes the engine's Automation API beside them.
- SECRET RAIL: a `"remediate"` policy returning credential material wraps it in `new policy.Secret(value)` so the engine encrypts it in state — meeting the `security/secret` path; source remediated secrets from `@pulumiverse/doppler`, never literals.
- EFFECT WEAVE: policies are pure functions authored functionally; the pack is a plain value the `program/automation` composition passes into the run. `unknownCheckingProxy` guards preview-time reads so a validator never asserts on an unresolved `Output`.

[RAIL_LAW]:
- iac / policy rail; `node`-tier. Runs as the `pulumi-analyzer-policy` plugin the engine invokes at analysis time (`preview` and `up`), not as a resource in the graph — it observes the desired-state props, it does not provision. The analyzer protocol floor is manifest-owned. Violations are structured `ReportViolation` calls the engine surfaces; there is no in-band `Result` — the failure channel is the analyzer's violation report folded into the run receipt.
