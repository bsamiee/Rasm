# [H1][RASM_COMPUTE_ROADMAP]
>**Dictum:** *First compute slice proves equivalence and measurement.*

<br>

`Rasm.Compute` remains documentation-only. This roadmap sequences future implementation; it does not authorize package pins or source creation by itself.

---
## [1][READINESS]
>**Dictum:** *Compute starts when direct execution needs measured orchestration.*

<br>

Start implementation only when a concrete plugin/app has a long-running or measured compute operation that existing `Rasm` APIs cannot manage alone.

Required decisions before source:

- Baseline `Rasm`/MathNet/CSparse operation.
- Input class, tolerance policy, and output equivalence rule.
- Cancellation/deadline/progress requirement.
- Benchmark target and memory evidence category.
- Candidate package versions refreshed from latest stable source.

---
## [2][FIRST_SLICE]
>**Dictum:** *A cancellable measured envelope comes before tensors, models, and remotes.*

<br>

First slice: one cancellable compute operation around an existing `Rasm`/MathNet/CSparse operation with equivalence and baseline timing receipts.

| [INDEX] | [LANDS] | [DEFERRED] |
| :-----: | ------- | ---------- |
|   [1]   | Compute intent | Multi-substrate selector |
|   [2]   | Cancellation/deadline receipt | Progress UI integration |
|   [3]   | Baseline timing receipt | TensorPrimitives candidate |
|   [4]   | Output equivalence receipt | ML.NET model lane |
|   [5]   | Failure taxonomy | gRPC companion lane |

---
## [3][DONE_WHEN]
>**Dictum:** *A compute slice is done when it can disprove itself.*

<br>

The first slice is complete when receipts identify substrate, input class, output equivalence, timing, allocation category, cancellation, timeout, and failure path. Runtime/performance claims remain scoped to the measured input class.

---
## [4][DEFERRED_UNTIL]
>**Dictum:** *Advanced compute waits for measured pressure.*

<br>

| [INDEX] | [DEFERRED] | [UNBLOCKS_WHEN] |
| :-----: | ---------- | --------------- |
|   [1]   | System.Numerics.Tensors | Baseline span kernel needs measured improvement |
|   [2]   | ML.NET | Named model and lifecycle policy exist |
|   [3]   | gRPC client | Companion compute service contract exists |
|   [4]   | Remote retry policy | AppHost outbound-hop ownership exists |
|   [5]   | Model/native asset receipts | Model or package actually enters graph |
