# [RASM_APPHOST_API_ML_TOKENIZERS]

`Microsoft.ML.Tokenizers` supplies the abstract `Tokenizer` owner and the `TiktokenTokenizer` BPE implementation as a deterministic offline token-count source. `CountTokens` feeds the grant broker's `CostModel.Variable` delegate a per-prompt `CostUnit.ModelTokens` count before a model draw consumes tokens, and `GetIndexByTokenCount` truncates a prompt to its model context window without a network round trip. The `*.Data.*` vocabulary packages embed the `o200k_base` and `cl100k_base` `.tiktoken` files for air-gapped dry-run pricing; `api-ml-tokenizers-o200k.md` and `api-ml-tokenizers-cl100k.md` own their catalogues.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.Tokenizers`

- package: `Microsoft.ML.Tokenizers`
- assembly: `Microsoft.ML.Tokenizers`
- namespace: `Microsoft.ML.Tokenizers`
- license: `MIT`
- asset: runtime library (multi-target `net8.0` + `netstandard2.0`; the `net10.0` consumer binds `lib/net8.0` — no `net10.0` asset ships, so `net8.0` is the bound asset by TFM precedence)
- closure: the `net8.0` asset pulls `Microsoft.Bcl.Memory 9.0.4` (advisory `GHSA-73j8-2gch-69rq`); the central manifest floor-pins the transitive to `10.0.9` so restore lifts the resolved version above the vuln
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tokenizer abstraction and tiktoken implementation

- rail: capability-agent

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                                                            |
| :-----: | :------------------ | :----------------- | :---------------------------------------------------------------- |
|  [01]   | `Tokenizer`         | abstract base      | polymorphic encode/decode/count owner — derived impls override    |
|  [02]   | `TiktokenTokenizer` | `sealed` BPE impl  | rapid Byte-Pair-Encoding tiktoken tokenizer — the cost-broker one |
|  [03]   | `PreTokenizer`      | abstract splitter  | pre-tokenization stage — `Tokenizer.PreTokenizer` accessor        |
|  [04]   | `Normalizer`        | abstract normalize | normalization stage — `Tokenizer.Normalizer` accessor             |

[PUBLIC_TYPE_SCOPE]: encode result and settings carriers

- rail: capability-agent

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [RAIL]                                                              |
| :-----: | :----------------- | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `EncodeResults<T>` | `struct` result   | `Tokens : IReadOnlyList<T>`, `NormalizedText`, `CharsConsumed`      |
|  [02]   | `EncodeSettings`   | `struct` request  | `ConsiderPreTokenization`, `ConsiderNormalization`, `MaxTokenCount` |
|  [03]   | `EncodedToken`     | `readonly struct` | `Id : int`, `Value : string`, `Offset : Range` — `IEquatable`       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token counting and window truncation (cost-broker surface)

- rail: capability-agent

| [INDEX] | [SURFACE]                     | [INPUT]              | [RESULT]     |
| :-----: | :---------------------------- | :------------------- | :----------- |
|  [01]   | `CountTokens`                 | `string`             | token count  |
|  [02]   | `CountTokens`                 | `ReadOnlySpan<char>` | token count  |
|  [03]   | `GetIndexByTokenCount`        | `string`             | prefix index |
|  [04]   | `GetIndexByTokenCount`        | `ReadOnlySpan<char>` | prefix index |
|  [05]   | `GetIndexByTokenCountFromEnd` | `string`             | suffix index |
|  [06]   | `GetIndexByTokenCountFromEnd` | `ReadOnlySpan<char>` | suffix index |

- [01]-[COUNT_STRING]: `Tokenizer.CountTokens(string text, bool considerPreTokenization = true, bool considerNormalization = true)` returns the `int` consumed by `CostModel.Variable` as its per-prompt token count.
- [02]-[COUNT_SPAN]: `Tokenizer.CountTokens(ReadOnlySpan<char> text, bool = true, bool = true)` counts without allocation.
- [03]-[PREFIX_STRING]: `Tokenizer.GetIndexByTokenCount(string text, int maxTokenCount, out string? normalizedText, out int tokenCount, bool = true, bool = true)` returns the character index spanning the first `maxTokenCount` tokens.
- [04]-[PREFIX_SPAN]: `Tokenizer.GetIndexByTokenCount(ReadOnlySpan<char> text, int maxTokenCount, out string?, out int, bool, bool)` owns the span prefix path.
- [05]-[SUFFIX_STRING]: `Tokenizer.GetIndexByTokenCountFromEnd(string text, int maxTokenCount, out string?, out int, bool, bool)` returns the character index retaining the trailing `maxTokenCount` tokens.
- [06]-[SUFFIX_SPAN]: `Tokenizer.GetIndexByTokenCountFromEnd(ReadOnlySpan<char> text, int maxTokenCount, out string?, out int, bool, bool)` owns the span suffix path.

[ENTRYPOINT_SCOPE]: encode and decode operations

- rail: capability-agent

| [INDEX] | [SURFACE]        | [MODE]    | [RESULT]             |
| :-----: | :--------------- | :-------- | :------------------- |
|  [01]   | `EncodeToIds`    | unbounded | `IReadOnlyList<int>` |
|  [02]   | `EncodeToIds`    | capped    | ids and consumption  |
|  [03]   | `EncodeToTokens` | unbounded | encoded tokens       |
|  [04]   | `Decode`         | allocated | `string`             |
|  [05]   | `Decode`         | span      | `OperationStatus`    |

