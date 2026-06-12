# [RASM_COMPUTE_API_TENSORS]

`System.Numerics.Tensors` supplies tensor owners, tensor spans, dimension
spans, native-sized indexing, marshalling, and vectorized numeric primitives
for measured Compute execution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors`
- assembly: `System.Numerics.Tensors`
- namespaces: `System.Numerics.Tensors`, `System.Buffers`, `System.Runtime.InteropServices`
- asset: runtime library
- rail: tensor

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tensor shapes
- rail: tensor

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :--------------------------------------------- | :-------------- | :--------------------- |
|   [1]   | `Tensor`                                       | algebra root    | owns factories and ops |
|   [2]   | `Tensor<T>`                                    | tensor owner    | owns tensor data       |
|   [3]   | `ITensor` / `ITensor<TSelf,T>`                 | tensor contract | defines mutable tensor |
|   [4]   | `IReadOnlyTensor` / `IReadOnlyTensor<TSelf,T>` | tensor contract | defines read tensor    |
|   [5]   | `TensorSpan<T>`                                | span view       | addresses tensor data  |
|   [6]   | `ReadOnlyTensorSpan<T>`                        | span view       | reads tensor data      |
|   [7]   | `TensorDimensionSpan<T>`                       | dimension view  | addresses dimensions   |
|   [8]   | `ReadOnlyTensorDimensionSpan<T>`               | dimension view  | reads dimensions       |

[PUBLIC_TYPE_SCOPE]: indexing and marshalling
- rail: tensor

| [INDEX] | [SYMBOL]           | [NAMESPACE]                      | [CAPABILITY]            |
| :-----: | :----------------- | :------------------------------- | :---------------------- |
|   [1]   | `NIndex`           | `System.Buffers`                 | addresses dimensions    |
|   [2]   | `NRange`           | `System.Buffers`                 | slices dimensions       |
|   [3]   | `TensorMarshal`    | `System.Runtime.InteropServices` | bridges raw tensor refs |
|   [4]   | `TensorPrimitives` | `System.Numerics.Tensors`        | executes vectorized ops |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tensor construction and shape
- rail: tensor

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :-------------------------------- | :--------------- | :----------------------- |
|   [1]   | `Tensor.Create`                   | factory call     | wraps array as tensor    |
|   [2]   | `CreateFromShape`                 | factory call     | allocates shaped tensor  |
|   [3]   | `CreateFromShapeUninitialized`    | factory call     | allocates raw tensor     |
|   [4]   | `FillGaussianNormalDistribution`  | fill call        | fills random values      |
|   [5]   | `FillUniformDistribution`         | fill call        | fills random values      |
|   [6]   | `AsTensorSpan`                    | projection call  | projects span view       |
|   [7]   | `AsReadOnlyTensorSpan`            | projection call  | projects read view       |
|   [8]   | `Reshape`                         | shape call       | changes dimensions       |
|   [9]   | `Squeeze` / `Unsqueeze`           | shape call       | edits unit dimensions    |
|  [10]   | `PermuteDimensions` / `Transpose` | shape call       | reorders dimensions      |
|  [11]   | `Broadcast` / `BroadcastTo`       | shape call       | expands dimensions       |
|  [12]   | `Concatenate` / `Stack` / `Split` | composition call | joins and splits tensors |
|  [13]   | `SetSlice` / `FilteredUpdate`     | update call      | writes tensor regions    |
|  [14]   | `Tensor<T>.Slice`                 | tensor call      | slices tensor data       |
|  [15]   | `Tensor<T>.GetDimensionSpan`      | tensor call      | addresses dimension      |
|  [16]   | `Tensor<T>.FlattenTo`             | tensor call      | flattens to span         |
|  [17]   | `Tensor<T>.ToDenseTensor`         | tensor call      | densifies strided data   |
|  [18]   | `Tensor<T>.GetPinnableReference`  | tensor call      | exposes pinned ref       |

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
|   [7]   | `Dot`                  | reduction call      | computes inner product  |
|   [8]   | `CosineSimilarity`     | reduction call      | computes similarity     |
|   [9]   | `Sum`                  | aggregation call    | reduces values          |
|  [10]   | `Product`              | aggregation call    | reduces values          |
|  [11]   | `Max`                  | aggregation call    | reduces values          |
|  [12]   | `Min`                  | aggregation call    | reduces values          |
|  [13]   | `IndexOfMax`           | search call         | finds extrema index     |
|  [14]   | `SoftMax` / `Sigmoid`  | activation call     | computes activation     |
|  [15]   | `Cos`                  | transcendental call | computes trig op        |
|  [16]   | `Exp`                  | transcendental call | computes exponential op |
|  [17]   | `Log`                  | transcendental call | computes logarithm op   |
|  [18]   | `ConvertChecked`       | conversion call     | converts values         |
|  [19]   | `ConvertSaturating`    | conversion call     | converts values         |

[ENTRYPOINT_SCOPE]: marshalling operations
- rail: tensor

| [INDEX] | [SURFACE]                                | [CALL_SHAPE] | [CAPABILITY]          |
| :-----: | :--------------------------------------- | :----------- | :-------------------- |
|   [1]   | `TensorMarshal.CreateTensorSpan`         | factory call | wraps raw memory      |
|   [2]   | `TensorMarshal.CreateReadOnlyTensorSpan` | factory call | wraps raw read memory |
|   [3]   | `TensorMarshal.GetReference`             | ref call     | exposes span data ref |

## [4]-[IMPLEMENTATION_LAW]

[TENSOR_SHAPES]:
- namespace: `System.Numerics.Tensors`
- tensor root: `Tensor<T>` with `Tensor` as the static factory and operation surface
- span root: tensor spans and dimension spans
- index root: `NIndex` and `NRange` in `System.Buffers`
- marshal root: `TensorMarshal` in `System.Runtime.InteropServices`

[NUMERIC_PRIMITIVES]:
- operation root: `TensorPrimitives`
- operation families: arithmetic, fused, reduction, aggregation, extrema, activation, conversion, trigonometric, exponential, logarithmic
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
