# [RASM_PERSISTENCE_API_AZURE_KEYVAULT]

`Azure.Security.KeyVault.Keys` owns vault-side key custody and cryptography for the `azure` `KmsProvider` arm: `KeyClient` drives master-key CRUD, rotation, and secure-key-release over Azure Key Vault and Managed HSM, `CryptographyClient` drives the DEK envelope and asymmetric signing. Its `CryptographyClient.WrapKey`/`UnwrapKey` is a native vault key-wrap verb, so the Azure `EnvelopeKeyring` arm wraps a DEK directly rather than through an `Encrypt`/`Decrypt`-as-wrap shim, and its `Sign`/`Verify` binds the `SigningKeyring` arm over the precomputed `OpDigest`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Azure.Security.KeyVault.Keys`
- package: `Azure.Security.KeyVault.Keys` (MIT)
- assembly: `Azure.Security.KeyVault.Keys` (`lib/net10.0` binds the `net10.0` consumer)
- namespace: `Azure.Security.KeyVault.Keys`, `Azure.Security.KeyVault.Keys.Cryptography`
- asset: runtime library
- depends: `Azure.Core` (pipeline, `Response<T>`, `Pageable<T>`, `TokenCredential`), paired with `Azure.Identity` for the credential at composition
- rail: encryption (the DEK envelope arm), signing (the `SigningKeyring` arm)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and cryptography family

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]        | [CAPABILITY]                                                        |
| :-----: | :------------------------------- | :------------------- | :------------------------------------------------------------------ |
|  [01]   | `KeyClient`                      | management client    | key CRUD, backup, rotation, release over the vault                  |
|  [02]   | `CryptographyClient`             | cryptography client  | wrap, unwrap, encrypt, decrypt, sign, verify; `IKeyEncryptionKey`   |
|  [03]   | `KeyResolver`                    | resolver client      | `IKeyEncryptionKeyResolver` → `CryptographyClient` per key URI      |
|  [04]   | `KeyClientOptions`               | client options       | `ServiceVersion` and pipeline policy                                |
|  [05]   | `CryptographyClientOptions`      | client options       | service version for the remote cryptography client                  |
|  [06]   | `LocalCryptographyClientOptions` | client options       | options for the offline `JsonWebKey` cryptography constructor       |
|  [07]   | `KeyVaultKey`                    | key resource         | `Key` (`JsonWebKey`) with `Properties` (`KeyProperties`) carrier    |
|  [08]   | `JsonWebKey`                     | key material         | JWK key type, operations, curve, `ToRSA`/`ToECDsa`/`ToAes` material |
|  [09]   | `KeyProperties`                  | key metadata         | `Name`, `Id` (`Uri`), `Version`, `Managed`, `ReleasePolicy`         |
|  [10]   | `KeyVaultKeyIdentifier`          | identifier value     | parses `SourceId`/`VaultUri`/`Name`/`Version` from a key URI        |
|  [11]   | `DeletedKey`                     | soft-delete resource | recovery id and scheduled purge metadata                            |
|  [12]   | `KeyRotationPolicy`              | rotation policy      | `LifetimeActions`, `ExpiresIn`; read/written via `KeyClient`        |
|  [13]   | `ReleaseKeyResult`               | release result       | secure-key-release attestation payload                              |
|  [14]   | `RSAKeyVault`                    | crypto bridge        | `RSA` over the vault key from `CryptographyClient.CreateRSA`        |

[PUBLIC_TYPE_SCOPE]: algorithm and parameter vocabulary

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [CAPABILITY]                                    |
| :-----: | :-------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `KeyWrapAlgorithm`    | algorithm value   | key-wrap algorithm selector                     |
|  [02]   | `EncryptionAlgorithm` | algorithm value   | encrypt/decrypt algorithm selector              |
|  [03]   | `SignatureAlgorithm`  | algorithm value   | sign/verify algorithm selector                  |
|  [04]   | `KeyType`             | key-type value    | `Rsa`, `RsaHsm`, `Ec`, `EcHsm`, `Oct`, `OctHsm` |
|  [05]   | `KeyOperation`        | operation value   | permitted JWK operation token                   |
|  [06]   | `EncryptParameters`   | parameter carrier | algorithm-specific encrypt inputs               |
|  [07]   | `DecryptParameters`   | parameter carrier | algorithm-specific decrypt inputs               |

[SIGNATURE_ALGORITHM]: `ES256` `ES384` `ES512` `ES256K` `PS256` `PS384` `PS512` `RS256` `RS384` `RS512` `HS256` `HS384` `HS512`; the `SigningKeyring` binds the ES/PS/RS rows.

[PUBLIC_TYPE_SCOPE]: operation result carriers

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                                                 |
| :-----: | :-------------- | :------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `WrapResult`    | result value  | `EncryptedKey`, `KeyId`, `Algorithm` (`KeyWrapAlgorithm`)                                    |
|  [02]   | `UnwrapResult`  | result value  | unwrapped `Key`, `KeyId`, `Algorithm` (`KeyWrapAlgorithm`)                                   |
|  [03]   | `EncryptResult` | result value  | `Ciphertext`, `Iv`, `AuthenticationTag`, `AdditionalAuthenticatedData`, `KeyId`, `Algorithm` |
|  [04]   | `DecryptResult` | result value  | `Plaintext`, `KeyId`, `Algorithm` (`EncryptionAlgorithm`)                                    |
|  [05]   | `SignResult`    | result value  | `Signature`, `KeyId`, `Algorithm` (`SignatureAlgorithm`)                                     |
|  [06]   | `VerifyResult`  | result value  | `IsValid`, `KeyId`, `Algorithm` (`SignatureAlgorithm`)                                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: key-wrap and envelope cryptography

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `CryptographyClient(Uri keyId, TokenCredential, CryptographyClientOptions?)` | ctor     | one key id, remote; vault round-trip     |
|  [02]   | `CryptographyClient(JsonWebKey, LocalCryptographyClientOptions?)`            | ctor     | offline over cached JWK; no vault call   |
|  [03]   | `WrapKey(KeyWrapAlgorithm, byte[] key, ct)`                                  | instance | wraps a DEK to `WrapResult`              |
|  [04]   | `UnwrapKey(KeyWrapAlgorithm, byte[] encryptedKey, ct)`                       | instance | unwraps to `UnwrapResult`                |
|  [05]   | `Encrypt(EncryptionAlgorithm, byte[] plaintext, ct)`                         | instance | direct encrypt to `EncryptResult`        |
|  [06]   | `Encrypt(EncryptParameters, ct)`                                             | instance | parameterized encrypt with `Iv`/`Aad`    |
|  [07]   | `Decrypt(EncryptionAlgorithm, byte[] ciphertext, ct)`                        | instance | direct decrypt to `DecryptResult`        |
|  [08]   | `Decrypt(DecryptParameters, ct)`                                             | instance | parameterized decrypt with `Iv`/`Aad`    |
|  [09]   | `Sign(SignatureAlgorithm, byte[] digest, ct)`                                | instance | signs a precomputed digest               |
|  [10]   | `Verify(SignatureAlgorithm, byte[] digest, sig, ct)`                         | instance | verifies a digest signature              |
|  [11]   | `SignData(SignatureAlgorithm, byte[]\|Stream, ct)`                           | instance | hashes then signs data                   |
|  [12]   | `VerifyData(SignatureAlgorithm, byte[]\|Stream, sig)`                        | instance | hashes then verifies data                |
|  [13]   | `CreateRSA(ct)` / `CreateRSAAsync(ct)`                                       | instance | public key → `RSAKeyVault` (`RSA`)       |
|  [14]   | `KeyResolver(TokenCredential).Resolve(Uri keyId, ct)`                        | instance | key URI → bound `CryptographyClient`     |
|  [15]   | `CryptographyClient.KeyId`                                                   | property | bound key id (`IKeyEncryptionKey.KeyId`) |

[ENTRYPOINT_SCOPE]: key lifecycle management

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `KeyClient(Uri vaultUri, TokenCredential)`                | ctor     | binds a client to one vault `Uri`                |
|  [02]   | `CreateKey(name, KeyType, CreateKeyOptions?, ct)`         | instance | creates a key of the given `KeyType`             |
|  [03]   | `CreateRsaKey(CreateRsaKeyOptions, ct)`                   | instance | RSA-specific key create                          |
|  [04]   | `CreateEcKey(CreateEcKeyOptions, ct)`                     | instance | EC-specific key create                           |
|  [05]   | `CreateOctKey(CreateOctKeyOptions, ct)`                   | instance | symmetric (Managed HSM) key create               |
|  [06]   | `GetKey(name, version?, ct)`                              | instance | returns `Response<KeyVaultKey>`                  |
|  [07]   | `GetKeyAttestation(name, version?, ct)`                   | instance | public key plus attestation blob (`KeyVaultKey`) |
|  [08]   | `GetPropertiesOfKeys(ct)`                                 | instance | `Pageable<KeyProperties>` over all keys          |
|  [09]   | `GetPropertiesOfKeyVersions(name, ct)`                    | instance | `Pageable<KeyProperties>` over key versions      |
|  [10]   | `UpdateKeyProperties(KeyProperties, keyOps?, ct)`         | instance | updates attributes and permitted operations      |
|  [11]   | `ImportKey(name, JsonWebKey, ct)`                         | instance | imports external key material                    |
|  [12]   | `ImportKey(ImportKeyOptions, ct)`                         | instance | options-driven import with HSM flag              |
|  [13]   | `StartDeleteKey(name, ct)`                                | instance | `DeleteKeyOperation` long-running poller         |
|  [14]   | `GetDeletedKey(name, ct)` / `GetDeletedKeys(ct)`          | instance | reads recoverable `DeletedKey` resources         |
|  [15]   | `StartRecoverDeletedKey(name, ct)`                        | instance | `RecoverDeletedKeyOperation` poller              |
|  [16]   | `PurgeDeletedKey(name, ct)`                               | instance | irreversible removal from soft-delete            |
|  [17]   | `BackupKey(name, ct)` / `RestoreKeyBackup(byte[])`        | instance | opaque vault-portable key blob round-trip        |
|  [18]   | `RotateKey(name, ct)`                                     | instance | forces a new key version                         |
|  [19]   | `GetKeyRotationPolicy(keyName, ct)`                       | instance | reads the `KeyRotationPolicy`                    |
|  [20]   | `UpdateKeyRotationPolicy(keyName, KeyRotationPolicy, ct)` | instance | sets the `KeyRotationPolicy`                     |
|  [21]   | `GetRandomBytes(count, ct)`                               | instance | Managed HSM hardware random bytes                |
|  [22]   | `ReleaseKey(name, targetAttestationToken, ct)`            | instance | secure-key-release to `ReleaseKeyResult`         |
|  [23]   | `ReleaseKey(ReleaseKeyOptions, ct)`                       | instance | options-driven secure-key-release                |
|  [24]   | `GetCryptographyClient(keyName, keyVersion?)`             | factory  | derives a `CryptographyClient` for one key       |

[ENTRYPOINT_SCOPE]: algorithm selectors and identifier parse

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `KeyWrapAlgorithm.RsaOaep256` / `RsaOaep` / `Rsa15`                      | static   | wire `RSA-OAEP-256`/`RSA-OAEP`/`RSA1_5`  |
|  [02]   | `KeyWrapAlgorithm.A128KW` / `A192KW` / `A256KW`                          | static   | AES key-wrap algorithm constants         |
|  [03]   | `KeyWrapAlgorithm.CkmAesKeyWrap` / `CkmAesKeyWrapPad`                    | static   | PKCS#11 AES key-wrap constants           |
|  [04]   | `EncryptionAlgorithm.RsaOaep256` / `A256Gcm` / `A256CbcPad`              | static   | encrypt/decrypt algorithm constants      |
|  [05]   | `EncryptParameters.{Rsa15,RsaOaep,RsaOaep256}Parameters(plaintext)`      | factory  | RSA encrypt parameter construction       |
|  [06]   | `EncryptParameters.A{128,192,256}GcmParameters(plaintext, aad?)`         | factory  | AES-GCM encrypt params, optional AAD     |
|  [07]   | `EncryptParameters.A{128,192,256}{Cbc,CbcPad}Parameters(plaintext, iv?)` | factory  | AES-CBC / AES-CBC-PAD encrypt parameters |
|  [08]   | `DecryptParameters.*` (mirror set)                                       | factory  | decrypt inputs with `Iv`/`Aad`           |
|  [09]   | `KeyVaultKeyIdentifier.TryCreate(Uri, out identifier)`                   | static   | non-throwing split of a key URI          |
|  [10]   | `KeyVaultKeyIdentifier.SourceId` / `VaultUri` / `Name` / `Version`       | property | parsed key URI components                |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two namespaces carry the surface — `Azure.Security.KeyVault.Keys` for management, `.Cryptography` for operations — and the whole client rides `Azure.Core`: `Response<T>`/`Pageable<T>`/`AsyncPageable<T>` carriers, `TokenCredential`, and `RequestFailedException` converted to the page error rail once at the boundary, never inside the keyring delegates.
- `KeyClient` binds one vault `Uri` with a `TokenCredential`; `CryptographyClient` binds one key-identifier `Uri` (remote, vault round-trips) or one `JsonWebKey` (local, no service call), the same `IKeyEncryptionKey` surface gated by which constructor built it, and `KeyResolver` resolves a key URI to a bound `CryptographyClient` on demand.
- `KeyWrapAlgorithm`/`EncryptionAlgorithm`/`SignatureAlgorithm` are extensible `readonly struct` values with string-based equality and implicit string conversion, so a new algorithm is a wire string.
- Sync members carry async twins (`WrapKeyAsync`, `CreateKeyAsync`); delete and recover are long-running `DeleteKeyOperation`/`RecoverDeletedKeyOperation` pollers.
- `KeyVaultKey` pairs `Key` (`JsonWebKey`) with `Properties` (`KeyProperties`); `KeyProperties.Id` (`Uri`) and `.Version` pin the exact key version a wrap ran against, and `JsonWebKey` round-trips to BCL `RSA`/`ECDsa`/`Aes` through `ToRSA`/`ToECDsa`/`ToAes`.

[STACKING]:
- `api-aws-kms`, `api-google-kms`: the peer `KmsProvider` arms bind the same `EnvelopeKeyring`/`SigningKeyring` delegate surfaces against their own members; the Azure arm alone exposes a native `WrapKey`/`UnwrapKey` vault verb, so its `Rewrap` is a `WrapKey` against the new key version, never the `Encrypt`/`Decrypt`-as-wrap those arms run.
- `Element/identity` `KmsProvider` `[SmartEnum<string>]` axis: selects the `azure` arm (`NativeWrap: true`); `EnvelopeKeyring(Mint, Unwrap, Rewrap, Probe)` binds `Mint`→`WrapKey(RsaOaep256, dek)`, `Unwrap`→`UnwrapKey` zeroing the recovered `Key`, `Rewrap`→`WrapKey` under a new version, `Probe`→`KeyClient.GetKey` `KeyProperties`, and `SigningKeyring` `Sign`/`Verify` binds `CryptographyClient.Sign`/`Verify` over the precomputed `OpDigest` against a signing key distinct from the envelope CMK.
- within-lib crypto bridge: `CryptographyClient.CreateRSA`→`RSAKeyVault` (`RSA` over the vault key) and `JsonWebKey.ToRSA`/`ToAes` rehydrate BCL primitives for the `CryptographyClient(JsonWebKey)` offline path, so a cached JWK wraps or verifies with no vault round-trip.

[LOCAL_ADMISSION]:
- `KeyClient` owns master-key lifecycle (`CreateRsaKey`, `RotateKey`, `GetKeyRotationPolicy`/`UpdateKeyRotationPolicy` on a `KeyRotationPolicy.ExpiresIn` cadence); the persistence path consumes a `CryptographyClient` from `GetCryptographyClient(keyName, keyVersion)`, so a wrap binds one explicit version and rotation mints a new version without invalidating prior-version unwrap.
- `KeyEnvelope.WrappedDek` persists the wrapped DEK beside `WrapResult.KeyId` and `.Algorithm`; the recovered `UnwrapResult.Key` rehydrates the local cipher and zeroes immediately after the bind.
- Azure `WrapKey`/`UnwrapKey` carry no `EncryptionContext` parameter, so per-partition AAD rides the `FrozenDictionary<string,string>` the keyring threads and is enforced application-side before the call; an AAD that changes between mint and unwrap is rejected before the call, never by a vault error.
- `KeyVaultKeyIdentifier.TryCreate` parses a stored key URI into `VaultUri`/`Name`/`Version` at the configuration boundary, internal code holding the parsed components; clients are long-lived and thread-safe — one per vault at composition, never per wrap, consuming the per-open `SecretLease` CMK handle so this owner holds the client and AAD binding while the runtime lease owns the credential lifecycle.

[RAIL_LAW]:
- Package: `Azure.Security.KeyVault.Keys`
- Owns: master-key custody, native key-wrap cryptography for the `azure` `EnvelopeKeyring` arm, and asymmetric Sign/Verify for the `SigningKeyring` arm
- Accept: `CryptographyClient.WrapKey`/`UnwrapKey` for the DEK envelope, `Sign`/`Verify` over the precomputed `OpDigest`, the `CryptographyClient(JsonWebKey)` local path for an offline unwrap or verify, `KeyClient` for master-key CRUD/rotation and the attested `ReleaseKey` TEE export, well-known `KeyWrapAlgorithm`/`SignatureAlgorithm` constants, `Azure.Core` carriers converted once at the boundary
- Reject: `Encrypt`/`Decrypt`-as-wrap where the native `WrapKey` verb exists, `SignData`/`VerifyData` for an already-hashed `OpDigest`, exported master-key material outside the attested `ReleaseKey` path, per-operation client construction, raw key-URI strings in internal code, `RequestFailedException` past the keyring boundary
