# [PY_ARTIFACTS_ARCHIVE]

The multi-file archive-container half of the package close. Where `package/codec#CODEC` owns single-blob codec compression over one payload, `Archive` owns the two multi-file container arms — the `py7zr` `SevenZipFile` 7z container and the `stream-zip`/`stream-unzip` bounded-memory ZIP container — that fold a `*payloads` spread into ONE archive whose directory recovers the full `(name, ContentKey, algo, size)` row set, never N single-frame bundles. It COMPOSES the `package/codec#CODEC` `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold rather than re-owning it: `SevenZKnobs` (crypto/filters/header-encryption) and `ZipStreamKnobs` (ZIP method/level/password/names) are the two `CodecProfile` knob structs the codec union already carries, and `_archive_in_process`/`_archive_unpack` are the worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `seven_z`/`zip_stream` tags to. A 7z archive is the encrypted-filter-chain container; a ZIP-stream archive is the bounded-memory streamed container with WinZip AES-256 and a streamed-CRC list/test pass — both contribute `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` filling the `entries`/`verified` container slots the single-blob codecs leave zero.

## [01]-[INDEX]

- [01]-[ARCHIVE]: the `SEVEN_Z` (`py7zr` `SevenZipFile` filter-chain + AES256 header/content encryption) and `ZIP_STREAM` (`stream-zip` `stream_zip`/`async_stream_zip` bounded-memory member streaming + `stream-unzip` `stream_unzip` bounded-memory recovery) container arms — the `SevenZKnobs`/`ZipStreamKnobs` profile structs, the `ZipMethod` `Literal` axis and its `_ZIP_METHOD` dispatch row, the `_zip_members` `MemberFile`-fold builder, the streamed-CRC `zlib_ng.crc32` list/test fold, the `frozenset({NO_ENCRYPTION, AES_256})` trust allow-list, and the `_archive_in_process`/`_archive_unpack` worker pair the `package/codec#CODEC` dispatch delegates to, all on the cp315 core band.

## [02]-[ARCHIVE]

