# [RASM_APPHOST_API_ML_TOKENIZERS_CL100K]

`Microsoft.ML.Tokenizers.Data.Cl100kBase` embeds the `cl100k_base.tiktoken` BPE vocabulary as a data-only companion assembly with zero public surface, so a `TiktokenTokenizer` resolves the `cl100k_base` ranks air-gapped. Reference presence alone binds it: the grant broker prices a GPT-4 / GPT-3.5 / embedding-3 prompt offline for its `CostUnit.ModelTokens` preview with no network fetch.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.Tokenizers.Data.Cl100kBase`
- package: `Microsoft.ML.Tokenizers.Data.Cl100kBase` (MIT)
- assembly: `Microsoft.ML.Tokenizers.Data.Cl100kBase`
- namespace: `Microsoft.ML.Tokenizers` (carries only `internal sealed class Cl100kBaseTokenizerData`)
- asset: data-only runtime library (`netstandard2.0`; binds forward under `net10.0`)
- payload: embedded resource `cl100k_base.tiktoken`, the BPE rank table for the `cl100k_base` encoding
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: zero public surface; the assembly carries only `internal sealed class Cl100kBaseTokenizerData`, holding the embedded `cl100k_base.tiktoken` resource and reachable by no consumer.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: none direct; the engine resolves this vocabulary by reference presence through the two factory edges.

| [INDEX] | [SURFACE]                                            | [SHAPE] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------- | :------ | :-------------------------------------- |
|  [01]   | `TiktokenTokenizer.CreateForModel(string)`           | factory | map a `cl100k` model name to this vocab |
|  [02]   | `TiktokenTokenizer.CreateForEncoding("cl100k_base")` | factory | resolve the encoding name to this vocab |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Activation is reference presence: the `PackageReference` lets `Microsoft.ML.Tokenizers` read the embedded `cl100k_base` resource offline, and `cl100k_base` serves the GPT-4 / GPT-3.5 / `davinci-002` / embedding model family the engine's prefix table routes here.

[STACKING]:
- `Microsoft.ML.Tokenizers`(`.api/api-ml-tokenizers.md`): the engine owns the model-prefix routing table and reads this resource on `CreateForEncoding("cl100k_base")` / `CreateForModel`, then prices the prompt through its `CountTokens` rail.
- Rasm.AppHost cost broker: `CostModel.Variable` folds `CountTokens` into `CostUnit.ModelTokens` off one composition-time tokenizer, air-gapped.

[LOCAL_ADMISSION]:
- Admit only as a `PackageReference` companion to `Microsoft.ML.Tokenizers`; AppHost code never imports the namespace or names `Cl100kBaseTokenizerData`.
- Admit alongside `api-ml-tokenizers-o200k.md` so the broker prices both the `cl100k_base` (GPT-4 / GPT-3.5 / embedding-3) and `o200k_base` (o-series) families, since a model draw targets either encoding.

[RAIL_LAW]:
- Package: `Microsoft.ML.Tokenizers.Data.Cl100kBase`
- Owns: the offline `cl100k_base` BPE vocabulary for GPT-4 / GPT-3.5 / embedding-3 token pricing
- Accept: presence as a `PackageReference` so `TiktokenTokenizer.CreateForEncoding("cl100k_base")` resolves air-gapped
- Reject: a direct type reference, a runtime vocab download, or a hand-shipped `cl100k_base.tiktoken` file
