# [TS_IAC_API_CLICKHOUSE]

`clickhouse` is the analytics residence the deploy plane installs as one Helm chart. This chart renders no workload itself — it emits a `ClickHouseInstallation` custom resource the Altinity operator reconciles, so chart values decide the CR and the OPERATOR decides every rendered object name. That split is the whole contract: a values key pins the installation, and its rendered Service name is the operator's own decoration over it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `clickhouse`
- chart: `clickhouse` from `https://helm.altinity.com` (Apache-2.0), bundling `altinity-clickhouse-operator` as an optional dependency
- asset: one `ClickHouseInstallation`, its operator-rendered Services, StatefulSets, ConfigMaps, and PVCs, beside an optional `ClickHouseKeeperInstallation`
- plane: `plane:deploy` — rendered by `@pulumi/kubernetes` `helm.v4.Chart`, depended on by nothing at runtime
- rail: deployment / analytics-residence

## [02]-[CHART_VALUES]

| [INDEX] | [KEY]                             | [CAPABILITY]                                                                              |
| :-----: | :-------------------------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `clickhouse.replicasCount`        | `int` — replicas per shard; above one demands Keeper                                      |
|  [02]   | `clickhouse.shardsCount`          | `int` — shard count; ignored once `zones` is set                                          |
|  [03]   | `clickhouse.persistence.*`        | `{ enabled, size, accessMode, … }` — the data claim per replica, and a separate log claim |
|  [04]   | `clickhouse.initScripts.*`        | `{ enabled, configMapName, … }` — the one branch-owned DDL carrier                        |
|  [05]   | `clickhouse.defaultUser.*`        | default-user credential and its network reach                                             |
|  [06]   | `clickhouse.users`                | additional users with per-user profiles                                                   |
|  [07]   | `clickhouse.image.*`              | `string` — server distribution and tag                                                    |
|  [08]   | `clickhouse.service.*`            | `{ type, annotations, labels }` — the CHI-level Service the operator renders              |
|  [09]   | `clickhouse.keeper.{host,port}`   | `string` / `int` — an external Keeper when the subchart is off                            |
|  [10]   | `clickhouse.zones`                | zone spread; overrides `replicasCount` placement                                          |
|  [11]   | `clickhouse.antiAffinity{,Scope}` | `boolean` / `string` — placement spread at CHI, shard, replica, or cluster                |
|  [12]   | `clickhouse.clusterSecret.*`      | secret-based inter-node communication                                                     |
|  [13]   | `keeper.*`                        | the bundled Keeper installation                                                           |
|  [14]   | `operator.enabled`                | `boolean` — off where the operator is already cluster-wide                                |
|  [15]   | `namespaceDomainPattern`          | `string` — a printf pattern for a custom cluster domain                                   |
|  [16]   | `nameOverride` `fullnameOverride` | `string` — pins the CR name, never the rendered Service name                              |

[clickhouse.initScripts]: `enabled` `configMapName` `alwaysRun` — the ConfigMap's keys mount as SQL files the server runs at start; `alwaysRun` re-runs them on every restart, which converges only for idempotent DDL
[clickhouse.defaultUser]: `password` `password_secret_name` (a Secret keyed `password`) `allowExternalAccess` `hostIP` — `allowExternalAccess` overrides `hostIP` to `0.0.0.0/0` outright
[clickhouse.persistence]: `enabled` `size` `accessMode` `storageClass` beside `logs.{enabled,size,accessMode}`
[keeper]: `enabled` `replicaCount` (odd, and fixed after first deployment) `image` `tag` `settings` `localStorage.{size,storageClass}` `zoneSpread` `resources.{cpuRequestsMs,memoryRequestsMiB,cpuLimitsMs,memoryLimitsMiB}`
[ports]: the server listens `8123` http, `9000` native, `9009` interserver — the exporter dials native and every query end dials http

[SERVICE_NAME]: the operator renders the CHI-level Service as `clickhouse-<chi>`, where `<chi>` is the installation name `fullnameOverride` pins, so a values-side name pin alone never yields the address; the decoration is configurable only through `.spec.templates.serviceTemplates[].generateName`, which this chart does not surface, so an endpoint projection carries the prefix explicitly.

## [03]-[QUERY_CONTRACT]

Grafana reads this residence through `grafana-clickhouse-datasource`, a third-party plugin the Foundation SDK never bundles. Its query record and its config record are the two wire shapes a board fence transcribes; both are plugin-owned TypeScript, not an npm dependency, so the plugin release is the pin and every field below reads off it.

[DATASOURCE_TYPE]: `grafana-clickhouse-datasource` — the `type` an `oss.DataSource` row carries and the `DataSourceRef.type` a panel binds.

[QUERY_RECORD]: `CHSqlQuery extends CHQueryBase extends DataQuery` — the raw-SQL arm of a two-arm union whose sibling `CHBuilderQuery` carries the visual editor's `builderOptions` payload.

| [INDEX] | [FIELD]         | [SHAPE]               | [CAPABILITY]                                                     |
| :-----: | :-------------- | :-------------------- | :--------------------------------------------------------------- |
|  [01]   | `refId`         | `string` REQUIRED     | the frame identity every Grafana dataquery carries               |
|  [02]   | `editorType`    | `EditorType` REQUIRED | `"sql"` selects this arm, `"builder"` the visual-editor sibling  |
|  [03]   | `rawSql`        | `string` REQUIRED     | the statement the driver executes verbatim                       |
|  [04]   | `pluginVersion` | `string` REQUIRED     | the release the record was written against                       |
|  [05]   | `format`        | `number`              | the sqlds `FormatQueryOption` ordinal shaping the returned frame |
|  [06]   | `queryType`     | `QueryType`           | explore-view display selection alone                             |
|  [07]   | `expand`        | `boolean`             | editor-side macro preview alone                                  |
|  [08]   | `meta`          | `{ timezone?, … }`    | editor round-trip state for a builder-to-SQL migration           |
|  [09]   | `hide`          | `boolean`             | the base record's per-target mute                                |
|  [10]   | `datasource`    | `DataSourceRef`       | the base record's per-target binding                             |

