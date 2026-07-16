# [RASM_API_TENSORS]

`System.Numerics.Tensors` supplies tensor owners, tensor spans, dimension spans, native-sized indexing, marshalling, and a hardware-accelerated `TensorPrimitives` vectorized-numeric surface for measured Compute execution. Its value: `TensorPrimitives` JIT-lowers each elementwise/reduction operator to the widest available SIMD ISA (AVX-512 / AVX2 / NEON via `System.Runtime.Intrinsics`) over a caller-supplied span, so the kernel composes one vectorized primitive against its existing `ReadOnlySpan<float>`/`ReadOnlySpan<double>` coordinate buffers rather than authoring a hand-rolled element loop the JIT cannot vectorize. The dense rail stacks: a `Tensor<T>` strided owner (or a raw coordinate span) feeds a `TensorPrimitives` fused operator whose `Span<T>` destination is the next stage's input, the whole pipeline gated by a BenchmarkDotNet receipt that proves the vectorized path beats the scalar baseline on the hot lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors`
- assembly: `System.Numerics.Tensors`
- license: MIT (.NET runtime library)
- namespaces: `System.Numerics.Tensors`, `System.Buffers`, `System.Runtime.InteropServices`
- asset: runtime library (managed; `TensorPrimitives` lowers to `System.Runtime.Intrinsics` SIMD, falling back to a scalar loop where no ISA is available — the operator is correct on every target, accelerated where the hardware admits)
- abi: `Tensor<T>`/`TensorSpan<T>`/`ReadOnlyTensorSpan<T>` are `ref struct`-adjacent strided views over `nint`-indexed memory; `TensorPrimitives` operators are `static` generic-math methods constrained over the BCL numeric interfaces — no instance state, no allocation, the destination span is caller-owned
- rail: tensor

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tensor shapes
- rail: tensor

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]  | [CAPABILITY]           |
| :-----: | :------------------------------- | :-------------- | :--------------------- |
|  [01]   | `Tensor`                         | algebra root    | owns tensor algebra    |
|  [02]   | `Tensor<T>`                      | tensor owner    | owns tensor data       |
|  [03]   | `ITensor`                        | tensor contract | defines mutable tensor |
|  [04]   | `ITensor<TSelf,T>`               | tensor contract | types mutable tensor   |
|  [05]   | `IReadOnlyTensor`                | tensor contract | defines read tensor    |
|  [06]   | `IReadOnlyTensor<TSelf,T>`       | tensor contract | types read tensor      |
|  [07]   | `TensorSpan<T>`                  | span view       | addresses tensor data  |
|  [08]   | `ReadOnlyTensorSpan<T>`          | span view       | reads tensor data      |
|  [09]   | `TensorDimensionSpan<T>`         | dimension view  | addresses dimensions   |
|  [10]   | `ReadOnlyTensorDimensionSpan<T>` | dimension view  | reads dimensions       |

[PUBLIC_TYPE_SCOPE]: indexing and marshalling
- rail: tensor

| [INDEX] | [SYMBOL]           | [NAMESPACE]                      | [CAPABILITY]              |
| :-----: | :----------------- | :------------------------------- | :------------------------ |
|  [01]   | `NIndex`           | `System.Buffers`                 | indexes native dimensions |
|  [02]   | `NRange`           | `System.Buffers`                 | slices native dimensions  |
|  [03]   | `TensorShape`      | `System.Numerics.Tensors`        | describes strided layout  |
|  [04]   | `TensorFlags`      | `System.Numerics.Tensors`        | marks tensor layout       |
|  [05]   | `TensorMarshal`    | `System.Runtime.InteropServices` | bridges raw references    |
|  [06]   | `TensorPrimitives` | `System.Numerics.Tensors`        | runs SIMD span operators  |

[PUBLIC_MEMBER_SCOPE]: native indexing and layout
- rail: tensor

