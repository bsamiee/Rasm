# [PY_ARTIFACTS_API_MSOFFCRYPTO_TOOL]

`msoffcrypto-tool` is the encrypted-Office confidentiality owner for the artifacts exchange/document rail: `OfficeFile(file)` sniffs an encrypted Office container and returns the matching format object (`OOXMLFile`, `Doc97File`, `Xls97File`, `Ppt97File`), which `load_key` unlocks from a password / private key / secret key and `decrypt` streams as plaintext to an output file; `OOXMLFile.encrypt(password, outfile)` is the inverse rail that re-seals a plaintext OOXML payload under a fresh agile container. `BaseOfficeFile.is_encrypted()` on the factory-returned object is the confidentiality probe the consumer reads before dispatch (the package root exports only `OfficeFile` + `exceptions`; there is no module-level `is_encrypted` library surface — that name lives in `__main__` as a CLI-internal helper). The two consumers compose it as ONE confidentiality edge: `folder:exchange/detect#DETECT` routes the `ENCRYPTED`/`OFFICE_LEGACY` `MediaClass` to this owner, and `folder:document/egress#EGRESS` folds it into the `PROTECT` finishing step — the format-discriminated `load_key`/`decrypt` unlock OR the `OOXMLFile.encrypt` reseal, both idempotent at the `is_encrypted()` gate. The owner removes any hand-rolled ECMA-376 agile/standard/extensible and legacy RC4/XOR crypto because the `method/` tier (`ECMA376Agile`/`ECMA376Standard`, `DocumentRC4`/`DocumentRC4CryptoAPI`, `DocumentXOR`) owns the full key schedule over `cryptography`/`olefile`; it never re-derives the AES/SHA schedule or re-parses the OOXML/OLE document tree the readers (`python-docx`/`openpyxl`/`python-pptx`) and persistence owners (`py7zr`/`stream-zip`) already model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msoffcrypto-tool`
- package: `msoffcrypto-tool`
- import: `msoffcrypto`
- owner: `artifacts`
- rail: confidentiality
- consumers: `folder:exchange/detect#DETECT` (the `MediaClass.ENCRYPTED`/`MediaClass.OFFICE_LEGACY` routing target — never imported there, only routed to), `folder:document/egress#EGRESS` (the `PROTECT` `EgressStep` arm — `lazy import msoffcrypto` at boundary scope, folded onto `anyio.to_thread` under `CapacityLimiter(8)`)
- installed: `6.0.0`
- license: MIT (permissive; no copyleft obligation)
- abi: pure-Python (no extension module); cp315-clean, ungated in the manifest
- deps: `olefile` (OLE container parse/stream IO) + `cryptography` (AES/SHA primitives backing the key schedule) are the only runtime deps; the ECMA-376 agile/standard/extensible and RC4/XOR derivation live in-package under `method/`
- entry points: console script `msoffcrypto-tool` (`msoffcrypto.__main__:main`); library use is import-only and the package root exports exactly `OfficeFile` + `exceptions` (the concrete format classes and the `method/` tier are reached by submodule import, not re-exported)
- capability: encrypted-Office format detection (OOXML agile/standard, legacy DOC/XLS/PPT 97 RC4/RC4-CryptoAPI/XOR), password / private-key / secret-key derivation with optional verification, decrypted-stream output to a writable file, and OOXML re-encryption (`OOXMLFile.encrypt`) under a fresh agile container

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: office-file roots and confidentiality failures
- rail: confidentiality

