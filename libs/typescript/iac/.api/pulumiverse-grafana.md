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

Two arg shapes ride that one pattern. Flat-arg resources spell their fields directly on `*Args` and mark each required there. App-platform resources — everything under a versioned sub-namespace — spell `*Args` as an all-optional `{ metadata, options, spec }` triple whose required fields sit one level down, so the type admits an empty construction that the provider then refuses; a fence reads the block rosters, never the arg signature, to learn what a resource demands.

[FOLDER]: `Folder(string,FolderArgs,pulumi.CustomResourceOptions?)` `Folder.get(string,pulumi.Input<pulumi.ID>,FolderState?,pulumi.CustomResourceOptions?) -> Folder` `Folder.isInstance(any) -> obj is Folder` `Folder.uid: pulumi.Output<string>`
[FOLDER_ARGS]: `FolderArgs.title: pulumi.Input<string>` `FolderArgs.uid: pulumi.Input<string>` `FolderArgs.parentFolderUid: pulumi.Input<string>` `FolderArgs.orgId: pulumi.Input<string>` `FolderArgs.preventDestroyIfNotEmpty: pulumi.Input<boolean>`
[SURFACES]: `getDashboard(GetDashboardArgs?,pulumi.InvokeOptions?) -> Promise<GetDashboardResult>`

## [04]-[NAMESPACE_ROSTER]

Each namespace lands as a new row on the resource pattern. Telemetry consumers touch `oss`, `alerting`, and `slo`; the rest ship prepared.

[CONSUMED]: telemetry-touched namespaces and their resource rosters
- [01]-`oss` (boards): `Dashboard` (`configJson` required; `folder?`, `message?`, `orgId?`, `overwrite?`), `DashboardPublic`, `Folder`, `DataSource`, `DataSourceConfig`, `LibraryPanel`, `Playlist`, `Organization`, `OrganizationPreferences`, `Team`, `User`, `ServiceAccount`, `ServiceAccountToken`, `ServiceAccountRotatingToken`, `SsoSettings`, `Annotation`, a `<resource>Permission`/`<resource>PermissionItem` RBAC pair per dashboard, folder, and service account, and `get*` data sources.
- [02]-`alerting` (alerts): `RuleGroup` (`folderUid`, `intervalSeconds`, `rules` required; `name?`, `orgId?`, `disableProvenance?`), `ContactPoint` (`name` with one array per channel: `emails`, `slacks`, `webhooks`, …), `NotificationPolicy`, `MuteTiming`, `MessageTemplate`, and the two versioned sub-namespaces `alerting.v0alpha1` (`AlertRule`, `RecordingRule`, `RuleSequence`) and `alerting.v1beta1` (`AlertEnrichment`, `NotificationsInhibitionRule`, `NotificationsRoutingTree`).
- [03]-`slo` (SLOs): `SLO` and `getSlos`.

[DATA_SOURCE_ARGS]: `type` REQUIRED beside `uid?` `url?` `name?` `orgId?` `accessMode?` (`proxy | direct`) `isDefault?` `databaseName?` `username?` `basicAuthEnabled?` `basicAuthUsername?` `httpHeaders?` `jsonDataEncoded?` `secureJsonDataEncoded?` `privateDataSourceConnectNetworkId?`.
[DATA_SOURCE_TRAPS]: both `jsonData` halves cross as SERIALIZED JSON STRINGS, never typed structs — `jsonDataEncoded` and `secureJsonDataEncoded` take whatever camelCased key set the target plugin's own config declares, so the plugin's config record is the contract a fence transcribes and this resource type checks none of it; `url` is generic and a plugin dialing its own host/port coordinates ignores it outright. `orgId` takes `Input<string>` while `oss.Organization.orgId` yields `Output<number>`, so an org-scoped row renders the realized number through `.apply(String)`.
[CUSTOM_HEADERS]: `httpHeaders` (`Input<{[k:string]:Input<string>}>`) is the ONE seat. Grafana core stores each header one-indexed and dense as `httpHeaderName<N>` in the plain document beside `httpHeaderValue<N>` in the secure one, so this attribute writes that key space alone. `operate/observe#BOARD_APPLY` fills it from the scope header an `org`-tenancy store row demands, and a `label`-tenancy row hands the empty record.
[HEADER_REFUSAL]: hand-folding that indexed pair into the encoded bodies is REFUTED rather than merely redundant — this provider RESERVES both spellings, `jsonDataEncoded` refusing any string containing `httpHeaderName` and `secureJsonDataEncoded` any containing `httpHeaderValue`, each naming `httpHeaders` as the replacement. Such a fold fails validation before one API call, which makes the typed map coexist with both documents by construction. `teamHttpHeaders` is reserved on the same validator and routes to `oss.DataSourceConfigLbacRules`.

