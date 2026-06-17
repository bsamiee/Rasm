# [TYPESCRIPT_API_INFRA_DATA]

Deploy/provisioning resource surfaces (`@pulumi/pulumi`, `@pulumi/aws`, `@pulumi/azure-native`,
`@pulumi/gcp`), the Redis client (`ioredis`), the S3 object-store client (`@aws-sdk/client-s3`),
and the geo/map rendering libraries (`maplibre-gl`, `deck.gl`). These packages back the
`node-tier` RESOURCE_COMPONENTS_AND_LIFECYCLE and DURABLE_WORK_AND_RPC clusters, the durable
backplane, and the `view-surfaces` OBSERVATION_ROUTES GeoSeriesSurface.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@pulumi/pulumi`
- package: `@pulumi/pulumi`
- entry: `@pulumi/pulumi` (runtime), `@pulumi/pulumi/automation` (programmatic API)
- asset: resource model, output algebra, config, stack references, automation SDK
- rail: deployment

[PACKAGE_SURFACE]: `@pulumi/aws`
- package: `@pulumi/aws`
- entry: `@pulumi/aws`
- asset: AWS provider resource classes (s3, ecs, ec2, iam, rds, vpc, …)
- rail: deployment

[PACKAGE_SURFACE]: `@pulumi/azure-native`
- package: `@pulumi/azure-native`
- entry: `@pulumi/azure-native`
- asset: Azure provider resource classes (storage, compute, network, containerservice, …)
- rail: deployment

[PACKAGE_SURFACE]: `@pulumi/gcp`
- package: `@pulumi/gcp`
- entry: `@pulumi/gcp`
- asset: GCP provider resource classes (storage, compute, container, sql, …)
- rail: deployment

[PACKAGE_SURFACE]: `ioredis`
- package: `ioredis`
- entry: `ioredis` (default export `Redis`, named export `Cluster`)
- asset: Redis client, cluster client, pipeline, pub/sub
- rail: durable-backplane

[PACKAGE_SURFACE]: `@aws-sdk/client-s3`
- package: `@aws-sdk/client-s3`
- entry: `@aws-sdk/client-s3`
- asset: S3 command-style client, presigner companion `@aws-sdk/s3-request-presigner`
- rail: object-store

[PACKAGE_SURFACE]: `maplibre-gl`
- package: `maplibre-gl`
- entry: `maplibre-gl`
- asset: Map class, source and layer specification types, event types
- rail: geo-render

[PACKAGE_SURFACE]: `deck.gl` / `@deck.gl/core` / `@deck.gl/layers` / `@deck.gl/react`
- packages: `deck.gl` (umbrella), `@deck.gl/core`, `@deck.gl/layers`, `@deck.gl/react`
- entry: `@deck.gl/core` (Deck, Layer, CompositeLayer, View types), `@deck.gl/layers` (GeoJsonLayer, ScatterplotLayer, …), `@deck.gl/react` (DeckGL component)
- asset: layer system, view state, picking, react integration
- rail: geo-render

## [2]-[PUBLIC_TYPES]

### @pulumi/pulumi — core type family

[PUBLIC_TYPE_SCOPE]: output algebra
- rail: deployment

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                   |
| :-----: | :-------------------- | :---------------- | :--------------------------------------- |
|   [1]   | `Output<T>`           | async value monad | wraps resource-provisioned values        |
|   [2]   | `Input<T>`            | union alias       | `T \| Output<T>` — accepted by all props |
|   [3]   | `all<T>(outputs)`     | combinator        | `Output<T[]>` — join heterogeneous tuple |
|   [4]   | `interpolate`         | tagged template   | string `Output<string>` interpolation    |
|   [5]   | `output<T>(val)`      | lift              | wraps plain value into `Output<T>`       |
|   [6]   | `secret<T>(val)`      | lift              | wraps value as secret `Output<T>`        |
|   [7]   | `Output.apply<U>(fn)` | method            | `(fn: (T) => Input<U>) => Output<U>`     |
|   [8]   | `Output.get()`        | method            | unwrap to `T` (inside `apply` only)      |

[PUBLIC_TYPE_SCOPE]: resource and component family
- rail: deployment

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]     | [RAIL]                                  |
| :-----: | :------------------------- | :---------------- | :-------------------------------------- |
|   [1]   | `ComponentResource`        | abstract class    | logical grouping owner                  |
|   [2]   | `CustomResource`           | abstract class    | leaf provisioned resource               |
|   [3]   | `ResourceOptions`          | options interface | `parent`, `dependsOn`, `protect`, …     |
|   [4]   | `ComponentResourceOptions` | options interface | extends `ResourceOptions` + `providers` |
|   [5]   | `CustomResourceOptions`    | options interface | extends `ResourceOptions` + `import`    |
|   [6]   | `StackReference`           | class             | cross-stack output read                 |
|   [7]   | `Config`                   | class             | typed config and secret resolution      |
|   [8]   | `asset.FileAsset`          | class             | local file asset                        |
|   [9]   | `asset.StringAsset`        | class             | inline string asset                     |

```ts contract
// ComponentResource — canonical component base
class ComponentResource {
  constructor(
    type: string,
    name: string,
    args?: Inputs,
    opts?: ComponentResourceOptions
  );
  registerOutputs(outputs?: Inputs | Promise<Inputs> | Output<Inputs>): void;
}

