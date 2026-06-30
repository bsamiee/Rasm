# [RASM_PERSISTENCE_API_AZURE_KEYVAULT]

`Azure.Security.KeyVault.Keys` supplies the `KeyClient` for vault-side key CRUD and the
`CryptographyClient` for key-wrap and envelope operations over Azure Key Vault and Managed HSM.
It carries the `Keys.Cryptography` algorithm vocabularies (`KeyWrapAlgorithm`, `EncryptionAlgorithm`, `SignatureAlgorithm`) plus the `KeyVaultKey`/`JsonWebKey` material model. It is the `KmsProvider.Azure` arm of TWO Persistence delegate surfaces the `Element/identity#AUTHORITY` `KmsProvider` `[SmartEnum<string>]` axis selects (the axis is owned by `Element/identity#AUTHORITY`, not a `Store/encryption` page — that page does not exist): the ENVELOPE `EnvelopeKeyring` and the SIGNING `Element/identity#AUTHORITY` `SigningKeyring`. The `CryptographyClient` is the single admitted KMS provider whose envelope arm exposes a **native** `WrapKey`/`UnwrapKey` verb pair (`RSA-OAEP-256` over the vault CMK), so the Azure arm wraps a DEK directly rather than through the `Encrypt`/`Decrypt`-as-wrap shim the AWS-KMS and GCP-KMS arms (`GenerateDataKey`/`Decrypt`/`ReEncrypt`) require, and whose signing arm exposes a first-class `Sign`/`Verify` (and `SignData`/`VerifyData`) over the `SignatureAlgorithm` axis the `SigningKeyring` binds. The `CryptographyClient(JsonWebKey)` local-only constructor is the path for an offline/cached wrap or verify that performs no vault round-trip.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Azure.Security.KeyVault.Keys`
- package: `Azure.Security.KeyVault.Keys`
- version: `4.10.0` (`Directory.Packages.props`); license: MIT (Azure SDK)
- assembly: `Azure.Security.KeyVault.Keys` (`lib/net10.0`, consumer-bound; `net8.0`/`netstandard2.0` fallbacks present)
- namespace: `Azure.Security.KeyVault.Keys`, `Azure.Security.KeyVault.Keys.Cryptography`
- depends: `Azure.Core` (pipeline, `Response<T>`, `Pageable<T>`, `TokenCredential`); pairs with `Azure.Identity` for the credential at composition
- service-version: `KeyClientOptions.ServiceVersion.V2025_07_01` is the `LatestVersion` default; pin lower only to target an older vault API
- asset: runtime library
- rail: encryption (the DEK envelope arm), signing (the `Element/identity#AUTHORITY` `SigningKeyring` arm)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and cryptography family
- rail: encryption

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]        | [RAIL]                                                      |
| :-----: | :----------------------------- | :------------------- | :--------------------------------------------------------- |
|  [01]   | `KeyClient`                    | management client    | key CRUD, backup, rotation, release over the vault         |
|  [02]   | `CryptographyClient`           | cryptography client  | wrap, unwrap, encrypt, decrypt, sign, verify; `IKeyEncryptionKey` |
|  [03]   | `KeyResolver`                  | resolver client      | `IKeyEncryptionKeyResolver` → `CryptographyClient` per key URI |
|  [04]   | `KeyClientOptions`             | client options       | `ServiceVersion` and pipeline policy                       |
|  [05]   | `CryptographyClientOptions`    | client options       | service version for the remote cryptography client         |
|  [06]   | `LocalCryptographyClientOptions` | client options     | options for the offline `JsonWebKey` cryptography constructor |
|  [07]   | `KeyVaultKey`                  | key resource         | `Key` (`JsonWebKey`) plus `Properties` (`KeyProperties`) carrier |
|  [08]   | `JsonWebKey`                   | key material         | JWK key type, operations, curve, `ToRSA`/`ToECDsa`/`ToAes` material |
|  [09]   | `KeyProperties`                | key metadata         | `Name`, `Id` (`Uri`), `Version`, `Managed`, `ReleasePolicy` |
|  [10]   | `KeyVaultKeyIdentifier`        | identifier value     | parses `SourceId`/`VaultUri`/`Name`/`Version` from a key URI |
|  [11]   | `DeletedKey`                   | soft-delete resource | recovery id and scheduled purge metadata                   |
|  [12]   | `KeyRotationPolicy`            | rotation policy      | `LifetimeActions`, `ExpiresIn`; read/written via `KeyClient` |
|  [13]   | `ReleaseKeyResult`             | release result       | secure-key-release attestation payload                     |
|  [14]   | `RSAKeyVault`                  | crypto bridge        | `RSA` over the vault key from `CryptographyClient.CreateRSA` |

