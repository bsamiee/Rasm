# [SECURITY_API_NODE_RS_ARGON2]

`@node-rs/argon2` is the argon2 password-hashing primitive the `security/sign/crypto` design composes for credential-at-rest — a NAPI-RS native addon (Rust `argon2` crate) exposing `hash`/`verify` as promise-returning members off the libuv threadpool, with `hashRaw` for raw digest bytes and `*Sync` mirrors for non-Effect boot paths. The whole cost/policy surface is ONE `Options` carrier — `memoryCost`/`timeCost`/`parallelism`/`outputLen`/`algorithm`/`version`/`secret`/`salt` — so the design pins a single named policy row per credential class rather than scattering knobs across call sites. `hash` emits a self-describing PHC string that embeds the salt and every cost parameter, so `verify` needs no options to check a stored digest; the argon2id variant (`Algorithm.Argon2id`, `Version.V0x13`) is the hybrid GPU-and-side-channel-resistant default the design fixes. `secret` is a server-side pepper (a `Redacted` key injected at layer construction), `salt` defaults to a fresh CSPRNG value, and both async members accept an `AbortSignal` so a request-scoped hash cancels with its fiber. The package is node-only (native `.node` binary per platform, wasm32-wasi fallback), which is why `sign/` carries the node-only-subpath ban.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@node-rs/argon2`
- package: `@node-rs/argon2` (2.0.2, MIT, © NAPI-RS / node-rs)
- module format: CommonJS (`main: index.js`, `engines.node >= 10`); the `.node` loader is selected by the root package via per-platform `optionalDependencies`, imported through the `@node-rs/argon2` root — no deep subpaths
- runtime target: node-only — a native NAPI addon linking the Rust `argon2` crate, prebuilt for 14 targets (`darwin-arm64`/`-x64`, `linux-x64/arm64-gnu/musl`, `win32-x64/arm64/ia32`, `android`, `freebsd`, `armv7`) with a `wasm32-wasi` fallback; no browser build, so `sign/crypto` is node-boundary despite jose's isomorphism
- asset: native addon (`.node` per-platform binary + `.js` loader + `.d.ts`); the ABI is the real gate — a missing/mismatched prebuilt or Node ABI break is a load-time failure, not a type error, so the install must resolve the arch-matched optional dependency
- rail: `security/sign` — the argon2 hashing primitive within the crypto owner (admitted in `sign/` only, node-only subpath; catalogued at the folder tier)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the single cost/policy carrier and its bounded vocabularies
- rail: shapes

| [INDEX] | [SYMBOL]                                                                | [TYPE_FAMILY]     | [CONSUMER]                                                          |
| :-----: | :---------------------------------------------------------------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `Options` (`memoryCost`/`timeCost`/`parallelism`/`outputLen`/`algorithm`/`version`/`secret`/`salt`) | cost policy | `sign/crypto` — the one policy carrier; the design pins named rows (login vs api-key) as `Config`/`Layer` values, never inline knobs |
|  [02]   | `Algorithm` (`Argon2d` = 0, `Argon2i` = 1, `Argon2id` = 2)              | variant enum      | `sign/crypto` — `Argon2id` is the only admitted arm (hybrid GPU + side-channel resistance) |
|  [03]   | `Version` (`V0x10` = 0, `V0x13` = 1)                                    | format-version enum | `sign/crypto` — `V0x13` (argon2 v1.3) is the pinned default; `V0x10` exists only to verify legacy digests |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: hash and verify — the async rail (default) and its sync mirror
- rail: rails-and-effects

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY]  | [CONSUMER]                                                          |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `hash(password: string \| Uint8Array, options?: Options, abortSignal?: AbortSignal)` → `Promise<string>` | hash → PHC | `sign/crypto` ← `authn/apikey` (digest-at-rest), `secret/material` — the credential mint; PHC string stored verbatim |
|  [02]   | `verify(hashed: string \| Uint8Array, password: string \| Uint8Array, options?: Options, abortSignal?: AbortSignal)` → `Promise<boolean>` | verify | `sign/crypto` — constant-time check; reads cost params from the PHC string, returns `false` on mismatch (throws only on a malformed hash) |
|  [03]   | `hashRaw(password, options?, abortSignal?)` → `Promise<Buffer>`                          | hash → raw      | `sign/crypto` — raw digest bytes when the design owns its own salt/encoding (KDF-style key derivation), not the PHC envelope |
|  [04]   | `hashSync` / `hashRawSync` / `verifySync` (no `AbortSignal`)                             | sync mirror     | `sign/crypto` — boot/CLI paths outside the Effect rail only; never on a request fiber |

## [04]-[IMPLEMENTATION_LAW]

[ARGON2_TOPOLOGY]:
- The four async members run the Rust hash off the libuv threadpool and honor `AbortSignal`, so a per-request hash is cancellable and does not block the event loop. The `*Sync` mirrors block the calling thread and exist only for boot/CLI paths; a sync hash on a request fiber is a starvation defect.
- `Options` is the entire tuning surface. The design fixes `algorithm: Argon2id` + `version: V0x13` and pins a named cost row per credential class (an interactive-login row and a higher-cost api-key row) as a `Layer`/`Config` value — `memoryCost`/`timeCost`/`parallelism` are the security-vs-latency dial, `outputLen` the digest width. Cost is never chosen at a call site.
- `hash` returns a PHC string (`$argon2id$v=19$m=...,t=...,p=...$salt$hash`) that self-describes the salt and every parameter, so `verify` reconstructs the KDF from the stored string with no options. Rehash-on-login is therefore a design fold, not a package member: after a successful `verify`, the design compares the PHC-embedded parameters to the current policy row and re-`hash`es when they drift — `@node-rs/argon2` exposes no `needsRehash`, so the fold owns it.
- `secret` is a server-side pepper: a `Redacted` key mixed into the hash so a stolen digest is uncrackable without the app key, held at layer construction and unwrapped only into the call. `salt` defaults to a fresh CSPRNG value per hash; the design overrides it only for deterministic KDF derivation via `hashRaw`.
- `verify` is constant-time and returns a boolean; it throws only when the stored hash is structurally malformed. The design treats a `false` as an ordinary auth-fail arm and the throw as a corrupt-record fault, never conflating the two.

[STACKS_WITH]:
- `effect` (`.api/effect.md`): `Effect.tryPromise({ try: (signal) => argon2.verify(stored, pw, opts, signal), catch })` lifts hash/verify onto the rail and threads the fiber's interrupt through the `AbortSignal`, so canceling the request cancels the hash; `Redacted` holds the password, the pepper (`secret`), and the resulting digest, unwrapped only at the argon2 call; the named cost `Options` rows are `Config`/`Layer` values; the rehash-needed decision is a `Match` fold over the PHC-embedded params versus the policy row; a malformed-hash throw becomes a `Data.TaggedError`.
- `security/sign/crypto` (in-folder owner): argon2 is one primitive inside `sign/crypto` alongside HMAC webhook signing and the AES-GCM envelope; `sign/crypto` is the single owner `authn/apikey` delegates its digest-at-rest to and `secret/material` derives key material through, so no folder re-imports argon2 directly.
- `@oslojs/crypto` (`.api/oslojs-crypto.md`): the sibling's `constantTimeEqual` (subpath `@oslojs/crypto/subtle`) guards fixed-length token/digest comparisons where argon2 is the wrong tool (opaque session-token lookup by hash) — argon2's own `verify` is already constant-time, so a stored argon2 digest is checked by `verify`, never re-compared through oslo; the two never double-wrap.
- `authn/apikey` (in-folder consumer): machine-credential secrets are minted, then their `hash` PHC string is stored and prefix-indexed for `byHash` resolution; the plaintext key is shown once and never persisted.

[LOCAL_ADMISSION]:
- Use the async `hash`/`verify` wrapped in `Effect.tryPromise` with the fiber's `AbortSignal`; never a `*Sync` member on a request fiber, and never a bare `Promise` in domain logic.
- Use `Algorithm.Argon2id` + `Version.V0x13` with a named cost `Options` row per credential class; never pick cost parameters at a call site and never a non-id variant.
- Store the PHC string from `hash` verbatim and let `verify` read its params; own rehash-on-login as a `Match` fold over the embedded parameters, since the package has no `needsRehash`.
- Hold the password, the `secret` pepper, and the digest in `Redacted`; unwrap only into the argon2 call, and never log any of them.
- Route every credential digest through `sign/crypto`; never re-import argon2 in `authn`/`secret` folders.

[RAIL_LAW]:
- Package: `@node-rs/argon2`
- Owns: argon2id credential hashing at rest — `hash` (PHC), `hashRaw` (bytes), `verify` (constant-time), their sync mirrors, the single `Options` cost carrier, and the `Algorithm`/`Version` vocabularies
- Accept: async `hash`/`verify` under `Effect.tryPromise` with `AbortSignal` threading, `Argon2id`+`V0x13` with named `Options` cost rows as `Layer`/`Config` values, PHC-string storage, a design-owned rehash fold, `Redacted` password/pepper/digest, the arch-matched native prebuilt resolved at install
- Reject: a `*Sync` member on a request fiber, call-site cost knobs, a non-id variant, a hand-rolled constant-time compare of an argon2 digest, a re-import outside `sign/crypto`, a password/pepper/digest outside `Redacted`, treating a `false` verify as an error rather than an auth-fail arm
