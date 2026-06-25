# [RASM_APPHOST_API_ML_TOKENIZERS]

`Microsoft.ML.Tokenizers` supplies the abstract `Tokenizer` polymorphic owner plus the
`TiktokenTokenizer` BPE implementation that AppHost capability-agent owners use as a
deterministic, offline, pre-call token-count source: `CountTokens` feeds the grant-broker
`CostModel.Variable` delegate a per-prompt `CostUnit.ModelTokens` count so a model draw is
priced and ceiling-gated BEFORE any tokens are spent, and `GetIndexByTokenCount` truncates a
prompt to a model context window without a network round-trip. The two `*.Data.*` vocab
packages embed the `o200k_base` and `cl100k_base` `.tiktoken` files so the dry-run pricing is
air-gapped; see `api-ml-tokenizers-o200k.md` and `api-ml-tokenizers-cl100k.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.Tokenizers`
- package: `Microsoft.ML.Tokenizers`
- assembly: `Microsoft.ML.Tokenizers`
- namespace: `Microsoft.ML.Tokenizers`
- version: `2.0.0`
- license: `MIT`
- asset: runtime library (multi-target `net8.0` + `netstandard2.0`; the `net10.0` consumer binds `lib/net8.0` — no `net10.0` asset ships, so `net8.0` is the bound asset by TFM precedence)
- closure: the `net8.0` asset pulls `Microsoft.Bcl.Memory 9.0.4` (advisory `GHSA-73j8-2gch-69rq`); the central manifest floor-pins the transitive to `10.0.9` so restore lifts the resolved version above the vuln
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tokenizer abstraction and tiktoken implementation
- rail: capability-agent

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                                                            |
| :-----: | :------------------ | :----------------- | :--------------------------------------------------------------- |
|  [01]   | `Tokenizer`         | abstract base      | polymorphic encode/decode/count owner — derived impls override   |
|  [02]   | `TiktokenTokenizer` | `sealed` BPE impl  | rapid Byte-Pair-Encoding tiktoken tokenizer — the cost-broker one |
|  [03]   | `PreTokenizer`      | abstract splitter  | pre-tokenization stage — `Tokenizer.PreTokenizer` accessor        |
|  [04]   | `Normalizer`        | abstract normalize | normalization stage — `Tokenizer.Normalizer` accessor             |

[PUBLIC_TYPE_SCOPE]: encode result and settings carriers
- rail: capability-agent

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]       | [RAIL]                                                       |
| :-----: | :--------------------- | :------------------ | :---------------------------------------------------------- |
|  [01]   | `EncodeResults<T>`     | `struct` result     | `Tokens : IReadOnlyList<T>`, `NormalizedText`, `CharsConsumed` |
|  [02]   | `EncodeSettings`       | `struct` request    | `ConsiderPreTokenization`, `ConsiderNormalization`, `MaxTokenCount` |
|  [03]   | `EncodedToken`         | `readonly struct`   | `Id : int`, `Value : string`, `Offset : Range` — `IEquatable` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token counting and window truncation (cost-broker surface)
- rail: capability-agent

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                                                          |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `Tokenizer.CountTokens(string text, bool considerPreTokenization = true, bool considerNormalization = true)` | count call     | returns `int` — the `CostModel.Variable` per-prompt token count |
|  [02]   | `Tokenizer.CountTokens(ReadOnlySpan<char> text, bool = true, bool = true)`                          | count call     | zero-alloc span overload                                        |
|  [03]   | `Tokenizer.GetIndexByTokenCount(string text, int maxTokenCount, out string? normalizedText, out int tokenCount, bool = true, bool = true)` | truncate call  | char index spanning the first `maxTokenCount` tokens — window trim |
|  [04]   | `Tokenizer.GetIndexByTokenCount(ReadOnlySpan<char> text, int maxTokenCount, out string?, out int, bool, bool)` | truncate call  | span overload of the prefix-trim                               |
|  [05]   | `Tokenizer.GetIndexByTokenCountFromEnd(string text, int maxTokenCount, out string?, out int, bool, bool)` | truncate call  | suffix-trim — keeps the trailing `maxTokenCount` tokens         |
|  [06]   | `Tokenizer.GetIndexByTokenCountFromEnd(ReadOnlySpan<char> text, int maxTokenCount, out string?, out int, bool, bool)` | truncate call  | span overload of the suffix-trim                               |