- Owner: `Archive` the multi-file container concern over the two `CompressionAlgo` container rows `SEVEN_Z`/`ZIP_STREAM`, composing the `package/codec#CODEC` `Bundle` owner; `SevenZKnobs` the frozen 7z policy struct (`filters`, `header_encryption`, `password`); `ZipStreamKnobs` the frozen ZIP-stream policy struct (`method`, `level`, `password`, `names`); `ZipMethod` the closed `Literal` over the `stream-zip` `Method` family resolved through one `_ZIP_METHOD` dispatch row. `Archive` owns no `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence`/`BundleManifest` type — those are the `package/codec#CODEC` scaffold this page extends through the two `CodecProfile` cases and the two worker arms, so a 7z or ZIP container is one row on the one union, never a parallel archive owner.
- Cases: `CompressionAlgo.SEVEN_Z` binds `py7zr.SevenZipFile` reading the `seven_z` `SevenZKnobs` profile case; `CompressionAlgo.ZIP_STREAM` binds `stream_zip`/`stream_unzip` reading the `zip_stream` `ZipStreamKnobs` profile case — matched by the codec `_in_process`/`_unpack_in_process` dispatch falling through to `_archive_in_process`/`_archive_unpack`, never an `if 7z` branch. The `ZipMethod` `Literal` (`"auto"`/`"zip64"`/`"zip32"`/`"store32"`/`"store64"`) resolves the `stream-zip` `ZIP_AUTO`/`ZIP_64`/`ZIP_32`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64` `Method` instances through the one frozen `_ZIP_METHOD` dispatch row at archive scope, so a string-built attribute lookup and a raw `Method`-instance profile field are the deleted forms and the serialized `ZipStreamKnobs` carries no native `Method` handle across the lane (both arms resolve on the cp315 core, so no lane crossing lands, but the `Literal` token keeps the profile serializable for the elision-key fold).
- Modality: both arms are multi-file — the `*payloads` spread the codec `pack` recovers folds into ONE container. `SEVEN_Z` writes each payload as a `payload-{i}` entry through `SevenZipFile.writef(BytesIO(payload), name)` then re-reads the written bytes through `SevenZipFile.test()` for the CRC-verified entry count; `ZIP_STREAM` folds the spread into one `stream_zip` member sequence so a multi-payload bundle is a single archive, never N archives. The `unpack` inverse recovers the full member row set from the container directory bounded — `SEVEN_Z` through `SevenZipFile.list()` reading the `(filename, uncompressed, crc32)` rows WITHOUT materializing a payload, `ZIP_STREAM` through `stream_unzip` draining each member's chunks through a rolling `zlib_ng.crc32` fold — so the list/test pass never buffers the payloads, only their size and CRC digest seed, and the recovered triples feed the codec `BundleManifest.of` content-key fold.
- Crypto: the `SEVEN_Z` arm threads `password` plus a `{"id": py7zr.FILTER_CRYPTO_AES256_SHA256}` filter-chain entry alongside `header_encryption=True`, so encryption is the functional three-field row the `.api` crypto axis mandates, never a lone `header_encryption` bool; `SevenZipFile.test()` re-reads the written archive for the CRC-verified entry count the evidence carries. The `ZIP_STREAM` arm threads `password` through `stream_zip(..., password=...)` switching every member to WinZip AES-256 (the `pycryptodome` AES-256/HMAC-SHA1/PBKDF2 backend `stream-zip` owns), and the `unpack` inverse passes the `password.encode()` `bytes` and an `allowed_encryption_mechanisms` allow-list (`frozenset({NO_ENCRYPTION, AES_256})` hardening the trust policy to refuse legacy ZipCrypto and weak AES key lengths) to `stream_unzip`, so the encrypted round-trip is one `password` row on each verb, never a parallel encrypted function.
- Stream: the `ZIP_STREAM` arm is the bounded-memory container — `_zip_members` folds each payload into a `(name, modified_at, mode, method, data)` `MemberFile` tuple whose `method` is the `_ZIP_METHOD[method]` row (`ZIP_AUTO(size, level)` the size-driven ZIP32/ZIP64 auto-upgrade default, `ZIP_64`/`ZIP_32` the forced rows, the `NO_COMPRESSION_*(size, crc_32)` rows the stored variants whose pre-declared CRC the composed `zlib_ng.crc32` substrate computes) and whose `data` is a one-chunk `iter((payload,))` lazy iterable so member bytes interleave encode-and-emit, then `stream_zip(members, password=...)` yields the container `bytes` chunks `b"".join(...)` seals into the bundle blob; the `async_stream_zip` async-iterable boundary rides the runtime `async_boundary` for a remote-streamed member feed. `unpack` drives `stream_unzip(chunks, password=..., allowed_encryption_mechanisms=...)` member by member, draining each member's `chunks` generator fully (the `UnfinishedIterationError` ordered-consumption guard the `.api` mandates) through a rolling `zlib_ng.crc32` fold that accumulates the per-member size and a 4-byte CRC digest seed into one `(name, size, digest)` triple, so the list/test pass folds each member to its row without ever buffering the payload — `bounded-memory` holds on both legs, and the CRC digest seeds the `BundleManifest.of` content-key fold.
- Receipt: each container pack contributes the `package/codec#CODEC` `BundleEvidence.measure` projection onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` — the `entries` slot is the member count and the `verified` slot is the container-level CRC-verified proof (`SEVEN_Z` from `SevenZipFile.test()`, `ZIP_STREAM` the streamed-integrity member count), the two slots the single-blob codecs leave zero. `_archive_in_process` returns the `(blobs, BundleEvidence)` pair the codec `_in_process` dispatch threads onto `_emit`, so the archive arm mints no receipt of its own; the evidence `level` carries the ZIP level for `ZIP_STREAM` and zero for `SEVEN_Z` (filter-chain selected, not a single level), and the `frame_size` carries the summed declared member size. The shared `package/codec#CODEC` `BundleEvidence` value crosses no module boundary into the receipt owner — the codec `_emit` spreads its named fields onto the flat-scalar `Bundle` case.
- Packages: `py7zr` (`SevenZipFile`, `FILTER_CRYPTO_AES256_SHA256`, `writef`, `list`, `test`, `extractall`, `getnames`, `FileInfo` with `filename`/`uncompressed`/`crc32`/`is_directory`, the `ArchiveError` typed-fault rail, `max_extract_size` decompression-bomb + `AbsolutePathError` traversal defense on the untrusted-upload extract path), `stream-zip` (`stream_zip`, `async_stream_zip`, `ZIP_64`, `ZIP_32`, `ZIP_AUTO`, `NO_COMPRESSION_32`, `NO_COMPRESSION_64`, the `MemberFile` `(name, modified_at, mode, method, data)` shape, `ZipError`/`ZipValueError`), `stream-unzip` (`stream_unzip`, `async_stream_unzip`, `NO_ENCRYPTION`, `AES_256`, the `(name, size, chunks)` triple, `UnzipError`/`UnzipValueError`/`InvalidOperationError`/`UnfinishedIterationError`) all on the cp315 core (un-gated; `stream-unzip` and its `stream-inflate` companion are pure-Python sdists that build and import on cp315); the shared `data:compression` substrate `zlib_ng.crc32` (the rolling CRC the stored-ZIP `NO_COMPRESSION_*` member and the streamed-unzip member fold seed through) composed not re-admitted; the `package/codec#CODEC` `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold and `expression`/`msgspec` consumed as the codec union's imports, never re-declared.
- Growth: a new container algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant in the codec owner, one arm in `_archive_in_process`, and one arm in `_archive_unpack`; a new ZIP method is one token on the `ZipMethod` `Literal` plus one `_ZIP_METHOD` dispatch-row entry; a new 7z filter is one entry on `SevenZKnobs.filters`; a new container evidence fact rides the existing `entries`/`verified` container slots, never a new receipt field. Zero new surface beside the two worker arms and the two profile structs.
- Boundary: a per-container archive class family, a `zipfile.ZipFile` whole-archive buffer where `stream-zip`/`stream-unzip` bound memory, a `py7zr` `readall`/`read` whole-archive in-memory mapping where the bounded `list()`/`crc32` directory read recovers the manifest, a temp-file extract round-trip where the streamed CRC fold lists/tests, a hardcoded `ZIP_64` literal or a raw `Method`-instance profile field where the `ZipMethod` `Literal` plus `_ZIP_METHOD` dispatch resolves, a lone `header_encryption` bool without the `FILTER_CRYPTO_AES256_SHA256` chain entry, a second encrypted function beside the `password` row, a parallel `BundleManifest`-per-container type, and a re-owned `Bundle`/`CompressionAlgo`/`BundleEvidence` where the `package/codec#CODEC` scaffold already owns them are the deleted forms; this page owns the two container worker arms and the two profile structs composing the codec union, and re-mints no content key. Both arms resolve on the cp315 core, so neither crosses the runtime subprocess lane the `package/codec#CODEC` gated `LZ4`/`BROTLI` arms ride.

