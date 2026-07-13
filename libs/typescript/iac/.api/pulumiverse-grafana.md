# [TS_IAC_API_PULUMIVERSE_GRAFANA]

[PACKAGE_SURFACE]:
- package: `@pulumiverse/grafana` · version `` · license `Apache-2.0`
- module: CJS (`type: commonjs`); barrel `index.d.ts` re-exports `provider` flat + 18 resource namespaces + `types` as `import * as ns`.
- asset: `index.d.ts` (barrel), `provider.d.ts` (the `Provider` + `ProviderArgs`), one `.d.ts` per resource under each namespace folder (class + `Args` + `State`, or a `get*` data-source fn).
- target: a Pulumi bridged provider (Terraform-bridge over the Grafana TF provider). The JS package is the typed SDK ONLY; the `pulumi-resource-grafana` plugin binary is a deploy-host fact resolved by the Pulumi CLI / `LocalWorkspace.installPlugin` at `up` time — never a JS import.
- plane: `plane:deploy` — the generated ban list scopes `@pulumi/*` and `@pulumiverse/*` to `iac` alone; depended on by nothing at runtime.
- rail: deployment / observability-resource.

`@pulumiverse/grafana` is the terminal applier of the `observe/apply` page: the `telemetry/board` design functions emit dashboard models and alert specs, and this provider realizes them as Grafana resources — `oss.Dashboard`/`oss.Folder`/`oss.DataSource` for the boards, `alerting.RuleGroup`/`alerting.ContactPoint`/`alerting.NotificationPolicy` for the alerts, `slo.Slo` for SLOs — inside the same Automation-API inline program that stands up the LGTM stack via `@pulumi/kubernetes` `helm.v4`. Every resource is a row on the `@pulumi/pulumi` `CustomResource` model (`pulumi-pulumi.md`); this package adds Grafana's resource vocabulary, not a new deployment mechanism.

## [01]-[PROVIDER]

One `Provider` (a `pulumi.ProviderResource`) carries the full auth surface; every resource either rides package-wide config or takes an explicit `Provider` via `opts.provider`. The `auth` token is Doppler-sourced (`secret/doppler`), passed as a `pulumi.Input<string>` — never inline.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]             | [CAPABILITY_BOUNDARY]                              |
| :-----: | :------------- | :------------------------ | :------------------------------------------------- |
|  [01]   | `Provider`     | `pulumi.ProviderResource` | explicit provider instance for fine-grained auth   |
|  [02]   | `ProviderArgs` | interface                 | the full auth/endpoint bag — all `pulumi.Input<…>` |

```ts signature
import * as pulumi from "@pulumi/pulumi"
export declare class Provider extends pulumi.ProviderResource {
  constructor(name: string, args?: ProviderArgs, opts?: pulumi.ResourceOptions)
  static isInstance(obj: any): obj is Provider
}
export interface ProviderArgs {
  url?: pulumi.Input<string>                       // Grafana instance URL (GRAFANA_URL)
  auth?: pulumi.Input<string>                      // API token | "user:pass" | "anonymous" — Doppler-sourced
  orgId?: pulumi.Input<number>; stackId?: pulumi.Input<number>
  cloudAccessPolicyToken?: pulumi.Input<string>; cloudApiUrl?: pulumi.Input<string>
  cloudProviderAccessToken?: pulumi.Input<string>; cloudProviderUrl?: pulumi.Input<string>
  connectionsApiAccessToken?: pulumi.Input<string>; connectionsApiUrl?: pulumi.Input<string>
  fleetManagementAuth?: pulumi.Input<string>; fleetManagementUrl?: pulumi.Input<string>
  frontendO11yApiAccessToken?: pulumi.Input<string>; frontendO11yApiUrl?: pulumi.Input<string>
  oncallAccessToken?: pulumi.Input<string>; oncallUrl?: pulumi.Input<string>
  smAccessToken?: pulumi.Input<string>; smUrl?: pulumi.Input<string>
  k6AccessToken?: pulumi.Input<string>; k6Url?: pulumi.Input<string>
  caCert?: pulumi.Input<string>; tlsCert?: pulumi.Input<string>; tlsKey?: pulumi.Input<string>
  insecureSkipVerify?: pulumi.Input<boolean>; httpHeaders?: pulumi.Input<{ [k: string]: pulumi.Input<string> }>
  retries?: pulumi.Input<number>; retryStatusCodes?: pulumi.Input<pulumi.Input<string>[]>; retryWait?: pulumi.Input<number>
  storeDashboardSha256?: pulumi.Input<boolean>     // drift-diff dashboards by content hash, not full JSON
}
```

## [02]-[RESOURCE_PATTERN]

Every resource in every namespace is the SAME parameterized shape — not a per-resource API. Documenting it once is the mechanism; the namespace roster in [03] is seed data. Each class extends `pulumi.CustomResource`, each carries an input `*Args` and a rehydration `*State`, and each namespace pairs resources with `get*` data-source functions.