- [01]-[ENCODE_IDS]: `Tokenizer.EncodeToIds(string text, bool = true, bool = true)` returns token identifiers.
- [02]-[ENCODE_IDS_CAPPED]: `Tokenizer.EncodeToIds(string text, int maxTokenCount, out string? normalizedText, out int charsConsumed, bool, bool)` returns token identifiers with normalized text and consumed character count.
- [03]-[ENCODE_TOKENS]: `Tokenizer.EncodeToTokens(string text, out string? normalizedText, bool = true, bool = true)` returns `IReadOnlyList<EncodedToken>` values carrying identifiers, text values, and offsets.
- [04]-[DECODE_STRING]: The `virtual` `Tokenizer.Decode(IEnumerable<int> ids)` returns a reconstructed string.
- [05]-[DECODE_SPAN]: The allocation-free `abstract` `Tokenizer.Decode(IEnumerable<int> ids, Span<char> destination, out int idsConsumed, out int charsWritten)` returns `OperationStatus`.

[ENTRYPOINT_SCOPE]: `TiktokenTokenizer` construction (model/encoding resolution)

- rail: capability-agent

| [INDEX] | [SURFACE]             | [SOURCE]        | [EXECUTION]  |
| :-----: | :-------------------- | :-------------- | :----------- |
|  [01]   | `CreateForModel`      | embedded model  | synchronous  |
|  [02]   | `CreateForEncoding`   | embedded name   | synchronous  |
|  [03]   | `Create`              | stream          | synchronous  |
|  [04]   | `Create`              | file            | synchronous  |
|  [05]   | `CreateForModel`      | supplied stream | synchronous  |
|  [06]   | `CreateAsync`         | file            | asynchronous |
|  [07]   | `CreateForModelAsync` | supplied stream | asynchronous |
|  [08]   | `SpecialTokens`       | tokenizer       | property     |

- [01]-[MODEL_EMBEDDED]: `TiktokenTokenizer.CreateForModel(string modelName, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, Normalizer? normalizer = null)` maps `gpt-4o` to `o200k` and `gpt-4` to `cl100k`, then loads the referenced embedded vocabulary without a stream.
- [02]-[ENCODING_EMBEDDED]: `TiktokenTokenizer.CreateForEncoding(string encodingName, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, Normalizer? normalizer = null)` resolves an embedded `o200k_base` or `cl100k_base` vocabulary by encoding name.
- [03]-[STREAM_CREATE]: `TiktokenTokenizer.Create(Stream vocabStream, PreTokenizer? preTokenizer, Normalizer? normalizer, IReadOnlyDictionary<string,int>? specialTokens = null, int cacheSize = 8192)` constructs from a custom vocabulary stream outside the embedded path.
- [04]-[FILE_CREATE]: `TiktokenTokenizer.Create(string vocabFilePath, PreTokenizer?, Normalizer?, IReadOnlyDictionary<string,int>? specialTokens = null, int cacheSize = 8192)` constructs from a vocabulary file.
- [05]-[MODEL_STREAM]: `TiktokenTokenizer.CreateForModel(string modelName, Stream vocabStream, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, int cacheSize = 8192, Normalizer? normalizer = null)` resolves a model mapping against the supplied stream.
- [06]-[FILE_CREATE_ASYNC]: `TiktokenTokenizer.CreateAsync(string vocabFilePath, PreTokenizer?, Normalizer?, IReadOnlyDictionary<string,int>? specialTokens = null, int cacheSize = 8192, CancellationToken = default)` returns `Task<TiktokenTokenizer>` as the asynchronous stream and file `Create` mirror.
- [07]-[MODEL_STREAM_ASYNC]: `TiktokenTokenizer.CreateForModelAsync(string modelName, Stream vocabStream, IReadOnlyDictionary<string,int>? extraSpecialTokens = null, int cacheSize = 8192, Normalizer? normalizer = null, CancellationToken = default)` resolves a model mapping asynchronously against the supplied stream.
- [08]-[SPECIAL_TOKENS]: `TiktokenTokenizer.SpecialTokens` exposes the `IReadOnlyDictionary<string,int>?` special-token vocabulary.

## [04]-[IMPLEMENTATION_LAW]

[TOKENIZER_TOPOLOGY]:

- `Tokenizer` is the abstract polymorphic owner: `EncodeToIds`/`EncodeToTokens`/`CountTokens`/`GetIndexByTokenCount`/`Decode` are the canonical surface, with `string` and `ReadOnlySpan<char>` overloads on every read path; derived implementations override `EncodeToIds`/`Decode` for an efficient path.
- `TiktokenTokenizer : Tokenizer` is `sealed` — a rapid BPE tokenizer over a byte-pair encoder with an internal `LruCache` (default `cacheSize: 8192`, words ≤15 chars cached); it carries the GPT special-token vocabulary (`<|endoftext|>`, `<|im_start|>`, FIM/harmony control tokens).
- model resolution is a private `ModelEncoding` enum behind `CreateForModel`: a model-name prefix table maps `gpt-4o-`/`gpt-5-`/`o1-`/`o3-`/`o4-mini-`/`chatgpt-4o-` → `o200k_base`, and `gpt-4-`/`gpt-3.5-`/`gpt-35-`/`davinci-002`/`text-embedding-3-*`/`text-embedding-ada-002` → `cl100k_base`; an unknown model name throws, so the cost broker pins a known model id.
- `CountTokens` is the cheap path: it counts without materializing the `EncodedToken` list, so the grant-broker pre-flight prices a prompt without allocating the token strings.
- `GetIndexByTokenCount` / `...FromEnd` return the char index bounding the first/last `maxTokenCount` tokens with the normalized text and exact token count as `out` values — the prompt-truncation primitive for a model context window, never a re-encode loop.

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
