# [RASM_COMPUTE]

`Rasm.Compute` is the measured-execution package with zero consumers; the implementation is full-capability with no holding back. The sub-domain-nested `.planning/` pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns typed intent admission, substrate selection, the CPU tensor lane, the numeric BLAS lane carrying dense and sparse linear algebra, the ONNX model lane with its generative token-streaming run, the remote lane carrying the suite wire vocabulary, the interchange lane carrying the field/result codec, the FastCDC geometry-delta codec, the two-hop tessellation bridge, the 3D-Tiles partition, and content-addressed artifact identity, the solver-and-optimization lane carrying discretization, the physics solve contract, and design-space search, staging memory, bounded work lanes, monotonic progress, the units boundary, and one twenty-one-case receipt union; it consumes AppHost ports and Persistence stores as settled vocabulary, composes the `Rasm.Bim` IFC/glTF/STEP semantic interchange at the companion seam, and never reverses the dependency. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                    | [OWNS]                                                                   |
| :-----: | :-------------------------------------------------------- | :---------------------------------------------------------------------- |
|   [1]   | [intent-and-selection](Intent/.planning/intent-and-selection.md)         | typed intent family; substrate-selection rail; total dispatch                 |
|   [2]   | [tensor-lane](Tensors/.planning/tensor-lane.md)                           | tensor shapes, dtype map, layout algebra, geometry encoding, op family, kernel dispatch |
|   [3]   | [model-lane](Models/.planning/model-lane.md)                             | ONNX identity, session capsule, EP rows, run modes, generative streaming, result cache |
|   [4]   | [numeric-lane](Numeric/.planning/numeric-lane.md)                         | RID-keyed BLAS provider table; dense factorization; sparse solve; kernel lowering |
|   [5]   | [remote-lane](Remote/.planning/remote-lane.md)                           | proto wire vocabulary; transports; channel capsule; credential axis; artifact frame law |
|   [6]   | [staging-and-streams](Staging/.planning/staging-and-streams.md)           | allocation classes; pooled memory; recyclable streams                         |
|   [7]   | [scheduling-and-lanes](Scheduling/.planning/scheduling-and-lanes.md)         | work-lane channels; solve-path guard; drain participation; job-graph scheduler |
|   [8]   | [progress-and-observation](Progress/.planning/progress-and-observation.md) | monotonic phases; zero-alloc capsules; observation seams                       |
|   [9]   | [units-boundary](Units/.planning/units-boundary.md)                     | quantity-family rows; conversion-at-admission; unit evidence                   |
|  [10]   | [receipts-and-benchmarks](Receipts/.planning/receipts-and-benchmarks.md)   | receipt union; fold projections; benchmark claims; wire stamps                 |
|  [11]   | [interchange](Interchange/.planning/interchange.md)                           | field/result codec; FastCDC geometry-delta; two-hop tessellation; 3D-Tiles partition; content-addressing |
|  [12]   | [solver-and-optimization](Solver/.planning/solver-and-optimization.md)   | volumetric mesher; physics solve contract; design-space optimizer; sweep; clash and twin |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in `Directory.Packages.props`; this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`, `app-root-pending`, `tests-only`. App-root server packages and direct references gated on transitive resolution carry `catalogue-pending` or `app-root-pending` and are catalogued when the gate resolves; the consuming-page row names where the surface lands.

| [INDEX] | [PACKAGE]                            | [PAGE]                                        | [CATALOGUE]                        | [STATUS]          |
| :-----: | :----------------------------------- | :-------------------------------------------- | :--------------------------------- | :---------------- |
|   [1]   | CommunityToolkit.HighPerformance     | tensor-lane, staging-and-streams              | api-highperformance.md             | admitted          |
|   [2]   | Google.Protobuf                      | remote-lane, receipts-and-benchmarks          | api-protobuf.md                    | admitted          |
|   [3]   | Grpc.Net.Client                      | remote-lane                                   | api-grpc-client.md                 | admitted          |
|   [4]   | Grpc.Net.Client.Web                  | remote-lane                                   | api-grpc-client-web.md             | admitted          |
|   [5]   | Grpc.Tools                           | remote-lane                                   | api-grpc-tools.md                  | admitted          |
|   [6]   | Microsoft.AspNetCore.TestHost        | remote-lane                                   | api-microsoftaspnetcoretesthost.md | admitted          |
|   [7]   | Microsoft.IO.RecyclableMemoryStream  | staging-and-streams, remote-lane              | api-recyclable-stream.md           | admitted          |
|   [8]   | Microsoft.ML.OnnxRuntime             | model-lane, tensor-lane, intent-and-selection | api-onnxruntime.md                 | admitted          |
|   [9]   | Microsoft.ML.OnnxRuntime.Extensions  | model-lane                                    | api-onnx-extensions.md             | admitted          |
|  [10]   | System.Numerics.Tensors              | tensor-lane                                   | api-tensors.md                     | admitted          |
|  [11]   | Thinktecture.Runtime.Extensions      | all pages                                     | doctrine (stack atlas)             | admitted          |
|  [12]   | Thinktecture.Runtime.Extensions.Json | receipts-and-benchmarks                       | doctrine (stack atlas)             | admitted          |
|  [13]   | UnitsNet                             | units-boundary                                | api-unitsnet.md                    | admitted          |
|  [14]   | SharpFuzz                            | remote-lane, tensor-lane                      | doctrine (testing-cs stack)        | admitted          |
|  [15]   | BenchmarkDotNet                      | model-lane                                    | doctrine (testing-cs stack)        | admitted          |
|  [16]   | MathNet.Numerics                     | numeric-lane                                  | api-mathnet-providers.md           | admitted          |
|  [17]   | CSparse                              | numeric-lane                                  | api-mathnet-providers.md           | admitted          |
|  [18]   | Microsoft.ML.OnnxRuntimeGenAI        | model-lane                                    | api-onnxruntimegenai.md            | admitted          |
|  [19]   | Microsoft.Extensions.AI.Abstractions | model-lane                                    | api-onnxruntimegenai.md            | admitted          |
|  [20]   | Microsoft.ML.OnnxRuntime.Gpu         | model-lane                                    | —                                  | app-root-pending  |
|  [21]   | Microsoft.ML.OnnxRuntime.DirectML    | model-lane                                    | —                                  | app-root-pending  |
|  [22]   | Grpc.AspNetCore trio                 | remote-lane                                   | —                                  | app-root-pending  |
|  [23]   | NodaTime.Serialization.Protobuf      | remote-lane, receipts-and-benchmarks          | api-nodatime-protobuf.md           | catalogue-pending |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                       | [RAIL]                   | [EVIDENCE]                                                            |
| :-----: | :--------------------------- | :----------------------- | :------------------------------------------------------------------- |
|  [G1]   | locked restore               | Assay restore rail       | lockfile unchanged; zero NU1004                                      |
|  [G2]   | API catalogue resolve        | `assay api` doctor/resolve | package keys resolve; catalogues current                          |
|  [G3]   | static plan + build          | Assay static rail        | routed closure compiles, zero `': error '` lines                    |
|  [G4]   | spec law-matrix              | Assay test rail (Compute target) | CsCheck laws hold without tolerance loosening               |
|  [G5]   | host-seam bridge scenarios   | Assay bridge rail        | plugin-ALC ONNX load and UDS attach pass under live RhinoWIP         |
|  [G6]   | spec-compile fallback        | Assay test rail          | `Grpc.Core.Api` members compile until assay source-map coverage lands |
|  [G7]   | page diagram render          | local mermaid-cli        | page diagrams render through the local renderer                      |
