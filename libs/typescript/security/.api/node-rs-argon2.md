# [TS_SECURITY_API_NODE_RS_ARGON2]

`@node-rs/argon2` owns argon2id credential hashing at rest for `security/sign/crypto`: a NAPI native addon whose async members run the Rust hash off the libuv threadpool under an `AbortSignal`. One `Options` carrier holds the whole cost surface, so a named policy row per credential class replaces call-site knobs, and the PHC string `hash` emits self-describes salt and cost so `verify` needs no options.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@node-rs/argon2`
- package: `@node-rs/argon2` (MIT)
- module: CommonJS root entry; the per-platform `.node` loader resolves through the root import, never a deep subpath
- runtime: node-only — the Rust `argon2` crate ships as a per-target prebuilt addon with a `wasm32-wasi` recovery, which makes `sign/crypto` a node boundary
- abi: the arch-matched prebuilt resolves at install; a missing or ABI-mismatched binary fails at load, never at typecheck
- rail: `security/sign` — the credential-digest primitive inside the crypto owner

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the single cost carrier and the two bounded vocabularies it discriminates on

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :---------- | :------------ | :----------------------------------- |
|  [01]   | `Options`   | interface     | the whole cost and policy surface    |
|  [02]   | `Algorithm` | enum          | the argon2 variant vocabulary        |
|  [03]   | `Version`   | enum          | the argon2 format-version vocabulary |

[OPTIONS]: `memoryCost` `timeCost` `outputLen` `parallelism` `algorithm` `version` `secret` `salt`
[ALGORITHM]: `Argon2d` `Argon2i` `Argon2id`
[VERSION]: `V0x10` `V0x13`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: mint and check on an async rail and its blocking mirror; `Options` is optional on every member, and each async member takes a trailing optional `AbortSignal`

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------------------- | :------ | :----------------------------------------- |
|  [01]   | `hash(string\|Uint8Array, Options) -> Promise<string>`                        | static  | mint the self-describing PHC digest        |
|  [02]   | `verify(string\|Uint8Array, string\|Uint8Array, Options) -> Promise<boolean>` | static  | constant-time check of a stored PHC digest |
|  [03]   | `hashRaw(string\|Uint8Array, Options) -> Promise<Buffer>`                     | static  | raw digest bytes for KDF derivation        |
|  [04]   | `hashSync(string\|Uint8Array, Options) -> string`                             | static  | blocking mint on a boot or CLI path        |
|  [05]   | `hashRawSync(string\|Uint8Array, Options) -> Buffer`                          | static  | blocking raw digest on a boot path         |
|  [06]   | `verifySync(string\|Uint8Array, string\|Uint8Array, Options) -> boolean`      | static  | blocking check on a boot or CLI path       |

- `verify`: throws on a structurally malformed stored digest, so `false` is the auth-fail arm and the throw a corrupt-record fault.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Async members honor `AbortSignal` off the threadpool, so a request-scoped hash cancels with its fiber; a `*Sync` member on a request fiber starves the event loop.
- `Options` is the entire tuning surface: `memoryCost`, `timeCost`, and `parallelism` set the security-versus-latency dial and `outputLen` the digest width, fixed as a named cost row per credential class.
- `hash` emits a PHC string embedding the salt, the variant, and every cost parameter, so `verify` reconstructs the KDF from the stored string alone and rehash-on-login folds those embedded parameters against the current policy row, re-`hash`ing on drift.
- `secret` mixes a server-side pepper into the digest, so a stolen digest resists cracking without the app key; `salt` defaults to a fresh CSPRNG value and pins only for deterministic `hashRaw` derivation.

[STACKING]:
- `effect`(`.api/effect.md`): `Effect.tryPromise({ try: (signal) => verify(stored, pw, opts, signal), catch })` threads the fiber interrupt into `AbortSignal`; cost rows ride `Config`/`Layer` values, the rehash decision is a `Match` fold, and a malformed-digest throw becomes a `Data.TaggedError`.
- `@oslojs/encoding`(`.api/oslojs-encoding.md`): `hashRaw` bytes and the `secret` pepper render at rest through `encodeHexLowerCase` and parse back through `decodeHex`; the `hash` PHC string stores verbatim and never re-encodes.
- `@oslojs/crypto`(`.api/oslojs-crypto.md`): `constantTimeEqual` guards fixed-length token lookup where argon2 is the wrong tool; a stored argon2 digest checks through `verify` alone, so the two never double-wrap.
- `otplib`(`.api/otplib.md`): recovery and backup codes minted by `generateSecret`/`generateRandomString` take `hash` for digest-at-rest and `verify` for redemption — otplib owns no credential-storage row.
- `security/sign/crypto` (in-folder owner): argon2 sits beside HMAC signing and the AES-GCM envelope under one owner; `secret/material` derives key material through it, and `authn/apikey` delegates its digest-at-rest, storing the `hash` PHC string prefix-indexed for `byHash` resolution while the plaintext shows once.

[LOCAL_ADMISSION]:
- `Algorithm.Argon2id` with `Version.V0x13` on a named cost `Options` row per credential class fixes both the variant and the cost.
- Password, `secret` pepper, and digest are the `Redacted` values on this surface.

[RAIL_LAW]:
- Package: `@node-rs/argon2`
- Owns: argon2id credential hashing at rest — PHC mint, raw digest bytes, constant-time verify, the blocking mirrors, the single `Options` cost carrier, and the `Algorithm`/`Version` vocabularies
- Accept: async members under `Effect.tryPromise` with `AbortSignal` threading, `Argon2id` with `V0x13` on named `Options` cost rows, verbatim PHC storage, a design-owned rehash fold, `Redacted` credential material
- Reject: call-site cost knobs, a `*Sync` member on a request fiber, a hand-rolled constant-time compare of an argon2 digest, an import outside `sign/crypto`
