# [RASM_APPHOST_API_ML_TOKENIZERS_O200K]

`Microsoft.ML.Tokenizers.Data.O200kBase` is a data-only companion assembly that embeds the
`o200k_base.tiktoken` vocabulary file consumed by `Microsoft.ML.Tokenizers` (see
`api-ml-tokenizers.md`). It exposes **no public API**: the consumer never names a type from it.
Its sole job is to make the `o200k_base` ranks resolvable OFFLINE so a `TiktokenTokenizer`
built for a GPT-4o / GPT-5 / o-series model prices a prompt air-gapped for the grant-broker
`CostUnit.ModelTokens` cost preview.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.Tokenizers.Data.O200kBase`
- package: `Microsoft.ML.Tokenizers.Data.O200kBase`
- assembly: `Microsoft.ML.Tokenizers.Data.O200kBase`
- namespace: `Microsoft.ML.Tokenizers` (carries only `internal sealed class O200kBaseTokenizerData`)
- version: `2.0.0`
- license: `MIT`
- asset: data-only runtime library (`netstandard2.0`-only; binds forward under `net10.0`)
- payload: embedded resource `o200k_base.tiktoken` — the BPE rank table for the `o200k_base` encoding
- closure: nuspec pulls `Microsoft.ML.Tokenizers 2.0.0` + `Microsoft.Bcl.Memory 9.0.4` (advisory `GHSA-73j8-2gch-69rq`, floor-pinned to `10.0.9` centrally)
- rail: capability-agent

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: data-only assembly — zero public surface
- rail: capability-agent

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]        | [RAIL]                                                            |
| :-----: | :------------------------ | :------------------- | :--------------------------------------------------------------- |
|  [01]   | `O200kBaseTokenizerData`  | `internal sealed`    | not callable — holds the embedded `o200k_base.tiktoken` resource |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: no direct entrypoints — activated by reference through the engine
- rail: capability-agent

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]   | [RAIL]                                                              |
| :-----: | :--------------------------------------------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `TiktokenTokenizer.CreateForModel("gpt-4o" \| "gpt-5" \| "o1" \| "o3" \| "o4-mini" \| "chatgpt-4o-…")` | resolution edge  | the engine resolves the model→`o200k_base` map and loads THIS embedded vocab |
|  [02]   | `TiktokenTokenizer.CreateForEncoding("o200k_base")`             | resolution edge  | the engine resolves the encoding name directly to this vocab       |

## [04]-[IMPLEMENTATION_LAW]

[DATA_PACKAGE_TOPOLOGY]:
- the assembly carries exactly one `internal sealed class O200kBaseTokenizerData` plus the embedded `o200k_base.tiktoken` resource; reflection confirms no public type, so there is nothing for a consumer to call.
- activation is by **reference presence**: when `Microsoft.ML.Tokenizers` resolves the `o200k_base` encoding, it reads the embedded resource from whichever referenced `*.Data.*` assembly carries it; the `PackageReference` to this package is what makes that read succeed offline.
- `o200k_base` is the GPT-4o / GPT-5 / o-series / GPT-4.1 / GPT-4.5 vocabulary; the engine's model-prefix table (`gpt-4o-`, `gpt-5-`, `o1-`, `o3-`, `o4-mini-`, `chatgpt-4o-`, `ft:gpt-4o`) all route here.

[LOCAL_ADMISSION]:
- this package is admitted ONLY as a `PackageReference` companion to `Microsoft.ML.Tokenizers`; AppHost code never imports its namespace and never names `O200kBaseTokenizerData`.
- it pairs with `api-ml-tokenizers-cl100k.md` so the cost broker can price BOTH the o-series (`o200k_base`) and the GPT-4/3.5/embedding-3 (`cl100k_base`) model families offline; admit both, since a model draw may target either encoding.
- the only AppHost touch is `TiktokenTokenizer.CreateForEncoding("o200k_base")` / `CreateForModel("gpt-4o")` at composition; everything past that is the engine's `CountTokens` rail.

[RAIL_LAW]:
- Package: `Microsoft.ML.Tokenizers.Data.O200kBase`
- Owns: the offline `o200k_base` BPE vocabulary for GPT-4o / GPT-5 / o-series token pricing
- Accept: presence as a `PackageReference` so `TiktokenTokenizer.CreateForEncoding("o200k_base")` resolves air-gapped
- Reject: any direct type reference, a runtime vocab download, or a hand-shipped `o200k_base.tiktoken` file beside it