[FORMAT_LADDER]: `format` is the `sqlutil.FormatQueryOption` ordinal, not a string — `0` timeseries (long-to-wide pivot), `1` table, `2` logs, `3` traces, `4` multi; an unset value decodes as timeseries for backwards compatibility, and a value above the ladder refuses at the backend. Ordinals `2` and `3` set the frame's preferred visualization rather than reshaping it.
[EDITOR_ENUMS]: `EditorType = sql | builder`; `QueryType = table | logs | timeseries | traces`; `BuilderMode = list | aggregate | trend`.
[CONFIG_RECORD]: `CHConfig extends DataSourceJsonData` carries `host` `port` `protocol` `username` REQUIRED beside `version` `secure` `path` `defaultDatabase` `defaultTable` `tlsSkipVerify` `tlsAuth` `tlsAuthWithCACert` `dialTimeout` `queryTimeout` `connMaxLifetime` `maxIdleConns` `maxOpenConns` `validateSql` `logs` `traces` `aliasTables` `httpHeaders` `forwardGrafanaHeaders` `customSettings` `enableRowLimit` `rowCapacityHint` `enableMapKeysDiscovery` `hideTableNameInAdhocFilters` `configMode` `signalType`; `CHSecureConfig` carries `password` `tlsCACert` `tlsClientCert` `tlsClientKey`. `Protocol = native | http`.
[CONFIG_TRAPS]: the driver dials `host` and `port` and IGNORES the datasource `url` a provisioner sets, so an address alone provisions a door nothing connects through; `enableMapKeysDiscovery` defaults ON and issues `SELECT DISTINCT arrayJoin(col.keys) … LIMIT 1000` per `Map` column, which is a full-table probe on an OTel logs table; `rowCapacityHint` pre-allocates every frame at that width on EVERY query, so a value above the typical result wastes memory on all of them.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One residence per stack owns wide-event evidence under a branch-owned schema. Its writing exporter runs `create_schema: false`, so DDL lands before the first INSERT — this chart's init-script hook is the one place that ordering holds without minting a job the estate then owns and reconciles.

[STACKING]:
- `@pulumi/kubernetes`(`.api/pulumi-kubernetes.md`): `helm.v4.Chart` renders the CR and the operator's own objects as parented children; the DDL rides a `k8s.core.v1.ConfigMap` the tier mints and `initScripts.configMapName` names, and the credential rides the tier's existing `Secret` through `defaultUser.password_secret_name` rather than a values literal that renders into a ConfigMap.
- `opentelemetry-collector`(`.api/opentelemetry-collector.md`): the `clickhouse` exporter is the residence's one writer — its `logs_table_name` and `traces_table_name` address tables this chart's init scripts created, and its `ttl` and the DDL's `TTL` clause read one anchor so the two cannot disagree.
- `operate/observe#CHART_ROWS`: `_charts.clickhouse` supplies chart and repo, `Lgtm.Versions.clickhouse` the pin, `_RESIDENCE` the capability row, and `_ddl` the planted schema; `_urls` reads the row's `service` projection because the operator, not the chart, names what resolves.
- `operate/observe#BOARD_APPLY`: `_COLUMNAR.plugin` pins the type id, release, and editor discriminant; `_ResidenceQuery` transcribes `[QUERY_RECORD]` as a `cog.Builder<cog.Dataquery>`; the `_SOURCES` row answers `[CONFIG_RECORD]` through `jsonDataEncoded` and `secureJsonDataEncoded` precisely because `[CONFIG_TRAPS]` rules the datasource `url` inert for this driver.
- `@grafana/grafana-foundation-sdk`(`.api/grafana-grafana-foundation-sdk.md`): that pin ships no `./clickhouse` module at any dist-tag, and its `[TARGET_CONTRACT]` is what admits the branch-owned builder onto `withTarget` beside every bundled dataquery.

[LOCAL_ADMISSION]:
- Only the k8s arm admits these keys: `operate/observe#DEV_ROW` runs one all-in-one image with no residence at all, so the dev loop's analytics posture is absence, stated on the row rather than discovered on an empty query.
- Chart versions arrive as `Lgtm.Versions` args rather than a workspace-manifest row, because a Helm chart is a deploy-time reference and not a build dependency.

[RAIL_LAW]:
- Contract: `clickhouse` chart values + the `ClickHouseInstallation` the operator reconciles
- Owns: the columnar analytics residence — server placement, storage claims, credential custody, Keeper posture, and the init-script hook the branch DDL rides
- Accept: `operator.enabled` matching whether the operator is already cluster-wide; `initScripts` as the one DDL carrier with idempotent statements; `defaultUser.password_secret_name` for every credential; the explicit `clickhouse-` Service prefix in every endpoint projection
- Reject: a credential in a values literal; `allowExternalAccess`, which opens the default user to every source address; `create_schema: true` on the writing exporter, which replaces the owned sort key with an attribute map outside it; replicas above one without Keeper, which the CR refuses
