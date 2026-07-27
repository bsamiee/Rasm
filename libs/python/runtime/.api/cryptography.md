# [PY_RUNTIME_API_CRYPTOGRAPHY]

`cryptography` owns the branch's AEAD envelope and key-wrap primitives: one-shot authenticated ciphers binding associated data into the tag, deterministic key-wrapping over a KEK, HKDF derivation, and constant-time comparison. Its `hazmat` tier exposes the raw construction and refuses every default — nonce width, key length, and associated-data binding are the caller's contract — so the composing owner fixes each as a policy value and this catalog names the members that contract spells.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `cryptography`
- package: `cryptography` (Apache-2.0 OR BSD-3-Clause)
- module: `cryptography`
- namespaces: `cryptography.exceptions`, `cryptography.hazmat.primitives.ciphers.aead`, `cryptography.hazmat.primitives.keywrap`, `cryptography.hazmat.primitives.kdf.hkdf`, `cryptography.hazmat.primitives.constant_time`, `cryptography.hazmat.primitives.hashes`
- abi: Rust extension over OpenSSL (`rust_openssl`), compiled
- rail: evidence custody

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: authenticated ciphers

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :----------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `AESGCM`           | class         | nonce-per-message AEAD; the estate's envelope and key-wrap construction   |
|  [02]   | `AESGCMSIV`        | class         | nonce-misuse-resistant GCM; a repeated nonce leaks equality alone         |
|  [03]   | `AESSIV`           | class         | deterministic AEAD taking no nonce; ciphertext repeats on equal plaintext |
|  [04]   | `AESCCM`           | class         | counter-with-CBC-MAC AEAD for constrained peers                           |
|  [05]   | `AESOCB3`          | class         | offset-codebook AEAD, single-pass over one key schedule                   |
|  [06]   | `ChaCha20Poly1305` | class         | software-fast AEAD where no AES instruction set exists                    |

[PUBLIC_TYPE_SCOPE]: fault types

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                                            |
| :-----: | :--------------------- | :-------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `InvalidTag`           | decrypt failure | authentication tag mismatch — wrong key, altered ciphertext, or foreign associated data |
|  [02]   | `InvalidUnwrap`        | unwrap failure  | AES key-wrap integrity check failed under the supplied KEK                              |
|  [03]   | `InvalidKey`           | derive failure  | key material failed a KDF `verify` comparison                                           |
|  [04]   | `UnsupportedAlgorithm` | backend refusal | the linked OpenSSL build carries no such primitive                                      |
|  [05]   | `AlreadyFinalized`     | lifecycle       | a finalized context reused for a second operation                                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: AEAD seal and open

| [INDEX] | [SURFACE]                                      | [SHAPE]     | [CAPABILITY]                                                |
| :-----: | :--------------------------------------------- | :---------- | :---------------------------------------------------------- |
|  [01]   | `AESGCM.generate_key(bit_length)`              | static      | mints 128/192/256-bit key material from the platform CSPRNG |
|  [02]   | `AESGCM(key)`                                  | constructor | binds one key; the instance carries no nonce state          |
|  [03]   | `AESGCM.encrypt(nonce, data, associated_data)` | instance    | returns ciphertext with the 16-byte tag appended            |
|  [04]   | `AESGCM.decrypt(nonce, data, associated_data)` | instance    | returns plaintext or raises `InvalidTag`                    |
|  [05]   | `AESSIV.encrypt(data, associated_data)`        | instance    | deterministic arm taking no nonce parameter                 |

[ENTRYPOINT_SCOPE]: key wrapping, derivation, and comparison

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                                       |
| :-----: | :----------------------------------------------------- | :------- | :----------------------------------------------------------------- |
|  [01]   | `aes_key_wrap(wrapping_key, key_to_wrap)`              | function | RFC-3394 wrap; input length must be a multiple of eight bytes      |
|  [02]   | `aes_key_unwrap(wrapping_key, wrapped_key)`            | function | RFC-3394 unwrap; raises `InvalidUnwrap` on an integrity miss       |
|  [03]   | `aes_key_wrap_with_padding(wrapping_key, key_to_wrap)` | function | RFC-5649 wrap admitting any input length                           |
|  [04]   | `HKDF(algorithm, length, salt, info)`                  | class    | extract-and-expand derivation over one input keying material       |
|  [05]   | `HKDF.derive(key_material)`                            | instance | one-shot derive; `verify` re-derives and compares in constant time |
|  [06]   | `constant_time.bytes_eq(a, b)`                         | function | timing-invariant byte comparison for tags and digests              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- nonce law: `AESGCM` authenticates but never generates a nonce, so the composing owner mints 96 random bits per message from `secrets.token_bytes` and carries them beside the ciphertext; a repeated nonce under one key destroys both confidentiality and authenticity, which is what makes the nonce a stored field rather than a derived one.
- binding law: `associated_data` authenticates without encrypting, so an envelope binds its own identity coordinate there and a ciphertext relocated onto another identity fails its tag instead of opening under a live key.
- tag law: `decrypt` distinguishes nothing about WHY authentication failed — a wrong key, an altered ciphertext, and foreign associated data all raise `InvalidTag` — so an owner that must separate erasure from tampering decides absence from its own key ledger before it ever calls `decrypt`.
- length law: `aes_key_wrap` refuses input whose length is not a multiple of eight bytes, so wrapping arbitrary material rides the padded RFC-5649 arm or an AEAD wrap that carries its own nonce.

[STACKING]:
- `msgspec`(`libs/python/.api/msgspec.md`): ciphertext, nonce, and wrapped key ride `bytes` fields on frozen `Struct` rows, so a sealed envelope encodes and persists through the same codec every other wire shape crosses and no ciphertext is re-framed as text.
- stdlib `secrets`: `token_bytes` supplies every nonce and salt this rail consumes, because a `random` draw is predictable and reuses state across a fork.

[RAIL_LAW]:
- Package: `cryptography`
- Owns: AEAD envelope sealing, KEK key wrapping, HKDF derivation, and constant-time comparison
- Accept: one-shot `encrypt`/`decrypt` under a caller-minted nonce with the identity coordinate bound as associated data
- Reject: a reused nonce under one key, a hand-rolled tag comparison where `constant_time.bytes_eq` decides, a `random` draw for nonce or salt material, and an `InvalidTag` folded to absence where the key ledger owns that verdict