| [INDEX] | [OWNER]       | [SURFACE]                | [CAPABILITY]              |
| :-----: | :------------ | :----------------------- | :------------------------ |
|  [01]   | `NIndex`      | `FromStart`              | creates start index       |
|  [02]   | `NIndex`      | `FromEnd`                | creates end index         |
|  [03]   | `NIndex`      | `Start`                  | addresses first element   |
|  [04]   | `NIndex`      | `End`                    | addresses terminal bound  |
|  [05]   | `NIndex`      | `GetOffset`              | resolves dimension offset |
|  [06]   | `NIndex`      | `Index` conversion       | bridges BCL indexing      |
|  [07]   | `NRange`      | `NRange(NIndex, NIndex)` | constructs bounded range  |
|  [08]   | `NRange`      | `StartAt`                | creates open-ended range  |
|  [09]   | `NRange`      | `EndAt`                  | creates prefix range      |
|  [10]   | `NRange`      | `All`                    | spans full dimension      |
|  [11]   | `NRange`      | `GetOffsetAndLength`     | resolves range bounds     |
|  [12]   | `NRange`      | `Range` conversion       | bridges BCL ranges        |
|  [13]   | `TensorShape` | `Rank`                   | reads dimension count     |
|  [14]   | `TensorShape` | `Lengths`                | reads dimension lengths   |
|  [15]   | `TensorShape` | `Strides`                | reads dimension strides   |
|  [16]   | `TensorShape` | `IsDense`                | tests dense layout        |
|  [17]   | `TensorShape` | `HasAnyDenseDimensions`  | tests partial density     |
|  [18]   | `TensorShape` | `FlattenedLength`        | reads flattened length    |
|  [19]   | `TensorFlags` | `None`                   | marks no layout state     |
|  [20]   | `TensorFlags` | `IsDense`                | marks dense layout        |
|  [21]   | `TensorFlags` | `IsPinned`               | marks pinned layout       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: tensor construction and shape
- rail: tensor

| [INDEX] | [SURFACE]                        | [KIND]        | [CAPABILITY]             |
| :-----: | :------------------------------- | :------------ | :----------------------- |
|  [01]   | `Tensor.Create`                  | factory       | wraps array as tensor    |
|  [02]   | `CreateFromShape`                | factory       | allocates shaped tensor  |
|  [03]   | `CreateFromShapeUninitialized`   | factory       | allocates raw tensor     |
|  [04]   | `FillGaussianNormalDistribution` | fill          | fills Gaussian values    |
|  [05]   | `FillUniformDistribution`        | fill          | fills uniform values     |
|  [06]   | `AsTensorSpan`                   | projection    | projects mutable view    |
|  [07]   | `AsReadOnlyTensorSpan`           | projection    | projects read view       |
|  [08]   | `Reshape`                        | shape         | changes dimensions       |
|  [09]   | `Squeeze`                        | shape         | removes unit dimensions  |
|  [10]   | `Unsqueeze`                      | shape         | inserts unit dimension   |
|  [11]   | `PermuteDimensions`              | shape         | reorders dimensions      |
|  [12]   | `Transpose`                      | shape         | swaps dimensions         |
|  [13]   | `Broadcast`                      | shape         | expands into tensor      |
|  [14]   | `BroadcastTo`                    | shape         | expands into destination |
|  [15]   | `Concatenate`                    | composition   | joins tensors            |
|  [16]   | `Stack`                          | composition   | stacks tensors           |
|  [17]   | `Split`                          | composition   | partitions tensor        |
|  [18]   | `SetSlice`                       | update        | writes tensor slice      |
|  [19]   | `FilteredUpdate`                 | update        | writes filtered values   |
|  [20]   | `Tensor<T>.Slice`                | tensor member | slices tensor data       |
|  [21]   | `Tensor<T>.GetDimensionSpan`     | tensor member | addresses dimension      |
|  [22]   | `Tensor<T>.FlattenTo`            | tensor member | flattens to span         |
|  [23]   | `Tensor<T>.ToDenseTensor`        | tensor member | densifies strided data   |
|  [24]   | `Tensor<T>.GetPinnableReference` | tensor member | exposes pinned ref       |

[ENTRYPOINT_SCOPE]: shape-edit and remap operations
- rail: tensor

| [INDEX] | [SURFACE]                | [KIND] | [CAPABILITY]                |
| :-----: | :----------------------- | :----- | :-------------------------- |
|  [01]   | `SqueezeDimension`       | shape  | removes one unit dimension  |
|  [02]   | `StackAlongDimension`    | shape  | stacks on chosen axis       |
|  [03]   | `ConcatenateOnDimension` | shape  | joins on chosen axis        |
|  [04]   | `Reverse`                | remap  | reverses element order      |
|  [05]   | `ReverseDimension`       | remap  | reverses one dimension      |
|  [06]   | `Resize`                 | remap  | allocates resized tensor    |
|  [07]   | `ResizeTo`               | remap  | writes resized destination  |
|  [08]   | `TryBroadcastTo`         | shape  | broadcasts into destination |

