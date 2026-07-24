# [TS_IAC_API_PULUMI_POLICY]

`@pulumi/policy` owns Pulumi CrossGuard policy-as-code: a `PolicyPack` folds `ResourceValidationPolicy` (per-resource, with pre-validation remediation) and `StackValidationPolicy` (whole-stack, dependency-aware) checks, each tagged with an `EnforcementLevel`, `Severity`, and compliance framework over a JSON-schema-typed config. Its typed `*ResourceOfType` helper family narrows any `Resource` subclass to its `Unwrap`ped input props, so one parameterized helper validates every class the sibling `iac` catalogs export.

[EXPORTS]: `unknownCheckingProxy` `UnknownValueError`

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/policy`
- package: `@pulumi/policy` (Apache-2.0)
- module: `@pulumi/policy` (re-exports `./policy` + the `./proxy` unknown guard)
- asset: the pack, the resource/stack validation-policy shapes, the typed `*ResourceOfType` helper family, the inspection bag, the `EnforcementLevel`/`Severity`/compliance vocabularies, `ReportViolation`, remediation `Secret`, and the proxy guards
- runtime: `node` — runs as the `pulumi-analyzer-policy` plugin the engine invokes during `preview`/`up`, not a resource in the graph; speaks the analyzer gRPC service through bundled `@grpc/grpc-js` + `google-protobuf`
- rail: iac / policy

## [02]-[PUBLIC_TYPES]

### [02.1]-[POLICYPACK_THE_PACK_ENFORCEMENT_VOCABULARY]

[PUBLIC_TYPE_SCOPE]: pack + vocabularies

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                                                   |
| :-----: | :----------------- | :------------ | :--------------------------------------------------------------------------------------------- |
|  [01]   | `PolicyPack`       | class         | `new PolicyPack(name, args, initialConfig?)` — registers the pack                              |
|  [02]   | `PolicyPackArgs`   | interface     | `{ policies, enforcementLevel, description, displayName, readme, provider, tags, repository }` |
|  [03]   | `Policies`         | union array   | `(ResourceValidationPolicy \| StackValidationPolicy)[]`                                        |
|  [04]   | `EnforcementLevel` | string union  | `"advisory" \| "mandatory" \| "remediate" \| "disabled"`                                       |
|  [05]   | `Severity`         | string union  | `"low" \| "medium" \| "high" \| "critical"`                                                    |
|  [06]   | `PolicyPackConfig` | record        | `{ [policy: string]: PolicyConfig }` — per-policy config bag                                   |
|  [07]   | `PolicyConfig`     | union         | `EnforcementLevel \| ({ enforcementLevel?; [key]: any })`                                      |

[POLICY_PACK_ARGS]: `policies: Policies` `enforcementLevel: EnforcementLevel` `description: string` `displayName: string` `readme: string` `provider: string` `tags: string[]` `repository: string`

### [02.2]-[POLICY_BASE_SHARED_METADATA_CONFIG_SCHEMA]

[PUBLIC_TYPE_SCOPE]: policy base

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Policy`                    | interface     | base: `name`/`description` required; optional `severity`/`framework`/`configSchema` |
|  [02]   | `PolicyComplianceFramework` | interface     | `{ name; version; reference; specification }` — SOC2/PCI/etc. tag                   |
|  [03]   | `PolicyConfigSchema`        | interface     | `{ properties: { [k]: PolicyConfigJSONSchema }; required? }`                        |
|  [04]   | `PolicyConfigJSONSchema`    | interface     | JSON-schema node backing `configSchema` (from `./schema`)                           |

[POLICY]: `name: string` `description: string` `enforcementLevel: EnforcementLevel` `configSchema: PolicyConfigSchema` `displayName: string` `severity: Severity` `framework: PolicyComplianceFramework` `tags: string[]` `remediationSteps: string` `url: string`

### [02.3]-[RESOURCEVALIDATIONPOLICY_PER_RESOURCE_CHECK_REMEDIATION]

[PUBLIC_TYPE_SCOPE]: resource validation

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

[RESOURCE_VALIDATION_ARGS]: `type: string` `props: Record<string,any>` `urn: string` `name: string` `opts: PolicyResourceOptions` `provider: PolicyProviderResource` `stackTags: ReadonlyMap<string,string>` `isType(Class) -> boolean` `asType(Class) -> Unwrap<NonNullable<TArgs>>|undefined` `getConfig() -> T` `notApplicable(reason?) -> never`

### [02.4]-[THE_TYPED_RESOURCEOFTYPE_HELPER_FAMILY_THE_PARAMETERIZED_NARROWING_PATTERN]

[PUBLIC_TYPE_SCOPE]: typed helpers

One helper family narrows any Pulumi `Resource` subclass to its `Unwrap`ped typed props, so a policy authored against `kubernetes.apps.v1.Deployment` or `gcp.storage.Bucket` gets full field typing with zero `isType`/`asType` plumbing. Each helper takes the resource class first, then the typed callback below; the capability cell carries the callback and return shape.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `validateResourceOfType`                     | function      | `((props, args, report) => …) -> ResourceValidation`               |
|  [02]   | `remediateResourceOfType`                    | function      | `((props, args) => Record \| void) -> ResourceRemediation`         |
|  [03]   | `validateRemediateResourceOfType`            | function      | `cb -> { validateResource; remediateResource }` — spread in policy |
|  [04]   | `validateStackResourcesOfType`               | function      | `((resources[], args, report) => …) -> StackValidation`            |
|  [05]   | `TypedResourceValidation<TProps>`            | type          | typed twin of `ResourceValidation`                                 |
|  [06]   | `TypedResourceRemediation<TProps>`           | type          | typed twin of `ResourceRemediation`                                |
|  [07]   | `TypedResourceValidationRemediation<TProps>` | type          | typed twin feeding `validateRemediateResourceOfType`               |

