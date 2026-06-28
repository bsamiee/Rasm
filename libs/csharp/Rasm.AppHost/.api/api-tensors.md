# [RASM_APPHOST_API_TENSORS]

`System.Numerics.Tensors` is the BCL hardware-accelerated numeric substrate: a single
generic-math primitive surface (`TensorPrimitives`, ~173 operation families over
`ReadOnlySpan<T>`/`Span<T>`) plus the dense N-dimensional tensor algebra (`Tensor<T>` owned
buffer, `TensorSpan<T>`/`ReadOnlyTensorSpan<T>` strided views, the static `Tensor` shape
algebra) indexed by the native-width `NIndex`/`NRange` value types. Every elementwise,
reduction, similarity, activation, and predicate kernel dispatches through SIMD
(`Vector512`/`Vector256`/`Vector128`, AVX-512/AVX2/NEON) and is constrained by the
generic-math interfaces (`INumberBase<T>`, `IRootFunctions<T>`, `IExponentialFunctions<T>`,
`IBinaryInteger<T>`), so one `<T>` method spans `float`/`double`/`Half`/`int`/… with no
per-type kernel. The AppHost reasoning owner composes `TensorPrimitives.CosineSimilarity`
over `Embedding<float>.Vector` spans to rank capability descriptors by intent — the BCL
primitive replaces any hand-rolled dot-product loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Numerics.Tensors`
- package: `System.Numerics.Tensors`
- version: `10.0.9`
- license: MIT
- assembly: `System.Numerics.Tensors`
- namespace: `System.Numerics.Tensors` (the engine + tensor types), `System.Buffers` (`NIndex`/`NRange` native-width indexing)
- asset: multi-TFM (`net10.0`, `net9.0`, `net8.0`, `net462`); the `net10.0` consumer binds `lib/net10.0` — the stable `Tensor<T>`/`TensorSpan<T>` surface ships in the `net9.0`+ assets, not the down-level fallback
- transitive-floor: `Microsoft.Bcl.Numerics`, `System.Memory`, `System.Numerics.Vectors` (inbox on `net10.0`; the package declares them for down-level TFMs)
- acceleration: SIMD via `System.Runtime.Intrinsics` (`Vector512<T>`/`Vector256<T>`/`Vector128<T>`); kernels degrade to scalar when intrinsics are unavailable
- generic-math: operations are constrained by `System.Numerics` interfaces, not bound to a fixed element type
- rail: numeric-substrate

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the primitive and algebra owners
- namespace: `System.Numerics.Tensors`
- rail: numeric-substrate

| [INDEX] | [SYMBOL]                  | [RAIL]            | [CAPABILITY]                                                                                                          |
| :-----: | :------------------------ | :---------------- | :------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `TensorPrimitives`        | numeric-substrate | `static` SIMD generic-math over `ReadOnlySpan<T>`/`Span<T>` — ~173 operation families; the span-level numeric engine |
|  [02]   | `Tensor`                  | numeric-substrate | `static` N-d shape+algebra: `Create`/reshape/permute/concatenate/stack/split/broadcast/fill + reduction mirrors      |
|  [03]   | `Tensor<T>`               | numeric-substrate | `sealed` owned dense tensor (heap buffer + shape) — `ITensor<Tensor<T>,T>`, `IEnumerable<T>`; the carrier you hold   |
|  [04]   | `TensorSpan<T>`           | numeric-substrate | `readonly ref struct` mutable strided view over contiguous memory — `ITensor<TensorSpan<T>,T>`; the zero-copy lens   |
|  [05]   | `ReadOnlyTensorSpan<T>`   | numeric-substrate | `readonly ref struct` read-only strided view — `IReadOnlyTensor<…>`; the immutable lens                              |
|  [06]   | `TensorDimensionSpan<T>`  | numeric-substrate | `readonly ref struct` enumerator over the sub-tensors of one dimension (`Tensor<T>.GetDimensionSpan(int)`)           |

[PUBLIC_TYPE_SCOPE]: contracts and native-width indexing
- namespace: `System.Numerics.Tensors` (interfaces), `System.Buffers` (`NIndex`/`NRange`)
- rail: numeric-substrate

| [INDEX] | [SYMBOL]                     | [RAIL]            | [CAPABILITY]                                                                                       |
| :-----: | :--------------------------- | :---------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `IReadOnlyTensor`            | numeric-substrate | non-generic read root: `Rank`, `Lengths`/`Strides` (`ReadOnlySpan<nint>`), `FlattenedLength`, `IsDense`/`IsEmpty`/`IsPinned` |
|  [02]   | `IReadOnlyTensor<TSelf,T>`   | numeric-substrate | typed read contract — self-referential generic for `Tensor<T>`/`ReadOnlyTensorSpan<T>`/`TensorSpan<T>` |
|  [03]   | `ITensor`                    | numeric-substrate | non-generic mutable root (`: IReadOnlyTensor`) — `Clear`/`Fill`, mutability/pinned flags          |
|  [04]   | `ITensor<TSelf,T>`           | numeric-substrate | typed mutable contract; the constraint surface generic algorithms target instead of a concrete tensor |
|  [05]   | `System.Buffers.NIndex`      | numeric-substrate | `readonly struct` native-width index (the `nint` analogue of `System.Index`, from-end aware): `Value`, `IsFromEnd`, `Start`/`End`, `FromStart(nint)`/`FromEnd(nint)`, `GetOffset(nint)`, `ToIndex()` |
|  [06]   | `System.Buffers.NRange`      | numeric-substrate | `readonly struct` native-width range (`nint` analogue of `System.Range`): `Start`/`End` (`NIndex`), `All`/`StartAt`/`EndAt`, `GetOffsetAndLength(nint) → (nint Offset, nint Length)` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `TensorPrimitives` — similarity, distance, reduction
- namespace: `System.Numerics.Tensors`
- rail: numeric-substrate

Every span kernel has a `float` fast path and a generic `<T>` mirror; reductions return the
scalar, elementwise kernels write into a caller-owned `destination` span (in-place when
`destination` aliases an input). These are the AppHost reasoning rail's similarity engine.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                          | [CAPABILITY]                                              |
| :-----: | :----------------------- | :----------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `CosineSimilarity`       | `(ReadOnlySpan<float>, ReadOnlySpan<float>) → float` · `<T>(…) where T:IRootFunctions<T> → T` | cosine of the angle — the embedding-rank metric          |
|  [02]   | `Distance`               | `(ReadOnlySpan<float>, ReadOnlySpan<float>) → float` · `<T> where T:IRootFunctions<T>` | Euclidean (L2) distance between two vectors              |
|  [03]   | `Dot`                    | `(ReadOnlySpan<float>, ReadOnlySpan<float>) → float` · `<T> where T:IMultiplyOperators…,IAdditionOperators…` | inner product                                            |
|  [04]   | `Norm`                   | `<T>(ReadOnlySpan<T>) where T:IRootFunctions<T> → T`                                  | Euclidean (L2) magnitude                                 |
|  [05]   | `HammingDistance` / `HammingBitDistance` | `<T>(ReadOnlySpan<T>, ReadOnlySpan<T>) → int` · `<T> where T:IBinaryInteger<T> → long` | positional / bit-level Hamming distance (binary/quantized vectors) |
|  [06]   | `Sum` / `SumOfSquares` / `SumOfMagnitudes` | `<T>(ReadOnlySpan<T>) → T`                                          | additive reductions                                      |
|  [07]   | `Product` / `Average` / `StdDev` | `<T>(ReadOnlySpan<T>) → T`                                                    | multiplicative / mean / standard-deviation reductions    |
|  [08]   | `Max` / `Min` / `MaxMagnitude` / `MinMagnitude` (+`Number` variants) | `<T>(ReadOnlySpan<T>) → T`                            | extremum reductions (IEEE `…Number` ignores `NaN`)       |
|  [09]   | `IndexOfMax` / `IndexOfMin` (+`Magnitude` variants) | `<T>(ReadOnlySpan<T>) → int`                                | argmax / argmin                                          |

[ENTRYPOINT_SCOPE]: `TensorPrimitives` — elementwise, activation, predicate
- namespace: `System.Numerics.Tensors`
- rail: numeric-substrate

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                          | [CAPABILITY]                                              |
| :-----: | :----------------------- | :----------------------------------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `Add`/`Subtract`/`Multiply`/`Divide` | `<T>(ReadOnlySpan<T> x, ReadOnlySpan<T>\|T y, Span<T> destination)`           | elementwise arithmetic (tensor·tensor or tensor·scalar)  |
|  [02]   | `MultiplyAdd`/`AddMultiply`/`FusedMultiplyAdd`/`MultiplyAddEstimate`/`Lerp` | `<T>(…, Span<T> destination)`           | fused 3-operand kernels                                  |
|  [03]   | `Abs`/`Negate`/`Sqrt`/`Cbrt`/`RootN`/`Reciprocal`/`ReciprocalSqrt`(+`Estimate`) | `<T>(ReadOnlySpan<T>, Span<T>)`    | elementwise unary math                                   |
|  [04]   | `Exp`/`Exp2`/`Exp10`/`Log`/`Log2`/`Log10`/`Pow`/`ILogB` (+`M1`/`P1`) | `<T>(…, Span<T>)`                                   | exponential / logarithmic family                         |
|  [05]   | `Sin`/`Cos`/`Tan`/`SinCos`/`Asin`/`Acos`/`Atan`/`Atan2`/`Sinh`/`Cosh`/`Tanh`/`Hypot` (+`Pi` variants) | `<T>(…, Span<T>)`     | trigonometric / hyperbolic family                        |
|  [06]   | `Sigmoid` / `SoftMax` / `Tanh` | `<T>(ReadOnlySpan<T> x, Span<T> destination) where T:IExponentialFunctions<T>` | ML activation kernels                                    |
|  [07]   | `Clamp`/`CopySign`/`Round`/`Floor`/`Ceiling`/`Truncate`/`ScaleB` | `<T>(…, Span<T>)`                              | rounding / sign / clamp                                  |
|  [08]   | `BitwiseAnd`/`BitwiseOr`/`Xor`/`OnesComplement`/`ShiftLeft`/`ShiftRightArithmetic`/`ShiftRightLogical`/`RotateLeft`/`RotateRight`/`PopCount`/`LeadingZeroCount`/`TrailingZeroCount` | `<T>(…) where T:IBinaryInteger<T>` | integer/bitwise kernels      |
|  [09]   | `ConvertChecked`/`ConvertSaturating`/`ConvertTruncating`/`ConvertToHalf`/`ConvertToSingle`/`ConvertToInteger`(`Native`) | `<TFrom,TTo>(ReadOnlySpan<TFrom>, Span<TTo>)` | element-type conversion |
|  [10]   | `Is…`/`Is…All`/`Is…Any` (`IsNaN`,`IsFinite`,`IsInfinity`,`IsInteger`,`IsNegative`,`IsNormal`,`IsZero`,`IsPow2`,…) | `<T>(ReadOnlySpan<T>, Span<bool>) → void` · `<T>(ReadOnlySpan<T>) → bool` | predicate-tensor / quantifier folds |

[ENTRYPOINT_SCOPE]: `Tensor` static — N-d shape algebra
- namespace: `System.Numerics.Tensors`
- rail: numeric-substrate

Shape transforms return either an owned `Tensor<T>` (allocating) or a `TensorSpan<T>`/
`ReadOnlyTensorSpan<T>` re-view over the same buffer (`ref readonly`/strided, no copy).

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                    | [CAPABILITY]                                          |
| :-----: | :----------------------------------------- | :----------------------------------------------------------------------------- | :--------------------------------------------------- |
|  [01]   | `Create` / `CreateFromShape` / `CreateFromShapeUninitialized` | `<T>(T[] array, …ReadOnlySpan<nint> lengths/strides) → Tensor<T>` · `<T>(ReadOnlySpan<nint> lengths, bool pinned=false) → Tensor<T>` | `Create` wraps an existing `T[]` with a shape; `CreateFromShape*` allocate an owned buffer from a shape |
|  [02]   | `Reshape` / `Squeeze` / `SqueezeDimension` / `Unsqueeze` | `<T>(in tensor, ReadOnlySpan<nint>) → Tensor<T>\|TensorSpan<T>`    | rank/shape re-view                                    |
|  [03]   | `PermuteDimensions` / `Transpose`          | `<T>(in tensor, …) → Tensor<T>`                                                 | axis reorder                                          |
|  [04]   | `Concatenate` / `ConcatenateOnDimension` / `Stack` / `StackAlongDimension` / `Split` | `<T>(…) → Tensor<T>\|Tensor<T>[]`                | join / stack / split                                 |
|  [05]   | `Reverse` / `ReverseDimension` / `SetSlice` | `<T>(…) → Tensor<T>\|ref readonly TensorSpan<T>`                               | reverse / region assignment                          |
|  [06]   | `BroadcastTo` / `TryBroadcastTo` / `ResizeTo` | `<T>(in tensor, ReadOnlySpan<nint> shape) → Tensor<T>\|TensorSpan<T>\|bool`  | broadcast / resize                                   |
|  [07]   | `FillUniformDistribution` / `FillGaussianNormalDistribution` | `<T>(in TensorSpan<T>, Random?) → ref readonly TensorSpan<T>` | randomized fill                                      |
|  [08]   | reduction/compare mirrors                  | `Sum`/`Norm`/`Average`/`StdDev`/`Max`/`Min`/`Dot`/`CosineSimilarity`/`Distance`/`IndexOf*`/`EqualsAll`/`EqualsAny`/`GreaterThan*`/`LessThan*`/`SequenceEqual` | the `TensorPrimitives` algebra lifted to `Tensor<T>`/spans |

[ENTRYPOINT_SCOPE]: `Tensor<T>` — indexing, views, lifetime
- namespace: `System.Numerics.Tensors`
- rail: numeric-substrate

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                                                                  | [CAPABILITY]                                               |
| :-----: | :------------------------------- | :--------------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `this[ReadOnlySpan<nint>\|NIndex]` | `→ ref T`                                                                    | scalar element by multi-dim index                         |
|  [02]   | `this[ReadOnlySpan<NRange>]`     | `→ Tensor<T>`                                                                | sub-tensor by range                                       |
|  [03]   | `AsTensorSpan` / `AsReadOnlyTensorSpan` | `(…nint\|NIndex\|NRange…) → TensorSpan<T>\|ReadOnlyTensorSpan<T>`     | zero-copy strided view (optionally sliced)                |
|  [04]   | `Slice` / `GetDimensionSpan` / `GetSpan` | `(…) → Tensor<T>\|TensorDimensionSpan<T>\|Span<T>`                    | slice / per-dimension enumeration / contiguous run        |
|  [05]   | `CopyTo` / `FlattenTo` / `Fill` / `Clear` | `(…)`                                                                  | bulk move / row-major flatten / fill / zero               |
|  [06]   | `GetPinnableReference` / `GetPinnedHandle` / `IsPinned` | `() → ref T \| MemoryHandle \| bool`                          | `fixed`/pinning interop boundary                          |
|  [07]   | `Rank`/`Lengths`/`Strides`/`FlattenedLength`/`IsDense`/`IsEmpty`/`Empty` | `→ int\|ReadOnlySpan<nint>\|nint\|bool`                 | shape introspection                                       |
|  [08]   | implicit operators / `GetEnumerator`   | `T[] → Tensor<T>` · `Tensor<T> → TensorSpan<T>\|ReadOnlyTensorSpan<T>` · `→ Enumerator` | array adoption, view conversion, row-major iteration |

## [04]-[IMPLEMENTATION_LAW]

[ACCELERATION_LAW]:
- the kernels are auto-vectorized: a span is processed in `Vector512`/`Vector256`/`Vector128` blocks with a scalar tail; there is no consumer knob — the widest available ISA wins at JIT time. Hand-rolled SIMD or a manual reduction loop is the deleted form.
- the `…Estimate` variants (`ReciprocalEstimate`, `ReciprocalSqrtEstimate`, `MultiplyAddEstimate`) trade last-bit accuracy for throughput; choose them only where the downstream tolerance admits it.

[GENERIC_MATH_LAW]:
- one `<T>` method covers every element type its constraint admits: `IRootFunctions<T>` (`CosineSimilarity`/`Distance`/`Norm`/`Sqrt`), `IExponentialFunctions<T>` (`Exp`/`SoftMax`/`Sigmoid`), `IBinaryInteger<T>` (bitwise/`HammingBitDistance`), `INumberBase<T>` (`Abs`/`Add`/…). Do not branch on `float`/`double` — pass the element type as `<T>`.
- the `float`/`double` non-generic overloads are convenience fast paths over the identical kernel; the `float` `CosineSimilarity` is what the embedding rail calls because `Embedding<float>.Vector` is `ReadOnlyMemory<float>`.

[SPAN_CONTRACT]:
- inputs are `ReadOnlySpan<T>`; elementwise results write into a caller-owned `Span<T> destination` that may alias an input for in-place transforms; binary kernels require equal lengths (or a scalar second operand) and `destination.Length >= x.Length`, else `ArgumentException`.
- the tensor views (`TensorSpan<T>`/`ReadOnlyTensorSpan<T>`/`TensorDimensionSpan<T>`) are `ref struct`s — stack-only, never a field, never captured in `async`/lambda, never boxed; the owned `Tensor<T>` is the type to store, the spans are the per-operation lens.
- shape is row-major (`Strides` are element counts); `Lengths`/`Strides` are `ReadOnlySpan<nint>` (native width) and `NIndex`/`NRange` index them — not `int`/`Index`/`Range`.

[INTEGRATION_STACK]:
- embedding-rank rail (the primary AppHost consumer): the reasoning owner's `SemanticDiscovery.Index` batches descriptor self-descriptions through the `Microsoft.Extensions.AI` `IEmbeddingGenerator<string,Embedding<float>>` (`api-extensions-ai`), freezes `Embedding<float>.Vector` (`ReadOnlyMemory<float>`) into a `FrozenDictionary<string,ReadOnlyMemory<float>>`, then `Rank` embeds the intent and scores each row by `TensorPrimitives.CosineSimilarity(query.Span, candidate.Span)` — one BCL primitive, zero-copy over the frozen memory, no hand-rolled dot product. The tokenizer leg (`api-ml-tokenizers`) bounds the text before embedding; the cosine score is metered as `ModelTokens` through the grant broker and traced on the GenAI span (`api-otel`).
- quantized/binary-vector leg: `HammingDistance`/`HammingBitDistance<T> where T:IBinaryInteger<T>` rank binary or product-quantized embeddings without dequantizing, the same span-in/scalar-out shape as `CosineSimilarity`.
- identity leg: a tensor or vector buffer reinterpreted via `MemoryMarshal.AsBytes` feeds `System.IO.Hashing` `XxHash3`/`XxHash128` (`api-hashing`) for a content key over the frozen embedding index, so a re-index that produces identical vectors short-circuits.
- substrate boundary: `Tensor<T>` is the numeric carrier, not a domain model — geometry (`Rasm.Geometry` vectors/meshes) and BIM payloads cross to it only as flattened `ReadOnlySpan<T>` at the kernel call; the tensor types never leak into a domain signature.

[LOCAL_ADMISSION]:
- numeric span/tensor math enters through `TensorPrimitives` (span level) or the static `Tensor` algebra (N-d level); the owned buffer is `Tensor<T>`, the per-op lens is `TensorSpan<T>`/`ReadOnlyTensorSpan<T>`, indexed by `NIndex`/`NRange`.
- element type is a generic-math `<T>`, never a per-type kernel; the destination span is caller-owned; the `ref struct` views never escape the stack.
- hardware acceleration, ISA selection, and the scalar tail are owned by the runtime — admit no manual SIMD, no `Vector<T>` loop, no per-element accumulation for a covered operation.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: SIMD generic-math over spans (`TensorPrimitives`) and the dense N-dimensional tensor algebra (`Tensor`/`Tensor<T>`/`TensorSpan<T>`/`ReadOnlyTensorSpan<T>`, `NIndex`/`NRange`)
- Accept: vector similarity/distance, reductions, elementwise/activation/predicate kernels, N-d shape algebra, embedding-rank scoring
- Reject: hand-rolled SIMD or reduction loops, per-element-type kernels, storing a `ref struct` tensor view, leaking tensor types into domain models, and any cryptographic or security claim from these arithmetic primitives
