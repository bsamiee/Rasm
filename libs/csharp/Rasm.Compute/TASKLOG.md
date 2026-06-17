# [COMPUTE_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[RESIDUAL_TIER3_PROBES]

Tier-3 residuals: the owner member shape is FINALIZED and proved on the reachable half; the open gate is a host, native asset, or hardware this single-RID osx-arm64 host structurally cannot supply. Each is named in its page RESEARCH cluster.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | numeric-lane native-MKL / native-OpenBLAS dense GEMM + CSparse native sparse execution; the RID axis is FINALIZED and the managed terminal proved end-to-end on osx-arm64, the native row activates on a win-x64 / linux-x64 host carrying the `MathNet.Numerics.MKL.Win-x64` / `.Linux-x64` or OpenBLAS native asset where `LinearProvider.Select` activates the native `ILinearAlgebraProvider` | numeric-lane#DENSE_ALGEBRA · numeric-lane#SPARSE_SOLVE | SPIKE |
| [2] | model-lane `Cuda` / `DirectMl` GPU-EP execution; `AppendExecutionProvider_CUDA(0)` / `_DML(0)` member shape FINALIZED and compiled, execution registers and runs on device on an NVIDIA-Linux / Windows RID + driver (CUDA) or Windows RID (DirectML) | model-lane#EP_AXIS | SPIKE |
| [3] | model-lane in-host ONNX dylib native load inside the RhinoWIP plugin ALC; `libortextensions.dylib` versioned-RID resolution; `RunOptions.Terminate` latch propagation latency + deadline-poll cadence on the CoreML and CPU rows under the live Rhino/GH2 host process | model-lane#SESSION_CAPSULE · model-lane#INFERENCE_MODES | SPIKE |
| [4] | remote-lane Kestrel `ListenUnixSocket` server leg inside the Rhino plugin ALC; `Grpc.Core.Api` transitive route proof; CoreML option value domains grounded against the live CoreML surface under the live Rhino-plugin app root + UDS attach scenario | remote-lane#TRANSPORT_AXIS · remote-lane#CALL_POLICY | SPIKE |
| [5] | GH2 async result readback ceiling and solver idle-loop behaviour under the live GH2 host | scheduling-and-lanes#SOLVE_GUARD | SPIKE |
| [6] | ONNX Runtime native dylib load inside the RhinoWIP plugin ALC, the bridge-gated start probe unblocking the session capsule | model-lane#SESSION_CAPSULE | SPIKE |
| [7] | `libortextensions.dylib` resolution under the portable RID graph, the extension-op start probe with a bin-output native-asset listing | model-lane#EXTENSION_OPS | SPIKE |
| [8] | Native-BLAS asset presence per RID — `Control.TryUseNativeMKL` / `TryUseNativeOpenBLAS` boolean probe over the restored native asset graph, managed fallback proven on osx-arm64 | numeric-lane#DENSE_ALGEBRA · numeric-lane#SPARSE_SOLVE | SPIKE |
| [9] | ORT-GenAI live multi-token generation against a genai-format model asset; the token-streaming member shape is FINALIZED, the model artifact is the residual gate for `GenerativeRun.Stream` | model-lane#GENERATIVE_RUN | SPIKE |

## [2]-[IMPLEMENTATION_PROBES]

Implementation-time probes discharged against scratch processes, the spec project, MathNet/Vectors cross-folder alignment, or a manifest decision — no live host required.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `Grpc.Core.Api` `Metadata` carrier members compile through the spec-compile fallback gate until the assay source map registers the transitive package | remote-lane#FAULT_PROJECTION | SPIKE |
| [2] | Zero-alloc stream-pool eleven-event fold (`Check.Faster`) and the drop-callback allocation profile resolve with no alloc on the hot fold path | staging-and-streams#STREAM_POOL | SPIKE |
| [3] | Progress no-regress race over concurrent `Advance` (CsCheck `SampleParallel`) holds with no observed rank regress | progress-and-observation#PHASE_FAMILY | SPIKE |
| [4] | `ComputeWireContext` + suite Strict resolver-merge round-trip over Thinktecture key scalars, the 21-case union including Factorization/Generate, survives merge without loss | receipts-and-benchmarks#WIRE_STAMPS | SPIKE |
| [5] | UnitsNet next-major `QuantityInfo` reshape staged-restore check passes against the next-major shape | units-boundary#QUANTITY_TABLE | SPIKE |
| [6] | `MeshKernel` / `SolveLane` element-assembly kernels (Poisson/elasticity/Helmholtz shape-function + quadrature) ground against the `Rasm`/Vectors core kernel + `MeshSpace` boundary-extraction surface; tet10/hex20 shape-function and boundary-extraction member spellings confirmed at cross-folder alignment | solver-and-optimization#SOLVE_CONTRACT · solver-and-optimization#DISCRETIZATION_MESH | SPIKE |
| [7] | `Surrogate.Fit` GP-covariance Cholesky + POD/SVD reduced-basis projection ground against the live MathNet SVD/factorization surface; reduced-basis projection + GP marginalization member shapes confirmed | solver-and-optimization#OPTIMIZER_LANE | SPIKE |
| [8] | `FieldCodec` chunked-field decode + Zarr/VTK-class layout ground against the admitted field-format library surface; chunk policy, error-bound gate, and zero-copy handoff confirmed at the field-codec admission gate | interchange#FIELD_RESULT_CODEC | SPIKE |
| [9] | `SensitivityLaw` DDG-operator VJP bodies (cotangent-Laplacian transpose, heat-flow backward-Euler adjoint, spectral-mode sensitivity) ground against the `Rasm`/Vectors operator kernel; adjoint-coefficient member spellings confirmed at cross-folder alignment | tensor-lane#EQUIVALENCE_INTEROP | SPIKE |
| [10] | `TilePartition` leaf-tile content emit grounds against the `Rasm.Bim` b3dm/glTF tile-emit codec; the `TileSet` octree partition, per-node content-key, and quantization-bit policy are transcription-complete, the leaf emit body grounds at cross-package alignment | interchange#TILE_PARTITION | SPIKE |
| [11] | NodaTime converter precedence over combined source-gen metadata in the Strict merge | receipts-and-benchmarks#WIRE_STAMPS | SPIKE |

## [3]-[ADMISSION_DECISIONS]

Manifest admission verdicts pending; each lands a package row at its matched servicing line or confirms a designed-only EP execution row.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `Microsoft.AspNetCore.TestHost` admitted at the matched ASP.NET Core servicing line as a test-only `PackageReference` for the InProcess transport row helper | remote-lane#TRANSPORT_AXIS | BLOCKED |
| [2] | `Microsoft.ML.OnnxRuntime.Gpu` / `.DirectML` Windows-profile verdict for the Cuda/DirectMl EP registration members; designed-only EP execution rows confirmed or promoted | model-lane#EP_AXIS | BLOCKED |
| [3] | `NodaTime.Serialization.Protobuf` direct admission gated on whether `Google.Api.CommonProtos` calendar surface resolves transitively through the admitted NodaTime + Grpc graph; admission verdict recorded | receipts-and-benchmarks#WIRE_STAMPS · remote-lane#PROTO_VOCABULARY | BLOCKED |

## [4]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (vocabulary owners before consumers, `Faults.cs` through `Solver/Lane.cs`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the build-order files per `ARCHITECTURE.md` `[SOURCE_TREE]`; the test project `Rasm.Compute.Tests` node is present and empty | intent-and-selection#DISPATCH_SPINE | QUEUED |