// ResourceOptions — key option fields
interface ResourceOptions {
  parent?: Resource;
  dependsOn?: Input<Input<Resource>[]> | Input<Resource>;
  protect?: boolean;
  ignoreChanges?: string[];
  provider?: ProviderResource;
  deleteBeforeReplace?: boolean;
}

// Output<T> — core monad
interface Output<T> {
  apply<U>(func: (t: T) => Input<U>): Output<U>;
  get(): T;
}

// Config
class Config {
  constructor(name?: string);
  get(key: string, opts?: StringConfigOptions): string;
  requireSecret(key: string): Output<string>;
  getBoolean(key: string): boolean | undefined;
  requireObject<T>(key: string): T;
}

// StackReference
class StackReference extends CustomResource {
  constructor(name: string, args?: StackReferenceArgs, opts?: CustomResourceOptions);
  getOutput(name: Input<string>): Output<any>;
  requireOutput(name: Input<string>): Output<any>;
}
```

[PUBLIC_TYPE_SCOPE]: automation API family
- rail: deployment
- entry: `@pulumi/pulumi/automation`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                            |
| :-----: | :---------------------- | :-------------- | :-------------------------------- |
|   [1]   | `LocalWorkspace`        | workspace class | local Pulumi program execution    |
|   [2]   | `Stack`                 | stack class     | lifecycle: up, preview, destroy   |
|   [3]   | `LocalWorkspaceOptions` | options         | workDir, envVars, stackSettings   |
|   [4]   | `UpResult`              | result type     | summary + outputs map             |
|   [5]   | `PreviewResult`         | result type     | change summary                    |
|   [6]   | `DestroyResult`         | result type     | resource removal summary          |
|   [7]   | `OutputMap`             | type alias      | `Record<string, OutputValue>`     |
|   [8]   | `OutputValue`           | value type      | `{ value: any; secret: boolean }` |
|   [9]   | `UpOptions`             | options         | `onOutput`, `onEvent`, parallel   |

```ts contract
// automation entry — programmatic lifecycle driver
import { LocalWorkspace, Stack } from '@pulumi/pulumi/automation';

// create or select
const stack: Stack = await LocalWorkspace.createOrSelectStack({
  stackName: string;
  workDir: string;
  // or inline program:
  program?: PulumiFn;
}, opts?: LocalWorkspaceOptions);