[VERSIONED_SPLIT]: the versioned sub-namespace owns every resource it declares, so a fence spells `alerting.v0alpha1.RecordingRule` and reads its required halves off the metadata and spec blocks below; the namespace root re-exports the same resources under flat names that carry no capability of their own.

[RECORDING_RULE]: `alerting.v0alpha1.RecordingRule(string,RecordingRuleArgs?,pulumi.CustomResourceOptions?)` `RecordingRule.get(string,pulumi.Input<pulumi.ID>,RecordingRuleState?,pulumi.CustomResourceOptions?) -> RecordingRule` `RecordingRule.isInstance(any) -> obj is RecordingRule` `RecordingRule.metadata: pulumi.Output<RecordingRuleMetadata|undefined>` `RecordingRule.options: pulumi.Output<RecordingRuleOptions|undefined>` `RecordingRule.spec: pulumi.Output<RecordingRuleSpec|undefined>`
[RECORDING_RULE_ARGS]: `RecordingRuleArgs.metadata: pulumi.Input<RecordingRuleMetadata>` `RecordingRuleArgs.options: pulumi.Input<RecordingRuleOptions>` `RecordingRuleArgs.spec: pulumi.Input<RecordingRuleSpec>` — every arg optional at the type, so the required halves live one level down
[RECORDING_RULE_METADATA]: `uid: pulumi.Input<string>` REQUIRED `annotations?: pulumi.Input<{[k:string]:pulumi.Input<string>}>` `folderUid?: pulumi.Input<string>` `url?: pulumi.Input<string>` `uuid?: pulumi.Input<string>` `version?: pulumi.Input<string>` — the folder binding is metadata, never spec
[RECORDING_RULE_SPEC]: `expressions: pulumi.Input<{[k:string]:pulumi.Input<string>}>` REQUIRED `metric: pulumi.Input<string>` REQUIRED `targetDatasourceUid: pulumi.Input<string>` REQUIRED `title: pulumi.Input<string>` REQUIRED `labels?: pulumi.Input<{[k:string]:pulumi.Input<string>}>` `paused?: pulumi.Input<boolean>` `trigger?: pulumi.Input<RecordingRuleSpecTrigger>`
[RECORDING_RULE_TRIGGER]: `RecordingRuleSpecTrigger.interval: pulumi.Input<string>` REQUIRED — the evaluation cadence, a duration string
[RECORDING_RULE_OPTIONS]: `managerIdentity?: pulumi.Input<string>` `overwrite?: pulumi.Input<boolean>` — the manager identity distinguishes stacks targeting one Grafana
[RECORDING_RULE_TRAPS]: `expressions` is a map of refId to a JSON-ENCODED expression stage, not a typed struct — each value carries `model` (the datasource query with its own `refId`/`expr`/`instant`/`range`), `datasource_uid`, `relative_time_range.{from,to}`, `query_type`, and `source`, so the rule's query is a string a fence assembles rather than a member the type checks; `metric` names the series the rule WRITES into `targetDatasourceUid`, which makes the target datasource's write path — not its read path — the availability question a consumer answers before landing a rule.

