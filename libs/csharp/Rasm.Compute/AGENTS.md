# [H1][RASM_COMPUTE_AGENTS]
>**Dictum:** *Measure before promoting compute surface.*

<br>

[CRITICAL] `Rasm.Compute` is docs-only until a future implementation explicitly creates a project and production source. Do not add package references, `.csproj`, or C# files from this folder contract alone.

---
## [1][OWNER_CONTRACT]
>**Dictum:** *Compute owns execution evidence, not domain meaning.*

<br>

- Start from existing `Rasm` + MathNet + CSparse algorithms.
- Use System.Numerics.Tensors only for measured span/TensorPrimitives kernels.
- Use ML.NET only for named-model lifecycle and inference proof.
- Use gRPC client only for out-of-process companion compute.
- Keep substrate choice internal to Compute operations.
- Emit typed receipts with substrate, timing, allocation, cancellation, and equivalence fields.

---
## [2][BOUNDARY_RULES]
>**Dictum:** *Scheduling is not execution semantics.*

<br>

| [INDEX] | [BOUNDARY] | [RULE] |
| :-----: | ---------- | ------ |
|   [1]   | `Rasm` | Owns domain result types and existing numeric algorithms |
|   [2]   | `Rasm.AppHost` | May schedule/drain compute work |
|   [3]   | `Rasm.Compute` | Owns substrate selection and benchmark/model/remote receipts |
|   [4]   | Remote service | Exists only through companion contract |
|   [5]   | Benchmarks | Required for speed/allocation claims |

---
## [3][EVIDENCE]
>**Dictum:** *Docs name evidence categories; source slices produce evidence.*

<br>

Documentation edits require manual consistency only. Executable proof begins when source, package references, benchmarks, or runtime scenarios land.

Future Compute evidence categories:

- Output equivalence against baseline.
- Benchmark timing and allocation profile.
- Cancellation, timeout, and failure receipts.
- Tensor shape/dtype/layout/copy policy when tensors exist.
- Model identity/version/load/dispose/inference policy when models exist.
- gRPC deadline/payload/retry/failure receipts when remote compute exists.

---
## [4][REJECTIONS]
>**Dictum:** *No deep-learning lane exists by default.*

<br>

- No acceleration claim without benchmark evidence.
- No tensor package wrapper.
- No model lane without named model.
- No gRPC server or client rail without companion contract.
- No generic job framework inside Compute.