`OfficeFile` is the format-dispatch factory that returns a `BaseOfficeFile` subclass keyed by the container's stream layout: `OOXMLFile` for ECMA-376 OOXML (and the plain/unencrypted OOXML zip), `Doc97File`/`Xls97File`/`Ppt97File` for the OLE 97 formats. The `exceptions` namespace is a two-tier rail: `FileFormatError`/`ParseError`/`DecryptionError`/`EncryptionError` derive directly from `Exception`, and `InvalidKeyError` subclasses `DecryptionError` (a wrong/unverifiable key is a decryption fault). A `try/except DecryptionError` therefore catches both a generic decrypt failure and the key-verification failure in one arm — the consumer never catches `InvalidKeyError` separately unless it must distinguish a bad password from a malformed payload.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]    | [RAIL]                                                        |
| :-----: | :--------------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `OfficeFile`                 | factory function | `OfficeFile(file)` stream-layout dispatch to a `BaseOfficeFile` |
|  [02]   | `format.base.BaseOfficeFile` | abstract base    | `load_key`/`decrypt`/`is_encrypted` ABC contract             |
|  [03]   | `format.ooxml.OOXMLFile`     | format object    | ECMA-376 OOXML agile/standard container (also `plain` for an unencrypted OOXML zip); the only subtype carrying `encrypt`/`keyTypes`/`type` |
|  [04]   | `format.doc97.Doc97File`     | format object    | legacy MS-DOC (Word 97) RC4 / RC4-CryptoAPI / XOR container   |
|  [05]   | `format.xls97.Xls97File`     | format object    | legacy MS-XLS (Excel 97) container                           |
|  [06]   | `format.ppt97.Ppt97File`     | format object    | legacy MS-PPT (PowerPoint 97) container                      |
|  [07]   | `exceptions.FileFormatError` | error root       | unsupported or unrecognized container format (factory sniff miss) |
|  [08]   | `exceptions.ParseError`      | error root       | container cannot be parsed                                   |
|  [09]   | `exceptions.DecryptionError` | error root       | container cannot be decrypted; parent of `InvalidKeyError`   |
|  [10]   | `exceptions.EncryptionError` | error root       | container cannot be encrypted (e.g. re-seal of an already-encrypted file) |
|  [11]   | `exceptions.InvalidKeyError` | error            | wrong or unverifiable password/key (subclass of `DecryptionError`) |

[PUBLIC_TYPE_SCOPE]: read discriminants the factory sets
- rail: confidentiality

`OfficeFile` sets instance attributes the consumer reads to drive the credential and decrypt axes WITHOUT branching on the concrete class. The egress consumer reads them through `getattr` with the legacy-97 defaults, so the same arm services every format object: `getattr(office, "format", "")` and `getattr(office, "keyTypes", ("password",))`.

| [INDEX] | [ATTRIBUTE]             | [SET_ON]                          | [VALUE]                                                            |
| :-----: | :---------------------- | :-------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `OOXMLFile.format`      | `OOXMLFile` only (`"ooxml"`)      | the family discriminant; absent on the 97 objects, so `getattr(office, "format", "")` distinguishes OOXML from legacy in one read |
|  [02]   | `OOXMLFile.type`        | `OOXMLFile` only                  | the encryption-method discriminant `agile`/`standard`/`plain` (the `extensible` version raises `DecryptionError` at sniff and never yields a usable object) |
|  [03]   | `OOXMLFile.keyTypes`    | `OOXMLFile` agile/standard only   | `('password','private_key','secret_key')` for `agile`, `('password','secret_key')` for `standard`; UNSET on `plain` (and on the unreachable `extensible`), so the consumer falls back to `("password",)` via `getattr` |
|  [04]   | `BaseOfficeFile.is_encrypted` | every format object         | the `bool` confidentiality gate every consumer reads before unlock/reseal |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory dispatch
- rail: confidentiality

`OfficeFile` seeks the file to zero, sniffs OLE versus ZIP (`olefile.isOleFile` then `zipfile.is_zipfile`), inspects the `EncryptionInfo`/`wordDocument`/`Workbook`/`PowerPoint Document` stream, and returns the format object; it raises `FileFormatError` when no admissible container is recognized. The handle stays OPEN and the file position changes — the consumer owns handle lifetime and must not assume position zero after the call. The confidentiality probe is `OfficeFile(file).is_encrypted()` on the returned object (the egress consumer constructs once over `BytesIO(egress.source)` then gates on `is_encrypted()`); the package root exposes no standalone `is_encrypted` function.

| [INDEX] | [SURFACE]        | [CALL_SHAPE]                           | [CAPABILITY]                                                      |
| :-----: | :--------------- | :------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `OfficeFile`     | `OfficeFile(file)` -> `BaseOfficeFile` | sniff the container, return the format object, raise `FileFormatError` on miss |

[ENTRYPOINT_SCOPE]: `OOXMLFile` key load, decrypt, encrypt, inspect
- rail: confidentiality

