# [TS_IAC_API_PULUMI_GITHUB]

`@pulumi/github` provisions the source-control leg of the bootstrap axis — repository shells, branch law, deployment-environment gates, Actions slots, deploy keys, webhooks, org/team RBAC — as typed resources under one `Provider`; secret VALUES never route here, so `iac` authors only the shells the Doppler mirror fills.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/github`
- package: `@pulumi/github` (Apache-2.0)
- module: `@pulumi/github` — flat resource roster + `get*`/`get*Output` data sources + `Provider`
- rail: fabric / source-control
- runtime: Node deploy-host; every op is a GitHub REST/GraphQL call under the provider credential
- depends-on: `@pulumi/pulumi`; composes `@pulumiverse/doppler` (Actions-secret mirror), `@pulumi/tls` (deploy-key material)
- abi-note: the generated quadruple holds roster-wide — `class X extends pulumi.CustomResource` + `XArgs`/`XState` + `X.get`/`X.isInstance`

## [02]-[PROVIDER_SEAM]

[PROVIDER_SCOPE]: one credentialed seam per estate
- Rate posture rides provider knobs: `maxRetries`/`retryDelayMs`/`retryableErrors`/`readDelayMs`/`writeDelayMs`/`parallelRequests`/`maxPerPage`.

| [INDEX] | [FIELD]   | [MEANING]                                                                                       |
| :-----: | :-------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `token`   | `Input<string>` — the Doppler fan-in read (`GITHUB_TOKEN`), never a literal                     |
|  [02]   | `owner`   | `Input<string>` — org/user scope for unqualified names                                          |
|  [03]   | `appAuth` | `{ id, installationId, pemFile }` — GitHub-App identity, the durable-machine upgrade over a PAT |
|  [04]   | `baseUrl` | GitHub Enterprise endpoint row                                                                  |

## [03]-[RESOURCE_FAMILIES]

[FAMILY_SCOPE]: the roster grouped by concern; a new resource is a row on the quadruple, and every data source pairs a `get*Output` graph-threaded twin.

| [INDEX] | [FAMILY]           | [KEY_CLASSES_LOAD_BEARING_ARGS]                                                                                     |
| :-----: | :----------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | repo core          | `Repository` (`visibility`, `securityAndAnalysis`), `RepositoryFile`, `RepositoryTopics`                            |
|  [02]   | repo collaborators | `RepositoryCollaborator`/`RepositoryCollaborators`                                                                  |
|  [03]   | repo security      | `RepositoryDependabotSecurityUpdates`, `RepositoryVulnerabilityAlerts`                                              |
|  [04]   | branch law         | `Branch`, `BranchDefault`, `BranchProtection`, `RepositoryRuleset`, `BranchProtectionV3` (owner rule in branch law) |
|  [05]   | environments       | `RepositoryEnvironment` (`canAdminsBypass`; gate args in the environment law)                                       |
|  [06]   | env deploy policy  | `RepositoryEnvironmentDeploymentPolicy` (`branchPattern` XOR `tagPattern`)                                          |
|  [07]   | Actions secrets    | `ActionsSecret`/`ActionsEnvironmentSecret`/`ActionsOrganizationSecret` (`plaintextValue` XOR `encryptedValue`)      |
|  [08]   | Actions variables  | `ActionsVariable`/`ActionsEnvironmentVariable`/`ActionsOrganizationVariable` (`variableName`, `value`)              |
|  [09]   | Actions config     | `ActionsRunnerGroup`, `ActionsRepositoryPermissions`, the OIDC subject-claim template pair                          |
|  [10]   | deploy keys        | `RepositoryDeployKey` (`repository`, `key`, `title`, `readOnly`)                                                    |
|  [11]   | webhooks           | `RepositoryWebhook` (`events`, `configuration: { url, contentType, secret, insecureSsl }`), `OrganizationWebhook`   |
|  [12]   | org                | `OrganizationSettings` (`defaultRepositoryPermission`, `membersCanCreateRepositories`), `OrganizationRuleset`       |
|  [13]   | team               | `Team` (`privacy`, `parentTeamId`), `TeamMembership` (`role`), `TeamRepository` (`permission`), `Membership`        |
|  [14]   | data sources       | `getRepository`, `getBranch`, `getRelease`, `getTeam`, `getOrganization`, `getUser`                                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- one `Provider` per owner scope, constructed from the fan-in token once and threaded through tier options; `appAuth` supersedes a PAT for durable machine identity, and rate posture rides the provider knobs.
- one branch-law owner per repo: `RepositoryRuleset` for rules-engine estates, `BranchProtection` for classic protection, `BranchProtectionV3` only when adopting a REST-managed repo.
- `RepositoryEnvironment` is the deployment gate shell — `reviewers`, `waitTimer`, `preventSelfReview`, and a `deploymentBranchPolicy` refined by `RepositoryEnvironmentDeploymentPolicy` (`branchPattern` XOR `tagPattern`).

[STACKING]:
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `secretssync.GithubActions` (`syncTarget: "repo"|"org"`) writes Actions secret VALUES from the canonical config into the repo/environment slots, its `environmentName` targeting the `RepositoryEnvironment` shell authored here.
- `@pulumi/tls`(`.api/pulumi-tls.md`): `RepositoryDeployKey.key` binds `PrivateKey.publicKeyOpenssh` (`readOnly: true` the default posture, a write key deliberate) — the private half never enters this provider; `RepositoryWebhook.configuration.secret` binds a Doppler-generated entry the receiving endpoint verifies.
- within-lib: environment names align with the estate's `StackSpec.doppler.config` axis, so the mirror, the gate, and the stack speak one environment vocabulary.

[RAIL_LAW]:
- Package: `@pulumi/github`
- Owns: source-control provisioning — repositories, branch law, deployment-environment gates, Actions slots, deploy keys, webhooks, org/team RBAC
- Accept: environment shells the Doppler mirror fills, `ActionsVariable` for non-secret config, deploy keys from `tls` public halves, one branch-law owner per repo, `appAuth` for durable identity, `get*Output` for graph-threaded reads
- Reject: secret values authored through `ActionsSecret` beside the mirror, private-key material in any arg, `BranchProtectionV3` beside `BranchProtection`, per-resource retry wrappers over provider rate knobs, token literals
