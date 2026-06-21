# [RASM_PERSISTENCE_API_AWS_KMS]

`AWSSDK.KeyManagementService` supplies the `IAmazonKeyManagementService` client and the
`GenerateDataKey`/`Encrypt`/`Decrypt`/`ReEncrypt` request and response shapes for the store
envelope-encryption rail. The `KmsProvider` wraps and unwraps the data-encryption key through
KMS, deriving the wrapped key via `GenerateDataKey` and round-tripping the wrap through
`Encrypt`/`Decrypt` because KMS exposes no native wrap verb.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AWSSDK.KeyManagementService`
- package: `AWSSDK.KeyManagementService`
- assembly: `AWSSDK.KeyManagementService`
- namespace: `Amazon.KeyManagementService`, `Amazon.KeyManagementService.Model`
- asset: runtime library
- rail: encryption

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and configuration family
- rail: encryption

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [RAIL]                                  |
| :-----: | :------------------------------------ | :-------------- | :-------------------------------------- |
|  [01]   | `IAmazonKeyManagementService`         | client contract | async KMS operation surface             |
|  [02]   | `AmazonKeyManagementServiceClient`    | client root     | concrete client, `IDisposable`          |
|  [03]   | `AmazonKeyManagementServiceConfig`    | configuration   | region, endpoint, retry, timeout policy |
|  [04]   | `AmazonKeyManagementServiceException` | service failure | KMS request failure base                |

[PUBLIC_TYPE_SCOPE]: envelope request and response shapes
- rail: encryption

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `GenerateDataKeyRequest`                  | request shape  | data-key request with plaintext return       |
|  [02]   | `GenerateDataKeyResponse`                 | response shape | plaintext plus wrapped `CiphertextBlob`      |
|  [03]   | `GenerateDataKeyWithoutPlaintextRequest`  | request shape  | data-key request, wrapped-only               |
|  [04]   | `GenerateDataKeyWithoutPlaintextResponse` | response shape | wrapped `CiphertextBlob` only                |
|  [05]   | `EncryptRequest`                          | request shape  | wrap arbitrary plaintext under a key         |
|  [06]   | `EncryptResponse`                         | response shape | `CiphertextBlob` plus echoed algorithm       |
|  [07]   | `DecryptRequest`                          | request shape  | unwrap a `CiphertextBlob`                    |
|  [08]   | `DecryptResponse`                         | response shape | recovered `Plaintext` plus resolving key ARN |
|  [09]   | `ReEncryptRequest`                        | request shape  | rewrap ciphertext under a new key            |
|  [10]   | `ReEncryptResponse`                       | response shape | rewrapped `CiphertextBlob` with source ARN   |
|  [11]   | `RecipientInfo`                           | attestation    | Nitro enclave recipient binding              |

[PUBLIC_TYPE_SCOPE]: algorithm and key-spec vocabularies
- rail: encryption

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [RAIL]                                        |
| :-----: | :------------------------ | :------------- | :-------------------------------------------- |
|  [01]   | `DataKeySpec`             | constant class | data-key length: `AES_256`, `AES_128`         |
|  [02]   | `EncryptionAlgorithmSpec` | constant class | wrap algorithm: `SYMMETRIC_DEFAULT`, RSA, SM2 |
|  [03]   | `KeySpec`                 | constant class | KMS key material spec                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: envelope operations
- rail: encryption

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :---------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `GenerateDataKeyAsync(GenerateDataKeyRequest, ct)`          | data-key mint  | returns plaintext key plus wrapped `CiphertextBlob` |
|  [02]   | `GenerateDataKeyWithoutPlaintextAsync(request, ct)`         | data-key mint  | returns wrapped `CiphertextBlob` only               |
|  [03]   | `EncryptAsync(EncryptRequest, ct)`                          | wrap           | wraps `Plaintext` (the DEK) under `KeyId`           |
|  [04]   | `DecryptAsync(DecryptRequest, ct)`                          | unwrap         | recovers `Plaintext` from a `CiphertextBlob`        |
|  [05]   | `ReEncryptAsync(ReEncryptRequest, ct)`                      | rewrap         | KMS-side rewrap, plaintext never leaves KMS         |
|  [06]   | `new AmazonKeyManagementServiceClient(credentials, region)` | construction   | concrete `IAmazonKeyManagementService` instance     |

[ENTRYPOINT_SCOPE]: `GenerateDataKey` request and response fields
- rail: encryption

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `GenerateDataKeyRequest.KeyId`             | request field  | symmetric KMS key that wraps the data key       |
|  [02]   | `GenerateDataKeyRequest.KeySpec`           | request field  | `DataKeySpec` length selector                   |
|  [03]   | `GenerateDataKeyRequest.NumberOfBytes`     | request field  | explicit byte length, `1`–`1024`                |
|  [04]   | `GenerateDataKeyRequest.EncryptionContext` | request field  | non-secret AAD, exact match required to unwrap  |
|  [05]   | `GenerateDataKeyRequest.GrantTokens`       | request field  | up to `10` eventual-consistency grant tokens    |
|  [06]   | `GenerateDataKeyRequest.DryRun`            | request field  | permission probe without effect                 |
|  [07]   | `GenerateDataKeyResponse.Plaintext`        | response field | `MemoryStream` DEK, sensitive, `1`–`4096` bytes |
|  [08]   | `GenerateDataKeyResponse.CiphertextBlob`   | response field | wrapped DEK, `1`–`6144` bytes                   |
|  [09]   | `GenerateDataKeyResponse.KeyId`            | response field | wrapping key ARN                                |
|  [10]   | `GenerateDataKeyResponse.KeyMaterialId`    | response field | key-material identifier; omitted with recipient |

[ENTRYPOINT_SCOPE]: `Encrypt` and `Decrypt` request and response fields
- rail: encryption

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :----------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `EncryptRequest.KeyId`               | request field  | required wrapping key, `1`–`2048` chars        |
|  [02]   | `EncryptRequest.Plaintext`           | request field  | required `MemoryStream`, sensitive, `1`–`4096` |
|  [03]   | `EncryptRequest.EncryptionAlgorithm` | request field  | `EncryptionAlgorithmSpec` selector             |
|  [04]   | `EncryptRequest.EncryptionContext`   | request field  | AAD bound to the ciphertext                    |
|  [05]   | `EncryptResponse.CiphertextBlob`     | response field | wrapped output, `1`–`6144` bytes               |
|  [06]   | `EncryptResponse.KeyId`              | response field | resolving key ARN                              |
|  [07]   | `DecryptRequest.CiphertextBlob`      | request field  | wrapped input, `1`–`6144` bytes                |
|  [08]   | `DecryptRequest.KeyId`               | request field  | optional explicit key for symmetric ciphertext |
|  [09]   | `DecryptRequest.EncryptionContext`   | request field  | AAD; must match the wrap context exactly       |
|  [10]   | `DecryptRequest.Recipient`           | request field  | `RecipientInfo` enclave attestation binding    |
|  [11]   | `DecryptResponse.Plaintext`          | response field | recovered `MemoryStream`, sensitive            |
|  [12]   | `DecryptResponse.KeyId`              | response field | key ARN that decrypted the ciphertext          |

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
|  [07]   | `ReEncryptResponse.KeyId`                       | response field | destination key ARN                           |
|  [08]   | `ReEncryptResponse.SourceKeyId`                 | response field | source key ARN that unwrapped the input       |

## [04]-[IMPLEMENTATION_LAW]

[KMS_TOPOLOGY]:
- `IAmazonKeyManagementService` is the operation contract; `AmazonKeyManagementServiceClient` is the long-lived concrete root and is `IDisposable`.
- Every operation is async-only: the v4 line exposes `…Async(request, CancellationToken)` and carries no synchronous twin.
- `CiphertextBlob`, `Plaintext`, and `CiphertextForRecipient` are `MemoryStream`; outside the HTTP API and CLI the bytes are raw, never Base64.
- `DataKeySpec` is `AES_256` or `AES_128`; either set `KeySpec` or set `NumberOfBytes` (`1`–`1024`), never both.
- `EncryptionAlgorithmSpec` is `SYMMETRIC_DEFAULT`, `RSAES_OAEP_SHA_1`, `RSAES_OAEP_SHA_256`, or `SM2PKE`; symmetric envelope work uses `SYMMETRIC_DEFAULT`.
- `EncryptionContext` is non-secret AAD logged in CloudTrail; the exact case-sensitive map supplied at wrap time is required at unwrap time, and it binds only to symmetric KMS keys.
- `GrantTokens` accepts up to `10` tokens to cover grants that have not reached eventual consistency.
- `DryRun` probes permission without performing the operation; KMS raises `DryRunOperationException` on a would-succeed probe.

[ENVELOPE_LAW]:
- KMS has no native wrap verb. The wrap of a data-encryption key is `Encrypt` and the unwrap is `Decrypt`, both keyed by the customer master key ARN under `SYMMETRIC_DEFAULT`.
- `GenerateDataKey` mints a fresh DEK and returns it twice: `Plaintext` for immediate local AES use and `CiphertextBlob` as the already-wrapped form to persist beside the ciphertext.
- `GenerateDataKeyWithoutPlaintext` returns only the wrapped `CiphertextBlob`; use it when the minting node never performs local encryption and the DEK is unwrapped later on the read path.
- `ReEncrypt` rotates the wrapping key entirely inside KMS — the DEK plaintext never crosses the wire — so key rotation rewraps stored blobs without a managed `Decrypt`-then-`Encrypt` round trip.
- `KeyMaterialId` on a `GenerateDataKey`/`ReEncrypt` response pins the exact key material used; it is omitted whenever the request carries a `Recipient` attestation.

[LOCAL_ADMISSION]:
- `KmsProvider` holds one `IAmazonKeyManagementService` per configured KMS key; the concrete client is constructed once at composition, never per operation.
- The wrap path is `GenerateDataKey(KeyId, KeySpec=AES_256)`: the returned `Plaintext` drives local symmetric encryption and the returned `CiphertextBlob` is the persisted wrapped DEK.
- The unwrap path is `Decrypt(CiphertextBlob, EncryptionContext)`; the recovered `Plaintext` rehydrates the DEK and is zeroed immediately after the local decrypt.
- `EncryptionContext` carries the store partition identity as AAD on both wrap and unwrap, so a blob cannot be unwrapped under a foreign partition.
- Key rotation rewraps persisted blobs through `ReEncrypt(DestinationKeyId)`; the plaintext DEK never re-enters managed memory.

[RAIL_LAW]:
- Package: `AWSSDK.KeyManagementService`
- Owns: envelope wrap and unwrap of the data-encryption key through AWS KMS
- Accept: one `IAmazonKeyManagementService` per key, `GenerateDataKey` for mint, `Encrypt`/`Decrypt` for the wrap round trip, `ReEncrypt` for rotation, `EncryptionContext` AAD on every call
- Reject: a native KMS wrap verb, per-operation client construction, plaintext DEK persistence, or unwrap without the binding `EncryptionContext`
