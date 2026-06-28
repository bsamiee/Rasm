# [RASM_API_TENSORS]

`System.Numerics.Tensors` supplies tensor owners, tensor spans, dimension spans, native-sized indexing, marshalling, and a hardware-accelerated `TensorPrimitives` vectorized-numeric surface for measured Compute execution. Its value: `TensorPrimitives` JIT-lowers each elementwise/reduction operator to the widest available SIMD ISA (AVX-512 / AVX2 / NEON via `System.Runtime.Intrinsics`) over a caller-supplied span, so the kernel composes one vectorized primitive against its existing `ReadOnlySpan<float>`/`ReadOnlySpan<double>` coordinate buffers rather than authoring a hand-rolled element loop the JIT cannot vectorize. The dense rail stacks: a `Tensor<T>` strided owner (or a raw coordinate span) feeds a `TensorPrimitives` fused operator whose `Span<T>` destination is the next stage's input, the whole pipeline gated by a BenchmarkDotNet receipt that proves the vectorized path beats the scalar baseline on the hot lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors`
- version: `10.0.9` (NuGet pin; consumer-bound `lib/net10.0` asset)
- assembly: `System.Numerics.Tensors`
- license: MIT (.NET runtime library)
- namespaces: `System.Numerics.Tensors`, `System.Buffers`, `System.Runtime.InteropServices`
- asset: runtime library (managed; `TensorPrimitives` lowers to `System.Runtime.Intrinsics` SIMD, falling back to a scalar loop where no ISA is available — the operator is correct on every target, accelerated where the hardware admits)
- abi: `Tensor<T>`/`TensorSpan<T>`/`ReadOnlyTensorSpan<T>` are `ref struct`-adjacent strided views over `nint`-indexed memory; `TensorPrimitives` operators are `static` generic-math methods constrained over the BCL numeric interfaces — no instance state, no allocation, the destination span is caller-owned
- rail: tensor

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

| [INDEX] | [SYMBOL]           | [NAMESPACE]                      | [CAPABILITY]                                                                            |
| :-----: | :----------------- | :------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `NIndex`           | `System.Buffers`                 | native-sized dimension index; `FromStart`/`FromEnd`/`Start`/`End`/`GetOffset`, `Index` interop |
|  [02]   | `NRange`           | `System.Buffers`                 | native-sized dimension slice; `StartAt`/`EndAt`/`All`/`GetOffsetAndLength`, `Range` interop |
|  [03]   | `TensorShape`      | `System.Numerics.Tensors`        | carries rank, lengths, strides, `IsDense`/`HasAnyDenseDimensions`/`FlattenedLength` — the strided layout descriptor every view shares |
|  [04]   | `TensorFlags`      | `System.Numerics.Tensors`        | `[Flags]` layout state (`None`/`IsDense`/`IsPinned`) the shape carries                  |
|  [05]   | `TensorMarshal`    | `System.Runtime.InteropServices` | bridges raw tensor refs; the unsafe `ref`-to-`TensorSpan` boundary                      |
|  [06]   | `TensorPrimitives` | `System.Numerics.Tensors`        | executes SIMD-vectorized elementwise/reduction/predicate ops over spans                 |

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

| [INDEX] | [SURFACE]                          | [CALL_SHAPE] | [CAPABILITY]                                                  |
| :-----: | :--------------------------------- | :----------- | :----------------------------------------------------------- |
|  [01]   | `SqueezeDimension`                 | shape call   | removes one unit dimension                                   |
|  [02]   | `StackAlongDimension`              | shape call   | stacks along a chosen axis                                   |
|  [03]   | `ConcatenateOnDimension`           | shape call   | joins along a chosen axis                                    |
|  [04]   | `Reverse` / `ReverseDimension`     | remap call   | reverses element order globally or along one axis            |
|  [05]   | `Resize` / `ResizeTo`              | remap call   | resizes to a new shape; `ResizeTo` writes into a destination |
|  [06]   | `TryBroadcastTo`                   | shape call   | non-throwing broadcast that fails the `bool` on a shape mismatch |

