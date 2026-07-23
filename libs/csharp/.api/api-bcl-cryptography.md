# [RASM_API_BCL_CRYPTOGRAPHY]

`System.Security.Cryptography` owns RFC-7468 armor, X.509 admission and export, AEAD sealing, ECDSA attestation, and buffer zeroization for every credential wire, sealed object, and signed receipt the branch mints. Each allocating form carries a caller-buffer span twin sized by the surface's own probe, so credential material rides one rented destination from armor through overwrite. Confidentiality and authenticity are the claims this surface binds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.Security.Cryptography`
- package: `System.Security.Cryptography` (MIT)
- assembly: `System.Security.Cryptography.dll` (shared framework)
- namespace: `System.Security.Cryptography`, `System.Security.Cryptography.X509Certificates`
- rail: credential-armor, object-seal, attestation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: armor, certificate, seal, signature, and integrity owners

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]  | [CAPABILITY]                                |
| :-----: | :----------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `PemEncoding`                        | static class   | RFC-7468 armor write and locate             |
|  [02]   | `PemFields`                          | struct         | armor element ranges over the source        |
|  [03]   | `X509CertificateLoader`              | static class   | DER and PKCS#12 certificate admission       |
|  [04]   | `Pkcs12LoaderLimits`                 | class          | parse bounds a PKCS#12 load binds           |
|  [05]   | `X509Certificate2`                   | class          | certificate identity and PEM round-trip     |
|  [06]   | `ECDsaCertificateExtensions`         | static class   | certificate-to-`ECDsa` key binding          |
|  [07]   | `AesGcm`                             | sealed class   | authenticated encryption under a caller key |
|  [08]   | `AuthenticationTagMismatchException` | exception      | forged-tag signal an open raises            |
|  [09]   | `ECDsa`                              | abstract class | elliptic-curve sign and verify              |
|  [10]   | `DSASignatureFormat`                 | enum           | signature wire framing selector             |
|  [11]   | `CryptographicOperations`            | static class   | zeroization and constant-time integrity     |
|  [12]   | `RandomNumberGenerator`              | abstract class | cryptographic entropy fill and draw         |
|  [13]   | `HashAlgorithmName`                  | struct         | digest algorithm selector on every call     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: RFC-7468 armor (`PemEncoding`)

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `GetEncodedSize(int, int) -> int`                                           | static  | size a destination before a write |
|  [02]   | `WriteString(ReadOnlySpan<char>, ReadOnlySpan<byte>) -> string`             | static  | armor one element as text         |
|  [03]   | `Write(ReadOnlySpan<char>, ReadOnlySpan<byte>) -> char[]`                   | static  | armor into a fresh char array     |
|  [04]   | `WriteUtf8(ReadOnlySpan<byte>, ReadOnlySpan<byte>) -> byte[]`               | static  | armor a byte-framed wire          |
|  [05]   | `TryWrite(ReadOnlySpan<char>, ReadOnlySpan<byte>, Span<char>, out int)`     | static  | armor into a rented span          |
|  [06]   | `TryWriteUtf8(ReadOnlySpan<byte>, ReadOnlySpan<byte>, Span<byte>, out int)` | static  | UTF-8 armor into a rented span    |
|  [07]   | `TryFind(ReadOnlySpan<char>, out PemFields) -> bool`                        | static  | locate the next element           |
|  [08]   | `TryFindUtf8(ReadOnlySpan<byte>, out PemFields) -> bool`                    | static  | locate over a byte-framed bundle  |
|  [09]   | `Find(ReadOnlySpan<char>) -> PemFields`                                     | static  | locate; throws on absent armor    |
|  [10]   | `FindUtf8(ReadOnlySpan<byte>) -> PemFields`                                 | static  | UTF-8 locate; throws on absent    |

- `PemEncoding.TryFind`: returns `false` at end-of-input, so a multi-element walk terminates on the rail.
- `PemFields`: `Location`, `Label`, and `Base64Data` index the ORIGINAL span; `DecodedDataLength` sizes the DER destination and `Location.End` advances the walk cursor.

[ENTRYPOINT_SCOPE]: certificate admission (`X509CertificateLoader`)

| [INDEX] | [SURFACE]                                                                                     | [SHAPE] | [CAPABILITY]             |
| :-----: | :-------------------------------------------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `LoadCertificate(ReadOnlySpan<byte>)`                                                         | static  | admit a DER certificate  |
|  [02]   | `LoadCertificateFromFile(string)`                                                             | static  | admit a DER file         |
|  [03]   | `LoadPkcs12(ReadOnlySpan<byte>, ReadOnlySpan<char>, X509KeyStorageFlags, Pkcs12LoaderLimits)` | static  | admit under parse bounds |

