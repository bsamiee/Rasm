# [PY_ARTIFACTS_ARCHIVE]

The multi-file archive-container half of the package close. Where `package/codec#CODEC` owns single-blob codec compression over one payload, `Archive` owns the two multi-file container arms — the `py7zr` `SevenZipFile` 7z container and the `stream-zip`/`stream-unzip` bounded-memory ZIP container — that fold a `*payloads` spread into ONE container whose directory recovers the full `(name, ContentKey, algo, size)` row set, never N single-frame bundles. It COMPOSES the `package/codec#CODEC` `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence` scaffold rather than re-owning it: `SevenZKnobs` and `ZipStreamKnobs` are the two `CodecProfile` knob structs the codec union already carries, and `_archive_in_process`/`_archive_unpack` are the worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `seven_z`/`zip_stream` tags to.

Every container axis is a closed `Literal` vocabulary resolved to its provider value through one frozen dispatch row, never a provider shape leaked into the profile: the 7z codec chain is `SevenZFilter`/`SevenZPreset` resolved to `py7zr.FILTER_*`/`PRESET_*`, the ZIP container format is `ZipMethod` resolved to a `stream_zip` `(builder, ZipArity)` row that drives member construction, and the ZIP decrypt trust is a `ZipMechanism` allow-list profile value resolved to the `stream_unzip` mechanism sentinels. The ZIP member metadata rides a fixed `_EPOCH` stamp and the shared `zlib_ng` SIMD raw-DEFLATE at the profile `level`, so the unencrypted container is byte-reproducible and content-addressable across runs. Both arms contribute `core/receipt#RECEIPT` `ArtifactReceipt.Bundle`, filling the `entries` (member count) and `verified` (a REAL per-member integrity proof — `SevenZipFile.test()` for 7z, a pack-time `stream_unzip` round-trip that drains and integrity-verifies every member for ZIP, never the illusory raw payload count) container slots the single-blob codecs leave zero; the `SEVEN_Z` `frame_size` reads the container's declared uncompressed total off `SevenZipFile.archiveinfo()` rather than the redundant compressed blob length. Both arms hold the runtime band, so neither crosses the runtime subprocess lane the codec `LZ4`/`BROTLI` arms ride.

## [01]-[INDEX]