```python signature
from datetime import datetime, timezone
from io import BytesIO
from collections.abc import Iterable
from typing import Final, Literal, assert_never

from msgspec import Struct

from artifacts.package.codec import BundleEvidence, CodecProfile, CompressionAlgo

type ZipMethod = Literal["auto", "zip64", "zip32", "store32", "store64"]


class SevenZKnobs(Struct, frozen=True):
    filters: tuple[dict[str, int], ...] = ()
    header_encryption: bool = False
    password: str | None = None


class ZipStreamKnobs(Struct, frozen=True):
    method: ZipMethod = "auto"
    level: int = 9
    password: str | None = None
    names: tuple[str, ...] = ()
```

```python signature
from datetime import datetime, timezone
from io import BytesIO
from collections.abc import Iterable

from stream_zip import ZIP_32, ZIP_64, ZIP_AUTO, NO_COMPRESSION_32, NO_COMPRESSION_64, stream_zip
from stream_unzip import AES_256, NO_ENCRYPTION, stream_unzip


def _zip_members(payloads: tuple[bytes, ...], knobs: ZipStreamKnobs) -> Iterable[tuple[str, datetime, int, object, Iterable[bytes]]]:
    from zlib_ng import zlib_ng

    stamp = datetime.now(timezone.utc)
    builder = _ZIP_METHOD[knobs.method]
    for index, payload in enumerate(payloads):
        name = knobs.names[index] if index < len(knobs.names) else f"payload-{index}"
        method = builder(len(payload), knobs.level) if knobs.method == "auto" else builder(len(payload), zlib_ng.crc32(payload)) if knobs.method.startswith("store") else builder
        yield name, stamp, 0o600, method, iter((payload,))


def _archive_in_process(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[bytes, ...], BundleEvidence]:
    match profile:
        case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
            import py7zr

            crypto = [{"id": py7zr.FILTER_CRYPTO_AES256_SHA256}] if k.password is not None else []
            chain = [*(dict(f) for f in k.filters), *crypto] or None
            sink = BytesIO()
            with py7zr.SevenZipFile(sink, mode="w", filters=chain, header_encryption=k.header_encryption, password=k.password) as archive:
                for i, payload in enumerate(payloads):
                    archive.writef(BytesIO(payload), f"payload-{i}")
            blob = sink.getvalue()
            with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password) as reader:
                verified = len(payloads) if reader.test() is not False else 0
            return (blob,), BundleEvidence.measure(algo, 0, 0, len(blob), verified, payloads, (blob,))
        case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
            blob = b"".join(stream_zip(_zip_members(payloads, k), password=k.password))
            return (blob,), BundleEvidence.measure(algo, k.level, 0, sum(map(len, payloads)), len(payloads), payloads, (blob,))
        case _:
            assert_never(profile)


def _archive_unpack(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
            import py7zr

            with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password) as reader:
                verified = reader.test() is not False
                infos = [info for info in reader.list() if not info.is_directory]
            return tuple((info.filename, info.uncompressed, info.crc32.to_bytes(4, "big")) for info in infos) if verified else ()
        case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
            from zlib_ng import zlib_ng

            mechanisms = frozenset({NO_ENCRYPTION, AES_256})
            password = k.password.encode() if k.password is not None else None
            recovered: list[tuple[str, int, bytes]] = []
            for name, _size, chunks in stream_unzip((blob,), password=password, allowed_encryption_mechanisms=mechanisms):
                running, total = 0, 0
                for chunk in chunks:
                    running, total = zlib_ng.crc32(chunk, running), total + len(chunk)
                recovered.append((name.decode(), total, running.to_bytes(4, "big")))
            return tuple(recovered)
        case _:
            assert_never(profile)


_ZIP_METHOD: Final[dict[ZipMethod, object]] = {"auto": ZIP_AUTO, "zip64": ZIP_64, "zip32": ZIP_32, "store32": NO_COMPRESSION_32, "store64": NO_COMPRESSION_64}
```

