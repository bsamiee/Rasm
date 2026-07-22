# [RASM_APPHOST_API_TENSORS]

`libs/csharp/.api/api-tensors.md` owns the full `System.Numerics.Tensors` surface; this overlay binds AppHost's one tensor seam — the cosine embedding-rank scorer `Agent/reasoning` discovery folds over frozen `ReadOnlyMemory<float>` vectors.

## [01]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Embedding<float>.Vector` (`ReadOnlyMemory<float>`) is the only buffer crossing to a span primitive; no `Tensor<T>` or `ref struct` view enters a domain signature.

[STACKING]:
- `Agent/reasoning`: `SemanticDiscovery.Index` freezes each `CapabilityDescriptor` embedding into `EmbeddingIndex.Vectors` (`FrozenDictionary<string, ReadOnlyMemory<float>>`) through the governed `IEmbeddingGenerator<string, Embedding<float>>`; `Rank` scores every row via `TensorPrimitives.CosineSimilarity(query.Vector.Span, candidate.Span)` to `Seq<IntentMatch>`.
- `api-extensions-ai`(`.api/api-extensions-ai.md`): the DI-injected `IEmbeddingGenerator` yields the `ReadOnlyMemory<float>` vector, so the `float` non-generic `CosineSimilarity` overload is the embedding-rail call.
- `api-ml-tokenizers`(`.api/api-ml-tokenizers.md`): `CountTokens` prices the intent text as `CostUnit.ModelTokens` before the cached, GenAI-traced embedding draw.

[LOCAL_ADMISSION]:
- Only the frozen-embedding cosine scorer admits this surface; the substrate's N-d `Tensor` algebra and non-`float` generic-math have no AppHost consumer.

[RAIL_LAW]:
- Package: `System.Numerics.Tensors`
- Owns: cosine embedding-rank scoring over frozen `ReadOnlyMemory<float>` vectors in `Agent/reasoning` discovery
- Accept: `TensorPrimitives.CosineSimilarity` over the frozen `EmbeddingIndex` vectors behind the governed embedder
- Reject: a hand-rolled similarity loop over embedding memory, or a `Tensor<T>` or `ref struct` view in a domain signature
