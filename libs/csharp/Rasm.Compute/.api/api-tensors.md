# [RASM_COMPUTE_API_TENSORS]

`System.Numerics.Tensors` supplies tensor owners, tensor spans, dimension
spans, native-sized indexing, marshalling, and vectorized numeric primitives
for measured Compute execution.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors` (`10.0.9`)
- assembly: `System.Numerics.Tensors`
- license: MIT
- namespaces: `System.Numerics.Tensors`, `System.Buffers`, `System.Runtime.InteropServices`
- asset: runtime library (net10.0)
- rail: tensor
- roster: `TensorPrimitives` is the static vectorized-op kernel; `Tensor`/`Tensor<T>` the shaped owner; `TensorSpan<T>`/`ReadOnlyTensorSpan<T>` the strided views; `TensorShape`/`TensorFlags`/`TensorOperation` are the internal shape/flag/op-descriptor support types

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tensor shapes
- rail: tensor

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :--------------------------------------------- | :-------------- | :--------------------- |
|  [01]   | `Tensor`                                       | algebra root    | owns factories and ops |
|  [02]   | `Tensor<T>`                                    | tensor owner    | owns tensor data       |
|  [03]   | `ITensor` / `ITensor<TSelf,T>`                 | tensor contract | defines mutable tensor |
|  [04]   | `IReadOnlyTensor` / `IReadOnlyTensor<TSelf,T>` | tensor contract | defines read tensor    |
|  [05]   | `TensorSpan<T>`                                | span view       | addresses tensor data  |
|  [06]   | `ReadOnlyTensorSpan<T>`                        | span view       | reads tensor data      |
|  [07]   | `TensorDimensionSpan<T>`                       | dimension view  | addresses dimensions   |
|  [08]   | `ReadOnlyTensorDimensionSpan<T>`               | dimension view  | reads dimensions       |

[PUBLIC_TYPE_SCOPE]: indexing and marshalling
- rail: tensor

| [INDEX] | [SYMBOL]           | [NAMESPACE]                      | [CAPABILITY]            |
| :-----: | :----------------- | :------------------------------- | :---------------------- |
|  [01]   | `NIndex`           | `System.Buffers`                 | addresses dimensions    |
|  [02]   | `NRange`           | `System.Buffers`                 | slices dimensions       |
|  [03]   | `TensorMarshal`    | `System.Runtime.InteropServices` | bridges raw tensor refs |
|  [04]   | `TensorPrimitives` | `System.Numerics.Tensors`        | executes vectorized ops |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tensor construction and shape
- rail: tensor

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :-------------------------------- | :--------------- | :----------------------- |
|  [01]   | `Tensor.Create`                   | factory call     | wraps array as tensor    |
|  [02]   | `CreateFromShape`                 | factory call     | allocates shaped tensor  |
|  [03]   | `CreateFromShapeUninitialized`    | factory call     | allocates raw tensor     |
|  [04]   | `FillGaussianNormalDistribution`  | fill call        | fills random values      |
|  [05]   | `FillUniformDistribution`         | fill call        | fills random values      |
|  [06]   | `AsTensorSpan`                    | projection call  | projects span view       |
|  [07]   | `AsReadOnlyTensorSpan`            | projection call  | projects read view       |
|  [08]   | `Reshape`                         | shape call       | changes dimensions       |
|  [09]   | `Squeeze` / `Unsqueeze`           | shape call       | edits unit dimensions    |
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
|  [01]   | `SqueezeDimension`       | shape call   | removes one unit dimension |
|  [02]   | `StackAlongDimension`    | shape call   | stacks along a chosen axis |
|  [03]   | `ConcatenateOnDimension` | shape call   | joins along a chosen axis  |
|  [04]   | `Reverse`                | remap call   | reverses element order     |
|  [05]   | `Resize`                 | remap call   | resizes to a new shape     |

[ENTRYPOINT_SCOPE]: primitive operations
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]        | [CAPABILITY]            |
| :-----: | :--------------------- | :------------------ | :---------------------- |
|  [01]   | `TensorPrimitives.Add` | primitive call      | computes elementwise op |
|  [02]   | `Subtract`             | primitive call      | computes elementwise op |
|  [03]   | `Multiply`             | primitive call      | computes elementwise op |
|  [04]   | `Divide`               | primitive call      | computes elementwise op |
|  [05]   | `MultiplyAdd`          | primitive call      | computes fused op       |
|  [06]   | `FusedMultiplyAdd`     | primitive call      | computes fused op       |
|  [07]   | `Dot`                  | reduction call      | computes inner product  |
|  [08]   | `CosineSimilarity`     | reduction call      | computes similarity     |
|  [09]   | `Sum`                  | aggregation call    | reduces values          |
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
|  [01]   | `Negate`           | primitive call      | computes elementwise op    |
|  [02]   | `Abs`              | primitive call      | computes elementwise op    |
|  [03]   | `CopySign`         | primitive call      | transfers sign elementwise |
|  [04]   | `AddMultiply`      | primitive call      | computes fused op          |
|  [05]   | `Clamp`            | primitive call      | bounds values elementwise  |
|  [06]   | `Round`            | rounding call       | rounds values              |
|  [07]   | `Floor`            | rounding call       | rounds values down         |
|  [08]   | `Ceiling`          | rounding call       | rounds values up           |
|  [09]   | `Sin`              | transcendental call | computes trig op           |
|  [10]   | `Tanh`             | transcendental call | computes hyperbolic op     |
|  [11]   | `Sqrt`             | transcendental call | computes square root       |
|  [12]   | `Cbrt`             | transcendental call | computes cube root         |
|  [13]   | `DegreesToRadians` | transcendental call | converts angle units; `RadiansToDegrees` mirrors it |
|  [14]   | `Pow`              | transcendental call | computes power op          |
|  [15]   | `Atan2`            | transcendental call | computes arctangent op     |
|  [16]   | `Acos` / `Asin` / `Atan` | transcendental call | inverse-trig family; `*Pi` (`AcosPi`/`AsinPi`/`AtanPi`/`Atan2Pi`) divide-by-π variants |
|  [17]   | `Acosh` / `Asinh` / `Atanh` | transcendental call | inverse-hyperbolic family             |
|  [18]   | `Cosh` / `Sinh`    | transcendental call | hyperbolic family (paired with `Tanh`) |
|  [19]   | `Exp2` / `Exp10` / `ExpM1` | transcendental call | base-2/base-10/`exp(x)-1` exponentials |
|  [20]   | `Log2` / `Log10` / `LogP1` | logarithm call      | base-2/base-10/`log(1+x)` logarithms  |
|  [21]   | `BitIncrement` / `BitDecrement` | primitive call | next/previous representable float       |

[ENTRYPOINT_SCOPE]: reduction, similarity, bitwise, and conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]                    |
| :-----: | :--------------------- | :-------------- | :------------------------------ |
|  [01]   | `Norm`                 | reduction call  | computes Euclidean norm         |
|  [02]   | `MaxMagnitude`         | reduction call  | reduces by absolute extremum    |
|  [03]   | `Average`              | statistics call | computes mean                   |
|  [04]   | `StdDev`               | statistics call | computes standard deviation     |
|  [05]   | `Distance`             | similarity call | computes Euclidean distance     |
|  [06]   | `HammingDistance`      | similarity call | counts differing elements       |
|  [07]   | `BitwiseAnd`           | bitwise call    | computes bitwise op             |
|  [08]   | `ShiftLeft`            | bitwise call    | shifts integer values           |
|  [09]   | `ShiftRightArithmetic` | bitwise call    | shifts with sign extension      |
|  [10]   | `ConvertTruncating`    | conversion call | converts values                 |
|  [11]   | `ConvertSaturating`    | conversion call | converts values with saturation |

[ENTRYPOINT_SCOPE]: interpolation, reciprocal, hypot, and half-conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]          | [CALL_SHAPE]        | [CAPABILITY]                                                              |
| :-----: | :----------------- | :------------------ | :------------------------------------------------------------------------ |
|  [01]   | `Lerp`             | interpolation call  | `(x, y, amount, dst)` linear interpolation; `amount` span or scalar       |
|  [02]   | `Hypot`            | reduction call      | `(x, y, dst)` overflow-safe Euclidean magnitude per element               |
|  [03]   | `Reciprocal`       | primitive call      | `(x, dst)` element reciprocal; `ReciprocalEstimate` is the fast variant   |
|  [04]   | `ReciprocalSqrt`   | primitive call      | `(x, dst)` reciprocal square root; `ReciprocalSqrtEstimate` fast variant  |
|  [05]   | `RootN`            | transcendental call | `(x, int n, dst)` element nth root                                        |
|  [06]   | `ScaleB`           | transcendental call | `(x, int n, dst)` scales by `2^n`                                         |
|  [07]   | `SinCos`           | transcendental call | `(x, sinDst, cosDst)` fused sine and cosine                               |
|  [08]   | `Ieee754Remainder` | transcendental call | `(x, y, dst)` IEEE-754 remainder                                          |
|  [09]   | `ConvertToHalf`    | conversion call     | `(ReadOnlySpan<float>, Span<Half>)` narrows to `Half`                     |
|  [10]   | `ConvertToSingle`  | conversion call     | `(ReadOnlySpan<Half>, Span<float>)` widens from `Half`                    |
|  [11]   | `ConvertToInteger` | conversion call     | `(TFrom, TTo)` float-to-integer; `ConvertToIntegerNative` saturating form |
|  [12]   | `Truncate`         | rounding call       | `(x, dst)` truncates toward zero                                          |

[ENTRYPOINT_SCOPE]: bitwise, population, and rotation primitives
- rail: tensor

| [INDEX] | [SURFACE]           | [CALL_SHAPE] | [CAPABILITY]                                                          |
| :-----: | :------------------ | :----------- | :-------------------------------------------------------------------- |
|  [01]   | `PopCount`          | bitwise call | `(x, dst)` per-element set-bit count; `long PopCount(x)` total reduce |
|  [02]   | `LeadingZeroCount`  | bitwise call | `(x, dst)` leading-zero count per element                             |
|  [03]   | `TrailingZeroCount` | bitwise call | `(x, dst)` trailing-zero count per element                            |
|  [04]   | `BitwiseOr`         | bitwise call | `(x, y, dst)` element bitwise OR; `y` span or scalar                  |
|  [05]   | `Xor`               | bitwise call | `(x, y, dst)` element bitwise XOR                                     |
|  [06]   | `OnesComplement`    | bitwise call | `(x, dst)` element ones complement                                    |
|  [07]   | `ShiftRightLogical` | bitwise call | `(x, int shiftAmount, dst)` logical right shift                       |
|  [08]   | `RotateLeft`        | bitwise call | `(x, int rotateAmount, dst)`; `RotateRight` mirrors it                |

[ENTRYPOINT_SCOPE]: reduction, sign, and predicate-mask primitives
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]                                                                         |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------------------- |
|  [01]   | `SumOfSquares`         | reduction call  | `T SumOfSquares(x)` reduces the sum of squared elements                              |
|  [02]   | `SumOfMagnitudes`      | reduction call  | `T SumOfMagnitudes(x)` reduces the sum of absolute values                            |
|  [03]   | `ProductOfSums`        | reduction call  | `T ProductOfSums(x, y)` reduces the product of pairwise sums                         |
|  [04]   | `ProductOfDifferences` | reduction call  | `T ProductOfDifferences(x, y)` reduces the product of pairwise diffs                 |
|  [05]   | `IndexOfMaxMagnitude`  | search call     | `int` index of the absolute extremum; `IndexOfMin` / `IndexOfMinMagnitude` mirror it |
|  [06]   | `Sign`                 | primitive call  | `(x, Span<int> dst)` element sign as `-1`/`0`/`1`                                    |
|  [07]   | `HammingBitDistance`   | similarity call | `long` count of differing bits across the integral pair                              |
|  [08]   | `IsNaN` / `IsFinite`   | predicate call  | `(x, Span<bool> dst)` per-element mask; `*All` / `*Any` aggregate variants           |
|  [09]   | `MinNumber`            | reduction call  | `T MinNumber<T>(ReadOnlySpan<T>) where T : INumber<T>` NaN-skipping minimum          |
|  [10]   | `MaxNumber`            | reduction call  | `T MaxNumber<T>(ReadOnlySpan<T>) where T : INumber<T>` NaN-skipping maximum          |

[ENTRYPOINT_SCOPE]: marshalling operations
- rail: tensor

| [INDEX] | [SURFACE]                                | [CALL_SHAPE] | [CAPABILITY]          |
| :-----: | :--------------------------------------- | :----------- | :-------------------- |
|  [01]   | `TensorMarshal.CreateTensorSpan`         | factory call | wraps raw memory      |
|  [02]   | `TensorMarshal.CreateReadOnlyTensorSpan` | factory call | wraps raw read memory |
|  [03]   | `TensorMarshal.GetReference`             | ref call     | exposes span data ref |

## [04]-[IMPLEMENTATION_LAW]

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

[INTEGRATION_STACKING]:
- `TensorPrimitives` is the kernel substrate for `Tensor/dispatch#KERNEL_DISPATCH`: every span row binds the `TensorPrimitives` member matching its Pascal-cased `TensorOpFamily` key into a `FrozenDictionary<TensorOpFamily, …Kernel<T>>` delegate table, and the `Activations<T>` author-folds (`ReLU`/`Gelu`/`SiLU`/`LogSoftMax`) compose `TensorPrimitives.Clamp`/`Sigmoid`/`Multiply`/`MultiplyAdd`/`Tanh`/`Max`/`Exp`/`Sum`/`Subtract` — never a per-element loop and never a fabricated `TensorPrimitives.Relu` phantom.
- The matrix family (`MatMul`, `Conv1D`/`2D`/`3D`) holds NO `TensorPrimitives` member: `Tensor/dispatch#KERNEL_DISPATCH` `Map` resolves these through the `Tensor/factor#KERNEL_LOWERING` GEMM/im2col-GEMM lowering or, under the device residency gate, the `Tensor/dispatch#DEVICE_KERNELS` WGSL `Silk.NET.WebGPU` `ComputePipeline` — a tensor span feeds a WGPU storage `Buffer` and a `Tensor<T>.GetPinnableReference`/`TensorMarshal.GetReference` root admits to an ORT device value through `Tensor/residency#ORT_BRIDGE` `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)`, so the same `Tensor<T>` crosses the CPU `TensorPrimitives`, the WebGPU compute (`api-silk-webgpu.md`), and the ONNX device boundary with no parallel tensor type. `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>` and the sibling C-data residency members (`CreateTensorValueWithData`/`CreateAllocatedTensorValue`/`BindOutputToDevice`) are owned by `api-onnxruntime.md` — a forward cross-catalog dependency this note tracks.
- `Tensor.FillGaussianNormalDistribution`/`FillUniformDistribution` are the equivalence-sampler fillers `Tensor/dispatch#EQUIVALENCE_INTEROP` `EquivalenceLaw.Prove` calls (a hand-rolled sample-RNG loop is the deleted form); `Tensor.FilteredUpdate(in TensorSpan<T>, in ReadOnlyTensorSpan<bool>, in ReadOnlyTensorSpan<T>)` is the predicate-masked-write the `Mask`/`MaskedWrite` row binds (the unconditional `SetSlice` region overwrite that ignores the mask is the deleted form); `Tensor<T>.ToDenseTensor` densifies a permuted/sliced backing once before the `Pool` flat-window walk over a `GetDimensionSpan` cursor.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors` (`10.0.9`, MIT)
- Owns: tensors, tensor spans, numeric primitives
- Accept: measured tensor execution; `TensorPrimitives` delegate-table binding into the dispatch kernel surface; `Tensor<T>` as the one carrier crossing CPU/WebGPU/ONNX boundaries via `TensorMarshal.GetReference`/`GetPinnableReference`
- Reject: bespoke tensor wrappers; a `DeviceTensor`/`GpuTensor` parallel type (device-ness is the `Tensor/residency#ORT_BRIDGE` residency discriminant); a single-call `TensorPrimitives.Normalize` row (it has no such member — compose `Norm` then `Divide`)