[PUBLIC_TYPE_SCOPE]: algorithm and parameter vocabulary
- rail: encryption

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :-------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `KeyWrapAlgorithm`    | algorithm value   | key-wrap algorithm selector                     |
|  [02]   | `EncryptionAlgorithm` | algorithm value   | encrypt/decrypt algorithm selector              |
|  [03]   | `SignatureAlgorithm`  | algorithm value   | sign/verify algorithm selector: `ES256`/`ES384`/`ES512`/`ES256K`, `PS256`/`PS384`/`PS512`, `RS256`/`RS384`/`RS512`, `HS256`/`HS384`/`HS512` (the `SigningKeyring` binds the ES/PS/RS rows) |
|  [04]   | `KeyType`             | key-type value    | `Rsa`, `RsaHsm`, `Ec`, `EcHsm`, `Oct`, `OctHsm` |
|  [05]   | `KeyOperation`        | operation value   | permitted JWK operation token                   |
|  [06]   | `EncryptParameters`   | parameter carrier | algorithm-specific encrypt inputs               |
|  [07]   | `DecryptParameters`   | parameter carrier | algorithm-specific decrypt inputs               |

[PUBLIC_TYPE_SCOPE]: operation result carriers
- rail: encryption

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                                            |
| :-----: | :-------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `WrapResult`    | result value  | `EncryptedKey`, `KeyId`, `Algorithm` (`KeyWrapAlgorithm`)         |
|  [02]   | `UnwrapResult`  | result value  | unwrapped `Key`, `KeyId`, `Algorithm` (`KeyWrapAlgorithm`)        |
|  [03]   | `EncryptResult` | result value  | `Ciphertext`, `Iv`, `AuthenticationTag`, `AdditionalAuthenticatedData`, `KeyId`, `Algorithm` |
|  [04]   | `DecryptResult` | result value  | `Plaintext`, `KeyId`, `Algorithm` (`EncryptionAlgorithm`)        |
|  [05]   | `SignResult`    | result value  | `Signature`, `KeyId`, `Algorithm` (`SignatureAlgorithm`)         |
|  [06]   | `VerifyResult`  | result value  | `IsValid`, `KeyId`, `Algorithm` (`SignatureAlgorithm`)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: key-wrap and envelope cryptography
- rail: encryption

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY]   | [RAIL]                                                  |
| :-----: | :----------------------------------------------------- | :--------------- | :----------------------------------------------------- |
|  [01]   | `CryptographyClient(Uri keyId, TokenCredential, CryptographyClientOptions?)` | remote ctor | binds a client to one key identifier (vault round-trip) |
|  [02]   | `CryptographyClient(JsonWebKey, LocalCryptographyClientOptions?)` | local ctor | offline cryptography over cached JWK material; no vault call |
|  [03]   | `WrapKey(KeyWrapAlgorithm, byte[] key, ct)`            | wrap             | wraps a data-encryption key to `WrapResult`            |
|  [04]   | `UnwrapKey(KeyWrapAlgorithm, byte[] encryptedKey, ct)` | unwrap           | unwraps a key to `UnwrapResult`                        |
|  [05]   | `Encrypt(EncryptionAlgorithm, byte[] plaintext, ct)`  | encrypt          | direct encrypt to `EncryptResult`                      |
|  [06]   | `Encrypt(EncryptParameters, ct)`                      | encrypt          | parameterized encrypt with `Iv`/`Aad`                  |
|  [07]   | `Decrypt(EncryptionAlgorithm, byte[] ciphertext, ct)` | decrypt          | direct decrypt to `DecryptResult`                      |
|  [08]   | `Decrypt(DecryptParameters, ct)`                      | decrypt          | parameterized decrypt with `Iv`/`Aad`                  |
|  [09]   | `Sign(SignatureAlgorithm, byte[] digest, ct)`         | sign             | signs a precomputed digest                             |
|  [10]   | `Verify(SignatureAlgorithm, byte[] digest, sig, ct)`  | verify           | verifies a digest signature                            |
|  [11]   | `SignData(SignatureAlgorithm, byte[]\|Stream, ct)`    | sign             | hashes then signs data                                 |
|  [12]   | `VerifyData(SignatureAlgorithm, byte[]\|Stream, sig)` | verify           | hashes then verifies data                              |
|  [13]   | `CreateRSA(ct)` / `CreateRSAAsync(ct)`                | crypto bridge    | downloads the public key, returns an `RSAKeyVault` (`RSA`) |
|  [14]   | `KeyResolver(TokenCredential).Resolve(Uri keyId, ct)` | resolver         | resolves a key URI to a bound `CryptographyClient`     |
|  [15]   | `CryptographyClient.KeyId`                            | identity         | the bound key identifier (`IKeyEncryptionKey.KeyId`)   |

