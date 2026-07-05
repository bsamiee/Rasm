# [TS_IAC_API_PULUMI_PULUMISERVICE]

`@pulumi/pulumiservice` provisions the Pulumi Cloud control plane as typed resources — deployment settings, drift/TTL/deployment/rotation schedules, ESC environments, webhooks, teams and permissions, access tokens, agent pools, OIDC issuers, and review-stack template sources — so Cloud-side automation is IaC rows, never hand-rolled REST against `api.pulumi.com`. The lane boundary is explicit: the estate's default backend is self-managed (`PULUMI_BACKEND_URL`), where `Drift.sweep` and `Automation.ephemeral` own drift cadence and stack expiry locally; these resources activate only for a Cloud-backed stack, where they are the hosted twins of those same owners — `DriftSchedule` ↔ the reconcile sweep, `TtlSchedule` ↔ the ephemeral bracket, `DeploymentSchedule.pulumiOperation` ↔ the mutating ledger vocabulary, `Webhook` drift filters ↔ the evidence-delivery law the Doppler webhook already carries. The division of labor is fixed: the Automation API is an execution backend; trigger, schedule, and review-stack POLICY is this package's data.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumiservice`
- package: `@pulumi/pulumiservice`
- license: Apache-2.0
- import: `@pulumi/pulumiservice` → the resource roster + `Provider` + the `get*`/`get*Output` reads + the `build*Permissions` token-scope helpers
- owner: `iac`
- rail: operate / cloud-control-plane
- runtime: Node deploy-host; every resource is a Pulumi Cloud API object under `ProviderArgs { accessToken?, apiUrl? }`
- depends-on: `@pulumi/pulumi`; pairs with `@pulumi/esc-sdk` (imperative twin over the same `Environment` objects)
- capability: deployment settings with VCS triggers and review stacks, drift/TTL/operation/rotation schedules, ESC environment authoring with version tags, event webhooks across stack/deployment/drift/environment groups, org RBAC (teams, permissions, roles, members), scoped access tokens, agent pools, OIDC trust, template sources
- abi-note: `DeploymentSettingsArgs.github` carries the deprecation marker — `vcs` is the current trigger block; `Environment.yaml` is `Input<Asset | Archive>`, never a plain string

## [02]-[SCHEDULES_AND_SETTINGS]

[SCHEDULE_SCOPE]: the schedule family — one concern per class, all keyed `organization`/`project`/`stack`
- rail: cloud-control-plane

| [INDEX] | [SYMBOL] | [SHAPE / MEANING] |
| :-----: | :------- | :---------------- |
|  [01]   | `DriftSchedule` | `{ scheduleCron, autoRemediate? }` — hosted drift detection; `autoRemediate: false` mirrors the local law that a mutating `refresh` is a deliberate choice after reading evidence |
|  [02]   | `TtlSchedule` | `{ timestamp, deleteAfterDestroy? }` — stack expiry; the hosted twin of the `Automation.ephemeral` acquire/destroy bracket |
|  [03]   | `DeploymentSchedule` | `{ pulumiOperation: PulumiOperation, scheduleCron? XOR timestamp? }` — cron or one-shot runs; `PulumiOperation` is `"update" \| "preview" \| "refresh" \| "destroy"`, the mutating-ledger subset (reconcile stays a local leg) |
|  [04]   | `EnvironmentRotationSchedule` | the ESC rotated-secrets cadence row |

[SETTINGS_SCOPE]: `DeploymentSettings` — the per-stack execution contract
- rail: cloud-control-plane
- One resource carries source, execution, and trigger posture: `sourceContext.git { repoUrl, branch, repoDir, commit, gitAuth: { sshAuth { sshPrivateKey, password? } | basicAuth { username, password } } }`, `operationContext { environmentVariables, preRunCommands, options, oidc }`, `executorContext { executorImage }`, `agentPoolId` (self-hosted runners), `cacheOptions { enable }`, and `vcs { provider (required), repository, deployCommits, previewPullRequests, deployPullRequest, pullRequestTemplate, paths, installationId }` — the current trigger block (`github` is its deprecated shape). Review stacks are data here: `vcs.previewPullRequests` plus `pullRequestTemplate` mint per-PR ephemeral stacks, `TtlSchedule` bounds their lifetime, and `TemplateSource` publishes the template estates fork from.

## [03]-[EVENTS_AND_ACCESS]

[EVENT_SCOPE]: `Webhook` — one delivery row per sink
- rail: cloud-control-plane
- `{ active, displayName, organizationName, payloadUrl, projectName?, stackName?, environmentName?, secret?, format?, filters?, groups? }` — org-, stack-, or environment-scoped by which coordinates are present. `WebhookFormat`: `"raw" | "slack" | "ms_teams" | "pulumi_deployments"`. `WebhookFilters` spans stack lifecycle (`StackCreated`/`StackDeleted`), operation verdicts (`UpdateSucceeded`/`UpdateFailed`, preview/destroy/refresh twins), deployment lifecycle (`DeploymentQueued`/`DeploymentStarted`/`DeploymentSucceeded`/`DeploymentFailed`), the drift set (`DriftDetected`, `DriftDetectionSucceeded`/`Failed`, `DriftRemediationSucceeded`/`Failed`), and the environment set (revision/tag created-retracted-updated rows, `EnvironmentRotationSucceeded`/`Failed`).

[ACCESS_SCOPE]: identity, RBAC, and environment rows
- rail: cloud-control-plane

| [INDEX] | [FAMILY] | [MEMBERS] |
| :-----: | :------- | :-------- |
|  [01]   | environments | `Environment { organization, project?, name, yaml: Input<Asset \| Archive> }` (outputs `environmentId`, `revision`), `EnvironmentVersionTag` — the declarative twin of the `esc-sdk` writer |
|  [02]   | RBAC | `Team`, `TeamStackPermission`, `TeamEnvironmentPermission`, `TeamRoleAssignment`, `OrganizationMember`, `OrganizationRole`, `ApprovalRule` |
|  [03]   | tokens | `AccessToken`, `OrgAccessToken`, `TeamAccessToken` + the `buildStackScopedPermissions`/`buildEnvironmentScopedPermissions`/`buildAllowPermissions` scope helpers |
|  [04]   | execution | `AgentPool` (self-hosted deploy runners), `OidcIssuer` (federated trust into the org), `TemplateSource` (review-stack/new-project templates) |
|  [05]   | governance | `PolicyGroup`, `PolicyPack`, `Stack`, `StackTag`/`StackTags`, `InsightsAccount` |
|  [06]   | reads | `getCurrentUser`, `getEnvironment`, `getPolicyPack(s)`, `getOrganizationMember(s)`, `getInsightsAccount(s)` — each with its `*Output` twin |

## [04]-[IMPLEMENTATION_LAW]

[CONTROL_PLANE_TOPOLOGY]:
- lane law: these rows exist only against Pulumi Cloud — the provider binds `accessToken` from the Doppler fan-in and activates when a `StackSpec` value names a Cloud-backed stack; on the self-managed backend the local owners hold (`Drift.sweep`, `Automation.ephemeral`, the receipt ledger), and configuring Cloud automation by REST call instead of these resources is the hand-rolled defect this package exists to delete.
- twin law: hosted and local automation share one vocabulary — `DeploymentSchedule.pulumiOperation` is the ledger's mutating subset, `DriftSchedule` produces the evidence the local `DriftReport` fold reads from `previewRefresh` locally, and `autoRemediate` stays false wherever the estate's law is evidence-then-deliberate-`refresh`; a hosted schedule and a local sweep both watching one stack is double-clocking, pick per backend.
- review-stack law: per-PR stacks are `DeploymentSettings.vcs.previewPullRequests` + `pullRequestTemplate` + a `TtlSchedule` bound — policy as three data rows; the Automation-API `RemoteWorkspace` stays the demoted execution alternative, never the trigger configuration surface.
- settings law: `vcs` over the deprecated `github` block in every new settings row; deploy credentials ride `operationContext.oidc` or `gitAuth` bound from Doppler reads, `preRunCommands`/`environmentVariables` carry run posture, and `agentPoolId` routes execution onto owned runners when data must not transit shared compute.
- environment law: one writer per environment — this package's `Environment` (yaml as a `StringAsset`/`FileAsset`) for graph-owned environments, the `esc-sdk` client (`.api/pulumi-esc-sdk.md`) for out-of-graph ones; `EnvironmentVersionTag` pins consumers so rotation is a tag move. Webhook `secret` values are Doppler-generated entries, and a drift-filter webhook lands beside the Doppler secret-change webhook as one evidence-delivery law with two sources.
- token law: access tokens mint at the narrowest scope the `build*Permissions` helpers can express — a stack-scoped team token over an org token wherever the consumer is one stack's automation.

[RAIL_LAW]:
- Package: `@pulumi/pulumiservice`
- Owns: Pulumi Cloud control-plane configuration — deployment settings, schedules, review-stack policy, webhooks, ESC environment authoring, org RBAC, scoped tokens, agent pools, OIDC trust
- Accept: `vcs`-block triggers, `DriftSchedule` with deliberate remediation, `TtlSchedule` on ephemeral estates, `Environment` with asset-shaped yaml and version-tag pinning, drift/deployment webhook filters, `build*Permissions`-scoped tokens
- Reject: hand-rolled `api.pulumi.com` REST, the deprecated `github` settings block, `autoRemediate` where evidence-then-refresh is law, hosted schedules doubled over local sweeps on one stack, dual writers per environment, org-wide tokens for single-stack consumers