`TryBroadcastTo` returns `false` when the destination shape is not broadcast-compatible.

[ENTRYPOINT_SCOPE]: tensor-level comparison and equality
- rail: tensor
- relational families return `Tensor<bool>` masks or write `TensorSpan<bool>` destinations and own `All` and `Any` aggregate companions

| [INDEX] | [SURFACE]            | [FORM]      | [CAPABILITY]              |
| :-----: | :------------------- | :---------- | :------------------------ |
|  [01]   | `Equals`             | mask family | compares equality         |
|  [02]   | `GreaterThan`        | mask family | compares strict greater   |
|  [03]   | `GreaterThanOrEqual` | mask family | compares greater or equal |
|  [04]   | `LessThan`           | mask family | compares strict less      |
|  [05]   | `LessThanOrEqual`    | mask family | compares less or equal    |
|  [06]   | `SequenceEqual`      | aggregate   | tests structural equality |

[ENTRYPOINT_SCOPE]: arithmetic and fused primitive operations
- rail: tensor

| [INDEX] | [SURFACE]              | [KIND]      | [CAPABILITY]            |
| :-----: | :--------------------- | :---------- | :---------------------- |
|  [01]   | `TensorPrimitives.Add` | primitive   | computes elementwise op |
|  [02]   | `Subtract`             | primitive   | computes elementwise op |
|  [03]   | `Multiply`             | primitive   | computes elementwise op |
|  [04]   | `Divide`               | primitive   | computes elementwise op |
|  [05]   | `MultiplyAdd`          | primitive   | computes fused op       |
|  [06]   | `FusedMultiplyAdd`     | primitive   | computes fused op       |
|  [07]   | `Dot`                  | reduction   | computes inner product  |
|  [08]   | `CosineSimilarity`     | reduction   | computes similarity     |
|  [09]   | `Sum`                  | aggregation | reduces values          |

[ENTRYPOINT_SCOPE]: aggregation, activation, transcendental, and conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]           | [KIND]         | [CAPABILITY]            |
| :-----: | :------------------ | :------------- | :---------------------- |
|  [01]   | `Product`           | aggregation    | reduces values          |
|  [02]   | `Max`               | aggregation    | reduces values          |
|  [03]   | `Min`               | aggregation    | reduces values          |
|  [04]   | `IndexOfMax`        | search         | finds extrema index     |
|  [05]   | `SoftMax`           | activation     | normalizes exponentials |
|  [06]   | `Sigmoid`           | activation     | computes logistic map   |
|  [07]   | `Cos`               | transcendental | computes trig op        |
|  [08]   | `Exp`               | transcendental | computes exponential op |
|  [09]   | `Log`               | transcendental | computes logarithm op   |
|  [10]   | `ConvertChecked`    | conversion     | converts values         |
|  [11]   | `ConvertSaturating` | conversion     | converts values         |

[ENTRYPOINT_SCOPE]: elementwise, rounding, and transcendental primitives
- rail: tensor

| [INDEX] | [SURFACE]          | [KIND]         | [CAPABILITY]               |
| :-----: | :----------------- | :------------- | :------------------------- |
|  [01]   | `Negate`           | primitive      | computes elementwise op    |
|  [02]   | `Abs`              | primitive      | computes elementwise op    |
|  [03]   | `CopySign`         | primitive      | transfers sign elementwise |
|  [04]   | `AddMultiply`      | primitive      | computes fused op          |
|  [05]   | `Clamp`            | primitive      | bounds values elementwise  |
|  [06]   | `Round`            | rounding       | rounds values              |
|  [07]   | `Floor`            | rounding       | rounds values down         |
|  [08]   | `Ceiling`          | rounding       | rounds values up           |
|  [09]   | `Sin`              | transcendental | computes trig op           |
|  [10]   | `Tanh`             | transcendental | computes hyperbolic op     |
|  [11]   | `Sqrt`             | transcendental | computes square root       |
|  [12]   | `Cbrt`             | transcendental | computes cube root         |
|  [13]   | `DegreesToRadians` | transcendental | converts angle units       |
|  [14]   | `RadiansToDegrees` | transcendental | converts angle units       |
|  [15]   | `Pow`              | transcendental | computes power op          |
|  [16]   | `Atan2`            | transcendental | computes arctangent op     |

[ENTRYPOINT_SCOPE]: reduction, similarity, bitwise, and conversion primitives
- rail: tensor

