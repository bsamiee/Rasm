# [RASM_PERSISTENCE_API_AWS_KMS]

`AWSSDK.KeyManagementService` is the AWS SDK for .NET client for AWS KMS: the `KmsProvider.Aws` arm of two disjoint Persistence delegate surfaces the `Element/identity` `KmsProvider` axis selects. Its ENVELOPE arm wraps a data-encryption key under a symmetric customer master key; its SIGNING arm signs an `OpDigest` over an asymmetric key, feeding the `SigningKeyring`. Every operation is async-only and pure-managed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AWSSDK.KeyManagementService`
- package: `AWSSDK.KeyManagementService` (`Apache-2.0`, Amazon Web Services)
- assembly: `AWSSDK.KeyManagementService` (`lib/net8.0` binds the `net10.0` consumer; `netstandard2.0`/`net472` fallbacks)
- namespace: `Amazon.KeyManagementService`, `Amazon.KeyManagementService.Model`
- depends: `AWSSDK.Core`; pure-managed, no native asset
- rail: envelope DEK wrap/unwrap and asymmetric `OpDigest` sign/verify — the AWS arm of the `Element/identity` `KmsProvider` axis

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and configuration family

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]   | [CAPABILITY]                                   |
| :-----: | :-------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `IAmazonKeyManagementService`           | client contract | async KMS operation contract; no sync twin     |
|  [02]   | `AmazonKeyManagementServiceClient`      | client root     | concrete client, `IDisposable`, long-lived     |
|  [03]   | `AmazonKeyManagementServiceConfig`      | configuration   | region, endpoint, retry, timeout policy        |
|  [04]   | `IKeyManagementServicePaginatorFactory` | paginator       | `Paginators` property for list-op auto-paging  |
|  [05]   | `AmazonKeyManagementServiceException`   | service failure | KMS request-failure base                       |
|  [06]   | `DryRunOperationException`              | dry-run failure | thrown on a permission-positive `DryRun` probe |

[PUBLIC_TYPE_SCOPE]: envelope request and response shapes

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY]  | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `GenerateDataKeyRequest`                           | request shape  | data-key request keyed by `KeyId`, `KeySpec`, `EncryptionContext` |
|  [02]   | `GenerateDataKeyResponse`                          | response shape | `Plaintext` + wrapped `CiphertextBlob` + `KeyMaterialId`          |
|  [03]   | `GenerateDataKeyWithoutPlaintextRequest`           | request shape  | data-key request, wrapped-only                                    |
|  [04]   | `GenerateDataKeyWithoutPlaintextResponse`          | response shape | wrapped `CiphertextBlob` only                                     |
|  [05]   | `EncryptRequest`                                   | request shape  | wrap arbitrary plaintext (the DEK) under a key                    |
|  [06]   | `EncryptResponse`                                  | response shape | `CiphertextBlob`, echoed `EncryptionAlgorithm`, `KeyId`           |
|  [07]   | `DecryptRequest`                                   | request shape  | unwrap a `CiphertextBlob` under `EncryptionContext`               |
|  [08]   | `DecryptResponse`                                  | response shape | `Plaintext` + `KeyId`/`EncryptionAlgorithm`/`KeyMaterialId`       |
|  [09]   | `ReEncryptRequest`                                 | request shape  | rewrap ciphertext under `DestinationKeyId`                        |
|  [10]   | `ReEncryptResponse`                                | response shape | rewrapped `CiphertextBlob` + source/destination ARN               |
|  [11]   | `GenerateRandomRequest` / `GenerateRandomResponse` | random shape   | HSM/FIPS random bytes for off-board DEK material                  |
|  [12]   | `RecipientInfo`                                    | attestation    | Nitro enclave recipient binding (`CiphertextForRecipient`)        |

[PUBLIC_TYPE_SCOPE]: algorithm and key-spec vocabularies

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                                            |
| :-----: | :------------------------ | :------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `DataKeySpec`             | constant class | data-key length: `AES_256`, `AES_128`                                                   |
|  [02]   | `EncryptionAlgorithmSpec` | constant class | wrap algorithm: `SYMMETRIC_DEFAULT`, `RSAES_OAEP_SHA_1`, `RSAES_OAEP_SHA_256`, `SM2PKE` |
|  [03]   | `KeySpec`                 | constant class | KMS key-material spec                                                                   |

[PUBLIC_TYPE_SCOPE]: signing request, response, and algorithm vocabulary

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]    | [CAPABILITY]                                                     |
| :-----: | :--------------------------------------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `SignRequest`                                  | request shape    | signs the `OpDigest` under `KeyId`                               |
|  [02]   | `SignResponse`                                 | response shape   | `KeyId`, `Signature` (`MemoryStream`), echoed `SigningAlgorithm` |
|  [03]   | `VerifyRequest`                                | request shape    | verifies a `Signature` against the digest                        |
|  [04]   | `VerifyResponse`                               | response shape   | `KeyId`, `SignatureValid` (`bool?`), echoed `SigningAlgorithm`   |
|  [05]   | `SigningAlgorithmSpec`                         | constant class   | the sign-algorithm vocabulary ([ALGOS] below)                    |
|  [06]   | `MessageType`                                  | constant class   | `DIGEST` (pre-hashed `OpDigest`), `RAW`, `EXTERNAL_MU`           |
|  [07]   | `GetPublicKeyRequest` / `GetPublicKeyResponse` | key-export shape | downloads the public key for outside-KMS verification            |

[ALGOS]: `ECDSA_SHA_256`/`ECDSA_SHA_384`/`ECDSA_SHA_512`, `RSASSA_PSS_SHA_256/384/512`, `RSASSA_PKCS1_V1_5_SHA_256/384/512`, `ED25519_SHA_512`/`ED25519_PH_SHA_512`, `ML_DSA_SHAKE_256`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope and random operations, each `…Async(request, CancellationToken)`

| [INDEX] | [SURFACE]                                                             | [SHAPE]       | [CAPABILITY]                                    |
| :-----: | :-------------------------------------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `GenerateDataKeyAsync(GenerateDataKeyRequest)`                        | data-key mint | plaintext key + wrapped `CiphertextBlob`        |
|  [02]   | `GenerateDataKeyWithoutPlaintextAsync(request)`                       | data-key mint | wrapped `CiphertextBlob` only                   |
|  [03]   | `EncryptAsync(EncryptRequest)`                                        | wrap          | wraps `Plaintext` (the DEK) under `KeyId`       |
|  [04]   | `DecryptAsync(DecryptRequest)`                                        | unwrap        | recovers `Plaintext` from a `CiphertextBlob`    |
|  [05]   | `ReEncryptAsync(ReEncryptRequest)`                                    | rewrap        | KMS-side rewrap; plaintext never leaves KMS     |
|  [06]   | `GenerateRandomAsync(GenerateRandomRequest)` / `(int? numberOfBytes)` | random        | FIPS/HSM random bytes (off-board DEK source)    |
|  [07]   | `DescribeKeyAsync(string keyId)`                                      | key probe     | key metadata (`KeyState`, `KeySpec`, ARN)       |
|  [08]   | `Paginators`                                                          | paginator     | `IKeyManagementServicePaginatorFactory`         |
|  [09]   | `new AmazonKeyManagementServiceClient(credentials, region)`           | construction  | concrete `IAmazonKeyManagementService` instance |

[ENTRYPOINT_SCOPE]: signing operations, each `…Async(request, CancellationToken)`

| [INDEX] | [SURFACE]                         | [SHAPE]    | [CAPABILITY]                                            |
| :-----: | :-------------------------------- | :--------- | :------------------------------------------------------ |
|  [01]   | `SignAsync(SignRequest)`          | sign       | signs the `OpDigest` under `KeyId` → `Signature`        |
|  [02]   | `VerifyAsync(VerifyRequest)`      | verify     | verifies `Signature`; `SignatureValid` the verdict      |
|  [03]   | `GetPublicKeyAsync(string keyId)` | key export | downloads the public key for an outside-KMS verify path |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `IAmazonKeyManagementService` is the operation contract; `AmazonKeyManagementServiceClient` is the long-lived `IDisposable` concrete root; every operation is async-only (`…Async(request, CancellationToken)`), no sync twin, and `Paginators` exposes the list-op auto-pager.
- `CiphertextBlob`, `Plaintext`, `CiphertextForRecipient`, and the signing `Message`/`Signature` are raw `MemoryStream` bytes, never Base64 outside the HTTP API and CLI.
- KMS exposes no native wrap verb: the envelope arm wraps a DEK through `GenerateDataKey` + `Encrypt` and unwraps through `Decrypt`, keyed by the customer master key ARN under `SYMMETRIC_DEFAULT`; a mint sets `KeySpec` (`AES_256`/`AES_128`) or `NumberOfBytes`, never both.
- `ReEncrypt` rotates the wrapping key entirely inside KMS, so the plaintext DEK never crosses the wire; `GenerateDataKeyWithoutPlaintext` returns only the wrapped blob for a mint node that never encrypts locally; `GenerateRandom` is the off-board FIPS/HSM DEK source.
- `EncryptionContext` is a `Dictionary<string,string>` of non-secret AAD; the exact case-sensitive map supplied at wrap is required at unwrap and binds only to symmetric keys. `GrantTokens` carries up to 10 not-yet-consistent grants; `DryRun` probes permission and raises `DryRunOperationException` on a permission-positive probe.
- `KeyMaterialId` pins the exact key material on `GenerateDataKey`/`Decrypt`/`ReEncrypt` responses; a `Recipient` attestation omits it and returns the enclave-bound blob on `CiphertextForRecipient` instead of `Plaintext`.
- `SigningKeyring` and `EnvelopeKeyring` bind two different keys behind the one `KmsProvider.Aws` arm: `Sign`/`Verify` operate over an asymmetric key (ECDSA/RSA-PSS/RSA-PKCS1/Ed25519/ML-DSA) disjoint from the symmetric envelope CMK.
- `Sign` takes the precomputed `OpDigest` under `MessageType.DIGEST` (the seam already hashed the op bytes), so KMS signs the supplied digest directly; `MessageType.RAW` (KMS hashes) is the rejected path for an already-hashed digest. `Verify` returns `SignatureValid` (`bool?`) the keyring lifts to a verdict; `GetPublicKey` is the outside-KMS verification path.

[STACKING]:
- `Element/identity`: owns the `KmsProvider` `[SmartEnum<string>]` axis (AWS/Azure/GCP/none) and both delegate surfaces this arm binds — the `EnvelopeKeyring(Mint, Unwrap, Rewrap, Probe)` DEK quartet (`Mint`→`GenerateDataKeyAsync`, `Unwrap`→`DecryptAsync`, `Rewrap`→`ReEncryptAsync`, `Probe`→`DescribeKeyAsync` `KeyState`) and the `SigningKeyring` `Sign`/`Verify` pair; `api-azure-keyvault`/`api-google-kms` bind the same delegates against their own members, so a per-provider keying class is the deleted form.
- `Store/blobstore`: `ObjectEncryption` carries the wrapped DEK as SSE key material; the signing arm produces the `Signature` the `SignedAuthorship` blame attestation `Element/identity` owns.
- `Runtime/secrets`: `SecretLease` delivers the per-open CMK access handle the client binds once per configured KMS key at composition; this owner holds the `EncryptionContext` AAD binding and the client, the lease owns the credential lifecycle.

[LOCAL_ADMISSION]:
- `KmsProvider` holds one `IAmazonKeyManagementService` per configured KMS key, constructed once at composition, never per operation.
- wrap: `GenerateDataKey(KeyId, KeySpec=AES_256)` — `Plaintext` drives local symmetric encryption and `CiphertextBlob` persists as the wrapped DEK.
- unwrap: `Decrypt(CiphertextBlob, EncryptionContext)` — the recovered `Plaintext` rehydrates the DEK (the SQLCipher `PRAGMA key`/`rekey` ceremony or the SSE-KMS object key) and is zeroed immediately after the local bind, so a persisted plaintext DEK is the deleted form.
- `EncryptionContext` carries the store partition identity (tenant id under RLS) as AAD on every wrap and unwrap, so a blob cannot be unwrapped under a foreign partition.
- rotation rewraps persisted blobs through `ReEncrypt(DestinationKeyId)`; the plaintext DEK never re-enters managed memory.
- sign: `Sign(SignRequest{ KeyId, Message=opDigest, MessageType=DIGEST, SigningAlgorithm })` over the asymmetric key returns the `Signature` the `SignedAuthorship` carries, and `Verify(VerifyRequest)` returns `SignatureValid` lifted to `Authentic`/`Forged`; the signing key is distinct from the envelope CMK, both leased through the same per-open `SecretLease` handle.
- `SigningAlgorithmSpec` maps from `SigningAlgorithm.WireName` (`ECDSA_SHA_256`↔`es256`, `RSASSA_PSS_SHA_256`↔`ps256`, `RSASSA_PKCS1_V1_5_SHA_256`↔`rs256`, and the 384/512 widths) at the keyring delegate edge.
- `KmsProvider.None` (the local tier) never reaches this surface — attest and verify short to `Unsigned`, so a store with no KMS still records the delta→actor binding, never a fabricated signature.

[RAIL_LAW]:
- Package: `AWSSDK.KeyManagementService`
- Owns: envelope wrap/unwrap of the data-encryption key AND asymmetric Sign/Verify of the seam `OpDigest` through AWS KMS — two disjoint surfaces behind the one `KmsProvider.Aws` arm
- Accept: one `IAmazonKeyManagementService` per key, `GenerateDataKey` for the DEK mint + `Encrypt`/`Decrypt` for the wrap round trip + `ReEncrypt` for rotation with `EncryptionContext` AAD on every call, and `SignAsync(MessageType.DIGEST)`/`VerifyAsync` over an asymmetric key feeding the `SigningKeyring`
- Reject: a native KMS wrap verb, per-operation client construction, plaintext DEK persistence, unwrap without the binding `EncryptionContext`, `MessageType.RAW` for an already-hashed `OpDigest`, or signing over the symmetric envelope CMK
