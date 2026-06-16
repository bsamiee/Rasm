# [COMPUTE_TASKLOG]

Open work owned by this folder; closed items do not appear. The residual SPIKE owners carry their named closure environment — each is a transcription-complete fence whose only open gate is the live host, native asset, or GPU hardware named below.

## [1]-[RESIDUAL_TIER3_PROBES]

Genuine tier-3 residuals: the owner member shape is FINALIZED and proved on the reachable half; the open gate is a host, native asset, or hardware this single-RID osx-arm64 host structurally cannot supply.

| [INDEX] | [ITEM] | [CLOSURE_ENVIRONMENT] |
| :-----: | ------ | --------------------- |
| [1] | numeric-lane native-MKL / native-OpenBLAS dense GEMM + CSparse native sparse EXECUTION (`LinearProvider.native-mkl` / `native-openblas` rows; numeric-lane#DENSE_ALGEBRA + #SPARSE_SOLVE) — RID axis FINALIZED, managed terminal proved end-to-end on osx-arm64; native execution is a per-RID deploy-asset gate, not an open owner spike: the `Control.TryUse*` boolean fall-through returns false on this RID because no osx-arm64 native asset ships | win-x64 / linux-x64 host carrying the `MathNet.Numerics.MKL.Win-x64` / `.Linux-x64` (or OpenBLAS) native asset: `LinearProvider.Select` activates the native row and the dense/sparse solve runs through the native `ILinearAlgebraProvider` |
| [2] | model-lane `Cuda` / `DirectMl` GPU-EP EXECUTION (`AppendExecutionProvider_CUDA(0)` / `_DML(0)`; model-lane#RESEARCH [EP_EXECUTION]) — registration member shape FINALIZED and compiled; CoreML EP proved tier-2 on this arm64 mac | NVIDIA-Linux / Windows RID + driver (CUDA) or Windows RID (DirectML): the GPU EP registers and inference executes on device |
| [3] | model-lane `ModelSessions` / `RunOps` in-host ONNX dylib native load inside the RhinoWIP plugin ALC; `libortextensions.dylib` versioned-RID resolution; `RunOptions.Terminate` latch propagation latency + deadline-poll cadence on the CoreML and CPU rows (model-lane#RESEARCH [CANCELLATION]) | live Rhino/GH2 host process: dylib loads without ALC isolation failure, RID resolution confirmed, latch cadence measured under the plugin ALC (charter `bridge` gate, ROADMAP START_GATES [1]-[2]) |
| [4] | remote-lane `WireChannels` / `CallSpine` Kestrel `ListenUnixSocket` server leg inside the Rhino plugin ALC; `Grpc.Core.Api` transitive route proof; CoreML option value domains (MLComputeUnits, SpecializationStrategy) grounded against the live CoreML surface | live Rhino-plugin app root + UDS attach scenario (charter `bridge` + `spec-rail` gates, ROADMAP START_GATES [3]-[4]) |
| [5] | GH2 async result readback ceiling (solver idle-loop) | live GH2 host: readback ceiling measured, idle-loop behaviour documented |

## [2]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Zero-alloc stream-pool eleven-event fold (Check.Faster); drop-callback allocation profile | allocation profile clean; no alloc on the hot fold path |
| [2] | Progress no-regress race (CsCheck SampleParallel over concurrent Advance) | no regress observed over parallel Advance calls |
| [3] | ComputeWireContext + suite Strict resolver-merge round-trip over Thinktecture key scalars (15-case union incl. Factorization/Generate) | round-trip passes; key scalars and the two new cases survive merge without loss |
| [4] | UnitsNet next-major QuantityInfo reshape staged-restore check | staged-restore passes against next-major QuantityInfo shape |

## [3]-[ADMISSION_DECISIONS_PENDING]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Microsoft.AspNetCore.TestHost (InProcess transport row helper) | admitted at matched ASP.NET Core servicing line as test-only PackageReference |
| [2] | Microsoft.ML.OnnxRuntime.Gpu / .DirectML (Cuda/DirectMl EP registration members) | Windows-profile verdict recorded; designed-only rows on model-lane RESEARCH [EP_EXECUTION] confirmed or promoted |

## [4]-[DEEPENING_FINDINGS]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Optional NodaTime.Serialization.Protobuf direct admission gated on whether Google.Api.CommonProtos calendar surface resolves transitively through the admitted NodaTime + Grpc graph (receipts-and-benchmarks RESEARCH [CALENDAR_BRIDGE]; Compute GAP_LEDGER [20] + CATALOGUE_PENDING) | transitive resolution confirmed or denied; admission verdict recorded |
| [2] | `MeshKernel`/`SolveLane` element assembly kernels (Poisson/elasticity/Helmholtz shape-function + quadrature) ground against the `Rasm`/Vectors core kernel + `MeshSpace` boundary-extraction surface (solver-and-optimization RESEARCH [ASSEMBLY_KERNELS]); the discretization fold and CSR handoff to numeric-lane are transcription-complete | shape-function member spellings for tet10/hex20 + boundary-extraction member confirmed at cross-folder alignment |
| [3] | `Surrogate.Fit` GP-covariance Cholesky + POD/SVD reduced-basis projection ground against the live MathNet SVD/factorization surface (solver-and-optimization RESEARCH [SURROGATE_FIT]); the duality contract and error-bound gate are transcription-complete | reduced-basis projection + GP marginalization member shapes confirmed against MathNet |
| [4] | `FieldCodec` chunked-field decode + Zarr/VTK-class layout ground against the admitted field-format library surface (interchange RESEARCH); the chunk policy, error-bound gate, and zero-copy handoff are transcription-complete | field-format decode member spellings confirmed at the field-codec admission gate |
| [5] | `SensitivityLaw` DDG-operator VJP bodies (cotangent-Laplacian transpose, heat-flow backward-Euler adjoint, spectral-mode sensitivity) ground against the `Rasm`/Vectors operator kernel (tensor-lane RESEARCH [DDG_ADJOINT]); the elementwise/matmul VJP table and tape chain are transcription-complete | DDG-operator adjoint-coefficient member spellings confirmed at cross-folder alignment |
| [6] | AP242/native/IFC5/Draco/Meshopt/3D-Tiles codec member spellings ground against the STEP, native-companion, GeometryGym IFC5, and SharpGLTF extension surfaces (interchange RESEARCH [AP242_CODEC], [NATIVE_FORMAT_BRIDGES], [IFC5_ECS], [MESH_COMPRESSION]); rows, codecs, capability/frame columns transcription-complete, native legs land at the Python-companion cross-branch touchpoint | codec/encode member spellings confirmed; native-companion legs sequenced at cross-branch alignment |
