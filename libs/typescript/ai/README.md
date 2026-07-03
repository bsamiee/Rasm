# [AI]

`ai` is the intelligence rail — the one folder that owns model access as composable Effect `Layer`/`Service` families a thin app root selects. `LanguageModel`/`EmbeddingModel` provider rows (anthropic, openai, google, bedrock, openrouter) fold onto ONE capability-asymmetry table that replaces every provider-uniformity assumption: a provider is a row with its asymmetry columns and its cost/latency tier-routing policy, never a fork. ONE guardrail gate — input/output moderation folds plus `Schema`-refusal admission — sits over every provider row, so no call reaches a model without passing the same admission. `embed` publishes `EmbeddingModel` rows that satisfy the `store/retrieve` `Embedder` port at app composition, and `token` folds app-passed `store/retrieve` results into context-assembly rows — retrieval arrives as values, never a `store` import. `tool` defines `Schema`-typed toolkits as data; `tool/mcp` hosts them on the native `@effect/ai` `McpServer`/`McpSchema` (app toolkits projected as MCP tools, selected at the app root), while `@modelcontextprotocol/sdk` carries only the MCP-client lane that consumes external servers. `agent` builds durable agents over `work` cluster entities. Provider capability is TS-native: no `*Wire` shape decodes here and no C# seam points in. The folder imports `kernel`, `host`, and `work` and nothing else. The domain map and seam record live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[PROVIDER](.planning/model/provider.md): `LanguageModel` provider rows + the capability-asymmetry table + cost/latency tier-routing policy rows + the ONE guardrail gate (input/output moderation folds, `Schema`-refusal admission) every provider row passes.
- [02]-[TOKEN](.planning/model/token.md): tokenizer budgets over the `AnthropicTokenizer` owner + context-assembly rows folding app-passed `store/retrieve` results, never a `store` import.
- [03]-[EMBEDDER](.planning/embed/embedder.md): `EmbeddingModel` rows satisfying the `store/retrieve` `Embedder` port at app composition.
- [04]-[CHUNK](.planning/embed/chunk.md): chunking and normalization policy rows feeding the embedder.
- [05]-[TOOLKIT](.planning/tool/toolkit.md): `Schema`-typed toolkits as data.
- [06]-[MCP](.planning/tool/mcp.md): MCP hosting on the native `McpServer`/`McpSchema` (app toolkits projected as MCP tools at the app root); `@modelcontextprotocol/sdk` is the client lane only.
- [07]-[ACTOR](.planning/agent/actor.md): durable agents over `work` entities.
- [08]-[MEMORY](.planning/agent/memory.md): agent memory and session-state rows.

## [2]-[DOMAIN_PACKAGES]

The `ai` domain libraries — the `# ai` catalog group. Versions are centralized in the one TS manifest and never pinned here; API evidence lives in the adjacent `.api/` folder.

[AI_CORE]:
- `@effect/ai` — the provider-agnostic intelligence surface: `LanguageModel`/`EmbeddingModel` service Tags, `Toolkit` as `Schema`-typed data, the native `McpServer`/`McpSchema` hosting primitives, and the tool-call/response schemas. Every `model/*`, `embed/*`, `tool/*`, and `agent/*` page composes it.

[PROVIDER_LAYERS]:
- `@effect/ai-anthropic`
- `@effect/ai-openai`
- `@effect/ai-google`
- `@effect/ai-amazon-bedrock`
- `@effect/ai-openrouter`

The five provider Layer families the `model/provider.ts` rows select over the capability-asymmetry table; a new provider is one row, never a fork. `@effect/ai-anthropic` carries the `AnthropicTokenizer` the `model/token.ts` budgets own. HttpClient default-policy rows compose from `host/net`; every row passes the one `model/provider.ts` guardrail gate.

[MCP_CLIENT]:
- `@modelcontextprotocol/sdk` — the MCP-client lane ONLY, consuming external MCP servers. MCP HOSTING is the native `@effect/ai` `McpServer`/`McpSchema` on `tool/mcp.ts`; the sdk never re-owns hosting.

## [3]-[SUBSTRATE_PACKAGES]

The TS substrate registry cards this folder composes; the full registry and substrate contracts live in `libs/typescript/.planning/README.md`, with shared API evidence in `libs/typescript/.api/`.

[FUNCTIONAL_CORE]:
- `effect` — rails, `Schema`, `Layer`, `Match`, `Stream`; the provider rows, guardrail folds, and toolkit vocabulary are all values over it.

[PLATFORM]:
- `@effect/platform` — the `HttpClient` and service contracts the provider Layers compose through `host/net`.

[OBSERVABILITY]:
- `@effect/opentelemetry` — the OTLP span and metric emission every provider call and agent run rides.

[TEST_SUBSTRATE]:
- `@effect/vitest` — the dev-plane spec runner binding the `@rasm/ts-testkit` law combinators (`tests/typescript/_testkit`) to this folder's colocated specs.