[RULE_SEQUENCE]: `alerting.v0alpha1.RuleSequence(string,RuleSequenceArgs?,pulumi.CustomResourceOptions?)` `RuleSequence.get(string,pulumi.Input<pulumi.ID>,RuleSequenceState?,pulumi.CustomResourceOptions?) -> RuleSequence` `RuleSequence.isInstance(any) -> obj is RuleSequence` `RuleSequence.metadata: pulumi.Output<RuleSequenceMetadata|undefined>` `RuleSequence.spec: pulumi.Output<RuleSequenceSpec|undefined>` — the app-platform triple, so `RuleSequenceArgs` admits an empty construction the provider then refuses
[RULE_SEQUENCE_METADATA]: `uid: pulumi.Input<string>` REQUIRED beside `annotations?` `folderUid?` `url?` `uuid?` `version?` — the `RecordingRuleMetadata` shape verbatim, folder binding on metadata
[RULE_SEQUENCE_SPEC]: `recordingRules: pulumi.Input<{name}[]>` REQUIRED `alertingRules?: pulumi.Input<{name}[]>` `trigger?: RuleSequenceSpecTrigger` whose `interval` REQUIRED sets one cadence for every member — each entry names a rule by its UID and the array ORDER is the evaluation order, so the sequence carries dependency between rules that a `RuleGroup` interval cannot express
[RULE_SEQUENCE_LAW]: a sequence orders resources it never owns, so it inherits their write posture whole — `recordingRules` is required, which makes every sequence a Grafana-managed recording-rule chain writing through `targetDatasourceUid`, and the estate's store-side evaluator takes that capability instead

[NOTIFICATION_POLICY]: `alerting.NotificationPolicy(string,NotificationPolicyArgs,pulumi.CustomResourceOptions?)` — one root tree per org, never one resource per route
[NOTIFICATION_POLICY_ARGS]: `contactPoint: pulumi.Input<string>` REQUIRED `groupBies: pulumi.Input<pulumi.Input<string>[]>` REQUIRED `disableProvenance?: pulumi.Input<boolean>` `groupInterval?: pulumi.Input<string>` `groupWait?: pulumi.Input<string>` `orgId?: pulumi.Input<string>` `policies?: pulumi.Input<pulumi.Input<NotificationPolicyPolicy>[]>` `repeatInterval?: pulumi.Input<string>`
[NOTIFICATION_POLICY_ROUTE]: `NotificationPolicyPolicy.{activeTimings?,contactPoint?,continue?,groupBies?,groupInterval?,groupWait?,matchers?,muteTimings?,policies?,repeatInterval?}` — `groupBies` optional per route and inherited from the parent when absent, `matchers` an array of `{label,match,value}` all required, and the tree carries SIX route levels below the root because each depth generates its own interface named by one more `Policy` suffix, `NotificationPolicyPolicy` through `NotificationPolicyPolicyPolicyPolicyPolicyPolicy`
[NOTIFICATION_POLICY_LEAF]: levels one through five hold that uniform shape; the SIXTH drops `policies?` entirely and re-REQUIRES `groupBies`, so the deepest route is a terminal leaf that cannot nest and must name its grouping — one recursive projection over a uniform route type fails there on both counts, so a fold either bounds itself at five or special-cases the leaf, and a fold bounding shallower than five discards nesting levels the provider accepts
[NOTIFICATION_POLICY_TRAPS]: `groupBies` is REQUIRED at the root and optional at every intermediate child, so a tree omitting it fails at the type rather than defaulting; the special label `...` groups by all labels and disables grouping outright; matchers key on ALERT LABELS alone, so identity a rule carries as an annotation is unroutable, ungroupable, and unsilenceable; `continue` is a bare reserved-word key, so a spread over a route record spells it quoted.

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
|  [09]   | `agento11y`                                                                | evaluators, eval and hook rules, collections   |
|  [10]   | `k6`                                                                       | load-test projects; mirrors the k6 e2e lane    |
|  [11]   | `apps` · `assert` · `assistant` · `enterprise` · `experimental` · `config` | app/enterprise/overlay resource rows           |
|  [12]   | `types`                                                                    | shared input/output interface library          |

## [05]-[IMPLEMENTATION_LAW]