[ENTRYPOINT_SCOPE]: key lifecycle management
- rail: encryption

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]   | [RAIL]                                      |
| :-----: | :------------------------------------------------- | :--------------- | :------------------------------------------ |
|  [01]   | `KeyClient(Uri vaultUri, TokenCredential)`         | constructor      | binds a client to one vault `Uri`           |
|  [02]   | `CreateKey(name, KeyType, CreateKeyOptions?, ct)`  | create           | creates a key of the given `KeyType`        |
|  [03]   | `CreateRsaKey(CreateRsaKeyOptions, ct)`            | create           | RSA-specific key create                     |
|  [04]   | `CreateEcKey(CreateEcKeyOptions, ct)`              | create           | EC-specific key create                      |
|  [05]   | `CreateOctKey(CreateOctKeyOptions, ct)`            | create           | symmetric (Managed HSM) key create          |
|  [06]   | `GetKey(name, version?, ct)`                       | read             | returns `Response<KeyVaultKey>`             |
|  [07]   | `GetKeyAttestation(name, version?, ct)`            | read             | public key plus attestation blob (`KeyVaultKey`) |
|  [08]   | `GetPropertiesOfKeys(ct)`                          | list             | `Pageable<KeyProperties>` over all keys     |
|  [09]   | `GetPropertiesOfKeyVersions(name, ct)`             | list             | `Pageable<KeyProperties>` over key versions |
|  [10]   | `UpdateKeyProperties(KeyProperties, keyOps?, ct)`  | update           | updates attributes and permitted operations |
|  [11]   | `ImportKey(name, JsonWebKey, ct)`                  | import           | imports external key material               |
|  [12]   | `ImportKey(ImportKeyOptions, ct)`                  | import           | options-driven import with HSM flag         |
|  [13]   | `StartDeleteKey(name, ct)`                         | delete           | `DeleteKeyOperation` long-running poller    |
|  [14]   | `GetDeletedKey(name, ct)` / `GetDeletedKeys(ct)`   | soft-delete read | reads recoverable `DeletedKey` resources    |
|  [15]   | `StartRecoverDeletedKey(name, ct)`                 | recover          | `RecoverDeletedKeyOperation` poller         |
|  [16]   | `PurgeDeletedKey(name, ct)`                        | purge            | irreversible removal from soft-delete       |
|  [17]   | `BackupKey(name, ct)` / `RestoreKeyBackup(byte[])` | backup           | opaque vault-portable key blob round-trip   |
|  [18]   | `RotateKey(name, ct)`                              | rotate           | forces a new key version                    |
|  [19]   | `GetKeyRotationPolicy(keyName, ct)` / `UpdateKeyRotationPolicy(keyName, KeyRotationPolicy, ct)` | rotation policy | reads and sets `KeyRotationPolicy` |
|  [20]   | `GetRandomBytes(count, ct)`                        | rng              | Managed HSM hardware random bytes           |
|  [21]   | `ReleaseKey(name, targetAttestationToken, ct)` / `ReleaseKey(ReleaseKeyOptions, ct)` | release | secure-key-release to `ReleaseKeyResult` |
|  [22]   | `GetCryptographyClient(keyName, keyVersion?)`      | factory          | derives a `CryptographyClient` for one key  |

