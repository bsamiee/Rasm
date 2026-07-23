# [RASM_APPHOST_API_ML_TOKENIZERS]

`Microsoft.ML.Tokenizers` owns deterministic offline token counting for the grant-broker cost preview: the abstract `Tokenizer` surface and its `sealed` `TiktokenTokenizer` BPE implementation price a prompt with no network round trip. `CountTokens` feeds `CostModel.Variable` a per-prompt `CostUnit.ModelTokens` count before a model draw charges the budget, and `GetIndexByTokenCount` truncates a prompt to its model context window in place. Only the offline counting and truncation surface binds; encode-for-inference and custom tokenization stages stay out of the cost rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.Tokenizers`
- package: `Microsoft.ML.Tokenizers` (MIT)
- assembly: `Microsoft.ML.Tokenizers`
- namespace: `Microsoft.ML.Tokenizers`
- asset: runtime library, multi-target `net8.0` + `netstandard2.0`; the `net10.0` consumer binds `lib/net8.0` by TFM precedence
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: tokenizer abstraction and tiktoken implementation

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------ | :------------ | :-------------------------------------------- |
|  [01]   | `Tokenizer`         | class         | abstract encode/decode/count owner            |
|  [02]   | `TiktokenTokenizer` | class         | sealed BPE tiktoken tokenizer                 |
|  [03]   | `PreTokenizer`      | class         | split stage behind `Tokenizer.PreTokenizer`   |
|  [04]   | `Normalizer`        | class         | normalize stage behind `Tokenizer.Normalizer` |

[PUBLIC_TYPE_SCOPE]: encode result and settings carriers

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :----------------- | :------------ | :------------------------------ |
|  [01]   | `EncodeResults<T>` | struct        | encode output carrier           |
|  [02]   | `EncodeSettings`   | struct        | encode request knobs            |
|  [03]   | `EncodedToken`     | struct        | readonly id/value/offset record |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: token counting and window truncation, the cost-broker surface

`Tokenizer` instance methods; every read entry carries trailing optional `considerPreTokenization`/`considerNormalization` bools defaulting `true`.

| [INDEX] | [SURFACE]                                                                           | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------------------- | :---------------------------------------- |
|  [01]   | `CountTokens(string) -> int`                                                        | per-prompt token count for the cost model |
|  [02]   | `CountTokens(ReadOnlySpan<char>) -> int`                                            | allocation-free count                     |
|  [03]   | `GetIndexByTokenCount(string, int, out string?, out int) -> int`                    | prefix char index bounding first N tokens |
|  [04]   | `GetIndexByTokenCount(ReadOnlySpan<char>, int, out string?, out int) -> int`        | span prefix path                          |
|  [05]   | `GetIndexByTokenCountFromEnd(string, int, out string?, out int) -> int`             | suffix char index retaining last N tokens |
|  [06]   | `GetIndexByTokenCountFromEnd(ReadOnlySpan<char>, int, out string?, out int) -> int` | span suffix path                          |

[ENTRYPOINT_SCOPE]: encode and decode operations

`Tokenizer` instance methods.

| [INDEX] | [SURFACE]                                                                   | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | `EncodeToIds(string) -> IReadOnlyList<int>`                                 | token ids, unbounded                |
|  [02]   | `EncodeToIds(string, int, out string?, out int) -> IReadOnlyList<int>`      | capped ids with truncation evidence |
|  [03]   | `EncodeToTokens(string, out string?) -> IReadOnlyList<EncodedToken>`        | encoded token records               |
|  [04]   | `Decode(IEnumerable<int>) -> string`                                        | reconstruct a string                |
|  [05]   | `Decode(IEnumerable<int>, Span<char>, out int, out int) -> OperationStatus` | allocation-free decode into span    |

- `Tokenizer.Decode(IEnumerable<int>)` is `virtual`; the `Span<char>` overload is `abstract`, so a derived impl owns the allocation-free path.

[ENTRYPOINT_SCOPE]: `TiktokenTokenizer` construction, model and encoding resolution

Every factory carries optional `extraSpecialTokens`, `cacheSize` (default `8192`), and `normalizer`; embedded-vocab factories omit `cacheSize`.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `CreateForModel(string) -> TiktokenTokenizer`                                | factory  | embedded vocab by model name     |
|  [02]   | `CreateForEncoding(string) -> TiktokenTokenizer`                             | factory  | embedded vocab by encoding name  |
|  [03]   | `Create(Stream, PreTokenizer?, Normalizer?) -> TiktokenTokenizer`            | factory  | custom vocab stream              |
|  [04]   | `Create(string, PreTokenizer?, Normalizer?) -> TiktokenTokenizer`            | factory  | custom vocab file                |
|  [05]   | `CreateForModel(string, Stream) -> TiktokenTokenizer`                        | factory  | model map over a supplied stream |
|  [06]   | `CreateAsync(string, PreTokenizer?, Normalizer?) -> Task<TiktokenTokenizer>` | factory  | async file and stream mirror     |
|  [07]   | `CreateForModelAsync(string, Stream) -> Task<TiktokenTokenizer>`             | factory  | async model map over a stream    |
|  [08]   | `SpecialTokens -> IReadOnlyDictionary<string,int>?`                          | property | special-token vocabulary         |

- `CreateForModel`/`CreateForEncoding` load the referenced embedded `*.Data.*` vocabulary with no stream; an unknown model name throws.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Tokenizer` owns the polymorphic surface — `EncodeToIds`/`EncodeToTokens`/`CountTokens`/`GetIndexByTokenCount`/`Decode` each carry `string` and `ReadOnlySpan<char>` overloads; a derived impl overrides the protected `EncodeToTokens`/`CountTokens`/`Decode` core for an efficient path.
- `TiktokenTokenizer : Tokenizer` is `sealed`, a BPE tokenizer over an internal `LruCache` (default `cacheSize` `8192`, words ≤15 chars cached), carrying the GPT special-token vocabulary (`<|endoftext|>`, `<|im_start|>`, FIM and harmony control tokens).
- `CreateForModel` resolves a model-name prefix table to a private `ModelEncoding`: `gpt-4o-`/`gpt-5-`/`o1-`/`o3-`/`o4-mini-`/`chatgpt-4o-` route to `o200k_base`, and `gpt-4-`/`gpt-3.5-`/`gpt-35-`/`davinci-002`/`text-embedding-3-*`/`text-embedding-ada-002` route to `cl100k_base`; an unknown model name throws, so the cost broker pins a known model id.
- `CountTokens` counts without materializing the `EncodedToken` list, pricing a prompt with no token-string allocation.
- `GetIndexByTokenCount`/`...FromEnd` return the char index bounding the first or last `maxTokenCount` tokens with the normalized text and exact token count as `out` values — the context-window truncation primitive, never a re-encode loop.
- `EncodeResults<T>` carries `Tokens : IReadOnlyList<T>`, `NormalizedText : string?`, and `CharsConsumed : int`, and capped overloads thread truncation evidence back through it; `EncodeSettings` carries `ConsiderPreTokenization`/`ConsiderNormalization` (raw-struct default `false`, public overloads passing `true`) and `MaxTokenCount` (default `int.MaxValue`); `EncodedToken` is a `readonly struct` (`Id : int`, `Value : string`, `Offset : Range`) implementing `IEquatable<EncodedToken>`, the offset a `System.Range` over the source span.

