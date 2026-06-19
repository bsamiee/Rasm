# [COMPUTE_TASKLOG]

The open and closed work for measured execution, distilled from `IDEAS.md`. Each open task carries a status marker and the capability-to-build, packages, integration points/boundaries, and key considerations; one idea spawns one or more tasks across one or more files. Closed cards record already-settled cleanup and the residual live-host probes whose owner shape is complete.

## [1]-[OPEN]

[NATIVE-BLAS]-[BLOCKED]: Activate the native-OpenBLAS/MKL dense and sparse execution row.
- The `Tensor/blas#DENSE_ALGEBRA`/#SPARSE_SOLVE `LinearProvider` RID axis is shape-complete and the managed terminal is proved end-to-end on osx-arm64; the native row activates on a win-x64 / linux-x64 host carrying the `MathNet.Numerics.MKL.Win-x64`/`.Linux-x64` or OpenBLAS native asset where `LinearProvider.Select` binds the native `ILinearAlgebraProvider` and the `Control.TryUseNativeOpenBLAS`/`TryUseNativeMKL` probe returns true.
- Blocked on a host RID that resolves the native asset; the managed fallback is the correct cold start until then.

[GPU-EP]-[BLOCKED]: Register and run the CUDA/DirectML GPU execution-provider rows.
- The `Model/providers#EP_AXIS` `AppendExecutionProvider_CUDA(0)`/`_DML(0)` member shapes are compiled; execution registers and runs on device on an NVIDIA-Linux/Windows RID + driver (CUDA) or Windows RID (DirectML), gated on the `Microsoft.ML.OnnxRuntime.Gpu`/`.DirectML` Windows-profile admission verdict.
- Blocked on a GPU host RID and the manifest admission verdict.

[LIVE-HOST-ONNX]-[BLOCKED]: Prove the in-host ONNX/GenAI native load under the live Rhino plugin ALC.
- The `Model/sessions#SESSION_CAPSULE`/#INFERENCE_MODES/#GENERATIVE_RUN owners are transcription-complete; the residual gates are the `libortextensions.dylib` versioned-RID resolution, the `RunOptions.Terminate` latch cadence, the ORT native dylib load inside the RhinoWIP plugin ALC, and a live multi-token generation against a genai-format model asset — each a live-host probe this osx-arm64 host structurally supplies only under the bridge scenario.
- Blocked on the live Rhino/GH2 host process and the genai-format model artifact.

[LIVE-HOST-UDS]-[BLOCKED]: Prove the Kestrel UDS server leg and GH2 solve-path ceiling under the live host.
- The `Runtime/channels#TRANSPORT_AXIS`/#CALL_POLICY UDS transport and the `Runtime/scheduling#SOLVE_GUARD` async-result guard are shape-complete; the residual gates are the Kestrel `ListenUnixSocket` server leg inside the Rhino plugin ALC, the `Grpc.Core.Api` transitive route proof, the live CoreML option-value domains, and the GH2 async readback ceiling under the live GH2 host.
- Blocked on the live Rhino-plugin app root + UDS attach scenario and the live GH2 host.

## [2]-[CLOSED]

(none)
