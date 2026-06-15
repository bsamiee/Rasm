# [TYPESCRIPT_RUNTIME_HOST]

One page owns the co-hosted single-page-application root — one Layer graph, one runtime, the browser platform bindings, the self-telemetry edge, the build and PWA pipeline, and the browser decode-worker pool that moves heavy snapshot decode off the main thread. It assembles every other browser domain into one running surface and provides the five closed app-services once. The self-telemetry edge ships the host's own spans and metrics to the collector and crosses no wire contract. The page consumes the runtime feed (cluster 1) through state and authors no decode.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]                | [OWNS]                                                       |
| :-----: | :----------------------- | :----------------------------------------------------------- |
|   [1]   | COMPOSITION_AND_PLATFORM | the Layer graph, the runtime, and the browser platform layer |
|   [2]   | SELF_TELEMETRY           | the host instrumentation export edge                         |
|   [3]   | BUILD_AND_WORKER_POOL    | the build and PWA pipeline and the browser decode pool       |

## [2]-[COMPOSITION_AND_PLATFORM]

- Owner: `CompositionRoot`, the one Layer graph and one runtime, plus `BrowserPlatform`, the browser platform layer owning the HTTP client, the key-value store, and the worker pool.
- Cases: `CompositionRoot` assembles every browser domain into one Layer graph and one runtime; `BrowserPlatform` binds the platform services; configuration enters as one domain value at the root, never scattered flag reads.
- Entry: the five closed app-service owners — `WireClients`, `SnapshotFeed`, `RuntimeFeed`, `CommandGateway`, and `EvidenceFeed` named on `architecture-posture.md` — are provided once in this one Layer graph; a sixth sibling service is the named defect, a new state or gateway capability landing as a method or row on one of the five.
- Packages: the effect core, the platform and platform-browser layers, and the platform worker primitives.
- Growth: a new host service capability lands as a method on one of the five owners; a new platform binding lands as one platform-layer row.
- Boundary: the service-owner budget is closed at five; no integration path resolves into the C# tree, only the inventoried wire contracts.

## [3]-[SELF_TELEMETRY]

- Owner: `SelfTelemetry`, the host instrumentation export edge over the browser opentelemetry layer.
- Cases: `SelfTelemetry` ships the host's own spans and metrics to the collector through the OTLP web export; the collector is the only telemetry path; dashboards read the collector through the view-surfaces collector panel.
- Entry: instrumentation appears only as this host edge and the dashboards collector reader; the wire-contracts page references no telemetry type.
- Packages: the effect core and the browser opentelemetry layer.
- Growth: a new signal lands as one instrumented span or metric on the host edge.
- Boundary: the self-telemetry spans and metrics cross no wire contract — they ship from the host root to the collector, which is the only telemetry path; the observability stack that backs the collector is provisioned by the node-tier page.

## [4]-[BUILD_AND_WORKER_POOL]

- Owner: `BuildPipeline`, the build and PWA pipeline, plus `DecodeWorkerPool`, the browser decode-worker pool.
- Cases: `BuildPipeline` produces the co-hosted same-origin asset bundle with its plugin set, the styling pipeline, and the offline PWA worker; `DecodeWorkerPool` moves heavy snapshot decode and fold work onto a structured worker pool so the scoped-fiber guarantee holds without blocking the main thread.
- Entry: the worker pool is bound to the SPA root as a main-thread-offload concern, separate from the node-side durable cluster the node-tier page owns.
- Packages: the build toolchain and its plugin family, the styling pipeline, and the platform worker primitives.
- Growth: a new build concern lands as one plugin row; a new offload concern lands as one worker-pool task.
- Boundary: the browser worker pool is a main-thread-offload concern and never a durable tier; the durable cluster is the node-tier page's owner.