// lifecycle operations
const upResult: UpResult       = await stack.up(opts?: UpOptions);
const previewResult: PreviewResult = await stack.preview(opts?: PreviewOptions);
const destroyResult: DestroyResult = await stack.destroy(opts?: DestroyOptions);
const outputs: OutputMap       = await stack.outputs();
await stack.setConfig(key: string, value: ConfigValue): Promise<void>;
await stack.cancel(): Promise<void>;
```

---

### @pulumi/aws — provider resource family (key surfaces)

[PUBLIC_TYPE_SCOPE]: AWS provider resource classes
- rail: deployment

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                 |
| :-----: | :------------------------ | :------------- | :--------------------- |
|   [1]   | `aws.s3.Bucket`           | resource class | S3 bucket provisioning |
|   [2]   | `aws.s3.BucketObject`     | resource class | object upload          |
|   [3]   | `aws.ecs.Cluster`         | resource class | ECS cluster            |
|   [4]   | `aws.ecs.TaskDefinition`  | resource class | ECS task               |
|   [5]   | `aws.ecs.Service`         | resource class | ECS service            |
|   [6]   | `aws.ec2.Vpc`             | resource class | VPC network            |
|   [7]   | `aws.ec2.Subnet`          | resource class | subnet                 |
|   [8]   | `aws.ec2.SecurityGroup`   | resource class | security group         |
|   [9]   | `aws.iam.Role`            | resource class | IAM role               |
|  [10]   | `aws.iam.RolePolicy`      | resource class | inline policy          |
|  [11]   | `aws.rds.Instance`        | resource class | RDS instance           |
|  [12]   | `aws.elasticache.Cluster` | resource class | ElastiCache cluster    |
|  [13]   | `aws.ecr.Repository`      | resource class | container image repo   |

```ts contract
// All provider resource constructors follow the pattern:
new aws.s3.Bucket(name: string, args: aws.s3.BucketArgs, opts?: pulumi.CustomResourceOptions);
// outputs are all Output<T>; inputs accept Input<T>
```

---

### @pulumi/azure-native and @pulumi/gcp — provider resource family (key surfaces)

[PUBLIC_TYPE_SCOPE]: Azure and GCP provider resource classes
- rail: deployment

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]  | [RAIL]                |
| :-----: | :--------------------------------------------- | :------------- | :-------------------- |
|   [1]   | `azure_native.storage.StorageAccount`          | resource class | Azure storage account |
|   [2]   | `azure_native.storage.BlobContainer`           | resource class | blob container        |
|   [3]   | `azure_native.containerservice.ManagedCluster` | resource class | AKS cluster           |
|   [4]   | `azure_native.compute.VirtualMachine`          | resource class | Azure VM              |
|   [5]   | `gcp.storage.Bucket`                           | resource class | GCS bucket            |
|   [6]   | `gcp.container.Cluster`                        | resource class | GKE cluster           |
|   [7]   | `gcp.compute.Instance`                         | resource class | Compute Engine VM     |
|   [8]   | `gcp.sql.DatabaseInstance`                     | resource class | Cloud SQL instance    |

---

### ioredis — client and cluster family

[PUBLIC_TYPE_SCOPE]: client family
- rail: durable-backplane

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                              |
| :-----: | :---------------- | :-------------- | :---------------------------------- |
|   [1]   | `Redis` (default) | client class    | single-node Redis connection        |
|   [2]   | `Cluster`         | client class    | Redis Cluster multi-node connection |
|   [3]   | `Pipeline`        | pipeline class  | queued command batch                |
|   [4]   | `RedisOptions`    | options type    | host, port, password, db, tls, …    |
|   [5]   | `ClusterOptions`  | options type    | nodes, redisOptions, scaleReads, …  |
|   [6]   | `ClusterNode`     | node descriptor | `{ host: string; port: number }`    |

```ts contract
import Redis, { Cluster } from 'ioredis';
import type { RedisOptions, ClusterOptions, ClusterNode } from 'ioredis';

// single-node
const redis = new Redis(port?: number, host?: string, options?: RedisOptions);
const redis2 = new Redis(options?: RedisOptions);

