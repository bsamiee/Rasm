# [PY_ARTIFACTS_API_MSOFFCRYPTO_TOOL]

`msoffcrypto-tool` supplies the office-document confidentiality surface for the artifacts rail: the `OfficeFile` factory sniffs an encrypted Office container and returns the matching format object (`OOXMLFile`, `Doc97File`, `Xls97File`, `Ppt97File`), which `load_key` unlocks from a password, private key, or secret key and `decrypt` streams as plaintext to an output file. The package owner composes `OfficeFile`, `load_key`, and `decrypt` into the `OFFICE_CONFIDENTIALITY` path; it removes any hand-rolled ECMA-376 agile/standard/extensible and legacy RC4/XOR crypto because the format dispatch and key derivation are in-package, and it never re-derives the AES/SHA key schedule msoffcrypto already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msoffcrypto-tool`
- package: `msoffcrypto-tool`
- import: `msoffcrypto`
- owner: `artifacts`
- rail: confidentiality
- installed: `6.0.0` reflected via `assay api` on cp315
- entry points: console script `msoffcrypto-tool` (`msoffcrypto.__main__:main`); library use is import-only
- capability: encrypted-Office format detection (OOXML agile/standard/extensible, legacy DOC/XLS/PPT 97), password/private-key/secret-key derivation with optional verification, and decrypted-stream output to a writable file

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: office-file roots and confidentiality failures
- rail: confidentiality

`OfficeFile` is the format-dispatch factory that returns a `BaseOfficeFile` subclass keyed by the container's stream layout: `OOXMLFile` for ECMA-376 OOXML, `Doc97File`/`Xls97File`/`Ppt97File` for the OLE 97 formats. The `exceptions` namespace raises `FileFormatError` on an unsupported container, `InvalidKeyError` on a wrong or unverifiable key, and `DecryptionError`/`ParseError`/`EncryptionError` on the corresponding crypto faults.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                             |
| :-----: | :--------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `OfficeFile`                 | factory       | format-sniffing dispatch to a `BaseOfficeFile`     |
|  [02]   | `format.base.BaseOfficeFile` | abstract base | `load_key`/`decrypt`/`is_encrypted` contract       |
|  [03]   | `format.ooxml.OOXMLFile`     | format object | ECMA-376 OOXML agile/standard/extensible container |
|  [04]   | `format.doc97.Doc97File`     | format object | legacy MS-DOC (Word 97) container                  |
|  [05]   | `format.xls97.Xls97File`     | format object | legacy MS-XLS (Excel 97) container                 |
|  [06]   | `format.ppt97.Ppt97File`     | format object | legacy MS-PPT (PowerPoint 97) container            |
|  [07]   | `exceptions.FileFormatError` | error         | unsupported or unrecognized container format       |
|  [08]   | `exceptions.InvalidKeyError` | error         | wrong or unverifiable password/key                 |
|  [09]   | `exceptions.DecryptionError` | error         | container cannot be decrypted                      |
|  [10]   | `exceptions.ParseError`      | error         | container cannot be parsed                         |
|  [11]   | `exceptions.EncryptionError` | error         | container cannot be encrypted                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory dispatch
- rail: confidentiality

`OfficeFile` seeks the file to zero, sniffs OLE versus ZIP, inspects the `EncryptionInfo`/`wordDocument`/`Workbook`/`PowerPoint Document` stream, and returns the format object; it raises `FileFormatError` when no admissible container is recognized. The handle stays open and the file position changes.

| [INDEX] | [SURFACE]    | [CALL_SHAPE]                           | [CAPABILITY]                                     |
| :-----: | :----------- | :------------------------------------- | :----------------------------------------------- |
|  [01]   | `OfficeFile` | `OfficeFile(file)` -> `BaseOfficeFile` | sniff the container and return the format object |

[ENTRYPOINT_SCOPE]: `OOXMLFile` key load, decrypt, inspect
- rail: confidentiality

`load_key` derives the secret key from a `password`, `private_key`, or `secret_key`; `verify_password=True` raises `InvalidKeyError` when the key fails the data-integrity check. `decrypt` streams the plaintext payload to `outfile`; `verify_integrity=True` validates the HMAC before writing. `keyTypes` is the read tuple advertising which key inputs the container admits.

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                                        | [CAPABILITY]                                          |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------- | :---------------------------------------------------- |
|  [01]   | `OOXMLFile.load_key`     | `load_key(password=None, private_key=None, secret_key=None, verify_password=False)` | derive the secret key (password/key, optional verify) |
|  [02]   | `OOXMLFile.decrypt`      | `decrypt(outfile, verify_integrity=False)`                                          | stream plaintext to `outfile` (optional HMAC check)   |
|  [03]   | `OOXMLFile.is_encrypted` | `is_encrypted()` -> `bool`                                                          | report whether the container is encrypted             |
|  [04]   | `OOXMLFile.keyTypes`     | attribute -> `tuple[str, ...]`                                                      | admissible key inputs for the container type          |

[ENTRYPOINT_SCOPE]: legacy 97 format key load, decrypt, inspect
- rail: confidentiality

The OLE 97 formats accept only a `password` and stream plaintext with no integrity option. `Doc97File`, `Xls97File`, and `Ppt97File` share the identical call shape.

| [INDEX] | [SURFACE]                     | [CALL_SHAPE]               | [CAPABILITY]                               |
| :-----: | :---------------------------- | :------------------------- | :----------------------------------------- |
|  [01]   | `Doc97File.load_key`          | `load_key(password=None)`  | derive the legacy MS-DOC key from password |
|  [02]   | `Doc97File.decrypt`           | `decrypt(outfile)`         | stream plaintext to `outfile`              |
|  [03]   | `Xls97File.load_key`          | `load_key(password=None)`  | derive the legacy MS-XLS key from password |
|  [04]   | `Xls97File.decrypt`           | `decrypt(outfile)`         | stream plaintext to `outfile`              |
|  [05]   | `Ppt97File.load_key`          | `load_key(password=None)`  | derive the legacy MS-PPT key from password |
|  [06]   | `Ppt97File.decrypt`           | `decrypt(outfile)`         | stream plaintext to `outfile`              |
|  [07]   | `BaseOfficeFile.is_encrypted` | `is_encrypted()` -> `bool` | report whether the container is encrypted  |

## [04]-[IMPLEMENTATION_LAW]

[CONFIDENTIALITY_OFFICE]:
- import: `import msoffcrypto` at boundary scope only; module-level import is banned by the manifest import policy.
- dispatch axis: `OfficeFile(file)` is the single entry; container kind (OOXML agile/standard/extensible, DOC/XLS/PPT 97) is a stream-layout discriminant resolved by the factory, never a caller-selected format flag or a parallel reader per extension.
- key axis: `load_key` owns key derivation; `password`, `private_key`, and `secret_key` are call rows on the OOXML surface and `password` is the sole row on the 97 surface — never a per-credential method family; `verify_password=True` folds key verification into load.
- decrypt axis: `decrypt(outfile, ...)` is the single plaintext-output surface keyed by the format object; OOXML adds `verify_integrity` as an HMAC row, the 97 formats take none — the decrypted stream writes to the caller's file handle, never an in-package buffer type.
- evidence: each unlock captures container format (`format`/`type`), `keyTypes`, key-input kind, verification outcome, and decrypted byte length as a confidentiality receipt; `InvalidKeyError` is the typed failure for a wrong or unverifiable key, `FileFormatError` for an unrecognized container.
- boundary: msoffcrypto owns Office container detection, key derivation, and decryption with the in-package ECMA-376 and RC4/XOR methods; the plaintext stream feeds the document and persistence owners downstream; key material stays inside the boundary call and is never logged.

[RAIL_LAW]:
- Package: `msoffcrypto-tool`
- Owns: encrypted-Office format detection, password/private-key/secret-key derivation with optional verification, and decrypted-stream output for OOXML and legacy 97 containers
- Accept: confidentiality unlock of an encrypted Office document feeding the document and persistence owners
- Reject: wrapper-renames of `OfficeFile`/`load_key`/`decrypt`; a hand-rolled ECMA-376 or RC4/XOR key schedule; a caller-selected format flag where stream sniffing already dispatches; a parallel reader type per file extension; a `load_key` method family split per credential kind