- `X509CertificateLoader.LoadPkcs12Collection`: same argument shape over a full chain, returning `X509Certificate2Collection`.

[ENTRYPOINT_SCOPE]: certificate PEM round-trip and key binding (`X509Certificate2`)

| [INDEX] | [SURFACE]                                                                            | [SHAPE]   | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------------------- | :-------- | :-------------------------------- |
|  [01]   | `CreateFromPem(ReadOnlySpan<char>) -> X509Certificate2`                              | static    | admit an armored certificate      |
|  [02]   | `CreateFromPem(ReadOnlySpan<char>, ReadOnlySpan<char>)`                              | static    | pair an armored private key       |
|  [03]   | `CreateFromEncryptedPem(ReadOnlySpan<char>, ReadOnlySpan<char>, ReadOnlySpan<char>)` | static    | admit a password-wrapped key      |
|  [04]   | `CreateFromPemFile(string, string?)`                                                 | static    | admit both legs from disk         |
|  [05]   | `CreateFromEncryptedPemFile(string, ReadOnlySpan<char>, string?)`                    | static    | admit an encrypted key file       |
|  [06]   | `ExportCertificatePem() -> string`                                                   | instance  | project the certificate as armor  |
|  [07]   | `TryExportCertificatePem(Span<char>, out int) -> bool`                               | instance  | project into a rented span        |
|  [08]   | `RawData -> byte[]`                                                                  | property  | certificate DER as a fresh array  |
|  [09]   | `RawDataMemory -> ReadOnlyMemory<byte>`                                              | property  | certificate DER without a copy    |
|  [10]   | `GetECDsaPrivateKey() -> ECDsa?`                                                     | extension | signing key; `null` when absent   |
|  [11]   | `GetECDsaPublicKey() -> ECDsa?`                                                      | extension | verifying key; `null` when absent |

[ENTRYPOINT_SCOPE]: AEAD seal (`AesGcm`)

| [INDEX] | [SURFACE]                                                                                             | [SHAPE]  | [CAPABILITY]     |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------- | :--------------- |
|  [01]   | `AesGcm(ReadOnlySpan<byte>, int)`                                                                     | ctor     | bind key and tag |
|  [02]   | `Encrypt(ReadOnlySpan<byte>, ReadOnlySpan<byte>, Span<byte>, Span<byte>, ReadOnlySpan<byte>)`         | instance | seal in place    |
|  [03]   | `Decrypt(ReadOnlySpan<byte>, ReadOnlySpan<byte>, ReadOnlySpan<byte>, Span<byte>, ReadOnlySpan<byte>)` | instance | open and verify  |
|  [04]   | `NonceByteSizes -> KeySizes`                                                                          | static   | legal nonce band |
|  [05]   | `TagByteSizes -> KeySizes`                                                                            | static   | legal tag band   |
|  [06]   | `TagSizeInBytes -> int?`                                                                              | property | bound tag size   |
|  [07]   | `IsSupported -> bool`                                                                                 | static   | platform probe   |

- `AesGcm.Encrypt`: nonce is 12 bytes, ciphertext length equals plaintext length, and the tag rides its own span.
- `AesGcm.Decrypt`: a forged tag raises `AuthenticationTagMismatchException`, converted once at the boundary.

[ENTRYPOINT_SCOPE]: entropy (`RandomNumberGenerator`)

| [INDEX] | [SURFACE]                   | [SHAPE] | [CAPABILITY]                      |
| :-----: | :-------------------------- | :------ | :-------------------------------- |
|  [01]   | `Fill(Span<byte>)`          | static  | fill a rented buffer with entropy |
|  [02]   | `GetBytes(int) -> byte[]`   | static  | draw a fresh entropy array        |
|  [03]   | `GetInt32(int, int) -> int` | static  | draw a bounded unbiased integer   |

[ENTRYPOINT_SCOPE]: signature (`ECDsa`)

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :---------------------------- |
|  [01]   | `GetMaxSignatureSize(DSASignatureFormat) -> int`                                            | instance | size a signature destination  |
|  [02]   | `SignData(ReadOnlySpan<byte>, HashAlgorithmName, DSASignatureFormat) -> byte[]`             | instance | hash and sign a preimage      |
|  [03]   | `SignData(ReadOnlySpan<byte>, Span<byte>, HashAlgorithmName, DSASignatureFormat) -> int`    | instance | sign into a rented span       |
|  [04]   | `VerifyData(ReadOnlySpan<byte>, ReadOnlySpan<byte>, HashAlgorithmName, DSASignatureFormat)` | instance | verify preimage and signature |

