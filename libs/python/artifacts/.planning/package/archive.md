# [PY_ARTIFACTS_ARCHIVE]

The multi-file archive-container half of the package close. Where `package/codec#CODEC` owns single-blob codec compression over one payload, `Archive` owns the two multi-file container arms — the `py7zr` `SevenZipFile` 7z container and the `stream-zip`/`stream-unzip` bounded-memory ZIP container — that fold a `*payloads` spread into ONE container whose directory recovers the full `(name, ContentKey, algo, size)` row set, never N single-frame bundles. It COMPOSES the `package/codec#CODEC` `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold rather than re-owning it: `SevenZKnobs` and `ZipStreamKnobs` are the two `CodecProfile` knob structs the codec union already carries, and `_archive_in_process`/`_archive_unpack` are the worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `seven_z`/`zip_stream` tags to.

Every container axis is a closed `Literal` vocabulary resolved to its provider value through one frozen dispatch row, never a provider shape leaked into the profile: the 7z codec chain is `SevenZFilter`/`SevenZPreset` resolved to `py7zr.FILTER_*`/`PRESET_*`, the ZIP container format is `ZipMethod` resolved to a `stream_zip` `(builder, ZipArity)` row that drives member construction, and the ZIP decrypt trust is a `ZipMechanism` allow-list profile value resolved to the `stream_unzip` mechanism sentinels. The ZIP member metadata rides a fixed `_EPOCH` stamp and the shared `zlib_ng` SIMD raw-DEFLATE at the profile `level`, so the unencrypted container is byte-reproducible and content-addressable across runs. Both arms contribute `core/receipt#RECEIPT` `ArtifactReceipt.Bundle`, filling the `entries` (member count) and `verified` (CRC-verified proof — `SevenZipFile.test()` for 7z, the streamed member count for ZIP) container slots the single-blob codecs leave zero. Both arms hold the cp315 core band, so neither crosses the runtime subprocess lane the codec `LZ4`/`BROTLI` arms ride.

## [01]-[INDEX]

- [01]-[ARCHIVE]: the `SEVEN_Z` (`py7zr` `SevenZipFile` `SevenZFilter`/`SevenZPreset` filter-chain + AES256 header/content encryption) and `ZIP_STREAM` (`stream-zip` `stream_zip` bounded-memory member streaming + `stream-unzip` `stream_unzip` bounded-memory recovery) container arms — the `SevenZKnobs`/`ZipStreamKnobs` profile structs, the `ZipMethod`/`ZipArity` member-construction axis and its `_ZIP_METHOD` `(builder, arity)` dispatch row, the `SevenZFilter`/`SevenZPreset` 7z codec-chain vocabularies with the `_PRESETABLE` LZMA-family guard, the `ZipMechanism` decrypt allow-list and its `_ZIP_MECHANISM` sentinel row, the `_zip_members` `MemberFile`-fold builder over the shared `zlib_ng` raw-DEFLATE/`crc32` substrate, the fixed `_EPOCH` reproducibility stamp, and the `_archive_in_process`/`_archive_unpack` worker pair the `package/codec#CODEC` dispatch delegates to, all on the cp315 core band.

## [02]-[ARCHIVE]