`load_key` derives the secret key from a `password`, `private_key`, or `secret_key`; `verify_password=True` raises `InvalidKeyError` when the key fails the verifier-hash check (OOXML verification is OPT-IN, unlike the always-verifying 97 path). `decrypt` streams the plaintext payload to `outfile`; `verify_integrity=True` validates the HMAC before writing, and the arm independently raises `InvalidKeyError("The file could not be decrypted with this password")` when the decrypted bytes are not a valid zip — a post-decrypt validity gate that catches a silently-wrong key even with `verify_integrity=False`. `encrypt(password, outfile)` is the inverse: it re-seals a plaintext OOXML stream under a NEW agile-encryption container (always agile, never standard/extensible) and raises `EncryptionError("File is already encrypted")` when the source is already sealed.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                        | [CAPABILITY]                                          |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `OOXMLFile.load_key`     | `load_key(password=None, private_key=None, secret_key=None, verify_password=False)` | derive the secret key (password/private-key/secret-key, optional verify); `private_key` is agile-only, others raise `DecryptionError` |
|  [02]   | `OOXMLFile.decrypt`      | `decrypt(outfile, verify_integrity=False)`                                          | stream plaintext to `outfile`; optional HMAC check + always a post-decrypt zip-validity gate |
|  [03]   | `OOXMLFile.encrypt`      | `encrypt(password, outfile)`                                                        | re-seal plaintext OOXML under a fresh AGILE container; `EncryptionError` if already encrypted |
|  [04]   | `OOXMLFile.is_encrypted` | `is_encrypted()` -> `bool`                                                          | `type=="plain"` -> `False`, else an OLE-backed handle -> `True` |
|  [05]   | `OOXMLFile.keyTypes`     | instance attribute -> `tuple[str, ...]`                                             | admissible key inputs per `type`; `('password','private_key','secret_key')` agile, `('password','secret_key')` standard, UNSET on `plain` |
|  [06]   | `OOXMLFile.type`         | instance attribute -> `str`                                                         | encryption discriminant `agile`/`standard`/`plain` |
|  [07]   | `OOXMLFile.format`       | instance attribute -> `str` (`"ooxml"`)                                             | family discriminant the consumer reads to distinguish OOXML from legacy |

[ENTRYPOINT_SCOPE]: legacy 97 format key load, decrypt, inspect
- rail: confidentiality

The OLE 97 formats accept only a `password` and stream plaintext with no integrity option. `load_key` ALWAYS verifies the password as part of derivation (it raises `InvalidKeyError("Failed to verify password")` on a wrong key with no opt-in flag — the structural asymmetry with the OOXML opt-in `verify_password`) and resolves an internal method discriminant (`rc4` / `rc4_cryptoapi` / `xor`) from the FIB/header, never a caller-selected scheme. `is_encrypted()` reads the on-stream encryption flag (`fEncrypted` for DOC, the analogous BIFF/PPT flag). `Doc97File`, `Xls97File`, and `Ppt97File` share the identical public call shape.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]               | [CAPABILITY]                                                  |
| :-----: | :---------------------------- | :------------------------- | :----------------------------------------------------------- |
|  [01]   | `Doc97File.load_key`          | `load_key(password=None)`  | derive + ALWAYS verify the legacy MS-DOC key (RC4/RC4-CryptoAPI/XOR) from password |
|  [02]   | `Doc97File.decrypt`           | `decrypt(outfile)`         | stream plaintext to `outfile` (no integrity option)          |
|  [03]   | `Xls97File.load_key`          | `load_key(password=None)`  | derive + verify the legacy MS-XLS key from password          |
|  [04]   | `Xls97File.decrypt`           | `decrypt(outfile)`         | stream plaintext to `outfile`                                |
|  [05]   | `Ppt97File.load_key`          | `load_key(password=None)`  | derive + verify the legacy MS-PPT key from password          |
|  [06]   | `Ppt97File.decrypt`           | `decrypt(outfile)`         | stream plaintext to `outfile`                                |
|  [07]   | `BaseOfficeFile.is_encrypted` | `is_encrypted()` -> `bool` | report whether the container is encrypted (97 reads the on-stream flag) |

[ENTRYPOINT_SCOPE]: in-package crypto method tier (the schedule the owner never re-rolls)
- rail: confidentiality

The `method/` tier is the proof that the owner composes the in-package key schedule rather than hand-rolling one over `cryptography`. The format objects call these static methods internally; the consumer never touches them, but a catalog that claims "the schedule is in-package" must name the surface that owns it. Naming them also defends the boundary: a request to re-derive AES/SHA against `cryptography.hazmat` directly is rejected because `ECMA376Agile`/`ECMA376Standard` already own it.

