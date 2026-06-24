# [RASM_PERSISTENCE_API_GOOGLE_KMS]

`Google.Cloud.Kms.V1` is the Cloud KMS GA client library: the abstract
`KeyManagementServiceClient`, its symmetric `Encrypt`/`Decrypt`/`GenerateRandomBytes`/`RawEncrypt`/
`RawDecrypt` entrypoints, the `EncryptRequest`/`DecryptRequest` protobuf message pair with CRC32C
integrity fields, and the strongly typed `CryptoKeyName`/`CryptoKeyVersionName`/`KeyRingName`
resource-name builders. It serves the persistence-store `KmsProvider` encryption rail: envelope
encryption of column and blob payloads over a pure-managed `Grpc.Net.Client` transport, the
data-encryption key wrapped by a remote `CryptoKey`. The 3.24.0 package also bundles the
`Autokey`, `AutokeyAdmin`, `EkmService`, and `HsmManagement` clients in the same assembly; the
Persistence rail consumes only `KeyManagementServiceClient`. All payload fields are
`Google.Protobuf.ByteString`, and the bound asset on the `net10.0` consumer is the
`netstandard2.0` library — this package multi-targets only `net462`/`netstandard2.0` and has no
`net8.0`/`net10.0`-specific asset, so the consumer binds `netstandard2.0`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Cloud.Kms.V1`
- package: `Google.Cloud.Kms.V1`
- version: `3.24.0`
- license: `Apache-2.0`
- assembly: `Google.Cloud.Kms.V1` (`lib/netstandard2.0` binds for the `net10.0` consumer; `net462` is the framework fallback — there is no net8.0/net10.0 asset)
- namespace: `Google.Cloud.Kms.V1`
- asset: pure-managed library over `Grpc.Net.Client` + `Google.Protobuf` (no native gRPC binary required)
- abi: all payload fields are `Google.Protobuf.ByteString`; messages are protobuf `IMessage` types
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

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]    | [RAIL]                                            |
| :-----: | :---------------------------- | :--------------- | :----------------------------------------------- |
|  [01]   | `EncryptRequest`              | request message  | `Name`/`ResourceName`, `Plaintext`, AAD, CRC32C  |
|  [02]   | `EncryptResponse`             | response message | `Ciphertext`, `CiphertextCrc32C`, verified-flags, `ProtectionLevel` |
|  [03]   | `DecryptRequest`              | request message  | `Name`/`CryptoKeyName`, `Ciphertext`, AAD, CRC32C |
|  [04]   | `DecryptResponse`             | response message | `Plaintext`, `PlaintextCrc32C`, `UsedPrimary`, `ProtectionLevel` |
|  [05]   | `GenerateRandomBytesRequest`  | request message  | location, length, protection level               |
|  [06]   | `GenerateRandomBytesResponse` | response message | random `Data` (+ CRC32C)                          |
|  [07]   | `RawEncryptRequest` / `RawEncryptResponse` | raw message | bring-your-own-IV symmetric encrypt (+ `InitializationVector`) |
|  [08]   | `RawDecryptRequest` / `RawDecryptResponse` | raw message | bring-your-own-IV symmetric decrypt              |
|  [09]   | `MacSignRequest`/`MacVerifyRequest` (+responses) | MAC message | HMAC sign/verify over a Mac-purpose key       |
|  [10]   | `AsymmetricDecryptRequest` (+response) | asym message | decrypt under an `AsymmetricDecrypt`-purpose version |

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

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `ProtectionLevel`                              | enum          | `Unspecified`, `Software`, `Hsm`, `External`, `ExternalVpc`, `HsmSingleTenant` |
|  [02]   | `CryptoKey.Types.CryptoKeyPurpose`             | nested enum   | `EncryptDecrypt`, `RawEncryptDecrypt`, `AsymmetricSign`, `AsymmetricDecrypt`, `Mac`, `KeyEncapsulation` |
|  [03]   | `CryptoKeyVersion.Types.CryptoKeyVersionState` | nested enum   | `PendingGeneration`, `Enabled`, `Disabled`, `Destroyed`, `DestroyScheduled`, `PendingImport`, `ImportFailed` |

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
|  [06]   | `KeyManagementServiceClient.GrpcClient`                       | property       | the generated gRPC client escape hatch       |

[ENTRYPOINT_SCOPE]: symmetric envelope operations (sync; each has an `…Async` mirror taking `CallSettings`/`CancellationToken`)
- rail: encryption

| [INDEX] | [SURFACE]                                               | [ENTRY_FAMILY]  | [RAIL]                                               |
| :-----: | :------------------------------------------------------ | :-------------- | :--------------------------------------------------- |
|  [01]   | `Encrypt(EncryptRequest, callSettings?)`                | request encrypt | full-request encrypt with AAD and CRC32C             |
|  [02]   | `Encrypt(string name, ByteString plaintext, …)`         | name encrypt    | encrypt under a key resource name                    |
|  [03]   | `Encrypt(IResourceName name, ByteString plaintext, …)`  | typed encrypt   | encrypt under `CryptoKeyName`/`CryptoKeyVersionName` |
|  [04]   | `EncryptAsync(EncryptRequest, callSettings? \| ct)`     | async encrypt   | awaitable full-request encrypt                       |
|  [05]   | `Decrypt(DecryptRequest, callSettings?)`                | request decrypt | full-request decrypt with AAD and CRC32C             |
|  [06]   | `Decrypt(string name, ByteString ciphertext, …)`        | name decrypt    | decrypt under a key resource name                    |
|  [07]   | `Decrypt(CryptoKeyName name, ByteString ciphertext, …)` | typed decrypt   | decrypt under `CryptoKeyName`                        |
|  [08]   | `DecryptAsync(DecryptRequest, callSettings? \| ct)`     | async decrypt   | awaitable full-request decrypt                       |
|  [09]   | `RawEncrypt(RawEncryptRequest, callSettings?)`          | raw encrypt     | bring-your-own-IV symmetric encrypt                  |
|  [10]   | `RawDecrypt(RawDecryptRequest, callSettings?)`          | raw decrypt     | bring-your-own-IV symmetric decrypt                  |
|  [11]   | `MacSign(...)` / `MacVerify(...)`                       | mac             | HMAC over a `Mac`-purpose key (integrity, not envelope) |
|  [12]   | `AsymmetricDecrypt(...)`                                | asym decrypt    | decrypt under an `AsymmetricDecrypt` version         |

[ENTRYPOINT_SCOPE]: random and key-material operations
- rail: encryption

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `GenerateRandomBytes(GenerateRandomBytesRequest, callSettings?)`    | random request  | HSM-backed random `ByteString`               |
|  [02]   | `GenerateRandomBytes(string location, int lengthBytes, level, …)`   | random shortcut | location + length + `ProtectionLevel`        |
|  [03]   | `GenerateRandomBytesAsync(GenerateRandomBytesRequest, …)`           | async random    | awaitable HSM-backed random                  |
|  [04]   | `CreateKeyRing(KeyRingName parent, string keyRingId, KeyRing, …)`   | provisioning    | creates a key ring in a location             |
|  [05]   | `CreateCryptoKey(KeyRingName parent, string id, CryptoKey, …)`      | provisioning    | creates a `CryptoKey` with purpose template  |
|  [06]   | `CreateCryptoKeyVersion(CryptoKeyName parent, CryptoKeyVersion, …)` | provisioning    | adds a new key version (rotation source)     |
|  [07]   | `UpdateCryptoKeyPrimaryVersion(CryptoKeyName, string versionId, …)` | rotation        | repoints the primary version for encrypt     |
|  [08]   | `DestroyCryptoKeyVersion(CryptoKeyVersionName name, …)`             | lifecycle       | schedules a version for destruction          |
|  [09]   | `RestoreCryptoKeyVersion(CryptoKeyVersionName name, …)`             | lifecycle       | restores a scheduled-for-destruction version |

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
- `KeyManagementServiceClient` is an abstract client; obtain a concrete instance through `Create`, `CreateAsync`, a `KeyManagementServiceClientBuilder`, or `AddKeyManagementServiceClient` DI registration, never by direct construction. `GrpcClient` exposes the underlying generated client for surfaces the wrapper does not project.
- the client is thread-safe and long-lived; it wraps one `Grpc.Net.Client` channel and is shared across operations rather than created per call. Transport is pure-managed; no native gRPC binary is required.
- resource paths follow `projects/{project}/locations/{location}/keyRings/{keyRing}/cryptoKeys/{cryptoKey}`; the typed `CryptoKeyName`/`CryptoKeyVersionName`/`KeyRingName` builders own this grammar so callers never concatenate path segments.
- symmetric envelope encryption uses a `CryptoKey` of purpose `EncryptDecrypt`; `Encrypt` resolves the primary `CryptoKeyVersion`, while `Decrypt` selects the version internally and reports whether the primary was used through `DecryptResponse.UsedPrimary`. `RawEncrypt`/`RawDecrypt` (purpose `RawEncryptDecrypt`) expose a caller-supplied IV; `MacSign`/`MacVerify` (`Mac`) and `AsymmetricDecrypt` are separate purposes outside the envelope rail.
- `EncryptRequest` carries `Plaintext`, optional `AdditionalAuthenticatedData`, and optional `PlaintextCrc32C`/`AdditionalAuthenticatedDataCrc32C` (`long?` carriers for `uint32` checksums); `EncryptResponse` returns `Ciphertext`, `CiphertextCrc32C` (`long?`), the boolean `VerifiedPlaintextCrc32C`/`VerifiedAdditionalAuthenticatedDataCrc32C` confirmation flags, the resolving `Name`, and the `ProtectionLevel`.
- `DecryptRequest` carries `Ciphertext`, optional AAD, and `CiphertextCrc32C`/`AdditionalAuthenticatedDataCrc32C`; `DecryptResponse` returns `Plaintext`, `PlaintextCrc32C` (`long?` carrier for a `uint32` checksum of the recovered plaintext), `UsedPrimary`, and `ProtectionLevel`. A mismatch between `DecryptResponse.PlaintextCrc32C` and the locally computed CRC32C means the payload is corrupt and the response is discarded with a bounded retry, not trusted.
- all payload fields are `Google.Protobuf.ByteString`; column and blob bytes convert through `ByteString.CopyFrom` on the way in and `ByteString.ToByteArray`/`.Span`/`.Memory` on the way out.
- `ProtectionLevel` selects the backend: `Software`, `Hsm`, `External`, `ExternalVpc`, or `HsmSingleTenant` (`Unspecified` is the proto-default); the value is fixed by the `CryptoKey` version template and echoed on every response.

[STACK_INTEGRATION]:
- this client is the GCP arm of the polymorphic `Store/encryption` `KmsProvider` `[SmartEnum<string>]` axis: the page binds the provider-neutral `EnvelopeKeyring(Mint, Unwrap, Rotate)` delegate triple to `Mint`→(`GenerateRandomBytes`-or-local DEK then `EncryptAsync(CryptoKeyName, plaintext)`), `Unwrap`→`DecryptAsync(CryptoKeyName, ciphertext)`, `Rotate`→`UpdateCryptoKeyPrimaryVersion` (Cloud KMS rotation repoints the primary, so existing ciphertext still decrypts via the embedded version and stored blobs are never rewritten — the GCP arm's `Rotate` is a primary repoint, not a `ReEncrypt`, unlike the AWS arm in `api-aws-kms`).
- the CRC32C fields are the load-bearing integrity stack: every `EncryptRequest` sets `PlaintextCrc32C` and every `Encrypt`/`Decrypt` response CRC32C (`CiphertextCrc32C`, `VerifiedPlaintextCrc32C`, `PlaintextCrc32C`) is verified before the payload crosses the persistence boundary; a checksum mismatch fails the operation onto the typed `EnvelopeFact.demand-breach`/fault rail (System.IO.Hashing `Crc32` computes the local check), never a trusted partial.
- the `KmsProvider` holds one `KeyManagementServiceClient` singleton resolved from DI and consumes the per-open CMK access through the AppHost `Runtime/config#SECRET_LEASE` seam; key identity persists as the typed `CryptoKeyName` string, rebuilt through `CryptoKeyName.FromProjectLocationKeyRingCryptoKey` at composition rather than as loose path fragments.

