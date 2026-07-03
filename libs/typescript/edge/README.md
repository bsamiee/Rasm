# [EDGE]

`edge` is the one public front door of the TypeScript branch — the W4 folder where the outside world enters over HTTP, WebSocket/SSE, inbound webhooks, and the terminal, and where every fault leaves as an RFC 9457 problem detail. Entry families generalize under ONE assembly law: domain folders contribute `HttpApiGroup` families as data — `RpcGroup`/`RpcServer` rows (http | websocket | worker) the second contribution family — and CLI verb families the same way; the APP assembles exactly one `HttpApi` VALUE and exactly one `Command` root from selected families, and MCP hosting (`ai/mcp`) is selected at the same app root. The god-contract is structurally impossible because the assembled value has no lib-side existence. The assembled api VALUE is the single derivation source — the OpenApi document, the `HttpApiScalar` reference surface, and the `HttpApiClient` typed SDK derive from it and never drift from the served contract — and it serves through one serve row (`NodeHttpServer` | `BunHttpServer` | the `toWebHandler` fetch row, plus the SPA/static-asset row with immutable-asset cache headers). Middleware is a closed row family: auth + API-key admission (`security`), W3C trace continuation (`telemetry`), rate/quota + load-shed (concurrency caps, queue-depth 503/Retry-After), CORS + security-header, idempotency-key admission, locale negotiation (Accept-Language → the kernel `Locale` FiberRef row), and the FiberRef request/tenant context rows. `live` serves WS/SSE over `state` Subscribables with SSE resume-token rows and carries the protocol-handler mount port; `hook` owns inbound webhook admission — signature verify over `security/sign`, replay protection, quota admission against the `work` fenced-quota rows via port — enqueueing through a declared ingress port Tag the app root satisfies; `problem` is the outbound-only fault altitude. Durable signed EGRESS is `work/deliver`, never here. The folder imports `kernel`, `state`, `host`, `security`, `telemetry`; the domain map and seam record live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[GROUP](.planning/api/group.md): the `HttpApiGroup` contribution law — domain folders export groups as data, the app owns the assembled `HttpApi` value; the `RpcGroup`/`RpcServer` rows (http | websocket | worker) as the second contribution family; version-prefix + pagination convention vocabulary rows.
- [02]-[MIDDLEWARE](.planning/api/middleware.md): the closed middleware row family — auth + API-key admission (`security`), W3C trace continuation (`telemetry`), rate/quota + load-shed (concurrency caps, queue-depth 503/Retry-After), CORS + security-header, idempotency-key admission, locale negotiation (Accept-Language → the kernel `Locale` FiberRef row), and the FiberRef request/tenant context rows.
- [03]-[SERVE](.planning/api/serve.md): the serve rows — `NodeHttpServer` | `BunHttpServer` | the `toWebHandler` fetch row — plus the SPA/static-asset row with immutable-asset cache headers.
- [04]-[EMIT](.planning/api/emit.md): OpenApi document emission, the `HttpApiScalar` reference surface, and the `HttpApiClient` typed-SDK derivation from the app-assembled `HttpApi` value — spec, docs, and client never drift from the served contract.
- [05]-[DETAIL](.planning/problem/detail.md): the RFC 9457 problem-details mapping — the outbound-only fault altitude projecting the reconstructed `FaultDetail` and every folder's `Data.TaggedError` rail outward.
- [06]-[POLICY](.planning/problem/policy.md): the status/retry-after/redaction policy rows the detail mapping folds.
- [07]-[SOCKET](.planning/live/socket.md): WS/SSE endpoints over `state` Subscribables, SSE resume-token rows, and the protocol-handler mount port — an `HttpApp` port Tag the app root satisfies; the `store` EventLog sync server is the standing satisfier.
- [08]-[PRESENCE](.planning/live/presence.md): presence/subscription admission.
- [09]-[VERIFY](.planning/hook/verify.md): inbound webhook signature verify over `security/sign`.
- [10]-[ADMIT](.planning/hook/admit.md): replay protection + quota admission typed against the `work` fenced-quota rows via port; admitted hooks enqueue through the declared ingress port Tag a `work` queue or `store` journal Layer satisfies at the app root.
- [11]-[VERB](.planning/cli/verb.md): CLI verb contribution families under the one assembly law — the app assembles exactly one `Command` root from selected families; doctor/replay/inspect ship as the lib ops family, executing over `host/exec`.
- [12]-[RENDER](.planning/cli/render.md): CLI output rendering rows — `Doc` composition through `@effect/printer`(-ansi).

## [2]-[DOMAIN_PACKAGES]

The folder-local packages of the `# edge` catalog group. Versions live only in the `pnpm-workspace.yaml` catalog and are never pinned here; API catalogues live at the adjacent `.api/`.

[ENTRY_RPC]:
- `@effect/rpc` — the `RpcGroup` contribution families and the `RpcServer` protocol rows (http | websocket | worker), the second contribution family under the same assembly law as `HttpApi`.

[TERMINAL]:
- `@effect/cli` — the `Command`/args/options vocabulary behind the `cli/verb` contribution families; the app assembles exactly one `Command` root.
- `@effect/printer` — the `Doc` algebra the `cli/render` rows compose.
- `@effect/printer-ansi` — ANSI styling and terminal rendering of the composed `Doc` rows.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the registry lives in `libs/typescript/.planning/README.md` and the catalogues at `libs/typescript/.api/`.

- `effect`
- `@effect/platform` — the `HttpApi`/`HttpApiGroup` contracts, the middleware contracts, and the `HttpApp` mount vocabulary the whole folder builds on.
- `@effect/platform-node` — the `NodeHttpServer` serve row.
- `@effect/platform-bun` — the `BunHttpServer` serve row.
