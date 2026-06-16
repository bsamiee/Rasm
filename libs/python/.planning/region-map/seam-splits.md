# [PYTHON_SEAM_SPLITS]

Every cross-owner fact records its altitude split: mechanics at the owning page, consequence at the consumer. A seam re-taught instead of consumed is a defect repaired by routing to the owner.

## [1]-[CROSS_BOUNDARY_SEAMS]

| [SEAM]                  | [MECHANICS_OWNER]                       | [CONSUMER_CONSEQUENCE]                                                   |
| :---------------------- | :-------------------------------------- | :---------------------------------------------------------------------- |
| companion gRPC wire     | `geometry/ifc-companion#DAEMON`         | C# `remote-lane#TRANSPORT_AXIS` reaches the companion over the EXISTING contract; no new wire |
| companion serve runtime | `runtime/server-host#SERVE`             | geometry `IfcCompanion` hosts its inbound serve through `ServerHost`; never the C# host lifecycle |
| content identity        | `runtime/content-identity#IDENTITY`     | data/geometry/artifacts key by `ContentIdentity` reproducing C# `InterchangeIdentity`; never re-minted |
| geometry graduation     | `compute/graduation#GRADUATION`         | geometry evidence reaches the C# owner system through the `HandoffAxis` geometry case |
| receipt contribution    | `runtime/observability#RECEIPT`         | data/compute/geometry/artifacts typed receipts wire through one `ReceiptContributor` port |
| resilience policy        | `runtime/rails-resilience#RESILIENCE`   | rails AND transport clusters consume one `Retry` owner via policy rows; never two |

## [2]-[BOUNDARY_SEAMS]

| [SEAM]                  | [MECHANICS_OWNER]                       | [CONSUMER_CONSEQUENCE]                                                   |
| :---------------------- | :-------------------------------------- | :---------------------------------------------------------------------- |
| host lifecycle          | `Rasm.AppHost`                          | runtime accepts caller-owned context and resource roots only            |
| product telemetry       | `Rasm.AppHost`                          | runtime emits Python-local receipt facts only                           |
| durable stores          | `Rasm.Persistence`                      | data emits portable import/export bundles                               |
| query rails             | `Rasm.Persistence`                      | data owns offline scan plans and query receipts                         |
| production compute      | `Rasm.Compute`                          | compute owns studies, assets, and handoff receipts                      |
| benchmark authority     | `Rasm.Compute`                          | compute emits research evidence only                                    |
| IFC semantic in-process | `Rasm.Compute` (GeometryGym)            | geometry owns only the tessellation hop the managed surface cannot do   |
| glTF in-process         | `Rasm.Compute` (SharpGLTF)              | geometry returns GLB the C# side reads; no Python glTF authoring        |
| live UI                 | `Rasm.AppUi` and TypeScript             | artifacts emits static files and visual specs                           |
| Rhino/GH mutation       | bridge and C# host owners               | data/geometry read or emit files only                                   |
| Assay command surface   | `tools/assay`                           | runtime `Entrypoint` is the companion's PRIVATE entry only; no new public commands |
| external API members    | package `.api` owner                    | a fence names a member only after evidence verifies its spelling        |

## [3]-[INTRA_BRANCH_SEAMS]

| [SEAM]                  | [MECHANICS_OWNER]                       | [CONSUMER_CONSEQUENCE]                                                   |
| :---------------------- | :-------------------------------------- | :---------------------------------------------------------------------- |
| labelled-array catalogue| `data/.api` (`xarray`, `dask`)          | compute composes the data catalogues as study-input bundle shapes; deletes its duplicate stubs |
| mesh-file exchange      | `data/graph-mesh#MESH`                  | geometry consumes mesh-file shapes as inputs; the IFC->GLB rail stays in geometry |
| remote AEC transport    | `runtime/resources-lanes#RESOURCE`      | data/geometry reach Speckle streams through the `TransportResource` row, not a data owner |
