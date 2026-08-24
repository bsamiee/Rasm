# [RASM_APPHOST_API_ML_TOKENIZERS_O200K]

`Microsoft.ML.Tokenizers.Data.O200kBase` embeds the `o200k_base.tiktoken` BPE rank table as a data-only companion assembly, resolving the `o200k_base` vocabulary offline for a `TiktokenTokenizer` priced against a GPT-4o, GPT-5, or o-series model. It exposes no public API — a `PackageReference` activates it and the consumer names no type — so its whole capability is air-gapped `CostUnit.ModelTokens` pricing for the grant broker.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.Tokenizers.Data.O200kBase`
- package: `Microsoft.ML.Tokenizers.Data.O200kBase` (MIT)
- assembly: `Microsoft.ML.Tokenizers.Data.O200kBase`
- namespace: `Microsoft.ML.Tokenizers`
- asset: data-only `netstandard2.0` library binding forward under `net10.0`
- payload: embedded `o200k_base.tiktoken` BPE rank table for the `o200k_base` encoding
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data-only assembly — zero public surface

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                      |
| :-----: | :----------------------- | :---------------- | :------------------------------------------------ |
|  [01]   | `O200kBaseTokenizerData` | `internal sealed` | holds the embedded `o200k_base.tiktoken` resource |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: no direct entrypoints — the engine resolves this vocabulary by reference

| [INDEX] | [SURFACE]                             | [SHAPE] | [CAPABILITY]                          |
| :-----: | :------------------------------------ | :------ | :------------------------------------ |
|  [01]   | `TiktokenTokenizer.CreateForModel`    | factory | resolve the model-to-`o200k_base` map |
|  [02]   | `TiktokenTokenizer.CreateForEncoding` | factory | resolve `o200k_base` by encoding name |

[MODEL_NAMES]: `gpt-4o` | `gpt-5` | `o1` | `o3` | `o4-mini` | `chatgpt-4o` | `ft:gpt-4o` | …

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Microsoft.ML.Tokenizers` reads the embedded `o200k_base.tiktoken` from whichever referenced `*.Data.*` assembly carries it, so a `PackageReference` alone resolves that read offline.

[STACKING]:
- `Microsoft.ML.Tokenizers`(`.api/api-ml-tokenizers.md`): `CreateForModel`/`CreateForEncoding("o200k_base")` read this embedded resource to build the `TiktokenTokenizer` and own the model-prefix routing that selects `o200k_base`; its `CountTokens` prices the prompt.
- capability-agent: one tokenizer built at composition feeds the grant broker's `CostModel.Variable` a `CostUnit.ModelTokens` count per model-draw pre-flight.

[LOCAL_ADMISSION]:
- admit this package only as a `PackageReference` companion to `Microsoft.ML.Tokenizers`; AppHost imports no namespace and names no type from it.
- admit it alongside `api-ml-tokenizers-cl100k.md` so the broker prices both the o-series (`o200k_base`) and GPT-4/3.5/embedding-3 (`cl100k_base`) families offline, since a model draw targets either encoding.
- AppHost touches this package only at composition, through `CreateForEncoding("o200k_base")` or `CreateForModel("gpt-4o")`; the engine's `CountTokens` rail owns everything past it.

[RAIL_LAW]:
- Package: `Microsoft.ML.Tokenizers.Data.O200kBase`
- Owns: the offline `o200k_base` BPE vocabulary for GPT-4o, GPT-5, and o-series token pricing
- Accept: presence as a `PackageReference` so `CreateForEncoding("o200k_base")` resolves air-gapped
- Reject: a direct type reference, a runtime vocab download, or a hand-shipped `o200k_base.tiktoken` beside it