[ENTRYPOINT_SCOPE]: encode and decode operations
- rail: capability-agent

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [RAIL]                                                         |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `Tokenizer.EncodeToIds(string text, bool = true, bool = true)`                                  | encode call    | returns `IReadOnlyList<int>` token ids                        |
|  [02]   | `Tokenizer.EncodeToIds(string text, int maxTokenCount, out string? normalizedText, out int charsConsumed, bool, bool)` | encode call    | capped-encode returning ids + chars consumed                 |
|  [03]   | `Tokenizer.EncodeToTokens(string text, out string? normalizedText, bool = true, bool = true)`   | encode call    | returns `IReadOnlyList<EncodedToken>` with id/value/offset    |
|  [04]   | `Tokenizer.Decode(IEnumerable<int> ids)`                                                        | decode call    | `virtual` — returns reconstructed `string`                    |
|  [05]   | `Tokenizer.Decode(IEnumerable<int> ids, Span<char> destination, out int idsConsumed, out int charsWritten)` | decode call    | `abstract` — `OperationStatus` span-decode (no alloc)         |

[ENTRYPOINT_SCOPE]: `TiktokenTokenizer` construction (model/encoding resolution)
- rail: capability-agent

| [INDEX] | [SURFACE]                                                                                       | [ENTRY_FAMILY] | [RAIL]                                                              |
| :-----: | :---------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `TiktokenTokenizer.CreateForModel(string modelName, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, Normalizer? normalizer = null)` | factory call   | resolves the model→encoding map (`gpt-4o`→o200k, `gpt-4`→cl100k) and loads the **referenced embedded vocab** — air-gapped, no stream |
|  [02]   | `TiktokenTokenizer.CreateForEncoding(string encodingName, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, Normalizer? normalizer = null)` | factory call   | resolves by encoding name (`o200k_base`/`cl100k_base`) from the embedded vocab |
|  [03]   | `TiktokenTokenizer.Create(Stream vocabStream, PreTokenizer? preTokenizer, Normalizer? normalizer, IReadOnlyDictionary<string,int>? specialTokens = null, int cacheSize = 8192)` | factory call   | explicit-stream construction (custom vocab, not the embedded path) |
|  [04]   | `TiktokenTokenizer.Create(string vocabFilePath, PreTokenizer?, Normalizer?, IReadOnlyDictionary<string,int>? specialTokens = null, int cacheSize = 8192)` | factory call   | file-path construction                                             |
|  [05]   | `TiktokenTokenizer.CreateForModel(string modelName, Stream vocabStream, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, int cacheSize = 8192, Normalizer? normalizer = null)` | factory call   | model resolution against a supplied stream                        |
|  [06]   | `TiktokenTokenizer.CreateAsync(string vocabFilePath, PreTokenizer?, Normalizer?, IReadOnlyDictionary<string,int>? specialTokens = null, int cacheSize = 8192, CancellationToken = default)` | async factory  | `Task<TiktokenTokenizer>` async mirror of stream/file `Create`     |
|  [07]   | `TiktokenTokenizer.CreateForModelAsync(string modelName, Stream vocabStream, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, int cacheSize = 8192, Normalizer? normalizer = null, CancellationToken = default)` | async factory  | async mirror of model-resolution construction                     |
|  [08]   | `TiktokenTokenizer.SpecialTokens`                                                               | property       | `IReadOnlyDictionary<string,int>?` — the special-token vocab       |

## [04]-[IMPLEMENTATION_LAW]

