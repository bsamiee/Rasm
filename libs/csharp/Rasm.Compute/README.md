# [RASM_COMPUTE]

`Rasm.Compute` is the measured-execution package: typed intent admission, substrate selection, the CPU tensor lane, the ONNX model lane, the remote lane carrying the suite wire vocabulary, staging memory, bounded work lanes, monotonic progress, the units boundary, and one thirteen-case receipt union.

## [1]-[SCOPE_AND_INTENT]

The package has zero consumers; implementation lands full-capability with no holding back. Production source transcribes the nine finalized `.planning/` pages — decision-complete blueprints that are never re-designed downstream. Every execution concern enters as a row on an owned axis (`Substrate`, `ExecutionProvider`, `RemoteTransport`, `AllocationClass`, `WorkLane`, `ProgressPhase`, `QuantityFamily`, `ComputeReceipt`); a new capability is a row or case, never a new surface.

`Rasm.Compute` is not a tensor wrapper, ONNX wrapper, gRPC wrapper, training pipeline, job framework, process-queue owner, or UI scheduler. Algorithms stay in `Rasm`; runtime policy stays in `Rasm.AppHost`; durable storage stays in `Rasm.Persistence`; presentation scheduling stays in `Rasm.AppUi`.

## [2]-[ROUTING]

| [INDEX] | [OPEN]                                                     | [READ_FOR]                                                    |
| :-----: | ---------------------------------------------------------- | ------------------------------------------------------------- |
|   [1]   | [ARCHITECTURE.md](ARCHITECTURE.md)                         | Rails, axes, cross-package seams, package-API map             |
|   [2]   | [ROADMAP.md](ROADMAP.md)                                   | Start gates, research probes, build-order-sequenced tasks     |
|   [3]   | [FEATURES.md](FEATURES.md)                                 | Capability atlas keyed to planning-page anchors               |
|   [4]   | [Implementation charter](.planning/README.md)              | Page index, density bar, build order, proof gates, admissions |
|   [5]   | [Package API catalogues](.reports/api/README.md)           | External package API facts per lane                           |
|   [6]   | [C# stack doctrine](../../../docs/stacks/csharp/README.md) | Language, shapes, rails, surfaces, and boundary law           |
|   [7]   | [Suite planning standard](../.planning/README.md)          | Page grammar and machine checks governing `.planning/`        |
|   [8]   | [Suite region map](../.planning/_region-map.md)            | Suite symbol ledger and seam splits                           |