[ENTRYPOINT_SCOPE]: algorithm selectors and identifier parse
- rail: encryption

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]    | [RAIL]                                          |
| :-----: | :---------------------------------------------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `KeyWrapAlgorithm.RsaOaep256` / `RsaOaep` / `Rsa15`         | RSA wrap value    | RSA key-wrap algorithm constants (`RSA-OAEP-256`/`RSA-OAEP`/`RSA1_5`) |
|  [02]   | `KeyWrapAlgorithm.A128KW` / `A192KW` / `A256KW`             | AES wrap value    | AES key-wrap algorithm constants               |
|  [03]   | `KeyWrapAlgorithm.CkmAesKeyWrap` / `CkmAesKeyWrapPad`       | CKM wrap value    | PKCS#11 AES key-wrap constants                 |
|  [04]   | `EncryptionAlgorithm.RsaOaep256` / `A256Gcm` / `A256CbcPad` | encrypt value     | encrypt/decrypt algorithm constants            |
|  [05]   | `EncryptParameters.{Rsa15,RsaOaep,RsaOaep256}Parameters(plaintext)` | parameter factory | RSA encrypt parameter construction      |
|  [06]   | `EncryptParameters.A{128,192,256}GcmParameters(plaintext, aad?)` | parameter factory | AES-GCM encrypt parameters with optional AAD |
|  [07]   | `EncryptParameters.A{128,192,256}{Cbc,CbcPad}Parameters(plaintext, iv?)` | parameter factory | AES-CBC / AES-CBC-PAD encrypt parameters |
|  [08]   | `DecryptParameters.*` (mirror set)                          | parameter factory | algorithm-specific decrypt inputs with `Iv`/`Aad` |
|  [09]   | `KeyVaultKeyIdentifier.TryCreate(Uri, out identifier)`      | identifier parse  | non-throwing split of a key URI                |
|  [10]   | `KeyVaultKeyIdentifier.SourceId` / `VaultUri` / `Name` / `Version` | identifier value | parsed key URI components               |

## [04]-[IMPLEMENTATION_LAW]

