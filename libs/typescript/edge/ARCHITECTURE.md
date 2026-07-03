# [EDGE_ARCHITECTURE]

The domain map of `edge` — the W4 public front door where the `api`, `problem`, `live`, `hook`, and `cli` sub-domains carry every public entry family under the one assembly law: contribution families exported as data, the singleton `HttpApi` VALUE and `Command` root assembled only in the app, every fault leaving through the outbound-only `problem` altitude. Public ingress is TS-native — the C# wire enters the branch at `wire`, never here.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
edge/src/ # W4 public front door — imports kernel, state, host, security, telemetry
├── api/ # The HTTP entry family under the one assembly law
│ ├── group.ts # HttpApiGroup contribution law — the app owns the assembled HttpApi value; RpcGroup/RpcServer rows (http | websocket | worker) the second contribution family; version-prefix + pagination convention rows
│ ├── middleware.ts # auth + API-key admission (security) + W3C trace continuation (telemetry) + rate/quota + load-shed (concurrency caps, queue-depth 503/Retry-After) + CORS + security-header + idempotency-key admission + locale negotiation (Accept-Language → kernel Locale FiberRef row) + FiberRef request/tenant context rows
│ ├── serve.ts # NodeHttpServer | BunHttpServer | toWebHandler serve rows + the SPA/static-asset row (immutable-asset cache headers)
│ └── emit.ts # OpenApi document emission + HttpApiScalar reference + HttpApiClient typed-SDK derivation from the app-assembled HttpApi value — spec, docs, and client never drift from the served contract
├── problem/ # The outbound-only fault altitude
│ ├── detail.ts # RFC 9457 problem-details mapping — the reconstructed FaultDetail and every folder's Data.TaggedError rail projected outward
│ └── policy.ts # status/retry-after/redaction policy rows
├── live/ # Realtime over state Subscribables
│ ├── socket.ts # WS/SSE endpoints over state Subscribables + SSE resume-token rows + the protocol-handler mount port (an HttpApp port Tag the app root satisfies)
│ └── presence.ts # presence/subscription admission
├── hook/ # Inbound webhook admission
│ ├── verify.ts # inbound webhook signature verify (security/sign)
│ └── admit.ts # replay protection + quota admission (work fenced-quota rows via port) + the declared ingress port Tag admitted hooks enqueue through (work queue or store journal satisfies)
└── cli/ # The terminal entry family under the same assembly law
  ├── verb.ts # verb contribution families — the app assembles exactly one Command root from selected families; doctor/replay/inspect the lib ops family, executing over host/exec
  └── render.ts # CLI output rendering rows — Doc composition through @effect/printer(-ansi)
```

The `api` sub-domain is the spine: `group.ts` declares the contribution law every entry family instantiates, `middleware.ts` the closed cross-cutting row family, `serve.ts` the one serving surface, `emit.ts` the derivations that keep spec, docs, and SDK drift-free against the served contract. `live`, `hook`, and `cli` are the other entry families under the same law; `problem` is the one fault exit every family shares.

## [02]-[SEAMS]

```text seams
hook/admit     ← work          # [PORT]: the declared ingress port Tag admitted hooks enqueue through — a work queue Layer satisfies at the app root; quota admission types against the work fenced-quota rows via port
hook/admit     ← store         # [PORT]: the same ingress port Tag — a store journal Layer is the alternate app-root satisfier
live/socket    ← store         # [PORT]: the protocol-handler mount port (an HttpApp port Tag) — the store EventLog sync server is the standing satisfier
problem/detail ← wire/fault    # [FAULT]: the reconstructed C#-minted FaultDetail projected outward as RFC 9457 problem details — outbound-only, never an ingress decode
problem/detail ← kernel/fault  # [FAULT]: the fault-classification vocabulary every folder's Data.TaggedError rail lowers onto, projected outward through the problem/policy status/redaction rows
```

No inbound C# row exists: public ingress is TS-native, and the C# wire enters the branch only at `wire`. The `[PORT]` rows are the folder's whole cross-folder surface — a port exists exactly where the edge ledger forbids the import, and the app root wires every satisfier. Durable signed egress is `work/deliver`; MCP hosting is `ai/mcp`; both are app-root selections, never edge surfaces. The render posture is law: the SPA/static-asset serve row serves the client-rendered PWA and its build-time prerendered per-route HTML; a streaming-SSR react server runtime is the named non-goal, and the `react*` scope tags keep it structurally unreachable from `scope:edge`.
