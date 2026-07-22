# [RASM_PERSISTENCE_API_GOOGLE_KMS]

`Google.Cloud.Kms.V1` binds the Cloud KMS remote-key surface behind the `Element/identity` `KmsProvider.Gcp` arm: encrypt-as-wrap envelope custody of a data-encryption key, and asymmetric signing of the seam `OpDigest` whose verification runs client-side against the downloaded public key. One abstract `KeyManagementServiceClient` serves both arms over a pure-managed HTTP/2 transport, and every payload rides as `Google.Protobuf.ByteString` beside a CRC32C companion the caller sets and checks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Cloud.Kms.V1`
- package: `Google.Cloud.Kms.V1` (`Apache-2.0`, Google LLC)
- assembly: `Google.Cloud.Kms.V1` (`lib/netstandard2.0` binds the `net10.0` consumer, `lib/net462` the framework fallback)
- namespace: `Google.Cloud.Kms.V1`, `Microsoft.Extensions.DependencyInjection`
- depends: `Google.Api.Gax.Grpc` over `Grpc.Net.Client`; pure-managed, no native gRPC asset
- abi: payloads and key blobs are `Google.Protobuf.ByteString`, messages are protobuf `IMessage`, and each `uint32` CRC32C rides a `long?` carrier
- rail: envelope DEK wrap/unwrap and asymmetric `OpDigest` sign — the GCP arm of the `Element/identity` `KmsProvider` axis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, configuration, and injection family

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]  | [CAPABILITY]                                           |
| :-----: | :---------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `KeyManagementServiceClient`        | abstract class | the KMS operation surface both custody arms bind       |
|  [02]   | `KeyManagementServiceClientBuilder` | builder        | endpoint, credential, and channel configuration        |
|  [03]   | `KeyManagementServiceSettings`      | settings       | per-method `CallSettings` expiration and retry policy  |
|  [04]   | `ServiceCollectionExtensions`       | static class   | `AddKeyManagementServiceClient` container registration |

[PUBLIC_TYPE_SCOPE]: message family — protobuf request and response shapes; each `*Crc32C` is a `long?` carrier and each `Verified*Crc32C` a service-side confirmation flag.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `EncryptRequest`              | request       | `Name`/`ResourceName`, `Plaintext`, AAD, and both checksums           |
|  [02]   | `EncryptResponse`             | response      | `Ciphertext`, `CiphertextCrc32C`, two verified flags, `Name`          |
|  [03]   | `DecryptRequest`              | request       | `Name`/`CryptoKeyName`, `Ciphertext`, AAD, and both checksums         |
|  [04]   | `DecryptResponse`             | response      | `Plaintext`, `PlaintextCrc32C`, `UsedPrimary`, `ProtectionLevel`      |
|  [05]   | `GenerateRandomBytesRequest`  | request       | `Location`, `LengthBytes`, `ProtectionLevel`                          |
|  [06]   | `GenerateRandomBytesResponse` | response      | HSM-born `Data` under `DataCrc32C`                                    |
|  [07]   | `AsymmetricSignRequest`       | request       | `CryptoKeyVersionName`, `Digest`, `Data`, and both checksums          |
|  [08]   | `AsymmetricSignResponse`      | response      | `Signature`, `SignatureCrc32C`, two verified flags, `ProtectionLevel` |
|  [09]   | `Digest`                      | oneof message | the pre-hashed digest, width selected by `DigestCase`                 |
|  [10]   | `GetPublicKeyRequest`         | request       | `CryptoKeyVersionName` under an explicit `PublicKeyFormat`            |
|  [11]   | `PublicKey`                   | response      | `Pem`, `PemCrc32C`, `Algorithm`, `PublicKey_` checksummed data        |

[DIGEST_ARMS]: `Sha256` `Sha384` `Sha512` `ExternalMu`
[RAW_IV_MESSAGES]: `RawEncryptRequest` `RawEncryptResponse` `RawDecryptRequest` `RawDecryptResponse` — caller-supplied `InitializationVector` and `TagLength` beside the same plaintext, AAD, and checksum fields.
[ADJACENT_PURPOSES]: `MacSignRequest` `MacSignResponse` `MacVerifyRequest` `MacVerifyResponse` `AsymmetricDecryptRequest` `AsymmetricDecryptResponse` `ImportCryptoKeyVersionRequest`

[PUBLIC_TYPE_SCOPE]: resource model and its typed path builders; the bounded vocabularies below carry the enum rosters.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]    | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `KeyRing`                  | resource message | key container scoped to one location                               |
|  [02]   | `CryptoKey`                | resource message | `Purpose`, `Primary`, `VersionTemplate`, and the rotation schedule |
|  [03]   | `CryptoKeyVersion`         | resource message | `State`, `Algorithm`, `ProtectionLevel`, `Attestation`             |
|  [04]   | `CryptoKeyVersionTemplate` | resource message | the `ProtectionLevel` and `Algorithm` a minted version inherits    |
|  [05]   | `KeyOperationAttestation`  | resource message | HSM attestation evidence over a generated version                  |
|  [06]   | `CryptoKeyName`            | resource name    | typed `cryptoKeys/*` path builder                                  |
|  [07]   | `CryptoKeyVersionName`     | resource name    | typed `cryptoKeyVersions/*` path builder                           |
|  [08]   | `KeyRingName`              | resource name    | typed `keyRings/*` path builder                                    |

[PROTECTION_LEVEL]: the backend a version template fixes and every response echoes — `Unspecified` `Software` `Hsm` `External` `ExternalVpc` `HsmSingleTenant`
[CRYPTO_KEY_PURPOSE]: the one operation family a `CryptoKey` admits, nested under `CryptoKey.Types` — `EncryptDecrypt` `RawEncryptDecrypt` `AsymmetricSign` `AsymmetricDecrypt` `Mac` `KeyEncapsulation`
[CRYPTO_KEY_VERSION_STATE]: the lifecycle posture `EnvelopeKeyring.Probe` maps onto `KeyState`, nested under `CryptoKeyVersion.Types` — `PendingGeneration` `Enabled` `Disabled` `Destroyed` `DestroyScheduled` `PendingImport` `ImportFailed` `GenerationFailed` `PendingExternalDestruction` `ExternalDestructionFailed`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and the transport escape hatch

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `KeyManagementServiceClient.Create()`                             | factory  | default-endpoint client                  |
|  [02]   | `KeyManagementServiceClient.CreateAsync(CancellationToken)`       | factory  | awaitable default-endpoint client        |
|  [03]   | `KeyManagementServiceClientBuilder.Build()`                       | instance | configured client                        |
|  [04]   | `KeyManagementServiceClientBuilder.BuildAsync(CancellationToken)` | instance | awaitable configured client              |
|  [05]   | `IServiceCollection.AddKeyManagementServiceClient(Action)`        | static   | container registration of the singleton  |
|  [06]   | `KeyManagementServiceClient.GrpcClient`                           | property | the generated gRPC client under the wrap |

[ENTRYPOINT_SCOPE]: envelope and random operations — each member carries an `…Async` twin taking `CallSettings` or a `CancellationToken`.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `Encrypt(EncryptRequest)`                           | instance | full-request wrap carrying AAD and CRC32C   |
|  [02]   | `Encrypt(IResourceName, ByteString)`                | instance | wrap under a typed key or key-version name  |
|  [03]   | `Decrypt(DecryptRequest)`                           | instance | full-request unwrap carrying AAD and CRC32C |
|  [04]   | `Decrypt(CryptoKeyName, ByteString)`                | instance | unwrap under the typed key name             |
|  [05]   | `RawEncrypt(RawEncryptRequest)`                     | instance | caller-supplied-IV symmetric encrypt        |
|  [06]   | `RawDecrypt(RawDecryptRequest)`                     | instance | caller-supplied-IV symmetric decrypt        |
|  [07]   | `GenerateRandomBytes(GenerateRandomBytesRequest)`   | instance | HSM-born random bytes under a checksum      |
|  [08]   | `GenerateRandomBytes(string, int, ProtectionLevel)` | instance | location, length, and backend shortcut      |

[ENTRYPOINT_SCOPE]: signing operations — the `Element/identity` `SigningKeyring` arm.

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `AsymmetricSign(AsymmetricSignRequest)`        | instance | full-request sign with checksums on digest and data |
|  [02]   | `AsymmetricSign(CryptoKeyVersionName, Digest)` | instance | sign a pre-hashed digest under the typed version    |
|  [03]   | `GetPublicKey(CryptoKeyVersionName)`           | instance | download the `PublicKey` the local verify reads     |

[ENTRYPOINT_SCOPE]: key lifecycle and provisioning

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `CreateKeyRing(LocationName, string, KeyRing)`            | instance | mint a key ring in one location              |
|  [02]   | `CreateCryptoKey(KeyRingName, string, CryptoKey)`         | instance | mint a key under its purpose template        |
|  [03]   | `CreateCryptoKeyVersion(CryptoKeyName, CryptoKeyVersion)` | instance | add a version, the rotation source           |
|  [04]   | `UpdateCryptoKeyPrimaryVersion(CryptoKeyName, string)`    | instance | repoint the primary version encrypt resolves |
|  [05]   | `GetCryptoKeyVersion(CryptoKeyVersionName)`               | instance | read `State`, `Algorithm`, and `Attestation` |
|  [06]   | `ListCryptoKeyVersions(CryptoKeyName) -> PagedEnumerable` | instance | auto-paged version enumeration               |
|  [07]   | `DestroyCryptoKeyVersion(CryptoKeyVersionName)`           | instance | schedule a version for destruction           |
|  [08]   | `RestoreCryptoKeyVersion(CryptoKeyVersionName)`           | instance | restore a scheduled version                  |
|  [09]   | `ImportCryptoKeyVersion(ImportCryptoKeyVersionRequest)`   | instance | bind externally generated key material       |

[ENTRYPOINT_SCOPE]: resource-name builders — each `From…` composes ordered `string` segments and each `Parse` reads one full resource string.

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------------- | :------ | :----------------------------------- |
|  [01]   | `CryptoKeyName.FromProjectLocationKeyRingCryptoKey(…)`                        | factory | compose a `cryptoKeys/*` path        |
|  [02]   | `CryptoKeyName.Parse(string)`                                                 | factory | parse a full key resource string     |
|  [03]   | `CryptoKeyName.TryParse(string, out CryptoKeyName)`                           | static  | non-throwing parse onto a typed rail |
|  [04]   | `CryptoKeyVersionName.FromProjectLocationKeyRingCryptoKeyCryptoKeyVersion(…)` | factory | compose a `cryptoKeyVersions/*` path |
|  [05]   | `KeyRingName.FromProjectLocationKeyRing(…)`                                   | factory | compose a `keyRings/*` path          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `KeyManagementServiceClient` is abstract, so `Create`, `CreateAsync`, a builder, or the container registration yields the concrete instance and `GrpcClient` reaches the generated client beneath it; of the assembly's client roots the Persistence rails bind this one.
- One instance is thread-safe and long-lived over a single channel every operation shares.
- `CryptoKeyName`, `CryptoKeyVersionName`, and `KeyRingName` own the `projects/*/locations/*/keyRings/*/cryptoKeys/*` grammar, so a path composes through a builder.
- Envelope custody rides an `EncryptDecrypt` key: `Encrypt` resolves the primary version while `Decrypt` reads the version embedded in the ciphertext and reports `UsedPrimary`, so rotation is a primary repoint through `UpdateCryptoKeyPrimaryVersion` or the key's own `RotationPeriod` schedule and stored ciphertext survives it unrewritten.
- Signing rides an `AsymmetricSign` key version: `AsymmetricSign(CryptoKeyVersionName, Digest)` signs the pre-hashed `OpDigest` in the `Digest` arm matching the version algorithm, and the service answers sign alone, so `GetPublicKey` downloads the `Pem` the keyring verifies against in process.

[STACKING]:
- `Element/identity`: owns the `KmsProvider` axis and the two delegate records this arm binds against the members below.
- `EnvelopeKeyring.Mint`: `GenerateRandomBytesAsync(location, length, Hsm)` mints HSM-born DEK bytes and `EncryptAsync(CryptoKeyName, plaintext)` wraps them.
- `EnvelopeKeyring.MintSealed`: runs that same pair and drops the plaintext DEK at the boundary, so only the wrapped blob leaves.
- `EnvelopeKeyring.Unwrap`: `DecryptAsync(CryptoKeyName, ciphertext)` recovers the plaintext DEK.
- `EnvelopeKeyring.Rewrap`: `UpdateCryptoKeyPrimaryVersionAsync` advances the wrapping version.
- `EnvelopeKeyring.Probe`: `GetCryptoKeyVersionAsync` reads `CryptoKeyVersionState` onto `KeyState`.
- `SigningKeyring.Sign`: `AsymmetricSignAsync(CryptoKeyVersionName, Digest)` over the `Digest`-wrapped `OpDigest`.
- `SigningKeyring.Verify`: an in-process check of `Signature` against the `GetPublicKeyAsync` `Pem`, gated on `PemCrc32C`.
- `api-aws-kms`, `api-azure-keyvault`: peer provider arms binding the same two delegate records against their own members, so one keyring shape serves every `KmsProvider` row.
- `api-protobuf`(`.api/api-protobuf.md`): `ByteString.CopyFrom` admits column and blob bytes and `ToByteArray`/`.Span`/`.Memory` recovers them, so one conversion serves each direction at the boundary.
- `api-grpc-client`(`.api/api-grpc-client.md`): `Google.Api.Gax.Grpc` drives one `GrpcChannel` beneath the client, and a remote fault leaves as `RpcException` for the typed fault fold.
- `api-otel-instrumentation-grpcnetclient`(`.api/api-otel-instrumentation-grpcnetclient.md`): decorates the `GrpcOut` activity every KMS call raises with RPC method and canonical status, registered once at the AppHost root.
- `Runtime/secrets`: `SecretLease` delivers the per-open credential handle the client binds once at composition.

[LOCAL_ADMISSION]:
- `KmsProvider` holds one container-resolved `KeyManagementServiceClient` singleton, and every operation calls the `…Async` twin at the persistence boundary.
- Key identity persists as the typed `CryptoKeyName` string, rebuilt through `CryptoKeyName.FromProjectLocationKeyRingCryptoKey` at composition.
- `EnvelopeAad` rides `AdditionalAuthenticatedData` on every wrap and unwrap, so a DEK wrapped for one partition and tenant recovers under that pair alone.
- Every request sets its `*Crc32C` and every response checksum is checked before the payload crosses the boundary; a mismatch rails the typed `IdentityFault` band under a bounded retry.
- Signing binds an `AsymmetricSign` key version distinct from the `EncryptDecrypt` envelope key, both leased through the one per-open `SecretLease` handle.

[RAIL_LAW]:
- Package: `Google.Cloud.Kms.V1`
- Owns: envelope wrap and unwrap of the data-encryption key and asymmetric signing of the seam `OpDigest` through Cloud KMS — two disjoint surfaces behind the one `KmsProvider.Gcp` arm
- Accept: a container-resolved client singleton, typed resource names, `ByteString` payloads, CRC32C set on request and checked on response, `AsymmetricSign` over a `Digest`-wrapped `OpDigest`, and `GetPublicKey` feeding the in-process verify
- Reject: per-operation client construction, hand-built resource path strings, an unchecked CRC32C payload crossing the boundary, signing under the symmetric envelope key, or a rewrap that rewrites stored ciphertext
