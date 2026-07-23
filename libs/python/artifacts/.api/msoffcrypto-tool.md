# [PY_ARTIFACTS_API_MSOFFCRYPTO_TOOL]

`msoffcrypto-tool` owns encrypted-Office confidentiality for the artifacts document and exchange rail: `OfficeFile(file)` sniffs a container to its format object, `load_key` derives the secret key from a password, private key, or secret key, and `decrypt` streams plaintext to a caller handle; `OOXMLFile.encrypt` reseals a plaintext OOXML payload under a fresh agile container. It composes the in-package ECMA-376 and RC4/XOR key schedules over `cryptography` and `olefile`, never re-deriving the crypto and never re-parsing the OOXML/OLE document tree the readers and persistence owners model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msoffcrypto-tool`
- package: `msoffcrypto-tool` (MIT)
- module: `msoffcrypto`
- namespaces: `msoffcrypto.format`, `msoffcrypto.exceptions`, `msoffcrypto.method`
- rail: confidentiality
- abi: pure-Python (no extension module)
- depends: `olefile` (OLE stream IO), `cryptography` (AES/SHA primitives)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: office-file roots and confidentiality failures

`BaseOfficeFile` is the ABC every format object satisfies; `InvalidKeyError` subclasses `DecryptionError`, so one `except DecryptionError` arm catches both a generic decrypt fault and a key-verification failure.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `format.base.BaseOfficeFile` | abstract base | `load_key`/`decrypt`/`is_encrypted` ABC contract         |
|  [02]   | `format.ooxml.OOXMLFile`     | class         | ECMA-376 agile/standard/plain; sole `encrypt` carrier    |
|  [03]   | `format.doc97.Doc97File`     | class         | legacy Word 97 RC4/RC4-CryptoAPI/XOR container           |
|  [04]   | `format.xls97.Xls97File`     | class         | legacy Excel 97 container                                |
|  [05]   | `format.ppt97.Ppt97File`     | class         | legacy PowerPoint 97 container                           |
|  [06]   | `exceptions.FileFormatError` | exception     | unrecognized or unsupported container (factory miss)     |
|  [07]   | `exceptions.ParseError`      | exception     | container cannot be parsed                               |
|  [08]   | `exceptions.DecryptionError` | exception     | decrypt fault; parent of `InvalidKeyError`               |
|  [09]   | `exceptions.EncryptionError` | exception     | reseal fault (already-encrypted source)                  |
|  [10]   | `exceptions.InvalidKeyError` | exception     | wrong or unverifiable key; subclass of `DecryptionError` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory dispatch

`OfficeFile` seeks to zero, sniffs OLE then ZIP, and returns the format object or raises `FileFormatError`. It leaves the handle OPEN and the position moved, so the caller owns lifetime and never assumes position zero after the call; the confidentiality probe is `is_encrypted()` on the returned object, and no module-level `is_encrypted` exists.

| [INDEX] | [SURFACE]                            | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :----------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `OfficeFile(file) -> BaseOfficeFile` | factory | sniff the container, raise `FileFormatError` on miss |

[ENTRYPOINT_SCOPE]: `OOXMLFile` key load, decrypt, encrypt, inspect

`keyTypes` advertises the admissible credential rows per container `type` — agile all three, standard password and secret_key, plain unset — so the caller reads the set rather than hardcoding it. OOXML verification is opt-in; `private_key` is agile-only and any other `type` raises `DecryptionError`; `decrypt` runs an always-on post-decrypt zip-validity gate that raises `InvalidKeyError` on a silently-wrong key even when `verify_integrity=False`.

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `load_key(password=None, private_key=None, secret_key=None, verify_password=False)` | instance | derive the key; opt-in verifier check   |
|  [02]   | `decrypt(outfile, verify_integrity=False)`                                          | instance | stream plaintext; opt-in HMAC, zip gate |
|  [03]   | `encrypt(password, outfile)`                                                        | instance | reseal under a fresh agile container    |
|  [04]   | `is_encrypted() -> bool`                                                            | instance | false for `plain`, true when OLE-backed |
|  [05]   | `keyTypes -> tuple[str, ...]`                                                       | property | advertised credential rows per `type`   |
|  [06]   | `type -> str`                                                                       | property | `agile`/`standard`/`plain` discriminant |
|  [07]   | `format -> str`                                                                     | property | `"ooxml"`; `getattr` default `""` on 97 |

[ENTRYPOINT_SCOPE]: legacy 97 format key load, decrypt, inspect

`Doc97File`, `Xls97File`, and `Ppt97File` accept only a `password`, stream plaintext with no integrity option, and share the identical call shape. Each `load_key` ALWAYS verifies as part of derivation — a wrong key raises `InvalidKeyError` with no opt-in flag — and resolves the RC4/RC4-CryptoAPI/XOR scheme from the header, never a caller-selected one.

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------ | :------- | :------------------------------------- |
|  [01]   | `{Doc97,Xls97,Ppt97}File.load_key(password=None)` | instance | derive and always-verify the 97 key    |
|  [02]   | `{Doc97,Xls97,Ppt97}File.decrypt(outfile)`        | instance | stream plaintext (no integrity option) |
|  [03]   | `BaseOfficeFile.is_encrypted() -> bool`           | instance | read the on-stream encryption flag     |

[ENTRYPOINT_SCOPE]: in-package crypto method tier

`method/` owns the key schedule the format objects call internally; a request to re-derive AES/SHA against `cryptography` directly is rejected because these own it, and `OOXMLFile.encrypt` drives the agile re-seal schedule.

| [INDEX] | [SURFACE]                              | [SHAPE] | [CAPABILITY]                                                               |
| :-----: | :------------------------------------- | :------ | :------------------------------------------------------------------------- |
|  [01]   | `ecma376_agile.ECMA376Agile`           | static  | `makekey_from_{password,privkey}`, `verify_password`, `decrypt`, `encrypt` |
|  [02]   | `ecma376_standard.ECMA376Standard`     | static  | `makekey_from_password`, `verifykey`, `decrypt`                            |
|  [03]   | `ecma376_extensible.ECMA376Extensible` | static  | extensible placeholder; `DecryptionError` at decrypt                       |
|  [04]   | `rc4.DocumentRC4`                      | static  | legacy RC4 `verifypw` + stream decrypt                                     |
|  [05]   | `rc4_cryptoapi.DocumentRC4CryptoAPI`   | static  | RC4-CryptoAPI verify + decrypt                                             |
|  [06]   | `xor_obfuscation.DocumentXOR`          | static  | XOR obfuscation for `fObfuscation==1` DOC containers                       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Package root exports only `OfficeFile` and `exceptions`; the concrete `OOXMLFile`/97 classes are factory-returned, never caller-selected constructors. Import at boundary scope only (`lazy import msoffcrypto` in `folder:document/egress#EGRESS`); module-level import is banned by the manifest import policy.
- `OfficeFile(file)` is the single entry; container kind is a stream-layout discriminant resolved by the factory, never a caller-selected format flag or a per-extension reader.
- `load_key` owns key derivation; `password`/`private_key`/`secret_key` are call rows, never a per-credential method family, and `keyTypes` advertises which rows the container `type` admits.
- OOXML folds verification into the call opt-in (`verify_password` on `load_key`, `verify_integrity` on `decrypt`) while the 97 path always verifies inside `load_key`; `decrypt` writes to the caller's handle under an always-on post-decrypt zip-validity gate, never an in-package buffer.
- `OOXMLFile.encrypt` is the inverse rail (fresh agile container only), so an encrypt on a legacy object is an `AttributeError` at the type edge. Both directions are idempotent at the `is_encrypted()` gate: `encrypt` on a sealed file raises `EncryptionError`, so the consumer short-circuits an already-sealed reseal and an already-plaintext unlock.
- Each unlock/reseal captures container `format`/`type`, advertised `keyTypes`, key-input kind, verification outcome, and byte length as confidentiality evidence; `InvalidKeyError` is the wrong-key fault, `FileFormatError` the unrecognized-container fault, `EncryptionError` the reseal fault.