## [03]-[RESEARCH]

- [ZIP_STREAM_METHOD] [RESOLVED]: `stream_zip(files, chunk_size=65536, password=None, ...)` consumes an `Iterable[MemberFile]` of `(name, modified_at, mode, method, data)` tuples and yields container `bytes` chunks; `_zip_members` folds each payload into one tuple whose `method` is the `_ZIP_METHOD[knobs.method]` row — `ZIP_AUTO(uncompressed_size, level)` the size-driven ZIP32/ZIP64 auto-upgrade default, `ZIP_64`/`ZIP_32` the forced-format instances used directly, `NO_COMPRESSION_32`/`NO_COMPRESSION_64(uncompressed_size, crc_32)` the stored variants taking the pre-declared CRC32 (`zlib_ng.crc32(payload)` from the composed `data:compression` substrate) — and whose `data` is a one-chunk `iter((payload,))` lazy iterable. `password` switches every member to WinZip AES-256 (the `pycryptodome` backend `stream-zip` owns); `b"".join(stream_zip(...))` seals the bundle blob. The `stream_zip`/`async_stream_zip`/`ZIP_64`/`ZIP_32`/`ZIP_AUTO`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64` member names and the `(name, modified_at, mode, method, data)` tuple shape verify against the folder `stream-zip` `.api` `[02]` public-type rows `[01]`-`[08]` and the `[03]` entrypoint table; the `ZIP_AUTO(size, level)`/`NO_COMPRESSION_*(size, crc_32)` call shapes are the `.api` `[03]` `Method` selection rows `[03]`-`[05]`, all reflected installed `0.0.84` on cp315.
- [ZIP_STREAM_UNZIP] [RESOLVED]: `stream_unzip(zipfile_chunks, password=None, chunk_size=65536, allow_zip64=True, allowed_encryption_mechanisms=_ALL_ENCRYPTIONS)` yields `(name, size, chunks)` triples whose inner `chunks` generator MUST be fully drained before the outer generator advances (the `UnfinishedIterationError` under `InvalidOperationError` ordered-consumption guard); `unpack` drains each member's `chunks` through a rolling `zlib_ng.crc32` fold that accumulates the running CRC and the byte total without buffering the payload, projecting one `(name.decode(), total, crc.to_bytes(4, "big"))` triple, passing `password.encode()` `bytes` (not `str`) and the `frozenset({NO_ENCRYPTION, AES_256})` allow-list that rejects legacy ZipCrypto and weak AES key lengths before decryption — the bounded list/test pass never materializes a member payload, only its size and CRC digest seed. The `stream_unzip`/`async_stream_unzip`/`NO_ENCRYPTION`/`AES_256` member names, the `(name, size, chunks)` triple, the `bytes` password, and the `allowed_encryption_mechanisms` allow-list verify against the folder `stream-unzip` `.api` `[02]` public-type rows `[01]`/`[07]`/`[09]`-`[52]` and the `[03]` entrypoint row `[01]`; the package is the un-gated pure-Python sdist (`stream-inflate` companion) that builds and imports on cp315 (installed `0.0.101`), so it rides the core band beside `stream-zip`, never a `python_version<'3.15'` gate on a building sdist.
- [SEVEN_Z_INVERSE] [RESOLVED]: `py7zr.SevenZipFile(sink, mode="w", filters=[dict], header_encryption=bool, password=str)`, `writef(file_like, name)`, `SevenZipFile.test()` (the CRC-verification entrypoint returning `True`/`False`/`None`), and the unpack-side `SevenZipFile.list() -> list[FileInfo]` (the bounded archive-directory listing whose `FileInfo.filename`/`uncompressed`/`crc32`/`is_directory` rows the inverse reads WITHOUT materializing a payload) verify against the folder `py7zr` `.api` `[02]` public-type rows and the `[03]` archive-open/entrypoint tables (installed `1.1.3`, `py3-none-any` pure Python, un-gated). The SEVEN_Z `unpack` arm is bounded — `list()` yields the `(name, uncompressed size, crc32)` manifest rows and `test()` gates the CRC-verified pass, so the `BundleManifest` is recovered from the container directory rather than a whole-archive payload buffer, mirroring the `stream_unzip` streamed list/test. The `{"id": py7zr.FILTER_CRYPTO_AES256_SHA256}` filter-chain entry and the `password` constructor keyword are the verified crypto-row spelling, and `SevenZipFile.test()` is the CRC-verified entry-count source for the pack-side `verified` evidence field. `extractall` (the named-file directory-output recovery alternate when full payload bytes are required), `list`, `test`, `writef`, and `getnames` are all confirmed present on `SevenZipFile`; the prior `readall`/`read` in-memory-mapping spelling is NOT a member of this `py7zr` release and is the deleted phantom — the bounded `list()`/`crc32` directory read is the recovery surface. The member `FileInfo.crc32` integer is rendered to the 4-byte big-endian digest seed (`crc32.to_bytes(4, "big")`) the `BundleManifest.of` content-key fold consumes, never a re-decompressed payload.
- [ARCHIVE_BOUNDED_EXTRACT] [RESOLVED]: the bounded recovery never reaches for `py7zr` `extractall`/`BytesIOFactory` whole-archive materialization on the list/test pass — `SevenZipFile.list()` reads the container directory and `SevenZipFile.test()` walks the per-entry CRC, so the `(filename, uncompressed, crc32)` triple is recovered without a per-entry decode; the `extractall(path=None, *, callback=None, factory=None)` and the `io.BytesIOFactory(limit)`/`HashIOFactory()`/`NullIOFactory` streamed-sink protocol verify against the `py7zr` `.api` `[03]` extract rows and the `[02]` built-in-sink row for the case a downstream consumer DOES need the payload bytes (then a `max_extract_size`-bounded `extract(..., factory=BytesIOFactory(cap))` streams into a capped buffer mapping `DecompressionBombError`/`AbsolutePathError`/`CrcError` at the boundary), but the `BundleManifest` recovery itself stays directory-only. The `ZIP_STREAM` inverse is symmetric — `stream_unzip` drains each member's `chunks` to the CRC fold and discards them, so a list/test recovers the row set bounded and a payload-needing consumer drains the same `chunks` to its own sink (`documents/lens`, `data:tabular` `msgspec.msgpack.decode`) without a temp file.
- [BAND_PLACEMENT] [RESOLVED]: both container arms hold the cp315 core band — `py7zr` is `py3-none-any` pure Python (its codec backends `pyppmd`/`pybcj`/`brotli`/`pycryptodomex`/`inflate64` carry their own native wheels), and `stream-zip`/`stream-unzip` (with the `stream-inflate` companion) are pure-Python sdists that build and import on cp315, so neither rides the `package/codec#CODEC` gated `python_version<'3.15'` band the `LZ4`/`BROTLI` arms cross onto the runtime subprocess lane. `_archive_in_process`/`_archive_unpack` are the cp315-core worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `seven_z`/`zip_stream` tags to, importing `py7zr` at arm scope (the manifest import policy) and binding `stream_zip`/`stream_unzip` at module scope on the core band.
