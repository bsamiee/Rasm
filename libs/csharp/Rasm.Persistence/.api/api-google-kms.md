# [RASM_PERSISTENCE_API_GOOGLE_KMS]

`Google.Cloud.Kms.V1` supplies the Cloud KMS `KeyManagementServiceClient`, its symmetric `Encrypt`/`Decrypt`/`GenerateRandomBytes` entrypoints, the `EncryptRequest`/`DecryptRequest` message pair with CRC32C integrity fields, and the strongly typed `CryptoKeyName`/`CryptoKeyVersionName` resource-name builders. It serves the persistence-store `KmsProvider` encryption rail: envelope encryption of column and blob payloads over a pure-managed `Grpc.Net.Client` transport, with the data-encryption key wrapped by a remote `CryptoKey`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Cloud.Kms.V1`
- package: `Google.Cloud.Kms.V1`
- assembly: `Google.Cloud.Kms.V1`
- namespace: `Google.Cloud.Kms.V1`
- asset: runtime library
- rail: encryption

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and builder family
- rail: encryption

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `KeyManagementServiceClient`        | client root   | abstract KMS operation surface               |
|  [02]   | `KeyManagementServiceClientBuilder` | builder       | endpoint, credential, channel configuration  |
|  [03]   | `KeyManagementServiceSettings`      | settings      | per-method retry, timeout, and call settings |
|  [04]   | `ServiceCollectionExtensions`       | DI extension  | `AddKeyManagementServiceClient` registration |

[PUBLIC_TYPE_SCOPE]: cryptographic message family
- rail: encryption

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [RAIL]                                        |
| :-----: | :---------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `EncryptRequest`              | request message  | name, plaintext, AAD, CRC32C                  |
|  [02]   | `EncryptResponse`             | response message | ciphertext, verified CRC32C, protection level |
|  [03]   | `DecryptRequest`              | request message  | name, ciphertext, AAD, CRC32C                 |
|  [04]   | `DecryptResponse`             | response message | plaintext, used-primary, protection level     |
|  [05]   | `GenerateRandomBytesRequest`  | request message  | location, length, protection level            |
|  [06]   | `GenerateRandomBytesResponse` | response message | random `Data` with CRC32C                     |
|  [07]   | `RawEncryptRequest`           | request message  | bring-your-own-IV symmetric encrypt           |
|  [08]   | `RawDecryptRequest`           | request message  | bring-your-own-IV symmetric decrypt           |

[PUBLIC_TYPE_SCOPE]: resource-model family
- rail: encryption

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [RAIL]                                          |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------- |
|  [01]   | `KeyRing`              | resource message | container for crypto keys in a location         |
|  [02]   | `CryptoKey`            | resource message | logical key, purpose, rotation, primary version |
|  [03]   | `CryptoKeyVersion`     | resource message | one key version, state, algorithm, attestation  |
|  [04]   | `CryptoKeyName`        | resource name    | typed `cryptoKeys/*` path builder               |
|  [05]   | `CryptoKeyVersionName` | resource name    | typed `cryptoKeyVersions/*` path builder        |
|  [06]   | `KeyRingName`          | resource name    | typed `keyRings/*` path builder                 |

[PUBLIC_TYPE_SCOPE]: bounded vocabulary
- rail: encryption

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :--------------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `ProtectionLevel`                              | enum          | `Software`, `Hsm`, `External`, `ExternalVpc`  |
|  [02]   | `CryptoKey.Types.CryptoKeyPurpose`             | nested enum   | `EncryptDecrypt`, `AsymmetricSign`, `Mac`     |
|  [03]   | `CryptoKeyVersion.Types.CryptoKeyVersionState` | nested enum   | `Enabled`, `Disabled`, `Destroyed`, `Pending` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction
- rail: encryption

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------ | :------------- | :------------------------------------------- |
|  [01]   | `KeyManagementServiceClient.Create()`                         | static factory | default-endpoint synchronous client          |
|  [02]   | `KeyManagementServiceClient.CreateAsync(cancellationToken?)`  | async factory  | default-endpoint asynchronous client         |
|  [03]   | `KeyManagementServiceClientBuilder.Build()`                   | builder        | configured synchronous client                |
|  [04]   | `KeyManagementServiceClientBuilder.BuildAsync(cancellation?)` | builder        | configured asynchronous client               |
|  [05]   | `services.AddKeyManagementServiceClient(action?)`             | DI extension   | registers `KeyManagementServiceClient` in DI |
|  [06]   | `KeyManagementServiceClient.GrpcClient`                       | property       | underlying generated gRPC client             |

[ENTRYPOINT_SCOPE]: symmetric envelope operations
- rail: encryption

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]  | [RAIL]                                               |
| :-----: | :------------------------------------------------------ | :-------------- | :--------------------------------------------------- |
|  [01]   | `Encrypt(EncryptRequest, callSettings?)`                | request encrypt | full-request encrypt with AAD and CRC32C             |
|  [02]   | `Encrypt(string name, ByteString plaintext, …)`         | name encrypt    | encrypt under a key resource name                    |
|  [03]   | `Encrypt(IResourceName name, ByteString plaintext, …)`  | typed encrypt   | encrypt under `CryptoKeyName`/`CryptoKeyVersionName` |
|  [04]   | `EncryptAsync(EncryptRequest, callSettings?)`           | async encrypt   | awaitable full-request encrypt                       |
|  [05]   | `Decrypt(DecryptRequest, callSettings?)`                | request decrypt | full-request decrypt with AAD and CRC32C             |
|  [06]   | `Decrypt(string name, ByteString ciphertext, …)`        | name decrypt    | decrypt under a key resource name                    |
|  [07]   | `Decrypt(CryptoKeyName name, ByteString ciphertext, …)` | typed decrypt   | decrypt under `CryptoKeyName`                        |
|  [08]   | `DecryptAsync(DecryptRequest, callSettings?)`           | async decrypt   | awaitable full-request decrypt                       |
|  [09]   | `RawEncrypt(RawEncryptRequest, callSettings?)`          | raw encrypt     | bring-your-own-IV symmetric encrypt                  |
|  [10]   | `RawDecrypt(RawDecryptRequest, callSettings?)`          | raw decrypt     | bring-your-own-IV symmetric decrypt                  |

[ENTRYPOINT_SCOPE]: random and key-material operations
- rail: encryption

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `GenerateRandomBytes(GenerateRandomBytesRequest, callSettings?)`    | random request  | HSM-backed random `ByteString`               |
|  [02]   | `GenerateRandomBytes(string location, int lengthBytes, level, …)`   | random shortcut | location plus length plus `ProtectionLevel`  |
|  [03]   | `GenerateRandomBytesAsync(GenerateRandomBytesRequest, …)`           | async random    | awaitable HSM-backed random                  |
|  [04]   | `CreateKeyRing(KeyRingName parent, string keyRingId, KeyRing, …)`   | provisioning    | creates a key ring in a location             |
|  [05]   | `CreateCryptoKey(KeyRingName parent, string id, CryptoKey, …)`      | provisioning    | creates a `CryptoKey` with purpose template  |
|  [06]   | `UpdateCryptoKeyPrimaryVersion(CryptoKeyName, string versionId, …)` | rotation        | repoints the primary version for encrypt     |
|  [07]   | `DestroyCryptoKeyVersion(CryptoKeyVersionName name, …)`             | lifecycle       | schedules a version for destruction          |
|  [08]   | `RestoreCryptoKeyVersion(CryptoKeyVersionName name, …)`             | lifecycle       | restores a scheduled-for-destruction version |

[ENTRYPOINT_SCOPE]: resource-name builders
- rail: encryption

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `CryptoKeyName.FromProjectLocationKeyRingCryptoKey(project, location, keyRing, key)`   | typed builder  | composes a `cryptoKeys/*` path        |
|  [02]   | `CryptoKeyName.Parse(cryptoKeyName)`                                                   | parse          | parses a full key resource string     |
|  [03]   | `CryptoKeyVersionName.FromProjectLocationKeyRingCryptoKeyCryptoKeyVersion(…, version)` | typed builder  | composes a `cryptoKeyVersions/*` path |
|  [04]   | `CryptoKeyVersionName.Parse(cryptoKeyVersionName)`                                     | parse          | parses a full version resource string |
|  [05]   | `KeyRingName.FromProjectLocationKeyRing(project, location, keyRing)`                   | typed builder  | composes a `keyRings/*` path          |

## [04]-[IMPLEMENTATION_LAW]

[KMS_TOPOLOGY]:
- `KeyManagementServiceClient` is an abstract client; obtain a concrete instance through `Create`, `CreateAsync`, a `KeyManagementServiceClientBuilder`, or `AddKeyManagementServiceClient` DI registration, never by direct construction.
- The client is thread-safe and long-lived; it wraps one `Grpc.Net.Client` channel and is shared across operations rather than created per call.
- Resource paths follow `projects/{project}/locations/{location}/keyRings/{keyRing}/cryptoKeys/{cryptoKey}`; the typed `CryptoKeyName` and `CryptoKeyVersionName` builders own this grammar so callers never concatenate path segments.
- Symmetric envelope encryption uses a `CryptoKey` of purpose `EncryptDecrypt`; `Encrypt` resolves the primary `CryptoKeyVersion`, while `Decrypt` selects the version internally and reports it through `DecryptResponse.UsedPrimary`.
- `EncryptRequest` carries `Plaintext`, optional `AdditionalAuthenticatedData`, and optional `PlaintextCrc32C`/`AdditionalAuthenticatedDataCrc32C` integrity checksums; `EncryptResponse` returns `Ciphertext`, `CiphertextCrc32C`, `VerifiedPlaintextCrc32C`, `VerifiedAdditionalAuthenticatedDataCrc32C`, and the `ProtectionLevel`.
- `DecryptResponse.PlaintextCrc32C` is an `int64` carrier for a `uint32` checksum; a mismatch against the locally computed CRC32C means the payload is corrupt and the response is discarded with a bounded retry, not trusted.
- All payload fields are `Google.Protobuf.ByteString`; column and blob bytes convert through `ByteString.CopyFrom` on the way in and `ByteString.ToByteArray`/`Span` on the way out.
- `ProtectionLevel` selects the backend: `Software`, `Hsm`, `External`, `ExternalVpc`, or `HsmSingleTenant`; the value is fixed by the `CryptoKey` version template and echoed on every response.
- Transport is pure-managed `Grpc.Net.Client`; no native gRPC binary is required.

[LOCAL_ADMISSION]:
- The `KmsProvider` holds one `KeyManagementServiceClient` singleton resolved from DI; operations call `EncryptAsync`/`DecryptAsync` at the persistence boundary, never construct a client per row.
- The data-encryption key is generated through `GenerateRandomBytes` (or locally) and wrapped by `EncryptAsync(CryptoKeyName, plaintext)`; the wrapped key persists beside the ciphertext, and `DecryptAsync` unwraps it on read.
- Key identity persists as the typed `CryptoKeyName` string; the provider rebuilds it through `CryptoKeyName.FromProjectLocationKeyRingCryptoKey` at composition rather than storing loose path fragments.
- Every `EncryptRequest` sets `PlaintextCrc32C` and every `DecryptResponse`/`EncryptResponse` CRC32C is verified before the payload crosses the persistence boundary; a checksum mismatch fails the operation onto the typed error rail.
- Rotation repoints encryption through `UpdateCryptoKeyPrimaryVersion`; existing ciphertext still decrypts because `Decrypt` resolves the embedded version, so rotation never rewrites stored blobs.

[RAIL_LAW]:
- Package: `Google.Cloud.Kms.V1`
- Owns: remote envelope encryption and decryption of persistence payloads via Cloud KMS
- Accept: a DI-resolved `KeyManagementServiceClient` singleton, typed `CryptoKeyName`/`CryptoKeyVersionName`, `ByteString` payloads, and CRC32C-verified responses
- Reject: per-operation client construction, hand-built resource path strings, and unchecked CRC32C payloads crossing the persistence boundary
