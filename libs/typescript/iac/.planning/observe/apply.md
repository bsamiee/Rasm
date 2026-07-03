# [IAC_APPLY]

The terminal applier of the telemetry board plane: `telemetry/board` emits dashboards and alert specs as identity-derived data — `typeof DashboardModel.Encoded` through the model's own `Schema.encode`, `Alert.Spec` rows from the SLO fold — and one `Boards` tier realizes them as Grafana resources against the LGTM-rendered instance: one folder per app, one `oss.DataSource` row per backend URL, one `oss.Dashboard` per encoded model, alert rows per spec. The board is code and the UI is drift: `storeDashboardSha256: true` makes the provider diff dashboards by content hash — the deploy-plane echo of the kernel content-key discipline — so a hand-edited board surfaces in the `policy/drift` fold as an `update` step. The provider binds in-graph: `auth` is `admin` plus the same Doppler-generated password the LGTM chart seeded, so no token crosses an env round-trip and nothing enters state in cleartext beyond the provider's encrypted fields. The module is `iac/src/observe/apply.ts`; a new dashboard is one more encoded model in the args array, a new backend source is one `_SOURCES` row, and no dashboard JSON exists on disk anywhere.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                             | [PUBLIC] |
| :-----: | :----------------- | :------------------------------------------------------------------- | :------- |
|  [01]   | `PROVIDER_BINDING` | the one Grafana provider: auth, retry rows, content-hash drift      | `Boards` |
|  [02]   | `BOARD_APPLY`      | folder, data sources, dashboards, and alert rows from board outputs | `Boards` |

## [2]-[PROVIDER_BINDING]

[PROVIDER_BINDING]:
- Owner: the tier-constructed `grafana.Provider` — `url` from `Lgtm.urls.grafana`, `auth` as the `user:password` form woven from the Doppler read, `retries`/`retryWait` as the transient-fault posture (bridged providers retry HTTP, not semantics), and `storeDashboardSha256: true` so dashboard drift diffs by hash instead of full JSON.
- Law: one provider per stack — every resource in the tier threads `{ provider }` through `child()`; a second provider instance is the split-diamond defect.
- Law: auth never rides env here — the in-graph read is the canonical binding for deploy-time application; the env mode (`doppler run` + a runtime `Config.redacted`) belongs to operator tooling outside this tier.
- Growth: an org or cloud-lane axis is one `ProviderArgs` field fed from the spec when an app earns it.
- Boundary: the Grafana instance itself is `observe/stack.md`'s chart row; drift interpretation is `policy/drift.md`'s.
- Packages: `@pulumiverse/grafana` (`Provider`); `@pulumi/pulumi` (`interpolate`); `../observe/stack.ts` (`Lgtm`).

## [3]-[BOARD_APPLY]

[BOARD_APPLY]:
- Owner: the apply fold — one `oss.Folder` roots the app's boards (uid slugged from the spec's app key), `_SOURCES` maps backend rows (`prometheus`, `loki`, `tempo`) onto `oss.DataSource` constructions from the `Lgtm` URLs, each encoded `DashboardModel` becomes one `oss.Dashboard` under the folder, and each `Alert.Spec` row realizes through the alerting arm — rule group, contact point, notification policy — with `slo.Slo` rows when the spec carries an objective.
- Law: models arrive encoded — the tier consumes `typeof DashboardModel.Encoded` values and `pulumi.jsonStringify` is the only serialization; the model's uid is the resource name, so the board's identity derivation survives into the Grafana state and the drift receipt.
- Law: sources bind outputs — every `DataSource` url is an `Lgtm` `query`-plane projection `Output` (the read API, never an ingest path), so re-plumbing a backend re-points every board with zero board edits; a literal URL in a source row is the named defect.
- Law: the unverified argument surfaces are held at signature depth — `_boardArgs`, `_alertRows`, and `_sloRows` are declared signatures over the catalogued class names (`oss.Dashboard`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `slo.Slo`) whose exact field spellings settle when the grafana catalogue reaches operator depth on those argument records; the tier's shape, ordering, and identity law are settled now.
- Entry: `new Boards("boards", { spec, lgtm, auth, boards, alerts }, opts)` inside the k8s arm, `boards`/`alerts` produced by the app's `telemetry/board` suite call.
- Growth: a new panel family or board is upstream data — this tier changes only when Grafana grows a resource kind worth a row.
- Boundary: `DashboardModel`/`Alert` shapes are `telemetry`'s owners consumed as encoded values; folder placement conventions live here, board content never does.
- Packages: `@pulumiverse/grafana` (`oss.Folder`, `oss.DataSource`, `oss.Dashboard`, `alerting.RuleGroup`, `alerting.ContactPoint`, `alerting.NotificationPolicy`, `slo.Slo`); `@rasm/ts/telemetry` (`DashboardModel`, `Alert`); `effect` (`Array`); `../program/spec.ts` (`StackSpec`); `../stack/component.ts` (`Tier`).

```typescript
import * as grafana from "@pulumiverse/grafana"
import * as pulumi from "@pulumi/pulumi"
import type { Alert, DashboardModel } from "@rasm/ts/telemetry"
import { Array, Record } from "effect"
import type { StackSpec } from "../program/spec.ts"
import { Tier } from "../stack/component.ts"
import type { Lgtm } from "./stack.ts"

declare namespace Boards {
  type Model = typeof DashboardModel.Encoded
  type Args = {
    readonly spec: StackSpec
    readonly lgtm: Lgtm
    readonly auth: pulumi.Input<string>
    readonly boards: ReadonlyArray<Model>
    readonly alerts: ReadonlyArray<Alert.Spec>
  }
}

declare const _boardArgs: (model: Boards.Model, folder: pulumi.Output<string>) => grafana.oss.DashboardArgs
declare const _alertRows: (
  alerts: ReadonlyArray<Alert.Spec>,
  folder: pulumi.Output<string>,
  child: pulumi.CustomResourceOptions,
) => ReadonlyArray<pulumi.CustomResource>
declare const _sloRows: (
  alerts: ReadonlyArray<Alert.Spec>,
  child: pulumi.CustomResourceOptions,
) => ReadonlyArray<grafana.slo.Slo>

const _SOURCES = {
  prometheus: { type: "prometheus", url: (urls: Lgtm.Urls) => urls.query.prometheus },
  loki: { type: "loki", url: (urls: Lgtm.Urls) => urls.query.loki },
  tempo: { type: "tempo", url: (urls: Lgtm.Urls) => urls.query.tempo },
} as const

class Boards extends Tier {
  constructor(name: string, args: Boards.Args, opts?: pulumi.ComponentResourceOptions) {
    super("Boards", name, opts)
    const provider = new grafana.Provider(name, {
      url: args.lgtm.urls.grafana,
      auth: pulumi.interpolate`admin:${args.auth}`,
      retries: 3,
      storeDashboardSha256: true,
    }, { parent: this })
    const child = this.child({ provider })
    const folder = new grafana.oss.Folder(name, { title: args.spec.app, uid: args.spec.app }, child)
    Array.map(Record.toEntries(_SOURCES), ([key, row]) =>
      new grafana.oss.DataSource(key, { type: row.type, url: row.url(args.lgtm.urls) }, child))
    Array.map(args.boards, (model) => new grafana.oss.Dashboard(model.uid, _boardArgs(model, folder.uid), child))
    _alertRows(args.alerts, folder.uid, child)
    _sloRows(args.alerts, child)
    this.seal({ folder: folder.uid })
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Boards }
```