[KEYVAULT_TOPOLOGY]:
- Two namespaces carry the public surface: `Azure.Security.KeyVault.Keys` for management and `Azure.Security.KeyVault.Keys.Cryptography` for operations. The whole client surface rides `Azure.Core`: `Response<T>`/`Pageable<T>`/`AsyncPageable<T>` carriers, the `TokenCredential` from `Azure.Identity`, and `RequestFailedException` for transport faults — convert that exception to the page error rail once at the boundary, never inside the keyring delegates.
- `KeyClient` binds to one vault `Uri` plus a `TokenCredential`; `CryptographyClient` binds to one key identifier `Uri` (remote) or one `JsonWebKey` (local).
- `CryptographyClient` implements `IKeyEncryptionKey`; the remote constructor performs vault round-trips while the `CryptographyClient(JsonWebKey)` / `(JsonWebKey, LocalCryptographyClientOptions)` constructors perform the operation **locally** over cached material with no service call — the same surface, gated by which constructor built it. `KeyResolver` (`IKeyEncryptionKeyResolver`) resolves a key URI to a bound `CryptographyClient` on demand.
- NATIVE WRAP: unlike AWS KMS and GCP KMS (which expose no wrap verb, forcing `Encrypt`/`Decrypt`/`ReEncrypt`-as-wrap), `CryptographyClient.WrapKey(KeyWrapAlgorithm, dek)` / `UnwrapKey` is a first-class vault verb. The Azure `EnvelopeKeyring` arm therefore wraps a DEK directly with `RSA-OAEP-256`; the AWS/GCP "KMS has no native wrap verb" law is those arms', not this one.
- SIGNING: `CryptographyClient.Sign(SignatureAlgorithm, digest, ct)` / `Verify(SignatureAlgorithm, digest, signature, ct)` is the first-class signing verb the `Element/identity#AUTHORITY` `SigningKeyring` `Sign`/`Verify` arm binds — `Sign` takes the PRECOMPUTED `OpDigest` (the seam already hashed the op bytes), `SignData`/`VerifyData` the hash-then-sign convenience the seam does not use; the `SignatureAlgorithm.WireName` maps `ES256`↔`es256`, `PS256`↔`ps256`, `RS256`↔`rs256` (and the 384/512 widths), so the keyring delegate edge is the only place the Azure algorithm dialect lives, and `VerifyResult.IsValid` is the boolean the keyring lifts to `Authentic`/`Forged`.
- `KeyWrapAlgorithm`/`EncryptionAlgorithm`/`SignatureAlgorithm` are extensible `readonly struct` value types with static well-known members; equality is string-value based and a string converts implicitly — a future algorithm needs no library upgrade, just the wire string.
- `KeyType` carries `Rsa`/`RsaHsm`/`Ec`/`EcHsm`/`Oct`/`OctHsm` (wire `RSA`/`RSA-HSM`/`EC`/`EC-HSM`/`oct`/`oct-HSM`); `KeyOperation` carries `Encrypt`, `Decrypt`, `Sign`, `Verify`, `WrapKey`, `UnwrapKey`, `Import`.
- `WrapResult.EncryptedKey` is the wrapped key bytes; `UnwrapResult.Key` is the recovered key bytes; both carry `KeyId` and `Algorithm` for receipt round-trip. `EncryptResult` additionally carries `Iv`, `AuthenticationTag`, and `AdditionalAuthenticatedData` for the AES-GCM authenticated-encryption path.
- `KeyVaultKey` is `Key` (`JsonWebKey`) plus `Properties` (`KeyProperties`); `KeyProperties.Id` (a `Uri`) and `KeyProperties.Version` pin the exact key version a wrap was performed against. `JsonWebKey` round-trips to BCL primitives via `ToRSA`/`ToECDsa`/`ToAes` and is constructed from `RSA`/`ECDsa`/`Aes` providers.
- Sync members have async twins (`WrapKeyAsync`, `UnwrapKeyAsync`, `CreateKeyAsync`, `GetKeyAsync`); delete and recover are long-running operations (`DeleteKeyOperation`, `RecoverDeletedKeyOperation`).
- `KeyVaultKeyIdentifier.TryCreate` is the non-throwing URI split (`SourceId`/`VaultUri`/`Name`/`Version`); it never validates that the URI references a live vault.
- Secure-key-release: `GetKeyAttestation` returns the key plus its attestation blob and `ReleaseKey(name, targetAttestationToken)` / `ReleaseKey(ReleaseKeyOptions)` exports an exportable key to a `ReleaseKeyResult` under a confidential-compute attestation token — the path for moving a key into a TEE, distinct from the DEK-wrap rail.

