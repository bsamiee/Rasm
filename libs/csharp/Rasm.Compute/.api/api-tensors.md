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

| [INDEX] | [SYMBOL]           | [NAMESPACE]                      | [CAPABILITY]             |
| :-----: | :----------------- | :------------------------------- | :----------------------- |
|   [1]   | `NIndex`           | `System.Buffers`                 | addresses dimensions     |
|   [2]   | `NRange`           | `System.Buffers`                 | slices dimensions        |
|   [3]   | `TensorShape`      | `System.Numerics.Tensors`        | carries rank and strides |
|   [4]   | `TensorMarshal`    | `System.Runtime.InteropServices` | bridges raw tensor refs  |
|   [5]   | `TensorPrimitives` | `System.Numerics.Tensors`        | executes vectorized ops  |

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

[ENTRYPOINT_SCOPE]: shape-edit and remap operations
- rail: tensor

| [INDEX] | [SURFACE]                | [CALL_SHAPE] | [CAPABILITY]               |
| :-----: | :----------------------- | :----------- | :------------------------- |
|   [1]   | `SqueezeDimension`       | shape call   | removes one unit dimension |
|   [2]   | `StackAlongDimension`    | shape call   | stacks along a chosen axis |
|   [3]   | `ConcatenateOnDimension` | shape call   | joins along a chosen axis  |
|   [4]   | `Reverse`                | remap call   | reverses element order     |
|   [5]   | `Resize`                 | remap call   | resizes to a new shape     |

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

[ENTRYPOINT_SCOPE]: elementwise, rounding, and transcendental primitives
- rail: tensor

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]               |
| :-----: | :----------------- | :------------------ | :------------------------- |
|   [1]   | `Negate`           | primitive call      | computes elementwise op    |
|   [2]   | `Abs`              | primitive call      | computes elementwise op    |
|   [3]   | `CopySign`         | primitive call      | transfers sign elementwise |
|   [4]   | `AddMultiply`      | primitive call      | computes fused op          |
|   [5]   | `Clamp`            | primitive call      | bounds values elementwise  |
|   [6]   | `Round`            | rounding call       | rounds values              |
|   [7]   | `Floor`            | rounding call       | rounds values down         |
|   [8]   | `Ceiling`          | rounding call       | rounds values up           |
|   [9]   | `Sin`              | transcendental call | computes trig op           |
|  [10]   | `Tanh`             | transcendental call | computes hyperbolic op     |
|  [11]   | `Sqrt`             | transcendental call | computes square root       |
|  [12]   | `Cbrt`             | transcendental call | computes cube root         |
|  [13]   | `DegreesToRadians` | transcendental call | converts angle units       |
|  [14]   | `Pow`              | transcendental call | computes power op          |
|  [15]   | `Atan2`            | transcendental call | computes arctangent op     |

[ENTRYPOINT_SCOPE]: reduction, similarity, bitwise, and conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]                    |
| :-----: | :--------------------- | :-------------- | :------------------------------ |
|   [1]   | `Norm`                 | reduction call  | computes Euclidean norm         |
|   [2]   | `MaxMagnitude`         | reduction call  | reduces by absolute extremum    |
|   [3]   | `Average`              | statistics call | computes mean                   |
|   [4]   | `StdDev`               | statistics call | computes standard deviation     |
|   [5]   | `Distance`             | similarity call | computes Euclidean distance     |
|   [6]   | `HammingDistance`      | similarity call | counts differing elements       |
|   [7]   | `BitwiseAnd`           | bitwise call    | computes bitwise op             |
|   [8]   | `ShiftLeft`            | bitwise call    | shifts integer values           |
|   [9]   | `ShiftRightArithmetic` | bitwise call    | shifts with sign extension      |
|  [10]   | `ConvertTruncating`    | conversion call | converts values                 |
|  [11]   | `ConvertSaturating`    | conversion call | converts values with saturation |

[ENTRYPOINT_SCOPE]: interpolation, reciprocal, hypot, and half-conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]                                                              |
| :-----: | :----------------- | :------------------ | :------------------------------------------------------------------------ |
|   [1]   | `Lerp`             | interpolation call  | `(x, y, amount, dst)` linear interpolation; `amount` span or scalar       |
|   [2]   | `Hypot`            | reduction call      | `(x, y, dst)` overflow-safe Euclidean magnitude per element               |
|   [3]   | `Reciprocal`       | primitive call      | `(x, dst)` element reciprocal; `ReciprocalEstimate` is the fast variant   |
|   [4]   | `ReciprocalSqrt`   | primitive call      | `(x, dst)` reciprocal square root; `ReciprocalSqrtEstimate` fast variant  |
|   [5]   | `RootN`            | transcendental call | `(x, int n, dst)` element nth root                                        |
|   [6]   | `ScaleB`           | transcendental call | `(x, int n, dst)` scales by `2^n`                                         |
|   [7]   | `SinCos`           | transcendental call | `(x, sinDst, cosDst)` fused sine and cosine                               |
|   [8]   | `Ieee754Remainder` | transcendental call | `(x, y, dst)` IEEE-754 remainder                                          |
|   [9]   | `ConvertToHalf`    | conversion call     | `(ReadOnlySpan<float>, Span<Half>)` narrows to `Half`                     |
|  [10]   | `ConvertToSingle`  | conversion call     | `(ReadOnlySpan<Half>, Span<float>)` widens from `Half`                    |
|  [11]   | `ConvertToInteger` | conversion call     | `(TFrom, TTo)` float-to-integer; `ConvertToIntegerNative` saturating form |
|  [12]   | `Truncate`         | rounding call       | `(x, dst)` truncates toward zero                                          |