| [INDEX] | [SURFACE]              | [KIND]     | [CAPABILITY]                 |
| :-----: | :--------------------- | :--------- | :--------------------------- |
|  [01]   | `Norm`                 | reduction  | computes Euclidean norm      |
|  [02]   | `MaxMagnitude`         | reduction  | reduces by absolute extremum |
|  [03]   | `Average`              | statistics | computes mean                |
|  [04]   | `StdDev`               | statistics | computes standard deviation  |
|  [05]   | `Distance`             | similarity | computes Euclidean distance  |
|  [06]   | `HammingDistance`      | similarity | counts differing elements    |
|  [07]   | `BitwiseAnd`           | bitwise    | computes bitwise op          |
|  [08]   | `ShiftLeft`            | bitwise    | shifts integer values        |
|  [09]   | `ShiftRightArithmetic` | bitwise    | shifts with sign extension   |
|  [10]   | `ConvertTruncating`    | conversion | converts values              |

[ENTRYPOINT_SCOPE]: interpolation, reciprocal, hypot, and half-conversion primitives
- rail: tensor
- `Lerp` accepts span or scalar `amount` operands and writes into the caller destination

| [INDEX] | [SURFACE]                | [KIND]         | [CAPABILITY]                     |
| :-----: | :----------------------- | :------------- | :------------------------------- |
|  [01]   | `Lerp`                   | interpolation  | interpolates elements            |
|  [02]   | `Hypot`                  | primitive      | computes overflow-safe magnitude |
|  [03]   | `Reciprocal`             | primitive      | computes reciprocal              |
|  [04]   | `ReciprocalEstimate`     | primitive      | estimates reciprocal             |
|  [05]   | `ReciprocalSqrt`         | primitive      | computes reciprocal root         |
|  [06]   | `ReciprocalSqrtEstimate` | primitive      | estimates reciprocal root        |
|  [07]   | `RootN`                  | transcendental | computes nth roots               |
|  [08]   | `ScaleB`                 | transcendental | scales by binary exponent        |
|  [09]   | `SinCos`                 | transcendental | writes paired trig outputs       |
|  [10]   | `Ieee754Remainder`       | transcendental | computes IEEE remainder          |
|  [11]   | `ConvertToHalf`          | conversion     | narrows singles to halves        |
|  [12]   | `ConvertToSingle`        | conversion     | widens halves to singles         |
|  [13]   | `ConvertToInteger`       | conversion     | saturates integer overflow       |
|  [14]   | `ConvertToIntegerNative` | conversion     | uses platform overflow behavior  |
|  [15]   | `Truncate`               | rounding       | truncates toward zero            |

[ENTRYPOINT_SCOPE]: bitwise, population, and rotation primitives
- rail: tensor
- `BitwiseOr` and `Xor` accept span or scalar right operands; `PopCount` writes per-element counts or reduces to `long`

| [INDEX] | [SURFACE]           | [KIND]  | [CAPABILITY]             |
| :-----: | :------------------ | :------ | :----------------------- |
|  [01]   | `PopCount`          | bitwise | counts set bits          |
|  [02]   | `LeadingZeroCount`  | bitwise | counts leading zeros     |
|  [03]   | `TrailingZeroCount` | bitwise | counts trailing zeros    |
|  [04]   | `BitwiseOr`         | bitwise | computes bitwise or      |
|  [05]   | `Xor`               | bitwise | computes bitwise xor     |
|  [06]   | `OnesComplement`    | bitwise | complements element bits |
|  [07]   | `ShiftRightLogical` | bitwise | shifts without sign      |
|  [08]   | `RotateLeft`        | bitwise | rotates bits left        |
|  [09]   | `RotateRight`       | bitwise | rotates bits right       |

[ENTRYPOINT_SCOPE]: reduction, search, sign, and similarity primitives
- rail: tensor
- `MinNumber` and `MaxNumber` bind `INumber<T>`; each reduces one span or maps a span/scalar right operand into a destination

