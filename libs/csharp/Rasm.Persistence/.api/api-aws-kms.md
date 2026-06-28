# [RASM_PERSISTENCE_API_AWS_KMS]

`AWSSDK.KeyManagementService` is the AWS SDK for .NET v4 client for AWS KMS: the
`IAmazonKeyManagementService` async operation contract, its concrete `AmazonKeyManagementServiceClient`
root, and the `GenerateDataKey`/`Encrypt`/`Decrypt`/`ReEncrypt`/`GenerateRandom` request and
response shapes for the store envelope-encryption rail. KMS exposes no native key-wrap verb, so
the Persistence `Store/encryption` `EnvelopeKeyring` derives a wrapped DEK via `GenerateDataKey`
and round-trips the wrap through `Encrypt`/`Decrypt`, keyed by the customer master key ARN under
`SYMMETRIC_DEFAULT`, with `EncryptionContext` as binding AAD; `ReEncrypt` rotates the wrapping key
entirely inside KMS without the plaintext DEK crossing the wire. The whole surface is async-only
on the v4 line and pure-managed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AWSSDK.KeyManagementService`
- package: `AWSSDK.KeyManagementService`
- version: `4.0.12.9`
- license: `Apache-2.0`
- assembly: `AWSSDK.KeyManagementService` (`lib/net8.0` binds for the `net10.0` consumer; `netcoreapp3.1`/`netstandard2.0`/`net472` are fallback assets)
- namespace: `Amazon.KeyManagementService`, `Amazon.KeyManagementService.Model`
- asset: pure-managed library (depends on `AWSSDK.Core`; no native asset)
- rail: encryption

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and configuration family
- rail: encryption

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [RAIL]                                       |
| :-----: | :---------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `IAmazonKeyManagementService`             | client contract | async KMS operation surface (v4: no sync twin) |
|  [02]   | `AmazonKeyManagementServiceClient`        | client root     | concrete client, `IDisposable`, long-lived   |
|  [03]   | `AmazonKeyManagementServiceConfig`        | configuration   | region, endpoint, retry, timeout policy      |
|  [04]   | `IKeyManagementServicePaginatorFactory`   | paginator       | `Paginators` property for list-op auto-paging |
|  [05]   | `AmazonKeyManagementServiceException`     | service failure | KMS request-failure base                     |
|  [06]   | `DryRunOperationException`                | dry-run failure | thrown on a would-succeed `DryRun` probe     |

[PUBLIC_TYPE_SCOPE]: envelope request and response shapes
- rail: encryption

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `GenerateDataKeyRequest`                  | request shape  | data-key request with plaintext return       |
|  [02]   | `GenerateDataKeyResponse`                 | response shape | plaintext + wrapped `CiphertextBlob` + `KeyMaterialId` |
|  [03]   | `GenerateDataKeyWithoutPlaintextRequest`  | request shape  | data-key request, wrapped-only               |
|  [04]   | `GenerateDataKeyWithoutPlaintextResponse` | response shape | wrapped `CiphertextBlob` only                |
|  [05]   | `EncryptRequest`                          | request shape  | wrap arbitrary plaintext (the DEK) under a key |
|  [06]   | `EncryptResponse`                         | response shape | `CiphertextBlob`, echoed `EncryptionAlgorithm`, `KeyId` |
|  [07]   | `DecryptRequest`                          | request shape  | unwrap a `CiphertextBlob`                     |
|  [08]   | `DecryptResponse`                         | response shape | recovered `Plaintext`, resolving `KeyId`, echoed `EncryptionAlgorithm`, `KeyMaterialId` |
|  [09]   | `ReEncryptRequest`                        | request shape  | rewrap ciphertext under a new key             |
|  [10]   | `ReEncryptResponse`                       | response shape | rewrapped `CiphertextBlob` + source/destination ARN |
|  [11]   | `GenerateRandomRequest` / `GenerateRandomResponse` | random shape | HSM/FIPS random bytes for off-board DEK material |
|  [12]   | `RecipientInfo`                           | attestation    | Nitro enclave recipient binding (`CiphertextForRecipient`) |

[PUBLIC_TYPE_SCOPE]: algorithm and key-spec vocabularies
- rail: encryption

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                                |
| :-----: | :------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `DataKeySpec`             | constant class | data-key length: `AES_256`, `AES_128`                |
|  [02]   | `EncryptionAlgorithmSpec` | constant class | wrap algorithm: `SYMMETRIC_DEFAULT`, `RSAES_OAEP_SHA_1`, `RSAES_OAEP_SHA_256`, `SM2PKE` |
|  [03]   | `KeySpec`                 | constant class | KMS key-material spec                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope and random operations (all `…Async(request, CancellationToken)`)
- rail: encryption

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `GenerateDataKeyAsync(GenerateDataKeyRequest, ct)`          | data-key mint  | returns plaintext key + wrapped `CiphertextBlob`    |
|  [02]   | `GenerateDataKeyWithoutPlaintextAsync(request, ct)`         | data-key mint  | returns wrapped `CiphertextBlob` only               |
|  [03]   | `EncryptAsync(EncryptRequest, ct)`                          | wrap           | wraps `Plaintext` (the DEK) under `KeyId`           |
|  [04]   | `DecryptAsync(DecryptRequest, ct)`                          | unwrap         | recovers `Plaintext` from a `CiphertextBlob`        |
|  [05]   | `ReEncryptAsync(ReEncryptRequest, ct)`                      | rewrap         | KMS-side rewrap; plaintext never leaves KMS         |
|  [06]   | `GenerateRandomAsync(GenerateRandomRequest, ct)` / `GenerateRandomAsync(int? numberOfBytes, ct)` | random | FIPS/HSM random bytes (off-board DEK material if not using `GenerateDataKey`) |
|  [07]   | `DescribeKeyAsync(string keyId, ct)`                        | key probe      | resolves key metadata (`KeyState`, `KeySpec`, ARN)  |
|  [08]   | `Paginators`                                                | paginator      | `IKeyManagementServicePaginatorFactory` for list ops |
|  [09]   | `new AmazonKeyManagementServiceClient(credentials, region)` | construction   | concrete `IAmazonKeyManagementService` instance     |

[ENTRYPOINT_SCOPE]: `GenerateDataKey` request and response fields
- rail: encryption

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `GenerateDataKeyRequest.KeyId`             | request field  | symmetric KMS key that wraps the data key       |
|  [02]   | `GenerateDataKeyRequest.KeySpec`           | request field  | `DataKeySpec` length selector                   |
|  [03]   | `GenerateDataKeyRequest.NumberOfBytes`     | request field  | `int?` explicit byte length (`1`–`1024`)        |
|  [04]   | `GenerateDataKeyRequest.EncryptionContext` | request field  | `Dictionary<string,string>` non-secret AAD, exact match required to unwrap |
|  [05]   | `GenerateDataKeyRequest.GrantTokens`       | request field  | up to `10` eventual-consistency grant tokens    |
|  [06]   | `GenerateDataKeyRequest.DryRun`            | request field  | `bool?` permission probe without effect          |
|  [07]   | `GenerateDataKeyResponse.Plaintext`        | response field | `MemoryStream` DEK, sensitive, `1`–`4096` bytes |
|  [08]   | `GenerateDataKeyResponse.CiphertextBlob`   | response field | wrapped DEK, `1`–`6144` bytes                   |
|  [09]   | `GenerateDataKeyResponse.KeyId`            | response field | wrapping key ARN                                |
|  [10]   | `GenerateDataKeyResponse.KeyMaterialId`    | response field | key-material identifier; omitted with a recipient |

[ENTRYPOINT_SCOPE]: `Encrypt` and `Decrypt` request and response fields
- rail: encryption

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :----------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `EncryptRequest.KeyId`               | request field  | required wrapping key, `1`–`2048` chars        |
|  [02]   | `EncryptRequest.Plaintext`           | request field  | required `MemoryStream`, sensitive, `1`–`4096` |
|  [03]   | `EncryptRequest.EncryptionAlgorithm` | request field  | `EncryptionAlgorithmSpec` selector             |
|  [04]   | `EncryptRequest.EncryptionContext`   | request field  | `Dictionary<string,string>` AAD bound to the ciphertext |
|  [05]   | `EncryptResponse.CiphertextBlob`     | response field | wrapped output, `1`–`6144` bytes               |
|  [06]   | `EncryptResponse.KeyId` / `.EncryptionAlgorithm` | response field | resolving key ARN + echoed algorithm      |
|  [07]   | `DecryptRequest.CiphertextBlob`      | request field  | wrapped input, `1`–`6144` bytes                |
|  [08]   | `DecryptRequest.KeyId`               | request field  | optional explicit key for symmetric ciphertext |
|  [09]   | `DecryptRequest.EncryptionContext`   | request field  | AAD; must match the wrap context exactly       |
|  [10]   | `DecryptRequest.EncryptionAlgorithm` | request field  | required for asymmetric ciphertext             |
|  [11]   | `DecryptRequest.Recipient` / `DryRun` / `DryRunModifiers` | request field | `RecipientInfo` enclave binding; permission probe |
|  [12]   | `DecryptResponse.Plaintext`          | response field | recovered `MemoryStream`, sensitive            |
|  [13]   | `DecryptResponse.KeyId` / `.EncryptionAlgorithm` / `.KeyMaterialId` | response field | key ARN that decrypted + echoed algorithm + material id |

[ENTRYPOINT_SCOPE]: `ReEncrypt` rewrap fields
- rail: encryption

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                        |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `ReEncryptRequest.CiphertextBlob`               | request field  | existing wrapped ciphertext, `1`–`6144` bytes |
|  [02]   | `ReEncryptRequest.DestinationKeyId`             | request field  | required new wrapping key                     |
|  [03]   | `ReEncryptRequest.SourceKeyId`                  | request field  | source key for asymmetric source ciphertext   |
|  [04]   | `ReEncryptRequest.SourceEncryptionContext`      | request field  | AAD that bound the source ciphertext          |
|  [05]   | `ReEncryptRequest.DestinationEncryptionContext` | request field  | AAD bound to the rewrapped output             |
|  [06]   | `ReEncryptResponse.CiphertextBlob`              | response field | rewrapped output                              |
|  [07]   | `ReEncryptResponse.KeyId` / `.KeyMaterialId`    | response field | destination key ARN + destination material id |
|  [08]   | `ReEncryptResponse.SourceKeyId`                 | response field | source key ARN that unwrapped the input       |

## [04]-[IMPLEMENTATION_LAW]

[KMS_TOPOLOGY]:
- `IAmazonKeyManagementService` is the operation contract; `AmazonKeyManagementServiceClient` is the long-lived concrete root and is `IDisposable`. `Paginators` exposes the list-operation auto-pager.
- every operation is async-only: the v4 line exposes `…Async(request, CancellationToken)` and carries no synchronous twin.
- `CiphertextBlob`, `Plaintext`, and `CiphertextForRecipient` are `MemoryStream`; outside the HTTP API and CLI the bytes are raw, never Base64.
- `DataKeySpec` is `AES_256` or `AES_128`; either set `KeySpec` or set `NumberOfBytes` (`1`–`1024`), never both.
- `EncryptionAlgorithmSpec` is `SYMMETRIC_DEFAULT`, `RSAES_OAEP_SHA_1`, `RSAES_OAEP_SHA_256`, or `SM2PKE`; symmetric envelope work uses `SYMMETRIC_DEFAULT` and the response echoes the resolved algorithm.
- `EncryptionContext` is a `Dictionary<string,string>` of non-secret AAD logged in CloudTrail; the exact case-sensitive map supplied at wrap is required at unwrap, and it binds only to symmetric KMS keys.
- `GrantTokens` accepts up to `10` tokens to cover grants not yet at eventual consistency; `DryRun` (with `DryRunModifiers`) probes permission without effect and raises `DryRunOperationException` on a would-succeed probe.
- `KeyMaterialId` on `GenerateDataKey`/`Decrypt`/`ReEncrypt` responses pins the exact key material used; it is omitted when the request carries a `Recipient` attestation, in which case `CiphertextForRecipient` carries the enclave-bound blob instead of `Plaintext`.

[ENVELOPE_LAW]:
- KMS has no native wrap verb. The wrap of a data-encryption key is `Encrypt` and the unwrap is `Decrypt`, both keyed by the customer master key ARN under `SYMMETRIC_DEFAULT`.
- `GenerateDataKey` mints a fresh DEK and returns it twice: `Plaintext` for immediate local AES use and `CiphertextBlob` as the already-wrapped form to persist beside the ciphertext.
- `GenerateDataKeyWithoutPlaintext` returns only the wrapped `CiphertextBlob`; use it when the minting node never performs local encryption and the DEK is unwrapped later on the read path.
- `ReEncrypt` rotates the wrapping key entirely inside KMS — the DEK plaintext never crosses the wire — so key rotation rewraps stored blobs without a managed `Decrypt`-then-`Encrypt` round trip.
- `GenerateRandom` returns FIPS/HSM-grade random bytes; it is the off-board source when a DEK is generated outside `GenerateDataKey` (e.g. a locally-encrypted-then-`Encrypt`-wrapped DEK) and parallels Cloud KMS `GenerateRandomBytes`.

[STACK_INTEGRATION]:
- this client is the AWS arm of the polymorphic `Store/encryption` `KmsProvider` `[SmartEnum<string>]` axis (AWS/Azure/GCP/none): the page projects one provider-neutral `EnvelopeKeyring(Mint, Unwrap, Rotate)` delegate triple, and the AWS arm binds `Mint`→`GenerateDataKeyAsync(KeyId, KeySpec=AES_256)`, `Unwrap`→`DecryptAsync(CiphertextBlob, EncryptionContext)`, `Rotate`→`ReEncryptAsync(DestinationKeyId)`. Azure KeyVault and Google Cloud KMS bind the same three delegates against their own wrap/unwrap members (`api-azure-keyvault`, `api-google-kms`), so the keyring is the single dense surface and a per-provider keying class is the deleted form.
- the concrete client is constructed once per configured KMS key at composition (never per operation) and consumes the per-open CMK access handle delivered by the AppHost `Runtime/config#SECRET_LEASE` lease through the `Store/encryption ⇄ Rasm.AppHost/Runtime # [PORT]: KMS-unwrap port` seam; this owner holds the `EncryptionContext` AAD binding and the `KmsProvider` client, while the credential lifecycle is the runtime lease's concern.
- the recovered `Plaintext` DEK rehydrates the local cipher (the SQLCipher `PRAGMA key`/`rekey` ceremony, or the SSE-KMS object key) and is zeroed immediately after the local bind, so a persisted plaintext DEK is the deleted form; the `EncryptionContext` carries the store partition identity (and tenant id under RLS) on every wrap and unwrap, so a blob cannot be unwrapped under a foreign partition.