[ENTRYPOINT_SCOPE]: bitwise, population, and rotation primitives
- rail: tensor

| [INDEX] | [SURFACE]           | [CALL_SHAPE] | [CAPABILITY]                                                          |
| :-----: | :------------------ | :----------- | :-------------------------------------------------------------------- |
|   [1]   | `PopCount`          | bitwise call | `(x, dst)` per-element set-bit count; `long PopCount(x)` total reduce |
|   [2]   | `LeadingZeroCount`  | bitwise call | `(x, dst)` leading-zero count per element                             |
|   [3]   | `TrailingZeroCount` | bitwise call | `(x, dst)` trailing-zero count per element                            |
|   [4]   | `BitwiseOr`         | bitwise call | `(x, y, dst)` element bitwise OR; `y` span or scalar                  |
|   [5]   | `Xor`               | bitwise call | `(x, y, dst)` element bitwise XOR                                     |
|   [6]   | `OnesComplement`    | bitwise call | `(x, dst)` element ones complement                                    |
|   [7]   | `ShiftRightLogical` | bitwise call | `(x, int shiftAmount, dst)` logical right shift                       |
|   [8]   | `RotateLeft`        | bitwise call | `(x, int rotateAmount, dst)`; `RotateRight` mirrors it                |

[ENTRYPOINT_SCOPE]: reduction, sign, and predicate-mask primitives
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]                                                                         |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------------------- |
|   [1]   | `SumOfSquares`         | reduction call  | `T SumOfSquares(x)` reduces the sum of squared elements                              |
|   [2]   | `SumOfMagnitudes`      | reduction call  | `T SumOfMagnitudes(x)` reduces the sum of absolute values                            |
|   [3]   | `ProductOfSums`        | reduction call  | `T ProductOfSums(x, y)` reduces the product of pairwise sums                         |
|   [4]   | `ProductOfDifferences` | reduction call  | `T ProductOfDifferences(x, y)` reduces the product of pairwise diffs                 |
|   [5]   | `IndexOfMaxMagnitude`  | search call     | `int` index of the absolute extremum; `IndexOfMin` / `IndexOfMinMagnitude` mirror it |
|   [6]   | `Sign`                 | primitive call  | `(x, Span<int> dst)` element sign as `-1`/`0`/`1`                                    |
|   [7]   | `HammingBitDistance`   | similarity call | `long` count of differing bits across the integral pair                              |
|   [8]   | `IsNaN` / `IsFinite`   | predicate call  | `(x, Span<bool> dst)` per-element mask; `*All` / `*Any` aggregate variants           |
|   [9]   | `MinNumber`            | reduction call  | `T MinNumber<T>(ReadOnlySpan<T>) where T : INumber<T>` NaN-skipping minimum          |
|  [10]   | `MaxNumber`            | reduction call  | `T MaxNumber<T>(ReadOnlySpan<T>) where T : INumber<T>` NaN-skipping maximum          |

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
- operation families: arithmetic, fused, interpolation, reciprocal, reduction, aggregation, extrema, sign, activation, conversion, trigonometric, exponential, logarithmic, bitwise, population-count, rotation, predicate-mask
- generic-math constraints: operators are generic over `T` bound by `INumberBase`, `IFloatingPointIeee754`, `IRootFunctions`, `IBinaryInteger`, `IBitwiseOperators`, or `IShiftOperators` as the family requires; integer-only families reject floating element types at the constraint
- destination rule: elementwise operators write into a caller-supplied `Span<T>` destination; reductions return a scalar `T`; predicate masks write `Span<bool>` with `*All` / `*Any` aggregate forms
- memory rule: primitives operate over spans and tensor spans before package-local wrappers
- benchmark rule: primitive selection requires measured receipts for hot paths

[ABSENT_OPERATORS]:
- `TensorPrimitives` exposes no `Normalize` operator; vector normalization composes from `Norm` (or `SumOfSquares` + `Sqrt`) followed by `Divide` against the reduced magnitude
- a normalization owner row that names a single `TensorPrimitives.Normalize` call is unresolvable and stays SPIKE until expressed as the `Norm`/`Divide` composition

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