[LOCAL_ADMISSION]:
- the `KmsProvider` holds one `KeyManagementServiceClient` singleton resolved from DI; operations call `EncryptAsync`/`DecryptAsync` at the persistence boundary, never construct a client per row.
- the data-encryption key is generated through `GenerateRandomBytes` (or locally) and wrapped by `EncryptAsync(CryptoKeyName, plaintext)`; the wrapped key persists beside the ciphertext, and `DecryptAsync` unwraps it on read.
- key identity persists as the typed `CryptoKeyName` string; the provider rebuilds it through `CryptoKeyName.FromProjectLocationKeyRingCryptoKey` at composition rather than storing loose path fragments.
- every `EncryptRequest` sets `PlaintextCrc32C` and every `DecryptResponse`/`EncryptResponse` CRC32C is verified before the payload crosses the persistence boundary; a checksum mismatch fails the operation onto the typed error rail.
- rotation repoints encryption through `UpdateCryptoKeyPrimaryVersion`; existing ciphertext still decrypts because `Decrypt` resolves the embedded version, so rotation never rewrites stored blobs.

[RAIL_LAW]:
- Package: `Google.Cloud.Kms.V1`
- Owns: remote envelope encryption and decryption of persistence payloads via Cloud KMS
- Accept: a DI-resolved `KeyManagementServiceClient` singleton, typed `CryptoKeyName`/`CryptoKeyVersionName`, `ByteString` payloads, and CRC32C-verified responses
- Reject: per-operation client construction, hand-built resource path strings, and unchecked CRC32C payloads crossing the persistence boundary