[STACKING]:
- `api-ml-tokenizers-o200k.md`, `api-ml-tokenizers-cl100k.md`: the two data-only `*.Data.*` companions embed the `o200k_base`/`cl100k_base` `.tiktoken` rank tables, and `CreateForModel`/`CreateForEncoding` reads the embedded vocabulary from whichever companion the model or encoding routes to, air-gapped.
- grant-broker cost model: `CostModel.Variable` folds `CountTokens(prompt)` into `CostUnit.ModelTokens` before `CostModel.Estimate` charges a model draw, and the dry-run `Simulate` path prices off the same `CountTokens` call without charging.

[LOCAL_ADMISSION]:
- AppHost capability-agent constructs one `TiktokenTokenizer` per model encoding through `CreateForModel`/`CreateForEncoding`, resolving the embedded vocab from the referenced `*.Data.*` assembly.
- one shared instance answers concurrent `CountTokens` thread-safe, built once at composition and injected, since vocab load and BPE rank build is the expensive step.
- only the offline counting and truncation surface is admitted; encode-for-inference, custom `PreTokenizer`/`Normalizer` stages, the `Bpe`/`SentencePiece`/`WordPiece`/`Bert`/`Llama` model family, and the `TokenizerExtensions` helpers stay outside the cost-preview rail.

[RAIL_LAW]:
- Package: `Microsoft.ML.Tokenizers`
- Owns: deterministic offline token counting and prompt-window truncation for the grant-broker model-draw cost preview
- Accept: `CountTokens`/`GetIndexByTokenCount` over a `CreateForModel`/`CreateForEncoding` tokenizer bound to an embedded vocab
- Reject: a hand-rolled BPE counter, a network token-count call, a per-request tokenizer construction, a heuristic `chars/4` estimate, and a vocab loaded outside the referenced `*.Data.*` assembly
