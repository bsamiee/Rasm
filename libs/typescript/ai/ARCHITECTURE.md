# [AI_ARCHITECTURE]

The domain map of `ai` — the intelligence rail as a W3 peer folder. `model` owns provider selection, budgets, guardrails, and tier-routing; `embed` the embedding and chunking rows; `tool` the `Schema`-typed toolkits and native MCP hosting; `agent` the durable agents over `work` entities. Provider capability is TS-native — no `*Wire` shape decodes here and no C# seam points in. The folder imports `kernel`, `host`, and `work`; it satisfies the `store/retrieve` `Embedder` port at app composition without importing `store`.

Each codemap node is the eventual source file its `.planning/` design page becomes, in the branch's own file casing — `camelCase.ts`. Treat every node as realized code; the `.planning/` scaffold is authoring substrate, never part of the map. The permitted-edge ledger and port registry are `libs/typescript/.planning/composition-system.md`; dependency direction is the branch `ARCHITECTURE.md`.

## [01]-[DOMAIN_MAP]

```text codemap
ai/ # imports kernel, host, work; satisfies the store/retrieve Embedder port at app composition; no inbound C# seam
├── model/ # provider selection, budgets, guardrails, tier-routing
│   ├── provider.ts # LanguageModel provider rows (anthropic/openai/google/bedrock/openrouter) + the capability-asymmetry table + cost/latency tier-routing + the ONE guardrail gate (input/output moderation folds + Schema-refusal admission) over every row
│   └── token.ts    # AnthropicTokenizer budgets (the one tokenizer owner) + context-assembly rows folding app-passed store/retrieve results — retrieval arrives as values, never a store import
├── embed/ # embedding and chunking
│   ├── embedder.ts # EmbeddingModel rows satisfying the store/retrieve Embedder port
│   └── chunk.ts    # chunking and normalization policy rows
├── tool/ # Schema-typed tools and MCP
│   ├── toolkit.ts  # Schema-typed toolkits as data
│   └── mcp.ts      # MCP hosting on native McpServer/McpSchema (app toolkits projected as MCP tools at the app root); @modelcontextprotocol/sdk is the client lane only
└── agent/ # durable agents
    ├── actor.ts    # durable agents over work entities
    └── memory.ts   # agent memory and session-state rows
```

The `model` sub-domain is the spine: `provider.ts` folds every provider onto one capability-asymmetry table under one guardrail gate, and `token.ts` bounds budgets and assembles context from app-passed values. `embed` produces the `Embedder` the `store` retrieval lane consumes; `tool` supplies the toolkits `agent` and MCP hosting both project; `agent` composes `work` cluster entities for durable runs.

## [02]-[SEAMS]

```text seams
embed/embedder → store/retrieve   # [PORT]: EmbeddingModel Layer satisfying the store/retrieve Embedder port at app composition
agent/actor    ← work/engine      # [SHAPE]: durable agents composed over work cluster entities
model/provider ← host/net         # [SHAPE]: HttpClient default-policy rows (timeout/retry/proxy) the provider Layers compose
```

No inbound C# rows: provider capability is TS-native, so no `*Wire` codec, no descriptor gate, and no cross-language content-key crossing lands in this folder. The one alignment to `store` is the `Embedder` port satisfied at app composition — `ai` produces the `EmbeddingModel` Layer the app root wires into the port `store/retrieve` declares, and `ai` imports no `store` code. `token.ts` context-assembly folds `store/retrieve` results as app-passed values, so the retrieval join is data, never a `store` import edge. `agent` and `model` reach `work` and `host` by legal downward import per the branch edge ledger.
