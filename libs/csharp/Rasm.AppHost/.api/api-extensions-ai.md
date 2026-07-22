# [RASM_APPHOST_API_EXTENSIONS_AI]

`libs/csharp/.api/api-extensions-ai.md` owns the full `Microsoft.Extensions.AI.Abstractions` surface; this overlay binds the AppHost admission discipline over the DI-injected `IChatClient` and `IEmbeddingGenerator` contracts its capability-agent owners consume.

## [01]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A capability-agent owner binds the `IChatClient` and `IEmbeddingGenerator` contracts through DI; provider construction, builder composition, and middleware stay behind the host boundary.
- `AIFunction` tools reach a request only through `ChatOptions.Tools`, and the middleware pipeline invokes each local function.

[STACKING]:
- `api-extensions-ai-middleware`(`.api/api-extensions-ai-middleware.md`): `ChatClientBuilder`/`EmbeddingGeneratorBuilder` decorate and build the injected contract this overlay admits, holding the abstraction-to-concrete split at the DI seam.
- `api-mcp`(`.api/api-mcp.md`): `McpClientTool : AIFunction` surfaces each server tool as an `AIFunction` row in `ChatOptions.Tools`.

[LOCAL_ADMISSION]:
- A capability-agent owner consumes DI-injected `IChatClient`/`IEmbeddingGenerator`, registers `AIFunction` tools through `ChatOptions.Tools`, and mutates only the `Clone()` copy of `ChatOptions`/`ChatMessage`/`EmbeddingGenerationOptions`.
- `AdditionalPropertiesDictionary` carries every provider-specific key outside the typed option, response, and content surfaces.

[RAIL_LAW]:
- Package: `Microsoft.Extensions.AI.Abstractions`
- Owns: the AppHost admission boundary over the provider-neutral chat, embedding, and modal contracts with their content and tool algebra
- Accept: DI-injected client contracts and `ChatOptions.Tools`-registered `AIFunction` rows inside a capability-agent owner
- Reject: a provider SDK type, a hand-rolled HTTP call, or a model-specific option class in a capability-agent owner