```ts signature
// The uniform resource shape (exemplar: oss.Folder — every resource matches this).
export declare class Folder extends pulumi.CustomResource {
  constructor(name: string, args: FolderArgs, opts?: pulumi.CustomResourceOptions)   // opts.provider = the Provider
  static get(name: string, id: pulumi.Input<pulumi.ID>, state?: FolderState, opts?: pulumi.CustomResourceOptions): Folder
  static isInstance(obj: any): obj is Folder
  readonly uid: pulumi.Output<string>              // every field surfaces as Output<T> for downstream wiring
}
export interface FolderArgs { title: pulumi.Input<string>; uid?: pulumi.Input<string>; parentFolderUid?: pulumi.Input<string>; orgId?: pulumi.Input<string>; preventDestroyIfNotEmpty?: pulumi.Input<boolean> }
export interface FolderState { /* every Args field, optional, for adoption via get() */ }
// The uniform data-source shape (exemplar: oss.getDashboard):
export declare function getDashboard(args?: GetDashboardArgs, opts?: pulumi.InvokeOptions): Promise<GetDashboardResult>
```

## [03]-[NAMESPACE_ROSTER]

18 resource namespaces + `types` + the `Provider`, all SEED DATA on the [02] pattern. The telemetry consumer touches `oss`, `alerting`, and `slo` (full rosters below); the rest are prepared rows a future capability finalizes.

[CONSUMED]: the three telemetry-touched namespaces and their full resource rosters
- [01]-`oss` (boards): `Dashboard` (`DashboardArgs { configJson (required), folder?, message?, orgId?, overwrite? }`), `DashboardPublic`, `Folder`, `DataSource`, `DataSourceConfig`, `LibraryPanel`, `Playlist`, `Organization`, `Team`, `User`, `ServiceAccount`, `ServiceAccountToken`, `SsoSettings`, `Annotation` + `get*`.
- [02]-`alerting` (alerts): `RuleGroup` (`RuleGroupArgs { folderUid (required), intervalSeconds (required), rules (required), name?, orgId?, disableProvenance? }`), `ContactPoint` (`name` + one array per channel: `emails`, `slacks`, `webhooks`, …), `NotificationPolicy`, `MuteTiming`, `MessageTemplate`, `AlertEnrichment`, `AlertRuleV0Alpha1`, `RecordingRuleV0Alpha1`.
- [03]-`slo` (SLOs): `Slo` + `getSlos`.

[PREPARED]: the remaining namespaces on the same pattern (`types` is the shared interface library, not a resource row)

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

## [04]-[INTEGRATION]

[STACK: `@pulumi/pulumi` `Output`/`Input`] — resources bind to the LGTM stack outputs, not literals. The Prometheus/Loki/Tempo service URLs the `observe/stack` Helm release exposes are `Output<string>`; an `oss.DataSource` takes them directly (`{ url: prometheus.url, type: "prometheus" }`), and `pulumi.interpolate`/`Output.apply` weave dashboard JSON that references those data-source UIDs. `storeDashboardSha256: true` makes the drift diff compare dashboard content hashes, aligning with the kernel `ContentKey` discipline.

[STACK: Automation-API inline program] — grafana resources are constructed INSIDE the `program: PulumiFn` of `LocalWorkspace.createOrSelectStack` (`pulumi-pulumi.md`), one `new grafana.Provider(...)` sourced from the `StackSpec`'s Grafana endpoint, every resource passed `{ provider }`. The program returns dashboard/folder UIDs as stack outputs → `output.ts` typed `StackOutputs`. No `Pulumi.yaml`, zero authored YAML — the whole board topology is lib code.

[STACK: `effect` rails] — the provider-arm choice is the closed `Match.exhaustive` dispatch (`provider/dispatch`); `Layer` composes the observability sub-program; `Schema` types the `StackSpec` (Grafana URL, org, Doppler ref) and the `StackOutputs` receipt. The `auth` token is `@pulumiverse/doppler`-provisioned and injected via `doppler run` (`secret/inject`) — the Grafana API token never enters a span, log, or state file in cleartext.

[STACK: drift fold] — `policy/drift` runs `Stack.previewRefresh({ onEvent })` read-only against the live Grafana state; each dashboard/alert divergence arrives as a `resourcePreEvent` whose `StepEventMetadata.op` is an `OpType` and whose `detailedDiff` is the per-property delta, folded into the drift ledger and reconciled against `PreviewResult.changeSummary` (`OpMap`). A hand-edited dashboard in the Grafana UI surfaces here as an `update` op — the board is code, the UI is drift.

## [05]-[RAIL_LAW]

- Owns: the Grafana resource vocabulary (dashboards, folders, data sources, alert rule groups, contact points, notification policies, SLOs) as Pulumi `CustomResource` rows applied by the deploy plane.
- Accept: one Doppler-sourced `Provider` per stack; resources fed LGTM-stack `Output<T>` URLs, not literals; construction inside the Automation-API inline program with `{ provider }` on every resource; `storeDashboardSha256` for content-hash drift.
- Reject: an inline `auth` token (Doppler-canonical); authored `Pulumi.yaml` or dashboard-JSON files on disk (the `telemetry/board` functions emit the model); importing this package outside `iac` (banned by the generated scope list); a `get*` data source where a managed resource with `Output` wiring belongs.
- Boundary: this is the typed SDK only — the `pulumi-resource-grafana` plugin binary is a deploy-host fact the Pulumi CLI installs; a bridged provider surfaces Grafana-API errors as `DiagnosticEvent`s in the engine stream, matched on `severity`, never on message text.