[STACKING]:
- `@pulumi/pulumi`(`.api/pulumi-pulumi.md`): resources bind LGTM service URLs as `Output<string>` (an `oss.DataSource` takes `{ url: prometheus.url, type: "prometheus" }`), never literals, and `pulumi.interpolate`/`Output.apply` weave dashboard JSON referencing those data-source UIDs; `Stack.previewRefresh({ onEvent })` streams each dashboard/alert divergence as a `resourcePreEvent` whose `StepEventMetadata.op` is an `OpType` and `detailedDiff` the per-property delta, reconciled against `PreviewResult.changeSummary` (`OpMap`), so a UI-hand-edited board surfaces as an `update` op; a bridged Grafana-API error surfaces in the engine stream as a `DiagnosticEvent` matched on `severity`, never message text.
- `@pulumiverse/doppler`(`.api/pulumiverse-doppler.md`): `Provider.auth` binds the config-scoped token env-injected through `doppler run` into `Config.redacted`, the grafana consumer row of the credential fan-in, so the Grafana API token never enters a span, log, or state file in cleartext.
- `operate/observe#BOARD_APPLY`: `_alerted` realizes the severity `contacts` record as one `alerting.ContactPoint`/`MessageTemplate`/`MuteTiming` per row beside one root `alerting.NotificationPolicy` whose `groupBies` takes the `_GROUPED` roster and whose route matchers key on `Convention.rasm.sloSeverity`; the `RuleGroup` rules carry the spec's owned `Convention` rows as `labels` precisely because matchers, grouping, and silences read that plane alone.
- within-lib: `telemetry/board` models realize as `oss`/`alerting`/`slo` rows inside the `LocalWorkspace.createOrSelectStack` inline `program: PulumiFn`, one `new grafana.Provider(...)` per `StackSpec` endpoint with `{ provider }` on every resource; `effect` `Match.exhaustive` selects the provider arm, `Layer` composes the sub-program, `Schema` types `StackSpec` and `StackOutputs`, and `storeDashboardSha256: true` compares content hashes for the kernel `ContentKey` drift discipline.

[RAIL_LAW]:
- Package: `@pulumiverse/grafana`
- Owns: the Grafana resource vocabulary — dashboards, folders, data sources, alert rule groups, contact points, notification policies, SLOs — as `CustomResource` rows applied by the deploy plane
- Accept: one Doppler-sourced `Provider` per stack; resources fed LGTM `Output<T>` URLs; every `oss.DataSource` row answering all three settings columns — `jsonDataEncoded`, `secureJsonDataEncoded`, and `httpHeaders` — so a plugin dialing its own coordinates connects, a header lands on its own typed seat, and a row needing none of the three encodes empty documents beside an empty map; construction inside the Automation-API inline program with `{ provider }` on every resource; `storeDashboardSha256` for content-hash drift; `oss.ServiceAccountRotatingToken` as the durable automation credential over an Editor account scoped by one `oss.FolderPermissionItem`; `orgId` (`Input<string>`) tenant scoping from the realized `oss.Organization.orgId` (`Output<number>`); explicit org-scoped `uid` on every `oss.DataSource` so one compiled dashboard JSON binds identically in every org; the versioned owner under `alerting.v0alpha1`/`alerting.v1beta1` for every resource the namespace root also aliases; routing identity carried as alert LABELS so matchers, `groupBies`, and silences all read one plane
- Reject: an inline `auth` token; a hand-folded `httpHeaderName<N>`/`httpHeaderValue<N>` pair inside either encoded document, which the provider's own validators refuse by name; authored `Pulumi.yaml` or dashboard-JSON files on disk; importing outside `iac`; a `get*` data source where a managed resource with `Output` wiring belongs; a flat namespace-root spelling where the versioned owner stands; `alerting.v1beta1.AlertEnrichment`, a Grafana Cloud preview surface with no self-hosted OSS target; a rule whose write target the store row's own ingest posture does not admit, `alerting.v0alpha1.RuleSequence` included, since its required `recordingRules` make every sequence that same write