// cluster
const cluster = new Cluster(nodes: ClusterNode[], options?: ClusterOptions);

// pipeline — returns Pipeline (fluent, then exec)
const pipeline: Pipeline = redis.pipeline();
pipeline.set('key', 'val').get('key');
const results: [Error | null, unknown][] = await pipeline.exec();

// pub/sub — dedicated connection per channel direction
redis.subscribe(...channels: string[]): Promise<number>;
redis.on('message', (channel: string, message: string) => void);
redis.publish(channel: string, message: string): Promise<number>;

// key commands (representative — all standard Redis commands present)
redis.get(key: string): Promise<string | null>;
redis.set(key: string, value: string | Buffer, ...args): Promise<string>;
redis.del(...keys: string[]): Promise<number>;
redis.hset(key: string, field: string, value: string): Promise<number>;
redis.expire(key: string, seconds: number): Promise<number>;
redis.xadd(key: string, id: string, ...fieldValues: string[]): Promise<string>;
```

---

### @aws-sdk/client-s3 — command-style client family

[PUBLIC_TYPE_SCOPE]: client and command family
- rail: object-store

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :----------------------- | :------------ | :---------------------------- |
|   [1]   | `S3Client`               | client class  | connection/credential holder  |
|   [2]   | `S3ClientConfig`         | config type   | region, credentials, endpoint |
|   [3]   | `GetObjectCommand`       | command class | object download               |
|   [4]   | `PutObjectCommand`       | command class | object upload                 |
|   [5]   | `DeleteObjectCommand`    | command class | object deletion               |
|   [6]   | `ListObjectsV2Command`   | command class | key enumeration               |
|   [7]   | `CreateBucketCommand`    | command class | bucket creation               |
|   [8]   | `GetObjectCommandInput`  | input type    | `{ Bucket, Key, … }`          |
|   [9]   | `PutObjectCommandInput`  | input type    | `{ Bucket, Key, Body, … }`    |
|  [10]   | `GetObjectCommandOutput` | output type   | `{ Body: Readable, … }`       |
|  [11]   | `getSignedUrl`           | function      | presigned URL generation      |

```ts contract
import { S3Client, GetObjectCommand, PutObjectCommand } from '@aws-sdk/client-s3';
import type { S3ClientConfig, GetObjectCommandInput, PutObjectCommandInput } from '@aws-sdk/client-s3';
import { getSignedUrl } from '@aws-sdk/s3-request-presigner';

// client — one instance, reused across operations
const client = new S3Client({ region: string; credentials?: AwsCredentialIdentity; endpoint?: string });

// command pattern — tree-shaking compatible
const output = await client.send(new GetObjectCommand({ Bucket: string; Key: string }));
const body: Readable = output.Body as Readable;

await client.send(new PutObjectCommand({
  Bucket: string;
  Key: string;
  Body: string | Buffer | Readable;
  ContentType?: string;
  Metadata?: Record<string, string>;
}));

