# [RASM_APPHOST_API_TENSORS]

Full surface and stacking: `libs/csharp/.api/api-tensors.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

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
- substrate boundary: `Tensor<T>` is the numeric carrier, not a domain model — geometry (`Rasm` kernel vectors/meshes) and BIM payloads cross to it only as flattened `ReadOnlySpan<T>` at the kernel call; the tensor types never leak into a domain signature.

[LOCAL_ADMISSION]:
- numeric span/tensor math enters through `TensorPrimitives` (span level) or the static `Tensor` algebra (N-d level); the owned buffer is `Tensor<T>`, the per-op lens is `TensorSpan<T>`/`ReadOnlyTensorSpan<T>`, indexed by `NIndex`/`NRange`.
- element type is a generic-math `<T>`, never a per-type kernel; the destination span is caller-owned; the `ref struct` views never escape the stack.
- hardware acceleration, ISA selection, and the scalar tail are owned by the runtime — admit no manual SIMD, no `Vector<T>` loop, no per-element accumulation for a covered operation.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: SIMD generic-math over spans (`TensorPrimitives`) and the dense N-dimensional tensor algebra (`Tensor`/`Tensor<T>`/`TensorSpan<T>`/`ReadOnlyTensorSpan<T>`, `NIndex`/`NRange`)
- Accept: vector similarity/distance, reductions, elementwise/activation/predicate kernels, N-d shape algebra, embedding-rank scoring
- Reject: hand-rolled SIMD or reduction loops, per-element-type kernels, storing a `ref struct` tensor view, leaking tensor types into domain models, and any cryptographic or security claim from these arithmetic primitives
