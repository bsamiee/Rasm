# [COMPUTE_TASKLOG]

Open work owned by this folder; closed items do not appear.

## [1]-[NATIVE_AND_SERVER_PROBES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | CoreML option value domains (MLComputeUnits, SpecializationStrategy); Terminate-latch latency cadence | value domains grounded against CoreML surface; latch cadence measured |
| [2] | GH2 async result readback ceiling (solver idle-loop) | readback ceiling measured; idle-loop behaviour documented |
| [3] | Native-BLAS asset presence per RID for `LinearProvider` rows (native-mkl x64-only documented gate; native-openblas osx-arm64 asset checked empirically; managed fallback proven on arm64) | per-RID `Control.TryUse*` boolean fall-through confirmed against the restored native asset graph; managed terminal proven |
| [4] | ORT-GenAI live multi-token generation against a genai-format model directory (streaming contract runtime-proved; model artifact is the residual gate) | `GenerativeRun.Stream` yields incremental pieces against a real genai-format model; the member shape is FINALIZED |

## [2]-[SPEC_PROOFS_AT_IMPLEMENTATION]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Zero-alloc stream-pool eleven-event fold (Check.Faster); drop-callback allocation profile | allocation profile clean; no alloc on the hot fold path |
| [2] | Progress no-regress race (CsCheck SampleParallel over concurrent Advance) | no regress observed over parallel Advance calls |
| [3] | ComputeWireContext + suite Strict resolver-merge round-trip over Thinktecture key scalars | round-trip passes; key scalars survive merge without loss |
| [4] | UnitsNet next-major QuantityInfo reshape staged-restore check | staged-restore passes against next-major QuantityInfo shape |

## [3]-[ADMISSION_DECISIONS_PENDING]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Microsoft.AspNetCore.TestHost (InProcess transport row helper) | admitted at matched ASP.NET Core servicing line as test-only PackageReference |
| [2] | Microsoft.ML.OnnxRuntime.Gpu / .DirectML (Cuda/DirectMl EP registration members) | Windows-profile verdict recorded; designed-only rows on model-lane RESEARCH [EP_OPTIONS] confirmed or promoted |

## [4]-[PLANNING_CLOSE_OUT_SPIKES]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | ONNX dylib load inside the RhinoWIP ALC; libortextensions versioned-RID resolution | dylib loads without ALC isolation failure; RID resolution confirmed |

## [5]-[DEEPENING_FINDINGS]

| [INDEX] | [ITEM] | [EXIT] |
| :-----: | ------ | ------ |
| [1] | Optional NodaTime.Serialization.Protobuf direct admission gated on whether Google.Api.CommonProtos calendar surface resolves transitively through the admitted NodaTime + Grpc graph (receipts-and-benchmarks RESEARCH [CALENDAR_BRIDGE]; Compute GAP_LEDGER [20] + CATALOGUE_PENDING) | transitive resolution confirmed or denied; admission verdict recorded |
