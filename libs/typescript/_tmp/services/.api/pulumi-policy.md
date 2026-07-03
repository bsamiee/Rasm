# [API_CATALOGUE] @pulumi/policy

`@pulumi/policy` supplies `PolicyPack`, `ResourceValidationPolicy`, `StackValidationPolicy`, and the `validateResourceOfType` / `remediateResourceOfType` / `validateRemediateResourceOfType` helpers for authoring CrossGuard policy packs that validate or auto-remediate Pulumi resource definitions before and during deployment.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/policy`
- package: `@pulumi/policy`
- module: `@pulumi/policy`
- asset: policy pack runtime, resource and stack validation surfaces, enforcement levels, remediation helpers
- rail: policy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: policy pack family
- rail: policy

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]  | [RAIL]                                                                                                   |
| :-----: | :----------------- | :------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `PolicyPack`       | class          | container for a named set of policies                                                                    |
|  [02]   | `PolicyPackArgs`   | args interface | `policies`, `enforcementLevel`, `description`, `displayName`, `readme`, `provider`, `tags`, `repository` |
|  [03]   | `EnforcementLevel` | string union   | `"advisory" \| "mandatory" \| "remediate" \| "disabled"`                                                 |
|  [04]   | `Severity`         | string union   | `"low" \| "medium" \| "high" \| "critical"`                                                              |
|  [05]   | `Policies`         | type alias     | `(ResourceValidationPolicy \| StackValidationPolicy)[]`                                                  |
|  [06]   | `PolicyPackConfig` | type alias     | `Record<string, EnforcementLevel \| { enforcementLevel?: EnforcementLevel; [key: string]: any }>`        |
|  [07]   | `Secret`           | class          | wraps a plaintext value as a Pulumi secret in remediations                                               |

[PUBLIC_TYPE_SCOPE]: policy definition family
- rail: policy

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]    | [RAIL]                                                                                                                               |
| :-----: | :-------------------------- | :--------------- | :----------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Policy`                    | base interface   | `name`, `description`, `enforcementLevel`, `severity`, `configSchema`, `displayName`, `framework`, `tags`, `remediationSteps`, `url` |
|  [02]   | `ResourceValidationPolicy`  | policy interface | extends `Policy`; `validateResource`, `remediateResource`                                                                            |
|  [03]   | `StackValidationPolicy`     | policy interface | extends `Policy`; `validateStack`                                                                                                    |
|  [04]   | `PolicyConfigSchema`        | schema interface | `properties: Record<string, PolicyConfigJSONSchema>`, `required?`                                                                    |
|  [05]   | `PolicyComplianceFramework` | interface        | `name`, `version`, `reference`, `specification`                                                                                      |

[PUBLIC_TYPE_SCOPE]: validation and remediation callback family
- rail: policy

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [RAIL]                                                                                                                                |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `ResourceValidation`     | callback type  | `(args: ResourceValidationArgs, reportViolation: ReportViolation) => Promise<void> \| void`                                           |
|  [02]   | `ResourceRemediation`    | callback type  | `(args: ResourceValidationArgs) => Record<string, any> \| void \| Promise<…>`                                                         |
|  [03]   | `StackValidation`        | callback type  | `(args: StackValidationArgs, reportViolation: ReportViolation) => Promise<void> \| void`                                              |
|  [04]   | `ReportViolation`        | callback type  | `(message: string, urn?: string) => void`                                                                                             |
|  [05]   | `ResourceValidationArgs` | args interface | `type`, `props`, `urn`, `name`, `opts`, `provider?`, `stackTags`, `isType<T>()`, `asType<T,A>()`, `getConfig<T>()`, `notApplicable()` |
|  [06]   | `StackValidationArgs`    | args interface | `resources: PolicyResource[]`, `stackTags`, `getConfig<T>()`, `notApplicable()`                                                       |
|  [07]   | `PolicyResource`         | interface      | `type`, `props`, `urn`, `name`, `opts`, `provider?`, `parent?`, `dependencies`, `propertyDependencies`, `isType<T>()`, `asType<T>()`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: policy pack construction
- rail: policy

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `new PolicyPack(name, args, initialConfig?)` | pack init      | registers all policies under `name` |

[ENTRYPOINT_SCOPE]: typed policy helpers
- rail: policy

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]     | [RAIL]                                                                     |
| :-----: | :------------------------------------------------------------------ | :----------------- | :------------------------------------------------------------------------- |
|  [01]   | `validateResourceOfType(resourceClass, validate)`                   | validation helper  | strongly-typed `ResourceValidation` factory                                |
|  [02]   | `remediateResourceOfType(resourceClass, remediate)`                 | remediation helper | strongly-typed `ResourceRemediation` factory                               |
|  [03]   | `validateRemediateResourceOfType(resourceClass, validateRemediate)` | combined helper    | returns `{ validateResource, remediateResource }` for spread into a policy |
|  [04]   | `validateStackResourcesOfType(resourceClass, validate)`             | stack helper       | strongly-typed `StackValidation` that filters by resource class            |

## [04]-[IMPLEMENTATION_LAW]

[POLICY_TOPOLOGY]:
- a `PolicyPack` registers one or more `ResourceValidationPolicy` or `StackValidationPolicy` entries; it runs as a separate process invoked by the Pulumi CLI during previews and updates
- `enforcementLevel`: `"advisory"` reports violations without blocking; `"mandatory"` blocks deployment; `"remediate"` triggers auto-remediation before blocking; `"disabled"` suppresses the policy
- `remediateResource` receives the resource args before provisioning and returns a partial or full replacement args object; the Pulumi engine applies the returned values — return `undefined` to leave the resource unchanged
- `ResourceValidationArgs.getConfig<T>()` returns typed config declared in `PolicyPackConfig` on the policy's `configSchema`; call `notApplicable(reason?)` to skip evaluation cleanly
- `Secret` wraps a plaintext value so the Pulumi engine encrypts it as a secret when returned from a remediation function
- `PolicyResource.asType<TResource>()` returns a fully resolved (non-`Output`) snapshot of the resource properties for stack-level validation

[LOCAL_ADMISSION]:
- Each `PolicyPack` lives in its own TypeScript project under a `PulumiPolicy.yaml` manifest; it is not bundled with the infrastructure program.
- Use `validateRemediateResourceOfType` when the same callback must both flag and fix violations, then spread the result onto the policy object.

[RAIL_LAW]:
- Package: `@pulumi/policy`
- Owns: CrossGuard policy pack authoring and enforcement
- Accept: `ResourceValidationPolicy` for per-resource gates; `StackValidationPolicy` for cross-resource checks
- Reject: inline ad-hoc validation inside resource constructors when a `ResourceValidationPolicy` with `enforcementLevel: "mandatory"` achieves the same invariant portably across stacks