| [INDEX] | [SURFACE]                            | [OWNS]                                                                 |
| :-----: | :----------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `method.ecma376_agile.ECMA376Agile`  | agile `makekey_from_password`/`makekey_from_privkey`, `verify_password`, `verify_integrity`, `decrypt`, and `encrypt` (the agile re-seal schedule `OOXMLFile.encrypt` drives) |
|  [02]   | `method.ecma376_standard.ECMA376Standard` | standard `makekey_from_password`, `verifykey`, `decrypt`         |
|  [03]   | `method.ecma376_extensible`          | the extensible-encryption placeholder (sniffed but `DecryptionError` at parse — unsupported, never reached as a usable object) |
|  [04]   | `method.rc4.DocumentRC4`             | legacy RC4 `verifypw` + stream decrypt (Office binary RC4)           |
|  [05]   | `method.rc4_cryptoapi.DocumentRC4CryptoAPI` | legacy RC4-CryptoAPI verify + decrypt                         |
|  [06]   | `method.xor_obfuscation`             | the XOR-obfuscation method for `fObfuscation==1` DOC containers       |

## [04]-[IMPLEMENTATION_LAW]

[CONFIDENTIALITY_OFFICE]:
- import: `import msoffcrypto` at boundary scope only (`lazy import msoffcrypto` in `folder:document/egress#EGRESS`); module-level import is banned by the manifest import policy. `OfficeFile` and `exceptions` are reached from the package root; the concrete `OOXMLFile`/97 classes are an implementation detail the factory returns, never a per-extension constructor the consumer selects.
- dispatch axis: `OfficeFile(file)` is the single entry; container kind (OOXML agile/standard/plain, DOC/XLS/PPT 97) is a stream-layout discriminant resolved by the factory, never a caller-selected format flag or a parallel reader per extension. The two consumers route to it polymorphically: `folder:exchange/detect#DETECT` resolves a `MediaClass` (`ENCRYPTED`/`OFFICE_LEGACY` -> this owner) so the dispatch is a closed-vocabulary routing value, and `folder:document/egress#EGRESS` reads `getattr(office, "format", "")`/`getattr(office, "keyTypes", ("password",))` so ONE `PROTECT` arm services every format object.
- key axis: `load_key` owns key derivation; `password`/`private_key`/`secret_key` are call rows on the OOXML surface and `password` is the sole row on the 97 surface — never a per-credential method family. `keyTypes` is read from the format object after sniff to advertise which credential rows the container's `type` admits (`agile` all three; `standard` password/secret-key; `plain`/97 fall back to `("password",)` via `getattr`), so the caller never hardcodes the credential set and the egress consumer builds `{kind: creds[kind] for kind in key_axis if kind in creds}` to pass only admissible inputs.
- verify axis: OOXML folds verification into the call OPT-IN — `verify_password=True` on `load_key` raises `InvalidKeyError` on a bad verifier hash, `verify_integrity=True` on `decrypt` validates the HMAC; the 97 path ALWAYS verifies inside `load_key` (raising `InvalidKeyError("Failed to verify password")` with no flag). The egress consumer expresses this asymmetry as `**({"verify_password": True} if ooxml and verify else {})` / `**({"verify_integrity": verify} if ooxml else {})`, never a uniform flag the 97 surface would reject.
- decrypt axis: `decrypt(outfile, ...)` is the single plaintext-output surface keyed by the format object; OOXML adds `verify_integrity` as an HMAC row plus a non-suppressible post-decrypt zip-validity gate (`InvalidKeyError` when the result is not a valid zip), the 97 formats take none — the decrypted stream writes to the caller's file handle (a `BytesIO` sink in egress), never an in-package buffer type.
- encrypt axis: `OOXMLFile.encrypt(password, outfile)` is the inverse confidentiality rail (re-seal plaintext under a fresh AGILE container — never standard/extensible); it exists only on the OOXML object, so an encrypt request on a legacy container is an `AttributeError` at the type edge (the egress `reseal` arm is structurally guarded by `OfficeFile` returning an `OOXMLFile` for an OOXML payload), not a string flag. Both directions are idempotent at the `is_encrypted()` gate: `encrypt` on an already-sealed file raises `EncryptionError`, so the consumer short-circuits an already-sealed reseal and an already-plaintext unlock to the source bytes unchanged.
- evidence: each unlock/reseal captures container `format`/`type`, the advertised `keyTypes`, key-input kind, verification outcome, and decrypted/resealed byte length as the confidentiality slot of the egress `ArtifactReceipt` case; `InvalidKeyError` (subclass of `DecryptionError`) is the typed failure for a wrong or unverifiable key, `FileFormatError` for an unrecognized container, `EncryptionError` for a failed re-seal.
- boundary: msoffcrypto owns Office container detection, key derivation, decryption, and OOXML re-encryption with the in-package ECMA-376 (over `cryptography`) and RC4/XOR methods under `method/`; the plaintext stream feeds the document readers (`python-docx`/`openpyxl`/`python-pptx`) and persistence owners (`py7zr`/`stream-zip`) downstream; `olefile` owns OLE stream IO, `cryptography` the AES/SHA primitives; key material stays inside the boundary call and is never logged.