// presigned URL — companion package
const url: string = await getSignedUrl(
  client,
  new GetObjectCommand({ Bucket: string; Key: string }),
  { expiresIn: number }  // seconds
);
```

---

### maplibre-gl — Map and source/layer family

[PUBLIC_TYPE_SCOPE]: Map class and core types
- rail: geo-render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [RAIL]                                                        |
| :-----: | :--------------------- | :------------------ | :------------------------------------------------------------ |
|   [1]   | `Map`                  | class               | map instance, event emitter                                   |
|   [2]   | `MapOptions`           | options type        | container, style, center, zoom, …                             |
|   [3]   | `StyleSpecification`   | type                | full style document shape                                     |
|   [4]   | `LngLatLike`           | type union          | `[number, number] \| LngLat \| …`                             |
|   [5]   | `LngLatBoundsLike`     | type union          | bounding-box input shapes                                     |
|   [6]   | `GeoJSONSourceOptions` | type                | extends `GeoJSONSourceSpecification`; `data`, `workerOptions` |
|   [7]   | `SourceSpecification`  | discriminated union | all source type specs (`geojson`, `raster`, `vector`, …)      |
|   [8]   | `LayerSpecification`   | discriminated union | all layer type specs (`fill`, `line`, `circle`, `symbol`, …)  |
|   [9]   | `MapLayerEventType`    | event map           | `click`, `mouseenter`, `mouseleave`, …                        |
|  [10]   | `MapGeoJSONFeature`    | type                | picked feature with geometry                                  |
|  [11]   | `Marker`               | class               | DOM-overlay marker                                            |
|  [12]   | `Popup`                | class               | DOM-overlay popup                                             |

```ts contract
import maplibregl from 'maplibre-gl';
import type { MapOptions, StyleSpecification, LngLatLike, SourceSpecification, LayerSpecification } from 'maplibre-gl';

// Map constructor
const map = new maplibregl.Map({
  container: string | HTMLElement;
  style: StyleSpecification | string;
  center?: LngLatLike;      // default [0, 0]
  zoom?: number;            // default 0
  pitch?: number;
  bearing?: number;
  minZoom?: number;
  maxZoom?: number;
  pixelRatio?: number;      // defaults to devicePixelRatio
  antialias?: boolean;
  attributionControl?: boolean;
}: MapOptions);

// source and layer mutation — must run after 'load' event
map.on('load', () => {
  map.addSource(id: string, source: SourceSpecification): void;
  map.removeSource(id: string): this;
  map.addLayer(layer: LayerSpecification, beforeId?: string): void;
  map.removeLayer(id: string): this;
  map.getSource(id: string): Source | undefined;
});

// update helpers
map.setFilter(layerId: string, filter: FilterSpecification | null): void;
map.setPaintProperty(layerId: string, name: string, value: unknown): void;
map.setLayoutProperty(layerId: string, name: string, value: unknown): void;
map.setStyle(style: StyleSpecification | string | null): this;
map.flyTo(options: FlyToOptions): this;
map.fitBounds(bounds: LngLatBoundsLike, options?: FitBoundsOptions): this;

