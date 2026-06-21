# [RASM_PERSISTENCE_API_AZURE_KEYVAULT]

`Azure.Security.KeyVault.Keys` supplies the `KeyClient` for vault-side key CRUD and the
`CryptographyClient` for key-wrap and envelope operations over Azure Key Vault and Managed HSM.
It carries the `Keys.Cryptography` algorithm vocabularies (`KeyWrapAlgorithm`, `EncryptionAlgorithm`) plus the `KeyVaultKey`/`JsonWebKey` material model for the Persistence store encryption `KmsProvider` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Azure.Security.KeyVault.Keys`
- package: `Azure.Security.KeyVault.Keys`
- assembly: `Azure.Security.KeyVault.Keys`
- namespace: `Azure.Security.KeyVault.Keys`, `Azure.Security.KeyVault.Keys.Cryptography`
- asset: runtime library
- rail: encryption

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and cryptography family
- rail: encryption

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]        | [RAIL]                                              |
| :-----: | :-------------------------- | :------------------- | :-------------------------------------------------- |
|  [01]   | `KeyClient`                 | management client    | key CRUD, backup, rotation, release over the vault  |
|  [02]   | `CryptographyClient`        | cryptography client  | wrap, unwrap, encrypt, decrypt, sign, verify        |
|  [03]   | `KeyClientOptions`          | client options       | service version and pipeline policy                 |
|  [04]   | `CryptographyClientOptions` | client options       | service version for the cryptography client         |
|  [05]   | `KeyVaultKey`               | key resource         | `JsonWebKey` plus `KeyProperties` carrier           |
|  [06]   | `JsonWebKey`                | key material         | JWK key type, operations, and curve material        |
|  [07]   | `KeyProperties`             | key metadata         | id, name, version, managed, release policy          |
|  [08]   | `KeyVaultKeyIdentifier`     | identifier value     | parses `VaultUri`, `Name`, `Version` from a key URI |
|  [09]   | `DeletedKey`                | soft-delete resource | recovery id and scheduled purge metadata            |

[PUBLIC_TYPE_SCOPE]: algorithm and parameter vocabulary
- rail: encryption

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :-------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `KeyWrapAlgorithm`    | algorithm value   | key-wrap algorithm selector                     |
|  [02]   | `EncryptionAlgorithm` | algorithm value   | encrypt/decrypt algorithm selector              |
|  [03]   | `SignatureAlgorithm`  | algorithm value   | sign/verify algorithm selector                  |
|  [04]   | `KeyType`             | key-type value    | `Rsa`, `RsaHsm`, `Ec`, `EcHsm`, `Oct`, `OctHsm` |
|  [05]   | `KeyOperation`        | operation value   | permitted JWK operation token                   |
|  [06]   | `EncryptParameters`   | parameter carrier | algorithm-specific encrypt inputs               |
|  [07]   | `DecryptParameters`   | parameter carrier | algorithm-specific decrypt inputs               |

[PUBLIC_TYPE_SCOPE]: operation result carriers
- rail: encryption

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [RAIL]                                  |
| :-----: | :-------------- | :------------ | :-------------------------------------- |
|  [01]   | `WrapResult`    | result value  | `EncryptedKey`, `KeyId`, `Algorithm`    |
|  [02]   | `UnwrapResult`  | result value  | unwrapped `Key`, `KeyId`, `Algorithm`   |
|  [03]   | `EncryptResult` | result value  | `Ciphertext`, `Iv`, `AuthenticationTag` |
|  [04]   | `DecryptResult` | result value  | `Plaintext`, `KeyId`, `Algorithm`       |
|  [05]   | `SignResult`    | result value  | `Signature`, `KeyId`, `Algorithm`       |
|  [06]   | `VerifyResult`  | result value  | `IsValid`, `KeyId`, `Algorithm`         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: key-wrap and envelope cryptography
- rail: encryption

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `CryptographyClient(Uri keyId, TokenCredential)`       | constructor    | binds a client to one key identifier        |
|  [02]   | `WrapKey(KeyWrapAlgorithm, byte[] key, ct)`            | wrap           | wraps a data-encryption key to `WrapResult` |
|  [03]   | `UnwrapKey(KeyWrapAlgorithm, byte[] encryptedKey, ct)` | unwrap         | unwraps a key to `UnwrapResult`             |
|  [04]   | `Encrypt(EncryptionAlgorithm, byte[] plaintext, ct)`   | encrypt        | direct encrypt to `EncryptResult`           |
|  [05]   | `Encrypt(EncryptParameters, ct)`                       | encrypt        | parameterized encrypt with `Iv`/`Aad`       |
|  [06]   | `Decrypt(EncryptionAlgorithm, byte[] ciphertext, ct)`  | decrypt        | direct decrypt to `DecryptResult`           |
|  [07]   | `Decrypt(DecryptParameters, ct)`                       | decrypt        | parameterized decrypt with `Iv`/`Aad`       |
|  [08]   | `Sign(SignatureAlgorithm, byte[] digest, ct)`          | sign           | signs a precomputed digest                  |
|  [09]   | `Verify(SignatureAlgorithm, byte[] digest, sig, ct)`   | verify         | verifies a digest signature                 |
|  [10]   | `SignData(SignatureAlgorithm, byte[]\|Stream, ct)`     | sign           | hashes then signs data                      |
|  [11]   | `VerifyData(SignatureAlgorithm, byte[]\|Stream, sig)`  | verify         | hashes then verifies data                   |

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
|  [07]   | `GetPropertiesOfKeys(ct)`                          | list             | `Pageable<KeyProperties>` over all keys     |
|  [08]   | `GetPropertiesOfKeyVersions(name, ct)`             | list             | `Pageable<KeyProperties>` over key versions |
|  [09]   | `UpdateKeyProperties(KeyProperties, keyOps?, ct)`  | update           | updates attributes and permitted operations |
|  [10]   | `ImportKey(name, JsonWebKey, ct)`                  | import           | imports external key material               |
|  [11]   | `ImportKey(ImportKeyOptions, ct)`                  | import           | options-driven import with HSM flag         |
|  [12]   | `StartDeleteKey(name, ct)`                         | delete           | `DeleteKeyOperation` long-running poller    |
|  [13]   | `GetDeletedKey(name, ct)` / `GetDeletedKeys(ct)`   | soft-delete read | reads recoverable `DeletedKey` resources    |
|  [14]   | `StartRecoverDeletedKey(name, ct)`                 | recover          | `RecoverDeletedKeyOperation` poller         |
|  [15]   | `PurgeDeletedKey(name, ct)`                        | purge            | irreversible removal from soft-delete       |
|  [16]   | `BackupKey(name, ct)` / `RestoreKeyBackup(byte[])` | backup           | opaque vault-portable key blob round-trip   |
|  [17]   | `RotateKey(name, ct)`                              | rotate           | forces a new key version                    |
|  [18]   | `GetKeyRotationPolicy` / `UpdateKeyRotationPolicy` | rotation policy  | reads and sets `KeyRotationPolicy`          |
|  [19]   | `GetRandomBytes(count, ct)`                        | rng              | Managed HSM hardware random bytes           |
|  [20]   | `ReleaseKey(name, targetAttestationToken, ct)`     | release          | secure-key-release to `ReleaseKeyResult`    |
|  [21]   | `GetCryptographyClient(keyName, keyVersion?)`      | factory          | derives a `CryptographyClient` for one key  |

[ENTRYPOINT_SCOPE]: algorithm selectors and identifier parse
- rail: encryption

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]    | [RAIL]                                     |
| :-----: | :---------------------------------------------------------- | :---------------- | :----------------------------------------- |
|  [01]   | `KeyWrapAlgorithm.RsaOaep256` / `RsaOaep` / `Rsa15`         | RSA wrap value    | RSA key-wrap algorithm constants           |
|  [02]   | `KeyWrapAlgorithm.A128KW` / `A192KW` / `A256KW`             | AES wrap value    | AES key-wrap algorithm constants           |
|  [03]   | `KeyWrapAlgorithm.CkmAesKeyWrap` / `CkmAesKeyWrapPad`       | CKM wrap value    | PKCS#11 AES key-wrap constants             |
|  [04]   | `EncryptionAlgorithm.RsaOaep256` / `A256Gcm` / `A256CbcPad` | encrypt value     | encrypt/decrypt algorithm constants        |
|  [05]   | `EncryptParameters.A256GcmParameters(plaintext, aad?)`      | parameter factory | AES-GCM encrypt parameter construction     |
|  [06]   | `KeyVaultKeyIdentifier.TryCreate(Uri, out identifier)`      | identifier parse  | splits a key URI into vault, name, version |
|  [07]   | `KeyVaultKeyIdentifier.VaultUri` / `Name` / `Version`       | identifier value  | parsed key URI components                  |

## [04]-[IMPLEMENTATION_LAW]

[KEYVAULT_TOPOLOGY]:
- Two namespaces carry the public surface: `Azure.Security.KeyVault.Keys` for management and `Azure.Security.KeyVault.Keys.Cryptography` for operations.
- `KeyClient` binds to one vault `Uri` plus a `TokenCredential`; `CryptographyClient` binds to one key identifier `Uri`.
- `CryptographyClient` implements `IKeyEncryptionKey`; it serves both remote (vault round-trip) and local (cached `JsonWebKey` material) cryptography behind one surface.
- `KeyWrapAlgorithm` and `EncryptionAlgorithm` are extensible `readonly struct` value types with static well-known members; equality is string-value based and a string converts implicitly.
- `KeyType` carries `Rsa`/`RsaHsm`/`Ec`/`EcHsm`/`Oct`/`OctHsm`; `KeyOperation` carries `Encrypt`, `Decrypt`, `Sign`, `Verify`, `WrapKey`, `UnwrapKey`, `Import`.
- `WrapResult.EncryptedKey` is the wrapped key bytes; `UnwrapResult.Key` is the recovered key bytes; both carry `KeyId` and `Algorithm` for receipt round-trip.
- `KeyVaultKey` is `JsonWebKey` plus `KeyProperties`; `KeyVaultKey.Id` and `KeyProperties.Version` pin the exact key version a wrap was performed against.
- Sync members have async twins (`WrapKeyAsync`, `UnwrapKeyAsync`, `CreateKeyAsync`, `GetKeyAsync`); delete and recover are long-running operations (`DeleteKeyOperation`, `RecoverDeletedKeyOperation`).
- `KeyVaultKeyIdentifier.TryCreate` is the non-throwing URI split; it never validates that the URI references a live vault.

[LOCAL_ADMISSION]:
- The store encryption `KmsProvider` wraps a data-encryption key with `CryptographyClient.WrapKey(KeyWrapAlgorithm.RsaOaep256, dek)` and unwraps it with `UnwrapKey`; the master key never leaves the vault.
- `KeyClient` owns the master-key lifecycle (`CreateRsaKey`, `RotateKey`, `GetKeyRotationPolicy`); the persistence path consumes a `CryptographyClient` derived through `GetCryptographyClient(keyName, keyVersion)`.
- The wrapped DEK persists alongside `WrapResult.KeyId` and `WrapResult.Algorithm` so unwrap rebinds to the exact key version; rotation produces a new version without invalidating prior-version unwrap.
- `KeyVaultKeyIdentifier.TryCreate` parses a stored key URI into `VaultUri`, `Name`, and `Version` at the configuration boundary; internal code holds the parsed components, not raw URIs.
- The clients are long-lived and thread-safe; construct one per vault at composition and reuse across operations, never per wrap call.

[RAIL_LAW]:
- Package: `Azure.Security.KeyVault.Keys`
- Owns: master-key custody and key-wrap cryptography for the store encryption `KmsProvider`
- Accept: `CryptographyClient.WrapKey`/`UnwrapKey` for DEK envelope, `KeyClient` for master-key CRUD and rotation, `KeyWrapAlgorithm` constants for algorithm selection
- Reject: exporting master key material, local key-wrap reimplementation, per-operation client construction, raw key-URI strings inside internal code