[TOKENIZER_TOPOLOGY]:
- `Tokenizer` is the abstract polymorphic owner: `EncodeToIds`/`EncodeToTokens`/`CountTokens`/`GetIndexByTokenCount`/`Decode` are the canonical surface, with `string` and `ReadOnlySpan<char>` overloads on every read path; derived implementations override `EncodeToIds`/`Decode` for an efficient path.
- `TiktokenTokenizer : Tokenizer` is `sealed` — a rapid BPE tokenizer over a byte-pair encoder with an internal `LruCache` (default `cacheSize: 8192`, words ≤15 chars cached); it carries the GPT special-token vocabulary (`<|endoftext|>`, `<|im_start|>`, FIM/harmony control tokens).
- model resolution is a private `ModelEncoding` enum behind `CreateForModel`: a model-name prefix table maps `gpt-4o-`/`gpt-5-`/`o1-`/`o3-`/`o4-mini-`/`chatgpt-4o-` → `o200k_base`, and `gpt-4-`/`gpt-3.5-`/`gpt-35-`/`davinci-002`/`text-embedding-3-*`/`text-embedding-ada-002` → `cl100k_base`; an unknown model name throws, so the cost broker pins a known model id.
- `CountTokens` is the cheap path: it counts without materializing the `EncodedToken` list, so the grant-broker pre-flight prices a prompt without allocating the token strings.
- `GetIndexByTokenCount` / `...FromEnd` return the **char index** bounding the first/last `maxTokenCount` tokens with the normalized text and exact token count as `out` values — the prompt-truncation primitive for a model context window, never a re-encode loop.

[ENCODE_RESULT_TOPOLOGY]:
- `EncodeResults<T>` is a `struct` carrying `Tokens : IReadOnlyList<T>` (ids or `EncodedToken`), `NormalizedText : string?`, and `CharsConsumed : int`; the capped overloads thread the truncation evidence back through it.
- `EncodeSettings` is a `struct` with `ConsiderPreTokenization`/`ConsiderNormalization` (both default `false` on the raw struct, but the public string/span overloads default them `true`) and `MaxTokenCount` (default `int.MaxValue`).
- `EncodedToken` is a `readonly struct` (`Id : int`, `Value : string`, `Offset : Range`) implementing `IEquatable<EncodedToken>` — the offset is a `System.Range` over the source span for span-precise highlighting.

[LOCAL_ADMISSION]:
- AppHost capability-agent constructs one `TiktokenTokenizer` per model encoding via `CreateForModel`/`CreateForEncoding` (the no-stream overloads), resolving the embedded vocab from the referenced `*.Data.*` assembly — never a network fetch and never a hand-shipped `.tiktoken` file.
- The single tokenizer instance is shared and thread-safe for concurrent `CountTokens`; build it once at composition and inject it, never per-request, because vocab load + BPE rank build is the expensive step.
- The cost-broker `CostModel.Variable` delegate calls `CountTokens(prompt)` and folds the result into `CostUnit.ModelTokens` so `CostModel.Estimate` prices a model draw before the broker charges the budget; the dry-run `Simulate` path prices off the same `CountTokens` call without charging.
- Only the offline counting/truncation surface is admitted; encode-for-inference, custom `PreTokenizer`/`Normalizer` stages, the `Bpe`/`SentencePiece`/`WordPiece`/`Bert`/`Llama` model family, and the `TokenizerExtensions` dictionary helpers are out of scope for the cost-preview rail.

[RAIL_LAW]:
- Package: `Microsoft.ML.Tokenizers`
- Owns: deterministic offline token counting and prompt-window truncation for the grant-broker model-draw cost preview
- Accept: `CountTokens` / `GetIndexByTokenCount` over a `CreateForModel`/`CreateForEncoding` tokenizer bound to an embedded vocab
- Reject: a hand-rolled BPE counter, a network token-count call, a per-request tokenizer construction, a heuristic `chars/4` token estimate, and any vocab loaded from outside the referenced `*.Data.*` assembly