// GeoJSON source runtime data update
const src = map.getSource('id') as maplibregl.GeoJSONSource;
src.setData(data: GeoJSON.GeoJSON | string): void;
src.updateData(diff: { update: UpdateableFeature[] }): void;
```

---

### deck.gl — Layer and view family

[PUBLIC_TYPE_SCOPE]: Deck instance and core types
- rail: geo-render
- entry: `@deck.gl/core`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]   | [RAIL]                                                   |
| :-----: | :-------------------------- | :-------------- | :------------------------------------------------------- |
|   [1]   | `Deck<V>`                   | class           | headless deck instance                                   |
|   [2]   | `DeckProps<V>`              | props interface | `layers`, `views`, `viewState`, …                        |
|   [3]   | `Layer<DataT>`              | abstract class  | base layer; never instantiated                           |
|   [4]   | `CompositeLayer`            | abstract class  | layer that renders sub-layers                            |
|   [5]   | `LayerProps<DataT>`         | props interface | `id`, `data`, `visible`, `pickable`, `updateTriggers`, … |
|   [6]   | `LayersList`                | type alias      | `(Layer \| null \| undefined \| LayersList)[]`           |
|   [7]   | `MapView`                   | view class      | web-mercator map viewport                                |
|   [8]   | `OrthographicView`          | view class      | orthographic viewport                                    |
|   [9]   | `MapViewState`              | type            | `{ longitude, latitude, zoom, pitch, bearing }`          |
|  [10]   | `OrthographicViewState`     | type            | `{ target, zoom }`                                       |
|  [11]   | `PickingInfo<T>`            | type            | `{ object: T; coordinate; layer; … }`                    |
|  [12]   | `ViewStateChangeParameters` | type            | `{ viewState; interactionState; … }`                     |

[PUBLIC_TYPE_SCOPE]: built-in layer family
- rail: geo-render
- entry: `@deck.gl/layers`

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                                      |
| :-----: | :------------------------- | :------------ | :-------------------------------------------------------------------------- |
|   [1]   | `GeoJsonLayer<P>`          | layer class   | GeoJSON features (all geometry)                                             |
|   [2]   | `GeoJsonLayerProps<P>`     | props type    | `data`, `getFillColor`, `getLineColor`, `stroked`, `filled`, `pointType`, … |
|   [3]   | `ScatterplotLayer<D>`      | layer class   | point data as circles                                                       |
|   [4]   | `ScatterplotLayerProps<D>` | props type    | `data`, `getPosition`, `getRadius`, `getFillColor`, `stroked`, …            |
|   [5]   | `PathLayer<D>`             | layer class   | polyline paths                                                              |
|   [6]   | `PolygonLayer<D>`          | layer class   | polygon fills                                                               |
|   [7]   | `IconLayer<D>`             | layer class   | icon sprites                                                                |
|   [8]   | `TextLayer<D>`             | layer class   | text labels                                                                 |
|   [9]   | `HeatmapLayer<D>`          | layer class   | kernel density heatmap                                                      |

[PUBLIC_TYPE_SCOPE]: React integration family
- rail: geo-render
- entry: `@deck.gl/react`

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]   | [RAIL]                                       |
| :-----: | :--------------- | :-------------- | :------------------------------------------- |
|   [1]   | `DeckGL<V>`      | React component | deck.gl canvas + optional base map slot      |
|   [2]   | `DeckGLProps<V>` | props interface | extends `DeckProps`; `children` for base-map |

```ts contract
// @deck.gl/core — headless usage
import { Deck, MapView } from '@deck.gl/core';
import type { DeckProps, MapViewState, PickingInfo, LayersList } from '@deck.gl/core';

const deck = new Deck<MapView>({
  initialViewState?: MapViewState;
  viewState?: MapViewState;
  controller?: boolean | Controller;
  layers?: LayersList;
  views?: View | View[];
  onViewStateChange?: (params: ViewStateChangeParameters) => void;
  getTooltip?: (info: PickingInfo) => string | TooltipContent | null;
  canvas?: string | HTMLCanvasElement;
  width?: number | string;
  height?: number | string;
}: DeckProps<MapView>);

// @deck.gl/layers — layer constructors
import { GeoJsonLayer, ScatterplotLayer } from '@deck.gl/layers';
import type { GeoJsonLayerProps, ScatterplotLayerProps } from '@deck.gl/layers';
import type { Feature, Geometry } from 'geojson';

new GeoJsonLayer<PropertiesT>({
  id: string;
  data: string | Feature[] | GeoJSON;
  stroked?: boolean;
  filled?: boolean;
  pointType?: string;        // 'circle' | 'circle+text' | 'icon'
  pickable?: boolean;
  getFillColor?: Color | ((f: Feature<Geometry, PropertiesT>) => Color);
  getLineColor?: Color | ((f: Feature<Geometry, PropertiesT>) => Color);
  getLineWidth?: number | Accessor;
  getPointRadius?: number | Accessor;
  getText?: (f: Feature<Geometry, PropertiesT>) => string;
  updateTriggers?: Record<string, unknown>;
}: GeoJsonLayerProps<PropertiesT>);

new ScatterplotLayer<DataT>({
  id: string;
  data: string | DataT[];
  getPosition: (d: DataT) => Position;
  getRadius?: number | ((d: DataT) => number);
  getFillColor?: Color | ((d: DataT) => Color);
  getLineColor?: Color;
  getLineWidth?: number;
  stroked?: boolean;
  radiusScale?: number;
  radiusUnits?: 'meters' | 'pixels';
  pickable?: boolean;
  updateTriggers?: Record<string, unknown>;
}: ScatterplotLayerProps<DataT>);