[LOCAL_ADMISSION]:
- `KmsProvider` holds one `IAmazonKeyManagementService` per configured KMS key; the concrete client is constructed once at composition, never per operation.
- the wrap path is `GenerateDataKey(KeyId, KeySpec=AES_256)`: the returned `Plaintext` drives local symmetric encryption and the returned `CiphertextBlob` is the persisted wrapped DEK.
- the unwrap path is `Decrypt(CiphertextBlob, EncryptionContext)`; the recovered `Plaintext` rehydrates the DEK and is zeroed immediately after the local decrypt.
- `EncryptionContext` carries the store partition identity as AAD on both wrap and unwrap, so a blob cannot be unwrapped under a foreign partition.
- key rotation rewraps persisted blobs through `ReEncrypt(DestinationKeyId)`; the plaintext DEK never re-enters managed memory.

[RAIL_LAW]:
- Package: `AWSSDK.KeyManagementService`
- Owns: envelope wrap and unwrap of the data-encryption key through AWS KMS
- Accept: one `IAmazonKeyManagementService` per key, `GenerateDataKey` for mint, `Encrypt`/`Decrypt` for the wrap round trip, `ReEncrypt` for rotation, `EncryptionContext` AAD on every call
- Reject: a native KMS wrap verb, per-operation client construction, plaintext DEK persistence, or unwrap without the binding `EncryptionContext`