[LOCAL_ADMISSION]:
- This package is the `azure` arm (the `KmsProvider.Azure` row, `NativeWrap: true`) of the `Element/identity#AUTHORITY` `KmsProvider` `[SmartEnum<string>]`. Its envelope `EnvelopeKeyring(Mint, Unwrap, Rewrap, Probe)` delegate quartet binds: `Mint` calls `CryptographyClient.WrapKey(KeyWrapAlgorithm.RsaOaep256, dek)` over a freshly generated DEK, `Unwrap` calls `UnwrapKey` and zeroes the recovered `UnwrapResult.Key` after the local bind, `Rewrap` re-wraps the persisted blob under a `destinationKeyId` (the native wrap verb means rotation is a `WrapKey` against the new version, not a decrypt-then-encrypt round trip, and the plaintext DEK never re-enters managed memory), and `Probe` resolves the key lifecycle through `KeyClient.GetKey` `KeyProperties.Enabled` → `KeyState`. The master key never leaves the vault.
- The signing `Element/identity#AUTHORITY` `SigningKeyring` `Sign`/`Verify` pair binds the SAME `CryptographyClient` against a separate asymmetric signing key: `Sign` calls `CryptographyClient.Sign(SignatureAlgorithm, opDigest, ct)` over the precomputed `OpDigest`, `Verify` calls `Verify(SignatureAlgorithm, opDigest, signature, ct)` reading `VerifyResult.IsValid`; the signing key is distinct from the envelope CMK, both resolved through the same per-open `SecretLease` handle (`Element/identity#AUTHORITY`).
- `KeyClient` owns the master-key lifecycle (`CreateRsaKey`, `RotateKey`, `GetKeyRotationPolicy`/`UpdateKeyRotationPolicy` with a `KeyRotationPolicy.ExpiresIn` cadence); the persistence path consumes a `CryptographyClient` derived through `GetCryptographyClient(keyName, keyVersion)` so the wrap binds to one explicit version.
- The wrapped DEK persists onto `KeyEnvelope.WrappedDek` alongside `WrapResult.KeyId` (the `KeyArn`) and `WrapResult.Algorithm` so `Unwrap` rebinds to the exact key version; rotation produces a new version without invalidating prior-version unwrap, matching the page's `RotationPolicy.RetainVersions` ladder.
- AAD parity: Azure `WrapKey`/`UnwrapKey` carry no native `EncryptionContext` parameter (unlike AWS `Decrypt`), so the page's per-partition AAD binding rides the `FrozenDictionary<string,string>` the keyring threads and is enforced application-side at the boundary rather than by the vault — a wrap whose AAD changes between mint and unwrap is the page's rejected form, surfaced before the call, not by a vault error.
- Offline path: a cached `JsonWebKey` builds a `CryptographyClient(JsonWebKey, LocalCryptographyClientOptions)` that wraps/unwraps with no service round-trip — the path for a sidecar/headless unwrap where the per-open `SecretLease`-class handle already carries the resolved JWK, avoiding a vault call on the hot open.
- `KeyVaultKeyIdentifier.TryCreate` parses a stored key URI into `VaultUri`/`Name`/`Version` at the configuration boundary; internal code holds the parsed components, not raw URIs.
- The clients are long-lived and thread-safe; construct one per vault at composition (per the page's `SecretLease`-managed CMK access) and reuse across operations, never per wrap call.

[RAIL_LAW]:
- Package: `Azure.Security.KeyVault.Keys`
- Owns: master-key custody, **native** key-wrap cryptography for the `azure` arm of the `Element/identity#AUTHORITY` `EnvelopeKeyring`, AND native asymmetric Sign/Verify for the `Element/identity#AUTHORITY` `SigningKeyring` arm
- Accept: `CryptographyClient.WrapKey`/`UnwrapKey` for the DEK envelope, `CryptographyClient.Sign`/`Verify` over the precomputed `OpDigest` for the signing arm, the `CryptographyClient(JsonWebKey)` local path for an offline unwrap/verify, `KeyClient` for master-key CRUD/rotation, `KeyWrapAlgorithm`/`EncryptionAlgorithm`/`SignatureAlgorithm` well-known constants for algorithm selection, `Azure.Core` `Response<T>`/`Pageable<T>` carriers converted once at the boundary
- Reject: exporting master key material (except the explicit attested `ReleaseKey` path), `Encrypt`/`Decrypt`-as-wrap when the native `WrapKey` verb exists, `SignData`/`VerifyData` (hash-then-sign) for an already-hashed `OpDigest`, local key-wrap reimplementation, per-operation client construction, raw key-URI strings inside internal code, `RequestFailedException` leaking past the keyring boundary