// @deck.gl/react — React integration
import { DeckGL } from '@deck.gl/react';
// JSX: <DeckGL initialViewState={...} controller layers={layers} />
// Children are base-map slots (e.g. maplibre-gl Map element)
```

## [3]-[IMPLEMENTATION_LAW]

[DEPLOYMENT_TOPOLOGY]:
- `AutomationDriver` drives up/preview/destroy through `@pulumi/pulumi/automation` `LocalWorkspace` + `Stack`, not the bare CLI; node platform layer is the driver host.
- `ResourceComponent` classes extend `pulumi.ComponentResource`; every child resource sets `parent: this`; outputs registered via `registerOutputs`.
- `SecretResolver` reads typed config via `pulumi.Config.requireSecret`; config resolves at the composition root so config arrives as a domain value, never as scattered flag reads.
- `PolicyGuard` uses `@pulumi/policy` (`PolicyPack`, `ResourceValidationPolicy`) — admitted as a separate row from the core provider packages.
- Provider rows (`@pulumi/aws`, `@pulumi/azure-native`, `@pulumi/gcp`) are additive stack rows; one component class per stack, one provider row per new provider.

[REDIS_TOPOLOGY]:
- `RunnerBackplane` drives the durable cluster sharding over `Cluster` for multi-node production; single `Redis` instance for ephemeral test harness scenarios.
- Pub/sub lanes use dedicated connections (`sub` and `pub` are separate `Redis` instances — a client that has called `subscribe` enters subscriber mode and cannot issue other commands).
- Pipeline batches (`pipeline().cmd1().cmd2().exec()`) bound to a single node in cluster mode; all keys in a pipeline batch must hash to the same slot.
- Stream commands (`xadd`, `xreadgroup`) used for durable-work event sourcing within the cluster engine.

[S3_TOPOLOGY]:
- One `S3Client` instance per deployment scope, reused across operations (`CreateBucketCommand`, `PutObjectCommand`, `GetObjectCommand`, `ListObjectsV2Command`).
- Presigned URL generation via `getSignedUrl` (`@aws-sdk/s3-request-presigner`) does not require a separate client instance.
- `Body` from `GetObjectCommandOutput` is a `Readable` stream on Node; pipe or collect before the response completes.
- `@effect-aws/client-s3` is the workspace-admitted Effect wrapper over `@aws-sdk/client-s3`; prefer that wrapper for domain logic so the Effect rail is preserved end to end.

[GEO_RENDER_TOPOLOGY]:
- `GeoSeriesSurface` drives `maplibre-gl` `Map` as the base cartographic canvas; `deck.gl` layers overlay it through the reverse-controlled React pattern (`DeckGL` wraps the `maplibre-gl` `Map` element as a child).
- All `addSource` / `addLayer` calls gate on the map `load` event; `_checkLoaded()` throws if style is not ready.
- GeoJSON geometry decoded by `GeometryRail` (wire-contracts page) is delivered as `GeoJSON.GeoJSON` before being passed to `GeoJSONSourceOptions.data` or `GeoJsonLayer.data`; the geometry rail is the single decode owner.
- `updateTriggers` objects on `GeoJsonLayer` and `ScatterplotLayer` must list every external variable that accessor functions close over; failure to declare triggers silently skips re-evaluation on data changes.
- `DeckGL` and `Map` share the view state; `onViewStateChange` propagates deck.gl view state back to the maplibre camera to keep them synchronized.

[RAIL_LAW]:
- `@pulumi/pulumi` + providers: deploy and provisioning rail; no import resolves into this package set from browser-side code.
- `ioredis`: durable-backplane rail; node-tier only; never imported in browser bundles.
- `@aws-sdk/client-s3`: object-store rail; node-tier only; browser presigned-URL consumption uses the presigned URL string, not the SDK directly.
- `maplibre-gl` + `deck.gl`: geo-render rail; browser view-surfaces only; no node-tier import.