| [INDEX] | [SURFACE]                      | [KIND]      | [CAPABILITY]                    |
| :-----: | :----------------------------- | :---------- | :------------------------------ |
|  [01]   | `SumOfSquares`                 | reduction   | sums squared elements           |
|  [02]   | `SumOfMagnitudes`              | reduction   | sums absolute values            |
|  [03]   | `ProductOfSums`                | reduction   | multiplies pairwise sums        |
|  [04]   | `ProductOfDifferences`         | reduction   | multiplies pairwise differences |
|  [05]   | `IndexOfMaxMagnitude`          | search      | finds maximum magnitude         |
|  [06]   | `IndexOfMin`                   | search      | finds minimum value             |
|  [07]   | `IndexOfMinMagnitude`          | search      | finds minimum magnitude         |
|  [08]   | `Sign`                         | projection  | writes element signs            |
|  [09]   | `HammingBitDistance`           | similarity  | counts differing bits           |
|  [10]   | `MinNumber(x)`                 | reduction   | finds NaN-skipping minimum      |
|  [11]   | `MinNumber(x, y, destination)` | elementwise | maps NaN-skipping minimum       |
|  [12]   | `MaxNumber(x)`                 | reduction   | finds NaN-skipping maximum      |
|  [13]   | `MaxNumber(x, y, destination)` | elementwise | maps NaN-skipping maximum       |

`Sign` writes `-1`, `0`, or `1` to a `Span<int>` destination. `HammingBitDistance` returns the `long` bit-difference count across integral spans.

[ENTRYPOINT_SCOPE]: predicate-mask primitives
- rail: tensor
- every predicate writes a `Span<bool>` mask and owns `All` and `Any` aggregate companions

| [INDEX] | [SURFACE]            | [DOMAIN]           |
| :-----: | :------------------- | :----------------- |
|  [01]   | `IsCanonical`        | `INumberBase<T>`   |
|  [02]   | `IsComplexNumber`    | `INumberBase<T>`   |
|  [03]   | `IsEvenInteger`      | `INumberBase<T>`   |
|  [04]   | `IsFinite`           | `INumberBase<T>`   |
|  [05]   | `IsImaginaryNumber`  | `INumberBase<T>`   |
|  [06]   | `IsInfinity`         | `INumberBase<T>`   |
|  [07]   | `IsInteger`          | `INumberBase<T>`   |
|  [08]   | `IsNaN`              | `INumberBase<T>`   |
|  [09]   | `IsNegative`         | `INumberBase<T>`   |
|  [10]   | `IsNegativeInfinity` | `INumberBase<T>`   |
|  [11]   | `IsNormal`           | `INumberBase<T>`   |
|  [12]   | `IsOddInteger`       | `INumberBase<T>`   |
|  [13]   | `IsPositive`         | `INumberBase<T>`   |
|  [14]   | `IsPositiveInfinity` | `INumberBase<T>`   |
|  [15]   | `IsPow2`             | `IBinaryNumber<T>` |
|  [16]   | `IsRealNumber`       | `INumberBase<T>`   |
|  [17]   | `IsSubnormal`        | `INumberBase<T>`   |
|  [18]   | `IsZero`             | `INumberBase<T>`   |
|  [19]   | `IsFiniteAll`        | `INumberBase<T>`   |

`IsFiniteAll<T>(ReadOnlySpan<T>)` returns one `bool` and returns `false` for an empty span.

[ENTRYPOINT_SCOPE]: marshalling operations
- rail: tensor

| [INDEX] | [SURFACE]                                | [KIND]  | [CAPABILITY]          |
| :-----: | :--------------------------------------- | :------ | :-------------------- |
|  [01]   | `TensorMarshal.CreateTensorSpan`         | factory | wraps raw memory      |
|  [02]   | `TensorMarshal.CreateReadOnlyTensorSpan` | factory | wraps raw read memory |
|  [03]   | `TensorMarshal.GetReference`             | ref     | exposes span data ref |

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
- Span feed: Existing coordinate buffers feed `TensorPrimitives` as `ReadOnlySpan<T>`, and `TensorMarshal.CreateTensorSpan` projects pinned raw buffers without copying. The buffer owner retains the only layout.
- Fused rail: Destination aliasing chains vectorized stages in one buffer; fused primitives collapse multi-step arithmetic into one pass, and tensor-level mirrors carry the same operations over strided owners.
- Receipt: A BenchmarkDotNet receipt admits a tensor primitive to a hot path only after measuring its win.
- Predicate boundary: `TensorPrimitives` owns floating SIMD transforms, while exact geometric decisions stay on the exact-arithmetic predicate floor. Crossing and containment signs never route through floating reductions.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: tensors, tensor spans, dimension spans, native-sized indexing, marshalling, and SIMD numeric primitives
- Accept: measured tensor execution composing the kernel's existing span buffers through one fused vectorized rail
- Reject: bespoke tensor wrappers; a package-local numeric loop over a span `TensorPrimitives` already vectorizes; an exact-predicate decision routed through a floating SIMD reduction
