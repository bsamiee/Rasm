# [TS_IAC_API_PULUMIVERSE_GRAFANA]

`@pulumiverse/grafana` mints the Grafana resource vocabulary of the deploy plane — dashboards, folders, data sources, alert rule groups, contact points, notification policies, SLOs — each a `pulumi.CustomResource` row realized inside the Automation-API inline program. It adds resource vocabulary, never a deployment mechanism: one bridged `Provider` carries the full auth surface and every resource folds through it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumiverse/grafana`
- package: `@pulumiverse/grafana` (Apache-2.0)
- module: CJS (`type: commonjs`); barrel `index.d.ts` re-exports `provider` flat, one `import * as ns` per resource namespace, with `config` and `types`
- asset: Grafana resource vocabulary — boards, alerts, SLOs, folders, data sources, RBAC grants — as bridged `CustomResource` classes with `get*` data sources
- runtime: `node` — the plugin binary `pulumi-resource-grafana` is a deploy-host fact the Pulumi CLI resolves at `up` time, never a JS import; the JS package is the typed SDK only
- plane: `plane:deploy` — the generated ban list scopes `@pulumi/*` and `@pulumiverse/*` to `iac`, depended on by nothing at runtime
- rail: deployment / observability-resource

## [02]-[PROVIDER]

One `Provider` carries the full auth surface; every resource rides package-wide config or takes an explicit instance through `opts.provider`. `auth` is Doppler-sourced as a `pulumi.Input<string>`.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Provider`     | class         | explicit `pulumi.ProviderResource` for fine-grained auth |
|  [02]   | `ProviderArgs` | interface     | the auth/endpoint bag, all `pulumi.Input<…>`             |

[PROVIDER]: `Provider(string,ProviderArgs?,pulumi.ResourceOptions?)` `Provider.isInstance(any) -> obj is Provider`
[PROVIDER_ARGS]: `ProviderArgs.url: pulumi.Input<string>` `ProviderArgs.auth: pulumi.Input<string>` `ProviderArgs.orgId: pulumi.Input<number>` `ProviderArgs.stackId: pulumi.Input<number>` `ProviderArgs.cloudAccessPolicyToken: pulumi.Input<string>` `ProviderArgs.cloudApiUrl: pulumi.Input<string>` `ProviderArgs.cloudProviderAccessToken: pulumi.Input<string>` `ProviderArgs.cloudProviderUrl: pulumi.Input<string>` `ProviderArgs.connectionsApiAccessToken: pulumi.Input<string>` `ProviderArgs.connectionsApiUrl: pulumi.Input<string>` `ProviderArgs.fleetManagementAuth: pulumi.Input<string>` `ProviderArgs.fleetManagementUrl: pulumi.Input<string>` `ProviderArgs.frontendO11yApiAccessToken: pulumi.Input<string>` `ProviderArgs.frontendO11yApiUrl: pulumi.Input<string>` `ProviderArgs.oncallAccessToken: pulumi.Input<string>` `ProviderArgs.oncallUrl: pulumi.Input<string>` `ProviderArgs.smAccessToken: pulumi.Input<string>` `ProviderArgs.smUrl: pulumi.Input<string>` `ProviderArgs.k6AccessToken: pulumi.Input<string>` `ProviderArgs.k6Url: pulumi.Input<string>` `ProviderArgs.caCert: pulumi.Input<string>` `ProviderArgs.tlsCert: pulumi.Input<string>` `ProviderArgs.tlsKey: pulumi.Input<string>` `ProviderArgs.insecureSkipVerify: pulumi.Input<boolean>` `ProviderArgs.httpHeaders: pulumi.Input<{[k:string]:pulumi.Input<string>}>` `ProviderArgs.retries: pulumi.Input<number>` `ProviderArgs.retryStatusCodes: pulumi.Input<pulumi.Input<string>[]>` `ProviderArgs.retryWait: pulumi.Input<number>` `ProviderArgs.storeDashboardSha256: pulumi.Input<boolean>`

## [03]-[RESOURCE_PATTERN]

Every resource in every namespace is the same parameterized shape, never a per-resource API: each class extends `pulumi.CustomResource`, carries an input `*Args` and a rehydration `*State`, and each namespace pairs resources with `get*` data-source functions. `Folder` is the exemplar.

[FOLDER]: `Folder(string,FolderArgs,pulumi.CustomResourceOptions?)` `Folder.get(string,pulumi.Input<pulumi.ID>,FolderState?,pulumi.CustomResourceOptions?) -> Folder` `Folder.isInstance(any) -> obj is Folder` `Folder.uid: pulumi.Output<string>`
[FOLDER_ARGS]: `FolderArgs.title: pulumi.Input<string>` `FolderArgs.uid: pulumi.Input<string>` `FolderArgs.parentFolderUid: pulumi.Input<string>` `FolderArgs.orgId: pulumi.Input<string>` `FolderArgs.preventDestroyIfNotEmpty: pulumi.Input<boolean>`
[SURFACES]: `getDashboard(GetDashboardArgs?,pulumi.InvokeOptions?) -> Promise<GetDashboardResult>`

## [04]-[NAMESPACE_ROSTER]

A namespace is a new row on the resource pattern, never a new mechanism. Telemetry consumers touch `oss`, `alerting`, and `slo`; the rest ship prepared.

[CONSUMED]: telemetry-touched namespaces and their resource rosters
- [01]-`oss` (boards): `Dashboard` (`configJson` required; `folder?`, `message?`, `orgId?`, `overwrite?`), `DashboardPublic`, `Folder`, `DataSource`, `DataSourceConfig`, `LibraryPanel`, `Playlist`, `Organization`, `OrganizationPreferences`, `Team`, `User`, `ServiceAccount`, `ServiceAccountToken`, `ServiceAccountRotatingToken`, `SsoSettings`, `Annotation`, a `<resource>Permission`/`<resource>PermissionItem` RBAC pair per dashboard, folder, and service account, and `get*` data sources.
- [02]-`alerting` (alerts): `RuleGroup` (`folderUid`, `intervalSeconds`, `rules` required; `name?`, `orgId?`, `disableProvenance?`), `ContactPoint` (`name` with one array per channel: `emails`, `slacks`, `webhooks`, …), `NotificationPolicy`, `MuteTiming`, `MessageTemplate`, `AlertEnrichment`, `AlertRuleV0Alpha1`, `RecordingRuleV0Alpha1`.
- [03]-`slo` (SLOs): `SLO` and `getSlos`.

[PREPARED]: the remaining namespaces on the same pattern; `types` is the shared input/output interface library, not a resource row

| [INDEX] | [NAMESPACE]                                                                | [OWNS]                                         |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `cloud`                                                                    | Grafana Cloud stacks, access policies, plugins |
|  [02]   | `machinelearning`                                                          | ML jobs, holidays, outlier detectors           |
|  [03]   | `oncall`                                                                   | schedules, escalation chains, integrations     |
|  [04]   | `syntheticmonitoring`                                                      | probes, checks                                 |
|  [05]   | `cloudprovider`                                                            | AWS/Azure/GCP CloudWatch scrape jobs           |
|  [06]   | `connections`                                                              | metrics endpoints, collector configs           |
|  [07]   | `fleetmanagement`                                                          | collector fleet pipelines                      |
|  [08]   | `frontendobservability`                                                    | RUM apps                                       |
|  [09]   | `k6`                                                                       | load-test projects; mirrors the k6 e2e lane    |
|  [10]   | `apps` · `assert` · `assistant` · `enterprise` · `experimental` · `config` | app/enterprise/overlay resource rows           |
|  [11]   | `types`                                                                    | shared input/output interface library          |

## [05]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): resources bind LGTM service URLs as `Output<string>` (an `oss.DataSource` takes `{ url: prometheus.url, type: "prometheus" }`), never literals, and `pulumi.interpolate`/`Output.apply` weave dashboard JSON referencing those data-source UIDs; `Stack.previewRefresh({ onEvent })` streams each dashboard/alert divergence as a `resourcePreEvent` whose `StepEventMetadata.op` is an `OpType` and `detailedDiff` the per-property delta, reconciled against `PreviewResult.changeSummary` (`OpMap`), so a UI-hand-edited board surfaces as an `update` op; a bridged Grafana-API error surfaces in the engine stream as a `DiagnosticEvent` matched on `severity`, never message text.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `Provider.auth` binds the config-scoped token env-injected through `doppler run` into `Config.redacted`, the grafana consumer row of the credential fan-in, so the Grafana API token never enters a span, log, or state file in cleartext.
- within-lib: `telemetry/board` models realize as `oss`/`alerting`/`slo` rows inside the `LocalWorkspace.createOrSelectStack` inline `program: PulumiFn`, one `new grafana.Provider(...)` per `StackSpec` endpoint with `{ provider }` on every resource; `effect` `Match.exhaustive` selects the provider arm, `Layer` composes the sub-program, `Schema` types `StackSpec` and `StackOutputs`, and `storeDashboardSha256: true` compares content hashes for the kernel `ContentKey` drift discipline.

[RAIL_LAW]:
- Package: `@pulumiverse/grafana`
- Owns: the Grafana resource vocabulary — dashboards, folders, data sources, alert rule groups, contact points, notification policies, SLOs — as `CustomResource` rows applied by the deploy plane
- Accept: one Doppler-sourced `Provider` per stack; resources fed LGTM `Output<T>` URLs; construction inside the Automation-API inline program with `{ provider }` on every resource; `storeDashboardSha256` for content-hash drift; `oss.ServiceAccountRotatingToken` as the durable automation credential over an Editor account scoped by one `oss.FolderPermissionItem`; `orgId` (`Input<string>`) tenant scoping from the realized `oss.Organization.orgId` (`Output<number>`); explicit org-scoped `uid` on every `oss.DataSource` so one compiled dashboard JSON binds identically in every org
- Reject: an inline `auth` token; authored `Pulumi.yaml` or dashboard-JSON files on disk; importing outside `iac`; a `get*` data source where a managed resource with `Output` wiring belongs; `alerting.AlertEnrichment`, a Grafana Cloud preview surface with no self-hosted OSS target
