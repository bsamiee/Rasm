# [RASM_API_TENSORS]

`System.Numerics.Tensors` owns two composable numeric planes: `Tensor<T>` and its span views carry `nint`-indexed strided data under one operator algebra, and `TensorPrimitives` folds every elementwise, reduction, and predicate operator over flat caller-owned spans. Both planes write into a caller-supplied destination and allocate nothing, so a fused chain threads one buffer through every stage.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors` (MIT, .NET Foundation)
- assembly: `System.Numerics.Tensors`
- namespaces: `System.Numerics.Tensors`, `System.Buffers`, `System.Runtime.InteropServices`
- abi: strided `nint`-indexed views over caller-owned memory; span and dimension-span types are `ref struct` and `Tensor<T>` the array-backed heap owner
- rail: tensor

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: strided owners, borrowed views, native indexing, and the two operator roots

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]   | [CAPABILITY]                              |
| :-----: | :------------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `Tensor`                         | static class    | strided operator algebra and shape edits  |
|  [02]   | `Tensor<T>`                      | sealed class    | array-backed strided heap owner           |
|  [03]   | `TensorSpan<T>`                  | ref struct      | mutable borrowed strided window           |
|  [04]   | `ReadOnlyTensorSpan<T>`          | ref struct      | read-only borrowed strided window         |
|  [05]   | `TensorDimensionSpan<T>`         | ref struct      | one rank walked as mutable sub-spans      |
|  [06]   | `ReadOnlyTensorDimensionSpan<T>` | ref struct      | one rank walked as read-only sub-spans    |
|  [07]   | `ITensor<TSelf,T>`               | interface       | static-abstract construction and mutation |
|  [08]   | `IReadOnlyTensor<TSelf,T>`       | interface       | typed read conformance over `TSelf`       |
|  [09]   | `ITensor`                        | interface       | untyped mutation facade                   |
|  [10]   | `IReadOnlyTensor`                | interface       | untyped rank and layout facade            |
|  [11]   | `TensorPrimitives`               | static class    | vectorized operators over flat spans      |
|  [12]   | `NIndex`                         | readonly struct | native-sized index with end-relative form |
|  [13]   | `NRange`                         | readonly struct | native-sized half-open range              |
|  [14]   | `TensorMarshal`                  | static class    | raw reference to strided view bridge      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Tensor` construction and `TensorMarshal` raw-memory admission
- Each `TensorMarshal` factory takes `(ref T data, nint dataLength, ReadOnlySpan<nint> lengths, ReadOnlySpan<nint> strides, bool pinned)` over memory the caller already owns.

| [INDEX] | [SURFACE]                                                                           | [SHAPE] | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `Tensor.Create(T[], int, ReadOnlySpan<nint>, ReadOnlySpan<nint>) -> Tensor<T>`      | factory | wrap an array at an offset        |
|  [02]   | `Tensor.CreateFromShape(ReadOnlySpan<nint>, ReadOnlySpan<nint>, bool)`              | factory | allocate cleared under a pin flag |
|  [03]   | `Tensor.CreateFromShapeUninitialized(ReadOnlySpan<nint>, ReadOnlySpan<nint>, bool)` | factory | allocate without clearing         |
|  [04]   | `Tensor.AsTensorSpan(T[], ReadOnlySpan<nint>, ReadOnlySpan<nint>)`                  | static  | borrow an array mutably           |
|  [05]   | `Tensor.AsReadOnlyTensorSpan(T[], ReadOnlySpan<nint>, ReadOnlySpan<nint>)`          | static  | borrow an array read-only         |
|  [06]   | `Tensor.FillUniformDistribution(TensorSpan<T>, Random)`                             | static  | fill uniform samples in place     |
|  [07]   | `Tensor.FillGaussianNormalDistribution(TensorSpan<T>, Random)`                      | static  | fill normal samples in place      |
|  [08]   | `TensorMarshal.CreateTensorSpan -> TensorSpan<T>`                                   | factory | wrap caller memory mutably        |
|  [09]   | `TensorMarshal.CreateReadOnlyTensorSpan -> ReadOnlyTensorSpan<T>`                   | factory | wrap caller memory read-only      |
|  [10]   | `TensorMarshal.GetReference(TensorSpan<T>) -> ref T`                                | static  | recover the backing reference     |