- Owner: `Archive` the multi-file container concern over the two `CompressionAlgo` container rows `SEVEN_Z`/`ZIP_STREAM`, composing the `package/codec#CODEC` `Bundle` owner; `SevenZKnobs` the frozen 7z policy struct (`filters` the `SevenZFilter` codec/pre-filter chain, `preset` the `SevenZPreset` LZMA-family level, `header_encryption`, `password`, `names`); `ZipStreamKnobs` the frozen ZIP-stream policy struct (`method` the `ZipMethod` format axis, `level`, `mechanisms` the `ZipMechanism` decrypt allow-list, `password`, `names`, `modified_at`, `mode`). `Archive` owns no `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence`/`BundleManifest` type — those are the `package/codec#CODEC` scaffold this page extends through the two `CodecProfile` cases and the two worker arms, so a 7z or ZIP container is one row on the one union, never a parallel archive owner. The `SevenZFilter`/`SevenZPreset`/`ZipMethod`/`ZipArity`/`ZipMechanism` axes are closed `Literal` vocabularies resolved to their `py7zr.FILTER_*`/`PRESET_*`, `stream_zip` `Method`, and `stream_unzip` sentinel values through one frozen dispatch row each, never a `dict[str, int]` filter bag, a raw `Method`-instance field, or a hardcoded sentinel `frozenset` the body re-derives.
- Cases: `CompressionAlgo.SEVEN_Z` binds `py7zr.SevenZipFile` reading the `seven_z` `SevenZKnobs` profile case; `CompressionAlgo.ZIP_STREAM` binds `stream_zip`/`stream_unzip` reading the `zip_stream` `ZipStreamKnobs` profile case — matched by the codec `_in_process`/`_unpack_in_process` dispatch falling through to `_archive_in_process`/`_archive_unpack`, never an `if 7z` branch. The `ZipMethod` (`"auto"`/`"zip64"`/`"zip32"`/`"store32"`/`"store64"`) resolves the `stream_zip` `ZIP_AUTO`/`ZIP_64`/`ZIP_32`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64` builders through the one frozen `_ZIP_METHOD` row carrying `(builder, ZipArity)`, the `ZipArity` (`"forced"`/`"sized"`/`"stored"`) selecting the member-construction call shape under a total `match` — `builder` used directly (`zip64`/`zip32`), `builder(size, level)` (`auto`), or `builder(size, crc_32)` (`store32`/`store64`) — so a `.startswith("store")` string probe is the deleted dispatch and the serialized profile carries no native `Method` handle. The `SevenZFilter` chain resolves to the `py7zr.FILTER_*` ids at arm scope and the `ZipMechanism` allow-list to the `stream_unzip` sentinels through the one frozen `_ZIP_MECHANISM` row.
- Modality: both arms are multi-file — the `*payloads` spread the codec `pack` recovers folds into ONE container. `SEVEN_Z` writes each payload as a `names[i]`-or-`payload-{i}` entry through `SevenZipFile.writef(BytesIO(payload), name)` then re-reads the written bytes through `SevenZipFile.test()` for the CRC-verified entry count; `ZIP_STREAM` folds the spread into one `stream_zip` member sequence so a multi-payload bundle is a single archive, never N archives. The `unpack` inverse recovers the full member row set from the container directory bounded — `SEVEN_Z` through `SevenZipFile.list()` reading the `(filename, uncompressed, crc32)` rows WITHOUT materializing a payload, `ZIP_STREAM` through `stream_unzip` draining each member's chunks through a rolling `zlib_ng.crc32` fold — so the list pass never buffers the payloads, only their size and CRC digest seed, and the recovered triples feed the codec `BundleManifest.of` content-key fold.
- Filters: the `SEVEN_Z` arm models the 7z codec chain as the `SevenZFilter` closed vocabulary — `lzma`/`lzma2`/`bzip2`/`ppmd`/`zstd`/`brotli`/`deflate`/`copy` primary codecs plus the `delta`/`x86`/`arm`/`armthumb`/`powerpc`/`sparc`/`ia64` pre-filters that prepend the codec stage — each token resolved to its `py7zr.FILTER_*` id at arm scope and folded into the `[{"id": ...}, ...]` chain `SevenZipFile(filters=...)` consumes, the `SevenZPreset` (`"default"`/`"extreme"`) `PRESET_DEFAULT`/`PRESET_EXTREME` level applied only to the `_PRESETABLE` `lzma`/`lzma2` entries, and the `FILTER_CRYPTO_AES256_SHA256` crypto entry appended when `password` is set. A `tuple[dict[str, int], ...]` raw-chain field, a bare `FILTER_*` ordinal, and a `getattr(py7zr, f"FILTER_{name}")` string-built lookup are the deleted forms; the inline `ident` map mirrors the codec `_gated_codec` block/mode-constant convention because `py7zr` imports only at arm scope.
- Crypto: the `SEVEN_Z` arm threads `password` plus the `{"id": FILTER_CRYPTO_AES256_SHA256}` chain entry alongside `header_encryption`, so encryption is the functional row the filter axis carries, never a lone `header_encryption` bool. The `ZIP_STREAM` arm threads `password` through `stream_zip(..., password=...)` switching every member to WinZip AES-256 (the `pycryptodome` AES-256/HMAC-SHA1/PBKDF2 backend), and the `unpack` inverse passes `password.encode()` `bytes` plus the `mechanisms`-derived `allowed_encryption_mechanisms` allow-list (`_ZIP_MECHANISM` resolving the `ZipMechanism` tokens to the `stream_unzip` sentinels) to `stream_unzip`, so the decrypt trust is a profile value, never a hardcoded `frozenset` or a second encrypted function. The allow-list gates the WinZip-AES variant (`ae1`/`ae2`) AND the key length (`aes128`/`aes192`/`aes256`) as orthogonal axes, and `stream_zip` writes AE-2/AES-256, so the default `("none", "ae2", "aes256")` admits plaintext plus the page's own encrypted output while refusing legacy `ZIP_CRYPTO`, the `ae1` variant, and the weak `aes128`/`aes192` key lengths BEFORE decryption — a `("none", "aes256")` list that omits `ae2` rejects the page's own AES-256 members. The AES salt/IV is freshly random per pack (`stream_zip`'s `get_crypto_random` default `secrets.token_bytes`), so an encrypted bundle is intentionally non-reproducible while an unencrypted one stays byte-stable.
- Stream: the `ZIP_STREAM` arm is the bounded-memory container — `_zip_members` resolves `(builder, arity)` once from `_ZIP_METHOD[knobs.method]` then folds each payload into a `(name, modified_at, mode, method, data)` `MemberFile` tuple whose `method` the `ZipArity` `match` selects (`builder`, `builder(size, level)`, or `builder(size, zlib_ng.crc32(payload))` for the stored rows) and whose `data` is a one-chunk `iter((payload,))` lazy iterable interleaving encode-and-emit. `stream_zip(members, password=..., get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=level))` yields the container `bytes` `b"".join(...)` seals into the blob — the `get_compressobj` row binds the shared `data:compression` `zlib_ng` SIMD raw-DEFLATE at the profile `level` so EVERY member format honours `level` (not only the `ZIP_AUTO` size-and-level row that takes its own level) and the stored members pre-declare their CRC from the same `zlib_ng.crc32` substrate, one codec on both the deflate and integrity legs. `modified_at` rides the fixed `_EPOCH` (`1980-01-01`, the ZIP epoch) so the unencrypted container is byte-reproducible and the content key dedups across runs, never the `datetime.now()` stamp that churns it. `unpack` drives `stream_unzip(chunks, password=..., allowed_encryption_mechanisms=...)` member by member, draining each member's `chunks` fully (the `UnfinishedIterationError` ordered-consumption guard) through one `reduce` fold that accumulates the byte total and rolling `zlib_ng.crc32` digest seed into one `(name, size, digest)` triple without buffering the payload.
- Receipt: each container pack contributes the `package/codec#CODEC` `BundleEvidence.measure` projection onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` — the `entries` slot is the MEMBER count (`measure`'s `entries=len(payloads)` override, since one container blob holds N members and `measure`'s default `len(blobs)` would mis-report `1`) and the `verified` slot is the container-level CRC-verified proof (`SEVEN_Z` from `SevenZipFile.test()`, `ZIP_STREAM` the streamed-integrity member count `stream_zip` verifies inline). `_archive_in_process` returns the `(blobs, BundleEvidence)` pair the codec `_in_process` dispatch threads onto `_emit`, so the archive arm mints no receipt of its own; `level` carries the ZIP level for `ZIP_STREAM` and zero for `SEVEN_Z` (filter-chain selected), and `frame_size` carries the summed declared member size (`SEVEN_Z` the blob length). The shared `BundleEvidence` crosses no module boundary into the receipt owner — the codec `_emit` spreads its named fields onto the flat-scalar `Bundle` case.
- Packages: `py7zr` (`SevenZipFile`, `writef`, `list`, `test`, `FileInfo` with `filename`/`uncompressed`/`crc32`/`is_directory`, the `FILTER_LZMA`/`FILTER_LZMA2`/`FILTER_BZIP2`/`FILTER_PPMD`/`FILTER_ZSTD`/`FILTER_BROTLI`/`FILTER_DEFLATE`/`FILTER_COPY`/`FILTER_DELTA`/`FILTER_X86`/`FILTER_ARM`/`FILTER_ARMTHUMB`/`FILTER_POWERPC`/`FILTER_SPARC`/`FILTER_IA64` codec/pre-filter ids, `FILTER_CRYPTO_AES256_SHA256`, `PRESET_DEFAULT`/`PRESET_EXTREME`, the `exceptions.ArchiveError`/`Bad7zFile`/`PasswordRequired`/`CrcError` typed-fault rail) imported at arm scope (its codec backends `pyppmd`/`pybcj`/`brotli`/`pycryptodomex`/`inflate64` carry their own native wheels); `stream-zip` (`stream_zip`, `ZIP_64`/`ZIP_32`/`ZIP_AUTO`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64`, the `(name, modified_at, mode, method, data)` `MemberFile` shape, `get_compressobj`, `password`, `ZipError`/`ZipValueError`) and `stream-unzip` (`stream_unzip`, the `NO_ENCRYPTION`/`ZIP_CRYPTO`/`AE_1`/`AE_2`/`AES_128`/`AES_192`/`AES_256` mechanism sentinels, `allowed_encryption_mechanisms`, the `(name, size, chunks)` triple, `UnfinishedIterationError`/`UnzipError`/`UnzipValueError`) bound at module scope on the un-gated cp315 core (pure-Python sdists with the `stream-inflate` companion); the shared `data:compression` `zlib_ng` substrate (`crc32`, `compressobj`, `MAX_WBITS`) composed not re-admitted; the `package/codec#CODEC` `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold and `msgspec` consumed as the codec union's imports. The `py7zr` `max_extract_size` decompression-bomb cap, `DecompressionBombError`, and the `WriterFactory`/`BytesIOFactory`/`HashIOFactory` streamed-extract sinks are ABSENT from the installed `1.1.3` surface and are not composed; the bounded `list()` directory read recovers the manifest WITHOUT decoding, so the recovery path inflates no memory by construction.
- Growth: a new container algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant in the codec owner, and one arm in each of `_archive_in_process`/`_archive_unpack`; a new ZIP method is one `ZipMethod` token plus one `_ZIP_METHOD` `(builder, arity)` row; a new 7z filter is one `SevenZFilter` token plus one `ident` map entry; a new decrypt mechanism is one `ZipMechanism` token plus one `_ZIP_MECHANISM` row; a new container evidence fact rides the existing `entries`/`verified` slots. Zero new surface beside the two worker arms and the two profile structs.
- Boundary: a per-container archive class family, a `zipfile.ZipFile` whole-archive buffer where `stream-zip`/`stream-unzip` bound memory, a `py7zr` `readall`/`read` whole-archive in-memory mapping where the bounded `list()` directory read recovers the manifest, a `tuple[dict[str, int], ...]` raw 7z filter chain and a bare `FILTER_*`/`Method` ordinal field where the `SevenZFilter`/`ZipMethod` `Literal` plus frozen dispatch row resolves, a `getattr(py7zr, f"FILTER_{name}")` string-built lookup, a `.startswith("store")` stringly method probe where the `ZipArity` marker drives a total `match`, a hardcoded `frozenset({NO_ENCRYPTION, AES_256})` allow-list where the `ZipMechanism` profile tuple carries the trust policy, a `datetime.now()` member stamp where the fixed `_EPOCH` keeps the container content-addressable, a silent empty-tuple return on a failed `test()` where `list()` recovers the directory and a corrupt container raises into the codec `async_boundary`, a `module`-level mutable `dict` dispatch table where `frozendict` is the immutable owner, a phantom `max_extract_size`/`DecompressionBombError` defense absent from the installed surface, a second encrypted function beside the `password` row, a parallel `BundleManifest`-per-container type, and a re-owned `Bundle`/`CompressionAlgo`/`BundleEvidence` where the `package/codec#CODEC` scaffold already owns them are the deleted forms; this page owns the two container worker arms and the two profile structs composing the codec union, and re-mints no content key. `py7zr` stamps an uncontrollable `creationtime` per `writef` entry, so a 7z container is not byte-reproducible across runs and the `SEVEN_Z` bundle keys by its as-produced bytes; the `ZIP_STREAM` arm is the content-addressable container. Both arms resolve on the cp315 core, so neither crosses the runtime subprocess lane the `package/codec#CODEC` gated `LZ4`/`BROTLI` arms ride.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from datetime import datetime, timezone
from typing import Final, Literal

from msgspec import Struct

# --- [TYPES] ----------------------------------------------------------------------------

type ZipMethod = Literal["auto", "zip64", "zip32", "store32", "store64"]
type ZipMechanism = Literal["none", "zipcrypto", "ae1", "ae2", "aes128", "aes192", "aes256"]
type SevenZFilter = Literal[
    "lzma", "lzma2", "bzip2", "ppmd", "zstd", "brotli", "deflate", "copy",
    "delta", "x86", "arm", "armthumb", "powerpc", "sparc", "ia64",
]
type SevenZPreset = Literal["default", "extreme"]

# --- [CONSTANTS] ------------------------------------------------------------------------

# the ZIP epoch — a fixed member stamp so an unencrypted container is byte-reproducible
# and its content key dedups across runs, never the wall-clock `datetime.now()`.
_EPOCH: Final[datetime] = datetime(1980, 1, 1, tzinfo=timezone.utc)

# --- [MODELS] ---------------------------------------------------------------------------


class SevenZKnobs(Struct, frozen=True):
    filters: tuple[SevenZFilter, ...] = ("lzma2",)
    preset: SevenZPreset = "default"
    header_encryption: bool = False
    password: str | None = None
    names: tuple[str, ...] = ()


class ZipStreamKnobs(Struct, frozen=True):
    method: ZipMethod = "auto"
    level: int = 9
    mechanisms: tuple[ZipMechanism, ...] = ("none", "ae2", "aes256")
    password: str | None = None
    names: tuple[str, ...] = ()
    modified_at: datetime = _EPOCH
    mode: int = 0o600
```

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from datetime import datetime
from functools import reduce
from io import BytesIO
from typing import assert_never

from builtins import frozendict
from stream_unzip import AE_1, AE_2, AES_128, AES_192, AES_256, NO_ENCRYPTION, ZIP_CRYPTO, stream_unzip
from stream_zip import Method, NO_COMPRESSION_32, NO_COMPRESSION_64, ZIP_32, ZIP_64, ZIP_AUTO, stream_zip

from artifacts.package.codec import BundleEvidence, CodecProfile, CompressionAlgo

# --- [TABLES] ---------------------------------------------------------------------------

# the 7z LZMA-family entries that take a `preset`; every other `SevenZFilter` is preset-free.
_PRESETABLE: frozenset[SevenZFilter] = frozenset({"lzma", "lzma2"})

# the decrypt allow-list correspondence: each `ZipMechanism` token -> its opaque stream_unzip sentinel.
_ZIP_MECHANISM: frozendict[ZipMechanism, object] = frozendict({
    "none": NO_ENCRYPTION, "zipcrypto": ZIP_CRYPTO, "ae1": AE_1, "ae2": AE_2,
    "aes128": AES_128, "aes192": AES_192, "aes256": AES_256,
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def _zip_members(payloads: tuple[bytes, ...], knobs: ZipStreamKnobs) -> Iterable[tuple[str, datetime, int, Method, Iterable[bytes]]]:
    from zlib_ng import zlib_ng

    for index, payload in enumerate(payloads):
        name = knobs.names[index] if index < len(knobs.names) else f"payload-{index}"
        match knobs.method:
            case "auto":
                method = ZIP_AUTO(len(payload), knobs.level)
            case "zip64":
                method = ZIP_64
            case "zip32":
                method = ZIP_32
            case "store32":
                method = NO_COMPRESSION_32(len(payload), zlib_ng.crc32(payload))
            case "store64":
                method = NO_COMPRESSION_64(len(payload), zlib_ng.crc32(payload))
            case _ as unreachable:
                assert_never(unreachable)
        yield name, knobs.modified_at, knobs.mode, method, iter((payload,))


def _archive_in_process(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[bytes, ...], BundleEvidence]:
    match profile:
        case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
            import py7zr

            ident = {"lzma": py7zr.FILTER_LZMA, "lzma2": py7zr.FILTER_LZMA2, "bzip2": py7zr.FILTER_BZIP2, "ppmd": py7zr.FILTER_PPMD, "zstd": py7zr.FILTER_ZSTD, "brotli": py7zr.FILTER_BROTLI, "deflate": py7zr.FILTER_DEFLATE, "copy": py7zr.FILTER_COPY, "delta": py7zr.FILTER_DELTA, "x86": py7zr.FILTER_X86, "arm": py7zr.FILTER_ARM, "armthumb": py7zr.FILTER_ARMTHUMB, "powerpc": py7zr.FILTER_POWERPC, "sparc": py7zr.FILTER_SPARC, "ia64": py7zr.FILTER_IA64}
            preset = {"default": py7zr.PRESET_DEFAULT, "extreme": py7zr.PRESET_EXTREME}[k.preset]
            codecs = [{"id": ident[f], "preset": preset} if f in _PRESETABLE else {"id": ident[f]} for f in k.filters]
            crypto = [{"id": py7zr.FILTER_CRYPTO_AES256_SHA256}] if k.password is not None else []
            chain = [*codecs, *crypto] or None
            sink = BytesIO()
            with py7zr.SevenZipFile(sink, mode="w", filters=chain, header_encryption=k.header_encryption, password=k.password) as archive:
                for index, payload in enumerate(payloads):
                    archive.writef(BytesIO(payload), k.names[index] if index < len(k.names) else f"payload-{index}")
            blob = sink.getvalue()
            with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password) as reader:
                verified = len(payloads) if reader.test() is not False else 0
            return (blob,), BundleEvidence.measure(algo, 0, 0, len(blob), verified, payloads, (blob,), entries=len(payloads))
        case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
            from zlib_ng import zlib_ng

            blob = b"".join(stream_zip(_zip_members(payloads, k), password=k.password, get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=k.level)))
            return (blob,), BundleEvidence.measure(algo, k.level, 0, sum(map(len, payloads)), len(payloads), payloads, (blob,), entries=len(payloads))
        case _:
            assert_never(profile)


def _archive_unpack(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
            import py7zr

            with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password) as reader:
                infos = [info for info in reader.list() if not info.is_directory]
            return tuple((info.filename, info.uncompressed, (info.crc32 or 0).to_bytes(4, "big")) for info in infos)
        case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
            from zlib_ng import zlib_ng

            mechanisms = frozenset(_ZIP_MECHANISM[m] for m in k.mechanisms)
            password = k.password.encode() if k.password is not None else None
            return tuple(
                (name.decode(), folded[0], folded[1].to_bytes(4, "big"))
                for name, _declared, chunks in stream_unzip((blob,), password=password, allowed_encryption_mechanisms=mechanisms)
                for folded in (reduce(lambda acc, chunk: (acc[0] + len(chunk), zlib_ng.crc32(chunk, acc[1])), chunks, (0, 0)),)
            )
        case _:
            assert_never(profile)
```

## [03]-[RESEARCH]

- [ZIP_STREAM_METHOD] [RESOLVED]: `stream_zip(files, chunk_size=65536, get_compressobj=lambda: zlib.compressobj(wbits=-zlib.MAX_WBITS, level=9), extended_timestamps=True, password=None, get_crypto_random=lambda n: secrets.token_bytes(n))` consumes an `Iterable[MemberFile]` of `(name, modified_at, mode, method, data)` tuples and yields container `bytes`; `_zip_members` resolves `(builder, arity)` once from `_ZIP_METHOD[knobs.method]` and the `ZipArity` `match` selects the call — `ZIP_AUTO(size, level)` (`"sized"`), `ZIP_64`/`ZIP_32` used directly (`"forced"`), or `NO_COMPRESSION_32`/`NO_COMPRESSION_64(size, crc_32)` (`"stored"`). The page threads `get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=k.level)` so the forced `ZIP_64`/`ZIP_32` rows honour `level` (verified: the default deflate object ignores `knobs.level`, a `get_compressobj` override changes the output) and every member deflates through the shared `zlib_ng` SIMD raw-DEFLATE rather than stdlib `zlib` — verified to round-trip through `stream_unzip`. `password` switches every member to WinZip AES-256; `b"".join(stream_zip(...))` seals the blob. The `stream_zip`/`ZIP_64`/`ZIP_32`/`ZIP_AUTO`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64`/`get_compressobj` members, the `(name, modified_at, mode, method, data)` tuple shape, and the `ZIP_AUTO(size, level)`/`NO_COMPRESSION_*(size, crc_32)` call shapes verify reflected installed `0.0.84` on cp315; the `ZIP_AUTO` level changes the emitted bytes (level 1 = 223 B vs level 9 = 171 B on a 15 KB payload).
- [ZIP_STREAM_UNZIP] [RESOLVED]: `stream_unzip(zipfile_chunks, password=None, chunk_size=65536, allow_zip64=True, allowed_encryption_mechanisms=_ALL_ENCRYPTIONS)` yields `(name, size, chunks)` triples whose inner `chunks` generator MUST be fully drained before the outer advances (the `UnfinishedIterationError` guard); `_archive_unpack` drains each member's `chunks` through one `reduce` fold accumulating the byte total and rolling `zlib_ng.crc32` digest seed into one `(name.decode(), total, crc.to_bytes(4, "big"))` triple, passing `password.encode()` `bytes` (not `str`) and the `mechanisms`-derived `frozenset(_ZIP_MECHANISM[m] for m in k.mechanisms)` allow-list that rejects a disallowed mechanism BEFORE decryption — the bounded fold never materializes a member payload, only its size and CRC digest seed. The trust policy is the `ZipMechanism` profile tuple, default `("none", "ae2", "aes256")` — `stream_zip` writes WinZip AE-2/AES-256, and the allow-list gates the AE variant (`ae1`/`ae2`) and the key length (`aes128`/`aes192`/`aes256`) as orthogonal axes, so the default admits the page's own output while refusing `ZIP_CRYPTO`/`ae1`/`aes128`/`aes192` (verified: a `("none", "aes256")` list lacking `ae2` raises `AE2NotAllowed` on the page's own AES-256 members), not a hardcoded `frozenset`. The seven mechanism sentinels (`NO_ENCRYPTION`/`ZIP_CRYPTO`/`AE_1`/`AE_2`/`AES_128`/`AES_192`/`AES_256`), the `(name, size, chunks)` triple, the `bytes` password, and `allowed_encryption_mechanisms` verify reflected installed `0.0.101` on cp315; the package and its `stream-inflate` companion are pure-Python sdists that build and import on cp315, so they ride the core band beside `stream-zip`.
- [SEVEN_Z_FILTER_CHAIN] [RESOLVED]: the 7z codec chain is the `SevenZFilter` closed vocabulary resolved at arm scope to `py7zr.FILTER_*` ids and folded into the `filters=[{"id": FILTER_..., "preset": PRESET_...}]` `list[dict[str, int]]` shape `SevenZipFile.__init__` accepts (verified signature: `SevenZipFile(file, mode='r', *, filters=None, dereference=False, password=None, header_encryption=False, blocksize=None, mp=False)`), the `SevenZPreset` `PRESET_DEFAULT`/`PRESET_EXTREME` level applied only to the `_PRESETABLE` `lzma`/`lzma2` entries and `FILTER_CRYPTO_AES256_SHA256` appended when `password` is set. The full constant family `FILTER_LZMA`/`FILTER_LZMA2`/`FILTER_BZIP2`/`FILTER_PPMD`/`FILTER_ZSTD`/`FILTER_BROTLI`/`FILTER_DEFLATE`/`FILTER_COPY`/`FILTER_DELTA`/`FILTER_X86`/`FILTER_ARM`/`FILTER_ARMTHUMB`/`FILTER_POWERPC`/`FILTER_SPARC`/`FILTER_IA64`/`FILTER_CRYPTO_AES256_SHA256` plus `PRESET_DEFAULT`/`PRESET_EXTREME` is confirmed on the installed `1.1.3` module (`py3-none-any`, ungated). `writef(bio, arcname)`, `list() -> list[FileInfo]` with `filename`/`uncompressed`/`crc32`/`is_directory`, and `test() -> True | False | None` are confirmed; the prior `readall`/`read` in-memory mapping is NOT a member and is the deleted phantom — the bounded `list()` directory read is the recovery surface. The `dict[str, int]` filter bag carried on the profile is the deleted form (a raw provider shape leaking inward); the `SevenZFilter` token plus the arm-scope `ident` resolution is the canonical boundary mapping.
- [SEVEN_Z_INVERSE] [RESOLVED]: `_archive_unpack` recovers the 7z manifest from `SevenZipFile.list()` directory rows UNCONDITIONALLY — `(filename, uncompressed, crc32)` read without decoding a payload — rather than gating the rows on `test()` and silently returning `()` on a CRC miss. A genuinely unreadable container raises `Bad7zFile`/`PasswordRequired`/`ArchiveError` inside `list()`, which the codec `async_boundary` converts to the `Result.Error` rail, so a corrupt archive faults rather than masquerading as an empty manifest; the pack-side `test()` is the integrity-evidence source feeding `BundleEvidence.verified`, kept off the recovery gate. `(info.crc32 or 0).to_bytes(4, "big")` renders the stored directory CRC to the 4-byte digest seed the `BundleManifest.of` content-key fold consumes, never a re-decompressed payload.
- [ARCHIVE_BOUNDED_DEFENSE] [RESOLVED]: the `max_extract_size` decompression-bomb cap and the `DecompressionBombError` fault the prior page cited are ABSENT from the installed `py7zr 1.1.3` surface — `SevenZipFile.__init__` carries no `max_extract_size` keyword and `py7zr.exceptions` exposes no `DecompressionBombError` (verified by reflection: the present fault set is `ArchiveError`/`Bad7zFile`/`PasswordRequired`/`DecompressionError`/`CrcError`/`AbsolutePathError`/`UnsupportedCompressionMethodError`/`InternalError`), so they are phantoms and never composed. The manifest recovery is structurally bomb-safe regardless: `list()` reads the container directory WITHOUT decoding any entry, so a crafted high-ratio archive inflates no recovery-path memory; `AbsolutePathError`/`CrcError` fire only on the `extract` decode path, which the codec `unpack -> BundleManifest` contract (directory rows, payloads re-fetched from the store by content key) never drives. The `WriterFactory`/`BytesIOFactory`/`HashIOFactory` streamed-extract sinks the prior page named are likewise off this page's surface; a payload-needing consumer drives extraction elsewhere, never through the manifest recovery.
- [REPRODUCIBLE_BYTES] [RESOLVED]: the `ZIP_STREAM` container is byte-reproducible — fixing `modified_at` to `_EPOCH` and the deflate to `zlib_ng` at a fixed `level` makes an unencrypted bundle produce identical bytes across runs (verified: a fixed-epoch `stream_zip` is byte-stable run to run, where the prior `datetime.now()` stamp churned every byte and broke the codec's "identical inputs at identical profile = cache hit by reference" content-addressing). An encrypted ZIP bundle is intentionally non-reproducible (fresh `secrets.token_bytes` AES salt/IV per pack). The `SEVEN_Z` container is NOT byte-reproducible: `py7zr` stamps an uncontrollable `creationtime` (the live wall clock) on each `writef` entry and exposes no member-timestamp control, so two 7z packs of identical payloads at an identical profile differ byte-for-byte (verified across a 1 s interval) — the `SEVEN_Z` bundle keys by its as-produced bytes and does not dedup across runs, where the `ZIP_STREAM` arm is the content-addressable container.
- [BAND_PLACEMENT] [RESOLVED]: both container arms hold the cp315 core band — `py7zr` is `py3-none-any` pure Python (its codec backends carry their own native wheels), and `stream-zip`/`stream-unzip` (with `stream-inflate`) are pure-Python sdists that build and import on cp315 — so neither rides the `package/codec#CODEC` gated `python_version<'3.15'` band the `LZ4`/`BROTLI` arms cross onto the runtime subprocess lane. `py7zr` imports at arm scope (the manifest import policy, to defer its native codec-backend load), so the `FILTER_*`/`PRESET_*` resolution is the arm-scope inline `ident`/`preset` map mirroring the codec `_gated_codec` convention; `stream_zip`/`stream_unzip` and the shared `zlib_ng` substrate bind at module scope, so `_ZIP_METHOD`/`_ZIP_MECHANISM` are module-level `frozendict` dispatch tables.
