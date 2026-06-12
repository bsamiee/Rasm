# [RASM_COMPUTE_API_TENSORS]

`System.Numerics.Tensors` supplies tensor shapes, tensor spans, tensor
interfaces, vectorized primitives, marshalling, and numeric operation families
for measured Compute execution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors`
- assembly: `System.Numerics.Tensors`
- namespace: `System.Numerics.Tensors`
- asset: runtime library
- rail: tensor

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tensor shapes
- rail: tensor

| [INDEX] | [SYMBOL]                      | [PACKAGE_ROLE]   | [CAPABILITY]            |
| :-----: | :---------------------------- | :--------------- | :---------------------- |
|   [1]   | `Tensor<T>`                   | tensor root      | owns tensor data        |
|   [2]   | `ITensor<T>`                  | tensor contract  | defines mutable tensor  |
|   [3]   | `IReadOnlyTensor<T>`          | tensor contract  | defines read tensor     |
|   [4]   | `TensorShape`                 | shape descriptor | describes dimensions    |
|   [5]   | `TensorSpan<T>`               | span view        | addresses tensor data   |
|   [6]   | `ReadOnlyTensorSpan<T>`       | span view        | reads tensor data       |
|   [7]   | `TensorDimensionSpan`         | dimension view   | addresses dimensions    |
|   [8]   | `ReadOnlyTensorDimensionSpan` | dimension view   | reads dimensions        |
|   [9]   | `TensorFlags`                 | tensor flags     | classifies tensor state |
|  [10]   | `TensorOperation`             | operation root   | executes tensor ops     |

[PUBLIC_TYPE_SCOPE]: indexing and marshalling
- rail: tensor

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]  | [CAPABILITY]            |
| :-----: | :----------------- | :-------------- | :---------------------- |
|   [1]   | `NIndex`           | index value     | addresses dimensions    |
|   [2]   | `NRange`           | range value     | slices dimensions       |
|   [3]   | `TensorMarshal`    | marshal surface | bridges memory handles  |
|   [4]   | `TensorPrimitives` | primitive root  | executes vectorized ops |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tensor construction and shape
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE] | [CAPABILITY]           |
| :-----: | :--------------------- | :----------- | :--------------------- |
|   [1]   | `Tensor.Create`        | factory call | creates tensor         |
|   [2]   | `CreateFromArray`      | factory call | creates tensor         |
|   [3]   | `CreateFromMemory`     | factory call | wraps memory           |
|   [4]   | `CreateFromSequence`   | factory call | builds sequence tensor |
|   [5]   | `CreateFromDiagonal`   | factory call | builds diagonal tensor |
|   [6]   | `Reshape`              | tensor call  | changes dimensions     |
|   [7]   | `Flatten`              | tensor call  | flattens dimensions    |
|   [8]   | `Slice`                | tensor call  | slices tensor data     |
|   [9]   | `GetPinnableReference` | span call    | exposes pinned ref     |

[ENTRYPOINT_SCOPE]: primitive operations
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]        | [CAPABILITY]            |
| :-----: | :--------------------- | :------------------ | :---------------------- |
|   [1]   | `TensorPrimitives.Add` | primitive call      | computes elementwise op |
|   [2]   | `Subtract`             | primitive call      | computes elementwise op |
|   [3]   | `Multiply`             | primitive call      | computes elementwise op |
|   [4]   | `Divide`               | primitive call      | computes elementwise op |
|   [5]   | `MultiplyAdd`          | primitive call      | computes fused op       |
|   [6]   | `FusedMultiplyAdd`     | primitive call      | computes fused op       |
|   [7]   | `Sum`                  | aggregation call    | reduces values          |
|   [8]   | `Product`              | aggregation call    | reduces values          |
|   [9]   | `Max`                  | aggregation call    | reduces values          |
|  [10]   | `Min`                  | aggregation call    | reduces values          |
|  [11]   | `IndexOfMax`           | search call         | finds extrema index     |
|  [12]   | `Cos`                  | transcendental call | computes trig op        |
|  [13]   | `Exp`                  | transcendental call | computes exponential op |
|  [14]   | `Log`                  | transcendental call | computes logarithm op   |
|  [15]   | `ConvertChecked`       | conversion call     | converts values         |
|  [16]   | `ConvertSaturating`    | conversion call     | converts values         |

## [4]-[IMPLEMENTATION_LAW]

[TENSOR_SHAPES]:
- namespace: `System.Numerics.Tensors`
- tensor root: `Tensor<T>`
- shape root: `TensorShape`
- span root: tensor spans and dimension spans
- index root: `NIndex` and `NRange`

[NUMERIC_PRIMITIVES]:
- operation root: `TensorPrimitives`
- operation families: arithmetic, fused, aggregation, extrema, conversion, trigonometric, exponential, logarithmic
- memory rule: primitives operate over spans and tensor spans before package-local wrappers
- benchmark rule: primitive selection requires measured receipts for hot paths

[LOCAL_ADMISSION]:
- Compute tensor lanes use package tensor shapes and primitives as first-class execution material.
- Tensor operations stay rail-owned and cannot become loose numeric helpers.
- Shape, rank, stride, slicing, and conversion rules are explicit execution policy.
- Model and vector rails can consume tensor spans without redefining tensor ownership.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: tensors, tensor spans, numeric primitives
- Accept: measured tensor execution
- Reject: bespoke tensor wrappers