[ENTRYPOINT_SCOPE]: `Tensor` shape, remap, and composition
- Each op mints a `Tensor<T>`; `Reshape`, `Squeeze`, `SqueezeDimension`, and `Unsqueeze` also re-window a span view with no allocation, and the composition family mirrors a `ref readonly TensorSpan<T>` destination overload.
- A bare form acts on the default axis where its axis twin takes the `int` dimension — [AXIS_TWIN]: `SqueezeDimension` `ReverseDimension` `ConcatenateOnDimension` `StackAlongDimension`.

| [INDEX] | [SURFACE]                                                              | [SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `Reshape(Tensor<T>, ReadOnlySpan<nint>) -> Tensor<T>`                  | static  | re-length under one flattened count |
|  [02]   | `Squeeze(Tensor<T>) -> Tensor<T>`                                      | static  | drop every unit dimension           |
|  [03]   | `Unsqueeze(Tensor<T>, int) -> Tensor<T>`                               | static  | insert a unit dimension             |
|  [04]   | `PermuteDimensions(Tensor<T>, ReadOnlySpan<int>) -> Tensor<T>`         | static  | reorder ranks by index vector       |
|  [05]   | `Transpose(Tensor<T>) -> Tensor<T>`                                    | static  | swap the final two dimensions       |
|  [06]   | `Broadcast(ReadOnlyTensorSpan<T>, ReadOnlySpan<nint>)`                 | static  | expand to a broadcast shape         |
|  [07]   | `BroadcastTo(ReadOnlyTensorSpan<T>, TensorSpan<T>)`                    | static  | expand into a caller destination    |
|  [08]   | `TryBroadcastTo(ReadOnlyTensorSpan<T>, TensorSpan<T>) -> bool`         | static  | probe destination shape fit         |
|  [09]   | `Resize(Tensor<T>, ReadOnlySpan<nint>) -> Tensor<T>`                   | static  | re-allocate at a new element count  |
|  [10]   | `ResizeTo(ReadOnlyTensorSpan<T>, TensorSpan<T>)`                       | static  | truncate or zero-pad into a window  |
|  [11]   | `Reverse(ReadOnlyTensorSpan<T>) -> Tensor<T>`                          | static  | reorder elements at one shape       |
|  [12]   | `Concatenate(ReadOnlySpan<Tensor<T>>) -> Tensor<T>`                    | static  | join along an existing axis         |
|  [13]   | `Stack(ReadOnlySpan<Tensor<T>>) -> Tensor<T>`                          | static  | stack into a new leading axis       |
|  [14]   | `Split(ReadOnlyTensorSpan<T>, int, nint) -> Tensor<T>[]`               | static  | partition one axis evenly           |
|  [15]   | `SetSlice(TensorSpan<T>, ReadOnlyTensorSpan<T>, ReadOnlySpan<NRange>)` | static  | write values into a ranged window   |
|  [16]   | `FilteredUpdate(TensorSpan<T>, ReadOnlyTensorSpan<bool>, T)`           | static  | write where the mask holds          |

[ENTRYPOINT_SCOPE]: `Tensor<T>` and span-view members
- `Tensor<T>`, `TensorSpan<T>`, and `ReadOnlyTensorSpan<T>` carry one member set through `ITensor<TSelf,T>` and `IReadOnlyTensor<TSelf,T>`, so an algorithm generic over `TSelf` binds the heap owner and either borrowed window; the mutating half rides `ITensor<TSelf,T>` alone.
- [LAYOUT]: `Rank` `Lengths` `Strides` `FlattenedLength` `IsDense` `HasAnyDenseDimensions` `IsEmpty` `IsPinned`
- Implicit conversions lift `T[]` to `Tensor<T>` and `TensorSpan<T>`, widen `Tensor<T>` to both span views, and narrow `TensorSpan<T>` to `ReadOnlyTensorSpan<T>`.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `this[ReadOnlySpan<nint>] -> ref T`                        | property | address one element by native index |
|  [02]   | `this[ReadOnlySpan<NIndex>] -> ref T`                      | property | address one element end-relative    |
|  [03]   | `this[ReadOnlySpan<NRange>] -> TSelf`                      | property | window a sub-tensor by ranges       |
|  [04]   | `Slice(ReadOnlySpan<NRange>) -> TSelf`                     | instance | window a sub-tensor                 |
|  [05]   | `GetDimensionSpan(int) -> TensorDimensionSpan<T>`          | instance | walk one dimension as sub-spans     |
|  [06]   | `GetSpan(ReadOnlySpan<nint>, int) -> Span<T>`              | instance | flat span over a contiguous run     |
|  [07]   | `TryGetSpan(ReadOnlySpan<nint>, int, out Span<T>) -> bool` | instance | probe contiguity before reading     |
|  [08]   | `FlattenTo(Span<T>)`                                       | instance | copy strided data flattened         |
|  [09]   | `TryFlattenTo(Span<T>) -> bool`                            | instance | flatten when the destination fits   |
|  [10]   | `CopyTo(TensorSpan<T>)`                                    | instance | copy into a shaped destination      |
|  [11]   | `TryCopyTo(TensorSpan<T>) -> bool`                         | instance | copy when the shapes match          |
|  [12]   | `ToDenseTensor() -> TSelf`                                 | instance | materialize dense from strided      |
|  [13]   | `Fill(T)`                                                  | instance | write one value across the window   |
|  [14]   | `Clear()`                                                  | instance | zero the window                     |
|  [15]   | `GetPinnableReference() -> ref T`                          | instance | pin the backing storage             |
|  [16]   | `Tensor<T>.GetPinnedHandle() -> MemoryHandle`              | instance | hold a pin across native calls      |
|  [17]   | `Tensor<T>.GetEnumerator() -> Enumerator`                  | instance | walk elements in flattened order    |

[ENTRYPOINT_SCOPE]: `Tensor` comparison and mask families
- Each relational op takes `(ReadOnlyTensorSpan<T> x, ReadOnlyTensorSpan<T>|T y, TensorSpan<bool> destination)`, returns the mask by `ref readonly`, and mints `<Name>All` and `<Name>Any` `bool` reducers over the same operands.
- [RELATIONAL]: `Equals` `GreaterThan` `GreaterThanOrEqual` `LessThan` `LessThanOrEqual`
- `SequenceEqual(ReadOnlyTensorSpan<T>, ReadOnlyTensorSpan<T>) -> bool` compares shape and elements in one call.

[ENTRYPOINT_SCOPE]: `TensorPrimitives` unary elementwise operators
- Each operator takes `(ReadOnlySpan<T> x, Span<T> destination)` under the generic-math constraint its family names, and `destination` may alias `x` for an in-place pass.
- [TRIGONOMETRY]: `Sin` `Cos` `Tan` `SinPi` `CosPi` `TanPi` `Asin` `Acos` `Atan` `AsinPi` `AcosPi` `AtanPi` `Sinh` `Cosh` `Tanh` `Asinh` `Acosh` `Atanh` `DegreesToRadians` `RadiansToDegrees`
- [EXPONENTIAL]: `Exp` `Exp2` `Exp10` `ExpM1` `Exp2M1` `Exp10M1` `Log` `Log2` `Log10` `LogP1` `Log2P1` `Log10P1` `Sqrt` `Cbrt` `Reciprocal` `ReciprocalEstimate` `ReciprocalSqrt` `ReciprocalSqrtEstimate` `Sigmoid` `SoftMax`
- [ARITHMETIC]: `Abs` `Negate` `Increment` `Decrement` `Round` `Floor` `Ceiling` `Truncate` `BitIncrement` `BitDecrement`
- [BITWISE]: `OnesComplement` `PopCount` `LeadingZeroCount` `TrailingZeroCount`

[ENTRYPOINT_SCOPE]: `TensorPrimitives` binary, ternary, and shaped operators
- A binary operator takes `(ReadOnlySpan<T> x, ReadOnlySpan<T>|T y, Span<T> destination)` and a ternary operator admits a span or a scalar in every operand slot.
- [BINARY]: `Add` `Subtract` `Multiply` `Divide` `Remainder` `Pow` `Log` `Atan2` `Atan2Pi` `Ieee754Remainder` `CopySign` `Max` `Min` `MaxNumber` `MinNumber` `MaxMagnitude` `MinMagnitude` `MaxMagnitudeNumber` `MinMagnitudeNumber` `BitwiseAnd` `BitwiseOr` `Xor`
- [TERNARY]: `MultiplyAdd` `MultiplyAddEstimate` `FusedMultiplyAdd` `AddMultiply` `Lerp` `Clamp`
- An integer-parameter operator takes `(ReadOnlySpan<T> x, int, Span<T> destination)` — [INT_PARAM]: `RootN` `ScaleB` `ShiftLeft` `ShiftRightArithmetic` `ShiftRightLogical` `RotateLeft` `RotateRight`; `ShiftRightArithmetic` extends the sign bit where `ShiftRightLogical` fills with zero.

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `Hypot(ReadOnlySpan<T>, ReadOnlySpan<T>, Span<T>)`       | static  | overflow-safe magnitude of two spans  |
|  [02]   | `DivRem(ReadOnlySpan<T>, T, Span<T>, Span<T>)`           | static  | write quotient and remainder together |
|  [03]   | `SinCos(ReadOnlySpan<T>, Span<T>, Span<T>)`              | static  | write sine and cosine together        |
|  [04]   | `SinCosPi(ReadOnlySpan<T>, Span<T>, Span<T>)`            | static  | write the pi-scaled trig pair         |
|  [05]   | `Round(ReadOnlySpan<T>, int, MidpointRounding, Span<T>)` | static  | round at digits under a mode          |
|  [06]   | `Sign(ReadOnlySpan<T>, Span<int>)`                       | static  | write the per-element sign            |
|  [07]   | `ILogB(ReadOnlySpan<T>, Span<int>)`                      | static  | write the binary exponent             |

[ENTRYPOINT_SCOPE]: `TensorPrimitives` reductions and searches
- A reduction returns a scalar over caller-owned spans: the single-span family takes `(ReadOnlySpan<T>) -> T`, the paired family `(ReadOnlySpan<T>, ReadOnlySpan<T>) -> T`, and the search family `(ReadOnlySpan<T>) -> int`.
- [SPAN_REDUCE]: `Sum` `SumOfSquares` `SumOfMagnitudes` `Product` `Average` `StdDev` `Norm` `Max` `Min` `MaxNumber` `MinNumber` `MaxMagnitude` `MinMagnitude` `MaxMagnitudeNumber` `MinMagnitudeNumber`
- [PAIR_REDUCE]: `Dot` `CosineSimilarity` `Distance` `ProductOfSums` `ProductOfDifferences`
- [INDEX_SEARCH]: `IndexOfMax` `IndexOfMin` `IndexOfMaxMagnitude` `IndexOfMinMagnitude`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `PopCount(ReadOnlySpan<T>) -> long`                            | static  | total set bits across the span |
|  [02]   | `HammingDistance(ReadOnlySpan<T>, ReadOnlySpan<T>) -> int`     | static  | count differing elements       |
|  [03]   | `HammingBitDistance(ReadOnlySpan<T>, ReadOnlySpan<T>) -> long` | static  | count differing bits           |

[ENTRYPOINT_SCOPE]: `TensorPrimitives` predicate masks
- Each predicate writes `(ReadOnlySpan<T>, Span<bool> destination)` and mints `<Name>All` and `<Name>Any` `(ReadOnlySpan<T>) -> bool` reducers; an empty span answers `false` from both reducers.
- [PREDICATE]: `IsCanonical` `IsComplexNumber` `IsEvenInteger` `IsFinite` `IsImaginaryNumber` `IsInfinity` `IsInteger` `IsNaN` `IsNegative` `IsNegativeInfinity` `IsNormal` `IsOddInteger` `IsPositive` `IsPositiveInfinity` `IsRealNumber` `IsSubnormal` `IsZero`
- `IsPow2` binds `IBinaryNumber<T>` where every other predicate binds `INumberBase<T>`.

[ENTRYPOINT_SCOPE]: `TensorPrimitives` element-type conversion

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------ | :------ | :----------------------------- |
|  [01]   | `ConvertChecked<TFrom,TTo>(ReadOnlySpan<TFrom>, Span<TTo>)`         | static  | convert and raise on overflow  |
|  [02]   | `ConvertSaturating<TFrom,TTo>(ReadOnlySpan<TFrom>, Span<TTo>)`      | static  | clamp overflow to the bound    |
|  [03]   | `ConvertTruncating<TFrom,TTo>(ReadOnlySpan<TFrom>, Span<TTo>)`      | static  | drop high bits on overflow     |
|  [04]   | `ConvertToInteger<TFrom,TTo>(ReadOnlySpan<TFrom>, Span<TTo>)`       | static  | saturate float to integer      |
|  [05]   | `ConvertToIntegerNative<TFrom,TTo>(ReadOnlySpan<TFrom>, Span<TTo>)` | static  | use platform overflow behavior |
|  [06]   | `ConvertToHalf(ReadOnlySpan<float>, Span<Half>)`                    | static  | narrow singles to halves       |
|  [07]   | `ConvertToSingle(ReadOnlySpan<Half>, Span<float>)`                  | static  | widen halves to singles        |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Each `TensorPrimitives` operator lowers to the widest `System.Runtime.Intrinsics` ISA the target carries and falls back to a scalar loop, so the operator is correct on every target and vectorized wherever the hardware admits.
- An elementwise operator writes into a caller-owned destination that may alias its input, so a fused chain reuses one buffer across every stage; a reduction returns a scalar and a predicate family writes `Span<bool>`.
- Generic-math constraints select the admitted element type per family — `INumberBase<T>`, `IFloatingPointIeee754<T>`, `IRootFunctions<T>`, `IBinaryInteger<T>`, `IBitwiseOperators<T,T,T>`, `IShiftOperators<T,int,T>` — so an integer-only family rejects a floating element at the constraint.
- `Tensor<T>` and both span views conform to `ITensor<TSelf,T>`, so one algorithm generic over `TSelf` binds the heap owner and either borrowed window without an overload family.
- `Tensor` carries the arithmetic, bitwise, and shift operator set (`+` `-` `*` `/` `&` `|` `^` `~` `<<` `>>` `>>>`) as C# extension blocks over `Tensor<T>`, `TensorSpan<T>`, and `ReadOnlyTensorSpan<T>`, each operator constrained on its matching generic-math interface and minting a fresh `Tensor<T>`.
- `GetDimensionSpan(int)` walks one rank as `TensorSpan<T>` slices, so a rank reduction chains over the same buffer with no intermediate tensor.
- Vector normalization folds `Norm` into `Divide` against the reduced magnitude over one destination span.

[STACKING]:
- `MathNet.Numerics`(`.api/api-mathnet-numerics.md`): the split `double[] real, double[] imaginary` spectral buffers and the `Generate`/`Window` axes enter as `ReadOnlySpan<double>`, so magnitude, phase, and taper passes vectorize with no `Complex` marshalling.
- `CSparse`(`.api/api-csparse.md`): the `Span<double>` a GEMV or `ISolver<double>.Solve` writes is the operand of the residual, axpy, and `Norm` passes, so a solve loop measures convergence with no copy.
- `CommunityToolkit.HighPerformance`(`.api/api-highperformance.md`): `MemoryOwner<T>.Span` feeds a primitive directly and `MemoryOwner<T>.DangerousGetReference()` feeds `TensorMarshal.CreateTensorSpan`, so one pooled rental backs the whole vectorized chain.
- `NetTopologySuite`(`.api/api-nettopologysuite.md`): crossing and containment signs resolve on its robust predicate floor, and this rail carries only the metric and transform passes downstream of that decision.
- Within-library fold: `CreateFromShapeUninitialized` mints the destination, the extension operator set composes the expression, `GetDimensionSpan` walks the reduced rank, and `Tensor<T>.GetPinnedHandle` holds the buffer across a native call — one allocation spanning the whole pipeline.

[LOCAL_ADMISSION]:
- Compute tensor lanes admit these shapes and primitives as first-class execution material, and a consuming model or vector rail takes the span without re-declaring tensor ownership.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: strided tensor owners and views, native-sized indexing and ranges, raw-memory marshalling, and the vectorized span operator set
- Accept: a fused vectorized chain over caller-owned spans whose measured BenchmarkDotNet receipt beats the scalar baseline on the hot lane
- Reject: a package-local numeric loop over a span this surface already vectorizes, a bespoke tensor wrapper, and an exact-predicate decision routed through a floating reduction