[UNIVERSAL_RAIL_STACK]:
- detect seam: `folder:exchange/detect#DETECT` is the upstream gate — its `DetectIdentity` resolves a `MediaClass` over the sniffed MIME, and `ENCRYPTED`/`OFFICE_LEGACY` route to this owner. The catalog stacks ONTO that closed vocabulary: the consumer never re-sniffs the bytes (`Detect` already produced the `Trust` verdict and `MediaClass`), it reads the one routing member and constructs `OfficeFile` once. `application/encrypted` -> `ENCRYPTED`, `application/x-ole-storage`/`application/CDFV2`/`application/msword`/`application/vnd.ms-excel`/`application/vnd.ms-powerpoint` -> `OFFICE_LEGACY` are the rows that reach this owner.
- egress seam + rails: `folder:document/egress#EGRESS` folds this owner into the `PROTECT` `EgressStep` over the universal rails — the `expression`/`RuntimeRail` Result rail carries the `RuntimeRail[ContentKey]` every finish returns, the `async_boundary` capsule converts a `msoffcrypto.exceptions.DecryptionError`/`FileFormatError` raise into the runtime `BoundaryFault` (never a bare raise into domain flow), the `anyio.to_thread.run_sync` seam under `CapacityLimiter(8)` runs the GIL-releasing decrypt off the event loop, and `xxhash`-backed `ContentIdentity.of` mints the content key over the decrypted/resealed bytes for the `ArtifactReceipt.Office` case. `structlog` is the only logging rail and key material is excluded from every event — the `password`/`secret_key` never enter a log record.
- struct/receipt rail: the confidentiality result rides the `msgspec.Struct` `FinishFact` (bytes + evidence) the egress fold threads through `structs.replace`, contributing the single kind-discriminated `ArtifactReceipt` case through the runtime `ReceiptContributor` port — never a parallel confidentiality receipt shape. The `Confidentiality` `@tagged_union` (`unlock`/`reseal`) is the closed disposition the `PROTECT` arm matches on, so a new disposition is one case, not a new method.
- boundary capsule: `beartype` validates the boundary signature at the egress capsule; the format object's open handle is owned by the egress `BytesIO(egress.source)` lifetime (a fresh in-memory stream per finish), so no file-descriptor leak crosses the seam and the `anyio` worker never shares a mutable handle.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `msoffcrypto-tool`
- Owns: encrypted-Office format detection (OOXML agile/standard, legacy DOC/XLS/PPT 97 RC4/RC4-CryptoAPI/XOR), password / private-key / secret-key derivation with optional (OOXML) or mandatory (97) verification, decrypted-stream output for OOXML and legacy 97 containers, and OOXML re-encryption under a fresh agile container
- Accept: confidentiality unlock (and OOXML re-seal) of an Office document feeding the document readers and persistence owners; an in-memory `BufferedReader`/`BytesIO` source over which the factory dispatches; credentials passed as the `keyTypes`-advertised key rows
- Reject: wrapper-renames of `OfficeFile`/`load_key`/`decrypt`/`encrypt`/`is_encrypted`; a hand-rolled ECMA-376 or RC4/XOR key schedule duplicating the `method/`-tier `cryptography`-backed methods (`ECMA376Agile`/`ECMA376Standard`/`DocumentRC4`/`DocumentRC4CryptoAPI`); a caller-selected format flag where stream sniffing already dispatches; a parallel reader type per file extension; a `load_key` method family split per credential kind; a uniform `verify_*` flag forced onto the 97 surface that does not accept one; a `reseal` arm targeting a non-OOXML container (an `AttributeError` the type edge already forbids); a flat exception catch that ignores `InvalidKeyError`'s `DecryptionError` parentage; re-parsing the decrypted OOXML/OLE document tree the reader owners model