- `DSASignatureFormat.Rfc3279DerSequence` fixes the ASN.1 DER framing sign and verify share.

[ENTRYPOINT_SCOPE]: integrity primitives (`CryptographicOperations`)

| [INDEX] | [SURFACE]                                                                                     | [SHAPE] | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------------------------------- | :------ | :--------------------- |
|  [01]   | `ZeroMemory(Span<byte>)`                                                                      | static  | overwrite a buffer     |
|  [02]   | `FixedTimeEquals(ReadOnlySpan<byte>, ReadOnlySpan<byte>) -> bool`                             | static  | constant-time equality |
|  [03]   | `HashData(HashAlgorithmName, ReadOnlySpan<byte>, Span<byte>) -> int`                          | static  | digest into a span     |
|  [04]   | `HashDataAsync(HashAlgorithmName, Stream, Memory<byte>, CancellationToken) -> ValueTask<int>` | static  | async stream digest    |
|  [05]   | `TryHmacData(HashAlgorithmName, ReadOnlySpan<byte>, ReadOnlySpan<byte>, Span<byte>, out int)` | static  | MAC; `false` if short  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every allocating form has a caller-buffer twin whose destination the surface's own probe sizes — `GetEncodedSize`, `GetMaxSignatureSize`, `TagByteSizes` — so credential material occupies one rented span from armor through overwrite.
- RFC-7468 armor self-delimits: an `-----END-----`/`-----BEGIN-----` pair joined by `\n` bounds each element and `PemFields.Location.End` advances the walk, so a bundle needs no separator token.
- `AesGcm` seals and opens in place under caller-owned spans, so one instance per key serves every message the key covers and the plaintext never doubles.

[STACKING]:
- `api-hashing`(`.api/api-hashing.md`): each armored block's digest is `ContentHash.Of` over its DER, so a bundle carrier proves element identity while this surface contributes armor alone.
- `api-redaction`(`.api/api-redaction.md`): a secret-bearing block carries `DataClassification.Secret`, so the bound `Redactor` erases its bytes at every egress while the label and digest cross.
- `api-languageext`(`.api/api-languageext.md`): each `Try*` `bool` folds to `Fin.Succ`/`Fin.Fail` at the boundary, so an absent element, a short destination, or a failed parse lands as a typed fault row.
- `api-highperformance`(`.api/api-highperformance.md`): `MemoryOwner<T>.Allocate` rents the destination `GetEncodedSize` sizes, and `ZeroMemory` overwrites it before `Dispose` returns the rental to the pool.
- `Rasm.AppHost` credential lifecycle: `PemEncoding` and `X509Certificate2` own the at-rest and on-wire armor while the lease owner holds the live rented copy and drives its `ZeroMemory` terminal, so material never carries two encodings.
- `Rasm.Persistence` object store: the client-side seal binds one `AesGcm` per KMS-unwrapped DEK and derives its nonce from the content address, so a resumed multipart replays byte-identical ciphertext and the DEK zeroizes at the same terminal.
- `Rasm.Fabrication` attestation: `GetECDsaPrivateKey` signs the receipt preimage under `Rfc3279DerSequence`, and verification runs `CreateFromPem` with `GetECDsaPublicKey` over the exported certificate.

[LOCAL_ADMISSION]:
- Certificates enter through `X509CertificateLoader` or `CreateFromPem` and leave through `ExportCertificatePem` or `RawDataMemory`.
- `CryptographicOperations` MAC and digest members serve authenticity claims; identity and cache keys ride the `api-hashing` non-cryptographic digest.
- A secret buffer is rented, filled once, and overwritten through `ZeroMemory` at its owning lifecycle's terminal.

[RAIL_LAW]:
- Package: `System.Security.Cryptography`
- Owns: RFC-7468 armor, X.509 admission and export, AEAD sealing, ECDSA attestation, buffer zeroization
- Accept: span writes into rented destinations, size probes ahead of every span call, `Try*` results folded onto a typed rail
- Reject: a hand-built `-----BEGIN-----` string, a base64 credential envelope, a third-party PEM codec, a hand-rolled constant-time compare