[STACKING]:
- `openpyxl`(`.api/openpyxl.md`)/`python-docx`(`.api/python-docx.md`)/`python-pptx`(`.api/python-pptx.md`): the decrypted plaintext stream is their document-open input; this owner unlocks and never re-parses the OOXML/OLE tree they model.
- `py7zr`(`.api/py7zr.md`)/`stream-zip`(`.api/stream-zip.md`): the plaintext or resealed bytes flow to the persistence archive owners.
- within-lib detect: `folder:exchange/detect#DETECT` routes the `MediaClass.ENCRYPTED`/`OFFICE_LEGACY` verdict here, so this owner reads one routing member and never re-sniffs the bytes.
- within-lib egress: `folder:document/egress#EGRESS` folds the format-discriminated unlock/reseal into its `PROTECT` `EgressStep`; an `expression` Result carries `RuntimeRail[ContentKey]`, the `async_boundary` capsule maps a `DecryptionError`/`FileFormatError` to a `BoundaryFault`, and the `anyio` `lane.offload` seam runs the GIL-releasing decrypt off the event loop.
- within-lib capsule: `xxhash`-backed `ContentIdentity.of` mints the content key over the decrypted/resealed bytes, `beartype` validates the boundary over a per-finish `BytesIO(egress.source)` handle, and `structlog` excludes key material from every event.
- receipt: the result rides the `msgspec.Struct` `FinishFact` the egress fold threads through `structs.replace`, contributing one kind-discriminated `ArtifactReceipt` case via the runtime `ReceiptContributor` port; the `Confidentiality` `@tagged_union` (`unlock`/`reseal`) is the closed disposition the `PROTECT` arm matches on.

[LOCAL_ADMISSION]:
- Unlock reads `OfficeFile(file)` -> `load_key(...)` -> `decrypt(outfile)` over a `BytesIO` source and sink; reseal reads `OOXMLFile.encrypt(password, outfile)`, both gated on `is_encrypted()`.

[RAIL_LAW]:
- Package: `msoffcrypto-tool`
- Owns: encrypted-Office detection (OOXML agile/standard, legacy DOC/XLS/PPT 97 RC4/RC4-CryptoAPI/XOR), password/private-key/secret-key derivation with OOXML-optional or 97-mandatory verification, decrypted-stream output, and OOXML re-encryption under a fresh agile container
- Accept: confidentiality unlock and OOXML reseal feeding the document readers and persistence owners; an in-memory `BufferedReader`/`BytesIO` source the factory dispatches over; credentials as the `keyTypes`-advertised rows
- Reject: wrapper-renames of `OfficeFile`/`load_key`/`decrypt`/`encrypt`/`is_encrypted`; a hand-rolled ECMA-376 or RC4/XOR schedule duplicating the `method/` tier; a caller-selected format flag where stream sniffing dispatches; a per-extension reader; a `load_key` family split per credential; a `verify_*` flag forced on the 97 surface; a `reseal` on a non-OOXML object; a flat catch ignoring `InvalidKeyError`'s `DecryptionError` parentage; re-parsing the decrypted document tree the reader owners model
