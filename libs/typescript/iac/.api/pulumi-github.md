# [TS_IAC_API_PULUMI_GITHUB]

`@pulumi/github` is the Terraform-bridged provider for the source-control leg of the bootstrap axis: repositories, branch law, deployment environments, Actions configuration, deploy keys, webhooks, and org/team RBAC as typed resources under one `Provider` that binds `token`/`owner` (or a GitHub-App `appAuth`). The package is the standard generated quadruple (`class X extends pulumi.CustomResource` + `XArgs` + `XState` + `X.get`/`X.isInstance`) applied across a large roster — learn the families, not the classes. In `iac` it provisions the SHELLS the delivery pipeline lands on: the `RepositoryEnvironment` rows are what the Doppler `secretssync.GithubActions` mirror populates (`Secrets.mirrored` — secret VALUES never route through this provider), `RepositoryDeployKey` binds a `tls.PrivateKey.publicKeyOpenssh` so machine access keys stay in the one entropy owner, and `RepositoryWebhook` is the event seam a deploy-triggering endpoint subscribes through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/github`
- package: `@pulumi/github`
- license: Apache-2.0
- import: `@pulumi/github` → the flat resource roster + `get*`/`get*Output` data sources + `Provider`
- owner: `iac`
- rail: fabric / source-control
- runtime: Node deploy-host; every operation is a GitHub REST/GraphQL call under the provider credential
- depends-on: `@pulumi/pulumi`; composes `@pulumiverse/doppler` (the Actions-secret mirror), `@pulumi/tls` (deploy-key material)
- capability: repository/branch-law/ruleset provisioning, deployment environments with reviewer and branch-policy gates, Actions secret/variable slots at repo/environment/org scope, deploy keys, webhooks, org settings, team RBAC
- abi-note: the generated quadruple holds roster-wide; `BranchProtectionV3` is the retired REST twin of `BranchProtection` — one branch-law owner per repo, never both

## [02]-[PROVIDER_SEAM]

[PROVIDER_SCOPE]: one credentialed seam per estate
- rail: source-control
- Rate posture is provider data, never per-resource handling: `maxRetries`/`retryDelayMs`/`retryableErrors`/`readDelayMs`/`writeDelayMs`/`parallelRequests`/`maxPerPage`.

| [INDEX] | [FIELD]   | [MEANING]                                                                                       |
| :-----: | :-------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `token`   | `Input<string>` — the Doppler fan-in read (`GITHUB_TOKEN`), never a literal                     |
|  [02]   | `owner`   | `Input<string>` — org/user scope for unqualified names (`organization` is its deprecated alias) |
|  [03]   | `appAuth` | `{ id, installationId, pemFile }` — GitHub-App identity, the durable-machine upgrade over a PAT |
|  [04]   | `baseUrl` | GitHub Enterprise endpoint row                                                                  |

## [03]-[RESOURCE_FAMILIES]

[FAMILY_SCOPE]: the roster grouped by concern — each row is the generated quadruple
- rail: source-control
- Args the `[IMPLEMENTATION_LAW]` below already governs (environment gates, mirror slots, branch-law owner) are stated there; the cells carry class names plus the remaining load-bearing args. `Repository` carries `name`/`visibility`/`autoInit`/`pages`/`securityAndAnalysis`; `Team` carries `name`; `OrganizationSettings` also carries `name`/`billingEmail`/`webCommitSignoffRequired`; `ActionsSecret*` carry `secretName`; `RepositoryWebhook` carries `active`. Each data source pairs a `get*Output` graph-threaded twin.

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

[SOURCE_CONTROL_TOPOLOGY]:
- mirror law: Actions secret VALUES arrive through the Doppler mirror — `Secrets.mirrored`'s `secretssync.GithubActions` row (`.api/pulumiverse-doppler.md`) writes FROM the canonical config into the repo/environment slots; `ActionsSecret.plaintextValue` is admitted only for a value that is not secret material misfiled as one, and a credential authored here instead of mirrored is the second-source-of-truth defect. `ActionsVariable` rows own non-secret configuration freely.
- environment law: `RepositoryEnvironment` is the deployment gate shell — `reviewers`, `waitTimer`, `preventSelfReview`, and a `deploymentBranchPolicy` refined by `RepositoryEnvironmentDeploymentPolicy` patterns; the environment names align with the estate's `StackSpec.doppler.config` axis so the mirror, the gate, and the stack speak one environment vocabulary.
- key law: `RepositoryDeployKey.key` binds `tls.PrivateKey.publicKeyOpenssh` (`.api/pulumi-tls.md`) — the private half never enters this provider; `readOnly: true` is the default posture and a write key is a deliberate row. The webhook `configuration.secret` binds a Doppler-generated entry the receiving endpoint verifies.
- branch-law law: one owner per repo — `RepositoryRuleset` for rules-engine estates, `BranchProtection` where classic protection suffices, `BranchProtectionV3` only when adopting a pre-existing REST-managed repo; two branch-law owners on one repo is the split the family table forbids.
- provider law: one `Provider` per owner scope, constructed from the fan-in token exactly once and threaded through tier options; `appAuth` supersedes a PAT when the estate earns a durable machine identity, and rate posture lives on the provider knobs, never in retry loops around resources.

[RAIL_LAW]:
- Package: `@pulumi/github`
- Owns: source-control provisioning — repositories, branch law, deployment-environment gates, Actions slots, deploy keys, webhooks, org/team RBAC
- Accept: environment shells the Doppler mirror fills, `ActionsVariable` for non-secret config, deploy keys from `tls` public halves, one branch-law owner per repo, `appAuth` for durable identity, `get*Output` for graph-threaded reads
- Reject: secret values authored through `ActionsSecret` beside the mirror, private-key material in any arg, `BranchProtectionV3` beside `BranchProtection`, per-resource retry wrappers over provider rate knobs, token literals