### [02.5]-[STACKVALIDATIONPOLICY_WHOLE_STACK_DEPENDENCY_AWARE]

[PUBLIC_TYPE_SCOPE]: stack validation

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :---------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `StackValidationPolicy` | interface     | `Policy` + `validateStack: StackValidation`                                             |
|  [02]   | `StackValidation`       | callback      | `(args: StackValidationArgs, report: ReportViolation) => Promise<void> \| void`         |
|  [03]   | `StackValidationArgs`   | interface     | `{ resources: PolicyResource[]; stackTags; getConfig; notApplicable }`                  |
|  [04]   | `PolicyResource`        | interface     | resource-graph node with `dependencies`/`propertyDependencies` edges, `isType`/`asType` |

[POLICY_RESOURCE]: `type: string` `props: Record<string,any>` `urn: string` `name: string` `opts: PolicyResourceOptions` `provider: PolicyProviderResource` `parent: PolicyResource` `dependencies: PolicyResource[]` `propertyDependencies: Record<string,PolicyResource[]>` `isType(Class) -> boolean` `asType(Class) -> q.ResolvedResource<T>|undefined`

### [02.6]-[REPORTING_REMEDIATION_SECRETS_PREVIEW_GUARD]

[PUBLIC_TYPE_SCOPE]: reporting / secret / proxy

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                            |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------------------------------------- |
|  [01]   | `ReportViolation`      | callback      | `(message: string, urn?: string) => void` — call N times for N violations               |
|  [02]   | `Secret`               | class         | `new Secret(value)`; `.value` reads plaintext — marks a remediated value for encryption |
|  [03]   | `unknownCheckingProxy` | re-export     | preview-unknown props guard re-exported from `./proxy`; empty `.d.ts`, runtime-only     |
|  [04]   | `UnknownValueError`    | re-export     | paired guard export; same empty-`.d.ts` caveat, no declared shape                       |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PolicyPack` instantiates once per plugin process; `policies` mixes `ResourceValidationPolicy` and `StackValidationPolicy`. Pack `enforcementLevel` is the default each policy overrides; `PolicyPackConfig`/`initialConfig` sets per-policy config and level at registration. `"remediate"` runs `remediateResource` and applies returned props, `"mandatory"` blocks the run, `"advisory"` warns, `"disabled"` skips.
- Author `Policies` as pure data — a policy is one `{ name, description, enforcementLevel, validateResource }` row; grow the pack by adding a row, never by branching a validator.
- `validateResourceOfType(Class, (props, args, report) => …)` narrows `props` to the resource's `Unwrap`ped input type at compile time; `report(message, urn?)` accumulates violations, `args.notApplicable(reason?)` short-circuits a resource, `args.getConfig<T>()` decodes the `configSchema`.
- Remediation runs before validation: `remediateResource` returns a corrected prop bag or `void`; `validateRemediateResourceOfType` yields both halves from one callback for spread into a policy literal.
- `StackValidationPolicy` sees the whole `PolicyResource[]` with `dependencies`/`propertyDependencies` for cross-resource invariants; `validateStackResourcesOfType` narrows the stack to one class array.
- `PolicyPack` runs as the analyzer plugin — observing desired-state props at analysis time, never provisioning — and surfaces violations as structured `ReportViolation` calls folded into the run receipt, the sole failure channel with no in-band `Result`.

[STACKING]:
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): the pack rides `UpOptions.policyPacks` into every `LocalWorkspace` `preview`/`up`, and violations fold into the `RunReceipt` over the up|preview|refresh|destroy ledger; the `policy/drift` fold reads `Stack.previewRefresh() -> PreviewResult` and buckets `OpType`/`OpMap`, symbols owned by the automation catalog, never here.
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`), `@pulumi/gcp`(`.api/pulumi-gcp.md`), `@pulumi/postgresql`(`.api/pulumi-postgresql.md`): `validateResourceOfType(Class, cb)` narrows against the exact exported class — `kubernetes.apps.v1.Deployment`, `gcp.storage.Bucket`, `postgresql.Role` where `role.superuser` gates app roles.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): a `"remediate"` policy returning credential material wraps it in `new Secret(value)` for engine encryption, sourcing the value from Doppler.
- within-lib: policies stay pure functions and the pack a plain value the `program`/`automation` composition passes into the run; `unknownCheckingProxy` guards preview-time reads so a validator never asserts on an unresolved `Output`.

[RAIL_LAW]:
- Package: `@pulumi/policy`
- Owns: the CrossGuard analyzer plugin — per-resource and whole-stack validation with pre-validation remediation, gating `preview`/`up`.
- Accept: typed `validateResourceOfType`/`validateStackResourcesOfType` narrowing; `ReportViolation` as the violation channel; `new Secret(value)` for remediated credentials; the pack as a plain value the run composes.
- Reject: hand-written `isType`/`asType` plumbing where the typed helper narrows; a validator asserting on an unresolved `Output`; an authored `PulumiPolicy.yaml`.
