# [TS_IAC_API_PULUMI_PULUMISERVICE]

`@pulumi/pulumiservice` mints Pulumi Cloud control-plane configuration as typed resources, so trigger, schedule, review-stack, webhook, RBAC, and token policy is IaC data, never REST against `api.pulumi.com`.

These resources activate only against a Cloud-backed stack as the hosted twins of the self-managed backend's local drift and expiry owners; the Automation API stays the execution backend, never the policy surface.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumiservice`
- package: `@pulumi/pulumiservice` (Apache-2.0)
- module: `@pulumi/pulumiservice` → resource roster, `Provider`, `get*`/`get*Output` reads, `build*Permissions` scope helpers
- runtime: Node deploy-host; every resource a Pulumi Cloud API object under `ProviderArgs { accessToken?, apiUrl? }`
- rail: cloud-control-plane
- depends: `@pulumi/pulumi`; pairs with `@pulumi/esc-sdk` over the same `Environment` objects
- abi: `DeploymentSettingsArgs.vcs` is the live trigger block, `github` deprecated; `Environment.yaml` is `Input<Asset | Archive>`

## [02]-[SCHEDULES_AND_SETTINGS]

[SCHEDULE_SCOPE]: one concern per class, all keyed `organization`/`project`/`stack`

| [INDEX] | [SYMBOL]                      | [CONFIGURES]                                                                             |
| :-----: | :---------------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | `DriftSchedule`               | `{ scheduleCron, autoRemediate? }` hosted drift detection                                |
|  [02]   | `TtlSchedule`                 | `{ timestamp, deleteAfterDestroy? }` stack expiry                                        |
|  [03]   | `DeploymentSchedule`          | `{ pulumiOperation: PulumiOperation, scheduleCron? XOR timestamp? }` mutating-op cadence |
|  [04]   | `EnvironmentRotationSchedule` | ESC rotated-secrets cadence                                                              |

[SETTINGS_SCOPE]: `DeploymentSettings` — the per-stack execution contract
- `DeploymentSettings` binds `sourceContext.git`, `operationContext { environmentVariables, preRunCommands, oidc }`, `executorContext`, `agentPoolId`, `cacheOptions`, and the `vcs` trigger block; review stacks mint from `vcs.previewPullRequests` + `pullRequestTemplate` bounded by `TtlSchedule`, forked from a `TemplateSource`.

## [03]-[EVENTS_AND_ACCESS]

[EVENT_SCOPE]: `Webhook` — one delivery row per sink
- `Webhook` scopes org, stack, or environment by which of `projectName`/`stackName`/`environmentName` is present; `WebhookFormat` selects the payload shape, `WebhookFilters` the delivered event class across stack/operation/deployment/drift/environment lifecycles, and `WebhookGroup` batches a whole class.

[ACCESS_SCOPE]: identity, RBAC, environment, token, and execution rows
- [ENVIRONMENTS]: `Environment` `EnvironmentVersionTag`
- [TEAM_RBAC]: `Team` `TeamStackPermission` `TeamEnvironmentPermission` `TeamRoleAssignment`
- [ORG_RBAC]: `OrganizationMember` `OrganizationRole` `ApprovalRule`
- [TOKENS]: `AccessToken` `OrgAccessToken` `TeamAccessToken`, scoped by `buildStackScopedPermissions`/`buildEnvironmentScopedPermissions`/`buildAllowPermissions`
- [EXECUTION]: `AgentPool` `OidcIssuer` `TemplateSource`
- [GOVERNANCE]: `PolicyGroup` `PolicyPack` `Stack` `StackTag` `StackTags` `InsightsAccount`
- [READS]: `getCurrentUser` `getEnvironment` `getPolicyPack(s)` `getOrganizationMember(s)` `getInsightsAccount(s)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- These rows exist only against Pulumi Cloud: the provider binds `accessToken` from the Doppler fan-in and activates when a `StackSpec` names a Cloud-backed stack, where the self-managed backend instead holds its local owners (`Drift.sweep`, `Automation.ephemeral`, the receipt ledger).

[STACKING]:
- `@pulumi/esc-sdk`(`.api/pulumi-esc-sdk.md`): `Environment` authors graph-owned environments declaratively while the SDK's `EscApi` owns out-of-graph reads and writes over the same objects — one writer per environment, `EnvironmentVersionTag` pinning consumers so rotation is a tag move.
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): `DeploymentSchedule.pulumiOperation` is the Automation ledger's mutating subset, and the Automation-API `RemoteWorkspace` stays the demoted execution alternative to `DeploymentSettings.vcs`, never the trigger surface.
- within-lib: `DriftSchedule` feeds the local `DriftReport` fold that `previewRefresh` produces, so a hosted schedule doubled over a local sweep on one stack double-clocks — pick per backend; a drift-filter `Webhook` lands beside the Doppler secret-change webhook as one evidence-delivery law.

[LOCAL_ADMISSION]:
- `vcs` over the deprecated `github` block in every settings row; deploy credentials ride `operationContext.oidc` or `gitAuth` bound from Doppler, `agentPoolId` routes execution onto owned runners, and `autoRemediate` stays false where the estate's law is evidence-then-deliberate-`refresh`.
- Access tokens mint at the narrowest scope the `build*Permissions` helpers express — a stack-scoped team token over an org token for single-stack automation.

[RAIL_LAW]:
- Package: `@pulumi/pulumiservice`
- Owns: Pulumi Cloud control-plane configuration — deployment settings, schedules, review-stack policy, webhooks, ESC environment authoring, org RBAC, scoped tokens, agent pools, OIDC trust
- Accept: `vcs`-block triggers, deliberate-remediation `DriftSchedule`, `TtlSchedule` on ephemeral estates, asset-shaped `Environment` with version-tag pinning, drift/deployment webhook filters, `build*Permissions`-scoped tokens
- Reject: hand-rolled `api.pulumi.com` REST, the deprecated `github` block, `autoRemediate` where evidence-then-`refresh` is law, hosted schedules doubled over local sweeps, dual writers per environment, org-wide tokens for single-stack consumers