[ENTRYPOINT_SCOPE]: tensor-level comparison and equality (mask + aggregate)
- rail: tensor

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]   | [CAPABILITY]                                                                  |
| :-----: | :------------------------------------------------- | :------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Equals` / `EqualsAll` / `EqualsAny`               | comparison call | elementwise `Tensor<bool>` mask plus the all/any aggregate fold              |
|  [02]   | `GreaterThan` / `GreaterThanAll` / `GreaterThanAny` | comparison call | strict-greater mask + aggregate; `GreaterThanOrEqual` family mirrors it       |
|  [03]   | `LessThan` / `LessThanAll` / `LessThanAny`         | comparison call | strict-less mask + aggregate; `LessThanOrEqual` family mirrors it             |
|  [04]   | `SequenceEqual`                                    | comparison call | whole-tensor structural equality the reconciliation lane reads               |
|  [05]   | `RadiansToDegrees`                                 | transcendental call | the tensor-level inverse of `DegreesToRadians` (the `TensorPrimitives` form documents `DegreesToRadians` only) |

[ENTRYPOINT_SCOPE]: arithmetic and fused primitive operations
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]     | [CAPABILITY]            |
| :-----: | :--------------------- | :--------------- | :---------------------- |
|  [01]   | `TensorPrimitives.Add` | primitive call   | computes elementwise op |
|  [02]   | `Subtract`             | primitive call   | computes elementwise op |
|  [03]   | `Multiply`             | primitive call   | computes elementwise op |
|  [04]   | `Divide`               | primitive call   | computes elementwise op |
|  [05]   | `MultiplyAdd`          | primitive call   | computes fused op       |
|  [06]   | `FusedMultiplyAdd`     | primitive call   | computes fused op       |
|  [07]   | `Dot`                  | reduction call   | computes inner product  |
|  [08]   | `CosineSimilarity`     | reduction call   | computes similarity     |
|  [09]   | `Sum`                  | aggregation call | reduces values          |

[ENTRYPOINT_SCOPE]: aggregation, activation, transcendental, and conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]             | [CALL_SHAPE]        | [CAPABILITY]            |
| :-----: | :-------------------- | :------------------ | :---------------------- |
|  [01]   | `Product`             | aggregation call    | reduces values          |
|  [02]   | `Max`                 | aggregation call    | reduces values          |
|  [03]   | `Min`                 | aggregation call    | reduces values          |
|  [04]   | `IndexOfMax`          | search call         | finds extrema index     |
|  [05]   | `SoftMax` / `Sigmoid` | activation call     | computes activation     |
|  [06]   | `Cos`                 | transcendental call | computes trig op        |
|  [07]   | `Exp`                 | transcendental call | computes exponential op |
|  [08]   | `Log`                 | transcendental call | computes logarithm op   |
|  [09]   | `ConvertChecked`      | conversion call     | converts values         |
|  [10]   | `ConvertSaturating`   | conversion call     | converts values         |

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
|  [13]   | `DegreesToRadians` | transcendental call | converts angle units       |
|  [14]   | `Pow`              | transcendental call | computes power op          |
|  [15]   | `Atan2`            | transcendental call | computes arctangent op     |

[ENTRYPOINT_SCOPE]: reduction, similarity, bitwise, and conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]              | [CALL_SHAPE]    | [CAPABILITY]                 |
| :-----: | :--------------------- | :-------------- | :--------------------------- |
|  [01]   | `Norm`                 | reduction call  | computes Euclidean norm      |
|  [02]   | `MaxMagnitude`         | reduction call  | reduces by absolute extremum |
|  [03]   | `Average`              | statistics call | computes mean                |
|  [04]   | `StdDev`               | statistics call | computes standard deviation  |
|  [05]   | `Distance`             | similarity call | computes Euclidean distance  |
|  [06]   | `HammingDistance`      | similarity call | counts differing elements    |
|  [07]   | `BitwiseAnd`           | bitwise call    | computes bitwise op          |
|  [08]   | `ShiftLeft`            | bitwise call    | shifts integer values        |
|  [09]   | `ShiftRightArithmetic` | bitwise call    | shifts with sign extension   |
|  [10]   | `ConvertTruncating`    | conversion call | converts values              |

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
|  [08]   | `IsNaN` / `IsFinite`   | predicate call  | `(x, Span<bool> dst)` per-element mask; the full family is `IsNaN`/`IsFinite`/`IsInfinity`/`IsNormal`/`IsSubnormal`/`IsZero`/`IsPositive`/`IsNegative`/`IsInteger`/`IsEvenInteger`/`IsOddInteger`/`IsPow2`, each with `*All`/`*Any` aggregate forms |
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
- memory rule: primitives operate over spans and tensor spans before package-local wrappers; in-place is admitted (`destination` may overlap `x`) so a fused pipeline reuses one buffer across stages
- simd rule: each operator JIT-lowers to the widest available `System.Runtime.Intrinsics` ISA (AVX-512/AVX2/SSE on x64, AdvSimd on ARM64) with a scalar tail and a scalar fallback where no ISA is present — the primitive is the vectorized form a hand-rolled element loop cannot match, which is precisely why a package-local numeric loop over the same span is the rejected reinvention
- benchmark rule: primitive selection requires measured BenchmarkDotNet receipts for hot paths — the vectorized-vs-scalar speedup is proven per lane, never assumed

[ABSENT_OPERATORS]:
- `TensorPrimitives` exposes no `Normalize` operator; vector normalization composes from `Norm` (or `SumOfSquares` + `Sqrt`) followed by `Divide` against the reduced magnitude
- a normalization owner row that names a single `TensorPrimitives.Normalize` call is unresolvable and stays SPIKE until expressed as the `Norm`/`Divide` composition

[LOCAL_ADMISSION]:
- Compute tensor lanes use package tensor shapes and primitives as first-class execution material.
- Tensor operations stay rail-owned and cannot become loose numeric helpers.
- Shape, rank, stride, slicing, and conversion rules are explicit execution policy.
- Model and vector rails can consume tensor spans without redefining tensor ownership.

[INTEGRATION_STACKING]:
- Span-feed, not re-pack: the geometry kernel's struct-of-arrays coordinate buffers (`float[]`/`double[]` already laid out by the BVH `NodeStore`, the mesh vertex store, the predicate `Expansion` arrays) feed `TensorPrimitives` operators DIRECTLY as `ReadOnlySpan<T>` — and `TensorMarshal.CreateTensorSpan(ref data, dataLength, lengths, strides, pinned)` wraps a pinned raw buffer as a `TensorSpan<T>` view without a copy. A second tensor-shaped re-pack of a buffer the kernel already owns is the rejected double-layout.
- Fused single rail: a vectorized stage chains by aliasing the destination — `Subtract(x, y, tmp)` then `Multiply(tmp, tmp, tmp)` then `Sum(tmp)` is one allocation-free squared-distance reduction; `MultiplyAdd`/`FusedMultiplyAdd`/`AddMultiply`/`Lerp`/`Hypot` collapse a two-step arithmetic into one fused vectorized pass, and the `Tensor`-level mirror (`Tensor.Add`/`Tensor.CosineSimilarity`/`Tensor.Norm`) runs the same operator over a strided owner when the lane is rank-aware.
- Receipt-gated: the hot vectorized lane carries a BenchmarkDotNet receipt proving the speedup; the receipt is the typed evidence, not a comment — a tensor primitive enters the hot path only with its measured win.
- Not the exact-predicate path: `TensorPrimitives` is IEEE-754 floating SIMD — fast bulk arithmetic, NEVER the substrate of an exact geometric predicate. A robustness decision (`Orient3D`/`Orient2D` straddle sign, an `Expansion` ordering key) is the `Numerics/predicates` exact-arithmetic floor's concern, and routing a crossing/containment sign through a vectorized float reduction is the named non-robustness defect. Tensors accelerate the bulk numeric transform; the predicate floor owns the exact decision.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: tensors, tensor spans, dimension spans, native-sized indexing, marshalling, and SIMD numeric primitives
- Accept: measured tensor execution composing the kernel's existing span buffers through one fused vectorized rail
- Reject: bespoke tensor wrappers; a package-local numeric loop over a span `TensorPrimitives` already vectorizes; an exact-predicate decision routed through a floating SIMD reduction