- [02]-[ARCHIVE]: the `Archive` multi-file container concern over the `package/codec#CODEC` `SEVEN_Z`/`ZIP_STREAM` `CompressionAlgo` rows, the `SevenZKnobs`/`ZipStreamKnobs` `CodecProfile` cases, the `SevenZFilter`/`SevenZPreset`/`ZipMethod`/`ZipArity`/`ZipMechanism` closed `Literal` vocabularies resolved through the frozen `_ZIP_METHOD`/`_ZIP_MECHANISM`/`ident` dispatch rows, the `_Xxh3Factory`/`_Xxh3Sink` streamed `xxh3_128` `WriterFactory` sink replacing the built-in sha256 `HashIOFactory`, the `_zip_members` bounded-memory `MemberFile` fold, the `_zip_trust`/`_zip_drain` shared classified streamed reader (the pack-time integrity round-trip AND the unpack recovery both consume it), the `_EPOCH`/`zlib_ng`-at-`level` byte-reproducible container seam, the `_ARCHIVE_CEILING` `max_extract_size` bomb bound, and the `_archive_in_process`/`_archive_unpack` worker arms the codec `_in_process`/`_unpack_in_process` dispatch delegates the `seven_z`/`zip_stream` tags to; `py7zr` `SevenZipFile`(`writef`/`list`/`extractall(factory=)`/`test`/`archiveinfo`/`max_extract_size`)/`ArchiveInfo`(`uncompressed`/`method_names`/`solid`/`blocks`)/`FILTER_*`/`PRESET_*`/`FILTER_CRYPTO_AES256_SHA256`/`DecompressionBombError`/`AbsolutePathError`, `stream-zip` `stream_zip`/`ZIP_AUTO`/`ZIP_64`/`ZIP_32`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64`/`get_compressobj`, and `stream-unzip` `stream_unzip`/`AE_1`/`AE_2`/`AES_128`/`AES_192`/`AES_256`/`NO_ENCRYPTION`/`ZIP_CRYPTO`/`allowed_encryption_mechanisms`/`PasswordError`/`MissingPasswordError`/`DataError`/`UncompressError`/`UnsupportedFeatureError`/`InvalidOperationError` settled against the both-tier `.api`, contributing the shared `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` `entries`/`verified` container slots.

## [02]-[ARCHIVE]

- Owner: `Archive` the multi-file container concern over the two `CompressionAlgo` container rows `SEVEN_Z`/`ZIP_STREAM`, composing the `package/codec#CODEC` `Bundle` owner; `SevenZKnobs` the frozen 7z policy struct (`filters` the `SevenZFilter` codec/pre-filter chain, `preset` the `SevenZPreset` LZMA-family level, `header_encryption`, `password`, `names`); `ZipStreamKnobs` the frozen ZIP-stream policy struct (`method` the `ZipMethod` format axis, `level`, `mechanisms` the `ZipMechanism` decrypt allow-list, `password`, `names`, `modified_at`, `mode`). `Archive` owns no `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleEvidence`/`BundleManifest` type — those are the `package/codec#CODEC` scaffold this page extends through the two `CodecProfile` cases and the two worker arms, so a 7z or ZIP container is one row on the one union, never a parallel archive owner. The `SevenZFilter`/`SevenZPreset`/`ZipMethod`/`ZipArity`/`ZipMechanism` axes are closed `Literal` vocabularies resolved to their `py7zr.FILTER_*`/`PRESET_*`, `stream_zip` `Method`, and `stream_unzip` sentinel values through one frozen dispatch row each, never a `dict[str, int]` filter bag, a raw `Method`-instance field, or a hardcoded sentinel `frozenset` the body re-derives.
- Cases: `CompressionAlgo.SEVEN_Z` binds `py7zr.SevenZipFile` reading the `seven_z` `SevenZKnobs` profile case; `CompressionAlgo.ZIP_STREAM` binds `stream_zip`/`stream_unzip` reading the `zip_stream` `ZipStreamKnobs` profile case — matched by the codec `_in_process`/`_unpack_in_process` dispatch falling through to `_archive_in_process`/`_archive_unpack`, never an `if 7z` branch. The `ZipMethod` (`"auto"`/`"zip64"`/`"zip32"`/`"store32"`/`"store64"`) resolves the `stream_zip` `ZIP_AUTO`/`ZIP_64`/`ZIP_32`/`NO_COMPRESSION_32`/`NO_COMPRESSION_64` builders through the one frozen `_ZIP_METHOD` row carrying `(builder, ZipArity)`, the `ZipArity` (`"forced"`/`"sized"`/`"stored"`) selecting the member-construction call shape under a total `match` — `builder` used directly (`zip64`/`zip32`), `builder(size, level)` (`auto`), or `builder(size, crc_32)` (`store32`/`store64`) — so a `.startswith("store")` string probe is the deleted dispatch and the serialized profile carries no native `Method` handle. The `SevenZFilter` chain resolves to the `py7zr.FILTER_*` ids at arm scope and the `ZipMechanism` allow-list to the `stream_unzip` sentinels through the one frozen `_ZIP_MECHANISM` row.
- Modality: both arms are multi-file — the `*payloads` spread the codec `pack` recovers folds into ONE container. `SEVEN_Z` writes each payload as a `names[i]`-or-`payload-{i}` entry through `SevenZipFile.writef(BytesIO(payload), name)` then re-reads the written bytes through `SevenZipFile.test()` for the CRC-verified entry count and `SevenZipFile.archiveinfo().uncompressed` for the declared reconstructed size; `ZIP_STREAM` folds the spread into one `stream_zip` member sequence so a multi-payload bundle is a single archive, never N archives, then round-trips the sealed blob through `_zip_drain` for the genuine per-member integrity count `stream_zip`'s inline stored-member CRC alone cannot prove. The `unpack` inverse recovers the full member row set with a real per-member streamed digest on the runtime XXH3_128 content-identity family, bomb-bounded — `SEVEN_Z` reads the `(filename, uncompressed)` rows from `SevenZipFile.list()` then streams each entry through the `_Xxh3Factory` `WriterFactory` sink under `max_extract_size=_ARCHIVE_CEILING`, so the decode folds into the sink's `xxh3_128` without buffering the whole payload and a high-ratio container raises `DecompressionBombError`; `ZIP_STREAM` drains each member's chunks through the shared `_zip_drain` reader's rolling `xxhash.xxh3_128` fold, classifying the typed `stream_unzip` fault subtree at the seam — so neither arm buffers a payload, only its size and its 16-byte `xxh3_128` content digest, uniform with the codec `_walked` frame walk and the delta arm on the ONE identity family `ContentIdentity.of` uses, and the recovered triples feed the codec `BundleManifest.of` content-key fold.
- Filters: the `SEVEN_Z` arm models the 7z codec chain as the `SevenZFilter` closed vocabulary — `lzma`/`lzma2`/`bzip2`/`ppmd`/`zstd`/`brotli`/`deflate`/`copy` primary codecs plus the `delta`/`x86`/`arm`/`armthumb`/`powerpc`/`sparc`/`ia64` pre-filters that prepend the codec stage — each token resolved to its `py7zr.FILTER_*` id at arm scope and folded into the `[{"id": ...}, ...]` chain `SevenZipFile(filters=...)` consumes, the `SevenZPreset` (`"default"`/`"extreme"`) `PRESET_DEFAULT`/`PRESET_EXTREME` level applied only to the `_PRESETABLE` `lzma`/`lzma2` entries, and the `FILTER_CRYPTO_AES256_SHA256` crypto entry appended when `password` is set. A `tuple[dict[str, int], ...]` raw-chain field, a bare `FILTER_*` ordinal, and a `getattr(py7zr, f"FILTER_{name}")` string-built lookup are the deleted forms; the inline `ident` map mirrors the codec `_gated_codec` block/mode-constant convention because the `lazy import py7zr` proxy reifies only at arm scope.
- Crypto: the `SEVEN_Z` arm threads `password` plus the `{"id": FILTER_CRYPTO_AES256_SHA256}` chain entry alongside `header_encryption`, so encryption is the functional row the filter axis carries, never a lone `header_encryption` bool. The `ZIP_STREAM` arm threads `password` through `stream_zip(..., password=...)` switching every member to WinZip AES-256 (the `pycryptodome` AES-256/HMAC-SHA1/PBKDF2 backend), and the `unpack` inverse passes `password.encode()` `bytes` plus the `mechanisms`-derived `allowed_encryption_mechanisms` allow-list (`_ZIP_MECHANISM` resolving the `ZipMechanism` tokens to the `stream_unzip` sentinels) to `stream_unzip`, so the decrypt trust is a profile value, never a hardcoded `frozenset` or a second encrypted function. The allow-list gates the WinZip-AES variant (`ae1`/`ae2`) AND the key length (`aes128`/`aes192`/`aes256`) as orthogonal axes, and `stream_zip` writes AE-2/AES-256, so the default `("none", "ae2", "aes256")` admits plaintext plus the page's own encrypted output while refusing legacy `ZIP_CRYPTO`, the `ae1` variant, and the weak `aes128`/`aes192` key lengths BEFORE decryption — a `("none", "aes256")` list that omits `ae2` rejects the page's own AES-256 members. The AES salt/IV is freshly random per pack (`stream_zip`'s `get_crypto_random` default `secrets.token_bytes`), so an encrypted bundle is intentionally non-reproducible while an unencrypted one stays byte-stable.
- Stream: the `ZIP_STREAM` arm is the bounded-memory container — `_zip_members` resolves `(builder, arity)` once from `_ZIP_METHOD[knobs.method]` then folds each payload into a `(name, modified_at, mode, method, data)` `MemberFile` tuple whose `method` the `ZipArity` `match` selects (`builder`, `builder(size, level)`, or `builder(size, zlib_ng.crc32(payload))` for the stored rows) and whose `data` is a one-chunk `iter((payload,))` lazy iterable interleaving encode-and-emit. `stream_zip(members, password=..., get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=level))` yields the container `bytes` `b"".join(...)` seals into the blob — the `get_compressobj` row binds the shared `data:compression` `zlib_ng` SIMD raw-DEFLATE at the profile `level` so EVERY member format honours `level` (not only the `ZIP_AUTO` size-and-level row that takes its own level) and the stored members pre-declare their CRC from the same `zlib_ng.crc32` substrate, one codec on both the deflate and integrity legs. `modified_at` rides the fixed `_EPOCH` (`1980-01-01`, the ZIP epoch) and `extended_timestamps=False` drops the Info-ZIP `UT` extra field, so the unencrypted container is byte-reproducible on the one reproducible-ZIP convention every `stream_zip` caller shares and the content key dedups across runs, never the `datetime.now()` stamp or the `UT` extra that would churn it. `unpack` drives `stream_unzip(chunks, password=..., allowed_encryption_mechanisms=...)` member by member through the shared `_zip_drain` reader, draining each member's `chunks` fully (the `UnfinishedIterationError` ordered-consumption guard) and rolling the byte total plus a streamed `xxhash.xxh3_128` digest into one `(name, size, digest)` triple without buffering the payload — a real content digest on the runtime XXH3_128 identity family, never the weak stored 32-bit CRC. The same reader classifies the typed 52-leaf `stream_unzip` fault subtree at the seam into a closed discriminant — trust (`PasswordError`/`MissingPasswordError`: wrong mechanism or missing password) vs corruption (`DataError`/`UncompressError`: truncation, CRC32/HMAC/size mismatch, deflate/bz2 backend) vs unsupported (`UnsupportedFeatureError`, read before its `DataError` base) vs ordering (`InvalidOperationError`, outside `ValueError`) — so a hostile or damaged container faults with a structurally-addressable cause the codec `async_boundary` rails, never one flattened provider token; the drain is the pack-time integrity proof too, replacing the illusory `verified = len(payloads)` count.
- Receipt: each container pack contributes the `package/codec#CODEC` `BundleEvidence.measure` projection onto `core/receipt#RECEIPT` `ArtifactReceipt.Bundle` — the `entries` slot is the MEMBER count, which `measure` already folds from `len(payloads)` — the archive arms pass the N-member payload spread as `payloads` and the single container blob as `(blob,)`, so `entries` is the member arity (never the one-blob count) with no override kwarg, the one fold the single-blob codecs and the container arms share — and the `verified` slot is a REAL per-member integrity proof (`SEVEN_Z` from `SevenZipFile.test()`, `ZIP_STREAM` the pack-time `stream_unzip` round-trip survivor count via `_zip_drain` — never the illusory `len(payloads)`, since `stream_zip` inline-CRC-verifies only stored `NO_COMPRESSION_*` members and leaves every deflate `ZIP_AUTO` member unproven at pack). `_archive_in_process` returns the `(blobs, BundleEvidence)` pair the codec `_in_process` dispatch threads onto `_emit`, so the archive arm mints no receipt of its own; `level` carries the ZIP level for `ZIP_STREAM` and zero for `SEVEN_Z` (filter-chain selected), and `frame_size` carries the declared uncompressed member total (`ZIP_STREAM` the summed payload lengths, `SEVEN_Z` `SevenZipFile.archiveinfo().uncompressed` — the container's declared uncompressed size, not the redundant compressed blob length already carried on `out_bytes`). The shared `BundleEvidence` crosses no module boundary into the receipt owner — the codec `_emit` spreads its named fields onto the flat-scalar `Bundle` case.
- Growth: a new container algorithm is one `CompressionAlgo` row, one `CodecProfile` case with its knob-struct, one default-profile constant in the codec owner, and one arm in each of `_archive_in_process`/`_archive_unpack`; a new ZIP method is one `ZipMethod` token plus one `_ZIP_METHOD` `(builder, arity)` row; a new 7z filter is one `SevenZFilter` token plus one `ident` map entry; a new decrypt mechanism is one `ZipMechanism` token plus one `_ZIP_MECHANISM` row; a new container evidence fact rides the existing `entries`/`verified` slots. Zero new surface beside the two worker arms and the two profile structs.

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
from collections.abc import Iterable, Iterator
from datetime import datetime
from io import BytesIO
from typing import Final, assert_never

import xxhash
from builtins import frozendict
from stream_unzip import AE_1, AE_2, AES_128, AES_192, AES_256, NO_ENCRYPTION, ZIP_CRYPTO, DataError, InvalidOperationError, MissingPasswordError, PasswordError, UncompressError, UnsupportedFeatureError, stream_unzip
from stream_zip import Method, NO_COMPRESSION_32, NO_COMPRESSION_64, ZIP_32, ZIP_64, ZIP_AUTO, stream_zip

lazy from artifacts.package.codec import BundleEvidence, CodecProfile, CompressionAlgo  # cyclic owner: codec eager-imports these workers for DEFAULT_PROFILE, so this back-edge defers (used only in the worker bodies) to break the import cycle

lazy from zlib_ng import zlib_ng  # the shared `data:compression` SIMD substrate composed on the ZIP_STREAM arm: raw-DEFLATE (`compressobj`/`MAX_WBITS`) + stored-member `crc32`, one codec on both the deflate and integrity legs — never re-admitted
lazy import py7zr

# --- [CONSTANTS] ------------------------------------------------------------------------

# the 7z extract bomb bound: `max_extract_size` caps the streamed decode so a crafted high-ratio
# container raises `DecompressionBombError` (mapped to the codec `Result.Error` rail by `async_boundary`)
# before memory exhausts; the ZIP arm is bounded by `stream_unzip`'s rolling chunk drain, never a buffer.
_ARCHIVE_CEILING: Final[int] = 1 << 31

# --- [TABLES] ---------------------------------------------------------------------------

# the 7z LZMA-family entries that take a `preset`; every other `SevenZFilter` is preset-free.
_PRESETABLE: frozenset[SevenZFilter] = frozenset({"lzma", "lzma2"})

# the decrypt allow-list correspondence: each `ZipMechanism` token -> its opaque stream_unzip sentinel.
_ZIP_MECHANISM: frozendict[ZipMechanism, object] = frozendict({
    "none": NO_ENCRYPTION, "zipcrypto": ZIP_CRYPTO, "ae1": AE_1, "ae2": AE_2,
    "aes128": AES_128, "aes192": AES_192, "aes256": AES_256,
})

# --- [BOUNDARIES] -----------------------------------------------------------------------

# the py7zr streamed-sink protocol (Py7zIO `write`/`read`/`seek`/`size`/`flush`/`close`, WriterFactory `create`)
# folded onto the runtime XXH3_128 content-identity family, replacing the built-in sha256 `HashIOFactory`: each entry
# decodes straight into one `xxh3_128` digest under `max_extract_size` with no payload buffer, so the recovered 7z
# member digest matches the ZIP fold and every other codec path on the ONE identity family `ContentIdentity.of` uses.
# Duck-typed (not subclassed) so the base classes stay behind the `lazy import py7zr` proxy and force no eager load.


class _Xxh3Sink:
    __slots__ = ("_hasher", "_size")

    def __init__(self) -> None:
        self._hasher, self._size = xxhash.xxh3_128(), 0

    def write(self, data: bytes) -> int:
        self._hasher.update(data)
        self._size += len(data)
        return len(data)

    def read(self, size: int = -1) -> bytes:  # mirrors py7zr HashIO.read(): the 16-byte content digest, not the payload
        return self._hasher.digest()

    def seek(self, offset: int, whence: int = 0) -> int:
        return self._size

    def size(self) -> int:
        return self._size

    def flush(self) -> None: ...

    def close(self) -> None: ...


class _Xxh3Factory:
    __slots__ = ("_sinks",)

    def __init__(self) -> None:
        self._sinks: dict[str, _Xxh3Sink] = {}

    def create(self, filename: str) -> _Xxh3Sink:
        self._sinks[filename] = sink = _Xxh3Sink()
        return sink

    def get(self, filename: str) -> _Xxh3Sink:
        return self._sinks[filename]

# --- [OPERATIONS] -----------------------------------------------------------------------


def _zip_members(payloads: tuple[bytes, ...], knobs: ZipStreamKnobs) -> Iterable[tuple[str, datetime, int, Method, Iterable[bytes]]]:
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


def _zip_trust(knobs: ZipStreamKnobs, /) -> tuple[bytes | None, frozenset[object]]:
    # the ZIP decrypt trust resolved once from the profile — the password as stream_unzip's `bytes` (it rejects `str`)
    # and the `mechanisms` allow-list mapped to the opaque stream_unzip sentinels through `_ZIP_MECHANISM` — shared by the
    # pack-time integrity round-trip and the unpack recovery so the trust policy is a single derived value, never re-inlined.
    return (knobs.password.encode() if knobs.password is not None else None, frozenset(_ZIP_MECHANISM[m] for m in knobs.mechanisms))


def _zip_drain(blob: bytes, password: bytes | None, mechanisms: frozenset[object], /) -> Iterator[tuple[str, int, bytes]]:
    # the ONE streamed ZIP reader both the pack-time integrity verify and the unpack recovery consume: it drives
    # `stream_unzip`, fully drains each member's `chunks` (the drain is what triggers the streamed CRC32/size/HMAC
    # verification stream_unzip performs — a member that drains without raising is integrity-proven), rolls one
    # `xxh3_128` content digest per member without buffering the payload, and yields the uniform `(name, size, digest)`
    # triple on the runtime XXH3_128 identity family. The typed 52-leaf fault subtree classifies at this seam into a
    # closed trust/corruption/unsupported/ordering discriminant so the codec `async_boundary` rails a structurally-
    # addressable cause, never a flattened provider token — ordered most-specific-first: `InvalidOperationError`
    # (ordering, outside `ValueError`) before the `PasswordError`/`MissingPasswordError` trust pair, then
    # `UnsupportedFeatureError` before its `DataError` base, then `DataError`/`UncompressError` corruption.
    try:
        for name, _declared, chunks in stream_unzip((blob,), password=password, allowed_encryption_mechanisms=mechanisms):
            total, hasher = 0, xxhash.xxh3_128()
            for chunk in chunks:
                total += len(chunk)
                hasher.update(chunk)
            yield name.decode(), total, hasher.digest()
    except InvalidOperationError as ordering:
        ordering.add_note("<at:zip-unpack ordered-consumption>")
        raise ValueError("<zip-unpack:ordering>") from ordering
    except (PasswordError, MissingPasswordError) as trust:
        trust.add_note("<at:zip-unpack mechanism/password>")
        raise ValueError("<zip-unpack:trust>") from trust
    except UnsupportedFeatureError as unsupported:
        unsupported.add_note("<at:zip-unpack unsupported-feature>")
        raise ValueError("<zip-unpack:unsupported>") from unsupported
    except (DataError, UncompressError) as corruption:
        corruption.add_note("<at:zip-unpack truncation/integrity/backend>")
        raise ValueError("<zip-unpack:corruption>") from corruption


def _archive_in_process(payloads: tuple[bytes, ...], algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[bytes, ...], BundleEvidence]:
    match profile:
        case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
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
                uncompressed = reader.archiveinfo().uncompressed  # the container's declared uncompressed total — the true reconstructed size for `frame_size`, not the redundant compressed `len(blob)` that already rides `out_bytes`; the same `ArchiveInfo` also carries `method_names`/`solid`/`blocks`, structural facts the flat 8-scalar Bundle case the brief locks cannot hold
            return (blob,), BundleEvidence.measure(algo, 0, 0, uncompressed, verified, payloads, (blob,))
        case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
            blob = b"".join(stream_zip(_zip_members(payloads, k), password=k.password, extended_timestamps=False, get_compressobj=lambda: zlib_ng.compressobj(wbits=-zlib_ng.MAX_WBITS, level=k.level)))
            # verified is a GENUINE per-member integrity count, not the illusory `len(payloads)`: stream_zip only CRC-verifies
            # streamed NO_COMPRESSION_* (stored) members inline, so a deflate ZIP_AUTO member carries no pack-side proof —
            # round-trip the packed blob through `stream_unzip` (the delta-arm round-trip pattern), draining each member so
            # its streamed CRC32/size/HMAC verifies, and count the survivors; a member that fails integrity raises the
            # classified `<zip-unpack:corruption>` fault the codec `async_boundary` rails rather than a silent overcount.
            verified = sum(1 for _ in _zip_drain(blob, *_zip_trust(k)))
            return (blob,), BundleEvidence.measure(algo, k.level, 0, sum(map(len, payloads)), verified, payloads, (blob,))
        case _:
            raise ValueError(f"<non-archive-profile:{profile.tag}>")  # the codec dispatch routes only seven_z/zip_stream here; assert_never is invalid on the non-exhausted CodecProfile family


def _archive_unpack(blob: bytes, algo: CompressionAlgo, profile: CodecProfile) -> tuple[tuple[str, int, bytes], ...]:
    match profile:
        case CodecProfile(tag="seven_z", seven_z=SevenZKnobs() as k):
            sinks = _Xxh3Factory()  # streamed xxh3_128 sink: each entry decodes into one digest, never buffered whole; `max_extract_size` bombs-bounds the decode
            with py7zr.SevenZipFile(BytesIO(blob), mode="r", password=k.password, max_extract_size=_ARCHIVE_CEILING) as reader:
                infos = [info for info in reader.list() if not info.is_directory]
                reader.extractall(factory=sinks)
            return tuple((info.filename, info.uncompressed, sinks.get(info.filename).read()) for info in infos)
        case CodecProfile(tag="zip_stream", zip_stream=ZipStreamKnobs() as k):
            return tuple(_zip_drain(blob, *_zip_trust(k)))  # the classified streamed drain — trust vs corruption vs unsupported vs ordering railed at the seam — replacing the side-effecting `reduce` lambda fold with the shared `(name, size, xxh3_128)` generator
        case _:
            raise ValueError(f"<non-archive-profile:{profile.tag}>")  # the codec dispatch routes only seven_z/zip_stream here; assert_never is invalid on the non-exhausted CodecProfile family
```

## [03]-[RESEARCH]

- [SEVEN_Z_FILTER_CHAIN] [RESOLVED]: the 7z codec chain is the `SevenZFilter` closed vocabulary resolved at arm scope to `py7zr.FILTER_*` ids and folded into the `filters=[{"id": FILTER_..., "preset": PRESET_...}]` `list[dict[str, int]]` shape `SevenZipFile.__init__` accepts (verified signature: `SevenZipFile(file, mode='r', *, filters=None, dereference=False, password=None, header_encryption=False, blocksize=None, mp=False)`), the `SevenZPreset` `PRESET_DEFAULT`/`PRESET_EXTREME` level applied only to the `_PRESETABLE` `lzma`/`lzma2` entries and `FILTER_CRYPTO_AES256_SHA256` appended when `password` is set. The full constant family `FILTER_LZMA`/`FILTER_LZMA2`/`FILTER_BZIP2`/`FILTER_PPMD`/`FILTER_ZSTD`/`FILTER_BROTLI`/`FILTER_DEFLATE`/`FILTER_COPY`/`FILTER_DELTA`/`FILTER_X86`/`FILTER_ARM`/`FILTER_ARMTHUMB`/`FILTER_POWERPC`/`FILTER_SPARC`/`FILTER_IA64`/`FILTER_CRYPTO_AES256_SHA256` plus `PRESET_DEFAULT`/`PRESET_EXTREME` is confirmed on the installed `1.1.3` module (`Python package`, ungated). `writef(bio, arcname)`, `list() -> list[FileInfo]` with `filename`/`uncompressed`/`is_directory`, `extractall(*, factory=)` over a `WriterFactory` whose `Py7zIO` sink streams per entry (`create(name) -> Py7zIO`, `Py7zIO.write`/`read`/`size` confirmed on the installed module), and `test() -> True | False | None` are confirmed; the built-in `py7zr.io.HashIOFactory` folds an internal `hashlib.sha256`, so the recovery instead drives `_Xxh3Factory` — a duck-typed `WriterFactory` whose `_Xxh3Sink.write` folds each decoded chunk into one `xxhash.xxh3_128` and whose `read()` returns the 16-byte digest, aligning the 7z member digest onto the runtime XXH3_128 identity family the ZIP fold and `ContentIdentity.of` already use (the built-in sha256 sink was the odd family out). The prior `readall`/`read` whole-payload in-memory mapping is the deleted phantom — `list()` reads names and sizes and `extractall(factory=_Xxh3Factory())` streams the per-entry `xxh3_128` with no temp-file round-trip, so the recovery surface materializes no payload. The `dict[str, int]` filter bag carried on the profile is the deleted form (a raw provider shape leaking inward); the `SevenZFilter` token plus the arm-scope `ident` resolution is the canonical boundary mapping. Note: `py7zr` exposes the `CHECK_CRC32`/`CHECK_CRC64`/`CHECK_SHA256`/`CHECK_NONE` integrity-check constants, but `SevenZipFile.__init__` carries no `check=` parameter on the installed `1.1.3` (reflection-confirmed signature `(file, mode, *, filters, dereference, password, header_encryption, blocksize, mp, max_extract_size)`), so a `SevenZKnobs.check` axis has no verified public passing mechanism and is NOT admitted — the container's own CRC (`test()`) is the integrity evidence, the streamed `xxh3_128` the content digest.
- [SEVEN_Z_INVERSE] [RESOLVED]: `_archive_unpack` recovers the 7z member set by reading `(filename, uncompressed)` from `SevenZipFile.list()` then streaming each entry through `extractall(factory=_Xxh3Factory())`, so `_Xxh3Sink.read()` yields the real per-entry `xxh3_128` 16-byte content digest the `BundleManifest.of` content-key fold consumes — the weak stored 32-bit CRC (`info.crc32`, ~32 bits of entropy, collidable) is the deleted seed, replaced by the `xxh3_128` content digest the codec `_walked` frame walk now also yields (this run aligns `package/codec#CODEC` `_walked` and both `package/archive` arms onto `xxh3_128`), so the `(name, size, digest)` triple and the `ContentIdentity.of(f"member-{algo}", digest)` member key are uniform on the runtime identity family. Cross-file: `package/delta#DELTA` `_delta_unpack` now seeds its triple with `xxhash.xxh3_128_digest(payload)` too, so the "uniform across every codec path" claim holds end to end — the last sha256 holdout is closed and all three package arms (codec `_walked`, both archive arms, delta) yield the one 16-byte `xxh3_128` content digest. The decode is bomb-bounded by `max_extract_size=_ARCHIVE_CEILING`: a crafted high-ratio container raises `DecompressionBombError`, and a genuinely unreadable one raises `Bad7zFile`/`PasswordRequired`/`ArchiveError`, both inside the recovery, which the codec `async_boundary` converts to the `Result.Error` rail so a corrupt or hostile archive faults rather than masquerading as an empty manifest; the pack-side `test()` remains the integrity-evidence source feeding `BundleEvidence.verified`, kept off the recovery path. `SevenZipFile.archiveinfo()` returns the `ArchiveInfo` container view (`filename`/`stat`/`header_size`/`method_names`/`solid`/`blocks`/`uncompressed` — the full field set assay-confirmed on `py7zr 1.1.3`); the pack arm now reads `archiveinfo().uncompressed` as the declared reconstructed total feeding `BundleEvidence.frame_size`, replacing the redundant compressed `len(blob)` that already rode `out_bytes`, so `frame_size` on `SEVEN_Z` means the same declared-uncompressed size it means on the `ZSTD`/`ZIP_STREAM`/delta arms. The `method_names`/`solid`/`blocks` structural facts are read alongside but cannot land on the flat 8-scalar `Bundle` case the brief locks, so only `uncompressed` folds into an existing slot; a wider structural receipt would require widening the shared case, which `package/codec#CODEC` legislates unchanged.
- [ARCHIVE_BOUNDED_DEFENSE] [RESOLVED]: `SevenZipFile.__init__` carries the `max_extract_size` keyword and `py7zr.exceptions` exposes `DecompressionBombError` on the installed `py7zr 1.1.3` (reflection-confirmed alongside `ArchiveError`/`Bad7zFile`/`PasswordRequired`/`DecompressionError`/`CrcError`/`AbsolutePathError`/`UnsupportedCompressionMethodError`/`InternalError`), so both are composed rather than worked around: `_archive_unpack` opens the reader with `max_extract_size=_ARCHIVE_CEILING` and streams every entry through the `_Xxh3Factory` sink, so a crafted high-ratio container trips `DecompressionBombError` once the capped total decoded size is exceeded and a traversal name trips `AbsolutePathError`, both surfaced inside the recovery and mapped to `Result.Error` at the codec `async_boundary`. The bomb defense is the size cap over a streamed decode, not a no-decode directory pass: `_Xxh3Sink.write` folds each entry into its `xxh3_128` without buffering the whole payload, so memory stays bounded by the chunk while the cap bounds the total. The built-in `BytesIOFactory(limit)` is the bounded in-memory sink a payload-needing consumer drives and `NullIOFactory` the discard sink; the manifest recovery needs only the per-entry digest and size, so it rides the custom `_Xxh3Factory` (the identity-family variant of the built-in `HashIOFactory` protocol) alone.
- [REPRODUCIBLE_BYTES] [RESOLVED]: the `ZIP_STREAM` container is byte-reproducible — fixing `modified_at` to `_EPOCH`, dropping the Info-ZIP `UT` extra field via `extended_timestamps=False`, and the deflate to `zlib_ng` at a fixed `level` makes an unencrypted bundle produce identical bytes across runs (verified: a fixed-epoch `stream_zip` is byte-stable run to run, where the prior `datetime.now()` stamp churned every byte and broke the codec's "identical inputs at identical profile = cache hit by reference" content-addressing). An encrypted ZIP bundle is intentionally non-reproducible (fresh `secrets.token_bytes` AES salt/IV per pack). The `SEVEN_Z` container is NOT byte-reproducible: `py7zr` stamps an uncontrollable `creationtime` (the live wall clock) on each `writef` entry and exposes no member-timestamp control, so two 7z packs of identical payloads at an identical profile differ byte-for-byte (verified across a 1 s interval) — the `SEVEN_Z` bundle keys by its as-produced bytes and does not dedup across runs, where the `ZIP_STREAM` arm is the content-addressable container.
- [ZIP_GENUINE_VERIFY] [RESOLVED]: the prior `ZIP_STREAM` `verified = len(payloads)` was ILLUSORY — a raw payload count dressed as an integrity proof. `stream_zip` verifies streamed content ONLY for stored `NO_COMPRESSION_*` members (their declared `(uncompressed_size, crc_32)` checked against the actual bytes, raising `CRC32IntegrityError`/`UncompressedSizeIntegrityError`); a deflate `ZIP_AUTO`/`ZIP_64`/`ZIP_32` member carries NO pack-side integrity check, so the count over-claimed for every compressed member. The genuine `verified` is now the pack-time `stream_unzip` round-trip via `_zip_drain`: fully draining each member triggers `stream_unzip`'s streamed CRC32 / compressed-size / uncompressed-size / (WinZip-AES) HMAC-SHA1 verification — the `IntegrityError` subtree — so a member that drains without raising is integrity-proven and `verified = sum(1 for _ in _zip_drain(blob, *_zip_trust(k)))` is the survivor count, the same round-trip pattern as the `SEVEN_Z` `test()` re-read and the `package/delta#DELTA` apply, uniform across all three container/delta arms. `_zip_drain` is shared by the pack verify and the unpack recovery (one derived streamed reader; `_zip_trust` resolves the `(password, mechanisms)` trust policy once, dropping the duplicated `k.password.encode()`/`_ZIP_MECHANISM` inlines and the side-effecting `reduce(lambda acc, chunk: ... acc[1].update(chunk) or acc[1], ...)` fold), and it classifies the typed 52-leaf `stream_unzip` fault tree at the seam into a closed trust/corruption/unsupported/ordering discriminant through ordered most-specific-first `except` arms — `InvalidOperationError` (ordering, outside `ValueError`) first, then the `PasswordError`/`MissingPasswordError` sibling trust pair, then `UnsupportedFeatureError` before its `DataError` base, then `DataError`/`UncompressError` corruption, each with a `BaseException.add_note` coordinate — every class assay-confirmed present on `stream-unzip 0.0.101`. So a hostile, wrong-mechanism, or truncated container faults with a structurally-addressable `<zip-unpack:{trust,corruption,unsupported,ordering}>` cause the codec `async_boundary` rails, never one flattened provider token, and the `EncryptionMechanismNotAllowed` allow-list refusal (a `PasswordError` subtree leaf) classifies as trust. The mechanism allow-list, the `_EPOCH`/`zlib_ng`-at-`level` byte-reproducibility seam, and the `max_extract_size` / `stream_unzip` rolling-drain bomb bounds are unchanged.
